using UnityEngine;
using System.Collections;

public class MapGenerator : MonoBehaviour {

	void Start() {
		GridMap.Instance.GenerateGrid();
		EquatorGenerator.Instance.GenerateEquator();
		Biomes.Instance.GenerateElevation();
		Biomes.Instance.GenerateBiome();
		Biomes.Instance.GenerateTileDetails();
		CityGenerator.Instance.GenerateHabitableTiles(GridMap.Instance.listHexes);
		//PathGenerator.Instance.GenerateConnections(CityGenerator.Instance.stoneHabitableTiles);
		KingdomManager.Instance.GenerateInitialKingdoms(CityGenerator.Instance.stoneHabitableTiles, CityGenerator.Instance.woodHabitableTiles);
//		UIManager.Instance.UpdateKingsGrid();
		WorldEventManager.Instance.BoonOfPowerTrigger();
		GameManager.Instance.StartProgression();
		CameraMove.Instance.CenterCameraOn(KingdomManager.Instance.allKingdoms[0].cities[0].hexTile.gameObject);
	}

}
