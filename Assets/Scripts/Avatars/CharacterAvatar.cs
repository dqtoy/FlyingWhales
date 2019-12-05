using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EZObjectPools;
using System;


public class CharacterAvatar : MonoBehaviour {

    private Action onPathFinished;
    private Action onPathStarted;
    private Action onPathCancelled;
    private Action onArriveAction;

    private PathFindingThread _currPathfindingRequest; //the current pathfinding request this avatar is waiting for

	[SerializeField] protected SmoothMovement smoothMovement;
	[SerializeField] protected DIRECTION direction;
    [SerializeField] protected GameObject _avatarHighlight;
    [SerializeField] protected GameObject _avatarVisual;
    [SerializeField] protected SpriteRenderer _avatarSpriteRenderer;
    [SerializeField] protected SpriteRenderer _frameSpriteRenderer;
    [SerializeField] protected SpriteRenderer _centerSpriteRenderer;

    protected Party _party;

    public Region targetLocation { get; protected set; }
    public LocationStructure targetStructure { get; protected set; }
    public LocationGridTile targetTile { get; protected set; }
    public IPointOfInterest targetPOI { get; protected set; }
    public bool placeCharacterAsTileObject { get; private set; }

    [SerializeField] protected List<HexTile> path;

	[SerializeField] private bool _hasArrived = false;
    [SerializeField] private bool _isInitialized = false;
    [SerializeField] private bool _isMovementPaused = false;
    [SerializeField] private bool _isTravelling = false;
    [SerializeField] private bool _isTravellingOutside = false;
    private int _distanceToTarget;
    private bool _isVisualShowing;
    private bool _isTravelCancelled;
    private PATHFINDING_MODE _pathfindingMode;
    private TravelLine _travelLine;
    private Action queuedAction = null;

    public CharacterPortrait characterPortrait { get; private set; }
    #region getters/setters
    public Party party {
        get { return _party; }
    }
    public bool isTravelling {
        get { return _isTravelling; }
    }
    public bool isTravellingOutside {
        get { return _isTravellingOutside; } //if the character is travelling from area to area, as oppose to only travelling inside area map
    }
    public bool isVisualShowing {
        get {
            if (_isVisualShowing) {
                return _isVisualShowing;
            } else {
                //check if this characters current location area is being tracked
                if (party.specificLocation != null ) { //&& party.specificLocation.isBeingTracked
                    return true;
                }
            }
            return _isVisualShowing;
        }
    }
    public TravelLine travelLine {
        get { return _travelLine; }
    }
    #endregion

    public virtual void Init(Party party) {
        _party = party;
        //SetPosition(_party.specificLocation.tileLocation.transform.position);
        this.smoothMovement.avatarGO = this.gameObject;
        this.smoothMovement.onMoveFinished += OnMoveFinished;
        _isInitialized = true;
        _hasArrived = true;
        SetVisualState(true);
        SetSprite(_party.owner.role.roleType);
        SetIsPlaceCharacterAsTileObject(true);

        this.name = party.owner.name + "'s Avatar";

#if !WORLD_CREATION_TOOL
        GameObject portraitGO = UIManager.Instance.InstantiateUIObject(CharacterManager.Instance.characterPortraitPrefab.name, this.transform);
        characterPortrait = portraitGO.GetComponent<CharacterPortrait>();
        characterPortrait.GeneratePortrait(_party.owner);
        portraitGO.SetActive(false);

        CharacterManager.Instance.AddCharacterAvatar(this);
#endif
        //Messenger.AddListener(Signals.TOGGLE_CHARACTERS_VISIBILITY, OnToggleCharactersVisibility);
        //Messenger.AddListener(Signals.INSPECT_ALL, OnInspectAll);
        //Messenger.AddListener<CharacterToken>(Signals.CHARACTER_TOKEN_ADDED, OnCharacterTokenObtained);
    }

    #region Monobehaviour
    private void OnDestroy() {
        //Messenger.RemoveListener(Signals.INSPECT_ALL, OnInspectAll);
        //if (Messenger.eventTable.ContainsKey(Signals.CHARACTER_TOKEN_ADDED)) {
            //Messenger.RemoveListener<CharacterToken>(Signals.CHARACTER_TOKEN_ADDED, OnCharacterTokenObtained);
        //}
        //Messenger.RemoveListener(Signals.TOGGLE_CHARACTERS_VISIBILITY, OnToggleCharactersVisibility);
        if (_isTravelling) {
            CancelledDeparture();
        }
#if !WORLD_CREATION_TOOL
        CharacterManager.Instance.RemoveCharacterAvatar(this);
#endif
    }
    #endregion

