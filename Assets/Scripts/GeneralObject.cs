using UnityEngine;
using System.Collections;

public class GeneralObject : MonoBehaviour {
	public Citizen general;

	void OnTriggerEnter2D(Collider2D other){
		if(this.gameObject != null && other.gameObject != null){
			if(other.gameObject.GetComponent<GeneralObject>().general.city.kingdom.id != this.general.city.kingdom.id){
				CombatManager.Instance.BattleMidway(this.general, other.gameObject.GetComponent<GeneralObject>().general);
			}
		}

	}
}
