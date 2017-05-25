using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StateVisit : GameEvent {
	public Kingdom inviterKingdom;
	public Kingdom invitedKingdom;
	public Citizen visitor;
	public List<Kingdom> otherKingdoms;
//	public List<Envoy> saboteurEnvoys;
	public Envoy saboteurEnvoy;

	public int successMeter;
	protected GameEvent gameEventTrigger;


	private bool isDoneBySabotage;
	private bool visitorHasDied;
	private bool isSuccessful;
	public StateVisit(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom invitedKingdom, Citizen visitor) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.STATE_VISIT;
		this.description = startedBy.name + " invited " + visitor.name + " of " + invitedKingdom.name + " to visit his/her kingdom.";
		this.durationInDays = 30;
		this.remainingDays = this.durationInDays;
		this.inviterKingdom = startedBy.city.kingdom;
		this.invitedKingdom = invitedKingdom;
		this.visitor = visitor;
		this.otherKingdoms = GetOtherKingdoms ();
//		this.saboteurEnvoys = new List<Envoy> ();
		this.saboteurEnvoy = null;
		this.successMeter = 50;
		this.invitedKingdom.cities[0].hexTile.AddEventOnTile(this);
		this.gameEventTrigger = gameEventTrigger;
		this.isDoneBySabotage = false;
		this.visitorHasDied = false;
		this.isSuccessful = false;
		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		Debug.LogError("STATE VISIT");

		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "StateVisit", "event_title");
		newLogTitle.AddToFillers (visitor, visitor.name);
		newLogTitle.AddToFillers (this.inviterKingdom, this.inviterKingdom.name);

		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "StateVisit", "start");
		newLog.AddToFillers (visitor, visitor.name);
		newLog.AddToFillers (this.inviterKingdom, this.inviterKingdom.name);
		newLog.AddToFillers (this.invitedKingdom.king, this.invitedKingdom.king.name);
	}

	internal override void PerformAction(){
		CheckVisitor ();
		if(!this.visitor.isDead){
			this.remainingDays -= 1;
			if(this.remainingDays <= 0){
				this.remainingDays = 0;
				this.isSuccessful = true;
				DoneEvent ();
			}else{
				TriggerAssassinationEvent ();
				TriggerSabotage ();
			}
		}
	}

	internal override void DoneEvent(){
		RelationshipKings relationship = null;
		if(this.invitedKingdom.isAlive()){
			relationship = this.invitedKingdom.king.SearchRelationshipByID (this.inviterKingdom.king.id);
		}
		if(this.isSuccessful){
			if(relationship != null){
				relationship.AdjustLikeness (20, this);
			}
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "StateVisit", "event_end");
			newLog.AddToFillers (this.invitedKingdom.king, this.invitedKingdom.king.name);
			newLog.AddToFillers (this.inviterKingdom.king, this.inviterKingdom.king.name);
			newLog.AddToFillers (this.visitor, this.visitor.name);
		}else{
			if(this.isDoneBySabotage){
				if (relationship != null) {
					relationship.AdjustLikeness (-10, this);
				}
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "StateVisit", "sabotage_success");
				newLog.AddToFillers (this.saboteurEnvoy.citizen, this.saboteurEnvoy.citizen.name);
				newLog.AddToFillers (this.invitedKingdom.king, this.invitedKingdom.king.name);
				newLog.AddToFillers (this.inviterKingdom.king, this.inviterKingdom.king.name);
			}else{
				if(this.visitorHasDied){
					Debug.Log ("VISITOR DIED!");
					if(relationship.like <= 0){
						if (relationship != null) {
							relationship.AdjustLikeness (-35, this);
						}
//						relationship.relationshipHistory.Add (new History (
//							GameManager.Instance.month,
//							GameManager.Instance.days,
//							GameManager.Instance.year,
//							"Visitor from " + this.invitedKingdom.name + " died while visiting " + this.inviterKingdom.name,
//							HISTORY_IDENTIFIER.KING_RELATIONS,
//							false
//						));
					}else{
						if (relationship != null) {
							relationship.like = -35;
							relationship.UpdateKingRelationshipStatus ();
						}
					}

					Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "StateVisit", "visitor_died");
					newLog.AddToFillers (this.visitor, this.visitor.name);
					newLog.AddToFillers (null, this.visitor.deathReasonText);
					newLog.AddToFillers (this.invitedKingdom.king, this.invitedKingdom.king.name);
					newLog.AddToFillers (this.inviterKingdom.king, this.inviterKingdom.king.name);
				}
			}

		}

