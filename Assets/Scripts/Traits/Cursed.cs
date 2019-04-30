using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursed : Trait {
    private Character _responsibleCharacter;
    private Character _sourceCharacter;
    //private GoapPlanJob _removeTraitJob;

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
            //_sourceCharacter.CreateRemoveTraitJob(name);
            _sourceCharacter.AddTraitNeededToBeRemoved(this);
            _sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, name.ToLower());
        }
    }
    public override void OnRemoveTrait(IPointOfInterest sourceCharacter) {
        //if (_removeTraitJob != null) {
        //    _removeTraitJob.jobQueueParent.CancelJob(_removeTraitJob);
        //}
        _sourceCharacter.CancelAllJobsTargettingThisCharacter("Remove Trait", name);
        _sourceCharacter.RemoveTraitNeededToBeRemoved(this);
        _sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, name.ToLower());
        base.OnRemoveTrait(sourceCharacter);
    }
    #endregion
}
