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

		[SerializeField] private GameObject goRemoveItem;

        internal ECS.Character currSelectedCharacter;
		internal Item currSelectedItem;

        private void Awake() {
            Instance = this;
        }

        private void Start() {
			CombatPrototypeManager.Instance.Initialize();
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
                if (i == 0) {
                    itemTypePopupList.value = itemTypes[i].ToString();
                }
            }
        }

        public void LoadItemChoices() {
            equipmentPopupList.Clear();
            equipmentPopupList.value = string.Empty;
            ITEM_TYPE currTypeChosen = (ITEM_TYPE)itemTypePopupList.data;
            List<string> items = GetAllItemsOfType(currTypeChosen);
            for (int i = 0; i < items.Count; i++) {
                equipmentPopupList.AddItem(items[i]);
                if (i == 0) {
                    equipmentPopupList.value = items[i];
                }
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
            ECS.Character newChar = CombatPrototypeManager.Instance.CreateNewCharacter((CharacterSetup)sideAPopupList.data);
            CombatPrototypeManager.Instance.combat.AddCharacter(SIDES.A, newChar);
            CombatPrototypeUI.Instance.SetCharacterAsSelected(newChar);
        }

        public void AddCharacterToSideB() {
            ECS.Character newChar = CombatPrototypeManager.Instance.CreateNewCharacter((CharacterSetup)sideBPopupList.data);
            CombatPrototypeManager.Instance.combat.AddCharacter(SIDES.B, newChar);
            CombatPrototypeUI.Instance.SetCharacterAsSelected(newChar);
        }

        public void UpdateCharactersList(SIDES side) {
            UILabel labelToUpdate = sideACharactersLbl;
            string sideText = "_sideA";
            if(side == SIDES.B) {
                labelToUpdate = sideBCharactersLbl;
                sideText = "_sideB";
            }
            List<ECS.Character> charactersFromSide = CombatPrototypeManager.Instance.combat.GetCharactersOnSide(side);
            labelToUpdate.text = string.Empty;
            for (int i = 0; i < charactersFromSide.Count; i++) {
                ECS.Character currCharacter = charactersFromSide[i];
				labelToUpdate.text += "[url=" + i.ToString() + sideText + "]" + currCharacter.name + "[/url]\n";
            }
        }

        public void UpdateCharacterSummary(ECS.Character character) {
            characterSummary.UpdateCharacterSummary(character);
        }
        public void UpdateCharacterSummary() {
            characterSummary.UpdateCharacterSummary(currSelectedCharacter);
        }


        public void AddCombatLog(string combatLog, SIDES side) {
			if(side == SIDES.B){
				combatSummaryLbl.text += "--------------";
			}
            combatSummaryLbl.text += "--" + combatLog + "\n\n";
            combatSummaryScrollView.UpdatePosition();
            combatSummaryScrollView.UpdateScrollbars();
        }

        public void ClearCombatLogs() {
            combatSummaryLbl.text = string.Empty;
            combatSummaryScrollView.ResetPosition();
            combatSummaryScrollView.UpdateScrollbars();
        }

        public void ResetSimulation() {
			while (CombatPrototypeManager.Instance.combat.charactersSideA.Count > 0) {
				CombatPrototypeManager.Instance.combat.charactersSideA [0].Death ();
			}
			while (CombatPrototypeManager.Instance.combat.charactersSideB.Count > 0) {
				CombatPrototypeManager.Instance.combat.charactersSideB [0].Death ();
			}
			CombatPrototypeManager.Instance.NewCombat ();
			ClearCombatLogs();
//            CombatPrototypeManager.Instance.combat.charactersSideA.Clear();
//            CombatPrototypeManager.Instance.combat.charactersSideB.Clear();
            UpdateCharactersList(SIDES.A);
            UpdateCharactersList(SIDES.B);
			ResetCurrentSelectedItem ();
        }

        public void SetCharacterAsSelected(ECS.Character character) {
			if(currSelectedCharacter != null && currSelectedCharacter != character){
				ResetCurrentSelectedItem ();
			}
            currSelectedCharacter = character;
            UpdateCharacterSummary(character);
        }
		public void SetItemAsSelected(Item item) {
			currSelectedItem = item;
			goRemoveItem.SetActive (true);
		}

        #region Equpment
        public void EquipItem() {
            string itemType = itemTypePopupList.value;
            string equipmentType = equipmentPopupList.value;
			currSelectedCharacter.EquipItem (itemType, equipmentType);
        }
        #endregion

		#region Levels
//		public void LevelUp(){
//			if(currSelectedCharacter != null){
//				currSelectedCharacter.IncreaseLevel ();
//				UpdateCharacterSummary (currSelectedCharacter);
//			}
//		}
//		public void LevelDown(){
//			if(currSelectedCharacter != null){
//				currSelectedCharacter.DecreaseLevel ();
//				UpdateCharacterSummary (currSelectedCharacter);
//			}
//		}
		#endregion

		public void OnClickRemoveItem(){
			if(currSelectedItem != null){
				currSelectedCharacter.UnequipItem (currSelectedItem);
				characterSummary.UpdateItemSummary (currSelectedCharacter);
				ResetCurrentSelectedItem ();
			}
		}

		public void ResetCurrentSelectedItem(){
			currSelectedItem = null;
			goRemoveItem.SetActive (false);
		}

    }
}
