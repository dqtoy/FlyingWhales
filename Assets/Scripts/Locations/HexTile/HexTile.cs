using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathFind;
using System.Linq;
using Actionables;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using JetBrains.Annotations;
using SpriteGlow;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UtilityScripts;

public class HexTile : MonoBehaviour, IHasNeighbours<HexTile>, IPlayerActionTarget, ISelectable {

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
    [SerializeField] private SpriteGlowEffect topLeftBorderGlow;
    [SerializeField] private SpriteGlowEffect leftBorderGlow;
    [SerializeField] private SpriteGlowEffect botLeftBorderGlow;
    [SerializeField] private SpriteGlowEffect botRightBorderGlow;
    [SerializeField] private SpriteGlowEffect rightBorderGlow;
    [SerializeField] private SpriteGlowEffect topRightBorderGlow;

    [Space(10)]
    [Header("Structure Objects")]
    [SerializeField] private GameObject structureParentGO;
    [SerializeField] private SpriteRenderer mainStructure;
    [SerializeField] private SpriteRenderer structureTint;
    [SerializeField] private Animator structureAnimation;

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
    public bool isCurrentlyBeingCorrupted { get; private set; }
    public List<LocationGridTile> locationGridTiles { get; private set; }
    public Sprite baseSprite { get; private set; }
    public Vector2 selectableSize { get; private set; }

    private List<LocationGridTile> corruptedTiles;
    private int _uncorruptibleLandmarkNeighbors = 0; //if 0, can be corrupted, otherwise, cannot be corrupted
    private Dictionary<HEXTILE_DIRECTION, HexTile> _neighbourDirections;

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
    private string locationName => $"({xCoordinate.ToString()}, {yCoordinate.ToString()})";
    private GameObject centerPiece => _centerPiece;
    private GameObject highlightGO => _highlightGO;
    private Dictionary<HEXTILE_DIRECTION, HexTile> neighbourDirections => _neighbourDirections;
    public bool isCorrupted => _isCorrupted;
    public Vector3 worldPosition {
        get {
            Vector2 pos = ownedBuildSpots[0].spotItem.transform.position;
            pos.x += 3.5f;
            pos.y += 3.5f;
            return pos;
        }
    }
    #endregion

