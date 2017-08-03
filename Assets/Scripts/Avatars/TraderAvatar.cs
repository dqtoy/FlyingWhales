using UnityEngine;
using System.Collections;
using Panda;
using System.Collections.Generic;

public class TraderAvatar : MonoBehaviour {
    private Trader _trader;
    [SerializeField] private PandaBehaviour pandaBehaviour;
    [SerializeField] private Animator animator;

    private DIRECTION direction;
    private bool collidedWithHostile = false;
    private General otherGeneral;
    private List<HexTile> pathToUnhighlight = new List<HexTile>();

    internal void Init(Trader trader) {
        this._trader = trader;
        this.direction = DIRECTION.LEFT;
		this.GetComponent<Avatar> ().kingdom = this._trader.citizen.city.kingdom;
		this.GetComponent<Avatar> ().gameEvent = this._trader.tradeEvent;
		this.GetComponent<Avatar> ().citizen = this._trader.citizen;
        visibleTiles = new List<HexTile>();

        this.AddBehaviourTree();
    }

    #region Monobehaviour Functions
    //void OnTriggerEnter2D(Collider2D other) {
    //    if (other.tag == "General") {
    //        this.collidedWithHostile = false;
    //        if (this.gameObject != null && other.gameObject != null) {
				//Kingdom kingdomOfGeneral = other.gameObject.GetComponent<GeneralAvatar>().general.citizen.city.kingdom;
    //            Kingdom kingdomOfTrader = this._trader.citizen.city.kingdom;
    //            if (kingdomOfGeneral.id != kingdomOfTrader.id) {
    //                RelationshipKings relOfGeneralWithTrader = kingdomOfGeneral.king.GetRelationshipWithCitizen(kingdomOfTrader.king);
    //                RelationshipKings relOfTraderWithGeneral = kingdomOfTrader.king.GetRelationshipWithCitizen(kingdomOfGeneral.king);
    //                if (relOfGeneralWithTrader.lordRelationship == RELATIONSHIP_STATUS.ENEMY || relOfGeneralWithTrader.lordRelationship == RELATIONSHIP_STATUS.RIVAL ||
    //                   relOfTraderWithGeneral.lordRelationship == RELATIONSHIP_STATUS.ENEMY || relOfTraderWithGeneral.lordRelationship == RELATIONSHIP_STATUS.RIVAL) {
				//		if (!other.gameObject.GetComponent<GeneralAvatar>().general.citizen.isDead) {
    //                        this.collidedWithHostile = true;
				//			this.otherGeneral = other.gameObject.GetComponent<GeneralAvatar>().general;
    //                    }
    //                }  
    //            }
    //        }
    //    }
    //}
	void OnTriggerEnter2D(Collider2D other){
		if(other.tag == "Avatar"){
			if(this.gameObject != null && other.gameObject != null){
				if(other.gameObject.GetComponent<Avatar>().kingdom.id != this._trader.citizen.city.kingdom.id){
					if(!other.gameObject.GetComponent<Avatar> ().citizen.isDead){
						CombatManager.Instance.HasCollidedWithHostile (this.GetComponent<Avatar> (), other.gameObject.GetComponent<Avatar>());
					}
				}
			}
		}else if(other.tag == "Trader"){
			if(this.gameObject != null && other.gameObject != null){
				Kingdom kingdomOfGeneral = this._trader.citizen.city.kingdom;
				Kingdom kingdomOfTrader = other.gameObject.GetComponent<Avatar>().kingdom;
				if (kingdomOfGeneral.id != kingdomOfTrader.id) {
					RelationshipKings relOfGeneralWithTrader = kingdomOfGeneral.king.GetRelationshipWithCitizen(kingdomOfTrader.king);
					RelationshipKings relOfTraderWithGeneral = kingdomOfTrader.king.GetRelationshipWithCitizen(kingdomOfGeneral.king);
					if (relOfGeneralWithTrader.lordRelationship == RELATIONSHIP_STATUS.ENEMY || relOfGeneralWithTrader.lordRelationship == RELATIONSHIP_STATUS.RIVAL ||
						relOfTraderWithGeneral.lordRelationship == RELATIONSHIP_STATUS.ENEMY || relOfTraderWithGeneral.lordRelationship == RELATIONSHIP_STATUS.RIVAL) {
						if (!other.gameObject.GetComponent<Avatar>().citizen.isDead) {
							CombatManager.Instance.HasCollidedWithHostile (this.GetComponent<Avatar> (), other.gameObject.GetComponent<Avatar>());
						}
					}  
				}
			}
		}
	}
    void OnMouseEnter() {
        if (!UIManager.Instance.IsMouseOnUI()) {
            UIManager.Instance.ShowSmallInfo(this._trader.tradeEvent.name);
            this.HighlightPath();
        }
    }

    void OnMouseExit() {
        UIManager.Instance.HideSmallInfo();
        this.UnHighlightPath();
    }

