using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Friend : RelationshipTrait {
        public override string nameInUI {
            get { return "Friend: " + targetCharacter.name; }
        }

        public Friend(Character target) : base(target) {
            name = "Friend";
            description = "This character is a friend of " + targetCharacter.name;
            relType = RELATIONSHIP_TYPE.FRIEND;
            type = TRAIT_TYPE.RELATIONSHIP;
            effect = TRAIT_EFFECT.POSITIVE;
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
                Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", "friend");
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
                Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", "not_friend");
                log.AddToFillers(sourceCharacter, sourceCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                //sourceCharacter.AddHistory(log);
                log.AddLogToInvolvedObjects();
            }
        }
        public override bool IsUnique() {
            return false;
        }
        #endregion
    }

}
