using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Raid : GameEvent {
	public Kingdom sourceKingdom;
	public City raidedCity;
	public General general;
	public List<Kingdom> otherKingdoms;
	public string pilfered;

	bool hasBeenDiscovered = false;
	bool hasDeflected = false;
	bool hasDeath = false;
	bool isSuccessful = false;
	Kingdom kingdomToBlame = null;
	Citizen citizenDied = null;

	public Raid(int startWeek, int startMonth, int startYear, Citizen startedBy, City raidedCity, General general) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.RAID;
		this.durationInWeeks = 15;
		this.remainingWeeks = this.durationInWeeks;
		this.sourceKingdom = startedBy.city.kingdom;
		this.general = general;
		this.raidedCity = raidedCity;

		this.otherKingdoms = GetOtherKingdoms ();
		if(this.raidedCity != null){
			this.raidedCity.hexTile.AddEventOnTile(this);
		}
		this.description = startedBy.name + " of " + startedBy.city.kingdom.name + " sent " + this.general.citizen.name + " to raid " + this.raidedCity.name + ".";
		this.startedBy.city.cityHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 
			this.startedBy.city + " has started a raid event against " + raidedCity.name , HISTORY_IDENTIFIER.NONE));
		
		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		Debug.LogError("RAID " + this.description);
	}

	internal override void PerformAction(){
		if (this.general.citizen.isDead) {
			this.DoneEvent();
			return;
		}
		this.remainingWeeks -= 1;
		if(this.remainingWeeks <= 0){
			this.remainingWeeks = 0;
			ActualRaid ();
			DoneEvent ();
		}
	}
	internal override void DoneEvent(){
		if(this.general != null){
			this.general.inAction = false;
		}
		EventManager.Instance.onWeekEnd.RemoveListener (this.PerformAction);
		this.isActive = false;
		EventManager.Instance.onGameEventEnded.Invoke(this);
		this.endMonth = GameManager.Instance.month;
		this.endWeek = GameManager.Instance.days;
		this.endYear = GameManager.Instance.year;

		string deadCitizen = string.Empty;
		string result = "unsuccessful";
		if(this.hasDeath){
			if(this.citizenDied != null){
				deadCitizen = this.citizenDied.name + " was killed during the raid.";
			}
		}
		if (this.isSuccessful) {
			result = "successful";
		}
		if (this.hasBeenDiscovered) {
			if (this.hasDeflected) {
				this.resolution = ((MONTH)this.endMonth).ToString () + " " + this.endWeek + ", " + this.endYear + ". " + this.general.citizen.name + " was " + result + " in raiding " + this.raidedCity.name
				+ " but their identity were discovered." + deadCitizen + " " + this.kingdomToBlame.king.name + " relationship with " + this.startedBy.name + " significantly deteriorated.(DEFLECTED)";

				raidedCity.cityHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 
					"Discovered raiders sent from " + this.kingdomToBlame.name + " (DEFLECTED)", HISTORY_IDENTIFIER.NONE));
			} else {
				this.resolution = ((MONTH)this.endMonth).ToString () + " " + this.endWeek + ", " + this.endYear + ". " + this.general.citizen.name + " was " + result + " in raiding " + this.raidedCity.name
				+ " but their identity were discovered." + deadCitizen + " " + this.raidedCity.kingdom.king.name + " relationship with " + this.startedBy.name + " significantly deteriorated.";

				raidedCity.cityHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 
					"Discovered raiders sent from city " + this.startedByCity.name, HISTORY_IDENTIFIER.NONE));
			}
		} else {
			this.resolution = ((MONTH)this.endMonth).ToString () + " " + this.endWeek + ", " + this.endYear + ". " + this.general.citizen.name + " was " + result + " in raiding " + this.raidedCity.name
			+ " but their identity were not discovered." + deadCitizen;
		
			this.startedByCity.cityHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 
				"Raid against " + this.raidedCity.name + " was " + result, HISTORY_IDENTIFIER.NONE));
		}


		if(this.general.citizen.isDead){
			this.resolution = this.general.citizen.name + " died before the event could finish.";
		}
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
	private void ActualRaid(){
		if(this.general == null){
			return;
		}
		if(this.raidedCity == null){
			return;
		}
		int chance = UnityEngine.Random.Range (0, 100);
//		if(chance < 85){
		if(chance < 40){
			Steal ();
		}
	}

	private void Steal(){
		this.isSuccessful = true;
//		BASE_RESOURCE_TYPE basicResource = BASE_RESOURCE_TYPE.NONE;
//		BASE_RESOURCE_TYPE rareResource = BASE_RESOURCE_TYPE.NONE;

		int stolenGold = (int)(this.raidedCity.goldCount * 0.10f);
//		int stolenBasicResource = (int)(GetRandomBasicResource(ref basicResource) * 0.15f);
//		int stolenRareResource = (int)(GetRandomRareResource(ref rareResource) * 0.15f);

		this.pilfered = string.Empty;
		this.pilfered += stolenGold.ToString() + " Gold";
//		if(basicResource != BASE_RESOURCE_TYPE.NONE){
//			this.pilfered += ", " + stolenBasicResource.ToString() + " " + basicResource.ToString();
//		}
//		if(rareResource != BASE_RESOURCE_TYPE.NONE){
//			this.pilfered += ", " + stolenRareResource.ToString() + " " + rareResource.ToString();
//		}
//
//		this.general.citizen.city.goldCount += stolenGold;
//		this.raidedCity.goldCount -= stolenGold;
//		Debug.Log (this.general.citizen.name + " of " + this.general.citizen.city.name + " has stolen " + stolenGold + " gold from " + this.raidedCity.name + " of " + this.raidedCity.kingdom.name + ".");
//
//		if(basicResource != BASE_RESOURCE_TYPE.NONE){
//			if(basicResource == BASE_RESOURCE_TYPE.WOOD){
//				this.general.citizen.city.lumberCount += stolenBasicResource;
//				this.raidedCity.lumberCount -= stolenBasicResource;
//				Debug.Log (this.general.citizen.name + " of " + this.general.citizen.city.name + " has stolen " + stolenBasicResource + " lumber from " + this.raidedCity.name + " of " + this.raidedCity.kingdom.name + ".");
//			}else if(basicResource == BASE_RESOURCE_TYPE.STONE){
//				this.general.citizen.city.stoneCount += stolenBasicResource;
//				this.raidedCity.stoneCount -= stolenBasicResource;
//				Debug.Log (this.general.citizen.name + " of " + this.general.citizen.city.name + " has stolen " + stolenBasicResource + " stone from " + this.raidedCity.name + " of " + this.raidedCity.kingdom.name + ".");
//			}
//		}
//	
//		if(rareResource != BASE_RESOURCE_TYPE.NONE){
//			if(rareResource == BASE_RESOURCE_TYPE.MANA_STONE){
//				this.general.citizen.city.manaStoneCount += stolenRareResource;
//				this.raidedCity.manaStoneCount -= stolenRareResource;
//				Debug.Log (this.general.citizen.name + " of " + this.general.citizen.city.name + " has stolen " + stolenRareResource + " mana stone from " + this.raidedCity.name + " of " + this.raidedCity.kingdom.name + ".");
//			}else if(rareResource == BASE_RESOURCE_TYPE.MITHRIL){
//				this.general.citizen.city.mithrilCount += stolenRareResource;
//				this.raidedCity.mithrilCount -= stolenRareResource;
//				Debug.Log (this.general.citizen.name + " of " + this.general.citizen.city.name + " has stolen " + stolenRareResource + " mithril from " + this.raidedCity.name + " of " + this.raidedCity.kingdom.name + ".");
//			}else if(rareResource == BASE_RESOURCE_TYPE.COBALT){
//				this.general.citizen.city.cobaltCount += stolenRareResource;
//				this.raidedCity.cobaltCount -= stolenRareResource;
//				Debug.Log (this.general.citizen.name + " of " + this.general.citizen.city.name + " has stolen " + stolenRareResource + " cobalt from " + this.raidedCity.name + " of " + this.raidedCity.kingdom.name + ".");
//			}
//
//		}
			
		GeneralDiscovery (ref this.hasBeenDiscovered, ref this.hasDeflected, ref this.hasDeath, ref this.kingdomToBlame, ref this.citizenDied);

		if(this.hasBeenDiscovered){
			if(this.hasDeflected){
				this.general.citizen.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.general.citizen.name + " raided " + this.raidedCity.name
					+ " with a small group of raiders. The raid was successful. Their presence were discovered and their identities were revealed but " + this.general.citizen.name + " managed to deflect blame to " + kingdomToBlame.name + ".", HISTORY_IDENTIFIER.NONE));
			}else{
				this.general.citizen.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.general.citizen.name + " raided " + this.raidedCity.name
					+ " with a small group of raiders. The raid was successful. Their presence were discovered and their identities were revealed.", HISTORY_IDENTIFIER.NONE));
			}
		}else{
			this.general.citizen.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.general.citizen.name + " raided " + this.raidedCity.name
				+ " with a small group of raiders. The raid was successful. Their presence were not discovered in time.", HISTORY_IDENTIFIER.NONE));
		}

	}
	private void GeneralDiscovery(ref bool hasBeenDiscovered, ref bool hasDeflected, ref bool hasDeath, ref Kingdom kingdomBlame, ref Citizen citizenDied){
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
				citizenDied = deadCitizen;
				deadCitizen.Death (DEATH_REASONS.INTERNATIONAL_WAR);
			}
		}
		int chance = UnityEngine.Random.Range (0, 100);
//		int value = 50;
		int value = 100;
		if(this.general.citizen.skillTraits.Contains(SKILL_TRAIT.STEALTHY)){
			value -= 15;
		}
		if(chance < value){
			//DISCOVERY
			hasBeenDiscovered = true;
//			int amountToAdjust = -5;
			int amountToAdjust = -20;
			if (deadCitizen != null) {
				if (isGovernor || isKing) {
					if (isGovernor) {
//						amountToAdjust = -25;
						amountToAdjust = -40;
					} else {
//						amountToAdjust = -35;
						amountToAdjust = -50;
					}
				} else {
//					amountToAdjust = -15;
					amountToAdjust = -30;
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
							GameManager.Instance.days,
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
							GameManager.Instance.days,
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
						GameManager.Instance.days,
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
					GameManager.Instance.days,
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
			RelationshipKingdom relationship = this.raidedCity.kingdom.GetRelationshipWithOtherKingdom (this.otherKingdoms [i]);
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
