using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class CharacterAIPath : AIPath {
    public CharacterMarker marker;
    public int doNotMove { get; private set; }
    public bool isStopMovement { get; private set; }
    public Path currentPath { get; private set; }

    public int searchLength = 1000;
    public int spread = 5000;
    public float aimStrength = 1f;

    private float _originalRepathRate;

    protected override void Start() {
        base.Start();
        _originalRepathRate = repathRate;
    }
    public override void OnTargetReached() {
        base.OnTargetReached();
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

    protected override void OnPathComplete(Path newPath) {
        base.OnPathComplete(newPath);
        currentPath = newPath;
    }
    //public override void SearchPath() {
    //    if (float.IsPositiveInfinity(destination.x)) return;
    //    if (onSearchPath != null) onSearchPath();

    //    lastRepath = Time.time;
    //    waitingForPathCalculation = true;

    //    seeker.CancelCurrentPathRequest();

    //    Vector3 start, end;
    //    CalculatePathRequestEndpoints(out start, out end);


    //    if (marker.character.stateComponent.currentState != null && marker.character.stateComponent.currentState.characterState == CHARACTER_STATE.STROLL) {
    //        //Alternative way of requesting the path
    //        canSearch = false;
    //        RandomPath rp = RandomPath.Construct(start, searchLength);
    //        rp.spread = spread;
    //        rp.aimStrength = aimStrength;
    //        rp.aim = end;
    //        seeker.StartPath(rp);
    //    } else {
    //        // This is where we should search to
    //        // Request a path to be calculated from our current position to the destination
    //        seeker.StartPath(start, end);
    //    }
    //}

    public override void UpdateMe() {
        if (!marker.gameObject.activeSelf) {
            return;
        }
        marker.UpdatePosition();
        if (marker.character.currentParty.icon.isTravelling && marker.character.IsInOwnParty()) { //only rotate if character is travelling
            marker.visualsParent.localRotation = Quaternion.LookRotation(Vector3.forward, this.velocity);
        } else if (marker.character.currentAction != null && marker.character.currentAction.poiTarget != marker.character) {
            marker.LookAt(marker.character.currentAction.poiTarget.gridTileLocation.centeredWorldLocation); //so that the charcter will always face the target, even if it is moving
        }
        if (doNotMove > 0 || isStopMovement) { return; }
        base.UpdateMe();
       
    }
    public void AdjustDoNotMove(int amount) {
        doNotMove += amount;
        doNotMove = Mathf.Max(0, doNotMove);
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
