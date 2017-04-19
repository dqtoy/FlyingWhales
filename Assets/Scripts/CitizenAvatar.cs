using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CitizenAvatar : MonoBehaviour {

	private Citizen citizen;

	private List<HexTile> path;

	internal void AssignCitizen(Citizen citizen){
		this.citizen = citizen;
		this.path = new List<HexTile>();
		if (citizen.role == ROLE.TRADER) {
			this.path = ((Trader)citizen.assignedRole).pathToTargetCity;
		}
	}

	internal void MakeCitizenMove(HexTile startTile, HexTile targetTile){
		this.transform.position = Vector3.MoveTowards (startTile.transform.position, targetTile.transform.position, 0.5f);
	}

	void OnMouseOver(){
		if (!UIManager.Instance.IsMouseOnUI()) {
			string text = "Trader Buffs ";
			Trader trader = (Trader)citizen.assignedRole;
			for (int i = 0; i < trader.currentlySelling.Count; i++) {
				text += trader.currentlySelling [i].ToString ();
				if (i != (trader.currentlySelling.Count - 1)) {
					text += ", ";
				}
			}
			UIManager.Instance.ShowSmallInfo (text, UIManager.Instance.transform);
			this.HighlightPath();
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
