using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Kingdom{
	public int id;
	public string name;
	public RACE race;
	public List<City> cities;
	public Citizen king;
	public List<Citizen> successionLine;

	public RESOURCE basicResource;
	public RESOURCE rareResource;

	public string kingdomHistory;

	public Kingdom(RACE race){
		this.id = Utilities.SetID(this);
		this.name = "Kingdom" + this.id.ToString();
		this.race = race;
		this.cities = new List<City>();
		this.king = null;
		this.successionLine = new List<Citizen>();
		this.basicResource = RESOURCE.NONE;
		this.rareResource = RESOURCE.NONE;
		this.kingdomHistory = string.Empty;
	}
}
