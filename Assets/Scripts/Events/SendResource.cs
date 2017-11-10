using UnityEngine;
using System.Collections;

public class SendResource : GameEvent {

	public Caravan caravan;

	internal int foodAmount;
	internal int materialAmount;
	internal int oreAmount;

	internal RESOURCE_TYPE resourceType;

	internal City targetCity;

	public SendResource(int startDay, int startMonth, int startYear, Citizen startedBy, int foodAmount, int materialAmount, int oreAmount, RESOURCE_TYPE resourceType) : base (startDay, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.SEND_RESOURCES;
		this.name = "Send Resources";
		this.caravan = (Caravan)startedBy.assignedRole;
		this.foodAmount = foodAmount;
		this.materialAmount = materialAmount;
		this.oreAmount = oreAmount;
		this.resourceType = resourceType;
		this.targetCity = this.caravan.targetCity;

		this.targetCity.AdjustVirtualFoodCount (foodAmount);
		this.targetCity.AdjustVirtualMaterialCount (materialAmount);
		this.targetCity.AdjustVirtualOreCount (oreAmount);

	}

	#region Overrides
	internal override void DoneCitizenAction(Citizen citizen){
		this.DoneEvent ();
		if(foodAmount > 0){
			citizen.assignedRole.targetCity.AdjustFoodCount (foodAmount);
			citizen.assignedRole.targetCity.AdjustVirtualFoodCount (-foodAmount);
		}
		if (materialAmount > 0) {
			citizen.assignedRole.targetCity.AdjustMaterialCount (materialAmount);
			citizen.assignedRole.targetCity.AdjustVirtualMaterialCount (-materialAmount);
		}
		if (oreAmount > 0) {
			citizen.assignedRole.targetCity.AdjustOreCount (oreAmount);
			citizen.assignedRole.targetCity.AdjustVirtualOreCount (-oreAmount);
		}
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
}
