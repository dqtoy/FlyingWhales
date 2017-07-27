using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Panda;

public class HealerAvatar : MonoBehaviour {
    public Healer healer;
    public PandaBehaviour pandaBehaviour;
    public Animator animator;
    public bool collidedWithHostile;
    public General otherGeneral;

    private bool hasArrived = false;
    private List<HexTile> pathToUnhighlight = new List<HexTile>();

    internal DIRECTION direction;

    internal void Init(Healer healer) {
        this.healer = healer;
        this.direction = DIRECTION.LEFT;
        this.GetComponent<Avatar>().kingdom = this.healer.citizen.city.kingdom;
        this.GetComponent<Avatar>().gameEvent = this.healer.plagueEvent;
        this.GetComponent<Avatar>().citizen = this.healer.citizen;
        visibleTiles = new List<HexTile>();

        ResetValues();
        this.AddBehaviourTree();
    }
	void OnTriggerEnter2D(Collider2D other){
		if(other.tag == "Avatar"){
			if(this.gameObject != null && other.gameObject != null){
				if(other.gameObject.GetComponent<Avatar>().kingdom.id != this.healer.citizen.city.kingdom.id){
					if(!other.gameObject.GetComponent<Avatar> ().citizen.isDead){
						CombatManager.Instance.HasCollidedWithHostile (this.GetComponent<Avatar> (), other.gameObject.GetComponent<Avatar>());
					}
				}
			}
		}else if(other.tag == "Trader"){
			if(this.gameObject != null && other.gameObject != null){
				Kingdom kingdomOfGeneral = this.healer.citizen.city.kingdom;
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
    internal void MakeCitizenMove(HexTile startTile, HexTile targetTile) {
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
    private void StopMoving() {
        this.animator.Play("Idle");
    }

    [Task]
    public void IsThereCitizen() {
        if (this.healer.citizen != null) {
            Task.current.Succeed();
        } else {
            Task.current.Fail();
        }
    }
    [Task]
    public void IsThereEvent() {
        if (this.healer.plagueEvent != null) {
            Task.current.Succeed();
        } else {
            Task.current.Fail();
        }
    }

    [Task]
    public void HasArrivedAtTargetHextile() {
        if (this.healer.location == this.healer.targetLocation) {
            if (!this.hasArrived) {
                this.hasArrived = true;
                this.healer.Attack();
            }
            Task.current.Succeed();
        } else {
            Task.current.Fail();
        }

    }
    [Task]
    public void HasDiedOfOtherReasons() {
        if (this.healer.citizen.isDead) {
            //Citizen has died
            this.healer.plagueEvent.DeathByOtherReasons();
            Task.current.Succeed();
        } else {
            Task.current.Fail();
        }
    }
    [Task]
    public void MoveToNextTile() {
        Move();
        Task.current.Succeed();
    }

    private void ResetValues() {
        this.collidedWithHostile = false;
        this.otherGeneral = null;
    }

    private void Move() {
        if (this.healer.targetLocation != null) {
            if (this.healer.path != null) {
                if (this.healer.path.Count > 0) {
                    this.MakeCitizenMove(this.healer.location, this.healer.path[0]);
                    this.healer.location = this.healer.path[0];
                    this.healer.citizen.currentLocation = this.healer.path[0];
                    this.UpdateFogOfWar();
                    this.healer.path.RemoveAt(0);
                    this.healer.location.CollectEventOnTile(this.healer.citizen.city.kingdom, this.healer.citizen);
                    this.CheckForKingdomDiscovery();
                }
            }
        }
    }

    private void CheckForKingdomDiscovery() {
        HexTile currentLocation = this.healer.location;
        if (currentLocation.isOccupied && currentLocation.ownedByCity != null &&
            currentLocation.ownedByCity.kingdom.id != this.healer.citizen.city.kingdom.id) {
            Kingdom thisKingdom = this.healer.citizen.city.kingdom;
            Kingdom otherKingdom = currentLocation.ownedByCity.kingdom;
            thisKingdom.DiscoverKingdom(otherKingdom);
            otherKingdom.DiscoverKingdom(thisKingdom);
        } else if (currentLocation.isBorder) {
            Kingdom thisKingdom = this.healer.citizen.city.kingdom;
            Kingdom otherKingdom = CityGenerator.Instance.GetCityByID(currentLocation.isBorderOfCityID).kingdom;
            if (otherKingdom.id != this.healer.citizen.city.kingdom.id) {
                thisKingdom.DiscoverKingdom(otherKingdom);
                otherKingdom.DiscoverKingdom(thisKingdom);
            }
        }
    }

    private List<HexTile> visibleTiles;
    private void UpdateFogOfWar(bool forDeath = false) {
        for (int i = 0; i < visibleTiles.Count; i++) {
            HexTile currTile = visibleTiles[i];
            this.healer.citizen.city.kingdom.SetFogOfWarStateForTile(currTile, FOG_OF_WAR_STATE.SEEN);
        }
        visibleTiles.Clear();
        if (!forDeath) {
            visibleTiles.Add(this.healer.location);
            visibleTiles.AddRange(this.healer.location.AllNeighbours);
            for (int i = 0; i < visibleTiles.Count; i++) {
                HexTile currTile = visibleTiles[i];
                this.healer.citizen.city.kingdom.SetFogOfWarStateForTile(currTile, FOG_OF_WAR_STATE.VISIBLE);
            }
        }
        
    }

    internal void AddBehaviourTree() {
        BehaviourTreeManager.Instance.allTrees.Add(this.pandaBehaviour);
    }

    internal void RemoveBehaviourTree() {
        BehaviourTreeManager.Instance.allTrees.Remove(this.pandaBehaviour);
    }


    void OnMouseEnter() {
        if (!UIManager.Instance.IsMouseOnUI()) {
            UIManager.Instance.ShowSmallInfo(this.healer.plagueEvent.name);
            this.HighlightPath();
        }
    }

    void OnMouseExit() {
        UIManager.Instance.HideSmallInfo();
        this.UnHighlightPath();
    }

    private void Update() {
        if (KingdomManager.Instance.useFogOfWar) {
            if (this.healer.location.currFogOfWarState == FOG_OF_WAR_STATE.VISIBLE) {
                gameObject.GetComponent<SpriteRenderer>().enabled = true;
            } else {
                gameObject.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
    }

    void HighlightPath() {
        this.pathToUnhighlight.Clear();
        for (int i = 0; i < this.healer.path.Count; i++) {
            this.healer.path[i].highlightGO.SetActive(true);
            this.pathToUnhighlight.Add(this.healer.path[i]);
        }
    }

    void UnHighlightPath() {
        for (int i = 0; i < this.pathToUnhighlight.Count; i++) {
            this.pathToUnhighlight[i].highlightGO.SetActive(false);
        }
    }

    void OnDestroy() {
		RemoveBehaviourTree();
        UnHighlightPath();
        UpdateFogOfWar(true);
    }

    public void OnEndAttack() {
        this.healer.plagueEvent.DoneCitizenAction(this.healer.citizen);
        this.healer.DestroyGO();
    }

    internal void HasAttacked() {
        this.GetComponent<SmoothMovement>().hasAttacked = true;
    }
}
