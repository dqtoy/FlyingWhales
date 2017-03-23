using UnityEngine;
using System.Collections;

public class Gatherer : Role {
	
	public Gatherer(Citizen citizen): base(citizen){

	}


	override internal int[] GetResourceProduction(){ //food, lumber, stone, mana stone, mithril, cobalt, gold
		if(this.citizen.workLocation != null){
			RESOURCE resourceToUse = RESOURCE.NONE;
			if (this.citizen.workLocation.specialResource == RESOURCE.NONE) {
				resourceToUse = this.citizen.workLocation.defaultResource;
			} else {
				resourceToUse = this.citizen.workLocation.specialResource;
			}

			if (Utilities.GetBaseResourceType (resourceToUse) == BASE_RESOURCE_TYPE.STONE) {
				if (resourceToUse == RESOURCE.GRANITE) {
					return new int[]{ 0, 0, 10, 0, 0, 0, 40 };
				} else if (resourceToUse == RESOURCE.SLATE) {
					return new int[]{ 0, 0, 14, 0, 0, 0, 40 };
				} else if (resourceToUse == RESOURCE.MARBLE) {
					return new int[]{ 0, 0, 18, 0, 0, 0, 40 };
				}
			} else if (Utilities.GetBaseResourceType (resourceToUse) == BASE_RESOURCE_TYPE.WOOD) {
				if (resourceToUse == RESOURCE.CEDAR) {
					return new int[]{ 0, 10, 0, 0, 0, 0, 40 };
				} else if (resourceToUse == RESOURCE.OAK) {
					return new int[]{ 0, 14, 0, 0, 0, 0, 40 };
				} else if (resourceToUse == RESOURCE.EBONY) {
					return new int[]{ 0, 18, 0, 0, 0, 0, 40 };
				}
			}
		}
		return new int[]{ 0, 0, 0, 0, 0, 0, 0 };
	}
}
