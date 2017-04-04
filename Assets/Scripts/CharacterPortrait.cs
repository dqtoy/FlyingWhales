using UnityEngine;
using System.Collections;

public class CharacterPortrait : MonoBehaviour {

	public UI2DSprite kingdomColorGO;
	public GameObject isDeadIcon;
	public GameObject relationshipLine;
	public GameObject[] relationshipCircles;

	public Citizen citizen;
	private bool isHoverEnabled = true;

	public void SetCitizen(Citizen citizen){
		this.citizen = citizen;
		this.kingdomColorGO.color = this.citizen.city.kingdom.kingdomColor;
		if (citizen.isDead) {
			isDeadIcon.SetActive (true);
		} else {
			isDeadIcon.SetActive (false);
		}
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
			UIManager.Instance.ShowSmallInfo ("[b]" + citizen.name + "[/b]" + "\n" + "[i]" + citizen.city.kingdom.name + "[/i]", this.transform);
		} else {
			UIManager.Instance.HideSmallInfo ();
		}
	}

	void OnClick(){
		UIManager.Instance.ShowCitizenInfo(citizen);
	}
}
