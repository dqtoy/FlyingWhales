using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Panda;

public class GeneralAvatar : CitizenAvatar {
	public UILabel txtDamage;
	public UI2DSprite kingdomIndicator;
	public SpriteRenderer sprtAvatar;
	public Sprite sprtReinforceAvatar;

	#region Overrides
	internal override void Init (Role citizenRole){
		base.Init (citizenRole);
		this.kingdomIndicator.color = this.citizenRole.citizen.city.kingdom.kingdomColor;
		this.EnableCollider(true);
	}
	internal override void NewMove() {
		if (this.citizenRole.targetLocation != null) {
//			if(this.citizenRole.targetLocation.city == null || (this.citizenRole.targetLocation.city != null && this.citizenRole.targetLocation.city.id != this.citizenRole.targetCity.id)){
//				CancelEventInvolvedIn ();
//				return;
//			}
			if (this.citizenRole.path != null) {
				if (this.citizenRole.path.Count > 0) {
					this.citizenRole.location.ExitCitizen (this.citizenRole.citizen);
					this.MakeCitizenMove(this.citizenRole.location, this.citizenRole.path[0]);
				}else{
//					CancelEventInvolvedIn ();
				}
			}
		}
	}
	internal override void HasArrivedAtTargetLocation (){
		if (this.citizenRole.location == this.citizenRole.targetLocation) {
			if (!this.hasArrived) {
				SetHasArrivedState(true);
				this.citizenRole.Attack ();
			}
		}
	}
	internal override void UpdateUI (){
		if(this.txtDamage != null){
			if(this.txtDamage.gameObject != null){
				this.txtDamage.text = ((General)this.citizenRole).soldiers.ToString ();
			}
		}
	}

	#endregion

	internal void ChangeAvatarImage(GENERAL_TASKS task){
		if(task == GENERAL_TASKS.REINFORCE_CITY){
			this.sprtAvatar.sprite = this.sprtReinforceAvatar;
		}
	}
}
