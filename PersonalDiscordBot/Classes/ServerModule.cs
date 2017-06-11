using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QueryMaster;
using Discord.Commands;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Net;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Controls;
using PersonalDiscordBot.Settings;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;

namespace PersonalDiscordBot.Classes
{
    [Group("server")]
    public class ServerModule : ModuleBase
    {

        [Command("games"), Summary("Returns List of Current Servers")]
        public async Task Games()
        {
            try
            {
                StringBuilder _sb = new StringBuilder();
                _sb.AppendLine("");
                foreach (var game in MainWindow.ServerList)
                {
                    _sb.AppendLine(string.Format("```Game: {1}{0} Server Name: {2}{0} Password: {3}{0} Modded: {4}{0} Host: {5} Port: {6}```", Environment.NewLine, game.Game, game.ServerName, game.Password, game.Modded, game.ExtHostname, game.PortNum));
                }
                await Context.Channel.SendMessageAsync(_sb.ToString());
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        [Command("reboot"), Summary("Reboots the chosen server")]
        public async Task Reboot([Remainder]string gameServer)
        {
            try
            {
                List<GameServer> servsFound = new List<GameServer>();
                foreach (var game in MainWindow.ServerList)
                {
                    if (game.Game.ToLower() == gameServer.ToLower())
                    {
                        servsFound.Add(game);
                    }
                }
                if (servsFound.Count == 0)
                {
                    Toolbox.uDebugAddLog($"No servers were found, servCount: {servsFound.Count}");
                    await Context.Channel.SendMessageAsync(string.Format("There currently aren't any game servers in the server list running the game {0}", gameServer));
                    return;
                }

                #region If More Than 1 Result
                else if (servsFound.Count > 1)
                {
                    Toolbox.uDebugAddLog($"Multiple servers found, servCount: {servsFound.Count}");
                    int servCount = 0;
                    string verifyServ = string.Format("Multiple servers running the game {0} was found, which one would you like? (Enter the number){1}", gameServer, Environment.NewLine);
                    foreach (var serv in servsFound)
                    {
                        servCount++;
                        verifyServ = string.Format("{0} - [{1}] {2} {3}", verifyServ, servCount, serv.ServerName, Environment.NewLine);
                    }
                    var sentMsg = await Context.Channel.SendMessageAsync(verifyServ);
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    int servNumber = 0;
                    var timeNow = DateTime.Now;
                    bool respReceived = false;
                    string response = string.Empty;
                    Toolbox.uDebugAddLog("Waiting for response now");
                    while (!respReceived)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        Toolbox.uDebugAddLog("Gathering message list");
                        var newList = await Context.Channel.GetMessagesAsync(5).Flatten();
                        Toolbox.uDebugAddLog("Gathered message list");
                        foreach (IMessage msg in newList)
                        {
                            if ((Context.Message.Author == msg.Author) && (sentMsg.Timestamp.DateTime < msg.Timestamp.DateTime))
                            {
                                Toolbox.uDebugAddLog("Message found from author that has a newer DateTime than send message");
                                Toolbox.uDebugAddLog($"Before response: {msg.Content.ToString()}");
                                response = Regex.Replace(msg.Content.ToString(), @"\s+", "");
                                Toolbox.uDebugAddLog($"After response: {response}");
                                respReceived = true;
                                var answer = int.TryParse(response, out servNumber);
                                if (!(answer && servNumber <= servCount))
                                {
                                    Toolbox.uDebugAddLog($"Response wasn't valid: {response}");
                                    await Context.Channel.SendMessageAsync(string.Format("The response \"{0}\" entered wasn't valid, please try the reboot again.", response));
                                    return;
                                }
                            }
                        }
                        if (timeNow + TimeSpan.FromSeconds(60) <= DateTime.Now)
                        {
                            Toolbox.uDebugAddLog("Response wasn't recieved in the last 1 minute");
                            await Context.Channel.SendMessageAsync("An answer wasn't recieved within 1 minute, canceling reboot request.");
                            return;
                        }
                    }
                    GameServer chosenServ = servsFound[servNumber -1];
                    if (string.IsNullOrWhiteSpace(chosenServ.ServerBatchPath) || string.IsNullOrWhiteSpace(chosenServ.ServerProcName) || string.IsNullOrWhiteSpace(chosenServ.ServerExe))
                    {
                        await Context.Channel.SendMessageAsync(string.Format("The game server {0} isn't setup for reboot functionality, please ask the server admin to add the exectuable/batch paths and process name to allow this functionality.", chosenServ.ServerName));
                        return;
                    }
                    if (Toolbox.serversRebooted.Count > 0)
                    {
                        foreach (var rebSrv in Toolbox.serversRebooted)
                        {
                            if (rebSrv.Server.ServerName == chosenServ.ServerName)
                            {
                                TimeSpan timeRebooted = DateTime.Now.ToLocalTime() - rebSrv.Rebooted;
                                var timeLeft = 15 - timeRebooted.Minutes;
                                await Context.Channel.SendMessageAsync(string.Format("The game server {0} was rebooted {1}min ago, please wait another {2}min before attempting to reboot again", rebSrv.Server.ServerName, timeRebooted.Minutes, timeLeft));
                                return;
                            }
                        }
                    }
                    string procName = chosenServ.ServerProcName;
                    if (procName.ToLower().EndsWith(".exe"))
                        procName = procName.ToLower().Replace(".exe", "");
                    int procsKilled = 0;
                    Process[] foundProcs = Process.GetProcessesByName(procName);
                    string sendResp = string.Empty;
                    foreach (var proc in foundProcs)
                    {
                        if (chosenServ.ServerExe == proc.MainModule.FileName)
                        {
                            procsKilled++;
                            proc.Kill();
                            await Context.Channel.SendMessageAsync(string.Format("Successfully killed the game server {0}, now attempting to start the server...", chosenServ.ServerName));
                        }
                    }
                    if (procsKilled == 0)
                        await Context.Channel.SendMessageAsync("I wasn't able to find a running process with the same start path as the game server, skipping process kill");
                    Process newProc = new Process();
                    newProc.StartInfo.FileName = chosenServ.ServerBatchPath;
                    newProc.Start();
                    RebootedServer rebServ = new RebootedServer { Rebooted = DateTime.Now.ToLocalTime(), Server = chosenServ };
                    Toolbox.serversRebooted.Add(rebServ);
                    Toolbox.RemoveRebootedServer(rebServ);
                    await Context.Channel.SendMessageAsync(string.Format("Successfully started the game server {0}, please wait for the server to boot up. I will check on the status automatically and let you know what I find, otherwise you can manually check the status by using the command ;server status {1}", chosenServ.ServerName, chosenServ.Game));
                    await CheckOnServer(chosenServ);
                }
                #endregion

                #region If 1 Result
                else
                {
                    GameServer chosenServ = servsFound[0];
                    if (string.IsNullOrWhiteSpace(chosenServ.ServerBatchPath) || string.IsNullOrWhiteSpace(chosenServ.ServerProcName) || string.IsNullOrWhiteSpace(chosenServ.ServerExe))
                    {
                        await Context.Channel.SendMessageAsync(string.Format("The game server {0} isn't setup for reboot functionality, please ask the server admin to add the exectuable/batch paths and process name to allow this functionality.", chosenServ.ServerName));
                        return;
                    }
                    if (Toolbox.serversRebooted.Count > 0)
                    {
                        foreach (var rebSrv in Toolbox.serversRebooted)
                        {
                            if (rebSrv.Server.ServerName == chosenServ.ServerName)
                            {
                                TimeSpan timeLeft = DateTime.Now.ToLocalTime() - rebSrv.Rebooted;
                                await Context.Channel.SendMessageAsync(string.Format("The game server {0} was rebooted {1}min ago, please wait another {1}min before attempting to reboot again", rebSrv.Server.ServerName, timeLeft.Minutes));
                                return;
                            }
                        }
                    }
                    string procName = chosenServ.ServerProcName;
                    if (procName.ToLower().EndsWith(".exe"))
                        procName = procName.ToLower().Replace(".exe", "");
                    int procsKilled = 0;
                    Process[] foundProcs = Process.GetProcessesByName(procName);
                    string sendResp = string.Empty;
                    foreach (var proc in foundProcs)
                    {
                        if (chosenServ.ServerExe == proc.MainModule.FileName)
                        {
                            procsKilled++;
                            proc.Kill();
                            await Context.Channel.SendMessageAsync(string.Format("Successfully killed the game server {0}, now attempting to start the server...", chosenServ.ServerName));
                        }
                    }
                    if (procsKilled == 0)
                        await Context.Channel.SendMessageAsync("I wasn't able to find a running process with the same start path as the game server, skipping process kill");
                    Process newProc = new Process();
                    newProc.StartInfo.FileName = chosenServ.ServerBatchPath;
                    newProc.Start();
                    RebootedServer rebServ = new RebootedServer { Rebooted = DateTime.Now.ToLocalTime(), Server = chosenServ };
                    Toolbox.serversRebooted.Add(rebServ);
                    Toolbox.RemoveRebootedServer(rebServ);
                    await Context.Channel.SendMessageAsync(string.Format("Successfully started the game server {0}, please wait for the server to boot up. I will check on the status automatically and let you know what I find, otherwise you can manually check the status by using the command ;server status {1}", chosenServ.ServerName, chosenServ.Game));
                    await CheckOnServer(chosenServ);
                } 
                #endregion
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        [Command("logs"), Summary("Get's the game logs from the chosen game server")]
        public async Task GetLogs([Remainder]string gameServer)
        {
            try
            {
                List<GameServer> serversFound = new List<GameServer>();
                foreach (GameServer game in MainWindow.ServerList)
                {
                    if (game.Game.ToLower() == gameServer.ToLower())
                    {
                        serversFound.Add(game);
                    }
                }
                if (serversFound.Count == 0)
                {
                    Toolbox.uDebugAddLog($"No servers found, servCount: {serversFound.Count}");
                    await Context.Channel.SendMessageAsync(string.Format("There currently aren't any game servers in the server list matching {0}", gameServer));
                }
                else if (serversFound.Count == 1)
                {
                    Toolbox.uDebugAddLog($"One server found, servCount: {serversFound.Count}");
                    GameServer game = serversFound[0];
                    if (string.IsNullOrWhiteSpace(game.ServerLogPath))
                    {
                        await Context.Channel.SendMessageAsync(string.Format("The game server {0} isn't setup to pull logs, please ask the server admin to enable this functionality by entering a server log location", game.ServerName));
                        return;
                    }
                    string logPath = string.Format(@"{0}", game.ServerLogPath);
                    if (!Directory.Exists(logPath))
                    {
                        await Context.Channel.SendMessageAsync(string.Format("The log location that is setup for server {0} wasn't found, please ask the server admin to enter the correct log location", game.ServerName));
                        return;
                    }
                    List<FileInfo> list = new List<FileInfo>();
                    list = GetServerLogs(logPath);
                    foreach (FileInfo file in list)
                    {
                        await Context.Channel.SendFileAsync(file.FullName);
                    }
                    await Context.Channel.SendMessageAsync(string.Format("Here are the latest log files from the {0} server, go nuts", game.ServerName));
                }
                else
                {
                    Toolbox.uDebugAddLog($"Multiple servers found, servCount: {serversFound.Count}");
                    int servCount = 0;
                    string verifyServ = string.Format("Multiple servers running the game {0} was found, which one would you like? (Enter the number){1}", gameServer, Environment.NewLine);
                    foreach (var serv in serversFound)
                    {
                        servCount++;
                        verifyServ = string.Format("{0} - [{1}] {2} {3}", verifyServ, servCount, serv.ServerName, Environment.NewLine);
                    }
                    var sentMsg = await Context.Channel.SendMessageAsync(verifyServ);
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    var timeNow = DateTime.Now;
                    int servNumber = 0;
                    bool respReceived = false;
                    string response = string.Empty;
                    while (!respReceived)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        Toolbox.uDebugAddLog("Gathering message list");
                        var newList = await Context.Channel.GetMessagesAsync(5).Flatten();
                        Toolbox.uDebugAddLog("Gathered message list");
                        foreach (IMessage msg in newList)
                        {
                            if ((Context.Message.Author == msg.Author) && (sentMsg.Timestamp.DateTime < msg.Timestamp.DateTime))
                            {
                                Toolbox.uDebugAddLog("Message from same author found newer than original message");
                                Toolbox.uDebugAddLog($"Before response: {msg.Content.ToString()}");
                                response = Regex.Replace(msg.Content.ToString(), @"\s+", "");
                                Toolbox.uDebugAddLog($"After response: {response}");
                                respReceived = true;
                                var answer = int.TryParse(response, out servNumber);
                                if (!(answer && servNumber <= servCount))
                                {
                                    Toolbox.uDebugAddLog($"Response was invalid: {response}");
                                    await Context.Channel.SendMessageAsync(string.Format("The response \"{0}\" entered wasn't valid, please try the reboot again.", response));
                                    return;
                                }
                            }
                        }
                        if (timeNow + TimeSpan.FromSeconds(60) <= DateTime.Now)
                        {
                            Toolbox.uDebugAddLog("Response wasn't received within 60 seconds, canceling");
                            await Context.Channel.SendMessageAsync("An answer wasn't recieved within 1 minute, canceling log request.");
                            return;
                        }
                    }
                    GameServer game = serversFound[servNumber - 1];
                    if (string.IsNullOrWhiteSpace(game.ServerLogPath))
                    {
                        await Context.Channel.SendMessageAsync(string.Format("The game server {0} isn't setup to pull logs, please ask the server admin to enable this functionality by entering a server log location", game.ServerName));
                        return;
                    }
                    string logPath = string.Format(@"{0}", game.ServerLogPath);
                    if (!Directory.Exists(logPath))
                    {
                        await Context.Channel.SendMessageAsync(string.Format("The log location that is setup for server {0} wasn't found, please ask the server admin to enter the correct log location", game.ServerName));
                        return;
                    }
                    List<FileInfo> list = new List<FileInfo>();
                    list = GetServerLogs(logPath);
                    foreach (FileInfo file in list)
                    {
                        await Context.Channel.SendFileAsync(file.FullName);
                    }
                    await Context.Channel.SendMessageAsync(string.Format("Here are the latest log files from the {0} server, go nuts", game.ServerName));
                }
            }
            catch (Exception ex)
            {
                if (ex.HResult == -2147024864)
                {
                    await Context.Channel.SendMessageAsync("The log file is currently in use and is unable to be retreived");
                    return;
                }
                FullExceptionLog(ex);
            }

        }
        
        [Command("logs"), Summary("Get's the game logs from the chosen game server")]
        public async Task GetLogs(string count, [Remainder]string gameServer)
        {
            try
            {
                List<GameServer> serversFound = new List<GameServer>();
                var isInt = int.TryParse(count, out int logCount);
                if (!isInt)
                {
                    await Context.Channel.SendMessageAsync(string.Format("{0} isn't a valid number, please try again", count));
                    return;
                }
                foreach (GameServer game in MainWindow.ServerList)
                {
                    if (game.Game.ToLower() == gameServer.ToLower())
                    {
                        serversFound.Add(game);
                    }
                }
                if (serversFound.Count == 0)
                {
                    await Context.Channel.SendMessageAsync(string.Format("There currently aren't any game servers in the server list matching {0}", gameServer));
                }
                else if (serversFound.Count == 1)
                {
                    GameServer game = serversFound[0];
                    if (string.IsNullOrWhiteSpace(game.ServerLogPath))
                    {
                        await Context.Channel.SendMessageAsync(string.Format("The game server {0} isn't setup to pull logs, please ask the server admin to enable this functionality by entering a server log location", game.ServerName));
                        return;
                    }
                    string logPath = string.Format(@"{0}", game.ServerLogPath);
                    if (!Directory.Exists(logPath))
                    {
                        await Context.Channel.SendMessageAsync(string.Format("The log location that is setup for server {0} wasn't found, please ask the server admin to enter the correct log location", game.ServerName));
                        return;
                    }
                    List<FileInfo> list = new List<FileInfo>();
                    list = GetServerLogs(logPath, logCount);
                    foreach (FileInfo file in list)
                    {
                        await Context.Channel.SendFileAsync(file.FullName);
                    }
                    await Context.Channel.SendMessageAsync(string.Format("Here are the latest log files from the {0} server, go nuts", game.ServerName));
                }
                else
                {
                    Toolbox.uDebugAddLog($"Multiple servers found, servCount: {serversFound.Count}");
                    int servCount = 0;
                    string verifyServ = string.Format("Multiple servers running the game {0} was found, which one would you like? (Enter the number){1}", gameServer, Environment.NewLine);
                    foreach (var serv in serversFound)
                    {
                        servCount++;
                        verifyServ = string.Format("{0} - [{1}] {2} {3}", verifyServ, servCount, serv.ServerName, Environment.NewLine);
                    }
                    var sentMsg = await Context.Channel.SendMessageAsync(verifyServ);
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    var timeNow = DateTime.Now;
                    int servNumber = 0;
                    bool respReceived = false;
                    string response = string.Empty;
                    while (!respReceived)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        Toolbox.uDebugAddLog("Gathering message list");
                        var newList = await Context.Channel.GetMessagesAsync(5).Flatten();
                        Toolbox.uDebugAddLog("Gathered message list");
                        foreach (IMessage msg in newList)
                        {
                            if ((Context.Message.Author == msg.Author) && (sentMsg.Timestamp.DateTime < msg.Timestamp.DateTime))
                            {
                                Toolbox.uDebugAddLog("Message from same author found newer than original message");
                                Toolbox.uDebugAddLog($"Before response: {msg.Content.ToString()}");
                                response = Regex.Replace(msg.Content.ToString(), @"\s+", "");
                                Toolbox.uDebugAddLog($"After response: {response}");
                                respReceived = true;
                                var answer = int.TryParse(response, out servNumber);
                                if (!(answer && servNumber <= servCount))
                                {
                                    Toolbox.uDebugAddLog($"Response was invalid: {response}");
                                    await Context.Channel.SendMessageAsync(string.Format("The response \"{0}\" entered wasn't valid, please try the reboot again.", response));
                                    return;
                                }
                            }
                        }
                        if (timeNow + TimeSpan.FromSeconds(60) <= DateTime.Now)
                        {
                            Toolbox.uDebugAddLog("Response wasn't received within 60 seconds, canceling");
                            await Context.Channel.SendMessageAsync("An answer wasn't recieved within 1 minute, canceling log request.");
                            return;
                        }
                    }
                    GameServer game = serversFound[servNumber - 1];
                    if (string.IsNullOrWhiteSpace(game.ServerLogPath))
                    {
                        await Context.Channel.SendMessageAsync(string.Format("The game server {0} isn't setup to pull logs, please ask the server admin to enable this functionality by entering a server log location", game.ServerName));
                        return;
                    }
                    string logPath = string.Format(@"{0}", game.ServerLogPath);
                    if (!Directory.Exists(logPath))
                    {
                        await Context.Channel.SendMessageAsync(string.Format("The log location that is setup for server {0} wasn't found, please ask the server admin to enter the correct log location", game.ServerName));
                        return;
                    }
                    List<FileInfo> list = new List<FileInfo>();
                    list = GetServerLogs(logPath, logCount);
                    foreach (FileInfo file in list)
                    {
                        await Context.Channel.SendFileAsync(file.FullName);
                    }
                    await Context.Channel.SendMessageAsync(string.Format("Here are the latest log files from the {0} server, go nuts", game.ServerName));
                }
            }
            catch (Exception ex)
            {
                if (ex.HResult == -2147024864)
                {
                    await Context.Channel.SendMessageAsync("The log file is currently in use and is unable to be retreived");
                    return;
                }
                FullExceptionLog(ex);
            }

        }

        [Command("status"), Summary("Get's the server status")]
        public async Task GetServerStatus([Remainder]string gameServer)
        {
            GameServer cacheGame = null;
            List<GameServer> serversFound = new List<GameServer>();
            try
            {
                foreach (GameServer game in MainWindow.ServerList)
                {
                    if (game.Game.ToLower() == gameServer.ToLower())
                    {
                        serversFound.Add(game);
                    }
                }
                if (serversFound.Count == 0)
                {
                    Toolbox.uDebugAddLog($";server status {gameServer}: found {serversFound.Count} games");
                    await Context.Channel.SendMessageAsync(string.Format("There currently aren't any game servers in the server list matching {0}", gameServer));
                    return;
                }
                else if (serversFound.Count == 1)
                {
                    Toolbox.uDebugAddLog($";server status {gameServer}: found {serversFound.Count} game");
                    GameServer game = serversFound[0];
                    cacheGame = game;
                    string response = string.Format("_{0}{1} Server Status:{0}", Environment.NewLine, game.Game);
                    IPEndPoint endpoint = CreateIPEndPoint(string.Format("{0}:{1}", game.IPAddress, game.QueryPort));
                    QueryMaster.ServerInfo servInfo = GetServerInfo(endpoint, out ReadOnlyCollection<Player> playerList);
                    response = string.Format("{0}Server Name: {1}{4}Map: {5}{4}Requires Password: {2}{4}Ping: {3}{4}", response, servInfo.Name, servInfo.IsPrivate, servInfo.Ping, Environment.NewLine, servInfo.Map);
                    Ping png = new Ping();
                    PingReply rep = png.Send(game.IPAddress);
                    bool isOnline = rep.Status == IPStatus.Success ? true : false;
                    response = string.Format("{0}Is Online: {1}{2}", response, isOnline, Environment.NewLine);
                    bool isConnectable = servInfo == null ? false : true;
                    response = string.Format("{0}Is Connectable: {1}{2}", response, isConnectable, Environment.NewLine);
                    response = string.Format("{0}Players: {1}/{2}{3}", response, playerList.Count, servInfo.MaxPlayers, Environment.NewLine);
                    await Context.Channel.SendMessageAsync(response);
                }
                else
                {
                    Toolbox.uDebugAddLog($";server status {gameServer}: found {serversFound.Count} games");
                    int servCount = 0;
                    string verifyServ = string.Format("Multiple servers running the game {0} was found, which one would you like? (Enter the number){1}", gameServer, Environment.NewLine);
                    foreach (var serv in serversFound)
                    {
                        servCount++;
                        verifyServ = string.Format("{0} - [{1}] {2} {3}", verifyServ, servCount, serv.ServerName, Environment.NewLine);
                    }
                    var sentMsg = await Context.Channel.SendMessageAsync(verifyServ);
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    int servNumber = 0;
                    bool respReceived = false;
                    string response = string.Empty;
                    var timeNow = DateTime.Now;
                    Toolbox.uDebugAddLog("Waiting for response on server number");
                    while (!respReceived)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        Toolbox.uDebugAddLog("Generating message list");
                        var newList = await Context.Channel.GetMessagesAsync(5).Flatten();
                        Toolbox.uDebugAddLog("Generated message list");
                        foreach (IMessage msg in newList)
                        {
                            if ((Context.Message.Author == msg.Author) && (sentMsg.Timestamp.DateTime < msg.Timestamp.DateTime))
                            {
                                Toolbox.uDebugAddLog("Found message from OP with a newer DateTime than the original message");
                                Toolbox.uDebugAddLog($"Before response: {msg.Content.ToString()}");
                                response = Regex.Replace(msg.Content.ToString(), @"\s+", "");
                                Toolbox.uDebugAddLog($"After response: {response}");
                                respReceived = true;
                                var answer = int.TryParse(response, out servNumber);
                                if (!(answer && servNumber <= servCount))
                                {
                                    await Context.Channel.SendMessageAsync(string.Format("The response \"{0}\" entered wasn't valid, please try the reboot again.", response));
                                    Toolbox.uDebugAddLog($"Answer was invalid: {answer}, servCount: {servCount}, servNum: {servNumber}");
                                    return;
                                }
                            }
                        }
                        if (timeNow + TimeSpan.FromSeconds(60) <= DateTime.Now)
                        {
                            await Context.Channel.SendMessageAsync("An answer wasn't recieved within 1 minute, canceling status request.");
                            Toolbox.uDebugAddLog("Waited 60 seconds, no answer received");
                            return;
                        }
                    }
                    Toolbox.uDebugAddLog($"Answer recieved, rebooting server {servNumber} for {gameServer}");
                    GameServer game = serversFound[servNumber - 1];
                    cacheGame = game;
                    string responseStatus = string.Format("_{0}{1} Server Status:{0}", Environment.NewLine, game.Game);
                    IPEndPoint endpoint = CreateIPEndPoint(string.Format("{0}:{1}", game.IPAddress, game.QueryPort));
                    QueryMaster.ServerInfo servInfo = GetServerInfo(endpoint, out ReadOnlyCollection<Player> playerList);
                    responseStatus = string.Format("{0}Server Name: {1}{4}Map: {5}{4}Requires Password: {2}{4}Ping: {3}{4}", responseStatus, servInfo.Name, servInfo.IsPrivate, servInfo.Ping, Environment.NewLine, servInfo.Map);
                    Ping png = new Ping();
                    PingReply rep = png.Send(game.IPAddress);
                    bool isOnline = rep.Status == IPStatus.Success ? true : false;
                    responseStatus = string.Format("{0}Is Online: {1}{2}", responseStatus, isOnline, Environment.NewLine);
                    bool isConnectable = servInfo == null ? false : true;
                    responseStatus = string.Format("{0}Is Connectable: {1}{2}", responseStatus, isConnectable, Environment.NewLine);
                    responseStatus = string.Format("{0}Players: {1}/{2}{3}", responseStatus, playerList.Count, servInfo.MaxPlayers, Environment.NewLine);
                    await Context.Channel.SendMessageAsync(responseStatus);
                }
            }
            catch (Exception ex)
            {
                if (ex.HResult == -2147467259)
                {

                    try
                    {
                        string response = string.Format("_{0}{1} Server Status:{0}", Environment.NewLine, cacheGame.Game);
                        IPEndPoint endpoint = CreateIPEndPoint(string.Format("{0}:{1}", cacheGame.IPAddress, cacheGame.QueryPort));
                        bool requiresPass = string.IsNullOrWhiteSpace(cacheGame.Password) ? false : true;
                        Ping png = new Ping();
                        PingReply rep = png.Send(cacheGame.IPAddress);
                        bool isOnline = rep.Status == IPStatus.Success ? true : false;
                        dynamic ping = "N/A";
                        if (isOnline) ping = rep.RoundtripTime;
                        response = string.Format("{0}Server Name: {1}{4}Map: {5}{4}Requires Password: {2}{4}Ping: {3}{4}", response, cacheGame.ServerName, requiresPass, ping, Environment.NewLine, "N/A");
                        response = string.Format("{0}Is Online: {1}{2}", response, isOnline, Environment.NewLine);
                        bool isConnectable = false;
                        response = string.Format("{0}Is Connectable: {1}{2}", response, isConnectable, Environment.NewLine);
                        response = string.Format("{0}Players: {1}", response, "N/A");
                        await Context.Channel.SendMessageAsync(response);
                        return;
                    }
                    catch (Exception ex1)
                    {
                        FullExceptionLog(ex1);
                    }
                }
                if (ex.HResult == -2147467259)
                {
                    await Context.Channel.SendMessageAsync("The IP Address entered for this server isn't valid, please update the IP address");
                    return;
                }
                FullExceptionLog(ex);
            }
        }

