using UnityEngine;
using System.Collections;
using Panda;
using System.Collections.Generic;
using System.Linq;
using EZObjectPools;

public class CitizenAvatar : PooledObject {
    [SerializeField] private int citizenID;
    [SerializeField] internal string citizenName;
    [SerializeField] private string roleType;
	[SerializeField] public Collider2D colliderForInteraction;

    public Role citizenRole;
//    public PandaBehaviour pandaBehaviour;
//    public Animator animator;
    public bool collidedWithHostile;
    public General otherGeneral;

    private bool currAvatarState = true;

    [SerializeField] private bool _hasArrived = false;
    private List<HexTile> pathToUnhighlight = new List<HexTile>();

    private Transform[] childObjects;

    [SerializeField] internal DIRECTION direction;
	internal List<HexTile> visibleTiles;
	public SmoothMovement smoothMovement;

    #region getters/setters
    public bool hasArrived {
        get { return _hasArrived; }
    }
    #endregion

	void Awake(){
//		this.smoothMovement = this.animator.GetComponent<SmoothMovement> ();
		this.smoothMovement.avatarGO = this.gameObject;
	}

    #region virtuals
    internal virtual void Init(Role citizenRole) {
		this.citizenRole = citizenRole;
        this.citizenID = citizenRole.citizen.id;
        this.citizenName = citizenRole.citizen.name;
        this.roleType = citizenRole.ToString();
        this.direction = DIRECTION.LEFT;
		this.citizenRole.location.EnterCitizen (this.citizenRole.citizen);
        this.smoothMovement.onMoveFinished += OnMoveFinished;
        visibleTiles = new List<HexTile>();
        childObjects = Utilities.GetComponentsInDirectChildren<Transform>(this.gameObject);
		SetHasArrivedState (false);
        if(citizenRole.location.currFogOfWarState == FOG_OF_WAR_STATE.VISIBLE) {
            SetAvatarState(true);
        } else {
            SetAvatarState(false);
        }

        ResetValues();
//        AddBehaviourTree();
		UpdateUI ();
//		StartMoving ();
    }
	internal void CreatePath(PATHFINDING_MODE pathFindingMode){
		if(this.citizenRole.targetLocation != null){
			PathGenerator.Instance.CreatePath (this, this.citizenRole.location, this.citizenRole.targetLocation, pathFindingMode, BASE_RESOURCE_TYPE.STONE, this.citizenRole.citizen.city.kingdom);
		}
	}
	internal virtual void ReceivePath(List<HexTile> path){
		if(path != null && path.Count > 0){
			this.citizenRole.path = path;
			if(!this.citizenRole.isIdle){
				StartMoving ();
			}
		}else{
			CancelEventInvolvedIn ();
		}
	}
	internal void StartMoving(){
		NewMove ();
	}
    internal virtual void Move() {
        if (this.citizenRole.targetLocation != null) {
            if (this.citizenRole.path != null) {
                if (this.citizenRole.path.Count > 0) {
                    this.MakeCitizenMove(this.citizenRole.location, this.citizenRole.path[0]);
                    this.citizenRole.location = this.citizenRole.path[0];
                    this.citizenRole.citizen.currentLocation = this.citizenRole.path[0];
                    this.citizenRole.path.RemoveAt(0);
                }
            }
        }
    }
	internal virtual void NewMove() {
		if (this.citizenRole.targetLocation != null) {
			if (this.citizenRole.path != null) {
				if (this.citizenRole.path.Count > 0) {
					this.citizenRole.location.ExitCitizen (this.citizenRole.citizen);
					this.MakeCitizenMove(this.citizenRole.location, this.citizenRole.path[0]);
				}else{
					CancelEventInvolvedIn ();
				}
			}
		}
	}

    internal virtual void OnMoveFinished() {
//		Debug.LogError (this.citizenRole.role.ToString() + " " + this.citizenName + " END DAY: " + GameManager.Instance.days);
		if(this.citizenRole.path.Count > 0){
			this.citizenRole.location = this.citizenRole.path[0];
			this.citizenRole.citizen.currentLocation = this.citizenRole.path[0];
			this.citizenRole.path.RemoveAt(0);
			this.citizenRole.location.EnterCitizen (this.citizenRole.citizen);
		}

        //this.CheckForKingdomDiscovery();
//        this.UpdateFogOfWar();
//        this.transform.SetParent(this.citizenRole.location.transform);
//        this.transform.localPosition = Vector3.zero;
        if(this.citizenRole.location.currFogOfWarState == FOG_OF_WAR_STATE.VISIBLE) {
            if (!currAvatarState) {
                SetAvatarState(true);
            }
        } else {
            if (currAvatarState) {
                SetAvatarState(false);
            }
        }

		HasArrivedAtTargetLocation ();
		if(!this.hasArrived){
			NewMove ();
		}
	}

