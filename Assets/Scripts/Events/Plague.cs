using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Plague : GameEvent {

    private delegate void OnPerformAction();
    private OnPerformAction onPerformAction;

	internal City sourceCity;
	internal Kingdom sourceKingdom;
	internal List<Kingdom> affectedKingdoms;
	internal List<City> affectedCities;
	internal List<Kingdom> otherKingdoms;
	internal int bioWeaponMeter;
	internal int vaccineMeter;
	internal int bioWeaponMeterMax;
	internal int vaccineMeterMax;
	internal int daysCount;

    private bool isVaccineDeveloped;
    private bool isBioWeaponDeveloped;
	private bool hasHumanisticKingdom;
	private bool hasPragmaticKingdom;
	private bool hasOpportunisticKingdom;


    private const float DEFAULT_CURE_CHANCE = 1.5f;
    private const float DEFAULT_SETTLEMENT_SPREAD_CHANCE = 10f;

    protected Dictionary<int, float[]> kingdomChances; //0 - cure chance, 1 - settlement spread chance

    protected List<Kingdom> pragmaticKingdoms;
    protected List<Kingdom> opportunisticKingdoms;
    protected List<Kingdom> humanisticKingdoms;

    protected List<Exterminator> exterminators;
    protected List<Scourge> scourgers;
    protected List<Healer> healers;


    private string[] plagueAdjectives = new string[] {
        "Red", "Green", "Yellow", "Black", "Rotting", "Silent", "Screaming", "Trembling", "Sleeping",
        "Cat", "Dog", "Pig", "Lamb", "Lizard", "Bog", "Death", "Stomach", "Eye", "Finger", "Rabid",
        "Fatal", "Blistering", "Icy", "Scaly", "Sexy", "Violent", "Necrotic", "Foul", "Vile", "Nasty",
        "Ghastly", "Malodorous", "Cave", "Phantom", "Wicked", "Strange"
    };

    private string[] plagueDieseases = new string[] {
        "Sores", "Ebola", "Anthrax", "Pox", "Face", "Sneeze", "Gangrene", "Throat", "Rash", "Warts",
        "Cholera", "Colds", "Ache", "Syndrome", "Tumor", "Chills", "Blisters", "Mouth", "Fever", "Delirium",
        "Measles", "Mutata", "Disease"
    };

    private string _plagueName;

    #region getters/setters
    public string plagueName {
        get { return this._plagueName; }
    }
    #endregion

    public Plague(int startWeek, int startMonth, int startYear, Citizen startedBy, City sourceCity) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.PLAGUE;
		this.name = "Plague";
		this._warTrigger = WAR_TRIGGER.OPPOSING_APPROACH;
        this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
		this.sourceCity = sourceCity;
        this._plagueName = GeneratePlagueName();
		this.sourceKingdom = sourceCity.kingdom;
		this.affectedKingdoms = new List<Kingdom>();
		this.affectedCities = new List<City> ();
		this.otherKingdoms = GetOtherKingdoms ();
		this.bioWeaponMeter = 0;
		this.vaccineMeter = 0;
		this.daysCount = 0;
        this.isVaccineDeveloped = false;
        this.isBioWeaponDeveloped = false;
		this.hasHumanisticKingdom = false;
		this.hasPragmaticKingdom = false;
		this.hasOpportunisticKingdom = false;

        this.pragmaticKingdoms = new List<Kingdom>();
        this.opportunisticKingdoms = new List<Kingdom>();
        this.humanisticKingdoms = new List<Kingdom>();

        this.exterminators = new List<Exterminator>();
        this.scourgers = new List<Scourge>();
        this.healers = new List<Healer>();

        this.kingdomChances = new Dictionary<int, float[]>();

        int maxMeter = 200 * NumberOfCitiesInWorld ();
		this.bioWeaponMeterMax = maxMeter;
		this.vaccineMeterMax = maxMeter;
       
		WorldEventManager.Instance.currentPlague = this;

        EventManager.Instance.AddEventToDictionary(this);
        EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
        EventManager.Instance.onKingdomDiedEvent.AddListener(CureAKingdom);

		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Plague", "event_title");
		newLogTitle.AddToFillers (null, this._plagueName, LOG_IDENTIFIER.RANDOM_GENERATED_EVENT_NAME);

		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Plague", "start");
		newLog.AddToFillers (this.sourceCity, this.sourceCity.name, LOG_IDENTIFIER.CITY_1);
		newLog.AddToFillers (this.sourceKingdom, this.sourceKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (null, this._plagueName, LOG_IDENTIFIER.RANDOM_GENERATED_EVENT_NAME);

		this.InitializePlague();
		base.EventIsCreated();
	}

    private string GeneratePlagueName() {
        return plagueAdjectives[Random.Range(0, plagueAdjectives.Length)] + " " + plagueDieseases[Random.Range(0, plagueDieseases.Length)];
    }

    internal void AddAgentToList(Citizen citizen) {
//		string logName = string.Empty;
        if(citizen.assignedRole is Exterminator) {
            this.exterminators.Add((Exterminator)citizen.assignedRole);
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Plague", "exterminator_send");
			newLog.AddToFillers (citizen.city.kingdom.king, citizen.city.kingdom.king.name, LOG_IDENTIFIER.KING_1);
			newLog.AddToFillers (citizen, citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        }else if (citizen.assignedRole is Healer) {
            this.healers.Add((Healer)citizen.assignedRole);
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Plague", "healer_send");
			newLog.AddToFillers (citizen.city.kingdom.king, citizen.city.kingdom.king.name, LOG_IDENTIFIER.KING_1);
			newLog.AddToFillers (citizen, citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        }
    }

    #region overrides
    internal override void PerformAction() {
		this.daysCount += 1;
        onPerformAction();
    }
    internal override void DoneCitizenAction(Citizen citizen) {
        base.DoneCitizenAction(citizen);
        if (citizen.assignedRole is Exterminator) {
            DestroyASettlementInCity(citizen.assignedRole.targetCity);
            citizen.city.kingdom.AdjustUnrest(5);
            this.exterminators.Remove((Exterminator)citizen.assignedRole);
        } else if (citizen.assignedRole is Healer) {
            CureASettlementInCity(citizen.assignedRole.targetCity);
            this.healers.Remove((Healer)citizen.assignedRole);
        }
        citizen.assignedRole.DestroyGO();
    }
    internal override void DoneEvent() {
        base.DoneEvent();
        EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);
        onPerformAction = null;
        DisembargoKingdoms();
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Plague", "event_end");
		newLog.AddToFillers (null, this._plagueName, LOG_IDENTIFIER.RANDOM_GENERATED_EVENT_NAME);

		WorldEventManager.Instance.currentPlague = null;
    }
    #endregion

    private void InitializePlague(){
//		this.PlagueASettlement (this.sourceCity.structures [0]);
		this.PlagueAKingdom (this.sourceKingdom, this.sourceCity);
        onPerformAction += SpreadPlagueWithinCity;
        onPerformAction += SpreadPlagueWithinKingdom;
        onPerformAction += CureAPlagueSettlementEveryday;
        onPerformAction += DestroyASettlementEveryday;
        onPerformAction += IncreaseUnrestEveryMonth;
        onPerformAction += CheckForGovernorOrRelativeDeath;
        onPerformAction += CheckForKingOrRelativeDeath;

    }

    private List<Kingdom> GetOtherKingdoms(){
		if(this.sourceCity == null){
			return null;
		}
		List<Kingdom> kingdoms = new List<Kingdom> ();
		for(int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++){
			if(KingdomManager.Instance.allKingdoms[i].id != this.sourceKingdom.id && KingdomManager.Instance.allKingdoms[i].isAlive()){
				kingdoms.Add (KingdomManager.Instance.allKingdoms [i]);
			}
		}
		return kingdoms;
	}
	private int NumberOfCitiesInWorld(){
		int count = 0;
		for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
			count += KingdomManager.Instance.allKingdoms [i].cities.Count;
		}
		return count;
	}

    
    #region Event Approaches
    private void HumanisticApproach() {
        for (int i = 0; i < this.humanisticKingdoms.Count; i++) {
            Kingdom currKingdom = this.humanisticKingdoms[i];
            if (this.IsDaysMultipleOf(5)) {
                if (this.healers.FirstOrDefault(x => x.citizen.city.kingdom.id == currKingdom.id) != null) {
                    //There is currently an active healer for currKingdom.
                    return;
                }
                //Every multiple of 5 day, Humanistic Kingdoms have 3% chance for each city it has to produce a Healer Agent.
//                int chanceForHealer = 3 * currKingdom.cities.Count;
				int chanceForHealer = 50;
                int chance = Random.Range(0, 100);
                if (chance < chanceForHealer) {
                    //Create Healer
                    List<HexTile> path = null;
                    City randCity = currKingdom.cities[Random.Range(0, currKingdom.cities.Count)];
                    List<City> plaguedCitiesToChooseFrom = currKingdom.cities.Where(x => x.plague != null).ToList();
                    if (plaguedCitiesToChooseFrom.Count <= 0) {
                        //If there are no plagued cities within the kingdom, the Healer Agent 
                        //will target nearest plagued city of neutral relationship or better Kingdoms.
                        List<Kingdom> elligibleKingdomsForHealer = currKingdom.GetKingdomsByRelationship(new RELATIONSHIP_STATUS[] {
                            RELATIONSHIP_STATUS.NEUTRAL,
                            RELATIONSHIP_STATUS.WARM,
                            RELATIONSHIP_STATUS.FRIEND,
                            RELATIONSHIP_STATUS.ALLY
                        });
                        for (int j = 0; j < elligibleKingdomsForHealer.Count; j++) {
                            plaguedCitiesToChooseFrom.AddRange(elligibleKingdomsForHealer[j].cities.Where(x => x.plague != null));
                        }
                    }

                    if (plaguedCitiesToChooseFrom.Count > 0) {
                        City targetCity = GetNearestCityFrom(randCity.hexTile, plaguedCitiesToChooseFrom, ref path);
                        if (targetCity != null && path != null) {
                            Citizen healer = randCity.CreateAgent(ROLE.HEALER, this.eventType, targetCity.hexTile, this.durationInDays, path);
                            if (healer != null) {
                                healer.assignedRole.Initialize(this);
                            }
                        }
                    }

                }
            }
            ContributeToVaccine(currKingdom);
        }
    }
    private void PragmaticApproach() {
        for (int i = 0; i < this.pragmaticKingdoms.Count; i++) {
            Kingdom currKingdom = this.pragmaticKingdoms[i];
            if (this.IsDaysMultipleOf(5)) {
                if (this.exterminators.FirstOrDefault(x => x.citizen.city.kingdom.id == currKingdom.id) != null) {
                    //There is currently an active exterminator for currKingdom.
                    return;
                }
                //Every multiple of 5 day, Pragmatic Kingdoms have 3% chance for each city it has to produce an Exterminator Agent.
                int chanceForExterminator = 3 * currKingdom.cities.Count;
                int chance = Random.Range(0, 100);
                if (chance < chanceForExterminator) {
                    //Create Exterminator
                    List<HexTile> path = null;
                    City randCity = currKingdom.cities[Random.Range(0, currKingdom.cities.Count)];
                    List<City> plaguedCitiesInKingdom = currKingdom.cities.Where(x => x.plague != null).ToList();
                    if (plaguedCitiesInKingdom.Count > 0) {
                        City targetCity = GetNearestCityFrom(randCity.hexTile, plaguedCitiesInKingdom, ref path);
                        if (targetCity != null && path != null) {
                            Citizen exterminator = randCity.CreateAgent(ROLE.EXTERMINATOR, this.eventType, targetCity.hexTile, this.durationInDays, path);
                            if (exterminator != null) {
                                exterminator.assignedRole.Initialize(this);
                            }
                        }
                    }
                }
            }
            ContributeToVaccine(currKingdom);
        }
    }
    private void OpportunisticApproach() {
        if (!this.isBioWeaponDeveloped) {
            for (int i = 0; i < this.opportunisticKingdoms.Count; i++) {
                Kingdom currKingdom = this.opportunisticKingdoms[i];
                ContributeToBioWeapon(currKingdom);
            }
        }
    }

	/*
	 * Update Approach of new king when old king dies
	 * */
	internal void UpdateApproach(Kingdom kingdom){
		if(!IsApproachTheSame(kingdom) && this.affectedKingdoms.Contains(kingdom)){
			RemoveKingdomFromApproaches (kingdom);
			EVENT_APPROACH chosenApproach = this.ChooseApproach(kingdom.king);
			UpdateKingdomEmbargos();
			DifferentApproachesLogs (kingdom.king, chosenApproach, null);
		}
	}
	/*
	 * Remove kingdom from previous approach
	 * */
	private void RemoveKingdomFromApproaches(Kingdom kingdom){
		if(this.humanisticKingdoms.Contains(kingdom)){
			this.humanisticKingdoms.Remove (kingdom);
		}else if(this.pragmaticKingdoms.Contains(kingdom)){
			this.pragmaticKingdoms.Remove (kingdom);
		}else if(this.opportunisticKingdoms.Contains(kingdom)){
			this.opportunisticKingdoms.Remove (kingdom);
		}
		CheckCurrentKingdoms ();
	}

	/*
	 * Check if the new king and old king has the same approach
	 * */
	private bool IsApproachTheSame(Kingdom kingdom){
		EVENT_APPROACH approach = this.DetermineApproach (kingdom.king);
		if(approach == EVENT_APPROACH.HUMANISTIC && this.humanisticKingdoms.Contains(kingdom)){
			return true;
		}else if(approach == EVENT_APPROACH.PRAGMATIC && this.pragmaticKingdoms.Contains(kingdom)){
			return true;
		}else if(approach == EVENT_APPROACH.OPPORTUNISTIC && this.opportunisticKingdoms.Contains(kingdom)){
			return true;
		}
		return false;
	}

	/*
	 * Check if current kingdoms 
	 * */
	private void CheckCurrentKingdoms(){
		if(this.humanisticKingdoms.Count <= 0 && this.hasHumanisticKingdom){
			this.hasHumanisticKingdom = false;
			onPerformAction -= HumanisticApproach;
		}else if(this.pragmaticKingdoms.Count <= 0 && this.hasPragmaticKingdom){
			this.hasPragmaticKingdom = false;
			onPerformAction -= PragmaticApproach;
		}else if(this.opportunisticKingdoms.Count <= 0 && this.hasOpportunisticKingdom){
			this.hasOpportunisticKingdom = false;
			onPerformAction -= OpportunisticApproach;
		}
	}

    internal void ForceUpdateKingRelationships(Citizen king) {
        EVENT_APPROACH chosenApproach = DetermineApproach(king);
        this.ChangeKingRelationshipsAfterApproach(king, chosenApproach);
    }
    /*
     * Choose what approach this king will use
     * to handle this event. Then add his kingdom
     * to the appropriate list.
     * */
    private EVENT_APPROACH ChooseApproach(Citizen citizen) {
        Dictionary<CHARACTER_VALUE, int> importantCharVals = citizen.importantCharacterValues;
        EVENT_APPROACH chosenApproach = DetermineApproach(citizen);
        this.AddKingdomToApproach(chosenApproach, citizen.city.kingdom);

        this.ChangeKingRelationshipsAfterApproach(citizen, chosenApproach);
        this.ChangeLoyaltyAfterApproach(citizen, chosenApproach);

        return chosenApproach;
    }

    private void ChangeKingRelationshipsAfterApproach(Citizen citizen, EVENT_APPROACH chosenApproach) {
        for (int i = 0; i < citizen.city.kingdom.discoveredKingdoms.Count; i++) {
            Kingdom otherKingdom = citizen.city.kingdom.discoveredKingdoms[i];
            RelationshipKings rel = otherKingdom.king.GetRelationshipWithCitizen(citizen);
            EVENT_APPROACH otherKingApproach = this.DetermineApproach(otherKingdom.king);
            if (otherKingApproach == chosenApproach) {
                rel.AddEventModifier(20, "plague handling", this);
            } else {
                rel.AddEventModifier(-20, "plague handling", this);
            }
        }
    }

    private void ChangeLoyaltyAfterApproach(Citizen citizen, EVENT_APPROACH chosenApproach) {
        for (int i = 0; i < citizen.city.kingdom.cities.Count; i++) {
            Governor gov = (Governor)citizen.city.kingdom.cities[i].governor.assignedRole;
            EVENT_APPROACH govApproach = this.DetermineApproach(gov.citizen);
            if (govApproach == chosenApproach) {
                gov.AddEventModifier(20, "+20 plague handling", this);
            } else {
                gov.AddEventModifier(-20, "-20 plague handling", this);
            }
        }
    }

    /*
     * Determine what approach a citizen
     * will choose. This will not add that citizen's
     * kingdom to the appropriate list.
     * */
    private EVENT_APPROACH DetermineApproach(Citizen citizen) {
        Dictionary<CHARACTER_VALUE, int> importantCharVals = citizen.importantCharacterValues;
        EVENT_APPROACH chosenApproach = EVENT_APPROACH.NONE;
        if (importantCharVals.ContainsKey(CHARACTER_VALUE.LIFE) || importantCharVals.ContainsKey(CHARACTER_VALUE.EQUALITY) ||
            importantCharVals.ContainsKey(CHARACTER_VALUE.GREATER_GOOD) || importantCharVals.ContainsKey(CHARACTER_VALUE.CHAUVINISM)) {

            KeyValuePair<CHARACTER_VALUE, int> priotiyValue = importantCharVals
				.FirstOrDefault(x => x.Key == CHARACTER_VALUE.CHAUVINISM || x.Key == CHARACTER_VALUE.GREATER_GOOD 
                || x.Key == CHARACTER_VALUE.LIFE || x.Key == CHARACTER_VALUE.EQUALITY);

            if (priotiyValue.Key == CHARACTER_VALUE.CHAUVINISM) {
                chosenApproach = EVENT_APPROACH.OPPORTUNISTIC;
            } else if (priotiyValue.Key == CHARACTER_VALUE.GREATER_GOOD) {
                chosenApproach = EVENT_APPROACH.PRAGMATIC;
            } else {
                chosenApproach = EVENT_APPROACH.HUMANISTIC;
            }
        } else {
            //a king who does not value any of the these four ethics will choose OPPORTUNISTIC APPROACH in dealing with a plague.
            chosenApproach = EVENT_APPROACH.OPPORTUNISTIC;
        }
        return chosenApproach;
    }

    private void AddKingdomToApproach(EVENT_APPROACH approach, Kingdom kingdomToAdd) {
		
        if (approach == EVENT_APPROACH.HUMANISTIC) {
			if (humanisticKingdoms.Count <= 0 && !this.hasHumanisticKingdom) {
				this.hasHumanisticKingdom = true;
                onPerformAction += HumanisticApproach;
            }
            if (!humanisticKingdoms.Contains(kingdomToAdd)) {
                humanisticKingdoms.Add(kingdomToAdd);
            }
        } else if (approach == EVENT_APPROACH.PRAGMATIC) {
			if (pragmaticKingdoms.Count <= 0 && !this.hasPragmaticKingdom) {
				this.hasPragmaticKingdom = true;
                onPerformAction += PragmaticApproach;
            }
            if (!pragmaticKingdoms.Contains(kingdomToAdd)) {
                pragmaticKingdoms.Add(kingdomToAdd);
                //The plague spread for Pragmatic Kingdoms is reduced from 2% to 1.5%
                this.kingdomChances[kingdomToAdd.id][1] = 1.5f;
            }
        } else if (approach == EVENT_APPROACH.OPPORTUNISTIC) {
			if (opportunisticKingdoms.Count <= 0 && !this.hasOpportunisticKingdom) {
				this.hasOpportunisticKingdom = true;
                onPerformAction += OpportunisticApproach;
            }
            if (!opportunisticKingdoms.Contains(kingdomToAdd)) {
                opportunisticKingdoms.Add(kingdomToAdd);
				//The cure chance for Opportunistic Kingdoms is reduced from 1.5% to 0.5%
                this.kingdomChances[kingdomToAdd.id][0] = 0.5f;
            }
        }
    }
    #endregion

    internal HexTile InfectRandomSettlement(List<HexTile> hexTilesToChooseFrom) {
		HexTile targetSettlement = hexTilesToChooseFrom.FirstOrDefault(x => !x.isPlagued);
        if (targetSettlement != null) {
            this.PlagueASettlement(targetSettlement);
            return targetSettlement;
        }
        return null;
    }

    private void PlagueASettlement(HexTile hexTile){
		hexTile.SetPlague(true);
	}
	internal HexTile PlagueACity(City city){
		city.plague = this;
		this.affectedCities.Add (city);
		HexTile infectedTile = InfectRandomSettlement (city.structures);
        return infectedTile;
	}
	internal HexTile PlagueAKingdom(Kingdom kingdom, City city){
		kingdom.plague = this;
		this.affectedKingdoms.Add (kingdom);
        float cureChance = DEFAULT_CURE_CHANCE;
        if (this.isVaccineDeveloped) {
            cureChance = 5f;
        }
        kingdomChances.Add(kingdom.id, new float[] {
            cureChance,
            DEFAULT_SETTLEMENT_SPREAD_CHANCE
        });
        EVENT_APPROACH chosenApproach = this.ChooseApproach(kingdom.king);
        UpdateKingdomEmbargos();
        DifferentApproachesLogs(kingdom.king, chosenApproach, city);

        HexTile infectedTile = this.PlagueACity (city);
        return infectedTile;
	}

    #region Trading
    /*
    * Put all other kingdoms in a kigndoms
    * embargo list.
    * */
    private void EmbargoOtherKingdoms(Kingdom kingdom) {
        for (int i = 0; i < affectedKingdoms.Count; i++) {
            Kingdom otherKingdom = affectedKingdoms[i];
            if (otherKingdom.id != kingdom.id) {
                kingdom.AddKingdomToEmbargoList(otherKingdom, EMBARGO_REASON.PLAGUE);
                otherKingdom.AddKingdomToEmbargoList(kingdom, EMBARGO_REASON.PLAGUE);
            }
        }
    }

    /*
     * Update the embargo lists of the pragmatic
     * kingdoms to include all plagued kingdoms.
     * */
    private void UpdateKingdomEmbargos() {
        List<Kingdom> kingdoms = new List<Kingdom>(pragmaticKingdoms);
        kingdoms.AddRange(opportunisticKingdoms);
        for (int i = 0; i < kingdoms.Count; i++) {
            Kingdom currKingdom = kingdoms[i];
            EmbargoOtherKingdoms(currKingdom);
        }
    }

    /*
     * Lift plague related embargos.
     * */
    private void DisembargoKingdoms() {
        for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
            Kingdom currKingdom = KingdomManager.Instance.allKingdoms[i];
            for (int j = 0; j < currKingdom.embargoList.Count; j++) {
                KeyValuePair<Kingdom, EMBARGO_REASON> kvp = currKingdom.embargoList.ElementAt(j);
                if(kvp.Value == EMBARGO_REASON.PLAGUE) {
                    currKingdom.RemoveKingdomFromEmbargoList(kvp.Key);
                }
            }
        }
    }
    #endregion

    private void CureASettlement(HexTile hexTile){
		hexTile.SetPlague(false);
        CheckIfCityIsCured(hexTile.ownedByCity);
	}
	private void CureACity(City city){
		List<HexTile> plaguedSettlements = city.plaguedSettlements;
		for (int i = 0; i < plaguedSettlements.Count; i++) {
			this.CureASettlement (plaguedSettlements [i]);
		}
//		DisinfectACity(city);
	}
	private void CureAKingdom(Kingdom kingdom){
//      DisinfectAKingdom(kingdom);
		for (int i = 0; i < kingdom.cities.Count; i++) {
			this.CureACity (kingdom.cities [i]);
		}
	}

    private void DisinfectACity(City city) {
        city.plague = null;
        this.affectedCities.Remove(city);
        CheckIfKingdomIsCured(city.kingdom);
    }

    private void DisinfectAKingdom(Kingdom kingdom) {
		kingdom.plague = null;
        this.affectedKingdoms.Remove(kingdom);
		RemoveKingdomFromApproaches (kingdom);
        if (this.kingdomChances.ContainsKey(kingdom.id)) {
            this.kingdomChances.Remove(kingdom.id);
        }
        CheckIfPlagueIsCured();
    }

    internal void CheckIfCityIsCured(City city) {
		if (city.plaguedSettlements.Count <= 0 || city.plaguedSettlements == null) {
            DisinfectACity(city);
        }
    }

    private void CheckIfKingdomIsCured(Kingdom kingdom) {
        if(this.affectedCities.Intersect(kingdom.cities).Count() <= 0) {
            DisinfectAKingdom(kingdom);
        }
    }

    private void CheckIfPlagueIsCured() {
        if(this.affectedKingdoms.Count <= 0 && this.affectedCities.Count <= 0) {
            this.DoneEvent();
        }
    }
   

	private void DestroyASettlementInCity(City city){
		List<HexTile> plaguedSettlements = city.plaguedSettlements;
		if(plaguedSettlements != null && plaguedSettlements.Count > 0){
			HexTile targetSettlement = plaguedSettlements [plaguedSettlements.Count - 1];
			if(targetSettlement != null){
				/*
             * Reset tile for now.
             * TODO: When ruined settlement sprites are provided, use those instead.
             * */
				city.RemoveTileFromCity(targetSettlement);
				
			}
		}

	}
    private void CureASettlementInCity(City city) {
        List<HexTile> plaguedSettlements = city.plaguedSettlements;
		if (plaguedSettlements != null && plaguedSettlements.Count > 0) {
			HexTile targetSettlement = plaguedSettlements [plaguedSettlements.Count - 1];
			if (targetSettlement != null) {
				this.CureASettlement (targetSettlement);
			}
		}

    }
    private bool IsDaysMultipleOf (int multiple){
		if((this.daysCount % multiple) == 0){
			return true;
		}
		return false;
	}

    #region Plague Default Actions
    /*
     * Every multiple of 4 day, per city, there is a base 2% chance for every 
     * plagued settlement that the plague will spread to another settlement 
     * within the city.
     * */
    private void SpreadPlagueWithinCity(){
		if(this.IsDaysMultipleOf(4)){
			for (int i = 0; i < this.affectedCities.Count; i++) {
				float chance = UnityEngine.Random.Range (0f, 99f);
                Kingdom kingdomOfCity = this.affectedCities[i].kingdom;
                if (this.kingdomChances.ContainsKey(kingdomOfCity.id)) {
                    float value = this.kingdomChances[kingdomOfCity.id][1] * this.affectedCities[i].plaguedSettlements.Count;
                    if (chance < value) {
                        InfectRandomSettlement(this.affectedCities[i].structures);
                    }
                }
			}
		}
	}

    /*
     * Every multiple of 4 day, per kingdom, there is a 1% chance for every plagued settlement
     * within the kingdom that the plague will spread to another clean city within the kingdom.
     * */
	private void SpreadPlagueWithinKingdom(){
		if (this.IsDaysMultipleOf (4)) {
			for (int i = 0; i < this.affectedKingdoms.Count; i++) {
				int chance = UnityEngine.Random.Range (0, 100);
				int value = 3 * this.affectedKingdoms [i].cities.Sum (x => x.plaguedSettlements.Count);
				if (chance < value) {
					City targetCity = this.affectedKingdoms [i].cities.FirstOrDefault (x => x.plague == null);
					if (targetCity != null) {
						this.PlagueACity (targetCity);
					}
				}
			}
		}
	}

    /*
     * Every multiple of 5 day, per kingdom, there is a 1.5% chance for every 
     * plagued settlement that one of them will be cured.
     * */
	private void CureAPlagueSettlementEveryday(){
		if (this.IsDaysMultipleOf (5)) {
			for (int i = 0; i < this.affectedKingdoms.Count; i++) {
				float chance = UnityEngine.Random.Range (0f, 99f);
                Kingdom kingdomOfCity = this.affectedCities[i].kingdom;
                float value = this.kingdomChances[kingdomOfCity.id][0] * (float)this.affectedKingdoms [i].cities.Sum (x => x.plaguedSettlements.Count);
				if (chance < value) {
					City targetCity = this.affectedKingdoms [i].cities.FirstOrDefault (x => x.plague != null);
					if (targetCity != null) {
						HexTile targetSettlement = targetCity.structures.FirstOrDefault (x => x.isPlagued);
						if(targetSettlement != null){
							this.CureASettlement (targetSettlement);
						}
					}
				}
			}
		}
	}

    /*
     * Every multiple of 6 day, per kingdom, there is a 1% chance for 
     * every plagued settlement that one of them will be ruined.
     * */
	private void DestroyASettlementEveryday(){
		if (this.IsDaysMultipleOf (6)) {
			for (int i = 0; i < this.affectedKingdoms.Count; i++) {
				int chance = UnityEngine.Random.Range (0, 100);
				int value = 1 * this.affectedKingdoms [i].cities.Sum (x => x.plaguedSettlements.Count);
				if (chance < value) {
					City targetCity = this.affectedKingdoms [i].cities.FirstOrDefault (x => x.plague != null);
					if (targetCity != null) {
						this.DestroyASettlementInCity (targetCity);
					}
				}
			}
		}
	}

    /*
     * At the start of each month, unrest in the kingdom 
     * increases by 1 for every plagued settlement.
     * */
    private void IncreaseUnrestEveryMonth() {
        if (GameManager.Instance.days == 1) {
            for (int i = 0; i < this.affectedKingdoms.Count; i++) {
                Kingdom currKingdom = this.affectedKingdoms[i];
                int unrestIncrease = 1 * currKingdom.cities.Sum(x => x.plaguedSettlements.Count);
                if (this.humanisticKingdoms.Contains(currKingdom)) {
                    unrestIncrease /= 2;
                }
                currKingdom.AdjustUnrest(unrestIncrease);
            }
        }
    }
    
    /*
     * Every mult 4 day, 3% chance for every plagued settlement, 
     * for a city governor or a random relative to die of the plague.
     * */
    private void CheckForGovernorOrRelativeDeath() {
        if (this.IsDaysMultipleOf(4)) {
            for (int i = 0; i < this.affectedCities.Count; i++) {
                City currCity = this.affectedCities[i];
                int chance = UnityEngine.Random.Range(0, 100);
                int value = 3 * currCity.plaguedSettlements.Count;
                if(chance < value) {
                    List<Citizen> citizensToChooseFrom = new List<Citizen>() { currCity.governor };
                    citizensToChooseFrom.AddRange(currCity.governor.GetRelatives(-1));
                    Citizen citizenToDie = citizensToChooseFrom[Random.Range(0, citizensToChooseFrom.Count)];
                    citizenToDie.Death(DEATH_REASONS.PLAGUE);
                    Log plagueDeath = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Plague", "plague_citizen_death");
                }
            }
        }
    }

    /*
     * Every mult 5 day, 3% chance for every plagued settlement in the capital, 
     * for a king or a random relative to die of the plague.
     * */
    private void CheckForKingOrRelativeDeath() {
        if (this.IsDaysMultipleOf(5)) {
            for (int i = 0; i < this.affectedKingdoms.Count; i++) {
                Kingdom currKingdom = this.affectedKingdoms[i];
                int chance = UnityEngine.Random.Range(0, 100);
                int value = 3 * currKingdom.capitalCity.plaguedSettlements.Count;
                if (chance < value) {
                    List<Citizen> citizensToChooseFrom = new List<Citizen>() { currKingdom.king };
                    citizensToChooseFrom.AddRange(currKingdom.king.GetRelatives(-1));
                    Citizen citizenToDie = citizensToChooseFrom[Random.Range(0, citizensToChooseFrom.Count)];
                    citizenToDie.Death(DEATH_REASONS.PLAGUE);
                    Log plagueDeath = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Plague", "plague_citizen_death");
                }
            }
        }
    }
    #endregion

    private City GetNearestCityFrom(HexTile hexTile, List<City> citiesToChooseFrom, ref List<HexTile> path) {
        City nearestCity = null;
        int nearestDistance = 0;
        for (int i = 0; i < citiesToChooseFrom.Count; i++) {
            City currCity = citiesToChooseFrom[i];
            List<HexTile> newPath = PathGenerator.Instance.GetPath(hexTile, currCity.hexTile, PATHFINDING_MODE.AVATAR);
            if(newPath != null) {
                if(nearestCity == null) {
                    nearestCity = currCity;
                    nearestDistance = newPath.Count;
                    path = newPath;
                } else {
                    if(newPath.Count < nearestDistance) {
                        nearestCity = currCity;
                        nearestDistance = newPath.Count;
                        path = newPath;
                    }
                }
            }
        }
        return nearestCity;
    }

    private void ContributeToVaccine(Kingdom kingdom) {
		if(!this.isVaccineDeveloped){
			int multiplier = 0;
			if (this.pragmaticKingdoms.Contains(kingdom)) {
				multiplier = 1;
			} else if (this.humanisticKingdoms.Contains(kingdom)) {
				multiplier = 2;
			}

			this.vaccineMeter += (multiplier * kingdom.cities.Count);
			this.vaccineMeter = Mathf.Clamp(this.vaccineMeter, 0, this.vaccineMeterMax);
			if (this.vaccineMeter == this.vaccineMeterMax) {
				DevelopVaccine();
			}
		}
    }

	private void ContributeToBioWeapon(Kingdom kingdom) {
		if(!this.isBioWeaponDeveloped){
			this.bioWeaponMeter += 3 * kingdom.cities.Count;
			this.bioWeaponMeter = Mathf.Clamp(this.bioWeaponMeter, 0, this.bioWeaponMeterMax);
			if(this.bioWeaponMeter == this.bioWeaponMeterMax) {
				SelectKingdomForBioWeaponDevelopment ();
				this.isBioWeaponDeveloped = true;
			}
		}
	}

	private void SelectKingdomForBioWeaponDevelopment(){
		Kingdom selectedKingdom = this.opportunisticKingdoms[UnityEngine.Random.Range(0, this.opportunisticKingdoms.Count)];
		selectedKingdom.SetBioWeapon (true);
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Plague", "bioweapon_developed");
		newLog.AddToFillers (selectedKingdom, selectedKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (null, this._plagueName, LOG_IDENTIFIER.RANDOM_GENERATED_EVENT_NAME);
	}

    private void DevelopVaccine() {
        this.isVaccineDeveloped = true;
        for (int i = 0; i < this.affectedKingdoms.Count; i++) {
            Kingdom currKingdom = this.affectedKingdoms[i];
            this.kingdomChances[currKingdom.id][0] = 5f;
        }
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Plague", "vaccine_developed");
		newLog.AddToFillers (null, this._plagueName, LOG_IDENTIFIER.RANDOM_GENERATED_EVENT_NAME);
    }

	private void DifferentApproachesLogs(Citizen citizen, EVENT_APPROACH approach, City city){
		if(citizen.city.kingdom.id == this.sourceKingdom.id || city == null){
			//This is the kingdom where the plague started
			string logName = string.Empty;
			switch(approach){
			case EVENT_APPROACH.HUMANISTIC:
				logName = "start_humanistic";
				break;
			case EVENT_APPROACH.PRAGMATIC:
				logName = "start_pragmatic";
				break;
			case EVENT_APPROACH.OPPORTUNISTIC:
				logName = "start_opportunistic";
				break;
			}
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Plague", logName);
			newLog.AddToFillers (citizen, citizen.name, LOG_IDENTIFIER.KING_1);
		}else{
			//This is the kingdom where the plague has spread
			string logName = string.Empty;
			switch(approach){
			case EVENT_APPROACH.HUMANISTIC:
				logName = "spread_humanistic";
				break;
			case EVENT_APPROACH.PRAGMATIC:
				logName = "spread_pragmatic";
				break;
			case EVENT_APPROACH.OPPORTUNISTIC:
				logName = "spread_opportunistic";
				break;
			}
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Plague", logName);
			newLog.AddToFillers (null, this._plagueName, LOG_IDENTIFIER.RANDOM_GENERATED_EVENT_NAME);
			newLog.AddToFillers (city, city.name, LOG_IDENTIFIER.CITY_1);
			newLog.AddToFillers (citizen.city.kingdom, citizen.city.kingdom.name, LOG_IDENTIFIER.KINGDOM_1);
			newLog.AddToFillers (citizen, citizen.name, LOG_IDENTIFIER.KING_1);
		}
	}
}
