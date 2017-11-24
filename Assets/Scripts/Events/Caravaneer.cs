using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Caravaneer : GameEvent {
	internal Caravan caravan;
	internal CaravanAvatar caravanAvatar;
	internal City sourceCity;
	internal City targetCity;
	internal RESOURCE_TYPE producedResource;
	internal RESOURCE_TYPE neededResource;

	internal List<HexTile> path;
	internal bool isReturning;

	internal int resourceAmount;
	internal int reserveAmount;

	public Caravaneer(int startDay, int startMonth, int startYear, Citizen startedBy) : base (startDay, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.CARAVANEER;
		this.name = "Caravaneer";
		this.caravan = (Caravan)startedBy.assignedRole;
		this.sourceCity = startedBy.city;
		this.producedResource = this.sourceCity.region.tileWithSpecialResource.specialResourceType;
		if(this.producedResource == RESOURCE_TYPE.MATERIAL){
			if(this.sourceCity.kingdom.race != RACE.HUMANS && (this.sourceCity.region.tileWithSpecialResource.specialResource == RESOURCE.SLATE || this.sourceCity.region.tileWithSpecialResource.specialResource == RESOURCE.GRANITE)){
				this.producedResource = RESOURCE_TYPE.NONE;
			}else if(this.sourceCity.kingdom.race != RACE.ELVES && (this.sourceCity.region.tileWithSpecialResource.specialResource == RESOURCE.OAK || this.sourceCity.region.tileWithSpecialResource.specialResource == RESOURCE.EBONY)){
				this.producedResource = RESOURCE_TYPE.NONE;
			}
		}
		this.path = new List<HexTile> ();
		this.isReturning = false;
		this.resourceAmount = 0;
		this.reserveAmount = 0;

	}
	internal void Initialize(){
		this.caravanAvatar = startedBy.assignedRole.avatar.GetComponent<CaravanAvatar>();
		this.DeactivateCaravan ();
		this.SearchForCityToObtainResource ();
	}
	#region Overrides
	internal override void DoneCitizenAction(Citizen citizen){
		//Search for Another City to Obtain Resource
		if(!this.isReturning){
			if(this.caravan.targetLocation.city == null || (this.caravan.targetLocation.city != null && this.caravan.targetLocation.city.id != this.targetCity.id) || this.targetCity.isDead){
				ReturnCaravan ();
			}else{
				this.targetCity.GiveResourceToCaravan (this, this.reserveAmount);
			}
		}else{
			ObtainResource ();
			SearchForCityToObtainResource ();
		}
	}
	internal override void DoneEvent(){
		Debug.Log ("CARAVANEER EVENT DONE FOR " + this.sourceCity.name + " because: This event is active " + this.isActive.ToString() + ", the caravan is destroyed " + this.caravan.isDestroyed.ToString()
			+ ", city is dead " + this.sourceCity.isDead.ToString());
		base.DoneEvent();
		if(this.reserveAmount > 0){
			ReturnReservedResource ();
		}
		this.caravan.DestroyGO();
		this.sourceCity.caravaneer = null;
	}
	internal override void CancelEvent (){
		base.CancelEvent ();
		this.DoneEvent ();
	}
	#endregion

	private void SearchForCityToObtainResource(){
		if(this.isActive && !this.caravan.isDestroyed && !this.sourceCity.isDead){
			this.neededResource = GetNeededResource ();
			CaravaneerThreadPool.Instance.AddToThreadPool (new CaravaneerThread (this));
		}else{
			this.DoneEvent();
		}

	}

	private void ActivateCaravan(){
		this.caravan.CaravanActivation (true);
	}

	private void DeactivateCaravan(){
		this.isReturning = false;
		this.caravanAvatar.amountPanelGO.SetActive (false);
		this.caravan.CaravanActivation (false);
	}
	private RESOURCE_TYPE GetNeededResource(){
		if(this.producedResource == RESOURCE_TYPE.FOOD){
			if(this.sourceCity.materialCount <= this.sourceCity.materialRequirement){
				return RESOURCE_TYPE.MATERIAL;
			}else if(this.sourceCity.oreCount <= this.sourceCity.oreRequirement){
				return RESOURCE_TYPE.ORE;
			}else{
				if(this.sourceCity.materialCount <= this.sourceCity.oreCount){
					return RESOURCE_TYPE.MATERIAL;
				}else{
					return RESOURCE_TYPE.ORE;
				}
			}

		}else if(this.producedResource == RESOURCE_TYPE.MATERIAL){
			if (this.sourceCity.foodCount <= this.sourceCity.foodRequirement) {
				return RESOURCE_TYPE.FOOD;
			} else if (this.sourceCity.oreCount <= this.sourceCity.oreRequirement) {
				return RESOURCE_TYPE.ORE;
			} else {
				if(this.sourceCity.foodCount <= this.sourceCity.oreCount){
					return RESOURCE_TYPE.FOOD;
				}else{
					return RESOURCE_TYPE.ORE;
				}
			}
		}else if(this.producedResource == RESOURCE_TYPE.ORE){
			if (this.sourceCity.foodCount <= this.sourceCity.foodRequirement) {
				return RESOURCE_TYPE.FOOD;
			} else if (this.sourceCity.materialCount <= this.sourceCity.materialRequirement) {
				return RESOURCE_TYPE.MATERIAL;
			} else {
				if(this.sourceCity.foodCount <= this.sourceCity.materialCount){
					return RESOURCE_TYPE.FOOD;
				}else{
					return RESOURCE_TYPE.MATERIAL;
				}
			}
		}else{
			if (this.sourceCity.foodCount <= this.sourceCity.foodRequirement) {
				return RESOURCE_TYPE.FOOD;
			} else if (this.sourceCity.materialCount <= this.sourceCity.materialRequirement) {
				return RESOURCE_TYPE.MATERIAL;
			} else if (this.sourceCity.oreCount <= this.sourceCity.oreRequirement) {
				return RESOURCE_TYPE.ORE;
			} else {
				if(this.sourceCity.foodCount <= this.sourceCity.materialCount && this.sourceCity.foodCount <= this.sourceCity.oreCount){
					return RESOURCE_TYPE.FOOD;
				}else if(this.sourceCity.materialCount <= this.sourceCity.foodCount && this.sourceCity.materialCount <= this.sourceCity.oreCount){
					return RESOURCE_TYPE.MATERIAL;
				}else{
					return RESOURCE_TYPE.ORE;
				}
			}
		}
	}
	private void RescheduleChecking(){
		GameDate newSchedule = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
		newSchedule.AddDays (5);
		SchedulingManager.Instance.AddEntry (newSchedule, () => SearchForCityToObtainResource ());
	}

	internal void ReceiveCityToObtainResource(City targetCity, List<HexTile> path){
		if(this.isActive){
			if(targetCity == null){
				DeactivateCaravan ();
				RescheduleChecking ();
			}else{
				this.path = path;
				this.targetCity = targetCity;
				ReserveResource ();
				ActivateCaravan ();
				SendCaravan ();
			}
		}
	}
	private void ReserveResource(){
		if (neededResource == RESOURCE_TYPE.FOOD) {
			this.reserveAmount = this.targetCity.ReserveFood ();
		}else if (neededResource == RESOURCE_TYPE.MATERIAL) {
			RESOURCE resource = RESOURCE.SLATE;
			if(this.sourceCity.kingdom.race == RACE.ELVES){
				resource = RESOURCE.OAK;
			}
			this.reserveAmount = this.targetCity.ReserveMaterial (resource);
		}else if (neededResource == RESOURCE_TYPE.ORE) {
			this.reserveAmount = this.targetCity.ReserveOre ();
		}
	}
	private void ReturnReservedResource(){
		if(this.reserveAmount > 0 && !this.targetCity.isDead){
			if (neededResource == RESOURCE_TYPE.FOOD) {
				this.targetCity.AdjustReserveFood (-this.reserveAmount);
				this.targetCity.AdjustFoodCount (this.reserveAmount);
			}else if (neededResource == RESOURCE_TYPE.MATERIAL) {
				RESOURCE resource = RESOURCE.SLATE;
				if(this.sourceCity.kingdom.race == RACE.ELVES){
					resource = RESOURCE.OAK;
				}
				this.targetCity.AdjustReservedMaterialCount (-this.reserveAmount, resource);
				this.targetCity.AdjustMaterialCount (this.reserveAmount, resource);
			}else if (neededResource == RESOURCE_TYPE.ORE) {
				this.targetCity.AdjustReserveOre (-this.reserveAmount);
				this.targetCity.AdjustOreCount (this.reserveAmount);
			}
		}

	}
	private void SendCaravan(){
		this.caravanAvatar.amountPanelGO.SetActive (false);
		this.isReturning = false;
		this.caravan.path = new List<HexTile> (this.path);
		this.caravan.path.RemoveAt(0);
		this.caravan.targetCity = this.targetCity;
		this.caravan.targetLocation = this.targetCity.hexTile;
		this.caravanAvatar.SetHasArrivedState (false);
		this.caravanAvatar.StartMoving ();
	}

	private void ReturnCaravan(){
		this.isReturning = true;
		this.caravan.path = new List<HexTile> (this.path);
		this.caravan.path.Reverse ();
		this.caravan.path.RemoveAt (0);
		this.caravan.targetCity = this.sourceCity;
		this.caravan.targetLocation = this.sourceCity.hexTile;
		this.caravanAvatar.SetHasArrivedState (false);
		this.caravanAvatar.StartMoving ();
	}

	internal void ReceiveResourceFromCity(int amount){
		if(amount > 0){
			this.caravanAvatar.amountPanelGO.SetActive (true);
			this.caravanAvatar.ChangeResourceIcon (neededResource);
			this.caravanAvatar.ChangeAmountText (amount);
		}
		this.resourceAmount = amount;
		ReturnCaravan ();
	}

	private void ObtainResource(){
		if(this.resourceAmount > 0){
			if(neededResource == RESOURCE_TYPE.FOOD){
				this.sourceCity.AdjustFoodCount (this.resourceAmount);
			}else if(neededResource == RESOURCE_TYPE.MATERIAL){
				this.sourceCity.AdjustMaterialCount (this.resourceAmount, RESOURCE.NONE);
			}else if(neededResource == RESOURCE_TYPE.ORE){
				this.sourceCity.AdjustOreCount (this.resourceAmount);
			}
		}
		this.resourceAmount = 0;
		this.reserveAmount = 0;
	}
}
