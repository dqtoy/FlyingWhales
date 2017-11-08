using UnityEngine;
using System.Collections;

public class CaravanAvatar : CitizenAvatar {
	public SpriteRenderer sprtCaravan;
	public Sprite sprtFoodCaravan;
	public Sprite sprtMaterialCaravan;
	public Sprite sprtOreCaravan;

	internal override void Init (Role citizenRole){
		base.Init (citizenRole);
		if(citizenRole.gameEventInvolvedIn is SendResource){
			SendResource sendResource = (SendResource)citizenRole.gameEventInvolvedIn;
			if(sendResource.resourceType == RESOURCE_TYPE.FOOD){
				sprtCaravan.sprite = sprtFoodCaravan;
			}else if(sendResource.resourceType == RESOURCE_TYPE.MATERIAL){
				sprtCaravan.sprite = sprtMaterialCaravan;
			}else if(sendResource.resourceType == RESOURCE_TYPE.ORE){
				sprtCaravan.sprite = sprtOreCaravan;
			}
		}
	}
	internal override void NewMove() {
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
