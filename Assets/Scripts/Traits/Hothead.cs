using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hothead : Trait {

    public Hothead() {
        name = "Hothead";
        description = "This character is hotheaded.";
        type = TRAIT_TYPE.FLAW;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
        //effects = new List<TraitEffect>();
    }

    #region Overrides
    public override bool CreateJobsOnEnterVisionBasedOnOwnerTrait(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
        if (targetPOI is Character) {
            Character targetCharacter = targetPOI as Character;
            if (!targetCharacter.isDead) {
                int chance = UnityEngine.Random.Range(0, 100);
                if (chance < 2 && characterThatWillDoJob.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.NEGATIVE) {
                    characterThatWillDoJob.PrintLogIfActive(GameManager.Instance.TodayLogString() + characterThatWillDoJob.name
                        + "Hothead Assault Chance: 2, Roll: " + chance);
                    if (characterThatWillDoJob.marker.AddHostileInRange(targetCharacter, false, false, false)) {
                        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "hothead_assault");
                        log.AddToFillers(characterThatWillDoJob, characterThatWillDoJob.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                        //log.AddLogToInvolvedObjects();
                        characterThatWillDoJob.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
                        characterThatWillDoJob.marker.ProcessCombatBehavior();
                    }
                    return true;
                }
            }
        }
        return base.CreateJobsOnEnterVisionBasedOnOwnerTrait(targetPOI, characterThatWillDoJob);
    }
    #endregion
}
