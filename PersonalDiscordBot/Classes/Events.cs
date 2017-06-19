using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalDiscordBot.Classes
{
    public class Events
    {
        #region UpdateStatus Event

        public delegate void MessageShown(PromptArgs args);
        public static event MessageShown MessagePromptShown;
        public static void uStatusUpdateExt(string status)
        {
            PromptArgs args = new PromptArgs(status);
            MessagePromptShown(args);
        }

        #endregion

        #region DiscordMessage Event

        public delegate void DiscordMessage(MessageArgs args);
        public static event DiscordMessage DiscordMessageSend;
        public static void SendDiscordMessage(CommandContext context, string message)
        {
            MessageArgs args = new MessageArgs(context, message);
            DiscordMessageSend(args);
            Toolbox.uDebugAddLog($"Sent Discord Message via Event: [UN]{context.Message.Author.Username} [MSG]{message} [ID]{context.Message.Author.Id}");
        }

        #endregion

        #region MatchComplete Event

        public delegate void MatchComplete(MatchArgs args);
        public static event MatchComplete MatchCompleted;
        public static void CompleteMatch(CommandContext context, OwnerProfile owner, Match match, TimeSpan matchTime, RPG.MatchCompleteResult result)
        {
            MatchArgs args = new MatchArgs(context, owner, match, matchTime, result);
            MatchCompleted(args);
            Toolbox.uDebugAddLog($"MatchCompleted Event Triggered: [R]{result} [EC]{match.DefeatedEnemies.Count} [EXP]{match.ExperienceEarned} [T]{matchTime.Days}D {matchTime.Hours}H {matchTime.Seconds}S [O]{owner.OwnerID}");
        }
        public static void CompleteMatch(OwnerProfile owner, Match match, TimeSpan matchTime, RPG.MatchCompleteResult result)
        {
            MatchArgs args = new MatchArgs(owner, match, matchTime, result);
            MatchCompleted(args);
            Toolbox.uDebugAddLog($"MatchCompleted Event Triggered: [R]{result} [EC]{match.DefeatedEnemies.Count} [EXP]{match.ExperienceEarned} [T]{matchTime.Days}D {matchTime.Hours}H {matchTime.Seconds}S [O]{owner.OwnerID}");
        }

        #endregion

        #region TurnChanged Event

        public delegate void TurnChanged(TurnArgs args);
        public static event TurnChanged MatchTurnChanged;
        public static void ChangedTurn(RPG.Turn newTurn, RPG.Turn oldTurn, ulong ownerID)
        {
            TurnArgs args = new TurnArgs(newTurn, oldTurn, ownerID);
            MatchTurnChanged(args);
            Toolbox.uDebugAddLog($"Match Turn Changed from {oldTurn} to {newTurn} | OwnerID: {ownerID}");
        } 

        #endregion
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
        private CommandContext context;
        private OwnerProfile owner;
        private Match match;
        private TimeSpan matchTime;
        private RPG.MatchCompleteResult result;
        public MatchArgs(OwnerProfile owner, Match match, TimeSpan time, RPG.MatchCompleteResult result)
        {
            this.owner = owner;
            this.match = match;
            this.matchTime = time;
            this.result = result;
        }
        public MatchArgs(CommandContext context, OwnerProfile owner, Match match, TimeSpan time, RPG.MatchCompleteResult result)
        {
            this.context = context;
            this.owner = owner;
            this.match = match;
            this.matchTime = time;
            this.result = result;
        }
        public CommandContext Context { get { return context; } }
        public OwnerProfile Owner { get { return owner; } }
        public Match Match { get { return match; } }
        public TimeSpan MatchTime { get { return matchTime; } }
        public RPG.MatchCompleteResult Result { get { return result; } }
    }

    public class MessageArgs : EventArgs
    {
        private CommandContext context;
        private string message;
        public MessageArgs(CommandContext context, string message)
        {
            this.context = context;
            this.message = message;
        }
        public CommandContext Context { get { return context; } }
        public string Message { get { return message; } }
    }

    public class TurnArgs : EventArgs
    {
        private RPG.Turn newTurn;
        private RPG.Turn oldTurn;
        private ulong ownerID;
        public TurnArgs(RPG.Turn newTurn, RPG.Turn oldTurn, ulong ownerID)
        {
            this.newTurn = newTurn;
            this.oldTurn = oldTurn;
            this.ownerID = ownerID;
        }
        public RPG.Turn NewTurn { get { return newTurn; } }
        public RPG.Turn OldTurn { get { return oldTurn; } }
        public ulong OwnerID { get { return ownerID; } }
    }
}