    private void Awake() {
        _highlightSpriteRenderer = _highlightGO.GetComponent<SpriteRenderer>();
        _structureAnimatorSpriteRenderer = structureAnimation.gameObject.GetComponent<SpriteRenderer>();
        _highlightGOSpriteRenderer = highlightGO.GetComponent<SpriteRenderer>();
        _hoverHighlightSpriteRenderer = _hoverHighlightGO.GetComponent<SpriteRenderer>();
        Random.ColorHSV();
        ConstructDefaultActions();
    }
    public void Initialize() {
        featureComponent = new TileFeatureComponent();
        selectableSize = new Vector2Int(12, 12);
        Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);
    }
    private void OnGameLoaded() {
        Messenger.RemoveListener(Signals.GAME_LOADED, OnGameLoaded);
        SubscribeListeners();
        SetBordersState(false, false, Color.red);
        if (landmarkOnTile != null && landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.VILLAGE) {
            CheckIfStructureVisualsAreStillValid();    
        }
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
    private void SetLandmarkOnTile(BaseLandmark landmarkOnTile) {
        this.landmarkOnTile = landmarkOnTile;
    }
    public BaseLandmark CreateLandmarkOfType(LANDMARK_TYPE landmarkType) {
        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(landmarkType);
        //SetLandmarkOnTile(new BaseLandmark(this, landmarkType));
        SetLandmarkOnTile(LandmarkManager.Instance.CreateNewLandmarkInstance(this, landmarkType));
        //Create Landmark Game Object on tile
        GameObject landmarkGO = CreateLandmarkVisual(landmarkType, landmarkOnTile, landmarkData);
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
        SetLandmarkOnTile(LandmarkManager.Instance.CreateNewLandmarkInstance(this, saveData));
        //Create Landmark Game Object on tile
        GameObject landmarkGO = CreateLandmarkVisual(saveData.landmarkType, landmarkOnTile, landmarkData);
        if (landmarkGO != null) {
            landmarkGO.transform.localPosition = Vector3.zero;
            landmarkGO.transform.localScale = Vector3.one;
            landmarkOnTile.SetLandmarkObject(landmarkGO.GetComponent<LandmarkVisual>());
        }
        return landmarkOnTile;
    }
    private GameObject CreateLandmarkVisual(LANDMARK_TYPE landmarkType, BaseLandmark landmark, LandmarkData data) {
        GameObject landmarkGO = Instantiate(LandmarkManager.Instance.GetLandmarkGO(), structureParentGO.transform) as GameObject;
        RACE race = RACE.NONE;
        if (region != null) {
            if (region.locationType == LOCATION_TYPE.ELVEN_SETTLEMENT) {
                race = RACE.ELVES;
            } else if (region.locationType == LOCATION_TYPE.HUMAN_SETTLEMENT) {
                race = RACE.HUMANS;
            }
        }
        List<LandmarkStructureSprite> landmarkTileSprites = LandmarkManager.Instance.GetLandmarkTileSprites(this, landmarkType, race);
        if (landmarkTileSprites == null || landmarkTileSprites.Count == 0) {
            //DeactivateCenterPiece();
            HideLandmarkTileSprites();
            landmarkGO.GetComponent<LandmarkVisual>().SetIconState(true);
        } else {
            SetLandmarkTileSprite(landmarkTileSprites[Random.Range(0, landmarkTileSprites.Count)]);
            landmarkGO.GetComponent<LandmarkVisual>().SetIconState(false);
        }
        if (settlementOnTile != null && settlementOnTile.owner != null) {
            settlementOnTile.TintStructures(settlementOnTile.owner.factionColor);    
        }
        return landmarkGO;
    }
    public void UpdateStructureVisuals(LANDMARK_TYPE landmarkType) {
        LandmarkData _data = LandmarkManager.Instance.GetLandmarkData(landmarkType);
        List<LandmarkStructureSprite> landmarkTileSprites = LandmarkManager.Instance.GetLandmarkTileSprites(this, landmarkOnTile.specificLandmarkType);
        if (landmarkTileSprites == null || landmarkTileSprites.Count == 0) {
            //DeactivateCenterPiece();
            HideLandmarkTileSprites();
            landmarkOnTile.landmarkVisual.SetIconState(true);
        } else {
            SetLandmarkTileSprite(landmarkTileSprites[Random.Range(0, landmarkTileSprites.Count)]);
            landmarkOnTile.landmarkVisual.SetIconState(false);
        }
        
    }
    public BaseLandmark LoadLandmark(BaseLandmark landmark) {
        GameObject landmarkGO = null;
        //Create Landmark Game Object on tile
        landmarkGO = Instantiate(LandmarkManager.Instance.GetLandmarkGO(), structureParentGO.transform) as GameObject;
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
        Destroy(landmarkOnTile.landmarkVisual.gameObject);
    }
    public void SetLandmarkTileSprite(LandmarkStructureSprite sprites) {
        mainStructure.sprite = sprites.mainSprite;
        structureTint.sprite = sprites.tintSprite;
        mainStructure.gameObject.SetActive(true);
        structureTint.gameObject.SetActive(true);
        
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
        structureTint.color = color;
        Debug.Log($"Tinted structure on {ToString()}");
    }
    #endregion

    #region Tile Utilities
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
                    }
                }
                if (i == range - 1 && isOnlyOuter) {
                    return tilesToAdd;
                }
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
    public bool HasNeighbourFromOtherRegion() {
        for (int i = 0; i < AllNeighbours.Count; i++) {
            HexTile currNeighbour = AllNeighbours[i];
            if (currNeighbour.region != region) {
                return true;
            }
        }
        return false;
    }
    public bool TryGetDifferentRegionNeighbours(out List<Region> regions) {
        regions = new List<Region>();
        for (int i = 0; i < AllNeighbours.Count; i++) {
            HexTile currNeighbour = AllNeighbours[i];
            if (currNeighbour.region != region) {
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
    public string GetDisplayName() {
        if (settlementOnTile != null) {
            return settlementOnTile.name;
        } else if (landmarkOnTile != null) {
            return landmarkOnTile.landmarkName;
        } else {
            string displayName = string.Empty;
            if (isCorrupted) {
                displayName = "Corrupted ";
            }
            displayName += $"{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(biomeType.ToString())} " +
                           $"{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(elevationType.ToString())}";
            return displayName;
        }
    }
    public string GetSubName() {
        if (landmarkOnTile != null) {
            return UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(landmarkOnTile.specificLandmarkType.ToString());
        }
        return string.Empty;
    }
    #endregion

    #region Pathfinding
    public void FindNeighbours(HexTile[,] gameBoard) {
        _neighbourDirections = new Dictionary<HEXTILE_DIRECTION, HexTile>();
        var neighbours = new List<HexTile>();

        List<Point> possibleExits;

        if ((yCoordinate % 2) == 0) {
            possibleExits = UtilityScripts.Utilities.EvenNeighbours;
        } else {
            possibleExits = UtilityScripts.Utilities.OddNeighbours;
        }

        for (int i = 0; i < possibleExits.Count; i++) {
            int neighbourCoordinateX = xCoordinate + possibleExits[i].X;
            int neighbourCoordinateY = yCoordinate + possibleExits[i].Y;
            if (neighbourCoordinateX >= 0 && neighbourCoordinateX < gameBoard.GetLength(0) && neighbourCoordinateY >= 0 && neighbourCoordinateY < gameBoard.GetLength(1)) {
                HexTile currNeighbour = gameBoard[neighbourCoordinateX, neighbourCoordinateY];
                if (currNeighbour != null) {
                    neighbours.Add(currNeighbour);
                }
            }

        }
        AllNeighbours = neighbours;

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
            possibleExits = UtilityScripts.Utilities.EvenNeighbours;
        } else {
            possibleExits = UtilityScripts.Utilities.OddNeighbours;
        }

        for (int i = 0; i < possibleExits.Count; i++) {
            int neighbourCoordinateX = xCoordinate + possibleExits[i].X;
            int neighbourCoordinateY = yCoordinate + possibleExits[i].Y;
            HexTile neighbour = GridMap.Instance.GetTileFromCoordinates(neighbourCoordinateX, neighbourCoordinateY);
            if (neighbour != null) {
                neighbours.Add(neighbour);
            }

        }
        AllNeighbours = neighbours;

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
    private HEXTILE_DIRECTION GetNeighbourDirection(HexTile neighbour) {
        if (neighbour == null) {
            return HEXTILE_DIRECTION.NONE;
        }
        if (!AllNeighbours.Contains(neighbour)) {
            throw new System.Exception(neighbour.name + " is not a neighbour of " + name);
        }
        int thisXCoordinate = xCoordinate;
        int thisYCoordinate = yCoordinate;
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

        topLeftBorder.sortingOrder = sortingOrder + 2;
        topRightBorder.sortingOrder = sortingOrder + 2;
        leftBorder.sortingOrder = sortingOrder + 2;
        botLeftBorder.sortingOrder = sortingOrder + 2;
        botRightBorder.sortingOrder = sortingOrder + 2;
        rightBorder.sortingOrder = sortingOrder + 2;

        emptyBuildingSpotGO.sortingOrder = sortingOrder + 1;

        mainStructure.sortingOrder = sortingOrder + 5;
        structureTint.sortingOrder = sortingOrder + 6;
        _structureAnimatorSpriteRenderer.sortingOrder = sortingOrder + 7;
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
    public void SetBordersState(bool state, bool glowState, Color color) {
        topLeftBorder.gameObject.SetActive(state);
        botLeftBorder.gameObject.SetActive(state);
        topRightBorder.gameObject.SetActive(state);
        botRightBorder.gameObject.SetActive(state);
        leftBorder.gameObject.SetActive(state);
        rightBorder.gameObject.SetActive(state);
        
        topLeftBorderGlow.enabled = glowState;
        botLeftBorderGlow.enabled = glowState;
        topRightBorderGlow.enabled = glowState;
        botRightBorderGlow.enabled = glowState;
        leftBorderGlow.enabled = glowState;
        rightBorderGlow.enabled = glowState;
        
        SetBorderColor(color);
    }
    private void SetBorderColor(Color color) {
        topLeftBorder.color = color;
        botLeftBorder.color = color;
        topRightBorder.color = color;
        botRightBorder.color = color;
        leftBorder.color = color;
        rightBorder.color = color;
        
        topLeftBorderGlow.GlowColor = color;
        botLeftBorderGlow.GlowColor = color;
        topRightBorderGlow.GlowColor = color;
        botRightBorderGlow.GlowColor = color;
        leftBorderGlow.GlowColor = color;
        rightBorderGlow.GlowColor = color;
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
        GetComponent<Collider2D>().enabled = false;
        Collider[] colliders = GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++) {
            colliders[i].enabled = false;
        }
    }
    private UIMenu GetMenuToShowWhenTileIsClicked() {
        if (region != null) {
            //if region info ui is showing, show tile info ui
            if (UIManager.Instance.regionInfoUI.isShowing) {
                if (UIManager.Instance.regionInfoUI.activeRegion == region) {
                    return UIManager.Instance.hexTileInfoUI;    
                } else {
                    return UIManager.Instance.regionInfoUI;
                }
            } else if (UIManager.Instance.hexTileInfoUI.isShowing) {
                if (UIManager.Instance.hexTileInfoUI.currentlyShowingHexTile.region == region) {
                    if (UIManager.Instance.hexTileInfoUI.currentlyShowingHexTile == this) {
                        return UIManager.Instance.regionInfoUI;
                    } else {
                        return UIManager.Instance.hexTileInfoUI;
                    }
                } else {
                    return UIManager.Instance.regionInfoUI;    
                }
            } else {
                return UIManager.Instance.regionInfoUI;
            }
        }
        return null;
    }
    #endregion

    #region Monobehaviour Functions
    private void LeftClick() {
        if (UIManager.Instance.IsMouseOnUI() || UIManager.Instance.IsConsoleShowing() || CameraMove.Instance.isDragging) {
            return;
        }
        Messenger.Broadcast(Signals.TILE_LEFT_CLICKED, this);
        UIMenu menuToShow = GetMenuToShowWhenTileIsClicked();
        if (menuToShow != null) {
            if (menuToShow is RegionInfoUI) {
                UIManager.Instance.ShowRegionInfo(region);
            } else if (menuToShow is HextileInfoUI) {
                UIManager.Instance.ShowHexTileInfo(this);
            }
        } else {
            Messenger.Broadcast(Signals.HIDE_MENUS);
        }
        MouseOver();
    }
    private void RightClick() {
        if (UIManager.Instance.IsMouseOnUI() || UIManager.Instance.IsConsoleShowing()) {
            return;
        }
        Messenger.Broadcast(Signals.TILE_RIGHT_CLICKED, this);
    }
    private void MouseOver() {
        UIMenu menuToOpen = GetMenuToShowWhenTileIsClicked();
        if (menuToOpen is RegionInfoUI) {
            region.ShowBorders(Color.red);
        } else if (menuToOpen is HextileInfoUI) {
            SetBordersState(true, false, Color.red);
        }
        Messenger.Broadcast(Signals.TILE_HOVERED_OVER, this);
    }
    private void MouseExit() {
        UIMenu menuToOpen = GetMenuToShowWhenTileIsClicked();
        if (menuToOpen is RegionInfoUI) {
            region.HideBorders();
        } else if (menuToOpen is HextileInfoUI) {
            SetBordersState(false, false, Color.red);
        }
        // UIManager.Instance.HideSmallInfo();
        Messenger.Broadcast(Signals.TILE_HOVERED_OUT, this);
    }
    private void DoubleLeftClick() {
        if (UIManager.Instance.IsMouseOnUI() || UIManager.Instance.IsConsoleShowing()) {
            return;
        }
        if (region != null) {
            InnerMapManager.Instance.TryShowLocationMap(region);
            InnerMapCameraMove.Instance.CenterCameraOnTile(this);
        }
    }
    public void PointerClick(BaseEventData bed) {
        PointerEventData ped = bed as PointerEventData;
        if (ped.clickCount >= 2) {
            if (ped.button == PointerEventData.InputButton.Left) {
                DoubleLeftClick();
            }
        } else if (ped.clickCount == 1) {
            if (ped.button == PointerEventData.InputButton.Left) {
                LeftClick();
            } else if (ped.button == PointerEventData.InputButton.Right) {
                RightClick();
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
    private SpriteRenderer _highlightSpriteRenderer;
    [ContextMenu("Show Tiles In Range")]
    public void ShowTilesInRange() {
        for (int i = 0; i < tiles.Count; i++) {
            tiles[i].spriteRenderer.color = Color.white;
        }
        tiles.Clear();
        tiles.AddRange(GetTilesInRange(range));
        for (int i = 0; i < tiles.Count; i++) {
            tiles[i].spriteRenderer.color = Color.magenta;
        }
    }
    public override string ToString() {
        return $"{locationName} - {landmarkOnTile?.specificLandmarkType.ToString() ?? "No Landmark"} - {region?.name ?? "No Region"}";
    }
    public void ShowTileInfo() {
        string summary = $"{ToString()}";
        summary += "\nLeft Most: " + (region.GetLeftMostTile()?.ToString() ?? "Null");
        summary += "\nRight Most: " + (region.GetRightMostTile()?.ToString() ?? "Null");
        summary += "\nFeatures:";
        for (int i = 0; i < featureComponent.features.Count; i++) {
            TileFeature feature = featureComponent.features[i];
            summary += $"{feature.name}, ";
        }
        summary += "\nLeft Most Rows:";
        List<int> leftMostRows = region.GetLeftMostRows();
        for (int i = 0; i < leftMostRows.Count; i++) {
            summary += $"{leftMostRows[i].ToString()}, ";
        }
        summary += "\nRight Most Rows:";
        List<int> rightMostRows = region.GetRightMostRows();
        for (int i = 0; i < rightMostRows.Count; i++) {
            summary += $"{rightMostRows[i].ToString()}, ";
        }
        UIManager.Instance.ShowSmallInfo(summary);
        
    }
    #endregion

    #region Corruption
    public void SetCorruption(bool state) {
        if(_isCorrupted != state) {
            _isCorrupted = state;
            Biomes.Instance.UpdateTileSprite(this, spriteRenderer.sortingOrder);
            for (int i = 0; i < AllNeighbours.Count; i++) {
                HexTile neighbour = AllNeighbours[i];
                if (neighbour.isCorrupted == false && neighbour.isCurrentlyBeingCorrupted == false) {
                    neighbour.CheckForCorruptAction();
                }
            }
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
    private void CheckForCorruptAction() {
        PlayerAction existingCorruptAction = GetPlayerAction("Corrupt");
        if (CanBeCorrupted()) {
            if (existingCorruptAction == null) {
                PlayerAction corruptAction = new PlayerAction("Corrupt", CanBeCorrupted, StartPerTickCorruption);
                AddPlayerAction(corruptAction);
            }
        } else {
            if (existingCorruptAction != null) {
                RemovePlayerAction(existingCorruptAction);    
            }
        }
    }
    private bool CanBeCorrupted() {
        if (isCorrupted) {
            return false; //already corrupted.
        }
        if (isCurrentlyBeingCorrupted) {
            return false; //already being corrupted.
        }
        if (settlementOnTile != null) {
            return false; //disabled corruption of NPC settlements for now.
        }
        if (PlayerManager.Instance.player.mana < EditableValuesManager.Instance.corruptTileManaCost) {
            return false;
        }
        //if it has any build spots that have a blueprint on them, do not allow
        for (int i = 0; i < ownedBuildSpots.Length; i++) {
            BuildingSpot spot = ownedBuildSpots[i];
            if (spot.hasBlueprint) {
                return false;
            }
        }
        for (int i = 0; i < AllNeighbours.Count; i++) {
            HexTile neighbour = AllNeighbours[i];
            if (neighbour.isCorrupted) {
                return true;
            }
        }
        return false;
    }
    private void StartPerTickCorruption() {
        // corruptedTiles = new List<LocationGridTile>();
        // LocationGridTile startTile = GetGridTileNearestToCorruption();
        // startTile.CorruptTile();
        // corruptedTiles.Add(startTile);
        // isCurrentlyBeingCorrupted = true;
        PlayerManager.Instance.player.AdjustMana(-EditableValuesManager.Instance.corruptTileManaCost);
        // Messenger.AddListener(Signals.TICK_STARTED, PerTickCorruption);
        InstantlyCorruptAllOwnedInnerMapTiles();
        OnCorruptSuccess();
    }
    private void PerTickCorruption() {
        List<LocationGridTile> newTilesToCorrupt = new List<LocationGridTile>();
        for (int i = 0; i < corruptedTiles.Count; i++) {
            LocationGridTile tile = corruptedTiles[i];
            List<LocationGridTile> neighbours = CollectionUtilities.Shuffle(tile.FourNeighbours());
            for (int j = 0; j < neighbours.Count; j++) {
                LocationGridTile neighbour = neighbours[j];
                if (neighbour.isCorrupted == false 
                    && newTilesToCorrupt.Contains(neighbour) == false
                    && locationGridTiles.Contains(neighbour)) {
                    newTilesToCorrupt.Add(neighbour);
                    break;
                }
            }
        }
        for (int i = 0; i < newTilesToCorrupt.Count; i++) {
            LocationGridTile tile = newTilesToCorrupt[i];
            tile.CorruptTile();
            corruptedTiles.Add(tile);
        }
        
        if (corruptedTiles.Count == locationGridTiles.Count) {
            //corruption finished
            OnCorruptSuccess();
        }
    }
    public void InstantlyCorruptAllOwnedInnerMapTiles() {
        // List<LocationGridTile> allTilesToConsider = new List<LocationGridTile>(locationGridTiles);
        // for (int i = 0; i < AllNeighbours.Count; i++) {
        //     HexTile neighbour = AllNeighbours[i];
        //     if (neighbour.region == region && neighbour.isCorrupted) {
        //         allTilesToConsider.AddRange(neighbour.locationGridTiles);
        //     }
        // }
        //
        // LocationGridTile[,] tileMap =
        //     Cellular_Automata.CellularAutomataGenerator.ConvertListToGridMap(allTilesToConsider);
        //
        // int width = tileMap.GetUpperBound(0) + 1;
        // int height = tileMap.GetUpperBound(1) + 1;
        //
        // int[,] cellAutomataMap = Cellular_Automata.CellularAutomataGenerator.GenerateMap(tileMap, locationGridTiles, 1, 30, edgesAreAlwaysWalls: false);
        //
        // for (int x = 0; x < width; x++) {
        //     for (int y = 0; y < height; y++) {
        //         int cellMapValue = cellAutomataMap[x, y];
        //         LocationGridTile gridTile = tileMap[x, y];
        //         if (gridTile != null && locationGridTiles.Contains(gridTile)) {
        //             if (cellMapValue == 0) {
        //                 gridTile.CorruptTile();
        //             } else {
        //                 gridTile.RevertToPreviousGroundVisual();
        //                 gridTile.CreateSeamlessEdgesForSelfAndNeighbours();
        //             }    
        //         }
        //     }    
        // }
        
        for (int i = 0; i < locationGridTiles.Count; i++) {
            LocationGridTile tile = locationGridTiles[i];
            tile.CorruptTile();
        }
    }
    private LocationGridTile GetGridTileNearestToCorruption() {
        HexTile corruptedNeighbour = GetCorruptedNeighbour();
        HEXTILE_DIRECTION corruptedDirection = GetNeighbourDirection(corruptedNeighbour);
        LocationGridTile compareTo = null;
        int minX = locationGridTiles.Min(t => t.localPlace.x);
        int maxX = locationGridTiles.Max(t => t.localPlace.x);
        int minY = locationGridTiles.Min(t => t.localPlace.y);
        int maxY = locationGridTiles.Max(t => t.localPlace.y);

        int differenceY = (maxY - minY) + 1;
        int midY = minY + (differenceY / 2);
        
        switch (corruptedDirection) {
            case HEXTILE_DIRECTION.EAST:
                compareTo = region.innerMap.map[maxX, midY];
                break;
            case HEXTILE_DIRECTION.WEST:
                compareTo = region.innerMap.map[minX, midY];
                break;
            case HEXTILE_DIRECTION.NORTH_EAST:
                compareTo = region.innerMap.map[maxX, maxY];
                break;
            case HEXTILE_DIRECTION.NORTH_WEST:
                compareTo = region.innerMap.map[minX, maxY];
                break;
            case HEXTILE_DIRECTION.SOUTH_EAST:
                compareTo = region.innerMap.map[maxX, minY];
                break;
            case HEXTILE_DIRECTION.SOUTH_WEST:
                compareTo = region.innerMap.map[minX, minY];
                break;
        }
        return compareTo;
    }
    private HexTile GetCorruptedNeighbour() {
        for (int i = 0; i < AllNeighbours.Count; i++) {
            HexTile tile = AllNeighbours[i];
            if (tile.isCorrupted) {
                return tile;
            }
        }
        return null;
    }
    private void OnCorruptSuccess() {
        PlayerManager.Instance.player.playerSettlement.AddTileToSettlement(this);
        // Messenger.RemoveListener(Signals.TICK_STARTED, PerTickCorruption);
        // isCurrentlyBeingCorrupted = false;
        
        //remove features
        featureComponent.RemoveAllFeaturesExcept(this, TileFeatureDB.Wood_Source_Feature);
        
        RemovePlayerAction(GetPlayerAction("Corrupt"));
        if (CanBuildDemonicStructure()) {
            PlayerAction buildAction = new PlayerAction("Build Demonic Structure", CanBuildDemonicStructure, OnClickBuild);
            AddPlayerAction(buildAction);
        }
    }
    #endregion

    #region Settlement
    public void SetSettlementOnTile(Settlement settlement) {
        settlementOnTile = settlement;
        landmarkOnTile?.nameplate.UpdateVisuals();
        CheckForCorruptAction();
    }
    #endregion

    #region Pathfinding
    public TravelLine CreateTravelLine(HexTile target, int numOfTicks, Character character) {
        TravelLineParent lineParent = BezierCurveManager.Instance.GetTravelLineParent(this, target);
        if (lineParent == null) {
            GameObject goParent = Instantiate(GameManager.Instance.travelLineParentPrefab);
            lineParent = goParent.GetComponent<TravelLineParent>();
            lineParent.SetStartAndEndPositions(this, target, numOfTicks);
        }
        GameObject go = Instantiate(GameManager.Instance.travelLinePrefab, lineParent.transform);
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
        if (elevationType != ELEVATION.WATER) {
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
    public void SetOwnedBuildSpot([NotNull]BuildingSpot[] spot) {
        ownedBuildSpots = spot;
        
        locationGridTiles = new List<LocationGridTile>();
        for (int i = 0; i < ownedBuildSpots.Length; i++) {
            BuildingSpot currSpot = ownedBuildSpots[i];
            if (currSpot != null) {
                locationGridTiles.AddRange(currSpot.tilesInTerritory);    
            }
        }
    }
    public List<TileObject> GetTileObjectsInHexTile(TILE_OBJECT_TYPE type) {
        List<TileObject> tileObjects = new List<TileObject>();
        for (int i = 0; i < locationGridTiles.Count; i++) {
            LocationGridTile tile = locationGridTiles[i];
            if (tile.objHere is TileObject && (tile.objHere as TileObject).tileObjectType == type) {
                tileObjects.Add(tile.objHere as TileObject);
            }
        }
        return tileObjects;
    }
    #endregion

    #region Listeners
    private void SubscribeListeners() {    
        Messenger.AddListener<LocationStructure>(Signals.STRUCTURE_OBJECT_PLACED, OnStructurePlaced);
        Messenger.AddListener<LocationStructure, BuildingSpot>(Signals.STRUCTURE_OBJECT_REMOVED, OnStructureRemoved);
    }
    private void OnStructurePlaced(LocationStructure structure) {
        if (ownedBuildSpots != null && ownedBuildSpots.Contains(structure.occupiedBuildSpot.spot)) {
            CheckIfStructureVisualsAreStillValid();
        }
    }
    private void OnStructureRemoved(LocationStructure structure, BuildingSpot spot) {
        if (ownedBuildSpots != null && ownedBuildSpots.Contains(spot)) {
            CheckIfStructureVisualsAreStillValid();
        }
    }
    private STRUCTURE_TYPE GetMostImportantStructureOnTile() {
        STRUCTURE_TYPE mostImportant = STRUCTURE_TYPE.WILDERNESS;
        foreach (KeyValuePair<STRUCTURE_TYPE,List<LocationStructure>> pair in region.structures) {
            for (int i = 0; i < pair.Value.Count; i++) {
                LocationStructure structure = pair.Value[i];
                if (structure.occupiedBuildSpot != null && structure.occupiedBuildSpot.spot.hexTileOwner == this) {
                    int value = pair.Key.StructurePriority(); 
                    if (value > mostImportant.StructurePriority()) {
                        mostImportant = pair.Key;
                    }    
                }
            }
        }
        
        return mostImportant;
    }
    private void CheckIfStructureVisualsAreStillValid() {
        string log = $"Checking {ToString()} to check if landmark on it is still valid";
        STRUCTURE_TYPE mostImportantStructure = GetMostImportantStructureOnTile();
        LANDMARK_TYPE landmarkType = LandmarkManager.Instance.GetLandmarkTypeFor(mostImportantStructure);
        log += $"\nMost important structure is {mostImportantStructure.ToString()}";
        log += $"\nLandmark to create is {landmarkType.ToString()}";
        if (landmarkOnTile == null) {
            LandmarkManager.Instance.CreateNewLandmarkOnTile(this, landmarkType, false);
        } else {
            if (landmarkOnTile.specificLandmarkType != landmarkType) {
                landmarkOnTile.ChangeLandmarkType(landmarkType);    
            }
        }
        Debug.Log(log);
    }
    #endregion
    
    #region Player Action Target
    public List<PlayerAction> actions { get; private set; }
    public void ConstructDefaultActions() {
        actions = new List<PlayerAction>();
    }
    public void AddPlayerAction(PlayerAction action) {
        if (actions.Contains(action) == false) {
            actions.Add(action);
            Messenger.Broadcast(Signals.PLAYER_ACTION_ADDED_TO_TARGET, action, this as IPlayerActionTarget);    
        }
    }
    public void RemovePlayerAction(PlayerAction action) {
        if (actions.Remove(action)) {
            Messenger.Broadcast(Signals.PLAYER_ACTION_REMOVED_FROM_TARGET, action, this as IPlayerActionTarget);
        }
    }
    private PlayerAction GetPlayerAction(string actionName) {
        for (int i = 0; i < actions.Count; i++) {
            PlayerAction playerAction = actions[i];
            if (playerAction.actionName == actionName) {
                return playerAction;
            }
        }
        return null;
    }
    public void ClearPlayerActions() {
        actions.Clear();
    }
    #endregion

    #region Demonic Structure Building
    private bool CanBuildDemonicStructure() {
        return isCorrupted && isCurrentlyBeingCorrupted == false && landmarkOnTile == null 
               && elevationType != ELEVATION.WATER && elevationType != ELEVATION.MOUNTAIN &&
            PlayerManager.Instance.player.mana >= EditableValuesManager.Instance.buildStructureManaCost;
    }
    private void OnClickBuild() {
        List<string> landmarkNames = new List<string>();
        for (int i = 0; i < PlayerManager.Instance.allLandmarksThatCanBeBuilt.Length; i++) {
            landmarkNames.Add(UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(PlayerManager.Instance.allLandmarksThatCanBeBuilt[i].ToString()));
        }
        UIManager.Instance.dualObjectPicker.ShowDualObjectPicker(PlayerManager.Instance.player.minions.Select(x => x.character).ToList(), landmarkNames,
            "Choose a minion", "Choose a structure",
            CanChooseMinion, CanChooseLandmark,
            OnHoverEnterMinion, OnHoverLandmarkChoice,
            OnHoverExitMinion, OnHoverExitLandmarkChoice,
            StartBuild, "Build", column2Identifier: "Landmark");
    }
    private bool CanChooseMinion(Character character) {
        return !character.minion.isAssigned && character.minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.BUILDER);
    }
    private void OnHoverEnterMinion(Character character) {
        if (!CanChooseMinion(character)) {
            string message = string.Empty;
            if (character.minion.isAssigned) {
                message = character.name + " is already doing something else.";
            } else if (!character.minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.BUILDER)) {
                message = character.name + " does not have the required trait: Builder";
            }
            UIManager.Instance.ShowSmallInfo(message);
        }
    }
    private void OnHoverExitMinion(Character character) {
        UIManager.Instance.HideSmallInfo();
    }
    private bool CanChooseLandmark(string landmarkName) {
        // return false;
        if (landmarkName == "The Pit") {
            return false;
        }
        // if(landmarkName == "The Kennel" && !featureComponent.HasFeature(TileFeatureDB.Summons_Feature)) {
        //     return false;
        // }
        // if (landmarkName == "The Crypt" && (!featureComponent.HasFeature(TileFeatureDB.Artifact_Feature) || PlayerManager.Instance.player.playerFaction.HasOwnedRegionWithLandmarkType(LANDMARK_TYPE.THE_CRYPT))) {
        //     return false;
        // }
        return true;
    }
    private void OnHoverLandmarkChoice(string landmarkName) {
        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(landmarkName);
        string info = landmarkData.description;
        if (info != string.Empty) {
            info += "\n";
        }
        info += $"Duration: {GameManager.Instance.GetCeilingHoursBasedOnTicks(landmarkData.buildDuration).ToString()} hours";
        UIManager.Instance.ShowSmallInfo(info);
    }
    private void OnHoverExitLandmarkChoice(string landmarkName) {
        UIManager.Instance.HideSmallInfo();
    }
    private void StartBuild(object minionObj, object landmarkObj) {
        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(landmarkObj as string);
        BaseLandmark newLandmark =
            LandmarkManager.Instance.CreateNewLandmarkOnTile(this, landmarkData.landmarkType, false);
        LandmarkManager.Instance.CreateStructureObjectForLandmark(newLandmark, settlementOnTile);
        PlayerManager.Instance.player.AdjustMana(-EditableValuesManager.Instance.buildStructureManaCost);
        newLandmark.OnFinishedBuilding();
    }
    #endregion

    #region Border Tester
    [Header("Border Tester")]
    [SerializeField] private LineRenderer borderLine;
    [SerializeField] private Transform[] vertices;
    public Transform[] GetVertices(HEXTILE_DIRECTION direction) {
        Transform[] _vertices = new Transform[2];
        switch (direction) {
            case HEXTILE_DIRECTION.NORTH_WEST:
                _vertices[0] = vertices[1];
                _vertices[1] = vertices[0];
                break;
            case HEXTILE_DIRECTION.NORTH_EAST:
                _vertices[0] = vertices[5];
                _vertices[1] = vertices[0];
                break;
            case HEXTILE_DIRECTION.EAST:
                _vertices[0] = vertices[5];
                _vertices[1] = vertices[4];
                break;
            case HEXTILE_DIRECTION.SOUTH_EAST:
                _vertices[0] = vertices[4];
                _vertices[1] = vertices[3];
                break;
            case HEXTILE_DIRECTION.SOUTH_WEST:
                _vertices[0] = vertices[3];
                _vertices[1] = vertices[2];
                break;
            case HEXTILE_DIRECTION.WEST:
                _vertices[0] = vertices[2];
                _vertices[1] = vertices[1];
                break;
        }
        return _vertices;
    }
    #endregion
    
    #region Selectable
    public bool IsCurrentlySelected() {
        return UIManager.Instance.hexTileInfoUI.isShowing &&
               UIManager.Instance.hexTileInfoUI.currentlyShowingHexTile == this;
    }
    public void SelectAction() {
        UIManager.Instance.ShowHexTileInfo(this);
    }
    #endregion
}
