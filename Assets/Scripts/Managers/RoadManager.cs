using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RoadManager : MonoBehaviour {
    public static RoadManager Instance = null;

    //public Transform majorRoadParent;
    //public Transform minorRoadParent;

    //public GameObject majorRoadGO;
    //public GameObject minorRoadGO;

    //public int maxConnections;
    //public int maxCityConnections;
    public int recommendedLandmarkConnections;
    public int maxLandmarkConnections;
    public int maxRoadLength; //how many tiles can a road be?

    private List<HexTile> _roadTiles;
    private List<HexTile> _minorRoadTiles;
    private List<HexTile> _majorRoadTiles;

    #region getters/setters
    internal List<HexTile> roadTiles {
        get { return _roadTiles; }
    }
    internal List<HexTile> minorRoadTiles {
        get { return _minorRoadTiles; }
    }
    internal List<HexTile> majorRoadTiles {
        get { return _majorRoadTiles; }
    }
    #endregion

    void Awake() {
        Instance = this;
        _roadTiles = new List<HexTile>();
        _majorRoadTiles = new List<HexTile>();
        _minorRoadTiles = new List<HexTile>();
    }


    public bool GenerateRoads() {
        if (!ConnectRegionLandmarks()) {
            return false;
        }
        if (!ConnectRegions()) {
            return false;
        }

        Debug.Log("Landmarks with 1 connection: " + LandmarkManager.Instance.GetAllLandmarks().Where(x => x.connections.Count == 1).Count());
        Debug.Log("Landmarks with 2 connections: " + LandmarkManager.Instance.GetAllLandmarks().Where(x => x.connections.Count == 2).Count());
        Debug.Log("Landmarks with 3 connections: " + LandmarkManager.Instance.GetAllLandmarks().Where(x => x.connections.Count == 3).Count());

        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            currRegion.CheckForRoadAdjacency(); //populate the adjacentViaMajorRoads of all regions
        }
        return true;
    }
    private bool ConnectRegionLandmarks() {
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            if (currRegion.landmarks.Count <= 1) {
                continue; //skip
            }
            Dictionary<BaseLandmark, Island> islands = new Dictionary<BaseLandmark, Island>();
            for (int j = 0; j < currRegion.landmarks.Count; j++) {
                BaseLandmark currLandmark = currRegion.landmarks[j];
                islands.Add(currLandmark, new Island(currLandmark));
            }

            List<BaseLandmark> checkedLandmarks = new List<BaseLandmark>();
            Queue<BaseLandmark> landmarks = new Queue<BaseLandmark>();
            BaseLandmark initialLandmark = currRegion.landmarks[Random.Range(0, currRegion.landmarks.Count)];
            landmarks.Enqueue(initialLandmark);

            while (landmarks.Count != 0) {
                BaseLandmark currLandmark = landmarks.Dequeue();
                if (currLandmark.connections.Count >= recommendedLandmarkConnections) {
                    if (landmarks.Count == 0 && checkedLandmarks.Count != currRegion.landmarks.Count) {
                        //missed some landmarks
                        for (int j = 0; j < currRegion.landmarks.Count; j++) {
                            BaseLandmark landmark = currRegion.landmarks[j];
                            if (!checkedLandmarks.Contains(landmark)) {
                                landmarks.Enqueue(landmark);
                                break;
                            }
                        }
                    }
                    checkedLandmarks.Add(currLandmark);
                    continue;
                }
                //Get the 2 nearest landmarks to the current landmark and connect the current landmark to both of them
                List<BaseLandmark> otherLandmarks = new List<BaseLandmark>(currRegion.landmarks
                    .Where(x => x.id != currLandmark.id && x.connections.Count < recommendedLandmarkConnections
                                && !currLandmark.IsConnectedTo(x) && !currLandmark.IsIndirectlyConnectedTo(x)
                    && PathGenerator.Instance.GetPath(currLandmark.tileLocation, x.tileLocation, PATHFINDING_MODE.LANDMARK_CONNECTION) != null)
                    .OrderBy(y => PathGenerator.Instance.GetPath(currLandmark.tileLocation, y.tileLocation, PATHFINDING_MODE.LANDMARK_CONNECTION).Count));

                for (int j = 0; j < otherLandmarks.Count; j++) {
                    BaseLandmark otherLandmark = otherLandmarks[j];
                    List<HexTile> path = PathGenerator.Instance.GetPath(currLandmark.tileLocation, otherLandmark.tileLocation, PATHFINDING_MODE.LANDMARK_CONNECTION);
                    if (path != null && path.Count <= maxRoadLength) {
                        Island islandOfChosenLandmark = islands[currLandmark];
                        Island islandOfOtherLandmark = islands[otherLandmark];
                        MergeIslands(islandOfChosenLandmark, islandOfOtherLandmark, islands);
                        ConnectLandmarkToLandmark(currLandmark, otherLandmark);
                        CreateRoad(path, ROAD_TYPE.MAJOR);
                        if (!checkedLandmarks.Contains(otherLandmark)) {
                            landmarks.Enqueue(otherLandmark);
                        }
                    }
                    if (currLandmark.connections.Count >= recommendedLandmarkConnections) {
                        break;
                    }
                }
                checkedLandmarks.Add(currLandmark);
                //if (!currLandmark.IsConnectedTo(currRegion)) {
                //    throw new System.Exception(currLandmark.landmarkName + " could not connect to any other landmark in its region!");
                //    //return false; //The current landmark has not connected to a landmark in its region, return a failure
                //}
                if (landmarks.Count == 0 && checkedLandmarks.Count != currRegion.landmarks.Count) {
                    //missed some landmarks
                    for (int j = 0; j < currRegion.landmarks.Count; j++) {
                        BaseLandmark landmark = currRegion.landmarks[j];
                        if (!checkedLandmarks.Contains(landmark)) {
                            landmarks.Enqueue(landmark);
                            break;
                        }
                    }
                }
            }
            if (!CheckForIslands(islands)) {
                return false;
            }

            //for (int j = 0; j < currRegion.landmarks.Count; j++) {
            //    BaseLandmark currLandmark = currRegion.landmarks[j];
            //    List<BaseLandmark> otherLandmarks = new List<BaseLandmark>(currRegion.landmarks.Where(x => x.id != currLandmark.id && PathGenerator.Instance.GetPath(currLandmark.tileLocation, x.tileLocation, PATHFINDING_MODE.LANDMARK_CONNECTION) != null)
            //        .OrderBy(y => PathGenerator.Instance.GetPath(currLandmark.tileLocation, y.tileLocation, PATHFINDING_MODE.LANDMARK_CONNECTION).Count));

            //    for (int k = 0; k < otherLandmarks.Count; k++) {
            //        BaseLandmark otherLandmark = otherLandmarks[k];
            //        if (currLandmark.id != otherLandmark.id && otherLandmark.connections.Count < recommendedLandmarkConnections 
            //            && !currLandmark.IsConnectedTo(otherLandmark) && !currLandmark.IsIndirectlyConnectedTo(otherLandmark)) {
            //            List<HexTile> path = PathGenerator.Instance.GetPath(currLandmark.tileLocation, otherLandmark.tileLocation, PATHFINDING_MODE.LANDMARK_CONNECTION);
            //            if (path != null) {
            //                //Island islandOfChosenLandmark = islands[currLandmark];
            //                //Island islandOfOtherLandmark = islands[otherLandmark];
            //                //MergeIslands(islandOfChosenLandmark, islandOfOtherLandmark, islands);
            //                ConnectLandmarkToLandmark(currLandmark, otherLandmark);
            //                CreateRoad(path, ROAD_TYPE.MAJOR);
            //            }
            //        }
            //        if (currLandmark.connections.Count >= recommendedLandmarkConnections) {
            //            break;
            //        }
            //    }
            //    if (!currLandmark.IsConnectedTo(currRegion)) {
            //        throw new System.Exception(currLandmark.landmarkName + " could not connect to any other landmark in its region!");
            //        //return false; //The current landmark has not connected to a landmark in its region, return a failure
            //    }
            //}
        }
        //yield return null;
        return true;
    }
    private bool ConnectRegions() {
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            for (int j = 0; j < currRegion.adjacentRegions.Count; j++) {
                Region otherRegion = currRegion.adjacentRegions[j];
                if (!currRegion.HasConnectionToRegion(otherRegion)) {
                    BaseLandmark chosenLandmark = currRegion.GetLandmarkNearestTo(otherRegion);
                    if (chosenLandmark != null) {
                        List<BaseLandmark> choices = new List<BaseLandmark>(otherRegion.landmarks.Where(x => x.connections.Count < maxLandmarkConnections));
                        if (choices.Count == 0) {
                            choices = new List<BaseLandmark>(otherRegion.landmarks);
                        }
                        choices = choices.OrderBy(x => chosenLandmark.tileLocation.GetDistanceTo(x.tileLocation)).ToList();
                        for (int l = 0; l < choices.Count; l++) {
                            BaseLandmark otherLandmark = choices[l];
                            List<HexTile> path = PathGenerator.Instance.GetPath(chosenLandmark.tileLocation, otherLandmark.tileLocation, PATHFINDING_MODE.LANDMARK_CONNECTION);
                            if (path != null && path.Count <= maxRoadLength) {
                                //Debug.Log("Connecting " + chosenLandmark.landmarkName + " to " + otherLandmark.landmarkName);
                                ConnectLandmarkToLandmark(chosenLandmark, otherLandmark);
                                CreateRoad(path, ROAD_TYPE.MAJOR);
                                break;
                            }
                        }
                    } else {
                        //throw new System.Exception("Could not connect region!");
                        bool isConnectedToAnyRegion = false;
                        for (int k = 0; k < GridMap.Instance.allRegions.Count; k++) {
                            Region region = GridMap.Instance.allRegions[k];
                            if (currRegion.id != region.id) {
                                if (currRegion.HasConnectionToRegion(region)) {
                                    isConnectedToAnyRegion = true;
                                    break;
                                }
                            }
                        }
                        if (!isConnectedToAnyRegion) {
                            return false; //currRegion is not connected to any region
                        }
                    }
                }
            }
        }
        return true;
    }

    //public bool GenerateRoads() {
    //    List<BaseLandmark> allLandmarks = LandmarkManager.Instance.GetAllLandmarks();
    //    Dictionary<BaseLandmark, Island> islands = new Dictionary<BaseLandmark, Island>();
    //    for (int i = 0; i < allLandmarks.Count; i++) {
    //        islands.Add(allLandmarks[i], new Island(allLandmarks[i]));
    //    }
    //    if (!ConnectFactionRegions(islands)) {
    //        return false;
    //    }

    //    if (!ConnectRegionLandmarks(islands)) {
    //        return false;
    //    }

    //    allLandmarks = allLandmarks.Where(x => x.connections.Count < recommendedLandmarkConnections).ToList();
    //    //choose a random landmark to start
    //    BaseLandmark initialLandmark = allLandmarks[Random.Range(0, allLandmarks.Count)];
    //    Queue<BaseLandmark> landmarkQueue = new Queue<BaseLandmark>();
    //    landmarkQueue.Enqueue(initialLandmark);

    //    while (landmarkQueue.Count > 0) {
    //        BaseLandmark currLandmark = landmarkQueue.Dequeue();
    //        //yield return new WaitForSeconds(2f);
    //        //UIManager.Instance.ShowLandmarkInfo(currLandmark);
    //        BaseLandmark landmarkToConnectTo = GetLandmarkForConnection(currLandmark);
    //        if (landmarkToConnectTo != null) {
    //            ConnectLandmarkToLandmark(currLandmark, landmarkToConnectTo);
    //            //yield return new WaitForSeconds(1f);
    //            CreateRoad(PathGenerator.Instance.GetPath(currLandmark.tileLocation, landmarkToConnectTo.tileLocation, PATHFINDING_MODE.LANDMARK_CONNECTION), ROAD_TYPE.MAJOR);
    //            Island islandOfCurrLandmark = islands[currLandmark];
    //            Island islandOfOtherLandmark = islands[landmarkToConnectTo];
    //            MergeIslands(islandOfCurrLandmark, islandOfOtherLandmark, islands);
    //            if (currLandmark.connections.Count >= recommendedLandmarkConnections) {
    //                allLandmarks.Remove(currLandmark);
    //            }
    //            if (landmarkToConnectTo.connections.Count >= recommendedLandmarkConnections) {
    //                allLandmarks.Remove(landmarkToConnectTo);
    //                //enqueue a random landmark that can still connect
    //                if (allLandmarks.Count > 0) {
    //                    landmarkQueue.Enqueue(allLandmarks[Random.Range(0, allLandmarks.Count)]);
    //                }
    //            } else {
    //                landmarkQueue.Enqueue(landmarkToConnectTo);
    //            }
    //        } else {
    //            //throw new System.Exception(currLandmark.landmarkName + " cannot find a landmark to connect to!");
    //            allLandmarks.Remove(currLandmark);
    //            if (allLandmarks.Count > 0) {
    //                landmarkQueue.Enqueue(allLandmarks[Random.Range(0, allLandmarks.Count)]);
    //            }
    //        }
    //    }
    //    if (!CheckForIslands(islands)) {
    //        return false;
    //    }
    //    Debug.Log("Landmarks with 1 connection: " + LandmarkManager.Instance.GetAllLandmarks().Where(x => x.connections.Count == 1).Count());
    //    Debug.Log("Landmarks with 2 connections: " + LandmarkManager.Instance.GetAllLandmarks().Where(x => x.connections.Count == 2).Count());
    //    Debug.Log("Landmarks with 3 connections: " + LandmarkManager.Instance.GetAllLandmarks().Where(x => x.connections.Count == 3).Count());

    //    for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
    //        Region currRegion = GridMap.Instance.allRegions[i];
    //        currRegion.CheckForRoadAdjacency(); //populate the adjacentViaMajorRoads of all regions
    //    }

    //    return true;
    //}

    /*
     Connect faction regions with each other
         */
    private bool ConnectFactionRegions(Dictionary<BaseLandmark, Island> islands) {
        for (int i = 0; i < FactionManager.Instance.allTribes.Count; i++) {
            Faction currFaction = FactionManager.Instance.allTribes[i];
            if (currFaction.ownedRegions.Count > 1) {
                Debug.Log("========== Creating Roads For " + currFaction.name + " ==========");
                List<Region> factionOwnedRegions = new List<Region>(currFaction.ownedRegions.OrderBy(x => x.adjacentRegions.Where(y => currFaction.ownedRegions.Contains(y)).Count()));
                for (int j = 0; j < factionOwnedRegions.Count; j++) {
                    Region currRegion = factionOwnedRegions[j];
                    //List<BaseLandmark> choices = new List<BaseLandmark>(); //get landamrks in adjacent regions that is owned by the current faction
                    for (int k = 0; k < currRegion.adjacentRegions.Count; k++) {
                        Region otherRegion = currRegion.adjacentRegions[k];
                        if (currFaction.ownedRegions.Contains(otherRegion)) {
                            if (!currRegion.HasConnectionToRegion(otherRegion)) { //make sure that the other region doesn't already have a connection with the current region
                                //pick landmark in this region that is nearest to the other region
                                BaseLandmark chosenLandmark = currRegion.GetLandmarkNearestTo(otherRegion);
                                if (chosenLandmark != null) {
                                    List<BaseLandmark> choices = new List<BaseLandmark>(otherRegion.landmarks);
                                    choices = choices.OrderBy(x => chosenLandmark.tileLocation.GetDistanceTo(x.tileLocation)).ToList();
                                    for (int l = 0; l < choices.Count; l++) {
                                        BaseLandmark otherLandmark = choices[l];
                                        List<HexTile> path = PathGenerator.Instance.GetPath(chosenLandmark.tileLocation, otherLandmark.tileLocation, PATHFINDING_MODE.LANDMARK_CONNECTION);
                                        if (path != null && path.Count <= maxRoadLength) {
                                            Debug.Log("Connecting " + chosenLandmark.landmarkName + " to " + otherLandmark.landmarkName);
                                            Island islandOfChosenLandmark = islands[chosenLandmark];
                                            Island islandOfOtherLandmark = islands[otherLandmark];
                                            MergeIslands(islandOfChosenLandmark, islandOfOtherLandmark, islands);
                                            ConnectLandmarkToLandmark(chosenLandmark, otherLandmark);
                                            CreateRoad(path, ROAD_TYPE.MAJOR);
                                            break;
                                        }
                                    }
                                } else {
                                    return false;
                                }
                                
                            }
                        }
                    }
                    
                }
            }
        }
        return true;
    }
    //private bool ConnectRegionLandmarks(Dictionary<BaseLandmark, Island> islands) {
    //    for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
    //        Region currRegion = GridMap.Instance.allRegions[i];
    //        if (currRegion.landmarks.Count <= 1) {
    //            continue; //skip
    //        }
    //        for (int j = 0; j < currRegion.landmarks.Count; j++) {
    //            BaseLandmark currLandmark = currRegion.landmarks[j];
    //            List<BaseLandmark> otherLandmarks = new List<BaseLandmark>(currRegion.landmarks.Where(x => x.id != currLandmark.id && PathGenerator.Instance.GetPath(currLandmark.tileLocation, x.tileLocation, PATHFINDING_MODE.LANDMARK_CONNECTION) != null)
    //                .OrderBy(y => PathGenerator.Instance.GetPath(currLandmark.tileLocation, y.tileLocation, PATHFINDING_MODE.LANDMARK_CONNECTION).Count));

    //            for (int k = 0; k < otherLandmarks.Count; k++) {
    //                BaseLandmark otherLandmark = otherLandmarks[k];
    //                if (currLandmark.id != otherLandmark.id && otherLandmark.connections.Count < maxLandmarkConnections && !currLandmark.IsConnectedTo(otherLandmark) && !currLandmark.IsIndirectlyConnectedTo(otherLandmark)) {
    //                    List<HexTile> path = PathGenerator.Instance.GetPath(currLandmark.tileLocation, otherLandmark.tileLocation, PATHFINDING_MODE.LANDMARK_CONNECTION);
    //                    if (path != null && path.Count < maxRoadLength) {
    //                        Island islandOfChosenLandmark = islands[currLandmark];
    //                        Island islandOfOtherLandmark = islands[otherLandmark];
    //                        MergeIslands(islandOfChosenLandmark, islandOfOtherLandmark, islands);
    //                        ConnectLandmarkToLandmark(currLandmark, otherLandmark);
    //                        CreateRoad(path, ROAD_TYPE.MAJOR);
    //                    }
    //                }
    //                if (currLandmark.connections.Count >= maxLandmarkConnections) {
    //                    break;
    //                }
    //            }
    //            if (!currLandmark.IsConnectedTo(currRegion)) {
    //                return false; //The current landmark has not connected to a landmark in its region, return a failure
    //            }

    //            //if (!currLandmark.IsConnectedTo(currRegion)) { //currLandmark is not yet connected to a region from its region
    //                //List<BaseLandmark> choices = new List<BaseLandmark>();
    //                //choices.AddRange(currRegion.landmarks);
    //                //choices.Remove(currLandmark);
    //                ////connect the currLandmark to the nearest landmark
    //                //List<HexTile> path = new List<HexTile>();
    //                //BaseLandmark nearestLandmark = GetLandmarkNearestTo(currLandmark, choices, PATHFINDING_MODE.LANDMARK_CONNECTION, ref path);
    //                //if (nearestLandmark != null) {
    //                //    Island islandOfChosenLandmark = islands[currLandmark];
    //                //    Island islandOfOtherLandmark = islands[nearestLandmark];
    //                //    MergeIslands(islandOfChosenLandmark, islandOfOtherLandmark, islands);
    //                //    ConnectLandmarkToLandmark(currLandmark, nearestLandmark);
    //                //    CreateRoad(path, ROAD_TYPE.MAJOR);
    //                //} else {
    //                //    throw new System.Exception(currLandmark.landmarkName + " could not connect to any landmark");
    //                //}
    //            //}
    //        }
    //    }
    //    return true;
    //}
    private BaseLandmark GetLandmarkForConnection(BaseLandmark origin) {
        Debug.Log("========== Choosing connection for " + origin.landmarkName + " ==========");
        Region regionOfOrigin = origin.tileLocation.region;

        List<Region> regionsToCheck = new List<Region>();
        regionsToCheck.Add(regionOfOrigin);
        regionsToCheck.AddRange(regionOfOrigin.adjacentRegions);
        List<BaseLandmark> choices = new List<BaseLandmark>();
        regionsToCheck.ForEach(x => choices.AddRange(x.landmarks.Where(y => y.connections.Count < maxLandmarkConnections && !y.connections.Contains(origin))));
        choices.Remove(origin);
        string choicesStr = string.Empty;
        for (int i = 0; i < choices.Count; i++) {
            choicesStr = choices[i].landmarkName;
        }
        Debug.Log("Choices are: " + choicesStr);
        //prioritize landmarks that have less than the minimum connections
        //Get the nearest landmark the origin landmark can connect to
        int nearestDistance = 9999;
        BaseLandmark nearestLandmark = null;
        List<BaseLandmark> choicesLessThanMin = new List<BaseLandmark>(choices.Where(x => x.connections.Count < recommendedLandmarkConnections));
        for (int i = 0; i < choicesLessThanMin.Count; i++) {
            BaseLandmark currChoice = choicesLessThanMin[i];
            if (origin.IsIndirectlyConnectedTo(currChoice)) {
                continue;
            }
            List<HexTile> path = PathGenerator.Instance.GetPath(origin.tileLocation, currChoice.tileLocation, PATHFINDING_MODE.LANDMARK_CONNECTION);
            if (path != null && path.Count <= maxRoadLength && path.Count < nearestDistance) {
                nearestDistance = path.Count;
                nearestLandmark = currChoice;
                Debug.Log("Nearest is now " + currChoice.landmarkName + " at only " + nearestDistance.ToString() + " tiles");
            }
        }
        if (nearestLandmark != null) {
            Debug.Log("Chosen tile is " + nearestLandmark.landmarkName + " at distance " + nearestDistance.ToString());
        } else {
            for (int i = 0; i < choices.Count; i++) {
                BaseLandmark currChoice = choices[i];
                List<HexTile> path = PathGenerator.Instance.GetPath(origin.tileLocation, currChoice.tileLocation, PATHFINDING_MODE.LANDMARK_CONNECTION);
                if (path != null && path.Count <= maxRoadLength && path.Count < nearestDistance) {
                    nearestDistance = path.Count;
                    nearestLandmark = currChoice;
                    Debug.Log("Nearest is now " + currChoice.landmarkName + " at only " + nearestDistance.ToString() + " tiles");
                }
            }
            if (nearestLandmark != null) {
                Debug.Log("Chosen tile is " + nearestLandmark.landmarkName + " at distance " + nearestDistance.ToString());
            }
        }
        return nearestLandmark;
    }
    private bool CheckForIslands(Dictionary<BaseLandmark, Island> islandsDict) {
        List<Island> allIslands = new List<Island>();
        foreach (KeyValuePair<BaseLandmark, Island> kvp in islandsDict) {
            if (!allIslands.Contains(kvp.Value)) {
                allIslands.Add(kvp.Value);
            }
        }
        Island mainIsland = allIslands[0];
        if (allIslands.Count > 1) {
            for (int i = 1; i < allIslands.Count; i++) {
                Island otherIsland = allIslands[i];
                ConnectIslands(mainIsland, otherIsland, islandsDict);
            }
        }

        foreach (KeyValuePair<BaseLandmark, Island> kvp in islandsDict) {
            if (kvp.Value.landamrksInIsland.Count > 0 && kvp.Value != mainIsland) {
                return false; //an island that is not the main island still remains
            }
        }
        return true;
    }
    private void ConnectIslands(Island island1, Island island2, Dictionary<BaseLandmark, Island> islands) {
        List<BaseLandmark> island1Choices = island1.landamrksInIsland.Where(x => x.connections.Count < maxLandmarkConnections).ToList();
        List<BaseLandmark> island2Choices = island2.landamrksInIsland.Where(x => x.connections.Count < maxLandmarkConnections).ToList();

        for (int i = 0; i < island1Choices.Count; i++) {
            BaseLandmark island1Landmark = island1Choices[i];
            int nearestDistance = 9999;
            BaseLandmark nearestLandmark = null;
            for (int j = 0; j < island2Choices.Count; j++) {
                BaseLandmark currChoice = island2Choices[j];
                List<HexTile> path = PathGenerator.Instance.GetPath(island1Landmark.tileLocation, currChoice.tileLocation, PATHFINDING_MODE.LANDMARK_CONNECTION);
                if (path != null && path.Count <= maxRoadLength && path.Count < nearestDistance) {
                    nearestDistance = path.Count;
                    nearestLandmark = currChoice;
                }
            }
            if (nearestLandmark != null) {
                ConnectLandmarkToLandmark(island1Landmark, nearestLandmark);
                CreateRoad(PathGenerator.Instance.GetPath(island1Landmark.tileLocation, nearestLandmark.tileLocation, PATHFINDING_MODE.LANDMARK_CONNECTION), ROAD_TYPE.MAJOR);
                MergeIslands(island1, island2, islands);
                break;
            }
        }
        
    }

    /*
     * <summary>
     * Create a new road given a path.
     * This will activate the road sprites in the path
     * </summary>
     * */
    public void CreateRoad(List<HexTile> path, ROAD_TYPE roadType) {
        for (int i = 0; i < path.Count; i++) {
            HexTile previousTile = path.ElementAtOrDefault(i - 1);
            HexTile currTile = path[i];
            HexTile nextTile = path.ElementAtOrDefault(i + 1);
            HEXTILE_DIRECTION from = currTile.GetNeighbourDirection(previousTile);
            HEXTILE_DIRECTION to = currTile.GetNeighbourDirection(nextTile);
            if(previousTile != null) {
                if (!currTile.allNeighbourRoads.Contains(previousTile)) {
                    currTile.allNeighbourRoads.Add(previousTile);
                }
                if (!previousTile.allNeighbourRoads.Contains(currTile)) {
                    previousTile.allNeighbourRoads.Add(currTile);
                }
            }

            if (nextTile != null) {
                if (!currTile.allNeighbourRoads.Contains(nextTile)) {
                    currTile.allNeighbourRoads.Add(nextTile);
                }
                if (!nextTile.allNeighbourRoads.Contains(currTile)) {
                    nextTile.allNeighbourRoads.Add(currTile);
                }
            }

            //GameObject roadGO = currTile.GetRoadGameObjectForDirection(from, to);
            //if (roadGO != null) {
            //    currTile.SetTileAsRoad(true, roadType, roadGO);
            //    if (currTile.roadType == ROAD_TYPE.MINOR) {
            //        currTile.SetRoadColor(roadGO, Color.gray);
            //        currTile.SetRoadState(false);
            //    } else if (currTile.roadType == ROAD_TYPE.MAJOR) {
            //        currTile.SetRoadColor(roadGO, Color.white);
            //        currTile.SetRoadState(false);
            //    }
            //}
            //currTile.SetPassableState(true);
        }
    }
    public void ConnectLandmarkToLandmark(BaseLandmark landmark1, BaseLandmark landmark2) {
        landmark1.AddConnection(landmark2);
        landmark2.AddConnection(landmark1);
    }
    /*
     * This will return a list of road tiles that are connected
     * to the provided hex tile using roads.
     * */
    public List<HexTile> GetRoadTilesConnectedTo(HexTile destination, HexTile start) {
        List<HexTile> connectedRoadTiles = new List<HexTile>();
        List<HexTile> tilesToCheck = _roadTiles.OrderBy(x => Vector2.Distance(start.transform.position, x.transform.position)).Take(50).ToList();
        for (int i = 0; i < tilesToCheck.Count; i++) {
            HexTile currTile = tilesToCheck[i];
            if(!connectedRoadTiles.Contains(currTile) && PathGenerator.Instance.GetPath(currTile, destination, PATHFINDING_MODE.POINT_TO_POINT) != null) {
                //There is a path from currTile to destination using roads
                connectedRoadTiles.Add(currTile);
            }
        }
        return connectedRoadTiles;
    }
    /*
     * This will return a list of road tiles that are connected
     * to the provided hex tile using roads. Restricted within region
     * */
    //public List<HexTile> GetRoadTilesConnectedTo(HexTile destination, HexTile start, Region region) {
    //    List<HexTile> connectedRoadTiles = new List<HexTile>();
    //    List<HexTile> tilesToCheck = region.roadTilesInRegion.Where(x => x.roadType == ROAD_TYPE.MINOR)
    //        .OrderBy(x => Vector2.Distance(start.transform.position, x.transform.position)).Take(50).ToList();
    //    for (int i = 0; i < tilesToCheck.Count; i++) {
    //        HexTile currTile = tilesToCheck[i];
    //        if (!connectedRoadTiles.Contains(currTile) && PathGenerator.Instance.GetPath(currTile, destination, PATHFINDING_MODE.POINT_TO_POINT) != null) {
    //            //There is a path from currTile to destination using roads
    //            connectedRoadTiles.Add(currTile);
    //        }
    //    }
    //    return connectedRoadTiles;
    //}
    //public List<HexTile> GetMinorRoadTilesConnectedTo(HexTile destination, HexTile start) {
    //    List<HexTile> connectedRoadTiles = new List<HexTile>();
    //    List<HexTile> tilesToCheck = _roadTiles.Where(x => x.roadType == ROAD_TYPE.MINOR)
    //        .OrderBy(x => Vector2.Distance(start.transform.position, x.transform.position)).Take(50).ToList();

    //    for (int i = 0; i < tilesToCheck.Count; i++) {
    //        HexTile currTile = tilesToCheck[i];
    //        if (!connectedRoadTiles.Contains(currTile) && PathGenerator.Instance.GetPath(currTile, destination, PATHFINDING_MODE.POINT_TO_POINT) != null) {
    //            //There is a path from currTile to destination using roads
    //            connectedRoadTiles.Add(currTile);
    //        }
    //    }
    //    return connectedRoadTiles;
    //}
    
    public Island MergeIslands(Island island1, Island island2, Dictionary<BaseLandmark, Island> islands) {
        if (island1 == island2) {
            return island1;
        }
        island1.AddLandmark(island2.landamrksInIsland);
        for (int i = 0; i < island2.landamrksInIsland.Count; i++) {
            BaseLandmark currLandmark = island2.landamrksInIsland[i];
            islands[currLandmark] = island1;
        }
        island2.ClearIsland();
        return island1;
    }

    public void AddTileAsRoadTile(HexTile tile) {
        if (!_roadTiles.Contains(tile)) {
            _roadTiles.Add(tile);
        }
        //if(tile.roadType == ROAD_TYPE.MAJOR) {
        //    if (!_majorRoadTiles.Contains(tile)) {
        //        _majorRoadTiles.Add(tile);
        //    }
        //} else if (tile.roadType == ROAD_TYPE.MINOR) {
        //    if (!_minorRoadTiles.Contains(tile)) {
        //        _minorRoadTiles.Add(tile);
        //    }
        //}
    }
    public void RemoveTileAsRoadTile(HexTile tile) {
        _roadTiles.Remove(tile);
        _majorRoadTiles.Remove(tile);
        _minorRoadTiles.Remove(tile);
    }

    internal void FlattenRoads() {
        for (int i = 0; i < _majorRoadTiles.Count; i++) {
            HexTile currRoadTile = _majorRoadTiles[i];
            if (currRoadTile.elevationType != ELEVATION.PLAIN) {
                currRoadTile.SetElevation(ELEVATION.PLAIN);
            }
        }

        for (int i = 0; i < _minorRoadTiles.Count; i++) {
            HexTile currRoadTile = _minorRoadTiles[i];
            if (currRoadTile.elevationType == ELEVATION.WATER) {
                currRoadTile.SetElevation(ELEVATION.PLAIN);
            }
        }

        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            if(currRegion.centerOfMass.elevationType != ELEVATION.PLAIN) {
                currRegion.centerOfMass.SetElevation(ELEVATION.PLAIN);
            }
            for (int j = 0; j < currRegion.landmarks.Count; j++) {
                BaseLandmark currLandmark = currRegion.landmarks[j];
				if (currLandmark.tileLocation.elevationType != ELEVATION.PLAIN) {
					currLandmark.tileLocation.SetElevation(ELEVATION.PLAIN);
                }
            }
        }

    }

    private BaseLandmark GetLandmarkNearestTo(BaseLandmark origin, List<BaseLandmark> choices, PATHFINDING_MODE pathfindingMode, ref List<HexTile> bestPath) {
        BaseLandmark nearestLandmark = null;
        List<HexTile> nearestPath = null;
        for (int i = 0; i < choices.Count; i++) {
            BaseLandmark currChoice = choices[i];
            if (origin.IsConnectedTo(currChoice)) {
                continue;
            }
            List<HexTile> path = PathGenerator.Instance.GetPath(origin.tileLocation, currChoice.tileLocation, pathfindingMode);
            if (path != null && path.Count <= maxRoadLength && (nearestPath == null || path.Count < nearestPath.Count)) {
                nearestPath = path;
                nearestLandmark = currChoice;
            }
        }
        bestPath = nearestPath;
        return nearestLandmark;
    }

    public void GenerateTilePassableTypes() {
        for (int i = 0; i < GridMap.Instance.hexTiles.Count; i++) {
            HexTile currTile = GridMap.Instance.hexTiles[i];
            if (currTile.isPassable) {
                currTile.DeterminePassableType();
            }
        }
    }
}

public class Island {
    private List<BaseLandmark> _landamrksInIsland;

    #region getters/setters
    public List<BaseLandmark> landamrksInIsland {
        get { return _landamrksInIsland; }
    }
    #endregion

    public Island(BaseLandmark initialLandmark) {
        _landamrksInIsland = new List<BaseLandmark>();
        AddLandmark(initialLandmark);
    }

    public void AddLandmark(BaseLandmark landmark) {
        if (!_landamrksInIsland.Contains(landmark)) {
            _landamrksInIsland.Add(landmark);
        }
    }
    public void AddLandmark(List<BaseLandmark> landmarks) {
        for (int i = 0; i < landmarks.Count; i++) {
            AddLandmark(landmarks[i]);
        }
    }

    public void RemoveLandmark(BaseLandmark landmark) {
        _landamrksInIsland.Remove(landmark);
    }
    public void RemoveLandmark(List<BaseLandmark> landmarks) {
        for (int i = 0; i < landmarks.Count; i++) {
            RemoveLandmark(landmarks[i]);
        }
    }

    public void ClearIsland() {
        _landamrksInIsland.Clear();
    }
}