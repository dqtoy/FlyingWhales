using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapBiomeGeneration : MapGenerationComponent {
	public override IEnumerator Execute(MapGenerationData data) {
		yield return MapGenerator.Instance.StartCoroutine(Biomes.Instance.GenerateBiome(GridMap.Instance.hexTiles));
	}
}
