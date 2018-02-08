using UnityEngine;
using System.Collections;

[System.Serializable]
public struct Production {
	public int foodCost;
	public int civilianCost;
	public int resourceCost;
	public int duration;

	public void Combine(Production production1, Production production2){
		this.foodCost = production1.foodCost + production2.foodCost;
		this.civilianCost = production1.civilianCost + production2.civilianCost;
		this.resourceCost = production1.resourceCost + production2.resourceCost;
		this.duration = production1.duration + production2.duration;
	}
}
