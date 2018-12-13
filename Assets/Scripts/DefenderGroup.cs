
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenderGroup {

    public Party party;
    //public DefenderToken token;
    public Area defendingArea;
	
    public DefenderGroup() {
        //this.party = party;
        //intel = new DefenderToken(this);
        Messenger.AddListener<Party>(Signals.PARTY_DIED, OnPartyDied);
    }

    public void AddCharacterToGroup(Character character) {
        if (party == null) {
            party = character.ownParty;
        } else {
            party.AddCharacter(character);
        }
        character.OnSetAsDefender(defendingArea);
    }
    public void RemoveCharacterFromGroup(Character defender) {
        //party.RemoveCharacter(defender);
        if (party != null) {
            if (party.owner.id == defender.id) {
                //if the character that needs to be removed is the owner of the defender party
                //check if there are any other characters left in the defender party
                List<Character> otherCharacters = new List<Character>();
                for (int i = 0; i < party.characters.Count; i++) {
                    Character currCharacter = party.characters[i];
                    if (party.owner.id != currCharacter.id) {
                        otherCharacters.Add(currCharacter);
                    }
                }
                if (otherCharacters.Count > 0) {
                    //if there are other characters left
                    //set the defender party to the party of the first remaining character, 
                    //then add all other characters to that party
                    Party partyToUse = otherCharacters[0].ownParty;
                    party = partyToUse;
                    for (int i = 1; i < otherCharacters.Count; i++) {
                        partyToUse.AddCharacter(otherCharacters[i]);
                    }
                } else {
                    //there are no more other characters other than the owner
                    //set the defenders to null
                    party = null;
                }
                defender.OnRemoveAsDefender();
            } else {
                if (party.characters.Contains(defender)) {
                    party.RemoveCharacter(defender);
                    defender.OnRemoveAsDefender();
                }
            }
        }
    }

    public void SetDefendingArea(Area area) {
        defendingArea = area;
    }

    private void GroupDeath() {
        defendingArea.RemoveDefenderGroup(this);
        Messenger.RemoveListener<Party>(Signals.PARTY_DIED, OnPartyDied);
    }

    private void OnPartyDied(Party partyThatDied) {
        if (this.party.id == partyThatDied.id) {
            GroupDeath();
        }
    }
}
