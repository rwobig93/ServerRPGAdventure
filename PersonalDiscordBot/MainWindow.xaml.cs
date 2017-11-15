using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Discord;
using Discord.WebSocket;
using Discord.Net;
using Discord.Commands;
using System.Windows.Threading;
using System.Threading;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Media.Animation;
using PersonalDiscordBot.Classes;
using System.Reflection;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.Xml;
using System.Net;
using System.Runtime.CompilerServices;
using PersonalDiscordBot.Windows;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace PersonalDiscordBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Toolbox.uDebugAddLog(string.Format("{0}########################## Application Start ##########################{0}", Environment.NewLine));
            SetupConfig();
            txtLogDirectory.Text = Toolbox._paths.LogLocation;
            Events.MessagePromptShown += (e) => { uStatusUpdate(e.Content); };
            Events.MatchCompleted += async (e) => { await Management.EndOfMatchLootAsync(e); };
            Events.DiscordMessageSend += async (e, b) => { if (b) { await e.Context.Channel.SendMessageAsync(e.Context.Message.Author.Mention, false, e.Embed.Build()); } else { string resp = $"{e.Context.Message.Author.Mention} {e.Message}"; await e.Context.Channel.SendMessageAsync(resp); Toolbox.uDebugAddLog($"DCRDMSGSNT: {resp}"); } };
            Events.UseGlobalAction += (e) => { HandleGlobalAction(e.Action); };
        }

        #region Global Variables

        public static DiscordSocketClient client;
        private CommandService commands;
        private IServiceProvider services;
        public static Octokit.GitHubClient gitClient;
        private bool _activeSession = false;
        private bool notificationPlaying = false;
        public static ObservableCollection<GameServer> ServerList = new ObservableCollection<GameServer>();
        public ObservableCollection<GameServer> CurrentServerList
        {
            get { return ServerList; }
        }

        public enum ConfigType
        {
            Paths,
            Servers
        }
        public enum ServModifyType
        {
            NewServer,
            ExistingServer
        }

        public string line = Environment.NewLine;
        #endregion

        #region Form Handling

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void winMain_Loaded(object sender, RoutedEventArgs e)
        {
            Management.DeSerializeData();
            Permissions.DeSerializePermissions();
            VerifyDebug();
            Thread.Sleep(300);
            LoadWindowLocation();
            HideGrids();
            CheckVersion();
            tSaveRPGData();
            tRefreshAdminList();
            CleanupLogDir();
#if DEBUG
            uStatusUpdate("DEBUG mode running, skipping version update");
#else
            btnCheckForUpdates_Click(sender, e);
#endif
        }

        private void winMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Thread sendDisco = new Thread(async () =>
            {
                try
                {
                    if (Permissions.GeneralPermissions.logChannel != 0)
                    {
                        var channel = client.GetChannel(Permissions.GeneralPermissions.logChannel);
                        if (channel != null && client.CurrentUser.Username != null && ((IMessageChannel)channel).Name != null)
                        {
#if DEBUG
                            await ((IMessageChannel)channel).SendMessageAsync($"**{client.CurrentUser.Username}** has **disconnected** in **DEBUG** Mode biiiiiiiiiiiiiiiiiiiiiiatch!!!");
#else
                        await ((IMessageChannel)channel).SendMessageAsync($"**{client.CurrentUser.Username}** has **disconnected** biiiiiiiiiiiiiiiiiiiiiiatch!!!");
#endif
                            ShowNotification($"Bot {client.CurrentUser.Username} Sent disconnected message to log channel {((IMessageChannel)channel).Name}", 6);
                        }
                    }
                }
                catch (NullReferenceException) { Toolbox.uDebugAddLog($"Disconnect message not sent to logchannel due to being null, [ChannelID]{Permissions.GeneralPermissions.logChannel}"); return; }
                catch (Exception ex)
                {
                    Toolbox.FullExceptionLog(ex);
                }
            });
            sendDisco.Start();
            SaveWindowLocation();
            SaveAllData();
            Toolbox.uDebugAddLog(string.Format("{0}########################## Application Stop ##########################{0}", Environment.NewLine));
            Toolbox.DumpDebugLog();
            DumpStatusLog();
        }

        private void winMain_Closed(object sender, EventArgs e)
        {
            Process _currProc = Process.GetCurrentProcess();
            _currProc.Kill();
        }

        #endregion

        #region Event Handlers

        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(Toolbox._paths.BotToken))
                {
                    Toolbox.uDebugAddLog("Token wasn't found in LocalSettings, prompting for token and sending notification");
                    Thickness from = new Thickness(-734, 97, 0, 0);
                    Thickness to = new Thickness(0, 97, 0, 0);
                    SlideGrid(from, to, grdToken);
                    ShowNotification("A previous token wasn't found, please enter a bot token, save, then try again.", 4);
                    return;
                }
                await Start();
                string _senderText = sender.ToString();
                Thread _updateConn = new Thread(tUpdateConnectionStatus);
                _updateConn.Start();
                _activeSession = true;
                await SendConnectedMsg();
            }
            catch (Exception ex)
            {
                _activeSession = false;
                FullExceptionLog(ex);
            }
        }

        private async void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var botName = client.CurrentUser.Username;

                if (Permissions.GeneralPermissions.logChannel != 0)
                {
                    var channel = client.GetChannel(Permissions.GeneralPermissions.logChannel);
                    if (channel != null && client.CurrentUser.Username != null && ((IMessageChannel)channel).Name != null)
                    {
#if DEBUG
                        await ((IMessageChannel)channel).SendMessageAsync($"**{client.CurrentUser.Username}** has **disconnected** in **DEBUG** Mode biiiiiiiiiiiiiiiiiiiiiiatch!!!");
#else
                        await ((IMessageChannel)channel).SendMessageAsync($"**{client.CurrentUser.Username}** has **disconnected** biiiiiiiiiiiiiiiiiiiiiiatch!!!");
#endif
                        ShowNotification($"Bot {client.CurrentUser.Username} Sent disconnected message to log channel {((IMessageChannel)channel).Name}", 6);
                    }
                }
                await client.StopAsync();
                Toolbox.uDebugAddLog("Disconnected Client");
                await client.LogoutAsync();
                Toolbox.uDebugAddLog("Logged out");
                _activeSession = false;
                uStatusUpdate("Bot Client has disconnected and logged out");
                ShowNotification($"Bot {botName} has disconnected and logged out", 5);
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.WindowState = WindowState.Minimized;
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void btnOpenStatus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SwitchGrids(grdStatus);
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void btnSaveToken_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Toolbox._paths.BotToken = txtTokenValue.Text;
                SaveConfig(ConfigType.Paths);
                Toolbox.uDebugAddLog(string.Format("Saved new token: {0}", Toolbox._paths.BotToken));
                Thickness from = new Thickness(0, 97, 0, 0);
                Thickness to = new Thickness(-734, 97, 0, 0);
                SlideGrid(from, to, grdToken);
                Toolbox.uDebugAddLog("Slid grdToken back out of view");
                ShowNotification($"Successfully saved new token!", 3);
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void btnClearToken_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Toolbox._paths.BotToken = string.Empty;
                SaveConfig(ConfigType.Paths);
                txtTokenValue.Text = string.Empty;
                Toolbox.uDebugAddLog("Cleared token from LocalSettings and the txtTokenValue textbox");
                ShowNotification("Cleared token data", 3);
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void btnCancelToken_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Thickness from = new Thickness(0, 97, 0, 0);
                Thickness to = new Thickness(-734, 97, 0, 0);
                SlideGrid(from, to, grdToken);
                Toolbox.uDebugAddLog("Slid grdToken out of view");
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void btnChangeToken_Click(object sender, RoutedEventArgs e)
        {
            Thickness from = new Thickness(-734, 97, 0, 0);
            Thickness to = new Thickness(0, 97, 0, 0);
            SlideGrid(from, to, grdToken);
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SwitchGrids(grdSettings);
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void txtLogDirectory_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                Toolbox.uDebugAddLog(string.Format("Log Directory Location changed to: {0}", txtLogDirectory.Text));
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void btnOpenServers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SwitchGrids(grdServers);
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void btnSaveServers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveConfig(ConfigType.Servers);
                uStatusUpdate("Successfully saved current Game Server list");
            }
            catch (Exception ex)
            {
                uStatusUpdate("Current Game Server list wasn't saved successfully, application exception logged");
                FullExceptionLog(ex);
            }
        }

        private void btnRemoveServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GameServer selGame = (GameServer)lvServers.SelectedItem;
                ServerList.Remove(selGame);
                ServerList = new ObservableCollection<GameServer>(ServerList.OrderBy(x => x.Game));
                lvServers.ItemsSource = null;
                lvServers.ItemsSource = CurrentServerList;
                uStatusUpdate(string.Format("Removed game server from list: Game[{0}] Server Name[{1}], don't forget to save if you want this change to stick!", selGame.Game, selGame.ServerName));
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void btnAddServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReturnedServerEntry entry = Server_Editor.Open(ServModifyType.NewServer, null);
                if (entry != null && entry.Type == ServModifyType.NewServer)
                {
                    uStatusUpdate(string.Format("The game server {0} was successfully added, if you want this change to stick please save!", entry.Server.ServerName));
                    Toolbox.uDebugAddLog(string.Format("Successfully added server {0} || Type {1}", entry.Server.ServerName, entry.Type.ToString()));
                }
                else if (entry != null)
                {
                    uStatusUpdate(string.Format("Something unexpected happened, the return entry wasn't null but the return type wasn't ExistingServer. ServName: {0} || Type: {1}", entry.Server.ServerName, entry.Type.ToString()));
                    Toolbox.uDebugAddLog(string.Format("Unexpected Server Entry Returned: Serv: {0} || Type {1} || Type Expect {3}", entry.Server.ServerName, entry.Type.ToString(), ServModifyType.NewServer.ToString()));
                }
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void btnEditServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GameServer selServ = (GameServer)lvServers.SelectedItem;
                if (selServ != null)
                {
                    ReturnedServerEntry entry = Server_Editor.Open(ServModifyType.ExistingServer, selServ);
                    if (entry != null && entry.Type == ServModifyType.ExistingServer)
                    {
                        uStatusUpdate(string.Format("The game server {0} was successfully updated, if you want this change to stick please save!", entry.Server.ServerName));
                        Toolbox.uDebugAddLog(string.Format("Successfully added server {0} || Type {1}", entry.Server.ServerName, entry.Type.ToString()));
                    }
                    else if (entry != null)
                    {
                        uStatusUpdate(string.Format("Something unexpected happened, the return entry wasn't null but the return type wasn't ExistingServer. ServName: {0} || Type: {1}", entry.Server.ServerName, entry.Type.ToString()));
                        Toolbox.uDebugAddLog(string.Format("Unexpected Server Entry Returned: Serv: {0} || Type Ret {1} || Type Expect {3}", entry.Server.ServerName, entry.Type.ToString(), ServModifyType.ExistingServer.ToString()));
                    }
                }
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void btnRefreshServers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReadConfig(ConfigType.Servers);
                RefreshServerList();
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void btnOpenBotInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SwitchGrids(grdBotInfo);
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private async void btnChangePlaying_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string newStatus = txtPlayingValue.Text;
                await SetPlaying(newStatus);
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private async void btnStopPlaying_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await SetPlaying();
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private async void btnChangeName_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string newName = txtNameValue.Text;
                await ChangeName(newName);
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void btnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Toolbox._paths.LogLocation = txtLogDirectory.Text;
                SaveConfig(ConfigType.Paths);
                uStatusUpdate("Saved Settings Successfully");
                ShowNotification("Saved Application Settings", 3);
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void btnDumpDebug_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Toolbox.DumpDebugLog();
                uStatusUpdate("Manually Dumped Debug Log");
                ShowNotification("Successfully Dumped Debug Log", 3);
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void btnAddAdmin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ulong adminID = 0;
                var isUlong = ulong.TryParse(txtAdminUlong.Text, out adminID);
                if (!isUlong)
                {
                    ShowNotification("Admin Id entered was invalid, please try again", 5);
                    return;
                }
                if (Permissions.Administrators.Find(x => x.ID == adminID) != null)
                {
                    uStatusUpdate($"{adminID} is already in the admin list, canceling...");
                    return;
                }
                Administrator newAdmin = new Administrator() { ID = adminID };
                Permissions.Administrators.Add(newAdmin);
                ShowNotification($"Added admin ID: {adminID}", 4);
                Events.uStatusUpdateExt($"Added admin ID: {adminID}");
                tRefreshAdminList();
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void btnRemoveAdmin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ulong adminID = 0;
                var isUlong = ulong.TryParse(((Administrator)comboAdmins.SelectedItem).ID.ToString(), out adminID);
                if (!isUlong)
                {
                    ShowNotification("Admin ID selected from combobox is invalid", 5);
                    uStatusUpdate($"Combobox Admin ID invalid: {adminID}");
                    return;
                }
                var foundAdmin = Permissions.Administrators.Find(x => x.ID == adminID);
                if (foundAdmin == null)
                {
                    ShowNotification($"Admin profile not found for {adminID}", 5);
                    uStatusUpdate($"Admin profile not found for requested removal: {adminID}");
                    return;
                }
                Permissions.Administrators.Remove(foundAdmin);
                ShowNotification($"Removed admin: {foundAdmin.Username} | {foundAdmin.ID}", 4);
                uStatusUpdate($"Removed admin: {foundAdmin.Username} | {foundAdmin.ID}");
                tRefreshAdminList();
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void btnSaveRPG_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Management.SerializeData();
                Permissions.SerializePermissions();
                ShowNotification("Saved RPG Data", 3);
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void btnShowDebug_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Toolbox.uDebugAddLog("Toggling Status/Debug textbox");
                if (txtStatusValue.Visibility == Visibility.Visible)
                {
                    Toolbox.uDebugAddLog("Status window is currently visible, switching to debug window and changing button color");
                    btnShowDebug.Background = new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString("#FF4F1515"));
                    txtStatusValue.Visibility = Visibility.Hidden;
                    txtDebugValue.Visibility = Visibility.Visible;
                    txtDebugValue.Text = Toolbox.statusUpdater.DebugLog;
                    Toolbox.uDebugAddLog("Finished toggling to debug window");
                    ShowNotification("Changed status window to debug information", 5);
                }
                else
                {
                    Toolbox.uDebugAddLog("Debug window is currently visible, switching to status window and changing button color");
                    btnShowDebug.Background = new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString("#FF202020"));
                    txtStatusValue.Visibility = Visibility.Visible;
                    txtDebugValue.Visibility = Visibility.Hidden;
                    Toolbox.uDebugAddLog("Finished toggling to status window");
                    ShowNotification("Changed status window to standard status information", 6);
                }
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private async void btnCheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            await UpdateApplication();
        }

        private void btnOpenRPGData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SwitchGrids(grdRPGData);
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        #endregion

        #region Methods

        public void uStatusUpdate(string _status)
        {
            try
            {
                string _timeNow = $"{DateTime.Now.ToLocalTime().ToShortDateString()}_{DateTime.Now.ToLocalTime().ToShortTimeString()}";
                string _statusString = string.Format("{0} :: {1}{2}", _timeNow, _status, Environment.NewLine);
                bool isFocused = true;
                int lineCount = 0;
                Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { txtStatusValue.AppendText(_statusString); lineCount = txtStatusValue.LineCount; });
                Toolbox.uDebugAddLog(string.Format("STATUS: {0}", _status));
                Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { isFocused = txtStatusValue.IsFocused; });
                if (lineCount > 300)
                    DumpStatusLog();
                if (!isFocused)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { txtStatusValue.ScrollToEnd(); });
                }
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void SetupConfig()
        {
            try
            {
                string _currDir = Directory.GetCurrentDirectory();
                string _logDir = string.Format(@"{0}\Logs", _currDir);
                string _confDir = string.Format(@"{0}\Config", _currDir);
                string _pathConfig = string.Format(@"{0}\Paths.json", _confDir);
                string _servConfig = string.Format(@"{0}\ServerConfig.xml", _confDir);
                Toolbox._paths.ConfigLocation = _confDir;
                Toolbox._paths.LogLocation = _logDir;
                Toolbox._paths.PathsConfig = _pathConfig;
                Toolbox._paths.ServerConfig = _servConfig;
                if (!Directory.Exists(_logDir))
                {
                    Directory.CreateDirectory(_logDir); Toolbox.uDebugAddLog(string.Format("Didn't find Log Directory, created at: {0}", _logDir));
                }
                else { Toolbox.uDebugAddLog(string.Format("Found Log Directory at: {0}", _logDir)); }
                if (!Directory.Exists(_confDir))
                {
                    Directory.CreateDirectory(_confDir); Toolbox.uDebugAddLog(string.Format("Didn't find Config Directory, created at: {0}", _confDir));
                }
                else { Toolbox.uDebugAddLog(string.Format("Found Config Directory at: {0}", _confDir)); }
                if (!File.Exists(_pathConfig))
                {
                    CreateDefaultConfig(ConfigType.Paths); Toolbox.uDebugAddLog(string.Format("Paths.json not found, created at: {0}", _pathConfig));
                }
                else { Toolbox.uDebugAddLog(string.Format("Found Paths.json at: {0}", _pathConfig)); }
                if (!File.Exists(_servConfig))
                {
                    CreateDefaultConfig(ConfigType.Servers); Toolbox.uDebugAddLog(string.Format("ServerConfig.xml not found, created at {0}", _servConfig));
                }
                else { Toolbox.uDebugAddLog(string.Format("Found ServerConfig.xml at: {0}", _servConfig)); }
                ReadConfig(ConfigType.Paths);
                ReadConfig(ConfigType.Servers);
                RefreshServerList();
                Toolbox.uDebugAddLog("Finished reading config");
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        public static void ReadConfig(ConfigType confType)
        {
            try
            {
                switch (confType)
                {
                    case ConfigType.Paths:
                        using (StreamReader _sr = File.OpenText(Toolbox._paths.PathsConfig))
                        {
                            LocalSettings pathsCopy = Toolbox._paths;
                            string _origPath = Toolbox._paths.PathsConfig;
                            string _json = _sr.ReadToEnd();
                            Toolbox._paths = JsonConvert.DeserializeObject<LocalSettings>(_json);
                            if (!Directory.Exists(Toolbox._paths.ConfigLocation))
                                Toolbox._paths.ConfigLocation = pathsCopy.ConfigLocation;
                            if (!Directory.Exists(Toolbox._paths.LogLocation))
                                Toolbox._paths.LogLocation = pathsCopy.LogLocation;
                            if (!File.Exists(Toolbox._paths.PathsConfig))
                                Toolbox._paths.PathsConfig = pathsCopy.PathsConfig;
                            if (!File.Exists(Toolbox._paths.ServerConfig))
                                Toolbox._paths.ServerConfig = pathsCopy.ServerConfig;
                            Toolbox.uDebugAddLog(string.Format("{0} Deserialized:{1} LogLocation[{2}]{1} ConfigLocation[{3}]{1} PathsConfig[{4}]{1} ServerConfig[{5}]", _origPath, Environment.NewLine, Toolbox._paths.LogLocation, Toolbox._paths.ConfigLocation, Toolbox._paths.PathsConfig, Toolbox._paths.ServerConfig));
                        }
                        break;
                    case ConfigType.Servers:
                        ServerList.Clear();
                        StreamReader reader = new StreamReader(new FileStream(Toolbox._paths.ServerConfig, FileMode.Open, FileAccess.Read, FileShare.Read));
                        XmlDocument doc = new XmlDocument();
                        string xmlIn = reader.ReadToEnd();
                        reader.Close();
                        doc.LoadXml(xmlIn);
                        string[] attributes = { "game", "servername", "password", "ipaddress", "port", "queryport", "exthost", "modded", "runexepath", "exepath", "procname", "logpath" };
                        foreach (XmlNode child in doc.ChildNodes)
                            if (child.Name.Equals("GameServers"))
                                foreach (XmlNode node in child.ChildNodes)
                                {
                                    node.VerifyXMLNodeAttributes(attributes);
                                    int portNum = int.TryParse(node.Attributes["port"].Value, out portNum) ? Convert.ToInt32(node.Attributes["port"].Value) : 0;
                                    int queryNum = int.TryParse(node.Attributes["queryport"].Value, out queryNum) ? Convert.ToInt32(node.Attributes["queryport"].Value) : 0;
                                    GameServer gs = new GameServer
                                    {
                                        Game = node.Attributes["game"].Value ?? "",
                                        ServerName = node.Attributes["servername"].Value ?? "",
                                        Password = node.Attributes["password"].Value ?? "",
                                        IPAddress = node.Attributes["ipaddress"].Value ?? "",
                                        PortNum = portNum,
                                        QueryPort = queryNum,
                                        ExtHostname = node.Attributes["exthost"].Value ?? "",
                                        Modded = node.Attributes["modded"].Value.ToLower() == "true" ? true : false,
                                        ServerExe = node.Attributes["runexepath"].Value ?? "",
                                        ServerBatchPath = node.Attributes["exepath"].Value ?? "",
                                        ServerProcName = node.Attributes["procname"].Value ?? "",
                                        ServerLogPath = node.Attributes["logpath"].Value ?? ""
                                    };
                                    ServerList.Add(gs);
                                    Toolbox.uDebugAddLog(string.Format("Added GameServer from config: Game[{0}] ServName[{1}]", gs.Game, gs.ServerName));
                                }
                        ServerList = new ObservableCollection<GameServer>(ServerList.OrderBy(x => x.Game));
                        break;
                }
            }
            catch (Exception ex)
            {
                Toolbox.FullExceptionLog(ex);
            }
        }

        public static void SaveConfig(ConfigType confType)
        {
            try
            {
                switch (confType)
                {
                    case ConfigType.Paths:
                        string jSon = JsonConvert.SerializeObject(Toolbox._paths, Newtonsoft.Json.Formatting.Indented);
                        File.WriteAllText(Toolbox._paths.PathsConfig, jSon);
                        Toolbox.uDebugAddLog("Saved current config to Paths.json");
                        break;
                    case ConfigType.Servers:
                        XmlDocument doc = new XmlDocument();
                        XmlDeclaration xmlDec = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                        XmlElement root = doc.DocumentElement;
                        doc.InsertBefore(xmlDec, root);

                        XmlElement gameServ = doc.CreateElement("GameServers");
                        doc.AppendChild(gameServ);

                        foreach (GameServer game in ServerList)
                        {
                            XmlElement serv = doc.CreateElement("Server");
                            serv.SetAttribute("game", game.Game ?? "");
                            serv.SetAttribute("servername", game.ServerName ?? "");
                            serv.SetAttribute("password", game.Password ?? "");
                            serv.SetAttribute("ipaddress", game.IPAddress ?? "");
                            serv.SetAttribute("port", game.PortNum.ToString() ?? "");
                            serv.SetAttribute("queryport", game.QueryPort.ToString() ?? "");
                            serv.SetAttribute("exthost", game.ExtHostname ?? "");
                            serv.SetAttribute("modded", game.Modded.ToString() ?? "");
                            serv.SetAttribute("runexepath", game.ServerExe.ToString() ?? "");
                            serv.SetAttribute("exepath", game.ServerBatchPath ?? "");
                            serv.SetAttribute("procname", game.ServerProcName ?? "");
                            serv.SetAttribute("logpath", game.ServerLogPath ?? "");
                            gameServ.AppendChild(serv);
                        }
                        doc.Save(Toolbox._paths.ServerConfig);
                        Toolbox.uDebugAddLog("Saved current game server list to ServerConfig.xml");
                        break;
                }
            }
            catch (Exception ex)
            {
                Toolbox.FullExceptionLog(ex);
            }
        }

        private void RefreshServerList()
        {
            lvServers.ItemsSource = null;
            lvServers.ItemsSource = CurrentServerList;
        }

        private void CreateDefaultConfig(ConfigType confType)
        {
            try
            {
                string _currDir = Directory.GetCurrentDirectory();
                string _logDir = string.Format(@"{0}\Logs", _currDir);
                string _confDir = string.Format(@"{0}\Config", _currDir);
                string _pathConfig = string.Format(@"{0}\Paths.json", _confDir);
                string _servConfig = string.Format(@"{0}\ServerConfig.xml", _confDir);
                switch (confType)
                {
                    case ConfigType.Paths:
                        LocalSettings _pathsT = new LocalSettings()
                        {
                            LogLocation = _logDir,
                            ConfigLocation = _confDir,
                            PathsConfig = _pathConfig,
                            ServerConfig = _servConfig
                        };
                        Toolbox._paths.LogLocation = _logDir; Toolbox._paths.ConfigLocation = _confDir; Toolbox._paths.PathsConfig = _pathConfig; Toolbox._paths.ServerConfig = _servConfig;
                        string _json = JsonConvert.SerializeObject(_pathsT, Newtonsoft.Json.Formatting.Indented);
                        File.WriteAllText(_pathConfig, _json);
                        Toolbox.uDebugAddLog(string.Format("Created default Paths.json: LogLoc: {0} ConfLoc: {1} PathLoc: {2} ServLoc: {3}", _logDir, _confDir, _pathConfig, _servConfig));
                        break;
                    case ConfigType.Servers:
                        XDocument doc = new XDocument(new XElement("GameServers"));
                        doc.Save(Toolbox._paths.ServerConfig);
                        Toolbox.uDebugAddLog(string.Format("Created default ServerConfig.xml at {0}", _servConfig));
                        break;
                }
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void FullExceptionLog(Exception ex, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null, [CallerFilePath] string filePath = null)
        {
            string exString = string.Format("TimeStamp: {1}{0}Exception Type: {2}{0}Caller: {3} at {4}{0}Message: {5}{0}HR: {6}{0}StackTrace:{0}{7}{0}", Environment.NewLine, string.Format("{0}_{1}", DateTime.Now.ToLocalTime().ToString("MM-dd-yy"), DateTime.Now.ToLocalTime().ToLongTimeString()), ex.GetType().Name, caller, lineNumber, ex.Message, ex.HResult, ex.StackTrace);
            Toolbox.uDebugAddLog(string.Format("EXCEPTION: {0} at {1}", caller, lineNumber));
            uStatusUpdate(string.Format("An Exception Occured: {0} at {1}{2}Msg: {3}", caller, lineNumber, Environment.NewLine, ex.Message));
            string _logLocation = string.Format(@"{0}\Exceptions.log", Toolbox._paths.LogLocation);
            try
            {
                if (!File.Exists(_logLocation))
                    using (StreamWriter _sw = new StreamWriter(_logLocation))
                        _sw.WriteLine(exString + Environment.NewLine);
                else
                    using (StreamWriter _sw = File.AppendText(_logLocation))
                        _sw.WriteLine(exString + Environment.NewLine);
            }
            catch (IOException)
            {
                Toolbox.SaveFileRetry(_logLocation, exString);
            }
        }

        private void ResultLog(IResult ex, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null, [CallerFilePath] string filePath = null)
        {
            if (ex.ErrorReason.ToLower() == "unknown command.") return;
            string exString = $"TimeStamp: {$"{DateTime.Now.ToLocalTime().ToString("MM-dd-yy")} {DateTime.Now.ToLocalTime().ToLongTimeString()}"}{line}Exception Type: {ex.GetType().Name}{line}Caller: {caller} at {lineNumber}{line}Error: {ex.Error}{line}Error Reason: {ex.ErrorReason}{line}StackTrace: {line}{ex.Error.Value}{line}";
            Toolbox.uDebugAddLog(string.Format("RSLTFAIL: {0}", exString));
            uStatusUpdate(string.Format("A result failed: {0}", exString));
            string _logLocation = string.Format(@"{0}\Exceptions.log", Toolbox._paths.LogLocation);
            try
            {
                if (!File.Exists(_logLocation))
                    using (StreamWriter _sw = new StreamWriter(_logLocation))
                        _sw.WriteLine(exString);
                else
                    using (StreamWriter _sw = File.AppendText(_logLocation))
                        _sw.WriteLine(exString);
            }
            catch (IOException)
            {
                Toolbox.SaveFileRetry(_logLocation, exString);
            }
        }

        private void SlideGrid(Thickness from, Thickness to, Grid _grd)
        {
            try
            {
                ThicknessAnimation _animate = new ThicknessAnimation();
                _animate.From = from;
                _animate.To = to;
                _animate.AccelerationRatio = .9;
                _animate.Duration = new Duration(TimeSpan.FromSeconds(.3));
                _grd.BeginAnimation(Grid.MarginProperty, _animate);
                Toolbox.uDebugAddLog($"Slid Grid {_grd.Name} from {from.ToString()} to {to.ToString()}");
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void SlideVertical(double _top, double _bottom, Grid _grd)
        {
            try
            {
                ThicknessAnimation _animate = new ThicknessAnimation();
                _animate.From = new Thickness(_grd.Margin.Left, _grd.Margin.Top, _grd.Margin.Right, _grd.Margin.Bottom);
                _animate.To = new Thickness(_grd.Margin.Left, _top, _grd.Margin.Right, _bottom);
                _animate.AccelerationRatio = .9;
                _animate.Duration = new Duration(TimeSpan.FromSeconds(.3));
                _grd.BeginAnimation(Grid.MarginProperty, _animate);
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void SlideGridTo(double left, double top, double right, double bottom, Grid grd)
        {
            ThicknessAnimation animate = new ThicknessAnimation();
            animate.To = new Thickness(left, top, right, bottom);
            animate.AccelerationRatio = .9;
            animate.Duration = new Duration(TimeSpan.FromSeconds(.3));
            grd.BeginAnimation(Grid.MarginProperty, animate);
        }

        private void FadeGridIn(Grid grd)
        {
            Storyboard animate = ((Storyboard)this.Resources["sbFadeGridIn"]);
            animate.Begin(grd);
        }

        private void FadeGridOut(Grid grd)
        {
            Storyboard animate = ((Storyboard)this.Resources["sbFadeGridOut"]);
            animate.Begin(grd);
        }

        private void MainMenuGridSlide(bool secondClick)
        {
            try
            {
                if (!secondClick)
                {
                    SlideGridTo(0, 0, 0, 0, grdMenu);
                    Toolbox.uDebugAddLog("Slid Main Menu out of view");
                }
                else if (secondClick)
                {
                    SlideGridTo(0, 0, 0, -285, grdMenu);
                    Toolbox.uDebugAddLog("Slid Main Menu into view");
                }
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void SwitchGrids(Grid grd)
        {
            switch (grd.Visibility)
            {
                case Visibility.Visible:
                    grd.Visibility = Visibility.Hidden;
                    Toolbox.uDebugAddLog(string.Format("Hid Grid {0}", grd.Name));
                    MainMenuGridSlide(true);
                    break;
                case Visibility.Hidden:
                    foreach (Grid grid in FindVisualChildren<Grid>(grdMenu))
                    {
                        if (grid != grd)
                        {
                            grid.Visibility = Visibility.Hidden;
                            Toolbox.uDebugAddLog(string.Format("Hid Grid {0}", grid.Name));
                        }
                        else
                        {
                            grid.Visibility = Visibility.Visible;
                            Toolbox.uDebugAddLog(string.Format("Revealed Grid {0}", grid.Name));
                        }
                    }
                    MainMenuGridSlide(false);
                    break;
            }
        }

        private void HideGrids()
        {
            foreach (Grid grd in FindVisualChildren<Grid>(grdMenu))
            {
                grd.Visibility = Visibility.Hidden;
                Toolbox.uDebugAddLog(string.Format("Set Grid {0} to Hidden on launch", grd.Name));
            }
        }

        private void CheckVersion()
        {
            Toolbox._paths.CurrentVersion = GetVersionNumber();
            if (Toolbox._paths.PreviousVersion == new Version("0.0.0.0"))
                Toolbox._paths.LastUpdated = $"{DateTime.Now.ToLocalTime().ToString("MM-dd-yyyy hh:mm:ss tt")}";
            if (Toolbox._paths.PreviousVersion < Toolbox._paths.CurrentVersion)
                Toolbox._paths.Updated = true;
            lblUpdateTime.Text = Toolbox._paths.LastUpdated;
            lblVersionNumber.Text = $"Version {Toolbox._paths.CurrentVersion}";
            uStatusUpdate($"Current Version: {Toolbox._paths.CurrentVersion}");
            if (Toolbox._paths.Updated)
            {
                Version prevVersion = Toolbox._paths.PreviousVersion;
                Toolbox._paths.Updated = false;
                uStatusUpdate($"Updated to github v{Toolbox._paths.CurrentVersion} from v{prevVersion}");
                Toolbox._paths.PreviousVersion = Toolbox._paths.CurrentVersion;
                Toolbox._paths.LastUpdated = $"{DateTime.Now.ToLocalTime().ToString("MM-dd-yyyy hh:mm:ss tt")}";

                SaveConfig(ConfigType.Paths);

                BackgroundWorker worker = new BackgroundWorker() { WorkerReportsProgress = true };
                worker.ProgressChanged += (sender2, e2) => { if (e2.ProgressPercentage == 1) { RoutedEventArgs e3 = new RoutedEventArgs(); btnConnect_Click(sender2, e3); } };
                worker.DoWork += (sender, e) =>
                {
                    worker.ReportProgress(1);
                    while (client == null)
                    {
                        Thread.Sleep(1000);
                    }
                    while (client.ConnectionState != ConnectionState.Connected)
                    {
                        Thread.Sleep(1000);
                    }
                    if (Permissions.GeneralPermissions.logChannel != 0)
                    {
                        var channel = (IMessageChannel)client.GetChannel(Permissions.GeneralPermissions.logChannel);
                        channel.SendMessageAsync($"Bot upgraded to github v{Toolbox._paths.CurrentVersion} from v{prevVersion}");
                    }
                };
                worker.RunWorkerAsync();
            }
            else
                SaveConfig(ConfigType.Paths);
        }

        private Version GetVersionNumber()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }
                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        if (child != null && child is T)
                        {
                            yield return (T)child;
                        }
                    }
                }
            }
        }

        private void SaveWindowLocation()
        {
            Toolbox.uDebugAddLog("Saving window location");
            Thickness winLocation = new Thickness(this.Left, this.Top, this.Width, this.Height);
            Toolbox._paths.WindowLocation = winLocation;
            SaveConfig(ConfigType.Paths);
            Toolbox.uDebugAddLog($"Window Location Saved: [L]{winLocation.Left} [T]{winLocation.Top} [R]{winLocation.Right} [B]{winLocation.Bottom}");
        }

        private void LoadWindowLocation()
        {
            Toolbox.uDebugAddLog("Loading window location");
            Thickness savedLocation = Toolbox._paths.WindowLocation;
            this.Left = savedLocation.Left;
            this.Top = savedLocation.Top;
            this.Width = savedLocation.Right;
            this.Height = savedLocation.Bottom;
            Toolbox.uDebugAddLog($"Window Location Loaded and applied: [L]{savedLocation.Left} [T]{savedLocation.Top} [R]{savedLocation.Right} [B]{savedLocation.Bottom}");
        }

        private void DumpStatusLog()
        {
            string _dateNow = DateTime.Now.ToLocalTime().ToString("MM-dd-yy");
            string _logLocation = string.Format(@"{0}\StatusLog_{1}.log", Toolbox._paths.LogLocation, _dateNow);
            string textDump = string.Empty;
            Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { textDump = txtStatusValue.Text; txtStatusValue.Text = string.Empty; });
            try
            {
                if (!File.Exists(_logLocation))
                    using (StreamWriter _sw = new StreamWriter(_logLocation))
                        _sw.WriteLine(textDump);
                else
                    using (StreamWriter _sw = File.AppendText(_logLocation))
                        _sw.WriteLine(textDump);
            }
            catch (IOException)
            {
                Toolbox.SaveFileRetry(_logLocation, textDump);
            }
            uStatusUpdate($"Dumped status log to: {_logLocation}");
        }

        private void VerifyDebug()
        {
#if DEBUG
            uStatusUpdate("Running DEBUG Mode");
            btnTest.Visibility = Visibility.Visible;
#else
            btnTest.Visibility = Visibility.Hidden;   
#endif
        }

        public static void StartUpdate()
        {
            try
            {
                Permissions.SerializePermissions();
                Management.SerializeData();
                SaveConfig(ConfigType.Paths);
                SaveConfig(ConfigType.Servers);
                string updaterLocation = $@"{Directory.GetCurrentDirectory()}\PDBUpdater.exe";
                Process updater = new Process() { StartInfo = new ProcessStartInfo { FileName = updaterLocation } };
                updater.Start();
                Toolbox._paths.Updated = true;
            }
            catch (Exception ex)
            {
                Toolbox.FullExceptionLog(ex);
            }
        }

        public static void SaveRPGData()
        {
            try
            {
                Management.SerializeData();
                Permissions.SerializePermissions();
            }
            catch (Exception ex)
            {
                Toolbox.FullExceptionLog(ex);
            }
        }

        public static void SaveAppData()
        {
            try
            {
                SaveConfig(ConfigType.Paths);
                SaveConfig(ConfigType.Servers);
            }
            catch (Exception ex)
            {
                Toolbox.FullExceptionLog(ex);
            }
        }

        public static void SaveAllData()
        {
            try
            {
                SaveRPGData();
                SaveAppData();
                Toolbox.uDebugAddLog("Successfully saved all data");
            }
            catch (Exception ex)
            {
                Toolbox.FullExceptionLog(ex);
            }
        }

        private static void CleanupLogDir()
        {
            try
            {
                Toolbox.uDebugAddLog($"Cleaning up Log Directory: {Toolbox._paths.LogLocation}");
                DirectoryInfo _dI = new DirectoryInfo(Toolbox._paths.LogLocation);
                foreach (FileInfo _fI in _dI.GetFiles())
                {
                    if (_fI.CreationTime.ToLocalTime() <= DateTime.Now.AddDays(-14).ToLocalTime())
                    {
                        try
                        {
                            _fI.Delete(); Toolbox.uDebugAddLog(string.Format("Deleted old log file: {0}", _fI.Name));
                        }
                        catch (IOException ioe)
                        {
                            Toolbox.uDebugAddLog($"Unable to delete old log file: {_fI.Name} | {ioe.Message}");
                        }
                    }
                }
                Toolbox.uDebugAddLog("Finished cleaning up Log directory");
            }
            catch (Exception ex)
            {
                Toolbox.FullExceptionLog(ex);
            }
        }

        private void HandleGlobalAction(Toolbox.GlobalAction action)
        {
            try
            {
                switch (action)
                {
                    case Toolbox.GlobalAction.AdminChanged:
                        tRefreshAdminList();
                        break;
                    default:
                        uStatusUpdate($"Something went wrong and a Global Action wasn't handled, action: {action.ToString()}");
                        break;
                }
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void lblVersionNumber_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var verText = lblVersionNumber.Text.Replace("Version ", "").ToString();
                Clipboard.SetText(verText);
                Toolbox.uDebugAddLog($"Copied \"{verText}\" to the clipboard");
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        #endregion

        #region Threaded Methods

        private void ShowNotification(string notification, int showTime)
        {
            try
            {
                System.ComponentModel.BackgroundWorker worker = new System.ComponentModel.BackgroundWorker()
                {
                    WorkerReportsProgress = true
                };
                worker.ProgressChanged += (sender, e) =>
                {
                    try
                    {
                        ThicknessAnimation slideOut = new ThicknessAnimation() { AccelerationRatio = .9, Duration = new Duration(TimeSpan.FromSeconds(.3)), To = new Thickness(0, 42, 0, 0) };
                        ThicknessAnimation slideIn = new ThicknessAnimation() { AccelerationRatio = .9, Duration = new Duration(TimeSpan.FromSeconds(.3)), To = new Thickness(0, 42, -734, 0) };
                        switch (e.ProgressPercentage)
                        {
                            case 1:
                                Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { try { grdNotification.BeginAnimation(Grid.MarginProperty, slideOut); } catch (InvalidOperationException ioe) { uStatusUpdate($"Notification wasn't shown due to an exception: {ioe.Message}"); return; } catch (Exception ex) { FullExceptionLog(ex); } });
                                break;
                            case 2:
                                Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { try { grdNotification.BeginAnimation(Grid.MarginProperty, slideIn); } catch (InvalidOperationException ioe) { uStatusUpdate($"Notification wasn't shown due to an exception: {ioe.Message}"); return; } catch (Exception ex) { FullExceptionLog(ex); } });
                                break;
                            default:
                                Toolbox.uDebugAddLog($"Something happened and the incorrect notification state was used ({e.ProgressPercentage}), accepted states: 1 or 2");
                                break;
                        }
                    }
                    catch (InvalidOperationException ioe) { uStatusUpdate($"Notification wasn't shown due to an exception: {ioe.Message}"); return; }
                    catch (Exception ex)
                    {
                        FullExceptionLog(ex);
                        return;
                    }
                };
                worker.DoWork += (sender, e) =>
                {
                    try
                    {
                        Notification newNote = new Notification() { Message = notification, ShowTime = showTime };
                        Toolbox.uDebugAddLog($"New Notification: {newNote.Message}, {newNote.ShowTime}sec(s)");
                        Notification.notifications.Add(newNote);
                        if (notificationPlaying) { Toolbox.uDebugAddLog("Notification is currently playing, returning"); return; }
                        notificationPlaying = true;
                        Toolbox.uDebugAddLog("Notification wasn't playing, starting notification play cycles");
                        while (Notification.notifications.Count != 0)
                        {
                            foreach (var notif in Notification.notifications.ToList())
                            {
                                Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { try { lblNotificationValue.Text = notif.Message; } catch (Exception ex) { FullExceptionLog(ex); } });
                                worker.ReportProgress(1);
                                Thread.Sleep(TimeSpan.FromSeconds(notif.ShowTime));
                                worker.ReportProgress(2);
                                Notification.notifications.Remove(notif);
                                Toolbox.uDebugAddLog($"Removed notification: {notif.Message}");
                                Toolbox.uDebugAddLog($"Notifications left: {Notification.notifications.Count}");
                            }
                        }
                        notificationPlaying = false;
                        Toolbox.uDebugAddLog("Finished playing all notifications");
                    }
                    catch (InvalidOperationException ioe) { uStatusUpdate($"Notification wasn't shown due to an exception: {ioe.Message}"); return; }
                    catch (Exception ex)
                    {
                        FullExceptionLog(ex);
                        return;
                    }
                };
                worker.RunWorkerAsync();
            }
            catch (InvalidOperationException ioe) { uStatusUpdate($"Notification wasn't shown due to an exception: {ioe.Message}"); return; }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
                return;
            }
        }

        private void tUpdateConnectionStatus()
        {
            try
            {
                while (client == null)
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                while (_activeSession)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { lblConnectionValue.Text = string.Format("Connection: {0}", client.ConnectionState.ToString()); });
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
                Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { lblConnectionValue.Text = string.Format("Connection: {0}", client.ConnectionState.ToString()); });
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void tSaveRPGData()
        {
            Thread save = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(TimeSpan.FromMinutes(10));
                    Management.SerializeData();
                }
            });
            save.Start();
        }

        public void tRefreshAdminList()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                try
                {
                    Toolbox.uDebugAddLog("Refreshing admin list");
                    Thread.Sleep(TimeSpan.FromSeconds(3));
                    Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { try { comboAdmins.Items.Clear(); } catch (Exception ex) { FullExceptionLog(ex); } });
                    Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { try { txtAdminUlong.Text = ""; } catch (Exception ex) { FullExceptionLog(ex); } });
                    int count = 0;
                    if (Permissions.Administrators.Count <= 0)
                    {
                        Events.uStatusUpdateExt($"Refreshed admin list, total admins: {count}");
                        return;
                    }
                    foreach (var admin in Permissions.Administrators)
                    {
                        count++;
                        Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { comboAdmins.Items.Add(admin); });
                    }
                    Events.uStatusUpdateExt($"Refreshed admin list, total admins: {count}");
                }
                catch (Exception ex)
                {
                    FullExceptionLog(ex);
                }
            };
            worker.RunWorkerAsync();
        }

        #endregion

        #region Async Methods

        public async Task Start()
        {
            try
            {
                // Define the DiscordSocketClient
                client = new DiscordSocketClient();

                Thread loginUpdate = new Thread(async () =>
                {
                    string userName = Toolbox._paths.BotName;
                    string playingValue = Toolbox._paths.BotPlaying;
                    uStatusUpdate("Starting login updater");
                    while (client.ConnectionState != ConnectionState.Connected)
                        Thread.Sleep(500);
                    if (!string.IsNullOrWhiteSpace(userName)) { await ChangeName(userName); Toolbox.uDebugAddLog($"Previous bot name used from config: {userName}"); }
                    if (!string.IsNullOrEmpty(playingValue)) { await SetPlaying(playingValue); Toolbox.uDebugAddLog($"Previous bot playing status used from config: {playingValue}"); }
                    Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { txtNameValue.Text = client.CurrentUser.Username; });
                    Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { txtPlayingValue.Text = client.CurrentUser.Game.HasValue ? client.CurrentUser.Game.Value.Name : string.Empty; });
                    uStatusUpdate("Updated Name and Playing Value");
                });
                loginUpdate.Start();

                var token = Toolbox._paths.BotToken;
                commands = new CommandService();
                services = new ServiceCollection().BuildServiceProvider();
                await InstallCommands();

                // Login and connect to Discord.
                await client.LoginAsync(TokenType.Bot, token);
                await client.StartAsync();

                ShowNotification("Bot Successfully Connected", 4);
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private async Task SetPlaying()
        {
            try
            {
                await client.SetGameAsync(null);
                uStatusUpdate("Set Playing to null");
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private async Task SetPlaying(string status)
        {
            try
            {
                await client.SetGameAsync(status);
                uStatusUpdate(string.Format("Set Playing to: {0}", status));
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private async Task ChangeName(string newName)
        {
            try
            {
                await client.CurrentUser.ModifyAsync(x => x.Username = newName);
                uStatusUpdate(string.Format("Updated Bot Username to: {0}", newName));
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private async Task InstallCommands()
        {
            try
            {
                client.MessageReceived += MessageHandler;
                await commands.AddModulesAsync(Assembly.GetEntryAssembly());
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private async Task MessageHandler(SocketMessage arg)
        {
            try
            {
                SocketUserMessage msg = arg as SocketUserMessage;
                if (msg == null) return;
                int argPos = 0;
                if (!(msg.Author.Username == client.CurrentUser.Username) && Toolbox._paths.Snooping && !msg.HasCharPrefix(';', ref argPos))
                {
                    await msg.DeleteAsync();
                    await msg.Channel.SendMessageAsync(string.Format("{0}{1}{2}", msg.Author.Mention, Environment.NewLine, msg.Content.ToSnoopification()));
                    return;
                }
                if (!(msg.HasCharPrefix(';', ref argPos) || msg.HasMentionPrefix(client.CurrentUser, ref argPos))) return;

                CommandContext context = new CommandContext(client, msg);
                var cmd = $"User: {arg.Author.Username} ◥◤ Command: {arg.ToString()}";
                uStatusUpdate(cmd);
                Toolbox.uDebugAddLog(string.Format("COMMAND: {0}", cmd));
                var result = await commands.ExecuteAsync(context, argPos, services);
                if (!result.IsSuccess)
                {
                    var possEx = result.Error.Value;
                    uStatusUpdate(result.ErrorReason);
                    ResultLog(result);
                }
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private async Task UpdateApplication()
        {
            try
            {
                if (gitClient == null)
                    gitClient = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("PDB"));
                if (Toolbox._paths.CurrentVersion == null)
                    Toolbox._paths.CurrentVersion = new Version("0.1.0.0");
                var releases = await gitClient.Repository.Release.GetAll("rwobig93", "ServerRPGAdventure");
                var release = releases[0];
                Version releaseVersion = new Version(release.TagName);
                var result = Toolbox._paths.CurrentVersion.CompareTo(releaseVersion);
                if (result < 0)
                {
                    uStatusUpdate($"Newer release found, updating now... [Current]{Toolbox._paths.CurrentVersion} [Release]{releaseVersion}");
                    Toolbox._paths.CurrentVersion = releaseVersion;
                    SaveConfig(ConfigType.Paths);
                    SaveRPGData();
                    StartUpdate();
                }
                else
                {
                    uStatusUpdate($"Release Version is the same version or older than running assembly: {Environment.NewLine}[Current]{Toolbox._paths.CurrentVersion}{Environment.NewLine}[Release]{releaseVersion}");
                }
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private async Task SendConnectedMsg()
        {
            await Task.Delay(3000);
            if (Permissions.GeneralPermissions.logChannel != 0)
            {
                var channel = client.GetChannel(Permissions.GeneralPermissions.logChannel);
                if (channel != null)
                {
#if DEBUG
                    await ((IMessageChannel)channel).SendMessageAsync($"**{client.CurrentUser.Username}** has **connected** in **DEBUG** Mode biiiiiiiiiiiiiiiiiiiiiiatch!!!");
                    Toolbox.uDebugAddLog($"Sent connected debug message to channel {channel.Id}");
#else
                    await ((IMessageChannel)channel).SendMessageAsync($"**{client.CurrentUser.Username}** has **connected** biiiiiiiiiiiiiiiiiiiiiiatch!!!");
                    Toolbox.uDebugAddLog($"Sent connected message to channel {channel.Id}");
#endif
                    ShowNotification($"Bot {client.CurrentUser.Username} Sent connected message to log channel {((IMessageChannel)channel).Name}", 6);
                }
                else
                    Toolbox.uDebugAddLog($"IMessageChannel came back null when looking for {Permissions.GeneralPermissions.logChannel}, connected message wasn't sent");
            }
        }

        #endregion

        #region WIP

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            Permissions.SerializePermissions();
            Management.SerializeData();
            uStatusUpdate(Testing.ShowBackPackItems());
            OwnerProfile owner = new OwnerProfile() { OwnerID = 123456789, Currency = 696969, OwnerUN = "That one Guy", TotalPebbles = 99 };
            RPG.Owners.Add(owner);
            uStatusUpdate(Testing.EmulateFight(owner));
        }

        private void SetupTest()
        {
            RPG.Owners.Add(Testing.testiculeesProfile);
            var owner = RPG.Owners.Find(x => x.OwnerID == 12345678910111213);
            var chara = Testing.testiculeesCharacter;
            owner.CharacterList.Add(chara);
            owner.CurrentCharacter = chara;
            Permissions.Administrators.Add(new Administrator() { ID = owner.OwnerID, Username = owner.OwnerUN });
            uStatusUpdate("Testing setup");
        }

        #endregion
    }

    public class Notification
    {
        public static List<Notification> notifications = new List<Notification>();

        public string Message { get; set; }
        public int ShowTime { get; set; }
    }
}
