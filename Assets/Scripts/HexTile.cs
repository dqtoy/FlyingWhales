using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathFind;
using System.Linq;

using worldcreator;
using SpriteGlow;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HexTile : MonoBehaviour, IHasNeighbours<HexTile>, ILocation {

    public HexTileData data;
    private Area _areaOfTile;
    public SpriteRenderer spriteRenderer;

    [Space(10)]
    [Header("Booleans")]
    public bool isOccupied = false;
    private bool _isCorrupted = false;
    private bool _isExternal = false;
    private bool _isInternal = false;

    [Space(10)]
    [Header("Tile Visuals")]
    [SerializeField] private GameObject _centerPiece;
    [SerializeField] private GameObject _highlightGO;
    [SerializeField] private GameObject _hoverHighlightGO;
    [SerializeField] private GameObject _clickHighlightGO;
    [SerializeField] private Animator baseTileAnimator;

    [Space(10)]
    [Header("Tile Borders")]
    [SerializeField] private SpriteRenderer topLeftBorder;
    [SerializeField] private SpriteRenderer leftBorder;
    [SerializeField] private SpriteRenderer botLeftBorder;
    [SerializeField] private SpriteRenderer botRightBorder;
    [SerializeField] private SpriteRenderer rightBorder;
    [SerializeField] private SpriteRenderer topRightBorder;

    //[SerializeField] private SpriteGlowEffect topLeftBorderSGE;
    //[SerializeField] private SpriteGlowEffect leftBorderSGE;
    //[SerializeField] private SpriteGlowEffect botLeftBorderSGE;
    //[SerializeField] private SpriteGlowEffect botRightBorderSGE;
    //[SerializeField] private SpriteGlowEffect rightBorderSGE;
    //[SerializeField] private SpriteGlowEffect topRightBorderSGE;

    [Space(10)]
    [Header("Structure Objects")]
    [SerializeField] private GameObject structureParentGO;
    [SerializeField] private SpriteRenderer mainStructure;
    [SerializeField] private SpriteRenderer structureTint;
    [SerializeField] private Animator structureAnimation;

    [Space(10)]
    [Header("Biome Details")]
    [SerializeField] private Transform biomeDetailsParent;

    [Space(10)]
    [Header("Corruption")]
    [SerializeField] private GameObject[] defaultCorruptionObjects;
    [SerializeField] private GameObject[] particleEffects;
    [SerializeField] private TileSpriteCorruptionListDictionary tileCorruptionObjects;

    [Space(10)]
    [Header("Beaches")]
    [SerializeField] private SpriteRenderer topLeftBeach;
    [SerializeField] private SpriteRenderer leftBeach;
    [SerializeField] private SpriteRenderer botLeftBeach;
    [SerializeField] private SpriteRenderer botRightBeach;
    [SerializeField] private SpriteRenderer rightBeach;
    [SerializeField] private SpriteRenderer topRightBeach;

    //[Space(10)]
    //[Header("UI")]
    //[SerializeField] private GraphicRaycaster uiRaycaster;

    public BaseLandmark landmarkOnTile { get; private set; }
    public Region region { get; private set; }

    private Dictionary<HEXTILE_DIRECTION, HexTile> _neighbourDirections;

    public List<HexTile> AllNeighbours { get; set; }
    public List<HexTile> ValidTiles { get { return AllNeighbours.Where(o => o.elevationType != ELEVATION.WATER && o.elevationType != ELEVATION.MOUNTAIN).ToList(); } }
    public List<HexTile> NoWaterTiles { get { return AllNeighbours.Where(o => o.elevationType != ELEVATION.WATER).ToList(); } }
    public List<TILE_TAG> tileTags { get; private set; }

    private List<HexTile> _tilesConnectedInGoingToMarker = new List<HexTile>();
    private List<HexTile> _tilesConnectedInComingFromMarker = new List<HexTile>();

    private int _uncorruptibleLandmarkNeighbors = 0; //if 0, can be corrupted, otherwise, cannot be corrupted
    public BaseLandmark corruptedLandmark = null;
    private GameObject _spawnedTendril = null;

    public Sprite baseSprite { get; private set; }

    #region getters/setters
    public int id { get { return data.id; } }
    public int xCoordinate { get { return data.xCoordinate; } }
    public int yCoordinate { get { return data.yCoordinate; } }
    public int corruptDuration { get { return GetCorruptDuration(); } }
    public string tileName { get { return data.tileName; } }
    public string thisName { get { return data.tileName; } }
    public float elevationNoise { get { return data.elevationNoise; } }
    public float moistureNoise { get { return data.moistureNoise; } }
    public float temperature { get { return data.temperature; } }
    public BIOMES biomeType { get { return data.biomeType; } }
    public ELEVATION elevationType { get { return data.elevationType; } }
    public Area areaOfTile { get { return _areaOfTile; } }
    public string locationName {
        get { return "(" + xCoordinate + ", " + yCoordinate + ")"; }
    }
    public GameObject centerPiece {
        get { return this._centerPiece; }
    }
    public GameObject highlightGO {
        get { return this._highlightGO; }
    }
    internal Dictionary<HEXTILE_DIRECTION, HexTile> neighbourDirections {
        get { return _neighbourDirections; }
    }
    public GameObject clickHighlightGO {
        get { return _clickHighlightGO; }
    }
    public HexTile tileLocation {
        get { return this; }
    }
    public LOCATION_IDENTIFIER locIdentifier {
        get { return LOCATION_IDENTIFIER.HEXTILE; }
    }
    public bool hasLandmark {
        get { return landmarkOnTile != null; }
    }
    public bool isCorrupted {
        get { return _isCorrupted; }
    }
    public int uncorruptibleLandmarkNeighbors {
        get { return _uncorruptibleLandmarkNeighbors; }
    }
    #endregion

    public void Initialize() {
        //spriteRenderer = this.GetComponent<SpriteRenderer>();
        //SetMagicAbundance();
        //StartCorruptionAnimation();
        Messenger.AddListener<Area>(Signals.AREA_MAP_OPENED, AreaMapOpened);
        Messenger.AddListener<Area>(Signals.AREA_MAP_CLOSED, AreaMapClosed);
        tileTags = new List<TILE_TAG>();
    }

    private void AreaMapOpened(Area area) {
        //uiRaycaster.enabled = false;
    }
    private void AreaMapClosed(Area area) {
        //uiRaycaster.enabled = true;
    }

    #region Elevation Functions
    internal void SetElevation(ELEVATION elevationType) {
        data.elevationType = elevationType;
    }
    //public void UpdateLedgesAndOutlines() {
    //    if (neighbourDirections == null) {
    //        return;
    //    }
    //    if (elevationType != ELEVATION.WATER) {
    //        //re enable all outlines and disable all ledges
    //        //topLeftLedge.SetActive(false);
    //        //topRightLedge.SetActive(false);
    //        //SetOutlinesState(true);
    //    } else { //tile is water
    //        //check neighbours
    //        //if north west tile is not water, activate top left ledge
    //        //if (neighbourDirections.ContainsKey(HEXTILE_DIRECTION.NORTH_WEST) && neighbourDirections[HEXTILE_DIRECTION.NORTH_WEST].elevationType != ELEVATION.WATER) {
    //        //    topLeftLedge.SetActive(true);
    //        //    //topLeftOutline.SetActive(false);
    //        //} else {
    //        //    //tile doesn't have a north west neighbour
    //        //}
    //        ////if north east tile is not water, activate top right edge
    //        //if (neighbourDirections.ContainsKey(HEXTILE_DIRECTION.NORTH_EAST) && neighbourDirections[HEXTILE_DIRECTION.NORTH_EAST].elevationType != ELEVATION.WATER) {
    //        //    topRightLedge.SetActive(true);
    //        //    //topRightOutline.SetActive(false);
    //        //} else {
    //        //    //tile doesn't have a north east neighbour
    //        //}

    //        ////check outlines
    //        //foreach (KeyValuePair<HEXTILE_DIRECTION, HexTile> kvp in neighbourDirections) {
    //        //    HexTile neighbour = kvp.Value;
    //        //    HEXTILE_DIRECTION direction = kvp.Key;
    //        //    if (neighbour.elevationType == ELEVATION.WATER) {
    //        //        //deactivate the outline tile in that direction
    //        //        switch (direction) {
    //        //            case HEXTILE_DIRECTION.NORTH_WEST:
    //        //                topLeftOutline.SetActive(false);
    //        //                break;
    //        //            case HEXTILE_DIRECTION.NORTH_EAST:
    //        //                topRightOutline.SetActive(false);
    //        //                break;
    //        //            case HEXTILE_DIRECTION.EAST:
    //        //                rightOutline.SetActive(false);
    //        //                break;
    //        //            case HEXTILE_DIRECTION.SOUTH_EAST:
    //        //                botRightOutline.SetActive(false);
    //        //                break;
    //        //            case HEXTILE_DIRECTION.SOUTH_WEST:
    //        //                botLeftOutline.SetActive(false);
    //        //                break;
    //        //            case HEXTILE_DIRECTION.WEST:
    //        //                leftOutline.SetActive(false);
    //        //                break;
    //        //            default:
    //        //                break;
    //        //        }
    //        //    }
    //        //}
    //    }
    //}
    #endregion

    #region Biome Functions
    internal void SetBiome(BIOMES biome) {
        data.biomeType = biome;
    }
    #endregion

    #region Landmarks
    public void SetLandmarkOnTile(BaseLandmark landmarkOnTile) {
        this.landmarkOnTile = landmarkOnTile;
    }
    public BaseLandmark CreateLandmarkOfType(LANDMARK_TYPE landmarkType) {
        LandmarkData data = LandmarkManager.Instance.GetLandmarkData(landmarkType);
        SetLandmarkOnTile(new BaseLandmark(this, landmarkType));
        if (data.minimumTileCount > 1) {
            if (neighbourDirections.ContainsKey(data.connectedTileDirection) && neighbourDirections[data.connectedTileDirection] != null) {
                HexTile tileToConnect = neighbourDirections[data.connectedTileDirection];
                landmarkOnTile.SetConnectedTile(tileToConnect);
                tileToConnect.SetElevation(ELEVATION.PLAIN);
                tileToConnect.SetLandmarkOnTile(this.landmarkOnTile); //set the landmark of the connected tile to the same landmark on this tile
                Biomes.Instance.UpdateTileVisuals(tileToConnect);
            }
        }
        //Create Landmark Game Object on tile
        GameObject landmarkGO = CreateLandmarkVisual(landmarkType, this.landmarkOnTile, data);
        if (landmarkGO != null) {
            landmarkGO.transform.localPosition = Vector3.zero;
            landmarkGO.transform.localScale = Vector3.one;
            landmarkOnTile.SetLandmarkObject(landmarkGO.GetComponent<LandmarkVisual>());
        }
        if (landmarkType == LANDMARK_TYPE.CAVE) {
            SetElevation(ELEVATION.MOUNTAIN);
        } else {
            SetElevation(ELEVATION.PLAIN);
        }
        Biomes.Instance.UpdateTileVisuals(this);
        return landmarkOnTile;
    }
    public BaseLandmark CreateLandmarkOfType(LandmarkSaveData saveData) {
        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(saveData.landmarkType);
        SetLandmarkOnTile(new BaseLandmark(this, saveData));
        if (landmarkData.minimumTileCount > 1) {
            if (neighbourDirections.ContainsKey(landmarkData.connectedTileDirection) && neighbourDirections[landmarkData.connectedTileDirection] != null) {
                HexTile tileToConnect = neighbourDirections[landmarkData.connectedTileDirection];
                landmarkOnTile.SetConnectedTile(tileToConnect);
                tileToConnect.SetElevation(ELEVATION.PLAIN);
                tileToConnect.SetLandmarkOnTile(this.landmarkOnTile); //set the landmark of the connected tile to the same landmark on this tile
                Biomes.Instance.UpdateTileVisuals(tileToConnect);
            }
        }
        //Create Landmark Game Object on tile
        GameObject landmarkGO = CreateLandmarkVisual(saveData.landmarkType, this.landmarkOnTile, landmarkData);
        if (landmarkGO != null) {
            landmarkGO.transform.localPosition = Vector3.zero;
            landmarkGO.transform.localScale = Vector3.one;
            landmarkOnTile.SetLandmarkObject(landmarkGO.GetComponent<LandmarkVisual>());
        }
        return landmarkOnTile;
    }
    public BaseLandmark CreateLandmarkOfType(SaveDataLandmark saveData) {
        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(saveData.landmarkType);
        SetLandmarkOnTile(new BaseLandmark(this, saveData));
        //if (landmarkData.minimumTileCount > 1) {
        //    if (neighbourDirections.ContainsKey(landmarkData.connectedTileDirection) && neighbourDirections[landmarkData.connectedTileDirection] != null) {
        //        HexTile tileToConnect = neighbourDirections[landmarkData.connectedTileDirection];
        //        landmarkOnTile.SetConnectedTile(tileToConnect);
        //        tileToConnect.SetElevation(ELEVATION.PLAIN);
        //        tileToConnect.SetLandmarkOnTile(this.landmarkOnTile); //set the landmark of the connected tile to the same landmark on this tile
        //        Biomes.Instance.UpdateTileVisuals(tileToConnect);
        //    }
        //}
        //Create Landmark Game Object on tile
        GameObject landmarkGO = CreateLandmarkVisual(saveData.landmarkType, this.landmarkOnTile, landmarkData);
        if (landmarkGO != null) {
            landmarkGO.transform.localPosition = Vector3.zero;
            landmarkGO.transform.localScale = Vector3.one;
            landmarkOnTile.SetLandmarkObject(landmarkGO.GetComponent<LandmarkVisual>());
        }
        return landmarkOnTile;
    }
    private GameObject CreateLandmarkVisual(LANDMARK_TYPE landmarkType, BaseLandmark landmark, LandmarkData data) {
#if WORLD_CREATION_TOOL
        GameObject landmarkGO = GameObject.Instantiate(worldcreator.WorldCreatorManager.Instance.landmarkItemPrefab, structureParentGO.transform) as GameObject;
#else
        GameObject landmarkGO = GameObject.Instantiate(LandmarkManager.Instance.GetLandmarkGO(), structureParentGO.transform) as GameObject;
#endif
        RACE race = RACE.NONE;
        if (areaOfTile != null) {
            if (areaOfTile.areaType == AREA_TYPE.ELVEN_SETTLEMENT) {
                race = RACE.ELVES;
            } else if (areaOfTile.areaType == AREA_TYPE.HUMAN_SETTLEMENT) {
                race = RACE.HUMANS;
            }
        }
        List<LandmarkStructureSprite> landmarkTileSprites = LandmarkManager.Instance.GetLandmarkTileSprites(this, landmarkType, race);
        if (data.minimumTileCount > 1) {
            SetLandmarkTileSprite(landmarkTileSprites[0]);
            landmark.connectedTile.SetLandmarkTileSprite(landmarkTileSprites[1]);
            landmarkGO.GetComponent<LandmarkVisual>().SetIconState(false);
        } else {
            if (landmarkTileSprites == null || landmarkTileSprites.Count == 0) {
                //DeactivateCenterPiece();
                HideLandmarkTileSprites();
                landmarkGO.GetComponent<LandmarkVisual>().SetIconState(true);
            } else {
                SetLandmarkTileSprite(landmarkTileSprites[Random.Range(0, landmarkTileSprites.Count)]);
                landmarkGO.GetComponent<LandmarkVisual>().SetIconState(false);
            }
        }
        return landmarkGO;
    }
    public void UpdateLandmarkVisuals() {
        LandmarkData data = LandmarkManager.Instance.GetLandmarkData(landmarkOnTile.specificLandmarkType);
        List<LandmarkStructureSprite> landmarkTileSprites = LandmarkManager.Instance.GetLandmarkTileSprites(this, landmarkOnTile.specificLandmarkType);
        if (data.minimumTileCount > 1) {
            SetLandmarkTileSprite(landmarkTileSprites[0]);
            landmarkOnTile.connectedTile.SetLandmarkTileSprite(landmarkTileSprites[1]);
            landmarkOnTile.landmarkVisual.SetIconState(false);
        } else {
            if (landmarkTileSprites == null || landmarkTileSprites.Count == 0) {
                //DeactivateCenterPiece();
                HideLandmarkTileSprites();
                landmarkOnTile.landmarkVisual.SetIconState(true);
            } else {
                SetLandmarkTileSprite(landmarkTileSprites[Random.Range(0, landmarkTileSprites.Count)]);
                landmarkOnTile.landmarkVisual.SetIconState(false);
            }
        }
    }
    public BaseLandmark LoadLandmark(BaseLandmark landmark) {
        GameObject landmarkGO = null;
        //Create Landmark Game Object on tile
        landmarkGO = GameObject.Instantiate(LandmarkManager.Instance.GetLandmarkGO(), structureParentGO.transform) as GameObject;
        landmarkGO.transform.localPosition = Vector3.zero;
        landmarkGO.transform.localScale = Vector3.one;
        landmarkOnTile = landmark;
        if (landmarkGO != null) {
            landmarkOnTile.SetLandmarkObject(landmarkGO.GetComponent<LandmarkVisual>());
        }
        return landmarkOnTile;
    }
    public void RemoveLandmarkOnTile() {
        landmarkOnTile = null;
    }
    public void RemoveLandmarkVisuals() {
        HideLandmarkTileSprites();
        GameObject.Destroy(landmarkOnTile.landmarkVisual.gameObject);
    }
    public void SetLandmarkTileSprite(LandmarkStructureSprite sprites) {
        mainStructure.sprite = sprites.mainSprite;
        structureTint.sprite = sprites.tintSprite;
        mainStructure.gameObject.SetActive(true);
        structureTint.gameObject.SetActive(true);
        if (sprites.animation == null) {
            structureAnimation.gameObject.SetActive(false);
        } else {
            structureAnimation.gameObject.SetActive(true);
            structureAnimation.runtimeAnimatorController = sprites.animation;
        }
    }
    public void HideLandmarkTileSprites() {
        mainStructure.gameObject.SetActive(false);
        structureTint.gameObject.SetActive(false);
    }
    public void SetStructureTint(Color color) {
        color.a = 150f/255f;
        structureTint.color = color;
    }
    #endregion

    #region Tile Utilities
    public void SetData(HexTileData data) {
        this.data = data;
    }
    /*
	 * Returns all Hex tiles gameobjects within a radius
	 * */
    public List<HexTile> GetTilesInRange(int range, bool isOnlyOuter) {
        List<HexTile> tilesInRange = new List<HexTile>();
        List<HexTile> checkedTiles = new List<HexTile>();
        List<HexTile> tilesToAdd = new List<HexTile>();

        for (int i = 0; i < range; i++) {
            if (tilesInRange.Count <= 0) {
                //tilesInRange = this.AllNeighbours;
                for (int j = 0; j < AllNeighbours.Count; j++) {
                    tilesInRange.Add(AllNeighbours[j]);
                }
                checkedTiles.Add(this);
            } else {
                tilesToAdd.Clear();
                int tilesInRangeCount = tilesInRange.Count;
                for (int j = 0; j < tilesInRangeCount; j++) {
                    if (!checkedTiles.Contains(tilesInRange[j])) {
                        checkedTiles.Add(tilesInRange[j]);
                        List<HexTile> neighbors = tilesInRange[j].AllNeighbours;
                        for (int k = 0; k < neighbors.Count; k++) {
                            if (!tilesInRange.Contains(neighbors[k])) {
                                tilesToAdd.Add(neighbors[k]);
                            }
                        }
                        tilesInRange.AddRange(tilesToAdd);
                        //						tilesToAdd.AddRange (tilesInRange[j].AllNeighbours.Where(x => !tilesInRange.Contains(x)).ToList());
                    }
                }
                if (i == range - 1 && isOnlyOuter) {
                    return tilesToAdd;
                }

                //				tilesInRange = tilesInRange.Distinct ().ToList ();
            }
        }
        return tilesInRange;
    }
    public List<HexTile> GetTilesInRange(int range) {
        List<HexTile> tilesInRange = new List<HexTile>();
        CubeCoordinate cube = GridMap.Instance.OddRToCube(new HexCoordinate(xCoordinate, yCoordinate));
        //Debug.Log("Center in cube coordinates: " + cube.x.ToString() + "," + cube.y.ToString() + "," + cube.z.ToString());
        for (int dx = -range; dx <= range; dx++) {
            for (int dy = Mathf.Max(-range, -dx - range); dy <= Mathf.Min(range, -dx + range); dy++) {
                int dz = -dx - dy;
                HexCoordinate hex = GridMap.Instance.CubeToOddR(new CubeCoordinate(cube.x + dx, cube.y + dy, cube.z + dz));
                //Debug.Log("Hex neighbour: " + hex.col.ToString() + "," + hex.row.ToString());
                if (hex.col >= 0 && hex.row >= 0
                    && hex.col < GridMap.Instance.width && hex.row < GridMap.Instance.height
                    && !(hex.col == xCoordinate && hex.row == yCoordinate)) {
                    tilesInRange.Add(GridMap.Instance.map[hex.col, hex.row]);
                }
            }
        }
        return tilesInRange;
    }
    public int GetTileDistanceTo(HexTile target) {
        float distance = Vector3.Distance(this.transform.position, target.transform.position);
        int distanceAsTiles = Mathf.CeilToInt(distance / 2.315188f);
        return distanceAsTiles;
    }
    public bool IsAtEdgeOfMap() {
        return AllNeighbours.Count < 6; //if this tile has less than 6 neighbours, it is at the edge of the map
    }
    public bool HasNeighbourAtEdgeOfMap() {
        for (int i = 0; i < AllNeighbours.Count; i++) {
            HexTile currNeighbour = AllNeighbours[i];
            if (currNeighbour.IsAtEdgeOfMap()) {
                return true;
            }
        }
        return false;
    }
    public void GenerateInitialTileTags() {
        //Elevation
        if(landmarkOnTile == null || landmarkOnTile.specificLandmarkType != LANDMARK_TYPE.CAVE) {
            if (elevationType == ELEVATION.MOUNTAIN) {
                tileTags.Add(TILE_TAG.MOUNTAIN);
            } else if (elevationType == ELEVATION.PLAIN) {
                tileTags.Add(TILE_TAG.FLATLAND);
            } else if (elevationType == ELEVATION.TREES) {
                tileTags.Add(TILE_TAG.FOREST);
            }
        }

        //Biome
        if (biomeType == BIOMES.DESERT) {
            tileTags.Add(TILE_TAG.DESERT);
        } else if (biomeType == BIOMES.FOREST) {
            if (elevationType == ELEVATION.TREES) {
                tileTags.Add(TILE_TAG.JUNGLE); //Myk, tama ba to? Hahaha, di ko sure to lol. RE: Kapag ata may trees din yung forest na biome, saka siya nagiging jungle? ahahaha
            }
        } else if (biomeType == BIOMES.GRASSLAND) {
            tileTags.Add(TILE_TAG.GRASSLAND);
        } else if (biomeType == BIOMES.SNOW) {
            tileTags.Add(TILE_TAG.SNOW);
        } else if (biomeType == BIOMES.TUNDRA) {
            tileTags.Add(TILE_TAG.TUNDRA);
        }

        //Landmark Tags
        if (landmarkOnTile != null) {
            switch (landmarkOnTile.specificLandmarkType) {
                case LANDMARK_TYPE.CAVE:
                    tileTags.Add(TILE_TAG.CAVE);
                    break;
                default:
                    tileTags.Add(TILE_TAG.DUNGEON); //Ginawa ko na dungeon type na lang muna lahat ng landmarks na di caves.
                    break;
            }
        }
    }
    public void AddTileTag(TILE_TAG tag) {
        tileTags.Add(tag);
    }
    public void RemoveTileTag(TILE_TAG tag) {
        tileTags.Remove(tag);
    }
    public bool HasNeighbourFromOtherRegion() {
        for (int i = 0; i < AllNeighbours.Count; i++) {
            HexTile currNeighbour = AllNeighbours[i];
            if (currNeighbour.region != this.region) {
                return true;
            }
        }
        return false;
    }
    public bool TryGetDifferentRegionNeighbours(out List<Region> regions) {
        regions = new List<Region>();
        for (int i = 0; i < AllNeighbours.Count; i++) {
            HexTile currNeighbour = AllNeighbours[i];
            if (currNeighbour.region != this.region) {
                regions.Add(currNeighbour.region);
            }
        }
        return regions.Count > 0;
    }
    #endregion

    #region Pathfinding
    public void FindNeighbours(HexTile[,] gameBoard) {
        _neighbourDirections = new Dictionary<HEXTILE_DIRECTION, HexTile>();
        var neighbours = new List<HexTile>();

        List<Point> possibleExits;

        if ((yCoordinate % 2) == 0) {
            possibleExits = Utilities.EvenNeighbours;
        } else {
            possibleExits = Utilities.OddNeighbours;
        }

        for (int i = 0; i < possibleExits.Count; i++) {
            int neighbourCoordinateX = xCoordinate + possibleExits[i].X;
            int neighbourCoordinateY = yCoordinate + possibleExits[i].Y;
            if (neighbourCoordinateX >= 0 && neighbourCoordinateX < gameBoard.GetLength(0) && neighbourCoordinateY >= 0 && neighbourCoordinateY < gameBoard.GetLength(1)) {
                HexTile currNeighbour = gameBoard[neighbourCoordinateX, neighbourCoordinateY];
                if (currNeighbour != null) {
                    neighbours.Add(currNeighbour);
                } 
                //else {
                //    //This part is for outerGridTiles only!
                //    try {
                //        neighbourCoordinateX -= GridMap.Instance._borderThickness;
                //        neighbourCoordinateY -= GridMap.Instance._borderThickness;
                //        currNeighbour = GridMap.Instance.map[neighbourCoordinateX, neighbourCoordinateY];
                //        neighbours.Add(currNeighbour);
                //    } catch {
                //        //No Handling
                //    }
                //}
            }

        }
        this.AllNeighbours = neighbours;

        for (int i = 0; i < neighbours.Count; i++) {
            HexTile currNeighbour = neighbours[i];
            if (currNeighbour == null) {
                continue;
            }
            HEXTILE_DIRECTION dir = GetNeighbourDirection(currNeighbour);
            if (dir != HEXTILE_DIRECTION.NONE) {
                _neighbourDirections.Add(dir, currNeighbour);
            }
        }
    }
    public void FindNeighboursForBorders() {
        _neighbourDirections = new Dictionary<HEXTILE_DIRECTION, HexTile>();
        var neighbours = new List<HexTile>();

        List<Point> possibleExits;

        if ((yCoordinate % 2) == 0) {
            possibleExits = Utilities.EvenNeighbours;
        } else {
            possibleExits = Utilities.OddNeighbours;
        }

        for (int i = 0; i < possibleExits.Count; i++) {
            int neighbourCoordinateX = xCoordinate + possibleExits[i].X;
            int neighbourCoordinateY = yCoordinate + possibleExits[i].Y;
#if WORLD_CREATION_TOOL
            HexTile neighbour = WorldCreatorManager.Instance.GetTileFromCoordinates(neighbourCoordinateX, neighbourCoordinateY);
#else
            HexTile neighbour = GridMap.Instance.GetTileFromCoordinates(neighbourCoordinateX, neighbourCoordinateY);
#endif

            if (neighbour != null) {
                neighbours.Add(neighbour);
            }

        }
        this.AllNeighbours = neighbours;

        for (int i = 0; i < neighbours.Count; i++) {
            HexTile currNeighbour = neighbours[i];
            if (currNeighbour == null) {
                continue;
            }
            HEXTILE_DIRECTION dir = GetNeighbourDirection(currNeighbour);
            if (dir != HEXTILE_DIRECTION.NONE) {
                _neighbourDirections.Add(dir, currNeighbour);
            }
        }
    }
    internal HEXTILE_DIRECTION GetNeighbourDirection(HexTile neighbour) {
        if (neighbour == null) {
            return HEXTILE_DIRECTION.NONE;
        }
        if (!AllNeighbours.Contains(neighbour)) {
            throw new System.Exception(neighbour.name + " is not a neighbour of " + this.name);
        }
        int thisXCoordinate = this.xCoordinate;
        int thisYCoordinate = this.yCoordinate;
//        if (isForOuterGrid) {
//#if WORLD_CREATION_TOOL
//            if (!worldcreator.WorldCreatorManager.Instance.outerGridList.Contains(neighbour)) {
//                thisXCoordinate -= worldcreator.WorldCreatorManager.Instance._borderThickness;
//                thisYCoordinate -= worldcreator.WorldCreatorManager.Instance._borderThickness;
//            }
//#else
//            if (!GridMap.Instance.outerGridList.Contains(neighbour)) {
//                thisXCoordinate -= GridMap.Instance._borderThickness;
//                thisYCoordinate -= GridMap.Instance._borderThickness;
//            }
//#endif
//        }
            Point difference = new Point((neighbour.xCoordinate - thisXCoordinate),
                    (neighbour.yCoordinate - thisYCoordinate));
        if (thisYCoordinate % 2 == 0) { //even
            if (difference.X == -1 && difference.Y == 1) {
                //top left
                return HEXTILE_DIRECTION.NORTH_WEST;
            } else if (difference.X == 0 && difference.Y == 1) {
                //top right
                return HEXTILE_DIRECTION.NORTH_EAST;
            } else if (difference.X == 1 && difference.Y == 0) {
                //right
                return HEXTILE_DIRECTION.EAST;
            } else if (difference.X == 0 && difference.Y == -1) {
                //bottom right
                return HEXTILE_DIRECTION.SOUTH_EAST;
            } else if (difference.X == -1 && difference.Y == -1) {
                //bottom left
                return HEXTILE_DIRECTION.SOUTH_WEST;
            } else if (difference.X == -1 && difference.Y == 0) {
                //left
                return HEXTILE_DIRECTION.WEST;
            }
        } else { //odd
            if (difference.X == 0 && difference.Y == 1) {
                //top left
                return HEXTILE_DIRECTION.NORTH_WEST;
            } else if (difference.X == 1 && difference.Y == 1) {
                //top right
                return HEXTILE_DIRECTION.NORTH_EAST;
            } else if (difference.X == 1 && difference.Y == 0) {
                //right
                return HEXTILE_DIRECTION.EAST;
            } else if (difference.X == 1 && difference.Y == -1) {
                //bottom right
                return HEXTILE_DIRECTION.SOUTH_EAST;
            } else if (difference.X == 0 && difference.Y == -1) {
                //bottom left
                return HEXTILE_DIRECTION.SOUTH_WEST;
            } else if (difference.X == -1 && difference.Y == 0) {
                //left
                return HEXTILE_DIRECTION.WEST;
            }
        }
        return HEXTILE_DIRECTION.NONE;
    }
    public HexTile GetNeighbour(HEXTILE_DIRECTION direction) {
        if (neighbourDirections.ContainsKey(direction)) {
            return neighbourDirections[direction];
        }
        return null;
    }
    #endregion

    #region Tile Visuals
    internal void SetSortingOrder(int sortingOrder, string sortingLayerName = "Default") {
        spriteRenderer.sortingOrder = sortingOrder;
        spriteRenderer.sortingLayerName = sortingLayerName;
        UpdateSortingOrder();
    }
    internal void UpdateSortingOrder() {
        int sortingOrder = spriteRenderer.sortingOrder;
        _hoverHighlightGO.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 1;
        highlightGO.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 1;

        topLeftBeach.sortingOrder = sortingOrder + 1;
        leftBeach.sortingOrder = sortingOrder + 1;
        botLeftBeach.sortingOrder = sortingOrder + 1;
        botRightBeach.sortingOrder = sortingOrder + 1;
        rightBeach.sortingOrder = sortingOrder + 1;
        topRightBeach.sortingOrder = sortingOrder + 1;

        topLeftBorder.sortingOrder = sortingOrder + 12;
        topRightBorder.sortingOrder = sortingOrder + 12;
        leftBorder.sortingOrder = sortingOrder + 12;
        botLeftBorder.sortingOrder = sortingOrder + 12;
        botRightBorder.sortingOrder = sortingOrder + 12;
        rightBorder.sortingOrder = sortingOrder + 12;
        

        mainStructure.sortingOrder = sortingOrder + 3;
        structureTint.sortingOrder = sortingOrder + 4;
        structureAnimation.gameObject.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 5;
    }
    public SpriteRenderer GetBorder(HEXTILE_DIRECTION direction) {
        SpriteRenderer border = null;
        switch (direction) {
            case HEXTILE_DIRECTION.NORTH_WEST:
                border = topLeftBorder;
                break;
            case HEXTILE_DIRECTION.NORTH_EAST:
                border = topRightBorder;
                break;
            case HEXTILE_DIRECTION.EAST:
                border = rightBorder;
                break;
            case HEXTILE_DIRECTION.SOUTH_EAST:
                border = botRightBorder;
                break;
            case HEXTILE_DIRECTION.SOUTH_WEST:
                border = botLeftBorder;
                break;
            case HEXTILE_DIRECTION.WEST:
                border = leftBorder;
                break;
            default:
                break;
        }
        return border;
    }
    public SpriteRenderer[] GetAllBorders() {
        SpriteRenderer[] border = new SpriteRenderer[] {
            topLeftBorder, topRightBorder, rightBorder, botRightBorder, botLeftBorder, leftBorder
        };
        return border;
    }
    internal void DeactivateCenterPiece() {
        centerPiece.SetActive(false);
    }
    internal void SetBaseSprite(Sprite baseSprite) {
        this.baseSprite = baseSprite;
        spriteRenderer.sprite = baseSprite;
        RuntimeAnimatorController animation = Biomes.Instance.GetTileSpriteAnimation(baseSprite);
        if (animation != null) {
            baseTileAnimator.runtimeAnimatorController = animation;
            baseTileAnimator.enabled = true;
        } else {
            baseTileAnimator.enabled = false;
        }
    }
    public void HighlightTile(Color color, float alpha) {
        color.a = alpha;
        _highlightGO.SetActive(true);
        _highlightGO.GetComponent<SpriteRenderer>().color = color;
    }
    public void UnHighlightTile() {
            _highlightGO.SetActive(false);
        }
    public void SetBordersState(bool stat) {
        topLeftBorder.gameObject.SetActive(stat);
        botLeftBorder.gameObject.SetActive(stat);
        topRightBorder.gameObject.SetActive(stat);
        botRightBorder.gameObject.SetActive(stat);
        leftBorder.gameObject.SetActive(stat);
        rightBorder.gameObject.SetActive(stat);
    }
    public void SetBorderColor(Color color) {
        topLeftBorder.color = color;
        botLeftBorder.color = color;
        topRightBorder.color = color;
        botRightBorder.color = color;
        leftBorder.color = color;
        rightBorder.color = color;

        //topLeftBorderSGE.GlowColor = color;
        //botLeftBorderSGE.GlowColor = color;
        //topRightBorderSGE.GlowColor = color;
        //botRightBorderSGE.GlowColor = color;
        //leftBorderSGE.GlowColor = color;
        //rightBorderSGE.GlowColor = color;
    }
    #endregion

    #region Tile Functions
    public void DisableColliders() {
        this.GetComponent<Collider2D>().enabled = false;
        Collider[] colliders = this.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++) {
            colliders[i].enabled = false;
        }
    }
    /*
        * Reset all values for this tile.
        * NOTE: This will set the structure to ruined.
        * To force destroy structure, call DestroyStructure
        * in StructureObject instead.
        * */
    public void ResetTile() {
        this.isOccupied = false;
    }
    public void Occupy() {
        this.isOccupied = true;
    }
    public void Unoccupy() {
        isOccupied = false;
    }
    #endregion

    #region Monobehaviour Functions
    public void LeftClick() {
#if WORLD_CREATION_TOOL
        Messenger.Broadcast<HexTile>(Signals.TILE_LEFT_CLICKED, this);
#else
        if (UIManager.Instance.IsMouseOnUI() || UIManager.Instance.IsConsoleShowing()) {
            return;
        }
        //StartCorruptionAnimation();
        Messenger.Broadcast(Signals.TILE_LEFT_CLICKED, this);
        if (PlayerManager.Instance.isChoosingStartingTile) {
            return;
        }

        if (this.region != null) {
            if (this.region.mainLandmark.tileLocation.areaOfTile != null) {
                UIManager.Instance.ShowAreaInfo(this.region.mainLandmark.tileLocation.areaOfTile);
            } else {
                UIManager.Instance.ShowRegionInfo(this.region);
            }
        } else {
            Messenger.Broadcast(Signals.HIDE_MENUS);
        }
#endif
    }
    public void RightClick() {
#if WORLD_CREATION_TOOL
        Messenger.Broadcast<HexTile>(Signals.TILE_RIGHT_CLICKED, this);
#else
        if (UIManager.Instance.IsMouseOnUI() || UIManager.Instance.IsConsoleShowing()) {
            return;
        }
        Messenger.Broadcast(Signals.TILE_RIGHT_CLICKED, this);
        //if (landmarkOnTile != null && (UIManager.Instance.characterInfoUI.currentlyShowingCharacter != null && UIManager.Instance.characterInfoUI.currentlyShowingCharacter.role.roleType == CHARACTER_ROLE.PLAYER)) {
        //    UIManager.Instance.ShowPlayerActions(this.landmarkOnTile);
        //}
#endif
    }
    private void MouseOver() {
#if WORLD_CREATION_TOOL
        //Debug.Log("IS MOUSE OVER UI " + worldcreator.WorldCreatorUI.Instance.IsMouseOnUI());
        Messenger.Broadcast<HexTile>(Signals.TILE_HOVERED_OVER, this);
        //if (!worldcreator.WorldCreatorUI.Instance.IsMouseOnUI()) {
        //    Messenger.Broadcast<HexTile>(Signals.TILE_HOVERED_OVER, this);
        //    if (Input.GetMouseButton(0)) {
        //        Messenger.Broadcast<HexTile>(Signals.TILE_LEFT_CLICKED, this);
        //    }
        //    if (Input.GetMouseButtonUp(1)) {
        //        Messenger.Broadcast<HexTile>(Signals.TILE_RIGHT_CLICKED, this);
        //    }
        //}
        //ShowHexTileInfo();
#else
        //if (this.areaOfTile != null) {
            _hoverHighlightGO.SetActive(true);
        //SetBordersState(true);
        //}
        Messenger.Broadcast(Signals.TILE_HOVERED_OVER, this);
#endif
    }
    private void MouseExit() {
#if WORLD_CREATION_TOOL
        //if (!worldcreator.WorldCreatorUI.Instance.IsMouseOnUI()) {
            Messenger.Broadcast<HexTile>(Signals.TILE_HOVERED_OUT, this);
        //}
        //if (WorldCreatorManager.Instance.outerGridList.Contains(this)) {
        //    SetNeighbourHighlightState(false);
        //}
#else
        _hoverHighlightGO.SetActive(false);
        //SetBordersState(false);
        UIManager.Instance.HideSmallInfo();
        //if (UIManager.Instance.IsMouseOnUI() || UIManager.Instance.IsConsoleShowing()) {
        //    return;
        //}
        Messenger.Broadcast(Signals.TILE_HOVERED_OUT, this);
#endif
    }
    private bool hasPendingJob = false;
    private void DoubleLeftClick() {
        //Debug.Log("double click");
        //PlayerUI.Instance.ShowCorruptTileConfirmation(this);
    }
    public void PointerClick(BaseEventData bed) {
        PointerEventData ped = bed as PointerEventData;
        if (ped.clickCount == 1) {
            if (ped.button == PointerEventData.InputButton.Left) {
                LeftClick();
            } else if (ped.button == PointerEventData.InputButton.Right) {
                RightClick();
            }
        } else if (ped.clickCount == 2) {
            if (ped.button == PointerEventData.InputButton.Left) {
                DoubleLeftClick();
            }
        }
    }
    public void OnPointerEnter(BaseEventData bed) {
        PointerEventData ped = bed as PointerEventData;
        if (ped.pointerCurrentRaycast.gameObject.tag == "Avatar") {
            OnPointerExit(bed);
            return;
        }
        MouseOver();
    }
    public void OnPointerExit(BaseEventData bed) {
        MouseExit();
    }
    #endregion

    #region For Testing
    [Space(10)]
    [Header("For Testing")]
    [SerializeField] private int range = 0;
    List<HexTile> tiles = new List<HexTile>();
    [ContextMenu("Show Tiles In Range")]
    public void ShowTilesInRange() {
        for (int i = 0; i < tiles.Count; i++) {
            tiles[i].spriteRenderer.color = Color.white;
        }
        tiles.Clear();
        tiles.AddRange(this.GetTilesInRange(range));
        for (int i = 0; i < tiles.Count; i++) {
            tiles[i].spriteRenderer.color = Color.magenta;
        }
    }
    [ContextMenu("Show Hextile Positions")]
    public void ShowHextileBounds() {
        Debug.Log("Local Pos: " + this.transform.localPosition.ToString());
        Debug.Log("Pos: " + this.transform.position.ToString());
    }
    [ContextMenu("Force Reset Tile")]
    public void ForceResetTile() {
        ResetTile();
    }
    [ContextMenu("Log Neighbour Directions")]
    public void LogNeighbourDirections() {
        string text = this.name + " neighbours: ";
        foreach (KeyValuePair<HEXTILE_DIRECTION, HexTile> kvp in neighbourDirections) {
            text += "\n" + kvp.Key.ToString() + " - " + kvp.Value.name;
        }
        Debug.Log(text);
    }
    [ContextMenu("Log Screen Position")]
    public void LogScreenPosition() {
        //Debug.Log(CameraMove.Instance.wholeMapCamera.WorldToScreenPoint(this.transform.position));
    }
    [ContextMenu("Force Update Tile Visuals")]
    public void ForceUpdateTileVisuals() {
        Biomes.Instance.UpdateTileVisuals(this);
    }
    public override string ToString() {
        return this.locationName;
    }
    public void ShowTileInfo() {
        string summary = "Landmark: " + landmarkOnTile?.specificLandmarkType.ToString();
        if (landmarkOnTile != null) {
            summary += "\n\t- World Object: " + landmarkOnTile.worldObj?.ToString();
            summary += "\n\t- Connections: " + landmarkOnTile.connections.Count.ToString();
            for (int i = 0; i < landmarkOnTile.connections.Count; i++) {
                BaseLandmark connection = landmarkOnTile.connections[i];
                summary += "\n\t\t- " + connection.specificLandmarkType.ToString() + " " + connection.tileLocation.locationName;
            }
        }
        summary += "\nRegion: " + region.id + " (Tiles: " + region.tiles.Count.ToString() + ")";
        summary += "\nArea: " + areaOfTile?.name;
        //summary += "\nNeighbours: " + AllNeighbours.Count.ToString();
        //for (int i = 0; i < AllNeighbours.Count; i++) {
        //    HexTile currNeighbour = AllNeighbours[i];
        //    summary += "\n\t-" + currNeighbour.name;
        //}
        UIManager.Instance.ShowSmallInfo(summary, this.ToString() + " Info: ");
    }
    #endregion

    #region Corruption
    public void SetCorruption(bool state, BaseLandmark landmark = null) {
        if(_isCorrupted != state) {
            _isCorrupted = state;
        }
    }
    public void SetUncorruptibleLandmarkNeighbors(int amount) {
        _uncorruptibleLandmarkNeighbors = amount;
    }
    public void AdjustUncorruptibleLandmarkNeighbors(int amount) {
        _uncorruptibleLandmarkNeighbors += amount;
        if(_uncorruptibleLandmarkNeighbors < 0) {
            _uncorruptibleLandmarkNeighbors = 0;
        }
        if(_uncorruptibleLandmarkNeighbors > 1 && landmarkOnTile != null) {
            _uncorruptibleLandmarkNeighbors = 1;
        }
    }
    public void StartCorruptionAnimation() {
        GameObject tendril = null;
        if (tileCorruptionObjects.ContainsKey(spriteRenderer.sprite)) {
            List<GameObject> choices = tileCorruptionObjects[spriteRenderer.sprite];
            tendril = choices[Random.Range(0, choices.Count)];
        } else {
            tendril = defaultCorruptionObjects[Random.Range(0, defaultCorruptionObjects.Length)];
        }
        //if (this.biomeType == BIOMES.DESERT && spriteRenderer.sprite.name.Contains("mountains")) {
        //    tendril = desertTendrils[0];
        //    //if (spriteRenderer.sprite.name.Contains("1")) {
        //    //    tendril = desertTendrils[0];
        //    //} else if (spriteRenderer.sprite.name.Contains("2")) {
        //    //    tendril = desertTendrils[1];
        //    //} else if (spriteRenderer.sprite.name.Contains("3")) {
        //    //    tendril = desertTendrils[2];
        //    //}
        //} else {
        //    tendril = defaultCorruptionObjects[Random.Range(0, defaultCorruptionObjects.Length)];
        //}
        _spawnedTendril = GameObject.Instantiate(tendril, biomeDetailsParent);
        SpriteRenderer[] srs = Utilities.GetComponentsInDirectChildren<SpriteRenderer>(_spawnedTendril);
        for (int i = 0; i < srs.Length; i++) {
            SpriteRenderer currRenderer = srs[i];
            currRenderer.sortingLayerName = "Default";
            currRenderer.sortingOrder = spriteRenderer.sortingOrder + 5;
        }
        //for (int i = 0; i < particleEffects.Length; i++) {
        //    particleEffects[i].gameObject.SetActive(true);
        //}
    }
    public void StopCorruptionAnimation() {
        if(_spawnedTendril != null) {
            GameObject.Destroy(_spawnedTendril);
            _spawnedTendril = null;
        }
        for (int i = 0; i < particleEffects.Length; i++) {
            particleEffects[i].gameObject.SetActive(false);
        }
    }
    private int GetCorruptDuration() {
        //return 3;
        int duration = 0;
        for (int i = 0; i < tileTags.Count; i++) {
            duration += GetTileTagCorruptDuration(tileTags[i]);
        }
        return duration;
    }
    private int GetTileTagCorruptDuration(TILE_TAG tileTag) {
        switch (tileTag) {
            case TILE_TAG.CAVE:
                return 7;
            case TILE_TAG.DESERT:
                return 3;
            case TILE_TAG.DUNGEON:
                return 8;
            case TILE_TAG.FLATLAND:
                return 3;
            case TILE_TAG.FOREST:
                return 5;
            case TILE_TAG.GRASSLAND:
                return 4;
            case TILE_TAG.JUNGLE:
                return 5;
            case TILE_TAG.MOUNTAIN:
                return 6;
            case TILE_TAG.SNOW:
                return 5;
            case TILE_TAG.TUNDRA:
                return 4;
        }
        return 0;
    }
    public bool CanBeCorrupted() {
        return true;

        //bool canBeCorrupted = false;
        //if (landmarkOnTile != null && landmarkOnTile.connections != null) {
        //    for (int i = 0; i < landmarkOnTile.connections.Count; i++) {
        //        BaseLandmark connection = landmarkOnTile.connections[i];
        //        if (connection.tileLocation.isCorrupted) {
        //            canBeCorrupted = true;
        //            break;
        //        }
        //    }
        //}
        return region.CanBeInvaded();
    }
    #endregion

    #region Areas
    public void SetArea(Area area) {
        _areaOfTile = area;
        if (_areaOfTile == null) {
            SetExternalState(false);
            SetInternalState(false);
        }
    }
    private bool IsAdjacentToPlayerArea() {
        for (int i = 0; i < AllNeighbours.Count; i++) {
            HexTile currNeighbour = AllNeighbours[i];
            if (currNeighbour.areaOfTile != null && currNeighbour.areaOfTile.id == PlayerManager.Instance.player.playerArea.id) {
                return true;
            }
        }
        return false;
    }
    public void SetExternalState(bool state) {
        _isExternal = state;
    }
    public void SetInternalState(bool state) {
        _isInternal = state;
    }
    #endregion

    #region Context Menu
