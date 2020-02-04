using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class UIManager : MonoBehaviour {

	public static UIManager Instance = null;

    public delegate void OnAddNewBattleLog();
    public OnAddNewBattleLog onAddNewBattleLog;

    public UICamera uiCamera;

    [SerializeField] UIMenu[] allMenus;

    [Space(10)]
    [Header("Prefabs")]
 //   public GameObject characterPortraitPrefab;
	//public GameObject historyPortraitPrefab;
	//public GameObject eventPortraitPrefab;
	//public GameObject gameEventPrefab;
	//public GameObject kingdomEventsListParentPrefab;
	//public GameObject kingdomEventsListItemPrefab;
 //   public GameObject kingdomFlagPrefab;
 //   public GameObject logItemPrefab;
 //   public GameObject cityItemPrefab;
	//public GameObject lairItemPrefab;
	//public GameObject hextileEventItemPrefab;
    public GameObject resourceIconPrefab;
    //public GameObject playerEventItemPrefab;
    //[SerializeField] private GameObject kingdomIntervenePrefab;
    [SerializeField] private GameObject notificationPrefab;
    //[SerializeField] private GameObject logHistoryPrefab;
    //public GameObject warHistoryPrefab;
    //public GameObject battleHistoryPrefab;

    [Space(10)]
    [Header("Main UI Objects")]
    public GameObject smallInfoGO;
	public GameObject citizenInfoGO;
	public GameObject cityInfoGO;
	public GameObject kingdomInfoGO;
	public GameObject eventsGo;
	public GameObject eventsOfTypeGo;
	public UIGrid eventCategoriesGrid;
	public GameObject relationshipsGO;
	public GameObject relationshipHistoryGO;
	public GameObject familyTreeGO;
	public GameObject citizenHistoryGO;
	public GameObject allKingdomEventsGO;
	public GameObject eventLogsGO;
    public GameObject kingdomCitiesGO;
    public GameObject interveneMenuGO;
	public GameObject playerActionsMenuGO;
    [SerializeField] private GameObject kingdomSummaryGO;

	[Space(10)]
    [Header("For Testing Objects")]
   	//Relationship Editor
	public UILabel sourceKinglikenessLbl;
	public UILabel targetKinglikenessLbl;
	//Relocate
	public GameObject relocateGO;
	public UIPopupList citiesForRelocationPopupList;
	//Force War
	public GameObject forceWarGO;
	public UIPopupList kingdomsForWar;
	//Unrest
	public GameObject unrestGO;
	public UIInput unrestInput;
	//Force Plague
	public GameObject forcePlagueGO;
	public UIPopupList kingdomsForPlague;

	[Space(10)]
    [Header("Date Objects")]
    public ButtonGroupItem pauseBtn;
	public ButtonGroupItem x1Btn;
	public ButtonGroupItem x2Btn;
	public ButtonGroupItem x4Btn;
    public UILabel dateLbl;

    [Space(10)]
    [Header("Miscellaneous")]
    public UILabel smallInfoLbl;
	

	//[Space(10)]
 //   [Header("Citizen UI Objects")]
 //   public UI2DSprite ctizenPortraitBG;
 //   public GameObject citizenInfoIsDeadIcon;
 //   public UILabel citizenNameLbl;
	//public UILabel citizenRoleAndKingdomLbl;
	//public UILabel citizenAgeLbl;
	//public UILabel citizenCityNameLbl;
	//public UIGrid citizenHistoryGrid;
	//public ButtonToggle characterValuesBtn;
	//public ButtonToggle familyTreeBtn;
	//public ButtonToggle citizenHistoryBtn;
 //   public GameObject characterValuesGO;
 //   public UILabel characterValuesLbl;
	//public GameObject citizenInfoForTestingGO;
	////public CitizenInfoUI citizenInfoUI;
 //   [SerializeField] private UILabel preferredKingdomTypeLbl;
 //   [SerializeField] private UILabel loyaltyToKingLbl;
	//[SerializeField] private UILabel citizenWarmongerLbl;
 //   //[SerializeField] private TraitIcon[] citizenTraitIcons;

	//[Space(10)]
 //   [Header("Events UI Objects")]
 //   public UIGrid gameEventsOfTypeGrid;
	//public Sprite assassinationIcon;
	//public Sprite rebellionPlotIcon;
	//public Sprite stateVisitIcon;
	//public Sprite borderConflictIcon;
	//public Sprite raidIcon;
	//public Sprite invasionPlotIcon;
	//public Sprite militarizationIcon;
	//public Sprite requestPeaceIcon;
	//public Sprite declareWarIcon;
	//public Sprite expansionIcon;
	//public Sprite marriageInvitationIcon;

	//[Space(10)]
 //   [Header("Relationships UI Objects")]
 //   public GameObject kingRelationshipsParentGO;
 //   public GameObject kingRelationshipLine;
 //   public GameObject kingNoRelationshipsGO;
 //   public UIScrollView kingRelationshipsScrollView;
 //   public UIScrollBar kingRelationshipsScrollBar;
 //   public UIGrid kingRelationshipsGrid;

	//[Space(10)]
 //   [Header("Relationship History UI Objects")]
 //   public UI2DSprite relationshipStatusSprite;
	//public UIGrid relationshipHistoryGrid;
	//public GameObject noRelationshipsToShowGO;
	//public GameObject relationshipHistoryForTestingGO;

	//[Space(10)]
 //   [Header("Family Tree UI Objects")]
 //   public GameObject familyTreeFatherGO;
	//public GameObject familyTreeMotherGO;
	//public GameObject familyTreeSpouseGO;
	//public UIGrid familyTreeChildGrid;
	//public UI2DSprite familyTreeInnerSprite;
	//public GameObject nextMarriageBtn;
 //   [SerializeField] private GameObject successorParentGO;
 //   //[SerializeField] private CharacterPortrait successorPortrait;
 //   [SerializeField] private UILabel successorPreferredKingdomTypeLbl;
 //   [SerializeField] private GameObject otherCitizensGO;
 //   [SerializeField] private CharacterPortrait chancellorPortrait;
 //   [SerializeField] private CharacterPortrait marshalPortrait;
 //   [SerializeField] private UILabel spouseCompatibility;

    //[Space(10)]
    //[Header("Kingdom Events UI Objects")]
    //public ButtonToggle kingdomCurrentEventsBtn;
    //public GameObject kingdomCurrentEventsGO;
    //public UITable kingdomCurrentEventsContentParent;
    //public UILabel kingdomNoCurrentEventsLbl;
    //public ButtonToggle kingdomHistoryBtn;
    //public GameObject kingdomHistoryGO;
    //public UIGrid kingdomHistoryGrid;
    //public UILabel kingdomHistoryNoEventsLbl;

 //   [Space(10)]
 //   [Header("Kingdoms UI Objects")]
 //   public UILabel kingdomNameLbl;
 //   public UILabel kingdomUnrestLbl;
 //   [SerializeField] private UILabel kingdomPrestigeLbl;
	//public UILabel kingdomTechLbl;
	//public UIProgressBar kingdomTechMeter;
 //   public UI2DSprite kingdomBasicResourceSprite;
 //   public UILabel kingdomBasicResourceLbl;
 //   public UIGrid kingdomOtherResourcesGrid;
 //   public UIGrid kingdomTradeResourcesGrid;
	//public CharacterPortrait kingdomListActiveKing;
	//public ButtonToggle kingdomListEventButton;
	//public ButtonToggle kingdomListRelationshipButton;
	//public ButtonToggle kingdomListCityButton;
 //   public Sprite stoneSprite;
 //   public Sprite lumberSprite;
	//public UILabel actionDayLbl;
	//public UILabel warmongerLbl;
	//public GameObject militarizingGO;
	//public GameObject fortifyingGO;
 //   public UILabel populationSummary;
 //   [SerializeField] private UI2DSprite kingdomListEmblemOutline;
 //   [SerializeField] private UI2DSprite kingdomListEmblemBG;
 //   [SerializeField] private UI2DSprite kingdomListEmblemSprite;

 //   [Space(10)]
 //   [Header("Event Logs UI Objects")]
 //   public GameObject elmEventLogsParentGO;
	//public UILabel elmEventTitleLbl;
	//public GameObject elmSuccessRateGO;
	//public UILabel elmSuccessRateLbl;
	//public UILabel elmProgressBarLbl;
	//public UIProgressBar elmEventProgressBar;
	//public GameObject elmFirstAnchor;
	//public UIScrollView elmScrollView;

    //[Space(10)]
    //[Header("Cities UI Objects")]
    //public UIScrollView kingdomCitiesScrollView;
    //public UIGrid kingdomCitiesGrid;
    //public GameObject relationshipSummaryGO;
    //public UILabel relationshipSummaryLbl;
    //public UILabel relationshipSummaryTitleLbl;

    //[Space(10)]
    //[Header("Intervene UI Objects")]
    //[SerializeField] private UIGrid interveneMenuGrid;
    //[SerializeField] private UIScrollView interveneMenuScrollView;
    //[SerializeField] private ButtonToggle interveneMenuBtn;
    //[SerializeField] private GameObject interveneActonsGO;
    ////Switch Kingdom Objects
    //[SerializeField] private GameObject switchKingdomGO;
    //[SerializeField] private UIGrid switchKingdomGrid;
    //[SerializeField] private ButtonToggle switchKingdomsBtn;
    ////Create Kingdom Objects
    //[SerializeField] private GameObject createKingdomGO;
    //[SerializeField] private ButtonToggle createKingdomBtn;
    //[SerializeField] private UILabel createKingdomRaceSelectedLbl;
    //[SerializeField] private UILabel createKingdomErrorLbl;
    //[SerializeField] private UIPopupList createKingdomPopupList;
    //[SerializeField] private UIButton createKingdomExecuteBtn;
    ////Choose Citizen Objects
    //[SerializeField] private GameObject chooseCitizenGO;
    //[SerializeField] private UIGrid chooseCitizenGrid;
    //[SerializeField] private UIButton chooseCitizenOkBtn;
    //[SerializeField] private UIButton chooseCitizenCancelBtn;
    //[SerializeField] private GameObject chooseCitizenSelectedGO;
    //[SerializeField] private UILabel chooseCitizenInstructionLbl;
    //[SerializeField] private GameObject chooseCitizenNoChoicesGO;
    ////Choose Kingdom Objects
    //[SerializeField] private GameObject chooseKingdomGO;
    //[SerializeField] private UIGrid chooseKingdomGrid;
    //[SerializeField] private UIButton chooseKingdomOkBtn;
    //[SerializeField] private UIButton chooseKingdomCancelBtn;
    //[SerializeField] private GameObject chooseKingdomSelectedGO;
    //[SerializeField] private UILabel chooseKingdomInstructionLbl;
    //[SerializeField] private GameObject chooseKingdomNoChoicesGO;

    [Space(10)]
    [Header("Minimap")]
    [SerializeField] private GameObject minimapGO;
    [SerializeField] private GameObject minimapTextureGO;

    [Space(10)]
    [Header("Notification Area")]
    [SerializeField] private UITable notificationParent;
    [SerializeField] private UIScrollView notificationScrollView;

 //   [Space(10)]
	//[Header("Alliance List")]
	//[SerializeField] private UILabel allianceSummaryLbl;

    [Space(10)]
	[Header("Notification History")]
	[SerializeField] private GameObject notificationHistoryGO;
    [SerializeField] private UITable notificationHistoryTable;
    [SerializeField] private UIScrollView notificationHistoryScrollView;
    [SerializeField] private UILabel notificationHistoryLbl;
    [SerializeField] private GameObject logHistoryGO;
    [SerializeField] private GameObject warHistoryGO;
    [SerializeField] private UITable warHistoryTable;
	[SerializeField] public GameObject notificationCityHistoryGO;
	[SerializeField] private UITable notificationCityHistoryTable;
	[SerializeField] private UILabel notificationCityHistoryLbl;

    [Space(10)]
    [Header("World Info Menu")]
    [SerializeField] private GameObject worldInfoSelectedGO;
    [SerializeField] private GameObject worldInfoCharactersBtn;
    //[SerializeField] private GameObject worldInfoAllianceBtn;
    //[SerializeField] private GameObject worldInfoWarsBtn;
    [SerializeField] private GameObject worldInfoQuestsBtn;
    [SerializeField] private GameObject worldInfoStorylinesBtn;

    //[Space(10)]
    //[Header("World History Menu")]
    //[SerializeField] private WorldHistoryUI worldHistoryUI;

    [Space(10)]
    [Header("Faction Summary Menu")]
    [SerializeField] private FactionSummaryUI factionSummaryUI;

    [Space(10)]
    [Header("Kingdom Info Preview")]
    [SerializeField] private GameObject kingdomInfoPreviewGO;
    [SerializeField] private UILabel kingdomInfoPreviewLbl;

    //private List<MarriedCouple> marriageHistoryOfCurrentCitizen;
	private int currentMarriageHistoryIndex;
	//internal Citizen currentlyShowingCitizen = null;
	//internal City currentlyShowingCity = null;
	//internal Kingdom currentlyShowingKingdom = null;
	//private GameEvent currentlyShowingEvent;
	//private KingdomRelationship currentlyShowingRelationship;
	private GameObject lastClickedEventType = null;
	internal object currentlyShowingLogObject = null;
    //private Kingdom currentKingdomRelationshipShowing;
    //private Kingdom currentlyShowingKingdomCities;

    [Space(10)] //FOR TESTING
    [Header("For Testing")]
    public GameObject goCreateEventUI;
	public GameObject goRaid;
	public GameObject goStateVisit;
	public GameObject goMarriageInvitation;
	public GameObject goPowerGrab;
	public GameObject goExpansion;
	public GameObject goInvasionPlan;
	public GameObject goAlliance;
	public UIPopupList eventDropdownList;
	public UILabel eventDropdownCurrentSelectionLbl;
    public UILabel forTestingLoyaltyLbl;
	public GameObject goLoyalty;
    public ButtonToggle toggleBordersBtn;

    //	[Space(10)] //Settlement Related UI
    //	public GameObject plagueIconGO;

    public delegate void OnPauseEventExpiration(bool state);
	public OnPauseEventExpiration onPauseEventExpiration;

    [Space(10)]
    [Header("Font Sizes")]
    [SerializeField] private int HEADER_FONT_SIZE = 25;
    [SerializeField] private int BODY_FONT_SIZE = 20;
    [SerializeField] private int TOOLTIP_FONT_SIZE = 18;
    [SerializeField] private int SMALLEST_FONT_SIZE = 12;

    private const int KINGDOM_EXPIRY_DAYS = 30;

    //private Dictionary<DateTime, Kingdom> kingdomDisplayExpiry;

	internal List<object> eventLogsQueue = new List<object> ();

	internal string warAllianceState = string.Empty;

    private List<NotificationItem> notificationItemsThatCanBeReused;
    private List<Log> logHistory;
	private bool isShowKingdomHistoryOnly;

    private List<UIMenuSettings> _menuHistory;

    #region getters/setters
    internal GameObject minimapTexture {
        get { return minimapTextureGO; }
    }
    internal List<UIMenuSettings> menuHistory {
        get { return _menuHistory; }
    }
    #endregion

    void Awake(){
		Instance = this;
        logHistory = new List<Log>();
        notificationItemsThatCanBeReused = new List<NotificationItem>();
        _menuHistory = new List<UIMenuSettings>();
        //kingdomSummaryEntries = Utilities.GetComponentsInDirectChildren<KingdomSummaryEntry>(kingdomSummaryGrid.gameObject);
        //onAddNewBattleLog += UpdateBattleLogs;
    }

	void Start(){
        //Messenger.AddListener(Signals.DAY_END, CheckForKingdomExpire);
        Messenger.AddListener("UpdateUI", UpdateUI);
        //EventManager.Instance.onKingdomDiedEvent.AddListener(CheckIfShowingKingdomIsAlive);
        //EventManager.Instance.onCreateNewKingdomEvent.AddListener(AddKingdomToList);
        //EventManager.Instance.onKingdomDiedEvent.AddListener(QueueKingdomForRemoval);
        NormalizeFontSizes();
        ToggleBorders();
        toggleBordersBtn.SetClickState(true);
		isShowKingdomHistoryOnly = false;
  //      PopulateHistoryTable();
		//PopulateCityHistoryTable ();
        UpdateUI();
	}

    internal void InitializeUI() {
        for (int i = 0; i < allMenus.Length; i++) {
            allMenus[i].Initialize();
        }
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.BackQuote)) {
            if (GameManager.Instance.allowConsole) {
                ToggleConsole();
            }
        }
        if (!consoleUI.isShowing) {
            if (Input.GetKeyDown(KeyCode.Space)) {
                if (GameManager.Instance.isPaused) {
                    Unpause();
                    if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X1) {
                        SetProgressionSpeed1X();
                    } else if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X2) {
                        SetProgressionSpeed2X();
                    } else if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X4) {
                        SetProgressionSpeed4X();
                    }
                } else {
                    Pause();
                }
            }
        }
  
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
        UILabel[] allLabels = this.GetComponentsInChildren<UILabel>(true);
        //Debug.Log ("ALL LABELS COUNT: " + allLabels.Length.ToString());
        for (int i = 0; i < allLabels.Length; i++) {
            NormalizeFontSizeOfLabel(allLabels[i]);
        }
    }
    private void NormalizeFontSizeOfLabel(UILabel lbl) {
        string lblName = lbl.name;
        UILabel.Overflow overflowMethod = UILabel.Overflow.ClampContent;
        if (lblName.Contains("HEADER")) {
            lbl.fontSize = HEADER_FONT_SIZE;
            overflowMethod = UILabel.Overflow.ClampContent;
        } else if (lblName.Contains("BODY")) {
            lbl.fontSize = BODY_FONT_SIZE;
            overflowMethod = UILabel.Overflow.ClampContent;
        } else if (lblName.Contains("TOOLTIP")) {
            lbl.fontSize = TOOLTIP_FONT_SIZE;
            overflowMethod = UILabel.Overflow.ResizeHeight;
        } else if (lblName.Contains("SMALLEST")) {
            lbl.fontSize = SMALLEST_FONT_SIZE;
            overflowMethod = UILabel.Overflow.ClampContent;
        }

        if (!lblName.Contains("NO")) {
            lbl.overflowMethod = overflowMethod;
        }

    }
    #endregion

    private void UpdateUI(){
        dateLbl.text = GameManager.Instance.days.ToString() + " " + LocalizationManager.Instance.GetLocalizedValue("General", "Months", ((MONTH)GameManager.Instance.month).ToString()) + ", " + GameManager.Instance.year.ToString ();
  //      KingdomManager.Instance.UpdateKingdomList();
		//UpdateAllianceSummary ();
  //      if (currentlyShowingKingdom != null) {
  //          UpdateKingdomInfo();
  //      }

		//if (eventLogsGO.activeSelf) {
		//	if (currentlyShowingLogObject != null) {
		//		ShowEventLogs (currentlyShowingLogObject);
		//	}
		//}

  //      if (allKingdomEventsGO.activeSelf) {
  //          if (currentlyShowingKingdom != null) {
  //              if (kingdomCurrentEventsGO.activeSelf) {
  //                  ShowKingdomCurrentEvents();
  //              } else if (kingdomHistoryGO.activeSelf) {
  //                  ShowKingdomPastEvents();
  //              }
  //          }
  //      }
  //      if (relationshipsGO.activeSelf) {
  //          if (currentlyShowingKingdom != null) {
  //              ShowKingRelationships();
  //          }
  //      }
        
  //      if (kingdomCitiesGO.activeSelf) {
  //          if (currentlyShowingKingdom != null) {
  //              ShowKingdomCities();
  //          }
  //      }

  //      if (createKingdomGO.activeSelf) {
  //          UpdateCreateKingdomErrorMessage();
  //      }
	}

    #region World Controls
    public void SetProgressionSpeed1X() {
        Unpause();
        GameManager.Instance.SetProgressionSpeed(PROGRESSION_SPEED.X1);
        x1Btn.SetAsClicked();
    }
    public void SetProgressionSpeed2X() {
        Unpause();
        GameManager.Instance.SetProgressionSpeed(PROGRESSION_SPEED.X2);
        x2Btn.SetAsClicked();
    }
    public void SetProgressionSpeed4X() {
        Unpause();
        GameManager.Instance.SetProgressionSpeed(PROGRESSION_SPEED.X4);
        x4Btn.SetAsClicked();
    }
    public void Pause() {
        GameManager.Instance.SetPausedState(true);
        pauseBtn.SetAsClicked();
        if (onPauseEventExpiration != null) {
            onPauseEventExpiration(true);
        }
    }
    private void Unpause() {
        GameManager.Instance.SetPausedState(false);
        if (onPauseEventExpiration != null) {
            onPauseEventExpiration(false);
        }
    }
    #endregion

  //  #region Kingdom Info
  //  internal void SetKingdomAsActive(Kingdom kingdom) {
  //      //if(currentlyShowingKingdom != null) {
  //      //    currentlyShowingKingdom.UnHighlightAllOwnedTilesInKingdom();
  //      //}
  //      currentlyShowingKingdom = kingdom;

  //      currentlyShowingKingdom.UpdateFogOfWarVisual();
  //      UpdateMinimapInfo();
  //      //currentlyShowingKingdom.HighlightAllOwnedTilesInKingdom();
  //      //CameraMove.Instance.CenterCameraOn(currentlyShowingKingdom.capitalCity.hexTile.gameObject);

  //      //RemoveAllNotifications();

  //      //Load all current active events of kingdom
  //      //List<GameEvent> activeGameEventsStartedByKingdom = currentlyShowingKingdom.activeEvents.Where(x => !Utilities.eventsNotToShow.Contains(x.eventType)).ToList();;
  //      //List<EventItem> presentEventItems = activeKingdomFlag.eventsGrid.GetChildList().Select(x => x.GetComponent<EventItem>()).ToList();

  //      //if(activeGameEventsStartedByKingdom.Count > presentEventItems.Count) {
  //      //    int missingItems = activeGameEventsStartedByKingdom.Count - presentEventItems.Count;
  //      //    for (int i = 0; i < missingItems; i++) {
  //      //        GameObject eventGO = InstantiateUIObject(gameEventPrefab.name, activeKingdomFlag.eventsGrid.transform);
  //      //        activeKingdomFlag.AddGameObjectToGrid(eventGO);
  //      //        presentEventItems.Add(eventGO.GetComponent<EventItem>());
  //      //    }
  //      //}

  //      //for (int i = 0; i < presentEventItems.Count; i++) {
  //      //    EventItem currItem = presentEventItems[i];
  //      //    GameEvent currGameEvent = activeGameEventsStartedByKingdom.ElementAtOrDefault(i);
  //      //    if(currGameEvent != null) {
  //      //        currItem.SetEvent(currGameEvent);
  //      //        currItem.gameObject.SetActive(true);
  //      //    } else {
  //      //        currItem.gameObject.SetActive(false);
  //      //    }
  //      //}


  //      //		if (kingdomInfoGO.activeSelf) {
  //      //			ShowKingdomInfo(currentlyShowingKingdom);
  //      //		}
  //      if (citizenInfoGO.activeSelf) {
  //          ShowCitizenInfo(currentlyShowingKingdom.king);
  //      }
  //      if (allKingdomEventsGO.activeSelf) {
  //          //ShowKingdomEvents();
  //          if (kingdomCurrentEventsGO.activeSelf) {
  //              ShowKingdomCurrentEvents();
  //          } else if (kingdomHistoryGO.activeSelf) {
  //              ShowKingdomPastEvents();
  //          }
  //      }
  //      if (relationshipsGO.activeSelf) {
  //          ShowKingRelationships();
  //      }
  //      if (kingdomCitiesGO.activeSelf) {
  //          ShowKingdomCities();
  //      }
  //      UpdateKingdomInfo();
  //      if (this.notificationHistoryGO.activeSelf && this.logHistoryGO.activeSelf && this.isShowKingdomHistoryOnly) {
  //          ShowLogHistory();
  //      }
  //  }
  //  internal void PreviewKingdomInfo(Kingdom kingdom) {
  //      string text = "[b]" + kingdom.name + "[/b]\n";

  //      text += "\n [b]Kingdom Food S/D:[/b] " + kingdom.cities.Count.ToString() + "/" + kingdom.foodCityCapacity +
  //      "\n [b]Kingdom Material For Humans S/D:[/b] " + ((kingdom.race == RACE.HUMANS) ? kingdom.cities.Count.ToString() : "0") + "/" + kingdom.materialCityCapacityForHumans +
  //      "\n [b]Kingdom Material For Elves S/D:[/b] " + ((kingdom.race == RACE.ELVES) ? kingdom.cities.Count.ToString() : "0") + "/" + kingdom.materialCityCapacityForElves +
  //      "\n [b]Kingdom Ore S/D:[/b] " + kingdom.cities.Count.ToString() + "/" + kingdom.oreCityCapacity +
  //      "\n [b]Kingdom Base Weapons:[/b] " + kingdom.baseWeapons.ToString() +
  //      "\n [b]Weapons Over Production:[/b] " + kingdom.GetWeaponOverProductionPercentage().ToString() + "%" +
  //      "\n [b]Kingdom Type:[/b] " + kingdom.kingdomType.ToString() +
  //      "\n [b]Kingdom Size:[/b] " + kingdom.kingdomSize.ToString() +
  //      "\n [b]Draft Rate: [/b]" + (kingdom.draftRate * 100f).ToString() + "%" +
  //      "\n [b]Research Rate: [/b]" + (kingdom.researchRate * 100f).ToString() + "%" +
  //      "\n [b]Production Rate: [/b]" + (kingdom.productionRate * 100f).ToString() + "%";
		//text += "\n[b]Trade Deals: [/b]\n";
		//if (kingdom.kingdomsInTradeDealWith.Count > 0) {
		//	for (int i = 0; i < kingdom.kingdomsInTradeDealWith.Count; i++) {
		//		text += kingdom.kingdomsInTradeDealWith[i].name + "\n";
		//	}
		//} else {
		//	text += "NONE\n";
		//}
		//text += "[b]Adjacent Kingdoms: [/b]\n";
		//if (kingdom.adjacentKingdoms.Count > 0) {
		//	for (int i = 0; i < kingdom.adjacentKingdoms.Count; i++) {
		//		text += kingdom.adjacentKingdoms[i].name + "\n";
		//	}
		//} else {
		//	text += "NONE\n";
		//}
  //      kingdomInfoPreviewLbl.text = text;

  //      var v3 = Input.mousePosition;
  //      v3.z = 10.0f;
  //      v3 = uiCamera.GetComponent<Camera>().ScreenToWorldPoint(v3);
  //      v3.x -= 0.30f;
  //      kingdomInfoPreviewGO.transform.position = v3;
  //      kingdomInfoPreviewGO.SetActive(true);
  //  }
  //  internal void HideKingdomInfoPreview() {
  //      kingdomInfoPreviewGO.SetActive(false);
  //  }
  //  private void UpdateKingdomInfo() {
  //      //currentlyShowingKingdom.UpdateFogOfWarVisual();
  //      kingdomListActiveKing.SetCitizen(currentlyShowingKingdom.king); //King

  //      //Emblem
  //      Color emblemShieldColor = currentlyShowingKingdom.kingdomColor;
  //      emblemShieldColor.a = 255f / 255f;
  //      kingdomListEmblemBG.sprite2D = currentlyShowingKingdom.emblemBG;
  //      kingdomListEmblemBG.color = emblemShieldColor;
  //      kingdomListEmblemBG.MakePixelPerfect();
  //      kingdomListEmblemBG.width += Mathf.FloorToInt(kingdomListEmblemBG.width * 0.25f);

  //      kingdomListEmblemOutline.sprite2D = currentlyShowingKingdom.emblemBG;
  //      kingdomListEmblemOutline.MakePixelPerfect();
  //      Color outlineColor;
  //      ColorUtility.TryParseHtmlString("#2d2e2e", out outlineColor);
  //      kingdomListEmblemOutline.color = outlineColor;
  //      kingdomListEmblemOutline.width = (kingdomListEmblemBG.width + 8);

  //      kingdomListEmblemSprite.sprite2D = currentlyShowingKingdom.emblem;
  //      kingdomListEmblemSprite.MakePixelPerfect();
  //      kingdomListEmblemSprite.width += Mathf.FloorToInt(kingdomListEmblemSprite.width * 0.25f);

  //      kingdomNameLbl.text = currentlyShowingKingdom.name; //Kingdom Name
  //      kingdomUnrestLbl.text = currentlyShowingKingdom.stability.ToString(); //Unrest
  //      kingdomTechLbl.text = currentlyShowingKingdom.techLevel.ToString(); //Tech
  //      UpdateTechMeter();
  //      this.militarizingGO.SetActive(currentlyShowingKingdom.isMilitarize);
  //      this.fortifyingGO.SetActive(currentlyShowingKingdom.isFortifying);
  //      this.actionDayLbl.text = this.currentlyShowingKingdom.actionDay.ToString();
  //      this.warmongerLbl.text = this.currentlyShowingKingdom.warmongerValue.ToString();
  //      populationSummary.text = currentlyShowingKingdom.population.ToString() + "/" + currentlyShowingKingdom.populationCapacity.ToString();
  //      //		float newValue = (float)currentlyShowingKingdom.techCounter / (float)currentlyShowingKingdom.techCapacity;
  //      //		float oldValue = kingdomTechMeter.value;
  //      //		kingdomTechMeter.value = iTween.FloatUpdate(oldValue, newValue, GameManager.Instance.progressionSpeed);
  //      //Basic Resource
  //      if (currentlyShowingKingdom.basicResource == BASE_RESOURCE_TYPE.STONE) {
  //          kingdomBasicResourceSprite.sprite2D = stoneSprite;
  //      } else if (currentlyShowingKingdom.basicResource == BASE_RESOURCE_TYPE.WOOD) {
  //          kingdomBasicResourceSprite.sprite2D = lumberSprite;
  //      }
  //      //kingdomBasicResourceLbl.text = currentlyShowingKingdom.basicResourceCount.ToString();

  //      //Available Resources
  //      List<Transform> children = kingdomOtherResourcesGrid.GetChildList();
  //      List<RESOURCE> resourcesInGrid = children.Where(x => x.GetComponent<ResourceIcon>() != null).Select(x => x.GetComponent<ResourceIcon>().resource).ToList();

  //      //List<RESOURCE> allOtherResources = currentlyShowingKingdom.availableResources.Keys.ToList();
  //      //if (resourcesInGrid.Except(allOtherResources).Any() || allOtherResources.Except(resourcesInGrid).Any()) {
  //      //    for (int i = 0; i < children.Count; i++) {
  //      //        kingdomOtherResourcesGrid.RemoveChild(children[i]);
  //      //        ObjectPoolManager.Instance.DestroyObject(children[i].gameObject);
  //      //    }
  //      //    for (int i = 0; i < currentlyShowingKingdom.availableResources.Keys.Count; i++) {
  //      //        RESOURCE currResource = currentlyShowingKingdom.availableResources.Keys.ElementAt(i);
  //      //        GameObject resourceGO = InstantiateUIObject(resourceIconPrefab.name, this.transform);
  //      //        resourceGO.GetComponent<ResourceIcon>().SetResource(currResource);
  //      //        resourceGO.transform.localScale = Vector3.one;
  //      //        kingdomOtherResourcesGrid.AddChild(resourceGO.transform);
  //      //        kingdomOtherResourcesGrid.Reposition();
  //      //    }
  //      //    RepositionGridCallback(kingdomOtherResourcesGrid);
  //      //}

  //      //currentlyShowingKingdom.HighlightAllOwnedTilesInKingdom();
  //  }
  //  internal void UpdateTechMeter() {
  //      kingdomTechMeter.value = (float)currentlyShowingKingdom.techCounter / (float)currentlyShowingKingdom.techCapacity;
  //  }
  //  internal void CheckIfShowingKingdomIsAlive(Kingdom kingdom) {
  //      if (currentlyShowingKingdom.id == kingdom.id) {
  //          SetKingdomAsActive(KingdomManager.Instance.allKingdoms.FirstOrDefault());
  //      }
  //  }
  //  #endregion

    #region Minimap
    internal void UpdateMinimapInfo() {
        //for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
        //    Region currRegion = GridMap.Instance.allRegions[i];
        //    currRegion.ShowNaturalResourceLevelForRace(currentlyShowingKingdom.race);
        //}
        CameraMove.Instance.UpdateMinimapTexture();
    }
    #endregion

    //#region Citizen Info
    //internal void ShowCitizenInfo(Citizen citizenToShow) {
    //    currentlyShowingCitizen = citizenToShow;
    //    if (relationshipsGO.activeSelf) {
    //        if (currentlyShowingCitizen.isKing) {
    //            if (kingRelationshipsParentGO.activeSelf) {
    //                ShowKingRelationships();
    //            }
    //            //            else {
    //            //	ShowRelationships();
    //            //	ShowGovernorRelationships();
    //            //}
    //            HideRelationshipHistory();
    //        } else {
    //            HideRelationships();
    //        }
    //    }
    //    if (familyTreeGO.activeSelf) {
    //        HideFamilyTree();
    //    }
    //    if (citizenHistoryGO.activeSelf) {
    //        ShowCitizenHistory();
    //    }
    //    if (relocateGO.activeSelf) {
    //        HideRelocate();
    //    }

    //    //ForTesting
    //    citizenInfoForTestingGO.SetActive(true);
    //    preferredKingdomTypeLbl.text = currentlyShowingCitizen.balanceType.ToString();
    //    loyaltyToKingLbl.text = currentlyShowingCitizen.loyaltyToKing.ToString();
    //    citizenWarmongerLbl.text = currentlyShowingCitizen.warmonger.ToString() + " WARMONGERING";

    //    HideSmallInfo();

    //    citizenNameLbl.text = currentlyShowingCitizen.name;
    //    ROLE roleOfCitizen = currentlyShowingCitizen.role;
    //    if (currentlyShowingCitizen.city != null) {
    //        switch (roleOfCitizen) {
    //            case ROLE.UNTRAINED:
    //                citizenRoleAndKingdomLbl.text = "Citizen of " + currentlyShowingCitizen.city.name;
    //                break;
    //            case ROLE.GOVERNOR:
    //                citizenRoleAndKingdomLbl.text = "Governor of " + currentlyShowingCitizen.city.name;
    //                break;
    //            case ROLE.KING:
    //                if (currentlyShowingCitizen.gender == GENDER.MALE) {
    //                    citizenRoleAndKingdomLbl.text = "King of " + currentlyShowingCitizen.city.kingdom.name;
    //                } else {
    //                    citizenRoleAndKingdomLbl.text = "Queen of " + currentlyShowingCitizen.city.kingdom.name;
    //                }
    //                break;
    //            case ROLE.QUEEN:
    //                if (currentlyShowingCitizen.gender == GENDER.MALE) {
    //                    citizenRoleAndKingdomLbl.text = "Queen's Consort of " + currentlyShowingCitizen.city.kingdom.name;
    //                } else {
    //                    citizenRoleAndKingdomLbl.text = "Queen of " + currentlyShowingCitizen.city.kingdom.name;
    //                }
    //                break;
    //            default:
    //                string role = Utilities.NormalizeString(roleOfCitizen.ToString());
    //                citizenRoleAndKingdomLbl.text = role + " of " + currentlyShowingCitizen.city.kingdom.name;
    //                break;
    //        }
    //        citizenCityNameLbl.text = currentlyShowingCitizen.city.name;
    //        ctizenPortraitBG.color = currentlyShowingCitizen.city.kingdom.kingdomColor;
    //    } else {
    //        citizenRoleAndKingdomLbl.text = Utilities.NormalizeString(roleOfCitizen.ToString());
    //        citizenCityNameLbl.text = "No City";
    //        ctizenPortraitBG.color = Color.white;
    //    }

    //    citizenAgeLbl.text = "Age: " + currentlyShowingCitizen.age.ToString();

    //    if (currentlyShowingCitizen.isDead) {
    //        citizenInfoIsDeadIcon.SetActive(true);
    //    } else {
    //        citizenInfoIsDeadIcon.SetActive(false);
    //    }

    //    //if (citizenToShow.isKing) {
    //    //	characterValuesBtn.gameObject.SetActive(true);
    //    //} else {
    //    //          characterValuesBtn.gameObject.SetActive(false);
    //    //}

    //    //HideCityInfo();
    //    citizenInfoGO.SetActive(true);
    //    this.marriageHistoryOfCurrentCitizen = MarriageManager.Instance.GetCouplesCitizenInvoledIn(citizenToShow);

    //    //HideGovernorLoyalty ();
    //    //this.citizenInfoUI.SetTraits(currentlyShowingCitizen);
    //    //Utilities.GetColorForTrait(currentlyShowingCitizen.efficiency);
    //    //      if (citizenToShow.assignedRole != null){
    //    //	if (citizenToShow.assignedRole is Governor) {
    //    //		Governor governor = (Governor)citizenToShow.assignedRole;
    //    //		//ShowGovernorLoyalty ();
    //    //		this.citizenInfoUI.SetGovernorTraits (governor);
    //    //	} else if (citizenToShow.assignedRole is King) {
    //    //		King king = (King)citizenToShow.assignedRole;
    //    //		this.citizenInfoUI.SetKingTraits (king);
    //    //	}
    //    //}

    //    List<TRAIT> traitsToShow = new List<TRAIT>();
    //    if (currentlyShowingCitizen.charisma != TRAIT.NONE) {
    //        traitsToShow.Add(currentlyShowingCitizen.charisma);
    //    }
    //    if (currentlyShowingCitizen.intelligence != TRAIT.NONE) {
    //        traitsToShow.Add(currentlyShowingCitizen.intelligence);
    //    }
    //    if (currentlyShowingCitizen.efficiency != TRAIT.NONE) {
    //        traitsToShow.Add(currentlyShowingCitizen.efficiency);
    //    }
    //    //if (currentlyShowingCitizen.science != SCIENCE.NEUTRAL) {
    //    //    traitsToShow.Add(currentlyShowingCitizen.science);
    //    //}
    //    if (currentlyShowingCitizen.military != TRAIT.NONE) {
    //        traitsToShow.Add(currentlyShowingCitizen.military);
    //    }
    //    //if (currentlyShowingCitizen.loyalty != LOYALTY.NEUTRAL) {
    //    //    traitsToShow.Add(currentlyShowingCitizen.loyalty);
    //    //}

    //    traitsToShow.AddRange(currentlyShowingCitizen.otherTraits);
    //    //traits
    //    for (int i = 0; i < citizenTraitIcons.Length; i++) {
    //        TraitIcon currIcon = citizenTraitIcons[i];
    //        TRAIT trait = traitsToShow.ElementAtOrDefault(i);
    //        if (trait == TRAIT.NONE) {
    //            currIcon.gameObject.SetActive(false);
    //        } else {
    //            currIcon.SetTrait(trait);
    //            currIcon.gameObject.SetActive(true);
    //        }
    //    }
    //}
    //public void HideCitizenInfo() {
    //    currentlyShowingCitizen = null;
    //    citizenInfoGO.SetActive(false);
    //    HideFamilyTree();
    //    //HideGovernorLoyalty ();
    //}
    //public void ToggleCitizenHistory() {
    //    if (this.citizenHistoryGO.activeSelf) {
    //        this.citizenHistoryGO.SetActive(false);
    //    } else {
    //        ShowCitizenHistory();
    //    }
    //}
    //public void ShowCitizenHistory() {
    //    if (this.currentlyShowingCitizen == null) {
    //        return;
    //    }

    //    List<Transform> children = this.citizenHistoryGrid.GetChildList();
    //    for (int i = 0; i < children.Count; i++) {
    //        ObjectPoolManager.Instance.DestroyObject(children[i].gameObject);
    //    }

    //    for (int i = 0; i < this.currentlyShowingCitizen.history.Count; i++) {
    //        GameObject citizenGO = InstantiateUIObject(this.historyPortraitPrefab.name, this.citizenHistoryGrid.transform);
    //        citizenGO.GetComponent<HistoryPortrait>().SetHistory(this.currentlyShowingCitizen.history[i]);
    //        citizenGO.transform.localScale = Vector3.one;
    //        citizenGO.transform.localPosition = Vector3.zero;
    //    }

    //    StartCoroutine(RepositionGrid(this.citizenHistoryGrid));
    //    this.citizenHistoryGO.SetActive(true);
    //}
    //public void HideCitizenHistory() {
    //    citizenHistoryBtn.SetClickState(false);
    //    this.citizenHistoryGO.SetActive(false);
    //}
    //public void ShowCurrentlyShowingCitizenCityInfo() {
    //    //ShowCityInfo (currentlyShowingCitizen.city, true);
    //    CameraMove.Instance.CenterCameraOn(currentlyShowingCitizen.city.hexTile.gameObject);
    //}
    //public void ShowGovernorInfo() {
    //    if (this.currentlyShowingCity != null) {
    //        this.ShowCitizenInfo(this.currentlyShowingCity.governor);
    //    }
    //}
    //#endregion

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

    //#region Relationships Menu
    //public void UpdateRelationships() {
    //    if (relationshipsGO.activeSelf) {
    //        if (currentlyShowingKingdom != null) {
    //            ShowKingRelationships();
    //        }
    //    }
    //}
    //public void ToggleRelationships() {
    //    if (relationshipsGO.activeSelf) {
    //        //kingRelationshipsParentGO.SetActive(false);
    //        //relationshipsGO.SetActive (false);
    //        HideRelationships();
    //    } else {
    //        ShowKingRelationships();
    //    }
    //}
    //public void ShowKingRelationships() {
    //    HideKingdomCities();
    //    List<CharacterPortrait> characterPortraits = kingRelationshipsGrid.gameObject.GetComponentsInChildren<Transform>(true)
    //                                                .Where(x => x.GetComponent<CharacterPortrait>() != null)
    //                                                .Select(x => x.GetComponent<CharacterPortrait>()).ToList();

    //    int nextIndex = 0;
    //    List<KingdomRelationship> relationshipsToShow = currentlyShowingKingdom.relationships
    //        .Where(x => currentlyShowingKingdom.discoveredKingdoms.Contains(x.Value.targetKingdom)).Select(x => x.Value).ToList();

    //    if (relationshipsToShow.Count > 0) {
    //        kingRelationshipLine.SetActive(true);
    //        kingNoRelationshipsGO.SetActive(false);
    //    } else {
    //        kingRelationshipLine.SetActive(false);
    //        kingNoRelationshipsGO.SetActive(true);
    //    }

    //    for (int i = 0; i < characterPortraits.Count; i++) {
    //        CharacterPortrait currPortrait = characterPortraits[i];
    //        if (i < relationshipsToShow.Count) {
    //            KingdomRelationship currRel = relationshipsToShow[i];
    //            if (currRel != null) {
    //                currPortrait.SetCitizen(currRel.targetKingdom.king, true);
    //                currPortrait.ShowRelationshipLine(currRel, currRel.targetKingdom.GetRelationshipWithKingdom(currentlyShowingKingdom));
    //                currPortrait.gameObject.SetActive(true);
    //            } else {
    //                currPortrait.gameObject.SetActive(false);
    //            }
    //            nextIndex = i + 1;
    //        } else {
    //            currPortrait.gameObject.SetActive(false);
    //        }
    //    }

    //    if (relationshipsToShow.Count - 1 >= nextIndex) {
    //        for (int i = nextIndex; i < relationshipsToShow.Count; i++) {
    //            KingdomRelationship rel = relationshipsToShow[i];

    //            GameObject kingGO = InstantiateUIObject(characterPortraitPrefab.name, this.transform);
    //            kingGO.GetComponent<CharacterPortrait>().SetCitizen(rel.targetKingdom.king, true);
    //            kingGO.transform.localScale = Vector3.one;
    //            kingGO.GetComponent<CharacterPortrait>().ShowRelationshipLine(rel,
    //                rel.targetKingdom.GetRelationshipWithKingdom(currentlyShowingKingdom));
    //            kingRelationshipsGrid.AddChild(kingGO.transform);
    //            kingRelationshipsGrid.Reposition();
    //        }

    //        if (relationshipsGO.activeSelf) {
    //            StartCoroutine(RepositionScrollView(kingRelationshipsScrollView));
    //        }
    //    }

    //    if (currentKingdomRelationshipShowing == null || currentKingdomRelationshipShowing.id != currentlyShowingKingdom.id || !relationshipsGO.activeSelf) {
    //        StartCoroutine(RepositionGrid(kingRelationshipsGrid));
    //        StartCoroutine(RepositionScrollView(kingRelationshipsScrollView));
    //    }

    //    relationshipsGO.SetActive(true);
    //    kingdomListRelationshipButton.SetClickState(true);
    //    currentKingdomRelationshipShowing = currentlyShowingKingdom;
    //}
    //public void HideRelationships() {
    //    //kingRelationshipsParentGO.SetActive (false);
    //    relationshipsGO.SetActive(false);
    //    //relationshipsBtn.SetClickState(false);
    //    kingdomListRelationshipButton.SetClickState(false);
    //}
    //public void HideRelationshipHistory() {
    //    relationshipHistoryGO.SetActive(false);
    //}
    //public void ShowRelationshipSummary(Citizen citizen, string summary) {
    //    if (citizen.assignedRole is Governor) {
    //        relationshipSummaryTitleLbl.text = "Loyalty";
    //    } else if (citizen.assignedRole is King) {
    //        relationshipSummaryTitleLbl.text = "Affinity";
    //    }
    //    relationshipSummaryLbl.text = summary;
    //    relationshipSummaryGO.SetActive(true);
    //}
    //public void HideRelationshipSummary() {
    //    relationshipSummaryGO.SetActive(false);
    //}
    //#endregion

    #region Tooltips
    public void ShowSmallInfo(string info) {
        smallInfoLbl.text = info;

        var v3 = Input.mousePosition;
        v3.z = 10.0f;
        v3 = uiCamera.GetComponent<Camera>().ScreenToWorldPoint(v3);
        v3.y -= 0.15f;

        //Bounds uiCameraBounds = uiCamera.GetComponent<Camera>().bound

        if (v3.y <= 0f) {
            v3 = Input.mousePosition;
            v3.z = 10.0f;
            v3 = uiCamera.GetComponent<Camera>().ScreenToWorldPoint(v3);
            v3.y += 0.1f;
        }
        if (v3.x >= -13.8f) {
            v3 = Input.mousePosition;
            v3.z = 10.0f;
            v3 = uiCamera.GetComponent<Camera>().ScreenToWorldPoint(v3);
            v3.x -= 0.2f;
        }

        smallInfoGO.transform.position = v3;
        smallInfoGO.SetActive(true);
    }
    public void HideSmallInfo() {
        smallInfoGO.SetActive(false);
        smallInfoGO.transform.parent = this.transform;
    }
    #endregion
    //#region Family Tree
    //public void ToggleFamilyTree() {
    //    if (familyTreeGO.activeSelf) {
    //        HideFamilyTree();
    //    } else {
    //        ShowFamilyTree();
    //    }
    //}
    //public void ShowFamilyTree() {
    //    if (familyTreeFatherGO.GetComponentInChildren<CharacterPortrait>() != null) {
    //        ObjectPoolManager.Instance.DestroyObject(familyTreeFatherGO.GetComponentInChildren<CharacterPortrait>().gameObject);
    //    }
    //    if (currentlyShowingCitizen.father != null) {
    //        GameObject fatherGO = InstantiateUIObject(characterPortraitPrefab.name, familyTreeFatherGO.transform);
    //        fatherGO.transform.localScale = new Vector3(2.1f, 2.1f, 0f);
    //        fatherGO.transform.localPosition = Vector3.zero;
    //        fatherGO.GetComponent<CharacterPortrait>().SetCitizen(currentlyShowingCitizen.father);
    //    }
    //    if (familyTreeMotherGO.GetComponentInChildren<CharacterPortrait>() != null) {
    //        ObjectPoolManager.Instance.DestroyObject(familyTreeMotherGO.GetComponentInChildren<CharacterPortrait>().gameObject);
    //    }
    //    if (currentlyShowingCitizen.mother != null) {
    //        GameObject motherGO = InstantiateUIObject(characterPortraitPrefab.name, familyTreeMotherGO.transform);
    //        motherGO.transform.localScale = new Vector3(2.1f, 2.1f, 0f);
    //        motherGO.transform.localPosition = Vector3.zero;
    //        motherGO.GetComponent<CharacterPortrait>().SetCitizen(currentlyShowingCitizen.mother);
    //    }
    //    if (familyTreeSpouseGO.GetComponentInChildren<CharacterPortrait>() != null) {
    //        ObjectPoolManager.Instance.DestroyObject(familyTreeSpouseGO.GetComponentInChildren<CharacterPortrait>().gameObject);
    //    }
    //    if (currentlyShowingCitizen.spouse != null) {
    //        GameObject spouseGO = InstantiateUIObject(characterPortraitPrefab.name, familyTreeSpouseGO.transform);
    //        spouseGO.transform.localScale = new Vector3(2.1f, 2.1f, 0f);
    //        spouseGO.transform.localPosition = Vector3.zero;
    //        spouseGO.GetComponent<CharacterPortrait>().SetCitizen(currentlyShowingCitizen.spouse);
    //        if (currentlyShowingCitizen.spouse is Spouse) {
    //            spouseCompatibility.gameObject.SetActive(true);
    //            spouseCompatibility.text = ((Spouse)currentlyShowingCitizen.spouse)._marriageCompatibility.ToString();
    //        } else {
    //            spouseCompatibility.gameObject.SetActive(false);
    //        }

    //        //for (int i = 0; i < this.marriageHistoryOfCurrentCitizen.Count; i++) {
    //        //    if (currentlyShowingCitizen.gender == GENDER.MALE) {
    //        //        if (this.marriageHistoryOfCurrentCitizen[i].wife.id == currentlyShowingCitizen.spouse.id) {
    //        //            this.currentMarriageHistoryIndex = i;
    //        //            break;
    //        //        }
    //        //    } else {
    //        //        if (this.marriageHistoryOfCurrentCitizen[i].husband.id == currentlyShowingCitizen.spouse.id) {
    //        //            this.currentMarriageHistoryIndex = i;
    //        //            break;
    //        //        }
    //        //    }
    //        //}
    //    } else {
    //        spouseCompatibility.gameObject.SetActive(false);
    //    }
    //    //else if (this.marriageHistoryOfCurrentCitizen.Count > 0) {
    //    //    GameObject spouseGO = InstantiateUIObject(characterPortraitPrefab.name, familyTreeSpouseGO.transform);
    //    //    spouseGO.transform.localScale = new Vector3(2.1f, 2.1f, 0f);
    //    //    spouseGO.transform.localPosition = Vector3.zero;
    //    //    if (currentlyShowingCitizen.gender == GENDER.MALE) {
    //    //        spouseGO.GetComponent<CharacterPortrait>().SetCitizen(this.marriageHistoryOfCurrentCitizen[0].wife);
    //    //    } else {
    //    //        spouseGO.GetComponent<CharacterPortrait>().SetCitizen(this.marriageHistoryOfCurrentCitizen[0].husband);
    //    //    }
    //    //    this.currentMarriageHistoryIndex = 0;
    //    //}

    //    CharacterPortrait[] children = familyTreeChildGrid.GetComponentsInChildren<CharacterPortrait>();
    //    for (int i = 0; i < children.Length; i++) {
    //        ObjectPoolManager.Instance.DestroyObject(children[i].gameObject);
    //    }

    //    List<Transform> childPositions = familyTreeChildGrid.GetChildList();
    //    for (int i = 0; i < currentlyShowingCitizen.children.Count; i++) {
    //        GameObject childGO = InstantiateUIObject(characterPortraitPrefab.name, childPositions[i].transform);
    //        childGO.transform.localScale = new Vector3(2.1f, 2.1f, 0f);
    //        childGO.transform.localPosition = Vector3.zero;
    //        childGO.GetComponent<CharacterPortrait>().SetCitizen(currentlyShowingCitizen.children[i]);
    //    }

    //    if (this.marriageHistoryOfCurrentCitizen.Count > 1) {
    //        nextMarriageBtn.SetActive(true);
    //    } else {
    //        nextMarriageBtn.SetActive(false);
    //    }

    //    familyTreeInnerSprite.color = currentlyShowingCitizen.city.kingdom.kingdomColor;

    //    //Show Successor
    //    if (currentlyShowingCitizen.role == ROLE.KING) {
    //        if (currentlyShowingCitizen.city.kingdom.successionLine.Count > 0) {
    //            Citizen successor = currentlyShowingCitizen.city.kingdom.successionLine.FirstOrDefault();
    //            successorPortrait.SetCitizen(successor, false, true);
    //            //successorPreferredKingdomTypeLbl.text = successor.preferredKingdomType.ToString();
    //            successorParentGO.SetActive(true);
    //        } else {
    //            successorParentGO.SetActive(false);
    //        }

    //        //Show Other Citizens
    //        otherCitizensGO.SetActive(true);
    //        chancellorPortrait.SetCitizen(currentlyShowingCitizen.city.kingdom.GetCitizenWithRoleInKingdom(ROLE.GRAND_CHANCELLOR));
    //        marshalPortrait.SetCitizen(currentlyShowingCitizen.city.kingdom.GetCitizenWithRoleInKingdom(ROLE.GRAND_MARSHAL));
    //    } else {
    //        successorParentGO.SetActive(false);
    //        otherCitizensGO.SetActive(false);
    //    }

    //    familyTreeGO.SetActive(true);
    //}
    //public void HideFamilyTree() {
    //    familyTreeBtn.SetClickState(false);
    //    familyTreeGO.SetActive(false);
    //}
    //public void ShowNextMarriage() {
    //    int nextIndex = this.currentMarriageHistoryIndex + 1;
    //    if (nextIndex == this.marriageHistoryOfCurrentCitizen.Count) {
    //        //max index reached
    //        nextIndex = 0;
    //    }

    //    ObjectPoolManager.Instance.DestroyObject(familyTreeSpouseGO.GetComponentInChildren<CharacterPortrait>().gameObject);
    //    CharacterPortrait[] children = familyTreeChildGrid.GetComponentsInChildren<CharacterPortrait>();
    //    for (int i = 0; i < children.Length; i++) {
    //        ObjectPoolManager.Instance.DestroyObject(children[i].gameObject);
    //    }

    //    MarriedCouple marriedCoupleToShow = this.marriageHistoryOfCurrentCitizen[nextIndex];
    //    if (marriedCoupleToShow.husband.id == currentlyShowingCitizen.id) {
    //        //currentlyShowingCitizen is male
    //        GameObject spouseGO = InstantiateUIObject(characterPortraitPrefab.name, familyTreeSpouseGO.transform);
    //        spouseGO.transform.localScale = new Vector3(2.1f, 2.1f, 0f);
    //        spouseGO.transform.localPosition = Vector3.zero;
    //        spouseGO.GetComponent<CharacterPortrait>().SetCitizen(marriedCoupleToShow.wife);
    //    } else {
    //        //currentlyShowingCitizen is female	
    //        GameObject spouseGO = InstantiateUIObject(characterPortraitPrefab.name, familyTreeSpouseGO.transform);
    //        spouseGO.transform.localScale = new Vector3(2.1f, 2.1f, 0f);
    //        spouseGO.transform.localPosition = Vector3.zero;
    //        spouseGO.GetComponent<CharacterPortrait>().SetCitizen(marriedCoupleToShow.husband);
    //    }

    //    List<Transform> childPositions = familyTreeChildGrid.GetChildList();
    //    for (int i = 0; i < marriedCoupleToShow.children.Count; i++) {
    //        GameObject childGO = InstantiateUIObject(characterPortraitPrefab.name, childPositions[i].transform);
    //        childGO.transform.localScale = new Vector3(2.1f, 2.1f, 0f);
    //        childGO.transform.localPosition = Vector3.zero;
    //        childGO.GetComponent<CharacterPortrait>().SetCitizen(marriedCoupleToShow.children[i]);
    //    }
    //}
    //#endregion

    #region Notifications Area
