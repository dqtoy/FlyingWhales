using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EZObjectPools;
using System;

public class CharacterAvatar : PooledObject{

    public delegate void OnPathFinished();
    public OnPathFinished onPathFinished;

	[SerializeField] protected SmoothMovement smoothMovement;
	[SerializeField] protected DIRECTION direction;

	protected List<ECS.Character> _characters;

	protected HexTile currLocation;
    protected HexTile targetLocation;

	protected List<HexTile> path;

	protected bool _hasArrived = false;

    internal virtual void Init(ECS.Character character) {
        this.smoothMovement.avatarGO = this.gameObject;
        _characters = new List<ECS.Character>();
        AddNewCharacter(character);
        this.currLocation = character.currLocation;
        this.smoothMovement.onMoveFinished += OnMoveFinished;
    }
    internal virtual void Init(Party party) {
        this.smoothMovement.avatarGO = this.gameObject;
        _characters = new List<ECS.Character>();
        for (int i = 0; i < party.partyMembers.Count; i++) {
            AddNewCharacter(party.partyMembers[i]);
        }
        this.currLocation = party.partyLeader.currLocation;
        this.smoothMovement.onMoveFinished += OnMoveFinished;
    }

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
    internal void SetTarget(HexTile target) {
        targetLocation = target;
    }
    internal void StartPath(PATHFINDING_MODE pathFindingMode, OnPathFinished actionOnPathFinished = null) {
        if (this.targetLocation != null) {
            SetHasArrivedState(false);
            onPathFinished = null;
            if(actionOnPathFinished != null) {
                onPathFinished += actionOnPathFinished;
            }
            PathGenerator.Instance.CreatePath(this, this.currLocation, this.targetLocation, pathFindingMode, BASE_RESOURCE_TYPE.STONE, null);
        }
    }
    internal virtual void ReceivePath(List<HexTile> path) {
        if (path != null && path.Count > 0) {
            this.path = path;
            NewMove();
        }
    }
    internal virtual void NewMove() {
        if (this.targetLocation != null) {
            if (this.path != null) {
                if (this.path.Count > 0) {
                    this.MakeCitizenMove(this.currLocation, this.path[0]);
                }
            }
        }
    }
    internal void MakeCitizenMove(HexTile startTile, HexTile targetTile) {
        this.smoothMovement.Move(targetTile.transform.position, this.direction);
    }
    /*
     This is called each time the avatar traverses a node in the
     saved path.
         */
    internal virtual void OnMoveFinished() {
        if (this.path.Count > 0) {
            if(this.currLocation.landmarkOnTile != null) {
                //Remove characters from previous landmark
                RemoveCharactersFromLandmark(this.currLocation.landmarkOnTile);
            }
            if (this.path[0].landmarkOnTile != null) {
                //Add characters to the landmark they are on
                AddCharactersToLandmark(this.path[0].landmarkOnTile);
            }
            this.currLocation = this.path[0];
            for (int i = 0; i < _characters.Count; i++) {
                ECS.Character currCharacter = _characters[i];
                currCharacter.SetLocation(this.currLocation);
            }
            this.path.RemoveAt(0);
        }
        RevealRoads();
        RevealLandmarks();

        HasArrivedAtTargetLocation();
    }
    internal virtual void HasArrivedAtTargetLocation() {
        if (this.currLocation == this.targetLocation) {
            if (!this._hasArrived) {
                SetHasArrivedState(true);
                if(onPathFinished != null) {
                    onPathFinished();
                }
            }
		}else{
			NewMove();
		}
    }
    internal void SetHasArrivedState(bool state) {
        _hasArrived = state;
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
        this.currLocation.SetRoadState(true);
    }
    private void RevealLandmarks() {
        if(this.currLocation.landmarkOnTile != null) {
            this.currLocation.landmarkOnTile.SetHiddenState(false);
        }
    }
    private void RemoveCharactersFromLandmark(BaseLandmark landmark) {
        for (int i = 0; i < _characters.Count; i++) {
            ECS.Character currCharacter = _characters[i];
            landmark.RemoveCharacterOnLandmark(currCharacter);
        }
        UIManager.Instance.UpdateSettlementInfo();
    }
    private void AddCharactersToLandmark(BaseLandmark landmark) {
        for (int i = 0; i < _characters.Count; i++) {
            ECS.Character currCharacter = _characters[i];
            landmark.AddCharacterOnLandmark(currCharacter);
        }
        UIManager.Instance.UpdateSettlementInfo();
    }
    #endregion

    #region overrides
    public override void Reset() {
        base.Reset();
        smoothMovement.Reset();
        onPathFinished = null;
        direction = DIRECTION.LEFT;
        currLocation = null;
        targetLocation = null;
        path = null;
        _hasArrived = false;
    }
    #endregion
}
