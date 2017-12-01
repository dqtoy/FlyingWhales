using UnityEngine;
using System.Collections;

public class RefugeeAvatar : CitizenAvatar {

	public UILabel populationLbl;

	internal override void Init (Role citizenRole){
		base.Init (citizenRole);
	}

	#region Overrides
	internal override void NewMove() {
		if (this.citizenRole.targetLocation != null) {
			if (this.citizenRole.path != null) {
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
			if (!this.hasArrived) {
				SetHasArrivedState(true);
				EndAttack ();
			}
		}
	}
	internal override void UpdateUI (){
		if(this.populationLbl != null){
			if(this.populationLbl.gameObject != null){
				this.populationLbl.text = ((Refugee)this.citizenRole).population.ToString ();
			}
		}
	}
	#endregion
}
