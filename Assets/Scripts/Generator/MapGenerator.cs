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
        System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();

        GridMap.Instance.GenerateGrid();
        CameraMove.Instance.CalculateCameraBounds();
        Minimap.Instance.Initialize();
        ObjectPoolManager.Instance.InitializeObjectPools();
        CameraMove.Instance.SetWholemapCameraValues();
        EquatorGenerator.Instance.GenerateEquator();
        Biomes.Instance.GenerateElevation();
        Biomes.Instance.GenerateBiome();
        Biomes.Instance.LoadPassableObjects();
        
        st.Start();
        bool regionGenerationFailed = !GridMap.Instance.GenerateRegions(GridMap.Instance.numOfRegions, GridMap.Instance.refinementLevel);
        st.Stop();

        if (regionGenerationFailed) {
            Debug.LogWarning("Region generation ran into a problem, reloading scene...");
            Messenger.Cleanup();
            ReloadScene();
            return;
        } else {
            Debug.Log(string.Format("Region Generation took {0} ms to complete", st.ElapsedMilliseconds));
        }

        st.Start();
        Biomes.Instance.DetermineIslands();
        st.Stop();
        Debug.Log(string.Format("Island Connections took {0} ms to complete", st.ElapsedMilliseconds));

        RoadManager.Instance.FlattenRoads();
        Biomes.Instance.LoadElevationSprites();
        Biomes.Instance.GenerateTileBiomeDetails();
        RoadManager.Instance.GenerateTilePassableTypes();
        return;




        GridMap.Instance.GenerateOuterGrid();
        GridMap.Instance.DivideOuterGridRegions();

        UIManager.Instance.InitializeUI();

        st.Start();
        bool factionGenerationFailed = !FactionManager.Instance.GenerateInitialFactions();
        st.Stop();

        if (factionGenerationFailed) {
            //reset
            Debug.LogWarning("Faction generation ran into a problem, reloading scene...");
            Messenger.Cleanup();
            ReloadScene();
            return;
        } else {
            Debug.Log(string.Format("Faction Generation took {0} ms to complete", st.ElapsedMilliseconds));
        }

        st.Start();
        bool landmarkGenerationFailed = !LandmarkManager.Instance.GenerateLandmarks();
        st.Stop();

        if (landmarkGenerationFailed) {
            //reset
            Debug.LogWarning("Landmark generation ran into a problem, reloading scene...");
            Messenger.Cleanup();
            ReloadScene();
            return;
        } else {
            Debug.Log(string.Format("Landmark Generation took {0} ms to complete", st.ElapsedMilliseconds));
        }

        st.Start();
        bool roadGenerationFailed = !RoadManager.Instance.GenerateRoads();
        st.Stop();

        if (roadGenerationFailed) {
            //reset
            Debug.LogWarning("Road generation ran into a problem, reloading scene...");
            Messenger.Cleanup();
            ReloadScene();
            return;
        } else {
            Debug.Log(string.Format("Road Generation took {0} ms to complete", st.ElapsedMilliseconds));
        }

        PathfindingManager.Instance.CreateGrid();

        FactionManager.Instance.OccupyLandmarksInFactionRegions();
        ObjectManager.Instance.Initialize();
        LandmarkManager.Instance.ConstructAllLandmarkObjects();

        LandmarkManager.Instance.GenerateMaterials();

        RoadManager.Instance.FlattenRoads();
        Biomes.Instance.GenerateTileTags();
        GridMap.Instance.GenerateNeighboursWithSameTag();
        Biomes.Instance.LoadElevationSprites();
        Biomes.Instance.GenerateTileBiomeDetails();

        GameManager.Instance.StartProgression();
        LandmarkManager.Instance.InitializeLandmarks();
        CharacterManager.Instance.GenerateCharactersForTesting(1);
        //FactionManager.Instance.GenerateFactionCharacters();
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
