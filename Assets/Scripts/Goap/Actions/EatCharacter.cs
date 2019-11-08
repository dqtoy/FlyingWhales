using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatCharacter : GoapAction {
    protected override string failActionState { get { return "Eat Fail"; } }

    public EatCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.EAT_CHARACTER, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        //actionLocationType = ACTION_LOCATION_TYPE.ON_TARGET;
        actionIconString = GoapActionStateDB.Eat_Icon;
    }

    #region Overrides
    //protected override void ConstructRequirementOnBuildGoapTree() {
    //    _requirementOnBuildGoapTreeAction = RequirementOnBuildGoapTree;
    //}
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Unconscious", targetPOI = poiTarget }, HasUnconsciousOrDeadTarget);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = actor });
    }
    public override void Perform() {
        base.Perform();
        if (!isTargetMissing) {
            Character target = poiTarget as Character;
            if (target.GetNormalTrait("Unconscious", "Dead") != null) {
                SetState("Eat Success");
            } else {
                SetState("Eat Fail");
            }
        } else {
            if (!poiTarget.IsAvailable()) {
                SetState("Eat Fail");
            } else {
                SetState("Target Missing");
            }
        }
    }
    protected override int GetBaseCost() {
        if (poiTarget is Character) {
            Character target = poiTarget as Character;
            RELATIONSHIP_EFFECT relEffect = actor.GetRelationshipEffectWith(target);
            if (relEffect == RELATIONSHIP_EFFECT.NEGATIVE) {
                return 15;
            }else if (relEffect == RELATIONSHIP_EFFECT.NONE) {
                return 20;
            }
        }
        return 15;
    }
    public override void OnStopActionDuringCurrentState() {
        if (currentState.name == "Eat Success") {
            actor.AdjustDoNotGetHungry(-1);
        }
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if(actor != poiTarget) {
            if (actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
                return false;
            }
            if (poiTarget is Character) {
                Character target = poiTarget as Character;
                if (actor.race == target.race) {
                    RELATIONSHIP_EFFECT relEffect = actor.GetRelationshipEffectWith(target);
                    if (relEffect == RELATIONSHIP_EFFECT.NONE || relEffect == RELATIONSHIP_EFFECT.NEGATIVE) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    #endregion

    #region Preconditions
    private bool HasUnconsciousOrDeadTarget() {
        Character target = poiTarget as Character;
        return target.GetNormalTrait("Unconscious", "Dead") != null;
    }
    #endregion

    #region Effects
    private void PreEatSuccess() {
        SetCommittedCrime(CRIME.ABERRATION, new Character[] { actor });
        //poiTarget.SetPOIState(POI_STATE.INACTIVE);
        actor.AdjustDoNotGetHungry(1);
        //currentState.SetIntelReaction(DrinkBloodSuccessIntelReaction);
    }
    private void PerTickEatSuccess() {
        actor.AdjustFullness(12);
    }
    private void AfterEatSuccess() {
        actor.AdjustDoNotGetHungry(-1);
        if(poiTarget.GetNormalTrait("Unconscious") != null) {
            SetCannotCancelAction(true);
            Character target = poiTarget as Character;
            target.Death("eaten", deathFromAction: this, responsibleCharacter: actor);
        }
    }
    private void PreEatFail() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    //private void PreTargetMissing() {
    //    currentState.AddLogFiller(actor.currentStructure.location, actor.currentStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    //private void AfterTargetMissing() {
    //    actor.RemoveAwareness(poiTarget);
    //}
    #endregion
}

public class EatCharacterData : GoapActionData {
    public EatCharacterData() : base(INTERACTION_TYPE.EAT_CHARACTER) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (actor != poiTarget) {
            if (poiTarget.gridTileLocation == null) {
                return false;
            }
            if (actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
                return false;
            }
            if (!actor.isStarving) {
                return false;
            }
            if (actor.GetNormalTrait("Cannibal") == null) {
                return false;
            }
            if (poiTarget is Character) {
                Character target = poiTarget as Character;
                if (actor.race == target.race) {
                    RELATIONSHIP_EFFECT relEffect = actor.GetRelationshipEffectWith(target);
                    if (relEffect == RELATIONSHIP_EFFECT.NONE || relEffect == RELATIONSHIP_EFFECT.NEGATIVE) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
}
