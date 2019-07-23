﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Taunted : Trait {

    private Character _responsibleCharacter;
    private Character _sourceCharacter;

    public override Character responsibleCharacter {
        get { return _responsibleCharacter; }
    }

    public Taunted() {
        name = "Taunted";
        description = "This character is taunted.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEGATIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void SetCharacterResponsibleForTrait(Character character) {
        _responsibleCharacter = character;
    }
    public override void OnAddTrait(IPointOfInterest sourcePOI) {
        base.OnAddTrait(sourcePOI);
        _sourceCharacter = sourcePOI as Character;
        if (_sourceCharacter.stateComponent.currentState == null || _sourceCharacter.stateComponent.currentState.characterState != CHARACTER_STATE.COMBAT) {
            _sourceCharacter.marker.AddHostileInRange(_responsibleCharacter, CHARACTER_STATE.COMBAT);
        } else {
            Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, _sourceCharacter);
        }
        Messenger.AddListener<Character, Trait>(Signals.TRAIT_ADDED, OnCharacterGainedTrait);
        Messenger.AddListener<Character, Character>(Signals.CHARACTER_REMOVED_FROM_VISION, OnCharacterRemovedFromVision);
    }
    public override void OnRemoveTrait(IPointOfInterest sourcePOI, Character removedBy) {
        base.OnRemoveTrait(sourcePOI, removedBy);
        Messenger.RemoveListener<Character, Trait>(Signals.TRAIT_ADDED, OnCharacterGainedTrait);
        Messenger.RemoveListener<Character, Character>(Signals.CHARACTER_REMOVED_FROM_VISION, OnCharacterRemovedFromVision);
        Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, sourcePOI as Character);
    }
    public override void OnDeath(Character character) {
        base.OnDeath(character);
        Messenger.RemoveListener<Character, Trait>(Signals.TRAIT_ADDED, OnCharacterGainedTrait);
        Messenger.RemoveListener<Character, Character>(Signals.CHARACTER_REMOVED_FROM_VISION, OnCharacterRemovedFromVision);
        _sourceCharacter.RemoveTrait(this);
    }
    #endregion

    private void OnCharacterGainedTrait(Character character, Trait trait) {
        if (responsibleCharacter.id == character.id) {
            if (character.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER) || character.isDead) {
                _sourceCharacter.RemoveTrait(this); //if the character that taunted this character becomes dead or negatively disabled, remove this trait.
            }
        }
    }
    private void OnCharacterRemovedFromVision(Character character, Character removedCharacter) {
        if (character == _sourceCharacter && removedCharacter == responsibleCharacter) {
            _sourceCharacter.RemoveTrait(this);
        }
    }
}