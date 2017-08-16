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
	public RACE race;
    public int age;
    private int foundationYear;
    private int foundationMonth;
    private int foundationDay;

    [NonSerialized] public int[] horoscope; 

	[SerializeField]
	private KingdomTypeData _kingdomTypeData;
	private Kingdom _sourceKingdom;

    //Resources
    private int _goldCount;
    private int _maxGold = 5000;
    private Dictionary<RESOURCE, int> _availableResources; //only includes resources that the kingdom has bought via tile purchasing

    //Trading
    private Dictionary<Kingdom, EMBARGO_REASON> _embargoList;

    private int _unrest;

    private List<City> _cities;
	private List<Camp> camps;
	internal City capitalCity;
	internal Citizen king;
	internal List<Citizen> successionLine;
	internal List<Citizen> pretenders;

	internal List<Rebellion> rebellions;

	internal BASE_RESOURCE_TYPE basicResource;

	internal List<RelationshipKingdom> relationshipsWithOtherKingdoms;

	internal Color kingdomColor;
	internal List<History> kingdomHistory;

	internal List<City> adjacentCitiesFromOtherKingdoms;
	internal List<Kingdom> adjacentKingdoms;

	private List<Kingdom> _discoveredKingdoms;

//	private CombatStats _combatStats;

	//Plague
	internal Plague plague;

	//Boon of Power
	private List<BoonOfPower> _boonOfPowers;
	private List<BoonOfPower> _activatedBoonOfPowers;

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
    private Dictionary<FOG_OF_WAR_STATE, List<HexTile>> _fogOfWarDict;

	//Crimes
	private CrimeData _crimeData;
	private CrimeDate _crimeDate;

	//Events of Kingdom
	private List<GameEvent> _activeEvents;
	private List<GameEvent> _doneEvents;


    //Expansion
    private float expansionChance = 1f;

    protected Dictionary<CHARACTER_VALUE, int> _dictCharacterValues;
    protected Dictionary<CHARACTER_VALUE, int> _importantCharacterValues;

    protected const int INCREASE_CITY_HP_CHANCE = 5;
	protected const int INCREASE_CITY_HP_AMOUNT = 20;
    protected const int GOLD_GAINED_FROM_TRADE = 10;
    protected const int UNREST_DECREASE_PER_MONTH = -5;
    protected const int UNREST_INCREASE_CONQUER = 5;
    protected const int UNREST_INCREASE_EMBARGO = 5;

	private bool _isDead;
	private bool _hasBioWeapon;
	private bool _isLockedDown;
	private bool _isTechProducing;
