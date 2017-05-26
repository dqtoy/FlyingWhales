using UnityEngine;
using System.Collections;

public class GovernorPortrait : MonoBehaviour {

	public UI2DSprite kingdomColorGO;
	public UILabel citizenName;
	public UILabel citizenCity;


	public Citizen citizen;

	public void SetGovernor(Citizen citizen){
		this.citizen = citizen;
		this.kingdomColorGO.color = this.citizen.city.kingdom.kingdomColor;
		this.citizenName.text = citizen.name;
		this.citizenCity.text = citizen.city.name;
	}

	void OnHover(bool isOver){
		if (isOver) {
			UIManager.Instance.ShowSmallInfo ("[b]" + citizen.name + "[/b]" + "\n" + "[i]" + citizen.city.kingdom.name + "[/i]");
		} else {
			UIManager.Instance.HideSmallInfo ();
		}
	}

	void OnClick(){
		UIManager.Instance.ShowCitizenInfo (citizen);
	}
}
