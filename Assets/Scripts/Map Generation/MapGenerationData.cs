using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class used to store all data for map generation, this data will be passed around between
/// map generation components.
/// </summary>
public class MapGenerationData {

	//batching values
	public static int WorldMapTileGenerationBatches = 200;
	public static int WorldMapOuterGridGenerationBatches = 200;
	public static int InnerMapTileGenerationBatches = 200;
	public static int InnerMapSeamlessEdgeBatches = 200;
	public static int InnerMapDetailBatches = 200; 
	
	//world map
	public int mapWidth;
	public int mapHeight;
	public int regionCount;
	public BaseLandmark portal;
	public BaseLandmark settlement;
}
