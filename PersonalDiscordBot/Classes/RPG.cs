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
        #region Variables

        public interface IBackPackItem { };
        public static Random rng = new Random((int)(DateTime.Now.Ticks & 0x7FFFFFFF));
        public static int maxLevel = 20;

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
        };

        #endregion
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
                    newChar.MaxMana = 0;
                    newChar.CurrentMana = newChar.MaxMana;
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
                    newChar.MaxMana = 0;
                    newChar.CurrentMana = newChar.MaxMana;
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

    }

    #region Base Classes

    public class OwnerProfile
    {
        public ulong OwnerID { get; set; }
        public List<Character> CharacterList = new List<Character>();
        public Character CurrentCharacter { get; set; }
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
        public int MaxMana { get; set; }
        public int CurrentMana { get; set; }
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
        public RarityType Rarity { get; set; } = RarityType.Common;
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
        public RarityType Rarity { get; set; }
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

    #endregion

    public class LootDrop
    {
       
        /// <summary>
        /// The range of the probability values (dividing a value in _lootProbabilites by this would give a probability in the range 0..1).
        /// </summary>
        protected const int MaxProbability = 1000;

        #region LootDrop General

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
            600, 700, 800, 850, 870,  // Chances: Item(60%), Armor(10%), Weapon(10%), Spell(5%), Backpack(2%), Nothing(13%)
            MaxProbability
        };

        /// <summary>
        /// Cumulative probabilities - each entry corresponds to the member of RarityType in the corresponding position.
        /// </summary>
        protected static int[] _rarityProbabilities = new int[]
        {
            430, 730, 880, 980, MaxProbability // Chances: Common(43%), Uncommon(30%), Rare(15%), Epic(10%), Legendary(2%)
        };

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
        public static IBackPackItem PickLoot(Character character)
        {
            IBackPackItem chosenLoot = null;
            LootType chosenType = LootType.Weapon; //LootDrop.ChooseType();
            RarityType rarityType = ChooseRarity();
            switch (chosenType)
            {
                case LootType.Item:
                    chosenLoot = ItemPicker(rarityType, character);
                    break;
                case LootType.Nothing:
                    break;
                case LootType.Armor:
                    break;
                case LootType.Weapon:
                    chosenLoot = WeaponPicker(rarityType, character);
                    break;
                case LootType.Spell:
                    break;
                case LootType.Backpack:
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
                return Items.repairPowderPack;
            else
                return Items.repairPowder;
        }

        public static Item ItemPicker(RarityType rarity, Character character)
        {
            Item _item = null;
            int rarityValue = GetRarityValue(rarity);
            ItemType type = ChooseItemType();
            switch (type)
            {
                case ItemType.Currency:
                    _item = new Item { Worth = CurrencyPicker(character.Lvl, rarityValue), Type =  type};
                    break;
                case ItemType.Restorative:
                    _item = Items.itemRestorativeList[rng.Next(0, Items.itemRestorativeList.Count)];
                    break;
                case ItemType.Buff:
                    _item = Items.itemBuffList[rng.Next(0, Items.itemBuffList.Count)];
                    break;
                case ItemType.Damaging:
                    _item = Items.itemDamagingList[rng.Next(0, Items.itemDamagingList.Count)];
                    break;
                case ItemType.Repair:
                    _item = RepairPicker();
                    break;
            }
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
                    weaponArray = new int[] { 250, 450, 650, 795, 890, 900, 920, 940, MaxProbability, MaxProbability, MaxProbability }; // Sword(25%), Dagger(20%), Greatsword(20%), Katana(14.5%), Staff(9.5%), FocusStone(1%), Spear(2%), DragonSpear(2%), TwinSwords(6%), Other(0%), Starter(0%)
                    break;
                case CharacterClass.Dragoon:
                    weaponArray = new int[] { 100, 200, 250, 300, 350, 400, 600, 800, MaxProbability, MaxProbability, MaxProbability }; // Sword(10%), Dagger(10%), Greatsword(5%), Katana(5%), Staff(5%), FocusStone(5%), Spear(20%), DragonSpear(20%), TwinSwords(20%), Other(0%), Starter(0%)
                    break;
                case CharacterClass.Mage:
                    weaponArray = new int[] { 50, 100, 150, 200, 550, 800, 850, 900, MaxProbability, MaxProbability, MaxProbability }; // Sword(5%), Dagger(5%), Greatsword(5%), Katana(5%), Staff(55%), FocusStone(25%), Spear(5%), DragonSpear(5%), TwinSwords(10%), Other(0%), Starter(0%)
                    break;
                case CharacterClass.Necromancer:
                    weaponArray = new int[] { 50, 100, 150, 200, 450, 800, 850, 900, MaxProbability, MaxProbability, MaxProbability }; // Sword(5%), Dagger(5%), Greatsword(5%), Katana(5%), Staff(55%), FocusStone(25%), Spear(5%), DragonSpear(5%), TwinSwords(10%), Other (0%), Starter(0%)
                    break;
                case CharacterClass.Rogue:
                    weaponArray = new int[] { 150, 450, 550, 600, 650, 700, 750, 770, MaxProbability, MaxProbability, MaxProbability }; // Sword(15%), Dagger(30%), Greatsword(10%), Katana(5%), Staff(5%), FocusStone(5%), Spear(5%), DragonSpear(2%), TwinSwords(23%), Other(0%), Starter(0%)
                    break;
            }

            return weaponArray;
        }

        public static Weapon WeaponPicker(RarityType rarity, Character chara)
        {
            Weapon weap = new Weapon();

            if (rarity == RarityType.Legendary)
            {
                if (ChanceRoll(30))
                {
                    weap = Weapons.weaponList[rng.Next(0, Weapons.weaponList.Count)];
                }
                else
                {
                    weap = Weapons.WeaponRandomGen(rarity, ChooseWeaponType(chara), chara.Lvl);
                }
            }
            else
            {
                weap = Weapons.WeaponRandomGen(rarity, ChooseWeaponType(chara), chara.Lvl);
            }
            return weap;
        } 

        #endregion
    }

    public static class Weapons
    {

        #region Weapon Names and Descriptions
        public static string[] weaponNamesSword = // Weapon name & desc index should match
        {
            "Not a weapon",
            "Butterknife",
            "Pokes' you in the eye",
            "Cut you real bad",
            "A sword of sorts",
            "Stabby McStab Stab",
            "Sword from the 7th Floor"
        };
        public static string[] weaponDescSword = // Weapon name & desc index should match
        {
            "This isn't a weapon.... at least I don't think it is",
            "Used to butter that toast, or butter your bread. Mmmmm bread",
            "Everytime you attack this weapon makes an attempt to poke you in the eye... like really hard",
            "I'm gonna cut you so bad, you gonna wish I never cut you so bad",
            "This might be a sword, or might not, does it matter?",
            "Stab. Stabby. Stab, stab stab. Stab stab stab stabby Mcstab stab. Stab, stab stab.",
            "Ya blew it"
        };
        public static string[] weaponNamesGreatsword = // Weapon name & desc index should match
        {
            "Not a weapon",
            "Butterknife",
            "Pokes' you in the eye",
            "Cut you real bad",
            "A sword of sorts",
            "Stabby McStab Stab",
            "Chainsword",
            "\"Hammer\" of Thor"
        };
        public static string[] weaponDescGreatsword = // Weapon name & desc index should match
        {
            "This isn't a weapon.... at least I don't think it is",
            "Used to butter that toast, or butter your bread. Mmmmm bread",
            "Everytime you attack this weapon makes an attempt to poke you in the eye... like really hard",
            "I'm gonna cut you so bad, you gonna wish I never cut you so bad",
            "This might be a sword, or might not, does it matter?",
            "Stab. Stabby. Stab, stab stab. Stab stab stab stabby Mcstab stab. Stab, stab stab.",
            "A serrated sword that, when the magic word \"Gettum!\" is spoken it causes the serrations to spin around the blade.",
            "A one handed \"Hammer\" that deals crushing / electric damage"
        };
        public static string[] weaponNamesDagger = // Weapon name & desc index should match
        {
            "Not a weapon",
            "Butterknife",
            "Pokes' you in the eye",
            "Cut you real bad",
            "Stabby McStab Stab"
        };
        public static string[] weaponDescDagger = // Weapon name & desc index should match
        {
            "This isn't a weapon.... at least I don't think it is",
            "Used to butter that toast, or butter your bread. Mmmmm bread",
            "Everytime you attack this weapon makes an attempt to poke you in the eye... like really hard",
            "I'm gonna cut you so bad, you gonna wish I never cut you so bad",
            "Stab. Stabby. Stab, stab stab. Stab stab stab stabby Mcstab stab. Stab, stab stab."
        };
        public static string[] weaponNamesKatana = // Weapon name & desc index should match
        {
            "Not a weapon",
            "Butterknife",
            "Pokes' you in the eye",
            "Cut you real bad",
            "Stabby McStab Stab"
        };
        public static string[] weaponDescKatana = // Weapon name & desc index should match
        {
            "This isn't a weapon.... at least I don't think it is",
            "Used to butter that toast, or butter your bread. Mmmmm bread",
            "Everytime you attack this weapon makes an attempt to poke you in the eye... like really hard",
            "I'm gonna cut you so bad, you gonna wish I never cut you so bad",
            "Stab. Stabby. Stab, stab stab. Stab stab stab stabby Mcstab stab. Stab, stab stab."
        };
        public static string[] weaponNamesStaff = // Weapon name & desc index should match
        {
            "Not a weapon",
            "Pokes' you in the eye"
        };
        public static string[] weaponDescStaff = // Weapon name & desc index should match
        {
            "This isn't a weapon.... at least I don't think it is",
            "Everytime you attack this weapon makes an attempt to poke you in the eye... like really hard"
        };
        public static string[] weaponNamesFocusStone = // Weapon name & desc index should match
        {
            "Not a weapon",
            "Pokes' you in the eye"
        };
        public static string[] weaponDescFocusStone = // Weapon name & desc index should match
        {
            "This isn't a weapon.... at least I don't think it is",
            "Everytime you attack this weapon makes an attempt to poke you in the eye... like really hard"
        };
        public static string[] weaponNamesSpear = // Weapon name & desc index should match
        {
            "Not a weapon",
            "Pokes' you in the eye"
        };
        public static string[] weaponDescSpear = // Weapon name & desc index should match
        {
            "This isn't a weapon.... at least I don't think it is",
            "Everytime you attack this weapon makes an attempt to poke you in the eye... like really hard"
        };
        public static string[] weaponNamesDragonSpear = // Weapon name & desc index should match
        {
            "Not a weapon",
            "Pokes' you in the eye"
        };
        public static string[] weaponDescDragonSpear = // Weapon name & desc index should match
        {
            "This isn't a weapon.... at least I don't think it is",
            "Everytime you attack this weapon makes an attempt to poke you in the eye... like really hard"
        };
        public static string[] weaponNamesTwinSwords = // Weapon name & desc index should match
        {
            "Not a weapon",
            "Butterknife",
            "Pokes' you in the eye",
            "Cut you real bad",
            "Stabby McStab Stab"
        };
        public static string[] weaponDescTwinSwords = // Weapon name & desc index should match
        {
            "This isn't a weapon.... at least I don't think it is",
            "Used to butter that toast, or butter your bread. Mmmmm bread",
            "Everytime you attack this weapon makes an attempt to poke you in the eye... like really hard",
            "I'm gonna cut you so bad, you gonna wish I never cut you so bad",
            "Stab. Stabby. Stab, stab stab. Stab stab stab stabby Mcstab stab. Stab, stab stab."
        };
        #endregion

        #region Weapon Methods

        public static string WeaponNameandDescGen(WeaponType type, RarityType rarity, out string description, out bool isUniqueName)
        {
            string weaponName = string.Empty;
            string weaponDesc = string.Empty;
            isUniqueName = ChanceRoll(30);
            switch (type)
            {
                case WeaponType.Dagger:
                    int daggerIndex = rng.Next(0, weaponNamesDagger.Length);
                    if (isUniqueName)
                    {
                        weaponName = weaponNamesDagger[daggerIndex];
                        weaponDesc = weaponDescDagger[daggerIndex];
                    }
                    else
                    {
                        weaponName = $"{rarity.ToString()} Dagger";
                        weaponDesc = $"An average {rarity.ToString()} Dagger";
                    }
                    break;
                case WeaponType.DragonSpear:
                    int dsIndex = rng.Next(0, weaponNamesDragonSpear.Length);
                    if (isUniqueName)
                    {
                        weaponName = weaponNamesDragonSpear[dsIndex];
                        weaponDesc = weaponDescDragonSpear[dsIndex];
                    }
                    else
                    {
                        weaponName = $"{rarity.ToString()} Dragon Spear";
                        weaponDesc = $"An average {rarity.ToString()} Dragon Spear";
                    }
                    break;
                case WeaponType.FocusStone:
                    int fsIndex = rng.Next(0, weaponNamesFocusStone.Length);
                    if (isUniqueName)
                    {
                        weaponName = weaponNamesFocusStone[fsIndex];
                        weaponDesc = weaponDescFocusStone[fsIndex];
                    }
                    else
                    {
                        weaponName = $"{rarity.ToString()} Focus Stone";
                        weaponDesc = $"An average {rarity.ToString()} Focus Stone";
                    }
                    break;
                case WeaponType.Greatsword:
                    int gsIndex = rng.Next(0, weaponNamesGreatsword.Length);
                    if (isUniqueName)
                    {
                        weaponName = weaponNamesGreatsword[gsIndex];
                        weaponDesc = weaponDescGreatsword[gsIndex];
                    }
                    else
                    {
                        weaponName = $"{rarity.ToString()} Great Sword";
                        weaponDesc = $"An average {rarity.ToString()} Great Sword";
                    }
                    break;
                case WeaponType.Katana:
                    int katanaIndex = rng.Next(0, weaponNamesKatana.Length);
                    if (isUniqueName)
                    {
                        weaponName = weaponNamesKatana[katanaIndex];
                        weaponDesc = weaponDescKatana[katanaIndex];
                    }
                    else
                    {
                        weaponName = $"{rarity.ToString()} Katana";
                        weaponDesc = $"An average {rarity.ToString()} Katana";
                    }
                    break;
                case WeaponType.Spear:
                    int spearIndex = rng.Next(0, weaponNamesSpear.Length);
                    if (isUniqueName)
                    {
                        weaponName = weaponNamesSpear[spearIndex];
                        weaponDesc = weaponDescSpear[spearIndex];
                    }
                    else
                    {
                        weaponName = $"{rarity.ToString()} Spear";
                        weaponDesc = $"An average {rarity.ToString()} Spear";
                    }
                    break;
                case WeaponType.Staff:
                    int staffIndex = rng.Next(0, weaponNamesStaff.Length);
                    if (isUniqueName)
                    {
                        weaponName = weaponNamesStaff[staffIndex];
                        weaponDesc = weaponDescStaff[staffIndex];
                    }
                    else
                    {
                        weaponName = $"{rarity.ToString()} Staff";
                        weaponDesc = $"An average {rarity.ToString()} Staff";
                    }
                    break;
                case WeaponType.Sword:
                    int swordIndex = rng.Next(0, weaponNamesSword.Length);
                    if (isUniqueName)
                    {
                        weaponName = weaponNamesSword[swordIndex];
                        weaponDesc = weaponDescSword[swordIndex];
                    }
                    else
                    {
                        weaponName = $"{rarity.ToString()} Sword";
                        weaponDesc = $"An average {rarity.ToString()} Sword";
                    }
                    break;
                case WeaponType.TwinSwords:
                    int tsIndex = rng.Next(0, weaponNamesTwinSwords.Length);
                    if (isUniqueName)
                    {
                        weaponName = weaponNamesTwinSwords[tsIndex];
                        weaponDesc = weaponDescTwinSwords[tsIndex];
                    }
                    else
                    {
                        weaponName = $"{rarity.ToString()} Twin Swords";
                        weaponDesc = $"An average {rarity.ToString()} Twin Swords";
                    }
                    break;
            }
            description = weaponDesc;
            return weaponName;
        }

        public static Weapon WeaponRandomGen(RarityType rarity, WeaponType type, int level)
        {
            int rarityValue = 0;
            bool isUniqueName = false;
            string descr = string.Empty;

            rarityValue = LootDrop.GetRarityValue(rarity);

            if (level < maxLevel) level = level - 2 > 0 ? level + rng.Next(-2, 2) : level + rng.Next(0, 2);
            level = level > maxLevel ? level = maxLevel : level;

            Weapon weap = new Weapon()
            {
                Name = WeaponNameandDescGen(type, rarity, out descr, out isUniqueName),
                Desc = descr,
                Type = type,
                Rarity = rarity,
                Lvl = level
            };
            if (isUniqueName)
                rarityValue = rarityValue + 2;
            weap.MaxDurability = (10 * level) + (rarityValue * 4);
            weap.CurrentDurability = weap.MaxDurability;
            switch (type)
            {
                case WeaponType.Dagger:
                    weap.PhysicalDamage = (level + rng.Next(1, 5) + rarityValue);
                    weap.Speed = (level + 120 + rng.Next(40, 80));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case WeaponType.DragonSpear:
                    weap.PhysicalDamage = (level + rng.Next(3, 8) + rarityValue);
                    weap.FireDamage = ChanceRoll(30) ? level + rng.Next(0, 5) : 0;
                    weap.IceDamage = ChanceRoll(30) ? level + rng.Next(0, 5) : 0;
                    weap.LightningDamage = ChanceRoll(30) ? level + rng.Next(0, 5) : 0;
                    weap.WindDamage = ChanceRoll(30) ? level + rng.Next(0, 5) : 0;
                    weap.Speed = (level + 80 + rng.Next(10, 90));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed + (weap.FireDamage + weap.IceDamage + weap.LightningDamage + weap.WindDamage) / weap.PhysicalDamage));
                    break;
                case WeaponType.FocusStone:
                    weap.PhysicalDamage = (level + rng.Next(0, 2) + rarityValue);
                    weap.MagicDamage = (level + rng.Next(2, 8) + rarityValue);
                    weap.Speed = (level + 80 + rng.Next(20, 110));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.MagicDamage));
                    break;
                case WeaponType.Greatsword:
                    weap.PhysicalDamage = (level + rng.Next(8, 20) + rarityValue);
                    weap.LightningDamage = weap.Name.ToLower().Contains("hammer") ? (level + rng.Next(2, 8) + rarityValue) : 0;
                    weap.Speed = (level + 70 + rng.Next(10, 40));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case WeaponType.Katana:
                    weap.PhysicalDamage = (level + rng.Next(5, 15) + rarityValue);
                    weap.Speed = (level + 100 + rng.Next(10, 110));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case WeaponType.Spear:
                    weap.PhysicalDamage = (level + rng.Next(4, 10) + rarityValue);
                    weap.Speed = (level + 90 + rng.Next(10, 110));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case WeaponType.Staff:
                    weap.PhysicalDamage = (level + rng.Next(0, 3) + rarityValue);
                    weap.MagicDamage = (level + rng.Next(4, 10) + rarityValue);
                    weap.Speed = (level + 70 + rng.Next(20, 80));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.MagicDamage));
                    break;
                case WeaponType.Sword:
                    weap.PhysicalDamage = (level + rng.Next(4, 10) + rarityValue);
                    weap.Speed = (level + 100 + rng.Next(20, 50));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
                case WeaponType.TwinSwords:
                    weap.PhysicalDamage = ((level + rng.Next(1, 5) * 2) + rarityValue);
                    weap.Speed = (level + 80 + rng.Next(20, 70));
                    weap.Worth = (((level + rarityValue) * rarityValue) + (weap.Speed / weap.PhysicalDamage));
                    break;
            }
            return weap;
        }

        #endregion

        #region Static Weapons

        public static Weapon warriorFists = new Weapon { Name = "Warrior Fists", Type = WeaponType.Starter, CurrentDurability = -1, MaxDurability = -1, PhysicalDamage = 5, Desc = "The mighty fist, great for fisting.... I mean beating the shit out of everyone that gets in your way" };
        public static Weapon rogueDaggers = new Weapon { Name = "Rogues' Daggers", Speed = 110, PhysicalDamage = 3, Type = WeaponType.Starter, CurrentDurability = -1, MaxDurability = -1, Desc = "A pair of old daggers that are rusted; Like really bad, why are you using these again?" };
        public static Weapon dragonSpear = new Weapon { Name = "Novice Dragon Hunter Spear", Speed = 80, Type = WeaponType.Starter, CurrentDurability = -1, MaxDurability = -1, PhysicalDamage = 6, FireDamage = 1, LightningDamage = 1, IceDamage = 1, WindDamage = 1, Desc = "Nothing is more bad ass then a Dragon Hunter, thats why you are here, doesn't matter that there aren't any dragons around... Remember: Bad. Ass." };
        public static Weapon stick = new Weapon { Name = "A Stick", Speed = 500, Lvl = 0, Type = WeaponType.Other, Worth = 0, CurrentDurability = -1, MaxDurability = -1, PhysicalDamage = 1, Desc = "The mighty stick, it doesn't have good damage, level, or worth. But you can hit shit reeeally fast and that can be annoying as hell" };
        public static Weapon glowyOrb = new Weapon { Name = "Glowing Orb", Speed = 300, Lvl = 0, Type = WeaponType.Other, Worth = 0, CurrentDurability = -1, MaxDurability = -1, MagicDamage = 1, Desc = "You found this glowing orb in an abandoned chocolate factory, it glows a tremendous light when you hold it up and... That's it, it was probably a discontinued toy off the line" };

        #endregion

        public static List<Weapon> weaponList = new List<Weapon>()
        {
            rogueDaggers,
            dragonSpear,
            stick
        };
    }

    public static class Spells
    {
        #region Static Spells

        public static Spell magesEnergy = new Spell { Name = "Mages' Energy", MagicDamage = 5, ManaCost = 0, Lvl = 0, Type = SpellType.Starter, Desc = "The spell that started them all, some might call it the 'Hello World' spell, it gets the job done and your grueling training means you can infinitely it.... cool!" };
        public static Spell boneSpike = new Spell { Name = "Necromancer Bone Spike", ManaCost = 0, PhysicalDamage = 3, WindDamage = 1, MagicDamage = 2, Type = SpellType.Starter, Desc = "A giant spike comes out of the ground with a 70% chance of hitting the genitals, what's more to like?" };
        public static Spell arcaneArmor = new Spell { Name = "Novice Arcane Armor", Type = SpellType.Defense, PhysicalDamage = 3, ManaCost = 2, MagicDamage = 5, Lvl = 1, Desc = "Thin hovering layers of pure arcane defense here to protect you, I think the warranty expired last week. Be Careful" };
        public static Spell dragonRage = new Spell { Name = "Novice Dragon Rage", Type = SpellType.Defense, Lvl = 1, Speed = 50, ManaCost = 5, PhysicalDamage = 3, FireDamage = 3, IceDamage = 3, LightningDamage = 3, MagicDamage = 3, WindDamage = 3, Desc = "You have learned to harness a dragon's rage and use it to fuel your body to whoop some major ass" };

        #endregion
    }

    public static class Armors
    {
        #region Static Armors

        public static Armor knightArmor = new Armor { Name = "Novice Knight Armor", Type = ArmorType.Heavy, Lvl = 1, Speed = 50, Worth = 100, MaxDurability = 20, CurrentDurability = 20, Physical = 100, Desc = "Some beatup old armor you found in the old shed out back, next to the bones of an old dog... what was it's name again?" };
        public static Armor mageRobe = new Armor { Name = "Novice Mages' Robe", Type = ArmorType.Light, CurrentDurability = 10, MaxDurability = 10, Speed = 150, Worth = 100, Magic = 100, Lvl = 1, Physical = 10, Desc = "These might be 'Robes' if you believe hard enough, go on, believe... I can wait" };
        public static Armor theiveGarb = new Armor { Name = "Novice Theives Garb", Type = ArmorType.Light, CurrentDurability = 15, Lvl = 1, MaxDurability = 15, Speed = 130, Worth = 100, Physical = 70, Magic = 30, Desc = "What better way to rock your first gear then to steal it, even if it was from old miss bitchface who is a blind amputee" };
        public static Armor undeadArmor = new Armor { Name = "Undead Armor", Type = ArmorType.Medium, CurrentDurability = 18, MaxDurability = 18, Lvl = 1, Speed = 80, Worth = 100, Physical = 85, Magic = 30, Desc = "Nothing weird here, you just picked up the bones from some dead people and strapped it to your body... they weren't using it anyway" };
        public static Armor dragonArmor = new Armor { Name = "Novice Dragon Hunter Armor", Type = ArmorType.Medium, Lvl = 1, MaxDurability = 20, CurrentDurability = 20, Speed = 100, Worth = 100, Physical = 80, Fire = 10, Ice = 10, Lightning = 10, Wind = 10, Desc = "Bad. Ass. Bad. Ass. Bad. Ass. Bad. Ass. - Naive thoughts running in your mind" };

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

    public class Testing
    {
        public static string line = Environment.NewLine;

        public static string RandomWeap()
        {
            var pickedLoot = (Weapon)LootDrop.PickLoot(testiculeesCharacter);
            return ($"{line}Name: {pickedLoot.Name}{line}Description: {pickedLoot.Desc}{line}Type: {pickedLoot.Type.ToString()}{line}Rarity: {pickedLoot.Rarity}{line}Level: {pickedLoot.Lvl}{line}Max Durability: {pickedLoot.MaxDurability}{line}Current Durability: {pickedLoot.CurrentDurability}{line}Worth: {pickedLoot.Worth}{line}Speed: {pickedLoot.Speed}{line}Physical Damage: {pickedLoot.PhysicalDamage}");
        }

        public static string RandomMassTest(int num)
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

            for (int i = 0; i <= num; i++)
            {
                var pickedLoot = (Weapon)LootDrop.PickLoot(testiculeesCharacter);
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
                if (!pickedLoot.Desc.ToLower().Contains("average")) unique++;
            }

               return ($"{line}sword = {sword}{line}dagger = {dagger}{line}greatsword = {greatsword}{line}katana = {katana}{line}staff = {staff}{line}focusStone = {focusStone}{line}spear = {spear}{line}dragonSpear = {dragonSpear}{line}twinSwords = {twinSwords}{line}other = {other}{line}starter = {starter}{line}unique = {unique}{line}------------------------------------------------{line}common = {common}{line}uncommon = {uncommon}{line}rare = {rare}{line}epic = {epic}{line}legendary = {legendary}{line}");
        }

        public static OwnerProfile testiculeesProfile = new OwnerProfile()
        {
            CurrentCharacter = testiculeesCharacter,
            CharacterList = new List<Character>() { testiculeesCharacter },
            Currency = 696969,
            BossesBeat = 101,
            OwnerID = 12345678910111213
        };

        public static Character testiculeesCharacter = new Character()
        {
            Name = "Testiculees teh Great",
            Class = CharacterClass.Warrior,
            Lvl = 1,
            OwnerID = testiculeesProfile.OwnerID,
            Armor = Armors.knightArmor,
            MaxHP = 12000000,
            CurrentHP = 12000000,
            Weapon = Weapons.warriorFists,
            Exp = 0
        };

    }

}