//		for(int i = 0; i < this.saboteurEnvoys.Count; i++){
//			this.saboteurEnvoys[i].inAction = false;
//		}
		if(this.saboteurEnvoy != null){
			this.saboteurEnvoy.inAction = false;
		}
		EventManager.Instance.onWeekEnd.RemoveListener (this.PerformAction);
		this.isActive = false;
		this.endMonth = GameManager.Instance.month;
		this.endDay = GameManager.Instance.days;
		this.endYear = GameManager.Instance.year;
		EventManager.Instance.onGameEventEnded.Invoke(this);

//		EventManager.Instance.allEvents [EVENT_TYPES.STATE_VISIT].Remove (this);
	}
	private void AdjustSuccessMeter(int amount){
		this.successMeter += amount;
		if (this.successMeter >= 100) {
			this.successMeter = 100;
		}else if (this.successMeter <= -100) {
			this.successMeter = -100;
		}
	}

	private void CheckEndSuccessMeter(){
		int invitedPoints = (int)((float)this.successMeter / 5f);
		int inviterPoints = (int)(invitedPoints / 2);

		RelationshipKings inviterRelationship = inviterKingdom.king.SearchRelationshipByID (invitedKingdom.king.id);
		RelationshipKings invitedRelationship = invitedKingdom.king.SearchRelationshipByID (inviterKingdom.king.id);

		inviterRelationship.AdjustLikeness (inviterPoints, this);
		invitedRelationship.AdjustLikeness (invitedPoints, this);

		if (inviterPoints < 0) {
			inviterRelationship.relationshipHistory.Add (new History (
				GameManager.Instance.month,
				GameManager.Instance.days,
				GameManager.Instance.year,
				"State Visit fail",
				HISTORY_IDENTIFIER.KING_RELATIONS,
				false
			));
		} else {
			inviterRelationship.relationshipHistory.Add (new History (
				GameManager.Instance.month,
				GameManager.Instance.days,
				GameManager.Instance.year,
				"State Visit success",
				HISTORY_IDENTIFIER.KING_RELATIONS,
				true
			));
		}


		if (invitedPoints < 0) {
			invitedRelationship.relationshipHistory.Add (new History (
				GameManager.Instance.month,
				GameManager.Instance.days,
				GameManager.Instance.year,
				"State Visit fail",
				HISTORY_IDENTIFIER.KING_RELATIONS,
				false
			));
		} else {
			invitedRelationship.relationshipHistory.Add (new History (
				GameManager.Instance.month,
				GameManager.Instance.days,
				GameManager.Instance.year,
				"State Visit success",
				HISTORY_IDENTIFIER.KING_RELATIONS,
				true
			));
		}

		string result = "successful";
		if(this.successMeter <= 0){
			result = "unsuccessful";
		}

		if (this.visitor.isDead) {
			//if(svReason == STATEVISIT_TRIGGER_REASONS.DISCOVERING_A){
			if (gameEventTrigger is Assassination) {
				Assassination gameEventAss = (Assassination)gameEventTrigger;
				this.inviterKingdom.king.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.inviterKingdom.king.name + " invited " + this.visitor.name + " for a State Visit after discovering Assassination. The State Visit was " + result + ". " + this.visitor.name + " died during the visit.", HISTORY_IDENTIFIER.NONE));
			} else if (gameEventTrigger is InvasionPlan) {
				this.inviterKingdom.king.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.inviterKingdom.king.name + " invited " + this.visitor.name + " for a State Visit after discovering Invasion Plan. The State Visit was " + result + ". " + this.visitor.name + " died during the visit.", HISTORY_IDENTIFIER.NONE));
			} else {
				this.inviterKingdom.king.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.inviterKingdom.king.name + " invited " + this.visitor.name + " for a State Visit. The State Visit was " + result + ". " + this.visitor.name + " died during the visit.", HISTORY_IDENTIFIER.NONE));
			}
		} else {
			if (gameEventTrigger is Assassination) {
				this.inviterKingdom.king.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.inviterKingdom.king.name + " invited " + this.visitor.name + " for a State Visit after discovering Assassination. The State Visit was " + result + ". " , HISTORY_IDENTIFIER.NONE));
			} else if (gameEventTrigger is InvasionPlan) {
				this.inviterKingdom.king.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.inviterKingdom.king.name + " invited " + this.visitor.name + " for a State Visit after discovering Invasion Plan. The State Visit was " + result + ". " , HISTORY_IDENTIFIER.NONE));
			} else {
				this.inviterKingdom.king.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.inviterKingdom.king.name + " invited " + this.visitor.name + " for a State Visit. The State Visit was " + result + ". " , HISTORY_IDENTIFIER.NONE));
			}
		}

	}

	private List<Kingdom> GetOtherKingdoms(){
		List<Kingdom> kingdoms = new List<Kingdom> ();
		for(int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++){
			if(KingdomManager.Instance.allKingdoms[i].id != this.inviterKingdom.id && KingdomManager.Instance.allKingdoms[i].id != this.invitedKingdom.id && KingdomManager.Instance.allKingdoms[i].isAlive()){
				kingdoms.Add (KingdomManager.Instance.allKingdoms [i]);
			}
		}
		return kingdoms;
	}
	private Kingdom GetRandomKingdom(){
		this.otherKingdoms.RemoveAll (x => !x.isAlive ());
		return this.otherKingdoms [UnityEngine.Random.Range (0, this.otherKingdoms.Count)];

	}
	private void TriggerAssassinationEvent(){
		if (this.otherKingdoms != null && this.otherKingdoms.Count > 0) {
			int chance = UnityEngine.Random.Range (0, 100);
			if (chance < 1) {
				Kingdom selectedKingdom = GetRandomKingdom();
				RelationshipKings relationship = selectedKingdom.king.SearchRelationshipByID (inviterKingdom.king.id);
				if (relationship.lordRelationship == RELATIONSHIP_STATUS.ENEMY || relationship.lordRelationship == RELATIONSHIP_STATUS.RIVAL) {
					if (selectedKingdom.king.hasTrait(TRAIT.SCHEMING)) {
						//ASSASSINATION EVENT
						Citizen spy = GetSpy (selectedKingdom);
						if (spy != null) {
							Assassination assassination = new Assassination (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, selectedKingdom.king, this.visitor, spy, this);
							Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "StateVisit", "assassination_start");
							newLog.AddToFillers (selectedKingdom.king, selectedKingdom.king.name);
							newLog.AddToFillers (spy, spy.name);
							newLog.AddToFillers (assassination, "assassinate");
							newLog.AddToFillers (this.visitor, this.visitor.name);
							newLog.AddToFillers (this.inviterKingdom, this.inviterKingdom.name);
						}
					}
				}
			}
		}
	}
	private Citizen GetSpy(Kingdom kingdom){
		List<Citizen> unwantedGovernors = GetUnwantedGovernors (kingdom.king);
		List<Citizen> spies = new List<Citizen> ();
		for(int i = 0; i < kingdom.cities.Count; i++){
			if(!IsItThisGovernor(kingdom.cities[i].governor, unwantedGovernors)){
				for(int j = 0; j < kingdom.cities[i].citizens.Count; j++){
					if (!kingdom.cities [i].citizens [j].isDead) {
						if (kingdom.cities [i].citizens [j].assignedRole != null && kingdom.cities [i].citizens [j].role == ROLE.SPY) {
							if(kingdom.cities [i].citizens [j].assignedRole is Spy){
								if (!((Spy)kingdom.cities [i].citizens [j].assignedRole).inAction) {
									spies.Add (kingdom.cities [i].citizens [j]);
								}
							}

						}
					}
				}
			}
		}

		if(spies.Count > 0){
			int random = UnityEngine.Random.Range (0, spies.Count);
			((Spy)spies [random].assignedRole).inAction = true;
			return spies [random];
		}else{
			Debug.Log (kingdom.king.name + " CAN'T SEND SPY BECAUSE THERE IS NONE!");
			return null;
		}
	}
	private void TriggerSabotage(){
		if(this.otherKingdoms != null && this.otherKingdoms.Count > 0){
			int chance = UnityEngine.Random.Range (0, 100);
			if (chance < 1) {
				Kingdom selectedKingdom = GetRandomKingdom();
				if (selectedKingdom.king.hasTrait(TRAIT.SCHEMING)) {
					if (CheckForRelationship (selectedKingdom, false)) {
						if(this.saboteurEnvoy == null){
							SendEnvoySabotage (selectedKingdom);
						}
					}
				}
			}
		}
	}
	private bool CheckForRelationship(Kingdom otherKingdom, bool isIncrease){
		RelationshipKings relationship1 = otherKingdom.king.SearchRelationshipByID (this.inviterKingdom.king.id);
		RelationshipKings relationship2 = otherKingdom.king.SearchRelationshipByID (this.invitedKingdom.king.id);

		List<RELATIONSHIP_STATUS> statuses = new List<RELATIONSHIP_STATUS> ();
		statuses.Add (relationship1.lordRelationship);
		statuses.Add (relationship2.lordRelationship);

		if(!isIncrease){
			if(statuses.Contains(RELATIONSHIP_STATUS.ENEMY) || statuses.Contains(RELATIONSHIP_STATUS.RIVAL)){
				if(!statuses.Contains(RELATIONSHIP_STATUS.FRIEND) && !statuses.Contains(RELATIONSHIP_STATUS.ALLY)){
					return true;
				}
			}
		}else{
			if(relationship2.lordRelationship == RELATIONSHIP_STATUS.FRIEND || relationship2.lordRelationship == RELATIONSHIP_STATUS.ALLY){
				return true;
			}
		}


		return false;
	}
	private void SendEnvoySabotage(Kingdom sender){
		Citizen chosenCitizen = GetEnvoy (sender);
		if(chosenCitizen == null){
			return;
		}
		Envoy chosenEnvoy = null;
		if (chosenCitizen.assignedRole is Envoy) {
			chosenEnvoy = (Envoy)chosenCitizen.assignedRole;
		}

		if(chosenEnvoy == null){
			return;
		}

		chosenEnvoy.eventDuration = 5;
		chosenEnvoy.currentEvent = this;
		chosenEnvoy.inAction = true;
		EventManager.Instance.onWeekEnd.AddListener (chosenEnvoy.WeeklyAction);
		this.saboteurEnvoy = chosenEnvoy;

		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "StateVisit", "sabotage_start");
		newLog.AddToFillers (sender.king, sender.king.name);
		newLog.AddToFillers (this.inviterKingdom.king, this.inviterKingdom.king.name);
		newLog.AddToFillers (chosenEnvoy.citizen, chosenEnvoy.citizen.name);
