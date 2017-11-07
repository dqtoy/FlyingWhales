using UnityEngine;
using System.Collections;

public class SendResource : GameEvent {

	public Caravan caravan;

	internal int foodAmount;
	internal int materialAmount;
	internal int oreAmount;

	internal RESOURCE_TYPE resourceType;

	public SendResource(int startDay, int startMonth, int startYear, Citizen startedBy, int foodAmount, int materialAmount, int oreAmount, RESOURCE_TYPE resourceType) : base (startDay, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.SEND_RESOURCES;
		this.name = "Send Resources";
		this.caravan = (Caravan)startedBy.assignedRole;
		this.foodAmount = foodAmount;
		this.materialAmount = materialAmount;
		this.oreAmount = oreAmount;
		this.resourceType = resourceType;
	}

	#region Overrides
	internal override void DoneCitizenAction(Citizen citizen){
		if(foodAmount > 0){
			citizen.assignedRole.targetCity.AdjustFoodCount (foodAmount);
		}
		if (materialAmount > 0) {
			citizen.assignedRole.targetCity.AdjustMaterialCount (materialAmount);
		}
		if (oreAmount > 0) {
			citizen.assignedRole.targetCity.AdjustOreCount (oreAmount);
		}
		this.DoneEvent ();
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
