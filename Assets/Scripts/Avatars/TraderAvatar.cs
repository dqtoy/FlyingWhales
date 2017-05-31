using UnityEngine;
using System.Collections;
using Panda;

public class TraderAvatar : MonoBehaviour {
    private Trader _trader;
    [SerializeField] private PandaBehaviour pandaBehaviour;
    [SerializeField] private Animator animator;

    private DIRECTION direction;
    private bool collidedWithHostile = false;
    private General otherGeneral;

    internal void Init(Trader trader) {
        this._trader = trader;
        this.direction = DIRECTION.LEFT;
        this.AddBehaviourTree();
    }

    #region Monobehaviour Functions
    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "General") {
            this.collidedWithHostile = false;
            if (this.gameObject != null && other.gameObject != null) {
                Kingdom kingdomOfGeneral = other.gameObject.GetComponent<GeneralObject>().general.citizen.city.kingdom;
                Kingdom kingdomOfTrader = this._trader.citizen.city.kingdom;
                if (kingdomOfGeneral.id != kingdomOfTrader.id) {
                    RelationshipKings relOfGeneralWithTrader = kingdomOfGeneral.king.GetRelationshipWithCitizen(kingdomOfTrader.king);
                    RelationshipKings relOfTraderWithGeneral = kingdomOfTrader.king.GetRelationshipWithCitizen(kingdomOfGeneral.king);
                    if (relOfGeneralWithTrader.lordRelationship == RELATIONSHIP_STATUS.ENEMY || relOfGeneralWithTrader.lordRelationship == RELATIONSHIP_STATUS.RIVAL ||
                       relOfTraderWithGeneral.lordRelationship == RELATIONSHIP_STATUS.ENEMY || relOfTraderWithGeneral.lordRelationship == RELATIONSHIP_STATUS.RIVAL) {
                        if (!other.gameObject.GetComponent<GeneralObject>().general.citizen.isDead) {
                            this.collidedWithHostile = true;
                            this.otherGeneral = other.gameObject.GetComponent<GeneralObject>().general;
                        }
                    }  
                }
            }
        }


    }
    #endregion

    #region Behaviour Tree Functions
    internal void AddBehaviourTree() {
        BehaviourTreeManager.Instance.allTrees.Add(this.pandaBehaviour);
    }

    internal void RemoveBehaviourTree() {
        BehaviourTreeManager.Instance.allTrees.Remove(this.pandaBehaviour);
    }
    #endregion

    #region Behaviour Tree Tasks
    [Task]
    private bool IsTraderAlive() {
        return !this._trader.citizen.isDead;
    }

    [Task]
    private bool IsTradeEventDone() {
        return !this._trader.tradeEvent.isActive;
    }

    [Task]
    private bool HasTraderReachedTarget() {
        if (this._trader.location == this._trader.targetLocation) {
            return true;
        }
        return false;
    }

    [Task]
    private bool HasTraderCollidedWithHostileGeneral() {
        if (this.collidedWithHostile) {
            this.collidedWithHostile = false;
            this._trader.citizen.Death(DEATH_REASONS.BATTLE);
            return true;
        }
        return false;
    }

    [Task]
    private void MoveToNextTile() {
        
    }
    #endregion

}
