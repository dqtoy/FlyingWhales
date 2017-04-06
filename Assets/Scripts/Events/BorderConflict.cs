﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BorderConflict : GameEvent {

	public Kingdom kingdom1;
	public Kingdom kingdom2;
	public List<Kingdom> otherKingdoms;
	public List<Citizen> activeEnvoysReduceSelf;
	public List<Citizen> activeEnvoysReduce;
	public List<Citizen> activeEnvoysIncrease;

	public int tension;

	public BorderConflict(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom kingdom1, Kingdom kingdom2) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.BORDER_CONFLICT;
		if(startedBy != null){
			this.description = startedBy.name + " has created a border conflict between " + kingdom1.name + " and " + kingdom2.name + ".";
		}else{
			this.description = "A border conflict has began between " + kingdom1.name + " and " + kingdom2.name + ".";
		}
		this.durationInWeeks = 0;
		this.remainingWeeks = this.durationInWeeks;
		this.tension = 20;
		this.kingdom1 = kingdom1;
		this.kingdom2 = kingdom2;
		this.activeEnvoysReduceSelf = new List<Citizen> ();
		this.activeEnvoysReduce = new List<Citizen> ();
		this.activeEnvoysIncrease = new List<Citizen> ();
		this.otherKingdoms = GetOtherKingdoms ();

		this.kingdom1.cities[0].hexTile.AddEventOnTile(this);
		this.kingdom2.cities[0].hexTile.AddEventOnTile(this);

		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		Debug.Log (this.description);
	}

	internal override void PerformAction(){
		IncreaseTensionPerWeek ();
		InvolvedKingsTensionAdjustment ();
		NotInvolvedKingsTensionAdjustment ();
		CheckIfAlreadyAtWar ();
	}
	internal override void DoneCitizenAction(Citizen envoy){
		if(!envoy.isDead){
			//Search for envoys task first on activeenvoys
			//Do something here add tension or reduce depending on the envoys task
			if(!SearchEnvoyWhere(this.activeEnvoysReduceSelf, envoy)){
				if(!SearchEnvoyWhere(this.activeEnvoysReduce, envoy)){
					if(!SearchEnvoyWhere(this.activeEnvoysIncrease, envoy)){
						Debug.Log ("CAN'T FIND ENVOY!");
					}else{
						int tensionAmount = 15;
						if(envoy.skillTraits.Contains(SKILL_TRAIT.PERSUASIVE)){
							tensionAmount += 4;
						}
						AdjustTension (tensionAmount);
					}
				}else{
					int tensionAmount = 15;
					if(envoy.skillTraits.Contains(SKILL_TRAIT.PERSUASIVE)){
						tensionAmount += 4;
					}
					AdjustTension (-tensionAmount);
				}
			}else{
				int tensionAmount = 25;
				if(envoy.skillTraits.Contains(SKILL_TRAIT.PERSUASIVE)){
					tensionAmount += 5;
				}
				AdjustTension (-tensionAmount);
			}
		}
		this.activeEnvoysReduce.Remove (envoy);
	}
	internal bool SearchEnvoyWhere(List<Citizen> whereToSearch, Citizen envoy){
		for(int i = 0; i < whereToSearch.Count; i++){
			if(envoy.id == whereToSearch[i].id){
				return true;
			}
		}
		return false;
	}
	private List<Kingdom> GetOtherKingdoms(){
		List<Kingdom> kingdoms = new List<Kingdom> ();
		for(int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++){
			if(KingdomManager.Instance.allKingdoms[i].id != this.kingdom1.id && KingdomManager.Instance.allKingdoms[i].id != this.kingdom2.id){
				kingdoms.Add (KingdomManager.Instance.allKingdoms [i]);
			}
		}
		return kingdoms;
	}
	private void IncreaseTensionPerWeek(){
		int tensionIncrease = UnityEngine.Random.Range (1, 6);
		AdjustTension (tensionIncrease);
		Debug.Log ("TENSION: "+ this.tension);

	}

	private void CheckTensionMeter(){
		if(this.tension >= 100){
			this.tension = 100;
			//Deteriorate 15 points on each
			Debug.Log("BORDER CONFLICT BETWEEN " + this.kingdom1.name + " AND " + this.kingdom2.name + " ENDED HORRIBLY! RELATIONSHIP DETERIORATED!");

			RelationshipKings relationship1 = this.kingdom1.king.SearchRelationshipByID (this.kingdom2.king.id);
			RelationshipKings relationship2 = this.kingdom2.king.SearchRelationshipByID (this.kingdom1.king.id);

			relationship1.AdjustLikeness (-15);
			relationship2.AdjustLikeness (-15);

			DoneEvent ();
		}else if(this.tension <= 0){
			this.tension = 0;
			//Conflict will end peacefully
			Debug.Log("BORDER CONFLICT BETWEEN " + this.kingdom1.name + " AND " + this.kingdom2.name + " ENDED PEACEFULLY!");

			DoneEvent ();
		}
	}

	private void InvolvedKingsTensionAdjustment(){
		if(GameManager.Instance.week % 4 == 0){
			//Send Envoys
			SendEnvoy (this.kingdom1, this.kingdom2);
			SendEnvoy (this.kingdom2, this.kingdom1);
		}

	}
	private void NotInvolvedKingsTensionAdjustment(){
		if(GameManager.Instance.week % 4 == 0){
			for(int i = 0; i < this.otherKingdoms.Count; i++){
				if(CheckForRelationship(this.otherKingdoms[i], true)){
					SendEnvoy (this.otherKingdoms [i], null, true, true);	
				}else if(CheckForRelationship(this.otherKingdoms[i], false)){
					SendEnvoy (this.otherKingdoms [i], null, true, false);	
				}
			}
		}
	}
	private bool CheckForRelationship(Kingdom otherKingdom, bool isIncrease){
		RelationshipKings relationship1 = otherKingdom.king.SearchRelationshipByID (this.kingdom1.id);
		RelationshipKings relationship2 = otherKingdom.king.SearchRelationshipByID (this.kingdom2.id);

		List<RELATIONSHIP_STATUS> statuses = new List<RELATIONSHIP_STATUS> ();
		statuses.Add (relationship1.lordRelationship);
		statuses.Add (relationship2.lordRelationship);

		if(isIncrease){
			if(statuses.Contains(RELATIONSHIP_STATUS.ENEMY) || statuses.Contains(RELATIONSHIP_STATUS.RIVAL)){
				if(!statuses.Contains(RELATIONSHIP_STATUS.FRIEND) && !statuses.Contains(RELATIONSHIP_STATUS.ALLY)){
					return true;
				}
			}
		}else{
			if(statuses.Contains(RELATIONSHIP_STATUS.FRIEND) || statuses.Contains(RELATIONSHIP_STATUS.ALLY)){
				if(!statuses.Contains(RELATIONSHIP_STATUS.ENEMY) && !statuses.Contains(RELATIONSHIP_STATUS.RIVAL)){
					return true;
				}
			}
		}

		return false;
	}
	private void AdjustTension(int amount){
		this.tension += amount;
		CheckTensionMeter ();
	}
	private void SendEnvoy(Kingdom sender, Kingdom receiver, bool isFromOthers = false, bool isIncreaseTension = false){
		int chance = 20;
		Citizen chosenEnvoy = GetEnvoy (sender);
		if(chosenEnvoy == null){
			return;
		}
		if(!isFromOthers){
			chance = 30;
			RelationshipKings relationship = sender.king.SearchRelationshipByID (receiver.king.id);
			if(relationship.lordRelationship == RELATIONSHIP_STATUS.ALLY){
				chance += 15;
			}else if(relationship.lordRelationship == RELATIONSHIP_STATUS.ALLY){
				chance += 10;
			}else if(relationship.lordRelationship == RELATIONSHIP_STATUS.ALLY){
				chance += 5;
			}
		}


		int random = UnityEngine.Random.Range (0, 100);
		if(chance < random){
			((Envoy)chosenEnvoy.assignedRole).eventDuration = 2;
			((Envoy)chosenEnvoy.assignedRole).currentEvent = this;
			((Envoy)chosenEnvoy.assignedRole).inAction = true;
			EventManager.Instance.onWeekEnd.AddListener (((Envoy)chosenEnvoy.assignedRole).WeeklyAction);

			if(!isFromOthers){
				this.activeEnvoysReduceSelf.Add (chosenEnvoy);
			}else{
				if(isIncreaseTension){
					this.activeEnvoysIncrease.Add (chosenEnvoy);
				}else{
					this.activeEnvoysReduce.Add (chosenEnvoy);
				}
			}
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
							if (!((Envoy)kingdom.cities [i].citizens [j].assignedRole).inAction) {
								envoys.Add (kingdom.cities [i].citizens [j]);
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
			DoneEvent ();
		}
	}
	internal override void DoneEvent(){
		for(int i = 0; i < this.activeEnvoysIncrease.Count; i++){
			((Envoy)this.activeEnvoysIncrease[i].assignedRole).inAction = false;
		}
		for(int i = 0; i < this.activeEnvoysReduce.Count; i++){
			((Envoy)this.activeEnvoysReduce[i].assignedRole).inAction = false;
		}
		for(int i = 0; i < this.activeEnvoysReduceSelf.Count; i++){
			((Envoy)this.activeEnvoysReduceSelf[i].assignedRole).inAction = false;
		}
		this.activeEnvoysIncrease.Clear ();
		this.activeEnvoysReduce.Clear ();
		this.activeEnvoysReduceSelf.Clear ();

		EventManager.Instance.onWeekEnd.RemoveListener (this.PerformAction);
		this.isActive = false;
		EventManager.Instance.onGameEventEnded.Invoke(this);
//		EventManager.Instance.allEvents [EVENT_TYPES.BORDER_CONFLICT].Remove (this);

		//Remove UI Icon
	}
}
