// Project:         Skill Books mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2021 Ralzar
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Ralzar

using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using UnityEngine;
using System;
using Wenzil.Console;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop;
using System.Collections.Generic;
using DaggerfallConnect;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using System.Linq;

namespace SkillBooks
{
    public class SkillBooks : MonoBehaviour
    {
        static Mod mod;

        static PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;

        public static bool ReadPrompt = false;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            var go = new GameObject(mod.Title);
            go.AddComponent<SkillBooks>();
            ModSettings settings = mod.GetSettings();

            ReadPrompt = settings.GetValue<bool>("General", "ReadingPrompt");

            DaggerfallUnity.Instance.ItemHelper.RegisterCustomItem(BasicSkillBook.templateIndex, ItemGroups.UselessItems2, typeof(BasicSkillBook));
            DaggerfallUnity.Instance.ItemHelper.RegisterCustomItem(AdvancedSkillBook.templateIndex, ItemGroups.UselessItems2, typeof(AdvancedSkillBook));
            DaggerfallUnity.Instance.ItemHelper.RegisterCustomItem(BasicMagicBook.templateIndex, ItemGroups.UselessItems2, typeof(BasicMagicBook));
            DaggerfallUnity.Instance.ItemHelper.RegisterCustomItem(AdvancedMagicBook.templateIndex, ItemGroups.UselessItems2, typeof(AdvancedMagicBook));

            PlayerActivate.OnLootSpawned += AddSkillBooks_OnLootSpawned;
            EnemyDeath.OnEnemyDeath += SkillBookLoot_OnEnemyDeath;
        }

        void Awake()
        {
            mod.IsReady = true;
            Debug.Log("[SkillBooks] Ready");
        }

        void Start()
        {
            RegisterSBCommands();
        }

        public static void ReadingBook(DaggerfallUnityItem book)
        {
            List<DFCareer.Skills> skillsToTrain = GetSkillList(book);
            List<string> skillsTrained = new List<string>();
            DaggerfallDateTime now = DaggerfallUnity.Instance.WorldTime.Now;
            bool advancedBook = (book.TemplateIndex == AdvancedSkillBook.templateIndex || book.TemplateIndex == AdvancedMagicBook.templateIndex);
            bool tooAdvanced = true;
            foreach (DFCareer.Skills skill in skillsToTrain)
            {
                int skillMax = 0;
                int skillMin = 0;

                if (playerEntity.GetPrimarySkills().Contains(skill))
                    skillMax += 70;
                else if (playerEntity.GetMajorSkills().Contains(skill))
                    skillMax += 60;
                else if (playerEntity.GetMinorSkills().Contains(skill))
                    skillMax += 50;
                else
                    skillMax += 40;

                if (advancedBook)
                {
                    skillMax += 20;
                    skillMin += 40;
                }

                if (playerEntity.Skills.GetLiveSkillValue(skill) < skillMax && playerEntity.Skills.GetLiveSkillValue(skill) > skillMin)
                {
                    tooAdvanced = false;
                    TrainSkill(skill, advancedBook);
                    skillsTrained.Add(DaggerfallUnity.Instance.TextProvider.GetSkillName(skill));
                }
            }

            if (tooAdvanced)
                DaggerfallUI.MessageBox("This book is too advanced for you.");
            else
            {
                now.RaiseTime(DaggerfallDateTime.SecondsPerHour);
                int skillsCount = skillsTrained.Count;
                if (book.TemplateIndex == 553 || book.TemplateIndex == 554)
                    playerEntity.CurrentMagicka -= playerEntity.MaxMagicka / 4;
                playerEntity.DecreaseFatigue(PlayerEntity.DefaultFatigueLoss * 90);
                book.currentCondition -= 1;
                if (book.currentCondition <= 0)
                {
                    playerEntity.Items.RemoveItem(book);
                    if (book.TemplateIndex == 554)
                        DaggerfallUI.MessageBox("The tablet cracks, and the magic energies within fades away.");
                    else if (book.TemplateIndex == 553)
                        DaggerfallUI.MessageBox("The tomes have become so worn you are no longer able to decipher them.");
                    else
                        DaggerfallUI.MessageBox("The book is so old and worn it has become unreadable.");
                }
                else if (skillsCount == 3)
                    DaggerfallUI.MessageBox("You learn more about the skills " + skillsTrained[0] + ", " + skillsTrained[1] + " and " + skillsTrained[2] + ".");
                else if (skillsCount == 2)
                    DaggerfallUI.MessageBox("You learn more about the skills " + skillsTrained[0] + " and " + skillsTrained[1] + ".");
                else if (skillsCount == 1)
                    DaggerfallUI.MessageBox("You learn more about the skill " + skillsTrained[0] + ".");
                else
                    DaggerfallUI.MessageBox("You spend an hour reading, but learn nothing new.");
            }
        }

