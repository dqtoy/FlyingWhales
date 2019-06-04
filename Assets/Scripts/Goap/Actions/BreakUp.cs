using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakUp : GoapAction {

    public Character targetCharacter { get; private set; }

    public BreakUp(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.BREAK_UP, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Social_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        //**Effect 1**: Actor - Remove Lover relationship with Target
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TARGET_REMOVE_RELATIONSHIP, conditionKey = "Lover", targetPOI = actor });
        //**Effect 2**: Actor - Remove Paramour relationship with Target
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TARGET_REMOVE_RELATIONSHIP, conditionKey = "Paramour", targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            SetState("Break Up Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 1;
    }
    #endregion

    #region Effects
    private void PreBreakUpSuccess() {
        Character target = poiTarget as Character;
        if (actor.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.LOVER)) {
            //**Effect 1**: Actor - Remove Lover relationship with Character 2
            CharacterManager.Instance.RemoveRelationshipBetween(actor, target, RELATIONSHIP_TRAIT.LOVER);
            //if the relationship that was removed is lover, change home to a random unoccupied dwelling,
            //otherwise, no home. Reference: https://trello.com/c/JUSt9bEa/1938-broken-up-characters-should-live-in-separate-house
            actor.MigrateHomeStructureTo(null);
            actor.homeArea.AssignCharacterToDwellingInArea(actor);
        } else {
            //**Effect 2**: Actor - Remove Paramour relationship with Character 2
            CharacterManager.Instance.RemoveRelationshipBetween(actor, target, RELATIONSHIP_TRAIT.PARAMOUR);
        }
        //**Effect 3**: Target gains Heartbroken trait
        AddTraitTo(target, "Heartbroken", actor);
    }
    private void PreTargetMissing() {
        //currentState.AddLogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    #endregion

    #region Requirement
    protected bool Requirement() {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        Character target = poiTarget as Character;
        if (target == actor) {
            return false;
        }
        if (target.currentAlterEgoName != CharacterManager.Original_Alter_Ego) {
            return false;
        }
        if (target.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
            return false;
        }
        if (!actor.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.LOVER) && !actor.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.PARAMOUR)) {
            return false; //**Advertised To**: All characters with Lover or Paramour relationship with the character
        }
        return target.IsInOwnParty();
    }
    #endregion
}
