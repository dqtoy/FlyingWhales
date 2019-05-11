using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.Linq;

public class CharacterAIPath : AILerp {
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
        if (!_hasReachedTarget && !pathPending && reachedEndOfPath &&
            //only execute target reach if the agent has a destination transform, vector or has a flee path
            (marker.destinationSetter.target != null || !float.IsPositiveInfinity(destination.x) || marker.hasFleePath)) { 
            _hasReachedTarget = true;
            canSearch = true;
            if (marker.hasFleePath) {
                marker.OnFinishedTraversingFleePath();
            } else {
                marker.ArrivedAtTarget();
            }
            currentPath = null;
            
        }
    }

    protected override void OnPathComplete(Path newPath) {
        if (newPath.CompleteState == PathCompleteState.Error) {
            Debug.LogWarning(marker.character.name + " path request returned a path with errors!");
        }
        currentPath = newPath as ABPath;
        if (UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter == marker.character && currentPath.traversalProvider != null && marker.terrifyingCharacters.Count > 0) {
            string costLog = "PATH FOR " + marker.character.name;
            uint totalCost = 0;
            for (int i = 0; i < currentPath.path.Count; i++) {
                Vector3 nodePos = (Vector3) currentPath.path[i].position;
                uint currentCost = currentPath.traversalProvider.GetTraversalCost(newPath, currentPath.path[i]);
                //float dx = (marker.terrifyingCharacters[0].marker.character.gridTileLocation.centeredWorldLocation.x - nodePos.x);
                //float dz = (marker.terrifyingCharacters[0].marker.character.gridTileLocation.centeredWorldLocation.y - nodePos.y);
                //float distSqr = dx * dx + dz * dz;
                //costLog += "\n-> " + nodePos + "(" + currentCost + ")" + "[" + distSqr + "]";
                Vector3 newNodePos = new Vector3((Mathf.Floor(nodePos.x)) + 0.5f, (Mathf.Floor(nodePos.y)) + 0.5f, Mathf.Floor(nodePos.z));
                costLog += "\n-> " + newNodePos + "(" + currentCost + ")" + "[" + Vector2.Distance(newNodePos, marker.terrifyingCharacters[0].gridTileLocation.centeredWorldLocation) + "]";
                totalCost += currentCost;
            }
            costLog += "\nTOTAL COST: " + totalCost;

            costLog += "\n\nVECTOR PATH";
            for (int i = 0; i < currentPath.vectorPath.Count; i++) {
                costLog += "\n-> " + currentPath.vectorPath[i] + "(" + GetNodePenalty(currentPath.vectorPath[i]) + ")" + "[" + Vector2.Distance(currentPath.vectorPath[i], marker.terrifyingCharacters[0].gridTileLocation.centeredWorldLocation) + "]";
            }
            Debug.LogWarning(costLog);
        }
        base.OnPathComplete(newPath);
        _hasReachedTarget = false;
    }
    public override void SearchPath() {
        if (float.IsPositiveInfinity(destination.x)) return;
        _hasReachedTarget = false;
        if (onSearchPath != null) onSearchPath();

        lastRepath = Time.time;

        // This is where the path should start to search from
        var currentPosition = GetFeetPosition();

        // If we are following a path, start searching from the node we will
        // reach next this can prevent odd turns right at the start of the path
        /*if (interpolator.valid) {
            var prevDist = interpolator.distance;
            // Move to the end of the current segment
            interpolator.MoveToSegment(interpolator.segmentIndex, 1);
            currentPosition = interpolator.position;
            // Move back to the original position
            interpolator.distance = prevDist;
        }*/

        canSearchAgain = false;

        for (int i = 0; i < marker.terrifyingCharacters.Count; i++) {
            if (!marker.terrifyingCharacters[i].isDead) {
                marker.terrifyingCharacters[i].marker.UpdateCenteredWorldPos();
            }
        }
        // Alternative way of requesting the path
        ABPath p = ABPath.Construct(currentPosition, destination, null);
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
            Vector3 direction;
            if (!interpolator.valid) {
                direction = Vector3.zero;
            } else {
                direction = interpolator.tangent;
            }
            marker.visualsParent.localRotation = Quaternion.LookRotation(Vector3.forward, direction);
            //if(marker.character.currentParty.icon.travelLine == null) {
            //    if (!IsNodeWalkable(destination)) {
            //        //if(marker.character.currentAction)
            //    }
            //}
        } else if (marker.character.currentAction != null && marker.character.currentAction.poiTarget != marker.character) {
            if (marker.character.currentAction.poiTarget.gridTileLocation != null) {
                marker.LookAt(marker.character.currentAction.poiTarget.gridTileLocation.centeredWorldLocation); //so that the charcter will always face the target, even if it is moving
            }
            
        }
        base.UpdateMe();
    }

    //protected override void Update() {
    //    if (!marker.gameObject.activeSelf) {
    //        return;
    //    }
    //    marker.UpdatePosition();
    //    if (doNotMove > 0 || isStopMovement) { return; }
    //    if (marker.character.currentParty.icon.isTravelling && marker.character.IsInOwnParty()) { //only rotate if character is travelling
    //        Vector3 direction;
    //        if (!interpolator.valid) {
    //            direction = Vector3.zero;
    //        } else {
    //            direction = interpolator.tangent;
    //        }
    //        marker.visualsParent.localRotation = Quaternion.LookRotation(Vector3.forward, direction);
    //        //if(marker.character.currentParty.icon.travelLine == null) {
    //        //    if (!IsNodeWalkable(destination)) {
    //        //        //if(marker.character.currentAction)
    //        //    }
    //        //}
    //    } else if (marker.character.currentAction != null && marker.character.currentAction.poiTarget != marker.character) {
    //        marker.LookAt(marker.character.currentAction.poiTarget.gridTileLocation.centeredWorldLocation); //so that the charcter will always face the target, even if it is moving
    //    }
    //    base.Update();
    //}
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
        path = null; //located at AILerp base class. Reference https://forum.arongranberg.com/t/how-to-stop-a-path-prematurely/1321/2
        _hasReachedTarget = false;
        marker.SetTargetTransform(null);
        marker.SetDestination(Vector3.positiveInfinity);
    }

    public bool IsNodeWalkable(Vector3 nodePos) {
        if (marker.terrifyingCharacters.Count > 0) {
            for (int i = 0; i < marker.terrifyingCharacters.Count; i++) {
                Character terrifyingCharacter = marker.terrifyingCharacters.ElementAtOrDefault(i);
                if (terrifyingCharacter == null || (terrifyingCharacter.currentParty.icon.isTravelling && terrifyingCharacter.currentParty.icon.travelLine != null && marker.character.currentStructure != terrifyingCharacter.currentStructure)) {
                    continue;
                }
                if (!terrifyingCharacter.isDead) {
                    //float dx = (terrifyingCharacter.marker.character.gridTileLocation.centeredWorldLocation.x - nodePos.x);
                    //float dz = (terrifyingCharacter.marker.character.gridTileLocation.centeredWorldLocation.y - nodePos.y);
                    //float distSqr = dx * dx + dz * dz;
                    //if (distSqr <= marker.penaltyRadius) {
                    //    return false;
                    //}
                    Vector3 newNodePos = new Vector3((Mathf.Floor(nodePos.x)) + 0.5f, (Mathf.Floor(nodePos.y)) + 0.5f, Mathf.Floor(nodePos.z));
                    if (Vector3.Distance(newNodePos, terrifyingCharacter.marker.character.gridTileLocation.centeredWorldLocation) <= marker.penaltyRadius) {
                        return false;
                    }
                }
            }
        }
        return true;
    }
    public uint GetNodePenalty(Vector3 nodePos) {
        if (marker.terrifyingCharacters.Count > 0) {
            for (int i = 0; i < marker.terrifyingCharacters.Count; i++) {
                Character terrifyingCharacter = marker.terrifyingCharacters.ElementAtOrDefault(i);
                if (terrifyingCharacter == null) {
                    continue;
                }
                if(terrifyingCharacter.currentParty == null || terrifyingCharacter.currentParty.icon == null || (terrifyingCharacter.currentParty.icon.isTravelling && terrifyingCharacter.currentParty.icon.travelLine != null && marker.character.currentStructure != terrifyingCharacter.currentStructure)) {
                    continue;
                }
                if(terrifyingCharacter.currentParty.icon == null || (terrifyingCharacter.currentParty.icon.isTravelling && terrifyingCharacter.currentParty.icon.travelLine != null && marker.character.currentStructure != terrifyingCharacter.currentStructure)) {
                    continue;
                }
                if(terrifyingCharacter.currentParty.icon.isTravelling && terrifyingCharacter.currentParty.icon.travelLine != null && marker.character.currentStructure != terrifyingCharacter.currentStructure) {
                    continue;
                }
                if (!terrifyingCharacter.isDead) {
                    //float dx = (terrifyingCharacter.marker.character.gridTileLocation.centeredWorldLocation.x - nodePos.x);
                    //float dz = (terrifyingCharacter.marker.character.gridTileLocation.centeredWorldLocation.y - nodePos.y);
                    //float distSqr = dx * dx + dz * dz;
                    //if (distSqr <= marker.penaltyRadius) {
                    //    return false;
                    //}
                    Vector3 newNodePos = new Vector3((Mathf.Floor(nodePos.x)) + 0.5f, (Mathf.Floor(nodePos.y)) + 0.5f, Mathf.Floor(nodePos.z));
                    float distance = Vector3.Distance(newNodePos, terrifyingCharacter.marker.centeredWorldPos);
                    if (distance <= marker.penaltyRadius) {
                        return 1000000;
                        //float multiplier = marker.penaltyRadius - distance;
                        //if(multiplier <= 0.5f) {
                        //    multiplier = 1;
                        //}
                        //return multiplier * 5000000;
                    }
                }
            }
        }
        return 1000;
    }
}
