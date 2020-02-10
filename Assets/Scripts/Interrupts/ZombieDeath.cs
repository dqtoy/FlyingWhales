using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

namespace Interrupts {
    public class ZombieDeath : Interrupt {
        public ZombieDeath() : base(INTERRUPT.Zombie_Death) {
            duration = 3;
            doesStopCurrentAction = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target, ref Log overrideEffectLog) {
            actor.Death("Zombie Virus");
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