	internal virtual void HasArrivedAtTargetLocation(){
		if (this.citizenRole.location == this.citizenRole.targetLocation) {
			if (!this.hasArrived) {
				SetHasArrivedState(true);
				this.citizenRole.Attack ();
			}
		}
	}

	public virtual void UpdateFogOfWar(bool forDeath = false) {
		Kingdom kingdomOfAgent = this.citizenRole.citizen.city.kingdom;

        for (int i = 0; i < visibleTiles.Count; i++) {
			HexTile currTile = visibleTiles[i];
            if(kingdomOfAgent.regionFogOfWarDict[currTile.region] != FOG_OF_WAR_STATE.VISIBLE) {
                kingdomOfAgent.SetFogOfWarStateForTile(currTile, FOG_OF_WAR_STATE.SEEN);
            }
            //if (currTile.visibleByKingdoms.Count > 0) {
            //    if (!currTile.visibleByKingdoms.Contains(kingdomOfAgent)) {
            //        kingdomOfAgent.SetFogOfWarStateForTile(currTile, FOG_OF_WAR_STATE.SEEN);
            //    }
            //} else {
            //    kingdomOfAgent.SetFogOfWarStateForTile(currTile, FOG_OF_WAR_STATE.SEEN);
            //}
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

	internal virtual void UpdateUI(){}
    #endregion

    #region overrides
    public override void Reset() {
        base.Reset();
		this.EnableCollider (false);
//        UpdateFogOfWar(true);
        ResetValues();
//        ResetBehaviourTree();
//        RemoveBehaviourTree();
        UnHighlightPath();
//        SetHasArrivedState(false);
        //this.citizenRole = null;
//        animator.Rebind();
        this.direction = DIRECTION.LEFT;
		this.smoothMovement.Reset();
//        this.GetComponent<BoxCollider2D>().enabled = true;
        visibleTiles = new List<HexTile>();
        //childObjects = Utilities.GetComponentsInDirectChildren<Transform>(this.gameObject);
    }
    #endregion

    private void ResetValues() {
        this.collidedWithHostile = false;
        this.otherGeneral = null;
    }

    internal void SetHasArrivedState(bool state) {
        _hasArrived = state;
    }

    /*
     * Check if the agent has discovered a new kingdom
     * */
    internal void CheckForKingdomDiscovery() {
        List<HexTile> tilesToCheck = new List<HexTile>();
        tilesToCheck.Add(this.citizenRole.location);
        tilesToCheck.AddRange(visibleTiles);
        Kingdom thisKingdom = this.citizenRole.citizen.city.kingdom;

        List<City> citiesSeen = new List<City>();

        for (int i = 0; i < tilesToCheck.Count; i++) {
            HexTile currTile = tilesToCheck[i];
            if (currTile.isOccupied && currTile.ownedByCity != null &&
                currTile.ownedByCity.kingdom.id != thisKingdom.id) {
                City otherCity = currTile.ownedByCity;
                if (!citiesSeen.Contains(otherCity)) {
                    citiesSeen.Add(otherCity);
                }
                Kingdom otherKingdom = currTile.ownedByCity.kingdom;
                if (otherKingdom.id != thisKingdom.id && !thisKingdom.discoveredKingdoms.Contains(otherKingdom)) {
                    KingdomManager.Instance.DiscoverKingdom(thisKingdom, otherKingdom);
                }
            } else if (currTile.isBorder) {
                for (int j = 0; j < currTile.isBorderOfCities.Count; j++) {
                    City otherCity = currTile.isBorderOfCities[j];
                    Kingdom otherKingdom = otherCity.kingdom;
                    if (otherKingdom.id != thisKingdom.id) {
                        if (!citiesSeen.Contains(otherCity)) {
                            citiesSeen.Add(otherCity);
                        }
                        if (!thisKingdom.discoveredKingdoms.Contains(otherKingdom)) {
                            KingdomManager.Instance.DiscoverKingdom(thisKingdom, otherKingdom);
                        }
                    }
                }
            }
        }

        for (int i = 0; i < citiesSeen.Count; i++) {
            City currCity = citiesSeen[i];
            //thisKingdom.DiscoverCity(currCity);
            //Debug.Log("Citizen of " + thisKingdom.name + " has seen " + currCity.name);
//            List<HexTile> tilesToSetAsSeen = currCity.ownedTiles.Union(currCity.borderTiles).ToList();
//            for (int j = 0; j < tilesToSetAsSeen.Count; j++) {
//                HexTile currTile = tilesToSetAsSeen[j];
//                if (!currTile.visibleByKingdoms.Contains(thisKingdom)) {
//                    thisKingdom.SetFogOfWarStateForTile(currTile, FOG_OF_WAR_STATE.SEEN);
//                }
//            }
			foreach (var tileToSetAsSeen in currCity.ownedTiles.Union(currCity.borderTiles)) {
				HexTile currTile = tileToSetAsSeen;
				if (!currTile.visibleByKingdoms.Contains(thisKingdom)) {
					thisKingdom.SetFogOfWarStateForTile(currTile, FOG_OF_WAR_STATE.SEEN);
				}
			}
        }
    }

    internal void MakeCitizenMove(HexTile startTile, HexTile targetTile) {
//        startTile.ExitCitizen(this.citizenRole.citizen);
//        targetTile.EnterCitizen(this.citizenRole.citizen);

//        if (startTile.transform.position.x <= targetTile.transform.position.x) {
//            if (this.animator.gameObject.transform.localScale.x > 0) {
//                this.animator.gameObject.transform.localScale = new Vector3(this.animator.gameObject.transform.localScale.x * -1, this.animator.gameObject.transform.localScale.y, this.animator.gameObject.transform.localScale.z);
//            }
//        } else {
//            if (this.animator.gameObject.transform.localScale.x < 0) {
//                this.animator.gameObject.transform.localScale = new Vector3(this.animator.gameObject.transform.localScale.x * -1, this.animator.gameObject.transform.localScale.y, this.animator.gameObject.transform.localScale.z);
//            }
//        }
//        if (startTile.transform.position.y < targetTile.transform.position.y) {
//            this.direction = DIRECTION.UP;
//            this.animator.Play("Walk_Up");
//        } else if (startTile.transform.position.y > targetTile.transform.position.y) {
//            this.direction = DIRECTION.DOWN;
//            this.animator.Play("Walk_Down");
//        } else {
//            if (startTile.transform.position.x < targetTile.transform.position.x) {
//                this.direction = DIRECTION.RIGHT;
//                this.animator.Play("Walk_Right");
//            } else {
//                this.direction = DIRECTION.LEFT;
//                this.animator.Play("Walk_Left");
//            }
//        }
		this.smoothMovement.Move(targetTile.transform.position, this.direction);
		this.UpdateUI ();
    }

    private void HighlightPath() {
        this.pathToUnhighlight.Clear();
		if(this.citizenRole.path != null){
			for (int i = 0; i < this.citizenRole.path.Count; i++) {
				this.citizenRole.path[i].highlightGO.SetActive(true);
				this.pathToUnhighlight.Add(this.citizenRole.path[i]);
			}
		}
    }

    private void UnHighlightPath() {
        for (int i = 0; i < this.pathToUnhighlight.Count; i++) {
            this.pathToUnhighlight[i].highlightGO.SetActive(false);
        }
    }

    public void EndAttack() {
		HasAttacked();
//      this.citizenRole.DestroyGO();
		if(this.citizenRole.gameEventInvolvedIn != null){
			this.citizenRole.gameEventInvolvedIn.DoneCitizenAction(this.citizenRole.citizen);
		}else{
			this.citizenRole.ArrivedAtTargetLocation ();
		}
    }

	public void CancelEventInvolvedIn(){
//		this.citizenRole.DestroyGO();
		if(this.citizenRole.gameEventInvolvedIn != null){
			this.citizenRole.gameEventInvolvedIn.CancelEvent();
		}
	}

    internal void HasAttacked() {
		this.citizenRole.hasAttacked = true;
    }

    #region BehaviourTree Functions
//    internal void AddBehaviourTree() {
//        //BehaviourTreeManager.Instance.allTrees.Add(this.pandaBehaviour);
//        Messenger.AddListener("OnDayEnd", this.pandaBehaviour.Tick);
//    }
//
//    internal void RemoveBehaviourTree() {
//        //BehaviourTreeManager.Instance.allTrees.Remove(this.pandaBehaviour);
//        Messenger.RemoveListener("OnDayEnd", this.pandaBehaviour.Tick);
//    }
//
//    internal void ResetBehaviourTree() {
//        this.pandaBehaviour.Reset();
//    }
    #endregion

    #region Monobehaviour Functions
    private void OnMouseEnter() {
//        if (!UIManager.Instance.IsMouseOnUI()) {
//            UIManager.Instance.ShowSmallInfo(this.citizenRole.gameEventInvolvedIn.name);
//            this.HighlightPath();
//        }
    }

    private void OnMouseExit() {
        UIManager.Instance.HideSmallInfo();
        this.UnHighlightPath();
    }

    //private void Update() {
    //    if (KingdomManager.Instance.useFogOfWar) {
    //        bool state = true;
    //        if (citizenRole.location.currFogOfWarState == FOG_OF_WAR_STATE.VISIBLE) {
    //            state = true;
    //        } else {
    //            state = false;
    //        }
    //        this.gameObject.GetComponent<SpriteRenderer>().enabled = state;
    //        for (int i = 0; i < childObjects.Length; i++) {
				//if(childObjects[i].GetComponent<Animator>() != null){
				//	childObjects[i].GetComponent<SpriteRenderer>().enabled = state;
				//}else{
				//	childObjects[i].gameObject.SetActive(state);
				//}
    //        } 
    //    }
    //}

    //private void OnDestroy() {
    //    RemoveBehaviourTree();
    //    UnHighlightPath();
    //}

	#region Collision
    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Avatar") {
            if (this.gameObject != null && other.gameObject != null) {
				Citizen otherAgent = other.gameObject.GetComponent<CitizenAvatar>().citizenRole.citizen;
				if (!otherAgent.isDead) {
					CollisionManager.Instance.HasCollided (this.citizenRole, otherAgent.assignedRole);
//					if (kingdomOfThis.isDead) {
//						this.citizenRole.gameEventInvolvedIn.CancelEvent ();
//					} else if (kingdomOfOther.isDead) {
//						otherAgent.assignedRole.gameEventInvolvedIn.CancelEvent ();
//					}
				}
            }
        } else if (other.tag == "Trader") {
            if (this.gameObject != null && other.gameObject != null) {
				Citizen otherAgent = other.gameObject.GetComponent<CitizenAvatar>().citizenRole.citizen;
				if(!otherAgent.isDead){
					Kingdom kingdomOfGeneral = this.citizenRole.citizen.city.kingdom;
					Kingdom kingdomOfTrader = other.gameObject.GetComponent<CitizenAvatar>().citizenRole.citizen.city.kingdom;
					if (kingdomOfGeneral.id != kingdomOfTrader.id) {
						KingdomRelationship relOfGeneralWithTrader = kingdomOfGeneral.GetRelationshipWithKingdom(kingdomOfTrader);
						KingdomRelationship relOfTraderWithGeneral = kingdomOfTrader.GetRelationshipWithKingdom(kingdomOfGeneral);
						if (relOfGeneralWithTrader.relationshipStatus == RELATIONSHIP_STATUS.HATE || relOfGeneralWithTrader.relationshipStatus == RELATIONSHIP_STATUS.SPITE ||
							relOfTraderWithGeneral.relationshipStatus == RELATIONSHIP_STATUS.HATE || relOfTraderWithGeneral.relationshipStatus == RELATIONSHIP_STATUS.SPITE) {
//							CombatManager.Instance.HasCollidedWithHostile(this.citizenRole, otherAgent.assignedRole);
						}
					}
				}
            }
        }
    }
	#endregion

    #endregion

    public void SetAvatarState(bool state) {
        currAvatarState = state;
//        this.gameObject.GetComponent<SpriteRenderer>().enabled = state;
        for (int i = 0; i < childObjects.Length; i++) {
            if (childObjects[i].GetComponent<Animator>() != null) {
                childObjects[i].GetComponent<SpriteRenderer>().enabled = state;
            } else {
                childObjects[i].gameObject.SetActive(state);
            }
        }
    }

	internal void EnableCollider(bool state){
		if(this.colliderForInteraction != null){
			this.colliderForInteraction.enabled = state;
		}
	}
}
