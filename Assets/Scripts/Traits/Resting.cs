using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resting : Trait {
    public override bool isNotSavable {
        get { return true; }
    }

    private Character _character;
    public Lycanthrope lycanthropyTrait { get; private set; }

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
        //effects = new List<TraitEffect>();
        //advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.DRINK_BLOOD };
    }

    #region Overrides
    public override void OnAddTrait(ITraitable sourceCharacter) {
        if (sourceCharacter is Character) {
            _character = sourceCharacter as Character;
            lycanthropyTrait = _character.GetNormalTrait("Lycanthrope") as Lycanthrope;
            //if(lycanthropyTrait != null) {
            //    Messenger.AddListener(Signals.HOUR_STARTED, CheckForLycanthropy);
            //}
            Messenger.AddListener(Signals.TICK_STARTED, RecoverHP);
        }
        base.OnAddTrait(sourceCharacter);
    }
    public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
        //if (lycanthropyTrait != null) {
        //    Messenger.RemoveListener(Signals.HOUR_STARTED, CheckForLycanthropy);
        //}
        Messenger.RemoveListener(Signals.TICK_STARTED, RecoverHP);
        _character = null;
        base.OnRemoveTrait(sourceCharacter, removedBy);
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
