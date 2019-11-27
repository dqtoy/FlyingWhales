using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolveCombat : GoapAction {

    public ResolveCombat() : base(INTERACTION_TYPE.RESOLVE_COMBAT) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Hostile_Icon;
        doesNotStopTargetCharacter = true;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.STARTS_COMBAT, target = GOAP_EFFECT_TARGET.TARGET }, IsCombatFinished);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Unconscious", target = GOAP_EFFECT_TARGET.TARGET });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.CANNOT_MOVE, target = GOAP_EFFECT_TARGET.TARGET });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT_EFFECT, conditionKey = "Negative", target = GOAP_EFFECT_TARGET.TARGET });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode actionNode) {
        base.Perform(actionNode);
        SetState("Combat Success", actionNode);
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        string stateName = "Target Missing";
        bool defaultTargetMissing = false;
        GoapActionInvalidity goapActionInvalidity = new GoapActionInvalidity(defaultTargetMissing, stateName);
        //resolve cannot be invalid
        return goapActionInvalidity;
    }
    #endregion

    #region Effects
    public void AfterCombatSuccess(ActualGoapNode goapNode) {

    }
    #endregion

    #region Preconditions
    private bool IsCombatFinished(Character actor, IPointOfInterest target, object[] otherData) {
        if (target is Character) {
            Character targetCharcater = target as Character;
            if (targetCharcater.canMove == false || targetCharcater.canWitness == false) {
                return true;
            }
        } else {
            return target.gridTileLocation == null;
        }
        return false;
    }
    #endregion
}
