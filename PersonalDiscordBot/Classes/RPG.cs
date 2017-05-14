using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static PersonalDiscordBot.Classes.RPG;

namespace PersonalDiscordBot.Classes
{
    public static class RPG
    {
        public interface IBackPackItem { };
        public static Random rng = new Random(Environment.TickCount);

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
            Starter
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
    }

    public static class Management
    {
        public static List<Character> CharacterList = new List<Character>();

        public static Character CreateNewCharacter(ulong ownerId, CharacterClass chosenClass)
        {
            Character newChar = new Character();
            switch (chosenClass)
            {
                case CharacterClass.Dragoon:
                    break;
                case CharacterClass.Mage:
                    break;
                case CharacterClass.Necromancer:
                    break;
                case CharacterClass.Rogue:
                    break;
                case CharacterClass.Warrior:
                    newChar.Lvl = 1;
                    newChar.Exp = 0;
                    newChar.MaxHP = rng.Next(50, 100);
                    newChar.CurrentHP = newChar.MaxHP;
                    newChar.Backpack = new BackPack
                    {
                        Stored =
                        {
                            Weapons.warriorFists,
                            Armors.knightArmor,
                            Items.smallHealthPotionPack
                        }
                    };
                    break;
            }
            return newChar;
        }
    }

    public class Character
    {
        public ulong OwnerID { get; set; }
        public string Name { get; set; }
        public CharacterClass Class { get; set; }
        public Weapon Weapon { get; set; }
        public BackPack Backpack { get; set; }
        public int Lvl { get; set; }
        public int Exp { get; set; }
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

    public class BackPack
    {
        public string Name { get; set; }
        public int Capacity { get; set; } = 10;
        public int Weight { get; set; } = 10;
        public int Currency { get; set; } = 100;
        public List<IBackPackItem> Stored = new List<IBackPackItem>();
        public List<Spell> Spells = new List<Spell>();
    }

    public class Weapon : IBackPackItem
    {
        public string Name { get; set; }
        public WeaponType Type { get; set; }
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
    }

    public class Spell
    {
        public string Name { get; set; }
        public int Lvl { get; set; } = 0;
        public bool Starter { get; set; } = false;
        public int ManaCost { get; set; } = 1;
        public int Speed { get; set; } = 100;
        public int PhysicalDamage { get; set; } = 0;
        public int MagicDamage { get; set; } = 0;
        public int FireDamage { get; set; } = 0;
        public int LightningDamage { get; set; } = 0;
        public int IceDamage { get; set; } = 0;
        public int WindDamage { get; set; } = 0;
    }

    public class Item : IBackPackItem
    {
        public string Name { get; set; }
        public ItemType Type { get; set; }
        public int Lvl { get; set; } = 0; //used to calculate amount of Repair
        public int Count { get; set; } = 1;
        public int Worth { get; set; } = 0;
        public int Physical { get; set; } = 0;
        public int Magic { get; set; } = 0;
        public int Fire { get; set; } = 0;
        public int Lightning { get; set; } = 0;
        public int Ice { get; set; } = 0;
        public int Wind { get; set; } = 0;
    }

    public class Armor : IBackPackItem
    {
        public string Name { get; set; }
        public ArmorType Type { get; set; }
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
    }

    public static class Weapons
    {
        public static Weapon warriorFists = new Weapon { Name = "Warrior Fists", Type = WeaponType.Starter, PhysicalDamage = 5 };
        public static Weapon rogueDaggers = new Weapon { Name = "Rogues' Daggers", Speed = 110, PhysicalDamage = 3, Type = WeaponType.Starter };
        public static Weapon dragonSpear = new Weapon { Name = "Novice Dragon Hunter Spear", Speed = 80, Type = WeaponType.Starter, PhysicalDamage = 6, FireDamage = 1, LightningDamage = 1, IceDamage = 1, WindDamage = 1 };
    }

    public static class Spells
    {
        public static Spell magesEnergy = new Spell { Name = "Mages' Energy", MagicDamage = 5, ManaCost = 0, Lvl = 0, Starter = true };
        public static Spell boneSpike = new Spell { Name = "Necromancer Bone Spike", ManaCost = 0, PhysicalDamage = 2, WindDamage = 1, MagicDamage = 2, Starter = true  };
    }

    public static class Armors
    {
        public static Armor knightArmor = new Armor { Name = "Novice Knight Armor", Type = ArmorType.Heavy, Lvl = 1, Speed = 50, Worth = 100, MaxDurability = 20, CurrentDurability = 20, Physical = 100 };
        public static Armor mageRobe = new Armor { Name = "Novice Mages' Robe", Type = ArmorType.Light, CurrentDurability = 10, MaxDurability = 10, Speed = 150, Worth = 100, Magic = 100, Lvl = 1, Physical = 10 };
        public static Armor theiveGarb = new Armor { Name = "Novice Theives Garb", Type = ArmorType.Light, CurrentDurability = 15, Lvl = 1, MaxDurability = 15, Speed = 130, Worth = 100, Physical = 70, Magic = 30 };
        public static Armor undeadArmor = new Armor { Name = "Undead Armor", Type = ArmorType.Medium, CurrentDurability = 18, MaxDurability = 18, Lvl = 1, Speed = 80, Worth = 100, Physical = 85, Magic = 30 };
        public static Armor dragonArmor = new Armor { Name = "Novice Dragon Hunter Armor", Type = ArmorType.Medium, Lvl = 1, MaxDurability = 20, CurrentDurability = 20, Speed = 100, Worth = 100, Physical = 80, Fire = 10, Ice = 10, Lightning = 10, Wind = 10 };
    }

    public static class Items
    {
        public static Item smallHealthPotion = new Item { Name = "Small Health Potion", Type = ItemType.Restorative, Lvl = 1, Worth = 2 };
        public static Item smallHealthPotionPack = new Item { Name = "Small Health Potion Pack", Type = ItemType.Restorative, Lvl = 1, Worth = 2, Count = 5 };
        public static Item smallManaPotion = new Item { Name = "Small Mana Potion", Type = ItemType.Restorative, Lvl = 1, Worth = 5 };
        public static Item smallManaPotionPack = new Item { Name = "Small Mana Potion Pack", Type = ItemType.Restorative, Lvl = 1, Worth = 5, Count = 5 };
    }
}
