using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathFind;
using System.Linq;
using ECS;
using worldcreator;
using SpriteGlow;

public class HexTile : MonoBehaviour, IHasNeighbours<HexTile>, ILocation {

    public HexTileData data;
    private Region _region;
    private Area _areaOfTile;
    public SpriteRenderer spriteRenderer;

    [Space(10)]
    [Header("Booleans")]
    public bool isRoad = false;
    public bool isOccupied = false;
    [SerializeField] private bool _isPassable = false;
    private bool _isCorrupted = false;
    private bool _isExternal = false;
    private bool _isInternal = false;

    [Space(10)]
    [Header("Pathfinding")]
    public GameObject unpassableGO;
    public Transform goingToMarker;
    public Transform comingFromMarker;

    [Space(10)]
    [Header("Tile Visuals")]
    [SerializeField] private GameObject _centerPiece;
    [SerializeField] private GameObject _highlightGO;
    [SerializeField] internal Transform UIParent;
    [SerializeField] private GameObject _hoverHighlightGO;
    [SerializeField] private GameObject _clickHighlightGO;
    [SerializeField] private GameObject _corruptionHighlightGO;
    [SerializeField] private Sprite manaTileSprite;

    [Space(10)]
    [Header("Tile Borders")]
    [SerializeField] private SpriteRenderer topLeftBorder;
    [SerializeField] private SpriteRenderer leftBorder;
    [SerializeField] private SpriteRenderer botLeftBorder;
    [SerializeField] private SpriteRenderer botRightBorder;
    [SerializeField] private SpriteRenderer rightBorder;
    [SerializeField] private SpriteRenderer topRightBorder;

    [SerializeField] private SpriteGlowEffect topLeftBorderSGE;
    [SerializeField] private SpriteGlowEffect leftBorderSGE;
    [SerializeField] private SpriteGlowEffect botLeftBorderSGE;
    [SerializeField] private SpriteGlowEffect botRightBorderSGE;
    [SerializeField] private SpriteGlowEffect rightBorderSGE;
    [SerializeField] private SpriteGlowEffect topRightBorderSGE;


    [Space(10)]
    [Header("Structure Objects")]
    [SerializeField] private GameObject structureParentGO;
    [SerializeField] private SpriteRenderer mainStructure;
    [SerializeField] private SpriteRenderer structureTint;
    [SerializeField] private Animator structureAnimation;

    [Space(10)]
    [Header("Minimap Objects")]
    [SerializeField] private SpriteRenderer minimapHexSprite;

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

    private PASSABLE_TYPE _passableType;
    //private int _redMagicAmount;
    //private int _blueMagicAmount;
    //private int _greenMagicAmount;
    private BaseLandmark _landmarkOnTile = null;

    protected List<Party> _charactersAtLocation = new List<Party>(); //List of characters/party on landmark

    private Dictionary<HEXTILE_DIRECTION, HexTile> _neighbourDirections;

    public List<HexTile> allNeighbourRoads = new List<HexTile>();

    public List<HexTile> AllNeighbours { get; set; }
    public List<HexTile> ValidTiles { get { return AllNeighbours.Where(o => o.elevationType != ELEVATION.WATER && o.elevationType != ELEVATION.MOUNTAIN).ToList(); } }
    public List<HexTile> NoWaterTiles { get { return AllNeighbours.Where(o => o.elevationType != ELEVATION.WATER).ToList(); } }
    public List<HexTile> LandmarkConnectionTiles { get { return AllNeighbours.Where(o => !o.isRoad).ToList(); } }
    public List<HexTile> AllNeighbourRoadTiles { get { return AllNeighbours.Where(o => o.isRoad).ToList(); } }
    public List<HexTile> PassableNeighbours { get { return AllNeighbours.Where(o => o.isPassable).ToList(); } }

    private List<HexTile> _tilesConnectedInGoingToMarker = new List<HexTile>();
    private List<HexTile> _tilesConnectedInComingFromMarker = new List<HexTile>();

    private int _uncorruptibleLandmarkNeighbors = 0; //if 0, can be corrupted, otherwise, cannot be corrupted
    public BaseLandmark corruptedLandmark = null;
    private GameObject _spawnedTendril = null;

