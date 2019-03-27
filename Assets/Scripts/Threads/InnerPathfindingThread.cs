using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InnerPathfindingThread : Multithread {
    public List<LocationGridTile> path { get; private set; }
    public Character character { get; private set; }
    public LocationGridTile startingTile { get; private set; }
    public LocationGridTile destinationTile { get; private set; }
    public GRID_PATHFINDING_MODE pathfindingMode { get; private set; }

    public InnerPathfindingThread(Character character) {
        this.character = character;
    }

    public void SetValues(LocationGridTile startingTile, LocationGridTile destinationTile, GRID_PATHFINDING_MODE pathfindingMode) {
        this.startingTile = startingTile;
        this.destinationTile = destinationTile;
        this.pathfindingMode = pathfindingMode;
        this.path = null;
    }

    #region Overrides
    public override void DoMultithread() {
        base.DoMultithread();
        FindPath();
    }
    public override void FinishMultithread() {
        base.FinishMultithread();
        ReturnPath();
    }
    #endregion
    public void FindPath() {
        path = PathGenerator.Instance.GetPath(startingTile, destinationTile, pathfindingMode);
    }

    public void ReturnPath() {
        this.character.marker.ReceivePathFromPathfindingThread(this);
    }
}
