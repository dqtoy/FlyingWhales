using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArgueCharacter : GoapAction {
    private LocationStructure _targetStructure;
    public override LocationStructure targetStructure {
        get { return _targetStructure; }
    }
    public ArgueCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.ARGUE_CHARACTER, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Charmed", targetPOI = actor }, ShouldNotBeCharmed);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.ADD_TRAIT, conditionKey = "Annoyed", targetPOI = poiTarget });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
    }
    public override void PerformActualAction() {
        if (poiTarget.gridTileLocation.structure == actor.gridTileLocation.structure) {
            SetState("Argue Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        int cost = 4;
        Character targetCharacter = poiTarget as Character;
        Charmed charmed = actor.GetTrait("Charmed") as Charmed;
        if (charmed != null && charmed.responsibleCharacter != null && charmed.responsibleCharacter.id == targetCharacter.id) {
            cost -=2;
        }
        if (actor.faction.id != targetCharacter.faction.id) {
            cost += 2;
        }
        if (actor.race != targetCharacter.race) {
            cost += 4;
        }
        CharacterRelationshipData relData = actor.GetCharacterRelationshipData(targetCharacter);
        if (relData != null) {
            if (relData.HasRelationshipTrait(RELATIONSHIP_TRAIT.ENEMY)) {
                cost += 8;
            }
            if (relData.HasRelationshipTrait(RELATIONSHIP_TRAIT.SERVANT)) {
                cost -= 1;
            }
            if (relData.HasRelationshipTrait(RELATIONSHIP_TRAIT.LOVER)) {
                cost -= 2;
            }
            if (relData.HasRelationshipTrait(RELATIONSHIP_TRAIT.RELATIVE)) {
                cost -= 2;
            }
            if (relData.HasRelationshipTrait(RELATIONSHIP_TRAIT.FRIEND)) {
                cost -= 3;
            }
            if (relData.HasRelationshipTrait(RELATIONSHIP_TRAIT.PARAMOUR)) {
                cost -= 3;
            }
        }
        return cost;
    }
    public override bool IsHalted() {
        TIME_IN_WORDS timeInWords = GameManager.GetCurrentTimeInWordsOfTick();
        if (timeInWords == TIME_IN_WORDS.LATE_NIGHT || timeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
            return true;
        }
        return false;
    }
    public override void DoAction(GoapPlan plan) {
        CharacterRelationshipData relData = actor.GetCharacterRelationshipData(poiTarget as Character);
        if (relData != null && relData.knownStructure != null) {
            _targetStructure = relData.knownStructure;
        } else {
            _targetStructure = poiTarget.gridTileLocation.structure;
        }
        base.DoAction(plan);
    }
    #endregion

    #region Preconditions
    private bool ShouldNotBeCharmed() {
        return actor.GetTrait("Charmed") != null;
    }
    #endregion

    #region State Effects
    private void PreArgueSuccess() {
        actor.AdjustDoNotGetLonely(1);
        currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    private void PerTickArgueSuccess() {
        actor.AdjustHappiness(4);
    }
    private void AfterArgueSuccess() {
        actor.AdjustDoNotGetLonely(-1);
        (poiTarget as Character).AddTrait("Annoyed");
    }
    private void PreTargetMissing() {
        currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    #endregion

    #region Requirement
    protected bool Requirement() {
        if (actor != poiTarget) {
            Character target = poiTarget as Character;
            return target.role.roleType != CHARACTER_ROLE.BEAST;
        }
        return false;
    }
    #endregion
}
