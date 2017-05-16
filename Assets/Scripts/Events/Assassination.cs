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
		this.durationInDays = 4;
		this.remainingDays = this.durationInDays;
		this.assassinKingdom = startedBy.city.kingdom;
		this._targetCitizen = targetCitizen;
		this.otherKingdoms = GetOtherKingdoms ();
		this.guardians = new List<Citizen>();
		this.uncovered = new List<Citizen>();
		this.spy = spy;
		this.successRate = GetActualChancePercentage (this.spy);

		string description;
		if (gameEventTrigger is Assassination) {
			this.description = startedBy.name + " sent " + this.spy.name + " to kill " + targetCitizen.name + " after discovering that " + gameEventTrigger.startedBy.name
							+ " also sent an assassin to kill " + (gameEventTrigger as Assassination).targetCitizen.name;
			startedBy.history.Add(new History(startMonth, startWeek, startYear, this.description, HISTORY_IDENTIFIER.NONE));

		} else if (gameEventTrigger is InvasionPlan) {
			this.description = startedBy.name + " sent " + this.spy.name + " to kill " + targetCitizen.name + " after discovering " + gameEventTrigger.startedBy.name + "'s "
				+ " Invasion Plan against " + (gameEventTrigger as InvasionPlan).targetKingdom.name;
			startedBy.history.Add(new History(startMonth, startWeek, startYear, this.description, HISTORY_IDENTIFIER.NONE));

		} else if (gameEventTrigger is StateVisit){
			this.description = startedBy.name + " sent " + this.spy.name + " to kill " + targetCitizen.name + " while on a State Visit to " + targetCitizen.city.kingdom.name + ".";
			startedBy.history.Add(new History(startMonth, startWeek, startYear, this.description, HISTORY_IDENTIFIER.NONE));

		} else if (gameEventTrigger is BorderConflict){
			this.description = startedBy.name + " sent " + this.spy.name + " to kill " + targetCitizen.name + " in response to worsening Border Conflict.";
			startedBy.history.Add(new History(startMonth, startWeek, startYear, this.description, HISTORY_IDENTIFIER.NONE));

		} else if (gameEventTrigger is DiplomaticCrisis){
			this.description = startedBy.name + " sent " + this.spy.name + " to kill " + targetCitizen.name + " in the aftermath of a recent Diplomatic Crisis.";
			startedBy.history.Add(new History(startMonth, startWeek, startYear, this.description, HISTORY_IDENTIFIER.NONE));

		} else if (gameEventTrigger is Espionage){
			this.description = startedBy.name + " sent " + this.spy.name + " to kill " + targetCitizen.name + " after finding out that " + gameEventTrigger.startedBy.name + " spied on " + (gameEventTrigger as Espionage).targetKingdom.name + ".";
			startedBy.history.Add(new History(startMonth, startWeek, startYear, this.description, HISTORY_IDENTIFIER.NONE));

		} else if (gameEventTrigger is Raid){
			this.description = startedBy.name + " sent " + this.spy.name + " to kill " + targetCitizen.name + " after the raid of " + (gameEventTrigger as Raid).raidedCity.name + ".";
			startedBy.history.Add(new History(startMonth, startWeek, startYear, this.description, HISTORY_IDENTIFIER.NONE));

		} else {
			this.description = startedBy.name + " sent " + this.spy.name + " to kill " + targetCitizen.name + ".";
			startedBy.history.Add(new History(startMonth, startWeek, startYear, this.description, HISTORY_IDENTIFIER.NONE));

		} 
		/*
		else {
			startedBy.history.Add(new History(startMonth, startWeek, startYear, startedBy.name + " sent " + this.spy.name + " to kill " + targetCitizen.name + " after relationship deterioration due to " + triggerReason.ToString() + ".", HISTORY_IDENTIFIER.NONE));
			this.description = startedBy.name + " of " + startedBy.city.kingdom.name + " wants to assassinate " + targetCitizen.name + " of " + targetCitizen.city.kingdom.name + " after relationship deterioration due to " + triggerReason.ToString() + ".";

		}
		*/
		TriggerGuardian ();
		EventManager.Instance.AddEventToDictionary(this);
		this.targetCitizen.city.hexTile.AddEventOnTile(this);
		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
	}

	internal override void PerformAction(){
		this.remainingDays -= 1;
		if(this.remainingDays <= 0){
			this.remainingDays = 0;
			AssassinationMoment ();
			DoneEvent ();
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
		for(int i = 0; i < this.otherKingdoms.Count; i++){
			if(_targetCitizen.isKing){
				RelationshipKings relationship = this.otherKingdoms [i].king.SearchRelationshipByID (_targetCitizen.id);
				if(relationship.lordRelationship == RELATIONSHIP_STATUS.FRIEND){
					int chance = UnityEngine.Random.Range (0, 100);
					if(chance < 25){
						AssignGuardian (this.otherKingdoms [i]);
					}
				}else if(relationship.lordRelationship == RELATIONSHIP_STATUS.ALLY){
					int chance = UnityEngine.Random.Range (0, 100);
					if(chance < 50){
						AssignGuardian (this.otherKingdoms [i]);
					}
				}
			}else{
				if(this.otherKingdoms[i].king.supportedCitizen == null){
					if(_targetCitizen.isHeir){
						int chance = UnityEngine.Random.Range (0, 100);
						if(chance < 25){
							AssignGuardian (this.otherKingdoms [i]);
						}
					}
				}else{
					if(this.otherKingdoms[i].king.supportedCitizen.id == _targetCitizen.id){
						int chance = UnityEngine.Random.Range (0, 100);
						if(chance < 25){
							AssignGuardian (this.otherKingdoms [i]);
						}
					}
				}
			}
		}
	}
	private void AssignGuardian(Kingdom otherKingdom){
		Citizen guardian = GetGuardian (otherKingdom);
		if (guardian != null) {
			((Guardian)guardian.assignedRole).inAction = true;
			this.guardians.Add (guardian);
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
		Citizen kingToBlame = null;
		if(chance < this.successRate){
			AssassinateTarget (ref hasAssassinated);
			SpyDiscovery (ref hasBeenDiscovered, ref hasDeflected, ref kingToBlame);
			if (hasAssassinated && hasBeenDiscovered) {
				if (hasDeflected) {
					this.spy.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.spy.name + " was successful in assassinating " + this._targetCitizen.name + ". "
					+ this.spy.name + "'s actions were discovered but he/she successfully deflected the blame to " + kingToBlame.name + ". " + this.spy.name + " survived.", HISTORY_IDENTIFIER.NONE));
				} else {
					this.spy.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.spy.name + " was successful in assassinating " + this._targetCitizen.name + ". "
					+ this.spy.name + "'s actions were discovered. " + this.spy.name + " survived.", HISTORY_IDENTIFIER.NONE));
				}

			} else if (!hasAssassinated && hasBeenDiscovered) {
				if (hasDeflected) {
					this.spy.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.spy.name + " was unsuccessful in assassinating " + this._targetCitizen.name + ". "
					+ this.spy.name + "'s actions were discovered but he/she successfully deflected the blame to " + kingToBlame.name + ". " + this.spy.name + " survived.", HISTORY_IDENTIFIER.NONE));
				} else {
					this.spy.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.spy.name + " was unsuccessful in assassinating " + this._targetCitizen.name + ". "
					+ this.spy.name + "'s actions were discovered. " + this.spy.name + " survived.", HISTORY_IDENTIFIER.NONE));
				}
			} else if (hasAssassinated && !hasBeenDiscovered) {
				this.spy.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.spy.name + " was successful in assassinating " + this._targetCitizen.name + ". "
				+ this.spy.name + "'s actions were not discovered. " + this.spy.name + " survived.", HISTORY_IDENTIFIER.NONE));
			} else if (!hasAssassinated && !hasBeenDiscovered) {
				this.spy.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.spy.name + " was unsuccessful in assassinating " + this._targetCitizen.name + ". "
					+ this.spy.name + "'s actions were not discovered. " + this.spy.name + " survived.", HISTORY_IDENTIFIER.NONE));
			}
		}else{
			SpyDiscovery (ref hasBeenDiscovered, ref hasDeflected, ref kingToBlame);
			int dieSpy = UnityEngine.Random.Range (0, 100);
			if(dieSpy < 5){
				this.spy.Death (DEATH_REASONS.TREACHERY);
				Debug.Log (this.spy.name + " HAS DIED!");

				if (hasBeenDiscovered) {
					if (hasDeflected) {
						this.spy.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.spy.name + " was unsuccessful in assassinating " + this._targetCitizen.name + ". "
							+ this.spy.name + "'s actions were discovered but he/she successfully deflected the blame to " + kingToBlame.name + ". " + this.spy.name + " died.", HISTORY_IDENTIFIER.NONE));
					} else {
						this.spy.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.spy.name + " was unsuccessful in assassinating " + this._targetCitizen.name + ". "
							+ this.spy.name + "'s actions were discovered. " + this.spy.name + " died.", HISTORY_IDENTIFIER.NONE));
					}

				}else{
					this.spy.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.spy.name + " was unsuccessful in assassinating " + this._targetCitizen.name + ". "
						+ this.spy.name + "'s actions were not discovered. " + this.spy.name + " died.", HISTORY_IDENTIFIER.NONE));
				}
			}

			if (hasBeenDiscovered) {
				if (hasDeflected) {
					this.spy.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.spy.name + " was unsuccessful in assassinating " + this._targetCitizen.name + ". "
						+ this.spy.name + "'s actions were discovered but he/she successfully deflected the blame to " + kingToBlame.name + ". " + this.spy.name + " survived.", HISTORY_IDENTIFIER.NONE));
				} else {
					this.spy.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.spy.name + " was unsuccessful in assassinating " + this._targetCitizen.name + ". "
						+ this.spy.name + "'s actions were discovered. " + this.spy.name + " survived.", HISTORY_IDENTIFIER.NONE));
				}

			}else{
				this.spy.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.spy.name + " was unsuccessful in assassinating " + this._targetCitizen.name + ". "
					+ this.spy.name + "'s actions were not discovered. " + this.spy.name + " survived.", HISTORY_IDENTIFIER.NONE));
			}
		}

	}

	private int GetActualChancePercentage(Citizen spy){
		int value = 35;
		if(_targetCitizen.isKing){
			value = 25;
		}else if(_targetCitizen.isGovernor){
			value = 30;
		}

		if(spy.skillTraits.Contains(SKILL_TRAIT.STEALTHY)){
			value += 10;
		}
//		int guardianPercent = this.guardians.Count * 15;
		int guardianPercent = 0;
		for (int i = 0; i < this.guardians.Count; i++) {
			if (this.guardians [i].skillTraits.Contains (SKILL_TRAIT.ALERT)) {
				guardianPercent += 25;
			} else {
				guardianPercent += 15;
			}
		}

		value -= guardianPercent;
		return value;
	}
	private void AssassinateTarget(ref bool hasAssassinated){
		if(!_targetCitizen.isDead){
			this._targetCitizen.Death (DEATH_REASONS.ASSASSINATION);
			hasAssassinated = true;
		}
		hasAssassinated = false;
	}
	private void SpyDiscovery(ref bool hasBeenDiscovered, ref bool hasDeflected, ref Citizen kingToBlame){
		int chance = UnityEngine.Random.Range (0, 100);
		int value = 20;
		if(this.spy.skillTraits.Contains(SKILL_TRAIT.STEALTHY)){
			value -= 5;
		}

		if(chance < value){
			hasBeenDiscovered = true;
			if(this.spy.behaviorTraits.Contains(BEHAVIOR_TRAIT.SCHEMING)){
				int deflectChance = UnityEngine.Random.Range (0, 100);
				if(deflectChance < 35){
					Kingdom kingdomToBlame = GetRandomKingdomToBlame ();
					if(kingdomToBlame != null){
						hasDeflected = true;
						kingToBlame = kingdomToBlame.king;
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
