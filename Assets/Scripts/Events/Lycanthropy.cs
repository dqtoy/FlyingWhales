using UnityEngine;
using System.Collections;

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

    protected void MakeCitizenLycanthrope(Citizen citizen) {
        citizen.AssignRole(ROLE.LYCANTHROPE);
        this._lycanthrope = (Lycanthrope)citizen.assignedRole;
        this._lycanthrope.Initialize(this);
    }
}
