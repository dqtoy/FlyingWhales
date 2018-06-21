using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace worldcreator {
    public class EditAreasMenu : MonoBehaviour {

        [SerializeField] private GameObject areaItemPrefab;
        [SerializeField] private Dropdown areaTypeDropdown;
        [SerializeField] private ScrollRect areasScrollView;
        [SerializeField] private Button createNewAreaBtn;

        public void HideMenu() {
            Messenger.RemoveListener<HexTile>(Signals.TILE_LEFT_CLICKED, CreateNewArea);
            createNewAreaBtn.interactable = true;
            UnhighlightAreas();
            this.gameObject.SetActive(false);
        }
        public void ShowMenu() {
            this.gameObject.SetActive(true);
        }

        #region Monobehaviours
        private void Awake() {
            LoadAreaTypeChoices();
            Messenger.AddListener<Area>(Signals.AREA_CREATED, OnAreaCreated);
        }
        private void Update() {
            if (this.gameObject.activeSelf) {
                HighlightAreas();
            }
        }
        #endregion

        public void StartNewAreaCreation() {
            Messenger.AddListener<HexTile>(Signals.TILE_LEFT_CLICKED, CreateNewArea);
            createNewAreaBtn.interactable = false;
            WorldCreatorUI.Instance.messageBox.ShowMessageBox(MESSAGE_BOX.OK, "New Area Creation", "Pick a core tile for the new area");
            WorldCreatorManager.Instance.selectionComponent.ClearSelectedTiles();
            //List<HexTile> validSelectedTiles = new List<HexTile>(WorldCreatorManager.Instance.selectionComponent.selection.Where(x => x.isPassable));
            //HexTile coreTile = validSelectedTiles[UnityEngine.Random.Range(0, validSelectedTiles.Count)];
            //AREA_TYPE chosenAreaType = (AREA_TYPE)Enum.Parse(typeof(AREA_TYPE), areaTypeDropdown.options[areaTypeDropdown.value].text);
            //LandmarkManager.Instance.CreateNewArea(coreTile, chosenAreaType, validSelectedTiles);
        }
        private void CreateNewArea(HexTile coreTile) {
            if (!coreTile.isPassable) {
                WorldCreatorUI.Instance.messageBox.ShowMessageBox(MESSAGE_BOX.OK, "Area Creation Error", "Core tile must be passable!");
                return;
            }
            if (coreTile.areaOfTile != null) {
                WorldCreatorUI.Instance.messageBox.ShowMessageBox(MESSAGE_BOX.OK, "Area Creation Error", "That tile is already part of an existing area!");
                return;
            }
            AREA_TYPE chosenAreaType = (AREA_TYPE)Enum.Parse(typeof(AREA_TYPE), areaTypeDropdown.options[areaTypeDropdown.value].text);
            LandmarkManager.Instance.CreateNewArea(coreTile, chosenAreaType);
            Messenger.RemoveListener<HexTile>(Signals.TILE_LEFT_CLICKED, CreateNewArea);
            createNewAreaBtn.interactable = true;
        }

        private void OnAreaCreated(Area createdArea) {
            GameObject areaItemGO = GameObject.Instantiate(areaItemPrefab, areasScrollView.content);
            AreaEditorItem areaItem = areaItemGO.GetComponent<AreaEditorItem>();
            areaItem.SetArea(createdArea);
        }
        private void LoadAreaTypeChoices() {
            areaTypeDropdown.ClearOptions();
            areaTypeDropdown.AddOptions(Utilities.GetEnumChoices<AREA_TYPE>());
        }

        private void HighlightAreas() {
            for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
                Area currArea = LandmarkManager.Instance.allAreas[i];
                currArea.HighlightArea();
            }
        }
        private void UnhighlightAreas() {
            for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
                Area currArea = LandmarkManager.Instance.allAreas[i];
                currArea.UnhighlightArea();
            }
        }
    }
}

