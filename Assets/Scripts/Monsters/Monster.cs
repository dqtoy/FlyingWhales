using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Monster {

	public MONSTER type;
	public int hp;
	public GameObject avatar;
	public HexTile originHextile;
	public HexTile targetLocation;
	public HexTile location;
	public bool markAsDead;
	public bool isDead;

	public List<HexTile> path;

	public Monster(MONSTER type, HexTile originHextile){
		this.type = type;
		this.hp = GetMonsterHP();
		this.avatar = null;
		this.originHextile = originHextile;
		this.location = originHextile;
		this.targetLocation = null;
		this.markAsDead = false;
		this.isDead = false;
		this.path = new List<HexTile>();
	}

	private int GetMonsterHP(){
		switch (this.type){
		case MONSTER.LYCAN:
			return 50;
		}
		return 0;
	}
	internal void UpdateUI(){
		if(this.avatar != null){
			if(this is Lycan){
				this.avatar.GetComponent<LycanAvatar>().UpdateUI();
			}
		}
	}
	#region Virtual
	internal virtual void Initialize(){}
	internal virtual void Attack(){}
	internal virtual void Death(){}

	internal virtual void DoneAction(){}
	#endregion
}