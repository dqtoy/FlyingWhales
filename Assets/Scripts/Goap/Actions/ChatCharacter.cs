using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatCharacter : GoapAction {
    private LocationStructure _targetStructure;
    public override LocationStructure targetStructure {
        get { return _targetStructure; }
    }
    public ChatCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.CHAT_CHARACTER, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
        };
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
    }
    public override void PerformActualAction() {
        if (poiTarget.gridTileLocation.structure == actor.gridTileLocation.structure) {
            SetState("Chat Success");
        } else {
            SetState("Target Missing");
        }
        base.PerformActualAction();
    }
    protected override int GetCost() {
        int cost = 4;
        Character targetCharacter = poiTarget as Character;
        Charmed charmed = actor.GetTrait("Charmed") as Charmed;
        if (charmed != null && charmed.responsibleCharacter != null && charmed.responsibleCharacter.id == targetCharacter.id) {
            //If Actor is charmed and target is the charmer: -2
            cost -= 2;
        }
        if (actor.faction.id != targetCharacter.faction.id) {
            //Target is not from the same faction: +2
            cost += 2;
        }
        if (actor.race != targetCharacter.race) {
            //Target is a different race: +2
            cost += 2;
        }
        CharacterRelationshipData relData = actor.GetCharacterRelationshipData(targetCharacter);
        if (relData != null) {
            if (relData.HasRelationshipTrait(RELATIONSHIP_TRAIT.ENEMY)) {
                //Target is an Enemy: +4
                cost += 4;
            }
            if (relData.HasRelationshipTrait(RELATIONSHIP_TRAIT.LOVER)) {
                //Target is a Lover: -1
                cost -= 1;
            }
            if (relData.HasRelationshipTrait(RELATIONSHIP_TRAIT.RELATIVE)) {
                //Target is a Relative: -1
                cost -= 1;
            }
            if (relData.HasRelationshipTrait(RELATIONSHIP_TRAIT.FRIEND)) {
                //Target is a Friend: -2
                cost -= 2;
            }
            if (relData.HasRelationshipTrait(RELATIONSHIP_TRAIT.PARAMOUR)) {
                //Target is a Paramour: -2
                cost -= 2;
            }
        }
        //Then add a random cost between 0 to 4.
        cost += Utilities.rng.Next(0, 4);
        return cost;
    }
    //public override bool IsHalted() {
    //    TIME_IN_WORDS timeInWords = GameManager.GetCurrentTimeInWordsOfTick();
    //    if (timeInWords == TIME_IN_WORDS.LATE_NIGHT || timeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
    //        return true;
    //    }
    //    return false;
    //}
    public override void DoAction(GoapPlan plan) {
        CharacterRelationshipData relData = actor.GetCharacterRelationshipData(poiTarget as Character);
        if (relData != null && relData.knownStructure != null) {
            _targetStructure = relData.knownStructure;
        } else {
            _targetStructure = poiTarget.gridTileLocation.structure;
        }
        base.DoAction(plan);
    }
    public override void FailAction() {
        base.FailAction();
        SetState("Target Missing");
    }
    #endregion

    #region State Effects
    private void PreChatSuccess() {
        actor.AdjustDoNotGetLonely(1);
        currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    private void PerTickChatSuccess() {
        actor.AdjustHappiness(10);
    }
    private void AfterChatSuccess() {
        actor.AdjustDoNotGetLonely(-1);
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
