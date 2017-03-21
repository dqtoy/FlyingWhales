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
//	public List<Citizen> royaltyList;

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
		List<Citizen> orderedMaleRoyalties = this.successionLine.Where (x => x.gender == GENDER.MALE && x.generation > this.king.generation && x.isDirectDescendant == true).OrderBy(x => x.generation).ThenByDescending(x => x.age).ToList();
		List<Citizen> orderedFemaleRoyalties = this.successionLine.Where (x => x.gender == GENDER.FEMALE && x.generation > this.king.generation && x.isDirectDescendant == true).OrderBy(x => x.generation).ThenByDescending(x => x.age).ToList();
		List<Citizen> orderedBrotherRoyalties = this.successionLine.Where (x => x.gender == GENDER.MALE && x.father.id == this.king.father.id && x.id != this.king.id).OrderByDescending(x => x.age).ToList();
		List<Citizen> orderedSisterRoyalties = this.successionLine.Where (x => x.gender == GENDER.FEMALE && x.father.id == this.king.father.id && x.id != this.king.id).OrderByDescending(x => x.age).ToList();

		List<Citizen> orderedRoyalties = orderedMaleRoyalties.Concat (orderedFemaleRoyalties).Concat(orderedBrotherRoyalties).Concat(orderedSisterRoyalties).ToList();

		this.successionLine.Clear ();
		this.successionLine = orderedRoyalties;
	}

	internal void AssignNewLord(Citizen newLord){
//		RoyaltyEventDelegate.TriggerMassChangeLoyalty(newLord, this.assignedLord);

		if(!newLord.isDirectDescendant){
			//				RoyaltyEventDelegate.TriggerChangeIsDirectDescendant (false);
			Utilities.ChangeDescendantsRecursively (newLord, true);
			Utilities.ChangeDescendantsRecursively (this.king, false);
		}
		this.king = newLord;
		this.successionLine.Clear();
		ChangeSuccessionLineRescursively (newLord);
		this.successionLine.AddRange (GetSiblings (newLord));
		UpdateKingSuccession ();
	}

	internal void ChangeSuccessionLineRescursively(Citizen royalty){
		if(this.king.id != royalty.id){
			if(!royalty.isDead){
				this.successionLine.Add (royalty);
			}
		}

		for(int i = 0; i < royalty.children.Count; i++){
			if(royalty.children[i] != null){
				this.ChangeSuccessionLineRescursively (royalty.children [i]);
			}
		}
	}

	internal List<Citizen> GetSiblings(Citizen royalty){
		List<Citizen> siblings = new List<Citizen> ();
		for(int i = 0; i < royalty.mother.children.Count; i++){
			if(royalty.mother.children[i].id != royalty.id){
				if(!royalty.mother.children[i].isDead){
					siblings.Add (royalty.mother.children [i]);
				}
			}
		}

		return siblings;
	}
}
