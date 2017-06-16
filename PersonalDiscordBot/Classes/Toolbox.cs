using Discord.Commands;
using PersonalDiscordBot.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml;

namespace PersonalDiscordBot.Classes
{
    public static class Extensions
    {
        public static void uDebugAddLogExternal(string _log)
        {
            string _dateNow = DateTime.Now.ToLocalTime().ToString("MM-dd-yy");
            string _timeNow = DateTime.Now.ToLocalTime().ToLongTimeString();
            Toolbox.uDebugAddLog(string.Format("{0}_{1} :: {2}", _dateNow, _timeNow, _log));
        }
        public static void AddToDebugLog(this string _log)
        {
            string _dateNow = DateTime.Now.ToLocalTime().ToString("MM-dd-yy");
            string _timeNow = DateTime.Now.ToLocalTime().ToLongTimeString();
            Toolbox.uDebugAddLog(string.Format("{0}_{1} :: {2}", _dateNow, _timeNow, _log));
        }

        public static void VerifyXMLNodeAttributes(this XmlNode node, string attribute)
        {
            if (node.Attributes[attribute] == null)
            {
                XmlAttribute attr = node.OwnerDocument.CreateAttribute(attribute);
                node.Attributes.Append(attr);
                node.Attributes[attribute].Value = "";
                uDebugAddLogExternal(string.Format("Added missing {0} attribute to XMLNode {1}", attribute, node.Name));
            }
            node.OwnerDocument.Save(MainWindow._paths.ServerConfig);
        }

        public static void VerifyXMLNodeAttributes(this XmlNode node, string[] attributes)
        {
            foreach (var attribute in attributes)
                if (node.Attributes[attribute] == null)
                {
                    XmlAttribute attr = node.OwnerDocument.CreateAttribute(attribute);
                    node.Attributes.Append(attr);
                    node.Attributes[attribute].Value = "";
                    uDebugAddLogExternal(string.Format("Added missing {0} attribute to XMLNode {1}", attribute, node.Name));
                }
            node.OwnerDocument.Save(MainWindow._paths.ServerConfig);
        }

        public static string ToUpperFirst(this string str)
        {
            if (str.Length <= 0 || str == null)
                return null;
            else if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);
            else
                return str.ToUpper();
        }

        public static string ToUpperAllFirst(this string str)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        }

        public static string ReturnType(this object obj)
        {
            return obj.GetType().ToString();
        }

        public static int ToArrayLength(this Array array)
        {
            return array.Length - 1;
        }
    }

    public class PromptArgs : EventArgs
    {
        private string content;
        public PromptArgs(string msgContent)
        {
            this.content = msgContent;
        }
        public string Content { get { return content; } }
    }

    public class MatchArgs : EventArgs
    {
        private string enemyCount;
        private int experienceEarned;
        private TimeSpan matchTime;
        public MatchArgs(string enemies, int exp, TimeSpan time)
        {
            this.enemyCount = enemies;
            this.experienceEarned = exp;
            this.matchTime = time;
        }
        public string EnemyCount { get { return enemyCount; } }
        public int ExperienceEarned { get { return experienceEarned; } }
        public TimeSpan MatchTime { get { return matchTime; } }
    }

    public static class Toolbox
    {
        public static StringBuilder debugLog = new StringBuilder();
        public static Classes.LocalSettings _paths = new Classes.LocalSettings();

        public static void uDebugAddLog(string _log)
        {
            try
            {
                string _dateNow = DateTime.Now.ToLocalTime().ToString("MM-dd-yy");
                string _timeNow = DateTime.Now.ToLocalTime().ToLongTimeString();
                debugLog.AppendLine(string.Format("{0}_{1} :: {2}", _dateNow, _timeNow, _log));
                if (debugLog.Length >= 5000)
                    DumpDebugLog();
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        public static void DumpDebugLog()
        {
            try
            {
                string _dateNow = DateTime.Now.ToLocalTime().ToString("MM-dd-yy");
                string _debugLocation = string.Format(@"{0}\DebugLog_{1}.txt", _paths.LogLocation, _dateNow);
                if (!File.Exists(_debugLocation))
                    using (StreamWriter _sw = new StreamWriter(_debugLocation))
                        _sw.WriteLine(debugLog.ToString());
                else
                    using (StreamWriter _sw = File.AppendText(_debugLocation))
                        _sw.WriteLine(debugLog.ToString());
                debugLog.Clear();
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

        private static void FullExceptionLog(Exception ex, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null, [CallerFilePath] string filePath = null)
        {
            string exString = string.Format("TimeStamp: {1}{0}Exception Type: {2}{0}Caller: {3} at {4}{0}Message: {5}{0}HR: {6}{0}StackTrace:{0}{7}{0}", Environment.NewLine, string.Format("{0}_{1}", DateTime.Now.ToLocalTime().ToString("MM-dd-yy"), DateTime.Now.ToLocalTime().ToLongTimeString()), ex.GetType().Name, caller, lineNumber, ex.Message, ex.HResult, ex.StackTrace);
            uDebugAddLog(string.Format("EXCEPTION: {0} at {1}", caller, lineNumber));
            string _logLocation = string.Format(@"{0}\Exceptions.log", _paths.LogLocation);
            if (!File.Exists(_logLocation))
                using (StreamWriter _sw = new StreamWriter(_logLocation))
                    _sw.WriteLine(exString + Environment.NewLine);
            else
                using (StreamWriter _sw = File.AppendText(_logLocation))
                    _sw.WriteLine(exString + Environment.NewLine);
        }

        public static List<RebootedServer> serversRebooted = new List<RebootedServer>();

        public static void RemoveRebootedServer(RebootedServer rebServ)
        {
            try
            {
                Thread remServ = new Thread(() =>
                {
                    try
                    {
                        Thread.Sleep(TimeSpan.FromMinutes(15));
                        serversRebooted.Remove(rebServ);
                        string.Format("Removed rebooted server entry for game server {0} after 15 min", rebServ.Server.ServerName).AddToDebugLog();
                    }
                    catch (Exception ex)
                    {
                        ServerModule.FullExceptionLog(ex);
                    }
                });
                remServ.Start();
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        public static void RefreshItemSource(this ListView listView)
        {
            var tempStorage = listView.ItemsSource;
            listView.ItemsSource = null;
            listView.ItemsSource = tempStorage;
        }

        public static bool IsPortOpen(string ipAddress, int portNum)
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    socket.Connect(ipAddress, portNum);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.ConnectionRefused || ex.SocketErrorCode == SocketError.TimedOut)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        #region UpdateStatus Event

        public delegate void MessageShown(PromptArgs args);
        public static event MessageShown MessagePromptShown;
        public static void uStatusUpdateExt(string status)
        {
            PromptArgs args = new PromptArgs(status);
            MessagePromptShown(args);
        }

        #endregion
    }

}
