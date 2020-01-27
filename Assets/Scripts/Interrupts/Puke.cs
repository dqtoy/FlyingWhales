using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Puke : Interrupt {
        public Puke() : base(INTERRUPT.Puke) {
            duration = 2;
            doesStopCurrentAction = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target) {
            actor.SetPOIState(POI_STATE.INACTIVE);
            return true;
        }
        public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
            actor.SetPOIState(POI_STATE.ACTIVE);
            return true;
        }
        public override string ReactionToActor(Character witness, Character actor, IPointOfInterest target, Interrupt interrupt) {
            string response = base.ReactionToActor(witness, actor, target, interrupt);
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disgust, witness, actor);
            return response;
        }
        #endregion
    }
}