using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolveConflict : GoapAction {

    private RelationshipTrait chosenEnemyTrait;

    public ResolveConflict(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.RESOLVE_CONFLICT, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Work_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TARGET_REMOVE_RELATIONSHIP, conditionKey = "Enemy", targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            SetState("Resolve Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 4;
    }
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    public override void OnResultReturnedToActor() {
        base.OnResultReturnedToActor();
        if(currentState.name == "Resolve Success") {
            Character targetCharacter = poiTarget as Character;
            CharacterManager.Instance.RemoveOneWayRelationship(targetCharacter, chosenEnemyTrait.targetCharacter, RELATIONSHIP_TRAIT.ENEMY);
        }
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        bool hasEnemy = false;
        if(poiTarget is Character) {
            Character targetCharacter = poiTarget as Character;
            hasEnemy = targetCharacter.HasRelationshipTraitOf(RELATIONSHIP_TRAIT.ENEMY);
        }
        return actor != poiTarget && hasEnemy && actor.GetNormalTrait("Diplomatic") != null;
    }
    #endregion

    #region State Effects
    public void PreResolveSuccess() {
        if (poiTarget is Character) {
            Character targetCharacter = poiTarget as Character;
            List<RelationshipTrait> allEnemyTraits = targetCharacter.GetAllRelationshipsOfType(actor, RELATIONSHIP_TRAIT.ENEMY);
            if (allEnemyTraits.Count > 0) {
                chosenEnemyTrait = allEnemyTraits[UnityEngine.Random.Range(0, allEnemyTraits.Count)];
                currentState.AddLogFiller(chosenEnemyTrait.targetCharacter, chosenEnemyTrait.targetCharacter.name, LOG_IDENTIFIER.CHARACTER_3);

                //NOTE: Moved removal of enemy trait after the action is fully processed for proper arrangement of logs
            } else {
                throw new System.Exception("Cannot resolve conflict for " + targetCharacter.name + " because he/she does not have enemies!");
            }
        }
    }
    #endregion
}