#if WORLD_CREATION_TOOL
    public ContextMenuSettings GetContextMenuSettings() {
        ContextMenuSettings settings = new ContextMenuSettings();
        if (this.elevationType == ELEVATION.PLAIN) {
            if (WorldCreatorManager.Instance.selectionComponent.selection.Count > 0) {
                ContextMenuItemSettings setMana = new ContextMenuItemSettings("Set Mana");
                setMana.onClickAction = () => WorldCreatorUI.Instance.messageBox.ShowInputMessageBox("Set Mana", "Home much mana?", WorldCreatorManager.Instance.SetManaOnTiles, UnityEngine.UI.InputField.CharacterValidation.Integer);
                settings.AddMenuItem(setMana);
            } else {
                ContextMenuItemSettings setMana = new ContextMenuItemSettings("Set Mana");
                setMana.onClickAction = () => WorldCreatorUI.Instance.messageBox.ShowInputMessageBox("Set Mana", "Home much mana?", SetManaOnTile, UnityEngine.UI.InputField.CharacterValidation.Integer);
                settings.AddMenuItem(setMana);
            }
        }
        if (this.areaOfTile != null) {
            ContextMenuItemSettings renameArea = new ContextMenuItemSettings("Rename Area");
            renameArea.onClickAction = () => worldcreator.WorldCreatorUI.Instance.messageBox.ShowInputMessageBox("Rename Area", "Rename area to what?", this.areaOfTile.SetName, UnityEngine.UI.InputField.CharacterValidation.Name);
            settings.AddMenuItem(renameArea);
        }
        if (this.landmarkOnTile != null) {
            //edit landmark info
            ContextMenuItemSettings editInfo = new ContextMenuItemSettings("Edit Landmark Info");
            editInfo.onClickAction = () => worldcreator.WorldCreatorUI.Instance.ShowLandmarkInfoEditor(this.landmarkOnTile);
            settings.AddMenuItem(editInfo);
            //end edit landmark info

            ////rename landmark
            //ContextMenuItemSettings renameLandmark = new ContextMenuItemSettings("Rename Landmark");
            //renameLandmark.onClickAction = () => worldcreator.WorldCreatorUI.Instance.messageBox.ShowInputMessageBox("Rename Landmark", "Rename landmark to what?", this.landmarkOnTile.SetName, UnityEngine.UI.InputField.CharacterValidation.Name);
            //settings.AddMenuItem(renameLandmark);
            ////end rename landmark

            //monster spawn set
            if (landmarkOnTile is MonsterSpawnerLandmark) {
                LandmarkData data = LandmarkManager.Instance.GetLandmarkData(landmarkOnTile.specificLandmarkType);
                ContextMenuItemSettings setMonsterSet = new ContextMenuItemSettings("Set Monster Spawn Choices");
                settings.AddMenuItem(setMonsterSet);
                ContextMenuSettings setMonsterSettings = new ContextMenuSettings();
                setMonsterSet.SetSubMenu(setMonsterSettings);
                for (int i = 0; i < data.monsterSets.Count; i++) {
                    MonsterSet monsterSet = data.monsterSets[i];
                    ContextMenuItemSettings setMonsterItem = new ContextMenuItemSettings(monsterSet.name);
                    setMonsterItem.onClickAction = () => (landmarkOnTile as MonsterSpawnerLandmark).SetMonsterChoices(monsterSet);
                    setMonsterSettings.AddMenuItem(setMonsterItem);
                }
            }
            //monster spawn end

            //place item
            ContextMenuItemSettings placeItem = new ContextMenuItemSettings("Place Item");
            settings.AddMenuItem(placeItem);
            ContextMenuSettings placeItemSubMenu = new ContextMenuSettings();
            placeItem.SetSubMenu(placeItemSubMenu);
            foreach (KeyValuePair<string, Item> kvp in ItemManager.Instance.allItems) {
                ContextMenuItemSettings currItemItem = new ContextMenuItemSettings(kvp.Key);
                currItemItem.onClickAction = () => landmarkOnTile.AddItem(ItemManager.Instance.CreateNewItemInstance(kvp.Key));
                placeItemSubMenu.AddMenuItem(currItemItem);
            }
            //place item end

            if (landmarkOnTile.itemsInLandmark.Count > 0) {
                //remove item
                ContextMenuItemSettings removeItem = new ContextMenuItemSettings("Remove Item");
                settings.AddMenuItem(removeItem);
                ContextMenuSettings removeItemSubMenu = new ContextMenuSettings();
                removeItem.SetSubMenu(removeItemSubMenu);
                for (int i = 0; i < landmarkOnTile.itemsInLandmark.Count; i++) {
                    Item currItem = landmarkOnTile.itemsInLandmark[i];
                    ContextMenuItemSettings currItemItem = new ContextMenuItemSettings(currItem.itemName);
                    currItemItem.onClickAction = () => landmarkOnTile.RemoveItemInLandmark(currItem);
                    removeItemSubMenu.AddMenuItem(currItemItem);
                }
                //remove item end
            }
        }

        if (this.areaOfTile != null) {
            AreaData areaData = LandmarkManager.Instance.GetAreaData(_areaOfTile.areaType);
            if (landmarkOnTile == null) {
                //landmark creation
                ContextMenuItemSettings createLandmarkItem = new ContextMenuItemSettings("Create Landmark");
                settings.AddMenuItem(createLandmarkItem);

                ContextMenuSettings createLandmarkSettings = new ContextMenuSettings();
                createLandmarkItem.SetSubMenu(createLandmarkSettings);
                LANDMARK_TYPE[] types = Utilities.GetEnumValues<LANDMARK_TYPE>();

                for (int i = 0; i < types.Length; i++) {
                    LANDMARK_TYPE landmarkType = types[i];
                    ContextMenuItemSettings createLandmark = new ContextMenuItemSettings(Utilities.NormalizeStringUpperCaseFirstLetters(landmarkType.ToString()));
                    createLandmark.onClickAction = () => worldcreator.WorldCreatorUI.Instance.editLandmarksMenu.SpawnLandmark(landmarkType, this);
                    createLandmarkSettings.AddMenuItem(createLandmark);
                }
                //end landmark creation
            } else {
                //Landmark Destruction
                ContextMenuItemSettings destroyLandmarkItem = new ContextMenuItemSettings("Destroy Landmark");
                destroyLandmarkItem.onClickAction = () => worldcreator.WorldCreatorManager.Instance.DestroyLandmarks(this);
                settings.AddMenuItem(destroyLandmarkItem);
                //end landmark destruction

                ////Monster Spawning
                //ContextMenuItemSettings spawnMonster = new ContextMenuItemSettings("Spawn Monster");
                //settings.AddMenuItem(spawnMonster);

                //ContextMenuSettings createMonsterSettings = new ContextMenuSettings();
                //spawnMonster.SetSubMenu(createMonsterSettings);

                //for (int i = 0; i < areaData.possibleMonsterSpawns.Count; i++) {
                //    MonsterPartyComponent monsterParty = areaData.possibleMonsterSpawns[i];
                //    ContextMenuItemSettings spawnMonsterItem = new ContextMenuItemSettings(monsterParty.name);
                //    spawnMonsterItem.onClickAction = () => MonsterManager.Instance.SpawnMonsterPartyOnLandmark(landmarkOnTile, monsterParty);
                //    createMonsterSettings.AddMenuItem(spawnMonsterItem);
                //}
                ////end monster spawning
            }
        }
        return settings;
    }