//	internal bool hasConflicted;

	private int borderConflictLoyaltyExpiration;
	private float _techProductionPercentage;
	private float _productionGrowthPercentage;

	private bool _hasUpheldHiddenHistoryBook;

	private bool _hasSecession;
	private bool _hasRiot;

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
	public Kingdom sourceKingdom {
		get { return this._sourceKingdom; }
	}
    public int goldCount {
        get { return this._goldCount; }
    }
    public int maxGold {
        get { return this._maxGold; }
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
//	public List<Camp> camps{
//		get{ return this._camps; }
//	}
    public int unrest {
        get { return this._unrest; }
		set { this._unrest = value;}
    }
    public int basicResourceCount {
        get { return this._availableResources.Where(x => Utilities.GetBaseResourceType(x.Key) == this.basicResource).Sum(x => x.Value); }
    }
    /*
     * Will return all discovered kingdoms, otherwise, if useDiscoveredKingdoms
     * is set to false will return all kingdoms except this one.
     * */
    public List<Kingdom> discoveredKingdoms {
        get {
            if (KingdomManager.Instance.useDiscoveredKingdoms) {
                return this._discoveredKingdoms;
            } else {
                return KingdomManager.Instance.allKingdoms.Where(x => x.id != this.id).ToList();
            }
        }
    }
	public int techLevel{
		get{return this._techLevel + (3 * this._activatedBoonOfPowers.Count);}
	}
	public int techCapacity{
		get{return this._techCapacity;}
	}
	public int techCounter{
		get{return this._techCounter;}
	}
    public float expansionRate {
        get { return this.expansionChance; }
    }
    public Dictionary<CHARACTER_VALUE, int> dictCharacterValues {
        get { return this._dictCharacterValues; }
    }
    public Dictionary<CHARACTER_VALUE, int> importantCharacterValues {
        get { return this._importantCharacterValues; }
    }

    public bool hasBioWeapon {
		get { return this._hasBioWeapon; }
	}
	public EventRate[] dailyCumulativeEventRate {
		get { return this._dailyCumulativeEventRate; }
	}
    public List<City> plaguedCities {
        get { return this.cities.Where(x => x.plague != null).ToList(); }
    }
    public bool isGrowthEnabled {
        get { return _isGrowthEnabled; }
    }
	public List<City> nonRebellingCities {
		get { return this.cities.Where(x => x.rebellion == null).ToList(); }
	}
	public int serumsOfAlacrity {
		get { return this._serumsOfAlacrity; }
	}
    public FOG_OF_WAR_STATE[,] fogOfWar {
        get { return _fogOfWar; }
    }
    public Dictionary<FOG_OF_WAR_STATE, List<HexTile>> fogOfWarDict {
        get { return _fogOfWarDict; }
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
	public float productionGrowthPercentage {
		get { return this._productionGrowthPercentage; }
	}
	public bool hasUpheldHiddenHistoryBook{
		get { return this._hasUpheldHiddenHistoryBook;}
	}
	public bool hasSecession{
		get { return this._hasSecession;}
	}
	public bool hasRiot{
		get { return this._hasRiot;}
	}

	public List<GameEvent> activeEvents{
		get { return this._activeEvents;}
	}
	public List<GameEvent> doneEvents{
		get { return this._doneEvents;}
	}
    #endregion

    // Kingdom constructor paramters
    //	race - the race of this kingdom
    //	cities - the cities that this kingdom will initially own
    //	sourceKingdom (optional) - the kingdom from which this new kingdom came from
    public Kingdom(RACE race, List<HexTile> cities, Kingdom sourceKingdom = null) {
		this.id = Utilities.SetID(this);
		this.race = race;
        SetGrowthState(true);
		this.name = RandomNameGenerator.Instance.GenerateKingdomName(this.race);
		this.king = null;
        this.successionLine = new List<Citizen>();
		this.pretenders = new List<Citizen> ();
		this._cities = new List<City> ();
		this.camps = new List<Camp> ();
		this.kingdomHistory = new List<History>();
		this.kingdomColor = Utilities.GetColorForKingdom();
		this.adjacentCitiesFromOtherKingdoms = new List<City>();
		this.adjacentKingdoms = new List<Kingdom>();
		this._goldCount = 0;
		this._availableResources = new Dictionary<RESOURCE, int> ();
		this.relationshipsWithOtherKingdoms = new List<RelationshipKingdom>();
		this._isDead = false;
		this._isLockedDown = false;
		this._hasUpheldHiddenHistoryBook = false;
        this._embargoList = new Dictionary<Kingdom, EMBARGO_REASON>();
        this._unrest = 0;
		this._sourceKingdom = sourceKingdom;
//		this.hasConflicted = false;
		this.borderConflictLoyaltyExpiration = 0;
		this.rebellions = new List<Rebellion> ();
		this._discoveredKingdoms = new List<Kingdom>();
		this._techLevel = 1;
		this._techCounter = 0;
		this._hasBioWeapon = false;
		this._boonOfPowers = new List<BoonOfPower> ();
		this._activatedBoonOfPowers = new List<BoonOfPower> ();
		this.plague = null;
        this.age = 0;
        this.foundationYear = GameManager.Instance.year;
        this.foundationDay = GameManager.Instance.days;
        this.foundationMonth = GameManager.Instance.month;
        this._dictCharacterValues = new Dictionary<CHARACTER_VALUE, int>();
        this._importantCharacterValues = new Dictionary<CHARACTER_VALUE, int>();
        this._fogOfWar = new FOG_OF_WAR_STATE[(int)GridMap.Instance.width, (int)GridMap.Instance.height];
        this._fogOfWarDict = new Dictionary<FOG_OF_WAR_STATE, List<HexTile>>();
        _fogOfWarDict.Add(FOG_OF_WAR_STATE.HIDDEN, new List<HexTile>(GridMap.Instance.listHexes.Select(x => x.GetComponent<HexTile>())));
        _fogOfWarDict.Add(FOG_OF_WAR_STATE.SEEN, new List<HexTile>());
        _fogOfWarDict.Add(FOG_OF_WAR_STATE.VISIBLE, new List<HexTile>());
		this._activeEvents = new List<GameEvent> ();
		this._doneEvents = new List<GameEvent> ();


        this.GenerateKingdomCharacterValues();
        this.SetLockDown(false);
		this.SetTechProduction(true);
		this.SetTechProductionPercentage(1);
		this.SetProductionGrowthPercentage(1);
		this.UpdateTechCapacity ();
		this.SetSecession (false);
		this.SetRiot (false);
		this.NewRandomCrimeDate (true);
		// Determine what type of Kingdom this will be upon initialization.
		this._kingdomTypeData = null;
		this.UpdateKingdomTypeData();

        this.basicResource = Utilities.GetBasicResourceForRace(race);

        

        if (cities.Count > 0) {
            for (int i = 0; i < cities.Count; i++) {
                this.CreateNewCityOnTileForKingdom(cities[i]);
            }
        }
		
		//if(this._cities.Count > 0 && this._cities[0] != null){
		//	this.capitalCity = this._cities [0];

  //          // For the kingdom's first city, setup its distance towards other habitable tiles.
  //          HexTile habitableTile;
  //          if (this.basicResource == BASE_RESOURCE_TYPE.STONE) {
  //              for (int i = 0; i < CityGenerator.Instance.stoneHabitableTiles.Count; i++) {
  //                  habitableTile = CityGenerator.Instance.stoneHabitableTiles[i];
  //                  this.cities[0].AddHabitableTileDistance(habitableTile, PathGenerator.Instance.GetDistanceBetweenTwoTiles(this.cities[0].hexTile, habitableTile));
  //              }
  //          } else if (this.basicResource == BASE_RESOURCE_TYPE.WOOD) {
  //              for (int i = 0; i < CityGenerator.Instance.woodHabitableTiles.Count; i++) {
  //                  habitableTile = CityGenerator.Instance.woodHabitableTiles[i];
  //                  this.cities[0].AddHabitableTileDistance(habitableTile, PathGenerator.Instance.GetDistanceBetweenTwoTiles(this.cities[0].hexTile, habitableTile));
  //              }
  //          }
  //      }
		
//		Debug.Log ("Kingdom: " + this.name + " : " + this.cities [0].habitableTileDistance.Count);
		//this.cities [0].OrderHabitableTileDistanceList ();


		this.CreateInitialRelationships();
		Messenger.AddListener<Kingdom>("OnNewKingdomCreated", CreateNewRelationshipWithKingdom);
		Messenger.AddListener("OnDayEnd", KingdomTickActions);
        Messenger.AddListener<Kingdom>("OnKingdomDied", OtherKingdomDiedActions);
        //EventManager.Instance.onKingdomDiedEvent.AddListener(OtherKingdomDiedActions);
		this.kingdomHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "This kingdom was born.", HISTORY_IDENTIFIER.NONE));
	}

    internal void SetGrowthState(bool state) {
        _isGrowthEnabled = state;
    }

	// Updates this kingdom's type and horoscope
	public void UpdateKingdomTypeData() {
		// Update Kingdom Type whenever the kingdom expands to a new city
		KingdomTypeData prevKingdomTypeData = this._kingdomTypeData;
		this._kingdomTypeData = StoryTellingManager.Instance.InitializeKingdomType (this);
		this._dailyCumulativeEventRate = this._kingdomTypeData.dailyCumulativeEventRate;
		// If the Kingdom Type Data changed
		if (this._kingdomTypeData != prevKingdomTypeData) {			
			// Update horoscope
			if (prevKingdomTypeData == null) {
				this.horoscope = GetHoroscope ();
			} else {				
				this.horoscope = GetHoroscope (prevKingdomTypeData.kingdomType);
			}
            // Update expansion chance
            this.UpdateExpansionRate();

			//Update Character Values of King and Governors
			this.UpdateCharacterValuesOfKingsAndGovernors();

			//Update Relationship Opinion
			UpdateAllRelationshipKings();
        }

//		UpdateCombatStats();
    }
	internal int[] GetHoroscope(KINGDOM_TYPE prevKingdomType = KINGDOM_TYPE.NONE){
		int[] newHoroscope = new int[2];

		if (this._kingdomTypeData.kingdomType == KINGDOM_TYPE.BARBARIC_TRIBE) {
			newHoroscope [0] = UnityEngine.Random.Range (0, 2);
			newHoroscope [1] = 0;
		} else if (this._kingdomTypeData.kingdomType == KINGDOM_TYPE.HERMIT_TRIBE) {
			newHoroscope [0] = UnityEngine.Random.Range (0, 2);
			newHoroscope [1] = 1;
		} else if (this._kingdomTypeData.kingdomType == KINGDOM_TYPE.RELIGIOUS_TRIBE) {
			newHoroscope [0] = 0;
			newHoroscope [1] = UnityEngine.Random.Range (0, 2);
		} else if (this._kingdomTypeData.kingdomType == KINGDOM_TYPE.OPPORTUNISTIC_TRIBE) {
			newHoroscope [0] = 1;
			newHoroscope [1] = UnityEngine.Random.Range (0, 2);
		} else if (this._kingdomTypeData.kingdomType == KINGDOM_TYPE.NOBLE_KINGDOM) {
			newHoroscope [0] = 0;
			newHoroscope [1] = 0;
		} else if (this._kingdomTypeData.kingdomType == KINGDOM_TYPE.EVIL_EMPIRE) {
			newHoroscope [0] = 1;
			newHoroscope [1] = 0;
		} else if (this._kingdomTypeData.kingdomType == KINGDOM_TYPE.MERCHANT_NATION) {
			newHoroscope [0] = 0;
			newHoroscope [1] = 1;
		} else if (this._kingdomTypeData.kingdomType == KINGDOM_TYPE.CHAOTIC_STATE) {
			newHoroscope [0] = 1;
			newHoroscope [1] = 1;
		} else if (this._kingdomTypeData.kingdomType == KINGDOM_TYPE.RIGHTEOUS_SUPERPOWER) {
			if (prevKingdomType == KINGDOM_TYPE.NOBLE_KINGDOM) {
				newHoroscope [0] = 0;
				newHoroscope [1] = UnityEngine.Random.Range (0, 2);
			} else if (prevKingdomType == KINGDOM_TYPE.MERCHANT_NATION) {
				newHoroscope [0] = UnityEngine.Random.Range (0, 2);
				newHoroscope [1] = 1;				
			}
		} else if (this._kingdomTypeData.kingdomType == KINGDOM_TYPE.WICKED_SUPERPOWER) {
			if (prevKingdomType == KINGDOM_TYPE.EVIL_EMPIRE) {
				newHoroscope [0] = 1;
				newHoroscope [1] = UnityEngine.Random.Range (0, 2);
			} else if (prevKingdomType == KINGDOM_TYPE.CHAOTIC_STATE) {
				newHoroscope [0] = UnityEngine.Random.Range (0, 2);
				newHoroscope [1] = 0;				
			}
		}

		return newHoroscope;
	}

	// Function to call if you want to determine whether the Kingdom is still alive or dead
	// At the moment, a Kingdom is considered dead if it doesnt have any cities.
	public bool isAlive() {
		if (this.nonRebellingCities.Count > 0) {
			return true;
		}
		return false;
	}

	/*
	 * Every time a city of this kingdom dies, check if
	 * this kingdom has no more cities, if so, the kingdom is
	 * considered dead. Remove all ties from other kingdoms.
	 * */
	internal void CheckIfKingdomIsDead(){
		if (this.cities.Count <= 0) {
			//Kingdom is dead
			this.DestroyKingdom();
		}
	}

	/*
	 * Kill this kingdom. This removes all ties with other kingdoms.
	 * Only call this when a kingdom has no more cities.
	 * */
	internal void DestroyKingdom(){
		this._isDead = true;
        this.CancelEventKingdomIsInvolvedIn(EVENT_TYPES.ALL);
        //EventManager.Instance.onCreateNewKingdomEvent.RemoveListener(CreateNewRelationshipWithKingdom);
        Messenger.RemoveListener<Kingdom>("OnNewKingdomCreated", CreateNewRelationshipWithKingdom);
        Messenger.RemoveListener("OnDayEnd", KingdomTickActions);
        Messenger.RemoveListener<Kingdom>("OnKingdomDied", OtherKingdomDiedActions);
        //EventManager.Instance.onKingdomDiedEvent.RemoveListener(OtherKingdomDiedActions);

        //EventManager.Instance.onKingdomDiedEvent.Invoke(this);
		Messenger.Broadcast<Kingdom>("OnKingdomDied", this);

        KingdomManager.Instance.RemoveRelationshipToOtherKings(this.king);
        this.RemoveRelationshipsWithOtherKingdoms();
        KingdomManager.Instance.allKingdoms.Remove(this);

        UIManager.Instance.CheckIfShowingKingdomIsAlive(this);

        Debug.Log(this.id + " - Kingdom: " + this.name + " has died!");
        Debug.Log("Stack Trace: " + System.Environment.StackTrace);
	}

    private void CancelEventKingdomIsInvolvedIn(EVENT_TYPES eventType) {
        if (eventType == EVENT_TYPES.KINGDOM_WAR) {
            List<GameEvent> wars = EventManager.Instance.GetAllEventsKingdomIsInvolvedIn(this, new EVENT_TYPES[] { EVENT_TYPES.KINGDOM_WAR }).Where(x => x.isActive).ToList();
            for (int i = 0; i < wars.Count; i++) {
                wars[i].CancelEvent();
            }
        } else {
            List<GameEvent> allEvents = EventManager.Instance.GetAllEventsKingdomIsInvolvedIn(this, new EVENT_TYPES[] { EVENT_TYPES.ALL }).Where(x => x.isActive).ToList();
            for (int i = 0; i < allEvents.Count; i++) {
                allEvents[i].CancelEvent();
            }
        }
    }
		
	protected void CreateInitialRelationships() {
		for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
			if (KingdomManager.Instance.allKingdoms[i].id != this.id) {
				this.relationshipsWithOtherKingdoms.Add (new RelationshipKingdom(this, KingdomManager.Instance.allKingdoms [i]));
			}
		}
	}

	/*
	 * Used to create a new RelationshipKingdom with the
	 * newly created kingdom. Function is listening to onCreateNewKingdom Event.
	 * */
	protected void CreateNewRelationshipWithKingdom(Kingdom createdKingdom){
		if (createdKingdom.id == this.id) {
			return;
		}
		for (int i = 0; i < this.relationshipsWithOtherKingdoms.Count; i++) {
			if (this.relationshipsWithOtherKingdoms [i].targetKingdom.id == createdKingdom.id) {
				//this kingdom already has a relationship with created kingdom!
				return;
			}
		}
		this.relationshipsWithOtherKingdoms.Add(new RelationshipKingdom(this, createdKingdom));
	}

	protected void RemoveRelationshipsWithOtherKingdoms(){
		this.relationshipsWithOtherKingdoms.Clear();
	}

	/*
	 * Used to remove a relationship between 2 kingdoms.
	 * Usually done when a kingdom dies.
	 * */
	protected void RemoveRelationshipWithKingdom(Kingdom kingdomToRemove){
		if (kingdomToRemove.id == this.id) {
			return;
		}
		for (int i = 0; i < this.relationshipsWithOtherKingdoms.Count; i++) {
			if (this.relationshipsWithOtherKingdoms[i].targetKingdom.id == kingdomToRemove.id) {
                Debug.Log(this.name + " removed relationship with " + kingdomToRemove.name);
				this.relationshipsWithOtherKingdoms.RemoveAt(i); //remove relationship with kingdom
				break;
			}
		}
	}

    protected void RemoveRelationshipWithKing(Kingdom kingdomToRemove) {
        if (kingdomToRemove.id == this.id) {
            return;
        }
        for (int i = 0; i < king.relationshipKings.Count; i++) {
            RelationshipKings currRel = king.relationshipKings[i];
            if(currRel.king.city.kingdom.id == kingdomToRemove.id) {
                Debug.Log("King " + king.name + " of " + this.name + " removed relationship with " + currRel.king.name + " of " + currRel.king.city.kingdom.name);
                king.relationshipKings.Remove(currRel);
                break;
            }
        }
    }

    protected void OtherKingdomDiedActions(Kingdom kingdomThatDied) {
        if (kingdomThatDied.id != this.id) {
            RemoveRelationshipWithKingdom(kingdomThatDied);
            //RemoveAllTradeRoutesWithOtherKingdom(kingdomThatDied);
            RemoveRelationshipWithKing(kingdomThatDied);
            RemoveKingdomFromDiscoveredKingdoms(kingdomThatDied);
            RemoveKingdomFromEmbargoList(kingdomThatDied);
        }
    }

	/*
	 * This function is listening to the onWeekEnd Event. Put functions that you want to
	 * happen every tick here.
	 * */
	protected void KingdomTickActions(){
        //this.ProduceGoldFromTrade();
        this.AttemptToAge();
        this.AdaptToKingValues();
        if (_isGrowthEnabled) {
            this.AttemptToExpand();
        }
//		this.AttemptToCreateAttackCityEvent ();
//		this.AttemptToCreateReinforcementEvent ();
        //		this.AttemptToIncreaseCityHP();
        this.DecreaseUnrestEveryMonth();
//		this.CheckBorderConflictLoyaltyExpiration ();
		this.IncreaseTechCounterPerTick();
        this.TriggerEvents();
        //if (GameManager.Instance.days == GameManager.daysInMonth[GameManager.Instance.month]) {
        //    this.AttemptToTrade();
        //}
    }

    private void AdaptToKingValues() {
        if(GameManager.Instance.days == 1 && GameManager.Instance.month == 1) {
            for (int i = 0; i < _dictCharacterValues.Count; i++) {
                CHARACTER_VALUE currValue = _dictCharacterValues.ElementAt(i).Key;
                if (king.importantCharacterValues.ContainsKey(currValue)) {
                    UpdateSpecificCharacterValue(currValue, 1);
                } else {
                    UpdateSpecificCharacterValue(currValue, -1);
                }
            }
            UpdateKingdomCharacterValues();
        }
    }

    private void AttemptToAge() {
        if(GameManager.Instance.year > foundationYear && GameManager.Instance.month == foundationMonth && GameManager.Instance.days == foundationDay) {
            age += 1;
        }
    }

    private void TriggerEvents() {
        this.TriggerSlavesMerchant();
        this.TriggerHypnotism();
        this.TriggerKingdomHoliday();
        //this.TriggerDevelopWeapons();
        this.TriggerKingsCouncil();
		this.TriggerCrime ();
    }
	/*
	 * Attempt to create an attack city event
	 * This will only happen if there's a war with any other kingdom
	 * */
