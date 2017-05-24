using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
	private EventRate[] _eventRates;

	[SerializeField]
	private WarTrigger[] _warTriggers;

	[SerializeField]
	private WarRateModifierMilitary[] warRateModifierMilitary;

	[SerializeField]
	private WarRateModifierRelationship[] warRateModifierRelationship;

	[SerializeField]
	private int warRateModifierPer15HexDistance;

	[SerializeField]
	private int warRateModifierPerActiveWar;

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

	public int eventStartRate {
		get { 
			return this._eventStartRate; 
		}
	}

	public EventRate[] eventRates {
		get { 
			return this._eventRates; 
		}
	}
}