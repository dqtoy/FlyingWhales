using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingManagerAIPath : AIPath {

    [SerializeField] private AIDestinationSetter destinationSetter;

    private System.Action<List<Vector3>> onComputePath;

    public void ComputePath(HexTile startTile, HexTile endTile, System.Action<List<Vector3>> onComputePath) {
        if (onComputePath != null) {
            throw new System.Exception("There is already a path computing");
        }
        this.onComputePath = onComputePath;

        this.transform.position = startTile.transform.position;
        destinationSetter.target = endTile.transform;
        SearchPath();
    }

    protected override void OnPathComplete(Path newPath) {
        base.OnPathComplete(newPath);
        if (onComputePath != null) {
            onComputePath(path.vectorPath);
            onComputePath = null;
        }
    }
}
