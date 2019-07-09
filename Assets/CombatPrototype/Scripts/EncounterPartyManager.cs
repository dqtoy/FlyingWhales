using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EncounterPartyManager : MonoBehaviour {
	public static EncounterPartyManager Instance;

    //private List<EncounterParty> _allEncounterParties;

    void Awake(){
		Instance = this;
	}
	//internal void Initialize(){
		//_allEncounterParties = new List<EncounterParty>();
		//foreach (Transform child in this.transform) {
		//	if(child.gameObject.activeSelf){
		//		EncounterParty encounterParty = child.GetComponent<EncounterParty> ();
		//		encounterParty.Initialize();
		//		_allEncounterParties.Add(encounterParty);
		//	}
		//}
	//}
	//internal EncounterParty GetEncounterParty(string partyName){
	//	for (int i = 0; i < _allEncounterParties.Count; i++) {
	//		EncounterParty encounterParty = _allEncounterParties[i];
	//		if(encounterParty.gameObject.name.ToLower() == partyName.ToLower()){
	//			return encounterParty;
	//		}
	//	}
	//	return null;
	//}
	//internal EncounterParty GetRandomEncounterParty(){
	//	EncounterParty encounterParty = _allEncounterParties[UnityEngine.Random.Range(0, _allEncounterParties.Count)];
	//	return encounterParty;
	//}
}
