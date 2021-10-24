// Project:         Skill Books mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2021 Ralzar
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Ralzar

using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using System.Collections.Generic;

namespace SkillBooks
{


    public class AdvancedSkillBook : DaggerfallUnityItem
    {
        static PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;

        public const int templateIndex = 552;
        public static int messageSet = -1;

        internal const string NAME = "Encyclopaedica Tamriellica";

        static DaggerfallUnityItem book = null;
        public static List<string> bookTitlesList = new List<string>
        {
            "The Knights Martial Techniques",
            "The Crouching Fist and the Hidden Blade",
            "The Art Of The Hunt",
            "The Anatomy of the Athlete",
            "Physicians Lexicon",
            "A Treatise on the Mannerisms of the Courts of the Illiac Bay",
            "Diary of a Footpad",
            "The Theory of Supply and Demand",
            "Languages of the Woodland Creatures vol 2",
            "Languages Of The Mountain Races vol 2",
            "Dining in Orsinium : Common Orcish Phrasebook",
            "Infernal Tongues vol 2",
            "The Locksmiths Index of Lock Variations"
        };


        public AdvancedSkillBook() : base(ItemGroups.UselessItems2, templateIndex)
        {
            if (messageSet >= 0)
                message = messageSet;            
            else
                message = Random.Range(0, 13);
            messageSet = -1;

            if (Random.Range(1, 3) > 1)
                dyeColor = DyeColors.Red;

            shortName = GetSkillBookName(message);
        }

        public override string ItemName
        {
            get { return GetSkillBookName(message); }
        }

        public override string LongName
        {
            get { return GetSkillBookName(message); }
        }


        public override bool UseItem(ItemCollection collection)
        {
            if (GameManager.Instance.AreEnemiesNearby(true))
            {
                DaggerfallUI.MessageBox("There are enemies nearby.");
                return false;
            }
            else if (playerEntity.Stats.LiveIntelligence <= 49)
            {
                DaggerfallUI.MessageBox("Your intelligence is too low to learn anything from this book.");
                return false;
            }
            else if (playerEntity.CurrentFatigue <= PlayerEntity.DefaultFatigueLoss * 90)
            {
                DaggerfallUI.MessageBox("You do not have the stamina to study.");
                return false;
            }

            book = this;

            if (SkillBooks.ReadPrompt)
            {
                DaggerfallMessageBox skillBookPopUp = new DaggerfallMessageBox(DaggerfallUI.UIManager, DaggerfallUI.UIManager.TopWindow);
                string[] message = { "Do you wish to spend an hour studying?" };
                skillBookPopUp.SetText(message);
                skillBookPopUp.OnButtonClick += SkillBookPopup_OnButtonClick;
                skillBookPopUp.AddButton(DaggerfallMessageBox.MessageBoxButtons.Yes);
                skillBookPopUp.AddButton(DaggerfallMessageBox.MessageBoxButtons.No, true);
                skillBookPopUp.AllowCancel = true;
                skillBookPopUp.ClickAnywhereToClose = true;
                skillBookPopUp.Show();
            }
            else
                SkillBooks.ReadingBook(book);

            return true;
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(AdvancedSkillBook).ToString();
            return data;
        }

        private static string GetSkillBookName(int message)
        {
            return bookTitlesList[message];
        }

        private static void SkillBookPopup_OnButtonClick(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton)
        {
            if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.Yes)
            {
                SkillBooks.ReadingBook(book);
            }
            else
                sender.CloseWindow();
        } 
    }
}
