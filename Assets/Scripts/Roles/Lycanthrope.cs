using UnityEngine;
using System.Collections;

public class Lycanthrope : Role {

    private Lycanthropy _lycanthropyEvent;

    private Kingdom _captor;
    private Kingdom _targetKingdom;

    private bool _isReturningHome;

    #region getters/setters
    public Lycanthropy lycanthropyEvent {
        get { return this._lycanthropyEvent; }
    }
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
        this.citizen.city.citizens.Remove(this.citizen);
        _captor = null;
        _targetKingdom = null;
        _isReturningHome = false;
        EventManager.Instance.onWeekEnd.AddListener(CheckForFreedom);
    }

    internal override void Initialize(GameEvent gameEvent) {
        if (gameEvent is Lycanthropy) {
            base.Initialize(gameEvent);
            this._lycanthropyEvent = (Lycanthropy)gameEvent;
            this.avatar = GameObject.Instantiate(Resources.Load("GameObjects/Lycanthrope"), this.citizen.city.hexTile.transform) as GameObject;
            this.avatar.transform.localPosition = Vector3.zero;
            this.avatar.GetComponent<LycanthropeAvatar>().Init(this);
        }
    }
    internal override void Attack() {
        //		base.Attack ();
        if (this.avatar != null) {
            this.avatar.GetComponent<LycanthropeAvatar>().HasAttacked();
            if (this.avatar.GetComponent<LycanthropeAvatar>().direction == DIRECTION.LEFT) {
                this.avatar.GetComponent<LycanthropeAvatar>().animator.Play("Attack_Left");
            } else if (this.avatar.GetComponent<LycanthropeAvatar>().direction == DIRECTION.RIGHT) {
                this.avatar.GetComponent<LycanthropeAvatar>().animator.Play("Attack_Right");
            } else if (this.avatar.GetComponent<LycanthropeAvatar>().direction == DIRECTION.UP) {
                this.avatar.GetComponent<LycanthropeAvatar>().animator.Play("Attack_Up");
            } else {
                this.avatar.GetComponent<LycanthropeAvatar>().animator.Play("Attack_Down");
            }
        }
    }

    private void CheckForFreedom() {
        if (_targetKingdom != null) {
            if (_targetKingdom.isDead) {
                Log freeKingdomLog = _lycanthropyEvent.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Lycanthropy", "free_lycanthrope_kingdom");
                FreeLycanthrope();
            }
        }
        if (_captor != null) {
            if (_captor.isDead) {
                Log freeKingLog = _lycanthropyEvent.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Lycanthropy", "free_lycanthrope_king");
                FreeLycanthrope();
            }
        }
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
        EventManager.Instance.onWeekEnd.RemoveListener(CheckForFreedom);
    }
}