    #region getters/setters
    public int id { get { return data.id; } }
    public int xCoordinate { get { return data.xCoordinate; } }
    public int yCoordinate { get { return data.yCoordinate; } }
    public string tileName { get { return data.tileName; } }
    public string thisName { get { return data.tileName; } }
    public float elevationNoise { get { return data.elevationNoise; } }
    public float moistureNoise { get { return data.moistureNoise; } }
    public float temperature { get { return data.temperature; } }
    public BIOMES biomeType { get { return data.biomeType; } }
    public ELEVATION elevationType { get { return data.elevationType; } }
    public Area areaOfTile { get { return _areaOfTile; } }
    public string locationName {
        get { return tileName + "(" + xCoordinate + ", " + yCoordinate + ")"; }
    }
    public string urlName {
        get { return "<link=" + '"' + this.id.ToString() + '"' + "_hextile>" + tileName + "</link>"; }
    }
    public string coordinates {
        get { return xCoordinate + ", " + yCoordinate; }
    }
    public Region region {
        get { return _region; }
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
    public BaseLandmark landmarkOnTile {
        get { return _landmarkOnTile; }
    }
    public GameObject clickHighlightGO {
        get { return _clickHighlightGO; }
    }
    public List<Party> charactersAtLocation {
        get { return _charactersAtLocation; }
    }
    public HexTile tileLocation {
        get { return this; }
    }
    public LOCATION_IDENTIFIER locIdentifier {
        get { return LOCATION_IDENTIFIER.HEXTILE; }
    }
    public bool isOuterTileOfRegion {
        get { return this.region.outerTiles.Contains(this); }
    }
    public bool hasLandmark {
        get { return _landmarkOnTile != null; }
    }
    public bool isPassable {
        get { return _isPassable; }
    }
    public bool isCorrupted {
        get { return _isCorrupted; }
    }
    public bool isInternal {
        get { return _isInternal; }
    }
    public bool isExternal {
        get { return _isExternal; }
    }
    public PASSABLE_TYPE passableType {
        get { return _passableType; }
    }
    public int uncorruptibleLandmarkNeighbors {
        get { return _uncorruptibleLandmarkNeighbors; }
    }
    public MORALITY morality {
        get {
            if (landmarkOnTile == null || landmarkOnTile.owner == null) {
                return MORALITY.GOOD;
            } else {
                return landmarkOnTile.owner.morality;
            }
        }
    }
    public SpriteRenderer mainStructureSprite {
        get { return mainStructure; }
    }
    #endregion

    public void Initialize() {
        //spriteRenderer = this.GetComponent<SpriteRenderer>();
        //SetMagicAbundance();
        //StartCorruptionAnimation();
    }

    #region Region Functions
    internal void SetRegion(Region region) {
        _region = region;
        if (region == null) {
            data.regionID = -1;
        } else {
            data.regionID = region.id;
        }
    }
    internal bool IsAdjacentWithRegion(Region region) {
        List<HexTile> neighbors = this.AllNeighbours;
        for (int i = 0; i < neighbors.Count; i++) {
            if (neighbors[i].region.id == region.id) {
                return true;
            }
        }
        return false;
    }
    internal bool IsAdjacentToOtherRegion() {
        List<HexTile> neighbors = this.AllNeighbours;
        for (int i = 0; i < neighbors.Count; i++) {
            if (neighbors[i].region.id != this.region.id) {
                return true;
            }
        }
        return false;
    }
    #endregion

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
        _landmarkOnTile = landmarkOnTile;
    }
    public BaseLandmark CreateLandmarkOfType(LANDMARK_TYPE landmarkType) {
        LandmarkData data = LandmarkManager.Instance.GetLandmarkData(landmarkType);
        if (data.isMonsterSpawner) {
            SetLandmarkOnTile(new MonsterSpawnerLandmark(this, landmarkType));
        } else {
            SetLandmarkOnTile(new BaseLandmark(this, landmarkType));
        }
        if (data.minimumTileCount > 1) {
            if (neighbourDirections.ContainsKey(data.connectedTileDirection) && neighbourDirections[data.connectedTileDirection] != null) {
                HexTile tileToConnect = neighbourDirections[data.connectedTileDirection];
                _landmarkOnTile.SetConnectedTile(tileToConnect);
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
            _landmarkOnTile.SetLandmarkObject(landmarkGO.GetComponent<LandmarkVisual>());
        }
        _region.AddLandmarkToRegion(_landmarkOnTile);
        if (_landmarkOnTile != null) {
            SetPassableState(true);
//#if !WORLD_CREATION_TOOL
//            _landmarkOnTile.SetObject(ObjectManager.Instance.CreateNewObject(OBJECT_TYPE.STRUCTURE, Utilities.NormalizeStringUpperCaseFirstLetters(landmarkType.ToString())) as StructureObj);
//#endif
        }
        Biomes.Instance.UpdateTileVisuals(this);
        return _landmarkOnTile;
    }
    public BaseLandmark CreateLandmarkOfType(LandmarkSaveData saveData) {
        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(saveData.landmarkType);
        if (landmarkData.isMonsterSpawner) {
            SetLandmarkOnTile(new MonsterSpawnerLandmark(this, saveData));
        } else {
            SetLandmarkOnTile(new BaseLandmark(this, saveData));
        }
        if (landmarkData.minimumTileCount > 1) {
            if (neighbourDirections.ContainsKey(landmarkData.connectedTileDirection) && neighbourDirections[landmarkData.connectedTileDirection] != null) {
                HexTile tileToConnect = neighbourDirections[landmarkData.connectedTileDirection];
                _landmarkOnTile.SetConnectedTile(tileToConnect);
                tileToConnect.SetElevation(ELEVATION.PLAIN);
                tileToConnect.SetLandmarkOnTile(this.landmarkOnTile); //set the landmark of the connected tile to the same landmark on this tile
                Biomes.Instance.UpdateTileVisuals(tileToConnect);
            }
        }
        //Create Landmark Game Object on tile
        GameObject landmarkGO = CreateLandmarkVisual(saveData.landmarkType, this.landmarkOnTile, landmarkData);
        _landmarkOnTile.SetCivilianCount(saveData.civilianCount);
        if (landmarkGO != null) {
            landmarkGO.transform.localPosition = Vector3.zero;
            landmarkGO.transform.localScale = Vector3.one;
            _landmarkOnTile.SetLandmarkObject(landmarkGO.GetComponent<LandmarkVisual>());
        }
        _region.AddLandmarkToRegion(_landmarkOnTile);
        if (_landmarkOnTile != null) {
            SetPassableState(true);
//#if !WORLD_CREATION_TOOL
//            _landmarkOnTile.SetObject(ObjectManager.Instance.CreateNewObject(OBJECT_TYPE.STRUCTURE, Utilities.NormalizeStringUpperCaseFirstLetters(_landmarkOnTile.specificLandmarkType.ToString())) as StructureObj);
//#endif
        }
        return _landmarkOnTile;
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
    public BaseLandmark LoadLandmark(BaseLandmark landmark) {
        GameObject landmarkGO = null;
        //Create Landmark Game Object on tile
        landmarkGO = GameObject.Instantiate(LandmarkManager.Instance.GetLandmarkGO(), structureParentGO.transform) as GameObject;
        landmarkGO.transform.localPosition = Vector3.zero;
        landmarkGO.transform.localScale = Vector3.one;
        _landmarkOnTile = landmark;
        if (landmarkGO != null) {
            _landmarkOnTile.SetLandmarkObject(landmarkGO.GetComponent<LandmarkVisual>());
        }
        _region.AddLandmarkToRegion(_landmarkOnTile);
        if (_landmarkOnTile != null) {
            SetPassableState(true);
            _landmarkOnTile.SetObject(ObjectManager.Instance.CreateNewObject(OBJECT_TYPE.STRUCTURE, Utilities.NormalizeStringUpperCaseFirstLetters(_landmarkOnTile.specificLandmarkType.ToString())) as StructureObj);
        }
        return _landmarkOnTile;
    }
    public void RemoveLandmarkOnTile() {
        _landmarkOnTile = null;
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
        UpdateManaVisual();
    }
    public bool HasNeighbourThatIsLandmark() {
        return AllNeighbours.Where(x => x.hasLandmark).Any();
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
    internal float GetDistanceTo(HexTile targetHextile) {
        return Vector3.Distance(this.transform.position, targetHextile.transform.position);
    }
    public int GetDistanceFrom(HexTile target) {
        List<HexTile> path = PathGenerator.Instance.GetPath(this, target, PATHFINDING_MODE.PASSABLE);
        if (path != null) {
            return path.Count;
        }
        return -1;
    }
    public bool CanBuildLandmarkHere(LANDMARK_TYPE landmarkToBuild, LandmarkData data, Dictionary<HexTile, LANDMARK_TYPE> landmarksToBeCreated) {
        if (this.hasLandmark || !this.isPassable || landmarksToBeCreated.ContainsKey(this)) {
            return false; //this tile is not passable or already has a landmark
        }
        //if (landmarkToBuild == LANDMARK_TYPE.OAK_FORTIFICATION || landmarkToBuild == LANDMARK_TYPE.IRON_FORTIFICATION) {
        //    if (this.PassableNeighbours.Where(x => x.hasLandmark || landmarksToBeCreated.ContainsKey(x)).Any()) {
        //        return false; //check if this tile has any neighbours that are not fortifications
        //    }
        //} else {
            if (this.PassableNeighbours.Where(x => x.hasLandmark || landmarksToBeCreated.ContainsKey(x)).Any()) {
                return false; //check if this tile has any neighbours that have landmarks
            }
        //}
        if (!data.possibleSpawnPoints.Contains(this.passableType)) {
            return false; //check if this tiles' passable type meets the types the landmark can spawn on
        }
        
        //if (this.region.outerTiles.Contains(this)) {
        //    return false; //exclude outer tiles of region
        //}
        //if (!this.region.IsPartOfMainIsland(this)) {
        //    return false;
        //}
        return true;
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
                } else {
                    //This part is for outerGridTiles only!
                    try {
                        neighbourCoordinateX -= GridMap.Instance._borderThickness;
                        neighbourCoordinateY -= GridMap.Instance._borderThickness;
                        currNeighbour = GridMap.Instance.map[neighbourCoordinateX, neighbourCoordinateY];
                        neighbours.Add(currNeighbour);
                    } catch {
                        //No Handling
                    }
                }
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

    #region Roads
    public void HighlightRoad(Color color) {
        //for (int i = 0; i < roadGOs.Count; i++) {
        //    GameObject currRoad = roadGOs[i];
        //    SetRoadColor(currRoad, color);
        //}
    }
    public void SetRoadColor(GameObject roadToChange, Color color) {
        //roadToChange.spriteRenderer.color = color;
        SpriteRenderer[] children = roadToChange.GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < children.Length; i++) {
            children[i].color = color;
        }
    }
    public GameObject GetRoadGameObjectForDirection(HEXTILE_DIRECTION from, HEXTILE_DIRECTION to) {
        return null;
        //if (from == HEXTILE_DIRECTION.NONE && to == HEXTILE_DIRECTION.NONE) {
        //    return null;
        //}
        //if (from == HEXTILE_DIRECTION.NONE && to != HEXTILE_DIRECTION.NONE) {
        //    //Show the directionGO of to
        //    HexRoads roadToUse = roads.First(x => x.from == to);
        //    return roadToUse.directionGO;
        //} else if (from != HEXTILE_DIRECTION.NONE && to == HEXTILE_DIRECTION.NONE) {
        //    //Show the directionGO of from
        //    HexRoads roadToUse = roads.First(x => x.from == from);
        //    return roadToUse.directionGO;
        //} else {
        //    List<RoadObject> availableRoads = roads.First(x => x.from == from).destinations;
        //    return availableRoads.Where(x => x.to == to).First().roadObj;
        //}
    }
    public void SetTileAsRoad(bool isRoad, ROAD_TYPE roadType, GameObject roadGO) {
        //roadGOs.Add(roadGO);
        //if (this.hasLandmark) {//this.isHabitable
        //    if (isRoad) {
        //        if (_roadType == ROAD_TYPE.NONE) {
        //            _roadType = roadType;
        //        }
        //    } else {
        //        _roadType = ROAD_TYPE.NONE;
        //    }
        //} else {
        //    this.isRoad = isRoad;
        //    if (isRoad) {
        //        if (_roadType == ROAD_TYPE.NONE) {
        //            _roadType = roadType;
        //        }
        //        region.AddTileAsRoad(this);
        //        RoadManager.Instance.AddTileAsRoadTile(this);
        //    } else {
        //        _roadType = ROAD_TYPE.NONE;
        //        region.RemoveTileAsRoad(this);
        //        RoadManager.Instance.RemoveTileAsRoadTile(this);
        //    }
        //}
    }
    public void SetRoadState(bool state) {
        //for (int i = 0; i < roadGOs.Count; i++) {
        //    GameObject road = roadGOs[i];
        //    road.SetActive(state);
        //}
    }
    public void ResetRoadsColors() {
            //Color color = Color.white;
            //if (this.roadType == ROAD_TYPE.MINOR) {
            //    color = Color.gray;
            //}
            //for (int i = 0; i < roadGOs.Count; i++) {
            //    GameObject currRoad = roadGOs[i];
            //    SetRoadColor(currRoad, color);
            //}
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


        //if (mainStructure.sprite != null && mainStructure.sprite.name.Contains("mountains")) {
        //    Utilities.SetSpriteSortingLayer(mainStructure, spriteRenderer.sortingLayerName);
        //    mainStructure.sortingOrder = spriteRenderer.sortingOrder + 1;
        //} else {
        mainStructure.sortingOrder = sortingOrder + 2;
        structureTint.sortingOrder = sortingOrder + 3;
        structureAnimation.gameObject.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 4;
        //}
    }
    internal SpriteRenderer ActivateBorder(HEXTILE_DIRECTION direction, Color color) {
        SpriteRenderer activatedBorder = null;
        switch (direction) {
            case HEXTILE_DIRECTION.NORTH_WEST:
                topLeftBorder.gameObject.SetActive(true);
                activatedBorder = topLeftBorder;
                break;
            case HEXTILE_DIRECTION.NORTH_EAST:
                topRightBorder.gameObject.SetActive(true);
                activatedBorder = topRightBorder;
                break;
            case HEXTILE_DIRECTION.EAST:
                rightBorder.gameObject.SetActive(true);
                activatedBorder = rightBorder;
                break;
            case HEXTILE_DIRECTION.SOUTH_EAST:
                botRightBorder.gameObject.SetActive(true);
                activatedBorder = botRightBorder;
                break;
            case HEXTILE_DIRECTION.SOUTH_WEST:
                botLeftBorder.gameObject.SetActive(true);
                activatedBorder = botLeftBorder;
                break;
            case HEXTILE_DIRECTION.WEST:
                leftBorder.gameObject.SetActive(true);
                activatedBorder = leftBorder;
                break;
            default:
                break;
        }
        if (activatedBorder != null) {
            activatedBorder.color = color;
        }
        return activatedBorder;
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
    //internal void LoadEdges() {
    //    int biomeLayerOfHexTile = Utilities.biomeLayering.IndexOf(this.biomeType);
    //    List<HexTile> neighbours = new List<HexTile>(this.AllNeighbours);
    //    if (this.elevationType == ELEVATION.WATER) {
    //        neighbours = neighbours.Where(x => x.elevationType != ELEVATION.WATER).ToList();
    //    }
    //    for (int i = 0; i < neighbours.Count; i++) {
    //        HexTile currentNeighbour = neighbours[i];

    //        int biomeLayerOfNeighbour = Utilities.biomeLayering.IndexOf(currentNeighbour.biomeType);

    //        if (biomeLayerOfHexTile < biomeLayerOfNeighbour || this.elevationType == ELEVATION.WATER) {
    //            //int neighbourX = currentNeighbour.xCoordinate;
    //            //int neighbourY = currentNeighbour.yCoordinate;

    //            Point difference = new Point((currentNeighbour.xCoordinate - this.xCoordinate),
    //                (currentNeighbour.yCoordinate - this.yCoordinate));
    //            if ((currentNeighbour.biomeType != this.biomeType && currentNeighbour.elevationType != ELEVATION.WATER) ||
    //                this.elevationType == ELEVATION.WATER) {
    //                GameObject gameObjectToEdit = null;
    //                Texture[] spriteMasksToChooseFrom = null;
    //                if (this.yCoordinate % 2 == 0) {
    //                    if (difference.X == -1 && difference.Y == 1) {
    //                        //top left
    //                        gameObjectToEdit = this.topLeftEdge;
    //                        spriteMasksToChooseFrom = Biomes.Instance.topLeftMasks;
    //                    } else if (difference.X == 0 && difference.Y == 1) {
    //                        //top right
    //                        gameObjectToEdit = this.topRightEdge;
    //                        spriteMasksToChooseFrom = Biomes.Instance.topRightMasks;
    //                    } else if (difference.X == 1 && difference.Y == 0) {
    //                        //right
    //                        gameObjectToEdit = this.rightEdge;
    //                        spriteMasksToChooseFrom = Biomes.Instance.rightMasks;
    //                    } else if (difference.X == 0 && difference.Y == -1) {
    //                        //bottom right
    //                        gameObjectToEdit = this.botRightEdge;
    //                        spriteMasksToChooseFrom = Biomes.Instance.botRightMasks;
    //                    } else if (difference.X == -1 && difference.Y == -1) {
    //                        //bottom left
    //                        gameObjectToEdit = this.botLeftEdge;
    //                        spriteMasksToChooseFrom = Biomes.Instance.botLeftMasks;
    //                    } else if (difference.X == -1 && difference.Y == 0) {
    //                        //left
    //                        gameObjectToEdit = this.leftEdge;
    //                        spriteMasksToChooseFrom = Biomes.Instance.leftMasks;
    //                    }
    //                } else {
    //                    if (difference.X == 0 && difference.Y == 1) {
    //                        //top left
    //                        gameObjectToEdit = this.topLeftEdge;
    //                        spriteMasksToChooseFrom = Biomes.Instance.topLeftMasks;
    //                    } else if (difference.X == 1 && difference.Y == 1) {
    //                        //top right
    //                        gameObjectToEdit = this.topRightEdge;
    //                        spriteMasksToChooseFrom = Biomes.Instance.topRightMasks;
    //                    } else if (difference.X == 1 && difference.Y == 0) {
    //                        //right
    //                        gameObjectToEdit = this.rightEdge;
    //                        spriteMasksToChooseFrom = Biomes.Instance.rightMasks;
    //                    } else if (difference.X == 1 && difference.Y == -1) {
    //                        //bottom right
    //                        gameObjectToEdit = this.botRightEdge;
    //                        spriteMasksToChooseFrom = Biomes.Instance.botRightMasks;
    //                    } else if (difference.X == 0 && difference.Y == -1) {
    //                        //bottom left
    //                        gameObjectToEdit = this.botLeftEdge;
    //                        spriteMasksToChooseFrom = Biomes.Instance.botLeftMasks;
    //                    } else if (difference.X == -1 && difference.Y == 0) {
    //                        //left
    //                        gameObjectToEdit = this.leftEdge;
    //                        spriteMasksToChooseFrom = Biomes.Instance.leftMasks;
    //                    }
    //                }
    //                if (gameObjectToEdit != null && spriteMasksToChooseFrom != null) {
    //                    SpriteRenderer sr = gameObjectToEdit.GetComponent<SpriteRenderer>();
    //                    sr.sprite = Biomes.Instance.GetTextureForBiome(currentNeighbour.biomeType);
    //                    sr.sortingOrder += biomeLayerOfNeighbour;
    //                    //                        Material mat = new Material(Shader.Find("AlphaMask"));
    //                    gameObjectToEdit.GetComponent<SpriteRenderer>().material.SetTexture("_Alpha", spriteMasksToChooseFrom[Random.Range(0, spriteMasksToChooseFrom.Length)]);
    //                    gameObjectToEdit.SetActive(true);

    //                    //                        gameObjectToEdit.spriteRenderer.material = mat;
    //                    //gameObjectToEdit.spriteRenderer.material.SetTexture("Alpha (A)", (Texture)spriteMasksToChooseFrom[Random.Range(0, spriteMasksToChooseFrom.Length)]);
    //                    //					gameObjectToEdit.GetComponent<SpriteRenderer> ().material = materialForTile;
    //                }

    //            }
    //        }


    //    }
    //}
    internal void SetBaseSprite(Sprite baseSprite) {
        spriteRenderer.sprite = baseSprite;
    }
    internal void SetCenterSprite(Sprite centerSprite) {
        this.centerPiece.GetComponent<SpriteRenderer>().sprite = centerSprite;
        this.centerPiece.SetActive(true);
    }
    internal void SetMinimapTileColor(Color color) {
        color.a = 255f / 255f;
        minimapHexSprite.color = color;
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

        topLeftBorderSGE.GlowColor = color;
        botLeftBorderSGE.GlowColor = color;
        topRightBorderSGE.GlowColor = color;
        botRightBorderSGE.GlowColor = color;
        leftBorderSGE.GlowColor = color;
        rightBorderSGE.GlowColor = color;
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

        //RuinStructureOnTile(false);
        Transform[] children = Utilities.GetComponentsInDirectChildren<Transform>(UIParent.gameObject);
        for (int i = 0; i < children.Length; i++) {
            ObjectPoolManager.Instance.DestroyObject(children[i].gameObject);
        }
    }
    public void Occupy() {
        this.isOccupied = true;
    }
    public void Unoccupy(bool immediatelyDestroyStructures = false) {
        isOccupied = false;

        Transform[] children = Utilities.GetComponentsInDirectChildren<Transform>(UIParent.gameObject);
        for (int i = 0; i < children.Length; i++) {
            ObjectPoolManager.Instance.DestroyObject(children[i].gameObject);
        }
    }
    #endregion

    #region Passability
    public void SetPassableState(bool state) {
        _isPassable = state;
        unpassableGO.SetActive(!state);
        if (!state) {
            _passableType = PASSABLE_TYPE.UNPASSABLE;
        }
    }
    public void DeterminePassableType() {
        _passableType = GetPassableType();
    }
    public PASSABLE_TYPE GetPassableType() {
        PassableTileData data = GetPassableTileData();
        if (data.adjacentTiles.Count == 2 
            || (data.adjacentTiles.Count == 1 && data.adjacentTiles[0].tiles.Count <= 2 && data.unadjacentTiles.Count == 1)) {
            //minor bottleneck (connected to 2 unadjacent pairs of either 1 or 2 adjacent passable tiles)
            return PASSABLE_TYPE.MINOR_BOTTLENECK;
        } else if (data.unadjacentTiles.Count == 2) {
            //major bottleneck (connected to 2 unadjacent passable tiles)
            return PASSABLE_TYPE.MAJOR_BOTTLENECK;
        } else if (data.IsDeadEnd()) {
            if (data.HasNumberOfUnadjacentTiles(1) || data.HasNumberOfAdjacentTiles(2)) {
                return PASSABLE_TYPE.MAJOR_DEADEND;
            } else if (data.HasNumberOfAdjacentTiles(3)) {
                return PASSABLE_TYPE.MINOR_DEADEND;
            }
            throw new System.Exception("Cannot Get Dead End Type!");
        }  else if (data.unadjacentTiles.Count == 3) {
            //crossroad (connected to 3 unadjacent passable tiles)
            return PASSABLE_TYPE.CROSSROAD;
        } else if (data.TotalPassableNeighbours(this) >= 4) {
            //wide open (connected to 4o to 6 passable tiles)
            return PASSABLE_TYPE.WIDE_OPEN;
        } else {
            //open (the rest)
            return PASSABLE_TYPE.OPEN;
        }
    }
    private PassableTileData GetPassableTileData() {
        return new PassableTileData(this);
    }
    public bool IsBottleneck() {
        return _passableType == PASSABLE_TYPE.MAJOR_BOTTLENECK || _passableType == PASSABLE_TYPE.MINOR_BOTTLENECK;
    }
    public bool IsDeadEnd() {
        return _passableType == PASSABLE_TYPE.MAJOR_DEADEND || _passableType == PASSABLE_TYPE.MINOR_DEADEND;
    }
    public bool IsBorderTileOfRegion() {
            return region.outerTiles.Contains(this);
        }
    #endregion

    #region Monobehaviour Functions
    private void OnMouseOver() {
        MouseOver();
    }
    private void OnMouseExit() {
        MouseExit();
    }
    public void LeftClick() {
#if !WORLD_CREATION_TOOL
        if (UIManager.Instance.IsMouseOnUI() || UIManager.Instance.IsConsoleShowing()) {
            return;
        }
        //StartCorruptionAnimation();
        Messenger.Broadcast(Signals.TILE_LEFT_CLICKED, this);
        if (PlayerManager.Instance.isChoosingStartingTile) {
            return;
        }

        if (this.landmarkOnTile != null) {
            if (!this.landmarkOnTile.tileLocation.isCorrupted) {
                UIManager.Instance.ShowLandmarkInfo(this.landmarkOnTile);
            } else {
                UIManager.Instance.ShowPlayerLandmarkInfo(this.landmarkOnTile);
            }
        } else {
            Messenger.Broadcast(Signals.HIDE_MENUS);
        }
        //UIManager.Instance.playerActionsUI.CloseMenu();
#endif
    }
    public void RightClick() {
#if !WORLD_CREATION_TOOL
        if (UIManager.Instance.IsMouseOnUI() || UIManager.Instance.IsConsoleShowing()) {
            return;
        }
        Messenger.Broadcast(Signals.TILE_RIGHT_CLICKED, this);
        //if (landmarkOnTile != null && (UIManager.Instance.characterInfoUI.currentlyShowingCharacter != null && UIManager.Instance.characterInfoUI.currentlyShowingCharacter.role.roleType == CHARACTER_ROLE.PLAYER)) {
        //    UIManager.Instance.ShowPlayerActions(this.landmarkOnTile);
        //}
#endif
    }
    public void MouseOver() {
#if WORLD_CREATION_TOOL
        //Debug.Log("IS MOUSE OVER UI " + worldcreator.WorldCreatorUI.Instance.IsMouseOnUI());
        if (!worldcreator.WorldCreatorUI.Instance.IsMouseOnUI()) {
            Messenger.Broadcast<HexTile>(Signals.TILE_HOVERED_OVER, this);
            if (Input.GetMouseButton(0)) {
                //Debug.Log("MOUSE DOWN!");
                Messenger.Broadcast<HexTile>(Signals.TILE_LEFT_CLICKED, this);
            }
            if (Input.GetMouseButtonUp(1)) {
                Messenger.Broadcast<HexTile>(Signals.TILE_RIGHT_CLICKED, this);
            }
        }
        //ShowHexTileInfo();
#else
        if (UIManager.Instance.IsMouseOnUI()) {
            return;
        }
        if (Input.GetMouseButtonDown(0)) {
            LeftClick();
        }
        if (Input.GetMouseButtonDown(1)) {
            RightClick();
        }
        if (this.landmarkOnTile != null) {
            _hoverHighlightGO.SetActive(true);
            //if (this.areaOfTile != null) {
            //    this.areaOfTile.HighlightArea();
            //} else {
            //    _hoverHighlightGO.SetActive(true);
            //}
        }
        Messenger.Broadcast(Signals.TILE_HOVERED_OVER, this);
        //ShowHexTileInfo();
        //if (Input.GetMouseButtonDown(0)) {
        //    LeftClick();
        //} else if (Input.GetMouseButtonDown(1)) {
        //    RightClick();
        //}
#endif
    }
    public void MouseExit() {
#if WORLD_CREATION_TOOL
        //if (!worldcreator.WorldCreatorUI.Instance.IsMouseOnUI()) {
            Messenger.Broadcast<HexTile>(Signals.TILE_HOVERED_OUT, this);
        //}
#else
        _hoverHighlightGO.SetActive(false);
        //if (this.landmarkOnTile != null) {
        //    if (this.areaOfTile != null) {
        //        this.areaOfTile.UnhighlightArea();
        //    } else {
        //        _hoverHighlightGO.SetActive(false);
        //    }
        //}
        HideSmallInfoWindow();
        if (UIManager.Instance.IsMouseOnUI() || UIManager.Instance.IsConsoleShowing()) {
            return;
        }
        Messenger.Broadcast(Signals.TILE_HOVERED_OUT, this);
        //if (_landmarkOnTile != null && isHabitable) {
        //	if (_landmarkOnTile.owner != null) {
        //		this.region.HighlightRegionTiles(_landmarkOnTile.owner.factionColor, 69f / 255f);
        //	}
        //}

#endif
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
        Debug.Log(CameraMove.Instance.wholeMapCamera.WorldToScreenPoint(this.transform.position));
    }
    private void HideSmallInfoWindow() {
        UIManager.Instance.HideSmallInfo();
    }
    public override string ToString() {
        return this.locationName;
    }
    #endregion

    #region Characters
	public void AddCharacterToLocation(Party iparty) {
		if (!_charactersAtLocation.Contains(iparty)) {
			_charactersAtLocation.Add(iparty);
            iparty.SetSpecificLocation(this);
            //if (character.icharacterType == ICHARACTER_TYPE.CHARACTER){
            //  Character currChar = character as Character;
            //  currChar.SetSpecificLocation(this);
			//}else if(character is Party){
            //  Party currParty = character as Party;
            //  currParty.SetSpecificLocation(this);
			//}
            //if (!_hasScheduledCombatCheck) {
            //    ScheduleCombatCheck();
            //}
        }
	}
    public void RemoveCharacterFromLocation(Party iparty, bool addToTile = false) {
		_charactersAtLocation.Remove(iparty);
        iparty.SetSpecificLocation(null);
  //      if (character.icharacterType == ICHARACTER_TYPE.CHARACTER){
  //          Character currChar = character as Character;
  //          currChar.SetSpecificLocation(null);
  //      } else if(character is Party){
  //          Party currParty = character as Party;
  //          currParty.SetSpecificLocation(null);
		//}
        //if(_charactersAtLocation.Count == 0 && _hasScheduledCombatCheck) {
        //    UnScheduleCombatCheck();
        //}
	}
    public void ReplaceCharacterAtLocation(Party ipartyToReplace, Party ipartyToAdd) {
        if (_charactersAtLocation.Contains(ipartyToReplace)) {
            int indexOfCharacterToReplace = _charactersAtLocation.IndexOf(ipartyToReplace);
            _charactersAtLocation.Insert(indexOfCharacterToReplace, ipartyToAdd);
            _charactersAtLocation.Remove(ipartyToReplace);
            ipartyToAdd.SetSpecificLocation(this);
            //if (characterToAdd.icharacterType == ICHARACTER_TYPE.CHARACTER) {
            //    Character currChar = characterToAdd as Character;
            //    currChar.SetSpecificLocation(this);
            //} else if (characterToAdd is Party) {
            //    Party currParty = characterToAdd as Party;
            //    currParty.SetSpecificLocation(this);
            //}
            //if (!_hasScheduledCombatCheck) {
            //    ScheduleCombatCheck();
            //}
        }
    }
    public bool IsCharacterAtLocation(ICharacter character) {
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            Party currParty = _charactersAtLocation[i];
            if (currParty.icharacters.Contains(character)) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Combat
    public bool HasHostileCharactersWith(Character character) {
        return false;
    }
    #endregion

    #region Corruption
    public void SetCorruption(bool state, BaseLandmark landmark = null) {
        if(_isCorrupted != state) {
            _isCorrupted = state;
        }
    }
    public bool CanThisTileBeCorrupted() {
        return true;
    }
    public void SetUncorruptibleLandmarkNeighbors(int amount) {
        _uncorruptibleLandmarkNeighbors = amount;
    }
    public void AdjustUncorruptibleLandmarkNeighbors(int amount) {
        _uncorruptibleLandmarkNeighbors += amount;
        if(_uncorruptibleLandmarkNeighbors < 0) {
            _uncorruptibleLandmarkNeighbors = 0;
        }
        if(_uncorruptibleLandmarkNeighbors > 1 && _landmarkOnTile != null) {
            _uncorruptibleLandmarkNeighbors = 1;
        }
    }
    public int GetCorruptedNeighborsCount() {
        int count = 0;
        for (int i = 0; i < AllNeighbours.Count; i++) {
            if (AllNeighbours[i].isCorrupted) {
                count++;
            }
        }
        return count;
    }
    public void SpreadCorruptionToNeighbors() {
        for (int i = 0; i < AllNeighbours.Count; i++) {
            HexTile neighbor = AllNeighbours[i];
            if (!neighbor.isCorrupted) {
                PlayerManager.Instance.AddTileToPlayerArea(neighbor);
            }
        }
    }
    public void ScheduleCorruption() {
        for (int i = 0; i < AllNeighbours.Count; i++) {
            HexTile neighbor = AllNeighbours[i];
            if (!neighbor.isCorrupted) {
                neighbor.StartCorruptionAnimation();
            }
        }

        GameDate nextCorruptionDate = GameManager.Instance.Today();
        nextCorruptionDate.AddHours(GameManager.hoursPerDay);
        SchedulingManager.Instance.AddEntry(nextCorruptionDate, () => SpreadCorruptionToNeighbors());
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
        for (int i = 0; i < particleEffects.Length; i++) {
            particleEffects[i].gameObject.SetActive(true);
        }
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

                //Monster despawning
                if (MonsterManager.Instance.HasMonsterOnLandmark(this.landmarkOnTile)) {
                    ContextMenuItemSettings despawnMonster = new ContextMenuItemSettings("Despawn Monster");
                    settings.AddMenuItem(despawnMonster);

                    ContextMenuSettings despawnMonsterSettings = new ContextMenuSettings();
                    despawnMonster.SetSubMenu(despawnMonsterSettings);

                    for (int i = 0; i < this.landmarkOnTile.charactersAtLocation.Count; i++) {
                        IParty currParty = this.landmarkOnTile.charactersAtLocation[i];
                        if (currParty is MonsterParty) {
                            ContextMenuItemSettings despawnMonsterItem = new ContextMenuItemSettings(currParty.name);
                            despawnMonsterItem.onClickAction = () => MonsterManager.Instance.DespawnMonsterPartyOnLandmark(landmarkOnTile, currParty as MonsterParty);
                            despawnMonsterSettings.AddMenuItem(despawnMonsterItem);
                        }
                    }
                }
                //end monster despawning
            }
        }
        return settings;
    }
#else
    public ContextMenuSettings GetContextMenuSettings() {
        ContextMenuSettings settings = new ContextMenuSettings();
        if ((this.areaOfTile == null || this.areaOfTile.id != PlayerManager.Instance.player.playerArea.id) && this.landmarkOnTile == null && this.isPassable && IsAdjacentToPlayerArea()) {
            ContextMenuItemSettings purchaseTileItem = new ContextMenuItemSettings("Purchase Tile");
            purchaseTileItem.onClickAction += () => PlayerManager.Instance.PurchaseTile(this);
            settings.AddMenuItem(purchaseTileItem);
        }
        if (this.areaOfTile != null && this.areaOfTile.id == PlayerManager.Instance.player.playerArea.id && this.landmarkOnTile == null) {
            ContextMenuItemSettings createLandmarkItem = new ContextMenuItemSettings("Create Landmark");
            ContextMenuSettings createLandmarkSettings = new ContextMenuSettings();
            createLandmarkItem.SetSubMenu(createLandmarkSettings);
            settings.AddMenuItem(createLandmarkItem);

            foreach (LANDMARK_TYPE landmarkType in PlayerManager.Instance.playerStructureTypes.Keys) {
                if (landmarkType != LANDMARK_TYPE.DEMONIC_PORTAL) {
                    if (PlayerManager.Instance.CanCreateLandmarkOnTile(landmarkType, this)) {
                        ContextMenuItemSettings createLandmark = new ContextMenuItemSettings(Utilities.NormalizeStringUpperCaseFirstLetters(landmarkType.ToString()));
                        createLandmark.onClickAction = () => PlayerManager.Instance.CreatePlayerLandmarkOnTile(this, landmarkType);
                        createLandmarkSettings.AddMenuItem(createLandmark);
                    }
                }
            }
        }
#if UNITY_EDITOR
        if (UIManager.Instance.characterInfoUI.activeCharacter != null && UIManager.Instance.characterInfoUI.isShowing) {
            if (this.landmarkOnTile != null) {
                ECS.Character character = UIManager.Instance.characterInfoUI.activeCharacter;
                ContextMenuItemSettings forceActionMain = new ContextMenuItemSettings("Force Action");
                //forceActionMain.onClickAction += () => PlayerManager.Instance.AddTileToPlayerArea(this);
                bool hasDoableAction = false;
                ContextMenuSettings forceActionSub = new ContextMenuSettings();
                forceActionMain.SetSubMenu(forceActionSub);
                for (int i = 0; i < landmarkOnTile.landmarkObj.currentState.actions.Count; i++) {
                    CharacterAction action = landmarkOnTile.landmarkObj.currentState.actions[i];
                    if (action.MeetsRequirements(character.party, this._landmarkOnTile) && action.CanBeDone(landmarkOnTile.landmarkObj) && action.CanBeDoneBy(character.party, landmarkOnTile.landmarkObj)) {
                        hasDoableAction = true;
                        ContextMenuItemSettings doableAction = new ContextMenuItemSettings(Utilities.NormalizeStringUpperCaseFirstLetters(action.actionType.ToString()));
                        doableAction.onClickAction = () => character.party.actionData.ForceDoAction(action, landmarkOnTile.landmarkObj);
                        forceActionSub.AddMenuItem(doableAction);
                    }
                }
                if (hasDoableAction) {
                    settings.AddMenuItem(forceActionMain);
                }
            }
        }
#endif
        return settings;
    }
#endif
    #endregion

    #region Magic
    public void SetManaOnTile(int amount) {
        data.manaOnTile = amount;
        data.manaOnTile = Mathf.Max(data.manaOnTile, 0);
        UpdateManaVisual();
    }
    public void SetManaOnTile(string amount) {
        int value = System.Int32.Parse(amount);
        SetManaOnTile(value);
    }
    public void AdjustManaOnTile(int amount) {
        data.manaOnTile += amount;
        data.manaOnTile = Mathf.Max(data.manaOnTile, 0);
        UpdateManaVisual();
    }
    private void UpdateManaVisual() {
        if (data.manaOnTile > 0) {
            SetCenterSprite(manaTileSprite);
        } else {
            DeactivateCenterPiece();
        }
    }
    #endregion

    #region Pathfinding
    public void AddConnectionInGoingTo(HexTile tile) {
        _tilesConnectedInGoingToMarker.Add(tile);
    }
    public void RemoveConnectionInGoingTo(HexTile tile) {
        _tilesConnectedInGoingToMarker.Remove(tile);
    }
    public void AddConnectionInComingFrom(HexTile tile) {
        _tilesConnectedInComingFromMarker.Add(tile);
    }
    public void RemoveConnectionInComingFrom(HexTile tile) {
        _tilesConnectedInComingFromMarker.Remove(tile);
    }
    public BezierCurve ATileIsTryingToConnect(HexTile tile, int numOfTicks) {
        BezierCurve curve = null;
        if (!_tilesConnectedInGoingToMarker.Contains(tile)) {
            if (!_tilesConnectedInComingFromMarker.Contains(tile)) {
                if (tile._tilesConnectedInGoingToMarker.Contains(this)) {
                    AddConnectionInComingFrom(tile);
                    curve = BezierCurveManager.Instance.DrawCubicCurve(tile.transform.position, comingFromMarker.position, numOfTicks, DIRECTION.DOWN);
                } else {
                    AddConnectionInGoingTo(tile);
                    curve = BezierCurveManager.Instance.DrawCubicCurve(tile.transform.position, goingToMarker.position, numOfTicks, DIRECTION.UP);
                }
            } else {
                curve = BezierCurveManager.Instance.DrawCubicCurve(tile.transform.position, comingFromMarker.position, numOfTicks, DIRECTION.DOWN);
            }
        } else {
            curve = BezierCurveManager.Instance.DrawCubicCurve(tile.transform.position, goingToMarker.position, numOfTicks, DIRECTION.UP);
        }
        BezierCurveParent curveParent = BezierCurveManager.Instance.GetCurveParent(curve.startPos, curve.endPos);
        if(curveParent != null) {
            curveParent.AddChild(curve);
        } else {
            curveParent = BezierCurveManager.Instance.CreateNewCurveParent(curve.startPos, curve.endPos);
            curveParent.AddChild(curve);
            BezierCurveManager.Instance.AddCurveParent(curveParent);
        }

        return curve;
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
}
