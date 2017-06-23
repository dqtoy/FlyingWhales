using UnityEngine;
using System.Collections;

public class BoonOfPowerAvatar : MonoBehaviour {

	public BoonOfPower boonOfPower;

	void OnTriggerEnter2D(Collider2D other){
		if(other.tag == "Avatar" || other.tag == "General"){
			this.boonOfPower.TransferBoonOfPower (other.GetComponent<Avatar>().kingdom);
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
