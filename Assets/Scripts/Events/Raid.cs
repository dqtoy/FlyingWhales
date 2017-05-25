using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Raid : GameEvent {
	public Kingdom sourceKingdom;
	public Kingdom targetKingdom;
	public City raidedCity;
	public List<Kingdom> otherKingdoms;
	public string pilfered;

	private bool hasBeenDiscovered;
	private bool hasDeflected;
	private bool hasDeath;
	private bool isSuccessful;
	private bool hasArrived;
	private Kingdom kingdomToBlame;
//	List<Citizen> citizenDied = new List<Citizen>();

	public Raid(int startWeek, int startMonth, int startYear, Citizen startedBy, City raidedCity) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.RAID;
		this.durationInDays = 20;
		this.remainingDays = this.durationInDays;
		this.sourceKingdom = startedBy.city.kingdom;
		this.targetKingdom = raidedCity.kingdom;
		this.raidedCity = raidedCity;
		this.hasBeenDiscovered = false;
		this.hasDeflected = false;
		this.hasDeath = false;
		this.isSuccessful = false;
		this.hasArrived = false;
		this.kingdomToBlame = null;

		this.otherKingdoms = GetOtherKingdoms ();
		if(this.raidedCity != null){
			this.raidedCity.hexTile.AddEventOnTile(this);
		}

		this.startedBy.city.cityHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 
			this.startedBy.city + " has started a raid event against " + raidedCity.name , HISTORY_IDENTIFIER.NONE));
		
		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
