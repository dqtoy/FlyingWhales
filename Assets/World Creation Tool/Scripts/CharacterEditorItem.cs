using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace worldcreator {
    public class CharacterEditorItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

        private ECS.Character _character;

        [SerializeField] private Text characterNameLbl;
        [SerializeField] private Button editBtn;
        [SerializeField] private Button setLocationBtn;
        [SerializeField] private Button setHomeBtn;

        #region getters/setters
        public ECS.Character character {
            get { return _character; }
        }
        #endregion

        public void SetCharacter(ECS.Character character) {
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
            List<BaseLandmark> choices = new List<BaseLandmark>(WorldCreatorManager.Instance.selectionComponent.selectedLandmarks);
            BaseLandmark chosenLandmark = choices[Random.Range(0, choices.Count)];
            _character.SetSpecificLocation(chosenLandmark);
            WorldCreatorUI.Instance.editCharactersMenu.characterInfoEditor.UpdateBasicInfo();
        }
        public void SetHome() {
            List<BaseLandmark> choices = new List<BaseLandmark>(WorldCreatorManager.Instance.selectionComponent.selectedLandmarks);
            BaseLandmark chosenLandmark = choices[Random.Range(0, choices.Count)];
            _character.SetHome(chosenLandmark);
            WorldCreatorUI.Instance.editCharactersMenu.characterInfoEditor.UpdateBasicInfo();
        }

        #region Monobehaviours
        private void Update() {
            if (WorldCreatorManager.Instance.selectionComponent.selectedLandmarks.Count == 0) {
                setLocationBtn.interactable = false;
                setHomeBtn.interactable = false;
            } else {
                setLocationBtn.interactable = true;
                setHomeBtn.interactable = true;
            }
            
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

