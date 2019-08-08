using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieDeath : GoapAction {

    public ZombieDeath(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.ZOMBIE_DEATH, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.No_Icon;
    }

    #region Overrides
    public override void PerformActualAction() {
        base.PerformActualAction();
        SetState("Zombie Death Success");
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
    #endregion

    #region Effects
    private void AfterZombieDeathSuccess() {
        actor.Death("Zombie Death", this, _deathLog:currentState.descriptionLog);
    }
    #endregion
}