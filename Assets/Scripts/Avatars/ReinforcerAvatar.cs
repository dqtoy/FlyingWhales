using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Panda;

public class ReinforcerAvatar : CitizenAvatar {
	public TextMesh txtDamage;

	#region BehaviourTree Tasks
//	[Task]
//	public void IsThereCitizen() {
//		if (this.citizenRole.citizen != null) {
//			Task.current.Succeed();
//		} else {
//			Task.current.Fail();
//		}
//	}
//	[Task]
//	public void IsThereEvent() {
//		if (this.citizenRole.gameEventInvolvedIn != null) {
//			Task.current.Succeed();
//		} else {
//			Task.current.Fail();
//		}
//	}

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

	internal override void Move(){
		if(this.citizenRole.targetLocation != null){
			if(this.citizenRole.path != null){
				if(this.citizenRole.path.Count > 0){
					if(this.citizenRole.daysBeforeMoving <= 0){
						this.MakeCitizenMove (this.citizenRole.location, this.citizenRole.path [0]);
						this.citizenRole.daysBeforeMoving = this.citizenRole.path [0].movementDays;
						this.citizenRole.location = this.citizenRole.path[0];
						this.citizenRole.citizen.currentLocation = this.citizenRole.path [0];
						this.UpdateFogOfWar();
						this.citizenRole.path.RemoveAt (0);
						this.citizenRole.location.CollectEventOnTile(this.citizenRole.citizen.city.kingdom, this.citizenRole.citizen);
						this.CheckForKingdomDiscovery();
					}
					this.citizenRole.daysBeforeMoving -= 1;
				}
			}
		}
	}

	internal override void UpdateUI (){
		if(this.txtDamage.gameObject != null){
			this.txtDamage.text = ((Reinforcer)this.citizenRole).reinforcementValue.ToString ();
		}
	}
}
