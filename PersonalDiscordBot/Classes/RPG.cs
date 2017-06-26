using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using static PersonalDiscordBot.Classes.RPG;

namespace PersonalDiscordBot.Classes
{

    #region Base Classes

    public class OwnerProfile
    {
        public ulong OwnerID { get; set; }
        public int Currency { get; set; } = 100;
        public int TotalPebbles { get; set; } = 0;
        public List<Character> CharacterList = new List<Character>();
        public Character CurrentCharacter { get; set; }
        public IDidTheThingOwner ThingsDone = new IDidTheThingOwner();
    }

    public class Character
    {
        public ulong OwnerID { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; } = "A new adventurer set out to..... Adventure?";
        public CharacterClass Class { get; set; }
        public Weapon Weapon { get; set; }
        public Weapon StarterWeapon { get { return Weapons.GetStarterWeapon(this); } }
        public Armor Armor { get; set; }
        public BackPack Backpack { get; set; }
        public List<Spell> SpellBook = new List<Spell>();
        public List<IBackPackItem> Loot = new List<IBackPackItem>();
        public IDidTheThingPlayer ThingsDone = new IDidTheThingPlayer();
        public List<Affliction> StatusEffects = new List<Affliction>();
        public int Pebbles { get; set; } = 0;
        public int Lvl { get; set; }
        public int Exp { get; set; }
        public int MaxHP { get; set; }
        public int CurrentHP { get; set; }
        public int MaxMana { get; set; }
        public int CurrentMana { get; set; }
        public int Str { get; set; }
        public int Def { get; set; }
        public int Dex { get; set; }
        public int Int { get; set; }
        public int Spd { get; set; }
        public int Lck { get; set; }
    }

    public class Match
    {
        public OwnerProfile Owner { get; set; }
        public DateTime MatchStart { get; set; }
        public DateTime LastPlayerTurn { get; set; }
        public TimeSpan TurnTimeLimit { get; set; } = TimeSpan.FromDays(3);
        public List<Enemy> EnemyList { get; set; } = new List<Enemy>();
        public List<Enemy> DefeatedEnemies { get; set; } = new List<Enemy>();
        public Enemy CurrentEnemy { get; set; }
        public Turn CurrentTurn { get; set; } = Turn.NotChosen;
        public int Turns { get; set; }
        public int EnemyTurnTime { get; set; }
        public int PlayerTurnTime { get; set; }
        public int EnemySpeedTime { get; set; }
        public int PlayerSpeedTime { get; set; }
        public int ExperienceEarned { get; set; }
    }

    public class PlayerMatch
    {
        public Character Player1 { get; set; }
        public Character Player2 { get; set; }
        public Turn CurrentTurn { get; set; }
        public TimeSpan TurnTimeLimit { get; set; } = TimeSpan.FromDays(1);
    }

    public class Enemy
    {
        public string Name { get; set; }
        public string Desc { get; set; } = "I want your bod, not in a sexual or romantic way. But more of a dead and ragdoll kinda way";
        public Weapon Weapon { get; set; }
        public Armor Armor { get; set; }
        public List<Affliction> StatusEffects = new List<Affliction>();
        public int Lvl { get; set; }
        public int ExpLoot { get; set; }
        public int MaxHP { get; set; }
        public int CurrentHP { get; set; }
        public int Mana { get; set; }
        public int Str { get; set; }
        public int Def { get; set; }
        public int Dex { get; set; }
        public int Int { get; set; }
        public int Spd { get; set; }
        public int Lck { get; set; }
    }

    public class Boss : Enemy
    {
        public List<IBackPackItem> Loot { get; set; } = new List<IBackPackItem>();
    }

    public class BackPack
    {
        public string Name { get; set; } = "BackPack";
        public string Desc { get; set; } = "Your trusty friend you shove shit into";
        public int Capacity { get; set; } = 10;
        public int Weight { get; set; } = 10;
        public List<IBackPackItem> Stored = new List<IBackPackItem>();
    }

    public class Weapon : IBackPackItem
    {
        public string Name { get; set; }
        public string Desc { get; set; } = "A weapon like any other";
        public WeaponType Type { get; set; }
        public RarityType Rarity { get; set; } = RarityType.Common;
        public bool IsUnique { get; set; } = false;
        public int Lvl { get; set; } = 0;
        public int MaxDurability { get; set; } = 0;
        public int CurrentDurability { get; set; } = 0;
        public int Worth { get; set; } = 0;
        public int Speed { get; set; } = 100;
        public int PhysicalDamage { get; set; } = 0;
        public int MagicDamage { get; set; } = 0;
        public int FireDamage { get; set; } = 0;
        public int LightningDamage { get; set; } = 0;
        public int IceDamage { get; set; } = 0;
        public int WindDamage { get; set; } = 0;
        public IBackPackItem GetItem()
        {
            //Name = this.Name;
            //Desc = this.Desc;
            //Type = this.Type;
            //Rarity = this.Rarity;
            //IsUnique = this.IsUnique;
            //Lvl = this.Lvl;
            //MaxDurability = this.MaxDurability;
            //CurrentDurability = this.CurrentDurability;
            //Worth = this.Worth;
            //Speed = this.Speed;
            //PhysicalDamage = this.PhysicalDamage;
            //MagicDamage = this.MagicDamage;
            //FireDamage = this.FireDamage;
            //LightningDamage = this.LightningDamage;
            //IceDamage = this.IceDamage;
            //WindDamage = this.WindDamage;
            return this;
        }
        public LootDrop.LootType GetLootType()
        {
            return LootDrop.LootType.Weapon;
        }
    }

    public class Spell : IBackPackItem
    {
        public string Name { get; set; }
        public string Desc { get; set; } = "What is this? Magic or something?";
        public int Lvl { get; set; } = 0;
        public int Worth { get; set; } = 0;
        public SpellType Type { get; set; } = SpellType.Attack;
        public RarityType Rarity { get; set; } = RarityType.Common;
        public bool IsUnique { get; set; } = false;
        public int ManaCost { get; set; } = 1;
        public int Speed { get; set; } = 100;
        public int PhysicalDamage { get; set; } = 0;
        public int MagicDamage { get; set; } = 0;
        public int FireDamage { get; set; } = 0;
        public int LightningDamage { get; set; } = 0;
        public int IceDamage { get; set; } = 0;
        public int WindDamage { get; set; } = 0;
        public IBackPackItem GetItem()
        {
            //Name = this.Name;
            //Desc = this.Desc;
            //Lvl = this.Lvl;
            //Worth = this.Worth;
            //Type = this.Type;
            //Rarity = this.Rarity;
            //IsUnique = this.IsUnique;
            //ManaCost = this.ManaCost;
            //Speed = this.Speed;
            //PhysicalDamage = this.PhysicalDamage;
            //MagicDamage = this.MagicDamage;
            //FireDamage = this.FireDamage;
            //LightningDamage = this.LightningDamage;
            //IceDamage = this.IceDamage;
            //WindDamage = this.WindDamage;
            return this;
        }
        public LootDrop.LootType GetLootType()
        {
            return LootDrop.LootType.Spell;
        }
    }

    public class Item : IBackPackItem
    {
        public string Name { get; set; }
        public string Desc { get; set; }
        public ItemType Type { get; set; }
        public RarityType Rarity { get; set; } = RarityType.Common;
        public bool IsUnique { get; set; } = false;
        public int Lvl { get; set; } = 0; //used to calculate amount of Repair
        public int Count { get; set; } = 1;
        public int Worth { get; set; } = 0;
        public int Physical { get; set; } = 0;
        public int Magic { get; set; } = 0;
        public int Fire { get; set; } = 0;
        public int Lightning { get; set; } = 0;
        public int Ice { get; set; } = 0;
        public int Wind { get; set; } = 0;
        public IBackPackItem GetItem()
        {
            //Name = this.Name;
            //Desc = this.Desc;
            //Type = this.Type;
            //Lvl = this.Lvl;
            //Count = this.Count;
            //Worth = this.Worth;
            //Physical = this.Physical;
            //Magic = this.Magic;
            //Fire = this.Magic;
            //Lightning = this.Lightning;
            //Ice = this.Ice;
            //Wind = this.Wind;
            return this;
        }
        public LootDrop.LootType GetLootType()
        {
            return LootDrop.LootType.Item;
        }
    }

    public class Armor : IBackPackItem
    {
        public string Name { get; set; }
        public string Desc { get; set; } = "An armor like any other";
        public ArmorType Type { get; set; }
        public RarityType Rarity { get; set; }
        public bool IsUnique { get; set; } = false;
        public int Lvl { get; set; } = 0;
        public int MaxDurability { get; set; } = 0;
        public int CurrentDurability { get; set; } = 0;
        public int Worth { get; set; } = 0;
        public int Speed { get; set; } = 100;
        public int Physical { get; set; } = 0;
        public int Magic { get; set; } = 0;
        public int Fire { get; set; } = 0;
        public int Lightning { get; set; } = 0;
        public int Ice { get; set; } = 0;
        public int Wind { get; set; } = 0;
        public IBackPackItem GetItem()
        {
            //Name = this.Name;
            //Desc = this.Desc;
            //Type = this.Type;
            //Rarity = this.Rarity;
            //IsUnique = this.IsUnique;
            //Lvl = this.Lvl;
            //MaxDurability = this.MaxDurability;
            //CurrentDurability = this.CurrentDurability;
            //Worth = this.Worth;
            //Speed = this.Speed;
            //Physical = this.Physical;
            //Magic = this.Magic;
            //Fire = this.Fire;
            //Lightning = this.Lightning;
            //Ice = this.Ice;
            //Wind = this.Wind;
            return this;
        }
        public LootDrop.LootType GetLootType()
        {
            return LootDrop.LootType.Armor;
        }
    }

    public class Pebble : IBackPackItem
    {
        public int Count { get; set; }
        public IBackPackItem GetItem()
        {
            //Count = this.Count;
            return this;
        }
        public LootDrop.LootType GetLootType()
        {
            return LootDrop.LootType.Nothing;
        }
        public string Name { get; set; } = "Pebble";
        public string Desc { get; set; }
        public RarityType Rarity { get; set; } = RarityType.Common;
        public bool IsUnique { get; set; } = false;
        public int Lvl { get; set; } = 0;
        public int Worth { get; set; } = 0;
    }

    public class IDidTheThingOwner
    {
        // General Stats
        public int FightsTotal { get; set; } = 0;
        public int FightsWon { get; set; } = 0;
        public int FigthsLost { get; set; } = 0;
        public int PlayerFightsTotal { get; set; } = 0;
        public int PlayerFightsWon { get; set; } = 0;
        public int PlayerFightsLost { get; set; } = 0;
        public int BossesBeat { get; set; } = 0;
        public int TotalCharactersMade { get; set; } = 0;

        // Things that have been done (cheevos)
        public bool FirstFight { get; set; } = false;
        public bool FirstBoss { get; set; } = false;
        public bool FirstPlayerFight { get; set; } = false;
    }

    public class IDidTheThingPlayer
    {
        // General Stats
        public int FightsTotal { get; set; } = 0;
        public int FightsWon { get; set; } = 0;
        public int FigthsLost { get; set; } = 0;
        public int PlayerFightsTotal { get; set; } = 0;
        public int PlayerFightsWon { get; set; } = 0;
        public int PlayerFightsLost { get; set; } = 0;
        public int BossesBeat { get; set; } = 0;

        // Things that have been done (cheevos)
        public bool FirstFight { get; set; } = false;
        public bool FirstBoss { get; set; } = false;
        public bool FirstPlayerFight { get; set; } = false;
    }

    public class Affliction
    {
        public string Name { get; set; }
        public bool Positive { get; set; }
        public int TurnsActive { get; set; }
        public int TurnStarted { get; set; }
        public int Physical { get; set; } = 0;
        public int Magic { get; set; } = 0;
        public int Fire { get; set; } = 0;
        public int Lightning { get; set; } = 0;
        public int Ice { get; set; } = 0;
        public int Wind { get; set; } = 0;
    }

    #endregion

    public static class RPG
    {
        #region Variables

        public interface IBackPackItem
        {
            IBackPackItem GetItem();
            LootDrop.LootType GetLootType();
            string Name { get; set; }
            string Desc { get; set; }
            RarityType Rarity { get; set; }
            bool IsUnique { get; set; }
            int Lvl { get; set; }
            int Worth { get; set; }
        };
        public static Random rng = new Random((int)(DateTime.Now.Ticks & 0x7FFFFFFF));
        public static int maxLevel = 20;
        public static List<OwnerProfile> Owners = new List<OwnerProfile>();
        public static List<Match> MatchList = new List<Match>();
        public static List<PlayerMatch> PlayerMatchList = new List<PlayerMatch>();

        #endregion

        #region Methods

        /// <summary>
        /// Returns true or false if random gen number is greater or equal to the chance entered (1-99) to be true
        /// </summary>
        /// <param name="trueChance">Percent that returns true (ex: 30 would mean 30% chance of returning true)</param>
        /// <returns></returns>
        public static bool ChanceRoll(int trueChance)
        {
            if (trueChance > 100 || trueChance < 0)
                throw new ArgumentOutOfRangeException("trueChance", "int was above or below argument range");
            return rng.Next(0, 100) >= (100 - trueChance);
        }

        #endregion

        #region Enums

        public enum CharacterClass
        {
            Warrior,
            Mage,
            Rogue,
            Necromancer,
            Dragoon
        }

        public enum WeaponType
        {
            Sword,
            Dagger,
            Greatsword,
            Katana,
            Staff,
            FocusStone,
            Spear,
            DragonSpear,
            TwinSwords,
            Other,
            Starter
            //switch (type)
            // {
            //     case WeaponType.Dagger:
            //         break;
            //     case WeaponType.DragonSpear:
            //         break;
            //     case WeaponType.FocusStone:
            //         break;
            //     case WeaponType.Greatsword:
            //         break;
            //     case WeaponType.Katana:
            //         break;
            //     case WeaponType.Spear:
            //         break;
            //     case WeaponType.Staff:
            //         break;
            //     case WeaponType.Sword:
            //         break;
            //     case WeaponType.TwinSwords:
            //         break;
            // }
        }

        public enum SpellType
        {
            Attack,
            Defense,
            Restorative,
            Starter
        }

        public enum ElementType
        {
            Physical,
            Magic,
            Fire,
            Lightning,
            Ice,
            Wind
        }

        public enum ItemType
        {
            Restorative,
            Buff,
            Damaging,
            Currency,
            Repair
        }

        public enum ArmorType
        {
            Light,
            Medium,
            Heavy
        }

        public enum RarityType
        {
            Common,
            Uncommon,
            Rare,
            Epic,
            Legendary
            //case RarityType.Common:
            //    break;
            //case RarityType.Uncommon:
            //    break;
            //case RarityType.Rare:
            //    break;
            //case RarityType.Epic:
            //    break;
            //case RarityType.Legendary:
            //    break;
        }

        public enum Turn
        {
            Player,
            Enemy,
            Player1,
            Player2,
            NotChosen
        }

        public enum EnemyTier
        {
            Standard,
            Armored,
            Honed,
            BloodStarved,
            Sickly,
            HyperSensitive
        }

        public enum EnemyType
        {
            Goblin,
            Troll,
            Knight,
            DarkKnight,
            Bear,
            MattPegler
        }

        public enum MatchCompleteResult
        {
            Won,
            Lost,
            Forfeit
        }

        #endregion
    }

    public static class Management
    {
        #region General Methods

