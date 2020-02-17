using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class NarcolepticAttack : Interrupt {
        public NarcolepticAttack() : base(INTERRUPT.Narcoleptic_Attack) {
            duration = 6;
            doesStopCurrentAction = true;
            interruptIconString = GoapActionStateDB.Flirt_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target, ref Log overrideEffectLog) {
            actor.traitContainer.AddTrait(actor, "Resting");
            return true;
        }
        public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
            actor.traitContainer.RemoveTrait(actor, "Resting");
            return true;
        }
        public override string ReactionToActor(Character witness, Character actor, IPointOfInterest target, Interrupt interrupt) {
            string response = base.ReactionToActor(witness, actor, target, interrupt);
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor);
            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
            if (opinionLabel == OpinionComponent.Enemy) {
                if (UnityEngine.Random.Range(0, 2) == 0) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Scorn, witness, actor);
                }
            } else if (opinionLabel == OpinionComponent.Rival) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Scorn, witness, actor);
            }
            return response;
        }
        #endregion
    }
}