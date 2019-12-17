using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Traits {
	public class ElementalMaster : Trait {
		
		public ElementalMaster() {
			name = "Elemental Master";
			description = "This character has mastered the elements.";
			type = TRAIT_TYPE.BUFF;
			effect = TRAIT_EFFECT.POSITIVE;
			ticksDuration = 0;
		}

		#region Overrides
		public override bool CreateJobsOnEnterVisionBasedOnOwnerTrait(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
			if (targetPOI is TornadoTileObject) {
				GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.STOP_TORNADO,
					INTERACTION_TYPE.SNUFF_TORNADO, targetPOI, characterThatWillDoJob);
				characterThatWillDoJob.jobQueue.AddJobInQueue(job);
			}
			return base.CreateJobsOnEnterVisionBasedOnOwnerTrait(targetPOI, characterThatWillDoJob);
		}
		#endregion
		
	}
}