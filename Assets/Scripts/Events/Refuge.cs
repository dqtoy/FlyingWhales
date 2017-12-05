using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Refuge : GameEvent {

	private enum GOVERNOR_DECISION{
		ACCEPT,
		REJECT,
		KILL,
	}

	public Refugee refugee;
	public Kingdom refugeeKingdom;
	public HexTile previousLocation;
	public City startingCity;

	public Refuge(int startDay, int startMonth, int startYear, Citizen startedBy, City startingCity) : base (startDay, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.REFUGE;
		this.name = "Refuge";
		this.refugee = (Refugee)startedBy.assignedRole;
		this.refugeeKingdom = startedBy.city.kingdom;
		this.previousLocation = startedBy.assignedRole.location;
		this.startingCity = startingCity;

		GameDate newDate = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
		newDate.AddDays (3);
		SchedulingManager.Instance.AddEntry (newDate, () => DecayPopulation ());
	}

	internal void Initialize(){
		FindNewPath ();
	}
	#region Overrides
	internal override void DoneCitizenAction(Citizen citizen){
		if(this.refugee.citizen.id == citizen.id){
			if(this.refugee.location.city == null || (this.refugee.location.city != null && this.refugee.location.city.id == this.startingCity.id)){
				FindNewPath ();
			}else{
				GovernorDecision ();
			}
		}
	}
	internal override void DoneEvent(){
		base.DoneEvent();
		this.refugee.citizen.Death (DEATH_REASONS.BATTLE);
	}
	internal override void CancelEvent (){
		FindNewPath ();
	}
	#endregion

	private void DecayPopulation(){
		if(this.isActive){
			this.refugee.AdjustPopulation (-1);
			GameDate newDate = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
			newDate.AddDays (3);
			SchedulingManager.Instance.AddEntry (newDate, () => DecayPopulation ());
		}
	}
	private void FindNewPath(){
		HexTile newTargetLocation = null;
		if(previousLocation == null){
			newTargetLocation = this.refugee.location.allNeighbourRoads [UnityEngine.Random.Range (0, this.refugee.location.allNeighbourRoads.Count)];
		}else{
			List<HexTile> allNeighbors = new List<HexTile>(this.refugee.location.allNeighbourRoads);
			if(allNeighbors.Count > 1){
				allNeighbors.Remove (this.previousLocation);
			}
			newTargetLocation = allNeighbors [UnityEngine.Random.Range (0, allNeighbors.Count)];
		}

		this.previousLocation = this.refugee.location;

		if(newTargetLocation != null){
			this.refugee.targetLocation = newTargetLocation;
			this.refugee.targetCity = newTargetLocation.city;
			this.refugee.citizenAvatar.SetHasArrivedState (false);
			this.refugee.path.Clear();
			this.refugee.path.Add (newTargetLocation);
			this.refugee.citizenAvatar.StartMoving ();
		}else{
			DoneEvent ();
		}
	}
	private void GovernorDecision(){
		Dictionary<GOVERNOR_DECISION, int> decisionDict = new Dictionary<GOVERNOR_DECISION, int> ();
		Citizen governor = this.refugee.location.city.governor;

		decisionDict.Add (GOVERNOR_DECISION.ACCEPT, GetDecisionWeight (GOVERNOR_DECISION.ACCEPT, governor));
		decisionDict.Add (GOVERNOR_DECISION.REJECT, GetDecisionWeight (GOVERNOR_DECISION.REJECT, governor));
		decisionDict.Add (GOVERNOR_DECISION.KILL, GetDecisionWeight (GOVERNOR_DECISION.KILL, governor));

		GOVERNOR_DECISION finalDecision = Utilities.PickRandomElementWithWeights<GOVERNOR_DECISION> (decisionDict);
		GovernorDecisionResult (finalDecision, governor);
	}

	private int GetDecisionWeight(GOVERNOR_DECISION decision, Citizen governor){
		int totalWeight = 0;
		if(decision == GOVERNOR_DECISION.ACCEPT){
			if(governor.otherTraits.Contains(TRAIT.BENEVOLENT)){
				totalWeight += 200;
			}
			if(governor.city.kingdom.id == this.refugeeKingdom.id){
				totalWeight += 300;
			}else{
				KingdomRelationship kr = this.refugeeKingdom.GetRelationshipWithKingdom (governor.city.kingdom);
				if(kr.AreAllies()){
					totalWeight += 300;
				}else{
					if(!kr.sharedRelationship.isAtWar){
						totalWeight += 200;
					}else{
						totalWeight += 50;
					}
				}
			}
		}else if(decision == GOVERNOR_DECISION.REJECT){
			if(governor.otherTraits.Contains(TRAIT.RUTHLESS)){
				totalWeight += 100;
			}
			if(governor.otherTraits.Contains(TRAIT.HOSTILE)){
				totalWeight += 100;
			}
			if(governor.city.kingdom.id == this.refugeeKingdom.id){
				totalWeight += 50;
			}else{
				KingdomRelationship kr = this.refugeeKingdom.GetRelationshipWithKingdom (governor.city.kingdom);
				if(kr.AreAllies()){
					totalWeight += 100;
				}else{
					if(!kr.sharedRelationship.isAtWar){
						totalWeight += 200;
					}else{
						totalWeight += 300;
					}
				}
			}
		}else if(decision == GOVERNOR_DECISION.KILL){
			if(this.refugeeKingdom.id != governor.city.kingdom.id){
				KingdomRelationship kr = this.refugeeKingdom.GetRelationshipWithKingdom (governor.city.kingdom);
				if(kr.sharedRelationship.isAtWar){
					totalWeight += 200;
				}	
			}
		}
		return totalWeight;
	}

	private void GovernorDecisionResult(GOVERNOR_DECISION decision, Citizen governor){
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Refuge", decision.ToString().ToLower());
		newLog.AddToFillers (governor, governor.name, LOG_IDENTIFIER.GOVERNOR_1);
		newLog.AddToFillers (governor.city, governor.city.name, LOG_IDENTIFIER.CITY_1);
		UIManager.Instance.ShowNotification (newLog);

		if(decision == GOVERNOR_DECISION.ACCEPT){
			governor.city.AdjustPopulation (this.refugee.population);
			DoneEvent ();
		}else if(decision == GOVERNOR_DECISION.REJECT){
			FindNewPath ();
		}else{
			this.refugee.AdjustPopulation (-this.refugee.population);
			DoneEvent ();
		}


	}
}
