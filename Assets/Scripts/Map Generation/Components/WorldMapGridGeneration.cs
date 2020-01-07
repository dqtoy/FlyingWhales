using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapGridGeneration : MapGenerationComponent {
	
	public override IEnumerator Execute(MapGenerationData data) {
		data.regionCount = 5;
		GenerateMapWidthAndHeightFromRegionCount(data.regionCount, out data.mapWidth, out data.mapHeight);
		Debug.Log($"Width: {data.mapWidth.ToString()} Height: {data.mapHeight.ToString()} Region Count: {data.regionCount.ToString()}");
		GridMap.Instance.SetupInitialData(data.mapWidth, data.mapHeight);
		yield return MapGenerator.Instance.StartCoroutine(GridMap.Instance.GenerateGrid());
	}
	
	private void GenerateMapWidthAndHeightFromRegionCount(int regionCount, out int width, out int height) {
		width = 0;
		height = 0;
		int maxColumns = 3;
		int currColumn = 0;
		
		int currentRowWidth = 0;
		
		string summary = "Map Size Generation: ";
		for (int i = 0; i < regionCount; i++) {
			int regionWidth = Random.Range(WorldConfigManager.Instance.minRegionWidthCount, WorldConfigManager.Instance.maxRegionWidthCount + 1);
			int regionHeight = Random.Range(WorldConfigManager.Instance.minRegionHeightCount, WorldConfigManager.Instance.maxRegionHeightCount + 1);
			if (currColumn < maxColumns) {
				//only directly add to width
				currentRowWidth += regionWidth;
				if (currentRowWidth > width) {
					width = currentRowWidth;
				}
				if (regionHeight > height) {
					height = regionHeight;
				}
				currColumn++;
			} else {
				//place next set into next row
				currColumn = 1;
				currentRowWidth = 0;
				height += regionHeight;
			}
			summary += $"\n{i} - Width: {regionWidth.ToString()} Height: {regionHeight.ToString()}";
		}
		summary += "\nTotal tiles : " + (width * height).ToString();
		
		Debug.Log(summary);
	}

}
