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


	void Awake(){
		Instance = this;
	}

	void Update(){
		dateLbl.text = "[b]" + ((MONTH)GameManager.Instance.month).ToString () + " " + GameManager.Instance.week.ToString () + ", " + GameManager.Instance.year.ToString () + "[/b]";
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
		citizenNameLbl.text = "[b]" + citizenToShow.name + "[/b]";
		citizenKingdomNameLbl.text = citizenToShow.city.kingdom.name;
		citizenBdayLbl.text = "[b]" + ((MONTH)citizenToShow.birthMonth).ToString() + " " + citizenToShow.birthWeek.ToString() + ", " + citizenToShow.birthYear.ToString() + "(" + citizenToShow.age.ToString() + ")[/b]";
		citizenCityNameLbl.text = "[b]" + citizenToShow.city.name + "[/b]";
		citizenRoleLbl.text = "[b]" + citizenToShow.role.ToString() + "[/b]";
		citizenPrestigeLbl.text = "[b]" + citizenToShow.prestige.ToString() + "[/b]";

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
		citizenInfoGO.SetActive (false);
	}

	public void ShowCityInfo(City cityToShow){
		cityNameLbl.text = cityToShow.name;
		cityGovernorLbl.text = cityToShow.governor.name;
		cityKingdomLbl.text = cityToShow.kingdom.name;
		cityGoldLbl.text = cityToShow.goldCount.ToString();
		cityStoneLbl.text = cityToShow.stoneCount.ToString();
		cityLumberLbl.text = cityToShow.lumberCount.ToString();
		cityManaStoneLbl.text = cityToShow.manaStoneCount.ToString();
		cityCobaltLbl.text = cityToShow.cobaltCount.ToString();
		cityMithrilLbl.text = cityToShow.mithrilCount.ToString();
		cityFoodLbl.text = "CITIZENS: " + cityToShow.citizens.Count + "/" + cityToShow.sustainability.ToString();

		List<Citizen> citizensConcerned = cityToShow.GetCitizensWithRole(ROLE.FOODIE);
		for (int i = 0; i < citizensConcerned.Count; i++) {
			
		}
//		public UIGrid foodProducersGrid;
//		public UIGrid gatherersGrid;
//		public UIGrid minersGrid;
//		public UIGrid tradersGrid;
//		public UIGrid generalsGrid;
//		public UIGrid spiesGrid;
//		public UIGrid envoysGrid;
//		public UIGrid guardiansGrid;
//		public UIGrid untrainedGrid;
//		public UI2DSprite cityInfoCtizenPortraitBG;
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
		smallInfoGO.SetActive (true);
	}

	internal void HideSmallInfo(){
		smallInfoGO.SetActive (false);
		smallInfoGO.transform.parent = this.transform;
	}
}
