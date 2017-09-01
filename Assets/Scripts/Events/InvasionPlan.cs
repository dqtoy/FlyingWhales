using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class InvasionPlan : GameEvent {

	private Kingdom _sourceKingdom;
	private Kingdom _targetKingdom;
	private List<Citizen> _uncovered;
	private War _war;
	private Militarization _militarizationEvent = null;
	private List<JoinWar> _joinWarEvents;

	#region getters/setters
	public Kingdom sourceKingdom {
		get { return this._sourceKingdom; }
	}

	public Kingdom targetKingdom {
		get { return this._targetKingdom; }
	}

	public List<Citizen> uncovered {
		get { return this._uncovered; }
	}

	public War war {
		get { return this._war; }
	}

	public Militarization militarizationEvent {
		get { return this._militarizationEvent; }
	}

	public List<JoinWar> joinWarEvents{
		get { return _joinWarEvents; }
	}
	#endregion

	public InvasionPlan(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom _sourceKingdom, Kingdom _targetKingdom, GameEvent gameEventTrigger, War _war) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.INVASION_PLAN;
		this.eventStatus = EVENT_STATUS.HIDDEN;
		this.name = "Invasion Plan";
		this.description = startedBy.name + " created an invasion plan against " + _targetKingdom.king.name + ".";
		this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
		this.remainingDays = this.durationInDays;
		this._sourceKingdom = _sourceKingdom;
		this._targetKingdom = _targetKingdom;
		this._war = _war;
		this._uncovered = new List<Citizen>();
		this._joinWarEvents = new List<JoinWar>();

		Log invasionPlanStart = this._war.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Reasons", "InvasionPlanReasons", this._war.warTrigger.ToString());
		GameDate newDate = Utilities.GetNewDateAfterNumberOfDays(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.durationInDays);

		invasionPlanStart.AddToFillers (this._sourceKingdom.king, this._sourceKingdom.king.name, LOG_IDENTIFIER.KING_1);
		invasionPlanStart.AddToFillers (this._sourceKingdom, this._sourceKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
		invasionPlanStart.AddToFillers (this._targetKingdom.king, this._targetKingdom.king.name, LOG_IDENTIFIER.KING_2);
		invasionPlanStart.AddToFillers (this._targetKingdom, this._targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		invasionPlanStart.AddToFillers (null, ((MONTH)newDate.month).ToString() + " " + newDate.day.ToString() + ", " + newDate.year.ToString(), LOG_IDENTIFIER.DATE);

		if(warTrigger == WAR_TRIGGER.OPPOSING_APPROACH){
			invasionPlanStart.AddToFillers (gameEventTrigger, gameEventTrigger.name, LOG_IDENTIFIER.GAME_EVENT);
		}

		Messenger.AddListener("OnDayEnd", this.PerformAction);
		EventManager.Instance.AddEventToDictionary(this);
		this._war.EventIsCreated (this._sourceKingdom, true);
		EventIsCreated (this._sourceKingdom, false);
		EventIsCreated (this._targetKingdom, false);

		this._war.hasInvasionPlan = true;
//		this.StartMilitarizationEvent();
	}

	#region Overrides
	internal override void PerformAction(){
        this.remainingDays -= 1;
        if (this.remainingDays > 0){
			if (this.startedBy.isDead) {
				this.resolution = "Invasion plan was cancelled because " + this.startedBy.name + " died.";
				Log invasionPlanCancelDeath = this._war.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "War", "invasion_plan_cancel_death");
				invasionPlanCancelDeath.AddToFillers (this.startedBy, this.startedBy.name, LOG_IDENTIFIER.KING_1);

				this.DoneEvent();
				return;
			}
			if (!this.startedBy.isKing) {
				this.resolution = "Invasion plan was cancelled because " + this.startedBy.name + " is no longer king.";
				Log invasionPlanCancelDethrone = this._war.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "War", "invasion_plan_cancel_dethrone");
				invasionPlanCancelDethrone.AddToFillers (this.startedBy, this.startedBy.name, LOG_IDENTIFIER.KING_1);

				this.DoneEvent();
				return;
			}

			War warBetweenKingdoms = KingdomManager.Instance.GetWarBetweenKingdoms (this._sourceKingdom, this._targetKingdom);
			if (warBetweenKingdoms != null && warBetweenKingdoms.isAtWar) {
				this.resolution = "Invasion plan was cancelled because a war has already started between " + this._sourceKingdom.name + " and " + this._targetKingdom.name;
				this.DoneEvent();
				return;
			}

            List<Kingdom> friends = this.startedByKingdom.GetKingdomsByRelationship(new RELATIONSHIP_STATUS[] { RELATIONSHIP_STATUS.FRIEND, RELATIONSHIP_STATUS.ALLY });

			if (friends.Count > 0) {
				for (int i = 0; i < friends.Count; i++) {
                    Kingdom currFriend = friends[i];
					War friendWarWithTargetKingdom = KingdomManager.Instance.GetWarBetweenKingdoms (currFriend, this._targetKingdom);
                    List<GameEvent> friendsActiveInvasionPlans = currFriend.activeEvents.Where(x => x.eventType == EVENT_TYPES.INVASION_PLAN).ToList();
                    JoinWar activeJoinWarRequest = KingdomManager.Instance.GetJoinWarRequestBetweenKingdoms(this.startedByKingdom, currFriend);
                    if (friendsActiveInvasionPlans.Count > 0 || activeJoinWarRequest != null || 
                        (friendWarWithTargetKingdom != null && friendWarWithTargetKingdom.isAtWar)) {
						//friend already has an active invasion plan or friend already has an active join war request from this startedByKingdom or is already
						//at war with target kingdom
						continue;
					}
					int chanceToSendJoinWarRequest = 2;
					if (currFriend.GetRelationshipWithKingdom(startedByKingdom).relationshipStatus == RELATIONSHIP_STATUS.ALLY) {
						chanceToSendJoinWarRequest = 3;
					}
					int chance = Random.Range (0, 100);
					if (chance < chanceToSendJoinWarRequest) {
						JoinWar joinWar = EventCreator.Instance.CreateJoinWarEvent (this.startedByKingdom, currFriend, this);
						if(joinWar != null){
							this._joinWarEvents.Add(joinWar);
						}
					}
				}
			}
		}else{
			InvasionPlanSuccessful ();
		}

	}

	internal override void DoneEvent(){
        base.DoneEvent();
        Messenger.RemoveListener("OnDayEnd", this.PerformAction);
	}

	internal override void CancelEvent (){
		this.resolution = "Event was cancelled.";
        for (int i = 0; i < this._joinWarEvents.Count; i++) {
            if (this._joinWarEvents[i].isActive) {
                this._joinWarEvents[i].CancelEvent();
            }
        }
        if(this._militarizationEvent != null && this.militarizationEvent.isActive) {
            this._militarizationEvent.CancelEvent();
        }
		Log invasionPlanCancel = this._war.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Reasons", "InvasionPlanCancelReasons", "reason1");
//		invasionPlanCancel.AddToFillers (this.startedBy, this.startedBy.name, LOG_IDENTIFIER.KING_1);
//		invasionPlanCancel.AddToFillers (null, " because relationships with the target kingdom were improved");

		this._war.InvasionPlanCancelled();

		this.DoneEvent();
	}
	#endregion


	internal void InvasionPlanSuccessful(){
		this.resolution = "Invasion plan was successful and war is now declared between " + this._sourceKingdom.name + " and " + this._targetKingdom.name;
		this.war.DeclareWar (this._sourceKingdom);
		this.DoneEvent();
	}

	private void StartMilitarizationEvent(){
		Militarization currentMilitarization = null;
		if(IsThereMilitarizationActive(ref currentMilitarization)){
			this._militarizationEvent = currentMilitarization;
			currentMilitarization.remainingDays = currentMilitarization.durationInDays;
		}else{
			//New Militarization
			Militarization newMilitarization  = new Militarization(this.startDay, this.startMonth, this.startYear, this.startedBy, this);
			this._militarizationEvent = newMilitarization;
		}
	}

	private bool IsThereMilitarizationActive(ref Militarization currentMilitarization){
		List<GameEvent> militarizationEvents = EventManager.Instance.GetEventsOfType (EVENT_TYPES.MILITARIZATION).Where(x => x.isActive).ToList();
		if(militarizationEvents != null){
			for (int i = 0; i < militarizationEvents.Count; i++) {
				if (((Militarization)militarizationEvents[i]).startedBy.id == _sourceKingdom.king.id) {
					currentMilitarization = (Militarization) militarizationEvents [i];
					return true;
				}
			}
			return false;
		}else{
			return false;
		}
	}

	internal void AddCitizenThatUncoveredEvent(Citizen citizen){
		this._uncovered.Add(citizen);
		Log invasionPlanStart = this._war.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 
			"Events", "War", "invasion_plan_discovered");
		invasionPlanStart.AddToFillers(citizen.city.kingdom.king, citizen.city.kingdom.king.name, LOG_IDENTIFIER.KING_3);
		invasionPlanStart.AddToFillers(this._targetKingdom.king, this._targetKingdom.king.name, LOG_IDENTIFIER.KING_2);
		invasionPlanStart.AddToFillers(this.startedBy, this.startedBy.name, LOG_IDENTIFIER.KING_1);
	}
}
