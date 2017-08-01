using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RequestPeace : GameEvent {

	private RelationshipKingdom _targetKingdomRel;
	private Kingdom _targetKingdom;

    private Envoy _envoySent;

	#region getters/setters
	public Kingdom targetKingdom {
		get { return this._targetKingdom; }
	}

	public Envoy envoySent {
		get { return this._envoySent; }
	}
	#endregion

	public RequestPeace(int startWeek, int startMonth, int startYear, Citizen startedBy, Envoy _envoySent, Kingdom _targetKingdom) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.REQUEST_PEACE;
		this.name = "Request Peace";
		this.description = startedBy.name + " has sent " + _envoySent.citizen.name + " to " + _targetKingdom.name + " to request peace.";
		this.durationInDays = 4;
		this.remainingDays = this.durationInDays;
		this._envoySent = _envoySent;
		this._targetKingdom = _targetKingdom;
		this._targetKingdomRel = _targetKingdom.GetRelationshipWithOtherKingdom(this.startedBy.city.kingdom);
			
		Log startLog = this._targetKingdomRel.war.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year,
				            "Events", "War", "request_peace_start_envoy");
		startLog.AddToFillers (this._startedBy, this._startedBy.name, LOG_IDENTIFIER.KING_1);
		startLog.AddToFillers (this._envoySent.citizen, this._envoySent.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		startLog.AddToFillers (this._targetKingdom.king, this._targetKingdom.king.name, LOG_IDENTIFIER.KING_2);
		

		//EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		EventManager.Instance.AddEventToDictionary(this);

	}

    //internal override void PerformAction(){
    //	if (this._citizenSent.isDead) {
    //		this.resolution = this._citizenSent.name + " died before he could reach " + this._targetKingdom.name;
    //		this.DoneEvent();
    //		return;
    //	}

    //	if (this.remainingDays > 0) {
    //		this.remainingDays -= 1;
    //	}
    //	int targetWarExhaustion = this._targetKingdomRel.kingdomWar.exhaustion;
    //	if (this.remainingDays <= 0) {
    //		int chanceForSuccess = 0;

    //		if (targetWarExhaustion >= 75) {
    //			chanceForSuccess += 75;
    //		}else if (targetWarExhaustion >= 50) {
    //			chanceForSuccess += 50;
    //		}else{
    //			chanceForSuccess += 10;
    //		}

    //		int chance = Random.Range(0, 100);
    //		if (chance < chanceForSuccess) {
    //			Log requestPeaceSuccess = this._targetKingdomRel.war.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year,
    //				"Events", "War", "request_peace_success");
    //			requestPeaceSuccess.AddToFillers (this._targetKingdom.king, this._targetKingdom.king.name);
    //			requestPeaceSuccess.AddToFillers (this._startedBy, this._startedBy.name);

    //			//request accepted
    //			KingdomManager.Instance.GetWarBetweenKingdoms(this.startedByKingdom, this._targetKingdom).DeclarePeace();
    //			this.resolution = this._targetKingdom.king.name + " accepted " + this.startedBy.name + "'s request for peace.";
    //		} else {
    //			Log requestPeaceSuccess = this._targetKingdomRel.war.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year,
    //				"Events", "War", "request_peace_fail");
    //			requestPeaceSuccess.AddToFillers (this._targetKingdom.king, this._targetKingdom.king.name);
    //			requestPeaceSuccess.AddToFillers (this._startedBy, this._startedBy.name);

    //			//request rejected
    //			RelationshipKingdom relationshipOfRequester = this.startedByKingdom.GetRelationshipWithOtherKingdom(this._targetKingdom);
    //			int moveOnMonth = GameManager.Instance.month;
    //			for (int i = 0; i < 3; i++) {
    //				moveOnMonth += 1;
    //				if (moveOnMonth > 12) {
    //					moveOnMonth = 1;
    //				}
    //			}
    //			relationshipOfRequester.SetMoveOnPeriodAfterRequestPeaceRejection(moveOnMonth);
    //			this.resolution = this._targetKingdom.king.name + " rejected " + this.startedBy.name + "'s request for peace.";
    //		}
    //		this.DoneEvent();
    //	}
    //}

	#region Overrides
    internal override void DoneCitizenAction(Citizen citizen) {
        if (isActive) {
            base.DoneCitizenAction(citizen);
            int chance = UnityEngine.Random.Range(0, 100);
            if (chance < 20 * (this._targetKingdomRel.kingdomWar.peaceRejected + 1)) {
                //request accepted
                Log requestPeaceSuccess = this._targetKingdomRel.war.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year,
                    "Events", "War", "request_peace_success");
                requestPeaceSuccess.AddToFillers(this._targetKingdom.king, this._targetKingdom.king.name, LOG_IDENTIFIER.KING_2);
                requestPeaceSuccess.AddToFillers(this._startedBy, this._startedBy.name, LOG_IDENTIFIER.KING_1);

                KingdomManager.Instance.GetWarBetweenKingdoms(this.startedByKingdom, this._targetKingdom).DeclarePeace();
                this.resolution = this._targetKingdom.king.name + " accepted " + this.startedBy.name + "'s request for peace.";
            } else {
                //request rejected
                this._targetKingdomRel.kingdomWar.peaceRejected += 1;

                //Set when startedByKingdom can request for peace again
                RelationshipKingdom relationshipOfRequester = this.startedByKingdom.GetRelationshipWithOtherKingdom(this._targetKingdom);
                int moveOnMonth = GameManager.Instance.month;
                for (int i = 0; i < 3; i++) {
                    moveOnMonth += 1;
                    if (moveOnMonth > 12) {
                        moveOnMonth = 1;
                    }
                }
                relationshipOfRequester.SetMoveOnPeriodAfterRequestPeaceRejection(moveOnMonth);

				Log requestPeaceFail = this._targetKingdomRel.war.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year,
					"Events", "War", "request_peace_fail");
				requestPeaceFail.AddToFillers(this._targetKingdom.king, this._targetKingdom.king.name, LOG_IDENTIFIER.KING_2);
				requestPeaceFail.AddToFillers(this._startedBy, this._startedBy.name, LOG_IDENTIFIER.KING_1);
            }
            this.DoneEvent();
        }
        
        //        int targetWarExhaustion = this._targetKingdomRel.kingdomWar.exhaustion;
        //
        //        int chanceForSuccess = 0;
        //
        //        if (targetWarExhaustion >= 75) {
        //            chanceForSuccess = 75;
        //        } else if (targetWarExhaustion >= 50) {
        //            chanceForSuccess = 50;
        //        } else {
        //            chanceForSuccess = 10;
        //        }
        //        int chance = Random.Range(0, 100);
        //        if (chance < chanceForSuccess) {
        //            Log requestPeaceSuccess = this._targetKingdomRel.war.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year,
        //                    "Events", "War", "request_peace_success");
        //			requestPeaceSuccess.AddToFillers(this._targetKingdom.king, this._targetKingdom.king.name, LOG_IDENTIFIER.KING_2);
        //			requestPeaceSuccess.AddToFillers(this._startedBy, this._startedBy.name, LOG_IDENTIFIER.KING_1);
        //
        //            //request accepted
        //            KingdomManager.Instance.GetWarBetweenKingdoms(this.startedByKingdom, this._targetKingdom).DeclarePeace();
        //            this.resolution = this._targetKingdom.king.name + " accepted " + this.startedBy.name + "'s request for peace.";
        //        } else {
        //            Log requestPeaceSuccess = this._targetKingdomRel.war.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year,
        //                    "Events", "War", "request_peace_fail");
        //			requestPeaceSuccess.AddToFillers(this._targetKingdom.king, this._targetKingdom.king.name, LOG_IDENTIFIER.KING_2);
        //			requestPeaceSuccess.AddToFillers(this._startedBy, this._startedBy.name, LOG_IDENTIFIER.KING_1);
        //
        //            //request rejected
        //            RelationshipKingdom relationshipOfRequester = this.startedByKingdom.GetRelationshipWithOtherKingdom(this._targetKingdom);
        //            int moveOnMonth = GameManager.Instance.month;
        //            for (int i = 0; i < 3; i++) {
        //                moveOnMonth += 1;
        //                if (moveOnMonth > 12) {
        //                    moveOnMonth = 1;
        //                }
        //            }
        //            relationshipOfRequester.SetMoveOnPeriodAfterRequestPeaceRejection(moveOnMonth);
        //            this.resolution = this._targetKingdom.king.name + " rejected " + this.startedBy.name + "'s request for peace.";
        //        }
    }

    internal override void DoneEvent(){
        base.DoneEvent();
//      this._envoySent.DestroyGO();
//		for (int i = 0; i < this._saboteurs.Count; i++) {
//			((Envoy)this._saboteurs[i].assignedRole).inAction = false;
//		}
//		EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);
    }
	internal override void CancelEvent (){
		base.CancelEvent ();
		this.DoneEvent ();
	}
	#endregion
}
