using UnityEngine;
using System.Collections;

public class IndividualFactionRelationship {

	protected FactionRelationship _factionRelationship;
    protected Faction _sourceFaction;
    protected Faction _targetFaction;

    #region getters/setters
	public FactionRelationship factionRelationship{
		get { return _factionRelationship; }
	}
	public Faction sourceFaction{
		get { return _sourceFaction; }
	}
	public Faction targetFaction{
		get { return _targetFaction; }
	}
	public int relevantPower {
		get { return (int)((_sourceFaction.factionPower + AdjacentFactionPower()) - EnemyPower()); }
    }
	public int relativeStrength{
		get { return RelativeStrength (); }
	}
	public int threat{
		get {
			int str = relativeStrength;
			if(str > 0){
				return Mathf.Min (str, _sourceFaction.warmongering);
			}else{
				return str;
			}
		}
	}
    #endregion

	public IndividualFactionRelationship(Faction sourceFaction, Faction targetFaction, FactionRelationship factionRelationship) {
		_sourceFaction = sourceFaction;
		_targetFaction = targetFaction;
		_factionRelationship = factionRelationship;
    }

	#region Relevant Power
	private float AdjacentFactionPower(){ //Sum Adj Tribal Power
		float power = 0f;
		foreach (FactionRelationship fr in _targetFaction.relationships.Values) {
			if(fr.isAdjacent && !fr.isAtWar){
				Faction oppositeFaction = fr.factionLookup [_targetFaction.id].targetFaction;
				if(oppositeFaction.id != _sourceFaction.id){
					FactionRelationship frSourceToAlly = _sourceFaction.GetRelationshipWith (oppositeFaction);
					if(frSourceToAlly != null && frSourceToAlly.AreAllies()){
						power += oppositeFaction.factionPower;
					}
				}
			}
		}
		return power;
	}
	private float EnemyPower(){ //Sum Enemy Power
		float power = 0f;
		foreach (FactionRelationship fr in _sourceFaction.relationships.Values) {
			if(fr.isAtWar){
				power += fr.factionLookup [_sourceFaction.id].targetFaction.factionPower;
			}
		}
		return power;
	}
	#endregion

	#region Relative Strength
	private int RelativeStrength(){
		int sourceRelevantPower = _factionRelationship.factionLookup [_sourceFaction.id].relevantPower;
		int targetRelevantPower = _factionRelationship.factionLookup [_targetFaction.id].relevantPower;
		if(sourceRelevantPower >= targetRelevantPower){
			return (int)((((float)sourceRelevantPower / (float)targetRelevantPower) * 100f) - 100f);
		}else{
			return -(int)((((float)targetRelevantPower / (float)sourceRelevantPower) * 100f) - 100f);
		}
	}
	#endregion
}
