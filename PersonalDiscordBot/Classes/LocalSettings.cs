using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PersonalDiscordBot.Classes
{
    public class LocalSettings
    {
        // Application Settings
        public Thickness WindowLocation { get; set; } = new Thickness(0, 0, 0, 0);
        public Version CurrentVersion { get; set; } = new Version("0.1.0.0");
        public Version PreviousVersion { get; set; } = new Version("0.0.0.0");
        public bool Updated { get; set; } = false;
        public string LastUpdated { get; set; } = "08-23-1993 06:30:00 AM";
        // Local Resource Settings
        public string LogLocation { get; set; }
        public string ConfigLocation { get; set; }
        public string PathsConfig { get; set; }
        public string ServerConfig { get; set; }
        // Bot settings
        public string BotToken { get; set; }
        public string BotPlaying { get; set; }
        public string BotName { get; set; }
        public bool Snooping { get; set; } = false;
    }
}
