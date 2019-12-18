using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Enemy : RelationshipTrait {
        public override string nameInUI {
            get { return "Enemy: " + targetCharacter.name; }
        }

        public Enemy(Character target) : base(target) {
            name = "Enemy";
            description = "This character is an enemy of " + targetCharacter.name;
            relType = RELATIONSHIP_TYPE.ENEMY;
            type = TRAIT_TYPE.RELATIONSHIP;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            //effects = new List<TraitEffect>();
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
            if (sourcePOI is Character) {
                Character sourceCharacter = sourcePOI as Character;
                if (!GameManager.Instance.gameHasStarted || sourceCharacter.isSwitchingAlterEgo) {
                    return; //do not log initial relationships or when switching alter egos
                }
                Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", "enemy");
                log.AddToFillers(sourceCharacter, sourceCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                //sourceCharacter.AddHistory(log);
                log.AddLogToInvolvedObjects();
            }
        }
        public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy) {
            if (sourcePOI is Character) {
                Character sourceCharacter = sourcePOI as Character;
                if (!GameManager.Instance.gameHasStarted || sourceCharacter.isSwitchingAlterEgo) {
                    return; //do not log initial relationships or when switching alter egos
                }
                Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", "not_enemy");
                log.AddToFillers(sourceCharacter, sourceCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                //sourceCharacter.AddHistory(log);
                log.AddLogToInvolvedObjects();
            }
        }
        public override bool IsUnique() {
            return false;
        }
        public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
            //In relationships, the trait owner is not the usual target because the trait owner is also the character that will do job, instead use the target character that is stored in it
            if (!targetCharacter.isDead && !characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.UNDERMINE_ENEMY, targetCharacter)) {
                int chance = UnityEngine.Random.Range(0, 100);
                int value = 0;
                CHARACTER_MOOD currentMood = characterThatWillDoJob.currentMoodType;
                if (currentMood == CHARACTER_MOOD.DARK) {
                    value = 20;
                } else if (currentMood == CHARACTER_MOOD.BAD) {
                    value = 10;
                }
                if (chance < value) {
                    return characterThatWillDoJob.CreateUndermineJobOnly(targetCharacter, "saw");
                }
            }
            return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
        }
        #endregion
    }
}

