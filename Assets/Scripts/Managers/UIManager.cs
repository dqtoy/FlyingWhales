using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class UIManager : MonoBehaviour {

	public static UIManager Instance = null;

	public UICamera uiCamera;

	[Space(10)]//Prefabs
	public GameObject characterPortraitPrefab;
	public GameObject successionPortraitPrefab;
	public GameObject historyPortraitPrefab;
	public GameObject eventPortraitPrefab;
	public GameObject governorPortraitPrefab;
	public GameObject traitPrefab;
	public GameObject gameEventPrefab;

	[Space(10)]//Main Objects
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
	public GameObject specificEventGO;
	public GameObject citizenHistoryGO;

	[Space(10)]//For Testing
	//Trait Editor
	public GameObject traitEditorGO;
	public UIPopupList addTraitPopUpList;
	public UIPopupList removeTraitPopUpList;
	public UILabel addTraitChoiceLbl;
	public UILabel removeTraitChoiceLbl;
	//Relationship Editor
	public UILabel sourceKinglikenessLbl;
	public UILabel targetKinglikenessLbl;

	[Space(10)]//World UI
	public ButtonToggle pauseBtn;
	public ButtonToggle x1Btn;
	public ButtonToggle x2Btn;
	public ButtonToggle x4Btn;

	[Space(10)]
	public UILabel dateLbl;
	public UILabel smallInfoLbl;
	public UI2DSprite ctizenPortraitBG;
	public UIGrid kingsGrid;
	public GameObject citizenInfoIsDeadIcon;

	[Space(10)]//Citizen Info UI
	public UILabel citizenNameLbl;
	public UILabel citizenKingdomNameLbl;
	public UILabel citizenBdayLbl;
	public UILabel citizenCityNameLbl;
	public UILabel citizenRoleLbl;
	public UILabel citizenPrestigeLbl;
	public UIGrid citizenTraitsGrid;
	public UIGrid citizenHistoryGrid;
	public GameObject kingSpecificGO;
	public ButtonToggle relationshipsBtn;
	public ButtonToggle familyTreeBtn;
	public ButtonToggle successionBtn;
	public ButtonToggle citizenHistoryBtn;
	public UIGrid citizenInfoSuccessionGrid;
	public GameObject citizenInfoSuccessionGO;
	public GameObject citizenInfoForTestingGO;
	public GameObject citizenInfoNoSuccessorsGO;

	[Space(10)] //City Info UI
	public UILabel cityNameLbl;
	public UILabel cityGovernorLbl;
	public UILabel cityKingdomLbl;
	public UILabel cityGoldLbl;
	public UILabel cityStoneLbl;
	public UILabel cityLumberLbl;
	public UILabel cityManaStoneLbl;
	public UILabel cityCobaltLbl;
	public UILabel cityMithrilLbl;
	public UILabel cityFoodLbl;
	public UIGrid foodProducersGrid;
	public UIGrid gatherersGrid;
	public UIGrid minersGrid;
	public UIGrid tradersGrid;
	public UIGrid generalsGrid;
	public UIGrid spiesGrid;
	public UIGrid envoysGrid;
	public UIGrid guardiansGrid;
	public UIGrid untrainedGrid;
	public UI2DSprite cityInfoCtizenPortraitBG;
	public GameObject citizensBtn;
	public GameObject cityInfoCitizensParent;
	public GameObject cityInfoHistoryParent;
	public UIGrid cityInfoHistoryGrid;
	public GameObject cityInfoEventsParent;
	public UIGrid cityInfoEventsGrid;
	public GameObject noEventsGO;

	[Space(10)]
	public GameObject kingdomSuccessionGO;
	public GameObject kingdomHistoryGO;
	public GameObject kingdomEventsGO;
	public GameObject kingdomGovernorsGO;
	public UILabel kingdomNameBigLbl;
	public UILabel kingdomNameSmallLbl;
	public UILabel kingdomKingNameLbl;
	public UILabel kingdomCityCountLbl;
	public UILabel kingdomGeneralCountLbl;
	public UILabel kingdomWarsLbl;
	public UIGrid kingdomSuccessionGrid;
	public UIGrid kingdomHistoryGrid;
	public UIGrid kingdomEventsGrid;
	public UIGrid kingdomGovernorsGrid;

	[Space(10)] //Events UI
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

	[Space(10)] //Relationship UI
	public GameObject kingRelationshipsParentGO;
	public GameObject governorRelationshipsParentGO;
	public UIGrid kingRelationshipsGrid;
	public UIGrid governorsRelationshipGrid;
	public UI2DSprite kingMainLineSprite;
	public UI2DSprite governorMainLineSprite;
	public UI2DSprite relationshipKingSprite;
	public UILabel relationshipKingName;
	public UILabel relationshipKingKingdomName;
	public ButtonToggle kingRelationshipsBtn;
	public ButtonToggle governorRelationshipsBtn;

	[Space(10)] //Relationship History UI
	public UI2DSprite relationshipStatusSprite;
	public UIGrid relationshipHistoryGrid;
	public GameObject noRelationshipsToShowGO;
	public GameObject relationshipHistoryForTestingGO;

	[Space(10)]
	public GameObject familyTreeFatherGO;
	public GameObject familyTreeMotherGO;
	public GameObject familyTreeSpouseGO;
	public UIGrid familyTreeChildGrid;
	public UI2DSprite familyTreeInnerSprite;
	public GameObject nextMarriageBtn;

	[Space(10)] //Specific Event UI
	public UILabel specificEventNameLbl;
	public UILabel specificEventStartDateLbl;
	public UILabel specificEventDescriptionLbl;
	public UILabel specificEventBarTitle;
	public UIProgressBar specificEventProgBar;
	public UIGrid specificEventStartedByGrid;
	public UILabel specificEventCandidatesTitleLbl;
	public UIGrid specificEventCandidatesGrid;
	public UILabel specificEventMiscTitleLbl;
	public UIGrid specificEventMiscGrid;
	public UILabel specificEventResolutionLbl;
	public GameObject goSpecificEventHidden;
	public GameObject goSpecificHiddenEventItem;
	public UILabel lblSpecificPilfered;
	public UILabel lblSpecificEventCity;
	public UILabel lblSpecificSuccessRate;
	public UILabel specificEventsCandidatesLbl;

	//For War UI
	public GameObject specificEventNormalGO;
	public GameObject specificEventWarGO;
	public UILabel warKingdom1Lbl;
	public UILabel warKingdom1BatWonLbl;
	public UILabel warKingdom1BatLostLbl;
	public UILabel warKingdom1CitiesWonLbl;
	public UILabel warKingdom1CitiesLostLbl;
	public UIProgressBar warKingdom1ExhaustionBar;
	public UILabel warKingdom2Lbl;
	public UILabel warKingdom2BatWonLbl;
	public UILabel warKingdom2BatLostLbl;
	public UILabel warKingdom2CitiesWonLbl;
	public UILabel warKingdom2CitiesLostLbl;
	public UIProgressBar warKingdom2ExhaustionBar;

	private List<MarriedCouple> marriageHistoryOfCurrentCitizen;
	private int currentMarriageHistoryIndex;
	private Citizen currentlyShowingCitizen;
	internal City currentlyShowingCity;
	private Kingdom currentlyShowingKingdom;
	private GameEvent currentlyShowingEvent;
	private RelationshipKings currentlyShowingRelationship;
	private GameObject lastClickedEventType = null;

	[Space(10)] //FOR TESTING
	public GameObject goCreateEventUI;
	public GameObject goRaid;
	public GameObject goStateVisit;
	public GameObject goMarriageInvitation;
	public GameObject goPowerGrab;
	public GameObject goExpansion;
	public UIPopupList eventDropdownList;
	public UILabel eventDropdownCurrentSelectionLbl;


	void Awake(){
		Instance = this;
	}

	void Start(){
		EventManager.Instance.onUpdateUI.AddListener(UpdateUI);
		dateLbl.text = "[b]" + ((MONTH)GameManager.Instance.month).ToString () + " " + GameManager.Instance.week.ToString () + ", " + GameManager.Instance.year.ToString () + "[/b]";
	}

	private void UpdateUI(){
		dateLbl.text = "[b]" + ((MONTH)GameManager.Instance.month).ToString () + " " + GameManager.Instance.week.ToString () + ", " + GameManager.Instance.year.ToString () + "[/b]";
		if (cityInfoGO.activeSelf) {
			if (currentlyShowingCity != null) {
				this.ShowCityInfo (currentlyShowingCity);
			}
		}

		if (specificEventGO.activeSelf) {
			if (currentlyShowingEvent != null) {
				this.ShowSpecificEvent (currentlyShowingEvent);
			}
		}

		if (eventsOfTypeGo.activeSelf) {
			if (lastClickedEventType != null) {
				this.UpdateEventsOfType();
			}
		}

	}

	public void SetProgressionSpeed1X(){
		GameManager.Instance.SetProgressionSpeed(4f);
		if (pauseBtn.isClicked) {
			this.TogglePause();
			pauseBtn.OnClick();
		} else if (x2Btn.isClicked) {
			x2Btn.OnClick();
		} else if (x4Btn.isClicked) {
			x4Btn.OnClick();
		}
	}

	public void SetProgressionSpeed2X(){
		GameManager.Instance.SetProgressionSpeed(2f);
		if (pauseBtn.isClicked) {
			this.TogglePause();
			pauseBtn.OnClick();
		} else if (x1Btn.isClicked) {
			x1Btn.OnClick();
		} else if (x4Btn.isClicked) {
			x4Btn.OnClick();
		}
	}

	public void SetProgressionSpeed4X(){
		GameManager.Instance.SetProgressionSpeed(1f);
//		GameManager.Instance.SetProgressionSpeed(0.5f);
		if (pauseBtn.isClicked) {
			this.TogglePause();
			pauseBtn.OnClick();
		} else if (x2Btn.isClicked) {
			x2Btn.OnClick();
		} else if (x1Btn.isClicked) {
			x1Btn.OnClick();
		}
	}

	public void TogglePause(){
		GameManager.Instance.TogglePause();
		if (x1Btn.isClicked) {
			x1Btn.OnClick();
		} else if (x2Btn.isClicked) {
			x2Btn.OnClick();
		} else if (x4Btn.isClicked) {
			x4Btn.OnClick();
		}
	}

	internal void UpdateKingsGrid(){
		List<Transform> children = kingsGrid.GetChildList();
		for (int i = 0; i < children.Count; i++) {
			Destroy (children [i].gameObject);
		}

		for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
			GameObject kingGO = GameObject.Instantiate (characterPortraitPrefab, kingsGrid.transform) as GameObject;
			kingGO.GetComponent<CharacterPortrait>().SetCitizen(KingdomManager.Instance.allKingdoms[i].king);
			kingGO.transform.localScale = Vector3.one;
		}
		StartCoroutine (RepositionGrid (kingsGrid));
		kingsGrid.enabled = true;
	}

	internal void ShowCitizenInfo(Citizen citizenToShow){
		if (relationshipsGO.activeSelf) {
			HideRelationships();
		}
		if (familyTreeGO.activeSelf) {
			HideFamilyTree();
		}
		if (citizenHistoryGO.activeSelf) {
			HideCitizenHistory();
		}
		if (traitEditorGO.activeSelf) {
			HideTraitEditor();
		}

		//ForTesting
		citizenInfoForTestingGO.SetActive (true);

		successionBtn.SetClickState(false);
		citizenInfoSuccessionGO.SetActive(false);

		HideSmallInfo();

		currentlyShowingCitizen = citizenToShow;
		citizenNameLbl.text = "[b]" + citizenToShow.name + "[/b]";
		if (citizenToShow.city != null) {
			citizenKingdomNameLbl.text = citizenToShow.city.kingdom.name;
			citizenCityNameLbl.text = "[b]" + citizenToShow.city.name + "[/b]";
		} else {
			citizenKingdomNameLbl.text = "[i]No Kingdom[/i]";
			citizenCityNameLbl.text = "[i] No City [/i]";
		}


		citizenBdayLbl.text = "[b]" + ((MONTH)citizenToShow.birthMonth).ToString() + " " + citizenToShow.birthWeek.ToString() + ", " + citizenToShow.birthYear.ToString() + "(" + citizenToShow.age.ToString() + ")[/b]";

		citizenRoleLbl.text = "[b]" + citizenToShow.role.ToString() + "[/b]";
		citizenPrestigeLbl.text = "[b]" + citizenToShow.prestige.ToString() + "[/b]";

		if (citizenToShow.isDead) {
			citizenInfoIsDeadIcon.SetActive (true);
		} else {
			citizenInfoIsDeadIcon.SetActive (false);
		}


		List<Transform> children = citizenTraitsGrid.GetChildList();
		for (int i = 0; i < children.Count; i++) {
			Destroy (children [i].gameObject);
		}

		for (int i = 0; i < citizenToShow.behaviorTraits.Count; i++) {
			GameObject traitGO = GameObject.Instantiate (traitPrefab, citizenTraitsGrid.transform) as GameObject;
			traitGO.GetComponent<TraitObject>().SetTrait(citizenToShow.behaviorTraits[i], SKILL_TRAIT.NONE, MISC_TRAIT.NONE);
			traitGO.transform.localScale = Vector3.one;
			traitGO.transform.localPosition = Vector3.zero;
		}

		for (int i = 0; i < citizenToShow.skillTraits.Count; i++) {
			GameObject traitGO = GameObject.Instantiate (traitPrefab, citizenTraitsGrid.transform) as GameObject;
			traitGO.GetComponent<TraitObject>().SetTrait(BEHAVIOR_TRAIT.NONE, citizenToShow.skillTraits[i], MISC_TRAIT.NONE);
			traitGO.transform.localScale = Vector3.one;
			traitGO.transform.localPosition = Vector3.zero;
		}

		for (int i = 0; i < citizenToShow.miscTraits.Count; i++) {
			GameObject traitGO = GameObject.Instantiate (traitPrefab, citizenTraitsGrid.transform) as GameObject;
			traitGO.GetComponent<TraitObject>().SetTrait(BEHAVIOR_TRAIT.NONE, SKILL_TRAIT.NONE, citizenToShow.miscTraits[i]);
			traitGO.transform.localScale = Vector3.one;
			traitGO.transform.localPosition = Vector3.zero;
		}
		StartCoroutine (RepositionGrid (citizenTraitsGrid));

		if (citizenToShow.isKing) {
			kingSpecificGO.SetActive(true);
		} else {
			kingSpecificGO.SetActive(false);
		}

		if (citizenToShow.city != null) {
			ctizenPortraitBG.color = citizenToShow.city.kingdom.kingdomColor;
		} else {
			ctizenPortraitBG.color = Color.white;
		}


		citizenInfoGO.SetActive (true);

		if(citizenToShow.isKing){
			ShowKingdomInfo (citizenToShow.city.kingdom);
		}

		this.marriageHistoryOfCurrentCitizen = MarriageManager.Instance.GetCouplesCitizenInvoledIn(citizenToShow);
	}

	public void ToggleSuccessionLine(){
		if (citizenInfoSuccessionGO.activeSelf) {
			citizenInfoSuccessionGO.SetActive(false);
		} else {
			//CLEAR SUCCESSION
			List<Transform> children = citizenInfoSuccessionGrid.GetChildList ();
			for (int i = 0; i < children.Count; i++) {
				Destroy (children [i].gameObject);
			}

			//POPULATE
			for (int i = 0; i < currentlyShowingCitizen.city.kingdom.successionLine.Count; i++) {
				if (i > 2) {
					break;
				}
				GameObject citizenGO = GameObject.Instantiate (successionPortraitPrefab, citizenInfoSuccessionGrid.transform) as GameObject;
				citizenGO.GetComponent<SuccessionPortrait> ().SetCitizen (currentlyShowingCitizen.city.kingdom.successionLine [i], currentlyShowingCitizen.city.kingdom);
				citizenGO.transform.localScale = Vector3.one;
				citizenGO.transform.localPosition = Vector3.zero;
			}

			for (int i = 0; i < currentlyShowingCitizen.city.kingdom.pretenders.Count; i++) {
				GameObject citizenGO = GameObject.Instantiate (successionPortraitPrefab, citizenInfoSuccessionGrid.transform) as GameObject;
				citizenGO.GetComponent<SuccessionPortrait> ().SetCitizen (currentlyShowingCitizen.city.kingdom.pretenders [i], currentlyShowingCitizen.city.kingdom);
				citizenGO.transform.localScale = Vector3.one;
				citizenGO.transform.localPosition = Vector3.zero;

			}
			StartCoroutine (RepositionGrid (citizenInfoSuccessionGrid));
			if (citizenInfoSuccessionGrid.GetChildList ().Count <= 0) {
				citizenInfoNoSuccessorsGO.SetActive (true);
			} else {
				citizenInfoNoSuccessorsGO.SetActive (false);
			}

			citizenInfoSuccessionGO.SetActive (true);
		}

	}

	public void HideCitizenInfo(){
		currentlyShowingCitizen = null;
		citizenInfoGO.SetActive(false);
		citizenInfoSuccessionGO.SetActive(false);
		familyTreeGO.SetActive(false);
		relationshipsGO.SetActive(false);
		relationshipHistoryGO.SetActive(false);
		citizenInfoSuccessionGO.SetActive(false);
	}
	public void ToggleCitizenHistory(){
		if (this.citizenHistoryGO.activeSelf) {
			this.citizenHistoryGO.SetActive (false);
		} else {
			if (this.currentlyShowingCitizen == null) {
				return;
			}

			List<Transform> children = this.citizenHistoryGrid.GetChildList ();
			for (int i = 0; i < children.Count; i++) {
				Destroy (children [i].gameObject);
			}

			for (int i = 0; i < this.currentlyShowingCitizen.history.Count; i++) {
				GameObject citizenGO = GameObject.Instantiate (this.historyPortraitPrefab, this.citizenHistoryGrid.transform) as GameObject;
				citizenGO.GetComponent<HistoryPortrait> ().SetHistory (this.currentlyShowingCitizen.history [i]);
				citizenGO.transform.localScale = Vector3.one;
				citizenGO.transform.localPosition = Vector3.zero;
			}

			StartCoroutine (RepositionGrid (this.citizenHistoryGrid));
			this.citizenHistoryGO.SetActive (true);
		}
	}
	public void HideCitizenHistory(){
		citizenHistoryBtn.SetClickState(false);
		this.citizenHistoryGO.SetActive(false);
	}

	public void ShowCityInfo(City cityToShow, bool showCitizens = false){
		if(cityToShow == null){
			return;
		}
		currentlyShowingCity = cityToShow;
		cityNameLbl.text = cityToShow.name;
		if (cityToShow.governor != null) {
			cityGovernorLbl.text = cityToShow.governor.name;
		}
		cityKingdomLbl.text = cityToShow.kingdom.name;
		cityGoldLbl.text = cityToShow.goldCount.ToString();
		cityStoneLbl.text = cityToShow.stoneCount.ToString();
		cityLumberLbl.text = cityToShow.lumberCount.ToString();
		cityManaStoneLbl.text = cityToShow.manaStoneCount.ToString();
		cityCobaltLbl.text = cityToShow.cobaltCount.ToString();
		cityMithrilLbl.text = cityToShow.mithrilCount.ToString();
		cityFoodLbl.text = "CITIZENS: " + cityToShow.citizens.Count + "/" + cityToShow.sustainability.ToString();
		cityInfoCtizenPortraitBG.color = cityToShow.kingdom.kingdomColor;


		if (cityInfoCitizensParent.activeSelf || showCitizens) {
			ShowCitizens();
		}

		if (cityInfoEventsParent.activeSelf) {
			ShowCityEvents();
		}

		if (cityInfoHistoryParent.activeSelf) {
			ShowCityHistory();
		}

		cityInfoGO.SetActive (true);
	}

	public void ShowCitizens(){
//		CharacterPortrait[] characters = cityInfoCitizensParent.GetComponentsInChildren<CharacterPortrait>();
//		List<Citizen> citizensExcludingKingAndGovernor = currentlyShowingCity.citizens.Where(x => x.role != ROLE.GOVERNOR && x.role != ROLE.KING).ToList();


//		for (int i = 0; i < characters.Length; i++) {
//			Destroy (characters [i].gameObject);
//		}
		//Foodie
		List<Transform> children = foodProducersGrid.GetChildList();
		List<Citizen> citizensConcerned = currentlyShowingCity.GetCitizensWithRole (ROLE.FOODIE);
		List<Citizen> citizensOfTypeCurrentlyShowed = foodProducersGrid.GetChildList().Select(x => x.GetComponent<CharacterPortrait>().citizen).ToList();
		List<Citizen> missedOutCitizens = citizensOfTypeCurrentlyShowed.Except(citizensConcerned).Union(citizensConcerned.Except(citizensOfTypeCurrentlyShowed)).ToList();
		if (missedOutCitizens.Count > 0) {
			for (int i = 0; i < children.Count; i++) {
				Destroy (children[i].gameObject);
			}
			for (int i = 0; i < citizensConcerned.Count; i++) {
				GameObject citizenGO = GameObject.Instantiate (characterPortraitPrefab, foodProducersGrid.transform) as GameObject;
				citizenGO.GetComponent<CharacterPortrait> ().SetCitizen (citizensConcerned [i]);
				citizenGO.transform.localScale = Vector3.one;
			}
			StartCoroutine (RepositionGrid (this.foodProducersGrid));
		}

		//Gatherer
		citizensConcerned.Clear ();
		children = gatherersGrid.GetChildList();
		citizensConcerned = currentlyShowingCity.GetCitizensWithRole (ROLE.GATHERER);
		citizensOfTypeCurrentlyShowed = gatherersGrid.GetChildList().Select(x => x.GetComponent<CharacterPortrait>().citizen).ToList();
		missedOutCitizens = citizensOfTypeCurrentlyShowed.Except(citizensConcerned).Union(citizensConcerned.Except(citizensOfTypeCurrentlyShowed)).ToList();
		if (missedOutCitizens.Count > 0) {
			for (int i = 0; i < children.Count; i++) {
				Destroy (children [i].gameObject);
			}
			for (int i = 0; i < citizensConcerned.Count; i++) {
				GameObject citizenGO = GameObject.Instantiate (characterPortraitPrefab, gatherersGrid.transform) as GameObject;
				citizenGO.GetComponent<CharacterPortrait> ().SetCitizen (citizensConcerned [i]);
				citizenGO.transform.localScale = Vector3.one;
			}
			StartCoroutine (RepositionGrid (this.gatherersGrid));
		}

		//Miner
		citizensConcerned.Clear ();
		children = minersGrid.GetChildList();
		citizensConcerned = currentlyShowingCity.GetCitizensWithRole (ROLE.MINER);
		citizensOfTypeCurrentlyShowed = minersGrid.GetChildList().Select(x => x.GetComponent<CharacterPortrait>().citizen).ToList();
		missedOutCitizens = citizensOfTypeCurrentlyShowed.Except(citizensConcerned).Union(citizensConcerned.Except(citizensOfTypeCurrentlyShowed)).ToList();
		if (missedOutCitizens.Count > 0) {
			for (int i = 0; i < children.Count; i++) {
				Destroy (children [i].gameObject);
			}
			for (int i = 0; i < citizensConcerned.Count; i++) {
				GameObject citizenGO = GameObject.Instantiate (characterPortraitPrefab, minersGrid.transform) as GameObject;
				citizenGO.GetComponent<CharacterPortrait> ().SetCitizen (citizensConcerned [i]);
				citizenGO.transform.localScale = Vector3.one;
			}
			StartCoroutine (RepositionGrid (this.minersGrid));
		}

		//Trader
		citizensConcerned.Clear ();
		children = tradersGrid.GetChildList();
		citizensConcerned = currentlyShowingCity.GetCitizensWithRole (ROLE.TRADER);
		citizensOfTypeCurrentlyShowed = tradersGrid.GetChildList().Select(x => x.GetComponent<CharacterPortrait>().citizen).ToList();
		missedOutCitizens = citizensOfTypeCurrentlyShowed.Except(citizensConcerned).Union(citizensConcerned.Except(citizensOfTypeCurrentlyShowed)).ToList();
		if (missedOutCitizens.Count > 0) {
			for (int i = 0; i < children.Count; i++) {
				Destroy (children [i].gameObject);
			}
			for (int i = 0; i < citizensConcerned.Count; i++) {
				GameObject citizenGO = GameObject.Instantiate (characterPortraitPrefab, tradersGrid.transform) as GameObject;
				citizenGO.GetComponent<CharacterPortrait> ().SetCitizen (citizensConcerned [i]);
				citizenGO.transform.localScale = Vector3.one;
			}
			StartCoroutine (RepositionGrid (this.tradersGrid));
		}

		//General
		citizensConcerned.Clear ();
		children = generalsGrid.GetChildList();
		citizensConcerned = currentlyShowingCity.GetCitizensWithRole (ROLE.GENERAL);
		citizensOfTypeCurrentlyShowed = generalsGrid.GetChildList().Select(x => x.GetComponent<CharacterPortrait>().citizen).ToList();
		missedOutCitizens = citizensOfTypeCurrentlyShowed.Except(citizensConcerned).Union(citizensConcerned.Except(citizensOfTypeCurrentlyShowed)).ToList();
		if (missedOutCitizens.Count > 0) {
			for (int i = 0; i < children.Count; i++) {
				Destroy (children [i].gameObject);
			}
			for (int i = 0; i < citizensConcerned.Count; i++) {
				GameObject citizenGO = GameObject.Instantiate (characterPortraitPrefab, generalsGrid.transform) as GameObject;
				citizenGO.GetComponent<CharacterPortrait> ().SetCitizen (citizensConcerned [i]);
				citizenGO.transform.localScale = Vector3.one;
			}
			StartCoroutine (RepositionGrid (this.generalsGrid));
		}

		//Spy
		citizensConcerned.Clear ();
		children = spiesGrid.GetChildList();
		citizensConcerned = currentlyShowingCity.GetCitizensWithRole (ROLE.SPY);
		citizensOfTypeCurrentlyShowed = spiesGrid.GetChildList().Select(x => x.GetComponent<CharacterPortrait>().citizen).ToList();
		missedOutCitizens = citizensOfTypeCurrentlyShowed.Except(citizensConcerned).Union(citizensConcerned.Except(citizensOfTypeCurrentlyShowed)).ToList();
		if (missedOutCitizens.Count > 0) {
			for (int i = 0; i < children.Count; i++) {
				Destroy (children [i].gameObject);
			}
			for (int i = 0; i < citizensConcerned.Count; i++) {
				GameObject citizenGO = GameObject.Instantiate (characterPortraitPrefab, spiesGrid.transform) as GameObject;
				citizenGO.GetComponent<CharacterPortrait> ().SetCitizen (citizensConcerned [i]);
				citizenGO.transform.localScale = Vector3.one;
			}
			StartCoroutine (RepositionGrid (this.spiesGrid));
		}

		//Envoy
		citizensConcerned.Clear ();
		children = envoysGrid.GetChildList();
		citizensConcerned = currentlyShowingCity.GetCitizensWithRole (ROLE.ENVOY);
		citizensOfTypeCurrentlyShowed = envoysGrid.GetChildList().Select(x => x.GetComponent<CharacterPortrait>().citizen).ToList();
		missedOutCitizens = citizensOfTypeCurrentlyShowed.Except(citizensConcerned).Union(citizensConcerned.Except(citizensOfTypeCurrentlyShowed)).ToList();
		if (missedOutCitizens.Count > 0) {
			for (int i = 0; i < children.Count; i++) {
				Destroy (children [i].gameObject);
			}
			for (int i = 0; i < citizensConcerned.Count; i++) {
				GameObject citizenGO = GameObject.Instantiate (characterPortraitPrefab, envoysGrid.transform) as GameObject;
				citizenGO.GetComponent<CharacterPortrait> ().SetCitizen (citizensConcerned [i]);
				citizenGO.transform.localScale = Vector3.one;
			}
			StartCoroutine (RepositionGrid (this.envoysGrid));
		}

		//Guardian
		citizensConcerned.Clear ();
		children = guardiansGrid.GetChildList();
		citizensConcerned = currentlyShowingCity.GetCitizensWithRole (ROLE.GUARDIAN);
		citizensOfTypeCurrentlyShowed = guardiansGrid.GetChildList().Select(x => x.GetComponent<CharacterPortrait>().citizen).ToList();
		missedOutCitizens = citizensOfTypeCurrentlyShowed.Except(citizensConcerned).Union(citizensConcerned.Except(citizensOfTypeCurrentlyShowed)).ToList();
		if (missedOutCitizens.Count > 0) {
			for (int i = 0; i < children.Count; i++) {
				Destroy (children [i].gameObject);
			}
			for (int i = 0; i < citizensConcerned.Count; i++) {
				GameObject citizenGO = GameObject.Instantiate (characterPortraitPrefab, guardiansGrid.transform) as GameObject;
				citizenGO.GetComponent<CharacterPortrait> ().SetCitizen (citizensConcerned [i]);
				citizenGO.transform.localScale = Vector3.one;
			}
			StartCoroutine (RepositionGrid (this.guardiansGrid));
		}

		//Untrained
		citizensConcerned.Clear ();
		children = untrainedGrid.GetChildList();
		citizensConcerned = currentlyShowingCity.GetCitizensWithRole (ROLE.UNTRAINED);
		citizensOfTypeCurrentlyShowed = untrainedGrid.GetChildList().Select(x => x.GetComponent<CharacterPortrait>().citizen).ToList();
		missedOutCitizens = citizensOfTypeCurrentlyShowed.Except(citizensConcerned).Union(citizensConcerned.Except(citizensOfTypeCurrentlyShowed)).ToList();
		if (missedOutCitizens.Count > 0) {
			for (int i = 0; i < children.Count; i++) {
				Destroy (children [i].gameObject);
			}
			for (int i = 0; i < citizensConcerned.Count; i++) {
				GameObject citizenGO = GameObject.Instantiate (characterPortraitPrefab, untrainedGrid.transform) as GameObject;
				citizenGO.GetComponent<CharacterPortrait> ().SetCitizen (citizensConcerned [i]);
				citizenGO.transform.localScale = Vector3.one;
			}
			StartCoroutine (RepositionGrid (this.untrainedGrid));
		}

		cityInfoHistoryParent.SetActive(false);
		cityInfoEventsParent.SetActive(false);
		cityInfoCitizensParent.SetActive(true);
	}

	public void ShowCityHistory(){
		List<History> cityHistory = currentlyShowingCity.cityHistory;
		List<History> cityHistoryCurrentlyShowing = cityInfoHistoryGrid.GetChildList().Select(x => x.GetComponent<HistoryPortrait>().history).ToList();
		int numberOfSameHistoryShowing = cityHistory.Intersect (cityHistoryCurrentlyShowing).Count();

		if (numberOfSameHistoryShowing != cityHistory.Count) {
			List<Transform> historyShowing = cityInfoHistoryGrid.GetChildList();
			for (int i = 0; i < historyShowing.Count; i++) {
				Destroy (historyShowing[i].gameObject);
			}

			for (int i = 0; i < currentlyShowingCity.cityHistory.Count; i++) {
				GameObject historyGO = GameObject.Instantiate (historyPortraitPrefab, cityInfoHistoryGrid.transform) as GameObject;
				historyGO.GetComponent<HistoryPortrait>().SetHistory(currentlyShowingCity.cityHistory[i]);
				historyGO.transform.localScale = Vector3.one;
				historyGO.transform.localPosition = Vector3.zero;
			}
			StartCoroutine (RepositionGrid (cityInfoHistoryGrid));
		}

		cityInfoCitizensParent.SetActive(false);
		cityInfoEventsParent.SetActive(false);
		cityInfoHistoryParent.SetActive(true);
	}

	public void ShowCityEvents(){
		List<GameEvent> eventsInCity = EventManager.Instance.GetAllEventsPerCity(currentlyShowingCity);
		List<GameEvent> eventsCurrentlyShowing = cityInfoEventsGrid.GetChildList().Select(x => x.GetComponent<EventItem>().gameEvent).ToList();
		int numberOfSameEventsShowing = eventsInCity.Intersect (eventsCurrentlyShowing).Count();

		if (eventsInCity.Count > 0) {
			noEventsGO.SetActive (false);
		} else {
			noEventsGO.SetActive (true);
			List<Transform> eventsShowing = cityInfoEventsGrid.GetChildList();
			for (int i = 0; i < eventsShowing.Count; i++) {
				Destroy (eventsShowing[i].gameObject);
			}
		}

		if (numberOfSameEventsShowing != eventsInCity.Count) {
			List<Transform> eventsShowing = cityInfoEventsGrid.GetChildList();
			for (int i = 0; i < eventsShowing.Count; i++) {
				Destroy (eventsShowing[i].gameObject);
			}

			for (int i = 0; i < eventsInCity.Count; i++) {
				GameObject eventGO = GameObject.Instantiate (gameEventPrefab, cityInfoEventsGrid.transform) as GameObject;
				eventGO.GetComponent<EventItem>().SetEvent (eventsInCity[i]);
				eventGO.GetComponent<EventItem> ().SetSpriteIcon (GetSpriteForEvent(eventsInCity[i].eventType));
				eventGO.GetComponent<EventItem> ().onClickEvent += ShowSpecificEvent;
				eventGO.transform.localScale = Vector3.one;
			}
			StartCoroutine(RepositionGrid (cityInfoEventsGrid));
		}

		if (eventsInCity.Count > 0) {
			noEventsGO.SetActive (false);
		} else {
			noEventsGO.SetActive (true);
		}

		cityInfoCitizensParent.SetActive(false);
		cityInfoHistoryParent.SetActive(false);
		cityInfoEventsParent.SetActive(true);
	}

	public void HideCityInfo(){
//		currentlyShowingCity = null;
		cityInfoCitizensParent.SetActive(false);
		cityInfoEventsParent.SetActive(false);
		cityInfoHistoryParent.SetActive(false);
		cityInfoGO.SetActive (false);
	}
		
	public void ShowKingdomInfo(Kingdom kingdom){
		this.currentlyShowingKingdom = kingdom;
		this.kingdomNameBigLbl.text = "[b]" + kingdom.name + "[/b]";
		this.kingdomNameSmallLbl.text = kingdom.name;
		this.kingdomKingNameLbl.text = "[b]" + kingdom.king.name + "[/b]";
		this.kingdomCityCountLbl.text = "[b]" + kingdom.cities.Count.ToString() + "[/b]";

		int generalCount = 0;
		for(int i = 0; i < kingdom.cities.Count; i++){
			for(int j = 0; j < kingdom.cities[i].citizens.Count; j++){
				if(kingdom.cities[i].citizens[j].assignedRole != null && kingdom.cities[i].citizens[j].role == ROLE.GENERAL){
					generalCount += 1;
				}
			}
		}

		this.kingdomGeneralCountLbl.text = "[b]" + generalCount.ToString () + "[/b]";

		this.kingdomWarsLbl.text = "[b]";
		for(int i = 0; i < kingdom.relationshipsWithOtherKingdoms.Count; i++){
			if(kingdom.relationshipsWithOtherKingdoms[i].isAtWar){
				this.kingdomWarsLbl.text += kingdom.relationshipsWithOtherKingdoms [i].objectInRelationship.name + ", ";
			}
		}
		this.kingdomWarsLbl.text.TrimEnd (',');
		this.kingdomWarsLbl.text += "[/b]";

		OnClickShowKingdomHistory ();

		this.kingdomInfoGO.SetActive (true);

	}
	public void HideKingdomInfo(){
		this.currentlyShowingKingdom = null;
		this.kingdomInfoGO.SetActive (false);

	}

	public void OnClickShowKingdomSuccession(){
		if(this.currentlyShowingKingdom == null){
			return;
		}

		//CLEAR SUCCESSION
		List<Transform> children = this.kingdomSuccessionGrid.GetChildList();
		for (int i = 0; i < children.Count; i++) {
			Destroy (children [i].gameObject);
		}
		//POPULATE
		for(int i = 0; i < this.currentlyShowingKingdom.successionLine.Count; i++){
			if(i > 2){
				break;
			}
			GameObject citizenGO = GameObject.Instantiate (this.successionPortraitPrefab, this.kingdomSuccessionGrid.transform) as GameObject;
			citizenGO.GetComponent<SuccessionPortrait> ().SetCitizen (this.currentlyShowingKingdom.successionLine[i], this.currentlyShowingKingdom);
			citizenGO.transform.localScale = Vector3.one;
			citizenGO.transform.localPosition = Vector3.zero;
		}

		for(int i = 0; i < this.currentlyShowingKingdom.pretenders.Count; i++){
			GameObject citizenGO = GameObject.Instantiate (this.successionPortraitPrefab, this.kingdomSuccessionGrid.transform) as GameObject;
			citizenGO.GetComponent<SuccessionPortrait> ().SetCitizen (this.currentlyShowingKingdom.pretenders[i], this.currentlyShowingKingdom);
			citizenGO.transform.localScale = Vector3.one;
			citizenGO.transform.localPosition = Vector3.zero;

		}
		StartCoroutine (RepositionGrid (this.kingdomSuccessionGrid));
		this.kingdomSuccessionGO.SetActive (true);
		this.kingdomHistoryGO.SetActive (false);
		this.kingdomEventsGO.SetActive (false);
		this.kingdomGovernorsGO.SetActive (false);

	}
	public void OnClickShowKingdomHistory(){
		if(this.currentlyShowingKingdom == null){
			return;
		}
		List<Transform> children = this.kingdomHistoryGrid.GetChildList();
		for (int i = 0; i < children.Count; i++) {
			Destroy (children [i].gameObject);
		}

		for(int i = 0; i < this.currentlyShowingKingdom.kingdomHistory.Count; i++){
			GameObject citizenGO = GameObject.Instantiate (this.historyPortraitPrefab, this.kingdomHistoryGrid.transform) as GameObject;
			citizenGO.GetComponent<HistoryPortrait> ().SetHistory (this.currentlyShowingKingdom.kingdomHistory[i]);
			citizenGO.transform.localScale = Vector3.one;
			citizenGO.transform.localPosition = Vector3.zero;
		}

		StartCoroutine (RepositionGrid (this.kingdomHistoryGrid));
		this.kingdomSuccessionGO.SetActive (false);
		this.kingdomHistoryGO.SetActive (true);
		this.kingdomEventsGO.SetActive (false);
		this.kingdomGovernorsGO.SetActive (false);

	}
	public void OnClickShowKingdomEvents(){
		if(this.currentlyShowingKingdom == null){
			return;
		}

		List<Transform> children = this.kingdomEventsGrid.GetChildList();
		for (int i = 0; i < children.Count; i++) {
			Destroy (children [i].gameObject);
		}

		List<GameEvent> eventsAffected = new List<GameEvent> ();
		List<EVENT_TYPES> eventKeys = new List<EVENT_TYPES> (EventManager.Instance.allEvents.Keys);
		for(int i = 0; i < eventKeys.Count; i++){
			if(eventKeys[i] == EVENT_TYPES.BORDER_CONFLICT){
				for(int j = 0; j < EventManager.Instance.allEvents[eventKeys[i]].Count; j++){
					if(EventManager.Instance.allEvents[eventKeys[i]][j].isActive){
						if(EventManager.Instance.allEvents[eventKeys[i]][j] is BorderConflict){
							if(((BorderConflict)EventManager.Instance.allEvents[eventKeys[i]][j]).kingdom1.id == this.currentlyShowingKingdom.id || ((BorderConflict)EventManager.Instance.allEvents[eventKeys[i]][j]).kingdom2.id == this.currentlyShowingKingdom.id){
								eventsAffected.Add (EventManager.Instance.allEvents [eventKeys [i]] [j]);
							}
						}

					}

				}
			}else if(eventKeys[i] == EVENT_TYPES.STATE_VISIT){
				for(int j = 0; j < EventManager.Instance.allEvents[eventKeys[i]].Count; j++){
					if(EventManager.Instance.allEvents[eventKeys[i]][j].isActive){
						if (EventManager.Instance.allEvents [eventKeys [i]] [j] is StateVisit) {
							if (((StateVisit)EventManager.Instance.allEvents [eventKeys [i]] [j]).inviterKingdom.id == this.currentlyShowingKingdom.id) {
								eventsAffected.Add (EventManager.Instance.allEvents [eventKeys [i]] [j]);
							}
						}
					}

				}
			}else if(eventKeys[i] == EVENT_TYPES.ASSASSINATION){
				for(int j = 0; j < EventManager.Instance.allEvents[eventKeys[i]].Count; j++){
					if(EventManager.Instance.allEvents[eventKeys[i]][j].isActive){
						if (EventManager.Instance.allEvents [eventKeys [i]] [j] is Assassination) {
							if (((Assassination)EventManager.Instance.allEvents [eventKeys [i]] [j]).targetCitizen.city.kingdom.id == this.currentlyShowingKingdom.id) {
								eventsAffected.Add (EventManager.Instance.allEvents [eventKeys [i]] [j]);
							}
						}
					}

				}
			}else if(eventKeys[i] == EVENT_TYPES.ESPIONAGE){
				for(int j = 0; j < EventManager.Instance.allEvents[eventKeys[i]].Count; j++){
					if(EventManager.Instance.allEvents[eventKeys[i]][j].isActive){
						if (EventManager.Instance.allEvents [eventKeys [i]] [j] is Espionage) {
							if (((Espionage)EventManager.Instance.allEvents [eventKeys [i]] [j]).targetKingdom.id == this.currentlyShowingKingdom.id) {
								eventsAffected.Add (EventManager.Instance.allEvents [eventKeys [i]] [j]);
							}
						}
					}

				}
			}else if(eventKeys[i] == EVENT_TYPES.RAID){
				for(int j = 0; j < EventManager.Instance.allEvents[eventKeys[i]].Count; j++){
					if(EventManager.Instance.allEvents[eventKeys[i]][j].isActive){
						if (EventManager.Instance.allEvents [eventKeys [i]] [j] is Raid) {
							if (((Raid)EventManager.Instance.allEvents [eventKeys [i]] [j]).raidedCity.kingdom.id == this.currentlyShowingKingdom.id) {
								eventsAffected.Add (EventManager.Instance.allEvents [eventKeys [i]] [j]);
							}
						}
					}
				}
			}else if(eventKeys[i] == EVENT_TYPES.INVASION_PLAN){
				for(int j = 0; j < EventManager.Instance.allEvents[eventKeys[i]].Count; j++){
					if(EventManager.Instance.allEvents[eventKeys[i]][j].isActive){
						if (EventManager.Instance.allEvents [eventKeys [i]] [j] is InvasionPlan) {
							if (((InvasionPlan)EventManager.Instance.allEvents [eventKeys [i]] [j]).targetKingdom.id == this.currentlyShowingKingdom.id) {
								eventsAffected.Add (EventManager.Instance.allEvents [eventKeys [i]] [j]);
							}
						}
					}

				}
			}else if(eventKeys[i] == EVENT_TYPES.MILITARIZATION){
				for(int j = 0; j < EventManager.Instance.allEvents[eventKeys[i]].Count; j++){
					if(EventManager.Instance.allEvents[eventKeys[i]][j].isActive){
						if (EventManager.Instance.allEvents [eventKeys [i]] [j] is Militarization) {
							if (((Militarization)EventManager.Instance.allEvents [eventKeys [i]] [j]).startedByKingdom.id == this.currentlyShowingKingdom.id) {
								eventsAffected.Add (EventManager.Instance.allEvents [eventKeys [i]] [j]);
							}
						}
					}

				}
			}else if(eventKeys[i] == EVENT_TYPES.JOIN_WAR_REQUEST){
				for(int j = 0; j < EventManager.Instance.allEvents[eventKeys[i]].Count; j++){
					if(EventManager.Instance.allEvents[eventKeys[i]][j].isActive){
						if (EventManager.Instance.allEvents [eventKeys [i]] [j] is JoinWar) {
							if (((JoinWar)EventManager.Instance.allEvents [eventKeys [i]] [j]).candidateForAlliance.city.kingdom.id == this.currentlyShowingKingdom.id) {
								eventsAffected.Add (EventManager.Instance.allEvents [eventKeys [i]] [j]);
							}
						}
					}

				}
			}else if(eventKeys[i] == EVENT_TYPES.POWER_GRAB){
				for(int j = 0; j < EventManager.Instance.allEvents[eventKeys[i]].Count; j++){
					if(EventManager.Instance.allEvents[eventKeys[i]][j].isActive){
						if (EventManager.Instance.allEvents [eventKeys [i]] [j] is PowerGrab) {
							if (((PowerGrab)EventManager.Instance.allEvents [eventKeys [i]] [j]).kingToOverthrow.city.kingdom.id == this.currentlyShowingKingdom.id) {
								eventsAffected.Add (EventManager.Instance.allEvents [eventKeys [i]] [j]);
							}
						}
					}

				}
			}else if(eventKeys[i] == EVENT_TYPES.EXHORTATION){
				for(int j = 0; j < EventManager.Instance.allEvents[eventKeys[i]].Count; j++){
					if(EventManager.Instance.allEvents[eventKeys[i]][j].isActive){
						if (EventManager.Instance.allEvents [eventKeys [i]] [j] is Exhortation) {
							if (((Exhortation)EventManager.Instance.allEvents [eventKeys [i]] [j]).targetCitizen.city.kingdom.id == this.currentlyShowingKingdom.id) {
								eventsAffected.Add (EventManager.Instance.allEvents [eventKeys [i]] [j]);
							}
						}
					}
				}
			}
		}


		for(int i = 0; i < eventsAffected.Count; i++){
			GameObject citizenGO = GameObject.Instantiate (this.eventPortraitPrefab, this.kingdomEventsGrid.transform) as GameObject;
			citizenGO.GetComponent<EventPortrait> ().SetEvent (eventsAffected[i]);
			citizenGO.transform.localScale = Vector3.one;
			citizenGO.transform.localPosition = Vector3.zero;
		}

		StartCoroutine (RepositionGrid (this.kingdomEventsGrid));
		this.kingdomSuccessionGO.SetActive (false);
		this.kingdomHistoryGO.SetActive (false);
		this.kingdomEventsGO.SetActive (true);
		this.kingdomGovernorsGO.SetActive (false);

	}
	public void OnClickShowKingdomGovernors(){
		if(this.currentlyShowingKingdom == null){
			return;
		}

		List<Transform> children = this.kingdomGovernorsGrid.GetChildList();
		for (int i = 0; i < children.Count; i++) {
			Destroy (children [i].gameObject);
		}

		for(int i = 0; i < this.currentlyShowingKingdom.cities.Count; i++){
			GameObject citizenGO = GameObject.Instantiate (this.governorPortraitPrefab, this.kingdomGovernorsGrid.transform) as GameObject;
			citizenGO.GetComponent<GovernorPortrait> ().SetGovernor (this.currentlyShowingKingdom.cities[i].governor);
			citizenGO.transform.localScale = Vector3.one;
			citizenGO.transform.localPosition = Vector3.zero;
		}

		StartCoroutine (RepositionGrid (this.kingdomGovernorsGrid));
		this.kingdomSuccessionGO.SetActive (false);
		this.kingdomHistoryGO.SetActive (false);
		this.kingdomEventsGO.SetActive (false);
		this.kingdomGovernorsGO.SetActive (true);

	}
	public IEnumerator RepositionGrid(UIGrid thisGrid){
		yield return null;
		thisGrid.Reposition ();
		yield return null;
	}

	public void ShowRelationships(){
		relationshipKingName.text = "King " + currentlyShowingCitizen.name;
		relationshipKingKingdomName.text = currentlyShowingCitizen.city.kingdom.name;
		relationshipKingSprite.color = currentlyShowingCitizen.city.kingdom.kingdomColor;
		relationshipsGO.SetActive (true);
		ShowKingRelationships ();
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
		if (kingRelationshipsParentGO.activeSelf) {
			return;
		}
		kingRelationshipsBtn.SetClickState(true);
		governorRelationshipsBtn.SetClickState(false);
		List<Transform> children = kingRelationshipsGrid.GetChildList();
		for (int i = 0; i < children.Count; i++) {
			Destroy (children [i].gameObject);
		}

		for (int i = 0; i < currentlyShowingCitizen.relationshipKings.Count; i++) {
			GameObject kingGO = GameObject.Instantiate(characterPortraitPrefab, kingRelationshipsGrid.transform) as GameObject;
			kingGO.GetComponent<CharacterPortrait>().SetCitizen(currentlyShowingCitizen.relationshipKings [i].king, true);
			kingGO.GetComponent<CharacterPortrait>().DisableHover();
			kingGO.transform.localScale = new Vector3(1.5f, 1.5f, 0);
			kingGO.GetComponent<CharacterPortrait> ().ShowRelationshipLine (currentlyShowingCitizen.relationshipKings [i], 
				currentlyShowingCitizen.relationshipKings[i].king.GetRelationshipWithCitizen(currentlyShowingCitizen));
			kingGO.GetComponent<CharacterPortrait>().onClickCharacterPortrait += ShowRelationshipHistory;

		}

		governorRelationshipsParentGO.SetActive(false);
		kingRelationshipsParentGO.SetActive(true);
		StartCoroutine(RepositionGrid(kingRelationshipsGrid));

		if (currentlyShowingCitizen.relationshipKings.Count > 1) {
//			NGUITools.SetActive (kingMainLineSprite.gameObject, true);
//			kingMainLineSprite.updateAnchors = UIRect.AnchorUpdate.OnEnable;
//			kingMainLineSprite.topAnchor.target = kingRelationshipsGrid.GetChildList ().First ().GetComponent<CharacterPortrait> ().lineGO.transform;
//			kingMainLineSprite.topAnchor.absolute = 0;
//			kingMainLineSprite.bottomAnchor.target = kingRelationshipsGrid.GetChildList ().Last ().GetComponent<CharacterPortrait> ().lineGO.transform;
//			kingMainLineSprite.bottomAnchor.absolute = 0;
//			kingMainLineSprite.UpdateAnchors();
//			NGUITools.SetActive (kingMainLineSprite.gameObject, false);
//			NGUITools.SetActive (kingMainLineSprite.gameObject, true);
			kingMainLineSprite.height = 100 * currentlyShowingCitizen.relationshipKings.Count;
			kingMainLineSprite.gameObject.SetActive(true);
		} else {
			kingMainLineSprite.gameObject.SetActive (false);
		}
	}
	public void ShowGovernorRelationships(){
		if (governorRelationshipsParentGO.activeSelf) {
			return;
		}
		kingRelationshipsBtn.SetClickState(false);
		governorRelationshipsBtn.SetClickState(true);
		List<Transform> children = governorsRelationshipGrid.GetChildList();
		for (int i = 0; i < children.Count; i++) {
			Destroy (children [i].gameObject);
		}

		for (int i = 0; i < currentlyShowingCitizen.city.kingdom.cities.Count; i++) {
			GameObject governorGO = GameObject.Instantiate(characterPortraitPrefab, governorsRelationshipGrid.transform) as GameObject;
			governorGO.GetComponent<CharacterPortrait>().SetCitizen(currentlyShowingCitizen.city.kingdom.cities[i].governor, true);
			governorGO.GetComponent<CharacterPortrait>().DisableHover();
			governorGO.transform.localScale = new Vector3(1.5f, 1.5f, 0);
			governorGO.GetComponent<CharacterPortrait>().ShowRelationshipLine();
//			governorGO.GetComponent<CharacterPortrait>().onClickCharacterPortrait += ShowRelationshipHistory;
		}

		kingRelationshipsParentGO.SetActive(false);
		governorRelationshipsParentGO.SetActive(true);
		StartCoroutine(RepositionGrid(governorsRelationshipGrid));

		if (currentlyShowingCitizen.city.kingdom.cities.Count > 1) {
//			NGUITools.SetActive (governorMainLineSprite.gameObject, true);
//			governorMainLineSprite.updateAnchors = UIRect.AnchorUpdate.OnEnable;
//			governorMainLineSprite.topAnchor.target = governorsRelationshipGrid.GetChildList ().First ().GetComponent<CharacterPortrait> ().lineGO.transform;
//			governorMainLineSprite.topAnchor.absolute = 0;
//			governorMainLineSprite.bottomAnchor.target = governorsRelationshipGrid.GetChildList ().Last ().GetComponent<CharacterPortrait> ().lineGO.transform;
//			governorMainLineSprite.bottomAnchor.absolute = 0;
//			governorMainLineSprite.UpdateAnchors();
//			NGUITools.SetActive (governorMainLineSprite.gameObject, false);
//			NGUITools.SetActive (governorMainLineSprite.gameObject, true);

			governorMainLineSprite.height = 95 * currentlyShowingCitizen.relationshipKings.Count;
			governorMainLineSprite.gameObject.SetActive(true);
		} else {
			governorMainLineSprite.gameObject.SetActive (false);
		}
	}

	public void HideRelationships(){
		kingRelationshipsParentGO.SetActive (false);
		governorRelationshipsParentGO.SetActive(false);
		relationshipsGO.SetActive (false);
		relationshipsBtn.OnClick();
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
			GameObject historyGO = GameObject.Instantiate (this.historyPortraitPrefab, this.relationshipHistoryGrid.transform) as GameObject;
			historyGO.GetComponent<HistoryPortrait> ().SetHistory(relationship.relationshipHistory[i]);
			historyGO.transform.localScale = Vector3.one;
			historyGO.transform.localPosition = Vector3.zero;
		}

		StartCoroutine (RepositionGrid (relationshipHistoryGrid));

		//For Testing
		relationshipHistoryForTestingGO.SetActive(true);
		sourceKinglikenessLbl.text = relationship.like.ToString();
		targetKinglikenessLbl.text = citizenInRelationshipWith.GetRelationshipWithCitizen(currentlyShowingCitizen).like.ToString();

		relationshipStatusSprite.color = Utilities.GetColorForRelationship(relationship.lordRelationship);
		relationshipHistoryGO.SetActive(true);
	}

	public void EditSourceRelationship(){
		int newLikeRating;
		if(Int32.TryParse(sourceKinglikenessLbl.text, out newLikeRating)){
			currentlyShowingRelationship.ChangeSourceKingLikeness(newLikeRating);
			this.ShowRelationshipHistory(currentlyShowingRelationship.king);
			kingRelationshipsParentGO.SetActive(false);
			governorRelationshipsParentGO.SetActive(false);
			this.ShowRelationships();
		}
	}

	public void EditTargetRelationship(){
		int newLikeRating;
		if(Int32.TryParse(targetKinglikenessLbl.text, out newLikeRating)){
			currentlyShowingRelationship.ChangeTargetKingLikeness(newLikeRating);
			this.ShowRelationshipHistory(currentlyShowingRelationship.king);
			kingRelationshipsParentGO.SetActive(false);
			governorRelationshipsParentGO.SetActive(false);
			this.ShowRelationships();
		}
	}

	public void HideRelationshipHistory(){
		relationshipHistoryGO.SetActive(false);
	}

	public void ShowGovernorInfo(){
		if (this.currentlyShowingCity != null) {
			this.ShowCitizenInfo (this.currentlyShowingCity.governor);
		}
	}

	public void ShowSmallInfo(string info, Transform parent){
		smallInfoLbl.text = info;

		var v3 = Input.mousePosition;
		v3.z = 10.0f;
		v3 = uiCamera.GetComponent<Camera>().ScreenToWorldPoint(v3);
		v3.y -= 0.15f;
		smallInfoGO.transform.position = v3;
//		smallInfoGO.transform.parent = parent;
//		smallInfoGO.transform.localPosition = new Vector3 (0f, -100f, 0f);
//		Vector3 newPos = smallInfoGO.transform.localPosition;

//		smallInfoGO.transform.localPosition = newPos;
//		smallInfoGO.transform.parent = this.transform;
//		smallInfoGO.transform.localScale = Vector3.one;
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
				GameObject fatherGO = GameObject.Instantiate (characterPortraitPrefab, familyTreeFatherGO.transform) as GameObject;
				fatherGO.transform.localScale = new Vector3 (2.1f, 2.1f, 0f);
				fatherGO.transform.localPosition = Vector3.zero;
				fatherGO.GetComponent<CharacterPortrait> ().SetCitizen (currentlyShowingCitizen.father);
			}
			if (familyTreeMotherGO.GetComponentInChildren<CharacterPortrait>() != null) {
				Destroy (familyTreeMotherGO.GetComponentInChildren<CharacterPortrait>().gameObject);
			}
			if (currentlyShowingCitizen.mother != null) {
				GameObject motherGO = GameObject.Instantiate (characterPortraitPrefab, familyTreeMotherGO.transform) as GameObject;
				motherGO.transform.localScale = new Vector3 (2.1f, 2.1f, 0f);
				motherGO.transform.localPosition = Vector3.zero;
				motherGO.GetComponent<CharacterPortrait> ().SetCitizen (currentlyShowingCitizen.mother);
			}
			if (familyTreeSpouseGO.GetComponentInChildren<CharacterPortrait>() != null) {
				Destroy (familyTreeSpouseGO.GetComponentInChildren<CharacterPortrait>().gameObject);
			}
			if (currentlyShowingCitizen.spouse != null) {
				GameObject spouseGO = GameObject.Instantiate (characterPortraitPrefab, familyTreeSpouseGO.transform) as GameObject;
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
						if (this.marriageHistoryOfCurrentCitizen[i].husband.id == currentlyShowingCitizen.spouse.id) {
							this.currentMarriageHistoryIndex = i;
							break;
						}
					}
				}
			}

			CharacterPortrait[] children = familyTreeChildGrid.GetComponentsInChildren<CharacterPortrait>();
			for (int i = 0; i < children.Length; i++) {
				Destroy (children [i].gameObject);
			}

			List<Transform> childPositions = familyTreeChildGrid.GetChildList ();
			for (int i = 0; i < currentlyShowingCitizen.children.Count; i++) {
				GameObject childGO = GameObject.Instantiate (characterPortraitPrefab, childPositions [i].transform) as GameObject;
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
			GameObject spouseGO = GameObject.Instantiate (characterPortraitPrefab, familyTreeSpouseGO.transform) as GameObject;
			spouseGO.transform.localScale = new Vector3 (2.1f, 2.1f, 0f);
			spouseGO.transform.localPosition = Vector3.zero;
			spouseGO.GetComponent<CharacterPortrait> ().SetCitizen (marriedCoupleToShow.wife);
		} else {
			//currentlyShowingCitizen is female	
			GameObject spouseGO = GameObject.Instantiate (characterPortraitPrefab, familyTreeSpouseGO.transform) as GameObject;
			spouseGO.transform.localScale = new Vector3 (2.1f, 2.1f, 0f);
			spouseGO.transform.localPosition = Vector3.zero;
			spouseGO.GetComponent<CharacterPortrait> ().SetCitizen (marriedCoupleToShow.husband);
		}

		List<Transform> childPositions = familyTreeChildGrid.GetChildList ();
		for (int i = 0; i < marriedCoupleToShow.children.Count; i++) {
			GameObject childGO = GameObject.Instantiate(characterPortraitPrefab, childPositions[i].transform) as GameObject;
			childGO.transform.localScale = new Vector3(2.1f, 2.1f, 0f);
			childGO.transform.localPosition = Vector3.zero;
			childGO.GetComponent<CharacterPortrait>().SetCitizen(marriedCoupleToShow.children[i]);
		}
	}

	public void ToggleEventsMenu(){
		eventsGo.SetActive(!eventsGo.activeSelf);
		if (!eventsGo.activeSelf) {
			eventsOfTypeGo.SetActive(false);
			EventManager.Instance.onHideEvents.Invoke();
			List<Transform> events = eventCategoriesGrid.GetChildList();
			for (int i = 0; i < events.Count; i++) {
				events [i].GetComponent<ButtonToggle>().SetClickState(false);
			}
		}

	}

	public void ShowEventsOfType(GameObject GO){
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
	}

	public void UpdateEventsOfType(){
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
				EventManager.Instance.onShowEventsOfType.Invoke (EVENT_TYPES.ALL);
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
	}
		
	public void ShowSpecificEvent(GameEvent gameEvent){
		specificEventNameLbl.text = gameEvent.eventType.ToString().Replace("_", " ");
		specificEventDescriptionLbl.text = gameEvent.description;
		specificEventStartDateLbl.text = "Started " + ((MONTH)gameEvent.startMonth).ToString() + " " + gameEvent.startWeek.ToString() + ", " + gameEvent.startYear.ToString();

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

		if (gameEvent.eventType != EVENT_TYPES.BORDER_CONFLICT && gameEvent.eventType != EVENT_TYPES.STATE_VISIT) {
			this.specificEventBarTitle.text = "Duration";
			specificEventProgBar.value = (float)((float)gameEvent.remainingWeeks / (float)gameEvent.durationInWeeks);
		}

		if (gameEvent.eventType == EVENT_TYPES.MARRIAGE_INVITATION) {
			MarriageInvitation marriageEvent = (MarriageInvitation)gameEvent;
			ShowMarriageInvitationEvent (marriageEvent);
		}else if (gameEvent.eventType == EVENT_TYPES.BORDER_CONFLICT) {
			BorderConflict borderConflict = (BorderConflict)gameEvent;
			ShowBorderConflictEvent (borderConflict);
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


		this.specificEventBarTitle.text = "Tension";
		this.specificEventProgBar.value = (float)borderConflict.tension / 100f;
		this.specificEventCandidatesTitleLbl.text = "PACIFIERS";
		for(int i = 0; i < borderConflict.activeEnvoysReduce.Count; i++){
			GameObject candidates = GameObject.Instantiate (characterPortraitPrefab, this.specificEventCandidatesGrid.transform) as GameObject;
			candidates.GetComponent<CharacterPortrait> ().SetCitizen (borderConflict.activeEnvoysReduce[i].citizen.city.kingdom.king);
			candidates.transform.localScale = Vector3.one;
			candidates.transform.position = Vector3.zero;
		}
		StartCoroutine (RepositionGrid (this.specificEventCandidatesGrid));

		this.specificEventMiscTitleLbl.text = "PROVOKERS";
		this.specificEventMiscTitleLbl.gameObject.SetActive(true);
		for(int i = 0; i < borderConflict.activeEnvoysIncrease.Count; i++){
			GameObject candidates = GameObject.Instantiate (characterPortraitPrefab, this.specificEventMiscGrid.transform) as GameObject;
			candidates.GetComponent<CharacterPortrait> ().SetCitizen (borderConflict.activeEnvoysIncrease[i].citizen.city.kingdom.king);
			candidates.transform.localScale = Vector3.one;
			candidates.transform.position = Vector3.zero;
		}
		StartCoroutine (RepositionGrid (this.specificEventMiscGrid));
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
		this.specificEventCandidatesTitleLbl.text = "SUPPORTERS";
		for(int i = 0; i < stateVisit.helperEnvoys.Count; i++){
			GameObject candidates = GameObject.Instantiate (characterPortraitPrefab, this.specificEventCandidatesGrid.transform) as GameObject;
			candidates.GetComponent<CharacterPortrait> ().SetCitizen (stateVisit.helperEnvoys[i].city.kingdom.king);
			candidates.transform.localScale = Vector3.one;
			candidates.transform.position = Vector3.zero;
		}
		StartCoroutine (RepositionGrid (this.specificEventCandidatesGrid));

		this.specificEventMiscTitleLbl.text = "SABOTEURS";
		this.specificEventMiscTitleLbl.gameObject.SetActive(true);
		for(int i = 0; i < stateVisit.saboteurEnvoys.Count; i++){
			GameObject candidates = GameObject.Instantiate (characterPortraitPrefab, this.specificEventMiscGrid.transform) as GameObject;
			candidates.GetComponent<CharacterPortrait> ().SetCitizen (stateVisit.saboteurEnvoys[i].city.kingdom.king);
			candidates.transform.localScale = Vector3.one;
			candidates.transform.position = Vector3.zero;
		}
		StartCoroutine (RepositionGrid (this.specificEventMiscGrid));
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
			totalMilitaryStrength += ((General)generals[i].assignedRole).army.hp;
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

	public void ToggleResourceIcons(){
		CameraMove.Instance.ToggleResourceIcons();
	}

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
		}
		return assassinationIcon;
	}

	public bool IsMouseOnUI(){
		if( uiCamera != null ){
			if (UICamera.hoveredObject != null && UICamera.hoveredObject != this.gameObject) {
				return true;
			}
		}
		return false;
	}







