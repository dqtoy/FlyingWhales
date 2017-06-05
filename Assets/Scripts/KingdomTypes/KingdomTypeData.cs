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

	[SerializeField]
	private int _warGeneralCreationRate;

	[SerializeField]
	private int _warReinforcementCreationRate;

	[SerializeField]
	private AgentCreationRate[] _agentCreationRate;

	private int _hexDistanceModifier = 15;

	private Dictionary<WAR_TRIGGER, int> _dictWarTriggers = new Dictionary<WAR_TRIGGER, int> ();

	private Dictionary<MILITARY_STRENGTH, int> _dictWarRateModifierMilitary = new Dictionary<MILITARY_STRENGTH, int> ();

	private Dictionary<RELATIONSHIP_STATUS, int> _dictWarRateModifierRelationship = new Dictionary<RELATIONSHIP_STATUS, int> ();

	private Dictionary<EVENT_TYPES, int> _dictAgentCreationRate = new Dictionary<EVENT_TYPES, int> ();

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

	public Dictionary<WAR_TRIGGER, int> dictWarTriggers {
		get { 
			return this._dictWarTriggers; 
		}
	}
	public Dictionary<MILITARY_STRENGTH, int> dictWarRateModifierMilitary {
		get { 
			return this._dictWarRateModifierMilitary; 
		}
	}

	public Dictionary<RELATIONSHIP_STATUS, int> dictWarRateModifierRelationship {
		get { 
			return this._dictWarRateModifierRelationship; 
		}
	}

	public Dictionary<EVENT_TYPES, int> dictAgentCreationRate {
		get { 
			return this._dictAgentCreationRate; 
		}
	}

	public int _warRateModifierPer15HexDistance {
		get { 
			return this.warRateModifierPer15HexDistance; 
		}
	}

	public int hexDistanceModifier {
		get { 
			return this._hexDistanceModifier; 
		}
	}

	public int _warRateModifierPerActiveWar {
		get { 
			return this.warRateModifierPerActiveWar; 
		}
	}

	public int warGeneralCreationRate {
		get { 
			return this._warGeneralCreationRate; 
		}
	}

	public int warReinforcementCreationRate {
		get { 
			return this._warReinforcementCreationRate; 
		}
	}

	void Awake(){
		this._dictWarTriggers.Clear ();
		this._dictWarRateModifierMilitary.Clear ();
		this._dictWarRateModifierRelationship.Clear ();
		this._dictAgentCreationRate.Clear ();

		for (int i = 0; i < this._warTriggers.Length; i++) {
			this._dictWarTriggers.Add (this._warTriggers [i].warTrigger, this._warTriggers [i].rate);
		}
		for (int i = 0; i < this.warRateModifierMilitary.Length; i++) {
			this._dictWarRateModifierMilitary.Add (this.warRateModifierMilitary [i].militaryStrength, this.warRateModifierMilitary [i].rate);
		}
		for (int i = 0; i < this.warRateModifierRelationship.Length; i++) {
			this._dictWarRateModifierRelationship.Add (this.warRateModifierRelationship [i].relationshipStatus, this.warRateModifierRelationship [i].rate);
		}
		for (int i = 0; i < this._agentCreationRate.Length; i++) {
			this._dictAgentCreationRate.Add (this._agentCreationRate [i].eventType, this._agentCreationRate [i].rate);
		}
	}
}