using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EZObjectPools;
using System;


public class CharacterAvatar : MonoBehaviour{

    private Action onPathFinished;
    private Action onPathReceived;
    private Action onPathCancelled;

    private PathFindingThread _currPathfindingRequest; //the current pathfinding request this avatar is waiting for

	[SerializeField] protected SmoothMovement smoothMovement;
	[SerializeField] protected DIRECTION direction;
    [SerializeField] protected GameObject _avatarHighlight;
    [SerializeField] protected GameObject _avatarVisual;
    [SerializeField] protected SpriteRenderer _avatarSpriteRenderer;
    [SerializeField] protected SpriteRenderer _frameSpriteRenderer;
    [SerializeField] protected SpriteRenderer _centerSpriteRenderer;

    protected Party _party;

    public Area targetLocation { get; protected set; }
    public LocationStructure targetStructure { get; protected set; }

    [SerializeField] protected List<HexTile> path;

	[SerializeField] private bool _hasArrived = false;
    [SerializeField] private bool _isInitialized = false;
    [SerializeField] private bool _isMovementPaused = false;
    [SerializeField] private bool _isTravelling = false;
    [SerializeField] private bool _isMovingToHex = false;
    private int _distanceToTarget;
    private bool _isVisualShowing;
    private bool _isTravelCancelled;
    private PATHFINDING_MODE _pathfindingMode;
    //private Character _trackTarget = null;
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
    public bool isMovementPaused {
        get { return _isMovementPaused; }
    }
    public bool isMovingToHex {
		get { return _isMovingToHex; }
	}
    public bool hasArrived {
        get { return _hasArrived; }
    }
    public bool isVisualShowing {
        get {
            if (_isVisualShowing) {
                return _isVisualShowing;
            } else {
                //check if this characters current location area is being tracked
                if (party.specificLocation != null 
                    && party.specificLocation.isBeingTracked) {
                    return true;
                }
            }
            return _isVisualShowing;
        }
    }
    public GameObject avatarVisual {
        get { return _avatarVisual; }
    }
    #endregion

    public virtual void Init(Party party) {
        _party = party;
        //SetPosition(_party.specificLocation.tileLocation.transform.position);
        this.smoothMovement.avatarGO = this.gameObject;
        this.smoothMovement.onMoveFinished += OnMoveFinished;
        _isInitialized = true;
        _hasArrived = true;
        SetVisualState(false);
        if (_party.mainCharacter is CharacterArmyUnit) {
            _avatarSpriteRenderer.sprite = CharacterManager.Instance.villainSprite;
        } else {
            SetSprite(_party.mainCharacter.role.roleType);
        }
        //else if (_party.mainCharacter is MonsterArmyUnit) {
        //    _avatarSpriteRenderer.sprite = CharacterManager.Instance.villainSprite;
        //} else if (_party.mainCharacter is Monster) {
        //    SetSprite((_party.mainCharacter as Monster).type);
        //}
#if !WORLD_CREATION_TOOL
        GameObject portraitGO = UIManager.Instance.InstantiateUIObject(CharacterManager.Instance.characterPortraitPrefab.name, this.transform);
        characterPortrait = portraitGO.GetComponent<CharacterPortrait>();
        characterPortrait.GeneratePortrait(_party.mainCharacter);
        portraitGO.SetActive(false);

        CharacterManager.Instance.AddCharacterAvatar(this);
#endif
        //Messenger.AddListener(Signals.TOGGLE_CHARACTERS_VISIBILITY, OnToggleCharactersVisibility);
        Messenger.AddListener(Signals.INSPECT_ALL, OnInspectAll);
        Messenger.AddListener<CharacterToken>(Signals.CHARACTER_TOKEN_ADDED, OnCharacterTokenObtained);
    }

