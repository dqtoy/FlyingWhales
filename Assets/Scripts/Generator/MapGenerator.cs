using UnityEngine;
using System.Collections;
using System.Linq;

public class MapGenerator : MonoBehaviour {

	void Start() {
		GridMap.Instance.GenerateGrid();
        CameraMove.Instance.SetMinimapCamValues();
        EquatorGenerator.Instance.GenerateEquator();
		Biomes.Instance.GenerateElevation();
		Biomes.Instance.GenerateBiome();
        Biomes.Instance.GenerateTileTags();
		Biomes.Instance.GenerateTileDetails();
		CityGenerator.Instance.GenerateHabitableTiles(GridMap.Instance.listHexes);

		//PathGenerator.Instance.GenerateConnections(CityGenerator.Instance.stoneHabitableTiles);
		KingdomManager.Instance.GenerateInitialKingdoms(CityGenerator.Instance.stoneHabitableTiles, CityGenerator.Instance.woodHabitableTiles);

		CityGenerator.Instance.GenerateLairHabitableTiles(GridMap.Instance.listHexes);
		MonsterManager.Instance.GenerateLairs();
        //		UIManager.Instance.UpdateKingsGrid();
        WorldEventManager.Instance.TriggerInitialWorldEvents();
		//WorldEventManager.Instance.BoonOfPowerTrigger();
		//WorldEventManager.Instance.AltarOfBlessingTrigger();
  //      WorldEventManager.Instance.FirstAndKeystoneTrigger();
        GameManager.Instance.StartProgression();
        CameraMove.Instance.CenterCameraOn(KingdomManager.Instance.allKingdoms.FirstOrDefault().cities.FirstOrDefault().hexTile.gameObject);
    }

}
