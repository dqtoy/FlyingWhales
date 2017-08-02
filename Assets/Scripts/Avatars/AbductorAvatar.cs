using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Panda;

public class AbductorAvatar : CitizenAvatar {
	#region BehaviourTree Tasks
	[Task]
	public void IsThereCitizen() {
		if (this.citizenRole.citizen != null) {
			Task.current.Succeed();
		} else {
			Task.current.Fail();
		}
	}
	[Task]
	public void IsThereEvent() {
		if (this.citizenRole.gameEventInvolvedIn != null) {
			Task.current.Succeed();
		} else {
			Task.current.Fail();
		}
	}

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
