using UnityEngine;
using System.Collections;
using Panda;
using System.Collections.Generic;
using System.Linq;

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
                    this.citizenRole.path.RemoveAt(0);
                    if (this.citizenRole is Adventurer) {
                        GetNextTargetTile();
                    }
                    this.UpdateFogOfWar();
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
        if (KingdomManager.Instance.useFogOfWar) {
            if (citizenRole.location.currFogOfWarState == FOG_OF_WAR_STATE.VISIBLE) {
                gameObject.GetComponent<SpriteRenderer>().enabled = true;
            } else {
                gameObject.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
    }

    private void OnDestroy() {
        RemoveBehaviourTree();
        UnHighlightPath();
        UpdateFogOfWar(true);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Avatar") {
            if (this.gameObject != null && other.gameObject != null) {
                if (other.gameObject.GetComponent<Avatar>().kingdom.id != this.citizenRole.citizen.city.kingdom.id) {
                    if (!other.gameObject.GetComponent<Avatar>().citizen.isDead) {
                        CombatManager.Instance.HasCollidedWithHostile(this.GetComponent<Avatar>(), other.gameObject.GetComponent<Avatar>());
                    }
                }
            }
        } else if (other.tag == "Trader") {
            if (this.gameObject != null && other.gameObject != null) {
                Kingdom kingdomOfGeneral = this.citizenRole.citizen.city.kingdom;
                Kingdom kingdomOfTrader = other.gameObject.GetComponent<Avatar>().kingdom;
                if (kingdomOfGeneral.id != kingdomOfTrader.id) {
                    RelationshipKings relOfGeneralWithTrader = kingdomOfGeneral.king.GetRelationshipWithCitizen(kingdomOfTrader.king);
                    RelationshipKings relOfTraderWithGeneral = kingdomOfTrader.king.GetRelationshipWithCitizen(kingdomOfGeneral.king);
                    if (relOfGeneralWithTrader.lordRelationship == RELATIONSHIP_STATUS.ENEMY || relOfGeneralWithTrader.lordRelationship == RELATIONSHIP_STATUS.RIVAL ||
                        relOfTraderWithGeneral.lordRelationship == RELATIONSHIP_STATUS.ENEMY || relOfTraderWithGeneral.lordRelationship == RELATIONSHIP_STATUS.RIVAL) {
                        if (!other.gameObject.GetComponent<Avatar>().citizen.isDead) {
                            CombatManager.Instance.HasCollidedWithHostile(this.GetComponent<Avatar>(), other.gameObject.GetComponent<Avatar>());
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region Unique Functions
    //Adventurer
    private void GetNextTargetTile() {
        Kingdom kingdomOfAdventurer = this.citizenRole.citizen.city.kingdom;
        List<HexTile> tilesToChooseFrom = this.citizenRole.location.AllNeighbours.Where(x => x.elevationType != ELEVATION.WATER && kingdomOfAdventurer.fogOfWar[x.xCoordinate, x.yCoordinate] != FOG_OF_WAR_STATE.VISIBLE).ToList();
        if (tilesToChooseFrom.Count <= 0) {
            tilesToChooseFrom = this.citizenRole.location.AllNeighbours.Where(x => x.elevationType != ELEVATION.WATER).ToList();
        }
        List<HexTile> hexTilesWithEvents = tilesToChooseFrom.Where(x => x.gameEventInTile != null).ToList();
        if (hexTilesWithEvents.Count > 0) {
            tilesToChooseFrom = hexTilesWithEvents;
        }

        HexTile newTargetTile = tilesToChooseFrom[Random.Range(0, tilesToChooseFrom.Count)];
        if(newTargetTile != null) {
            this.citizenRole.targetLocation = newTargetTile;
            this.citizenRole.path = PathGenerator.Instance.GetPath(this.citizenRole.location, this.citizenRole.targetLocation, PATHFINDING_MODE.AVATAR);
            this.citizenRole.daysBeforeMoving = this.citizenRole.path[0].movementDays;
        } else {
            Debug.LogError("Adventurer from " + this.citizenRole.citizen.city.kingdom.name + " could not find a new target tile! Current location is " + this.citizenRole.location.name);
            this.citizenRole.gameEventInvolvedIn.DoneEvent();
        }
    }
    #endregion
}
