using UnityEngine;
using System.Collections;

public class Miner : Role {

	public Miner(Citizen citizen): base(citizen){

	}

//	override internal int[] GetResourceProduction(){
//		if (this.citizen.workLocation != null) {
//			RESOURCE resourceToUse = RESOURCE.NONE;
//			if (this.citizen.workLocation.specialResource == RESOURCE.NONE) {
//				resourceToUse = this.citizen.workLocation.defaultResource;
//			} else {
//				resourceToUse = this.citizen.workLocation.specialResource;
//			}
//
//			int goldProduction = 40;
//			if (this.citizen.city.governor.skillTraits.Contains (SKILL_TRAIT.LAVISH)) {
//				goldProduction -= 5;
//			} else if (this.citizen.city.governor.skillTraits.Contains (SKILL_TRAIT.THRIFTY)) {
//				goldProduction += 5;
//			}
//
//			if (this.citizen.city.kingdom.king.skillTraits.Contains (SKILL_TRAIT.LAVISH)) {
//				goldProduction -= 5;
//			} else if (this.citizen.city.kingdom.king.skillTraits.Contains (SKILL_TRAIT.THRIFTY)) {
//				goldProduction += 5;
//			}
//
//			if (resourceToUse == RESOURCE.GOLD) {
//				return new int[]{ 0, 0, 0, 0, 0, 0, goldProduction, 40 };
//			} else if (resourceToUse == RESOURCE.MANA_STONE) {
//				return new int[]{ 0, 0, 0, 10, 0, 0, goldProduction, 0 };
//			} else if (resourceToUse == RESOURCE.MITHRIL) {
//				return new int[]{ 0, 0, 0, 0, 10, 0, goldProduction, 0 };
//			} else if (resourceToUse == RESOURCE.COBALT) {
//				return new int[]{ 0, 0, 0, 0, 0, 10, goldProduction, 0 };
//			}
//			return new int[]{ 0, 0, 0, 0, 0, 0, 0, 0 };
//		}
//		return new int[]{ 0, 0, 0, 0, 0, 0, 0, 0 };
//	}
}
