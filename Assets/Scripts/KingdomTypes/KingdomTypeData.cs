using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
	private EventRate[] _dailyCumulativeEventRate;

	[SerializeField]
	private CharacterValue[] _characterValues;

	[SerializeField]
	private int _tradeRouteCap; // number of active trade routes that this kingdom is involved with

	[SerializeField]
	private KingdomTypeData generalKingdomTypeData;

	private int _hexDistanceModifier = 15;

	private Dictionary<WAR_TRIGGER, int> _dictWarTriggers = new Dictionary<WAR_TRIGGER, int> ();

	private Dictionary<MILITARY_STRENGTH, int> _dictWarRateModifierMilitary = new Dictionary<MILITARY_STRENGTH, int> ();

	private Dictionary<RELATIONSHIP_STATUS, int> _dictWarRateModifierRelationship = new Dictionary<RELATIONSHIP_STATUS, int> ();

	private Dictionary<CHARACTER_VALUE, int> _dictCharacterValues = new Dictionary<CHARACTER_VALUE, int> ();


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
			return this._eventRates.Concat(this.generalKingdomTypeData.eventRates).ToArray();
		}
	}

	public EventRate[] dailyCumulativeEventRate {
		get { 
			return this._dailyCumulativeEventRate.Concat(this.generalKingdomTypeData.dailyCumulativeEventRate).ToArray(); 
		}
	}

	public CharacterValue[] characterValues {
		get { 
			return this._characterValues.Concat(this.generalKingdomTypeData.characterValues).ToArray(); 
		}
	}

	public WarTrigger[] warTriggers {
		get { 
			return this._warTriggers.Concat(this.generalKingdomTypeData.warTriggers).ToArray(); 
		}
	}

	public WarRateModifierMilitary[] _warRateModifierMilitary {
		get { 
			return this.warRateModifierMilitary.Concat(this.generalKingdomTypeData._warRateModifierMilitary).ToArray(); 
		}
	}

	public WarRateModifierRelationship[] _warRateModifierRelationship {
		get { 
			return this.warRateModifierRelationship.Concat(this.generalKingdomTypeData.warRateModifierRelationship).ToArray(); 
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

	public Dictionary<CHARACTER_VALUE, int> dictCharacterValues {
		get { 
			return this._dictCharacterValues; 
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
		this._dictCharacterValues.Clear ();

		for (int i = 0; i < this.warTriggers.Length; i++) {
			this._dictWarTriggers.Add (this.warTriggers [i].warTrigger, this.warTriggers [i].rate);
		}
		for (int i = 0; i < this._warRateModifierMilitary.Length; i++) {
			this._dictWarRateModifierMilitary.Add (this._warRateModifierMilitary [i].militaryStrength, this._warRateModifierMilitary [i].rate);
		}
		for (int i = 0; i < this._warRateModifierRelationship.Length; i++) {
			this._dictWarRateModifierRelationship.Add (this._warRateModifierRelationship [i].relationshipStatus, this._warRateModifierRelationship [i].rate);
		}
		for (int i = 0; i < this.characterValues.Length; i++) {
			this._dictCharacterValues.Add (this.characterValues [i].character, this.characterValues [i].value);
		}
	}
}