﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrioritizedCry : GoapAction {

    public PrioritizedCry(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.PRIORITIZED_CRY, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Entertain_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        isNotificationAnIntel = false;
    }

    #region Overrides
    public override void PerformActualAction() {
        base.PerformActualAction();
        SetState("Cry Success");
    }
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    protected override int GetCost() {
        return Utilities.rng.Next(25, 51);
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    #endregion

    #region State Effects
    private void PerTickCrySuccess() {
        actor.AdjustHappiness(500);
    }
    #endregion
}

public class PrioritizedCryData : GoapActionData {
    public PrioritizedCryData() : base(INTERACTION_TYPE.PRIORITIZED_CRY) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor == poiTarget;
    }
}