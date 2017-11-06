using UnityEngine;
using System.Collections;

public class CaravanAvatar : CitizenAvatar {
	internal override void Init (Role citizenRole){
		base.Init (citizenRole);
		CreatePath (PATHFINDING_MODE.USE_ROADS);
	}
	internal virtual void NewMove() {
		if (this.citizenRole.targetLocation != null) {
			if (this.citizenRole.path != null) {
				if(this.citizenRole.targetLocation.city == null || (this.citizenRole.targetLocation.city != null && this.citizenRole.targetLocation.city.id != this.citizenRole.targetCity.id)){
					CancelEventInvolvedIn ();
					return;
				}
				if (this.citizenRole.path.Count > 0) {
					this.citizenRole.location.ExitCitizen (this.citizenRole.citizen);
					this.MakeCitizenMove(this.citizenRole.location, this.citizenRole.path[0]);
				}else{
					CancelEventInvolvedIn ();
				}
			}
		}
	}
	internal override void HasArrivedAtTargetLocation (){
		if (this.citizenRole.location == this.citizenRole.targetLocation) {
			if(this.citizenRole.targetLocation.city == null || (this.citizenRole.targetLocation.city != null && this.citizenRole.targetLocation.city.id != this.citizenRole.targetCity.id)){
				CancelEventInvolvedIn ();
				return;
			}
			if (!this.hasArrived) {
				SetHasArrivedState(true);
				EndAttack ();
			}
		}
	}
}
