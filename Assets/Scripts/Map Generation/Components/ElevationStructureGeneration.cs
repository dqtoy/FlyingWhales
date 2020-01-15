using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class ElevationStructureGeneration : MapGenerationComponent {
	public override IEnumerator Execute(MapGenerationData data) {
		for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
			Region region = GridMap.Instance.allRegions[i];
			List<ElevationIsland> islandsInRegion = GetElevationIslandsInRegion(region);
			Debug.Log($"Islands in region {region.name} is {islandsInRegion.Count.ToString()}");
			LocationStructure wilderness = region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
			for (int j = 0; j < islandsInRegion.Count; j++) {
				ElevationIsland currIsland = islandsInRegion[j];
				STRUCTURE_TYPE structureType = GetStructureTypeFor(currIsland.elevation);
				LocationStructure structure = LandmarkManager.Instance.CreateNewStructureAt(region, structureType);
				yield return MapGenerator.Instance.StartCoroutine(GenerateElevationPerlin(structure, wilderness, currIsland, region));
				region.innerMap.structureTilemapCollider.enabled = false;
				yield return new WaitForSeconds(0.5f);
				region.innerMap.structureTilemapCollider.enabled = true;
			}
		}

		// for (int i = 0; i < GridMap.Instance.normalHexTiles.Count; i++) {
		// 	HexTile tile = GridMap.Instance.normalHexTiles[i];
		// 	if (tile.landmarkOnTile == null && tile.elevationType != ELEVATION.WATER && tile.elevationType != ELEVATION.PLAIN && tile.elevationType != ELEVATION.TREES) {
		// 		yield return MapGenerator.Instance.StartCoroutine(CreateStructureObjectForTile(tile));
		// 	}
		// }
		yield return null;
	}
	private IEnumerator CreateStructureObjectForTile(HexTile tile) {
		LocationStructure structure = LandmarkManager.Instance.CreateNewStructureAt(tile.region,
			GetStructureTypeFor(tile.elevationType));
		 yield return MapGenerator.Instance.StartCoroutine(PlaceInitialStructure(structure, tile.region.innerMap, tile));
	}
	private STRUCTURE_TYPE GetStructureTypeFor(ELEVATION elevation) {
		switch (elevation) {
			case ELEVATION.MOUNTAIN:
				return STRUCTURE_TYPE.CAVE;
			case ELEVATION.WATER:
				return STRUCTURE_TYPE.OCEAN;
		}
		throw new Exception($"There is no corresponding structure type for {elevation.ToString()}");
	}
	private IEnumerator PlaceInitialStructure(LocationStructure structure, InnerTileMap innerTileMap, HexTile tile) {
		if (structure.structureType.ShouldBeGeneratedFromTemplate()) {
			List<GameObject> choices =
				InnerMapManager.Instance.GetStructurePrefabsForStructure(structure.structureType);
			GameObject chosenStructurePrefab = Utilities.GetRandomElement(choices);
			LocationStructureObject lso = chosenStructurePrefab.GetComponent<LocationStructureObject>();
			BuildingSpot chosenBuildingSpot;
			if (TryGetBuildSpotForStructureInTile(lso, tile, innerTileMap, out chosenBuildingSpot)) {
				innerTileMap.PlaceStructureObjectAt(chosenBuildingSpot, chosenStructurePrefab, structure);
			} else {
				throw new System.Exception(
					$"Could not find valid building spot for {structure.ToString()} using prefab {chosenStructurePrefab.name}");
			}
			yield return null;
		}

	}
	private bool TryGetBuildSpotForStructureInTile(LocationStructureObject structureObject, HexTile currTile, 
		InnerTileMap innerTileMap, out BuildingSpot spot) {
		for (int j = 0; j < currTile.ownedBuildSpots.Length; j++) {
			BuildingSpot currSpot = currTile.ownedBuildSpots[j];
			if (currSpot.isOccupied == false && currSpot.CanFitStructureOnSpot(structureObject, innerTileMap)) {
				spot = currSpot;
				return true;
			}
		}
		spot = null;
		return false;
	}
	
	private List<ElevationIsland> GetElevationIslandsInRegion(Region region) {
		List<ElevationIsland> islands = new List<ElevationIsland>();
		ELEVATION[] elevationsToCheck = new[] {ELEVATION.WATER, ELEVATION.MOUNTAIN};
		for (int i = 0; i < elevationsToCheck.Length; i++) {
			ELEVATION elevation = elevationsToCheck[i];
			List<HexTile> tilesOfThatElevation = GetTilesWithElevationInRegion(region, elevation);
			List<ElevationIsland> initialIslands = CreateInitialIslands(tilesOfThatElevation, elevation);
			List<ElevationIsland> mergedIslands = MergeIslands(initialIslands);
			islands.AddRange(mergedIslands);
		}
		return islands;
	}
	private List<HexTile> GetTilesWithElevationInRegion(Region region, ELEVATION elevation) {
		List<HexTile> tiles = new List<HexTile>();
		for (int i = 0; i < region.tiles.Count; i++) {
			HexTile tile = region.tiles[i];
			if (tile.elevationType == elevation) {
				tiles.Add(tile);
			}
		}
		return tiles;
	}
	private List<ElevationIsland> CreateInitialIslands(List<HexTile> tiles, ELEVATION elevation) {
		List<ElevationIsland> islands = new List<ElevationIsland>();
		for (int i = 0; i < tiles.Count; i++) {
			HexTile tile = tiles[i];
			ElevationIsland island = new ElevationIsland(elevation);
			island.AddTile(tile);
			islands.Add(island);
		}
		return islands;
	}
	private List<ElevationIsland> MergeIslands(List<ElevationIsland> islands) {
		List<ElevationIsland> mergedIslands = new List<ElevationIsland>(islands);
		for (int i = 0; i < islands.Count; i++) {
			ElevationIsland currIsland = islands[i];
			for (int j = 0; j < islands.Count; j++) {
				ElevationIsland otherIsland = islands[j];
				if (currIsland != otherIsland) {
					if (currIsland.IsAdjacentToIsland(otherIsland)) {
						currIsland.MergeWithIsland(otherIsland, ref mergedIslands);
					}
				}
			}
		}
		return mergedIslands;
	}
	private IEnumerator GenerateElevationPerlin(LocationStructure structure, LocationStructure wilderness, ElevationIsland island, Region region) {
		List<LocationGridTile> allTiles = new List<LocationGridTile>();
		for (int i = 0; i < island.tilesInIsland.Count; i++) {
			HexTile tileInIsland = island.tilesInIsland[i];
			allTiles.AddRange(tileInIsland.locationGridTiles);
		}
		
		List<LocationGridTile> outerTiles = new List<LocationGridTile>();
		for (int i = 0; i < allTiles.Count; i++) {
			LocationGridTile gridTile = allTiles[i];
			if (gridTile.HasNeighbourNotInList(allTiles)) {
				outerTiles.Add(gridTile);
				List<LocationGridTile> neighbours = gridTile.FourNeighbours();
				for (int j = 0; j < neighbours.Count; j++) {
					LocationGridTile neighbour = neighbours[j];
					if (allTiles.Contains(neighbour) && outerTiles.Contains(neighbour) == false) {
						outerTiles.Add(neighbour);
					}
				}
			}
		}

		float offsetX = Random.Range(500f, 2000f);
		float offsetY = Random.Range(500f, 2000f);
		
		int minX = allTiles.Min(t => t.localPlace.x);
		int maxX = allTiles.Max(t => t.localPlace.x);
		int minY = allTiles.Min(t => t.localPlace.y);
		int maxY = allTiles.Max(t => t.localPlace.y);

		int xSize = maxX - minX;
		int ySize = maxY - minY;

		int outerRange = 4;

		outerTiles = outerTiles.OrderBy(x => x.localPlace.x).ThenBy(x => x.localPlace.y).ToList();

		for (int i = 0; i < allTiles.Count; i++) {
			LocationGridTile gridTile = allTiles[i];
			gridTile.SetGroundTilemapVisual(GetGroundTileAssetForElevation(island.elevation, region));
			gridTile.parentMap.structureTilemap.SetTile(gridTile.localPlace, GetWallTileAssetForElevation(island.elevation));
			gridTile.SetStructure(structure);
		}
		
		for (int i = 0; i < outerTiles.Count; i++) {
			LocationGridTile gridTile = outerTiles[i];
			float xCoord = ((float)gridTile.localPlace.x / xSize) * offsetX;
			float yCoord = ((float)gridTile.localPlace.y / ySize) * offsetY;
			float sample = Mathf.PerlinNoise(xCoord, yCoord);
			float chance = 0.5f;
			
			if (sample < chance) {
				//set as elevation
				gridTile.SetGroundTilemapVisual(GetGroundTileAssetForElevation(island.elevation, region));
				gridTile.parentMap.structureTilemap.SetTile(gridTile.localPlace, GetWallTileAssetForElevation(island.elevation));
			} else {
				//set as ground
				gridTile.SetGroundTilemapVisual(GetGroundTileAssetForElevation(ELEVATION.PLAIN, region));
				gridTile.parentMap.structureTilemap.SetTile(gridTile.localPlace, GetWallTileAssetForElevation(ELEVATION.PLAIN));
				gridTile.SetStructure(wilderness);
			}
		}

		for (int i = 0; i < outerTiles.Count; i++) {
			LocationGridTile gridTile = outerTiles[i];
			LocationGridTile.Ground_Type groundTypeForElevation = GetGroundTypeForElevation(island.elevation);
			if (gridTile.groundType == groundTypeForElevation && gridTile.GetNeighbourOfTypeCount(groundTypeForElevation, true) < 2) {
				// Debug.Log($"{gridTile.ToString()} at {region.name} does not have neighbours of elevation {island.elevation.ToString()}. Setting it as ground.");
				//if current outer tile does not have neighbour of the same elevation type, then set it as ground
				gridTile.SetGroundTilemapVisual(GetGroundTileAssetForElevation(ELEVATION.PLAIN, region));
				gridTile.parentMap.structureTilemap.SetTile(gridTile.localPlace, GetWallTileAssetForElevation(ELEVATION.PLAIN));
				gridTile.SetStructure(wilderness);
			}
		}

		for (int i = 0; i < island.tilesInIsland.Count; i++) {
			HexTile hexTile = island.tilesInIsland[i];
			for (int j = 0; j < hexTile.ownedBuildSpots.Length; j++) {
				BuildingSpot spot = hexTile.ownedBuildSpots[j];
				if (spot.isOccupied == false) {
					spot.SetIsOccupied(true);
					spot.UpdateAdjacentSpotsOccupancy(hexTile.region.innerMap);	
				}
			}
		}
		
		yield return null;
	}

	private TileBase GetWallTileAssetForElevation(ELEVATION elevation) {
		switch (elevation) {
			case ELEVATION.WATER:
				return InnerMapManager.Instance.assetManager.shoreTle;
			case ELEVATION.MOUNTAIN:
				return InnerMapManager.Instance.assetManager.caveWallTile;
		}
		return null;
	}
	private TileBase GetGroundTileAssetForElevation(ELEVATION elevation, Region region) {
		switch (elevation) {
			case ELEVATION.WATER:
				return InnerMapManager.Instance.assetManager.waterTle;
			case ELEVATION.MOUNTAIN:
				return InnerMapManager.Instance.assetManager.caveWallTile;
			case ELEVATION.PLAIN:
				return InnerMapManager.Instance.assetManager.GetOutsideFloorTile(region);
		}
		return null;
	}
	private LocationGridTile.Ground_Type GetGroundTypeForElevation(ELEVATION elevation) {
		switch (elevation) {
			case ELEVATION.WATER:
				return LocationGridTile.Ground_Type.Water;
			case ELEVATION.MOUNTAIN:
				return LocationGridTile.Ground_Type.Cave;
		}
		return LocationGridTile.Ground_Type.Soil;
	}
}

