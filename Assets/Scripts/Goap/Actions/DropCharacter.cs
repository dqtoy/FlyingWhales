using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropCharacter : GoapAction {
    private LocationStructure _workAreaStructure;

    public override LocationStructure targetStructure {
        get { return _workAreaStructure; }
    }

    public DropCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.DROP_CHARACTER, actor, poiTarget) {
        _workAreaStructure = actor.homeArea.GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA);
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.IN_PARTY, conditionKey = actor, targetPOI = poiTarget }, IsInActorParty);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = actor.homeArea, targetPOI = poiTarget });
    }
    public override bool PerformActualAction() {
        if (base.PerformActualAction()) {
            Character target = poiTarget as Character;
            actor.ownParty.RemoveCharacter(target);
            target.MoveToAnotherStructure(_workAreaStructure);
            return true;
        }
        return false;
    }
    protected override int GetCost() {
        return 1;
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
}
