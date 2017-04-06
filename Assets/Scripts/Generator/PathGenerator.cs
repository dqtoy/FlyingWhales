using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class PathGenerator : MonoBehaviour {

	public static PathGenerator Instance = null;

	private List<HexTile> roadTiles = new List<HexTile>();

	void Awake(){
		Instance = this;
	}

	#region Road Generation
	public void GenerateConnections(List<HexTile> habitableTiles){
		List<HexTile> pendingTiles = new List<HexTile>();
		pendingTiles.Add(habitableTiles[0]);
		for (int i = 0; i < pendingTiles.Count; i++) {
			HexTile currentHexTile = pendingTiles [i];

			List<HexTile> habitableTilesOrderedByDistance = this.GetAllHabitableTilesByDistance(currentHexTile);
			int randomNumberOfRoads = this.GenerateNumberOfRoads();
			int createdRoads = habitableTiles[i].connectedTiles.Count;

			for (int j = 0; createdRoads < randomNumberOfRoads; j++) {
				HexTile tileToConnectTo = habitableTilesOrderedByDistance[j];

				if ((j + 1) == habitableTilesOrderedByDistance.Count) {
					//if j has reached habitableTilesOrderedByDistance's upper bound, connect to nearest city
					if (AreTheseTilesConnected(currentHexTile, habitableTilesOrderedByDistance[0], PATHFINDING_MODE.ROAD_CREATION)) {
						createdRoads++;
						if (!currentHexTile.connectedTiles.Contains(habitableTilesOrderedByDistance[0])) {
							ConnectCities (currentHexTile, habitableTilesOrderedByDistance[0]);
							if (!pendingTiles.Contains(habitableTilesOrderedByDistance[0])) {
								pendingTiles.Add(habitableTilesOrderedByDistance[0]);
							}
						}
					}
					break;
				} else {
					if (tileToConnectTo.connectedTiles.Count < 3 && !currentHexTile.connectedTiles.Contains(tileToConnectTo)) {
						if (AreTheseTilesConnected (currentHexTile, tileToConnectTo, PATHFINDING_MODE.ROAD_CREATION)) {
							createdRoads++;
							ConnectCities (currentHexTile, tileToConnectTo);
							if (!pendingTiles.Contains (tileToConnectTo)) {
								pendingTiles.Add (tileToConnectTo);
							}
						}
					}
				}
			}

			if (pendingTiles.Count != habitableTiles.Count) {
				Debug.Log ("MISSED SOME CITIES!");
				Debug.Log ("Create Lines for missed out cities");
				for (int x = 0; x < habitableTiles.Count; x++) {
					
					if (!pendingTiles.Contains (habitableTiles[x])) {
						Debug.Log ("======Missed out tile: " + habitableTiles [x].name + " ======");
						HexTile missedOutTile = habitableTiles[x];
						HexTile possibleConnectionTile = FindNearestCityWithConnection (missedOutTile);

						List<HexTile> habitableTilesOrdered = GetAllHabitableTilesByDistance(missedOutTile);

						if (possibleConnectionTile == null || !AreTheseTilesConnected (missedOutTile, possibleConnectionTile, PATHFINDING_MODE.ROAD_CREATION)) {
							for (int j = 0; j < habitableTilesOrdered.Count; j++) {
								if (AreTheseTilesConnected (missedOutTile, habitableTilesOrdered [j], PATHFINDING_MODE.ROAD_CREATION)) {
									possibleConnectionTile = habitableTilesOrdered [j];
									break;
								}
							}
						}

						if (possibleConnectionTile != null && !missedOutTile.connectedTiles.Contains(possibleConnectionTile)) {
							if (AreTheseTilesConnected (missedOutTile, possibleConnectionTile, PATHFINDING_MODE.ROAD_CREATION)) {
								ConnectCities (missedOutTile, possibleConnectionTile);
								if (!pendingTiles.Contains (missedOutTile)) {
									pendingTiles.Add (missedOutTile);
								}

								if (!pendingTiles.Contains (possibleConnectionTile)) {
									pendingTiles.Add (possibleConnectionTile);
								}
							}
						}
					}
				}
			}

		}
	}

	void ConnectCities(HexTile originTile, HexTile targetTile){
		Debug.Log (originTile.name + " is now connected to: " + targetTile.name);
		this.DeterminePath (originTile, targetTile);
		originTile.connectedTiles.Add(targetTile);
		targetTile.connectedTiles.Add(originTile);
	}


	/*
	 * Generate Path From one tile to another
	 * */
	public void DeterminePath(HexTile start, HexTile destination){
//		List<HexTile> roadListByDistance = SortAllRoadsByDistance (start, destination); //Sort all road tiles in regards to how far they are from the start
//		for (int i = 0; i < roadListByDistance.Count; i++) {
//			if (AreTheseTilesConnected (roadListByDistance [i], destination, PATHFINDING_MODE.NORMAL)) {
//				Debug.Log ("Connect to roadTile: " + roadListByDistance [i].name + " instead");
//				if (roadListByDistance[i].isHabitable && roadListByDistance[i].connectedTiles.Contains(start)) {
//					return; //use the already created road between the 2 cities.
//				}
//				SetTilesAsRoads(GetPath(start, roadListByDistance[i], PATHFINDING_MODE.ROAD_CREATION));
//				return;
//			}
//		}
		SetTilesAsRoads(GetPath(start, destination, PATHFINDING_MODE.ROAD_CREATION));
	}

	/*
	 * Get List of tiles (Path) that will connect 2 city tiles
	 * */
	public IEnumerable<HexTile> GetPath(HexTile startingTile, HexTile destinationTile, PATHFINDING_MODE pathfindingMode){
		Func<HexTile, HexTile, double> distance = (node1, node2) => 1;
		Func<HexTile, double> estimate = t => Math.Sqrt(Math.Pow(t.xCoordinate - destinationTile.xCoordinate, 2) + Math.Pow(t.yCoordinate - destinationTile.yCoordinate, 2));
		if (pathfindingMode != PATHFINDING_MODE.ROAD_CREATION) {
			startingTile.isRoad = true;
			destinationTile.isRoad = true;
		}
		var path = PathFind.PathFind.FindPath(startingTile, destinationTile, distance, estimate, pathfindingMode);
		if (pathfindingMode != PATHFINDING_MODE.ROAD_CREATION) {
			startingTile.isRoad = false;
			destinationTile.isRoad = false;
		}
		return path;
	}

	public bool AreTheseTilesConnected(HexTile tile1, HexTile tile2, PATHFINDING_MODE pathfindingMode){
		Func<HexTile, HexTile, double> distance = (node1, node2) => 1;
		Func<HexTile, double> estimate = t => Math.Sqrt(Math.Pow(t.xCoordinate - tile2.xCoordinate, 2) + Math.Pow(t.yCoordinate - tile2.yCoordinate, 2));

		if (pathfindingMode == PATHFINDING_MODE.NORMAL) {
			if (tile1.isHabitable) {
				tile1.isRoad = true;
			}
			if (tile2.isHabitable) {
				tile2.isRoad = true;
			}
		}

		var path = GetPath (tile1, tile2, pathfindingMode);

		if (pathfindingMode == PATHFINDING_MODE.NORMAL) {
			if (tile1.isHabitable) {
				tile1.isRoad = false;
			}
			if (tile2.isHabitable) {
				tile2.isRoad = false;
			}
		}

		return path != null;
	}

	List<HexTile> SortAllRoadsByDistance(HexTile startTile, HexTile destinationTile){
		List<HexTile> tempRoadTiles = roadTiles;
		/*
		 * Code if you don't want to use cities included in some paths
		 * * */
		for (int i = 0; i < tempRoadTiles.Count; i++) {
			if (tempRoadTiles [i].city != null) {
				tempRoadTiles.Remove (tempRoadTiles [i]);
			}
		}
		tempRoadTiles.Add(destinationTile);

		List<HexTile> allRoadTiles = tempRoadTiles.OrderBy(
			x => Vector2.Distance(startTile.transform.position, x.transform.position) 
		).ToList();

		return allRoadTiles;
	}

	private void SetTilesAsRoads(IEnumerable<HexTile> path) {
		List<HexTile> pathList = path.ToList();
		for (int i = 0; i < pathList.Count; i++) {
			if (!pathList[i].isHabitable || pathList[i].city == null) {
				pathList[i].isRoad = true;
				roadTiles.Add(pathList[i]);
			}
		}
	}
	#endregion

	private int GenerateNumberOfRoads(){
		int linesRandomizer = UnityEngine.Random.Range (0, 101);
		if (linesRandomizer >= 0 && linesRandomizer < 10) {
			return 1;
		} else if (linesRandomizer >= 10 && linesRandomizer < 70) {
			return 2;
		} else {
			return 3;
		}
	}

	public List<HexTile> GetAllHabitableTilesByDistance(HexTile hexTile){
		List<HexTile> allHabitableTiles = CityGenerator.Instance.habitableTiles.OrderBy(x => Vector2.Distance(hexTile.transform.position, x.transform.position)).ToList();
		allHabitableTiles.Remove(hexTile);
		return allHabitableTiles;
	}

	public HexTile FindNearestCityWithConnection(HexTile hexTile){
		List<HexTile> habitableTilesOrderedByDistance = this.GetAllHabitableTilesByDistance(hexTile);
		for (int i = 0; i < habitableTilesOrderedByDistance.Count; i++) {
			if (habitableTilesOrderedByDistance[i].connectedTiles.Count > 0) {
				return habitableTilesOrderedByDistance[i];
			}
		}
		if (habitableTilesOrderedByDistance.Count > 0) {
			return habitableTilesOrderedByDistance[0];
		}
		return null;
	}
}
