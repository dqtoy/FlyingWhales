using UnityEngine;
using System.Collections;

public class CharacterPortrait : MonoBehaviour {

	public UI2DSprite kingdomColorGO;
	public GameObject isDeadIcon;

	public Citizen citizen;

	public void SetCitizen(Citizen citizen){
		this.citizen = citizen;
		this.kingdomColorGO.color = this.citizen.city.kingdom.kingdomColor;
		if (citizen.isDead) {
			isDeadIcon.SetActive (true);
		} else {
			isDeadIcon.SetActive (false);
		}
	}

	void OnHover(bool isOver){
		if (isOver) {
			UIManager.Instance.ShowSmallInfo ("[b]" + citizen.name + "[/b]" + "\n" + "[i]" + citizen.city.kingdom.name + "[/i]", this.transform);
		} else {
			UIManager.Instance.HideSmallInfo ();
		}
	}

	void OnClick(){
		UIManager.Instance.ShowCitizenInfo (citizen);
	}
}
