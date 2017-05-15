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
            uDebugAddLog(string.Format("{0}########################## Application Start ##########################{0}", Environment.NewLine));
            SetupConfig();
            txtLogDirectory.Text = _paths.LogLocation;
        }

        #region Global Variables

        public static DiscordSocketClient client;
        private CommandService commands;
        private DependencyMap map;
        public static StringBuilder _debugLog = new StringBuilder();
        public static Classes.Paths _paths = new Classes.Paths();
        private bool _activeSession = false;
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
            uStatusUpdate(LootDrop.PickLoot().ToString());
        }

        private void winMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            uDebugAddLog(string.Format("{0}########################## Application Stop ##########################{0}", Environment.NewLine));
            DumpDebugLog();
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
                if (string.IsNullOrEmpty(sGeneral.Default.Token))
                {
                    uDebugAddLog("Token wasn't found in sGeneral.Settings, prompting for token and sending notification");
                    SlideHorizontal(0, 0, grdToken);
                    //tShowNotification("A previous token wasn't found, please enter a bot token, save, then try again.", 7);
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
                uDebugAddLog("Disconnected Client");
                await client.LogoutAsync();
                uDebugAddLog("Logged out");
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
                sGeneral.Default.Token = txtTokenValue.Text;
                sGeneral.Default.Save();
                uDebugAddLog(string.Format("Saved new token: {0}", txtTokenValue.Text));
                SlideHorizontal(-grdToken.Width, grdToken.Width, grdToken);
                uDebugAddLog("Slid grdToken back out of view");
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
                sGeneral.Default.Token = string.Empty;
                sGeneral.Default.Save();
                txtTokenValue.Text = string.Empty;
                uDebugAddLog("Cleared token from sGeneral.Settings and the txtTokenValue textbox");
                tShowNotification("Cleared token data", 3);
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
                SlideHorizontal(-grdToken.Width, grdToken.Width, grdToken);
                uDebugAddLog("Slid grdToken out of view");
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
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
                uDebugAddLog(string.Format("Log Directory Location changed to: {0}", txtLogDirectory.Text));
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
                    uDebugAddLog(string.Format("Successfully added server {0} || Type {1}", entry.Server.ServerName, entry.Type.ToString()));
                }
                else if (entry != null)
                {
                    uStatusUpdate(string.Format("Something unexpected happened, the return entry wasn't null but the return type wasn't ExistingServer. ServName: {0} || Type: {1}", entry.Server.ServerName, entry.Type.ToString()));
                    uDebugAddLog(string.Format("Unexpected Server Entry Returned: Serv: {0} || Type {1} || Type Expect {3}", entry.Server.ServerName, entry.Type.ToString(), ServModifyType.NewServer.ToString()));
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
                        uDebugAddLog(string.Format("Successfully added server {0} || Type {1}", entry.Server.ServerName, entry.Type.ToString()));
                    }
                    else if (entry != null)
                    {
                        uStatusUpdate(string.Format("Something unexpected happened, the return entry wasn't null but the return type wasn't ExistingServer. ServName: {0} || Type: {1}", entry.Server.ServerName, entry.Type.ToString()));
                        uDebugAddLog(string.Format("Unexpected Server Entry Returned: Serv: {0} || Type Ret {1} || Type Expect {3}", entry.Server.ServerName, entry.Type.ToString(), ServModifyType.ExistingServer.ToString()));
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

        #endregion

        #region Methods

        public void uStatusUpdate(string _status)
        {
            try
            {
                string _timeNow = DateTime.Now.ToLocalTime().ToShortTimeString();
                string _statusString = string.Format("{0} :: {1}{2}", _timeNow, _status, Environment.NewLine);
                bool isFocused = true;
                Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { txtStatusValue.AppendText(_statusString); });
                uDebugAddLog(string.Format("STATUS: {0}", _status));
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

        private void uDebugAddLog(string _log)
        {
            try
            {
                string _dateNow = DateTime.Now.ToLocalTime().ToString("MM-dd-yy");
                string _timeNow = DateTime.Now.ToLocalTime().ToLongTimeString();
                _debugLog.AppendLine(string.Format("{0}_{1} :: {2}", _dateNow, _timeNow, _log));
                if (_debugLog.Length >= 250)
                    DumpDebugLog();
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private void DumpDebugLog()
        {
            try
            {
                string _dateNow = DateTime.Now.ToLocalTime().ToString("MM-dd-yy");
                string _debugLocation = string.Format(@"{0}\DebugLog_{1}.txt", _paths.LogLocation, _dateNow);
                if (!File.Exists(_debugLocation))
                    using (StreamWriter _sw = new StreamWriter(_debugLocation))
                        _sw.WriteLine(_debugLog.ToString());
                else
                    using (StreamWriter _sw = File.AppendText(_debugLocation))
                        _sw.WriteLine(_debugLog.ToString());
                _debugLog.Clear();
                DirectoryInfo _dI = new DirectoryInfo(_paths.LogLocation);
                foreach (FileInfo _fI in _dI.GetFiles())
                {
                    if (_fI.Name.StartsWith("DebugLog") && _fI.CreationTime.ToLocalTime() <= DateTime.Now.AddDays(-14).ToLocalTime())
                    {
                        _fI.Delete(); uDebugAddLog(string.Format("Deleted old DebugLog: {0}", _fI.Name));
                    }
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
                    Directory.CreateDirectory(_logDir); uDebugAddLog(string.Format("Didn't find Log Directory, created at: {0}", _logDir));
                }
                else { uDebugAddLog(string.Format("Found Log Directory at: {0}", _logDir)); }
                if (!Directory.Exists(_confDir))
                {
                    Directory.CreateDirectory(_confDir); uDebugAddLog(string.Format("Didn't find Config Directory, created at: {0}", _confDir));
                }
                else { uDebugAddLog(string.Format("Found Config Directory at: {0}", _confDir)); }
                if (!File.Exists(_pathConfig))
                {
                    CreateDefaultConfig(ConfigType.Paths); uDebugAddLog(string.Format("Paths.json not found, created at: {0}", _pathConfig));
                }
                else { uDebugAddLog(string.Format("Found Paths.json at: {0}", _pathConfig)); }
                if (!File.Exists(_servConfig))
                {
                    CreateDefaultConfig(ConfigType.Servers); uDebugAddLog(string.Format("ServerConfig.xml not found, created at {0}", _servConfig));
                }
                else { uDebugAddLog(string.Format("Found ServerConfig.xml at: {0}", _servConfig)); }
                ReadConfig(ConfigType.Paths);
                ReadConfig(ConfigType.Servers);
                uDebugAddLog("Finished reading config");
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
                            string _origPath = _paths.PathsConfig;
                            string _json = _sr.ReadToEnd();
                            _paths = JsonConvert.DeserializeObject<List<Paths>>(_json)[0];
                            uDebugAddLog(string.Format("{0} Deserialized:{1} LogLocation[{2}]{1} ConfigLocation[{3}]{1} PathsConfig[{4}]{1} ServerConfig[{5}]", _origPath, Environment.NewLine, _paths.LogLocation, _paths.ConfigLocation, _paths.PathsConfig, _paths.ServerConfig));
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
                                    uDebugAddLog(string.Format("Added GameServer from config: Game[{0}] ServName[{1}]", gs.Game, gs.ServerName));
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
                        List<Paths> _pathsT = new List<Paths>();
                        FileInfo _fI = new FileInfo(_paths.PathsConfig);
                        if (File.Exists(_fI.FullName))
                            _fI.Delete();
                        _pathsT.Add(new Paths
                        {
                            LogLocation = _paths.LogLocation,
                            ConfigLocation = _paths.ConfigLocation,
                            PathsConfig = _paths.PathsConfig,
                            ServerConfig = _paths.ServerConfig
                        });
                        string jSon = JsonConvert.SerializeObject(_pathsT.ToArray(), Newtonsoft.Json.Formatting.Indented);
                        File.WriteAllText(_paths.PathsConfig, jSon);
                        uDebugAddLog("Saved current config to Paths.json");
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
                            serv.SetAttribute("queryport", game.PortNum.ToString() ?? "");
                            serv.SetAttribute("exthost", game.ExtHostname ?? "");
                            serv.SetAttribute("modded", game.Modded.ToString() ?? "");
                            serv.SetAttribute("runexepath", game.ServerExe.ToString() ?? "");
                            serv.SetAttribute("exepath", game.ServerBatchPath ?? "");
                            serv.SetAttribute("procname", game.ServerProcName ?? "");
                            serv.SetAttribute("logpath", game.ServerLogPath ?? "");
                            gameServ.AppendChild(serv);
                        }
                        doc.Save(_paths.ServerConfig);
                        uDebugAddLog("Saved current game server list to ServerConfig.xml");
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
                        List<Classes.Paths> _pathsT = new List<Classes.Paths>();
                        _pathsT.Add(new Classes.Paths
                        {
                            LogLocation = _logDir,
                            ConfigLocation = _confDir,
                            PathsConfig = _pathConfig,
                            ServerConfig = _servConfig
                        });
                        _paths.LogLocation = _logDir; _paths.ConfigLocation = _confDir; _paths.PathsConfig = _pathConfig; _paths.ServerConfig = _servConfig;
                        string _json = JsonConvert.SerializeObject(_pathsT, Newtonsoft.Json.Formatting.Indented);
                        File.WriteAllText(_pathConfig, _json);
                        uDebugAddLog(string.Format("Created default Paths.json: LogLoc: {0} ConfLoc: {1} PathLoc: {2} ServLoc: {3}", _logDir, _confDir, _pathConfig, _servConfig));
                        break;
                    case ConfigType.Servers:
                        XDocument doc = new XDocument(new XElement("GameServers"));
                        doc.Save(_paths.ServerConfig);
                        uDebugAddLog(string.Format("Created default ServerConfig.xml at {0}", _servConfig));
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
            uDebugAddLog(string.Format("EXCEPTION: {0} at {1}", caller, lineNumber));
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
            uDebugAddLog(string.Format("RSLTFAIL: {0}", exString));
            uStatusUpdate(string.Format("A result failed: {0}", exString));
            string _logLocation = string.Format(@"{0}\Exceptions.txt", _paths.LogLocation);
            if (!File.Exists(_logLocation))
                using (StreamWriter _sw = new StreamWriter(_logLocation))
                    _sw.WriteLine(exString);
            else
                using (StreamWriter _sw = File.AppendText(_logLocation))
                    _sw.WriteLine(exString);
        }

        private void SlideHorizontal(double _left, double _right, Grid _grd)
        {
            try
            {
                ThicknessAnimation _animate = new ThicknessAnimation();
                _animate.From = new Thickness(_grd.Margin.Left, _grd.Margin.Top, _grd.Margin.Right, _grd.Margin.Bottom);
                _animate.To = new Thickness(_left, _grd.Margin.Top, _right, _grd.Margin.Bottom);
                _animate.AccelerationRatio = .9;
                _animate.Duration = new Duration(TimeSpan.FromSeconds(.3));
                _grd.BeginAnimation(Grid.MarginProperty, _animate);
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
                    uDebugAddLog("Slid Main Menu out of view");
                }
                else if (secondClick)
                {
                    SlideGridTo(0, 0, 0, -285, grdMenu);
                    uDebugAddLog("Slid Main Menu into view");
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
                    uDebugAddLog(string.Format("Hid Grid {0}", grd.Name));
                    MainMenuGridSlide(true);
                    break;
                case Visibility.Hidden:
                    foreach (Grid grid in FindVisualChildren<Grid>(grdMenu))
                    {
                        if (grid != grd)
                        {
                            grid.Visibility = Visibility.Hidden;
                            uDebugAddLog(string.Format("Hid Grid {0}", grid.Name));
                        }
                        else
                        {
                            grid.Visibility = Visibility.Visible;
                            uDebugAddLog(string.Format("Revealed Grid {0}", grid.Name));
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
                uDebugAddLog(string.Format("Set Grid {0} to Hidden on launch", grd.Name));
            }
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

        #endregion

        #region Threaded Methods

        private void tShowNotification(string _notification, int _howLong)
        {
            Thread _showNotification = new Thread(() =>
            {
                try
                {
                    double _currLeft = 0.0;
                    double _currRight = 0.0;
                    double _currTop = 0.0;
                    double _currBottom = 0.0;
                    uDebugAddLog("Notification Method Called");
                    Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
                    {
                        try
                        {
                            _currLeft = grdNotification.Margin.Left;
                            _currRight = grdNotification.Margin.Right;
                            _currTop = grdNotification.Margin.Top;
                            _currBottom = grdNotification.Margin.Bottom;
                            lblNotificationValue.Text = _notification;
                        }
                        catch (Exception ex)
                        {
                            FullExceptionLog(ex);
                        }
                    });
                    uDebugAddLog(string.Format("Notification Dimensions L[{0}] R[{1}] T[{2}] B[{3}] - Time: {4}", _currLeft, _currRight, _currTop, _currBottom, _howLong));
                    uDebugAddLog(string.Format("Notification Message: {0}", _notification));
                    ThicknessAnimation _animate = new ThicknessAnimation
                    {
                        From = new Thickness(_currLeft, _currTop, _currRight, _currBottom),
                        To = new Thickness(0, _currTop, 0, _currBottom),
                        AccelerationRatio = .9,
                        Duration = new Duration(TimeSpan.FromSeconds(.3))
                    };
                    Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { grdNotification.BeginAnimation(Grid.MarginProperty, _animate); });
                    uDebugAddLog(string.Format("Slid Notification into view, now waiting {0} seconds to slide back out", _howLong));
                    Thread.Sleep(TimeSpan.FromSeconds(_howLong));
                    ThicknessAnimation _animate2 = new ThicknessAnimation
                    {
                        From = new Thickness(0, _currTop, 0, _currBottom),
                        To = new Thickness(_currLeft, _currTop, _currRight, _currBottom),
                        AccelerationRatio = .9,
                        Duration = new Duration(TimeSpan.FromSeconds(.3))
                    };
                    Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { grdNotification.BeginAnimation(Grid.MarginProperty, _animate2); });
                    uDebugAddLog("Slid Notification back out of view");
                }   
                catch (Exception ex)
                {
                    FullExceptionLog(ex);
                }
            });
            _showNotification.Start();
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

                var token = sGeneral.Default.Token;
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
                uDebugAddLog(string.Format("COMMAND: {0}", cmd));
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

        } 

        #endregion
    }
}
