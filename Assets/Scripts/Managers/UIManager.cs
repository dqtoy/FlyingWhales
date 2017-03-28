using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour {

	public static UIManager Instance = null;

	public GameObject characterPortraitPrefab;
	public GameObject traitPrefab;
	public GameObject smallInfoGO;
	public GameObject citizenInfoGO;
	public GameObject cityInfoGO;

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


	private Citizen currentlyShowingCitizen;
	private City currentlyShowingCity;

	void Awake(){
		Instance = this;
	}

	void Update(){
		dateLbl.text = "[b]" + ((MONTH)GameManager.Instance.month).ToString () + " " + GameManager.Instance.week.ToString () + ", " + GameManager.Instance.year.ToString () + "[/b]";
		if (currentlyShowingCity != null) {
			this.ShowCityInfo(currentlyShowingCity);
		}

		//		if (currentlyShowingCitizen != null) {
		//			this.ShowCitizenInfo(currentlyShowingCitizen);
		//		}
	}

	public void SetProgressionSpeed1X(){
		GameManager.Instance.SetProgressionSpeed(4f);
	}

	public void SetProgressionSpeed2X(){
		GameManager.Instance.SetProgressionSpeed(2f);
	}

	public void SetProgressionSpeed4X(){
		GameManager.Instance.SetProgressionSpeed(1f);
	}

	public void TogglePause(){
		GameManager.Instance.TogglePause();
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

	}

	public void HideCitizenInfo(){
		currentlyShowingCitizen = null;
		citizenInfoGO.SetActive (false);
	}

	public void ShowCityInfo(City cityToShow){
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

		CharacterPortrait[] characters = citizensParent.GetComponentsInChildren<CharacterPortrait>();
		if (characters.Length <= 0 || (characters.Length > 0 && characters [0].citizen.city.id != cityToShow.id) || (characters.Length > 0 && characters.Length != cityToShow.citizens.Count)) {
			for (int i = 0; i < characters.Length; i++) {
				Destroy (characters [i].gameObject);
			}

			List<Citizen> citizensConcerned = cityToShow.GetCitizensWithRole (ROLE.FOODIE);
			for (int i = 0; i < citizensConcerned.Count; i++) {
				GameObject citizenGO = GameObject.Instantiate (characterPortraitPrefab, foodProducersGrid.transform) as GameObject;
				citizenGO.GetComponent<CharacterPortrait> ().SetCitizen (citizensConcerned [i]);
				citizenGO.transform.localScale = Vector3.one;
			}
			citizensConcerned.Clear ();
			citizensConcerned = cityToShow.GetCitizensWithRole (ROLE.GATHERER);
			for (int i = 0; i < citizensConcerned.Count; i++) {
				GameObject citizenGO = GameObject.Instantiate (characterPortraitPrefab, gatherersGrid.transform) as GameObject;
				citizenGO.GetComponent<CharacterPortrait> ().SetCitizen (citizensConcerned [i]);
				citizenGO.transform.localScale = Vector3.one;
			}
			citizensConcerned.Clear ();
			citizensConcerned = cityToShow.GetCitizensWithRole (ROLE.MINER);
			for (int i = 0; i < citizensConcerned.Count; i++) {
				GameObject citizenGO = GameObject.Instantiate (characterPortraitPrefab, minersGrid.transform) as GameObject;
				citizenGO.GetComponent<CharacterPortrait> ().SetCitizen (citizensConcerned [i]);
				citizenGO.transform.localScale = Vector3.one;
			}
			citizensConcerned.Clear ();
			citizensConcerned = cityToShow.GetCitizensWithRole (ROLE.TRADER);
			for (int i = 0; i < citizensConcerned.Count; i++) {
				GameObject citizenGO = GameObject.Instantiate (characterPortraitPrefab, tradersGrid.transform) as GameObject;
				citizenGO.GetComponent<CharacterPortrait> ().SetCitizen (citizensConcerned [i]);
				citizenGO.transform.localScale = Vector3.one;
			}
			citizensConcerned.Clear ();
			citizensConcerned = cityToShow.GetCitizensWithRole (ROLE.GENERAL);
			for (int i = 0; i < citizensConcerned.Count; i++) {
				GameObject citizenGO = GameObject.Instantiate (characterPortraitPrefab, generalsGrid.transform) as GameObject;
				citizenGO.GetComponent<CharacterPortrait> ().SetCitizen (citizensConcerned [i]);
				citizenGO.transform.localScale = Vector3.one;
			}
			citizensConcerned.Clear ();
			citizensConcerned = cityToShow.GetCitizensWithRole (ROLE.SPY);
			for (int i = 0; i < citizensConcerned.Count; i++) {
				GameObject citizenGO = GameObject.Instantiate (characterPortraitPrefab, spiesGrid.transform) as GameObject;
				citizenGO.GetComponent<CharacterPortrait> ().SetCitizen (citizensConcerned [i]);
				citizenGO.transform.localScale = Vector3.one;
			}
			citizensConcerned.Clear ();
			citizensConcerned = cityToShow.GetCitizensWithRole (ROLE.ENVOY);
			for (int i = 0; i < citizensConcerned.Count; i++) {
				GameObject citizenGO = GameObject.Instantiate (characterPortraitPrefab, envoysGrid.transform) as GameObject;
				citizenGO.GetComponent<CharacterPortrait> ().SetCitizen (citizensConcerned [i]);
				citizenGO.transform.localScale = Vector3.one;
			}
			citizensConcerned.Clear ();
			citizensConcerned = cityToShow.GetCitizensWithRole (ROLE.GUARDIAN);
			for (int i = 0; i < citizensConcerned.Count; i++) {
				GameObject citizenGO = GameObject.Instantiate (characterPortraitPrefab, guardiansGrid.transform) as GameObject;
				citizenGO.GetComponent<CharacterPortrait> ().SetCitizen (citizensConcerned [i]);
				citizenGO.transform.localScale = Vector3.one;
			}
			citizensConcerned.Clear ();
			citizensConcerned = cityToShow.GetCitizensWithRole (ROLE.UNTRAINED);
			for (int i = 0; i < citizensConcerned.Count; i++) {
				GameObject citizenGO = GameObject.Instantiate (characterPortraitPrefab, untrainedGrid.transform) as GameObject;
				citizenGO.GetComponent<CharacterPortrait> ().SetCitizen (citizensConcerned [i]);
				citizenGO.transform.localScale = Vector3.one;
			}
			foodProducersGrid.enabled = true;
			gatherersGrid.enabled = true;
			minersGrid.enabled = true;
			tradersGrid.enabled = true;
			generalsGrid.enabled = true;
			spiesGrid.enabled = true;
			envoysGrid.enabled = true;
			guardiansGrid.enabled = true;
			untrainedGrid.enabled = true;
		}
		cityInfoGO.SetActive (true);
	}

	public void HideCityInfo(){
		currentlyShowingCity = null;
		cityInfoGO.SetActive (false);
	}

	internal void ShowSmallInfo(string info, Transform parent){
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

	internal void HideSmallInfo(){
		smallInfoGO.SetActive (false);
		smallInfoGO.transform.parent = this.transform;
	}
		
}