        static void TrainSkill(DFCareer.Skills skill, bool advancedBook)
        {
            int intMod = playerEntity.Stats.LiveIntelligence / 10;
            int bookMax = advancedBook ? 5 : 0;
            int skillAdvancementMultiplier = DaggerfallSkills.GetAdvancementMultiplier(skill);
            short tallyAmount = (short)(UnityEngine.Random.Range(intMod - 1, intMod + bookMax) * skillAdvancementMultiplier);
            playerEntity.TallySkill(skill, tallyAmount);
        }

        static List<DFCareer.Skills> GetSkillList(DaggerfallUnityItem item)
        {
            List<DFCareer.Skills> list = new List<DFCareer.Skills>();
            switch (item.message)
            {
                case 236548927:
                    list.Add(DFCareer.Skills.Axe);
                    list.Add(DFCareer.Skills.BluntWeapon);
                    list.Add(DFCareer.Skills.LongBlade);
                    return list;
                case 1:
                    list.Add(DFCareer.Skills.HandToHand);
                    list.Add(DFCareer.Skills.ShortBlade);
                    list.Add(DFCareer.Skills.Backstabbing);
                    return list;
                case 2:
                    list.Add(DFCareer.Skills.Archery);
                    list.Add(DFCareer.Skills.Stealth);
                    return list;
                case 3:
                    list.Add(DFCareer.Skills.Jumping);
                    list.Add(DFCareer.Skills.Swimming);
                    list.Add(DFCareer.Skills.Climbing);
                    return list;
                case 4:
                    list.Add(DFCareer.Skills.Medical);
                    list.Add(DFCareer.Skills.CriticalStrike);
                    return list;
                case 5:
                    list.Add(DFCareer.Skills.Etiquette);
                    return list;
                case 6:
                    list.Add(DFCareer.Skills.Streetwise);
                    return list;
                case 7:
                    list.Add(DFCareer.Skills.Mercantile);
                    return list;
                case 8:
                    list.Add(DFCareer.Skills.Centaurian);
                    list.Add(DFCareer.Skills.Spriggan);
                    list.Add(DFCareer.Skills.Nymph);
                    return list;
                case 9:
                    list.Add(DFCareer.Skills.Dragonish);
                    list.Add(DFCareer.Skills.Harpy);
                    list.Add(DFCareer.Skills.Giantish);
                    return list;
                case 10:
                    list.Add(DFCareer.Skills.Orcish);
                    return list;
                case 11:
                    list.Add(DFCareer.Skills.Daedric);
                    list.Add(DFCareer.Skills.Impish);
                    return list;
                case 12:
                    list.Add(DFCareer.Skills.Lockpicking);
                    return list;
                case 13:
                    list.AddRange(GetThreeRandomMagicSkills());
                    return list;
                default:
                    return list;
            }
        }

        static IEnumerable<DFCareer.Skills> GetMagicSkills()
        {
            yield return DFCareer.Skills.Alteration;
            yield return DFCareer.Skills.Destruction;
            yield return DFCareer.Skills.Illusion;
            yield return DFCareer.Skills.Mysticism;
            yield return DFCareer.Skills.Restoration;
            yield return DFCareer.Skills.Thaumaturgy;
        }

        static IEnumerable<DFCareer.Skills> GetThreeRandomMagicSkills()
        {
            System.Random rng = new System.Random();
            return GetMagicSkills().OrderBy(_ => rng.Next()).Take(3);
        }


        static void SkillBookLoot_OnEnemyDeath(object sender, EventArgs e)
        {
            EnemyDeath enemyDeath = sender as EnemyDeath;
            if (enemyDeath != null)
            {
                DaggerfallEntityBehaviour entityBehaviour = enemyDeath.GetComponent<DaggerfallEntityBehaviour>();
                if (entityBehaviour != null)
                {
                    EnemyEntity enemyEntity = entityBehaviour.Entity as EnemyEntity;
                    if (enemyEntity != null)
                    {
                        if (enemyEntity.MobileEnemy.Affinity == MobileAffinity.Human || HumanoidCheck(enemyEntity.MobileEnemy.ID))
                        {
                            int luckRoll = UnityEngine.Random.Range(1, 30) + (playerEntity.Stats.LiveLuck / 10);
                            int index = 0;
                            bool caster = enemyEntity.MobileEnemy.ID > 127 && enemyEntity.MobileEnemy.ID < 132;
                            if (caster)
                                luckRoll += 5;
                            if (luckRoll > 30)
                            {
                                int roll = UnityEngine.Random.Range(1, 20);
                                if (caster)
                                    roll += 5;
                                if (roll < 17)
                                {
                                    index = 552;
                                }
                                else
                                {
                                    index = 554;
                                }
                            }
                            else if (luckRoll > 28)
                            {
                                int roll = UnityEngine.Random.Range(1, 20);
                                if (caster)
                                    roll += 5;
                                if (roll < 17)
                                {
                                    index = 551;
                                }
                                else
                                {
                                    index = 553;
                                }
                            }
                            if (index > 550)
                            {
                                DaggerfallUnityItem skillBook = ItemBuilder.CreateItem(ItemGroups.UselessItems2, index);
                                skillBook.currentCondition /= UnityEngine.Random.Range(1, 5);
                                entityBehaviour.CorpseLootContainer.Items.AddItem(skillBook);
                            }
                        }
                    }
                }
            }
        }