    #region Pathfinding
    public void SetTarget(Region target, LocationStructure structure, IPointOfInterest poi, LocationGridTile tile) {
        targetLocation = target;
        targetStructure = structure;
        targetPOI = poi;
        targetTile = tile;
    }
    public void SetOnPathFinished(Action action) {
        onPathFinished = action;
    }
    public void SetOnArriveAction(Action action) {
        onArriveAction = action;
    }
    public void StartPath(PATHFINDING_MODE pathFindingMode, Action actionOnPathFinished = null, Action actionOnPathStart = null) {
        //if (smoothMovement.isMoving) {
        //    smoothMovement.ForceStopMovement();
        //}
        //     Reset();
        //     if (this.targetLocation != null) {
        //         SetHasArrivedState(false);
        //         _pathfindingMode = pathFindingMode;
        //         _trackTarget = trackTarget;
        //         onPathFinished = actionOnPathFinished;
        //         onPathReceived = actionOnPathReceived;
        //Faction faction = _party.faction;
        //_currPathfindingRequest = PathGenerator.Instance.CreatePath(this, _party.specificLocation.tileLocation, targetLocation.tileLocation, pathFindingMode, faction);
        //     }
        Reset();
        if (targetLocation != null) {
            SetOnPathFinished(actionOnPathFinished);
            StartTravelling();
            actionOnPathStart?.Invoke();
        }
    }
    public void CancelTravel(Action onCancelTravel = null) {
        if (_isTravelling && !_isTravelCancelled) {
            _isTravelCancelled = true;
            onPathCancelled = onCancelTravel;
            Messenger.RemoveListener(Signals.TICK_STARTED, TraverseCurveLine);
            Messenger.AddListener(Signals.TICK_STARTED, ReduceCurveLine);
        }
    }
    private void StartTravelling() {
        SetIsTravellingOutside(true);
        _party.owner.SetPOIState(POI_STATE.INACTIVE);
        if (_party.isCarryingAnyPOI) {
            _party.carriedPOI.SetPOIState(POI_STATE.INACTIVE);
        }
        ////for (int i = 0; i < _party.characters.Count; i++) {
        ////    _party.characters[i].SetPOIState(POI_STATE.INACTIVE);
        ////}
        _distanceToTarget = PathGenerator.Instance.GetTravelTime(_party.specificLocation.coreTile, targetLocation.coreTile);
        _travelLine = _party.specificLocation.coreTile.CreateTravelLine(targetLocation.coreTile, _distanceToTarget, _party.owner);
        _travelLine.SetActiveMeter(isVisualShowing);
        _party.owner.marker.gameObject.SetActive(false);
        Messenger.AddListener(Signals.TICK_STARTED, TraverseCurveLine);
        Messenger.Broadcast(Signals.PARTY_STARTED_TRAVELLING, this.party);
    }
    private void TraverseCurveLine() {
        if (_travelLine == null) {
            Messenger.RemoveListener(Signals.TICK_STARTED, TraverseCurveLine);
            return;
        }
        if (_travelLine.isDone) {
            Messenger.RemoveListener(Signals.TICK_STARTED, TraverseCurveLine);
            ArriveAtLocation();
            return;
        }
        _travelLine.AddProgress();
    }
    private void ReduceCurveLine() {
        if (_travelLine.isDone) {
            Messenger.RemoveListener(Signals.TICK_STARTED, ReduceCurveLine);
            CancelTravelDeparture();
            return;
        }
        _travelLine.ReduceProgress();
    }
    private void CancelTravelDeparture() {
        CancelledDeparture();
        if(onPathCancelled != null) {
            onPathCancelled();
        }
    }
    private void CancelledDeparture() {
        if(_travelLine != null) {
            SetIsTravelling(false);
            _isTravelCancelled = false;
            _travelLine.travelLineParent.RemoveChild(_travelLine);
            GameObject.Destroy(_travelLine.gameObject);
            _travelLine = null;
        }
    }
    private void ArriveAtLocation() {
        SetIsTravelling(false);
        SetIsTravellingOutside(false);
        _travelLine.travelLineParent.RemoveChild(_travelLine);
        GameObject.Destroy(_travelLine.gameObject);
        _travelLine = null;
        SetHasArrivedState(true);
        _party.specificLocation.RemoveCharacterFromLocation(_party);
        targetLocation.AddCharacterToLocation(_party.owner);

        _party.owner.marker.ClearHostilesInRange();
        _party.owner.marker.ClearAvoidInRange();
        _party.owner.marker.ClearPOIsInVisionRange();

        //place marker at edge tile of target location
        if (targetLocation.area != null) {
            LocationGridTile entrance = targetLocation.area.GetRandomUnoccupiedEdgeTile();
            _party.owner.marker.PlaceMarkerAt(entrance);
        }
        //_party.owner.marker.gameObject.SetActive(true);

        _party.owner.marker.pathfindingAI.SetIsStopMovement(true);
        //Debug.Log(GameManager.Instance.TodayLogString() + _party.name + " has arrived at " + targetLocation.name + " on " + _party.owner.gridTileLocation.ToString());
        Log arriveLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "arrive_location");
        _party.owner.SetPOIState(POI_STATE.ACTIVE);
        arriveLog.AddToFillers(_party.owner, _party.owner.name, LOG_IDENTIFIER.CHARACTER_LIST_1, false);
        if (_party.isCarryingAnyPOI) {
            arriveLog.AddToFillers(_party.carriedPOI, _party.carriedPOI.name, LOG_IDENTIFIER.CHARACTER_LIST_1, false);
        }
        arriveLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_1);
        arriveLog.AddLogToInvolvedObjects();
        //if (_party.characters.Count > 0) {
        //    for (int i = 0; i < _party.characters.Count; i++) {
        //        Character character = party.characters[i];
        //        character.SetPOIState(POI_STATE.ACTIVE);
        //        //character.SetDailyInteractionGenerationTick();
        //        arriveLog.AddToFillers(character, character.name, LOG_IDENTIFIER.CHARACTER_LIST_1, false);
        //    }
        //    arriveLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_1);
        //    arriveLog.AddLogToInvolvedObjects();
        //}

        Messenger.Broadcast(Signals.PARTY_DONE_TRAVELLING, this.party);
        if(onArriveAction != null) {
            onArriveAction();
            SetOnArriveAction(null);
        }
        if (targetStructure != null) {
            _party.owner.MoveToAnotherStructure(targetStructure, targetTile, targetPOI, onPathFinished);
        } else {
            if(onPathFinished != null) {
                onPathFinished();
                SetOnPathFinished(null);
            }
        }
    }
    public virtual void ReceivePath(List<HexTile> path, PathFindingThread fromThread) {
        if (!_isInitialized) {
            return;
        }
        if (_currPathfindingRequest == null) {
            return; //this avatar currently has no pathfinding request
        } else {
            if (_currPathfindingRequest != fromThread) {
                return; //the current pathfinding request and the thread that returned the path are not the same
            }
        }
        if (path == null) {
            Debug.LogError(_party.name + ". There is no path from " + _party.specificLocation.name + " to " + targetLocation.name, this);
            return;
        }
        if (path != null && path.Count > 0) {
            this.path = path;
            _currPathfindingRequest = null;
            SetIsTravelling(true);
            //if(_party.specificLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) {
            //    _party.specificLocation.coreTile.landmarkOnTile.landmarkVisual.OnCharacterExitedLandmark(_party);
            //}
            NewMove();
            if(onPathStarted != null) {
                onPathStarted();
            }
        }
    }
    public virtual void NewMove() {
        if (this.targetLocation != null && this.path != null) {
            if (this.path.Count > 0) {
				//this.MakeCitizenMove(_party.specificLocation.tileLocation, this.path[0]);
    //            if(_party.specificLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) {
    //                RemoveCharactersFromLocation(_party.specificLocation);
    //            }
                //AddCharactersToLocation(this.path[0]);
                //this.path.RemoveAt(0);
            }
        }
    }
    /*
     This is called each time the avatar traverses a node in the
     saved path.
         */
    public virtual void OnMoveFinished() {
		if(this.path == null){
			Debug.LogError (GameManager.Instance.Today ().ToStringDate());
			Debug.LogError ("Location: " + _party.specificLocation.name);
		}
        //if (_trackTarget != null) {
        //    if(_trackTarget.currentParty.specificLocation.id != targetLocation.id) {
        //        _party.GoToLocation(_trackTarget.currentParty.specificLocation.coreTile, _pathfindingMode, onPathFinished, _trackTarget, onPathReceived);
        //        return;
        //    }
        //}
        if (this.path.Count > 0) {
            //if(_party.specificLocation.locIdentifier == LOCATION_IDENTIFIER.HEXTILE) {
            //    RemoveCharactersFromLocation(_party.specificLocation);
            //}
            //AddCharactersToLocation(this.path[0]);
            this.path.RemoveAt(0);
        }
        HasArrivedAtTargetLocation();
    }
    public virtual void HasArrivedAtTargetLocation() {
		if (_party.specificLocation.coreTile.id == targetLocation.coreTile.id) {
            if (!this._hasArrived) {
                SetIsTravelling(false);
                //_trackTarget = null;
                SetHasArrivedState(true);
                targetLocation.AddCharacterToLocation(_party.owner);
                //Debug.Log(_party.name + " has arrived at " + targetLocation.name + " on " + GameManager.Instance.continuousDays);
                ////Every time the party arrives at home, check if it still not ruined
                //if(_party.mainCharacter.homeLandmark.specificLandmarkType == LANDMARK_TYPE.CAMP) {
                //    //Check if the location the character arrived at is the character's home landmark
                //    if (targetLocation.tileLocation.id == _party.mainCharacter.homeLandmark.tileLocation.id) {
                //        //Check if the current landmark in the location is a camp and it is not yet ruined
                //        if (targetLocation.tileLocation.landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.CAMP) {
                //            Character character = _party.mainCharacter;
                //            if (targetLocation.tileLocation.landmarkOnTile.landmarkObj.currentState.stateName != "Ruined") {
                //                //Make it the character's new home landmark
                //                _party.mainCharacter.homeLandmark.RemoveCharacterHomeOnLandmark(character);
                //                targetLocation.tileLocation.landmarkOnTile.AddCharacterHomeOnLandmark(character);
                //            } else {
                //                //Create new camp
                //                BaseLandmark newCamp = targetLocation.tileLocation.areaOfTile.CreateCampOnTile(targetLocation.tileLocation);
                //                _party.mainCharacter.homeLandmark.RemoveCharacterHomeOnLandmark(character);
                //                newCamp.AddCharacterHomeOnLandmark(character);
                //            }
                //        }
                //    }
                //}
                if(onPathFinished != null) {
                    onPathFinished();
                }
            }
			if(queuedAction != null){
				queuedAction ();
				queuedAction = null;
				return;
			}
		}else{
			if(queuedAction != null){
				queuedAction ();
				queuedAction = null;
				return;
			}
            if (!_isMovementPaused) {
                NewMove();
            }
		}
    }
    public void SetHasArrivedState(bool state) {
        _hasArrived = state;
    }
    public void SetIsTravelling(bool state) {
        _isTravelling = state;
    }
    public void SetIsTravellingOutside(bool state) {
        _isTravellingOutside = state;
    }
    public void SetIsPlaceCharacterAsTileObject(bool state) {
        placeCharacterAsTileObject = state;
    }
    #endregion

    #region Utilities
    public void SetVisualState(bool state) {
        _isVisualShowing = state;
        if(_travelLine != null) {
            _travelLine.SetActiveMeter(isVisualShowing);
        }
    }
    public void SetHighlightState(bool state) {
        _avatarHighlight.SetActive(state);
    }
    public void SetPosition(Vector3 position) {
        this.transform.position = position;
    }
    public void SetSprite(CHARACTER_ROLE role){
		Sprite sprite = CharacterManager.Instance.GetSpriteByRole (role);
		if(sprite != null){
			_avatarSpriteRenderer.sprite = sprite;
		}
	}
    public void SetFrameOrderLayer(int layer) {
        _frameSpriteRenderer.sortingOrder = layer;
    }
    public void SetCenterOrderLayer(int layer) {
        _centerSpriteRenderer.sortingOrder = layer;
    }
    #endregion

    #region overrides
    public void Reset() {
        //base.Reset();
        smoothMovement.Reset();
        SetOnPathFinished(null);
        onPathStarted = null;
        direction = DIRECTION.LEFT;
        //targetLocation = null;
        path = null;
        _isMovementPaused = false;
        _hasArrived = false;
        _isTravelCancelled = false;
        //_trackTarget = null;
        //_isInitialized = false;
        _currPathfindingRequest = null;
        SetHighlightState(false);
    }
    #endregion


}
