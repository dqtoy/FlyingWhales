using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Role {
	public Citizen citizen;

	public Role(Citizen citizen){
		this.citizen = citizen;
	}

	internal virtual int[] GetResourceProduction(){
		int goldProduction = 40;
//		if (this.citizen.city.governor.skillTraits.Contains (SKILL_TRAIT.LAVISH)) {
//			goldProduction -= 5;
//		} else if (this.citizen.city.governor.skillTraits.Contains (SKILL_TRAIT.THRIFTY)) {
//			goldProduction += 5;
//		}
//
//		if (this.citizen.city.kingdom.king.skillTraits.Contains (SKILL_TRAIT.LAVISH)) {
//			goldProduction -= 5;
//		} else if (this.citizen.city.kingdom.king.skillTraits.Contains (SKILL_TRAIT.THRIFTY)) {
//			goldProduction += 5;
//		}
		return new int[]{ 0, 0, 0, 0, 0, 0, goldProduction, 0 };
	}

	internal virtual void OnDeath(){}

	internal virtual void Initialize(GameEvent gameEvent, List<HexTile> path){}

	internal virtual void DestroyGO(){}

	internal virtual void Attack(){}
}
