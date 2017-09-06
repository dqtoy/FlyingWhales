using UnityEngine;
using System.Collections;
using Panda;

public class InstigatorAvatar: CitizenAvatar {
	#region BehaviourTree Tasks

	[Task]
	public void HasArrivedAtTargetHextile() {
		if (this.citizenRole.location == this.citizenRole.targetLocation) {
			if (!this.hasArrived) {
				SetHasArrivedState(true);
				this.citizenRole.Attack ();
			}
			Task.current.Succeed();
		} else {
			Task.current.Fail();
		}

	}
	[Task]
	public void HasDiedOfOtherReasons() {
		if (this.citizenRole.citizen.isDead) {
			//Citizen has died
			this.citizenRole.gameEventInvolvedIn.DeathByOtherReasons();
			Task.current.Succeed();
		} else {
			Task.current.Fail();
		}
	}
	[Task]
	public void MoveToNextTile() {
		Move();
		Task.current.Succeed();
	}
	#endregion
}
