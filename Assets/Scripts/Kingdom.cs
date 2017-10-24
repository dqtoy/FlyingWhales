using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

[System.Serializable]
public class Kingdom{
    [Space(10)]
    [Header("General Info")]
    public int id;
	public string name;
    public string kingdomTag; //Used for pathfinding
    public int kingdomTagIndex; //Used for pathfinding
    public RACE race;
    public int age;
    private int _prestige;
    private int _population;
    private int _populationCapacity;
    private int foundationYear;
    private int foundationMonth;
    private int foundationDay;

	private KingdomTypeData _kingdomTypeData;
    private KINGDOM_SIZE _kingdomSize;
	private Kingdom _sourceKingdom;
	private Kingdom _mainThreat;
	private int _evenActionDay;
    private int _oddActionDay;

    //Resources
    private Dictionary<RESOURCE, int> _availableResources; //only includes resources that the kingdom has bought via tile purchasing
    internal BASE_RESOURCE_TYPE basicResource;

    //Trading
    private Dictionary<Kingdom, EMBARGO_REASON> _embargoList;

    private int _baseArmor;
    private int _baseWeapons;
    private int _baseStability;
    private int _stability;
    private List<City> _cities;
    private List<Region> _regions;
	private List<Camp> camps;
	internal City capitalCity;
	internal Citizen king;
    internal Citizen nextInLine;
	internal List<Citizen> successionLine;
    private Dictionary<City, List<Citizen>> _citizens;


	internal Dictionary<Kingdom, KingdomRelationship> relationships;

	internal Color kingdomColor;
	internal List<History> kingdomHistory;

	[NonSerialized] private List<Kingdom> _discoveredKingdoms;

	//Plague
	internal Plague plague;


	//Daily Cumulative
	private EventRate[] _dailyCumulativeEventRate;
	
	//Tech
	private int _techLevel;
	private int _techCapacity;
	private int _techCounter;

	//The First and The Keystone
	internal FirstAndKeystoneOwnership firstAndKeystoneOwnership;
    private bool _isGrowthEnabled;

	//Serum of Alacrity
	private int _serumsOfAlacrity;

    //FogOfWar
    private FOG_OF_WAR_STATE[,] _fogOfWar;
	private Dictionary<FOG_OF_WAR_STATE, HashSet<HexTile>> _fogOfWarDict;
    private Dictionary<Region, FOG_OF_WAR_STATE> _regionFogOfWarDict;

    //Crimes
    private CrimeData _crimeData;
	private CrimeDate _crimeDate;

	//Events of Kingdom
	private List<GameEvent> _activeEvents;
	private List<GameEvent> _doneEvents;

    //Expansion
    private int _expansionRate;

	//Balance of Power
//	private int _effectivePower;
//	private int _effectiveDefense;
	private bool _isMobilizing;
    [NonSerialized] private List<Kingdom> _adjacentKingdoms;
//	[NonSerialized] private List<Kingdom> _allianceKingdoms;

    //protected Dictionary<CHARACTER_VALUE, int> _dictCharacterValues;
    //protected Dictionary<CHARACTER_VALUE, int> _importantCharacterValues;

    protected const int INCREASE_CITY_HP_CHANCE = 5;
	protected const int INCREASE_CITY_HP_AMOUNT = 20;
    protected const int GOLD_GAINED_FROM_TRADE = 10;
    protected const int UNREST_DECREASE_PER_MONTH = -5;
    protected const int STABILITY_DECREASE_CONQUER = -5;
    protected const int STABILITY_DECREASE_EMBARGO = -5;

	private bool _isDead;
	private bool _hasBioWeapon;
	private bool _isLockedDown;
	private bool _isTechProducing;
	private bool _isMilitarize;
	private bool _isFortifying;

	private int borderConflictLoyaltyExpiration;
	private float _techProductionPercentage;
	private float _productionGrowthPercentage;

	private bool _hasUpheldHiddenHistoryBook;

	private bool _hasSecession;

	private List<Citizen> orderedMaleRoyalties;
	private List<Citizen> orderedFemaleRoyalties;
	private List<Citizen> orderedBrotherRoyalties;
	private List<Citizen> orderedSisterRoyalties;

	//Kingdom Threat
	private int _warmongerValue;

	//Alliance
	private AlliancePool _alliancePool;

	//Warfare
	private Dictionary<int, WarfareInfo> _warfareInfo;

    private float _researchRateFromKing;
    private float _draftRateFromKing;
    private float _productionRateFromKing;

    private int _stabilityDecreaseFromInvasionCounter;
    internal List<GameDate> datesStabilityDecreaseWillExpire = new List<GameDate>(); //TODO Remove this when testing is done

	internal Kingdom highestThreatAdjacentKingdomAbove50;
	internal float highestThreatAdjacentKingdomAbove50Value;

	internal Kingdom highestRelativeStrengthAdjacentKingdom;
	internal int highestRelativeStrengthAdjacentKingdomValue;

	internal bool has100OrAboveThreat;

	internal List<int> checkedWarfareID;

    #region getters/setters
    public KINGDOM_TYPE kingdomType {
		get { 
			if (this._kingdomTypeData == null) {
				return KINGDOM_TYPE.NONE;
			}
			return this._kingdomTypeData.kingdomType; 
		}
	}
	public KingdomTypeData kingdomTypeData {
		get { return this._kingdomTypeData; }
	}
    internal KINGDOM_SIZE kingdomSize {
        get { return _kingdomSize; }
    }
	public Kingdom sourceKingdom {
		get { return this._sourceKingdom; }
	}
	public Kingdom mainThreat {
		get { return this._mainThreat; }
	}
    public int prestige {
        get { return _prestige; }
    }
    internal int population {
        get { return _population; }
    }
    internal int populationCapacity {
        get { return _populationCapacity; }
    }
    public int cityCap {
        get { return Mathf.FloorToInt(prestige / GridMap.Instance.numOfRegions); }
    }
	public Dictionary<RESOURCE, int> availableResources{
		get{ return this._availableResources; }
	}
    public Dictionary<Kingdom, EMBARGO_REASON> embargoList {
        get { return this._embargoList;  }
    }
	public bool isDead{
		get{ return this._isDead; }
	}
	public List<City> cities{
		get{ return this._cities; }
	}
    internal List<Region> regions {
        get { return _regions; }
    }
//	public List<Camp> camps{
//		get{ return this._camps; }
//	}
    public int stability {
        get { return this._stability; }
        //get { return -100; }
        //		set { this._stability = value;}
    }
    public int basicResourceCount {
        get { return this._availableResources.Where(x => Utilities.GetBaseResourceType(x.Key) == this.basicResource).Sum(x => x.Value); }
    }
    public List<Kingdom> discoveredKingdoms {
        get { return this._discoveredKingdoms; }
    }
	public int techLevel{
		get{return this._techLevel;}
	}
	public int techCapacity{
		get{return this._techCapacity;}
	}
	public int techCounter{
		get{return this._techCounter;}
	}
    public int expansionRate {
        get { return _expansionRate; }
    }
    //public Dictionary<CHARACTER_VALUE, int> dictCharacterValues {
    //    get { return this._dictCharacterValues; }
    //}
    //public Dictionary<CHARACTER_VALUE, int> importantCharacterValues {
    //    get { return this._importantCharacterValues; }
    //}
    public bool hasBioWeapon {
		get { return this._hasBioWeapon; }
	}
	public EventRate[] dailyCumulativeEventRate {
		get { return this._dailyCumulativeEventRate; }
	}
//    public List<City> plaguedCities {
//        get { return this.cities.Where(x => x.plague != null).ToList(); }
//    }
    public bool isGrowthEnabled {
        get { return _isGrowthEnabled; }
    }
	public int serumsOfAlacrity {
		get { return this._serumsOfAlacrity; }
	}
    public FOG_OF_WAR_STATE[,] fogOfWar {
        get { return _fogOfWar; }
    }
    public Dictionary<FOG_OF_WAR_STATE, HashSet<HexTile>> fogOfWarDict {
        get { return _fogOfWarDict; }
    }
    internal Dictionary<Region, FOG_OF_WAR_STATE> regionFogOfWarDict {
        get { return _regionFogOfWarDict; }
    }
//	public CombatStats combatStats {
//		get { return this._combatStats; }
//	}
//	public int waves{
//		get { return this._combatStats.waves - GetNumberOfWars();}
//	}
    public bool isLockedDown{
		get { return this._isLockedDown;}
	}
	public bool isTechProducing{
		get { return this._isTechProducing;}
	}
	public bool isMilitarize{
		get { return this._isMilitarize;}
	}
	public bool isFortifying{
		get { return this._isFortifying;}
	}
	public float productionGrowthPercentage {
		get { return this._productionGrowthPercentage; }
	}
	public bool hasUpheldHiddenHistoryBook{
		get { return this._hasUpheldHiddenHistoryBook;}
	}
	public bool hasSecession{
		get { return this._hasSecession;}
	}
	public List<GameEvent> activeEvents{
		get { return this._activeEvents;}
	}
	public List<GameEvent> doneEvents{
		get { return this._doneEvents;}
	}
	public int baseWeapons{
		get { return _baseWeapons;}
	}
	public int baseArmor{
		get { return _baseArmor;}
	}
//	public int effectiveWeapons{
//		get { return this.effectiveAttack + (int)(this.effectiveDefense / 3) + (int)(GetPosAllianceWeapons() / 3);}
//	}
//	public int effectiveArmor{
//		get { return this.effectiveDefense + (int)(this.effectiveAttack / 3) + (int)(GetPosAllianceArmor() / 3);}
//	}
	public List<Kingdom> adjacentKingdoms{
        get { return _adjacentKingdoms; }
    }
//	public List<Kingdom> allianceKingdoms{
//		get { return this._allianceKingdoms;}
//	}
	public bool isMobilizing{
		get { return this._isMobilizing;}
	}
	public float techProductionPercentage{
		get { return this._techProductionPercentage;}
	}
	public int actionDay{
		get { return this._evenActionDay;}
	}
    public int oddActionDay {
        get { return this._oddActionDay; }
    }
    public int warmongerValue{
		get { return this._warmongerValue;}
	}
	public AlliancePool alliancePool{
		get { return this._alliancePool;}
	}
	public Dictionary<int, WarfareInfo> warfareInfo{
		get { return this._warfareInfo;}
	}
    internal int scientists {
        get { return Mathf.Max(1, Mathf.FloorToInt(population * researchRate)); }
    }
    internal int soldiers {
        get { return Mathf.Max(1, Mathf.FloorToInt(population * draftRate)); }
    }
    internal int workers {
        get { return Mathf.Max(1, Mathf.FloorToInt(population * productionRate)); }
    }
    internal float draftRate {
        get {
            if (this._kingdomTypeData == null) {
                return 0f;
            }
            return this._kingdomTypeData.populationRates.draftRate + _draftRateFromKing;
        }
    }
    internal float researchRate {
        get {
            if (this._kingdomTypeData == null) {
                return 0f;
            }
            return this._kingdomTypeData.populationRates.researchRate + _researchRateFromKing;
        }
    }
    internal float productionRate {
        get {
            if (this._kingdomTypeData == null) {
                return 0f;
            }
            return this._kingdomTypeData.populationRates.productionRate + _productionRateFromKing;
        }
    }
	internal int effectiveAttack{
		get{ 
			float mySoldiers = (float)this.soldiers;
			float numerator = 2f * mySoldiers * (float)this._baseWeapons;
			return (int)(numerator / (mySoldiers + (float)this._baseWeapons));
		}
	}
	internal int effectiveDefense{
		get{ 
			float mySoldiers = (float)this.soldiers;
			float numerator = 2f * mySoldiers * (float)this._baseArmor;
			return (int)(numerator / (mySoldiers + (float)this._baseArmor));
		}
	}
    internal Dictionary<City, List<Citizen>> citizens {
        get { return _citizens; }
    }
    internal int stabilityDecreaseFromInvasionCounter {
        get { return _stabilityDecreaseFromInvasionCounter; }
    }
    #endregion

    // Kingdom constructor paramters
    //	race - the race of this kingdom
    //	cities - the cities that this kingdom will initially own
    //	sourceKingdom (optional) - the kingdom from which this new kingdom came from
    public Kingdom(RACE race, List<HexTile> cities, Kingdom sourceKingdom = null) {
		this.id = Utilities.SetID(this);
		this.race = race;
        this._prestige = 0;
		this.name = RandomNameGenerator.Instance.GenerateKingdomName(this.race);
        this.kingdomTag = name + "_" + id;
        this.kingdomTagIndex = PathfindingManager.Instance.AddNewTag(kingdomTag);
        this.king = null;
        this.nextInLine = null;
        this._kingdomSize = KINGDOM_SIZE.SMALL;
        this._mainThreat = null;
        this.successionLine = new List<Citizen>();
		this._cities = new List<City> ();
        this._regions = new List<Region>();
        this._citizens = new Dictionary<City, List<Citizen>>();
		this.camps = new List<Camp> ();
		this.kingdomHistory = new List<History>();
		this.kingdomColor = Utilities.GetColorForKingdom();
		this._availableResources = new Dictionary<RESOURCE, int> ();
		this.relationships = new Dictionary<Kingdom, KingdomRelationship>();
		this._isDead = false;
		this._isLockedDown = false;
		this._isMilitarize = false;
		this._isFortifying = false;
		this._hasUpheldHiddenHistoryBook = false;
        this._embargoList = new Dictionary<Kingdom, EMBARGO_REASON>();
        this._stability = 0;
		this._sourceKingdom = sourceKingdom;
		this.borderConflictLoyaltyExpiration = 0;
		this._discoveredKingdoms = new List<Kingdom>();
		this._techLevel = 0;
		this._techCounter = 0;
		this._hasBioWeapon = false;
		this.plague = null;
        this.age = 0;
        this.foundationYear = GameManager.Instance.year;
        this.foundationDay = GameManager.Instance.days;
        this.foundationMonth = GameManager.Instance.month;
        //this._dictCharacterValues = new Dictionary<CHARACTER_VALUE, int>();
        //this._importantCharacterValues = new Dictionary<CHARACTER_VALUE, int>();

        //Fog Of War
        this._fogOfWar = new FOG_OF_WAR_STATE[(int)GridMap.Instance.width, (int)GridMap.Instance.height];
		this._fogOfWarDict = new Dictionary<FOG_OF_WAR_STATE, HashSet<HexTile>>();
		this._fogOfWarDict.Add(FOG_OF_WAR_STATE.HIDDEN, new HashSet<HexTile>(GridMap.Instance.listHexes.Select(x => x.GetComponent<HexTile>())));
		this._fogOfWarDict.Add(FOG_OF_WAR_STATE.SEEN, new HashSet<HexTile>());
		this._fogOfWarDict.Add(FOG_OF_WAR_STATE.VISIBLE, new HashSet<HexTile>());
        this._regionFogOfWarDict = new Dictionary<Region, FOG_OF_WAR_STATE>();
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            _regionFogOfWarDict.Add(GridMap.Instance.allRegions[i], FOG_OF_WAR_STATE.HIDDEN);
        }

        this._activeEvents = new List<GameEvent> ();
		this._doneEvents = new List<GameEvent> ();
		this.orderedMaleRoyalties = new List<Citizen> ();
		this.orderedFemaleRoyalties = new List<Citizen> ();
		this.orderedBrotherRoyalties = new List<Citizen> ();
		this.orderedSisterRoyalties = new List<Citizen> ();
		this._adjacentKingdoms = new List<Kingdom> ();

		this._evenActionDay = 0;
		this._alliancePool = null;
		this._warfareInfo = new Dictionary<int, WarfareInfo>();
        this._stabilityDecreaseFromInvasionCounter = 0;
		this.highestThreatAdjacentKingdomAbove50 = null;

		this.checkedWarfareID = new List<int> ();

        AdjustPrestige(GridMap.Instance.numOfRegions);
        //		AdjustPrestige(500);


        AdjustPopulation(50);
        AdjustStability(50);
        AdjustBaseWeapons(25);
        AdjustBaseArmors(25);
        SetGrowthState(true);
        //this.GenerateKingdomCharacterValues();
        this.SetLockDown(false);
		this.SetTechProduction(true);
		this.SetTechProductionPercentage(1f);
		this.SetProductionGrowthPercentage(1f);
		this.UpdateTechCapacity ();
		this.SetSecession (false);
		this.SetWarmongerValue (25);
//		this.NewRandomCrimeDate (true);
		// Determine what type of Kingdom this will be upon initialization.
		this._kingdomTypeData = null;
        SetKingdomType(StoryTellingManager.Instance.GetRandomKingdomTypeForKingdom());
		//this.UpdateKingdomTypeData();

        this.basicResource = Utilities.GetBasicResourceForRace(race);

		Messenger.AddListener<Kingdom>("OnNewKingdomCreated", CreateNewRelationshipWithKingdom);
		//Messenger.AddListener("OnDayEnd", KingdomTickActions);
        Messenger.AddListener<Kingdom>("OnKingdomDied", OtherKingdomDiedActions);

		SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.Instance.days, (GameManager.Instance.year + 1), () => AttemptToAge());
        //SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.daysInMonth[GameManager.Instance.month], GameManager.Instance.year, () => DecreaseUnrestEveryMonth());
        SchedulingManager.Instance.AddEntry(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, () => IncreaseExpansionRatePerMonth());
        SchedulingManager.Instance.AddEntry(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, () => IncreaseBOPAttributesPerMonth());
        //SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.daysInMonth[GameManager.Instance.month], GameManager.Instance.year, () => MonthlyPrestigeActions());
        //SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, () => AdaptToKingValues());
        SchedulingManager.Instance.AddEntry(GameManager.Instance.month, 1, GameManager.Instance.year, () => IncreasePopulationEveryMonth());
        SchedulingManager.Instance.AddEntry (1, 1, GameManager.Instance.year + 1, () => WarmongerDecreasePerYear ());
        //		ScheduleEvents ();
        ScheduleOddDayActions();
        ScheduleActionDay();


        this.kingdomHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "This kingdom was born.", HISTORY_IDENTIFIER.NONE));
	}

    #region Initialization Functions
    internal void CreateInitialCities(List<HexTile> initialCityLocations) {
        if (initialCityLocations.Count > 0) {
            for (int i = 0; i < initialCityLocations.Count; i++) {
                HexTile initialCityLocation = initialCityLocations[i];
                City newCity = this.CreateNewCityOnTileForKingdom(initialCityLocation);
                initialCityLocation.region.SetOccupant(newCity);
            }
        }
    }
    internal void CreateInitialFamilies() {
        CreateInitialRoyalFamily();
        CreateNewChancellorFamily();
        CreateNewMarshalFamily();
        for (int i = 0; i < cities.Count; i++) {
            City currCity = cities[i];
            CreateNewGovernorFamily(currCity);
        }
    }
    internal void CreateInitialRoyalFamily() {
        successionLine.Clear();
        GENDER gender = GENDER.MALE;
        int randomGender = UnityEngine.Random.Range(0, 100);
        if (randomGender < 20) {
            gender = GENDER.FEMALE;
        }
        Citizen king = new Citizen(capitalCity, UnityEngine.Random.Range(20, 36), gender, 2);
        Citizen father = new Citizen(capitalCity, UnityEngine.Random.Range(60, 81), GENDER.MALE, 1);
        Citizen mother = new Citizen(capitalCity, UnityEngine.Random.Range(60, 81), GENDER.FEMALE, 1);

        MONTH monthFather = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthMother = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthKing = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        father.AssignBirthday(monthFather, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthFather] + 1), GameManager.Instance.year - father.age, false);
        mother.AssignBirthday(monthMother, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthMother] + 1), GameManager.Instance.year - mother.age, false);
        king.AssignBirthday(monthKing, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthKing] + 1), (GameManager.Instance.year - king.age));

        father.isDirectDescendant = true;
        mother.isDirectDescendant = true;
        father.isDead = true;
        mother.isDead = true;

        father.AddChild(king);
        mother.AddChild(king);
        king.AddParents(father, mother);

        MarriageManager.Instance.Marry(father, mother);

        AddCitizenToKingdom(king, capitalCity);
        king.isDirectDescendant = true;
        AssignNewKing(king);

        MONTH monthSibling = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthSibling2 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        int siblingsChance = UnityEngine.Random.Range(0, 100);
        if (siblingsChance < 25) {
            Citizen sibling = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, king.age));
            Citizen sibling2 = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, king.age));

            sibling.AssignBirthday(monthSibling, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling] + 1), (GameManager.Instance.year - sibling.age));
            sibling2.AssignBirthday(monthSibling2, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling2] + 1), (GameManager.Instance.year - sibling2.age));
            AddCitizenToKingdom(sibling, capitalCity);
            AddCitizenToKingdom(sibling2, capitalCity);
            sibling.UpdateKingOpinion();
            sibling2.UpdateKingOpinion();
        } else if (siblingsChance >= 25 && siblingsChance < 75) {
            Citizen sibling = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, king.age));
            sibling.AssignBirthday(monthSibling, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling] + 1), (GameManager.Instance.year - sibling.age));
            AddCitizenToKingdom(sibling, capitalCity);
            sibling.UpdateKingOpinion();
        }

        MONTH monthChild1 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthChild2 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthChild3 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        int spouseChance = UnityEngine.Random.Range(0, 100);
        if (spouseChance < 80) {
            Citizen spouse = MarriageManager.Instance.CreateSpouse(king);
            AddCitizenToKingdom(spouse, capitalCity);
            spouse.UpdateKingOpinion();
        }
    }
    internal void CreateNewChancellorFamily() {
        GENDER gender = GENDER.MALE;
        int randomGender = UnityEngine.Random.Range(0, 100);
        if (randomGender < 20) {
            gender = GENDER.FEMALE;
        }
        Citizen chancellor = new Citizen(capitalCity, UnityEngine.Random.Range(20, 36), gender, 2);
        Citizen father = new Citizen(capitalCity, UnityEngine.Random.Range(60, 81), GENDER.MALE, 1);
        Citizen mother = new Citizen(capitalCity, UnityEngine.Random.Range(60, 81), GENDER.FEMALE, 1);

        MONTH monthFather = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthMother = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthChancellor = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        father.AssignBirthday(monthFather, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthFather] + 1), GameManager.Instance.year - father.age, false);
        mother.AssignBirthday(monthMother, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthMother] + 1), GameManager.Instance.year - mother.age, false);
        chancellor.AssignBirthday(monthChancellor, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthChancellor] + 1), (GameManager.Instance.year - chancellor.age));

        father.isDead = true;
        mother.isDead = true;

        father.AddChild(chancellor);
        mother.AddChild(chancellor);
        chancellor.AddParents(father, mother);
        AddCitizenToKingdom(chancellor, capitalCity);
        chancellor.UpdateKingOpinion();

        MarriageManager.Instance.Marry(father, mother);

        MONTH monthSibling = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthSibling2 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        int siblingsChance = UnityEngine.Random.Range(0, 100);
        if (siblingsChance < 25) {
            Citizen sibling = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, chancellor.age));
            Citizen sibling2 = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, chancellor.age));

            sibling.AssignBirthday(monthSibling, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling] + 1), (GameManager.Instance.year - sibling.age));
            sibling2.AssignBirthday(monthSibling2, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling2] + 1), (GameManager.Instance.year - sibling2.age));
            AddCitizenToKingdom(sibling, capitalCity);
            AddCitizenToKingdom(sibling2, capitalCity);
            sibling.UpdateKingOpinion();
            sibling2.UpdateKingOpinion();
        } else if (siblingsChance >= 25 && siblingsChance < 75) {
            Citizen sibling = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, chancellor.age));
            sibling.AssignBirthday(monthSibling, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling] + 1), (GameManager.Instance.year - sibling.age));
            AddCitizenToKingdom(sibling, capitalCity);
            sibling.UpdateKingOpinion();
        }

        MONTH monthChild1 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthChild2 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthChild3 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        int spouseChance = UnityEngine.Random.Range(0, 100);
        if (spouseChance < 80) {
            Citizen spouse = MarriageManager.Instance.CreateSpouse(chancellor);
            AddCitizenToKingdom(spouse, capitalCity);
            spouse.UpdateKingOpinion();
        }
        chancellor.AssignRole(ROLE.GRAND_CHANCELLOR);
    }
    internal void CreateNewMarshalFamily() {
        GENDER gender = GENDER.MALE;
        int randomGender = UnityEngine.Random.Range(0, 100);
        if (randomGender < 20) {
            gender = GENDER.FEMALE;
        }
        Citizen marshal = new Citizen(capitalCity, UnityEngine.Random.Range(20, 36), gender, 2);
        Citizen father = new Citizen(capitalCity, UnityEngine.Random.Range(60, 81), GENDER.MALE, 1);
        Citizen mother = new Citizen(capitalCity, UnityEngine.Random.Range(60, 81), GENDER.FEMALE, 1);

        MONTH monthFather = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthMother = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthMarshal = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        father.AssignBirthday(monthFather, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthFather] + 1), GameManager.Instance.year - father.age, false);
        mother.AssignBirthday(monthMother, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthMother] + 1), GameManager.Instance.year - mother.age, false);
        marshal.AssignBirthday(monthMarshal, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthMarshal] + 1), (GameManager.Instance.year - marshal.age));

        father.isDead = true;
        mother.isDead = true;

        father.AddChild(marshal);
        mother.AddChild(marshal);
        marshal.AddParents(father, mother);
        AddCitizenToKingdom(marshal, capitalCity);
        marshal.UpdateKingOpinion();

        MarriageManager.Instance.Marry(father, mother);

        MONTH monthSibling = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthSibling2 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        int siblingsChance = UnityEngine.Random.Range(0, 100);
        if (siblingsChance < 25) {
            Citizen sibling = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, marshal.age));
            Citizen sibling2 = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, marshal.age));

            sibling.AssignBirthday(monthSibling, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling] + 1), (GameManager.Instance.year - sibling.age));
            sibling2.AssignBirthday(monthSibling2, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling2] + 1), (GameManager.Instance.year - sibling2.age));
            AddCitizenToKingdom(sibling, capitalCity);
            AddCitizenToKingdom(sibling2, capitalCity);
            sibling.UpdateKingOpinion();
            sibling2.UpdateKingOpinion();
        } else if (siblingsChance >= 25 && siblingsChance < 75) {
            Citizen sibling = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, marshal.age));
            sibling.AssignBirthday(monthSibling, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling] + 1), (GameManager.Instance.year - sibling.age));
            AddCitizenToKingdom(sibling, capitalCity);
            sibling.UpdateKingOpinion();
        }

        MONTH monthChild1 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthChild2 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthChild3 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        int spouseChance = UnityEngine.Random.Range(0, 100);
        if (spouseChance < 80) {
            Citizen spouse = MarriageManager.Instance.CreateSpouse(marshal);
            AddCitizenToKingdom(spouse, capitalCity);
            spouse.UpdateKingOpinion();
        }
        marshal.AssignRole(ROLE.GRAND_MARSHAL);
    }
    internal void CreateNewGovernorFamily(City cityOfFamily) {
        GENDER gender = GENDER.MALE;
        int randomGender = UnityEngine.Random.Range(0, 100);
        if (randomGender < 20) {
            gender = GENDER.FEMALE;
        }
        Citizen governor = new Citizen(cityOfFamily, UnityEngine.Random.Range(20, 36), gender, 2);
        Citizen father = new Citizen(cityOfFamily, UnityEngine.Random.Range(60, 81), GENDER.MALE, 1);
        Citizen mother = new Citizen(cityOfFamily, UnityEngine.Random.Range(60, 81), GENDER.FEMALE, 1);

        governor.AssignRole(ROLE.GOVERNOR);

        father.AddChild(governor);
        mother.AddChild(governor);
        governor.AddParents(father, mother);
        AddCitizenToKingdom(governor, cityOfFamily);
        governor.UpdateKingOpinion();

        MONTH monthFather = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthMother = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthGovernor = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        father.AssignBirthday(monthFather, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthFather] + 1), GameManager.Instance.year - father.age, false);
        mother.AssignBirthday(monthMother, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthMother] + 1), GameManager.Instance.year - mother.age, false);
        governor.AssignBirthday(monthGovernor, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthGovernor] + 1), (GameManager.Instance.year - governor.age));

        father.isDead = true;
        mother.isDead = true;
        MarriageManager.Instance.Marry(father, mother);

        cityOfFamily.governor = governor;

        MONTH monthSibling = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthSibling2 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        int siblingsChance = UnityEngine.Random.Range(0, 100);
        if (siblingsChance < 25) {
            Citizen sibling = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, governor.age));
            Citizen sibling2 = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, governor.age));
            sibling.AssignBirthday(monthSibling, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling] + 1), (GameManager.Instance.year - sibling.age));
            sibling2.AssignBirthday(monthSibling2, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling2] + 1), (GameManager.Instance.year - sibling2.age));
            AddCitizenToKingdom(sibling, cityOfFamily);
            AddCitizenToKingdom(sibling2, cityOfFamily);
            sibling.UpdateKingOpinion();
            sibling2.UpdateKingOpinion();
        } else if (siblingsChance >= 25 && siblingsChance < 75) {
            Citizen sibling = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, governor.age));
            sibling.AssignBirthday(monthSibling, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling] + 1), (GameManager.Instance.year - sibling.age));
            AddCitizenToKingdom(sibling, cityOfFamily);
            sibling.UpdateKingOpinion();
        }

        MONTH monthChild1 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthChild2 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthChild3 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        int spouseChance = UnityEngine.Random.Range(0, 100);
        if (spouseChance < 80) {
            Citizen spouse = MarriageManager.Instance.CreateSpouse(governor);
            AddCitizenToKingdom(spouse, cityOfFamily);
            spouse.UpdateKingOpinion();
        }
    }
    internal void SetKingdomType(KINGDOM_TYPE kingdomType) {
        KINGDOM_TYPE prevKingdomType = this.kingdomType;
        switch (kingdomType) {
            case KINGDOM_TYPE.DEFENSIVE_KINGDOM:
                this._kingdomTypeData = KingdomManager.Instance.kingdomTypeNoble;
                break;
            case KINGDOM_TYPE.OFFENSIVE_KINGDOM:
                this._kingdomTypeData = KingdomManager.Instance.kingdomTypeEvil;
                break;
            case KINGDOM_TYPE.SCIENTIFIC_KINGDOM:
                this._kingdomTypeData = KingdomManager.Instance.kingdomTypeMerchant;
                break;
            case KINGDOM_TYPE.BALANCED_KINGDOM:
                this._kingdomTypeData = KingdomManager.Instance.kingdomTypeChaotic;
                break;
        }

        if (this.kingdomTypeData.dailyCumulativeEventRate != null) {
            this._dailyCumulativeEventRate = this._kingdomTypeData.dailyCumulativeEventRate;
        }

        // If the Kingdom Type Data changed
        if (prevKingdomType != this.kingdomType) {

            //Update Relationship Opinion
            UpdateAllRelationshipsLikenessFromOthers();
        }
    }
    #endregion

    #region Kingdom Death
    // Function to call if you want to determine whether the Kingdom is still alive or dead
    // At the moment, a Kingdom is considered dead if it doesnt have any cities.
    public bool isAlive() {
        if (this.cities.Count > 0) {
            return true;
        }
        return false;
    }
    /*
     * <summary>
	 * Every time a city of this kingdom dies, check if
	 * this kingdom has no more cities, if so, the kingdom is
	 * considered dead. Remove all ties from other kingdoms.
     * </summary>
	 * */
    internal void CheckIfKingdomIsDead() {
        if (this.cities.Count <= 0) {
            //Kingdom is dead
            this.DestroyKingdom();
        }
    }
    /*
     * <summary>
	 * Kill this kingdom. This removes all ties with other kingdoms.
	 * Only call this when a kingdom has no more cities.
     * </summary>
	 * */
    internal void DestroyKingdom() {
        _isDead = true;
        CancelEventKingdomIsInvolvedIn(EVENT_TYPES.ALL);
		if(this.alliancePool != null){
			LeaveAlliance (true);
		}
        //PathfindingManager.Instance.RemoveTag(kingdomTag);
        KingdomManager.Instance.UnregisterKingdomFromActionDays(this);
        ResolveWars();
        Messenger.RemoveListener<Kingdom>("OnNewKingdomCreated", CreateNewRelationshipWithKingdom);
        //Messenger.RemoveListener("OnDayEnd", KingdomTickActions);
        Messenger.RemoveListener<Kingdom>("OnKingdomDied", OtherKingdomDiedActions);

        Messenger.Broadcast<Kingdom>("OnKingdomDied", this);

        this.DeleteRelationships();
        KingdomManager.Instance.RemoveKingdom(this);
        Log newLog = new Log(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", "obliterated");
        string yearsLasted = string.Empty;
        if (age == 1) {
            yearsLasted = "a year";
        } else if (age <= 0) {
            yearsLasted = "less than a year";
        } else {
            yearsLasted = age.ToString() + " years";
        }
        newLog.AddToFillers(this, this.name, LOG_IDENTIFIER.KINGDOM_1);
        newLog.AddToFillers(null, yearsLasted, LOG_IDENTIFIER.OTHER);
        UIManager.Instance.ShowNotification(newLog);

        UIManager.Instance.CheckIfShowingKingdomIsAlive(this);

        Debug.Log(this.id + " - Kingdom: " + this.name + " has died!");
        Debug.Log("Stack Trace: " + System.Environment.StackTrace);
    }
    private void CancelEventKingdomIsInvolvedIn(EVENT_TYPES eventType) {
		List<GameEvent> eventsToCancel = new List<GameEvent>(this.activeEvents);
		if (eventType == EVENT_TYPES.ALL) {
			for (int i = 0; i < eventsToCancel.Count; i++) {
				eventsToCancel [i].CancelEvent ();
			}
		}else{
			for (int i = 0; i < eventsToCancel.Count; i++) {
				if (eventsToCancel[i].eventType == eventType) {
					eventsToCancel[i].CancelEvent ();
				}
			}
		}
    }
    private void ResolveWars() {
//        List<War> warsToResolve = relationships.Values.Where(x => x.war != null).Select(x => x.war).ToList();
//        for (int i = 0; i < warsToResolve.Count; i++) {
//            warsToResolve[i].DoneEvent();
//        }

		foreach (KingdomRelationship rel in relationships.Values) {
			if (rel.isAtWar) {
				rel.ChangeWarStatus (false, null);
			}
		}
		List<WarfareInfo> warfareInfos = this._warfareInfo.Values.ToList ();
		for (int i = 0; i < warfareInfos.Count; i++) {
			warfareInfos [i].warfare.UnjoinWar (this);
		}
    }
    protected void OtherKingdomDiedActions(Kingdom kingdomThatDied) {
        if (kingdomThatDied.id != this.id) {
            RemoveRelationshipWithKingdom(kingdomThatDied);
            RemoveKingdomFromDiscoveredKingdoms(kingdomThatDied);
            //RemoveKingdomFromEmbargoList(kingdomThatDied);
            RemoveAdjacentKingdom(kingdomThatDied);
            if (_mainThreat != null && _mainThreat == kingdomThatDied) {
                _mainThreat = null;
            }
        }
    }
    #endregion

    #region Relationship Functions
    internal void CreateInitialRelationships() {
        for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
            if (KingdomManager.Instance.allKingdoms[i].id != this.id) {
                Kingdom currOtherKingdom = KingdomManager.Instance.allKingdoms[i];
                //CreateNewRelationshipWithKingdom(currOtherKingdom);
                this.relationships.Add(currOtherKingdom, new KingdomRelationship(this, currOtherKingdom));
            }
        }
    }
    /*
     * <summary>
	 * Used to create a new KingdomRelationship with the
	 * newly created kingdom. Function is listening to onCreateNewKingdom Event.
     * </summary>
	 * */
    protected void CreateNewRelationshipWithKingdom(Kingdom createdKingdom) {
        if (createdKingdom.id == this.id) {
            return;
        }

        if (relationships.ContainsKey(createdKingdom)) {
            return;
        }

        //Debug.Log(this.name + " created new relationship with " + createdKingdom.name);

        KingdomRelationship newRel = new KingdomRelationship(this, createdKingdom);
        relationships.Add(createdKingdom, newRel);
        newRel.UpdateLikeness(null);
    }
    /* 
     * <summary>
     * Clear all relationships from self.
     * </summary>
     * */
    protected void DeleteRelationships() {
        this.relationships.Clear();
    }
    /*
     * <summary>
     * Remove a kingdom from this kingdom's list of relationships
     * </summary>
     * */
    protected void RemoveRelationshipWithKingdom(Kingdom kingdomThatDied) {
        if (!relationships.Remove(kingdomThatDied)) {
            string errorMsg = this.name + " cannot remove relationship with " + kingdomThatDied.name + " because it only has relationships with ";
            for (int i = 0; i < relationships.Keys.Count; i++) {
                errorMsg += "\n" + relationships.Keys.ElementAt(i).id + " - " + relationships.Keys.ElementAt(i).name;
            }
            throw new Exception(errorMsg);
        }
    }
    /*
     * <summary>
     * Actions to perform when a relationship that this kingdom owns, deteriorates
     * </summary>
     * */
    internal void OnRelationshipDeteriorated(KingdomRelationship relationship, GameEvent gameEventTrigger, bool isDiscovery, ASSASSINATION_TRIGGER_REASONS assassinationReasons) {
//        if (assassinationReasons != ASSASSINATION_TRIGGER_REASONS.NONE) {
//            TriggerAssassination(relationship, gameEventTrigger, assassinationReasons);
//        }
    }
    /*
    * <summary>
    * Actions to perform when a relationship that this kingdom owns, improves
    * </summary>
    * */
    internal void OnRelationshipImproved(KingdomRelationship relationship) {
        //Improvement of Relationship
//        int chance = UnityEngine.Random.Range(0, 100);
//        int value = 0;
//        if (relationship.relationshipStatus == RELATIONSHIP_STATUS.SPITE) {
//            value = 5;
//        } else if (relationship.relationshipStatus == RELATIONSHIP_STATUS.HATE) {
//            value = 15;
//        } else {
//            value = 25;
//        }
//        if (chance < value) {
//            CancelInvasionPlan(relationship);
//        }
    }
    /*
     * <summary>
     * Get all kingdoms that this kingdom has a specific relationshipStatus
     * </summary>
     * <param name="relationshipStatuses">Relationship Statuses to be checked</param>
     * <param name="discoveredOnly">Should only return discovered kingdoms?</param>
     * */
	internal List<Kingdom> GetKingdomsByRelationship(RELATIONSHIP_STATUS[] relationshipStatuses, Kingdom exception = null, bool discoveredOnly = true) {
        List<Kingdom> kingdomsWithRelationshipStatus = new List<Kingdom>();
		if(discoveredOnly){
			foreach (Kingdom currKingdom in relationships.Keys) {
				if(exception != null && exception.id == currKingdom.id){
					continue;
				}
				//        for (int i = 0; i < relationships.Count; i++) {
				//            Kingdom currKingdom = relationships.Keys.ElementAt(i);
				if (!discoveredKingdoms.Contains(currKingdom)) {
					continue;
				}

				RELATIONSHIP_STATUS currStatus = relationships[currKingdom].relationshipStatus;
				if (relationshipStatuses.Contains(currStatus)) {
					kingdomsWithRelationshipStatus.Add(currKingdom);
				}
			}
		}else{
			foreach (Kingdom currKingdom in relationships.Keys) {
				//        for (int i = 0; i < relationships.Count; i++) {
				//            Kingdom currKingdom = relationships.Keys.ElementAt(i);
				if(exception != null && exception.id == currKingdom.id){
					continue;
				}
				RELATIONSHIP_STATUS currStatus = relationships[currKingdom].relationshipStatus;
				if (relationshipStatuses.Contains(currStatus)) {
					kingdomsWithRelationshipStatus.Add(currKingdom);
				}
			}
		}
        return kingdomsWithRelationshipStatus;
    }
    internal KingdomRelationship GetRelationshipWithKingdom(Kingdom kingdom, bool ignoreException = false) {
        if (relationships.ContainsKey(kingdom)) {
            return relationships[kingdom];
        } else {
            if (ignoreException) {
                return null;
            } else {
                throw new Exception(this.name + " does not have relationship with " + kingdom.name);
            }
        }
        
    }
