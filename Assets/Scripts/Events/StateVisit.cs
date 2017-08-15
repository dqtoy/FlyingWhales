using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class StateVisit : GameEvent {
	public Kingdom inviterKingdom;
	public Kingdom invitedKingdom;
	public Envoy visitor;
	public List<Kingdom> otherKingdoms;
	public Envoy saboteurEnvoy;

//	protected GameEvent gameEventTrigger;

	private bool isDoneBySabotage;
	private bool visitorHasDied;
	private bool isSuccessful;
	private bool visitorHasArrived;

	public StateVisit(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom invitedKingdom, Envoy visitor) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.STATE_VISIT;
		this.name = "State Visit";
		this.description = startedBy.name + " invited " + visitor.citizen.name + " of " + invitedKingdom.name + " to visit his/her kingdom.";
		this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
		this.remainingDays = this.durationInDays;
//		this.inviterKingdom = startedBy.city.kingdom;
//		this.invitedKingdom = invitedKingdom;
		this.inviterKingdom = invitedKingdom;
		this.invitedKingdom = startedBy.city.kingdom;
		this.visitor = visitor;
		this.otherKingdoms = GetOtherKingdoms ();
		this.saboteurEnvoy = null;
//		this.gameEventTrigger = gameEventTrigger;
		this.isDoneBySabotage = false;
		this.visitorHasDied = false;
		this.isSuccessful = false;
		this.visitorHasArrived = false;
		Messenger.AddListener("OnDayEnd", this.PerformAction);

		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "StateVisit", "event_title");
		newLogTitle.AddToFillers (visitor.citizen, visitor.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		newLogTitle.AddToFillers (this.inviterKingdom, this.inviterKingdom.name, LOG_IDENTIFIER.KINGDOM_2);

		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "StateVisit", "start");
		newLog.AddToFillers (visitor.citizen, visitor.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		newLog.AddToFillers (this.inviterKingdom, this.inviterKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		newLog.AddToFillers (this.invitedKingdom.king, this.invitedKingdom.king.name, LOG_IDENTIFIER.KING_1);

		EventManager.Instance.AddEventToDictionary (this);
		this.EventIsCreated (this.invitedKingdom, true);
		this.EventIsCreated (this.inviterKingdom, false);

	}

	#region Overrides
	internal override void DoneCitizenAction(Citizen citizen){
        base.DoneCitizenAction(citizen);
		this.visitorHasArrived = true;
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "StateVisit", "visitor_arrival");
		newLog.AddToFillers (this.visitor.citizen, this.visitor.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		newLog.AddToFillers (this.inviterKingdom, this.inviterKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
	}
	internal override void PerformAction(){
		CheckVisitor ();
		TriggerSabotage ();
//		TriggerAssassinationEvent ();
		if(this.visitorHasArrived){
			if(!this.visitor.citizen.isDead){
				this.remainingDays -= 1;
				if(this.remainingDays <= 0){
					this.remainingDays = 0;
					this.isSuccessful = true;
					DoneEvent ();
				}
			}else{
				this.isSuccessful = false;
				DoneEvent ();
			}
		}

	}

	internal override void DoneEvent(){
        base.DoneEvent();
		RelationshipKings relationship = null;
		if(this.inviterKingdom.isAlive()){
			relationship = this.inviterKingdom.king.SearchRelationshipByID (this.invitedKingdom.king.id);
		}
		if(this.isSuccessful){
			if(relationship != null){
				relationship.AddEventModifier (5, this.name + " event", this);
			}
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "StateVisit", "event_end");
			newLog.AddToFillers (this.invitedKingdom.king, this.invitedKingdom.king.name, LOG_IDENTIFIER.KING_1);
			newLog.AddToFillers (this.inviterKingdom.king, this.inviterKingdom.king.name, LOG_IDENTIFIER.KING_2);
			newLog.AddToFillers (this.visitor.citizen, this.visitor.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		}else{
			if(this.isDoneBySabotage){
				if (relationship != null) {
					relationship.AddEventModifier (-3, this.name + " event", this);
				}
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "StateVisit", "sabotage_success");
				newLog.AddToFillers (this.saboteurEnvoy.citizen, this.saboteurEnvoy.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				newLog.AddToFillers (this.invitedKingdom.king, this.invitedKingdom.king.name, LOG_IDENTIFIER.KING_1);
				newLog.AddToFillers (this.inviterKingdom.king, this.inviterKingdom.king.name, LOG_IDENTIFIER.KING_2);
			}else{
				if(this.visitorHasDied){
					if (relationship != null) {
						if (relationship.totalLike <= 0) {
							relationship.AddEventModifier (-9, this.name + " event", this);
						} else {
							relationship.AddEventModifier (-9,  this.name + " event", this);
							relationship.UpdateKingRelationshipStatus ();
						}
					}

					Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "StateVisit", "visitor_died");
					newLog.AddToFillers (this.visitor.citizen, this.visitor.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
					newLog.AddToFillers (this.invitedKingdom.king, this.invitedKingdom.king.name, LOG_IDENTIFIER.KING_1);
					newLog.AddToFillers (this.inviterKingdom.king, this.inviterKingdom.king.name, LOG_IDENTIFIER.KING_2);
				}
			}
		}
//		if(this.visitor != null){
//			this.visitor.DestroyGO ();
//		}
//		if(this.saboteurEnvoy != null){
//			this.saboteurEnvoy.DestroyGO();
//		}
		Messenger.RemoveListener("OnDayEnd", this.PerformAction);
	}
	internal override void DeathByOtherReasons(){
		//Add logs: death_by_other
		this.visitorHasDied = true;
		this.isSuccessful = false;
		this.DoneEvent();
	}
	internal override void DeathByAgent(Citizen citizen, Citizen deadCitizen){
		//Add logs: death_by_general
		base.DeathByAgent(citizen, deadCitizen);
		this.visitorHasDied = true;
		this.isSuccessful = false;
		this.DoneEvent();
	}
	internal override void CancelEvent (){
		base.CancelEvent ();
		this.isDoneBySabotage = false;
		this.visitorHasDied = false;
		this.isSuccessful = false;
		this.visitorHasArrived = false;
		this.DoneEvent ();
	}
	#endregion
	private List<Kingdom> GetOtherKingdoms(){
		List<Kingdom> kingdoms = new List<Kingdom> ();
		for(int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++){
            Kingdom currKingdom = KingdomManager.Instance.allKingdoms[i];
            if (currKingdom.id != this.inviterKingdom.id && currKingdom.id != this.invitedKingdom.id 
                && currKingdom.isAlive() 
                && (currKingdom.discoveredKingdoms.Contains(this.invitedKingdom) || currKingdom.discoveredKingdoms.Contains(this.inviterKingdom))) {
				kingdoms.Add (KingdomManager.Instance.allKingdoms [i]);
			}
		}
		return kingdoms;
	}
	private Kingdom GetRandomKingdomForAction(){
		this.otherKingdoms.RemoveAll (x => !x.isAlive ());
        //List<Kingdom> kingdomsToChooseFrom = this.otherKingdoms
        //    .Where(x => x.discoveredKingdoms.Contains(targetKingdom)).ToList();

        return this.otherKingdoms[UnityEngine.Random.Range (0, this.otherKingdoms.Count)];

	}
	private void TriggerAssassinationEvent(){
		if (this.otherKingdoms != null && this.otherKingdoms.Count > 0) {
			int chance = UnityEngine.Random.Range (0, 100);
			if (chance < 1) {
				Kingdom selectedKingdom = GetRandomKingdomForAction();

                RelationshipKings relationship = selectedKingdom.king.SearchRelationshipByID (inviterKingdom.king.id);
				if (relationship.lordRelationship == RELATIONSHIP_STATUS.ENEMY || relationship.lordRelationship == RELATIONSHIP_STATUS.RIVAL) {
					if (selectedKingdom.king.hasTrait(TRAIT.SCHEMING)) {

						KingdomManager.Instance.DiscoverKingdom (selectedKingdom, this.visitor.citizen.city.kingdom);
//                        selectedKingdom.DiscoverKingdom(this.visitor.citizen.city.kingdom);
//                        this.visitor.citizen.city.kingdom.DiscoverKingdom(selectedKingdom);

						KingdomManager.Instance.DiscoverKingdom (selectedKingdom, inviterKingdom);

//                        selectedKingdom.DiscoverKingdom(inviterKingdom);
//                        inviterKingdom.DiscoverKingdom(selectedKingdom);

                        //ASSASSINATION EVENT
                        int remainingDays = this.visitor.path.Sum(x => x.movementDays);
						Assassination assassination = EventCreator.Instance.CreateAssassinationEvent (selectedKingdom, this.visitor.citizen, this, remainingDays, ASSASSINATION_TRIGGER_REASONS.STATE_VISIT);
						if(assassination != null){
							Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "StateVisit", "assassination_start");
							newLog.AddToFillers (selectedKingdom.king, selectedKingdom.king.name, LOG_IDENTIFIER.KING_1);
							newLog.AddToFillers (assassination.spy.citizen, assassination.spy.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
							newLog.AddToFillers (assassination, "assassinate", LOG_IDENTIFIER.GAME_EVENT);
							newLog.AddToFillers (this.visitor.citizen, this.visitor.citizen.name, LOG_IDENTIFIER.TARGET_CHARACTER);
							newLog.AddToFillers (this.inviterKingdom, this.inviterKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
						}

					}
				}
			}
		}
	}
	/*private Citizen GetSpy(Kingdom kingdom){
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
//			Debug.Log (kingdom.king.name + " CAN'T SEND SPY BECAUSE THERE IS NONE!");
			return null;
		}
	}*/
	private void TriggerSabotage(){
		if(this.otherKingdoms != null && this.otherKingdoms.Count > 0){
			int chance = UnityEngine.Random.Range (0, 100);
			if (chance < 1) {
				Kingdom selectedKingdom = GetRandomKingdomForAction();
				if (CheckForRelationship (selectedKingdom, false)) {
					if(this.saboteurEnvoy == null){
						SendEnvoySabotage (selectedKingdom);
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
		if (this.inviterKingdom == null) {
			return;
		}
		if (this.inviterKingdom.capitalCity == null) {
			return;
		}
		int remainingDays = this.visitor.path.Sum(x => x.movementDays);
			
		Sabotage sabotage = EventCreator.Instance.CreateSabotageEvent(sender, this.inviterKingdom, this, remainingDays);
		if(sabotage != null){
			KingdomManager.Instance.DiscoverKingdom (sender, this.inviterKingdom);
//            sender.DiscoverKingdom(this.inviterKingdom);
//            this.inviterKingdom.DiscoverKingdom(sender);
			this.saboteurEnvoy = sabotage.saboteur;
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "StateVisit", "sabotage_start");
			newLog.AddToFillers (sender.king, sender.king.name, LOG_IDENTIFIER.KING_1);
			newLog.AddToFillers (this.inviterKingdom.king, this.inviterKingdom.king.name, LOG_IDENTIFIER.KING_2);
			newLog.AddToFillers (this.saboteurEnvoy.citizen, this.saboteurEnvoy.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		}


//		Citizen chosenCitizen = GetEnvoy (sender);
//		if(chosenCitizen == null){
//			return;
//		}
//		Envoy chosenEnvoy = null;
//		if (chosenCitizen.assignedRole is Envoy) {
//			chosenEnvoy = (Envoy)chosenCitizen.assignedRole;
//		}
//
//		if(chosenEnvoy == null){
//			return;
//		}
//
//		chosenEnvoy.eventDuration = 5;
//		chosenEnvoy.currentEvent = this;
//		chosenEnvoy.inAction = true;
//		Messenger.AddListener("OnDayEnd", chosenEnvoy.WeeklyAction);
//		this.saboteurEnvoy = chosenEnvoy;
//
//		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "StateVisit", "sabotage_start");
//		newLog.AddToFillers (sender.king, sender.king.name);
//		newLog.AddToFillers (this.inviterKingdom.king, this.inviterKingdom.king.name);
//		newLog.AddToFillers (chosenEnvoy.citizen, chosenEnvoy.citizen.name);
	}

	/*private Citizen GetEnvoy(Kingdom kingdom){
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
//			Debug.Log (kingdom.king.name + " CAN'T SEND ENVOY BECAUSE THERE IS NONE!");
			return null;
		}
	}*/

	private void CheckVisitor(){
		if(this.visitor.citizen.isDead){
			this.visitorHasDied = true;
			this.isSuccessful = false;
			DoneEvent ();
		}
	}
	internal void EventIsSabotaged(){
		this.isDoneBySabotage = true;
		this.isSuccessful = false;
		DoneEvent ();
	}
//	internal override void DoneCitizenAction(Envoy envoy){
//		if (!envoy.citizen.isDead) {
//			if (this.saboteurEnvoy != null) {
//				if (envoy.citizen.id == this.saboteurEnvoy.citizen.id) {
//					int chance = UnityEngine.Random.Range (0, 100);
//					if (chance < 20) {
//						this.isDoneBySabotage = true;
//						this.isSuccessful = false;
//						DoneEvent ();
//						return;
//					}
//				}
//			}
//		}
//		envoy.DestroyGO ();
//		this.saboteurEnvoy = null;
//	}
}
