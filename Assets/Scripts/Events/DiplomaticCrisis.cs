using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DiplomaticCrisis : GameEvent {

	public Kingdom kingdom1;
	public Kingdom kingdom2;
	public List<Kingdom> otherKingdoms;

	public Envoy activeEnvoyResolve;
	public Envoy activeEnvoyProvoke;

	public bool isResolvedPeacefully;
	public DiplomaticCrisis(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom kingdom1, Kingdom kingdom2) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.DIPLOMATIC_CRISIS;
		if(startedBy != null){
			this.description = startedBy.name + " has created a diplomatic crisis between " + kingdom1.name + " and " + kingdom2.name + ".";
			this.startedBy.city.cityHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 
				startedBy.name + " has created a diplomatic crisis between " + kingdom1.name + " and " + kingdom2.name + "." , HISTORY_IDENTIFIER.NONE));
		}else{
			this.description = "A diplomatic crisis has began between " + kingdom1.name + " and " + kingdom2.name + ".";
		}
		this.durationInDays = 30;
		this.remainingDays = this.durationInDays;
		this.kingdom1 = kingdom1;
		this.kingdom2 = kingdom2;
		this.otherKingdoms = GetOtherKingdoms ();
		this.activeEnvoyResolve = null;
		this.activeEnvoyProvoke = null;
		this.kingdom1.cities[0].hexTile.AddEventOnTile(this);
		this.kingdom2.cities[0].hexTile.AddEventOnTile(this);
		this._warTrigger = WAR_TRIGGER.BORDER_CONFLICT;

		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		Debug.LogError (this.description);

		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DiplomaticCrisis", "event_title");
		newLogTitle.AddToFillers (kingdom1.king, kingdom1.king.name);
		newLogTitle.AddToFillers (kingdom2.king, kingdom2.king.name);
		newLogTitle.AddToFillers (null, Utilities.crisis[0]);

		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DiplomaticCrisis", "start");
		newLog.AddToFillers (kingdom1.king, kingdom1.king.name);
		newLog.AddToFillers (kingdom2.king, kingdom2.king.name);
		newLog.AddToFillers (null, Utilities.crisis[0]);

	}

	internal override void PerformAction(){
		this.remainingDays -= 1;
		if (this.remainingDays <= 0) {
			this.remainingDays = 0;
			this.isResolvedPeacefully = false;
			DoneEvent ();
		}else{
			CheckIfAlreadyAtWar ();
			ResolvePeacefullyConflict ();
			SpeedUpConflict ();
		}
	}
	internal override void DoneCitizenAction(Envoy envoy){
		if(!envoy.citizen.isDead){
			//Search for envoys task first on activeenvoys
			//Do something here add tension or reduce depending on the envoys task
			if (this.activeEnvoyResolve != null) {
				if (envoy.citizen.id == this.activeEnvoyResolve.citizen.id) {
					int chance = UnityEngine.Random.Range (0, 100);
					int value = 20;
//					if (this.activeEnvoyResolve.citizen.skillTraits.Contains (SKILL_TRAIT.PERSUASIVE)) {
//						value += 10;
//					}
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
	}
	private List<Kingdom> GetOtherKingdoms(){
		List<Kingdom> kingdoms = new List<Kingdom> ();
		for(int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++){
			if(KingdomManager.Instance.allKingdoms[i].id != this.kingdom1.id && KingdomManager.Instance.allKingdoms[i].id != this.kingdom2.id && KingdomManager.Instance.allKingdoms[i].isAlive()){
				kingdoms.Add (KingdomManager.Instance.allKingdoms [i]);
			}
		}
		return kingdoms;
	}

	private void ResolvePeacefullyConflict(){
		//Send Envoys
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
	private void SpeedUpConflict(){
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

	}
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
	private void SendEnvoy(Kingdom sender, Citizen dislikedKing = null, bool isFromOthers = false){
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
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DiplomaticCrisis", "envoy_sabotage");
			newLog.AddToFillers (sender.king, sender.king.name);
			newLog.AddToFillers (dislikedKing, dislikedKing.name);
			newLog.AddToFillers (chosenEnvoy.citizen, chosenEnvoy.citizen.name);
		}else{
			this.activeEnvoyResolve = chosenEnvoy;
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DiplomaticCrisis", "envoy_resolve");
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
			Debug.Log (kingdom.king.name + " CAN'T SEND ENVOY BECAUSE THERE IS NONE!");
			return null;
		}
	}
	private void CheckIfAlreadyAtWar(){
		if(this.kingdom1.GetRelationshipWithOtherKingdom(this.kingdom2).isAtWar){
			this.isResolvedPeacefully = false;
			DoneEvent ();
		}
	}

	internal override void DoneEvent(){
		if(this.activeEnvoyResolve != null){
			this.activeEnvoyResolve.eventDuration = 0;
			this.activeEnvoyResolve.currentEvent = null;
			this.activeEnvoyResolve.inAction = false;
		}

		if(this.activeEnvoyProvoke != null){
			this.activeEnvoyProvoke.eventDuration = 0;
			this.activeEnvoyProvoke.currentEvent = null;
			this.activeEnvoyProvoke.inAction = false;
		}


		EventManager.Instance.onWeekEnd.RemoveListener (this.PerformAction);
		this.isActive = false;
		EventManager.Instance.onGameEventEnded.Invoke(this);
		this.endDay = GameManager.Instance.days;
		this.endMonth = GameManager.Instance.month;
		this.endYear = GameManager.Instance.year;

		RelationshipKings relationship1 = null;
		if(this.kingdom1.isAlive()){
			relationship1 = this.kingdom1.king.SearchRelationshipByID (this.kingdom2.king.id);
		}
//		RelationshipKings relationship2 = this.kingdom2.king.SearchRelationshipByID (this.kingdom1.king.id);

		if(this.isResolvedPeacefully){
			Debug.Log("DIPLOMATIC CRISIS BETWEEN " + this.kingdom1.name + " AND " + this.kingdom2.name + " ENDED PEACEFULLY!");

			this.resolution = "Ended on " + ((MONTH)this.endMonth).ToString() + " " + this.endDay + ", " + this.endYear + ". Diplomatic Crisis was resolved peacefully.";

//			relationship1.relationshipHistory.Add (new History (
//				GameManager.Instance.month,
//				GameManager.Instance.days,
//				GameManager.Instance.year,
//				this.kingdom1.king.name +  " did not hate " + this.kingdom2.king.name + ".",
//				HISTORY_IDENTIFIER.KING_RELATIONS,
//				false
//			));
//			relationship2.relationshipHistory.Add (new History (
//				GameManager.Instance.month,
//				GameManager.Instance.days,
//				GameManager.Instance.year,
//				this.kingdom1.king.name +  " did not hate " + this.kingdom2.king.name + ".",
//				HISTORY_IDENTIFIER.KING_RELATIONS,
//				false
//			));
		}else{
			Debug.Log("DIPLOMATIC CRISIS BETWEEN " + this.kingdom1.name + " AND " + this.kingdom2.name + " ENDED HORRIBLY! RELATIONSHIP DETERIORATED!");

			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "DiplomaticCrisis", "event_end");
			newLog.AddToFillers (this.kingdom1.king, this.kingdom1.king.name);
			newLog.AddToFillers (this.kingdom2.king, this.kingdom2.king.name);

			this.resolution = "Ended on " + ((MONTH)this.endMonth).ToString() + " " + this.endDay + ", " + this.endYear + ". Diplomatic Crisis caused deterioration in relationship.";

			if(relationship1 != null){
				relationship1.AdjustLikeness (-25, this);
				relationship1.sourceKing.WarTrigger (relationship1, this, this.kingdom1.kingdomTypeData);
			}

//			relationship1.relationshipHistory.Add (new History (
//				GameManager.Instance.month,
//				GameManager.Instance.days,
//				GameManager.Instance.year,
//				this.kingdom1.king.name +  " hated " + this.kingdom2.king.name + ".",
//				HISTORY_IDENTIFIER.KING_RELATIONS,
//				false
//			));
//			relationship2.relationshipHistory.Add (new History (
//				GameManager.Instance.month,
//				GameManager.Instance.days,
//				GameManager.Instance.year,
//				this.kingdom1.king.name +  " hated " + this.kingdom2.king.name + ".",
//				HISTORY_IDENTIFIER.KING_RELATIONS,
//				false
//			));
		}
		//		EventManager.Instance.allEvents [EVENT_TYPES.BORDER_CONFLICT].Remove (this);

		//Remove UI Icon
	}
}
