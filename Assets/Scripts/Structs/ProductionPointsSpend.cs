using UnityEngine;
using System.Collections;

[System.Serializable]
public struct ProductionPointsSpend  {
	public int power;
	public int defense;
	public int tech;

	public ProductionPointsSpend(int power, int defense, int tech){
		this.power = power;
		this.defense = defense;
		this.tech = tech;
	}
}
