using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class MapGenerationFinalization : MapGenerationComponent {
	public override IEnumerator Execute(MapGenerationData data) {
		yield return MapGenerator.Instance.StartCoroutine(RescanAllPathfindingGrids());
	}

	private IEnumerator RescanAllPathfindingGrids() {
		for (int i = 0; i < InnerMapManager.Instance.innerMaps.Count; i++) {
			InnerTileMap map = InnerMapManager.Instance.innerMaps[i];
			PathfindingManager.Instance.RescanGrid(map.pathfindingGraph);
			yield return null;
		}
	}
}
