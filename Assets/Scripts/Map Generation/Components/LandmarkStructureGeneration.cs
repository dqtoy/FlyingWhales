using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;

public class LandmarkStructureGeneration : MapGenerationComponent {
	public override IEnumerator Execute(MapGenerationData data) {
		List<BaseLandmark> landmarks = LandmarkManager.Instance.GetAllLandmarks();
		for (int i = 0; i < landmarks.Count; i++) {
			BaseLandmark landmark = landmarks[i];
			if (landmark.specificLandmarkType != LANDMARK_TYPE.VILLAGE) {
				yield return MapGenerator.Instance.StartCoroutine(CreateStructureObjectForLandmark(landmark));	
			}
		}
		yield return null;
	}

	private IEnumerator CreateStructureObjectForLandmark(BaseLandmark landmark) {
		LocationStructure structure = LandmarkManager.Instance.CreateNewStructureAt(landmark.tileLocation.region,
			GetStructureTypeFor(landmark.specificLandmarkType));
		 yield return MapGenerator.Instance.StartCoroutine(PlaceInitialStructure(structure, landmark.tileLocation.region.innerMap, landmark.tileLocation));
	}

	private STRUCTURE_TYPE GetStructureTypeFor(LANDMARK_TYPE landmarkType) {
		switch (landmarkType) {
			case LANDMARK_TYPE.MONSTER_LAIR:
				return STRUCTURE_TYPE.MONSTER_LAIR;
			case LANDMARK_TYPE.MINES:
				return STRUCTURE_TYPE.ABANDONED_MINE;
			case LANDMARK_TYPE.TEMPLE:
				return STRUCTURE_TYPE.TEMPLE;
			case LANDMARK_TYPE.MAGE_TOWER:
				return STRUCTURE_TYPE.MAGE_TOWER;
			case LANDMARK_TYPE.THE_PORTAL:
				return STRUCTURE_TYPE.PORTAL;
		}
		throw new Exception($"There is no corresponding structure type for {landmarkType.ToString()}");
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
			if (currSpot.isOccupied == false && currSpot.CanPlaceStructureOnSpot(structureObject, innerTileMap)) {
				spot = currSpot;
				return true;
			}
		}
		spot = null;
		return false;
	}
}
