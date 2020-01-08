using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class ReduceConflict : Interrupt {
        public ReduceConflict() : base(INTERRUPT.Reduce_Conflict) {
            duration = 0;
            isSimulateneous = true;
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
            Character targetCharacter = target as Character;
            List<Character> enemyOrRivalCharacters = targetCharacter.opinionComponent.GetCharactersWithOpinionLabel(OpinionComponent.Enemy, OpinionComponent.Rival);
            if(enemyOrRivalCharacters.Count > 0) {
                Character chosenEnemyOrRival = enemyOrRivalCharacters[UnityEngine.Random.Range(0, enemyOrRivalCharacters.Count)];
                targetCharacter.opinionComponent.AdjustOpinion(chosenEnemyOrRival, "Base", 15);
                Log log = new Log(GameManager.Instance.Today(), "Interrupt", "Reduce Conflict", "reduce_conflict");
                log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                log.AddToFillers(chosenEnemyOrRival, chosenEnemyOrRival.name, LOG_IDENTIFIER.CHARACTER_3);
                actor.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
            }
            return true;
        }
        #endregion
    }
}