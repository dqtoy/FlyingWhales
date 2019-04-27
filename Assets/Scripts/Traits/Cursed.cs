using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursed : Trait {
    private Character _responsibleCharacter;
    private Character _sourceCharacter;
    private GoapPlanJob _removeTraitJob;

    #region getters/setters
    public override Character responsibleCharacter {
        get { return _responsibleCharacter; }
    }
    #endregion

    public Cursed() {
        name = "Cursed";
        description = "This character has been afflicted with a debilitating curse.";
        type = TRAIT_TYPE.ENCHANTMENT;
        effect = TRAIT_EFFECT.NEGATIVE;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
        advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.DISPEL_MAGIC, };
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
            _sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, name.ToLower());
        }
    }
    public override void OnRemoveTrait(IPointOfInterest sourceCharacter) {
        if (_removeTraitJob != null) {
            _removeTraitJob.jobQueueParent.CancelJob(_removeTraitJob);
        }
        _sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, name.ToLower());
        base.OnRemoveTrait(sourceCharacter);
    }
    #endregion

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
