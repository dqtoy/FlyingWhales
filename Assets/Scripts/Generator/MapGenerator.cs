using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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
    public void InitializeWorld(Save data) {
        StartCoroutine(InitializeWorldCoroutine(data));
    }
    private IEnumerator InitializeWorldCoroutine() {
        System.Diagnostics.Stopwatch loadingWatch = new System.Diagnostics.Stopwatch();
        loadingWatch.Start();

        LevelLoaderManager.UpdateLoadingInfo("Generating Map...");
        yield return null;
        RandomWorld world = WorldConfigManager.Instance.GenerateRandomWorldData();
        world.LogWorldData();
        //GridMap.Instance.SetupInitialData(WorldConfigManager.Instance.gridSizeX, WorldConfigManager.Instance.gridSizeY);
        GridMap.Instance.SetupInitialData(world.mapWidth, world.mapHeight);
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
        BaseLandmark portal;

        LandmarkManager.Instance.GenerateLandmarks(world, out portal);
        FactionManager.Instance.CreateNeutralFaction();


        LandmarkManager.Instance.SetCascadingLevelsForAllCharacters(portal.tileLocation);
        LandmarkManager.Instance.LoadAdditionalAreaData();
        GridMap.Instance.GenerateInitialTileTags();
        yield return null;

        //Region[] generatedRegions;
        //LandmarkManager.Instance.DivideToRegions(GridMap.Instance.hexTiles, WorldConfigManager.Instance.regionCount, WorldConfigManager.Instance.gridSizeX * WorldConfigManager.Instance.gridSizeY, out generatedRegions);
        //LandmarkManager.Instance.GenerateSettlements(new IntRange(WorldConfigManager.Instance.minSettltementCount, WorldConfigManager.Instance.maxSettltementCount), generatedRegions, new IntRange(WorldConfigManager.Instance.minCitizenCount, WorldConfigManager.Instance.maxCitizenCount), out portal);
        //LandmarkManager.Instance.SetCascadingLevelsForAllCharacters(portal.tileLocation);
        //LandmarkManager.Instance.LoadAdditionalAreaData();
        //LandmarkManager.Instance.GenerateMinorLandmarks(GridMap.Instance.hexTiles);
        //GridMap.Instance.GenerateInitialTileTags();
        //yield return null;

        GridMap.Instance.GenerateOuterGrid();
        Biomes.Instance.UpdateTileVisuals(GridMap.Instance.allTiles);
        CameraMove.Instance.CalculateCameraBounds();
        yield return null;
        UIManager.Instance.InitializeUI();

        TokenManager.Instance.Initialize();
        CharacterManager.Instance.GenerateRelationships();
        //CharacterManager.Instance.PlaceInitialCharacters();
        CharacterManager.Instance.GiveInitialItems();
        //CharacterManager.Instance.GenerateInitialAwareness();
        InteractionManager.Instance.Initialize();
        StoryEventsManager.Instance.Initialize();

        PlayerManager.Instance.InitializePlayer(portal);

        loadingWatch.Stop();
        Debug.Log(string.Format("Total loading time is {0} ms", loadingWatch.ElapsedMilliseconds));
        LevelLoaderManager.SetLoadingState(false);
        Messenger.Broadcast(Signals.GAME_LOADED);
        CameraMove.Instance.CenterCameraOn(PlayerManager.Instance.player.playerArea.coreTile.gameObject);
        yield return new WaitForSeconds(1f);
        GameManager.Instance.StartProgression();
        UIManager.Instance.SetTimeControlsState(false);

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

    private IEnumerator InitializeWorldCoroutine(Save data) {
        System.Diagnostics.Stopwatch loadingWatch = new System.Diagnostics.Stopwatch();
        System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
        loadingWatch.Start();

        LevelLoaderManager.UpdateLoadingInfo("Loading Map...");
        GridMap.Instance.SetupInitialData(data.width, data.height);
        yield return null;
        CameraMove.Instance.Initialize();
        InteriorMapManager.Instance.Initialize();
        ObjectPoolManager.Instance.InitializeObjectPools();
        yield return null;
        GridMap.Instance.GenerateGrid(data);
        yield return null;
        GridMap.Instance.GenerateOuterGrid(data);
        yield return null;
        Biomes.Instance.UpdateTileVisuals(GridMap.Instance.allTiles);
        yield return null;
        data.LoadPlayerArea();
        data.LoadNonPlayerAreas();
        data.LoadFactions();
        data.LoadPlayerAreaItems();
        data.LoadNonPlayerAreaItems();
        LandmarkManager.Instance.LoadAdditionalAreaData();
        data.LoadCharacters();
        yield return null;
        data.LoadCharacterRelationships();
        yield return null;
        data.LoadLandmarks();
        yield return null;
        GridMap.Instance.GenerateInitialTileTags();
        yield return null;

        CameraMove.Instance.CalculateCameraBounds();
        UIManager.Instance.InitializeUI();
        LevelLoaderManager.UpdateLoadingInfo("Starting Game...");
        yield return null;

        TokenManager.Instance.Initialize();
        //CharacterManager.Instance.GenerateRelationships();
        InteractionManager.Instance.Initialize();
        StoryEventsManager.Instance.Initialize();


        data.LoadPlayer();

        loadingWatch.Stop();
        Debug.Log(string.Format("Total loading time is {0} ms", loadingWatch.ElapsedMilliseconds));
        LevelLoaderManager.SetLoadingState(false);
        Messenger.Broadcast(Signals.GAME_LOADED);
        CameraMove.Instance.CenterCameraOn(PlayerManager.Instance.player.playerArea.coreTile.gameObject);
        yield return new WaitForSeconds(1f);
        data.LoadCurrentDate();
        GameManager.Instance.StartProgression();
        UIManager.Instance.SetTimeControlsState(false);
        Messenger.Broadcast(Signals.UPDATE_UI);
    }

    internal void ReloadScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
