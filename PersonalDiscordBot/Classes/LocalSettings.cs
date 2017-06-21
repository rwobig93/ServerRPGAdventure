using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalDiscordBot.Classes
{
    [Serializable]
    public class LocalSettings
    {
        public string LogLocation { get; set; }
        public string ConfigLocation { get; set; }
        public string PathsConfig { get; set; }
        public string ServerConfig { get; set; }
        public string BotToken { get; set; }
    }
}
