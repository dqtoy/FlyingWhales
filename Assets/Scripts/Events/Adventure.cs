using UnityEngine;
using System.Collections;

public class Adventure : GameEvent {

    private Adventurer _adventurer;

    public Adventure(int startWeek, int startMonth, int startYear, Citizen startedBy, Adventurer adventurer) : base(startWeek, startMonth, startYear, startedBy) {
        this.eventType = EVENT_TYPES.ADVENTURE;
		this.name = "Adventure";
        _adventurer = adventurer;

		Log newLogTitle = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Adventure", "event_title");
		Log startLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Adventure", "start");
		startLog.AddToFillers(_adventurer.citizen, _adventurer.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);

        EventManager.Instance.AddEventToDictionary(this);
        //EventIsCreated();
		EventIsCreated(this.startedByKingdom, false);

    }

    #region Overrides
    internal override void DoneCitizenAction(Citizen citizen) {
        base.DoneCitizenAction(citizen);
        //adventurer has arrived at target tile
        if(((Adventurer)citizen.assignedRole).latestDiscovery != null) {
            Log discoveredLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Adventure", "citizen_discovered");
            discoveredLog.AddToFillers(citizen, citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            discoveredLog.AddToFillers(null, ((Adventurer)citizen.assignedRole).latestDiscovery.ToString(), LOG_IDENTIFIER.OTHER);
            ((Adventurer)citizen.assignedRole).SetLatestDiscovery(null);
        }
    }

    internal override void DeathByAgent(Citizen citizen, Citizen deadCitizen) {
        base.DeathByAgent(citizen, deadCitizen);
        Log discoveredLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Adventure", "citizen_death_by_agent");
        discoveredLog.AddToFillers(deadCitizen, deadCitizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        discoveredLog.AddToFillers(citizen, citizen.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        //if (UIManager.Instance.currentlyShowingKingdom.id == startedByKingdom.id) {
        //    UIManager.Instance.ShowNotification(discoveredLog);
        //}

        this.DoneEvent();
    }

    internal override void DoneEvent() {
        base.DoneEvent();
        //_adventurer.citizen.Death(DEATH_REASONS.NONE);
        Log discoveredLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Adventure", "citizen_death");
        discoveredLog.AddToFillers(_adventurer.citizen, _adventurer.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);

        //if (UIManager.Instance.currentlyShowingKingdom.id == startedByKingdom.id) {
        //    UIManager.Instance.ShowNotification(discoveredLog);
        //}
        _adventurer.DestroyGO();
    }
    #endregion
}
