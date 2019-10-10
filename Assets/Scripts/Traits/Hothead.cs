using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hothead : Trait {

    public Hothead() {
        name = "Hothead";
        description = "Hotheads are easy to anger and may have bouts of rage fits.";
        type = TRAIT_TYPE.FLAW;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
        canBeTriggered = true;
        //effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void OnSeePOI(IPointOfInterest targetPOI, Character character) {
        base.OnSeePOI(targetPOI, character);
        if(targetPOI is Character) {
            if(UnityEngine.Random.Range(0, 100) < 20) {
                Character targetCharacter = targetPOI as Character;
                if (character.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.NEGATIVE) {
                    character.AddTrait("Angry");
                    Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "angry_saw");
                    log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    character.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
                }
            }
        }
    }
    //public override bool CreateJobsOnEnterVisionBasedOnOwnerTrait(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
    //    if (targetPOI is Character) {
    //        Character targetCharacter = targetPOI as Character;
    //        if (!targetCharacter.isDead) {
    //            int chance = UnityEngine.Random.Range(0, 100);
    //            if (chance < 2 && characterThatWillDoJob.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                characterThatWillDoJob.PrintLogIfActive(GameManager.Instance.TodayLogString() + characterThatWillDoJob.name
    //                    + " Hothead Assault Chance: 2, Roll: " + chance);
    //                if (characterThatWillDoJob.marker.AddHostileInRange(targetCharacter, false, false, false)) {
    //                    if (!characterThatWillDoJob.marker.avoidInRange.Contains(targetCharacter)) {
    //                        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "hothead_assault");
    //                        log.AddToFillers(characterThatWillDoJob, characterThatWillDoJob.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //                        log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //                        //log.AddLogToInvolvedObjects();
    //                        characterThatWillDoJob.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
    //                    }
    //                    //characterThatWillDoJob.marker.ProcessCombatBehavior();
    //                }
    //                return true;
    //            }
    //        }
    //    }
    //    return base.CreateJobsOnEnterVisionBasedOnOwnerTrait(targetPOI, characterThatWillDoJob);
    //}
    public override void TriggerFlaw(Character character) {
        base.TriggerFlaw(character);
        character.AddTrait("Angry");
    }
    #endregion
}
