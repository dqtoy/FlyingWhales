using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class CharacterAIPath : AIPath {
    public CharacterMarker marker;
    public int doNotMove { get; private set; }
    public bool isStopMovement { get; private set; }
    public ABPath currentPath { get; private set; }
    public bool hasReachedTarget { get; private set; }

    public int searchLength = 1000;
    public int spread = 5000;
    public float aimStrength = 1f;

    private float _originalRepathRate;
    private BlockerTraversalProvider blockerTraversalProvider;
    private bool _hasReachedTarget;

    protected override void Start() {
        base.Start();
        _originalRepathRate = repathRate;
        blockerTraversalProvider = new BlockerTraversalProvider(marker);
    }
    public override void OnTargetReached() {
        base.OnTargetReached();
        //if(currentPath != null && destination == currentPath.originalEndPoint){
        if (!_hasReachedTarget) {
            _hasReachedTarget = true;
            canSearch = true;
            marker.ArrivedAtLocation();
            currentPath = null;
            //TODO: Move these to delegates
            if (marker.hasFleePath) {
                marker.OnFinishFleePath();
            }
            //else if (marker.currentlyEngaging != null) {
            //    marker.OnReachEngageTarget();
            //}
        }
        //}
    }

    protected override void OnPathComplete(Path newPath) {
        currentPath = newPath as ABPath;
        base.OnPathComplete(newPath);
        _hasReachedTarget = false;
    }
    public override void SearchPath() {
        if (float.IsPositiveInfinity(destination.x)) return;
        if (onSearchPath != null) onSearchPath();

        lastRepath = Time.time;
        waitingForPathCalculation = true;

        seeker.CancelCurrentPathRequest();

        Vector3 start, end;
        CalculatePathRequestEndpoints(out start, out end);

        // Alternative way of requesting the path
        ABPath p = ABPath.Construct(start, end, null);
        p.traversalProvider = blockerTraversalProvider;
        seeker.StartPath(p);

        // This is where we should search to
        // Request a path to be calculated from our current position to the destination
        //seeker.StartPath(start, end);
    }

    public override void UpdateMe() {
        if (!marker.gameObject.activeSelf) {
            return;
        }
        marker.UpdatePosition();
        if (doNotMove > 0 || isStopMovement) { return; }
        if (marker.character.currentParty.icon.isTravelling && marker.character.IsInOwnParty()) { //only rotate if character is travelling
            marker.visualsParent.localRotation = Quaternion.LookRotation(Vector3.forward, this.velocity);
        } else if (marker.character.currentAction != null && marker.character.currentAction.poiTarget != marker.character) {
            marker.LookAt(marker.character.currentAction.poiTarget.gridTileLocation.centeredWorldLocation); //so that the charcter will always face the target, even if it is moving
        }
        base.UpdateMe();
       
    }
    public string lastAdjustDoNotMoveST { get; private set; }
    public void AdjustDoNotMove(int amount) {
        doNotMove += amount;
        doNotMove = Mathf.Max(0, doNotMove);
        if (!StackTraceUtility.ExtractStackTrace().Contains("Pause")) {
            lastAdjustDoNotMoveST = "Adjustment: " + amount.ToString() + "\n" + StackTraceUtility.ExtractStackTrace();
        }
    }
    public string stopMovementST;
    public void SetIsStopMovement(bool state) {
        isStopMovement = state;
        if (isStopMovement) {
            stopMovementST = StackTraceUtility.ExtractStackTrace();
        }
    }

    public void ClearPath() {
        currentPath = null;
    }

}
