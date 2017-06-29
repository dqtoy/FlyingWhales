using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Lycanthropy : GameEvent {

    private Lycanthrope _lycanthrope;

    public Lycanthropy(int startWeek, int startMonth, int startYear, Citizen startedBy, Citizen lycanthrope) : base (startWeek, startMonth, startYear, startedBy){
        this.eventType = EVENT_TYPES.LYCANTHROPY;
        this.name = Utilities.FirstLetterToUpperCase(this.eventType.ToString().ToLower());
        this.durationInDays = EventManager.Instance.eventDuration[this.eventType];

        MakeCitizenLycanthrope(lycanthrope);

        Log newLogTitle = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Lycanthropy", "event_title");

        EventManager.Instance.AddEventToDictionary(this);
        this.EventIsCreated();
    }

    #region overrides
    internal override void DoneCitizenAction(Citizen citizen) {
        base.DoneCitizenAction(citizen);
        
        Debug.LogError("LYCANTHROPY DONE CITIZEN ACTION!");
        //chance for capture
        int captureChance = 10;
        int chance = Random.Range(0, 100);
        if(chance < captureChance) {

        } else {
            //Wreak havoc
            if(Random.Range(0,100) < 80) {
                //Destroy Settlement
                HexTile tileToDestroy = this._lycanthrope.targetCity.structures[Random.Range(0, this._lycanthrope.targetCity.structures.Count)];
                this._lycanthrope.targetCity.RemoveTileFromCity(tileToDestroy);
            } else {
                //Kill Citizen
                Citizen citizenToKill = this._lycanthrope.targetCity.citizens[Random.Range(0, this._lycanthrope.targetCity.citizens.Count)];
                citizenToKill.Death(DEATH_REASONS.LYCANTHROPE);
            }
            this._lycanthrope.targetCity.kingdom.AdjustUnrest(10);
        }

        this._lycanthrope.targetLocation = null;
        this._lycanthrope.targetCity = null;
        this._lycanthrope.path = null;
    }
    #endregion
    protected void MakeCitizenLycanthrope(Citizen citizen) {
        citizen.AssignRole(ROLE.LYCANTHROPE);
        this._lycanthrope = (Lycanthrope)citizen.assignedRole;
        this._lycanthrope.Initialize(this);
    }

    /*
     * Determine what approach a citizen
     * will choose. This will not add that citizen's
     * kingdom to the appropriate list.
     * */
    private EVENT_APPROACH DetermineApproach(Citizen citizen) {
        Dictionary<CHARACTER_VALUE, int> importantCharVals = citizen.importantCharacterValues;

        EVENT_APPROACH chosenApproach = EVENT_APPROACH.NONE;
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
        return chosenApproach;
    }

    internal void GetTargetCity() {
        List<City> citiesToChooseFrom = new List<City>();
        for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
            Kingdom currKingdom = KingdomManager.Instance.allKingdoms[i];
            citiesToChooseFrom.AddRange(GetElligibleTargetCities(currKingdom));
        }
        if(citiesToChooseFrom.Count > 0) {
            City chosenCity = citiesToChooseFrom[Random.Range(0, citiesToChooseFrom.Count)];
            this._lycanthrope.targetLocation = chosenCity.hexTile;
            this._lycanthrope.targetCity = chosenCity;
            this._lycanthrope.path = PathGenerator.Instance.GetPath(this._lycanthrope.location, chosenCity.hexTile, PATHFINDING_MODE.NORMAL);
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
