using UnityEngine;
using System.Collections;

public class MapGenerator : MonoBehaviour {

	void Start () {
		GridMap.Instance.GenerateGrid();
		EquatorGenerator.Instance.GenerateEquator();
		Biomes.Instance.GenerateElevation();
		Biomes.Instance.GenerateBiome();
		CityGenerator.Instance.GenerateHabitableTiles(GridMap.Instance.listHexes);
		PathGenerator.Instance.GenerateConnections(CityGenerator.Instance.habitableTiles);
		KingdomManager.Instance.GenerateInitialKingdoms(CityGenerator.Instance.habitableTiles);
		UIManager.Instance.UpdateKingsGrid();
		GameManager.Instance.StartProgression();
		CameraMove.Instance.CenterCameraOn(KingdomManager.Instance.allKingdoms[0].cities[0].hexTile.gameObject);
	}

}
