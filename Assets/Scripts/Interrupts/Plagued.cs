using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
	public class Plagued : Interrupt {
		public Plagued() : base(INTERRUPT.Plagued) {
			duration = 0;
			isSimulateneous = true;
		}

		#region Overrides
		public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target) {
			if (actor.traitContainer.AddTrait(actor, "Plagued")) {
				Log log = new Log(GameManager.Instance.Today(), "Interrupt", "Plagued", "contract");
				log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddLogToInvolvedObjects();
				return true;
			}
			return base.ExecuteInterruptStartEffect(actor, target);
		}
		#endregion
	}
}