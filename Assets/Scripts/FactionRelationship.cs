using UnityEngine;
using System.Collections;

public class FactionRelationship {

    protected Faction _faction1;
    protected Faction _faction2;

    protected RELATIONSHIP_STATUS _realtionshipStatus;

    #region getters/setters
    public RELATIONSHIP_STATUS relationshipStatus {
        get { return _realtionshipStatus; }
    }
    #endregion

    public FactionRelationship(Faction faction1, Faction faction2) {
        _faction1 = faction1;
        _faction2 = faction2;

        _realtionshipStatus = RELATIONSHIP_STATUS.NEUTRAL; //TODO: Change this when logic is updated
    }
}
