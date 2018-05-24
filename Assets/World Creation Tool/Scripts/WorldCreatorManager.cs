using System;
using System.Collections;
using System.Collections.Generic;
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

        public List<Region> allRegions { get; private set; }

        private void Awake() {
            Instance = this;
            allRegions = new List<Region>();
        }

        #region Grid Generation
        public IEnumerator GenerateGrid(int width, int height) {
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
                    currHex.id = id;
                    //currHex.tileName = RandomNameGenerator.Instance.GetTileName();
                    currHex.xCoordinate = x;
                    currHex.yCoordinate = y;
                    currHex.Initialize();
                    //listHexes.Add(hex);
                    map[x, y] = currHex;
                    id++;
                    WorldCreatorUI.Instance.UpdateLoading((float)hexTiles.Count / (float)totalTiles, "Generating tile " + id + "/" + totalTiles.ToString());
                    yield return null;
                }
            }
            hexTiles.ForEach(o => o.FindNeighbours(map));
            Biomes.Instance.UpdateTileVisuals(hexTiles);
            Biomes.Instance.GenerateTileBiomeDetails(hexTiles);
            Biomes.Instance.LoadPassableObjects(hexTiles);
            CreateNewRegion(hexTiles);
            //mapWidth = listHexes[listHexes.Count - 1].transform.position.x;
            //mapHeight = listHexes[listHexes.Count - 1].transform.position.y;
            WorldCreatorUI.Instance.OnDoneLoadingGrid();
        }
        #endregion

        #region Map Editing
        public void EnableSelection() {
            selectionComponent.enabled = true;
        }
        public void SetEditMode(EDIT_MODE editMode) {
            currentMode = editMode;
        }
        public void SetSelectionMode(SELECTION_MODE selectionMode) {
            this.selectionMode = selectionMode;
        }
        #endregion

        #region Region Editing
        public Region GetBiggestRegion(Region except) {
            return allRegions.Where(x => x.id != except.id).OrderByDescending(x => x.tilesInRegion.Count).First();
        }
        public void CreateNewRegion(List<HexTile> tiles) {
            List<Region> emptyRegions = new List<Region>();

            for (int i = 0; i < tiles.Count; i++) {
                HexTile currTile = tiles[i];
                if (currTile.region != null) {
                    Region regionOfTile = currTile.region;
                    regionOfTile.tilesInRegion.Remove(currTile);
                    if (regionOfTile.tilesInRegion.Count == 0) {
                        if (!emptyRegions.Contains(regionOfTile)) {
                            emptyRegions.Add(regionOfTile);
                        }
                    }

                    currTile.SetRegion(null);
                }
            }
            
            HexTile center = Utilities.GetCenterTile(tiles, map, width, height);
            Region newRegion = new Region(center);
            newRegion.AddTile(tiles);
            allRegions.Add(newRegion);
            for (int i = 0; i < emptyRegions.Count; i++) {
                Region currEmptyRegion = emptyRegions[i];
                DeleteRegion(currEmptyRegion);
            }

            for (int i = 0; i < allRegions.Count; i++) {
                Region currRegion = allRegions[i];
                currRegion.UpdateAdjacency();
            }
            WorldCreatorUI.Instance.editRegionsMenu.OnRegionCreated(newRegion);
        }
        public void DeleteRegion(Region regionToDelete) {
            for (int i = 0; i < regionToDelete.regionBorderLines.Count; i++) {
                SpriteRenderer currBorderLine = regionToDelete.regionBorderLines[i];
                currBorderLine.gameObject.SetActive(false);
            }
            //Give tiles from region to delete to another region
            Region regionToGiveTo = GetBiggestRegion(regionToDelete);
            regionToGiveTo.AddTile(regionToDelete.tilesInRegion);
            regionToDelete.UnhighlightRegion();
            allRegions.Remove(regionToDelete);
            for (int i = 0; i < allRegions.Count; i++) {
                allRegions[i].UpdateAdjacency();
            }
            WorldCreatorUI.Instance.editRegionsMenu.OnRegionDeleted(regionToDelete);
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
        TILE
    }
}

