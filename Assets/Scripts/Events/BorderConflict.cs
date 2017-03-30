using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BorderConflict : GameEvent {

	public Kingdom kingdom1;
	public Kingdom kingdom2;
	public List<Kingdom> otherKingdoms;
	public List<Citizen> activeEnvoysReduce;
	public List<Citizen> activeEnvoysIncrease;

	public int tension;

	public BorderConflict(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom kingdom1, Kingdom kingdom2) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.BORDER_CONFLICT;
		this.description = startedBy.name + " is looking for a suitable wife as the vessel of his heir";
		this.durationInWeeks = 8;
		this.remainingWeeks = this.durationInWeeks;
		this.tension = 20;
		this.kingdom1 = kingdom1;
		this.kingdom2 = kingdom2;
		this.otherKingdoms = GetOtherKingdoms ();
		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
	}

	internal override void PerformAction(){
		
	}
	internal override void DoneCitizenAction(Citizen envoy){
		if(!envoy.isDead){
			
			//Search for envoys task first on activeenvoys
			//Do something here add tension or reduce depending on the envoys task
		}
		this.activeEnvoysReduce.Remove (envoy);
	}
//	internal void
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
	}

	private void CheckTensionMeter(){
		if(this.tension >= 100){
			this.tension = 100;
			//Deteriorate 15 points on each
		}else if(this.tension <= 0){
			this.tension = 0;
			//Conflict will end peacefully
		}
	}

	private void InvolvedKingsTensionAdjustment(Kingdom kingdom){
		int random = UnityEngine.Random.Range (0, 100);
		this.kingdom1.king.SearchRelationshipByID (this.kingdom2.king.id);
	}

	private void AdjustTension(int amount){
		this.tension += amount;
		CheckTensionMeter ();
	}
	private void SendEnvoy(Kingdom sender, Kingdom receiver){
		int chance = 30;
		Citizen chosenEnvoy = GetEnvoy (sender);
		RelationshipKings relationship = sender.king.SearchRelationshipByID (receiver.king.id);

		if(relationship.lordRelationship == RELATIONSHIP_STATUS.ALLY){
			chance += 15;
		}else if(relationship.lordRelationship == RELATIONSHIP_STATUS.ALLY){
			chance += 10;
		}else if(relationship.lordRelationship == RELATIONSHIP_STATUS.ALLY){
			chance += 5;
		}

		int random = UnityEngine.Random.Range (0, 100);
		if(chance < random){
			((Envoy)chosenEnvoy.assignedRole).eventDuration = 2;
			((Envoy)chosenEnvoy.assignedRole).currentEvent = this;
			EventManager.Instance.onWeekEnd.AddListener (((Envoy)chosenEnvoy.assignedRole).WeeklyAction);
			this.activeEnvoysReduce.Add (chosenEnvoy);
		}
		if(chosenEnvoy.skillTraits.Contains(SKILL_TRAIT.PERSUASIVE)){
			
		}
	}
	private Citizen GetEnvoy(Kingdom kingdom){
		List<Citizen> unwantedGovernors = GetUnwantedGovernors (kingdom.king);
		List<Citizen> envoys = new List<Citizen> ();
		for(int i = 0; i < kingdom.cities.Count; i++){
			if(!IsItThisGovernor(kingdom.cities[i].governor, unwantedGovernors)){
				for(int j = 0; j < kingdom.cities[i].citizens.Count; j++){
					if(kingdom.cities[i].citizens[j].assignedRole != null && kingdom.cities[i].citizens[j].role == ROLE.ENVOY){
						if(!((Envoy)kingdom.cities[i].citizens[j].assignedRole).inAction){
							envoys.Add (kingdom.cities [i].citizens [j]);
						}
					}
				}
			}
		}

		if(envoys.Count > 0){
			return envoys [UnityEngine.Random.Range (0, envoys.Count)];
		}else{
			Debug.Log (kingdom.king.name + " CAN'T SENT ENVOY BECAUSE THERE IS NONE!");
			return null;
		}
	}
	private bool IsItThisGovernor(Citizen governor, List<Citizen> unwantedGovernors){
		for(int i = 0; i < unwantedGovernors.Count; i++){
			if(governor.id == unwantedGovernors[i].id){
				return true;
			}	
		}
		return false;
	}
	private List<Citizen> GetUnwantedGovernors(Citizen king){
		List<Citizen> unwantedGovernors = new List<Citizen> ();
		for(int i = 0; i < king.civilWars.Count; i++){
			if(king.civilWars[i].isGovernor){
				unwantedGovernors.Add (king.civilWars [i]);
			}
		}
		for(int i = 0; i < king.successionWars.Count; i++){
			if(king.successionWars[i].isGovernor){
				unwantedGovernors.Add (king.successionWars [i]);
			}
		}

		return unwantedGovernors;
	}

}
