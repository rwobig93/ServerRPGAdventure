using PersonalDiscordBot.Classes;
using System;
using System.Windows;
using static PersonalDiscordBot.MainWindow;

namespace PersonalDiscordBot.Windows
{
    /// <summary>
    /// Interaction logic for Server_Editor.xaml
    /// </summary>
    public partial class Server_Editor : Window
    {
        private Server_Editor(ServModifyType type, GameServer gameSvr)
        {
            InitializeComponent();
            modifyServ = type;
            modifyingServer = gameSvr;
        }
        
        private ServModifyType modifyServ = ServModifyType.NewServer;
        private GameServer modifyingServer = null;
        private static ReturnedServerEntry returnEntry = null;

        private void btnServSave_Click(object sender, RoutedEventArgs e)
        {
            int portNum = int.TryParse(txtServPortValue.Text, out portNum) ? Convert.ToInt32(txtServPortValue.Text) : 0;
            int queryNum = int.TryParse(txtQueryPortValue.Text, out queryNum) ? Convert.ToInt32(txtQueryPortValue.Text) : 0;
            if (portNum == 0)
            {
                MessageBox.Show("The port number entered was invalid, please try again");
                return;
            }
            if (queryNum == 0 && !string.IsNullOrWhiteSpace(txtQueryPortValue.Text))
            {
                MessageBox.Show("Something was entered into the Query Port textbox but wasn't valid, please try again");
                return;
            }
            switch (modifyServ)
            {
                case ServModifyType.NewServer:
                    GameServer newServ = new GameServer
                    {
                        Game = txtServGameValue.Text,
                        ServerName = txtServNameValue.Text,
                        Password = txtServPassValue.Text,
                        IPAddress = txtServIPValue.Text,
                        ExtHostname = txtExtHostnameValue.Text,
                        PortNum = portNum,
                        QueryPort = queryNum,
                        Modded = comboServModded.Text.ToLower() == "true" ? true : false,
                        ServerExe = txtServExePath.Text,
                        ServerBatchPath = txtServStartPathValue.Text,
                        ServerProcName = txtServProcessValue.Text,
                        ServerLogPath = txtServLogLocationValue.Text
                    };
                    ServerList.Add(newServ);
                    returnEntry = new ReturnedServerEntry { Server = newServ, Type = ServModifyType.NewServer };
                    break;
                case ServModifyType.ExistingServer:
                    ServerList.Remove(modifyingServer);
                    GameServer chngdServ = new GameServer
                    {
                        Game = txtServGameValue.Text,
                        ServerName = txtServNameValue.Text,
                        Password = txtServPassValue.Text,
                        IPAddress = txtServIPValue.Text,
                        ExtHostname = txtExtHostnameValue.Text,
                        PortNum = portNum,
                        QueryPort = queryNum,
                        Modded = comboServModded.Text.ToLower() == "true" ? true : false,
                        ServerExe = txtServExePath.Text,
                        ServerBatchPath = txtServStartPathValue.Text,
                        ServerProcName = txtServProcessValue.Text,
                        ServerLogPath = txtServLogLocationValue.Text
                    };
                    ServerList.Add(chngdServ);
                    returnEntry = new ReturnedServerEntry { Server = chngdServ, Type = ServModifyType.ExistingServer };
                    break;
            }
            this.Close();
        }

        private void btnServCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public static ReturnedServerEntry Open(ServModifyType type, GameServer gameSvr)
        {
            Server_Editor se = new Server_Editor(type, gameSvr);
            if (type == ServModifyType.ExistingServer)
            {
                se.txtServGameValue.Text = gameSvr.Game;
                se.txtServNameValue.Text = gameSvr.ServerName;
                se.txtServPassValue.Text = gameSvr.Password;
                se.txtServIPValue.Text = gameSvr.IPAddress;
                se.txtExtHostnameValue.Text = gameSvr.ExtHostname;
                se.txtServPortValue.Text = gameSvr.PortNum.ToString();
                se.txtQueryPortValue.Text = gameSvr.QueryPort.ToString();
                se.comboServModded.Text = gameSvr.Modded.ToString().ToLower().Contains("true") ? "True" : "False";
                se.txtServExePath.Text = gameSvr.ServerExe;
                se.txtServStartPathValue.Text = gameSvr.ServerBatchPath;
                se.txtServProcessValue.Text = gameSvr.ServerProcName;
                se.txtServLogLocationValue.Text = gameSvr.ServerLogPath;
            }
            se.ShowDialog();
            return returnEntry;
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                DragMove();
            }
        }
    }

    public class ReturnedServerEntry
    {
        public GameServer Server { get; set; }
        public ServModifyType Type { get; set; }
    }
}
