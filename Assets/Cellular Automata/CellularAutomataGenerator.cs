using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Tilemaps;
namespace Cellular_Automata {
	public static class CellularAutomataGenerator {

		public static int[,] GenerateMap(int width, int height, int smoothing, int randomFillPercent, 
			LocationGridTile[,] tileMap, List<LocationGridTile> allTiles, string seed = "", 
			bool edgesAreAlwaysWalls = true) {
			int[,] map = new int[width, height];
			RandomFillMap(width, height, map, randomFillPercent, tileMap, allTiles, seed, edgesAreAlwaysWalls);
			
			for (int i = 0; i < smoothing; i++) {
				SmoothMap(map, width, height, tileMap, allTiles, edgesAreAlwaysWalls);	
			}
			return map;
		}
		private static void RandomFillMap(int width, int height, int[,] map, int randomFillPercent, 
			LocationGridTile[,] tileMap, List<LocationGridTile> allTiles, string seed, 
			bool edgesAreAlwaysWalls = true) {
			if (string.IsNullOrEmpty(seed)) {
				seed = Time.time.ToString();
			}
			
			System.Random pseudoRandom = new System.Random(seed.GetHashCode());
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					LocationGridTile tile = tileMap[x, y];
					if (edgesAreAlwaysWalls && (tile == null || IsAtEdgeOfMap(tile, allTiles))) {
						//wall
						map[x, y] = 1;
					} else {
						//random
						map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;	
					}
				}	
			}
		}
		private static void SmoothMap(int[,] map, int width, int height, LocationGridTile[,] tileMap, 
			List<LocationGridTile> allTiles, bool edgesAreAlwaysWalls = true) {
			int[,] tempMap = map;
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					LocationGridTile tile = tileMap[x, y];
					if (edgesAreAlwaysWalls && tile != null && IsAtEdgeOfMap(tile, allTiles)) {
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
		}
		
		public static LocationGridTile[,] ConvertListToGridMap(List<LocationGridTile> locationGridTiles) {
			int minX = locationGridTiles.Min(t => t.localPlace.x);
			int maxX = locationGridTiles.Max(t => t.localPlace.x);
			int minY = locationGridTiles.Min(t => t.localPlace.y);
			int maxY = locationGridTiles.Max(t => t.localPlace.y);

			int xSize = (maxX - minX) + 1;
			int ySize = (maxY - minY) + 1;
			
			LocationGridTile[,] arrangedMap = new LocationGridTile[xSize, ySize];
			for (int i = 0; i < locationGridTiles.Count; i++) {
				LocationGridTile tile = locationGridTiles[i];
				int localX = tile.localPlace.x - minX;
				int localY = tile.localPlace.y - minY;
				arrangedMap[localX, localY] = tile;
			}

			return arrangedMap;
		}

		public static void DrawMap(LocationGridTile[,] tileMap, int[,] cellAutomata, TileBase wallAsset,
			TileBase nonWallAsset) {
			
		}
	}	
}
