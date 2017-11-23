using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SendResource : GameEvent {

	public Caravan caravan;

	internal int foodAmount;
	internal int materialAmount;
	internal int oreAmount;

	internal RESOURCE_TYPE resourceType;
	internal RESOURCE resource;

	internal City targetCity;

	public SendResource(int startDay, int startMonth, int startYear, Citizen startedBy, int foodAmount, int materialAmount, int oreAmount, RESOURCE_TYPE resourceType, RESOURCE resource) : base (startDay, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.SEND_RESOURCES;
		this.name = "Send Resources";
		this.caravan = (Caravan)startedBy.assignedRole;
		this.foodAmount = foodAmount;
		this.materialAmount = materialAmount;
		this.oreAmount = oreAmount;
		this.resourceType = resourceType;
		this.resource = resource;
		this.targetCity = this.caravan.targetCity;
	}

	#region Overrides
	internal override void DoneCitizenAction(Citizen citizen){
		if(citizen.assignedRole.targetLocation.city == null || (citizen.assignedRole.targetLocation.city != null && citizen.assignedRole.targetLocation.city.id != citizen.assignedRole.targetCity.id)){
			CancelEvent ();
			return;
		}
		this.DoneEvent ();
		if (foodAmount > 0 && this.resourceType	== RESOURCE_TYPE.FOOD) {
			this.targetCity.AdjustFoodCount (foodAmount);
		}
		if (materialAmount > 0 && this.resourceType	== RESOURCE_TYPE.MATERIAL) {
			this.targetCity.AdjustMaterialCount (materialAmount, this.resource);
		}
		if (oreAmount > 0 && this.resourceType	== RESOURCE_TYPE.ORE) {
			this.targetCity.AdjustOreCount (oreAmount);
		}
//		if(foodAmount > 0){
//			citizen.assignedRole.targetCity.AdjustVirtualFoodCount (-foodAmount);
//			if(citizen.assignedRole.targetCity.foodCount >= citizen.assignedRole.targetCity.foodCapacity){
//				SendResourceThreadPool.Instance.AddToThreadPool (new SendResourceThread (this.foodAmount, this.materialAmount, this.oreAmount, this.resourceType, citizen.assignedRole.location, citizen.assignedRole.location.city, this));
//			}else{
//				this.DoneEvent ();
//				citizen.assignedRole.targetCity.AdjustFoodCount (foodAmount);
//			}
//
//		}
//		if (materialAmount > 0) {
//			citizen.assignedRole.targetCity.AdjustVirtualMaterialCount (-materialAmount);
//			if(citizen.assignedRole.targetCity.materialCount >= citizen.assignedRole.targetCity.materialCapacity){
//				SendResourceThreadPool.Instance.AddToThreadPool (new SendResourceThread (this.foodAmount, this.materialAmount, this.oreAmount, this.resourceType, citizen.assignedRole.location, citizen.assignedRole.location.city, this));
//			}else{
//				this.DoneEvent ();
//				citizen.assignedRole.targetCity.AdjustMaterialCount (materialAmount);
//			}
//
//		}
//		if (oreAmount > 0) {
//			citizen.assignedRole.targetCity.AdjustVirtualOreCount (-oreAmount);
//			if(citizen.assignedRole.targetCity.oreCount >= citizen.assignedRole.targetCity.oreCapacity){
//				SendResourceThreadPool.Instance.AddToThreadPool (new SendResourceThread (this.foodAmount, this.materialAmount, this.oreAmount, this.resourceType, citizen.assignedRole.location, citizen.assignedRole.location.city, this));
//			}else{
//				this.DoneEvent ();
//				citizen.assignedRole.targetCity.AdjustOreCount (oreAmount);
//			}
//		}
	}
	internal override void DoneEvent(){
		base.DoneEvent();
		this.caravan.DestroyGO();
	}
	internal override void CancelEvent (){
		base.CancelEvent ();
		this.DoneEvent ();
	}
	#endregion

	internal void ReceiveSendResourceThread(int foodAmount, int materialAmount, int oreAmount, RESOURCE_TYPE resourceType, HexTile sourceHextile, HexTile targetHextile, City targetCity, List<HexTile> path){
		if (targetHextile != null && targetHextile.city != null && targetCity != null && (path != null || path.Count > 0)) {
			if(targetCity.id == targetHextile.city.id){
				this.caravan.targetLocation = targetHextile;
				this.caravan.targetCity = targetCity;
				this.caravan.path = new List<HexTile> (path);
				this.caravan.avatar.GetComponent<CaravanAvatar> ().StartMoving ();
				this.caravan.avatar.GetComponent<CaravanAvatar> ().SetHasArrivedState (false);
				this.targetCity = targetCity;
				this.targetCity.AdjustVirtualFoodCount (foodAmount);
				this.targetCity.AdjustVirtualMaterialCount (materialAmount);
				this.targetCity.AdjustVirtualOreCount (oreAmount);
				return;
			}
		}
		this.DoneEvent ();
	}
}
