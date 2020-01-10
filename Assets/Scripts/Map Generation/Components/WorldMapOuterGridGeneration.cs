using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapOuterGridGeneration : MapGenerationComponent {
	public override IEnumerator Execute(MapGenerationData data) {
		yield return MapGenerator.Instance.StartCoroutine(GridMap.Instance.GenerateOuterGrid());
		Biomes.Instance.UpdateTileVisuals(GridMap.Instance.allTiles);
		CameraMove.Instance.CalculateCameraBounds();
		yield return null;
	}
}
