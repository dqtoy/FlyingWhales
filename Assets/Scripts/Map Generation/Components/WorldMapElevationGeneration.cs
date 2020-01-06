using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapElevationGeneration : MapGenerationComponent {
	public override IEnumerator Execute(MapGenerationData data) {
		EquatorGenerator.Instance.GenerateEquator(GridMap.Instance.width, GridMap.Instance.height, GridMap.Instance.hexTiles);
		yield return  MapGenerator.Instance.StartCoroutine(Biomes.Instance.GenerateElevation(GridMap.Instance.hexTiles, GridMap.Instance.width, GridMap.Instance.height));
	}
}
