using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EZObjectPools;
using System;

public class CharacterAvatar : PooledObject{

    //public delegate void OnPathFinished();
    //public OnPathFinished onPathFinished;
    private Action onPathFinished;

	[SerializeField] protected SmoothMovement smoothMovement;
	[SerializeField] protected DIRECTION direction;

	protected List<ECS.Character> _characters;

	protected ILocation _specificLocation;
    protected ILocation targetLocation;
    protected bool _startCombatOnReachLocation;

	protected List<HexTile> path;

	protected bool _hasArrived = false;
    private bool _isInitialized = false;
    private bool _isMovementPaused = false;
    private bool _isTravelling = false;
	private bool _isMovingToHex = false;
	private Action queuedAction = null;

    #region getters/setters
    public List<ECS.Character> characters {
        get { return _characters; }
    }
	public ILocation specificLocation {
		get { return _specificLocation; }
    }
    public bool isTravelling {
        get { return _isTravelling; }
    }
	public bool isMovingToHex {
		get { return _isMovingToHex; }
	}
    #endregion

    internal virtual void Init(ECS.Character character) {
        this.smoothMovement.avatarGO = this.gameObject;
        _characters = new List<ECS.Character>();
        AddNewCharacter(character);
		_specificLocation = character.specificLocation;
        this.smoothMovement.onMoveFinished += OnMoveFinished;
        _isInitialized = true;
    }
    internal virtual void Init(Party party) {
        this.smoothMovement.avatarGO = this.gameObject;
        _characters = new List<ECS.Character>();
        for (int i = 0; i < party.partyMembers.Count; i++) {
            AddNewCharacter(party.partyMembers[i]);
        }
		_specificLocation = party.specificLocation;
        this.smoothMovement.onMoveFinished += OnMoveFinished;
        _isInitialized = true;
    }

    #region For Testing
    [ContextMenu("Log Characters")]
    public void LogPartyMembers() {
        Debug.Log("========== Characters ==========");
        if (characters[0].party != null) {
            Debug.Log("Party: " + characters[0].party.name);
        }
        for (int i = 0; i < characters.Count; i++) {
            ECS.Character currMember = characters[i];
            Debug.Log(currMember.name);
        }
    }
    #endregion

	#region Monobehaviour
	public void OnMouseDown(){
		if (UIManager.Instance.IsMouseOnUI()){
			return;
		}
		if(characters[0].party != null){
			UIManager.Instance.ShowCharacterInfo (characters [0].party.partyLeader);
		}else{
			UIManager.Instance.ShowCharacterInfo (characters [0]);
		}
	}
	#endregion

    #region ECS.Character Management
    public void AddNewCharacter(ECS.Character character) {
        if (!_characters.Contains(character)) {
            _characters.Add(character);
            character.SetAvatar(this);
        }
    }
    public void RemoveCharacter(ECS.Character character) {
        _characters.Remove(character);
        character.SetAvatar(null);
		if(_characters.Count <= 0){
			DestroyObject ();
		}
    }
    #endregion

