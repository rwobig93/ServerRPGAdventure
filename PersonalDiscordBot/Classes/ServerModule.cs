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
                    GameServer game = serversFound[servNumber - 1];
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
            string exString = string.Format("TimeStamp: {1}{0}Exception Type: {2}{0}Caller: {3} at {4}{0}Message: {5}{0}HR: {6}{0}StackTrace:{0}{7}{0}", Environment.NewLine, $"{DateTime.Now.ToLocalTime().ToShortDateString()} {DateTime.Now.ToLocalTime().ToShortTimeString()}", ex.GetType().Name, caller, lineNumber, ex.Message, ex.HResult, ex.StackTrace);
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
                 "{0}```╬▧۩ Help Articles ۩▨╬```{0}" +
                 "```;help server{0}Shows game server commands, now you have the powah```" +
                 "```;help general{0}General commands regarding the bot or misc functions, remember I'm your buddy friend pal```" +
                 "```;help translate{0}Text/Message translation methods, leetify your snoop game yo```",
                 Environment.NewLine
                );
                await Context.Channel.SendMessageAsync(_helpArticle);
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("server"), Summary("Server Commands")]
        public async Task DefHelpServ()
        {
            try
            {
                string _helpArticle = string.Format
                (
                 "_{0}" +
                 "{0}```▧ Server Commands ▨```{0}" +
                 "```;server games{0}Displays current group servers and information```" +
                 "```;server reboot %GameName%{0}Reboot's the the requested server```" +
                 "```;server logs %GameName%{0}Get's the latest 3 server logs on the requested server```" +
                 "```;server logs 8 %GameName%{0}Get's the latest 8 server logs on the requested server, or however many you want, replace the 8 with a different number```" +
                 "```;server status %GameName%{0}Get's the current server status and info (players, online, connectable, ect) on the requested server```" +
                 "```;server players %GameName%{0}Show's all players and info for the requested server```",
                 Environment.NewLine
                );
                await Context.Channel.SendMessageAsync(_helpArticle);
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("general"), Summary("General Commands")]
        public async Task DefHelpGeneral()
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
                 "```;f @Someone{0}Pays respects to a mentioned person / or whatever you want```",
                 Environment.NewLine
                );
                await Context.Channel.SendMessageAsync(_helpArticle);
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }
        
        [Command("translate"), Summary("Translate Commands")]
        public async Task DefHelpTranslate()
        {
            try
            {
                string _helpArticle = string.Format
                (
                 "_{0}" +
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
            try
            {
                if (sGeneral.Default.Snooping) sGeneral.Default.Snooping = false;
                else sGeneral.Default.Snooping = true;
                sGeneral.Default.Save();
                await Context.Channel.SendMessageAsync(string.Format("Snooping all messages in this channel: {0}", sGeneral.Default.Snooping));
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command(""), Summary("Default Entry")]
        public async Task Snoopify([Remainder]string message)
        {
            try
            {
                await Context.Message.DeleteAsync();
                await Context.Channel.SendMessageAsync(string.Format("{0}{1}{2}", Context.User.Mention, Environment.NewLine, message.ToSnoopification()));
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }
    }

    [Group("leet"), Summary("Makes your text that of a 1337 hackaman")]
    public class LeetModule : ModuleBase
    {
        [Command(""), Summary("Default Entry")]
        public async Task Leetify([Remainder]string message)
        {
            try
            {
                await Context.Message.DeleteAsync();
                await Context.Channel.SendMessageAsync(string.Format("{0}{1}{2}", Context.User.Mention, Environment.NewLine, message.ToLeet()));
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command(""), Summary("Leetify Level")]
        public async Task Leetify(string level, [Remainder]string message)
        {
            try
            {
                int lvl = 30;
                int.TryParse(level, out lvl);
                await Context.Message.DeleteAsync();
                await Context.Channel.SendMessageAsync(string.Format("{0}{1}{2}", Context.User.Mention, Environment.NewLine, Leet.ToLeet(message, lvl)));
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }
    }

    [Group("status"), Summary("Gets the bot status/info")]
    public class StatusModule : ModuleBase
    {
        [Command(""), Summary("Default Entry")]
        public async Task GetStatus()
        {
            try
            {
                await Context.Channel.SendMessageAsync(string.Format("Snoopify All: {0}", sGeneral.Default.Snooping));
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }
    }

    [Group("playing"), Summary("Changes bot playing to nothing")]
    public class PlayingModule : ModuleBase
    {
        [Command(""), Summary("Default Entry")]
        public async Task SetPlaying()
        {
            try
            {
                await MainWindow.client.SetGameAsync(null);
                await Context.Channel.SendMessageAsync(string.Format("Set bot to playing nothing"));
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command(""), Summary("Changes bot playing to entry")]
        public async Task SetPlaying([Remainder]string playing)
        {
            try
            {
                await MainWindow.client.SetGameAsync(playing);
                await Context.Channel.SendMessageAsync(string.Format("Set bot playing to: {0}", playing));
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }
    }

    [Group("name"), Summary("Changes bot name")]
    public class NameModule : ModuleBase
    {
        [Command(""), Summary("Default Entry")]
        public async Task SetName()
        {
            try
            {
                await MainWindow.client.CurrentUser.ModifyAsync(x => x.Username = "My Boiiiiiiiii");
                await Context.Channel.SendMessageAsync(string.Format("Set bot name to: {0}", "My Boiiiiiiiii"));
                string.Format("Set bot name to: {0}", "My Boiiiiiiiii").AddToDebugLog();
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command(""), Summary("Specify Name")]
        public async Task SetName([Remainder]string newName)
        {
            try
            {
                await MainWindow.client.CurrentUser.ModifyAsync(x => x.Username = newName);
                await Context.Channel.SendMessageAsync(string.Format("Set bot name to: {0}", newName));
                string.Format("Set bot name to: {0}", newName).AddToDebugLog();
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }
    }

    [Group("f"), Summary("Pay Respects")]
    public class RespectsModule : ModuleBase
    {
        [Command(""), Summary("Pay Respects Mention")]
        public async Task MentionRespects(IUser mentionedUser)
        {
            try
            {
                await Context.Message.DeleteAsync();
                await Context.Channel.SendMessageAsync(string.Format("{0} has paid their respects to {1} :heart:", Context.Message.Author.Mention, mentionedUser.Mention));
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command(""), Summary("Pay Respects String")]
        public async Task StringRespects([Remainder]string respectsTo)
        {
            try
            {
                await Context.Message.DeleteAsync();
                await Context.Channel.SendMessageAsync(string.Format("{0} has paid their respects to **{1}** :heart:", Context.Message.Author.Mention, respectsTo));
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }
    }

    [Group("test"), Summary("For Testing")]
    public class TestingModule : ModuleBase
    {
        private static string line = Environment.NewLine;
        [Command(""), Summary("Testicules Engage")]
        public async Task Testacules()
        {
            try
            {
                await Context.Channel.SendMessageAsync("No default test method is set");
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("weap"), Summary("Testicules Weap Gen")]
        public async Task Testacules2w()
        {
            try
            {
                string weapName = string.Empty;
                string randGen = Testing.RandomWeap(out weapName);
                await Context.Channel.SendMessageAsync($"Generated {weapName}{randGen}");
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("spell"), Summary("Testicules Spell Gen")]
        public async Task Testacules2s()
        {
            try
            {
                await Context.Channel.SendMessageAsync(Testing.RandomSpell());
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("rng weap"), Summary("Testicules RNG Gen Weap")]
        public async Task Testacules3w(string times)
        {
            try
            {
                var intTimes = 1000;
                var isANum = int.TryParse(times, out intTimes);
                if (isANum)
                {
                    if (intTimes > 100000)
                    {
                        await Context.Channel.SendMessageAsync("The highest integer allowed is 100,000. I'm generating that for you now, don't do that again!");
                        await Context.Channel.SendMessageAsync($"Generated {100000} Weapons:{Environment.NewLine}{Testing.RandomMassTestWeap(1000)}");
                    }
                    else
                        await Context.Channel.SendMessageAsync($"Generated {intTimes} Weapons:{Environment.NewLine}{Testing.RandomMassTestWeap(intTimes)}");
                }
                else
                    await Context.Channel.SendMessageAsync($"Generated {1000} Weapons:{Environment.NewLine}{Testing.RandomMassTestWeap(1000)}");
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("rng spell"), Summary("Testicules RNG Gen Spell")]
        public async Task Testacules3s(string times)
        {
            try
            {
                var intTimes = 1000;
                var isANum = int.TryParse(times, out intTimes);
                if (isANum)
                {
                    if (intTimes > 100000)
                    {
                        await Context.Channel.SendMessageAsync("The highest integer allowed is 100,000. I'm generating that for you now, don't do that again!");
                        await Context.Channel.SendMessageAsync($"Generated {1000} Spells:{Environment.NewLine}{Testing.RandomMassTestSpell(100000)}");
                    }
                    else
                        await Context.Channel.SendMessageAsync($"Generated {intTimes} Spells:{Environment.NewLine}{Testing.RandomMassTestSpell(intTimes)}");
                }
                else
                    await Context.Channel.SendMessageAsync($"Generated {1000} Spells:{Environment.NewLine}{Testing.RandomMassTestSpell(1000)}");
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("lootdrop"), Summary("Testicules Lootdrop")]
        public async Task Testacules4()
        {
            try
            {
                await Context.Channel.SendMessageAsync($"```{Testing.LootDropGen()}```");
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("create"), Summary("Testicules Create")]
        public async Task Testacules5()
        {
            try
            {
                var hasCharacters = await VerifyOwnerProfileAndIfHasCharacters();
                OwnerProfile ownerProfile = RPG.Owners.Find(x => x.OwnerID == Context.Message.Author.Id);
                int cost = Management.DetermineCharacterCost(ownerProfile);
                if (hasCharacters)
                {
                    if (ownerProfile.Currency < cost)
                    {
                        await Context.Channel.SendMessageAsync($"A new character for you costs {cost} currency but you only have {ownerProfile.Currency}, please get good");
                        return;
                    }
                    var costQuestion = await Context.Channel.SendMessageAsync(
                        $"It will cost you {cost} currency to create a new character, would you still like to create a character? (Yes/No){line}" +
                        $"(You have {ownerProfile.Currency} currently)");
                    DateTime costTimeStamp = DateTime.Now;
                    bool costResponseRecvd = false;
                    while (!costResponseRecvd)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        var newList = await Context.Channel.GetMessagesAsync(5).Flatten();
                        Toolbox.uDebugAddLog("Generated message list");
                        string response = string.Empty;
                        foreach (IMessage msg in newList)
                        {
                            if ((Context.Message.Author == msg.Author) && (costQuestion.Timestamp.DateTime < msg.Timestamp.DateTime))
                            {
                                response = msg.Content.ToString();
                                Toolbox.uDebugAddLog("Found message from OP with a newer DateTime than the original message");
                                Toolbox.uDebugAddLog($"Response: {response}");
                                costResponseRecvd = true;
                            }
                        }
                        if (costTimeStamp + TimeSpan.FromSeconds(60) <= DateTime.Now)
                        {
                            Toolbox.uDebugAddLog($"Response wasn't received from {Context.Message.Author.Username} ({Context.Message.Author.Id}) within 60s, canceled character creation");
                            await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} A valid response wasn't received within 60 seconds, canceling creation request");
                            return;
                        }
                    }
                }
                var sentMsg = await Context.Channel.SendMessageAsync(
                    $"What class would you like your new adventurer to be?{line}" +
                    $"```Class: Warrior{line}Focus on Strength/Melee Damage, Higher Loot Chance: Swords, Greatswords, Katanas```" +
                    $"```Class: Dragoon{line}Focus on Dexterity/Elements, Higher Loot Chance: Spears, DragonSpears, Twinswords```" +
                    $"```Class: Mage{line}Focus on All Spells, Higher Loot Chance: Staffs, FocusStones```" +
                    $"```Class: Necromancer{line}Focus on Attack Spells/Summonding, Higher Loot Chance: FocusStones, Staffs```" +
                    $"```Class: Rogue{line}Focus on Speed/Dexterity, Higher Loot Chance: Dagger, TwinSwords```"
                    );
                bool responseRecvd = false;
                bool nameChosen = false;
                string charName = string.Empty;
                List<IMessage> respondedList = new List<IMessage>();
                DateTime timeStamp = DateTime.Now;
                RPG.CharacterClass chosenClass = 0;
                while (!responseRecvd)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    var newList = await Context.Channel.GetMessagesAsync(5).Flatten();
                    Toolbox.uDebugAddLog("Generated message list");
                    string response = string.Empty;
                    foreach (IMessage msg in newList)
                    {
                        if ((Context.Message.Author == msg.Author) && (sentMsg.Timestamp.DateTime < msg.Timestamp.DateTime) && (!respondedList.Contains(msg)))
                        {
                            respondedList.Add(msg);
                            Toolbox.uDebugAddLog("Found message from OP with a newer DateTime than the original message");
                            Toolbox.uDebugAddLog($"Before response: {msg.Content.ToString()}");
                            response = Regex.Replace(msg.Content.ToString(), @"\s+", "");
                            Toolbox.uDebugAddLog($"After response: {response}");
                            response = response.ToLower();
                            switch (response)
                            {
                                case "warrior":
                                    chosenClass = RPG.CharacterClass.Warrior;
                                    responseRecvd = true;
                                    break;
                                case "dragoon":
                                    chosenClass = RPG.CharacterClass.Dragoon;
                                    responseRecvd = true;
                                    break;
                                case "mage":
                                    chosenClass = RPG.CharacterClass.Mage;
                                    responseRecvd = true;
                                    break;
                                case "necromancer":
                                    chosenClass = RPG.CharacterClass.Necromancer;
                                    responseRecvd = true;
                                    break;
                                case "rogue":
                                    chosenClass = RPG.CharacterClass.Rogue;
                                    responseRecvd = true;
                                    break;
                                default:
                                    await Context.Channel.SendMessageAsync($"{response} isn't a valid response, please try again");
                                    break;
                            }
                        }
                    }
                    if (timeStamp + TimeSpan.FromSeconds(60) <= DateTime.Now)
                    {
                        Toolbox.uDebugAddLog($"Response wasn't received from {Context.Message.Author.Username} ({Context.Message.Author.Id}) within 60s, canceled character creation");
                        await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} A valid response wasn't received within 60 seconds, canceling creation request");
                        return;
                    }
                }
                while (!nameChosen)
                {
                    DateTime nameTimeStamp = DateTime.Now;
                    var nameQuestion = await Context.Channel.SendMessageAsync($"What can we call your {chosenClass}?");
                    bool responseRecvd2 = false;
                    DateTime timeStamp2 = DateTime.Now;
                    while (!responseRecvd2)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        var newList = await Context.Channel.GetMessagesAsync(5).Flatten();
                        Toolbox.uDebugAddLog("Generated message list");
                        string response = string.Empty;
                        foreach (IMessage msg in newList)
                        {
                            if ((Context.Message.Author == msg.Author) && (nameQuestion.Timestamp.DateTime < msg.Timestamp.DateTime) && (!respondedList.Contains(msg)))
                            {
                                respondedList.Add(msg);
                                response = msg.Content.ToString();
                                Toolbox.uDebugAddLog("Found message from OP with a newer DateTime than the original message");
                                Toolbox.uDebugAddLog($"Response: {response}");
                                charName = response;
                                responseRecvd2 = true;
                            }
                        }
                        if (timeStamp2 + TimeSpan.FromSeconds(60) <= DateTime.Now)
                        {
                            Toolbox.uDebugAddLog($"Response wasn't received from {Context.Message.Author.Username} ({Context.Message.Author.Id}) within 60s, canceled character creation");
                            await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} A valid response wasn't received within 60 seconds, canceling creation request");
                            return;
                        }
                    }
                    var verification = await Context.Channel.SendMessageAsync($"Would you like your {chosenClass} to be called \"{charName}\"? (Yes/No)");
                    bool responseRecvd3 = false;
                    DateTime timeStamp3 = DateTime.Now;
                    while (!responseRecvd3)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        var newList = await Context.Channel.GetMessagesAsync(5).Flatten();
                        Toolbox.uDebugAddLog("Generated message list");
                        string response = string.Empty;
                        foreach (IMessage msg in newList)
                        {
                            if ((Context.Message.Author == msg.Author) && (verification.Timestamp.DateTime < msg.Timestamp.DateTime) && (!respondedList.Contains(msg)))
                            {
                                respondedList.Add(msg);
                                response = msg.Content.ToString();
                                Toolbox.uDebugAddLog("Found message from OP with a newer DateTime than the original message");
                                Toolbox.uDebugAddLog($"Response: {response}");
                                switch (response.ToLower())
                                {
                                    case "yes":
                                        nameChosen = true;
                                        responseRecvd3 = true;
                                        break;
                                    case "no":
                                        nameChosen = false;
                                        responseRecvd3 = true;
                                        break;
                                    default:
                                        nameChosen = false;
                                        responseRecvd3 = false;
                                        await Context.Channel.SendMessageAsync($"{response} isn't a valid response, please try again");
                                        break;
                                }
                            }
                        }
                        if (timeStamp3 + TimeSpan.FromSeconds(60) <= DateTime.Now)
                        {
                            Toolbox.uDebugAddLog($"Response wasn't received from {Context.Message.Author.Username} ({Context.Message.Author.Id}) within 60s, canceled character creation");
                            await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} A valid response wasn't received within 60 seconds, canceling creation request");
                            return;
                        }
                    }
                    if (nameTimeStamp + TimeSpan.FromMinutes(5) <= DateTime.Now)
                    {
                        Toolbox.uDebugAddLog($"Response wasn't received from {Context.Message.Author.Username} ({Context.Message.Author.Id}) within 5min, canceled character creation");
                        await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} A valid response wasn't received within 60 seconds, canceling creation request");
                        return;
                    }
                }
                if (!hasCharacters) ownerProfile.Currency -= cost;
                await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} You have been charged {cost} currency, you now have: {ownerProfile.Currency}");
                Character newChar = Management.CreateNewCharacter(Context.Message.Author.Id, chosenClass, charName);
                ownerProfile.CharacterList.Add(newChar);
                if (ownerProfile.CharacterList.Count == 1)
                    ownerProfile.CurrentCharacter = newChar;
                await Context.Channel.SendMessageAsync($"Congratulations! Your new hero has been born:```{line}" +
                    $"Name:{newChar.Name}{line}" +
                    $"Class: {newChar.Class}{line}" +
                    $"HP: {newChar.MaxHP}{line}" +
                    $"Mana:{newChar.MaxMana}{line}" +
                    $"Defense: {newChar.Def}{line}" +
                    $"Dexterity: {newChar.Dex}{line}" +
                    $"Intelligence: {newChar.Int}{line}" +
                    $"Luck: {newChar.Lck}{line}" +
                    $"Speed: {newChar.Spd}{line}" +
                    $"Strength: {newChar.Str}{line}" +
                    $"Level: {newChar.Lvl}{line}" +
                    $"Experience: {newChar.Exp}```" +
                    $"```Weapon:{line}" +
                    $"Name: {newChar.Weapon.Name}{line}" +
                    $"Description: {newChar.Weapon.Desc}```" +
                    $"```Armor:{line}" +
                    $"Name: {newChar.Armor.Name}{line}" +
                    $"Description: {newChar.Armor.Desc}```");
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("give"), Summary("Testicules Give Currency")]
        public async Task Testacules6(string mentionedUser, string amount)
        {
            try
            {
                if (!Permissions.Administrators.Contains(Context.Message.Author.Id))
                {
                    await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} You don't have rights to run this command");
                    return;
                }
                int currency = 0;
                var isNum = int.TryParse(amount, out currency);
                if (!isNum)
                {
                    Toolbox.uDebugAddLog($"Invalid Number: {amount}");
                    await Context.Channel.SendMessageAsync($"{amount} isn't a valid number");
                    return;
                }
                Toolbox.uDebugAddLog($"Before Removing '<,@,>': {mentionedUser}");
                mentionedUser = mentionedUser.Replace("<", string.Empty).Replace("@", string.Empty).Replace(">", string.Empty);
                Toolbox.uDebugAddLog($"After Removing '<,@,>': {mentionedUser}");
                ulong userID = 0;
                var isUlong = ulong.TryParse(mentionedUser, out userID);
                if (!isUlong)
                {
                    Toolbox.uDebugAddLog($"Invalid Ulong: {userID}");
                    await Context.Channel.SendMessageAsync($"{mentionedUser} isn't a valid discord user");
                    return;
                }
                var userFound = await Context.Channel.GetUserAsync(userID);
                if (userFound == null)
                {
                    Toolbox.uDebugAddLog($"Invalid User: {userFound.Username} | {userID}");
                    await Context.Channel.SendMessageAsync($"{userID} doesn't match a discord user on your server");
                    return;
                }
                Toolbox.uDebugAddLog($"MentionedUser: {mentionedUser}");
                int foundUsers = 0;
                foreach (var owner in RPG.Owners)
                {
                    if (userFound.Id == owner.OwnerID)
                    {
                        owner.Currency += currency;
                        Events.uStatusUpdateExt($"Added {currency} currency to {userFound.Username} | {userFound.Id}");
                        await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} Added {currency} currency to {userFound.Mention}'s profile");
                        foundUsers++;
                    }
                }
                if (foundUsers == 0)
                {
                    Toolbox.uDebugAddLog($"No users found matching ID: {userFound.Id} Users: {foundUsers}");
                    await Context.Channel.SendMessageAsync($"{userFound.Mention} doesn't have an owner profile yet, to get one they need to create a character");
                    return;
                }
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("switch"), Summary("Testicules Switch Character")]
        public async Task Testacules7()
        {
            try
            {
                bool hasCharacters = await VerifyOwnerProfileAndIfHasCharacters();
                OwnerProfile ownerProfile = RPG.Owners.Find(x => x.OwnerID == Context.Message.Author.Id);
                if (!hasCharacters)
                {
                    await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} you don't have a character yet, try creating one... pleb");
                    return;
                }
                string response = $"Please enter the number for the respective character you want to use:{line}";
                int counter = 0;
                foreach (Character chara in ownerProfile.CharacterList)
                {
                    counter++;
                    response = $"{response}[{counter}] {chara.Name}{line}";
                }
                var sentMsg = await Context.Channel.SendMessageAsync($"{response}"); DateTime timeStamp2 = DateTime.Now;
                bool respRecvd = false;
                int chosenCharacter = 0;
                while (!respRecvd)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    var newList = await Context.Channel.GetMessagesAsync(5).Flatten();
                    Toolbox.uDebugAddLog("Generated message list");
                    string answer = string.Empty;
                    foreach (IMessage msg in newList)
                    {
                        if ((Context.Message.Author == msg.Author) && (sentMsg.Timestamp.DateTime < msg.Timestamp.DateTime))
                        {
                            answer = msg.Content.ToString();
                            Toolbox.uDebugAddLog("Found message from OP with a newer DateTime than the original message");
                            Toolbox.uDebugAddLog($"Before Response: {answer}");
                            answer = Regex.Replace(msg.Content.ToString(), @"\s+", "");
                            Toolbox.uDebugAddLog($"After response: {answer}");
                            var isNum = int.TryParse(answer, out chosenCharacter);
                            if (!isNum)
                            {
                                await Context.Channel.SendMessageAsync($"{answer} isnt' a valid response");
                                respRecvd = false;
                            }
                            else
                                respRecvd = true;
                        }
                    }
                    if (timeStamp2 + TimeSpan.FromSeconds(60) <= DateTime.Now)
                    {
                        Toolbox.uDebugAddLog($"Response wasn't received from {Context.Message.Author.Username} ({Context.Message.Author.Id}) within 60s, canceled character creation");
                        await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} A valid response wasn't received within 60 seconds, canceling creation request");
                        return;
                    }
                }
                Character selChara = ownerProfile.CharacterList[chosenCharacter - 1];
                Management.ChangeCharacter(ownerProfile.OwnerID, selChara);
                await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} your active character is now {selChara.Name}!");
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("testiculees"), Summary("Testicules Add Testiculees")]
        public async Task Testacules9()
        {
            try
            {
                if (!Permissions.Administrators.Contains(Context.Message.Author.Id))
                {
                    await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} You don't have rights to run this command");
                    return;
                }
                OwnerProfile ownerProfile = RPG.Owners.Find(x => x.OwnerID == Context.Message.Author.Id);
                if (ownerProfile == null)
                {
                    OwnerProfile owner = new OwnerProfile() { OwnerID = Context.Message.Author.Id };
                    RPG.Owners.Add(owner);
                    Events.uStatusUpdateExt($"Owner profile not found, created one for {Context.Message.Author.Username} | {Context.Message.Author.Id}");
                    await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} you didn't have a profile yet so I made you one");
                }
                else
                    Toolbox.uDebugAddLog($"Owner profile was found for {Context.Message.Author.Username} | {Context.Message.Author.Id}");
                Character testiculees = Testing.testiculeesCharacter;
                ownerProfile.CharacterList.Add(testiculees);
                ownerProfile.CurrentCharacter = testiculees;
                await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} you have been granted the power of TESTICULEEEEEES!!!");
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("delete"), Summary("Testicules Delete Profile")]
        public async Task Testacules10()
        {
            try
            {
                if (!Permissions.Administrators.Contains(Context.Message.Author.Id))
                {
                    await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} You don't have rights to run this command");
                    return;
                }
                OwnerProfile ownerProfile = RPG.Owners.Find(x => x.OwnerID == Context.Message.Author.Id);
                if (ownerProfile != null)
                {
                    RPG.Owners.Remove(ownerProfile);
                    await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} your profile has been successfully deleted");
                }
                else
                    await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} you don't have a profile to delete");
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("delete"), Summary("Testicules Delete Profile")]
        public async Task Testacules10o([Remainder]string mentionedUser)
        {
            try
            {
                if (!Permissions.Administrators.Contains(Context.Message.Author.Id))
                {
                    await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} You don't have rights to run this command");
                    return;
                }
                Toolbox.uDebugAddLog($"Before Removing '<,@,>': {mentionedUser}");
                mentionedUser = mentionedUser.Replace("<", string.Empty).Replace("@", string.Empty).Replace(">", string.Empty);
                Toolbox.uDebugAddLog($"After Removing '<,@,>': {mentionedUser}");
                ulong userID = 0;
                var isUlong = ulong.TryParse(mentionedUser, out userID);
                if (!isUlong)
                {
                    Toolbox.uDebugAddLog($"Invalid Ulong: {userID}");
                    await Context.Channel.SendMessageAsync($"{mentionedUser} isn't a valid discord user");
                    return;
                }
                var userFound = await Context.Channel.GetUserAsync(userID);
                if (userFound == null)
                {
                    Toolbox.uDebugAddLog($"Invalid User: {userFound.Username} | {userID}");
                    await Context.Channel.SendMessageAsync($"{userID} doesn't match a discord user on your server");
                    return;
                }
                Toolbox.uDebugAddLog($"MentionedUser: {mentionedUser}");
                OwnerProfile ownerProfile = RPG.Owners.Find(x => x.OwnerID == userFound.Id);
                if (ownerProfile != null)
                {
                    RPG.Owners.Remove(ownerProfile);
                    await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} {userFound.Username}'s RPG profile has been deleted");
                }
                else
                    await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} {userFound.Username} doesn't have a RPG profile");
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("rpg"), Summary("Testicules Delete Profile")]
        public async Task Testacules11()
        {
            try
            {
                if (!Permissions.AdminPermissions(Context))
                {
                    await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} You don't have rights to run this command");
                    return;
                }
                if (!Permissions.AllowedChannels.Contains(Context.Channel.Id))
                {
                    Toolbox.uDebugAddLog($"Channel isn't an RPG channel, attempting to add RPG Channel: {Context.Channel.Name} | {Context.Channel.Id}");
                    Permissions.AllowedChannels.Add(Context.Channel.Id);
                    Events.uStatusUpdateExt($"RPG Channel Added: {Context.Channel.Name} | {Context.Channel.Id}");
                    await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} Successfully added RPG Channel **{Context.Channel.Name}**");
                }
                else
                {
                    Toolbox.uDebugAddLog($"Channel is already an RPG channel, attempting to remove RPG Channel: {Context.Channel.Name} | {Context.Channel.Id}");
                    Permissions.AllowedChannels.Remove(Context.Channel.Id);
                    Events.uStatusUpdateExt($"RPG Channel Removed: {Context.Channel.Name} | {Context.Channel.Id}");
                    await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} Successfully removed RPG Channel **{Context.Channel.Name}**");
                }
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }
        
        [Command("permission"), Summary("Testicules Permission Test")]
        public async Task Testacules12()
        {
            try
            {
                if (Permissions.RPGChannelPermission(Context))
                    await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} this channel is an RPG channel, go nuts!");
                else
                    await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} this channel hasn't been enabled as an RPG channel");
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("match"), Summary("Testicules Match Test")]
        public async Task Testacules13()
        {
            try
            {
                var hasChar = await VerifyOwnerProfileAndIfHasCharacters();
                if (!hasChar)
                {
                    await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} you don't currently have any characters, please create one before trying to start a match");
                    return;
                }
                Toolbox.uDebugAddLog($"Starting match command");
                OwnerProfile owner = RPG.Owners.Find(x => x.OwnerID == Context.Message.Author.Id);
                Management.CreateMatch(Context, owner);
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("attack"), Summary("Testicules Attack Test")]
        public async Task Testacules14()
        {
            try
            {
                var hasChar = await VerifyOwnerProfileAndIfHasCharacters();
                if (!hasChar)
                {
                    await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} you don't currently have any characters, please create one before trying to attack something");
                    return;
                }
                OwnerProfile owner = RPG.Owners.Find(x => x.OwnerID == Context.Message.Author.Id);
                Match match = RPG.MatchList.Find(x => x.Owner == owner);
                if (match == null)
                {
                    await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} you don't currently have an active match, please start a match before trying to attack nothing");
                    return; 
                }
                Management.AttackEnemy(Context, owner, match.CurrentEnemy);
                Management.CalculateTurn(Context, owner);
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("loot"), Summary("Testicules Loot Test")]
        public async Task Testacules15()
        {
            try
            {
                var hasChar = await VerifyOwnerProfileAndIfHasCharacters();
                if (!hasChar)
                {
                    await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} you don't currently have any characters, please create one before trying to get some of that dank loot");
                    return;
                }
                await Management.EmptyLoot(Context);
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        public async Task<bool> VerifyOwnerProfileAndIfHasCharacters()
        {
            OwnerProfile ownerProfile = RPG.Owners.Find(x => x.OwnerID == Context.Message.Author.Id);
            if (ownerProfile == null)
            {
                OwnerProfile owner = new OwnerProfile() { OwnerID = Context.Message.Author.Id };
                RPG.Owners.Add(owner);
                Events.uStatusUpdateExt($"Owner profile not found, created one for {Context.Message.Author.Username} | {Context.Message.Author.Id}");
                await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} you didn't have a profile yet so I made you one");
                ownerProfile = RPG.Owners.Find(x => x.OwnerID == Context.Message.Author.Id);
            }
            else
                Toolbox.uDebugAddLog($"Owner profile was found for {Context.Message.Author.Username} | {Context.Message.Author.Id}");
            return ownerProfile.CharacterList.Count == 0 ? false : true;
        }
    }
}