//	private void AttemptToCreateAttackCityEvent(){
//		if (this.activeCitiesPairInWar.Count > 0) {
//			CityWarPair warPair = this.activeCitiesPairInWar [0];
//			if (warPair.sourceCity == null || warPair.targetCity == null) {
//				return;
//			}
//			warPair.sourceCity.AttackCityEvent (warPair.targetCity);
//		}
//	}

	/*
	 * Attempt to create a reinforcement event to increase a friendly city's hp
	 * This will only happen if there's a war with any other kingdom
	 * */
//	private void AttemptToCreateReinforcementEvent(){
//		int chance = UnityEngine.Random.Range (0, 100);
//		if(chance < this.kingdomTypeData.warReinforcementCreationRate){
//			EventCreator.Instance.CreateReinforcementEvent (this);
//		}
//	}

	/*
	 * Kingdom will attempt to expand. 
	 * Chance for expansion can be edited by changing the value of expansionChance.
	 * NOTE: expansionChance increases on it's own.
	 * */
	protected void AttemptToExpand(){
		if (EventManager.Instance.GetEventsStartedByKingdom(this, new EVENT_TYPES[]{EVENT_TYPES.EXPANSION}).Count() > 0) {
			return;
		}
        float upperBound = 300f + (150f * (float)this.cities.Count);
        float chance = UnityEngine.Random.Range (0, upperBound);
		if (chance < this.expansionChance) {
			//Debug.Log ("Expansion Rate: " + this.expansionChance);		
			//List<City> citiesThatCanExpand = new List<City> ();
			//List<Citizen> allUnassignedAdultCitizens = new List<Citizen> ();
			//List<Resource> expansionCost = new List<Resource> () {
			//	new Resource (BASE_RESOURCE_TYPE.GOLD, 0)
			//};

			if (this.cities.Count > 0) {
				EventCreator.Instance.CreateExpansionEvent (this);
			}

		}
	}

	/*
	 * Checks if there has been successful relationship deterioration cause by border conflcit within the past 3 months
	 * If expiration value has reached zero (0), return all governor loyalty to normal, else, it will remain -10
	 * */
