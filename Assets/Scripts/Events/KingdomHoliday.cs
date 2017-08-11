using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class KingdomHoliday : GameEvent {

    private Kingdom _sourceKingdom;

    protected string[] suffixes = new string[] { "Festival", "Day", "Celebration" };
    protected string[] prefixes = new string[] { "Reincarnation", "Mothers", "Fathers", "Dragons", "Recreation", "Sunrise", "Light", "Monsters", "Remembrance", "Dead" };

    public KingdomHoliday(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom _sourceKingdom) : base(startWeek, startMonth, startYear, startedBy) {
        eventType = EVENT_TYPES.KINGDOM_HOLIDAY;
        name = GenerateHolidayName();
        durationInDays = Random.Range(7, 22);
        remainingDays = durationInDays;

        this._sourceKingdom = _sourceKingdom;

        Log newLogTitle = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "KingdomHoliday", "event_title");
        newLogTitle.AddToFillers(null, name, LOG_IDENTIFIER.RANDOM_GENERATED_EVENT_NAME);

        Initialize();

        Messenger.AddListener("OnDayEnd", PerformAction);
        EventManager.Instance.AddEventToDictionary(this);
        EventIsCreated();
    }

    protected string GenerateHolidayName() {
        return prefixes[Random.Range(0, prefixes.Length)] + " " + suffixes[Random.Range(0, suffixes.Length)];
    }

    protected void Initialize() {
        for (int i = 0; i < _sourceKingdom.cities.Count; i++) {
            City currCity = _sourceKingdom.cities[i];
            //When a kingdom is under a holiday, it's citizens rejoice and the daily growth of the kingdoms cities is increased by 100.
            currCity.AdjustDailyGrowthBuffs(100);

            //Once a holiday is started the king's governors will react, if he/she also values tradition his/her loyalty to the king will increase by 20.
            Citizen currGovernor = currCity.governor;
            if (currGovernor.importantCharacterValues.ContainsKey(CHARACTER_VALUE.TRADITION)) {
                ((Governor)currGovernor.assignedRole).AddEventModifier(20, "Kingdom Holiday Celebration", this);
            }
        }
        if (_sourceKingdom.importantCharacterValues.ContainsKey(CHARACTER_VALUE.TRADITION)) {
            _sourceKingdom.AdjustUnrest(-10);
        }

        Log newLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "KingdomHoliday", "start");
        newLog.AddToFillers(startedBy, startedBy.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        newLog.AddToFillers(null, name, LOG_IDENTIFIER.RANDOM_GENERATED_EVENT_NAME);
        newLog.AddToFillers(_sourceKingdom, _sourceKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
    }

    protected void ResetBuffs() {
        for (int i = 0; i < _sourceKingdom.cities.Count; i++) {
            City currCity = _sourceKingdom.cities[i];
            currCity.AdjustDailyGrowthBuffs(-100);
        }
    }

    protected void CheckWars() {
        List<GameEvent> activeWars = EventManager.Instance.GetAllEventsKingdomIsInvolvedIn(_sourceKingdom, new EVENT_TYPES[] { EVENT_TYPES.KINGDOM_WAR }).Where(x => x.isActive).ToList();
        if (activeWars.Count > 0) {
            CancelEvent();
        }
    }

    #region Overrides
    internal override void PerformAction() {
        CheckWars();
        if(remainingDays > 0) {
            remainingDays -= 1;
        } else {
            DoneEvent();
        }
    }

    internal override void DoneEvent() {
        base.DoneEvent();
        Messenger.RemoveListener("OnDayEnd", PerformAction);
        ResetBuffs();
        Log newLogTitle = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "KingdomHoliday", "holiday_end");
        newLogTitle.AddToFillers(null, name, LOG_IDENTIFIER.RANDOM_GENERATED_EVENT_NAME);
    }

    internal override void CancelEvent() {
        base.CancelEvent();
        Messenger.RemoveListener("OnDayEnd", PerformAction);
        ResetBuffs();
        Log newLogTitle = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "KingdomHoliday", "holiday_cancel");
        newLogTitle.AddToFillers(null, name, LOG_IDENTIFIER.RANDOM_GENERATED_EVENT_NAME);
    }
    #endregion
}
