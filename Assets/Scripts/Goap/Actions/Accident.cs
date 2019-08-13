using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Accident : GoapAction {
    public GoapAction actionToDo { get; private set; }

    public Accident(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.ACCIDENT, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.No_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        SetState("Accident Success");
    }
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    protected override int GetCost() {
        return 5;
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    public override void OnResultReturnedToActor() {
        base.OnResultReturnedToActor();
        if(currentState.name == "Accident Success") {
            if(actor.currentHP <= 0) {
                actor.Death(deathFromAction: this);
            }
        }
    }
    public override bool InitializeOtherData(object[] otherData) {
        if (otherData.Length == 1 && otherData[0] is GoapAction) {
            actionToDo = otherData[0] as GoapAction;
            return true;
        }
        return base.InitializeOtherData(otherData);
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        return actor == poiTarget;
    }
    #endregion

    #region State Effects
    private void PreAccidentSuccess() {
        currentState.AddLogFiller(actionToDo, actionToDo.goapName, LOG_IDENTIFIER.STRING_1);
    }
    private void AfterAccidentSuccess() {
        actor.AddTrait("Injured", gainedFromDoing: this);

        int randomHpToLose = UnityEngine.Random.Range(5, 26);
        float percentMaxHPToLose = randomHpToLose / 100f;
        int actualHPToLose = Mathf.CeilToInt(actor.maxHP * percentMaxHPToLose);

        actor.AdjustHP(-actualHPToLose);
    }
    #endregion
}
