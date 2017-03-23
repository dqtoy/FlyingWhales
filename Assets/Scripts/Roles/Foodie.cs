using UnityEngine;
using System.Collections;

public class Foodie : Role {
	
	public Foodie(Citizen citizen): base(citizen){

	}

	override internal int[] GetResourceProduction(){
		if(this.citizen.workLocation != null){
			RESOURCE resourceToUse = RESOURCE.NONE;
			if (this.citizen.workLocation.specialResource == RESOURCE.NONE) {
				resourceToUse = this.citizen.workLocation.defaultResource;
			} else {
				resourceToUse = this.citizen.workLocation.specialResource;
			}

			if (resourceToUse == RESOURCE.CORN || resourceToUse == RESOURCE.DEER) {
				return new int[]{ 12, 0, 0, 0, 0, 0, 40 };
			} else if (resourceToUse == RESOURCE.WHEAT || resourceToUse == RESOURCE.PIG) {
				return new int[]{ 16, 0, 0, 0, 0, 0, 40 };
			} else if (resourceToUse == RESOURCE.RICE || resourceToUse == RESOURCE.BEHEMOTH) {
				return new int[]{ 20, 0, 0, 0, 0, 0, 40 };
			}
		}
		return new int[]{ 0, 0, 0, 0, 0, 0, 0 };
	}
}