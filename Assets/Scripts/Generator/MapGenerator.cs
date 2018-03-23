using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;

public class MapGenerator : MonoBehaviour {

    public static MapGenerator Instance = null;

    private void Awake() {
        Instance = this;
    }

    internal void InitializeWorld() {
        //StartCoroutine (StartGeneration());
        if (GameManager.Instance.enableGameAgents) {
            PathfindingManager.Instance.Initialize();
        }
        GridMap.Instance.GenerateGrid();
        CameraMove.Instance.CalculateCameraBounds();
        Minimap.Instance.Initialize();
        ObjectPoolManager.Instance.InitializeObjectPools();
        CameraMove.Instance.SetWholemapCameraValues();
        EquatorGenerator.Instance.GenerateEquator();
        Biomes.Instance.GenerateElevation();
        Biomes.Instance.GenerateBiome();
        if (GameManager.Instance.enableGameAgents) {
            PathfindingManager.Instance.CreateGrid();
        }
        
        //Biomes.Instance.GenerateSpecialResources ();
        //Biomes.Instance.GenerateTileTags();
        if(!GridMap.Instance.GenerateRegions(GridMap.Instance.numOfRegions, GridMap.Instance.refinementLevel)) {
            Debug.LogWarning("Region generation ran into a problem, reloading scene...");
            Messenger.Cleanup();
            ReloadScene();
            return;
        }
        GridMap.Instance.GenerateOuterGrid();
        GridMap.Instance.DivideOuterGridRegions();

        UIManager.Instance.InitializeUI();

        if (!FactionManager.Instance.GenerateInitialFactions()) {
            //reset
            Debug.LogWarning("Faction generation ran into a problem, reloading scene...");
            Messenger.Cleanup();
            ReloadScene();
            return;
        }

        if (!RoadManager.Instance.GenerateRegionRoads()) {
            //reset
            Debug.LogWarning("Road generation ran into a problem, reloading scene...");
            Messenger.Cleanup();
            ReloadScene();
            return;
        }        
        LandmarkManager.Instance.GenerateOtherLandmarks();
        LandmarkManager.Instance.GenerateMaterials();

        //KingdomManager.Instance.GenerateInitialKingdoms();
        //GridMap.Instance.UpdateAllRegionsDiscoveredKingdoms();

        //GridMap.Instance.GenerateResourcesPerRegion();
        //		GridMap.Instance.GenerateResourceTiles();
        //GridMap.Instance.GenerateOtherLandmarksPerRegion();
        //GridMap.Instance.GenerateUniqueLandmarks();
        //GridMap.Instance.GenerateLandmarkExtraConnections();
        //Biomes.Instance.GenerateElevationAfterRoads();
        //Biomes.Instance.GenerateRegionBorderElevation();
        RoadManager.Instance.FlattenRoads();
        Biomes.Instance.GenerateTileTags();
        GridMap.Instance.GenerateNeighboursWithSameTag();
        Biomes.Instance.LoadElevationSprites();
        Biomes.Instance.GenerateTileBiomeDetails();
        Biomes.Instance.GenerateTileEdges();

        //UIManager.Instance.SetKingdomAsActive(KingdomManager.Instance.allKingdoms[0]);

        GameManager.Instance.StartProgression();
        LandmarkManager.Instance.InitializeLandmarks();
        FactionManager.Instance.GenerateFactionCharacters();
        StorylineManager.Instance.GenerateStoryLines();
		CharacterManager.Instance.SchedulePrisonerConversion ();
        //CameraMove.Instance.CenterCameraOn(KingdomManager.Instance.allKingdoms.FirstOrDefault().cities.FirstOrDefault().hexTile.gameObject);
		CameraMove.Instance.CenterCameraOn(FactionManager.Instance.allTribes.FirstOrDefault().settlements.FirstOrDefault().tileLocation.gameObject);
        CameraMove.Instance.UpdateMinimapTexture();
    }

    internal void ReloadScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
