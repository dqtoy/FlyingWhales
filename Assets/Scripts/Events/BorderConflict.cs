using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BorderConflict : GameEvent {

	public Kingdom kingdom1;
	public Kingdom kingdom2;
	public List<Kingdom> otherKingdoms;

	public Envoy activeEnvoyResolve;
	public Envoy activeEnvoyProvoke;

	internal City targetCity;

	public bool isResolvedPeacefully;

    protected const int UNREST_ADJUSTMENT = 10;

	public BorderConflict(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom kingdom1, Kingdom kingdom2) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.BORDER_CONFLICT;
		this.name = "Border Conflict";
		this.description = "A border conflict has began between " + kingdom1.name + " and " + kingdom2.name + ".";
		this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
		this.remainingDays = this.durationInDays;
		this.kingdom1 = kingdom1;
		this.kingdom2 = kingdom2;
		this.targetCity = null;
//		this.otherKingdoms = GetOtherKingdoms ();
		this.otherKingdoms = null;
		this.activeEnvoyResolve = null;
		this.activeEnvoyProvoke = null;
		this._warTrigger = WAR_TRIGGER.BORDER_CONFLICT;

		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		Debug.LogError (this.description);

		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "BorderConflict", "event_title");
		newLogTitle.AddToFillers (null, kingdom1.name, LOG_IDENTIFIER.KINGDOM_1);
		newLogTitle.AddToFillers (null, kingdom2.name, LOG_IDENTIFIER.KINGDOM_2);

		string randomReason = LocalizationManager.Instance.GetRandomLocalizedKey ("Reasons", "BorderConflictReasons");
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Reasons", "BorderConflictReasons", randomReason);
		if(randomReason == "border_altercation"){
			newLog.AddToFillers (kingdom1, kingdom1.name, LOG_IDENTIFIER.KINGDOM_1);
			newLog.AddToFillers (kingdom2, kingdom2.name, LOG_IDENTIFIER.KINGDOM_2);
		}else if(randomReason == "insulted_governor"){
			newLog.AddToFillers (kingdom1, kingdom1.name, LOG_IDENTIFIER.KINGDOM_1);
			newLog.AddToFillers (kingdom2, kingdom2.name, LOG_IDENTIFIER.KINGDOM_2);
			Citizen randomGovernor1 = kingdom1.cities[UnityEngine.Random.Range(0,kingdom1.cities.Count)].governor;
			Citizen randomGovernor2 = kingdom2.cities[UnityEngine.Random.Range(0,kingdom2.cities.Count)].governor;
			newLog.AddToFillers (randomGovernor1, randomGovernor1.name, LOG_IDENTIFIER.GOVERNOR_1);
			newLog.AddToFillers (randomGovernor1.city, randomGovernor1.city.name, LOG_IDENTIFIER.CITY_1);
			newLog.AddToFillers (randomGovernor2, randomGovernor2.name, LOG_IDENTIFIER.GOVERNOR_2);
			newLog.AddToFillers (randomGovernor2.city, randomGovernor2.city.name, LOG_IDENTIFIER.CITY_2);
		}else if(randomReason == "cultural_misunderstanding"){
			newLog.AddToFillers (kingdom1, kingdom1.name, LOG_IDENTIFIER.KINGDOM_1);
			newLog.AddToFillers (kingdom2, kingdom2.name, LOG_IDENTIFIER.KINGDOM_2);
		}else if(randomReason == "pig"){
			newLog.AddToFillers (kingdom1, kingdom1.name, LOG_IDENTIFIER.KINGDOM_1);
			newLog.AddToFillers (kingdom2, kingdom2.name, LOG_IDENTIFIER.KINGDOM_2);
			City randomCity = kingdom2.cities[UnityEngine.Random.Range(0,kingdom2.cities.Count)];
			newLog.AddToFillers (randomCity, randomCity.name, LOG_IDENTIFIER.CITY_2);
		}


