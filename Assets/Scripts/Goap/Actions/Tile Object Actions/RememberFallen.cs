using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RememberFallen : GoapAction {
    protected override string failActionState { get { return "Target Missing"; } }

    public RememberFallen(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.REMEMBER_FALLEN, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Entertain_Icon;
        isNotificationAnIntel = false;
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
            SetState("Remember Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        //**Cost**: randomize between 5-35
        return Utilities.rng.Next(5, 36);
    }
    protected override void AddDefaultObjectsToLog(Log log) {
        log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        Tombstone tombstone = poiTarget as Tombstone;
        log.AddToFillers(tombstone.character, tombstone.character.name, LOG_IDENTIFIER.TARGET_CHARACTER); //Target character is only the identifier but it doesn't mean that this is a character, it can be item, etc.
        if (targetStructure != null) {
            log.AddToFillers(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        } else {
            log.AddToFillers(actor.specificLocation, actor.specificLocation.name, LOG_IDENTIFIER.LANDMARK_1);
        }
    }
    public override void OnStopActionDuringCurrentState() {
        if (currentState.name == "Remember Success") {
            actor.AdjustDoNotGetLonely(-1);
        }
    }
    #endregion

    #region Requirement
    protected bool Requirement() {
        if (!poiTarget.IsAvailable() || poiTarget.gridTileLocation == null) {
            return false;
        }
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        if (poiTarget is Tombstone) {
            Tombstone tombstone = poiTarget as Tombstone;
            Character target = tombstone.character;
            return actor.HasRelationshipOfEffectWith(target, TRAIT_EFFECT.POSITIVE);
        }
        return false;
    }
    #endregion

    #region Effects
    private void PreRememberSuccess() {
        Tombstone tombstone = poiTarget as Tombstone;
        currentState.AddLogFiller(null, tombstone.character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        actor.AdjustDoNotGetLonely(1);
    }
    private void PerTickRememberSuccess() {
        actor.AdjustHappiness(16);
    }
    private void AfterRememberSuccess() {
        actor.AdjustDoNotGetLonely(-1);
    }
    private void PreTargetMissing() {
        Tombstone tombstone = poiTarget as Tombstone;
        currentState.AddLogFiller(null, tombstone.character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    #endregion
}

public class RememberFallenData : GoapActionData {
    public RememberFallenData() : base(INTERACTION_TYPE.REMEMBER_FALLEN) {
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (!poiTarget.IsAvailable() || poiTarget.gridTileLocation == null) {
            return false;
        }
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        if (poiTarget is Tombstone) {
            Tombstone tombstone = poiTarget as Tombstone;
            Character target = tombstone.character;
            return actor.HasRelationshipOfEffectWith(target, TRAIT_EFFECT.POSITIVE);
        }
        return false;
    }
}