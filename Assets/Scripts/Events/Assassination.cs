using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Assassination : GameEvent {
	public Kingdom assassinKingdom;
	private Citizen _targetCitizen;
	public List<Kingdom> otherKingdoms;
	public List<Citizen> guardians;
	public List<Citizen> uncovered;
	public Citizen spy;
	public int successRate = 0;

	public Citizen targetCitizen {
		get { 
			return this._targetCitizen; 
		}
	}

	public Assassination(int startWeek, int startMonth, int startYear, Citizen startedBy, Citizen targetCitizen, Citizen spy, GameEvent gameEventTrigger) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.ASSASSINATION;
		this.eventStatus = EVENT_STATUS.HIDDEN;
		this.durationInDays = 15;
		this.remainingDays = this.durationInDays;
		this.assassinKingdom = startedBy.city.kingdom;
		this._targetCitizen = targetCitizen;
		this.otherKingdoms = GetOtherKingdoms ();
		this.guardians = new List<Citizen>();
		this.uncovered = new List<Citizen>();
		this.spy = spy;
		this.successRate = GetActualChancePercentage (this.spy);

		string triggerReason = string.Empty;
		if (gameEventTrigger is Assassination) {
			triggerReason = "after discovering that " + gameEventTrigger.startedBy.name
				+ " also sent an assassin to kill " + (gameEventTrigger as Assassination).targetCitizen.name;
		} else if (gameEventTrigger is InvasionPlan) {
			triggerReason = "after discovering " + gameEventTrigger.startedBy.name + "'s "
				+ " Invasion Plan against " + (gameEventTrigger as InvasionPlan).targetKingdom.name;
		} else if (gameEventTrigger is StateVisit){
			triggerReason = "while on a State Visit to " + targetCitizen.city.kingdom.name + ".";
		} else if (gameEventTrigger is BorderConflict){
			triggerReason = "in response to worsening Border Conflict.";
		} else if (gameEventTrigger is DiplomaticCrisis){
			triggerReason = "in the aftermath of a recent Diplomatic Crisis.";
		} else if (gameEventTrigger is Espionage){
			triggerReason = "after finding out that " + gameEventTrigger.startedBy.name + " spied on " + (gameEventTrigger as Espionage).targetKingdom.name + ".";
		} else if (gameEventTrigger is Raid){
			triggerReason = "after the raid of " + (gameEventTrigger as Raid).raidedCity.name + ".";
		} else {
			string gender = "he";
			if(startedBy.gender == GENDER.FEMALE){
				gender = "she";
			}
			triggerReason = "because " + gender + " doesn't like " + targetCitizen.name;
		} 
//		if (gameEventTrigger is Assassination) {
//			this.description = startedBy.name + " sent " + this.spy.name + " to kill " + targetCitizen.name + " after discovering that " + gameEventTrigger.startedBy.name
//							+ " also sent an assassin to kill " + (gameEventTrigger as Assassination).targetCitizen.name;
//			startedBy.history.Add(new History(startMonth, startWeek, startYear, this.description, HISTORY_IDENTIFIER.NONE));
//
//		} else if (gameEventTrigger is InvasionPlan) {
//			this.description = startedBy.name + " sent " + this.spy.name + " to kill " + targetCitizen.name + " after discovering " + gameEventTrigger.startedBy.name + "'s "
//				+ " Invasion Plan against " + (gameEventTrigger as InvasionPlan).targetKingdom.name;
//			startedBy.history.Add(new History(startMonth, startWeek, startYear, this.description, HISTORY_IDENTIFIER.NONE));
//
//		} else if (gameEventTrigger is StateVisit){
//			this.description = startedBy.name + " sent " + this.spy.name + " to kill " + targetCitizen.name + " while on a State Visit to " + targetCitizen.city.kingdom.name + ".";
//			startedBy.history.Add(new History(startMonth, startWeek, startYear, this.description, HISTORY_IDENTIFIER.NONE));
//
//		} else if (gameEventTrigger is BorderConflict){
//			this.description = startedBy.name + " sent " + this.spy.name + " to kill " + targetCitizen.name + " in response to worsening Border Conflict.";
//			startedBy.history.Add(new History(startMonth, startWeek, startYear, this.description, HISTORY_IDENTIFIER.NONE));
//
//		} else if (gameEventTrigger is DiplomaticCrisis){
//			this.description = startedBy.name + " sent " + this.spy.name + " to kill " + targetCitizen.name + " in the aftermath of a recent Diplomatic Crisis.";
//			startedBy.history.Add(new History(startMonth, startWeek, startYear, this.description, HISTORY_IDENTIFIER.NONE));
//
//		} else if (gameEventTrigger is Espionage){
//			this.description = startedBy.name + " sent " + this.spy.name + " to kill " + targetCitizen.name + " after finding out that " + gameEventTrigger.startedBy.name + " spied on " + (gameEventTrigger as Espionage).targetKingdom.name + ".";
//			startedBy.history.Add(new History(startMonth, startWeek, startYear, this.description, HISTORY_IDENTIFIER.NONE));
//
//		} else if (gameEventTrigger is Raid){
//			this.description = startedBy.name + " sent " + this.spy.name + " to kill " + targetCitizen.name + " after the raid of " + (gameEventTrigger as Raid).raidedCity.name + ".";
//			startedBy.history.Add(new History(startMonth, startWeek, startYear, this.description, HISTORY_IDENTIFIER.NONE));
//
//		} else {
//			this.description = startedBy.name + " sent " + this.spy.name + " to kill " + targetCitizen.name + ".";
//			startedBy.history.Add(new History(startMonth, startWeek, startYear, this.description, HISTORY_IDENTIFIER.NONE));
//
//		} 
		/*
		else {
			startedBy.history.Add(new History(startMonth, startWeek, startYear, startedBy.name + " sent " + this.spy.name + " to kill " + targetCitizen.name + " after relationship deterioration due to " + triggerReason.ToString() + ".", HISTORY_IDENTIFIER.NONE));
			this.description = startedBy.name + " of " + startedBy.city.kingdom.name + " wants to assassinate " + targetCitizen.name + " of " + targetCitizen.city.kingdom.name + " after relationship deterioration due to " + triggerReason.ToString() + ".";

		}
		*/

		EventManager.Instance.AddEventToDictionary(this);
		this.targetCitizen.city.hexTile.AddEventOnTile(this);
		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);

		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Assassination", "event_title");
		newLogTitle.AddToFillers (this._targetCitizen, this._targetCitizen.name);

		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Assassination", "start");
		newLog.AddToFillers (startedBy, startedBy.name);
		newLog.AddToFillers (this.assassinKingdom, this.assassinKingdom.name);
		newLog.AddToFillers (spy, spy.name);
		newLog.AddToFillers (targetCitizen, targetCitizen.name);
		newLog.AddToFillers (null, triggerReason);
	}

	internal override void PerformAction(){
		this.remainingDays -= 1;
		if(this.remainingDays <= 0){
			this.remainingDays = 0;
			AssassinationMoment ();
			DoneEvent ();
		}else{
			TriggerGuardian ();
		}
	}

	internal override void DoneEvent(){
		if(this.spy != null){
			((Spy)this.spy.assignedRole).inAction = false;
		}
		for(int i = 0; i < this.guardians.Count; i++){
			((Guardian)this.guardians[i].assignedRole).inAction = false;
		}
		EventManager.Instance.onWeekEnd.RemoveListener (this.PerformAction);
		this.isActive = false;
		EventManager.Instance.onGameEventEnded.Invoke(this);
		this.endMonth = GameManager.Instance.month;
		this.endDay = GameManager.Instance.days;
		this.endYear = GameManager.Instance.year;
//		EventManager.Instance.allEvents [EVENT_TYPES.ASSASSINATION].Remove (this);

	}

	private List<Kingdom> GetOtherKingdoms(){
		List<Kingdom> kingdoms = new List<Kingdom> ();
		for(int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++){
			if(KingdomManager.Instance.allKingdoms[i].id != this.assassinKingdom.id && KingdomManager.Instance.allKingdoms[i].id != this._targetCitizen.city.kingdom.id){
				kingdoms.Add (KingdomManager.Instance.allKingdoms [i]);
			}
		}
		return kingdoms;
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
			Debug.Log (kingdom.king.name + " CAN'T SEND GAURDIAN BECAUSE THERE IS NONE!");
			return null;
		}
	}

	private void TriggerGuardian(){
		Citizen kingOfTarget = this._targetCitizen.city.kingdom.king;
		int value = 0;
		for(int i = 0; i < this.otherKingdoms.Count; i++){
			if(this.otherKingdoms[i].king.behaviorTraits.Contains(BEHAVIOR_TRAIT.NAIVE)){
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
			this.guardians.Add (guardian);

			if(otherKingdom.king.id == this._targetCitizen.id){
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Assassination", "guardian_same");
				newLog.AddToFillers (otherKingdom.king, otherKingdom.king.name);
				newLog.AddToFillers (otherKingdom, otherKingdom.name);
				newLog.AddToFillers (guardian, guardian.name);
			}else{
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Assassination", "guardian_different");
				newLog.AddToFillers (otherKingdom.king, otherKingdom.king.name);
				newLog.AddToFillers (otherKingdom, otherKingdom.name);
				newLog.AddToFillers (guardian, guardian.name);
				newLog.AddToFillers (this._targetCitizen, this._targetCitizen.name);
			}

		}
	}
	private void AssassinationMoment(){
		if(this.spy == null){
			Debug.Log ("CAN'T ASSASSINATE NO SPIES AVAILABLE");
			return;
		}
		if(this.spy.isDead){
			Debug.Log ("CAN'T ASSASSINATE, SPY IS DEAD!");
			return;
		}
		int chance = UnityEngine.Random.Range (0, 100);

		bool hasBeenDiscovered = false;
		bool hasDeflected = false;
		bool hasAssassinated = false;
		bool hasSpyDied = false;
		Kingdom kingdomToBlame = null;

		this.successRate -= GetGuardianReduction ();
		if(chance < this.successRate){
			AssassinateTarget (ref hasAssassinated);		
		}

		SpyDiscovery (ref hasBeenDiscovered, ref hasDeflected, ref kingdomToBlame);
		if (hasAssassinated && hasBeenDiscovered) {
			if (hasDeflected) { //success_discovery_deflect
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Assassination", "success_discovery_deflect");
				newLog.AddToFillers (this.spy, this.spy.name);
				newLog.AddToFillers (this._targetCitizen, this._targetCitizen.name);
				newLog.AddToFillers (kingdomToBlame, kingdomToBlame.name);
				newLog.AddToFillers (this._targetCitizen.city.kingdom.king, this._targetCitizen.city.kingdom.king.name);
				newLog.AddToFillers (kingdomToBlame.king, kingdomToBlame.king.name);
			} else { //success_discovery
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Assassination", "success_discovery");
				newLog.AddToFillers (this.spy, this.spy.name);
				newLog.AddToFillers (this._targetCitizen, this._targetCitizen.name);
				newLog.AddToFillers (this._targetCitizen.city.kingdom.king, this._targetCitizen.city.kingdom.king.name);
				newLog.AddToFillers (this.assassinKingdom.king, this.assassinKingdom.king.name);
			}

		} else if (!hasAssassinated && hasBeenDiscovered) {
			if (hasDeflected) { //fail_discovery_deflect
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Assassination", "fail_discovery_deflect");
				newLog.AddToFillers (this.spy, this.spy.name);
				newLog.AddToFillers (this._targetCitizen, this._targetCitizen.name);
				newLog.AddToFillers (kingdomToBlame, kingdomToBlame.name);
				newLog.AddToFillers (this._targetCitizen.city.kingdom.king, this._targetCitizen.city.kingdom.king.name);
				newLog.AddToFillers (kingdomToBlame.king, kingdomToBlame.king.name);
			} else { //fail_discovery
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Assassination", "fail_discovery");
				newLog.AddToFillers (this.spy, this.spy.name);
				newLog.AddToFillers (this._targetCitizen, this._targetCitizen.name);
				newLog.AddToFillers (this._targetCitizen.city.kingdom.king, this._targetCitizen.city.kingdom.king.name);
				newLog.AddToFillers (this.assassinKingdom.king, this.assassinKingdom.king.name);
			}
		} else if (hasAssassinated && !hasBeenDiscovered) { //success
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Assassination", "success");
			newLog.AddToFillers (this.spy, this.spy.name);
			newLog.AddToFillers (this._targetCitizen, this._targetCitizen.name);
		} else if (!hasAssassinated && !hasBeenDiscovered) { //fail
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Assassination", "fail");
			newLog.AddToFillers (this.spy, this.spy.name);
			newLog.AddToFillers (this._targetCitizen, this._targetCitizen.name);
		}

		if(!hasAssassinated){
			int dieSpy = UnityEngine.Random.Range (0, 100);
			if(dieSpy < 5){
				hasSpyDied = true;
			}
		}
		if(hasSpyDied){
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Assassination", "spy_died");
			newLog.AddToFillers (this.spy, this.spy.name);
			this.spy.Death (DEATH_REASONS.TREACHERY);
			Debug.Log (this.spy.name + " HAS DIED!");
		}else{
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Assassination", "spy_alive");
			newLog.AddToFillers (this.spy, this.spy.name);
		}
//		if (hasAssassinated && hasBeenDiscovered) {
//			if (hasDeflected) {
//				this.spy.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.spy.name + " was successful in assassinating " + this._targetCitizen.name + ". "
//					+ this.spy.name + "'s actions were discovered but he/she successfully deflected the blame to " + kingToBlame.name + ". " + this.spy.name + " survived.", HISTORY_IDENTIFIER.NONE));
//			} else {
//				this.spy.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.spy.name + " was successful in assassinating " + this._targetCitizen.name + ". "
//					+ this.spy.name + "'s actions were discovered. " + this.spy.name + " survived.", HISTORY_IDENTIFIER.NONE));
//			}
//
//		} else if (!hasAssassinated && hasBeenDiscovered) {
//			if (hasDeflected) {
//				this.spy.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.spy.name + " was unsuccessful in assassinating " + this._targetCitizen.name + ". "
//					+ this.spy.name + "'s actions were discovered but he/she successfully deflected the blame to " + kingToBlame.name + ". " + this.spy.name + " survived.", HISTORY_IDENTIFIER.NONE));
//			} else {
//				this.spy.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.spy.name + " was unsuccessful in assassinating " + this._targetCitizen.name + ". "
//					+ this.spy.name + "'s actions were discovered. " + this.spy.name + " survived.", HISTORY_IDENTIFIER.NONE));
//			}
//		} else if (hasAssassinated && !hasBeenDiscovered) {
//			this.spy.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.spy.name + " was successful in assassinating " + this._targetCitizen.name + ". "
//				+ this.spy.name + "'s actions were not discovered. " + this.spy.name + " survived.", HISTORY_IDENTIFIER.NONE));
//		} else if (!hasAssassinated && !hasBeenDiscovered) {
//			this.spy.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.spy.name + " was unsuccessful in assassinating " + this._targetCitizen.name + ". "
//				+ this.spy.name + "'s actions were not discovered. " + this.spy.name + " survived.", HISTORY_IDENTIFIER.NONE));
//		}
	}

	private int GetActualChancePercentage(Citizen spy){
		int value = 35;
		if(this._targetCitizen.isKing){
			value = 25;
		}else if(this._targetCitizen.isGovernor){
			value = 30;
		}
		return value;
	}

	//Gets the percentage that the assassination success rate will reduce
	private int GetGuardianReduction(){
		return this.guardians.Count * 15;
	}
	private void AssassinateTarget(ref bool hasAssassinated){
		if(!this._targetCitizen.isDead){
			this._targetCitizen.Death (DEATH_REASONS.ASSASSINATION);
			hasAssassinated = true;
		}
		hasAssassinated = false;
	}
	private void SpyDiscovery(ref bool hasBeenDiscovered, ref bool hasDeflected, ref Kingdom kingdomToBlame){
		int chance = UnityEngine.Random.Range (0, 100);
		int value = 20;
		if(this.spy.skillTraits.Contains(SKILL_TRAIT.STEALTHY)){
			value -= 5;
		}

		if(chance < value){
			hasBeenDiscovered = true;
			if(this.assassinKingdom.king.behaviorTraits.Contains(BEHAVIOR_TRAIT.SCHEMING)){
				int deflectChance = UnityEngine.Random.Range (0, 100);
				if(deflectChance < 35){
					Kingdom kingdomToBlameCopy = GetRandomKingdomToBlame ();
					if(kingdomToBlame != null){
						hasDeflected = true;
						kingdomToBlame = kingdomToBlameCopy;
						RelationshipKings relationship = this._targetCitizen.city.kingdom.king.SearchRelationshipByID (kingdomToBlame.king.id);
						relationship.AdjustLikeness (-15, this);
						relationship.relationshipHistory.Add (new History (
							GameManager.Instance.month,
							GameManager.Instance.days,
							GameManager.Instance.year,
							this._targetCitizen.city.kingdom.king.name +  " caught an assassin, that was from " + kingdomToBlame.name,
							HISTORY_IDENTIFIER.KING_RELATIONS,
							false
						));
					}else{
						RelationshipKings relationship = this._targetCitizen.city.kingdom.king.SearchRelationshipByID (assassinKingdom.king.id);
						relationship.AdjustLikeness (-15, this);
						relationship.relationshipHistory.Add (new History (
							GameManager.Instance.month,
							GameManager.Instance.days,
							GameManager.Instance.year,
							this._targetCitizen.city.kingdom.king.name +  " caught an assassin, that was from " + assassinKingdom.name,
							HISTORY_IDENTIFIER.KING_RELATIONS,
							false
						));
					}
				}else{
					RelationshipKings relationship = this._targetCitizen.city.kingdom.king.SearchRelationshipByID (assassinKingdom.king.id);
					relationship.AdjustLikeness (-15, this);
					relationship.relationshipHistory.Add (new History (
						GameManager.Instance.month,
						GameManager.Instance.days,
						GameManager.Instance.year,
						this._targetCitizen.city.kingdom.king.name +  " caught an assassin, that was from " + assassinKingdom.name,
						HISTORY_IDENTIFIER.KING_RELATIONS,
						false
					));
				}
			}else{
				RelationshipKings relationship = this._targetCitizen.city.kingdom.king.SearchRelationshipByID (assassinKingdom.king.id);
				relationship.AdjustLikeness (-15, this);
				relationship.relationshipHistory.Add (new History (
					GameManager.Instance.month,
					GameManager.Instance.days,
					GameManager.Instance.year,
					this._targetCitizen.city.kingdom.king.name +  " caught an assassin, that was from " + assassinKingdom.name,
					HISTORY_IDENTIFIER.KING_RELATIONS,
					false
				));
			}
		}
	}
	private Kingdom GetRandomKingdomToBlame(){
		List<Kingdom> otherAdjacentKingdoms = new List<Kingdom> ();
		for(int i = 0; i < this.otherKingdoms.Count; i++){
			RelationshipKingdom relationship = assassinKingdom.GetRelationshipWithOtherKingdom (this.otherKingdoms [i]);
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
}
