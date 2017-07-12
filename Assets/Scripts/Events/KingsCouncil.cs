using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class KingsCouncil : GameEvent {

    protected enum COUNCIL_REASONS { RELIGION, BUILD_RELATIONS, INEQUALITY, FREEDOM_OF_SPEECH };

    protected Kingdom _sourceKingdom;
    protected CHARACTER_VALUE _councilReasonVal;
    protected COUNCIL_REASONS _councilReason;

    protected List<Kingdom> _attendingKingdoms;
    protected List<Kingdom> _presentKingdoms;

    public KingsCouncil(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom _sourceKingdom) : base(startWeek, startMonth, startYear, startedBy) {
        eventType = EVENT_TYPES.KINGS_COUNCIL;
        this._sourceKingdom = _sourceKingdom;
        durationInDays = 5;
        remainingDays = durationInDays;
        _councilReason = GenerateCouncilReason();       

        EventManager.Instance.AddEventToDictionary(this);
        EventIsCreated();

        InviteKingdoms();
    }

    #region Overrides
    internal override void PerformAction() {
        if (_startedBy.isDead || _sourceKingdom.king.id != _startedBy.id) {
            CancelEvent();
            return;
        }
        if(remainingDays > 0) {
            remainingDays -= 1;
        } else {
            //council is done
            EventManager.Instance.onWeekEnd.RemoveListener(PerformAction);
            OnCouncilFinish();
        }
    }
    internal override void DoneCitizenAction(Citizen citizen) {
        base.DoneCitizenAction(citizen);
        _presentKingdoms.Add(citizen.city.kingdom);
        CheckIfAllKingdomsHaveArrived();
    }
    internal override void CancelEvent() {
        base.CancelEvent();
        EventManager.Instance.onWeekEnd.RemoveListener(PerformAction);
        EventManager.Instance.onKingdomDiedEvent.RemoveListener(RemoveKingdomFromGuestList);
    }
    internal override void DoneEvent() {
        base.DoneEvent();
        EventManager.Instance.onWeekEnd.RemoveListener(PerformAction);
        EventManager.Instance.onKingdomDiedEvent.RemoveListener(RemoveKingdomFromGuestList);
    }
    #endregion

    protected COUNCIL_REASONS GenerateCouncilReason() {
        KeyValuePair<CHARACTER_VALUE, int> priorityValue = startedBy.importantCharacterValues
               .FirstOrDefault(x => x.Key == CHARACTER_VALUE.LIBERTY || x.Key == CHARACTER_VALUE.PEACE);
        _councilReasonVal = priorityValue.Key;
        List<COUNCIL_REASONS> councilReasons = GetCouncilReasonForTrait(priorityValue.Key);
        return councilReasons[UnityEngine.Random.Range(0, councilReasons.Count)];
    }
    protected List<COUNCIL_REASONS> GetCouncilReasonForTrait(CHARACTER_VALUE charVal) {
        List<COUNCIL_REASONS> councilReasons = new List<COUNCIL_REASONS>();
        if(charVal == CHARACTER_VALUE.LIBERTY) {
            councilReasons.Add(COUNCIL_REASONS.INEQUALITY);
            councilReasons.Add(COUNCIL_REASONS.FREEDOM_OF_SPEECH);
        } else if (charVal == CHARACTER_VALUE.PEACE) {
            councilReasons.Add(COUNCIL_REASONS.RELIGION);
            councilReasons.Add(COUNCIL_REASONS.BUILD_RELATIONS);
        }
        return councilReasons;
    }
    protected void InviteKingdoms() {
        _attendingKingdoms = new List<Kingdom>();
        for (int i = 0; i < _sourceKingdom.discoveredKingdoms.Count; i++) {
            Kingdom currKingdom = _sourceKingdom.discoveredKingdoms[i];
            RelationshipKings currRel = currKingdom.king.GetRelationshipWithCitizen(_sourceKingdom.king);
            if(UnityEngine.Random.Range(0, 100) < (50 + currRel.totalLike)) {
                //accept invitation
                _attendingKingdoms.Add(currKingdom);
            }
        }

        if(_attendingKingdoms.Count <= 0) {
            //If none of the invited kings accepted the invitation, the source king will decrease his relationship with all of them by 10.
            for (int i = 0; i < _sourceKingdom.discoveredKingdoms.Count; i++) {
                Kingdom currKingdom = _sourceKingdom.discoveredKingdoms[i];
                RelationshipKings currRel = _sourceKingdom.king.GetRelationshipWithCitizen(currKingdom.king);
                currRel.AddEventModifier(-10, "Declined Council Invitation", this);
            }
            //Event is done
            DoneEvent();
        } else {
            List<Kingdom> kingdomsThatCannotAttend = new List<Kingdom>();
            //After all the invitations have been received and answered, all the invited kings will now send an envoy to the capital city of the king that invited them.
            for (int i = 0; i < _attendingKingdoms.Count; i++) {
                Kingdom currKingdom = _attendingKingdoms[i];
                Citizen envoy = currKingdom.capitalCity.CreateAgent(ROLE.ENVOY, eventType,_sourceKingdom.capitalCity.hexTile, EventManager.Instance.eventDuration[eventType]);
                if(envoy != null) {
                    ((Envoy)envoy.assignedRole).Initialize(this);
                } else {
                    kingdomsThatCannotAttend.Add(currKingdom);
                }
            }

            for (int i = 0; i < kingdomsThatCannotAttend.Count; i++) {
                _attendingKingdoms.Remove(kingdomsThatCannotAttend[i]);
            }
            EventManager.Instance.onKingdomDiedEvent.AddListener(RemoveKingdomFromGuestList);
        }
    }
    protected void CheckIfAllKingdomsHaveArrived() {
        if(_presentKingdoms.Count == _attendingKingdoms.Count) {
            //all kingdoms have arrived
            StartCouncil();
        }
    }
    protected void StartCouncil() {
        //When the council is started, the governors of the source king will react, those that value PEACE or LIBERTY will increase their loyalty to the king by 20.
        for (int i = 0; i < _sourceKingdom.cities.Count; i++) {
            Governor currGov = (Governor)_sourceKingdom.cities[i].governor.assignedRole;
            if(currGov.citizen.importantCharacterValues.ContainsKey(_councilReasonVal)) {
                currGov.AddEventModifier(20, "King Council Approval", this);
            }
        }
        EventManager.Instance.onWeekEnd.AddListener(PerformAction);
    }
    protected void OnCouncilFinish() {
        for (int i = 0; i < _presentKingdoms.Count; i++) {
            Kingdom currKingdom = _presentKingdoms[i];
            int baseChance = 30;
            if (currKingdom.king.importantCharacterValues.ContainsKey(_councilReasonVal)) {
                baseChance += 50;
            }
            if (UnityEngine.Random.Range(0, 100) < baseChance) {
                //approve
                currKingdom.king.GetRelationshipWithCitizen(_sourceKingdom.king).AddEventModifier(20, "Council Approval", this);
            } else {
                //disapprove
                currKingdom.king.GetRelationshipWithCitizen(_sourceKingdom.king).AddEventModifier(-20, "Council Disapproval", this);
            }
        }
        DoneEvent();
    }

    protected void RemoveKingdomFromGuestList(Kingdom kingdom) {
        if(_presentKingdoms.Contains(kingdom) || _attendingKingdoms.Contains(kingdom)) {
            _presentKingdoms.Remove(kingdom);
            _attendingKingdoms.Remove(kingdom);
            if (_presentKingdoms.Count <= 0 && _attendingKingdoms.Count <= 0) {
                CancelEvent();
            }
        }
    }
}