        [Command("players"), Summary("Get's the names of players on the chosen server")]
        public async Task GetPlayerInfo([Remainder]string gameServer)
        {
            GameServer gameCache = null;
            try
            {
                List<GameServer> serversFound = new List<GameServer>();
                foreach (GameServer game in MainWindow.ServerList)
                {
                    if (game.Game.ToLower() == gameServer.ToLower())
                    {
                        serversFound.Add(game);
                    }
                }
                if (serversFound.Count == 0)
                {
                    await Context.Channel.SendMessageAsync(string.Format("There currently aren't any game servers in the server list matching {0}", gameServer));
                    return;
                }
                else if (serversFound.Count == 1)
                {
                    GameServer game = serversFound[0];
                    gameCache = game;
                    string response = string.Format("_{0}Players found on {1}:{0}", Environment.NewLine, game.ServerName);
                    IPEndPoint endpoint = CreateIPEndPoint(string.Format("{0}:{1}", game.IPAddress, game.QueryPort));
                    QueryMaster.ServerInfo servInfo = GetServerInfo(endpoint, out ReadOnlyCollection<Player> playerList);
                    if (playerList.Count == 0) response = string.Format("No players are currently on {0}", game.ServerName);
                    else
                        foreach (var player in playerList)
                        {
                            response = string.Format("{0}```Name: {1}{7}Score: {2}{7}Time Connected: {3}D {4}H {5}M {6}S```", response, player.Name, player.Score.ToString(), player.Time.Days, player.Time.Hours, player.Time.Minutes, player.Time.Seconds, Environment.NewLine);
                        }
                    await Context.Channel.SendMessageAsync(response);
                }
                else
                {
                    int servCount = 0;
                    string verifyServ = string.Format("Multiple servers running the game {0} was found, which one would you like? (Enter the number){1}", gameServer, Environment.NewLine);
                    foreach (var serv in serversFound)
                    {
                        servCount++;
                        verifyServ = string.Format("{0} - [{1}] {2} {3}", verifyServ, servCount, serv.ServerName, Environment.NewLine);
                    }
                    var sentMsg = await Context.Channel.SendMessageAsync(verifyServ);
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    int waited = 0;
                    int servNumber = 0;
                    bool respReceived = false;
                    string response = string.Empty;
                    while (!respReceived)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        var newList = await Context.Channel.GetMessagesAsync(5).Flatten();
                        foreach (IMessage msg in newList)
                        {
                            if (Context.Message.Author == msg.Author && sentMsg.Timestamp < msg.Timestamp)
                            {
                                response = Regex.Replace(msg.Content.ToString(), @"\s+", "");
                                respReceived = true;
                                var answer = int.TryParse(response, out servNumber);
                                if (!(answer && servNumber <= servCount))
                                {
                                    await Context.Channel.SendMessageAsync(string.Format("The response \"{0}\" entered wasn't valid, please try the reboot again.", response));
                                    return;
                                }
                            }
                        }
                        if (waited >= 60)
                        {
                            await Context.Channel.SendMessageAsync("An answer wasn't recieved within 1 minute, canceling reboot request.");
                            return;
                        }
                        waited++;
                    }
                    GameServer game = serversFound[servNumber];
                    gameCache = game;
                    string responsePlayers = string.Format("_{0}Players found on {1}:{0}", Environment.NewLine, game.ServerName);
                    IPEndPoint endpoint = CreateIPEndPoint(string.Format("{0}:{1}", game.IPAddress, game.QueryPort));
                    QueryMaster.ServerInfo servInfo = GetServerInfo(endpoint, out ReadOnlyCollection<Player> playerList);
                    if (playerList.Count == 0) responsePlayers = string.Format("No players are currently on {0}", game.ServerName);
                    else
                        foreach (var player in playerList)
                        {
                            responsePlayers = string.Format("{0}```Name: {1}{7}Score: {2}{7}Time Connected: {3}D {4}H {5}M {6}S```", responsePlayers, player.Name, player.Score.ToString(), player.Time.Days, player.Time.Hours, player.Time.Minutes, player.Time.Seconds, Environment.NewLine);
                        }
                    await Context.Channel.SendMessageAsync(responsePlayers);
                }
            }
            catch (Exception ex)
            {
                if (ex.HResult == -2147467259)
                {
                    try
                    {
                        string response = string.Format("_{0}The server {1} isn't currently available{0}", Environment.NewLine, gameCache.ServerName);
                        await Context.Channel.SendMessageAsync(response);
                        return;
                    }
                    catch (Exception ex1)
                    {
                        FullExceptionLog(ex1);
                    }
                }
                if (ex.HResult == -2147467259)
                {
                    await Context.Channel.SendMessageAsync("The IP Address entered for this server isn't valid, please update the IP address");
                    return;
                }
                FullExceptionLog(ex);
            }
        }

