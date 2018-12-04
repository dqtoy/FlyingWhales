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
        public Canvas canvas;

        public const float toolbarShowingPos = -480f;
        public const float toolbarHiddenPos = -599.95f;

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
        [SerializeField] private EditLandmarksMenu _editLandmarksMenu;
        [SerializeField] private LandmarkInfoEditor _landmarkInfoEditor;

        [Space(10)]
        [Header("Edit Characters Menu")]
        [SerializeField] private GameObject editCharactersMenuGO;
        [SerializeField] private EditCharactersMenu _editCharactersMenu;

        [Space(10)]
        [Header("Edit Areas Menu")]
        [SerializeField] private GameObject editAreasMenuGO;
        [SerializeField] private EditAreasMenu _editAreasMenu;

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

        [Space(10)]
        [Header("Small Character Info")]
        [SerializeField] private SmallCharacterInfo smallCharacterInfo;

        [Space(10)]
        [Header("Character Portrait Editor")]
        [SerializeField] private CharacterPortraitEditor portraitEditor;

        [Space(10)]
        [Header("Tile Info")]
        [SerializeField] private TileInfoUI tileInfo;

        [Space(10)]
        [Header("Context Menu")]
        public GameObject contextMenuPrefab;
        public GameObject contextMenuItemPrefab;
        public UIContextMenu contextMenu;

        [Space(10)]
        [Header("Character Items")]
        public CharacterItemsMenu characterItemsMenu;

        #region getters/setters
        public EditRegionsMenu editRegionsMenu {
            get { return _editRegionsMenu; }
        }
        public EditFactionsMenu editFactionsMenu {
            get { return _editFactionsMenu; }
        }
        public EditLandmarksMenu editLandmarksMenu {
            get { return _editLandmarksMenu; }
        }
        public EditCharactersMenu editCharactersMenu {
            get { return _editCharactersMenu; }
        }
        public EditAreasMenu editAreasMenu {
            get { return _editAreasMenu; }
        }
        public MessageBox messageBox {
            get { return _messageBox; }
        }
        public LandmarkInfoEditor landmarkInfoEditor {
            get { return _landmarkInfoEditor; }
        }
        #endregion

        private void Awake() {
            Instance = this;
            LoadSaveFiles();
            Messenger.AddListener<HexTile>(Signals.TILE_RIGHT_CLICKED, ShowContextMenu);
            Messenger.AddListener<HexTile>(Signals.TILE_LEFT_CLICKED, HideContextMenu);
        }
        private void Update() {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                HideContextMenu();
            }
        }

        public void InitializeMenus() {
            characterItemsMenu.Initialize();
            editCharactersMenu.Initialize();
            tileInfo.Initialize();
            editAreasMenu.Initialize();
            landmarkInfoEditor.Initialize();
        }

        #region Main Menu
        public void OnClickCreateWorld() {
            gridSizeGO.SetActive(true);
            mainMenuGO.SetActive(false);
            portraitEditor.HideMenu();
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
            editCharactersMenuGO.SetActive(false);
            _editAreasMenu.HideMenu();
            landmarkInfoEditor.CloseMenu();

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
            editCharactersMenuGO.SetActive(false);
            _editAreasMenu.HideMenu();
            landmarkInfoEditor.CloseMenu();

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
            _editAreasMenu.HideMenu();
            landmarkInfoEditor.CloseMenu();

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
            editCharactersMenuGO.SetActive(false);
            _editAreasMenu.HideMenu();
            editFactionsMenu.HideFactionInfo();
            landmarkInfoEditor.CloseMenu();

            tileSelectionBtn.interactable = true;
            rectangleSelectionBtn.interactable = true;
            regionSelectionBtn.interactable = true;
            //regionSelectionBtn.isOn = true;
        }
        public void OnClickEditLandmarks() {
            WorldCreatorManager.Instance.SetEditMode(EDIT_MODE.LANDMARKS);
            editBiomeMenuGO.SetActive(false);
            editElevationMenuGO.SetActive(false);
            editFactionMenuGO.SetActive(false);
            editRegionsMenuGO.SetActive(false);
            editLandmarksMenuGO.SetActive(true);
            editCharactersMenuGO.SetActive(false);
            _editAreasMenu.HideMenu();
            landmarkInfoEditor.CloseMenu();

            rectangleSelectionBtn.interactable = true;
            regionSelectionBtn.interactable = true;
            tileSelectionBtn.interactable = true;
        }
        public void OnClickEditCharacters() {
            editBiomeMenuGO.SetActive(false);
            editElevationMenuGO.SetActive(false);
            editFactionMenuGO.SetActive(false);
            editRegionsMenuGO.SetActive(false);
            editLandmarksMenuGO.SetActive(false);
            editCharactersMenuGO.SetActive(true);
            if (editCharactersMenu.characterInfoEditor.gameObject.activeSelf) {
                editCharactersMenu.characterInfoEditor.UpdateInfo();
            }
            _editAreasMenu.HideMenu();
            landmarkInfoEditor.CloseMenu();

            rectangleSelectionBtn.interactable = true;
            regionSelectionBtn.interactable = true;
            tileSelectionBtn.interactable = true;
        }
        public void OnClickEditAreas() {
            editBiomeMenuGO.SetActive(false);
            editElevationMenuGO.SetActive(false);
            editFactionMenuGO.SetActive(false);
            editRegionsMenuGO.SetActive(false);
            editLandmarksMenuGO.SetActive(false);
            editCharactersMenuGO.SetActive(false);
            _editAreasMenu.ShowMenu();
            landmarkInfoEditor.CloseMenu();

            rectangleSelectionBtn.interactable = true;
            regionSelectionBtn.interactable = true;
            tileSelectionBtn.interactable = true;
        }
        public void OnClickEditSquads() {
            editBiomeMenuGO.SetActive(false);
            editElevationMenuGO.SetActive(false);
            editFactionMenuGO.SetActive(false);
            editRegionsMenuGO.SetActive(false);
            editLandmarksMenuGO.SetActive(false);
            editCharactersMenuGO.SetActive(false);
            _editAreasMenu.HideMenu();
            landmarkInfoEditor.CloseMenu();

            rectangleSelectionBtn.interactable = true;
            regionSelectionBtn.interactable = true;
            tileSelectionBtn.interactable = true;
        }
        public void OnClickEditStories() {
            editBiomeMenuGO.SetActive(false);
            editElevationMenuGO.SetActive(false);
            editFactionMenuGO.SetActive(false);
            editRegionsMenuGO.SetActive(false);
            editLandmarksMenuGO.SetActive(false);
            editCharactersMenuGO.SetActive(false);
            _editAreasMenu.HideMenu();
            landmarkInfoEditor.CloseMenu();

            rectangleSelectionBtn.interactable = false;
            regionSelectionBtn.interactable = false;
            tileSelectionBtn.interactable = false;
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
            portraitEditor.HideMenu();
            WorldCreatorManager.Instance.LoadWorld(fileName);
        }
        #endregion

        #region Small Character Info
        public void ShowSmallCharacterInfo(Character character) {
            smallCharacterInfo.ShowCharacterInfo(character);
        }
        public void HideSmallCharacterInfo() {
            smallCharacterInfo.HideSmallCharacterInfo();
        }
        #endregion

        public void OnRegionDeleted(Region deletedRegion) {
            editRegionsMenu.OnRegionDeleted(deletedRegion);
            editFactionsMenu.OnRegionDeleted(deletedRegion);
        }

        #region Portrait Editor
        public void ShowPortraitEditor() {
            portraitEditor.ShowMenu();
        }
        #endregion

        #region Character Editors
        public void OnPortraitTemplatesChanged() {
            editCharactersMenu.OnPortraitTemplatesChanged();
        }
        #endregion

        #region Context Menu
        private void ShowContextMenu(HexTile tile) {
            ContextMenuSettings settings = tile.GetContextMenuSettings();
            if (settings.items.Count > 0) {
                contextMenu.LoadSettings(settings);
                contextMenu.gameObject.SetActive(true);
                //Vector2 pos;
                //RectTransformUtility.ScreenPointToLocalPointInRectangle(this.transform as RectTransform, Input.mousePosition, Camera.main, out pos);

                contextMenu.transform.position = Input.mousePosition;
            }
            //Vector2 newPos;
            //RectTransformUtility.ScreenPointToLocalPointInRectangle(this.transform as RectTransform, Input.mousePosition, CameraMove.Instance.uiCamera, out newPos);
            //contextMenu.transform.position = CameraMove.Instance.uiCamera.Sc;
            //GameObject contextMenuGO = GameObject.Instantiate(contextMenuPrefab, this.transform);
            //contextMenuGO.GetComponent<UIContextMenu>().LoadSettings(settings);
        }
        public void HideContextMenu() {
            contextMenu.gameObject.SetActive(false);
        }
        public void HideContextMenu(HexTile tile) {
            HideContextMenu();
        }
        #endregion

        #region Utilities
        public bool IsMouseOnUI() {
            return eventSystem.IsPointerOverGameObject();
        }
        public bool IsUserOnUI() {
            if (eventSystem.currentSelectedGameObject != null) {
                if (eventSystem.currentSelectedGameObject.GetComponent<InputField>() != null) {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Landmark Info Editor
        public void ShowLandmarkInfoEditor(BaseLandmark landmark) {
            OnClickEditLandmarks();
            landmarkInfoEditor.ShowLandmarkInfo(landmark);
        }
        #endregion
    }
}
