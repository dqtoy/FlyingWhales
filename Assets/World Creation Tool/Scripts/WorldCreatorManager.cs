using BayatGames.SaveGameFree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace worldcreator {
    public class WorldCreatorManager : MonoBehaviour {
        public static WorldCreatorManager Instance = null;

        public bool isDoneLoadingWorld = false;

        [Header("Map Generation")]
        [SerializeField] private float xOffset;
        [SerializeField] private float yOffset;
        [SerializeField] private int tileSize;
        [SerializeField] private GameObject goHex;
        public List<HexTile> hexTiles;
        public HexTile[,] map;
        public int width;
        public int height;
        public GameObject landmarkItemPrefab;

        public EDIT_MODE currentMode;
        public SELECTION_MODE selectionMode;
        public UnitSelectionComponent selectionComponent;

        [Space(10)]
        [Header("Outer Grid")]
        public List<HexTile> outerGridList;
        [SerializeField] private Transform _borderParent;
        public int _borderThickness;

        public List<HexTile> allTiles { get; private set; }

        private void Awake() {
            Instance = this;
        }
        private void Start() {
            DataConstructor.Instance.InitializeData();
        }
        private void Update() {
            //HighlightAreas();
        }

        #region Grid Generation
        public IEnumerator GenerateGrid(int width, int height, bool randomize) {
            this.width = width;
            this.height = height;
            float newX = xOffset * (width / 2);
            float newY = yOffset * (height / 2);
            this.transform.localPosition = new Vector2(-newX, -newY);
            map = new HexTile[(int)width, (int)height];
            hexTiles = new List<HexTile>();
            int totalTiles = width * height;
            int id = 0;
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    float xPosition = x * xOffset;

                    float yPosition = y * yOffset;
                    if (y % 2 == 1) {
                        xPosition += xOffset / 2;
                    }

                    GameObject hex = GameObject.Instantiate(goHex) as GameObject;
                    hex.transform.SetParent(this.transform);
                    hex.transform.localPosition = new Vector3(xPosition, yPosition, 0f);
                    hex.transform.localScale = new Vector3(tileSize, tileSize, 0f);
                    hex.name = x + "," + y;
                    HexTile currHex = hex.GetComponent<HexTile>();
                    hexTiles.Add(currHex);
                    currHex.Initialize();
                    currHex.data.id = id;
                    currHex.data.tileName = RandomNameGenerator.Instance.GetTileName();
                    currHex.data.xCoordinate = x;
                    currHex.data.yCoordinate = y;
                    //listHexes.Add(hex);
                    map[x, y] = currHex;
                    id++;
                    WorldCreatorUI.Instance.UpdateLoading((float)hexTiles.Count / (float)totalTiles, "Generating tile " + id + "/" + totalTiles.ToString());
                    yield return null;
                }
            }
            hexTiles.ForEach(o => o.FindNeighbours(map));
            if (randomize) {
                EquatorGenerator.Instance.GenerateEquator(width, height, hexTiles);
                Biomes.Instance.GenerateElevation(hexTiles, width, height);
                Biomes.Instance.GenerateBiome(hexTiles);
            }

            WorldCreatorUI.Instance.InitializeMenus();
            //CombatManager.Instance.Initialize();
            //TokenManager.Instance.Initialize();
            //Biomes.Instance.GenerateTileBiomeDetails(hexTiles);
            GenerateOuterGrid();
            Biomes.Instance.UpdateTileVisuals(allTiles);
            WorldCreatorUI.Instance.OnDoneLoadingGrid();
        }
        public IEnumerator GenerateGrid(WorldSaveData data) {
            this.width = data.width;
            this.height = data.height;
            float newX = xOffset * (width / 2);
            float newY = yOffset * (height / 2);
            this.transform.localPosition = new Vector2(-newX, -newY);
            map = new HexTile[(int)width, (int)height];
            hexTiles = new List<HexTile>();
            int totalTiles = width * height;
            int id = 0;
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    float xPosition = x * xOffset;

                    float yPosition = y * yOffset;
                    if (y % 2 == 1) {
                        xPosition += xOffset / 2;
                    }

                    GameObject hex = GameObject.Instantiate(goHex) as GameObject;
                    hex.transform.SetParent(this.transform);
                    hex.transform.localPosition = new Vector3(xPosition, yPosition, 0f);
                    hex.transform.localScale = new Vector3(tileSize, tileSize, 0f);
                    hex.name = x + "," + y;
                    HexTile currHex = hex.GetComponent<HexTile>();
                    hexTiles.Add(currHex);
                    currHex.Initialize();
                    currHex.SetData(data.GetTileData(id));
                    map[x, y] = currHex;
                    id++;
                    WorldCreatorUI.Instance.UpdateLoading((float)hexTiles.Count / (float)totalTiles, "Loading tile " + id + "/" + totalTiles.ToString());
                    yield return null;
                }
            }
            hexTiles.ForEach(o => o.FindNeighbours(map));

            WorldCreatorUI.Instance.InitializeMenus();
            //CombatManager.Instance.Initialize();
            FactionManager.Instance.LoadFactions(data);
            LandmarkManager.Instance.LoadAreas(data);
            LandmarkManager.Instance.LoadLandmarks(data);
            GenerateOuterGrid(data);
            CharacterManager.Instance.LoadCharacters(data);
            CharacterManager.Instance.LoadRelationships(data);
            //MonsterManager.Instance.LoadMonsters(data);
            TokenManager.Instance.Initialize();
            //CharacterManager.Instance.LoadSquads(data);
            //LandmarkManager.Instance.LoadDefenders(data);
            //CharacterManager.Instance.LoadCharactersInfo(data);
            Biomes.Instance.UpdateTileVisuals(allTiles);
            //PathfindingManager.Instance.LoadSettings(data.pathfindingSettings);

            WorldCreatorUI.Instance.OnDoneLoadingGrid();
        }
        internal void GenerateOuterGrid() {
            int newWidth = (int)width + (_borderThickness * 2);
            int newHeight = (int)height + (_borderThickness * 2);

            float newX = xOffset * (int)(newWidth / 2);
            float newY = yOffset * (int)(newHeight / 2);

            outerGridList = new List<HexTile>();

            int id = 0;

            _borderParent.transform.localPosition = new Vector2(-newX, -newY);
            for (int x = 0; x < newWidth; x++) {
                for (int y = 0; y < newHeight; y++) {
                    if ((x >= _borderThickness && x < newWidth - _borderThickness) && (y >= _borderThickness && y < newHeight - _borderThickness)) {
                        continue;
                    }
                    float xPosition = x * xOffset;

                    float yPosition = y * yOffset;
                    if (y % 2 == 1) {
                        xPosition += xOffset / 2;
                    }

                    GameObject hex = GameObject.Instantiate(goHex) as GameObject;
                    hex.transform.SetParent(_borderParent.transform);
                    hex.transform.localPosition = new Vector3(xPosition, yPosition, 0f);
                    hex.transform.localScale = new Vector3(tileSize, tileSize, 0f);
                    HexTile currHex = hex.GetComponent<HexTile>();
                    currHex.Initialize();
                    currHex.data.id = id;
                    currHex.data.tileName = hex.name;
                    currHex.data.xCoordinate = x - _borderThickness;
                    currHex.data.yCoordinate = y - _borderThickness;
                    outerGridList.Add(currHex);

                    int xToCopy = x - _borderThickness;
                    int yToCopy = y - _borderThickness;
                    if (x < _borderThickness && y - _borderThickness >= 0 && y < height) { //if border thickness is 2 (0 and 1)
                        //left border
                        xToCopy = 0;
                        yToCopy = y - _borderThickness;
                    } else if (x >= _borderThickness && x <= width && y < _borderThickness) {
                        //bottom border
                        xToCopy = x - _borderThickness;
                        yToCopy = 0;
                    } else if (x > width && (y - _borderThickness >= 0 && y - _borderThickness <= height - 1)) {
                        //right border
                        xToCopy = (int)width - 1;
                        yToCopy = y - _borderThickness;
                    } else if (x >= _borderThickness && x <= width && y - _borderThickness >= height) {
                        //top border
                        xToCopy = x - _borderThickness;
                        yToCopy = (int)height - 1;
                    } else {
                        //corners
                        xToCopy = x;
                        yToCopy = y;
                        xToCopy = Mathf.Clamp(xToCopy, 0, (int)width - 1);
                        yToCopy = Mathf.Clamp(yToCopy, 0, (int)height - 1);
                    }

                    HexTile hexToCopy = map[xToCopy, yToCopy];

                    currHex.name = currHex.xCoordinate + "," + currHex.yCoordinate + "(Border) Copied from " + hexToCopy.name;

                    currHex.SetElevation(hexToCopy.elevationType);
                    Biomes.Instance.SetBiomeForTile(hexToCopy.biomeType, currHex);
                    //Biomes.Instance.GenerateTileBiomeDetails(currHex);
                    //Biomes.Instance.UpdateTileVisuals(currHex);
                    Biomes.Instance.UpdateTileVisuals(currHex);


                    //currHex.DisableColliders();
                    //currHex.unpassableGO.GetComponent<PolygonCollider2D>().enabled = true;
                    //currHex.HideFogOfWarObjects();
                    id++;
                }
            }
            allTiles = GetAllTiles();
            outerGridList.ForEach(o => o.GetComponent<HexTile>().FindNeighboursForBorders());
        }
        internal void GenerateOuterGrid(WorldSaveData data) {
            if (data.outerGridTilesData == null) {
                GenerateOuterGrid(); //generate default outer grid
                return;
            }
            _borderThickness = data.borderThickness;
            int newWidth = (int)width + (_borderThickness * 2);
            int newHeight = (int)height + (_borderThickness * 2);

            float newX = xOffset * (int)(newWidth / 2);
            float newY = yOffset * (int)(newHeight / 2);

            outerGridList = new List<HexTile>();
            int id = 0;
            _borderParent.transform.localPosition = new Vector2(-newX, -newY);
            for (int x = 0; x < newWidth; x++) {
                for (int y = 0; y < newHeight; y++) {
                    if ((x >= _borderThickness && x < newWidth - _borderThickness) && (y >= _borderThickness && y < newHeight - _borderThickness)) {
                        continue;
                    }
                    float xPosition = x * xOffset;

                    float yPosition = y * yOffset;
                    if (y % 2 == 1) {
                        xPosition += xOffset / 2;
                    }

                    GameObject hex = GameObject.Instantiate(goHex) as GameObject;
                    hex.transform.SetParent(_borderParent.transform);
                    hex.transform.localPosition = new Vector3(xPosition, yPosition, 0f);
                    hex.transform.localScale = new Vector3(tileSize, tileSize, 0f);
                    HexTile currHex = hex.GetComponent<HexTile>();
                    currHex.Initialize();
                    HexTileData existingData = data.GetOuterTileData(id);
                    if (existingData != null) {
                        currHex.data = existingData;
                    } else {
                        currHex.data.id = id;
                        currHex.data.tileName = hex.name;
                    }
                    currHex.data.xCoordinate = x - _borderThickness;
                    currHex.data.yCoordinate = y - _borderThickness;

                    currHex.name = currHex.xCoordinate + "," + currHex.yCoordinate;

                    outerGridList.Add(currHex);
                    id++;
                }
            }
            allTiles = GetAllTiles();
            Biomes.Instance.UpdateTileVisuals(outerGridList);
            outerGridList.ForEach(o => o.GetComponent<HexTile>().FindNeighboursForBorders());
        }
        private bool IsCoordinatePartOfMainMap(int x, int y) {
            try {
                HexTile tile = map[x, y];
                if (tile != null) {
                    return true;
                }
                return false;
            }catch(IndexOutOfRangeException) {
                return false;
            }
        }
        private List<HexTile> GetAllTiles() {
            List<HexTile> allTiles = new List<HexTile>(hexTiles);
            allTiles.AddRange(outerGridList);
            return allTiles;
        }
        public HexTile GetTileFromCoordinates(int x, int y) {
            if ((x < 0 || x > width - 1) || (y < 0 || y > height - 1)) {
                //outer tile
                return GetBorderTile(x, y);
            } else {
                return map[x, y];
            }
        }
        private HexTile GetBorderTile(int x, int y) {
            for (int i = 0; i < outerGridList.Count; i++) {
                HexTile currTile = outerGridList[i];
                if (currTile.xCoordinate == x && currTile.yCoordinate == y) {
                    return currTile;
                }
            }
            return null;
        }
        public void SetBorderTilesVisualState(bool state) {
            _borderParent.gameObject.SetActive(state);
        }
        #endregion

        #region Map Editing
        public void EnableSelection() {
            selectionComponent.enabled = true;
        }
        public void SetEditMode(EDIT_MODE editMode) {
            currentMode = editMode;
            //selectionComponent.ClearSelectedTiles();
        }
        public void SetSelectionMode(SELECTION_MODE selectionMode) {
            this.selectionMode = selectionMode;
            //selectionComponent.ClearSelectedTiles();
        }
        #endregion

        #region Biome Edit
        public void SetBiomes(List<HexTile> tiles, BIOMES biome) {
            for (int i = 0; i < tiles.Count; i++) {
                HexTile currTile = tiles[i];
                SetBiomes(currTile, biome, false);
            }
            for (int i = 0; i < tiles.Count; i++) {
                HexTile currTile = tiles[i];
                Biomes.Instance.UpdateTileVisuals(currTile);
                //Biomes.Instance.GenerateTileBiomeDetails(currTile);
            }
        }
        public void SetBiomes(HexTile tile, BIOMES biome, bool updateVisuals = true) {
            tile.SetBiome(biome);
            if (updateVisuals) {
                Biomes.Instance.UpdateTileVisuals(tile);
            }
        }
        #endregion

        #region Elevation Edit
        public void SetElevation(List<HexTile> tiles, ELEVATION elevation) {
            for (int i = 0; i < tiles.Count; i++) {
                HexTile currTile = tiles[i];
                SetElevation(currTile, elevation, false);
            }
            for (int i = 0; i < tiles.Count; i++) {
                HexTile currTile = tiles[i];
                Biomes.Instance.UpdateTileVisuals(currTile);
                //if (currTile.AllNeighbours != null) {
                    for (int j = 0; j < currTile.AllNeighbours.Count; j++) {
                        HexTile currNeighbour = currTile.AllNeighbours[j];
                        Biomes.Instance.UpdateTileVisuals(currNeighbour);
                    }
                //}
            }
        }
        public void SetElevation(HexTile tile, ELEVATION elevation, bool updateVisuals = true) {
            if (elevation != ELEVATION.PLAIN) {
                if (tile.areaOfTile != null) {
                    if (tile.areaOfTile.coreTile.id == tile.id) {
                        WorldCreatorUI.Instance.messageBox.ShowMessageBox(MESSAGE_BOX.OK, "Elevation error", "Cannot change elevation of " + tile.tileName + " because it is a core tile of an area!");
                        return;
                    }
                    //tile.areaOfTile.RemoveTile(tile);
                }
                if (tile.landmarkOnTile != null) {
                    LandmarkManager.Instance.DestroyLandmarkOnTile(tile);
                }
            }
            tile.SetElevation(elevation);
            if (updateVisuals) {
                Biomes.Instance.UpdateTileVisuals(tile);
                for (int i = 0; i < tile.AllNeighbours.Count; i++) {
                    HexTile currNeighbour = tile.AllNeighbours[i];
                    Biomes.Instance.UpdateTileVisuals(currNeighbour);
                }
            }
        }
        #endregion

        #region Landmark Edit
        public List<BaseLandmark> SpawnLandmark(List<HexTile> tiles, LANDMARK_TYPE landmarkType) {
            List<BaseLandmark> landmarks = new List<BaseLandmark>();
            for (int i = 0; i < tiles.Count; i++) {
                HexTile hexTile = tiles[i];
                BaseLandmark spawnedLandmark = SpawnLandmark(hexTile, landmarkType);
                if (spawnedLandmark != null) {
                    landmarks.Add(spawnedLandmark);
                }
            }
            return landmarks;
        }
        public BaseLandmark SpawnLandmark(HexTile tile, LANDMARK_TYPE landmarkType) {
            if (outerGridList.Contains(tile)) {
                return null;
            }
            LandmarkData data = LandmarkManager.Instance.GetLandmarkData(landmarkType);
            return LandmarkManager.Instance.CreateNewLandmarkOnTile(tile, landmarkType);
        }
        public void DestroyLandmarks(List<HexTile> tiles) {
            for (int i = 0; i < tiles.Count; i++) {
                DestroyLandmarks(tiles[i]);
            }
        }
        public void DestroyLandmarks(HexTile tile) {
            LandmarkManager.Instance.DestroyLandmarkOnTile(tile);
        }
        #endregion

        #region Faction Edit
        public void CreateNewFaction() {
            Faction createdFaction = FactionManager.Instance.CreateNewFaction();
            WorldCreatorUI.Instance.editFactionsMenu.OnFactionCreated(createdFaction);
            WorldCreatorUI.Instance.editCharactersMenu.characterInfoEditor.LoadFactionDropdownOptions();
        }
        public void DeleteFaction(Faction faction) {
            FactionManager.Instance.DeleteFaction(faction);
            WorldCreatorUI.Instance.editFactionsMenu.OnFactionDeleted(faction);
        }
        #endregion

        #region Saving
        public void SaveWorld(string saveName) {
            WorldSaveData worldData = new WorldSaveData(width, height, _borderThickness);
            worldData.OccupyTileData(hexTiles);
            worldData.OccupyOuterTileData(outerGridList);
            worldData.OccupyFactionData(FactionManager.Instance.allFactions);
            worldData.OccupyLandmarksData(LandmarkManager.Instance.GetAllLandmarks());
            worldData.OccupyCharactersData(CharacterManager.Instance.allCharacters);
            worldData.OccupyAreaData(LandmarkManager.Instance.allAreas);    
            //worldData.OccupySquadData(CharacterManager.Instance.allSquads);
            //worldData.OccupyMonstersData(MonsterManager.Instance.allMonsterParties);
            worldData.OccupyPathfindingSettings(map, width, height);
            if (!saveName.Contains(Utilities.worldConfigFileExt)) {
                saveName += Utilities.worldConfigFileExt;
            }
            SaveGame.Save<WorldSaveData>(Utilities.worldConfigsSavePath + saveName, worldData);
            StartCoroutine(CaptureScreenshot(saveName));
            //PathfindingManager.Instance.ClearGraphs();
            WorldCreatorUI.Instance.OnFileSaved(saveName);
        }
        IEnumerator CaptureScreenshot(string fileName) {
            CameraMove.Instance.uiCamera.gameObject.SetActive(false);
            fileName = fileName.Replace(Utilities.worldConfigFileExt, "");
            yield return new WaitForEndOfFrame();

            string path = Application.persistentDataPath + "/Saves/"
                    + fileName + ".png";

            Texture2D screenImage = new Texture2D(Screen.width, Screen.height);
            //Get Image from screen
            screenImage.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            screenImage.Apply();
            //Convert to png
            byte[] imageBytes = screenImage.EncodeToPNG();

            //Save image to file
            System.IO.File.WriteAllBytes(path, imageBytes);
            CameraMove.Instance.uiCamera.gameObject.SetActive(true);
        }
        public void LoadWorld(string saveName) {
            WorldSaveData data = GetWorldData(saveName);
            LoadWorld(data);
        }
        public void LoadWorld(WorldSaveData data) {
            StartCoroutine(GenerateGrid(data));
        }
        public WorldSaveData GetWorldData(string saveName) {
            //FileInfo saveFile = GetSaveFile(saveName);
            WorldSaveData saveData = SaveGame.Load<WorldSaveData>(Utilities.worldConfigsSavePath + saveName);
            Utilities.ValidateSaveData(saveData);
            return saveData;
        }
        public FileInfo GetSaveFile(string saveName) {
            Directory.CreateDirectory(Utilities.worldConfigsSavePath);
            DirectoryInfo info = new DirectoryInfo(Utilities.worldConfigsSavePath);
            FileInfo[] files = info.GetFiles();
            for (int i = 0; i < files.Length; i++) {
                FileInfo fileInfo = files[i];
                if (fileInfo.Name.Equals(saveName)) {
                    return fileInfo;
                }
            }
            return null;
        }
        #endregion

        #region Areas
        private void HighlightAreas() {
            for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
                Area currArea = LandmarkManager.Instance.allAreas[i];
                currArea.HighlightArea();
            }
        }
        private void UnhighlightAreas() {
            for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
                Area currArea = LandmarkManager.Instance.allAreas[i];
                currArea.UnhighlightArea();
            }
        }
        #endregion

        #region Tile Data
        public void SetManaOnTiles(string amount) {
            int value = Int32.Parse(amount);
            for (int i = 0; i < selectionComponent.selection.Count; i++) {
                HexTile currTile = selectionComponent.selection[i];
                if (currTile.elevationType == ELEVATION.PLAIN) {
                }
            }
        }
        #endregion
    }

    public enum EDIT_MODE {
        BIOME,
        ELEVATION,
        FACTION,
        REGION,
        LANDMARKS,
    }
    public enum SELECTION_MODE {
        RECTANGLE,
        TILE,
    }
}