        private async Task CheckOnServer(GameServer game)
        {
            try
            {
                TimeSpan totalTime = TimeSpan.FromMinutes(15);
                IPEndPoint endpoint = CreateIPEndPoint(string.Format("{0}:{1}", game.IPAddress, game.QueryPort));
                QueryMaster.ServerInfo servInfo = GetServerInfo(endpoint, out ReadOnlyCollection<Player> playerList);
                while (servInfo == null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(30));
                    Toolbox.uDebugAddLog(string.Format("Waited 30 seconds after rebooting the {0} game server", game.ServerName));
                    totalTime = totalTime - TimeSpan.FromSeconds(30);
                    if (totalTime <= TimeSpan.FromSeconds(0))
                    {
                        await Context.Channel.SendMessageAsync(string.Format("I tried checking on the {0} server for you but it never came up or I can't connect to it for some reason, its been 15min so I'm gonna stop checking...", game.ServerName));
                        Toolbox.uDebugAddLog(string.Format("Waited the full 15 min after rebooting the {0} game server without connectivity", game.ServerName));
                        return;
                    }
                }
                await Context.Channel.SendMessageAsync(string.Format("The server {0} is now up and running", game.ServerName));
                Toolbox.uDebugAddLog(string.Format("Successfully alerted when the {0} game server came back up, took {1} min", game.ServerName, totalTime.Minutes));
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        public static IPEndPoint CreateIPEndPoint(string endPoint)
        {
            string[] ep = endPoint.Split(':');
            if (ep.Length != 2) throw new FormatException("Invalid endpoint format");
            if (!IPAddress.TryParse(ep[0], out IPAddress ip))
            {
                throw new FormatException("Invalid ip-adress");
            }
            if (!int.TryParse(ep[1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out int port))
            {
                throw new FormatException("Invalid port");
            }
            return new IPEndPoint(ip, port);
        }

        private static QueryMaster.ServerInfo GetServerInfo(IPEndPoint endpoint, out ReadOnlyCollection<QueryMaster.Player> players)
        {
            players = null;

            QueryMaster.ServerInfo serverInfo = null;
            using (var server = ServerQuery.GetServerInstance(EngineType.Source, endpoint))
            {
                serverInfo = server.GetInfo();
                players = server.GetPlayers();
            }
            if (players != null)
                players = new ReadOnlyCollection<QueryMaster.Player>(players.Where(record => !string.IsNullOrWhiteSpace(record.Name)).ToList());

            return serverInfo;
        }

        private List<FileInfo> GetServerLogs(string location, int logCount = 3)
        {
            List<FileInfo> _returnList = new List<FileInfo>();

            DirectoryInfo _dI = new DirectoryInfo(location);
            foreach (FileInfo _fI in _dI.GetFiles())
            {
                if (_fI.Extension.ToLower() == ".txt" || _fI.Extension.ToLower() == ".log")
                    _returnList.Add(_fI);
            }
            
            return _returnList.OrderByDescending(x => x.CreationTime).ToList().Take(logCount).ToList();
        }

        public static void FullExceptionLog(Exception ex, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null, [CallerFilePath] string filePath = null)
        {
            string exString = string.Format("TimeStamp: {1}{0}Exception Type: {2}{0}Caller: {3} at {4}{0}Message: {5}{0}HR: {6}{0}StackTrace:{0}{7}{0}", Environment.NewLine, DateTime.Now.ToLocalTime().ToShortTimeString(), ex.GetType().Name, caller, lineNumber, ex.Message, ex.HResult, ex.StackTrace);
            Toolbox.uDebugAddLog(string.Format("EXCEPTION: {0} at {1}", caller, lineNumber));
            string _logLocation = string.Format(@"{0}\Exceptions.log", MainWindow._paths.LogLocation);
            if (!File.Exists(_logLocation))
                using (StreamWriter _sw = new StreamWriter(_logLocation))
                    _sw.WriteLine(exString + Environment.NewLine);
            else
                using (StreamWriter _sw = File.AppendText(_logLocation))
                    _sw.WriteLine(exString + Environment.NewLine);
        }
    }

    [Group("help"), Summary("Returns current help articles")]
    public class HelpModule : ModuleBase
    {
        [Command(""), Summary("Default Entry")]
        public async Task DefHelp()
        {
            try
            {
                string _helpArticle = string.Format
                (
                 "_{0}" +
                 "{0}```۩ General Commands ۩```{0}" +
                 "```;help{0}You already did this command, don't do it again unless you're a pleb or something...```" +
                 "```;status{0}Displays information about the bots' current status/info```" +
                 "```;playing{0}Sets the bots' Playing status to nothing (not playing anything)```" +
                 "```;playing a game{0}Sets the bots' Playing status to 'a game' or whatever you type after playing```" +
                 "```;name{0}Sets the bots' Name status to 'My Boiiiiiiiiiiiii'```" +
                 "```;name Raspberry Schmeckles{0}Sets the bots' Name status to 'Raspberry Schmeckles' or whatever you type after name```" +
                 "```;f @Someone{0}Pays respects to a mentioned person / or whatever you want```" +
                 "{0}```▧ Server Commands ▨```{0}" +
                 "```;server games{0}Displays current group servers and information```" +
                 "```;server reboot %GameName%{0}Reboot's the the requested server```" +
                 "```;server logs %GameName%{0}Get's the latest 3 server logs on the requested server```" +
                 "```;server logs 8 %GameName%{0}Get's the latest 8 server logs on the requested server, or however many you want, replace the 8 with a different number```" +
                 "```;server status %GameName%{0}Get's the current server status and info (players, online, connectable, ect) on the requested server```" +
                 "```;server players %GameName%{0}Show's all players and info for the requested server```" +
                 "{0}```╬ Translate Commands ╬```{0}" +
                 "```;snoop{0}Turns whatever you say into a snoopified sentence of glory```" +
                 "```;snoop dogg{0}Toggles every message being snoopified```" +
                 "```;leet{0}Turns whatever you say into a string that of a 1337 hackaman```" +
                 "```;leet 100{0}Does the same thing as leet but the number you put in front of your sentence defines what level of leet you want the sentence to be translated to```",
                 Environment.NewLine
                );
                await Context.Channel.SendMessageAsync(_helpArticle);
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }
    }

    [Group("snoop"), Summary("Snoopifies yo text")]
    public class SnoopModule : ModuleBase
    {
        [Command("dogg"), Summary("Toggles all messages being translated")]
        public async Task SnoopToggle()
        {
            if (sGeneral.Default.Snooping) sGeneral.Default.Snooping = false;
            else sGeneral.Default.Snooping = true;
            sGeneral.Default.Save();
            await Context.Channel.SendMessageAsync(string.Format("Snooping all messages in this channel: {0}", sGeneral.Default.Snooping));
            return;
        }

        [Command(""), Summary("Default Entry")]
        public async Task Snoopify([Remainder]string message)
        {
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync(string.Format("{0}{1}{2}", Context.User.Mention, Environment.NewLine, message.ToSnoopification()));
        }
    }

    [Group("leet"), Summary("Makes your text that of a 1337 hackaman")]
    public class LeetModule : ModuleBase
    {
        [Command(""), Summary("Default Entry")]
        public async Task Leetify([Remainder]string message)
        {
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync(string.Format("{0}{1}{2}", Context.User.Mention, Environment.NewLine, message.ToLeet()));
        }

        [Command(""), Summary("Leetify Level")]
        public async Task Leetify(string level, [Remainder]string message)
        {
            int lvl = 30;
            int.TryParse(level, out lvl);
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync(string.Format("{0}{1}{2}", Context.User.Mention, Environment.NewLine, Leet.ToLeet(message, lvl)));
        }
    }

    [Group("status"), Summary("Gets the bot status/info")]
    public class StatusModule : ModuleBase
    {
        [Command(""), Summary("Default Entry")]
        public async Task GetStatus()
        {
            await Context.Channel.SendMessageAsync(string.Format("Snoopify All: {0}", sGeneral.Default.Snooping));
        }
    }

    [Group("playing"), Summary("Changes bot playing to nothing")]
    public class PlayingModule : ModuleBase
    {
        [Command(""), Summary("Default Entry")]
        public async Task SetPlaying()
        {
            await MainWindow.client.SetGameAsync(null);
            await Context.Channel.SendMessageAsync(string.Format("Set bot to playing nothing"));
        }

        [Command(""), Summary("Changes bot playing to entry")]
        public async Task SetPlaying([Remainder]string playing)
        {
            await MainWindow.client.SetGameAsync(playing);
            await Context.Channel.SendMessageAsync(string.Format("Set bot playing to: {0}", playing));
        }
    }

    [Group("name"), Summary("Changes bot name")]
    public class NameModule : ModuleBase
    {
        [Command(""), Summary("Default Entry")]
        public async Task SetName()
        {
            await MainWindow.client.CurrentUser.ModifyAsync(x => x.Username = "My Boiiiiiiiii");
            await Context.Channel.SendMessageAsync(string.Format("Set bot name to: {0}", "My Boiiiiiiiii"));
            string.Format("Set bot name to: {0}", "My Boiiiiiiiii").AddToDebugLog();
        }

        [Command(""), Summary("Specify Name")]
        public async Task SetName([Remainder]string newName)
        {
            await MainWindow.client.CurrentUser.ModifyAsync(x => x.Username = newName);
            await Context.Channel.SendMessageAsync(string.Format("Set bot name to: {0}", newName));
            string.Format("Set bot name to: {0}", newName).AddToDebugLog();
        }
    }

    [Group("f"), Summary("Pay Respects")]
    public class RespectsModule : ModuleBase
    {
        [Command(""), Summary("Pay Respects Mention")]
        public async Task MentionRespects(IUser mentionedUser)
        {
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync(string.Format("{0} has paid their respects to {1} :heart:", Context.Message.Author.Mention, mentionedUser.Mention));
        }

        [Command(""), Summary("Pay Respects String")]
        public async Task StringRespects([Remainder]string respectsTo)
        {
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync(string.Format("{0} has paid their respects to **{1}** :heart:", Context.Message.Author.Mention, respectsTo));
        }
    }

    [Group("test"), Summary("For Testing")]
    public class TestingModule : ModuleBase
    {
        [Command(""), Summary("Testicules Engage")]
        public async Task Testacules()
        {
            await Context.Channel.SendMessageAsync("No default test method is set");
        }

        [Command("weap"), Summary("Testicules Weap Gen")]
        public async Task Testacules2()
        {
            string weapName = string.Empty;
            string randGen = Testing.RandomWeap(out weapName);
            await Context.Channel.SendMessageAsync($"Generated {weapName}{randGen}");
        }

        [Command("rng"), Summary("Testicules RNG Gen")]
        public async Task Testacules3(string times)
        {
            var intTimes = 1000;
            var isANum = int.TryParse(times, out intTimes);
            if (isANum)
                await Context.Channel.SendMessageAsync($"Generated {intTimes} Weapons:{Environment.NewLine}{Testing.RandomMassTest(intTimes)}");
            else
                await Context.Channel.SendMessageAsync($"Generated {1000} Weapons:{Environment.NewLine}{Testing.RandomMassTest(1000)}");
        }
    }

    public static class Toolbox
    {
        public static StringBuilder debugLog = new StringBuilder();
        public static Classes.Paths _paths = new Classes.Paths();

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

        public static void uUpdateStatusExternal(string status)
        {
            try
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType() == typeof(MainWindow))
                    {
                        (window as MainWindow).txtStatusValue.AppendText(status);
                    }
                }
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
    }
}
