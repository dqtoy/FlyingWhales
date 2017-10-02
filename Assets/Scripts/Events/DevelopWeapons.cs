using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class DevelopWeapons : GameEvent {

    internal GameObject avatar;

    //private HexTile _weaponLocation;
    private Kingdom _sourceKingdom;

    private CHARACTER_VALUE chosenValue;

    public DevelopWeapons(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom sourceKingdom) : base(startWeek, startMonth, startYear, startedBy) {
        eventType = EVENT_TYPES.DEVELOP_WEAPONS;
        durationInDays = Random.Range(5, 11);
        remainingDays = durationInDays;
        //name = "Sacred Weapon"
        _sourceKingdom = sourceKingdom;
        //_weaponLocation = weaponLocation;
        //WorldEventManager.Instance.AddWorldEvent(this);
		this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DevelopWeapons", "event_title");
        ClaimWeapon(_sourceKingdom);
        //Initialize();
    }

    #region Overrides
    internal override void PerformAction() {
        if(remainingDays > 0) {
            remainingDays -= 1;
        } else {
            if(_sourceKingdom.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.STRENGTH) 
                || _sourceKingdom.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.TRADITION)) {

                KeyValuePair<CHARACTER_VALUE, int> priorityValue = _sourceKingdom.king.importantCharacterValues.FirstOrDefault(x => x.Key == CHARACTER_VALUE.STRENGTH
                || x.Key == CHARACTER_VALUE.TRADITION);

                if(priorityValue.Key == CHARACTER_VALUE.STRENGTH) {
                    ProduceWeapons();
                } else {
                    HideWeapons();
                }
            }
            DoneEvent();
        }
    }
    internal override void DoneEvent() {
        base.DoneEvent();
        Messenger.RemoveListener("OnDayEnd", PerformAction);
    }
    #endregion

    //protected void Initialize() {
    //    _weaponLocation.PutEventOnTile(this);
    //}

    internal void ClaimWeapon(Kingdom claimant) {
        //SetStartedBy(claimant.king);
        //_sourceKingdom = claimant;
        //GameObject.Destroy(this.avatar);
        //_weaponLocation.RemoveEventOnTile();

        Log newLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DevelopWeapons", "start");
        newLog.AddToFillers(startedBy, startedBy.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);

        GameDate dueDate = Utilities.GetNewDateAfterNumberOfDays(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, durationInDays);
        //SchedulingManager.Instance.AddEntry

        Messenger.AddListener("OnDayEnd", PerformAction);
        EventManager.Instance.AddEventToDictionary(this);
		EventIsCreated(claimant, true);
    }

    protected void ProduceWeapons() {
        chosenValue = CHARACTER_VALUE.STRENGTH;
        Log prduceWeaponsLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DevelopWeapons", "produce_weapons_start");
        prduceWeaponsLog.AddToFillers(_sourceKingdom.king, _sourceKingdom.king.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);

        if (Random.Range(0, 100) < 50) {
            //success
            int numOfWeaponsDeveloped = 5;
            _sourceKingdom.AdjustWeaponsCount(numOfWeaponsDeveloped);
            Log newLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DevelopWeapons", "development_success");
            newLog.AddToFillers(null, numOfWeaponsDeveloped.ToString(), LOG_IDENTIFIER.OTHER);
            AdjustRelationships();
        } else {
            //fail
            Log newLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DevelopWeapons", "development_fail");
        }
    }

    protected void HideWeapons() {
        chosenValue = CHARACTER_VALUE.TRADITION;
        Log prduceWeaponsLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DevelopWeapons", "hide_weapons_start");
        prduceWeaponsLog.AddToFillers(_sourceKingdom.king, _sourceKingdom.king.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);

        _sourceKingdom.AdjustWeaponsCount(1);
        AdjustRelationships();
    }

    protected void AdjustRelationships() {
        //Kings
        for (int i = 0; i < _sourceKingdom.discoveredKingdoms.Count; i++) {
            Kingdom otherKingdom = _sourceKingdom.discoveredKingdoms[i];
            Citizen otherKing = otherKingdom.king;
            KingdomRelationship rel = otherKingdom.GetRelationshipWithKingdom(_sourceKingdom);
            if (otherKing.importantCharacterValues.ContainsKey(CHARACTER_VALUE.STRENGTH)
                || otherKing.importantCharacterValues.ContainsKey(CHARACTER_VALUE.TRADITION)) {

                KeyValuePair<CHARACTER_VALUE, int> priorityValue = otherKing.importantCharacterValues.FirstOrDefault(x => x.Key == CHARACTER_VALUE.STRENGTH
                || x.Key == CHARACTER_VALUE.TRADITION);
                if(priorityValue.Key == chosenValue) {
                    rel.AddEventModifier(5, "Developed Weapons", this);
                } else {
                    rel.AddEventModifier(-5, "Developed Weapons", this);
                }
                
            }
        }

        //Governors
        for (int i = 0; i < _sourceKingdom.cities.Count; i++) {
            Governor gov = (Governor)_sourceKingdom.cities[i].governor.assignedRole;
            if (gov.citizen.importantCharacterValues.ContainsKey(CHARACTER_VALUE.STRENGTH)
                || gov.citizen.importantCharacterValues.ContainsKey(CHARACTER_VALUE.TRADITION)) {
                KeyValuePair<CHARACTER_VALUE, int> priorityValue = gov.citizen.importantCharacterValues.FirstOrDefault(x => x.Key == CHARACTER_VALUE.STRENGTH
                || x.Key == CHARACTER_VALUE.TRADITION);

                if (priorityValue.Key == chosenValue) {
                    gov.AddEventModifier(5, "Developed Weapons", this);
                } else {
                    gov.AddEventModifier(-5, "Developed Weapons", this);
                }
            }
        }

        //Kingdom
        if(_sourceKingdom.importantCharacterValues.ContainsKey(CHARACTER_VALUE.STRENGTH) ||
            _sourceKingdom.importantCharacterValues.ContainsKey(CHARACTER_VALUE.TRADITION)) {
            KeyValuePair<CHARACTER_VALUE, int> priorityValue = _sourceKingdom.importantCharacterValues.FirstOrDefault(x => x.Key == CHARACTER_VALUE.STRENGTH
                || x.Key == CHARACTER_VALUE.TRADITION);

            if (priorityValue.Key == chosenValue) {
                _sourceKingdom.AdjustStability(10);
            } else {
                _sourceKingdom.AdjustStability(-10);
            }
        }
    }
}
