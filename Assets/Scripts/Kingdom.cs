using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Kingdom{
	public int id;
	public string name;
	public RACE race;
	public int[] horoscope; 

	[SerializeField]
	private KingdomTypeData _kingdomTypeData;
	private Kingdom _sourceKingdom;

    //Resources
    private int _goldCount;
    private int _maxGold = 5000;
    private int _basicResourceCount;
    private Dictionary<RESOURCE, int> _availableResources; //only includes resources that the kingdom has bought via tile purchasing

    //Trading
    private List<TradeRoute> _tradeRoutes;
    private Dictionary<Kingdom, EMBARGO_REASON> _embargoList;

    private int _unrest;

	private List<City> _cities;
	private List<Camp> camps;
	internal City capitalCity;
	internal Citizen king;
	internal List<Citizen> successionLine;
	internal List<Citizen> pretenders;

//	public List<Citizen> royaltyList;
	public List<City> intlWarCities;
	public List<City> activeCitiesToAttack;
	public List<CityWarPair> activeCitiesPairInWar;
	internal List<City> holderIntlWarCities;
	internal List<Rebellion> rebellions;

	internal BASE_RESOURCE_TYPE basicResource;
	internal BASE_RESOURCE_TYPE rareResource;

	internal List<RelationshipKingdom> relationshipsWithOtherKingdoms;

	internal Color kingdomColor;
	internal List<History> kingdomHistory;

	internal List<City> adjacentCitiesFromOtherKingdoms;
	internal List<Kingdom> adjacentKingdoms;

	private List<Kingdom> _discoveredKingdoms;
	
	//Tech
	private int _techLevel;
	private int techCapacity;
	private int techCounter;

	private int expansionChance = 1;
    
    protected const int INCREASE_CITY_HP_CHANCE = 5;
	protected const int INCREASE_CITY_HP_AMOUNT = 20;
    protected const int GOLD_GAINED_FROM_TRADE = 10;
    protected const int UNREST_DECREASE_PER_MONTH = -5;
    protected const int UNREST_INCREASE_CONQUER = 5;
    protected const int UNREST_INCREASE_EMBARGO = 5;

    protected List<Resource> increaseCityHPCost = new List<Resource> () {
		new Resource (BASE_RESOURCE_TYPE.GOLD, 300)
	};

	private bool _isDead;
	internal bool hasConflicted;

	private int borderConflictLoyaltyExpiration;

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
    public List<TradeRoute> tradeRoutes {
        get { return this._tradeRoutes;  }
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
        get { return this._basicResourceCount; }
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
		get{return this._techLevel;}
	}
    public int expansionRate {
        get { return this.expansionChance; }
    }
	#endregion
	// Kingdom constructor paramters
	//	race - the race of this kingdom
	//	cities - the cities that this kingdom will initially own
	//	sourceKingdom (optional) - the kingdom from which this new kingdom came from
	public Kingdom(RACE race, List<HexTile> cities, Kingdom sourceKingdom = null) {
		this.id = Utilities.SetID(this);
		this.race = race;
		this.name = RandomNameGenerator.Instance.GenerateKingdomName(this.race);
		this.king = null;
		this.successionLine = new List<Citizen>();
		this.pretenders = new List<Citizen> ();
		this.intlWarCities = new List<City>();
		this.activeCitiesToAttack = new List<City>();
		this.activeCitiesPairInWar = new List<CityWarPair>();
		this.holderIntlWarCities = new List<City>();
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
        this._tradeRoutes = new List<TradeRoute>();
        this._embargoList = new Dictionary<Kingdom, EMBARGO_REASON>();
        this._unrest = 0;
		this._sourceKingdom = sourceKingdom;
		this.hasConflicted = false;
		this.borderConflictLoyaltyExpiration = 0;
		this.rebellions = new List<Rebellion> ();
		this._discoveredKingdoms = new List<Kingdom>();
		this._techLevel = 1;
		this.techCounter = 0;
		this.UpdateTechCapacity ();
		// Determine what type of Kingdom this will be upon initialization.
		this._kingdomTypeData = null;
		this.UpdateKingdomTypeData();

		if (race == RACE.HUMANS) {
			this.basicResource = BASE_RESOURCE_TYPE.STONE;
			this.rareResource = BASE_RESOURCE_TYPE.MITHRIL;
		} else if (race == RACE.ELVES) {
			this.basicResource = BASE_RESOURCE_TYPE.WOOD;
			this.rareResource = BASE_RESOURCE_TYPE.MANA_STONE;
		} else if (race == RACE.MINGONS) {
			this.basicResource = BASE_RESOURCE_TYPE.WOOD;
			this.rareResource = BASE_RESOURCE_TYPE.NONE;
		} else {
			this.basicResource = BASE_RESOURCE_TYPE.STONE;
			this.rareResource = BASE_RESOURCE_TYPE.COBALT;
		}

        if(cities.Count > 0) {
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
		EventManager.Instance.onCreateNewKingdomEvent.AddListener(CreateNewRelationshipWithKingdom);
		EventManager.Instance.onWeekEnd.AddListener(KingdomTickActions);
		EventManager.Instance.onKingdomDiedEvent.AddListener(OtherKingdomDiedActions);
		this.kingdomHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "This kingdom was born.", HISTORY_IDENTIFIER.NONE));
	}

	// Updates this kingdom's type and horoscope
	public void UpdateKingdomTypeData() {
		// Update Kingdom Type whenever the kingdom expands to a new city
		KingdomTypeData prevKingdomTypeData = this._kingdomTypeData;
		this._kingdomTypeData = StoryTellingManager.Instance.InitializeKingdomType (this);

		// If the Kingdom Type Data changed
		if (_kingdomTypeData != prevKingdomTypeData) {			
			// Update horoscope
			if (prevKingdomTypeData == null) {
				this.horoscope = GetHoroscope ();
			} else {				
				this.horoscope = GetHoroscope (prevKingdomTypeData.kingdomType);
			}
            // Update expansion chance
            this.UpdateExpansionRate();
        }
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
		if (this.cities.Where(x => x.rebellion == null).ToList().Count > 0) {
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
        this.RemoveRelationshipsWithOtherKingdoms();
        KingdomManager.Instance.allKingdoms.Remove(this);
        EventManager.Instance.onCreateNewKingdomEvent.RemoveListener(CreateNewRelationshipWithKingdom);
        EventManager.Instance.onWeekEnd.RemoveListener(KingdomTickActions);
        EventManager.Instance.onKingdomDiedEvent.RemoveListener(OtherKingdomDiedActions);

        EventManager.Instance.onKingdomDiedEvent.Invoke(this);
	}

    private void CancelEventKingdomIsInvolvedIn(EVENT_TYPES eventType) {
        if (eventType == EVENT_TYPES.KINGDOM_WAR) {
            List<GameEvent> wars = EventManager.Instance.GetAllEventsKingdomIsInvolvedIn(this, new EVENT_TYPES[] { EVENT_TYPES.KINGDOM_WAR });
            for (int i = 0; i < wars.Count; i++) {
                wars[i].CancelEvent();
            }
        } else {
            List<GameEvent> allEvents = EventManager.Instance.GetAllEventsKingdomIsInvolvedIn(this, new EVENT_TYPES[] { EVENT_TYPES.ALL });
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
				this.relationshipsWithOtherKingdoms.RemoveAt(i); //remove relationship with kingdom
				break;
			}
		}
	}

    protected void OtherKingdomDiedActions(Kingdom kingdomThatDied) {
        if (kingdomThatDied.id != this.id) {
            RemoveRelationshipWithKingdom(kingdomThatDied);
            RemoveAllTradeRoutesWithOtherKingdom(kingdomThatDied);
            RemoveKingdomFromDiscoveredKingdoms(kingdomThatDied);
        }
    }

	/*
	 * This function is listening to the onWeekEnd Event. Put functions that you want to
	 * happen every tick here.
	 * */
	protected void KingdomTickActions(){
        //this.ProduceGoldFromTrade();
        this.AttemptToExpand();
//		this.AttemptToCreateAttackCityEvent ();
//		this.AttemptToCreateReinforcementEvent ();
        //		this.AttemptToIncreaseCityHP();
        this.DecreaseUnrestEveryMonth();
		this.CheckBorderConflictLoyaltyExpiration ();
		this.IncreaseTechCounterPerTick();
        //if (GameManager.Instance.days == GameManager.daysInMonth[GameManager.Instance.month]) {
        //    this.AttemptToTrade();
        //}
        
    }
	/*
	 * Attempt to an event with agent
	 * This happens everyday
	 * */
	private void AttemptToCreateEvent(){
		
	}
	private void CreatEvent(EVENT_TYPES eventType){
		switch (eventType){
		case EVENT_TYPES.TRADE:
//			this.AttemptToTrade ();
			break;
		case EVENT_TYPES.STATE_VISIT:
//			EventCreator.Instance.CreateStateVisitEvent
			break;
		case EVENT_TYPES.RAID:
			break;
		}
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
		if (EventManager.Instance.GetEventsStartedByKingdom(this, new EVENT_TYPES[]{EVENT_TYPES.EXPANSION}).Where(x => x.isActive).Count() > 0) {
			return;
		}

		int chance = Random.Range (0, 300 + (50 * this.cities.Count));
		if (chance < this.expansionChance) {
		
			List<City> citiesThatCanExpand = new List<City> ();
			List<Citizen> allUnassignedAdultCitizens = new List<Citizen> ();
			List<Resource> expansionCost = new List<Resource> () {
				new Resource (BASE_RESOURCE_TYPE.GOLD, 0)
			};

			if (this.cities.Count > 0) {
				EventCreator.Instance.CreateExpansionEvent (this);
			}

		}
	}

	/*
	 * Attempt to increase 1 city's hp. 
	 * Chance to occur is stored in INCREASE_CITY_HP_CHANCE.
	 * */
	protected void AttemptToIncreaseCityHP(){
		int chance = Random.Range(0, 100);
		if (chance < INCREASE_CITY_HP_CHANCE && this.HasEnoughResourcesForAction(this.increaseCityHPCost)) {
			List<City> citiesElligibleForUpgrade = new List<City>();
			for (int i = 0; i < this.cities.Count; i++) {
				City currCity = this.cities[i];
				if (currCity.hp < currCity.maxHP) {
					citiesElligibleForUpgrade.Add (currCity);
				}
			}

			if (citiesElligibleForUpgrade.Count > 0) {
				citiesElligibleForUpgrade = citiesElligibleForUpgrade.OrderBy (x => x.hp).ToList ();
				int lowestHP = citiesElligibleForUpgrade.First ().hp;
				List<City> citiesWithLowestHP = citiesElligibleForUpgrade.Where (x => x.hp == lowestHP).ToList ();
				City cityToUpgrade = citiesWithLowestHP[Random.Range(0, citiesWithLowestHP.Count)];
				this.AdjustResources(this.increaseCityHPCost);
				cityToUpgrade.IncreaseHP(INCREASE_CITY_HP_AMOUNT);
			}
		}
	}

	/*
	 * Checks if there has been successful relationship deterioration cause by border conflcit within the past 3 months
	 * If expiration value has reached zero (0), return all governor loyalty to normal, else, it will remain -10
	 * */
	private void CheckBorderConflictLoyaltyExpiration(){
		if(this.hasConflicted){
			if(this.borderConflictLoyaltyExpiration > 0){
				this.borderConflictLoyaltyExpiration -= 1;
			}else{
				this.HasNotConflicted ();
			}
		}
	}

    #region Trading
    /*
     * Make kingdom attempt to trade to another kingdom.
     * */
    protected void AttemptToTrade() {
        Kingdom targetKingdom = null;
        List<Kingdom> friendKingdoms = this.GetKingdomsByRelationship(RELATIONSHIP_STATUS.ALLY);
        friendKingdoms.AddRange(this.GetKingdomsByRelationship(RELATIONSHIP_STATUS.FRIEND));
        List<HexTile> path = null;

        friendKingdoms = friendKingdoms.Where(x => this.discoveredKingdoms.Contains(x)).ToList();
        //Check if sourceKingdom is friends or allies with anybody
        if (friendKingdoms.Count > 0) {
            for (int i = 0; i < friendKingdoms.Count; i++) {
                //if present, check if the sourceKingdom has resources that the friend does not
                Kingdom otherKingdom = friendKingdoms[i];
                RelationshipKingdom relWithOtherKingdom = this.GetRelationshipWithOtherKingdom(otherKingdom);
                Trade activeTradeEvent = EventManager.Instance.GetActiveTradeEventBetweenKingdoms(this, otherKingdom);
                path = PathGenerator.Instance.GetPath(this.capitalCity.hexTile, otherKingdom.capitalCity.hexTile, PATHFINDING_MODE.NORMAL).ToList();
                List<RESOURCE> resourcesSourceKingdomCanOffer = this.GetResourcesOtherKingdomDoesNotHave(otherKingdom);
                /*
                 * There should be no active trade event between the two kingdoms (started by this kingdom), the 2 kingdoms should not be at war, 
                 * there should be a path from this kingdom's capital city to the otherKingdom's capital city, the otherKingdom should not be part of this kingdom's embargo list
                 * and this kingdom should have a resource that the otherKingdom does not.
                 * */
                if (activeTradeEvent == null && !relWithOtherKingdom.isAtWar && path != null && !this._embargoList.ContainsKey(otherKingdom) && resourcesSourceKingdomCanOffer.Count > 0) {
                    targetKingdom = otherKingdom;
                    break;
                }
            }
        }

        //if no friends can be traded to, check warm, neutral or cold kingdoms
        if (targetKingdom == null) {
            List<Kingdom> otherKingdoms = this.GetKingdomsByRelationship(RELATIONSHIP_STATUS.WARM);
            otherKingdoms.AddRange(this.GetKingdomsByRelationship(RELATIONSHIP_STATUS.NEUTRAL));
            otherKingdoms.AddRange(this.GetKingdomsByRelationship(RELATIONSHIP_STATUS.COLD));

            otherKingdoms = otherKingdoms.Where(x => this.discoveredKingdoms.Contains(x)).ToList();
            //check if sourceKingdom has resources that the other kingdom does not 
            for (int i = 0; i < otherKingdoms.Count; i++) {
                Kingdom otherKingdom = otherKingdoms[i];
                RelationshipKingdom relWithOtherKingdom = this.GetRelationshipWithOtherKingdom(otherKingdom);
                Trade activeTradeEvent = EventManager.Instance.GetActiveTradeEventBetweenKingdoms(this, otherKingdom);
                path = PathGenerator.Instance.GetPath(this.capitalCity.hexTile, otherKingdom.capitalCity.hexTile, PATHFINDING_MODE.NORMAL).ToList();
                List<RESOURCE> resourcesSourceKingdomCanOffer = this.GetResourcesOtherKingdomDoesNotHave(otherKingdom);
                /*
                 * There should be no active trade event between the two kingdoms (started by this kingdom), the 2 kingdoms should not be at war, 
                 * there should be a path from this kingdom's capital city to the otherKingdom's capital city, the otherKingdom should not be part of this kingdom's embargo list
                 * and this kingdom should have a resource that the otherKingdom does not.
                 * */
                if (activeTradeEvent == null && !relWithOtherKingdom.isAtWar && path != null && !this._embargoList.ContainsKey(otherKingdom) && resourcesSourceKingdomCanOffer.Count > 0) {
                    targetKingdom = otherKingdom;
                    break;
                }
            }
        }

        if (targetKingdom != null) {
            EventCreator.Instance.CreateTradeEvent(this, targetKingdom);
        }
    }

    internal void AddTradeRoute(TradeRoute tradeRoute) {
        this._tradeRoutes.Add(tradeRoute);
    }

    /*
     * Remove references of the trade routes in this kingdom where
     * otherKingdom is involved in.
     * */
    internal void RemoveAllTradeRoutesWithOtherKingdom(Kingdom otherKingdom) {
        List<TradeRoute> tradeRoutesWithOtherKingdom = this._tradeRoutes.Where(x => x.targetKingdom.id == otherKingdom.id || x.sourceKingdom.id == otherKingdom.id).ToList();
        for (int i = 0; i < tradeRoutesWithOtherKingdom.Count; i++) {
            TradeRoute tradeRouteToRemove = tradeRoutesWithOtherKingdom[i];
            this.RemoveTradeRoute(tradeRouteToRemove);
        }
        this.UpdateAllCitiesDailyGrowth();
        this.UpdateBasicResourceCount();
    }

    internal void AddKingdomToEmbargoList(Kingdom kingdomToAdd, EMBARGO_REASON embargoReason = EMBARGO_REASON.NONE) {
        if (!this._embargoList.ContainsKey(kingdomToAdd)) {
            this._embargoList.Add(kingdomToAdd, embargoReason);
            //Remove all existing trade routes between kingdomToAdd and this Kingdom
            this.RemoveAllTradeRoutesWithOtherKingdom(kingdomToAdd);
            kingdomToAdd.RemoveAllTradeRoutesWithOtherKingdom(this);
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
    private void RemoveObsoleteTradeRoutes(RESOURCE obsoleteResource) {
        List<TradeRoute> tradeRoutesToRemove = new List<TradeRoute>();
        for (int i = 0; i < this._tradeRoutes.Count; i++) {
            TradeRoute currTradeRoute = this._tradeRoutes[i];
            if (currTradeRoute.resourceBeingTraded == obsoleteResource) {
                tradeRoutesToRemove.Add(currTradeRoute);
            }
        }
        for (int i = 0; i < tradeRoutesToRemove.Count; i++) {
            TradeRoute tradeRouteToRemove = tradeRoutesToRemove[i];
            tradeRouteToRemove.sourceKingdom.RemoveTradeRoute(tradeRouteToRemove);
            tradeRouteToRemove.targetKingdom.RemoveTradeRoute(tradeRouteToRemove);
            tradeRouteToRemove.sourceKingdom.UpdateAllCitiesDailyGrowth();
            tradeRouteToRemove.sourceKingdom.UpdateAllCitiesDailyGrowth();
        }
    }

    internal void RemoveTradeRoute(TradeRoute tradeRoute) {
        this._tradeRoutes.Remove(tradeRoute);
    }

    /*
     * Check the trade routes this kingdom is supplying to,
     * and whether this kingdom can still supply the trade routes resource
     * */
    internal void RemoveInvalidTradeRoutes() {
        List<TradeRoute> invalidTradeRoutes = new List<TradeRoute>();
        for (int i = 0; i < this._tradeRoutes.Count; i++) {
            TradeRoute currTradeRoute = this._tradeRoutes[i];
            //Check if the current trade route is being supplied by this kingdom, then check if 
            //this kingdom still has the resource that is being traded.
            if (currTradeRoute.sourceKingdom.id == this.id && 
                !this._availableResources.ContainsKey(currTradeRoute.resourceBeingTraded)) {
                //remove trade route from both kingdoms trade routes because it is no longer valid
                this.RemoveTradeRoute(currTradeRoute);
                currTradeRoute.targetKingdom.RemoveTradeRoute(currTradeRoute);
                this.UpdateAllCitiesDailyGrowth();
                currTradeRoute.targetKingdom.UpdateAllCitiesDailyGrowth();
            }
        }

    }
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
        this.UpdateTechLevel();
        if (this._cities.Count == 1 && this._cities[0] != null) {
            this.capitalCity = this._cities[0];

            HexTile habitableTile;
            if (this.basicResource == BASE_RESOURCE_TYPE.STONE) {
                for (int i = 0; i < CityGenerator.Instance.stoneHabitableTiles.Count; i++) {
                    habitableTile = CityGenerator.Instance.stoneHabitableTiles[i];
					this.capitalCity.AddHabitableTileDistance(habitableTile, PathGenerator.Instance.GetDistanceBetweenTwoTiles(this.cities[0].hexTile, habitableTile));
                }

            } else if (this.basicResource == BASE_RESOURCE_TYPE.WOOD) {
                for (int i = 0; i < CityGenerator.Instance.woodHabitableTiles.Count; i++) {
                    habitableTile = CityGenerator.Instance.woodHabitableTiles[i];
					this.capitalCity.AddHabitableTileDistance(habitableTile, PathGenerator.Instance.GetDistanceBetweenTwoTiles(this.cities[0].hexTile, habitableTile));
                }

            }
        }
    }

    /*
     * Remove city from this kingdom.
     * Check if kingdom is dead beacuse of city removal.
     * If not, recompute this kingdom's capital city, kingdom type data, 
     * available resources, and daily growth of remaining cities.
     * */
    internal void RemoveCityFromKingdom(City city) {
        this._cities.Remove(city);
        this.CheckIfKingdomIsDead();
        if (!this.isDead) {
            this.RemoveInvalidTradeRoutes();
            this.UpdateKingdomTypeData();
            this.UpdateAvailableResources();
            this.UpdateAllCitiesDailyGrowth();
            this.UpdateExpansionRate();
            this.UpdateTechLevel();
			if (this._cities[0] != null && this.capitalCity.id == city.id) {
                this.capitalCity = this._cities[0];

				HexTile habitableTile;
				if (this.basicResource == BASE_RESOURCE_TYPE.STONE) {
					for (int i = 0; i < CityGenerator.Instance.stoneHabitableTiles.Count; i++) {
						habitableTile = CityGenerator.Instance.stoneHabitableTiles[i];
						this.capitalCity.AddHabitableTileDistance(habitableTile, PathGenerator.Instance.GetDistanceBetweenTwoTiles(this.cities[0].hexTile, habitableTile));
					}

				} else if (this.basicResource == BASE_RESOURCE_TYPE.WOOD) {
					for (int i = 0; i < CityGenerator.Instance.woodHabitableTiles.Count; i++) {
						habitableTile = CityGenerator.Instance.woodHabitableTiles[i];
						this.capitalCity.AddHabitableTileDistance(habitableTile, PathGenerator.Instance.GetDistanceBetweenTwoTiles(this.cities[0].hexTile, habitableTile));
					}

				}
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
		List<Citizen> orderedBrotherRoyalties = this.successionLine.Where (x => x.gender == GENDER.MALE && x.father.id == this.king.father.id && x.id != this.king.id).OrderByDescending(x => x.age).ToList();
		List<Citizen> orderedSisterRoyalties = this.successionLine.Where (x => x.gender == GENDER.FEMALE && x.father.id == this.king.father.id && x.id != this.king.id).OrderByDescending(x => x.age).ToList();

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
				newKing = this.king.city.CreateNewKing ();
			}else{
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
		this.capitalCity = newKing.city;
		newKing.city.hasKing = true;

        if (newKing.isMarried) {
            if (newKing.spouse.city.kingdom.king != null && newKing.spouse.city.kingdom.king.id == newKing.spouse.id) {
                AssimilateKingdom(newKing.spouse.city.kingdom);
                return;
            }
        }
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
		newKing.AssignRole(ROLE.KING);
        if (newKing.city.governor.id == newKing.id) {
            newKing.city.AssignNewGovernor();
        }

        //		newKing.isKing = true;
        newKing.isGovernor = false;
//			KingdomManager.Instance.RemoveRelationshipToOtherKings (this.king);
		newKing.history.Add(new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, newKing.name + " became the new King/Queen of " + this.name + ".", HISTORY_IDENTIFIER.NONE));
//		this.king = newKing;
		newKing.CreateInitialRelationshipsToKings ();
		KingdomManager.Instance.AddRelationshipToOtherKings (newKing);
		this.successionLine.Clear();
		ChangeSuccessionLineRescursively (newKing);
		this.successionLine.AddRange (newKing.GetSiblings());
		UpdateKingSuccession ();
		this.RetrieveInternationWar();
//		UIManager.Instance.UpdateKingsGrid();
//		UIManager.Instance.UpdateKingdomSuccession ();

//		for (int i = 0; i < this.cities.Count; i++) {
//			this.cities[i].UpdateResourceProduction();
//		}
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
		this.RetrieveInternationWar();
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
	internal void AssimilateKingdom(Kingdom newKingdom){
		for(int i = 0; i < this.cities.Count; i++){
			newKingdom.AddCityToKingdom (this.cities [i]);
		}
		KingdomManager.Instance.MakeKingdomDead(this);
	}

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

	internal List<Kingdom> GetKingdomsByRelationship(RELATIONSHIP_STATUS relationshipStatus){
		List<Kingdom> kingdomsByRelationship = new List<Kingdom>();
		for (int i = 0; i < this.king.relationshipKings.Count; i++) {
			if (this.king.relationshipKings[i].lordRelationship == relationshipStatus) {
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

	internal void PassOnInternationalWar(){
		this.holderIntlWarCities.Clear();
		this.holderIntlWarCities.AddRange(this.intlWarCities);
	}
	internal void RetrieveInternationWar(){
		this.intlWarCities.AddRange(this.holderIntlWarCities);
		this.holderIntlWarCities.Clear();
	}

	internal City SearchForCityById(int id){
		for(int i = 0; i < this.cities.Count; i++){
			if(this.cities[i].id == id){
				return this.cities[i];
			}
		}
		return null;
	}

	internal IEnumerator ConquerCity(City city, General attacker){
		if (this.id != city.kingdom.id){
			HexTile hex = city.hexTile;
			//		city.kingdom.cities.Remove(city);
			city.KillCity();
			yield return null;
			City newCity = CreateNewCityOnTileForKingdom(hex);
			newCity.hp = 100;
			newCity.CreateInitialFamilies(false);
			KingdomManager.Instance.UpdateKingdomAdjacency();
//			this.AddInternationalWarCity (newCity);
			if (UIManager.Instance.currentlyShowingKingdom.id == newCity.kingdom.id) {
				newCity.kingdom.HighlightAllOwnedTilesInKingdom();
			}
			KingdomManager.Instance.CheckWarTriggerMisc (newCity.kingdom, WAR_TRIGGER.TARGET_GAINED_A_CITY);
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
	internal void AddInternationalWarCity(City newCity){
		for(int i = 0; i < this.relationshipsWithOtherKingdoms.Count; i++){
			if(this.relationshipsWithOtherKingdoms[i].isAtWar){
				if(!this.relationshipsWithOtherKingdoms[i].targetKingdom.intlWarCities.Contains(newCity)){
					this.relationshipsWithOtherKingdoms [i].targetKingdom.intlWarCities.Add (newCity);
				}
			}
		}
//		if(this.IsKingdomHasWar()){
//			if(!this.king.campaignManager.SearchForDefenseWarCities(newCity, WAR_TYPE.INTERNATIONAL)){
//				this.king.campaignManager.defenseWarCities.Add (new CityWar (newCity, false, WAR_TYPE.INTERNATIONAL));
//			}
//		}

	}
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
			return MILITARY_STRENGTH.COMPARABLE;
		}else{
			if(sourceMilStrength > (targetMilStrength + fiftyPercent)){
				return MILITARY_STRENGTH.MUCH_STRONGER;
			}else if(sourceMilStrength > (targetMilStrength + twentyPercent)){
				return MILITARY_STRENGTH.SLIGHTLY_STRONGER;
			}else if(sourceMilStrength > (targetMilStrength - twentyPercent)){
				return MILITARY_STRENGTH.COMPARABLE;
			}else if(sourceMilStrength > (targetMilStrength - fiftyPercent)){
				return MILITARY_STRENGTH.SLIGHTLY_WEAKER;
			}else{
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
				List<HexTile> path = PathGenerator.Instance.GetPath (this.cities [i].hexTile, allHostileCities [j].hexTile, PATHFINDING_MODE.AVATAR).ToList();
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
	internal void HasConflicted(){
		if(!this.hasConflicted){
			this.hasConflicted = true;
			for(int i = 0; i < this.cities.Count; i++){
				if(this.cities[i].governor != null){
					((Governor)this.cities[i].governor.assignedRole).UpdateLoyalty ();
				}
			}
		}
		this.borderConflictLoyaltyExpiration = 90;

	}
	internal void HasNotConflicted(){
		this.hasConflicted = false;
		this.borderConflictLoyaltyExpiration = 0;
		for(int i = 0; i < this.cities.Count; i++){
			if(this.cities[i].governor != null){
				((Governor)this.cities[i].governor.assignedRole).UpdateLoyalty ();
			}
		}
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
		if (!this._availableResources.ContainsKey(resource)) {
			this._availableResources.Add(resource, 0);
            this.RemoveObsoleteTradeRoutes(resource);
            this.UpdateExpansionRate();
            this.UpdateAllCitiesDailyGrowth();
            this.UpdateBasicResourceCount();
        }
		this._availableResources[resource] += 1;  
	}

    internal void UpdateExpansionRate() {
        this.expansionChance = this.kingdomTypeData.expansionRate;
        //get all resources from tiles and trade routes, only include trade routes where this kingom is the target
        List<RESOURCE> allAvailableResources = this._availableResources.Keys.ToList();
        for (int i = 0; i < this._tradeRoutes.Count; i++) {
            TradeRoute currTradeRoute = this._tradeRoutes[i];
            if (currTradeRoute.targetKingdom.id == this.id) {
                if (!allAvailableResources.Contains(currTradeRoute.resourceBeingTraded)) {
                    allAvailableResources.Add(currTradeRoute.resourceBeingTraded);
                }
            }
        }

        for (int i = 0; i < allAvailableResources.Count; i++) {
            RESOURCE currResource = allAvailableResources[i];
            if (Utilities.GetBaseResourceType(currResource) == this.basicResource) {
                if(currResource == RESOURCE.CEDAR || currResource == RESOURCE.GRANITE) {
                    this.expansionChance += 1;
                } else if (currResource == RESOURCE.OAK || currResource == RESOURCE.SLATE) {
                    this.expansionChance += 2;
                } else if (currResource == RESOURCE.EBONY || currResource == RESOURCE.MARBLE) {
                    this.expansionChance += 3;
                }
            }
        }
    }

    internal void UpdateTechLevel() {
        this._techLevel = 1;
        //get all resources from tiles and trade routes, only include trade routes where this kingom is the target
        List<RESOURCE> allAvailableResources = this._availableResources.Keys.ToList();
        for (int i = 0; i < this._tradeRoutes.Count; i++) {
            TradeRoute currTradeRoute = this._tradeRoutes[i];
            if (currTradeRoute.targetKingdom.id == this.id) {
                if (!allAvailableResources.Contains(currTradeRoute.resourceBeingTraded)) {
                    allAvailableResources.Add(currTradeRoute.resourceBeingTraded);
                }
            }
        }

        for (int i = 0; i < allAvailableResources.Count; i++) {
            RESOURCE currResource = allAvailableResources[i];
            if (currResource == RESOURCE.MANA_STONE) {
                this._techLevel += 1;
            } else if (currResource == RESOURCE.COBALT) {
                this._techLevel += 2;
            } else if (currResource == RESOURCE.MITHRIL) {
                this._techLevel += 3;
            }
        }
    }

    internal void UpdateAllCitiesDailyGrowth() {
        //get all resources from tiles and trade routes, only include trade routes where this kingom is the target
        List<RESOURCE> allAvailableResources = this._availableResources.Keys.ToList();
        for (int i = 0; i < this._tradeRoutes.Count; i++) {
            TradeRoute currTradeRoute = this._tradeRoutes[i];
            if (currTradeRoute.targetKingdom.id == this.id) {
                if (!allAvailableResources.Contains(currTradeRoute.resourceBeingTraded)) {
                    allAvailableResources.Add(currTradeRoute.resourceBeingTraded);
                }
            }
        }
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
            //			if (currentResource == RESOURCE.GRANITE || currentResource == RESOURCE.SLATE || currentResource == RESOURCE.MARBLE) {
            //				this.stoneCount += 3;
            //			} else if (currentResource == RESOURCE.CEDAR || currentResource == RESOURCE.OAK || currentResource == RESOURCE.EBONY) {
            //				this.lumberCount += 3;
            //			} else 
            if (currentResource == RESOURCE.CORN || currentResource == RESOURCE.DEER) {
                dailyGrowthGained += 4;
            } else if (currentResource == RESOURCE.WHEAT || currentResource == RESOURCE.RICE ||
                currentResource == RESOURCE.PIG || currentResource == RESOURCE.BEHEMOTH ||
                currentResource == RESOURCE.COBALT) {
                dailyGrowthGained += 8;
            } else if (currentResource == RESOURCE.MANA_STONE) {
                dailyGrowthGained += 12;
            } else if (currentResource == RESOURCE.MITHRIL) {
                dailyGrowthGained += 16;
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
        List<RESOURCE> allAvailableResourcesOfOtherKingdom = otherKingdom.availableResources.Keys.Union(otherKingdom.tradeRoutes.Select(x => x.resourceBeingTraded)).ToList();
        for (int i = 0; i < this._availableResources.Keys.Count; i++) {
            RESOURCE currKey = this._availableResources.Keys.ElementAt(i);
            if (!allAvailableResourcesOfOtherKingdom.Contains(currKey)) {
                //otherKingdom does not have that resource
                resourcesOtherKingdomDoesNotHave.Add(currKey);
            }
        }
        return resourcesOtherKingdomDoesNotHave;
    }

    protected void ProduceGoldFromTrade() {
        for (int i = 0; i < this._tradeRoutes.Count; i++) {
            TradeRoute currTradeRoute = this._tradeRoutes[i];
            if (currTradeRoute.sourceKingdom.id == this.id) {
                this.AdjustGold(GOLD_GAINED_FROM_TRADE);
            }
        }
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

    internal void UpdateBasicResourceCount() {
        this._basicResourceCount = 0;
        for (int i = 0; i < this._availableResources.Keys.Count; i++) {
            RESOURCE currResource = this._availableResources.Keys.ElementAt(i);
            if (Utilities.GetBaseResourceType(currResource) == this.basicResource) {
                this._basicResourceCount += 1;
            }
        }

        for (int i = 0; i < this._tradeRoutes.Count; i++) {
            TradeRoute currTradeRoute = this._tradeRoutes[i];
            if (currTradeRoute.targetKingdom.id == this.id && 
                Utilities.GetBaseResourceType(currTradeRoute.resourceBeingTraded) == this.basicResource) {
                this._basicResourceCount += 1;
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
		Citizen chosenGovernor = null;
		List<Citizen> ambitiousGovernors = this.cities.Select (x => x.governor).Where (x => x != null && x.hasTrait (TRAIT.AMBITIOUS) && ((Governor)x.assignedRole).loyalty < 0).ToList ();
		if(ambitiousGovernors != null && ambitiousGovernors.Count > 0){
			chosenGovernor = ambitiousGovernors [UnityEngine.Random.Range (0, ambitiousGovernors.Count)];
		}
		if(chosenGovernor != null){
			//Secession Event
//			EventCreator.Instance.CreateSecessionEvent(chosenGovernor);
		}else{
			int chance = UnityEngine.Random.Range (0, 2);
			if(chance == 0){
				//Riot Event
//				EventCreator.Instance.CreateRiotEvent(this);
				EventCreator.Instance.CreateRebellionEvent(this);
			}else{
				//Rebellion Event
				EventCreator.Instance.CreateRebellionEvent(this);
			}
		}
	}
    #endregion

    #region Tech
    private void IncreaseTechCounterPerTick(){
		int amount = 1 * this.cities.Count;
		int bonus = 0;
		amount += bonus;
		this.AdjustTechCounter (amount);
	}
	private void UpdateTechCapacity(){
		this.techCapacity = 2000 * this._techLevel;
	}
	private void AdjustTechCounter(int amount){
		this.techCounter += amount;
		this.techCounter = Mathf.Clamp(this.techCounter, 0, this.techCapacity);
		if(this.techCounter == this.techCapacity){
			this.UpgradeTechLevel (1);
		}
	}
	private void UpgradeTechLevel(int amount){
		this._techLevel += amount;
		this.techCounter = 0;
		this.UpdateTechCapacity ();
	}
	#endregion
	

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
                List<HexTile> neighbours = tilesToCheck[i].PurchasableTiles.ToList();
                    //.Where(x => x.ownedByCity != null && x.ownedByCity.kingdom.id != this.id && !this._discoveredKingdoms.Contains(x.ownedByCity.kingdom))
                    //.ToList();
                for (int k = 0; k < neighbours.Count; k++) {
                    HexTile currNeighbour = neighbours[i];
                    if (currNeighbour.isOccupied && currNeighbour.ownedByCity != null
                        && currNeighbour.ownedByCity.kingdom.id != this.id) {
                        Kingdom otherKingdom = currNeighbour.ownedByCity.kingdom;

                        this.DiscoverKingdom(otherKingdom);
                        otherKingdom.DiscoverKingdom(this);
                    } else if (currNeighbour.isBorder) {
                        Kingdom otherKingdom = CityGenerator.Instance.GetCityByID(currNeighbour.isBorderOfCityID).kingdom;

                        if (otherKingdom.id != this.id) {
                            this.DiscoverKingdom(otherKingdom);
                            otherKingdom.DiscoverKingdom(this);
                        }
                    }

                    //Kingdom otherKingdom = neighbours[i].ownedByCity.kingdom;
                    //this.DiscoverKingdom(otherKingdom);
                    //otherKingdom.DiscoverKingdom(this);
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
            List<HexTile> neighbours = tilesToCheck[i].PurchasableTiles.ToList();
            for (int j = 0; j < neighbours.Count; j++) {
                HexTile currNeighbour = neighbours[j];
                if (currNeighbour.isOccupied && currNeighbour.ownedByCity != null
                    && currNeighbour.ownedByCity.kingdom.id != this.id) {
                    Kingdom otherKingdom = currNeighbour.ownedByCity.kingdom;

                    this.DiscoverKingdom(otherKingdom);
                    otherKingdom.DiscoverKingdom(this);
                } else if (currNeighbour.isBorder) {
                    Kingdom otherKingdom = CityGenerator.Instance.GetCityByID(currNeighbour.isBorderOfCityID).kingdom;

                    if (otherKingdom.id != this.id) {
                        this.DiscoverKingdom(otherKingdom);
                        otherKingdom.DiscoverKingdom(this);
                    }
                }
            }
        }
            
            //.Where(x => x.ownedByCity != null && x.ownedByCity.kingdom.id != this.id && !this._discoveredKingdoms.Contains(x.ownedByCity.kingdom))
            //.ToList();
        
    }

    internal void DiscoverKingdom(Kingdom discoveredKingdom) {
        if (!this._discoveredKingdoms.Contains(discoveredKingdom)) {
            this._discoveredKingdoms.Add(discoveredKingdom);
            Debug.LogError(this.name + " discovered " + discoveredKingdom.name + "!");
        }
    }

    internal void RemoveKingdomFromDiscoveredKingdoms(Kingdom kingdomToRemove) {
        this._discoveredKingdoms.Remove(kingdomToRemove);
    }
}
