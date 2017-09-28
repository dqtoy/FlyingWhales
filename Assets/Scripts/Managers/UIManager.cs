using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class UIManager : MonoBehaviour {

	public static UIManager Instance = null;

	public UICamera uiCamera;

	[Space(10)]
    [Header("Prefabs")]
    public GameObject characterPortraitPrefab;
	public GameObject historyPortraitPrefab;
	public GameObject eventPortraitPrefab;
	//public GameObject traitPrefab;
	public GameObject gameEventPrefab;
	public GameObject kingdomEventsListParentPrefab;
	public GameObject kingdomWarEventsListParentPrefab;
	public GameObject kingdomEventsListItemPrefab;
	//public GameObject kingdomWarEventsListItemPrefab;
	//public GameObject kingdomFlagPrefab;
	public GameObject logItemPrefab;
    public GameObject cityItemPrefab;
	//public GameObject lairItemPrefab;
	//public GameObject hextileEventItemPrefab;
    public GameObject resourceIconPrefab;
    public GameObject playerEventItemPrefab;
    [SerializeField] private GameObject kingdomIntervenePrefab;
    [SerializeField] private GameObject notificationPrefab;

	[Space(10)]
    [Header("Main UI Objects")]
    public GameObject smallInfoGO;
	public GameObject campaignInfoGO;
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
	

	[Space(10)]
    [Header("Citizen UI Objects")]
    public UI2DSprite ctizenPortraitBG;
    public GameObject citizenInfoIsDeadIcon;
    public UILabel citizenNameLbl;
	public UILabel citizenRoleAndKingdomLbl;
	public UILabel citizenAgeLbl;
	public UILabel citizenCityNameLbl;
	public UIGrid citizenHistoryGrid;
	public ButtonToggle characterValuesBtn;
	public ButtonToggle familyTreeBtn;
	public ButtonToggle citizenHistoryBtn;
    public GameObject characterValuesGO;
    public UILabel characterValuesLbl;
	public GameObject citizenInfoForTestingGO;
	public CitizenInfoUI citizenInfoUI;
    [SerializeField] private UILabel preferredKingdomTypeLbl;

	[Space(10)]
    [Header("Events UI Objects")]
    public UIGrid gameEventsOfTypeGrid;
	public Sprite assassinationIcon;
	public Sprite rebellionPlotIcon;
	public Sprite stateVisitIcon;
	public Sprite borderConflictIcon;
	public Sprite raidIcon;
	public Sprite invasionPlotIcon;
	public Sprite militarizationIcon;
	public Sprite requestPeaceIcon;
	public Sprite declareWarIcon;
	public Sprite expansionIcon;
	public Sprite marriageInvitationIcon;

	[Space(10)]
    [Header("Relationships UI Objects")]
    public GameObject kingRelationshipsParentGO;
	public GameObject governorRelationshipsParentGO;
    public GameObject kingRelationshipLine;
    public GameObject kingNoRelationshipsGO;
    public UIScrollView kingRelationshipsScrollView;
    public UIScrollBar kingRelationshipsScrollBar;
    public UIGrid kingRelationshipsGrid;
	public UIGrid governorsRelationshipGrid;

	[Space(10)]
    [Header("Relationship History UI Objects")]
    public UI2DSprite relationshipStatusSprite;
	public UIGrid relationshipHistoryGrid;
	public GameObject noRelationshipsToShowGO;
	public GameObject relationshipHistoryForTestingGO;

	[Space(10)]
    [Header("Family Tree UI Objects")]
    public GameObject familyTreeFatherGO;
	public GameObject familyTreeMotherGO;
	public GameObject familyTreeSpouseGO;
	public UIGrid familyTreeChildGrid;
	public UI2DSprite familyTreeInnerSprite;
	public GameObject nextMarriageBtn;
    [SerializeField] private GameObject successorParentGO;
    [SerializeField] private CharacterPortrait successorPortrait;
    [SerializeField] private UILabel successorPreferredKingdomTypeLbl;
    [SerializeField] private GameObject otherCitizensGO;
    [SerializeField] private CharacterPortrait chancellorPortrait;
    [SerializeField] private CharacterPortrait marshalPortrait;

    [Space(10)]
    [Header("Kingdom Events UI Objects")]
    public ButtonToggle kingdomCurrentEventsBtn;
    public GameObject kingdomCurrentEventsGO;
    public UITable kingdomCurrentEventsContentParent;
    public UILabel kingdomNoCurrentEventsLbl;
    public ButtonToggle kingdomHistoryBtn;
    public GameObject kingdomHistoryGO;
    public UIGrid kingdomHistoryGrid;
    public UILabel kingdomHistoryNoEventsLbl;

    [Space(10)]
    [Header("Kingdoms UI Objects")]
    public UILabel kingdomNameLbl;
    public UILabel kingdomUnrestLbl;
    [SerializeField] private UILabel kingdomPrestigeLbl;
	public UILabel kingdomTechLbl;
	public UIProgressBar kingdomTechMeter;
    public UI2DSprite kingdomBasicResourceSprite;
    public UILabel kingdomBasicResourceLbl;
    public UIGrid kingdomOtherResourcesGrid;
    public UIGrid kingdomTradeResourcesGrid;
	public CharacterPortrait kingdomListActiveKing;
    public KingdomFlagItem activeKingdomFlag;
	//public UIGrid kingdomListOtherKingdomsGrid;
	public ButtonToggle kingdomListEventButton;
	public ButtonToggle kingdomListRelationshipButton;
	public ButtonToggle kingdomListCityButton;
    public Sprite stoneSprite;
    public Sprite lumberSprite;
	public UILabel actionDayLbl;
	public UILabel warmongerLbl;
	public GameObject militarizingGO;
	public GameObject fortifyingGO;

    [Space(10)]
    [Header("Event Logs UI Objects")]
    public GameObject elmEventLogsParentGO;
	public UILabel elmEventTitleLbl;
	public GameObject elmSuccessRateGO;
	public UILabel elmSuccessRateLbl;
	public UILabel elmProgressBarLbl;
	public UIProgressBar elmEventProgressBar;
	public GameObject elmFirstAnchor;
	public UIScrollView elmScrollView;

    [Space(10)]
    [Header("Cities UI Objects")]
    public UIScrollView kingdomCitiesScrollView;
    public UIGrid kingdomCitiesGrid;
    public GameObject relationshipSummaryGO;
    public UILabel relationshipSummaryLbl;
    public UILabel relationshipSummaryTitleLbl;

    [Space(10)]
    [Header("Intervene UI Objects")]
    [SerializeField] private UIGrid interveneMenuGrid;
    [SerializeField] private UIScrollView interveneMenuScrollView;
    [SerializeField] private ButtonToggle interveneMenuBtn;
    [SerializeField] private GameObject interveneActonsGO;
    //Switch Kingdom Objects
    [SerializeField] private GameObject switchKingdomGO;
    [SerializeField] private UIGrid switchKingdomGrid;
    [SerializeField] private ButtonToggle switchKingdomsBtn;
    //Create Kingdom Objects
    [SerializeField] private GameObject createKingdomGO;
    [SerializeField] private ButtonToggle createKingdomBtn;
    [SerializeField] private UILabel createKingdomRaceSelectedLbl;
    [SerializeField] private UILabel createKingdomErrorLbl;
    [SerializeField] private UIPopupList createKingdomPopupList;
    [SerializeField] private UIButton createKingdomExecuteBtn;

    [Space(10)]
    [Header("Minimap")]
    [SerializeField] private GameObject minimapGO;
    [SerializeField] private GameObject minimapTextureGO;

    [Space(10)]
    [Header("Notification Area")]
    [SerializeField] private UITable notificationParent;
    [SerializeField] private UIScrollView notificationScrollView;

    [Space(10)]
    [Header("Prestige List")]
    [SerializeField] private UILabel prestigeSummaryLbl;

	[Space(10)]
	[Header("Alliance List")]
	[SerializeField] private UILabel allianceSummaryLbl;

    private List<MarriedCouple> marriageHistoryOfCurrentCitizen;
	private int currentMarriageHistoryIndex;
	internal Citizen currentlyShowingCitizen = null;
	internal City currentlyShowingCity = null;
	internal Kingdom currentlyShowingKingdom = null;
	private GameEvent currentlyShowingEvent;
	private KingdomRelationship currentlyShowingRelationship;
	private GameObject lastClickedEventType = null;
	private War currentlyShowingWar = null;
	internal object currentlyShowingLogObject = null;

	[Space(10)] //FOR TESTING
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

    private Dictionary<DateTime, Kingdom> kingdomDisplayExpiry;

	internal List<object> eventLogsQueue = new List<object> ();

	private string warAllianceState = string.Empty;

    #region getters/setters
    internal GameObject minimapTexture {
        get { return minimapTextureGO; }
    }
    #endregion

    void Awake(){
		Instance = this;
    }

	void Start(){
        //Messenger.AddListener("OnDayEnd", CheckForKingdomExpire);
        Messenger.AddListener("UpdateUI", UpdateUI);
        //EventManager.Instance.onKingdomDiedEvent.AddListener(CheckIfShowingKingdomIsAlive);
        //EventManager.Instance.onCreateNewKingdomEvent.AddListener(AddKingdomToList);
        //EventManager.Instance.onKingdomDiedEvent.AddListener(QueueKingdomForRemoval);
        NormalizeFontSizes();
        ToggleBorders();
        toggleBordersBtn.SetClickState(true);
        //LoadKingdomList();
        UpdateUI();
	}

    private void Update() {
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

    private void NormalizeFontSizes(){
		UILabel[] allLabels = this.GetComponentsInChildren<UILabel>(true);
		//Debug.Log ("ALL LABELS COUNT: " + allLabels.Length.ToString());
		for (int i = 0; i < allLabels.Length; i++) {
			NormalizeFontSizeOfLabel(allLabels [i]);
		}
	}

	private void NormalizeFontSizeOfLabel(UILabel lbl){
		string lblName = lbl.name;
        UILabel.Overflow overflowMethod = UILabel.Overflow.ClampContent;
		if (lblName.Contains ("HEADER")) {
			lbl.fontSize = HEADER_FONT_SIZE;
            overflowMethod = UILabel.Overflow.ClampContent;
        } else if (lblName.Contains ("BODY")) {
			lbl.fontSize = BODY_FONT_SIZE;
            overflowMethod = UILabel.Overflow.ClampContent;
        } else if (lblName.Contains ("TOOLTIP")) {
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

    private void UpdateUI(){
        dateLbl.text = LocalizationManager.Instance.GetLocalizedValue("General", "Months", ((MONTH)GameManager.Instance.month).ToString()) + " " + GameManager.Instance.days.ToString () + ", " + GameManager.Instance.year.ToString ();
        KingdomManager.Instance.UpdateKingdomPrestigeList();
		UpdateAllianceSummary ();
        if (currentlyShowingKingdom != null) {
            UpdateKingdomInfo();
        }

		//if (cityInfoGO.activeSelf) {
		//	if (currentlyShowingCity != null) {
		//		ShowCityInfo (currentlyShowingCity);
		//	}
		//}

		if (eventLogsGO.activeSelf) {
			if (currentlyShowingLogObject != null) {
				ShowEventLogs (currentlyShowingLogObject);
			}
		}

        if (allKingdomEventsGO.activeSelf) {
            if (currentlyShowingKingdom != null) {
                //ShowKingdomEvents();
                if (kingdomCurrentEventsGO.activeSelf) {
                    ShowKingdomCurrentEvents();
                } else if (kingdomHistoryGO.activeSelf) {
                    ShowKingdomPastEvents();
                }
            }
        }

        //if (kingdomHistoryGO.activeSelf) {
        //	if (currentlyShowingKingdom != null) {
        //		ShowKingdomHistory ();
        //	}
        //}
        if (relationshipsGO.activeSelf) {
            if (currentlyShowingKingdom != null) {
                ShowRelationships();
            }
        }
        
        if (kingdomCitiesGO.activeSelf) {
            if (currentlyShowingKingdom != null) {
                ShowKingdomCities();
            }
        }

        if (createKingdomGO.activeSelf) {
            UpdateCreateKingdomErrorMessage();
        }
        //if (citizenInfoGO.activeSelf) {
        //    if (currentlyShowingCitizen != null) {
        //        ShowCitizenInfo(currentlyShowingCitizen);
        //    }
        //}
	}

	public void SetProgressionSpeed1X(){
		Unpause ();
		GameManager.Instance.SetProgressionSpeed(PROGRESSION_SPEED.X1);
		x1Btn.SetAsClicked();
	}
	public void SetProgressionSpeed2X(){
		Unpause ();
		GameManager.Instance.SetProgressionSpeed(PROGRESSION_SPEED.X2);
		x2Btn.SetAsClicked();
	}
	public void SetProgressionSpeed4X(){
		Unpause ();
		GameManager.Instance.SetProgressionSpeed(PROGRESSION_SPEED.X4);
		x4Btn.SetAsClicked();
	}
	public void Pause(){
		GameManager.Instance.SetPausedState(true);
		pauseBtn.SetAsClicked();
		if (onPauseEventExpiration != null) {
			onPauseEventExpiration(true);
		}
	}
	private void Unpause(){
		GameManager.Instance.SetPausedState(false);
		if (onPauseEventExpiration != null) {
			onPauseEventExpiration(false);
		}
	}

    private void UpdateKingdomInfo() {
        //currentlyShowingKingdom.UpdateFogOfWarVisual();
        kingdomListActiveKing.SetCitizen(currentlyShowingKingdom.king); //King
        kingdomPrestigeLbl.text = currentlyShowingKingdom.prestige.ToString();
        kingdomNameLbl.text = currentlyShowingKingdom.name; //Kingdom Name
        kingdomUnrestLbl.text = currentlyShowingKingdom.happiness.ToString(); //Unrest
		kingdomTechLbl.text = currentlyShowingKingdom.techLevel.ToString(); //Tech
		kingdomTechMeter.value = (float)currentlyShowingKingdom.techCounter / (float)currentlyShowingKingdom.techCapacity;
		this.militarizingGO.SetActive (currentlyShowingKingdom.isMilitarize);
		this.fortifyingGO.SetActive (currentlyShowingKingdom.isFortifying);
		this.actionDayLbl.text = this.currentlyShowingKingdom.actionDay.ToString();
		this.warmongerLbl.text = this.currentlyShowingKingdom.warmongerValue.ToString();

//		float newValue = (float)currentlyShowingKingdom.techCounter / (float)currentlyShowingKingdom.techCapacity;
//		float oldValue = kingdomTechMeter.value;
//		kingdomTechMeter.value = iTween.FloatUpdate(oldValue, newValue, GameManager.Instance.progressionSpeed);
        //Basic Resource
        if (currentlyShowingKingdom.basicResource == BASE_RESOURCE_TYPE.STONE) {
            kingdomBasicResourceSprite.sprite2D = stoneSprite;
        } else if (currentlyShowingKingdom.basicResource == BASE_RESOURCE_TYPE.WOOD) {
            kingdomBasicResourceSprite.sprite2D = lumberSprite;
        }
        kingdomBasicResourceLbl.text = currentlyShowingKingdom.basicResourceCount.ToString();

        //Available Resources
        List<Transform> children = kingdomOtherResourcesGrid.GetChildList();
        List<RESOURCE> resourcesInGrid = children.Where(x => x.GetComponent<ResourceIcon>() != null).Select(x => x.GetComponent<ResourceIcon>().resource).ToList();
        
        List<RESOURCE> allOtherResources = currentlyShowingKingdom.availableResources.Keys.ToList();
        if (resourcesInGrid.Except(allOtherResources).Any() || allOtherResources.Except(resourcesInGrid).Any()) {
            for (int i = 0; i < children.Count; i++) {
                kingdomOtherResourcesGrid.RemoveChild(children[i]);
                ObjectPoolManager.Instance.DestroyObject(children[i].gameObject);
            }
            for (int i = 0; i < currentlyShowingKingdom.availableResources.Keys.Count; i++) {
                RESOURCE currResource = currentlyShowingKingdom.availableResources.Keys.ElementAt(i);
                GameObject resourceGO = InstantiateUIObject(resourceIconPrefab.name, this.transform);
                resourceGO.GetComponent<ResourceIcon>().SetResource(currResource);
                resourceGO.transform.localScale = Vector3.one;
                kingdomOtherResourcesGrid.AddChild(resourceGO.transform);
                kingdomOtherResourcesGrid.Reposition();
            }
            RepositionGridCallback(kingdomOtherResourcesGrid);
        }

        //currentlyShowingKingdom.HighlightAllOwnedTilesInKingdom();
    }

    internal void CheckIfShowingKingdomIsAlive(Kingdom kingdom) {
        if(currentlyShowingKingdom.id == kingdom.id) {
            SetKingdomAsActive(KingdomManager.Instance.allKingdoms.FirstOrDefault());
        }
    }

    internal void UpdateMinimapInfo() {
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            currRegion.ShowNaturalResourceLevelForRace(currentlyShowingKingdom.race);
        }
        CameraMove.Instance.UpdateMinimapTexture();
    }

	internal void SetKingdomAsActive(Kingdom kingdom){
        //if(currentlyShowingKingdom != null) {
        //    currentlyShowingKingdom.UnHighlightAllOwnedTilesInKingdom();
        //}

		currentlyShowingKingdom = kingdom;

        currentlyShowingKingdom.UpdateFogOfWarVisual();
        UpdateMinimapInfo();
        //currentlyShowingKingdom.HighlightAllOwnedTilesInKingdom();
        //CameraMove.Instance.CenterCameraOn(currentlyShowingKingdom.capitalCity.hexTile.gameObject);

        //RemoveAllNotifications();

        activeKingdomFlag.SetKingdom(currentlyShowingKingdom);
        //Load all current active events of kingdom
        List<GameEvent> activeGameEventsStartedByKingdom = currentlyShowingKingdom.activeEvents.Where(x => !Utilities.eventsNotToShow.Contains(x.eventType)).ToList();;
        List<EventItem> presentEventItems = activeKingdomFlag.eventsGrid.GetChildList().Select(x => x.GetComponent<EventItem>()).ToList();

        if(activeGameEventsStartedByKingdom.Count > presentEventItems.Count) {
            int missingItems = activeGameEventsStartedByKingdom.Count - presentEventItems.Count;
            for (int i = 0; i < missingItems; i++) {
                GameObject eventGO = InstantiateUIObject(gameEventPrefab.name, activeKingdomFlag.eventsGrid.transform);
                activeKingdomFlag.AddGameObjectToGrid(eventGO);
                presentEventItems.Add(eventGO.GetComponent<EventItem>());
            }
        }

        for (int i = 0; i < presentEventItems.Count; i++) {
            EventItem currItem = presentEventItems[i];
            GameEvent currGameEvent = activeGameEventsStartedByKingdom.ElementAtOrDefault(i);
            if(currGameEvent != null) {
                currItem.SetEvent(currGameEvent);
                currItem.gameObject.SetActive(true);
            } else {
                currItem.gameObject.SetActive(false);
            }
        }


//		if (kingdomInfoGO.activeSelf) {
//			ShowKingdomInfo(currentlyShowingKingdom);
//		}
		if (citizenInfoGO.activeSelf) {
			ShowCitizenInfo(currentlyShowingKingdom.king);
		}
        if (allKingdomEventsGO.activeSelf) {
            //ShowKingdomEvents();
            if (kingdomCurrentEventsGO.activeSelf) {
                ShowKingdomCurrentEvents();
            } else if (kingdomHistoryGO.activeSelf) {
                ShowKingdomPastEvents();
            }
        }
        if (relationshipsGO.activeSelf) {
            ShowRelationships();
        }
        if (kingdomCitiesGO.activeSelf) {
            ShowKingdomCities();
        }
		UpdateKingdomInfo();
	}

	internal void ShowCitizenInfo(Citizen citizenToShow){
		currentlyShowingCitizen = citizenToShow;
		if (relationshipsGO.activeSelf) {
			if (currentlyShowingCitizen.isKing) {
				if (kingRelationshipsParentGO.activeSelf) {
					ShowRelationships ();
				} else {
					ShowRelationships();
					ShowGovernorRelationships();
				}
				HideRelationshipHistory();
			}else{
				HideRelationships();
			}
		}
		if (familyTreeGO.activeSelf) {
			HideFamilyTree();
		}
		if (citizenHistoryGO.activeSelf) {
			ShowCitizenHistory();
		}
		if (relocateGO.activeSelf) {
			HideRelocate();
		}
        if (characterValuesGO.activeSelf) {
            ShowCitizenCharacterValues();
        }

		//ForTesting
		citizenInfoForTestingGO.SetActive (true);
        preferredKingdomTypeLbl.text = currentlyShowingCitizen.preferredKingdomType.ToString();

        HideSmallInfo();

		citizenNameLbl.text = currentlyShowingCitizen.name;
        ROLE roleOfCitizen = currentlyShowingCitizen.role;
        if (currentlyShowingCitizen.city != null) {
            switch (roleOfCitizen) {
                case ROLE.UNTRAINED:
                    citizenRoleAndKingdomLbl.text = "Citizen of " + currentlyShowingCitizen.city.name;
                    break;
                case ROLE.GOVERNOR:
                    citizenRoleAndKingdomLbl.text = "Governor of " + currentlyShowingCitizen.city.name;
                    break;
                case ROLE.KING:
                    if (currentlyShowingCitizen.gender == GENDER.MALE) {
                        citizenRoleAndKingdomLbl.text = "King of " + currentlyShowingCitizen.city.kingdom.name;
                    } else {
                        citizenRoleAndKingdomLbl.text = "Queen of " + currentlyShowingCitizen.city.kingdom.name;
                    }
                    break;
                case ROLE.QUEEN_CONSORT:
                    citizenRoleAndKingdomLbl.text = "Queen's Consort of " + currentlyShowingCitizen.city.kingdom.name;
                    break;
                default:
                    string role = Utilities.NormalizeString(roleOfCitizen.ToString());
                    citizenRoleAndKingdomLbl.text = role + " of " + currentlyShowingCitizen.city.kingdom.name;
                    break;
            }
			citizenCityNameLbl.text = currentlyShowingCitizen.city.name;
			ctizenPortraitBG.color = currentlyShowingCitizen.city.kingdom.kingdomColor;
		} else {
			citizenRoleAndKingdomLbl.text = Utilities.NormalizeString(roleOfCitizen.ToString());
			citizenCityNameLbl.text = "No City";
			ctizenPortraitBG.color = Color.white;
		}

		citizenAgeLbl.text = "Age: " + currentlyShowingCitizen.age.ToString();

		if (currentlyShowingCitizen.isDead) {
			citizenInfoIsDeadIcon.SetActive (true);
		} else {
			citizenInfoIsDeadIcon.SetActive (false);
		}

		//if (citizenToShow.isKing) {
		//	characterValuesBtn.gameObject.SetActive(true);
		//} else {
  //          characterValuesBtn.gameObject.SetActive(false);
		//}

		//HideCityInfo();
		citizenInfoGO.SetActive (true);
		this.marriageHistoryOfCurrentCitizen = MarriageManager.Instance.GetCouplesCitizenInvoledIn(citizenToShow);

        //HideGovernorLoyalty ();
        this.citizenInfoUI.SetTraits(currentlyShowingCitizen);
  //      if (citizenToShow.assignedRole != null){
		//	if (citizenToShow.assignedRole is Governor) {
		//		Governor governor = (Governor)citizenToShow.assignedRole;
		//		//ShowGovernorLoyalty ();
		//		this.citizenInfoUI.SetGovernorTraits (governor);
		//	} else if (citizenToShow.assignedRole is King) {
		//		King king = (King)citizenToShow.assignedRole;
		//		this.citizenInfoUI.SetKingTraits (king);
		//	}
		//}
	}
	public void HideCitizenInfo(){
		currentlyShowingCitizen = null;
		citizenInfoGO.SetActive(false);
		HideFamilyTree();
        HideCitizenCharacterValues();
		//HideGovernorLoyalty ();
    }

    public void ToggleCharacterValues() {
        if (this.characterValuesGO.activeSelf) {
            HideCitizenCharacterValues();
        } else {
            ShowCitizenCharacterValues();
        }
    }
    public void ShowCitizenCharacterValues() {
        characterValuesLbl.text = string.Empty;
        for (int i = 0; i < currentlyShowingCitizen.importantCharacterValues.Keys.Count; i++) {
            CHARACTER_VALUE currValue = currentlyShowingCitizen.importantCharacterValues.Keys.ElementAt(i);
            characterValuesLbl.text += Utilities.FirstLetterToUpperCase(currValue.ToString().Replace('_', ' '));
            if(currentlyShowingCitizen.importantCharacterValues.Keys.Last() != currValue) {
                characterValuesLbl.text += "\n";
            }
        }

        this.characterValuesGO.SetActive(true);
    }
    public void HideCitizenCharacterValues() {
        this.characterValuesBtn.SetClickState(false);
        this.characterValuesGO.SetActive(false);
    }

    public void ToggleCitizenHistory(){
		if (this.citizenHistoryGO.activeSelf) {
			this.citizenHistoryGO.SetActive (false);
		} else {
			ShowCitizenHistory();
		}
	}
	public void ShowCitizenHistory(){
		if (this.currentlyShowingCitizen == null) {
			return;
		}

		List<Transform> children = this.citizenHistoryGrid.GetChildList ();
		for (int i = 0; i < children.Count; i++) {
			ObjectPoolManager.Instance.DestroyObject(children [i].gameObject);
		}

		for (int i = 0; i < this.currentlyShowingCitizen.history.Count; i++) {
			GameObject citizenGO = InstantiateUIObject(this.historyPortraitPrefab.name, this.citizenHistoryGrid.transform);
			citizenGO.GetComponent<HistoryPortrait>().SetHistory(this.currentlyShowingCitizen.history[i]);
			citizenGO.transform.localScale = Vector3.one;
			citizenGO.transform.localPosition = Vector3.zero;
		}

		StartCoroutine (RepositionGrid (this.citizenHistoryGrid));
		this.citizenHistoryGO.SetActive (true);
	}
	public void HideCitizenHistory(){
		citizenHistoryBtn.SetClickState(false);
		this.citizenHistoryGO.SetActive(false);
	}

	public void ShowCurrentlyShowingCitizenCityInfo(){
		//ShowCityInfo (currentlyShowingCitizen.city, true);
		CameraMove.Instance.CenterCameraOn (currentlyShowingCitizen.city.hexTile.gameObject);
	}

	public void RepositionGridCallback(UIGrid thisGrid){
		StartCoroutine (RepositionGrid (thisGrid));
	}

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

    public void UpdateRelationships() {
        if (relationshipsGO.activeSelf) {
            if (currentlyShowingKingdom != null) {
                ShowRelationships();
            }
        }
    }
	public void ShowRelationships(){
        kingdomListRelationshipButton.SetClickState(true);
        relationshipsGO.SetActive (true);
        HideKingdomCities();
        HideAllKingdomEvents();
        ShowKingRelationships();
	}
	public void ToggleRelationships(){
		if (relationshipsGO.activeSelf == true) {
			governorRelationshipsParentGO.SetActive(false);
			kingRelationshipsParentGO.SetActive(false);
			relationshipsGO.SetActive (false);
		} else {
			ShowRelationships();
		}
	}

    private Kingdom currentKingdomRelationshipShowing;
	public void ShowKingRelationships(){
		List<CharacterPortrait> characterPortraits = kingRelationshipsGrid.gameObject.GetComponentsInChildren<Transform>(true)
                                                    .Where(x => x.GetComponent<CharacterPortrait>() != null)
                                                    .Select(x => x.GetComponent<CharacterPortrait>()).ToList();

        int nextIndex = 0;
        List<KingdomRelationship> relationshipsToShow = currentlyShowingKingdom.relationships
            .Where(x => currentlyShowingKingdom.discoveredKingdoms.Contains(x.Value.targetKingdom)).Select(x => x.Value).ToList();

        if (relationshipsToShow.Count > 0) {
            kingRelationshipLine.SetActive(true);
            kingNoRelationshipsGO.SetActive(false);
        } else {
            kingRelationshipLine.SetActive(false);
            kingNoRelationshipsGO.SetActive(true);
        }

        for (int i = 0; i < characterPortraits.Count; i++) {
			CharacterPortrait currPortrait = characterPortraits[i];
            if(i < relationshipsToShow.Count) {
                KingdomRelationship currRel = relationshipsToShow[i];
                if (currRel != null) {
                    currPortrait.SetCitizen(currRel.targetKingdom.king, true);
                    currPortrait.ShowRelationshipLine(currRel, currRel.targetKingdom.GetRelationshipWithKingdom(currentlyShowingKingdom));
                    currPortrait.gameObject.SetActive(true);
                } else {
                    currPortrait.gameObject.SetActive(false);
                }
                nextIndex = i + 1;
            } else {
                currPortrait.gameObject.SetActive(false);
            }
        }

        if (relationshipsToShow.Count - 1 >= nextIndex) {
			for (int i = nextIndex; i < relationshipsToShow.Count; i++) {
                KingdomRelationship rel = relationshipsToShow[i];

                GameObject kingGO = InstantiateUIObject(characterPortraitPrefab.name, this.transform);
				kingGO.GetComponent<CharacterPortrait>().SetCitizen(rel.targetKingdom.king, true);
				kingGO.transform.localScale = Vector3.one;
				kingGO.GetComponent<CharacterPortrait> ().ShowRelationshipLine (rel, 
                    rel.targetKingdom.GetRelationshipWithKingdom(currentlyShowingKingdom));
				kingRelationshipsGrid.AddChild(kingGO.transform);
                kingRelationshipsGrid.Reposition();
            }

            if (kingRelationshipsParentGO.activeSelf) {
                StartCoroutine(RepositionScrollView(kingRelationshipsScrollView, true));
            }
        }

        if(currentKingdomRelationshipShowing == null || currentKingdomRelationshipShowing.id != currentlyShowingKingdom.id || !kingRelationshipsParentGO.activeSelf) {
            StartCoroutine(RepositionGrid(kingRelationshipsGrid));
            StartCoroutine(RepositionScrollView(kingRelationshipsScrollView));
            kingRelationshipsScrollView.UpdateScrollbars();
        }

        governorRelationshipsParentGO.SetActive(false);
		kingRelationshipsParentGO.SetActive(true);
        currentKingdomRelationshipShowing = currentlyShowingKingdom;
    }
	public void ShowGovernorRelationships(){
		if (governorRelationshipsParentGO.activeSelf) {
			return;
		}
		List<Transform> children = governorsRelationshipGrid.GetChildList();
		for (int i = 0; i < children.Count; i++) {
			ObjectPoolManager.Instance.DestroyObject(children [i].gameObject);
		}

        for (int i = 0; i < currentlyShowingCitizen.city.kingdom.cities.Count; i++) {
			GameObject governorGO = InstantiateUIObject(characterPortraitPrefab.name, governorsRelationshipGrid.transform);
			governorGO.GetComponent<CharacterPortrait>().SetCitizen(currentlyShowingCitizen.city.kingdom.cities[i].governor, true);
			governorGO.GetComponent<CharacterPortrait>().DisableHover();
			governorGO.transform.localScale = new Vector3(1.5f, 1.5f, 0);
			governorGO.GetComponent<CharacterPortrait>().ShowRelationshipLine();
//			governorGO.GetComponent<CharacterPortrait>().onClickCharacterPortrait += ShowRelationshipHistory;
		}

		kingRelationshipsParentGO.SetActive(false);
		governorRelationshipsParentGO.SetActive(true);
		RepositionGridCallback(governorsRelationshipGrid);
	}
	public void HideRelationships(){
        kingRelationshipsParentGO.SetActive (false);
		governorRelationshipsParentGO.SetActive(false);
		relationshipsGO.SetActive (false);
        //relationshipsBtn.SetClickState(false);
        kingdomListRelationshipButton.SetClickState(false);
	}

	public void HideRelationshipHistory(){
		relationshipHistoryGO.SetActive(false);
	}

	public void ShowGovernorInfo(){
		if (this.currentlyShowingCity != null) {
			this.ShowCitizenInfo (this.currentlyShowingCity.governor);
		}
	}

	public void ShowSmallInfo(string info){
		smallInfoLbl.text = info;

		var v3 = Input.mousePosition;
		v3.z = 10.0f;
		v3 = uiCamera.GetComponent<Camera>().ScreenToWorldPoint(v3);
		v3.y -= 0.15f;
		if (v3.y <= -1f) {
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
		smallInfoGO.SetActive (true);
	}
	public void HideSmallInfo(){
		smallInfoGO.SetActive (false);
		smallInfoGO.transform.parent = this.transform;
	}

	public void ShowCampaignInfo(Campaign campaign, General general, Transform parent){
//		smallInfoLbl.text = info;
		this.campaignInfoGO.GetComponent<CampaignInfo>().SetCampaignInfo(campaign, general);
		var v3 = Input.mousePosition;
		v3.z = 10.0f;
		v3 = uiCamera.GetComponent<Camera>().ScreenToWorldPoint(v3);
		v3.y -= 0.13f;
		this.campaignInfoGO.transform.position = v3;
		//		smallInfoGO.transform.parent = parent;
		//		smallInfoGO.transform.localPosition = new Vector3 (0f, -100f, 0f);
		//		Vector3 newPos = smallInfoGO.transform.localPosition;

		//		smallInfoGO.transform.localPosition = newPos;
		//		smallInfoGO.transform.parent = this.transform;
		//		smallInfoGO.transform.localScale = Vector3.one;
		this.campaignInfoGO.SetActive (true);
	}
	public void HideCampaignInfo(){
		this.campaignInfoGO.SetActive (false);
		this.campaignInfoGO.transform.parent = this.transform;
	}

	public void ToggleFamilyTree(){
		if (familyTreeGO.activeSelf) {
            HideFamilyTree();
		} else {
            ShowFamilyTree();
		}
	}
    public void ShowFamilyTree() {
        if (familyTreeFatherGO.GetComponentInChildren<CharacterPortrait>() != null) {
            ObjectPoolManager.Instance.DestroyObject(familyTreeFatherGO.GetComponentInChildren<CharacterPortrait>().gameObject);
        }
        if (currentlyShowingCitizen.father != null) {
            GameObject fatherGO = InstantiateUIObject(characterPortraitPrefab.name, familyTreeFatherGO.transform);
            fatherGO.transform.localScale = new Vector3(2.1f, 2.1f, 0f);
            fatherGO.transform.localPosition = Vector3.zero;
            fatherGO.GetComponent<CharacterPortrait>().SetCitizen(currentlyShowingCitizen.father);
        }
        if (familyTreeMotherGO.GetComponentInChildren<CharacterPortrait>() != null) {
            ObjectPoolManager.Instance.DestroyObject(familyTreeMotherGO.GetComponentInChildren<CharacterPortrait>().gameObject);
        }
        if (currentlyShowingCitizen.mother != null) {
            GameObject motherGO = InstantiateUIObject(characterPortraitPrefab.name, familyTreeMotherGO.transform);
            motherGO.transform.localScale = new Vector3(2.1f, 2.1f, 0f);
            motherGO.transform.localPosition = Vector3.zero;
            motherGO.GetComponent<CharacterPortrait>().SetCitizen(currentlyShowingCitizen.mother);
        }
        if (familyTreeSpouseGO.GetComponentInChildren<CharacterPortrait>() != null) {
            ObjectPoolManager.Instance.DestroyObject(familyTreeSpouseGO.GetComponentInChildren<CharacterPortrait>().gameObject);
        }
        if (currentlyShowingCitizen.spouse != null) {
            GameObject spouseGO = InstantiateUIObject(characterPortraitPrefab.name, familyTreeSpouseGO.transform);
            spouseGO.transform.localScale = new Vector3(2.1f, 2.1f, 0f);
            spouseGO.transform.localPosition = Vector3.zero;
            spouseGO.GetComponent<CharacterPortrait>().SetCitizen(currentlyShowingCitizen.spouse);
            //for (int i = 0; i < this.marriageHistoryOfCurrentCitizen.Count; i++) {
            //    if (currentlyShowingCitizen.gender == GENDER.MALE) {
            //        if (this.marriageHistoryOfCurrentCitizen[i].wife.id == currentlyShowingCitizen.spouse.id) {
            //            this.currentMarriageHistoryIndex = i;
            //            break;
            //        }
            //    } else {
            //        if (this.marriageHistoryOfCurrentCitizen[i].husband.id == currentlyShowingCitizen.spouse.id) {
            //            this.currentMarriageHistoryIndex = i;
            //            break;
            //        }
            //    }
            //}
        } 
        //else if (this.marriageHistoryOfCurrentCitizen.Count > 0) {
        //    GameObject spouseGO = InstantiateUIObject(characterPortraitPrefab.name, familyTreeSpouseGO.transform);
        //    spouseGO.transform.localScale = new Vector3(2.1f, 2.1f, 0f);
        //    spouseGO.transform.localPosition = Vector3.zero;
        //    if (currentlyShowingCitizen.gender == GENDER.MALE) {
        //        spouseGO.GetComponent<CharacterPortrait>().SetCitizen(this.marriageHistoryOfCurrentCitizen[0].wife);
        //    } else {
        //        spouseGO.GetComponent<CharacterPortrait>().SetCitizen(this.marriageHistoryOfCurrentCitizen[0].husband);
        //    }
        //    this.currentMarriageHistoryIndex = 0;
        //}

        CharacterPortrait[] children = familyTreeChildGrid.GetComponentsInChildren<CharacterPortrait>();
        for (int i = 0; i < children.Length; i++) {
            ObjectPoolManager.Instance.DestroyObject(children[i].gameObject);
        }

        List<Transform> childPositions = familyTreeChildGrid.GetChildList();
        for (int i = 0; i < currentlyShowingCitizen.children.Count; i++) {
            GameObject childGO = InstantiateUIObject(characterPortraitPrefab.name, childPositions[i].transform);
            childGO.transform.localScale = new Vector3(2.1f, 2.1f, 0f);
            childGO.transform.localPosition = Vector3.zero;
            childGO.GetComponent<CharacterPortrait>().SetCitizen(currentlyShowingCitizen.children[i]);
        }

        if (this.marriageHistoryOfCurrentCitizen.Count > 1) {
            nextMarriageBtn.SetActive(true);
        } else {
            nextMarriageBtn.SetActive(false);
        }

        familyTreeInnerSprite.color = currentlyShowingCitizen.city.kingdom.kingdomColor;

        //Show Successor
        if (currentlyShowingCitizen.role == ROLE.KING) {
            if (currentlyShowingCitizen.city.kingdom.successionLine.Count > 0) {
                Citizen successor = currentlyShowingCitizen.city.kingdom.successionLine.FirstOrDefault();
                successorPortrait.SetCitizen(successor, false, true);
                successorPreferredKingdomTypeLbl.text = successor.preferredKingdomType.ToString();
                successorParentGO.SetActive(true);
            } else {
                successorParentGO.SetActive(false);
            }

            //Show Other Citizens
            otherCitizensGO.SetActive(true);
            chancellorPortrait.SetCitizen(currentlyShowingCitizen.city.importantCitizensInCity[ROLE.GRAND_CHANCELLOR], false, true);
            chancellorPortrait.SetCitizen(currentlyShowingCitizen.city.importantCitizensInCity[ROLE.GRAND_MARSHAL], false, true);
        } else {
            successorParentGO.SetActive(false);
            otherCitizensGO.SetActive(false);
        }
        
        familyTreeGO.SetActive(true);
    }
	public void HideFamilyTree(){
		familyTreeBtn.SetClickState(false);
		familyTreeGO.SetActive(false);
	}

	public void ShowNextMarriage(){
		int nextIndex = this.currentMarriageHistoryIndex + 1;
		if (nextIndex == this.marriageHistoryOfCurrentCitizen.Count) {
			//max index reached
			nextIndex = 0;
		}

        ObjectPoolManager.Instance.DestroyObject(familyTreeSpouseGO.GetComponentInChildren<CharacterPortrait> ().gameObject);
		CharacterPortrait[] children = familyTreeChildGrid.GetComponentsInChildren<CharacterPortrait>();
		for (int i = 0; i < children.Length; i++) {
            ObjectPoolManager.Instance.DestroyObject(children [i].gameObject);
		}

		MarriedCouple marriedCoupleToShow = this.marriageHistoryOfCurrentCitizen[nextIndex];
		if (marriedCoupleToShow.husband.id == currentlyShowingCitizen.id) {
			//currentlyShowingCitizen is male
			GameObject spouseGO = InstantiateUIObject(characterPortraitPrefab.name, familyTreeSpouseGO.transform);
			spouseGO.transform.localScale = new Vector3 (2.1f, 2.1f, 0f);
			spouseGO.transform.localPosition = Vector3.zero;
			spouseGO.GetComponent<CharacterPortrait> ().SetCitizen (marriedCoupleToShow.wife);
		} else {
			//currentlyShowingCitizen is female	
			GameObject spouseGO = InstantiateUIObject(characterPortraitPrefab.name, familyTreeSpouseGO.transform);
			spouseGO.transform.localScale = new Vector3 (2.1f, 2.1f, 0f);
			spouseGO.transform.localPosition = Vector3.zero;
			spouseGO.GetComponent<CharacterPortrait> ().SetCitizen (marriedCoupleToShow.husband);
		}

		List<Transform> childPositions = familyTreeChildGrid.GetChildList ();
		for (int i = 0; i < marriedCoupleToShow.children.Count; i++) {
			GameObject childGO = InstantiateUIObject(characterPortraitPrefab.name, childPositions[i].transform);
			childGO.transform.localScale = new Vector3(2.1f, 2.1f, 0f);
			childGO.transform.localPosition = Vector3.zero;
			childGO.GetComponent<CharacterPortrait>().SetCitizen(marriedCoupleToShow.children[i]);
		}
	}

	public void ToggleEventsMenu(){
		eventsGo.SetActive(!eventsGo.activeSelf);
		if (!eventsGo.activeSelf) {
			eventsOfTypeGo.SetActive(false);
//			EventManager.Instance.onHideEvents.Invoke();
//			EventManager.Instance.onShowEventsOfType.Invoke (EVENT_TYPES.ALL);
			List<Transform> events = eventCategoriesGrid.GetChildList();
			for (int i = 0; i < events.Count; i++) {
				events [i].GetComponent<ButtonToggle>().SetClickState(false);
			}
		}

	}

	public void ShowEventsOfType(GameEvent gameEvent, Kingdom kingdom = null){
		if(Utilities.eventsNotToShow.Contains(gameEvent.eventType)){
			return;
		}
		if(gameEvent.startedBy != null){ //Kingdom Event
			if(kingdom == null){
				if(gameEvent.startedByKingdom.id == currentlyShowingKingdom.id) {
					activeKingdomFlag.AddEventToGrid(gameEvent);
					if(this.currentlyShowingLogObject != null){
						this.eventLogsQueue.Add (gameEvent);
					}else{
						//					Pause();
						ShowEventLogs(gameEvent);
					}
				}
			}else{
				if(kingdom.id == currentlyShowingKingdom.id) {
					activeKingdomFlag.AddEventToGrid(gameEvent);
					if(this.currentlyShowingLogObject != null){
						this.eventLogsQueue.Add (gameEvent);
					}else{
						//					Pause();
						ShowEventLogs(gameEvent);
					}
				}
			}


			//KingdomFlagItem kingdomOwner = kingdomListOtherKingdomsGrid.GetChildList()
			//	.Where(x => x.GetComponent<KingdomFlagItem>().kingdom.id == gameEvent.startedByKingdom.id)
			//	.FirstOrDefault().GetComponent<KingdomFlagItem>();
			//if (kingdomOwner != null) {
			//	GameObject eventGO = InstantiateUIObject(gameEventPrefab, this.transform);
			//	eventGO.GetComponent<EventItem>().SetEvent(gameEvent);
			//	eventGO.GetComponent<EventItem>().SetSpriteIcon(GetSpriteForEvent(gameEvent.eventType));
			//	eventGO.GetComponent<EventItem>().onClickEvent += ShowEventLogs;
			//	eventGO.GetComponent<EventItem>().StartExpirationTimer();
			//	eventGO.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
			//	kingdomOwner.AddGameObjectToGrid(eventGO);
			//	gameEvent.goEventItem = eventGO;
			//	if(kingdomOwner.kingdom.id == this.currentlyShowingKingdom.id){
			//		Pause();
			//		ShowEventLogs(gameEvent);
			//	}
			//}
		}else{ //World Event
			GameObject eventGO = InstantiateUIObject(this.gameEventPrefab.name, this.gameEventsOfTypeGrid.transform);
			eventGO.GetComponent<EventItem>().SetEvent(gameEvent);
			//eventGO.GetComponent<EventItem>().SetSpriteIcon(GetSpriteForEvent(gameEvent.eventType));
			//eventGO.GetComponent<EventItem>().onClickEvent += ShowEventLogs;
			//eventGO.GetComponent<EventItem>().StartExpirationTimer();
			eventGO.transform.localPosition = Vector3.zero;
			eventGO.transform.localScale = Vector3.one;
			RepositionGridCallback(this.gameEventsOfTypeGrid);
			//gameEvent.goEventItem = eventGO;
		}
	}
	public void ShowPlayerEventsOfType(PlayerEvent playerEvent){
		if (playerEvent.affectedKingdoms.Contains(this.currentlyShowingKingdom)) {
			if (this.currentlyShowingLogObject != null) {
				this.eventLogsQueue.Add (playerEvent);
			} else {
//				Pause ();
				ShowEventLogs (playerEvent);
			}
		}
	}

    public void ShowNotification(Log log) {
        GameObject notifGO = InstantiateUIObject(notificationPrefab.name, notificationParent.transform);
        notifGO.transform.localScale = Vector3.one;
        notifGO.GetComponent<NotificationItem>().SetLog(log);
        RepositionNotificationTable();
        //notificationScrollView.UpdatePosition();
        //notificationParent.Reposition();
        notificationScrollView.UpdatePosition();

    }
    public void RemoveAllNotifications() {
        List<Transform> children = notificationParent.GetChildList();
        for (int i = 0; i < children.Count; i++) {
            ObjectPoolManager.Instance.DestroyObject(children[i].gameObject);
        }
    }
    public void RepositionNotificationTable() {
        StartCoroutine(RepositionTable(notificationParent));
        //StartCoroutine(RepositionScrollView(notificationParent.GetComponentInParent<UIScrollView>()));
    }

	/*
	 * Show Event Logs menu
	 * */
	public void ShowEventLogs(object obj){
		if (obj == null) {
			return;
		}
		if(this.eventLogsQueue.Contains(obj)){
			this.eventLogsQueue.Remove (obj);
		}
		List<Log> logs = new List<Log> ();
		if (obj is GameEvent) {
			GameEvent ge = ((GameEvent)obj);
			logs = ge.logs;
			elmEventTitleLbl.text = Utilities.LogReplacer (logs.FirstOrDefault ());
            elmEventProgressBar.gameObject.SetActive (false);
			elmProgressBarLbl.gameObject.SetActive (false);
		} else if (obj is PlayerEvent) {
			PlayerEvent pe = ((PlayerEvent)obj);
			logs = pe.logs;
			elmEventTitleLbl.text = Utilities.LogReplacer (logs.FirstOrDefault ());
            elmEventProgressBar.gameObject.SetActive (false);
			elmProgressBarLbl.gameObject.SetActive (false);
		}
		//elmProgressBarLbl.text = "Progress:";
		elmSuccessRateGO.SetActive (false);

		currentlyShowingLogObject = obj;

		GameObject nextAnchorPoint = elmFirstAnchor;
		List<EventLogItem> currentlyShowingLogs = elmEventLogsParentGO.GetComponentsInChildren<EventLogItem>(true).ToList();

        if((logs.Count - 1) > currentlyShowingLogs.Count) {
            int logItemsToCreate = (logs.Count - 1) - currentlyShowingLogs.Count;
            for (int i = 0; i < logItemsToCreate; i++) {
                GameObject logGO = InstantiateUIObject(logItemPrefab.name, elmEventLogsParentGO.transform);
                logGO.transform.localScale = Vector3.one;
                currentlyShowingLogs.Add(logGO.GetComponent<EventLogItem>());
            }
        }

        for (int i = 0; i < currentlyShowingLogs.Count; i++) {
            EventLogItem currELI = currentlyShowingLogs[i];
            Log currLog = logs.ElementAtOrDefault(i+1);
            if(currLog == null) {
                currELI.gameObject.SetActive(false);
            } else {
                currELI.SetLog(currLog);
                if ((i+1) % 2 == 0) {
                    currELI.DisableBG();
                } else {
                    currELI.EnableBG();
                }
                currELI.gameObject.SetActive(true);
            }
        }

		StartCoroutine(RepositionTable(elmEventLogsParentGO.GetComponent<UITable>()));
        StartCoroutine(RepositionScrollView(elmScrollView));

	    if(this.currentlyShowingLogObject is GameEvent){
		    if(((GameEvent)this.currentlyShowingLogObject).goEventItem != null){
			    ((GameEvent)this.currentlyShowingLogObject).goEventItem.GetComponent<EventItem>().DeactivateNewLogIndicator ();
		    }
	    }

	    if(!this.eventLogsGO.activeSelf){
		    this.eventLogsGO.SetActive(true);
	    }
	}
	public void HideEventLogs(){
		if(this.currentlyShowingLogObject is GameEvent){
			GameEvent gameEvent = (GameEvent)this.currentlyShowingLogObject;
			if(!gameEvent.isActive){
				if (gameEvent.goEventItem != null) {
					gameEvent.goEventItem.GetComponent<EventItem>().HasExpired();
                    StartCoroutine(RepositionGrid(activeKingdomFlag.eventsGrid));
				}
            } else {
                if (gameEvent.goEventItem != null) {
                    gameEvent.goEventItem.GetComponent<ButtonToggle>().SetClickState(false);
                }
            }
		}
		//if(GameManager.Instance.isPaused){
		//	SetProgressionSpeed1X();
		//}
		currentlyShowingLogObject = null;
		if(this.eventLogsQueue.Count > 0){
//			Pause();
			object obj = this.eventLogsQueue [0];
			ShowEventLogs (obj);
		}
        eventLogsGO.SetActive(false);
    }
	
    /*
	 * Toggle Kingdom Events Menu
	 * */
	public void ToggleKingdomEvents(){
		if (allKingdomEventsGO.activeSelf) {
			HideAllKingdomEvents();
		} else {
//			Pause();
			ShowKingdomEvents();
		}
	}
	/*
	 * Show Kingdom Events Menu
	 * */
	public void ShowKingdomEvents(){
        //HideKingdomHistory();
        HideRelationships();
        HideKingdomCities();
        ShowKingdomCurrentEvents();

        allKingdomEventsGO.SetActive (true);
	}
    public void ShowKingdomCurrentEvents() {
        HideKingdomPastEvents();
//		List<GameEvent> allActiveEventsInKingdom = EventManager.Instance.GetAllEventsKingdomIsInvolvedIn(currentlyShowingKingdom).Where(x => x.isActive).ToList();
		List<GameEvent> politicalEvents = currentlyShowingKingdom.activeEvents.Where(x => x.eventType != EVENT_TYPES.KINGDOM_WAR && x.eventType != EVENT_TYPES.INVASION_PLAN && 
        x.eventType != EVENT_TYPES.JOIN_WAR_REQUEST && x.eventType != EVENT_TYPES.REQUEST_PEACE && !Utilities.eventsNotToShow.Contains(x.eventType)).ToList();

		List<GameEvent> wars = currentlyShowingKingdom.activeEvents.Where(x => x.eventType == EVENT_TYPES.KINGDOM_WAR).ToList();

        if (politicalEvents.Count <= 0 && wars.Count <= 0) {
            kingdomNoCurrentEventsLbl.gameObject.SetActive(true);
            allKingdomEventsGO.SetActive(true);
            //			this.pauseBtn.SetAsClicked ();
            //			GameManager.Instance.SetPausedState (true);
            EventListParent[] currentParents = Utilities.GetComponentsInDirectChildren<EventListParent>(kingdomCurrentEventsContentParent.gameObject);
            if (currentParents.Length > 0) {
                for (int i = 0; i < currentParents.Length; i++) {
                    EventListParent currentParent = currentParents[i];
                    ObjectPoolManager.Instance.DestroyObject(currentParent.gameObject);
                }
            }
        } else {
            kingdomNoCurrentEventsLbl.gameObject.SetActive(false);
            LoadPoliticalEvents(politicalEvents);
            LoadWarEvents(wars);
        }
        
        kingdomCurrentEventsBtn.SetClickState(true);
        kingdomCurrentEventsGO.SetActive(true);
    }
    private void HideKingdomCurrentEvents() {
        kingdomCurrentEventsBtn.SetClickState(false);
        kingdomCurrentEventsGO.SetActive(false);
    }
    public void ShowKingdomPastEvents() {
        HideKingdomCurrentEvents();
        List<GameEvent> allDoneEvents = currentlyShowingKingdom.doneEvents.
            Where(x => !Utilities.eventsNotToShow.Contains(x.eventType)).ToList();
        allDoneEvents = allDoneEvents.OrderByDescending(x => x.startDate).ToList();

        List<EventListItem> currentItems = kingdomHistoryGrid.GetChildList().Select(x => x.GetComponent<EventListItem>()).ToList();
        int nextItem = 0;
        for (int i = 0; i < currentItems.Count; i++) {
            EventListItem currItem = currentItems[i];
            if (i < allDoneEvents.Count) {
                GameEvent gameEventToShow = allDoneEvents[i];
                if (gameEventToShow != null) {
                    currItem.SetEvent(gameEventToShow, currentlyShowingKingdom);
                    currItem.gameObject.SetActive(true);
                    nextItem = i + 1;
                } else {
                    currItem.gameObject.SetActive(false);
                }
            } else {
                currItem.gameObject.SetActive(false);
            }
        }

        for (int i = nextItem; i < allDoneEvents.Count; i++) {
            GameObject eventGO = InstantiateUIObject(kingdomEventsListItemPrefab.name, this.transform);
            eventGO.transform.localScale = Vector3.one;
            kingdomHistoryGrid.AddChild(eventGO.transform);
            eventGO.GetComponent<EventListItem>().SetEvent(allDoneEvents[i], currentlyShowingKingdom);
            eventGO.GetComponent<EventListItem>().onClickEvent += ShowEventLogs;
            kingdomHistoryGrid.Reposition();
        }

        if (allDoneEvents.Count <= 0) {
            kingdomHistoryNoEventsLbl.gameObject.SetActive(true);
        } else {
            kingdomHistoryNoEventsLbl.gameObject.SetActive(false);
        }
        kingdomHistoryBtn.SetClickState(true);
        kingdomHistoryGO.SetActive(true);
    }
    private void HideKingdomPastEvents() {
        kingdomHistoryBtn.SetClickState(false);
        kingdomHistoryGO.SetActive(false);
    }
    /*
	 * Load all political events onto the kingdom events menu.
	 * Political Events incl. [STATE VISIT, RAID, ASSASSINATION, DIPLOMATIC CRISIS, BORDER CONFLICT]
	 * update this as needed!
	 * */
    private void LoadPoliticalEvents(List<GameEvent> politicalEvents){
		EventListParent politicsParent = null;
		List<Transform> allCurrentParents = Utilities.GetComponentsInDirectChildren<Transform>(kingdomCurrentEventsContentParent.gameObject).ToList();

		GameObject politicsParentGO = null;
		for (int i = 0; i < allCurrentParents.Count; i++) {
			if (allCurrentParents[i].name.Equals("PoliticsParent")) {
				politicsParentGO = allCurrentParents[i].gameObject;
				allCurrentParents.RemoveAt (i);
				break;
			}
		}

		if (politicalEvents.Count <= 0) {
			if (politicsParentGO == null) {
                ObjectPoolManager.Instance.DestroyObject(politicsParentGO);
			}
			return;
		}

		if (politicsParentGO == null) {
			//Instantiate Politics Parent
			politicsParentGO = InstantiateUIObject(kingdomEventsListParentPrefab.name, kingdomCurrentEventsContentParent.transform) as GameObject;
			politicsParentGO.name = "PoliticsParent";
			politicsParentGO.transform.localScale = Vector3.one;
			politicsParentGO.transform.localPosition = Vector3.zero;
			politicsParentGO.GetComponent<EventListParent>().eventTitleLbl.text = "Politics";
		}
		
		politicsParent = politicsParentGO.GetComponent<EventListParent>();

		List<EventListItem> currentlyShowingGameEvents = politicsParent.eventsGrid.GetChildList ().Select (x => x.GetComponent<EventListItem>()).ToList ();
		List<int> currentlyShowingGameEventIDs = currentlyShowingGameEvents.Select (x => x.gameEvent.id).ToList ();
		List<int> actualPoliticalEventIDs = politicalEvents.Select (x => x.id).ToList ();
		if (actualPoliticalEventIDs.Except (currentlyShowingGameEventIDs).Union (currentlyShowingGameEventIDs.Except (actualPoliticalEventIDs)).Any()) {
			for (int i = 0; i < currentlyShowingGameEvents.Count; i++) {
				politicsParent.eventsGrid.RemoveChild (currentlyShowingGameEvents[i].transform);
                ObjectPoolManager.Instance.DestroyObject(currentlyShowingGameEvents[i].gameObject);
			}

			//Instantiate all polical events into the politics parent grid
			for (int i = 0; i < politicalEvents.Count; i++) {
				GameObject eventGO = InstantiateUIObject(kingdomEventsListItemPrefab.name, this.transform);
				eventGO.transform.localScale = Vector3.one;
				politicsParent.eventsGrid.AddChild(eventGO.transform);
				eventGO.GetComponent<EventListItem>().SetEvent (politicalEvents[i], currentlyShowingKingdom);
				eventGO.GetComponent<EventListItem>().onClickEvent += ShowEventLogs;
			}
			StartCoroutine (RepositionGrid (politicsParent.eventsGrid));
		}
		RepositionKingdomEventsTable ();
	}
	/*
	 * Load all wars onto the kingdom events menu.
	 * War events only contain Campaigns.
	 * */
	private void LoadWarEvents(List<GameEvent> wars){
		WarEventListParent currentWarParent = null;
        List<int> allWarIDs = wars.Select(x => x.id).ToList();
		for (int i = 0; i < wars.Count; i++) {
			War currentWar = (War)wars [i];
			Kingdom kingdomAtWarWith = currentWar.kingdom1;
			if (currentWar.kingdom1.id == currentlyShowingKingdom.id) {
				kingdomAtWarWith = currentWar.kingdom2;
			}

			List<Transform> allCurrentParents = Utilities.GetComponentsInDirectChildren<Transform>(kingdomCurrentEventsContentParent.gameObject).ToList();

			GameObject warParentGO = null;
			for (int j = 0; j < allCurrentParents.Count; j++) {
                WarEventListParent currWarParent = allCurrentParents[j].GetComponent<WarEventListParent>();
                if (currWarParent != null) {
                    War warOfCurrentObject = currWarParent.war;
                    if (allWarIDs.Contains(warOfCurrentObject.id)) {
                        if (allCurrentParents[j].name.Equals("WarParent-" + currentWar.id.ToString())) {
                            warParentGO = allCurrentParents[j].gameObject;
                            allCurrentParents.RemoveAt(j);
                            break;
                        }
                    } else {
                        ObjectPoolManager.Instance.DestroyObject(currWarParent.gameObject);
                    }
                    
                }
				
			}

			if (wars.Count <= 0) {
				if (warParentGO == null) {
                    ObjectPoolManager.Instance.DestroyObject(warParentGO);
				}
				return;
			}

			if (warParentGO == null) {
				warParentGO = InstantiateUIObject(kingdomWarEventsListParentPrefab.name, kingdomCurrentEventsContentParent.transform);
				warParentGO.name = "WarParent-" + currentWar.id.ToString();
				warParentGO.transform.localScale = Vector3.one;
				warParentGO.transform.localPosition = Vector3.zero;
				warParentGO.GetComponent<WarEventListParent>().SetWarEvent (currentWar, kingdomAtWarWith);
				warParentGO.GetComponent<WarEventListParent>().onClickEvent += ShowEventLogs;
			}
				
			currentWarParent = warParentGO.GetComponent<WarEventListParent> ();

			List<WarEventListItem> currentlyShowingCampaigns = currentWarParent.eventsGrid.GetChildList ().Select (x => x.GetComponent<WarEventListItem>()).ToList ();
			List<int> currentlyShowingCampaignIDs = currentlyShowingCampaigns.Select (x => x.campaign.id).ToList ();
			List<int> actualCampaignIDs = new List<int>();
			List<Campaign> campaignsToShow = new List<Campaign>();
			RepositionKingdomEventsTable();
		}
	}
	public void RepositionKingdomEventsTable(){
		StartCoroutine (RepositionTable (kingdomCurrentEventsContentParent));
	}
	/*
	 * Hide Kingdom Events Menu
	 * */
	public void HideAllKingdomEvents(){
		allKingdomEventsGO.SetActive (false);
		kingdomListEventButton.SetClickState(false);
	}

    public void ToggleKingdomCities() {
        if (kingdomCitiesGO.activeSelf) {
            HideKingdomCities();
        } else {
            ShowKingdomCities();
        }
    }

    Kingdom currentlyShowingKingdomCities;
    /*
     * Show all cities owned by currentlyShowingKingdom.
     * */
    public void ShowKingdomCities() {
        //HideKingdomHistory();
        HideRelationships();
        HideAllKingdomEvents();
        kingdomCitiesGrid.cellHeight = 123f; //Disable if not using for testing
        List<CityItem> cityItems = kingdomCitiesGrid.gameObject.GetComponentsInChildren<Transform>(true)
            .Where(x => x.GetComponent<CityItem>() != null)
            .Select(x => x.GetComponent<CityItem>()).ToList();
        int nextIndex = 0;
        for (int i = 0; i < cityItems.Count; i++) {
            CityItem currCityItem = cityItems[i];
            if(i < currentlyShowingKingdom.cities.Count) {
                City currCity = currentlyShowingKingdom.cities.ElementAtOrDefault(i);
                if (currCity != null) {
                    currCityItem.SetCity(currCity, true, false, true);
                    currCityItem.gameObject.SetActive(true);
                } else {
                    currCityItem.gameObject.SetActive(false);
                }
                nextIndex = i + 1;
            } else {
                currCityItem.gameObject.SetActive(false);
            }
        }

        if (currentlyShowingKingdom.cities.Count > nextIndex) {
            for (int i = nextIndex; i < currentlyShowingKingdom.cities.Count; i++) {
                City currCity = currentlyShowingKingdom.cities[i];
                GameObject cityGO = InstantiateUIObject(cityItemPrefab.name, this.transform);
                cityGO.GetComponent<CityItem>().SetCity(currCity, true, false, true);
                cityGO.transform.localScale = Vector3.one;
                kingdomCitiesGrid.AddChild(cityGO.transform);
                kingdomCitiesGrid.Reposition();
            }
            if (kingdomCitiesGO.activeSelf) {
                StartCoroutine(RepositionScrollView(kingdomCitiesScrollView, true));
            }
        }

        if (currentlyShowingKingdomCities == null || currentlyShowingKingdomCities.id != currentlyShowingKingdom.id || !kingdomCitiesGO.activeSelf) {
            StartCoroutine(RepositionGrid(kingdomCitiesGrid));
            StartCoroutine(RepositionScrollView(kingdomCitiesScrollView));
            kingdomCitiesScrollView.UpdateScrollbars();
        }

        kingdomCitiesGO.SetActive(true);
        currentlyShowingKingdomCities = currentlyShowingKingdom;
    }
    public void HideKingdomCities() {
        kingdomCitiesGO.SetActive(false);
        kingdomListCityButton.SetClickState(false);
    }

    public void ShowRelationshipSummary(Citizen citizen, string summary) {
        if(citizen.assignedRole is Governor) {
            relationshipSummaryTitleLbl.text = "Loyalty";
        } else if (citizen.assignedRole is King) {
            relationshipSummaryTitleLbl.text = "Affinity";
        }
        relationshipSummaryLbl.text = summary;
        relationshipSummaryGO.SetActive(true);
    }
    public void HideRelationshipSummary() {
        relationshipSummaryGO.SetActive(false);
    }

    public void ToggleInterveneMenu() {
        if (interveneMenuGO.activeSelf) {
            HideInterveneMenu();
        } else {
            ShowInterveneMenu();
        }
    }
    private void ShowInterveneMenu() {
        if(interveneMenuGrid.GetChildList().Count <= 0) {
            LoadInterveneEvents();
        }
        interveneMenuBtn.SetClickState(true);
        interveneMenuGO.SetActive(true);
    }
    private void LoadInterveneEvents() {
        for (int i = 0; i < EventManager.Instance.playerPlacableEvents.Length; i++) {
            EVENT_TYPES currEvent = EventManager.Instance.playerPlacableEvents[i];
            GameObject playerEventItemGO = InstantiateUIObject(playerEventItemPrefab.name, this.transform);
            interveneMenuGrid.AddChild(playerEventItemGO.transform);
            playerEventItemGO.transform.localPosition = Vector3.zero;
            playerEventItemGO.transform.localScale = Vector3.one;
            playerEventItemGO.GetComponent<PlayerEventItem>().SetEvent(currEvent);
        }
        StartCoroutine(RepositionGrid(interveneMenuGrid));
        StartCoroutine(RepositionScrollView(interveneMenuScrollView));
    }
    private void HideInterveneMenu() {
        interveneMenuBtn.SetClickState(false);
        interveneMenuGO.SetActive(false);
    }

    public void ToggleInterveneActionsMenu() {
        if (interveneActonsGO.activeSelf) {
            HideInterveneActionsMenu();
        } else {
            ShowInterveneActionsMenu();
        }
    }
    private void ShowInterveneActionsMenu() {
        interveneActonsGO.SetActive(true);
    }
    public void HideInterveneActionsMenu() {
        HideSwitchKingdomsMenu();
        HideCreateKingdomMenu();
        interveneMenuBtn.SetClickState(false);
        interveneActonsGO.SetActive(false);
    }

    public void ToggleSwitchKingdomsMenu() {
        if (switchKingdomGO.activeSelf) {
            HideSwitchKingdomsMenu();
        } else {
            ShowSwitchKingdomsMenu();
        }
    }
    private void ShowSwitchKingdomsMenu() {
        HideCreateKingdomMenu();
        //Load Kingdoms
        List<Kingdom> kingdomsToList = KingdomManager.Instance.allKingdoms.Where(x => x.id != currentlyShowingKingdom.id).ToList();

        List<KingdomInterveneItem> presentKingdomItems = switchKingdomGrid.GetChildList().Select(x => x.GetComponent<KingdomInterveneItem>()).ToList();
        if(kingdomsToList.Count > presentKingdomItems.Count) {
            int numOfItemsToCreate = kingdomsToList.Count - presentKingdomItems.Count;
            for (int i = 0; i < numOfItemsToCreate; i++) {
                GameObject kingdomItemGO = InstantiateUIObject(kingdomIntervenePrefab.name, switchKingdomGrid.transform);
                switchKingdomGrid.AddChild(kingdomItemGO.transform);
                kingdomItemGO.transform.localScale = Vector3.one;
                presentKingdomItems.Add(kingdomItemGO.GetComponent<KingdomInterveneItem>());
            }
            StartCoroutine(RepositionGrid(switchKingdomGrid));
        }

        for (int i = 0; i < presentKingdomItems.Count; i++) {
            KingdomInterveneItem currItem = presentKingdomItems[i];
            Kingdom currKingdom = kingdomsToList.ElementAtOrDefault(i);
            if (currKingdom != null) {
                currItem.SetKingdom(currKingdom);
                currItem.gameObject.SetActive(true);
            } else {
                currItem.gameObject.SetActive(false);
            }
        }
        switchKingdomsBtn.SetClickState(true);
        switchKingdomGO.SetActive(true);
    }
    public void HideSwitchKingdomsMenu() {
        switchKingdomsBtn.SetClickState(false);
        switchKingdomGO.SetActive(false);
    }

    public void ToggleCreateKingdomMenu() {
        if (createKingdomGO.activeSelf) {
            HideCreateKingdomMenu();
        } else {
            ShowCreateKingdomMenu();
        }
    }
    private void ShowCreateKingdomMenu() {
        HideSwitchKingdomsMenu();
        createKingdomPopupList.Clear();
        RACE[] allRaces = Utilities.GetEnumValues<RACE>().Where(x => x != RACE.NONE).ToArray();
        for (int i = 0; i < allRaces.Length; i++) {
            createKingdomPopupList.AddItem(allRaces[i].ToString(), allRaces[i]);
        }
        createKingdomPopupList.value = createKingdomPopupList.items.First();
        createKingdomRaceSelectedLbl.text = createKingdomPopupList.value;
        UpdateCreateKingdomErrorMessage();

        createKingdomBtn.SetClickState(true);
        createKingdomGO.SetActive(true);
    }
    List<HexTile> tilesToChooseForNewKingdom;
    public void UpdateCreateKingdomErrorMessage() {
        List<HexTile> elligibleTilesForNewKingdom = CityGenerator.Instance.GetHabitableTilesForRace((RACE)createKingdomPopupList.data);
        BIOMES forbiddenBiomeForRace = CityGenerator.Instance.GetForbiddenBiomeOfRace((RACE)createKingdomPopupList.data);
        elligibleTilesForNewKingdom = elligibleTilesForNewKingdom.Where(x => x.biomeType != forbiddenBiomeForRace && !x.isBorder && !x.isOccupied).ToList();

        tilesToChooseForNewKingdom = new List<HexTile>();

        for (int i = 0; i < elligibleTilesForNewKingdom.Count; i++) {
            HexTile currTile = elligibleTilesForNewKingdom[i];
            List<HexTile> invalidNeighbours = currTile.GetTilesInRange(3).Where(x => x.isBorder || x.isOccupied).ToList();
            if (invalidNeighbours.Count <= 0) {
                tilesToChooseForNewKingdom.Add(currTile);
            }
        }

        if (tilesToChooseForNewKingdom.Count <= 0) {
            createKingdomErrorLbl.text = "Cannot create kingdom for that race!";
            createKingdomErrorLbl.gameObject.SetActive(true);
            createKingdomExecuteBtn.GetComponent<BoxCollider>().enabled = false;
            createKingdomExecuteBtn.SetState(UIButtonColor.State.Disabled, true);
        } else {
            createKingdomErrorLbl.gameObject.SetActive(false);
            createKingdomExecuteBtn.GetComponent<BoxCollider>().enabled = true;
            createKingdomExecuteBtn.SetState(UIButtonColor.State.Normal, true);
        }
    }
    public void CreateNewKingdom() {
        List<HexTile> citiesForNewKingdom = new List<HexTile>() { tilesToChooseForNewKingdom[UnityEngine.Random.Range(0, tilesToChooseForNewKingdom.Count)] };
        Kingdom newKingdom = KingdomManager.Instance.GenerateNewKingdom((RACE)createKingdomPopupList.data, citiesForNewKingdom, true);
        newKingdom.HighlightAllOwnedTilesInKingdom();
        //newKingdom.king.CreateInitialRelationshipsToKings();
        //KingdomManager.Instance.AddRelationshipToOtherKings(newKingdom.king);

        HideInterveneActionsMenu();
    }
    private void HideCreateKingdomMenu() {
        createKingdomBtn.SetClickState(false);
        createKingdomGO.SetActive(false);
    }

    public void UpdatePrestigeSummary() {
        prestigeSummaryLbl.text = string.Empty;
        List<Kingdom> kingdomsToShow = new List<Kingdom>(KingdomManager.Instance.allKingdomsOrderedByPrestige);
        kingdomsToShow.Reverse();
        for (int i = 0; i < kingdomsToShow.Count; i++) {
            Kingdom currKingdom = kingdomsToShow[i];
            prestigeSummaryLbl.text += currKingdom.name + " - " + currKingdom.prestige.ToString() + " (" + currKingdom.cityCap + ")" + 
                " P: " + currKingdom.effectivePower.ToString() + " D: " + currKingdom.effectiveDefense.ToString();
            if(i + 1 < KingdomManager.Instance.allKingdomsOrderedByPrestige.Count) {
                prestigeSummaryLbl.text += "\n";
            }
        }
    }
	public void UpdateAllianceSummary() {
		if(UIManager.Instance.goAlliance.activeSelf){
			this.allianceSummaryLbl.text = string.Empty;
			if (warAllianceState == "alliance") {
				for (int i = 0; i < KingdomManager.Instance.alliances.Count; i++) {
					AlliancePool alliance = KingdomManager.Instance.alliances [i];
					if (i != 0) {
						this.allianceSummaryLbl.text += "\n";
					}
					this.allianceSummaryLbl.text += alliance.name;
					for (int j = 0; j < alliance.kingdomsInvolved.Count; j++) {
						Kingdom kingdom = alliance.kingdomsInvolved [j];
						this.allianceSummaryLbl.text += "\n- " + kingdom.name;
					}
				}
			} else if (warAllianceState == "warfare") {
				for (int i = 0; i < KingdomManager.Instance.kingdomWars.Count; i++) {
					Warfare warfare = KingdomManager.Instance.kingdomWars [i];
					if (i != 0) {
						this.allianceSummaryLbl.text += "\n";
					}
					this.allianceSummaryLbl.text += "War " + warfare.id;
					for (int j = 0; j < warfare.battles.Count; j++) {
						if(warfare.battles[j].attackCity != null && warfare.battles[j].defenderCity != null){
							this.allianceSummaryLbl.text += "\n- " + warfare.battles[j].attackCity.name + " -> " + warfare.battles[j].defenderCity.name + " (" 
								+ ((MONTH)warfare.battles[j].supposedAttackDate.month).ToString() + " " + warfare.battles[j].supposedAttackDate.day.ToString() + ", " + warfare.battles[j].supposedAttackDate.year.ToString() + ")";
						}
					}
				}
			}

		}
	}
    /*
	 * Generic toggle function, toggles gameobject to on/off state.
	 * */
    public void ToggleObject(GameObject objectToToggle){
		objectToToggle.SetActive(!objectToToggle.activeSelf);
	}

	/*
	 * Get the corresponding icon for each event
	 * */
	internal Sprite GetSpriteForEvent(EVENT_TYPES eventType){
		switch (eventType) {
		case EVENT_TYPES.ASSASSINATION:
			return assassinationIcon;
		case EVENT_TYPES.BORDER_CONFLICT:
			return borderConflictIcon;
		case EVENT_TYPES.EXPANSION:
			return expansionIcon;
		case EVENT_TYPES.INVASION_PLAN:
			return invasionPlotIcon;
		case EVENT_TYPES.KINGDOM_WAR:
			return declareWarIcon;
		case EVENT_TYPES.MARRIAGE_INVITATION:
			return marriageInvitationIcon;
		case EVENT_TYPES.MILITARIZATION:
			return militarizationIcon;
		case EVENT_TYPES.RAID:
			return raidIcon;
		case EVENT_TYPES.REBELLION_PLOT:
			return rebellionPlotIcon;
		case EVENT_TYPES.REQUEST_PEACE:
			return requestPeaceIcon;
		case EVENT_TYPES.STATE_VISIT:
			return stateVisitIcon;
		case EVENT_TYPES.DIPLOMATIC_CRISIS:
			return rebellionPlotIcon;
		case EVENT_TYPES.ADMIRATION:
			return requestPeaceIcon;
		case EVENT_TYPES.RIOT:
			return rebellionPlotIcon;
		case EVENT_TYPES.SECESSION:
			return rebellionPlotIcon;
		case EVENT_TYPES.REBELLION:
			return rebellionPlotIcon;
		case EVENT_TYPES.PLAGUE:
			return militarizationIcon;
		case EVENT_TYPES.BOON_OF_POWER:
			return marriageInvitationIcon;
		case EVENT_TYPES.TRADE:
			return marriageInvitationIcon;
		case EVENT_TYPES.PROVOCATION:
			return rebellionPlotIcon;
		case EVENT_TYPES.EVANGELISM:
			return requestPeaceIcon;
		case EVENT_TYPES.SPOUSE_ABDUCTION:
			return militarizationIcon;
		case EVENT_TYPES.FIRST_AND_KEYSTONE:
			return marriageInvitationIcon;
		case EVENT_TYPES.RUMOR:
			return militarizationIcon;
		case EVENT_TYPES.SLAVES_MERCHANT:
			return rebellionPlotIcon;
		case EVENT_TYPES.HIDDEN_HISTORY_BOOK:
			return requestPeaceIcon;
		case EVENT_TYPES.SERUM_OF_ALACRITY:
			return assassinationIcon;
		case EVENT_TYPES.ALTAR_OF_BLESSING:
			return requestPeaceIcon;
        case EVENT_TYPES.DEVELOP_WEAPONS:
            return assassinationIcon;
        case EVENT_TYPES.HYPNOTISM:
            return militarizationIcon;
        case EVENT_TYPES.KINGS_COUNCIL:
            return stateVisitIcon;
        case EVENT_TYPES.KINGDOM_HOLIDAY:
            return requestPeaceIcon;
        case EVENT_TYPES.ADVENTURE:
            return raidIcon;
        case EVENT_TYPES.EVIL_INTENT:
            return assassinationIcon;
		case EVENT_TYPES.GREAT_STORM:
			return assassinationIcon;
		case EVENT_TYPES.ANCIENT_RUIN:
			return marriageInvitationIcon;
        }
		return assassinationIcon;
	}

	/*
	 * Checker for if the mouse is currently
	 * over a UI Object
	 * */
	public bool IsMouseOnUI(){
        if (uiCamera != null) {
            if (Minimap.Instance.isDragging) {
                return true;
            }
            if (UICamera.hoveredObject != null && UICamera.hoveredObject.layer == LayerMask.NameToLayer("UI")) {
                return true;
            }
        }
        return false;
	}

	/*
	 * Use this to instantiate UI Objects, so that the program can normalize it's
	 * font sizes.
	 * */
	internal GameObject InstantiateUIObject(string prefabObjName, Transform parent){
        //GameObject go = GameObject.Instantiate (prefabObj, parent) as GameObject;
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(prefabObjName, Vector3.zero, Quaternion.identity, parent);
		UILabel[] goLbls = go.GetComponentsInChildren<UILabel>(true);
		for (int i = 0; i < goLbls.Length; i++) {
			NormalizeFontSizeOfLabel(goLbls[i]);
		}
		return go;
	}





//------------------------------------------------------------------------------------------- FOR TESTING ---------------------------------------------------------------------

    public void ToggleBorders() {
        CameraMove.Instance.ToggleMainCameraLayer("Borders");
    }

	public void ToggleResourceIcons(){
		CameraMove.Instance.ToggleResourceIcons();
	}

	public void ToggleGeneralCamera(){
		CameraMove.Instance.ToggleGeneralCamera();
	}

	public void ToggleTraderCamera(){
		CameraMove.Instance.ToggleTraderCamera();
	}
	
    public void ForceAdventurer() {
        EventCreator.Instance.CreateAdventureEvent(currentlyShowingKingdom);
    }

	public void OnValueChangeEventDropdown(){
		eventDropdownCurrentSelectionLbl.text = this.eventDropdownList.value;
		if(this.eventDropdownList.value == "Raid"){
			goRaid.SetActive (true);
			goStateVisit.SetActive (false);
			goMarriageInvitation.SetActive (false);
			goPowerGrab.SetActive (false);
			goExpansion.SetActive (false);
			goInvasionPlan.SetActive (false);
		}else if(this.eventDropdownList.value == "State Visit"){
			goRaid.SetActive (false);
			goStateVisit.SetActive (true);
			goMarriageInvitation.SetActive (false);
			goPowerGrab.SetActive (false);
			goExpansion.SetActive (false);
			goInvasionPlan.SetActive (false);
		}else if(this.eventDropdownList.value == "Marriage Invitation"){
			goRaid.SetActive (false);
			goStateVisit.SetActive (false);
			goMarriageInvitation.SetActive (true);
			goPowerGrab.SetActive (false);
			goExpansion.SetActive (false);
			goInvasionPlan.SetActive (false);
		}else if(this.eventDropdownList.value == "Power Grab"){
			goRaid.SetActive (false);
			goStateVisit.SetActive (false);
			goMarriageInvitation.SetActive (false);
			goPowerGrab.SetActive (true);
			goExpansion.SetActive (false);
			goInvasionPlan.SetActive (false);
		}else if(this.eventDropdownList.value == "Expansion"){
			goRaid.SetActive (false);
			goStateVisit.SetActive (false);
			goMarriageInvitation.SetActive (false);
			goPowerGrab.SetActive (false);
			goExpansion.SetActive (true);
			goInvasionPlan.SetActive (false);
		}else if(this.eventDropdownList.value == "Invasion Plan"){
			goRaid.SetActive (false);
			goStateVisit.SetActive (false);
			goMarriageInvitation.SetActive (false);
			goPowerGrab.SetActive (false);
			goExpansion.SetActive (false);
			goInvasionPlan.SetActive (true);
		}
	}

	public void ShowCreateEventUI(){
		this.goCreateEventUI.SetActive (true);
	}
	public void HideCreateEventUI(){
		this.goCreateEventUI.SetActive (false);
	}

	#region For Testing
	public void ToggleRelocate(){
		if (this.relocateGO.activeSelf) {
			this.relocateGO.SetActive (false);
		} else {
			this.citiesForRelocationPopupList.Clear ();
			if(this.currentlyShowingCitizen != null){
				for(int i = 0; i < this.currentlyShowingCitizen.city.kingdom.cities.Count; i++){
					if(this.currentlyShowingCitizen.city.kingdom.cities[i].id != this.currentlyShowingCitizen.city.id){
						this.citiesForRelocationPopupList.AddItem (this.currentlyShowingCitizen.city.kingdom.cities [i].name, this.currentlyShowingCitizen.city.kingdom.cities [i]);
					}
				}
			}
			this.relocateGO.SetActive (true);
		}
	}
	public void OnClickOkRelocation(){
		if(this.citiesForRelocationPopupList.data != null){
			City newCityForCitizen = (City)this.citiesForRelocationPopupList.data;
			if(this.currentlyShowingCitizen != null){
//				Debug.LogError (this.currentlyShowingCitizen.name + " HAS MOVED FROM " + this.currentlyShowingCitizen.city.name + " TO " + newCityForCitizen.name);
				newCityForCitizen.MoveCitizenToThisCity (this.currentlyShowingCitizen, false);
			}
		}
	}
	public void HideRelocate(){
		this.relocateGO.SetActive (false);
	}
	public void ToggleForceWar(){
		if (this.forceWarGO.activeSelf) {
			this.forceWarGO.SetActive (false);
		} else {
			this.kingdomsForWar.Clear ();
			if(this.currentlyShowingCitizen != null){
				for(int i = 0; i < this.currentlyShowingCitizen.city.kingdom.relationships.Count; i++){
					if(!this.currentlyShowingCitizen.city.kingdom.relationships.ElementAt(i).Value.isAtWar){
						this.kingdomsForWar.AddItem (this.currentlyShowingCitizen.city.kingdom.relationships.ElementAt(i).Value.targetKingdom.name
                            , this.currentlyShowingCitizen.city.kingdom.relationships.ElementAt(i).Value.targetKingdom);
					}
				}
			}
			this.forceWarGO.SetActive (true);
		}
	}
	public void OnClickForceWar(){
		if(this.kingdomsForWar.data != null){
			Kingdom targetKingdom = (Kingdom)this.kingdomsForWar.data;
			if(this.currentlyShowingCitizen != null){
				//				Debug.LogError (this.currentlyShowingCitizen.name + " HAS MOVED FROM " + this.currentlyShowingCitizen.city.name + " TO " + newCityForCitizen.name);
				this.currentlyShowingCitizen.ForceWar(targetKingdom, null, WAR_TRIGGER.TARGET_GAINED_A_CITY);
			}
		}
	}
	public void HideForceWar(){
		this.forceWarGO.SetActive (false);
	}
	public void ToggleUnrest(){
		if (this.unrestGO.activeSelf) {
			this.unrestGO.SetActive (false);
		} else {
			if(this.currentlyShowingKingdom != null){
				this.unrestInput.value = this.currentlyShowingKingdom.happiness.ToString();
				this.unrestGO.SetActive (true);
			}
		}
	}
	public void OnChangeUnrest(){
		if(this.currentlyShowingKingdom != null){
			this.currentlyShowingKingdom.ChangeHappiness(int.Parse(this.unrestInput.value));
            UpdateKingdomInfo();
		}
	}
	public void HideUnrest(){
		this.unrestGO.SetActive (false);
	}
	public void OnClickBoonOfPower(){
		ShowInterveneEvent (EVENT_TYPES.BOON_OF_POWER);
	}
    public void GenerateChildForCitizen() {
        if (currentlyShowingCitizen.spouse == null) {
            //			Debug.Log ("Could not generate child because no spouse");
            return;
        }
        Citizen child = null;
        if (currentlyShowingCitizen.gender == GENDER.MALE) {
            child = MarriageManager.Instance.MakeBaby(currentlyShowingCitizen, currentlyShowingCitizen.spouse);
        } else {
            child = MarriageManager.Instance.MakeBaby(currentlyShowingCitizen.spouse, currentlyShowingCitizen);
        }
        currentlyShowingCitizen.city.RemoveCitizenFromCity(child);
        //		Debug.Log("====== " + child.name + " Behaviour Traits ======");
        //		for (int i = 0; i < child.behaviorTraits.Count; i++) {
        //			Debug.Log (child.behaviorTraits [i]);
        //		}
        //		Debug.Log("====== " + child.name + " Skill Traits ======");
        //		for (int i = 0; i < child.skillTraits.Count; i++) {
        //			Debug.Log (child.skillTraits [i]);
        //		}
        //		Debug.Log("====== " + child.name + " Misc Traits ======");
        //		for (int i = 0; i < child.miscTraits.Count; i++) {
        //			Debug.Log (child.miscTraits [i]);
        //		}
        currentlyShowingCitizen.children.Remove(child);
        currentlyShowingCitizen.spouse.children.Remove(child);
    }
	//private void ShowGovernorLoyalty(){
	//	if(!this.goLoyalty.activeSelf){
	//		this.goLoyalty.SetActive (true);
	//	}
	//}
	//public void HideGovernorLoyalty(){
	//	this.goLoyalty.SetActive (false);
	//}
 //   public void ChangeGovernorLoyalty() {
	//	((Governor)this.currentlyShowingCitizen.assignedRole).SetLoyalty(Int32.Parse(forTestingLoyaltyLbl.text));
	//	Debug.Log("Changed loyalty of: " + this.currentlyShowingCitizen.name + " to " + ((Governor)this.currentlyShowingCitizen.assignedRole).loyalty.ToString());
 //   }
    public void LogRelatives() {
        List<Citizen> allRelatives = currentlyShowingCitizen.GetRelatives(-1);
        Debug.Log("========== " + currentlyShowingCitizen.name + " Relatives ==========");
        for (int i = 0; i < allRelatives.Count; i++) {
            Debug.Log("Relative: " + allRelatives[i].name);
        }
    }
    public void ChangePrestige() {
        currentlyShowingKingdom.SetPrestige(Int32.Parse(kingdomPrestigeLbl.text));
    }
    public void ForceExpansion() {
        EventCreator.Instance.CreateExpansionEvent(currentlyShowingKingdom);
    }
	public void ToggleAlliance() {
		if(warAllianceState == "alliance"){
			this.goAlliance.SetActive (!this.goAlliance.activeSelf);
		}else{
			this.goAlliance.SetActive (true);
		}
		if(this.goAlliance.activeSelf){
			warAllianceState = "alliance";
			UpdateAllianceSummary ();
		}
	}
	public void ToggleWarfare() {
		if(warAllianceState == "warfare"){
			this.goAlliance.SetActive (!this.goAlliance.activeSelf);
		}else{
			this.goAlliance.SetActive (true);
		}
		if(this.goAlliance.activeSelf){
			warAllianceState = "warfare";
			UpdateAllianceSummary ();
		}
	}
    public void ForceKillCurrentCitizen() {
        currentlyShowingCitizen.Death(DEATH_REASONS.ACCIDENT);
        ShowCitizenInfo(currentlyShowingCitizen);
    }
    #endregion


    public void CenterCameraOnCitizen(){
		if(this.currentlyShowingCitizen != null){
			if(!this.currentlyShowingCitizen.isDead){
				CameraMove.Instance.CenterCameraOn (this.currentlyShowingCitizen.currentLocation.gameObject);
			}
		}
	}

	#region Intervene Events
	internal void ShowInterveneEvent(EVENT_TYPES eventType){
		WorldEventManager.Instance.currentInterveneEvent = eventType;
		switch(eventType){
		case EVENT_TYPES.PLAGUE:
			ShowIntervenePlagueEvent ();
			break;
		case EVENT_TYPES.BOON_OF_POWER:
			break;
        case EVENT_TYPES.LYCANTHROPY:
            ToggleLycanthropyMenu();
            break;
        case EVENT_TYPES.EVIL_INTENT:
            ToggleEvilIntentMenu();
            break;
		}
	}

    #region Plague
    private void ShowIntervenePlagueEvent(){
		ToggleForcePlague ();
	}
	public void ToggleForcePlague(){
		if (this.forcePlagueGO.activeSelf) {
			this.forcePlagueGO.SetActive (false);
		} else {
			if(!WorldEventManager.Instance.HasEventOfType(EVENT_TYPES.PLAGUE)){
				this.kingdomsForPlague.Clear ();
				this.kingdomsForPlague.AddItem ("RANDOM", null);
				for(int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++){
					if(KingdomManager.Instance.allKingdoms[i].cities.FirstOrDefault(x => x.structures.Count > 0) != null){
						this.kingdomsForPlague.AddItem (KingdomManager.Instance.allKingdoms[i].name, KingdomManager.Instance.allKingdoms[i]);
					}
				}
				this.forcePlagueGO.SetActive (true);
			}

		}
	}
	public void OnClickForcePlague(){
		Kingdom targetKingdom = null;
		if(this.kingdomsForPlague.data != null){
			targetKingdom = (Kingdom)this.kingdomsForPlague.data;
		}else{
			List<Kingdom> filteredKingdoms = new List<Kingdom> ();
			for(int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++){
				if(KingdomManager.Instance.allKingdoms[i].cities.FirstOrDefault(x => x.structures.Count > 0) != null){
					filteredKingdoms.Add (KingdomManager.Instance.allKingdoms[i]);
				}
			}
			if(filteredKingdoms != null && filteredKingdoms.Count > 0){
				targetKingdom = filteredKingdoms [UnityEngine.Random.Range (0, filteredKingdoms.Count)];
			}
		}
		if(targetKingdom != null){
			EventCreator.Instance.CreatePlagueEvent (targetKingdom);
		}
	}
    #endregion

    #region Lycanthropy
    [Space(10)]
    [Header("Lycanthropy Objects")]
    [SerializeField] private GameObject lycanthropyMenuGO;
    [SerializeField] private UIGrid lycanthropyMenuGrid;
    [SerializeField] private GameObject lycanthropySelectedGO;
    private Citizen lycanthropySelectedCitizen;

    private void ToggleLycanthropyMenu() {
        if (lycanthropyMenuGO.activeSelf) {
            HideLycanthropyMenu();
        } else {
            ShowInterveneLycanthropyEvent();
        }
    }

    private void ShowInterveneLycanthropyEvent() {
        int numOfCitizensToChooseFrom = 5;
        List<Citizen> allGovernors = KingdomManager.Instance.GetAllCitizensOfType(ROLE.GOVERNOR);
        List<Citizen> allGovernorRelatives = new List<Citizen>();
        //Get relatives of all the governors
        for (int i = 0; i < allGovernors.Count; i++) {
            Governor currGovernor = (Governor)allGovernors[i].assignedRole;
            List<Citizen> currGovernorRelatives = allGovernors[i].GetRelatives().Where(x => !x.isDead && x.role == ROLE.UNTRAINED).ToList();
            allGovernorRelatives = allGovernorRelatives.Union(currGovernorRelatives).ToList();
        }

        //Randomly choose a number of relatives
        List<Citizen> citizensToChooseFrom = new List<Citizen>();
        if (allGovernorRelatives.Count > 0) {
            for (int i = 0; i < numOfCitizensToChooseFrom; i++) {
                if(allGovernorRelatives.Count <= 0) {
                    break;
                }
                Citizen chosenCitizen = allGovernorRelatives[UnityEngine.Random.Range(0, allGovernorRelatives.Count)];
                citizensToChooseFrom.Add(chosenCitizen);
                allGovernorRelatives.Remove(chosenCitizen);
            }
        } else {
            Debug.LogError("No elligible citizens!");
            return;
        }
        List<CharacterPortrait> portraits = Utilities.GetComponentsInDirectChildren<CharacterPortrait>(lycanthropyMenuGrid.gameObject).ToList();

        List<CharacterPortrait> activePortraits = new List<CharacterPortrait>();
        //Display chosen citizens to player
        for (int i = 0; i < portraits.Count; i++) {
            CharacterPortrait currPortrait = portraits[i];
            Citizen currCitizen = null;
            try {
                currCitizen = citizensToChooseFrom[i];
            } catch {
                currPortrait.gameObject.SetActive(false);
                continue;
            }
            currPortrait.SetCitizen(currCitizen, false, true);
            currPortrait.onClickCharacterPortrait += SelectCitizenForLycanthropy;
            currPortrait.gameObject.SetActive(true);
            activePortraits.Add(currPortrait);
        }

        if (activePortraits.Count > 0) {
            SelectCitizenForLycanthropy(activePortraits.FirstOrDefault().citizen);
        }
        lycanthropyMenuGO.SetActive(true);
    }

    private void SelectCitizenForLycanthropy(Citizen citizen) {
        lycanthropySelectedCitizen = citizen;
        ShowCitizenInfo(citizen);
        CharacterPortrait charPortraitOfCitizen = null;
        List<CharacterPortrait> portraits = Utilities.GetComponentsInDirectChildren<CharacterPortrait>(lycanthropyMenuGrid.gameObject).ToList();
        for (int i = 0; i < portraits.Count; i++) {
            CharacterPortrait currPortrait = portraits[i];
            if(currPortrait.citizen.id == citizen.id) {
                charPortraitOfCitizen = currPortrait;
                break;
            }
        }

        lycanthropySelectedGO.transform.SetParent(charPortraitOfCitizen.transform);
        lycanthropySelectedGO.transform.localPosition = Vector3.zero;
        lycanthropySelectedGO.SetActive(true);
    }

    public void StartLycanthropyEvent() {
        Lycanthropy newLycanthropy = new Lycanthropy(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, null, lycanthropySelectedCitizen);
        HideLycanthropyMenu();
        HideInterveneMenu();
        if(currentlyShowingCitizen != null) {
            ShowCitizenInfo(currentlyShowingCitizen);
        }
        
    }

    public void HideLycanthropyMenu() {
        lycanthropyMenuGO.SetActive(false);
    }
    #endregion

    #region Evil Intent
    [Space(10)]
    [Header("Evil Intent Objects")]
    [SerializeField] private GameObject evilIntentMenuGO;
    [SerializeField] private GameObject evilIntentTargetKingParentGO;
    [SerializeField] private UIGrid evilIntentMenuSourceKingGrid;
    [SerializeField] private UIGrid evilIntentMenuTargetKingGrid;
    [SerializeField] private GameObject evilIntentSelectedSourceKingGO;
    [SerializeField] private GameObject evilIntentSelectedTargetKingGO;
    [SerializeField] private UIButton startEvilIntentBtn;

    private Citizen evilIntentSelectedSourceKing;
    private Citizen evilIntentSelectedTargetKing;

    private void ToggleEvilIntentMenu() {
        if (evilIntentMenuGO.activeSelf) {
            HideEvilIntentMenu();
        } else {
            ShowInterveneEvilIntentEvent();
        }
    }

    private void ShowInterveneEvilIntentEvent() {
        evilIntentSelectedSourceKing = null;
        evilIntentSelectedTargetKing = null;
        evilIntentTargetKingParentGO.SetActive(false);
        evilIntentSelectedSourceKingGO.SetActive(false);
        evilIntentSelectedTargetKingGO.SetActive(false);
        startEvilIntentBtn.GetComponent<BoxCollider>().enabled = false;

        List<Citizen> allKings = KingdomManager.Instance.allKingdoms.Select(x => x.king).ToList();
        List<CharacterPortrait> portraits = Utilities.GetComponentsInDirectChildren<CharacterPortrait>(evilIntentMenuSourceKingGrid.gameObject).ToList();

        if (allKings.Count > portraits.Count) {
            int numOfMissingPortraits = allKings.Count - portraits.Count;
            for (int i = 0; i < numOfMissingPortraits; i++) {
                GameObject newPortrait = InstantiateUIObject(characterPortraitPrefab.name, evilIntentMenuSourceKingGrid.transform) as GameObject;
                newPortrait.transform.localScale = Vector3.one;
                evilIntentMenuSourceKingGrid.AddChild(newPortrait.transform);
                StartCoroutine(RepositionGrid(evilIntentMenuSourceKingGrid));
                portraits.Add(newPortrait.GetComponent<CharacterPortrait>());
            }
        }

        for (int i = 0; i < portraits.Count; i++) {
            CharacterPortrait currPortrait = portraits[i];
            Citizen currKing;
            try {
                currKing = allKings[i];
                currPortrait.SetCitizen(currKing, false, true);
                currPortrait.onClickCharacterPortrait = null;
                currPortrait.onClickCharacterPortrait += SelectSourceKingForEvilIntent;
                currPortrait.gameObject.SetActive(true);
            } catch {
                currPortrait.gameObject.SetActive(false);
            }
        }

        evilIntentMenuGO.SetActive(true);
    }

    private void LoadEvilIntentTargetChoices() {
        List<Citizen> allOtherKings = evilIntentSelectedSourceKing.city.kingdom.discoveredKingdoms.Select(x => x.king).Where(x => x.id != evilIntentSelectedSourceKing.id).ToList();
        List<CharacterPortrait> portraits = Utilities.GetComponentsInDirectChildren<CharacterPortrait>(evilIntentMenuTargetKingGrid.gameObject).ToList();

        if (allOtherKings.Count > portraits.Count) {
            int numOfMissingPortraits = allOtherKings.Count - portraits.Count;
            for (int i = 0; i < numOfMissingPortraits; i++) {
                GameObject newPortrait = InstantiateUIObject(characterPortraitPrefab.name, evilIntentMenuTargetKingGrid.transform) as GameObject;
                newPortrait.transform.localScale = Vector3.one;
                evilIntentMenuTargetKingGrid.AddChild(newPortrait.transform);
                StartCoroutine(RepositionGrid(evilIntentMenuTargetKingGrid));
                portraits.Add(newPortrait.GetComponent<CharacterPortrait>());
            }
        }

        for (int i = 0; i < portraits.Count; i++) {
            CharacterPortrait currPortrait = portraits[i];
            Citizen currKing;
            try {
                currKing = allOtherKings[i];
                currPortrait.SetCitizen(currKing, false, true);
                currPortrait.onClickCharacterPortrait = null;
                currPortrait.onClickCharacterPortrait += SelectTargetKingForEvilIntent;
                currPortrait.gameObject.SetActive(true);
            } catch {
                currPortrait.gameObject.SetActive(false);
            }
        }

        evilIntentTargetKingParentGO.SetActive(true);
    }

    private void SelectSourceKingForEvilIntent(Citizen citizen) {
        evilIntentSelectedTargetKing = null;
        evilIntentSelectedTargetKingGO.SetActive(false);
        startEvilIntentBtn.GetComponent<BoxCollider>().enabled = false;

        evilIntentSelectedSourceKing = citizen;
        ShowCitizenInfo(citizen);
        CharacterPortrait charPortraitOfCitizen = null;
        List<CharacterPortrait> portraits = Utilities.GetComponentsInDirectChildren<CharacterPortrait>(evilIntentMenuSourceKingGrid.gameObject).ToList();
        for (int i = 0; i < portraits.Count; i++) {
            CharacterPortrait currPortrait = portraits[i];
            if (currPortrait.citizen.id == citizen.id) {
                charPortraitOfCitizen = currPortrait;
                break;
            }
        }

        evilIntentSelectedSourceKingGO.transform.SetParent(charPortraitOfCitizen.transform);
        evilIntentSelectedSourceKingGO.transform.localPosition = Vector3.zero;
        evilIntentSelectedSourceKingGO.SetActive(true);
        LoadEvilIntentTargetChoices();
    }

    private void SelectTargetKingForEvilIntent(Citizen citizen) {
        startEvilIntentBtn.GetComponent<BoxCollider>().enabled = true;
        startEvilIntentBtn.SetState(UIButtonColor.State.Normal, true);

        evilIntentSelectedTargetKing = citizen;
        ShowCitizenInfo(citizen);
        CharacterPortrait charPortraitOfCitizen = null;
        List<CharacterPortrait> portraits = Utilities.GetComponentsInDirectChildren<CharacterPortrait>(evilIntentMenuTargetKingGrid.gameObject).ToList();
        for (int i = 0; i < portraits.Count; i++) {
            CharacterPortrait currPortrait = portraits[i];
            if (currPortrait.citizen.id == citizen.id) {
                charPortraitOfCitizen = currPortrait;
                break;
            }
        }

        evilIntentSelectedTargetKingGO.transform.SetParent(charPortraitOfCitizen.transform);
        evilIntentSelectedTargetKingGO.transform.localPosition = Vector3.zero;
        evilIntentSelectedTargetKingGO.SetActive(true);

        evilIntentTargetKingParentGO.SetActive(true);
    }

    public void StartEvilIntentEvent() {
        EvilIntent newEvilIntent = new EvilIntent(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, null, evilIntentSelectedSourceKing, evilIntentSelectedTargetKing);
        HideEvilIntentMenu();
        HideInterveneMenu();
        if (currentlyShowingCitizen != null) {
            ShowCitizenInfo(currentlyShowingCitizen);
        }
    }

    public void HideEvilIntentMenu() {
        evilIntentMenuGO.SetActive(false);
    }
    #endregion

    #endregion
}
