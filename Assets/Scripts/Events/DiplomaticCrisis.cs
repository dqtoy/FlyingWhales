using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DiplomaticCrisis : GameEvent {

	public Kingdom kingdom1;
	public Kingdom kingdom2;
	public List<Kingdom> otherKingdoms;

	public Envoy activeEnvoyResolve;
	public Envoy activeEnvoyProvoke;

	internal City targetCity;
	internal string crisis;

	public bool isResolvedPeacefully;
	public DiplomaticCrisis(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom kingdom1, Kingdom kingdom2) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.DIPLOMATIC_CRISIS;
		this.name = "Diplomatic Crisis";
		this.description = "A diplomatic crisis has began between " + kingdom1.name + " and " + kingdom2.name + ".";
		this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
		this.remainingDays = this.durationInDays;
		this.kingdom1 = kingdom1;
		this.kingdom2 = kingdom2;
		this.targetCity = null;
		this.otherKingdoms = null;
		this.activeEnvoyResolve = null;
		this.activeEnvoyProvoke = null;
		this._warTrigger = WAR_TRIGGER.DIPLOMATIC_CRISIS;

		Messenger.AddListener("OnDayEnd", this.PerformAction);
		Debug.LogError (this.description);

		this.crisis = Utilities.crisis [UnityEngine.Random.Range (0, Utilities.crisis.Length)];

		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DiplomaticCrisis", "event_title");
		newLogTitle.AddToFillers (null, kingdom1.name, LOG_IDENTIFIER.KINGDOM_1);
		newLogTitle.AddToFillers (null, kingdom2.name, LOG_IDENTIFIER.KINGDOM_2);
		newLogTitle.AddToFillers (null, crisis, LOG_IDENTIFIER.RANDOM_GENERATED_EVENT_NAME);

		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DiplomaticCrisis", "start");
		newLog.AddToFillers (kingdom1.king, kingdom1.king.name, LOG_IDENTIFIER.KING_1);
		newLog.AddToFillers (kingdom1, kingdom1.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (kingdom2.king, kingdom2.king.name, LOG_IDENTIFIER.KING_2);
		newLog.AddToFillers (kingdom2, kingdom2.name, LOG_IDENTIFIER.KINGDOM_2);
		newLog.AddToFillers (null, crisis, LOG_IDENTIFIER.RANDOM_GENERATED_EVENT_NAME);

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
						Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DiplomaticCrisis", "envoy_resolve_success");
						newLog.AddToFillers (this.activeEnvoyResolve.citizen, this.activeEnvoyResolve.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);

						this.isResolvedPeacefully = true;
						DoneEvent ();
						return;
					} else {
						Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DiplomaticCrisis", "envoy_resolve_fail");
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
			if (this.activeEnvoyResolve != null) {
				if (envoy.citizen.id == this.activeEnvoyResolve.citizen.id) {
					int chance = UnityEngine.Random.Range (0, 100);
					int value = 20;
					if (chance < value) {
						Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DiplomaticCrisis", "envoy_resolve_success");
						newLog.AddToFillers (this.activeEnvoyResolve.citizen, this.activeEnvoyResolve.citizen.name);

						this.isResolvedPeacefully = true;
						DoneEvent ();
					}else{
						Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DiplomaticCrisis", "envoy_resolve_fail");
						newLog.AddToFillers (this.activeEnvoyResolve.citizen, this.activeEnvoyResolve.citizen.name);
					}
				}
			}else{
				if (this.activeEnvoyProvoke != null) {
					if (envoy.citizen.id == this.activeEnvoyProvoke.citizen.id) {
						this.remainingDays -= 3;
						Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DiplomaticCrisis", "envoy_sabotage_success");
						newLog.AddToFillers (this.activeEnvoyProvoke.citizen, this.activeEnvoyProvoke.citizen.name);
					}
				}
			} 
		}else{
			if (envoy.citizen.id == this.activeEnvoyResolve.citizen.id) {
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DiplomaticCrisis", "envoy_resolve_fail_died");
				newLog.AddToFillers (this.activeEnvoyResolve.citizen, this.activeEnvoyResolve.citizen.name);
			}else if (envoy.citizen.id == this.activeEnvoyProvoke.citizen.id) {
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DiplomaticCrisis", "envoy_sabotage_fail_died");
				newLog.AddToFillers (this.activeEnvoyProvoke.citizen, this.activeEnvoyProvoke.citizen.name);
			}
		}
		this.activeEnvoyResolve = null;
		this.activeEnvoyProvoke = null;
	}*/

	internal override void DeathByOtherReasons(){
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DiplomaticCrisis", "envoy_resolve_fail_died");
		newLog.AddToFillers (this.activeEnvoyResolve.citizen, this.activeEnvoyResolve.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
	}
	internal override void DeathByAgent(Citizen citizen, Citizen deadCitizen){
		base.DeathByAgent(citizen, deadCitizen);
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DiplomaticCrisis", "envoy_resolve_fail_died");
		newLog.AddToFillers (this.activeEnvoyResolve.citizen, this.activeEnvoyResolve.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
	}
	internal override void DoneEvent(){
		base.DoneEvent();

		Messenger.RemoveListener("OnDayEnd", this.PerformAction);

		RelationshipKings relationship1 = null;
		if(this.kingdom1.isAlive()){
			relationship1 = this.kingdom1.king.SearchRelationshipByID (this.kingdom2.king.id);
		}
		if(this.isResolvedPeacefully){
			//			Debug.Log("DIPLOMATIC CRISIS BETWEEN " + this.kingdom1.name + " AND " + this.kingdom2.name + " ENDED PEACEFULLY!");
			this.resolution = "Ended on " + ((MONTH)this.endMonth).ToString() + " " + this.endDay + ", " + this.endYear + ". Diplomatic Crisis was resolved peacefully.";
		}else{
			//			Debug.Log("DIPLOMATIC CRISIS BETWEEN " + this.kingdom1.name + " AND " + this.kingdom2.name + " ENDED HORRIBLY! RELATIONSHIP DETERIORATED!");

			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DiplomaticCrisis", "event_end");
			newLog.AddToFillers (this.kingdom1.king, this.kingdom1.king.name, LOG_IDENTIFIER.KING_1);
			newLog.AddToFillers (this.kingdom2.king, this.kingdom2.king.name, LOG_IDENTIFIER.KING_2);

			this.resolution = "Ended on " + ((MONTH)this.endMonth).ToString() + " " + this.endDay + ", " + this.endYear + ". Diplomatic Crisis caused deterioration in relationship.";

			if(relationship1 != null){
				relationship1.AdjustLikeness (-25, this);
				relationship1.sourceKing.WarTrigger (relationship1, this, this.kingdom1.kingdomTypeData, this._warTrigger);
			}
		}
	}
	internal override void CancelEvent (){
		base.CancelEvent ();
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
		if (this.activeEnvoyResolve == null && this.activeEnvoyProvoke == null) {
			int chance = UnityEngine.Random.Range (0, 100);
			int value = 1;
			RelationshipKings relationship = this.kingdom2.king.SearchRelationshipByID (this.kingdom1.king.id);
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
				SendEnvoy (this.kingdom2);
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
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DiplomaticCrisis", "envoy_resolve");
		newLog.AddToFillers (sender.king, sender.king.name, LOG_IDENTIFIER.KING_1);
		newLog.AddToFillers (chosenCitizen, chosenCitizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		/*if(isFromOthers){
			this.activeEnvoyProvoke = chosenEnvoy;
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DiplomaticCrisis", "envoy_sabotage");
			newLog.AddToFillers (sender.king, sender.king.name);
			newLog.AddToFillers (dislikedKing, dislikedKing.name);
			newLog.AddToFillers (chosenEnvoy.citizen, chosenEnvoy.citizen.name);
		}else{
			this.activeEnvoyResolve = chosenEnvoy;
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DiplomaticCrisis", "envoy_resolve");
			newLog.AddToFillers (sender.king, sender.king.name);
			newLog.AddToFillers (chosenEnvoy.citizen, chosenEnvoy.citizen.name);
		}*/
	}
	/*private Citizen GetEnvoy(Kingdom kingdom){
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
		}else{
			this.isResolvedPeacefully = true;
			DoneEvent ();
		}
	}
}
