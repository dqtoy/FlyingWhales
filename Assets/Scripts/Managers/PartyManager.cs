using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PartyManager : MonoBehaviour {

    public static PartyManager Instance = null;

    public List<Party> allParties = new List<Party>();

    private void Awake() {
        Instance = this;
    }

    public void AddParty(Party newParty) {
        if (!allParties.Contains(newParty)) {
            allParties.Add(newParty);
        }
    }
    public void RemoveParty(Party party) {
        allParties.Remove(party);
    }

	public Party GetPartyByLeaderID(int id){
		for (int i = 0; i < allParties.Count; i++) {
			if(allParties[i].partyLeader.id == id){
				return allParties [i];
			}
		}
		return null;
	}
}
