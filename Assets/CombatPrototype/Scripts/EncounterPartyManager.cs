using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EncounterPartyManager : MonoBehaviour {
	public static EncounterPartyManager Instance;

	void Awake(){
		Instance = this;
	}
	internal void Initialize(){
		foreach (Transform child in this.transform) {
			child.GetComponent<EncounterParty> ().Initialize();
		}
	}
	internal EncounterParty GetEncounterParty(string partyName){
		foreach (Transform child in this.transform) {
			if(child.gameObject.name.ToLower() == partyName.ToLower()){
				return child.GetComponent<EncounterParty> ();
			}
		}
		return null;
	}
	internal EncounterParty GetRandomEncounterParty(){
		Transform randomChild = this.transform.GetChild(UnityEngine.Random.Range(0, this.transform.childCount));
		return randomChild.GetComponent<EncounterParty>();
	}
}
