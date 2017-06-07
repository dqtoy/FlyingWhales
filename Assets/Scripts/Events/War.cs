using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class War : GameEvent {

	private Kingdom _kingdom1;
	private Kingdom _kingdom2;

	private RelationshipKingdom _kingdom1Rel;
	private RelationshipKingdom _kingdom2Rel;

	private bool _isAtWar;

	#region getters/setters
	public Kingdom kingdom1 {
		get { return _kingdom1; }
	}

	public Kingdom kingdom2{
		get { return _kingdom2; }
	}

	public RelationshipKingdom kingdom1Rel {
		get { return _kingdom1Rel; }
	}

	public RelationshipKingdom kingdom2Rel {
		get { return _kingdom2Rel; }
	}

	public bool isAtWar {
		get { return _isAtWar; }
	}
	#endregion

	public War(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom _kingdom1, Kingdom _kingdom2) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.KINGDOM_WAR;
		this.description = "War between " + _kingdom1.name + " and " + _kingdom2.name + ".";
		this._kingdom1 = _kingdom1;
		this._kingdom2 = _kingdom2;
		this._kingdom1Rel = _kingdom1.GetRelationshipWithOtherKingdom(_kingdom2);
		this._kingdom2Rel = _kingdom2.GetRelationshipWithOtherKingdom(_kingdom1);
		this._kingdom1Rel.AssignWarEvent(this);
		this._kingdom2Rel.AssignWarEvent(this);

		Log titleLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "War", "event_title");
		titleLog.AddToFillers (_kingdom1, _kingdom1.name);
		titleLog.AddToFillers (_kingdom2, _kingdom2.name);

		EventManager.Instance.AddEventToDictionary(this);

		this.EventIsCreated ();
	}

	internal void CreateInvasionPlan(Kingdom kingdomToDeclare, GameEvent gameEventTrigger, WAR_TRIGGER warTrigger = WAR_TRIGGER.NONE){
		if (kingdomToDeclare.id == this._kingdom1.id) {
			this._kingdom1Rel.CreateInvasionPlan(gameEventTrigger, warTrigger);
		} else {
			this._kingdom2Rel.CreateInvasionPlan(gameEventTrigger, warTrigger);
		}
	}

	internal void CreateRequestPeaceEvent(Kingdom kingdomToRequest){
        RequestPeace requestPeaceEvent = null;
		if (kingdomToRequest.id == this._kingdom1.id) {
            //this._kingdom1Rel.CreateRequestPeaceEvent(citizenToSend, saboteurs);
            requestPeaceEvent = EventCreator.Instance.CreateRequestPeace(kingdomToRequest, this._kingdom2);
            if (requestPeaceEvent != null) {
                this._kingdom1Rel.AssignRequestPeaceEvent(requestPeaceEvent);
            }
        } else {
            //this._kingdom2Rel.CreateRequestPeaceEvent(citizenToSend, saboteurs);
            requestPeaceEvent = EventCreator.Instance.CreateRequestPeace(kingdomToRequest, this._kingdom1);
            if (requestPeaceEvent != null) {
                this._kingdom2Rel.AssignRequestPeaceEvent(requestPeaceEvent);
            }
        }
	}

	internal void DeclareWar(Kingdom sourceKingdom){
		if(!this._isAtWar){
			this._isAtWar = true;
			if(sourceKingdom.id == this._kingdom1.id){
				KingdomManager.Instance.DeclareWarBetweenKingdoms(this._kingdom1, this._kingdom2, this);
			}else{
				KingdomManager.Instance.DeclareWarBetweenKingdoms(this._kingdom2, this._kingdom1, this);
			}
            EventManager.Instance.onWeekEnd.AddListener(AttemptToRequestPeace);
		}
	}

	internal void DeclarePeace(){
		this._isAtWar = false;
		this._kingdom1Rel.DeclarePeace();
		this._kingdom2Rel.DeclarePeace();
		KingdomManager.Instance.DeclarePeaceBetweenKingdoms(this._kingdom1, this._kingdom2);
		this.DoneEvent();
	}

	internal Kingdom GetKingdomInvolvedInWar(Kingdom kingdom){
		if (kingdom1.id == kingdom.id) {
			return kingdom1;
		} else {
			return kingdom2;
		}
	}

	internal void InvasionPlanCancelled(){
		if (this._kingdom1Rel.invasionPlan == null && this._kingdom2Rel.invasionPlan == null) {
			this.DeclarePeace();
			return;
		}

		if (this._kingdom1Rel.invasionPlan != null) {
			if (this._kingdom2Rel.invasionPlan != null) {
				if (!this._kingdom1Rel.invasionPlan.isActive && !this._kingdom2Rel.invasionPlan.isActive) {
					this.DeclarePeace ();
					return;
				}
			} else {
				if (!this._kingdom1Rel.invasionPlan.isActive) {
					this.DeclarePeace ();
					return;
				}
			}
		} else {
			if (this._kingdom2Rel.invasionPlan != null) {
				if (!this._kingdom2Rel.invasionPlan.isActive) {
					this.DeclarePeace ();
					return;
				}
			}
		}
	}

    protected void AttemptToRequestPeace() {
        Kingdom[] kingdomsInWar = new Kingdom[] { this._kingdom1, this._kingdom2 };
        for (int i = 0; i < kingdomsInWar.Length; i++) {
            Kingdom currKingdom = kingdomsInWar[i];
            Kingdom otherKingdom = this._kingdom2;
            RelationshipKingdom rel = this._kingdom1Rel;
            if (currKingdom.id == this._kingdom2.id) {
                otherKingdom = this._kingdom1;
                rel = this._kingdom2Rel;
            }

            if (rel.monthToMoveOnAfterRejection == MONTH.NONE
                && KingdomManager.Instance.GetRequestPeaceBetweenKingdoms(currKingdom, otherKingdom) == null) {

                int chanceToTriggerRequestPeace = 0;
                if (rel.kingdomWar.exhaustion >= 100) {
                    if (currKingdom.king.hostilityTrait == TRAIT.PACIFIST) {
                        chanceToTriggerRequestPeace = 4;
                    } else if (currKingdom.king.hostilityTrait == TRAIT.WARMONGER) {
                        chanceToTriggerRequestPeace = 2;
                    }
                } else if (rel.kingdomWar.exhaustion >= 75) {
                    if (currKingdom.king.hostilityTrait == TRAIT.PACIFIST) {
                        chanceToTriggerRequestPeace = 3;
                    } else if (currKingdom.king.hostilityTrait == TRAIT.WARMONGER) {
                        chanceToTriggerRequestPeace = 1;
                    }
                } else if (rel.kingdomWar.exhaustion >= 50) {
                    if (currKingdom.king.hostilityTrait == TRAIT.PACIFIST) {
                        chanceToTriggerRequestPeace = 2;
                    } else if (currKingdom.king.hostilityTrait == TRAIT.WARMONGER) {
                        chanceToTriggerRequestPeace = 0;
                    }
                }

                int chance = Random.Range(0, 100);
                if (chance < chanceToTriggerRequestPeace) {
                    this.CreateRequestPeaceEvent(currKingdom);
                }
            }
        }
    }

	#region Overrides
    internal override void DoneEvent() {
        base.DoneEvent();
        EventManager.Instance.onWeekEnd.RemoveListener(AttemptToRequestPeace);
    }
	internal override void CancelEvent (){
		base.CancelEvent ();
		this.DoneEvent ();
	}
	#endregion
}
