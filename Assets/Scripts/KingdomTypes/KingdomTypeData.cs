using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class KingdomTypeData : MonoBehaviour {

	[SerializeField]
	private KINGDOM_TYPE _kingdomType;

	[SerializeField]
	private KINGDOM_SIZE _kingdomSize;

	[SerializeField]
	private PURPOSE _purpose;
	
	[SerializeField]
	private int _expansionRate;

	[SerializeField]
	private int _expansionDistanceFromBorder;


	[SerializeField]
	private int _eventStartRate;
//
//	[SerializeField]
//	private EventRate[] _eventRates;
//
//	[SerializeField]
//	private AssassinationTrigger[] _assassinationTriggers;
//
//	[SerializeField]
//	private WarRateModifierMilitary[] _assassinationRateModifierMilitary;
//
//	[SerializeField]
//	private WarRateModifierRelationship[] _assassinationRateModifierRelationship;
//
//	[SerializeField]
//	private WarTrigger[] _warTriggers;
//
//	[SerializeField]
//	private WarRateModifierMilitary[] warRateModifierMilitary;
//
//	[SerializeField]
//	private WarRateModifierRelationship[] warRateModifierRelationship;
//
//	[SerializeField]
//	private int warRateModifierPer15HexDistance;
//
//	[SerializeField]
//	private int warRateModifierPerActiveWar;
//
//	[SerializeField]
//	private int _warGeneralCreationRate;
//
//	[SerializeField]
//	private int _warReinforcementCreationRate;
	[SerializeField]
	private int _tradeRouteCap; // number of active trade routes that this kingdom is involved with


	[SerializeField]
	private EventRate[] _dailyCumulativeEventRate;

//	[SerializeField]
//	private EventRate[] _reactionEventRate;
//
	[SerializeField]
	private CharacterValue[] _characterValues;

	[SerializeField]
	private RelationshipKingdomType[] _relationshipKingdomType;

	[SerializeField]
	private CombatStats _combatStats;

	[SerializeField]
	private ProductionPointsSpend _productionPointsSpend;

    [SerializeField]
    private PopulationRates _populationRates;

	[SerializeField]
	private int _prepareForWarChance;

	[SerializeField]
	private KingdomTypeData generalKingdomTypeData;

	private int _hexDistanceModifier = 15;

//	private Dictionary<WAR_TRIGGER, int> _dictWarTriggers = new Dictionary<WAR_TRIGGER, int> ();
//
//	private Dictionary<MILITARY_STRENGTH, int> _dictWarRateModifierMilitary = new Dictionary<MILITARY_STRENGTH, int> ();
//
//	private Dictionary<RELATIONSHIP_STATUS, int> _dictWarRateModifierRelationship = new Dictionary<RELATIONSHIP_STATUS, int> ();
//
//	private Dictionary<ASSASSINATION_TRIGGER, int> _dictAssassinationTriggers = new Dictionary<ASSASSINATION_TRIGGER, int> ();
//
//	private Dictionary<MILITARY_STRENGTH, int> _dictAssassinationRateModifierMilitary = new Dictionary<MILITARY_STRENGTH, int> ();
//
//	private Dictionary<RELATIONSHIP_STATUS, int> _dictAssassinationRateModifierRelationship = new Dictionary<RELATIONSHIP_STATUS, int> ();

	//private Dictionary<CHARACTER_VALUE, int> _dictCharacterValues = new Dictionary<CHARACTER_VALUE, int> ();

	private Dictionary<KINGDOM_TYPE, int> _dictRelationshipKingdomType = new Dictionary<KINGDOM_TYPE, int> ();

	public KINGDOM_TYPE kingdomType {
		get { 
			return this._kingdomType; 
		}
	}

	public KINGDOM_SIZE kingdomSize {
		get { 
			return this._kingdomSize; 
		}
	}

	public PURPOSE purpose {
		get { 
			return this._purpose; 
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

//	public EventRate[] eventRates {
//		get { 
//			return this._eventRates;
//		}
//	}

	public EventRate[] dailyCumulativeEventRate {
		get { 
			return this._dailyCumulativeEventRate;
		}
	}

//	public EventRate[] reactionEventRate {
//		get { 
//			return this._reactionEventRate;
//		}
//	}

	public CharacterValue[] characterValues {
		get { 
			return this._characterValues;
		}
	}

	public CombatStats combatStats {
		get { 
			return this._combatStats;
		}
	}

//	public WarTrigger[] warTriggers {
//		get { 
//			return this._warTriggers;
//		}
//	}
//
//	public WarRateModifierMilitary[] _warRateModifierMilitary {
//		get { 
//			return this.warRateModifierMilitary;
//		}
//	}
//
//	public WarRateModifierRelationship[] _warRateModifierRelationship {
//		get { 
//			return this.warRateModifierRelationship;
//		}
//	}
//
//	public AssassinationTrigger[] assassinationTriggers {
//		get { 
//			return this._assassinationTriggers;
//		}
//	}
//
//	public WarRateModifierMilitary[] assassinationRateModifierMilitary {
//		get { 
//			return this._assassinationRateModifierMilitary;
//		}
//	}
//
//	public WarRateModifierRelationship[] assassinationRateModifierRelationship {
//		get { 
//			return this._assassinationRateModifierRelationship;
//		}
//	}

	public RelationshipKingdomType[] relationshipKingdomType {
		get { 
			return this._relationshipKingdomType;
		}
	}

//	public Dictionary<WAR_TRIGGER, int> dictWarTriggers {
//		get { 
//			return this._dictWarTriggers; 
//		}
//	}
//	public Dictionary<MILITARY_STRENGTH, int> dictWarRateModifierMilitary {
//		get { 
//			return this._dictWarRateModifierMilitary; 
//		}
//	}
//
//	public Dictionary<RELATIONSHIP_STATUS, int> dictWarRateModifierRelationship {
//		get { 
//			return this._dictWarRateModifierRelationship; 
//		}
//	}

	//public Dictionary<CHARACTER_VALUE, int> dictCharacterValues {
	//	get { 
	//		return this._dictCharacterValues; 
	//	}
	//}
	public Dictionary<KINGDOM_TYPE, int> dictRelationshipKingdomType {
		get { 
			return this._dictRelationshipKingdomType; 
		}
	}

//	public int _warRateModifierPer15HexDistance {
//		get { 
//			return this.warRateModifierPer15HexDistance; 
//		}
//	}

	public int hexDistanceModifier {
		get { 
			return this._hexDistanceModifier; 
		}
	}

//	public int _warRateModifierPerActiveWar {
//		get { 
//			return this.warRateModifierPerActiveWar; 
//		}
//	}
//
//	public int warGeneralCreationRate {
//		get { 
//			return this._warGeneralCreationRate; 
//		}
//	}
//
//	public int warReinforcementCreationRate {
//		get { 
//			return this._warReinforcementCreationRate; 
//		}
//	}

    public ProductionPointsSpend productionPointsSpend {
        get { return _productionPointsSpend; }
    }

    public PopulationRates populationRates {
        get { return _populationRates; }
    }

    public int prepareForWarChance {
		get { 
			return this._prepareForWarChance; 
		}
	}

    void Awake(){
		if(this.generalKingdomTypeData != null){
//			this._eventRates = this._eventRates.Concat (this.generalKingdomTypeData.eventRates).ToArray ();
//			this._warTriggers = this._warTriggers.Concat (this.generalKingdomTypeData.warTriggers).ToArray ();
//			this.warRateModifierMilitary = this.warRateModifierMilitary.Concat (this.generalKingdomTypeData._warRateModifierMilitary).ToArray ();
//			this.warRateModifierRelationship = this.warRateModifierRelationship.Concat (this.generalKingdomTypeData._warRateModifierRelationship).ToArray ();
//			this._assassinationTriggers = this._assassinationTriggers.Concat (this.generalKingdomTypeData.assassinationTriggers).ToArray ();
//			this._assassinationRateModifierMilitary = this._assassinationRateModifierMilitary.Concat (this.generalKingdomTypeData.assassinationRateModifierMilitary).ToArray ();
//			this._assassinationRateModifierRelationship = this._assassinationRateModifierRelationship.Concat (this.generalKingdomTypeData.assassinationRateModifierRelationship).ToArray ();
			this._dailyCumulativeEventRate = this._dailyCumulativeEventRate.Concat (this.generalKingdomTypeData._dailyCumulativeEventRate).ToArray ();
//			this._reactionEventRate = this._reactionEventRate.Concat (this.generalKingdomTypeData.reactionEventRate).ToArray ();
			this._characterValues = this._characterValues.Concat (this.generalKingdomTypeData._characterValues).ToArray ();
			this._relationshipKingdomType = this._relationshipKingdomType.Concat (this.generalKingdomTypeData._relationshipKingdomType).ToArray ();

		}


//		this._dictWarTriggers.Clear ();
//		this._dictWarRateModifierMilitary.Clear ();
//		this._dictWarRateModifierRelationship.Clear ();
//		this._dictAssassinationTriggers.Clear ();
//		this._dictAssassinationRateModifierMilitary.Clear ();
//		this._dictAssassinationRateModifierRelationship.Clear ();
		//this._dictCharacterValues.Clear ();
		this._dictRelationshipKingdomType.Clear ();

//		for (int i = 0; i < this.warTriggers.Length; i++) {
//			this._dictWarTriggers.Add (this.warTriggers [i].warTrigger, this.warTriggers [i].rate);
//		}
//		for (int i = 0; i < this._warRateModifierMilitary.Length; i++) {
//			this._dictWarRateModifierMilitary.Add (this._warRateModifierMilitary [i].militaryStrength, this._warRateModifierMilitary [i].rate);
//		}
//		for (int i = 0; i < this._warRateModifierRelationship.Length; i++) {
//			this._dictWarRateModifierRelationship.Add (this._warRateModifierRelationship [i].relationshipStatus, this._warRateModifierRelationship [i].rate);
//		}
//
//		for (int i = 0; i < this.assassinationTriggers.Length; i++) {
//			this._dictAssassinationTriggers.Add (this.assassinationTriggers [i].assassinationTrigger, this.assassinationTriggers [i].rate);
//		}
//		for (int i = 0; i < this.assassinationRateModifierMilitary.Length; i++) {
//			this._dictAssassinationRateModifierMilitary.Add (this.assassinationRateModifierMilitary [i].militaryStrength, this.assassinationRateModifierMilitary [i].rate);
//		}
//		for (int i = 0; i < this.assassinationRateModifierRelationship.Length; i++) {
//			this._dictAssassinationRateModifierRelationship.Add (this.assassinationRateModifierRelationship [i].relationshipStatus, this.assassinationRateModifierRelationship [i].rate);
//		}
		//for (int i = 0; i < this.characterValues.Length; i++) {
		//	this._dictCharacterValues.Add (this.characterValues [i].character, this.characterValues [i].value);
		//}
		for (int i = 0; i < this.relationshipKingdomType.Length; i++) {
			this._dictRelationshipKingdomType.Add (this.relationshipKingdomType [i].kingdomType, this.relationshipKingdomType [i].relationshipModifier);
		}
	}
}