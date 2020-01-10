using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using System.Linq;

public class SleepOutside : GoapAction {

    public SleepOutside() : base(INTERACTION_TYPE.SLEEP_OUTSIDE) {
        actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
        actionIconString = GoapActionStateDB.Sleep_Icon;
        //animationName = "Sleep Ground";
        shouldIntelNotificationOnlyIfActorIsActive = true;
        isNotificationAnIntel = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = string.Empty, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Rest Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 1000;
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        actor.traitContainer.RemoveTrait(actor, "Resting");
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
                return false;
            }
            return actor == poiTarget;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreRestSuccess(ActualGoapNode goapNode) {
        goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Resting");
        //GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
        //goapNode.OverrideCurrentStateDuration(currentState.duration - goapNode.actor.currentSleepTicks); //this can make the current duration negative
    }
    public void PerTickRestSuccess(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;
        CharacterNeedsComponent needsComponent = actor.needsComponent;
        if (needsComponent.currentSleepTicks == 1) { //If sleep ticks is down to 1 tick left, set current duration to end duration so that the action will end now, we need this because the character must only sleep the remaining hours of his sleep if ever that character is interrupted while sleeping
            goapNode.OverrideCurrentStateDuration(goapNode.currentState.duration);
        }
        needsComponent.AdjustTiredness(1.1f);
        needsComponent.AdjustSleepTicks(-1);

        float comfortAdjustment = 0f;
        if (actor.currentStructure == actor.homeStructure) {
            comfortAdjustment = 1f;
        } else if (actor.currentStructure is Dwelling && actor.currentStructure != actor.homeStructure) {
            comfortAdjustment = 0.5f;
        } else if (actor.currentStructure.structureType == STRUCTURE_TYPE.INN) {
            comfortAdjustment = 0.8f;
        } else if (actor.currentStructure.structureType == STRUCTURE_TYPE.PRISON) {
            comfortAdjustment = 0.4f;
        } else if (actor.currentStructure.structureType.IsOpenSpace()) {
            comfortAdjustment = 0.3f;
        }
        needsComponent.AdjustComfort(comfortAdjustment);
    }
    public void AfterRestSuccess(ActualGoapNode goapNode) {
        goapNode.actor.traitContainer.RemoveTrait(goapNode.actor, "Resting");
    }
    #endregion
}

public class SleepOutsideData : GoapActionData {
    public SleepOutsideData() : base(INTERACTION_TYPE.SLEEP_OUTSIDE) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        return actor == poiTarget;
    }
}