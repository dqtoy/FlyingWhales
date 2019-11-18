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

    public RectTransform mainRT;
    [SerializeField] private EventSystem eventSystem;

    private UIMenu[] allMenus;

    [Space(10)]
    [Header("Date Objects")]
    [SerializeField] private ToggleGroup speedToggleGroup;
    public Toggle pauseBtn;
    public Toggle x1Btn;
    public Toggle x2Btn;
    public Toggle x4Btn;
    [SerializeField] private TextMeshProUGUI dateLbl;
    [SerializeField] private TextMeshProUGUI timeLbl;

    [Space(10)]
    [Header("Small Info")]
    public GameObject smallInfoGO;
    public RectTransform smallInfoRT;
    public HorizontalLayoutGroup smallInfoBGParentLG;
    public RectTransform smallInfoBGRT;
    public TextMeshProUGUI smallInfoLbl;
    public EnvelopContentUnityUI smallInfoEnvelopContent;
    public LocationSmallInfo locationSmallInfo;
    public RectTransform locationSmallInfoRT;
    public GameObject characterPortraitHoverInfoGO;
    public CharacterPortrait characterPortraitHoverInfo;
    public RectTransform characterPortraitHoverInfoRT;

    [Space(10)]
    [Header("Detailed Info")]
    public GameObject detailedInfoGO;
    public RectTransform detailedInfoRect;
    public TextMeshProUGUI detailedInfoLbl;
    public Image detailedInfoIcon;
    public RectTransform detailedInfoContentParent;
    public CharacterPortrait[] detailedInfoPortraits;

    [Space(10)]
    [Header("Other Area Info")]
    public Sprite[] areaCenterSprites;
    public GameObject portalPopup;

    [Space(10)]
    [Header("Notification Area")]
    public DeveloperNotificationArea developerNotificationArea;

    [Space(10)]
    [Header("Portraits")]
    public Transform characterPortraitsParent;

    [Space(10)]
    [Header("Player")]
    [SerializeField] private Toggle minionsMenuToggle;
    [SerializeField] private Toggle charactersMenuToggle;
    [SerializeField] private Toggle locationsMenuToggle;
    [SerializeField] private Toggle factionsMenuToggle;

    [Space(10)]
    [Header("Shared")]
    [SerializeField] private GameObject cover;

    [Space(10)]
    [Header("World UI")]
    [SerializeField] private RectTransform worldUIParent;
    [SerializeField] private GraphicRaycaster worldUIRaycaster;
    [SerializeField] private GameObject worldEventIconPrefab;

    [Space(10)]
    [Header("Object Picker")]
    [SerializeField] private ObjectPicker objectPicker;

    [Space(10)]
    [Header("Options")]
    [SerializeField] private GameObject optionsGO;

    [Space(10)] //FOR TESTING
    [Header("For Testing")]
    public ButtonToggle toggleBordersBtn;
    public ButtonToggle corruptionBtn;
    public POITestingUI poiTestingUI;

    [Space(10)]
    [Header("Combat")]
    public CombatUI combatUI;

    [Space(10)]
    [Header("Nameplate Prefabs")]
    public GameObject characterNameplatePrefab;
    public GameObject stringNameplatePrefab;
    public GameObject unsummonedMinionNameplatePrefab;
    public GameObject worldEventNameplatePrefab;

    [Space(10)]
    [Header("Dual Object Picker")]
    public DualObjectPicker dualObjectPicker;

    public bool isShowingAreaTooltip { get; private set; } //is the tooltip for area double clicks showing?
    private UIMenu lastOpenedMenu = null;
    private List<object> _uiMenuHistory;

    public bool tempDisableShowInfoUI { get; private set; }

    #region Monobehaviours
    private void Awake() {
        Instance = this;
        //_menuHistory = new List<UIMenuSettings>();
    }
    private void Start() {
        _uiMenuHistory = new List<object>();
        Messenger.AddListener<bool>(Signals.PAUSED, UpdateSpeedToggles);
        Messenger.AddListener(Signals.UPDATE_UI, UpdateUI);
        Messenger.AddListener(Signals.INSPECT_ALL, UpdateInteractableInfoUI);
        ToggleBorders();
    }
    private void Update() {
        if (isHoveringTile) {
            //if (currentTileHovered.areaOfTile != null && currentTileHovered.areaOfTile.areaType != AREA_TYPE.DEMONIC_INTRUSION) {
            //    ShowSmallInfo("Double click to view.", currentTileHovered.areaOfTile.name);
            //    isShowingAreaTooltip = true;
            //}
#if UNITY_EDITOR
            if (currentTileHovered.landmarkOnTile != null) {
                currentTileHovered.ShowTileInfo();
               
            }
            //else  {
            //    currentTileHovered.ShowTileInfo();
            //}
#endif
            currentTileHovered.region?.OnHoverOverAction();
        }
    }
    #endregion

    public void ExitGame() {
        Application.Quit();
    }
    internal void InitializeUI() {
        allMenus = this.transform.GetComponentsInChildren<UIMenu>(true);
        for (int i = 0; i < allMenus.Length; i++) {
            allMenus[i].Initialize();
            //allMenus[i].ApplyUnifiedSettings(settings);
        }
        questInfoUI.Initialize();
        //Image[] images = this.gameObject.GetComponentsInChildren<Image>();
        //for (int i = 0; i < images.Length; i++) {
        //    images[i].alphaHitTestMinimumThreshold = 1f;
        //}
        //UnifySelectables();
        //popupMessageBox.Initialize();
        Messenger.AddListener(Signals.HIDE_MENUS, HideMenus);
        Messenger.AddListener<string, int, UnityAction>(Signals.SHOW_DEVELOPER_NOTIFICATION, ShowDeveloperNotification);
        Messenger.AddListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);

        Messenger.AddListener<HexTile>(Signals.TILE_HOVERED_OVER, OnHoverOverTile);
        Messenger.AddListener<HexTile>(Signals.TILE_HOVERED_OUT, OnHoverOutTile);

        Messenger.AddListener<Combat>(Signals.COMBAT_DONE, OnCombatDone);
        //Messenger.AddListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
        //Messenger.AddListener<IInteractable, Interaction>(Signals.ADDED_INTERACTION, OnInteractionAdded);

        Messenger.AddListener(Signals.INTERACTION_MENU_OPENED, OnInteractionMenuOpened);
        Messenger.AddListener(Signals.INTERACTION_MENU_CLOSED, OnInteractionMenuClosed);
        //Messenger.AddListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnPartyStartedTravelling);
        //Messenger.AddListener<Party>(Signals.PARTY_DONE_TRAVELLING, OnPartyDoneTravelling);
        //Messenger.AddListener(Signals.CAMERA_OUT_OF_FOCUS, OnCameraOutOfFocus);
        Messenger.AddListener<Area>(Signals.AREA_MAP_OPENED, OnAreaMapOpened);
        Messenger.AddListener<Area>(Signals.AREA_MAP_CLOSED, OnAreaMapClosed);

        Messenger.AddListener<Intel>(Signals.SHOW_INTEL_NOTIFICATION, ShowPlayerNotification);
        Messenger.AddListener<Log>(Signals.SHOW_PLAYER_NOTIFICATION, ShowPlayerNotification);

        Messenger.AddListener(Signals.ON_OPEN_SHARE_INTEL, OnOpenShareIntelMenu);
        Messenger.AddListener(Signals.ON_CLOSE_SHARE_INTEL, OnCloseShareIntelMenu);
        Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);

        Messenger.AddListener<Region, WorldEvent>(Signals.WORLD_EVENT_SPAWNED, OnWorldEventSpawned);
        Messenger.AddListener<Region, WorldEvent>(Signals.WORLD_EVENT_DESPAWNED, OnWorldEventDespawned);

        Messenger.AddListener<UIMenu>(Signals.MENU_OPENED, OnUIMenuOpened);
        Messenger.AddListener<UIMenu>(Signals.MENU_CLOSED, OnUIMenuClosed);

        UpdateUI();
    }
    private void OnGameLoaded() {
        UpdateUI();
    }
    private void HideMenus() {
        poiTestingUI.HideUI();
        if (characterInfoUI.isShowing) {
            characterInfoUI.CloseMenu();
        }
        if (factionInfoUI.isShowing) {
            factionInfoUI.CloseMenu();
        }
        if (areaInfoUI.isShowing) {
            areaInfoUI.CloseMenu();
        }
        if (regionInfoUI.isShowing) {
            regionInfoUI.CloseMenu();
        }
        if (tileObjectInfoUI.isShowing) {
            tileObjectInfoUI.CloseMenu();
        }
        if (objectPicker.gameObject.activeSelf) {
            HideObjectPicker();
        }
        if (PlayerUI.Instance.attackGridGO.activeSelf) {
            PlayerUI.Instance.HideCombatGrid();
        }
        if (PlayerUI.Instance.isShowingKillSummary) {
            PlayerUI.Instance.HideKillSummary();
        }
        if (PlayerUI.Instance.isShowingMinionList) {
            PlayerUI.Instance.HideMinionList();
        }
        ClearUIMenuHistory();
    }
    public void AddToUIMenuHistory(object data) {
        if(_uiMenuHistory.Count > 0 && _uiMenuHistory[_uiMenuHistory.Count - 1] == data) {
            //This will prevent from having same consecutive objects to go back
            return;
        }
        _uiMenuHistory.Add(data);
    }
    public void ClearUIMenuHistory() {
        _uiMenuHistory.Clear();
    }
    public object GetLastUIMenuHistory() {
        int index = _uiMenuHistory.Count - 2;
        if(index < 0) {
            return null;
        } else {
            return _uiMenuHistory[index];
        }
    }
    public void RemoveLastUIMenuHistory() {
        for (int i = 0; i < 2; i++) {
            if (_uiMenuHistory.Count > 0) {
                _uiMenuHistory.RemoveAt(_uiMenuHistory.Count - 1);
            }
        }
    }
    private void UpdateUI() {
        //dateLbl.SetText(GameManager.Instance.continuousDays + "/" + GameManager.ConvertTickToTime(GameManager.Instance.tick));
        dateLbl.SetText("Day " + GameManager.Instance.continuousDays + "\n" + GameManager.ConvertTickToTime(GameManager.Instance.tick));
        //timeLbl.SetText(GameManager.GetTimeInWordsOfTick(GameManager.Instance.tick).ToString());
        //timeLbl.SetText("");

        UpdateInteractableInfoUI();
        UpdateFactionInfo();
        //UpdateHexTileInfo();
        //UpdateCombatLogs();
        //UpdateQuestSummary();
        PlayerUI.Instance.UpdateUI();
    }
    private void UpdateInteractableInfoUI() {
        UpdateCharacterInfo();
        UpdateTileObjectInfo();
        UpdateRegionInfo();
        UpdateAreaInfo();
        UpdateQuestInfo();
    }

    #region World Controls
    private void UpdateSpeedToggles(bool isPaused) {
        if (!gameObject.activeInHierarchy) {
            return;
        }
        if (isPaused) {
            pauseBtn.isOn = true;
            speedToggleGroup.NotifyToggleOn(pauseBtn);
        } else {
            if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X1) {
                x1Btn.isOn = true;
                speedToggleGroup.NotifyToggleOn(x1Btn);
                //SetProgressionSpeed1X();
            } else if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X2) {
                x2Btn.isOn = true;
                speedToggleGroup.NotifyToggleOn(x2Btn);
                //SetProgressionSpeed2X();
            } else if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X4) {
                x4Btn.isOn = true;
                speedToggleGroup.NotifyToggleOn(x4Btn);
                //SetProgressionSpeed4X();
            }
        }
    }
    private void OnProgressionSpeedChanged(PROGRESSION_SPEED speed) {
        UpdateSpeedToggles(GameManager.Instance.isPaused);
    }
    public void SetProgressionSpeed1X() {
        if (!x1Btn.IsInteractable()) {
            return;
        }
        Unpause();
        GameManager.Instance.SetProgressionSpeed(PROGRESSION_SPEED.X1);
    }
    public void SetProgressionSpeed2X() {
        if (!x2Btn.IsInteractable()) {
            return;
        }
        Unpause();
        GameManager.Instance.SetProgressionSpeed(PROGRESSION_SPEED.X2);
    }
    public void SetProgressionSpeed4X() {
        if (!x4Btn.IsInteractable()) {
            return;
        }
        Unpause();
        GameManager.Instance.SetProgressionSpeed(PROGRESSION_SPEED.X4);
    }
    public void Pause() {
        GameManager.Instance.SetPausedState(true);
    }
    public void Unpause() {
        GameManager.Instance.SetPausedState(false);
    }
    public void ShowDateSummary() {
        ShowSmallInfo(GameManager.Instance.Today().ToStringDate());
    }
    public void SetSpeedTogglesState(bool state) {
        pauseBtn.interactable = state;
        x1Btn.interactable = state;
        x2Btn.interactable = state;
        x4Btn.interactable = state;
    }
    /// <summary>
    /// Resume the last speed that the player was in before pausing the game.
    /// </summary>
    public void ResumeLastProgressionSpeed() {
        SetSpeedTogglesState(true);
        if (GameManager.Instance.lastProgressionBeforePausing == "paused") {
            //pause the game
            Pause();
        } else if (GameManager.Instance.lastProgressionBeforePausing == "1") {
            SetProgressionSpeed1X();
        } else if (GameManager.Instance.lastProgressionBeforePausing == "2") {
            SetProgressionSpeed2X();
        } else if (GameManager.Instance.lastProgressionBeforePausing == "4") {
            SetProgressionSpeed4X();
        }
    }
    #endregion

    #region Options
    public void ToggleOptionsMenu() {
        optionsGO.SetActive(!optionsGO.activeSelf);
        if (optionsGO.activeSelf) {
            Pause();
            SetSpeedTogglesState(false);
        } else {
            ResumeLastProgressionSpeed();
        }
    }
    #endregion

    #region Minimap
    internal void UpdateMinimapInfo() {
        //CameraMove.Instance.UpdateMinimapTexture();
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
    public string smallInfoShownFrom { get; private set; }
    public void ShowSmallInfo(string info, string header = "") {
        string message = string.Empty;
        if (!string.IsNullOrEmpty(header)) {
            message = "<font=\"Eczar-Medium\"><line-height=100%><size=18>" + header + "</font>\n";
        }
        message += "<line-height=70%><size=16>" + info;

        message = message.Replace("\\n", "\n");

        smallInfoLbl.text = message;
        if (!IsSmallInfoShowing()) {
            smallInfoGO.transform.SetParent(this.transform);
            smallInfoGO.SetActive(true);
            //smallInfoEnvelopContent.Execute();
        }
        PositionTooltip(smallInfoGO, smallInfoRT, smallInfoBGRT);
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
        // get calling method name
        smallInfoShownFrom = stackTrace.GetFrame(1).GetMethod().Name;
        //Debug.Log(smallInfoShownFrom);
        //Debug.Log("Show small info " + info);
    }
    public void ShowSmallInfo(string info, UIHoverPosition pos, string header = "") {
        string message = string.Empty;
        if (!string.IsNullOrEmpty(header)) {
            message = "<font=\"Eczar-Medium\"><line-height=100%><size=18>" + header + "</font>\n";
        }
        message += "<line-height=70%><size=16>" + info;

        message = message.Replace("\\n", "\n");

        smallInfoLbl.text = message;
        if (!IsSmallInfoShowing()) {
            smallInfoGO.SetActive(true);
        }
        PositionTooltip(pos, smallInfoGO, smallInfoRT);
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
        // get calling method name
        smallInfoShownFrom = stackTrace.GetFrame(1).GetMethod().Name;
        //Debug.Log(smallInfoShownFrom);
        //Debug.Log("Show small info " + info);
    }
    public void HideSmallInfo() {
        if (IsSmallInfoShowing()) {
            smallInfoShownFrom = string.Empty;
            smallInfoGO.SetActive(false);
        }
    }
    public bool IsSmallInfoShowing() {
        return smallInfoGO.activeSelf;
    }
    public void ShowCharacterPortraitHoverInfo(Character character) {
        characterPortraitHoverInfo.GeneratePortrait(character);
        characterPortraitHoverInfoGO.SetActive(true);

        characterPortraitHoverInfoRT.SetParent(this.transform);
        PositionTooltip(characterPortraitHoverInfoRT.gameObject, characterPortraitHoverInfoRT, characterPortraitHoverInfoRT);
    }
    public void HideCharacterPortraitHoverInfo() {
        characterPortraitHoverInfoGO.SetActive(false);
    }
    public void PositionTooltip(GameObject tooltipParent, RectTransform rtToReposition, RectTransform boundsRT) {
        PositionTooltip(Input.mousePosition, tooltipParent, rtToReposition, boundsRT);
    }
    private void PositionTooltip(Vector3 position, GameObject tooltipParent, RectTransform rtToReposition, RectTransform boundsRT) {
        var v3 = position;

        rtToReposition.pivot = new Vector2(0f, 1f);
        smallInfoBGParentLG.childAlignment = TextAnchor.UpperLeft;

        if (CursorManager.Instance.currentCursorType == CursorManager.Cursor_Type.Cross || CursorManager.Instance.currentCursorType == CursorManager.Cursor_Type.Check || CursorManager.Instance.currentCursorType == CursorManager.Cursor_Type.Link) {
            v3.x += 100f;
            v3.y -= 32f;
        } else {
            v3.x += 25f;
            v3.y -= 25f;
        }
        
        tooltipParent.transform.position = v3;

        Vector3[] corners = new Vector3[4]; //bottom-left, top-left, top-right, bottom-right
        List<int> cornersOutside = new List<int>();
        boundsRT.GetWorldCorners(corners);
        for (int i = 0; i < 4; i++) {
            // Backtransform to parent space
            Vector3 localSpacePoint = mainRT.InverseTransformPoint(corners[i]);
            // If parent (canvas) does not contain checked items any point
            if (!mainRT.rect.Contains(localSpacePoint)) {
                cornersOutside.Add(i);
            }
        }

        if (cornersOutside.Count != 0) {
            string log = "Corners outside are: ";
            for (int i = 0; i < cornersOutside.Count; i++) {
                log += cornersOutside[i].ToString() + ", ";
            }
            //Debug.Log(log);
            if (cornersOutside.Contains(2) && cornersOutside.Contains(3)) {
                if (cornersOutside.Contains(0)) {
                    //bottom side and right side are outside, move anchor to bottom right
                    rtToReposition.pivot = new Vector2(1f, 0f);
                    smallInfoBGParentLG.childAlignment = TextAnchor.LowerRight;
                } else {
                    //right side is outside, move anchor to top right side
                    rtToReposition.pivot = new Vector2(1f, 1f);
                    smallInfoBGParentLG.childAlignment = TextAnchor.UpperRight;
                }
            } else if (cornersOutside.Contains(0) && cornersOutside.Contains(3)) {
                //bottom side is outside, move anchor to bottom left
                rtToReposition.pivot = new Vector2(0f, 0f);
                smallInfoBGParentLG.childAlignment = TextAnchor.LowerLeft;
            }
            rtToReposition.localPosition = Vector3.zero;
        }
    }
    private void PositionTooltip(UIHoverPosition position, GameObject tooltipParent, RectTransform rt) {
        tooltipParent.transform.SetParent(position.transform);
        RectTransform tooltipParentRT = tooltipParent.transform as RectTransform;
        tooltipParentRT.pivot = position.pivot;

        Vector2 anchorMin = Vector2.zero;
        Vector2 anchorMax = Vector2.zero;
        Utilities.GetAnchorMinMax(position.anchor, ref anchorMin, ref anchorMax);
        tooltipParentRT.anchorMin = anchorMin;
        tooltipParentRT.anchorMax = anchorMax;
        tooltipParentRT.anchoredPosition = Vector2.zero;

        smallInfoBGParentLG.childAlignment = position.anchor;
        rt.pivot = position.pivot;
    }
    public void ShowSmallLocationInfo(Region region, RectTransform initialParent, Vector2 adjustment, string subText = "") {
        locationSmallInfo.ShowRegionInfo(region, subText);
        locationSmallInfoRT.SetParent(initialParent);
        locationSmallInfoRT.anchoredPosition = Vector3.zero;
        locationSmallInfoRT.anchoredPosition += adjustment;
        locationSmallInfoRT.SetParent(this.transform);
        //(locationSmallInfo.transform as RectTransform).anchoredPosition = pos;
    }
    public void ShowSmallLocationInfo(Region region, Vector3 pos, string subText = "") {
        locationSmallInfo.ShowRegionInfo(region, subText);
        locationSmallInfoRT.position = pos;
    }
    public void HideSmallLocationInfo() {
        locationSmallInfo.Hide();
    }
    public bool IsSmallLocationInfoShowing() {
        return locationSmallInfoRT.gameObject.activeSelf;
    }
    public Region GetCurrentlyShowingSmallInfoLocation() {
        if (IsSmallLocationInfoShowing()) {
            return locationSmallInfo.region;
        }
        return null;
    }
    #endregion

    #region Developer Notifications Area
    private void ShowDeveloperNotification(string text, int expirationTicks, UnityAction onClickAction) {
        developerNotificationArea.ShowNotification(text, expirationTicks, onClickAction);
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
    private void OnUIMenuOpened(UIMenu menu) {
        if (menu is RegionInfoUI) {
            MoveNotificationMenuToModifiedPos();
        }
    }
    private void OnUIMenuClosed(UIMenu menu) {
        if (menu is RegionInfoUI) {
            MoveNotificationMenuToDefaultPos();
        }
    }
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
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycastResults);

        if (raycastResults.Count > 0) {
            foreach (var go in raycastResults) {
                if (go.gameObject.layer == LayerMask.NameToLayer("UI") || go.gameObject.layer == LayerMask.NameToLayer("WorldUI")) {
                    //Debug.Log(go.gameObject.name, go.gameObject);
                    return true;
                }

            }
        }
        return false;
    }
    public bool IsMouseOnInput() {
        if (EventSystem.current.currentSelectedGameObject == null ||
            (EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() == null &&
            EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() == null)) {
            return false;
        }
        return true;
    }
    public void SetCoverState(bool state, bool blockClicks = true) {
        cover.SetActive(state);
        cover.GetComponent<Image>().raycastTarget = blockClicks;
    }
    private void OnInteractionMenuOpened() {
        if (areaInfoUI.isShowing) {
            lastOpenedMenu = areaInfoUI;
        } else if (characterInfoUI.isShowing) {
            lastOpenedMenu = characterInfoUI;
        }
        //if (objectPicker.gameObject.activeSelf) {
        //    HideObjectPicker();
        //}
        //HideMenus();
        if (areaInfoUI.isShowing) {
            areaInfoUI.gameObject.SetActive(false);
        }
        if (characterInfoUI.isShowing) {
            characterInfoUI.gameObject.SetActive(false);
        }
    }
    private void OnInteractionMenuClosed() {
        //reopen last opened menu
        if (lastOpenedMenu != null) {
            lastOpenedMenu.OpenMenu();
            lastOpenedMenu = null;
        }
    }
    public void ScrollRectSnapTo(ScrollRect scrollRect, RectTransform target) {
        Canvas.ForceUpdateCanvases();
        Vector2 ogPos = scrollRect.content.anchoredPosition;
        Vector2 diff = (Vector2)scrollRect.transform.InverseTransformPoint(scrollRect.content.position)
            - (Vector2)scrollRect.transform.InverseTransformPoint(target.position);
        scrollRect.content.anchoredPosition = new Vector2(ogPos.x, diff.y);
            
    }
    public void SetTempDisableShowInfoUI(bool state) {
        tempDisableShowInfoUI = state;
    }
    #endregion

    #region Object Pooling
    /*
     * Use this to instantiate UI Objects, so that the program can normalize it's
     * font sizes.
     * */
    internal GameObject InstantiateUIObject(string prefabObjName, Transform parent) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(prefabObjName, Vector3.zero, Quaternion.identity, parent);
        return go;
    }
    #endregion

    #region Nameplate
    public void CreateAreaNameplate(Area area) {
        GameObject nameplateGO = UIManager.Instance.InstantiateUIObject("AreaNameplate", worldUIParent);
        //nameplateGO.transform.localScale = new Vector3(0.02f, 0.02f, 1f);
        nameplateGO.GetComponent<AreaNameplate>().SetArea(area);
    }
    public LandmarkNameplate CreateLandmarkNameplate(BaseLandmark landmark) {
        GameObject nameplateGO = UIManager.Instance.InstantiateUIObject("LandmarkNameplate", worldUIParent);
        nameplateGO.transform.localScale = Vector3.one;
        LandmarkNameplate nameplate = nameplateGO.GetComponent<LandmarkNameplate>();
        nameplate.SetLandmark(landmark);
        return nameplate;
    }
    #endregion

    #region Object Picker
    public void ShowClickableObjectPicker<T>(List<T> choices, Action<object> onClickAction, IComparer<T> comparer = null
        , Func<T, bool> validityChecker = null, string title = ""
        , Action<T> onHoverAction = null, Action<T> onHoverExitAction = null, string identifier = "", bool showCover = false, int layer = 9, bool closable = true) {

        objectPicker.ShowClickable(choices, onClickAction, comparer, validityChecker, title, onHoverAction, onHoverExitAction, identifier, showCover, layer, closable);
        //Pause();
        //SetSpeedTogglesState(false);
    }
    //public void ShowDraggableObjectPicker<T>(List<T> choices, IComparer<T> comparer = null, Func<T, bool> validityChecker = null, string title = "") {
    //    objectPicker.ShowDraggable(choices, comparer, validityChecker, title);
    //}
    public void HideObjectPicker() {
        objectPicker.Hide();
        //Unpause();
        //SetSpeedTogglesState(true);
    }
    public bool IsObjectPickerOpen() {
        return objectPicker.gameObject.activeSelf;
    }
    #endregion

    #region For Testing
    public void ToggleBorders() {
        CameraMove.Instance.ToggleMainCameraLayer("Borders");
        CameraMove.Instance.ToggleMainCameraLayer("MinimapAndHextiles");
    }
    public void SetUIState(bool state) {
        this.gameObject.SetActive(state);
    }
    public void DateHover() {
        ShowSmallInfo("Day: " +  GameManager.Instance.continuousDays.ToString() + " Tick: " + GameManager.Instance.tick.ToString());
    }
    [ExecuteInEditMode]
    [ContextMenu("Set All Scroll Rect Scroll Speed")]
    public void SetAllScrollSpeed() {
        ScrollRect[] allScroll = this.gameObject.GetComponentsInChildren<ScrollRect>(true);
        for (int i = 0; i < allScroll.Length; i++) {
            ScrollRect rect = allScroll[i];
            rect.scrollSensitivity = 25f;
        }
    }
    #endregion

    #region Area Info
    [Space(10)]
    [Header("Area Info")]
    [SerializeField]
    internal AreaInfoUI areaInfoUI;

    //Why change data parameter from Area to Hextile?
    //It's because the tile data is needed now since we can construct demonic landmarks, etc
    //If we only pass the area data, the only tile we can get is the coreTile
    //We have no way of knowing now how to get the actual tile the player clicked
    //Thus, the information that will be shown to the player will be wrong
    //So in order for us to process exactly what the player clicked, the tile must be passed not the area
    //IMPORTANT NOTE: MAKE SURE THAT THE TILE PASSED HAS AN AREA
    public void ShowAreaInfo(HexTile tile) { //Area area
        //if (PlayerManager.Instance.player.homeArea == area) {
        //    portalPopup.SetActive(true);
        //} else {
        if (factionInfoUI.isShowing) {
            factionInfoUI.CloseMenu();
        }
        if (characterInfoUI.isShowing) {
            characterInfoUI.CloseMenu();
        }
        if (tileObjectInfoUI.isShowing) {
            tileObjectInfoUI.CloseMenu();
        }
        if (regionInfoUI.isShowing) {
            regionInfoUI.CloseMenu();
        }
        areaInfoUI.SetData(tile);
        areaInfoUI.OpenMenu();
        areaInfoUI.CenterOnTile();
        //}
    }
    public void UpdateAreaInfo() {
        if (areaInfoUI.isShowing) {
            areaInfoUI.UpdateAreaInfo();
        }
    }
    public Sprite GetAreaCenterSprite(string name) {
        for (int i = 0; i < areaCenterSprites.Length; i++) {
            if (areaCenterSprites[i].name.ToLower() == name.ToLower()) {
                return areaCenterSprites[i];
            }
        }
        return null;
    }
    #endregion

    #region Faction Info
    [Space(10)]
    [Header("Faction Info")]
    [SerializeField]
    internal FactionInfoUI factionInfoUI;
    public void ShowFactionInfo(Faction faction) {
        if (tempDisableShowInfoUI) {
            SetTempDisableShowInfoUI(false);
            return;
        }
        if (areaInfoUI.isShowing) {
            areaInfoUI.CloseMenu();
        }
        if (characterInfoUI.isShowing) {
            characterInfoUI.CloseMenu();
        }
        if (tileObjectInfoUI.isShowing) {
            tileObjectInfoUI.CloseMenu();
        }
        if (regionInfoUI.isShowing) {
            regionInfoUI.CloseMenu();
        }
        factionInfoUI.SetData(faction);
        factionInfoUI.OpenMenu();
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
    public void ShowCharacterInfo(Character character, bool centerOnCharacter = false) {
        if (tempDisableShowInfoUI) {
            SetTempDisableShowInfoUI(false);
            return;
        }
        if (areaInfoUI.isShowing) {
            areaInfoUI.CloseMenu();
        }
        if (factionInfoUI.isShowing) {
            factionInfoUI.CloseMenu();
        }
        if (tileObjectInfoUI.isShowing) {
            tileObjectInfoUI.CloseMenu();
        }
        if (regionInfoUI.isShowing) {
            regionInfoUI.CloseMenu();
        }
        characterInfoUI.SetData(character);
        characterInfoUI.OpenMenu();
        if (centerOnCharacter) {
            character.CenterOnCharacter();
        }
    }
    public void UpdateCharacterInfo() {
        if (characterInfoUI.isShowing) {
            characterInfoUI.UpdateCharacterInfo();
        }
    }
    //private void OnPartyStartedTravelling(Party party) {
    //    if(characterInfoUI.isShowing && party.characters.Contains(characterInfoUI.activeCharacter)) {
    //        characterInfoUI.activeCharacter.CenterOnCharacter();
    //    }
    //}
    //private void OnPartyDoneTravelling(Party party) {
    //    if (characterInfoUI.isShowing && party.characters.Contains(characterInfoUI.activeCharacter)) {
    //        characterInfoUI.activeCharacter.CenterOnCharacter();
    //    }
    //}
    public void OnCameraOutOfFocus() {
        if (characterInfoUI.isShowing) {
            characterInfoUI.OnClickCloseMenu();
        }
    }
    #endregion

    #region Region Info
    [Space(10)]
    [Header("Region Info")]
    public RegionInfoUI regionInfoUI;
    public void ShowRegionInfo(Region region, bool centerOnRegion = true) {
        if (factionInfoUI.isShowing) {
            factionInfoUI.CloseMenu();
        }
        if (characterInfoUI.isShowing) {
            characterInfoUI.CloseMenu();
        }
        if (tileObjectInfoUI.isShowing) {
            tileObjectInfoUI.CloseMenu();
        }
        if (areaInfoUI.isShowing) {
            areaInfoUI.CloseMenu();
        }
        regionInfoUI.SetData(region);
        regionInfoUI.OpenMenu();

        if (centerOnRegion) {
            region.CenterCameraOnRegion();
            region.ShowSolidBorder();
        }
    }
    public void UpdateRegionInfo() {
        if (regionInfoUI.isShowing) {
            regionInfoUI.UpdateInfo();
        }
    }
    #endregion

    #region Hextile Info
    //public bool ShowRegionInfo(HexTile hexTile) {
    //    if (hexTile.region != null && hexTile == hexTile.region.coreTile) {
    //        //if (hexTile.areaOfTile != null && hexTile.areaOfTile.areaType.IsSettlementType()) {
    //        //    //if (hexTile.areaOfTile.coreTile == hexTile && hexTile.areaOfTile == PlayerManager.Instance.player.playerArea) {
    //        //    //    portalPopup.SetActive(true);
    //        //    //    return true;
    //        //    //}
    //        //    ShowAreaInfo(hexTile);
    //        //    return true;
    //        //} else {
    //            //This is an exception in showing area info ui
    //            //Usually, area info ui must only be shown if the tile's region has an area
    //            //But for demonic landmarks, even if the region has no area, area info ui will still be shown
    //            //The area that will become active is the playerArea
    //            //This is done so that the player can build, research, etc.
    //            //if (hexTile.region.owner == PlayerManager.Instance.player.playerFaction) {
    //            //    ShowAreaInfo(hexTile);
    //            //    return true;
    //            //} else {
    //                ShowRegionInfo(hexTile.region);
    //                return true;
    //            //}
    //        //}
    //    }
    //    return false;
    //}
    #endregion

    #region Tile Object Info
    [Space(10)]
    [Header("Tile Object Info")]
    [SerializeField]
    internal TileObjectInfoUI tileObjectInfoUI;
    public void ShowTileObjectInfo(TileObject tileObject) {
        if (tempDisableShowInfoUI) {
            SetTempDisableShowInfoUI(false);
            return;
        }
        if (factionInfoUI.isShowing) {
            factionInfoUI.CloseMenu();
        }
        if (characterInfoUI.isShowing) {
            characterInfoUI.CloseMenu();
        }
        if (areaInfoUI.isShowing) {
            areaInfoUI.CloseMenu();
        }
        if (regionInfoUI.isShowing) {
            regionInfoUI.CloseMenu();
        }
        tileObjectInfoUI.SetData(tileObject);
        tileObjectInfoUI.OpenMenu();
    }
    public void UpdateTileObjectInfo() {
        if (tileObjectInfoUI.isShowing) {
            tileObjectInfoUI.UpdateTileObjectInfo();
        }
    }
    #endregion

    #region Combat Info
    [Space(10)]
    [Header("Combat History")]
    [SerializeField] internal CombatLogsUI combatLogUI;
    public void ShowCombatLog(Combat combat) {
        //if(questLogUI.isShowing){
        //	questLogUI.HideQuestLogs ();
        //}
        combatLogUI.ShowCombatLogs(combat);
    }
    //public void UpdateCombatLogs() {
    //    if (combatLogUI.isShowing) {
    //        combatLogUI.UpdateCombatLogs();
    //    }
    //}
    #endregion

    #region Quest Info
    [Space(10)]
    [Header("Quest UI")]
    public QuestInfoUI questInfoUI;
    public void ShowQuestInfo(Quest quest) {
        questInfoUI.ShowQuestInfoUI(quest);
    }
    public void UpdateQuestInfo() {
        if (questInfoUI.gameObject.activeSelf) {
            questInfoUI.UpdateQuestInfo();
        }
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

    #region Tile Hover
    //private HexTile previousTileHovered;
    private HexTile currentTileHovered;
    private float timeHovered;
    private const float hoverThreshold = 1.5f;
    private bool isHoveringTile = false;
    private void OnHoverOverTile(HexTile tile) {
        //previousTileHovered = currentTileHovered;
        currentTileHovered = tile;
        isHoveringTile = true;
    }
    public void OnHoverOutTile(HexTile tile) {
        currentTileHovered = null;
        isHoveringTile = false;
        tile.region?.OnHoverOutAction();
        if (tile.areaOfTile != null) {
            HideSmallInfo();
            isShowingAreaTooltip = false;
        }
    }
    #endregion

    #region Player
    private void OnCombatDone(Combat combat) {
        ShowDeveloperNotification("Combat at <b>" + combat.location.name + "</b>!", 5, () => ShowCombatLog(combat));
    }
    #endregion

    public void ShowMinionsMenu() {
        minionsMenuToggle.isOn = true;
    }
    public void ShowCharacterTokenMenu() {
        charactersMenuToggle.isOn = true;
    }
    public void ShowLocationTokenMenu() {
        locationsMenuToggle.isOn = true;
    }
    public void ShowFactionTokenMenu() {
        factionsMenuToggle.isOn = true;
    }
    public void HideRightMenus() {
        minionsMenuToggle.isOn = false;
        charactersMenuToggle.isOn = false;
        locationsMenuToggle.isOn = false;
        factionsMenuToggle.isOn = false;
    }

    public void OnMinionsMenuToggled(bool state) {
        if (!state) {
            if (!AreAllSideMenusAreClosed()) {
                PlayerUI.Instance.previousMenu = "minion";
            }
        }
    }
    public void OnCharacterTokenMenuToggled(bool state) {
        if (!state) {
            if (!AreAllSideMenusAreClosed()) {
                PlayerUI.Instance.previousMenu = "character";
            }
        }
    }
    public void OnLocationTokenMenuToggled(bool state) {
        if (!state) {
            if (!AreAllSideMenusAreClosed()) {
                PlayerUI.Instance.previousMenu = "location";
            }
        }
    }
    public void OnFactionTokenMenuToggled(bool state) {
        if (!state) {
            if (!AreAllSideMenusAreClosed()) {
                PlayerUI.Instance.previousMenu = "faction";
            }
        }
    }
    private bool AreAllSideMenusAreClosed() {
        if (!minionsMenuToggle.isOn && !charactersMenuToggle.isOn 
            && !locationsMenuToggle.isOn && !factionsMenuToggle.isOn) {
            PlayerUI.Instance.previousMenu = string.Empty;
            return true;
        }
        return false;
    }

    #region Area Map
    [SerializeField] private Button returnToWorldBtn;
    [SerializeField] private UIHoverPosition returnToWorldBtnTooltipPos;
    private void OnAreaMapOpened(Area area) {
        //returnToWorldBtn.interactable = true;
        //ShowPlayerNotificationArea();
        worldUIRaycaster.enabled = false;
    }
    private void OnAreaMapClosed(Area area) {
        //returnToWorldBtn.interactable = false;
        //HidePlayerNotificationArea();
        worldUIRaycaster.enabled = true;
    }
    //public void PointerClickWorldMap(BaseEventData bed) {
    //    //PointerEventData ped = bed as PointerEventData;
    //    //if (ped.clickCount == 2) {
    //        ReturnToWorlMap();
    //    //}
    //}
    public void ToggleBetweenMaps() {
        if (InteriorMapManager.Instance.isAnAreaMapShowing) {
            InteriorMapManager.Instance.HideAreaMap();
            OnCameraOutOfFocus();
        } else {
            InteriorMapManager.Instance.TryShowAreaMap(LandmarkManager.Instance.enemyOfPlayerArea);
        }
    }
    public void ToggleMapsHover() {
        if (InteriorMapManager.Instance.currentlyShowingArea != null) {
            ShowSmallInfo("Click to exit " + InteriorMapManager.Instance.currentlyShowingArea.name + ".", returnToWorldBtnTooltipPos);
        } else {
            ShowSmallInfo("Click to enter " + LandmarkManager.Instance.enemyOfPlayerArea.name + ".", returnToWorldBtnTooltipPos);
        }
    }
    #endregion

    #region Share Intel
    [Header("Share Intel")]
    [SerializeField] private ShareIntelMenu shareIntelMenu;
    public void OpenShareIntelMenu(Character targetCharacter) {
        shareIntelMenu.Open(targetCharacter);
    }
    public void OpenShareIntelMenu(Character targetCharacter, Character actor, Intel intel) {
        shareIntelMenu.Open(targetCharacter, actor, intel);
    }
    public bool IsShareIntelMenuOpen() {
        return shareIntelMenu.gameObject.activeSelf;
    }
    public void CloseShareIntelMenu() {
        shareIntelMenu.Close();
    }
    private void OnOpenShareIntelMenu() {
        returnToWorldBtn.interactable = false;
        SetCoverState(true);
        //playerNotificationParent.SetSiblingIndex(1);
    }
    private void OnCloseShareIntelMenu() {
        returnToWorldBtn.interactable = true;
        SetCoverState(false);
        //Unpause();
        //SetSpeedTogglesState(true);
        //playerNotificationParent.SetAsLastSibling();
    }
    #endregion

    #region Intel Notification
    [Header("Intel Notification")]
    [SerializeField] private RectTransform playerNotificationParent;
    [SerializeField] private GameObject intelPrefab;
    [SerializeField] private GameObject defaultNotificationPrefab;
    [SerializeField] private Button notifExpandButton;

    //public ScrollRect playerNotifScrollView;
    public GameObject playerNotifGO;
    public RectTransform playerNotificationScrollRectTransform;
    public ScrollRect playerNotifScrollRect;
    public Image[] playerNotifTransparentImages;
    public int maxPlayerNotif;
    public List<PlayerNotificationItem> activeNotifications = new List<PlayerNotificationItem>(); //notifications that are currently being shown.
    private void ShowPlayerNotification(Intel intel) {
        GameObject newIntelGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(intelPrefab.name, Vector3.zero, Quaternion.identity, playerNotifScrollRect.content);
        IntelNotificationItem newItem = newIntelGO.GetComponent<IntelNotificationItem>();
        newItem.Initialize(intel, true, OnNotificationDestroyed);
        newIntelGO.transform.localScale = Vector3.one;
        PlaceNewNotification(newItem);
    }
    private void ShowPlayerNotification(Log log) {
        GameObject newIntelGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(defaultNotificationPrefab.name, Vector3.zero, Quaternion.identity, playerNotifScrollRect.content);
        PlayerNotificationItem newItem = newIntelGO.GetComponent<PlayerNotificationItem>();
        newItem.Initialize(log, true, OnNotificationDestroyed);
        newIntelGO.transform.localScale = Vector3.one;
        PlaceNewNotification(newItem);        
    }
    public void ShowPlayerNotification(Log log, int tick) {
        GameObject newIntelGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(defaultNotificationPrefab.name, Vector3.zero, Quaternion.identity, playerNotifScrollRect.content);
        PlayerNotificationItem newItem = newIntelGO.GetComponent<PlayerNotificationItem>();
        newItem.Initialize(log, true, OnNotificationDestroyed);
        newItem.SetTickShown(tick);
        newIntelGO.transform.localScale = Vector3.one;
        PlaceNewNotification(newItem);
    }
    private void PlaceNewNotification(PlayerNotificationItem newNotif) {
        //check if the log used is from a GoapAction
        //then check all other currently showing notifications, if it is from the same goap action and the active character of both logs are the same.
        //replace that log with this new one
        PlayerNotificationItem itemToReplace = null;
        if (newNotif.shownLog != null && newNotif.shownLog.category == "GoapAction") {
            for (int i = 0; i < activeNotifications.Count; i++) {
                PlayerNotificationItem currItem = activeNotifications[i];
                if (currItem.shownLog.category == "GoapAction" && currItem.shownLog.file == newNotif.shownLog.file
                    && currItem.shownLog.HasFillerForIdentifier(LOG_IDENTIFIER.ACTIVE_CHARACTER) && newNotif.shownLog.HasFillerForIdentifier(LOG_IDENTIFIER.ACTIVE_CHARACTER)
                    && currItem.shownLog.GetFillerForIdentifier(LOG_IDENTIFIER.ACTIVE_CHARACTER).obj == newNotif.shownLog.GetFillerForIdentifier(LOG_IDENTIFIER.ACTIVE_CHARACTER).obj) {
                    itemToReplace = currItem;
                    break;
                }
            }
        }

        if (itemToReplace != null) {
            newNotif.SetTickShown(itemToReplace.tickShown);
            int index = (itemToReplace.transform as RectTransform).GetSiblingIndex();
            itemToReplace.DeleteNotification();
            (newNotif.gameObject.transform as RectTransform).SetSiblingIndex(index);
        }
        //else {
        //    (newNotif.gameObject.transform as RectTransform).SetAsLastSibling();
        //}
        activeNotifications.Add(newNotif);
        if (!notifExpandButton.gameObject.activeSelf) {
            //notifExpandButton.gameObject.SetActive(true);
        }
        if (activeNotifications.Count > maxPlayerNotif) {
            activeNotifications[0].DeleteNotification();
        }
    }
    private void OnNotificationDestroyed(PlayerNotificationItem item) {
        activeNotifications.Remove(item);
        if(activeNotifications.Count <= 0) {
            //notifExpandButton.gameObject.SetActive(false);
        }
    }
    public void OnClickExpand() {
        if(playerNotificationScrollRectTransform.sizeDelta.y == 950f) {
            playerNotificationScrollRectTransform.sizeDelta = new Vector2(playerNotificationScrollRectTransform.sizeDelta.x, 194f);
        }else if (playerNotificationScrollRectTransform.sizeDelta.y == 194f) {
            playerNotificationScrollRectTransform.sizeDelta = new Vector2(playerNotificationScrollRectTransform.sizeDelta.x, 950f);
        }
        //Canvas.ForceUpdateCanvases();
    }
    public void OnHoverNotificationArea() {
        for (int i = 0; i < playerNotifTransparentImages.Length; i++) {
            playerNotifTransparentImages[i].color = new Color(playerNotifTransparentImages[i].color.r, playerNotifTransparentImages[i].color.g, playerNotifTransparentImages[i].color.b, 120f/255f);
        }
    }
    public void OnHoverExitNotificationArea() {
        for (int i = 0; i < playerNotifTransparentImages.Length; i++) {
            playerNotifTransparentImages[i].color = new Color(playerNotifTransparentImages[i].color.r, playerNotifTransparentImages[i].color.g, playerNotifTransparentImages[i].color.b, 25f/255f);
        }
    }
    public void ShowPlayerNotificationArea() {
        Utilities.DestroyChildren(playerNotifScrollRect.content);
        playerNotifGO.SetActive(true);
    }
    public void HidePlayerNotificationArea() {
        playerNotifGO.SetActive(false);
    }
    private void MoveNotificationMenuToDefaultPos() {
        playerNotificationParent.anchoredPosition = new Vector2(506f, 14f);
    }
    private void MoveNotificationMenuToModifiedPos() {
        playerNotificationParent.anchoredPosition = new Vector2(930f, 14f);
    }
    #endregion

    #region Audio
    public void ToggleMute(bool state) {
        AudioManager.Instance.SetMute(state);
    }
    #endregion

    #region Controls
    public void ToggleEdgePanning(bool state) {
        CameraMove.Instance.AllowEdgePanning(state);
        AreaMapCameraMove.Instance.AllowEdgePanning(state);
    }
    #endregion

    #region Interior Map Loading
    [Header("Interior Map Loading")]
    [SerializeField] private GameObject interiorMapLoadingGO;
    [SerializeField] private Image interiorMapLoadingBGImage;
    [SerializeField] private GameObject interiorMapLoadingDetailsGO;
    public void SetInteriorMapLoadingState(bool state) {
        interiorMapLoadingGO.SetActive(state);
        //if (state) {
        //    interiorMapLoadingGO.SetActive(true);
        //    //StartCoroutine(LoadingCoroutine(128f/ 255f));
        //    //interiorMapLoadingTween.PlayForward();
        //} else {
        //    //interiorMapLoadingTween.PlayReverse();
        //    //interiorMapLoadingBGImage.CrossFadeAlpha(0, 0.5f, true);
        //    //StartCoroutine(LoadingCoroutine(0f / 255f));
        //}
    }
    #endregion

    #region World Events
    private void OnWorldEventSpawned(Region region, WorldEvent we) {
        //create world event popup
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(worldEventIconPrefab.name, Vector3.zero, Quaternion.identity, worldUIParent);
        WorldEventIcon icon = go.GetComponent<WorldEventIcon>();
        icon.PlaceAt(region);
        region.SetEventIcon(go);
    }
    private void OnWorldEventDespawned(Region region, WorldEvent we) {
        ObjectPoolManager.Instance.DestroyObject(region.eventIconGO);
    }
    #endregion

    #region Yes/No
    [Header("Yes or No Confirmation")]
    [SerializeField] private GameObject yesNoGO;
    [SerializeField] private GameObject yesNoCover;
    [SerializeField] private TextMeshProUGUI yesNoHeaderLbl;
    [SerializeField] private TextMeshProUGUI yesNoDescriptionLbl;
    [SerializeField] private Button yesBtn;
    [SerializeField] private Button noBtn;
    [SerializeField] private Button closeBtn;
    [SerializeField] private TextMeshProUGUI yesBtnLbl;
    [SerializeField] private TextMeshProUGUI noBtnLbl;
    [SerializeField] private UIHoverHandler yesBtnUnInteractableHoverHandler;
    public void ShowYesNoConfirmation(string header, string question, System.Action onClickYesAction = null, System.Action onClickNoAction = null,
        bool showCover = false, int layer = 21, string yesBtnText = "Yes", string noBtnText = "No", bool yesBtnInteractable = true, bool noBtnInteractable = true, bool pauseAndResume = false, 
        bool yesBtnActive = true, bool noBtnActive = true, System.Action yesBtnInactiveHoverAction = null, System.Action yesBtnInactiveHoverExitAction = null) {
        if (pauseAndResume) {
            SetSpeedTogglesState(false);
            Pause();
        }
        yesNoHeaderLbl.text = header;
        yesNoDescriptionLbl.text = question;

        yesBtnLbl.text = yesBtnText;
        noBtnLbl.text = noBtnText;

        yesBtn.gameObject.SetActive(yesBtnActive);
        noBtn.gameObject.SetActive(noBtnActive);

        yesBtn.interactable = yesBtnInteractable;
        noBtn.interactable = noBtnInteractable;

        //clear all listeners
        yesBtn.onClick.RemoveAllListeners();
        noBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.RemoveAllListeners();

        //hide confirmation menu on click
        yesBtn.onClick.AddListener(HideYesNoConfirmation);
        noBtn.onClick.AddListener(HideYesNoConfirmation);
        closeBtn.onClick.AddListener(HideYesNoConfirmation);

        //resume last prog speed on click any btn
        if (pauseAndResume) {
            yesBtn.onClick.AddListener(ResumeLastProgressionSpeed);
            noBtn.onClick.AddListener(ResumeLastProgressionSpeed);
            closeBtn.onClick.AddListener(ResumeLastProgressionSpeed);
        }

        //specific actions
        if (onClickYesAction != null) {
            yesBtn.onClick.AddListener(onClickYesAction.Invoke);
        }
        if (onClickNoAction != null) {
            noBtn.onClick.AddListener(onClickNoAction.Invoke);
            closeBtn.onClick.AddListener(onClickNoAction.Invoke);
        }

        yesBtnUnInteractableHoverHandler.gameObject.SetActive(!yesBtn.interactable);
        if (yesBtnInactiveHoverAction != null) {
            yesBtnUnInteractableHoverHandler.SetOnHoverAction(yesBtnInactiveHoverAction.Invoke);
        }
        if (yesBtnInactiveHoverExitAction != null) {
            yesBtnUnInteractableHoverHandler.SetOnHoverOutAction(yesBtnInactiveHoverExitAction.Invoke);
        }

        yesNoGO.SetActive(true);
        yesNoGO.transform.SetSiblingIndex(layer);
        yesNoCover.SetActive(showCover);
    }
    private void HideYesNoConfirmation() {
        yesNoGO.SetActive(false);
    }
    #endregion

    #region Important Notifications
    [Header("Important Notification")]
    [SerializeField] private ScrollRect importantNotifScrollView;
    [SerializeField] private GameObject importantNotifPrefab;
    public void ShowImportantNotification(GameDate date, string message, System.Action onClickAction) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(importantNotifPrefab.name, Vector3.zero, Quaternion.identity, importantNotifScrollView.content);
        ImportantNotificationItem item = go.GetComponent<ImportantNotificationItem>();
        item.Initialize(date, message, onClickAction);
    }
    #endregion

    #region Minion Card Info
    [Space(10)]
    [Header("Minion Card Info")]
    [SerializeField] private MinionCard minionCardTooltip;
    [SerializeField] private RectTransform minionCardRT;
    public void ShowMinionCardTooltip(Minion minion, UIHoverPosition position = null) {
        if (minionCardTooltip.minion != minion) {
            minionCardTooltip.SetMinion(minion);
        }
        if (!minionCardTooltip.gameObject.activeSelf) {
            minionCardTooltip.gameObject.SetActive(true);
        }
        if (position != null) {
            PositionMinionCardTooltip(position);
        } else {
            PositionMinionCardTooltip(Input.mousePosition);
        }
    }
    public void ShowMinionCardTooltip(UnsummonedMinionData minion, UIHoverPosition position = null) {
        if (!minionCardTooltip.minionData.Equals(minion)) {
            minionCardTooltip.SetMinion(minion);
        }
        if (!minionCardTooltip.gameObject.activeSelf) {
            minionCardTooltip.gameObject.SetActive(true);
        }
        if (position != null) {
            PositionMinionCardTooltip(position);
        } else {
            PositionMinionCardTooltip(Input.mousePosition);
        }
    }
    public void HideMinionCardTooltip() {
        minionCardTooltip.gameObject.SetActive(false);
    }
    private void PositionMinionCardTooltip(Vector3 screenPos) {
        minionCardTooltip.transform.SetParent(this.transform);
        var v3 = screenPos;

        minionCardRT.pivot = new Vector2(1f, 1f);

        //if (CursorManager.Instance.currentCursorType == CursorManager.Cursor_Type.Cross || CursorManager.Instance.currentCursorType == CursorManager.Cursor_Type.Check) {
        //    v3.x += 100f;
        //    v3.y -= 32f;
        //} else {
        //    v3.x += 25f;
        //    v3.y -= 25f;
        //}

        minionCardRT.transform.position = v3;

        //Vector3[] corners = new Vector3[4]; //bottom-left, top-left, top-right, bottom-right
        //List<int> cornersOutside = new List<int>();
        //boundsRT.GetWorldCorners(corners);
        //for (int i = 0; i < 4; i++) {
        //    // Backtransform to parent space
        //    Vector3 localSpacePoint = mainRT.InverseTransformPoint(corners[i]);
        //    // If parent (canvas) does not contain checked items any point
        //    if (!mainRT.rect.Contains(localSpacePoint)) {
        //        cornersOutside.Add(i);
        //    }
        //}

        //if (cornersOutside.Count != 0) {
        //    string log = "Corners outside are: ";
        //    for (int i = 0; i < cornersOutside.Count; i++) {
        //        log += cornersOutside[i].ToString() + ", ";
        //    }
        //    //Debug.Log(log);
        //    if (cornersOutside.Contains(2) && cornersOutside.Contains(3)) {
        //        if (cornersOutside.Contains(0)) {
        //            //bottom side and right side are outside, move anchor to bottom right
        //            rtToReposition.pivot = new Vector2(1f, 0f);
        //            smallInfoBGParentLG.childAlignment = TextAnchor.LowerRight;
        //        } else {
        //            //right side is outside, move anchor to top right side
        //            rtToReposition.pivot = new Vector2(1f, 1f);
        //            smallInfoBGParentLG.childAlignment = TextAnchor.UpperRight;
        //        }
        //    } else if (cornersOutside.Contains(0) && cornersOutside.Contains(3)) {
        //        //bottom side is outside, move anchor to bottom left
        //        rtToReposition.pivot = new Vector2(0f, 0f);
        //        smallInfoBGParentLG.childAlignment = TextAnchor.LowerLeft;
        //    }
        //    rtToReposition.localPosition = Vector3.zero;
        //}
    }
    private void PositionMinionCardTooltip(UIHoverPosition position) {
        minionCardTooltip.transform.SetParent(position.transform);
        RectTransform tooltipParentRT = minionCardTooltip.transform as RectTransform;
        tooltipParentRT.pivot = position.pivot;

        Vector2 anchorMin = Vector2.zero;
        Vector2 anchorMax = Vector2.zero;
        Utilities.GetAnchorMinMax(position.anchor, ref anchorMin, ref anchorMax);
        tooltipParentRT.anchorMin = anchorMin;
        tooltipParentRT.anchorMax = anchorMax;
        tooltipParentRT.anchoredPosition = Vector2.zero;
    }
    #endregion
}