using UnityEngine;
using System.Collections;

public class KingdomTypeData : MonoBehaviour {

	[SerializeField]
	private KINGDOM_TYPE _kingdomType;
	
	[SerializeField]
	private int _expansionRate;

	[SerializeField]
	private int _expansionDistanceFromBorder;


	[SerializeField]
	private int _eventStartRate;


	[SerializeField]
	private int _negativeCompatibilityRaidRate;

	[SerializeField]
	private int _negativeCompatibilityConflictRate;

	[SerializeField]
	private int _negativeCompatibilityCrisisRate;

	[SerializeField]
	private int _negativeCompatibilityStateVisitRate;

	/*
	[SerializeField]
	private int _neutralCompatibilityRaidRate;

	[SerializeField]
	private int _neutralCompatibilityConflictRate;

	[SerializeField]
	private int _neutralCompatibilityCrisisRate;

	[SerializeField]
	private int _neutralCompatibilityStateVisitRate;


	[SerializeField]
	private int _positiveCompatibilityRaidRate;

	[SerializeField]
	private int _positiveCompatibilityConflictRate;

	[SerializeField]
	private int _positiveCompatibilityCrisisRate;

	[SerializeField]
	private int _positiveCompatibilityStateVisitRate;
	*/

	[SerializeField]
	private RELATIONSHIP_STATUS[] _raidRelationshipTargets;

	[SerializeField]
	private KINGDOM_TYPE[] _raidKingdomTypes;

	[SerializeField]
	private MILITARY_STRENGTH[] _raidMilitaryStrength;


	[SerializeField]
	private RELATIONSHIP_STATUS[] _conflictRelationshipTargets;

	[SerializeField]
	private KINGDOM_TYPE[] _conflictKingdomTypes;

	[SerializeField]
	private MILITARY_STRENGTH[] _conflictMilitaryStrength;


	[SerializeField]
	private RELATIONSHIP_STATUS[] _crisisRelationshipTargets;

	[SerializeField]
	private KINGDOM_TYPE[] _crisisKingdomTypes;

	[SerializeField]
	private MILITARY_STRENGTH[] _crisisMilitaryStrength;


	[SerializeField]
	private RELATIONSHIP_STATUS[] _stateVisitRelationshipTargets;

	[SerializeField]
	private KINGDOM_TYPE[] _stateVisitKingdomTypes;

	[SerializeField]
	private MILITARY_STRENGTH[] _stateVisitMilitaryStrength;

	public KINGDOM_TYPE kingdomType {
		get { 
			return this._kingdomType; 
		}
	}

	public int expansionRate {
		get { 
			return this._expansionRate; 
		}
	}

	public int expansionDistanceFromBorder {
		get { 
			return this._expansionDistanceFromBorder; 
		}
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
