﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalDiscordBot.Classes
{
    [Serializable]
    public class LocalSettings
    {
        public Version CurrentVersion { get; set; } = new Version("0.1.0.0");
        public bool Updated { get; set; } = false;
        public string LogLocation { get; set; }
        public string ConfigLocation { get; set; }
        public string PathsConfig { get; set; }
        public string ServerConfig { get; set; }
        public string BotToken { get; set; }
        public string BotPlaying { get; set; }
        public string BotName { get; set; }
    }
}
