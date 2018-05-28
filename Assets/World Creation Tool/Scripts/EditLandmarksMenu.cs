using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace worldcreator {
    public class EditLandmarksMenu : MonoBehaviour {
        [SerializeField] private Dropdown landmarksDropDown;
        [SerializeField] private Button destroyBtn;

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
                options.Add(landmarkTypes[i].ToString());
            }
            landmarksDropDown.AddOptions(options);
        }
        public void OnClickSpawnLandmark() {
            LANDMARK_TYPE chosenLandmarkType = (LANDMARK_TYPE)Enum.Parse(typeof(LANDMARK_TYPE), landmarksDropDown.options[landmarksDropDown.value].text);
            List<HexTile> selectedTiles = WorldCreatorManager.Instance.selectionComponent.selection;
            WorldCreatorManager.Instance.SpawnLandmark(selectedTiles, chosenLandmarkType);
        }
        public void OnClickDestroyLandmark() {
            List<HexTile> selectedTiles = WorldCreatorManager.Instance.selectionComponent.selection;
            WorldCreatorManager.Instance.DestroyLandmarks(selectedTiles);
        }
        #endregion
    }
}