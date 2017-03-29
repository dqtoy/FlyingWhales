using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class MarriedCouple {

	public Citizen husband;
	public Citizen wife;

	public MarriedCouple(Citizen husband, Citizen wife){
		this.husband = husband;
		this.wife = wife;
	}
}
