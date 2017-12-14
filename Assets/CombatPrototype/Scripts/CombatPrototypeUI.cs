using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class CombatPrototypeUI : MonoBehaviour {

        public static CombatPrototypeUI Instance = null;

        [SerializeField] private UIPopupList sideAPopupList;
        [SerializeField] private UILabel sideACharactersLbl;

        [SerializeField] private UIPopupList sideBPopupList;
        [SerializeField] private UILabel sideBCharactersLbl;

        [SerializeField] private CharacterSummary characterSummary;

        [SerializeField] private UILabel combatSummaryLbl;
        [SerializeField] private UIScrollView combatSummaryScrollView;

        [SerializeField] private UIPopupList itemTypePopupList;
        [SerializeField] private UIPopupList equipmentPopupList;

        internal List<string> resultsLog;

        private void Awake() {
            Instance = this;
        }

        private void Start() {
			this.resultsLog = new List<string> ();
            LoadCharacterChoices();
            LoadItemTypeChoices();
        }

        private void LoadCharacterChoices() {
            sideAPopupList.Clear();
            sideBPopupList.Clear();
            for (int i = 0; i < CombatPrototypeManager.Instance.baseCharacters.Length; i++) {
                CharacterSetup currChoice = CombatPrototypeManager.Instance.baseCharacters[i];
                sideAPopupList.AddItem(currChoice.fileName, currChoice);
                sideBPopupList.AddItem(currChoice.fileName, currChoice);
                if (i == 0) {
                    sideAPopupList.value = currChoice.fileName;
                    sideBPopupList.value = currChoice.fileName;
                }
            }
        }
        private void LoadItemTypeChoices() {
            ITEM_TYPE[] itemTypes = Utilities.GetEnumValues<ITEM_TYPE>();
            for (int i = 0; i < itemTypes.Length; i++) {
                itemTypePopupList.AddItem(itemTypes[i].ToString(), itemTypes[i]);
            }
        }

        public void LoadItemChoices() {
            ITEM_TYPE currTypeChosen = (ITEM_TYPE)itemTypePopupList.data;
            List<string> items = GetAllItemsOfType(currTypeChosen);
            for (int i = 0; i < items.Count; i++) {
                equipmentPopupList.AddItem(items[i]);
            }
        }

        private List<string> GetAllItemsOfType(ITEM_TYPE itemType) {
            List<string> allItemsOfType = new List<string>();
            string path = "Assets/CombatPrototype/Data/Items/" + itemType.ToString() + "/";
            foreach (string file in System.IO.Directory.GetFiles(path, "*.json")) {
                allItemsOfType.Add(System.IO.Path.GetFileNameWithoutExtension(file));
            }
            return allItemsOfType;
        }

        public void AddCharacterToSideA() {
            Character newChar = CombatPrototypeManager.Instance.CreateNewCharacter((CharacterSetup)sideAPopupList.data);
            CombatPrototype.Instance.AddCharacter(SIDES.A, newChar);
        }

        public void AddCharacterToSideB() {
            Character newChar = CombatPrototypeManager.Instance.CreateNewCharacter((CharacterSetup)sideBPopupList.data);
            CombatPrototype.Instance.AddCharacter(SIDES.B, newChar);
        }

        public void UpdateCharactersList(SIDES side) {
            UILabel labelToUpdate = sideACharactersLbl;
            string sideText = "_sideA";
            if(side == SIDES.B) {
                labelToUpdate = sideBCharactersLbl;
                sideText = "_sideB";
            }
            List<Character> charactersFromSide = CombatPrototype.Instance.GetCharactersOnSide(side);
            labelToUpdate.text = string.Empty;
            for (int i = 0; i < charactersFromSide.Count; i++) {
                Character currCharacter = charactersFromSide[i];
                labelToUpdate.text += "[url=" + i.ToString() + sideText + "]" + currCharacter.name + "[/url]\n";
            }
        }

        public void UpdateCharacterSummary(Character character) {
            characterSummary.UpdateCharacterSummary(character);
        }

        public void AddCombatLog(string combatLog) {
            resultsLog.Add(combatLog);
            combatSummaryLbl.text += "--" + combatLog + "\n\n";
            combatSummaryScrollView.ResetPosition();
            combatSummaryScrollView.UpdateScrollbars();
        }

        public void ClearCombatLogs() {
            resultsLog.Clear();
            combatSummaryLbl.text = string.Empty;
            combatSummaryScrollView.ResetPosition();
            combatSummaryScrollView.UpdateScrollbars();
        }

        public void ResetSimulation() {
            ClearCombatLogs();
            CombatPrototype.Instance.charactersSideA.Clear();
            CombatPrototype.Instance.charactersSideB.Clear();
            UpdateCharactersList(SIDES.A);
            UpdateCharactersList(SIDES.B);
        }
    }
}
