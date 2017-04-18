using UnityEngine;
using System.Collections;

public class CharacterPortrait : MonoBehaviour {

	public delegate void OnClickCharacterPortrait(Citizen citizenClicked);
	public OnClickCharacterPortrait onClickCharacterPortrait;

	public UI2DSprite kingdomColorGO;
	public GameObject isDeadIcon;
	public GameObject characterInfoGO;
	public UILabel characterNameLbl;
	public UILabel characterKingdomNameLbl;
	public GameObject relationshipLine;
	public GameObject[] relationshipCircles;

	public Citizen citizen;
	private bool isHoverEnabled = true;

	public void SetCitizen(Citizen citizen, bool showInfo = false){
		this.citizen = citizen;
		if (this.citizen.city != null) {
			this.kingdomColorGO.color = this.citizen.city.kingdom.kingdomColor;
		} else {
			this.kingdomColorGO.color = Color.white;
		}
		if (citizen.isDead) {
			isDeadIcon.SetActive (true);
		} else {
			isDeadIcon.SetActive (false);
		}

		if (showInfo) {
			ShowCitizenInfo();
		}
	}

	public void ShowCitizenInfo(){
		characterNameLbl.text = this.citizen.name;
		characterKingdomNameLbl.text = this.citizen.city.kingdom.name;
		characterInfoGO.SetActive(true);
	}

	public void DisableHover(){
		isHoverEnabled = false;
	}

	public void ShowRelationshipLine(RelationshipKings relationship1 = null, RelationshipKings relationship2 = null){
		relationshipLine.SetActive(true);
		if (relationship1 != null && relationship2 != null) {
			relationshipCircles[0].SetActive(true);
			relationshipCircles[1].SetActive(true);
			relationshipCircles[0].GetComponent<UI2DSprite>().color = Utilities.GetColorForRelationship(relationship1.lordRelationship);
			relationshipCircles[1].GetComponent<UI2DSprite>().color = Utilities.GetColorForRelationship(relationship2.lordRelationship);
		}
	}

	void OnHover(bool isOver){
		if (!isHoverEnabled) {
			return;
		}
		if (isOver) {
			if (citizen.city != null) {
				UIManager.Instance.ShowSmallInfo ("[b]" + citizen.name + "[/b]" + "\n" + "[i]" + citizen.city.kingdom.name + "[/i]", this.transform);
			} else {
				UIManager.Instance.ShowSmallInfo ("[b]" + citizen.name + "[/b]" + "\n" + "[i] No Kingdom [/i]", this.transform);
			}

		} else {
			UIManager.Instance.HideSmallInfo ();
		}
	}

	void OnClick(){
		if (onClickCharacterPortrait == null) {
			if (citizen.father == null || citizen.mother == null) {
				Debug.Log (citizen.name + " doesn't have a father or a mother, not showing info");
				return;
			}
			UIManager.Instance.ShowCitizenInfo (citizen);
		} else {
			onClickCharacterPortrait(this.citizen);
		}
	}
}
