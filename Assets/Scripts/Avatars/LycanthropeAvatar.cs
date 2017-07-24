using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Panda;

public class LycanthropeAvatar : MonoBehaviour {
    public Lycanthrope lycanthrope;
    public PandaBehaviour pandaBehaviour;
    public Animator animator;
    public bool collidedWithHostile;
    public General otherGeneral;

    private bool hasArrived = false;

    private bool isWaitingForFullMoon = false;

    private List<HexTile> pathToUnhighlight = new List<HexTile>();

    internal DIRECTION direction;

    internal void Init(Lycanthrope lycanthrope) {
        this.lycanthrope = lycanthrope;
        this.direction = DIRECTION.LEFT;
        this.GetComponent<Avatar>().kingdom = this.lycanthrope.citizen.city.kingdom;
        this.GetComponent<Avatar>().gameEvent = this.lycanthrope.lycanthropyEvent;
        this.GetComponent<Avatar>().citizen = this.lycanthrope.citizen;

        ResetValues();
        this.AddBehaviourTree();
    }
    //	void OnTriggerEnter2D(Collider2D other){
    //		if(other.tag == "General"){
    //			this.collidedWithHostile = false;
    //			if(this.gameObject != null && other.gameObject != null){
    //				if(other.gameObject.GetComponent<GeneralAvatar>().general.citizen.city.kingdom.id != this.raider.citizen.city.kingdom.id){
    //					if(!other.gameObject.GetComponent<GeneralAvatar> ().general.citizen.isDead){
    //						this.collidedWithHostile = true;
    //						this.otherGeneral = other.gameObject.GetComponent<GeneralAvatar> ().general;
    //					}
    //				}
    //			}
    //		}
    //	}
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

    #region Behaviour Tree Tasks
    [Task]
    public void IsThereCitizen() {
        if (this.lycanthrope.citizen != null) {
            Task.current.Succeed();
        } else {
            Task.current.Fail();
        }
    }
    [Task]
    public void IsThereEvent() {
        if (this.lycanthrope.lycanthropyEvent != null) {
            Task.current.Succeed();
        } else {
            Task.current.Fail();
        }
    }

    [Task]
    public void HasArrivedAtTargetHextile() {
        if (this.lycanthrope.location == this.lycanthrope.targetLocation) {
            if (!this.hasArrived) {
                this.hasArrived = true;
                this.isWaitingForFullMoon = true;
                this.lycanthrope.avatar.SetActive(false);
                //this.lycanthrope.Attack();
            }
            Task.current.Succeed();
        } else {
            Task.current.Fail();
        }

    }

