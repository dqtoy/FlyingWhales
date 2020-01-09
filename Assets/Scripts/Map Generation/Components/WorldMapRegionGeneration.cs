using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapRegionGeneration : MapGenerationComponent {
	
	public override IEnumerator Execute(MapGenerationData data) {
		yield return MapGenerator.Instance.StartCoroutine(GridMap.Instance.DivideToRegions(
			GridMap.Instance.normalHexTiles, data.regionCount, 8));
	}
}