//	private void CheckBorderConflictLoyaltyExpiration(){
//		if(this.hasConflicted){
//			if(this.borderConflictLoyaltyExpiration > 0){
//				this.borderConflictLoyaltyExpiration -= 1;
//			}else{
//				this.HasNotConflicted ();
//			}
//		}
//	}

    #region Trading
    /*
     * Make kingdom attempt to trade to another kingdom.
     * */
    //protected void AttemptToTrade() {
    //    Kingdom targetKingdom = null;
    //    List<Kingdom> friendKingdoms = this.GetKingdomsByRelationship(RELATIONSHIP_STATUS.ALLY);
    //    friendKingdoms.AddRange(this.GetKingdomsByRelationship(RELATIONSHIP_STATUS.FRIEND));
    //    List<HexTile> path = null;

    //    friendKingdoms = friendKingdoms.Where(x => this.discoveredKingdoms.Contains(x)).ToList();
    //    //Check if sourceKingdom is friends or allies with anybody
    //    if (friendKingdoms.Count > 0) {
    //        for (int i = 0; i < friendKingdoms.Count; i++) {
    //            //if present, check if the sourceKingdom has resources that the friend does not
    //            Kingdom otherKingdom = friendKingdoms[i];
    //            RelationshipKingdom relWithOtherKingdom = this.GetRelationshipWithOtherKingdom(otherKingdom);
    //            Trade activeTradeEvent = EventManager.Instance.GetActiveTradeEventBetweenKingdoms(this, otherKingdom);
    //            path = PathGenerator.Instance.GetPath(this.capitalCity.hexTile, otherKingdom.capitalCity.hexTile, PATHFINDING_MODE.NORMAL).ToList();
    //            List<RESOURCE> resourcesSourceKingdomCanOffer = this.GetResourcesOtherKingdomDoesNotHave(otherKingdom);
    //            /*
    //             * There should be no active trade event between the two kingdoms (started by this kingdom), the 2 kingdoms should not be at war, 
    //             * there should be a path from this kingdom's capital city to the otherKingdom's capital city, the otherKingdom should not be part of this kingdom's embargo list
    //             * and this kingdom should have a resource that the otherKingdom does not.
    //             * */
    //            if (activeTradeEvent == null && !relWithOtherKingdom.isAtWar && path != null && !this._embargoList.ContainsKey(otherKingdom) && resourcesSourceKingdomCanOffer.Count > 0) {
    //                targetKingdom = otherKingdom;
    //                break;
    //            }
    //        }
    //    }

    //    //if no friends can be traded to, check warm, neutral or cold kingdoms
    //    if (targetKingdom == null) {
    //        List<Kingdom> otherKingdoms = this.GetKingdomsByRelationship(RELATIONSHIP_STATUS.WARM);
    //        otherKingdoms.AddRange(this.GetKingdomsByRelationship(RELATIONSHIP_STATUS.NEUTRAL));
    //        otherKingdoms.AddRange(this.GetKingdomsByRelationship(RELATIONSHIP_STATUS.COLD));

    //        otherKingdoms = otherKingdoms.Where(x => this.discoveredKingdoms.Contains(x)).ToList();
    //        //check if sourceKingdom has resources that the other kingdom does not 
    //        for (int i = 0; i < otherKingdoms.Count; i++) {
    //            Kingdom otherKingdom = otherKingdoms[i];
    //            RelationshipKingdom relWithOtherKingdom = this.GetRelationshipWithOtherKingdom(otherKingdom);
    //            Trade activeTradeEvent = EventManager.Instance.GetActiveTradeEventBetweenKingdoms(this, otherKingdom);
    //            path = PathGenerator.Instance.GetPath(this.capitalCity.hexTile, otherKingdom.capitalCity.hexTile, PATHFINDING_MODE.NORMAL).ToList();
    //            List<RESOURCE> resourcesSourceKingdomCanOffer = this.GetResourcesOtherKingdomDoesNotHave(otherKingdom);
    //            /*
    //             * There should be no active trade event between the two kingdoms (started by this kingdom), the 2 kingdoms should not be at war, 
    //             * there should be a path from this kingdom's capital city to the otherKingdom's capital city, the otherKingdom should not be part of this kingdom's embargo list
    //             * and this kingdom should have a resource that the otherKingdom does not.
    //             * */
    //            if (activeTradeEvent == null && !relWithOtherKingdom.isAtWar && path != null && !this._embargoList.ContainsKey(otherKingdom) && resourcesSourceKingdomCanOffer.Count > 0) {
    //                targetKingdom = otherKingdom;
    //                break;
    //            }
    //        }
    //    }

    //    if (targetKingdom != null) {
    //        EventCreator.Instance.CreateTradeEvent(this, targetKingdom);
    //    }
    //}

    //internal void AddTradeRoute(TradeRoute tradeRoute) {
    //    this._tradeRoutes.Add(tradeRoute);
    //}

    /*
     * Remove references of the trade routes in this kingdom where
     * otherKingdom is involved in.
     * */
    //internal void RemoveAllTradeRoutesWithOtherKingdom(Kingdom otherKingdom) {
    //    List<TradeRoute> tradeRoutesWithOtherKingdom = this._tradeRoutes.Where(x => x.targetKingdom.id == otherKingdom.id || x.sourceKingdom.id == otherKingdom.id).ToList();
    //    for (int i = 0; i < tradeRoutesWithOtherKingdom.Count; i++) {
    //        TradeRoute tradeRouteToRemove = tradeRoutesWithOtherKingdom[i];
    //        this.RemoveTradeRoute(tradeRouteToRemove);
    //    }
    //    this.UpdateAllCitiesDailyGrowth();
    //}

    internal void AddKingdomToEmbargoList(Kingdom kingdomToAdd, EMBARGO_REASON embargoReason = EMBARGO_REASON.NONE) {
        if (!this._embargoList.ContainsKey(kingdomToAdd)) {
            this._embargoList.Add(kingdomToAdd, embargoReason);
            //Remove all existing trade routes between kingdomToAdd and this Kingdom
            //this.RemoveAllTradeRoutesWithOtherKingdom(kingdomToAdd);
            //kingdomToAdd.RemoveAllTradeRoutesWithOtherKingdom(this);
            kingdomToAdd.AdjustUnrest(UNREST_INCREASE_EMBARGO);
        }
        
    }

    internal void RemoveKingdomFromEmbargoList(Kingdom kingdomToRemove) {
        this._embargoList.Remove(kingdomToRemove);
    }

    /*
     * Function to remove trade routes that are no longer used because this
     * kingdom already has a resource of that type
     * */
    //private void RemoveObsoleteTradeRoutes(RESOURCE obsoleteResource) {
    //    List<TradeRoute> tradeRoutesToRemove = new List<TradeRoute>();
    //    for (int i = 0; i < this._tradeRoutes.Count; i++) {
    //        TradeRoute currTradeRoute = this._tradeRoutes[i];
    //        if (currTradeRoute.resourceBeingTraded == obsoleteResource) {
    //            tradeRoutesToRemove.Add(currTradeRoute);
    //        }
    //    }
    //    for (int i = 0; i < tradeRoutesToRemove.Count; i++) {
    //        TradeRoute tradeRouteToRemove = tradeRoutesToRemove[i];
    //        tradeRouteToRemove.sourceKingdom.RemoveTradeRoute(tradeRouteToRemove);
    //        tradeRouteToRemove.targetKingdom.RemoveTradeRoute(tradeRouteToRemove);
    //        tradeRouteToRemove.sourceKingdom.UpdateAllCitiesDailyGrowth();
    //        tradeRouteToRemove.sourceKingdom.UpdateAllCitiesDailyGrowth();
    //    }
    //}

    //internal void RemoveTradeRoute(TradeRoute tradeRoute) {
    //    this._tradeRoutes.Remove(tradeRoute);
    //}

    /*
     * Check the trade routes this kingdom is supplying to,
     * and whether this kingdom can still supply the trade routes resource
     * */
    //internal void RemoveInvalidTradeRoutes() {
    //    List<TradeRoute> invalidTradeRoutes = new List<TradeRoute>();
    //    for (int i = 0; i < this._tradeRoutes.Count; i++) {
    //        TradeRoute currTradeRoute = this._tradeRoutes[i];
    //        //Check if the current trade route is being supplied by this kingdom, then check if 
    //        //this kingdom still has the resource that is being traded.
    //        if (currTradeRoute.sourceKingdom.id == this.id && 
    //            !this._availableResources.ContainsKey(currTradeRoute.resourceBeingTraded)) {
    //            //remove trade route from both kingdoms trade routes because it is no longer valid
    //            this.RemoveTradeRoute(currTradeRoute);
    //            currTradeRoute.targetKingdom.RemoveTradeRoute(currTradeRoute);
    //            this.UpdateAllCitiesDailyGrowth();
    //            currTradeRoute.targetKingdom.UpdateAllCitiesDailyGrowth();
    //        }
    //    }

    //}
    #endregion

    /*
     * Deacrease the kingdom's unrest by UNREST_DECREASE_PER_MONTH amount every month.
     * */
    protected void DecreaseUnrestEveryMonth() {
        if (GameManager.Instance.days == GameManager.daysInMonth[GameManager.Instance.month]) {
            this.AdjustUnrest(UNREST_DECREASE_PER_MONTH);
        }
    }

    /*
	 * Create a new city obj on the specified hextile.
	 * Then add it to this kingdoms cities.
	 * */
	internal City CreateNewCityOnTileForKingdom(HexTile tile){
		City createdCity = CityGenerator.Instance.CreateNewCity (tile, this);
		this.AddCityToKingdom(createdCity);
		return createdCity;
	}

    /*
     * Add a city to this kingdom.
     * Recompute kingdom type data, available resources and
     * daily growth of all cities. Assign city as capital city
     * if city is first city in kingdom.
     * */
	internal void AddCityToKingdom(City city){
		this._cities.Add (city);
        this.UpdateKingdomTypeData();
        this.UpdateAvailableResources();
        this.UpdateAllCitiesDailyGrowth();
        this.UpdateExpansionRate();
        if (this._cities.Count == 1 && this._cities[0] != null) {
            SetCapitalCity(this._cities[0]);
        }
    }

    /*
     * Remove city from this kingdom.
     * Check if kingdom is dead beacuse of city removal.
     * If not, recompute this kingdom's capital city, kingdom type data, 
     * available resources, and daily growth of remaining cities.
     * */
    internal void RemoveCityFromKingdom(City city) {
		city.rebellion = null;
        this._cities.Remove(city);
        this.CheckIfKingdomIsDead();
        if (!this.isDead) {
            //this.RemoveInvalidTradeRoutes();
            this.UpdateKingdomTypeData();
            this.UpdateAvailableResources();
            this.UpdateAllCitiesDailyGrowth();
            this.UpdateExpansionRate();
			//if (this._cities[0] != null) {
                SetCapitalCity(this._cities[0]);
            //}
        }
        
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

	/*
	 * Get a list of all the citizens in this kingdom.
	 * */
	internal List<Citizen> GetAllCitizensInKingdom(){
		List<Citizen> allCitizens = new List<Citizen>();
		for (int i = 0; i < this.cities.Count; i++) {
			allCitizens.AddRange (this.cities [i].citizens);
		}
		return allCitizens;
	}

	internal void UpdateKingSuccession(){
		List<Citizen> orderedMaleRoyalties = this.successionLine.Where (x => x.gender == GENDER.MALE && x.generation > this.king.generation && x.isDirectDescendant == true).OrderBy(x => x.generation).ThenByDescending(x => x.age).ToList();
		List<Citizen> orderedFemaleRoyalties = this.successionLine.Where (x => x.gender == GENDER.FEMALE && x.generation > this.king.generation && x.isDirectDescendant == true).OrderBy(x => x.generation).ThenByDescending(x => x.age).ToList();
		List<Citizen> orderedBrotherRoyalties = this.successionLine
            .Where (x => x.gender == GENDER.MALE 
            && (x.father != null && this.king.father != null && x.father.id == this.king.father.id)
            && x.id != this.king.id)
            .OrderByDescending(x => x.age).ToList();

        List<Citizen> orderedSisterRoyalties = this.successionLine
            .Where (x => x.gender == GENDER.FEMALE 
            && (x.father != null && this.king.father != null && x.father.id == this.king.father.id) 
            && x.id != this.king.id)
            .OrderByDescending(x => x.age).ToList();

		List<Citizen> orderedRoyalties = orderedMaleRoyalties.Concat (orderedFemaleRoyalties).Concat(orderedBrotherRoyalties).Concat(orderedSisterRoyalties).ToList();

		this.successionLine.Clear ();
		this.successionLine = orderedRoyalties;
	}

	internal void AssignNewKing(Citizen newKing, City city = null){
		if(this.king != null){
			if(this.king.city != null){
				this.king.city.hasKing = false;
			}
		}

		if(newKing == null){
//			KingdomManager.Instance.RemoveRelationshipToOtherKings (this.king);
//			this.king.city.CreateInitialRoyalFamily ();
//			this.king.CreateInitialRelationshipsToKings ();
//			KingdomManager.Instance.AddRelationshipToOtherKings (this.king);

			if(city == null){
                //				Debug.Log("NO MORE SUCCESSOR! CREATING NEW KING IN KINGDOM!" + this.name);
                if (this.king.city.isDead) {
                    Debug.LogError("City of previous king is dead! But still creating king in that dead city");
                }
				newKing = this.king.city.CreateNewKing ();
			} else {
//				Debug.Log("NO MORE SUCCESSOR! CREATING NEW KING ON CITY " + city.name + " IN KINGDOM!" + this.name);
				newKing = city.CreateNewKing ();
			}
			if(newKing == null){
				if(this.king != null){
					if(this.king.city != null){
						this.king.city.hasKing = true;
					}
				}
				return;
			}
		}
        SetCapitalCity(newKing.city);
		newKing.city.hasKing = true;

        //if (newKing.isMarried) {
        //    if (newKing.spouse.city.kingdom.king != null && newKing.spouse.city.kingdom.king.id == newKing.spouse.id) {
        //        AssimilateKingdom(newKing.spouse.city.kingdom);
        //        return;
        //    }
        //}
        if (!newKing.isDirectDescendant){
			//				RoyaltyEventDelegate.TriggerChangeIsDirectDescendant (false);
			Utilities.ChangeDescendantsRecursively (newKing, true);
            if(this.king != null) {
                Utilities.ChangeDescendantsRecursively(this.king, false);
            }
		}
        /*if(newKing.assignedRole != null && newKing.role == ROLE.GENERAL){
			newKing.DetachGeneralFromCitizen ();
		}*/
        //		newKing.role = ROLE.KING;
        Citizen previousKing = this.king;
		
        if (newKing.city.governor.id == newKing.id) {
            newKing.city.AssignNewGovernor();
        }

        //		newKing.isKing = true;
        newKing.isGovernor = false;
//			KingdomManager.Instance.RemoveRelationshipToOtherKings (this.king);
		newKing.history.Add(new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, newKing.name + " became the new King/Queen of " + this.name + ".", HISTORY_IDENTIFIER.NONE));
        //		this.king = newKing;

        //Inherit relationships of previous king, otherwise, create new relationships
        if (previousKing != null) {
            KingdomManager.Instance.InheritRelationshipFromCitizen(previousKing, newKing);
            previousKing.relationshipKings.Clear();
        } else {
            KingdomManager.Instance.RemoveRelationshipToOtherKings(previousKing);
            newKing.CreateInitialRelationshipsToKings();
            KingdomManager.Instance.AddRelationshipToOtherKings(newKing);
        }

        newKing.AssignRole(ROLE.KING);
        this.successionLine.Clear();
		ChangeSuccessionLineRescursively (newKing);
		this.successionLine.AddRange (newKing.GetSiblings());
		UpdateKingSuccession ();
//		this.RetrieveInternationWar();
        //		UIManager.Instance.UpdateKingsGrid();
        //		UIManager.Instance.UpdateKingdomSuccession ();

		this.UpdateAllGovernorsLoyalty ();
		this.UpdateAllRelationshipKings ();
        Debug.Log("Assigned new king: " + newKing.name + " because " + previousKing.name + " died!");
    }



    internal void SuccessionWar(Citizen newKing, List<Citizen> claimants){
//		Debug.Log ("SUCCESSION WAR");

		if(newKing.city.governor.id == newKing.id){
			newKing.city.AssignNewGovernor ();
		}
		if(!newKing.isDirectDescendant){
			Utilities.ChangeDescendantsRecursively (newKing, true);
			Utilities.ChangeDescendantsRecursively (this.king, false);
		}
		/*if(newKing.assignedRole != null && newKing.role == ROLE.GENERAL){
			newKing.DetachGeneralFromCitizen ();
		}*/
//		newKing.role = ROLE.KING;
		newKing.AssignRole(ROLE.KING);
//		newKing.isKing = true;
		newKing.isGovernor = false;
//		KingdomManager.Instance.RemoveRelationshipToOtherKings (this.king);
		newKing.history.Add(new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, newKing.name + " became the new King/Queen of " + this.name + ".", HISTORY_IDENTIFIER.NONE));

//		this.king = newKing;
		this.king.CreateInitialRelationshipsToKings ();
		KingdomManager.Instance.AddRelationshipToOtherKings (this.king);
		this.successionLine.Clear();
		ChangeSuccessionLineRescursively (newKing);
		this.successionLine.AddRange (newKing.GetSiblings());
		UpdateKingSuccession ();
//		this.RetrieveInternationWar();
//		UIManager.Instance.UpdateKingsGrid();
//		UIManager.Instance.UpdateKingdomSuccession ();

		for(int i = 0; i < claimants.Count; i++){
			newKing.AddSuccessionWar (claimants [i]);
//			newKing.campaignManager.CreateCampaign ();

			if(claimants[i].isGovernor){
				claimants [i].supportedCitizen = claimants [i];
			}
			claimants[i].AddSuccessionWar (newKing);
//			claimants[i].campaignManager.CreateCampaign ();
		}

	}
