using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Role {
	public Citizen citizen;

	public List<HexTile> path;
	public HexTile location;
	public HexTile targetLocation;

	public GameObject avatar;
	public int daysBeforeMoving;

	public Role(Citizen citizen){
		this.location = citizen.city.hexTile;
		this.targetLocation = null;
		this.path = new List<HexTile> ();
		this.avatar = null;
		this.daysBeforeMoving = 0;
	}
	internal void DestroyGO(){
		if(this.avatar != null){
			GameObject.Destroy (this.avatar);
		}
	}

	internal void Attack(){
		if(this.avatar != null){
			if(this.avatar.GetComponent<RaiderAvatar> ().isDirectionUp){
				this.avatar.GetComponent<RaiderAvatar> ().animator.Play ("Attack_Up");
			}else{
				this.avatar.GetComponent<RaiderAvatar> ().animator.Play ("Attack");
			}
		}
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

	internal virtual void Initialize(GameEvent gameEvent){}




}
