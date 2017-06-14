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
using PersonalDiscordBot.Settings;
using System.Reflection;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.Xml;
using System.Net;
using System.Runtime.CompilerServices;
using PersonalDiscordBot.Windows;
using System.ComponentModel;

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
            txtLogDirectory.Text = _paths.LogLocation;
            Toolbox.MessagePromptShown += (e) => { uStatusUpdate(e.Content); };
        }

        #region Global Variables
        
        public static DiscordSocketClient client;
        private CommandService commands;
        private DependencyMap map;
        public static Classes.LocalSettings _paths = new Classes.LocalSettings();
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
            HideGrids();
            UpdateVerison();
            Management.DeSerializeData();
            tSaveRPGData();
            RefreshAdminList();
        }

        private void winMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Management.SerializeData();
            Toolbox.uDebugAddLog(string.Format("{0}########################## Application Stop ##########################{0}", Environment.NewLine));
            Toolbox.DumpDebugLog();
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
                _activeSession = true;
                if (string.IsNullOrEmpty(_paths.BotToken))
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
                await client.DisconnectAsync();
                Toolbox.uDebugAddLog("Disconnected Client");
                await client.LogoutAsync();
                Toolbox.uDebugAddLog("Logged out");
                _activeSession = false;
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
                _paths.BotToken = txtTokenValue.Text;
                SaveConfig(ConfigType.Paths);
                Toolbox.uDebugAddLog(string.Format("Saved new token: {0}", _paths.BotToken));
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
                _paths.BotToken = string.Empty;
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
                _paths.LogLocation = txtLogDirectory.Text;
                SaveConfig(ConfigType.Paths);
                uStatusUpdate("Saved Settings Successfully");
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void btnDumpDebug_Click(object sender, RoutedEventArgs e)
        {
            Toolbox.DumpDebugLog();
        }

        private void btnAddAdmin_Click(object sender, RoutedEventArgs e)
        {
            ulong adminID = 0;
            var isUlong = ulong.TryParse(txtAdminUlong.Text, out adminID);
            if (!isUlong)
            {
                ShowNotification("Admin Id entered was invalid, please try again", 5);
                return;
            }
            Permissions.Administrators.Add(adminID);
            Toolbox.uStatusUpdateExt($"Added admin ID: {adminID}");
            RefreshAdminList();
        }

        private void btnRemoveAdmin_Click(object sender, RoutedEventArgs e)
        {
            ulong adminID = 0;
            var isUlong = ulong.TryParse(comboAdmins.SelectedItem.ToString(), out adminID);
            if (!isUlong)
            {
                ShowNotification("Admin ID selected from combobox is invalid", 5);
                uStatusUpdate($"Combobox Admin ID invalid: {adminID}");
                return;
            }
            Permissions.Administrators.Remove(adminID);
            uStatusUpdate($"Removed admin ID: {adminID}");
            RefreshAdminList();
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
                Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { txtStatusValue.AppendText(_statusString); });
                Toolbox.uDebugAddLog(string.Format("STATUS: {0}", _status));
                Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { isFocused = txtStatusValue.IsFocused; });
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
                _paths.ConfigLocation = _confDir; _paths.LogLocation = _logDir; _paths.PathsConfig = _pathConfig; _paths.ServerConfig = _servConfig;
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
                Toolbox.uDebugAddLog("Finished reading config");
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void ReadConfig(ConfigType confType)
        {
            try
            {
                switch (confType)
                {
                    case ConfigType.Paths:
                        using (StreamReader _sr = File.OpenText(_paths.PathsConfig))
                        {
                            LocalSettings pathsCopy = _paths;
                            string _origPath = _paths.PathsConfig;
                            string _json = _sr.ReadToEnd();
                            _paths = JsonConvert.DeserializeObject<List<LocalSettings>>(_json)[0];
                            if (!Directory.Exists(_paths.ConfigLocation))
                                _paths.ConfigLocation = pathsCopy.ConfigLocation;
                            if (!Directory.Exists(_paths.LogLocation))
                                _paths.LogLocation = pathsCopy.LogLocation;
                            if (!File.Exists(_paths.PathsConfig))
                                _paths.PathsConfig = pathsCopy.PathsConfig;
                            if (!File.Exists(_paths.ServerConfig))
                                _paths.ServerConfig = pathsCopy.ServerConfig;
                            Toolbox._paths = _paths;
                            Toolbox.uDebugAddLog(string.Format("{0} Deserialized:{1} LogLocation[{2}]{1} ConfigLocation[{3}]{1} PathsConfig[{4}]{1} ServerConfig[{5}]", _origPath, Environment.NewLine, _paths.LogLocation, _paths.ConfigLocation, _paths.PathsConfig, _paths.ServerConfig));
                            
                        }
                        break;
                    case ConfigType.Servers:
                        ServerList.Clear();
                        StreamReader reader = new StreamReader(new FileStream(_paths.ServerConfig, FileMode.Open, FileAccess.Read, FileShare.Read));
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
                        lvServers.ItemsSource = null;
                        lvServers.ItemsSource = CurrentServerList;
                        break;
                }
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void SaveConfig(ConfigType confType)
        {
            try
            {
                switch (confType)
                {
                    case ConfigType.Paths:
                        List<LocalSettings> _pathsT = new List<LocalSettings>();
                        FileInfo _fI = new FileInfo(_paths.PathsConfig);
                        if (File.Exists(_fI.FullName))
                            _fI.Delete();
                        _pathsT.Add(new LocalSettings
                        {
                            LogLocation = _paths.LogLocation,
                            ConfigLocation = _paths.ConfigLocation,
                            PathsConfig = _paths.PathsConfig,
                            ServerConfig = _paths.ServerConfig,
                            BotToken = _paths.BotToken
                        });
                        string jSon = JsonConvert.SerializeObject(_pathsT.ToArray(), Newtonsoft.Json.Formatting.Indented);
                        File.WriteAllText(_paths.PathsConfig, jSon);
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
                        doc.Save(_paths.ServerConfig);
                        Toolbox.uDebugAddLog("Saved current game server list to ServerConfig.xml");
                        break;
                }
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
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
                        List<Classes.LocalSettings> _pathsT = new List<Classes.LocalSettings>();
                        _pathsT.Add(new Classes.LocalSettings
                        {
                            LogLocation = _logDir,
                            ConfigLocation = _confDir,
                            PathsConfig = _pathConfig,
                            ServerConfig = _servConfig
                        });
                        _paths.LogLocation = _logDir; _paths.ConfigLocation = _confDir; _paths.PathsConfig = _pathConfig; _paths.ServerConfig = _servConfig;
                        string _json = JsonConvert.SerializeObject(_pathsT, Newtonsoft.Json.Formatting.Indented);
                        File.WriteAllText(_pathConfig, _json);
                        Toolbox.uDebugAddLog(string.Format("Created default Paths.json: LogLoc: {0} ConfLoc: {1} PathLoc: {2} ServLoc: {3}", _logDir, _confDir, _pathConfig, _servConfig));
                        break;
                    case ConfigType.Servers:
                        XDocument doc = new XDocument(new XElement("GameServers"));
                        doc.Save(_paths.ServerConfig);
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
            string _logLocation = string.Format(@"{0}\Exceptions.log", _paths.LogLocation);
            if (!File.Exists(_logLocation))
                using (StreamWriter _sw = new StreamWriter(_logLocation))
                    _sw.WriteLine(exString + Environment.NewLine);
            else
                using (StreamWriter _sw = File.AppendText(_logLocation))
                    _sw.WriteLine(exString + Environment.NewLine);
        }

        private void ResultLog(IResult ex)
        {
            if (ex.ErrorReason.ToLower() == "unknown command.") return;
            string exString = string.Format("ErrorReason[{0}] ErrorValue[{1}]", ex.ErrorReason, ex.Error.Value);
            Toolbox.uDebugAddLog(string.Format("RSLTFAIL: {0}", exString));
            uStatusUpdate(string.Format("A result failed: {0}", exString));
            string _logLocation = string.Format(@"{0}\Exceptions.txt", _paths.LogLocation);
            if (!File.Exists(_logLocation))
                using (StreamWriter _sw = new StreamWriter(_logLocation))
                    _sw.WriteLine(exString);
            else
                using (StreamWriter _sw = File.AppendText(_logLocation))
                    _sw.WriteLine(exString);
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

        private void UpdateVerison()
        {
            lblVersionNumber.Text = $"Version {GetVersionNumber()}";
            uStatusUpdate($"Current Version: {GetVersionNumber()}");
        }

        private string GetVersionNumber()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
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

        private void RefreshAdminList()
        {
            Toolbox.uDebugAddLog("Refreshing admin list");
            comboAdmins.Items.Clear();
            int count = 0;
            foreach (var admin in Permissions.Administrators)
            {
                count++;
                comboAdmins.Items.Add(admin);
            }
            Toolbox.uStatusUpdateExt($"Refreshed admin list, total admins: {count}");
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
                    ThicknessAnimation slideOut = new ThicknessAnimation() { AccelerationRatio = .9, Duration = new Duration(TimeSpan.FromSeconds(.3)), To = new Thickness(0, 42, 0, 0) };
                    ThicknessAnimation slideIn = new ThicknessAnimation() { AccelerationRatio = .9, Duration = new Duration(TimeSpan.FromSeconds(.3)), To = new Thickness(0, 42, -734, 0) };
                    switch (e.ProgressPercentage)
                    {
                        case 1:
                            grdNotification.BeginAnimation(Grid.MarginProperty, slideOut);
                            break;
                        case 2:
                            grdNotification.BeginAnimation(Grid.MarginProperty, slideIn);
                            break;
                        default:
                            Toolbox.uDebugAddLog("Something happened and the incorrect notification state was used, accepted states: 1 or 2");
                            break;
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
                    catch (Exception ex)
                    {
                        FullExceptionLog(ex);
                    }
                };
                worker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void tUpdateConnectionStatus()
        {
            try
            {
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
        #endregion

        #region Async Methods

        public async Task Start()
        {
            try
            {
                Thread loginUpdate = new Thread(() =>
                {
                    uStatusUpdate("Starting login updater");
                    while (client.ConnectionState != ConnectionState.Connected)
                        Thread.Sleep(500);
                    Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { txtNameValue.Text = client.CurrentUser.Username; });
                    Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { txtPlayingValue.Text = client.CurrentUser.Game.HasValue ? client.CurrentUser.Game.Value.Name : string.Empty; });
                    uStatusUpdate("Updated Name and Playing Value");
                });
                loginUpdate.Start();

                // Define the DiscordSocketClient
                client = new DiscordSocketClient();

                var token = _paths.BotToken;
                commands = new CommandService();
                map = new DependencyMap();
                map.Add(client);
                map.Add(commands);
                await InstallCommands();

                // Login and connect to Discord.
                await client.LoginAsync(TokenType.Bot, token);
                await client.ConnectAsync();
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
                if (!(msg.Author.Username == client.CurrentUser.Username) && sGeneral.Default.Snooping && !msg.HasCharPrefix(';', ref argPos))
                {
                    await msg.DeleteAsync();
                    await msg.Channel.SendMessageAsync(string.Format("{0}{1}{2}", msg.Author.Mention, Environment.NewLine, msg.Content.ToSnoopification()));
                    return;
                }
                if (!(msg.HasCharPrefix(';', ref argPos) || msg.HasMentionPrefix(client.CurrentUser, ref argPos))) return;
                
                CommandContext context = new CommandContext(client, msg);
                string cmd = string.Format("User: {0} ◥◤ Command: {1}", arg.Author.Username, arg.ToString());
                uStatusUpdate(cmd);
                Toolbox.uDebugAddLog(string.Format("COMMAND: {0}", cmd));
                var result = await commands.ExecuteAsync(context, argPos, map);
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

        #endregion

        #region WIP

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            uStatusUpdate(Testing.LootDropGen());
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
