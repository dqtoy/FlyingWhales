using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stumble : GoapAction {

    public Stumble(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.STUMBLE, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.No_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        SetState("Stumble Success");
    }
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    protected override int GetCost() {
        return 10;
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    public override void OnResultReturnedToActor() {
        base.OnResultReturnedToActor();
        if (currentState.name == "Stumble Success") {
            if (actor.currentHP <= 0) {
                actor.Death(deathFromAction: this);
            }
        }
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        return actor == poiTarget;
    }
    #endregion

    #region State Effects
    private void PerTickStumbleSuccess() {
        int randomHpToLose = UnityEngine.Random.Range(1, 6);
        float percentMaxHPToLose = randomHpToLose / 100f;
        int actualHPToLose = Mathf.CeilToInt(actor.maxHP * percentMaxHPToLose);

        actor.AdjustHP(-actualHPToLose);
    }
    #endregion
}