//    internal void UpdateMutualRelationships() {
//		foreach (KingdomRelationship currRel in relationships.Values) {
////        for (int i = 0; i < relationships.Count; i++) {
////            KingdomRelationship currRel = relationships.Values.ElementAt(i);
//            Kingdom targetKingdom = currRel.targetKingdom;
//            KingdomRelationship targetKingdomRel = targetKingdom.GetRelationshipWithKingdom(this);

//            if (targetKingdomRel == null || currRel == null) {
//                return;
//            }

//            currRel.ResetMutualRelationshipModifier();
//            targetKingdomRel.ResetMutualRelationshipModifier();

//			List<Kingdom> sourceKingRelationships = GetKingdomsByRelationship (new
//           [] { RELATIONSHIP_STATUS.HATE, RELATIONSHIP_STATUS.SPITE,
//				RELATIONSHIP_STATUS.AFFECTIONATE, RELATIONSHIP_STATUS.LOVE
//			}, targetKingdom);

//			List<Kingdom> targetKingRelationships = targetKingdom.GetKingdomsByRelationship (new
//                [] { RELATIONSHIP_STATUS.HATE, RELATIONSHIP_STATUS.SPITE,
//				RELATIONSHIP_STATUS.AFFECTIONATE, RELATIONSHIP_STATUS.LOVE
//			}, this);

////            List<Kingdom> kingdomsInCommon = sourceKingRelationships.Intersect(targetKingRelationships).ToList();
//			foreach (var currKingdom in sourceKingRelationships.Intersect(targetKingRelationships)) {
////            for (int j = 0; j < kingdomsInCommon.Count; j++) {
////                Kingdom currKingdom = kingdomsInCommon[j];
//                KingdomRelationship relSourceKingdom = this.GetRelationshipWithKingdom(currKingdom);
//                KingdomRelationship relTargetKingdom = targetKingdom.GetRelationshipWithKingdom(currKingdom);

//                if (relSourceKingdom.relationshipStatus == RELATIONSHIP_STATUS.HATE) {
//                    if (relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.HATE ||
//                        relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.SPITE) {
//                        currRel.AddMutualRelationshipModifier(5);
//                        targetKingdomRel.AddMutualRelationshipModifier(5);
//                    }
//                } else if (relSourceKingdom.relationshipStatus == RELATIONSHIP_STATUS.SPITE) {
//                    if (relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.HATE) {
//                        currRel.AddMutualRelationshipModifier(5);
//                    } else if (relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.SPITE) {
//                        targetKingdomRel.AddMutualRelationshipModifier(10);
//                    }
//                } else if (relSourceKingdom.relationshipStatus == RELATIONSHIP_STATUS.AFFECTIONATE) {
//                    if (relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.AFFECTIONATE ||
//                        relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.LOVE) {
//                        currRel.AddMutualRelationshipModifier(5);
//                        targetKingdomRel.AddMutualRelationshipModifier(5);
//                    }
//                } else if (relSourceKingdom.relationshipStatus == RELATIONSHIP_STATUS.LOVE) {
//                    if (relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.AFFECTIONATE) {
//                        currRel.AddMutualRelationshipModifier(5);
//                        targetKingdomRel.AddMutualRelationshipModifier(5);
//                    } else if (relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.LOVE) {
//                        currRel.AddMutualRelationshipModifier(10);
//                        targetKingdomRel.AddMutualRelationshipModifier(10);
//                    }
//                }
//            }
//        }
//    }
    internal void ResetRelationshipModifiers() {
        for (int i = 0; i < relationships.Count; i++) {
            KingdomRelationship currRel = relationships.ElementAt(i).Value;
            //currRel.ResetMutualRelationshipModifier();
            currRel.ResetEventModifiers();
        }
    }
    internal void UpdateAllRelationshipsLikeness() {
        if (this.king != null) {
			foreach (KingdomRelationship relationship in relationships.Values) {
                //if (relationship.isDiscovered) {
                    relationship.UpdateLikeness(null);
                //}
			}
        }
    }
	internal void UpdateAllRelationshipsLikenessFromOthers() {
		if (this.king != null) {
			foreach (KingdomRelationship relationship in relationships.Values) {
				KingdomRelationship relationshipFromOther = relationship.targetKingdom.GetRelationshipWithKingdom(this);
				relationshipFromOther.UpdateLikeness (null);
			}
		}
	}
    #endregion

	/*
	 * This function is listening to the onWeekEnd Event. Put functions that you want to
	 * happen every tick here.
	 * */
	//protected void KingdomTickActions(){
 //       //if (_isGrowthEnabled) {
 //       //    this.AttemptToExpand();
 //       //}
	//	this.IncreaseTechCounterPerTick();
 //       //this.TriggerEvents();
 //   }
//    private void AdaptToKingValues() {
//		if(!this.isDead){
//			for (int i = 0; i < _dictCharacterValues.Count; i++) {
//				CHARACTER_VALUE currValue = _dictCharacterValues.ElementAt(i).Key;
//				if (king.importantCharacterValues.ContainsKey(currValue)) {
//					UpdateSpecificCharacterValue(currValue, 1);
//				} else {
//					UpdateSpecificCharacterValue(currValue, -1);
//				}
//			}
//			UpdateKingdomCharacterValues();
//			SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year + 1, () => AdaptToKingValues());
//		}
////        if(GameManager.Instance.days == 1 && GameManager.Instance.month == 1) {
////            for (int i = 0; i < _dictCharacterValues.Count; i++) {
////                CHARACTER_VALUE currValue = _dictCharacterValues.ElementAt(i).Key;
////                if (king.importantCharacterValues.ContainsKey(currValue)) {
////                    UpdateSpecificCharacterValue(currValue, 1);
////                } else {
////                    UpdateSpecificCharacterValue(currValue, -1);
////                }
////            }
////            UpdateKingdomCharacterValues();
////        }
//    }
    private void AttemptToAge() {
		if(!this.isDead){
			age += 1;
			SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.Instance.days, (GameManager.Instance.year + 1), () => AttemptToAge());
		}

