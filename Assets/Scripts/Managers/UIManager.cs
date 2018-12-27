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
    [Header("Prefabs")]
    [SerializeField] private GameObject notificationPrefab;

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
    public RectTransform smallInfoRT;
    public TextMeshProUGUI smallInfoLbl;
    public EnvelopContentUnityUI smallInfoEnvelopContent;

    [Space(10)]
    [Header("Detailed Info")]
    public GameObject detailedInfoGO;
    public RectTransform detailedInfoRect;
    public TextMeshProUGUI detailedInfoLbl;
    public Image detailedInfoIcon;
    public RectTransform detailedInfoContentParent;
    public CharacterPortrait[] detailedInfoPortraits;

    [Space(10)]
    [Header("Popup Message Box")]
    [SerializeField] private PopupMessageBox popupMessageBox;

    [Space(10)]
    [Header("Notification Area")]
    [SerializeField] private PlayerNotificationArea notificationArea;

    [Space(10)]
    [Header("Portraits")]
    public Transform characterPortraitsParent;

    [Space(10)]
    [Header("Player")]
    [SerializeField] private ScrollRect playerPickerScroll;
    [SerializeField] private Transform playerPickerContentTransform;
    [SerializeField] private GameObject playerPickerGO;
    [SerializeField] private GameObject playerPickerButtonPrefab;
    [SerializeField] private Toggle minionsMenuToggle;
    [SerializeField] private Toggle charactersMenuToggle;
    [SerializeField] private Toggle locationsMenuToggle;
    [SerializeField] private Toggle factionsMenuToggle;

    [Space(10)]
    [Header("Shared")]
    [SerializeField] private GameObject cover;

    private List<PlayerPickerButton> currentActivePlayerPickerButtons;

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
    private UIMenu lastOpenedMenu = null;

    #region Monobehaviours
    private void Awake() {
        Instance = this;
        //_menuHistory = new List<UIMenuSettings>();
        Messenger.AddListener<bool>(Signals.PAUSED, UpdateSpeedToggles);
    }
    private void Start() {
        currentActivePlayerPickerButtons = new List<PlayerPickerButton>();
        Messenger.AddListener(Signals.UPDATE_UI, UpdateUI);
        Messenger.AddListener(Signals.INSPECT_ALL, UpdateInteractableInfoUI);
        //NormalizeFontSizes();
        ToggleBorders();
    }
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (contextMenu.gameObject.activeSelf) {
                HideContextMenu();
            }
        }
        //if (!IsConsoleShowing() && !IsMouseOnInput() && !PlayerManager.Instance.isChoosingStartingTile) {
        //    if (Input.GetKeyDown(KeyCode.Space)) {
        //        if (GameManager.Instance.isPaused) {
        //            //SetProgressionSpeed(currProgressionSpeed);
        //            //SetPausedState(false);
        //            if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X1) {
        //                SetProgressionSpeed1X();
        //            } else if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X2) {
        //                SetProgressionSpeed2X();
        //            } else if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X4) {
        //                SetProgressionSpeed4X();
        //            }
        //        } else {
        //            //pause
        //            //SetPausedState(true);
        //            Pause();
        //        }
        //    }
        //}
        UpdateSpeedToggles(GameManager.Instance.isPaused);
        //if (currentTileHovered != null) {
        //    if (previousTileHovered == null || currentTileHovered.id != previousTileHovered.id) {
        //        //tile hovered changed, reset timer
        //        timeHovered = 0f;
        //    } else {
        //        //previous tile hovered is same as current tile hovered, increment time hovered
        //        timeHovered += Time.deltaTime;
        //    }
        //    if (IsMouseOnUI()) {
        //        timeHovered = 0f;
        //        HideDetailedInfo();
        //    } else {
        //        if (timeHovered >= hoverThreshold) {
        //            //show tile info
        //            ShowDetailedInfo(currentTileHovered);
        //        } else {
        //            //hide Tile info
        //            HideDetailedInfo();
        //        }
        //    }
           
        //}
        //if (IsMouseOnUI()) {
        //    currentTileHovered = null;
        //}
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
            //allMenus[i].ApplyUnifiedSettings(settings);
        }
        //UnifySelectables();
        //popupMessageBox.Initialize();
        Messenger.AddListener<HexTile>(Signals.TILE_RIGHT_CLICKED, ShowContextMenu);
        Messenger.AddListener(Signals.HIDE_MENUS, HideMenus);
        Messenger.AddListener<string, int, UnityAction>(Signals.SHOW_NOTIFICATION, ShowNotification);

        Messenger.AddListener<HexTile>(Signals.TILE_HOVERED_OVER, OnHoverOverTile);
        Messenger.AddListener<HexTile>(Signals.TILE_HOVERED_OUT, OnHoverOutTile);

        Messenger.AddListener<Token>(Signals.TOKEN_ADDED, OnTokenAdded);
        Messenger.AddListener<Combat>(Signals.COMBAT_DONE, OnCombatDone);
        //Messenger.AddListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
        //Messenger.AddListener<IInteractable, Interaction>(Signals.ADDED_INTERACTION, OnInteractionAdded);

        Messenger.AddListener(Signals.INTERACTION_MENU_OPENED, OnInteractionMenuOpened);
        Messenger.AddListener(Signals.INTERACTION_MENU_CLOSED, OnInteractionMenuClosed);
    }
    //public void UnifySelectables() {
    //    UnifiedSelectableBehaviour[] selectables = this.GetComponentsInChildren<UnifiedSelectableBehaviour>(true);
    //    for (int i = 0; i < selectables.Length; i++) {
    //        selectables[i].Initialize();
    //    }
    //}
    private void HideMenus() {
        HideContextMenu();
        if (characterInfoUI.isShowing) {
            characterInfoUI.CloseMenu();
        }
        if (landmarkInfoUI.isShowing) {
            landmarkInfoUI.CloseMenu();
        }
        if (monsterInfoUI.isShowing) {
            monsterInfoUI.CloseMenu();
        }
        if (partyinfoUI.isShowing) {
            partyinfoUI.CloseMenu();
        }
        if (playerLandmarkInfoUI.isShowing) {
            playerLandmarkInfoUI.CloseMenu();
        }
        if (areaInfoUI.isShowing) {
            areaInfoUI.CloseMenu();
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
        dateLbl.SetText("Day " + GameManager.Instance.continuousDays);

        UpdateInteractableInfoUI();
        UpdateFactionInfo();
        //UpdateHexTileInfo();
        UpdatePartyInfo();
        //UpdateCombatLogs();
        //UpdateQuestSummary();
        PlayerUI.Instance.UpdateUI();
    }
    private void UpdateInteractableInfoUI() {
        UpdateCharacterInfo();
        UpdateLandmarkInfo();
        UpdateMonsterInfo();
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
        if (!pauseBtn.IsInteractable()) {
            return;
        }
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
    public void ShowSmallInfo(string info, string header = "", RectTransform position = null) {
        string message = string.Empty;
        if (!string.IsNullOrEmpty(header)) {
            message = "<font=\"Eczar-Medium\"><line-height=100%><size=18>" + header + "</font>\n";
        }
        message += "<line-height=70%><size=16>" + info;

        message = message.Replace("\\n", "\n");

        smallInfoLbl.text = message;
        smallInfoGO.SetActive(true);
        smallInfoEnvelopContent.Execute();
        if (position == null) {
            smallInfoRT.SetParent(this.transform);
            PositionTooltip(smallInfoRT);
        } else {
            smallInfoRT.SetParent(position);
            smallInfoRT.anchoredPosition = Vector2.zero;
            //smallInfoRT.anchoredPosition = pos;
            //smallInfoRT.position = new Vector3(pos.x, pos.y, 0f);
        }
        
        //Debug.Log("Show small info " + info);
    }
    public void HideSmallInfo() {
        smallInfoGO.SetActive(false);
        //smallInfoGO.transform.parent = this.transform;
    }
    public void ShowDetailedInfo(Party party) {
        detailedInfoGO.SetActive(true);
        detailedInfoRect.sizeDelta = new Vector2(226f, 80f);
        detailedInfoLbl.alignment = TextAlignmentOptions.Center;
        detailedInfoLbl.text = party.name;
        detailedInfoIcon.gameObject.SetActive(false);
        detailedInfoContentParent.gameObject.SetActive(true);
        Utilities.DestroyChildren(detailedInfoContentParent);
        for (int i = 0; i < party.characters.Count; i++) {
            Character character = party.characters[i];
            GameObject portraitGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("CharacterPortrait", Vector3.zero, Quaternion.identity, detailedInfoContentParent);
            CharacterPortrait portrait = portraitGO.GetComponent<CharacterPortrait>();
            //portrait.SetDimensions(48f);
            portrait.GeneratePortrait(character);
        }
        PositionTooltip(detailedInfoGO.transform as RectTransform);
    }
    public void ShowDetailedInfo(HexTile tile) {
        detailedInfoGO.SetActive(true);
        detailedInfoLbl.alignment = TextAlignmentOptions.TopLeft;
        detailedInfoLbl.text = Utilities.NormalizeString(tile.biomeType.ToString()) + "(" + Utilities.NormalizeString(tile.elevationType.ToString()) + ")";
        detailedInfoLbl.text += "\nMana: " + tile.data.manaOnTile.ToString();
        detailedInfoContentParent.gameObject.SetActive(false);
        if (tile.landmarkOnTile == null) {
            detailedInfoRect.sizeDelta = new Vector2(170f, 85f);
            detailedInfoIcon.gameObject.SetActive(false);
        } else {
            detailedInfoRect.sizeDelta = new Vector2(170f, 130f);
            detailedInfoIcon.gameObject.SetActive(true);
            detailedInfoIcon.sprite = LandmarkManager.Instance.GetLandmarkData(tile.landmarkOnTile.specificLandmarkType).landmarkTypeIcon;
        }
        //Utilities.DestroyChildren(detailedInfoContentParent);
        //for (int i = 0; i < party.icharacters.Count; i++) {
        //    ICharacter character = party.icharacters[i];
        //    GameObject portraitGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("CharacterPortrait", Vector3.zero, Quaternion.identity, detailedInfoContentParent);
        //    CharacterPortrait portrait = portraitGO.GetComponent<CharacterPortrait>();
        //    portrait.SetDimensions(48f);
        //    portrait.GeneratePortrait(character, IMAGE_SIZE.X64, true, true);
        //}
        PositionTooltip(detailedInfoGO.transform as RectTransform);
    }
    public void HideDetailedInfo() {
        detailedInfoGO.SetActive(false);
    }
    private void PositionTooltip(RectTransform rt) {
        var v3 = Input.mousePosition;

        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);

        v3.x += 25f;
        v3.y -= 25f;
        rt.position = v3;

        Vector3[] corners = new Vector3[4]; //bottom-left, top-left, top-right, bottom-right
        List<int> cornersOutside = new List<int>();
        rt.GetWorldCorners(corners);
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
                    rt.anchorMin = new Vector2(1f, 0f);
                    rt.anchorMax = new Vector2(1f, 0f);
                    rt.pivot = new Vector2(1f, 0f);
                } else {
                    //right side is outside, move anchor to top right side
                    rt.anchorMin = new Vector2(1f, 1f);
                    rt.anchorMax = new Vector2(1f, 1f);
                    rt.pivot = new Vector2(1f, 1f);
                }
            } else if (cornersOutside.Contains(0) && cornersOutside.Contains(3)) {
                //bottom side is outside, move anchor to bottom left
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(0f, 0f);
                rt.pivot = new Vector2(0f, 0f);
            }
            rt.position = Input.mousePosition;
        }
    }
    #endregion

    #region Notifications Area
    private void ShowNotification(string text, int expirationTicks, UnityAction onClickAction) {
        notificationArea.ShowNotification(text, expirationTicks, onClickAction);
    }
    private void OnInteractionAdded(BaseLandmark interactable, Interaction interaction) {
        if (GameManager.Instance.inspectAll || interactable.isBeingInspected) {
            ShowNotification("New <color=\"red\">" + interaction.name + "</color> interaction at <color=\"red\">" + interactable.name + "</color>", 50, () => ShowInteractableInfo(interactable));
        }
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
            //eventSystem.IsPointerOverGameObject();
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
    public bool IsMouseOnInput() {
        if (EventSystem.current.currentSelectedGameObject == null ||
            (EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() == null &&
            EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() == null)) {
            return false;
        }
        return true;
    }
    public void SetCoverState(bool state) {
        cover.SetActive(state);
    }
    private void BeforeOpeningMenu(UIMenu menuToOpen) {
        //if none of the menus are showing, pause the game when the menu opens
        if (!areaInfoUI.isShowing && !characterInfoUI.isShowing && !playerLandmarkInfoUI.isShowing) {
            menuToOpen.SetOpenMenuAction(() => Pause());
        }
    }
    private void OnMenuClosed(UIMenu closedMenu) {
        if (GameManager.Instance.isPaused) {
            //if the game is paused, and a menu was closed, check if all other menus are closed, if so unpause the game
            if (!InteractionUI.Instance.isShowing && !areaInfoUI.isShowing && !characterInfoUI.isShowing && !playerLandmarkInfoUI.isShowing) {
                Unpause();
            }
        }
    }
    private void OnInteractionMenuOpened() {
        if (areaInfoUI.isShowing) {
            lastOpenedMenu = areaInfoUI;
        } else if (characterInfoUI.isShowing) {
            lastOpenedMenu = characterInfoUI;
        } else if (playerLandmarkInfoUI.isShowing) {
            lastOpenedMenu = playerLandmarkInfoUI;
        }
        //HideMenus();
        if (areaInfoUI.isShowing) {
            areaInfoUI.gameObject.SetActive(false);
        }
        if (characterInfoUI.isShowing) {
            characterInfoUI.gameObject.SetActive(false);
        }
        if (playerLandmarkInfoUI.isShowing) {
            playerLandmarkInfoUI.gameObject.SetActive(false);
        }
    }
    private void OnInteractionMenuClosed() {
        //reopen last opened menu
        if (lastOpenedMenu != null) {
            lastOpenedMenu.OpenMenu();
            lastOpenedMenu = null;
        }
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
    public void AddLifestonesToWorld() {
        PlayerManager.Instance.AdjustTotalLifestones(10);
        PlayerUI.Instance.UpdateUI();
    }
    public void AddLifestonesToPlayer() {
        PlayerManager.Instance.player.AdjustLifestone(10);
        PlayerUI.Instance.UpdateUI();
    }
    public void UnlockAllTokens() {
        InteractionManager.Instance.UnlockAllTokens();
    }
    public void SetUIState(bool state) {
        //Transform[] children = Utilities.GetComponentsInDirectChildren<Transform>(this.gameObject);
        //for (int i = 0; i < children.Length; i++) {
        //    Transform currChild = children[i];
        //    if (currChild.GetComponent<ConsoleMenu>() == null) {
        //        currChild.gameObject.SetActive(state);
        //    }
        //}
        this.gameObject.SetActive(state);
    }
    #endregion

    //private void HideMainUI() {
    //    mainUIGO.SetActive(false);
    //}
    //public void ShowMainUI() {
    //    mainUIGO.SetActive(true);
    //}
    #region Area Info
    [Space(10)]
    [Header("Area Info")]
    [SerializeField]
    internal AreaInfoUI areaInfoUI;
    public void ShowAreaInfo(Area area, int indexToggleToBeActivated = 0) {
        //BeforeOpeningMenu(areaInfoUI);
        //HideMainUI();
        //if (factionInfoUI.isShowing) {
        //    factionInfoUI.HideMenu();
        //}
        if (characterInfoUI.isShowing) {
            characterInfoUI.CloseMenu();
        }
        //if (hexTileInfoUI.isShowing) {
        //    hexTileInfoUI.HideMenu();
        //}
        //if (questInfoUI.isShowing) {
        //    questInfoUI.HideMenu();
        //}
        if (partyinfoUI.isShowing) {
            partyinfoUI.CloseMenu();
        }
        if (monsterInfoUI.isShowing) {
            monsterInfoUI.CloseMenu();
        }
        if (playerLandmarkInfoUI.isShowing) {
            playerLandmarkInfoUI.CloseMenu();
        }
        if (landmarkInfoUI.isShowing) {
            landmarkInfoUI.CloseMenu();
        }
        areaInfoUI.SetData(area);
        areaInfoUI.OpenMenu();
        areaInfoUI.UpdateInvestigation(indexToggleToBeActivated);
        areaInfoUI.CenterOnCoreLandmark();
        //		playerActionsUI.ShowPlayerActionsUI ();
    }
    public void UpdateAreaInfo() {
        if (areaInfoUI.isShowing) {
            areaInfoUI.UpdateAreaInfo();
        }
    }
    #endregion
    #region Landmark Info
    [Space(10)]
    [Header("Landmark Info")]
    [SerializeField] internal LandmarkInfoUI landmarkInfoUI;
    public void ShowLandmarkInfo(BaseLandmark landmark, int indexToggleToBeActivated = 0) {
        //BeforeOpeningMenu(landmarkInfoUI);
        //HideMainUI();
        //if (factionInfoUI.isShowing) {
        //    factionInfoUI.HideMenu();
        //}
        if (characterInfoUI.isShowing) {
            characterInfoUI.CloseMenu();
        }
        //if (hexTileInfoUI.isShowing) {
        //    hexTileInfoUI.HideMenu();
        //}
        //if (questInfoUI.isShowing) {
        //    questInfoUI.HideMenu();
        //}
        if (partyinfoUI.isShowing) {
            partyinfoUI.CloseMenu();
        }
        if (monsterInfoUI.isShowing) {
            monsterInfoUI.CloseMenu();
        }
        if (playerLandmarkInfoUI.isShowing) {
            playerLandmarkInfoUI.CloseMenu();
        }
        if (areaInfoUI.isShowing) {
            areaInfoUI.CloseMenu();
        }
        landmarkInfoUI.SetData(landmark);
        landmarkInfoUI.OpenMenu();
        landmarkInfoUI.UpdateInvestigation(indexToggleToBeActivated);
        landmark.CenterOnLandmark();
        //		playerActionsUI.ShowPlayerActionsUI ();
    }
    public void UpdateLandmarkInfo() {
        if (landmarkInfoUI.isShowing) {
            landmarkInfoUI.UpdateLandmarkInfo();
        }
    }
    #endregion

    #region Player Landmark Info
    [Space(10)]
    [Header("Player Landmark Info")]
    public PlayerLandmarkInfoUI playerLandmarkInfoUI;
    public void ShowPlayerLandmarkInfo(BaseLandmark landmark) {
        //BeforeOpeningMenu(playerLandmarkInfoUI);
        if (characterInfoUI.isShowing) {
            characterInfoUI.CloseMenu();
        }
        if (partyinfoUI.isShowing) {
            partyinfoUI.CloseMenu();
        }
        if (monsterInfoUI.isShowing) {
            monsterInfoUI.CloseMenu();
        }
        if (landmarkInfoUI.isShowing) {
            landmarkInfoUI.CloseMenu();
        }
        if (areaInfoUI.isShowing) {
            areaInfoUI.CloseMenu();
        }
        playerLandmarkInfoUI.SetData(landmark);
        playerLandmarkInfoUI.OpenMenu();
        playerLandmarkInfoUI.CenterOnLandmark();
        //		playerActionsUI.ShowPlayerActionsUI ();
    }
    //public void UpdatePlayerLandmarkInfo() {
    //    if (playerLandmarkInfoUI.isShowing) {
    //        playerLandmarkInfoUI.UpdateLandmarkInfo();
    //    }
    //}
    #endregion

    #region Faction Info
    [Space(10)]
    [Header("Faction Info")]
    [SerializeField]
    internal FactionInfoUI factionInfoUI;
    public void ShowFactionInfo(Faction faction) {
        //BeforeOpeningMenu(factionInfoUI);
        //HideMainUI();
        //if (landmarkInfoUI.isShowing) {
        //    landmarkInfoUI.HideMenu();
        //}
        //if (characterInfoUI.isShowing) {
        //    characterInfoUI.HideMenu();
        //}
        //if (hexTileInfoUI.isShowing) {
        //    hexTileInfoUI.HideMenu();
        //}
        //if (questInfoUI.isShowing) {
        //    questInfoUI.HideMenu();
        //}
        if (partyinfoUI.isShowing) {
            partyinfoUI.CloseMenu();
        }
        //if (monsterInfoUI.isShowing) {
        //    monsterInfoUI.HideMenu();
        //}
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
    public void ShowCharacterInfo(Character character) {
        //BeforeOpeningMenu(characterInfoUI);
        //HideMainUI();
        if (landmarkInfoUI.isShowing) {
            landmarkInfoUI.CloseMenu();
        }
        if (playerLandmarkInfoUI.isShowing) {
            playerLandmarkInfoUI.CloseMenu();
        }
        if (areaInfoUI.isShowing) {
            areaInfoUI.CloseMenu();
        }
        //if (factionInfoUI.isShowing) {
        //    factionInfoUI.HideMenu();
        //}
        //if (hexTileInfoUI.isShowing) {
        //    hexTileInfoUI.HideMenu();
        //}
        //if (questInfoUI.isShowing) {
        //    questInfoUI.HideMenu();
        //}
        //if (partyinfoUI.isShowing) {
        //    partyinfoUI.CloseMenu();
        //}
        if (monsterInfoUI.isShowing) {
            monsterInfoUI.CloseMenu();
        }

        characterInfoUI.SetData(character);
        //if(character.role.roleType != CHARACTER_ROLE.PLAYER) {
        characterInfoUI.OpenMenu();
        //} else {
        //    characterInfoUI.CloseMenu();
        //}
        character.CenterOnCharacter();
        //		playerActionsUI.ShowPlayerActionsUI ();
    }
    public void UpdateCharacterInfo() {
        if (characterInfoUI.isShowing) {
            characterInfoUI.UpdateCharacterInfo();
        }
    }
    #endregion

    #region Party Info
    [Space(10)]
    [Header("Party Info")]
    [SerializeField] internal PartyInfoUI partyinfoUI;
    public void ShowPartyInfo(Party party) {
        //BeforeOpeningMenu(partyinfoUI);
        //HideMainUI();
        if (landmarkInfoUI.isShowing) {
            landmarkInfoUI.CloseMenu();
        }
        if (playerLandmarkInfoUI.isShowing) {
            playerLandmarkInfoUI.CloseMenu();
        }
        if (areaInfoUI.isShowing) {
            areaInfoUI.CloseMenu();
        }
        //if (factionInfoUI.isShowing) {
        //    factionInfoUI.HideMenu();
        //}
        //if (characterInfoUI.isShowing) {
        //    characterInfoUI.CloseMenu();
        //}
        //if (questInfoUI.isShowing) {
        //	questInfoUI.HideMenu();
        //}
        //if (hexTileInfoUI.isShowing) {
        //    hexTileInfoUI.HideMenu();
        //}
        if (monsterInfoUI.isShowing) {
            monsterInfoUI.CloseMenu();
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
    [SerializeField] internal MonsterInfoUI monsterInfoUI;
    public void ShowMonsterInfo(Monster monster) {
        //BeforeOpeningMenu(monsterInfoUI);
        //HideMainUI();
        if (landmarkInfoUI.isShowing) {
            landmarkInfoUI.CloseMenu();
        }
        if (playerLandmarkInfoUI.isShowing) {
            playerLandmarkInfoUI.CloseMenu();
        }
        if (areaInfoUI.isShowing) {
            areaInfoUI.CloseMenu();
        }
        //if (factionInfoUI.isShowing) {
        //    factionInfoUI.HideMenu();
        //}
        //if (hexTileInfoUI.isShowing) {
        //    hexTileInfoUI.HideMenu();
        //}
        //if (questInfoUI.isShowing) {
        //    questInfoUI.HideMenu();
        //}
        if (partyinfoUI.isShowing) {
            partyinfoUI.CloseMenu();
        }
        if (characterInfoUI.isShowing) {
            characterInfoUI.CloseMenu();
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

    //#region Player Actions
    //[Space(10)]
    //[Header("Player Actions")]
    //[SerializeField] internal PlayerActionsUI playerActionsUI;
    //public void ShowPlayerActions(BaseLandmark landmark) {
    //    playerActionsUI.SetData(landmark);
    //    playerActionsUI.OpenMenu();
    //}
    //public void UpdatePlayerActions() {
    //    if (playerActionsUI.isShowing) {
    //        playerActionsUI.UpdatePlayerActions();
    //    }
    //}
    //#endregion

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

    #region Characters Summary
    [Space(10)]
    [Header("Characters Summary")]
    [SerializeField] private GameObject charactersSummaryGO;
    public TokensUI charactersSummaryMenu;
    public void ShowCharactersSummary() {
        //HideQuestsSummary();
        //HideStorylinesSummary();
        //worldInfoCharactersSelectedGO.SetActive(true);
        charactersSummaryMenu.OpenMenu();
    }
    public void HideCharactersSummary() {
        //worldInfoCharactersSelectedGO.SetActive(false);
        charactersSummaryMenu.CloseMenu();
    }
    public void ToggleCharacterSummary() {
        if (charactersSummaryMenu.isShowing) {
            HideCharactersSummary();
        } else {
            ShowCharactersSummary();
        }
    }
    #endregion

    #region Faction Summary
    [Space(10)]
    [Header("Factions Summary")]
    public FactionTokenUI factionsSummaryMenu;
    public void ShowFactionsSummary() {
        factionsSummaryMenu.OpenMenu();
    }
    public void HideFactionSummary() {
        factionsSummaryMenu.CloseMenu();
    }
    public void ToggleFactionSummary() {
        if (factionsSummaryMenu.isShowing) {
            HideFactionSummary();
        } else {
            ShowFactionsSummary();
        }
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
            PositionTooltip(contextMenu.transform as RectTransform);
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

    //#region Quest Summary
    //[Space(10)]
    //[Header("Quest Summary")]
    //[SerializeField] private TextMeshProUGUI questSummaryLbl;
    //public void UpdateQuestSummary() {
    //    string questSummary = string.Empty;
    //    //foreach (KeyValuePair<QUEST_TYPE, List<Quest>> kvp in QuestManager.Instance.availableQuests) {
    //    //    for (int i = 0; i < kvp.Value.Count; i++) {
    //    //        Quest currQuest = kvp.Value[i];
    //    //        questSummary += "<b>" + currQuest.name + "</b>\n";
    //    //        //if (currQuest is BuildStructureQuest) {
    //    //        //    List<Resource> neededResources = (currQuest as BuildStructureQuest).GetNeededResources();
    //    //        //    questSummary += "Needed Resources: ";
    //    //        //    neededResources.ForEach(x => questSummary += x.resource.ToString() + " - " + x.amount.ToString() + "\n");
    //    //        //}
    //    //        //List<Character> characters = currQuest.GetAcceptedCharacters();
    //    //        //for (int j = 0; j < characters.Count; j++) {
    //    //        //    questSummary += "       " + characters[j].urlName + "\n";
    //    //        //}
    //    //    }
           
    //    //}
    //    questSummaryLbl.text = questSummary;
    //}
    //#endregion

    #region Tile Hover
    private HexTile previousTileHovered;
    private HexTile currentTileHovered;
    private float timeHovered;
    private const float hoverThreshold = 1.5f;
    private void OnHoverOverTile(HexTile tile) {
        previousTileHovered = currentTileHovered;
        currentTileHovered = tile;
    }
    private void OnHoverOutTile(HexTile tile) {
        currentTileHovered = null;
    }
    #endregion

    #region Player
    public void ShowPlayerPicker() {
        playerPickerGO.SetActive(true);
    }
    public void HidePlayerPicker() {
        playerPickerGO.SetActive(false);
        //PlayerUI.Instance.SetBottomMenuTogglesState(false);
        //PlayerManager.Instance.player.OnHidePlayerPicker();
    }
    public void OnClickOkPlayerPicker() {
        //PlayerManager.Instance.player.OnOkPlayerPicker();
        HidePlayerPicker();
    }
    public void OnClickClosePlayerPicker() {
        HidePlayerPicker();
    }
    public void PopulatePlayerItemsInPicker() {
        List<Item> items = PlayerManager.Instance.player.items;
        int length = items.Count;
        if(currentActivePlayerPickerButtons.Count > items.Count) {
            length = currentActivePlayerPickerButtons.Count;
        }
        for (int i = 0; i < length; i++) {
            if(i >= items.Count) {
                currentActivePlayerPickerButtons[i].gameObject.SetActive(false);
            }else if (i >= currentActivePlayerPickerButtons.Count) {
                CreatePlayerPickerButton(items[i]);
            } else {
                currentActivePlayerPickerButtons[i].gameObject.SetActive(true);
                currentActivePlayerPickerButtons[i].SetPlayerPicker(items[i]);
            }
        }
    }
    public void PopulatePlayerTokensInPicker() {
        List<Token> tokens = PlayerManager.Instance.player.tokens;
        int length = tokens.Count;
        if (currentActivePlayerPickerButtons.Count > tokens.Count) {
            length = currentActivePlayerPickerButtons.Count;
        }
        for (int i = 0; i < length; i++) {
            if (i >= tokens.Count) {
                currentActivePlayerPickerButtons[i].gameObject.SetActive(false);
            } else if (i >= currentActivePlayerPickerButtons.Count) {
                //CreatePlayerPickerButton(intels[i]);
            } else {
                currentActivePlayerPickerButtons[i].gameObject.SetActive(true);
                //currentActivePlayerPickerButtons[i].SetPlayerPicker(intels[i]);
            }
        }
    }
    //public void PopulateLandmarkItemsInPicker() {
    //    BaseLandmark landmark = PlayerManager.Instance.player.currentTargetinteractable;
    //    List<Item> items = landmark.itemsInLandmark;
    //    int length = items.Count;
    //    if (currentActivePlayerPickerButtons.Count > items.Count) {
    //        length = currentActivePlayerPickerButtons.Count;
    //    }
    //    for (int i = 0; i < length; i++) {
    //        if (i >= items.Count) {
    //            currentActivePlayerPickerButtons[i].gameObject.SetActive(false);
    //        } else if (i >= currentActivePlayerPickerButtons.Count) {
    //            CreatePlayerPickerButton(items[i]);
    //        } else {
    //            currentActivePlayerPickerButtons[i].gameObject.SetActive(true);
    //            currentActivePlayerPickerButtons[i].SetPlayerPicker(items[i]);
    //        }
    //    }
    //}
    private void CreatePlayerPickerButton(IPlayerPicker playerPicker) {
        GameObject go = GameObject.Instantiate(playerPickerButtonPrefab, playerPickerContentTransform);
        PlayerPickerButton playerPickerButton = go.GetComponent<PlayerPickerButton>();
        playerPickerButton.SetPlayerPicker(playerPicker);
        currentActivePlayerPickerButtons.Add(playerPickerButton);
    }
    private void OnTokenAdded(Token token) {
        UnityAction action = null;
        string notificationText = string.Empty;
        if (token is FactionToken) {
            action = () => ShowCharacterTokenMenu();
            notificationText = "Obtained token about faction: <color=\"green\"><b>" + (token as FactionToken).faction.name;
        } else if (token is LocationToken) {
            action = () => ShowCharacterTokenMenu();
            notificationText = "Obtained token about location: <color=\"green\"><b>" + (token as LocationToken).location.name;
        } else if (token is CharacterToken) {
            action = () => ShowCharacterTokenMenu();
            notificationText = "Obtained token about character: <color=\"green\"><b>" + (token as CharacterToken).character.name;
        } else if (token is DefenderToken) {
            action = () => ShowCharacterTokenMenu();
            notificationText = "Obtained token about defenders at: <color=\"green\"><b>" + (token as DefenderToken).owner.name;
        } else if (token is SpecialToken) {
            action = () => ShowCharacterTokenMenu();
            notificationText = "Obtained token about special item: <color=\"green\"><b>" + (token as SpecialToken).name;
        }
        ShowNotification(notificationText, 5, action);
    }
    private void OnCombatDone(Combat combat) {
        ShowNotification("Combat at <b>" + combat.location.name + "</b>!", 5, () => ShowCombatLog(combat));
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

    //public void EnvelopContentCoroutineStarter(RectTransform thisTransform, RectTransform otherTransform,
    //    bool followWidth, bool followHeight, Vector2 padding) {
    //    StartCoroutine(Envelop(thisTransform, otherTransform, followWidth, followHeight, padding));
    //}

    //private IEnumerator Envelop(RectTransform thisTransform, RectTransform otherTransform, 
    //    bool followWidth, bool followHeight, Vector2 padding) {
    //    yield return null;
    //    Vector2 newSize = thisTransform.sizeDelta;
    //    if (followWidth) {
    //        newSize.x = otherTransform.sizeDelta.x;
    //        newSize.x += padding.x;
    //    }
    //    if (followHeight) {
    //        newSize.y = otherTransform.sizeDelta.y;
    //        newSize.y += padding.y;
    //    }
    //    thisTransform.sizeDelta = newSize;
    //}
}