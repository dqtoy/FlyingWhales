using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class UIManager : MonoBehaviour {

	public static UIManager Instance = null;

	public GameObject characterPortraitPrefab;
	public GameObject successionPortraitPrefab;
	public GameObject historyPortraitPrefab;
	public GameObject eventPortraitPrefab;
	public GameObject governorPortraitPrefab;
	public GameObject traitPrefab;
	public GameObject gameEventPrefab;
	public GameObject relationshipLinePrefab;

	public GameObject smallInfoGO;
	public GameObject citizenInfoGO;
	public GameObject cityInfoGO;
	public GameObject kingdomInfoGO;
	public GameObject eventsGo;
	public GameObject eventsOfTypeGo;
	public GameObject relationshipsGO;

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

	[Space(10)]
	public UILabel citizenNameLbl;
	public UILabel citizenKingdomNameLbl;
	public UILabel citizenBdayLbl;
	public UILabel citizenCityNameLbl;
	public UILabel citizenRoleLbl;
	public UILabel citizenPrestigeLbl;
	public UIGrid citizenTraitsGrid;
	public GameObject kingSpecificGO;
	public ButtonToggle relationshipsBtn;

	[Space(10)]
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
	public GameObject citizensParent;
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


	[Space(10)]
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
	public GameObject kingRelationshipsParentGO;
	public GameObject governorRelationshipsParentGO;
	public UIGrid kingRelationshipsGrid;
	public UIGrid governorsRelationshipGrid;
	public UI2DSprite mainLineSprite;
	public UI2DSprite relationshipKingSprite;
	public UILabel relationshipKingName;
	public UILabel relationshipKingKingdomName;
	public ButtonToggle kingRelationshipsBtn;
	public ButtonToggle governorRelationshipsBtn;

	private Citizen currentlyShowingCitizen;
	private City currentlyShowingCity;
	private Kingdom currentlyShowingKingdom;

	void Awake(){
		Instance = this;
	}
	void Update(){
		dateLbl.text = "[b]" + ((MONTH)GameManager.Instance.month).ToString () + " " + GameManager.Instance.week.ToString () + ", " + GameManager.Instance.year.ToString () + "[/b]";
//		if (currentlyShowingCity != null) {
//			this.ShowCityInfo(currentlyShowingCity);
//		}

		//		if (currentlyShowingCitizen != null) {
		//			this.ShowCitizenInfo(currentlyShowingCitizen);
		//		}
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
		kingsGrid.enabled = true;
	}

	internal void ShowCitizenInfo(Citizen citizenToShow){
		if (relationshipsGO.activeSelf == true) {
			this.HideRelationships ();
		}

		currentlyShowingCitizen = citizenToShow;
		citizenNameLbl.text = "[b]" + citizenToShow.name + "[/b]";
		citizenKingdomNameLbl.text = citizenToShow.city.kingdom.name;
		citizenBdayLbl.text = "[b]" + ((MONTH)citizenToShow.birthMonth).ToString() + " " + citizenToShow.birthWeek.ToString() + ", " + citizenToShow.birthYear.ToString() + "(" + citizenToShow.age.ToString() + ")[/b]";
		citizenCityNameLbl.text = "[b]" + citizenToShow.city.name + "[/b]";
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
		}

		for (int i = 0; i < citizenToShow.skillTraits.Count; i++) {
			GameObject traitGO = GameObject.Instantiate (traitPrefab, citizenTraitsGrid.transform) as GameObject;
			traitGO.GetComponent<TraitObject>().SetTrait(BEHAVIOR_TRAIT.NONE, citizenToShow.skillTraits[i], MISC_TRAIT.NONE);
			traitGO.transform.localScale = Vector3.one;
		}

		for (int i = 0; i < citizenToShow.miscTraits.Count; i++) {
			GameObject traitGO = GameObject.Instantiate (traitPrefab, citizenTraitsGrid.transform) as GameObject;
			traitGO.GetComponent<TraitObject>().SetTrait(BEHAVIOR_TRAIT.NONE, SKILL_TRAIT.NONE, citizenToShow.miscTraits[i]);
			traitGO.transform.localScale = Vector3.one;
		}
		citizenTraitsGrid.enabled = true;

		if (citizenToShow.isKing) {
			kingSpecificGO.SetActive(true);
		} else {
			kingSpecificGO.SetActive(false);
		}

		ctizenPortraitBG.color = citizenToShow.city.kingdom.kingdomColor;

		citizenInfoGO.SetActive (true);

		if(citizenToShow.isKing){
			ShowKingdomInfo (citizenToShow.city.kingdom);
		}
	}

	public void HideCitizenInfo(){
		currentlyShowingCitizen = null;
		citizenInfoGO.SetActive (false);
	}

	public void ShowCityInfo(City cityToShow){
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

		citizensBtn.GetComponent<ButtonToggle> ().OnClick ();

		CharacterPortrait[] characters = citizensParent.GetComponentsInChildren<CharacterPortrait>();
		for (int i = 0; i < characters.Length; i++) {
			Destroy (characters [i].gameObject);
		}

		List<Citizen> citizensConcerned = cityToShow.GetCitizensWithRole (ROLE.FOODIE);
		for (int i = 0; i < citizensConcerned.Count; i++) {
			GameObject citizenGO = GameObject.Instantiate (characterPortraitPrefab, foodProducersGrid.transform) as GameObject;
			citizenGO.GetComponent<CharacterPortrait> ().SetCitizen (citizensConcerned [i]);
			citizenGO.transform.localScale = Vector3.one;
		}
		foodProducersGrid.enabled = true;

		citizensConcerned.Clear ();
		citizensConcerned = cityToShow.GetCitizensWithRole (ROLE.GATHERER);
		for (int i = 0; i < citizensConcerned.Count; i++) {
			GameObject citizenGO = GameObject.Instantiate (characterPortraitPrefab, gatherersGrid.transform) as GameObject;
			citizenGO.GetComponent<CharacterPortrait> ().SetCitizen (citizensConcerned [i]);
			citizenGO.transform.localScale = Vector3.one;
		}
		gatherersGrid.enabled = true;

		citizensConcerned.Clear ();
		citizensConcerned = cityToShow.GetCitizensWithRole (ROLE.MINER);
		for (int i = 0; i < citizensConcerned.Count; i++) {
			GameObject citizenGO = GameObject.Instantiate (characterPortraitPrefab, minersGrid.transform) as GameObject;
			citizenGO.GetComponent<CharacterPortrait> ().SetCitizen (citizensConcerned [i]);
			citizenGO.transform.localScale = Vector3.one;
		}
		minersGrid.enabled = true;

		citizensConcerned.Clear ();
		citizensConcerned = cityToShow.GetCitizensWithRole (ROLE.TRADER);
		for (int i = 0; i < citizensConcerned.Count; i++) {
			GameObject citizenGO = GameObject.Instantiate (characterPortraitPrefab, tradersGrid.transform) as GameObject;
			citizenGO.GetComponent<CharacterPortrait> ().SetCitizen (citizensConcerned [i]);
			citizenGO.transform.localScale = Vector3.one;
		}
		tradersGrid.enabled = true;

		citizensConcerned.Clear ();
		citizensConcerned = cityToShow.GetCitizensWithRole (ROLE.GENERAL);
		for (int i = 0; i < citizensConcerned.Count; i++) {
			GameObject citizenGO = GameObject.Instantiate (characterPortraitPrefab, generalsGrid.transform) as GameObject;
			citizenGO.GetComponent<CharacterPortrait> ().SetCitizen (citizensConcerned [i]);
			citizenGO.transform.localScale = Vector3.one;
		}
		generalsGrid.enabled = true;

		citizensConcerned.Clear ();
		citizensConcerned = cityToShow.GetCitizensWithRole (ROLE.SPY);
		for (int i = 0; i < citizensConcerned.Count; i++) {
			GameObject citizenGO = GameObject.Instantiate (characterPortraitPrefab, spiesGrid.transform) as GameObject;
			citizenGO.GetComponent<CharacterPortrait> ().SetCitizen (citizensConcerned [i]);
			citizenGO.transform.localScale = Vector3.one;
		}
		spiesGrid.enabled = true;

		citizensConcerned.Clear ();
		citizensConcerned = cityToShow.GetCitizensWithRole (ROLE.ENVOY);
		for (int i = 0; i < citizensConcerned.Count; i++) {
			GameObject citizenGO = GameObject.Instantiate (characterPortraitPrefab, envoysGrid.transform) as GameObject;
			citizenGO.GetComponent<CharacterPortrait> ().SetCitizen (citizensConcerned [i]);
			citizenGO.transform.localScale = Vector3.one;
		}
		envoysGrid.enabled = true;
		citizensConcerned.Clear ();
		citizensConcerned = cityToShow.GetCitizensWithRole (ROLE.GUARDIAN);
		for (int i = 0; i < citizensConcerned.Count; i++) {
			GameObject citizenGO = GameObject.Instantiate (characterPortraitPrefab, guardiansGrid.transform) as GameObject;
			citizenGO.GetComponent<CharacterPortrait> ().SetCitizen (citizensConcerned [i]);
			citizenGO.transform.localScale = Vector3.one;
		}
		guardiansGrid.enabled = true;

		citizensConcerned.Clear ();
		citizensConcerned = cityToShow.GetCitizensWithRole (ROLE.UNTRAINED);
		for (int i = 0; i < citizensConcerned.Count; i++) {
			GameObject citizenGO = GameObject.Instantiate (characterPortraitPrefab, untrainedGrid.transform) as GameObject;
			citizenGO.GetComponent<CharacterPortrait> ().SetCitizen (citizensConcerned [i]);
			citizenGO.transform.localScale = Vector3.one;
		}
		untrainedGrid.enabled = true;

		cityInfoGO.SetActive (true);
	}

	public void HideCityInfo(){
		currentlyShowingCity = null;
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
						if(((BorderConflict)EventManager.Instance.allEvents[eventKeys[i]][j]).kingdom1.id == this.currentlyShowingKingdom.id || ((BorderConflict)EventManager.Instance.allEvents[eventKeys[i]][j]).kingdom2.id == this.currentlyShowingKingdom.id){
							eventsAffected.Add (EventManager.Instance.allEvents [eventKeys [i]] [j]);
						}
					}

				}
			}else if(eventKeys[i] == EVENT_TYPES.STATE_VISIT){
				for(int j = 0; j < EventManager.Instance.allEvents[eventKeys[i]].Count; j++){
					if(EventManager.Instance.allEvents[eventKeys[i]][j].isActive){
						if(((StateVisit)EventManager.Instance.allEvents[eventKeys[i]][j]).inviterKingdom.id == this.currentlyShowingKingdom.id){
							eventsAffected.Add (EventManager.Instance.allEvents [eventKeys [i]] [j]);
						}
					}

				}
			}else if(eventKeys[i] == EVENT_TYPES.ASSASSINATION){
				for(int j = 0; j < EventManager.Instance.allEvents[eventKeys[i]].Count; j++){
					if(EventManager.Instance.allEvents[eventKeys[i]][j].isActive){
						if(((Assassination)EventManager.Instance.allEvents[eventKeys[i]][j]).targetCitizen.city.kingdom.id == this.currentlyShowingKingdom.id){
							eventsAffected.Add (EventManager.Instance.allEvents [eventKeys [i]] [j]);
						}
					}

				}
			}else if(eventKeys[i] == EVENT_TYPES.ESPIONAGE){
				for(int j = 0; j < EventManager.Instance.allEvents[eventKeys[i]].Count; j++){
					if(EventManager.Instance.allEvents[eventKeys[i]][j].isActive){
						if(((Espionage)EventManager.Instance.allEvents[eventKeys[i]][j]).targetKingdom.id == this.currentlyShowingKingdom.id){
							eventsAffected.Add (EventManager.Instance.allEvents [eventKeys [i]] [j]);
						}
					}

				}
			}else if(eventKeys[i] == EVENT_TYPES.RAID){
				for(int j = 0; j < EventManager.Instance.allEvents[eventKeys[i]].Count; j++){
					if(EventManager.Instance.allEvents[eventKeys[i]][j].isActive){
						if(((Raid)EventManager.Instance.allEvents[eventKeys[i]][j]).raidedCity.kingdom.id == this.currentlyShowingKingdom.id){
							eventsAffected.Add (EventManager.Instance.allEvents [eventKeys [i]] [j]);
						}
					}
				}
			}else if(eventKeys[i] == EVENT_TYPES.INVASION_PLAN){
				for(int j = 0; j < EventManager.Instance.allEvents[eventKeys[i]].Count; j++){
					if(EventManager.Instance.allEvents[eventKeys[i]][j].isActive){
						if(((InvasionPlan)EventManager.Instance.allEvents[eventKeys[i]][j]).targetKingdom.id == this.currentlyShowingKingdom.id){
							eventsAffected.Add (EventManager.Instance.allEvents [eventKeys [i]] [j]);
						}
					}

				}
			}else if(eventKeys[i] == EVENT_TYPES.MILITARIZATION){
				for(int j = 0; j < EventManager.Instance.allEvents[eventKeys[i]].Count; j++){
					if(EventManager.Instance.allEvents[eventKeys[i]][j].isActive){
						if(((Militarization)EventManager.Instance.allEvents[eventKeys[i]][j]).startedByKingdom.id == this.currentlyShowingKingdom.id){
							eventsAffected.Add (EventManager.Instance.allEvents [eventKeys [i]] [j]);
						}
					}

				}
			}else if(eventKeys[i] == EVENT_TYPES.JOIN_WAR_REQUEST){
				for(int j = 0; j < EventManager.Instance.allEvents[eventKeys[i]].Count; j++){
					if(EventManager.Instance.allEvents[eventKeys[i]][j].isActive){
						if(((JoinWar)EventManager.Instance.allEvents[eventKeys[i]][j]).candidateForAlliance.city.kingdom.id == this.currentlyShowingKingdom.id){
							eventsAffected.Add (EventManager.Instance.allEvents [eventKeys [i]] [j]);
						}
					}

				}
			}else if(eventKeys[i] == EVENT_TYPES.POWER_GRAB){
				for(int j = 0; j < EventManager.Instance.allEvents[eventKeys[i]].Count; j++){
					if(EventManager.Instance.allEvents[eventKeys[i]][j].isActive){
						if(((PowerGrab)EventManager.Instance.allEvents[eventKeys[i]][j]).kingToOverthrow.city.kingdom.id == this.currentlyShowingKingdom.id){
							eventsAffected.Add (EventManager.Instance.allEvents [eventKeys [i]] [j]);
						}
					}

				}
			}else if(eventKeys[i] == EVENT_TYPES.EXHORTATION){
				for(int j = 0; j < EventManager.Instance.allEvents[eventKeys[i]].Count; j++){
					if(EventManager.Instance.allEvents[eventKeys[i]][j].isActive){
						if(((Exhortation)EventManager.Instance.allEvents[eventKeys[i]][j]).targetCitizen.city.kingdom.id == this.currentlyShowingKingdom.id){
							eventsAffected.Add (EventManager.Instance.allEvents [eventKeys [i]] [j]);
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
	public void ToggleRelationships(){
		if (relationshipsGO.activeSelf == true) {
			relationshipsGO.SetActive (false);
		} else {
			relationshipKingName.text = "King " + currentlyShowingCitizen.name;
			relationshipKingKingdomName.text = currentlyShowingCitizen.city.kingdom.name;
			relationshipKingSprite.color = currentlyShowingCitizen.city.kingdom.kingdomColor;
			relationshipsGO.SetActive (true);
			kingRelationshipsBtn.SetAsClicked();
			ShowKingRelationships ();
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
			kingGO.transform.localScale = new Vector3(1.5f, 1.5f, 0);
			kingGO.GetComponent<CharacterPortrait>().ShowRelationshipLine (currentlyShowingCitizen.relationshipKings [i],
				currentlyShowingCitizen.relationshipKings[i].king.GetRelationshipWithCitizen(currentlyShowingCitizen));
			kingGO.GetComponent<CharacterPortrait>().onClickCharacterPortrait += ShowRelationshipHistory;

		}
		if (currentlyShowingCitizen.relationshipKings.Count > 1) {
			mainLineSprite.height = 137 * currentlyShowingCitizen.relationshipKings.Count;
			mainLineSprite.gameObject.SetActive(true);
		}
		governorRelationshipsParentGO.SetActive(false);
		kingRelationshipsParentGO.SetActive(true);
		kingRelationshipsGrid.enabled = true;
	}
	public void ShowGovernorRelationships(){

	}

	public void HideRelationships(){
		relationshipsGO.SetActive (false);
		relationshipsBtn.OnClick();
	}

	public void ShowGovernorInfo(){
		if (this.currentlyShowingCity != null) {
			this.ShowCitizenInfo (this.currentlyShowingCity.governor);
		}
	}

	public void ShowSmallInfo(string info, Transform parent){
		smallInfoLbl.text = info;
		smallInfoGO.transform.parent = parent;
		smallInfoGO.transform.localPosition = Vector3.zero;
		Vector3 newPos = smallInfoGO.transform.localPosition;
		if (parent.name == "CharacterPortraitPrefab(Clone)") {
			newPos.y -= 85f;
		} else {
			newPos.y -= 30f;
		}
		smallInfoGO.transform.localPosition = newPos;
		smallInfoGO.transform.parent = this.transform;
		smallInfoGO.SetActive (true);
	}

	public void HideSmallInfo(){
		smallInfoGO.SetActive (false);
		smallInfoGO.transform.parent = this.transform;
	}

	public void ToggleEventsMenu(){
		eventsGo.SetActive(!eventsGo.activeSelf);
	}
	private GameObject lastClickedEventType = null;
	public void ShowEventsOfType(GameObject GO){
		
		if (eventsOfTypeGo.activeSelf) {
			if (lastClickedEventType != null) {
				if (lastClickedEventType == GO) {
					eventsOfTypeGo.SetActive (false);
					return;
				} else {
					lastClickedEventType.GetComponent<ButtonToggle> ().OnClick ();
				}
			}
		} 
		lastClickedEventType = GO;
		if (GO.name == "AllBtn") {
			List<Transform> children = gameEventsOfTypeGrid.GetChildList ();
			for (int i = 0; i < children.Count; i++) {
				Destroy (children [i].gameObject);
			}
			for (int i = 0; i < EventManager.Instance.allEvents.Keys.Count; i++) {
				EVENT_TYPES currentKey = EventManager.Instance.allEvents.Keys.ElementAt(i);
				List<GameEvent> currentGameEventList = EventManager.Instance.allEvents[currentKey];
				for (int j = 0; j < currentGameEventList.Count; j++) {
					GameObject eventGO = GameObject.Instantiate (gameEventPrefab, gameEventsOfTypeGrid.transform) as GameObject;
					eventGO.GetComponent<EventItem>().SetEvent (currentGameEventList[j]);
					eventGO.GetComponent<EventItem> ().SetSpriteIcon (GetSpriteForEvent (currentKey));
					eventGO.transform.localScale = Vector3.one;
				}
			}
		} else {
			EVENT_TYPES eventType = (EVENT_TYPES)(System.Enum.Parse (typeof(EVENT_TYPES), GO.name));
			if (EventManager.Instance.allEvents.ContainsKey (eventType)) {
				List<GameEvent> gameEventsOfType = EventManager.Instance.allEvents [eventType];
				List<Transform> children = gameEventsOfTypeGrid.GetChildList ();
				for (int i = 0; i < children.Count; i++) {
					Destroy (children [i].gameObject);
				}
				for (int i = 0; i < gameEventsOfType.Count; i++) {
					GameObject eventGO = GameObject.Instantiate (gameEventPrefab, gameEventsOfTypeGrid.transform) as GameObject;
					eventGO.GetComponent<EventItem> ().SetEvent (gameEventsOfType [i]);
					eventGO.GetComponent<EventItem> ().SetSpriteIcon (GetSpriteForEvent (gameEventsOfType [i].eventType));
					eventGO.transform.localScale = Vector3.one;
				}
			}
		}
		gameEventsOfTypeGrid.enabled = true;
		eventsOfTypeGo.SetActive (true);
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


}
