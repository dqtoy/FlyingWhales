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

    public RectTransform mainRT;
    [SerializeField] private EventSystem eventSystem;

    [Space(10)]
    [SerializeField] UIMenu[] allMenus;

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
    [Header("Nameplates")]
    [SerializeField] private RectTransform nameplateParent;

    [Header("Object Picker")]
    [SerializeField] private ObjectPicker objectPicker;

    [Space(10)] //FOR TESTING
    [Header("For Testing")]
    public ButtonToggle toggleBordersBtn;
    public ButtonToggle corruptionBtn;
    public POITestingUI poiTestingUI;

    public delegate void OnPauseEventExpiration(bool state);
    public OnPauseEventExpiration onPauseEventExpiration;

    [Space(10)]
    [Header("Font Sizes")]
    [SerializeField] private int HEADER_FONT_SIZE = 25;
    [SerializeField] private int BODY_FONT_SIZE = 20;
    [SerializeField] private int TOOLTIP_FONT_SIZE = 18;
    [SerializeField] private int SMALLEST_FONT_SIZE = 12;

    [Space(10)]
    [Header("Combat")]
    public CombatUI combatUI;

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
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (contextMenu.gameObject.activeSelf) {
                HideContextMenu();
            }
        } else 
        //if (Input.GetKeyDown(KeyCode.Space) && !IsMouseOnInput()) {
        //    if (pauseBtn.IsInteractable()) {
        //        if (GameManager.Instance.isPaused) {
        //            Unpause();
        //        } else {
        //            Pause();
        //        }
        //    }
        //} else 
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            PlayerUI.Instance.ScrollRoleSlotTo(0);
        } else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            PlayerUI.Instance.ScrollRoleSlotTo(1);
        } else if (Input.GetKeyDown(KeyCode.Alpha3)) {
            PlayerUI.Instance.ScrollRoleSlotTo(2);
        } else if (Input.GetKeyDown(KeyCode.Alpha4)) {
            PlayerUI.Instance.ScrollRoleSlotTo(3);
        } else if (Input.GetKeyDown(KeyCode.Alpha5)) {
            PlayerUI.Instance.ScrollRoleSlotTo(4);
        }
        UpdateSpeedToggles(GameManager.Instance.isPaused);
        if (isHoveringTile) {
            if (currentTileHovered.areaOfTile != null && currentTileHovered.areaOfTile.areaType != AREA_TYPE.DEMONIC_INTRUSION) {
                ShowSmallInfo("Double click to view.", currentTileHovered.areaOfTile.name);
                isShowingAreaTooltip = true;
            }
        }
    }
    #endregion

    public void ExitGame() {
        Application.Quit();
    }

    public void SetTimeControlsState(bool state) {
        pauseBtn.interactable = state;
        x1Btn.interactable = state;
        x2Btn.interactable = state;
        x4Btn.interactable = state;
    }
    internal void InitializeUI() {
        for (int i = 0; i < allMenus.Length; i++) {
            allMenus[i].Initialize();
            //allMenus[i].ApplyUnifiedSettings(settings);
        }
        //Image[] images = this.gameObject.GetComponentsInChildren<Image>();
        //for (int i = 0; i < images.Length; i++) {
        //    images[i].alphaHitTestMinimumThreshold = 1f;
        //}
        //UnifySelectables();
        //popupMessageBox.Initialize();
        Messenger.AddListener<HexTile>(Signals.TILE_RIGHT_CLICKED, ShowContextMenu);
        Messenger.AddListener(Signals.HIDE_MENUS, HideMenus);
        Messenger.AddListener<string, int, UnityAction>(Signals.SHOW_DEVELOPER_NOTIFICATION, ShowDeveloperNotification);

        Messenger.AddListener<HexTile>(Signals.TILE_HOVERED_OVER, OnHoverOverTile);
        Messenger.AddListener<HexTile>(Signals.TILE_HOVERED_OUT, OnHoverOutTile);

        Messenger.AddListener<Combat>(Signals.COMBAT_DONE, OnCombatDone);
        //Messenger.AddListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
        //Messenger.AddListener<IInteractable, Interaction>(Signals.ADDED_INTERACTION, OnInteractionAdded);

        Messenger.AddListener(Signals.INTERACTION_MENU_OPENED, OnInteractionMenuOpened);
        Messenger.AddListener(Signals.INTERACTION_MENU_CLOSED, OnInteractionMenuClosed);
        Messenger.AddListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnPartyStartedTravelling);
        Messenger.AddListener<Party>(Signals.PARTY_DONE_TRAVELLING, OnPartyDoneTravelling);
        //Messenger.AddListener(Signals.CAMERA_OUT_OF_FOCUS, OnCameraOutOfFocus);
        Messenger.AddListener<Area>(Signals.AREA_MAP_OPENED, OnAreaMapOpened);
        Messenger.AddListener<Area>(Signals.AREA_MAP_CLOSED, OnAreaMapClosed);

        Messenger.AddListener<Intel>(Signals.SHOW_INTEL_NOTIFICATION, ShowPlayerNotification);
        Messenger.AddListener<Log>(Signals.SHOW_PLAYER_NOTIFICATION, ShowPlayerNotification);

        Messenger.AddListener(Signals.ON_OPEN_SHARE_INTEL, OnOpenShareIntelMenu);
        Messenger.AddListener(Signals.ON_CLOSE_SHARE_INTEL, OnCloseShareIntelMenu);
        Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);
        UpdateUI();
    }
    private void OnGameLoaded() {
        UpdateUI();
    }
    private void HideMenus() {
        HideContextMenu();
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
        if (lblName.Contains("NOTOUCH")) {
            return;
        }
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
        //dateLbl.SetText(GameManager.Instance.continuousDays + "/" + GameManager.ConvertTickToTime(GameManager.Instance.tick));
        dateLbl.SetText("Day " + GameManager.Instance.continuousDays + "\n" + GameManager.ConvertTickToTime(GameManager.Instance.tick));
        //timeLbl.SetText(GameManager.GetTimeInWordsOfTick(GameManager.Instance.tick).ToString());
        timeLbl.SetText("");

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
                SetProgressionSpeed1X();
            } else if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X2) {
                x2Btn.isOn = true;
                speedToggleGroup.NotifyToggleOn(x2Btn);
                SetProgressionSpeed2X();
            } else if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X4) {
                x4Btn.isOn = true;
                speedToggleGroup.NotifyToggleOn(x4Btn);
                SetProgressionSpeed4X();
            }
        }
    }
    public void SetProgressionSpeed1X() {
        if (!x1Btn.IsInteractable()) {
            return;
        }
        GameManager.Instance.SetProgressionSpeed(PROGRESSION_SPEED.X1);
        Unpause();
    }
    public void SetProgressionSpeed2X() {
        if (!x2Btn.IsInteractable()) {
            return;
        }
        GameManager.Instance.SetProgressionSpeed(PROGRESSION_SPEED.X2);
        Unpause();
    }
    public void SetProgressionSpeed4X() {
        if (!x4Btn.IsInteractable()) {
            return;
        }
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
        //Debug.Log("Unpaused from:\n " + StackTraceUtility.ExtractStackTrace());
        GameManager.Instance.SetPausedState(false);
        if (onPauseEventExpiration != null) {
            onPauseEventExpiration(false);
        }
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

        if (CursorManager.Instance.currentCursorType == CursorManager.Cursor_Type.Cross || CursorManager.Instance.currentCursorType == CursorManager.Cursor_Type.Check) {
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
    public void ShowSmallLocationInfo(Area area, RectTransform initialParent, Vector2 adjustment, string subText = "") {
        locationSmallInfo.ShowAreaInfo(area, subText);
        locationSmallInfoRT.SetParent(initialParent);
        locationSmallInfoRT.anchoredPosition = Vector3.zero;
        locationSmallInfoRT.anchoredPosition += adjustment;
        locationSmallInfoRT.SetParent(this.transform);
        //(locationSmallInfo.transform as RectTransform).anchoredPosition = pos;
    }
    public void ShowSmallLocationInfo(Area area, Vector3 pos, string subText = "") {
        locationSmallInfo.ShowAreaInfo(area, subText);
        locationSmallInfoRT.position = pos;
    }
    public void HideSmallLocationInfo() {
        locationSmallInfo.Hide();
    }
    public bool IsSmallLocationInfoShowing() {
        return locationSmallInfoRT.gameObject.activeSelf;
    }
    public Area GetCurrentlyShowingSmallInfoLocation() {
        if (IsSmallLocationInfoShowing()) {
            return locationSmallInfo.area;
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
        //GameObject go = GameObject.Instantiate (prefabObj, parent) as GameObject;
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(prefabObjName, Vector3.zero, Quaternion.identity, parent);
        TextMeshProUGUI[] goLbls = go.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < goLbls.Length; i++) {
            NormalizeFontSizeOfLabel(goLbls[i]);
        }
        return go;
    }
    #endregion

    #region Nameplate
    public void CreateAreaNameplate(Area area) {
        GameObject nameplateGO = UIManager.Instance.InstantiateUIObject("AreaNameplate", nameplateParent);
        //nameplateGO.transform.localScale = new Vector3(0.02f, 0.02f, 1f);
        nameplateGO.GetComponent<AreaNameplate>().SetArea(area);
    }
    #endregion

    #region Object Picker
    public void ShowClickableObjectPicker<T>(List<T> choices, Action<T> onClickAction, IComparer<T> comparer = null, Func<T, bool> validityChecker = null, string title = "", Action<T> onHoverAction = null) {
        objectPicker.ShowClickable(choices, onClickAction, comparer, validityChecker, title, onHoverAction);
        //Pause();
        //SetSpeedTogglesState(false);
    }
    public void ShowDraggableObjectPicker<T>(List<T> choices, IComparer<T> comparer = null, Func<T, bool> validityChecker = null, string title = "") {
        objectPicker.ShowDraggable(choices, comparer, validityChecker, title);
    }
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
    #endregion

    #region Area Info
    [Space(10)]
    [Header("Area Info")]
    [SerializeField]
    internal AreaInfoUI areaInfoUI;
    public void ShowAreaInfo(Area area, int indexToggleToBeActivated = 0) {
        if (PlayerManager.Instance.player.homeArea == area) {
            UIManager.Instance.portalPopup.SetActive(true);
        } else {
            if (factionInfoUI.isShowing) {
                factionInfoUI.CloseMenu();
            }
            if (characterInfoUI.isShowing) {
                characterInfoUI.CloseMenu();
            }
            if (tileObjectInfoUI.isShowing) {
                tileObjectInfoUI.CloseMenu();
            }
            areaInfoUI.SetData(area);
            areaInfoUI.OpenMenu();
            areaInfoUI.CenterOnCoreLandmark();
        }
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
        //BeforeOpeningMenu(factionInfoUI);
        //HideMainUI();
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
    public void ShowCharacterInfo(Character character) {
        //BeforeOpeningMenu(characterInfoUI);
        //HideMainUI();
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
        //if (hexTileInfoUI.isShowing) {
        //    hexTileInfoUI.HideMenu();
        //}
        //if (questInfoUI.isShowing) {
        //    questInfoUI.HideMenu();
        //}
        //if (partyinfoUI.isShowing) {
        //    partyinfoUI.CloseMenu();
        //}

        characterInfoUI.SetData(character);
        //if(character.role.roleType != CHARACTER_ROLE.PLAYER) {
        characterInfoUI.OpenMenu();
        //} else {
        //    characterInfoUI.CloseMenu();
        //}
        //character.CenterOnCharacter();
        //		playerActionsUI.ShowPlayerActionsUI ();
    }
    public void UpdateCharacterInfo() {
        if (characterInfoUI.isShowing) {
            characterInfoUI.UpdateCharacterInfo();
        }
    }
    private void OnPartyStartedTravelling(Party party) {
        if(characterInfoUI.isShowing && party.characters.Contains(characterInfoUI.activeCharacter)) {
            characterInfoUI.activeCharacter.CenterOnCharacter();
        }
    }
    private void OnPartyDoneTravelling(Party party) {
        if (characterInfoUI.isShowing && party.characters.Contains(characterInfoUI.activeCharacter)) {
            characterInfoUI.activeCharacter.CenterOnCharacter();
        }
    }
    private void OnCameraOutOfFocus() {
        if (characterInfoUI.isShowing) {
            characterInfoUI.OnClickCloseMenu();
        }
    }
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

    #region Context Menu
    [Space(10)]
    [Header("Context Menu")]
    public GameObject contextMenuPrefab;
    public GameObject contextMenuItemPrefab;
    public UIContextMenu contextMenu;
    private void ShowContextMenu(HexTile tile) {
        if (PlayerManager.Instance.isChoosingStartingTile) {
            //|| landmarkInfoUI.isWaitingForAttackTarget
            return;
        }
        ContextMenuSettings settings = tile.GetContextMenuSettings();
        if (settings.items.Count > 0) {
            contextMenu.LoadSettings(settings);
            contextMenu.gameObject.SetActive(true);
            //Vector2 pos;
            //RectTransformUtility.ScreenPointToLocalPointInRectangle(this.transform as RectTransform, Input.mousePosition, eventSystem.camera, out pos);
            //contextMenu.transform.position = Input.mousePosition;
            PositionTooltip(contextMenu.gameObject, contextMenu.transform as RectTransform, contextMenu.transform as RectTransform);
        }
        
    }
    public void HideContextMenu() {
        contextMenu.gameObject.SetActive(false);
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

    #region Interaction
    public void ShowInteractableInfo(BaseLandmark interactable) {
        ShowAreaInfo(interactable.tileLocation.areaOfTile);
        //if (interactable is BaseLandmark) {
        //    ShowLandmarkInfo(interactable);
        //} else if (interactable is Character) {
        //    ShowCharacterInfo(interactable as Character);
        //}
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
    private void OnAreaMapOpened(Area area) {
        returnToWorldBtn.interactable = true;
        ShowPlayerNotificationArea();
    }
    private void OnAreaMapClosed(Area area) {
        returnToWorldBtn.interactable = false;
        HidePlayerNotificationArea();
    }
    //public void PointerClickWorldMap(BaseEventData bed) {
    //    //PointerEventData ped = bed as PointerEventData;
    //    //if (ped.clickCount == 2) {
    //        ReturnToWorlMap();
    //    //}
    //}
    public void ReturnToWorlMap() {
        InteriorMapManager.Instance.HideAreaMap();
        OnCameraOutOfFocus();
    }
    public void ReturnToWorldMapHover() {
        if (InteriorMapManager.Instance.currentlyShowingArea != null) {
            ShowSmallInfo("Click to exit " + InteriorMapManager.Instance.currentlyShowingArea.name + ".");
        }
    }
    #endregion

    #region Share Intel
    [Header("Share Intel")]
    [SerializeField] private ShareIntelMenu shareIntelMenu;
    public void OpenShareIntelMenu(Character targetCharacter, Character actor) {
        shareIntelMenu.Open(targetCharacter, actor);
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
        playerNotificationParent.SetSiblingIndex(1);
    }
    private void OnCloseShareIntelMenu() {
        returnToWorldBtn.interactable = true;
        SetCoverState(false);
        //Unpause();
        //SetSpeedTogglesState(true);
        playerNotificationParent.SetAsLastSibling();
    }
    #endregion

    #region Intel Notification
    [Header("Intel Notification")]
    [SerializeField] private RectTransform playerNotificationParent;
    [SerializeField] private GameObject intelPrefab;
    [SerializeField] private GameObject defaultNotificationPrefab;
    //public ScrollRect playerNotifScrollView;
    public GameObject playerNotifGO;
    public RectTransform playerNotificationScrollRectTransform;
    public ScrollRect playerNotifScrollRect;
    public Image[] playerNotifTransparentImages;

    private List<PlayerNotificationItem> activeNotifications = new List<PlayerNotificationItem>(); //notifications that are currently being shown.
    private void ShowPlayerNotification(Intel intel) {
        GameObject newIntelGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(intelPrefab.name, Vector3.zero, Quaternion.identity, playerNotifScrollRect.content);
        IntelNotificationItem newItem = newIntelGO.GetComponent<IntelNotificationItem>();
        newItem.Initialize(intel, true, OnNotificationDestroyed);
        newIntelGO.transform.localScale = Vector3.one;
        PlaceNewNotificaton(newItem);
    }
    private void ShowPlayerNotification(Log log) {
        GameObject newIntelGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(defaultNotificationPrefab.name, Vector3.zero, Quaternion.identity, playerNotifScrollRect.content);
        PlayerNotificationItem newItem = newIntelGO.GetComponent<PlayerNotificationItem>();
        newItem.Initialize(log, true, OnNotificationDestroyed);
        newIntelGO.transform.localScale = Vector3.one;
        PlaceNewNotificaton(newItem);        
    }
    private void PlaceNewNotificaton(PlayerNotificationItem newNotif) {
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
            int index = (itemToReplace.transform as RectTransform).GetSiblingIndex();
            itemToReplace.DeleteNotification();
            (newNotif.gameObject.transform as RectTransform).SetSiblingIndex(index);
        } 
        //else {
        //    (newNotif.gameObject.transform as RectTransform).SetAsFirstSibling();
        //}
        activeNotifications.Add(newNotif);
    }
    private void OnNotificationDestroyed(PlayerNotificationItem item) {
        activeNotifications.Remove(item);
    }
    public void OnClickExpand() {
        if(playerNotificationScrollRectTransform.sizeDelta.y == 1000f) {
            playerNotificationScrollRectTransform.sizeDelta = new Vector2(playerNotificationScrollRectTransform.sizeDelta.x, 194f);
        }else if (playerNotificationScrollRectTransform.sizeDelta.y == 194f) {
            playerNotificationScrollRectTransform.sizeDelta = new Vector2(playerNotificationScrollRectTransform.sizeDelta.x, 1000f);
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
    #endregion

    #region Audio
    public void ToggleMute(bool state) {
        AudioManager.Instance.SetMute(state);
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
}