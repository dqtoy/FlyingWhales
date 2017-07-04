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
	public GameObject traitPrefab;
	public GameObject gameEventPrefab;
	public GameObject kingdomEventsListParentPrefab;
	public GameObject kingdomWarEventsListParentPrefab;
	public GameObject kingdomEventsListItemPrefab;
	public GameObject kingdomWarEventsListItemPrefab;
	public GameObject kingdomFlagPrefab;
	public GameObject logItemPrefab;
    public GameObject cityItemPrefab;
    public GameObject resourceIconPrefab;
    public GameObject playerEventItemPrefab;

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
    //Trait Editor
    public GameObject traitEditorGO;
	public UIPopupList addTraitPopUpList;
	public UIPopupList removeTraitPopUpList;
	public UILabel addTraitChoiceLbl;
	public UILabel removeTraitChoiceLbl;
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
	public UIGrid citizenTraitsGrid;
	public UIGrid citizenHistoryGrid;
	public ButtonToggle characterValuesBtn;
	public ButtonToggle familyTreeBtn;
	public ButtonToggle citizenHistoryBtn;
    public GameObject characterValuesGO;
    public UILabel characterValuesLbl;
	public GameObject citizenInfoForTestingGO;

	[Space(10)] //City Info UI
//	public UILabel cityNameLbl;
//	public UILabel cityGovernorLbl;
//	public UILabel cityKingdomLbl;
//	public UILabel cityGoldLbl;
//	public UILabel cityStoneLbl;
//	public UILabel cityLumberLbl;
//	public UILabel cityManaStoneLbl;
//	public UILabel cityCobaltLbl;
//	public UILabel cityMithrilLbl;
//	public UILabel cityFoodLbl;
//	public UIGrid foodProducersGrid;
//	public UIGrid gatherersGrid;
//	public UIGrid minersGrid;
//	public UIGrid tradersGrid;
//	public UIGrid generalsGrid;
//	public UIGrid spiesGrid;
//	public UIGrid envoysGrid;
//	public UIGrid guardiansGrid;
//	public UIGrid untrainedGrid;
//	public UI2DSprite cityInfoCtizenPortraitBG;
//	public GameObject citizensBtn;
//	public GameObject cityInfoCitizensParent;
//	public GameObject cityInfoHistoryParent;
//	public UIGrid cityInfoHistoryGrid;
//	public GameObject cityInfoEventsParent;
//	public UIGrid cityInfoEventsGrid;
//	public GameObject noEventsGO;
	//public UI2DSprite cityInfoPortrait;
	//public UILabel cityInfoCityNameLbl;
	//public CharacterPortrait cityInfoGovernorPortrait;
	//public UIGrid cityInfoCitizenGrid;
	//public UILabel cityInfoGoldLbl;
	//public UILabel cityInfoBasicLbl;
	//public UI2DSprite cityInfoBasicResource;
	//public GameObject cityInfoManaStoneIcon;
	//public GameObject cityInfoCobaltIcon;
	//public GameObject cityInfoMithrilIcon;
	//public GameObject cityInfoStoneIcon;
	//public GameObject cityInfoLumberIcon;
	


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
    public UILabel kingdomGoldLbl;
	public UILabel kingdomTechLbl;
	public UIProgressBar kingdomTechMeter;
    public UI2DSprite kingdomBasicResourceSprite;
    public UILabel kingdomBasicResourceLbl;
    public UIGrid kingdomOtherResourcesGrid;
    public UIGrid kingdomTradeResourcesGrid;
	public CharacterPortrait kingdomListActiveKing;
	public UIGrid kingdomListOtherKingdomsGrid;
	public ButtonToggle kingdomListEventButton;
	public ButtonToggle kingdomListRelationshipButton;
	public ButtonToggle kingdomListCityButton;
    public Sprite stoneSprite;
    public Sprite lumberSprite;

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

    [Space(10)]
    [Header("Minimap")]
    [SerializeField] private GameObject minimapGO;

    private List<MarriedCouple> marriageHistoryOfCurrentCitizen;
	private int currentMarriageHistoryIndex;
	internal Citizen currentlyShowingCitizen = null;
	internal City currentlyShowingCity = null;
	internal Kingdom currentlyShowingKingdom = null;
	private GameEvent currentlyShowingEvent;
	private RelationshipKings currentlyShowingRelationship;
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
	public UIPopupList eventDropdownList;
	public UILabel eventDropdownCurrentSelectionLbl;
    public UILabel forTestingLoyaltyLbl;

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

    void Awake(){
		Instance = this;
    }

	void Start(){
        EventManager.Instance.onUpdateUI.AddListener(UpdateUI);
        EventManager.Instance.onCreateNewKingdomEvent.AddListener(AddKingdomToList);
        EventManager.Instance.onKingdomDiedEvent.AddListener(RemoveKingdomFromList);
        NormalizeFontSizes();
        LoadKingdomList();
        UpdateUI();
	}

	private void NormalizeFontSizes(){
		UILabel[] allLabels = this.GetComponentsInChildren<UILabel>(true);
		Debug.Log ("ALL LABELS COUNT: " + allLabels.Length.ToString());
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

    public void MinimapClick() {
        BoxCollider bc = minimapGO.GetComponent<BoxCollider>();
        Vector3 pt = bc.transform.TransformPoint(bc.center);
        float width = minimapGO.transform.localScale.x;
        float height = minimapGO.transform.localScale.y;

        pt = UICamera.currentCamera.WorldToScreenPoint(pt);

        Rect miniMapRect = new Rect(pt.x - width / 2, pt.y - height / 2, width, height);
        //var miniMapRect = minimapGO.GetComponent<RectTransform>().rect;
        var screenRect = new Rect(
            minimapGO.transform.position.x,
            minimapGO.transform.position.y,
            miniMapRect.width, miniMapRect.height);

        var mousePos = Input.mousePosition;
        mousePos.y -= screenRect.y;
        mousePos.x -= screenRect.x;

        var camPos = new Vector3(
            mousePos.x * (GridMap.Instance.width / screenRect.width),
            mousePos.y * (GridMap.Instance.height / screenRect.height),
            Camera.main.transform.position.z);
        Camera.main.transform.position = camPos;
    }

    private void UpdateUI(){
		dateLbl.text = LocalizationManager.Instance.GetLocalizedValue("General", "Months", ((MONTH)GameManager.Instance.month).ToString()) + " " + GameManager.Instance.days.ToString () + ", " + GameManager.Instance.year.ToString ();
		
        if(currentlyShowingKingdom != null) {
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

        //if (citizenInfoGO.activeSelf) {
        //    if (currentlyShowingCitizen != null) {
        //        ShowCitizenInfo(currentlyShowingCitizen);
        //    }
        //}
	}

	public void SetProgressionSpeed1X(){
		Unpause ();
		GameManager.Instance.SetProgressionSpeed(2f);
		x1Btn.SetAsClicked();
	}

	public void SetProgressionSpeed2X(){
		Unpause ();
		GameManager.Instance.SetProgressionSpeed(1f);
		x2Btn.SetAsClicked();
	}

	public void SetProgressionSpeed4X(){
		Unpause ();
		GameManager.Instance.SetProgressionSpeed(0.3f);
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


    private void ForceUpdateKingdomList(Kingdom kingdomThatDied) {
        if (currentlyShowingKingdom.id == kingdomThatDied.id) {
            //SetKingdomAsActive(KingdomManager.Instance.allKingdoms.First());
            currentlyShowingKingdom = null;
        }
        LoadKingdomList();
    }

    private void RemoveKingdomFromList(Kingdom kingdomToRemove) {
        List<KingdomFlagItem> allKingdomsInGrid = kingdomListOtherKingdomsGrid.GetChildList()
                .Select(x => x.GetComponent<KingdomFlagItem>()).ToList();
        for (int i = 0; i < allKingdomsInGrid.Count; i++) {
            if (allKingdomsInGrid[i].kingdom.id == kingdomToRemove.id) {
                kingdomListOtherKingdomsGrid.RemoveChild(allKingdomsInGrid[i].transform);
                Destroy(allKingdomsInGrid[i].gameObject);
                break;
            }
        }
        RepositionGridCallback(kingdomListOtherKingdomsGrid);
		kingdomListOtherKingdomsGrid.GetChildList().FirstOrDefault().GetComponent<KingdomFlagItem>().SetAsSelected();
    }

    private void AddKingdomToList(Kingdom kingdomToAdd) {
        GameObject kingdomGO = InstantiateUIObject(kingdomFlagPrefab, this.transform);
        kingdomGO.GetComponent<KingdomFlagItem>().SetKingdom(kingdomToAdd);
        //kingdomGO.GetComponent<KingdomFlagItem>().onHoverOver += currKingdom.HighlightAllOwnedTilesInKingdom;
        //kingdomGO.GetComponent<KingdomFlagItem>().onHoverExit += currKingdom.UnHighlightAllOwnedTilesInKingdom;
        
        kingdomListOtherKingdomsGrid.AddChild(kingdomGO.transform);
		kingdomGO.transform.localScale = Vector3.one;
        kingdomListOtherKingdomsGrid.Reposition();

		kingdomListOtherKingdomsGrid.GetChildList().FirstOrDefault().GetComponent<KingdomFlagItem>().SetAsSelected();
    }

    private void LoadKingdomList(){
        for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
            Kingdom currKingdom = KingdomManager.Instance.allKingdoms[i];
            AddKingdomToList(currKingdom);
        }
        kingdomListOtherKingdomsGrid.Reposition();
 
        if (currentlyShowingKingdom == null) {
			kingdomListOtherKingdomsGrid.GetChildList().FirstOrDefault().GetComponent<KingdomFlagItem>().SetAsSelected();
            return;
            //currentlyShowingKingdom = KingdomManager.Instance.allKingdoms.First();
		}
	}

    private void UpdateKingdomInfo() {
        kingdomListActiveKing.SetCitizen(currentlyShowingKingdom.king); //King

        kingdomNameLbl.text = currentlyShowingKingdom.name; //Kingdom Name
        kingdomUnrestLbl.text = currentlyShowingKingdom.unrest.ToString(); //Unrest
        kingdomGoldLbl.text = currentlyShowingKingdom.goldCount.ToString() + "/" + currentlyShowingKingdom.maxGold.ToString(); //Gold
		kingdomTechLbl.text = currentlyShowingKingdom.techLevel.ToString(); //Tech
		kingdomTechMeter.value = (float)currentlyShowingKingdom.techCounter / (float)currentlyShowingKingdom.techCapacity;
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
        if (resourcesInGrid.Except(allOtherResources).Count() > 0 || allOtherResources.Except(resourcesInGrid).Count() > 0) {
            for (int i = 0; i < children.Count; i++) {
                kingdomOtherResourcesGrid.RemoveChild(children[i]);
                Destroy(children[i].gameObject);
            }
            for (int i = 0; i < currentlyShowingKingdom.availableResources.Keys.Count; i++) {
                RESOURCE currResource = currentlyShowingKingdom.availableResources.Keys.ElementAt(i);
                GameObject resourceGO = InstantiateUIObject(resourceIconPrefab, this.transform);
                resourceGO.GetComponent<ResourceIcon>().SetResource(currResource);
                resourceGO.transform.localScale = Vector3.one;
                kingdomOtherResourcesGrid.AddChild(resourceGO.transform);
                kingdomOtherResourcesGrid.Reposition();
            }
            RepositionGridCallback(kingdomOtherResourcesGrid);
        }

        //Trade Resources
        //children = kingdomTradeResourcesGrid.GetChildList();
        //resourcesInGrid = new List<RESOURCE>();
        //for (int i = 0; i < children.Count; i++) {
        //    RESOURCE resource = (RESOURCE)Enum.Parse(typeof(RESOURCE), Utilities.GetComponentsInDirectChildren<UI2DSprite>(children[i].gameObject).First().sprite2D.name);
        //    resourcesInGrid.Add(resource);
        //}

        //allOtherResources = new List<RESOURCE>();
        //for (int i = 0; i < currentlyShowingKingdom.tradeRoutes.Count; i++) {
        //    TradeRoute currTradeRoute = currentlyShowingKingdom.tradeRoutes[i];
        //    if (currTradeRoute.targetKingdom.id == currentlyShowingKingdom.id) {
        //        allOtherResources.Add(currTradeRoute.resourceBeingTraded);
        //    }
        //}

        //if (resourcesInGrid.Except(allOtherResources).Count() > 0 || allOtherResources.Except(resourcesInGrid).Count() > 0) {
        //    for (int i = 0; i < children.Count; i++) {
        //        kingdomTradeResourcesGrid.RemoveChild(children[i]);
        //        Destroy(children[i].gameObject);
        //    }
        //    for (int i = 0; i < currentlyShowingKingdom.tradeRoutes.Count; i++) {
        //        TradeRoute currTradeRoute = currentlyShowingKingdom.tradeRoutes[i];
        //        if (currTradeRoute.targetKingdom.id == currentlyShowingKingdom.id) {
        //            GameObject resourceGO = InstantiateUIObject(resourceIconPrefab, this.transform);
        //            Utilities.GetComponentsInDirectChildren<UI2DSprite>(resourceGO).First().sprite2D = Resources
        //                .LoadAll<Sprite>("Resources Icons")
        //                .Where(x => x.name == currTradeRoute.resourceBeingTraded.ToString()).ToList()[0];
        //            resourceGO.transform.localScale = Vector3.one;
        //            kingdomTradeResourcesGrid.AddChild(resourceGO.transform);
        //            kingdomTradeResourcesGrid.Reposition();
        //        }
        //    }
        //    StartCoroutine(RepositionGrid(kingdomTradeResourcesGrid));
        //}
        currentlyShowingKingdom.HighlightAllOwnedTilesInKingdom();
    }

    internal void SetKingdomAsSelected(Kingdom kingdom) {
        List<KingdomFlagItem> allKingdomsInGrid = kingdomListOtherKingdomsGrid.GetChildList()
                .Select(x => x.GetComponent<KingdomFlagItem>()).ToList();
        for (int i = 0; i < allKingdomsInGrid.Count; i++) {
            if (allKingdomsInGrid[i].kingdom.id == kingdom.id) {
                allKingdomsInGrid[i].SetAsSelected();
                break;
            }
        }
    }

	internal void SetKingdomAsActive(Kingdom kingdom){
        if(currentlyShowingKingdom != null && currentlyShowingKingdom.id != kingdom.id) {
		    currentlyShowingKingdom.UnHighlightAllOwnedTilesInKingdom ();
            List<KingdomFlagItem> children = kingdomListOtherKingdomsGrid.GetChildList()
                .Select(x => x.GetComponent<KingdomFlagItem>()).ToList();
            for (int i = 0; i < children.Count; i++) {
                KingdomFlagItem kfi = children[i];
                if(kfi.kingdom.id == currentlyShowingKingdom.id) {
                    kfi.PlayAnimationReverse();
                    break;
                }
            }
        }
		if (currentlyShowingCity != null) {
			currentlyShowingCity.HighlightAllOwnedTiles(204f/255f);
		}
		currentlyShowingKingdom = kingdom;
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
		if (traitEditorGO.activeSelf) {
			HideTraitEditor();
		}
		if (relocateGO.activeSelf) {
			HideRelocate();
		}
        if (characterValuesGO.activeSelf) {
            ShowCitizenCharacterValues();
        }

		//ForTesting
		citizenInfoForTestingGO.SetActive (true);

		HideSmallInfo();

		citizenNameLbl.text = currentlyShowingCitizen.name;
		string role = "Citizen";
		if (currentlyShowingCitizen.role != ROLE.UNTRAINED) {
			role = currentlyShowingCitizen.role.ToString();
		}
		if (currentlyShowingCitizen.city != null) {
            if(currentlyShowingCitizen.role == ROLE.GOVERNOR) {
                citizenRoleAndKingdomLbl.text = role + " of " + currentlyShowingCitizen.city.name;
            } else {
                citizenRoleAndKingdomLbl.text = role + " of " + currentlyShowingCitizen.city.kingdom.name;
            }
			citizenCityNameLbl.text = currentlyShowingCitizen.city.name;
			ctizenPortraitBG.color = currentlyShowingCitizen.city.kingdom.kingdomColor;
		} else {
			citizenRoleAndKingdomLbl.text = role;
			citizenCityNameLbl.text = "No City";
			ctizenPortraitBG.color = Color.white;
		}

		citizenAgeLbl.text = "Age: " + currentlyShowingCitizen.age.ToString();

		if (currentlyShowingCitizen.isDead) {
			citizenInfoIsDeadIcon.SetActive (true);
		} else {
			citizenInfoIsDeadIcon.SetActive (false);
		}


		List<TraitObject> traits = citizenTraitsGrid.GetChildList().Select(x => x.GetComponent<TraitObject>()).ToList();
		for (int i = 0; i < traits.Count; i++) {
			TRAIT traitToUse = TRAIT.NONE;
			if (i == 0) {
				traitToUse = currentlyShowingCitizen.honestyTrait;
			} else if (i == 1) {
				traitToUse = currentlyShowingCitizen.hostilityTrait;
			} else if (i == 2) {
				traitToUse = currentlyShowingCitizen.miscTrait;
			}
			traits [i].SetTrait (traitToUse);
		}

		StartCoroutine (RepositionGrid (citizenTraitsGrid));

		//if (citizenToShow.isKing) {
		//	characterValuesBtn.gameObject.SetActive(true);
		//} else {
  //          characterValuesBtn.gameObject.SetActive(false);
		//}

		HideCityInfo();
		citizenInfoGO.SetActive (true);
		this.marriageHistoryOfCurrentCitizen = MarriageManager.Instance.GetCouplesCitizenInvoledIn(citizenToShow);
	}

	public void HideCitizenInfo(){
		currentlyShowingCitizen = null;
		citizenInfoGO.SetActive(false);
		HideFamilyTree();
        HideCitizenCharacterValues();
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
			Destroy (children [i].gameObject);
		}

		for (int i = 0; i < this.currentlyShowingCitizen.history.Count; i++) {
			GameObject citizenGO = InstantiateUIObject(this.historyPortraitPrefab, this.citizenHistoryGrid.transform);
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

//	public void ShowCityInfo(City cityToShow, bool showCitizens = false){
//        HideCitizenInfo();
//        if (currentlyShowingCity != null) {
//			//unhighlight previously selected city
//			currentlyShowingCity.UnHighlightAllOwnedTiles ();
//		}
//		if(cityToShow == null){
//			return;
//		}
//		currentlyShowingCity = cityToShow;

//		if (currentlyShowingKingdom != null) {
//			currentlyShowingKingdom.HighlightAllOwnedTilesInKingdom ();
//		}

//		//Show Basic City Info
//		cityInfoCityNameLbl.text = currentlyShowingCity.name;
//		cityInfoPortrait.color = currentlyShowingCity.kingdom.kingdomColor;

//		//Show Governor
//		cityInfoGovernorPortrait.SetCitizen (currentlyShowingCity.governor);

//		//Show Citizens
//		List<Transform> gridChildren = cityInfoCitizenGrid.GetChildList();
//		List<Citizen> citizensInCity = currentlyShowingCity.citizens.Where(x => x.role != ROLE.GOVERNOR && x.role != ROLE.KING && x.role != ROLE.UNTRAINED).ToList();

//		List<int> currentlyShowingCitizens = gridChildren.Select (x => x.GetComponent<CharacterPortrait>().citizen.id).ToList();
//		List<int> citizens = citizensInCity.Select(x => x.id).ToList();
//		if (citizens.Except(currentlyShowingCitizens).Union(currentlyShowingCitizens.Except(citizens)).ToList().Count > 0) {
//			for (int i = 0; i < gridChildren.Count; i++) {
//				cityInfoCitizenGrid.RemoveChild(gridChildren[i]);
//				Destroy(gridChildren[i].gameObject);
//			}
//			for (int i = 0; i < citizensInCity.Count; i++) {
//				GameObject citizenGO = InstantiateUIObject(characterPortraitPrefab, this.transform);
//				citizenGO.GetComponent<CharacterPortrait> ().SetCitizen (citizensInCity[i]);
//				citizenGO.transform.localScale = Vector3.one;
//				cityInfoCitizenGrid.AddChild (citizenGO.transform);
//			}
//		}
//		cityInfoCitizenGrid.transform.parent.GetComponent<UIScrollView>().ResetPosition();
////		StartCoroutine(RepositionGrid(cityInfoCitizenGrid));

//		//Show City Resources
//		//Gold
////		cityInfoGoldLbl.text = currentlyShowingCity.goldCount.ToString() + "/" + currentlyShowingCity.MAX_GOLD.ToString();

////		//Basic Resource
////		if (currentlyShowingCity.kingdom.basicResource == BASE_RESOURCE_TYPE.STONE) {
////			cityInfoBasicResource.sprite2D = stoneSprite;
////			cityInfoBasicLbl.text = currentlyShowingCity.totalCitizenConsumption.ToString() + "/" + currentlyShowingCity.stoneCount.ToString();
////		} else if (currentlyShowingCity.kingdom.basicResource == BASE_RESOURCE_TYPE.WOOD) {
////			cityInfoBasicResource.sprite2D = lumberSprite;
////			cityInfoBasicLbl.text = currentlyShowingCity.totalCitizenConsumption.ToString() + "/" + currentlyShowingCity.lumberCount.ToString();
////		}

//		//Additional Resources
//		if (currentlyShowingCity.manaStoneCount > 0) {
//			cityInfoManaStoneIcon.SetActive (true);
//		} else {
//			cityInfoManaStoneIcon.SetActive (false);
//		}

//		if (currentlyShowingCity.cobaltCount > 0) {
//			cityInfoCobaltIcon.SetActive (true);
//		} else {
//			cityInfoCobaltIcon.SetActive (false);
//		}

//		if (currentlyShowingCity.mithrilCount > 0) {
//			cityInfoMithrilIcon.SetActive (true);
//		} else {
//			cityInfoMithrilIcon.SetActive (false);
//		}

//		if (currentlyShowingCity.kingdom.basicResource != BASE_RESOURCE_TYPE.WOOD && currentlyShowingCity.lumberCount > 0) {
//			cityInfoLumberIcon.SetActive (true);
//		} else {
//			cityInfoLumberIcon.SetActive (false);
//		}

//		if (currentlyShowingCity.kingdom.basicResource != BASE_RESOURCE_TYPE.STONE && currentlyShowingCity.stoneCount > 0) {
//			cityInfoStoneIcon.SetActive (true);
//		} else {
//			cityInfoStoneIcon.SetActive (false);
//		}

//		currentlyShowingCity.HighlightAllOwnedTiles(204f / 255f);

//        //ForTesting
//        forTestingLoyaltyLbl.text = ((Governor)currentlyShowingCity.governor.assignedRole).loyalty.ToString();

//		cityInfoGO.SetActive (true);

//	}

	public void HideCityInfo(){
		//unhighlight previously selected city
		if (currentlyShowingCity != null) {
			currentlyShowingCity.UnHighlightAllOwnedTiles ();
		}
		currentlyShowingCity = null;
		if (currentlyShowingKingdom != null) {
			currentlyShowingKingdom.HighlightAllOwnedTilesInKingdom ();
		}
		cityInfoGO.SetActive (false);
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

	public IEnumerator RepositionScrollView(UIScrollView thisScrollView){
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		thisScrollView.ResetPosition();
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
	public void ShowKingRelationships(){
		List<CharacterPortrait> characterPortraits = kingRelationshipsGrid.gameObject.GetComponentsInChildren<CharacterPortrait>().ToList();

		int nextIndex = 0;
        List<RelationshipKings> relationshipsToShow = currentlyShowingKingdom.king.relationshipKings
            .Where(x => currentlyShowingKingdom.discoveredKingdoms.Contains(x.king.city.kingdom)).ToList();

        if(relationshipsToShow.Count > 0) {
            kingRelationshipLine.SetActive(true);
            kingNoRelationshipsGO.SetActive(false);
        } else {
            kingRelationshipLine.SetActive(false);
            kingNoRelationshipsGO.SetActive(true);
        }

        for (int i = 0; i < characterPortraits.Count; i++) {
			CharacterPortrait currPortrait = characterPortraits[i];
            if(i < relationshipsToShow.Count) {
                RelationshipKings currRel = relationshipsToShow[i];
                if (currRel != null) {
                    currPortrait.SetCitizen(currRel.king, true);
                    currPortrait.ShowRelationshipLine(currRel, currRel.king.GetRelationshipWithCitizen(currentlyShowingKingdom.king));
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
                RelationshipKings rel = relationshipsToShow[i];

                GameObject kingGO = InstantiateUIObject(characterPortraitPrefab, this.transform);
				kingGO.GetComponent<CharacterPortrait>().SetCitizen(rel.king, true);
				kingGO.transform.localScale = Vector3.one;
				kingGO.GetComponent<CharacterPortrait> ().ShowRelationshipLine (rel, 
                    rel.king.GetRelationshipWithCitizen(currentlyShowingKingdom.king));
				kingRelationshipsGrid.AddChild(kingGO.transform);
				//kingRelationshipsGrid.Reposition ();
	//			kingGO.GetComponent<CharacterPortrait>().onClickCharacterPortrait += ShowRelationshipHistory;
			}
            StartCoroutine(RepositionGrid(kingRelationshipsGrid));
            StartCoroutine(RepositionScrollView(kingRelationshipsGrid.transform.parent.GetComponent<UIScrollView>()));
		}

		governorRelationshipsParentGO.SetActive(false);
		kingRelationshipsParentGO.SetActive(true);
	}
	public void ShowGovernorRelationships(){
		if (governorRelationshipsParentGO.activeSelf) {
			return;
		}
		List<Transform> children = governorsRelationshipGrid.GetChildList();
		for (int i = 0; i < children.Count; i++) {
			Destroy (children [i].gameObject);
		}

        for (int i = 0; i < currentlyShowingCitizen.city.kingdom.cities.Count; i++) {
			GameObject governorGO = InstantiateUIObject(characterPortraitPrefab, governorsRelationshipGrid.transform);
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

	public void ShowRelationshipHistory(Citizen citizenInRelationshipWith){
		RelationshipKings relationship = currentlyShowingCitizen.GetRelationshipWithCitizen(citizenInRelationshipWith);
		currentlyShowingRelationship = relationship;

		if (relationship.relationshipHistory.Count <= 0) {
			noRelationshipsToShowGO.SetActive (true);
		} else {
			noRelationshipsToShowGO.SetActive (false);
		}

		List<Transform> children = this.relationshipHistoryGrid.GetChildList();
		for (int i = 0; i < children.Count; i++) {
			Destroy(children[i].gameObject);
		}

		for (int i = 0; i < relationship.relationshipHistory.Count; i++) {
			GameObject historyGO = InstantiateUIObject(this.historyPortraitPrefab, this.relationshipHistoryGrid.transform);
			historyGO.GetComponent<HistoryPortrait> ().SetHistory(relationship.relationshipHistory[i]);
			historyGO.transform.localScale = Vector3.one;
			historyGO.transform.localPosition = Vector3.zero;
		}

		StartCoroutine (RepositionGrid (relationshipHistoryGrid));

		//For Testing
		relationshipHistoryForTestingGO.SetActive(true);
		sourceKinglikenessLbl.text = relationship.totalLike.ToString();
		targetKinglikenessLbl.text = citizenInRelationshipWith.GetRelationshipWithCitizen(currentlyShowingCitizen).totalLike.ToString();

		relationshipStatusSprite.color = Utilities.GetColorForRelationship(relationship.lordRelationship);
		relationshipHistoryGO.SetActive(true);
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
			familyTreeGO.SetActive (false);
		} else {
			if (familyTreeFatherGO.GetComponentInChildren<CharacterPortrait>() != null) {
				Destroy (familyTreeFatherGO.GetComponentInChildren<CharacterPortrait>().gameObject);
			}
			if (currentlyShowingCitizen.father != null) {
				GameObject fatherGO = InstantiateUIObject(characterPortraitPrefab, familyTreeFatherGO.transform);
				fatherGO.transform.localScale = new Vector3 (2.1f, 2.1f, 0f);
				fatherGO.transform.localPosition = Vector3.zero;
				fatherGO.GetComponent<CharacterPortrait> ().SetCitizen (currentlyShowingCitizen.father);
			}
			if (familyTreeMotherGO.GetComponentInChildren<CharacterPortrait>() != null) {
				Destroy (familyTreeMotherGO.GetComponentInChildren<CharacterPortrait>().gameObject);
			}
			if (currentlyShowingCitizen.mother != null) {
				GameObject motherGO = InstantiateUIObject(characterPortraitPrefab, familyTreeMotherGO.transform);
				motherGO.transform.localScale = new Vector3 (2.1f, 2.1f, 0f);
				motherGO.transform.localPosition = Vector3.zero;
				motherGO.GetComponent<CharacterPortrait> ().SetCitizen (currentlyShowingCitizen.mother);
			}
			if (familyTreeSpouseGO.GetComponentInChildren<CharacterPortrait>() != null) {
				Destroy (familyTreeSpouseGO.GetComponentInChildren<CharacterPortrait>().gameObject);
			}
			if (currentlyShowingCitizen.spouse != null) {
				GameObject spouseGO = InstantiateUIObject(characterPortraitPrefab, familyTreeSpouseGO.transform);
				spouseGO.transform.localScale = new Vector3 (2.1f, 2.1f, 0f);
				spouseGO.transform.localPosition = Vector3.zero;
				spouseGO.GetComponent<CharacterPortrait> ().SetCitizen (currentlyShowingCitizen.spouse);
				for (int i = 0; i < this.marriageHistoryOfCurrentCitizen.Count; i++) {
					if (currentlyShowingCitizen.gender == GENDER.MALE) {
						if (this.marriageHistoryOfCurrentCitizen [i].wife.id == currentlyShowingCitizen.spouse.id) {
							this.currentMarriageHistoryIndex = i;
							break;
						}
					} else {
						if (this.marriageHistoryOfCurrentCitizen [i].husband.id == currentlyShowingCitizen.spouse.id) {
							this.currentMarriageHistoryIndex = i;
							break;
						}
					}
				}
			} else if (this.marriageHistoryOfCurrentCitizen.Count > 0) {
				GameObject spouseGO = InstantiateUIObject(characterPortraitPrefab, familyTreeSpouseGO.transform);
				spouseGO.transform.localScale = new Vector3 (2.1f, 2.1f, 0f);
				spouseGO.transform.localPosition = Vector3.zero;
				if (currentlyShowingCitizen.gender == GENDER.MALE) {
					spouseGO.GetComponent<CharacterPortrait> ().SetCitizen (this.marriageHistoryOfCurrentCitizen[0].wife);
				} else {
					spouseGO.GetComponent<CharacterPortrait> ().SetCitizen (this.marriageHistoryOfCurrentCitizen[0].husband);
				}
				this.currentMarriageHistoryIndex = 0;
			}

			CharacterPortrait[] children = familyTreeChildGrid.GetComponentsInChildren<CharacterPortrait>();
			for (int i = 0; i < children.Length; i++) {
				Destroy (children [i].gameObject);
			}

			List<Transform> childPositions = familyTreeChildGrid.GetChildList ();
			for (int i = 0; i < currentlyShowingCitizen.children.Count; i++) {
				GameObject childGO = InstantiateUIObject(characterPortraitPrefab, childPositions [i].transform);
				childGO.transform.localScale = new Vector3 (2.1f, 2.1f, 0f);
				childGO.transform.localPosition = Vector3.zero;
				childGO.GetComponent<CharacterPortrait> ().SetCitizen (currentlyShowingCitizen.children [i]);
			}

			if (this.marriageHistoryOfCurrentCitizen.Count > 1) {
				nextMarriageBtn.SetActive (true);
			} else {
				nextMarriageBtn.SetActive (false);
			}

			familyTreeInnerSprite.color = currentlyShowingCitizen.city.kingdom.kingdomColor;

			familyTreeGO.SetActive (true);
		}
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

		Destroy(familyTreeSpouseGO.GetComponentInChildren<CharacterPortrait> ().gameObject);
		CharacterPortrait[] children = familyTreeChildGrid.GetComponentsInChildren<CharacterPortrait>();
		for (int i = 0; i < children.Length; i++) {
			Destroy (children [i].gameObject);
		}

		MarriedCouple marriedCoupleToShow = this.marriageHistoryOfCurrentCitizen[nextIndex];
		if (marriedCoupleToShow.husband.id == currentlyShowingCitizen.id) {
			//currentlyShowingCitizen is male
			GameObject spouseGO = InstantiateUIObject(characterPortraitPrefab, familyTreeSpouseGO.transform);
			spouseGO.transform.localScale = new Vector3 (2.1f, 2.1f, 0f);
			spouseGO.transform.localPosition = Vector3.zero;
			spouseGO.GetComponent<CharacterPortrait> ().SetCitizen (marriedCoupleToShow.wife);
		} else {
			//currentlyShowingCitizen is female	
			GameObject spouseGO = InstantiateUIObject(characterPortraitPrefab, familyTreeSpouseGO.transform);
			spouseGO.transform.localScale = new Vector3 (2.1f, 2.1f, 0f);
			spouseGO.transform.localPosition = Vector3.zero;
			spouseGO.GetComponent<CharacterPortrait> ().SetCitizen (marriedCoupleToShow.husband);
		}

		List<Transform> childPositions = familyTreeChildGrid.GetChildList ();
		for (int i = 0; i < marriedCoupleToShow.children.Count; i++) {
			GameObject childGO = InstantiateUIObject(characterPortraitPrefab, childPositions[i].transform);
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

	/*public void ShowEventsOfType(GameObject GO){
		if (eventsOfTypeGo.activeSelf) {
			if (lastClickedEventType != null) {
				if (lastClickedEventType == GO) {
					eventsOfTypeGo.SetActive(false);
					EventManager.Instance.onHideEvents.Invoke();
					return;
				} else {
					lastClickedEventType.GetComponent<ButtonToggle> ().OnClick ();
				}
			}
			EventManager.Instance.onHideEvents.Invoke();
		}
		lastClickedEventType = GO;
		if (GO.name == "AllBtn") {
			bool noEvents = true;
			List<Transform> children = gameEventsOfTypeGrid.GetChildList ();
			for (int i = 0; i < children.Count; i++) {
				Destroy (children [i].gameObject);
			}
			for (int i = 0; i < EventManager.Instance.allEvents.Keys.Count; i++) {
				EVENT_TYPES currentKey = EventManager.Instance.allEvents.Keys.ElementAt(i);
				List<GameEvent> currentGameEventList = EventManager.Instance.allEvents[currentKey].Where(x => x.isActive).ToList();
				for (int j = 0; j < currentGameEventList.Count; j++) {
					noEvents = false;
					GameObject eventGO = GameObject.Instantiate (gameEventPrefab, gameEventsOfTypeGrid.transform) as GameObject;
					eventGO.GetComponent<EventItem>().SetEvent (currentGameEventList[j]);
					eventGO.GetComponent<EventItem> ().SetSpriteIcon (GetSpriteForEvent (currentKey));
					eventGO.GetComponent<EventItem> ().onClickEvent += ShowSpecificEvent;
					eventGO.transform.localScale = Vector3.one;
				}
			}
			if (!noEvents) {
				EventManager.Instance.onShowEventsOfType.Invoke (EVENT_TYPES.ALL);
				//				CameraMove.Instance.ShowWholeMap ();
			}
		} else {
			EVENT_TYPES eventType = (EVENT_TYPES)(System.Enum.Parse (typeof(EVENT_TYPES), GO.name));
			if (EventManager.Instance.allEvents.ContainsKey (eventType)) {
				List<GameEvent> gameEventsOfType = EventManager.Instance.allEvents [eventType].Where (x => x.isActive).ToList ();
				List<Transform> children = gameEventsOfTypeGrid.GetChildList ();
				for (int i = 0; i < children.Count; i++) {
					Destroy (children [i].gameObject);
				}
				for (int i = 0; i < gameEventsOfType.Count; i++) {
					GameObject eventGO = GameObject.Instantiate (gameEventPrefab, gameEventsOfTypeGrid.transform) as GameObject;
					eventGO.GetComponent<EventItem> ().SetEvent (gameEventsOfType [i]);
					eventGO.GetComponent<EventItem> ().SetSpriteIcon (GetSpriteForEvent (gameEventsOfType [i].eventType));
					eventGO.GetComponent<EventItem> ().onClickEvent += ShowSpecificEvent;
					eventGO.transform.localScale = Vector3.one;
				}
				if (gameEventsOfType.Count > 0) {
				EventManager.Instance.onShowEventsOfType.Invoke (eventType);
					//					CameraMove.Instance.ShowWholeMap ();
				}
			} else {
				List<Transform> children = gameEventsOfTypeGrid.GetChildList ();
				for (int i = 0; i < children.Count; i++) {
					Destroy (children [i].gameObject);
				}
			}

		}

		StartCoroutine (RepositionGrid (gameEventsOfTypeGrid));
		eventsOfTypeGo.SetActive (true);
	} */

	public void ShowEventsOfType(GameEvent gameEvent){
        //GameObject eventGO = InstantiateUIObject(gameEventPrefab, gameEventsOfTypeGrid.transform);
        //eventGO.GetComponent<EventItem>().SetEvent(gameEvent);
        //eventGO.GetComponent<EventItem>().SetSpriteIcon(GetSpriteForEvent(gameEvent.eventType));
        //eventGO.GetComponent<EventItem>().onClickEvent += ShowEventLogs;
        //eventGO.GetComponent<EventItem>().StartExpirationTimer();
        //eventGO.transform.localScale = Vector3.one;
        //StartCoroutine(RepositionGrid(gameEventsOfTypeGrid));

		if(gameEvent.startedBy != null){ //Kingdom Event
			KingdomFlagItem kingdomOwner = kingdomListOtherKingdomsGrid.GetChildList()
				.Where(x => x.GetComponent<KingdomFlagItem>().kingdom.id == gameEvent.startedByKingdom.id)
				.FirstOrDefault().GetComponent<KingdomFlagItem>();
			if (kingdomOwner != null) {
				GameObject eventGO = InstantiateUIObject(gameEventPrefab, this.transform);
				eventGO.GetComponent<EventItem>().SetEvent(gameEvent);
				eventGO.GetComponent<EventItem>().SetSpriteIcon(GetSpriteForEvent(gameEvent.eventType));
				eventGO.GetComponent<EventItem>().onClickEvent += ShowEventLogs;
				eventGO.GetComponent<EventItem>().StartExpirationTimer();
				eventGO.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
				kingdomOwner.AddGameObjectToGrid(eventGO);
				gameEvent.goEventItem = eventGO;

			}
		}else{ //World Event
			GameObject eventGO = InstantiateUIObject(this.gameEventPrefab, this.gameEventsOfTypeGrid.transform);
			eventGO.GetComponent<EventItem>().SetEvent(gameEvent);
			eventGO.GetComponent<EventItem>().SetSpriteIcon(GetSpriteForEvent(gameEvent.eventType));
			eventGO.GetComponent<EventItem>().onClickEvent += ShowEventLogs;
			eventGO.GetComponent<EventItem>().StartExpirationTimer();
			eventGO.transform.localPosition = Vector3.zero;
			eventGO.transform.localScale = Vector3.one;
			RepositionGridCallback(this.gameEventsOfTypeGrid);
			gameEvent.goEventItem = eventGO;
		}
      
        
	}

	/*public void UpdateEventsOfType(){
		if (lastClickedEventType.name == "AllBtn") {
			List<Transform> children = gameEventsOfTypeGrid.GetChildList ();
			bool noEvents = true;
			for (int i = 0; i < children.Count; i++) {
				Destroy (children [i].gameObject);
			}
			for (int i = 0; i < EventManager.Instance.allEvents.Keys.Count; i++) {
				EVENT_TYPES currentKey = EventManager.Instance.allEvents.Keys.ElementAt(i);
				List<GameEvent> currentGameEventList = EventManager.Instance.allEvents[currentKey].Where(x => x.isActive).ToList();
				for (int j = 0; j < currentGameEventList.Count; j++) {
					noEvents = false;
					GameObject eventGO = GameObject.Instantiate (gameEventPrefab, gameEventsOfTypeGrid.transform) as GameObject;
					eventGO.GetComponent<EventItem>().SetEvent (currentGameEventList[j]);
					eventGO.GetComponent<EventItem> ().SetSpriteIcon (GetSpriteForEvent (currentKey));
					eventGO.GetComponent<EventItem> ().onClickEvent += ShowSpecificEvent;
					eventGO.transform.localScale = Vector3.one;
				}
			}
			if (!noEvents) {
//				EventManager.Instance.onShowEventsOfType.Invoke (EVENT_TYPES.ALL);
			}
		} else {
			EVENT_TYPES eventType = (EVENT_TYPES)(System.Enum.Parse (typeof(EVENT_TYPES), lastClickedEventType.name));
			if (EventManager.Instance.allEvents.ContainsKey (eventType)) {
				List<GameEvent> gameEventsOfType = EventManager.Instance.allEvents[eventType].Where(x => x.isActive).ToList();
				List<Transform> children = gameEventsOfTypeGrid.GetChildList ();
				for (int i = 0; i < children.Count; i++) {
					Destroy (children [i].gameObject);
				}
				for (int i = 0; i < gameEventsOfType.Count; i++) {
					GameObject eventGO = GameObject.Instantiate (gameEventPrefab, gameEventsOfTypeGrid.transform) as GameObject;
					eventGO.GetComponent<EventItem> ().SetEvent (gameEventsOfType [i]);
					eventGO.GetComponent<EventItem> ().SetSpriteIcon (GetSpriteForEvent (gameEventsOfType [i].eventType));
					eventGO.GetComponent<EventItem> ().onClickEvent += ShowSpecificEvent;
					eventGO.transform.localScale = Vector3.one;
				}
				if (gameEventsOfType.Count > 0) {
					EventManager.Instance.onShowEventsOfType.Invoke (eventType);
				}
			}

		}
		StartCoroutine (RepositionGrid (gameEventsOfTypeGrid));
		eventsOfTypeGo.SetActive (true);
	} */

	/*#region ShowSpecificEvent OLD
	public void ShowSpecificEvent(GameEvent gameEvent){
		specificEventNameLbl.text = gameEvent.eventType.ToString().Replace("_", " ");
		specificEventDescriptionLbl.text = gameEvent.description;
		specificEventStartDateLbl.text = "Started " + ((MONTH)gameEvent.startMonth).ToString() + " " + gameEvent.startDay.ToString() + ", " + gameEvent.startYear.ToString();

		this.specificEventMiscTitleLbl.gameObject.SetActive(false);
		this.lblSpecificEventCity.gameObject.SetActive(false);
		this.lblSpecificPilfered.gameObject.SetActive(false);
		this.lblSpecificSuccessRate.gameObject.SetActive(false);
		this.goSpecificHiddenEventItem.SetActive(false);
		specificEventsCandidatesLbl.gameObject.SetActive(false);
		specificEventNormalGO.SetActive (true);
		specificEventWarGO.SetActive (false);
		if(gameEvent.eventStatus == EVENT_STATUS.HIDDEN){
			this.goSpecificEventHidden.SetActive(true);
		}else{
			this.goSpecificEventHidden.SetActive(false);
		}

		if (gameEvent.eventType != EVENT_TYPES.STATE_VISIT && gameEvent.eventType != EVENT_TYPES.INVASION_PLAN) {
			this.specificEventBarTitle.text = "Duration";
			specificEventProgBar.value = (float)((float)gameEvent.remainingDays / (float)gameEvent.durationInDays);
		}

		if (gameEvent.eventType == EVENT_TYPES.MARRIAGE_INVITATION) {
			MarriageInvitation marriageEvent = (MarriageInvitation)gameEvent;
			ShowMarriageInvitationEvent (marriageEvent);
		}else if (gameEvent.eventType == EVENT_TYPES.BORDER_CONFLICT) {
			BorderConflict borderConflict = (BorderConflict)gameEvent;
			ShowBorderConflictEvent (borderConflict);
		}else if (gameEvent.eventType == EVENT_TYPES.DIPLOMATIC_CRISIS) {
			DiplomaticCrisis diplomaticCrisis = (DiplomaticCrisis)gameEvent;
			ShowDiplomaticCrisisEvent (diplomaticCrisis);
		}else if (gameEvent.eventType == EVENT_TYPES.ADMIRATION) {
			Admiration admiration = (Admiration)gameEvent;
			ShowAdmirationEvent (admiration);
		}else if (gameEvent.eventType == EVENT_TYPES.STATE_VISIT) {
			StateVisit stateVisit = (StateVisit)gameEvent;
			ShowStateVisitEvent (stateVisit);
		}else if (gameEvent.eventType == EVENT_TYPES.RAID) {
			Raid raid = (Raid)gameEvent;
			ShowRaidEvent (raid);
		}else if (gameEvent.eventType == EVENT_TYPES.ASSASSINATION) {
			Assassination assassination = (Assassination)gameEvent;
			ShowAssassinationEvent (assassination);
		}else if (gameEvent.eventType == EVENT_TYPES.ESPIONAGE) {
			Espionage espionage = (Espionage)gameEvent;
			ShowEspionageEvent (espionage);
		}else if (gameEvent.eventType == EVENT_TYPES.EXHORTATION) {
			Exhortation exhortation = (Exhortation)gameEvent;
			ShowExhortationEvent (exhortation);
		}else if (gameEvent.eventType == EVENT_TYPES.MILITARIZATION) {
			Militarization militarization = (Militarization)gameEvent;
			ShowMilitarizationEvent (militarization);
		}else if (gameEvent.eventType == EVENT_TYPES.POWER_GRAB) {
			PowerGrab powerGrab = (PowerGrab)gameEvent;
			ShowPowerGrabEvent (powerGrab);
		}else if (gameEvent.eventType == EVENT_TYPES.JOIN_WAR_REQUEST) {
			JoinWar joinWar = (JoinWar)gameEvent;
			ShowJoinWarEvent (joinWar);
		}else if (gameEvent.eventType == EVENT_TYPES.INVASION_PLAN) {
			InvasionPlan invasionPlan = (InvasionPlan)gameEvent;
			ShowInvasionPlanEvent (invasionPlan);
		}else if (gameEvent.eventType == EVENT_TYPES.EXPANSION) {
			Expansion expansion = (Expansion)gameEvent;
			ShowExpansionEvent (expansion);
		}else if (gameEvent.eventType == EVENT_TYPES.KINGDOM_WAR) {
			War war = (War)gameEvent;
			ShowWarEvent (war);
		}else if (gameEvent.eventType == EVENT_TYPES.REQUEST_PEACE) {
			RequestPeace requestPeace = (RequestPeace)gameEvent;
			ShowRequestPeace (requestPeace);
		}

		specificEventResolutionLbl.text = gameEvent.resolution;
		currentlyShowingEvent = gameEvent;
		specificEventGO.SetActive(true);
	}
	private void ShowBorderConflictEvent(BorderConflict borderConflict){
		List<Transform> children = this.specificEventStartedByGrid.GetChildList();
		for (int i = 0; i < children.Count; i++) {
			Destroy (children [i].gameObject);
		}

		List<Transform> children2 = this.specificEventCandidatesGrid.GetChildList();
		for (int i = 0; i < children2.Count; i++) {
			Destroy (children2 [i].gameObject);
		}

		List<Transform> children3 = this.specificEventMiscGrid.GetChildList();
		for (int i = 0; i < children3.Count; i++) {
			Destroy (children3 [i].gameObject);
		}

		if(borderConflict.startedBy != null){
			GameObject startedByGO = GameObject.Instantiate (characterPortraitPrefab, specificEventStartedByGrid.transform) as GameObject;
			startedByGO.GetComponent<CharacterPortrait> ().SetCitizen (borderConflict.startedBy);
			startedByGO.transform.localScale = Vector3.one;
			startedByGO.transform.position = Vector3.zero;
			StartCoroutine (RepositionGrid (specificEventStartedByGrid));
		}


		this.specificEventCandidatesTitleLbl.text = "RESOLVER";
		if(borderConflict.activeEnvoyResolve != null){
			GameObject candidates = GameObject.Instantiate (characterPortraitPrefab, this.specificEventCandidatesGrid.transform) as GameObject;
			candidates.GetComponent<CharacterPortrait> ().SetCitizen (borderConflict.activeEnvoyResolve.citizen);
			candidates.transform.localScale = Vector3.one;
			candidates.transform.position = Vector3.zero;
			StartCoroutine (RepositionGrid (this.specificEventCandidatesGrid));
		}

		this.specificEventMiscTitleLbl.text = "PROVOKER";
		this.specificEventMiscTitleLbl.gameObject.SetActive(true);

		if(borderConflict.activeEnvoyProvoke != null){
			GameObject candidates = GameObject.Instantiate (characterPortraitPrefab, this.specificEventMiscGrid.transform) as GameObject;
			candidates.GetComponent<CharacterPortrait> ().SetCitizen (borderConflict.activeEnvoyProvoke.citizen);
			candidates.transform.localScale = Vector3.one;
			candidates.transform.position = Vector3.zero;
			StartCoroutine (RepositionGrid (this.specificEventMiscGrid));

		}
	}
	private void ShowDiplomaticCrisisEvent(DiplomaticCrisis diplomaticCrisis){
		List<Transform> children = this.specificEventStartedByGrid.GetChildList();
		for (int i = 0; i < children.Count; i++) {
			Destroy (children [i].gameObject);
		}

		List<Transform> children2 = this.specificEventCandidatesGrid.GetChildList();
		for (int i = 0; i < children2.Count; i++) {
			Destroy (children2 [i].gameObject);
		}

		List<Transform> children3 = this.specificEventMiscGrid.GetChildList();
		for (int i = 0; i < children3.Count; i++) {
			Destroy (children3 [i].gameObject);
		}

		if(diplomaticCrisis.startedBy != null){
			GameObject startedByGO = GameObject.Instantiate (characterPortraitPrefab, specificEventStartedByGrid.transform) as GameObject;
			startedByGO.GetComponent<CharacterPortrait> ().SetCitizen (diplomaticCrisis.startedBy);
			startedByGO.transform.localScale = Vector3.one;
			startedByGO.transform.position = Vector3.zero;
			StartCoroutine (RepositionGrid (specificEventStartedByGrid));
		}


		this.specificEventCandidatesTitleLbl.text = "RESOLVER";
		if(diplomaticCrisis.activeEnvoyResolve != null){
			GameObject candidates = GameObject.Instantiate (characterPortraitPrefab, this.specificEventCandidatesGrid.transform) as GameObject;
			candidates.GetComponent<CharacterPortrait> ().SetCitizen (diplomaticCrisis.activeEnvoyResolve.citizen);
			candidates.transform.localScale = Vector3.one;
			candidates.transform.position = Vector3.zero;
			StartCoroutine (RepositionGrid (this.specificEventCandidatesGrid));
		}

		this.specificEventMiscTitleLbl.text = "PROVOKER";
		this.specificEventMiscTitleLbl.gameObject.SetActive(true);

		if(diplomaticCrisis.activeEnvoyProvoke != null){
			GameObject candidates = GameObject.Instantiate (characterPortraitPrefab, this.specificEventMiscGrid.transform) as GameObject;
			candidates.GetComponent<CharacterPortrait> ().SetCitizen (diplomaticCrisis.activeEnvoyProvoke.citizen);
			candidates.transform.localScale = Vector3.one;
			candidates.transform.position = Vector3.zero;
			StartCoroutine (RepositionGrid (this.specificEventMiscGrid));

		}
	}
	private void ShowAdmirationEvent(Admiration admiration){
		List<Transform> children = this.specificEventStartedByGrid.GetChildList();
		for (int i = 0; i < children.Count; i++) {
			Destroy (children [i].gameObject);
		}

		List<Transform> children2 = this.specificEventCandidatesGrid.GetChildList();
		for (int i = 0; i < children2.Count; i++) {
			Destroy (children2 [i].gameObject);
		}

		List<Transform> children3 = this.specificEventMiscGrid.GetChildList();
		for (int i = 0; i < children3.Count; i++) {
			Destroy (children3 [i].gameObject);
		}

		if(admiration.startedBy != null){
			GameObject startedByGO = GameObject.Instantiate (characterPortraitPrefab, specificEventStartedByGrid.transform) as GameObject;
			startedByGO.GetComponent<CharacterPortrait> ().SetCitizen (admiration.startedBy);
			startedByGO.transform.localScale = Vector3.one;
			startedByGO.transform.position = Vector3.zero;
			StartCoroutine (RepositionGrid (specificEventStartedByGrid));
		}


		this.specificEventCandidatesTitleLbl.text = "TARGET";
		if(admiration.kingdom2 != null){
			GameObject candidates = GameObject.Instantiate (characterPortraitPrefab, this.specificEventCandidatesGrid.transform) as GameObject;
			candidates.GetComponent<CharacterPortrait> ().SetCitizen (admiration.kingdom2.king);
			candidates.transform.localScale = Vector3.one;
			candidates.transform.position = Vector3.zero;
			StartCoroutine (RepositionGrid (this.specificEventCandidatesGrid));
		}

		this.specificEventMiscTitleLbl.gameObject.SetActive(false);

	}
	private void ShowStateVisitEvent(StateVisit stateVisit){
		List<Transform> children = this.specificEventStartedByGrid.GetChildList();
		for (int i = 0; i < children.Count; i++) {
			Destroy (children [i].gameObject);
		}

		List<Transform> children2 = this.specificEventCandidatesGrid.GetChildList();
		for (int i = 0; i < children2.Count; i++) {
			Destroy (children2 [i].gameObject);
		}

		List<Transform> children3 = this.specificEventMiscGrid.GetChildList();
		for (int i = 0; i < children3.Count; i++) {
			Destroy (children3 [i].gameObject);
		}

		if(stateVisit.startedBy != null){
			GameObject startedByGO = GameObject.Instantiate (characterPortraitPrefab, specificEventStartedByGrid.transform) as GameObject;
			startedByGO.GetComponent<CharacterPortrait> ().SetCitizen (stateVisit.startedBy);
			startedByGO.transform.localScale = Vector3.one;
			startedByGO.transform.position = Vector3.zero;
			StartCoroutine (RepositionGrid (specificEventStartedByGrid));
		}


		this.specificEventBarTitle.text = "Success";
		this.specificEventProgBar.value = (float)stateVisit.successMeter / 100f;
		this.specificEventCandidatesTitleLbl.text = "SABOTEURS";
		if(stateVisit.saboteurEnvoy != null){
			GameObject candidates = GameObject.Instantiate (characterPortraitPrefab, this.specificEventCandidatesGrid.transform) as GameObject;
			candidates.GetComponent<CharacterPortrait> ().SetCitizen (stateVisit.saboteurEnvoy.citizen);
			candidates.transform.localScale = Vector3.one;
			candidates.transform.position = Vector3.zero;
		}
		StartCoroutine (RepositionGrid (this.specificEventCandidatesGrid));

	}
	private void ShowAssassinationEvent(Assassination assassination){
		List<Transform> children = this.specificEventStartedByGrid.GetChildList();
		for (int i = 0; i < children.Count; i++) {
			Destroy (children [i].gameObject);
		}

		List<Transform> children2 = this.specificEventCandidatesGrid.GetChildList();
		for (int i = 0; i < children2.Count; i++) {
			Destroy (children2 [i].gameObject);
		}

		List<Transform> children3 = this.specificEventMiscGrid.GetChildList();
		for (int i = 0; i < children3.Count; i++) {
			Destroy (children3 [i].gameObject);
		}

		if(assassination.startedBy != null){
			GameObject startedByGO = GameObject.Instantiate (characterPortraitPrefab, specificEventStartedByGrid.transform) as GameObject;
			startedByGO.GetComponent<CharacterPortrait> ().SetCitizen (assassination.startedBy);
			startedByGO.transform.localScale = Vector3.one;
			startedByGO.transform.position = Vector3.zero;
			StartCoroutine (RepositionGrid (specificEventStartedByGrid));
		}


		this.specificEventCandidatesTitleLbl.text = "TARGET";
		GameObject candidates = GameObject.Instantiate (characterPortraitPrefab, this.specificEventCandidatesGrid.transform) as GameObject;
		candidates.GetComponent<CharacterPortrait> ().SetCitizen (assassination.targetCitizen);
		candidates.transform.localScale = Vector3.one;
		candidates.transform.position = Vector3.zero;
		StartCoroutine (RepositionGrid (this.specificEventCandidatesGrid));


		this.specificEventMiscTitleLbl.text = "UNCOVERED";
		this.specificEventMiscTitleLbl.gameObject.SetActive(true);
		for(int i = 0; i < assassination.uncovered.Count; i++){
			GameObject uncovered = GameObject.Instantiate (characterPortraitPrefab, this.specificEventMiscGrid.transform) as GameObject;
			uncovered.GetComponent<CharacterPortrait> ().SetCitizen (assassination.uncovered[i]);
			uncovered.transform.localScale = Vector3.one;
			uncovered.transform.position = Vector3.zero;
		}
		StartCoroutine (RepositionGrid (this.specificEventMiscGrid));


		this.lblSpecificSuccessRate.text = "SUCCESS RATE " + assassination.successRate + "%";
		this.lblSpecificSuccessRate.gameObject.SetActive(true);

	}
	private void ShowEspionageEvent(Espionage espionage){
		List<Transform> children = this.specificEventStartedByGrid.GetChildList();
		for (int i = 0; i < children.Count; i++) {
			Destroy (children [i].gameObject);
		}

		List<Transform> children2 = this.specificEventCandidatesGrid.GetChildList();
		for (int i = 0; i < children2.Count; i++) {
			Destroy (children2 [i].gameObject);
		}

		List<Transform> children3 = this.specificEventMiscGrid.GetChildList();
		for (int i = 0; i < children3.Count; i++) {
			Destroy (children3 [i].gameObject);
		}

		if(espionage.startedBy != null){
			GameObject startedByGO = GameObject.Instantiate (characterPortraitPrefab, specificEventStartedByGrid.transform) as GameObject;
			startedByGO.GetComponent<CharacterPortrait> ().SetCitizen (espionage.startedBy);
			startedByGO.transform.localScale = Vector3.one;
			startedByGO.transform.position = Vector3.zero;
			StartCoroutine (RepositionGrid (specificEventStartedByGrid));
		}
			
		this.specificEventCandidatesTitleLbl.text = "TARGET";
		GameObject candidates = GameObject.Instantiate (characterPortraitPrefab, this.specificEventCandidatesGrid.transform) as GameObject;
		candidates.GetComponent<CharacterPortrait> ().SetCitizen (espionage.targetKingdom.king);
		candidates.transform.localScale = Vector3.one;
		candidates.transform.position = Vector3.zero;
		StartCoroutine (RepositionGrid (this.specificEventCandidatesGrid));


		this.specificEventMiscTitleLbl.text = "HIDDEN EVENTS";
		this.specificEventMiscTitleLbl.gameObject.SetActive(true);
		for(int i = 0; i < espionage.allEventsAffectingTarget.Count; i++){
			GameObject eventGO = GameObject.Instantiate (gameEventPrefab, this.specificEventMiscGrid.transform) as GameObject;
			eventGO.GetComponent<EventItem>().SetEvent (espionage.allEventsAffectingTarget[i]);
			eventGO.GetComponent<EventItem> ().SetSpriteIcon (GetSpriteForEvent(espionage.allEventsAffectingTarget[i].eventType));
			eventGO.GetComponent<EventItem> ().onClickEvent += ShowSpecificEvent;
			eventGO.transform.localScale = Vector3.one;
			eventGO.transform.position = Vector3.zero;
		}
		StartCoroutine (RepositionGrid (this.specificEventMiscGrid));


		this.lblSpecificSuccessRate.text = "SUCCESS RATE " + espionage.successRate + "%";
		this.lblSpecificSuccessRate.gameObject.SetActive(true);

		if(espionage.chosenEvent != null && espionage.hasFound){
			goSpecificHiddenEventItem.GetComponent<EventItem>().SetEvent (espionage.chosenEvent);
			goSpecificHiddenEventItem.GetComponent<EventItem> ().SetSpriteIcon (GetSpriteForEvent(espionage.chosenEvent.eventType));
			goSpecificHiddenEventItem.GetComponent<EventItem> ().onClickEvent += ShowSpecificEvent;
			goSpecificHiddenEventItem.SetActive(true);
		}

	}
	private void ShowRaidEvent(Raid raid){
		List<Transform> children = this.specificEventStartedByGrid.GetChildList();
		for (int i = 0; i < children.Count; i++) {
			Destroy (children [i].gameObject);
		}

		List<Transform> children2 = this.specificEventCandidatesGrid.GetChildList();
		for (int i = 0; i < children2.Count; i++) {
			Destroy (children2 [i].gameObject);
		}

		List<Transform> children3 = this.specificEventMiscGrid.GetChildList();
		for (int i = 0; i < children3.Count; i++) {
			Destroy (children3 [i].gameObject);
		}

		if(raid.startedBy != null){
			GameObject startedByGO = GameObject.Instantiate (characterPortraitPrefab, specificEventStartedByGrid.transform) as GameObject;
			startedByGO.GetComponent<CharacterPortrait> ().SetCitizen (raid.startedBy);
			startedByGO.transform.localScale = Vector3.one;
			startedByGO.transform.position = Vector3.zero;
			StartCoroutine (RepositionGrid (specificEventStartedByGrid));
		}


		this.specificEventCandidatesTitleLbl.text = "TARGET";
		this.lblSpecificEventCity.text = raid.raidedCity.name;
		this.lblSpecificEventCity.gameObject.SetActive(true);

		this.specificEventMiscTitleLbl.text = "PILFERED";
		this.specificEventMiscTitleLbl.gameObject.SetActive(true);
		this.lblSpecificPilfered.text = raid.pilfered;
		this.lblSpecificPilfered.gameObject.SetActive(true);

		this.lblSpecificSuccessRate.text = "SUCCESS RATE 25%";
		this.lblSpecificSuccessRate.gameObject.SetActive(true);

	}
	private void ShowMarriageInvitationEvent(MarriageInvitation marriageEvent){
		List<Transform> children = specificEventStartedByGrid.GetChildList();
		if (children.Count <= 0 || (children.Count > 0 && children[0].GetComponent<CharacterPortrait>().citizen.id != marriageEvent.startedBy.id)) {
			for (int i = 0; i < children.Count; i++) {
				Destroy (children [i].gameObject);
			}

			GameObject startedByGO = GameObject.Instantiate (characterPortraitPrefab, specificEventStartedByGrid.transform) as GameObject;
			startedByGO.GetComponent<CharacterPortrait> ().SetCitizen (marriageEvent.startedBy);
			startedByGO.transform.localScale = Vector3.one;
			startedByGO.transform.position = Vector3.zero;
			StartCoroutine (RepositionGrid (specificEventStartedByGrid));
		}

		specificEventCandidatesTitleLbl.text = "CANDIDATES";
		children = specificEventCandidatesGrid.GetChildList();
		List<Citizen> elligibleCitizensCurrentlyShowed = new List<Citizen>();
		for (int i = 0; i < children.Count; i++) {
			elligibleCitizensCurrentlyShowed.Add(children [i].GetComponent<CharacterPortrait> ().citizen);
		}
		List<Citizen> additionalCitizens = marriageEvent.elligibleCitizens.Intersect(elligibleCitizensCurrentlyShowed).ToList();
		if ((additionalCitizens.Count != marriageEvent.elligibleCitizens.Count && elligibleCitizensCurrentlyShowed.Count != marriageEvent.elligibleCitizens.Count) 
			|| elligibleCitizensCurrentlyShowed.Count <= 0 || marriageEvent.elligibleCitizens.Count <= 0) {
			for (int i = 0; i < children.Count; i++) {
				Destroy (children [i].gameObject);
			}

			for (int i = 0; i < marriageEvent.elligibleCitizens.Count; i++) {
				GameObject candidateGO = GameObject.Instantiate (characterPortraitPrefab, specificEventCandidatesGrid.transform) as GameObject;
				candidateGO.GetComponent<CharacterPortrait> ().SetCitizen (marriageEvent.elligibleCitizens [i]);
				candidateGO.transform.localScale = Vector3.one;
				candidateGO.transform.position = Vector3.zero;
			}
			StartCoroutine (RepositionGrid (specificEventCandidatesGrid));
		}
	}
	private void ShowExhortationEvent(Exhortation exhortationEvent){
		//for started by
		List<Transform> children = specificEventStartedByGrid.GetChildList();
		if (children.Count <= 0 || (children.Count > 0 && children[0].GetComponent<CharacterPortrait>().citizen.id != exhortationEvent.startedBy.id)) {
			for (int i = 0; i < children.Count; i++) {
				Destroy (children [i].gameObject);
			}
			GameObject startedByGO = GameObject.Instantiate (characterPortraitPrefab, specificEventStartedByGrid.transform) as GameObject;
			startedByGO.GetComponent<CharacterPortrait> ().SetCitizen (exhortationEvent.startedBy);
			startedByGO.transform.localScale = Vector3.one;
			startedByGO.transform.position = Vector3.zero;
			StartCoroutine (RepositionGrid(specificEventStartedByGrid));
		}

		lblSpecificSuccessRate.text = exhortationEvent.successRate.ToString();
		lblSpecificSuccessRate.gameObject.SetActive (true);
		//for target
		specificEventCandidatesTitleLbl.text = "Target";
		children = specificEventCandidatesGrid.GetChildList();
		if (children.Count > 0) {
			Citizen currentlyShowingTarget = children[0].GetComponent<CharacterPortrait>().citizen;
			if (currentlyShowingTarget.id != exhortationEvent.targetCitizen.id) {
				children [0].GetComponent<CharacterPortrait>().SetCitizen (exhortationEvent.targetCitizen);
			}
		} else {
			GameObject targetGO = GameObject.Instantiate (characterPortraitPrefab, specificEventCandidatesGrid.transform) as GameObject;
			targetGO.GetComponent<CharacterPortrait> ().SetCitizen (exhortationEvent.targetCitizen);
			targetGO.transform.localScale = Vector3.one;
			targetGO.transform.position = Vector3.zero;

		}
		StartCoroutine (RepositionGrid(specificEventCandidatesGrid));

		specificEventMiscTitleLbl.text = "Sent";
		children = specificEventMiscGrid.GetChildList();
		if (children.Count > 0) {
			Citizen currentlyShowingSent = children[0].GetComponent<CharacterPortrait>().citizen;
			if (currentlyShowingSent.id != exhortationEvent.citizenSent.id) {
				children [0].GetComponent<CharacterPortrait>().SetCitizen (exhortationEvent.citizenSent);
			}
		} else {
			GameObject targetGO = GameObject.Instantiate (characterPortraitPrefab, specificEventMiscGrid.transform) as GameObject;
			targetGO.GetComponent<CharacterPortrait> ().SetCitizen (exhortationEvent.citizenSent);
			targetGO.transform.localScale = Vector3.one;
			targetGO.transform.position = Vector3.zero;
		}
		StartCoroutine (RepositionGrid(specificEventMiscGrid));
	}
	private void ShowMilitarizationEvent(Militarization militarizationEvent){
		//for started by
		List<Transform> children = specificEventStartedByGrid.GetChildList();
		if (children.Count <= 0 || (children.Count > 0 && children[0].GetComponent<CharacterPortrait>().citizen.id != militarizationEvent.startedBy.id)) {
			for (int i = 0; i < children.Count; i++) {
				Destroy (children [i].gameObject);
			}
			GameObject startedByGO = GameObject.Instantiate (characterPortraitPrefab, specificEventStartedByGrid.transform) as GameObject;
			startedByGO.GetComponent<CharacterPortrait> ().SetCitizen (militarizationEvent.startedBy);
			startedByGO.transform.localScale = Vector3.one;
			startedByGO.transform.position = Vector3.zero;
			StartCoroutine (RepositionGrid(specificEventStartedByGrid));
		}

		//for military summary
		children = specificEventCandidatesGrid.GetChildList();
		if (children.Count > 0) {
			for (int i = 0; i < children.Count; i++) {
				Destroy (children[i].gameObject);
			}
		}

		specificEventCandidatesTitleLbl.text = "MILITARY";
		int totalMilitaryStrength = 0;
		List<Citizen> generals = militarizationEvent.startedByKingdom.GetAllCitizensOfType(ROLE.GENERAL);
		for (int i = 0; i < generals.Count; i++) {
			totalMilitaryStrength += ((General)generals[i].assignedRole).GetArmyHP();
		}
		specificEventsCandidatesLbl.text = "Generals: " + generals.Count.ToString () + "\nTotal Strength: " + totalMilitaryStrength.ToString();
		specificEventsCandidatesLbl.gameObject.SetActive(true);

		//Uncovered
		specificEventMiscTitleLbl.text = "UNCOVERED";
		specificEventMiscTitleLbl.gameObject.SetActive(true);
		children = this.specificEventMiscGrid.GetChildList ();
		List<Citizen> currentlyShowingUncoveredCitizens = children.Select (x => x.GetComponent<CharacterPortrait>().citizen).ToList();
		List<Citizen> additionalCitizens = militarizationEvent.uncovered.Except(currentlyShowingUncoveredCitizens).ToList();
		if (additionalCitizens.Count > 0 || children.Count <= 0) {
			for (int i = 0; i < children.Count; i++) {
				Destroy (children [i].gameObject);
			}

			for (int i = 0; i < militarizationEvent.uncovered.Count; i++) {
				GameObject uncovered = GameObject.Instantiate (characterPortraitPrefab, this.specificEventMiscGrid.transform) as GameObject;
				uncovered.GetComponent<CharacterPortrait> ().SetCitizen (militarizationEvent.uncovered [i]);
				uncovered.transform.localScale = Vector3.one;
				uncovered.transform.position = Vector3.zero;
			}
			StartCoroutine (RepositionGrid (this.specificEventMiscGrid));
		}
	}
	private void ShowPowerGrabEvent(PowerGrab powerGrabEvent){
		//for started by
		List<Transform> children = specificEventStartedByGrid.GetChildList();
		if (children.Count <= 0 || (children.Count > 0 && children[0].GetComponent<CharacterPortrait>().citizen.id != powerGrabEvent.startedBy.id)) {
			for (int i = 0; i < children.Count; i++) {
				Destroy (children [i].gameObject);
			}
			GameObject startedByGO = GameObject.Instantiate (characterPortraitPrefab, specificEventStartedByGrid.transform) as GameObject;
			startedByGO.GetComponent<CharacterPortrait> ().SetCitizen (powerGrabEvent.startedBy);
			startedByGO.transform.localScale = Vector3.one;
			startedByGO.transform.position = Vector3.zero;
			StartCoroutine (RepositionGrid(specificEventStartedByGrid));
		}

		specificEventCandidatesTitleLbl.text = "EXHORTED";
		children = this.specificEventCandidatesGrid.GetChildList ();
		List<Citizen> currentlyShowingExhortedCitizens = children.Select (x => x.GetComponent<CharacterPortrait>().citizen).ToList();
		List<Citizen> additionalExhortedCitizens = powerGrabEvent.exhortedCitizens.Except(currentlyShowingExhortedCitizens).ToList();
		if (additionalExhortedCitizens.Count > 0 || children.Count <= 0) {
			for (int i = 0; i < children.Count; i++) {
				Destroy (children [i].gameObject);
			}

			for (int i = 0; i < powerGrabEvent.exhortedCitizens.Count; i++) {
				GameObject uncovered = GameObject.Instantiate (characterPortraitPrefab, this.specificEventCandidatesGrid.transform) as GameObject;
				uncovered.GetComponent<CharacterPortrait> ().SetCitizen (powerGrabEvent.exhortedCitizens[i]);
				uncovered.transform.localScale = Vector3.one;
				uncovered.transform.position = Vector3.zero;
			}
			StartCoroutine (RepositionGrid (this.specificEventCandidatesGrid));
		}

		//Uncovered
		specificEventMiscTitleLbl.text = "UNCOVERED";
		specificEventMiscTitleLbl.gameObject.SetActive(true);
		children = this.specificEventMiscGrid.GetChildList ();
		List<Citizen> currentlyShowingUncoveredCitizens = children.Select (x => x.GetComponent<CharacterPortrait>().citizen).ToList();
		List<Citizen> additionalCitizens = powerGrabEvent.uncovered.Except(currentlyShowingUncoveredCitizens).ToList();
		if (additionalCitizens.Count > 0 || children.Count <= 0) {
			for (int i = 0; i < children.Count; i++) {
				Destroy (children [i].gameObject);
			}

			for (int i = 0; i < powerGrabEvent.uncovered.Count; i++) {
				GameObject uncovered = GameObject.Instantiate (characterPortraitPrefab, this.specificEventMiscGrid.transform) as GameObject;
				uncovered.GetComponent<CharacterPortrait> ().SetCitizen (powerGrabEvent.uncovered [i]);
				uncovered.transform.localScale = Vector3.one;
				uncovered.transform.position = Vector3.zero;
			}
			StartCoroutine (RepositionGrid (this.specificEventMiscGrid));
		}
	}
	private void ShowJoinWarEvent(JoinWar joinWarEvent){
		//for started by
		List<Transform> children = specificEventStartedByGrid.GetChildList();
		if (children.Count <= 0 || (children.Count > 0 && children[0].GetComponent<CharacterPortrait>().citizen.id != joinWarEvent.startedBy.id)) {
			for (int i = 0; i < children.Count; i++) {
				Destroy (children [i].gameObject);
			}
			GameObject startedByGO = GameObject.Instantiate (characterPortraitPrefab, specificEventStartedByGrid.transform) as GameObject;
			startedByGO.GetComponent<CharacterPortrait> ().SetCitizen (joinWarEvent.startedBy);
			startedByGO.transform.localScale = Vector3.one;
			startedByGO.transform.position = Vector3.zero;
			StartCoroutine (RepositionGrid(specificEventStartedByGrid));
		}

		//for target
		specificEventCandidatesTitleLbl.text = "Target";
		children = specificEventCandidatesGrid.GetChildList();
		if (children.Count > 0) {
			Citizen currentlyShowingTarget = children[0].GetComponent<CharacterPortrait>().citizen;
			if (currentlyShowingTarget.id != joinWarEvent.candidateForAlliance.id) {
				children [0].GetComponent<CharacterPortrait>().SetCitizen (joinWarEvent.candidateForAlliance);
			}
		} else {
			GameObject targetGO = GameObject.Instantiate (characterPortraitPrefab, specificEventCandidatesGrid.transform) as GameObject;
			targetGO.GetComponent<CharacterPortrait> ().SetCitizen (joinWarEvent.candidateForAlliance);
			targetGO.transform.localScale = Vector3.one;
			targetGO.transform.position = Vector3.zero;

		}
		StartCoroutine (RepositionGrid(specificEventCandidatesGrid));

	}
	private void ShowInvasionPlanEvent(InvasionPlan invasionPlanEvent){
		//for bar
		this.specificEventBarTitle.text = "Militarization";
		specificEventProgBar.value = (float)((float)invasionPlanEvent.militarizationEvent.remainingDays / (float)invasionPlanEvent.militarizationEvent.durationInDays);

		//for started by
		List<Transform> children = specificEventStartedByGrid.GetChildList();
		if (children.Count <= 0 || (children.Count > 0 && children[0].GetComponent<CharacterPortrait>().citizen.id != invasionPlanEvent.startedBy.id)) {
			for (int i = 0; i < children.Count; i++) {
				Destroy (children [i].gameObject);
			}
			GameObject startedByGO = GameObject.Instantiate (characterPortraitPrefab, specificEventStartedByGrid.transform) as GameObject;
			startedByGO.GetComponent<CharacterPortrait> ().SetCitizen (invasionPlanEvent.startedBy);
			startedByGO.transform.localScale = Vector3.one;
			startedByGO.transform.position = Vector3.zero;
			StartCoroutine (RepositionGrid(specificEventStartedByGrid));
		}

		//for target
		specificEventCandidatesTitleLbl.text = "Target";
		children = specificEventCandidatesGrid.GetChildList();
		if (children.Count > 0) {
			Citizen currentlyShowingTarget = children[0].GetComponent<CharacterPortrait>().citizen;
			if (currentlyShowingTarget.id != invasionPlanEvent.targetKingdom.king.id) {
				children [0].GetComponent<CharacterPortrait>().SetCitizen (invasionPlanEvent.targetKingdom.king);
			}
		} else {
			GameObject targetGO = GameObject.Instantiate (characterPortraitPrefab, specificEventCandidatesGrid.transform) as GameObject;
			targetGO.GetComponent<CharacterPortrait> ().SetCitizen (invasionPlanEvent.targetKingdom.king);
			targetGO.transform.localScale = Vector3.one;
			targetGO.transform.position = Vector3.zero;

		}
		StartCoroutine (RepositionGrid(specificEventCandidatesGrid));

		//Uncovered
		specificEventMiscTitleLbl.text = "UNCOVERED";
		specificEventMiscTitleLbl.gameObject.SetActive(true);
		children = this.specificEventMiscGrid.GetChildList ();
		List<Citizen> currentlyShowingUncoveredCitizens = children.Select (x => x.GetComponent<CharacterPortrait>().citizen).ToList();
		List<Citizen> additionalCitizens = invasionPlanEvent.uncovered.Except(currentlyShowingUncoveredCitizens).ToList();
		if (additionalCitizens.Count > 0 || children.Count <= 0) {
			for (int i = 0; i < children.Count; i++) {
				Destroy (children [i].gameObject);
			}

			for (int i = 0; i < invasionPlanEvent.uncovered.Count; i++) {
				GameObject uncovered = GameObject.Instantiate (characterPortraitPrefab, this.specificEventMiscGrid.transform) as GameObject;
				uncovered.GetComponent<CharacterPortrait> ().SetCitizen (invasionPlanEvent.uncovered [i]);
				uncovered.transform.localScale = Vector3.one;
				uncovered.transform.position = Vector3.zero;
			}
			StartCoroutine (RepositionGrid (this.specificEventMiscGrid));
		}
	}
	private void ShowExpansionEvent(Expansion expansionEvent){
		//for started by
		List<Transform> children = specificEventStartedByGrid.GetChildList();
		if (children.Count <= 0 || (children.Count > 0 && children[0].GetComponent<CharacterPortrait>().citizen.id != expansionEvent.startedBy.id)) {
			for (int i = 0; i < children.Count; i++) {
				Destroy (children [i].gameObject);
			}
			GameObject startedByGO = GameObject.Instantiate (characterPortraitPrefab, specificEventStartedByGrid.transform) as GameObject;
			startedByGO.GetComponent<CharacterPortrait> ().SetCitizen (expansionEvent.startedBy);
			startedByGO.transform.localScale = Vector3.one;
			startedByGO.transform.position = Vector3.zero;
			StartCoroutine (RepositionGrid(specificEventStartedByGrid));
		}

		specificEventCandidatesTitleLbl.text = "";
	}
	private void ShowWarEvent(War warEvent){
		currentlyShowingWar = warEvent;
		warKingdom1Lbl.text = warEvent.kingdom1.name;
		warKingdom2Lbl.text = warEvent.kingdom2.name;

		warKingdom1ExhaustionBar.value = (float)warEvent.kingdom1Rel.kingdomWar.exhaustion / 100f;
		warKingdom2ExhaustionBar.value = (float)warEvent.kingdom2Rel.kingdomWar.exhaustion / 100f;

		warKingdom1BatWonLbl.text = warEvent.kingdom1Rel.kingdomWar.battlesWon.ToString();
		warKingdom1BatLostLbl.text = warEvent.kingdom1Rel.kingdomWar.battlesLost.ToString();
		warKingdom1CitiesWonLbl.text = warEvent.kingdom1Rel.kingdomWar.citiesWon.ToString();
		warKingdom1CitiesLostLbl.text = warEvent.kingdom1Rel.kingdomWar.citiesLost.ToString();

		warKingdom2BatWonLbl.text = warEvent.kingdom2Rel.kingdomWar.battlesWon.ToString();
		warKingdom2BatLostLbl.text = warEvent.kingdom2Rel.kingdomWar.battlesLost.ToString();
		warKingdom2CitiesWonLbl.text = warEvent.kingdom2Rel.kingdomWar.citiesWon.ToString();
		warKingdom2CitiesLostLbl.text = warEvent.kingdom2Rel.kingdomWar.citiesLost.ToString();


		specificEventNormalGO.SetActive (false);
		specificEventWarGO.SetActive (true);
	}
	private void ShowRequestPeace(RequestPeace requestPeace){
		//for started by
		List<Transform> children = specificEventStartedByGrid.GetChildList();
		if (children.Count <= 0 || (children.Count > 0 && children[0].GetComponent<CharacterPortrait>().citizen.id != requestPeace.startedBy.id)) {
			for (int i = 0; i < children.Count; i++) {
				Destroy (children [i].gameObject);
			}
			GameObject startedByGO = GameObject.Instantiate (characterPortraitPrefab, specificEventStartedByGrid.transform) as GameObject;
			startedByGO.GetComponent<CharacterPortrait> ().SetCitizen (requestPeace.startedBy);
			startedByGO.transform.localScale = Vector3.one;
			startedByGO.transform.position = Vector3.zero;
			StartCoroutine (RepositionGrid(specificEventStartedByGrid));
		}

		//for citizen sent
		specificEventCandidatesTitleLbl.text = "Citizen Sent";
		children = specificEventCandidatesGrid.GetChildList();
		if (children.Count > 0) {
			Citizen currentlyShowingTarget = children[0].GetComponent<CharacterPortrait>().citizen;
			if (currentlyShowingTarget.id != requestPeace.citizenSent.id) {
				children [0].GetComponent<CharacterPortrait>().SetCitizen (requestPeace.citizenSent);
			}
		} else {
			GameObject targetGO = GameObject.Instantiate (characterPortraitPrefab, specificEventCandidatesGrid.transform) as GameObject;
			targetGO.GetComponent<CharacterPortrait> ().SetCitizen (requestPeace.citizenSent);
			targetGO.transform.localScale = Vector3.one;
			targetGO.transform.position = Vector3.zero;

		}
		StartCoroutine (RepositionGrid(specificEventCandidatesGrid));

		//Saboteurs
		specificEventMiscTitleLbl.text = "Saboteurs";
		specificEventMiscTitleLbl.gameObject.SetActive(true);
		children = this.specificEventMiscGrid.GetChildList ();
		List<Citizen> currentlyShowingUncoveredCitizens = children.Select (x => x.GetComponent<CharacterPortrait>().citizen).ToList();
		List<Citizen> additionalCitizens = requestPeace.saboteurs.Except(currentlyShowingUncoveredCitizens).ToList();
		if (additionalCitizens.Count > 0 || children.Count <= 0) {
			for (int i = 0; i < children.Count; i++) {
				Destroy (children [i].gameObject);
			}

			for (int i = 0; i < requestPeace.saboteurs.Count; i++) {
				GameObject uncovered = GameObject.Instantiate (characterPortraitPrefab, this.specificEventMiscGrid.transform) as GameObject;
				uncovered.GetComponent<CharacterPortrait> ().SetCitizen (requestPeace.saboteurs [i]);
				uncovered.transform.localScale = Vector3.one;
				uncovered.transform.position = Vector3.zero;
			}
			StartCoroutine (RepositionGrid (this.specificEventMiscGrid));
		}
	}
	private void ClearSpecificEventUI(){
		Transform startedByGridItem = specificEventStartedByGrid.GetChild(0);
		List<Transform> candidatesGridItems = specificEventCandidatesGrid.GetChildList();
		List<Transform> miscGridItems = specificEventMiscGrid.GetChildList();
		if (startedByGridItem != null) {
			Destroy(startedByGridItem.gameObject);
		}

		for (int i = 0; i < candidatesGridItems.Count; i++) {
			Destroy(candidatesGridItems[i].gameObject);
		}

		for (int i = 0; i < miscGridItems.Count; i++) {
			Destroy(miscGridItems[i].gameObject);
		}
		lblSpecificSuccessRate.gameObject.SetActive(false);
		specificEventsCandidatesLbl.gameObject.SetActive(false);
		specificEventMiscTitleLbl.gameObject.SetActive(false);
		specificEventNormalGO.SetActive (true);
		specificEventWarGO.SetActive (false);
	}
	public void HideSpecificEvent(){
		specificEventGO.SetActive(false);
		ClearSpecificEventUI ();
	}
	#endregion*/

	/*
	 * Show Event Logs menu
	 * */
	public void ShowEventLogs(object obj){
		if (obj == null) {
			return;
		}

		List<Log> logs = new List<Log> ();
		if (obj is GameEvent) {
			GameEvent ge = ((GameEvent)obj);
			logs = ge.logs;
			elmEventTitleLbl.text = Utilities.LogReplacer(logs.FirstOrDefault());
            elmEventProgressBar.gameObject.SetActive(false);
            elmProgressBarLbl.gameObject.SetActive(false);
            //         if (ge.isActive) {
            //	if (ge.eventType == EVENT_TYPES.KINGDOM_WAR) {
            //		elmEventProgressBar.gameObject.SetActive (false);
            //	} else if (ge.eventType == EVENT_TYPES.EXPANSION) {
            //		elmEventProgressBar.gameObject.SetActive (false);
            //	} else {
            //		elmEventProgressBar.gameObject.SetActive (true);
            //		float targetValue = ((float)ge.remainingDays / (float)ge.durationInDays);
            //		if (currentlyShowingLogObject != null && ((GameEvent)currentlyShowingLogObject).id == ge.id) {
            //			currentLerpRoutine = StartCoroutine (LerpProgressBar (elmEventProgressBar, targetValue, GameManager.Instance.progressionSpeed));
            //		} else {
            //			if (currentLerpRoutine != null) {
            //				StopCoroutine (currentLerpRoutine);
            //				currentLerpRoutine = null;
            //			}
            //			elmEventProgressBar.value = targetValue;
            //		}

            //	}
            //} else {
            //	elmEventProgressBar.gameObject.SetActive (false);
            //}
        } else if (obj is Campaign) {
			logs = ((Campaign)obj).logs;
			elmEventTitleLbl.text = Utilities.LogReplacer(logs.FirstOrDefault());
			elmEventProgressBar.gameObject.SetActive (false);
		}
		//elmProgressBarLbl.text = "Progress:";
		elmSuccessRateGO.SetActive (false);

		currentlyShowingLogObject = obj;

		GameObject nextAnchorPoint = elmFirstAnchor;
		List<Log> currentlyShowingLogs = elmEventLogsParentGO.GetComponentsInChildren<EventLogItem>().Select(x => x.thisLog).ToList();
		if ((logs.Except (currentlyShowingLogs).Union (currentlyShowingLogs.Except (logs)).Count() - 1) > 0) {
			List<EventLogItem> children = elmEventLogsParentGO.GetComponentsInChildren<EventLogItem> ().ToList();
			for (int i = 0; i < children.Count; i++) {
				Destroy (children [i].gameObject);
			}
			for (int i = 1; i < logs.Count; i++) {
				GameObject logGO = InstantiateUIObject(logItemPrefab, elmEventLogsParentGO.transform);
				logGO.transform.localScale = Vector3.one;
				EventLogItem currELI = logGO.GetComponent<EventLogItem> ();
				currELI.SetLog (logs [i]);
				if (i % 2 == 0) {
					currELI.DisableBG();
				}
			}
			StartCoroutine(RepositionTable(elmEventLogsParentGO.GetComponent<UITable>()));
            StartCoroutine(RepositionScrollView(elmScrollView));
        }
		eventLogsGO.SetActive(true);
	}

	public void HideEventLogs(){
		eventLogsGO.SetActive(false);
		currentlyShowingLogObject = null;
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
        List<GameEvent> allActiveEventsInKingdom = EventManager.Instance.GetAllEventsKingdomIsInvolvedIn(currentlyShowingKingdom).Where(x => x.isActive).ToList();
        List<GameEvent> politicalEvents = allActiveEventsInKingdom.Where(x => x.eventType == EVENT_TYPES.STATE_VISIT || x.eventType == EVENT_TYPES.RAID ||
           x.eventType == EVENT_TYPES.ASSASSINATION || x.eventType == EVENT_TYPES.DIPLOMATIC_CRISIS || x.eventType == EVENT_TYPES.BORDER_CONFLICT).ToList();

        List<GameEvent> wars = allActiveEventsInKingdom.Where(x => x.eventType == EVENT_TYPES.KINGDOM_WAR).ToList();

        if (politicalEvents.Count <= 0 && wars.Count <= 0) {
            kingdomNoCurrentEventsLbl.gameObject.SetActive(true);
            allKingdomEventsGO.SetActive(true);
            //			this.pauseBtn.SetAsClicked ();
            //			GameManager.Instance.SetPausedState (true);
            EventListParent[] currentParents = Utilities.GetComponentsInDirectChildren<EventListParent>(kingdomCurrentEventsContentParent.gameObject);
            if (currentParents.Length > 0) {
                for (int i = 0; i < currentParents.Length; i++) {
                    EventListParent currentParent = currentParents[i];
                    Destroy(currentParent.gameObject);
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
        List<GameEvent> allDoneEvents = EventManager.Instance.GetAllEventsKingdomIsInvolvedIn(currentlyShowingKingdom).
            Where(x => !x.isActive && (x.eventType == EVENT_TYPES.STATE_VISIT || x.eventType == EVENT_TYPES.RAID ||
                x.eventType == EVENT_TYPES.ASSASSINATION || x.eventType == EVENT_TYPES.DIPLOMATIC_CRISIS || x.eventType == EVENT_TYPES.BORDER_CONFLICT ||
                x.eventType == EVENT_TYPES.KINGDOM_WAR)).ToList();
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
            GameObject eventGO = InstantiateUIObject(kingdomEventsListItemPrefab, this.transform);
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
				Destroy(politicsParentGO);
			}
			return;
		}

		if (politicsParentGO == null) {
			//Instantiate Politics Parent
			politicsParentGO = InstantiateUIObject(kingdomEventsListParentPrefab, kingdomCurrentEventsContentParent.transform) as GameObject;
			politicsParentGO.name = "PoliticsParent";
			politicsParentGO.transform.localScale = Vector3.one;
			politicsParentGO.transform.localPosition = Vector3.zero;
			politicsParentGO.GetComponent<EventListParent>().eventTitleLbl.text = "Politics";
		}
		
		politicsParent = politicsParentGO.GetComponent<EventListParent>();

		List<EventListItem> currentlyShowingGameEvents = politicsParent.eventsGrid.GetChildList ().Select (x => x.GetComponent<EventListItem>()).ToList ();
		List<int> currentlyShowingGameEventIDs = currentlyShowingGameEvents.Select (x => x.gameEvent.id).ToList ();
		List<int> actualPoliticalEventIDs = politicalEvents.Select (x => x.id).ToList ();
		if (actualPoliticalEventIDs.Except (currentlyShowingGameEventIDs).Union (currentlyShowingGameEventIDs.Except (actualPoliticalEventIDs)).Count () > 0) {
			for (int i = 0; i < currentlyShowingGameEvents.Count; i++) {
				politicsParent.eventsGrid.RemoveChild (currentlyShowingGameEvents[i].transform);
				Destroy (currentlyShowingGameEvents[i].gameObject);
			}

			//Instantiate all polical events into the politics parent grid
			for (int i = 0; i < politicalEvents.Count; i++) {
				GameObject eventGO = InstantiateUIObject(kingdomEventsListItemPrefab, this.transform);
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
                        Destroy(currWarParent.gameObject);
                    }
                    
                }
				
			}

			if (wars.Count <= 0) {
				if (warParentGO == null) {
					Destroy(warParentGO);
				}
				return;
			}

			if (warParentGO == null) {
				warParentGO = InstantiateUIObject(kingdomWarEventsListParentPrefab, kingdomCurrentEventsContentParent.transform);
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

	//public void ToggleKingdomHistory(){
	//	if (kingdomHistoryGO.activeSelf) {
	//		HideKingdomHistory();
	//	} else {
	//		ShowKingdomHistory();
	//	}
	//}

	/*
	 * Show past events started by the
	 * currentlyShowingKingdom
	 * */
	//public void ShowKingdomHistory(){
	//	HideAllKingdomEvents();
 //       HideKingdomCities();
	//	List<GameEvent> allDoneEvents = EventManager.Instance.GetAllEventsKingdomIsInvolvedIn(currentlyShowingKingdom).
	//		Where(x => !x.isActive && (x.eventType == EVENT_TYPES.STATE_VISIT || x.eventType == EVENT_TYPES.RAID ||
	//			x.eventType == EVENT_TYPES.ASSASSINATION || x.eventType == EVENT_TYPES.DIPLOMATIC_CRISIS || x.eventType == EVENT_TYPES.BORDER_CONFLICT ||
	//			x.eventType == EVENT_TYPES.KINGDOM_WAR)).ToList();
	//	allDoneEvents = allDoneEvents.OrderByDescending(x => x.startDate).ToList();

	//	List<EventListItem> currentItems = kingdomHistoryGrid.GetChildList ().Select(x => x.GetComponent<EventListItem>()).ToList();
	//	int nextItem = 0;
	//	for (int i = 0; i < currentItems.Count; i++) {
	//		EventListItem currItem = currentItems [i];
	//		if (i < allDoneEvents.Count) {
	//			GameEvent gameEventToShow = allDoneEvents [i];
	//			if (gameEventToShow != null) {
	//				currItem.SetEvent (gameEventToShow, currentlyShowingKingdom);
	//				currItem.gameObject.SetActive (true);
	//				nextItem = i + 1;
	//			} else {
	//				currItem.gameObject.SetActive (false);
	//			}
	//		} else {
	//			currItem.gameObject.SetActive (false);
	//		}
	//	}

	//	for (int i = nextItem; i < allDoneEvents.Count; i++) {
	//		GameObject eventGO = InstantiateUIObject(kingdomEventsListItemPrefab, this.transform);
	//		eventGO.transform.localScale = Vector3.one;
	//		kingdomHistoryGrid.AddChild(eventGO.transform);
	//		eventGO.GetComponent<EventListItem>().SetEvent(allDoneEvents[i], currentlyShowingKingdom);
	//		eventGO.GetComponent<EventListItem>().onClickEvent += ShowEventLogs;
	//		kingdomHistoryGrid.Reposition();
	//	}

	//	if (allDoneEvents.Count <= 0) {
	//		kingdomHistoryNoEventsLbl.gameObject.SetActive(true);
	//	} else {
	//		kingdomHistoryNoEventsLbl.gameObject.SetActive(false);
	//	}
	//	kingdomHistoryGO.SetActive(true);
	//}

	//public void HideKingdomHistory(){
	//	kingdomHistoryGO.SetActive(false);
 //       kingdomListRelationshipButton.SetClickState(false);
	//}

    public void ToggleKingdomCities() {
        if (kingdomCitiesGO.activeSelf) {
            HideKingdomCities();
        } else {
            ShowKingdomCities();
        }
    }

    /*
     * Show all cities owned by currentlyShowingKingdom.
     * */
    public void ShowKingdomCities() {
        //HideKingdomHistory();
        HideRelationships();
        HideAllKingdomEvents();
        List<CityItem> cityItems = kingdomCitiesGrid.gameObject.GetComponentsInChildren<Transform>(true)
            .Where(x => x.GetComponent<CityItem>() != null)
            .Select(x => x.GetComponent<CityItem>()).ToList();
        int nextIndex = 0;
        for (int i = 0; i < cityItems.Count; i++) {
            CityItem currCityItem = cityItems[i];
            if(i < currentlyShowingKingdom.cities.Count) {
                City currCity = currentlyShowingKingdom.cities.ElementAt(i);
                if (currCity != null) {
                    currCityItem.SetCity(currCity, true);
                    currCityItem.gameObject.SetActive(true);
                } else {
                    currCityItem.gameObject.SetActive(false);
                }
                nextIndex = i + 1;
            } else {
                currCityItem.gameObject.SetActive(false);
            }


        }

        if (currentlyShowingKingdom.cities.Count >= nextIndex) {
            for (int i = nextIndex; i < currentlyShowingKingdom.cities.Count; i++) {
                City currCity = currentlyShowingKingdom.cities[i];
                GameObject cityGO = InstantiateUIObject(cityItemPrefab, this.transform);
                cityGO.GetComponent<CityItem>().SetCity(currCity, true);
                cityGO.transform.localScale = Vector3.one;
                kingdomCitiesGrid.AddChild(cityGO.transform);
                kingdomCitiesGrid.Reposition();
            }
            StartCoroutine(RepositionScrollView(kingdomCitiesScrollView));
            //kingdomCitiesScrollView.ResetPosition();
        }
        //kingdomCitiesScrollView.UpdatePosition();
        //kingdomCitiesScrollView.UpdateScrollbars();
        kingdomCitiesGO.SetActive(true);
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
            GameObject playerEventItemGO = InstantiateUIObject(playerEventItemPrefab, this.transform);
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
		}
		return assassinationIcon;
	}

	/*
	 * Checker for if the mouse is currently
	 * over a UI Object
	 * */
	public bool IsMouseOnUI(){
        if (uiCamera != null) {
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
	internal GameObject InstantiateUIObject(GameObject prefabObj, Transform parent){
		GameObject go = GameObject.Instantiate (prefabObj, parent) as GameObject;
		UILabel[] goLbls = go.GetComponentsInChildren<UILabel>(true);
		for (int i = 0; i < goLbls.Length; i++) {
			NormalizeFontSizeOfLabel(goLbls[i]);
		}
		return go;
	}





//------------------------------------------------------------------------------------------- FOR TESTING ---------------------------------------------------------------------

	public void ToggleResourceIcons(){
		CameraMove.Instance.ToggleResourceIcons();
	}

	public void ToggleGeneralCamera(){
		CameraMove.Instance.ToggleGeneralCamera();
	}

	public void ToggleTraderCamera(){
		CameraMove.Instance.ToggleTraderCamera();
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
	public void ToggleTraitEditor(){
		if (traitEditorGO.activeSelf) {
			traitEditorGO.SetActive (false);
		} else {
			addTraitPopUpList.Clear();
			removeTraitPopUpList.Clear();
			TRAIT[] allTraits = Utilities.GetEnumValues<TRAIT>();
//			SKILL_TRAIT[] allSkillTraits = Utilities.GetEnumValues<SKILL_TRAIT>();
//			MISC_TRAIT[] allMiscTraits = Utilities.GetEnumValues<MISC_TRAIT>();

			for (int i = 0; i < allTraits.Length; i++) {
				if (allTraits[i] != TRAIT.NONE && !currentlyShowingCitizen.hasTrait(allTraits[i])) {
					addTraitPopUpList.AddItem (allTraits[i].ToString (), allTraits[i]);
				}
			}
            //			for (int i = 0; i < allSkillTraits.Length; i++) {
            //				if (allSkillTraits[i] != SKILL_TRAIT.NONE) {
            //					addTraitPopUpList.AddItem (allSkillTraits[i].ToString ());
            //				}
            //			}
            //			for (int i = 0; i < allMiscTraits.Length; i++) {
            //				if (allMiscTraits[i] != MISC_TRAIT.NONE) {
            //					addTraitPopUpList.AddItem (allMiscTraits[i].ToString ());
            //				}
            //			}

            //			for (int i = 0; i < currentlyShowingCitizen.behaviorTraits.Count; i++) {
            //				removeTraitPopUpList.AddItem (currentlyShowingCitizen.behaviorTraits[i].ToString());
            //			}
            //			for (int i = 0; i < currentlyShowingCitizen.skillTraits.Count; i++) {
            //				removeTraitPopUpList.AddItem (currentlyShowingCitizen.skillTraits[i].ToString());
            //			}
            //			for (int i = 0; i < currentlyShowingCitizen.miscTraits.Count; i++) {
            //				removeTraitPopUpList.AddItem (currentlyShowingCitizen.miscTraits[i].ToString());
            //			}
   //         removeTraitPopUpList.AddItem(currentlyShowingCitizen.honestyTrait.ToString());
   //         removeTraitPopUpList.AddItem(currentlyShowingCitizen.hos.ToString());
   //         removeTraitPopUpList.AddItem(currentlyShowingCitizen.honestyTrait.ToString());
   //         if (removeTraitPopUpList.items.Count > 0) {
			//	removeTraitPopUpList.value = removeTraitPopUpList.items [0];
			//} else {
			//	removeTraitPopUpList.value = "";
			//}
			addTraitPopUpList.value = addTraitPopUpList.items [0];
			//removeTraitChoiceLbl.text = removeTraitPopUpList.value;
			addTraitChoiceLbl.text = addTraitPopUpList.value;
			traitEditorGO.SetActive (true);
		}
	}
	public void AddTrait(){
		TRAIT chosenTrait = (TRAIT)addTraitPopUpList.data;
		if (chosenTrait == TRAIT.HONEST || chosenTrait == TRAIT.SCHEMING) {
			currentlyShowingCitizen.SetHonestyTrait (chosenTrait);
		} else if (chosenTrait == TRAIT.WARMONGER || chosenTrait == TRAIT.PACIFIST) {
			currentlyShowingCitizen.SetHostilityTrait (chosenTrait);
		}else if (chosenTrait == TRAIT.SMART || chosenTrait == TRAIT.STUPID || chosenTrait == TRAIT.AMBITIOUS) {
			currentlyShowingCitizen.SetMiscTrait (chosenTrait);
		}

		ShowCitizenInfo(currentlyShowingCitizen);
		ToggleTraitEditor();
	}

	public void RemoveTrait(){
		return;
//		if (string.IsNullOrEmpty (removeTraitPopUpList.value)) {
//			return;
//		}
//		BEHAVIOR_TRAIT chosenBehaviourTrait = BEHAVIOR_TRAIT.NONE;
//		SKILL_TRAIT chosenSkillTrait = SKILL_TRAIT.NONE;
//		MISC_TRAIT chosenMiscTrait = MISC_TRAIT.NONE;
//		if (chosenBehaviourTrait.TryParse<BEHAVIOR_TRAIT> (removeTraitPopUpList.value, out chosenBehaviourTrait)) {
//			currentlyShowingCitizen.behaviorTraits.Remove(chosenBehaviourTrait);
//			Debug.Log ("Removed behaviour trait : " + chosenBehaviourTrait.ToString () + " from " + currentlyShowingCitizen.name);
//		} else if (chosenSkillTrait.TryParse<SKILL_TRAIT> (removeTraitPopUpList.value, out chosenSkillTrait)) {
//			currentlyShowingCitizen.skillTraits.Remove(chosenSkillTrait);
//			Debug.Log ("Removed skill trait : " + chosenSkillTrait.ToString () + " from " + currentlyShowingCitizen.name);
//		} else if (chosenMiscTrait.TryParse<MISC_TRAIT> (removeTraitPopUpList.value, out chosenMiscTrait)) {
//			currentlyShowingCitizen.miscTraits.Remove(chosenMiscTrait);
//			if(chosenMiscTrait == MISC_TRAIT.TACTICAL){
//				currentlyShowingCitizen.campaignManager.campaignLimit = 2;
//			}else if(chosenMiscTrait == MISC_TRAIT.ACCIDENT_PRONE){
////				currentlyShowingCitizen.citizenChances.accidentChance = currentlyShowingCitizen.citizenChances.defaultAccidentChance;
//			}
//			Debug.Log ("Removed misc trait : " + chosenMiscTrait.ToString () + " from " + currentlyShowingCitizen.name);
//		}
//
//		ShowCitizenInfo(currentlyShowingCitizen);
//		ToggleTraitEditor();
//		if (currentlyShowingCitizen.city != null) {
//			currentlyShowingCitizen.city.UpdateResourceProduction();
//			currentlyShowingCitizen.city.UpdateCitizenCreationTable();
//		}
	}

	public void HideTraitEditor(){
		traitEditorGO.SetActive (false);
	}
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
				for(int i = 0; i < this.currentlyShowingCitizen.city.kingdom.relationshipsWithOtherKingdoms.Count; i++){
					if(!this.currentlyShowingCitizen.city.kingdom.relationshipsWithOtherKingdoms[i].isAtWar){
						this.kingdomsForWar.AddItem (this.currentlyShowingCitizen.city.kingdom.relationshipsWithOtherKingdoms[i].targetKingdom.name, this.currentlyShowingCitizen.city.kingdom.relationshipsWithOtherKingdoms[i].targetKingdom);
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
				this.unrestInput.value = this.currentlyShowingKingdom.unrest.ToString();
				this.unrestGO.SetActive (true);
			}
		}
	}
	public void OnChangeUnrest(){
		if(this.currentlyShowingKingdom != null){
			this.currentlyShowingKingdom.ChangeUnrest(int.Parse(this.unrestInput.value));
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

    public void ChangeGovernorLoyalty() {
        ((Governor)currentlyShowingCity.governor.assignedRole).SetLoyalty(Int32.Parse(forTestingLoyaltyLbl.text));
        Debug.Log("Changed loyalty of: " + currentlyShowingCity.governor.name + " to " + ((Governor)currentlyShowingCity.governor.assignedRole).loyalty.ToString());
    }

    public void LogRelatives() {
        List<Citizen> allRelatives = currentlyShowingCitizen.GetRelatives(-1);
        Debug.Log("========== " + currentlyShowingCitizen.name + " Relatives ==========");
        for (int i = 0; i < allRelatives.Count; i++) {
            Debug.Log("Relative: " + allRelatives[i].name);
        }
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
			if(WorldEventManager.Instance.currentPlague == null){
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
            } catch(Exception e) {
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

    #endregion
}
