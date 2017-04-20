using UnityEngine;
using System.Collections;

[System.Serializable]
public class KingdomWar {
//	public Kingdom sourceKingdom;
	public Kingdom targetKingdom;

	public int exhaustion;
	public int battlesWon;
	public int battlesLost;
	public int citiesWon;
	public int citiesLost;

	public KingdomWar(Kingdom targetKingdom){
//		this.sourceKingdom = sourceKingdom;
		this.targetKingdom = targetKingdom;
		this.exhaustion = 0;
		this.battlesWon = 0;
		this.battlesLost = 0;
		this.citiesWon = 0;
		this.citiesLost = 0;
	}

	internal void ResetKingdomWar(){
		this.exhaustion = 0;
		this.battlesWon = 0;
		this.battlesLost = 0;
		this.citiesWon = 0;
		this.citiesLost = 0;
	}

	internal void AdjustExhaustion(int amount){
		this.exhaustion += amount;
		if(this.exhaustion > 100){
			this.exhaustion = 100;
		}
		if(this.exhaustion < 0){
			this.exhaustion = 0;
		}
	}
}
