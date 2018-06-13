using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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

    //[Space(10)]
    //[Header("Minimap")]
    //[SerializeField] private GameObject minimapGO;
    //[SerializeField] private GameObject minimapTextureGO;

    //[Space(10)]
    //[Header("Notification Area")]
    //[SerializeField] private UITable notificationParent;
    //[SerializeField] private UIScrollView notificationScrollView;

    [Space(10)]
    [Header("World Info Menu")]
    [SerializeField] private GameObject worldInfoCharactersSelectedGO;
    [SerializeField] private GameObject worldInfoQuestsSelectedGO;
    [SerializeField] private GameObject worldInfoStorylinesSelectedGO;
    [SerializeField] private GameObject worldInfoCharactersBtn;
    [SerializeField] private GameObject worldInfoQuestsBtn;
    [SerializeField] private GameObject worldInfoStorylinesBtn;

    //[Space(10)]
    //[Header("Faction Summary Menu")]
    //[SerializeField] private FactionSummaryUI factionSummaryUI;

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

	internal List<object> eventLogsQueue = new List<object> ();

	//internal string warAllianceState = string.Empty;

    //private List<NotificationItem> notificationItemsThatCanBeReused;
    //private List<Log> logHistory;
	//private bool isShowKingdomHistoryOnly;

    private List<UIMenuSettings> _menuHistory;

    #region getters/setters
    //internal GameObject minimapTexture {
    //    get { return minimapTextureGO; }
    //}
    internal List<UIMenuSettings> menuHistory {
        get { return _menuHistory; }
    }
    #endregion

    void Awake(){
		Instance = this;
        //logHistory = new List<Log>();
        //notificationItemsThatCanBeReused = new List<NotificationItem>();
        _menuHistory = new List<UIMenuSettings>();
        //kingdomSummaryEntries = Utilities.GetComponentsInDirectChildren<KingdomSummaryEntry>(kingdomSummaryGrid.gameObject);
    }

	void Start(){
        //Messenger.AddListener(Signals.DAY_END, CheckForKingdomExpire);
        Messenger.AddListener("UpdateUI", UpdateUI);
        //EventManager.Instance.onKingdomDiedEvent.AddListener(CheckIfShowingKingdomIsAlive);
        //EventManager.Instance.onCreateNewKingdomEvent.AddListener(AddKingdomToList);
        //EventManager.Instance.onKingdomDiedEvent.AddListener(QueueKingdomForRemoval);
        NormalizeFontSizes();
        ToggleBorders();
        //toggleBordersBtn.SetClickState(true);
        //isShowKingdomHistoryOnly = false;
        //      PopulateHistoryTable();
        //PopulateCityHistoryTable ();
        //UpdateUI();
    }

    internal void InitializeUI() {
        for (int i = 0; i < allMenus.Length; i++) {
            allMenus[i].Initialize();
        }
    }

    private void Update() {
        uiCamera.orthographicSize = Screen.width /2;
        if (Input.GetKeyDown(KeyCode.BackQuote)) {
            if (GameManager.Instance.allowConsole) {
                ToggleConsole();
            }
        }
        //if (!consoleUI.isShowing) {
            if (Input.GetKeyDown(KeyCode.Space)) {
                if (GameManager.Instance.isPaused) {
                    if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X1) {
                        SetProgressionSpeed1X(true);
                    } else if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X2) {
                        SetProgressionSpeed2X(true);
                    } else if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X4) {
                        SetProgressionSpeed4X(true);
                    }
                } else {
                    Pause(true);
                }
            }
        //}
  
        //if (Input.GetMouseButton(0)) {
        //    UITexture uiTexture = minimapTextureGO.GetComponent<UITexture>();
        //    uiTexture.material.SetTexture("_MainTex", uiTexture.mainTexture);
        //    Texture2D tex = uiTexture.material.GetTexture("_MainTex") as Texture2D;
        //    RaycastHit hit;
        //    if (!Physics.Raycast(uiCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition), out hit))
        //        return;

        //    Debug.Log("Hit!: " + hit.collider.gameObject.name);

        //    Vector3 pixelUV = minimapTextureGO.transform.InverseTransformPoint(hit.point);
        //    pixelUV.x *= tex.width;
        //    pixelUV.y *= tex.height;

        //    tex.SetPixel((int)pixelUV.x, (int)pixelUV.y, Color.white);
        //    tex.Apply();

        //    Debug.Log("Hit Coordinates!: " + pixelUV.x.ToString() + "," + pixelUV.y.ToString());
        //}
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

    private void UpdateUI(){
        dateLbl.SetText(GameManager.Instance.days.ToString() + " " + LocalizationManager.Instance.GetLocalizedValue("General", "Months", ((MONTH)GameManager.Instance.month).ToString()) + ", " + GameManager.Instance.year.ToString());
	}

    #region World Controls
    public void SetProgressionSpeed1X(bool state) {
        if (state) {
            x1Btn.isOn = true;
            speedToggleGroup.NotifyToggleOn(x1Btn);
            GameManager.Instance.SetProgressionSpeed(PROGRESSION_SPEED.X1);
            Unpause();
        }
    }
    public void SetProgressionSpeed2X(bool state) {
        if (state) {
            x2Btn.isOn = true;
            speedToggleGroup.NotifyToggleOn(x2Btn);
            GameManager.Instance.SetProgressionSpeed(PROGRESSION_SPEED.X2);
            Unpause();
        }
    }
    public void SetProgressionSpeed4X(bool state) {
        if (state) {
            x4Btn.isOn = true;
            speedToggleGroup.NotifyToggleOn(x4Btn);
            GameManager.Instance.SetProgressionSpeed(PROGRESSION_SPEED.X4);
            Unpause();
        }
    }
    public void Pause(bool state) {
        if (state) {
            pauseBtn.isOn = true;
            speedToggleGroup.NotifyToggleOn(pauseBtn);
            GameManager.Instance.SetPausedState(true);
            //pauseBtn.SetAsClicked();
            if (onPauseEventExpiration != null) {
                onPauseEventExpiration(true);
            }
        }
        
    }
    private void Unpause() {
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
	public IEnumerator RepositionGrid(UIGrid thisGrid){
		yield return null;
		if(thisGrid != null && this.gameObject.activeSelf){
			thisGrid.Reposition ();
		}
		yield return null;
	}
	public IEnumerator RepositionTable(UITable thisTable){
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		thisTable.Reposition ();
	}
	public IEnumerator RepositionScrollView(UIScrollView thisScrollView, bool keepScrollPosition = false){
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
	public IEnumerator LerpProgressBar(UIProgressBar progBar, float targetValue, float lerpTime){
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
    //public void ShowNotification(Log log, HashSet<Kingdom> kingdomsThatShouldShowNotif, bool addLogToHistory = true) {
//      if (addLogToHistory) {
//          AddLogToLogHistory(log);
//      }
        //if (!kingdomsThatShouldShowNotif.Contains(currentlyShowingKingdom)) {
        //    //currentlyShowingKingdom is not included in kingdomsThatShouldShowNotif, don't show notification
        //    return;
        //}
        //if (notificationItemsThatCanBeReused.Count > 0) {
        //    NotificationItem itemToUse = notificationItemsThatCanBeReused[0];
        //    itemToUse.SetLog(log);
        //    RemoveNotificationItemFromReuseList(itemToUse);
        //    itemToUse.gameObject.SetActive(true);
        //} else {
        //    GameObject notifGO = InstantiateUIObject(notificationPrefab.name, notificationParent.transform);
        //    notifGO.transform.localScale = Vector3.one;
        //    notifGO.GetComponent<NotificationItem>().SetLog(log);
        //}
        ////notificationParent.AddChild(notifGO.transform);
        //RepositionNotificationTable();
        ////notificationScrollView.UpdatePosition();
        ////notificationParent.Reposition();
        //notificationScrollView.UpdatePosition();

    //}
//    public void ShowNotification(Log log, bool addLogToHistory = true) {
////        if (addLogToHistory) {
////            AddLogToLogHistory(log);
////        }
//        //if (kingdomsThatShouldShowNotif != null) {
//        //    if (!kingdomsThatShouldShowNotif.Contains(currentlyShowingKingdom)) {
//        //        //currentlyShowingKingdom is not included in kingdomsThatShouldShowNotif, don't show notification
//        //        return;
//        //    }
//        //}
//        if (notificationItemsThatCanBeReused.Count > 0) {
//            NotificationItem itemToUse = notificationItemsThatCanBeReused[0];
//            itemToUse.SetLog(log);
//            RemoveNotificationItemFromReuseList(itemToUse);
//            itemToUse.gameObject.SetActive(true);
//        } else {
//            GameObject notifGO = InstantiateUIObject(notificationPrefab.name, notificationParent.transform);
//            notifGO.transform.localScale = Vector3.one;
//            notifGO.GetComponent<NotificationItem>().SetLog(log);
//        }
//        //notificationParent.AddChild(notifGO.transform);
//        RepositionNotificationTable();
//        //notificationScrollView.UpdatePosition();
//        //notificationParent.Reposition();
//        notificationScrollView.UpdatePosition();

//    }
    //internal void AddNotificationItemToReuseList(NotificationItem item) {
    //    if (!notificationItemsThatCanBeReused.Contains(item)) {
    //        notificationItemsThatCanBeReused.Add(item);
    //    }
    //}
    //internal void RemoveNotificationItemFromReuseList(NotificationItem item) {
    //    notificationItemsThatCanBeReused.Remove(item);
    //}
    //public void RemoveAllNotifications() {
    //    List<Transform> children = notificationParent.GetChildList();
    //    for (int i = 0; i < children.Count; i++) {
    //        ObjectPoolManager.Instance.DestroyObject(children[i].gameObject);
    //    }
    //}
    //public void RepositionNotificationTable() {
    //    StartCoroutine(RepositionTable(notificationParent));
    //    //StartCoroutine(RepositionGrid(notificationParent));
    //    //StartCoroutine(RepositionScrollView(notificationParent.GetComponentInParent<UIScrollView>()));
    //}
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
        if(landmarkInfoUI.currentlyShowingLandmark != null) {
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

    #region Landmark Info
    [Space(10)]
    [Header("Landmark Info")]
    [SerializeField] internal LandmarkInfoUI landmarkInfoUI;
    public void ShowLandmarkInfo(BaseLandmark landmark) {
        HideMainUI();
        if (factionInfoUI.isShowing) {
            factionInfoUI.HideMenu();
        }
        if (characterInfoUI.isShowing){
			characterInfoUI.HideMenu ();
		}
        if (hexTileInfoUI.isShowing) {
            hexTileInfoUI.HideMenu();
        }
        //if (questInfoUI.isShowing) {
        //    questInfoUI.HideMenu();
        //}
        //if(partyinfoUI.isShowing){
        //	partyinfoUI.HideMenu ();
        //}
        landmarkInfoUI.SetData(landmark);
        landmarkInfoUI.OpenMenu();
        landmark.CenterOnLandmark();
//		playerActionsUI.ShowPlayerActionsUI ();
    }
    public void UpdateLandmarkInfo() {
		if(landmarkInfoUI.isShowing){
			landmarkInfoUI.UpdateLandmarkInfo();
		}
    }
    #endregion

    #region Faction Info
    [Space(10)]
    [Header("Faction Info")]
    [SerializeField] internal FactionInfoUI factionInfoUI;
	public void ShowFactionInfo(Faction faction) {
        HideMainUI();
		if(landmarkInfoUI.isShowing){
			landmarkInfoUI.HideMenu ();
		}
		if(characterInfoUI.isShowing){
			characterInfoUI.HideMenu ();
		}
        if (hexTileInfoUI.isShowing) {
            hexTileInfoUI.HideMenu();
        }
        //if (questInfoUI.isShowing) {
        //    questInfoUI.HideMenu();
        //}
        //if(partyinfoUI.isShowing){
        //	partyinfoUI.HideMenu ();
        //}
        factionInfoUI.SetData(faction);
        factionInfoUI.OpenMenu();
//		playerActionsUI.ShowPlayerActionsUI ();
	}
	public void UpdateFactionInfo() {
		if (factionInfoUI.isShowing) {
			factionInfoUI.UpdateFactionInfo ();
		}
	}
    #endregion

    private void HideMainUI() {
        mainUIGO.SetActive(false);
    }

    public void ShowMainUI() {
        mainUIGO.SetActive(true);
    }

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
        //if(partyinfoUI.isShowing){
        //	partyinfoUI.HideMenu ();
        //}
        characterInfoUI.SetData(character);
        characterInfoUI.OpenMenu();
        character.CenterOnCharacter();
//		playerActionsUI.ShowPlayerActionsUI ();
	}
	public void UpdateCharacterInfo() {
		if (characterInfoUI.isShowing) {
			characterInfoUI.UpdateCharacterInfo ();
		}
	}
    #endregion

    #region HexTile Info
    [Space(10)]
    [Header("HexTile Info")]
    [SerializeField] internal HextileInfoUI hexTileInfoUI;
	public void ShowHexTileInfo(HexTile hexTile) {
        HideMainUI();
		if(landmarkInfoUI.isShowing){
			landmarkInfoUI.HideMenu ();
		}
		if(factionInfoUI.isShowing){
			factionInfoUI.HideMenu ();
		}
		if(characterInfoUI.isShowing){
			characterInfoUI.HideMenu ();
		}
        //if (questInfoUI.isShowing) {
        //    questInfoUI.HideMenu();
        //}
		//if(partyinfoUI.isShowing){
		//	partyinfoUI.HideMenu ();
		//}
        hexTileInfoUI.SetData(hexTile);
        hexTileInfoUI.OpenMenu();
//		playerActionsUI.ShowPlayerActionsUI ();
	}
	public void UpdateHexTileInfo() {
		if (hexTileInfoUI.isShowing) {
			hexTileInfoUI.UpdateHexTileInfo ();
		}
	}
    #endregion

	#region Party Info
	[Space(10)]
	[Header("Party Info")]
	[SerializeField] internal PartyInfoUI partyinfoUI;
	public void ShowPartyInfo(Party party) {
		if(landmarkInfoUI.isShowing){
			landmarkInfoUI.HideMenu ();
		}
		if(factionInfoUI.isShowing){
			factionInfoUI.HideMenu ();
		}
		if(characterInfoUI.isShowing){
			characterInfoUI.HideMenu ();
		}
		//if (questInfoUI.isShowing) {
		//	questInfoUI.HideMenu();
		//}
		if(hexTileInfoUI.isShowing){
			hexTileInfoUI.HideMenu ();
		}
        partyinfoUI.SetData(party);
        partyinfoUI.OpenMenu();
	}
	public void UpdatePartyInfo() {
		if (partyinfoUI.isShowing) {
			partyinfoUI.UpdatePartyInfo ();
		}
	}
	#endregion


    //#region Faction Summary
    //public void ShowFactionsSummary() {
    //    factionSummaryUI.ShowFactionSummary();
    //    HideStorylinesSummary();
    //    HideQuestsSummary();
    //    SetWorldInfoMenuItemAsSelected(worldInfoFactionBtn.transform);
    //}
    //public void HideFactionsSummary() {
    //    factionSummaryUI.HideFactionSummary();
    //}
    //public void UpdateFactionSummary() {
    //    factionSummaryUI.UpdateFactionsSummary();
    //}
    //#endregion

    #region Player Actions
    [Space(10)]
    [Header("Player Actions")]
    [SerializeField] internal PlayerActionsUI playerActionsUI;
	public void ShowPlayerActions(ILocation location){
//		playerActionsUI.transform.parent = location.tileLocation.UIParent;
		playerActionsUI.ShowPlayerActionsUI(location);
		playerActionsUI.Reposition ();
	}
	public void HidePlayerActions() {
		playerActionsUI.HidePlayerActionsUI();
	}
    #endregion

  //  #region Quest Info
  //  [Space(10)]
  //  [Header("Quest Info")]
  //  [SerializeField] internal QuestInfoUI questInfoUI;
  //  public void ShowQuestInfo(OldQuest.Quest quest) {
  //      if (settlementInfoUI.isShowing) {
  //          settlementInfoUI.HideMenu();
  //      }
  //      if (factionInfoUI.isShowing) {
  //          factionInfoUI.HideMenu();
  //      }
  //      if (characterInfoUI.isShowing) {
  //          characterInfoUI.HideMenu();
  //      }
  //      if (hexTileInfoUI.isShowing) {
  //          hexTileInfoUI.HideMenu();
  //      }
		//if(partyinfoUI.isShowing){
		//	partyinfoUI.HideMenu ();
		//}
  //      questInfoUI.SetData(quest);
  //      questInfoUI.OpenMenu();
  //      //		playerActionsUI.ShowPlayerActionsUI ();
  //  }
  //  public void UpdateQuestInfo() {
  //      if (questInfoUI.isShowing) {
  //          questInfoUI.UpdateQuestInfo();
  //      }
  //  }
  //  #endregion

  //  #region Quest Logs
  //  [Space(10)]
  //  [Header("Quest Logs")]
  //  [SerializeField] internal QuestLogsUI questLogUI;
  //  public void ShowQuestLog(OldQuest.Quest quest) {
		//if(combatLogUI.isShowing){
		//	combatLogUI.HideCombatLogs ();
		//}
  //      questLogUI.ShowQuestLogs(quest);
  //      questLogUI.UpdateQuestLogs();
  //  }
  //  public void UpdateQuestLogs() {
  //      if (questLogUI.isShowing) {
  //          questLogUI.UpdateQuestLogs();
  //      }
  //  }
  //  #endregion

    #region Menu History
    public void AddMenuToQueue(UIMenu menu, object data) {
        UIMenuSettings latestSetting = _menuHistory.ElementAtOrDefault(0);
        if(latestSetting != null) {
            if(latestSetting.menu == menu && latestSetting.data == data) {
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
        return false;
        //return consoleUI.isShowing;
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

    #region Quests Summary
    [Space(10)]
    [Header("Quests Summary")]
    [SerializeField] private UILabel questsSummaryLbl;
    [SerializeField] private GameObject questsSummaryGO;
    public void ShowQuestsSummary() {
        HideCharactersSummary();
        HideStorylinesSummary();
        worldInfoQuestsSelectedGO.SetActive(true);
        questsSummaryGO.SetActive(true);
		UpdateQuestsSummary();
    }
    public void HideQuestsSummary() {
        worldInfoQuestsSelectedGO.SetActive(false);
        questsSummaryGO.SetActive(false);
    }
    public void UpdateQuestsSummary() {
        string questSummary = string.Empty;
        questSummary += "[b]Available Quests: [/b]";
        for (int i = 0; i < QuestManager.Instance.availableQuests.Count; i++) {
            Quest currentQuest = QuestManager.Instance.availableQuests[i];
            if (!currentQuest.isDone) {
                questSummary += "\n" + currentQuest.questURLName;
                questSummary += "\n   Characters on Quest: ";
                if (currentQuest.acceptedCharacters.Count > 0) {
                    for (int j = 0; j < currentQuest.acceptedCharacters.Count; j++) {
                        ECS.Character currCharacter = currentQuest.acceptedCharacters[j];
                        questSummary += "\n" + currCharacter.urlName + " (" + currCharacter.currentQuestPhase.phaseName + ")";
                    }
                } else {
                    questSummary += "NONE";
                }
            }
        }
        questsSummaryLbl.text = questSummary;
		questsSummaryLbl.ResizeCollider ();
    }
    #endregion

    #region Storylines Summary
    [Space(10)]
    [Header("Storylines Summary")]
    [SerializeField] private GameObject storylinesSummaryGO;
    public StorylinesSummaryMenu storylinesSummaryMenu;
    public void ShowStorylinesSummary() {
        HideCharactersSummary();
        HideQuestsSummary();
        worldInfoStorylinesSelectedGO.SetActive(true);
        storylinesSummaryMenu.ShowMenu();
		StartCoroutine(RepositionTable (storylinesSummaryMenu.storyTable));
        //UpdateQuestsSummary();
    }
    public void HideStorylinesSummary() {
        worldInfoStorylinesSelectedGO.SetActive(false);
        storylinesSummaryMenu.HideMenu();
    }
    #endregion

    #region Characters Summary
    [Space(10)]
    [Header("Characters Summary")]
    [SerializeField] private GameObject charactersSummaryGO;
    public CharactersSummaryUI charactersSummaryMenu;
    public void ShowCharactersSummary() {
        HideQuestsSummary();
        HideStorylinesSummary();
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
