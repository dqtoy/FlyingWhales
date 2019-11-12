using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class ResolveConflict : GoapAction {

    //private RelationshipTrait chosenEnemyTrait;

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
            if(poiTarget is Character) {
                Character targetCharacter = poiTarget as Character;
                if((targetCharacter.traitContainer.GetNormalTrait("Hothead") != null && UnityEngine.Random.Range(0, 2) == 0)
                    || (targetCharacter.stateComponent.currentState != null &&
                    (targetCharacter.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT 
                    || targetCharacter.stateComponent.currentState.characterState == CHARACTER_STATE.BERSERKED))) {
                    SetState("Resolve Fail");
                } else {
                    SetState("Resolve Success");
                }
            }
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
        //if(currentState.name == "Resolve Success") {
        //    Character targetCharacter = poiTarget as Character;
        //    RelationshipManager.Instance.RemoveOneWayRelationship(targetCharacter, chosenEnemyTrait.targetCharacter, RELATIONSHIP_TRAIT.ENEMY);
        //}
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        bool hasEnemy = false;
        if(poiTarget is Character) {
            Character targetCharacter = poiTarget as Character;
            hasEnemy = targetCharacter.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.ENEMY) != null;
        }
        return actor != poiTarget && hasEnemy && actor.traitContainer.GetNormalTrait("Diplomatic") != null;
    }
    #endregion

    #region State Effects
    public void AfterResolveSuccess() {
        if (poiTarget is Character) {
            Character targetCharacter = poiTarget as Character;
            List<Relatable> allEnemyTraits = targetCharacter.relationshipContainer.GetRelatablesWithRelationship(RELATIONSHIP_TRAIT.ENEMY);
            if (allEnemyTraits.Count > 0) {
                Relatable chosenEnemy = allEnemyTraits[UnityEngine.Random.Range(0, allEnemyTraits.Count)];
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