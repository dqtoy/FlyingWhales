using UnityEngine;
using System.Collections;

[System.Serializable]
public struct ProductionPointsSpend  {
	public int power;
	public int defense;
	public int stability;

	public ProductionPointsSpend(int power, int defense, int stability){
		this.power = power;
		this.defense = defense;
		this.stability = stability;
	}
}
