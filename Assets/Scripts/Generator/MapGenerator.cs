using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {

    public static MapGenerator Instance = null;

    private int width;
    private int height;
    private bool isCoroutineRunning;


    private void Awake() {
        Instance = this;
    }

    internal void InitializeWorld() {
        StartCoroutine(InitializeWorldCoroutine());
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
        EquatorGenerator.Instance.GenerateEquator(GridMap.Instance.width, GridMap.Instance.height, GridMap.Instance.hexTiles);
        Biomes.Instance.GenerateElevation(GridMap.Instance.hexTiles, GridMap.Instance.width, GridMap.Instance.height);

        //CameraMove.Instance.Initialize();
        //InteriorMapManager.Instance.Initialize();
        //ObjectPoolManager.Instance.InitializeObjectPools();

        LevelLoaderManager.UpdateLoadingInfo("Generating Biomes...");
        yield return null;
        Biomes.Instance.GenerateBiome(GridMap.Instance.hexTiles);
        Biomes.Instance.UpdateTileVisuals(GridMap.Instance.allTiles);
        yield return null;
        BaseLandmark portal;

        //New Map Generation
        BaseLandmark settlement;
        GridMap.Instance.DivideToRegions(GridMap.Instance.hexTiles, regionCount, width * height);
        LandmarkManager.Instance.GenerateLandmarks(GridMap.Instance.allRegions, out portal, out settlement);
        LandmarkManager.Instance.GenerateConnections(portal, settlement);

        FactionManager.Instance.CreateNeutralFaction();
        FactionManager.Instance.CreateDisguisedFaction();
        //LandmarkManager.Instance.SetCascadingLevelsForAllCharacters(portal.tileLocation);
        //LandmarkManager.Instance.GenerateRegionFeatures();
        yield return null;


        GridMap.Instance.GenerateOuterGrid();
        Biomes.Instance.UpdateTileVisuals(GridMap.Instance.allTiles);
        CameraMove.Instance.CalculateCameraBounds();
        yield return null;
        //UIManager.Instance.InitializeUI();

        //RelationshipManager.Instance.GenerateRelationships();
        //CharacterManager.Instance.PlaceInitialCharacters();
        CharacterManager.Instance.GiveInitialItems();
        //CharacterManager.Instance.GenerateInitialAwareness();

        PlayerManager.Instance.InitializePlayer(portal);
        yield return null;
        LandmarkManager.Instance.GenerateAreaMap(settlement.tileLocation.areaOfTile);
        yield return null;
        LandmarkManager.Instance.CreateTwoNewSettlementsAtTheStartOfGame();
        yield return null;
        LandmarkManager.Instance.LoadAdditionalAreaData();
        yield return null;
        //TokenManager.Instance.Initialize();

        loadingWatch.Stop();
        Debug.Log(string.Format("Total loading time is {0} ms", loadingWatch.ElapsedMilliseconds));
        LevelLoaderManager.SetLoadingState(false);
        CameraMove.Instance.CenterCameraOn(PlayerManager.Instance.player.playerArea.coreTile.gameObject);
        AudioManager.Instance.TransitionTo("World Music", 10);
        Messenger.Broadcast(Signals.GAME_LOADED);
        yield return new WaitForSeconds(1f);
        GameManager.Instance.StartProgression();
        UIManager.Instance.SetSpeedTogglesState(false);
        PlayerUI.Instance.ShowStartingMinionPicker();
        PlayerManager.Instance.player.IncreaseArtifactSlot();
        PlayerManager.Instance.player.IncreaseSummonSlot();
        PlayerManager.Instance.player.GainArtifact(ARTIFACT_TYPE.Necronomicon);
    }
    private IEnumerator InitializeWorldCoroutine(Save data) {
        System.Diagnostics.Stopwatch loadingWatch = new System.Diagnostics.Stopwatch();
        //System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
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
        WorldEventsManager.Instance.Initialize();

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

        data.LoadWorldEventsAndWorldObject();
        data.LoadCharacterCurrentStates();
        data.LoadFactionsActiveQuests();

        
        data.LoadNotifications();

        loadingWatch.Stop();
        Debug.Log(string.Format("Total loading time is {0} ms", loadingWatch.ElapsedMilliseconds));
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
    internal void ReloadScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    private void StartGenerateMapWidthAndHeightFromRegionCountCoroutine(int regionCount) {
        //StartCoroutine(GenerateMapWidthAndHeightFromRegionCountCoroutine(regionCount));
    }
    private IEnumerator GenerateMapWidthAndHeightFromRegionCountCoroutine(int regionCount) {
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
            yield return null;
        }
        //summary += "\nComputed total tiles : " + totalTiles.ToString();
        summary += "\nTotal tiles : " + (width * height).ToString();

        Debug.Log(summary);
        SetIsCoroutineRunning(false);
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
