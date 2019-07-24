using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace worldcreator {
    public class AreaEditorItem : MonoBehaviour {

        private Area _area;

        [SerializeField] private Text areaNameLbl;
        [SerializeField] private Text areaTilesLbl;
        [SerializeField] private Dropdown areaTypeDropdown;
        [SerializeField] private Image areaColorSwatch;

        #region getters/setters
        public Area area {
            get { return _area; }
        }
        #endregion

        #region Monobehaviours
        private void Update() {
            areaNameLbl.text = area.name;
        }
        #endregion

        public void Initialize() {
            LoadAreaTypeChoices();
            Messenger.AddListener<Area, HexTile>(Signals.AREA_TILE_ADDED, UpdateInfo);
            Messenger.AddListener<Area, HexTile>(Signals.AREA_TILE_REMOVED, UpdateInfo);
        }
        public void SetArea(Area area) {
            _area = area;
            UpdateInfo(area);
        }
        private void UpdateInfo(Area area) {
            areaColorSwatch.color = area.areaColor;
            areaNameLbl.text = area.name;
            areaTilesLbl.text = area.tiles.Count.ToString();
            areaTypeDropdown.value = Utilities.GetOptionIndex(areaTypeDropdown, area.areaType.ToString());
        }
        private void UpdateInfo(Area area, HexTile tile) {
            if (_area.id == area.id) {
                areaColorSwatch.color = area.areaColor;
                areaNameLbl.text = area.name;
                areaTilesLbl.text = area.tiles.Count.ToString();
                areaTypeDropdown.value = Utilities.GetOptionIndex(areaTypeDropdown, area.areaType.ToString());
            }
        }

        public void OnChangeAreaTypeDropdownValue(int choice) {
            AREA_TYPE areaType = (AREA_TYPE)Enum.Parse(typeof(AREA_TYPE), areaTypeDropdown.options[choice].text);
            _area.SetAreaType(areaType);
            //ValidateLandmarks();
        }

        #region Dropdown Data
        private void LoadAreaTypeChoices() {
            areaTypeDropdown.ClearOptions();
            areaTypeDropdown.AddOptions(Utilities.GetEnumChoices<AREA_TYPE>());
        }
        #endregion

        public void AddTilesToArea() {
            //AreaData data = LandmarkManager.Instance.GetAreaData(_area.areaType);
            List<HexTile> validSelectedTiles = new List<HexTile>();
            for (int i = 0; i < WorldCreatorManager.Instance.selectionComponent.selection.Count; i++) {
                HexTile currTile = WorldCreatorManager.Instance.selectionComponent.selection[i];
                if (WorldCreatorManager.Instance.outerGridList.Contains(currTile)) {
                    continue; //skip, the current tile is part of the outer grid
                }
                //if (!currTile.isPassable || currTile.areaOfTile != null) {
                //    continue;//skip
                //}
                //if (currTile.landmarkOnTile != null && !data.allowedLandmarkTypes.Contains(currTile.landmarkOnTile.specificLandmarkType)) {
                //    continue;//skip
                //}
                validSelectedTiles.Add(currTile);
            }
            _area.AddTile(validSelectedTiles);
            //UpdateInfo();
            WorldCreatorManager.Instance.selectionComponent.ClearSelectedTiles();
        }
        public void RemoveTilesFromArea() {
            List<HexTile> validSelectedTiles = new List<HexTile>(WorldCreatorManager.Instance.selectionComponent.selection
                .Where(x => x.areaOfTile != null && x.areaOfTile.id == _area.id));
            _area.RemoveTile(validSelectedTiles);
            //UpdateInfo();
            WorldCreatorManager.Instance.selectionComponent.ClearSelectedTiles();
        }
        public void DeleteArea() {
            LandmarkManager.Instance.RemoveArea(_area);
            _area.UnhighlightArea();
            while (_area.tiles.Count != 0) {
                _area.RemoveTile(_area.tiles[0]);
            }
            GameObject.Destroy(this.gameObject);
        }
        public void EditArea() {
            WorldCreatorUI.Instance.editAreasMenu.infoEditor.Show(_area);
        }

        #region Utilities
        //private void ValidateLandmarks() {
        //    AreaData data = LandmarkManager.Instance.GetAreaData(_area.areaType);
        //    List<BaseLandmark> invalidLandmarks = new List<BaseLandmark>();
        //    for (int i = 0; i < _area.landmarks.Count; i++) {
        //        BaseLandmark landmark = _area.landmarks[i];
        //        if (!data.allowedLandmarkTypes.Contains(landmark.specificLandmarkType)) {
        //            invalidLandmarks.Add(landmark);
        //        }
        //    }

        //    for (int i = 0; i < invalidLandmarks.Count; i++) {
        //        LandmarkManager.Instance.DestroyLandmarkOnTile(invalidLandmarks[i].tileLocation);
        //    }
        //}
        #endregion
    }
}
