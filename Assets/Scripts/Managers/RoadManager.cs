using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RoadManager : MonoBehaviour {
    public static RoadManager Instance = null;

    public Transform majorRoadParent;
    public Transform minorRoadParent;

    public GameObject majorRoadGO;
    public GameObject minorRoadGO;

    public int maxConnections;
    public int maxCityConnections;
    public int maxLandmarkConnections;

    private List<HexTile> _roadTiles;

    // Use this for initialization
    void Awake() {
        Instance = this;
        _roadTiles = new List<HexTile>();
    }


    internal void DrawConnection(HexTile fromTile, HexTile toTile, ROAD_TYPE roadType) {
        Debug.Log("DRAW CONNECTION: " + fromTile.name + ", " + toTile.name);
        Vector3 fromPos = fromTile.gameObject.transform.position;
        Vector3 toPos = toTile.gameObject.transform.position;
        Vector3 targetDir = toPos - fromPos;

        //float angle = Vector3.Angle (targetDir, fromTile.transform.forward);
        float angle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;
        Debug.Log("ANGLE: " + angle);

        GameObject connectionGO = majorRoadGO;
        if (roadType == ROAD_TYPE.MINOR) {
            connectionGO = minorRoadGO;
        }
        GameObject goConnection = (GameObject)GameObject.Instantiate(connectionGO);
        goConnection.transform.position = fromPos;
        goConnection.transform.Rotate(new Vector3(0f, 0f, angle));
        if (roadType == ROAD_TYPE.MAJOR) {
            goConnection.transform.parent = majorRoadParent;
        } else if (roadType == ROAD_TYPE.MINOR) {
            goConnection.transform.parent = minorRoadParent;
        }

        RoadConnection roadConnection = goConnection.GetComponent<RoadConnection>();
        roadConnection.SetConnection(fromTile, toTile, roadType);

        fromTile.connectedTiles.Add(toTile, roadConnection);
        toTile.connectedTiles.Add(fromTile, roadConnection);
        //		if(fromTile.city != null && toTile.city != null){
        //			if(fromTile.city.kingdom.id == toTile.city.kingdom.id){
        //				goConnection.GetComponent<CityConnection> ().SetColor (fromTile.city.kingdom.kingdomColor);
        //			}
        //		}
    }
    internal void DestroyConnection(HexTile fromTile, HexTile toTile) {
        RoadConnection roadConnection = fromTile.connectedTiles[toTile];
        GameObject.Destroy(roadConnection.gameObject);
        fromTile.connectedTiles.Remove(toTile);
        toTile.connectedTiles.Remove(fromTile);
    }

    internal bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2) {

        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        //is coplanar, and not parrallel
        if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f) {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineVec1 * s);
            return true;
        } else {
            intersection = Vector3.zero;
            return false;
        }
    }

    internal Vector3 GetIntersection(Vector3 firstPoint1, Vector3 firstPoint2, Vector3 secondPoint1, Vector3 secondPoint2) {
        float A1 = firstPoint2.y - firstPoint1.y;
        float B1 = firstPoint1.x - firstPoint2.x;
        float C1 = A1 * firstPoint1.x + B1 * firstPoint1.y;

        float A2 = secondPoint2.y - secondPoint1.y;
        float B2 = secondPoint1.x - secondPoint2.x;
        float C2 = A2 * secondPoint1.x + B2 * secondPoint1.y;

        float det = A1 * B2 - A2 * B1;

        float x = (B2 * C1 - B1 * C2) / det;
        float y = (A1 * C2 - A2 * C1) / det;

        return new Vector3(x, y, 0f);
    }

    internal bool IsIntersectingWith(HexTile fromTile, HexTile toTile, ROAD_TYPE roadType) {
        Vector3 fromPos = fromTile.gameObject.transform.position;
        Vector3 toPos = toTile.gameObject.transform.position;
        Vector3 targetDir = toPos - fromPos;
        //		Ray2D ray = new Ray2D (fromPos, targetDir);
        float distance = Vector3.Distance(fromTile.transform.position, toTile.transform.position);
        RaycastHit2D hit = Physics2D.Raycast(fromPos, targetDir, distance);
        if (hit) {
            //			Debug.LogError ("HIT: " + hit.collider.name);
            string tagName = "All";
            if (roadType == ROAD_TYPE.MAJOR) {
                tagName = "MajorRoad";
            } else if (roadType == ROAD_TYPE.MINOR) {
                tagName = "MinorRoad";
            }
            //			if(hit.collider.tag == "Hextile" && hit.collider.name == toTile.name){
            //				return false;
            //			}
            if (tagName == "All") {
                if (hit.collider.tag == "MajorRoad" || hit.collider.tag == "MinorRoad") {
                    RoadConnection roadConnection = hit.collider.transform.parent.gameObject.GetComponent<RoadConnection>();
                    //					Vector3 intersectionPoint = Vector3.zero;
                    Vector3 intersectionPoint = RoadManager.Instance.GetIntersection(fromPos, toPos, roadConnection.fromTile.transform.position, roadConnection.toTile.transform.position);
                    Debug.LogError(fromTile.name + " and " + toTile.name + " - " + roadConnection.fromTile.name + " and " + roadConnection.toTile.name + " HAS INTERSECTED AT POINT: " + intersectionPoint.ToString());

                    return true;
                }
            } else {
                if (hit.collider.tag == tagName) {
                    RoadConnection roadConnection = hit.collider.transform.parent.gameObject.GetComponent<RoadConnection>();
                    Vector3 intersectionPoint = RoadManager.Instance.GetIntersection(fromPos, toPos, roadConnection.fromTile.transform.position, roadConnection.toTile.transform.position);
                    Debug.LogError(fromTile.name + " and " + toTile.name + " - " + roadConnection.fromTile.name + " and " + roadConnection.toTile.name + " HAS INTERSECTED AT POINT: " + intersectionPoint.ToString());
                    return true;
                }
            }
        }
        return false;
    }

    /*
     * <summary>
     * Generate roads connecting one region to another (center of mass -> center of mass)
     * </summary>
     * */
    internal void GenerateRegionRoads() {
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            //Randomize number of connections from 1 - maxCityConnections
            int numOfConnectionsForRegion = Random.Range(1, maxCityConnections + 1);
            //Check if region is already connected to randomized number of regions
            if(currRegion.connections.Count >= numOfConnectionsForRegion || currRegion.connections.Count >= maxCityConnections) {
                //region is already connected to n amount of regions or has reached the maximum number of connections, skip it
                continue;
            }

            //Get adjacent regions that this region is not already connected to and has not exceeded the maximum number of connections
            List<Region> elligibleRegionsToConnectTo = currRegion.adjacentRegions.Where(x => !currRegion.connections.Contains(x) && x.connections.Count < maxCityConnections).ToList();
            if(elligibleRegionsToConnectTo.Count <= 0) {
                //There are no elligible regions to connect to, skip
                continue;
            }

            elligibleRegionsToConnectTo = elligibleRegionsToConnectTo.OrderByDescending(x => x.connections.Count).ToList();

            //region has less connections that the randomized number of connections, generate n number of connections
            //take into account this regions' current connections
            int connectionsToGenerate = numOfConnectionsForRegion - currRegion.connections.Count;
            if(connectionsToGenerate > elligibleRegionsToConnectTo.Count) {
                connectionsToGenerate = elligibleRegionsToConnectTo.Count;
            }

            for (int j = 0; j < connectionsToGenerate; j++) {
                Region regionToConnectTo = elligibleRegionsToConnectTo.First();
                ConnectRegions(currRegion, regionToConnectTo);
                elligibleRegionsToConnectTo.RemoveAt(0);
            }
        }

        //Check that all regions are connected to at least 1 other region, if not, connect that region to an adjacent region with the
        //least amount of connections.
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            if(currRegion.connections.Count <= 0) {
                Region regionToConnectTo = currRegion.adjacentRegions.OrderBy(x => x.connections.Count).First();
                ConnectRegions(currRegion, regionToConnectTo);
            }
        }
    }

    /*
     * <summary>
     * Connect 2 regions
     * This will generate the roads and set the road sprites as active
     * </summary>
     * */
    private void ConnectRegions(Region region1, Region region2) {
        region1.AddConnection(region2);
        region2.AddConnection(region1);
        List<HexTile> connection = PathGenerator.Instance.GetPath(region1.centerOfMass, region2.centerOfMass, PATHFINDING_MODE.ROAD_CREATION);
        if(connection == null) {
            throw new System.Exception("Cannot connect " + region1.centerOfMass.name + " to " + region2.centerOfMass.name);
        }
        //CreateRoad(connection, ROAD_TYPE.MAJOR);
        SmartCreateRoad(region1.centerOfMass, region2.centerOfMass, ROAD_TYPE.MAJOR);
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
            if (previousTile != null && nextTile != null) {
                HEXTILE_DIRECTION from = currTile.GetNeighbourDirection(previousTile);
                HEXTILE_DIRECTION to = currTile.GetNeighbourDirection(nextTile);
                GameObject roadGO = currTile.GetRoadGameObjectForDirection(from, to);
                roadGO.SetActive(true);
                currTile.SetTileAsRoad(true, roadType);
                if(currTile.roadType == ROAD_TYPE.MINOR) {
                    currTile.SetRoadColor(roadGO, Color.gray);
                } else if (currTile.roadType == ROAD_TYPE.MAJOR) {
                    currTile.SetRoadColor(roadGO, Color.white);
                }
            }
        }
    }

    public void SmartCreateRoad(HexTile start, HexTile destination, ROAD_TYPE roadType) {
        List<HexTile> roadTilesConnectedToDestination = GetRoadTilesConnectedTo(destination);
        HexTile tileToConnectTo = destination;
        float shortestDistanceFromStart = Vector2.Distance(start.transform.position, destination.transform.position);
        for (int i = 0; i < roadTilesConnectedToDestination.Count; i++) {
            HexTile currRoadTile = roadTilesConnectedToDestination[i];
            float distanceFromStart = Vector2.Distance(start.transform.position, currRoadTile.transform.position);
            if(distanceFromStart < shortestDistanceFromStart) {
                shortestDistanceFromStart = distanceFromStart;
                tileToConnectTo = currRoadTile;
            }
        }
        List<HexTile> path = PathGenerator.Instance.GetPath(start, tileToConnectTo, PATHFINDING_MODE.ROAD_CREATION);
        CreateRoad(path, roadType);
    }

    public void ConnectLandmarkToRegion(HexTile landmarkLocation, Region region) {
        region.AddConnection(landmarkLocation);
        landmarkLocation.landmark.AddConnection(region);
    }
    public void ConnectLandmarkToLandmark(HexTile landmarkLocation1, HexTile landmarkLocation2) {
        landmarkLocation1.landmark.AddConnection(landmarkLocation2);
        landmarkLocation2.landmark.AddConnection(landmarkLocation1);
    }

    /*
     * This will return a list of road tiles that are connected
     * to the provided hex tile using roads.
     * */
    public List<HexTile> GetRoadTilesConnectedTo(HexTile destination) {
        List<HexTile> connectedRoadTiles = new List<HexTile>();
        for (int i = 0; i < _roadTiles.Count; i++) {
            HexTile currTile = _roadTiles[i];
            if(!connectedRoadTiles.Contains(currTile) && PathGenerator.Instance.GetPath(currTile, destination, PATHFINDING_MODE.USE_ROADS) != null) {
                //There is a path from currTile to destination using roads
                connectedRoadTiles.Add(currTile);
            }
        }
        return connectedRoadTiles;
    }

    public void AddTileAsRoadTile(HexTile tile) {
        if (!_roadTiles.Contains(tile)) {
            _roadTiles.Add(tile);
        }
    }
    public void RemoveTileAsRoadTile(HexTile tile) {
        _roadTiles.Remove(tile);
    }
}
