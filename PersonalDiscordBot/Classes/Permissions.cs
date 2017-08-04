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
        #region Permission Lists

        public static List<Administrator> Administrators = new List<Administrator>();
        public static List<DiscordChannel> AllowedChannels = new List<DiscordChannel>();
        public static List<DiscordUser> TestingGroups = new List<DiscordUser>();
        public static GeneralPermissions GeneralPermissions = new GeneralPermissions();

        #endregion

        #region Permission Methods

        public static void SerializePermissions()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.RunWorkerCompleted += (sender, e) => { Events.uStatusUpdateExt($"Admin Serialization Complete"); };
            worker.DoWork += (sender, e) =>
            {
                try
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
                    var json3 = JsonConvert.SerializeObject(GeneralPermissions, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    File.WriteAllText($@"{permPath}\GeneralPermissions.perm", json3);
                    Toolbox.uDebugAddLog($"Serialized GeneralPermissions.perm");
                }
                catch (Exception ex)
                {
                    Toolbox.FullExceptionLog(ex);
                }
            };
            worker.RunWorkerAsync();
        }

        public static void DeSerializePermissions()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.RunWorkerCompleted += (sender, e) => { Events.uStatusUpdateExt($"Admin Deserialization Complete"); };
            worker.DoWork += (sender, e) =>
            {
                try
                {
                    Events.uStatusUpdateExt("Deserializing Permission Data");
                    string loadPath = $@"{Directory.GetCurrentDirectory()}\Permissions";
                    Administrators.Clear();
                    AllowedChannels.Clear();
                    Toolbox.uDebugAddLog("Cleared Administrators and AllowedChannels");
                    string adminPerm = $@"{loadPath}\Administrators.perm";
                    string rpgPerm = $@"{loadPath}\RPGChannels.perm";
                    string genPerm = $@"{loadPath}\GeneralPermissions.perm";
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
                            Administrators = JsonConvert.DeserializeObject<List<Administrator>>(sr.ReadToEnd());
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
                            AllowedChannels = JsonConvert.DeserializeObject<List<DiscordChannel>>(sr.ReadToEnd());
                            Toolbox.uDebugAddLog("Deserialized RPGChannels.perm");
                        }
                    }
                    else
                        Toolbox.uDebugAddLog($"RPGChannels.perm doesn't exist: {rpgPerm}");
                    if (File.Exists(genPerm))
                    {
                        Toolbox.uDebugAddLog($"Found GeneralPermissions.perm file: {genPerm}");
                        using (StreamReader sr = File.OpenText(genPerm))
                        {
                            GeneralPermissions = JsonConvert.DeserializeObject<GeneralPermissions>(sr.ReadToEnd());
                            Toolbox.uDebugAddLog("Deserialized GeneralPermissions.perm");
                        }
                    }
                    if (Administrators == null) Administrators = new List<Administrator>();
                    if (AllowedChannels == null) AllowedChannels = new List<DiscordChannel>();
                    if (GeneralPermissions == null) GeneralPermissions = new GeneralPermissions();
                }
                catch (Exception ex)
                {
                    Toolbox.FullExceptionLog(ex);
                }
            };
            worker.RunWorkerAsync();
        }

        public static bool AdminPermissions(ICommandContext context)
        {
            var admProfile = Administrators.Find(x => x.ID == context.User.Id);
            if (admProfile != null)
            {
                try { if (string.IsNullOrWhiteSpace(admProfile.Username)) { admProfile.Username = context.User.Username; Toolbox.uDebugAddLog($"Admin profile username was null or empty, added username: {admProfile.Username} | {admProfile.ID}"); Events.UseGlblAction(Toolbox.GlobalAction.AdminChanged); } } catch (Exception ex) { Toolbox.FullExceptionLog(ex); }
                return true;
            }
            else
                return false;
        }

        public static bool RPGChannelPermission(ICommandContext context)
        {
            var rpgChannel = AllowedChannels.Find(x => x.ID == context.Channel.Id);
            if (rpgChannel != null)
                return true;
            else
                return false;
        }

        #endregion
    }

    public class GeneralPermissions
    {
        public ulong logChannel { get; set; } = 0;
    }

    public class DiscordUser
    {
        public ulong ID { get; set; }
        public string Username { get; set; }
    }

    public class DiscordChannel
    {
        public ulong ID { get; set; }
        public string Name { get; set; }
    }

    public class Administrator : DiscordUser
    {
        public DateTime Added { get; set; } = DateTime.Now.ToLocalTime();
        public string AddedBy { get; set; } = "Gui - Local";
    }
}
