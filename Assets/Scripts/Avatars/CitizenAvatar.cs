using UnityEngine;
using System.Collections;
using Panda;
using System.Collections.Generic;

public class CitizenAvatar : MonoBehaviour {
    public Role citizenRole;
    public PandaBehaviour pandaBehaviour;
    public Animator animator;
    public bool collidedWithHostile;
    public General otherGeneral;

    private bool _hasArrived = false;
    private List<HexTile> pathToUnhighlight = new List<HexTile>();

    internal DIRECTION direction;

    #region getters/setters
    public bool hasArrived {
        get { return _hasArrived; }
    }
    #endregion

    #region virtuals
    internal virtual void Init(Role citizenRole) {
        this.citizenRole = citizenRole;
        this.direction = DIRECTION.LEFT;
        this.GetComponent<Avatar>().kingdom = this.citizenRole.citizen.city.kingdom;
        this.GetComponent<Avatar>().gameEvent = this.citizenRole.gameEventInvolvedIn;
        this.GetComponent<Avatar>().citizen = this.citizenRole.citizen;
        visibleTiles = new List<HexTile>();

        ResetValues();
        AddBehaviourTree();
    }
    #endregion


    private void ResetValues() {
        this.collidedWithHostile = false;
        this.otherGeneral = null;
    }

    internal void SetHasArrivedState(bool state) {
        _hasArrived = state;
    }

    internal void Move() {
        if (this.citizenRole.targetLocation != null) {
            if (this.citizenRole.path != null) {
                if (this.citizenRole.path.Count > 0) {
                    this.MakeCitizenMove(this.citizenRole.location, this.citizenRole.path[0]);
                    this.citizenRole.location = this.citizenRole.path[0];
                    this.citizenRole.citizen.currentLocation = this.citizenRole.path[0];
                    this.UpdateFogOfWar();
                    this.citizenRole.path.RemoveAt(0);
                    this.citizenRole.location.CollectEventOnTile(this.citizenRole.citizen.city.kingdom, this.citizenRole.citizen);
                    this.CheckForKingdomDiscovery();
                }
            }
        }
    }
    private void CheckForKingdomDiscovery() {
        HexTile currentLocation = this.citizenRole.location;
        if (currentLocation.isOccupied && currentLocation.ownedByCity != null &&
            currentLocation.ownedByCity.kingdom.id != this.citizenRole.citizen.city.kingdom.id) {
            Kingdom thisKingdom = this.citizenRole.citizen.city.kingdom;
            Kingdom otherKingdom = currentLocation.ownedByCity.kingdom;
            thisKingdom.DiscoverKingdom(otherKingdom);
            otherKingdom.DiscoverKingdom(thisKingdom);
        } else if (currentLocation.isBorder) {
            Kingdom thisKingdom = this.citizenRole.citizen.city.kingdom;
            Kingdom otherKingdom = CityGenerator.Instance.GetCityByID(currentLocation.isBorderOfCityID).kingdom;
            if (otherKingdom.id != this.citizenRole.citizen.city.kingdom.id) {
                thisKingdom.DiscoverKingdom(otherKingdom);
                otherKingdom.DiscoverKingdom(thisKingdom);
            }
        }
    }

    private List<HexTile> visibleTiles;
    private void UpdateFogOfWar(bool forDeath = false) {
        for (int i = 0; i < visibleTiles.Count; i++) {
            HexTile currTile = visibleTiles[i];
            this.citizenRole.citizen.city.kingdom.SetFogOfWarStateForTile(currTile, FOG_OF_WAR_STATE.SEEN);
        }
        visibleTiles.Clear();
        if (!forDeath) {
            visibleTiles.Add(this.citizenRole.location);
            visibleTiles.AddRange(this.citizenRole.location.AllNeighbours);
            for (int i = 0; i < visibleTiles.Count; i++) {
                HexTile currTile = visibleTiles[i];
                this.citizenRole.citizen.city.kingdom.SetFogOfWarStateForTile(currTile, FOG_OF_WAR_STATE.VISIBLE);
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



    private void HighlightPath() {
        this.pathToUnhighlight.Clear();
        for (int i = 0; i < this.citizenRole.path.Count; i++) {
            this.citizenRole.path[i].highlightGO.SetActive(true);
            this.pathToUnhighlight.Add(this.citizenRole.path[i]);
        }
    }

    private void UnHighlightPath() {
        for (int i = 0; i < this.pathToUnhighlight.Count; i++) {
            this.pathToUnhighlight[i].highlightGO.SetActive(false);
        }
    }

    public void OnEndAttack() {
        this.citizenRole.gameEventInvolvedIn.DoneCitizenAction(this.citizenRole.citizen);
        this.citizenRole.DestroyGO();
    }

    internal void HasAttacked() {
        this.GetComponent<SmoothMovement>().hasAttacked = true;
    }

    #region BehaviourTree Functions
    internal void AddBehaviourTree() {
        BehaviourTreeManager.Instance.allTrees.Add(this.pandaBehaviour);
    }

    internal void RemoveBehaviourTree() {
        BehaviourTreeManager.Instance.allTrees.Remove(this.pandaBehaviour);
    }
    #endregion

    #region Monobehaviour Functions
    private void OnMouseEnter() {
        if (!UIManager.Instance.IsMouseOnUI()) {
            UIManager.Instance.ShowSmallInfo(this.citizenRole.gameEventInvolvedIn.name);
            this.HighlightPath();
        }
    }

    private void OnMouseExit() {
        UIManager.Instance.HideSmallInfo();
        this.UnHighlightPath();
    }

    private void FixedUpdate() {
        if (citizenRole.location.currFogOfWarState == FOG_OF_WAR_STATE.VISIBLE) {
            gameObject.GetComponent<SpriteRenderer>().enabled = true;
        } else {
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    private void OnDestroy() {
        RemoveBehaviourTree();
        UnHighlightPath();
        UpdateFogOfWar(true);
    }
    #endregion
}
