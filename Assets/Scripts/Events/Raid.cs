﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Raid : GameEvent {
	public Kingdom sourceKingdom;
	public City raidedCity;
	public General general;
	public List<Kingdom> otherKingdoms;

	public Raid(int startWeek, int startMonth, int startYear, Citizen startedBy, City raidedCity = null, General general = null) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.RAID;
		this.durationInWeeks = 3;
		this.remainingWeeks = this.durationInWeeks;
		this.sourceKingdom = startedBy.city.kingdom;
		if(general == null){
			this.general = GetGeneral (this.sourceKingdom);
		}else{
			this.general = general;
		}
		if(raidedCity == null){
			this.raidedCity = GetRaidedCity();
		}else{
			this.raidedCity = raidedCity;
		}
		this.otherKingdoms = GetOtherKingdoms ();
		if(this.raidedCity != null){
			this.description = startedBy.name + " is sending someone to raid " + this.raidedCity.name + " of " + this.raidedCity.kingdom.name;
			this.raidedCity.hexTile.AddEventOnTile(this);
		}
		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		Debug.LogError("RAID");
	}

	internal override void PerformAction(){
		this.durationInWeeks -= 1;
		if(this.durationInWeeks <= 0){
			this.durationInWeeks = 0;
			ActualRaid ();
			DoneEvent ();
		}
	}
	internal override void DoneEvent(){
		if(this.general != null){
			this.general.inAction = false;
		}
		this.general = null;
		EventManager.Instance.onWeekEnd.RemoveListener (this.PerformAction);
		this.isActive = false;
		EventManager.Instance.onGameEventEnded.Invoke(this);
		//		EventManager.Instance.allEvents [EVENT_TYPES.ESPIONAGE].Remove (this);
	}
	private List<Kingdom> GetOtherKingdoms(){
		if(this.raidedCity == null){
			return null;
		}
		List<Kingdom> kingdoms = new List<Kingdom> ();
		for(int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++){
			if(KingdomManager.Instance.allKingdoms[i].id != this.sourceKingdom.id && KingdomManager.Instance.allKingdoms[i].id != this.raidedCity.kingdom.id){
				kingdoms.Add (KingdomManager.Instance.allKingdoms [i]);
			}
		}
		return kingdoms;
	}
	private City GetRaidedCity(){
		if(this.general == null){
			return null;
		}
		List<City> adjacentCities = new List<City> ();
		for(int i = 0; i < this.general.citizen.city.hexTile.connectedTiles.Count; i++){
			if(this.general.citizen.city.hexTile.connectedTiles[i].isOccupied){
				if(this.general.citizen.city.hexTile.connectedTiles[i].city.kingdom.id != this.general.citizen.city.kingdom.id){
					adjacentCities.Add (this.general.citizen.city.hexTile.connectedTiles[i].city);
				}
			}

		}

		if(adjacentCities.Count > 0){
			return adjacentCities [UnityEngine.Random.Range (0, adjacentCities.Count)];
		}else{
			return null;
		}
	}
	private General GetGeneral(Kingdom kingdom){
		List<Citizen> unwantedGovernors = GetUnwantedGovernors (kingdom.king);
		List<General> generals = new List<General> ();
		for(int i = 0; i < kingdom.cities.Count; i++){
			if(!IsItThisGovernor(kingdom.cities[i].governor, unwantedGovernors)){
				for(int j = 0; j < kingdom.cities[i].citizens.Count; j++){
					if (!kingdom.cities [i].citizens [j].isDead) {
						if (kingdom.cities [i].citizens [j].assignedRole != null && kingdom.cities [i].citizens [j].role == ROLE.GENERAL) {
							if(kingdom.cities [i].citizens [j].assignedRole is General){
								if (!((General)kingdom.cities [i].citizens [j].assignedRole).inAction) {
									generals.Add (((General)kingdom.cities [i].citizens [j].assignedRole));
								}
							}
						}
					}
				}
			}
		}

		if(generals.Count > 0){
			int random = UnityEngine.Random.Range (0, generals.Count);
			generals [random].inAction = true;
			return generals [random];
		}else{
			Debug.Log (kingdom.king.name + " CAN'T SEND GENERAL BECAUSE THERE IS NONE!");
			return null;
		}
	}
	private void ActualRaid(){
		if(this.general == null){
			return;
		}
		if(this.raidedCity == null){
			return;
		}
		int chance = UnityEngine.Random.Range (0, 100);
		if(chance < 25){
			Steal ();
		}
	}

	private void Steal(){
		BASE_RESOURCE_TYPE basicResource = BASE_RESOURCE_TYPE.NONE;
		BASE_RESOURCE_TYPE rareResource = BASE_RESOURCE_TYPE.NONE;

		int stolenGold = (int)(this.raidedCity.goldCount * 0.10f);
		int stolenBasicResource = (int)(GetRandomBasicResource(ref basicResource) * 0.15f);
		int stolenRareResource = (int)(GetRandomRareResource(ref rareResource) * 0.15f);

		this.general.citizen.city.goldCount += stolenGold;
		this.raidedCity.goldCount -= stolenGold;
		Debug.Log (this.general.citizen.name + " of " + this.general.citizen.city.name + " has stolen " + stolenGold + " gold from " + this.raidedCity.name + " of " + this.raidedCity.kingdom.name + ".");

		if(basicResource != BASE_RESOURCE_TYPE.NONE){
			if(basicResource == BASE_RESOURCE_TYPE.WOOD){
				this.general.citizen.city.lumberCount += stolenBasicResource;
				this.raidedCity.lumberCount -= stolenBasicResource;
				Debug.Log (this.general.citizen.name + " of " + this.general.citizen.city.name + " has stolen " + stolenBasicResource + " lumber from " + this.raidedCity.name + " of " + this.raidedCity.kingdom.name + ".");
			}else if(basicResource == BASE_RESOURCE_TYPE.STONE){
				this.general.citizen.city.stoneCount += stolenBasicResource;
				this.raidedCity.stoneCount -= stolenBasicResource;
				Debug.Log (this.general.citizen.name + " of " + this.general.citizen.city.name + " has stolen " + stolenBasicResource + " stone from " + this.raidedCity.name + " of " + this.raidedCity.kingdom.name + ".");
			}
		}
	
		if(rareResource != BASE_RESOURCE_TYPE.NONE){
			if(rareResource == BASE_RESOURCE_TYPE.MANA_STONE){
				this.general.citizen.city.manaStoneCount += stolenRareResource;
				this.raidedCity.manaStoneCount -= stolenRareResource;
				Debug.Log (this.general.citizen.name + " of " + this.general.citizen.city.name + " has stolen " + stolenRareResource + " mana stone from " + this.raidedCity.name + " of " + this.raidedCity.kingdom.name + ".");
			}else if(rareResource == BASE_RESOURCE_TYPE.MITHRIL){
				this.general.citizen.city.mithrilCount += stolenRareResource;
				this.raidedCity.mithrilCount -= stolenRareResource;
				Debug.Log (this.general.citizen.name + " of " + this.general.citizen.city.name + " has stolen " + stolenRareResource + " mithril from " + this.raidedCity.name + " of " + this.raidedCity.kingdom.name + ".");
			}else if(rareResource == BASE_RESOURCE_TYPE.COBALT){
				this.general.citizen.city.cobaltCount += stolenRareResource;
				this.raidedCity.cobaltCount -= stolenRareResource;
				Debug.Log (this.general.citizen.name + " of " + this.general.citizen.city.name + " has stolen " + stolenRareResource + " cobalt from " + this.raidedCity.name + " of " + this.raidedCity.kingdom.name + ".");
			}

		}
		bool hasBeenDiscovered = false;
		bool hasDeflected = false;
		bool hasDeath = false;
		Kingdom kingdomToBlame = null;
		GeneralDiscovery (ref hasBeenDiscovered, ref hasDeflected, ref hasDeath, ref kingdomToBlame);

		if(hasBeenDiscovered){
			if(hasDeflected){
				this.general.citizen.history.Add (new History (GameManager.Instance.month, GameManager.Instance.week, GameManager.Instance.year, this.general.citizen.name + " raided " + this.raidedCity.name
					+ " with a small group of raiders. The raid was successful. Their presence were discovered and their identities were revealed but " + this.general.citizen.name + " managed to deflect blame to " + kingdomToBlame.name + ".", HISTORY_IDENTIFIER.NONE));

			}else{
				this.general.citizen.history.Add (new History (GameManager.Instance.month, GameManager.Instance.week, GameManager.Instance.year, this.general.citizen.name + " raided " + this.raidedCity.name
					+ " with a small group of raiders. The raid was successful. Their presence were discovered and their identities were revealed.", HISTORY_IDENTIFIER.NONE));
			}
		}else{
			this.general.citizen.history.Add (new History (GameManager.Instance.month, GameManager.Instance.week, GameManager.Instance.year, this.general.citizen.name + " raided " + this.raidedCity.name
				+ " with a small group of raiders. The raid was successful. Their presence were not discovered in time.", HISTORY_IDENTIFIER.NONE));
		}

	}
	private void GeneralDiscovery(ref bool hasBeenDiscovered, ref bool hasDeflected, ref bool hasDeath, ref Kingdom kingdomBlame){
		Citizen deadCitizen = null;
		bool isGovernor = false;
		bool isKing = false;
		int deathChance = UnityEngine.Random.Range (0, 100);
		if(deathChance < 2){
			List<Citizen> citizens = new List<Citizen> ();
			for(int i = 0; i < this.raidedCity.citizens.Count; i++){
				if(!this.raidedCity.citizens[i].isDead){
					citizens.Add (this.raidedCity.citizens [i]);
				}
			}
			if(citizens.Count > 0){
				deadCitizen = citizens [UnityEngine.Random.Range (0, citizens.Count)];
				isGovernor = deadCitizen.isGovernor;
				isKing = deadCitizen.isKing;
				hasDeath = true;
				deadCitizen.Death (DEATH_REASONS.INTERNATIONAL_WAR);
			}
		}
		int chance = UnityEngine.Random.Range (0, 100);
		int value = 50;
		if(this.general.citizen.skillTraits.Contains(SKILL_TRAIT.STEALTHY)){
			value -= 15;
		}
		if(chance < value){
			//DISCOVERY
			hasBeenDiscovered = true;
			int amountToAdjust = -5;
			if (deadCitizen != null) {
				if (isGovernor || isKing) {
					if (isGovernor) {
						amountToAdjust = -25;
					} else {
						amountToAdjust = -35;
					}
				} else {
					amountToAdjust = -15;
				}
			}
			if(this.general.citizen.behaviorTraits.Contains(BEHAVIOR_TRAIT.SCHEMING)){
				int deflectChance = UnityEngine.Random.Range(0,100);
				if(deflectChance < 35){
					Kingdom kingdomToBlame = GetRandomKingdomToBlame ();
					if(kingdomToBlame != null){
						hasDeflected = true;
						kingdomBlame = kingdomToBlame;
						RelationshipKings relationship = this.raidedCity.kingdom.king.SearchRelationshipByID(kingdomToBlame.king.id);
						relationship.AdjustLikeness (amountToAdjust, EVENT_TYPES.RAID);
						relationship.relationshipHistory.Add (new History (
							GameManager.Instance.month,
							GameManager.Instance.week,
							GameManager.Instance.year,
							this.raidedCity.kingdom.king.name +  " caught a raider, that was from " + kingdomToBlame.name,
							HISTORY_IDENTIFIER.KING_RELATIONS,
							false
						));
					}else{
						RelationshipKings relationship = this.raidedCity.kingdom.king.SearchRelationshipByID(this.sourceKingdom.king.id);
						relationship.AdjustLikeness (amountToAdjust, EVENT_TYPES.RAID);
						relationship.relationshipHistory.Add (new History (
							GameManager.Instance.month,
							GameManager.Instance.week,
							GameManager.Instance.year,
							this.raidedCity.kingdom.king.name +  " caught a raider, that was from " + this.sourceKingdom.name,
							HISTORY_IDENTIFIER.KING_RELATIONS,
							false
						));
					}
				}else{
					RelationshipKings relationship = this.raidedCity.kingdom.king.SearchRelationshipByID(this.sourceKingdom.king.id);
					relationship.AdjustLikeness (amountToAdjust, EVENT_TYPES.RAID);
					relationship.relationshipHistory.Add (new History (
						GameManager.Instance.month,
						GameManager.Instance.week,
						GameManager.Instance.year,
						this.raidedCity.kingdom.king.name +  " caught a raider, that was from " + this.sourceKingdom.name,
						HISTORY_IDENTIFIER.KING_RELATIONS,
						false
					));
				}
			}else{
				RelationshipKings relationship = this.raidedCity.kingdom.king.SearchRelationshipByID(this.sourceKingdom.king.id);
				relationship.AdjustLikeness (amountToAdjust, EVENT_TYPES.RAID);
				relationship.relationshipHistory.Add (new History (
					GameManager.Instance.month,
					GameManager.Instance.week,
					GameManager.Instance.year,
					this.raidedCity.kingdom.king.name +  " caught a raider, that was from " + this.sourceKingdom.name,
					HISTORY_IDENTIFIER.KING_RELATIONS,
					false
				));
			}
		}
	}
	private Kingdom GetRandomKingdomToBlame(){
		if(this.otherKingdoms == null){
			return null;
		}
		List<Kingdom> otherAdjacentKingdoms = new List<Kingdom> ();
		for(int i = 0; i < this.otherKingdoms.Count; i++){
			Relationship<Kingdom> relationship = this.raidedCity.kingdom.GetRelationshipWithOtherKingdom (this.otherKingdoms [i]);
			if(relationship.isAdjacent){
				otherAdjacentKingdoms.Add (this.otherKingdoms [i]);
			}
		}

		if(otherAdjacentKingdoms.Count > 0){
			return otherAdjacentKingdoms [UnityEngine.Random.Range (0, otherAdjacentKingdoms.Count)];
		}else{
			return null;
		}
	}
	private int GetRandomBasicResource(ref BASE_RESOURCE_TYPE resourceType){
		if(this.raidedCity.lumberCount > 0 && this.raidedCity.stoneCount > 0){
			int chance = UnityEngine.Random.Range (0, 2);
			if(chance == 0){
				resourceType = BASE_RESOURCE_TYPE.WOOD;
				return this.raidedCity.lumberCount;
			}else{
				resourceType = BASE_RESOURCE_TYPE.STONE;
				return this.raidedCity.stoneCount;
			}
		}else if(this.raidedCity.lumberCount <= 0 && this.raidedCity.stoneCount > 0){
			resourceType = BASE_RESOURCE_TYPE.STONE;
			return this.raidedCity.stoneCount;
		}else if(this.raidedCity.lumberCount > 0 && this.raidedCity.stoneCount <= 0){
			resourceType = BASE_RESOURCE_TYPE.WOOD;
			return this.raidedCity.lumberCount;
		}else{
			resourceType = BASE_RESOURCE_TYPE.NONE;
			return 0;
		}
	}

	private int GetRandomRareResource(ref BASE_RESOURCE_TYPE resourceType){
		if(this.raidedCity.manaStoneCount > 0 && this.raidedCity.mithrilCount > 0 && this.raidedCity.cobaltCount > 0){
			int chance = UnityEngine.Random.Range (0, 3);
			if(chance == 0){
				resourceType = BASE_RESOURCE_TYPE.MANA_STONE;
				return this.raidedCity.manaStoneCount;
			}else if(chance == 1){
				resourceType = BASE_RESOURCE_TYPE.MITHRIL;
				return this.raidedCity.mithrilCount;
			}else {
				resourceType = BASE_RESOURCE_TYPE.COBALT;
				return this.raidedCity.cobaltCount;
			}
		}else if(this.raidedCity.manaStoneCount <= 0 && this.raidedCity.mithrilCount > 0 && this.raidedCity.cobaltCount > 0){
			int chance = UnityEngine.Random.Range (0, 2);
			if(chance == 0){
				resourceType = BASE_RESOURCE_TYPE.MITHRIL;
				return this.raidedCity.mithrilCount;
			}else {
				resourceType = BASE_RESOURCE_TYPE.COBALT;
				return this.raidedCity.cobaltCount;
			}
		}else if(this.raidedCity.manaStoneCount > 0 && this.raidedCity.mithrilCount <= 0 && this.raidedCity.cobaltCount > 0){
			int chance = UnityEngine.Random.Range (0, 2);
			if(chance == 0){
				resourceType = BASE_RESOURCE_TYPE.MANA_STONE;
				return this.raidedCity.manaStoneCount;
			}else {
				resourceType = BASE_RESOURCE_TYPE.COBALT;
				return this.raidedCity.cobaltCount;
			}
		}else if(this.raidedCity.manaStoneCount > 0 && this.raidedCity.mithrilCount > 0 && this.raidedCity.cobaltCount <= 0){
			int chance = UnityEngine.Random.Range (0, 2);
			if(chance == 0){
				resourceType = BASE_RESOURCE_TYPE.MANA_STONE;
				return this.raidedCity.manaStoneCount;
			}else {
				resourceType = BASE_RESOURCE_TYPE.MITHRIL;
				return this.raidedCity.mithrilCount;
			}
		}else if(this.raidedCity.manaStoneCount > 0 && this.raidedCity.mithrilCount <= 0 && this.raidedCity.cobaltCount <= 0){
			resourceType = BASE_RESOURCE_TYPE.MANA_STONE;
			return this.raidedCity.manaStoneCount;
		}else if(this.raidedCity.manaStoneCount <= 0 && this.raidedCity.mithrilCount > 0 && this.raidedCity.cobaltCount <= 0){
			resourceType = BASE_RESOURCE_TYPE.MITHRIL;
			return this.raidedCity.mithrilCount;
		}else if(this.raidedCity.manaStoneCount <= 0 && this.raidedCity.mithrilCount <= 0 && this.raidedCity.cobaltCount > 0){
			resourceType = BASE_RESOURCE_TYPE.COBALT;
			return this.raidedCity.cobaltCount;
		}else{
			resourceType = BASE_RESOURCE_TYPE.NONE;
			return 0;
		}
	}
}
