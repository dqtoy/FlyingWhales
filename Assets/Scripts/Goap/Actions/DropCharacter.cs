using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropCharacter : GoapAction {
    private LocationStructure _workAreaStructure;

    public override LocationStructure targetStructure {
        get { return _workAreaStructure; }
    }

    public DropCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.DROP_CHARACTER, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        _workAreaStructure = actor.homeArea.GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA);
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION_B;
        actionIconString = GoapActionStateDB.Hostile_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.IN_PARTY, conditionKey = actor, targetPOI = poiTarget }, IsInActorParty);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = actor.homeArea, targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        SetState("Drop Success");
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return 1;
    }
    public override void FailAction() {
        base.FailAction();
        SetState("Target Missing");
    }
    public override void DoAction(GoapPlan plan) {
        SetTargetStructure();
        base.DoAction(plan);
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        return actor != poiTarget;
    }
    #endregion

    #region Preconditions
    private bool IsInActorParty() {
        Character target = poiTarget as Character;
        return target.currentParty == actor.currentParty;
    }
    #endregion

    #region State Effects
    public void PreDropSuccess() {
        currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        currentState.AddLogFiller(_workAreaStructure.location, _workAreaStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterDropSuccess() {
        Character target = poiTarget as Character;
        actor.ownParty.RemoveCharacter(target);
        //target.MoveToAnotherStructure(_workAreaStructure);
    }
    #endregion
}