//------------------------------------------------------------------------------------------- FOR TESTING ---------------------------------------------------------------------

	public void OnValueChangeEventDropdown(){
		eventDropdownCurrentSelectionLbl.text = this.eventDropdownList.value;
		if(this.eventDropdownList.value == "Raid"){
			goRaid.SetActive (true);
			goStateVisit.SetActive (false);
			goMarriageInvitation.SetActive (false);
			goPowerGrab.SetActive (false);
			goExpansion.SetActive (false);
		}else if(this.eventDropdownList.value == "State Visit"){
			goRaid.SetActive (false);
			goStateVisit.SetActive (true);
			goMarriageInvitation.SetActive (false);
			goPowerGrab.SetActive (false);
			goExpansion.SetActive (false);
		}else if(this.eventDropdownList.value == "Marriage Invitation"){
			goRaid.SetActive (false);
			goStateVisit.SetActive (false);
			goMarriageInvitation.SetActive (true);
			goPowerGrab.SetActive (false);
			goExpansion.SetActive (false);
		}else if(this.eventDropdownList.value == "Power Grab"){
			goRaid.SetActive (false);
			goStateVisit.SetActive (false);
			goMarriageInvitation.SetActive (false);
			goPowerGrab.SetActive (true);
			goExpansion.SetActive (false);
		}else if(this.eventDropdownList.value == "Expansion"){
			goRaid.SetActive (false);
			goStateVisit.SetActive (false);
			goMarriageInvitation.SetActive (false);
			goPowerGrab.SetActive (false);
			goExpansion.SetActive (true);
		}
	}

	public void ShowCreateEventUI(){
		this.goCreateEventUI.SetActive (true);
	}
	public void HideCreateEventUI(){
		this.goCreateEventUI.SetActive (false);
	}

	public void ToggleTraitEditor(){
		if (traitEditorGO.activeSelf) {
			traitEditorGO.SetActive (false);
		} else {
			addTraitPopUpList.Clear();
			removeTraitPopUpList.Clear();
			BEHAVIOR_TRAIT[] allBehaviourTraits = Utilities.GetEnumValues<BEHAVIOR_TRAIT>();
			SKILL_TRAIT[] allSkillTraits = Utilities.GetEnumValues<SKILL_TRAIT>();
			MISC_TRAIT[] allMiscTraits = Utilities.GetEnumValues<MISC_TRAIT>();

			for (int i = 0; i < allBehaviourTraits.Length; i++) {
				if (allBehaviourTraits[i] != BEHAVIOR_TRAIT.NONE) {
					addTraitPopUpList.AddItem (allBehaviourTraits[i].ToString ());
				}
			}
			for (int i = 0; i < allSkillTraits.Length; i++) {
				if (allSkillTraits[i] != SKILL_TRAIT.NONE) {
					addTraitPopUpList.AddItem (allSkillTraits[i].ToString ());
				}
			}
			for (int i = 0; i < allMiscTraits.Length; i++) {
				if (allMiscTraits[i] != MISC_TRAIT.NONE) {
					addTraitPopUpList.AddItem (allMiscTraits[i].ToString ());
				}
			}

			for (int i = 0; i < currentlyShowingCitizen.behaviorTraits.Count; i++) {
				removeTraitPopUpList.AddItem (currentlyShowingCitizen.behaviorTraits[i].ToString());
			}
			for (int i = 0; i < currentlyShowingCitizen.skillTraits.Count; i++) {
				removeTraitPopUpList.AddItem (currentlyShowingCitizen.skillTraits[i].ToString());
			}
			for (int i = 0; i < currentlyShowingCitizen.miscTraits.Count; i++) {
				removeTraitPopUpList.AddItem (currentlyShowingCitizen.miscTraits[i].ToString());
			}
			if (removeTraitPopUpList.items.Count > 0) {
				removeTraitPopUpList.value = removeTraitPopUpList.items [0];
			} else {
				removeTraitPopUpList.value = "";
			}
			addTraitPopUpList.value = addTraitPopUpList.items [0];
			removeTraitChoiceLbl.text = removeTraitPopUpList.value;
			addTraitChoiceLbl.text = addTraitPopUpList.value;
			traitEditorGO.SetActive (true);
		}
	}

	public void AddTrait(){
		BEHAVIOR_TRAIT chosenBehaviourTrait = BEHAVIOR_TRAIT.NONE;
		SKILL_TRAIT chosenSkillTrait = SKILL_TRAIT.NONE;
		MISC_TRAIT chosenMiscTrait = MISC_TRAIT.NONE;
		if (chosenBehaviourTrait.TryParse<BEHAVIOR_TRAIT> (addTraitPopUpList.value, out chosenBehaviourTrait)) {
			if (chosenBehaviourTrait == BEHAVIOR_TRAIT.NAIVE) {
				if (currentlyShowingCitizen.behaviorTraits.Contains (BEHAVIOR_TRAIT.SCHEMING)) {
					currentlyShowingCitizen.behaviorTraits.Remove (BEHAVIOR_TRAIT.SCHEMING);
					Debug.Log ("Removed behaviour trait : " + BEHAVIOR_TRAIT.SCHEMING.ToString () + " from citizen " + currentlyShowingCitizen.name + " because trait to add is contradicting.");
				}
			} else if (chosenBehaviourTrait == BEHAVIOR_TRAIT.SCHEMING) {
				if (currentlyShowingCitizen.behaviorTraits.Contains (BEHAVIOR_TRAIT.NAIVE)) {
					currentlyShowingCitizen.behaviorTraits.Remove (BEHAVIOR_TRAIT.NAIVE);
					Debug.Log ("Removed behaviour trait : " + BEHAVIOR_TRAIT.NAIVE.ToString () + " from citizen " + currentlyShowingCitizen.name + " because trait to add is contradicting.");
				}
			} else if (chosenBehaviourTrait == BEHAVIOR_TRAIT.WARMONGER) {
				if (currentlyShowingCitizen.behaviorTraits.Contains (BEHAVIOR_TRAIT.PACIFIST)) {
					currentlyShowingCitizen.behaviorTraits.Remove (BEHAVIOR_TRAIT.PACIFIST);
					Debug.Log ("Removed behaviour trait : " + BEHAVIOR_TRAIT.PACIFIST.ToString () + " from citizen " + currentlyShowingCitizen.name + " because trait to add is contradicting.");
				}
			} else if (chosenBehaviourTrait == BEHAVIOR_TRAIT.PACIFIST) {
				if (currentlyShowingCitizen.behaviorTraits.Contains (BEHAVIOR_TRAIT.WARMONGER)) {
					currentlyShowingCitizen.behaviorTraits.Remove (BEHAVIOR_TRAIT.WARMONGER);
					Debug.Log ("Removed behaviour trait : " + BEHAVIOR_TRAIT.WARMONGER.ToString () + " from citizen " + currentlyShowingCitizen.name + " because trait to add is contradicting.");
				}
			}else if (chosenBehaviourTrait == BEHAVIOR_TRAIT.CHARISMATIC) {
				if (currentlyShowingCitizen.behaviorTraits.Contains (BEHAVIOR_TRAIT.REPULSIVE)) {
					currentlyShowingCitizen.behaviorTraits.Remove (BEHAVIOR_TRAIT.REPULSIVE);
					Debug.Log ("Removed behaviour trait : " + BEHAVIOR_TRAIT.REPULSIVE.ToString () + " from citizen " + currentlyShowingCitizen.name + " because trait to add is contradicting.");
				}
			} else if (chosenBehaviourTrait == BEHAVIOR_TRAIT.REPULSIVE) {
				if (currentlyShowingCitizen.behaviorTraits.Contains (BEHAVIOR_TRAIT.CHARISMATIC)) {
					currentlyShowingCitizen.behaviorTraits.Remove (BEHAVIOR_TRAIT.CHARISMATIC);
					Debug.Log ("Removed behaviour trait : " + BEHAVIOR_TRAIT.CHARISMATIC.ToString () + " from citizen " + currentlyShowingCitizen.name + " because trait to add is contradicting.");
				}
			} else if (chosenBehaviourTrait == BEHAVIOR_TRAIT.AGGRESSIVE) {
				if (currentlyShowingCitizen.behaviorTraits.Contains (BEHAVIOR_TRAIT.DEFENSIVE)) {
					currentlyShowingCitizen.behaviorTraits.Remove (BEHAVIOR_TRAIT.DEFENSIVE);
					Debug.Log ("Removed behaviour trait : " + BEHAVIOR_TRAIT.DEFENSIVE.ToString () + " from citizen " + currentlyShowingCitizen.name + " because trait to add is contradicting.");
				}
			} else if (chosenBehaviourTrait == BEHAVIOR_TRAIT.DEFENSIVE) {
				if (currentlyShowingCitizen.behaviorTraits.Contains (BEHAVIOR_TRAIT.AGGRESSIVE)) {
					currentlyShowingCitizen.behaviorTraits.Remove (BEHAVIOR_TRAIT.AGGRESSIVE);
					Debug.Log ("Removed behaviour trait : " + BEHAVIOR_TRAIT.AGGRESSIVE.ToString () + " from citizen " + currentlyShowingCitizen.name + " because trait to add is contradicting.");
				}
			}
			if (currentlyShowingCitizen.behaviorTraits.Contains (chosenBehaviourTrait)) {
				Debug.Log (currentlyShowingCitizen.name + " already has that trait!");
			} else {
				currentlyShowingCitizen.behaviorTraits.Add (chosenBehaviourTrait);
				Debug.Log ("Added behaviour trait : " + chosenBehaviourTrait.ToString () + " to citizen " + currentlyShowingCitizen.name);
			}
		} else if (chosenSkillTrait.TryParse<SKILL_TRAIT> (addTraitPopUpList.value, out chosenSkillTrait)) {
			if (chosenSkillTrait == SKILL_TRAIT.EFFICIENT) {
				if (currentlyShowingCitizen.skillTraits.Contains (SKILL_TRAIT.INEFFICIENT)) {
					currentlyShowingCitizen.skillTraits.Remove (SKILL_TRAIT.INEFFICIENT);
					Debug.Log ("Removed skill trait : " + SKILL_TRAIT.INEFFICIENT.ToString () + " from citizen " + currentlyShowingCitizen.name + " because trait to add is contradicting.");
				}
			} else if (chosenSkillTrait == SKILL_TRAIT.INEFFICIENT) {
				if (currentlyShowingCitizen.skillTraits.Contains (SKILL_TRAIT.EFFICIENT)) {
					currentlyShowingCitizen.skillTraits.Remove (SKILL_TRAIT.EFFICIENT);
					Debug.Log ("Removed skill trait : " + SKILL_TRAIT.EFFICIENT.ToString () + " from citizen " + currentlyShowingCitizen.name + " because trait to add is contradicting.");
				}
			} else if (chosenSkillTrait == SKILL_TRAIT.THRIFTY) {
				if (currentlyShowingCitizen.skillTraits.Contains (SKILL_TRAIT.LAVISH)) {
					currentlyShowingCitizen.skillTraits.Remove (SKILL_TRAIT.LAVISH);
					Debug.Log ("Removed skill trait : " + SKILL_TRAIT.LAVISH.ToString () + " from citizen " + currentlyShowingCitizen.name + " because trait to add is contradicting.");
				}
			} else if (chosenSkillTrait == SKILL_TRAIT.LAVISH) {
				if (currentlyShowingCitizen.skillTraits.Contains (SKILL_TRAIT.THRIFTY)) {
					currentlyShowingCitizen.skillTraits.Remove (SKILL_TRAIT.THRIFTY);
					Debug.Log ("Removed skill trait : " + SKILL_TRAIT.THRIFTY.ToString () + " from citizen " + currentlyShowingCitizen.name + " because trait to add is contradicting.");
				}
			}
			if (currentlyShowingCitizen.skillTraits.Contains (chosenSkillTrait)) {
				Debug.Log (currentlyShowingCitizen.name + " already has that trait!");
			} else {
				currentlyShowingCitizen.skillTraits.Add (chosenSkillTrait);
				Debug.Log ("Added skill trait : " + chosenSkillTrait.ToString () + " to citizen " + currentlyShowingCitizen.name);
			}
		} else if (chosenMiscTrait.TryParse<MISC_TRAIT> (addTraitPopUpList.value, out chosenMiscTrait)) {
			if (currentlyShowingCitizen.miscTraits.Contains (chosenMiscTrait)) {
				Debug.Log (currentlyShowingCitizen.name + " already has that trait!");
			} else {
				currentlyShowingCitizen.miscTraits.Add (chosenMiscTrait);
				if(chosenMiscTrait == MISC_TRAIT.TACTICAL){
					currentlyShowingCitizen.campaignManager.campaignLimit = 3;
				}else if(chosenMiscTrait == MISC_TRAIT.ACCIDENT_PRONE){
					currentlyShowingCitizen.citizenChances.accidentChance = 50f;
				}
				Debug.Log ("Added misc trait : " + chosenMiscTrait.ToString () + " to citizen " + currentlyShowingCitizen.name);
			}
		}

		ShowCitizenInfo(currentlyShowingCitizen);
		ToggleTraitEditor();
		if (currentlyShowingCitizen.city != null) {
			currentlyShowingCitizen.city.UpdateResourceProduction();
		}
	}

	public void RemoveTrait(){
		if (string.IsNullOrEmpty (removeTraitPopUpList.value)) {
			return;
		}
		BEHAVIOR_TRAIT chosenBehaviourTrait = BEHAVIOR_TRAIT.NONE;
		SKILL_TRAIT chosenSkillTrait = SKILL_TRAIT.NONE;
		MISC_TRAIT chosenMiscTrait = MISC_TRAIT.NONE;
		if (chosenBehaviourTrait.TryParse<BEHAVIOR_TRAIT> (removeTraitPopUpList.value, out chosenBehaviourTrait)) {
			currentlyShowingCitizen.behaviorTraits.Remove(chosenBehaviourTrait);
			Debug.Log ("Removed behaviour trait : " + chosenBehaviourTrait.ToString () + " from " + currentlyShowingCitizen.name);
		} else if (chosenSkillTrait.TryParse<SKILL_TRAIT> (removeTraitPopUpList.value, out chosenSkillTrait)) {
			currentlyShowingCitizen.skillTraits.Remove(chosenSkillTrait);
			Debug.Log ("Removed skill trait : " + chosenSkillTrait.ToString () + " from " + currentlyShowingCitizen.name);
		} else if (chosenMiscTrait.TryParse<MISC_TRAIT> (removeTraitPopUpList.value, out chosenMiscTrait)) {
			currentlyShowingCitizen.miscTraits.Remove(chosenMiscTrait);
			if(chosenMiscTrait == MISC_TRAIT.TACTICAL){
				currentlyShowingCitizen.campaignManager.campaignLimit = 2;
			}else if(chosenMiscTrait == MISC_TRAIT.ACCIDENT_PRONE){
				currentlyShowingCitizen.citizenChances.accidentChance = currentlyShowingCitizen.citizenChances.defaultAccidentChance;
			}
			Debug.Log ("Removed misc trait : " + chosenMiscTrait.ToString () + " from " + currentlyShowingCitizen.name);
		}

		ShowCitizenInfo(currentlyShowingCitizen);
		ToggleTraitEditor();
		if (currentlyShowingCitizen.city != null) {
			currentlyShowingCitizen.city.UpdateResourceProduction();
		}
	}

	public void HideTraitEditor(){
		traitEditorGO.SetActive (false);
	}
}
