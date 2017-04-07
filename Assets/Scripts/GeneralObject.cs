using UnityEngine;
using System.Collections;

public class GeneralObject : MonoBehaviour {
	public General general;

	void OnTriggerEnter2D(Collider2D other){
		if(this.gameObject != null && other.gameObject != null){
			if(other.gameObject.GetComponent<GeneralObject>().general.citizen.city.kingdom.id != this.general.citizen.city.kingdom.id){
				CombatManager.Instance.BattleMidway(this.general, other.gameObject.GetComponent<GeneralObject>().general);
			}
		}

	}
}
