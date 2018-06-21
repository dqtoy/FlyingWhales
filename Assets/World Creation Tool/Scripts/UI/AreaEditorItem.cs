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
        private void Awake() {
            LoadAreaTypeChoices();
            Messenger.AddListener<Area>(Signals.AREA_TILE_ADDED, UpdateInfo);
            Messenger.AddListener<Area>(Signals.AREA_TILE_REMOVED, UpdateInfo);
        }
        #endregion

        public void SetArea(Area area) {
            _area = area;
            UpdateInfo(area);
        }
        private void UpdateInfo(Area area) {
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
        }

        #region Dropdown Data
        private void LoadAreaTypeChoices() {
            areaTypeDropdown.ClearOptions();
            areaTypeDropdown.AddOptions(Utilities.GetEnumChoices<AREA_TYPE>());
        }
        #endregion

        public void AddTilesToArea() {
            List<HexTile> validSelectedTiles = new List<HexTile>(WorldCreatorManager.Instance.selectionComponent.selection
                .Where(x => x.isPassable && x.areaOfTile == null));
            _area.AddTile(validSelectedTiles);
            //UpdateInfo();
            WorldCreatorManager.Instance.selectionComponent.ClearSelectedTiles();
        }

        public void RemoveTilesFromArea() {
            List<HexTile> validSelectedTiles = new List<HexTile>(WorldCreatorManager.Instance.selectionComponent.selection
                .Where(x => x.areaOfTile != null && x.areaOfTile.id == _area.id && _area.coreTile.id != x.id));
            _area.RemoveTile(validSelectedTiles);
            //UpdateInfo();
            WorldCreatorManager.Instance.selectionComponent.ClearSelectedTiles();
        }

        public void DeleteArea() {
            LandmarkManager.Instance.RemoveArea(_area);
            _area.UnhighlightArea();
            while (_area.tiles.Count != 0) {
                _area.RemoveTile(_area.tiles[0], false);
            }
            GameObject.Destroy(this.gameObject);
        }
        //#region UI Events
        //public void OnPointerEnter(PointerEventData eventData) {
        //    _area.HighlightArea();
        //}
        //public void OnPointerExit(PointerEventData eventData) {
        //    _area.UnhighlightArea();
        //}
        //#endregion

    }
}
