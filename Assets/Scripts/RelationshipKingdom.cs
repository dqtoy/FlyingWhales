using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[System.Serializable]
public class RelationshipKingdom {

	private Kingdom _sourceKingdom;
	private Kingdom _targetKingdom;
	private bool _isAdjacent;
    private bool _isAtWar;
    private KingdomWar _kingdomWar;
	private MONTH _monthToMoveOnAfterRejection;
	private War _war;
	private InvasionPlan _invasionPlan;
	private RequestPeace _requestPeace;

	#region getters/setters
	public Kingdom sourceKingdom {
		get { return _sourceKingdom;}
	}

	public Kingdom targetKingdom {
		get { return _targetKingdom;}
	}

	public bool isAdjacent {
		get { return _isAdjacent;}
	}

	public bool isAtWar {
		get { return _isAtWar;}
	}

	public KingdomWar kingdomWar {
		get { return _kingdomWar;}
	}

	public MONTH monthToMoveOnAfterRejection {
		get { return _monthToMoveOnAfterRejection;}
	}

	public War war {
		get { return _war;}
	}

	public InvasionPlan invasionPlan {
		get { return _invasionPlan;}
	}
	#endregion

	public RelationshipKingdom(Kingdom _sourceKingdom, Kingdom _targetKingdom){
		this._sourceKingdom = _sourceKingdom;
		this._targetKingdom = _targetKingdom;
		this._isAtWar = false;
		this._isAdjacent = false;
		this._kingdomWar = new KingdomWar (_targetKingdom);
		this._monthToMoveOnAfterRejection = MONTH.NONE;
		this._invasionPlan = null;
	}

	internal void AdjustExhaustion(int amount){
		if(this._isAtWar){
			this._kingdomWar.AdjustExhaustion(amount);
		}

	}

	internal void MoveOnAfterRejection(){
		if ((MONTH)GameManager.Instance.month == monthToMoveOnAfterRejection) {
			Messenger.RemoveListener("OnDayEnd", MoveOnAfterRejection);
			this._monthToMoveOnAfterRejection = MONTH.NONE;
		}
	}

	internal void ResetAdjacency(){
		this._isAdjacent = false;
	}

	internal void SetAdjacency(bool isAdjacent){
		this._isAdjacent = isAdjacent;
	}

	internal void AssignWarEvent(War war){
		this._war = war;
	}

	internal void SetWarStatus(bool warStatus){
		this._isAtWar = warStatus;
	}

	internal void CreateInvasionPlan(GameEvent gameEventTrigger){
        if(this._invasionPlan == null) {
            this._invasionPlan = new InvasionPlan(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year,
            this._sourceKingdom.king, this._sourceKingdom, this._targetKingdom, gameEventTrigger, this._war);
        } else {
            Debug.LogError(_sourceKingdom.name + " already has an invasion plan towards " + _targetKingdom.name);
        }
	}

	//internal void CreateRequestPeaceEvent(Citizen citizenToSend, List<Citizen> saboteurs){
	//	this._requestPeace = new RequestPeace(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, this._sourceKingdom.king,
	//		citizenToSend, this._targetKingdom, saboteurs);
	//}

    internal void AssignRequestPeaceEvent(RequestPeace rp) {
        this._requestPeace = rp;
    }

	internal void DeclarePeace(){
        if(this._invasionPlan != null) {
            if (this._invasionPlan.isActive) {
                this._invasionPlan.CancelEvent();
            }
            this._invasionPlan = null;
        }
        if (this._requestPeace != null) {
            if (this._requestPeace.isActive) {
				this._war.GameEventWarWinner (this._targetKingdom);
                this._requestPeace.CancelEvent();
            }
            this._requestPeace = null;
        }
		this._war = null;
		this._isAtWar = false;
	}

	internal void SetMoveOnPeriodAfterRequestPeaceRejection(int moveOnMonth){
		this._monthToMoveOnAfterRejection = (MONTH)(moveOnMonth);
		Messenger.AddListener("OnDayEnd", this.MoveOnAfterRejection);
	}

	internal void TriggerRequestPeace(){
		this._kingdomWar.citiesLost += 1;
		if(this._sourceKingdom.cities.Count > 1 && _monthToMoveOnAfterRejection == MONTH.NONE){
			int peaceValue = 20;
			int chance = UnityEngine.Random.Range(0, 100);
			if(this._war != null && chance < peaceValue * this._kingdomWar.citiesLost){
				//Request Peace
				this._war.RequestPeace(this._sourceKingdom);
			}
		}
	}
}
