using UnityEngine;
using System.Collections;

public class DevelopWeapons : GameEvent {

    private Kingdom _sourceKingdom;

    public DevelopWeapons(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom _sourceKingdom) : base(startWeek, startMonth, startYear, startedBy) {
        eventType = EVENT_TYPES.DEVELOP_WEAPONS;
        durationInDays = Random.Range(5, 11);
        remainingDays = durationInDays;

        this._sourceKingdom = _sourceKingdom;

        EventManager.Instance.onWeekEnd.AddListener(PerformAction);

        EventManager.Instance.AddEventToDictionary(this);
        EventIsCreated();

        this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DevelopWeapons", "event_title");

        Log newLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DevelopWeapons", "start");
        newLog.AddToFillers(startedBy, startedBy.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    }

    #region Overrides
    internal override void PerformAction() {
        if(remainingDays > 0) {
            remainingDays -= 1;
        } else {
            if (Random.Range(0,100) < 50) {
                //success
                ProduceWeapons();
                AdjustRelationships();
            } else {
                //fail
                Log newLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DevelopWeapons", "development_fail");
            }
            DoneEvent();
        }
    }
    internal override void DoneEvent() {
        base.DoneEvent();
        EventManager.Instance.onWeekEnd.RemoveListener(PerformAction);
    }
    #endregion

    protected void ProduceWeapons() {
        int numOfWeaponsDeveloped = _sourceKingdom.techLevel * 2;
        _sourceKingdom.AdjustWeaponsCount(numOfWeaponsDeveloped);

        Log newLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DevelopWeapons", "development_success");
        newLog.AddToFillers(null, numOfWeaponsDeveloped.ToString(), LOG_IDENTIFIER.OTHER);
    }

    protected void AdjustRelationships() {
        //Kings
        for (int i = 0; i < _sourceKingdom.discoveredKingdoms.Count; i++) {
            Citizen otherKing = _sourceKingdom.discoveredKingdoms[i].king;
            RelationshipKings rel = otherKing.GetRelationshipWithCitizen(_sourceKingdom.king);
            if (otherKing.importantCharacterValues.ContainsKey(CHARACTER_VALUE.STRENGTH)) {
                rel.AddEventModifier(10, "Developed Weapons", this);
            } else {
                rel.AddEventModifier(-10, "Developed Weapons", this);
            }
        }

        //Governors
        for (int i = 0; i < _sourceKingdom.cities.Count; i++) {
            Governor gov = (Governor)_sourceKingdom.cities[i].governor.assignedRole;
            if (gov.citizen.importantCharacterValues.ContainsKey(CHARACTER_VALUE.STRENGTH)) {
                gov.AddEventModifier(20, "Developed Weapons", this);
            } else {
                gov.AddEventModifier(-20, "Developed Weapons", this);
            }
        }
    }
}
