using UnityEngine;
using System.Collections;

public class CaravanAvatar : CitizenAvatar {
	public SpriteRenderer sprtCaravan;
	public UI2DSprite sprtCaravanIcon;
	public Sprite sprtFoodCaravan;
	public Sprite sprtMaterialCaravan;
	public Sprite sprtOreCaravan;
	public Sprite sprtFoodCaravanIcon;
	public Sprite sprtMaterialCaravanIcon;
	public Sprite sprtOreCaravanIcon;

	public UILabel amountLbl;

	internal override void Init (Role citizenRole){
		base.Init (citizenRole);
		if(citizenRole.gameEventInvolvedIn is SendResource){
			SendResource sendResource = (SendResource)citizenRole.gameEventInvolvedIn;
			if(sendResource.resourceType == RESOURCE_TYPE.FOOD){
				sprtCaravan.sprite = sprtFoodCaravan;
				sprtCaravanIcon.sprite2D = sprtFoodCaravanIcon;
				amountLbl.text = sendResource.foodAmount.ToString ();
			}else if(sendResource.resourceType == RESOURCE_TYPE.MATERIAL){
				sprtCaravan.sprite = sprtMaterialCaravan;
				sprtCaravanIcon.sprite2D = sprtMaterialCaravanIcon;
				amountLbl.text = sendResource.materialAmount.ToString ();
			}else if(sendResource.resourceType == RESOURCE_TYPE.ORE){
				sprtCaravan.sprite = sprtOreCaravan;
				sprtCaravanIcon.sprite2D = sprtOreCaravanIcon;
				amountLbl.text = sendResource.oreAmount.ToString ();
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
