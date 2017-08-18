using UnityEngine;
using System.Collections;

public class BoonOfPowerAvatar : MonoBehaviour {

	public BoonOfPower boonOfPower;

	void OnTriggerEnter2D(Collider2D other){
		if(other.tag == "Avatar" || other.tag == "General"){
			Citizen otherAgent = other.gameObject.GetComponent<CitizenAvatar>().citizenRole.citizen;
			this.boonOfPower.TransferBoonOfPower (otherAgent.city.kingdom, otherAgent);
		}
	}

	internal void Init(BoonOfPower boonOfPower){
		this.boonOfPower = boonOfPower;
	}
	void OnMouseEnter(){
		if (!UIManager.Instance.IsMouseOnUI()) {
			UIManager.Instance.ShowSmallInfo (this.boonOfPower.name);
		}
	}

	void OnMouseExit(){
		UIManager.Instance.HideSmallInfo ();
	}

}
