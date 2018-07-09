using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class UIManager : MonoBehaviour {

    public static UIManager Instance = null;

    public delegate void OnAddNewBattleLog();
    public OnAddNewBattleLog onAddNewBattleLog;

    public Camera uiCamera;
    [SerializeField] private EventSystem eventSystem;

    [SerializeField] UIMenu[] allMenus;

    [Space(10)]
    [Header("Prefabs")]
    [SerializeField] private GameObject notificationPrefab;

    [Space(10)]
    [Header("Main UI Objects")]
    [SerializeField] private GameObject mainUIGO;


    [Space(10)]
    [Header("Date Objects")]
    [SerializeField] private ToggleGroup speedToggleGroup;
    [SerializeField] private Toggle pauseBtn;
    [SerializeField] private Toggle x1Btn;
    [SerializeField] private Toggle x2Btn;
    [SerializeField] private Toggle x4Btn;
    [SerializeField] private TextMeshProUGUI dateLbl;

    [Space(10)]
    [Header("Small Info")]
    public GameObject smallInfoGO;
    public TextMeshProUGUI smallInfoLbl;

    [Space(10)]
    [Header("World Info Menu")]
    [SerializeField] private GameObject worldInfoCharactersSelectedGO;
    [SerializeField] private GameObject worldInfoQuestsSelectedGO;
    [SerializeField] private GameObject worldInfoStorylinesSelectedGO;
    [SerializeField] private GameObject worldInfoCharactersBtn;
    [SerializeField] private GameObject worldInfoQuestsBtn;
    [SerializeField] private GameObject worldInfoStorylinesBtn;

    [Space(10)]
    [Header("Popup Message Box")]
    [SerializeField] private PopupMessageBox popupMessageBox;

    [Space(10)]
    [Header("Notification Area")]
    [SerializeField] private PlayerNotificationArea notificationArea;

    [Space(10)]
    [Header("Character Dialog Menu")]
    [SerializeField] private CharacterDialogMenu characterDialogMenu;

    [Space(10)] //FOR TESTING
    [Header("For Testing")]
    public ButtonToggle toggleBordersBtn;
    public ButtonToggle corruptionBtn;

    public delegate void OnPauseEventExpiration(bool state);
    public OnPauseEventExpiration onPauseEventExpiration;

    [Space(10)]
    [Header("Font Sizes")]
    [SerializeField] private int HEADER_FONT_SIZE = 25;
    [SerializeField] private int BODY_FONT_SIZE = 20;
    [SerializeField] private int TOOLTIP_FONT_SIZE = 18;
    [SerializeField] private int SMALLEST_FONT_SIZE = 12;

    internal List<object> eventLogsQueue = new List<object>();

    private List<UIMenuSettings> _menuHistory;

    #region getters/setters
    //internal GameObject minimapTexture {
    //    get { return minimapTextureGO; }
    //}
    internal List<UIMenuSettings> menuHistory {
        get { return _menuHistory; }
    }
    #endregion

    #region Monobehaviours
    private void Awake() {
        Instance = this;
        _menuHistory = new List<UIMenuSettings>();
        Messenger.AddListener<bool>(Signals.PAUSED, UpdateSpeedToggles);
    }
    private void Start() {
        Messenger.AddListener(Signals.UPDATE_UI, UpdateUI);
        NormalizeFontSizes();
        ToggleBorders();
    }
    private void Update() {
        if (Input.GetKeyDown(KeyCode.BackQuote)) {
            if (GameManager.Instance.allowConsole) {
                ToggleConsole();
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (contextMenu.gameObject.activeSelf) {
                HideContextMenu();
            }
        }
        UpdateSpeedToggles(GameManager.Instance.isPaused);
    }
    #endregion

    public void SetTimeControlsState(bool state) {
        pauseBtn.interactable = state;
        x1Btn.interactable = state;
        x2Btn.interactable = state;
        x4Btn.interactable = state;
    }

    internal void InitializeUI() {
        for (int i = 0; i < allMenus.Length; i++) {
            allMenus[i].Initialize();
        }
        popupMessageBox.Initialize();
        Messenger.AddListener<HexTile>(Signals.TILE_RIGHT_CLICKED, ShowContextMenu);
        Messenger.AddListener<HexTile>(Signals.TILE_LEFT_CLICKED, HideContextMenu);
        Messenger.AddListener<string, int, UnityAction>(Signals.SHOW_NOTIFICATION, ShowNotification);
    }

    #region Font Utilities
    private void NormalizeFontSizes() {
        TextMeshProUGUI[] allLabels = this.GetComponentsInChildren<TextMeshProUGUI>(true);
        //Debug.Log ("ALL LABELS COUNT: " + allLabels.Length.ToString());
        for (int i = 0; i < allLabels.Length; i++) {
            NormalizeFontSizeOfLabel(allLabels[i]);
        }
    }
    private void NormalizeFontSizeOfLabel(TextMeshProUGUI lbl) {
        string lblName = lbl.name;

        TextOverflowModes overflowMethod = TextOverflowModes.Truncate;
        if (lblName.Contains("HEADER")) {
            lbl.fontSize = HEADER_FONT_SIZE;
            overflowMethod = TextOverflowModes.Truncate;
        } else if (lblName.Contains("BODY")) {
            lbl.fontSize = BODY_FONT_SIZE;
            overflowMethod = TextOverflowModes.Truncate;
        } else if (lblName.Contains("TOOLTIP")) {
            lbl.fontSize = TOOLTIP_FONT_SIZE;
            overflowMethod = TextOverflowModes.Overflow;
        } else if (lblName.Contains("SMALLEST")) {
            lbl.fontSize = SMALLEST_FONT_SIZE;
            overflowMethod = TextOverflowModes.Truncate;
        }

        if (!lblName.Contains("NO")) {
            lbl.overflowMode = overflowMethod;
        }

    }
    #endregion

    private void UpdateUI() {
        dateLbl.SetText(GameManager.Instance.days.ToString() + " " + LocalizationManager.Instance.GetLocalizedValue("General", "Months", ((MONTH)GameManager.Instance.month).ToString()) + ", " + GameManager.Instance.year.ToString()
            + " (Tick " + GameManager.Instance.hour + ")");
    }

    #region World Controls
    private void UpdateSpeedToggles(bool isPaused) {
        if (isPaused) {
            pauseBtn.isOn = true;
            speedToggleGroup.NotifyToggleOn(pauseBtn);
        } else {
            if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X1) {
                x1Btn.isOn = true;
                speedToggleGroup.NotifyToggleOn(x1Btn);
            } else if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X2) {
                x2Btn.isOn = true;
                speedToggleGroup.NotifyToggleOn(x2Btn);
            } else if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X4) {
                x4Btn.isOn = true;
                speedToggleGroup.NotifyToggleOn(x4Btn);
            }
        }
        }

    public void SetProgressionSpeed1X() {
        GameManager.Instance.SetProgressionSpeed(PROGRESSION_SPEED.X1);
        Unpause();
    }
    public void SetProgressionSpeed2X() {
        GameManager.Instance.SetProgressionSpeed(PROGRESSION_SPEED.X2);
        Unpause();
    }
    public void SetProgressionSpeed4X() {
        GameManager.Instance.SetProgressionSpeed(PROGRESSION_SPEED.X4);
        Unpause();
    }
    public void Pause() {
        GameManager.Instance.SetPausedState(true);
        if (onPauseEventExpiration != null) {
            onPauseEventExpiration(true);
        }
    }
    public void Unpause() {
        GameManager.Instance.SetPausedState(false);
        if (onPauseEventExpiration != null) {
            onPauseEventExpiration(false);
        }
    }
    #endregion

    #region Minimap
    internal void UpdateMinimapInfo() {
        CameraMove.Instance.UpdateMinimapTexture();
    }
    #endregion

    #region coroutines
    public IEnumerator RepositionGrid(UIGrid thisGrid) {
        yield return null;
        if (thisGrid != null && this.gameObject.activeSelf) {
            thisGrid.Reposition();
        }
        yield return null;
    }
    public IEnumerator RepositionTable(UITable thisTable) {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        thisTable.Reposition();
    }
    public IEnumerator RepositionScrollView(UIScrollView thisScrollView, bool keepScrollPosition = false) {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        if (keepScrollPosition) {
            thisScrollView.UpdatePosition();
        } else {
            thisScrollView.ResetPosition();
            thisScrollView.Scroll(0f);
        }
        yield return new WaitForEndOfFrame();
        thisScrollView.UpdateScrollbars();
    }
    public IEnumerator LerpProgressBar(UIProgressBar progBar, float targetValue, float lerpTime) {
        float elapsedTime = 0f;
        while (elapsedTime < lerpTime) {
            progBar.value = Mathf.Lerp(progBar.value, targetValue, (elapsedTime/lerpTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
    #endregion

    #region Tooltips
    public void ShowSmallInfo(string info) {
        smallInfoLbl.text = info;

        var v3 = Input.mousePosition;
        //v3.z = 10.0f;
        //v3 = uiCamera.ScreenToWorldPoint(v3);
        //v3.y -= 0.15f;

        //Bounds uiCameraBounds = uiCamera.GetComponent<Camera>().bound

        //if (v3.y <= 0f) {
        //    v3 = Input.mousePosition;
        //    v3.z = 10.0f;
        //    v3 = uiCamera.ScreenToWorldPoint(v3);
        //    v3.y += 0.1f;
        //}
        //if (v3.x >= -13.8f) {
        //    v3 = Input.mousePosition;
        //    v3.z = 10.0f;
        //    v3 = uiCamera.ScreenToWorldPoint(v3);
        //    v3.x -= 0.2f;
        //}

        smallInfoGO.transform.position = v3;
        smallInfoGO.SetActive(true);
    }
    public void HideSmallInfo() {
        smallInfoGO.SetActive(false);
        //smallInfoGO.transform.parent = this.transform;
    }
    #endregion

    #region Notifications Area
    private void ShowNotification(string text, int expirationTicks, UnityAction onClickAction) {
        notificationArea.ShowNotification(text, expirationTicks, onClickAction);
    }
    #endregion

    #region World History
    internal void AddLogToLogHistory(Log log) {
        Messenger.Broadcast<Log>("AddLogToHistory", log);
    }
    public void ToggleNotificationHistory() {
        //worldHistoryUI.ToggleWorldHistoryUI();
        //if (notificationHistoryGO.activeSelf) {
        //    HideNotificationHistory();
        //} else {
        //    ShowLogHistory();
        //}
    }
    #endregion

    #region UI Utilities
    public void RepositionGridCallback(UIGrid thisGrid) {
        StartCoroutine(RepositionGrid(thisGrid));
    }
    private void EnableUIButton(UIButton btn, bool state) {
        if (state) {
            btn.GetComponent<BoxCollider>().enabled = true;
        } else {
            btn.GetComponent<BoxCollider>().enabled = false;
        }
    }
    /*
	 * Generic toggle function, toggles gameobject to on/off state.
	 * */
    public void ToggleObject(GameObject objectToToggle) {
        objectToToggle.SetActive(!objectToToggle.activeSelf);
    }

    /*
	 * Checker for if the mouse is currently
	 * over a UI Object
	 * */
    public bool IsMouseOnUI() {
        return eventSystem.IsPointerOverGameObject();
        //if (uiCamera != null) {
        //if (Minimap.Instance.isDragging) {
        //    return true;
        //}
        //if (UICamera.hoveredObject != null && (UICamera.hoveredObject.layer == LayerMask.NameToLayer("UI") || UICamera.hoveredObject.layer == LayerMask.NameToLayer("PlayerActions"))) {
        //    return true;
        //}
        //}
        //return false;
    }
    #endregion

    #region Object Pooling
    /*
     * Use this to instantiate UI Objects, so that the program can normalize it's
     * font sizes.
     * */
    internal GameObject InstantiateUIObject(string prefabObjName, Transform parent) {
        //GameObject go = GameObject.Instantiate (prefabObj, parent) as GameObject;
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(prefabObjName, Vector3.zero, Quaternion.identity, parent);
        TextMeshProUGUI[] goLbls = go.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < goLbls.Length; i++) {
            NormalizeFontSizeOfLabel(goLbls[i]);
        }
        return go;
    }
    #endregion

    #region For Testing
    public void ToggleBorders() {
        CameraMove.Instance.ToggleMainCameraLayer("Borders");
        CameraMove.Instance.ToggleMainCameraLayer("MinimapAndHextiles");
    }
    public void StartCorruption() {
        if (landmarkInfoUI.currentlyShowingLandmark != null) {
            landmarkInfoUI.currentlyShowingLandmark.tileLocation.SetUncorruptibleLandmarkNeighbors(0);
            landmarkInfoUI.currentlyShowingLandmark.tileLocation.SetCorruption(true, landmarkInfoUI.currentlyShowingLandmark);
        }
    }
    //public void ToggleResourceIcons() {
    //    CameraMove.Instance.ToggleResourceIcons();
    //}
    //public void ToggleGeneralCamera() {
    //    CameraMove.Instance.ToggleGeneralCamera();
    //}
    //public void ToggleTraderCamera() {
    //    CameraMove.Instance.ToggleTraderCamera();
    //}
    #endregion

    private void HideMainUI() {
        mainUIGO.SetActive(false);
    }

    public void ShowMainUI() {
        mainUIGO.SetActive(true);
    }

    #region Landmark Info
    [Space(10)]
    [Header("Landmark Info")]
    [SerializeField]
    internal LandmarkInfoUI landmarkInfoUI;
    public void ShowLandmarkInfo(BaseLandmark landmark) {
        HideMainUI();
        if (factionInfoUI.isShowing) {
            factionInfoUI.HideMenu();
        }
        if (characterInfoUI.isShowing) {
            characterInfoUI.HideMenu();
        }
        if (hexTileInfoUI.isShowing) {
            hexTileInfoUI.HideMenu();
        }
        //if (questInfoUI.isShowing) {
        //    questInfoUI.HideMenu();
        //}
        if (partyinfoUI.isShowing) {
            partyinfoUI.HideMenu();
        }
        if (monsterInfoUI.isShowing) {
            monsterInfoUI.HideMenu();
        }
        landmarkInfoUI.SetData(landmark);
        landmarkInfoUI.OpenMenu();
        landmark.CenterOnLandmark();
        //		playerActionsUI.ShowPlayerActionsUI ();
    }
    public void UpdateLandmarkInfo() {
        if (landmarkInfoUI.isShowing) {
            landmarkInfoUI.UpdateLandmarkInfo();
        }
    }
    #endregion

    #region Faction Info
    [Space(10)]
    [Header("Faction Info")]
    [SerializeField]
    internal FactionInfoUI factionInfoUI;
    public void ShowFactionInfo(Faction faction) {
        HideMainUI();
        if (landmarkInfoUI.isShowing) {
            landmarkInfoUI.HideMenu();
        }
        if (characterInfoUI.isShowing) {
            characterInfoUI.HideMenu();
        }
        if (hexTileInfoUI.isShowing) {
            hexTileInfoUI.HideMenu();
        }
        //if (questInfoUI.isShowing) {
        //    questInfoUI.HideMenu();
        //}
        if (partyinfoUI.isShowing) {
            partyinfoUI.HideMenu();
        }
        if (monsterInfoUI.isShowing) {
            monsterInfoUI.HideMenu();
        }
        factionInfoUI.SetData(faction);
        factionInfoUI.OpenMenu();
        //		playerActionsUI.ShowPlayerActionsUI ();
    }
    public void UpdateFactionInfo() {
        if (factionInfoUI.isShowing) {
            factionInfoUI.UpdateFactionInfo();
        }
    }
    #endregion

    #region Character Info
    [Space(10)]
    [Header("Character Info")]
    [SerializeField] internal CharacterInfoUI characterInfoUI;
    public void ShowCharacterInfo(ECS.Character character) {
        HideMainUI();
        if (landmarkInfoUI.isShowing) {
            landmarkInfoUI.HideMenu();
        }
        if (factionInfoUI.isShowing) {
            factionInfoUI.HideMenu();
        }
        if (hexTileInfoUI.isShowing) {
            hexTileInfoUI.HideMenu();
        }
        //if (questInfoUI.isShowing) {
        //    questInfoUI.HideMenu();
        //}
        if (partyinfoUI.isShowing) {
            partyinfoUI.HideMenu();
        }
        if (monsterInfoUI.isShowing) {
            monsterInfoUI.HideMenu();
        }
        characterInfoUI.SetData(character);
        characterInfoUI.OpenMenu();
        character.CenterOnCharacter();
        //		playerActionsUI.ShowPlayerActionsUI ();
    }
    public void UpdateCharacterInfo() {
        if (characterInfoUI.isShowing) {
            characterInfoUI.UpdateCharacterInfo();
        }
    }
    #endregion

    #region HexTile Info
    [Space(10)]
    [Header("HexTile Info")]
    [SerializeField] internal HextileInfoUI hexTileInfoUI;
    public void ShowHexTileInfo(HexTile hexTile) {
        HideMainUI();
        if (landmarkInfoUI.isShowing) {
            landmarkInfoUI.HideMenu();
        }
        if (factionInfoUI.isShowing) {
            factionInfoUI.HideMenu();
        }
        if (characterInfoUI.isShowing) {
            characterInfoUI.HideMenu();
        }
        //if (questInfoUI.isShowing) {
        //    questInfoUI.HideMenu();
        //}
        if (partyinfoUI.isShowing) {
            partyinfoUI.HideMenu();
        }
        if (monsterInfoUI.isShowing) {
            monsterInfoUI.HideMenu();
        }
        hexTileInfoUI.SetData(hexTile);
        hexTileInfoUI.OpenMenu();
        //		playerActionsUI.ShowPlayerActionsUI ();
    }
    public void UpdateHexTileInfo() {
        if (hexTileInfoUI.isShowing) {
            hexTileInfoUI.UpdateHexTileInfo();
        }
    }
    #endregion

    #region Party Info
    [Space(10)]
    [Header("Party Info")]
    [SerializeField] internal PartyInfoUI partyinfoUI;
    public void ShowPartyInfo(NewParty party) {
        HideMainUI();
        if (landmarkInfoUI.isShowing) {
            landmarkInfoUI.HideMenu();
        }
        if (factionInfoUI.isShowing) {
            factionInfoUI.HideMenu();
        }
        if (characterInfoUI.isShowing) {
            characterInfoUI.HideMenu();
        }
        //if (questInfoUI.isShowing) {
        //	questInfoUI.HideMenu();
        //}
        if (hexTileInfoUI.isShowing) {
            hexTileInfoUI.HideMenu();
        }
        if (monsterInfoUI.isShowing) {
            monsterInfoUI.HideMenu();
        }
        partyinfoUI.SetData(party);
        partyinfoUI.OpenMenu();
    }
    public void UpdatePartyInfo() {
        if (partyinfoUI.isShowing) {
            partyinfoUI.UpdatePartyInfo();
        }
    }
    #endregion

    #region Monster Info
    [Space(10)]
    [Header("Monster Info")]
    [SerializeField]
    internal MonsterInfoUI monsterInfoUI;
    public void ShowMonsterInfo(Monster monster) {
        HideMainUI();
        if (landmarkInfoUI.isShowing) {
            landmarkInfoUI.HideMenu();
        }
        if (factionInfoUI.isShowing) {
            factionInfoUI.HideMenu();
        }
        if (hexTileInfoUI.isShowing) {
            hexTileInfoUI.HideMenu();
        }
        //if (questInfoUI.isShowing) {
        //    questInfoUI.HideMenu();
        //}
        if (partyinfoUI.isShowing) {
            partyinfoUI.HideMenu();
        }
        if (characterInfoUI.isShowing) {
            characterInfoUI.HideMenu();
        }
        monsterInfoUI.SetData(monster);
        monsterInfoUI.OpenMenu();
        //		playerActionsUI.ShowPlayerActionsUI ();
    }
    public void UpdateMonsterInfo() {
        if (monsterInfoUI.isShowing) {
            monsterInfoUI.UpdateMonsterInfo();
        }
    }
    #endregion

    #region Player Actions
    [Space(10)]
    [Header("Player Actions")]
    [SerializeField] internal PlayerActionsUI playerActionsUI;
    public void ShowPlayerActions(ILocation location) {
        //		playerActionsUI.transform.parent = location.tileLocation.UIParent;
        playerActionsUI.ShowPlayerActionsUI(location);
        playerActionsUI.Reposition();
    }
    public void HidePlayerActions() {
        playerActionsUI.HidePlayerActionsUI();
    }
    #endregion

    #region Menu History
    public void AddMenuToQueue(UIMenu menu, object data) {
        UIMenuSettings latestSetting = _menuHistory.ElementAtOrDefault(0);
        if (latestSetting != null) {
            if (latestSetting.menu == menu && latestSetting.data == data) {
                //the menu settings to be added are the same as the latest one, ignore.
                return;
            }
        }
        _menuHistory.Add(new UIMenuSettings(menu, data));
        //string text = string.Empty;
        //for (int i = 0; i < _menuHistory.Count; i++) {
        //    UIMenuSettings currSetting = _menuHistory.ElementAt(i);
        //    text += currSetting.menu.GetType().ToString();
        //    if(currSetting.data is Faction) {
        //        text += " - Faction " + (currSetting.data as Faction).name;
        //    } else if(currSetting.data is Party) {
        //        text += " - Party " + (currSetting.data as Party).name;
        //    } else if (currSetting.data is HexTile) {
        //        text += " - HexTile " + (currSetting.data as HexTile).name;
        //    } else if (currSetting.data is BaseLandmark) {
        //        text += " - Landmark " + (currSetting.data as BaseLandmark).landmarkName;
        //    } else if (currSetting.data is ECS.Character) {
        //        text += " - Character " + (currSetting.data as ECS.Character).name;
        //    } else if (currSetting.data is OldQuest.Quest) {
        //        text += " - OldQuest.Quest " + (currSetting.data as OldQuest.Quest).questType.ToString();
        //    }
        //    text += "\n";
        //}
        //Debug.Log(text);
    }
    public void ShowPreviousMenu() {
        _menuHistory.RemoveAt(_menuHistory.Count - 1);
        UIMenuSettings menuToShow = _menuHistory.ElementAt(_menuHistory.Count - 1);
        //_menuHistory.Remove(menuToShow);
        menuToShow.menu.ShowMenu();
        menuToShow.menu.SetData(menuToShow.data);
        //string text = string.Empty;
        //for (int i = 0; i < _menuHistory.Count; i++) {
        //    UIMenuSettings currSetting = _menuHistory.ElementAt(i);
        //    text += currSetting.menu.GetType().ToString();
        //    if (currSetting.data is Faction) {
        //        text += " - Faction " + (currSetting.data as Faction).name;
        //    } else if (currSetting.data is Party) {
        //        text += " - Party " + (currSetting.data as Party).name;
        //    } else if (currSetting.data is HexTile) {
        //        text += " - HexTile " + (currSetting.data as HexTile).name;
        //    } else if (currSetting.data is BaseLandmark) {
        //        text += " - Landmark " + (currSetting.data as BaseLandmark).landmarkName;
        //    } else if (currSetting.data is ECS.Character) {
        //        text += " - Character " + (currSetting.data as ECS.Character).name;
        //    } else if (currSetting.data is OldQuest.Quest) {
        //        text += " - OldQuest.Quest " + (currSetting.data as OldQuest.Quest).questType.ToString();
        //    }
        //    text += "\n";
        //}
        //Debug.Log(text);
    }
    public void ClearMenuHistory() {
        _menuHistory.Clear();
    }
    #endregion

    #region Console
    [Space(10)]
    [Header("Console")]
    [SerializeField] internal ConsoleMenu consoleUI;
    public bool IsConsoleShowing() {
        //return false;
        return consoleUI.isShowing;
    }
    public void ToggleConsole() {
        if (consoleUI.isShowing) {
            HideConsole();
        } else {
            ShowConsole();
        }
    }
    public void ShowConsole() {
        consoleUI.ShowConsole();
    }
    public void HideConsole() {
        consoleUI.HideConsole();
    }
    #endregion

    #region Combat History Logs
    [Space(10)]
    [Header("Combat History")]
    [SerializeField] internal CombatLogsUI combatLogUI;
    public void ShowCombatLog(ECS.Combat combat) {
        //if(questLogUI.isShowing){
        //	questLogUI.HideQuestLogs ();
        //}
        combatLogUI.ShowCombatLogs(combat);
        combatLogUI.UpdateCombatLogs();
    }
    #endregion

    #region Characters Summary
    [Space(10)]
    [Header("Characters Summary")]
    [SerializeField] private GameObject charactersSummaryGO;
    public CharactersSummaryUI charactersSummaryMenu;
    public void ShowCharactersSummary() {
        //HideQuestsSummary();
        //HideStorylinesSummary();
        worldInfoCharactersSelectedGO.SetActive(true);
        charactersSummaryMenu.OpenMenu();
    }
    public void HideCharactersSummary() {
        worldInfoCharactersSelectedGO.SetActive(false);
        charactersSummaryMenu.CloseMenu();
    }
    //public void UpdateCharacterSummary() {
    //    string questSummary = string.Empty;
    //    questSummary += "[b]Available Quests: [/b]";
    //    for (int i = 0; i < QuestManager.Instance.availableQuests.Count; i++) {
    //        Quest currentQuest = QuestManager.Instance.availableQuests[i];
    //        if (!currentQuest.isDone) {
    //            questSummary += "\n" + currentQuest.questURLName;
    //            questSummary += "\n   Characters on Quest: ";
    //            if (currentQuest.acceptedCharacters.Count > 0) {
    //                for (int j = 0; j < currentQuest.acceptedCharacters.Count; j++) {
    //                    ECS.Character currCharacter = currentQuest.acceptedCharacters[j];
    //                    questSummary += "\n" + currCharacter.urlName + " (" + currCharacter.currentQuestPhase.phaseName + ")";
    //                }
    //            } else {
    //                questSummary += "NONE";
    //            }
    //        }
    //    }
    //    questsSummaryLbl.text = questSummary;
    //    questsSummaryLbl.ResizeCollider();
    //}
    #endregion

    #region Context Menu
    [Space(10)]
    [Header("Context Menu")]
    public GameObject contextMenuPrefab;
    public GameObject contextMenuItemPrefab;
    public UIContextMenu contextMenu;
    private void ShowContextMenu(HexTile tile) {
        if (PlayerManager.Instance.isChoosingStartingTile || landmarkInfoUI.isWaitingForAttackTarget) {
            return;
        }
        ContextMenuSettings settings = tile.GetContextMenuSettings();
        if (settings.items.Count > 0) {
            contextMenu.LoadSettings(settings);
            contextMenu.gameObject.SetActive(true);
            //Vector2 pos;
            //RectTransformUtility.ScreenPointToLocalPointInRectangle(this.transform as RectTransform, Input.mousePosition, eventSystem.camera, out pos);
            contextMenu.transform.position = Input.mousePosition;
        }
        
    }
    public void HideContextMenu() {
        contextMenu.gameObject.SetActive(false);
    }
    public void HideContextMenu(HexTile tile) {
        HideContextMenu();
    }
    #endregion

    #region Save
    public void Save() {
        //Save savefile = new Save();
        //savefile.hextiles = new List<HextileSave>();
        //for (int i = 0; i < GridMap.Instance.hexTiles.Count; i++) {
        //    if(GridMap.Instance.hexTiles[i].landmarkOnTile != null) {
        //        HextileSave hextileSave = new HextileSave();
        //        hextileSave.SaveTile(GridMap.Instance.hexTiles[i]);
        //        savefile.hextiles.Add(hextileSave);
        //    }
        //}
        //SaveGame.Save<Save>("SavedFile1", savefile);
        //LevelLoaderManager.Instance.LoadLevel("MainMenu");
    }
    #endregion

}
