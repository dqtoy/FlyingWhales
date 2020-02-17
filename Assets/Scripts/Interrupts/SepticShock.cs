using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

namespace Interrupts {
    public class SepticShock : Interrupt {
        public SepticShock() : base(INTERRUPT.Septic_Shock) {
            duration = 4;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            interruptIconString = GoapActionStateDB.Flirt_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
            actor.Death("Septic Shock");
            return true;
        }
        public override string ReactionToActor(Character witness, Character actor, IPointOfInterest target, Interrupt interrupt) {
            string response = base.ReactionToActor(witness, actor, target, interrupt);
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor);
            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
            if (opinionLabel == OpinionComponent.Acquaintance || opinionLabel == OpinionComponent.Friend ||
                opinionLabel == OpinionComponent.Close_Friend) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor);
            } else if (opinionLabel == OpinionComponent.Rival) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Scorn, witness, actor);
            }
            if (witness.traitContainer.HasTrait("Coward")) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, actor);
            }
            return response;
        }
        #endregion
    }
}