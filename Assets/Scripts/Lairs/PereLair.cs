using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PereLair : Lair {

	public PereLair(LAIR type, HexTile hexTile): base (type, hexTile){
		this.name = "Pere Lair";
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
				int chance = UnityEngine.Random.Range (0, 100);
				if (chance < 90) {
					Kingdom kingdom = this.region.occupant.kingdom;
					int kingdomEffDefense = kingdom.effectiveArmor;
					int damage = (int)((kingdomEffDefense / kingdom.cities.Count) / 18);
					DamageToCityDefense (damage);
				} else {
					DestroyLair ();
					return;
				}
			}
			GameDate gameDate = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
			gameDate.AddDays (5);
			SchedulingManager.Instance.AddEntry (gameDate.month, gameDate.day, gameDate.year, () => PerformAction ());
		}
	}
	#endregion

}
