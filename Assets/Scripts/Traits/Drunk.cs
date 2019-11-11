using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Drunk : Trait {

        public Drunk() {
            name = "Drunk";
            description = "This character is intoxicated and may lash out at nearby characters.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
            associatedInteraction = INTERACTION_TYPE.NONE;
            crimeSeverity = CRIME_CATEGORY.NONE;
            daysDuration = 24;
            //effects = new List<TraitEffect>();
        }

        #region Overrides
        public override bool CreateJobsOnEnterVisionBasedOnOwnerTrait(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            if (targetPOI is Character) {
                Character targetCharacter = targetPOI as Character;
                if (!targetCharacter.isDead) {
                    int value = 1;
                    RELATIONSHIP_EFFECT relEffect = characterThatWillDoJob.GetRelationshipEffectWith(targetCharacter);
                    if (relEffect == RELATIONSHIP_EFFECT.NEGATIVE) {
                        value = 3;
                    } else if (relEffect == RELATIONSHIP_EFFECT.NONE) {
                        value = 2;
                    }
                    if (characterThatWillDoJob.traitContainer.GetNormalTrait("Hothead") != null) {
                        value *= 5;
                    }
                    int chance = UnityEngine.Random.Range(0, 100);
                    if (chance < value) {
                        characterThatWillDoJob.PrintLogIfActive(GameManager.Instance.TodayLogString() + characterThatWillDoJob.name
                            + " Drunk Assault Chance: " + value + ", Roll: " + chance);
                        if (characterThatWillDoJob.marker.AddHostileInRange(targetCharacter, false, false, false)) {
                            if (!characterThatWillDoJob.marker.avoidInRange.Contains(targetCharacter)) {
                                Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "drunk_assault");
                                log.AddToFillers(characterThatWillDoJob, characterThatWillDoJob.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                                log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                                //log.AddLogToInvolvedObjects();
                                characterThatWillDoJob.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
                            }
                            //characterThatWillDoJob.marker.ProcessCombatBehavior();
                        }
                        return true;
                    }
                }
            }
            return base.CreateJobsOnEnterVisionBasedOnOwnerTrait(targetPOI, characterThatWillDoJob);
        }
        #endregion
    }
}