//        if(GameManager.Instance.year > foundationYear && GameManager.Instance.month == foundationMonth && GameManager.Instance.days == foundationDay) {
//            age += 1;
//        }
    }
	private void ScheduleEvents(){
//		SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.daysInMonth[GameManager.Instance.month], GameManager.Instance.year, () => TriggerSlavesMerchant());
		//SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.daysInMonth[GameManager.Instance.month], GameManager.Instance.year, () => TriggerHypnotism());
		//SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.daysInMonth[GameManager.Instance.month], GameManager.Instance.year, () => TriggerKingsCouncil());

		int month = UnityEngine.Random.Range (1, 5);
		SchedulingManager.Instance.AddEntry (month, UnityEngine.Random.Range(1, GameManager.daysInMonth[month]), GameManager.Instance.year, () => TriggerCrime());
	}

    #region Expansion Functions
    /*
	 * Kingdom will attempt to expand. 
	 * Chance for expansion can be edited by changing the value of expansionChance.
	 * NOTE: expansionChance increases on it's own.
	 * */
    protected void AttemptToExpand() {
        if (HasActiveEvent(EVENT_TYPES.EXPANSION)) {
            //Kingdom has a currently active expansion event
            return;
        }

        //if (cities.Count >= cityCap) {
        //    //Kingdom has reached max city capacity
        //    return;
        //}
        if(_expansionRate < GridMap.Instance.numOfRegions) {
            return;
        }

        float upperBound = 300f + (150f * (float)this.cities.Count);
        float chance = UnityEngine.Random.Range(0, upperBound);
        if (this.cities.Count > 0) {
            EventCreator.Instance.CreateExpansionEvent(this);
        }
    }
    private void IncreaseExpansionRatePerMonth() {
        //Reschedule next month
        GameDate dueDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
        dueDate.AddMonths(1);
        SchedulingManager.Instance.AddEntry(dueDate.month, dueDate.day, dueDate.year, () => IncreaseExpansionRatePerMonth());

        if (CityGenerator.Instance.GetExpandableTileForKingdom(this) == null) {
            //set expansion rate to 0 and don't increase expansion rate until kingdom can expand
            ResetExpansionRate();
            return;
        }
        if (_expansionRate < GridMap.Instance.numOfRegions) {
            AdjustExpansionRate(GetMonthlyExpansionRateIncrease());
        }
    }
    internal int GetMonthlyExpansionRateIncrease() {
        int monthlyExpansionRate = king.GetExpansionRateContribution();
        for (int i = 0; i < cities.Count; i++) {
            monthlyExpansionRate += cities[i].governor.GetExpansionRateContribution();
        }
        return monthlyExpansionRate;
    }
    internal void ResetExpansionRate() {
        _expansionRate = 0;
        KingdomManager.Instance.UpdateKingdomList();
    }
    private void AdjustExpansionRate(int adjustment) {
        _expansionRate += adjustment;
        _expansionRate = Mathf.Clamp(_expansionRate, 0, GridMap.Instance.numOfRegions);
        KingdomManager.Instance.UpdateKingdomList();
    }
    #endregion

    #region Odd Day Actions
    private void ScheduleOddDayActions() {
        this._oddActionDay = KingdomManager.Instance.GetOddActionDay(this);
        //KingdomManager.Instance.IncrementOddActionDay();
        SchedulingManager.Instance.AddEntry(GameManager.Instance.month, _oddActionDay, GameManager.Instance.year, () => OddDayActions());
    }
    private void OddDayActions() {
        if (_isGrowthEnabled) {
            AttemptToExpand();
        }
        GameDate nextActionDay = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
        nextActionDay.AddMonths(1);
        SchedulingManager.Instance.AddEntry(nextActionDay.month, nextActionDay.day, nextActionDay.year, () => OddDayActions());
    }
    #endregion

    #region Prestige
    internal void AdjustPrestige(int adjustment) {
        _prestige += adjustment;
        //_prestige = Mathf.Min(_prestige, KingdomManager.Instance.maxPrestige);
        //KingdomManager.Instance.UpdateKingdomPrestigeList();
    }
    internal void SetPrestige(int adjustment) {
        _prestige = adjustment;
        //_prestige = Mathf.Min(_prestige, KingdomManager.Instance.maxPrestige);
        //KingdomManager.Instance.UpdateKingdomPrestigeList();
    }
  //  internal void MonthlyPrestigeActions() {
  //      //Add Prestige
		//int prestigeToBeAdded = GetMonthlyPrestigeGain();
		//if(this.cityCap > this.cities.Count){
		//	float reduction = GetMonthlyPrestigeReduction (this.cityCap - this.cities.Count);
		//	prestigeToBeAdded -= (int)(prestigeToBeAdded * reduction);
		//}
		//AdjustPrestige(prestigeToBeAdded);
  //      //Reschedule event
  //      //GameDate gameDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
  //      //gameDate.AddMonths(1);
  //      //gameDate.day = GameManager.daysInMonth[gameDate.month];
  //      //SchedulingManager.Instance.AddEntry(gameDate.month, gameDate.day, gameDate.year, () => MonthlyPrestigeActions());
  //  }
	//private float GetMonthlyPrestigeReduction(int cityCapExcess){
	//	if(cityCapExcess == 1){
	//		return 0.05f;
	//	}else if(cityCapExcess == 2){
	//		return 0.15f;
	//	}else if(cityCapExcess == 3){
	//		return 0.3f;
	//	}else if(cityCapExcess == 4){
	//		return 0.5f;
	//	}else if(cityCapExcess == 5){
	//		return 0.7f;
	//	}else if(cityCapExcess == 6){
	//		return 0.9f;
	//	}else if(cityCapExcess >= 7){
	//		return 1f;
	//	}
	//	return 0f;
	//}
    //internal int GetMonthlyPrestigeGain() {
    //    int monthlyPrestigeGain = 0;
    //    monthlyPrestigeGain += king.GetPrestigeContribution();
    //    for (int i = 0; i < cities.Count; i++) {
    //        monthlyPrestigeGain += cities[i].governor.GetPrestigeContribution();
    //    }
    //    return monthlyPrestigeGain;
    //}
    #endregion

    #region Trading
    internal void AddKingdomToEmbargoList(Kingdom kingdomToAdd, EMBARGO_REASON embargoReason = EMBARGO_REASON.NONE) {
        if (!this._embargoList.ContainsKey(kingdomToAdd)) {
            this._embargoList.Add(kingdomToAdd, embargoReason);
            //Remove all existing trade routes between kingdomToAdd and this Kingdom
            //this.RemoveAllTradeRoutesWithOtherKingdom(kingdomToAdd);
            //kingdomToAdd.RemoveAllTradeRoutesWithOtherKingdom(this);
            //kingdomToAdd.AdjustStability(STABILITY_DECREASE_EMBARGO);
        }
        
    }
    internal void RemoveKingdomFromEmbargoList(Kingdom kingdomToRemove) {
        this._embargoList.Remove(kingdomToRemove);
    }
    #endregion

    #region City Management
    /* 
     * <summary>
	 * Create a new city obj on the specified hextile.
	 * Then add it to this kingdoms cities.
     * </summary>
	 * */
    internal City CreateNewCityOnTileForKingdom(HexTile tile) {
        City createdCity = CityGenerator.Instance.CreateNewCity(tile, this);
        this.AddCityToKingdom(createdCity);
        return createdCity;
    }
    /* 
     * <summary>
     * Add a city to this kingdom.
     * Recompute kingdom type data, available resources and
     * daily growth of all cities. Assign city as capital city
     * if city is first city in kingdom.
     * </summary>
     * */
    internal void AddCityToKingdom(City city) {
        this._cities.Add(city);
        _regions.Add(city.region);
        UpdateKingdomSize();
        UpdatePopulationCapacity();
        if ((capitalCity == null || capitalCity.isDead) && this._cities.Count == 1 && this._cities[0] != null) {
            SetCapitalCity(this._cities[0]);
        }
        if (!_citizens.ContainsKey(city)) {
            _citizens.Add(city, new List<Citizen>());
        }
        KingdomManager.Instance.UpdateKingdomList();
    }
    /* 
     * <summary>
     * Remove city from this kingdom.
     * Check if kingdom is dead beacuse of city removal.
     * If not, recompute this kingdom's capital city, kingdom type data, 
     * available resources, and daily growth of remaining cities.
     * </summary>
     * */
    internal List<Citizen> RemoveCityFromKingdom(City city) {
		if (!this._cities.Remove(city)) {
			Debug.LogError ("CITY REMOVAL ERROR Kingdom Name: " + this.name);
			Debug.LogError ("CITY REMOVAL ERROR City Name: " + city.name);
			Debug.LogError ("CITY REMOVAL ERROR City Kingdom Name: " + city.kingdom.name);
			UIManager.Instance.Pause ();
		}
        //this._cities.Remove(city);
        _regions.Remove(city.region);
        this.CheckIfKingdomIsDead();
        if (!this.isDead) {
            UpdateKingdomSize();
            UpdatePopulationCapacity();
            RevalidateKingdomAdjacency(city);
			SetCapitalCity(this._cities[0]);
            TransferCitizensFromCityToCapital(city);
            KingdomManager.Instance.UpdateKingdomList();
        }
        List<Citizen> remainingCitizens = new List<Citizen>();
        if (_citizens.ContainsKey(city)) {
            remainingCitizens.AddRange(_citizens[city]);
        }
        _citizens.Remove(city);
        UIManager.Instance.UpdateKingdomCitiesMenu();
        return remainingCitizens;
    }
    internal void SetCapitalCity(City city) {
        this.capitalCity = city;
        HexTile habitableTile;
        if (this.basicResource == BASE_RESOURCE_TYPE.STONE) {
            for (int i = 0; i < CityGenerator.Instance.stoneHabitableTiles.Count; i++) {
                habitableTile = CityGenerator.Instance.stoneHabitableTiles[i];
                this.capitalCity.AddHabitableTileDistance(habitableTile, PathGenerator.Instance.GetDistanceBetweenTwoTiles(city.hexTile, habitableTile));
            }
        } else if (this.basicResource == BASE_RESOURCE_TYPE.WOOD) {
            for (int i = 0; i < CityGenerator.Instance.woodHabitableTiles.Count; i++) {
                habitableTile = CityGenerator.Instance.woodHabitableTiles[i];
                this.capitalCity.AddHabitableTileDistance(habitableTile, PathGenerator.Instance.GetDistanceBetweenTwoTiles(city.hexTile, habitableTile));
            }
        }
    }
    private void UpdateKingdomSize() {
        if(cities.Count < KingdomManager.Instance.smallToMediumReq) {
            _kingdomSize = KINGDOM_SIZE.SMALL;
        } else if (cities.Count >= KingdomManager.Instance.smallToMediumReq && cities.Count < KingdomManager.Instance.mediumToLargeReq) {
            _kingdomSize = KINGDOM_SIZE.MEDIUM;
        } else if (cities.Count >= KingdomManager.Instance.mediumToLargeReq) {
            _kingdomSize = KINGDOM_SIZE.LARGE;
        }
    }
    #endregion

    #region Citizen Management
    internal void AddCitizenToKingdom(Citizen citizenToAdd, City cityCitizenBelongsTo) {
        citizenToAdd.city = cityCitizenBelongsTo;
        if (_citizens.ContainsKey(cityCitizenBelongsTo)) {
            if (!_citizens[cityCitizenBelongsTo].Contains(citizenToAdd)) {
                _citizens[cityCitizenBelongsTo].Add(citizenToAdd);
            }
        } else {
            _citizens.Add(cityCitizenBelongsTo, new List<Citizen>() { citizenToAdd });
        }
    }
    internal void RemoveCitizenFromKingdom(Citizen citizenToRemove, City cityCitizenBelongedTo) {
        if (_citizens.ContainsKey(cityCitizenBelongedTo)) {
            _citizens[cityCitizenBelongedTo].Remove(citizenToRemove);
        }
        //else {
        //    throw new Exception(citizenToRemove.role + " " + citizenToRemove.name + " cannot be removed because " + cityCitizenBelongedTo.name + " is not a city of " + this.name);
        //}
    }
    internal bool IsElligibleForRebellion() {
        if(stability <= -50 && kingdomSize != KINGDOM_SIZE.SMALL) {
            for (int i = 0; i < cities.Count; i++) {
                City currCity = cities[i];
                if (currCity.citizens.Where(x => x.role != ROLE.KING && x.role != ROLE.UNTRAINED && x.loyaltyToKing <= -50).Any()) {
                    return true;
                }
            }
        }
        return false;
    }
    internal List<Citizen> GetCitizensForRebellion() {
        List<Citizen> citizensForRebellion = new List<Citizen>();
        for (int i = 0; i < cities.Count; i++) {
            City currCity = cities[i];
            citizensForRebellion.AddRange(currCity.citizens.Where(x => x.role != ROLE.KING && x.role != ROLE.UNTRAINED && x.loyaltyToKing <= -50));
        }
        return citizensForRebellion;
    }
    internal Citizen GetCitizenWithRoleInKingdom(ROLE role) {
        for (int i = 0; i < cities.Count; i++) {
            City currCity = cities[i];
            for (int j = 0; j < currCity.citizens.Count; j++) {
                Citizen currCitizen = currCity.citizens[j];
                if(currCitizen.role == role) {
                    return currCitizen;
                }
            }
        }
        return null;
    }
    internal List<Citizen> GetCitizensWithRoleInKingdom(ROLE role) {
        List<Citizen> citizensWithRole = new List<Citizen>();
        for (int i = 0; i < cities.Count; i++) {
            City currCity = cities[i];
            for (int j = 0; j < currCity.citizens.Count; j++) {
                Citizen currCitizen = currCity.citizens[j];
                if (currCitizen.role == role) {
                    if (!citizensWithRole.Contains(currCitizen)) {
                        citizensWithRole.Add(currCitizen);
                    }
                }
            }
        }
        return citizensWithRole;
    }
    internal void TransferCitizensFromCityToCapital(City sourceCity, HashSet<Citizen> exceptions = null) {
        List<Citizen> importantCitizensInCity = new List<Citizen>(sourceCity.citizens.Where(x => x.role == ROLE.KING 
            || x.role == ROLE.GRAND_CHANCELLOR || x.role == ROLE.GRAND_MARSHAL));

        //Get Families of important citizens
        List<Citizen> citizensToTransfer = new List<Citizen>();
        for (int i = 0; i < importantCitizensInCity.Count; i++) {
            Citizen currImportantCitizen = importantCitizensInCity[i];
            citizensToTransfer.Add(currImportantCitizen);
            List<Citizen> familyOfCurrCitizen = currImportantCitizen.GetRelatives(-1);
            for (int j = 0; j < familyOfCurrCitizen.Count; j++) {
                Citizen currFamilyMember = familyOfCurrCitizen[j];
                if (!citizensToTransfer.Contains(currFamilyMember) && (exceptions == null || !exceptions.Contains(currFamilyMember)) && currFamilyMember.city.kingdom.id == currImportantCitizen.city.kingdom.id) {
                    citizensToTransfer.Add(currFamilyMember);
                }
            }
        }
        for (int i = 0; i < citizensToTransfer.Count; i++) {
            Citizen currCitizen = citizensToTransfer[i];
            RemoveCitizenFromKingdom(currCitizen, currCitizen.city);
            AddCitizenToKingdom(currCitizen, capitalCity);
        }
    }
    #endregion

    #region Succession
    internal void UpdateKingSuccession() {
		orderedMaleRoyalties.Clear ();
		orderedFemaleRoyalties.Clear ();
		orderedBrotherRoyalties.Clear ();
		orderedSisterRoyalties.Clear ();

		for (int i = 0; i < this.successionLine.Count; i++) {
			if (this.successionLine [i].generation > this.king.generation) {
				if (this.successionLine [i].gender == GENDER.MALE) {
					orderedMaleRoyalties.Add (this.successionLine [i]);
				} else {
					orderedFemaleRoyalties.Add (this.successionLine [i]);
				}
			}
		}

		for (int i = 0; i < this.successionLine.Count; i++) {
			if ((this.successionLine [i].father != null && this.king.father != null) && this.successionLine [i].father.id == this.king.father.id) {
				if (this.successionLine [i].gender == GENDER.MALE) {
					orderedBrotherRoyalties.Add (this.successionLine [i]);
				}else{
					orderedSisterRoyalties.Add (this.successionLine [i]);
				}
			}
		}

        this.successionLine.Clear();
        this.successionLine.AddRange (orderedMaleRoyalties.OrderBy (x => x.generation).ThenByDescending (x => x.age));
		this.successionLine.AddRange (orderedFemaleRoyalties.OrderBy (x => x.generation).ThenByDescending (x => x.age));

		this.successionLine.AddRange (orderedBrotherRoyalties.OrderByDescending (x => x.age));
		this.successionLine.AddRange (orderedSisterRoyalties.OrderByDescending (x => x.age));

        List<Citizen> invalidCitizens = new List<Citizen>();
        //Validate Succession line
        for (int i = 0; i < successionLine.Count; i++) {
            Citizen currSuccessor = successionLine[i];
            if (currSuccessor.city.kingdom.id != king.city.kingdom.id || currSuccessor.id == this.king.id || currSuccessor.role == ROLE.KING) {
                //successor is of a different kingdom!
                invalidCitizens.Add(currSuccessor);
            }
        }

        for (int i = 0; i < invalidCitizens.Count; i++) {
            successionLine.Remove(invalidCitizens[i]);
        }

        Citizen newNextInLine = successionLine.FirstOrDefault();
        if(newNextInLine != null && (newNextInLine.role == ROLE.KING || newNextInLine.role == ROLE.QUEEN)) {
            throw new Exception("New next in line " + newNextInLine.name + " is a " + newNextInLine.role.ToString() + " which is invalid!");
        }

        if (newNextInLine != null && nextInLine != null && newNextInLine.id != nextInLine.id && nextInLine.role == ROLE.CROWN_PRINCE) {
            //next in line is no longer the next in line
            nextInLine.AssignRole(ROLE.UNTRAINED);
        }
        nextInLine = newNextInLine;
        if(newNextInLine != null && newNextInLine.role != ROLE.CROWN_PRINCE) {
            newNextInLine.AssignRole(ROLE.CROWN_PRINCE);
        }
        
    }
    internal void ChangeSuccessionLineRescursively(Citizen royalty) {
        if (this.king.id != royalty.id) {
            if (!royalty.isDead) {
                this.successionLine.Add(royalty);
            }
        }

        for (int i = 0; i < royalty.children.Count; i++) {
            if (royalty.children[i] != null) {
                this.ChangeSuccessionLineRescursively(royalty.children[i]);
            }
        }
    }
    internal void RemoveFromSuccession(Citizen citizen) {
        if (citizen != null) {
            for (int i = 0; i < this.successionLine.Count; i++) {
                if (this.successionLine[i].id == citizen.id) {
                    this.successionLine.RemoveAt(i);
                    break;
                }
            }
        }
    }
    internal Citizen CreateNewKing() {
        GENDER gender = GENDER.MALE;
        int randomGender = UnityEngine.Random.Range(0, 100);
        if (randomGender < 20) {
            gender = GENDER.FEMALE;
        }
        Citizen king = new Citizen(capitalCity, UnityEngine.Random.Range(20, 36), gender, 2);

        MONTH monthKing = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        king.AssignBirthday(monthKing, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthKing] + 1), (GameManager.Instance.year - king.age));
        king.city.kingdom.AddCitizenToKingdom(king, capitalCity);
        king.CreateFamily();

        return king;
    }
    internal void AssignNewKing(Citizen newKing = null) {
        if(newKing == null) { 
            //A new king was not specified, check succession lines first, if there are no successors generate a new king
            if(successionLine.Count > 0) {
                newKing = successionLine.First();
            } else {
                newKing = CreateNewKing();
                List<Citizen> familyOfPreviousKing = new List<Citizen>(this.king.GetRelatives(-1));
                familyOfPreviousKing.Add(this.king);
                //Remove family of previous king from kingdom
                for (int i = 0; i < familyOfPreviousKing.Count; i++) {
                    Citizen currFamilyMember = familyOfPreviousKing[i];
                    RemoveCitizenFromKingdom(currFamilyMember, currFamilyMember.city);
                }
            }
        }

        if(newKing == null) {
            throw new Exception("No new king was generated for " + name);
        }

        if (!newKing.isDirectDescendant) {
            Utilities.ChangeDescendantsRecursively(newKing, true);
            if (this.king != null) {
                Utilities.ChangeDescendantsRecursively(this.king, false);
            }
        }

        this.king = newKing;
        newKing.AssignRole(ROLE.KING);
        ((King)newKing.assignedRole).SetOwnedKingdom(this);

        nextInLine = null;
        this.successionLine.Clear();
        ChangeSuccessionLineRescursively(newKing);
        this.successionLine.AddRange(newKing.GetSiblings());
        UpdateKingSuccession();

        this.UpdateProductionRatesFromKing();
        this.UpdateAllRelationshipsLikeness();
        this.UpdateAllCitizensOpinionOfKing();

    }
    internal void UpdateAllCitizensOpinionOfKing() {
        for (int i = 0; i < cities.Count; i++) {
            City currCity = cities[i];
            for (int j = 0; j < currCity.citizens.Count; j++) {
                Citizen currCitizen = currCity.citizens[j];
                if (currCitizen.id != king.id) {
                    currCitizen.UpdateKingOpinion();
                }
            }
        }
    }
    #endregion

    #region War
    internal void AdjustExhaustionToAllRelationship(int amount) {
        for (int i = 0; i < relationships.Count; i++) {
            relationships.ElementAt(i).Value.AdjustExhaustion(amount);
        }
    }
	internal void ConquerCity(City city, Warfare warfare){
		if (this.id != city.kingdom.id) {
			city.ConquerCity(this, warfare);
		}
	}
    #endregion

    #region Kingdom Tile Management
    internal void HighlightAllOwnedTilesInKingdom() {
        for (int i = 0; i < this.cities.Count; i++) {
            this.cities[i].HighlightAllOwnedTiles(69f / 255f);
        }
    }
    internal void UnHighlightAllOwnedTilesInKingdom() {
        for (int i = 0; i < this.cities.Count; i++) {
            if (UIManager.Instance.currentlyShowingCity != null && UIManager.Instance.currentlyShowingCity.id == this.cities[i].id) {
                continue;
            }
            this.cities[i].UnHighlightAllOwnedTiles();
        }
    }
    #endregion


    internal MILITARY_STRENGTH GetMilitaryStrengthAgainst(Kingdom kingdom){
		int sourceMilStrength = this.GetAllCityHp ();
		int targetMilStrength = kingdom.GetAllCityHp ();

		int fiftyPercent = (int)(targetMilStrength * 0.50f);
		int twentyPercent = (int)(targetMilStrength * 0.20f);
//		Debug.Log ("TARGET MILITARY STRENGTH: " + targetMilStrength);
//		Debug.Log ("SOURCE MILITARY STRENGTH: " + sourceMilStrength);
		if(sourceMilStrength == 0 && targetMilStrength == 0){
			Debug.Log (this.name + "'s military is COMPARABLE to " + kingdom.name);
			return MILITARY_STRENGTH.COMPARABLE;
		}else{
			if(sourceMilStrength > (targetMilStrength + fiftyPercent)){
				Debug.Log (this.name + "'s military is MUCH STRONGER than " + kingdom.name);
				return MILITARY_STRENGTH.MUCH_STRONGER;
			}else if(sourceMilStrength > (targetMilStrength + twentyPercent)){
				Debug.Log (this.name + "'s military is SLIGHTLY STRONGER than " + kingdom.name);
				return MILITARY_STRENGTH.SLIGHTLY_STRONGER;
			}else if(sourceMilStrength > (targetMilStrength - twentyPercent)){
				Debug.Log (this.name + "'s military is COMPARABLE to " + kingdom.name);
				return MILITARY_STRENGTH.COMPARABLE;
			}else if(sourceMilStrength > (targetMilStrength - fiftyPercent)){
				Debug.Log (this.name + "'s military is SLIGHTLY WEAKER than " + kingdom.name);
				return MILITARY_STRENGTH.SLIGHTLY_WEAKER;
			}else{
				Debug.Log (this.name + "'s military is MUCH WEAKER than " + kingdom.name);
				return MILITARY_STRENGTH.MUCH_WEAKER;
			}

		}
	}

	internal int GetAllCityHp(){
		int total = 0;
		for(int i = 0; i < this.cities.Count; i++){
			total += this.cities[i].hp;
		}
		return total;
	}
	internal int GetWarCount(){
		int total = 0;
//		for (int i = 0; i < relationships.Count; i++) {
//			if(relationships.ElementAt(i).Value.isAtWar){
//				total += 1;
//			}
//		}
		foreach (KingdomRelationship relationship in this.relationships.Values) {
			if(relationship.isAtWar){
				total += 1;
			}
		}
		return total;
	}
	internal City GetNearestCityFromKing(List<City> cities){
		City nearestCity = null;
		int nearestDistance = 0;
		for(int i = 0; i < cities.Count; i++){
			List<HexTile> path = PathGenerator.Instance.GetPath (cities [i].hexTile, this.king.city.hexTile, PATHFINDING_MODE.AVATAR);
			if(path != null){
				if(nearestCity == null){
					nearestCity = cities [i];
					nearestDistance = path.Count;
				}else{
					if(path.Count < nearestDistance){
						nearestCity = cities [i];
						nearestDistance = path.Count;
					}
				}
			}
		}
		return nearestCity;
	}

	#region Resource Management
	/*
	 * Add resource type to this kingdoms
	 * available resource (DO NOT ADD GOLD TO THIS!).
	 * */
