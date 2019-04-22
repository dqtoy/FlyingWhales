using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unconscious : Trait {
    private Character _responsibleCharacter;
    private Character _sourceCharacter;
    private GoapPlanJob _restrainJob;
    private GoapPlanJob _removeTraitJob;

    #region getters/setters
    public override Character responsibleCharacter {
        get { return _responsibleCharacter; }
    }
    #endregion

    public Unconscious() {
        name = "Unconscious";
        description = "This character is unconscious.";
        thoughtText = "[Character] is unconscious.";
        type = TRAIT_TYPE.DISABLER;
        effect = TRAIT_EFFECT.NEGATIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 144;
        advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.FIRST_AID_CHARACTER, };
        effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void SetCharacterResponsibleForTrait(Character character) {
        _responsibleCharacter = character;
    }
    public override string GetToolTipText() {
        if (_responsibleCharacter == null) {
            return description;
        }
        return "This character has been knocked out by " + _responsibleCharacter.name;
    }
    public override void OnAddTrait(IPointOfInterest sourceCharacter) {
        base.OnAddTrait(sourceCharacter);
        if(sourceCharacter is Character) {
            _sourceCharacter = sourceCharacter as Character;
            CheckToApplyRestrainJob();
            CheckToApplyRemoveTraitJob();
        }
    }
    public override void OnRemoveTrait(IPointOfInterest sourceCharacter) {
        if (_restrainJob != null) {
            _restrainJob.jobQueueParent.CancelJob(_restrainJob);
        }
        if (_removeTraitJob != null) {
            _removeTraitJob.jobQueueParent.CancelJob(_removeTraitJob);
        }
        base.OnRemoveTrait(sourceCharacter);
    }
    #endregion

    private void CheckToApplyRestrainJob() {
        if (_sourceCharacter.homeArea.id != _sourceCharacter.specificLocation.id && !_sourceCharacter.HasJobTargettingThisCharacter("Restrain")) {
            _restrainJob = new GoapPlanJob("Restrain", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = _sourceCharacter.specificLocation, targetPOI = _sourceCharacter });
            _restrainJob.SetCanTakeThisJobChecker(CanCharacterTakeRestrainJob);
            _sourceCharacter.specificLocation.jobQueue.AddJobInQueue(_restrainJob);
        }
    }
    private bool CanCharacterTakeRestrainJob(Character character) {
        return character.role.roleType == CHARACTER_ROLE.SOLDIER || character.role.roleType == CHARACTER_ROLE.CIVILIAN || character.role.roleType == CHARACTER_ROLE.ADVENTURER;
    }

    private void CheckToApplyRemoveTraitJob() {
        if (_sourceCharacter.homeArea.id == _sourceCharacter.specificLocation.id && _sourceCharacter.faction == _sourceCharacter.specificLocation.owner && !_sourceCharacter.HasJobTargettingThisCharacter("Remove Trait", name)) {
            _removeTraitJob = new GoapPlanJob("Remove Trait", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = name, targetPOI = _sourceCharacter });
            _removeTraitJob.SetCanTakeThisJobChecker(CanCharacterTakeRemoveTraitJob);
            _sourceCharacter.specificLocation.jobQueue.AddJobInQueue(_removeTraitJob);
        }
    }
    private bool CanCharacterTakeRemoveTraitJob(Character character) {
        return _sourceCharacter != character && !character.HasRelationshipOfTypeWith(_sourceCharacter, RELATIONSHIP_TRAIT.ENEMY);
    }
}
