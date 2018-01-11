using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EZObjectPools;

public class CharacterAvatar : PooledObject{

    [SerializeField] private SmoothMovement smoothMovement;
    [SerializeField] private DIRECTION direction;

    private List<Character> _characters;

    private HexTile currLocation;
    private HexTile targetLocation;

    private List<HexTile> path;

    internal virtual void Init(Character character) {
        _characters = new List<Character>();
        AddNewCharacter(character);
    }

    #region Character Management
    public void AddNewCharacter(Character character) {
        if (!_characters.Contains(character)) {
            _characters.Add(character);
        }
    }
    #endregion

    #region Pathfinding
    internal void SetTarget(HexTile target) {
        targetLocation = target;
    }
    internal void CreatePath(PATHFINDING_MODE pathFindingMode) {
        if (this.targetLocation != null) {
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
    #endregion
}