//	internal void AddResourceToKingdom(RESOURCE resource){
//		RESOURCE_BENEFITS resourceBenefit = Utilities.resourceBenefits[resource].Keys.FirstOrDefault();
//
//        if (!this._availableResources.ContainsKey(resource)) {
//			this._availableResources.Add(resource, 0);
//            //this.RemoveObsoleteTradeRoutes(resource);
//            if(resourceBenefit == RESOURCE_BENEFITS.GROWTH_RATE) {
//                this.UpdateAllCitiesDailyGrowth();
//            } else if (resourceBenefit == RESOURCE_BENEFITS.TECH_LEVEL) {
//                this.UpdateTechLevel();
//            }
//        }
//		this._availableResources[resource] += 1;
//        if (resourceBenefit == RESOURCE_BENEFITS.EXPANSION_RATE) {
//            this.UpdateExpansionRate();
//        }
//    }
//    internal void UpdateExpansionRate() {
//        this.expansionChance = this.kingdomTypeData.expansionRate;
//
//        for (int i = 0; i < this.availableResources.Keys.Count; i++) {
//            RESOURCE currResource = this.availableResources.Keys.ElementAt(i);
//            if (Utilities.GetBaseResourceType(currResource) == this.basicResource) {
//                int multiplier = this.availableResources[currResource];
//				RESOURCE_BENEFITS resourceBenefit = Utilities.resourceBenefits[currResource].Keys.FirstOrDefault();
//                float expansionRateGained = Utilities.resourceBenefits[currResource][resourceBenefit];
//                if (resourceBenefit == RESOURCE_BENEFITS.EXPANSION_RATE) {
//                    this.expansionChance += expansionRateGained * multiplier;
//                }
//            }
//        }
//    }
//    internal void UpdateTechLevel() {
//        this._techLevel = 0;
////        List<RESOURCE> allAvailableResources = this._availableResources.Keys.ToList();
////        for (int i = 0; i < allAvailableResources.Count; i++) {
//		foreach (RESOURCE currResource in this._availableResources.Keys) {
////            RESOURCE currResource = allAvailableResources[i];
//			RESOURCE_BENEFITS resourceBenefit = Utilities.resourceBenefits[currResource].Keys.FirstOrDefault();
//            if (resourceBenefit == RESOURCE_BENEFITS.TECH_LEVEL) {
//				int upgrade = (int)Utilities.resourceBenefits[currResource][resourceBenefit];
//				UpgradeTechLevel(upgrade);
//            }
//        }
//    }
//    internal void UpdateAllCitiesDailyGrowth() {
//        //get all resources from tiles and trade routes, only include trade routes where this kingom is the target
//        List<RESOURCE> allAvailableResources = this._availableResources.Keys.ToList();
//        int dailyGrowthGained = this.ComputeDailyGrowthGainedFromResources(allAvailableResources);
//        for (int i = 0; i < this.cities.Count; i++) {
//            City currCity = this.cities[i];
//            currCity.UpdateDailyGrowthBasedOnSpecialResources(dailyGrowthGained);
//        }
//    }
//    private int ComputeDailyGrowthGainedFromResources(List<RESOURCE> allAvailableResources) {
//        int dailyGrowthGained = 0;
//        for (int i = 0; i < allAvailableResources.Count; i++) {
//            RESOURCE currentResource = allAvailableResources[i];
//			RESOURCE_BENEFITS resourceBenefit = Utilities.resourceBenefits[currentResource].Keys.FirstOrDefault();
//            if(resourceBenefit == RESOURCE_BENEFITS.GROWTH_RATE) {
//                dailyGrowthGained += (int)Utilities.resourceBenefits[currentResource][resourceBenefit];
//            }
//        }
//        return dailyGrowthGained;
//    }
//    /*
//     * Gets a list of resources that otherKingdom does not have access to (By self or by trade).
//     * Will compare to this kingdoms available resources (excl. resources from trade)
//     * */
    internal List<RESOURCE> GetResourcesOtherKingdomDoesNotHave(Kingdom otherKingdom) {
        List<RESOURCE> resourcesOtherKingdomDoesNotHave = new List<RESOURCE>();
//        List<RESOURCE> allAvailableResourcesOfOtherKingdom = otherKingdom.availableResources.Keys.ToList();
		bool hasContainedResource = false;
		foreach (RESOURCE currKey in this._availableResources.Keys) {
			hasContainedResource = false;
			foreach (RESOURCE otherCurrKey in otherKingdom.availableResources.Keys) {
				if(otherCurrKey == currKey){
					hasContainedResource = true;
					break;
				}
			}
			if(!hasContainedResource){
				resourcesOtherKingdomDoesNotHave.Add(currKey);
			}
		}
        return resourcesOtherKingdomDoesNotHave;
    }
//    internal void UpdateAvailableResources() {
//        this._availableResources.Clear();
//        for (int i = 0; i < this.cities.Count; i++) {
//            City currCity = this.cities[i];
//            for (int j = 0; j < currCity.structures.Count; j++) {
//                HexTile currHexTile = currCity.structures[j];
//                if (currHexTile.specialResource != RESOURCE.NONE) {
//                    this.AddResourceToKingdom(currHexTile.specialResource);
//                }
//            }
//        }
//    }
    /*
     * <summary>
     * Set growth state of kingdom, disabling growth will prevent expansion,
     * building of new settlements and pregnancy
     * */
    internal void SetGrowthState(bool state) {
        _isGrowthEnabled = state;
    }
    #endregion

    #region Tech
 //   private void IncreaseTechCounterPerTick(){
	//	if(!this._isTechProducing){
	//		return;
	//	}
	//	int amount = this.cities.Count + GetTechContributionFromCitizens();
	//	amount = (int)(amount * this._techProductionPercentage);
	//	this.AdjustTechCounter (amount);
	//}
    internal int GetMonthlyTechGain() {
        int monthlyTechGain = GetTechContributionFromCitizens();
        for (int i = 0; i < cities.Count; i++) {
            City currCity = cities[i];
            if (!currCity.isDead) {
                monthlyTechGain += currCity.techPoints;
            }
        }

		int scientistFactor = (int)Math.Sqrt (50 * scientists);
        //Tech Gains
		monthlyTechGain = ((2 * scientistFactor * monthlyTechGain) / (scientistFactor + monthlyTechGain));
        monthlyTechGain = Mathf.FloorToInt(monthlyTechGain * techProductionPercentage);
        return monthlyTechGain;
    }
    internal int GetTechContributionFromCitizens() {
        int techContributionsFromCitizens = 0;
        techContributionsFromCitizens += king.GetTechContribution();
        for (int i = 0; i < cities.Count; i++) {
            techContributionsFromCitizens += cities[i].governor.GetTechContribution();
        }
        return techContributionsFromCitizens;
    }
	private void UpdateTechCapacity(){
		this._techCapacity = 500 + ((800 + (500 * this._techLevel)) * this._techLevel);
	}
	internal void AdjustTechCounter(int amount){
		this._techCounter += amount;
		this._techCounter = Mathf.Clamp(this._techCounter, 0, this._techCapacity);
		if(this._techCounter == this._techCapacity){
			this.UpgradeTechLevel (1);
		}
        if(UIManager.Instance.currentlyShowingKingdom != null && UIManager.Instance.currentlyShowingKingdom.id == this.id) {
            UIManager.Instance.UpdateTechMeter();
        }
	}
	internal void UpgradeTechLevel(int amount){
		this._techLevel += amount;
		if(this._techLevel < 0){
			amount -= this._techLevel;
			this._techLevel = 0;
		}
		this._techCounter = 0;
		AdjustPowerPointsToAllCities(amount);
        AdjustDefensePointsToAllCities(amount);
		this.UpdateTechCapacity();
        if (UIManager.Instance.currentlyShowingKingdom != null && UIManager.Instance.currentlyShowingKingdom.id == this.id) {
            UIManager.Instance.UpdateTechMeter();
        }
    }
    internal void DegradeTechLevel(int amount) {
        _techLevel -= amount;
        if(_techLevel >= 0) {
            AdjustPowerPointsToAllCities(-amount);
            AdjustDefensePointsToAllCities(-amount);
        }
        _techLevel = Mathf.Max(0, _techLevel);
        this._techCounter = 0;
        UpdateTechCapacity();
        if (UIManager.Instance.currentlyShowingKingdom != null && UIManager.Instance.currentlyShowingKingdom.id == this.id) {
            UIManager.Instance.UpdateTechMeter();
        }
    }
	#endregion
	
	#region Discovery
    /*
     * Check all the neighburs of the border tiles and owned tiles of all this kingdom's
     * cities, and check if any of them are owned by another kingdom, if so,
     * the two kingdoms have now discovered each other.
     * */
    internal void CheckForDiscoveredKingdoms() {
        for (int i = 0; i < this.cities.Count; i++) {
            City currCity = this.cities[i];
            List<HexTile> tilesToCheck = currCity.ownedTiles.Union(currCity.borderTiles).ToList();
            for (int j = 0; j < tilesToCheck.Count; j++) {
                //Get all neighbour tiles that are owned, but not by this kingdom, 
                //and that kingdom is not already added to this kingdom's discovered kingdoms.
                List<HexTile> neighbours = tilesToCheck[j].AllNeighbours;
                    //.Where(x => x.ownedByCity != null && x.ownedByCity.kingdom.id != this.id && !this._discoveredKingdoms.Contains(x.ownedByCity.kingdom))
                    //.ToList();
                for (int k = 0; k < neighbours.Count; k++) {
                    HexTile currNeighbour = neighbours[k];
                    if (currNeighbour.isOccupied && currNeighbour.ownedByCity != null
                        && currNeighbour.ownedByCity.kingdom.id != this.id) {
                        Kingdom otherKingdom = currNeighbour.ownedByCity.kingdom;
						KingdomManager.Instance.DiscoverKingdom (this, otherKingdom);
                    } else if (currNeighbour.isBorder) {
                        for (int l = 0; l < currNeighbour.isBorderOfCities.Count; l++) {
                            Kingdom otherKingdom = currNeighbour.isBorderOfCities[l].kingdom;
                            if (otherKingdom.id != this.id && !this.discoveredKingdoms.Contains(otherKingdom)) {
                                KingdomManager.Instance.DiscoverKingdom(this, otherKingdom);
                            }
                        }
                    }
                }
            }
        }
    }
    /*
     * Check all the neighbours of a HexTile and check if any of them are owned by another kingdom, if so,
     * the two kingdoms have now discovered each other.
     * */
    internal void CheckForDiscoveredKingdoms(City city) {
        //Get all neighbour tiles that are owned, but not by this kingdom, 
        //and that kingdom is not already added to this kingdom's discovered kingdoms.
        List<HexTile> tilesToCheck = city.ownedTiles.Union(city.borderTiles).ToList();
        for (int i = 0; i < tilesToCheck.Count; i++) {
            List<HexTile> neighbours = tilesToCheck[i].AllNeighbours;
            for (int j = 0; j < neighbours.Count; j++) {
                HexTile currNeighbour = neighbours[j];
                if (currNeighbour.isOccupied && currNeighbour.ownedByCity != null
                    && currNeighbour.ownedByCity.kingdom.id != this.id) {
                    Kingdom otherKingdom = currNeighbour.ownedByCity.kingdom;
					KingdomManager.Instance.DiscoverKingdom (this, otherKingdom);
                } else if (currNeighbour.isBorder) {
                    for (int k = 0; k < currNeighbour.isBorderOfCities.Count; k++) {
                        Kingdom otherKingdom = currNeighbour.isBorderOfCities[k].kingdom;
                        if (otherKingdom.id != this.id && !this.discoveredKingdoms.Contains(otherKingdom)) {
                            KingdomManager.Instance.DiscoverKingdom(this, otherKingdom);
                        }
                    }
                }
            }
        }
    }
    internal void DiscoverKingdom(Kingdom discoveredKingdom) {
        if(discoveredKingdom.id != this.id) {
            if (!this._discoveredKingdoms.Contains(discoveredKingdom)) {
                this._discoveredKingdoms.Add(discoveredKingdom);
                Debug.Log(this.name + " discovered " + discoveredKingdom.name + "!");
            }
        }
    }
    internal void RemoveKingdomFromDiscoveredKingdoms(Kingdom kingdomToRemove) {
        for (int i = 0; i < _discoveredKingdoms.Count; i++) {
            Kingdom currKingdom = _discoveredKingdoms[i];
            if(currKingdom.id == kingdomToRemove.id) {
                _discoveredKingdoms.RemoveAt(i);
                break;
            }
        }
        //this._discoveredKingdoms.Remove(kingdomToRemove);
    }
    #endregion

	#region Character Values
	//private void UpdateCharacterValuesOfKingsAndGovernors(){
	//	if(this.king != null){
	//		this.king.UpdateCharacterValues ();
	//	}
	//	for(int i = 0; i < this.cities.Count; i++){
	//		if(this.cities[i].governor != null){
	//			this.cities [i].governor.UpdateCharacterValues ();
	//		}
	//	}
	//}
    //internal void GenerateKingdomCharacterValues() {
    //    this._dictCharacterValues.Clear();
    //    this._dictCharacterValues = System.Enum.GetValues(typeof(CHARACTER_VALUE)).Cast<CHARACTER_VALUE>().ToDictionary(x => x, x => UnityEngine.Random.Range(1, 101));
    //    UpdateKingdomCharacterValues();
    //}
    //internal void UpdateKingdomCharacterValues() {
    //    this._importantCharacterValues = this._dictCharacterValues.Where(x => x.Value >= 50).OrderByDescending(x => x.Value).Take(4).ToDictionary(x => x.Key, x => x.Value);
    //}
    //private void UpdateSpecificCharacterValue(CHARACTER_VALUE key, int value) {
    //    if (this._dictCharacterValues.ContainsKey(key)) {
    //        this._dictCharacterValues[key] += value;
    //        //			UpdateCharacterValueByKey(key, value);
    //    }
    //}
    #endregion

    #region Bioweapon
    internal void SetBioWeapon(bool state){
		this._hasBioWeapon = state;
	}
	#endregion


	internal bool HasWar(Kingdom exceptionKingdom = null){
//		for(int i = 0; i < relationships.Count; i++){
//			if(relationships.ElementAt(i).Value.isAtWar){
//				return true;
//			}
//		}
		if(exceptionKingdom == null){
//			foreach (KingdomRelationship relationship in relationships.Values) {
//				if (relationship.isAtWar) {
//					return true;
//				}
//			}
			if(this._warfareInfo.Count > 0){
				return true;
			}
		}else{
			foreach (KingdomRelationship relationship in relationships.Values) {
				if (relationship.isAtWar && exceptionKingdom.id != relationship.targetKingdom.id) {
					return true;
				}
			}
		}

		return false;
	}

	internal Citizen GetRandomGovernorFromKingdom(){
		City randomCity = this.cities [UnityEngine.Random.Range (0, this.cities.Count)];
		return randomCity.governor;
	}

    #region Fog Of War
    internal void SetFogOfWarStateForRegion(Region region, FOG_OF_WAR_STATE fowState) {
        _regionFogOfWarDict[region] = fowState;
        for (int i = 0; i < region.tilesInRegion.Count; i++) {
            SetFogOfWarStateForTile(region.tilesInRegion[i], fowState);
        }
    }
    internal void SetFogOfWarStateForTile(HexTile tile, FOG_OF_WAR_STATE fowState) {
        FOG_OF_WAR_STATE previousStateOfTile = _fogOfWar[tile.xCoordinate, tile.yCoordinate];
        //Remove tile from previous list that it belonged to
        _fogOfWarDict[previousStateOfTile].Remove(tile);

        //Set new state of tile in fog of war dictionary
        _fogOfWar[tile.xCoordinate, tile.yCoordinate] = fowState;
        //Check if tile is already in the list
        if (!_fogOfWarDict[fowState].Contains(tile)) {
            //if not, add it to the new states list
            _fogOfWarDict[fowState].Add(tile);
        }

        //if this kingdom is currently the active kingdom, automatically update the tile visual
        if (UIManager.Instance.currentlyShowingKingdom != null && UIManager.Instance.currentlyShowingKingdom.id == this.id) {
            UpdateFogOfWarVisualForTile(tile, fowState);
        }

        //For checking if tile dictionary is accurate, remove this when checking is no longer necessary
        int sum = _fogOfWarDict.Sum(x => x.Value.Count);
        if (sum != GridMap.Instance.listHexes.Count) {
            throw new Exception("Fog of war dictionary is no longer accurate!");
        }
    }
    internal void UpdateFogOfWarVisual() {
        for (int x = 0; x < fogOfWar.GetLength(0); x++) {
            for (int y = 0; y < fogOfWar.GetLength(1); y++) {
                FOG_OF_WAR_STATE fowStateToUse = fogOfWar[x, y];
                HexTile currHexTile = GridMap.Instance.map[x, y];
                UpdateFogOfWarVisualForTile(currHexTile, fowStateToUse);
            }
        }
    }
    private void UpdateFogOfWarVisualForTile(HexTile hexTile, FOG_OF_WAR_STATE fowState) {
        hexTile.SetFogOfWarState(fowState);
    }
	internal FOG_OF_WAR_STATE GetFogOfWarStateOfTile(HexTile hexTile){
		return this._fogOfWar [hexTile.xCoordinate, hexTile.yCoordinate];
	}
    #endregion

	internal int GetNumberOfWars(){
		int numOfWars = 0;
//		for (int i = 0; i < relationships.Count; i++) {
//			if(relationships.ElementAt(i).Value.isAtWar){
//				numOfWars += 1;
//			}
//		}
		foreach (KingdomRelationship relationship in this.relationships.Values) {
			if(relationship.isAtWar){
				numOfWars += 1;
			}
		}
		if(numOfWars > 0){
			numOfWars -= 1;
		}
		return numOfWars;
	}

