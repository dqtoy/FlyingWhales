using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cellular_Automata;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class LandmarkStructureGeneration : MapGenerationComponent {
	public override IEnumerator Execute(MapGenerationData data) {
		List<BaseLandmark> landmarks = LandmarkManager.Instance.GetAllLandmarks();
		for (int i = 0; i < landmarks.Count; i++) {
			BaseLandmark landmark = landmarks[i];
			if (landmark.specificLandmarkType == LANDMARK_TYPE.MONSTER_LAIR) {
				LocationStructure structure = LandmarkManager.Instance.CreateNewStructureAt(landmark.tileLocation.region,
					LandmarkManager.Instance.GetStructureTypeFor(landmark.specificLandmarkType));
				landmark.tileLocation.settlementOnTile.GenerateStructures(structure);
				yield return MapGenerator.Instance.StartCoroutine(
					GenerateMonsterLair(landmark.tileLocation, structure));
			} else if (landmark.specificLandmarkType != LANDMARK_TYPE.VILLAGE) {
				yield return MapGenerator.Instance.StartCoroutine(CreateStructureObjectForLandmark(landmark, data));	
			}
		}
		yield return null;
	}

	private IEnumerator CreateStructureObjectForLandmark(BaseLandmark landmark, MapGenerationData data) {
		LocationStructure structure = LandmarkManager.Instance.CreateNewStructureAt(landmark.tileLocation.region,
			LandmarkManager.Instance.GetStructureTypeFor(landmark.specificLandmarkType));
		 yield return MapGenerator.Instance.StartCoroutine(PlaceInitialStructure(structure, landmark.tileLocation.region.innerMap, landmark.tileLocation));
		 if (structure.structureType == STRUCTURE_TYPE.THE_PORTAL) {
			 data.portalStructure = structure;
		 }
	}

	private IEnumerator PlaceInitialStructure(LocationStructure structure, InnerTileMap innerTileMap, HexTile tile) {
		if (structure.structureType.ShouldBeGeneratedFromTemplate()) {
			List<GameObject> choices =
				InnerMapManager.Instance.GetStructurePrefabsForStructure(structure.structureType);
			GameObject chosenStructurePrefab = CollectionUtilities.GetRandomElement(choices);
			LocationStructureObject lso = chosenStructurePrefab.GetComponent<LocationStructureObject>();
			BuildingSpot chosenBuildingSpot;
			if (LandmarkManager.Instance.TryGetBuildSpotForStructureInTile(lso, tile, innerTileMap, out chosenBuildingSpot)) {
				innerTileMap.PlaceStructureObjectAt(chosenBuildingSpot, chosenStructurePrefab, structure);
			} else {
				throw new System.Exception(
					$"Could not find valid building spot for {structure.ToString()} using prefab {chosenStructurePrefab.name}");
			}
			yield return null;
		}

	}
	
	#region Cellular Automata
	private IEnumerator GenerateMonsterLair(HexTile hexTile, LocationStructure structure) {
		List<LocationGridTile> locationGridTiles = new List<LocationGridTile>(hexTile.locationGridTiles);

		MonsterLairCellAutomata(locationGridTiles, structure);
		
		for (int j = 0; j < hexTile.ownedBuildSpots.Length; j++) {
			BuildingSpot spot = hexTile.ownedBuildSpots[j];
			if (spot.isOccupied == false) {
				spot.SetIsOccupied(true);
				spot.UpdateAdjacentSpotsOccupancy(hexTile.region.innerMap);	
			}
		}
		
		yield return null;
	}
	private void MonsterLairCellAutomata(List<LocationGridTile> locationGridTiles, LocationStructure elevationStructure) {
		List<LocationGridTile> refinedTiles =
			locationGridTiles.Where(t => t.HasNeighbourNotInList(locationGridTiles) == false && t.IsAtEdgeOfMap() == false).ToList();
		
		LocationGridTile[,] tileMap = CellularAutomataGenerator.ConvertListToGridMap(refinedTiles);
		int[,] cellMap = CellularAutomataGenerator.GenerateMap(tileMap, refinedTiles, 1, 35);
		
		Assert.IsNotNull(cellMap, $"There was no cellmap generated for elevation structure {elevationStructure.ToString()}");
		
		CellularAutomataGenerator.DrawMap(tileMap, cellMap, InnerMapManager.Instance.assetManager.caveWallTile, 
			null, 
			(locationGridTile) => SetAsMountainWall(locationGridTile, elevationStructure),
			(locationGridTile) => SetAsMountainGround(locationGridTile, elevationStructure));
	}
	private void SetAsMountainWall(LocationGridTile tile, LocationStructure structure) {
		tile.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.caveGroundTile);
		tile.SetTileType(LocationGridTile.Tile_Type.Wall);
		tile.SetTileState(LocationGridTile.Tile_State.Occupied);
		tile.SetStructure(structure);
	}
	private void SetAsMountainGround(LocationGridTile tile, LocationStructure structure) {
		tile.SetStructure(structure);
		tile.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.caveGroundTile);
		tile.SetStructure(structure);
	}
	#endregion
}
