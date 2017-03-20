using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PathGenerator : MonoBehaviour {

	public static PathGenerator Instance = null;

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
					//if j has reached listOrderedByDistance's upper bound, connect to nearest city
					createdRoads++;
					if (!currentHexTile.connectedTiles.Contains(habitableTilesOrderedByDistance[0])) {
						ConnectCities (currentHexTile, habitableTilesOrderedByDistance[0]);
						if (!pendingTiles.Contains(habitableTilesOrderedByDistance[0])) {
							pendingTiles.Add(habitableTilesOrderedByDistance[0]);
						}
					}
					break;
				} else {
					if (tileToConnectTo.connectedTiles.Count < 3 && !currentHexTile.connectedTiles.Contains(tileToConnectTo)) {
						createdRoads++;
						ConnectCities (currentHexTile, tileToConnectTo);
						if (!pendingTiles.Contains(tileToConnectTo)) {
							pendingTiles.Add(tileToConnectTo);
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

						if (possibleConnectionTile != null && !missedOutTile.connectedTiles.Contains(possibleConnectionTile)) {
							ConnectCities (missedOutTile, possibleConnectionTile);
							if (!pendingTiles.Contains (missedOutTile)) {
								pendingTiles.Add (missedOutTile);
							}

							if (!pendingTiles.Contains(possibleConnectionTile)) {
								pendingTiles.Add(possibleConnectionTile);
							}
						}
					}
				}
			}

		}
	}

	void ConnectCities(HexTile originTile, HexTile targetTile){
		Debug.Log (originTile.name + " is now connected to: " + targetTile.name);
//		PathManager.Instance.DeterminePath (originTile.tile, targetTile.tile);
		originTile.connectedTiles.Add(targetTile);
		targetTile.connectedTiles.Add(originTile);
	}

	#endregion

	private int GenerateNumberOfRoads(){
		int linesRandomizer = Random.Range (0, 101);
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
		return habitableTilesOrderedByDistance[0];
	}
}
