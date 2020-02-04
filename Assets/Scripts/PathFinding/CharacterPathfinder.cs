using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPathfinder : AIPath {

    [SerializeField] private AIDestinationSetter destinationSetter;

    private Action<List<Vector3>> onPathCalculated;
    public bool isWaitingForPathCalculation;

    public List<Vector3> computedPath;

    public void CalculatePath(HexTile destination, Action<List<Vector3>> onPathCalculated) {
        this.onPathCalculated = onPathCalculated;
        isWaitingForPathCalculation = true;

        destinationSetter.target = destination.transform;
        //Debug.Log("Calculating path to " + destination.name);
        SearchPath();
        this.maxSpeed = 0f;
    }

    #region overrides
    protected override void OnPathComplete(Path newPath) {
        //Debug.Log("Done computing path. Result is " + newPath.vectorPath.Count.ToString());
        base.OnPathComplete(newPath);
        if (onPathCalculated != null) {
            computedPath = newPath.vectorPath;
            onPathCalculated(newPath.vectorPath);
            onPathCalculated = null;
        }
        isWaitingForPathCalculation = false;
    }
    //public override void SearchPath() {
    //    if (float.IsPositiveInfinity(destination.x)) return;
    //    if (onSearchPath != null) onSearchPath();

    //    lastRepath = Time.time;
    //    waitingForPathCalculation = true;

    //    seeker.CancelCurrentPathRequest();

    //    Vector3 start, end;
    //    CalculatePathRequestEndpoints(out start, out end);

    //    // Alternative way of requesting the path
    //    //ABPath p = ABPath.Construct(start, end, null);
    //    //seeker.StartPath(p);

    //    // This is where we should search to
    //    // Request a path to be calculated from our current position to the destination
    //    seeker.StartPath(start, end);
    //}
    #endregion
}
