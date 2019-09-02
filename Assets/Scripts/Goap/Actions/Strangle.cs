using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Strangle : GoapAction {

    private LocationStructure _targetStructure;
    public override LocationStructure targetStructure {
        get { return _targetStructure; }
    }

    public Strangle(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.STRANGLE, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Sleep_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    public override void SetTargetStructure() {
        _targetStructure = actor.homeStructure;
        if (_targetStructure == null) {
            _targetStructure = actor.specificLocation.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
        }
        base.SetTargetStructure();
    }
    public override LocationGridTile GetTargetLocationTile() {
        if (actor.currentStructure == targetStructure) {
            return actor.gridTileLocation;
        }
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        //if (!isTargetMissing) {
            SetState("Strangle Success");
        //} else {
        //    SetState("Target Missing");
        //}
    }
    protected override int GetCost() {
        return 2;
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        return poiTarget == actor && poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
    }
    #endregion

    #region State Effects
    public void PerTickStrangleSuccess() {
        actor.AdjustHP(-(int)((float)actor.maxHP * 0.18f));
    }
    public void AfterStrangleSuccess() {
        actor.Death("suicide", this, _deathLog: currentState.descriptionLog);
    }
    #endregion
}

public class StrangleData : GoapActionData {
    public StrangleData() : base(INTERACTION_TYPE.STRANGLE) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return poiTarget == actor && poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
    }
}
