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
    [SerializeField] private HexTile startTile;
    [SerializeField] private HexTile targetTile;
    [SerializeField] private Vector2Int startLocGrid;
    [SerializeField] private Vector2Int destLocGrid;
    public PATHFINDING_MODE modeToUse;
    public GRID_PATHFINDING_MODE gridModeToUse;

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
    [ContextMenu("Get Loc GridPath")]
    public void GetLocGridPathForTesting() {
        List<LocationGridTile> path = GetPath(InteriorMapManager.Instance.currentlyShowingMap.map[startLocGrid.x, startLocGrid.y], InteriorMapManager.Instance.currentlyShowingMap.map[destLocGrid.x, destLocGrid.y], gridModeToUse);
        if (path != null) {
            Debug.Log("========== Path from " + startTile.name + " to " + targetTile.name + "============");
            for (int i = 0; i < path.Count; i++) {
                Debug.Log(path[i].ToString());
            }
        } else {
            Debug.LogError("Cannot get path from " + startTile.name + " to " + targetTile.name + " using " + modeToUse.ToString());
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

        Func<HexTile, HexTile, double> distance = (node1, node2) => 1;
		Func<HexTile, double> estimate = t => Math.Sqrt (Math.Pow (t.xCoordinate - destinationTile.tileLocation.xCoordinate, 2) + Math.Pow (t.yCoordinate - destinationTile.tileLocation.yCoordinate, 2));

		var path = PathFind.PathFind.FindPath (startingTile.tileLocation, destinationTile.tileLocation, distance, estimate, pathfindingMode, data);

        if (path != null) {
			if (pathfindingMode == PATHFINDING_MODE.UNRESTRICTED) {
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
    public List<LocationGridTile> GetPath(LocationGridTile startingTile, LocationGridTile destinationTile, GRID_PATHFINDING_MODE pathMode = GRID_PATHFINDING_MODE.NORMAL, bool includeFirstTile = false) {
        //LocationGridTile.Tile_Type startType = startingTile.tileType;
        //LocationGridTile.Tile_Type destinationType = destinationTile.tileType;
        //LocationGridTile.Tile_Access startAccess = startingTile.tileAccess;
        //LocationGridTile.Tile_Access destinationAccess = destinationTile.tileAccess;
        //switch (pathMode) {
        //    case GRID_PATHFINDING_MODE.NORMAL:
        //        startingTile.SetTileType(LocationGridTile.Tile_Type.Empty);
        //        destinationTile.SetTileType(LocationGridTile.Tile_Type.Empty);
        //        break;
        //    default:
        //        startingTile.SetTileType(LocationGridTile.Tile_Type.Empty);
        //        destinationTile.SetTileType(LocationGridTile.Tile_Type.Empty);
        //        break;
        //}


        List<LocationGridTile> path = null;

        
        if (startingTile.structure != destinationTile.structure && (startingTile.structure.entranceTile != null || destinationTile.structure.entranceTile != null)) {
            //pathfinding logic to use structure entrances

            Func<LocationGridTile, LocationGridTile, double> dist = (node1, node2) => 1;

            if (startingTile.structure.entranceTile != null && destinationTile.structure.entranceTile != null) {
                //both structures have entrances
                Func<LocationGridTile, double> est = t => Math.Sqrt(Math.Pow(t.localPlace.x - startingTile.structure.entranceTile.localPlace.x, 2) + Math.Pow(t.localPlace.y - startingTile.structure.entranceTile.localPlace.y, 2));
                var prePath = PathFind.PathFind.FindPath(startingTile, startingTile.structure.entranceTile, dist, est, pathMode).Reverse().ToList();

                est = t => Math.Sqrt(Math.Pow(t.localPlace.x - destinationTile.structure.entranceTile.localPlace.x, 2) + Math.Pow(t.localPlace.y - destinationTile.structure.entranceTile.localPlace.y, 2));
                var midPath = PathFind.PathFind.FindPath(startingTile.structure.entranceTile, destinationTile.structure.entranceTile, dist, est, pathMode, AllowedStructureTiles,
                new List<STRUCTURE_TYPE>() { STRUCTURE_TYPE.WORK_AREA, STRUCTURE_TYPE.WILDERNESS }, startingTile.structure.entranceTile, destinationTile.structure.entranceTile).Reverse().ToList();

                est = t => Math.Sqrt(Math.Pow(t.localPlace.x - destinationTile.localPlace.x, 2) + Math.Pow(t.localPlace.y - destinationTile.localPlace.y, 2));
                var afterPath = PathFind.PathFind.FindPath(destinationTile.structure.entranceTile, destinationTile, dist, est, pathMode).Reverse().ToList();

                path = prePath.Union(midPath).Union(afterPath).Reverse().ToList();
            } else if (startingTile.structure.entranceTile != null && destinationTile.structure.entranceTile == null) {
                //only the starting tile has an entrance
                Func<LocationGridTile, double> est = t => Math.Sqrt(Math.Pow(t.localPlace.x - startingTile.structure.entranceTile.localPlace.x, 2) + Math.Pow(t.localPlace.y - startingTile.structure.entranceTile.localPlace.y, 2));
                var prePath = PathFind.PathFind.FindPath(startingTile, startingTile.structure.entranceTile, dist, est, pathMode).Reverse().ToList();

                est = t => Math.Sqrt(Math.Pow(t.localPlace.x - destinationTile.localPlace.x, 2) + Math.Pow(t.localPlace.y - destinationTile.localPlace.y, 2));
                var midPath = PathFind.PathFind.FindPath(startingTile.structure.entranceTile, destinationTile, dist, est, pathMode, AllowedStructureTiles,
                new List<STRUCTURE_TYPE>() { STRUCTURE_TYPE.WORK_AREA, STRUCTURE_TYPE.WILDERNESS }, startingTile.structure.entranceTile, destinationTile).Reverse().ToList();

                path = prePath.Union(midPath).Reverse().ToList();
            } else {
                //only the destination tile has an entrance
                Func<LocationGridTile, double> est = t => Math.Sqrt(Math.Pow(t.localPlace.x - destinationTile.structure.entranceTile.localPlace.x, 2) + Math.Pow(t.localPlace.y - destinationTile.structure.entranceTile.localPlace.y, 2));
                var prePath = PathFind.PathFind.FindPath(startingTile, destinationTile.structure.entranceTile, dist, est, pathMode, AllowedStructureTiles,
                new List<STRUCTURE_TYPE>() { STRUCTURE_TYPE.WORK_AREA, STRUCTURE_TYPE.WILDERNESS }, startingTile, destinationTile.structure.entranceTile).Reverse().ToList();

                est = t => Math.Sqrt(Math.Pow(t.localPlace.x - destinationTile.localPlace.x, 2) + Math.Pow(t.localPlace.y - destinationTile.localPlace.y, 2));
                var midPath = PathFind.PathFind.FindPath(destinationTile.structure.entranceTile, destinationTile, dist, est, pathMode).Reverse().ToList();

                path = prePath.Union(midPath).Reverse().ToList();
            }
        } else {
            //normal pathfinding logic
            Func<LocationGridTile, LocationGridTile, double> distance = (node1, node2) => 1;
            Func<LocationGridTile, double> estimate = t => Math.Sqrt(Math.Pow(t.localPlace.x - destinationTile.localPlace.x, 2) + Math.Pow(t.localPlace.y - destinationTile.localPlace.y, 2));
            var p = PathFind.PathFind.FindPath(startingTile, destinationTile, distance, estimate, pathMode);
            if (p != null) {
                path = p.ToList();
            }
            
        }

        //startingTile.SetTileType(startType);
        //destinationTile.SetTileType(destinationType);
        //startingTile.SetTileAccess(startAccess);
        //destinationTile.SetTileAccess(destinationAccess);

        if (path != null) {
            path.Reverse();
            if (!includeFirstTile) {
                path.RemoveAt(0);
            }
            return path;
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

    public int GetTravelTime(HexTile from, HexTile to) {
        float distance = Vector3.Distance(from.transform.position, to.transform.position);
        return (Mathf.CeilToInt(distance / 2.315188f)) * 2;
    }

    #region Tile Getters
    private List<LocationGridTile> SameStructureTiles(LocationGridTile tile, params object[] args) {
        LocationStructure structure = args[0] as LocationStructure;
        LocationGridTile startingTile = args[1] as LocationGridTile;
        LocationGridTile destinationTile = args[2] as LocationGridTile;

        List<LocationGridTile> tiles = new List<LocationGridTile>();
        List<LocationGridTile> neighbours = tile.FourNeighbours();
        for (int i = 0; i < neighbours.Count; i++) {
            LocationGridTile currTile = neighbours[i];
            //if (currTile == startingTile || currTile == destinationTile || (currTile.tileAccess == LocationGridTile.Tile_Access.Passable && currTile.structure == structure)) {
            //    tiles.Add(currTile);
            //}
        }
        return tiles;
    }
    private List<LocationGridTile> AllowedStructureTiles(LocationGridTile tile, params object[] args) {
        List<STRUCTURE_TYPE> allowedTypes = args[0] as List<STRUCTURE_TYPE>;
        LocationGridTile startingTile = args[1] as LocationGridTile;
        LocationGridTile destinationTile = args[2] as LocationGridTile;

        List<LocationGridTile> tiles = new List<LocationGridTile>();
        List<LocationGridTile> neighbours = tile.FourNeighbours();
        for (int i = 0; i < neighbours.Count; i++) {
            LocationGridTile currTile = neighbours[i];
            //if (currTile == startingTile || currTile == destinationTile || (currTile.tileAccess == LocationGridTile.Tile_Access.Passable && currTile.structure != null && allowedTypes.Contains(currTile.structure.structureType))) {
            //    tiles.Add(currTile);
            //}
        }
        return tiles;
    }
    #endregion
}
