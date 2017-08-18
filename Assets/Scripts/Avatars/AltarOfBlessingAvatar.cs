using UnityEngine;
using System.Collections;

public class AltarOfBlessingAvatar : MonoBehaviour {

	public AltarOfBlessing altarOfBlessing;

	void OnTriggerEnter2D(Collider2D other){
		if(other.tag == "Avatar" || other.tag == "General"){
			Citizen otherAgent = other.gameObject.GetComponent<CitizenAvatar>().citizenRole.citizen;
			this.altarOfBlessing.TransferAltarOfBlessing (otherAgent.city.kingdom, otherAgent);
		}
	}

	internal void Init(AltarOfBlessing altarOfBlessing){
		this.altarOfBlessing = altarOfBlessing;
	}
	void OnMouseEnter(){
		if (!UIManager.Instance.IsMouseOnUI()) {
			UIManager.Instance.ShowSmallInfo (this.altarOfBlessing.name);
		}
	}

	void OnMouseExit(){
		UIManager.Instance.HideSmallInfo ();
	}

}
