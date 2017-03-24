using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Campaign {
	public int id;
	public Citizen leader;
	public City targetCity;
	public List<Citizen> registeredGenerals;
	public CAMPAIGN campaignType;
	public HexTile rallyPoint;
	public bool isFull;
	public bool hasStarted;
	public int neededArmyStrength;

	public Campaign(Citizen leader, City targetCity, CAMPAIGN campaignType, int neededArmyStrength = 0){
		this.id = Utilities.SetID (this);
		this.leader = leader;
		this.targetCity = targetCity;
		this.campaignType = campaignType;
		this.registeredGenerals = new List<Citizen> ();
		this.isFull = false;
		this.hasStarted = false;
		this.rallyPoint = null;
		this.neededArmyStrength = neededArmyStrength;
	}

	internal int GetArmyStrength(){
		int total = 0;
		for(int i = 0; i < this.registeredGenerals.Count; i++){
			total += ((General)this.registeredGenerals[i].assignedRole).GetArmyHP();
		}
		return total;
	}
	internal void GoToRallyPoint(){
		
	}
}
