using UnityEngine;
using System.Collections;

public class Adventure : GameEvent {

    private Adventurer _adventurer;

    public Adventure(int startWeek, int startMonth, int startYear, Citizen startedBy, Adventurer adventurer) : base(startWeek, startMonth, startYear, startedBy) {
        this.eventType = EVENT_TYPES.ADVENTURE;
        _adventurer = adventurer;

        EventManager.Instance.AddEventToDictionary(this);
        EventIsCreated();

        Log newLogTitle = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Adventure", "event_title");
        Log startLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Adventure", "start");
        startLog.AddToFillers(_adventurer.citizen, _adventurer.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
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
        Log discoveredLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Adventure", "citizen_death");
        discoveredLog.AddToFillers(deadCitizen, deadCitizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        discoveredLog.AddToFillers(citizen, citizen.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        this.DoneEvent();
    }
    #endregion
}
