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
        public static Random rng = new Random((int)(DateTime.Now.Ticks & 0x7FFFFFFF));

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
            Starter,
            Other
        }
        public enum SpellType
        {
            Attack,
            Defense,
            Restorative,
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
        public static Character CreateNewCharacter(ulong ownerId, CharacterClass chosenClass)
        {
            Character newChar = new Character();
            switch (chosenClass)
            {
                case CharacterClass.Dragoon:
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
                        },
                        Spells =
                        {
                            Spells.dragonRage
                        }
                    };
                    newChar.Class = chosenClass;
                    newChar.MaxHP = rng.Next(40, 100);
                    newChar.CurrentHP = newChar.MaxHP;
                    newChar.Mana = rng.Next(5, 10);
                    newChar.Str = rng.Next(4, 18);
                    newChar.Def = rng.Next(3, 16);
                    newChar.Dex = rng.Next(10, 20);
                    newChar.Int = rng.Next(3, 10);
                    newChar.Spd = rng.Next(115, 150);
                    newChar.Lck = rng.Next(0, 10);
                    break;
                case CharacterClass.Mage:
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
                        },
                        Spells =
                        {
                            Spells.magesEnergy,
                            Spells.arcaneArmor
                        }
                    };
                    newChar.Class = chosenClass;
                    newChar.MaxHP = rng.Next(20, 80);
                    newChar.CurrentHP = newChar.MaxHP;
                    newChar.Mana = rng.Next(10, 30);
                    newChar.Str = rng.Next(1, 10);
                    newChar.Def = rng.Next(1, 15);
                    newChar.Dex = rng.Next(1, 7);
                    newChar.Int = rng.Next(10, 20);
                    newChar.Spd = rng.Next(105, 125);
                    newChar.Lck = rng.Next(0, 10);
                    break;
                case CharacterClass.Necromancer:
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
                        },
                        Spells =
                        {
                            Spells.boneSpike
                        }
                    };
                    newChar.Class = chosenClass;
                    newChar.MaxHP = rng.Next(60, 110);
                    newChar.CurrentHP = newChar.MaxHP;
                    newChar.Mana = rng.Next(8, 40);
                    newChar.Str = rng.Next(5, 12);
                    newChar.Def = rng.Next(5, 12);
                    newChar.Dex = rng.Next(6, 14);
                    newChar.Int = rng.Next(6, 25);
                    newChar.Spd = rng.Next(80, 105);
                    newChar.Lck = rng.Next(0, 10);
                    break;
                case CharacterClass.Rogue:
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
                    newChar.Mana = 0;
                    newChar.Str = rng.Next(2, 15);
                    newChar.Def = rng.Next(4, 12);
                    newChar.Dex = rng.Next(10, 20);
                    newChar.Int = rng.Next(0, 8);
                    newChar.Spd = rng.Next(115, 140);
                    newChar.Lck = rng.Next(1, 12);
                    break;
                case CharacterClass.Warrior:
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
                    newChar.Mana = 0;
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
    }

    public class OwnerProfile
    {
        public ulong OwnerID { get; set; }
        public List<Character> CharacterList = new List<Character>();
        public int Currency { get; set; } = 100;
        public int FightsTotal { get; set; } = 0;
        public int FightsWon { get; set; } = 0;
        public int FigthsLost { get; set; } = 0;
        public int PlayerFightsTotal { get; set; } = 0;
        public int PlayerFightsWon { get; set; } = 0;
        public int PlayerFightsLost { get; set; } = 0;
        public int BossesBeat { get; set; } = 0;
        public int TotalPebbles { get; set; } = 0;
    }
    public class Character
    {
        public ulong OwnerID { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; } = "A new adventurer set out to..... Adventure?";
        public CharacterClass Class { get; set; }
        public Weapon Weapon { get; set; }
        public Armor Armor { get; set; }
        public BackPack Backpack { get; set; }
        public int Pebbles { get; set; } = 0;
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
    public class Enemy
    {
        public string Name { get; set; }
        public string Desc { get; set; } = "I want your bod, not in a sexual or romantic way. But more of a dead and ragdoll kinda way";
        public Weapon Weapon { get; set; }
        public Armor Armor { get; set; }
        public int Lvl { get; set; }
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
        public LootDrop Loot { get; set; }
    }
    public class BackPack
    {
        public string Name { get; set; }
        public string Desc { get; set; } = "This weird container made of animal skin holds stuff..... I think";
        public int Capacity { get; set; } = 10;
        public int Weight { get; set; } = 10;
        public List<IBackPackItem> Stored = new List<IBackPackItem>();
        public List<Spell> Spells = new List<Spell>();
    }
    public class Weapon : IBackPackItem
    {
        public string Name { get; set; }
        public string Desc { get; set; } = "A weapon like any other";
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
        public string Desc { get; set; } = "What is this? Magic or something?";
        public int Lvl { get; set; } = 0;
        public SpellType Type { get; set; } = SpellType.Attack;
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
        public string Desc { get; set; } = "A helpful item";
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
        public string Desc { get; set; } = "An armor like any other";
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
    public class LootDrop
    {
        /// <summary>
        /// The range of the probability values (dividing a value in _lootProbabilites by this would give a probability in the range 0..1).
        /// </summary>
        protected const int MaxProbability = 1000;

        /// <summary>
        /// Choose a random loot type.
        /// </summary>
        public static LootType ChooseType()
        {
            LootType lootType = 0;         // start at first one
            int randomValue = rng.Next(MaxProbability);
            while (_lootProbabilites[(int)lootType] <= randomValue)
            {
                lootType++;         // next loot type
            }
            return lootType;
        }

        /// <summary>
        /// Return a random loot item
        /// </summary>
        /// <returns></returns>
        public static IBackPackItem PickLoot()
        {
            IBackPackItem chosenLoot = null;
            LootType chosenType = ChooseType();
            switch (chosenType)
            {
                case LootType.Item:
                    
                    break;
                case LootType.Nothing:
                    break;
                case LootType.Armor:
                    break;
                case LootType.Weapon:
                    break;
                case LootType.Spell:
                    break;
                case LootType.Backpack:
                    break;
            }
            return chosenLoot;
        }

        /// <summary>
        /// The loot types.
        /// </summary>
        public enum LootType
        {
            Item, Armor, Weapon, Spell, Backpack, Nothing
        };

        /// <summary>
        /// Cumulative probabilities - each entry corresponds to the member of LootType in the corresponding position.
        /// </summary>
        protected static int[] _lootProbabilites = new int[]
        {
            600, 100, 100, 50, 20,  // Chances: Item(60%), Armor(10%), Weapon(10%), Spell(5%), Backpack(2%), Nothing(13%)
            MaxProbability
        };
    }

    public static class Weapons
    {
        public static List<Weapon> weaponList = new List<Weapon>()
        {
            
        };
        public static Weapon warriorFists = new Weapon { Name = "Warrior Fists", Type = WeaponType.Starter, PhysicalDamage = 5, Desc = "The mighty fist, great for fisting.... I mean beating the shit out of everyone that gets in your way" };
        public static Weapon rogueDaggers = new Weapon { Name = "Rogues' Daggers", Speed = 110, PhysicalDamage = 3, Type = WeaponType.Starter, Desc = "A pair of old daggers that are rusted; Like really bad, why are you using these again?" };
        public static Weapon dragonSpear = new Weapon { Name = "Novice Dragon Hunter Spear", Speed = 80, Type = WeaponType.Starter, PhysicalDamage = 6, FireDamage = 1, LightningDamage = 1, IceDamage = 1, WindDamage = 1, Desc = "Nothing is more bad ass then a Dragon Hunter, thats why you are here, doesn't matter that there aren't any dragons around... Remember: Bad. Ass." };
        public static Weapon stick = new Weapon { Name = "A Stick", Speed = 500, Lvl = 0, Type = WeaponType.Other, Worth = 0, CurrentDurability = -1, MaxDurability = -1, PhysicalDamage = 1, Desc = "The mighty stick, it doesn't have good damage, level, or worth. But you can hit shit reeeally fast and that can be annoying as hell" };
        public static Weapon glowyOrb = new Weapon { Name = "Glowing Orb", Speed = 300, Lvl = 0, Type = WeaponType.Other, Worth = 0, CurrentDurability = -1, MaxDurability = -1, MagicDamage = 1, Desc = "You found this glowing orb in an abandoned chocolate factory, it glows a tremendous light when you hold it up and... That's it, it was probably a discontinued toy off the line" };
    }

    public static class Spells
    {
        public static List<Spell> spellList = new List<Spell>()
        {
            arcaneArmor,
            dragonRage
        };
        public static Spell magesEnergy = new Spell { Name = "Mages' Energy", MagicDamage = 5, ManaCost = 0, Lvl = 0, Type = SpellType.Starter, Desc = "The spell that started them all, some might call it the 'Hello World' spell, it gets the job done and your grueling training means you can infinitely it.... cool!" };
        public static Spell boneSpike = new Spell { Name = "Necromancer Bone Spike", ManaCost = 0, PhysicalDamage = 3, WindDamage = 1, MagicDamage = 2, Type = SpellType.Starter, Desc = "A giant spike comes out of the ground with a 70% chance of hitting the genitals, what's more to like?" };
        public static Spell arcaneArmor = new Spell { Name = "Novice Arcane Armor", Type = SpellType.Defense, PhysicalDamage = 3, ManaCost = 2, MagicDamage = 5, Lvl = 1, Desc = "Thin hovering layers of pure arcane defense here to protect you, I think the warranty expired last week. Be Careful" };
        public static Spell dragonRage = new Spell { Name = "Novice Dragon Rage", Type = SpellType.Defense, Lvl = 1, Speed = 50, ManaCost = 5, PhysicalDamage = 3, FireDamage = 3, IceDamage = 3, LightningDamage = 3, MagicDamage = 3, WindDamage = 3, Desc = "You have learned to harness a dragon's rage and use it to fuel your body to whoop some major ass" };
    }

    public static class Armors
    {
        public static List<Armor> armorList = new List<Armor>()
        {
            knightArmor,
            mageRobe,
            theiveGarb,
            undeadArmor,
            dragonArmor
        };
        public static Armor knightArmor = new Armor { Name = "Novice Knight Armor", Type = ArmorType.Heavy, Lvl = 1, Speed = 50, Worth = 100, MaxDurability = 20, CurrentDurability = 20, Physical = 100, Desc = "Some beatup old armor you found in the old shed out back, next to the bones of an old dog... what was it's name again?" };
        public static Armor mageRobe = new Armor { Name = "Novice Mages' Robe", Type = ArmorType.Light, CurrentDurability = 10, MaxDurability = 10, Speed = 150, Worth = 100, Magic = 100, Lvl = 1, Physical = 10, Desc = "These might be 'Robes' if you believe hard enough, go on, believe... I can wait" };
        public static Armor theiveGarb = new Armor { Name = "Novice Theives Garb", Type = ArmorType.Light, CurrentDurability = 15, Lvl = 1, MaxDurability = 15, Speed = 130, Worth = 100, Physical = 70, Magic = 30, Desc = "What better way to rock your first gear then to steal it, even if it was from old miss bitchface who is a blind amputee" };
        public static Armor undeadArmor = new Armor { Name = "Undead Armor", Type = ArmorType.Medium, CurrentDurability = 18, MaxDurability = 18, Lvl = 1, Speed = 80, Worth = 100, Physical = 85, Magic = 30, Desc = "Nothing weird here, you just picked up the bones from some dead people and strapped it to your body... they weren't using it anyway" };
        public static Armor dragonArmor = new Armor { Name = "Novice Dragon Hunter Armor", Type = ArmorType.Medium, Lvl = 1, MaxDurability = 20, CurrentDurability = 20, Speed = 100, Worth = 100, Physical = 80, Fire = 10, Ice = 10, Lightning = 10, Wind = 10, Desc = "Bad. Ass. Bad. Ass. Bad. Ass. Bad. Ass. - Naive thoughts running in your mind" };
    }

    public static class Items
    {
        public static List<Item> itemList = new List<Item>()
        {
            smallHealthPotion,
            smallHealthPotionPack,
            smallManaPotion,
            smallManaPotionPack
        };
        public static Item smallHealthPotion = new Item { Name = "Small Health Potion", Type = ItemType.Restorative, Lvl = 1, Worth = 2, Desc = "The good ol' health potion, now with 5 shots of caffeine and no MSG!" };
        public static Item smallHealthPotionPack = new Item { Name = "Small Health Potion Pack", Type = ItemType.Restorative, Lvl = 1, Worth = 2, Count = 5, Desc = "5 health potions!? Is it christmas already? Get those f**kin socks away from me!" };
        public static Item smallManaPotion = new Item { Name = "Small Mana Potion", Type = ItemType.Restorative, Lvl = 1, Worth = 5, Desc = "A brew that fills your body with Magic energy, the health information sticker wore off long ago, don't worry about what is inside." };
        public static Item smallManaPotionPack = new Item { Name = "Small Mana Potion Pack", Type = ItemType.Restorative, Lvl = 1, Worth = 5, Count = 5, Desc = "5 mana potions!? Hot damn this pack radiates awesomeness... or is that radiation?" };
    }
}
