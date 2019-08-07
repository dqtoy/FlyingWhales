using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.Linq;

public class CharacterAIPath : AILerp {
    public CharacterMarker marker;
    public int doNotMove { get; private set; }
    public bool isStopMovement { get; private set; }
    public Path currentPath { get; private set; }
    public bool hasReachedTarget { get; private set; }

    public int searchLength = 1000;
    public int spread = 5000;
    public float aimStrength = 1f;

    //private float _originalRepathRate;
    private BlockerTraversalProvider blockerTraversalProvider;
    private bool _hasReachedTarget;

    public STRUCTURE_TYPE[] onlyAllowedStructures { get; private set; }
    public STRUCTURE_TYPE[] notAllowedStructures { get; private set; }

    private float Default_End_Reached_Distance; //This should only be set on the initialization of this object

    #region Monobehaviours
    protected override void Start() {
        base.Start();
        //_originalRepathRate = repathRate;
        Default_End_Reached_Distance = endReachDistance;
        blockerTraversalProvider = new BlockerTraversalProvider(marker);
    }
    #endregion

    #region Overrides
    public override void OnTargetReached() {
        base.OnTargetReached();
        if (!_hasReachedTarget && !pathPending && reachedEndOfPath &&
            //only execute target reach if the agent has a destination transform, vector or has a flee path
            (marker.destinationSetter.target != null || !float.IsPositiveInfinity(destination.x) || marker.hasFleePath)) {
            _hasReachedTarget = true;
            canSearch = true;
            //if (marker.hasFleePath) {
            //    marker.OnFinishedTraversingFleePath();
            //} else {
            marker.ArrivedAtTarget();
            //}
            currentPath = null;

        }
    }
    protected override void OnPathComplete(Path newPath) {
        if (newPath.CompleteState == PathCompleteState.Error) {
            Debug.LogWarning(marker.character.name + " path request returned a path with errors! Arrival action is: " + marker.arrivalAction?.Method.Name ?? "None" + "Destination is " + destination.ToString());
        }
        if (newPath is FleeMultiplePath) {
            currentPath = newPath;
            marker.OnFleePathComputed(newPath);
        } else {
            currentPath = newPath as CustomABPath;
            if (UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter == marker.character && currentPath.traversalProvider != null) { //&& marker.terrifyingCharacters.Count > 0
                string costLog = "PATH FOR " + marker.character.name;
                uint totalCost = 0;
                for (int i = 0; i < currentPath.path.Count; i++) {
                    Vector3 nodePos = (Vector3)currentPath.path[i].position;
                    uint currentCost = currentPath.traversalProvider.GetTraversalCost(newPath, currentPath.path[i]);
                    //float dx = (marker.terrifyingCharacters[0].marker.character.gridTileLocation.centeredWorldLocation.x - nodePos.x);
                    //float dz = (marker.terrifyingCharacters[0].marker.character.gridTileLocation.centeredWorldLocation.y - nodePos.y);
                    //float distSqr = dx * dx + dz * dz;
                    //costLog += "\n-> " + nodePos + "(" + currentCost + ")" + "[" + distSqr + "]";
                    Vector3 newNodePos = new Vector3((Mathf.Floor(nodePos.x)) + 0.5f, (Mathf.Floor(nodePos.y)) + 0.5f, Mathf.Floor(nodePos.z));
                    costLog += "\n-> " + newNodePos + "(" + currentCost + ")";
                    totalCost += currentCost;
                }
                costLog += "\nTOTAL COST: " + totalCost;

                costLog += "\n\nVECTOR PATH";
                for (int i = 0; i < currentPath.vectorPath.Count; i++) {
                    costLog += "\n-> " + currentPath.vectorPath[i] + "(" + GetNodePenalty(currentPath.vectorPath[i]) + GetNodePenaltyForStructures(currentPath, currentPath.vectorPath[i]) + ")";
                }
                //Debug.LogWarning(costLog);
            }
        }
        base.OnPathComplete(newPath);
        _hasReachedTarget = false;
    }
    public override void SearchPath() {
        if (float.IsPositiveInfinity(destination.x)) return;
        if (marker.character == null) return;
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

        for (int i = 0; i < marker.terrifyingObjects.Count; i++) {
            IPointOfInterest poi = marker.terrifyingObjects[i];
            if (poi is Character) {
                Character currCharacter = poi as Character;
                if (!currCharacter.isDead && currCharacter.marker != null) {
                    currCharacter.marker.UpdateCenteredWorldPos();
                }
            }
        }
        // Alternative way of requesting the path
        CustomABPath p = CustomABPath.Construct(currentPosition, destination, null);
        p.traversalProvider = blockerTraversalProvider;
        p.SetArea(marker.character.specificLocation);
        p.SetNotAllowedStructures(notAllowedStructures);
        p.SetOnlyAllowedStructures(onlyAllowedStructures);
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
        UpdateRotation();
        base.UpdateMe();
    }
    private void UpdateRotation() {
        if (marker.character.currentParty.icon.isTravelling && marker.character.IsInOwnParty() && currentPath != null) { //only rotate if character is travelling
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
    }
    #endregion

    #region Utilities
    public string lastAdjustNegativeDoNotMoveST { get; private set; }
    public string lastAdjustPositiveDoNotMoveST { get; private set; }
    public string stopMovementST { get; private set; }
    public void AdjustDoNotMove(int amount) {
        doNotMove += amount;
        doNotMove = Mathf.Max(0, doNotMove);
        if (!StackTraceUtility.ExtractStackTrace().Contains("Pause")) {
            if (amount < 0) {
                lastAdjustNegativeDoNotMoveST = "Adjustment: " + amount.ToString() + "\n" + StackTraceUtility.ExtractStackTrace();
            } else {
                lastAdjustPositiveDoNotMoveST = "Adjustment: " + amount.ToString() + "\n" + StackTraceUtility.ExtractStackTrace();
            }
        }
    }
    public void SetIsStopMovement(bool state) {
        isStopMovement = state;
        if (isStopMovement) {
            stopMovementST = StackTraceUtility.ExtractStackTrace();
        }
    }
    public void ClearAllCurrentPathData() {
        currentPath = null;
        path = null; //located at AILerp base class. Reference https://forum.arongranberg.com/t/how-to-stop-a-path-prematurely/1321/2
        _hasReachedTarget = false;
        marker.SetTargetTransform(null);
        marker.SetDestination(Vector3.positiveInfinity);
        marker.ClearArrivalAction();
        interpolator.SetPath(null);
        marker.StopMovement();
    }
    public void ResetThis() {
        ResetEndReachedDistance();
        ClearAllCurrentPathData();
        doNotMove = 0;
        blockerTraversalProvider = null;
        onlyAllowedStructures = null;
        notAllowedStructures = null;
        isStopMovement = false;
    }
    public void SetNotAllowedStructures(STRUCTURE_TYPE[] notAllowedStructures) {
        this.notAllowedStructures = notAllowedStructures;
    }
    public bool IsNodeWalkable(Vector3 nodePos) {
        if (marker.terrifyingObjects.Count > 0) {
            for (int i = 0; i < marker.terrifyingObjects.Count; i++) {
                IPointOfInterest currPOI = marker.terrifyingObjects.ElementAtOrDefault(i);
                if (currPOI is Character) {
                    Character terrifyingCharacter = currPOI as Character;
                    if (terrifyingCharacter.currentParty.icon.isTravelling && terrifyingCharacter.currentParty.icon.travelLine != null && marker.character.currentStructure != terrifyingCharacter.currentStructure) {
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
                } else {
                    Vector3 newNodePos = new Vector3((Mathf.Floor(nodePos.x)) + 0.5f, (Mathf.Floor(nodePos.y)) + 0.5f, Mathf.Floor(nodePos.z));
                    if (Vector3.Distance(newNodePos, currPOI.gridTileLocation.centeredWorldLocation) <= marker.penaltyRadius) {
                        return false;
                    }
                }
                
            }
        }
        return true;
    }
    public uint GetNodePenalty(Vector3 nodePos) {
        if (marker.terrifyingObjects.Count > 0) {
            for (int i = 0; i < marker.terrifyingObjects.Count; i++) {
                IPointOfInterest currPOI = marker.terrifyingObjects.ElementAtOrDefault(i);
                if (currPOI is Character) {
                    Character terrifyingCharacter = currPOI as Character;
                    if (terrifyingCharacter.marker == null) {
                        continue;
                    }
                    if (terrifyingCharacter.currentParty == null || terrifyingCharacter.currentParty.icon == null || (terrifyingCharacter.currentParty.icon.isTravelling && terrifyingCharacter.currentParty.icon.travelLine != null && marker.character.currentStructure != terrifyingCharacter.currentStructure)) {
                        continue;
                    }
                    if (terrifyingCharacter.currentParty.icon == null || (terrifyingCharacter.currentParty.icon.isTravelling && terrifyingCharacter.currentParty.icon.travelLine != null && marker.character.currentStructure != terrifyingCharacter.currentStructure)) {
                        continue;
                    }
                    if (terrifyingCharacter.currentParty.icon.isTravelling && terrifyingCharacter.currentParty.icon.travelLine != null && marker.character.currentStructure != terrifyingCharacter.currentStructure) {
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
                } else {
                    Vector3 newNodePos = new Vector3((Mathf.Floor(nodePos.x)) + 0.5f, (Mathf.Floor(nodePos.y)) + 0.5f, Mathf.Floor(nodePos.z));
                    if (currPOI != null && currPOI.gridTileLocation != null) {
                        float distance = Vector3.Distance(newNodePos, currPOI.gridTileLocation.centeredWorldLocation);
                        if (distance <= marker.penaltyRadius) {
                            return 1000000;
                        }
                    }
                }

                
            }
        }
        return 1000;
    }
    public uint GetNodePenaltyForStructures(Path path, Vector3 nodePos) {
        if (path is CustomABPath) {
            CustomABPath customPath = path as CustomABPath;
            if (customPath.notAllowedStructures == null && customPath.onlyAllowedStructures == null) {
                return 0;
            }
            if (customPath.area == null) {
                return 0;
            }
            Vector3 newNodePos = new Vector3(Mathf.Floor(nodePos.x), Mathf.Floor(nodePos.y), Mathf.Floor(nodePos.z));
            Vector3 localPos = customPath.area.areaMap.worldPos - newNodePos;
            Vector3Int localPlace = new Vector3Int(localPos.x < 0f ? Mathf.FloorToInt(localPos.x) * -1 : Mathf.FloorToInt(localPos.x), localPos.y < 0f ? Mathf.FloorToInt(localPos.y) * -1 : Mathf.FloorToInt(localPos.y), 0);
            //Vector3Int localPlace = customPath.area.areaMap.groundTilemap.WorldToCell(newNodePos);
            LocationGridTile nodeGridTile = null;
            if (Utilities.IsInRange(localPlace.x, 0, customPath.area.areaMap.map.GetUpperBound(0) + 1) &&
                    Utilities.IsInRange(localPlace.y, 0, customPath.area.areaMap.map.GetUpperBound(1) + 1)) {
                nodeGridTile = customPath.area.areaMap.map[localPlace.x, localPlace.y];
            }
            if (nodeGridTile != null && nodeGridTile.structure != null) {
                if (customPath.notAllowedStructures != null) {
                    for (int i = 0; i < customPath.notAllowedStructures.Length; i++) {
                        if (customPath.notAllowedStructures[i] == nodeGridTile.structure.structureType) {
                            return 1000000;
                        }
                    }
                } else if (customPath.onlyAllowedStructures != null) {
                    bool isAllowed = false;
                    for (int i = 0; i < customPath.onlyAllowedStructures.Length; i++) {
                        if (customPath.onlyAllowedStructures[i] == nodeGridTile.structure.structureType) {
                            isAllowed = true;
                            break;
                        }
                    }
                    if (!isAllowed) {
                        return 1000000;
                    }
                }
            }
        }
        return 0;
    }
    public Vector3 GetTangent() {
        if (interpolator.valid) {
            return interpolator.tangent;
        }
        return Vector3.zero;
    }
    public void SetEndReachedDistance(float distance) {
        endReachDistance = distance;
    }
    public void ResetEndReachedDistance() {
        endReachDistance = Default_End_Reached_Distance;
    }
    /// <summary>
    /// Is this character no allowed to move? Disregards pause conditions.
    /// </summary>
    /// <returns></returns>
    public bool IsNotAllowedToMove() {
        if (GameManager.Instance.isPaused) {
            return doNotMove > 1;
        } else {
            return doNotMove > 0;
        }
    }
    #endregion
}