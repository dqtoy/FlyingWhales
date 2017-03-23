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

			int foodProduction = 0;
			int goldProduction = 40;
			if (resourceToUse == RESOURCE.CORN || resourceToUse == RESOURCE.WHEAT || resourceToUse == RESOURCE.RICE) {
				if (this.citizen.skillTraits.Contains (SKILL_TRAIT.GREEN_THUMB)) {
					foodProduction += 2;
				}
				if (this.citizen.city.governor.skillTraits.Contains (SKILL_TRAIT.GREEN_THUMB)) {
					foodProduction += 1;
				}
				if (this.citizen.city.kingdom.king.skillTraits.Contains (SKILL_TRAIT.GREEN_THUMB)) {
					foodProduction += 1;
				}
			} else if (resourceToUse == RESOURCE.DEER || resourceToUse == RESOURCE.PIG || resourceToUse == RESOURCE.BEHEMOTH) {
				if (this.citizen.skillTraits.Contains (SKILL_TRAIT.HUNTER)) {
					foodProduction += 2;
				}
				if (this.citizen.city.governor.skillTraits.Contains (SKILL_TRAIT.HUNTER)) {
					foodProduction += 1;
				}
				if (this.citizen.city.kingdom.king.skillTraits.Contains (SKILL_TRAIT.HUNTER)) {
					foodProduction += 1;
				}
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

			if (resourceToUse == RESOURCE.CORN || resourceToUse == RESOURCE.DEER) {
				foodProduction += 12;
				return new int[]{ foodProduction, 0, 0, 0, 0, 0, goldProduction };
			} else if (resourceToUse == RESOURCE.WHEAT || resourceToUse == RESOURCE.PIG) {
				foodProduction += 16;
				return new int[]{ foodProduction, 0, 0, 0, 0, 0, goldProduction };
			} else if (resourceToUse == RESOURCE.RICE || resourceToUse == RESOURCE.BEHEMOTH) {
				foodProduction += 20;
				return new int[]{ foodProduction, 0, 0, 0, 0, 0, goldProduction };
			}
		}
		return new int[]{ 0, 0, 0, 0, 0, 0, 0 };
	}
}