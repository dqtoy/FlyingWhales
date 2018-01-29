using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FactionRelationship {

    protected Faction _faction1;
    protected Faction _faction2;
	protected Dictionary<int, IndividualFactionRelationship> _factionLookup;
	protected IndividualFactionRelationship _indivFactionRelationship1;
	protected IndividualFactionRelationship _indivFactionRelationship2;

    protected RELATIONSHIP_STATUS _relationshipStatus;

    protected int _sharedOpinion;

	protected bool _isAtWar;
	protected bool _isAdjacent;

    #region getters/setters
    public RELATIONSHIP_STATUS relationshipStatus {
		get { return _relationshipStatus; }
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
	public Dictionary<int, IndividualFactionRelationship> factionLookup {
		get { return _factionLookup; }
	}
    #endregion

    public FactionRelationship(Faction faction1, Faction faction2) {
        _faction1 = faction1;
        _faction2 = faction2;
        _sharedOpinion = 0;
		_relationshipStatus = RELATIONSHIP_STATUS.NEUTRAL; //TODO: Change this when logic is updated
		_isAtWar = false;
		_isAdjacent = false; //TODO: Faction Adjacency
		_indivFactionRelationship1 = new IndividualFactionRelationship (_faction1, _faction2, this);
		_indivFactionRelationship2 = new IndividualFactionRelationship (_faction2, _faction1, this);

		_factionLookup = new Dictionary<int, IndividualFactionRelationship> () {
			{ _faction1.id, _indivFactionRelationship1 },
			{ _faction2.id, _indivFactionRelationship2 },
		};
    }

    #region Shared Opinion
    public void AdjustSharedOpinion(int amount) {
        _sharedOpinion += amount;
        _sharedOpinion = Mathf.Clamp(_sharedOpinion, -100, 100);
    }
    #endregion

    #region Relationship Status
    public void ChangeRelationshipStatus(RELATIONSHIP_STATUS newStatus) {
		if(newStatus == _relationshipStatus) {
            return;
        }
		_relationshipStatus = newStatus;
    }
    #endregion

    #region War
    public void SetWarStatus(bool warStatus) {
        _isAtWar = warStatus;
    }
    #endregion

	#region Alliance
	public bool AreAllies(){
		//TODO: Alliance
		return false;
	}
	#endregion
}
