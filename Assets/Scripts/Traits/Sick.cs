using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sick : Trait {
    private Character _responsibleCharacter;
    private Character _sourceCharacter;
    private GoapPlanJob _removeTraitJob;

    #region getters/setters
    public override Character responsibleCharacter {
        get { return _responsibleCharacter; }
    }
    #endregion

    public Sick() {
        name = "Sick";
        description = "This character is sick.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEGATIVE;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 480;
        advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CURE_CHARACTER, };
        effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void SetCharacterResponsibleForTrait(Character character) {
        _responsibleCharacter = character;
    }
    public override void OnAddTrait(IPointOfInterest sourceCharacter) {
        base.OnAddTrait(sourceCharacter);
        if (sourceCharacter is Character) {
            _sourceCharacter = sourceCharacter as Character;
            CheckToApplyRemoveTraitJob();
        }
    }
    public override void OnRemoveTrait(IPointOfInterest sourceCharacter) {
        if (_removeTraitJob != null) {
            _removeTraitJob.jobQueueParent.CancelJob(_removeTraitJob);
        }
        base.OnRemoveTrait(sourceCharacter);
    }
    #endregion

    private void CheckToApplyRemoveTraitJob() {
        if (_sourceCharacter.homeArea.id == _sourceCharacter.specificLocation.id && !_sourceCharacter.HasJobTargettingThisCharacter("Remove Trait", name)) {
            _removeTraitJob = new GoapPlanJob("Remove Trait", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = name, targetPOI = _sourceCharacter });
            _removeTraitJob.SetCanTakeThisJobChecker(CanCharacterTakeRemoveTraitJob);
            _sourceCharacter.specificLocation.jobQueue.AddJobInQueue(_removeTraitJob);
        }
    }
    private bool CanCharacterTakeRemoveTraitJob(Character character) {
        return !character.HasRelationshipOfTypeWith(_sourceCharacter, RELATIONSHIP_TRAIT.ENEMY);
    }
}
