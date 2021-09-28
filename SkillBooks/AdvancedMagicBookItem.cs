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

namespace SkillBooks
{
    public class AdvancedMagicBook : DaggerfallUnityItem
    {
        static PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;

        public const int templateIndex = 554;

        internal const string NAME = "Tablet of Arcane Knowledge";

        static DaggerfallUnityItem book = null;


        public AdvancedMagicBook() : base(ItemGroups.UselessItems2, templateIndex)
        {
            message = 13;
        }

        public override string ItemName
        {
            get { return NAME; }
        }

        public override bool UseItem(ItemCollection collection)
        {
            if (GameManager.Instance.AreEnemiesNearby(true))
            {
                DaggerfallUI.MessageBox("There are enemies nearby.");
                return false;
            }
            else if (playerEntity.Stats.LiveIntelligence <= 69)
            {
                DaggerfallUI.MessageBox("Your intelligence is too low to learn anything from this book.");
                return false;
            }
            else if (playerEntity.CurrentFatigue <= PlayerEntity.DefaultFatigueLoss * 90)
            {
                DaggerfallUI.MessageBox("You do not have the stamina to study.");
                return false;
            }
            else if (playerEntity.CurrentMagicka <= playerEntity.MaxMagicka / 4)
            {
                DaggerfallUI.MessageBox("You do not have the magical energies to study.");
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
            data.className = typeof(AdvancedMagicBook).ToString();
            return data;
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
