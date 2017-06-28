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

	public InvasionPlan(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom _sourceKingdom, Kingdom _targetKingdom, GameEvent gameEventTrigger, War _war, WAR_TRIGGER warTrigger) : base (startWeek, startMonth, startYear, startedBy){
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

		string reason = string.Empty;
		if (gameEventTrigger != null) {
			LogFiller[] logFillers = null;

			Log invasionPlanStart = this._war.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Reasons", "InvasionPlanReasons", gameEventTrigger.eventType.ToString());

//			invasionPlanStart.AddToFillers (gameEventTrigger, reason);
//			invasionPlanStart.AddToFillers (this._startedBy, this._startedBy.name);
//			invasionPlanStart.AddToFillers (this._targetKingdom, this._targetKingdom.name);
			if (gameEventTrigger is Assassination) {

			} else if (gameEventTrigger is BorderConflict) {


			} else if (gameEventTrigger is DiplomaticCrisis) {

			} else if (gameEventTrigger is Espionage) {

			} else if (gameEventTrigger is Raid) {

			} else if (gameEventTrigger is JoinWar) {

			} else if (gameEventTrigger is Plague) {
				
			} else {
				this.description = startedBy.name + " created an invasion plan against " + targetKingdom.king.name + ".";
			}
//			if (gameEventTrigger is Assassination) {
//				this.description = startedBy.name + " created an invasion plan against " + targetKingdom.king.name + " after discovering that " + gameEventTrigger.startedBy.name
//				+ " sent an assassin to kill " + (gameEventTrigger as Assassination).targetCitizen.name;
//
//				logFillers = new LogFiller[] {
//					new LogFiller (gameEventTrigger.startedBy, gameEventTrigger.startedBy.name),
//					new LogFiller ((gameEventTrigger as Assassination).targetCitizen, (gameEventTrigger as Assassination).targetCitizen.name)
//				};
//
//			} else if (gameEventTrigger is BorderConflict) {
//				this.description = startedBy.name + " created an invasion plan against " + targetKingdom.king.name + " in response to worsening Border Conflict.";
//				logFillers = new LogFiller[] {
//					new LogFiller (gameEventTrigger, gameEventTrigger.name),
//				};
//
//			} else if (gameEventTrigger is DiplomaticCrisis) {
//				this.description = startedBy.name + " created an invasion plan against " + targetKingdom.king.name + " in the aftermath of a recent Diplomatic Crisis.";
//				logFillers = new LogFiller[] {
//					new LogFiller (gameEventTrigger, gameEventTrigger.name),
//				};
//
//			} else if (gameEventTrigger is Espionage) {
//				this.description = startedBy.name + " created an invasion plan against " + targetKingdom.king.name + " after finding out that " + gameEventTrigger.startedBy.name + " spied on " + (gameEventTrigger as Espionage).targetKingdom.name + ".";
//				logFillers = new LogFiller[] {
//					new LogFiller (gameEventTrigger.startedBy, gameEventTrigger.startedBy.name),
//					new LogFiller ((gameEventTrigger as Espionage).targetKingdom, (gameEventTrigger as Espionage).targetKingdom.name)
//				};
//
//			} else if (gameEventTrigger is Raid) {
//				this.description = startedBy.name + " created an invasion plan against " + targetKingdom.king.name + " after the raid of " + (gameEventTrigger as Raid).raidedCity.name + ".";
//				logFillers = new LogFiller[] {
//					new LogFiller ((gameEventTrigger as Raid).raidedCity, (gameEventTrigger as Raid).raidedCity.name)
//				};
//
//			} else if (gameEventTrigger is JoinWar) {
//				this.description = startedBy.name + " created an invasion plan against " + targetKingdom.king.name + " at the request of " + (gameEventTrigger as JoinWar).startedByKingdom.name + ".";
//				logFillers = new LogFiller[] {
//					new LogFiller ((gameEventTrigger as JoinWar).startedByKingdom, (gameEventTrigger as JoinWar).startedByKingdom.name)
//				};
//
//			} else if (gameEventTrigger is Plague) {
////				this.description = startedBy.name + " created an invasion plan against " + targetKingdom.king.name + " at the request of " + (gameEventTrigger as Plague).startedByKingdom.name + ".";
//				logFillers = new LogFiller[] {
//					new LogFiller (this._targetKingdom.king, this._targetKingdom.king.name),
//					new LogFiller (gameEventTrigger, gameEventTrigger.name),
//				};
//			} else {
//				this.description = startedBy.name + " created an invasion plan against " + targetKingdom.king.name + ".";
//			} 
//			reason = Utilities.StringReplacer (LocalizationManager.Instance.GetLocalizedValue("Reasons", "InvasionPlanReasons", gameEventTrigger.eventType.ToString()), logFillers);
//
//			Log invasionPlanStart = this._war.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "War", "invasion_plan_start");
//			invasionPlanStart.AddToFillers (gameEventTrigger, reason);
//			invasionPlanStart.AddToFillers (this._startedBy, this._startedBy.name);
//			invasionPlanStart.AddToFillers (this._targetKingdom, this._targetKingdom.name);

			System.DateTime newDate = Utilities.GetNewDateAfterNumberOfDays(GameManager.Instance.month, GameManager.Instance.days+1, GameManager.Instance.year, this.durationInDays);
			invasionPlanStart.AddToFillers (null, ((MONTH)newDate.Month).ToString() + " " + newDate.Day.ToString() + ", " + newDate.Year.ToString(), LOG_IDENTIFIER.DATE);

		}else{
//			reason = Utilities.StringReplacer (LocalizationManager.Instance.GetLocalizedValue("Reasons", "WarTriggerReasons", warTrigger.ToString ())
//				, new LogFiller[]{ new LogFiller (this._targetKingdom.king, this._targetKingdom.king.name) });
//
//			Log invasionPlanStart = this._war.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "War", "invasion_plan_start");
//			invasionPlanStart.AddToFillers (null, reason);
//			invasionPlanStart.AddToFillers (this._startedBy, this._startedBy.name);
//			invasionPlanStart.AddToFillers (this._targetKingdom, this._targetKingdom.name);

			Log invasionPlanStart = this._war.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Reasons", "InvasionPlanReasons", warTrigger.ToString ());
//			invasionPlanStart.AddToFillers (null, reason);
//			invasionPlanStart.AddToFillers (this._startedBy, this._startedBy.name);
//			invasionPlanStart.AddToFillers (this._targetKingdom, this._targetKingdom.name);

			System.DateTime newDate = Utilities.GetNewDateAfterNumberOfDays(GameManager.Instance.month, GameManager.Instance.days+1, GameManager.Instance.year, this.durationInDays);
			invasionPlanStart.AddToFillers (null, ((MONTH)newDate.Month).ToString() + " " + newDate.Day.ToString() + ", " + newDate.Year.ToString(), LOG_IDENTIFIER.DATE);
		}

		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		EventManager.Instance.AddEventToDictionary(this);
//		this.StartMilitarizationEvent();
	}

	#region Overrides
	internal override void PerformAction(){
		if(this.remainingDays > 0){
			this.remainingDays -= 1;
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

			List<RelationshipKings> friends = this.startedBy.friends
                .Where(x => this.startedByKingdom.discoveredKingdoms.Contains(x.king.city.kingdom)).ToList();

			if (friends.Count > 0) {
				for (int i = 0; i < friends.Count; i++) {
					War friendWarWithTargetKingdom = KingdomManager.Instance.GetWarBetweenKingdoms (friends [i].king.city.kingdom, this._targetKingdom);
                    List<GameEvent> friendsActiveInvasionPlans = EventManager.Instance.GetEventsStartedByKingdom(friends[i].king.city.kingdom, new EVENT_TYPES[] { EVENT_TYPES.INVASION_PLAN })
                        .Where(x => x.isActive).ToList();
                    JoinWar activeJoinWarRequest = KingdomManager.Instance.GetJoinWarRequestBetweenKingdoms(this.startedByKingdom, friends[i].king.city.kingdom);
                    if (friendsActiveInvasionPlans.Count > 0 || activeJoinWarRequest != null || 
                        (friendWarWithTargetKingdom != null && friendWarWithTargetKingdom.isAtWar)) {
						//friend already has an active invasion plan or friend already has an active join war request from this startedByKingdom or is already
						//at war with target kingdom
						continue;
					}
					int chanceToSendJoinWarRequest = 2;
					if (friends [i].lordRelationship == RELATIONSHIP_STATUS.ALLY) {
						chanceToSendJoinWarRequest = 3;
					}
					int chance = Random.Range (0, 100);
					if (chance < chanceToSendJoinWarRequest) {
						JoinWar joinWar = EventCreator.Instance.CreateJoinWarEvent (this.startedByKingdom, friends [i].king.city.kingdom, this);
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
        EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);
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
		invasionPlanStart.AddToFillers(citizen.city.kingdom.king, citizen.city.kingdom.king.name, LOG_IDENTIFIER.KING_1);
		invasionPlanStart.AddToFillers(this._targetKingdom.king, this._targetKingdom.king.name, LOG_IDENTIFIER.KING_2);
		invasionPlanStart.AddToFillers(this.startedBy, this.startedBy.name, LOG_IDENTIFIER.KING_1);
	}
}