//	internal void DethroneKing(Citizen newKing){
////		RoyaltyEventDelegate.TriggerMassChangeLoyalty(newLord, this.assignedLord);
//
//		if(!newKing.isDirectDescendant){
////			RoyaltyEventDelegate.TriggerChangeIsDirectDescendant (false);
//			Utilities.ChangeDescendantsRecursively (newKing, true);
//			Utilities.ChangeDescendantsRecursively (this.king, false);
//		}
//		this.king = newKing;
//		this.king.CreateInitialRelationshipsToKings ();
//		KingdomManager.Instance.AddRelationshipToOtherKings (this.king);
//		this.successionLine.Clear();
//		ChangeSuccessionLineRescursively (newKing);
//		this.successionLine.AddRange (GetSiblings (newKing));
//		UpdateKingSuccession ();
//	}
	internal void ChangeSuccessionLineRescursively(Citizen royalty){
		if(this.king.id != royalty.id){
			if(!royalty.isDead){
				this.successionLine.Add (royalty);
			}
		}

		for(int i = 0; i < royalty.children.Count; i++){
			if(royalty.children[i] != null){
				this.ChangeSuccessionLineRescursively (royalty.children [i]);
			}
		}
	}
		
	internal RelationshipKingdom GetRelationshipWithOtherKingdom(Kingdom kingdomTarget){
		for (int i = 0; i < this.relationshipsWithOtherKingdoms.Count; i++) {
			if (this.relationshipsWithOtherKingdoms[i].targetKingdom.id == kingdomTarget.id) {
				return this.relationshipsWithOtherKingdoms[i];
			}
		}
		return null;
	}
	internal void AddPretender(Citizen citizen){
		this.pretenders.Add (citizen);
		this.pretenders = this.pretenders.Distinct ().ToList ();
	}
	internal List<Citizen> GetPretenderClaimants(Citizen successor){
		List<Citizen> pretenderClaimants = new List<Citizen> ();
		for(int i = 0; i < this.pretenders.Count; i++){
			if(this.pretenders[i].prestige > successor.prestige){
				pretenderClaimants.Add (this.pretenders [i]);
			}
		}
		return pretenderClaimants;
	}
	internal bool CheckForSpecificWar(Kingdom kingdom){
		for(int i = 0; i < this.relationshipsWithOtherKingdoms.Count; i++){
			if(this.relationshipsWithOtherKingdoms[i].targetKingdom.id == kingdom.id){
				if(this.relationshipsWithOtherKingdoms[i].isAtWar){
					return true;
				}
			}
		}
		return false;
	}
	//internal void AssimilateKingdom(Kingdom newKingdom){
	//	for(int i = 0; i < this.cities.Count; i++){
	//		newKingdom.AddCityToKingdom (this.cities [i]);
	//	}
	//	KingdomManager.Instance.MakeKingdomDead(this);
	//}

	internal void ResetAdjacencyWithOtherKingdoms(){
		for (int i = 0; i < this.relationshipsWithOtherKingdoms.Count; i++) {
			this.relationshipsWithOtherKingdoms[i].ResetAdjacency();
		}
	}

	internal List<Citizen> GetAllCitizensOfType(ROLE role){
		List<Citizen> citizensOfType = new List<Citizen>();
		for (int i = 0; i < this.cities.Count; i++) {
			citizensOfType.AddRange (this.cities [i].GetCitizensWithRole(role));
		}
		return citizensOfType;
	}

	internal List<Kingdom> GetAdjacentKingdoms(){
		List<Kingdom> adjacentKingdoms = new List<Kingdom>();
		for (int i = 0; i < this.relationshipsWithOtherKingdoms.Count; i++) {
			if (relationshipsWithOtherKingdoms[i].isAdjacent) {
				adjacentKingdoms.Add(relationshipsWithOtherKingdoms[i].targetKingdom);
			}
		}
		return adjacentKingdoms;
	}

	internal bool IsKingdomAdjacentTo(Kingdom kingdomToCheck){
		if(this.id != kingdomToCheck.id){
			return this.GetRelationshipWithOtherKingdom(kingdomToCheck).isAdjacent;
		}else{
			return false;
		}
	}

	internal List<HexTile> GetAllHexTilesInKingdom(){
		List<HexTile> tilesOwnedByKingdom = new List<HexTile>();
		for (int i = 0; i < this.cities.Count; i++) {
			tilesOwnedByKingdom.AddRange (this.cities[i].ownedTiles);
		}
		return tilesOwnedByKingdom;
	}

	internal List<Kingdom> GetKingdomsByRelationship(RELATIONSHIP_STATUS[] relationshipStatuses){
		List<Kingdom> kingdomsByRelationship = new List<Kingdom>();
		for (int i = 0; i < this.king.relationshipKings.Count; i++) {
			if (relationshipStatuses.Contains(this.king.relationshipKings[i].lordRelationship)) {
				kingdomsByRelationship.Add(this.king.relationshipKings [i].king.city.kingdom);
			}
		}
		return kingdomsByRelationship;
	}

	internal void AddInternationalWar(Kingdom kingdom){
//		Debug.Log ("INTERNATIONAL WAR");
//		for(int i = 0; i < kingdom.cities.Count; i++){
//			if(!this.intlWarCities.Contains(kingdom.cities[i])){
//				this.intlWarCities.Add(kingdom.cities[i]);
//			}
//		}
//		this.TargetACityToAttack ();
//		for(int i = 0; i < this.cities.Count; i++){
//			if(!this.king.campaignManager.SearchForDefenseWarCities(this.cities[i], WAR_TYPE.INTERNATIONAL)){
//				this.king.campaignManager.defenseWarCities.Add(new CityWar(this.cities[i], false, WAR_TYPE.INTERNATIONAL));
//			}
//			if(this.cities[i].governor.supportedCitizen == null){
//				if(!this.king.campaignManager.SearchForDefenseWarCities(kingdom.cities[i])){
//					this.king.campaignManager.defenseWarCities.Add(new CityWar(kingdom.cities[i], false, WAR_TYPE.INTERNATIONAL));
//				}
//			}else{
//				if(!this.king.SearchForSuccessionWar(this.cities[i].governor.supportedCitizen)){
//					if(!this.king.campaignManager.SearchForDefenseWarCities(kingdom.cities[i])){
//						this.king.campaignManager.defenseWarCities.Add(new CityWar(kingdom.cities[i], false, WAR_TYPE.INTERNATIONAL));
//					}
//				}
//			}
//		}
//		this.king.campaignManager.CreateCampaign ();
	}

	internal void RemoveInternationalWar(Kingdom kingdom){
//		this.intlWarCities.RemoveAll(x => x.kingdom.id == kingdom.id);
//		for(int i = 0; i < this.king.campaignManager.activeCampaigns.Count; i++){
//			if(this.king.campaignManager.activeCampaigns[i].warType == WAR_TYPE.INTERNATIONAL){
//				if(this.king.campaignManager.activeCampaigns[i].targetCity.kingdom.id == kingdom.id){
//					this.king.campaignManager.CampaignDone(this.king.campaignManager.activeCampaigns[i]);
//				}
//			}
//		}
	}

//	internal void PassOnInternationalWar(){
//		this.holderIntlWarCities.Clear();
//		this.holderIntlWarCities.AddRange(this.intlWarCities);
//	}
//	internal void RetrieveInternationWar(){
//		this.intlWarCities.AddRange(this.holderIntlWarCities);
//		this.holderIntlWarCities.Clear();
//	}
//
	internal City SearchForCityById(int id){
		for(int i = 0; i < this.cities.Count; i++){
			if(this.cities[i].id == id){
				return this.cities[i];
			}
		}
		return null;
	}

	internal void ConquerCity(City city, General attacker){
		if (this.id != city.kingdom.id){
			RelationshipKingdom rel = this.GetRelationshipWithOtherKingdom (city.kingdom);
			if(rel != null && rel.war != null){
				rel.war.warPair.isDone = true;
			}

			HexTile hex = city.hexTile;
            //city.KillCity();
            if(this.race != city.kingdom.race) {
                city.KillCity();
            } else {
                city.ConquerCity(this);
            }
            
			//yield return null;
			//City newCity = CreateNewCityOnTileForKingdom(hex);
			//newCity.hp = 100;
			//newCity.CreateInitialFamilies(false);
//			this.AddInternationalWarCity (newCity);
			if (UIManager.Instance.currentlyShowingKingdom.id == city.kingdom.id) {
                city.kingdom.HighlightAllOwnedTilesInKingdom();
			}
			KingdomManager.Instance.CheckWarTriggerMisc (city.kingdom, WAR_TRIGGER.TARGET_GAINED_A_CITY);
			//Adjust unrest because a city of this kingdom was conquered.
			this.AdjustUnrest(UNREST_INCREASE_CONQUER);
		}else{
			if(city is RebelFort){
				city.rebellion.KillFort();
//				HexTile hex = city.hexTile;
//				city.KillCity();
			}else{
				if(city.rebellion != null){
					city.ChangeToCity ();
				}else{
					city.ChangeToRebelFort (attacker.citizen.city.rebellion);
				}
			}

		}

	}
//	internal void AddInternationalWarCity(City newCity){
//		for(int i = 0; i < this.relationshipsWithOtherKingdoms.Count; i++){
//			if(this.relationshipsWithOtherKingdoms[i].isAtWar){
//				if(!this.relationshipsWithOtherKingdoms[i].targetKingdom.intlWarCities.Contains(newCity)){
//					this.relationshipsWithOtherKingdoms [i].targetKingdom.intlWarCities.Add (newCity);
//				}
//			}
//		}
//		if(this.IsKingdomHasWar()){
//			if(!this.king.campaignManager.SearchForDefenseWarCities(newCity, WAR_TYPE.INTERNATIONAL)){
//				this.king.campaignManager.defenseWarCities.Add (new CityWar (newCity, false, WAR_TYPE.INTERNATIONAL));
//			}
//		}

