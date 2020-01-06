using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapRegionGeneration : MapGenerationComponent {
	
	public override IEnumerator Execute(MapGenerationData data) {
		yield return MapGenerator.Instance.StartCoroutine(GridMap.Instance.DivideToRegions(
			GridMap.Instance.hexTiles, data.regionCount, data.mapWidth * data.mapHeight));
		
		yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.GenerateLandmarks(
			GridMap.Instance.allRegions, data));
		
		// yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.GenerateConnections(
		// 	data.portal, data.settlement));
	}
}