//    public void ShowNotification(Log log, HashSet<Kingdom> kingdomsThatShouldShowNotif, bool addLogToHistory = true) {
////        if (addLogToHistory) {
////            AddLogToLogHistory(log);
////        }
//        if (!kingdomsThatShouldShowNotif.Contains(currentlyShowingKingdom)) {
//            //currentlyShowingKingdom is not included in kingdomsThatShouldShowNotif, don't show notification
//            return;
//        }
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
    public void ShowNotification(Log log, bool addLogToHistory = true) {
//        if (addLogToHistory) {
//            AddLogToLogHistory(log);
//        }
        //if (kingdomsThatShouldShowNotif != null) {
        //    if (!kingdomsThatShouldShowNotif.Contains(currentlyShowingKingdom)) {
        //        //currentlyShowingKingdom is not included in kingdomsThatShouldShowNotif, don't show notification
        //        return;
        //    }
        //}
        if (notificationItemsThatCanBeReused.Count > 0) {
            NotificationItem itemToUse = notificationItemsThatCanBeReused[0];
            itemToUse.SetLog(log);
            RemoveNotificationItemFromReuseList(itemToUse);
            itemToUse.gameObject.SetActive(true);
        } else {
            GameObject notifGO = InstantiateUIObject(notificationPrefab.name, notificationParent.transform);
            notifGO.transform.localScale = Vector3.one;
            notifGO.GetComponent<NotificationItem>().SetLog(log);
        }
        //notificationParent.AddChild(notifGO.transform);
        RepositionNotificationTable();
        //notificationScrollView.UpdatePosition();
        //notificationParent.Reposition();
        notificationScrollView.UpdatePosition();

    }
    internal void AddNotificationItemToReuseList(NotificationItem item) {
        if (!notificationItemsThatCanBeReused.Contains(item)) {
            notificationItemsThatCanBeReused.Add(item);
        }
    }
    internal void RemoveNotificationItemFromReuseList(NotificationItem item) {
        notificationItemsThatCanBeReused.Remove(item);
    }
    public void RemoveAllNotifications() {
        List<Transform> children = notificationParent.GetChildList();
        for (int i = 0; i < children.Count; i++) {
            ObjectPoolManager.Instance.DestroyObject(children[i].gameObject);
        }
    }
    public void RepositionNotificationTable() {
        StartCoroutine(RepositionTable(notificationParent));
        //StartCoroutine(RepositionGrid(notificationParent));
        //StartCoroutine(RepositionScrollView(notificationParent.GetComponentInParent<UIScrollView>()));
    }
    #endregion

 //   #region World History
 //   internal void AddLogToLogHistory(Log log) {
 //       Messenger.Broadcast<Log>("AddLogToHistory", log);
 // //      //string logString = log.month.ToString() + " " + log.day.ToString() + ", " + log.year.ToString() + "         ";
 // //      //if (log.fillers.Count > 0) {
 // //      //    logString += Utilities.LogReplacer(log);
 // //      //} else {
 // //      //    logString += LocalizationManager.Instance.GetLocalizedValue(log.category, log.file, log.key);
 // //      //}
 // //      logHistory.Add(log);
 // //      if(logHistory.Count > 269) {
 // //          int numOfExcessLogs = logHistory.Count - 269;
 // //          for (int i = 0; i < numOfExcessLogs; i++) {
 // //              logHistory.Remove(logHistory.First());
 // //          }
 // //      }
 // //      //StartCoroutine(ConstructNotificationSummary());
 // //      if (notificationHistoryGO.activeSelf) {
 // //          if (logHistoryGO.activeSelf) {
 // //              ShowLogHistory();
 // //          }else if (warHistoryGO.activeSelf) {
 // //              ShowWarHistory();
 // //          }
            
 // //      }
	//	//if(notificationCityHistoryGO.activeSelf){
	//	//	if(this.currentlyShowingCity != null){
	//	//		ShowCityHistory (this.currentlyShowingCity);
	//	//	}
	//	//}
 //   }
 //   private void UpdateBattleLogs() {
 //       if (notificationHistoryGO.activeSelf) {
 //           if (warHistoryGO.activeSelf) {
 //               ShowWarHistory();
 //           }
 //       }
 //   }
 //   private void PopulateHistoryTable() {
 //       for (int i = 0; i < 100; i++) {
 //           GameObject logGO = InstantiateUIObject(logHistoryPrefab.name, notificationHistoryTable.transform);
 //           logGO.transform.localScale = Vector3.one;
 //       }
 //       StartCoroutine(RepositionTable(notificationHistoryTable));
 //   }
	//private void PopulateCityHistoryTable() {
	//	for (int i = 0; i < 100; i++) {
	//		GameObject logGO = InstantiateUIObject(logHistoryPrefab.name, notificationCityHistoryTable.transform);
	//		logGO.transform.localScale = Vector3.one;
	//	}
	//	StartCoroutine(RepositionTable(notificationCityHistoryTable));
	//}
 //   public void ToggleNotificationHistory() {
 //       worldHistoryUI.ToggleWorldHistoryUI();
 //       //if (notificationHistoryGO.activeSelf) {
 //       //    HideNotificationHistory();
 //       //} else {
 //       //    ShowLogHistory();
 //       //}
 //   }
 //   public void ToggleWarHistory() {
 //       if (notificationHistoryGO.activeSelf) {
 //           HideNotificationHistory();
 //       } else {
 //           ShowWarHistory();
 //       }
 //   }
	//public void ShowLogHistory() {
 //       WorldHistoryItem[] presentItems = Utilities.GetComponentsInDirectChildren<WorldHistoryItem>(notificationHistoryTable.gameObject);
 //       List<Log> logHistoryReversed = new List<Log>(logHistory);
 //       logHistoryReversed.Reverse();
	//	if(!isShowKingdomHistoryOnly){
	//		for (int i = 0; i < presentItems.Length; i++) {
	//			WorldHistoryItem currItem = presentItems[i];
	//			Log logToShow = logHistoryReversed.ElementAtOrDefault(i);
	//			if(logToShow == null || logToShow.key == "expand") {
	//				currItem.gameObject.SetActive(false);
	//				notificationHistoryTable.Reposition();
	//			} else {
	//				currItem.SetLog(logToShow);
	//				currItem.gameObject.SetActive(true);
	//				notificationHistoryTable.Reposition();
	//			}
	//		}
	//	}else{
	//		for (int i = 0; i < presentItems.Length; i++) {
	//			WorldHistoryItem currItem = presentItems[i];
	//			Log logToShow = logHistoryReversed.ElementAtOrDefault(i);
	//			if(logToShow == null || logToShow.key == "expand") {
	//				currItem.gameObject.SetActive(false);
	//				notificationHistoryTable.Reposition();
	//			} else {
	//				if(IsKingdomPartOfLog(logToShow, this.currentlyShowingKingdom)){
	//					currItem.SetLog(logToShow);
	//					currItem.gameObject.SetActive(true);
	//					notificationHistoryTable.Reposition();
	//				}else{
	//					currItem.gameObject.SetActive(false);
	//					notificationHistoryTable.Reposition();
	//				}
	//			}
	//		}
	//	}
      
 //       StartCoroutine(RepositionTable(notificationHistoryTable));

 //       if (notificationHistoryTable.GetChildList().Count <= 0) {
 //           notificationHistoryLbl.text = "[b][i]No History[/i][/b]";
 //           notificationHistoryLbl.gameObject.SetActive(true);
 //       } else {
 //           notificationHistoryLbl.gameObject.SetActive(false);
 //       }
 //       logHistoryGO.SetActive(true);
 //       warHistoryGO.SetActive(false);
 //       notificationHistoryGO.SetActive(true);
 //       kingdomListEventButton.SetClickState(true);
 //   }
	//public void ShowCityHistory(City city){
	//	this.currentlyShowingCity = city;
	//	WorldHistoryItem[] presentItems = Utilities.GetComponentsInDirectChildren<WorldHistoryItem>(notificationCityHistoryTable.gameObject);
	//	List<Log> logHistoryReversed = new List<Log>(logHistory);
	//	logHistoryReversed.Reverse();
	//	for (int i = 0; i < presentItems.Length; i++) {
	//		WorldHistoryItem currItem = presentItems[i];
	//		Log logToShow = logHistoryReversed.ElementAtOrDefault(i);
	//		if(logToShow == null) {
	//			currItem.gameObject.SetActive(false);
	//			notificationCityHistoryTable.Reposition();
	//		} else {
	//			if(city == null){
	//				city = this.currentlyShowingKingdom.cities [0];
	//			}
	//			if(IsCityPartOfLog(logToShow, city)){
	//				currItem.SetLog(logToShow);
	//				currItem.gameObject.SetActive(true);
	//				notificationCityHistoryTable.Reposition();
	//			}else{
	//				currItem.gameObject.SetActive(false);
	//				notificationCityHistoryTable.Reposition();
	//			}
	//		}
	//	}

	//	StartCoroutine(RepositionTable(notificationCityHistoryTable));

	//	if (notificationCityHistoryTable.GetChildList().Count <= 0) {
	//		notificationCityHistoryLbl.text = "[b][i]No History[/i][/b]";
	//		notificationCityHistoryLbl.gameObject.SetActive(true);
	//	} else {
	//		notificationCityHistoryLbl.gameObject.SetActive(false);
	//	}
	//	notificationHistoryGO.SetActive (false);
	//	notificationCityHistoryGO.SetActive(true);
	//}
 //   public void ShowWarHistory() {
 //       WarHistoryItem[] presentItems = Utilities.GetComponentsInDirectChildren<WarHistoryItem>(warHistoryTable.gameObject);
 //       List<Warfare> warsToShow = KingdomManager.Instance.allWarsThatOccured;
 //       int nextIndex = 0;
 //       for (int i = 0; i < presentItems.Length; i++) {
 //           WarHistoryItem currItem = presentItems[i];
 //           Warfare warToShow = warsToShow.ElementAtOrDefault(i);
 //           if (warToShow == null) {
 //               currItem.gameObject.SetActive(false);
 //           } else {
 //               currItem.SetWar(warToShow);
 //               currItem.gameObject.SetActive(true);
 //           }
 //           nextIndex = i + 1;
 //       }

 //       for (int i = nextIndex; i < warsToShow.Count; i++) {
 //           GameObject warGO = UIManager.Instance.InstantiateUIObject(UIManager.Instance.warHistoryPrefab.name, warHistoryTable.transform);
 //           warGO.transform.localScale = Vector3.one;
 //           warGO.GetComponent<WarHistoryItem>().SetWar(warsToShow[i]);
 //       }
 //       StartCoroutine(RepositionTable(warHistoryTable));

 //       warHistoryGO.SetActive(true);
 //       logHistoryGO.SetActive(false);
 //       notificationHistoryGO.SetActive(true);
 //       kingdomListEventButton.SetClickState(true);
 //   }
 //   internal void RepositionWarHistoryTable() {
 //       StartCoroutine(RepositionTable(warHistoryTable));
 //   }
 //   public void HideNotificationHistory() {
 //       notificationHistoryGO.SetActive(false);
 //       kingdomListEventButton.SetClickState(false);
 //   }
	//public void HideNotificationCityHistory() {
	//	notificationCityHistoryGO.SetActive(false);
	//}
	//private bool IsKingdomPartOfLog(Log log, Kingdom kingdom){
	//	if(log.fillers.Count > 0){
	//		for (int i = 0; i < log.fillers.Count; i++) {
	//			LogFiller filler = log.fillers [i];
	//			if(filler.obj is Kingdom){
	//				if(((Kingdom)filler.obj).id == kingdom.id){
	//					return true;
	//				}
	//			}
	//		}
	//	}
	//	if(log.allInvolved != null && log.allInvolved.Length > 0){
	//		for (int i = 0; i < log.allInvolved.Length; i++) {
	//			object obj = log.allInvolved [i];
	//			if(obj is Kingdom){
	//				if(((Kingdom)obj).id == kingdom.id){
	//					return true;
	//				}
	//			}
	//		}
	//	}
	//	return false;
	//}
	//private bool IsCityPartOfLog(Log log, City city){
	//	if(log.fillers.Count > 0){
	//		for (int i = 0; i < log.fillers.Count; i++) {
	//			LogFiller filler = log.fillers [i];
	//			if(filler.obj is City){
	//				if(((City)filler.obj).name == city.name){
	//					return true;
	//				}
	//			}
	//		}
	//	}
	//	if(log.allInvolved != null && log.allInvolved.Length > 0){
	//		for (int i = 0; i < log.allInvolved.Length; i++) {
	//			object obj = log.allInvolved [i];
	//			if(obj is City){
	//				if(((City)obj).name == city.name){
	//					return true;
	//				}
	//			}
	//		}
	//	}
	//	return false;
	//}
 //   #endregion

  //  #region Game Events
  //  public void ToggleEventsMenu() {
  //      eventsGo.SetActive(!eventsGo.activeSelf);
  //      if (!eventsGo.activeSelf) {
  //          eventsOfTypeGo.SetActive(false);
  //          //			EventManager.Instance.onHideEvents.Invoke();
  //          //			EventManager.Instance.onShowEventsOfType.Invoke (EVENT_TYPES.ALL);
  //          List<Transform> events = eventCategoriesGrid.GetChildList();
  //          for (int i = 0; i < events.Count; i++) {
  //              events[i].GetComponent<ButtonToggle>().SetClickState(false);
  //          }
  //      }

  //  }
  //  public void ShowPlayerEventsOfType(PlayerEvent playerEvent) {
  //      if (playerEvent.affectedKingdoms.Contains(this.currentlyShowingKingdom)) {
  //          if (this.currentlyShowingLogObject != null) {
  //              this.eventLogsQueue.Add(playerEvent);
  //          } else {
  //              //				Pause ();
  //              ShowEventLogs(playerEvent);
  //          }
  //      }
  //  }
  //  /*
	 //* Show Event Logs menu
	 //* */
  //  public void ShowEventLogs(object obj) {
  //      if (obj == null) {
  //          return;
  //      }
  //      if (this.eventLogsQueue.Contains(obj)) {
  //          this.eventLogsQueue.Remove(obj);
  //      }
  //      List<Log> logs = new List<Log>();
  //      if (obj is GameEvent) {
  //          GameEvent ge = ((GameEvent)obj);
  //          logs = ge.logs;
  //          elmEventTitleLbl.text = Utilities.LogReplacer(logs.FirstOrDefault());
  //          elmEventProgressBar.gameObject.SetActive(false);
  //          elmProgressBarLbl.gameObject.SetActive(false);
  //      } else if (obj is PlayerEvent) {
  //          PlayerEvent pe = ((PlayerEvent)obj);
  //          logs = pe.logs;
  //          elmEventTitleLbl.text = Utilities.LogReplacer(logs.FirstOrDefault());
  //          elmEventProgressBar.gameObject.SetActive(false);
  //          elmProgressBarLbl.gameObject.SetActive(false);
  //      }
  //      //elmProgressBarLbl.text = "Progress:";
  //      elmSuccessRateGO.SetActive(false);

  //      currentlyShowingLogObject = obj;

  //      GameObject nextAnchorPoint = elmFirstAnchor;
  //      List<EventLogItem> currentlyShowingLogs = elmEventLogsParentGO.GetComponentsInChildren<EventLogItem>(true).ToList();

  //      if ((logs.Count - 1) > currentlyShowingLogs.Count) {
  //          int logItemsToCreate = (logs.Count - 1) - currentlyShowingLogs.Count;
  //          for (int i = 0; i < logItemsToCreate; i++) {
  //              GameObject logGO = InstantiateUIObject(logItemPrefab.name, elmEventLogsParentGO.transform);
  //              logGO.transform.localScale = Vector3.one;
  //              currentlyShowingLogs.Add(logGO.GetComponent<EventLogItem>());
  //          }
  //      }

  //      for (int i = 0; i < currentlyShowingLogs.Count; i++) {
  //          EventLogItem currELI = currentlyShowingLogs[i];
  //          Log currLog = logs.ElementAtOrDefault(i + 1);
  //          if (currLog == null) {
  //              currELI.gameObject.SetActive(false);
  //          } else {
  //              currELI.SetLog(currLog);
  //              if ((i + 1) % 2 == 0) {
  //                  currELI.DisableBG();
  //              } else {
  //                  currELI.EnableBG();
  //              }
  //              currELI.gameObject.SetActive(true);
  //          }
  //      }

  //      StartCoroutine(RepositionTable(elmEventLogsParentGO.GetComponent<UITable>()));
  //      StartCoroutine(RepositionScrollView(elmScrollView));

  //      if (this.currentlyShowingLogObject is GameEvent) {
  //          if (((GameEvent)this.currentlyShowingLogObject).goEventItem != null) {
  //              ((GameEvent)this.currentlyShowingLogObject).goEventItem.GetComponent<EventItem>().DeactivateNewLogIndicator();
  //          }
  //      }

  //      if (!this.eventLogsGO.activeSelf) {
  //          this.eventLogsGO.SetActive(true);
  //      }
  //  }
  //  /*
	 //* Toggle Kingdom Events Menu
	 //* */
  //  public void ToggleKingdomEvents() {
  //      if (allKingdomEventsGO.activeSelf) {
  //          HideAllKingdomEvents();
  //      } else {
  //          //			Pause();
  //          ShowKingdomEvents();
  //      }
  //  }
  //  /*
	 //* Show Kingdom Events Menu
	 //* */
  //  public void ShowKingdomEvents() {
  //      //HideKingdomHistory();
  //      HideRelationships();
  //      HideKingdomCities();
  //      ShowKingdomCurrentEvents();

  //      allKingdomEventsGO.SetActive(true);
  //  }
  //  public void ShowKingdomCurrentEvents() {
  //      HideKingdomPastEvents();
  //      //		List<GameEvent> allActiveEventsInKingdom = EventManager.Instance.GetAllEventsKingdomIsInvolvedIn(currentlyShowingKingdom).Where(x => x.isActive).ToList();
  //      List<GameEvent> politicalEvents = currentlyShowingKingdom.activeEvents.Where(x => x.eventType != EVENT_TYPES.KINGDOM_WAR && x.eventType != EVENT_TYPES.INVASION_PLAN &&
  //      x.eventType != EVENT_TYPES.JOIN_WAR_REQUEST && x.eventType != EVENT_TYPES.REQUEST_PEACE && !Utilities.eventsNotToShow.Contains(x.eventType)).ToList();

  //      List<GameEvent> wars = currentlyShowingKingdom.activeEvents.Where(x => x.eventType == EVENT_TYPES.KINGDOM_WAR).ToList();

  //      if (politicalEvents.Count <= 0 && wars.Count <= 0) {
  //          kingdomNoCurrentEventsLbl.gameObject.SetActive(true);
  //          allKingdomEventsGO.SetActive(true);
  //          //			this.pauseBtn.SetAsClicked ();
  //          //			GameManager.Instance.SetPausedState (true);
  //          EventListParent[] currentParents = Utilities.GetComponentsInDirectChildren<EventListParent>(kingdomCurrentEventsContentParent.gameObject);
  //          if (currentParents.Length > 0) {
  //              for (int i = 0; i < currentParents.Length; i++) {
  //                  EventListParent currentParent = currentParents[i];
  //                  ObjectPoolManager.Instance.DestroyObject(currentParent.gameObject);
  //              }
  //          }
  //      } else {
  //          kingdomNoCurrentEventsLbl.gameObject.SetActive(false);
  //          LoadPoliticalEvents(politicalEvents);
  //          //            LoadWarEvents(wars);
  //      }

  //      kingdomCurrentEventsBtn.SetClickState(true);
  //      kingdomCurrentEventsGO.SetActive(true);
  //  }
  //  private void HideKingdomCurrentEvents() {
  //      kingdomCurrentEventsBtn.SetClickState(false);
  //      kingdomCurrentEventsGO.SetActive(false);
  //  }
  //  public void ShowKingdomPastEvents() {
  //      HideKingdomCurrentEvents();
  //      List<GameEvent> allDoneEvents = currentlyShowingKingdom.doneEvents.
  //          Where(x => !Utilities.eventsNotToShow.Contains(x.eventType)).ToList();
  //      allDoneEvents = allDoneEvents.OrderByDescending(x => x.startDate).ToList();

  //      List<EventListItem> currentItems = kingdomHistoryGrid.GetChildList().Select(x => x.GetComponent<EventListItem>()).ToList();
  //      int nextItem = 0;
  //      for (int i = 0; i < currentItems.Count; i++) {
  //          EventListItem currItem = currentItems[i];
  //          if (i < allDoneEvents.Count) {
  //              GameEvent gameEventToShow = allDoneEvents[i];
  //              if (gameEventToShow != null) {
  //                  currItem.SetEvent(gameEventToShow, currentlyShowingKingdom);
  //                  currItem.gameObject.SetActive(true);
  //                  nextItem = i + 1;
  //              } else {
  //                  currItem.gameObject.SetActive(false);
  //              }
  //          } else {
  //              currItem.gameObject.SetActive(false);
  //          }
  //      }

  //      for (int i = nextItem; i < allDoneEvents.Count; i++) {
  //          GameObject eventGO = InstantiateUIObject(kingdomEventsListItemPrefab.name, this.transform);
  //          eventGO.transform.localScale = Vector3.one;
  //          kingdomHistoryGrid.AddChild(eventGO.transform);
  //          eventGO.GetComponent<EventListItem>().SetEvent(allDoneEvents[i], currentlyShowingKingdom);
  //          eventGO.GetComponent<EventListItem>().onClickEvent += ShowEventLogs;
  //          kingdomHistoryGrid.Reposition();
  //      }

  //      if (allDoneEvents.Count <= 0) {
  //          kingdomHistoryNoEventsLbl.gameObject.SetActive(true);
  //      } else {
  //          kingdomHistoryNoEventsLbl.gameObject.SetActive(false);
  //      }
  //      kingdomHistoryBtn.SetClickState(true);
  //      kingdomHistoryGO.SetActive(true);
  //  }
  //  private void HideKingdomPastEvents() {
  //      kingdomHistoryBtn.SetClickState(false);
  //      kingdomHistoryGO.SetActive(false);
  //  }
  //  /*
	 //* Load all political events onto the kingdom events menu.
	 //* Political Events incl. [STATE VISIT, RAID, ASSASSINATION, DIPLOMATIC CRISIS, BORDER CONFLICT]
	 //* update this as needed!
	 //* */
  //  private void LoadPoliticalEvents(List<GameEvent> politicalEvents) {
  //      EventListParent politicsParent = null;
  //      List<Transform> allCurrentParents = Utilities.GetComponentsInDirectChildren<Transform>(kingdomCurrentEventsContentParent.gameObject).ToList();

  //      GameObject politicsParentGO = null;
  //      for (int i = 0; i < allCurrentParents.Count; i++) {
  //          if (allCurrentParents[i].name.Equals("PoliticsParent")) {
  //              politicsParentGO = allCurrentParents[i].gameObject;
  //              allCurrentParents.RemoveAt(i);
  //              break;
  //          }
  //      }

  //      if (politicalEvents.Count <= 0) {
  //          if (politicsParentGO == null) {
  //              ObjectPoolManager.Instance.DestroyObject(politicsParentGO);
  //          }
  //          return;
  //      }

  //      if (politicsParentGO == null) {
  //          //Instantiate Politics Parent
  //          politicsParentGO = InstantiateUIObject(kingdomEventsListParentPrefab.name, kingdomCurrentEventsContentParent.transform) as GameObject;
  //          politicsParentGO.name = "PoliticsParent";
  //          politicsParentGO.transform.localScale = Vector3.one;
  //          politicsParentGO.transform.localPosition = Vector3.zero;
  //          politicsParentGO.GetComponent<EventListParent>().eventTitleLbl.text = "Politics";
  //      }

  //      politicsParent = politicsParentGO.GetComponent<EventListParent>();

  //      List<EventListItem> currentlyShowingGameEvents = politicsParent.eventsGrid.GetChildList().Select(x => x.GetComponent<EventListItem>()).ToList();
  //      List<int> currentlyShowingGameEventIDs = currentlyShowingGameEvents.Select(x => x.gameEvent.id).ToList();
  //      List<int> actualPoliticalEventIDs = politicalEvents.Select(x => x.id).ToList();
  //      if (actualPoliticalEventIDs.Except(currentlyShowingGameEventIDs).Union(currentlyShowingGameEventIDs.Except(actualPoliticalEventIDs)).Any()) {
  //          for (int i = 0; i < currentlyShowingGameEvents.Count; i++) {
  //              politicsParent.eventsGrid.RemoveChild(currentlyShowingGameEvents[i].transform);
  //              ObjectPoolManager.Instance.DestroyObject(currentlyShowingGameEvents[i].gameObject);
  //          }

  //          //Instantiate all polical events into the politics parent grid
  //          for (int i = 0; i < politicalEvents.Count; i++) {
  //              GameObject eventGO = InstantiateUIObject(kingdomEventsListItemPrefab.name, this.transform);
  //              eventGO.transform.localScale = Vector3.one;
  //              politicsParent.eventsGrid.AddChild(eventGO.transform);
  //              eventGO.GetComponent<EventListItem>().SetEvent(politicalEvents[i], currentlyShowingKingdom);
  //              eventGO.GetComponent<EventListItem>().onClickEvent += ShowEventLogs;
  //          }
  //          StartCoroutine(RepositionGrid(politicsParent.eventsGrid));
  //      }
  //      RepositionKingdomEventsTable();
  //  }
  //  public void RepositionKingdomEventsTable() {
  //      StartCoroutine(RepositionTable(kingdomCurrentEventsContentParent));
  //  }
  //  /*
	 //* Hide Kingdom Events Menu
	 //* */
  //  public void HideAllKingdomEvents() {
  //      allKingdomEventsGO.SetActive(false);
  //      kingdomListEventButton.SetClickState(false);
  //  }
  //  #endregion

    //#region Cities Menu
    //public void ToggleKingdomCities() {
    //    if (kingdomCitiesGO.activeSelf) {
    //        HideKingdomCities();
    //    } else {
    //        ShowKingdomCities();
    //    }
    //}
    //internal void UpdateKingdomCitiesMenu() {
    //    if (kingdomCitiesGO.activeSelf) {
    //        if (currentlyShowingKingdom != null) {
    //            ShowKingdomCities();
    //        }
    //    }
    //}
    ///*
    // * Show all cities owned by currentlyShowingKingdom.
    // * */
    //public void ShowKingdomCities() {
    //    //HideKingdomHistory();
    //    HideRelationships();
    //    //HideAllKingdomEvents();
    //    kingdomCitiesGrid.cellHeight = 123f; //Disable if not using for testing
    //    List<CityItem> cityItems = kingdomCitiesGrid.gameObject.GetComponentsInChildren<Transform>(true)
    //        .Where(x => x.GetComponent<CityItem>() != null)
    //        .Select(x => x.GetComponent<CityItem>()).ToList();
    //    int nextIndex = 0;
    //    for (int i = 0; i < cityItems.Count; i++) {
    //        CityItem currCityItem = cityItems[i];
    //        if (i < currentlyShowingKingdom.cities.Count) {
    //            City currCity = currentlyShowingKingdom.cities.ElementAtOrDefault(i);
    //            if (currCity != null) {
    //                currCityItem.SetCity(currCity, true, false, true);
    //                currCityItem.gameObject.SetActive(true);
    //            } else {
    //                currCityItem.gameObject.SetActive(false);
    //            }
    //            nextIndex = i + 1;
    //        } else {
    //            currCityItem.gameObject.SetActive(false);
    //        }
    //    }

    //    if (currentlyShowingKingdom.cities.Count > nextIndex) {
    //        for (int i = nextIndex; i < currentlyShowingKingdom.cities.Count; i++) {
    //            City currCity = currentlyShowingKingdom.cities[i];
    //            GameObject cityGO = InstantiateUIObject(cityItemPrefab.name, this.transform);
    //            cityGO.GetComponent<CityItem>().SetCity(currCity, true, false, true);
    //            cityGO.transform.localScale = Vector3.one;
    //            kingdomCitiesGrid.AddChild(cityGO.transform);
    //            kingdomCitiesGrid.Reposition();
    //        }
    //        if (kingdomCitiesGO.activeSelf) {
    //            StartCoroutine(RepositionScrollView(kingdomCitiesScrollView, true));
    //        }
    //    }

    //    if (currentlyShowingKingdomCities == null || currentlyShowingKingdomCities.id != currentlyShowingKingdom.id || !kingdomCitiesGO.activeSelf) {
    //        StartCoroutine(RepositionGrid(kingdomCitiesGrid));
    //        StartCoroutine(RepositionScrollView(kingdomCitiesScrollView));
    //        kingdomCitiesScrollView.UpdateScrollbars();
    //    }

    //    kingdomCitiesGO.SetActive(true);
    //    currentlyShowingKingdomCities = currentlyShowingKingdom;
    //}
    //public void HideKingdomCities() {
    //    kingdomCitiesGO.SetActive(false);
    //    kingdomListCityButton.SetClickState(false);
    //}
    //#endregion

    //#region Intervene Menu
    //public void ToggleInterveneMenu() {
    //    if (interveneMenuGO.activeSelf) {
    //        HideInterveneMenu();
    //    } else {
    //        ShowInterveneMenu();
    //    }
    //}
    //private void ShowInterveneMenu() {
    //    if (interveneMenuGrid.GetChildList().Count <= 0) {
    //        LoadInterveneEvents();
    //    }
    //    interveneMenuBtn.SetClickState(true);
    //    interveneMenuGO.SetActive(true);
    //}
    //private void LoadInterveneEvents() {
    //    for (int i = 0; i < EventManager.Instance.playerPlacableEvents.Length; i++) {
    //        EVENT_TYPES currEvent = EventManager.Instance.playerPlacableEvents[i];
    //        GameObject playerEventItemGO = InstantiateUIObject(playerEventItemPrefab.name, this.transform);
    //        interveneMenuGrid.AddChild(playerEventItemGO.transform);
    //        playerEventItemGO.transform.localPosition = Vector3.zero;
    //        playerEventItemGO.transform.localScale = Vector3.one;
    //        playerEventItemGO.GetComponent<PlayerEventItem>().SetEvent(currEvent);
    //    }
    //    StartCoroutine(RepositionGrid(interveneMenuGrid));
    //    StartCoroutine(RepositionScrollView(interveneMenuScrollView));
    //}
    //private void HideInterveneMenu() {
    //    interveneMenuBtn.SetClickState(false);
    //    interveneMenuGO.SetActive(false);
    //}
    //public void ToggleInterveneActionsMenu() {
    //    if (interveneActonsGO.activeSelf) {
    //        HideInterveneActionsMenu();
    //    } else {
    //        ShowInterveneActionsMenu();
    //    }
    //}
    //private void ShowInterveneActionsMenu() {
    //    interveneActonsGO.SetActive(true);
    //}
    //public void HideInterveneActionsMenu() {
    //    HideSwitchKingdomsMenu();
    //    HideCreateKingdomMenu();
    //    HideChooseKingdomMenu();
    //    HideChooseCitizenMenu();
    //    interveneMenuBtn.SetClickState(false);
    //    interveneActonsGO.SetActive(false);
    //}
    //#endregion

    //#region Switch Kingdoms Menu
    //public void ToggleSwitchKingdomsMenu() {
    //    if (switchKingdomGO.activeSelf) {
    //        HideSwitchKingdomsMenu();
    //    } else {
    //        ShowSwitchKingdomsMenu();
    //    }
    //}
    //private void ShowSwitchKingdomsMenu() {
    //    HideCreateKingdomMenu();
    //    //Load Kingdoms
    //    List<Kingdom> kingdomsToList = KingdomManager.Instance.allKingdoms.Where(x => x.id != currentlyShowingKingdom.id).ToList();

    //    List<KingdomInterveneItem> presentKingdomItems = switchKingdomGrid.GetChildList().Select(x => x.GetComponent<KingdomInterveneItem>()).ToList();
    //    if (kingdomsToList.Count > presentKingdomItems.Count) {
    //        int numOfItemsToCreate = kingdomsToList.Count - presentKingdomItems.Count;
    //        for (int i = 0; i < numOfItemsToCreate; i++) {
    //            GameObject kingdomItemGO = InstantiateUIObject(kingdomIntervenePrefab.name, switchKingdomGrid.transform);
    //            switchKingdomGrid.AddChild(kingdomItemGO.transform);
    //            kingdomItemGO.transform.localScale = Vector3.one;
    //            presentKingdomItems.Add(kingdomItemGO.GetComponent<KingdomInterveneItem>());
    //        }
    //        StartCoroutine(RepositionGrid(switchKingdomGrid));
    //    }

    //    for (int i = 0; i < presentKingdomItems.Count; i++) {
    //        KingdomInterveneItem currItem = presentKingdomItems[i];
    //        Kingdom currKingdom = kingdomsToList.ElementAtOrDefault(i);
    //        if (currKingdom != null) {
    //            currItem.SetKingdom(currKingdom);
    //            currItem.gameObject.SetActive(true);
    //        } else {
    //            currItem.gameObject.SetActive(false);
    //        }
    //    }
    //    switchKingdomsBtn.SetClickState(true);
    //    switchKingdomGO.SetActive(true);
    //}
    //public void HideSwitchKingdomsMenu() {
    //    switchKingdomsBtn.SetClickState(false);
    //    switchKingdomGO.SetActive(false);
    //}
    //#endregion

    //#region Create Kingdom Menu
    //public void ToggleCreateKingdomMenu() {
    //    if (createKingdomGO.activeSelf) {
    //        HideCreateKingdomMenu();
    //    } else {
    //        ShowCreateKingdomMenu();
    //    }
    //}
    //private void ShowCreateKingdomMenu() {
    //    HideSwitchKingdomsMenu();
    //    createKingdomPopupList.Clear();
    //    RACE[] allRaces = Utilities.GetEnumValues<RACE>().Where(x => x != RACE.NONE).ToArray();
    //    for (int i = 0; i < allRaces.Length; i++) {
    //        createKingdomPopupList.AddItem(allRaces[i].ToString(), allRaces[i]);
    //    }
    //    createKingdomPopupList.value = createKingdomPopupList.items.First();
    //    createKingdomRaceSelectedLbl.text = createKingdomPopupList.value;
    //    UpdateCreateKingdomErrorMessage();

    //    createKingdomBtn.SetClickState(true);
    //    createKingdomGO.SetActive(true);
    //}
    //List<HexTile> tilesToChooseForNewKingdom;
    //public void UpdateCreateKingdomErrorMessage() {
    //    List<HexTile> elligibleTilesForNewKingdom = CityGenerator.Instance.GetHabitableTilesForRace((RACE)createKingdomPopupList.data);
    //    BIOMES forbiddenBiomeForRace = CityGenerator.Instance.GetForbiddenBiomeOfRace((RACE)createKingdomPopupList.data);
    //    elligibleTilesForNewKingdom = elligibleTilesForNewKingdom.Where(x => x.biomeType != forbiddenBiomeForRace && !x.isBorder && !x.isOccupied).ToList();

    //    tilesToChooseForNewKingdom = new List<HexTile>();

    //    for (int i = 0; i < elligibleTilesForNewKingdom.Count; i++) {
    //        HexTile currTile = elligibleTilesForNewKingdom[i];
    //        List<HexTile> invalidNeighbours = currTile.GetTilesInRange(3).Where(x => x.isBorder || x.isOccupied).ToList();
    //        if (invalidNeighbours.Count <= 0) {
    //            tilesToChooseForNewKingdom.Add(currTile);
    //        }
    //    }

    //    if (tilesToChooseForNewKingdom.Count <= 0) {
    //        createKingdomErrorLbl.text = "Cannot create kingdom for that race!";
    //        createKingdomErrorLbl.gameObject.SetActive(true);
    //        createKingdomExecuteBtn.GetComponent<BoxCollider>().enabled = false;
    //        createKingdomExecuteBtn.SetState(UIButtonColor.State.Disabled, true);
    //    } else {
    //        createKingdomErrorLbl.gameObject.SetActive(false);
    //        createKingdomExecuteBtn.GetComponent<BoxCollider>().enabled = true;
    //        createKingdomExecuteBtn.SetState(UIButtonColor.State.Normal, true);
    //    }
    //}
    //public void CreateNewKingdom() {
    //    List<HexTile> citiesForNewKingdom = new List<HexTile>() { tilesToChooseForNewKingdom[UnityEngine.Random.Range(0, tilesToChooseForNewKingdom.Count)] };
    //    Kingdom newKingdom = KingdomManager.Instance.GenerateNewKingdom((RACE)createKingdomPopupList.data, citiesForNewKingdom, true);
    //    newKingdom.HighlightAllOwnedTilesInKingdom();
    //    //newKingdom.king.CreateInitialRelationshipsToKings();
    //    //KingdomManager.Instance.AddRelationshipToOtherKings(newKingdom.king);

    //    HideInterveneActionsMenu();
    //}
    //private void HideCreateKingdomMenu() {
    //    createKingdomBtn.SetClickState(false);
    //    createKingdomGO.SetActive(false);
    //}
    //#endregion


    //#region Kingdoms Summary
    ////public void UpdateKingdomSummary() {
    ////    List<Kingdom> kingdomsToShow = new List<Kingdom>(KingdomManager.Instance.allKingdomsOrderedBy);
    ////    kingdomsToShow.Reverse();

    ////    for (int i = 0; i < kingdomSummaryEntries.Length; i++) {
    ////        KingdomSummaryEntry kse = kingdomSummaryEntries[i];
    ////        Kingdom currKingdom = kingdomsToShow.ElementAtOrDefault(i);
    ////        if (currKingdom == null) {
    ////            kse.gameObject.SetActive(false);
    ////        } else {
    ////            kse.SetKingdom(currKingdom);
    ////            kse.gameObject.SetActive(true);
    ////        }
    ////        kingdomSummaryGrid.Reposition();
    ////    }
    ////}
    //#endregion

 //   public void UpdateAllianceSummary() {
	//	if(UIManager.Instance.goAlliance.activeSelf){
	//		this.allianceSummaryLbl.text = string.Empty;
	//		AllianceWarLabel allianceWarLbl = this.allianceSummaryLbl.GetComponent<AllianceWarLabel> ();
	//		allianceWarLbl.kingdomsInLabel.Clear ();
	//		allianceWarLbl.citiesInLabel.Clear ();

	//		if (warAllianceState == "alliance") {
	//			for (int i = 0; i < KingdomManager.Instance.alliances.Count; i++) {
	//				AlliancePool alliance = KingdomManager.Instance.alliances [i];
	//				if (i != 0) {
	//					this.allianceSummaryLbl.text += "\n";
	//				}
	//				this.allianceSummaryLbl.text += alliance.name;
	//				for (int j = 0; j < alliance.kingdomsInvolved.Count; j++) {
	//					Kingdom kingdom = alliance.kingdomsInvolved [j];
	//					allianceWarLbl.AddKingdom (kingdom);
	//					this.allianceSummaryLbl.text += "\n- [url=" + kingdom.id.ToString() + "_kingdom" + "]" + kingdom.name + "[/url]";
	//				}
	//			}
 //               if(KingdomManager.Instance.allTradeDeals.Count > 0) {
 //                   this.allianceSummaryLbl.text += "\n Trade Deals: ";
 //                   for (int i = 0; i < KingdomManager.Instance.allTradeDeals.Count; i++) {
 //                       TradeDeal currDeal = KingdomManager.Instance.allTradeDeals[i];
	//					allianceWarLbl.AddKingdom (currDeal.kingdom1);
	//					allianceWarLbl.AddKingdom (currDeal.kingdom2);
 //                       this.allianceSummaryLbl.text += "\n- [url=" + currDeal.kingdom1.id.ToString() + "_kingdom" + "]" + currDeal.kingdom1.name + "[/url]" +
 //                           " -> " + "[url= " + currDeal.kingdom2.id.ToString() + "_kingdom" + "]" + currDeal.kingdom2.name + "[/url]";
 //                   }
 //               }
                
	//		} else if (warAllianceState == "warfare") {
	//			for (int i = 0; i < KingdomManager.Instance.kingdomWars.Count; i++) {
	//				Warfare warfare = KingdomManager.Instance.kingdomWars [i];
	//				if (i != 0) {
	//					this.allianceSummaryLbl.text += "\n";
	//				}
	//				this.allianceSummaryLbl.text += warfare.name;
	//				if(warfare.kingdomSideList[WAR_SIDE.A].Count > 0){
	//					this.allianceSummaryLbl.text += "\n- SIDE A: ";
	//					for (int j = 0; j < warfare.kingdomSideList[WAR_SIDE.A].Count; j++) {
	//						Kingdom kingdom = warfare.kingdomSideList [WAR_SIDE.A] [j];
	//						allianceWarLbl.AddKingdom (kingdom);
	//						this.allianceSummaryLbl.text += "[url=" + kingdom.id.ToString() + "_kingdom" + "]" + kingdom.name + "[/url](" + warfare.kingdomSideWeariness[kingdom.id].weariness.ToString() + ")";
	//						if(j < warfare.kingdomSideList[WAR_SIDE.A].Count - 1){
	//							this.allianceSummaryLbl.text += ", ";
	//						}
	//					}
	//				}
	//				if(warfare.kingdomSideList[WAR_SIDE.B].Count > 0){
	//					this.allianceSummaryLbl.text += "\n- SIDE B: ";
	//					for (int j = 0; j < warfare.kingdomSideList[WAR_SIDE.B].Count; j++) {
	//						Kingdom kingdom = warfare.kingdomSideList [WAR_SIDE.B] [j];
	//						allianceWarLbl.AddKingdom (kingdom);
	//						this.allianceSummaryLbl.text += "[url=" + kingdom.id.ToString() + "_kingdom" + "]" + kingdom.name + "[/url](" + warfare.kingdomSideWeariness[kingdom.id].weariness.ToString() + ")";
	//						if(j < warfare.kingdomSideList[WAR_SIDE.B].Count - 1){
	//							this.allianceSummaryLbl.text += ", ";
	//						}
	//					}
	//				}
	//				if(warfare.battles.Count > 0){
	//					this.allianceSummaryLbl.text += "\n- BATTLES:";
	//					for (int j = 0; j < warfare.battles.Count; j++) {
	//						if(warfare.battles[j].attackCity != null && warfare.battles[j].defenderCity != null){
								
	//							allianceWarLbl.AddCity (warfare.battles[j].attackCity);
	//							allianceWarLbl.AddCity (warfare.battles[j].defenderCity);

	//							this.allianceSummaryLbl.text += "\n-- [url=" + warfare.battles[j].attackCity.id.ToString() + "_city" + "]" + warfare.battles[j].attackCity.name + "[/url] -> [url=" + warfare.battles[j].defenderCity.id.ToString() + "_city" + "]" + warfare.battles[j].defenderCity.name + "[/url]";
	//							//("+ ((MONTH)warfare.battles[j].supposedAttackDate.month).ToString() + " " + warfare.battles[j].supposedAttackDate.day.ToString() + ", " + warfare.battles[j].supposedAttackDate.year.ToString() + ")"
	//						}
	//					}
	//				}
	//			}
	//			for (int i = 0; i < KingdomManager.Instance.internationalIncidents.Count; i++) {
	//				InternationalIncident internationalIncident = KingdomManager.Instance.internationalIncidents [i];
	//				allianceWarLbl.AddKingdom (internationalIncident.sourceKingdom);
	//				allianceWarLbl.AddKingdom (internationalIncident.targetKingdom);

	//				if(this.allianceSummaryLbl.text != string.Empty){
	//					this.allianceSummaryLbl.text += "\n";
	//				}
	//				this.allianceSummaryLbl.text += internationalIncident.incidentName;
	//				this.allianceSummaryLbl.text += "\n-" + "[url=" + internationalIncident.sourceKingdom.id.ToString() + "_kingdom" + "]" + internationalIncident.sourceKingdom.name + "[/url]";
	//				if(internationalIncident.isSourceKingdomAggrieved){
	//					this.allianceSummaryLbl.text += " (Aggrieved)";
	//				}
	//				this.allianceSummaryLbl.text += "\n-" + "[url=" + internationalIncident.targetKingdom.id.ToString() + "_kingdom" + "]" + internationalIncident.targetKingdom.name + "[/url]";
	//				if(internationalIncident.isTargetKingdomAggrieved){
	//					this.allianceSummaryLbl.text += " (Aggrieved)";
	//				}
	//			}
	//		}

	//	}
	//}

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
  //  /*
	 //* Get the corresponding icon for each event
	 //* */
  //  internal Sprite GetSpriteForEvent(EVENT_TYPES eventType) {
  //      switch (eventType) {
  //          case EVENT_TYPES.ASSASSINATION:
  //              return assassinationIcon;
  //          case EVENT_TYPES.BORDER_CONFLICT:
  //              return borderConflictIcon;
  //          case EVENT_TYPES.EXPANSION:
  //              return expansionIcon;
  //          case EVENT_TYPES.INVASION_PLAN:
  //              return invasionPlotIcon;
  //          case EVENT_TYPES.KINGDOM_WAR:
  //              return declareWarIcon;
  //          case EVENT_TYPES.MARRIAGE_INVITATION:
  //              return marriageInvitationIcon;
  //          case EVENT_TYPES.MILITARIZATION:
  //              return militarizationIcon;
  //          case EVENT_TYPES.RAID:
  //              return raidIcon;
  //          case EVENT_TYPES.REBELLION_PLOT:
  //              return rebellionPlotIcon;
  //          case EVENT_TYPES.REQUEST_PEACE:
  //              return requestPeaceIcon;
  //          case EVENT_TYPES.STATE_VISIT:
  //              return stateVisitIcon;
  //          case EVENT_TYPES.DIPLOMATIC_CRISIS:
  //              return rebellionPlotIcon;
  //          case EVENT_TYPES.ADMIRATION:
  //              return requestPeaceIcon;
  //          case EVENT_TYPES.RIOT_WEAPONS:
  //              return rebellionPlotIcon;
  //          case EVENT_TYPES.SECESSION:
  //              return rebellionPlotIcon;
  //          case EVENT_TYPES.REBELLION:
  //              return rebellionPlotIcon;
  //          case EVENT_TYPES.PLAGUE:
  //              return militarizationIcon;
  //          case EVENT_TYPES.BOON_OF_POWER:
  //              return marriageInvitationIcon;
  //          case EVENT_TYPES.TRADE:
  //              return marriageInvitationIcon;
  //          case EVENT_TYPES.PROVOCATION:
  //              return rebellionPlotIcon;
  //          case EVENT_TYPES.EVANGELISM:
  //              return requestPeaceIcon;
  //          case EVENT_TYPES.SPOUSE_ABDUCTION:
  //              return militarizationIcon;
  //          case EVENT_TYPES.FIRST_AND_KEYSTONE:
  //              return marriageInvitationIcon;
  //          case EVENT_TYPES.RUMOR:
  //              return militarizationIcon;
  //          case EVENT_TYPES.SLAVES_MERCHANT:
  //              return rebellionPlotIcon;
  //          case EVENT_TYPES.HIDDEN_HISTORY_BOOK:
  //              return requestPeaceIcon;
  //          case EVENT_TYPES.SERUM_OF_ALACRITY:
  //              return assassinationIcon;
  //          case EVENT_TYPES.ALTAR_OF_BLESSING:
  //              return requestPeaceIcon;
  //          case EVENT_TYPES.DEVELOP_WEAPONS:
  //              return assassinationIcon;
  //          case EVENT_TYPES.HYPNOTISM:
  //              return militarizationIcon;
  //          case EVENT_TYPES.KINGS_COUNCIL:
  //              return stateVisitIcon;
  //          case EVENT_TYPES.KINGDOM_HOLIDAY:
  //              return requestPeaceIcon;
  //          case EVENT_TYPES.ADVENTURE:
  //              return raidIcon;
  //          case EVENT_TYPES.EVIL_INTENT:
  //              return assassinationIcon;
  //          case EVENT_TYPES.GREAT_STORM:
  //              return assassinationIcon;
  //          case EVENT_TYPES.ANCIENT_RUIN:
  //              return marriageInvitationIcon;
  //      }
  //      return assassinationIcon;
  //  }
    /*
	 * Checker for if the mouse is currently
	 * over a UI Object
	 * */
    public bool IsMouseOnUI() {
        if (uiCamera != null) {
            if (Minimap.Instance.isDragging) {
                return true;
            }
            if (UICamera.hoveredObject != null && (UICamera.hoveredObject.layer == LayerMask.NameToLayer("UI") || UICamera.hoveredObject.layer == LayerMask.NameToLayer("PlayerActions"))) {
                return true;
            }
        }
        return false;
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
        UILabel[] goLbls = go.GetComponentsInChildren<UILabel>(true);
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
//    public void ToggleResourceIcons() {
//        CameraMove.Instance.ToggleResourceIcons();
//    }
//    public void ToggleGeneralCamera() {
//        CameraMove.Instance.ToggleGeneralCamera();
//    }
//    public void ToggleTraderCamera() {
//        CameraMove.Instance.ToggleTraderCamera();
//    }
//    public void OnValueChangeEventDropdown() {
//        eventDropdownCurrentSelectionLbl.text = this.eventDropdownList.value;
//        if (this.eventDropdownList.value == "Raid") {
//            goRaid.SetActive(true);
//            goStateVisit.SetActive(false);
//            goMarriageInvitation.SetActive(false);
//            goPowerGrab.SetActive(false);
//            goExpansion.SetActive(false);
//            goInvasionPlan.SetActive(false);
//        } else if (this.eventDropdownList.value == "State Visit") {
//            goRaid.SetActive(false);
//            goStateVisit.SetActive(true);
//            goMarriageInvitation.SetActive(false);
//            goPowerGrab.SetActive(false);
//            goExpansion.SetActive(false);
//            goInvasionPlan.SetActive(false);
//        } else if (this.eventDropdownList.value == "Marriage Invitation") {
//            goRaid.SetActive(false);
//            goStateVisit.SetActive(false);
//            goMarriageInvitation.SetActive(true);
//            goPowerGrab.SetActive(false);
//            goExpansion.SetActive(false);
//            goInvasionPlan.SetActive(false);
//        } else if (this.eventDropdownList.value == "Power Grab") {
//            goRaid.SetActive(false);
//            goStateVisit.SetActive(false);
//            goMarriageInvitation.SetActive(false);
//            goPowerGrab.SetActive(true);
//            goExpansion.SetActive(false);
//            goInvasionPlan.SetActive(false);
//        } else if (this.eventDropdownList.value == "Expansion") {
//            goRaid.SetActive(false);
//            goStateVisit.SetActive(false);
//            goMarriageInvitation.SetActive(false);
//            goPowerGrab.SetActive(false);
//            goExpansion.SetActive(true);
//            goInvasionPlan.SetActive(false);
//        } else if (this.eventDropdownList.value == "Invasion Plan") {
//            goRaid.SetActive(false);
//            goStateVisit.SetActive(false);
//            goMarriageInvitation.SetActive(false);
//            goPowerGrab.SetActive(false);
//            goExpansion.SetActive(false);
//            goInvasionPlan.SetActive(true);
//        }
//    }
//    public void ShowCreateEventUI() {
//        this.goCreateEventUI.SetActive(true);
//    }
//    public void HideCreateEventUI() {
//        this.goCreateEventUI.SetActive(false);
//    }
//    public void ToggleRelocate(){
//		if (this.relocateGO.activeSelf) {
//			this.relocateGO.SetActive (false);
//		} else {
//			this.citiesForRelocationPopupList.Clear ();
//			if(this.currentlyShowingCitizen != null){
//				for(int i = 0; i < this.currentlyShowingCitizen.city.kingdom.cities.Count; i++){
//					if(this.currentlyShowingCitizen.city.kingdom.cities[i].id != this.currentlyShowingCitizen.city.id){
//						this.citiesForRelocationPopupList.AddItem (this.currentlyShowingCitizen.city.kingdom.cities [i].name, this.currentlyShowingCitizen.city.kingdom.cities [i]);
//					}
//				}
//			}
//			this.relocateGO.SetActive (true);
//		}
//	}
//	//public void OnClickOkRelocation(){
////		if(this.citiesForRelocationPopupList.data != null){
////			City newCityForCitizen = (City)this.citiesForRelocationPopupList.data;
////			if(this.currentlyShowingCitizen != null){
//////				Debug.LogError (this.currentlyShowingCitizen.name + " HAS MOVED FROM " + this.currentlyShowingCitizen.city.name + " TO " + newCityForCitizen.name);
////				newCityForCitizen.MoveCitizenToThisCity (this.currentlyShowingCitizen, false);
////			}
////		}
//	//}
//	public void HideRelocate(){
//		this.relocateGO.SetActive (false);
//	}
//	public void ToggleForceWar(){
//		if (this.forceWarGO.activeSelf) {
//			this.forceWarGO.SetActive (false);
//		} else {
//			this.kingdomsForWar.Clear ();
//			if(this.currentlyShowingKingdom != null){
//				foreach (KingdomRelationship kr in this.currentlyShowingKingdom.relationships.Values) {
//					if(!kr.sharedRelationship.isAtWar && kr.sharedRelationship.isAdjacent){
//						this.kingdomsForWar.AddItem (kr.targetKingdom.name, kr.targetKingdom);
//					}
//				}
////				for(int i = 0; i < this.currentlyShowingCitizen.city.kingdom.relationships.Count; i++){
////					if(!this.currentlyShowingCitizen.city.kingdom.relationships.ElementAt(i).Value.isAtWar){
////						this.kingdomsForWar.AddItem (this.currentlyShowingCitizen.city.kingdom.relationships.ElementAt(i).Value.targetKingdom.name
////                            , this.currentlyShowingCitizen.city.kingdom.relationships.ElementAt(i).Value.targetKingdom);
////					}
////				}
//			}
//			this.forceWarGO.SetActive (true);
//		}
//	}
//	public void HideForceWar(){
//		this.forceWarGO.SetActive (false);
//	}
//	public void ForceWar(){
//		Kingdom targetKingdom = (Kingdom)this.kingdomsForWar.data;
//		if(targetKingdom != null && this.currentlyShowingKingdom != null){
//			Warfare warfare = new Warfare (this.currentlyShowingKingdom, targetKingdom);
//			this.currentlyShowingKingdom.checkedWarfareID.Add (warfare.id);
//			targetKingdom.checkedWarfareID.Add (warfare.id);
//		}
//	}
//	public void ToggleUnrest(){
//		if (this.unrestGO.activeSelf) {
//			this.unrestGO.SetActive (false);
//		} else {
//			if(this.currentlyShowingKingdom != null){
//				this.unrestInput.value = this.currentlyShowingKingdom.stability.ToString();
//				this.unrestGO.SetActive (true);
//			}
//		}
//	}
//	public void OnChangeUnrest(){
//		if(this.currentlyShowingKingdom != null){
//			this.currentlyShowingKingdom.ChangeStability(int.Parse(this.unrestInput.value));
//            UpdateKingdomInfo();
//		}
//	}
//	public void HideUnrest(){
//		this.unrestGO.SetActive (false);
//	}
//	public void OnClickBoonOfPower(){
//		ShowInterveneEvent (EVENT_TYPES.BOON_OF_POWER);
//	}
//	//private void ShowGovernorLoyalty(){
//	//	if(!this.goLoyalty.activeSelf){
//	//		this.goLoyalty.SetActive (true);
//	//	}
//	//}
//	//public void HideGovernorLoyalty(){
//	//	this.goLoyalty.SetActive (false);
//	//}
// //   public void ChangeGovernorLoyalty() {
//	//	((Governor)this.currentlyShowingCitizen.assignedRole).SetLoyalty(Int32.Parse(forTestingLoyaltyLbl.text));
//	//	Debug.Log("Changed loyalty of: " + this.currentlyShowingCitizen.name + " to " + ((Governor)this.currentlyShowingCitizen.assignedRole).loyalty.ToString());
// //   }
//    public void LogRelatives() {
//        List<Citizen> allRelatives = currentlyShowingCitizen.GetRelatives(-1);
//        Debug.Log("========== " + currentlyShowingCitizen.name + " Relatives ==========");
//        for (int i = 0; i < allRelatives.Count; i++) {
//            Debug.Log("Relative: " + allRelatives[i].name);
//        }
//    }
//    public void ChangeStability() {
//        currentlyShowingKingdom.ChangeStability(Int32.Parse(kingdomUnrestLbl.text));
//        kingdomUnrestLbl.text = currentlyShowingKingdom.stability.ToString();
//    }
//    public void ForceExpansion() {
//        EventCreator.Instance.CreateExpansionEvent(currentlyShowingKingdom);
//    }
//	public void ToggleAlliance() {
//		if(warAllianceState == "alliance"){
//			this.goAlliance.SetActive (!this.goAlliance.activeSelf);
//		}else{
//			this.goAlliance.SetActive (true);
//		}
//		if(this.goAlliance.activeSelf){
//			warAllianceState = "alliance";
//			UpdateAllianceSummary ();
//		}
//	}
//	public void ToggleWarfare() {
//		if(warAllianceState == "warfare"){
//			this.goAlliance.SetActive (!this.goAlliance.activeSelf);
//		}else{
//			this.goAlliance.SetActive (true);
//		}
//		if(this.goAlliance.activeSelf){
//			warAllianceState = "warfare";
//			UpdateAllianceSummary ();
//		}
//	}
//    public void ForceKillCurrentCitizen() {
//        currentlyShowingCitizen.Death(DEATH_REASONS.ACCIDENT);
//        ShowCitizenInfo(currentlyShowingCitizen);
//    }
//    internal AGENT_TYPE spawnType = AGENT_TYPE.NONE;
//    [SerializeField] internal ButtonToggle _spawnNecromancerBtn;
//    public void SpawnNecromancer() {
//        if (_spawnNecromancerBtn.isClicked) {
//            spawnType = AGENT_TYPE.NECROMANCER;
//        } else {
//            spawnType = AGENT_TYPE.NONE;
//        }
//    }
    #endregion

 //   public void CenterCameraOnCitizen(){
	//	if(this.currentlyShowingCitizen != null){
	//		if(!this.currentlyShowingCitizen.isDead){
	//			CameraMove.Instance.CenterCameraOn (this.currentlyShowingCitizen.currentLocation.gameObject);
	//		}
	//	}
	//}

	//#region Intervene Events
	//internal void ShowInterveneEvent(EVENT_TYPES eventType){
	//	WorldEventManager.Instance.currentInterveneEvent = eventType;
	//	switch(eventType){
	//	case EVENT_TYPES.PLAGUE:
	//		ShowIntervenePlagueEvent ();
	//		break;
	//	case EVENT_TYPES.BOON_OF_POWER:
	//		break;
 //       case EVENT_TYPES.LYCANTHROPY:
 //           //ToggleLycanthropyMenu();
 //           break;
 //       case EVENT_TYPES.EVIL_INTENT:
 //           //ToggleEvilIntentMenu();
 //           break;
	//	}
	//}

 //   #region Plague
 //   private void ShowIntervenePlagueEvent(){
	//	ToggleForcePlague ();
	//}
	//public void ToggleForcePlague(){
	//	if (this.forcePlagueGO.activeSelf) {
	//		this.forcePlagueGO.SetActive (false);
	//	} else {
	//		if(!WorldEventManager.Instance.HasEventOfType(EVENT_TYPES.PLAGUE)){
	//			this.kingdomsForPlague.Clear ();
	//			this.kingdomsForPlague.AddItem ("RANDOM", null);
	//			for(int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++){
	//				if(KingdomManager.Instance.allKingdoms[i].cities.FirstOrDefault(x => x.structures.Count > 0) != null){
	//					this.kingdomsForPlague.AddItem (KingdomManager.Instance.allKingdoms[i].name, KingdomManager.Instance.allKingdoms[i]);
	//				}
	//			}
	//			this.forcePlagueGO.SetActive (true);
	//		}

	//	}
	//}
	//public void OnClickForcePlague(){
	//	Kingdom targetKingdom = null;
	//	if(this.kingdomsForPlague.data != null){
	//		targetKingdom = (Kingdom)this.kingdomsForPlague.data;
	//	}else{
	//		List<Kingdom> filteredKingdoms = new List<Kingdom> ();
	//		for(int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++){
	//			if(KingdomManager.Instance.allKingdoms[i].cities.FirstOrDefault(x => x.structures.Count > 0) != null){
	//				filteredKingdoms.Add (KingdomManager.Instance.allKingdoms[i]);
	//			}
	//		}
	//		if(filteredKingdoms != null && filteredKingdoms.Count > 0){
	//			targetKingdom = filteredKingdoms [UnityEngine.Random.Range (0, filteredKingdoms.Count)];
	//		}
	//	}
	//	if(targetKingdom != null){
	//		//EventCreator.Instance.CreatePlagueEvent (targetKingdom);
	//	}
	//}
 //   #endregion

 //   //#region Lycanthropy
 //   //[Space(10)]
 //   //[Header("Lycanthropy Objects")]
 //   //[SerializeField] private GameObject lycanthropyMenuGO;
 //   //[SerializeField] private UIGrid lycanthropyMenuGrid;
 //   //[SerializeField] private GameObject lycanthropySelectedGO;
 //   //private Citizen lycanthropySelectedCitizen;

 //   //private void ToggleLycanthropyMenu() {
 //   //    if (lycanthropyMenuGO.activeSelf) {
 //   //        HideLycanthropyMenu();
 //   //    } else {
 //   //        ShowInterveneLycanthropyEvent();
 //   //    }
 //   //}
 //   //private void ShowInterveneLycanthropyEvent() {
 //   //    int numOfCitizensToChooseFrom = 5;
 //   //    List<Citizen> allGovernors = KingdomManager.Instance.GetAllCitizensOfType(ROLE.GOVERNOR);
 //   //    List<Citizen> allGovernorRelatives = new List<Citizen>();
 //   //    //Get relatives of all the governors
 //   //    for (int i = 0; i < allGovernors.Count; i++) {
 //   //        Governor currGovernor = (Governor)allGovernors[i].assignedRole;
 //   //        List<Citizen> currGovernorRelatives = allGovernors[i].GetRelatives().Where(x => !x.isDead && x.role == ROLE.UNTRAINED).ToList();
 //   //        allGovernorRelatives = allGovernorRelatives.Union(currGovernorRelatives).ToList();
 //   //    }

 //   //    //Randomly choose a number of relatives
 //   //    List<Citizen> citizensToChooseFrom = new List<Citizen>();
 //   //    if (allGovernorRelatives.Count > 0) {
 //   //        for (int i = 0; i < numOfCitizensToChooseFrom; i++) {
 //   //            if(allGovernorRelatives.Count <= 0) {
 //   //                break;
 //   //            }
 //   //            Citizen chosenCitizen = allGovernorRelatives[UnityEngine.Random.Range(0, allGovernorRelatives.Count)];
 //   //            citizensToChooseFrom.Add(chosenCitizen);
 //   //            allGovernorRelatives.Remove(chosenCitizen);
 //   //        }
 //   //    } else {
 //   //        Debug.LogError("No elligible citizens!");
 //   //        return;
 //   //    }
 //   //    List<CharacterPortrait> portraits = Utilities.GetComponentsInDirectChildren<CharacterPortrait>(lycanthropyMenuGrid.gameObject).ToList();

 //   //    List<CharacterPortrait> activePortraits = new List<CharacterPortrait>();
 //   //    //Display chosen citizens to player
 //   //    for (int i = 0; i < portraits.Count; i++) {
 //   //        CharacterPortrait currPortrait = portraits[i];
 //   //        Citizen currCitizen = null;
 //   //        try {
 //   //            currCitizen = citizensToChooseFrom[i];
 //   //        } catch {
 //   //            currPortrait.gameObject.SetActive(false);
 //   //            continue;
 //   //        }
 //   //        currPortrait.SetCitizen(currCitizen, false, true);
 //   //        currPortrait.onClickCharacterPortrait += SelectCitizenForLycanthropy;
 //   //        currPortrait.gameObject.SetActive(true);
 //   //        activePortraits.Add(currPortrait);
 //   //    }

 //   //    if (activePortraits.Count > 0) {
 //   //        SelectCitizenForLycanthropy(activePortraits.FirstOrDefault().citizen, activePortraits.FirstOrDefault());
 //   //    }
 //   //    lycanthropyMenuGO.SetActive(true);
 //   //}
 //   //private void SelectCitizenForLycanthropy(Citizen citizen, CharacterPortrait clickedPortrait) {
 //   //    lycanthropySelectedCitizen = citizen;
 //   //    ShowCitizenInfo(citizen);
 //   //    CharacterPortrait charPortraitOfCitizen = null;
 //   //    List<CharacterPortrait> portraits = Utilities.GetComponentsInDirectChildren<CharacterPortrait>(lycanthropyMenuGrid.gameObject).ToList();
 //   //    for (int i = 0; i < portraits.Count; i++) {
 //   //        CharacterPortrait currPortrait = portraits[i];
 //   //        if(currPortrait.citizen.id == citizen.id) {
 //   //            charPortraitOfCitizen = currPortrait;
 //   //            break;
 //   //        }
 //   //    }

 //   //    lycanthropySelectedGO.transform.SetParent(charPortraitOfCitizen.transform);
 //   //    lycanthropySelectedGO.transform.localPosition = Vector3.zero;
 //   //    lycanthropySelectedGO.SetActive(true);
 //   //}
 //   //public void StartLycanthropyEvent() {
 //   //    Lycanthropy newLycanthropy = new Lycanthropy(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, null, lycanthropySelectedCitizen);
 //   //    HideLycanthropyMenu();
 //   //    HideInterveneMenu();
 //   //    if(currentlyShowingCitizen != null) {
 //   //        ShowCitizenInfo(currentlyShowingCitizen);
 //   //    }
        
 //   //}
 //   //public void HideLycanthropyMenu() {
 //   //    lycanthropyMenuGO.SetActive(false);
 //   //}
 //   //#endregion

 //   //#region Evil Intent
 //   //[Space(10)]
 //   //[Header("Evil Intent Objects")]
 //   //[SerializeField] private GameObject evilIntentMenuGO;
 //   //[SerializeField] private GameObject evilIntentTargetKingParentGO;
 //   //[SerializeField] private UIGrid evilIntentMenuSourceKingGrid;
 //   //[SerializeField] private UIGrid evilIntentMenuTargetKingGrid;
 //   //[SerializeField] private GameObject evilIntentSelectedSourceKingGO;
 //   //[SerializeField] private GameObject evilIntentSelectedTargetKingGO;
 //   //[SerializeField] private UIButton startEvilIntentBtn;

 //   //private Citizen evilIntentSelectedSourceKing;
 //   //private Citizen evilIntentSelectedTargetKing;

 //   //private void ToggleEvilIntentMenu() {
 //   //    if (evilIntentMenuGO.activeSelf) {
 //   //        HideEvilIntentMenu();
 //   //    } else {
 //   //        ShowInterveneEvilIntentEvent();
 //   //    }
 //   //}
 //   //private void ShowInterveneEvilIntentEvent() {
 //   //    evilIntentSelectedSourceKing = null;
 //   //    evilIntentSelectedTargetKing = null;
 //   //    evilIntentTargetKingParentGO.SetActive(false);
 //   //    evilIntentSelectedSourceKingGO.SetActive(false);
 //   //    evilIntentSelectedTargetKingGO.SetActive(false);
 //   //    startEvilIntentBtn.GetComponent<BoxCollider>().enabled = false;

 //   //    List<Citizen> allKings = KingdomManager.Instance.allKingdoms.Select(x => x.king).ToList();
 //   //    List<CharacterPortrait> portraits = Utilities.GetComponentsInDirectChildren<CharacterPortrait>(evilIntentMenuSourceKingGrid.gameObject).ToList();

 //   //    if (allKings.Count > portraits.Count) {
 //   //        int numOfMissingPortraits = allKings.Count - portraits.Count;
 //   //        for (int i = 0; i < numOfMissingPortraits; i++) {
 //   //            GameObject newPortrait = InstantiateUIObject(characterPortraitPrefab.name, evilIntentMenuSourceKingGrid.transform) as GameObject;
 //   //            newPortrait.transform.localScale = Vector3.one;
 //   //            evilIntentMenuSourceKingGrid.AddChild(newPortrait.transform);
 //   //            StartCoroutine(RepositionGrid(evilIntentMenuSourceKingGrid));
 //   //            portraits.Add(newPortrait.GetComponent<CharacterPortrait>());
 //   //        }
 //   //    }

 //   //    for (int i = 0; i < portraits.Count; i++) {
 //   //        CharacterPortrait currPortrait = portraits[i];
 //   //        Citizen currKing;
 //   //        try {
 //   //            currKing = allKings[i];
 //   //            currPortrait.SetCitizen(currKing, false, true);
 //   //            currPortrait.onClickCharacterPortrait = null;
 //   //            currPortrait.onClickCharacterPortrait += SelectSourceKingForEvilIntent;
 //   //            currPortrait.gameObject.SetActive(true);
 //   //        } catch {
 //   //            currPortrait.gameObject.SetActive(false);
 //   //        }
 //   //    }

 //   //    evilIntentMenuGO.SetActive(true);
 //   //}
 //   //private void LoadEvilIntentTargetChoices() {
 //   //    List<Citizen> allOtherKings = evilIntentSelectedSourceKing.city.kingdom.discoveredKingdoms.Select(x => x.king).Where(x => x.id != evilIntentSelectedSourceKing.id).ToList();
 //   //    List<CharacterPortrait> portraits = Utilities.GetComponentsInDirectChildren<CharacterPortrait>(evilIntentMenuTargetKingGrid.gameObject).ToList();

 //   //    if (allOtherKings.Count > portraits.Count) {
 //   //        int numOfMissingPortraits = allOtherKings.Count - portraits.Count;
 //   //        for (int i = 0; i < numOfMissingPortraits; i++) {
 //   //            GameObject newPortrait = InstantiateUIObject(characterPortraitPrefab.name, evilIntentMenuTargetKingGrid.transform) as GameObject;
 //   //            newPortrait.transform.localScale = Vector3.one;
 //   //            evilIntentMenuTargetKingGrid.AddChild(newPortrait.transform);
 //   //            StartCoroutine(RepositionGrid(evilIntentMenuTargetKingGrid));
 //   //            portraits.Add(newPortrait.GetComponent<CharacterPortrait>());
 //   //        }
 //   //    }

 //   //    for (int i = 0; i < portraits.Count; i++) {
 //   //        CharacterPortrait currPortrait = portraits[i];
 //   //        Citizen currKing;
 //   //        try {
 //   //            currKing = allOtherKings[i];
 //   //            currPortrait.SetCitizen(currKing, false, true);
 //   //            currPortrait.onClickCharacterPortrait = null;
 //   //            currPortrait.onClickCharacterPortrait += SelectTargetKingForEvilIntent;
 //   //            currPortrait.gameObject.SetActive(true);
 //   //        } catch {
 //   //            currPortrait.gameObject.SetActive(false);
 //   //        }
 //   //    }

 //   //    evilIntentTargetKingParentGO.SetActive(true);
 //   //}
 //   //private void SelectSourceKingForEvilIntent(Citizen citizen, CharacterPortrait portraitClicked) {
 //   //    evilIntentSelectedTargetKing = null;
 //   //    evilIntentSelectedTargetKingGO.SetActive(false);
 //   //    startEvilIntentBtn.GetComponent<BoxCollider>().enabled = false;

 //   //    evilIntentSelectedSourceKing = citizen;
 //   //    ShowCitizenInfo(citizen);
 //   //    CharacterPortrait charPortraitOfCitizen = null;
 //   //    List<CharacterPortrait> portraits = Utilities.GetComponentsInDirectChildren<CharacterPortrait>(evilIntentMenuSourceKingGrid.gameObject).ToList();
 //   //    for (int i = 0; i < portraits.Count; i++) {
 //   //        CharacterPortrait currPortrait = portraits[i];
 //   //        if (currPortrait.citizen.id == citizen.id) {
 //   //            charPortraitOfCitizen = currPortrait;
 //   //            break;
 //   //        }
 //   //    }

 //   //    evilIntentSelectedSourceKingGO.transform.SetParent(charPortraitOfCitizen.transform);
 //   //    evilIntentSelectedSourceKingGO.transform.localPosition = Vector3.zero;
 //   //    evilIntentSelectedSourceKingGO.SetActive(true);
 //   //    LoadEvilIntentTargetChoices();
 //   //}
 //   //private void SelectTargetKingForEvilIntent(Citizen citizen, CharacterPortrait clickedPortrait) {
 //   //    startEvilIntentBtn.GetComponent<BoxCollider>().enabled = true;
 //   //    startEvilIntentBtn.SetState(UIButtonColor.State.Normal, true);

 //   //    evilIntentSelectedTargetKing = citizen;
 //   //    ShowCitizenInfo(citizen);
 //   //    CharacterPortrait charPortraitOfCitizen = null;
 //   //    List<CharacterPortrait> portraits = Utilities.GetComponentsInDirectChildren<CharacterPortrait>(evilIntentMenuTargetKingGrid.gameObject).ToList();
 //   //    for (int i = 0; i < portraits.Count; i++) {
 //   //        CharacterPortrait currPortrait = portraits[i];
 //   //        if (currPortrait.citizen.id == citizen.id) {
 //   //            charPortraitOfCitizen = currPortrait;
 //   //            break;
 //   //        }
 //   //    }

 //   //    evilIntentSelectedTargetKingGO.transform.SetParent(charPortraitOfCitizen.transform);
 //   //    evilIntentSelectedTargetKingGO.transform.localPosition = Vector3.zero;
 //   //    evilIntentSelectedTargetKingGO.SetActive(true);

 //   //    evilIntentTargetKingParentGO.SetActive(true);
 //   //}
 //   //public void StartEvilIntentEvent() {
 //   //    EvilIntent newEvilIntent = new EvilIntent(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, null, evilIntentSelectedSourceKing, evilIntentSelectedTargetKing);
 //   //    HideEvilIntentMenu();
 //   //    HideInterveneMenu();
 //   //    if (currentlyShowingCitizen != null) {
 //   //        ShowCitizenInfo(currentlyShowingCitizen);
 //   //    }
 //   //}
 //   //public void HideEvilIntentMenu() {
 //   //    evilIntentMenuGO.SetActive(false);
 //   //}
 //   //#endregion

 //   #region Incurable Disease
 //   [Space(10)]
 //   [Header("Incurable Disease")]
 //   [SerializeField] private ButtonToggle incurableDiseaseBtn;
 //   public void ToggleChooseCitizensForIncurableDisease() {
 //       if (chooseCitizenGO.activeSelf) {
 //           HideChooseCitizenMenu();
 //       } else {
 //           List<Citizen> kingsToChooseFrom = new List<Citizen>(KingdomManager.Instance.GetAllKings().Where(x => !x.statusEffects.ContainsKey(CITIZEN_STATUS_EFFECTS.INCURABLE_DISEASE)));
 //           ShowChooseCitizenMenu(kingsToChooseFrom, InfectChosenCitizenWithIncurableDisease, "Choose a Citizen to Infect");
 //           onHideChooseCitizenMenu += incurableDiseaseBtn.SetAsUnClicked;
 //           //select default
 //           if (chooseCitizenGrid.GetChildList().Count > 0) {
 //               CharacterPortrait defaultPortrait = chooseCitizenGrid.GetChild(0).GetComponent<CharacterPortrait>();
 //               ChooseCitizen(defaultPortrait.citizen, defaultPortrait);
 //           } else {
 //               EnableUIButton(chooseCitizenOkBtn, false);
 //               chooseCitizenSelectedGO.SetActive(false);
 //           }
 //           HideSmallInfo();
 //       }
 //   }
 //   private void InfectChosenCitizenWithIncurableDisease() {
 //       currentChosenCitizen.AddStatusEffect(CITIZEN_STATUS_EFFECTS.INCURABLE_DISEASE);
 //       HideChooseCitizenMenu();
 //       //UpdateChooseCitizensMenuForIncurableDisease();
 //   }
 //   internal void UpdateChooseCitizensMenuForIncurableDisease() {
 //       if(chooseCitizenGO.activeSelf && incurableDiseaseBtn.isClicked) {
 //           List<Citizen> kingsToChooseFrom = new List<Citizen>(KingdomManager.Instance.GetAllKings().Where(x => !x.statusEffects.ContainsKey(CITIZEN_STATUS_EFFECTS.INCURABLE_DISEASE)));
 //           ShowChooseCitizenMenu(kingsToChooseFrom, InfectChosenCitizenWithIncurableDisease, "Choose a Citizen to Infect");
 //       }
 //   }
 //   #endregion

 //   #region Rebellion
 //   [Space(10)]
 //   [Header("Rebellion")]
 //   [SerializeField] private ButtonToggle rebellionBtn;
 //   public void ToggleChooseKingdomForRebellion() {
 //       if (chooseKingdomGO.activeSelf) {
 //           HideChooseKingdomMenu();
 //       } else {
 //           List<Kingdom> kingdomsToChooseFrom = new List<Kingdom>(KingdomManager.Instance.allKingdoms.Where(x => x.IsElligibleForRebellion()));
 //           ShowChooseKingdomMenu(kingdomsToChooseFrom, ShowChooseCitizensForRebellion, "Choose a Kingdom");
 //           onHideChooseKingdomMenu += rebellionBtn.SetAsUnClicked;
 //           //select default
 //           if (chooseKingdomGrid.GetChildList().Count > 0) {
 //               KingdomFlagItem defaultFlag = chooseKingdomGrid.GetChild(0).GetComponent<KingdomFlagItem>();
 //               ChooseKingdom(defaultFlag.kingdom, defaultFlag);
 //           } else {
 //               EnableUIButton(chooseKingdomOkBtn, false);
 //               chooseKingdomSelectedGO.SetActive(false);
 //           }

 //       }
 //   }
 //   private void ShowChooseCitizensForRebellion() {
 //       Kingdom chosenKingdom = currentChosenKingdom;
 //       HideChooseKingdomMenu();
 //       List<Citizen> citizensToChooseFrom = chosenKingdom.GetCitizensForRebellion();
 //       ShowChooseCitizenMenu(citizensToChooseFrom, StartRebellion, "Choose a Citizen to start the rebellion");
 //       onHideChooseCitizenMenu += rebellionBtn.SetAsUnClicked;
 //       EventDelegate.Set(rebellionBtn.GetComponent<UIEventTrigger>().onClick, delegate () { HideChooseCitizenMenu(); });

 //       //select default
 //       if (chooseCitizenGrid.GetChildList().Count > 0) {
 //           CharacterPortrait defaultPortrait = chooseCitizenGrid.GetChild(0).GetComponent<CharacterPortrait>();
 //           ChooseCitizen(defaultPortrait.citizen, defaultPortrait);
 //       }
 //   }
 //   private void StartRebellion() {
 //       Debug.Log("Start rebellion of " + currentChosenCitizen.name + " against " + currentChosenCitizen.city.kingdom.name);
 //       currentChosenCitizen.StartRebellion();
 //       HideChooseCitizenMenu();
 //   }
 //   #endregion

 //   #region Choose Citizen Menu
 //   private delegate void OnHideChooseCitizenMenu();
 //   private OnHideChooseCitizenMenu onHideChooseCitizenMenu;
 //   private Citizen currentChosenCitizen;
 //   private CharacterPortrait chosenPortrait;
 //   private void ShowChooseCitizenMenu(List<Citizen> citizensToChooseFrom, EventDelegate.Callback OnClick, string instructions = "Choose a citizen") {
 //       chooseCitizenInstructionLbl.text = instructions;
 //       if (citizensToChooseFrom.Count <= 0) {
 //           chooseCitizenNoChoicesGO.SetActive(true);
 //       } else {
 //           chooseCitizenNoChoicesGO.SetActive(false);
 //       }
 //       CharacterPortrait[] presentPortraits = Utilities.GetComponentsInDirectChildren<CharacterPortrait>(chooseCitizenGrid.gameObject);
 //       int nextIndex = 0;
 //       for (int i = 0; i < presentPortraits.Length; i++) {
 //           CharacterPortrait currPortrait = presentPortraits[i];
 //           Citizen citizenToShow = citizensToChooseFrom.ElementAtOrDefault(i);
 //           if(citizenToShow == null) {
 //               currPortrait.gameObject.SetActive(false);
 //           } else {
 //               currPortrait.SetCitizen(citizenToShow, false, false, true);
 //               currPortrait.onClickCharacterPortrait = null;
 //               currPortrait.onClickCharacterPortrait += ChooseCitizen;
 //               currPortrait.gameObject.SetActive(true);
 //           }
 //           nextIndex = i + 1;
 //       }
 //       for (int i = nextIndex; i < citizensToChooseFrom.Count; i++) {
 //           Citizen citizenToShow = citizensToChooseFrom[i];
 //           GameObject portraitGO = InstantiateUIObject(characterPortraitPrefab.name, this.transform);
 //           CharacterPortrait portrait = portraitGO.GetComponent<CharacterPortrait>();
 //           portrait.SetCitizen(citizenToShow, false, false, true);
 //           portrait.onClickCharacterPortrait = null;
 //           portrait.onClickCharacterPortrait += ChooseCitizen;
 //           portraitGO.transform.localScale = Vector3.one;
 //           chooseCitizenGrid.AddChild(portraitGO.transform);
 //           chooseCitizenGrid.Reposition();
 //       }
 //       StartCoroutine(RepositionGrid(chooseCitizenGrid));
 //       chooseCitizenOkBtn.onClick.Clear();
 //       EventDelegate.Set(chooseCitizenOkBtn.onClick, delegate () { OnClick(); });
 //       chooseCitizenGO.SetActive(true);
 //   }
 //   public void HideChooseCitizenMenu() {
 //       currentChosenCitizen = null;
 //       chosenPortrait = null;
 //       chooseCitizenGO.SetActive(false);
 //       chooseCitizenOkBtn.onClick.Clear();
 //       if(onHideChooseCitizenMenu != null) {
 //           onHideChooseCitizenMenu();
 //       }
 //       onHideChooseCitizenMenu = null;
 //       EventDelegate.Set(rebellionBtn.GetComponent<UIEventTrigger>().onClick, delegate () { ToggleChooseKingdomForRebellion(); });
 //   }
 //   private void ChooseCitizen(Citizen citizen, CharacterPortrait clickedPortrait) {
 //       chooseCitizenSelectedGO.transform.SetParent(clickedPortrait.transform);
 //       chooseCitizenSelectedGO.transform.localPosition = Vector3.zero;
 //       //chooseCitizenSelectedGO.transform.SetParent(chooseCitizenGO.transform);
 //       chooseCitizenSelectedGO.SetActive(true);
 //       currentChosenCitizen = citizen;
 //       chosenPortrait = clickedPortrait;
 //       EnableUIButton(chooseCitizenOkBtn, true);
 //   }
 //   #endregion

 //   #region Choose Kingdom Menu
 //   private delegate void OnHideChooseKingdomMenu();
 //   private OnHideChooseKingdomMenu onHideChooseKingdomMenu;
 //   private Kingdom currentChosenKingdom;
 //   private KingdomFlagItem chosenKingdomFlag;
 //   private void ShowChooseKingdomMenu(List<Kingdom> kingdomsToChooseFrom, EventDelegate.Callback OnClick, string instructions = "Choose a kingdom") {
 //       chooseKingdomInstructionLbl.text = instructions;
 //       if(kingdomsToChooseFrom.Count <= 0) {
 //           chooseKingdomNoChoicesGO.SetActive(true);
 //       } else {
 //           chooseKingdomNoChoicesGO.SetActive(false);
 //       }
 //       KingdomFlagItem[] presentFlags = Utilities.GetComponentsInDirectChildren<KingdomFlagItem>(chooseKingdomGrid.gameObject);
 //       int nextIndex = 0;
 //       for (int i = 0; i < presentFlags.Length; i++) {
 //           KingdomFlagItem currFlag = presentFlags[i];
 //           Kingdom kingdomToShow = kingdomsToChooseFrom.ElementAtOrDefault(i);
 //           if (kingdomToShow == null) {
 //               currFlag.gameObject.SetActive(false);
 //           } else {
 //               currFlag.SetKingdom(kingdomToShow, true);
 //               currFlag.onClickKingdomFlag = null;
 //               currFlag.onClickKingdomFlag += ChooseKingdom;
 //               currFlag.gameObject.SetActive(true);
 //           }
 //           nextIndex = i + 1;
 //       }
 //       for (int i = nextIndex; i < kingdomsToChooseFrom.Count; i++) {
 //           Kingdom kingdomToShow = kingdomsToChooseFrom[i];
 //           GameObject flagGO = InstantiateUIObject(kingdomFlagPrefab.name, this.transform);
 //           KingdomFlagItem flag = flagGO.GetComponent<KingdomFlagItem>();
 //           flag.SetKingdom(kingdomToShow, transform);
 //           flag.onClickKingdomFlag = null;
 //           flag.onClickKingdomFlag += ChooseKingdom;
 //           flagGO.transform.localScale = Vector3.one;
 //           chooseKingdomGrid.AddChild(flagGO.transform);
 //           chooseKingdomGrid.Reposition();
 //       }
 //       StartCoroutine(RepositionGrid(chooseKingdomGrid));
 //       chooseKingdomOkBtn.onClick.Clear();
 //       EventDelegate.Set(chooseKingdomOkBtn.onClick, delegate () { OnClick(); });
 //       chooseKingdomGO.SetActive(true);
 //   }
 //   public void HideChooseKingdomMenu() {
 //       currentChosenKingdom = null;
 //       chosenKingdomFlag = null;
 //       chooseKingdomGO.SetActive(false);
 //       chooseKingdomOkBtn.onClick.Clear();
 //       if (onHideChooseKingdomMenu != null) {
 //           onHideChooseKingdomMenu();
 //       }
 //       onHideChooseKingdomMenu = null;
 //   }
 //   private void ChooseKingdom(Kingdom kingdom, KingdomFlagItem clickedFlag) {
 //       chooseKingdomSelectedGO.transform.SetParent(clickedFlag.transform);
 //       chooseKingdomSelectedGO.transform.localPosition = new Vector3(89f, 0f, 0f);
 //       //chooseCitizenSelectedGO.transform.SetParent(chooseCitizenGO.transform);
 //       chooseKingdomSelectedGO.SetActive(true);
 //       currentChosenKingdom = kingdom;
 //       chosenKingdomFlag = clickedFlag;
 //       EnableUIButton(chooseKingdomOkBtn, true);
 //   }
 //   #endregion
 //   #endregion

	//public void ToggleKingdomHistory(){
	//	this.isShowKingdomHistoryOnly = !this.isShowKingdomHistoryOnly;
	//	ShowLogHistory ();
	//}

    #region World Info Menu
    //public void ShowKingdomsSummary() {
    //    HideAllianceSummary();
    //    HideWarSummary();
    //    //UpdateKingdomSummary();
    //    kingdomSummaryGO.SetActive(true);
    //    //SetWorldInfoMenuItemAsSelected(worldInfoKingdomBtn.transform);
    //}
    //public void HideKingdomSummary() {
    //    kingdomSummaryGO.SetActive(false);
    //}

    //public void ShowAllianceSummary() {
    //    //HideKingdomSummary();
    //    HideFactionsSummary();
    //    HideWarSummary();
    //    warAllianceState = "alliance";
    //    goAlliance.SetActive(true);
    //    UpdateAllianceSummary();
    //    SetWorldInfoMenuItemAsSelected(worldInfoAllianceBtn.transform);
    //}
    //public void HideAllianceSummary() {
    //    goAlliance.SetActive(false);
    //}

    //public void ShowWarSummary() {
    //    //HideKingdomSummary();
    //    HideFactionsSummary();
    //    HideAllianceSummary();
    //    warAllianceState = "warfare";
    //    goAlliance.SetActive(true);
    //    UpdateAllianceSummary();
    //    SetWorldInfoMenuItemAsSelected(worldInfoWarsBtn.transform);
    //}
    //public void HideWarSummary() {
    //    goAlliance.SetActive(false);
    //}
    private void SetWorldInfoMenuItemAsSelected(Transform worldInfoMenuItem) {
        worldInfoSelectedGO.transform.parent = worldInfoMenuItem;
        worldInfoSelectedGO.transform.localPosition = Vector3.zero;
    }
    #endregion

    #region Settlement Info
    [Space(10)]
    [Header("Settlement Info")]
    [SerializeField] internal LandmarkInfoUI settlementInfoUI;
    public void ShowLandmarkInfo(BaseLandmark landmark) {
		if(factionInfoUI.isShowing){
			factionInfoUI.HideMenu ();
		}
		if(characterInfoUI.isShowing){
			characterInfoUI.HideMenu ();
		}
		if(hexTileInfoUI.isShowing){
			hexTileInfoUI.HideMenu ();
		}
        //if (questInfoUI.isShowing) {
        //    questInfoUI.HideMenu();
        //}
		if(partyinfoUI.isShowing){
			partyinfoUI.HideMenu ();
		}
        settlementInfoUI.SetData(landmark);
        settlementInfoUI.OpenMenu();
        landmark.CenterOnLandmark();
//		playerActionsUI.ShowPlayerActionsUI ();
    }
    public void UpdateLandmarkInfo() {
		if(settlementInfoUI.isShowing){
			settlementInfoUI.UpdateLandmarkInfo();
		}
    }
    #endregion

    #region Faction Info
    [Space(10)]
    [Header("Faction Info")]
    [SerializeField] internal FactionInfoUI factionInfoUI;
	public void ShowFactionInfo(Faction faction) {
		if(settlementInfoUI.isShowing){
			settlementInfoUI.HideMenu ();
		}
		if(characterInfoUI.isShowing){
			characterInfoUI.HideMenu ();
		}
		if(hexTileInfoUI.isShowing){
			hexTileInfoUI.HideMenu ();
		}
        //if (questInfoUI.isShowing) {
        //    questInfoUI.HideMenu();
        //}
		if(partyinfoUI.isShowing){
			partyinfoUI.HideMenu ();
		}
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

    #region Character Info
    [Space(10)]
    [Header("Character Info")]
    [SerializeField] internal CharacterInfoUI characterInfoUI;
	public void ShowCharacterInfo(ECS.Character character) {
		if(settlementInfoUI.isShowing){
			settlementInfoUI.HideMenu ();
		}
		if(factionInfoUI.isShowing){
			factionInfoUI.HideMenu ();
		}
		if(hexTileInfoUI.isShowing){
			hexTileInfoUI.HideMenu ();
		}
        //if (questInfoUI.isShowing) {
        //    questInfoUI.HideMenu();
        //}
		if(partyinfoUI.isShowing){
			partyinfoUI.HideMenu ();
		}
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
		if(settlementInfoUI.isShowing){
			settlementInfoUI.HideMenu ();
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
		if(partyinfoUI.isShowing){
			partyinfoUI.HideMenu ();
		}
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
		if(settlementInfoUI.isShowing){
			settlementInfoUI.HideMenu ();
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
        SetWorldInfoMenuItemAsSelected(worldInfoQuestsBtn.transform);
		questsSummaryGO.SetActive(true);
		UpdateQuestsSummary();
    }
    public void HideQuestsSummary() {
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
        SetWorldInfoMenuItemAsSelected(worldInfoStorylinesBtn.transform);
        storylinesSummaryMenu.ShowMenu();
		StartCoroutine(RepositionTable (storylinesSummaryMenu.storyTable));
        //UpdateQuestsSummary();
    }
    public void HideStorylinesSummary() {
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
        SetWorldInfoMenuItemAsSelected(worldInfoCharactersBtn.transform);
        charactersSummaryMenu.OpenMenu();
    }
    public void HideCharactersSummary() {
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
}
