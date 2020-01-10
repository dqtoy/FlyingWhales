using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class MapGenerationFinalization : MapGenerationComponent {
	public override IEnumerator Execute(MapGenerationData data) {
		yield return MapGenerator.Instance.StartCoroutine(RescanAllPathfindingGrids());
		yield return MapGenerator.Instance.StartCoroutine(ExecuteFeatureInitialActions());
	}

	private IEnumerator RescanAllPathfindingGrids() {
		for (int i = 0; i < InnerMapManager.Instance.innerMaps.Count; i++) {
			InnerTileMap map = InnerMapManager.Instance.innerMaps[i];
			PathfindingManager.Instance.RescanGrid(map.pathfindingGraph);
			yield return null;
		}
	}

	private IEnumerator ExecuteFeatureInitialActions() {
		for (int i = 0; i < GridMap.Instance.normalHexTiles.Count; i++) {
			HexTile tile = GridMap.Instance.normalHexTiles[i];
			for (int j = 0; j < tile.featureComponent.features.Count; j++) {
				TileFeature feature = tile.featureComponent.features[j];
				feature.PerformInitialActions(tile);
			}
			yield return null;
		}
	}
}
