using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CitizenAvatar : MonoBehaviour {

	private Citizen citizen;

	private List<HexTile> path;

	private HexTile startTile = null;
	private HexTile targetTile = null;
	private float smoothTime = 0.3f;
	private Vector3 velocity = Vector3.zero;

	internal void AssignCitizen(Citizen citizen){
		this.citizen = citizen;
		this.path = new List<HexTile>();
	}

	internal void MakeCitizenMove(HexTile startTile, HexTile targetTile){
//		this.startTile = startTile;
//		this.targetTile = targetTile;
		this.transform.position = Vector3.MoveTowards (startTile.transform.position, targetTile.transform.position, 0.5f);
	}

	void OnMouseOver(){
		if (!UIManager.Instance.IsMouseOnUI()) {
//			if (citizen.assignedRole == null) {
//				return;
//			}
			if(this.citizen.assignedRole != null && this.citizen.role == ROLE.TRADER){
				if(this.citizen.assignedRole is Trader){
					Trader trader = (Trader)citizen.assignedRole;
					string text = "Name: [b]" + trader.citizen.name + "[/b]\n Home: [b]" + trader.homeCity.name + "[/b]\n Target: [b]" + trader.targetCity.name + "[/b]\n Trader Buffs: \n";
					for (int i = 0; i < trader.currentlySelling.Count; i++) {
						text += "[i]" + trader.currentlySelling [i].ToString () + "[/i]";
						if (i != (trader.currentlySelling.Count - 1)) {
							text += ", ";
						}
					}
					this.path = trader.pathToTargetCity;
					UIManager.Instance.ShowSmallInfo (text, UIManager.Instance.transform);
					this.HighlightPath();
				}
			}
		}

//		if (this.startTile != null && this.targetTile != null) {
//			transform.position = Vector3.SmoothDamp(this.startTile.transform.position, this.targetTile.transform.position, ref velocity, smoothTime);
//			if (Mathf.Approximately(transform.position.x, this.targetTile.transform.position.x) && Mathf.Approximately(transform.position.y, this.targetTile.transform.position.y)) {
//				this.targetTile = null;
//				this.startTile = null;
//			}
//		}
	}

	void OnMouseDown(){
		if (!UIManager.Instance.IsMouseOnUI()) {
			UIManager.Instance.ShowCitizenInfo (this.citizen);
		}
	}

	void OnMouseExit(){
		UIManager.Instance.HideSmallInfo();
		this.UnHighlightPath();
	}

	void HighlightPath(){
		for (int i = 0; i < path.Count; i++) {
			if (!path [i].isHabitable) {
				path [i].SetTileColor (Color.gray);
			}
		}
	}

	void UnHighlightPath(){
		for (int i = 0; i < path.Count; i++) {
			if (!path [i].isHabitable) {
				path [i].SetTileColor (Color.white);
			}
		}
	}
}
