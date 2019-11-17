using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class ResolveConflict : GoapAction {

    public ResolveConflict(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.RESOLVE_CONFLICT) {
        actionIconString = GoapActionStateDB.Work_Icon;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TARGET_REMOVE_RELATIONSHIP, conditionKey = "Enemy", target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Resolve Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 4;
    }
    public override GoapActionInvalidity IsInvalid(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        GoapActionInvalidity goapActionInvalidity =  base.IsInvalid(actor, poiTarget, otherData);
        if (goapActionInvalidity.isInvalid == false) {
            Character targetCharacter = poiTarget as Character;
            if ((targetCharacter.traitContainer.GetNormalTrait("Hothead") != null && UnityEngine.Random.Range(0, 2) == 0)
                || (targetCharacter.stateComponent.currentState != null &&
                (targetCharacter.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT
                || targetCharacter.stateComponent.currentState.characterState == CHARACTER_STATE.BERSERKED))) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.logKey = "resolve fail_description";
            } 
        }
        return goapActionInvalidity;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            bool hasEnemy = false;
            if (poiTarget is Character) {
                Character targetCharacter = poiTarget as Character;
                hasEnemy = targetCharacter.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.ENEMY) != null;
            }
            return actor != poiTarget && hasEnemy && actor.traitContainer.GetNormalTrait("Diplomatic") != null;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void AfterResolveSuccess(ActualGoapNode goapNode) {
        if (goapNode.poiTarget is Character) {
            Character targetCharacter = goapNode.poiTarget as Character;
            List<Relatable> allEnemyTraits = targetCharacter.relationshipContainer.GetRelatablesWithRelationship(RELATIONSHIP_TRAIT.ENEMY);
            if (allEnemyTraits.Count > 0) {
                Relatable chosenEnemy = allEnemyTraits[UnityEngine.Random.Range(0, allEnemyTraits.Count)];
                GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
                currentState.AddLogFiller(chosenEnemy, chosenEnemy.relatableName, LOG_IDENTIFIER.CHARACTER_3);
                RelationshipManager.Instance.RemoveOneWayRelationship(targetCharacter.currentAlterEgo, chosenEnemy, RELATIONSHIP_TRAIT.ENEMY);
                //NOTE: Moved removal of enemy trait after the action is fully processed for proper arrangement of logs
            } else {
                throw new System.Exception("Cannot resolve conflict for " + targetCharacter.name + " because he/she does not have enemies!");
            }
        }
    }
    #endregion
}

public class ResolveConflictData : GoapActionData {
    public ResolveConflictData() : base(INTERACTION_TYPE.RESOLVE_CONFLICT) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool hasEnemy = false;
        if (poiTarget is Character) {
            Character targetCharacter = poiTarget as Character;
            hasEnemy = targetCharacter.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.ENEMY) != null;
        }
        return actor != poiTarget && hasEnemy && actor.traitContainer.GetNormalTrait("Diplomatic") != null;
    }
}