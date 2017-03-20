using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
	}

	/*
	 * Initialize City With Initial Citizens aka. Families
	 * */
	protected void CreateInitialFamilies(){

	}
}