    #region Monobehaviour
    private void OnDestroy() {
        Messenger.RemoveListener(Signals.INSPECT_ALL, OnInspectAll);
        if (Messenger.eventTable.ContainsKey(Signals.CHARACTER_TOKEN_ADDED)) {
            Messenger.RemoveListener<CharacterToken>(Signals.CHARACTER_TOKEN_ADDED, OnCharacterTokenObtained);
        }
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
    public void SetTarget(Area target, LocationStructure structure) {
        targetLocation = target;
        targetStructure = structure;
    }
    public void StartPath(PATHFINDING_MODE pathFindingMode, Action actionOnPathFinished = null, Action actionOnPathReceived = null) {
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
            onPathFinished = actionOnPathFinished;
            StartTravelling();
        }
    }
    public void CancelTravel(Action onCancelTravel = null) {
        if (_isTravelling && !_isTravelCancelled) {
            _isTravelCancelled = true;
            onPathCancelled = onCancelTravel;
            Messenger.RemoveListener(Signals.DAY_STARTED, TraverseCurveLine);
            Messenger.AddListener(Signals.DAY_STARTED, ReduceCurveLine);
        }
    }
    private void StartTravelling() {
        _isTravelling = true;
        float distance = Vector3.Distance(_party.specificLocation.coreTile.transform.position, targetLocation.coreTile.transform.position);
        _distanceToTarget = (Mathf.CeilToInt(distance / 2.315188f)) * 2; //6
        _travelLine = _party.specificLocation.coreTile.CreateTravelLine(targetLocation.coreTile, _distanceToTarget);
        _travelLine.SetActiveMeter(isVisualShowing);
        Messenger.AddListener(Signals.DAY_STARTED, TraverseCurveLine);
        Messenger.Broadcast(Signals.PARTY_STARTED_TRAVELLING, this.party);
    }
    private void TraverseCurveLine() {
        if (_travelLine == null) {
            Messenger.RemoveListener(Signals.DAY_STARTED, TraverseCurveLine);
            return;
        }
        if (_travelLine.isDone) {
            Messenger.RemoveListener(Signals.DAY_STARTED, TraverseCurveLine);
            ArriveAtLocation();
            return;
        }
        _travelLine.AddProgress();
    }
    private void ReduceCurveLine() {
        if (_travelLine.isDone) {
            Messenger.RemoveListener(Signals.DAY_STARTED, ReduceCurveLine);
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
        _isTravelling = false;
        _isTravelCancelled = false;
        _travelLine.travelLineParent.RemoveChild(_travelLine);
        GameObject.Destroy(_travelLine.gameObject);
        _travelLine = null;
    }
    private void ArriveAtLocation() {
        _isTravelling = false;
        _travelLine.travelLineParent.RemoveChild(_travelLine);
        GameObject.Destroy(_travelLine.gameObject);
        _travelLine = null;
        SetHasArrivedState(true);
        _party.specificLocation.RemoveCharacterFromLocation(_party);
        targetLocation.AddCharacterToLocation(_party, targetStructure);
        Debug.Log(GameManager.Instance.TodayLogString() + _party.name + " has arrived at " + targetLocation.name + " on " + GameManager.Instance.continuousDays);
        if(_party.characters.Count > 0) {
            Log arriveLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "arrive_location");
            for (int i = 0; i < _party.characters.Count; i++) {
                Character character = party.characters[i];
                character.SetDailyInteractionGenerationTick();
                arriveLog.AddToFillers(character, character.name, LOG_IDENTIFIER.CHARACTER_LIST_1, false);
            }
            arriveLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_1);
            arriveLog.AddLogToInvolvedObjects();
        }

