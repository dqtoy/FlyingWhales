using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BorderConflict : GameEvent {

	public Kingdom kingdom1;
	public Kingdom kingdom2;
	public List<Kingdom> otherKingdoms;
	public List<Envoy> activeEnvoysReduceSelf;
	public List<Envoy> activeEnvoysReduce;
	public List<Envoy> activeEnvoysIncrease;

	public int tension;

	public BorderConflict(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom kingdom1, Kingdom kingdom2) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.BORDER_CONFLICT;
		if(startedBy != null){
			this.description = startedBy.name + " has created a border conflict between " + kingdom1.name + " and " + kingdom2.name + ".";
			this.startedBy.city.cityHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.week, GameManager.Instance.year, 
				startedBy.name + " has created a border conflict between " + kingdom1.name + " and " + kingdom2.name + "." , HISTORY_IDENTIFIER.NONE));
		}else{
			this.description = "A border conflict has began between " + kingdom1.name + " and " + kingdom2.name + ".";
		}
		this.durationInWeeks = 0;
		this.remainingWeeks = this.durationInWeeks;
		this.tension = 20;
		this.kingdom1 = kingdom1;
		this.kingdom2 = kingdom2;
		this.activeEnvoysReduceSelf = new List<Envoy> ();
		this.activeEnvoysReduce = new List<Envoy> ();
		this.activeEnvoysIncrease = new List<Envoy> ();
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
	internal override void DoneCitizenAction(Envoy envoy){
		if(!envoy.citizen.isDead){
			//Search for envoys task first on activeenvoys
			//Do something here add tension or reduce depending on the envoys task
			if(!SearchEnvoyWhere(this.activeEnvoysReduceSelf, envoy)){
				if(!SearchEnvoyWhere(this.activeEnvoysReduce, envoy)){
					if(!SearchEnvoyWhere(this.activeEnvoysIncrease, envoy)){
						Debug.Log ("CAN'T FIND ENVOY!");
					}else{
						int tensionAmount = 15;
						if(envoy.citizen.skillTraits.Contains(SKILL_TRAIT.PERSUASIVE)){
							tensionAmount += 4;
						}
						AdjustTension (tensionAmount);
					}
				}else{
					int tensionAmount = 15;
					if(envoy.citizen.skillTraits.Contains(SKILL_TRAIT.PERSUASIVE)){
						tensionAmount += 4;
					}
					AdjustTension (-tensionAmount);
				}
			}else{
				int tensionAmount = 25;
				if(envoy.citizen.skillTraits.Contains(SKILL_TRAIT.PERSUASIVE)){
					tensionAmount += 5;
				}
				AdjustTension (-tensionAmount);
			}
		}
		this.activeEnvoysReduce.Remove (envoy);
	}
	internal bool SearchEnvoyWhere(List<Envoy> whereToSearch, Envoy envoy){
		for(int i = 0; i < whereToSearch.Count; i++){
			if(envoy.citizen.id == whereToSearch[i].citizen.id){
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

			relationship1.AdjustLikeness (-15, EVENT_TYPES.BORDER_CONFLICT);
			relationship2.AdjustLikeness (-15, EVENT_TYPES.BORDER_CONFLICT);

			relationship1.relationshipHistory.Add (new History (
				GameManager.Instance.month,
				GameManager.Instance.week,
				GameManager.Instance.year,
				" A border conflict between " + this.kingdom1.name +  " " + this.kingdom2.name + " reached full tension.",
				HISTORY_IDENTIFIER.KING_RELATIONS,
				false
			));
			relationship2.relationshipHistory.Add (new History (
				GameManager.Instance.month,
				GameManager.Instance.week,
				GameManager.Instance.year,
				" A border conflict between " + this.kingdom2.name +  " " + this.kingdom1.name + " reached full tension.",
				HISTORY_IDENTIFIER.KING_RELATIONS,
				false
			));

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
		RelationshipKings relationship1 = otherKingdom.king.SearchRelationshipByID (this.kingdom1.king.id);
		RelationshipKings relationship2 = otherKingdom.king.SearchRelationshipByID (this.kingdom2.king.id);

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
		Citizen chosenCitizen = GetEnvoy (sender);
		if(chosenCitizen == null){
			return;
		}
		Envoy chosenEnvoy = null;
		if (chosenCitizen.assignedRole is Envoy) {
			chosenEnvoy = chosenCitizen.assignedRole as Envoy;
		}
		if (chosenEnvoy == null) {
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
			chosenEnvoy.eventDuration = 2;
			chosenEnvoy.currentEvent = this;
			chosenEnvoy.inAction = true;
			EventManager.Instance.onWeekEnd.AddListener (chosenEnvoy.WeeklyAction);

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
			DoneEvent ();
		}
	}

	internal override void DoneEvent(){
		for(int i = 0; i < this.activeEnvoysIncrease.Count; i++){
			this.activeEnvoysIncrease[i].inAction = false;
		}
		for(int i = 0; i < this.activeEnvoysReduce.Count; i++){
			this.activeEnvoysReduce[i].inAction = false;
		}
		for(int i = 0; i < this.activeEnvoysReduceSelf.Count; i++){
			this.activeEnvoysReduceSelf[i].inAction = false;
		}

		EventManager.Instance.onWeekEnd.RemoveListener (this.PerformAction);
		this.isActive = false;
		EventManager.Instance.onGameEventEnded.Invoke(this);
		this.endWeek = GameManager.Instance.week;
		this.endMonth = GameManager.Instance.month;
		this.endYear = GameManager.Instance.year;
		string result = "deterioration";
		if(this.tension <= 0){
			result = "improvement";
		}
		this.resolution = "Ended on " + ((MONTH)this.endMonth).ToString() + " " + this.endWeek + ", " + this.endYear + ". Tension reached "
			+ this.tension + " and caused " + result + " in relationship.";
//		EventManager.Instance.allEvents [EVENT_TYPES.BORDER_CONFLICT].Remove (this);

		//Remove UI Icon
	}
}