    [Task]
    public void HasDiedOfOtherReasons() {
        if (this.lycanthrope.citizen.isDead) {
            //Citizen has died
            this.lycanthrope.lycanthropyEvent.DeathByOtherReasons();
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

    [Task]
    protected bool HasTargetCity() {
        if(this.lycanthrope.targetCity == null) {
            return false;
        }
        return true;
    }

    [Task]
    protected void GetTargetCity() {
        this.lycanthrope.lycanthropyEvent.GetTargetCity();
        if(this.lycanthrope.targetCity != null) {
            this.hasArrived = false;
            this.lycanthrope.avatar.SetActive(true);
            Task.current.Succeed();
        } else {
            Task.current.Fail();
        }
    }

    [Task]
    protected bool IsWaitingForFullMoon() {
        return isWaitingForFullMoon;
    }

    [Task]
    protected bool IsFullMoon() {
        if(Utilities.GetMoonPhase(GameManager.Instance.year, GameManager.Instance.month, GameManager.Instance.days) == 4) {
            return true;
        }
        return false;
    }

    [Task]
    protected void Attack() {
        this.lycanthrope.avatar.SetActive(true);
        this.lycanthrope.Attack();
        this.isWaitingForFullMoon = false;
        Task.current.Succeed();
    }
    #endregion


    private void ResetValues() {
        this.collidedWithHostile = false;
        this.otherGeneral = null;
    }

    private void Move() {
        if (this.lycanthrope.targetLocation != null) {
            if (this.lycanthrope.path != null) {
                if (this.lycanthrope.path.Count > 0) {
                    this.MakeCitizenMove(this.lycanthrope.location, this.lycanthrope.path[0]);
                    this.lycanthrope.location = this.lycanthrope.path[0];
                    this.lycanthrope.citizen.currentLocation = this.lycanthrope.path[0];
                    this.lycanthrope.path.RemoveAt(0);
                    //this.CheckForKingdomDiscovery();
                }
            }
        }
    }

    //private void CheckForKingdomDiscovery() {
    //    HexTile currentLocation = this.raider.location;
    //    if (currentLocation.isOccupied && currentLocation.ownedByCity != null &&
    //        currentLocation.ownedByCity.kingdom.id != this.raider.citizen.city.kingdom.id) {
    //        Kingdom thisKingdom = this.raider.citizen.city.kingdom;
    //        Kingdom otherKingdom = currentLocation.ownedByCity.kingdom;
    //        thisKingdom.DiscoverKingdom(otherKingdom);
    //        otherKingdom.DiscoverKingdom(thisKingdom);
    //    } else if (currentLocation.isBorder) {
    //        Kingdom thisKingdom = this.raider.citizen.city.kingdom;
    //        Kingdom otherKingdom = CityGenerator.Instance.GetCityByID(currentLocation.isBorderOfCityID).kingdom;
    //        if (otherKingdom.id != this.raider.citizen.city.kingdom.id) {
    //            thisKingdom.DiscoverKingdom(otherKingdom);
    //            otherKingdom.DiscoverKingdom(thisKingdom);
    //        }
    //    }
    //}

    //private List<HexTile> visibleTiles;
    //private void UpdateFogOfWar() {
    //    for (int i = 0; i < visibleTiles.Count; i++) {
    //        HexTile currTile = visibleTiles[i];
    //        this.lycanthrope.citizen.city.kingdom.SetFogOfWarStateForTile(currTile, FOG_OF_WAR_STATE.SEEN);
    //    }
    //    visibleTiles.Clear();
    //    visibleTiles.Add(this.lycanthrope.location);
    //    visibleTiles.AddRange(this.lycanthrope.location.AllNeighbours);
    //    for (int i = 0; i < visibleTiles.Count; i++) {
    //        HexTile currTile = visibleTiles[i];
    //        this.lycanthrope.citizen.city.kingdom.SetFogOfWarStateForTile(currTile, FOG_OF_WAR_STATE.VISIBLE);
    //    }
    //}

    internal void AddBehaviourTree() {
        BehaviourTreeManager.Instance.allTrees.Add(this.pandaBehaviour);
    }

    internal void RemoveBehaviourTree() {
        BehaviourTreeManager.Instance.allTrees.Remove(this.pandaBehaviour);
    }


    void OnMouseEnter() {
        if (!UIManager.Instance.IsMouseOnUI()) {
            UIManager.Instance.ShowSmallInfo(this.lycanthrope.lycanthropyEvent.name);
            this.HighlightPath();
        }
    }

    void OnMouseExit() {
        UIManager.Instance.HideSmallInfo();
        this.UnHighlightPath();
    }

    private void FixedUpdate() {
        if (this.lycanthrope.location.currFogOfWarState == FOG_OF_WAR_STATE.VISIBLE) {
            gameObject.GetComponent<SpriteRenderer>().enabled = true;
        } else {
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    void HighlightPath() {
        this.pathToUnhighlight.Clear();
        for (int i = 0; i < this.lycanthrope.path.Count; i++) {
            this.lycanthrope.path[i].highlightGO.SetActive(true);
            this.pathToUnhighlight.Add(this.lycanthrope.path[i]);
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
    }

    public void OnEndAttack() {
        this.lycanthrope.lycanthropyEvent.DoneCitizenAction(this.lycanthrope.citizen);
        //this.lycanthrope.DestroyGO();
        this.lycanthrope.avatar.SetActive(false);
    }

    internal void HasAttacked() {
        this.GetComponent<SmoothMovement>().hasAttacked = true;
    }

}
