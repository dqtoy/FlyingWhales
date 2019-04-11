using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unconscious : Trait {
    private Character _responsibleCharacter;
    private GoapPlanJob _restrainJob;

    #region getters/setters
    public override Character responsibleCharacter {
        get { return _responsibleCharacter; }
    }
    #endregion

    public Unconscious() {
        name = "Unconscious";
        description = "This character is unconscious.";
        thoughtText = "[Character] has been knocked out.";
        type = TRAIT_TYPE.DISABLER;
        effect = TRAIT_EFFECT.NEGATIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 144;
        effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void SetCharacterResponsibleForTrait(Character character) {
        _responsibleCharacter = character;
    }
    public override string GetToolTipText() {
        return "This character has been knocked out by " + _responsibleCharacter.name;
    }
    public override void OnAddTrait(IPointOfInterest sourceCharacter) {
        base.OnAddTrait(sourceCharacter);
        CheckToApplyRestrainJob(sourceCharacter);
    }
    public override void OnRemoveTrait(IPointOfInterest sourceCharacter) {
        if (_restrainJob != null) {
            _restrainJob.jobQueueParent.CancelJob(_restrainJob);
        }
        base.OnRemoveTrait(sourceCharacter);
    }
    #endregion

    private void CheckToApplyRestrainJob(IPointOfInterest poi) {
        if(poi is Character) {
            Character character = poi as Character;
            if(character.homeArea.id != character.specificLocation.id && !character.specificLocation.jobQueue.HasJobRelatedTo(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Restrained", character)) {
                _restrainJob = new GoapPlanJob("Restrain", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = character });
                _restrainJob.SetCanTakeThisJobChecker(CanCharacterTakeRestrainJob);
                character.specificLocation.jobQueue.AddJobInQueue(_restrainJob);
            }
        }
    }
    private bool CanCharacterTakeRestrainJob(Character character) {
        return character.role.roleType == CHARACTER_ROLE.SOLDIER || character.role.roleType == CHARACTER_ROLE.CIVILIAN || character.role.roleType == CHARACTER_ROLE.ADVENTURER;
    }
}
