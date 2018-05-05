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
        GridMap.Instance.GenerateGrid();
        CameraMove.Instance.CalculateCameraBounds();
        Minimap.Instance.Initialize();
        ObjectPoolManager.Instance.InitializeObjectPools();
        CameraMove.Instance.SetWholemapCameraValues();
        EquatorGenerator.Instance.GenerateEquator();
        Biomes.Instance.GenerateElevation();
        Biomes.Instance.GenerateBiome();
        Biomes.Instance.LoadPassableObjects();
        if (!GridMap.Instance.GenerateRegions(GridMap.Instance.numOfRegions, GridMap.Instance.refinementLevel)) {
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
        if (!LandmarkManager.Instance.GenerateLandmarks()) {
            //reset
            Debug.LogWarning("Landmark generation ran into a problem, reloading scene...");
            Messenger.Cleanup();
            ReloadScene();
            return;
        }
        if (!RoadManager.Instance.GenerateRoads()) {
            //reset
            Debug.LogWarning("Road generation ran into a problem, reloading scene...");
            Messenger.Cleanup();
            ReloadScene();
            return;
        }
        PathfindingManager.Instance.CreateGrid();

        FactionManager.Instance.OccupyLandmarksInFactionRegions();
        ObjectManager.Instance.Initialize();
        LandmarkManager.Instance.ConstructAllLandmarkObjects();

        LandmarkManager.Instance.GenerateMaterials();

        RoadManager.Instance.FlattenRoads();
        Biomes.Instance.GenerateTileTags();
        GridMap.Instance.GenerateNeighboursWithSameTag();
        //Biomes.Instance.LoadElevationSprites();
        Biomes.Instance.GenerateTileBiomeDetails();

        GameManager.Instance.StartProgression();
        LandmarkManager.Instance.InitializeLandmarks();
        FactionManager.Instance.GenerateFactionCharacters();
        //FactionManager.Instance.GenerateMonsters();
        //StorylineManager.Instance.GenerateStoryLines();
        CharacterManager.Instance.SchedulePrisonerConversion();
        CameraMove.Instance.CenterCameraOn(FactionManager.Instance.allTribes.FirstOrDefault().settlements.FirstOrDefault().tileLocation.gameObject);
        CameraMove.Instance.UpdateMinimapTexture();
    }

    internal void ReloadScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
