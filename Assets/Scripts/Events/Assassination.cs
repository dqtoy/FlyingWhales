using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Assassination : GameEvent {
	public Kingdom assassinKingdom;
	public Citizen targetCitizen;
	public List<Kingdom> otherKingdoms;
	public List<Citizen> guardians;
	public Citizen spy;

	public Assassination(int startWeek, int startMonth, int startYear, Citizen startedBy, Citizen targetCitizen) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.BORDER_CONFLICT;
		this.eventStatus = EVENT_STATUS.HIDDEN;
		this.description = startedBy.name + " is planning on assassinating " + targetCitizen.name + ".";
		this.durationInWeeks = 4;
		this.remainingWeeks = this.durationInWeeks;
		this.assassinKingdom = startedBy.city.kingdom;
		this.targetCitizen = targetCitizen;
		this.otherKingdoms = GetOtherKingdoms ();
		this.guardians = new List<Citizen>();
		this.spy = GetSpy (assassinKingdom);
		TriggerGuardian ();
		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
	}

	internal override void PerformAction(){
		this.durationInWeeks -= 1;
		if(this.durationInWeeks <= 0){
			this.durationInWeeks = 0;
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
		this.spy = null;
		this.guardians.Clear ();
		EventManager.Instance.onWeekEnd.RemoveListener (this.PerformAction);
		this.isActive = false;
//		EventManager.Instance.allEvents [EVENT_TYPES.ASSASSINATION].Remove (this);

	}

	private List<Kingdom> GetOtherKingdoms(){
		List<Kingdom> kingdoms = new List<Kingdom> ();
		for(int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++){
			if(KingdomManager.Instance.allKingdoms[i].id != this.assassinKingdom.id && KingdomManager.Instance.allKingdoms[i].id != this.targetCitizen.city.kingdom.id){
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
							if (!((Spy)kingdom.cities [i].citizens [j].assignedRole).inAction) {
								spies.Add (kingdom.cities [i].citizens [j]);
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
							if(!((Guardian)kingdom.cities[i].citizens[j].assignedRole).inAction){
								guardian.Add (kingdom.cities [i].citizens [j]);
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
			if(targetCitizen.isKing){
				RelationshipKings relationship = this.otherKingdoms [i].king.SearchRelationshipByID (targetCitizen.id);
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
					if(targetCitizen.isHeir){
						int chance = UnityEngine.Random.Range (0, 100);
						if(chance < 25){
							AssignGuardian (this.otherKingdoms [i]);
						}
					}
				}else{
					if(this.otherKingdoms[i].king.supportedCitizen.id == targetCitizen.id){
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
		((Guardian)guardian.assignedRole).inAction = true;
		this.guardians.Add (guardian);
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
		int value = GetActualChancePercentage (this.spy);

		if(chance < value){
			AssassinateTarget ();
			SpyDiscovery ();
		}else{
			SpyDiscovery ();
			int dieSpy = UnityEngine.Random.Range (0, 100);
			if(dieSpy < 5){
				this.spy.Death ();
				Debug.Log (this.spy.name + " HAS DIED!");
			}
		}

	}

	private int GetActualChancePercentage(Citizen spy){
		int value = 35;
		if(targetCitizen.isKing){
			value = 25;
		}else if(targetCitizen.isGovernor){
			value = 30;
		}

		if(spy.skillTraits.Contains(SKILL_TRAIT.STEALTHY)){
			value += 10;
		}
		int guardianPercent = this.guardians.Count * 15;

		value -= guardianPercent;
		return value;
	}
	private void AssassinateTarget(){
		if(!targetCitizen.isDead){
			this.targetCitizen.Death ();
		}
	}
	private void SpyDiscovery(){
		int chance = UnityEngine.Random.Range (0, 100);
		int value = 20;
		if(this.spy.skillTraits.Contains(SKILL_TRAIT.STEALTHY)){
			value -= 5;
		}

		if(chance < value){
			if(this.spy.behaviorTraits.Contains(BEHAVIOR_TRAIT.SCHEMING)){
				int deflectChance = UnityEngine.Random.Range (0, 100);
				if(deflectChance < 35){
					Kingdom kingdomToBlame = GetRandomKingdomToBlame ();
					if(kingdomToBlame != null){
						RelationshipKings relationship = this.targetCitizen.city.kingdom.king.SearchRelationshipByID (kingdomToBlame.king.id);
						relationship.AdjustLikeness (-15);
					}else{
						RelationshipKings relationship = this.targetCitizen.city.kingdom.king.SearchRelationshipByID (assassinKingdom.king.id);
						relationship.AdjustLikeness (-15);
					}
				}else{
					RelationshipKings relationship = this.targetCitizen.city.kingdom.king.SearchRelationshipByID (assassinKingdom.king.id);
					relationship.AdjustLikeness (-15);
				}
			}else{
				RelationshipKings relationship = this.targetCitizen.city.kingdom.king.SearchRelationshipByID (assassinKingdom.king.id);
				relationship.AdjustLikeness (-15);
			}
		}
	}
	private Kingdom GetRandomKingdomToBlame(){
		List<Kingdom> otherAdjacentKingdoms = new List<Kingdom> ();
		for(int i = 0; i < this.otherKingdoms.Count; i++){
			Relationship<Kingdom> relationship = assassinKingdom.GetRelationshipWithOtherKingdom (this.otherKingdoms [i]);
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
