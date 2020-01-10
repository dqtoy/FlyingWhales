using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class ElevationStructureGeneration : MapGenerationComponent {
	public override IEnumerator Execute(MapGenerationData data) {
		for (int i = 0; i < GridMap.Instance.normalHexTiles.Count; i++) {
			HexTile tile = GridMap.Instance.normalHexTiles[i];
			if (tile.landmarkOnTile == null && tile.elevationType != ELEVATION.PLAIN && tile.elevationType != ELEVATION.TREES) {
				yield return MapGenerator.Instance.StartCoroutine(CreateStructureObjectForTile(tile));
			}
		}
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
}
