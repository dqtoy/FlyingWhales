using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EvilIntent : GameEvent {

    private Citizen _sourceKing;
    private Citizen _targetKing;

    public EvilIntent(int startWeek, int startMonth, int startYear, Citizen startedBy, Citizen sourceKing, Citizen targetKing) : base(startWeek, startMonth, startYear, startedBy) {
        eventType = EVENT_TYPES.EVIL_INTENT;

        _sourceKing = sourceKing;
        _targetKing = targetKing;

        EventManager.Instance.AddEventToDictionary(this);
        EventIsCreated();
    }

    #region Overrides
    internal override void PerformAction() {
        base.PerformAction();
    }
    #endregion

    /*
     * Determine action of first king when he/she is chosen
     * for evil intent.
     * */
    private void DetermineFirstAction() {
        if (_sourceKing.importantCharacterValues.ContainsKey(CHARACTER_VALUE.PEACE) || _sourceKing.importantCharacterValues.ContainsKey(CHARACTER_VALUE.DOMINATION)) {
            KeyValuePair<CHARACTER_VALUE, int> priotiyValue = _sourceKing.importantCharacterValues
                .FirstOrDefault(x => x.Key == CHARACTER_VALUE.PEACE || x.Key == CHARACTER_VALUE.DOMINATION);
            if (priotiyValue.Key == CHARACTER_VALUE.PEACE) {
                ChooseToResist();
            } else {
                ChooseToFeed();
            }
        } else {
            ChooseToFeed();
        }
    }

    private void ChooseToResist() {
        durationInDays = 5;
        remainingDays = durationInDays;
        EventManager.Instance.onWeekEnd.AddListener(Resist);
    }

    private void Resist() {
        durationInDays -= 1;
        if(durationInDays <= 0) {
            EventManager.Instance.onWeekEnd.RemoveListener(Resist);
            if(Random.Range(0, 100) < 60) {
                //Success
                for (int i = 0; i < _sourceKing.relationshipKings.Count; i++) {
                    Citizen otherKing = _sourceKing.relationshipKings[i].king;
                    RelationshipKings otherKingRel = otherKing.GetRelationshipWithCitizen(_sourceKing);
                    otherKingRel.AddEventModifier(20, "Evil Intent Reaction", this);
                }
                DoneEvent();
            } else {
                //Fail
                _sourceKing.Death(DEATH_REASONS.EVIL_INTENT);
                DoneEvent();
            }
        }
    }

    private void ChooseToFeed() {

    }
}
