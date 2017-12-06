using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;

public class MapGenerator : MonoBehaviour {

    public static MapGenerator Instance = null;

    private void Awake() {
        Instance = this;
    }

    internal void Start() {
        //StartCoroutine (StartGeneration());
        if (GameManager.Instance.enableGameAgents) {
            PathfindingManager.Instance.Initialize();
        }
        GridMap.Instance.GenerateGrid();
        CameraMove.Instance.CalculateCameraBounds();
        Minimap.Instance.Initialize();
        CitizenManager.Instance.ConstructTraitDictionary();
        ObjectPoolManager.Instance.InitializeObjectPools();
        CameraMove.Instance.SetWholemapCameraValues();
        EquatorGenerator.Instance.GenerateEquator();
        Biomes.Instance.GenerateElevation();
        Biomes.Instance.GenerateBiome();
        if (GameManager.Instance.enableGameAgents) {
            PathfindingManager.Instance.CreateGrid();
        }
        
        //Biomes.Instance.GenerateSpecialResources ();
        Biomes.Instance.GenerateTileTags();
        GridMap.Instance.GenerateNeighboursWithSameTag();
        if(!GridMap.Instance.GenerateRegions(GridMap.Instance.numOfRegions, GridMap.Instance.refinementLevel)) {
            Debug.LogWarning("Region generation ran into a problem, reloading scene...");
            Messenger.Cleanup();
            ReloadScene();
            return;
        }
        GridMap.Instance.GenerateOuterGrid();
        GridMap.Instance.DivideOuterGridRegions();
        if (!RoadManager.Instance.GenerateRegionRoads()) {
            //reset
            Debug.LogWarning("Road generation ran into a problem, reloading scene...");
            Messenger.Cleanup();
            ReloadScene();
            return;
        }
		KingdomManager.Instance.GenerateInitialKingdoms();
		GridMap.Instance.UpdateAllRegionsDiscoveredKingdoms();

        GridMap.Instance.GenerateResourcesPerRegion();
        //		GridMap.Instance.GenerateResourceTiles();
        GridMap.Instance.GenerateOtherLandmarksPerRegion();
        GridMap.Instance.GenerateUniqueLandmarks();
        GridMap.Instance.GenerateLandmarkExtraConnections();
        //GridMap.Instance.GenerateLandmarkExternalConnections();
        //GridMap.Instance.CheckLandmarkExternalConnections();
        Biomes.Instance.GenerateElevationAfterRoads();
        //      GridMap.Instance.GenerateLandmarksPerRegion();
        //GridMap.Instance.GenerateRoadConnectionLandmarkToCity();
        //GridMap.Instance.GenerateCityConnections ();
        //GridMap.Instance.GenerateExtraLandmarkConnections ();
        Biomes.Instance.GenerateTileBiomeDetails();
        Biomes.Instance.GenerateTileEdges();
        //CityGenerator.Instance.GenerateHabitableTiles(GridMap.Instance.listHexes);

        UIManager.Instance.InitializeUI();
        ////PathGenerator.Instance.GenerateConnections(CityGenerator.Instance.stoneHabitableTiles);
		UIManager.Instance.SetKingdomAsActive(KingdomManager.Instance.allKingdoms[0]);




        //CityGenerator.Instance.GenerateLairHabitableTiles(GridMap.Instance.listHexes);
        //MonsterManager.Instance.GenerateLairs();
        //      //UIManager.Instance.UpdateKingsGrid();
        //      //WorldEventManager.Instance.TriggerInitialWorldEvents();
        ////WorldEventManager.Instance.BoonOfPowerTrigger();
        ////WorldEventManager.Instance.AltarOfBlessingTrigger();
        //      //WorldEventManager.Instance.FirstAndKeystoneTrigger();
        GameManager.Instance.StartProgression();
        CameraMove.Instance.CenterCameraOn(KingdomManager.Instance.allKingdoms.FirstOrDefault().cities.FirstOrDefault().hexTile.gameObject);
        CameraMove.Instance.UpdateMinimapTexture();
        
    }

    internal void ReloadScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
