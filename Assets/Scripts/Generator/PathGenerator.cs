using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class PathGenerator : MonoBehaviour {

	public static PathGenerator Instance = null;

	private List<HexTile> roadTiles = new List<HexTile>();

    #region For Testing
    [Header("For Testing")]
    [SerializeField]
    private HexTile startTile;
    [SerializeField] private HexTile targetTile;
    public PATHFINDING_MODE modeToUse;

    [ContextMenu("Get Path")]
    public void GetPathForTesting() {
        List<HexTile> path = GetPath(startTile, targetTile, modeToUse);
        if (path != null) {
            Debug.Log("========== Path from " + startTile.name + " to " + targetTile.name + "============");
            for (int i = 0; i < path.Count; i++) {
                Debug.Log(path[i].name, path[i]);
            }
        } else {
            Debug.LogError("Cannot get path from " + startTile.name + " to " + targetTile.name + " using " + modeToUse.ToString());
        }
    }

    [ContextMenu("Log All Paths")]
    public void LogAllPaths() {
        List<List<HexTile>> allPaths = GetAllPaths(startTile, targetTile);
        for (int i = 0; i < allPaths.Count; i++) {
            List<HexTile> currPath = allPaths[i];
            for (int j = 0; j < currPath.Count; j++) {
                currPath[j].HighlightRoad(Color.red);
            }
        }
    }

    [ContextMenu("Get Distance")]
    public void GetDistance() {
        Debug.Log(Vector2.Distance(startTile.transform.position, targetTile.transform.position));
    }

    [ContextMenu("Show all road tiles")]
    public void ShowAllRoadTiles() {
        foreach (HexTile h in roadTiles) {
            h.spriteRenderer.color = Color.red;
        }
    }

    [ContextMenu("Hide all road tiles")]
    public void HideAllRoadTiles() {
        foreach (HexTile h in roadTiles) {
            h.spriteRenderer.color = Color.white;
        }
    }
    #endregion

    void Awake(){
		Instance = this;
	}

	/*
	 * Get List of tiles (Path) that will connect 2 city tiles
	 * */
	public List<HexTile> GetPath(ILocation startingTile, ILocation destinationTile, PATHFINDING_MODE pathfindingMode, object data = null){
		if(startingTile == null || destinationTile == null){
			return null;
		}
        //if(startingTile.tileTag != destinationTile.tileTag) {
        //    return null;
        //}

//        bool isStartingTileRoad = startingTile.isRoad;
//        bool isDestinationTileRoad = destinationTile.isRoad;

        //bool doesStartingTileHaveLandmark = startingTile.hasLandmark;
        //bool doesDestinationTileHaveLandmark = destinationTile.hasLandmark;

//		if (pathfindingMode == PATHFINDING_MODE.POINT_TO_POINT || pathfindingMode == PATHFINDING_MODE.USE_ROADS_WITH_ALLIES || pathfindingMode == PATHFINDING_MODE.USE_ROADS_ONLY_KINGDOM || pathfindingMode == PATHFINDING_MODE.USE_ROADS_TRADE
//			|| pathfindingMode == PATHFINDING_MODE.MAJOR_ROADS || pathfindingMode == PATHFINDING_MODE.MINOR_ROADS 
//			|| pathfindingMode == PATHFINDING_MODE.MAJOR_ROADS_ONLY_KINGDOM || pathfindingMode == PATHFINDING_MODE.MINOR_ROADS_ONLY_KINGDOM) {
//			startingTile.isRoad = true;
//			destinationTile.isRoad = true;
//		}

        Func<HexTile, HexTile, double> distance = (node1, node2) => 1;
		Func<HexTile, double> estimate = t => Math.Sqrt (Math.Pow (t.xCoordinate - destinationTile.tileLocation.xCoordinate, 2) + Math.Pow (t.yCoordinate - destinationTile.tileLocation.yCoordinate, 2));

		var path = PathFind.PathFind.FindPath (startingTile.tileLocation, destinationTile.tileLocation, distance, estimate, pathfindingMode, data);

//		if (pathfindingMode == PATHFINDING_MODE.POINT_TO_POINT || pathfindingMode == PATHFINDING_MODE.USE_ROADS_WITH_ALLIES || pathfindingMode == PATHFINDING_MODE.USE_ROADS_ONLY_KINGDOM || pathfindingMode == PATHFINDING_MODE.USE_ROADS_TRADE
//			|| pathfindingMode == PATHFINDING_MODE.MAJOR_ROADS || pathfindingMode == PATHFINDING_MODE.MINOR_ROADS 
//			|| pathfindingMode == PATHFINDING_MODE.MAJOR_ROADS_ONLY_KINGDOM || pathfindingMode == PATHFINDING_MODE.MINOR_ROADS_ONLY_KINGDOM) {
//			startingTile.isRoad = isStartingTileRoad;
//			destinationTile.isRoad = isDestinationTileRoad;
//		}

        if (path != null) {
			if (pathfindingMode == PATHFINDING_MODE.REGION_CONNECTION || pathfindingMode == PATHFINDING_MODE.USE_ROADS_TRADE ||
                pathfindingMode == PATHFINDING_MODE.LANDMARK_ROADS || pathfindingMode == PATHFINDING_MODE.LANDMARK_CONNECTION || pathfindingMode == PATHFINDING_MODE.UNRESTRICTED) {
				return path.Reverse().ToList();
			} else {
				List<HexTile> newPath = path.Reverse().ToList();
				if (newPath.Count > 1) {
					newPath.RemoveAt(0);
				}
				return newPath;
			}
		}
		return null;
	}
    public List<LocationGridTile> GetPath(LocationGridTile startingTile, LocationGridTile destinationTile, GRID_PATHFINDING_MODE pathMode = GRID_PATHFINDING_MODE.NORMAL) {
        LocationGridTile.Tile_Type startType = startingTile.tileType;
        LocationGridTile.Tile_Type destinationType = destinationTile.tileType;
        switch (pathMode) {
            case GRID_PATHFINDING_MODE.NORMAL:
                startingTile.SetTileType(LocationGridTile.Tile_Type.Empty);
                destinationTile.SetTileType(LocationGridTile.Tile_Type.Empty);
                break;
            case GRID_PATHFINDING_MODE.ROADS_ONLY:
                startingTile.SetTileType(LocationGridTile.Tile_Type.Road);
                destinationTile.SetTileType(LocationGridTile.Tile_Type.Road);
                break;
            case GRID_PATHFINDING_MODE.REALISTIC:
                startingTile.SetTileState(LocationGridTile.Tile_State.Empty);
                destinationTile.SetTileState(LocationGridTile.Tile_State.Empty);
                break;
            default:
                startingTile.SetTileType(LocationGridTile.Tile_Type.Empty);
                destinationTile.SetTileType(LocationGridTile.Tile_Type.Empty);
                break;
        }
       

        Func<LocationGridTile, LocationGridTile, double> distance = (node1, node2) => 1;
        Func<LocationGridTile, double> estimate = t => Math.Sqrt(Math.Pow(t.localPlace.x - destinationTile.localPlace.x, 2) + Math.Pow(t.localPlace.y - destinationTile.localPlace.y, 2));
        var path = PathFind.PathFind.FindPath(startingTile, destinationTile, distance, estimate, pathMode);

        startingTile.SetTileType(startType);
        destinationTile.SetTileType(destinationType);

        if (path != null) {
            List<LocationGridTile> truePath = path.Reverse().ToList();
            truePath.RemoveAt(0);
            return truePath;
        }
        return null;
    }
    public PathFindingThread CreatePath(CharacterAvatar characterAvatar, HexTile startingTile, HexTile destinationTile, PATHFINDING_MODE pathfindingMode, object data = null) {
        if (startingTile == null || destinationTile == null) {
            return null;
        }
        //if (startingTile.tileTag != destinationTile.tileTag) {
        //    return null;
        //}
        PathFindingThread newThread = new PathFindingThread(characterAvatar, startingTile, destinationTile, pathfindingMode, data);
        MultiThreadPool.Instance.AddToThreadPool(newThread);
        return newThread;
    }
    public PathFindingThread CreatePath(BaseLandmark landmark, HexTile startingTile, HexTile destinationTile, PATHFINDING_MODE pathfindingMode, object data = null) {
        if (startingTile == null || destinationTile == null) {
            return null;
        }
        //if (startingTile.tileTag != destinationTile.tileTag) {
        //    return null;
        //}
        PathFindingThread newThread = new PathFindingThread(landmark, startingTile, destinationTile, pathfindingMode, data);
        MultiThreadPool.Instance.AddToThreadPool(newThread);
        return newThread;
    }
    /*
	 * Counts the number of hex tiles between two input tiles
	 * */
    public int GetDistanceBetweenTwoTiles(HexTile startingTile, HexTile destinationTile){
		Func<HexTile, HexTile, double> distance = (node1, node2) => 1;
		Func<HexTile, double> estimate = t => Math.Sqrt(Math.Pow(t.xCoordinate - destinationTile.xCoordinate, 2) + Math.Pow(t.yCoordinate - destinationTile.yCoordinate, 2));
		var path = PathFind.PathFind.FindPath(startingTile, destinationTile, distance, estimate, PATHFINDING_MODE.POINT_TO_POINT);

		if (path != null) {			
			return (path.ToList().Count - 1);
		}
		return 99999;
	}

    // Gets all paths from
    // 's' to 'd'
    public List<List<HexTile>> GetAllPaths(HexTile s, HexTile d) {
        List<List<HexTile>> paths = new List<List<HexTile>>();

        Dictionary<HexTile, bool> isVisited = new Dictionary<HexTile, bool>();
        for (int i = 0; i < RoadManager.Instance.roadTiles.Count; i++) {
            isVisited.Add(RoadManager.Instance.roadTiles[i], false);
        }
        List<BaseLandmark> allLandmarks = LandmarkManager.Instance.GetAllLandmarks();
        for (int i = 0; i < allLandmarks.Count; i++) {
            isVisited.Add(allLandmarks[i].tileLocation, false);
        }
        List<HexTile> pathList = new List<HexTile>();

        //add source to path[]
        pathList.Add(s);

        Debug.Log("Following are all different paths from " + s.name + " to " + d.name);

        //Call recursive utility
        GetAllPathsUtil(s, d, isVisited, pathList, paths);
        return paths;
    }

    // A recursive function to print
    // all paths from 'u' to 'd'.
    // isVisited[] keeps track of
    // vertices in current path.
    // localPathList<> stores actual
    // vertices in the current path
    private void GetAllPathsUtil(HexTile u, HexTile d, Dictionary<HexTile, bool> isVisited,
                            List<HexTile> localPathList, List<List<HexTile>> allPaths) {

        // Mark the current node
        isVisited[u] = true;

        if (u.id == d.id) {
            string text = string.Empty;
            allPaths.Add(new List<HexTile>(localPathList));
            localPathList.ForEach(o => text += o.name + " -> ");
            Debug.Log(text);
        }
        // Recur for all the vertices
        // adjacent to current vertex
        foreach (HexTile tile in u.allNeighbourRoads) {
            if (!isVisited[tile]) {
                // store current node 
                // in path[]
                localPathList.Add(tile);
                GetAllPathsUtil(tile, d, isVisited, localPathList, allPaths);

                // remove current node
                // in path[]
                localPathList.Remove(tile);
            }
        }

        // Mark the current node
        isVisited[u] = false;
    }

    // A recursive function to print
    // all paths from 'u' to 'd'.
    // isVisited[] keeps track of
    // vertices in current path.
    // localPathList<> stores actual
    // vertices in the current path
    private void GetAllLandmarkPathsUtil(BaseLandmark u, BaseLandmark d, Dictionary<BaseLandmark, bool> isVisited,
                            List<BaseLandmark> localPathList, List<List<BaseLandmark>> allPaths, List<Region> includedRegions) {

        // Mark the current node
        isVisited[u] = true;

        if (u.id == d.id) {
            //string text = string.Empty;
            allPaths.Add(new List<BaseLandmark>(localPathList));
            //localPathList.ForEach(o => text += o.landmarkName + " -> ");
            //Debug.Log(text);
        }
        // Recur for all the vertices
        // adjacent to current vertex
        foreach (BaseLandmark landmark in u.connections.Where(x => includedRegions.Contains(x.tileLocation.region))) {
            if (!isVisited[landmark]) {
                // store current node 
                // in path[]
                localPathList.Add(landmark);
                GetAllLandmarkPathsUtil(landmark, d, isVisited, localPathList, allPaths, includedRegions);
                // remove current node
                // in path[]
                localPathList.Remove(landmark);
            }
        }

        // Mark the current node
        isVisited[u] = false;
    }


    public int GetTravelTime(HexTile from, HexTile to) {
        float distance = Vector3.Distance(from.transform.position, to.transform.position);
        return (Mathf.CeilToInt(distance / 2.315188f)) * 2;
    }
}