//		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "BorderConflict", "start");
//		newLog.AddToFillers (kingdom1, kingdom1.name, LOG_IDENTIFIER.KINGDOM_1);
//		newLog.AddToFillers (kingdom2, kingdom2.name, LOG_IDENTIFIER.KINGDOM_2);
//		newLog.AddToFillers (null, LocalizationManager.Instance.GetRandomLocalizedValue("Reasons", "BorderConflictReasons"), LOG_IDENTIFIER.TRIGGER_REASON);

		EventManager.Instance.AddEventToDictionary (this);
		this.EventIsCreated ();

	}

	#region Overrides
	internal override void PerformAction(){
		this.remainingDays -= 1;
		if (this.remainingDays <= 0) {
			this.remainingDays = 0;
			this.isResolvedPeacefully = false;
			DoneEvent ();
		}else{
			CheckIfAlreadyAtWar ();
			ResolvePeacefullyConflict ();
//			SpeedUpConflict ();
		}
	}
	internal override void DoneCitizenAction(Citizen citizen){
        base.DoneCitizenAction(citizen);
		if (citizen.assignedRole is Envoy) {
			if (this.activeEnvoyResolve != null) {
				if (citizen.id == this.activeEnvoyResolve.citizen.id) {
					int chance = UnityEngine.Random.Range (0, 100);
					int value = 20;
					if (chance < value) {
						Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "BorderConflict", "envoy_resolve_success");
						newLog.AddToFillers (this.activeEnvoyResolve.citizen, this.activeEnvoyResolve.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);

						this.isResolvedPeacefully = true;
						DoneEvent ();
						return;
					} else {
						Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "BorderConflict", "envoy_resolve_fail");
						newLog.AddToFillers (this.activeEnvoyResolve.citizen, this.activeEnvoyResolve.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
					}
				}
			}
			this.activeEnvoyResolve = null;
		}
	}
	/*internal override void DoneCitizenAction(Envoy envoy){
		if(!envoy.citizen.isDead){
			//Search for envoys task first on activeenvoys
			//Do something here add tension or reduce depending on the envoys task
			if(this.activeEnvoyResolve != null){
				if(envoy.citizen.id == this.activeEnvoyResolve.citizen.id){
					int chance = UnityEngine.Random.Range (0, 100);
					int value = 20;
					if(chance < value){
						Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "BorderConflict", "envoy_resolve_success");
						newLog.AddToFillers (this.activeEnvoyResolve.citizen, this.activeEnvoyResolve.citizen.name);

						this.isResolvedPeacefully = true;
						DoneEvent ();
					}else{
						Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "BorderConflict", "envoy_resolve_fail");
						newLog.AddToFillers (this.activeEnvoyResolve.citizen, this.activeEnvoyResolve.citizen.name);
					}
				}
			} else {
				if (this.activeEnvoyProvoke != null) {
					if (envoy.citizen.id == this.activeEnvoyProvoke.citizen.id) {
						this.remainingDays -= 3;
						Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "BorderConflict", "envoy_sabotage_success");
						newLog.AddToFillers (this.activeEnvoyProvoke.citizen, this.activeEnvoyProvoke.citizen.name);
					}
				}
			}

		}else{
			if (envoy.citizen.id == this.activeEnvoyResolve.citizen.id) {
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "BorderConflict", "envoy_resolve_fail_died");
				newLog.AddToFillers (this.activeEnvoyResolve.citizen, this.activeEnvoyResolve.citizen.name);
			}else if (envoy.citizen.id == this.activeEnvoyProvoke.citizen.id) {
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "BorderConflict", "envoy_sabotage_fail_died");
				newLog.AddToFillers (this.activeEnvoyProvoke.citizen, this.activeEnvoyProvoke.citizen.name);
			}
		}
		this.activeEnvoyResolve = null;
		this.activeEnvoyProvoke = null;
	}*/

	internal override void DeathByOtherReasons(){
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "BorderConflict", "envoy_resolve_fail_died");
		newLog.AddToFillers (this.activeEnvoyResolve.citizen, this.activeEnvoyResolve.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
	}
	internal override void DeathByAgent(Citizen citizen, Citizen deadCitizen){
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "BorderConflict", "envoy_resolve_fail_died");
		newLog.AddToFillers (this.activeEnvoyResolve.citizen, this.activeEnvoyResolve.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		this.activeEnvoyResolve.citizen.Death (DEATH_REASONS.BATTLE);
	}
	internal override void DoneEvent(){
		base.DoneEvent();
		//		if(this.activeEnvoyResolve != null){
		//			this.activeEnvoyResolve.DestroyGO ();
		//		}

		//		if(this.activeEnvoyProvoke != null){
		//			this.activeEnvoyProvoke.eventDuration = 0;
		//			this.activeEnvoyProvoke.currentEvent = null;
		//			this.activeEnvoyProvoke.inAction = false;
		//		}


		EventManager.Instance.onWeekEnd.RemoveListener (this.PerformAction);

		RelationshipKings relationship1 = null;
		if(this.kingdom1.isAlive()){
			relationship1 = this.kingdom1.king.SearchRelationshipByID (this.kingdom2.king.id);
		}
		RelationshipKings relationship2 = null;
		if (this.kingdom2.isAlive ()) {
			relationship2 = this.kingdom2.king.SearchRelationshipByID (this.kingdom1.king.id);
		}

		if(this.isResolvedPeacefully){

			//			Debug.Log("BORDER CONFLICT BETWEEN " + this.kingdom1.name + " AND " + this.kingdom2.name + " ENDED PEACEFULLY!");

			this.resolution = "Ended on " + ((MONTH)this.endMonth).ToString() + " " + this.endDay + ", " + this.endYear + ". Conflict was resolved peacefully.";

		}else{
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "BorderConflict", "event_end");
			newLog.AddToFillers (this.kingdom1, this.kingdom1.name, LOG_IDENTIFIER.KINGDOM_1);
			newLog.AddToFillers (this.kingdom2, this.kingdom2.name, LOG_IDENTIFIER.KINGDOM_2);

			//			Debug.Log("BORDER CONFLICT BETWEEN " + this.kingdom1.name + " AND " + this.kingdom2.name + " ENDED HORRIBLY! RELATIONSHIP DETERIORATED!");

			this.resolution = "Ended on " + ((MONTH)this.endMonth).ToString() + " " + this.endDay + ", " + this.endYear + ". Conflict caused deterioration in relationship.";

			if(relationship1 != null){
				relationship1.AdjustLikeness (-15, this);
				relationship1.sourceKing.WarTrigger (relationship1, this, this.kingdom1.kingdomTypeData, this._warTrigger);
			}
			if (relationship2 != null) {
				relationship2.AdjustLikeness (-15, this);
				relationship2.sourceKing.WarTrigger (relationship2, this, this.kingdom2.kingdomTypeData, this._warTrigger);
			}

			this.kingdom1.AdjustUnrest(UNREST_ADJUSTMENT);
			this.kingdom2.AdjustUnrest(UNREST_ADJUSTMENT);
			this.kingdom1.HasConflicted ();
			this.kingdom2.HasConflicted ();
		}
	}
	internal override void CancelEvent (){
		base.CancelEvent ();
        this.DoneEvent();
	}
	#endregion

	private List<Kingdom> GetOtherKingdoms(){
		List<Kingdom> kingdoms = new List<Kingdom> ();
		for(int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++){
			if(KingdomManager.Instance.allKingdoms[i].id != this.kingdom1.id && KingdomManager.Instance.allKingdoms[i].id != this.kingdom2.id && KingdomManager.Instance.allKingdoms[i].isAlive()
				&& (KingdomManager.Instance.allKingdoms[i].discoveredKingdoms.Contains(this.kingdom1) || KingdomManager.Instance.allKingdoms[i].discoveredKingdoms.Contains(this.kingdom2))){
				kingdoms.Add (KingdomManager.Instance.allKingdoms [i]);
			}
		}
		return kingdoms;
	}

	private void ResolvePeacefullyConflict(){
		//Send Envoys
		this.targetCity = this.kingdom1.cities[UnityEngine.Random.Range(0, this.kingdom1.cities.Count)];
		if(this.targetCity.id != this.kingdom1.capitalCity.id){
			if(this.activeEnvoyResolve == null && this.activeEnvoyProvoke == null){
				int chance = UnityEngine.Random.Range (0, 100);
				int value = 1;
				RelationshipKings relationship = this.kingdom1.king.SearchRelationshipByID (this.kingdom2.king.id);
				if(relationship != null){
					if(relationship.lordRelationship == RELATIONSHIP_STATUS.WARM){
						value += 2;
					}else if(relationship.lordRelationship == RELATIONSHIP_STATUS.FRIEND){
						value += 3;
					}else if(relationship.lordRelationship == RELATIONSHIP_STATUS.ALLY){
						value += 5;
					}
				}
				if(chance < value){
					SendEnvoy (this.kingdom1);
				}
			}
		}

		if (this.targetCity.id != this.kingdom2.capitalCity.id) {
			if (this.activeEnvoyResolve == null && this.activeEnvoyProvoke == null) {
				int chance = UnityEngine.Random.Range (0, 100);
				int value = 1;
				RelationshipKings relationship = this.kingdom2.king.SearchRelationshipByID (this.kingdom1.king.id);
				if (relationship != null) {
					if (relationship.lordRelationship == RELATIONSHIP_STATUS.WARM) {
						value += 2;
					} else if (relationship.lordRelationship == RELATIONSHIP_STATUS.FRIEND) {
						value += 3;
					} else if (relationship.lordRelationship == RELATIONSHIP_STATUS.ALLY) {
						value += 5;
					}
				}
				if (chance < value) {
					SendEnvoy (this.kingdom2);
				}
			}
		}
	}
	/*private void SpeedUpConflict(){
		int chance = UnityEngine.Random.Range (0, 100);
		if(chance < 1){
			for(int i = 0; i < this.otherKingdoms.Count; i++){
				if (this.otherKingdoms [i].isAlive ()) {
					Citizen dislikedKing = null;
					if (CheckForRelationship (this.otherKingdoms [i], ref dislikedKing)) {
						if (this.activeEnvoyResolve == null && this.activeEnvoyProvoke == null) {
							SendEnvoy (this.otherKingdoms [i], dislikedKing, true);	
						}
					}
				}
			}
		}

	}*/
	private bool CheckForRelationship(Kingdom otherKingdom, ref Citizen dislikedKing){
		RelationshipKings relationship1 = otherKingdom.king.SearchRelationshipByID (this.kingdom1.king.id);
		RelationshipKings relationship2 = otherKingdom.king.SearchRelationshipByID (this.kingdom2.king.id);

		List<RELATIONSHIP_STATUS> statuses = new List<RELATIONSHIP_STATUS> ();
		statuses.Add (relationship1.lordRelationship);
		statuses.Add (relationship2.lordRelationship);

		if(relationship1.lordRelationship == RELATIONSHIP_STATUS.ENEMY || relationship1.lordRelationship == RELATIONSHIP_STATUS.RIVAL){
			dislikedKing = relationship1.king;
		}else if(relationship2.lordRelationship == RELATIONSHIP_STATUS.ENEMY || relationship2.lordRelationship == RELATIONSHIP_STATUS.RIVAL){
			dislikedKing = relationship2.king;
		}

		if(statuses.Contains(RELATIONSHIP_STATUS.ENEMY) || statuses.Contains(RELATIONSHIP_STATUS.RIVAL)){
			if(!statuses.Contains(RELATIONSHIP_STATUS.FRIEND) && !statuses.Contains(RELATIONSHIP_STATUS.ALLY)){
				return true;
			}
		}

		return false;
	}
	private void SendEnvoy(Kingdom sender){
		if (this.targetCity == null) {
			return;
		}
		Citizen chosenCitizen = sender.capitalCity.CreateAgent (ROLE.ENVOY, this.eventType, this.targetCity.hexTile, this.remainingDays);
		if(chosenCitizen == null){
			return;
		}
		chosenCitizen.assignedRole.Initialize (this);
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "BorderConflict", "envoy_resolve");
		newLog.AddToFillers (sender.king, sender.king.name, LOG_IDENTIFIER.KING_1);
		newLog.AddToFillers (chosenCitizen, chosenCitizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
//		if(isFromOthers){
//			this.activeEnvoyProvoke = chosenEnvoy;
//			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "BorderConflict", "envoy_sabotage");
//			newLog.AddToFillers (sender.king, sender.king.name);
//			newLog.AddToFillers (dislikedKing, dislikedKing.name);
//			newLog.AddToFillers (chosenEnvoy.citizen, chosenEnvoy.citizen.name);
//		}else{
//			this.activeEnvoyResolve = chosenEnvoy;
//			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "BorderConflict", "envoy_resolve");
//			newLog.AddToFillers (sender.king, sender.king.name);
//			newLog.AddToFillers (chosenEnvoy.citizen, chosenEnvoy.citizen.name);
//		}
	}
	/*private void SendEnvoy(Kingdom sender, Citizen dislikedKing = null, bool isFromOthers = false){
		Citizen chosenCitizen = GetEnvoy (sender);
		if(chosenCitizen == null){
			return;
		}
		Envoy chosenEnvoy = null;
		if (chosenCitizen.assignedRole is Envoy) {
			chosenEnvoy = (Envoy)chosenCitizen.assignedRole;
		}
		if (chosenEnvoy == null) {
			return;
		}

		chosenEnvoy.eventDuration = 5;
		chosenEnvoy.currentEvent = this;
		chosenEnvoy.inAction = true;
		EventManager.Instance.onWeekEnd.AddListener (chosenEnvoy.WeeklyAction);

		if(isFromOthers){
			this.activeEnvoyProvoke = chosenEnvoy;
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "BorderConflict", "envoy_sabotage");
			newLog.AddToFillers (sender.king, sender.king.name);
			newLog.AddToFillers (dislikedKing, dislikedKing.name);
			newLog.AddToFillers (chosenEnvoy.citizen, chosenEnvoy.citizen.name);
		}else{
			this.activeEnvoyResolve = chosenEnvoy;
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "BorderConflict", "envoy_resolve");
			newLog.AddToFillers (sender.king, sender.king.name);
			newLog.AddToFillers (chosenEnvoy.citizen, chosenEnvoy.citizen.name);
		}
	}
	private Citizen GetEnvoy(Kingdom kingdom){
		List<Citizen> unwantedGovernors = GetUnwantedGovernors (kingdom.king);
		List<Citizen> envoys = new List<Citizen> ();
		for(int i = 0; i < kingdom.cities.Count; i++){
			if(!IsItThisGovernor(kingdom.cities[i].governor, unwantedGovernors)){
				for(int j = 0; j < kingdom.cities[i].citizens.Count; j++){
					if (!kingdom.cities [i].citizens [j].isDead) {
						if (kingdom.cities [i].citizens [j].assignedRole != null && kingdom.cities [i].citizens [j].role == ROLE.ENVOY) {
							if (kingdom.cities [i].citizens [j].assignedRole is Envoy) {
								if (!((Envoy)kingdom.cities [i].citizens [j].assignedRole).inAction) {
									envoys.Add (kingdom.cities [i].citizens [j]);
								}
							}
						}
					}
				}
			}
		}

		if(envoys.Count > 0){
			return envoys [UnityEngine.Random.Range (0, envoys.Count)];
		}else{
//			Debug.Log (kingdom.king.name + " CAN'T SEND ENVOY BECAUSE THERE IS NONE!");
			return null;
		}
	}*/
	private void CheckIfAlreadyAtWar(){
		RelationshipKingdom relationship = this.kingdom1.GetRelationshipWithOtherKingdom (this.kingdom2);
		if(relationship != null){
			if(relationship.isAtWar){
				this.isResolvedPeacefully = false;
				DoneEvent ();
			}
		}
	}
}
