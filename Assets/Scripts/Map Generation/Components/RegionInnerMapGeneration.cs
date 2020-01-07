using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;

public class RegionInnerMapGeneration : MapGenerationComponent {
	
	public override IEnumerator Execute(MapGenerationData data) {
		for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
			Region region = GridMap.Instance.allRegions[i];
			int width;
			int height;
			GetInnerTileMapSize(region, out width, out height);
			yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.GenerateRegionMap(region, width, height));
		}
	}


	private void GetInnerTileMapSize(Region region, out int width, out int height) {
		int maxX = region.tiles.Max(t => t.data.xCoordinate);
		int minX = region.tiles.Min(t => t.data.xCoordinate);
		int maxY = region.tiles.Max(t => t.data.yCoordinate);
		int minY = region.tiles.Min(t => t.data.yCoordinate);

		width = ((maxX - minX) + 1) * 14;
		height = ((maxY - minY) + 1) * 14;
	}
}
