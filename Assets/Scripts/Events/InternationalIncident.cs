using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InternationalIncident : GameEvent {

	private enum INCIDENT_ACTIONS{
		NONE,
		RESOLVE_PEACEFULLY,
		INCREASE_TENSION,
	}

	private string _incidentName;
	private Kingdom _sourceKingdom;
	private Kingdom _targetKingdom;
	private Citizen _sourceKing;
	private Citizen _targetKing;
	private KingdomRelationship _krSourceToTarget;
	private KingdomRelationship _krTargetToSource;

	private bool _isSourceKingdomAggrieved;
	private bool _isTargetKingdomAggrieved;

	private GameDate _reactionDate;

	private INCIDENT_ACTIONS _sourceKingPreviousAction;
	private INCIDENT_ACTIONS _targetKingPreviousAction;


	#region Getters/Setters
	public string incidentName{
		get { return this._incidentName; }
	}
	public Kingdom sourceKingdom{
		get { return this._sourceKingdom; }
	}
	public Kingdom targetKingdom{
		get { return this._targetKingdom; }
	}
	public bool isSourceKingdomAggrieved{
		get { return this._isSourceKingdomAggrieved; }
	}
	public bool isTargetKingdomAggrieved{
		get { return this._isTargetKingdomAggrieved; }
	}
	#endregion

	public InternationalIncident(int startDay, int startMonth, int startYear, Citizen startedBy, Kingdom sourceKingdom, Kingdom targetKingdom, bool isSourceKingdomAggrieved, bool isTargetKingdomAggrieved) : base (startDay, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.INTERNATIONAL_INCIDENT;
		this.name = "International Incident";
		this._incidentName = RandomNameGenerator.Instance.GetInternationalIncidentName();
		this._sourceKingdom = sourceKingdom;
		this._targetKingdom = targetKingdom;
		this._sourceKing = sourceKingdom.king;
		this._targetKing = targetKingdom.king;
		this._isSourceKingdomAggrieved = isSourceKingdomAggrieved;
		this._isTargetKingdomAggrieved = isTargetKingdomAggrieved;
		this._krSourceToTarget = sourceKingdom.GetRelationshipWithKingdom (targetKingdom);
		this._krTargetToSource = targetKingdom.GetRelationshipWithKingdom (sourceKingdom);
		this._sourceKingPreviousAction = INCIDENT_ACTIONS.NONE;
		this._targetKingPreviousAction = INCIDENT_ACTIONS.NONE;
		ScheduleReactionDate ();
		KingdomManager.Instance.AddInternationalIncidents (this);
	}
	#region Overrides
	internal override void DoneEvent(){
		base.DoneEvent();
		RemoveRelationshipModifications ();
		KingdomManager.Instance.RemoveInternationalIncidents (this);
	}
	internal override void CancelEvent (){
		base.CancelEvent ();
		this.DoneEvent ();
	}
	#endregion

	private void StartIncident(){
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "InternationalIncident", "start");
		newLog.AddToFillers (this._sourceKingdom, this._sourceKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (this._targetKingdom, this._targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		newLog.AddToFillers (null, this.incidentName, LOG_IDENTIFIER.OTHER);
		UIManager.Instance.ShowNotification (newLog);
	}
	private void AddRelationshipModifications(){
		if(this._isSourceKingdomAggrieved){
			this._krSourceToTarget.AddRelationshipModifier (-20, "International Incident", RELATIONSHIP_MODIFIER.INTERNATIONAL_INCIDENT, false, false);
		}
		if(this._isTargetKingdomAggrieved){
			this._krTargetToSource.AddRelationshipModifier (-20, "International Incident", RELATIONSHIP_MODIFIER.INTERNATIONAL_INCIDENT, false, false);
		}
	}
	private void RemoveRelationshipModifications(){
		this._krSourceToTarget.RemoveRelationshipModifier (RELATIONSHIP_MODIFIER.INTERNATIONAL_INCIDENT);
		this._krTargetToSource.RemoveRelationshipModifier (RELATIONSHIP_MODIFIER.INTERNATIONAL_INCIDENT);
	}

	private void ScheduleReactionDate(){
		this._reactionDate = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
		this._reactionDate.AddDays (UnityEngine.Random.Range (15, 31));
		SchedulingManager.Instance.AddEntry (this._reactionDate, () => ReactionDay ());
	}

	private void ReactionDay(){
		if(!this._sourceKingdom.isDead || !this._targetKingdom.isDead){
			CancelEvent ();
			return;
		}
		if(!this.isActive){
			return;
		}
		Kingdom chosenKingdom = null;
		KingdomRelationship kr = null;
		if(this._isSourceKingdomAggrieved && !this._isTargetKingdomAggrieved){
			chosenKingdom = this._sourceKingdom;
			kr = this._krSourceToTarget;
		}else if(!this._isSourceKingdomAggrieved && this._isTargetKingdomAggrieved){
			chosenKingdom = this._targetKingdom;
			kr = this._krTargetToSource;
		}else if(this._isSourceKingdomAggrieved && this._isTargetKingdomAggrieved){
			int chance = UnityEngine.Random.Range (0, 2);
			if(chance == 0){
				chosenKingdom = this._sourceKingdom;
				kr = this._krSourceToTarget;
			}else{
				chosenKingdom = this._targetKingdom;
				kr = this._krTargetToSource;
			}
		}

		if(chosenKingdom != null && kr != null){
			int resolvePeacefullyTotalWeight = GetTotalWeight (chosenKingdom, kr, INCIDENT_ACTIONS.RESOLVE_PEACEFULLY);
			int increaseTensionTotalWeight = GetTotalWeight (chosenKingdom, kr, INCIDENT_ACTIONS.INCREASE_TENSION);

			if(this._sourceKing.id != this._sourceKingdom.king.id){
				this._sourceKingPreviousAction = INCIDENT_ACTIONS.NONE;
				this._sourceKing = this._sourceKingdom.king;
			}
			if(this._targetKing.id != this._targetKingdom.king.id){
				this._targetKingPreviousAction = INCIDENT_ACTIONS.NONE;
				this._targetKing = this._targetKingdom.king;
			}

			Dictionary<INCIDENT_ACTIONS, int> actionWeightDict = new Dictionary<INCIDENT_ACTIONS, int> () {
				{ INCIDENT_ACTIONS.RESOLVE_PEACEFULLY, resolvePeacefullyTotalWeight },
				{ INCIDENT_ACTIONS.INCREASE_TENSION, increaseTensionTotalWeight },
			};
			INCIDENT_ACTIONS pickedAction = Utilities.PickRandomElementWithWeights<INCIDENT_ACTIONS> (actionWeightDict);
			DoPickedAction (pickedAction, chosenKingdom);
		}
		if(this.isActive){
			ScheduleReactionDate ();
		}
	}

	private int GetTotalWeight(Kingdom kingdom, KingdomRelationship kr, INCIDENT_ACTIONS incidentAction){
		int totalWeight = 0;
		if(incidentAction == INCIDENT_ACTIONS.RESOLVE_PEACEFULLY){
			if(kingdom.king.otherTraits.Contains(TRAIT.PACIFIST)){
				totalWeight += 50;
			}
			if(kingdom.king.otherTraits.Contains(TRAIT.DIPLOMATIC)){
				totalWeight += 100;
			}
			if(kingdom.king.otherTraits.Contains(TRAIT.BENEVOLENT)){
				totalWeight += 20;
				if(kr.totalLike > 0){
					totalWeight += kr.totalLike * 2;
				}
			}
			if(kr.totalLike > 0){
				totalWeight += kr.totalLike * 5;
			}
		}else if(incidentAction == INCIDENT_ACTIONS.INCREASE_TENSION){
			if(kingdom.king.otherTraits.Contains(TRAIT.HOSTILE)){
				totalWeight += 20;
				if(kr.totalLike < 0){
					totalWeight += ((kr.totalLike * 2) * -1);
				}
			}
			if(kr.totalLike < 0){
				totalWeight += ((kr.totalLike * 5) * -1);
			}
			if(kingdom.stability < -80){
				int stabilityModifier = (kingdom.stability + 80) * -1;
				totalWeight += stabilityModifier * 10;
			}
		}

		if(kingdom.id == this._sourceKingdom.id){
			if(this._sourceKing.id == this._sourceKingdom.king.id){
				if(this._sourceKingPreviousAction == incidentAction){
					totalWeight += 50;
				}
			}
		}else if(kingdom.id == this._targetKingdom.id){
			if(this._targetKing.id == this._targetKingdom.king.id){
				if(this._targetKingPreviousAction == incidentAction){
					totalWeight += 50;
				}
			}
		}

		return totalWeight;
	}

	private void DoPickedAction(INCIDENT_ACTIONS incidentAction, Kingdom chosenKingdom){
		if(incidentAction == INCIDENT_ACTIONS.RESOLVE_PEACEFULLY){
			int chance = UnityEngine.Random.Range (0, 100);
			if(chance < 20){
				//Success Resolve Peacefully
				ResolvePeacefully();
			}else{
				//Fail Resolve Peacefully
				chosenKingdom.AdjustStability(-3);
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "InternationalIncident", "fail_resolve_peacefully");
				newLog.AddToFillers (chosenKingdom.king, chosenKingdom.king.name, LOG_IDENTIFIER.KING_1);
				newLog.AddToFillers (chosenKingdom, chosenKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
				newLog.AddToFillers (null, this.incidentName, LOG_IDENTIFIER.OTHER);
				UIManager.Instance.ShowNotification (newLog);
			}
		}else if(incidentAction == INCIDENT_ACTIONS.INCREASE_TENSION){
			int chance = UnityEngine.Random.Range (0, 100);
			if(chance < 20){
				//Success Increase Tension
				GoToWar(chosenKingdom);
			}else{
				//Fail Increase Tension
				chosenKingdom.AdjustStability(3);
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "InternationalIncident", "fail_war");
				newLog.AddToFillers (chosenKingdom.king, chosenKingdom.king.name, LOG_IDENTIFIER.KING_1);
				newLog.AddToFillers (chosenKingdom, chosenKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
				newLog.AddToFillers (null, this.incidentName, LOG_IDENTIFIER.OTHER);
				UIManager.Instance.ShowNotification (newLog);
			}
		}
	}

	private void ResolvePeacefully(){
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "InternationalIncident", "resolve_peacefully");
		newLog.AddToFillers (null, this.incidentName, LOG_IDENTIFIER.OTHER);
		newLog.AddToFillers (this._sourceKingdom, this._sourceKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (this._targetKingdom, this._targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		UIManager.Instance.ShowNotification (newLog);
		this.DoneEvent();
	}

	private void GoToWar(Kingdom chosenKingdom){
		Warfare newWar = null;
		if(this._krSourceToTarget.sharedRelationship.isAdjacent && this._krTargetToSource.sharedRelationship.isAdjacent){
			if(chosenKingdom.id == this._sourceKingdom.id){
				newWar = new Warfare (this._sourceKingdom, this._targetKingdom);
				this.DoneEvent ();
			}else if(chosenKingdom.id == this._targetKingdom.id){
				newWar = new Warfare (this._targetKingdom, this._sourceKingdom);
				this.DoneEvent ();
			}
			if(newWar != null){
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "InternationalIncident", "war");
				newLog.AddToFillers (null, this.incidentName, LOG_IDENTIFIER.OTHER);
				newLog.AddToFillers (null, newWar.name, LOG_IDENTIFIER.WAR_NAME);
				newLog.AddToFillers (this._sourceKingdom, this._sourceKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
				newLog.AddToFillers (this._targetKingdom, this._targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
				UIManager.Instance.ShowNotification (newLog);
			}
		}else{
			ResolvePeacefully ();
		}
	}
}
