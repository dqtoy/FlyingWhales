﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drunk : Trait {

    public Drunk() {
        name = "Drunk";
        description = "This character is drunk.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 24;
        //effects = new List<TraitEffect>();
    }

    #region Overrides
    public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
        if (traitOwner is Character) {
            Character targetCharacter = traitOwner as Character;
            if (!targetCharacter.isDead) {
                int value = 5;
                RELATIONSHIP_EFFECT relEffect = characterThatWillDoJob.GetRelationshipEffectWith(targetCharacter);
                if (relEffect == RELATIONSHIP_EFFECT.NEGATIVE) {
                    value = 20;
                } else if (relEffect == RELATIONSHIP_EFFECT.NONE) {
                    value = 10;
                }
                int chance = UnityEngine.Random.Range(0, 100);
                if (chance < value) {
                    if (characterThatWillDoJob.marker.AddHostileInRange(targetCharacter, false)) {
                        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "drunk_assault");
                        log.AddToFillers(characterThatWillDoJob, characterThatWillDoJob.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                        log.AddLogToInvolvedObjects();
                        characterThatWillDoJob.RegisterLogAndShowNotifToThisCharacterOnly(log);
                    }
                    return true;
                }
            }
        }
        return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
    }
    #endregion
}