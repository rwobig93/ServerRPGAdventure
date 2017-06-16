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

        #region MatchComplete Event

        public delegate void MatchComplete(MatchArgs args);
        public static event MatchComplete MatchCompleted;
        public static void CompleteMatch(int enemyCount, int experienceEarned, TimeSpan matchTime, OwnerProfile owner)
        {
            MatchArgs args = new MatchArgs(enemyCount, experienceEarned, matchTime, owner);
            MatchCompleted(args);
            Toolbox.uDebugAddLog($"MatchCompleted Event Triggered: [EC]{enemyCount} [EXP]{experienceEarned} [T]{matchTime.Days}D {matchTime.Hours}H {matchTime.Seconds}S [O]{owner.OwnerID}");
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
        private OwnerProfile owner;
        private int enemyCount;
        private int experienceEarned;
        private TimeSpan matchTime;
        public MatchArgs(int enemies, int exp, TimeSpan time, OwnerProfile owner)
        {
            this.owner = owner;
            this.enemyCount = enemies;
            this.experienceEarned = exp;
            this.matchTime = time;
        }
        public OwnerProfile Owner { get { return owner; } }
        public int EnemyCount { get { return enemyCount; } }
        public int ExperienceEarned { get { return experienceEarned; } }
        public TimeSpan MatchTime { get { return matchTime; } }
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
