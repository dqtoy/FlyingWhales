using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

namespace Interrupts {
    public class ReduceConflict : Interrupt {
        public ReduceConflict() : base(INTERRUPT.Reduce_Conflict) {
            duration = 0;
            isSimulateneous = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target) {
            Character targetCharacter = target as Character;
            List<Character> enemyOrRivalCharacters = targetCharacter.relationshipContainer.GetEnemyCharacters();
            if(enemyOrRivalCharacters.Count > 0) {
                Character chosenEnemyOrRival = enemyOrRivalCharacters[UnityEngine.Random.Range(0, enemyOrRivalCharacters.Count)];
                string logKey = "reduce_conflict";
                if (UnityEngine.Random.Range(0, 2) == 0 && chosenEnemyOrRival.traitContainer.HasTrait("Hothead")) {
                    logKey = "reduce_conflict_rebuffed";
                } else {
                    targetCharacter.relationshipContainer.AdjustOpinion(targetCharacter, chosenEnemyOrRival, "Base", 15);
                }
                Log log = new Log(GameManager.Instance.Today(), "Interrupt", "Reduce Conflict", logKey);
                log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                log.AddToFillers(chosenEnemyOrRival, chosenEnemyOrRival.name, LOG_IDENTIFIER.CHARACTER_3);
                actor.logComponent.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
            }
            return true;
        }
        #endregion
    }
}