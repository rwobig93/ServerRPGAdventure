using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PersonalDiscordBot.Classes.RPG;

namespace PersonalDiscordBot.Classes
{
    public static class RPG
    {
        public interface IBackPackItem { };

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
            TwinSwords
        }

        public enum ItemType
        {
            Restorative,
            Buff,
            Damaging,
            Currency
        }
    }

    public static class Management
    {

    }

    public class Character
    {
        string Name { get; set; }
        CharacterClass Class { get; set; }
        Weapon Weapon { get; set; }
        BackPack Backpack { get; set; }
        int Lvl { get; set; }
        int Exp { get; set; }
        int HP { get; set; }
        int Str { get; set; }
        int Def { get; set; }
        int Dex { get; set; }
        int Int { get; set; }
        int Spd { get; set; }
        int Lck { get; set; }
    }

    public class BackPack
    {
        string Name { get; set; }
        int Capacity { get; set; }
        int Weight { get; set; }
        int Currency { get; set; }
        List<IBackPackItem> Stored = new List<IBackPackItem>();
        List<Spell> Spells = new List<Spell>();
    }

    public class Weapon : IBackPackItem
    {
        string Name { get; set; }
        WeaponType Type { get; set; }
        int Lvl { get; set; }
        int Durability { get; set; }
        int Speed { get; set; }
        int PhysicalDamage { get; set; }
        int MagicDamage { get; set; }
        int FireDamage { get; set; }
        int LightningDamage { get; set; }
        int IceDamage { get; set; }
        int WindDamage { get; set; }
    }

    public class Spell
    {
        string Name { get; set; }
        int Lvl { get; set; }
        int Speed { get; set; }
        int PhysicalDamage { get; set; }
        int MagicDamage { get; set; }
        int FireDamage { get; set; }
        int LightningDamage { get; set; }
        int IceDamage { get; set; }
        int WindDamage { get; set; }
    }

    public class Item : IBackPackItem
    {
        string Name { get; set; }
        ItemType Type { get; set; }
        int Lvl { get; set; }
        int Worth { get; set; }
        int Physical { get; set; }
        int Magic { get; set; }
        int Fire { get; set; }
        int Lightning { get; set; }
        int Ice { get; set; }
        int Wind { get; set; }
    }
}
