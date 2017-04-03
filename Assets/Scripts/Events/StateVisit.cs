using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StateVisit : GameEvent {
	public Kingdom inviterKingdom;
	public Kingdom invitedKingdom;
	public Citizen visitor;
	public List<Kingdom> otherKingdoms;
	public List<Citizen> helperEnvoys;
	public List<Citizen> saboteurEnvoys;
	public int successMeter;

	public StateVisit(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom invitedKingdom, Citizen visitor) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.BORDER_CONFLICT;
		this.description = startedBy.name + " invited " + invitedKingdom.king.name + " to visit his/her kingdom.";
		this.durationInWeeks = 6;
		this.remainingWeeks = this.durationInWeeks;
		this.inviterKingdom = startedBy.city.kingdom;
		this.invitedKingdom = invitedKingdom;
		this.visitor = visitor;
		this.otherKingdoms = GetOtherKingdoms ();
		this.helperEnvoys = new List<Citizen> ();
		this.saboteurEnvoys = new List<Citizen> ();
		this.successMeter = 50;
		TriggerAssassinationEvent ();
		TriggerSabotage ();
		TriggerHelp ();
		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
	}

	internal override void PerformAction(){
		CheckVisitor ();
		if(!this.visitor.isDead){
			this.durationInWeeks -= 1;
			if(this.durationInWeeks <= 0){
				this.durationInWeeks = 0;
				CheckEndSuccessMeter ();
				DoneEvent ();
			}
		}
	}

	internal override void DoneEvent(){
		for(int i = 0; i < this.helperEnvoys.Count; i++){
			((Envoy)this.helperEnvoys[i].assignedRole).inAction = false;
		}
		for(int i = 0; i < this.saboteurEnvoys.Count; i++){
			((Envoy)this.saboteurEnvoys[i].assignedRole).inAction = false;
		}
		this.helperEnvoys.Clear ();
		this.saboteurEnvoys.Clear ();
		EventManager.Instance.onWeekEnd.RemoveListener (this.PerformAction);
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

		inviterRelationship.AdjustLikeness (inviterPoints);
		invitedRelationship.AdjustLikeness (invitedPoints);

	}

	private List<Kingdom> GetOtherKingdoms(){
		List<Kingdom> kingdoms = new List<Kingdom> ();
		for(int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++){
			if(KingdomManager.Instance.allKingdoms[i].id != this.inviterKingdom.id && KingdomManager.Instance.allKingdoms[i].id != this.invitedKingdom.id){
				kingdoms.Add (KingdomManager.Instance.allKingdoms [i]);
			}
		}
		return kingdoms;
	}

	private void TriggerAssassinationEvent(){
		for(int i = 0; i < this.otherKingdoms.Count; i++){
			RelationshipKings relationship = this.otherKingdoms [i].king.SearchRelationshipByID (inviterKingdom.king.id);
			if(relationship.lordRelationship == RELATIONSHIP_STATUS.ENEMY || relationship.lordRelationship == RELATIONSHIP_STATUS.RIVAL){
				int chance = UnityEngine.Random.Range (0, 100);
				int value = 5;
				if(this.otherKingdoms[i].king.behaviorTraits.Contains(BEHAVIOR_TRAIT.SCHEMING)){
					value = 10;
				}
				if(chance < value){
					//ASSASSINATION EVENT
					Assassination assassination = new Assassination(GameManager.Instance.week, GameManager.Instance.month, GameManager.Instance.year, this.otherKingdoms[i].king, this.visitor);
					EventManager.Instance.AddEventToDictionary(assassination);
				}
			}
		}
	}

	private void TriggerSabotage(){
		for (int i = 0; i < this.otherKingdoms.Count; i++) {
			if(CheckForRelationship(this.otherKingdoms[i], false)){
				SendEnvoy (this.otherKingdoms [i], true, true);
			}
		}
	}
	private void TriggerHelp(){
		SendEnvoy (invitedKingdom, false, false);
		for (int i = 0; i < this.otherKingdoms.Count; i++) {
			if(CheckForRelationship(this.otherKingdoms[i], true)){
				SendEnvoy (this.otherKingdoms [i], false, true);
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
	private void SendEnvoy(Kingdom sender, bool isDecreaseSuccess = false, bool isFromOthers = false){
		int chance = 0;
		if(isDecreaseSuccess){
			chance = 10;
			if(sender.king.behaviorTraits.Contains(BEHAVIOR_TRAIT.SCHEMING)){
				chance = 20;
			}
		}else{
			chance = 15;
			if(isFromOthers){
				chance = 10;
			}
		}

		Citizen chosenEnvoy = GetEnvoy (sender);
		if(chosenEnvoy == null){
			return;
		}


		int random = UnityEngine.Random.Range (0, 100);
		if(chance < random){
			int amount = 15;
			if(chosenEnvoy.skillTraits.Contains(SKILL_TRAIT.PERSUASIVE)){
				amount += 10;
			}
			if(isDecreaseSuccess){
				this.saboteurEnvoys.Add (chosenEnvoy);
				AdjustSuccessMeter (-amount);
			}else{
				this.helperEnvoys.Add (chosenEnvoy);
				AdjustSuccessMeter (amount);
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
			Debug.Log ("VISITOR DIED!");
			RelationshipKings relationship = this.invitedKingdom.king.SearchRelationshipByID (this.inviterKingdom.king.id);
			if(relationship.like <= 0){
				relationship.AdjustLikeness (-50);
			}else{
				relationship.like = -50;
				relationship.UpdateKingRelationshipStatus ();
			}
			DoneEvent ();
		}
	}
}