//		this.saboteurEnvoys.Add (chosenEnvoy);
	}
//	private void SendEnvoy(Kingdom sender, bool isDecreaseSuccess = false, bool isFromOthers = false){
//		int chance = 0;
//		if(isDecreaseSuccess){
//			chance = 10;
//			if(sender.king.behaviorTraits.Contains(BEHAVIOR_TRAIT.SCHEMING)){
//				chance = 20;
//			}
//		}else{
//			chance = 15;
//			if(isFromOthers){
//				chance = 10;
//			}
//		}
//
//		Citizen chosenEnvoy = GetEnvoy (sender);
//		if(chosenEnvoy == null){
//			return;
//		}
//
//
//		int random = UnityEngine.Random.Range (0, 100);
//		if(chance < random){
//			int amount = 15;
//			if(chosenEnvoy.skillTraits.Contains(SKILL_TRAIT.PERSUASIVE)){
//				amount += 10;
//			}
//			if(isDecreaseSuccess){
//				this.saboteurEnvoys.Add (chosenEnvoy);
//				AdjustSuccessMeter (-amount);
//			}else{
//				this.helperEnvoys.Add (chosenEnvoy);
//				AdjustSuccessMeter (amount);
//			}
//		}
//
//	}
	private Citizen GetEnvoy(Kingdom kingdom){
		List<Citizen> unwantedGovernors = GetUnwantedGovernors (kingdom.king);
		List<Citizen> envoys = new List<Citizen> ();
		for(int i = 0; i < kingdom.cities.Count; i++){
			if(!IsItThisGovernor(kingdom.cities[i].governor, unwantedGovernors)){
				for(int j = 0; j < kingdom.cities[i].citizens.Count; j++){
					if (!kingdom.cities [i].citizens [j].isDead) {
						if (kingdom.cities [i].citizens [j].assignedRole != null && kingdom.cities [i].citizens [j].role == ROLE.ENVOY) {
							if(kingdom.cities [i].citizens [j].assignedRole is Envoy){
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
			int random = UnityEngine.Random.Range (0, envoys.Count);
			((Envoy)envoys [random].assignedRole).inAction = true;
			return envoys [random];
		}else{
			Debug.Log (kingdom.king.name + " CAN'T SEND ENVOY BECAUSE THERE IS NONE!");
			return null;
		}
	}

	private void CheckVisitor(){
		if(this.visitor.isDead){
			this.visitorHasDied = true;
			this.isSuccessful = false;
			DoneEvent ();
		}
	}
	internal override void DoneCitizenAction(Envoy envoy){
		if (!envoy.citizen.isDead) {
			if (this.saboteurEnvoy != null) {
				if (envoy.citizen.id == this.saboteurEnvoy.citizen.id) {
					int chance = UnityEngine.Random.Range (0, 100);
					if (chance < 20) {
						this.isDoneBySabotage = true;
						this.isSuccessful = false;
						DoneEvent ();
						return;
					}
				}
			}
		}

		this.saboteurEnvoy = null;
	}
}