public class ElevationIsland {
	public readonly ELEVATION elevation;
	public readonly List<HexTile> tilesInIsland;

	public ElevationIsland(ELEVATION elevation) {
		this.elevation = elevation;
		tilesInIsland = new List<HexTile>();
	}

	public void AddTile(HexTile tile) {
		tilesInIsland.Add(tile);
	}
	private void RemoveTile(HexTile tile) {
		tilesInIsland.Remove(tile);
	}

	public void MergeWithIsland(ElevationIsland otherIsland, ref List<ElevationIsland> allIslands) {
		for (int i = 0; i < otherIsland.tilesInIsland.Count; i++) {
			HexTile tileInOtherIsland = otherIsland.tilesInIsland[i];
			AddTile(tileInOtherIsland);
			otherIsland.RemoveTile(tileInOtherIsland);
		}
		allIslands.Remove(otherIsland);
	}

	public bool IsAdjacentToIsland(ElevationIsland otherIsland) {
		for (int i = 0; i < tilesInIsland.Count; i++) {
			HexTile tile = tilesInIsland[i];
			for (int j = 0; j < tile.AllNeighbours.Count; j++) {
				HexTile neighbour = tile.AllNeighbours[j];
				if (otherIsland.tilesInIsland.Contains(neighbour)) {
					//this island has a tile that has a neighbour that is part of the given island.
					return true;
				}
			}
		}
		return false;
	}
}
