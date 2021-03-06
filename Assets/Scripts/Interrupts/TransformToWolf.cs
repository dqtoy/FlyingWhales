﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

namespace Interrupts {
    public class TransformToWolf : Interrupt {
        public TransformToWolf() : base(INTERRUPT.Transform_To_Wolf) {
            duration = 6;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            interruptIconString = GoapActionStateDB.Flirt_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
            actor.lycanData.TurnToWolf();
            return base.ExecuteInterruptEndEffect(actor, target);
        }
        public override string ReactionToActor(Character witness, Character actor, IPointOfInterest target,
            Interrupt interrupt, REACTION_STATUS status) {
            string response = base.ReactionToActor(witness, actor, target, interrupt, status);
            Character originalForm = actor.lycanData.originalForm;
            if (!witness.isLycanthrope) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, originalForm, status);
                // response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, originalForm);

                string opinionLabel = witness.relationshipContainer.GetOpinionLabel(originalForm);
                if (opinionLabel == OpinionComponent.Acquaintance || opinionLabel == OpinionComponent.Friend ||
                    opinionLabel == OpinionComponent.Close_Friend) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Despair, witness, originalForm, status);
                }
                if (witness.traitContainer.HasTrait("Coward")) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, originalForm, status);
                } else {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, originalForm, status);
                }
                CrimeManager.Instance.ReactToCrime(witness, originalForm, this, CRIME_TYPE.HEINOUS);
            }
            return response;
        }
        #endregion
    }
}
