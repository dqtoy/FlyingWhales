using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace worldcreator {
    public class EditLandmarksMenu : MonoBehaviour {
        [SerializeField] private Dropdown landmarksDropDown;
        [SerializeField] private Button destroyBtn;

        private List<BaseLandmark> lastCreatedLandmarks;

        private void Awake() {
            LoadLandmarksDropDown();
        }

        private void Update() {
            if (WorldCreatorManager.Instance.selectionComponent.selection.Count > 0) {
                destroyBtn.interactable = true;
            } else {
                destroyBtn.interactable = false;
            }
        }

        #region Edit Landmarks Menu
        private void LoadLandmarksDropDown() {
            LANDMARK_TYPE[] landmarkTypes = Utilities.GetEnumValues<LANDMARK_TYPE>();
            List<string> options = new List<string>();
            for (int i = 0; i < landmarkTypes.Length; i++) {
                LANDMARK_TYPE currType = landmarkTypes[i];
                if (currType == LANDMARK_TYPE.GARRISON || currType == LANDMARK_TYPE.HUMAN_HOUSES || currType == LANDMARK_TYPE.ELVEN_HOUSES) {
                    options.Add(currType.ToString());
                }
            }
            landmarksDropDown.AddOptions(options);
        }
        
        public void OnClickSpawnLandmark() {
            LANDMARK_TYPE chosenLandmarkType = (LANDMARK_TYPE)Enum.Parse(typeof(LANDMARK_TYPE), landmarksDropDown.options[landmarksDropDown.value].text);
            List<HexTile> selectedTiles = WorldCreatorManager.Instance.selectionComponent.selection;
            lastCreatedLandmarks = WorldCreatorManager.Instance.SpawnLandmark(selectedTiles, chosenLandmarkType);
            OnSpawnLandmark(chosenLandmarkType);
        }
        public void OnClickDestroyLandmark() {
            List<HexTile> selectedTiles = WorldCreatorManager.Instance.selectionComponent.selection;
            WorldCreatorManager.Instance.DestroyLandmarks(selectedTiles);
        }
        private void OnSpawnLandmark(LANDMARK_TYPE landmarkType) {
            switch (landmarkType) {
                case LANDMARK_TYPE.ELVEN_HOUSES:
                case LANDMARK_TYPE.HUMAN_HOUSES:
                    WorldCreatorUI.Instance.messageBox.ShowInputMessageBox("Input Civilians", "Input number of civilians (per settlement)", SetHousesCivilians);
                    break;
                default:
                    break;
            }
        }
        private void SetHousesCivilians(string civilians) {
            int civiliansCount = Int32.Parse(civilians);
            for (int i = 0; i < lastCreatedLandmarks.Count; i++) {
                Settlement currSettlement = lastCreatedLandmarks[i] as Settlement;
                currSettlement.SetCivilianCount(civiliansCount);
            }
            WorldCreatorUI.Instance.messageBox.HideMessageBox();
        }
        #endregion
    }
}