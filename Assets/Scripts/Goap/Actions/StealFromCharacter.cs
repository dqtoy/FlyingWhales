using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StealFromCharacter : GoapAction {

    private SpecialToken _targetItem;

    public StealFromCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.STEAL_CHARACTER, INTERACTION_ALIGNMENT.EVIL, actor, poiTarget) {
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.EARLY_NIGHT,
            TIME_IN_WORDS.LATE_NIGHT,
            TIME_IN_WORDS.AFTER_MIDNIGHT,
        };
        actionIconString = GoapActionStateDB.Hostile_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.NONE, targetPOI = actor });
        if (actor.GetTrait("Kleptomaniac") != null) {
            AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
        }
    }
    public override void PerformActualAction() {
        if (actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation)) {
            Character target = poiTarget as Character;
            if (target.isHoldingItem) {
                SetState("Steal Success");
            } else {
                SetState("Steal Fail");
            }
        } else {
            SetState("Target Missing");
        }
        base.PerformActualAction();
    }
    protected override int GetCost() {
        if (actor.GetTrait("Kleptomaniac") != null) {
            return 4;
        }
        return 12;
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if (poiTarget.factionOwner.id != actor.faction.id) {
            return true;
        } else if(actor.faction.id == FactionManager.Instance.neutralFaction.id) {
            return true;
        }
        return false;
    }
    #endregion

    #region State Effects
    private void PreStealSuccess() {
        Character target = poiTarget as Character;
        _targetItem = target.items[UnityEngine.Random.Range(0, target.items.Count)];
        //**Note**: This is a Theft crime
        SetCommittedCrime(CRIME.THEFT);
        currentState.AddLogFiller(_targetItem, _targetItem.name, LOG_IDENTIFIER.ITEM_1);
        //currentState.SetIntelReaction(State1Reactions);
    }
    private void AfterStealSuccess() {
        actor.PickUpToken(_targetItem, false);
        if (actor.GetTrait("Kleptomaniac") != null) {
            actor.AdjustHappiness(60);
        }
    }
    #endregion
}
