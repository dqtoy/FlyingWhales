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

        private void Awake() {
            Instance = this;
        }

        private void Start() {
            LoadCharacterChoices();
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
    }
}