        if (onPathFinished != null) {
            onPathFinished();
        }
        Messenger.Broadcast(Signals.PARTY_DONE_TRAVELLING, this.party);
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
            _isTravelling = true;
            //if(_party.specificLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) {
            //    _party.specificLocation.coreTile.landmarkOnTile.landmarkVisual.OnCharacterExitedLandmark(_party);
            //}
            NewMove();
            if(onPathReceived != null) {
                onPathReceived();
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
  //  public void MakeCitizenMove(HexTile startTile, HexTile targetTile) {
		////CharacterHasLeftTile ();
		//_isMovingToHex = true;
  //      this.smoothMovement.Move(targetTile.transform.position, this.direction);
  //  }
    /*
     This is called each time the avatar traverses a node in the
     saved path.
         */
    public virtual void OnMoveFinished() {
		_isMovingToHex = false;
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
		if (_party.specificLocation.coreTile.id == targetLocation.id) {
            if (!this._hasArrived) {
                _isTravelling = false;
                //_trackTarget = null;
                SetHasArrivedState(true);
                targetLocation.AddCharacterToLocation(_party);
                Debug.Log(_party.name + " has arrived at " + targetLocation.name + " on " + GameManager.Instance.continuousDays);
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
    public void PauseMovement() {
        Debug.Log(_party.name + " has paused movement!");
        _isMovementPaused = true;
        smoothMovement.ForceStopMovement();
    }
    public void ResumeMovement() {
        Debug.Log(_party.name + " has resumed movement!");
        _isMovementPaused = false;
        NewMove();
    }
    public void AddActionOnPathFinished(Action action) {
        onPathFinished += action;
    }
    #endregion

    #region Utilities
    /*
     This will set the avatar reference of all characters
     using this avatar to null, then return this object back to the pool.
         */
    public void DestroyObject() {
        ObjectPoolManager.Instance.DestroyObject(this.gameObject);
    }
    public void ReclaimPortrait() {
        characterPortrait.transform.SetParent(this.transform);
        //(characterPortrait.transform as RectTransform).pivot = new Vector2(1f, 1f);
        characterPortrait.gameObject.SetActive(false);
    }
    public void SetVisualState(bool state) {
        _isVisualShowing = state;
        if(_travelLine != null) {
            _travelLine.SetActiveMeter(isVisualShowing);
        }
    }
    public void UpdateTravelLineVisualState() {
        if (_travelLine != null) {
            _travelLine.SetActiveMeter(isVisualShowing);
        }
    }
    public void UpdateVisualState() {
        if (GameManager.Instance.allCharactersAreVisible) {
            _avatarVisual.SetActive(isVisualShowing);
        } else {
            if (_party.IsPartyBeingInspected()) {
                _avatarVisual.SetActive(isVisualShowing);
            } else {
                _avatarVisual.SetActive(false);
            }
        }
    }
    public void SetQueuedAction(Action action){
		queuedAction = action;
	}
    public void SetHighlightState(bool state) {
        _avatarHighlight.SetActive(state);
    }
    public void SetPosition(Vector3 position) {
        this.transform.position = position;
    }
    //private void CharacterHasLeftTile(){
    //	LeaveCharacterTrace();
    //       CheckForItemDrop();
    //}
    public void SetSprite(CHARACTER_ROLE role){
		Sprite sprite = CharacterManager.Instance.GetSpriteByRole (role);
		if(sprite != null){
			_avatarSpriteRenderer.sprite = sprite;
		}
	}
    public void SetSprite(MONSTER_TYPE monsterType) {
        Sprite sprite = CharacterManager.Instance.GetSpriteByMonsterType(monsterType);
        if (sprite != null) {
            _avatarSpriteRenderer.sprite = sprite;
        }
    }
    public void SetMovementState(bool state) {
        smoothMovement.isHalted = state;
    }
    public void SetFrameOrderLayer(int layer) {
        _frameSpriteRenderer.sortingOrder = layer;
    }
    public void SetCenterOrderLayer(int layer) {
        _centerSpriteRenderer.sortingOrder = layer;
    }
    private void OnToggleCharactersVisibility() {
        UpdateVisualState();
    }
    #endregion

    #region overrides
    public void Reset() {
        //base.Reset();
        smoothMovement.Reset();
        onPathFinished = null;
        onPathReceived = null;
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

    #region Listeners
    private void OnCharacterTokenObtained(CharacterToken token) {
        if (_party.owner != null && _party.owner.characterToken == token) {
            SetVisualState(true);
        }
    }
    private void OnInspectAll() {
        if (GameManager.Instance.inspectAll) {
            SetVisualState(true);
        } else {
            if(_party.owner.minion != null || _party.owner.characterToken.isObtainedByPlayer) {
                SetVisualState(true);
            } else {
                SetVisualState(false);
            }
        }
    }
    #endregion


}
