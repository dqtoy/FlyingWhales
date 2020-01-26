using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

namespace Cellular_Automata {
	public static class CellularAutomataGenerator {

		public static int[,] GenerateMap(int width, int height, int smoothing, int randomFillPercent, 
			LocationGridTile[,] tileMap, List<LocationGridTile> allTiles, string seed = "") {
			int[,] map = new int[width, height];
			RandomFillMap(width, height, map, randomFillPercent, tileMap, allTiles, seed);
			
			for (int i = 0; i < smoothing; i++) {
				SmoothMap(map, width, height, tileMap, allTiles);	
			}
			return map;
		}
		private static void RandomFillMap(int width, int height, int[,] map, int randomFillPercent, 
			LocationGridTile[,] tileMap, List<LocationGridTile> allTiles, string seed) {
			if (string.IsNullOrEmpty(seed)) {
				seed = Time.time.ToString();
			}
			
			System.Random pseudoRandom = new System.Random(seed.GetHashCode());
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					LocationGridTile tile = tileMap[x, y];
					if (tile == null || IsAtEdgeOfMap(tile, allTiles)) {
						//wall
						map[x, y] = 1;
					} else {
						//random
						map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;	
					}
				}	
			}
		}
		private static void SmoothMap(int[,] map, int width, int height, LocationGridTile[,] tileMap, List<LocationGridTile> allTiles) {
			int[,] tempMap = map;
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					LocationGridTile tile = tileMap[x, y];
					if (tile != null && IsAtEdgeOfMap(tile, allTiles)) {
						tempMap[x, y] = 1;
					} else {
						int neighbourWallTiles = GetSurroundingWallCount(map, x, y, width, height);
						if (neighbourWallTiles > 4) {
							tempMap[x, y] = 1;
						} else if (neighbourWallTiles < 4) {
							tempMap[x, y] = 0;
						}	
					}
					
					
				}	
			}
			map = tempMap;
		}

		private static int GetSurroundingWallCount(int[,] map, int gridX, int gridY, int width, int height) {
			int wallCount = 0;
			for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++) {
				for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++) {
					if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height) {
						if (neighbourX != gridX || neighbourY != gridY) {
							wallCount += map[neighbourX, neighbourY];
						}	
					} else {
						wallCount++;
					}
				}
			}
			return wallCount;
		}
		// private void OnDrawGizmos() {
		// 	if (map != null) {
		// 		for (int x = 0; x < width; x++) {
		// 			for (int y = 0; y < height; y++) {
		// 				Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white;
		// 				Vector3 pos = new Vector3(-width/2 + x + 0.5f, 0, -height/2 + y + 0.5f);
		// 				Gizmos.DrawCube(pos, Vector3.one);
		// 			}	
		// 		}	
		// 	}
		// }

		private static bool IsAtEdgeOfMap(LocationGridTile tile, List<LocationGridTile> allTiles) {
			return tile.HasNeighbourNotInList(allTiles) || tile.IsAtEdgeOfMap();
			// List<LocationGridTile> neighbours = tile.neighbourList;
			// for (int i = 0; i < neighbours.Count; i++) {
			// 	LocationGridTile neighbour = neighbours[i];
			// 	if (allTiles.Contains(neighbour) == false) {
			// 		return true;
			// 	}
			// }
			// return false;
		}
	}	
}
