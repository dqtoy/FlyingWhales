using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

namespace Interrupts {
	public class Poisoned : Interrupt {
		public Poisoned() : base(INTERRUPT.Poisoned) {
			duration = 0;
			isSimulateneous = true;
		}

		#region Overrides
		public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target, ref Log overrideEffectLog) {
			if (UnityEngine.Random.Range(0, 2) == 0) {
				if(actor.traitContainer.AddTrait(actor, "Sick")) {
					//TODO: Can still be improved: Create a function that returns the trait that's been added instead of boolean
					Sick sick = actor.traitContainer.GetNormalTrait<Sick>("Sick");
					Traits.Poisoned poisoned = target.traitContainer.GetNormalTrait<Traits.Poisoned>("Poisoned");
					if (poisoned.responsibleCharacters != null) {
						for (int i = 0; i < poisoned.responsibleCharacters.Count; i++) {
							sick.AddCharacterResponsibleForTrait(poisoned.responsibleCharacters[i]);
						}
					}
					
					overrideEffectLog = new Log(GameManager.Instance.Today(), "Interrupt", "Poisoned", "sick");
					overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
					//log.AddLogToInvolvedObjects();
				}
			} else {
				actor.Death("poisoned");
			}

			return base.ExecuteInterruptStartEffect(actor, target, ref overrideEffectLog);
		}
		#endregion
	}
}