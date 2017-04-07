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

			int goldProduction = 40;
			int resourceProduction = 0;
			if (this.citizen.skillTraits.Contains (SKILL_TRAIT.EFFICIENT)) {
				resourceProduction += 2;
			} else if (this.citizen.skillTraits.Contains (SKILL_TRAIT.INEFFICIENT)) {
				resourceProduction -= 2;
			}

			if (this.citizen.city.governor.skillTraits.Contains (SKILL_TRAIT.EFFICIENT)) {
				resourceProduction += 1;
			} else if (this.citizen.city.governor.skillTraits.Contains (SKILL_TRAIT.INEFFICIENT)) {
				resourceProduction -= 1;
			}

			if (this.citizen.city.kingdom.king.skillTraits.Contains (SKILL_TRAIT.EFFICIENT)) {
				resourceProduction += 1;
			} else if (this.citizen.city.kingdom.king.skillTraits.Contains (SKILL_TRAIT.INEFFICIENT)) {
				resourceProduction -= 1;
			}

			if (this.citizen.city.governor.skillTraits.Contains (SKILL_TRAIT.LAVISH)) {
				goldProduction -= 5;
			} else if (this.citizen.city.governor.skillTraits.Contains (SKILL_TRAIT.THRIFTY)) {
				goldProduction += 5;
			}

			if (this.citizen.city.kingdom.king.skillTraits.Contains (SKILL_TRAIT.LAVISH)) {
				goldProduction -= 5;
			} else if (this.citizen.city.kingdom.king.skillTraits.Contains (SKILL_TRAIT.THRIFTY)) {
				goldProduction += 5;
			}

			if (Utilities.GetBaseResourceType (resourceToUse) == BASE_RESOURCE_TYPE.STONE) {
				if (resourceToUse == RESOURCE.GRANITE) {
					resourceProduction += 10;
				} else if (resourceToUse == RESOURCE.SLATE) {
					resourceProduction += 14;
				} else if (resourceToUse == RESOURCE.MARBLE) {
					resourceProduction += 18;
				}
				return new int[]{ 0, 0, resourceProduction, 0, 0, 0, goldProduction, 0 };
			} else if (Utilities.GetBaseResourceType (resourceToUse) == BASE_RESOURCE_TYPE.WOOD) {
				if (resourceToUse == RESOURCE.CEDAR) {
					resourceProduction += 10;
				} else if (resourceToUse == RESOURCE.OAK) {
					resourceProduction += 14;
				} else if (resourceToUse == RESOURCE.EBONY) {
					resourceProduction += 18;
				}
				return new int[]{ 0, 0, resourceProduction, 0, 0, 0, goldProduction, 0 };
			}
		}
		return new int[]{ 0, 0, 0, 0, 0, 0, 0, 0 };
	}
}
