using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarryCorpse : GoapAction {

    protected override bool isTargetMissing {
        get {
            bool targetMissing = poiTarget.gridTileLocation == null || actor.specificLocation != poiTarget.specificLocation
              || !(actor.gridTileLocation == poiTarget.gridTileLocation || actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation));

            if (targetMissing) {
                return targetMissing;
            } else {
                Invisible invisible = poiTarget.GetNormalTrait("Invisible") as Invisible;
                if (invisible != null && !invisible.charactersThatCanSee.Contains(actor)) {
                    return true;
                }
                return targetMissing;
            }
        }
    }

    public CarryCorpse(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.CARRY_CORPSE, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Work_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        //AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, conditionKey = poiTarget, targetPOI = poiTarget }, IsTargetDead);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.IN_PARTY, targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        //rather than checking location check if the character is not in anyone elses party and is still active
        if (!isTargetMissing && (poiTarget as Character).IsInOwnParty()) {
            SetState("Carry Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 1;
    }
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        Character targetCharacter = poiTarget as Character;
        if (actor == targetCharacter) {
            return false;
        }
        if (!targetCharacter.isDead) {
            return false;
        }
        return true;
    }
    #endregion

    #region State Effects
    public void AfterCarrySuccess() {
        Character target = poiTarget as Character;
        actor.ownParty.AddCharacter(target);
    }
    public void AfterTargetMissing() {
        actor.RemoveAwareness(poiTarget);
    }
    #endregion
}
