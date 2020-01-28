using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

public class LandmarkStructureGeneration : MapGenerationComponent {
	public override IEnumerator Execute(MapGenerationData data) {
		List<BaseLandmark> landmarks = LandmarkManager.Instance.GetAllLandmarks();
		for (int i = 0; i < landmarks.Count; i++) {
			BaseLandmark landmark = landmarks[i];
			if (landmark.specificLandmarkType != LANDMARK_TYPE.VILLAGE) {
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
}
