using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CitizenAvatar : MonoBehaviour {

	private Citizen citizen;

	private List<HexTile> path;

	internal void AssignCitizen(Citizen citizen){
		this.citizen = citizen;
	}

	internal void MakeCitizenMove(HexTile startTile, HexTile targetTile){
		this.transform.position = Vector3.MoveTowards (startTile.transform.position, targetTile.transform.position, 0.5f);
	}

	void OnMouseEnter(){
		if (!UIManager.Instance.IsMouseOnUI()) {
			string text = "Trader Buffs ";
			Trader trader = (Trader)citizen.assignedRole;
			for (int i = 0; i < trader.currentlySelling.Count; i++) {
				text += trader.currentlySelling [i].ToString ();
			}

			UIManager.Instance.ShowSmallInfo (text, UIManager.Instance.transform);
		}
	}

	void OnMouseExit(){
		UIManager.Instance.HideSmallInfo();
	}
}
