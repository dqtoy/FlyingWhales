using UnityEngine;
using System.Collections;

public class Adventurer : Role {

    private object _latestDiscovery;

    #region getters/setters
    public object latestDiscovery {
        get { return _latestDiscovery; }
    }

    #endregion

    public Adventurer(Citizen citizen) : base(citizen) {
        this.damage = 50;
    }

    internal override void Initialize(GameEvent gameEvent) {
        if (gameEvent is Adventure) {
            base.Initialize(gameEvent);
            this.avatar = GameObject.Instantiate(Resources.Load("GameObjects/Adventurer"), this.citizen.city.hexTile.transform) as GameObject;
            this.avatar.transform.localPosition = Vector3.zero;
            this.avatar.GetComponent<AdventurerAvatar>().Init(this);
        }
    }

    internal override void Attack() {
        //		base.Attack ();
        if (this.avatar != null) {
            this.avatar.GetComponent<AdventurerAvatar>().HasAttacked();
            if (this.avatar.GetComponent<AdventurerAvatar>().direction == DIRECTION.LEFT) {
                this.avatar.GetComponent<AdventurerAvatar>().animator.Play("Attack_Left");
            } else if (this.avatar.GetComponent<AdventurerAvatar>().direction == DIRECTION.RIGHT) {
                this.avatar.GetComponent<AdventurerAvatar>().animator.Play("Attack_Right");
            } else if (this.avatar.GetComponent<AdventurerAvatar>().direction == DIRECTION.UP) {
                this.avatar.GetComponent<AdventurerAvatar>().animator.Play("Attack_Up");
            } else {
                this.avatar.GetComponent<AdventurerAvatar>().animator.Play("Attack_Down");
            }
        }
    }

    internal void SetLatestDiscovery(object discovery) {
        _latestDiscovery = discovery;
    }
}
