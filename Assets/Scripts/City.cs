using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class City{

	public int id;
	public string name;
	public HexTile hexTile;
	public Kingdom kingdom;
	public List<HexTile> ownedTiles;
	public List<Citizen> citizens;
	public string cityHistory;
	public bool hasKing;

	[Space(10)] //Resources
	public int sustainability;
	public int lumberCount;
	public int stoneCount;
	public int manaStoneCount;
	public int mithrilCount;
	public int cobaltCount;
	public int goldCount;
	public int[] allResourceProduction;

	[Space(5)]
	public bool isStarving;

	//generals
	//incoming generals

	public City(HexTile hexTile, Kingdom kingdom){
		this.id = Utilities.SetID(this);
		this.name = "City" + this.id.ToString();
		this.hexTile = hexTile;
		this.kingdom = kingdom;
		this.ownedTiles = new List<HexTile>();
		this.citizens = new List<Citizen>();
		this.cityHistory = string.Empty;
		this.hasKing = false;

		CreateInitialFamilies();
		EventManager.StartListening ("Starvation", Starvation);
	}

	/*
	 * Initialize City With Initial Citizens aka. Families
	 * */
	protected void CreateInitialFamilies(){

	}

	internal void Starvation(){
		if(this.isStarving){
			int deathChance = UnityEngine.Random.Range (0, 100);
			if(deathChance < 5){
				int youngestAge = this.citizens.Min (x => x.age);
				List<Citizen> youngestCitizens = this.citizens.Where(x => x.age == youngestAge);
				youngestCitizens [UnityEngine.Random.Range].DeathByStarvation ();
			}
		}
	}
}
