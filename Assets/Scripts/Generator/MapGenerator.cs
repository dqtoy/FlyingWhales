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
        StartCoroutine(InitializeWorldCoroutine());
    }
    internal void InitializeWorld(WorldSaveData data) {
        StartCoroutine(InitializeWorldCoroutine(data));
    }
    private IEnumerator InitializeWorldCoroutine() {
        System.Diagnostics.Stopwatch loadingWatch = new System.Diagnostics.Stopwatch();
        System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
        loadingWatch.Start();

        LevelLoaderManager.UpdateLoadingInfo("Generating Map...");
        yield return null;
        GridMap.Instance.GenerateGrid();
        CameraMove.Instance.Initialize();
        CameraMove.Instance.CalculateCameraBounds();
        ObjectPoolManager.Instance.InitializeObjectPools();
        //CameraMove.Instance.SetWholemapCameraValues();
        //Minimap.Instance.Initialize();  //TODO: Uncomment when minimap is put back
        EquatorGenerator.Instance.GenerateEquator((int)GridMap.Instance.width, (int)GridMap.Instance.height, GridMap.Instance.hexTiles);
        Biomes.Instance.GenerateElevation(GridMap.Instance.hexTiles, (int)GridMap.Instance.width, (int)GridMap.Instance.height);

        LevelLoaderManager.UpdateLoadingInfo("Generating Biomes...");
        yield return null;
        Biomes.Instance.GenerateBiome(GridMap.Instance.hexTiles);
        Biomes.Instance.LoadPassableStates(GridMap.Instance.hexTiles, GridMap.Instance.outerGridList);

        LevelLoaderManager.UpdateLoadingInfo("Generating Regions...");
        yield return null;
        st.Start();
        GridMap.Instance.GenerateRegions(GridMap.Instance.numOfRegions, GridMap.Instance.refinementLevel);
        st.Stop();

        RoadManager.Instance.GenerateTilePassableTypes();

        GridMap.Instance.GenerateOuterGrid();
        //GridMap.Instance.DivideOuterGridRegions();

        UIManager.Instance.InitializeUI();
        //ObjectManager.Instance.Initialize();
        //CharacterScheduleManager.Instance.Initialize();

        LevelLoaderManager.UpdateLoadingInfo("Generating Factions...");
        yield return null;
        st.Start();
        FactionManager.Instance.GenerateInitialFactions();
        st.Stop();

        LevelLoaderManager.UpdateLoadingInfo("Generating Landmarks...");
        yield return null;
        st.Start();
        LandmarkManager.Instance.GenerateFactionLandmarks();
        st.Stop();

        TokenManager.Instance.Initialize();
        //PathfindingManager.Instance.CreateGrid(GridMap.Instance.map, (int)GridMap.Instance.width, (int)GridMap.Instance.height);

        //FactionManager.Instance.OccupyLandmarksInFactionRegions();

        LevelLoaderManager.UpdateLoadingInfo("Starting Game...");
        yield return null;
        //ObjectManager.Instance.Initialize();

        Biomes.Instance.UpdateTileVisuals(GridMap.Instance.allTiles);
        //Biomes.Instance.GenerateTileBiomeDetails(GridMap.Instance.hexTiles);

        LandmarkManager.Instance.InitializeLandmarks();
        //CharacterManager.Instance.GenerateCharactersForTesting(8);
        //CameraMove.Instance.UpdateMinimapTexture();
        //QuestManager.Instance.Initialize();
        //EventManager.Instance.Initialize();
        //CharacterManager.Instance.LoadCharactersInfo();
        if (SteamManager.Initialized) {
            AchievementManager.Instance.Initialize();
        }

        loadingWatch.Stop();
        Debug.Log(string.Format("Total loading time is {0} ms", loadingWatch.ElapsedMilliseconds));
        LevelLoaderManager.SetLoadingState(false);
        Messenger.Broadcast(Signals.GAME_LOADED);
        yield return new WaitForSeconds(1f);
        PlayerManager.Instance.ChooseStartingTile();
        //GameManager.Instance.StartProgression();
    }
    private IEnumerator InitializeWorldCoroutine(WorldSaveData data) {
        System.Diagnostics.Stopwatch loadingWatch = new System.Diagnostics.Stopwatch();
        System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
        loadingWatch.Start();

        LevelLoaderManager.UpdateLoadingInfo("Loading Map...");
        yield return null;
        GridMap.Instance.GenerateGrid(data);
        CameraMove.Instance.Initialize();
        ObjectPoolManager.Instance.InitializeObjectPools();

        LevelLoaderManager.UpdateLoadingInfo("Loading Regions...");
        yield return null;
        st.Start();
        GridMap.Instance.LoadRegions(data);
        st.Stop();

        GridMap.Instance.GenerateOuterGrid(data);
        CameraMove.Instance.CalculateCameraBounds();

        Biomes.Instance.LoadPassableStates(GridMap.Instance.hexTiles, GridMap.Instance.outerGridList);

        UIManager.Instance.InitializeUI();
        InteriorMapManager.Instance.Initialize();

        LevelLoaderManager.UpdateLoadingInfo("Loading Factions...");
        yield return null;
        st.Start();
        FactionManager.Instance.LoadFactions(data);
        st.Stop();

        LevelLoaderManager.UpdateLoadingInfo("Loading Areas...");
        yield return null;
        LandmarkManager.Instance.LoadAreas(data);


        LevelLoaderManager.UpdateLoadingInfo("Loading Landmarks...");
        yield return null;
        st.Start();
        LandmarkManager.Instance.LoadLandmarks(data);
        st.Stop();

        LevelLoaderManager.UpdateLoadingInfo("Starting Game...");
        yield return null;

        Biomes.Instance.UpdateTileVisuals(GridMap.Instance.allTiles);

        LandmarkManager.Instance.InitializeLandmarks();

        LandmarkManager.Instance.LoadAdditionalAreaData();

        TokenManager.Instance.Initialize();

        FactionManager.Instance.RandomizeStartingFactions(data);
        CharacterManager.Instance.LoadCharacters(data);
        FactionManager.Instance.GenerateStartingFactionData();
        CharacterManager.Instance.CreateNeutralCharacters();

        CharacterManager.Instance.LoadRelationships(data);
        CharacterManager.Instance.GenerateRelationships();
        CharacterManager.Instance.PlaceInitialCharacters();
        CharacterManager.Instance.GiveInitialItems();

        AudioManager.Instance.TransitionTo("World Music", 10);

        CharacterManager.Instance.GenerateInitialAwareness();
        InteractionManager.Instance.Initialize();
        if (SteamManager.Initialized) {
            AchievementManager.Instance.Initialize();
        }
        loadingWatch.Stop();
        Debug.Log(string.Format("Total loading time is {0} ms", loadingWatch.ElapsedMilliseconds));
        LevelLoaderManager.SetLoadingState(false);

        Messenger.Broadcast(Signals.GAME_LOADED);
        
        yield return new WaitForSeconds(1f);
        PlayerManager.Instance.LoadStartingTile();
    }

    internal void ReloadScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
