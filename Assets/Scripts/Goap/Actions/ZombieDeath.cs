using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieDeath : GoapAction {

    public ZombieDeath(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.ZOMBIE_DEATH, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.No_Icon;
    }

    #region Overrides
    public override void Perform() {
        base.Perform();
        SetState("Zombie Death Success");
    }
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    protected override int GetBaseCost() {
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

public class ZombieDeathData : GoapActionData {
    public ZombieDeathData() : base(INTERACTION_TYPE.ZOMBIE_DEATH) {
        //racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
    }
}