    private void Update() {
        if (KingdomManager.Instance.useFogOfWar) {
            if (this._trader.location.currFogOfWarState == FOG_OF_WAR_STATE.VISIBLE) {
                gameObject.GetComponent<SpriteRenderer>().enabled = true;
            } else {
                gameObject.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
    }

    void OnDestroy() {
		RemoveBehaviourTree();
        this.UnHighlightPath();
        UpdateFogOfWar(true);
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
    private bool IsTraderDead() {
        return this._trader.citizen.isDead;
    }

    [Task]
    private bool IsTradeEventDone() {
        return !this._trader.tradeEvent.isActive;
    }

    [Task]
    private bool HasTraderReachedTarget() {
        if (this._trader.location == this._trader.targetLocation) {
            this._trader.tradeEvent.DoneCitizenAction(this._trader.citizen);
            return true;
        }
        return false;
    }

//    [Task]
//    private bool HasTraderCollidedWithHostileGeneral() {
//        if (this.collidedWithHostile) {
//            this.collidedWithHostile = false;
//            this._trader.tradeEvent.KillTrader();
//            return true;
//        }
//        return false;
//    }

    [Task]
    private void MoveToNextTile() {
        Move();
        Task.current.Succeed();
    }
    #endregion

    private void MakeCitizenMove(HexTile startTile, HexTile targetTile) {
        if (startTile.transform.position.x <= targetTile.transform.position.x) {
            if (this.animator.gameObject.transform.localScale.x > 0) {
                this.animator.gameObject.transform.localScale = new Vector3(this.animator.gameObject.transform.localScale.x * -1, this.animator.gameObject.transform.localScale.y, this.animator.gameObject.transform.localScale.z);
            }
        } else {
            if (this.animator.gameObject.transform.localScale.x < 0) {
                this.animator.gameObject.transform.localScale = new Vector3(this.animator.gameObject.transform.localScale.x * -1, this.animator.gameObject.transform.localScale.y, this.animator.gameObject.transform.localScale.z);
            }
        }

        if (startTile.transform.position.y < targetTile.transform.position.y) {
            this.direction = DIRECTION.UP;
            this.animator.Play("Walk_Up");
        } else if (startTile.transform.position.y > targetTile.transform.position.y) {
            this.direction = DIRECTION.DOWN;
            this.animator.Play("Walk_Down");
        } else {
            if (startTile.transform.position.x < targetTile.transform.position.x) {
                this.direction = DIRECTION.RIGHT;
                this.animator.Play("Walk_Right");
            } else {
                this.direction = DIRECTION.LEFT;
                this.animator.Play("Walk_Left");
            }
        }
        this.GetComponent<SmoothMovement>().direction = this.direction;
        this.GetComponent<SmoothMovement>().Move(targetTile.transform.position);
    }

    private void Move() {
        if (this._trader.targetLocation != null) {
            if (this._trader.path != null) {
                if (this._trader.path.Count > 0) {
                    this.MakeCitizenMove(this._trader.location, this._trader.path[0]);
                    this._trader.location = this._trader.path[0];
                    this._trader.citizen.currentLocation = this._trader.path[0];
                    this.UpdateFogOfWar();
                    this._trader.path.RemoveAt(0);
                    this._trader.location.CollectEventOnTile(this._trader.citizen.city.kingdom, this._trader.citizen);
                    this.CheckForKingdomDiscovery();
                }
            }
        }
    }

    private void CheckForKingdomDiscovery() {
        List<HexTile> tilesToCheck = new List<HexTile>();
        tilesToCheck.Add(this._trader.location);
        tilesToCheck.AddRange(visibleTiles);
        for (int i = 0; i < tilesToCheck.Count; i++) {
            HexTile currTile = tilesToCheck[i];
            if (currTile.isOccupied && currTile.ownedByCity != null &&
                currTile.ownedByCity.kingdom.id != this._trader.citizen.city.kingdom.id) {
                Kingdom thisKingdom = this._trader.citizen.city.kingdom;
                Kingdom otherKingdom = currTile.ownedByCity.kingdom;
				KingdomManager.Instance.DiscoverKingdom (thisKingdom, otherKingdom);
            } else if (currTile.isBorder) {
                Kingdom thisKingdom = this._trader.citizen.city.kingdom;
                Kingdom otherKingdom = CityGenerator.Instance.GetCityByID(currTile.isBorderOfCityID).kingdom;
                if (otherKingdom.id != this._trader.citizen.city.kingdom.id) {
					KingdomManager.Instance.DiscoverKingdom (thisKingdom, otherKingdom);
                }
            }
        }
    }

    private List<HexTile> visibleTiles;
    private void UpdateFogOfWar(bool forDeath = false) {
        for (int i = 0; i < visibleTiles.Count; i++) {
            HexTile currTile = visibleTiles[i];
            this._trader.citizen.city.kingdom.SetFogOfWarStateForTile(currTile, FOG_OF_WAR_STATE.SEEN);
        }
        visibleTiles.Clear();
        if (!forDeath) {
            visibleTiles.Add(this._trader.location);
            visibleTiles.AddRange(this._trader.location.AllNeighbours);
            for (int i = 0; i < visibleTiles.Count; i++) {
                HexTile currTile = visibleTiles[i];
                this._trader.citizen.city.kingdom.SetFogOfWarStateForTile(currTile, FOG_OF_WAR_STATE.VISIBLE);
            }
        }

    }

    private void HighlightPath() {
        this.pathToUnhighlight.Clear();
        for (int i = 0; i < this._trader.path.Count; i++) {
            this._trader.path[i].highlightGO.SetActive(true);
            this.pathToUnhighlight.Add(this._trader.path[i]);
        }
    }

    private void UnHighlightPath() {
        for (int i = 0; i < this.pathToUnhighlight.Count; i++) {
            this.pathToUnhighlight[i].highlightGO.SetActive(false);
        }
    }

}
