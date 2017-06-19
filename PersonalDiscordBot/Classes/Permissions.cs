using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.ComponentModel;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;
using Discord.Commands;

namespace PersonalDiscordBot.Classes
{
    class Permissions
    {
        public static List<ulong> Administrators = new List<ulong>();
        public static List<ulong> AllowedChannels = new List<ulong>();

        public static void SerializePermissions()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.RunWorkerCompleted += (sender, e) => { Events.uStatusUpdateExt($"Admin Serialization Complete"); };
            worker.DoWork += (sender, e) =>
            {
                Events.uStatusUpdateExt("Serializing Permission Data");
                string permPath = $@"{Directory.GetCurrentDirectory()}\Permissions";
                if (!Directory.Exists(permPath))
                {
                    Directory.CreateDirectory(permPath);
                    Toolbox.uDebugAddLog($"Permissions folder created: {permPath}");
                }
                else
                    Toolbox.uDebugAddLog($"Permissions already exists: {permPath}");
                var json = JsonConvert.SerializeObject(Administrators, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                File.WriteAllText($@"{permPath}\Administrators.perm", json);
                Toolbox.uDebugAddLog($"Serialized Administrators.perm");
                var json2 = JsonConvert.SerializeObject(AllowedChannels, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                File.WriteAllText($@"{permPath}\RPGChannels.perm", json2);
                Toolbox.uDebugAddLog($"Serialized RPGChannels.perm");
            };
            worker.RunWorkerAsync();
        }

        public static void DeSerializePermissions()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.RunWorkerCompleted += (sender, e) => { Events.uStatusUpdateExt($"Admin Deserialization Complete"); };
            worker.DoWork += (sender, e) =>
            {
                Events.uStatusUpdateExt("Deserializing Permission Data");
                string loadPath = $@"{Directory.GetCurrentDirectory()}\Permissions";
                string adminPerm = $@"{loadPath}\Administrators.perm";
                string rpgPerm = $@"{loadPath}\RPGChannels.perm";
                if (!Directory.Exists(loadPath))
                {
                    Toolbox.uDebugAddLog($"Permissions folder doesn't exist, stopping deserialization: {loadPath}");
                    return;
                }
                if (File.Exists(adminPerm))
                {
                    Toolbox.uDebugAddLog($"Found Administrators.perm file: {adminPerm}");
                    using (StreamReader sr = File.OpenText(adminPerm))
                    {
                        Administrators = JsonConvert.DeserializeObject<List<ulong>>(sr.ReadToEnd());
                        Toolbox.uDebugAddLog("Deserialized Administrators.perm");
                    }
                }
                else
                    Toolbox.uDebugAddLog($"Administrators.perm doesn't exist: {adminPerm}");
                if (File.Exists(rpgPerm))
                {
                    Toolbox.uDebugAddLog($"Found RPGChannels.perm file: {rpgPerm}");
                    using (StreamReader sr = File.OpenText(rpgPerm))
                    {
                        AllowedChannels = JsonConvert.DeserializeObject<List<ulong>>(sr.ReadToEnd());
                        Toolbox.uDebugAddLog("Deserialized RPGChannels.perm");
                    }
                }
            };
            worker.RunWorkerAsync();
        }

        public static bool AdminPermissions(CommandContext context)
        {
            return Administrators.Contains(context.Message.Author.Id);
        }

        public static bool RPGChannelPermission(CommandContext context)
        {
            return AllowedChannels.Contains(context.Channel.Id);
        }
    }
}