//	private void UpdateCombatStats(){
//		this._combatStats = this._kingdomTypeData.combatStats;
//		this._combatStats.waves = this._kingdomTypeData.combatStats.waves - (GetNumberOfWars() + this.rebellions.Count);
//	}

	internal void SetLockDown(bool state){
		this._isLockedDown = state;
	}

	internal void SetTechProduction(bool state){
		this._isTechProducing = state;
	}
	internal void SetTechProductionPercentage(float amount){
		this._techProductionPercentage = amount;
	}
	internal void SetProductionGrowthPercentage(float amount){
		this._productionGrowthPercentage = amount;
	}
	internal void SetSecession(bool state){
		this._hasSecession = state;
	}

	#region Crimes
	private void NewRandomCrimeDate(bool isFirst = false){
		int month = 0;
		int day = 0;
		if(isFirst){
			month = UnityEngine.Random.Range (1, 5);
			day = UnityEngine.Random.Range (1, GameManager.daysInMonth [month] + 1);
		}else{
			int lowerBoundMonth = this._crimeDate.month + 3;
			int upperBoundMonth = lowerBoundMonth + 1;

			month = UnityEngine.Random.Range (lowerBoundMonth, upperBoundMonth + 1);
			if(month > 12){
				month -= 12;
			}
			day = UnityEngine.Random.Range (1, GameManager.daysInMonth [month] + 1);
		}
		this._crimeDate.month = month;
		this._crimeDate.day = day;
	} 
	private void TriggerCrime(){
		if(!this.isDead){
			CreateCrime ();
			GameDate gameDate = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
			gameDate.AddMonths (3);
			int chance = UnityEngine.Random.Range (0, 2);
			if(chance == 0){
				gameDate.AddMonths (1);
			}
			gameDate.day = UnityEngine.Random.Range (1, GameManager.daysInMonth [gameDate.month]);

			SchedulingManager.Instance.AddEntry (gameDate.month, gameDate.day, gameDate.year, () => TriggerCrime());
		}
//		if(GameManager.Instance.month == this._crimeDate.month && GameManager.Instance.days == this._crimeDate.day){
//			NewRandomCrimeDate ();
//			CreateCrime ();
//		}
	}
	private void CreateCrime(){
		CrimeData crimeData = CrimeEvents.Instance.GetRandomCrime ();
		EventCreator.Instance.CreateCrimeEvent (this, crimeData);
	}
    #endregion

    #region Event Management
    internal void AddActiveEvent(GameEvent gameEvent) {
        this.activeEvents.Add(gameEvent);
    }
    internal void RemoveActiveEvent(GameEvent gameEvent) {
        this.activeEvents.Remove(gameEvent);
        AddToDoneEvents(gameEvent);
    }
    internal void AddToDoneEvents(GameEvent gameEvent) {
        this.doneEvents.Add(gameEvent);
        if (this.doneEvents.Count > KingdomManager.Instance.maxKingdomEventHistory) {
            this.doneEvents.RemoveAt(0);
        }
    }
    internal bool HasActiveEvent(EVENT_TYPES eventType) {
        for (int i = 0; i < this.activeEvents.Count; i++) {
            if (this.activeEvents[i].eventType == eventType) {
                return true;
            }
        }
        return false;
    }
    internal int GetActiveEventsOfTypeCount(EVENT_TYPES eventType) {
        int count = 0;
        for (int i = 0; i < this.activeEvents.Count; i++) {
            if (this.activeEvents[i].eventType == eventType) {
                count += 1;
            }
        }
        return count;
    }
    internal List<GameEvent> GetEventsOfType(EVENT_TYPES eventType, bool isActiveOnly = true) {
        List<GameEvent> gameEvents = new List<GameEvent>();
        for (int i = 0; i < this.activeEvents.Count; i++) {
            if (this.activeEvents[i].eventType == eventType) {
                gameEvents.Add(this.activeEvents[i]);
            }
        }
        if (!isActiveOnly) {
            for (int i = 0; i < this.doneEvents.Count; i++) {
                if (this.doneEvents[i].eventType == eventType) {
                    gameEvents.Add(this.doneEvents[i]);
                }
            }
        }
        return gameEvents;
    }
    //	internal bool HasActiveEventWith(EVENT_TYPES eventType, Kingdom kingdom){
    //		for (int i = 0; i < this.activeEvents.Count; i++) {
    //			if(this.activeEvents[i].eventType == eventType){
    //				return true;
    //			}
    //		}
    //		return false;
    //	}
    #endregion

    #region Governors Loyalty/Opinion
    internal void HasConflicted(GameEvent gameEvent){
		for(int i = 0; i < this.cities.Count; i++){
			if(this.cities[i].governor != null){
				((Governor)this.cities[i].governor.assignedRole).AddEventModifier (-10, "Recent border conflict", gameEvent);
			}
		}
	}
	//internal void UpdateAllGovernorsLoyalty(){
	//	for(int i = 0; i < this.cities.Count; i++){
	//		if(this.cities[i].governor != null){
	//			((Governor)this.cities[i].governor.assignedRole).UpdateLoyalty();
	//		}
	//	}
	//}
	#endregion

	#region Balance of Power
	internal void Militarize(bool state, bool isAttacking = false){
		this._isMilitarize = state;
		if(UIManager.Instance.currentlyShowingKingdom.id == this.id){
			UIManager.Instance.militarizingGO.SetActive (state);
		}
        if (state) {
            Kingdom kingdom2 = null;
            float highestInvasionValue = -1;
			bool isAtWar = false;
            foreach (KingdomRelationship kr in relationships.Values) {
                if (kr.isDiscovered) {
                    if(kr.targetKingdomInvasionValue > highestInvasionValue) {
                        kingdom2 = kr.targetKingdom;
                        highestInvasionValue = kr.targetKingdomInvasionValue;
                    }
                }
				if(kr.isAtWar && isAttacking){
					isAtWar = true;
					kingdom2 = kr.targetKingdom;
					break;
				}
            }
            if (kingdom2 != null) {
				string militarizeFileName = "militarize";
				if(isAttacking && isAtWar){
					militarizeFileName = "militarize_attack";
				}
				Log militarizeLog = new Log(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", militarizeFileName);
                militarizeLog.AddToFillers(this, this.name, LOG_IDENTIFIER.KINGDOM_1);
                militarizeLog.AddToFillers(kingdom2, kingdom2.name, LOG_IDENTIFIER.KINGDOM_2);
                UIManager.Instance.ShowNotification(militarizeLog, new HashSet<Kingdom>() { this }, false);
            }
        }
    }
	internal void Fortify(bool state, bool isUnderAttack = false){
		this._isFortifying = state;
		if(UIManager.Instance.currentlyShowingKingdom.id == this.id){
			UIManager.Instance.fortifyingGO.SetActive (state);
		}
		if (state) {
            Kingdom kingdom2 = null;
            float highestKingdomThreat = -1;
			bool isAtWar = false;
            foreach (KingdomRelationship kr in relationships.Values) {
                if (kr.isDiscovered) {
                    if (kr.targetKingdomThreatLevel > highestKingdomThreat) {
                        kingdom2 = kr.targetKingdom;
                        highestKingdomThreat = kr.targetKingdomThreatLevel;
                    }
                }
				if(kr.isAtWar && isUnderAttack){
					isAtWar = true;
					kingdom2 = kr.targetKingdom;
					break;
				}
            }
            if (kingdom2 != null) {
				string fortifyFileName = "fortify";
				if(isUnderAttack && isAtWar){
					fortifyFileName = "fortify_under_attack";
				}
				Log fortifyLog = new Log(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", fortifyFileName);
                fortifyLog.AddToFillers(this, this.name, LOG_IDENTIFIER.KINGDOM_1);
                fortifyLog.AddToFillers(kingdom2, kingdom2.name, LOG_IDENTIFIER.KINGDOM_2);
                UIManager.Instance.ShowNotification(fortifyLog, new HashSet<Kingdom>() { this }, false);
            }
		}
	}
	private void ScheduleActionDay(){
        this._evenActionDay = KingdomManager.Instance.GetEvenActionDay(this);
        //KingdomManager.Instance.IncrementEvenActionDay (2);
		SchedulingManager.Instance.AddEntry (GameManager.Instance.month, _evenActionDay, GameManager.Instance.year, () => ActionDay ());
		
	}
	private void ActionDay(){
		if(!this.isDead){
			UpdateThreatLevelsAndInvasionValues ();
			if (this.king.balanceType == PURPOSE.BALANCE) {
				SeeksBalance.Initialize(this);
			}else if (this.king.balanceType == PURPOSE.SUPERIORITY) {
				SeeksSuperiority.Initialize(this);
			}else if (this.king.balanceType == PURPOSE.BANDWAGON) {
				SeeksBandwagon.Initialize(this);
			}
			CheckStability ();

			GameDate gameDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
			gameDate.AddMonths (1);
			SchedulingManager.Instance.AddEntry (gameDate.month, gameDate.day, gameDate.year, () => ActionDay ());
		}
	}
	internal void AdjustStability(int amountToAdjust) {
    	this._stability += amountToAdjust;
        this._stability = Mathf.Clamp(this._stability, -100, 100);
    }
	internal void CheckStability(){
        if (this.isDead) {
            return;
        }
        //When Stability reaches -100, there will either be a rebellion, a plague or rioting.
        if (_stability <= -100) {
            if(kingdomSize != KINGDOM_SIZE.SMALL) {
                //Large or medium kingdom
                int chance = UnityEngine.Random.Range(0, 100);
                if (chance < 15) {
                    StartAutomaticRebellion();
                } else if (chance >= 15 && chance < 35) {
                    EventCreator.Instance.CreatePlagueEvent(this);
                } else if (chance >= 35 && chance < 60) {
                    EventCreator.Instance.CreateRiotEvent(this);
                } else if (chance >= 60 && chance < 85) {
                    EventCreator.Instance.CreateRiotSettlementEvent(this);
                } else {
                    EventCreator.Instance.CreateRegressionEvent(this);
                }
            } else {
                //small kingdom
                int chance = UnityEngine.Random.Range(0, 100);
                if (chance < 25) {
                    EventCreator.Instance.CreatePlagueEvent(this);
                } else if (chance >= 25 && chance < 55) {
                    EventCreator.Instance.CreateRiotEvent(this);
                } else if (chance >= 55 && chance < 85) {
                    EventCreator.Instance.CreateRiotSettlementEvent(this);
                } else {
                    EventCreator.Instance.CreateRegressionEvent(this);
                }
            }

            }
	}
    internal void AdjustBaseWeapons(int amountToAdjust) {
		this._baseWeapons += amountToAdjust;
		if(this._baseWeapons < 0){
			this._baseWeapons = 0;
		}
        KingdomManager.Instance.UpdateKingdomList();
    }
    internal void SetBaseWeapons(int newBaseWeapons) {
        _baseWeapons = newBaseWeapons;
        KingdomManager.Instance.UpdateKingdomList();
    }
	internal void AdjustBaseArmors(int amountToAdjust) {
		this._baseArmor += amountToAdjust;
		if(this._baseArmor < 0){
			this._baseArmor = 0;
		}
        KingdomManager.Instance.UpdateKingdomList();
	}
    internal void SetBaseArmor(int newBaseArmor) {
        _baseArmor = newBaseArmor;
        KingdomManager.Instance.UpdateKingdomList();
    }
    internal void ChangeStability(int newAmount) {
		this._stability = newAmount;
        this._stability = Mathf.Clamp(this._stability, -100, 100);
        CheckStability();
    }

    private void IncreaseBOPAttributesPerMonth() {
        if (this.isDead) {
            return;
        }
        int totalWeaponsIncrease = 0;
        int totalArmorIncrease = 0;
        int totalTechIncrease = GetTechContributionFromCitizens();
        //Kings and Governors provide monthly Stability gains based on their Efficiency trait.
        int totalStabilityIncrease = GetStabilityContributionFromCitizens();
        for (int i = 0; i < cities.Count; i++) {
            City currCity = cities[i];
            if (!currCity.isDead) {
                int weaponsContribution = currCity.powerPoints;
                int armorContribution = currCity.defensePoints;
                int techContribution = currCity.techPoints;
                currCity.MonthlyResourceBenefits(ref weaponsContribution, ref armorContribution, ref totalStabilityIncrease);
                totalWeaponsIncrease += weaponsContribution;
                totalArmorIncrease += armorContribution;
                totalTechIncrease += techContribution;
            }
        }

        if (isMilitarize) {
            //Militarizing multiplies Weapon production by 2.5 for the month in exchange for 0 Armor and Tech production.
            totalWeaponsIncrease = Mathf.FloorToInt(totalWeaponsIncrease * 2.5f);
            totalArmorIncrease = 0;
            totalTechIncrease = 0;
            Militarize(false);
        } else if (isFortifying) {
            //Fortifying multiplies Armor production by 2.5 for the month in exchange for 0 Weapon and Tech production.
            totalArmorIncrease = Mathf.FloorToInt(totalArmorIncrease * 2.5f);
            totalWeaponsIncrease = 0;
            totalTechIncrease = 0;
            Fortify(false);
        }
        //overpopulation reduces Stability by 1 point per 10% of Overpopulation each month
        int overpopulation = GetOverpopulationPercentage();
        totalStabilityIncrease -= overpopulation / 10;
        //When occupying an invaded city, monthly Stability is reduced by 2 for six months.
        totalStabilityIncrease -= (_stabilityDecreaseFromInvasionCounter * 2);

        //Stability has a -5 monthly reduction when the Kingdom is Medium and a -10 monthly reduction when the Kingdom is Large
        if (kingdomSize == KINGDOM_SIZE.MEDIUM) {
            totalStabilityIncrease -= 5;
        } else if(kingdomSize == KINGDOM_SIZE.LARGE) {
            totalStabilityIncrease -= 10;
        }

        AdjustBaseWeapons(totalWeaponsIncrease);
        AdjustBaseArmors(totalArmorIncrease);
        AdjustStability(Mathf.Clamp(totalStabilityIncrease, -5, 5));

        //Tech Gains
        totalTechIncrease = ((2 * scientists * totalTechIncrease) / (scientists + totalTechIncrease));
        totalTechIncrease = Mathf.FloorToInt(totalTechIncrease * techProductionPercentage);
        AdjustTechCounter(totalTechIncrease);

        //Reschedule event
        GameDate dueDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
        dueDate.AddMonths(1);
        SchedulingManager.Instance.AddEntry(dueDate.month, dueDate.day, dueDate.year, () => IncreaseBOPAttributesPerMonth());

        CheckStability();
    }
    internal int GetMonthlyStabilityGain() {
        int totalStabilityIncrease = GetStabilityContributionFromCitizens();
        //totalStabilityIncrease = Mathf.FloorToInt(totalStabilityIncrease * (1f - draftRate));
        for (int i = 0; i < cities.Count; i++) {
            City currCity = cities[i];
            if (!currCity.isDead) {
                int weaponsContribution = 0;
                int armorContribution = 0;
                currCity.MonthlyResourceBenefits(ref weaponsContribution, ref armorContribution, ref totalStabilityIncrease);
            }
        }
        //overpopulation reduces Stability by 1 point per 10% of Overpopulation each month
        int overpopulation = GetOverpopulationPercentage();
        totalStabilityIncrease -= overpopulation / 10;
        //When occupying an invaded city, monthly Stability is reduced by 2 for six months.
        totalStabilityIncrease -= (_stabilityDecreaseFromInvasionCounter * 2);

        //Stability has a -5 monthly reduction when the Kingdom is Medium and a -10 monthly reduction when the Kingdom is Large
        if (kingdomSize == KINGDOM_SIZE.MEDIUM) {
            totalStabilityIncrease -= 5;
        } else if (kingdomSize == KINGDOM_SIZE.LARGE) {
            totalStabilityIncrease -= 10;
        }

        return Mathf.Clamp(totalStabilityIncrease, -5, 5);
    }
    internal void AddStabilityDecreaseBecauseOfInvasion() {
        _stabilityDecreaseFromInvasionCounter += 1;
        //Reschedule event
        GameDate dueDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
        dueDate.AddMonths(6);
        SchedulingManager.Instance.AddEntry(dueDate.month, dueDate.day, dueDate.year, () => ReduceStabilityDecreaseBecauseOfInvasion());
        datesStabilityDecreaseWillExpire.Add(dueDate);
    }
    private void ReduceStabilityDecreaseBecauseOfInvasion() {
        datesStabilityDecreaseWillExpire.RemoveAt(0);
        _stabilityDecreaseFromInvasionCounter -= 1;
    }
    private int GetStabilityContributionFromCitizens() {
        int stabilityContributionsFromCitizens = 0;
        stabilityContributionsFromCitizens += king.GetStabilityContribution();
        for (int i = 0; i < cities.Count; i++) {
            stabilityContributionsFromCitizens += cities[i].governor.GetStabilityContribution();
        }
        return stabilityContributionsFromCitizens;
    }
    //    internal void AdjustBaseWeapons(int adjustment) {
    //        _baseWeapons += adjustment;
    //    }
    //    internal void AdjustBaseArmor(int adjustment) {
    //        _baseArmor += adjustment;
    //    }
    internal void UpdateProductionRatesFromKing() {
        _researchRateFromKing = 0f;
        _draftRateFromKing = 0f;
        _productionRateFromKing = 0f;

        switch (king.science) {
            case SCIENCE.ERUDITE:
                _researchRateFromKing = 0.10f;
                break;
            case SCIENCE.ACADEMIC:
                _researchRateFromKing = 0.05f;
                break;
            case SCIENCE.IGNORANT:
                _researchRateFromKing = -0.05f;
                break;
            default:
                break;
        }
        _productionRateFromKing -= _researchRateFromKing;

        switch (king.military) {
            case MILITARY.HOSTILE:
                _draftRateFromKing = 0.10f;
                break;
            case MILITARY.MILITANT:
                _draftRateFromKing = 0.05f;
                break;
            case MILITARY.PACIFIST:
                _draftRateFromKing = -0.05f;
                break;
            default:
                break;
        }
        _productionRateFromKing -= _draftRateFromKing;


    }
    #endregion

    #region Adjacency
    internal void AddAdjacentKingdom(Kingdom kingdom) {
        if (!_adjacentKingdoms.Contains(kingdom)) {
            this._adjacentKingdoms.Add(kingdom);
        }
    }
    internal void RemoveAdjacentKingdom(Kingdom kingdom) {
        this._adjacentKingdoms.Remove(kingdom);
    }
    internal void RevalidateKingdomAdjacency(City removedCity) {
        List<Kingdom> kingdomsToCheck = new List<Kingdom>();
        for (int i = 0; i < removedCity.region.adjacentRegions.Count; i++) {
            Region currRegion = removedCity.region.adjacentRegions[i];
            if (currRegion.occupant != null) {
                if (currRegion.occupant.kingdom != this && !kingdomsToCheck.Contains(currRegion.occupant.kingdom)) {
                    kingdomsToCheck.Add(currRegion.occupant.kingdom);
                }
            }
        }
        for (int i = 0; i < kingdomsToCheck.Count; i++) {
            Kingdom otherKingdom = kingdomsToCheck[i];
            KingdomRelationship kr = GetRelationshipWithKingdom(otherKingdom, true);
            if (kr != null && kr.isAdjacent) {
                bool isValid = false;
                //Revalidate adjacency
                for (int j = 0; j < cities.Count; j++) {
                    Region regionOfCurrCity = cities[j].region;
                    foreach (Region otherRegion in regionOfCurrCity.adjacentRegions.Where(x => x.occupant != null && x.occupant.kingdom.id != this.id)) {
                        if (otherRegion.occupant.kingdom.id == otherKingdom.id) {
                            //otherKingdom is still adjacent to this kingdom, validity verified!
                            isValid = true;
                            break;
                        } 
                        //else if (kingdomsToCheck.Contains(otherRegion.occupant.kingdom)) {
                        //    //otherRegion.occupant.kingdom is still adjacent to this kingdom, validity verified!
                        //    isValid = true;
                        //    break;
                        //}
                    }
                    if (isValid) {
                        //otherKingdom has already been verified! Skip checking of other cities
                        break;
                    }
                }
                //Loop of all cities is done, check if validity has returned a true value,
                //if not, this kingdom and other kingdom are no longer adjacent, change appropriately
                if (!isValid) {
                    kr.ChangeAdjacency(false);
                }
            }
        }

    }
    #endregion

    #region Population
    internal int GetOverpopulationPercentage() {
        float overpopulationPercentage = ((float)_population / (float)_populationCapacity) * 100f;
        //overpopulationPercentage = overpopulationPercentage * 100 - 100;
        overpopulationPercentage -= 100;
        overpopulationPercentage = Mathf.Clamp(overpopulationPercentage, 0f, 100f);
        return Mathf.FloorToInt(overpopulationPercentage);
    }
    internal void UpdatePopulationCapacity() {
        _populationCapacity = GetPopulationCapacity();
    }
    internal int GetPopulationCapacity() {
        int populationCapacity = 0;
        for (int i = 0; i < cities.Count; i++) {
            populationCapacity += 500 + (50 * cities[i].cityLevel);
        }
        return populationCapacity;
    }
    internal int GetPopulationGrowth() {
        int populationGrowth = 0;
        for (int i = 0; i < cities.Count; i++) {
			populationGrowth += cities[i].region.populationGrowth + (cities[i].cityLevel * 2);
        }
		populationGrowth += this.techLevel * cities.Count;

		// If a Kingdom has active Plagues, its Population decreases instead
		// Its possible to have multiple active plagues in the same Kingdom. The kingdom will lose 50% of its current population growth per active plague.
		int activePlagues = GetActiveEventsOfTypeCount(EVENT_TYPES.PLAGUE);
		if (activePlagues > 0) {
			return Mathf.FloorToInt(-(float)populationGrowth * (activePlagues * 0.5f));
		}

		// Positive population growth is decreased by overpopulation
        float overpopulationPercentage = GetOverpopulationPercentage();
        populationGrowth = Mathf.FloorToInt(populationGrowth * ((100f - overpopulationPercentage) * 0.01f));

		return populationGrowth;

    }
    private void IncreasePopulationEveryMonth() {
        AdjustPopulation(GetPopulationGrowth());
        GameDate dueDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
        dueDate.AddMonths(1);
        SchedulingManager.Instance.AddEntry(dueDate.month, dueDate.day, dueDate.year, () => IncreasePopulationEveryMonth());
    }
    internal void AdjustPopulation(int adjustment) {
        _population += adjustment;
        if(_population <= 0) {
            //if at any time population is reduced to 0, the Kingdom will cease to exist and all his cities will be destroyed
            while (cities.Count > 0) {
                cities[0].KillCity();
            }
        }
        KingdomManager.Instance.UpdateKingdomList();
    }
    internal void SetPopulation(int newPopulation) {
        _population = newPopulation;
        KingdomManager.Instance.UpdateKingdomList();
    }
    #endregion

    #region Automatic Rebellion
    private void StartAutomaticRebellion() {
        //If a Kingdom has a -100 Stability, a Rebellion will automatically occur which will be started by the one with the most 
        //negative opinion towards the King. The Kingdom's Stability will then reset back to 50.
        List<Citizen> possibleCitizensForRebellion = new List<Citizen>();
        for (int i = 0; i < cities.Count; i++) {
            possibleCitizensForRebellion.AddRange(cities[i].citizens.Where(x => x.role != ROLE.KING && x.role != ROLE.UNTRAINED));
        }
        if (possibleCitizensForRebellion.Count > 0) {
            possibleCitizensForRebellion.OrderBy(x => x.loyaltyToKing).First().StartRebellion();
        }
    }
    #endregion

    internal void MobilizingState(bool state){
		this._isMobilizing = state;
	}

	private void AdjustPowerPointsToAllCities(int amount){
		for (int i = 0; i < this.cities.Count; i++) {
			this.cities[i].AdjustPowerPoints(amount);
		}
	}

    private void AdjustDefensePointsToAllCities(int amount) {
        for (int i = 0; i < this.cities.Count; i++) {
            this.cities[i].AdjustDefensePoints(amount);
        }
    }

	internal void AdjustWarmongerValue(int amount){
		this._warmongerValue += amount;
		this._warmongerValue = Mathf.Clamp(this._warmongerValue, 0, 200);
	}

	internal void SetWarmongerValue(int amount){
		this._warmongerValue = amount;
	}

	internal void WarmongerDecreasePerYear(){
		if(!this.isDead){
			if (!HasWar ()) {
				AdjustWarmongerValue (-5);
			}
			SchedulingManager.Instance.AddEntry (1, 1, GameManager.Instance.year + 1, () => WarmongerDecreasePerYear ());
		}
	}
	internal void UpdateThreatLevelsAndInvasionValues(){
		has100OrAboveThreat = false;
		if(this.king.balanceType == PURPOSE.BANDWAGON || this.king.balanceType == PURPOSE.SUPERIORITY){
			this.highestThreatAdjacentKingdomAbove50 = null;
			this.highestThreatAdjacentKingdomAbove50Value = 0f;
			this.highestRelativeStrengthAdjacentKingdom = null;
			this.highestRelativeStrengthAdjacentKingdomValue = 0;
			foreach (KingdomRelationship relationship in this.relationships.Values) {
				relationship.UpdateThreatLevelAndInvasionValue ();
			}
//			highestThreatAdjacentKingdomAbove50 = GetKingdomWithHighestAtLeastAbove50Threat ();
			foreach (KingdomRelationship relationship in this.relationships.Values) {
				relationship.UpdateLikeness (null);
			}
		}else{
			foreach (KingdomRelationship relationship in this.relationships.Values) {
				relationship.UpdateThreatLevelAndInvasionValue ();
				relationship.UpdateLikeness (null);
			}
		}
	}

	internal void SeekAlliance(){
		List<KingdomRelationship> kingdomRelationships = this.relationships.Values.OrderByDescending(x => x.totalLike).ToList ();
		Kingdom kingdomWithHighestThreat = GetKingdomWithHighestThreat();
		if(kingdomWithHighestThreat != null){
			for (int i = 0; i < kingdomRelationships.Count; i++) {
				KingdomRelationship kr = kingdomRelationships [i];
				if(kr.isDiscovered && !kr.cantAlly){
					if(kr.targetKingdom.id != kingdomWithHighestThreat.id){
						KingdomRelationship rk = kr.targetKingdom.GetRelationshipWithKingdom (kingdomWithHighestThreat);
						if(rk.isAdjacent){
							if(kr.targetKingdom.alliancePool == null){
								Debug.Log(name + " is looking to create an alliance with " + kr.targetKingdom.name);
								bool hasCreated = KingdomManager.Instance.AttemptToCreateAllianceBetweenTwoKingdoms(this, kr.targetKingdom);
								if(hasCreated){
									string log = name + " has created an alliance with ";
									for (int j = 0; j < _alliancePool.kingdomsInvolved.Count; j++) {
										if(_alliancePool.kingdomsInvolved[j].id != id) {
											log += _alliancePool.kingdomsInvolved[j].name;
											if(j + 1 < _alliancePool.kingdomsInvolved.Count) {
												log += ", ";
											}
										}
									}
									Debug.Log(log);
									break;
								}
							}else{
								Debug.Log(name + " is looking to join the alliance of " + kr.targetKingdom.name);
								bool hasJoined = kr.targetKingdom.alliancePool.AttemptToJoinAlliance(this, kr.targetKingdom);
								if(hasJoined){
									string log = name + " has joined an alliance with ";
									for (int j = 0; j < _alliancePool.kingdomsInvolved.Count; j++) {
										if (_alliancePool.kingdomsInvolved[j].id != id) {
											log += _alliancePool.kingdomsInvolved[j].name;
											if (j + 1 < _alliancePool.kingdomsInvolved.Count) {
												log += ", ";
											}
										}
									}
									break;
								}
							}
						}
					}
				}
			}
		}
		if(this._alliancePool == null){
			for (int i = 0; i < kingdomRelationships.Count; i++) {
				KingdomRelationship kr = kingdomRelationships [i];
				if(kr.isDiscovered && !kr.cantAlly){
					if(kr.targetKingdom.alliancePool == null){
						Debug.Log(name + " is looking to create an alliance with " + kr.targetKingdom.name);
						bool hasCreated = KingdomManager.Instance.AttemptToCreateAllianceBetweenTwoKingdoms(this, kr.targetKingdom);
						if(hasCreated){
							string log = name + " has created an alliance with ";
							for (int j = 0; j < _alliancePool.kingdomsInvolved.Count; j++) {
								if(_alliancePool.kingdomsInvolved[j].id != id) {
									log += _alliancePool.kingdomsInvolved[j].name;
									if(j + 1 < _alliancePool.kingdomsInvolved.Count) {
										log += ", ";
									}
								}
							}
							Debug.Log(log);
							break;
						}
					}else{
						Debug.Log(name + " is looking to join the alliance of " + kr.targetKingdom.name);
						bool hasJoined = kr.targetKingdom.alliancePool.AttemptToJoinAlliance(this, kr.targetKingdom);
						if(hasJoined){
							string log = name + " has joined an alliance with ";
							for (int j = 0; j < _alliancePool.kingdomsInvolved.Count; j++) {
								if (_alliancePool.kingdomsInvolved[j].id != id) {
									log += _alliancePool.kingdomsInvolved[j].name;
									if (j + 1 < _alliancePool.kingdomsInvolved.Count) {
										log += ", ";
									}
								}
							}
							break;
						}
					}
				}
			}
		}
        if(_alliancePool == null) {
            Debug.Log(name + " has failed to create/join an alliance");
        }
	}
	internal void SeekAllianceWith(Kingdom targetKingdom){
		if(targetKingdom.alliancePool == null){
			Debug.Log(name + " is looking to create an alliance with " + targetKingdom.name);
			bool hasCreated = KingdomManager.Instance.AttemptToCreateAllianceBetweenTwoKingdoms(this, targetKingdom);
			if(hasCreated){
				string log = name + " has created an alliance with ";
				for (int j = 0; j < _alliancePool.kingdomsInvolved.Count; j++) {
					if(_alliancePool.kingdomsInvolved[j].id != id) {
						log += _alliancePool.kingdomsInvolved[j].name;
						if(j + 1 < _alliancePool.kingdomsInvolved.Count) {
							log += ", ";
						}
					}
				}
				Debug.Log(log);
			}
		}else{
			Debug.Log(name + " is looking to join the alliance of " + targetKingdom.name);
			bool hasJoined = targetKingdom.alliancePool.AttemptToJoinAlliance(this, targetKingdom);
			if(hasJoined){
				string log = name + " has joined an alliance with ";
				for (int j = 0; j < _alliancePool.kingdomsInvolved.Count; j++) {
					if (_alliancePool.kingdomsInvolved[j].id != id) {
						log += _alliancePool.kingdomsInvolved[j].name;
						if (j + 1 < _alliancePool.kingdomsInvolved.Count) {
							log += ", ";
						}
					}
				}
			}
		}
	}
	internal void SetAlliancePool(AlliancePool alliancePool){
		this._alliancePool = alliancePool;
	}
	internal int GetUnadjacentPosAllianceWeapons(){
		int posAlliancePower = 0;
		if(this.alliancePool != null){
			for (int i = 0; i < this.alliancePool.kingdomsInvolved.Count; i++) {
				Kingdom kingdomInAlliance = this.alliancePool.kingdomsInvolved[i];
				if(this.id != kingdomInAlliance.id){
					KingdomRelationship relationship = kingdomInAlliance.GetRelationshipWithKingdom(this);
					if(relationship.totalLike >= 35){
						posAlliancePower += (int)((float)kingdomInAlliance.baseWeapons * 0.1f);
					}
				}
			}
        }
		return posAlliancePower;
	}
	internal int GetUnadjacentPosAllianceArmor(){
		int posAllianceDefense = 0;
		if(this.alliancePool != null){
			for (int i = 0; i < this.alliancePool.kingdomsInvolved.Count; i++) {
				Kingdom kingdomInAlliance = this.alliancePool.kingdomsInvolved[i];
				if(this.id != kingdomInAlliance.id){
					KingdomRelationship relationship = kingdomInAlliance.GetRelationshipWithKingdom(this);
					if(relationship.totalLike >= 35){
						posAllianceDefense += (int)((float)kingdomInAlliance.baseArmor * 0.1f);
					}
				}
			}
        }
//		foreach (KingdomRelationship relationship in this.relationships.Values) {
////			if(relationship.isAlly){
//				KingdomRelationship relationshipFrom = relationship.targetKingdom.GetRelationshipWithKingdom (this);
//				if(relationshipFrom.totalLike >= 35){
//					posAllianceDefense += relationship.targetKingdom.baseDefense;
//				}
////			}
//		}
		return posAllianceDefense;
	}
	internal void AddWarfareInfo(WarfareInfo info){
		if(!this._warfareInfo.ContainsKey(info.warfare.id)){
			this._warfareInfo.Add(info.warfare.id, info);
		}
	}
	internal void RemoveWarfareInfo(Warfare warfare){
		this._warfareInfo.Remove(warfare.id);
	}
	internal WarfareInfo GetWarfareInfo(int id){
		if(this._warfareInfo.ContainsKey(id)){
			return this._warfareInfo[id];
		}
		return new WarfareInfo(WAR_SIDE.NONE, null);
	}
	internal void LeaveAlliance(bool doNotShowLog = false){
		if(this.alliancePool != null){
            AlliancePool leftAlliance = this.alliancePool;
			object[] objects = leftAlliance.kingdomsInvolved.ToArray ();
			for (int i = 0; i < this.alliancePool.kingdomsInvolved.Count; i++) {
				if(this.alliancePool.kingdomsInvolved[i].id != this.id){
					KingdomRelationship kr = this.alliancePool.kingdomsInvolved [i].GetRelationshipWithKingdom (this);
					kr.AddRelationshipModifier (-50, "Broken Alliance", RELATIONSHIP_MODIFIER.LEAVE_ALLIANCE, true, false);
					kr.ChangeCantAlly (true);
				}
			}
            this.alliancePool.RemoveKingdomInAlliance(this);
			//When leaving an alliance, Stability is reduced by 15
			this.AdjustStability(-15);
			if(!doNotShowLog){
				Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Alliance", "leave_alliance");
				newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.KINGDOM_1);
                newLog.AddToFillers(null, leftAlliance.name, LOG_IDENTIFIER.ALLIANCE_NAME);
				newLog.AddAllInvolvedObjects (objects);
				UIManager.Instance.ShowNotification (newLog);
			}
		}
	}
	internal bool IsUnderAttack(){
		for (int i = 0; i < this.cities.Count; i++) {
			if(this.cities[i].isDefending){
				return true;
			}
		}
		return false;
	}
	internal bool IsAttacking(){
		for (int i = 0; i < this.cities.Count; i++) {
			if(this.cities[i].isAttacking){
				return true;
			}
		}
		return false;
	}
	internal string ProvideWeaponsArmorsAidToKingdom(Kingdom kingdomToBeProvided, float transferPercentage){
		string weaponsOrArmor = "Weapons";
		bool isAllyUnderAttack = kingdomToBeProvided.IsUnderAttack ();
		bool isAllyAttacking = kingdomToBeProvided.IsAttacking ();
		int transferAmount = 0;
		if(isAllyUnderAttack && isAllyAttacking){
			weaponsOrArmor = "Armors";
		}else{
			if(isAllyUnderAttack){
				weaponsOrArmor = "Armors";
			}
		}

		if(weaponsOrArmor == "Weapons"){
			transferAmount = (int)(this.baseWeapons * transferPercentage);
			this.AdjustBaseWeapons (-transferAmount);
			kingdomToBeProvided.AdjustBaseWeapons (transferAmount);
		}else{
			transferAmount = (int)(this.baseArmor * transferPercentage);
			this.AdjustBaseArmors (-transferAmount);
			kingdomToBeProvided.AdjustBaseArmors (transferAmount);
		}

		return transferAmount.ToString () + " " + weaponsOrArmor;
	}
	internal Kingdom GetKingdomWithHighestThreat(){
		float highestThreatLevel = 0f;
		Kingdom threat = null;
		foreach (KingdomRelationship kr in relationships.Values) {
			if(kr.targetKingdomThreatLevel > highestThreatLevel){
				highestThreatLevel = kr.targetKingdomThreatLevel;
				threat = kr.targetKingdom;
			}
		}
		return threat;
	}
	internal void ShowTransferWeaponsArmorsLog(Kingdom allyKingdom, string amount){
        List<Kingdom> kingdomsToShowNotif = new List<Kingdom>();
        foreach (WarfareInfo currWarFare in this.warfareInfo.Values) {
            foreach (List<Kingdom> kingdomsInvolved in currWarFare.warfare.kingdomSideList.Values) {
                kingdomsToShowNotif.AddRange(kingdomsInvolved);
            }
        }
        kingdomsToShowNotif.Add(allyKingdom);
        Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", "transfer_weapons_armors");
		newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (allyKingdom, allyKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		newLog.AddToFillers (null, amount, LOG_IDENTIFIER.OTHER);
		UIManager.Instance.ShowNotification (newLog, new HashSet<Kingdom>(kingdomsToShowNotif));
	}
	internal void ShowJoinWarLog(Kingdom allyKingdom, Warfare warfare){
        List<Kingdom> kingdomsToShowNotif = new List<Kingdom>();
        kingdomsToShowNotif.AddRange(warfare.GetListFromSide(WAR_SIDE.A));
        kingdomsToShowNotif.AddRange(warfare.GetListFromSide(WAR_SIDE.B));
        foreach (WarfareInfo currWarFare in allyKingdom.warfareInfo.Values) {
            foreach (List<Kingdom> kingdomsInvolved in currWarFare.warfare.kingdomSideList.Values) {
                kingdomsToShowNotif.AddRange(kingdomsInvolved);
            }
        }
        Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", "join_war");
		newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (null, warfare.name, LOG_IDENTIFIER.WAR_NAME);
		newLog.AddToFillers (allyKingdom, allyKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		UIManager.Instance.ShowNotification (newLog, new HashSet<Kingdom>(kingdomsToShowNotif));
	}
	internal void ShowRefuseAndLeaveAllianceLog(AlliancePool alliance, Warfare warfare){
        List<Kingdom> kingdomsToShowNotif = new List<Kingdom>();
        kingdomsToShowNotif.Add(this);
        kingdomsToShowNotif.AddRange(warfare.GetListFromSide(WAR_SIDE.A));
        kingdomsToShowNotif.AddRange(warfare.GetListFromSide(WAR_SIDE.B));
        kingdomsToShowNotif.AddRange(alliance.kingdomsInvolved);
        Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", "refuse_and_leave_alliance");
		newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (null, warfare.name, LOG_IDENTIFIER.WAR_NAME);
		newLog.AddToFillers (null, alliance.name, LOG_IDENTIFIER.ALLIANCE_NAME);
		newLog.AddAllInvolvedObjects (alliance.kingdomsInvolved.ToArray ());
		UIManager.Instance.ShowNotification (newLog, new HashSet<Kingdom>(kingdomsToShowNotif));
	}
	internal void ShowDoNothingLog(Warfare warfare){
		Debug.Log(this.name + " decided to do nothing with " + warfare.name);

        List<Kingdom> kingdomsToShowNotif = new List<Kingdom>();
        kingdomsToShowNotif.AddRange(warfare.GetListFromSide(WAR_SIDE.A));
        kingdomsToShowNotif.AddRange(warfare.GetListFromSide(WAR_SIDE.B));
        Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", "do_nothing_war");
		newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (null, warfare.name, LOG_IDENTIFIER.WAR_NAME);
		UIManager.Instance.ShowNotification (newLog, new HashSet<Kingdom>(kingdomsToShowNotif));
	}
	internal void ShowBetrayalWarLog(Warfare warfare, Kingdom kingdom){
        List<Kingdom> kingdomsToShowNotif = new List<Kingdom>();
        kingdomsToShowNotif.Add(kingdom);
        kingdomsToShowNotif.AddRange(warfare.GetListFromSide(WAR_SIDE.A));
        kingdomsToShowNotif.AddRange(warfare.GetListFromSide(WAR_SIDE.B));
        foreach (WarfareInfo currWarFare in kingdom.warfareInfo.Values) {
            foreach (List<Kingdom> kingdomsInvolved in currWarFare.warfare.kingdomSideList.Values) {
                kingdomsToShowNotif.AddRange(kingdomsInvolved);
            }
        }
        Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", "betrayal_war");
		newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (null, warfare.name, LOG_IDENTIFIER.WAR_NAME);
		newLog.AddToFillers (kingdom, kingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		UIManager.Instance.ShowNotification (newLog, new HashSet<Kingdom>(kingdomsToShowNotif));
	}
	internal void ShowBetrayalProvideLog(AlliancePool alliance, string logAmount, Kingdom kingdom){
        List<Kingdom> kingdomsToShowNotif = new List<Kingdom>();
        kingdomsToShowNotif.Add(this);
        kingdomsToShowNotif.Add(kingdom);
        kingdomsToShowNotif.AddRange(alliance.kingdomsInvolved);

        foreach (WarfareInfo currWarFare in this.warfareInfo.Values) {
            foreach (List<Kingdom> kingdomsInvolved in currWarFare.warfare.kingdomSideList.Values) {
                kingdomsToShowNotif.AddRange(kingdomsInvolved);
            }
        }
        foreach (WarfareInfo currWarFare in kingdom.warfareInfo.Values) {
            foreach (List<Kingdom> kingdomsInvolved in currWarFare.warfare.kingdomSideList.Values) {
                kingdomsToShowNotif.AddRange(kingdomsInvolved);
            }
        }
        Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", "betrayal_provide");
		newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (null, alliance.name, LOG_IDENTIFIER.ALLIANCE_NAME);
		newLog.AddToFillers (null, logAmount, LOG_IDENTIFIER.OTHER);
		newLog.AddToFillers (kingdom, kingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		UIManager.Instance.ShowNotification (newLog, new HashSet<Kingdom>(kingdomsToShowNotif));
	}

	internal Kingdom GetKingdomWithHighestAtLeastAbove50Threat(){
		float highestThreat = 0f;
		Kingdom highestThreatKingdom = null;
		foreach (KingdomRelationship kr in relationships.Values) {
			float threat = kr.targetKingdomThreatLevel;
			if(kr.isAdjacent && threat > 50f){
				if(highestThreatKingdom == null){
					highestThreatKingdom = kr.targetKingdom;
					highestThreat = threat;
				}else{
					if(threat > highestThreat){
						highestThreatKingdom = kr.targetKingdom;
						highestThreat = threat;
					}
				}
			}
		}
		return highestThreatKingdom;
	}
	private Kingdom GetAdjacentKingdomLeastLike(){
		int leastLike = 0;
		Kingdom targetKingdom = null;
		foreach (KingdomRelationship kr in relationships.Values) {
			if(kr.isAdjacent && !kr.AreAllies() && kr.totalLike < 0){
				if(targetKingdom == null){
					targetKingdom = kr.targetKingdom;
					leastLike = kr.totalLike;
				}else{
					if(kr.totalLike < leastLike){
						targetKingdom = kr.targetKingdom;
						leastLike = kr.totalLike;
					}
				}
			}
		}
		return targetKingdom;
	}
	internal int GetSumOfCityLevels(){
		return this.cities.Sum (x => x.cityLevel);
	}

	#region Subterfuge
	internal void Subterfuge(){
		Kingdom targetKingdom = GetAdjacentKingdomLeastLike ();
		if(targetKingdom != null){
			int triggerChance = UnityEngine.Random.Range (0, 100);
			int triggerValue = 5;
			if(this.king.loyalty == LOYALTY.SCHEMING){
				triggerValue += 3;
			}
			if(triggerChance < triggerValue){
				SUBTERFUGE_ACTIONS subterfuge = GetSubterfugeAction ();
				int successChance = UnityEngine.Random.Range (0, 100);
				int caughtChance = UnityEngine.Random.Range (0, 100);
				int successValue = 60;
				if(this.king.intelligence == INTELLIGENCE.SMART){
					successValue += 15;
				}else if(this.king.intelligence == INTELLIGENCE.DUMB){
					successValue -= 15;
				}
				if(successChance < successValue){
					//Success
					CreateSuccessSubterfugeAction(subterfuge, targetKingdom);

				}else{
					//Fail
					int criticalFailChance = UnityEngine.Random.Range (0, 100);
					int criticalFailValue = 20;
					if(this.king.efficiency == EFFICIENCY.INEPT){
						criticalFailValue += 5;
					}else if(this.king.efficiency == EFFICIENCY.EFFICIENT){
						criticalFailValue -= 5;
					}
					if(criticalFailChance < criticalFailValue){
						//Critical Failure
						CreateCriticalFailSubterfugeAction(subterfuge, targetKingdom);
					}else{
						//Failure
						CreateFailSubterfugeAction(subterfuge, targetKingdom);
					}
				}

				if(caughtChance < 5){
					CreateCaughtSubterfugeAction (subterfuge, targetKingdom);
				}
			
			}
		}
	}
	private SUBTERFUGE_ACTIONS GetSubterfugeAction(){
		int chance = UnityEngine.Random.Range (0, 100);

		if (this.king.balanceType == PURPOSE.BALANCE) {
			if(chance < 40){
				return SUBTERFUGE_ACTIONS.DESTROY_WEAPONS;
			}else if(chance >= 40 && chance < 70){
				return SUBTERFUGE_ACTIONS.REDUCE_STABILITY;
			}else if(chance >= 70 && chance < 90){
				return SUBTERFUGE_ACTIONS.FLATTER;
			}else{
				return SUBTERFUGE_ACTIONS.SPREAD_PLAGUE;
			}
		}

		if (this.king.balanceType == PURPOSE.SUPERIORITY) {
			if(chance < 40){
				return SUBTERFUGE_ACTIONS.DESTROY_ARMORS;
			}else if(chance >= 40 && chance < 70){
				return SUBTERFUGE_ACTIONS.REDUCE_STABILITY;
			}else if(chance >= 70 && chance < 90){
				return SUBTERFUGE_ACTIONS.DESTROY_WEAPONS;
			}else{
				return SUBTERFUGE_ACTIONS.SPREAD_PLAGUE;
			}
		}

		if (this.king.balanceType == PURPOSE.BANDWAGON) {
			if(chance < 40){
				return SUBTERFUGE_ACTIONS.FLATTER;
			}else if(chance >= 40 && chance < 70){
				return SUBTERFUGE_ACTIONS.DESTROY_WEAPONS;
			}else if(chance >= 70 && chance < 90){
				return SUBTERFUGE_ACTIONS.SPREAD_PLAGUE;
			}else{
				return SUBTERFUGE_ACTIONS.DESTROY_ARMORS;
			}
		}

		return SUBTERFUGE_ACTIONS.DESTROY_WEAPONS;
	}

	private void CreateSuccessSubterfugeAction(SUBTERFUGE_ACTIONS subterfuge, Kingdom targetKingdom){
		if(subterfuge == SUBTERFUGE_ACTIONS.DESTROY_WEAPONS){
			int weaponsDestroyed = targetKingdom.DestroyWeaponsSubterfuge ();
			ShowSuccessSubterfugeLog (subterfuge, targetKingdom, weaponsDestroyed);
		}else if(subterfuge == SUBTERFUGE_ACTIONS.DESTROY_ARMORS){
			int armorsDestroyed = targetKingdom.DestroyArmorsSubterfuge ();
			ShowSuccessSubterfugeLog (subterfuge, targetKingdom, armorsDestroyed);
		}else if(subterfuge == SUBTERFUGE_ACTIONS.REDUCE_STABILITY){
			targetKingdom.InciteUnrestSubterfuge ();
			ShowSuccessSubterfugeLog (subterfuge, targetKingdom);
		}else if(subterfuge == SUBTERFUGE_ACTIONS.FLATTER){
			FlatterSubterfuge (targetKingdom, 25);
			ShowSuccessSubterfugeLog (subterfuge, targetKingdom);
		}else if(subterfuge == SUBTERFUGE_ACTIONS.SPREAD_PLAGUE){
			string plagueName = targetKingdom.SpreadPlague ();
			ShowSuccessSubterfugeLog (subterfuge, targetKingdom, 0, plagueName);
		}
	}
	private void CreateCriticalFailSubterfugeAction(SUBTERFUGE_ACTIONS subterfuge, Kingdom targetKingdom){
		if(subterfuge == SUBTERFUGE_ACTIONS.DESTROY_WEAPONS){
			int weaponsDestroyed = DestroyWeaponsSubterfuge ();
			ShowCriticalFailSubterfugeLog (subterfuge, targetKingdom, weaponsDestroyed);
		}else if(subterfuge == SUBTERFUGE_ACTIONS.DESTROY_ARMORS){
			int armorsDestroyed = DestroyArmorsSubterfuge ();
			ShowCriticalFailSubterfugeLog (subterfuge, targetKingdom, armorsDestroyed);
		}else if(subterfuge == SUBTERFUGE_ACTIONS.REDUCE_STABILITY){
			InciteUnrestSubterfuge ();
			ShowCriticalFailSubterfugeLog (subterfuge, targetKingdom);
		}else if(subterfuge == SUBTERFUGE_ACTIONS.FLATTER){
			FlatterSubterfuge (targetKingdom, -25);
			ShowCriticalFailSubterfugeLog (subterfuge, targetKingdom);
		}else if(subterfuge == SUBTERFUGE_ACTIONS.SPREAD_PLAGUE){
			string plagueName = SpreadPlague ();
			ShowCriticalFailSubterfugeLog (subterfuge, targetKingdom, 0, plagueName);
		}
	}
	private void CreateFailSubterfugeAction(SUBTERFUGE_ACTIONS subterfuge, Kingdom targetKingdom){
		ShowFailSubterfugeLog (subterfuge, targetKingdom);
	}
	private void CreateCaughtSubterfugeAction(SUBTERFUGE_ACTIONS subterfuge, Kingdom targetKingdom){
		string incidentName = RandomNameGenerator.Instance.GetInternationalIncidentName ();
		ShowCaughtSubterfugeLog (subterfuge, targetKingdom, incidentName);
	}
	private int DestroyWeaponsSubterfuge(){
		int weaponsToBeDestroyed = (int)((float)this._baseWeapons * 0.05f);
		this.AdjustBaseWeapons (-weaponsToBeDestroyed);
		return weaponsToBeDestroyed;
	}
	private int DestroyArmorsSubterfuge(){
		int armorsToBeDestroyed = (int)((float)this._baseArmor * 0.05f);
		this.AdjustBaseArmors (-armorsToBeDestroyed);
		return armorsToBeDestroyed;
	}
	private void InciteUnrestSubterfuge(){
		this.AdjustStability (-5);
	}
	private void FlatterSubterfuge(Kingdom targetKingdom, int modifier){
		KingdomRelationship kr = targetKingdom.GetRelationshipWithKingdom (this);
		kr.AddRelationshipModifier (modifier, "Flatter", RELATIONSHIP_MODIFIER.FLATTER, true, false);
	}
	private string SpreadPlague(){
		Plague plague = EventCreator.Instance.CreatePlagueEvent(this, false);
		return plague._plagueName;
	}
	private void AssassinateKing(Kingdom targetKingdom){
		
	}
	private void ShowSuccessSubterfugeLog(SUBTERFUGE_ACTIONS subterfuge, Kingdom targetKingdom, int weaponsArmorsDestroyed = 0, string plagueName = ""){
		Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Subterfuge", subterfuge.ToString() + "_SUCCESS");
		newLog.AddToFillers (this.king, this.king.name, LOG_IDENTIFIER.KING_1);
		newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (targetKingdom, targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		if (subterfuge == SUBTERFUGE_ACTIONS.DESTROY_ARMORS || subterfuge == SUBTERFUGE_ACTIONS.DESTROY_WEAPONS) {
			newLog.AddToFillers (null, weaponsArmorsDestroyed.ToString(), LOG_IDENTIFIER.OTHER);
		}else if (subterfuge == SUBTERFUGE_ACTIONS.SPREAD_PLAGUE) {
			newLog.AddToFillers (null, plagueName.ToString(), LOG_IDENTIFIER.OTHER);
		}else if (subterfuge == SUBTERFUGE_ACTIONS.FLATTER) {
			newLog.AddToFillers (targetKingdom.king, targetKingdom.king.name, LOG_IDENTIFIER.KING_2);
		}
		UIManager.Instance.ShowNotification (newLog);
	}
	private void ShowFailSubterfugeLog(SUBTERFUGE_ACTIONS subterfuge, Kingdom targetKingdom, int weaponsArmorsDestroyed = 0, string plagueName = ""){
		Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Subterfuge", subterfuge.ToString() + "_FAIL");
		newLog.AddToFillers (this.king, this.king.name, LOG_IDENTIFIER.KING_1);
		newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (targetKingdom, targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		if (subterfuge == SUBTERFUGE_ACTIONS.FLATTER) {
			newLog.AddToFillers (targetKingdom.king, targetKingdom.king.name, LOG_IDENTIFIER.KING_2);
		}
		UIManager.Instance.ShowNotification (newLog);
	}
	private void ShowCriticalFailSubterfugeLog(SUBTERFUGE_ACTIONS subterfuge, Kingdom targetKingdom, int weaponsArmorsDestroyed = 0, string plagueName = ""){
		Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Subterfuge", subterfuge.ToString() + "_CRITICAL");
		newLog.AddToFillers (this.king, this.king.name, LOG_IDENTIFIER.KING_1);
		newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (targetKingdom, targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		if (subterfuge == SUBTERFUGE_ACTIONS.DESTROY_ARMORS || subterfuge == SUBTERFUGE_ACTIONS.DESTROY_WEAPONS) {
			newLog.AddToFillers (null, weaponsArmorsDestroyed.ToString(), LOG_IDENTIFIER.OTHER);
		}else if (subterfuge == SUBTERFUGE_ACTIONS.SPREAD_PLAGUE) {
			newLog.AddToFillers (null, plagueName.ToString(), LOG_IDENTIFIER.OTHER);
		}else if (subterfuge == SUBTERFUGE_ACTIONS.FLATTER) {
			newLog.AddToFillers (targetKingdom.king, targetKingdom.king.name, LOG_IDENTIFIER.KING_2);
		}
		UIManager.Instance.ShowNotification (newLog);
	}
	private void ShowCaughtSubterfugeLog(SUBTERFUGE_ACTIONS subterfuge, Kingdom targetKingdom, string incidentName){
		Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Subterfuge", "caught");
		newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (targetKingdom, targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		newLog.AddToFillers (null, incidentName, LOG_IDENTIFIER.OTHER);
		UIManager.Instance.ShowNotification (newLog);
	}
	#endregion
}
