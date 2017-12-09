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
using System.Net;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;

namespace PersonalDiscordBot.Classes
{
    #region Server
    [Group("server")]
    public class ServerModule : ModuleBase
    {
        [Command("games"), Summary("Returns List of Current Servers")]
        public async Task Games()
        {
            try
            {
                StringBuilder _sb = new StringBuilder();
                _sb.AppendLine($"```Server Game | # of Servers{Environment.NewLine}");
                foreach (var game in MainWindow.ServerList.GroupBy(s => s.Game))
                    _sb.AppendLine($"{game.Key} | {game.Count()}");
                _sb.AppendLine("```");
                await Context.SendDiscordMessage(_sb.ToString());
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        [Command("games"), Summary("Returns List of Current Servers of a specific game type")]
        public async Task GamesSpecific([Remainder]string gameServer)
        {
            try
            {
                int counter = 0;
                StringBuilder _sb = new StringBuilder();
                _sb.AppendLine("");
                foreach (var game in MainWindow.ServerList)
                {
                    if (game.Game.ToLower() == gameServer.ToLower())
                    {
                        _sb.AppendLine(string.Format("```Game: {1}{0} Server Name: {2}{0} Password: {3}{0} Modded: {4}{0} Host: {5} Port: {6}```", Environment.NewLine, game.Game, game.ServerName, game.Password, game.Modded, game.ExtHostname, game.PortNum));
                        counter++;
                    }
                }
                if (counter == 0)
                    _sb.AppendLine($"{Context.User.Mention} No game servers for **{gameServer}** were found");
                await Context.SendDiscordMessage(_sb.ToString());
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
                    await Context.SendDiscordMessage(string.Format("There currently aren't any game servers in the server list running the game {0}", gameServer));
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
                    CheckOnServer(Context, chosenServ);
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
                    CheckOnServer(Context, chosenServ);
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
                            response = string.Format("{0}```Name: {1}{7}Score: {2}{7}Time Connected: {3}D {4}H {5}M {6}S```", response, player.Name, player.Score.ToString(), player.Time.Days, player.Time.Hours, player.Time.Minutes, player.Time.Seconds, Environment.NewLine).Replace("Score: 0", "");
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

        private void CheckOnServer(ICommandContext Context, GameServer game)
        {
            try
            {
                DateTime timeStart = DateTime.Now;
                TimeSpan waitTime = TimeSpan.FromMinutes(30);
                Toolbox.uDebugAddLog("Got timestamp, now starting server check thread");
                Thread startCheck = new Thread(async () =>
                {
                    try
                    {
                        var oneFollowup = false;
                        var twoFollowup = false;
                        ReadOnlyCollection<Player> playerList = null;
                        IPEndPoint endpoint = CreateIPEndPoint(string.Format("{0}:{1}", game.IPAddress, game.QueryPort));
                        Toolbox.uDebugAddLog($"Connection endpoint: {endpoint.ToString()}");
                        QueryMaster.ServerInfo servInfo = GetServerInfo(endpoint, out playerList);
                        Toolbox.uDebugAddLog("Created servInfo variable");
                        while (servInfo == null)
                        {
                            Toolbox.uDebugAddLog($"servInfo for {game.Game} server {game.ServerName} is null");
                            servInfo = GetServerInfo(endpoint, out playerList);
                            Thread.Sleep(TimeSpan.FromSeconds(15));
                            Toolbox.uDebugAddLog(string.Format("Waited 15 seconds after rebooting the {0} game server", game.ServerName));
                            if (timeStart + TimeSpan.FromMinutes(10) <= DateTime.Now && oneFollowup == false) { await Context.Channel.SendMessageAsync($"It has been **{(DateTime.Now - timeStart).Minutes} min** since rebooting the **{game.Game}** server \"**{game.ServerName}**\", I will keep checking for the next **{(waitTime - (DateTime.Now - timeStart)).Minutes} min**"); oneFollowup = true; }
                            if (timeStart + TimeSpan.FromMinutes(20) <= DateTime.Now && twoFollowup == false) { await Context.Channel.SendMessageAsync($"It has been **{(DateTime.Now - timeStart).Minutes} min** since rebooting the **{game.Game}** server \"**{game.ServerName}**\", I will keep checking for the next **{(waitTime - (DateTime.Now - timeStart)).Minutes} min**"); twoFollowup = true; }
                            if (timeStart + waitTime <= DateTime.Now)
                            {
                                await Context.Channel.SendMessageAsync($"I tried checking on the \"**{game.ServerName}**\" server for you but it never came up or I can't connect to it for some reason, its been **{waitTime.Minutes} min** so I'm gonna stop checking...");
                                Toolbox.uDebugAddLog($"Waited the full {waitTime.Minutes} min after rebooting the {game.ServerName} game server without connectivity, stopping thread");
                                return;
                            }
                        }
                        TimeSpan timeTaken = DateTime.Now - timeStart;
                        Toolbox.uDebugAddLog($"servInfo for {game.Game} server {game.ServerName} was found");
                        await Context.Channel.SendMessageAsync($"The **{game.Game}** server \"**{game.ServerName}**\" is now up and running after **{timeTaken.Minutes} min**");
                        Toolbox.uDebugAddLog(string.Format("Successfully alerted when the {0} game server came back up, took {1} min", game.ServerName, timeTaken.Minutes));
                    }
                    catch (Exception ex)
                    {
                        FullExceptionLog(ex);
                    }
                });
                startCheck.Start();
                Toolbox.uDebugAddLog("Started server check thread");
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
            QueryMaster.ServerInfo serverInfo = null;
            players = null;
            try
            {
                using (var server = ServerQuery.GetServerInstance(EngineType.Source, endpoint))
                {
                    serverInfo = server.GetInfo();
                    players = server.GetPlayers();
                }
                if (players != null)
                    players = new ReadOnlyCollection<QueryMaster.Player>(players.Where(record => !string.IsNullOrWhiteSpace(record.Name)).ToList());

                return serverInfo;
            }
            catch (SocketException se)
            {
                if (se.HResult == -2147467259) { return serverInfo; } // A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond
                FullExceptionLog(se);
                return serverInfo;
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
                return serverInfo;
            }
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
            string _logLocation = string.Format(@"{0}\Exceptions.log", Toolbox._paths.LogLocation);
            try
            {
                if (!File.Exists(_logLocation))
                    using (StreamWriter _sw = new StreamWriter(_logLocation))
                        _sw.WriteLine(exString + Environment.NewLine);
                else
                    using (StreamWriter _sw = File.AppendText(_logLocation))
                        _sw.WriteLine(exString + Environment.NewLine);
            }
            catch (IOException)
            {
                Toolbox.SaveFileRetry(_logLocation, exString);
            }
        }
    }
    #endregion

    #region Help
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
                 "```;help translate{0}Text/Message translation methods, leetify your snoop game yo```"+
                 "```;help test{0}Show commands to test RPG stuff```",
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
                 "```;server games{0}Displays game servers and how many servers there```" +
                 "```;server games %GameName%{0}Displays game server info for specified game```" +
                 "```;server reboot %GameName%{0}Reboot's the the requested server```" +
                 "```;server logs %GameName%{0}Get's the latest 3 server logs on the requested server```" +
                 "```;server logs 8 %GameName%{0}Get's the latest 8 server logs on the requested server, or however many you want, replace the 8 with a different number```" +
                 "```;server status %GameName%{0}Get's the current server status and info (players, online, connectable, ect) on the requested server```" +
                 "```;server players %GameName%{0}Show's all players and info for the requested server```",
                 Environment.NewLine
                );
                await Context.SendDiscordMessage(_helpArticle);
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
                 "```;help{0}   You already did this command, don't do it again unless you're a pleb or something...{0}" +
                 ";status{0}   Displays information about the bots' current status/info{0}" +
                 ";playing{0}   Sets the bots' Playing status to nothing (not playing anything){0}" +
                 ";playing a game{0}   Sets the bots' Playing status to 'a game' or whatever you type after playing{0}" +
                 ";name{0}   Sets the bots' Name status to 'My Boiiiiiiiiiiiii'{0}" +
                 ";name Raspberry Schmeckles{0}   Sets the bots' Name status to 'Raspberry Schmeckles' or whatever you type after name{0}" +
                 ";f @Someone{0}   Pays respects to a mentioned person / or whatever you want{0}" +
                 ";update{0}   Checks for an update for the bot{0}" +
                 ";save{0}   Saves Paths, Server, and RPG Data manually```",
                 Environment.NewLine
                );
                await Context.SendDiscordMessage(_helpArticle);
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
                await Context.SendDiscordMessage(_helpArticle);
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("test"), Summary("RPG Testing Commands")]
        public async Task DefHelpTest()
        {
            try
            {
                string _helpArticle = string.Format
                (
                 "_{0}" +
                 "{0}```╬ RPG Testing Commands ╬```{0}" +
                 "```;test{0}" +
                 ";test add admin %USER%{0}" +
                 ";test remove admin %USER%{0}" +
                 ";test weap{0}" +
                 ";test spell{0}" +
                 ";test armor{0}" +
                 ";test thing{0}" +
                 ";test rng weap %Number%{0}" +
                 ";test rng spell %Number%{0}" +
                 ";test rng armor %Number%{0}" +
                 ";test rng thing %Number%{0}" +
                 ";test create{0}" +
                 ";test give %Character% %CurrencyAmount%{0}" +
                 ";test switch{0}" +
                 ";test testiculees{0}" +
                 ";test delete{0}" +
                 ";test delete %USER%{0}" +
                 ";test rpg{0}" +
                 ";test permission{0}" +
                 ";test match{0}" +
                 ";test attack{0}" +
                 ";test loot{0}" +
                 ";test item{0}" +
                 ";test view{0}" +
                 ";test change armor{0}" +
                 ";test change weapon{0}" +
                 ";test change description{0}" +
                 ";test add loot{0}" +
                 ";test testing %Role/Group%{0}" +
                 ";test log channel %MentionChannel%{0}" +
                 ";test check backpack{0}" +
                 ";test change color %color%{0}" +
                 "   (red, blue, black, green, yellow, brown, orange, gold, pink, purple, silver, slategray, white){0}" +
                 ";test change color 00,00,00 (r,g,b)```",
                 Environment.NewLine
                );
                await Context.SendDiscordMessage(_helpArticle);
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }
    }
    #endregion

    #region Snoop
    [Group("snoop"), Summary("Snoopifies yo text")]
    public class SnoopModule : ModuleBase
    {
        [Command("dogg"), Summary("Toggles all messages being translated")]
        public async Task SnoopToggle()
        {
            try
            {
                if (Toolbox._paths.Snooping) Toolbox._paths.Snooping = false;
                else Toolbox._paths.Snooping = true;
                await Context.SendDiscordMessage(string.Format("Snooping all messages in this channel: {0}", Toolbox._paths.Snooping));
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
                await Context.SendDiscordMessage(string.Format("{0}{1}{2}", Context.User.Mention, Environment.NewLine, message.ToSnoopification()));
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }
    }
    #endregion

    #region 1337
    [Group("leet"), Summary("Makes your text that of a 1337 hackaman")]
    public class LeetModule : ModuleBase
    {
        [Command(""), Summary("Default Entry")]
        public async Task Leetify([Remainder]string message)
        {
            try
            {
                await Context.Message.DeleteAsync();
                await Context.SendDiscordMessage(string.Format("{0}{1}{2}", Context.User.Mention, Environment.NewLine, message.ToLeet()));
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
                await Context.SendDiscordMessage(string.Format("{0}{1}{2}", Context.User.Mention, Environment.NewLine, Leet.ToLeet(message, lvl)));
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }
    }
    #endregion

    #region General Commands
    [Group("status"), Summary("Gets the bot status/info")]
    public class StatusModule : ModuleBase
    {
        [Command(""), Summary("Default Entry")]
        public async Task GetStatus()
        {
            try
            {
                await Context.SendDiscordMessage(string.Format("Snoopify All: {0}", Toolbox._paths.Snooping));
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
                await Context.SendDiscordMessage(string.Format("Set bot to playing nothing"));
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
                await Context.SendDiscordMessage(string.Format("Set bot playing to: {0}", playing));
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }
    }

    [Group("update"), Summary("Checks for update for bot")]
    public class UpdateModule : ModuleBase
    {
        [Command(""), Summary("Default Entry")]
        public async Task Update()
        {
            try
            {
                var admin = Permissions.AdminPermissions(Context);
                if (!admin)
                {
                    await Context.SendDiscordMessage($"{Context.User.Mention} You don't have permissions to run this command");
                    return;
                }
                var gitClient = MainWindow.gitClient;
                if (gitClient == null)
                    gitClient = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("PDB"));
                if (Toolbox._paths.CurrentVersion == null)
                    Toolbox._paths.CurrentVersion = new Version("0.1.0.0");
                var releases = await gitClient.Repository.Release.GetAll("rwobig93", "ServerRPGAdventure");
                var release = releases[0];
                Version releaseVersion = new Version(release.TagName);
                var result = Toolbox._paths.CurrentVersion.CompareTo(releaseVersion);
                if (result < 0)
                {
                    await Context.SendDiscordMessage($"Newer release found, updating now... [Current]{Toolbox._paths.CurrentVersion} [Release]{releaseVersion}");
                    //var logChannel = await Context.Guild.GetChannelAsync(Permissions.GeneralPermissions.logChannel);
                    //if (logChannel != null)
                    //{
                    //    await ((IMessageChannel)logChannel).SendMessageAsync($"Update command sent by {Context.User.Username} and a newer version was found, updating now...");
                    //}
                    Toolbox._paths.CurrentVersion = releaseVersion;
                    Toolbox._paths.Updated = true;
                    MainWindow.SaveConfig(MainWindow.ConfigType.Paths);
                    MainWindow.StartUpdate();
                }
                else
                {
                    await Context.SendDiscordMessage($"Release Version is the same version or older than running assembly: {Environment.NewLine}[Current]{Toolbox._paths.CurrentVersion}{Environment.NewLine}[Release]{releaseVersion}");
                }
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }
    }

    [Group("save"), Summary("Saves RPG and Bot Info")]
    public class SaveModule : ModuleBase
    {
        [Command(""), Summary("Default Entry")]
        public async Task Save()
        {
            try
            {
                var admin = Permissions.AdminPermissions(Context);
                if (!admin)
                {
                    await Context.SendDiscordMessage($"{Context.User.Mention} You don't have permissions to run this command");
                    return;
                }
                MainWindow.SaveConfig(MainWindow.ConfigType.Paths);
                MainWindow.SaveConfig(MainWindow.ConfigType.Servers);
                MainWindow.SaveRPGData();
                await Context.SendDiscordMessage($"{Context.User.Mention} Successfully saved Paths, Server, and RPG Data");
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
                await Context.SendDiscordMessage(string.Format("Set bot name to: {0}", "My Boiiiiiiiii"));
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
                await Context.SendDiscordMessage(string.Format("Set bot name to: {0}", newName));
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
                await Context.SendDiscordMessage(string.Format("{0} has paid their respects to {1} :heart:", Context.Message.Author.Mention, mentionedUser.Mention));
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
                await Context.SendDiscordMessage(string.Format("{0} has paid their respects to **{1}** :heart:", Context.Message.Author.Mention, respectsTo));
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }
    }
    #endregion

    #region RPG Testing
    [Group("test"), Summary("For Testing")]
    public class TestingModule : ModuleBase
    {
        private static string line = Environment.NewLine;
        [Command(""), Summary("Testicules Engage")]
        public async Task Testacules()
        {
            try
            {
                await Context.SendDiscordMessage("No default test method is set");
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("add admin"), Summary("Add User to Admin List")]
        public async Task Testacules1a(string mentionedUser)
        {
            try
            {
                if (!Permissions.AdminPermissions(Context))
                {
                    await Context.SendDiscordMessageMention($"You don't have rights to run this command");
                    return;
                }
                else
                {
                    Toolbox.uDebugAddLog($"Before Removing '<,@,>': {mentionedUser}");
                    mentionedUser = mentionedUser.Replace("<", string.Empty).Replace("@", string.Empty).Replace(">", string.Empty);
                    Toolbox.uDebugAddLog($"After Removing '<,@,>': {mentionedUser}");
                    ulong userID = 0;
                    var isUlong = ulong.TryParse(mentionedUser, out userID);
                    if (!isUlong)
                    {
                        Toolbox.uDebugAddLog($"Invalid Ulong: {userID}");
                        await Context.SendDiscordMessage($"{mentionedUser} isn't a valid discord user");
                        return;
                    }
                    var userFound = await Context.Channel.GetUserAsync(userID);
                    if (userFound == null)
                    {
                        Toolbox.uDebugAddLog($"Invalid User: {userFound.Username} | {userID}");
                        await Context.SendDiscordMessage($"{userID} doesn't match a discord user on your server");
                        return;
                    } 
                    Toolbox.uDebugAddLog($"MentionedUser: {mentionedUser}");
                    int foundUsers = 0;
                    foreach (var admin in Permissions.Administrators)
                    {
                        if (Permissions.Administrators.Find(x => x.ID == userFound.Id) != null)
                        {
                            Events.uStatusUpdateExt($"Found {userFound.Username} in Admin List | {userFound.Id}");
                            await Context.SendDiscordMessageMention($"{userFound.Mention} is already an Admin. You can't just give them Admin power twice. That's not how you create Super Admins.");
                            foundUsers++;
                        }
                    }
                    if (foundUsers == 0)
                    {
                        Toolbox.uDebugAddLog($"No admins found matching ID: {userFound.Id} MatchedAdmins: {foundUsers}");
                        Administrator newAdmin = new Administrator() { ID = userFound.Id, Username = userFound.Username, AddedBy = Context.User.Username };
                        Permissions.Administrators.Add(newAdmin);
                        await Context.SendDiscordMessageMention($"Added {userFound.Mention} as an Admin. I hope you know what you're doing.");
                        Toolbox.uDebugAddLog($"Added Admin: {newAdmin.Username} | {newAdmin.ID} | By: {Context.User.Username}");
                        return;
                    }
                    Events.UseGlblAction(Toolbox.GlobalAction.AdminChanged);
                }
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("remove admin"), Summary("Add User to Admin List")]
        public async Task Testacules1r(string mentionedUser)
        {
            try
            {
                if (!Permissions.AdminPermissions(Context))
                {
                    await Context.SendDiscordMessageMention($"You don't have rights to run this command");
                    return;
                }
                else
                {
                    Toolbox.uDebugAddLog($"Before Removing '<,@,>': {mentionedUser}");
                    mentionedUser = mentionedUser.Replace("<", string.Empty).Replace("@", string.Empty).Replace(">", string.Empty);
                    Toolbox.uDebugAddLog($"After Removing '<,@,>': {mentionedUser}");
                    ulong userID = 0;
                    var isUlong = ulong.TryParse(mentionedUser, out userID);
                    if (!isUlong)
                    {
                        Toolbox.uDebugAddLog($"Invalid Ulong: {userID}");
                        await Context.SendDiscordMessage($"{mentionedUser} isn't a valid discord user");
                        return;
                    }
                    var userFound = await Context.Channel.GetUserAsync(userID);
                    if (userFound == null)
                    {
                        Toolbox.uDebugAddLog($"Invalid User: {userFound.Username} | {userID}");
                        await Context.SendDiscordMessage($"{userID} doesn't match a discord user on your server");
                        return;
                    }
                    Toolbox.uDebugAddLog($"MentionedUser: {mentionedUser}");
                    int foundUsers = 0;
                    foreach (var admin in Permissions.Administrators)
                    {
                        var foundAdmin = Permissions.Administrators.Find(x => x.ID == userFound.Id);
                        if (foundAdmin != null)
                        {
                            Events.uStatusUpdateExt($"Removed {foundAdmin.Username} from Admin List | {foundAdmin.ID}");
                            Permissions.Administrators.Remove(foundAdmin);
                            await Context.SendDiscordMessageMention($"Removed {userFound.Mention}'s power. They are no longer an Admin.");
                            foundUsers++;
                        }
                    }
                    if (foundUsers == 0)
                    {
                        Toolbox.uDebugAddLog($"No admins found matching ID: {userFound.Id} Admins: {foundUsers}");
                        await Context.SendDiscordMessageMention($"{userFound.Mention} is not currently an Admin. You can't take away something they don't even have.");
                        return;
                    }
                    Events.UseGlblAction(Toolbox.GlobalAction.AdminChanged);
                }
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
                await Context.SendDiscordMessage($"Generated {weapName}{randGen}");
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
                await Context.SendDiscordMessage(Testing.RandomSpell());
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("armor"), Summary("Testicules Armor Gen")]
        public async Task Testacules2a()
        {
            try
            {
                string armorName = string.Empty;
                string randGen = Testing.RandomArmor(out armorName);
                await Context.SendDiscordMessage($"Generated: {armorName}{randGen}");
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("thing"), Summary("Testicules Item Gen")]
        public async Task Testacules2t()
        {
            try
            {
                await Context.SendDiscordMessage(Testing.RandomItem());
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
                var intTimes = 0;
                var isANum = int.TryParse(times, out intTimes);
                if (isANum)
                {
                    if (intTimes > 100000)
                    {
                        await Context.SendDiscordMessage("The highest integer allowed is 100,000. I'm generating that for you now, don't do that again!");
                        intTimes = 100000;
                        await Context.SendDiscordMessage($"Generated {intTimes} Weapons:{Environment.NewLine}{Testing.RandomMassTestWeap(intTimes)}");
                    }
                    else
                        await Context.SendDiscordMessage($"Generated {intTimes} Weapons:{Environment.NewLine}{Testing.RandomMassTestWeap(intTimes)}");
                }
                else
                    await Context.SendDiscordMessage($"{times} is not a valid number. Rethink your life choices and try again.");
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
                var intTimes = 0;
                var isANum = int.TryParse(times, out intTimes);
                if (isANum)
                {
                    if (intTimes > 100000)
                    {
                        await Context.SendDiscordMessage("The highest integer allowed is 100,000. I'm generating that for you now, don't do that again!");
                        intTimes = 100000;
                        await Context.SendDiscordMessage($"Generated {intTimes} Spells:{Environment.NewLine}{Testing.RandomMassTestSpell(intTimes)}");
                    }
                    else
                        await Context.SendDiscordMessage($"Generated {intTimes} Spells:{Environment.NewLine}{Testing.RandomMassTestSpell(intTimes)}");
                }
                else
                    await Context.SendDiscordMessage($"{times} is not a valid number. Rethink your life choices and try again.");
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("rng armor"), Summary("Testicules RNG Gen Armor")]
        public async Task Testacules3a(string times)
        {
            try
            {
                var intTimes = 0;
                var isANum = int.TryParse(times, out intTimes);
                if (isANum)
                {
                    if (intTimes > 100000)
                    {
                        await Context.SendDiscordMessage("The highest integer allowed in 100,000. I'm generating that for your now. Don't do that again!");
                        intTimes = 100000;
                        await Context.SendDiscordMessage($"Generated {intTimes} Armors: {Environment.NewLine}{Testing.RandomMassTestArmor(intTimes)}");
                    }
                    else
                        await Context.SendDiscordMessage($"Generated {intTimes} Armors: {Environment.NewLine}{Testing.RandomMassTestArmor(intTimes)}");
                }
                else
                    await Context.SendDiscordMessage($"{times} is not a valid number. Rethink your life choices and try again.");
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("rng thing"), Summary("Testicules RNG Gen Item")]
        public async Task Testacules3t(string times)
        {
            try
            {
                var intTimes = 0;
                var isANum = int.TryParse(times, out intTimes);
                if (isANum)
                {
                    if (intTimes > 100000)
                    {
                        await Context.SendDiscordMessage("The highest integer allowed is 100,000. I'm generating that for you now, don't do that again!");
                        intTimes = 100000;
                        await Context.SendDiscordMessage($"Generated {intTimes} Items: {Environment.NewLine}{Testing.RandomMassTestItem(intTimes)}");
                    }
                    await Context.SendDiscordMessage($"Generated {intTimes} Items: {Environment.NewLine}{Testing.RandomMassTestItem(intTimes)}");
                }
                else
                    await Context.SendDiscordMessage($"{times} is not a valid number. Rethink your life choices and try again.");
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
                        await Context.SendDiscordMessage($"A new character for you costs {cost} currency but you only have {ownerProfile.Currency}, please get good");
                        return;
                    }
                    Toolbox.uDebugAddLog($"SENDINGMESSAGE: It will cost you {cost} currency to create a new character, would you still like to create a character? (Yes/No) (You have {ownerProfile.Currency} currently) [ID]{Context.User.Id} [Name]{Context.User.Username}");
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
                                if (response.ToLower() == "no")
                                {
                                    await Context.SendDiscordMessage($"{Context.User.Mention} Operation cancelled");
                                    return;
                                }
                            }
                        }
                        if (costTimeStamp + TimeSpan.FromSeconds(60) <= DateTime.Now)
                        {
                            Toolbox.uDebugAddLog($"Response wasn't received from {Context.Message.Author.Username} ({Context.Message.Author.Id}) within 60s, canceled character creation");
                            await Context.SendDiscordMessageMention($"A valid response wasn't received within 60 seconds, canceling creation request");
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
                                    await Context.SendDiscordMessage($"{response} isn't a valid response, please try again");
                                    break;
                            }
                        }
                    }
                    if (timeStamp + TimeSpan.FromSeconds(60) <= DateTime.Now)
                    {
                        Toolbox.uDebugAddLog($"Response wasn't received from {Context.Message.Author.Username} ({Context.Message.Author.Id}) within 60s, canceled character creation");
                        await Context.SendDiscordMessageMention($"A valid response wasn't received within 60 seconds, canceling creation request");
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
                                if (response == "Testiculees teh Great")
                                {
                                    await Context.SendDiscordMessage($"{Context.User.Mention} Nice try but you are not the great one, that power is beyond your reach!");
                                    Toolbox.uDebugAddLog($"{Context.User.Username} tried to recreate Testiculees and we told them no [ID]{Context.User.Id}");
                                }
                                else
                                {
                                    Toolbox.uDebugAddLog($"Valid response recieved, setting Character name [ID]{Context.User.Id}");
                                    charName = response;
                                    responseRecvd2 = true;
                                }
                            }
                        }
                        if (timeStamp2 + TimeSpan.FromSeconds(60) <= DateTime.Now)
                        {
                            Toolbox.uDebugAddLog($"Response wasn't received from {Context.Message.Author.Username} ({Context.Message.Author.Id}) within 60s, canceled character creation");
                            await Context.SendDiscordMessageMention($"A valid response wasn't received within 60 seconds, canceling creation request");
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
                                        await Context.SendDiscordMessage($"{response} isn't a valid response, please try again");
                                        break;
                                }
                            }
                        }
                        if (timeStamp3 + TimeSpan.FromSeconds(60) <= DateTime.Now)
                        {
                            Toolbox.uDebugAddLog($"Response wasn't received from {Context.Message.Author.Username} ({Context.Message.Author.Id}) within 60s, canceled character creation");
                            await Context.SendDiscordMessageMention($"A valid response wasn't received within 60 seconds, canceling creation request");
                            return;
                        }
                    }
                    if (nameTimeStamp + TimeSpan.FromMinutes(5) <= DateTime.Now)
                    {
                        Toolbox.uDebugAddLog($"Response wasn't received from {Context.Message.Author.Username} ({Context.Message.Author.Id}) within 5min, canceled character creation");
                        await Context.SendDiscordMessageMention($"A valid response wasn't received within 60 seconds, canceling creation request");
                        return;
                    }
                }
                if (cost <= 0)
                    await Context.SendDiscordMessageMention($"This first character is on us, enjoy");
                else
                {
                    ownerProfile.Currency -= cost;
                    await Context.SendDiscordMessageMention($"You have been charged {cost} currency, you now have: {ownerProfile.Currency}");
                }
                Character newChar = Management.CreateNewCharacter(Context.Message.Author.Id, chosenClass, charName);
                ownerProfile.CharacterList.Add(newChar);
                if (ownerProfile.CharacterList.Count == 1)
                    ownerProfile.CurrentCharacter = newChar;
                await Context.SendDiscordMessage($"Congratulations! Your new hero has been born:```{line}" +
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
                if (!Permissions.AdminPermissions(Context))
                {
                    await Context.SendDiscordMessageMention($"You don't have rights to run this command");
                    return;
                }
                int currency = 0;
                var isNum = int.TryParse(amount, out currency);
                if (!isNum)
                {
                    Toolbox.uDebugAddLog($"Invalid Number: {amount}");
                    await Context.SendDiscordMessage($"{amount} isn't a valid number");
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
                    await Context.SendDiscordMessage($"{mentionedUser} isn't a valid discord user");
                    return;
                }
                var userFound = await Context.Channel.GetUserAsync(userID);
                if (userFound == null)
                {
                    Toolbox.uDebugAddLog($"Invalid User: {userFound.Username} | {userID}");
                    await Context.SendDiscordMessage($"{userID} doesn't match a discord user on your server");
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
                        await Context.SendDiscordMessageMention($"Added {currency} currency to {userFound.Mention}'s profile");
                        foundUsers++;
                    }
                }
                if (foundUsers == 0)
                {
                    Toolbox.uDebugAddLog($"No users found matching ID: {userFound.Id} Users: {foundUsers}");
                    await Context.SendDiscordMessage($"{userFound.Mention} doesn't have an owner profile yet, to get one they need to create a character");
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
                    await Context.SendDiscordMessageMention($"you don't have a character yet, try creating one... pleb");
                    return;
                }
                List<IMessage> recvdMsgs = new List<IMessage>();
                var match = RPG.MatchList.Find(x => x.Owner == ownerProfile);
                if (match != null)
                {
                    var verifyMatch = await Context.Channel.SendMessageAsync($"You currently have an active match with {match.CurrentEnemy.Name}, your match will end if you switch characters, would you still like to switch? (Yes/No)");
                    bool verifyRecvd = false;
                    while (!verifyRecvd)
                    {
                        var msgList = await Context.Channel.GetMessagesAsync(5).Flatten();
                        Toolbox.uDebugAddLog("Generated message list");
                        foreach (var msg in msgList)
                        {
                            if (msg.Author == Context.User && msg.Timestamp.DateTime > verifyMatch.Timestamp.DateTime && (!recvdMsgs.Contains(msg)))
                            {
                                recvdMsgs.Add(msg);
                                var answer = msg.Content.ToString();
                                Toolbox.uDebugAddLog($"Found newer message from the same author, answer: {answer}");
                                if (answer.ToLower() == "yes")
                                {
                                    verifyRecvd = true;
                                    RPG.MatchList.Remove(match);
                                    Toolbox.uDebugAddLog($"Removed active match due to switching characters [ID]{ownerProfile.OwnerID}");
                                }
                                else if (answer.ToLower() == "no")
                                {
                                    verifyRecvd = true;
                                    await Context.SendDiscordMessage($"{Context.User.Mention} Canceling character switch");
                                    return;
                                }
                                else
                                    await Context.SendDiscordMessage($"{Context.User.Mention} {answer} isn't a valid response, please try again");
                            }
                        }
                        if (verifyMatch.Timestamp.DateTime + TimeSpan.FromMinutes(5) <= DateTime.Now)
                        {
                            await Context.SendDiscordMessage($"{Context.User.Mention} An answer wasn't received within 5 min, canceling character switch...");
                            return;
                        }
                    }
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
                        if ((Context.Message.Author == msg.Author) && (sentMsg.Timestamp.DateTime < msg.Timestamp.DateTime) && (!recvdMsgs.Contains(msg)))
                        {
                            recvdMsgs.Add(msg);
                            answer = msg.Content.ToString();
                            Toolbox.uDebugAddLog("Found message from OP with a newer DateTime than the original message");
                            Toolbox.uDebugAddLog($"Before Response: {answer}");
                            answer = Regex.Replace(msg.Content.ToString(), @"\s+", "");
                            Toolbox.uDebugAddLog($"After response: {answer}");
                            var isNum = int.TryParse(answer, out chosenCharacter);
                            if (!isNum)
                            {
                                await Context.SendDiscordMessage($"{answer} isnt' a valid response");
                                respRecvd = false;
                            }
                            else
                                respRecvd = true;
                        }
                    }
                    if (timeStamp2 + TimeSpan.FromSeconds(60) <= DateTime.Now)
                    {
                        Toolbox.uDebugAddLog($"Response wasn't received from {Context.Message.Author.Username} ({Context.Message.Author.Id}) within 60s, canceled character creation");
                        await Context.SendDiscordMessageMention($"A valid response wasn't received within 60 seconds, canceling creation request");
                        return;
                    }
                }
                Character selChara = ownerProfile.CharacterList[chosenCharacter - 1];
                Management.ChangeCharacter(ownerProfile.OwnerID, selChara);
                await Context.SendDiscordMessageMention($"your active character is now {selChara.Name}!");
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
                if (!Permissions.AdminPermissions(Context))
                {
                    await Context.SendDiscordMessageMention($"You don't have rights to run this command");
                    return;
                }
                OwnerProfile ownerProfile = RPG.Owners.Find(x => x.OwnerID == Context.Message.Author.Id);
                if (ownerProfile == null)
                {
                    OwnerProfile owner = new OwnerProfile() { OwnerID = Context.Message.Author.Id };
                    RPG.Owners.Add(owner);
                    Events.uStatusUpdateExt($"Owner profile not found, created one for {Context.Message.Author.Username} | {Context.Message.Author.Id}");
                    await Context.SendDiscordMessageMention($"you didn't have a profile yet so I made you one");
                }
                else
                    Toolbox.uDebugAddLog($"Owner profile was found for {Context.Message.Author.Username} | {Context.Message.Author.Id}");
                Character testiculees = Testing.testiculeesCharacter;
                ownerProfile.CharacterList.Add(testiculees);
                ownerProfile.CurrentCharacter = testiculees;
                await Context.SendDiscordMessageMention($"you have been granted the power of TESTICULEEEEEES!!!");
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
                if (!Permissions.AdminPermissions(Context))
                {
                    await Context.SendDiscordMessageMention($"You don't have rights to run this command");
                    return;
                }
                OwnerProfile ownerProfile = RPG.Owners.Find(x => x.OwnerID == Context.Message.Author.Id);
                if (ownerProfile != null)
                {
                    RPG.Owners.Remove(ownerProfile);
                    await Context.SendDiscordMessageMention($"your profile has been successfully deleted");
                }
                else
                    await Context.SendDiscordMessageMention($"you don't have a profile to delete");
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
                if (!Permissions.AdminPermissions(Context))
                {
                    await Context.SendDiscordMessageMention($"You don't have rights to run this command");
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
                    await Context.SendDiscordMessage($"{mentionedUser} isn't a valid discord user");
                    return;
                }
                var userFound = await Context.Channel.GetUserAsync(userID);
                if (userFound == null)
                {
                    Toolbox.uDebugAddLog($"Invalid User: {userFound.Username} | {userID}");
                    await Context.SendDiscordMessage($"{userID} doesn't match a discord user on your server");
                    return;
                }
                Toolbox.uDebugAddLog($"MentionedUser: {mentionedUser}");
                OwnerProfile ownerProfile = RPG.Owners.Find(x => x.OwnerID == userFound.Id);
                if (ownerProfile != null)
                {
                    RPG.Owners.Remove(ownerProfile);
                    await Context.SendDiscordMessageMention($"{userFound.Username}'s RPG profile has been deleted");
                }
                else
                    await Context.SendDiscordMessageMention($"{userFound.Username} doesn't have a RPG profile");
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("rpg"), Summary("Testicules toggle rpg channel")]
        public async Task Testacules11()
        {
            try
            {
                if (!Permissions.AdminPermissions(Context))
                {
                    await Context.SendDiscordMessageMention($"You don't have rights to run this command");
                    return;
                }
                if (Permissions.AllowedChannels.Find(x => x.ID == Context.Channel.Id) == null)
                {
                    Toolbox.uDebugAddLog($"Channel isn't an RPG channel, attempting to add RPG Channel: {Context.Channel.Name} | {Context.Channel.Id}");
                    DiscordChannel newChannel = new DiscordChannel() { ID = Context.Channel.Id, Name = Context.Channel.Name };
                    Permissions.AllowedChannels.Add(newChannel);
                    Events.uStatusUpdateExt($"RPG Channel Added: {newChannel.Name} | {newChannel.ID}");
                    await Context.SendDiscordMessageMention($"**Added** RPG Channel **{newChannel.Name}**");
                }
                else
                {
                    var chnl = Permissions.AllowedChannels.Find(x => x.ID == Context.Channel.Id);
                    Toolbox.uDebugAddLog($"Channel is already an RPG channel, attempting to remove RPG Channel: {chnl.Name} | {chnl.ID}");
                    Permissions.AllowedChannels.Remove(chnl);
                    Events.uStatusUpdateExt($"RPG Channel Removed: {Context.Channel.Name} | {Context.Channel.Id}");
                    await Context.SendDiscordMessageMention($" **Removed** RPG Channel **{Context.Channel.Name}**");
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
                    await Context.SendDiscordMessageMention($"this channel is an RPG channel, go nuts!");
                else
                    await Context.SendDiscordMessageMention($"this channel hasn't been enabled as an RPG channel");
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
                    await Context.SendDiscordMessageMention($"you don't currently have any characters, please create one before trying to start a match");
                    return;
                }
                Toolbox.uDebugAddLog($"Starting match command");
                OwnerProfile owner = RPG.Owners.Find(x => x.OwnerID == Context.Message.Author.Id);
                try
                {
                    var match = RPG.MatchList.Find(x => x.Owner == owner);
                    if (match == null)
                    {
                        if (owner.CurrentCharacter.Loot.Count > 0)
                        {
                            await Context.SendDiscordMessage($"You still have {owner.CurrentCharacter.Loot.Count} pieces of loot to go through before you can start another match");
                            return;
                        }
                        Toolbox.uDebugAddLog($"Generating new match for {owner.OwnerID}");
                        Match newMatch = new Match() { Owner = owner, MatchStart = DateTime.Now };
                        int enemyCount = RPG.rng.Next(1, 5);
                        Toolbox.uDebugAddLog($"Enemy Count chosen: {enemyCount}");
                        for (int i = 0; i < enemyCount; i++)
                        {
                            Enemy newEnemy = Enemies.EnemyRanGen(LootDrop.ChooseLevel(owner.CurrentCharacter.Lvl));
                            if (i == 0) { newMatch.CurrentEnemy = newEnemy; Toolbox.uDebugAddLog($"Set {newEnemy.Name} as the current enemy for {owner.OwnerID}"); }
                            newMatch.EnemyList.Add(newEnemy);
                            Toolbox.uDebugAddLog($"Generated enemy {newEnemy.Name} and added to the enemy list for {owner.OwnerID}");
                            Toolbox.uDebugAddLog($"Generating Enemies Progress: [current]{i} [enemyCount]{enemyCount}");
                        }
                        RPG.MatchList.Add(newMatch);
                        Toolbox.uDebugAddLog($"Successfully generated new match with {newMatch.EnemyList.Count} enemies");
                        EmbedBuilder embed = new EmbedBuilder()
                        {
                            Title = $"A new match was generated with **{newMatch.EnemyList.Count}** enemies",
                            Color = owner.CurrentCharacter.Color,
                            Description = $"{owner.CurrentCharacter.Name} vs. {newMatch.CurrentEnemy.Name}"
                        };
                        //embed.AddField(x => { x.Name = "Player Img"; x.IsInline = true; x.Value = owner.CurrentCharacter.ImgURL; });
                        //embed.AddField(x => { x.Name = "Enemy Img"; x.IsInline = true; x.Value = newEnemy.ImgURL; });
                        await Context.SendDiscordEmbed(embed);
                        Toolbox.uDebugAddLog($"Successfully sent new match message to {Context.User.Username} | {Context.User.Id}");
                        await TurnSystem.CalculateTurn(Context, owner);
                        return;
                    }
                    else
                    {
                        Toolbox.uDebugAddLog($"Attempt to generate new match, existing match found for {owner.OwnerID}");
                        TimeSpan time = DateTime.Now - match.MatchStart;
                        TimeSpan timeLeft = (match.LastPlayerTurn + match.TurnTimeLimit) - match.LastPlayerTurn;
                        await Context.SendDiscordMessage($"You currently have an active match with **{match.CurrentEnemy.Name}** that was started **{time.Days}D {time.Hours}H {time.Minutes}M {time.Seconds}Secs** ago, please attack your current enemy, you have **{timeLeft.Days}D {timeLeft.Hours}H {timeLeft.Minutes}M {timeLeft.Seconds}Secs** left before you **forfeit**");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Toolbox.FullExceptionLog(ex);
                }
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("view match"), Summary("Testicules Match View Test")]
        public async Task Testacules13a()
        {
            var hasChar = await VerifyOwnerProfileAndIfHasCharacters();
            if (!hasChar)
            {
                await Context.SendDiscordMessageMention($"you don't currently have any characters, please create one before trying to start a match");
                return;
            }
            Toolbox.uDebugAddLog($"Starting match view command [{Context.User.Id}]");
            await Context.Message.Channel.SendMessageAsync($"{Context.User.Mention} Your current match details:{line}{Management.CheckMatchDetails(Context)}");
            Toolbox.uDebugAddLog($"Finished match view command [{Context.User.Id}]");
        }

        [Command("attack"), Summary("Testicules Attack Test")]
        public async Task Testacules14()
        {
            try
            {
                var hasChar = await VerifyOwnerProfileAndIfHasCharacters();
                if (!hasChar)
                {
                    await Context.SendDiscordMessageMention($"you don't currently have any characters, please create one before trying to attack something");
                    return;
                }
                OwnerProfile owner = RPG.Owners.Find(x => x.OwnerID == Context.Message.Author.Id);
                Match match = RPG.MatchList.Find(x => x.Owner == owner);
                if (match == null)
                {
                    await Context.SendDiscordMessageMention($"you don't currently have an active match, please start a match before trying to attack nothing");
                    return; 
                }
                await Management.AttackEnemy(Context, owner, match.CurrentEnemy);
                await TurnSystem.CalculateTurn(Context, owner);
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
                    await Context.SendDiscordMessageMention($"you don't currently have any characters, please create one before trying to get some of that dank loot");
                    return;
                }
                await Management.EmptyLoot(Context);
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("item"), Summary("Testicules Item Use")]
        public async Task Testacules16()
        {
            try
            {
                var hasChar = await VerifyOwnerProfileAndIfHasCharacters();
                if (!hasChar)
                {
                    await Context.SendDiscordMessageMention($"you don't currently have any characters, please create one before trying to get some of that dank loot");
                    return;
                }
                await Management.CharacterUseItem(Context);
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("view"), Summary("Testicules View Character")]
        public async Task Testacules17()
        {
            try
            {
                var hasChar = await VerifyOwnerProfileAndIfHasCharacters();
                if (!hasChar)
                {
                    await Context.SendDiscordMessageMention($"you don't currently have any characters, please create one before trying to get some of that dank loot");
                    return;
                }
                await Management.CheckCharacterStats(Context);
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("change armor"), Summary("Testicules Change Armor")]
        public async Task Testacules18a()
        {
            try
            {
                await Management.CharacterChangeArmor(Context);
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("change weapon"), Summary("Testicules Change Weapon")]
        public async Task Testacules18w()
        {
            try
            {
                await Management.CharacterChangeWeapon(Context);
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("change description"), Summary("Testicules Change Desc")]
        public async Task Testacules18d()
        {
            try
            {
                await Management.CharacterChangeDescription(Context);
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("add loot"), Summary("Testicules Change Armor")]
        public async Task Testacules19()
        {
            try
            {
                Toolbox.uDebugAddLog($"Starting add loot for testing [ID]{Context.User.Id}");
                var isAdmin = Permissions.AdminPermissions(Context);
                if (!isAdmin)
                {
                    await Context.SendDiscordMessageMention($"You don't have permission to run this command");
                    Toolbox.uDebugAddLog($"User didn't have permission, cancelling [ID]{Context.User.Id}");
                    return;
                }
                Toolbox.uDebugAddLog($"Generating loot for character [ID]{Context.User.Id}");
                await Context.SendDiscordMessageMention($"Generating loot for your character {RPG.Owners.Find(x => x.OwnerID == Context.Message.Author.Id).CurrentCharacter.Name}");
                Match lootMatch = new Match()
                {
                    DefeatedEnemies = new List<Enemy>()
                    {
                        Enemies.punchingBag(),
                        Enemies.punchingBag(),
                        Enemies.punchingBag(),
                        Enemies.punchingBag(),
                        Enemies.punchingBag(),
                        Enemies.punchingBag(),
                        Enemies.punchingBag(),
                        Enemies.punchingBag(),
                        Enemies.punchingBag(),
                        Enemies.punchingBag(),
                    },
                    ExperienceEarned = 1000,
                    MatchStart = DateTime.Now - TimeSpan.FromMinutes(20),
                    Owner = RPG.Owners.Find(x => x.OwnerID == Context.Message.Author.Id),
                    Turns = 10
                };
                Toolbox.uDebugAddLog($"Completing loot match [ID]{Context.User.Id}");
                Events.CompleteMatch(Context, lootMatch.Owner, lootMatch, DateTime.Now - lootMatch.MatchStart, RPG.MatchCompleteResult.Won);
                Toolbox.uDebugAddLog($"Loot match complete and loot given [ID]{Context.User.Id}");
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("testing"), Summary("Testicules Add Testing Group")]
        public async Task Testacules20(string testingRoleString)
        {
            try
            {
                var isAdmin = Permissions.AdminPermissions(Context);
                if (!isAdmin)
                {
                    await Context.SendDiscordMessageMention($"You don't have permission to run this command");
                }
                var testingRole = await GetDiscordRole(testingRoleString);
                if (testingRole == null)
                    return;
                var tstChannel = Permissions.TestingGroups.Find(x => x.ID == testingRole.Id);
                if (tstChannel == null)
                {
                    Permissions.TestingGroups.Add(new DiscordUser() { ID = testingRole.Id, Username = testingRole.Name });
                    await Context.SendDiscordMessageMention($"**Added** testing permission to **{testingRole.Name}**");
                    Toolbox.uDebugAddLog($"Added role as a testing group: {testingRole.Name} | {testingRole.Id} [ID]{Context.Message.Author.Id}");
                }
                else
                {
                    Permissions.TestingGroups.Remove(tstChannel);
                    await Context.SendDiscordMessageMention($"**Removed** testing permission from **{testingRole.Name}**");
                    Toolbox.uDebugAddLog($"Removed role as a testing group: {testingRole.Name} | {testingRole.Id} [ID]{Context.Message.Author.Id}");
                }
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("roles"), Summary("Testicules check roles")]
        public async Task Testacules21(string mentionedUser)
        {
            try
            {
                var user = await GetDiscordUser(mentionedUser);
                if (user == null)
                    return;
                var roleString = string.Empty;
                foreach (var id in ((SocketGuildUser)user).Roles)
                    roleString = $"{roleString}{id}{Environment.NewLine}";
                await Context.SendDiscordMessage($"Roles for {user.Username}:{Environment.NewLine}{roleString}");
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("inrole"), Summary("Testicules check role")]
        public async Task Testacules22(string mentionedUser, string mentionedRole)
        {
            try
            {
                var user = await GetDiscordUser(mentionedUser);
                if (user == null)
                    return;
                var role = await GetDiscordRole(mentionedRole);
                if (role == null)
                    return;
                if (((SocketGuildUser)user).Roles.Contains(role))
                    await Context.SendDiscordMessage($"{user.Username} is in {role.Mention}");
                else
                    await Context.SendDiscordMessage($"{user.Username} is not in {role.Mention}");
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("log channel"), Summary("Testicules add log channel")]
        public async Task Testacules23(string mentionedChannel)
        {
            try
            {
                if (!Permissions.AdminPermissions(Context))
                {
                    await Context.SendDiscordMessage($"{Context.User.Mention} You don't have permissions to use this command");
                    return;
                }
                var channel = await GetDiscordChannel(mentionedChannel);
                if (channel == null)
                    return;
                if (Permissions.GeneralPermissions.logChannel != channel.Id)
                {
                    Permissions.GeneralPermissions.logChannel = channel.Id;
                    Toolbox.uDebugAddLog($"Added {channel.Name} | {channel.Id} as the logging channel [ID]{Context.User.Id}");
                    await Context.SendDiscordMessage($"{Context.User.Mention} The **{channel.Name}** channel has been marked as the logging channel");
                }
                else
                {
                    Permissions.GeneralPermissions.logChannel = 0;
                    Toolbox.uDebugAddLog($"Removed {channel.Name} | {channel.Id} as the logging channel [ID]{Context.User.Id}");
                    await Context.SendDiscordMessage($"{Context.User.Mention} The **{channel.Name}** channel has been removed as the logging channel");
                }
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("check backpack"), Summary("Testicules checks the loot in his backpack")]
        public async Task Testacules24()
        {
            try
            {
                var hasChar = await VerifyOwnerProfileAndIfHasCharacters();
                if (!hasChar)
                {
                    await Context.SendDiscordMessageMention($"you don't currently have any characters, please create one before trying to view your phat loot");
                    return;
                }
                await Management.CheckCharacterBackpack(Context);
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("change color"), Summary("Testicules Change Color")]
        public async Task Testacules25(string color)
        {
            try
            {
                var hasChar = await VerifyOwnerProfileAndIfHasCharacters();
                if (!hasChar)
                    return;
                if (color.Contains(","))
                {
                    Toolbox.uDebugAddLog($"Changing character color, string contains comma [ID]{Context.User.Id}");
                    var split = color.Split(',');
                    if (split.Length <= 2)
                    {
                        await Context.SendDiscordMessage($"You didn't enter enough numbers to make an RGB color, please try again [entered]{color}");
                        Toolbox.uDebugAddLog($"Incorrect arguments (split.Length <= 2) for RGB color entered: {color} [ID]{Context.User.Id}");
                        return;
                    }
                    if (split.Length > 3)
                    {
                        await Context.SendDiscordMessage($"You entered too many arguments to create an RGB color, you need 3 arguments, please try again [entered]{color}");
                        Toolbox.uDebugAddLog($"Incorrect arguments (split.Length > 3) for RGB color entered: {color} [ID]{Context.User.Id}");
                        return;
                    }
                    int num1 = 0;
                    int num2 = 0;
                    int num3 = 0;
                    var isNum1 = int.TryParse(split[0], out num1);
                    if (!isNum1)
                    {
                        await Context.SendDiscordMessage($"The number you entered for argument 1 isn't a valid integer: {split[0]}");
                        Toolbox.uDebugAddLog($"Argument 1 isn't a valid integer: {split[0]} [ID]{Context.User.Id}");
                        return;
                    }
                    var isNum2 = int.TryParse(split[1], out num2);
                    if (!isNum2)
                    {
                        await Context.SendDiscordMessage($"The number you entered for argument 2 isn't a valid integer: {split[1]}");
                        Toolbox.uDebugAddLog($"Argument 2 isn't a valid integer: {split[1]} [ID]{Context.User.Id}");
                        return;
                    }
                    var isNum3 = int.TryParse(split[2], out num3);
                    if (!isNum3)
                    {
                        await Context.SendDiscordMessage($"The number you entered for argument 3 isn't a valid integer: {split[2]}");
                        Toolbox.uDebugAddLog($"Argument 3 isn't a valid integer: {split[2]} [ID]{Context.User.Id}");
                        return;
                    }
                    Discord.Color newColor = new Discord.Color(num1, num2, num3);
                    await Management.ChangeColor(Context, newColor);
                }
                else
                {
                    Toolbox.uDebugAddLog($"Changing character color, string doesn't contain comma [ID]{Context.User.Id}");
                    color = color.ToLower();
                    Discord.Color newColor = new Color(0,0,0);
                    System.Windows.Media.Color selColor = System.Windows.Media.Colors.Blue;
                    switch (color)
                    {
                        case "red":
                            selColor = System.Windows.Media.Colors.Red;
                            break;
                        case "blue":
                            selColor = System.Windows.Media.Colors.Blue;
                            break;
                        case "black":
                            selColor = System.Windows.Media.Colors.Black;
                            break;
                        case "green":
                            selColor = System.Windows.Media.Colors.Green;
                            break;
                        case "yellow":
                            selColor = System.Windows.Media.Colors.Yellow;
                            break;
                        case "brown":
                            selColor = System.Windows.Media.Colors.Brown;
                            break;
                        case "orange":
                            selColor = System.Windows.Media.Colors.Orange;
                            break;
                        case "gold":
                            selColor = System.Windows.Media.Colors.Gold;
                            break;
                        case "pink":
                            selColor = System.Windows.Media.Colors.Pink;
                            break;
                        case "purple":
                            selColor = System.Windows.Media.Colors.Purple;
                            break;
                        case "silver":
                            selColor = System.Windows.Media.Colors.Silver;
                            break;
                        case "slategray":
                            selColor = System.Windows.Media.Colors.SlateGray;
                            break;
                        case "white":
                            selColor = System.Windows.Media.Colors.White;
                            break;
                        default:
                            await Context.SendDiscordMessage($"{color} is an incorrect color, please try again");
                            return;
                    }
                    newColor = new Color(selColor.R, selColor.G, selColor.B);
                    await Management.ChangeColor(Context, newColor);
                }
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        [Command("change currency"), Summary("Testicules Change Currency Name")]
        public async Task Testacules26(string newCurrencyName)
        {
            Toolbox.uDebugAddLog("Starting change currency name command");
            var cacheCurrencyName = Toolbox._paths.CurrencyName.ToString();
            var isAdmin = Permissions.AdminPermissions(Context);
            if (!isAdmin)
            {
                await Context.SendDiscordMessageMention($"You don't have permission to run this command");
                Toolbox.uDebugAddLog($"User didn't have permission, cancelling [ID]{Context.User.Id}");
                return;
            }
            Management.UpdateCurrency(newCurrencyName);
            await Context.SendDiscordMessageMention($"Currency name updated from **{cacheCurrencyName}** to **{Toolbox._paths.CurrencyName}**");
            Toolbox.uDebugAddLog("Finished change currency name command");
        }

        public async Task<IUser> GetDiscordUser(string user)
        {
            IUser userFound = null;
            Toolbox.uDebugAddLog($"Before Removing '<,@,>': {user}");
            user = user.Replace("<", string.Empty).Replace("@", string.Empty).Replace(">", string.Empty);
            Toolbox.uDebugAddLog($"After Removing '<,@,>': {user}");
            ulong userID = 0;
            var isUlong = ulong.TryParse(user, out userID);
            if (!isUlong)
            {
                Toolbox.uDebugAddLog($"Invalid Ulong: {userID}");
                await Context.SendDiscordMessage($"{user} isn't a valid discord user");
                userID = 0;
                return userFound;
            }
            userFound = await Context.Channel.GetUserAsync(userID);
            if (userFound == null)
            {
                Toolbox.uDebugAddLog($"Invalid User: {userFound.Username} | {userID}");
                await Context.SendDiscordMessage($"{userID} doesn't match a discord user on your server");
                userID = 0;
                return userFound;
            }
            return userFound;
        }

        public async Task<IRole> GetDiscordRole(string role)
        {
            IRole roleFound = null;
            Toolbox.uDebugAddLog($"Before Removing '<,@,>,&': {role}");
            role = role.Replace("<", string.Empty).Replace("@", string.Empty).Replace(">", string.Empty).Replace("&", string.Empty);
            Toolbox.uDebugAddLog($"After Removing '<,@,>,&': {role}");
            ulong roleID = 0;
            var isUlong = ulong.TryParse(role, out roleID);
            if (!isUlong)
            {
                Toolbox.uDebugAddLog($"Invalid Ulong: {roleID}");
                await Context.SendDiscordMessage($"{role} isn't a valid discord role");
                roleID = 0;
                return roleFound;
            }
            roleFound = Context.Guild.GetRole(roleID);
            if (roleFound == null)
            {
                Toolbox.uDebugAddLog($"Invalid Role: {roleFound.Name} | {roleID}");
                await Context.SendDiscordMessage($"{roleID} doesn't match a discord role on your server");
                roleID = 0;
                return roleFound;
            }
            return roleFound;
        }

        public async Task<IGuildChannel> GetDiscordChannel(string channel)
        {
            IGuildChannel channelFound = null;
            Toolbox.uDebugAddLog($"Before Removing '<,@,>,#': {channel}");
            channel = channel.Replace("<", string.Empty).Replace("@", string.Empty).Replace(">", string.Empty).Replace("#", string.Empty);
            Toolbox.uDebugAddLog($"After Removing '<,@,>,#': {channel}");
            ulong channelID = 0;
            var isUlong = ulong.TryParse(channel, out channelID);
            if (!isUlong)
            {
                Toolbox.uDebugAddLog($"Invalid Ulong: {channelID}");
                await Context.SendDiscordMessage($"{channel} isn't a valid discord channel");
                channelID = 0;
                return channelFound;
            }
            channelFound = await Context.Guild.GetChannelAsync(channelID);
            if (channelFound == null)
            {
                Toolbox.uDebugAddLog($"Invalid Channel: {channelFound.Name} | {channelID}");
                await Context.SendDiscordMessage($"{channelID} doesn't match a discord channel on your server");
                channelID = 0;
                return channelFound;
            }
            return channelFound;
        }

        public async Task<bool> VerifyOwnerProfileAndIfHasCharacters()
        {
            OwnerProfile ownerProfile = RPG.Owners.Find(x => x.OwnerID == Context.Message.Author.Id);
            if (ownerProfile == null)
            {
                OwnerProfile owner = new OwnerProfile() { OwnerID = Context.Message.Author.Id, OwnerUN = Context.User.Username };
                RPG.Owners.Add(owner);
                Events.uStatusUpdateExt($"Owner profile not found, created one for {Context.Message.Author.Username} | {Context.Message.Author.Id}");
                await Context.SendDiscordMessageMention($"you didn't have a profile yet so I made you one");
                ownerProfile = RPG.Owners.Find(x => x.OwnerID == Context.Message.Author.Id);
            }
            else
                Toolbox.uDebugAddLog($"Owner profile was found for {Context.Message.Author.Username} | {Context.Message.Author.Id}");
            return ownerProfile.CharacterList.Count == 0 ? false : true;
        }

        public async Task<bool> HasTestingPermission(ICommandContext context)
        {
            await Context.SendDiscordMessage("");
            return true;
            //bool hasPerm = false;
            //string testingGroups = string.Empty;
            //if (Permissions.Administrators.Contains(context.User.Id))
            //    hasPerm = true;
            //foreach (var role in Permissions.TestingGroups)
            //{
            //    testingGroups = $"{testingGroups}{role}{Environment.NewLine}";
            //    if (((SocketGuildUser)context.User).RoleIds.Contains(role))
            //        hasPerm = true;
            //}
            //if (!hasPerm)
            //    await context.Channel.SendMessageAsync($"{context.User.Mention} You don't have access to run this command, you need to be in one of the below: {Environment.NewLine}{testingGroups}");
            //return hasPerm;
        }
    }
    #endregion
}