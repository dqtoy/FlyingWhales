using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace worldcreator {
    public class CharacterEditorItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

        private Character _character;

        [SerializeField] private Text characterNameLbl;
        [SerializeField] private Button editBtn;
        [SerializeField] private Button setLocationBtn;
        [SerializeField] private Button setHomeBtn;

        #region getters/setters
        public Character character {
            get { return _character; }
        }
        #endregion

        public void SetCharacter(Character character) {
            _character = character;
            UpdateInfo();
        }

        private void UpdateInfo() {
            characterNameLbl.text = _character.name;
        }
        public void SetEditAction(UnityAction action) {
            editBtn.onClick.RemoveAllListeners();
            editBtn.onClick.AddListener(action);
        }
        public void DeleteCharacter() {
            CharacterManager.Instance.RemoveCharacter(_character);
        }
        public void SetLocation() {
            List<HexTile> choices = new List<HexTile>(WorldCreatorManager.Instance.selectionComponent.nonOuterSelection);
            HexTile chosenTile = choices[Random.Range(0, choices.Count)];
            if (chosenTile.areaOfTile != null) {
                chosenTile.areaOfTile.AddCharacterToLocation(_character.party, null, true);
                //_character.party.SetSpecificLocation(chosenTile.landmarkOnTile);
            } 
            //else {
            //    _character.party.SetSpecificLocation(chosenTile);
            //}
            if (WorldCreatorUI.Instance.editCharactersMenu.characterInfoEditor.gameObject.activeSelf) {
                WorldCreatorUI.Instance.editCharactersMenu.characterInfoEditor.UpdateBasicInfo();
            }
        }
        public void SetHome() {
            List<HexTile> choices = new List<HexTile>(WorldCreatorManager.Instance.selectionComponent.nonOuterSelection);
            //for (int i = 0; i < WorldCreatorManager.Instance.selectionComponent.selection.Count; i++) {
            //    HexTile currTile = WorldCreatorManager.Instance.selectionComponent.selection[i];
            //    if (currTile.areaOfTile != null && !choices.Contains(currTile.areaOfTile)) {
            //        choices.Add(currTile.areaOfTile);
            //    }
            //}
            HexTile chosenTile = choices[Random.Range(0, choices.Count)];
            //_character.SetHome(chosenTile.areaOfTile);
            if (chosenTile.areaOfTile != null) {
                _character.MigrateHomeTo(chosenTile.areaOfTile);
                //chosenTile.areaOfTile.AddResident(_character, true);
            }
            //BaseLandmark chosenLandmark = choices[Random.Range(0, choices.Count)];
            //_character.SetHome(chosenLandmark);
            if (WorldCreatorUI.Instance.editCharactersMenu.characterInfoEditor.gameObject.activeSelf) {
                WorldCreatorUI.Instance.editCharactersMenu.characterInfoEditor.UpdateBasicInfo();
            }
        }

        #region Monobehaviours
        private void Update() {
            if (WorldCreatorManager.Instance.selectionComponent.selectedLandmarks.Count == 0) {
                setHomeBtn.interactable = false;
            } else {
                setHomeBtn.interactable = true;
            }

            if (WorldCreatorManager.Instance.selectionComponent.nonOuterSelection.Count == 0) {
                setLocationBtn.interactable = false;
            } else {
                setLocationBtn.interactable = true;
            }
            UpdateInfo();
        }
        #endregion

        #region Events
        public void OnPointerEnter(PointerEventData eventData) {
            worldcreator.WorldCreatorUI.Instance.ShowSmallCharacterInfo(_character);
        }
        public void OnPointerExit(PointerEventData eventData) {
            worldcreator.WorldCreatorUI.Instance.HideSmallCharacterInfo();
        }
        #endregion
    }
}

