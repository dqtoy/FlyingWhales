using UnityEngine;
using System.Collections;
using System.Linq;

public class MapGenerator : MonoBehaviour {

	void Start() {
        //StartCoroutine (StartGeneration());
        GridMap.Instance.GenerateGrid();
        CameraMove.Instance.CalculateCameraBounds();
        ObjectPoolManager.Instance.InitializeObjectPools();
        //CameraMove.Instance.SetMinimapCamValues();
        EquatorGenerator.Instance.GenerateEquator();
		Biomes.Instance.GenerateElevation();
		Biomes.Instance.GenerateBiome();
		//Biomes.Instance.GenerateSpecialResources ();
        Biomes.Instance.GenerateTileTags();
        GridMap.Instance.GenerateNeighboursWithSameTag();
        GridMap.Instance.GenerateRegions(40, 1);
        GridMap.Instance.GenerateResourcesPerRegion();
        //Biomes.Instance.GenerateTileDetails();
        //CityGenerator.Instance.GenerateHabitableTiles(GridMap.Instance.listHexes);

        ////PathGenerator.Instance.GenerateConnections(CityGenerator.Instance.stoneHabitableTiles);
        KingdomManager.Instance.GenerateInitialKingdoms();

        //CityGenerator.Instance.GenerateLairHabitableTiles(GridMap.Instance.listHexes);
        //MonsterManager.Instance.GenerateLairs();
        //      //UIManager.Instance.UpdateKingsGrid();
        //      //WorldEventManager.Instance.TriggerInitialWorldEvents();
        ////WorldEventManager.Instance.BoonOfPowerTrigger();
        ////WorldEventManager.Instance.AltarOfBlessingTrigger();
        //      //WorldEventManager.Instance.FirstAndKeystoneTrigger();
        GameManager.Instance.StartProgression();
        CameraMove.Instance.CenterCameraOn(KingdomManager.Instance.allKingdoms.FirstOrDefault().cities.FirstOrDefault().hexTile.gameObject);
    }
//	private IEnumerator StartGeneration(){
//		GridMap.Instance.GenerateGrid();
//		yield return null;
//		CameraMove.Instance.SetMinimapCamValues();
//		yield return null;
//		EquatorGenerator.Instance.GenerateEquator();
//		yield return null;
//		Biomes.Instance.GenerateElevation();
//		yield return null;
//		Biomes.Instance.GenerateBiome();
//		yield return null;
//		Biomes.Instance.GenerateSpecialResources ();
//		yield return null;
//		Biomes.Instance.DeactivateCenterPieces ();
//		yield return null;
//		Biomes.Instance.GenerateTileTags();
//		yield return null;
//		Biomes.Instance.GenerateTileDetails();
//		yield return null;
//		CityGenerator.Instance.GenerateHabitableTiles(GridMap.Instance.listHexes);
//		yield return null;
//		CityGenerator.Instance.GenerateLairHabitableTiles(GridMap.Instance.listHexes);
//		yield return null;
//		KingdomManager.Instance.GenerateInitialKingdoms(CityGenerator.Instance.stoneHabitableTiles, CityGenerator.Instance.woodHabitableTiles);
//		yield return null;
//		MonsterManager.Instance.GenerateLairs();
//		yield return null;
////		WorldEventManager.Instance.TriggerInitialWorldEvents();
//		yield return null;
//		GameManager.Instance.StartProgression();
//		yield return null;
//		CameraMove.Instance.CenterCameraOn(KingdomManager.Instance.allKingdoms.FirstOrDefault().cities.FirstOrDefault().hexTile.gameObject);


////		PathGenerator.Instance.GenerateConnections(CityGenerator.Instance.stoneHabitableTiles);

//		//		UIManager.Instance.UpdateKingsGrid();
//		//WorldEventManager.Instance.BoonOfPowerTrigger();
//		//WorldEventManager.Instance.AltarOfBlessingTrigger();
//		//      WorldEventManager.Instance.FirstAndKeystoneTrigger();
//	}
}
