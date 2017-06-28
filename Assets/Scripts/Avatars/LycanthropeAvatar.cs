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
                this.lycanthrope.Attack();
            }
            Task.current.Succeed();
        } else {
            Task.current.Fail();
        }

    }

    //	[Task]
    //	public void HasCollidedWithHostileGeneral(){
    //		if(this.collidedWithHostile){
    //			this.collidedWithHostile = false;
    //			if(!this.otherGeneral.citizen.isDead){
    //				//Death by general
    //				this.raider.raid.DeathByGeneral (this.otherGeneral);
    //				Task.current.Succeed ();
    //			}else{
    //				Task.current.Fail ();
    //			}
    //
    //		}else{
    //			Task.current.Fail ();
    //		}
    //	}
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
        BehaviourTreeManager.Instance.allTrees.Remove(this.pandaBehaviour);
        UnHighlightPath();
    }

    public void OnEndAttack() {
        this.lycanthrope.lycanthropyEvent.DoneCitizenAction(this.lycanthrope.citizen);
        this.lycanthrope.DestroyGO();
    }

    internal void HasAttacked() {
        this.GetComponent<SmoothMovement>().hasAttacked = true;
    }
}
