using UnityEngine;
using System.Collections;

[System.Serializable]
public struct ProductionPointsSpend  {
	public int power;
	public int defense;
	public int happiness;

	public ProductionPointsSpend(int power, int defense, int happiness){
		this.power = power;
		this.defense = defense;
		this.happiness = happiness;
	}
}
