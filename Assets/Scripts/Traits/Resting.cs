using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resting : Trait {

    private Character _character;
    public Lycanthropy lycanthropyTrait { get; private set; }

    public bool hasTransformed { get; private set; }
    public Resting() {
        name = "Resting";
        description = "This character is resting.";
        thoughtText = "[Character] is resting.";
        type = TRAIT_TYPE.DISABLER;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
        //advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.DRINK_BLOOD };
    }

    #region Overrides
    public override void OnAddTrait(IPointOfInterest sourceCharacter) {
        _character = sourceCharacter as Character;
        lycanthropyTrait = _character.GetNormalTrait("Lycanthropy") as Lycanthropy;
        //if(lycanthropyTrait != null) {
        //    Messenger.AddListener(Signals.HOUR_STARTED, CheckForLycanthropy);
        //}
        Messenger.AddListener(Signals.TICK_STARTED, RecoverHP);
        base.OnAddTrait(sourceCharacter);
    }
    public override void OnRemoveTrait(IPointOfInterest sourceCharacter) {
        //if (lycanthropyTrait != null) {
        //    Messenger.RemoveListener(Signals.HOUR_STARTED, CheckForLycanthropy);
        //}
        Messenger.RemoveListener(Signals.TICK_STARTED, RecoverHP);
        _character = null;
        base.OnRemoveTrait(sourceCharacter);
    }
    #endregion

    public void CheckForLycanthropy(bool forceTransform = false) {
        if(lycanthropyTrait == null) {
            return;
        }
        int chance = UnityEngine.Random.Range(0, 100);
        if(_character.race == RACE.WOLF) {
            //Turn back to normal form
            if (forceTransform || chance < 40) {
                lycanthropyTrait.PlanRevertToNormal();
                _character.currentAction.currentState.EndPerTickEffect();
                hasTransformed = true;
            }
        } else {
            //Turn to wolf
            if (forceTransform || chance < 40) {
                lycanthropyTrait.PlanTransformToWolf();
                _character.currentAction.currentState.EndPerTickEffect();
                hasTransformed = true;
            }
        }
    }

    private void RecoverHP() {
        _character.HPRecovery(0.02f);
    }
}
