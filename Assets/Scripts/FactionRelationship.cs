﻿using UnityEngine;
using System.Collections;

public class FactionRelationship {

    protected Faction _faction1;
    protected Faction _faction2;

    protected RELATIONSHIP_STATUS _realtionshipStatus;

    protected int _sharedOpinion;

	protected bool _isAtWar;
	protected bool _isAdjacent;

    #region getters/setters
    public RELATIONSHIP_STATUS relationshipStatus {
        get { return _realtionshipStatus; }
    }
    public int sharedOpinion {
        get { return _sharedOpinion; }
    }
	public bool isAtWar {
		get { return _isAtWar; }
	}
	public bool isAdjacent {
		get { return _isAdjacent; }
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
        _sharedOpinion = 0;
        _realtionshipStatus = RELATIONSHIP_STATUS.NEUTRAL; //TODO: Change this when logic is updated
		_isAtWar = false;
		_isAdjacent = false; //TODO: Faction Adjacency
    }

    #region Shared Opinion
    public void AdjustSharedOpinion(int amount) {
        _sharedOpinion += amount;
        _sharedOpinion = Mathf.Clamp(_sharedOpinion, -100, 100);
    }
    #endregion

    #region Relationship Status
    public void ChangeRelationshipStatus(RELATIONSHIP_STATUS newStatus) {
        if(newStatus == _realtionshipStatus) {
            return;
        }
        _realtionshipStatus = newStatus;
    }
    #endregion

    #region War
    public void SetWarStatus(bool warStatus) {
        _isAtWar = warStatus;
    }
    #endregion

}