    #region Pathfinding
    internal void SetTarget(ILocation target, bool startCombatOnReachLocation = false) {
        targetLocation = target;
        _startCombatOnReachLocation = startCombatOnReachLocation;
    }
    internal void StartPath(PATHFINDING_MODE pathFindingMode, Action actionOnPathFinished = null) {
        if (smoothMovement.isMoving) {
            smoothMovement.ForceStopMovement();
        }
        if (this.targetLocation != null) {
            SetHasArrivedState(false);
            onPathFinished = actionOnPathFinished;
            //if(actionOnPathFinished != null) {
            //    onPathFinished += actionOnPathFinished;
            //}
			Faction faction = _characters[0].faction;
            if (_characters[0].party != null) {
				faction = _characters[0].party.partyLeader.faction;
            }
			PathGenerator.Instance.CreatePath(this, this.specificLocation.tileLocation, targetLocation.tileLocation, pathFindingMode, faction);
            
            //this.path = PathGenerator.Instance.GetPath(this.currLocation, this.targetLocation, pathFindingMode, faction);
            //NewMove();
        }
    }
    internal virtual void ReceivePath(List<HexTile> path) {
        if (!_isInitialized) {
            return;
        }
        if (path != null && path.Count > 0) {
			if (this.specificLocation.tileLocation == null) {
                throw new Exception("Curr location of avatar is null! Is Initialized: " + _isInitialized.ToString());
            }
			if(this.specificLocation.tileLocation.landmarkOnTile != null){
				if(_characters[0].party != null){
					this.specificLocation.tileLocation.landmarkOnTile.AddHistory (_characters [0].party.name + " left.");
				}else{
					this.specificLocation.tileLocation.landmarkOnTile.AddHistory (_characters [0].name + " left.");
//					for (int i = 0; i < _characters.Count; i++) {
//						this.specificLocation.tileLocation.landmarkOnTile.AddHistory (_characters [i].name + " left.");
//					}
				}
			}

            this.path = path;
            _isTravelling = true;
            NewMove();
        }
    }
    internal virtual void NewMove() {
		if(_characters[0].party != null){
			if(_characters[0].party.isInCombat){
				_characters[0].party.SetCurrentFunction (() => NewMove ());
				return;
			}
		}else{
			if(_characters[0].isInCombat){
				_characters[0].SetCurrentFunction (() => NewMove ());
				return;
			}
		}

        if (this.targetLocation != null) {
            if (this.path != null) {
                if (this.path.Count > 0) {
                    //RemoveCharactersFromLocation(this.currLocation); //TODO: Only remove once character has actually exited the tile
					this.MakeCitizenMove(this.specificLocation.tileLocation, this.path[0]);
                }
            }
        }
    }
    internal void MakeCitizenMove(HexTile startTile, HexTile targetTile) {
		_isMovingToHex = true;
        this.smoothMovement.Move(targetTile.transform.position, this.direction);
    }
    /*
     This is called each time the avatar traverses a node in the
     saved path.
         */
    internal virtual void OnMoveFinished() {
//		if(!_isInitialized){
//			return;
//		}
		_isMovingToHex = false;
		if(this.path == null){
			Debug.LogError (GameManager.Instance.Today ().ToStringDate());
			Debug.LogError ("Character: " + _characters [0].name + ", " + _characters[0].specificLocation.locationName);
			if(_characters[0].party != null){
				Debug.LogError ("Party: " + _characters [0].party.name + ", " + _characters[0].party.specificLocation.locationName);
			}
			Debug.LogError ("Location: " + specificLocation.locationName);
		}
        if (this.path.Count > 0) {
			RemoveCharactersFromLocation(this.specificLocation);
            AddCharactersToLocation(this.path[0]);

			_specificLocation = this.path[0];
            this.path.RemoveAt(0);
        }
        //RevealRoads();
        //RevealLandmarks();
        HasArrivedAtTargetLocation();
    }
    internal virtual void HasArrivedAtTargetLocation() {
		if (this.specificLocation.tileLocation == targetLocation.tileLocation) {
            if (!this._hasArrived) {
                _isTravelling = false;
                AddCharactersToLocation(targetLocation, _startCombatOnReachLocation);
				_specificLocation = targetLocation; //set location as the target location, in case the target location is a landmark
				if (this.specificLocation.tileLocation.landmarkOnTile != null){
					string historyText = "Visited landmark ";
					if (this.specificLocation.tileLocation.landmarkOnTile is Settlement) {
						historyText = "Arrived at settlement ";
					}
						
					if(_characters[0].party != null){
						this.specificLocation.tileLocation.landmarkOnTile.AddHistory (_characters [0].party.name + " visited.");
					}else{
						this.specificLocation.tileLocation.landmarkOnTile.AddHistory (_characters [0].name + " visited.");
					}
					for (int i = 0; i < _characters.Count; i++) {
						_characters [i].AddHistory (historyText + this.specificLocation.tileLocation.landmarkOnTile.landmarkName + ".");
					}
				}
                SetHasArrivedState(true);
                if(onPathFinished != null) {
                    onPathFinished();
                }
            }
			if(queuedAction != null){
				queuedAction ();
				queuedAction = null;
			}
            _characters[0].DestroyAvatar(); //Destroy this avatar once it reaches it's destination
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
    internal void SetHasArrivedState(bool state) {
        _hasArrived = state;
    }
    internal void PauseMovement() {
        _isMovementPaused = true;
        smoothMovement.ForceStopMovement();
    }
    internal void ResumeMovement() {
        _isMovementPaused = false;
        NewMove();
    }
    #endregion

    #region Utilities
    /*
     This will set the avatar reference of all characters
     using this avatar to null, then return this object back to the pool.
         */
    public void DestroyObject() {
        for (int i = 0; i < _characters.Count; i++) {
            ECS.Character currCharacter = _characters[i];
            currCharacter.SetAvatar(null);
        }
        ObjectPoolManager.Instance.DestroyObject(this.gameObject);
    }
    private void RevealRoads() {
		this.specificLocation.tileLocation.SetRoadState(true);
    }
    protected void RemoveCharactersFromLocation(ILocation location) {
		if(_characters[0].party == null){
			location.RemoveCharacterFromLocation(_characters[0]);
//			for (int i = 0; i < _characters.Count; i++) {
//				ECS.Character currCharacter = _characters[i];
//				location.RemoveCharacterFromLocation(currCharacter);
//			}
		}else{
			location.RemoveCharacterFromLocation(_characters[0].party);
		}
        
		UIManager.Instance.UpdateHexTileInfo();
        UIManager.Instance.UpdateSettlementInfo();
    }
	protected void AddCharactersToLocation(ILocation location, bool startCombatOnReachLocation = true) {
		if(_characters[0].party == null){
			location.AddCharacterToLocation(_characters[0], startCombatOnReachLocation);
//			for (int i = 0; i < _characters.Count; i++) {
//				ECS.Character currCharacter = _characters[i];
//				location.AddCharacterToLocation(currCharacter, startCombatOnReachLocation);
//			}
		}else{
			location.AddCharacterToLocation(_characters[0].party, startCombatOnReachLocation);
		}

		UIManager.Instance.UpdateHexTileInfo();
        UIManager.Instance.UpdateSettlementInfo();
    }
	public void SetQueuedAction(Action action){
		queuedAction = action;
	}
    #endregion

    #region overrides
    public override void Reset() {
        base.Reset();
        smoothMovement.Reset();
        onPathFinished = null;
        direction = DIRECTION.LEFT;
		_specificLocation = null;
        targetLocation = null;
        path = null;
        _hasArrived = false;
        _isInitialized = false;
    }
    #endregion
}
