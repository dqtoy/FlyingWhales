using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Globalization;
using Inner_Maps;

public class MapGenerator : MonoBehaviour {

    public static MapGenerator Instance;

    private bool isCoroutineRunning;

    private void Awake() {
        Instance = this;
    }

    internal void InitializeWorld() {
        MapGenerationComponent[] mapGenerationComponents = {
            new WorldMapGridGeneration(), new WorldMapElevationGeneration(), new SupportingFactionGeneration(), 
            new WorldMapRegionGeneration(), new WorldMapBiomeGeneration(), new WorldMapOuterGridGeneration(),
            new TileFeatureGeneration(), new PortalLandmarkGeneration(), new WorldMapLandmarkGeneration(), 
            new RegionInnerMapGeneration(), 
             
            // new MainSettlementMapGeneration(), new SuppotingSettlementMapGeneration(), new RegionDataGeneration(), 
            // new PlayerDataGeneration(), 
        };
        StartCoroutine(InitializeWorldCoroutine(mapGenerationComponents));
    }
    public void InitializeWorld(Save data) {
        StartCoroutine(InitializeWorldCoroutine(data));
    }
    private IEnumerator InitializeWorldCoroutine(MapGenerationComponent[] components) {
        System.Diagnostics.Stopwatch loadingWatch = new System.Diagnostics.Stopwatch();
        loadingWatch.Start();

        MapGenerationData data = new MapGenerationData();
        for (int i = 0; i < components.Length; i++) {
            MapGenerationComponent currComponent = components[i];
            yield return StartCoroutine(currComponent.Execute(data));
        }
        
        loadingWatch.Stop();
        Debug.Log($"Total loading time is {loadingWatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds");
 
        LevelLoaderManager.SetLoadingState(false);
        // CameraMove.Instance.CenterCameraOn(PlayerManager.Instance.player.playerArea.coreTile.gameObject);
        AudioManager.Instance.TransitionTo("World Music", 10);
        Messenger.Broadcast(Signals.GAME_LOADED);
        yield return new WaitForSeconds(1f);
        // PlayerManager.Instance.player.IncreaseArtifactSlot();
        // PlayerManager.Instance.player.IncreaseSummonSlot();
        GameManager.Instance.StartProgression();
        UIManager.Instance.SetSpeedTogglesState(true);
        // UIManager.Instance.SetSpeedTogglesState(false);
        // PlayerUI.Instance.ShowStartingMinionPicker();

    }
    private IEnumerator InitializeWorldCoroutine(Save data) {
        System.Diagnostics.Stopwatch loadingWatch = new System.Diagnostics.Stopwatch();
        //System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
        loadingWatch.Start();

        LevelLoaderManager.UpdateLoadingInfo("Loading Map...");
        GridMap.Instance.SetupInitialData(data.width, data.height);
        yield return null;
        CameraMove.Instance.Initialize();
        InnerMapManager.Instance.Initialize();
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
        LandmarkManager.Instance.LoadAdditionalAreaData();
        data.LoadCharacters();
        data.LoadSpecialObjects();
        data.LoadTileObjects();
        yield return null;
        data.LoadCharacterRelationships();
        data.LoadCharacterTraits();
        yield return null;
        data.LoadLandmarks();
        data.LoadRegionConnections();
        data.LoadRegionCharacters();
        data.LoadRegionAdditionalData();
        yield return null;

        CameraMove.Instance.CalculateCameraBounds();
        UIManager.Instance.InitializeUI();
        LevelLoaderManager.UpdateLoadingInfo("Starting Game...");
        yield return null;

        TokenManager.Instance.Initialize();
        //CharacterManager.Instance.GenerateRelationships();

        yield return null;
        //LandmarkManager.Instance.GenerateAreaMap(LandmarkManager.Instance.enemyOfPlayerArea, false);
        data.LoadAreaMaps();
        data.LoadAreaStructureEntranceTiles();
        data.LoadTileObjectsPreviousTileAndCurrentTile();
        data.LoadAreaMapsObjectHereOfTiles();
        data.LoadAreaMapsTileTraits();
        data.LoadTileObjectTraits();
        data.LoadCharacterHomeStructures();
        data.LoadCurrentDate(); //Moved this because some jobs use current date
        data.LoadCharacterInitialPlacements();
        data.LoadPlayer();

        data.LoadAllJobs();
        data.LoadTileObjectsDataAfterLoadingAreaMap();

        //Note: Loading area items is after loading the inner map because LocationStructure and LocationGridTile is required
        data.LoadPlayerAreaItems();
        data.LoadNonPlayerAreaItems();
        yield return null;
        data.LoadCharacterHistories();

        data.LoadCharacterCurrentStates();
        data.LoadFactionsActiveQuests();

        
        data.LoadNotifications();

        loadingWatch.Stop();
        Debug.Log(string.Format("Total loading time is {0} ms", loadingWatch.ElapsedMilliseconds.ToString()));
        LevelLoaderManager.SetLoadingState(false);
        CameraMove.Instance.CenterCameraOn(PlayerManager.Instance.player.playerArea.coreTile.gameObject);
        AudioManager.Instance.TransitionTo("World Music", 10);
        yield return new WaitForSeconds(1f);
        GameManager.Instance.StartProgression();
        UIManager.Instance.SetSpeedTogglesState(true);
        Messenger.Broadcast(Signals.UPDATE_UI);
        yield return null;
        UIManager.Instance.Unpause();
        yield return null;
        UIManager.Instance.Pause();
        Messenger.Broadcast(Signals.GAME_LOADED);
        //data.LoadInvasion();
        //PlayerManager.Instance.player.LoadResearchNewInterventionAbility(data.playerSave);

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
    public void SetIsCoroutineRunning(bool state) {
        isCoroutineRunning = state;
    }
}