//		Debug.LogError("RAID " + this.description);

		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Raid", "event_title");
		newLogTitle.AddToFillers (this.raidedCity, this.raidedCity.name);

		Log raidStartLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Raid", "start");
		raidStartLog.AddToFillers (this.startedByCity, this.startedByCity.name);
		raidStartLog.AddToFillers (this.raidedCity, this.raidedCity.name);

		DeflectBlame ();

		this.EventIsCreated ();


	}

	internal override void PerformAction(){
		this.remainingDays -= 1;
		if(this.remainingDays <= 0){
			this.remainingDays = 0;
			ActualRaid ();
			DoneEvent ();
		}else{
			if(this.remainingDays < (this.durationInDays - 7)){
				if(!this.hasArrived){
					this.hasArrived = true;
					Arrival ();
				}
				RaidPartyDiscovery ();
				AccidentKilling ();
			}
		}
	}
	internal override void DoneEvent(){
		EventManager.Instance.onWeekEnd.RemoveListener (this.PerformAction);
		this.isActive = false;
//		EventManager.Instance.onGameEventEnded.Invoke(this);
		this.endMonth = GameManager.Instance.month;
		this.endDay = GameManager.Instance.days;
		this.endYear = GameManager.Instance.year;

		if(this.hasBeenDiscovered){
			this._warTrigger = WAR_TRIGGER.DISCOVERED_RAID_NO_DEATH;
			if(this.hasDeath){
				this._warTrigger = WAR_TRIGGER.DISCOVERED_RAID_WITH_DEATH;
			}
			RelationshipKings relationship = this.GetRelationship ();
			if (relationship != null) {
				this.targetKingdom.king.WarTrigger (relationship, this, this.targetKingdom.kingdomTypeData);
			}
		}

//		string deadCitizen = string.Empty;
//		string result = "unsuccessful";
//		if(this.hasDeath){
//			if(this.citizenDied != null){
//				deadCitizen = this.citizenDied.name + " was killed during the raid.";
//			}
//		}
//		if (this.isSuccessful) {
//			result = "successful";
//		}
//		if (this.hasBeenDiscovered) {
//			if (this.hasDeflected) {
//				this.resolution = ((MONTH)this.endMonth).ToString () + " " + this.endDay + ", " + this.endYear + ". " + this.general.citizen.name + " was " + result + " in raiding " + this.raidedCity.name
//				+ " but their identity were discovered." + deadCitizen + " " + this.kingdomToBlame.king.name + " relationship with " + this.startedBy.name + " significantly deteriorated.(DEFLECTED)";
//
//				raidedCity.cityHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 
//					"Discovered raiders sent from " + this.kingdomToBlame.name + " (DEFLECTED)", HISTORY_IDENTIFIER.NONE));
//			} else {
//				this.resolution = ((MONTH)this.endMonth).ToString () + " " + this.endDay + ", " + this.endYear + ". " + this.general.citizen.name + " was " + result + " in raiding " + this.raidedCity.name
//				+ " but their identity were discovered." + deadCitizen + " " + this.raidedCity.kingdom.king.name + " relationship with " + this.startedBy.name + " significantly deteriorated.";
//
//				raidedCity.cityHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 
//					"Discovered raiders sent from city " + this.startedByCity.name, HISTORY_IDENTIFIER.NONE));
//			}
//		} else {
//			this.resolution = ((MONTH)this.endMonth).ToString () + " " + this.endDay + ", " + this.endYear + ". " + this.general.citizen.name + " was " + result + " in raiding " + this.raidedCity.name
//			+ " but their identity were not discovered." + deadCitizen;
//		
//			this.startedByCity.cityHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 
//				"Raid against " + this.raidedCity.name + " was " + result, HISTORY_IDENTIFIER.NONE));
//		}
//		if(this.general.citizen.isDead){
//			this.resolution = this.general.citizen.name + " died before the event could finish.";
//		}
//		EventManager.Instance.allEvents [EVENT_TYPES.ESPIONAGE].Remove (this);
	}
	private List<Kingdom> GetOtherKingdoms(){
		if(this.raidedCity == null){
			return null;
		}
		List<Kingdom> kingdoms = new List<Kingdom> ();
		for(int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++){
			if(KingdomManager.Instance.allKingdoms[i].id != this.sourceKingdom.id && KingdomManager.Instance.allKingdoms[i].id != this.targetKingdom.id && KingdomManager.Instance.allKingdoms[i].isAlive()){
				kingdoms.Add (KingdomManager.Instance.allKingdoms [i]);
			}
		}
		return kingdoms;
	}

	//Raid party arrives at city
	private void Arrival(){
		if(this.raidedCity == null){
			return;
		}
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Raid", "raid_arrival");
		newLog.AddToFillers (this.raidedCity, this.raidedCity.name);
//		List<object> arrivedLogObjects = new List<object> {
//			this.raidedCity,
//		};
//		this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Raid", "raid_arrival", arrivedLogObjects);
	}

	//Moment that raid party is going to steal from city
	private void ActualRaid(){
		if(this.raidedCity == null){
			return;
		}

		int chance = UnityEngine.Random.Range (0, 100);
//		if(chance < 85){
		if(chance < 25){
			Steal ();
		} else {
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Raid", "raid_fail");
//			this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 
//				"Events", "Raid", "raid_fail", new List<object>());
		}
	}

	private void Steal(){
//		this.logs.Add (new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "EventLogs", "raidlogs", "successful"));

		this.isSuccessful = true;
//		BASE_RESOURCE_TYPE basicResource = BASE_RESOURCE_TYPE.NONE;
//		BASE_RESOURCE_TYPE rareResource = BASE_RESOURCE_TYPE.NONE;

		int stolenGold = (int)(this.raidedCity.goldCount * 0.20f);
		this.startedBy.city.goldCount += stolenGold;
		this.raidedCity.goldCount -= stolenGold;

		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Raid", "raid_success");
		newLog.AddToFillers (null, stolenGold.ToString());
//		List<object> raidSuccessLogObjects = new List<object> {
//			stolenGold
//		};
//		this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 
//			"Events", "Raid", "raid_success", raidSuccessLogObjects);
//		
		Debug.Log (this.startedBy.name + " of " + this.startedBy.city.name + " has stolen " + stolenGold + " gold from " + this.raidedCity.name + " of " + this.targetKingdom.name + ".");
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
			
//		GeneralDiscovery ();

//		if(this.hasBeenDiscovered){
//			if(this.hasDeflected){
//				this.general.citizen.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.general.citizen.name + " raided " + this.raidedCity.name
//					+ " with a small group of raiders. The raid was successful. Their presence were discovered and their identities were revealed but " + this.general.citizen.name + " managed to deflect blame to " + kingdomToBlame.name + ".", HISTORY_IDENTIFIER.NONE));
//			}else{
//				this.general.citizen.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.general.citizen.name + " raided " + this.raidedCity.name
//					+ " with a small group of raiders. The raid was successful. Their presence were discovered and their identities were revealed.", HISTORY_IDENTIFIER.NONE));
//			}
//		}else{
//			this.general.citizen.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.general.citizen.name + " raided " + this.raidedCity.name
//				+ " with a small group of raiders. The raid was successful. Their presence were not discovered in time.", HISTORY_IDENTIFIER.NONE));
//		}

	}

	//Whether or not the raid party can deflect the blame to another kingdom upon discovery
	private void DeflectBlame(){
		if (this.startedBy.hasTrait(TRAIT.SCHEMING)) {
			int deflectChance = UnityEngine.Random.Range (0, 100);
			if (deflectChance < 35) {
				Kingdom kingdomToBlame = GetRandomKingdomToBlame ();
				if (kingdomToBlame != null) {
					this.hasDeflected = true;
					this.kingdomToBlame = kingdomToBlame;
//					relationship.AdjustLikeness (amountToAdjust, this);
//					relationship.relationshipHistory.Add (new History (
//						GameManager.Instance.month,
//						GameManager.Instance.days,
//						GameManager.Instance.year,
//						this.raidedCity.kingdom.king.name + " caught a raider, that was from " + kingdomToBlame.name,
//						HISTORY_IDENTIFIER.KING_RELATIONS,
//						false
//					));
				}
			}
		}
	}

	//Accident killing of random citizen
	private void AccidentKilling(){
		if(this.raidedCity == null){
			return;
		}
		Citizen deadCitizen = null;
		bool isGovernor = false;
		bool isKing = false;
		int deathChance = UnityEngine.Random.Range (0, 100);
		if(deathChance < 1){
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
				this.hasDeath = true;
				deadCitizen.Death (DEATH_REASONS.INTERNATIONAL_WAR);
			}
		}
		if (deadCitizen != null) {
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Raid", "raid_accident");
			newLog.AddToFillers (deadCitizen, deadCitizen.name);
//			List<object> accidentDeathLogObjects = new List<object> {
//				deadCitizen,
//			};
//			this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 
//				"Events", "Raid", "raid_accident", accidentDeathLogObjects);
			if(this.hasBeenDiscovered){
				int amountToAdjust = -15;
				if (isGovernor || isKing) {
					if (isGovernor) {
						amountToAdjust = -25;
					} else {
						amountToAdjust = -35;
					}
				}
				RelationshipKings relationship = this.GetRelationship ();
				if(relationship != null){
					relationship.AdjustLikeness(amountToAdjust, this);
				}

			}

		}

	}

	//Discovery of Raid Party which will cause relationship deterioration
	private void RaidPartyDiscovery(){
		if(this.raidedCity == null){
			return;
		}
		if(this.hasBeenDiscovered){
			return;
		}
		int chance = UnityEngine.Random.Range (0, 100);
		if(chance < 4){
			//DISCOVERY
			this.hasBeenDiscovered = true;
			if (this.hasDeflected) {
				if(this.kingdomToBlame == null || !this.kingdomToBlame.isAlive()){
					this.kingdomToBlame = GetRandomKingdomToBlame ();
					if(this.kingdomToBlame != null){
						Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Raid", "raid_discovery_deflect");
						newLog.AddToFillers (this.raidedCity, this.raidedCity.name);
						newLog.AddToFillers (this.kingdomToBlame, this.kingdomToBlame.name);
					}else{
						Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Raid", "raid_discovery");
						newLog.AddToFillers (this.raidedCity, this.raidedCity.name);
					}
				}
//				List<object> discoveredLogObjects = new List<object> {
//					this.raidedCity,
//					this.startedByCity
//				};
//				this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 
//					"Events", "Raid", "raid_discovery_deflect", discoveredLogObjects);
			} else {
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Raid", "raid_discovery");
				newLog.AddToFillers (this.raidedCity, this.raidedCity.name);
//				List<object> discoveredLogObjects = new List<object> {
//					this.raidedCity,
//				};
//				this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 
//					"Events", "Raid", "raid_discovery", discoveredLogObjects);
			}
			RelationshipKings relationship = this.GetRelationship ();
			if(relationship != null){
				relationship.AdjustLikeness(-10, this);
			}

//				if(deflectChance < 35){
//					Kingdom kingdomToBlame = GetRandomKingdomToBlame ();
//					if(kingdomToBlame != null){
//						this.logs.Add (new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "EventLogs", "raidlogs", "deflect"));
//						this.hasDeflected = true;
//						this.kingdomToBlame = kingdomToBlame;
//						RelationshipKings relationship = this.raidedCity.kingdom.king.SearchRelationshipByID(kingdomToBlame.king.id);
//						relationship.AdjustLikeness (amountToAdjust, this);
//						relationship.relationshipHistory.Add (new History (
//							GameManager.Instance.month,
//							GameManager.Instance.days,
//							GameManager.Instance.year,
//							this.raidedCity.kingdom.king.name +  " caught a raider, that was from " + kingdomToBlame.name,
//							HISTORY_IDENTIFIER.KING_RELATIONS,
//							false
//						));
//					}else{
//						this.logs.Add (new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "EventLogs", "raidlogs", "nodeflect"));
//						RelationshipKings relationship = this.raidedCity.kingdom.king.SearchRelationshipByID(this.sourceKingdom.king.id);
//						relationship.AdjustLikeness (amountToAdjust, this);
//						relationship.relationshipHistory.Add (new History (
//							GameManager.Instance.month,
//							GameManager.Instance.days,
//							GameManager.Instance.year,
//							this.raidedCity.kingdom.king.name +  " caught a raider, that was from " + this.sourceKingdom.name,
//							HISTORY_IDENTIFIER.KING_RELATIONS,
//							false
//						));
//					}
//				}else{
//					this.logs.Add (new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "EventLogs", "raidlogs", "nodeflect"));
//					RelationshipKings relationship = this.raidedCity.kingdom.king.SearchRelationshipByID(this.sourceKingdom.king.id);
//					relationship.AdjustLikeness (amountToAdjust, this);
//					relationship.relationshipHistory.Add (new History (
//						GameManager.Instance.month,
//						GameManager.Instance.days,
//						GameManager.Instance.year,
//						this.raidedCity.kingdom.king.name +  " caught a raider, that was from " + this.sourceKingdom.name,
//						HISTORY_IDENTIFIER.KING_RELATIONS,
//						false
//					));
//				}
//			}else{
//				RelationshipKings relationship = this.raidedCity.kingdom.king.SearchRelationshipByID(this.sourceKingdom.king.id);
//				relationship.AdjustLikeness (amountToAdjust, this);
//				relationship.relationshipHistory.Add (new History (
//					GameManager.Instance.month,
//					GameManager.Instance.days,
//					GameManager.Instance.year,
//					this.raidedCity.kingdom.king.name +  " caught a raider, that was from " + this.sourceKingdom.name,
//					HISTORY_IDENTIFIER.KING_RELATIONS,
//					false
//				));
//			}
		}
	}
	private Kingdom GetRandomKingdomToBlame(){
		if(this.otherKingdoms == null || this.otherKingdoms.Count <= 0){
			return null;
		}
		this.otherKingdoms.RemoveAll (x => !x.isAlive ());
		return this.otherKingdoms [UnityEngine.Random.Range (0, this.otherKingdoms.Count)];
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

	private RelationshipKings GetRelationship(){
		RelationshipKings relationship = null;
		if(this.targetKingdom == null || !this.targetKingdom.isAlive()){
			return relationship;
		}
		relationship = this.targetKingdom.king.SearchRelationshipByID (this.sourceKingdom.king.id);
		if(this.hasDeflected){
			if(this.kingdomToBlame != null){
				if(this.kingdomToBlame.isAlive()){
					relationship = this.targetKingdom.king.SearchRelationshipByID (this.kingdomToBlame.king.id);
				}else{
					relationship = null;
				}
			}
		}
		return relationship;
	}
}