#else
    public ContextMenuSettings GetContextMenuSettings() {
        ContextMenuSettings settings = new ContextMenuSettings();
        if ((this.areaOfTile == null || this.areaOfTile.id != PlayerManager.Instance.player.playerArea.id) && this.landmarkOnTile == null && IsAdjacentToPlayerArea()) {
            ContextMenuItemSettings purchaseTileItem = new ContextMenuItemSettings("Purchase Tile");
            purchaseTileItem.onClickAction += () => PlayerManager.Instance.PurchaseTile(this);
            settings.AddMenuItem(purchaseTileItem);
        }
#if UNITY_EDITOR
        //if (UIManager.Instance.characterInfoUI.activeCharacter != null && UIManager.Instance.characterInfoUI.isShowing) {
        //    if (this.landmarkOnTile != null) {
        //        Character character = UIManager.Instance.characterInfoUI.activeCharacter;
        //        ContextMenuItemSettings forceActionMain = new ContextMenuItemSettings("Force Action");
        //        //forceActionMain.onClickAction += () => PlayerManager.Instance.AddTileToPlayerArea(this);
        //        bool hasDoableAction = false;
        //        ContextMenuSettings forceActionSub = new ContextMenuSettings();
        //        forceActionMain.SetSubMenu(forceActionSub);
        //        for (int i = 0; i < landmarkOnTile.landmarkObj.currentState.actions.Count; i++) {
        //            CharacterAction action = landmarkOnTile.landmarkObj.currentState.actions[i];
        //            if (action.MeetsRequirements(character.party, this._landmarkOnTile) && action.CanBeDone(landmarkOnTile.landmarkObj) && action.CanBeDoneBy(character.party, landmarkOnTile.landmarkObj)) {
        //                hasDoableAction = true;
        //                ContextMenuItemSettings doableAction = new ContextMenuItemSettings(Utilities.NormalizeStringUpperCaseFirstLetters(action.actionType.ToString()));
        //                doableAction.onClickAction = () => character.party.actionData.ForceDoAction(action, landmarkOnTile.landmarkObj);
        //                forceActionSub.AddMenuItem(doableAction);
        //            }
        //        }
        //        if (hasDoableAction) {
        //            settings.AddMenuItem(forceActionMain);
        //        }
        //    }
        //}
#endif
        return settings;
    }
