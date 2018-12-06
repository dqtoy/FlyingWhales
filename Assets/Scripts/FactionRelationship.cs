using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FactionRelationship {

    protected Faction _faction1;
    protected Faction _faction2;

    protected FACTION_RELATIONSHIP_STATUS _relationshipStatus;

    public int currentWarCombatCount { get; private set; } //this will be reset once relationship status is set to anything but AT_WAR

    #region getters/setters
    public FACTION_RELATIONSHIP_STATUS relationshipStatus {
		get { return _relationshipStatus; }
    }
	public Faction faction1 {
		get { return _faction1; }
	}
	public Faction faction2 {
		get { return _faction2; }
	}
    #endregion

    public FactionRelationship(Faction faction1, Faction faction2) {
        _faction1 = faction1;
        _faction2 = faction2;
    }

    #region Relationship Status
    public void ChangeRelationshipStatus(FACTION_RELATIONSHIP_STATUS newStatus) {
		if(newStatus == _relationshipStatus) {
            return;
        }
		_relationshipStatus = newStatus;
        if (_relationshipStatus != FACTION_RELATIONSHIP_STATUS.AT_WAR) {
            currentWarCombatCount = 0;
        }
    }
    #endregion

    public void AdjustWarCombatCount(int amount) {
        currentWarCombatCount += amount;
        currentWarCombatCount = Mathf.Max(0, currentWarCombatCount);
    }
}
