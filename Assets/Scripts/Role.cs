using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Role {
	public Citizen citizen;

	public List<HexTile> path;
	public HexTile location;
	public HexTile targetLocation;
	public City targetCity;

	public GameObject avatar;
	public int daysBeforeMoving;
	public bool isDestroyed;
	public bool hasAttacked;

	public Role(Citizen citizen){
		this.citizen = citizen;
		this.location = citizen.city.hexTile;
		this.targetLocation = null;
		this.targetCity = null;
		this.path = new List<HexTile> ();
		this.avatar = null;
		this.daysBeforeMoving = 0;
		this.isDestroyed = false;
		this.hasAttacked = false;
	}
	internal void DestroyGO(){
		if(this.avatar != null){
			UIManager.Instance.HideSmallInfo ();
			GameObject.Destroy (this.avatar);
		}
		this.isDestroyed = true;
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
	internal virtual void OnDeath(){
		this.DestroyGO ();
	}
	internal virtual void Initialize(GameEvent gameEvent){}
	internal virtual void Attack(){}
}
