using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spit : GoapAction {
    protected override string failActionState { get { return "Target Missing"; } }

    public Spit(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.SPIT, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Entertain_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            SetState("Spit Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        //**Cost**: randomize between 5-35
        return Utilities.rng.Next(5, 36);
    }
    #endregion

    #region Requirement
    protected bool Requirement() {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        if (poiTarget is Tombstone) {
            Tombstone tombstone = poiTarget as Tombstone;
            Character target = tombstone.character;
            return actor.HasRelationshipOfEffectWith(target, TRAIT_EFFECT.NEGATIVE);
        }
        return false;
    }
    #endregion

    #region Effects
    private void PreSpitSuccess() {
        Tombstone tombstone = poiTarget as Tombstone;
        currentState.AddLogFiller(null, tombstone.character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    private void AfterSpitSuccess() {
        actor.AdjustHappiness(75);
    }
    private void PreTargetMissing() {
        Tombstone tombstone = poiTarget as Tombstone;
        currentState.AddLogFiller(null, tombstone.character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    #endregion
}
