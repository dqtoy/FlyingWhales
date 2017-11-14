using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Expansion : GameEvent {
	protected bool isExpanding = false;
	protected City originCity;

	internal HexTile hexTileToExpandTo = null;
	internal Expander expander;
	internal int carriedPopulation;
	public Expansion(int startWeek, int startMonth, int startYear, Citizen startedBy, HexTile targetHextile) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.EXPANSION;
		this.name = "Expansion";
		this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
		this.description = startedBy.city.kingdom.king.name + " is looking to expand his kingdom and has funded and expedition led by " + startedBy.name;
		this.expander = (Expander)startedBy.assignedRole;
		this.originCity = startedBy.city;
		this.hexTileToExpandTo = targetHextile;
		this.hexTileToExpandTo.isTargeted = true;
		this.carriedPopulation = 50;
		this.originCity.AdjustPopulation (-this.carriedPopulation);

		EventManager.Instance.AddEventToDictionary(this);

		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Expansion", "event_title");
		newLogTitle.AddToFillers (null, startedBy.city.kingdom.name, LOG_IDENTIFIER.KINGDOM_1);


		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Expansion", "start");
		newLog.AddToFillers (startedBy, startedBy.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		newLog.AddToFillers (startedBy.city, startedBy.city.name, LOG_IDENTIFIER.CITY_1);

        //if(UIManager.Instance.currentlyShowingKingdom.id == startedByKingdom.id) {
        //    UIManager.Instance.ShowNotification(newLog);
        //}

		this.EventIsCreated (this.startedByKingdom, false);

	}

	#region Overrides
	internal override void DoneCitizenAction(Citizen citizen){
		//CheckIfCitizenIsCarryingPlague(citizen);
		if (this.hexTileToExpandTo.city == null || this.hexTileToExpandTo.city.id == 0) {
			City newCity = this.startedByKingdom.CreateNewCityOnTileForKingdom (this.hexTileToExpandTo);
			newCity.AdjustPopulation (this.carriedPopulation);
            newCity.SetupInitialValues();
            newCity.region.SetOccupant(newCity);
            newCity.ExpandToThisCity (this.startedBy);
            newCity.region.CheckForDiscoveredKingdoms();
            newCity.kingdom.ResetCurrentExpansionRate();
//			KingdomManager.Instance.DrawConnection (this.startedBy.city.hexTile, newCity.hexTile);
            for (int i = 0; i < startedByKingdom.discoveredKingdoms.Count; i++) {
                Kingdom otherKingdom = startedByKingdom.discoveredKingdoms[i];
                if(otherKingdom.regionFogOfWarDict[newCity.region] != FOG_OF_WAR_STATE.VISIBLE) {
                    otherKingdom.SetFogOfWarStateForRegion(newCity.region, FOG_OF_WAR_STATE.SEEN);
                }
            }
            //this.CheckIfCitizenIsCarryingPlague(citizen);


			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Expansion", "expand");
            newLog.AddToFillers(this.startedByKingdom, this.startedByKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
            newLog.AddToFillers (this.hexTileToExpandTo.city, this.hexTileToExpandTo.city.name, LOG_IDENTIFIER.CITY_1);
            //if (UIManager.Instance.currentlyShowingKingdom.id == startedByKingdom.id) {
			UIManager.Instance.ShowNotification(newLog, new HashSet<Kingdom>() { startedByKingdom }, true);
            //}

        }
		this.DoneEvent ();
	}
	internal void Disappearance(){
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Expansion", "disappearance");
		this.startedBy.Death (DEATH_REASONS.DISAPPEARED_EXPANSION);
        //if (UIManager.Instance.currentlyShowingKingdom.id == startedByKingdom.id) {
        //    UIManager.Instance.ShowNotification(newLog);
        //}
        this.DoneEvent();
	}
	internal override void DeathByOtherReasons(){
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Expansion", "death_by_other");
		newLog.AddToFillers (this.startedBy, this.startedBy.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //if (UIManager.Instance.currentlyShowingKingdom.id == startedByKingdom.id) {
        //    UIManager.Instance.ShowNotification(newLog);
        //}
        this.DoneEvent ();
	}
	internal override void DeathByAgent(Citizen citizen, Citizen deadCitizen){
		base.DeathByAgent(citizen, deadCitizen);
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Expansion", "death_by_agent");
		newLog.AddToFillers (citizen, citizen.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //if (UIManager.Instance.currentlyShowingKingdom.id == startedByKingdom.id) {
        //    UIManager.Instance.ShowNotification(newLog);
        //}

        this.DoneEvent ();
	}

	internal override void DoneEvent(){
		base.DoneEvent();
		this.hexTileToExpandTo.isTargeted = false;
        //expander.citizen.Death(DEATH_REASONS.NONE);
        this.expander.DestroyGO();
        //		Messenger.RemoveListener("OnDayEnd", this.PerformAction);
    }
    internal override void CancelEvent (){
		base.CancelEvent ();
		this.DoneEvent ();
	}
	#endregion
}
