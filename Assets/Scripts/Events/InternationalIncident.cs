using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InternationalIncident : GameEvent {

	public enum INCIDENT_ACTIONS{
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
		this._krSourceToTarget.sharedRelationship.AddInternationalIncident (this);
		KingdomManager.Instance.AddInternationalIncidents (this);
	}
	#region Overrides
	internal override void DoneEvent(){
		base.DoneEvent();
		RemoveRelationshipModifications ();
		this._krSourceToTarget.sharedRelationship.RemoveInternationalIncident (this);
		KingdomManager.Instance.RemoveInternationalIncidents (this);
	}
	internal override void CancelEvent (){
		base.CancelEvent ();
		this.DoneEvent ();
	}
	#endregion

    internal void ShowSuccessLog() {
        Log newLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "InternationalIncident", "start_success");
        newLog.AddToFillers(this.startedBy, this.startedBy.name, LOG_IDENTIFIER.KING_1);
        newLog.AddToFillers(this.startedBy.city.kingdom, this.startedBy.city.kingdom.name, LOG_IDENTIFIER.KINGDOM_1);
        newLog.AddToFillers(this._sourceKingdom, this._sourceKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
        newLog.AddToFillers(this._targetKingdom, this._targetKingdom.name, LOG_IDENTIFIER.KINGDOM_3);
        newLog.AddToFillers(null, this.incidentName, LOG_IDENTIFIER.OTHER);
        UIManager.Instance.ShowNotification(newLog, new HashSet<Kingdom> { this.startedBy.city.kingdom, _sourceKingdom, _targetKingdom });
    }
    internal void ShowCriticalFailLog(Kingdom otherKingdom) {
        Log newLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "InternationalIncident", "start_critical_fail");
        newLog.AddToFillers(this.startedBy, this.startedBy.name, LOG_IDENTIFIER.KING_1);
        newLog.AddToFillers(this.startedBy.city.kingdom, this.startedBy.city.kingdom.name, LOG_IDENTIFIER.KINGDOM_1);
        newLog.AddToFillers(this._sourceKingdom, this._sourceKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
        newLog.AddToFillers(this._targetKingdom, this._targetKingdom.name, LOG_IDENTIFIER.KINGDOM_3);
        UIManager.Instance.ShowNotification(newLog, new HashSet<Kingdom> { this.startedBy.city.kingdom, _sourceKingdom, _targetKingdom });
    }
    internal void ShowCaughtLog() {
        Log newLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "InternationalIncident", "start_caught");
        newLog.AddToFillers(this.startedBy.city.kingdom, this.startedBy.city.kingdom.name, LOG_IDENTIFIER.KINGDOM_1);
        newLog.AddToFillers(this._sourceKingdom, this._sourceKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
        newLog.AddToFillers(null, this.incidentName, LOG_IDENTIFIER.OTHER);
        UIManager.Instance.ShowNotification(newLog, new HashSet<Kingdom> { this.startedBy.city.kingdom, _sourceKingdom, _targetKingdom });
    }
	internal void ShowRandomLog() {
		Log newLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "InternationalIncident", "start_random");
		newLog.AddToFillers(this._sourceKingdom, this._sourceKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers(this._targetKingdom, this._targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		newLog.AddToFillers(null, this.incidentName, LOG_IDENTIFIER.OTHER);
		UIManager.Instance.ShowNotification(newLog, new HashSet<Kingdom> { this.startedBy.city.kingdom, _sourceKingdom, _targetKingdom });
	}

    private void StartIncident(){
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "InternationalIncident", "start");
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
		Debug.Log ("------------------------------- REACTION DAY FOR " + this._incidentName + "--------------------------------------");
		if(this._sourceKingdom.isDead || this._targetKingdom.isDead){
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
			Debug.Log ("REACTING KINGDOM: " + chosenKingdom.name);
			if(this._sourceKing.id != this._sourceKingdom.king.id){
				this._sourceKingPreviousAction = INCIDENT_ACTIONS.NONE;
				this._sourceKing = this._sourceKingdom.king;
			}
			if(this._targetKing.id != this._targetKingdom.king.id){
				this._targetKingPreviousAction = INCIDENT_ACTIONS.NONE;
				this._targetKing = this._targetKingdom.king;
			}

			int resolvePeacefullyTotalWeight = GetTotalWeight (chosenKingdom, kr, INCIDENT_ACTIONS.RESOLVE_PEACEFULLY);
			int increaseTensionTotalWeight = GetTotalWeight (chosenKingdom, kr, INCIDENT_ACTIONS.INCREASE_TENSION);

			Debug.Log ("RESOLVE_PEACEFULLY: " + resolvePeacefullyTotalWeight.ToString());
			Debug.Log ("INCREASE_TENSION: " + increaseTensionTotalWeight.ToString());

			if(resolvePeacefullyTotalWeight > 0 && increaseTensionTotalWeight > 0){
				Dictionary<INCIDENT_ACTIONS, int> actionWeightDict = new Dictionary<INCIDENT_ACTIONS, int> () {
					{ INCIDENT_ACTIONS.RESOLVE_PEACEFULLY, resolvePeacefullyTotalWeight },
					{ INCIDENT_ACTIONS.INCREASE_TENSION, increaseTensionTotalWeight },
				};
				INCIDENT_ACTIONS pickedAction = Utilities.PickRandomElementWithWeights<INCIDENT_ACTIONS> (actionWeightDict);
				DoPickedAction (pickedAction, chosenKingdom);
			}else{
				DoPickedAction (INCIDENT_ACTIONS.INCREASE_TENSION, chosenKingdom);
			}

		}
		if(this.isActive){
			ScheduleReactionDate ();
		}
	}

	private int GetTotalWeight(Kingdom kingdom, KingdomRelationship kr, INCIDENT_ACTIONS incidentAction){
		int totalWeight = 0;
		for (int i = 0; i < kingdom.king.allTraits.Count; i++) {
			totalWeight += kingdom.king.allTraits [i].GetInternationalIncidentReactionWeight (incidentAction, kr);
		}
		if(incidentAction == INCIDENT_ACTIONS.RESOLVE_PEACEFULLY){
			if(kr.totalLike > 0){
				totalWeight += (kr.totalLike * 2);
			}
			if(kr.AreAllies()){
				totalWeight += 200;
			}
			if(kr.AreTradePartners()){
				totalWeight += 100;
			}
			totalWeight += (300 * kingdom.warfareInfo.Count);

		}else if(incidentAction == INCIDENT_ACTIONS.INCREASE_TENSION){
			if(kr.totalLike < 0){
				totalWeight -= (kr.totalLike * 2);
			}
			if(kingdom.stability < -80){
				int stabilityModifier = kingdom.stability + 80;
				totalWeight -= stabilityModifier * 10;
			}
		}

		if(kingdom.id == this._sourceKingdom.id){
			if(this._sourceKingPreviousAction == incidentAction){
				totalWeight += 50;
			}
		}else if(kingdom.id == this._targetKingdom.id){
			if(this._targetKingPreviousAction == incidentAction){
				totalWeight += 50;
			}
		}
		if(totalWeight < 0){
			totalWeight = 0;
		}
		return totalWeight;
	}

	private void DoPickedAction(INCIDENT_ACTIONS incidentAction, Kingdom chosenKingdom){
		Debug.Log ("REACTION: " + incidentAction.ToString());
		if(chosenKingdom.id == this._sourceKingdom.id){
			this._sourceKingPreviousAction = incidentAction;
		}else if(chosenKingdom.id == this._targetKingdom.id){
			this._targetKingPreviousAction = incidentAction;
		}
		if(incidentAction == INCIDENT_ACTIONS.RESOLVE_PEACEFULLY){
			int chance = UnityEngine.Random.Range (0, 100);
			if(chance < 30){
				//Success Resolve Peacefully
				ResolvePeacefully();
			}else{
				//Fail Resolve Peacefully
				chosenKingdom.AdjustStability(-3);
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "InternationalIncident", "fail_resolve_peacefully");
				newLog.AddToFillers (chosenKingdom.king, chosenKingdom.king.name, LOG_IDENTIFIER.KING_1);
				newLog.AddToFillers (chosenKingdom, chosenKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
				newLog.AddToFillers (null, this.incidentName, LOG_IDENTIFIER.OTHER);
				UIManager.Instance.ShowNotification (newLog, new HashSet<Kingdom> { _sourceKingdom, _targetKingdom });
			}
		}else if(incidentAction == INCIDENT_ACTIONS.INCREASE_TENSION){
			int chance = UnityEngine.Random.Range (0, 100);
			if(chance < 20){
				//Success Increase Tension
				GoToWar(chosenKingdom);
			}else{
				//Fail Increase Tension
				chosenKingdom.AdjustStability(3);
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "InternationalIncident", "fail_war");
				newLog.AddToFillers (chosenKingdom.king, chosenKingdom.king.name, LOG_IDENTIFIER.KING_1);
				newLog.AddToFillers (chosenKingdom, chosenKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
				newLog.AddToFillers (null, this.incidentName, LOG_IDENTIFIER.OTHER);
				UIManager.Instance.ShowNotification (newLog, new HashSet<Kingdom> { _sourceKingdom, _targetKingdom });
			}
		}
	}

	private void ResolvePeacefully(){
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "InternationalIncident", "resolve_peacefully");
		newLog.AddToFillers (null, this.incidentName, LOG_IDENTIFIER.OTHER);
		newLog.AddToFillers (this._sourceKingdom, this._sourceKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (this._targetKingdom, this._targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		UIManager.Instance.ShowNotification (newLog, new HashSet<Kingdom> { _sourceKingdom, _targetKingdom });
		this.DoneEvent();
	}

	private void GoToWar(Kingdom chosenKingdom){
		Warfare newWar = null;
		if(!this._krSourceToTarget.sharedRelationship.isAtWar){
			if(chosenKingdom.id == this._sourceKingdom.id){
				this.DoneEvent ();
				newWar = new Warfare (this._sourceKingdom, this._targetKingdom);
			}else if(chosenKingdom.id == this._targetKingdom.id){
				this.DoneEvent ();
				newWar = new Warfare (this._targetKingdom, this._sourceKingdom);
			}
			if(newWar != null){
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "InternationalIncident", "war");
				newLog.AddToFillers (null, this.incidentName, LOG_IDENTIFIER.OTHER);
				newLog.AddToFillers (null, newWar.name, LOG_IDENTIFIER.WAR_NAME);
				newLog.AddToFillers (this._sourceKingdom, this._sourceKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
				newLog.AddToFillers (this._targetKingdom, this._targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
				UIManager.Instance.ShowNotification (newLog, new HashSet<Kingdom> { _sourceKingdom, _targetKingdom });
			}
		}else{
			ResolvePeacefully ();
		}
	}
}
