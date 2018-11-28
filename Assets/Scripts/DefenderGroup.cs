using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenderGroup {

    public Party party;
    public DefenderIntel intel;
    public Area defendingArea;
	
    public DefenderGroup(Party party) {
        this.party = party;
        intel = new DefenderIntel(this);
    }

    public void AddCharacterToGroup(ICharacter character) {
        party.AddCharacter(character);
    }
    public void RemoveCharacterFromGroup(ICharacter character) {
        party.RemoveCharacter(character);
    }

    public void SetDefendingArea(Area area) {
        defendingArea = area;
    }
}
