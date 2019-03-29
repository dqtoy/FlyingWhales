using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resting : Trait {

    private Character _character;
    private Lycanthropy _lycanthropyTrait;

    public Resting() {
        name = "Resting";
        description = "This character is resting.";
        thoughtText = "[Character] is resting.";
        type = TRAIT_TYPE.DISABLER;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_SEVERITY.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void OnAddTrait(IPointOfInterest sourceCharacter) {
        _character = sourceCharacter as Character;
        _lycanthropyTrait = _character.GetTrait("Lycanthropy") as Lycanthropy;
        if(_lycanthropyTrait != null) {
            Messenger.AddListener(Signals.HOUR_STARTED, CheckForLycanthropy);
        }
        base.OnAddTrait(sourceCharacter);
    }
    public override void OnRemoveTrait(IPointOfInterest sourceCharacter) {
        if (_lycanthropyTrait != null) {
            Messenger.AddListener(Signals.HOUR_STARTED, CheckForLycanthropy);
        }
        _character = null;
        base.OnRemoveTrait(sourceCharacter);
    }
    #endregion

    private void CheckForLycanthropy() {
        int chance = UnityEngine.Random.Range(0, 100);
        if(_character.race == RACE.WOLF) {
            //Turn back to normal form
            if (chance < 20) {

            }
        } else {
            //Turn to wolf
            if (chance < 20) {

            }
        }
    }
}