        static bool HumanoidCheck(int enemyID)
        {
            switch (enemyID)
            {
                case (int)MobileTypes.Orc:
                case (int)MobileTypes.Centaur:
                case (int)MobileTypes.OrcSergeant:
                case (int)MobileTypes.Giant:
                case (int)MobileTypes.OrcShaman:
                case (int)MobileTypes.OrcWarlord:
                    return true;
            }
            return false;
        }

        public static void AddSkillBooks_OnLootSpawned(object sender, ContainerLootSpawnedEventArgs e)
        {

            DaggerfallInterior interior = GameManager.Instance.PlayerEnterExit.Interior;
            if (interior != null &&
                e.ContainerType == LootContainerTypes.ShopShelves &&
                interior.BuildingData.BuildingType == DFLocation.BuildingTypes.Bookseller)
            {
                int quality = 2;
                switch (interior.BuildingData.Quality)
                {
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                        quality = 3;
                        break;
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                        quality = 4;
                        break;
                    case 13:
                    case 14:
                    case 15:
                    case 16:
                        quality = 5;
                        break;
                    case 17:
                    case 18:
                    case 19:
                    case 20:
                        quality = 6;
                        break;
                }

                int numBooks = UnityEngine.Random.Range(0, quality);
                while (numBooks > 0)
                {
                    int roll = UnityEngine.Random.Range(1, 101);
                    int index = 551;

                    if (roll > 95)
                    {
                        index = 554; //tablet
                    }
                    else if (roll > 85)
                    {
                        index = 553; //mTomes
                    }
                    else if (roll > 60)
                    {
                        index = 552; //advBook
                    }
                    DaggerfallUnityItem skillBook = ItemBuilder.CreateItem(ItemGroups.UselessItems2, index);
                    e.Loot.AddItem(skillBook);
                    numBooks--;
                }
            }
        }

        public static void RegisterSBCommands()
        {
            Debug.Log("[SkillBooks] Trying to register console commands.");
            try
            {
                ConsoleCommandsDatabase.RegisterCommand(AddBasicSkillBook.command, AddBasicSkillBook.description, AddBasicSkillBook.usage, AddBasicSkillBook.Execute);
                ConsoleCommandsDatabase.RegisterCommand(AddAdvSkillBook.command, AddAdvSkillBook.description, AddAdvSkillBook.usage, AddAdvSkillBook.Execute);
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("Error Registering SkillBooks Console commands: {0}", e.Message));
            }
        }

        private static class AddBasicSkillBook
        {
            public static readonly string command = "add_BasicSkillBook";
            public static readonly string description = "Put basic skillbook in players inventory";
            public static readonly string usage = "add_BasicSkillBook (a number 0-13)";

            public static string Execute(params string[] args)
            {
                int value = -1;
                if (args.Length < 1)
                    return usage;
                if (!int.TryParse(args[0], out value))
                    return string.Format("Could not parse argument `{0}` to a number", args[0]);
                if (value < 0 || value > 13)
                    return string.Format("Index {0} is out range. Must be 0-13.", value);
                if (args == null || args.Length < 1 || args.Length > 1)
                    return usage;
                if (args.Length == 1 && value < 14)
                {
                    int index = 553;
                    if (value != 13)
                    {
                        index = 551;
                        BasicSkillBook.messageSet = value;
                    }
                    DaggerfallUnityItem skillBook = ItemBuilder.CreateItem(ItemGroups.UselessItems2, index);
                    GameManager.Instance.PlayerEntity.Items.AddItem(skillBook);
                }
                return "SkillBook added";
            }
        }

        private static class AddAdvSkillBook
        {
            public static readonly string command = "add_AdvSkillBook";
            public static readonly string description = "Put basic skillbook in players inventory";
            public static readonly string usage = "add_AdvSkillBook (a number 0-13)";

            public static string Execute(params string[] args)
            {
                int value = -1;
                if (args.Length < 1)
                    return usage;
                if (!int.TryParse(args[0], out value))
                    return string.Format("Could not parse argument `{0}` to a number", args[0]);
                if (value < 0 || value > 13)
                    return string.Format("Index {0} is out range. Must be 0-13.", value);
                if (args == null || args.Length < 1 || args.Length > 1)
                    return usage;
                else if (args.Length == 1 && value < 14)
                {
                    int index = 554;
                    if (value != 13)
                    {
                        index = 552;
                        AdvancedSkillBook.messageSet = value;                        
                    }
                    DaggerfallUnityItem skillBook = ItemBuilder.CreateItem(ItemGroups.UselessItems2, index);
                    GameManager.Instance.PlayerEntity.Items.AddItem(skillBook);
                }
                return "SkillBook added";
            }
        }
    }
}