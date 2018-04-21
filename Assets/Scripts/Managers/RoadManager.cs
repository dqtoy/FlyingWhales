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

    // Use this for initialization
    void Awake() {
        Instance = this;
        _roadTiles = new List<HexTile>();
        _majorRoadTiles = new List<HexTile>();
        _minorRoadTiles = new List<HexTile>();
    }

    //internal void DrawConnection(HexTile fromTile, HexTile toTile, ROAD_TYPE roadType) {
    //    Debug.Log("DRAW CONNECTION: " + fromTile.name + ", " + toTile.name);
    //    Vector3 fromPos = fromTile.gameObject.transform.position;
    //    Vector3 toPos = toTile.gameObject.transform.position;
    //    Vector3 targetDir = toPos - fromPos;

    //    //float angle = Vector3.Angle (targetDir, fromTile.transform.forward);
    //    float angle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;
    //    Debug.Log("ANGLE: " + angle);

    //    GameObject connectionGO = majorRoadGO;
    //    if (roadType == ROAD_TYPE.MINOR) {
    //        connectionGO = minorRoadGO;
    //    }
    //    GameObject goConnection = (GameObject)GameObject.Instantiate(connectionGO);
    //    goConnection.transform.position = fromPos;
    //    goConnection.transform.Rotate(new Vector3(0f, 0f, angle));
    //    if (roadType == ROAD_TYPE.MAJOR) {
    //        goConnection.transform.parent = majorRoadParent;
    //    } else if (roadType == ROAD_TYPE.MINOR) {
    //        goConnection.transform.parent = minorRoadParent;
    //    }

    //    RoadConnection roadConnection = goConnection.GetComponent<RoadConnection>();
    //    roadConnection.SetConnection(fromTile, toTile, roadType);

    //    fromTile.connectedTiles.Add(toTile, roadConnection);
    //    toTile.connectedTiles.Add(fromTile, roadConnection);
    //    //		if(fromTile.city != null && toTile.city != null){
    //    //			if(fromTile.city.kingdom.id == toTile.city.kingdom.id){
    //    //				goConnection.GetComponent<CityConnection> ().SetColor (fromTile.city.kingdom.kingdomColor);
    //    //			}
    //    //		}
    //}
    //internal void DestroyConnection(HexTile fromTile, HexTile toTile) {
    //    RoadConnection roadConnection = fromTile.connectedTiles[toTile];
    //    GameObject.Destroy(roadConnection.gameObject);
    //    fromTile.connectedTiles.Remove(toTile);
    //    toTile.connectedTiles.Remove(fromTile);
    //}

    //internal bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2) {

    //    Vector3 lineVec3 = linePoint2 - linePoint1;
    //    Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
    //    Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

    //    float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

    //    //is coplanar, and not parrallel
    //    if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f) {
    //        float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
    //        intersection = linePoint1 + (lineVec1 * s);
    //        return true;
    //    } else {
    //        intersection = Vector3.zero;
    //        return false;
    //    }
    //}

    //internal Vector3 GetIntersection(Vector3 firstPoint1, Vector3 firstPoint2, Vector3 secondPoint1, Vector3 secondPoint2) {
    //    float A1 = firstPoint2.y - firstPoint1.y;
    //    float B1 = firstPoint1.x - firstPoint2.x;
    //    float C1 = A1 * firstPoint1.x + B1 * firstPoint1.y;

    //    float A2 = secondPoint2.y - secondPoint1.y;
    //    float B2 = secondPoint1.x - secondPoint2.x;
    //    float C2 = A2 * secondPoint1.x + B2 * secondPoint1.y;

    //    float det = A1 * B2 - A2 * B1;

    //    float x = (B2 * C1 - B1 * C2) / det;
    //    float y = (A1 * C2 - A2 * C1) / det;

    //    return new Vector3(x, y, 0f);
    //}

    //internal bool IsIntersectingWith(HexTile fromTile, HexTile toTile, ROAD_TYPE roadType) {
    //    Vector3 fromPos = fromTile.gameObject.transform.position;
    //    Vector3 toPos = toTile.gameObject.transform.position;
    //    Vector3 targetDir = toPos - fromPos;
    //    //		Ray2D ray = new Ray2D (fromPos, targetDir);
    //    float distance = Vector3.Distance(fromTile.transform.position, toTile.transform.position);
    //    RaycastHit2D hit = Physics2D.Raycast(fromPos, targetDir, distance);
    //    if (hit) {
    //        //			Debug.LogError ("HIT: " + hit.collider.name);
    //        string tagName = "All";
    //        if (roadType == ROAD_TYPE.MAJOR) {
    //            tagName = "MajorRoad";
    //        } else if (roadType == ROAD_TYPE.MINOR) {
    //            tagName = "MinorRoad";
    //        }
    //        //			if(hit.collider.tag == "Hextile" && hit.collider.name == toTile.name){
    //        //				return false;
    //        //			}
    //        if (tagName == "All") {
    //            if (hit.collider.tag == "MajorRoad" || hit.collider.tag == "MinorRoad") {
    //                RoadConnection roadConnection = hit.collider.transform.parent.gameObject.GetComponent<RoadConnection>();
    //                //					Vector3 intersectionPoint = Vector3.zero;
    //                Vector3 intersectionPoint = RoadManager.Instance.GetIntersection(fromPos, toPos, roadConnection.fromTile.transform.position, roadConnection.toTile.transform.position);
    //                Debug.LogError(fromTile.name + " and " + toTile.name + " - " + roadConnection.fromTile.name + " and " + roadConnection.toTile.name + " HAS INTERSECTED AT POINT: " + intersectionPoint.ToString());

    //                return true;
    //            }
    //        } else {
    //            if (hit.collider.tag == tagName) {
    //                RoadConnection roadConnection = hit.collider.transform.parent.gameObject.GetComponent<RoadConnection>();
    //                Vector3 intersectionPoint = RoadManager.Instance.GetIntersection(fromPos, toPos, roadConnection.fromTile.transform.position, roadConnection.toTile.transform.position);
    //                Debug.LogError(fromTile.name + " and " + toTile.name + " - " + roadConnection.fromTile.name + " and " + roadConnection.toTile.name + " HAS INTERSECTED AT POINT: " + intersectionPoint.ToString());
    //                return true;
    //            }
    //        }
    //    }
    //    return false;
    //}
    ///*
    // * <summary>
    // * Generate roads connecting one region to another (center of mass -> center of mass)
    // * </summary>
    // * */
    //internal bool GenerateRegionRoads() {
    //    float startTime = Time.time;

    //    List<Region> unconnectedRegions = new List<Region>(GridMap.Instance.allRegions);

    //    int islandNum = 0;
    //    Dictionary<Region, int> regionIslandDict = new Dictionary<Region, int>();
    //    Dictionary<int, List<Region>> islands = new Dictionary<int, List<Region>>();

    //    for (int i = 0; i < unconnectedRegions.Count; i++) {
    //        Region currRegion = unconnectedRegions[i];
    //        regionIslandDict.Add(currRegion, -1);
    //    }

    //    GenerateFactionVillageRoads(ref islandNum, regionIslandDict, islands, unconnectedRegions);

    //    Region activeRegion = null;

    //    while (unconnectedRegions.Count > 0) {
    //        //Choose a random region to start, the region must not have any connections yet
    //        if (activeRegion == null) {
    //            activeRegion = unconnectedRegions[Random.Range(0, unconnectedRegions.Count)];
    //            islandNum++;
    //            regionIslandDict[activeRegion] = islandNum;
    //            islands.Add(islandNum, new List<Region>() { activeRegion });
    //        }

    //        bool hasConnected = false;
    //        List<Region> elligibleRegionsToConnectTo = new List<Region>();
    //        //Order adjacent regions by least num of connections
    //        elligibleRegionsToConnectTo.AddRange(activeRegion.adjacentRegions.Where(x => !elligibleRegionsToConnectTo.Contains(x)).OrderBy(x => x.connections.Count));

    //        //connect active region to any adjacent region, use rules
    //        for (int i = 0; i < elligibleRegionsToConnectTo.Count; i++) {
    //            Region otherRegion = elligibleRegionsToConnectTo[i];
    //            //Check if other region has reached maximum number of city connections (3)
    //            //and active region is not already connected to other region
    //            if (otherRegion.connections.Count < maxCityConnections && !activeRegion.connections.Contains(otherRegion)) {
    //                //Check if active region has a path towards other region, use rules
    //                List<HexTile> pathToOtherRegion = PathGenerator.Instance.GetPath(activeRegion.centerOfMass, otherRegion.centerOfMass, PATHFINDING_MODE.REGION_CONNECTION);
    //                if (pathToOtherRegion != null) {
    //                    hasConnected = true;
    //                    unconnectedRegions.Remove(activeRegion);
    //                    unconnectedRegions.Remove(otherRegion);
    //                    ConnectRegions(activeRegion, otherRegion);
    //                    CreateRoad(pathToOtherRegion, ROAD_TYPE.MAJOR);

    //                    int islandOfActiveRegion = regionIslandDict[activeRegion];//island ID of active region
    //                    int previousIslandOfOtherRegion = regionIslandDict[otherRegion]; //Get island id of other region, -1 means it is not part of an island yet

    //                    if (previousIslandOfOtherRegion == -1) {
    //                        //other region is not part of an island yet, add it to the active regions' island
    //                        regionIslandDict[otherRegion] = islandOfActiveRegion;
    //                        islands[islandOfActiveRegion].Add(otherRegion);
    //                    } else {
    //                        if (islandOfActiveRegion != previousIslandOfOtherRegion) {
    //                            List<Region> regionsInPreviousIsland = islands[previousIslandOfOtherRegion];
    //                            for (int j = 0; j < regionsInPreviousIsland.Count; j++) {
    //                                Region regionToTransfer = regionsInPreviousIsland[j];
    //                                if (!islands[islandOfActiveRegion].Contains(regionToTransfer)) {
    //                                    islands[islandOfActiveRegion].Add(regionToTransfer);
    //                                }
    //                                regionIslandDict[regionToTransfer] = islandOfActiveRegion;
    //                            }
    //                            islands.Remove(previousIslandOfOtherRegion);
    //                        }
    //                    }

    //                    if (otherRegion.connections.Count >= 2 || otherRegion.owner != null) {
    //                        //if the active region has connected to another region that already has 2 or more connections, 
    //                        //randomize another active region
    //                        activeRegion = null;
    //                    } else {
    //                        activeRegion = otherRegion;
    //                    }
    //                    break;
    //                }
    //            }
    //        }

    //        if (!hasConnected) {
    //            Debug.LogWarning("Cannot connect " + activeRegion.centerOfMass.name + " to any region");
    //            unconnectedRegions.Remove(activeRegion);
    //            activeRegion = null;
    //        }
    //    }

    //    List<int> islandsToRemove = new List<int>();
    //    foreach (KeyValuePair<int, List<Region>> kvp in islands) {
    //        if (kvp.Value.Count == 0) {
    //            islandsToRemove.Add(kvp.Key);
    //        }
    //    }
    //    for (int i = 0; i < islandsToRemove.Count; i++) {
    //        int currIslandToRemove = islandsToRemove[i];
    //        islands.Remove(currIslandToRemove);
    //    }

    //    //Connect Islands to each other
    //    if (islands.Count > 1) {
    //        if (!ConnectIslands(islands)) {
    //            //failed to connect islands
    //            return false;
    //        }
    //    }

    //    //Check regions that have only 1 connection
    //    List<Region> regionsToReconnect = GridMap.Instance.allRegions.Where(x => x.connections.Count == 1).ToList();
    //    for (int i = 0; i < regionsToReconnect.Count; i++) {
    //        Region currRegion = regionsToReconnect[i];
    //        ConnectRegionToAdjacentRegion(currRegion);
    //    }

    //    //Get 1/4 of regions that have 2 connections
    //    List<Region> regionsWith2Connections = GridMap.Instance.allRegions.Where(x => x.connections.Count == 2).ToList();
    //    int numOfRegionsToReconnect = Mathf.FloorToInt(regionsWith2Connections.Count / 4);

    //    for (int i = 0; i < numOfRegionsToReconnect; i++) {
    //        Region chosenRegion = regionsWith2Connections[Random.Range(0, regionsWith2Connections.Count)];
    //        regionsWith2Connections.Remove(chosenRegion);
    //        ConnectRegionToAdjacentRegion(chosenRegion);
    //    }

    //    Debug.Log("Regions with no connections: " + GridMap.Instance.allRegions.Where(x => x.connections.Count == 0).Count().ToString());
    //    Debug.Log("Regions with 1 connection: " + GridMap.Instance.allRegions.Where(x => x.connections.Count == 1).Count().ToString());
    //    Debug.Log("Regions with 2 connections: " + GridMap.Instance.allRegions.Where(x => x.connections.Count == 2).Count().ToString());
    //    Debug.Log("Regions with 3 connections: " + GridMap.Instance.allRegions.Where(x => x.connections.Count == 3).Count().ToString());
    //    Debug.Log("Regions with 4 connections: " + GridMap.Instance.allRegions.Where(x => x.connections.Count == 4).Count().ToString());
    //    return true;
    //}

   // private void GenerateFactionVillageRoads(ref int islandNum, Dictionary<Region, int> regionIslandDict, Dictionary<int, List<Region>> islands, List<Region> unconnectedRegions) {
   //     //connect all faction villages with each other
   //     for (int i = 0; i < FactionManager.Instance.allTribes.Count; i++) {
   //         Tribe currTribe = FactionManager.Instance.allTribes[i];
			//List<Region> ownedRegions = currTribe.settlements.Select(x => x.tileLocation.region).ToList();
   //         if (ownedRegions.Count > 1) {
   //             for (int j = 0; j < ownedRegions.Count; j++) {
   //                 Region currOwnedRegion = ownedRegions[j];
   //                 //the current region is not yet connected with any region from it's own faction
   //                 if (!IsRegionConnectedToSameFactionRegion(currOwnedRegion)) {
   //                     List<Region> elligibleRegions = currOwnedRegion.adjacentRegions.Where(x => x.owner != null && x.owner.id == currTribe.id).ToList();
   //                     elligibleRegions.OrderBy(x => x.centerOfMass.GetDistanceTo(currOwnedRegion.centerOfMass));
   //                     for (int k = 0; k < elligibleRegions.Count; k++) {
   //                         Region possibleRegionToConnectTo = elligibleRegions[k];
   //                         List<HexTile> pathToOtherRegion = PathGenerator.Instance.GetPath(currOwnedRegion.centerOfMass, possibleRegionToConnectTo.centerOfMass, PATHFINDING_MODE.REGION_CONNECTION);
   //                         if (pathToOtherRegion != null) {
   //                             unconnectedRegions.Remove(currOwnedRegion);
   //                             unconnectedRegions.Remove(possibleRegionToConnectTo);
   //                             ConnectRegions(currOwnedRegion, possibleRegionToConnectTo);
   //                             CreateRoad(pathToOtherRegion, ROAD_TYPE.MAJOR);

   //                             regionIslandDict[currOwnedRegion] = islandNum;
   //                             islands.Add(islandNum, new List<Region>() { currOwnedRegion });

   //                             int islandOfCurrOwnedRegion = regionIslandDict[currOwnedRegion];//island ID of active region
   //                             int previousIslandOfOtherRegion = regionIslandDict[possibleRegionToConnectTo]; //Get island id of other region, -1 means it is not part of an island yet

   //                             if (previousIslandOfOtherRegion == -1) {
   //                                 //other region is not part of an island yet, add it to the active regions' island
   //                                 regionIslandDict[possibleRegionToConnectTo] = islandOfCurrOwnedRegion;
   //                                 islands[islandOfCurrOwnedRegion].Add(possibleRegionToConnectTo);
   //                             } else {
   //                                 if (islandOfCurrOwnedRegion != previousIslandOfOtherRegion) {
   //                                     List<Region> regionsInPreviousIsland = islands[previousIslandOfOtherRegion];
   //                                     for (int l = 0; l < regionsInPreviousIsland.Count; l++) {
   //                                         Region regionToTransfer = regionsInPreviousIsland[l];
   //                                         if (!islands[islandOfCurrOwnedRegion].Contains(regionToTransfer)) {
   //                                             islands[islandOfCurrOwnedRegion].Add(regionToTransfer);
   //                                         }
   //                                         regionIslandDict[regionToTransfer] = islandOfCurrOwnedRegion;
   //                                     }
   //                                     islands.Remove(previousIslandOfOtherRegion);
   //                                 }
   //                             }
   //                             islandNum++;
   //                             break;
   //                         }
   //                     }
   //                 }
   //             }
   //         }

   //     }
   // }

    //private bool IsRegionConnectedToSameFactionRegion(Region region) {
    //    if (region.owner == null) {
    //        return true;
    //    } else {
    //        List<Region> connections = region.connections.Where(x => x is Region).Select(x => x as Region).ToList();
    //        if (connections.Where(x => x.owner != null && x.owner.id == region.owner.id).Any()) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    //private void ConnectRegionToAdjacentRegion(Region region) {
    //    for (int i = 0; i < region.adjacentRegions.Count; i++) {
    //        Region otherRegion = region.adjacentRegions[i];
    //        //Check if other region has reached maximum number of city connections (3)
    //        //and active region is not already connected to other region
    //        if (region.connections.Count < maxCityConnections && otherRegion.connections.Count < maxCityConnections && !region.connections.Contains(otherRegion)) {
    //            //Check if active region has a path towards other region, use rules
    //            List<HexTile> pathToOtherRegion = PathGenerator.Instance.GetPath(region.centerOfMass, otherRegion.centerOfMass, PATHFINDING_MODE.REGION_CONNECTION);
    //            if (pathToOtherRegion != null) {
    //                ConnectRegions(region, otherRegion);
    //                CreateRoad(pathToOtherRegion, ROAD_TYPE.MAJOR);
    //                break;
    //            }
    //        }
    //    }
    //}

    //private bool ConnectIslands(Dictionary<int, List<Region>> islands) {
    //    int mainIslandKey = islands.Keys.First();
    //    List<Region> regionsInMainIsland = islands[mainIslandKey];
    //    for (int i = 1; i < islands.Keys.Count; i++) {
    //        int otherIslandKey = islands.Keys.ElementAt(i);
    //        List<Region> regionsInOtherIsland = islands[otherIslandKey];
    //        bool hasFoundConnection = false;
    //        //Find a possible connection for the 2 islands
    //        for (int j = 0; j < regionsInMainIsland.Count; j++) {
    //            Region currMainIslandRegion = regionsInMainIsland[j];
    //            for (int k = 0; k < regionsInOtherIsland.Count; k++) {
    //                Region otherIslandRegion = regionsInOtherIsland[k];
    //                //Check if both regions are adjacent and still has a slot for connection
    //                //if (currMainIslandRegion.connections.Count < maxCityConnections && otherIslandRegion.connections.Count < maxCityConnections
    //                //    && currMainIslandRegion.adjacentRegions.Contains(otherIslandRegion)) {
    //                if (currMainIslandRegion.adjacentRegions.Contains(otherIslandRegion)) {
    //                    //Check if the 2 regions have a path towards each other, using the rules
    //                    List<HexTile> pathToOtherRegion = PathGenerator.Instance
    //                        .GetPath(currMainIslandRegion.centerOfMass, otherIslandRegion.centerOfMass, PATHFINDING_MODE.REGION_CONNECTION);
    //                    if (pathToOtherRegion != null) {
    //                        regionsInMainIsland.AddRange(islands[otherIslandKey]);
    //                        islands[otherIslandKey].Clear();
    //                        hasFoundConnection = true;
    //                        ConnectRegions(currMainIslandRegion, otherIslandRegion);
    //                        CreateRoad(pathToOtherRegion, ROAD_TYPE.MAJOR);
    //                        break;
    //                    }
    //                }
    //            }
    //            if (hasFoundConnection) {
    //                break;
    //            }
    //        }
    //    }

    //    for (int i = 1; i < islands.Keys.Count; i++) {
    //        int otherIslandKey = islands.Keys.ElementAt(i);
    //        if(islands[otherIslandKey].Count > 0) {
    //            return false;
    //        }
    //    }
    //    return true;
    //}
    ///*
    // * <summary>
    // * Connect 2 regions
    // * This will generate the roads and set the road sprites as active
    // * </summary>
    // * */
    //private void ConnectRegions(Region region1, Region region2) {
    //    region1.AddConnection(region2);
    //    region2.AddConnection(region1);
    //    region1.centerOfMass.landmarkOnTile.AddConnection(region2.centerOfMass.landmarkOnTile);
    //    region2.centerOfMass.landmarkOnTile.AddConnection(region1.centerOfMass.landmarkOnTile);
    //    //if (region1.connections.Count > RoadManager.Instance.maxCityConnections) {
    //    //    throw new System.Exception("Exceeded maximum connections!");
    //    //}
    //    //if (region2.connections.Count > RoadManager.Instance.maxCityConnections) {
    //    //    throw new System.Exception("Exceeded maximum connections!");
    //    //}
    //    //List<HexTile> connection = PathGenerator.Instance.GetPath(region1.centerOfMass, region2.centerOfMass, PATHFINDING_MODE.ROAD_CREATION);
    //    //if(connection == null) {
    //    //    throw new System.Exception("Cannot connect " + region1.centerOfMass.name + " to " + region2.centerOfMass.name);
    //    //}
    //    //CreateRoad(connection, ROAD_TYPE.MAJOR);
    //    //SmartCreateRoad(region1.centerOfMass, region2.centerOfMass, PATHFINDING_MODE.ROAD_CREATION, ROAD_TYPE.MAJOR);
    //}

    public bool GenerateRoads() {
        List<BaseLandmark> allLandmarks = LandmarkManager.Instance.GetAllLandmarks();
        Dictionary<BaseLandmark, Island> islands = new Dictionary<BaseLandmark, Island>();
        for (int i = 0; i < allLandmarks.Count; i++) {
            islands.Add(allLandmarks[i], new Island(allLandmarks[i]));
        }
        if (!ConnectFactionRegions(islands)) {
            return false;
        }
        
        if (!ConnectRegionLandmarks(islands)) {
            return false;
        }
        
        allLandmarks = allLandmarks.Where(x => x.connections.Count < recommendedLandmarkConnections).ToList();
        //choose a random landmark to start
        BaseLandmark initialLandmark = allLandmarks[Random.Range(0, allLandmarks.Count)];
        Queue<BaseLandmark> landmarkQueue = new Queue<BaseLandmark>();
        landmarkQueue.Enqueue(initialLandmark);

        while (landmarkQueue.Count > 0) {
            BaseLandmark currLandmark = landmarkQueue.Dequeue();
            //yield return new WaitForSeconds(2f);
            //UIManager.Instance.ShowLandmarkInfo(currLandmark);
            BaseLandmark landmarkToConnectTo = GetLandmarkForConnection(currLandmark);
            if (landmarkToConnectTo != null) {
                ConnectLandmarkToLandmark(currLandmark, landmarkToConnectTo);
                //yield return new WaitForSeconds(1f);
                CreateRoad(PathGenerator.Instance.GetPath(currLandmark.tileLocation, landmarkToConnectTo.tileLocation, PATHFINDING_MODE.LANDMARK_CONNECTION), ROAD_TYPE.MAJOR);
                Island islandOfCurrLandmark = islands[currLandmark];
                Island islandOfOtherLandmark = islands[landmarkToConnectTo];
                MergeIslands(islandOfCurrLandmark, islandOfOtherLandmark, islands);
                if (currLandmark.connections.Count >= recommendedLandmarkConnections) {
                    allLandmarks.Remove(currLandmark);
                }
                if (landmarkToConnectTo.connections.Count >= recommendedLandmarkConnections) {
                    allLandmarks.Remove(landmarkToConnectTo);
                    //enqueue a random landmark that can still connect
                    if (allLandmarks.Count > 0) {
                        landmarkQueue.Enqueue(allLandmarks[Random.Range(0, allLandmarks.Count)]);
                    }
                } else {
                    landmarkQueue.Enqueue(landmarkToConnectTo);
                }
            } else {
                //throw new System.Exception(currLandmark.landmarkName + " cannot find a landmark to connect to!");
                allLandmarks.Remove(currLandmark);
                if (allLandmarks.Count > 0) {
                    landmarkQueue.Enqueue(allLandmarks[Random.Range(0, allLandmarks.Count)]);
                }
            }
        }
        if (!CheckForIslands(islands)) {
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
    private bool ConnectRegionLandmarks(Dictionary<BaseLandmark, Island> islands) {
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            if (currRegion.landmarks.Count <= 1) {
                continue; //skip
            }
            for (int j = 0; j < currRegion.landmarks.Count; j++) {
                BaseLandmark currLandmark = currRegion.landmarks[j];
                List<BaseLandmark> otherLandmarks = new List<BaseLandmark>(currRegion.landmarks.Where(x => x.id != currLandmark.id && PathGenerator.Instance.GetPath(currLandmark.tileLocation, x.tileLocation, PATHFINDING_MODE.LANDMARK_CONNECTION) != null)
                    .OrderBy(y => PathGenerator.Instance.GetPath(currLandmark.tileLocation, y.tileLocation, PATHFINDING_MODE.LANDMARK_CONNECTION).Count));

                for (int k = 0; k < otherLandmarks.Count; k++) {
                    BaseLandmark otherLandmark = otherLandmarks[k];
                    if (currLandmark.id != otherLandmark.id && otherLandmark.connections.Count < maxLandmarkConnections && !currLandmark.IsConnectedTo(otherLandmark) && !currLandmark.IsIndirectlyConnectedTo(otherLandmark)) {
                        List<HexTile> path = PathGenerator.Instance.GetPath(currLandmark.tileLocation, otherLandmark.tileLocation, PATHFINDING_MODE.LANDMARK_CONNECTION);
                        if (path != null && path.Count < maxRoadLength) {
                            Island islandOfChosenLandmark = islands[currLandmark];
                            Island islandOfOtherLandmark = islands[otherLandmark];
                            MergeIslands(islandOfChosenLandmark, islandOfOtherLandmark, islands);
                            ConnectLandmarkToLandmark(currLandmark, otherLandmark);
                            CreateRoad(path, ROAD_TYPE.MAJOR);
                        }
                    }
                    if (currLandmark.connections.Count >= maxLandmarkConnections) {
                        break;
                    }
                }
                if (!currLandmark.IsConnectedTo(currRegion)) {
                    return false; //The current landmark has not connected to a landmark in its region, return a failure
                }

                //if (!currLandmark.IsConnectedTo(currRegion)) { //currLandmark is not yet connected to a region from its region
                    //List<BaseLandmark> choices = new List<BaseLandmark>();
                    //choices.AddRange(currRegion.landmarks);
                    //choices.Remove(currLandmark);
                    ////connect the currLandmark to the nearest landmark
                    //List<HexTile> path = new List<HexTile>();
                    //BaseLandmark nearestLandmark = GetLandmarkNearestTo(currLandmark, choices, PATHFINDING_MODE.LANDMARK_CONNECTION, ref path);
                    //if (nearestLandmark != null) {
                    //    Island islandOfChosenLandmark = islands[currLandmark];
                    //    Island islandOfOtherLandmark = islands[nearestLandmark];
                    //    MergeIslands(islandOfChosenLandmark, islandOfOtherLandmark, islands);
                    //    ConnectLandmarkToLandmark(currLandmark, nearestLandmark);
                    //    CreateRoad(path, ROAD_TYPE.MAJOR);
                    //} else {
                    //    throw new System.Exception(currLandmark.landmarkName + " could not connect to any landmark");
                    //}
                //}
            }
        }
        return true;
    }
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

            GameObject roadGO = currTile.GetRoadGameObjectForDirection(from, to);
            if(roadGO != null) {
                currTile.SetTileAsRoad(true, roadType, roadGO);
                if (currTile.roadType == ROAD_TYPE.MINOR) {
                    currTile.SetRoadColor(roadGO, Color.gray);
                    currTile.SetRoadState(true);
                    //currTile.SetRoadState(!GameManager.Instance.initiallyHideRoads);
                } else if (currTile.roadType == ROAD_TYPE.MAJOR) {
                    currTile.SetRoadColor(roadGO, Color.white);
                    currTile.SetRoadState(true); //Major Roads should already be visible
                }
            }
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
    public List<HexTile> GetRoadTilesConnectedTo(HexTile destination, HexTile start, Region region) {
        List<HexTile> connectedRoadTiles = new List<HexTile>();
        List<HexTile> tilesToCheck = region.roadTilesInRegion.Where(x => x.roadType == ROAD_TYPE.MINOR)
            .OrderBy(x => Vector2.Distance(start.transform.position, x.transform.position)).Take(50).ToList();
        for (int i = 0; i < tilesToCheck.Count; i++) {
            HexTile currTile = tilesToCheck[i];
            if (!connectedRoadTiles.Contains(currTile) && PathGenerator.Instance.GetPath(currTile, destination, PATHFINDING_MODE.POINT_TO_POINT) != null) {
                //There is a path from currTile to destination using roads
                connectedRoadTiles.Add(currTile);
            }
        }
        return connectedRoadTiles;
    }
    public List<HexTile> GetMinorRoadTilesConnectedTo(HexTile destination, HexTile start) {
        List<HexTile> connectedRoadTiles = new List<HexTile>();
        List<HexTile> tilesToCheck = _roadTiles.Where(x => x.roadType == ROAD_TYPE.MINOR)
            .OrderBy(x => Vector2.Distance(start.transform.position, x.transform.position)).Take(50).ToList();

        for (int i = 0; i < tilesToCheck.Count; i++) {
            HexTile currTile = tilesToCheck[i];
            if (!connectedRoadTiles.Contains(currTile) && PathGenerator.Instance.GetPath(currTile, destination, PATHFINDING_MODE.POINT_TO_POINT) != null) {
                //There is a path from currTile to destination using roads
                connectedRoadTiles.Add(currTile);
            }
        }
        return connectedRoadTiles;
    }
    
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
        if(tile.roadType == ROAD_TYPE.MAJOR) {
            if (!_majorRoadTiles.Contains(tile)) {
                _majorRoadTiles.Add(tile);
            }
        } else if (tile.roadType == ROAD_TYPE.MINOR) {
            if (!_minorRoadTiles.Contains(tile)) {
                _minorRoadTiles.Add(tile);
            }
        }
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