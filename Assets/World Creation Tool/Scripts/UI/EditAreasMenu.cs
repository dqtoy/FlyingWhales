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

        public AreaInfoEditor infoEditor;
        
        public void HideMenu() {
            if (Messenger.eventTable.ContainsKey(Signals.TILE_LEFT_CLICKED)) {
                Messenger.RemoveListener<HexTile>(Signals.TILE_LEFT_CLICKED, CreateNewArea);
            }
            createNewAreaBtn.interactable = true;
            this.gameObject.SetActive(false);
        }
        public void ShowMenu() {
            this.gameObject.SetActive(true);
        }
        public void Initialize() {
            LoadAreaTypeChoices();
            Messenger.AddListener<Area>(Signals.AREA_CREATED, OnAreaCreated);
            infoEditor.Initialize();
        }

        public void StartNewAreaCreation() {
            Messenger.AddListener<HexTile>(Signals.TILE_LEFT_CLICKED, CreateNewArea);
            createNewAreaBtn.interactable = false;
            WorldCreatorUI.Instance.messageBox.ShowMessageBox(MESSAGE_BOX.OK, "New Area Creation", "Pick a core tile for the new area");
            WorldCreatorManager.Instance.selectionComponent.ClearSelectedTiles();
            WorldCreatorManager.Instance.SetSelectionMode(SELECTION_MODE.TILE);
            //List<HexTile> validSelectedTiles = new List<HexTile>(WorldCreatorManager.Instance.selectionComponent.selection.Where(x => x.isPassable));
            //HexTile coreTile = validSelectedTiles[UnityEngine.Random.Range(0, validSelectedTiles.Count)];
            //AREA_TYPE chosenAreaType = (AREA_TYPE)Enum.Parse(typeof(AREA_TYPE), areaTypeDropdown.options[areaTypeDropdown.value].text);
            //LandmarkManager.Instance.CreateNewArea(coreTile, chosenAreaType, validSelectedTiles);
        }
        private void CreateNewArea(HexTile coreTile) {
            if (WorldCreatorManager.Instance.outerGridList.Contains(coreTile)) {
                WorldCreatorManager.Instance.selectionComponent.ClearSelectedTiles();
                WorldCreatorUI.Instance.messageBox.ShowMessageBox(MESSAGE_BOX.OK, "Area Creation Error", "Core tile must not be part of outer grid!");
                return;
            }
            if (coreTile.areaOfTile != null) {
                WorldCreatorManager.Instance.selectionComponent.ClearSelectedTiles();
                WorldCreatorUI.Instance.messageBox.ShowMessageBox(MESSAGE_BOX.OK, "Area Creation Error", "That tile is already part of an existing area!");
                return;
            }
            AREA_TYPE chosenAreaType = (AREA_TYPE)Enum.Parse(typeof(AREA_TYPE), areaTypeDropdown.options[areaTypeDropdown.value].text);
            LandmarkManager.Instance.CreateNewArea(coreTile, chosenAreaType, 0);
            Messenger.RemoveListener<HexTile>(Signals.TILE_LEFT_CLICKED, CreateNewArea);
            createNewAreaBtn.interactable = true;
        }

        private void OnAreaCreated(Area createdArea) {
            GameObject areaItemGO = GameObject.Instantiate(areaItemPrefab, areasScrollView.content);
            AreaEditorItem areaItem = areaItemGO.GetComponent<AreaEditorItem>();
            areaItem.Initialize();
            areaItem.SetArea(createdArea);
        }
        private void LoadAreaTypeChoices() {
            areaTypeDropdown.ClearOptions();
            areaTypeDropdown.AddOptions(Utilities.GetEnumChoices<AREA_TYPE>());
        }

        //public void UpdateAreaInfo(Area area) {
        //    AreaEditorItem[] item = Utilities.GetComponentsInDirectChildren<AreaEditorItem>(areasScrollView.content.gameObject);
        //    for (int i = 0; i < item.Length; i++) {
        //        AreaEditorItem currItem = item[i];
        //        if (currItem.area.id == area.id) {
        //            currItem.UpdateInfo();
        //            break;
        //        }
        //    }
        //}
    }
}

