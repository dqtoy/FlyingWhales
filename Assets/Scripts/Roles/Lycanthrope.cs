using UnityEngine;
using System.Collections;

public class Lycanthrope : Role {

//    private Lycanthropy _lycanthropyEvent;

    private Kingdom _captor;
    private Kingdom _targetKingdom;

    private bool _isReturningHome;

    #region getters/setters
//    public Lycanthropy lycanthropyEvent {
//        get { return this._lycanthropyEvent; }
//    }
    public Kingdom captor {
        get { return this._captor; }
    }
    public Kingdom targetKingdom {
        get { return this._targetKingdom; }
    }
    public bool isReturningHome {
        get { return this._isReturningHome; }
        set { this._isReturningHome = value; }
    }
    #endregion
    public Lycanthrope(Citizen citizen): base(citizen){
        //this.citizen.city.citizens.Remove(this.citizen);
        _captor = null;
        _targetKingdom = null;
        _isReturningHome = false;
        Messenger.AddListener("OnDayEnd", CheckForFreedom);
    }

    internal override void Initialize(GameEvent gameEvent) {
//        if (gameEvent is Lycanthropy) {
//            base.Initialize(gameEvent);
//            this._lycanthropyEvent = (Lycanthropy)gameEvent;
//            this.avatar.GetComponent<LycanthropeAvatar>().Init(this);
//        }
    }

    private void CheckForFreedom() {
//        if (_captor != null) {
//            if (_captor.isDead) {
//                Log freeKingdomLog = _lycanthropyEvent.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Lycanthropy", "free_lycanthrope_kingdom");
//                freeKingdomLog.AddToFillers(this.citizen, this.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
//                freeKingdomLog.AddToFillers(_captor, _captor.name, LOG_IDENTIFIER.KINGDOM_2);
//                FreeLycanthrope();
//            }
//        }
    }
    internal void SetTargetKingdom(Kingdom kingdom) {
        _targetKingdom = kingdom;
    }
    internal void CaptureLycanthrope(Kingdom captor) {
        _captor = captor;
    }
    internal void FreeLycanthrope() {
        _captor = null;
        _targetKingdom = null;
    }

    internal override void OnDeath() {
        base.OnDeath();
        Messenger.RemoveListener("OnDayEnd", CheckForFreedom);
    }
}
