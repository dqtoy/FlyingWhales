using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Assassination : GameEvent {
	public Kingdom assassinKingdom;
	public Kingdom targetKingdom;
	private Citizen _targetCitizen;
	public List<Kingdom> otherKingdoms;
//	public List<Citizen> guardians;
	public List<Citizen> uncovered;
	public Spy spy;
	public int successRate = 0;

	private bool hasBeenDiscovered;
	private bool hasDeflected;
	private bool hasAssassinated;
	private bool hasSpyDied;
	private Kingdom kingdomToBlame;
	private RelationshipKings relationshipToAdjust;

	public Citizen targetCitizen {
		get { 
			return this._targetCitizen; 
		}
	}

	public Assassination(int startWeek, int startMonth, int startYear, Citizen startedBy, Citizen targetCitizen, Spy spy, GameEvent gameEventTrigger) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.ASSASSINATION;
		this.eventStatus = EVENT_STATUS.HIDDEN;
		this.name = "Assassination";
		this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
		this.remainingDays = this.durationInDays;
		this.assassinKingdom = startedBy.city.kingdom;
		this.targetKingdom = targetCitizen.city.kingdom;
		this._targetCitizen = targetCitizen;
		this.otherKingdoms = GetOtherKingdoms ();
//		this.guardians = new List<Citizen>();
		this.uncovered = new List<Citizen>();
		this.spy = spy;
		this.successRate = GetActualChancePercentage ();
		this.hasBeenDiscovered = false;
		this.hasDeflected = false;
		this.hasAssassinated = false;
		this.hasSpyDied = false;
		this.kingdomToBlame = null;
		this._warTrigger = GetWarTrigger ();
		this.relationshipToAdjust = null;

		EventManager.Instance.AddEventToDictionary(this);

		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Assassination", "event_title");
		newLogTitle.AddToFillers (null, this._targetCitizen.name, LOG_IDENTIFIER.TARGET_CHARACTER);

//		string triggerReason = string.Empty;
		if (gameEventTrigger is Assassination) {
			Assassination assassination = (Assassination)gameEventTrigger;
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Reasons", "AssassinationReasons", assassination.warTrigger.ToString());

			if(assassination.warTrigger == WAR_TRIGGER.ASSASSINATION_KING){
				newLog.AddToFillers (this.startedBy, this.startedBy.name, LOG_IDENTIFIER.KING_1);
				newLogTitle.AddToFillers (this.assassinKingdom, this.assassinKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
				newLogTitle.AddToFillers (this.spy, this.spy.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				newLogTitle.AddToFillers (this._targetCitizen, this._targetCitizen.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				newLogTitle.AddToFillers (this.targetKingdom, this.targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);

			}else if(assassination.warTrigger == WAR_TRIGGER.ASSASSINATION_ROYALTY){

			}else if(assassination.warTrigger == WAR_TRIGGER.ASSASSINATION_GOVERNOR){

			}
		}else if (gameEventTrigger is BorderConflict){
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Reasons", "AssassinationReasons", gameEventTrigger.warTrigger.ToString());
			newLog.AddToFillers (this.startedBy, this.startedBy.name, LOG_IDENTIFIER.KING_1);
			newLogTitle.AddToFillers (this.assassinKingdom, this.assassinKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
			newLogTitle.AddToFillers (this.spy, this.spy.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			newLogTitle.AddToFillers (this._targetCitizen, this._targetCitizen.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			newLogTitle.AddToFillers (this.targetKingdom, this.targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		} else if (gameEventTrigger is DiplomaticCrisis){
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Reasons", "AssassinationReasons", gameEventTrigger.warTrigger.ToString());
			newLog.AddToFillers (this.startedBy, this.startedBy.name, LOG_IDENTIFIER.KING_1);
			newLogTitle.AddToFillers (this.assassinKingdom, this.assassinKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
			newLogTitle.AddToFillers (this.spy, this.spy.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			newLogTitle.AddToFillers (this._targetCitizen, this._targetCitizen.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			newLogTitle.AddToFillers (this.targetKingdom, this.targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		}


//		string triggerReason = string.Empty;
//		if (gameEventTrigger is Assassination) {
//			triggerReason = "after discovering that " + gameEventTrigger.startedBy.name
//				+ " also sent an assassin to kill " + (gameEventTrigger as Assassination).targetCitizen.name;
//		} else if (gameEventTrigger is InvasionPlan) {
//			triggerReason = "after discovering " + gameEventTrigger.startedBy.name + "'s "
//				+ " Invasion Plan against " + (gameEventTrigger as InvasionPlan).targetKingdom.name;
//		} else if (gameEventTrigger is StateVisit){
//			triggerReason = "while on a State Visit to " + targetCitizen.city.kingdom.name;
//		} else if (gameEventTrigger is BorderConflict){
//			triggerReason = "in response to worsening Border Conflict";
//		} else if (gameEventTrigger is DiplomaticCrisis){
//			triggerReason = "in the aftermath of a recent Diplomatic Crisis";
//		} else if (gameEventTrigger is Espionage){
//			triggerReason = "after finding out that " + gameEventTrigger.startedBy.name + " spied on " + (gameEventTrigger as Espionage).targetKingdom.name;
//		} else if (gameEventTrigger is Raid){
//			triggerReason = "after the raid of " + (gameEventTrigger as Raid).raidedCity.name;
//		} else {
//			string gender = "he";
//			if(startedBy.gender == GENDER.FEMALE){
//				gender = "she";
//			}
//			triggerReason = "because " + gender + " doesn't like " + targetCitizen.name;
//		} 

//		newLog.AddToFillers (startedBy, startedBy.name, LOG_IDENTIFIER.KING_1);
//		newLog.AddToFillers (this.assassinKingdom, this.assassinKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
//		newLog.AddToFillers (spy.citizen, spy.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
//		newLog.AddToFillers (targetCitizen, targetCitizen.name, LOG_IDENTIFIER.TARGET_CHARACTER);
//		newLog.AddToFillers (gameEventTrigger, triggerReason, LOG_IDENTIFIER.TRIGGER_REASON);

		this.EventIsCreated ();

	}

	#region Overrides
	internal override void PerformAction(){
		if(this.spy.location == this.targetCitizen.currentLocation){
			AssassinationMoment ();
			DoneEvent ();
		}
//		this.remainingDays -= 1;
//		if(this.remainingDays <= 0){
//			this.remainingDays = 0;
//			AssassinationMoment ();
//			DoneEvent ();
//		}else{
//			TriggerGuardian ();
//		}
	}

	internal override void DoneCitizenAction (Citizen citizen){
        base.DoneCitizenAction(citizen);
		if(citizen.assignedRole is Spy){
			if(citizen.id == this.spy.citizen.id){
				if(this.targetCitizen.isDead || (this.targetCitizen.assignedRole != null && this.targetCitizen.assignedRole.isDestroyed)){
					AssassinationFail ();
				}else{
					if (this.spy.location == this.targetCitizen.currentLocation) {
						AssassinationMoment ();
						DoneEvent ();
					}else{
						WaitForTarget ();
					}
				}
			}
		}
	}
	internal override void DoneEvent(){
        //		if(this.spy != null){
        //			this.spy.DestroyGO();
        //		}
        //		for(int i = 0; i < this.guardians.Count; i++){
        //			((Guardian)this.guardians[i].assignedRole).inAction = false;
        //		}
        base.DoneEvent();
		EventManager.Instance.onWeekEnd.RemoveListener (this.PerformAction);
	}

	internal override void CancelEvent (){
		base.CancelEvent ();
		this.DoneEvent();
	}
	internal override void DeathByOtherReasons(){
		this.DoneEvent();
	}
	internal override void DeathByGeneral(General general){
		this.spy.citizen.Death (DEATH_REASONS.BATTLE);
		this.DoneEvent();
	}
	#endregion

	private void WaitForTarget(){
		//Add logs: wait_for_target
		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
	}
	private List<Kingdom> GetOtherKingdoms(){
		List<Kingdom> kingdoms = new List<Kingdom> ();
		for(int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++){
			if(KingdomManager.Instance.allKingdoms[i].id != this.assassinKingdom.id && KingdomManager.Instance.allKingdoms[i].id != this.targetKingdom.id && KingdomManager.Instance.allKingdoms[i].isAlive()){
				kingdoms.Add (KingdomManager.Instance.allKingdoms [i]);
			}
		}
		return kingdoms;
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
	private Citizen GetGuardian(Kingdom kingdom){
		List<Citizen> unwantedGovernors = GetUnwantedGovernors (kingdom.king);
		List<Citizen> guardian = new List<Citizen> ();
		for(int i = 0; i < kingdom.cities.Count; i++){
			if(!IsItThisGovernor(kingdom.cities[i].governor, unwantedGovernors)){
				for(int j = 0; j < kingdom.cities[i].citizens.Count; j++){
					if(!kingdom.cities[i].citizens[j].isDead){
						if(kingdom.cities[i].citizens[j].assignedRole != null && kingdom.cities[i].citizens[j].role == ROLE.GUARDIAN){
							if(kingdom.cities[i].citizens[j].assignedRole is Guardian){
								if(!((Guardian)kingdom.cities[i].citizens[j].assignedRole).inAction){
									guardian.Add (kingdom.cities [i].citizens [j]);
								}
							}
						}
					}

				}
			}
		}

		if(guardian.Count > 0){
			return guardian [UnityEngine.Random.Range (0, guardian.Count)];
		}else{
//			Debug.Log (kingdom.king.name + " CAN'T SEND GAURDIAN BECAUSE THERE IS NONE!");
			return null;
		}
	}

	private void TriggerGuardian(){
		Citizen kingOfTarget = this.targetKingdom.king;
		int value = 0;
		for(int i = 0; i < this.otherKingdoms.Count; i++){
			if(this.otherKingdoms[i].king.hasTrait(TRAIT.HONEST) && this.otherKingdoms[i].isAlive()){
				int chance = UnityEngine.Random.Range (0, 100);
				RelationshipKings relationship = this.otherKingdoms [i].king.SearchRelationshipByID (kingOfTarget.id);
				if(relationship.lordRelationship == RELATIONSHIP_STATUS.FRIEND){
					value = 5;
				}else if(relationship.lordRelationship == RELATIONSHIP_STATUS.ALLY){
					value = 10;
				}
				if(chance < value){
					AssignGuardian (this.otherKingdoms [i]);
				}
			}
		}
	}
	private void AssignGuardian(Kingdom otherKingdom){
		Citizen guardian = GetGuardian (otherKingdom);
		if (guardian != null) {
			((Guardian)guardian.assignedRole).inAction = true;
//			this.guardians.Add (guardian);

			if(otherKingdom.king.id == this._targetCitizen.id){
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Assassination", "guardian_same");
				newLog.AddToFillers (otherKingdom.king, otherKingdom.king.name, LOG_IDENTIFIER.KING_1);
				newLog.AddToFillers (otherKingdom, otherKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
				newLog.AddToFillers (guardian, guardian.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			}else{
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Assassination", "guardian_different");
				newLog.AddToFillers (otherKingdom.king, otherKingdom.king.name, LOG_IDENTIFIER.KING_1);
				newLog.AddToFillers (otherKingdom, otherKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
				newLog.AddToFillers (guardian, guardian.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				newLog.AddToFillers (this._targetCitizen, this._targetCitizen.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			}

		}
	}
	private void AssassinationMoment(){
		if(this.spy == null){
//			Debug.Log ("CAN'T ASSASSINATE NO SPIES AVAILABLE");
			return;
		}
		if(this.spy.citizen.isDead){
//			Debug.Log ("CAN'T ASSASSINATE, SPY IS DEAD!");
			return;
		}
		int chance = UnityEngine.Random.Range (0, 100);

//		this.successRate -= GetGuardianReduction ();
		if(chance < this.successRate){
			AssassinateTarget ();		
		}

		SpyDiscovery ();
		if (this.hasAssassinated && this.hasBeenDiscovered) {
			if (this.hasDeflected) { //success_discovery_deflect
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Assassination", "success_discovery_deflect");
				newLog.AddToFillers (this.spy.citizen, this.spy.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				newLog.AddToFillers (this._targetCitizen, this._targetCitizen.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				newLog.AddToFillers (this.kingdomToBlame, this.kingdomToBlame.name, LOG_IDENTIFIER.KINGDOM_1);
				newLog.AddToFillers (this.targetKingdom.king, this.targetKingdom.king.name, LOG_IDENTIFIER.KING_2);
				newLog.AddToFillers (this.kingdomToBlame.king, this.kingdomToBlame.king.name, LOG_IDENTIFIER.KING_1);
			} else { //success_discovery
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Assassination", "success_discovery");
				newLog.AddToFillers (this.spy.citizen, this.spy.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				newLog.AddToFillers (this._targetCitizen, this._targetCitizen.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				newLog.AddToFillers (this.targetKingdom.king, this.targetKingdom.king.name, LOG_IDENTIFIER.KING_2);
				newLog.AddToFillers (this.assassinKingdom.king, this.assassinKingdom.king.name, LOG_IDENTIFIER.KING_1);
			}

		} else if (!this.hasAssassinated && this.hasBeenDiscovered) {
			if (this.hasDeflected) { //fail_discovery_deflect
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Assassination", "fail_discovery_deflect");
				newLog.AddToFillers (this.spy.citizen, this.spy.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				newLog.AddToFillers (this._targetCitizen, this._targetCitizen.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				newLog.AddToFillers (this.kingdomToBlame, this.kingdomToBlame.name, LOG_IDENTIFIER.KINGDOM_1);
				newLog.AddToFillers (this.targetKingdom.king, this.targetKingdom.king.name, LOG_IDENTIFIER.KING_2);
				newLog.AddToFillers (this.kingdomToBlame.king, this.kingdomToBlame.king.name, LOG_IDENTIFIER.KING_1);
			} else { //fail_discovery
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Assassination", "fail_discovery");
				newLog.AddToFillers (this.spy.citizen, this.spy.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				newLog.AddToFillers (this._targetCitizen, this._targetCitizen.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				newLog.AddToFillers (this.targetKingdom.king, this.targetKingdom.king.name, LOG_IDENTIFIER.KING_2);
				newLog.AddToFillers (this.assassinKingdom.king, this.assassinKingdom.king.name, LOG_IDENTIFIER.KING_1);
			}
		} else if (this.hasAssassinated && !this.hasBeenDiscovered) { //success
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Assassination", "success");
			newLog.AddToFillers (this.spy.citizen, this.spy.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			newLog.AddToFillers (this._targetCitizen, this._targetCitizen.name, LOG_IDENTIFIER.TARGET_CHARACTER);
		} else if (!this.hasAssassinated && !this.hasBeenDiscovered) { //fail
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Assassination", "fail");
			newLog.AddToFillers (this.spy.citizen, this.spy.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			newLog.AddToFillers (this._targetCitizen, this._targetCitizen.name, LOG_IDENTIFIER.TARGET_CHARACTER);
		}

		if(!this.hasAssassinated){
			int dieSpy = UnityEngine.Random.Range (0, 100);
			if(dieSpy < 5){
				this.hasSpyDied = true;
			}
		}
		if(this.hasSpyDied){
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Assassination", "spy_died");
			newLog.AddToFillers (this.spy.citizen, this.spy.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			this.spy.citizen.Death (DEATH_REASONS.TREACHERY);
//			Debug.Log (this.spy.name + " HAS DIED!");
		}else{
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Assassination", "spy_alive");
			newLog.AddToFillers (this.spy.citizen, this.spy.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		}
	}
	private void AssassinationFail (){
		//Add logs: assassination_fail
		DoneEvent ();
	}
	private int GetActualChancePercentage(){
		int value = 35;
		if(this._targetCitizen.isKing){
			value = 25;
		}else if(this._targetCitizen.isGovernor){
			value = 30;
		}
		return value;
	}

	private WAR_TRIGGER GetWarTrigger(){
		if(this._targetCitizen.isKing){
			return WAR_TRIGGER.ASSASSINATION_KING;
		}else if(this._targetCitizen.isGovernor){
			return WAR_TRIGGER.ASSASSINATION_GOVERNOR;
		}else if(this._targetCitizen.isDirectDescendant){
			return WAR_TRIGGER.ASSASSINATION_ROYALTY;
		}else{
			return WAR_TRIGGER.ASSASSINATION_CIVILIAN;
		}
	}


	//Gets the percentage that the assassination success rate will reduce
	private int GetGuardianReduction(){
		return 0;
//		return this.guardians.Count * 15;
	}
	private void AssassinateTarget(){
		if(!this._targetCitizen.isDead){
			this._targetCitizen.Death (DEATH_REASONS.ASSASSINATION);
			this.hasAssassinated = true;
		}else{
			this.hasAssassinated = false;
		}

	}
	private void SpyDiscovery(){
		int chance = UnityEngine.Random.Range (0, 100);
		int value = 20;
		if(chance < value){
			this.hasBeenDiscovered = true;
			if(this.targetKingdom.isAlive()){
				this.relationshipToAdjust = this.targetKingdom.king.SearchRelationshipByID (this.assassinKingdom.king.id);
			}
			if(this.assassinKingdom.king.hasTrait(TRAIT.SCHEMING)){
				int deflectChance = UnityEngine.Random.Range (0, 100);
				if(deflectChance < 35){
					Kingdom kingdomToBlameCopy = GetRandomKingdomToBlame ();
					if(kingdomToBlameCopy != null){
						this.hasDeflected = true;
						this.kingdomToBlame = kingdomToBlameCopy;
						if (this.targetKingdom.isAlive ()) {
							this.relationshipToAdjust = this.targetKingdom.king.SearchRelationshipByID (this.kingdomToBlame.king.id);
						}
					}
				}
			}
			if(this.relationshipToAdjust != null){
				this.relationshipToAdjust.AdjustLikeness (-15, this);
				this.relationshipToAdjust.sourceKing.WarTrigger (this.relationshipToAdjust, this, this.relationshipToAdjust.sourceKing.city.kingdom.kingdomTypeData, this._warTrigger);
			}
		}
	}
	private Kingdom GetRandomKingdomToBlame(){
		if(this.otherKingdoms == null || this.otherKingdoms.Count <= 0){
			return null;
		}
		this.otherKingdoms.RemoveAll (x => !x.isAlive ());
		return this.otherKingdoms [UnityEngine.Random.Range (0, this.otherKingdoms.Count)];
	
	}
}