        public static void SerializeData()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.RunWorkerCompleted += (sender, e) => { Events.uStatusUpdateExt($"Owner Serializing Complete"); };
            worker.DoWork += (sender, e) =>
            {
                try
                {
                    Events.uStatusUpdateExt("Serializing Save Data");
                    string savePath = $@"{Directory.GetCurrentDirectory()}\SaveData";
                    if (!Directory.Exists(savePath))
                    {
                        Directory.CreateDirectory(savePath);
                        Toolbox.uDebugAddLog($"SaveData folder created: {savePath}");
                    }
                    else
                        Toolbox.uDebugAddLog($"SaveData already exists: {savePath}");
                    foreach (OwnerProfile owner in Owners)
                    {
                        var json = JsonConvert.SerializeObject(owner, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, TypeNameHandling = TypeNameHandling.Objects });
                        File.WriteAllText($@"{savePath}\{owner.OwnerID}.owner", json);
                        Toolbox.uDebugAddLog($"Serialized Owner: {owner.OwnerID}");
                    }
                }
                catch (Exception ex)
                {
                    Toolbox.FullExceptionLog(ex);
                }
            };
            worker.RunWorkerAsync();
        }

        public static void DeSerializeData()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.RunWorkerCompleted += (sender, e) => { Events.uStatusUpdateExt($"Owner Deserialization Complete"); };
            worker.DoWork += (sender, e) =>
            {
                try
                {
                    Events.uStatusUpdateExt("Deserializing Sava Data");
                    string loadPath = $@"{Directory.GetCurrentDirectory()}\SaveData";
                    if (!Directory.Exists(loadPath))
                    {
                        Toolbox.uDebugAddLog($"SaveData folder doesn't exist, stopping deserialization: {loadPath}");
                        return;
                    }
                    foreach (var file in Directory.EnumerateFiles(loadPath))
                    {
                        var info = new FileInfo(file);
                        if (info.Extension.ToLower() == ".owner")
                        {
                            Toolbox.uDebugAddLog($"Found .owner file: {file}");
                            using (StreamReader sr = File.OpenText(file))
                            {
                                OwnerProfile owner = JsonConvert.DeserializeObject<OwnerProfile>(sr.ReadToEnd(), new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, TypeNameHandling = TypeNameHandling.Objects });
                                Owners.Add(owner);
                                Toolbox.uDebugAddLog($"Added Owner {owner.OwnerID} to Owners List");
                            }
                        }
                        else
                            Toolbox.uDebugAddLog($"Skipped file for not being .owner: {file} || {info.Extension.ToLower()}");
                    }
                }
                catch (Exception ex)
                {
                    Toolbox.FullExceptionLog(ex);
                }
            };
            worker.RunWorkerAsync();
        }

        public static Match CopyNewMatch(Match matchToCopy)
        {
            Match newMatch = new Match();
            PropertyCopy.Copy(matchToCopy, newMatch);
            return newMatch;
        }

        public static bool VerifyProfile(CommandContext context)
        {
            bool hasProfile = false;
            OwnerProfile owner = RPG.Owners.Find(x => x.OwnerID == context.Message.Author.Id);
            if (owner == null)
            {
                hasProfile = false;
                Toolbox.uDebugAddLog($"{context.Message.Author.Id} doesn't currently have a profile");
                Events.SendDiscordMessage(context, "You don't currently have a profile, please create a character first");
            }
            else
                hasProfile = true;
            return hasProfile;
        }

        public static bool VerifyProfileAndHasCharacter(CommandContext context)
        {
            bool hasBoth = false;
            hasBoth = VerifyProfile(context);
            if (!hasBoth)
                return hasBoth;
            OwnerProfile owner = RPG.Owners.Find(x => x.OwnerID == context.Message.Author.Id);
            hasBoth = owner.CharacterList.Count > 0 ? true : false;
            if (!hasBoth)
            {
                Toolbox.uDebugAddLog($"{owner.OwnerID} doesn't currently have a character created");
                Events.SendDiscordMessage(context, "You don't currently have a character created, please create a character first");
            }
            return hasBoth;
        }

        #endregion

        #region Character Methods

        public static Character CreateNewCharacter(ulong ownerId, CharacterClass chosenClass, string name)
        {
            Character newChar = new Character() { OwnerID = ownerId };
            switch (chosenClass)
            {
                case CharacterClass.Dragoon:
                    newChar.Name = name;
                    newChar.Lvl = 1;
                    newChar.Exp = 0;
                    newChar.Weapon = Weapons.dragonSpear;
                    newChar.Armor = Armors.dragonArmor;
                    newChar.Backpack = new BackPack
                    {
                        Stored =
                        {
                            Items.smallHealthPotionPack,
                            Items.smallManaPotionPack
                        }
                    };
                    newChar.SpellBook.Add(Spells.dragonRage);
                    newChar.Class = chosenClass;
                    newChar.MaxHP = 85;
                    newChar.CurrentHP = newChar.MaxHP;
                    newChar.MaxMana = 5;
                    newChar.CurrentMana = newChar.MaxMana;
                    newChar.Str = 10;
                    newChar.Def = 3;
                    newChar.Dex = 4;
                    newChar.Int = 3;
                    newChar.Spd = rng.Next(115, 150);
                    newChar.Lck = 1;
                    break;
                case CharacterClass.Mage:
                    newChar.Name = name;
                    newChar.Lvl = 1;
                    newChar.Exp = 0;
                    newChar.Weapon = Weapons.stick;
                    newChar.Armor = Armors.mageRobe;
                    newChar.Backpack = new BackPack
                    {
                        Stored =
                        {
                            Items.smallHealthPotionPack,
                            Items.smallManaPotionPack
                        }
                    };
                    newChar.SpellBook.Add(Spells.magesEnergy);
                    newChar.SpellBook.Add(Spells.arcaneArmor);
                    newChar.Class = chosenClass;
                    newChar.MaxHP = rng.Next(20, 80);
                    newChar.CurrentHP = newChar.MaxHP;
                    newChar.MaxMana = 20;
                    newChar.CurrentMana = newChar.MaxMana;
                    newChar.Str = rng.Next(1, 10);
                    newChar.Def = rng.Next(1, 15);
                    newChar.Dex = rng.Next(1, 7);
                    newChar.Int = rng.Next(10, 20);
                    newChar.Spd = rng.Next(105, 125);
                    newChar.Lck = rng.Next(0, 10);
                    break;
                case CharacterClass.Necromancer:
                    newChar.Name = name;
                    newChar.Lvl = 1;
                    newChar.Exp = 0;
                    newChar.Weapon = Weapons.glowyOrb;
                    newChar.Armor = Armors.undeadArmor;
                    newChar.Backpack = new BackPack
                    {
                        Stored =
                        {
                            Items.smallHealthPotionPack,
                            Items.smallManaPotionPack
                        }
                    };
                    newChar.SpellBook.Add(Spells.boneSpike);
                    newChar.Class = chosenClass;
                    newChar.MaxHP = rng.Next(60, 110);
                    newChar.CurrentHP = newChar.MaxHP;
                    newChar.MaxMana = 0;
                    newChar.CurrentMana = newChar.MaxMana;
                    newChar.Str = rng.Next(5, 12);
                    newChar.Def = rng.Next(5, 12);
                    newChar.Dex = rng.Next(6, 14);
                    newChar.Int = rng.Next(6, 25);
                    newChar.Spd = rng.Next(80, 105);
                    newChar.Lck = rng.Next(0, 10);
                    break;
                case CharacterClass.Rogue:
                    newChar.Name = name;
                    newChar.Lvl = 1;
                    newChar.Exp = 0;
                    newChar.Weapon = Weapons.rogueDaggers;
                    newChar.Armor = Armors.theiveGarb;
                    newChar.Backpack = new BackPack
                    {
                        Stored =
                        {
                            Items.smallHealthPotionPack
                        }
                    };
                    newChar.Class = chosenClass;
                    newChar.MaxHP = rng.Next(70, 1110);
                    newChar.CurrentHP = newChar.MaxHP;
                    newChar.MaxMana = 0;
                    newChar.CurrentMana = newChar.MaxMana;
                    newChar.Str = rng.Next(2, 15);
                    newChar.Def = rng.Next(4, 12);
                    newChar.Dex = rng.Next(10, 20);
                    newChar.Int = rng.Next(0, 8);
                    newChar.Spd = rng.Next(115, 140);
                    newChar.Lck = rng.Next(1, 12);
                    break;
                case CharacterClass.Warrior:
                    newChar.Name = name;
                    newChar.Lvl = 1;
                    newChar.Exp = 0;
                    newChar.Weapon = Weapons.warriorFists;
                    newChar.Armor = Armors.knightArmor;
                    newChar.Backpack = new BackPack
                    {
                        Stored =
                        {
                            Items.smallHealthPotionPack
                        }
                    };
                    newChar.Class = chosenClass;
                    newChar.MaxHP = rng.Next(50, 100);
                    newChar.CurrentHP = newChar.MaxHP;
                    newChar.MaxMana = 0;
                    newChar.CurrentMana = newChar.MaxMana;
                    newChar.Str = rng.Next(5, 20);
                    newChar.Def = rng.Next(5, 20);
                    newChar.Dex = rng.Next(1, 15);
                    newChar.Int = rng.Next(0, 2);
                    newChar.Spd = rng.Next(100, 120);
                    newChar.Lck = rng.Next(0, 10);
                    break;
            }
            return newChar;
        }

        public static void ChangeCharacter(ulong ownerID, Character chosenCharacter)
        {
            try
            {
                OwnerProfile profile = RPG.Owners.Find(x => x.OwnerID == ownerID);
                profile.CurrentCharacter = chosenCharacter;
            }
            catch (Exception ex)
            {
                Toolbox.FullExceptionLog(ex);
            }
        }

        public static int DetermineCharacterCost(OwnerProfile owner)
        {
            int cost = 0;
            switch (owner.CharacterList.Count)
            {
                case 1:
                    cost = 500;
                    break;
                case 2:
                    cost = 1000;
                    break;
                case 3:
                    cost = 2000;
                    break;
                case 4:
                    cost = 5000;
                    break;
                case 5:
                    cost = 10000;
                    break;
                case 6:
                    cost = 50000;
                    break;
                default:
                    cost = 0;
                    break;
            }
            if (owner.CharacterList.Count > 6)
                cost = 100000;
            return cost;
        }

        public static void CheckCharacterStats(CommandContext context)
        {
            try
            {
                var line = Environment.NewLine;
                OwnerProfile owner = RPG.Owners.Find(x => x.OwnerID == context.Message.Author.Id);
                if (owner == null)
                {
                    Events.SendDiscordMessage(context, "You don't currently have a profile, please create a character first");
                    return;
                }
                else if (owner.CharacterList.Count <= 0)
                {
                    Events.SendDiscordMessage(context, "You don't currently have any characters, create one yo");
                    return;
                }
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder()
                    {
                        Name = owner.CurrentCharacter.Name
                    },
                    Color = new Discord.Color(25, 113, 255),
                    Title = owner.CurrentCharacter.Desc,
                    Description = $"Backpack Storage: {owner.CurrentCharacter.Backpack.Stored.Count}/{owner.CurrentCharacter.Backpack.Capacity}{line}Level: {owner.CurrentCharacter.Lvl}{line}Experience: {owner.CurrentCharacter.Exp}{line}HP: {owner.CurrentCharacter.CurrentHP}/{owner.CurrentCharacter.MaxHP}{line}Mana: {owner.CurrentCharacter.CurrentMana}/{owner.CurrentCharacter.MaxMana}{line}Strength: {owner.CurrentCharacter.Str}{line}Defense: {owner.CurrentCharacter.Def}{line}Dexterity: {owner.CurrentCharacter.Dex}{line}Intelligence: {owner.CurrentCharacter.Int}{line}Speed: {owner.CurrentCharacter.Spd}{line}Luck: {owner.CurrentCharacter.Lck}",
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = owner.CurrentCharacter.Class.ToString()
                    }
                };
                Events.SendDiscordMessage(context, embed);
            }
            catch (Exception ex)
            {
                Toolbox.FullExceptionLog(ex);
            }
        }

        public static void ChangeArmor(CommandContext context, Armor armor)
        {
            try
            {
                OwnerProfile owner = RPG.Owners.Find(x => x.OwnerID == context.Message.Author.Id);
                Toolbox.uDebugAddLog($"{owner.CurrentCharacter.Name} is changing armors, [Current]{owner.CurrentCharacter.Armor.Name} [ChangeTo]{armor.Name} [ID]{owner.OwnerID}");
                owner.CurrentCharacter.Armor = armor;
                Toolbox.uDebugAddLog($"{owner.CurrentCharacter.Name} equipped {armor.Name} [ID]{owner.OwnerID}");
                Events.SendDiscordMessage(context, $"{owner.CurrentCharacter.Name} is now wearing a(n) {armor.Name}");
            }
            catch (Exception ex)
            {
                Toolbox.FullExceptionLog(ex);
            }
        }

        public static void ChangeWeapon(CommandContext context, Weapon weapon)
        {
            try
            {
                OwnerProfile owner = RPG.Owners.Find(x => x.OwnerID == context.Message.Author.Id);
                Toolbox.uDebugAddLog($"{owner.CurrentCharacter.Name} is changing weapons [Current]{owner.CurrentCharacter.Weapon.Name} [ChangeTo]{weapon.Name} [ID]{owner.OwnerID}");
                owner.CurrentCharacter.Weapon = weapon;
                Toolbox.uDebugAddLog($"{owner.CurrentCharacter.Name} has equipped {weapon.Name} [ID]{owner.OwnerID}");
                Events.SendDiscordMessage(context, $"{owner.CurrentCharacter.Name} has equipped a(n) {weapon.Name}");
            }
            catch (Exception ex)
            {
                Toolbox.FullExceptionLog(ex);
            }
        }

        #endregion

        #region Combat Methods

        public static void CreateMatch(CommandContext context, OwnerProfile owner)
        {
            try
            {
                var match = RPG.MatchList.Find(x => x.Owner == owner);
                if (match == null)
                {
                    if (owner.CurrentCharacter.Loot.Count > 0)
                    {
                        Events.SendDiscordMessage(context, $"You still have {owner.CurrentCharacter.Loot.Count} pieces of loot to go through before you can start another match");
                        return;
                    }
                    Toolbox.uDebugAddLog($"Generating new match for {owner.OwnerID}");
                    Match newMatch = new Match() { Owner = owner, MatchStart = DateTime.Now };
                    Enemy newEnemy = Enemies.CopyNewEnemy(Enemies.punchingBag);
                    newMatch.CurrentEnemy = newEnemy;
                    newMatch.EnemyList.Add(newEnemy);
                    Toolbox.uDebugAddLog($"Generated enemy {newEnemy.Name}, set as current enemy and added to the enemy list for {owner.OwnerID}");
                    RPG.MatchList.Add(newMatch);
                    Toolbox.uDebugAddLog($"Successfully generated new match with {newMatch.EnemyList.Count} enemies");
                    Events.SendDiscordMessage(context, $"A new match was generated with **{newMatch.EnemyList.Count}** enemies");
                    CalculateTurn(context, owner);
                    return;
                }
                else
                {
                    Toolbox.uDebugAddLog($"Attempt to generate new match, existing match found for {owner.OwnerID}");
                    TimeSpan time = DateTime.Now - match.MatchStart;
                    TimeSpan timeLeft = (match.LastPlayerTurn + match.TurnTimeLimit) - match.LastPlayerTurn;
                    Events.SendDiscordMessage(context, $"You currently have an active match with **{match.CurrentEnemy.Name}** that was started **{time.Days}D {time.Hours}H {time.Minutes}M {time.Seconds}Secs** ago, please attack your current enemy, you have **{timeLeft.Days}D {timeLeft.Hours}H {timeLeft.Minutes}M {timeLeft.Seconds}Secs** left before you **forfeit**");
                    return;
                }
            }
            catch (Exception ex)
            {
                Toolbox.FullExceptionLog(ex);
            }
        }

        public static void AttackEnemy(CommandContext context, OwnerProfile owner, Enemy enemy)
        {
            try
            {
                var match = RPG.MatchList.Find(x => x.Owner == owner);
                if (match.CurrentTurn != Turn.Player)
                {
                    Events.SendDiscordMessage(context, $"It isn't {owner.CurrentCharacter.Name}'s turn, it's {enemy.Name}'s turn");
                    return;
                }
                Toolbox.uDebugAddLog($"Attacking enemy [C]{owner.CurrentCharacter.Name} [E]{enemy.Name} [ID]{owner.OwnerID}");
                int totalDamage = 0;
                int physDamage = owner.CurrentCharacter.Weapon.PhysicalDamage;
                int magiDamage = owner.CurrentCharacter.Weapon.MagicDamage;
                int fireDamage = owner.CurrentCharacter.Weapon.FireDamage;
                int lighDamage = owner.CurrentCharacter.Weapon.LightningDamage;
                int iceeDamage = owner.CurrentCharacter.Weapon.IceDamage;
                int windDamage = owner.CurrentCharacter.Weapon.WindDamage;

                int enemyPhys = enemy.Armor.Physical;
                int enemyMagi = enemy.Armor.Magic;
                int enemyFire = enemy.Armor.Fire;
                int enemyLigh = enemy.Armor.Lightning;
                int enemyIcee = enemy.Armor.Ice;
                int enemyWind = enemy.Armor.Wind;

                Affliction buff = new Affliction();
                foreach (var affl in owner.CurrentCharacter.StatusEffects)
                {
                    if (affl.Positive)
                    {
                        Toolbox.uDebugAddLog($"Applying affliction to {owner.CurrentCharacter.Name}, Before: [P]{physDamage} [M]{magiDamage} [F]{fireDamage} [L]{lighDamage} [I]{iceeDamage} [W]{windDamage} [ID]{owner.OwnerID}");
                        physDamage += affl.Physical;
                        magiDamage += affl.Magic;
                        fireDamage += affl.Fire;
                        lighDamage += affl.Lightning;
                        iceeDamage += affl.Ice;
                        windDamage += affl.Ice;
                        Toolbox.uDebugAddLog($"Applied affliction to {owner.CurrentCharacter.Name}, After: [P]{physDamage} [M]{magiDamage} [F]{fireDamage} [L]{lighDamage} [I]{iceeDamage} [W]{windDamage} [ID]{owner.OwnerID}");
                    }
                }

                Affliction dmg = new Affliction();
                foreach (var affl in enemy.StatusEffects)
                {
                    if (!affl.Positive)
                    {
                        Toolbox.uDebugAddLog($"Applying affliction to {enemy.Name}, Before: [P]{enemyPhys} [M]{enemyMagi} [F]{enemyFire} [L]{enemyLigh} [I]{enemyIcee} [W]{enemyWind} [ID]{owner.OwnerID}");
                        enemyPhys = enemyPhys - affl.Physical >= 0 ? enemyPhys - affl.Physical : 0;
                        enemyMagi = enemyMagi - affl.Magic >= 0 ? enemyMagi - affl.Magic : 0;
                        enemyFire = enemyFire - affl.Fire >= 0 ? enemyFire - affl.Fire : 0;
                        enemyLigh = enemyLigh - affl.Lightning >= 0 ? enemyLigh - affl.Lightning : 0;
                        enemyIcee = enemyIcee - affl.Ice >= 0 ? enemyIcee - affl.Ice : 0;
                        enemyWind = enemyWind - affl.Wind >= 0 ? enemyWind - affl.Wind : 0;
                        Toolbox.uDebugAddLog($"Applied affliction to {enemy.Name}, After: [P]{enemyPhys} [M]{enemyMagi} [F]{enemyFire} [L]{enemyLigh} [I]{enemyIcee} [W]{enemyWind} [ID]{owner.OwnerID}");
                    }
                }

                Toolbox.uDebugAddLog($"physDamage = ({physDamage} * {owner.CurrentCharacter.Str}) - ({enemyPhys} * {enemy.Def})");
                physDamage = (physDamage * owner.CurrentCharacter.Str) - (enemyPhys * enemy.Def);
                if (physDamage <= 0) physDamage = 0;
                Toolbox.uDebugAddLog("Calculating magiDamage");
                magiDamage = CalculateElement(magiDamage, enemyMagi);
                Toolbox.uDebugAddLog("Calculating fireDamage");
                fireDamage = CalculateElement(fireDamage, enemyFire);
                Toolbox.uDebugAddLog("Calculating lighDamage");
                lighDamage = CalculateElement(lighDamage, enemyLigh);
                Toolbox.uDebugAddLog("Calculating iceeDamage");
                iceeDamage = CalculateElement(iceeDamage, enemyIcee);
                Toolbox.uDebugAddLog("Calculating windDamage");
                windDamage = CalculateElement(windDamage, enemyWind);
                Toolbox.uDebugAddLog($"Calculated Damage Types: [P]{physDamage} [M]{magiDamage} [F]{fireDamage} [L]{lighDamage} [I]{iceeDamage} [W]{windDamage} [ID]{owner.OwnerID}");

                totalDamage = physDamage + magiDamage + fireDamage + lighDamage + iceeDamage + windDamage;
                Toolbox.uDebugAddLog($"Calculated Total Damage: {totalDamage} [ID]{owner.OwnerID}");
                RPG.MatchList.Find(x => x.Owner == owner).LastPlayerTurn = DateTime.Now;

                if (totalDamage > 0)
                {
                    if (enemy.CurrentHP - totalDamage <= 0)
                    {
                        EnemyDied(context, owner, enemy);
                        return;
                    }
                    else
                    {
                        enemy.CurrentHP -= totalDamage;
                        Toolbox.uDebugAddLog($"{owner.CurrentCharacter.Name} attacked {enemy.Name} [D]{totalDamage} [ID]{owner.OwnerID}");
                        Events.SendDiscordMessage(context, $"**{owner.CurrentCharacter.Name}** attacked **{enemy.Name}** and dealt **{totalDamage}** damage ({enemy.CurrentHP}/{enemy.MaxHP} left)");
                        return;
                    }
                }
                else if (totalDamage == 0)
                {
                    Toolbox.uDebugAddLog($"{owner.CurrentCharacter.Name} attacked {enemy.Name}, didn't deal damage [D]{totalDamage} [ID]{owner.OwnerID}");
                    Events.SendDiscordMessage(context, $"**{owner.CurrentCharacter.Name}** attacked **{enemy.Name}** and didn't deal any damage ({enemy.CurrentHP}/{enemy.MaxHP} left)");
                    return;
                }
                else
                {
                    if (enemy.CurrentHP + totalDamage < enemy.MaxHP)
                        enemy.CurrentHP -= totalDamage;
                    else
                        enemy.CurrentHP = enemy.MaxHP;
                    Toolbox.uDebugAddLog($"{owner.CurrentCharacter.Name} attacked {enemy.Name}, absorbed {totalDamage} [ID]{owner.OwnerID}");
                    Events.SendDiscordMessage(context, $"**{owner.CurrentCharacter.Name}** attacked, **{enemy.Name}** absorbed **{totalDamage}** damage and was healed ({enemy.CurrentHP}/{enemy.MaxHP} left)");
                    return;
                }
            }
            catch (Exception ex)
            {
                Toolbox.FullExceptionLog(ex);
            }
        }

        public static void AttackCharacter(CommandContext context, Enemy enemy, OwnerProfile owner)
        {
            try
            {
                Toolbox.uDebugAddLog($"Enemy {enemy.Name} attacking Character {owner.CurrentCharacter.Name} [ID]{owner.OwnerID}");
                int physDamage = owner.CurrentCharacter.Armor.Physical;
                int magiDamage = owner.CurrentCharacter.Armor.Magic;
                int fireDamage = owner.CurrentCharacter.Armor.Fire;
                int lighDamage = owner.CurrentCharacter.Armor.Lightning;
                int iceeDamage = owner.CurrentCharacter.Armor.Ice;
                int windDamage = owner.CurrentCharacter.Armor.Wind;

                int enemyTotal = 0;
                int enemyPhys = enemy.Weapon.PhysicalDamage;
                int enemyMagi = enemy.Weapon.MagicDamage;
                int enemyFire = enemy.Weapon.FireDamage;
                int enemyLigh = enemy.Weapon.LightningDamage;
                int enemyIcee = enemy.Weapon.IceDamage;
                int enemyWind = enemy.Weapon.WindDamage;

                Affliction buff = new Affliction();
                foreach (var affl in owner.CurrentCharacter.StatusEffects)
                {
                    if (affl.Positive)
                    {
                        Toolbox.uDebugAddLog($"Applying affliction to {owner.CurrentCharacter.Name}, Before: [P]{physDamage} [M]{magiDamage} [F]{fireDamage} [L]{lighDamage} [I]{iceeDamage} [W]{windDamage} [ID]{owner.OwnerID}");
                        physDamage += affl.Physical;
                        magiDamage += affl.Magic;
                        fireDamage += affl.Fire;
                        lighDamage += affl.Lightning;
                        iceeDamage += affl.Ice;
                        windDamage += affl.Ice;
                        Toolbox.uDebugAddLog($"Applied affliction to {owner.CurrentCharacter.Name}, After: [P]{physDamage} [M]{magiDamage} [F]{fireDamage} [L]{lighDamage} [I]{iceeDamage} [W]{windDamage} [ID]{owner.OwnerID}");
                    }
                }

                Affliction dmg = new Affliction();
                foreach (var affl in enemy.StatusEffects)
                {
                    if (!affl.Positive)
                    {
                        Toolbox.uDebugAddLog($"Applying affliction to {enemy.Name}, Before: [P]{enemyPhys} [M]{enemyMagi} [F]{enemyFire} [L]{enemyLigh} [I]{enemyIcee} [W]{enemyWind} [ID]{owner.OwnerID}");
                        enemyPhys = enemyPhys - affl.Physical >= 0 ? enemyPhys - affl.Physical : 0;
                        enemyMagi = enemyMagi - affl.Magic >= 0 ? enemyMagi - affl.Magic : 0;
                        enemyFire = enemyFire - affl.Fire >= 0 ? enemyFire - affl.Fire : 0;
                        enemyLigh = enemyLigh - affl.Lightning >= 0 ? enemyLigh - affl.Lightning : 0;
                        enemyIcee = enemyIcee - affl.Ice >= 0 ? enemyIcee - affl.Ice : 0;
                        enemyWind = enemyWind - affl.Wind >= 0 ? enemyWind - affl.Wind : 0;
                        Toolbox.uDebugAddLog($"Applied affliction to {enemy.Name}, After: [P]{enemyPhys} [M]{enemyMagi} [F]{enemyFire} [L]{enemyLigh} [I]{enemyIcee} [W]{enemyWind} [ID]{owner.OwnerID}");
                    }
                }

                Toolbox.uDebugAddLog($"physDamage = ({enemyPhys} * {enemy.Str}) - ({physDamage} * {owner.CurrentCharacter.Def})");
                enemyPhys = (enemyPhys * enemy.Str) - (physDamage * owner.CurrentCharacter.Def);
                if (enemyPhys <= 0) enemyPhys = 0;
                Toolbox.uDebugAddLog("Calculating magiDamage");
                enemyMagi = CalculateElement(enemyMagi, magiDamage);
                Toolbox.uDebugAddLog("Calculating fireDamage");
                enemyFire = CalculateElement(enemyFire, fireDamage);
                Toolbox.uDebugAddLog("Calculating lighDamage");
                enemyLigh = CalculateElement(enemyLigh, lighDamage);
                Toolbox.uDebugAddLog("Calculating iceeDamage");
                enemyIcee = CalculateElement(enemyIcee, iceeDamage);
                Toolbox.uDebugAddLog("Calculating windDamage");
                enemyWind = CalculateElement(enemyWind, windDamage);
                Toolbox.uDebugAddLog($"Calculated Damage Types: [P]{enemyPhys} [M]{enemyMagi} [F]{enemyFire} [L]{enemyLigh} [I]{enemyIcee} [W]{enemyWind} [ID]{owner.OwnerID}");

                enemyTotal = enemyPhys + enemyMagi + enemyFire + enemyLigh + enemyIcee + enemyWind;
                Toolbox.uDebugAddLog($"Calculated Total Damage: {enemyTotal} [ID]{owner.OwnerID}");

                if (enemyTotal > 0)
                {
                    if (owner.CurrentCharacter.CurrentHP - enemyTotal <= 0)
                    {
                        CharacterDied(context, enemy, owner);
                        return;
                    }
                    else
                    {
                        owner.CurrentCharacter.CurrentHP -= enemyTotal;
                        Toolbox.uDebugAddLog($"{enemy.Name} attacked {owner.CurrentCharacter.Name} and dealt {enemyTotal} [ID]{owner.OwnerID}");
                        Events.SendDiscordMessage(context, $"**{enemy.Name}** attacked **{owner.CurrentCharacter.Name}** and dealt **{enemyTotal}** damage ({owner.CurrentCharacter.CurrentHP}/{owner.CurrentCharacter.MaxHP} left)");
                        return;
                    }
                }
                else if (enemyTotal == 0)
                {
                    Toolbox.uDebugAddLog($"{enemy.Name} attacked {owner.CurrentCharacter.Name} and didn't deal any damage [D]{enemyTotal} [ID]{owner.OwnerID}");
                    Events.SendDiscordMessage(context, $"**{enemy.Name}** attacked **{owner.CurrentCharacter.Name}** and didn't deal any damage ({owner.CurrentCharacter.CurrentHP}/{owner.CurrentCharacter.MaxHP} left)");
                    return;
                }
                else
                {
                    if (owner.CurrentCharacter.CurrentHP + enemyTotal < owner.CurrentCharacter.MaxHP)
                        owner.CurrentCharacter.CurrentHP -= enemyTotal;
                    else
                        owner.CurrentCharacter.CurrentHP = owner.CurrentCharacter.MaxHP;
                    Toolbox.uDebugAddLog($"{enemy.Name} attacked, {owner.CurrentCharacter.Name} absorbed {enemyTotal} damage and was healed");
                    Events.SendDiscordMessage(context, $"**{enemy.Name}** attacked, **{owner.CurrentCharacter.Name}** absorbed **{enemyTotal}** damage and was healed ({owner.CurrentCharacter.CurrentHP}/{owner.CurrentCharacter.MaxHP} left)");
                    return;
                }
            }
            catch (Exception ex)
            {
                Toolbox.FullExceptionLog(ex);
            }
        }

        public static void EnemyDied(CommandContext context, OwnerProfile owner, Enemy enemy)
        {
            try
            {
                Toolbox.uDebugAddLog($"{owner.OwnerID} defeated {enemy.Name}");
                var match = RPG.MatchList.Find(x => x.Owner == owner);
                match.EnemyList.Remove(enemy);
                match.DefeatedEnemies.Add(enemy);
                match.ExperienceEarned += enemy.ExpLoot;
                Toolbox.uDebugAddLog($"Removed {enemy.Name} from the enemy list, added to the defeated enemy list and added ExpLoot");
                if (RPG.MatchList.Find(x => x.Owner == owner).EnemyList.Count <= 0)
                {
                    Events.SendDiscordMessage(context, $"You have defeated **{enemy.Name}** and earned **{enemy.ExpLoot} EXP!**");
                    MatchOver(context, owner, enemy, MatchCompleteResult.Won);
                    return;
                }
                else
                {
                    NextEnemy(context, owner, enemy);
                    return;
                }
            }
            catch (Exception ex)
            {
                Toolbox.FullExceptionLog(ex);
            }
        }

        public static void CharacterDied(CommandContext context, Enemy enemy, OwnerProfile owner)
        {
            try
            {
                Toolbox.uDebugAddLog($"{enemy.Name} defeated {owner.OwnerID}");
                MatchOver(context, owner, enemy, MatchCompleteResult.Lost);
            }
            catch (Exception ex)
            {
                Toolbox.FullExceptionLog(ex);
            }
        }

        public static void MatchOver(CommandContext context, OwnerProfile owner, Enemy enemy, MatchCompleteResult result)
        {
            try
            {
                owner.CurrentCharacter.CurrentHP = owner.CurrentCharacter.MaxHP;
                Toolbox.uDebugAddLog($"Match complete, healed {owner.CurrentCharacter.Name} to full health [CHP]{owner.CurrentCharacter.CurrentHP} [MHP]{owner.CurrentCharacter.MaxHP} [ID]{owner.OwnerID}");
                owner.CurrentCharacter.StatusEffects.Clear();
                Toolbox.uDebugAddLog($"Removed afflictions from {owner.CurrentCharacter.Name}, new count: {owner.CurrentCharacter.StatusEffects.Count} [ID]{owner.OwnerID}");
                switch (result)
                {
                    case MatchCompleteResult.Won:
                        var match = RPG.MatchList.Find(x => x.Owner == owner);
                        var defEnemies = match.DefeatedEnemies.Count;
                        TimeSpan time = (DateTime.Now - match.MatchStart);
                        Toolbox.uDebugAddLog($"{owner.OwnerID} finished the match after defeating {enemy.Name}");
                        Toolbox.uDebugAddLog($"Triggering MatchCompleted Event: [R]{result} [EC]{match.DefeatedEnemies.Count} [EXP]{match.ExperienceEarned} [T]{time.Days}D {time.Hours}H {time.Minutes}M {time.Seconds}S [O]{owner.OwnerID}");
                        Match copyMatch = CopyNewMatch(match);
                        Events.CompleteMatch(context, owner, copyMatch, time, result);
                        RPG.MatchList.Remove(match);
                        Toolbox.uDebugAddLog($"Removed match from match list for {owner.OwnerID}");
                        return;
                    case MatchCompleteResult.Lost:
                        var match2 = RPG.MatchList.Find(x => x.Owner == owner);
                        var defEnemies2 = match2.DefeatedEnemies.Count;
                        TimeSpan time2 = (DateTime.Now - match2.MatchStart);
                        Toolbox.uDebugAddLog($"{owner.OwnerID} was defeated by {enemy.Name}");
                        Toolbox.uDebugAddLog($"Triggering MatchCompleted Event: [R]{result} [EC]{match2.DefeatedEnemies.Count} [EXP]{match2.ExperienceEarned} [T]{time2.Days}D {time2.Hours}H {time2.Minutes}M {time2.Seconds}S [O]{owner.OwnerID}");
                        Match copyMatch2 = CopyNewMatch(match2);
                        Events.CompleteMatch(context, owner, copyMatch2, time2, result);
                        RPG.MatchList.Remove(match2);
                        Toolbox.uDebugAddLog($"Removed match from match list for {owner.OwnerID}");
                        return;
                    case MatchCompleteResult.Forfeit:
                        var match3 = RPG.MatchList.Find(x => x.Owner == owner);
                        var timeSpan = match3.TurnTimeLimit;
                        var exp = match3.ExperienceEarned;
                        Toolbox.uDebugAddLog($"{owner.OwnerID} forfeited the match agains {enemy.Name}");
                        Toolbox.uDebugAddLog($"Triggering MatchCompleted Event: [R]{result} [EC]{match3.DefeatedEnemies.Count} [EXP]{match3.ExperienceEarned} [T]{match3.TurnTimeLimit.Days}D {match3.TurnTimeLimit.Hours}H {match3.TurnTimeLimit.Minutes}M {match3.TurnTimeLimit.Seconds}S [O]{owner.OwnerID}");
                        Match copyMatch3 = CopyNewMatch(match3);
                        Events.CompleteMatch(context, owner, copyMatch3, timeSpan, result);
                        RPG.MatchList.Remove(match3);
                        Toolbox.uDebugAddLog($"Removed match from match list for {owner.OwnerID}");
                        return;
                }
                Toolbox.uDebugAddLog($"MATCHOVER UNREACHABLE WAS REACHED");
                Events.uStatusUpdateExt($"MATCHOVER UNREACHABLE WAS REACHED, PLEASE LET THE DEVELOPER KNOW!!!!");
            }
            catch (Exception ex)
            {
                Toolbox.FullExceptionLog(ex);
            }
        }

        public static async Task EndOfMatchLootAsync(MatchArgs args)
        {
            try
            {
                switch (args.Result)
                {
                    case MatchCompleteResult.Won:
                        await args.Context.Channel.SendMessageAsync($"{args.Context.Message.Author.Mention} You have defeated **{args.Match.DefeatedEnemies.Count} enemies** and completed the match!");
                        int lootCount = rng.Next(rng.Next(0, 2), 3 + (rng.Next(0, args.Match.DefeatedEnemies.Count)));
                        Toolbox.uDebugAddLog($"Generating lootdrop, lootcount: {lootCount}");
                        int lootTimes = 0;
                        for (int i = lootCount; i > 0; i--)
                        {
                            lootTimes++;
                            var loot = LootDrop.PickLoot(args.Owner.CurrentCharacter);
                            args.Owner.CurrentCharacter.Loot.Add(loot);
                        }
                        Toolbox.uDebugAddLog($"Lootdrop generated, lootcount before filter: {lootTimes} [ID]{args.Owner.OwnerID}");
                        int pebbles = 0;
                        int currency = 0;
                        LootDrop.FilterLoot(args.Owner.CurrentCharacter, out pebbles, out currency);
                        Toolbox.uDebugAddLog($"Lootdrop lootcount after filter: {args.Owner.CurrentCharacter.Loot.Count} [ID]{args.Owner.OwnerID}");
                        await args.Context.Channel.SendMessageAsync($"{args.Context.Message.Author.Mention} You earned {pebbles} pebbles and {currency} currency");
                        await EmptyLoot(args.Context);
                        break;
                    case MatchCompleteResult.Lost:
                        await args.Context.Channel.SendMessageAsync($"You were defeated in combat by {args.Match.CurrentEnemy.Name} after defeating **{args.Match.DefeatedEnemies.Count} enemies**");
                        break;
                    case MatchCompleteResult.Forfeit:
                        await args.Context.Channel.SendMessageAsync($"You forefeited the match after going beyond teh match time limit of **{args.Match.TurnTimeLimit.Days}D {args.Match.TurnTimeLimit.Hours}H {args.Match.TurnTimeLimit.Minutes}M {args.Match.TurnTimeLimit.Seconds}Secs**, you lost **{args.Match.ExperienceEarned} experience**");
                        break;
                }
            }
            catch (Exception ex)
            {
                Toolbox.FullExceptionLog(ex);
            }
        }

        public static async Task EmptyLoot(CommandContext context)
        {
            try
            {
                var owner = RPG.Owners.Find(x => x.OwnerID == context.Message.Author.Id);
                var timeStamp = DateTime.Now;
                List<IMessage> responded = new List<IMessage>();
                bool emptiedLoot = owner.CurrentCharacter.Loot.Count > 0 ? false : true;
                Toolbox.uDebugAddLog($"Emptying Loot: [HasLoot]{emptiedLoot} [Count]{owner.CurrentCharacter.Loot.Count} [ID]{owner.OwnerID}");
                while (!emptiedLoot)
                {
                    Toolbox.uDebugAddLog($"{owner.CurrentCharacter.Name} has loot, going through list [ID]{owner.OwnerID}");
                    owner = RPG.Owners.Find(x => x.OwnerID == context.Message.Author.Id);
                    var loot = owner.CurrentCharacter.Loot[0];
                    var lootMsg = await context.Channel.SendMessageAsync($"```{LootDrop.GetLootInfo(loot)}```{Environment.NewLine} What would you like to do with this loot? (trash, keep, sell, cancel)");
                    Toolbox.uDebugAddLog($"Got first loot and waiting for response. [Loot]{loot.Name} [ID]{owner.OwnerID}");
                    bool lootResp = false;
                    while (!lootResp)
                    {
                        var msgList = await context.Channel.GetMessagesAsync(5).Flatten();
                        foreach (var msg in msgList)
                        {
                            if ((msg.Author == context.Message.Author) && (msg.Timestamp.DateTime > lootMsg.Timestamp.DateTime) && (!responded.Contains(msg)))
                            {
                                responded.Add(msg);
                                string answer = msg.Content.ToString().ToLower();
                                Toolbox.uDebugAddLog($"Response recieved from same author and a newer message. [Resp]{answer} [ID]{owner.OwnerID}");
                                switch (answer)
                                {
                                    case "trash":
                                        owner.CurrentCharacter.Loot.Remove(loot);
                                        Toolbox.uDebugAddLog($"{owner.CurrentCharacter.Name} trashed item {loot.Name} [ID]{owner.OwnerID}");
                                        lootResp = true;
                                        await context.Channel.SendMessageAsync($"{context.Message.Author.Mention} {loot.GetLootType()} was trashed");
                                        break;
                                    case "keep":
                                        if (LootDrop.GetLootType(loot) == LootDrop.LootType.Item)
                                        {
                                            Item storedItem = (Item)owner.CurrentCharacter.Backpack.Stored.Find(x => x.Name == loot.Name && x.Lvl == loot.Lvl);
                                            Item newItem = (Item)loot;
                                            if (storedItem == null)
                                            {
                                                owner.CurrentCharacter.Backpack.Stored.Add(loot);
                                                Toolbox.uDebugAddLog($"{owner.CurrentCharacter.Name} didn't already have a lvl {newItem.Lvl} {newItem.Name}, adding to inventory [ID]{owner.OwnerID}");
                                            }
                                            else
                                            {
                                                Toolbox.uDebugAddLog($"{owner.CurrentCharacter.Name} already had a lvl {newItem.Lvl} {newItem.Name}, adding count[{newItem.Count}]. Before[{storedItem.Count}] [ID]{owner.OwnerID}");
                                                storedItem.Count += newItem.Count;
                                                Toolbox.uDebugAddLog($"After[{storedItem.Count}] [ID]{owner.OwnerID}");
                                            }
                                        }
                                        else
                                        {
                                            owner.CurrentCharacter.Backpack.Stored.Add(loot);
                                            Toolbox.uDebugAddLog($"Added {loot.Name} to {owner.CurrentCharacter.Name}'s backpack [ID]{owner.OwnerID}");
                                        }
                                        owner.CurrentCharacter.Loot.Remove(loot);
                                        Toolbox.uDebugAddLog($"Removed loot from loot list: {loot.Name} [ID]{owner.OwnerID}");
                                        lootResp = true;
                                        await context.Channel.SendMessageAsync($"{context.Message.Author.Mention} {loot.Name} was placed in your trusty backpack");
                                        break;
                                    case "sell":
                                        dynamic retLoot = loot.GetItem();
                                        Toolbox.uDebugAddLog($"{owner.CurrentCharacter.Name} is selling {loot.Name} for {loot.Worth}. [Before]{owner.Currency} [ID]{owner.OwnerID}");
                                        owner.Currency += retLoot.Worth;
                                        owner.CurrentCharacter.Loot.Remove(loot);
                                        Toolbox.uDebugAddLog($"{owner.CurrentCharacter.Name} sold {loot.Name}. [After]{owner.Currency} [ID]{owner.OwnerID}");
                                        lootResp = true;
                                        await context.Channel.SendMessageAsync($"{context.Message.Author.Mention} {loot.Name} was sold for {retLoot.Worth} currency");
                                        break;
                                    case "cancel":
                                        lootResp = true;
                                        emptiedLoot = true;
                                        Toolbox.uDebugAddLog($"{owner.CurrentCharacter.Name} canceled loot [ID]{owner.OwnerID}");
                                        await context.Channel.SendMessageAsync($"Canceled, you still have {owner.CurrentCharacter.Loot.Count} loot to go through before you can start another match, you can go through your loot later by using ;test loot");
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        await Task.Delay(1000);
                    }
                    if (owner.CurrentCharacter.Loot.Count <= 0)
                        emptiedLoot = true;
                    if (timeStamp + TimeSpan.FromMinutes(5) <= DateTime.Now)
                    {
                        await context.Channel.SendMessageAsync($"{context.Message.Author.Mention} A response hasn't been received in 5 minutes, you can go through your loot later by using ;test loot");
                        return;
                    }
                }
                await context.Channel.SendMessageAsync($"{context.Message.Author.Mention} You don't have any loot to go through currently");
            }
            catch (Exception ex)
            {
                Toolbox.FullExceptionLog(ex);
            }
        }

        public static async Task CharacterUseItem(CommandContext context)
        {
            try
            {
                bool hasSetup = VerifyProfileAndHasCharacter(context);
                if (!hasSetup)
                    return;
                OwnerProfile owner = RPG.Owners.Find(x => x.OwnerID == context.Message.Author.Id);
                List<Item> itemList = new List<Item>();
                foreach (IBackPackItem bpI in owner.CurrentCharacter.Backpack.Stored)
                {
                    if (bpI.GetLootType() == LootDrop.LootType.Item)
                    {
                        itemList.Add((Item)bpI);
                        Toolbox.uDebugAddLog($"Found Item in Backpack of type {bpI.GetLootType()} [Name]{bpI.Name} [Count]{((Item)bpI).Count} [ID]{owner.OwnerID}");
                    }
                }
                Toolbox.uDebugAddLog($"Found {itemList.Count} items in backpack of type {LootDrop.LootType.Item} [ID]{owner.OwnerID}");
                if (itemList.Count <= 0)
                {
                    Toolbox.uDebugAddLog($"Found no items in backpack of type {LootDrop.LootType.Item} [ID]{owner.OwnerID}");
                    await context.Channel.SendMessageAsync($"{context.Message.Author.Mention} You don't currently have any items in **{owner.CurrentCharacter.Name}'s** backpack, nice try!");
                    return;
                }
                itemList.OrderBy(x => x.Name);
                int itemCount = 1;
                string itemsString = $"{context.Message.Author.Mention}```Which item would you like to use? (which number){Environment.NewLine}";
                foreach (Item item in itemList)
                {
                    itemsString = $"{itemsString}[{itemCount}] Name: {item.Name} Level: {item.Lvl} Count: {item.Count}{Environment.NewLine}";
                    itemCount++;
                }
                itemsString = $"{itemsString}```";
                bool itemResp = false;
                var sendMsg = await context.Channel.SendMessageAsync(itemsString);
                while (!itemResp)
                {
                    var msgList = await context.Channel.GetMessagesAsync(5).Flatten();
                    foreach (var msg in msgList)
                    {
                        if ((msg.Author == context.Message.Author) && (msg.Timestamp.DateTime > sendMsg.Timestamp.DateTime))
                        {
                            int itemNum = 0;
                            bool isNum = int.TryParse(msg.Content.ToString(), out itemNum);
                            if (!isNum)
                                await context.Channel.SendMessageAsync($"{context.Message.Author.Mention} {msg.Content.ToString()} isn't a valid response, try again");
                            else if ((itemNum <= 0 || itemNum > itemList.Count))
                                await context.Channel.SendMessageAsync($"{context.Message.Author.Mention} {msg.Content.ToString()} is above or below the number of items you have in your backpack");
                            else
                            {
                                itemResp = true;
                                Item chosenItem = itemList[itemNum - 1];
                                UseItem(context, owner, chosenItem);
                            }
                        }
                    }
                    if (sendMsg.Timestamp.DateTime + TimeSpan.FromMinutes(5) <= DateTime.Now)
                    {
                        await context.Channel.SendMessageAsync($"{context.Message.Author.Mention} Response hasn't been received within 5min, canceling Use Item");
                        Toolbox.uDebugAddLog($"Response wasn't received within 5min, canceling [ID]{owner.OwnerID}");
                    }
                }
            }
            catch (Exception ex)
            {
                Toolbox.FullExceptionLog(ex);
            }
        }

        public static async Task CharacterChangeArmor(CommandContext context)
        {
            try
            {
                bool hasSetup = VerifyProfileAndHasCharacter(context);
                if (!hasSetup)
                    return;
                OwnerProfile owner = RPG.Owners.Find(x => x.OwnerID == context.Message.Author.Id);
                List<Armor> armorList = new List<Armor>();
                foreach (IBackPackItem iBPItem in owner.CurrentCharacter.Backpack.Stored)
                {
                    if (iBPItem.GetLootType() == LootDrop.LootType.Armor && (Armor)iBPItem != owner.CurrentCharacter.Armor)
                        armorList.Add((Armor)iBPItem);
                }
                if (armorList.Count <= 0)
                {
                    await context.Channel.SendMessageAsync($"{context.Message.Author.Mention} {owner.CurrentCharacter.Name} doesn't currently have any armor in their bag, nice try");
                    return;
                }
                armorList.OrderBy(x => x.Name);
                string armorString = $"{context.Message.Author.Mention}```What armor would you like to equip? (enter the number){Environment.NewLine}";
                int armorNum = 1;
                foreach (var arm in armorList)
                {
                    armorString = $"{armorString}[{armorNum}] {arm.Name}{Environment.NewLine}";
                    armorNum++;
                }
                armorString = $"{armorString}```";
                var sentMsg = await context.Channel.SendMessageAsync(armorString);
                List<IMessage> respondedList = new List<IMessage>();
                int chosenNum = 0;
                bool responded = false;
                while (!responded)
                {
                    var msgList = await context.Channel.GetMessagesAsync(5).Flatten();
                    foreach (var msg in msgList)
                        if (msg.Author == context.Message.Author && msg.Timestamp.DateTime > sentMsg.Timestamp.DateTime && !respondedList.Contains(msg))
                        {
                            respondedList.Add(msg);
                            bool isNum = int.TryParse(msg.Content.ToString(), out chosenNum);
                            if (!isNum)
                                await context.Channel.SendMessageAsync($"{context.Message.Author.Mention} {msg.Content.ToString()} isnt' a valid number, please try again");
                            else if (chosenNum < 1 || chosenNum > armorList.Count)
                                await context.Channel.SendMessageAsync($"{context.Message.Author.Mention} {chosenNum} is above or below the number of armors you have, please try again");
                            else
                                responded = true;
                        }
                    if (sentMsg.Timestamp.DateTime + TimeSpan.FromMinutes(5) <= DateTime.Now)
                    {
                        await context.Channel.SendMessageAsync($"{context.Message.Author.Mention} A response wasn't received within 5min, canceling Armor Change");
                        return;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
                Armor chosenArmor = armorList[chosenNum - 1];
                ChangeArmor(context, chosenArmor);
            }
            catch (Exception ex)
            {
                Toolbox.FullExceptionLog(ex);
            }
        }

        public static async Task CharacterChangeWeapon(CommandContext context)
        {
            try
            {
                bool hasSetup = VerifyProfileAndHasCharacter(context);
                if (!hasSetup)
                    return;
                OwnerProfile owner = RPG.Owners.Find(x => x.OwnerID == context.Message.Author.Id);
                List<Weapon> weapList = new List<Weapon>();
                foreach (IBackPackItem iBPItem in owner.CurrentCharacter.Backpack.Stored)
                {
                    if (iBPItem.GetLootType() == LootDrop.LootType.Weapon && (Weapon)iBPItem != owner.CurrentCharacter.Weapon)
                        weapList.Add((Weapon)iBPItem);
                }
                if (weapList.Count <= 0)
                {
                    await context.Channel.SendMessageAsync($"{context.Message.Author.Mention} {owner.CurrentCharacter.Name} doesn't currently have any weapons in their bag, nice try");
                    return;
                }
                weapList.OrderBy(x => x.Name);
                string weapString = $"{context.Message.Author.Mention}```What weapon would you like to equip? (enter the number){Environment.NewLine}";
                int weapNum = 1;
                foreach (var weap in weapList)
                {
                    weapString = $"{weapString}[{weapNum}] {weap.Name}{Environment.NewLine}";
                    weapNum++;
                }
                weapString = $"{weapString}```";
                var sentMsg = await context.Channel.SendMessageAsync(weapString);
                List<IMessage> respondedList = new List<IMessage>();
                int chosenNum = 0;
                bool responded = false;
                while (!responded)
                {
                    var msgList = await context.Channel.GetMessagesAsync(5).Flatten();
                    foreach (var msg in msgList)
                        if (msg.Author == context.Message.Author && msg.Timestamp.DateTime > sentMsg.Timestamp.DateTime && !respondedList.Contains(msg))
                        {
                            respondedList.Add(msg);
                            bool isNum = int.TryParse(msg.Content.ToString(), out chosenNum);
                            if (!isNum)
                                await context.Channel.SendMessageAsync($"{context.Message.Author.Mention} {msg.Content.ToString()} isnt' a valid number, please try again");
                            else if (chosenNum < 1 || chosenNum > weapList.Count)
                                await context.Channel.SendMessageAsync($"{context.Message.Author.Mention} {chosenNum} is above or below the number of weapons you have, please try again");
                            else
                                responded = true;
                        }
                    if (sentMsg.Timestamp.DateTime + TimeSpan.FromMinutes(5) <= DateTime.Now)
                    {
                        await context.Channel.SendMessageAsync($"{context.Message.Author.Mention} A response wasn't received within 5min, canceling Weapon Change");
                        return;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
                Weapon chosenWeapon = weapList[weapNum - 1];
                ChangeWeapon(context, chosenWeapon);
            }
            catch (Exception ex)
            {
                Toolbox.FullExceptionLog(ex);
            }
        }

        public static async Task CharacterChangeDescription(CommandContext context)
        {
            var isSetup = VerifyProfileAndHasCharacter(context);
            if (!isSetup)
                return;
            OwnerProfile owner = RPG.Owners.Find(x => x.OwnerID == context.User.Id);
            var timeStamp = DateTime.Now;
            List<IMessage> respondedList = new List<IMessage>();
            Toolbox.uDebugAddLog($"Changing description of {owner.CurrentCharacter.Name} [ID]{owner.OwnerID}");
            var sentMsg = await context.Channel.SendMessageAsync($"{context.User.Mention} What would you like the new description for {owner.CurrentCharacter.Name} to be? (enter answer or type cancel){Environment.NewLine}Current Description: {owner.CurrentCharacter.Desc}");
            bool setupDone = false;
            while (!setupDone)
            {
                var msgList = await context.Channel.GetMessagesAsync(5).Flatten();
                foreach (var msg in msgList)
                    if (msg.Author == context.User && msg.Timestamp.DateTime > sentMsg.Timestamp.DateTime && !respondedList.Contains(sentMsg))
                    {
                        respondedList.Add(msg);
                        if (msg.Content.ToString().ToLower() == "cancel")
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} canceled description change");
                            Toolbox.uDebugAddLog($"Canceling description change [ID]{owner.OwnerID}");
                            return;
                        }
                        Toolbox.uDebugAddLog($"Changing description, current: {owner.CurrentCharacter.Desc} [ID]{owner.OwnerID}");
                        owner.CurrentCharacter.Desc = msg.Content.ToString();
                        Toolbox.uDebugAddLog($"Changed description, new: {owner.CurrentCharacter.Desc} [ID]{owner.OwnerID}");
                        setupDone = true;
                        await context.Channel.SendMessageAsync($"{owner.CurrentCharacter.Name}'s description has been changed to : {msg.Content.ToString()}");
                    }
                if (timeStamp + TimeSpan.FromMinutes(5) <= DateTime.Now)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} A response hasn't been received within 5 min, description change canceled");
                    Toolbox.uDebugAddLog($"5min passed, canceling request [ID]{owner.OwnerID}");
                    return;
                }
                await Task.Delay(1000);
            }
        } 

        public static void UseItem(CommandContext context, OwnerProfile owner, Item item)
        {
            var iItem = owner.CurrentCharacter.Backpack.Stored.Find(x => x == item);
            if (iItem == null)
            {
                Toolbox.uDebugAddLog($"ERROR: Item wasn't found in backpack when it was pulled from backpack [Name] {item.Name} [Count] {item.Count}");
                Events.SendDiscordMessage(context, $"An error occured and the item you want to use wasn't found in your backpack after finding it... I know confusing but you should let the developer know (don't worry, I left it as your turn, I'm not heartless)");
                return;
            }
            var match = RPG.MatchList.Find(x => x.Owner == owner);
            if (match == null)
            {
                Toolbox.uDebugAddLog($"Match wasn't found for {owner.CurrentCharacter.Name}, canceling item use [ID]{owner.OwnerID}");
                Events.SendDiscordMessage(context, "You aren't currently in an active match, please start a match then try again");
                return;
            }
            Item tmpItem = (Item)iItem;
            switch (item.Type)
            {
                case ItemType.Buff:
                    Affliction bff = new Affliction()
                    {
                        Name = tmpItem.Name,
                        Positive = true,
                        TurnsActive = tmpItem.CalculateAfflictionTurnCount(),
                        Physical = tmpItem.Physical,
                        Magic = tmpItem.Magic,
                        Fire = tmpItem.Fire,
                        Lightning = tmpItem.Lightning,
                        Ice = tmpItem.Ice,
                        Wind = tmpItem.Wind
                    };
                    owner.CurrentCharacter.StatusEffects.Add(bff);
                    Toolbox.uDebugAddLog($"{owner.CurrentCharacter.Name} added affliction to themself [ID]{owner.OwnerID} {bff.EnumPropsLogging()}");
                    Events.SendDiscordMessage(context, $"{owner.CurrentCharacter.Name} used {tmpItem.Name} on themself which will last {bff.TurnsActive} turns");
                    if (tmpItem.Count > 1)
                    {
                        Item bpItem = (Item)owner.CurrentCharacter.Backpack.Stored.Find(x => x == item);
                        Toolbox.uDebugAddLog($"Removing 1 from count on item {bpItem.Name}, count: {bpItem.Count} [ID]{owner.OwnerID}");
                        bpItem.Count -= 1;
                        Toolbox.uDebugAddLog($"Removed 1 from count on item {bpItem.Name}, count: {bpItem.Count} [ID]{owner.OwnerID}");
                    }
                    else
                    {
                        owner.CurrentCharacter.Backpack.Stored.Remove(iItem);
                        Toolbox.uDebugAddLog($"Removed {iItem.Name} from {owner.CurrentCharacter}'s backpack as it had a count of {tmpItem.Count}");
                    }
                    CalculateTurn(context, owner);
                    break;
                case ItemType.Damaging:
                    Affliction dmg = new Affliction()
                    {
                        Name = tmpItem.Name,
                        Positive = false,
                        TurnsActive = tmpItem.CalculateAfflictionTurnCount(),
                        Physical = tmpItem.Physical,
                        Magic = tmpItem.Magic,
                        Fire = tmpItem.Fire,
                        Lightning = tmpItem.Lightning,
                        Ice = tmpItem.Ice,
                        Wind = tmpItem.Wind
                    };
                    match.CurrentEnemy.StatusEffects.Add(dmg);
                    Toolbox.uDebugAddLog($"{owner.CurrentCharacter.Name} added affliction to {match.CurrentEnemy.Name} [ID]{owner.OwnerID} {dmg.EnumPropsLogging()}");
                    Events.SendDiscordMessage(context, $"{owner.CurrentCharacter.Name} used {tmpItem.Name} on {match.CurrentEnemy.Name} which will last {dmg.TurnsActive} turns");
                    if (tmpItem.Count > 1)
                    {
                        Item bpItem = (Item)owner.CurrentCharacter.Backpack.Stored.Find(x => x == item);
                        Toolbox.uDebugAddLog($"Removing 1 from count on item {bpItem.Name}, count: {bpItem.Count} [ID]{owner.OwnerID}");
                        bpItem.Count -= 1;
                        Toolbox.uDebugAddLog($"Removed 1 from count on item {bpItem.Name}, count: {bpItem.Count} [ID]{owner.OwnerID}");
                    }
                    else
                    {
                        owner.CurrentCharacter.Backpack.Stored.Remove(iItem);
                        Toolbox.uDebugAddLog($"Removed {iItem.Name} from {owner.CurrentCharacter}'s backpack as it had a count of {tmpItem.Count}");
                    }
                    CalculateTurn(context, owner);
                    break;
                case ItemType.Repair:
                    Toolbox.uDebugAddLog($"Repairing {owner.CurrentCharacter.Name}'s {owner.CurrentCharacter.Weapon.Name}. [Current]{owner.CurrentCharacter.Weapon.CurrentDurability}/{owner.CurrentCharacter.Weapon.MaxDurability} [ID]{owner.OwnerID}");
                    owner.CurrentCharacter.Weapon.CurrentDurability = owner.CurrentCharacter.Weapon.MaxDurability;
                    Toolbox.uDebugAddLog($"Repaired {owner.CurrentCharacter.Name}'s {owner.CurrentCharacter.Weapon.Name}. [After]{owner.CurrentCharacter.Weapon.CurrentDurability}/{owner.CurrentCharacter.Weapon.MaxDurability} [ID]{owner.OwnerID}");
                    Events.SendDiscordMessage(context, $"{owner.CurrentCharacter.Name}'s {owner.CurrentCharacter.Weapon.Name} has been repaired, current durability: {owner.CurrentCharacter.Weapon.CurrentDurability}/{owner.CurrentCharacter.Weapon.MaxDurability} [ID]{owner.OwnerID}");
                    if (tmpItem.Count > 1)
                    {
                        Item bpItem = (Item)owner.CurrentCharacter.Backpack.Stored.Find(x => x == item);
                        Toolbox.uDebugAddLog($"Removing 1 from count on item {bpItem.Name}, count: {bpItem.Count} [ID]{owner.OwnerID}");
                        bpItem.Count -= 1;
                        Toolbox.uDebugAddLog($"Removed 1 from count on item {bpItem.Name}, count: {bpItem.Count} [ID]{owner.OwnerID}");
                    }
                    else
                    {
                        owner.CurrentCharacter.Backpack.Stored.Remove(iItem);
                        Toolbox.uDebugAddLog($"Removed {iItem.Name} from {owner.CurrentCharacter}'s backpack as it had a count of {tmpItem.Count}");
                    }
                    CalculateTurn(context, owner);
                    break;
                case ItemType.Restorative:
                    if (item.Name.ToLower().Contains("health"))
                    {
                        owner.CurrentCharacter.CurrentHP += item.CalculateHealthPotion(owner.CurrentCharacter);
                        Events.SendDiscordMessage(context, $"**{owner.CurrentCharacter.Name}** was health was restored by {item.CalculateHealthPotion(owner.CurrentCharacter)}, current health: {owner.CurrentCharacter.CurrentHP}/{owner.CurrentCharacter.MaxHP} ({item.Count - 1} left)");
                        CalculateTurn(context, owner);
                    }
                    else if (item.Name.ToLower().Contains("mana"))
                    {
                        owner.CurrentCharacter.CurrentMana += item.CalculateManaPotion(owner.CurrentCharacter);
                        Events.SendDiscordMessage(context, $"**{owner.CurrentCharacter.Name}'s** mana was restored by {item.CalculateManaPotion(owner.CurrentCharacter)}, current mana: {owner.CurrentCharacter.CurrentMana}/{owner.CurrentCharacter.MaxMana} ({item.Count - 1} left)");
                        CalculateTurn(context, owner);
                    }
                    else
                    {
                        Toolbox.uDebugAddLog($"Something happened and the restorative item didn't contain health or mana: {LootDrop.GetLootInfo(iItem)} [ID]{owner.OwnerID}");
                        Events.SendDiscordMessage(context, "Something happened and the restorative wasn't for health for mana, please let the dev know");
                        return;
                    }
                    if (item.Count > 1)
                    {
                        Toolbox.uDebugAddLog($"{owner.CurrentCharacter.Name} currently has {item.Count} {item.Name} in their bag, removing one [ID]{owner.OwnerID}");
                        Item _item = (Item)owner.CurrentCharacter.Backpack.Stored.Find(x => x == item);
                        _item.Count -= 1;
                        Toolbox.uDebugAddLog($"Removed 1 count from {iItem.Name}, count: {_item.Count} [ID]{owner.OwnerID}");
                    }
                    else
                    {
                        Item _item = (Item)owner.CurrentCharacter.Backpack.Stored.Find(x => x == item);
                        owner.CurrentCharacter.Backpack.Stored.Remove(iItem);
                        Toolbox.uDebugAddLog($"Removed item from backpack: {iItem.Name}, count was {_item.Count} [ID]{owner.OwnerID}");
                    }
                    CalculateTurn(context, owner);
                    break;
                default:
                    Toolbox.uDebugAddLog($"ERROR: Item was used and wasn't one of the 4 item types. [Char]{owner.CurrentCharacter.Name} [ID]{owner.OwnerID} [Item]{tmpItem.EnumPropsLogging()}");
                    Events.SendDiscordMessage(context, $"Something happend and you couldn't use the item {tmpItem.Name}, please let the devs know. You still have your turn");
                    break;
            }
        }

        public static void NextEnemy(CommandContext context, OwnerProfile owner, Enemy enemy)
        {
            try
            {
                Toolbox.uDebugAddLog($"{owner.OwnerID} defeated {enemy.Name}, switching enemies");
                Match match = RPG.MatchList.Find(x => x.Owner == owner);
                match.EnemyList.Remove(enemy);
                Toolbox.uDebugAddLog($"Removed {enemy.Name} from the enemy list");
                Enemy newEnemy = match.EnemyList[0];
                RPG.MatchList.Find(x => x.Owner == owner).CurrentEnemy = newEnemy;
                RPG.MatchList.Find(x => x.Owner == owner).CurrentTurn = Turn.NotChosen;
                Toolbox.uDebugAddLog($"Changed {enemy.Name} to the current enemy");
                Events.SendDiscordMessage(context, $"You have defeated **{enemy.Name}** and are now fighting **{newEnemy.Name}**. Enemies left: {match.EnemyList.Count}");
                CalculateTurn(context, owner);
            }
            catch (Exception ex)
            {
                Toolbox.FullExceptionLog(ex);
            }
        }

        public static void CalculateTurn(CommandContext context, OwnerProfile owner)
        {
            try
            {
                string result = string.Empty;
                var match = RPG.MatchList.Find(x => x.Owner == owner);
                if (match.CurrentTurn == Turn.NotChosen)
                {
                    Toolbox.uDebugAddLog($"Initial Calculate Turn: [T]{match.CurrentTurn} [PST]{match.PlayerSpeedTime} [EST]{match.EnemySpeedTime} [PTT]{match.PlayerTurnTime} [ETT]{match.EnemyTurnTime} [ID]{owner.OwnerID}");
                    match.PlayerSpeedTime = owner.CurrentCharacter.CalculateSpeed();
                    match.EnemySpeedTime = match.CurrentEnemy.CalculateSpeed();
                    match.PlayerTurnTime = match.PlayerSpeedTime;
                    match.EnemyTurnTime = match.EnemySpeedTime;
                    Toolbox.uDebugAddLog($"Initial Calculated Turn: [T]{match.CurrentTurn} [PST]{match.PlayerSpeedTime} [EST]{match.EnemySpeedTime} [PTT]{match.PlayerTurnTime} [ETT]{match.EnemyTurnTime} [ID]{owner.OwnerID}");
                }
                if (match.PlayerTurnTime > match.EnemyTurnTime)
                {
                    Toolbox.uDebugAddLog($"Calculate Turn: [T]{match.CurrentTurn} [PST]{match.PlayerSpeedTime} [EST]{match.EnemySpeedTime} [PTT]{match.PlayerTurnTime} [ETT]{match.EnemyTurnTime} [ID]{owner.OwnerID}");
                    match.CurrentTurn = Turn.Enemy;
                    match.PlayerTurnTime = match.PlayerTurnTime - match.EnemyTurnTime;
                    match.EnemyTurnTime = match.EnemySpeedTime;
                    Toolbox.uDebugAddLog($"Calculated Turn: [T]{match.CurrentTurn} [PST]{match.PlayerSpeedTime} [EST]{match.EnemySpeedTime} [PTT]{match.PlayerTurnTime} [ETT]{match.EnemyTurnTime} [ID]{owner.OwnerID}");
                    AttackCharacter(context, match.CurrentEnemy, owner);
                    match.Turns += 1;
                    RemoveAfflictions(context, owner, match.CurrentEnemy);
                    CalculateTurn(context, owner);
                    return;
                }
                else
                {
                    Toolbox.uDebugAddLog($"Calculate Turn: [T]{match.CurrentTurn} [PST]{match.PlayerSpeedTime} [EST]{match.EnemySpeedTime} [PTT]{match.PlayerTurnTime} [ETT]{match.EnemyTurnTime} [ID]{owner.OwnerID}");
                    match.CurrentTurn = Turn.Player;
                    match.Turns += 1;
                    match.EnemyTurnTime = match.EnemyTurnTime - match.PlayerTurnTime;
                    match.PlayerTurnTime = match.PlayerSpeedTime;
                    Toolbox.uDebugAddLog($"Calculated Turn: [T]{match.CurrentTurn} [PST]{match.PlayerSpeedTime} [EST]{match.EnemySpeedTime} [PTT]{match.PlayerTurnTime} [ETT]{match.EnemyTurnTime} [ID]{owner.OwnerID}");
                    Events.SendDiscordMessage(context, $"It is **{owner.CurrentCharacter.Name}'s** turn");
                    RemoveAfflictions(context, owner, match.CurrentEnemy);
                    return;
                }
            }
            catch (Exception ex)
            {
                Toolbox.FullExceptionLog(ex);
            }
        }

        public static int CalculateSpeed(this Character character)
        {
            return (10000 - (character.Spd + character.Armor.Speed + character.Weapon.Speed));
        }

        public static int CalculateSpeed(this Enemy enemy)
        {
            return (10000 - (enemy.Spd + enemy.Armor.Speed + enemy.Weapon.Speed));
        }

        public static int CalculateElement(int attackDmg, int armorDef)
        {
            Toolbox.uDebugAddLog($"Calculating Element Damage: [attack]{attackDmg} [defense]{armorDef}");
            if (armorDef > 100)
                return -((attackDmg) / (100 / (armorDef - 100)));
            else if (armorDef == 100)
                return 0;
            else if (armorDef == 0)
                return attackDmg;
            else
                return ((attackDmg) / (100 / (armorDef)));
        }

        public static void RemoveAfflictions(CommandContext context, OwnerProfile owner, Enemy enemy)
        {
            Toolbox.uDebugAddLog("Starting affliction removal");
            var match = RPG.MatchList.Find(x => x.Owner == owner);
            foreach (var affl in owner.CurrentCharacter.StatusEffects)
            {
                if (match.Turns >= (affl.TurnStarted + (affl.TurnsActive * 2)))
                {
                    owner.CurrentCharacter.StatusEffects.Remove(affl);
                    Toolbox.uDebugAddLog($"Removed affliction {affl.Name} from {owner.CurrentCharacter.Name} on turn {match.Turns} [afflStart]{affl.TurnStarted} [afflActive]{affl.TurnsActive} [afflTotalActive]{affl.TurnsActive * 2} [ID]{owner.OwnerID}");
                    Events.SendDiscordMessage(context, $"Affliction {affl.Name} has been removed from {owner.CurrentCharacter.Name} after {affl.TurnsActive} turns");
                }
            }
            foreach (var affl in enemy.StatusEffects)
            {
                if (match.Turns >= (affl.TurnStarted + (affl.TurnsActive * 2)))
                {
                    enemy.StatusEffects.Remove(affl);
                    Toolbox.uDebugAddLog($"Removed affliction {affl.Name} from {enemy.Name} on turn {match.Turns} [afflStart]{affl.TurnStarted} [afflActive]{affl.TurnsActive} [afflTotalActive]{affl.TurnsActive * 2} [ID]{owner.OwnerID}");
                    Events.SendDiscordMessage(context, $"Affliction {affl.Name} has been removed from {enemy.Name} after {affl.TurnsActive} turns");
                }
            }
            Toolbox.uDebugAddLog("Finished affliction removal");
        }

        #endregion
    }

    public class LootDrop
    {
        #region LootDrop Variables

        /// <summary>
        /// The range of the probability values (dividing a value in _lootProbabilites by this would give a probability in the range 0..1).
        /// </summary>
        protected const int MaxProbability = 1000;

        /// <summary>
        /// The loot types.
        /// </summary>
        public enum LootType
        {
            Item, Armor, Weapon, Spell, Nothing
        };

        /// <summary>
        /// Cumulative probabilities - each entry corresponds to the member of LootType in the corresponding position.
        /// </summary>
        protected static int[] LootProbabilites(CharacterClass charClass)
        {
            if (charClass == CharacterClass.Mage)
                return new int[] { 600, 700, 750, 850, MaxProbability }; // Chances: Item(60%), Armor(10%), Weapon(5%), Spell(10%), Nothing(13%)
            else
                return new int[] { 600, 700, 800, 850, MaxProbability }; // Chances: Item(60%), Armor(10%), Weapon(10%), Spell(5%), Nothing(13%)
        }

        /// <summary>
        /// Cumulative probabilities - each entry corresponds to the member of RarityType in the corresponding position.
        /// </summary>
        protected static int[] _rarityProbabilities = new int[]
        {
            430, 730, 880, 980, MaxProbability // Chances: Common(43%), Uncommon(30%), Rare(15%), Epic(10%), Legendary(2%)
        };

        #endregion

        #region LootDrop General

        /// <summary>
        /// Choose a random loot type.
        /// </summary>
        public static LootType ChooseLootType(CharacterClass charClass)
        {
            LootType lootType = 0;         // start at first one
            int randomValue = rng.Next(MaxProbability);
            var lootProb = LootProbabilites(charClass);
            while (lootProb[(int)lootType] <= randomValue)
            {
                lootType++;         // next loot type
            }
            return lootType;
        }

        /// <summary>
        /// Return a random loot item
        /// </summary>
        /// <returns></returns>
        public static IBackPackItem PickLoot(Character character)
        {
            IBackPackItem chosenLoot = null;
            LootType chosenType = LootDrop.ChooseLootType(character.Class);
            RarityType rarityType = ChooseRarity();
            switch (chosenType)
            {
                case LootType.Item:
                    chosenLoot = ItemPicker(rarityType, character);
                    break;
                case LootType.Nothing:
                    chosenLoot = new Pebble();
                    break;
                case LootType.Armor:
                    chosenLoot = ArmorPicker(rarityType, character);
                    break;
                case LootType.Weapon:
                    chosenLoot = WeaponPicker(rarityType, character);
                    break;
                case LootType.Spell:
                    chosenLoot = SpellPicker(rarityType, character);
                    break;
            }
            return chosenLoot;
        }

        public static RarityType ChooseRarity()
        {

            RarityType itemRarity = 0;  // start at first one
            int randomValue = rng.Next(MaxProbability);
            while (_rarityProbabilities[(int)itemRarity] <= randomValue)
            {
                itemRarity++;
            }

            return itemRarity;
        }

        public static int GetRarityValue(RarityType type)
        {
            int rarityValue = 0;
            switch (type)
            {
                case (RarityType.Common):
                    rarityValue = 3;
                    break;
                case (RarityType.Uncommon):
                    rarityValue = 6;
                    break;
                case (RarityType.Rare):
                    rarityValue = 10;
                    break;
                case (RarityType.Epic):
                    rarityValue = 15;
                    break;
                case (RarityType.Legendary):
                    rarityValue = 21;
                    break;
            }
            return rarityValue;
        }

        public static ElementType ChooseElement()
        {
            ElementType element = 0;
            int[] elementChances = new int[] { 166, 333, 500, 666, 833, 1000 }; // Physical(16%), Magic(16%), Fire(16%), Lightning(16%), Ice(16%), Wind(16%)
            while (elementChances[(int)element] <= rng.Next(1000))
            {
                element++;
            }
            return element;
        }

        public static int ChooseLevel(int level)
        {
            if (level < maxLevel) level = level - 2 > 0 ? level + rng.Next(-2, 2) : level + rng.Next(0, 2);
            return level = level > maxLevel ? level = maxLevel : level;
        }

        public static int ChooseElementCount(RarityType rarity)
        {
            int typeCount = 0;
            switch (rarity)
            {
                case RarityType.Common:
                    typeCount = 1;
                    break;
                case RarityType.Uncommon:
                    typeCount = 1;
                    break;
                case RarityType.Rare:
                    typeCount = 2;
                    break;
                case RarityType.Epic:
                    typeCount = 3;
                    break;
                case RarityType.Legendary:
                    typeCount = 4;
                    break;
            }
            return typeCount;
        }

        public static LootType GetLootType(IBackPackItem bpItem)
        {
            if (bpItem is Item)
                return LootType.Item;
            else if (bpItem is Weapon)
                return LootType.Weapon;
            else if (bpItem is Armor)
                return LootType.Armor;
            else if (bpItem is Spell)
                return LootType.Spell;
            else
                return LootType.Nothing;
        }

        public static string GetLootInfo(IBackPackItem loot)
        {
            string result = $"Loot Type: {loot.GetLootType()}";
            foreach (var prop in loot.GetType().GetProperties())
            {
                result = $"{result}{Environment.NewLine}{prop.Name}: {prop.GetValue(loot)}";
            }
            return result;
        }

        public static void FilterLoot(Character chara, out int pebblesAdded, out int currencyAdded)
        {
            Toolbox.uDebugAddLog($"Filtering out loot for {chara.Name} [ID]{chara.OwnerID}");
            OwnerProfile owner = RPG.Owners.Find(x => x.OwnerID == chara.OwnerID);
            pebblesAdded = 0;
            currencyAdded = 0;
            int filtered = 0;
            int total = chara.Loot.Count;
            var lootList = chara.Loot;
            foreach (var bpItem in lootList)
            {
                var lootType = bpItem.GetLootType();
                switch (lootType)
                {
                    case LootType.Nothing:
                        chara.Pebbles++;
                        owner.TotalPebbles++;
                        pebblesAdded++;
                        Toolbox.uDebugAddLog($"Added pebble to {chara.Name} and {owner.OwnerID} and removing from loot list for type {lootType}");
                        chara.Loot.Remove(bpItem);
                        break;
                    case LootType.Item:
                        Item item = (Item)bpItem;
                        if (item.Type == ItemType.Currency)
                        {
                            owner.Currency += item.Worth;
                            currencyAdded += item.Worth;
                            Toolbox.uDebugAddLog($"Added {item.Worth} currency to {owner.OwnerID} and removing from loot list for type {lootType}");
                            chara.Loot.Remove(bpItem);
                        }
                        break;
                }
            }
            Toolbox.uDebugAddLog($"Filtered loot:{filtered}/{total} [ID]{chara.OwnerID}");
        }

        #endregion

        #region LootDrop Items

        protected static int[] itemProbabilities = new int[]
        {
            300, 450, 600, 900, MaxProbability // Restorative(30%), Buff(15%), Damaging(15%), Currency(30%), Repair(10%)
        };

        public static int CurrencyPicker(int characterLevel, int rarityValue)
        {
            int currencyReturn = rng.Next(0, MaxProbability);
            if (currencyReturn < 300)
                currencyReturn = ((characterLevel * rarityValue) + rng.Next(1, 50));
            else if (currencyReturn >= 300 && currencyReturn < 500)
                currencyReturn = ((characterLevel * rarityValue) + rng.Next(50, 100));
            else if (currencyReturn >= 500 && currencyReturn < 700)
                currencyReturn = ((characterLevel * rarityValue) + rng.Next(100, 150));
            else if (currencyReturn >= 700 && currencyReturn < 850)
                currencyReturn = ((characterLevel * rarityValue) + rng.Next(150, 200));
            else if (currencyReturn >= 850 && currencyReturn < 900)
                currencyReturn = ((characterLevel * rarityValue) + rng.Next(200, 300));
            else if (currencyReturn >= 900 && currencyReturn < 950)
                currencyReturn = ((characterLevel * rarityValue) + rng.Next(300, 400));
            else if (currencyReturn >= 950 && currencyReturn < 998)
                currencyReturn = ((characterLevel * rarityValue) + rng.Next(400, 600));
            else if (currencyReturn >= 998)
                currencyReturn = ((characterLevel * rarityValue) + rng.Next(600, 2000));
            return currencyReturn;
        }

        public static Item RepairPicker()
        {
            var chance = ChanceRoll(5);
            if (chance)
                return Items.CopyNewItem(Items.repairPowderPack);
            else
                return Items.CopyNewItem(Items.repairPowder);
        }

        public static Item ItemPicker(RarityType rarity, Character character)
        {
            Item _item = new Item();
            int rarityValue = GetRarityValue(rarity);
            ItemType type = ChooseItemType();
            switch (type)
            {
                case ItemType.Currency:
                    _item = new Item { Worth = CurrencyPicker(character.Lvl, rarityValue), Type = type };
                    break;
                case ItemType.Restorative:
                    _item = Items.CopyNewItem(Items.itemRestorativeList[rng.Next(0, Items.itemRestorativeList.Count)]);
                    break;
                case ItemType.Buff:
                    _item = Items.CopyNewItem(Items.itemBuffList[rng.Next(0, Items.itemBuffList.Count)]);
                    break;
                case ItemType.Damaging:
                    _item = Items.CopyNewItem(Items.itemDamagingList[rng.Next(0, Items.itemDamagingList.Count)]);
                    break;
                case ItemType.Repair:
                    _item = RepairPicker();
                    break;
            }
            _item.Rarity = rarity;
            return _item;
        }

        public static ItemType ChooseItemType()
        {
            ItemType itemType = 0;
            int randomValue = rng.Next(MaxProbability);
            while (itemProbabilities[(int)itemType] <= randomValue)
            {
                itemType++;
            }
            return itemType;
        }

        #endregion

        #region LootDrop Weapon

        public static WeaponType ChooseWeaponType(Character character)
        {
            WeaponType weaponType = 0;
            int randomValue = rng.Next(MaxProbability);
            int[] weaponProbabilities = _weaponProbabilities(character.Class);
            while (weaponProbabilities[(int)weaponType] <= randomValue)
            {
                weaponType++;
            }

            return weaponType;
        }

        public static int[] _weaponProbabilities(CharacterClass charClass)
        {
            int[] weaponArray = new int[] { };
            switch (charClass)
            {
                case CharacterClass.Warrior:
                    weaponArray = new int[] { 250, 450, 650, 795, 890, 900, 920, 940, MaxProbability, 1001, 1001 }; // Sword(25%), Dagger(20%), Greatsword(20%), Katana(14.5%), Staff(9.5%), FocusStone(1%), Spear(2%), DragonSpear(2%), TwinSwords(6%), Other(0%), Starter(0%)
                    break;
                case CharacterClass.Dragoon:
                    weaponArray = new int[] { 100, 200, 250, 300, 350, 400, 600, 800, MaxProbability, 1001, 1001 }; // Sword(10%), Dagger(10%), Greatsword(5%), Katana(5%), Staff(5%), FocusStone(5%), Spear(20%), DragonSpear(20%), TwinSwords(20%), Other(0%), Starter(0%)
                    break;
                case CharacterClass.Mage:
                    weaponArray = new int[] { 50, 100, 150, 200, 550, 800, 850, 900, MaxProbability, 1001, 1001 }; // Sword(5%), Dagger(5%), Greatsword(5%), Katana(5%), Staff(55%), FocusStone(25%), Spear(5%), DragonSpear(5%), TwinSwords(10%), Other(0%), Starter(0%)
                    break;
                case CharacterClass.Necromancer:
                    weaponArray = new int[] { 50, 100, 150, 200, 450, 800, 850, 900, MaxProbability, 1001, 1001 }; // Sword(5%), Dagger(5%), Greatsword(5%), Katana(5%), Staff(25%), FocusStone(35%), Spear(5%), DragonSpear(5%), TwinSwords(10%), Other (0%), Starter(0%)
                    break;
                case CharacterClass.Rogue:
                    weaponArray = new int[] { 150, 450, 550, 600, 650, 700, 750, 770, MaxProbability, 1001, 1001 }; // Sword(15%), Dagger(30%), Greatsword(10%), Katana(5%), Staff(5%), FocusStone(5%), Spear(5%), DragonSpear(2%), TwinSwords(23%), Other(0%), Starter(0%)
                    break;
            }

            return weaponArray;
        }

        public static Weapon WeaponPicker(RarityType rarity, Character chara)
        {
            Weapon weap = new Weapon();
            int rarityValue = LootDrop.GetRarityValue(rarity);
            int level = ChooseLevel(chara.Lvl);
            WeaponType type = ChooseWeaponType(chara);
            var isUnique = ChanceRoll(30);
            if (isUnique)
                weap = Weapons.WeaponUniqueGen(rarity, type, rarityValue, level);
            else
                weap = Weapons.WeaponRandomGen(rarity, type, rarityValue, level);
            return weap;
        }

        #endregion

        #region LootDrop Armor

        public static ArmorType ChooseArmorType(Character chara)
        {
            ArmorType armorType = 0;
            int randomValue = rng.Next(MaxProbability);
            int[] armorProbabilities = _armorProbabilities(chara.Class);
            while (armorProbabilities[(int)armorType] <= randomValue)
            {
                armorType++;
            }
            return armorType;
        }

        protected static int[] _armorProbabilities(CharacterClass charClass)
        {
            int[] armorArray = new int[] { };
            switch (charClass)
            {
                case CharacterClass.Warrior:
                    armorArray = new int[] { 100, 400, MaxProbability }; // Light(10%), Medium(30%), Heavy(60%)
                    break;
                case CharacterClass.Dragoon:
                    armorArray = new int[] { 100, 600, MaxProbability }; // Light(10%), Medium(50%), Heavy(40%)
                    break;
                case CharacterClass.Mage:
                    armorArray = new int[] { 800, 900, MaxProbability }; // Light(80%), Medium(10%), Heavy(10%)
                    break;
                case CharacterClass.Necromancer:
                    armorArray = new int[] { 100, 800, MaxProbability }; // Light(10%), Medium(80%), Heavy(10%)
                    break;
                case CharacterClass.Rogue:
                    armorArray = new int[] { 100, 900, MaxProbability }; // Light(10%), Medium(80%), Heavy(10%)
                    break;
            }
            return armorArray;
        }

        public static Armor ArmorPicker(RarityType rarityType, Character chara)
        {
            Armor armor = new Armor();
            int rarityValue = LootDrop.GetRarityValue(rarityType);
            bool isUnique = ChanceRoll(30);
            armor.Type = ChooseArmorType(chara);
            armor.Rarity = rarityType;

            if (isUnique)
                return Armors.ArmorUniqueGen(armor, rarityType, rarityValue, chara.Lvl);
            else
                return Armors.ArmorRandomGen(rarityType, rarityValue, armor, chara.Lvl);
        }

        #endregion

        #region LootDrop Spells

        public static SpellType ChooseSpellType()
        {
            SpellType spellType = 0;
            int randomValue = rng.Next(MaxProbability);
            while (spellProbabilities[(int)spellType] <= randomValue)
            {
                spellType++;
            }
            return spellType;
        }

        public static int[] spellProbabilities = new int[]
        {
            333, 666, MaxProbability, MaxProbability // Attack(33%), Defense(33%), Restorative(33.1%), Starter(0%)
        };

        public static Spell SpellPicker(RarityType rarity, Character character)
        {
            Spell spell = new Spell();
            int rarityValue = LootDrop.GetRarityValue(rarity);
            spell.Type = ChooseSpellType();
            spell.Lvl = LootDrop.ChooseLevel(character.Lvl);
            var element = ChooseElement();
            bool isUnique = ChanceRoll(30);
            if (isUnique)
            {
                rarityValue += 2;
                return Spells.SpellUniqueGen(character, spell, rarity, element, spell.Type, rarityValue, character.Lvl);
            }
            else
                return Spells.SpellRandomGen(character, spell, rarity, element, spell.Type, rarityValue, character.Lvl);
        }

        public static Spell SpellPicker(RarityType rarity, Character character, out ElementType element)
        {
            Spell spell = new Spell();
            int rarityValue = LootDrop.GetRarityValue(rarity);
            spell.Type = ChooseSpellType();
            spell.Rarity = rarity;
            spell.Lvl = LootDrop.ChooseLevel(character.Lvl);
            element = ChooseElement();
            bool isUnique = ChanceRoll(30);
            if (isUnique)
            {
                rarityValue += 2;
                return Spells.SpellUniqueGen(character, spell, rarity, element, spell.Type, rarityValue, character.Lvl);
            }
            else
                return Spells.SpellRandomGen(character, spell, rarity, element, spell.Type, rarityValue, character.Lvl);
        }

        public static void ChooseSpellWorth(Spell spell, int rarityValue, bool isUnique = false)
        {
            int dmgTotal = spell.PhysicalDamage + spell.FireDamage + spell.IceDamage + spell.LightningDamage + spell.MagicDamage + spell.WindDamage;
            spell.Worth = (((spell.Lvl + rarityValue) * rarityValue) + (spell.Speed / (dmgTotal)));
            if (isUnique)
                spell.Worth = spell.Worth + ((spell.Lvl * rarityValue) + ((spell.Speed * rarityValue) / dmgTotal));
        }

        #endregion
    }

    public static class Weapons
    {
        #region Weapon Names and Descriptions

        public static string[] weaponNamesSword(RarityType rarity)
        {
            return new string[] 
            {
                $"{rarity} Not a weapon",
                $"{rarity} Butterknife",
                $"{rarity} Pokes' you in the eye",
                $"{rarity} Cut you real bad",
                $"{rarity} A sword of sorts",
                $"{rarity} Stabby McStab Stab",
                $"{rarity} Sword from the 7th Floor"
            };
        }
        public static void WeaponSwordAddition(Weapon weap, int rarityValue, int level, int index)
        {
            switch (index)
            {
                case 0:
                    weap.Desc = "This isn't a weapon.... at least I don't think it is";
                    weap.PhysicalDamage = (level + rng.Next(2, 14) + rarityValue);
                    weap.Speed = (level + 100 + rng.Next(20, 80));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case 1:
                    weap.Desc = "Used to butter that toast, or butter your bread. Mmmmm bread";
                    weap.PhysicalDamage = (level + rng.Next(5, 12) + rarityValue);
                    weap.Speed = (level + 120 + rng.Next(30, 60));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case 2:
                    weap.Desc = "Everytime you attack this weapon makes an attempt to poke you in the eye... like really hard";
                    weap.PhysicalDamage = (level + rng.Next(8, 14) + rarityValue);
                    weap.Speed = (level + 100 + rng.Next(20, 50));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case 3:
                    weap.Desc = "I'm gonna cut you so bad, you gonna wish I never cut you so bad";
                    weap.PhysicalDamage = (level + rng.Next(8, 20) + rarityValue);
                    weap.Speed = (level + 120 + rng.Next(30, 60));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case 4:
                    weap.Desc = "This might be a sword, or might not, does it matter?";
                    weap.PhysicalDamage = (level + rng.Next(3, 11) + rarityValue);
                    weap.Speed = (level + 110 + rng.Next(30, 60));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case 5:
                    weap.Desc = "Stab. Stabby. Stab, stab stab. Stab stab stab stabby Mcstab stab. Stab, stab stab.";
                    weap.PhysicalDamage = (level + rng.Next(5, 22) + rarityValue);
                    weap.Speed = (level + 100 + rng.Next(10, 80));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case 6:
                    weap.Desc = "Ya blew it";
                    weap.PhysicalDamage = (level + rng.Next(1, 30) + rarityValue);
                    weap.Speed = (level + 100 + rng.Next(10, 100));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
            }
        }
        public static string[] weaponNamesGreatsword(RarityType rarity)
        {
            return new string[]
            {
                $"{rarity} Not a weapon",
                $"{rarity} Butterknife",
                $"{rarity} Pokes' you in the eye",
                $"{rarity} Cut you real bad",
                $"{rarity} A sword of sorts",
                $"{rarity} Stabby McStab Stab",
                $"{rarity} Chainsword",
                $"{rarity} \"Hammer\" of Thor"
            };
        }
        public static void WeaponGreatSwordAddition(Weapon weap, int rarityValue, int level, int index)
        {
            switch (index)
            {
                case 0:
                    weap.Desc = "This isn't a weapon.... at least I don't think it is";
                    weap.PhysicalDamage = (level + rng.Next(10, 23) + rarityValue);
                    weap.Speed = (level + 70 + rng.Next(30, 60));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case 1:
                    weap.Desc = "Used to butter that toast, or butter your bread. Mmmmm bread";
                    weap.PhysicalDamage = (level + rng.Next(8, 25) + rarityValue);
                    weap.Speed = (level + 100 + rng.Next(20, 80));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case 2:
                    weap.Desc = "Everytime you attack this weapon makes an attempt to poke you in the eye... like really hard";
                    weap.PhysicalDamage = (level + rng.Next(12, 18) + rarityValue);
                    weap.Speed = (level + 110 + rng.Next(20, 70));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case 3:
                    weap.Desc = "I'm gonna cut you so bad, you gonna wish I never cut you so bad";
                    weap.PhysicalDamage = (level + rng.Next(8, 27) + rarityValue);
                    weap.Speed = (level + 60 + rng.Next(50, 80));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case 4:
                    weap.Desc = "This might be a sword, or might not, does it matter?";
                    weap.PhysicalDamage = (level + rng.Next(2, 30) + rarityValue);
                    weap.Speed = (level + 50 + rng.Next(20, 80));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case 5:
                    weap.Desc = "Stab. Stabby. Stab, stab stab. Stab stab stab stabby Mcstab stab. Stab, stab stab.";
                    weap.PhysicalDamage = (level + rng.Next(12, 20) + rarityValue);
                    weap.Speed = (level + 80 + rng.Next(40, 70));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case 6:
                    weap.Desc = "A serrated sword that, when the magic word \"Gettum!\" is spoken it causes the serrations to spin around the blade.";
                    weap.PhysicalDamage = (level + rng.Next(5, 28) + rarityValue);
                    weap.Speed = (level + 90 + rng.Next(20, 50));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case 7:
                    weap.Desc = "A one handed \"Hammer\" that deals crushing / electric damage";
                    weap.PhysicalDamage = (level + rng.Next(10, 25) + rarityValue);
                    weap.LightningDamage = (level + rng.Next(5, 15) + rarityValue);
                    weap.Speed = (level + 90 + rng.Next(20, 70));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
            }
        }
        public static string[] weaponNamesDagger(RarityType rarity)
        {
            return new string[]
            {
                $"{rarity} Not a weapon",
                $"{rarity} Butterknife",
                $"{rarity} Pokes' you in the eye",
                $"{rarity} Cut you real bad",
                $"{rarity} Stabby McStab Stab"
            };
        }
        public static void WeaponDaggerAddition(Weapon weap, int rarityValue, int level, int index)
        {
            switch (index)
            {
                case 0:
                    weap.Desc = "This isn't a weapon.... at least I don't think it is";
                    weap.PhysicalDamage = (level + rng.Next(2, 8) + rarityValue);
                    weap.Speed = (level + 120 + rng.Next(30, 110));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case 1:
                    weap.Desc = "Used to butter that toast, or butter your bread. Mmmmm bread";
                    weap.PhysicalDamage = (level + rng.Next(6, 10) + rarityValue);
                    weap.Speed = (level + 130 + rng.Next(70, 100));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case 2:
                    weap.Desc = "Everytime you attack this weapon makes an attempt to poke you in the eye... like really hard";
                    weap.PhysicalDamage = (level + rng.Next(5, 11) + rarityValue);
                    weap.Speed = (level + 120 + rng.Next(30, 110));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case 3:
                    weap.Desc = "I'm gonna cut you so bad, you gonna wish I never cut you so bad";
                    weap.PhysicalDamage = (level + rng.Next(8, 15) + rarityValue);
                    weap.Speed = (level + 100 + rng.Next(20, 90));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case 4:
                    weap.Desc = "Stab. Stabby. Stab, stab stab. Stab stab stab stabby Mcstab stab. Stab, stab stab.";
                    weap.PhysicalDamage = (level + rng.Next(6, 13) + rarityValue);
                    weap.Speed = (level + 140 + rng.Next(60, 100));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
            }
        }
        public static string[] weaponNamesKatana(RarityType rarity)
        {
            return new string[]
            {
                $"{rarity} Not a weapon",
                $"{rarity} Butterknife",
                $"{rarity} Pokes' you in the eye",
                $"{rarity} Cut you real bad",
                $"{rarity} Stabby McStab Stab"
            };
        }
        public static void WeaponKatanaAddition(Weapon weap, int rarityValue, int level, int index)
        {
            switch (index)
            {
                case 0:
                    weap.Desc = "This isn't a weapon.... at least I don't think it is";
                    weap.PhysicalDamage = (level + rng.Next(7, 17) + rarityValue);
                    weap.Speed = (level + 110 + rng.Next(20, 100));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case 1:
                    weap.Desc = "Used to butter that toast, or butter your bread. Mmmmm bread";
                    weap.PhysicalDamage = (level + rng.Next(9, 18) + rarityValue);
                    weap.Speed = (level + 130 + rng.Next(40, 130));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case 2:
                    weap.Desc = "Everytime you attack this weapon makes an attempt to poke you in the eye... like really hard";
                    weap.PhysicalDamage = (level + rng.Next(10, 17) + rarityValue);
                    weap.Speed = (level + 120 + rng.Next(20, 90));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case 3:
                    weap.Desc = "I'm gonna cut you so bad, you gonna wish I never cut you so bad";
                    weap.PhysicalDamage = (level + rng.Next(12, 19) + rarityValue);
                    weap.Speed = (level + 110 + rng.Next(40, 120));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case 4:
                    weap.Desc = "Stab. Stabby. Stab, stab stab. Stab stab stab stabby Mcstab stab. Stab, stab stab.";
                    weap.PhysicalDamage = (level + rng.Next(8, 16) + rarityValue);
                    weap.Speed = (level + 150 + rng.Next(10, 80));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
            }
        }
        public static string[] weaponNamesStaff(RarityType rarity)
        {
            return new string[]
            {
                $"{rarity} Not a weapon",
                $"{rarity} Pokes' you in the eye"
            };
        }
        public static void WeaponStaffAddition(Weapon weap, int rarityValue, int level, int index)
        {
            switch (index)
            {
                case 0:
                    weap.Desc = "This isn't a weapon.... at least I don't think it is";
                    weap.PhysicalDamage = (level + rng.Next(2, 8) + rarityValue);
                    weap.MagicDamage = (level + rng.Next(8, 15) + rarityValue);
                    weap.Speed = (level + 90 + rng.Next(10, 90));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.MagicDamage));
                    break;
                case 1:
                    weap.Desc = "Everytime you attack this weapon makes an attempt to poke you in the eye... like really hard";
                    weap.PhysicalDamage = (level + rng.Next(5, 10) + rarityValue);
                    weap.MagicDamage = (level + rng.Next(5, 20) + rarityValue);
                    weap.Speed = (level + 50 + rng.Next(10, 150));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.MagicDamage));
                    break;
            }
        }
        public static string[] weaponNamesFocusStone(RarityType rarity)
        {
            return new string[]
            {
                $"{rarity} Not a weapon",
                $"{rarity} Pokes' you in the eye"
            };
        }
        public static void WeaponFocusStoneAddition(Weapon weap, int rarityValue, int level, int index)
        {
            switch (index)
            {
                case 0:
                    weap.Desc = "This isn't a weapon.... at least I don't think it is";
                    weap.PhysicalDamage = (level + rng.Next(1, 4) + rarityValue);
                    weap.MagicDamage = (level + rng.Next(5, 12) + rarityValue);
                    weap.Speed = (level + 90 + rng.Next(60, 110));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.MagicDamage));
                    break;
                case 1:
                    weap.Desc = "Everytime you attack this weapon makes an attempt to poke you in the eye... like really hard";
                    weap.PhysicalDamage = (level + rng.Next(2, 6) + rarityValue);
                    weap.MagicDamage = (level + rng.Next(4, 10) + rarityValue);
                    weap.Speed = (level + 100 + rng.Next(50, 100));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.MagicDamage));
                    break;
            }
        }
        public static string[] weaponNamesSpear(RarityType rarity)
        {
            return new string[]
            {
                $"{rarity} Not a weapon",
                $"{rarity} Pokes' you in the eye"
            };
        }
        public static void WeaponSpearAddition(Weapon weap, int rarityValue, int level, int index)
        {
            switch (index)
            {
                case 0:
                    weap.Desc = "This isn't a weapon.... at least I don't think it is";
                    weap.PhysicalDamage = (level + rng.Next(8, 14) + rarityValue);
                    weap.Speed = (level + 100 + rng.Next(30, 90));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case 1:
                    weap.Desc = "Everytime you attack this weapon makes an attempt to poke you in the eye... like really hard";
                    weap.PhysicalDamage = (level + rng.Next(10, 18) + rarityValue);
                    weap.Speed = (level + 90 + rng.Next(10, 160));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
            }
        }
        public static string[] weaponNamesDragonSpear(RarityType rarity)
        {
            return new string[]
            {
                $"{rarity} Not a weapon",
                $"{rarity} Pokes' you in the eye",
                $"{rarity} Spear of the Ultres Dragon"
            };
        }
        public static void WeaponDragonSpearAddition(Weapon weap, int rarityValue, int level, int index)
        {
            switch (index)
            {
                case 0:
                    weap.Desc = "This isn't a weapon.... at least I don't think it is";
                    weap.PhysicalDamage = (level + rng.Next(5, 10) + rarityValue);
                    weap.FireDamage = ChanceRoll(30) ? level + rng.Next(0, 7) : 0;
                    weap.IceDamage = ChanceRoll(30) ? level + rng.Next(0, 7) : 0;
                    weap.LightningDamage = ChanceRoll(30) ? level + rng.Next(0, 7) : 0;
                    weap.WindDamage = ChanceRoll(30) ? level + rng.Next(0, 7) : 0;
                    weap.Speed = (level + 90 + rng.Next(10, 100));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed + (weap.FireDamage + weap.IceDamage + weap.LightningDamage + weap.WindDamage) / weap.PhysicalDamage));
                    break;
                case 1:
                    weap.Desc = "Everytime you attack this weapon makes an attempt to poke you in the eye... like really hard";
                    weap.PhysicalDamage = (level + rng.Next(10, 20) + rarityValue);
                    weap.FireDamage = ChanceRoll(30) ? level + rng.Next(0, 10) : 0;
                    weap.IceDamage = ChanceRoll(30) ? level + rng.Next(0, 10) : 0;
                    weap.LightningDamage = ChanceRoll(30) ? level + rng.Next(0, 10) : 0;
                    weap.WindDamage = ChanceRoll(30) ? level + rng.Next(0, 10) : 0;
                    weap.Speed = (level + 70 + rng.Next(30, 70));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed + (weap.FireDamage + weap.IceDamage + weap.LightningDamage + weap.WindDamage) / weap.PhysicalDamage));
                    break;
                case 2:
                    weap.Desc = "Spear bestowed to an ancient dragon tamer by the Great Dragon Ultres, shits waaaaaaaaaaack!";
                    weap.PhysicalDamage = (level + rng.Next(12, 30) + rarityValue);
                    weap.FireDamage = ChanceRoll(30) ? level + rng.Next(0, 7) : 0;
                    weap.IceDamage = ChanceRoll(30) ? level + rng.Next(0, 7) : 0;
                    weap.LightningDamage = ChanceRoll(30) ? level + rng.Next(10, 20) : rng.Next(5, 12);
                    weap.WindDamage = ChanceRoll(30) ? level + rng.Next(10, 20) : rng.Next(5, 12);
                    weap.Speed = (level + 100 + rng.Next(0, 100));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed + (weap.FireDamage + weap.IceDamage + weap.LightningDamage + weap.WindDamage) / weap.PhysicalDamage));
                    break;
            }
        }
        public static string[] weaponNamesTwinSwords(RarityType rarity)
        {
            return new string[]
            {
                $"{rarity} Not a weapon",
                $"{rarity} Butterknife",
                $"{rarity} Pokes' you in the eye",
                $"{rarity} Cut you real bad",
                $"{rarity} Stabby McStab Stab"
            };
        }
        public static void WeaponTwinSwordsAddition(Weapon weap, int rarityValue, int level, int index)
        {
            switch (index)
            {
                case 0:
                    weap.Desc = "This isn't a weapon.... at least I don't think it is";
                    weap.PhysicalDamage = ((level + rng.Next(2, 6) * 2) + rarityValue);
                    weap.Speed = (level + 90 + rng.Next(20, 70));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case 1:
                    weap.Desc = "Used to butter that toast, or butter your bread. Mmmmm bread";
                    weap.PhysicalDamage = ((level + rng.Next(1, 10) * 2) + rarityValue);
                    weap.Speed = (level + 100 + rng.Next(10, 80));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case 2:
                    weap.Desc = "Everytime you attack this weapon makes an attempt to poke you in the eye... like really hard";
                    weap.PhysicalDamage = ((level + rng.Next(3, 8) * 2) + rarityValue);
                    weap.Speed = (level + 90 + rng.Next(20, 70));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case 3:
                    weap.Desc = "I'm gonna cut you so bad, you gonna wish I never cut you so bad";
                    weap.PhysicalDamage = ((level + rng.Next(4, 10) * 2) + rarityValue);
                    weap.Speed = (level + 90 + rng.Next(30, 60));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case 4:
                    weap.Desc = "Stab. Stabby. Stab, stab stab. Stab stab stab stabby Mcstab stab. Stab, stab stab.";
                    weap.PhysicalDamage = ((level + rng.Next(3, 12) * 2) + rarityValue);
                    weap.Speed = (level + 100 + rng.Next(10, 100));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
            }
        }

        #endregion

        #region Weapon Methods

        public static Weapon WeaponRandomGen(RarityType rarity, WeaponType type, int rarityValue, int level)
        {
            Weapon weap = new Weapon()
            {
                Type = type,
                Rarity = rarity,
                Lvl = LootDrop.ChooseLevel(level),
                MaxDurability = (10 * level) + (rarityValue * 4)
            };
            weap.CurrentDurability = weap.MaxDurability;
            switch (type)
            {
                case WeaponType.Dagger:
                    weap.Name = $"{rarity} {type}";
                    weap.Desc = $"A {rarity} {type}";
                    weap.PhysicalDamage = (level + rng.Next(1, 5) + rarityValue);
                    weap.Speed = (level + 120 + rng.Next(40, 80));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case WeaponType.DragonSpear:
                    weap.Name = $"{rarity} {type}";
                    weap.Desc = $"A {rarity} {type}";
                    weap.PhysicalDamage = (level + rng.Next(3, 8) + rarityValue);
                    weap.FireDamage = ChanceRoll(30) ? level + rng.Next(0, 5) : 0;
                    weap.IceDamage = ChanceRoll(30) ? level + rng.Next(0, 5) : 0;
                    weap.LightningDamage = ChanceRoll(30) ? level + rng.Next(0, 5) : 0;
                    weap.WindDamage = ChanceRoll(30) ? level + rng.Next(0, 5) : 0;
                    weap.Speed = (level + 80 + rng.Next(10, 90));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed + (weap.FireDamage + weap.IceDamage + weap.LightningDamage + weap.WindDamage) / weap.PhysicalDamage));
                    break;
                case WeaponType.FocusStone:
                    weap.Name = $"{rarity} {type}";
                    weap.Desc = $"A {rarity} {type}";
                    weap.PhysicalDamage = (level + rng.Next(0, 2) + rarityValue);
                    weap.MagicDamage = (level + rng.Next(2, 8) + rarityValue);
                    weap.Speed = (level + 80 + rng.Next(20, 110));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.MagicDamage));
                    break;
                case WeaponType.Greatsword:
                    weap.Name = $"{rarity} {type}";
                    weap.Desc = $"A {rarity} {type}";
                    weap.PhysicalDamage = (level + rng.Next(8, 20) + rarityValue);
                    weap.Speed = (level + 70 + rng.Next(10, 40));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case WeaponType.Katana:
                    weap.Name = $"{rarity} {type}";
                    weap.Desc = $"A {rarity} {type}";
                    weap.PhysicalDamage = (level + rng.Next(5, 15) + rarityValue);
                    weap.Speed = (level + 100 + rng.Next(10, 110));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case WeaponType.Spear:
                    weap.Name = $"{rarity} {type}";
                    weap.Desc = $"A {rarity} {type}";
                    weap.PhysicalDamage = (level + rng.Next(4, 10) + rarityValue);
                    weap.Speed = (level + 90 + rng.Next(10, 110));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case WeaponType.Staff:
                    weap.Name = $"{rarity} {type}";
                    weap.Desc = $"A {rarity} {type}";
                    weap.PhysicalDamage = (level + rng.Next(0, 3) + rarityValue);
                    weap.MagicDamage = (level + rng.Next(4, 10) + rarityValue);
                    weap.Speed = (level + 70 + rng.Next(20, 80));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.MagicDamage));
                    break;
                case WeaponType.Sword:
                    weap.Name = $"{rarity} {type}";
                    weap.Desc = $"A {rarity} {type}";
                    weap.PhysicalDamage = (level + rng.Next(4, 10) + rarityValue);
                    weap.Speed = (level + 100 + rng.Next(20, 50));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case WeaponType.TwinSwords:
                    weap.Name = $"{rarity} {type}";
                    weap.Desc = $"{rarity} {type}";
                    weap.PhysicalDamage = ((level + rng.Next(1, 5) * 2) + rarityValue);
                    weap.Speed = (level + 80 + rng.Next(20, 70));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
            }
            return weap;
        }

        public static Weapon WeaponUniqueGen(RarityType rarity, WeaponType type, int rarityValue, int level)
        {
            Weapon weap = new Weapon();
            rarityValue += 2;
            weap.IsUnique = true;
            switch (type)
             {
                 case WeaponType.Dagger:
                    var dagNames = weaponNamesDagger(rarity);
                    int dagIndex = rng.Next(0, dagNames.ToArrayLength());
                    weap.Name = dagNames[dagIndex];
                    WeaponDaggerAddition(weap, rarityValue, level, dagIndex);
                     break;
                 case WeaponType.DragonSpear:
                    var dsNames = weaponNamesDragonSpear(rarity);
                    int dsIndex = rng.Next(0, dsNames.ToArrayLength());
                    weap.Name = dsNames[dsIndex];
                    WeaponDragonSpearAddition(weap, rarityValue, level, dsIndex);
                     break;
                 case WeaponType.FocusStone:
                    var fsNames = weaponNamesFocusStone(rarity);
                    int fsIndex = rng.Next(0, fsNames.ToArrayLength());
                    weap.Name = fsNames[fsIndex];
                    WeaponFocusStoneAddition(weap, rarityValue, level, fsIndex);
                     break;
                 case WeaponType.Greatsword:
                    var gsNames = weaponNamesGreatsword(rarity);
                    int gsIndex = rng.Next(0, gsNames.ToArrayLength());
                    weap.Name = gsNames[gsIndex];
                    WeaponGreatSwordAddition(weap, rarityValue, level, gsIndex);
                     break;
                 case WeaponType.Katana:
                    var ktNames = weaponNamesKatana(rarity);
                    int ktIndex = rng.Next(0, ktNames.ToArrayLength());
                    weap.Name = ktNames[ktIndex];
                    WeaponKatanaAddition(weap, rarityValue, level, ktIndex);
                     break;
                 case WeaponType.Spear:
                    var spNames = weaponNamesSpear(rarity);
                    int spIndex = rng.Next(0, spNames.ToArrayLength());
                    weap.Name = spNames[spIndex];
                    WeaponSpearAddition(weap, rarityValue, level, spIndex);
                     break;
                 case WeaponType.Staff:
                    var stNames = weaponNamesStaff(rarity);
                    int stIndex = rng.Next(0, stNames.ToArrayLength());
                    weap.Name = stNames[stIndex];
                    WeaponStaffAddition(weap, rarityValue, level, stIndex);
                     break;
                 case WeaponType.Sword:
                    var swNames = weaponNamesSword(rarity);
                    int swIndex = rng.Next(0, swNames.ToArrayLength());
                    weap.Name = swNames[swIndex];
                    WeaponSwordAddition(weap, rarityValue, level, swIndex);
                     break;
                 case WeaponType.TwinSwords:
                    var tsNames = weaponNamesTwinSwords(rarity);
                    int tsIndex = rng.Next(0, tsNames.ToArrayLength());
                    weap.Name = tsNames[tsIndex];
                    WeaponTwinSwordsAddition(weap, rarityValue, level, tsIndex);
                     break;
             }
            return weap;
        }

        public static Weapon CopyNewWeapon(Weapon weaponToCopy)
        {
            Weapon newWeap = new Weapon();
            PropertyCopy.Copy(weaponToCopy, newWeap);
            return newWeap;
        }

        public static Weapon GetStarterWeapon(Character chara)
        {
            Weapon weap = new Weapon();
            switch (chara.Class)
            {
                case CharacterClass.Dragoon:
                    weap = CopyNewWeapon(dragonSpear);
                    break;
                case CharacterClass.Mage:
                    weap = CopyNewWeapon(stick);
                    break;
                case CharacterClass.Necromancer:
                    weap = CopyNewWeapon(glowyOrb);
                    break;
                case CharacterClass.Rogue:
                    weap = CopyNewWeapon(rogueDaggers);
                    break;
                case CharacterClass.Warrior:
                    weap = CopyNewWeapon(warriorFists);
                    break;
            }
            return weap;
        }

        #endregion

        #region Static Weapons

        public static Weapon warriorFists = new Weapon { Name = "Warrior Fists", Type = WeaponType.Starter, CurrentDurability = -1, MaxDurability = -1, PhysicalDamage = 5, Desc = "The mighty fist, great for fisting.... I mean beating the shit out of everyone that gets in your way" };
        public static Weapon rogueDaggers = new Weapon { Name = "Rogues' Daggers", Speed = 110, PhysicalDamage = 3, Type = WeaponType.Starter, CurrentDurability = -1, MaxDurability = -1, Desc = "A pair of old daggers that are rusted; Like really bad, why are you using these again?" };
        public static Weapon dragonSpear = new Weapon { Name = "Novice Dragon Hunter Spear", Speed = 80, Type = WeaponType.Starter, CurrentDurability = -1, MaxDurability = -1, PhysicalDamage = 5, FireDamage = 1, LightningDamage = 1, IceDamage = 1, WindDamage = 1, Desc = "Nothing is more bad ass then a Dragon Hunter, thats why you are here, doesn't matter that there aren't any dragons around... Remember: Bad. Ass." };
        public static Weapon stick = new Weapon { Name = "A Stick", Speed = 500, Lvl = 0, Type = WeaponType.Other, Worth = 0, CurrentDurability = -1, MaxDurability = -1, PhysicalDamage = 1, Desc = "The mighty stick, it doesn't have good damage, level, or worth. But you can hit shit reeeally fast and that can be annoying as hell" };
        public static Weapon glowyOrb = new Weapon { Name = "Glowing Orb", Speed = 300, Lvl = 0, Type = WeaponType.Other, Worth = 0, CurrentDurability = -1, MaxDurability = -1, MagicDamage = 1, Desc = "You found this glowing orb in an abandoned chocolate factory, it glows a tremendous light when you hold it up and... That's it, it was probably a discontinued toy off the line" };
        
        public static List<Weapon> weaponList = new List<Weapon>()
        {
            rogueDaggers,
            dragonSpear,
            stick
        };

        #endregion

        #region Enemy Weapons

        public static Weapon enemySword = new Weapon() { Name = "Enemy Sword", Desc = "Sword forged from the depths of the developers dank minds, with a very unique name", Lvl = 1, MaxDurability = -1, CurrentDurability = -1, IsUnique = false, Speed = 100, PhysicalDamage = 5, FireDamage = 0, IceDamage = 0, LightningDamage = 0, MagicDamage = 0, WindDamage = 0, Rarity = RarityType.Common, Type = WeaponType.Sword, Worth = 0 };

        #endregion
    }

    public static class Spells
    {
        #region Spell Names and Descriptions

        public static string[] SpellAttackRanGenNames(ElementType type, RarityType rarity)
        {
            string[] names = null;
            switch (type)
            {
                case ElementType.Fire:
                    names = new string[] 
                    {
                        $"{rarity} Flare",
                        $"{rarity} Fire Bolt",
                        $"{rarity} Hot Bastard",
                        $"{rarity} Fire from teh kitchen stove",
                        $"{rarity} Gatling Flames",
                        $"{rarity} Wave of Flames"
                    };
                    break;
                case ElementType.Ice:
                    names = new string[]
                    {
                        $"{rarity} Ice Flash",
                        $"{rarity} Ice Bolt",
                        $"{rarity} Cold Bastard",
                        $"{rarity} Ice from teh kitchen freezer",
                        $"{rarity} Gatling Ice Shards",
                        $"{rarity} Slab of Ice"
                    };
                    break;
                case ElementType.Lightning:
                    names = new string[]
                    {
                        $"{rarity} Lightning Blast",
                        $"{rarity} Lightning Bolt",
                        $"{rarity} Static Shock",
                        $"{rarity} Charge picked up from socks sliding on the carpet",
                        $"{rarity} Rapid Lightning Bolts",
                        $"{rarity} Lightning Strike"
                    };
                    break;
                case ElementType.Magic:
                    names = new string[]
                    {
                        $"{rarity} Magic Bomb",
                        $"{rarity} Magic Bolt",
                        $"{rarity} Arcane Tingle",
                        $"{rarity} Magic from a magic show",
                        $"{rarity} Gatling magic bolt",
                        $"{rarity} Arcane Energy Wave"
                    };
                    break;
                case ElementType.Physical:
                    names = new string[]
                    {
                        $"{rarity} Bomb",
                        $"{rarity} Death Spike",
                        $"{rarity} Heavy Rock",
                        $"{rarity} Hammer from the shed",
                        $"{rarity} Gatling Gun",
                        $"{rarity} Concrete Wall"
                    };
                    break;
                case ElementType.Wind:
                    names = new string[]
                    {
                        $"{rarity} Jetstream Pressure Nuke",
                        $"{rarity} Wind Bolt",
                        $"{rarity} Windy Breeze",
                        $"{rarity} Light breeze coming through the apartment window",
                        $"{rarity} Gatling wind balls",
                        $"{rarity} Wall of Wind"
                    };
                    break;
            }
            return names;
        }

        public static void SpellAttackRanGenAdditions(ElementType type, Spell spell, int rarityValue, int index)
        {
            switch (type)
            {
                case ElementType.Fire:
                    switch (index)
                    {
                        case 0:
                            spell.Desc = "Like a mini nuke but in a firey death that melts away your organs";
                            spell.FireDamage += (rng.Next(1, 5) * rarityValue) * spell.Lvl;
                            spell.ManaCost = 10 + spell.Lvl;
                            spell.Speed = 80;
                            break;
                        case 1:
                            spell.Desc = "A bolt of fire, a bolt can be anything, it doesn't need to be lightning... right?";
                            spell.FireDamage += (rng.Next(1, 10) + (rarityValue / 2)) * spell.Lvl;
                            spell.Speed = 100;
                            spell.ManaCost = 3 + spell.Lvl;
                            break;
                        case 2:
                            spell.Desc = "Daaaaaaamn, this bastard is hot! tsssssssss";
                            spell.FireDamage += (rng.Next(1, 3) * (rarityValue / 2)) * spell.Lvl;
                            spell.Speed = 90;
                            spell.ManaCost = 6 + spell.Lvl;
                            break;
                        case 3:
                            spell.Desc = "You took this from the stove of the old lady down the street while she was cooking... heartless much?";
                            spell.FireDamage += (rng.Next(1, 8) + (rarityValue / 2) + (rarityValue / 3)) * spell.Lvl;
                            spell.Speed = 110;
                            spell.ManaCost = 5 + spell.Lvl;
                            break;
                        case 4:
                            spell.Desc = "Now you can shoot flames really fast, who needs accuracy anyway?";
                            spell.FireDamage += (rng.Next(1, 5) + (rarityValue / 2)) * spell.Lvl;
                            spell.Speed = 150;
                            spell.ManaCost = 2 + spell.Lvl;
                            break;
                        case 5:
                            spell.Desc = "Like a tsunami but made of flames, what's more awesome than that?";
                            spell.FireDamage += ((rng.Next(1, 3) * rarityValue) + (rarityValue / 3)) * spell.Lvl;
                            spell.Speed = 95;
                            spell.ManaCost = 10 + spell.Lvl;
                            break;
                    }
                    break;
                case ElementType.Ice:
                    switch (index)
                    {
                        case 0:
                            spell.Desc = "Big blast of below 0 tounge-stuck-to-the-pole goodness";
                            spell.IceDamage += (rng.Next(1, 5) * rarityValue) * spell.Lvl;
                            spell.ManaCost = 10 + spell.Lvl;
                            spell.Speed = 80;
                            break;
                        case 1:
                            spell.Desc = "A bolt of ice, a bolt can be anything, it doesn't need to be lightning... right?";
                            spell.IceDamage += (rng.Next(1, 10) + (rarityValue / 2)) * spell.Lvl;
                            spell.Speed = 100;
                            spell.ManaCost = 3 + spell.Lvl;
                            break;
                        case 2:
                            spell.Desc = "Daaaaaaamn, this bastard is cold! brrrrrrrrr";
                            spell.IceDamage += (rng.Next(1, 3) * (rarityValue / 2)) * spell.Lvl;
                            spell.Speed = 90;
                            spell.ManaCost = 6 + spell.Lvl;
                            break;
                        case 3:
                            spell.Desc = "Your hand almost got stuck getting this from the freezer.... worth it";
                            spell.IceDamage += (rng.Next(1, 8) + (rarityValue / 2) + (rarityValue / 3)) * spell.Lvl;
                            spell.Speed = 110;
                            spell.ManaCost = 5 + spell.Lvl;
                            break;
                        case 4:
                            spell.Desc = "Now you can shoot ice shards really fast, who needs accuracy anyway?";
                            spell.IceDamage += (rng.Next(1, 5) + (rarityValue / 2)) * spell.Lvl;
                            spell.Speed = 150;
                            spell.ManaCost = 2 + spell.Lvl;
                            break;
                        case 5:
                            spell.Desc = "A giant slab of ice summoned from wherever cold stuff comes from";
                            spell.IceDamage += ((rng.Next(1, 3) * rarityValue) + (rarityValue / 3)) * spell.Lvl;
                            spell.Speed = 95;
                            spell.ManaCost = 10 + spell.Lvl;
                            break;
                    }
                    break;
                case ElementType.Lightning:
                    switch (index)
                    {
                        case 0:
                            spell.Desc = "Large ball of concentrated electricity ready to explode, I hope you aren't standing in water";
                            spell.LightningDamage += (rng.Next(1, 5) * rarityValue) * spell.Lvl;
                            spell.ManaCost = 10 + spell.Lvl;
                            spell.Speed = 80;
                            break;
                        case 1:
                            spell.Desc = "A bolt of lightning, a bolt can't be anything, it needs to be lightning... LIGHTNING!!!!";
                            spell.LightningDamage += (rng.Next(1, 10) + (rarityValue / 2)) * spell.Lvl;
                            spell.Speed = 100;
                            spell.ManaCost = 3 + spell.Lvl;
                            break;
                        case 2:
                            spell.Desc = "You have built up a static charge, time to start poking people";
                            spell.LightningDamage += (rng.Next(1, 3) * (rarityValue / 2)) * spell.Lvl;
                            spell.Speed = 90;
                            spell.ManaCost = 6 + spell.Lvl;
                            break;
                        case 3:
                            spell.Desc = "You have been building this charge for awhile to get an unsuspecting target... It's time";
                            spell.LightningDamage += (rng.Next(1, 8) + (rarityValue / 2) + (rarityValue / 3)) * spell.Lvl;
                            spell.Speed = 110;
                            spell.ManaCost = 5 + spell.Lvl;
                            break;
                        case 4:
                            spell.Desc = "Rapidly fire bolts of lightning.... don't let anyone get on your bad side";
                            spell.LightningDamage += (rng.Next(1, 5) + (rarityValue / 2)) * spell.Lvl;
                            spell.Speed = 150;
                            spell.ManaCost = 2 + spell.Lvl;
                            break;
                        case 5:
                            spell.Desc = "A storm approaches and whispers in your ear \"I wanna strike someone\", what you do next is up to you";
                            spell.LightningDamage += ((rng.Next(1, 3) * rarityValue) + (rarityValue / 3)) * spell.Lvl;
                            spell.Speed = 95;
                            spell.ManaCost = 10 + spell.Lvl;
                            break;
                    }
                    break;
                case ElementType.Magic:
                    switch (index)
                    {
                        case 0:
                            spell.Desc = "A bomb made of arcane energy you summon out of nowhere, great for kids parties";
                            spell.MagicDamage += (rng.Next(1, 5) * rarityValue) * spell.Lvl;
                            spell.ManaCost = 10 + spell.Lvl;
                            spell.Speed = 80;
                            break;
                        case 1:
                            spell.Desc = "A bolt of arcane energy, a bolt can be anything, it doesn't need to be lightning... right?";
                            spell.MagicDamage += (rng.Next(1, 10) + (rarityValue / 2)) * spell.Lvl;
                            spell.Speed = 100;
                            spell.ManaCost = 3 + spell.Lvl;
                            break;
                        case 2:
                            spell.Desc = "Arcane energy that produces a tingle in the target, right before shit gets real";
                            spell.MagicDamage += (rng.Next(1, 3) * (rarityValue / 2)) * spell.Lvl;
                            spell.Speed = 90;
                            spell.ManaCost = 6 + spell.Lvl;
                            break;
                        case 3:
                            spell.Desc = "This magic was stolen from a stage magician in vegas, it's so unreal it's almost like.... magic?";
                            spell.MagicDamage += (rng.Next(1, 8) + (rarityValue / 2) + (rarityValue / 3)) * spell.Lvl;
                            spell.Speed = 110;
                            spell.ManaCost = 5 + spell.Lvl;
                            break;
                        case 4:
                            spell.Desc = "Rapidly fire bolts of arcane energy, like a boss";
                            spell.MagicDamage += (rng.Next(1, 5) + (rarityValue / 2)) * spell.Lvl;
                            spell.Speed = 150;
                            spell.ManaCost = 2 + spell.Lvl;
                            break;
                        case 5:
                            spell.Desc = "Magnificent wave of arcane energy ready to topple kingdoms";
                            spell.MagicDamage += ((rng.Next(1, 3) * rarityValue) + (rarityValue / 3)) * spell.Lvl;
                            spell.Speed = 95;
                            spell.ManaCost = 10 + spell.Lvl;
                            break;
                    }
                    break;
                case ElementType.Physical:
                    switch (index)
                    {
                        case 0:
                            spell.Desc = "A bomb is summoned out of your pocket.... magically";
                            spell.MagicDamage += (rng.Next(1, 5) * rarityValue) * spell.Lvl;
                            spell.ManaCost = 10 + spell.Lvl;
                            spell.Speed = 80;
                            break;
                        case 1:
                            spell.Desc = "A spike of death, I believe that's how it got it's name";
                            spell.MagicDamage += (rng.Next(1, 10) + (rarityValue / 2)) * spell.Lvl;
                            spell.Speed = 100;
                            spell.ManaCost = 3 + spell.Lvl;
                            break;
                        case 2:
                            spell.Desc = "Hurling a giant boulder from the ground, that's what you do on the weekends, and now";
                            spell.MagicDamage += (rng.Next(1, 3) * (rarityValue / 2)) * spell.Lvl;
                            spell.Speed = 90;
                            spell.ManaCost = 6 + spell.Lvl;
                            break;
                        case 3:
                            spell.Desc = "This \"Hammer\" was found in the shed out back and you are throwing it at your enemy... did I mention it weighs 350lbs?";
                            spell.MagicDamage += (rng.Next(1, 8) + (rarityValue / 2) + (rarityValue / 3)) * spell.Lvl;
                            spell.Speed = 110;
                            spell.ManaCost = 5 + spell.Lvl;
                            break;
                        case 4:
                            spell.Desc = "Someone showed you how to smuggle items through security and what did you do with that? You smuggled a gatling gun. (Up your butt)";
                            spell.MagicDamage += (rng.Next(1, 5) + (rarityValue / 2)) * spell.Lvl;
                            spell.Speed = 150;
                            spell.ManaCost = 2 + spell.Lvl;
                            break;
                        case 5:
                            spell.Desc = "A concrete wall rises from the ground, it's like heavy and stuff, it hurts and stuff, and.... stuff";
                            spell.MagicDamage += ((rng.Next(1, 3) * rarityValue) + (rarityValue / 3)) * spell.Lvl;
                            spell.Speed = 95;
                            spell.ManaCost = 10 + spell.Lvl;
                            break;
                    }
                    break;
                case ElementType.Wind:
                    switch (index)
                    {
                        case 0:
                            spell.Desc = "A spherical nuke containing pure pressurized jetstream, for only 2 easy payments of 19.99!";
                            spell.MagicDamage += (rng.Next(1, 5) * rarityValue) * spell.Lvl;
                            spell.ManaCost = 10 + spell.Lvl;
                            spell.Speed = 80;
                            break;
                        case 1:
                            spell.Desc = "Bolt of wind, some might ask how this is possible. But I don't know so stop asking!";
                            spell.MagicDamage += (rng.Next(1, 10) + (rarityValue / 2)) * spell.Lvl;
                            spell.Speed = 100;
                            spell.ManaCost = 3 + spell.Lvl;
                            break;
                        case 2:
                            spell.Desc = "A windy breeze flows past your enemy, here windy breeze means lvl 4 hurricane but same difference eh?";
                            spell.MagicDamage += (rng.Next(1, 3) * (rarityValue / 2)) * spell.Lvl;
                            spell.Speed = 90;
                            spell.ManaCost = 6 + spell.Lvl;
                            break;
                        case 3:
                            spell.Desc = "Bottle of wind captured from an apartment window. That's about it, nothing to see here.";
                            spell.MagicDamage += (rng.Next(1, 8) + (rarityValue / 2) + (rarityValue / 3)) * spell.Lvl;
                            spell.Speed = 110;
                            spell.ManaCost = 5 + spell.Lvl;
                            break;
                        case 4:
                            spell.Desc = "BALLS! Windy, Gatling, Windy Balls!!!";
                            spell.MagicDamage += (rng.Next(1, 5) + (rarityValue / 2)) * spell.Lvl;
                            spell.Speed = 150;
                            spell.ManaCost = 2 + spell.Lvl;
                            break;
                        case 5:
                            spell.Desc = "Massive \"Wall\" of wind uncontrollably eager to rip your enemies apart";
                            spell.MagicDamage += ((rng.Next(1, 3) * rarityValue) + (rarityValue / 3)) * spell.Lvl;
                            spell.Speed = 95;
                            spell.ManaCost = 10 + spell.Lvl;
                            break;
                    }
                    break;
            }
        }

        public static string[] SpellDefenseRanGenNames(ElementType type, RarityType rarity)
        {
            string[] names = null;
            switch (type)
            {
                case ElementType.Fire:
                    names = new string[]
                    {
                        $"{rarity} Heat Shield",
                        $"{rarity} Fire Vacuum",
                        $"{rarity} Fire Extinquisher",
                    };
                    break;
                case ElementType.Ice:
                    names = new string[]
                    {
                        $"{rarity} Ice Shield",
                        $"{rarity} Blow Torch",
                        $"{rarity} Lamp",
                    };
                    break;
                case ElementType.Lightning:
                    names = new string[]
                    {
                        $"{rarity} Static Buildup Dispersal",
                        $"{rarity} Lightning Rod",
                        $"{rarity} Metal Thing",
                    };
                    break;
                case ElementType.Magic:
                    names = new string[]
                    {
                        $"{rarity} Arcane Leech",
                        $"{rarity} Disbelief",
                        $"{rarity} Magic Shield",
                    };
                    break;
                case ElementType.Physical:
                    names = new string[]
                    {
                        $"{rarity} Beating Shield",
                        $"{rarity} Metal Armor",
                        $"{rarity} Rock Barrier",
                    };
                    break;
                case ElementType.Wind:
                    names = new string[]
                    {
                        $"{rarity} Wind Shield",
                        $"{rarity} Wind Jacket",
                        $"{rarity} Wind Wall",
                    };
                    break;
            }
            return names;
        }

        public static void SpellDefenseRanGenAdditions(ElementType type, Spell spell, int rarityValue, int index)
        {
            switch (type)
            {
                case ElementType.Fire:
                    switch (index)
                    {
                        case 0:
                            spell.Desc = "Magical shield that blocks any heat source, just like the ad said it would";
                            spell.FireDamage += rng.Next(0, 30) + spell.Lvl;
                            spell.ManaCost = 10 + spell.Lvl;
                            spell.Speed = 80;
                            break;
                        case 1:
                            spell.Desc = "Sphere containing an oxygen vacuum to snuff out fire, yea I said snuff... what about it?";
                            var valueGen = rng.Next(1, 5) * spell.Lvl;
                            spell.FireDamage += valueGen > 100 ? 100 : valueGen;
                            spell.Speed = 100;
                            spell.ManaCost = 8 + spell.Lvl;
                            break;
                        case 2:
                            spell.Desc = "Magical fire extinguisher with an energy emanating from within your body, you have a chance of getting crabs... worth it?";
                            spell.FireDamage += spell.Lvl <= 25 ? rng.Next(0, 50) + spell.Lvl : rng.Next(40, 100) + spell.Lvl;
                            spell.Speed = spell.Lvl <= 25 ? 90 : 125;
                            spell.ManaCost = spell.Lvl <= 25 ? 6 + spell.Lvl : 12 + spell.Lvl;
                            break;
                    }
                    break;
                case ElementType.Ice:
                    switch (index)
                    {
                        case 0:
                            spell.Desc = "Magical shield that blocks any cold source, just like the ad said it would";
                            spell.IceDamage += rng.Next(0, 30) + spell.Lvl;
                            spell.ManaCost = 10 + spell.Lvl;
                            spell.Speed = 80;
                            break;
                        case 1:
                            spell.Desc = "\"Magical\" auto torch to melt away ice, also great to make creme brulee with!";
                            var valueGen = rng.Next(1, 5) * spell.Lvl;
                            spell.IceDamage += valueGen > 100 ? 100 : valueGen;
                            spell.Speed = 100;
                            spell.ManaCost = 8 + spell.Lvl;
                            break;
                        case 2:
                            spell.Desc = "I love.......";
                            spell.IceDamage += spell.Lvl <= 25 ? rng.Next(0, 50) + spell.Lvl : rng.Next(40, 100) + spell.Lvl;
                            spell.Speed = spell.Lvl <= 25 ? 90 : 125;
                            spell.ManaCost = spell.Lvl <= 25 ? 6 + spell.Lvl : 12 + spell.Lvl;
                            break;
                    }
                    break;
                case ElementType.Lightning:
                    switch (index)
                    {
                        case 0:
                            spell.Desc = "Like a mini EMP nuke that disperses static electricity, also causes cancer but don't worry about that";
                            spell.LightningDamage += rng.Next(0, 30) + spell.Lvl;
                            spell.ManaCost = 10 + spell.Lvl;
                            spell.Speed = 80;
                            break;
                        case 1:
                            spell.Desc = "Lightning rod that absorbs lightning strikes, it doesn't look funny on your back at all, I promise!";
                            spell.LightningDamage += spell.Lvl <= 25 ? rng.Next(0, 50) + spell.Lvl : rng.Next(40, 100) + spell.Lvl;
                            spell.Speed = spell.Lvl <= 25 ? 90 : 125;
                            spell.ManaCost = spell.Lvl <= 25 ? 6 + spell.Lvl : 12 + spell.Lvl;
                            break;
                        case 2:
                            spell.Desc = @"Metal thing you found on the ground, ¯\_(ツ)_/¯";
                            var valueGen = rng.Next(1, 5) * spell.Lvl;
                            spell.LightningDamage += valueGen > 100 ? 100 : valueGen;
                            spell.Speed = 100;
                            spell.ManaCost = 8 + spell.Lvl;
                            break;
                    }
                    break;
                case ElementType.Magic:
                    switch (index)
                    {
                        case 0:
                            spell.Desc = "Magical leech that leeches and absorbs magic for itself magically.... magic. magic. magic. magic.";
                            spell.MagicDamage += spell.Lvl <= 25 ? rng.Next(0, 50) + spell.Lvl : rng.Next(40, 100) + spell.Lvl;
                            spell.Speed = spell.Lvl <= 25 ? 90 : 125;
                            spell.ManaCost = spell.Lvl <= 25 ? 6 + spell.Lvl : 12 + spell.Lvl;
                            break;
                        case 1:
                            spell.Desc = "Power to use real disbelief of magic to dispel magical energy, #conspiracy";
                            var valueGen = rng.Next(1, 5) * spell.Lvl;
                            spell.MagicDamage += valueGen > 100 ? 100 : valueGen;
                            spell.Speed = 100;
                            spell.ManaCost = 8 + spell.Lvl;
                            break;
                        case 2:
                            spell.Desc = "Magical shield that blocks any magic source, just like the ad said it would";
                            spell.MagicDamage += rng.Next(0, 30) + spell.Lvl;
                            spell.ManaCost = 10 + spell.Lvl;
                            spell.Speed = 80;
                            break;
                    }
                    break;
                case ElementType.Physical:
                    switch (index)
                    {
                        case 0:
                            spell.Desc = "Physical shield that blocks any physical beating, just like the ad said it would";
                            spell.PhysicalDamage += rng.Next(0, 30) + spell.Lvl;
                            spell.ManaCost = 10 + spell.Lvl;
                            spell.Speed = 80;
                            break;
                        case 1:
                            spell.Desc = @"Armor summoned from earth metals and morphed around your body, snug, cupping your genitals like only a lover would (° ͜ʖ°)";
                            spell.PhysicalDamage += spell.Lvl <= 25 ? rng.Next(0, 50) + spell.Lvl : rng.Next(40, 100) + spell.Lvl;
                            spell.Speed = spell.Lvl <= 25 ? 90 : 125;
                            spell.ManaCost = spell.Lvl <= 25 ? 6 + spell.Lvl : 12 + spell.Lvl;
                            break;
                        case 2:
                            spell.Desc = "Summon a wall made of rock from the {insert planet name here}";
                            var valueGen = rng.Next(1, 5) * spell.Lvl;
                            spell.PhysicalDamage += valueGen > 100 ? 100 : valueGen;
                            spell.Speed = 100;
                            spell.ManaCost = 8 + spell.Lvl;
                            break;
                    }
                    break;
                case ElementType.Wind:
                    switch (index)
                    {
                        case 0:
                            spell.Desc = "Magical shield that blocks any wind source, just like the ad said it would";
                            spell.WindDamage += rng.Next(0, 30) + spell.Lvl;
                            spell.ManaCost = 10 + spell.Lvl;
                            spell.Speed = 80;
                            break;
                        case 1:
                            spell.Desc = "Windbreaker jacket that breaks wind, while you break wind";
                            spell.WindDamage += spell.Lvl <= 25 ? rng.Next(0, 50) + spell.Lvl : rng.Next(40, 100) + spell.Lvl;
                            spell.Speed = spell.Lvl <= 25 ? 90 : 125;
                            spell.ManaCost = spell.Lvl <= 25 ? 6 + spell.Lvl : 12 + spell.Lvl;
                            break;
                        case 2:
                            spell.Desc = "Wall of magical energy that dissipates wind, even the gas you passed a few minutes ago";
                            var valueGen = rng.Next(1, 5) * spell.Lvl;
                            spell.WindDamage += valueGen > 100 ? 100 : valueGen;
                            spell.Speed = 100;
                            spell.ManaCost = 8 + spell.Lvl;
                            break;
                    }
                    break;
            }
        }

        public static string[] SpellRestorativeRanGenNames(ElementType type, RarityType rarity)
        {
            string[] names =
            {
                $"{rarity} Healing Wind",
                $"{rarity} Foot Rub",
                $"{rarity} Happy Ending",
                $"{rarity} Belief that we aren't just meat sacks brought to life by accident"
            };
            
            return names;
        }

        public static void SpellRestorativeRanGenAdditions(Character charac, Spell spell, int rarityValue, int index)
        {
            switch (index)
            {
                case 0:
                    spell.Desc = "Suddenly a wind rolls in that just happens to heal you, go figure, it's those healing winds we hear so much about";
                    spell.PhysicalDamage += (charac.MaxHP / 2) + (rarityValue * spell.Lvl);
                    spell.ManaCost += 7 + spell.Lvl;
                    spell.Speed = 90;
                    break;
                case 1:
                    spell.Desc = "Ooooooowwwwwwwhhhhhh yeeeeeaaaaaa that feels gooooooood. Give me a few minutes, wait your turn";
                    spell.PhysicalDamage += (charac.MaxHP / 3) + (rarityValue * spell.Lvl);
                    spell.ManaCost += 3 + spell.Lvl;
                    spell.Speed = 100;
                    break;
                case 2:
                    spell.Desc = "You know what this is and damn it feels good! Only 5 currency at your local massage parlor (° ͜ʖ°)";
                    spell.PhysicalDamage += (rarityValue * spell.Lvl) * rarityValue;
                    spell.ManaCost += 10 + spell.Lvl;
                    break;
                case 3:
                    spell.Desc = "Let it be known the spaghetti monster rose from the ashes and blessed it's words upon us: Tomato's make spaghetti sauce, the life blood";
                    spell.PhysicalDamage += (charac.MaxHP / 8) + (rarityValue + spell.Lvl + (charac.MaxHP / 6)) + rarityValue;
                    spell.ManaCost += 5 + spell.Lvl;
                    break;
            }
        }

        #endregion

        #region Spell Methods

        public static Spell SpellRandomGen(Character charac, Spell spell, RarityType rarity, ElementType type, SpellType spellType, int rarityValue, int level)
        {
            int typeCount = LootDrop.ChooseElementCount(rarity);
            spell.Name = $"{rarity} {type} Spell".ToUpperAllFirst();
            spell.Desc = $"{type} spell with {rarity} power levels emanating from within".ToUpperFirst();
            spell.Type = spellType;
            spell.Lvl = LootDrop.ChooseLevel(level);
            spell.Speed = ChanceRoll(35) ? rng.Next(60, 200) + rarityValue : rng.Next(60, 200);
            spell.ManaCost = rng.Next((2 + spell.Lvl), rarityValue + spell.Lvl);
            if (spell.Type != SpellType.Restorative)
                switch (type)
                {
                    case ElementType.Fire:
                        spell.FireDamage += (rng.Next(3, 16) + rarityValue) * spell.Lvl;
                        if (spell.Speed < 100) spell.FireDamage += (rarityValue * spell.Lvl) / 2;
                        else if (spell.Speed > 150) spell.FireDamage -= (rarityValue * spell.Lvl) / 2;
                        spell.ManaCost = rng.Next((2 + spell.Lvl), (rarityValue + spell.Lvl));
                        break;
                    case ElementType.Ice:
                        spell.IceDamage += (rng.Next(3, 16) + rarityValue) * spell.Lvl;
                        if (spell.Speed < 100) spell.IceDamage += (rarityValue * spell.Lvl) / 2;
                        else if (spell.Speed > 150) spell.IceDamage -= (rarityValue * spell.Lvl) / 2;
                        spell.ManaCost = rng.Next((2 + spell.Lvl), (rarityValue + spell.Lvl));
                        break;
                    case ElementType.Lightning:
                        spell.LightningDamage += (rng.Next(3, 16) + rarityValue) * spell.Lvl;
                        if (spell.Speed < 100) spell.LightningDamage += (rarityValue * spell.Lvl) / 2;
                        else if (spell.Speed > 150) spell.LightningDamage -= (rarityValue * spell.Lvl) / 2;
                        spell.ManaCost = rng.Next((2 + spell.Lvl), (rarityValue + spell.Lvl));
                        break;
                    case ElementType.Magic:
                        spell.MagicDamage += (rng.Next(3, 16) + rarityValue) * spell.Lvl;
                        if (spell.Speed < 100) spell.MagicDamage += (rarityValue * spell.Lvl) / 2;
                        else if (spell.Speed > 150) spell.MagicDamage -= (rarityValue * spell.Lvl) / 2;
                        spell.ManaCost = rng.Next((2 + spell.Lvl), (rarityValue + spell.Lvl));
                        break;
                    case ElementType.Physical:
                        spell.PhysicalDamage += (rng.Next(3, 16) + rarityValue) * spell.Lvl;
                        if (spell.Speed < 100) spell.PhysicalDamage += (rarityValue * spell.Lvl) / 2;
                        else if (spell.Speed > 150) spell.PhysicalDamage -= (rarityValue * spell.Lvl) / 2;
                        spell.ManaCost = rng.Next((2 + spell.Lvl), (rarityValue + spell.Lvl));
                        break;
                    case ElementType.Wind:
                        spell.WindDamage += (rng.Next(3, 16) + rarityValue) * spell.Lvl;
                        if (spell.Speed < 100) spell.WindDamage += (rarityValue * spell.Lvl) / 2;
                        else if (spell.Speed > 150) spell.WindDamage -= (rarityValue * spell.Lvl) / 2;
                        spell.ManaCost = rng.Next((2 + spell.Lvl), (rarityValue + spell.Lvl));
                        break;
                }
            else
            {
                spell.PhysicalDamage += (rarityValue * spell.Lvl) >= charac.MaxHP ? charac.MaxHP : rng.Next((rarityValue * spell.Lvl), charac.MaxHP);
                spell.ManaCost = rng.Next((2 + spell.Lvl), (rarityValue + spell.Lvl));
                spell.Speed = rng.Next(60, 150);
            }
            SpellAddElement(spell, typeCount, rarityValue);
            LootDrop.ChooseSpellWorth(spell, rarityValue);
            return spell;
        }

        public static Spell SpellUniqueGen(Character charac, Spell spell, RarityType rarity, ElementType type, SpellType spellType, int rarityValue, int level)
        {
            spell.IsUnique = true;
            switch (spellType)
            {
                case SpellType.Attack:
                    var attackNames = SpellAttackRanGenNames(type, rarity);
                    int atkIndex = rng.Next(0, attackNames.ToArrayLength());
                    spell.Name = attackNames[atkIndex];
                    SpellAttackRanGenAdditions(type, spell, rarityValue, atkIndex);
                    break;
                case SpellType.Defense:
                    var defNames = SpellDefenseRanGenNames(type, rarity);
                    int defIndex = rng.Next(0, defNames.ToArrayLength());
                    spell.Name = defNames[defIndex];
                    SpellDefenseRanGenAdditions(type, spell, rarityValue, defIndex);
                    break;
                case SpellType.Restorative:
                    var restNames = SpellRestorativeRanGenNames(type, rarity);
                    int restIndex = rng.Next(0, restNames.ToArrayLength());
                    spell.Name = restNames[restIndex];
                    SpellRestorativeRanGenAdditions(charac, spell, rarityValue, restIndex);
                    break;
            }
            LootDrop.ChooseSpellWorth(spell, rarityValue, true);
            return spell;
        }

        public static void SpellAddElement(Spell spell, int typeCount, int rarityValue)
        {
            if (typeCount > 1)
                for (int i = typeCount; i == 1; i--)
                {
                    ElementType type = LootDrop.ChooseElement();
                    switch (type)
                    {
                        case ElementType.Fire:
                            spell.FireDamage += rng.Next(0, rarityValue) * spell.Lvl;
                            break;
                        case ElementType.Ice:
                            spell.IceDamage += rng.Next(0, rarityValue) * spell.Lvl;
                            break;
                        case ElementType.Lightning:
                            spell.LightningDamage += rng.Next(0, rarityValue) * spell.Lvl;
                            break;
                        case ElementType.Magic:
                            spell.MagicDamage += rng.Next(0, rarityValue) * spell.Lvl;
                            break;
                        case ElementType.Physical:
                            spell.PhysicalDamage += rng.Next(0, rarityValue) * spell.Lvl;
                            break;
                        case ElementType.Wind:
                            spell.WindDamage += rng.Next(0, rarityValue) * spell.Lvl;
                            break;
                    }
                }
        }

        public static Spell CopyNewSpell(Spell spellToCopy)
        {
            Spell newSpell = new Spell();
            PropertyCopy.Copy(spellToCopy, newSpell);
            return newSpell;
        }

        #endregion

        #region Static Spells

        public static Spell magesEnergy = new Spell { Name = "Mages' Energy", MagicDamage = 5, ManaCost = 0, Lvl = 0, Type = SpellType.Starter, Desc = "The spell that started them all, some might call it the 'Hello World' spell, it gets the job done and your grueling training means you can infinitely use it.... cool!" };
        public static Spell boneSpike = new Spell { Name = "Necromancer Bone Spike", ManaCost = 0, PhysicalDamage = 3, WindDamage = 1, MagicDamage = 2, Type = SpellType.Starter, Desc = "A giant spike comes out of the ground with a 70% chance of hitting the genitals, what's more to like?" };
        public static Spell arcaneArmor = new Spell { Name = "Novice Arcane Armor", Type = SpellType.Defense, PhysicalDamage = 3, ManaCost = 2, MagicDamage = 5, Lvl = 1, Desc = "Thin hovering layers of pure arcane defense here to protect you, I think the warranty expired last week. Be Careful" };
        public static Spell dragonRage = new Spell { Name = "Novice Dragon Rage", Type = SpellType.Defense, Lvl = 1, Speed = 50, ManaCost = 5, PhysicalDamage = 3, FireDamage = 3, IceDamage = 3, LightningDamage = 3, MagicDamage = 3, WindDamage = 3, Desc = "You have learned to harness a dragon's rage and use it to fuel your body to whoop some major ass" };

        #endregion
    }

    public static class Armors
    {
        #region Armor Names and Descriptions

        public static string[] armorBasicLightNames =
        {
            "Robe",
            "Fur Armor",
            "Glass Armor"
        };
        public static string[] ArmorUniqueLightNames(RarityType rarity)
        {
            return new string[]
            {
                $"{rarity} Grand Wizard Robes",
                $"{rarity} Paper Garments"
            };
        }
        public static void ArmorUniqueLightAddition(Armor armor, int rarityValue, int level, int index)
        {
            switch (index)
            {
                case 0:
                    armor.Desc = "Grand white robes fit for a wizard. Just don't wear a white hood as well..";
                    armor.Physical = (level + rng.Next(2, 14) + rarityValue);
                    armor.Speed = (level + 100 + rng.Next(20, 80));
                    armor.Worth = (((level + rarityValue) * rarityValue) + (armor.Speed / armor.Physical));
                    break;
                case 1:
                    armor.Desc = "It's amazing how powerful you become when you aren't weighed down by 'protection'.";
                    armor.Physical = (level + rng.Next(5, 12) + rarityValue);
                    armor.Speed = (level + 120 + rng.Next(30, 60));
                    armor.Worth = (((level + rarityValue) * rarityValue) + (armor.Speed / armor.Physical));
                    break;
            }
        }

        public static string[] armorBasicMediumNames = 
        {
            "Leather Armor",
            "Scale Armor"
        };
        public static string[] ArmorUniqueMediumNames(RarityType rarity)
        {
            return new string[]
            {
                $"{rarity} Hide Armor",
                $"{rarity} Furry Armor"
            };
        }
        public static void ArmorUniqueMediumAddition(Armor armor, int rarityValue, int level, int index)
        {
            switch (index)
            {
                case 0:
                    armor.Desc = "Why do Rogues wear hide armor? Because it's made of leather. Wait..";
                    armor.Physical = (level + rng.Next(2, 14) + rarityValue);
                    armor.Speed = (level + 100 + rng.Next(20, 80));
                    armor.Worth = (((level + rarityValue) * rarityValue) + (armor.Speed / armor.Physical));
                    break;
                case 1:
                    armor.Desc = "Diaper not included.";
                    armor.Physical = (level + rng.Next(5, 12) + rarityValue);
                    armor.Speed = (level + 120 + rng.Next(30, 60));
                    armor.Worth = (((level + rarityValue) * rarityValue) + (armor.Speed / armor.Physical));
                    break;
            }
        }

        public static string[] armorBasicHeavyNames = 
        {
            "Steel Armor",
            "Plate Armor",
            "Diamond Armor",
            "Mythril Armor",
            "Obsidian Armor"
        };
        public static string[] ArmorUniqueHeavyNames(RarityType rarity)
        {
            return new string[]
            {
                $"{rarity} Diamond Armor",
                $"{rarity} Titanforged Steel"
            };
        }
        public static void ArmorUniqueHeavyAddition(Armor armor, int rarityValue, int level, int index)
        {
            switch (index)
            {
                case 0:
                    armor.Desc = "Protects your body, not your pride.";
                    armor.Physical = (level + rng.Next(2, 14) + rarityValue);
                    armor.Speed = (level + 100 + rng.Next(20, 80));
                    armor.Worth = (((level + rarityValue) * rarityValue) + (armor.Speed / armor.Physical));
                    break;
                case 1:
                    armor.Desc = "\"What is this? Armor for ants?\" - Titans, probably";
                    armor.Physical = (level + rng.Next(5, 12) + rarityValue);
                    armor.Speed = (level + 120 + rng.Next(30, 60));
                    armor.Worth = (((level + rarityValue) * rarityValue) + (armor.Speed / armor.Physical));
                    break;
            }
        }

        #endregion

        #region Static Armors

        public static Armor knightArmor = new Armor { Name = "Novice Knight Armor", Type = ArmorType.Heavy, Lvl = 1, Speed = 50, Worth = 100, MaxDurability = 20, CurrentDurability = 20, Physical = 100, Desc = "Some beatup old armor you found in the old shed out back, next to the bones of an old dog... what was it's name again?" };
        public static Armor mageRobe = new Armor { Name = "Novice Mages' Robe", Type = ArmorType.Light, CurrentDurability = 10, MaxDurability = 10, Speed = 150, Worth = 100, Magic = 100, Lvl = 1, Physical = 10, Desc = "These might be 'Robes' if you believe hard enough, go on, believe... I can wait" };
        public static Armor theiveGarb = new Armor { Name = "Novice Theives Garb", Type = ArmorType.Light, CurrentDurability = 15, Lvl = 1, MaxDurability = 15, Speed = 130, Worth = 100, Physical = 70, Magic = 30, Desc = "What better way to rock your first gear then to steal it, even if it was from old miss bitchface who is a blind amputee" };
        public static Armor undeadArmor = new Armor { Name = "Undead Armor", Type = ArmorType.Medium, CurrentDurability = 18, MaxDurability = 18, Lvl = 1, Speed = 80, Worth = 100, Physical = 85, Magic = 30, Desc = "Nothing weird here, you just picked up the bones from some dead people and strapped it to your body... they weren't using it anyway" };
        public static Armor dragonArmor = new Armor { Name = "Novice Dragon Hunter Armor", Type = ArmorType.Medium, Lvl = 1, MaxDurability = 20, CurrentDurability = 20, Speed = 100, Worth = 100, Physical = 6, Fire = 10, Ice = 10, Lightning = 10, Wind = 10, Desc = "Bad. Ass. Bad. Ass. Bad. Ass. Bad. Ass. - Naive thoughts running in your mind" };
        public static Armor royalRobeArmor = new Armor { Name = "Royal Robes", Type = ArmorType.Light, Lvl = 1, MaxDurability = 10, CurrentDurability = 10, Speed = 150, Worth = 100, Magic = 100, Physical = 10, Desc = "Yer a hairy Wizard!" };
        public static Armor glassArmor = new Armor { Name = "Glass Armor", Type = ArmorType.Light, Lvl = 1, MaxDurability = 10, CurrentDurability = 10, Speed = 150, Worth = 100, Magic = 100, Physical = 10, Desc = "The Emperor's new armor" };
        public static Armor leatheryArmor = new Armor { Name = "Skin Tight Leather Armor", Type = ArmorType.Medium, Lvl = 1, MaxDurability = 18, CurrentDurability = 18, Speed = 80, Worth = 100, Physical = 85, Magic = 20, Desc = "Why do Rogues wear leather? It's made of hide" };
        public static Armor scaleArmor = new Armor { Name = "Golden Scale Armor", Type = ArmorType.Medium, Lvl = 1, MaxDurability = 18, CurrentDurability = 18, Speed = 80, Worth = 100, Physical = 85, Magic = 20, Desc = "Scaley scales to scale...scaley" };
        public static Armor blackPlateArmor = new Armor { Name = "Blackened Plate Armor", Type = ArmorType.Heavy, Lvl = 1, MaxDurability = 30, CurrentDurability = 30, Speed = 60, Worth = 100, Physical = 100, Desc = "Armor darker than your Emo phase" };
        public static Armor imperialArmor = new Armor { Name = "Imperial Armor", Type = ArmorType.Heavy, Lvl = 1, MaxDurability = 30, CurrentDurability = 30, Speed = 60, Worth = 100, Physical = 100, Desc = "Armor that tends to be weaker around the knees. Mind the arrows" };

        #endregion

        #region Enemy Armors

        public static Armor basicEnemyArmor = new Armor { Name = "Enemy Armor", Desc = "Armor forged from the depths of the developers minds with a unique name", Fire = 50, Ice = 50, Lightning = 50, Magic = 50, Wind = 50, Physical = 5, Speed = 100, MaxDurability = -1, CurrentDurability = -1, IsUnique = false, Lvl = 1, Rarity = RarityType.Common, Type = ArmorType.Light, Worth = 0 };

        #endregion

        #region Armor Lists
        public static List<Armor> lightArmorList = new List<Armor>()
        {
            royalRobeArmor,
            glassArmor
        };

        public static List<Armor> mediumArmorList = new List<Armor>()
        {
            glassArmor,
            leatheryArmor
        };

        public static List<Armor> heavyArmorList = new List<Armor>()
        {
            blackPlateArmor,
            imperialArmor
        };
        #endregion

        #region Armor Methods

        public static Armor ArmorUniqueGen(Armor armor, RarityType rarity, int rarityValue, int charLevel)
        {
            armor.IsUnique = true;
            rarityValue += 2;

            switch (armor.Type)
            {
                case ArmorType.Light:
                    var armorLightName = ArmorUniqueLightNames(rarity);
                    int lNameIndex = armorLightName.ToArrayLength();
                    armor.Name = armorLightName[lNameIndex];
                    ArmorUniqueLightAddition(armor, rarityValue, charLevel, lNameIndex);
                    break;

                case ArmorType.Medium:
                    var armorMediumName = ArmorUniqueMediumNames(rarity);
                    int mNameIndex = armorMediumName.ToArrayLength();
                    armor.Name = armorMediumName[mNameIndex];
                    ArmorUniqueMediumAddition(armor, rarityValue, charLevel, mNameIndex);
                    break;

                case ArmorType.Heavy:
                    var armorHeavyName = ArmorUniqueHeavyNames(rarity);
                    int hNameIndex = armorHeavyName.ToArrayLength();
                    armor.Name = armorHeavyName[hNameIndex];
                    ArmorUniqueHeavyAddition(armor, rarityValue, charLevel, hNameIndex);
                    break;
            }

            return armor;
        }

        public static Armor ArmorRandomGen(RarityType rarity, int rarityValue, Armor armor, int charLevel)
        {
            int typeCount = LootDrop.ChooseElementCount(rarity);
            armor.Lvl = LootDrop.ChooseLevel(charLevel);
            armor.MaxDurability = (10 * armor.Lvl) + (rarityValue * 4);
            armor.CurrentDurability = armor.MaxDurability;
            armor.Worth = (rng.Next(1, 100) * armor.Lvl) * (typeCount);
            switch (armor.Type)
            {
                case ArmorType.Light:
                    int lightNum = rng.Next(0, armorBasicLightNames.ToArrayLength());
                    armor.Name = $"{rarity} {armorBasicLightNames[lightNum]}";
                    armor.Desc = $"{armor.Type} {armor.Name}";
                    armor.Speed = ((rng.Next(0, 2) + typeCount + armor.Lvl) * 10) + 80;
                    armor.Physical = ((rng.Next(0, 3) + typeCount + armor.Lvl));
                    break;
                case ArmorType.Medium:
                    int mediumNum = rng.Next(0, armorBasicMediumNames.ToArrayLength());
                    armor.Name = $"{rarity} {armorBasicMediumNames[mediumNum]}";
                    armor.Desc = $"{armor.Type} {armor.Name}";
                    armor.Speed = ((rng.Next(0, 2) + typeCount + armor.Lvl) * 10) + 60;
                    armor.Physical = ((rng.Next(9, 12) + typeCount + armor.Lvl));
                    break;
                case ArmorType.Heavy:
                    int heavyNum = rng.Next(0, armorBasicHeavyNames.ToArrayLength());
                    armor.Name = $"{rarity} {armorBasicHeavyNames[heavyNum]}";
                    armor.Desc = $"{armor.Type} {armor.Name}";
                    armor.Speed = ((rng.Next(0, 2) + typeCount + armor.Lvl) * 10) + 40;
                    armor.Physical = ((rng.Next(18, 21) + typeCount + armor.Lvl));
                    break;
            }
            ArmorAddElement(armor, typeCount, rarityValue);

            return armor;
        }

        public static void ArmorAddElement(Armor armor, int typeCount, int rarityValue)
        {

            if (typeCount > 1)
                for (int i = typeCount; i == 1; i--)
                {
                    ElementType type = LootDrop.ChooseElement();
                    switch (type)
                    {
                        case ElementType.Fire:
                            armor.Fire += rng.Next(0, rarityValue) * armor.Lvl;
                            break;
                        case ElementType.Ice:
                            armor.Ice += rng.Next(0, rarityValue) * armor.Lvl;
                            break;
                        case ElementType.Lightning:
                            armor.Lightning += rng.Next(0, rarityValue) * armor.Lvl;
                            break;
                        case ElementType.Magic:
                            armor.Magic += rng.Next(0, rarityValue) * armor.Lvl;
                            break;
                        case ElementType.Physical:
                            armor.Physical += rng.Next(0, rarityValue) * armor.Lvl;
                            break;
                        case ElementType.Wind:
                            armor.Wind += rng.Next(0, rarityValue) * armor.Lvl;
                            break;
                    }
                }

        }

        public static Armor CopyNewArmor(Armor armorToCopy)
        {
            Armor newArmor = new Armor();
            PropertyCopy.Copy(armorToCopy, newArmor);
            return newArmor;
        }

        #endregion
    }

    public static class Items
    {
        #region Static Items

        public static Item smallHealthPotion = new Item { Name = "Small Health Potion", Type = ItemType.Restorative, Lvl = 1, Worth = 2, Desc = "The good ol' health potion, now with 5 shots of caffeine and no MSG!" };
        public static Item smallHealthPotionPack = new Item { Name = "Small Health Potion Pack", Type = ItemType.Restorative, Lvl = 1, Worth = 2, Count = 5, Desc = "5 health potions!? Is it christmas already? Get those f**kin socks away from me!" };
        public static Item smallManaPotion = new Item { Name = "Small Mana Potion", Type = ItemType.Restorative, Lvl = 1, Worth = 5, Desc = "A brew that fills your body with Magic energy, the health information sticker wore off long ago, don't worry about what is inside." };
        public static Item smallManaPotionPack = new Item { Name = "Small Mana Potion Pack", Type = ItemType.Restorative, Lvl = 1, Worth = 5, Count = 5, Desc = "5 mana potions!? Hot damn this pack radiates awesomeness... or is that radiation?" };
        public static Item mediumHealthPotion = new Item { Name = "Medium Health Potion", Type = ItemType.Restorative, Lvl = 2, Worth = 25, Desc = "The good ol' health potion, now with 5 shots of caffeine and no MSG!" };
        public static Item mediumHealthPotionPack = new Item { Name = "Medium Health Potion Pack", Type = ItemType.Restorative, Lvl = 2, Worth = 25, Desc = "5 medium health potions!? Is it christmas already? Get those f**kin socks away from me!" };
        public static Item mediumManaPotion = new Item { Name = "Medium Mana Potion", Type = ItemType.Restorative, Lvl = 2, Worth = 35, Desc = "A brew that fills your body with Magic energy, the health information sticker wore off long ago, don't worry about what is inside." };
        public static Item mediumManaPotionPack = new Item { Name = "Medium Mana Potion Pack", Type = ItemType.Restorative, Lvl = 2, Worth = 35, Desc = "5 medium mana potions!? Hot damn this pack radiates awesomeness... or is that radiation?" };
        public static Item largeHealthPotion = new Item { Name = "Large Health Potion", Type = ItemType.Restorative, Lvl = 3, Worth = 70, Desc = "The good ol' health potion, now with 55 shots of caffeine and negative MSG!" };
        public static Item largeManaPotion = new Item { Name = "Large Mana Potion", Type = ItemType.Restorative, Lvl = 3, Worth = 90, Desc = "A brew that fills your body with Medicinal energy, the mana information sticker wore off long ago, don't worry about the organism." };
        public static Item rockLotion = new Item { Name = "Rock Lotion", Type = ItemType.Buff, Lvl = 1, Worth = 10, Physical = 10, Desc = "Lotion that turns skin to rock hard rock, I know what you are thinking.... but don't put the lotion there, bad idea" };
        public static Item lightningRod = new Item { Name = "Lightning Rod", Type = ItemType.Buff, Lvl = 1, Worth = 10, Lightning = 10, Desc = "A lightning rod you wear on your back with a metal cable dragging on the ground, no money back guarentee" };
        public static Item bucketOfWater = new Item { Name = "Bucket of Water", Type = ItemType.Buff, Lvl = 1, Worth = 10, Fire = 10, Desc = "A bucket, filled with water to put out fire. I might have washed my car with it, don't worry it works, I promise" };
        public static Item dryTowel = new Item { Name = "A Dry Towel", Type = ItemType.Buff, Lvl = 1, Worth = 10, Ice = 10, Desc = "Light this towel on fire and melt some ice! Cuz this towel is flammable... No you're a towel!" };
        public static Item magicReverb = new Item { Name = "Magic Reverb", Type = ItemType.Buff, Lvl = 1, Worth = 10, Magic = 10, Desc = "Reverberates incoming magic to prevent magic damage. I can feel the vibrations. running. up. my leg!" };
        public static Item footballJacket = new Item { Name = "Football Jacket", Type = ItemType.Buff, Lvl = 1, Worth = 10, Wind = 10, Desc = "A football jacket that helps prevent wind, it's not soccer, it's FOOTBALL!!!" };
        public static Item bomb = new Item { Name = "Bomb", Type = ItemType.Damaging, Lvl = 1, Worth = 20, Physical = 10, Count = 1, Desc = "A hard and heavy sphere with rope coming out of the top, what happens if we light it on fire?" };
        public static Item scrollLightning = new Item { Name = "Scroll of Zap Zap", Type = ItemType.Damaging, Lvl = 1, Worth = 20, Lightning = 10, Count = 1, Desc = "Have you ever butt raced on a carpet and shocked a friend? This is waaaaaaaaaay better!" };
        public static Item scrollFire = new Item { Name = "The \"Lighter\" Scroll", Type = ItemType.Damaging, Lvl = 1, Worth = 20, Fire = 10, Count = 1, Desc = "Oh, you wanna know if I have a lite huh? I will show you a lite" };
        public static Item scrollIce = new Item { Name = "Scroll from inside the Ice Machine", Type = ItemType.Damaging, Lvl = 1, Worth = 20, Ice = 10, Count = 1, Desc = "Found inside an old ice machine in the 80's next to a scorpion, they didn't look too friendly towards each other" };
        public static Item scrollMagic = new Item { Name = "Magic Scroll", Type = ItemType.Damaging, Lvl = 1, Worth = 20, Magic = 10, Count = 1, Desc = "A scroll just oooozing magic energy, don't touch it... It's very sticky" };
        public static Item scrollWind = new Item { Name = "Wind Scroll", Type = ItemType.Damaging, Lvl = 1, Worth = 20, Wind = 10, Count = 1, Desc = "A scroll that imbues the power of reaally powerfull flatulence, strong winds approach, your beef is strong!" };
        public static Item repairPowder = new Item { Name = "Repair Powder", Type = ItemType.Repair, Lvl = 1, Worth = 50, Count = 1, Desc = "Powder that repairs stuff, I hear it also gives you a wicked high if snorted... don't ask me how I know" };
        public static Item repairPowderPack = new Item { Name = "Repair Powder", Type = ItemType.Repair, Lvl = 1, Worth = 50, Count = 5, Desc = "Powder that repairs stuff, I hear it also gives you a wicked high if snorted... don't ask me how I know" };

        #endregion

        #region Item Methods

        public static Item CopyNewItem(Item itemToCopy)
        {
            Item newItem = new Item();
            PropertyCopy.Copy(itemToCopy, newItem);
            return newItem;
        }

        public static int CalculateHealthPotion(this Item restorative, Character character)
        {
            int healthReturn = 0;
            switch (restorative.Lvl)
            {
                case 1:
                    healthReturn = 25;
                    break;
                case 2:
                    healthReturn = 35;
                    break;
                case 3:
                    healthReturn = 45;
                    break;
                case 4:
                    healthReturn = 60;
                    break;
                case 5:
                    healthReturn = 80;
                    break;
                default:
                    healthReturn = 100;
                    break;
            }
            healthReturn = character.MaxHP / (100 / healthReturn);
            return healthReturn;
        }

        public static int CalculateManaPotion(this Item restorative, Character character)
        {
            int manaReturn = 0;
            switch (restorative.Lvl)
            {
                case 1:
                    manaReturn = 15;
                    break;
                case 2:
                    manaReturn = 25;
                    break;
                case 3:
                    manaReturn = 40;
                    break;
                case 4:
                    manaReturn = 60;
                    break;
                case 5:
                    manaReturn = 75;
                    break;
                default:
                    manaReturn = 100;
                    break;
            }
            manaReturn = character.MaxMana / (100 / manaReturn);
            return manaReturn;
        }

        public static int CalculateAfflictionTurnCount(this Item item)
        {
            int turns = 0;
            if (item.Lvl <= 1)
                turns = 1;
            else if (item.Lvl == 2 || item.Lvl == 3)
                turns = 2;
            else if (item.Lvl == 4 || item.Lvl == 5)
                turns = 3;
            else if (item.Lvl > 5 && item.Lvl <= 8)
                turns = 4;
            else
                turns = 5;
            return turns;
        }

        #endregion

        #region Item Lists

        public static List<Item> itemRestorativeList = new List<Item>()
        {
            smallHealthPotion,
            smallHealthPotionPack,
            smallManaPotion,
            smallManaPotionPack,
            mediumHealthPotion,
            mediumHealthPotionPack,
            mediumManaPotion,
            mediumManaPotionPack,
            largeHealthPotion,
            largeManaPotion
        };

        public static List<Item> itemBuffList = new List<Item>()
        {
            rockLotion,
            lightningRod,
            bucketOfWater,
            dryTowel,
            magicReverb,
            footballJacket
        };

        public static List<Item> itemDamagingList = new List<Item>()
        {
            bomb,
            scrollFire,
            scrollIce,
            scrollLightning,
            scrollMagic,
            scrollWind
        };

        public static List<Item> itemRepairList = new List<Item>()
        {
            repairPowder,
            repairPowderPack
        };

        #endregion
    }

    public static class Enemies
    {
        #region Enemy Methods

        public static EnemyType ChooseEnemyType()
        {
            EnemyType type = EnemyType.Goblin;
            return type;
        }

        public static Enemy EnemyRanGen(int level)
        {
            Enemy enemy = new Enemy();

            return enemy;
        }

        public static Enemy CopyNewEnemy(Enemy enemyToCopy)
        {
            Enemy newEnemy = new Enemy();
            PropertyCopy.Copy(enemyToCopy, newEnemy);
            return newEnemy;
        }

        #endregion

        #region Static Enemies

        public static Enemy punchingBag = new Enemy() { Name = "PunchingBag", Desc = "I was created by our developer gods as a baseline for combat, I also pass butter", Def = 5, Dex = 5, Int = 5, Lck = 5, Lvl = 1, Mana = 5, MaxHP = 100, CurrentHP = 100, Str = 5, Spd = 100, ExpLoot = 50, Armor = Armors.basicEnemyArmor, Weapon = Weapons.enemySword };

        #endregion
    }

    public class Testing
    {
        #region Testing Methods

        public static string line = Environment.NewLine;

        /// <summary>
        /// Generates a random Weapon and displays all of the properties in a string
        /// </summary>
        /// <param name="namer">This is the name of the Weapon</param>
        /// <returns></returns>
        public static string RandomWeap(out string namer)
        {
            RarityType rarity = LootDrop.ChooseRarity();
            var pickedLoot = LootDrop.WeaponPicker(rarity, testiculeesCharacter);
            namer = pickedLoot.Name;
            return ($"{line}Name: {pickedLoot.Name}{line}Description: {pickedLoot.Desc}{line}Type: {pickedLoot.Type.ToString()}{line}Unique: {pickedLoot.IsUnique}{line}Rarity: {pickedLoot.Rarity}{line}Level: {pickedLoot.Lvl}{line}Max Durability: {pickedLoot.MaxDurability}{line}Current Durability: {pickedLoot.CurrentDurability}{line}Worth: {pickedLoot.Worth}{line}Speed: {pickedLoot.Speed}{line}Physical Damage: {pickedLoot.PhysicalDamage}");
        }

        public static string RandomSpell()
        {
            RarityType rarity = LootDrop.ChooseRarity();
            var spell = LootDrop.SpellPicker(rarity, testiculeesCharacter);
            return $"{line}Name: {spell.Name}{line}Description: {spell.Desc}{line}Type: {spell.Type}{line}Unique: {spell.IsUnique}{line}Rarity: {spell.Rarity}{line}ManaCost: {spell.ManaCost}{line}Level: {spell.Lvl}{line}Speed: {spell.Speed}{line}Worth: {spell.Worth}{line}Physical: {spell.PhysicalDamage}{line}Fire: {spell.FireDamage}{line}Ice: {spell.IceDamage}{line}Lightning: {spell.LightningDamage}{line}Magic: {spell.MagicDamage}{line}Wind: {spell.WindDamage}";
        }

        public static string RandomArmor(out string namer)
        {
            string result = string.Empty;
            RarityType rarity = LootDrop.ChooseRarity();
            var armor = LootDrop.ArmorPicker(rarity, testiculeesCharacter);
            namer = armor.Name;
            foreach (var prop in armor.GetType().GetProperties())
            {
                result = $"{result}{line}{prop.Name}: {prop.GetValue(armor)}";
            }
            return $"Your Loot is: {result}";
        }

        public static string RandomItem()
        {
            string result = string.Empty;
            RarityType rarity = LootDrop.ChooseRarity();
            var item = LootDrop.ItemPicker(rarity, testiculeesCharacter);
            foreach (var prop in item.GetType().GetProperties())
            {
                result = $"{result}{line}{prop.Name}: {prop.GetValue(item)}";
            }
            return $"Your Loot is: {result}";
        }

        public static string LootDropGen()
        {
            string result = string.Empty;
            IBackPackItem loot = LootDrop.PickLoot(testiculeesCharacter);
            foreach (var prop in loot.GetType().GetProperties())
            {
                result = $"{result}{line}{prop.Name}: {prop.GetValue(loot)}";
            }
            var lootReturn = loot.GetLootType().ToString();
            return $"Your Loot is: {lootReturn}{result}";
        }

        public static string RandomMassTestWeap(int num)
        {
            int sword = 0;
            int dagger = 0;
            int greatsword = 0;
            int katana = 0;
            int staff = 0;
            int focusStone = 0;
            int spear = 0;
            int dragonSpear = 0;
            int twinSwords = 0;
            int other = 0;
            int starter = 0;
            int unique = 0;
            int common = 0;
            int uncommon = 0;
            int rare = 0;
            int epic = 0;
            int legendary = 0;
            var sw = new System.Diagnostics.Stopwatch();

            sw.Start();
            for (int i = 1; i <= num; i++)
            {
                RarityType rarity = LootDrop.ChooseRarity();
                var pickedLoot = LootDrop.WeaponPicker(rarity, testiculeesCharacter);
                switch (pickedLoot.Type)
                {
                    case RPG.WeaponType.Dagger:
                        dagger++;
                        break;
                    case RPG.WeaponType.DragonSpear:
                        dragonSpear++;
                        break;
                    case RPG.WeaponType.FocusStone:
                        focusStone++;
                        break;
                    case RPG.WeaponType.Greatsword:
                        greatsword++;
                        break;
                    case RPG.WeaponType.Katana:
                        katana++;
                        break;
                    case RPG.WeaponType.Spear:
                        spear++;
                        break;
                    case RPG.WeaponType.Staff:
                        staff++;
                        break;
                    case RPG.WeaponType.Sword:
                        sword++;
                        break;
                    case RPG.WeaponType.TwinSwords:
                        twinSwords++;
                        break;
                    case RPG.WeaponType.Other:
                        other++;
                        break;
                    case RPG.WeaponType.Starter:
                        starter++;
                        break;
                }
                switch (pickedLoot.Rarity)
                {
                    case RPG.RarityType.Common:
                        common++;
                        break;
                    case RPG.RarityType.Uncommon:
                        uncommon++;
                        break;
                    case RPG.RarityType.Rare:
                        rare++;
                        break;
                    case RPG.RarityType.Epic:
                        epic++;
                        break;
                    case RPG.RarityType.Legendary:
                        legendary++;
                        break;
                }
                if (pickedLoot.IsUnique) unique++;
            }
            sw.Stop();
            return ($"{line}--------------------------{line}It took {sw.Elapsed.TotalSeconds} seconds to run {num} times.{line}--------------------------{line}sword = {sword}{line}dagger = {dagger}{line}greatsword = {greatsword}{line}katana = {katana}{line}staff = {staff}{line}focusStone = {focusStone}{line}spear = {spear}{line}dragonSpear = {dragonSpear}{line}twinSwords = {twinSwords}{line}other = {other}{line}starter = {starter}{line}unique = {unique}{line}------------------------------------------------{line}common = {common}{line}uncommon = {uncommon}{line}rare = {rare}{line}epic = {epic}{line}legendary = {legendary}{line}");
        }

        public static string RandomMassTestSpell(int num)
        {
            int attack = 0;
            int defense = 0;
            int restorative = 0;
            int starter = 0;
            int physical = 0;
            int magic = 0;
            int fire = 0;
            int lightning = 0;
            int ice = 0;
            int wind = 0;
            int unique = 0;
            int common = 0;
            int uncommon = 0;
            int rare = 0;
            int epic = 0;
            int legendary = 0;

            for (int i = 1; i <= num; i++)
            {
                RarityType rarity = LootDrop.ChooseRarity();
                ElementType element;
                var spell = LootDrop.SpellPicker(rarity, testiculeesCharacter, out element);
                switch (spell.Type)
                {
                    case SpellType.Attack:
                        attack++;
                        break;
                    case SpellType.Defense:
                        defense++;
                        break;
                    case SpellType.Restorative:
                        restorative++;
                        break;
                    case SpellType.Starter:
                        starter++;
                        break;
                }
                switch (element)
                {
                    case ElementType.Fire:
                        fire++;
                        break;
                    case ElementType.Ice:
                        ice++;
                        break;
                    case ElementType.Lightning:
                        lightning++;
                        break;
                    case ElementType.Magic:
                        magic++;
                        break;
                    case ElementType.Physical:
                        physical++;
                        break;
                    case ElementType.Wind:
                        wind++;
                        break;
                }
                switch (spell.Rarity)
                {
                    case RarityType.Common:
                        common++;
                        break;
                    case RarityType.Epic:
                        epic++;
                        break;
                    case RarityType.Legendary:
                        legendary++;
                        break;
                    case RarityType.Rare:
                        rare++;
                        break;
                    case RarityType.Uncommon:
                        uncommon++;
                        break;
                }
                if (spell.IsUnique) unique++;
            }
            return $"{line}Attack: {attack}{line}Defense: {defense}{line}Restorative: {restorative}{line}Starter: {starter}{line}--------------------------------------------{line}Physical: {physical}{line}Magic: {magic}{line}Fire: {fire}{line}Lightning: {lightning}{line}Ice: {ice}{line}Wind: {wind}{line}--------------------------------------------{line}Unique: {unique}{line}--------------------------------------------{line}Common: {common}{line}UnCommon: {uncommon}{line}Rare: {rare}{line}Epic: {epic}{line}Legendary: {legendary}{line}";
        }

        public static string RandomMassTestArmor(int num)
        {
            int isUnique = 0;
            int light = 0;
            int medium = 0;
            int heavy = 0;
            int common = 0;
            int uncommon = 0;
            int rare = 0;
            int epic = 0;
            int legendary = 0;
            var sw = new System.Diagnostics.Stopwatch();

            sw.Start();
            for (int i = 1; i <= num; i++)
            {
                RarityType rarity = LootDrop.ChooseRarity();
                var loot = LootDrop.ArmorPicker(rarity, testiculeesCharacter);
                switch (loot.Type)
                {
                    case ArmorType.Light:
                        light++;
                        break;
                    case ArmorType.Medium:
                        medium++;
                        break;
                    case ArmorType.Heavy:
                        heavy++;
                        break;
                }
                switch (loot.Rarity)
                {
                    case RarityType.Common:
                        common++;
                        break;
                    case RarityType.Epic:
                        epic++;
                        break;
                    case RarityType.Legendary:
                        legendary++;
                        break;
                    case RarityType.Rare:
                        rare++;
                        break;
                    case RarityType.Uncommon:
                        uncommon++;
                        break;
                }
                if (loot.IsUnique) isUnique++;
            }
            sw.Stop();
            return $"{line}--------------------------{line}It took {sw.Elapsed} seconds to run {num} times.{line}--------------------------{line}Light: {light}{line}Medium: {medium}{line}Heavy: {heavy}{line}--------------------------{line}Common: {common}{line}Uncommon: {uncommon}{line}Rare: {rare}{line}Epic: {epic}{line}Legendary: {legendary}{line}--------------------------{line}Unique: {isUnique}{line}";

        }

        public static string RandomMassTestItem(int num)
        {
            int restorative = 0;
            int buff = 0;
            int damaging = 0;
            int currency = 0;
            int worth = 0;
            int repair = 0;
            int common = 0;
            int uncommon = 0;
            int rare = 0;
            int epic = 0;
            int legendary = 0;
            int isUnique = 0;
            int timesRan = 0;
            var sw = new System.Diagnostics.Stopwatch();

            sw.Start();
            for (int i = 1; i <= num; i++)
            {
                RarityType rarity = LootDrop.ChooseRarity();
                var item = LootDrop.ItemPicker(rarity, testiculeesCharacter);
                switch (item.Type)
                {
                    case ItemType.Buff:
                        buff++;
                        break;
                    case ItemType.Currency:
                        currency++;
                        worth = worth + item.Worth;
                        break;
                    case ItemType.Damaging:
                        damaging++;
                        break;
                    case ItemType.Repair:
                        repair++;
                        break;
                    case ItemType.Restorative:
                        restorative++;
                        break;
                }
                switch (item.Rarity)
                {
                    case RarityType.Common:
                        common++;
                        break;
                    case RarityType.Epic:
                        epic++;
                        break;
                    case RarityType.Legendary:
                        legendary++;
                        break;
                    case RarityType.Rare:
                        rare++;
                        break;
                    case RarityType.Uncommon:
                        uncommon++;
                        break;
                }
                if (item.IsUnique) isUnique++;
                timesRan++;
            }
            sw.Stop();
            return $"{line}--------------------------{line}" +
                $"It took {sw.Elapsed.TotalSeconds} seconds to run {timesRan} times.{line}" +
                $"--------------------------{line}" +
                $"Restorative: {restorative}{line}" +
                $"Buff: {buff}{line}" +
                $"Damaging: {damaging}{line}" +
                $"Currency: {currency}{line}" +
                $"     Amount: {worth}{line}" +
                $"Repair: {repair}{line}" +
                $"--------------------------{line}" +
                $"Common: {common}{line}" +
                $"Uncommon: {uncommon}{line}" +
                $"Rare: {rare}{line}" +
                $"Epic: {epic}{line}" +
                $"Legendary: {legendary}{line}" +
                $"--------------------------{line}" +
                $"Unique: {isUnique}";
        }

        public static string RandomMassTestLoot(int num)
        {
            string yaBlewIt = "";
            if (num > 1000000)
            {
                yaBlewIt = $"The max number was 1,000,000 you fool! Rerunning your result with the maxium number. Which is 1,000,000. 1,000,000. In case you forgot.{line}";
                num = 1000000;
            }
            else if (num < 2)
            {
                yaBlewIt = $"I pity the fool that tries to run the Mass Loot test less than 2 times.";
                num = 2;
            }
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            int armor = 0;
            int light = 0;
            int medium = 0;
            int heavy = 0;
            int item = 0;
            int restore = 0;
            int buff = 0;
            int damage = 0;
            int money = 0;
            int repair = 0;
            int pebble = 0;
            int spell = 0;
            int attack = 0;
            int defense = 0;
            int restorative = 0;
            int spellStarter = 0;
            int weap = 0;
            int sword = 0;
            int dagger = 0;
            int greatsword = 0;
            int katana = 0;
            int staff = 0;
            int focusStone = 0;
            int spear = 0;
            int dragonSpear = 0;
            int twinSwords = 0;
            int other = 0;
            int weaponStarter = 0;
            int common = 0;
            int uncommon = 0;
            int rare = 0;
            int epic = 0;
            int legendary = 0;
            int isUnique = 0;
            for (int i = 1; i <= num; i++)
            {
                IBackPackItem loot = LootDrop.PickLoot(testiculeesCharacter);

                switch (LootDrop.GetLootType(loot))
                {
                    case LootDrop.LootType.Armor:
                        armor++;
                        Armor arm = (Armor)loot;
                        switch (arm.Type)
                        {
                            case ArmorType.Light:
                                light++;
                                break;
                            case ArmorType.Medium:
                                medium++;
                                break;
                            case ArmorType.Heavy:
                                heavy++;
                                break;
                        }
                        break;
                    case LootDrop.LootType.Item:
                        item++;
                        Item ite = (Item)loot;
                        switch (ite.Type)
                        {
                            case ItemType.Buff:
                                buff++;
                                break;
                            case ItemType.Currency:
                                money++;
                                break;
                            case ItemType.Damaging:
                                damage++;
                                break;
                            case ItemType.Repair:
                                repair++;
                                break;
                            case ItemType.Restorative:
                                restore++;
                                break;
                        }
                        break;
                    case LootDrop.LootType.Nothing:
                        pebble++;
                        break;
                    case LootDrop.LootType.Spell:
                        spell++;
                        Spell spe = (Spell)loot;
                        switch (spe.Type)
                        {
                            case SpellType.Attack:
                                attack++;
                                break;
                            case SpellType.Defense:
                                defense++;
                                break;
                            case SpellType.Restorative:
                                restorative++;
                                break;
                            case SpellType.Starter:
                                spellStarter++;
                                break;
                        }
                        break;
                    case LootDrop.LootType.Weapon:
                        weap++;
                        Weapon wea = (Weapon)loot;
                        switch (wea.Type)
                        {
                            case WeaponType.Sword:
                                sword++;
                                break;
                            case WeaponType.Dagger:
                                dagger++;
                                break;
                            case WeaponType.Greatsword:
                                greatsword++;
                                break;
                            case WeaponType.Katana:
                                katana++;
                                break;
                            case WeaponType.Staff:
                                staff++;
                                break;
                            case WeaponType.FocusStone:
                                focusStone++;
                                break;
                            case WeaponType.Spear:
                                spear++;
                                break;
                            case WeaponType.DragonSpear:
                                dragonSpear++;
                                break;
                            case WeaponType.TwinSwords:
                                twinSwords++;
                                break;
                            case WeaponType.Other:
                                other++;
                                break;
                            case WeaponType.Starter:
                                weaponStarter++;
                                break;
                        }
                        break;
                }
                switch (loot.Rarity)
                {
                    case RarityType.Common:
                        common++;
                        break;
                    case RarityType.Epic:
                        epic++;
                        break;
                    case RarityType.Legendary:
                        legendary++;
                        break;
                    case RarityType.Rare:
                        rare++;
                        break;
                    case RarityType.Uncommon:
                        uncommon++;
                        break;
                }
                if (loot.IsUnique) isUnique++;
            }
            sw.Stop();
            return $"{yaBlewIt}{line}You simulated {num} loot drops.{line}It took {sw.Elapsed.TotalSeconds} seconds to run.{line}Here are your results:{line}---------------------------{line}Armor: {armor}{line}     Light: {light}{line}     Medium: {medium}{line}     Heavy: {heavy}{line}---------------------------{line}Item: {item}{line}     Restore: {restore}{line}     Buff: {buff}{line}     Damage: {damage}{line}     Money: {light}{line}     Repair: {repair}{line}---------------------------{line}Nothing: {pebble}{line}---------------------------{line}Spell: {spell}{line}     Attack: {attack}{line}     Defense: {defense}{line}     Restorative: {restorative}{line}     Starter Spell: {spellStarter}{line}---------------------------{line}Weapon: {weap}{line}     Sword: {sword}{line}     Dagger: {dagger}{line}     GreatSword: {greatsword}{line}     Katana: {katana}{line}     Staff: {staff}{line}     Focus Stone: {focusStone}{line}     Spear: {spear}{line}     Dragon Spear:  {dragonSpear}{line}     Twin Swords: {twinSwords}{line}     Other:  {other}{line}     Starter Weapon: {weaponStarter}{line}---------------------------{line}Rarity:{line}     Common:  {common}{line}     Uncommon: {uncommon}{line}     Rare: {rare}{line}     Epic: {epic}{line}     Legendary: {legendary}{line}---------------------------{line}Unique Items: {isUnique}";
        }

        public static string GetMarried()
        {
            var weapon = RandomWeap(out string wNamer);
            var armor = RandomArmor(out string aNamer);
            var time = rng.Next(100000, 382947828);
            var whatchaSay = weapon != null ? ($"{line}Hello Richard.{line}You have {time} seconds(s) of safety remaining until Heather uses {wNamer} on yo ass. You know why.{line}Hint: Marriage.{line}And don't think your {aNamer} will protect you.{line}Good Luck!{line}**************************{weapon}") : ($"{line}Hello Richard.{line}You have {time} seconds(s) of safety remaining until Heather uses [[ExceptionUnhandled]] on yo ass. You know why.{line}Hint: Marriage.{line}Good Luck!{line}***********************{line}This was supposed to be a weapon but the code blew it.{line}Thankfully I handled the unhandled exception exceptionally.");
            return whatchaSay;
        }

        #endregion

        #region Testing Assets

        public static OwnerProfile testiculeesProfile = new OwnerProfile()
        {
            CurrentCharacter = testiculeesCharacter,
            CharacterList = new List<Character>() { testiculeesCharacter },
            Currency = 696969,
            OwnerID = 12345678910111213
        };

        public static Character testiculeesCharacter = new Character()
        {
            Name = "Testiculees teh Great",
            Class = CharacterClass.Dragoon,
            Lvl = 1,
            OwnerID = testiculeesProfile.OwnerID,
            Armor = Armors.knightArmor,
            MaxHP = 12000000,
            CurrentHP = 12000000,
            Weapon = Weapons.warriorFists,
            Exp = 0,
            Str = 10,
            Def = 10,
            Spd = 100,
            Lck = 10
        };

        #endregion
    }

}
