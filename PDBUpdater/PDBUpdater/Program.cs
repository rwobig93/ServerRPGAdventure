using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Reflection;
using Octokit;
using System.Diagnostics;
using System.Net;
using System.ComponentModel;

namespace PDBUpdater
{
    class Program
    {
        #region Global Variables

        public static GitHubClient gitClient = null;
        public static SaveData saveData = new SaveData();
        public static string configFile = $@"{Directory.GetCurrentDirectory()}\Config\UpdaterConfig.json";
        public static string logPath = $@"{Directory.GetCurrentDirectory()}\Logs";
        public static bool downloadFinished = false;
        public static object _MessageLock = new object();

        #endregion

        public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        #region Async Methods

        public async Task MainAsync()
        {
            Initialize();
            SetupClient();
            await CheckNewRelease();
        }

        private async Task Cleanup()
        {
            uStatusWriteLine("Starting bot...", ConsoleColor.Green);
            await StartPDB();
            Exit();
        }

        private async Task CheckNewRelease()
        {
            try
            {
                var releases = await gitClient.Repository.Release.GetAll("rwobig93", "ServerRPGAdventure");
                var release = releases[0];
                Version releaseVersion = new Version(release.TagName);
                var result = saveData.CurrentVersion.CompareTo(releaseVersion);
                if (result < 0)
                {
                    uStatusWriteLine($"Newer release found, updating now... [Current]{saveData.CurrentVersion} [Release]{releaseVersion}");
                    UpdateToNewVersion(releaseVersion);
                    while (!downloadFinished)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(3));
                        uStatusWriteLine(".");
                    }
                    await Cleanup();
                }
                else
                {
                    uStatusWriteLine($"Release Version is the same version or older than running assembly. [Current]{saveData.CurrentVersion} [Release]{releaseVersion}");
                    Exit();
                }
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private async Task StartPDB()
        {
            try
            {
                Process startPDB = new Process() { StartInfo = new ProcessStartInfo { FileName = $@"{saveData.InstallDirectory}\PersonalDiscordBot.exe" } };
                startPDB.Start();
                Process started = Process.GetProcessById(startPDB.Id);
                if (started == null)
                {
                    uStatusWriteLine($"Process {startPDB.ProcessName} didn't start successfully, couldn't find process ID {startPDB.Id}, would you like to try again? (y/n)", ConsoleColor.Red);
                    string response = Console.ReadLine();
                    if (response.ToLower() == "y")
                    {
                        await CheckNewRelease();
                    }
                    else
                    {
                        uStatusWriteLine("Cancelling update, press enter to close...");
                        Console.ReadLine();
                        Exit();
                    }
                }
                else
                {
                    uStatusWriteLine($"Started PDB from {startPDB.StartInfo.FileName}", ConsoleColor.DarkGray);
                }
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        #endregion

        #region Form Handling

        private void Initialize()
        {
            try
            {
                if (!Directory.Exists(logPath))
                    Directory.CreateDirectory(logPath);
                VerifySaveData();
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        public void FullExceptionLog(Exception ex, [CallerLineNumber] int line = 0, [CallerMemberName] string caller = null, [CallerFilePath] string path = null)
        {
            string nl = Environment.NewLine;
            string fileName = $@"{logPath}\ExceptionLog_{DateTime.Now.ToLocalTime().ToString("MMddyyyy")}.log";
            string exceptionLog = $"Version: {Assembly.GetExecutingAssembly().GetName().Version}{nl}TimeStamp: {DateTime.Now.ToLocalTime().ToString("hh:mm:ss t")}{nl}Caller: {caller} at line {line}{nl}Type: {ex.GetType().Name}{nl}Message: {ex.Message}{nl}HR: {ex.HResult}{nl}Path: {path}{nl}StackTrace: {ex.StackTrace}{nl}";
            if (!File.Exists(fileName))
                using (StreamWriter sw = new StreamWriter(fileName))
                    sw.WriteLine(exceptionLog);
            uStatusWriteLine($@"EXCEPTION: [Caller]{caller} at {line} | [Type]{ex.GetType().Name} | [Msg]{ex.Message}", ConsoleColor.Red);
            uStatusWriteLine("An exception occured, please press enter to continue...");
            Console.ReadLine();
        }

        #endregion

        #region Methods

        private void VerifySaveData()
        {
            try
            {
                if (!Directory.Exists($@"{Directory.GetCurrentDirectory()}\Config"))
                {
                    Directory.CreateDirectory($@"{Directory.GetCurrentDirectory()}\Config");
                    uStatusWriteLine($@"Config Folder Missing, created: {Directory.GetCurrentDirectory()}\Config {Environment.NewLine}", ConsoleColor.DarkGray);
                }
                if (!File.Exists(configFile))
                {
                    saveData.CurrentVersion = new Version("0.1.00.00");
                    saveData.BackupFolder = $@"{Directory.GetCurrentDirectory()}\Backup";
                    saveData.InstallDirectory = $@"{Directory.GetCurrentDirectory()}";
                    saveData.DownloadBaseURL = $@"https://github.com/rwobig93/ServerRPGAdventure/releases/download/";
                    SerializeConfig();
                    uStatusWriteLine($@"Config file wasn't found, created default", ConsoleColor.DarkGray);
                }
                else
                    DeserializeConfig();
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void SerializeConfig()
        {
            try
            {
                var serializedData = JsonConvert.SerializeObject(saveData, Formatting.Indented);
                File.WriteAllText(configFile, serializedData);
                uStatusWriteLine($"Finished Serializing config data to: {configFile}", ConsoleColor.DarkGray);
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void DeserializeConfig()
        {
            try
            {
                if (File.Exists(configFile))
                {
                    using (StreamReader sr = File.OpenText(configFile))
                        saveData = JsonConvert.DeserializeObject<SaveData>(sr.ReadToEnd());
                    uStatusWriteLine($"Finished Deserializing config data from: {configFile}", ConsoleColor.DarkGray);
                }
                else
                    uStatusWriteLine($"Didn't locate a config file at {configFile}, stopped deserialization", ConsoleColor.DarkGray);
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void SetupClient()
        {
            try
            {
                gitClient = new GitHubClient(new ProductHeaderValue("PDB_Updater"));
                uStatusWriteLine("Successfully setup gitclient, press enter to continue...", ConsoleColor.Green);
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void UpdateToNewVersion(Version releaseVerNum)
        {
            KillRunningProcesses();
            saveData.CurrentVersion = releaseVerNum;
            string releaseURI = $@"{saveData.DownloadBaseURL}/{releaseVerNum}/";
            string exeName = "PersonalDiscordBot.exe";
            BackupPreviousVersion();
            WebClient webClient = new WebClient();
            webClient.DownloadProgressChanged += (sender2, e2) => { uStatusWriteLine($"Download Progress: {e2.ProgressPercentage}% ({e2.BytesReceived}/{e2.TotalBytesToReceive})", ConsoleColor.Yellow); };
            webClient.DownloadFileCompleted += (sender2, e2) => { uStatusWriteLine($"Download complete, now starting updated version {releaseVerNum}"); downloadFinished = true; saveData.CurrentVersion = releaseVerNum; };
            uStatusWriteLine($"Starting download for v{releaseVerNum}...");
            webClient.DownloadFileAsync(new Uri(releaseURI), exeName);
        }

        private void KillRunningProcesses()
        {
            try
            {
                var procs = Process.GetProcessesByName("PersonalDiscordBot.exe");
                if (procs.Length <= 0)
                    uStatusWriteLine($"{procs.Length} processes were found running", ConsoleColor.DarkGray);
                else
                    foreach (var proc in procs)
                    {
                        proc.Kill();
                        uStatusWriteLine($"Found running process for PDB and killed it. [ID]{proc.Id} [Name]{proc.ProcessName}", ConsoleColor.DarkGray);
                    }
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void BackupPreviousVersion()
        {
            try
            {
                if (!Directory.Exists(saveData.BackupFolder))
                {
                    Directory.CreateDirectory(saveData.BackupFolder);
                    uStatusWriteLine("Backup folder not found, created backup folder", ConsoleColor.DarkGray);
                }
                if (File.Exists($@"{saveData.BackupFolder}\PersonalDiscordBot.exe"))
                {
                    File.Delete($@"{saveData.BackupFolder}\PersonalDiscordBot.exe");
                    uStatusWriteLine("Removed previous backup executable", ConsoleColor.DarkGray);
                }
                if (File.Exists($@"{saveData.InstallDirectory}\PersonalDiscordBot.exe"))
                {
                    File.Move($@"{saveData.InstallDirectory}\PersonalDiscordBot.exe", $@"{saveData.BackupFolder}\PersonalDiscordBot.exe");
                    uStatusWriteLine($"Backed up current executable to {saveData.BackupFolder}{Environment.NewLine}", ConsoleColor.DarkGray);
                }
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void Exit()
        {
            SerializeConfig();
            uStatusWriteLine("Closing process...", ConsoleColor.DarkRed);
            var thisProc = Process.GetCurrentProcess();
            thisProc.Close();
        }

        private void uStatusWriteLine(string status, ConsoleColor color = ConsoleColor.White)
        {
            lock (_MessageLock)
            {
                if (color != ConsoleColor.White)
                    Console.ForegroundColor = color;
                else
                    Console.ResetColor();
                Console.WriteLine(status);
            }
        }

        #endregion
    }

    class SaveData
    {
        public Version CurrentVersion { get; set; }
        public string InstallDirectory { get; set; }
        public string BackupFolder { get; set; }
        public string DownloadBaseURL { get; set; }
    }
}
