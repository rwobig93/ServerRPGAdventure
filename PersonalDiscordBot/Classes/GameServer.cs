using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalDiscordBot.Classes
{
    public class GameServer
    {
        public string Game { get; set; }
        public string ServerName { get; set; }
        public string Password { get; set; }
        public string IPAddress { get; set; }
        public string ExtHostname { get; set; }
        public string ServerExe { get; set; }
        public string ServerBatchPath { get; set; }
        public string ServerProcName { get; set; }
        public string ServerLogPath { get; set; }
        public int PortNum { get; set; }
        public int QueryPort { get; set; }
        public bool Modded { get; set; }
    }

    public class RebootedServer
    {
        public GameServer Server { get; set; }
        public DateTime Rebooted { get; set; }
    }
}
