using UnityEngine;
using System.Collections;

public class GeneralObject : MonoBehaviour {
	public General general;

	void OnTriggerEnter2D(Collider2D other){
		if(this.gameObject != null && other.gameObject != null){
			if(!Utilities.AreTwoGeneralsFriendly(other.gameObject.GetComponent<GeneralObject>().general, this.general)){
				if(!Utilities.AreTwoGeneralsFriendly(this.general, other.gameObject.GetComponent<GeneralObject>().general)){
					CombatManager.Instance.BattleMidway (this.general, other.gameObject.GetComponent<GeneralObject> ().general);
				}
			}
		}

	}

	internal void MakeCitizenMove(HexTile startTile, HexTile targetTile){
//		this.transform.position = Vector3.MoveTowards (startTile.transform.position, targetTile.transform.position, 0.5f);
		this.transform.position = targetTile.transform.position;
	}
}