//	}
	internal bool IsKingdomHasWar(){
		for(int i = 0; i < this.relationshipsWithOtherKingdoms.Count; i++){
			if(this.relationshipsWithOtherKingdoms[i].isAtWar){
				return true;
			}
		}
		return false;
	}
	internal void RemoveFromSuccession(Citizen citizen){
		if(citizen != null){
			for(int i = 0; i < this.successionLine.Count; i++){
				if(this.successionLine[i].id == citizen.id){
					this.successionLine.RemoveAt (i);
//					UIManager.Instance.UpdateKingdomSuccession ();
					break;
				}
			}
		}
	}

	internal void AdjustExhaustionToAllRelationship(int amount){
		for (int i = 0; i < this.relationshipsWithOtherKingdoms.Count; i++) {
			this.relationshipsWithOtherKingdoms [i].AdjustExhaustion (amount);
		}
	}

	internal Citizen GetCitizenWithHighestPrestigeInKingdom(){
		Citizen citizenHighest = null;
		for(int i = 0; i < this.cities.Count; i++){
			Citizen citizen = this.cities [i].GetCitizenWithHighestPrestige ();
			if(citizen != null){
				if(citizenHighest == null){
					citizenHighest = citizen;
				}else{
					if(citizen.prestige > citizenHighest.prestige){
						citizenHighest = citizen;
					}
				}

			}
		}
		return citizenHighest;
	}

	internal void HighlightAllOwnedTilesInKingdom(){
		for (int i = 0; i < this.cities.Count; i++) {
			if (UIManager.Instance.currentlyShowingCity != null && UIManager.Instance.currentlyShowingCity.id == this.cities [i].id) {
				continue;
			}
			this.cities [i].HighlightAllOwnedTiles (127.5f / 255f);
		}
	}

	internal void UnHighlightAllOwnedTilesInKingdom(){
		for (int i = 0; i < this.cities.Count; i++) {
			if (UIManager.Instance.currentlyShowingCity != null && UIManager.Instance.currentlyShowingCity.id == this.cities [i].id) {
				continue;
			}
			this.cities [i].UnHighlightAllOwnedTiles ();
		}
	}

	internal void UpdateKingdomAdjacency(){
		this.adjacentKingdoms.Clear();
		this.adjacentCitiesFromOtherKingdoms.Clear ();
		for (int i = 0; i < this.cities.Count; i++) {
			List<City> adjacentCitiesOfCurrentCity = this.cities[i].adjacentCities;
			for (int j = 0; j < adjacentCitiesOfCurrentCity.Count; j++) {
				City currAdjacentCity = adjacentCitiesOfCurrentCity [j];
				if (adjacentCitiesOfCurrentCity[j].kingdom.id != this.id) {
					if (!this.adjacentKingdoms.Contains (currAdjacentCity.kingdom)) {
						this.adjacentKingdoms.Add (currAdjacentCity.kingdom);
					}
					if (!currAdjacentCity.kingdom.adjacentKingdoms.Contains(this)) {
						currAdjacentCity.kingdom.adjacentKingdoms.Add(this);
					}
					if (!this.adjacentCitiesFromOtherKingdoms.Contains(currAdjacentCity)) {
						this.adjacentCitiesFromOtherKingdoms.Add(currAdjacentCity);
					}
					if (!currAdjacentCity.kingdom.adjacentCitiesFromOtherKingdoms.Contains(this.cities[i])) {
						currAdjacentCity.kingdom.adjacentCitiesFromOtherKingdoms.Add(this.cities[i]);
					}

				}
			}
		}
	}

	protected List<HexTile> GetAllKingdomBorderTiles(){
		List<HexTile> allBorderTiles = new List<HexTile>();
		for (int i = 0; i < this.cities.Count; i++) {
			allBorderTiles.AddRange (this.cities [i].borderTiles);
		}
		return allBorderTiles;
	}

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

	/*internal int GetAllArmyHp(){
		int total = 0;
		List<Citizen> allGenerals = this.GetAllCitizensOfType (ROLE.GENERAL);
		for(int i = 0; i < allGenerals.Count; i++){
			if(allGenerals[i] is General){
				total += ((General)allGenerals [i].assignedRole).GetArmyHP ();
			}
		}
		return total;
	}*/

	internal int GetAllCityHp(){
		int total = 0;
		for(int i = 0; i < this.cities.Count; i++){
			total += this.cities[i].hp;
		}
		return total;
	}
	internal int GetWarCount(){
		int total = 0;
		for (int i = 0; i < relationshipsWithOtherKingdoms.Count; i++) {
			if(relationshipsWithOtherKingdoms[i].isAtWar){
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
	internal void TargetACityToAttack(){
//		List<City> allHostileCities = new List<City> ();
//		allHostileCities.AddRange (this.intlWarCities);
//		for(int i = 0; i < this.rebellions.Count; i++){
//			allHostileCities.AddRange (this.rebellions [i].conqueredCities);
//		}
//		if(allHostileCities.Count > 0 && this.activeCitiesPairInWar.Count <= 0){
//			City sourceCity = null;
//			City targetCity = null;
//			GetTargetCityAndSourceCityInWar (ref sourceCity, ref targetCity, allHostileCities);
//			if(sourceCity != null && targetCity != null){
//				this.activeCitiesPairInWar.Add (new CityWarPair (sourceCity, targetCity));
//				this.intlWarCities.Remove (targetCity);
//				targetCity.isUnderAttack = true;
//			}
//		}
//		if(this.intlWarCities.Count > 0 && this.activeCitiesToAttack.Count <= 0){
//			int nearestDistance = 0f;
//			City nearestSourceCity = null;
//			City nearestTargetCity = null;
//			for(int i = 0; i < this.intlWarCities.Count; i++){
//				float min = this.cities.Min (x => x.hexTile.GetDistanceTo (this.intlWarCities [i].hexTile));
//			}
//		}
	}

	private void GetTargetCityAndSourceCityInWar(ref City sourceCity, ref City targetCity, List<City> allHostileCities){
		int nearestDistance = 0;
		City source = null;
		City target = null;

		for (int i = 0; i < this.cities.Count; i++) {
			for (int j = 0; j < allHostileCities.Count; j++) {
				List<HexTile> path = PathGenerator.Instance.GetPath (this.cities [i].hexTile, allHostileCities [j].hexTile, PATHFINDING_MODE.AVATAR);
				if(path != null){
					int distance = path.Count;
					if(source == null && target == null){
						source = this.cities [i];
						target = allHostileCities [j];
						nearestDistance = distance;
					}else{
						if(distance < nearestDistance){
							source = this.cities [i];
							target = allHostileCities [j];
							nearestDistance = distance;
						}
					}
				}

			}
		}

		sourceCity = source;
		targetCity = target;
	}
	internal City GetSenderCityForReinforcement(){
		List<City> candidatesForReinforcement = this.cities.Where (x => !x.isUnderAttack && x.hp >= 100).ToList ();
		if(candidatesForReinforcement != null && candidatesForReinforcement.Count > 0){
			candidatesForReinforcement = candidatesForReinforcement.OrderByDescending(x => x.hp).ToList();
			return candidatesForReinforcement [0];
		}
		return null;
	}

	internal City GetReceiverCityForReinforcement(){
		List<City> candidatesForReinforcement = this.cities.Where (x => x.isUnderAttack).ToList ();
		if(candidatesForReinforcement != null && candidatesForReinforcement.Count > 0){
			return candidatesForReinforcement [UnityEngine.Random.Range (0, candidatesForReinforcement.Count)];
		}
		return null;
	}
	#region Resource Management
	/*
	 * Function to adjust the gold count of this kingdom.
	 * pass a negative value to reduce and a positive value
	 * to inrease
	 * */
	internal void AdjustGold(int goldAmount){
		this._goldCount += goldAmount;
		this._goldCount = Mathf.Clamp(this._goldCount, 0, this._maxGold);
	}

	/*
	 * Adjusts resource count. Only reduces gold for now. edit for other
	 * resources of necessary
	 * */
	internal void AdjustResources(List<Resource> resource, bool reduce = true){
		int currentResourceQuantity = 0;
		for(int i = 0; i < resource.Count; i++){
			Resource currResource = resource[i];
			if (currResource.resourceType == BASE_RESOURCE_TYPE.GOLD) {
				currentResourceQuantity = currResource.resourceQuantity;
				if (reduce) {
					currentResourceQuantity *= -1;
				}
				AdjustGold(currentResourceQuantity);
			}
		}
	}

	/*
	 * Add resource type to this kingdoms
	 * available resource (DO NOT ADD GOLD TO THIS!).
	 * */
	internal void AddResourceToKingdom(RESOURCE resource){
		RESOURCE_BENEFITS resourceBenefit = Utilities.resourceBenefits[resource].Keys.FirstOrDefault();

        if (!this._availableResources.ContainsKey(resource)) {
			this._availableResources.Add(resource, 0);
            //this.RemoveObsoleteTradeRoutes(resource);
            if(resourceBenefit == RESOURCE_BENEFITS.GROWTH_RATE) {
                this.UpdateAllCitiesDailyGrowth();
            } else if (resourceBenefit == RESOURCE_BENEFITS.TECH_LEVEL) {
                this.UpdateTechLevel();
            }
        }
		this._availableResources[resource] += 1;
        if (resourceBenefit == RESOURCE_BENEFITS.EXPANSION_RATE) {
            this.UpdateExpansionRate();
        }
    }

    internal void UpdateExpansionRate() {
        this.expansionChance = this.kingdomTypeData.expansionRate;

        for (int i = 0; i < this.availableResources.Keys.Count; i++) {
            RESOURCE currResource = this.availableResources.Keys.ElementAt(i);
            if (Utilities.GetBaseResourceType(currResource) == this.basicResource) {
                int multiplier = this.availableResources[currResource];
				RESOURCE_BENEFITS resourceBenefit = Utilities.resourceBenefits[currResource].Keys.FirstOrDefault();
                float expansionRateGained = Utilities.resourceBenefits[currResource][resourceBenefit];
                if (resourceBenefit == RESOURCE_BENEFITS.EXPANSION_RATE) {
                    this.expansionChance += expansionRateGained * multiplier;
                }
            }
        }
    }

    internal void UpdateTechLevel() {
        this._techLevel = 1;
        List<RESOURCE> allAvailableResources = this._availableResources.Keys.ToList();

        for (int i = 0; i < allAvailableResources.Count; i++) {
            RESOURCE currResource = allAvailableResources[i];
			RESOURCE_BENEFITS resourceBenefit = Utilities.resourceBenefits[currResource].Keys.FirstOrDefault();
            if (resourceBenefit == RESOURCE_BENEFITS.TECH_LEVEL) {
                this._techLevel += (int)Utilities.resourceBenefits[currResource][resourceBenefit];
            }
        }
    }

    internal void UpdateAllCitiesDailyGrowth() {
        //get all resources from tiles and trade routes, only include trade routes where this kingom is the target
        List<RESOURCE> allAvailableResources = this._availableResources.Keys.ToList();
        int dailyGrowthGained = this.ComputeDailyGrowthGainedFromResources(allAvailableResources);
        for (int i = 0; i < this.cities.Count; i++) {
            City currCity = this.cities[i];
            currCity.UpdateDailyGrowthBasedOnSpecialResources(dailyGrowthGained);
        }
    }

    private int ComputeDailyGrowthGainedFromResources(List<RESOURCE> allAvailableResources) {
        int dailyGrowthGained = 0;
        for (int i = 0; i < allAvailableResources.Count; i++) {
            RESOURCE currentResource = allAvailableResources[i];
			RESOURCE_BENEFITS resourceBenefit = Utilities.resourceBenefits[currentResource].Keys.FirstOrDefault();
            if(resourceBenefit == RESOURCE_BENEFITS.GROWTH_RATE) {
                dailyGrowthGained += (int)Utilities.resourceBenefits[currentResource][resourceBenefit];
            }
        }
        return dailyGrowthGained;
    }

	/*
	 * Check if the kingdom has enough resources for a given cost.
	 * */
	internal bool HasEnoughResourcesForAction(List<Resource> resourceCost){
        return true;
		//if(resourceCost != null){
		//	for (int i = 0; i < resourceCost.Count; i++) {
		//		Resource currentResource = resourceCost [i];
		//		if (currentResource.resourceType == BASE_RESOURCE_TYPE.GOLD) {
		//			if (this._goldCount < currentResource.resourceQuantity) {
		//				return false;
		//			}
		//		} 
  //  //            else {
		//		//	if (!this.HasResource(currentResource.resourceType)) {
		//		//		return false;
		//		//	}
		//		//}
		//	}
		//}else{
		//	return false;
		//}
		//return true;
	}

	/*
	 * Check if kingdom is producing a resource of type.
	 * Excluding Gold.
	 * */
	internal bool HasResource(RESOURCE resourceType){
		if (this._availableResources.ContainsKey(resourceType)) {
			return true;
		}
		return false;
	}

	/*
	 * Check if this kingdom has enough gold to create role.
	 * */
	internal bool CanCreateAgent(ROLE roleToCheck, ref int costToCreate){
//		costToCreate = 0;
		if (roleToCheck == ROLE.GENERAL) {
			costToCreate = 0;
		} else if (roleToCheck == ROLE.TRADER) {
			costToCreate = 300;
		} else if (roleToCheck == ROLE.ENVOY) {
			costToCreate = 200;
		} else if (roleToCheck == ROLE.SPY) {
			costToCreate = 200;
		} else if (roleToCheck == ROLE.TRADER) {
			costToCreate = 300;
		} else if (roleToCheck == ROLE.RAIDER) {
			costToCreate = 100;
		} else if (roleToCheck == ROLE.REINFORCER) {
			costToCreate = 0;
		} else if (roleToCheck == ROLE.REBEL) {
			costToCreate = 0;
		}

		if (this._goldCount < costToCreate) {
			return false;
		}
		return true;
	}

    /*
     * Gets a list of resources that otherKingdom does not have access to (By self or by trade).
     * Will compare to this kingdoms available resources (excl. resources from trade)
     * */
    internal List<RESOURCE> GetResourcesOtherKingdomDoesNotHave(Kingdom otherKingdom) {
        List<RESOURCE> resourcesOtherKingdomDoesNotHave = new List<RESOURCE>();
        List<RESOURCE> allAvailableResourcesOfOtherKingdom = otherKingdom.availableResources.Keys.ToList();
        for (int i = 0; i < this._availableResources.Keys.Count; i++) {
            RESOURCE currKey = this._availableResources.Keys.ElementAt(i);
            if (!allAvailableResourcesOfOtherKingdom.Contains(currKey)) {
                //otherKingdom does not have that resource
                resourcesOtherKingdomDoesNotHave.Add(currKey);
            }
        }
        return resourcesOtherKingdomDoesNotHave;
    }

    internal void UpdateAvailableResources() {
        this._availableResources.Clear();
        for (int i = 0; i < this.cities.Count; i++) {
            City currCity = this.cities[i];
            for (int j = 0; j < currCity.structures.Count; j++) {
                HexTile currHexTile = currCity.structures[j];
                if (currHexTile.specialResource != RESOURCE.NONE) {
                    this.AddResourceToKingdom(currHexTile.specialResource);
                }
            }
        }
    }
	#endregion

	#region Unrest
    internal void AdjustUnrest(int amountToAdjust) {
        this._unrest += amountToAdjust;
        this._unrest = Mathf.Clamp(this._unrest, 0, 100);
		if(this._unrest == 100){
			UnrestEvents ();
		}
    }
	internal void ChangeUnrest(int newAmount){
		this._unrest = newAmount;
		this._unrest = Mathf.Clamp(this._unrest, 0, 100);
		if(this._unrest == 100){
			UnrestEvents ();
		}
	}
	internal void UnrestEvents(){
		this._unrest = 0;
		int chance = UnityEngine.Random.Range (0, 2);
		if(chance == 0){
			//Riot Event
			if(!this._hasRiot){
				EventCreator.Instance.CreateRiotEvent(this);
			}else{
				List<GameEvent> riots = EventManager.Instance.GetEventsOfType (EVENT_TYPES.RIOT);
				if(riots != null && riots.Count > 0){
					for (int i = 0; i < riots.Count; i++) {
						Riot riot = (Riot)riots [i];
						if(riot.sourceKingdom.id == this.id && riot.isActive){
							riot.remainingDays += riot.durationInDays;
							break;
						}
					}
				}
			}
		}else{
			//Rebellion Event
			EventCreator.Instance.CreateRebellionEvent(this);
		}

//		Citizen chosenGovernor = null;
//		List<Citizen> ambitiousGovernors = this.cities.Select (x => x.governor).Where (x => x != null && x.hasTrait (TRAIT.AMBITIOUS) && ((Governor)x.assignedRole).loyalty < 0).ToList ();
//		if(ambitiousGovernors != null && ambitiousGovernors.Count > 0){
//			chosenGovernor = ambitiousGovernors [UnityEngine.Random.Range (0, ambitiousGovernors.Count)];
//		}
//		if(chosenGovernor != null){
//			//Secession Event
//			EventCreator.Instance.CreateSecessionEvent(chosenGovernor);
//		}else{
//			int chance = UnityEngine.Random.Range (0, 2);
//			if(chance == 0){
//				//Riot Event
//				EventCreator.Instance.CreateRiotEvent(this);
//			}else{
//				//Rebellion Event
//				EventCreator.Instance.CreateRebellionEvent(this);
//			}
//		}
	}
    #endregion

    #region Tech
    private void IncreaseTechCounterPerTick(){
		if(!this._isTechProducing){
			return;
		}
		int amount = 1 * this.cities.Count;
		int bonus = 0;
        for (int i = 0; i < this._availableResources.Count; i++) {
            RESOURCE currResource = this._availableResources.Keys.ElementAt(i);
			RESOURCE_BENEFITS resourceBenefit = Utilities.resourceBenefits[currResource].Keys.FirstOrDefault();
            if(resourceBenefit == RESOURCE_BENEFITS.TECH_LEVEL) {
                bonus += (int)Utilities.resourceBenefits[currResource][resourceBenefit];
            }
        }
		amount += bonus;
		amount = (int)(amount * this._techProductionPercentage);
		this.AdjustTechCounter (amount);
	}
	private void UpdateTechCapacity(){
		this._techCapacity = 2000 * this._techLevel;
	}

	internal void AdjustTechCounter(int amount){
		this._techCounter += amount;
		this._techCounter = Mathf.Clamp(this._techCounter, 0, this._techCapacity);
		if(this._techCounter == this._techCapacity){
			this.UpgradeTechLevel (1);
		}
	}
	internal void UpgradeTechLevel(int amount){
		this._techLevel += amount;
		if(this._techLevel < 1){
			this._techLevel = 1;
		}
		this._techCounter = 0;
		this.UpdateTechCapacity ();
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
                List<HexTile> neighbours = tilesToCheck[j].AllNeighbours.ToList();
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
            List<HexTile> neighbours = tilesToCheck[i].AllNeighbours.ToList();
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
                if (discoveredKingdom.plague != null) {
                    discoveredKingdom.plague.ForceUpdateKingRelationships(discoveredKingdom.king);
                }
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
	private void UpdateCharacterValuesOfKingsAndGovernors(){
		if(this.king != null){
			this.king.UpdateCharacterValues ();
		}
		for(int i = 0; i < this.cities.Count; i++){
			if(this.cities[i].governor != null){
				this.cities [i].governor.UpdateCharacterValues ();
			}
		}
	}

    internal void GenerateKingdomCharacterValues() {
        this._dictCharacterValues.Clear();
        this._dictCharacterValues = System.Enum.GetValues(typeof(CHARACTER_VALUE)).Cast<CHARACTER_VALUE>().ToDictionary(x => x, x => UnityEngine.Random.Range(1, 101));
        UpdateKingdomCharacterValues();
    }

    internal void UpdateKingdomCharacterValues() {
        this._importantCharacterValues = this._dictCharacterValues.Where(x => x.Value >= 50).OrderByDescending(x => x.Value).Take(4).ToDictionary(x => x.Key, x => x.Value);
    }

    private void UpdateSpecificCharacterValue(CHARACTER_VALUE key, int value) {
        if (this._dictCharacterValues.ContainsKey(key)) {
            this._dictCharacterValues[key] += value;
            //			UpdateCharacterValueByKey(key, value);
        }
    }
    #endregion

    #region Bioweapon
    internal void SetBioWeapon(bool state){
		this._hasBioWeapon = state;
	}
	#endregion

	#region Boon Of Power
	internal void CollectBoonOfPower(BoonOfPower boonOfPower){
		Debug.Log (this.name + " HAS COLLECTED A BOON OF POWER!");
		this._boonOfPowers.Add (boonOfPower);
		boonOfPower.AddOwnership (this);
	}
	internal void DestroyBoonOfPower(BoonOfPower boonOfPower){
		this._activatedBoonOfPowers.Remove (boonOfPower);
	}
	internal void ActivateBoonOfPowers(){
		for (int i = 0; i < this._boonOfPowers.Count; i++) {
			this._boonOfPowers [i].Activate ();
			this._activatedBoonOfPowers.Add (this._boonOfPowers [i]);
		}
		this._boonOfPowers.Clear ();
	}
	#endregion

	#region First And Keystone
	internal void CollectKeystone(){
		Debug.Log (this.name + " HAS COLLECTED A KEYSTONE!");
		GameEvent gameEvent = WorldEventManager.Instance.SearchEventOfType(EVENT_TYPES.FIRST_AND_KEYSTONE);
		if(gameEvent != null){
			((FirstAndKeystone)gameEvent).ChangeKeystoneOwnership (this);
		}
	}
	internal void CollectFirst(){
		Debug.Log (this.name + " HAS COLLECTED THE FIRST!");
		GameEvent gameEvent = WorldEventManager.Instance.SearchEventOfType(EVENT_TYPES.FIRST_AND_KEYSTONE);
		if(gameEvent != null){
			((FirstAndKeystone)gameEvent).ChangeFirstOwnership (this);
		}
	}
	#endregion

	internal bool HasWar(){
		for(int i = 0; i < this.relationshipsWithOtherKingdoms.Count; i++){
			if(this.relationshipsWithOtherKingdoms[i].isAtWar){
				return true;
			}
		}
		return false;
	}

	internal Citizen GetRandomGovernorFromKingdom(){
		City randomCity = this.cities [UnityEngine.Random.Range (0, this.cities.Count)];
		return randomCity.governor;
	}

	#region Slaves Merchant
	private void TriggerSlavesMerchant(){
		if(GameManager.Instance.days == 20){
			int chance = UnityEngine.Random.Range(0,100);
			if(chance < 8){
				EventCreator.Instance.CreateSlavesMerchantEvent(this.king);
			}
		}
	}
    #endregion

    #region Hypnotism
    private void TriggerHypnotism() {
        if (this.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.INFLUENCE)) {
            if (GameManager.Instance.days == GameManager.daysInMonth[GameManager.Instance.month]) {
                List<GameEvent> previousHypnotismEvents = EventManager.Instance.GetEventsStartedByKingdom(this, new EVENT_TYPES[] { EVENT_TYPES.HYPNOTISM }, false);
                if (previousHypnotismEvents.Where(x => x.startYear == GameManager.Instance.year).Count() <= 0) {
                    List<Kingdom> notFriends = new List<Kingdom>();
                    for (int i = 0; i < discoveredKingdoms.Count; i++) {
                        Kingdom currKingdom = discoveredKingdoms[i];
                        RelationshipKings rel = currKingdom.king.GetRelationshipWithCitizen(this.king);
                        if (rel.lordRelationship != RELATIONSHIP_STATUS.FRIEND && rel.lordRelationship != RELATIONSHIP_STATUS.ALLY) {
                            notFriends.Add(currKingdom);
                        }
                    }
                    if (UnityEngine.Random.Range(0, 100) < 10 && notFriends.Count > 0) {
                        EventCreator.Instance.CreateHypnotismEvent(this, notFriends[UnityEngine.Random.Range(0, notFriends.Count)]);
                    }
                }
            }
        }
    }
    #endregion

    #region Kingdom Holiday
    private void TriggerKingdomHoliday() {
        if (this.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.TRADITION)) {
            if (Utilities.IsCurrentDayMultipleOf(15)) {
                List<GameEvent> activeHolidays = EventManager.Instance.GetEventsStartedByKingdom(this, new EVENT_TYPES[] { EVENT_TYPES.KINGDOM_HOLIDAY });
                List<GameEvent> activeWars = EventManager.Instance.GetAllEventsKingdomIsInvolvedIn(this, new EVENT_TYPES[] { EVENT_TYPES.KINGDOM_WAR });
                if(activeHolidays.Count <= 0 && activeWars.Count <= 0) { //There can only be 1 active holiday per kingdom at a time. && Kingdoms that are at war, cannot celebrate holidays.
                    if (UnityEngine.Random.Range(0, 100) < 10) {
                        if(UnityEngine.Random.Range(0, 100) < 50) {
                            //Celebrate Holiday
                            EventCreator.Instance.CreateKingdomHolidayEvent(this);
                        } else {
                            //If a king chooses not to celebrate the holiday, his governors that value TRADITION will decrease loyalty by 20.
                            for (int i = 0; i < cities.Count; i++) {
                                Governor currGovernor = (Governor)cities[i].governor.assignedRole;
                                if (currGovernor.citizen.importantCharacterValues.ContainsKey(CHARACTER_VALUE.TRADITION)) {
									currGovernor.AddEventModifier(-5, "Did not celebrate holiday", null);
                                }
                            }
                            if (_importantCharacterValues.ContainsKey(CHARACTER_VALUE.TRADITION)) {
                                AdjustUnrest(10);
                            }
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region Develop Weapons
    private int _weaponsCount;
    public int weaponsCount {
        get { return _weaponsCount; }
    }
    internal void AdjustWeaponsCount(int adjustment) {
        _weaponsCount += adjustment;
    }
    //protected void TriggerDevelopWeapons() {
    //    if (this.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.STRENGTH)) {
    //        if (Utilities.IsCurrentDayMultipleOf(5)) {
    //            if (UnityEngine.Random.Range(0, 100) < 10) {
    //                if (EventManager.Instance.GetEventsStartedByKingdom(this, new EVENT_TYPES[] { EVENT_TYPES.DEVELOP_WEAPONS }).Count <= 0) {
    //                    //EventCreator.Instance.CreateDevelopWeaponsEvent(this);
    //                }
    //            }
    //        }
    //    }
    //}
    #endregion

    #region Kings Council
    protected void TriggerKingsCouncil() {
        if(this.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.LIBERTY) || this.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.PEACE)) {
            if (GameManager.Instance.days == GameManager.daysInMonth[GameManager.Instance.month]) {
                if (UnityEngine.Random.Range(0, 100) < 2) {
                    if (discoveredKingdoms.Count > 2 && EventManager.Instance.GetEventsStartedByKingdom(this, new EVENT_TYPES[] { EVENT_TYPES.KINGDOM_WAR, EVENT_TYPES.KINGS_COUNCIL }).Count <= 0) {
                        EventCreator.Instance.CreateKingsCouncilEvent(this);
                    }
                }
            }
        }
    }
    #endregion

	#region Serum of Alacrity
	internal void AdjustSerumOfAlacrity(int amount){
		this._serumsOfAlacrity += amount;
		if(this._serumsOfAlacrity < 0){
			this._serumsOfAlacrity = 0;
		}
	}
	#endregion

	#region Hidden History Book
	internal void SetUpheldHiddenHistoryBook(bool state){
		this._hasUpheldHiddenHistoryBook = state;
	}
	#endregion

    #region Fog Of War
    internal void SetFogOfWarStateForTile(HexTile tile, FOG_OF_WAR_STATE fowState, bool isForcedUpdate = false) {
        FOG_OF_WAR_STATE previousStateOfTile = tile.currFogOfWarState;
        fogOfWarDict[previousStateOfTile].Remove(tile);

        _fogOfWar[tile.xCoordinate, tile.yCoordinate] = fowState;
        fogOfWarDict[fowState].Add(tile);

        if (UIManager.Instance.currentlyShowingKingdom != null && UIManager.Instance.currentlyShowingKingdom.id == this.id) {
            UpdateFogOfWarVisualForTile(tile, fowState);
        }

        //if (isForcedUpdate) {
        //    if (UIManager.Instance.currentlyShowingKingdom != null && UIManager.Instance.currentlyShowingKingdom.id == this.id) {
        //        UpdateFogOfWarVisualForTile(tile, fowState);
        //    }
        //} 
        //else {
        //    if (fowState == FOG_OF_WAR_STATE.VISIBLE) {
        //        _fogOfWar[tile.xCoordinate, tile.yCoordinate] = fowState;
        //        fogOfWarDict[fowState].Add(tile);
        //        if (UIManager.Instance.currentlyShowingKingdom != null && UIManager.Instance.currentlyShowingKingdom.id == this.id) {
        //            UpdateFogOfWarVisualForTile(tile, fowState);
        //        }
        //        //			if(tile.lair != null){
        //        //				tile.lair.ActivateLair ();
        //        //			}
        //    } else {
        //        //if (!(tile.isVisibleByCities != null && cities.Intersect(tile.isVisibleByCities).Count() > 0)) {
        //            if (_fogOfWar[tile.xCoordinate, tile.yCoordinate] != FOG_OF_WAR_STATE.SEEN) {
        //                _fogOfWar[tile.xCoordinate, tile.yCoordinate] = fowState;
        //                fogOfWarDict[fowState].Add(tile);
        //                if (UIManager.Instance.currentlyShowingKingdom != null && UIManager.Instance.currentlyShowingKingdom.id == this.id) {
        //                    UpdateFogOfWarVisualForTile(tile, fowState);
        //                }
        //            }
        //        //}
        //    }
        //}
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
        if (KingdomManager.Instance.useFogOfWar) {
            hexTile.ShowFogOfWarObjects();
        } else {
            hexTile.HideFogOfWarObjects();
        }
    }

	internal FOG_OF_WAR_STATE GetFogOfWarStateOfTile(HexTile hexTile){
		return this._fogOfWar [hexTile.xCoordinate, hexTile.yCoordinate];
	}
    #endregion

	#region Altar of Blessing
	internal void CollectAltarOfBlessing(BoonOfPower boonOfPower){
		Debug.Log (this.name + " HAS COLLECTED A BOON OF POWER!");
		this._boonOfPowers.Add (boonOfPower);
		boonOfPower.AddOwnership (this);
	}
	#endregion
	internal int GetNumberOfWars(){
		int numOfWars = 0;
		for (int i = 0; i < this.relationshipsWithOtherKingdoms.Count; i++) {
			if(this.relationshipsWithOtherKingdoms[i].isAtWar){
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
	internal void SetRiot(bool state){
		this._hasRiot = state;
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
		if(GameManager.Instance.month == this._crimeDate.month && GameManager.Instance.days == this._crimeDate.day){
			NewRandomCrimeDate ();
			CreateCrime ();
		}
	}

	private void CreateCrime(){
		CrimeData crimeData = CrimeEvents.Instance.GetRandomCrime ();
		EventCreator.Instance.CreateCrimeEvent (this, crimeData);
	}
	#endregion

	internal void AddActiveEvent(GameEvent gameEvent){
		this.activeEvents.Add (gameEvent);
	}
	internal void RemoveActiveEvent(GameEvent gameEvent){
		this.activeEvents.Remove (gameEvent);
		AddToDoneEvents (gameEvent);
	}
	internal void AddToDoneEvents(GameEvent gameEvent){
		this.doneEvents.Add (gameEvent);
		if(this.doneEvents.Count > KingdomManager.Instance.maxKingdomEventHistory){
			this.doneEvents.RemoveAt (0);
		}
	}
	internal bool HasActiveEvent(EVENT_TYPES eventType){
		for (int i = 0; i < this.activeEvents.Count; i++) {
			if(this.activeEvents[i].eventType == eventType){
				return true;
			}
		}
		return false;
	}
	internal int GetActiveEventsOfTypeCount(EVENT_TYPES eventType){
		int count = 0;
		for (int i = 0; i < this.activeEvents.Count; i++) {
			if(this.activeEvents[i].eventType == eventType){
				count += 1;
			}
		}
		return count;
	}
//	internal bool HasActiveEventWith(EVENT_TYPES eventType, Kingdom kingdom){
//		for (int i = 0; i < this.activeEvents.Count; i++) {
//			if(this.activeEvents[i].eventType == eventType){
//				return true;
//			}
//		}
//		return false;
//	}


	#region Governors Loyalty/Opinion

	internal void HasConflicted(GameEvent gameEvent){
		for(int i = 0; i < this.cities.Count; i++){
			if(this.cities[i].governor != null){
				((Governor)this.cities[i].governor.assignedRole).AddEventModifier (-10, "Recent border conflict", gameEvent);
			}
		}
	}

	internal void UpdateAllGovernorsLoyalty(){
		for(int i = 0; i < this.cities.Count; i++){
			if(this.cities[i].governor != null){
				((Governor)this.cities[i].governor.assignedRole).UpdateLoyalty();
			}
		}
	}
	#endregion

	internal void UpdateAllRelationshipKings(){
		if(this.king != null){
			for (int i = 0; i < this.king.relationshipKings.Count; i++) {
				RelationshipKings rel = this.king.relationshipKings [i];
				rel.UpdateLikeness (null);
			}
		}
	}

	internal void CheckSharedBorders(){
		bool isSharingBorderNow = false;
		for (int i = 0; i < this.relationshipsWithOtherKingdoms.Count; i++) {
			isSharingBorderNow = KingdomManager.Instance.IsSharingBorders (this, this.relationshipsWithOtherKingdoms [i].targetKingdom);
			if (isSharingBorderNow != this.relationshipsWithOtherKingdoms[i].isSharingBorder) {
				this.relationshipsWithOtherKingdoms [i].SetBorderSharing (isSharingBorderNow);
				RelationshipKingdom rel2 = this.relationshipsWithOtherKingdoms [i].targetKingdom.GetRelationshipWithOtherKingdom (this);
				rel2.SetBorderSharing (isSharingBorderNow);
			}
		}
	}
}
