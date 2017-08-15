using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Lycanthropy : GameEvent {

    private Lycanthrope _lycanthrope;
    private City _homeCityOfLycanthrope;
    private Kingdom _homeKingdomOfLycanthrope;

    //private Citizen _currentCaptor;

    public Lycanthropy(int startWeek, int startMonth, int startYear, Citizen startedBy, Citizen lycanthrope) : base (startWeek, startMonth, startYear, startedBy){
        this.eventType = EVENT_TYPES.LYCANTHROPY;
        this.name = Utilities.FirstLetterToUpperCase(this.eventType.ToString().ToLower());
        this.durationInDays = EventManager.Instance.eventDuration[this.eventType];

        _homeCityOfLycanthrope = lycanthrope.city;
        _homeKingdomOfLycanthrope = lycanthrope.city.kingdom;

        MakeCitizenLycanthrope(lycanthrope);

        Log newLogTitle = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Lycanthropy", "event_title");

        Log startLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Lycanthropy", "start");
        startLog.AddToFillers(_lycanthrope.citizen, _lycanthrope.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);

        EventManager.Instance.AddEventToDictionary(this);
		this.EventIsCreated (this.startedByKingdom, true);
    }

    #region overrides
    internal override void DoneCitizenAction(Citizen citizen) {
        Log transformLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Lycanthropy", "full_moon");
        transformLog.AddToFillers(_lycanthrope.citizen, _lycanthrope.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);

        base.DoneCitizenAction(citizen);
        
        Debug.LogError("LYCANTHROPY DONE CITIZEN ACTION!");
        //chance for capture
        int captureChance = 30;
        if(this._lycanthrope.targetKingdom != null) {
            //werewolf cannot be captured if it already has a captor
            captureChance = 0;
        }
        int chance = Random.Range(0, 100);
        if(chance < captureChance) {
            CapturedActions();
        } else {
            WreakHavoc();
        }

        _lycanthrope.targetLocation = null;
        _lycanthrope.targetCity = null;
        _lycanthrope.path = null;
    }

    internal override void DoneEvent() {
        base.DoneEvent();
    }
    #endregion
    protected void MakeCitizenLycanthrope(Citizen citizen) {
        citizen.AssignRole(ROLE.LYCANTHROPE);
        _lycanthrope = (Lycanthrope)citizen.assignedRole;
        _lycanthrope.Initialize(this);
    }

    /*
     * Determine what approach a citizen
     * will choose. This will not add that citizen's
     * kingdom to the appropriate list.
     * */
    private EVENT_APPROACH DetermineApproach(object obj, bool forGovernorDecision = false) {
        Dictionary<CHARACTER_VALUE, int> importantCharVals = null;
        if (obj is Citizen) {
            importantCharVals = ((Citizen)obj).importantCharacterValues;
        } else if (obj is Kingdom){
            importantCharVals = ((Kingdom)obj).importantCharacterValues;
        }

        EVENT_APPROACH chosenApproach = EVENT_APPROACH.NONE;

        if (!forGovernorDecision) {
            if (importantCharVals.ContainsKey(CHARACTER_VALUE.LIFE) || importantCharVals.ContainsKey(CHARACTER_VALUE.LAW_AND_ORDER)) {
                KeyValuePair<CHARACTER_VALUE, int> priotiyValue = importantCharVals
                    .FirstOrDefault(x => x.Key == CHARACTER_VALUE.LIFE || x.Key == CHARACTER_VALUE.LAW_AND_ORDER);

                if (priotiyValue.Key == CHARACTER_VALUE.LIFE) {
                    chosenApproach = EVENT_APPROACH.HUMANISTIC;
                } else if (priotiyValue.Key == CHARACTER_VALUE.LAW_AND_ORDER) {
                    chosenApproach = EVENT_APPROACH.PRAGMATIC;
                } else {
                    chosenApproach = EVENT_APPROACH.OPPORTUNISTIC;
                }
            } else {
                //a king who does not value any of the these values will choose OPPORTUNISTIC APPROACH in dealing with the werewolf.
                chosenApproach = EVENT_APPROACH.OPPORTUNISTIC;
            }
        } else {
            if (importantCharVals.ContainsKey(CHARACTER_VALUE.LIBERTY) || importantCharVals.ContainsKey(CHARACTER_VALUE.GREATER_GOOD)
                || importantCharVals.ContainsKey(CHARACTER_VALUE.DOMINATION)) {
                KeyValuePair<CHARACTER_VALUE, int> priotiyValue = importantCharVals
                    .FirstOrDefault(x => x.Key == CHARACTER_VALUE.LIBERTY || x.Key == CHARACTER_VALUE.GREATER_GOOD ||
                     importantCharVals.ContainsKey(CHARACTER_VALUE.DOMINATION));

                if (priotiyValue.Key == CHARACTER_VALUE.LIBERTY) {
                    chosenApproach = EVENT_APPROACH.HUMANISTIC;
                } else if (priotiyValue.Key == CHARACTER_VALUE.DOMINATION && ((Governor)((Citizen)obj).assignedRole).loyalty <= 0) {
                    chosenApproach = EVENT_APPROACH.OPPORTUNISTIC;
                } else {
                    chosenApproach = EVENT_APPROACH.PRAGMATIC;
                }
            } else {
                //a king who does not value any of the these values will choose OPPORTUNISTIC APPROACH in dealing with the werewolf.
                chosenApproach = EVENT_APPROACH.PRAGMATIC;
            }
        }
        
        return chosenApproach;
    }

    private void CapturedActions() {
        _lycanthrope.CaptureLycanthrope(_lycanthrope.targetCity.kingdom);
        Log capturedLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Lycanthropy", "captured");
        capturedLog.AddToFillers(_lycanthrope.targetCity, _lycanthrope.targetCity.name, LOG_IDENTIFIER.CITY_2);
        capturedLog.AddToFillers(_lycanthrope.citizen, _lycanthrope.citizen.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        capturedLog.AddToFillers(_lycanthrope.targetCity.kingdom.king, _lycanthrope.targetCity.kingdom.king.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);


        EVENT_APPROACH chosenApproach = DetermineApproach(_lycanthrope.captor.king);
        ChangeKingRelationshipsAfterApproach(_lycanthrope.captor.king, chosenApproach);
        ChangeLoyaltyAfterApproach(_lycanthrope.captor.king, chosenApproach);

        if (chosenApproach == EVENT_APPROACH.HUMANISTIC) {
            KingHumanisticApproach();
        } else if (chosenApproach == EVENT_APPROACH.PRAGMATIC) {
            KingPragmaticApproach();
        } else if (chosenApproach == EVENT_APPROACH.OPPORTUNISTIC) {
            KingOpportunisticApproach();
        }

    }

    private void WreakHavoc() {
        if (Random.Range(0, 100) < 80) {
            //Destroy Settlement
            HexTile tileToDestroy = this._lycanthrope.targetCity.structures[Random.Range(0, this._lycanthrope.targetCity.structures.Count)];
            _lycanthrope.targetCity.RemoveTileFromCity(tileToDestroy);
            Log destroySettlementLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Lycanthropy", "destroy_settlement");
            destroySettlementLog.AddToFillers(_lycanthrope.citizen, _lycanthrope.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            destroySettlementLog.AddToFillers(_lycanthrope.targetCity, _lycanthrope.targetCity.name, LOG_IDENTIFIER.CITY_1);
        } else {
            //Kill Citizen
            Citizen citizenToKill = this._lycanthrope.targetCity.citizens[Random.Range(0, this._lycanthrope.targetCity.citizens.Count)];
            Log killCitizenLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Lycanthropy", "kill_citizen");
            killCitizenLog.AddToFillers(_lycanthrope.citizen, _lycanthrope.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            killCitizenLog.AddToFillers(citizenToKill, citizenToKill.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            citizenToKill.Death(DEATH_REASONS.LYCANTHROPE);
        }
        this._lycanthrope.targetCity.kingdom.AdjustUnrest(10);

        //Chance for lycanthrope to die
        if(Random.Range(0,100) < 15) {
            Log lycanthropeAccident = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Lycanthropy", "lycanthrope_accident_death");
            lycanthropeAccident.AddToFillers(_lycanthrope.citizen, _lycanthrope.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            lycanthropeAccident.AddToFillers(this._lycanthrope.targetCity, this._lycanthrope.targetCity.name, LOG_IDENTIFIER.CITY_1);
            KillLycanthrope(DEATH_REASONS.ACCIDENT);
        }
    }

    #region Approaches

    #region King Approaches
    private void KingHumanisticApproach() {
        Log kingHumanisticLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Lycanthropy", "king_humanistic");
        kingHumanisticLog.AddToFillers(_lycanthrope.captor.king, _lycanthrope.captor.king.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        kingHumanisticLog.AddToFillers(_lycanthrope.citizen, _lycanthrope.citizen.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        GiveBackLycanthrope();
        GiveBackActions();
    }

    private void KingPragmaticApproach() {
        Log kingPragmaticLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Lycanthropy", "king_pragmatic");
        kingPragmaticLog.AddToFillers(_lycanthrope.captor.king, _lycanthrope.captor.king.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        kingPragmaticLog.AddToFillers(_lycanthrope.citizen, _lycanthrope.citizen.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        KillLycanthrope(DEATH_REASONS.MURDER);

    }

    private void KingOpportunisticApproach() {
        List<Kingdom> possibleTargetKingdoms = _lycanthrope.captor.king.city.kingdom.GetKingdomsByRelationship(new RELATIONSHIP_STATUS[] {
            RELATIONSHIP_STATUS.ENEMY, RELATIONSHIP_STATUS.RIVAL
        }).Where(x => _lycanthrope.captor.king.city.kingdom.discoveredKingdoms.Contains(x)).ToList();

        Kingdom targetKingdom = null;
        if(possibleTargetKingdoms.Count <= 0) {
            possibleTargetKingdoms = _lycanthrope.captor.king.city.kingdom.discoveredKingdoms;
        }

        if (possibleTargetKingdoms.Count <= 0) {
            Log kingOpportunisticFailLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Lycanthropy", "king_opportunistic_fail");
            kingOpportunisticFailLog.AddToFillers(_lycanthrope.citizen, _lycanthrope.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            kingOpportunisticFailLog.AddToFillers(_lycanthrope.captor.king, _lycanthrope.captor.king.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            
            _lycanthrope.FreeLycanthrope();
            return;
        }
        
        targetKingdom = possibleTargetKingdoms[Random.Range(0, possibleTargetKingdoms.Count)];
        _lycanthrope.SetTargetKingdom(targetKingdom);

        Log kingOpportunisticLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Lycanthropy", "king_opportunistic");
        kingOpportunisticLog.AddToFillers(_lycanthrope.captor.king, _lycanthrope.captor.king.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        kingOpportunisticLog.AddToFillers(_lycanthrope.citizen, _lycanthrope.citizen.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        kingOpportunisticLog.AddToFillers(targetKingdom, targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
    }
    #endregion

    #region Governor Approaches
    private void GovHumanisticApproach() {
        Log govHumanisticLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Lycanthropy", "gov_humanistic");
        govHumanisticLog.AddToFillers(_lycanthrope.citizen.city.governor, _lycanthrope.citizen.city.governor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        govHumanisticLog.AddToFillers(_lycanthrope.citizen, _lycanthrope.citizen.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        if (Random.Range(0, 100) < 70) {
            //lycanthrope dies
            Log govHumanisticFailLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Lycanthropy", "cure_fail");
            govHumanisticFailLog.AddToFillers(_lycanthrope.citizen.city.governor, _lycanthrope.citizen.city.governor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            govHumanisticFailLog.AddToFillers(_lycanthrope.citizen, _lycanthrope.citizen.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            KillLycanthrope(DEATH_REASONS.TRIED_TO_CURE_LYCANTHROPY);
        } else {
            //cured
            _lycanthrope.citizen.AssignRole(ROLE.UNTRAINED);
            Log govHumanisticSuccessLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Lycanthropy", "cure_success");
            govHumanisticSuccessLog.AddToFillers(_lycanthrope.citizen.city.governor, _lycanthrope.citizen.city.governor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            govHumanisticSuccessLog.AddToFillers(_lycanthrope.citizen, _lycanthrope.citizen.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
    }

    private void GovPragmaticApproach() {
        Log govPragmaticLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Lycanthropy", "gov_pragmatic");
        govPragmaticLog.AddToFillers(_lycanthrope.citizen.city.governor, _lycanthrope.citizen.city.governor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        govPragmaticLog.AddToFillers(_lycanthrope.citizen, _lycanthrope.citizen.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        if (Random.Range(0, 100) < 30) {
            //governor also dies
            Log govPragmaticFailLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Lycanthropy", "gov_pragmatic_fail");
            govPragmaticFailLog.AddToFillers(_lycanthrope.citizen.city.governor, _lycanthrope.citizen.city.governor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            govPragmaticFailLog.AddToFillers(_lycanthrope.citizen, _lycanthrope.citizen.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            _lycanthrope.citizen.city.governor.Death(DEATH_REASONS.LYCANTHROPE);
            KillLycanthrope(DEATH_REASONS.MURDER);
            return;
        }
        Log govPragmaticSuccessLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Lycanthropy", "gov_pragmatic_success");
        govPragmaticSuccessLog.AddToFillers(_lycanthrope.citizen.city.governor, _lycanthrope.citizen.city.governor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        govPragmaticSuccessLog.AddToFillers(_lycanthrope.citizen, _lycanthrope.citizen.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        KillLycanthrope(DEATH_REASONS.MURDER);
    }

    private void GovOpportunisticApproach() {
        Log govOpportunisticLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Lycanthropy", "gov_opportunistic");
        if (Random.Range(0, 100) < 30) {
            //king also dies
            Log govOpportunisticSuccessLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Lycanthropy", "gov_opportunistic_success");
            govOpportunisticSuccessLog.AddToFillers(_lycanthrope.citizen, _lycanthrope.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            govOpportunisticSuccessLog.AddToFillers(_lycanthrope.citizen.city.kingdom.king, _lycanthrope.citizen.city.kingdom.king.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            govOpportunisticSuccessLog.AddToFillers(_lycanthrope.citizen.city.governor, _lycanthrope.citizen.city.governor.name, LOG_IDENTIFIER.GOVERNOR_1);

            _lycanthrope.citizen.city.kingdom.AdjustUnrest(10);
            _lycanthrope.citizen.city.kingdom.king.Death(DEATH_REASONS.LYCANTHROPE);
            KillLycanthrope(DEATH_REASONS.MURDER);

            return;
        }
        Log govOpportunisticFailLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Lycanthropy", "gov_opportunistic_fail");
        govOpportunisticFailLog.AddToFillers(_lycanthrope.citizen.city.kingdom.king, _lycanthrope.citizen.city.kingdom.king.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        govOpportunisticFailLog.AddToFillers(_lycanthrope.citizen, _lycanthrope.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        KillLycanthrope(DEATH_REASONS.MURDER);
    }
    #endregion

    #endregion

    private void KillLycanthrope(DEATH_REASONS deathReason) {
        _lycanthrope.citizen.Death(deathReason);
        DoneEvent();
    }

    private void GiveBackLycanthrope() {
        _lycanthrope.isReturningHome = true;
        City targetCity = _homeCityOfLycanthrope;
        if (_homeCityOfLycanthrope.isDead) {
            targetCity = _lycanthrope.captor.cities[Random.Range(0, _lycanthrope.captor.cities.Count)];
        }
        targetCity.AddCitizenToCity(_lycanthrope.citizen);
    }

    private void GiveBackActions() {
        Log arriveHomeLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Lycanthropy", "arrive_home");
        arriveHomeLog.AddToFillers(_lycanthrope.citizen, _lycanthrope.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        arriveHomeLog.AddToFillers(_lycanthrope.citizen.city.governor, _lycanthrope.citizen.city.governor.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        EVENT_APPROACH governorApproach = DetermineApproach(_lycanthrope.citizen.city.governor, true);
        if (governorApproach == EVENT_APPROACH.HUMANISTIC) {
            GovHumanisticApproach();
        } else if (governorApproach == EVENT_APPROACH.PRAGMATIC) {
            GovPragmaticApproach();
        } else {
            GovOpportunisticApproach();
        }
        DoneEvent();
    }

    #region Relationships
    private void ChangeKingRelationshipsAfterApproach(Citizen citizen, EVENT_APPROACH chosenApproach) {
        for (int i = 0; i < citizen.city.kingdom.discoveredKingdoms.Count; i++) {
            Kingdom otherKingdom = citizen.city.kingdom.discoveredKingdoms[i];
            RelationshipKings rel = otherKingdom.king.GetRelationshipWithCitizen(citizen);

            if(_lycanthrope.captor.id == citizen.city.kingdom.id && 
                (_lycanthrope.targetKingdom != null &&_lycanthrope.targetKingdom.id == otherKingdom.id)) {
                rel.AddEventModifier(-5, "Lycanthrope handling", this);
            } else {
                EVENT_APPROACH otherKingApproach = this.DetermineApproach(otherKingdom.king);
                if (otherKingApproach == chosenApproach) {
                    rel.AddEventModifier(5, "Lycanthrope handling", this);
                } else {
                    rel.AddEventModifier(-5, "Lycanthrope handling", this);
                }
            }

            
        }
    }

    private void ChangeLoyaltyAfterApproach(Citizen citizen, EVENT_APPROACH chosenApproach) {
        for (int i = 0; i < citizen.city.kingdom.cities.Count; i++) {
            Governor gov = (Governor)citizen.city.kingdom.cities[i].governor.assignedRole;
            EVENT_APPROACH govApproach = this.DetermineApproach(gov.citizen);

            float multiplier = 1f;
            if(_lycanthrope.captor.id == gov.citizen.city.kingdom.id) {
                multiplier = 1.5f;
            }

            if (govApproach == chosenApproach) {
                gov.AddEventModifier((int)(5 * multiplier), "Lycanthrope handling", this);
            } else {
                gov.AddEventModifier((int)(-5 * multiplier), "Lycanthrope handling", this);
            }
        }
    }

    private void ChangeUnrestAfterApproach(Kingdom kingdom, EVENT_APPROACH chosenApproach) {
        EVENT_APPROACH approachOfKingdom = DetermineApproach(kingdom);
        if(approachOfKingdom == chosenApproach) {
            kingdom.AdjustUnrest(-10);
        } else {
            kingdom.AdjustUnrest(10);
        }
    }
    #endregion

    internal void GetTargetCity() {
        List<City> citiesToChooseFrom = new List<City>();
        if(_lycanthrope.targetKingdom != null) {
            citiesToChooseFrom = _lycanthrope.targetKingdom.cities;
        } else {
            for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
                Kingdom currKingdom = KingdomManager.Instance.allKingdoms[i];
                citiesToChooseFrom.AddRange(GetElligibleTargetCities(currKingdom));
            }
        }
        
        if(citiesToChooseFrom.Count > 0) {
            City chosenCity = citiesToChooseFrom[Random.Range(0, citiesToChooseFrom.Count)];
            this._lycanthrope.targetLocation = chosenCity.hexTile;
            this._lycanthrope.targetCity = chosenCity;
            this._lycanthrope.path = PathGenerator.Instance.GetPath(this._lycanthrope.location, chosenCity.hexTile, PATHFINDING_MODE.NORMAL);

            Log targetCityLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Lycanthropy", "target_city");
            targetCityLog.AddToFillers(_lycanthrope.citizen, _lycanthrope.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            targetCityLog.AddToFillers(chosenCity, chosenCity.name, LOG_IDENTIFIER.CITY_1);
        }
    }

    protected List<City> GetElligibleTargetCities(Kingdom kingdom) {
        List<City> elligibleCitiesInKingdom = new List<City>();
        for (int i = 0; i < kingdom.cities.Count; i++) {
            City currCity = kingdom.cities[i];
            if(PathGenerator.Instance.GetPath(this._lycanthrope.location, currCity.hexTile, PATHFINDING_MODE.NORMAL) != null) {
                elligibleCitiesInKingdom.Add(currCity);
            }
        }
        return elligibleCitiesInKingdom;
    }
}
