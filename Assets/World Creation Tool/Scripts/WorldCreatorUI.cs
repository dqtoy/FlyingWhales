using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace worldcreator {
    public class WorldCreatorUI : MonoBehaviour {

        public static WorldCreatorUI Instance = null;

        public EventSystem eventSystem;

        [Header("World Generation")]
        [SerializeField] private GameObject mainMenuGO;
        [SerializeField] private GameObject gridSizeGO;
        [SerializeField] private Button generateBtn;
        [SerializeField] private InputField widthField;
        [SerializeField] private InputField heightField;
        [SerializeField] private GameObject loadingGO;
        [SerializeField] private Slider loadingSlider;
        [SerializeField] private Text loadingTxt;

        [Space(10)]
        [Header("Toolbar")]
        [SerializeField] private GameObject toolbarGO;

        [Space(10)]
        [Header("Edit Biome Menu")]
        [SerializeField] private GameObject editBiomeMenuGO;
        [SerializeField] private Dropdown biomesDropDown;

        [Space(10)]
        [Header("Edit Elevation Menu")]
        [SerializeField] private GameObject editElevationMenuGO;

        [Space(10)]
        [Header("Edit Faction Menu")]
        [SerializeField] private GameObject editFactionMenuGO;

        [Space(10)]
        [Header("Edit Regions Menu")]
        [SerializeField] private GameObject editRegionsMenuGO;

        [Space(10)]
        [Header("Edit Landmarks Menu")]
        [SerializeField] private GameObject editLandmarksMenuGO;

        private void Awake() {
            Instance = this;
        }

        #region Main Menu
        public void OnClickCreateWorld() {
            gridSizeGO.SetActive(true);
            mainMenuGO.SetActive(false);
        }
        public void OnClickGenerateGrid() {
            StartCoroutine(WorldCreatorManager.Instance.GenerateGrid(Int32.Parse(widthField.text), Int32.Parse(heightField.text)));
            generateBtn.interactable = false;
            widthField.interactable = false;
            heightField.interactable = false;
            //gridSizeGO.SetActive(false);
        }
        public void UpdateLoading(float progress, string text) {
            loadingTxt.text = text;
            loadingSlider.value = progress;
            if (!loadingGO.activeSelf) {
                loadingGO.SetActive(true);
            }
        }
        public void OnDoneLoadingGrid() {
            generateBtn.interactable = true;
            gridSizeGO.SetActive(false);
            toolbarGO.SetActive(true);
            OnClickEditBiomes();
            WorldCreatorManager.Instance.EnableSelection();
            CameraMove.Instance.CalculateCameraBounds();
        }
        #endregion

        #region Toolbar
        public void OnClickEditBiomes() {
            WorldCreatorManager.Instance.SetEditMode(EDIT_MODE.BIOME);
            editBiomeMenuGO.SetActive(true);
            editElevationMenuGO.SetActive(false);
            editFactionMenuGO.SetActive(false);
            editRegionsMenuGO.SetActive(false);
            editLandmarksMenuGO.SetActive(false);
        }
        public void OnClickEditElevation() {
            WorldCreatorManager.Instance.SetEditMode(EDIT_MODE.ELEVATION);
            editBiomeMenuGO.SetActive(false);
            editElevationMenuGO.SetActive(true);
            editFactionMenuGO.SetActive(false);
            editRegionsMenuGO.SetActive(false);
            editLandmarksMenuGO.SetActive(false);
        }
        public void OnClickEditRegions() {
            WorldCreatorManager.Instance.SetEditMode(EDIT_MODE.REGION);
            editBiomeMenuGO.SetActive(false);
            editElevationMenuGO.SetActive(false);
            editFactionMenuGO.SetActive(false);
            editRegionsMenuGO.SetActive(true);
            editLandmarksMenuGO.SetActive(false);
        }
        public void OnClickEditFactions() {
            WorldCreatorManager.Instance.SetEditMode(EDIT_MODE.FACTION);
            editBiomeMenuGO.SetActive(false);
            editElevationMenuGO.SetActive(false);
            editFactionMenuGO.SetActive(true);
            editRegionsMenuGO.SetActive(false);
            editLandmarksMenuGO.SetActive(false);
        }
        public void OnClickEditLandmarks() {
            WorldCreatorManager.Instance.SetEditMode(EDIT_MODE.LANDMARKS);
            editBiomeMenuGO.SetActive(false);
            editElevationMenuGO.SetActive(false);
            editFactionMenuGO.SetActive(false);
            editRegionsMenuGO.SetActive(false);
            editLandmarksMenuGO.SetActive(true);
        }
        #endregion

        #region Edit Biomes Menu
        public void OnClickEditBiomesApply() {
            BIOMES chosenBiome = (BIOMES) Enum.Parse(typeof(BIOMES), biomesDropDown.options[biomesDropDown.value].text);
            List<HexTile> selectedTiles = WorldCreatorManager.Instance.selectionComponent.selection;
            WorldCreatorManager.Instance.SetBiomes(selectedTiles, chosenBiome);
        }
        #endregion

        #region Utilities
        public bool IsMouseOnUI() {
            return eventSystem.IsPointerOverGameObject();
        }
        #endregion

    }
}
