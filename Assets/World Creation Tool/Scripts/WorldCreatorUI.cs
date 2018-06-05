using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace worldcreator {
    public class WorldCreatorUI : MonoBehaviour {

        public static WorldCreatorUI Instance = null;

        public EventSystem eventSystem;

        public const float toolbarShowingPos = -493.6f;
        public const float toolbarHiddenPos = -588f;

        [Header("World Generation")]
        [SerializeField] private GameObject mainMenuGO;
        [SerializeField] private GameObject gridSizeGO;
        [SerializeField] private Button generateBtn;
        [SerializeField] private Button goBackToMainBtn;
        [SerializeField] private InputField widthField;
        [SerializeField] private InputField heightField;
        [SerializeField] private Toggle randomizeWorldToggle;
        [SerializeField] private GameObject loadingGO;
        [SerializeField] private Slider loadingSlider;
        [SerializeField] private Text loadingTxt;

        [Space(10)]
        [Header("Toolbar")]
        [SerializeField] private GameObject toolbarGO;
        [SerializeField] private Toggle rectangleSelectionBtn;
        [SerializeField] private Toggle tileSelectionBtn;
        [SerializeField] private Toggle regionSelectionBtn;

        [Space(10)]
        [Header("Edit Biome Menu")]
        [SerializeField] private GameObject editBiomeMenuGO;
        [SerializeField] private Dropdown biomesDropDown;

        [Space(10)]
        [Header("Edit Elevation Menu")]
        [SerializeField] private GameObject editElevationMenuGO;
        [SerializeField] private Dropdown elevationDropDown;

        [Space(10)]
        [Header("Edit Faction Menu")]
        [SerializeField] private GameObject editFactionMenuGO;
        [SerializeField] private EditFactionsMenu _editFactionsMenu;

        [Space(10)]
        [Header("Edit Regions Menu")]
        [SerializeField] private GameObject editRegionsMenuGO;
        [SerializeField] private EditRegionsMenu _editRegionsMenu;

        [Space(10)]
        [Header("Edit Landmarks Menu")]
        [SerializeField] private GameObject editLandmarksMenuGO;
        [SerializeField] private EditLandmarksMenu editLandmarksMenu;

        [Space(10)]
        [Header("Saving")]
        [SerializeField] private GameObject saveMenuGO;
        [SerializeField] private GameObject saveItemPrefab;
        [SerializeField] private GameObject saveFieldsGO;
        [SerializeField] private ScrollRect savesScrollView;
        [SerializeField] private InputField saveNameField;

        [Space(10)]
        [Header("Message Box")]
        [SerializeField] private MessageBox _messageBox;

        #region getters/setters
        public EditRegionsMenu editRegionsMenu {
            get { return _editRegionsMenu; }
        }
        public EditFactionsMenu editFactionsMenu {
            get { return _editFactionsMenu; }
        }
        public MessageBox messageBox {
            get { return _messageBox; }
        }
        #endregion

        private void Awake() {
            Instance = this;
            LoadSaveFiles();
        }

        #region Main Menu
        public void OnClickCreateWorld() {
            gridSizeGO.SetActive(true);
            mainMenuGO.SetActive(false);
        }
        public void OnClickEditWorld() {
            ShowLoadingMenu();
        }
        public void OnClickBackToStart() {
            gridSizeGO.SetActive(false);
            mainMenuGO.SetActive(true);
        }
        public void OnClickGenerateGrid() {
            StartCoroutine(WorldCreatorManager.Instance.GenerateGrid(Int32.Parse(widthField.text), Int32.Parse(heightField.text), randomizeWorldToggle.isOn));
            randomizeWorldToggle.interactable = false;
            generateBtn.interactable = false;
            widthField.interactable = false;
            heightField.interactable = false;
            goBackToMainBtn.interactable = false;
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
            loadingGO.SetActive(false);
            OnClickEditBiomes();
            WorldCreatorManager.Instance.EnableSelection();
            CameraMove.Instance.CalculateCameraBounds();
        }
        #endregion

        #region Toolbar
        public void ShowHideToolbar() {
            if (Mathf.Approximately(toolbarGO.transform.localPosition.y, toolbarShowingPos)) {
                //Hide
                Vector3 newPos = toolbarGO.transform.localPosition;
                newPos.y = toolbarHiddenPos;
                toolbarGO.transform.localPosition = newPos;
            } else {
                //Show
                Vector3 newPos = toolbarGO.transform.localPosition;
                newPos.y = toolbarShowingPos;
                toolbarGO.transform.localPosition = newPos;
            }
        }
        public void OnClickEditBiomes() {
            WorldCreatorManager.Instance.SetEditMode(EDIT_MODE.BIOME);
            editBiomeMenuGO.SetActive(true);
            editElevationMenuGO.SetActive(false);
            editFactionMenuGO.SetActive(false);
            editRegionsMenuGO.SetActive(false);
            editLandmarksMenuGO.SetActive(false);

            rectangleSelectionBtn.interactable = true;
            regionSelectionBtn.interactable = true;
            tileSelectionBtn.interactable = true;
        }
        public void OnClickEditElevation() {
            WorldCreatorManager.Instance.SetEditMode(EDIT_MODE.ELEVATION);
            editBiomeMenuGO.SetActive(false);
            editElevationMenuGO.SetActive(true);
            editFactionMenuGO.SetActive(false);
            editRegionsMenuGO.SetActive(false);
            editLandmarksMenuGO.SetActive(false);

            rectangleSelectionBtn.interactable = true;
            regionSelectionBtn.interactable = true;
            tileSelectionBtn.interactable = true;
        }
        public void OnClickEditRegions() {
            WorldCreatorManager.Instance.SetEditMode(EDIT_MODE.REGION);
            editBiomeMenuGO.SetActive(false);
            editElevationMenuGO.SetActive(false);
            editFactionMenuGO.SetActive(false);
            editRegionsMenuGO.SetActive(true);
            editLandmarksMenuGO.SetActive(false);

            rectangleSelectionBtn.interactable = true;
            //rectangleSelectionBtn.isOn = true;
            regionSelectionBtn.interactable = false;
            tileSelectionBtn.interactable = true;
        }
        public void OnClickEditFactions() {
            WorldCreatorManager.Instance.SetEditMode(EDIT_MODE.FACTION);
            editBiomeMenuGO.SetActive(false);
            editElevationMenuGO.SetActive(false);
            editFactionMenuGO.SetActive(true);
            editRegionsMenuGO.SetActive(false);
            editLandmarksMenuGO.SetActive(false);

            tileSelectionBtn.interactable = false;
            rectangleSelectionBtn.interactable = false;
            regionSelectionBtn.interactable = true;
            regionSelectionBtn.isOn = true;
        }
        public void OnClickEditLandmarks() {
            WorldCreatorManager.Instance.SetEditMode(EDIT_MODE.LANDMARKS);
            editBiomeMenuGO.SetActive(false);
            editElevationMenuGO.SetActive(false);
            editFactionMenuGO.SetActive(false);
            editRegionsMenuGO.SetActive(false);
            editLandmarksMenuGO.SetActive(true);

            rectangleSelectionBtn.interactable = true;
            regionSelectionBtn.interactable = true;
            tileSelectionBtn.interactable = true;
        }

        public void OnClickRectangleSelection() {
            WorldCreatorManager.Instance.SetSelectionMode(SELECTION_MODE.RECTANGLE);
        }
        public void OnClickTileSelection() {
            WorldCreatorManager.Instance.SetSelectionMode(SELECTION_MODE.TILE);
        }
        public void OnClickRegionSelection() {
            WorldCreatorManager.Instance.SetSelectionMode(SELECTION_MODE.REGION);
        }
        #endregion

        #region Edit Biomes Menu
        public void OnClickEditBiomesApply() {
            BIOMES chosenBiome = (BIOMES) Enum.Parse(typeof(BIOMES), biomesDropDown.options[biomesDropDown.value].text);
            List<HexTile> selectedTiles = WorldCreatorManager.Instance.selectionComponent.selection;
            WorldCreatorManager.Instance.SetBiomes(selectedTiles, chosenBiome);
            WorldCreatorManager.Instance.selectionComponent.ClearSelectedTiles();
        }
        #endregion

        #region Edit Elevation Menu
        public void OnClickEditElevationApply() {
            ELEVATION chosenElevation = (ELEVATION)Enum.Parse(typeof(ELEVATION), elevationDropDown.options[elevationDropDown.value].text);
            List<HexTile> selectedTiles = WorldCreatorManager.Instance.selectionComponent.selection;
            WorldCreatorManager.Instance.SetElevation(selectedTiles, chosenElevation);
            WorldCreatorManager.Instance.selectionComponent.ClearSelectedTiles();
        }
        #endregion

        #region Saving
        private void LoadSaveFiles() {
            Directory.CreateDirectory(Utilities.worldConfigsSavePath);
            DirectoryInfo info = new DirectoryInfo(Utilities.worldConfigsSavePath);
            FileInfo[] files = info.GetFiles();
            for (int i = 0; i < files.Length; i++) {
                FileInfo currInfo = files[i];
                if (currInfo.Name.Contains(Utilities.worldConfigFileExt)) {
                    GameObject saveItemGO = GameObject.Instantiate(saveItemPrefab, savesScrollView.content.transform);
                    saveItemGO.GetComponent<SaveItem>().SetSave(currInfo.Name);
                }
            }
        }
        public void OnFileSaved(string newFile) {
            if (!HasSaveEntry(newFile)) {
                GameObject saveItemGO = GameObject.Instantiate(saveItemPrefab, savesScrollView.content.transform);
                SaveItem item = saveItemGO.GetComponent<SaveItem>();
                item.SetSave(newFile);
                item.UpdateFormat(false);
            }
        }
        public void ShowSavesMenu() {
            saveMenuGO.SetActive(true);
            saveFieldsGO.SetActive(true);
            SaveItem[] saves = Utilities.GetComponentsInDirectChildren<SaveItem>(savesScrollView.content.gameObject);
            for (int i = 0; i < saves.Length; i++) {
                saves[i].UpdateFormat(false);
            }
        }
        public void HideSaveMenu() {
            saveMenuGO.SetActive(false);
        }
        public void ShowSaveConfirmation(string fileName = "") {
            if (string.IsNullOrEmpty(fileName)) {
                fileName = saveNameField.text + Utilities.worldConfigFileExt;
            }
            if (Utilities.DoesFileExist(Utilities.worldConfigsSavePath + fileName)) {
                UnityAction yesAction = new UnityAction(() => WorldCreatorManager.Instance.SaveWorld(fileName));
                _messageBox.ShowMessageBox(MESSAGE_BOX.YES_NO, "Overwrite save", "Do you want to overwrite save file " + fileName + "?", yesAction);
            } else {
                WorldCreatorManager.Instance.SaveWorld(fileName);
            }
        }
        private bool HasSaveEntry(string saveName) {
            SaveItem[] saves = Utilities.GetComponentsInDirectChildren<SaveItem>(savesScrollView.content.gameObject);
            for (int i = 0; i < saves.Length; i++) {
                if (saves[i].saveName.Equals(saveName)) {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Loading
        public void ShowLoadingMenu() {
            saveMenuGO.SetActive(true);
            saveFieldsGO.SetActive(false);
            SaveItem[] saves = Utilities.GetComponentsInDirectChildren<SaveItem>(savesScrollView.content.gameObject);
            for (int i = 0; i < saves.Length; i++) {
                saves[i].UpdateFormat(true);
            }
        }
        public void ShowLoadConfirmation(string fileName) {
            UnityAction yesAction = new UnityAction(() => OnConfirmLoad(fileName));
            _messageBox.ShowMessageBox(MESSAGE_BOX.YES_NO, "Load save", "Do you want to load save file " + fileName + "?", yesAction);
        }
        private void OnConfirmLoad(string fileName) {
            HideSaveMenu();
            mainMenuGO.SetActive(false);
            WorldCreatorManager.Instance.LoadWorld(fileName);
        }
        #endregion

        public void OnRegionDeleted(Region deletedRegion) {
            editRegionsMenu.OnRegionDeleted(deletedRegion);
            editFactionsMenu.OnRegionDeleted(deletedRegion);
        }

        #region Utilities
        public bool IsMouseOnUI() {
            return eventSystem.IsPointerOverGameObject();
        }
        #endregion

    }
}
