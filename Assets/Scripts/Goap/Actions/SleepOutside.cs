﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SleepOutside : GoapAction {
    public SleepOutside(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.SLEEP_OUTSIDE, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
        actionIconString = GoapActionStateDB.Sleep_Icon;
        //animationName = "Sleep Ground";
        shouldIntelNotificationOnlyIfActorIsActive = true;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = null, targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        //if (targetTile != null) {
            SetState("Rest Success");
        //} else {
        //    SetState("Rest Fail");
        //}
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    protected override int GetCost() {
        return 18;
    }
    //public override void SetTargetStructure() {
    //    List<LocationStructure> choices = actor.specificLocation.GetStructuresOfType(STRUCTURE_TYPE.WILDERNESS).Where(x => x.unoccupiedTiles.Count > 0).ToList();
    //    if (actor.specificLocation.HasStructure(STRUCTURE_TYPE.DWELLING)) {
    //        choices.AddRange(actor.specificLocation.GetStructuresOfType(STRUCTURE_TYPE.DWELLING).Where(x => x.unoccupiedTiles.Count > 0 && !x.IsOccupied()));
    //    }
    //    _targetStructure = choices[Utilities.rng.Next(0, choices.Count)];
    //    base.SetTargetStructure();
    //}
    public override void OnStopActionDuringCurrentState() {
        if (currentState.name == "Rest Success") {
            RemoveTraitFrom(actor, "Resting");
        }
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        return actor == poiTarget;
    }
    #endregion

    #region State Effects
    private void PreRestSuccess() {
        //currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        //actor.AdjustDoNotGetTired(1);
        Resting restingTrait = new Resting();
        actor.AddTrait(restingTrait);
        currentState.SetAnimation("Sleep Ground");
    }
    private void PerTickRestSuccess() {
        actor.AdjustTiredness(3);
    }
    private void AfterRestSuccess() {
        //actor.AdjustDoNotGetTired(-1);
        RemoveTraitFrom(actor, "Resting");
    }
    //private void PreRestFail() {
    //    currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    #endregion
}