#endif
    #endregion   

    #region Pathfinding
    public TravelLine CreateTravelLine(HexTile target, int numOfTicks, Character character) {
        TravelLineParent lineParent = BezierCurveManager.Instance.GetTravelLineParent(this, target);
        if (lineParent == null) {
            GameObject goParent = GameObject.Instantiate(GameManager.Instance.travelLineParentPrefab);
            lineParent = goParent.GetComponent<TravelLineParent>();
            lineParent.SetStartAndEndPositions(this, target, numOfTicks);
        }
        GameObject go = GameObject.Instantiate(GameManager.Instance.travelLinePrefab, lineParent.transform);
        go.transform.SetParent(lineParent.transform);
        TravelLine travelLine = go.GetComponent<TravelLine>();
        travelLine.SetCharacter(character);
        lineParent.AddChild(travelLine);

        TravelLineParent targetLineParent = BezierCurveManager.Instance.GetTravelLineParent(target, this);
        if (targetLineParent != null) {
            targetLineParent.transform.localPosition = new Vector3(0f, 0.3f, 0f);
        }
        return travelLine;
    }
    #endregion

    #region Beaches
    public void LoadBeaches() {
        if (_neighbourDirections == null) {
            return;
        }
        if (this.elevationType != ELEVATION.WATER) {
            topLeftBeach.gameObject.SetActive(false);
            topRightBeach.gameObject.SetActive(false);
            rightBeach.gameObject.SetActive(false);
            botRightBeach.gameObject.SetActive(false);
            botLeftBeach.gameObject.SetActive(false);
            leftBeach.gameObject.SetActive(false);
            return;
        }
        foreach (KeyValuePair<HEXTILE_DIRECTION, HexTile> kvp in _neighbourDirections) {
            bool beachState;
            if (kvp.Value != null && kvp.Value.elevationType != ELEVATION.WATER) {
                beachState = true;
            } else {
                beachState = false;
            }
            switch (kvp.Key) {
                case HEXTILE_DIRECTION.NORTH_WEST:
                    topLeftBeach.gameObject.SetActive(beachState);
                    break;
                case HEXTILE_DIRECTION.NORTH_EAST:
                    topRightBeach.gameObject.SetActive(beachState);
                    break;
                case HEXTILE_DIRECTION.EAST:
                    rightBeach.gameObject.SetActive(beachState);
                    break;
                case HEXTILE_DIRECTION.SOUTH_EAST:
                    botRightBeach.gameObject.SetActive(beachState);
                    break;
                case HEXTILE_DIRECTION.SOUTH_WEST:
                    botLeftBeach.gameObject.SetActive(beachState);
                    break;
                case HEXTILE_DIRECTION.WEST:
                    leftBeach.gameObject.SetActive(beachState);
                    break;
                case HEXTILE_DIRECTION.NONE:
                    break;
                default:
                    break;
            }
        }
    }
    #endregion

    #region Story Events
    public StoryEvent GetRandomStoryEvent() {
        List<StoryEvent> pool = StoryEventsManager.Instance.GetPossibleEventsForTile(this);
        if (pool.Count > 0) {
            return pool[UnityEngine.Random.Range(0, pool.Count)];
        }
        return null;
    }
    #endregion

    #region Region
    public void SetRegion(Region region) {
        this.region = region;
    }
    #endregion
}
