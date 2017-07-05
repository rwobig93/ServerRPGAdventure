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
        public static GitHubClient gitClient = null;
        public static SaveData saveData = new SaveData();
        public static string configFile = $@"{Directory.GetCurrentDirectory()}\Config\UpdaterConfig.json";
        public static string logPath = $@"{Directory.GetCurrentDirectory()}\Logs";

        public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            Initialize();
            SetupClient();
            await CheckNewRelease();
        }

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

        private void VerifySaveData()
        {
            try
            {
                if (!Directory.Exists($@"{Directory.GetCurrentDirectory()}\Config"))
                {
                    Directory.CreateDirectory($@"{Directory.GetCurrentDirectory()}\Config");
                    Console.WriteLine($@"Config Folder Missing, created: {Directory.GetCurrentDirectory()}\Config");
                }
                if (!File.Exists(configFile))
                {
                    saveData.CurrentVersion = new Version("0.1.00.00");
                    saveData.BackupFolder = $@"{Directory.GetCurrentDirectory()}\Backup";
                    saveData.InstallDirectory = $@"{Directory.GetCurrentDirectory()}";
                    saveData.DownloadBaseURL = $@"https://github.com/rwobig93/ServerRPGAdventure/releases/download/";
                    SerializeConfig();
                    Console.WriteLine($@"Config file wasn't found, created default");
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
                Console.WriteLine($"Finished Serializing config data to: {configFile}");
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
                    Console.WriteLine($"Finished Deserializing config data from: {configFile}");
                }
                else
                    Console.WriteLine($"Didn't locate a config file at {configFile}, stopped deserialization");
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
            Console.WriteLine($@"EXCEPTION: [Caller]{caller} at {line} | [Type]{ex.GetType().Name} | [Msg]{ex.Message}");
        }

        private void SetupClient()
        {
            try
            {
                gitClient = new GitHubClient(new ProductHeaderValue("PDB_Updater"));
                Console.WriteLine("Successfully setup gitclient, press any key to continue...");
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
            WebClient webClient = new WebClient();
            webClient.DownloadProgressChanged += (sender2, e2) => { Console.WriteLine($"Download Progress: {e2.ProgressPercentage}% ({e2.BytesReceived}/{e2.TotalBytesToReceive})"); };
            webClient.DownloadFileCompleted += (sender2, e2) => { Console.WriteLine("Download complete, now starting updated version"); BackupPreviousVersion(); };
            Console.WriteLine($"Starting download for v{releaseVerNum}...");
            webClient.DownloadFileAsync(new Uri(releaseURI), exeName);
        }

        private void KillRunningProcesses()
        {
            try
            {
                foreach (var proc in Process.GetProcessesByName("PersonalDiscordBot.exe"))
                {
                    proc.Kill();
                    Console.WriteLine($"Found running process for PDB and killed it. [ID]{proc.Id} [Name]{proc.ProcessName}");
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
                    Console.WriteLine($"Process {startPDB.ProcessName} didn't start successfully, couldn't find process ID {startPDB.Id}, would you like to try again? (y/n)");
                    string response = Console.ReadLine();
                    if (response.ToLower() == "y")
                    {
                        await CheckNewRelease();
                    }
                    else
                    {
                        Console.WriteLine("Cancelling update, press any key to close...");
                        Console.ReadLine();
                        Exit();
                    }
                }
                else
                {
                    Console.WriteLine($"Started PDB from {startPDB.StartInfo.FileName}");
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
                    Directory.CreateDirectory(saveData.BackupFolder);
                if (File.Exists($@"{saveData.BackupFolder}\PersonalDiscordBot.exe"))
                    File.Delete($@"{saveData.BackupFolder}\PersonalDiscordBot.exe");
                if (File.Exists($@"{saveData.InstallDirectory}\PersonalDiscordBot.exe"))
                {
                    File.Move($@"{saveData.InstallDirectory}\PersonalDiscordBot.exe", $@"{saveData.BackupFolder}\PersonalDiscordBot.exe");
                    Console.WriteLine($"Backed up current executable to {saveData.BackupFolder}");
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
            this.Exit();
        }

        private async Task Cleanup()
        {
            Console.WriteLine("Starting bot...");
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
                    Console.WriteLine($"Newer release found, updating now... [Current]{saveData.CurrentVersion} [Release]{releaseVersion}");
                    UpdateToNewVersion(releaseVersion);
                    await Cleanup();
                }
                else
                {
                    Console.WriteLine($"Release Version is the same version or older than running assembly. [Current]{saveData.CurrentVersion} [Release]{releaseVersion}");
                    Exit();
                }
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }
    }

    class SaveData
    {
        public Version CurrentVersion { get; set; }
        public string InstallDirectory { get; set; }
        public string BackupFolder { get; set; }
        public string DownloadBaseURL { get; set; }
    }
}
