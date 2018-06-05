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
        public HexTile[,] outerGrid;
        [SerializeField] private Transform _borderParent;
        public int _borderThickness;

        public List<Region> allRegions { get; private set; }
        
        private void Awake() {
            Instance = this;
            allRegions = new List<Region>();
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
                    hex.transform.parent = this.transform;
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
            //CameraMove.Instance.SetWholemapCameraValues();
            Biomes.Instance.UpdateTileVisuals(hexTiles);
            Biomes.Instance.GenerateTileBiomeDetails(hexTiles);
            Biomes.Instance.LoadPassableObjects(hexTiles);
            CreateNewRegion(hexTiles);
            GenerateOuterGrid();
            //mapWidth = listHexes[listHexes.Count - 1].transform.position.x;
            //mapHeight = listHexes[listHexes.Count - 1].transform.position.y;
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
                    hex.transform.parent = this.transform;
                    hex.transform.localPosition = new Vector3(xPosition, yPosition, 0f);
                    hex.transform.localScale = new Vector3(tileSize, tileSize, 0f);
                    hex.name = x + "," + y;
                    HexTile currHex = hex.GetComponent<HexTile>();
                    hexTiles.Add(currHex);
                    currHex.Initialize();
                    currHex.data = data.GetTileData(id);
                    map[x, y] = currHex;
                    id++;
                    WorldCreatorUI.Instance.UpdateLoading((float)hexTiles.Count / (float)totalTiles, "Loading tile " + id + "/" + totalTiles.ToString());
                    yield return null;
                }
            }
            hexTiles.ForEach(o => o.FindNeighbours(map));
            Biomes.Instance.UpdateTileVisuals(hexTiles);
            Biomes.Instance.GenerateTileBiomeDetails(hexTiles);
            Biomes.Instance.LoadPassableObjects(hexTiles);

            LoadRegions(data);
            LoadFactions(data);
            LoadLandmarks(data);
            OccupyRegions(data);
            GenerateOuterGrid();
            //PathfindingManager.Instance.LoadSettings(data.pathfindingSettings);

            WorldCreatorUI.Instance.OnDoneLoadingGrid();
        }
        internal void GenerateOuterGrid() {
            int newWidth = (int)width + (_borderThickness * 2);
            int newHeight = (int)height + (_borderThickness * 2);

            float newX = xOffset * (int)(newWidth / 2);
            float newY = yOffset * (int)(newHeight / 2);

            outerGridList = new List<HexTile>();
            outerGrid = new HexTile[newWidth, newHeight];
            //if((int)width % 2 != 0) {
            //    //odd
            //    newX += xOffset / 2f;
            //}
            //if ((int)height % 2 != 0) {
            //    //odd
            //    newY += yOffset / 2f;
            //}
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
                    hex.transform.parent = _borderParent.transform;
                    hex.transform.localPosition = new Vector3(xPosition, yPosition, 0f);
                    hex.transform.localScale = new Vector3(tileSize, tileSize, 0f);
                    HexTile currHex = hex.GetComponent<HexTile>();
                    currHex.Initialize();
                    currHex.data.tileName = hex.name;
                    currHex.data.xCoordinate = x;
                    currHex.data.yCoordinate = y;

                    outerGrid[x, y] = currHex;
                    outerGridList.Add(currHex);

                    //int xToCopy = Mathf.Clamp(x, 0, (int)width - 1);
                    //int yToCopy = Mathf.Clamp(y, 0, (int)height - 1);

                    //int xToCopy = Mathf.Max(x - (_borderThickness * 2), 0);
                    //int yToCopy = Mathf.Max(y - (_borderThickness * 2), 0);
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

                    hex.name = x + "," + y + "(Border) Copied from " + hexToCopy.name;

                    currHex.SetElevation(hexToCopy.elevationType);
                    Biomes.Instance.SetBiomeForTile(hexToCopy.biomeType, currHex);
                    Biomes.Instance.AddBiomeDetailToTile(currHex);
                    Biomes.Instance.SetElevationSpriteForTile(currHex);
                    hexToCopy.region.AddOuterGridTile(currHex);


                    currHex.DisableColliders();
                    currHex.unpassableGO.GetComponent<PolygonCollider2D>().enabled = true;
                    currHex.unpassableGO.SetActive(true);
                    //currHex.HideFogOfWarObjects();
                }
            }

            outerGridList.ForEach(o => o.GetComponent<HexTile>().FindNeighbours(outerGrid, true));
        }
        private void LoadRegions(WorldSaveData data) {
            for (int i = 0; i < data.regionsData.Count; i++) {
                RegionSaveData currData = data.regionsData[i];
                HexTile centerTile = null;
                List<HexTile> regionTiles = GetRegionTiles(currData, ref centerTile);
                CreateNewRegion(regionTiles, centerTile, currData);
            }
            for (int i = 0; i < allRegions.Count; i++) {
                Region currRegion = allRegions[i];
                currRegion.UpdateAdjacency();
            }
        }
        private void LoadLandmarks(WorldSaveData data) {
            if (data.landmarksData != null) {
                for (int i = 0; i < data.landmarksData.Count; i++) {
                    LandmarkSaveData landmarkData = data.landmarksData[i];
                    LandmarkManager.Instance.CreateNewLandmarkOnTile(landmarkData);
                }
            }
        }
        private void LoadFactions(WorldSaveData data) {
            if (data.factionsData != null) {
                for (int i = 0; i < data.factionsData.Count; i++) {
                    FactionSaveData currData = data.factionsData[i];
                    Faction currFaction = FactionManager.Instance.CreateNewFaction(currData);
                    WorldCreatorUI.Instance.editFactionsMenu.OnFactionCreated(currFaction);
                }
            }
        }
        private void OccupyRegions(WorldSaveData data) {
            for (int i = 0; i < allRegions.Count; i++) {
                Region currRegion = allRegions[i];
                RegionSaveData regionData = data.GetRegionData(currRegion.id);
                if (regionData.owner != -1) {
                    Faction owner = FactionManager.Instance.GetFactionBasedOnID(regionData.owner);
                    owner.OwnRegion(currRegion);
                    currRegion.SetOwner(owner);
                    currRegion.ReColorBorderTiles(owner.factionColor);
                }
            }
            WorldCreatorUI.Instance.editFactionsMenu.UpdateItems();
        }
        private List<HexTile> GetRegionTiles(RegionSaveData regionData, ref HexTile centerTile) {
            List<int> tileIDs = new List<int>(regionData.tileData);
            List<HexTile> regionTiles = new List<HexTile>();
            for (int i = 0; i < hexTiles.Count; i++) {
                HexTile currTile = hexTiles[i];
                if (tileIDs.Contains(currTile.id)) {
                    regionTiles.Add(currTile);
                    tileIDs.Remove(currTile.id);
                    if (currTile.id == regionData.centerTileID) {
                        centerTile = currTile;
                    }
                    if (tileIDs.Count == 0) {
                        break;
                    }
                }
            }
            return regionTiles;
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

        #region Region Editing
        //public Region GetBiggestRegion(Region except) {
        //    return allRegions.Where(x => x.id != except.id).OrderByDescending(x => x.tilesInRegion.Count).First();
        //}
        public Region CreateNewRegion(List<HexTile> tiles) {
            List<Region> affectedRegions = new List<Region>();
            for (int i = 0; i < tiles.Count; i++) {
                HexTile currTile = tiles[i];
                if (currTile.region != null) {
                    Region regionOfTile = currTile.region;
                    regionOfTile.RemoveTile(currTile);
                    currTile.SetRegion(null);
                    if (!affectedRegions.Contains(regionOfTile)) {
                        affectedRegions.Add(regionOfTile);
                    }
                }
            }

            HexTile center = Utilities.GetCenterTile(tiles, map, width, height);
            Region newRegion = new Region(center, tiles);
            newRegion.AddTile(tiles);
            allRegions.Add(newRegion);

            //Re compute the center of masses of the regions that were affected
            for (int i = 0; i < affectedRegions.Count; i++) {
                Region currRegion = affectedRegions[i];
                currRegion.ReComputeCenterOfMass();
            }
            for (int i = 0; i < allRegions.Count; i++) {
                Region currRegion = allRegions[i];
                currRegion.UpdateAdjacency();
            }
            WorldCreatorUI.Instance.editRegionsMenu.OnRegionCreated(newRegion);
            return newRegion;
        }
        public Region CreateNewRegion(List<HexTile> tiles, HexTile centerTile, RegionSaveData data) {
            Region newRegion = new Region(centerTile, tiles, data);
            newRegion.AddTile(tiles);
            allRegions.Add(newRegion);

            WorldCreatorUI.Instance.editRegionsMenu.OnRegionCreated(newRegion);
            return newRegion;
        }
        public Region CreateNewRegion(List<HexTile> tiles, ref List<Region> affectedRegions) {
            List<Region> emptyRegions = new List<Region>();
            for (int i = 0; i < tiles.Count; i++) {
                HexTile currTile = tiles[i];
                if (currTile.region != null) {
                    Region regionOfTile = currTile.region;
                    regionOfTile.RemoveTile(currTile);
                    if (regionOfTile.tilesInRegion.Count == 0) {
                        if (!emptyRegions.Contains(regionOfTile)) {
                            emptyRegions.Add(regionOfTile);
                        }
                    }
                    currTile.SetRegion(null);
                    if (!affectedRegions.Contains(regionOfTile)) {
                        affectedRegions.Add(regionOfTile);
                    }
                }
            }
            
            HexTile center = Utilities.GetCenterTile(tiles, map, width, height);
            Region newRegion = new Region(center);
            newRegion.AddTile(tiles);
            allRegions.Add(newRegion);

            //delete empty regions
            for (int i = 0; i < emptyRegions.Count; i++) {
                Region currEmptyRegion = emptyRegions[i];
                DeleteRegion(currEmptyRegion);
            }

            //Re compute the center of masses of the regions that were affected
            for (int i = 0; i < affectedRegions.Count; i++) {
                Region currRegion = affectedRegions[i];
                if (!emptyRegions.Contains(currRegion)) {
                    currRegion.ReComputeCenterOfMass();
                }
            }
            for (int i = 0; i < allRegions.Count; i++) {
                Region currRegion = allRegions[i];
                currRegion.UpdateAdjacency();
            }
            WorldCreatorUI.Instance.editRegionsMenu.OnRegionCreated(newRegion);
            return newRegion;
        }
        public void AddTilesToRegion(List<HexTile> tiles, Region region) {
            List<Region> emptyRegions = new List<Region>();
            List<Region> affectedRegions = new List<Region>();
            for (int i = 0; i < tiles.Count; i++) {
                HexTile currTile = tiles[i];
                if (currTile.region != null && currTile.region.id != region.id) {
                    Region regionOfTile = currTile.region;
                    regionOfTile.RemoveTile(currTile);
                    if (regionOfTile.tilesInRegion.Count == 0) {
                        if (!emptyRegions.Contains(regionOfTile)) {
                            emptyRegions.Add(regionOfTile);
                        }
                    }
                    if (!affectedRegions.Contains(regionOfTile)) {
                        affectedRegions.Add(regionOfTile);
                    }
                    region.AddTile(currTile);
                }
            }

            //delete empty regions
            for (int i = 0; i < emptyRegions.Count; i++) {
                Region currEmptyRegion = emptyRegions[i];
                DeleteRegion(currEmptyRegion);
            }

            //Re compute the center of masses of the regions that were affected
            for (int i = 0; i < affectedRegions.Count; i++) {
                Region currRegion = affectedRegions[i];
                if (!emptyRegions.Contains(currRegion)) {
                    currRegion.ReComputeCenterOfMass();
                }
            }
            for (int i = 0; i < allRegions.Count; i++) {
                Region currRegion = allRegions[i];
                currRegion.UpdateAdjacency();
            }
            WorldCreatorUI.Instance.editRegionsMenu.OnRegionEdited();
        }
        public void ValidateRegions(List<Region> regions) {
            for (int i = 0; i < regions.Count; i++) {
                Region currRegion = regions[i];
                //StartCoroutine(currRegion.GetIslands());
                List<RegionIsland> islands = currRegion.GetIslands();
                if (islands.Count > 1) {
                    for (int j = 1; j < islands.Count; j++) {
                        RegionIsland currIsland = islands[j];
                        currRegion.RemoveTile(currIsland.tilesInIsland);
                        Region newRegion = CreateNewRegion(currIsland.tilesInIsland);
                    }
                }
            }
        }
        public void DeleteRegion(Region regionToDelete) {
            for (int i = 0; i < regionToDelete.regionBorderLines.Count; i++) {
                SpriteRenderer currBorderLine = regionToDelete.regionBorderLines[i];
                currBorderLine.gameObject.SetActive(false);
            }
            if (regionToDelete.owner != null) {
                regionToDelete.owner.UnownRegion(regionToDelete);
            }

            //Give tiles from region to delete to another region
            if (regionToDelete.adjacentRegions.Count > 0) {
                Region regionToGiveTo = regionToDelete.adjacentRegions[UnityEngine.Random.Range(0, regionToDelete.adjacentRegions.Count)];
                regionToGiveTo.AddTile(regionToDelete.tilesInRegion);
            }
            regionToDelete.UnhighlightRegion();
            allRegions.Remove(regionToDelete);
            for (int i = 0; i < allRegions.Count; i++) {
                allRegions[i].UpdateAdjacency();
            }
            WorldCreatorUI.Instance.OnRegionDeleted(regionToDelete);
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
                Biomes.Instance.UpdateTileVisuals(currTile, true);
                Biomes.Instance.GenerateTileBiomeDetails(currTile);
                Biomes.Instance.LoadPassableObjects(currTile);
            }
            
        }
        public void SetBiomes(HexTile tile, BIOMES biome, bool updateVisuals = true) {
            tile.SetBiome(biome);
            if (updateVisuals) {
                Biomes.Instance.UpdateTileVisuals(tile, true);
                Biomes.Instance.GenerateTileBiomeDetails(tile);
                Biomes.Instance.LoadPassableObjects(tile);
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
                Biomes.Instance.UpdateTileVisuals(currTile, true);
                Biomes.Instance.GenerateTileBiomeDetails(currTile);
                Biomes.Instance.LoadPassableObjects(currTile);
            }
        }
        public void SetElevation(HexTile tile, ELEVATION elevation, bool updateVisuals = true) {
            tile.SetElevation(elevation);
            if (updateVisuals) {
                Biomes.Instance.UpdateTileVisuals(tile, true);
                Biomes.Instance.GenerateTileBiomeDetails(tile);
                Biomes.Instance.LoadPassableObjects(tile);
            }
        }
        #endregion

        #region Landmark Edit
        public void SpawnLandmark(List<HexTile> tiles, LANDMARK_TYPE landmarkType) {
            for (int i = 0; i < tiles.Count; i++) {
                SpawnLandmark(tiles[i], landmarkType);
            }
        }
        public void SpawnLandmark(HexTile tile, LANDMARK_TYPE landmarkType) {
            LandmarkManager.Instance.CreateNewLandmarkOnTile(tile, landmarkType);
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
        public void CreateNewFaction(RACE race) {
            Faction createdFaction = FactionManager.Instance.CreateNewFaction(race);
            WorldCreatorUI.Instance.editFactionsMenu.OnFactionCreated(createdFaction);
        }
        public void DeleteFaction(Faction faction) {
            FactionManager.Instance.DeleteFaction(faction);
            WorldCreatorUI.Instance.editFactionsMenu.OnFactionDeleted(faction);
        }
        #endregion

        #region Saving
        public void SaveWorld(string saveName) {
            WorldSaveData worldData = new WorldSaveData(width, height);
            worldData.OccupyTileData(hexTiles);
            worldData.OccupyRegionData(allRegions);
            worldData.OccupyFactionData(FactionManager.Instance.allFactions);
            worldData.OccupyLandmarksData(LandmarkManager.Instance.GetAllLandmarks());
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
            FileInfo saveFile = GetSaveFile(saveName);
            return SaveGame.Load<WorldSaveData>(Utilities.worldConfigsSavePath + saveName);
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
    }

    public enum EDIT_MODE {
        BIOME,
        ELEVATION,
        FACTION,
        REGION,
        LANDMARKS
    }
    public enum SELECTION_MODE {
        RECTANGLE,
        TILE,
        REGION
    }
}