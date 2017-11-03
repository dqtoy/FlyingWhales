using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LycanLair : Lair {

	public LycanLair(LAIR type, HexTile hexTile): base (type, hexTile){
		this.name = "Lycan Lair";
		Initialize();
	}

	#region Overrides
	public override void Initialize(){
		base.Initialize();
        //Create structure
		this.goStructure = this.hexTile.CreateSpecialStructureOnTile(this.type);
    }
	internal override void PerformAction (){
		if (!this.isDead) {
			if (this.region.occupant != null) {
				int chance = UnityEngine.Random.Range(0,100);
				if(chance < 70){
					//Damage to Weapons
					int damage = UnityEngine.Random.Range(7,11);
					DamageToWeapons(damage);
				}
//				else if(chance >= 35 && chance < 70){
//					//Damage to Armors
//					int damage = UnityEngine.Random.Range(7,11);
//					DamageToArmors(damage);
//				}
				else if(chance >= 70 && chance < 80){
					//Damage to Population
					int damage = UnityEngine.Random.Range(3,6);
					DamageToPopulation(damage);
				}else{
					//Destroy Lair
					DestroyLair();
					return;
				}
			}
			GameDate gameDate = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
			gameDate.AddDays (MonsterManager.Instance.daysInterval);
			SchedulingManager.Instance.AddEntry (gameDate.month, gameDate.day, gameDate.year, () => PerformAction ());
		}
	}
	#endregion
}
