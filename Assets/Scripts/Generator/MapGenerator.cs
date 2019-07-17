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
        loadingWatch.Start();

        LevelLoaderManager.UpdateLoadingInfo("Generating Map...");
        yield return null;
        GridMap.Instance.SetupInitialData(WorldConfigManager.Instance.gridSizeX, WorldConfigManager.Instance.gridSizeY);
        GridMap.Instance.GenerateGrid();
        EquatorGenerator.Instance.GenerateEquator((int)GridMap.Instance.width, (int)GridMap.Instance.height, GridMap.Instance.hexTiles);
        Biomes.Instance.GenerateElevation(GridMap.Instance.hexTiles, (int)GridMap.Instance.width, (int)GridMap.Instance.height);

        CameraMove.Instance.Initialize();
        InteriorMapManager.Instance.Initialize();
        ObjectPoolManager.Instance.InitializeObjectPools();

        LevelLoaderManager.UpdateLoadingInfo("Generating Biomes...");
        yield return null;
        Biomes.Instance.GenerateBiome(GridMap.Instance.hexTiles);
        Biomes.Instance.UpdateTileVisuals(GridMap.Instance.allTiles);
        yield return null;

        Region[] generatedRegions;
        LandmarkManager.Instance.DivideToRegions(GridMap.Instance.hexTiles, WorldConfigManager.Instance.regionCount, WorldConfigManager.Instance.gridSizeX * WorldConfigManager.Instance.gridSizeY, out generatedRegions);
        BaseLandmark portal;
        LandmarkManager.Instance.GenerateSettlements(new IntRange(WorldConfigManager.Instance.minSettltementCount, WorldConfigManager.Instance.maxSettltementCount), generatedRegions, new IntRange(WorldConfigManager.Instance.minCitizenCount, WorldConfigManager.Instance.maxCitizenCount), out portal);
        LandmarkManager.Instance.LoadAdditionalAreaData();
        LandmarkManager.Instance.GenerateMinorLandmarks(GridMap.Instance.hexTiles);
        GridMap.Instance.GenerateInitialTileTags();
        yield return null;

        GridMap.Instance.GenerateOuterGrid();
        Biomes.Instance.UpdateTileVisuals(GridMap.Instance.outerGridList);
        CameraMove.Instance.CalculateCameraBounds();
        yield return null;
        UIManager.Instance.InitializeUI();
        
        TokenManager.Instance.Initialize();
        CharacterManager.Instance.GenerateRelationships();
        //CharacterManager.Instance.PlaceInitialCharacters();
        CharacterManager.Instance.GiveInitialItems();
        //CharacterManager.Instance.GenerateInitialAwareness();
        InteractionManager.Instance.Initialize();

        PlayerManager.Instance.InitializePlayer(portal);

        loadingWatch.Stop();
        Debug.Log(string.Format("Total loading time is {0} ms", loadingWatch.ElapsedMilliseconds));
        LevelLoaderManager.SetLoadingState(false);
        Messenger.Broadcast(Signals.GAME_LOADED);
        CameraMove.Instance.CenterCameraOn(PlayerManager.Instance.player.playerArea.coreTile.gameObject);
        yield return new WaitForSeconds(1f);
        GameManager.Instance.StartProgression();
        UIManager.Instance.SetTimeControlsState(true);

        PlayerUI.Instance.ShowStartingMinionPicker();
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

        CharacterManager.Instance.LoadRelationships(data);
        CharacterManager.Instance.GenerateRelationships();
        CharacterManager.Instance.GiveInitialItems();
#if TRAILER_BUILD
        CharacterManager.Instance.GenerateInitialLogs();
#endif

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
