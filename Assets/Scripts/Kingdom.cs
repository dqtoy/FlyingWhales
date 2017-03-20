using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Kingdom{
	public int id;
	public string name;
	public RACE race;
	public List<City> cities;
	public Citizen king;
	public List<Citizen> successionLine;
	public List<Citizen> royaltyList;

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

	internal void UpdateKingSuccession(){
		List<Citizen> orderedMaleRoyalties = this.royaltyList.Where (x => x.gender == GENDER.MALE && x.generation > this.king.generation && x.isDirectDescendant == true).OrderBy(x => x.generation).ThenByDescending(x => x.age).ToList();
		List<Citizen> orderedFemaleRoyalties = this.royaltyList.Where (x => x.gender == GENDER.FEMALE && x.generation > this.king.generation && x.isDirectDescendant == true).OrderBy(x => x.generation).ThenByDescending(x => x.age).ToList();
		List<Citizen> orderedBrotherRoyalties = this.royaltyList.Where (x => x.gender == GENDER.MALE && x.father == this.king.father && x.id != this.king.id).OrderByDescending(x => x.age).ToList();
		List<Citizen> orderedSisterRoyalties = this.royaltyList.Where (x => x.gender == GENDER.FEMALE && x.father == this.king.father && x.id != this.king.id).OrderByDescending(x => x.age).ToList();

		List<Citizen> orderedRoyalties = orderedMaleRoyalties.Concat (orderedFemaleRoyalties).Concat(orderedBrotherRoyalties).Concat(orderedSisterRoyalties).ToList();

		this.successionLine.Clear ();
		this.successionLine = orderedRoyalties;
	}
}
