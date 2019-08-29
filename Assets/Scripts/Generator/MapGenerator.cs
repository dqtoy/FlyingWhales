﻿using UnityEngine;
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
        int regionCount = Random.Range(WorldConfigManager.Instance.minRegionCount, WorldConfigManager.Instance.maxRegionCount + 1);
        int width;
        int height;
        GenerateMapWidthAndHeightFromRegionCount(regionCount, out width, out height);
        Debug.Log("Width: " + width + " Height: " + height + " Region Count: " + regionCount);
        GridMap.Instance.SetupInitialData(width, height);
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

        //New Map Generation
        BaseLandmark settlement;
        GridMap.Instance.DivideToRegions(GridMap.Instance.hexTiles, regionCount, width * height);
        LandmarkManager.Instance.GenerateLandmarksNew(GridMap.Instance.allRegions, out portal, out settlement);
        LandmarkManager.Instance.GenerateConnections(portal, settlement);

        FactionManager.Instance.CreateNeutralFaction();
        //LandmarkManager.Instance.SetCascadingLevelsForAllCharacters(portal.tileLocation);
        LandmarkManager.Instance.GenerateWorldObjects();
        LandmarkManager.Instance.LoadAdditionalAreaData();
        GridMap.Instance.GenerateInitialTileTags();
        yield return null;


        GridMap.Instance.GenerateOuterGrid();
        Biomes.Instance.UpdateTileVisuals(GridMap.Instance.allTiles);
        CameraMove.Instance.CalculateCameraBounds();
        yield return null;
        UIManager.Instance.InitializeUI();

        CharacterManager.Instance.GenerateRelationships();
        //CharacterManager.Instance.PlaceInitialCharacters();
        CharacterManager.Instance.GiveInitialItems();
        //CharacterManager.Instance.GenerateInitialAwareness();
        InteractionManager.Instance.Initialize();
        StoryEventsManager.Instance.Initialize();

        PlayerManager.Instance.InitializePlayer(portal);

        yield return null;
        LandmarkManager.Instance.GenerateAreaMap(settlement.tileLocation.areaOfTile, false);
        yield return null;

        TokenManager.Instance.Initialize();

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
    private IEnumerator InitializeWorldCoroutine(Save data) {
        System.Diagnostics.Stopwatch loadingWatch = new System.Diagnostics.Stopwatch();
        System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
        loadingWatch.Start();

        LevelLoaderManager.UpdateLoadingInfo("Loading Map...");
        GridMap.Instance.SetupInitialData(data.width, data.height);
        yield return null;
        CameraMove.Instance.Initialize();
        InteriorMapManager.Instance.Initialize();
        InteractionManager.Instance.Initialize();
        ObjectPoolManager.Instance.InitializeObjectPools();
        yield return null;
        GridMap.Instance.GenerateGrid(data);
        yield return null;
        GridMap.Instance.GenerateOuterGrid(data);
        yield return null;
        Biomes.Instance.UpdateTileVisuals(GridMap.Instance.allTiles);
        yield return null;
        data.LoadRegions();
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
        data.LoadLandmarkConnections();
        yield return null;
        GridMap.Instance.GenerateInitialTileTags();
        yield return null;

        CameraMove.Instance.CalculateCameraBounds();
        UIManager.Instance.InitializeUI();
        LevelLoaderManager.UpdateLoadingInfo("Starting Game...");
        yield return null;

        TokenManager.Instance.Initialize();
        //CharacterManager.Instance.GenerateRelationships();
        StoryEventsManager.Instance.Initialize();


        data.LoadPlayer();

        yield return null;
        LandmarkManager.Instance.GenerateAreaMap(LandmarkManager.Instance.enemyPlayerArea, false);
        yield return null;

        loadingWatch.Stop();
        Debug.Log(string.Format("Total loading time is {0} ms", loadingWatch.ElapsedMilliseconds));
        LevelLoaderManager.SetLoadingState(false);
        Messenger.Broadcast(Signals.GAME_LOADED);
        CameraMove.Instance.CenterCameraOn(PlayerManager.Instance.player.playerArea.coreTile.gameObject);
        yield return new WaitForSeconds(1f);
        data.LoadCurrentDate();
        GameManager.Instance.StartProgression();
        UIManager.Instance.SetTimeControlsState(false);
        UIManager.Instance.SetSpeedTogglesState(true);
        Messenger.Broadcast(Signals.UPDATE_UI);
        data.LoadInvasion();
        PlayerManager.Instance.player.LoadResearchNewInterventionAbility(data.playerSave);
        data.LoadLandmarkEvents();
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

    internal void ReloadScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void GenerateMapWidthAndHeightFromRegionCount(int regionCount, out int width, out int height) {
        width = 0;
        height = 0;

        int maxColumns = 6;
        int currColumn = 0;

        int currentRowWidth = 0;

        string summary = "Map Size Generation: ";
        for (int i = 0; i < regionCount; i++) {
            int regionWidth = Random.Range(WorldConfigManager.Instance.minRegionWidthCount, WorldConfigManager.Instance.maxRegionWidthCount + 1);
            int regionHeight = Random.Range(WorldConfigManager.Instance.minRegionHeightCount, WorldConfigManager.Instance.maxRegionHeightCount + 1);
            if (currColumn < maxColumns) {
                //only directly add to width
                currentRowWidth += regionWidth;
                if (currentRowWidth > width) {
                    width = currentRowWidth;
                }
                if (regionHeight > height) {
                    height = regionHeight;
                }
                currColumn++;
            } else {
                //place next set into next row
                currColumn = 1;
                currentRowWidth = 0;
                height += regionHeight;
            }
            
            //if (Utilities.IsEven(i)) {
            //    width += regionWidth;
            //} else {
            //    height += regionHeight;
            //}
            
            //totalTiles += regionWidth * regionHeight;
            summary += "\n" + i + " - Width: " + regionWidth + " Height: " + regionHeight;
        }
        //summary += "\nComputed total tiles : " + totalTiles.ToString();
        summary += "\nTotal tiles : " + (width * height).ToString();

        Debug.Log(summary);
    }
}
