using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathFind;
using System.Linq;
using Inner_Maps;
using SpriteGlow;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HexTile : MonoBehaviour, IHasNeighbours<HexTile> {

    public HexTileData data;
    private Settlement _settlementOfTile;
    public SpriteRenderer spriteRenderer;
    private bool _isCorrupted = false;
    
    [Space(10)]
    [Header("Tile Visuals")]
    [SerializeField] private GameObject _centerPiece;
    [SerializeField] private GameObject _highlightGO;
    [SerializeField] private GameObject _hoverHighlightGO;
    [SerializeField] private Animator baseTileAnimator;
    [SerializeField] private SpriteRenderer emptyBuildingSpotGO;
    [SerializeField] private SpriteRenderer currentlyBuildingSpotGO;

    [Space(10)]
    [Header("Tile Borders")]
    [SerializeField] private SpriteRenderer topLeftBorder;
    [SerializeField] private SpriteRenderer leftBorder;
    [SerializeField] private SpriteRenderer botLeftBorder;
    [SerializeField] private SpriteRenderer botRightBorder;
    [SerializeField] private SpriteRenderer rightBorder;
    [SerializeField] private SpriteRenderer topRightBorder;

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

    //properties
    public BaseLandmark landmarkOnTile { get; private set; }
    public Region region { get; private set; }
    public TileFeatureComponent featureComponent { get; private set; }
    public Settlement settlementOnTile { get; private set; }
    public BuildingSpot[] ownedBuildSpots { get; private set; }
    public List<HexTile> AllNeighbours { get; set; }
    public List<HexTile> ValidTiles { get { return AllNeighbours.Where(o => o.elevationType != ELEVATION.WATER && o.elevationType != ELEVATION.MOUNTAIN).ToList(); } }
    private int _uncorruptibleLandmarkNeighbors = 0; //if 0, can be corrupted, otherwise, cannot be corrupted
    private Dictionary<HEXTILE_DIRECTION, HexTile> _neighbourDirections;
    private GameObject _spawnedTendril = null;

    public Sprite baseSprite { get; private set; }

    #region getters/setters
    public int id => data.id;
    public int xCoordinate => data.xCoordinate;
    public int yCoordinate => data.yCoordinate;
    public string tileName => data.tileName;
    public string thisName => data.tileName;
    public float elevationNoise => data.elevationNoise;
    public float moistureNoise => data.moistureNoise;
    public float temperature => data.temperature;
    public BIOMES biomeType => data.biomeType;
    public ELEVATION elevationType => data.elevationType;
    public string locationName => $"({xCoordinate.ToString()}, {yCoordinate.ToString()})";
    private GameObject centerPiece => this._centerPiece;
    private GameObject highlightGO => this._highlightGO;
    private Dictionary<HEXTILE_DIRECTION, HexTile> neighbourDirections => _neighbourDirections;
    public bool isCorrupted => _isCorrupted;
    #endregion

    private void Awake() {
        _structureAnimatorSpriteRenderer = structureAnimation.gameObject.GetComponent<SpriteRenderer>();
        _highlightGOSpriteRenderer = highlightGO.GetComponent<SpriteRenderer>();
        _hoverHighlightSpriteRenderer = _hoverHighlightGO.GetComponent<SpriteRenderer>();
        Random.ColorHSV();
    }
    public void Initialize() {
        featureComponent = new TileFeatureComponent();
    }

    #region Elevation Functions
    internal void SetElevation(ELEVATION elevationType) {
        data.elevationType = elevationType;
    }
    #endregion

    #region Biome Functions
    internal void SetBiome(BIOMES biome) {
        data.biomeType = biome;
    }
    #endregion

    #region Landmarks
    private void SetLandmarkOnTile(BaseLandmark landmarkOnTile, bool addFeatures = true) {
        this.landmarkOnTile = landmarkOnTile;
        // if (addFeatures) {
        //     landmarkOnTile?.AddFeaturesToRegion();
        // }
        region.OnMainLandmarkChanged();
    }
    public BaseLandmark CreateLandmarkOfType(LANDMARK_TYPE landmarkType, bool addFeatures) {
        LandmarkData data = LandmarkManager.Instance.GetLandmarkData(landmarkType);
        //SetLandmarkOnTile(new BaseLandmark(this, landmarkType));
        SetLandmarkOnTile(LandmarkManager.Instance.CreateNewLandmarkInstance(this, landmarkType), addFeatures);
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
    public BaseLandmark CreateLandmarkOfType(SaveDataLandmark saveData) {
        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(saveData.landmarkType);
        //SetLandmarkOnTile(new BaseLandmark(this, saveData));
        SetLandmarkOnTile(LandmarkManager.Instance.CreateNewLandmarkInstance(this, saveData), false);
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
        GameObject landmarkGO = GameObject.Instantiate(LandmarkManager.Instance.GetLandmarkGO(), structureParentGO.transform) as GameObject;
        RACE race = RACE.NONE;
        if (region != null) {
            if (region.locationType == LOCATION_TYPE.ELVEN_SETTLEMENT) {
                race = RACE.ELVES;
            } else if (region.locationType == LOCATION_TYPE.HUMAN_SETTLEMENT) {
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
        LandmarkData _data = LandmarkManager.Instance.GetLandmarkData(landmarkOnTile.specificLandmarkType);
        List<LandmarkStructureSprite> landmarkTileSprites = LandmarkManager.Instance.GetLandmarkTileSprites(this, landmarkOnTile.specificLandmarkType);
        if (_data.minimumTileCount > 1) {
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
        region.OnMainLandmarkChanged();
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

        if (structureTint.sprite != null) {
            SetStructureTint(Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f));
        } else {
            structureTint.color = Color.white;
        }

        if (sprites.animation == null) {
            mainStructure.enabled = true;
            structureAnimation.gameObject.SetActive(false);
        } else {
            mainStructure.enabled = landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.MONSTER_LAIR; //SPECIAL CASE FOR MONSTER LAIR
            structureAnimation.gameObject.SetActive(true);
            structureAnimation.runtimeAnimatorController = sprites.animation;
        }
    }
    private void HideLandmarkTileSprites() {
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
    public bool HasNeighbourWithElevation(ELEVATION elevation) {
        for (int i = 0; i < AllNeighbours.Count; i++) {
            HexTile neighbour = AllNeighbours[i];
            if (neighbour.elevationType == elevation) {
                return true;
            }
        }
        return false;
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
    private void UpdateSortingOrder() {
        int sortingOrder = spriteRenderer.sortingOrder;
        _hoverHighlightSpriteRenderer.sortingOrder = sortingOrder + 1;
        _highlightGOSpriteRenderer.sortingOrder = sortingOrder + 1;

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

        emptyBuildingSpotGO.sortingOrder = sortingOrder + 1;
        //currentlyBuildingSpotGO.sortingOrder = sortingOrder + 1;

        mainStructure.sortingOrder = sortingOrder + 3;
        structureTint.sortingOrder = sortingOrder + 4;
        _structureAnimatorSpriteRenderer.sortingOrder = sortingOrder + 5;
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
    private SpriteRenderer[] GetAllBorders() {
        SpriteRenderer[] border = new[] {
            topLeftBorder, topRightBorder, rightBorder, botRightBorder, botLeftBorder, leftBorder
        };
        return border;
    }
    public void SetOutlineState(bool state) {
        SpriteRenderer[] borders = GetAllBorders();
        for (int i = 0; i < borders.Length; i++) {
            SpriteRenderer _renderer = borders[i];
            _renderer.gameObject.SetActive(state);
        }
    }
    internal void DeactivateCenterPiece() {
        centerPiece.SetActive(false);
    }
    internal void SetBaseSprite(Sprite baseSprite) {
        this.baseSprite = baseSprite;
        spriteRenderer.sprite = baseSprite;
        RuntimeAnimatorController _animation;
        if (Biomes.Instance.TryGetTileSpriteAnimation(baseSprite, out _animation)) {
            baseTileAnimator.runtimeAnimatorController = _animation;
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
    }
    public void UpdateBuildSprites() {
        if (region.demonicBuildingData.landmarkType != LANDMARK_TYPE.NONE) {
            //currently building at tile.
            currentlyBuildingSpotGO.gameObject.SetActive(true);
            emptyBuildingSpotGO.gameObject.SetActive(false);
        } else {
            emptyBuildingSpotGO.gameObject.SetActive(true);
            currentlyBuildingSpotGO.gameObject.SetActive(false);
        }
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
    }
    public void Occupy() {
    }
    public void Unoccupy() {
    }
    #endregion

    #region Monobehaviour Functions
    public void LeftClick() {
        if (UIManager.Instance.IsMouseOnUI() || UIManager.Instance.IsConsoleShowing() || CameraMove.Instance.isDragging) {
            return;
        }
        //StartCorruptionAnimation();
        Messenger.Broadcast(Signals.TILE_LEFT_CLICKED, this);
        if (PlayerManager.Instance.isChoosingStartingTile) {
            return;
        }
        if (region.coreTile == this) {
            UIManager.Instance.ShowRegionInfo(this.region);
        } else {
            Messenger.Broadcast(Signals.HIDE_MENUS);
        }
    }
    public void RightClick() {
        if (UIManager.Instance.IsMouseOnUI() || UIManager.Instance.IsConsoleShowing()) {
            return;
        }
        Messenger.Broadcast(Signals.TILE_RIGHT_CLICKED, this);
    }
    private void MouseOver() {
        if (this.landmarkOnTile != null) {
            _hoverHighlightGO.SetActive(true);
        }
        Messenger.Broadcast(Signals.TILE_HOVERED_OVER, this);
    }
    private void MouseExit() {
        if (this.landmarkOnTile != null) {
            _hoverHighlightGO.SetActive(false);
        }
        UIManager.Instance.HideSmallInfo();
        Messenger.Broadcast(Signals.TILE_HOVERED_OUT, this);
    }
    private void DoubleLeftClick() {
        if (UIManager.Instance.IsMouseOnUI() || UIManager.Instance.IsConsoleShowing()) {
            return;
        }
        // if(region.settlement != null && region.settlement != PlayerManager.Instance.player.playerSettlement) {
        //     InnerMapManager.Instance.TryShowLocationMap(region.settlement);
        // }
        if (region != null) {
            InnerMapManager.Instance.TryShowLocationMap(region);
        }
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
        if (ped.pointerCurrentRaycast.gameObject.CompareTag("Avatar")) {
            OnPointerExit(bed);
            return;
        }
        MouseOver();
    }
    public void OnPointerExit(BaseEventData bed) {
        MouseExit();
    }
    public void CenterCameraHere() {
        if (InnerMapManager.Instance.isAnInnerMapShowing) {
            InnerMapManager.Instance.HideAreaMap();
            UIManager.Instance.OnCameraOutOfFocus();
        }
        CameraMove.Instance.CenterCameraOn(gameObject);
    }
    #endregion

    #region For Testing
    [Space(10)]
    [Header("For Testing")]
    [SerializeField] private int range = 0;
    List<HexTile> tiles = new List<HexTile>();
    private SpriteRenderer _hoverHighlightSpriteRenderer;
    private SpriteRenderer _highlightGOSpriteRenderer;
    private SpriteRenderer _structureAnimatorSpriteRenderer;
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
    public override string ToString() {
        return $"{this.locationName} - {landmarkOnTile?.specificLandmarkType.ToString() ?? "No Landmark"} - {region?.name ?? "No Region"}";
    }
    public void ShowTileInfo() {
        if (region != null) {
            UIManager.Instance.ShowSmallInfo("Double click to view.", region.name);
        }
    }
    #endregion

    #region Corruption
    public void SetCorruption(bool state) {
        if(_isCorrupted != state) {
            _isCorrupted = state;
            Biomes.Instance.UpdateTileSprite(this, spriteRenderer.sortingOrder);
        }
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
        //return region.CanBeInvaded();
    }
    #endregion

    #region Settlement
    public void SetSettlementOnTile(Settlement settlement) {
        settlementOnTile = settlement;
    }
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

    #region Region
    public void SetRegion(Region region) {
        this.region = region;
    }
    #endregion

    #region Inner Map
    public void SetOwnedBuildSpot(BuildingSpot[] spot) {
        ownedBuildSpots = spot;
    }
    #endregion
}
