using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenderGroup {

    public Party party;
    public DefenderIntel intel;
    public Area defendingArea;
	
    public DefenderGroup() {
        //this.party = party;
        intel = new DefenderIntel(this);
    }

    public void AddCharacterToGroup(ICharacter character) {
        if (party == null) {
            party = character.ownParty;
        } else {
            party.AddCharacter(character);
        }
        (character as Character).OnSetAsDefender(defendingArea);
    }
    public void RemoveCharacterFromGroup(ICharacter defender) {
        //party.RemoveCharacter(defender);
        if (party != null) {
            if (party.owner.id == defender.id) {
                //if the character that needs to be removed is the owner of the defender party
                //check if there are any other characters left in the defender party
                List<ICharacter> otherCharacters = new List<ICharacter>();
                for (int i = 0; i < party.icharacters.Count; i++) {
                    ICharacter currCharacter = party.icharacters[i];
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
                if (defender is Character) {
                    (defender as Character).OnRemoveAsDefender();
                }
            } else {
                if (party.icharacters.Contains(defender)) {
                    party.RemoveCharacter(defender);
                    if (defender is Character) {
                        (defender as Character).OnRemoveAsDefender();
                    }
                }
            }
        }
    }

    public void SetDefendingArea(Area area) {
        defendingArea = area;
    }
}
