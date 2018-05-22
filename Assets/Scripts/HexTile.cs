#define WORLD_CREATION_TOOL

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathFind;
using System.Linq;
using Panda;
using ECS;

public class HexTile : MonoBehaviour, IHasNeighbours<HexTile>, ILocation {
    [Header("General Tile Details")]
    public int id;
    public int xCoordinate;
    public int yCoordinate;
    public int tileTag;
    public string tileName;
    private Region _region;
    [System.NonSerialized] public SpriteRenderer spriteRenderer;

    [Space(10)]
    [Header("Biome Settings")]
    public float elevationNoise;
    public float moistureNoise;
    public float temperature;
    public BIOMES biomeType;
    public ELEVATION elevationType;

    [Space(10)]
    [Header("Booleans")]
    public bool isRoad = false;
    public bool isOccupied = false;
    [SerializeField] private bool _isPassable = false;
    private bool _isCorrupted = false;

    [Space(10)]
    [Header("Pathfinding")]
    public GameObject unpassableGO;

    [Space(10)]
    [Header("Ledges")]
    [SerializeField] private GameObject topLeftLedge;
    [SerializeField] private GameObject topRightLedge;

    [Space(10)]
    [Header("Outlines")]
    [SerializeField] private GameObject topLeftOutline;
    [SerializeField] private GameObject topRightOutline;
    [SerializeField] private GameObject leftOutline;
    [SerializeField] private GameObject rightOutline;
    [SerializeField] private GameObject botLeftOutline;
    [SerializeField] private GameObject botRightOutline;

    [Space(10)]
    [Header("Tile Visuals")]
    [SerializeField] private GameObject _centerPiece;
    [SerializeField] private GameObject _highlightGO;
    [SerializeField] internal Transform UIParent;
    [SerializeField] private Transform resourceParent;
    [SerializeField] private GameObject biomeDetailParentGO;
    [SerializeField] private TextMesh tileTextMesh;
    [SerializeField] private GameObject _emptyCityGO;
    [SerializeField] private GameObject _hoverHighlightGO;
    [SerializeField] private GameObject _clickHighlightGO;
    [SerializeField] private GameObject _corruptionHighlightGO;


    [Space(10)]
    [Header("Tile Edges")]
    [SerializeField] private GameObject topLeftEdge;
    [SerializeField] private GameObject leftEdge;
    [SerializeField] private GameObject botLeftEdge;
    [SerializeField] private GameObject botRightEdge;
    [SerializeField] private GameObject rightEdge;
    [SerializeField] private GameObject topRightEdge;

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
    private StructureObject _structureObjOnTile;

    [Space(10)]
    [Header("Minimap Objects")]
    [SerializeField] private SpriteRenderer minimapHexSprite;
    //private Color biomeColor;

    [Space(10)]
    [Header("Fog Of War Objects")]
    [SerializeField] private SpriteRenderer FOWSprite;
    [SerializeField] private SpriteRenderer minimapFOWSprite;

    [Space(10)]
    [Header("Road Objects")]
    private List<GameObject> roadGOs = new List<GameObject>();
    [SerializeField] private List<HexRoads> roads;
    [SerializeField] private ROAD_TYPE _roadType = ROAD_TYPE.NONE;

    private PASSABLE_TYPE _passableType;

    //Landmark
    private BaseLandmark _landmarkOnTile = null;

    protected List<ICombatInitializer> _charactersAtLocation = new List<ICombatInitializer>(); //List of characters/party on landmark

    private Dictionary<HEXTILE_DIRECTION, HexTile> _neighbourDirections;

    public List<HexTile> allNeighbourRoads = new List<HexTile>();

    public List<HexTile> AllNeighbours { get; set; }
    public List<HexTile> ValidTiles { get { return AllNeighbours.Where(o => o.elevationType != ELEVATION.WATER && o.elevationType != ELEVATION.MOUNTAIN).ToList(); } }
    public List<HexTile> NoWaterTiles { get { return AllNeighbours.Where(o => o.elevationType != ELEVATION.WATER).ToList(); } }
    //public List<HexTile> RoadCreationTiles { get { return AllNeighbours.Where(o => !o.hasLandmark).ToList(); } }
    //public List<HexTile> LandmarkCreationTiles { get { return AllNeighbours.Where(o => !o.hasLandmark).ToList(); } }
    public List<HexTile> MajorRoadTiles { get { return allNeighbourRoads.Where(o => o._roadType == ROAD_TYPE.MAJOR).ToList(); } }
    public List<HexTile> MinorRoadTiles { get { return allNeighbourRoads.Where(o => o._roadType == ROAD_TYPE.MINOR).ToList(); } }
    //public List<HexTile> RegionConnectionTiles { get { return AllNeighbours.Where(o => !o.isRoad).ToList(); } }
    public List<HexTile> LandmarkConnectionTiles { get { return AllNeighbours.Where(o => !o.isRoad).ToList(); } }
    public List<HexTile> AllNeighbourRoadTiles { get { return AllNeighbours.Where(o => o.isRoad).ToList(); } }
    public List<HexTile> PassableNeighbours { get { return AllNeighbours.Where(o => o.isPassable).ToList(); } }
    //public List<HexTile> AvatarTiles { get { return NoWaterTiles; } }

    public List<HexTile> sameTagNeighbours;

    private bool _hasScheduledCombatCheck = false;

    private int _uncorruptibleLandmarkNeighbors = 0; //if 0, can be corrupted, otherwise, cannot be corrupted
    public BaseLandmark corruptedLandmark = null;

    #region getters/setters
    public string locationName {
        get { return tileName + "(" + xCoordinate + ", " + yCoordinate + ")"; }
    }
    public string urlName {
        get { return "[url=" + this.id.ToString() + "_hextile]" + tileName + "[/url]"; }
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
    public FOG_OF_WAR_STATE currFogOfWarState {
        //get { return _currFogOfWarState; }
        get { return FOG_OF_WAR_STATE.VISIBLE; }
    }
    internal Dictionary<HEXTILE_DIRECTION, HexTile> neighbourDirections {
        get { return _neighbourDirections; }
    }
    public GameObject emptyCityGO {
        get { return this._emptyCityGO; }
    }
    internal ROAD_TYPE roadType {
        get { return _roadType; }
    }
    public BaseLandmark landmarkOnTile {
        get { return _landmarkOnTile; }
    }
    public GameObject clickHighlightGO {
        get { return _clickHighlightGO; }
    }
    public List<ICombatInitializer> charactersAtLocation {
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
    public bool roadState {
        get { return roadGOs[0].activeInHierarchy; }
    }
    public StructureObject structureObjOnTile {
        get { return _structureObjOnTile; }
    }
    public bool isCorrupted {
        get { return _isCorrupted; }
    }
    public PASSABLE_TYPE passableType {
        get { return _passableType; }
    }
    public int uncorruptibleLandmarkNeighbors {
        get { return _uncorruptibleLandmarkNeighbors; }
    }
    #endregion

    public void Initialize() {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
    }

    #region Region Functions
    internal void SetRegion(Region region) {
        _region = region;
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
        this.elevationType = elevationType;
    }
    public void UpdateLedgesAndOutlines() {
        if (neighbourDirections == null) {
            return;
        }
        if (elevationType != ELEVATION.WATER) {
            //re enable all outlines and disable all ledges
            topLeftLedge.SetActive(false);
            topRightLedge.SetActive(false);
            SetOutlinesState(true);
        } else { //tile is water
            //check neighbours
            //if north west tile is not water, activate top left ledge
            if (neighbourDirections.ContainsKey(HEXTILE_DIRECTION.NORTH_WEST) && neighbourDirections[HEXTILE_DIRECTION.NORTH_WEST].elevationType != ELEVATION.WATER) {
                topLeftLedge.SetActive(true);
                topLeftOutline.SetActive(false);
            } else {
                //tile doesn't have a north west neighbour
            }
            //if north east tile is not water, activate top right edge
            if (neighbourDirections.ContainsKey(HEXTILE_DIRECTION.NORTH_EAST) && neighbourDirections[HEXTILE_DIRECTION.NORTH_EAST].elevationType != ELEVATION.WATER) {
                topRightLedge.SetActive(true);
                topRightOutline.SetActive(false);
            } else {
                //tile doesn't have a north east neighbour
            }

            //check outlines
            foreach (KeyValuePair<HEXTILE_DIRECTION, HexTile> kvp in neighbourDirections) {
                HexTile neighbour = kvp.Value;
                HEXTILE_DIRECTION direction = kvp.Key;
                if (neighbour.elevationType == ELEVATION.WATER) {
                    //deactivate the outline tile in that direction
                    switch (direction) {
                        case HEXTILE_DIRECTION.NORTH_WEST:
                            topLeftOutline.SetActive(false);
                            break;
                        case HEXTILE_DIRECTION.NORTH_EAST:
                            topRightOutline.SetActive(false);
                            break;
                        case HEXTILE_DIRECTION.EAST:
                            rightOutline.SetActive(false);
                            break;
                        case HEXTILE_DIRECTION.SOUTH_EAST:
                            botRightOutline.SetActive(false);
                            break;
                        case HEXTILE_DIRECTION.SOUTH_WEST:
                            botLeftOutline.SetActive(false);
                            break;
                        case HEXTILE_DIRECTION.WEST:
                            leftOutline.SetActive(false);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
    #endregion

    #region Biome Functions
    internal void SetBiome(BIOMES biome) {
        biomeType = biome;
        //if(elevationType == ELEVATION.WATER) {
        //    SetMinimapTileColor(new Color(64f/255f, 164f/255f, 223f/255f));
        //} else {
        //    SetMinimapTileColor(Utilities.biomeColor[biome]);
        //}
        //biomeColor = minimapHexSprite.color;

    }
    internal void AddBiomeDetailToTile(GameObject detailPrefab) {
        GameObject detailGO = GameObject.Instantiate(detailPrefab, biomeDetailParentGO.transform) as GameObject;
        detailGO.transform.localScale = Vector3.one;
        detailGO.transform.localPosition = Vector3.zero;
    }
    internal void SetBiomeDetailState(bool state) {
        biomeDetailParentGO.SetActive(state);
    }
    #endregion

    #region Landmarks
    public BaseLandmark CreateLandmarkOfType(BASE_LANDMARK_TYPE baseLandmarkType, LANDMARK_TYPE landmarkType) {
        GameObject landmarkGO = null;
        //Create Landmark Game Object on tile
        landmarkGO = GameObject.Instantiate(CityGenerator.Instance.GetLandmarkGO(), structureParentGO.transform) as GameObject;
        landmarkGO.transform.localPosition = Vector3.zero;
        landmarkGO.transform.localScale = Vector3.one;

        switch (baseLandmarkType) {
            case BASE_LANDMARK_TYPE.SETTLEMENT:
                _landmarkOnTile = new Settlement(this, landmarkType);
                break;
            case BASE_LANDMARK_TYPE.RESOURCE:
                _landmarkOnTile = new ResourceLandmark(this, landmarkType);
                break;
            case BASE_LANDMARK_TYPE.DUNGEON:
                _landmarkOnTile = new DungeonLandmark(this, landmarkType);
                break;
            case BASE_LANDMARK_TYPE.LAIR:
                _landmarkOnTile = new LairLandmark(this, landmarkType);
                break;
            default:
                _landmarkOnTile = new BaseLandmark(this, landmarkType);
                break;
        }
        if (landmarkGO != null) {
            _landmarkOnTile.SetLandmarkObject(landmarkGO.GetComponent<LandmarkObject>());
        }
        _region.AddLandmarkToRegion(_landmarkOnTile);
        if (_landmarkOnTile != null) {
            SetPassableState(true);
            _landmarkOnTile.SetObject(ObjectManager.Instance.CreateNewObject(landmarkType) as StructureObj);
        }
        return _landmarkOnTile;
    }
    public BaseLandmark LoadLandmark(BaseLandmark landmark) {
        GameObject landmarkGO = null;
        //Create Landmark Game Object on tile
        landmarkGO = GameObject.Instantiate(CityGenerator.Instance.GetLandmarkGO(), structureParentGO.transform) as GameObject;
        landmarkGO.transform.localPosition = Vector3.zero;
        landmarkGO.transform.localScale = Vector3.one;
        _landmarkOnTile = landmark;
        if (landmarkGO != null) {
            _landmarkOnTile.SetLandmarkObject(landmarkGO.GetComponent<LandmarkObject>());
        }
        _region.AddLandmarkToRegion(_landmarkOnTile);
        if (_landmarkOnTile != null) {
            SetPassableState(true);
            _landmarkOnTile.SetObject(ObjectManager.Instance.CreateNewObject(_landmarkOnTile.specificLandmarkType) as StructureObj);
        }
        return _landmarkOnTile;
    }
    public void RemoveLandmarkOnTile() {
        _landmarkOnTile = null;
    }
    #endregion

    #region Tile Utilities
    public bool HasNeighbourThatIsLandmark() {
        return AllNeighbours.Where(x => x.hasLandmark).Any();
    }
    public PandaBehaviour GetBehaviourTree() {
        return this.GetComponent<PandaBehaviour>();
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
    public void SetTag(int tag) {
        this.tileTag = tag;
    }
    public bool CanBuildLandmarkHere(LANDMARK_TYPE landmarkToBuild, LandmarkData data, Dictionary<HexTile, LANDMARK_TYPE> landmarksToBeCreated) {
        if (this.hasLandmark || !this.isPassable || landmarksToBeCreated.ContainsKey(this)) {
            return false; //this tile is not passable or already has a landmark
        }
        if (landmarkToBuild == LANDMARK_TYPE.OAK_FORTIFICATION || landmarkToBuild == LANDMARK_TYPE.IRON_FORTIFICATION) {
            if (this.PassableNeighbours.Where(x => x.hasLandmark || landmarksToBeCreated.ContainsKey(x)).Any()) {
                return false; //check if this tile has any neighbours that are not fortifications
            }
        } else {
            if (this.PassableNeighbours.Where(x => x.hasLandmark || landmarksToBeCreated.ContainsKey(x)).Any()) {
                return false; //check if this tile has any neighbours that have landmarks
            }
        }
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
    public void FindNeighbours(HexTile[,] gameBoard, bool isForOuterGrid = false) {
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
            _neighbourDirections.Add(GetNeighbourDirection(currNeighbour, isForOuterGrid), currNeighbour);
        }
    }
    internal HEXTILE_DIRECTION GetNeighbourDirection(HexTile neighbour, bool isForOuterGrid = false) {
        if (neighbour == null) {
            return HEXTILE_DIRECTION.NONE;
        }
        if (!AllNeighbours.Contains(neighbour)) {
            throw new System.Exception(neighbour.name + " is not a neighbour of " + this.name);
        }
        int thisXCoordinate = this.xCoordinate;
        int thisYCoordinate = this.yCoordinate;
        if (isForOuterGrid) {
            if (!GridMap.Instance.outerGridList.Contains(neighbour)) {
                thisXCoordinate -= GridMap.Instance._borderThickness;
                thisYCoordinate -= GridMap.Instance._borderThickness;
            }
        }
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
    #endregion

    #region Roads
    public void HighlightRoad(Color color) {
        for (int i = 0; i < roadGOs.Count; i++) {
            GameObject currRoad = roadGOs[i];
            SetRoadColor(currRoad, color);
        }
    }
    public void SetRoadColor(GameObject roadToChange, Color color) {
        //roadToChange.spriteRenderer.color = color;
        SpriteRenderer[] children = roadToChange.GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < children.Length; i++) {
            children[i].color = color;
        }
    }
    public GameObject GetRoadGameObjectForDirection(HEXTILE_DIRECTION from, HEXTILE_DIRECTION to) {
        if (from == HEXTILE_DIRECTION.NONE && to == HEXTILE_DIRECTION.NONE) {
            return null;
        }
        if (from == HEXTILE_DIRECTION.NONE && to != HEXTILE_DIRECTION.NONE) {
            //Show the directionGO of to
            HexRoads roadToUse = roads.First(x => x.from == to);
            return roadToUse.directionGO;
        } else if (from != HEXTILE_DIRECTION.NONE && to == HEXTILE_DIRECTION.NONE) {
            //Show the directionGO of from
            HexRoads roadToUse = roads.First(x => x.from == from);
            return roadToUse.directionGO;
        } else {
            List<RoadObject> availableRoads = roads.First(x => x.from == from).destinations;
            return availableRoads.Where(x => x.to == to).First().roadObj;
        }

    }
    public void SetTileAsRoad(bool isRoad, ROAD_TYPE roadType, GameObject roadGO) {
        roadGOs.Add(roadGO);
        if (this.hasLandmark) {//this.isHabitable
            if (isRoad) {
                if (_roadType == ROAD_TYPE.NONE) {
                    _roadType = roadType;
                }
            } else {
                _roadType = ROAD_TYPE.NONE;
            }
        } else {
            this.isRoad = isRoad;
            if (isRoad) {
                if (_roadType == ROAD_TYPE.NONE) {
                    _roadType = roadType;
                }
                region.AddTileAsRoad(this);
                RoadManager.Instance.AddTileAsRoadTile(this);
            } else {
                _roadType = ROAD_TYPE.NONE;
                region.RemoveTileAsRoad(this);
                RoadManager.Instance.RemoveTileAsRoadTile(this);
            }
        }
    }
    public void SetRoadState(bool state) {
        for (int i = 0; i < roadGOs.Count; i++) {
            GameObject road = roadGOs[i];
            road.SetActive(state);
        }
    }
    public void ResetRoadsColors() {
        Color color = Color.white;
        if (this.roadType == ROAD_TYPE.MINOR) {
            color = Color.gray;
        }
        for (int i = 0; i < roadGOs.Count; i++) {
            GameObject currRoad = roadGOs[i];
            SetRoadColor(currRoad, color);
        }
    }
    #endregion

    #region Tile Visuals
    internal void SetSortingOrder(int sortingOrder) {
        spriteRenderer.sortingOrder = sortingOrder;
        UpdateSortingOrder();
    }
    internal void UpdateSortingOrder() {
        int sortingOrder = spriteRenderer.sortingOrder;
        //if (elevationType == ELEVATION.MOUNTAIN) {
        //    centerPiece.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 56;
        //} else {
        //    centerPiece.GetComponent<SpriteRenderer>().sortingOrder = 60; //sortingOrder + 52;
        //}
        int centerPieceSortingOrder = (int)GridMap.Instance.height - yCoordinate;
        //SpriteRenderer mainRenderer = centerPiece.GetComponent<SpriteRenderer>();
        //mainRenderer.sortingOrder = centerPieceSortingOrder;
        SpriteRenderer[] children = centerPiece.GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < children.Length; i++) {
            SpriteRenderer currRenderer = children[i];
            //if (currRenderer != mainRenderer) {
                currRenderer.sortingOrder = centerPieceSortingOrder;
            //}
        }

        //SpriteRenderer[] resourcesSprites = resourceParent.GetComponentsInChildren<SpriteRenderer>();
        //for (int i = 0; i < resourcesSprites.Length; i++) {
        //    resourcesSprites[i].sortingOrder = sortingOrder + 57;
        //}

        //kingdomColorSprite.spriteRenderer.sortingOrder = sortingOrder + 3;
        highlightGO.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 4;

        //topLeftEdge.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 1;
        //leftEdge.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 1;
        //botLeftEdge.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 1;
        //botRightEdge.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 1;
        //rightEdge.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 1;
        //topRightEdge.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 1;
    }
    internal SpriteRenderer ActivateBorder(HEXTILE_DIRECTION direction) {
        switch (direction) {
            case HEXTILE_DIRECTION.NORTH_WEST:
                topLeftBorder.gameObject.SetActive(true);
                return topLeftBorder;
            case HEXTILE_DIRECTION.NORTH_EAST:
                topRightBorder.gameObject.SetActive(true);
                return topRightBorder;
            case HEXTILE_DIRECTION.EAST:
                rightBorder.gameObject.SetActive(true);
                return rightBorder;
            case HEXTILE_DIRECTION.SOUTH_EAST:
                botRightBorder.gameObject.SetActive(true);
                return botRightBorder;
            case HEXTILE_DIRECTION.SOUTH_WEST:
                botLeftBorder.gameObject.SetActive(true);
                return botLeftBorder;
            case HEXTILE_DIRECTION.WEST:
                leftBorder.gameObject.SetActive(true);
                return leftBorder;
            default:
                return null;
        }
    }
    internal void DeactivateCenterPiece() {
        if (this.biomeType == BIOMES.FOREST && this.elevationType == ELEVATION.PLAIN) {
            centerPiece.SetActive(false);
        }
    }
    internal void LoadEdges() {
        int biomeLayerOfHexTile = Utilities.biomeLayering.IndexOf(this.biomeType);
        List<HexTile> neighbours = new List<HexTile>(this.AllNeighbours);
        if (this.elevationType == ELEVATION.WATER) {
            neighbours = neighbours.Where(x => x.elevationType != ELEVATION.WATER).ToList();
        }
        for (int i = 0; i < neighbours.Count; i++) {
            HexTile currentNeighbour = neighbours[i];

            int biomeLayerOfNeighbour = Utilities.biomeLayering.IndexOf(currentNeighbour.biomeType);

            if (biomeLayerOfHexTile < biomeLayerOfNeighbour || this.elevationType == ELEVATION.WATER) {
                //int neighbourX = currentNeighbour.xCoordinate;
                //int neighbourY = currentNeighbour.yCoordinate;

                Point difference = new Point((currentNeighbour.xCoordinate - this.xCoordinate),
                    (currentNeighbour.yCoordinate - this.yCoordinate));
                if ((currentNeighbour.biomeType != this.biomeType && currentNeighbour.elevationType != ELEVATION.WATER) ||
                    this.elevationType == ELEVATION.WATER) {
                    GameObject gameObjectToEdit = null;
                    Texture[] spriteMasksToChooseFrom = null;
                    if (this.yCoordinate % 2 == 0) {
                        if (difference.X == -1 && difference.Y == 1) {
                            //top left
                            gameObjectToEdit = this.topLeftEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.topLeftMasks;
                        } else if (difference.X == 0 && difference.Y == 1) {
                            //top right
                            gameObjectToEdit = this.topRightEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.topRightMasks;
                        } else if (difference.X == 1 && difference.Y == 0) {
                            //right
                            gameObjectToEdit = this.rightEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.rightMasks;
                        } else if (difference.X == 0 && difference.Y == -1) {
                            //bottom right
                            gameObjectToEdit = this.botRightEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.botRightMasks;
                        } else if (difference.X == -1 && difference.Y == -1) {
                            //bottom left
                            gameObjectToEdit = this.botLeftEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.botLeftMasks;
                        } else if (difference.X == -1 && difference.Y == 0) {
                            //left
                            gameObjectToEdit = this.leftEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.leftMasks;
                        }
                    } else {
                        if (difference.X == 0 && difference.Y == 1) {
                            //top left
                            gameObjectToEdit = this.topLeftEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.topLeftMasks;
                        } else if (difference.X == 1 && difference.Y == 1) {
                            //top right
                            gameObjectToEdit = this.topRightEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.topRightMasks;
                        } else if (difference.X == 1 && difference.Y == 0) {
                            //right
                            gameObjectToEdit = this.rightEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.rightMasks;
                        } else if (difference.X == 1 && difference.Y == -1) {
                            //bottom right
                            gameObjectToEdit = this.botRightEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.botRightMasks;
                        } else if (difference.X == 0 && difference.Y == -1) {
                            //bottom left
                            gameObjectToEdit = this.botLeftEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.botLeftMasks;
                        } else if (difference.X == -1 && difference.Y == 0) {
                            //left
                            gameObjectToEdit = this.leftEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.leftMasks;
                        }
                    }
                    if (gameObjectToEdit != null && spriteMasksToChooseFrom != null) {
                        SpriteRenderer sr = gameObjectToEdit.GetComponent<SpriteRenderer>();
                        sr.sprite = Biomes.Instance.GetTextureForBiome(currentNeighbour.biomeType);
                        sr.sortingOrder += biomeLayerOfNeighbour;
                        //                        Material mat = new Material(Shader.Find("AlphaMask"));
                        gameObjectToEdit.GetComponent<SpriteRenderer>().material.SetTexture("_Alpha", spriteMasksToChooseFrom[Random.Range(0, spriteMasksToChooseFrom.Length)]);
                        gameObjectToEdit.SetActive(true);

                        //                        gameObjectToEdit.spriteRenderer.material = mat;
                        //gameObjectToEdit.spriteRenderer.material.SetTexture("Alpha (A)", (Texture)spriteMasksToChooseFrom[Random.Range(0, spriteMasksToChooseFrom.Length)]);
                        //					gameObjectToEdit.GetComponent<SpriteRenderer> ().material = materialForTile;
                    }

                }
            }


        }
    }
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
    public void SetTileText(string text, int fontSize, Color fontColor, string layer = "Default") {
        tileTextMesh.text = text;
        tileTextMesh.characterSize = fontSize;
        tileTextMesh.color = fontColor;
        tileTextMesh.gameObject.layer = LayerMask.NameToLayer(layer);
        tileTextMesh.transform.localPosition = Vector3.zero;
        tileTextMesh.gameObject.SetActive(true);
    }
    private void SetOutlinesState(bool state) {
        topLeftOutline.SetActive(state);
        topRightOutline.SetActive(state);
        leftOutline.SetActive(state);
        rightOutline.SetActive(state);
        botLeftOutline.SetActive(state);
        botRightOutline.SetActive(state);
    }
    #endregion

    #region Structures Functions
    internal void CreateStructureOnTile(Faction faction, STRUCTURE_TYPE structureType, STRUCTURE_STATE structureState = STRUCTURE_STATE.NORMAL) {
        GameObject[] gameObjectsToChooseFrom = CityGenerator.Instance.GetStructurePrefabsForRace(faction.race, structureType);

        string structureKey = gameObjectsToChooseFrom[Random.Range(0, gameObjectsToChooseFrom.Length)].name;
        GameObject structureGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(structureKey, Vector3.zero, Quaternion.identity, structureParentGO.transform);
        AssignStructureObjectToTile(structureGO.GetComponent<StructureObject>());
        if (structureType == STRUCTURE_TYPE.CITY) {
            structureGO.transform.localPosition = new Vector3(0f, -0.85f, 0f);
            _landmarkOnTile.landmarkObject.SetBGState(false);
        }

        _structureObjOnTile.Initialize(structureType, faction.factionColor, structureState, this);
        this._centerPiece.SetActive(false);
    }
    /*
     * Assign a structure object to this tile.
     * NOTE: This will destroy any current structures on this tile
     * and replace it with the new assigned one.
     * */
    internal void AssignStructureObjectToTile(StructureObject structureObj) {
        if (_structureObjOnTile != null) {
            //Destroy Current Structure
            _structureObjOnTile.DestroyStructure();
        }
        _structureObjOnTile = structureObj;
        structureObj.transform.SetParent(this.structureParentGO.transform);
        structureObj.transform.localPosition = Vector3.zero;
    }
    internal GameObject CreateSpecialStructureOnTile(LAIR lairType) {
        GameObject structureGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(CityGenerator.Instance.GetStructurePrefabForSpecialStructures(lairType).name,
            Vector3.zero, Quaternion.identity, structureParentGO.transform);
        //GameObject.Instantiate(
        //CityGenerator.Instance.GetStructurePrefabForSpecialStructures(lairType), structureParentGO.transform) as GameObject;
        structureGO.transform.localPosition = Vector3.zero;
        return structureGO;
    }
    internal void HideStructures() {
        structureParentGO.SetActive(false);
    }
    internal void ShowStructures() {
        structureParentGO.SetActive(true);
    }
    public void RuinStructureOnTile(bool immediatelyDestroyStructures) {
        if (_structureObjOnTile != null) {
            Debug.Log(GameManager.Instance.month + "/" + GameManager.Instance.days + "/" + GameManager.Instance.year + " - RUIN STRUCTURE ON: " + this.name);
            if (immediatelyDestroyStructures) {
                _structureObjOnTile.DestroyStructure();
            } else {
                _structureObjOnTile.SetStructureState(STRUCTURE_STATE.RUINED);
            }
            if (landmarkOnTile != null) {
                //landmarkOnTile.AddHistory ("Landmark structure destroyed!");
            }
        }
    }
    /*
     Does this tile have a structure on it?
         */
    public bool HasStructure() {
        return _structureObjOnTile != null || (landmarkOnTile != null && landmarkOnTile.isOccupied);
    }
    #endregion

    #region Fog of War Functions
    internal void SetFogOfWarState(FOG_OF_WAR_STATE fowState) {
        //if (!KingdomManager.Instance.useFogOfWar) {
        //    fowState = FOG_OF_WAR_STATE.VISIBLE;
        //}
        //_currFogOfWarState = fowState;
        Color newColor = FOWSprite.color;
        //Color minimapColor = minimapFOWSprite.color;
        switch (fowState) {
            case FOG_OF_WAR_STATE.VISIBLE:
                newColor.a = 0f / 255f;
                //if (isHabitable && isOccupied) {
                //    minimapColor = ownedByCity.kingdom.kingdomColor;
                //} else {
                //    minimapColor = biomeColor;
                //}
                //if ((isHabitable && isOccupied) || isLair) {
                //    ShowNamePlate();
                //}
                if (isOccupied) {
                    ShowStructures();
                }
                UIParent.gameObject.SetActive(true);
                //                ShowAllCitizensOnTile();
                break;
            case FOG_OF_WAR_STATE.SEEN:
                newColor.a = 128f / 255f;
                //if (isHabitable && isOccupied) {
                //    minimapColor = ownedByCity.kingdom.kingdomColor;
                //} else {
                //    minimapColor = biomeColor;
                //}
                //if ((isHabitable && isOccupied) || isLair) {
                //    ShowNamePlate();
                //}
                if (isOccupied) {
                    ShowStructures();
                }
                UIParent.gameObject.SetActive(true);
                //                HideAllCitizensOnTile();
                break;
            case FOG_OF_WAR_STATE.HIDDEN:
                newColor.a = 255f / 255f;
                //minimapColor = Color.black;
                //if ((isHabitable && isOccupied) || isLair) {
                //    HideNamePlate();
                //}
                if (isOccupied) {
                    HideStructures();
                }
                UIParent.gameObject.SetActive(false);
                //                HideAllCitizensOnTile();
                break;
            default:
                break;
        }
        FOWSprite.color = newColor;
        //minimapFOWSprite.color = minimapColor;
    }
    internal void HideFogOfWarObjects() {
        FOWSprite.gameObject.SetActive(false);
        //minimapFOWSprite.gameObject.SetActive(false);
    }
    internal void ShowFogOfWarObjects() {
        FOWSprite.gameObject.SetActive(true);
        //minimapFOWSprite.gameObject.SetActive(true);
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

        RuinStructureOnTile(false);
        Transform[] children = Utilities.GetComponentsInDirectChildren<Transform>(UIParent.gameObject);
        for (int i = 0; i < children.Length; i++) {
            ObjectPoolManager.Instance.DestroyObject(children[i].gameObject);
        }
    }
    public void Occupy() {
        this.isOccupied = true;
    }
    public void Unoccupy(bool immediatelyDestroyStructures = false) {
        //if (!_isBorderOfCities.Select(x => x.kingdom).Contains(ownedByCity.kingdom)
        //    && !_isOuterTileOfCities.Select(x => x.kingdom).Contains(ownedByCity.kingdom)) {
        //    _visibleByKingdoms.Remove(ownedByCity.kingdom);
        //}
        isOccupied = false;
        //ownedByCity = null;
        //SetMinimapTileColor(biomeColor);
        //this._kingdomColorSprite.color = Color.white;
        //this.kingdomColorSprite.gameObject.SetActive(false);
        RuinStructureOnTile(immediatelyDestroyStructures);
        //city = null;

        //Destroy Nameplates
        //RemoveCityNamePlate();
        Transform[] children = Utilities.GetComponentsInDirectChildren<Transform>(UIParent.gameObject);
        for (int i = 0; i < children.Length; i++) {
            ObjectPoolManager.Instance.DestroyObject(children[i].gameObject);
        }
    }
    #endregion

    #region Passability
    public void SetPassableState(bool state) {
        _isPassable = state;
        _centerPiece.SetActive(!state);
        unpassableGO.SetActive(!state);
        if (!state) {
            _passableType = PASSABLE_TYPE.UNPASSABLE;
        }
        //UpdatePassableVisuals();
    }
    public void SetPassableObject(object obj) {
        if (obj == null) {
            SetCenterSprite(null);
            return;
        }
        if (obj is Sprite) {
            SetCenterSprite(obj as Sprite);
        } else {
            GameObject centerObj = GameObject.Instantiate(obj as Object, centerPiece.transform) as GameObject;
            centerObj.transform.localPosition = Vector3.zero;
            centerObj.transform.localScale = Vector3.one;
            SpriteRenderer[] children = centerObj.GetComponentsInChildren<SpriteRenderer>();
            for (int i = 0; i < children.Length; i++) {
                SpriteRenderer currChild = children[i];
                currChild.sortingLayerName = "TileDetails";
            }
            SetCenterSprite(null);
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
#if WORLD_CREATION_TOOL

#else
        if (UIManager.Instance.IsMouseOnUI() || currFogOfWarState != FOG_OF_WAR_STATE.VISIBLE) {
			return;
		}
        if (this.landmarkOnTile != null) {
            _hoverHighlightGO.SetActive(true);
        }
        ShowHexTileInfo();
        if (Input.GetMouseButtonDown(0)){
			LeftClick ();
		}else if(Input.GetMouseButtonDown(1)){
			RightClick ();
		}
#endif
    }
    private void OnMouseExit() {
#if WORLD_CREATION_TOOL
        
#else
        _hoverHighlightGO.SetActive(false);
        if (UIManager.Instance.IsMouseOnUI() || currFogOfWarState != FOG_OF_WAR_STATE.VISIBLE || UIManager.Instance.IsConsoleShowing()) {
            return;
        }
        //if (_landmarkOnTile != null && isHabitable) {
        //	if (_landmarkOnTile.owner != null) {
        //		this.region.HighlightRegionTiles(_landmarkOnTile.owner.factionColor, 69f / 255f);
        //	}
        //}
        HideSmallInfoWindow();
#endif
    }
    private void LeftClick() {
        if (UIManager.Instance.IsMouseOnUI() || currFogOfWarState != FOG_OF_WAR_STATE.VISIBLE || UIManager.Instance.IsConsoleShowing()) {
            if (UIManager.Instance.IsConsoleShowing()) {
                UIManager.Instance.consoleUI.AddText(this.name);
            }
            return;
        }
		if(this.landmarkOnTile != null){
			UIManager.Instance.ShowLandmarkInfo (this.landmarkOnTile);
		}
  //      else{
		//	UIManager.Instance.ShowHexTileInfo (this);
		//}
		UIManager.Instance.HidePlayerActions ();
    }
	private void RightClick(){
		if (UIManager.Instance.IsMouseOnUI() || UIManager.Instance.IsConsoleShowing() || UIManager.Instance.characterInfoUI.activeCharacter == null || this.landmarkOnTile == null) {
            if (UIManager.Instance.IsConsoleShowing() && this.hasLandmark) {
                UIManager.Instance.consoleUI.AddText(this.landmarkOnTile.landmarkName);
            }
            return;
		}
		UIManager.Instance.ShowPlayerActions (this.landmarkOnTile);

//		if(this.landmarkOnTile == null){
//			UIManager.Instance.ShowPlayerActions (this);
//		}else{
//			UIManager.Instance.ShowPlayerActions (this.landmarkOnTile);
//		}
	}
    //private void OnTriggerEnter2D(Collider2D collision) {
    //    if (!this._isPassable) {
    //        return;
    //    }
    //    Debug.Log(collision.name + " entered " + this.name, this);
    //    Character character = collision.gameObject.GetComponent<CharacterAIPath>().icon.character;
    //    if (character.specificLocation != null) {
    //        character.specificLocation.RemoveCharacterFromLocation(character);
    //    }
    //    AddCharacterToLocation(character);
    //}
    //private void OnTriggerExit2D(Collider2D collision) {
    //    Debug.Log(collision.name + " exited " + this.name, this);
    //    Character character = collision.gameObject.GetComponent<CharacterIcon>().character;
    //    if (character.specificLocation == null) {
    //        Debug.LogError(character.name + " has no specific location!", this);
    //    }
    //    character.specificLocation.RemoveCharacterFromLocation(character);
    //}
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

#if UNITY_EDITOR
    [ContextMenu("Select Neighbours")]
    public void SelectAllTilesInRegion() {
        UnityEditor.Selection.objects = this.AllNeighbours.Select(x => x.gameObject).ToArray();
    }
#endif
    [ContextMenu("Load Border Lines")]
    public void LoadBorderLinesForTesting() {
        HexTile currTile = this;
        for (int j = 0; j < currTile.AllNeighbours.Count; j++) {
            HexTile currNeighbour = currTile.AllNeighbours[j];
            if (currNeighbour.region != currTile.region) {
                //Load Border For currTile
                Debug.Log(currNeighbour.name + " - " + currTile.GetNeighbourDirection(currNeighbour, true).ToString());
                //HEXTILE_DIRECTION borderTileToActivate = currTile.GetNeighbourDirection(currNeighbour, true);
                //SpriteRenderer border = currTile.ActivateBorder(borderTileToActivate);
                //currTile.region.AddRegionBorderLineSprite(border);

                //if(currTile.xCoordinate == _borderThickness - 1 && currTile.yCoordinate > _borderThickness && currTile.yCoordinate < height) {
                //    //tile is part of left border
                //    if(borderTileToActivate == HEXTILE_DIRECTION.NORTH_WEST) {
                //        currTile.region.AddRegionBorderLineSprite(currTile.ActivateBorder(HEXTILE_DIRECTION.NORTH_EAST));
                //    }
                //}
            }
        }
    }
    [ContextMenu("Log Neighbour Directions")]
    public void LogNeighbourDirections() {
        string text = this.name + " neighbours: ";
        foreach (KeyValuePair<HEXTILE_DIRECTION, HexTile> kvp in neighbourDirections) {
            text += "\n" + kvp.Key.ToString() + " - " + kvp.Value.name;
        }
        Debug.Log(text);
    }
    private void ShowHexTileInfo() {
        //if (_charactersAtLocation.Count > 0) {
        //    string text = string.Empty;
        //    text += "Characters in tile: ";
        //    for (int i = 0; i < _charactersAtLocation.Count; i++) {
        //        ICombatInitializer currObj = _charactersAtLocation[i];
        //        if (currObj is Party) {
        //            text += "\n" + ((Party)currObj).name;
        //        } else if (currObj is Character) {
        //            text += "\n" + ((Character)currObj).name;
        //        }
        //    }
        //    UIManager.Instance.ShowSmallInfo(text);
        //}
        //PassableTileData data = GetPassableTileData();
        //string text = this.name + " Adjacent Tile Collections: ";
        //for (int i = 0; i < data.adjacentTiles.Count; i++) {
        //    TileCollection currCollection = data.adjacentTiles[i];
        //    text += "\n " + i.ToString() + " - ";
        //    for (int j = 0; j < currCollection.tiles.Count; j++) {
        //        text += currCollection.tiles[j].name + "/";
        //    }
        //}
        //text += "\n" + this.name + " Unadjacent Tile Collections: ";
        //for (int i = 0; i < data.unadjacentTiles.Count; i++) {
        //    TileCollection currCollection = data.unadjacentTiles[i];
        //    text += "\n " + i.ToString() + " - ";
        //    for (int j = 0; j < currCollection.tiles.Count; j++) {
        //        text += currCollection.tiles[j].name + "/";
        //    }
        //}
        //PassableTileData data = GetPassableTileData();
        //string text = this.tileName + " - " + GetPassableType().ToString();
        //if (data.adjacentTiles.Count > 0) {
        //    text += "\n Adjacent Data: ";
        //    for (int i = 0; i < data.adjacentTiles.Count; i++) {
        //        TileCollection currCollection = data.adjacentTiles[i];
        //        text += "\n Collection " + i.ToString() + ": ";
        //        for (int j = 0; j < currCollection.tiles.Count; j++) {
        //            HexTile currTile = currCollection.tiles[j];
        //            text += "\n- " + currTile.tileName;
        //        }
        //    }
        //}
        //if (data.unadjacentTiles.Count > 0) {
        //    text += "\n Unadjacent Data: ";
        //    for (int i = 0; i < data.unadjacentTiles.Count; i++) {
        //        TileCollection currCollection = data.unadjacentTiles[i];
        //        text += "\n Collection " + i.ToString() + ": ";
        //        for (int j = 0; j < currCollection.tiles.Count; j++) {
        //            HexTile currTile = currCollection.tiles[j];
        //            text += "\n- " + currTile.tileName;
        //        }
        //    }
        //}
        UIManager.Instance.ShowSmallInfo(this.name + " - " + _isPassable.ToString());
    }
    private void ShowLandmarkInfo() {
        string text = string.Empty;
        text += "[b]Characters in landmark: [/b]";
        if(_landmarkOnTile.charactersAtLocation.Count > 0) {
            for (int i = 0; i < _landmarkOnTile.charactersAtLocation.Count; i++) {
                ICombatInitializer currObj = _landmarkOnTile.charactersAtLocation[i];
                if (currObj is Party) {
                    text += "\n" + ((Party)currObj).name;
                } else if (currObj is Character) {
                    text += "\n" + ((Character)currObj).name;
                }
            }
        } else {
            text += "NONE";
        }
        
        text += "\n[b]Characters in tile: [/b]";
        if (_charactersAtLocation.Count > 0) {
            for (int i = 0; i < _charactersAtLocation.Count; i++) {
                ICombatInitializer currObj = _charactersAtLocation[i];
                if (currObj is Party) {
                    text += "\n" + ((Party)currObj).name;
                } else if (currObj is Character) {
                    text += "\n" + ((Character)currObj).name;
                }
            }
        } else {
            text += "NONE";
        }

        UIManager.Instance.ShowSmallInfo(text);
    }
    private void HideSmallInfoWindow() {
        UIManager.Instance.HideSmallInfo();
    }
    public override string ToString() {
        return this.tileName;
    }
#endregion

#region Characters
	public void AddCharacterToLocation(ICombatInitializer character) {
		if (!_charactersAtLocation.Contains(character)) {
			_charactersAtLocation.Add(character);
			if(character is Character){
                Character currChar = character as Character;
                currChar.SetSpecificLocation(this);
			}else if(character is Party){
                Party currParty = character as Party;
                currParty.SetSpecificLocation(this);
			}
            if (!_hasScheduledCombatCheck) {
                ScheduleCombatCheck();
            }
        }
	}
	public void RemoveCharacterFromLocation(ICombatInitializer character) {
		_charactersAtLocation.Remove(character);
		if(character is Character){
            Character currChar = character as Character;
            currChar.SetSpecificLocation(null);
        } else if(character is Party){
            Party currParty = character as Party;
            currParty.SetSpecificLocation(null);
		}
        if(_charactersAtLocation.Count == 0 && _hasScheduledCombatCheck) {
            UnScheduleCombatCheck();
        }
	}
    public void ReplaceCharacterAtLocation(ICombatInitializer characterToReplace, ICombatInitializer characterToAdd) {
        if (_charactersAtLocation.Contains(characterToReplace)) {
            int indexOfCharacterToReplace = _charactersAtLocation.IndexOf(characterToReplace);
            _charactersAtLocation.Insert(indexOfCharacterToReplace, characterToAdd);
            _charactersAtLocation.Remove(characterToReplace);
            if (characterToAdd is Character) {
                Character currChar = characterToAdd as Character;
                currChar.SetSpecificLocation(this);
            } else if (characterToAdd is Party) {
                Party currParty = characterToAdd as Party;
                currParty.SetSpecificLocation(this);
            }
            if (!_hasScheduledCombatCheck) {
                ScheduleCombatCheck();
            }
        }
    }
    public Character GetCharacterAtLocationByID(int id, bool includeTraces = false){
		for (int i = 0; i < _charactersAtLocation.Count; i++) {
			if(_charactersAtLocation[i]	is Character){
				if(((Character)_charactersAtLocation[i]).id == id){
					return (Character)_charactersAtLocation [i];
				}
			}else if(_charactersAtLocation[i] is Party){
				Party party = (Party)_charactersAtLocation [i];
				for (int j = 0; j < party.partyMembers.Count; j++) {
					if(party.partyMembers[j].id == id){
						return party.partyMembers [j];
					}
				}
			}
		}
		return null;
	}
	public Party GetPartyAtLocationByLeaderID(int id){
		for (int i = 0; i < _charactersAtLocation.Count; i++) {
			if(_charactersAtLocation[i]	is Party){
				if(((Party)_charactersAtLocation[i]).partyLeader.id == id){
					return (Party)_charactersAtLocation [i];
				}
			}
		}
		return null;
	}
	public int CharactersCount(bool includeHostile = false){
		int count = 0;
		for (int i = 0; i < _charactersAtLocation.Count; i++) {
			if(_charactersAtLocation[i]	is Party){
				count += ((Party)_charactersAtLocation [i]).partyMembers.Count;
			}else {
				count += 1;
			}
		}
		return count;
	}
#endregion

#region Combat
    public void ScheduleCombatCheck() {
        //_hasScheduledCombatCheck = true;
        //Messenger.AddListener(Signals.DAY_START, CheckForCombat);
    }
    public void UnScheduleCombatCheck() {
        //_hasScheduledCombatCheck = false;
        //Messenger.RemoveListener(Signals.DAY_START, CheckForCombat);
    }
    /*
     Check this location for encounters, start if any.
     Mechanics can be found at https://trello.com/c/PgK25YvC/837-encounter-mechanics.
         */
    public void CheckForCombat() {
        //At the start of each day:
        if (HasHostilities() && HasCombatInitializers()) {
            PairUpCombats();
        }
        ContinueDailyActions();
    }
    public void PairUpCombats() {
        List<ICombatInitializer> combatInitializers = GetCharactersByCombatPriority();
        if (combatInitializers != null) {
            for (int i = 0; i < combatInitializers.Count; i++) {
                ICombatInitializer currInitializer = combatInitializers[i];
                Debug.Log("Finding combat pair for " + currInitializer.mainCharacter.name);
                if (currInitializer.isInCombat) {
                    continue; //this current group is already in combat, skip it
                }
                //- If there are hostile parties in combat stance who are not engaged in combat, the attacking character will initiate combat with one of them at random
                List<ICombatInitializer> combatGroups = new List<ICombatInitializer>(GetGroupsBasedOnStance(STANCE.COMBAT, true, currInitializer).Where(x => x.IsHostileWith(currInitializer)));
                if (combatGroups.Count > 0) {
                    ICombatInitializer chosenEnemy = combatGroups[Random.Range(0, combatGroups.Count)];
                    StartCombatBetween(currInitializer, chosenEnemy);
                    continue; //the attacking group has found an enemy! skip to the next group
                }

                //Otherwise, if there are hostile parties in neutral stance who are not engaged in combat, the attacking character will initiate combat with one of them at random
                List<ICombatInitializer> neutralGroups = new List<ICombatInitializer>(GetGroupsBasedOnStance(STANCE.NEUTRAL, true, currInitializer).Where(x => x.IsHostileWith(currInitializer)));
                if (neutralGroups.Count > 0) {
                    ICombatInitializer chosenEnemy = neutralGroups[Random.Range(0, neutralGroups.Count)];
                    StartCombatBetween(currInitializer, chosenEnemy);
                    continue; //the attacking group has found an enemy! skip to the next group
                }

                //- Otherwise, if there are hostile parties in stealthy stance who are not engaged in combat, the attacking character will attempt to initiate combat with one of them at random.
                List<ICombatInitializer> stealthGroups = new List<ICombatInitializer>(GetGroupsBasedOnStance(STANCE.STEALTHY, true, currInitializer).Where(x => x.IsHostileWith(currInitializer)));
                if (stealthGroups.Count > 0) {
                    //The chance of initiating combat is 35%
                    if (Random.Range(0, 100) < 35) {
                        ICombatInitializer chosenEnemy = stealthGroups[Random.Range(0, stealthGroups.Count)];
                        StartCombatBetween(currInitializer, chosenEnemy);
                        continue; //the attacking group has found an enemy! skip to the next group
                    }
                }
            }
        }
    }
    public List<ICombatInitializer> GetCharactersByCombatPriority() {
        //if (_charactersAtLocation.Count <= 0) {
        //    return null;
        //}
        //return _charactersAtLocation.Where(x => x.currentAction.combatPriority > 0).OrderByDescending(x => x.currentAction.combatPriority).ToList();
        return null;
    }
    public bool HasCombatInitializers() {
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            ICombatInitializer currChar = _charactersAtLocation[i];
            //if (currChar.currentAction.combatPriority > 0) {
            //    return true;
            //}
        }
        return false;
    }
    public bool HasHostilities() {
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            ICombatInitializer currItem = _charactersAtLocation[i];
            for (int j = 0; j < _charactersAtLocation.Count; j++) {
                ICombatInitializer otherItem = _charactersAtLocation[j];
                if(currItem != otherItem) {
                    if (currItem.IsHostileWith(otherItem)) {
                        return true; //there are characters with hostilities
                    }
                }
            }
        }
        return false;
    }
    public bool HasHostileCharactersWith(Character character) {
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            ICombatInitializer currItem = _charactersAtLocation[i];
            if (currItem == character) {
                continue; //skip
            }
            Faction factionOfItem = null;
            if (currItem is Character) {
                factionOfItem = (currItem as Character).faction;
            } else if (currItem is Party) {
                factionOfItem = (currItem as Party).faction;
            }
            if (factionOfItem == null || character.faction == null) {
                return true;
            } else {
                if (factionOfItem.id == character.faction.id) {
                    continue; //skip this item, since it has the same faction as the other faction
                }
                FactionRelationship rel = character.faction.GetRelationshipWith(factionOfItem);
                if (rel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasHostilitiesWith(Faction faction, bool withFactionOnly = false) {
        if(faction == null && _charactersAtLocation.Count > 0) {
            return true; //the passed faction is null (factionless), if there are any characters on this tile
        }
        if (!withFactionOnly) { //only check characters if with faction only is false
            for (int i = 0; i < _charactersAtLocation.Count; i++) {
                ICombatInitializer currItem = _charactersAtLocation[i];
                Faction factionOfItem = null;
                if (currItem is Character) {
                    factionOfItem = (currItem as Character).faction;
                } else if (currItem is Party) {
                    factionOfItem = (currItem as Party).faction;
                }
                if (factionOfItem == null) {
                    return true;
                } else {
                    if (factionOfItem.id == faction.id) {
                        continue; //skip this item, since it has the same faction as the other faction
                    }
                    FactionRelationship rel = faction.GetRelationshipWith(factionOfItem);
                    if (rel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE) {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }
    public List<ICombatInitializer> GetGroupsBasedOnStance(STANCE stance, bool notInCombatOnly, ICombatInitializer except = null) {
        List<ICombatInitializer> groups = new List<ICombatInitializer>();
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            ICombatInitializer currGroup = _charactersAtLocation[i];
            if (notInCombatOnly) {
                if (currGroup.isInCombat) {
                    continue; //skip
                }
            }
            if(currGroup.GetCurrentStance() == stance) {
                if (except != null && currGroup == except) {
                    continue; //skip
                }
                groups.Add(currGroup);
            }
        }
        return groups;
    }
    public void StartCombatBetween(ICombatInitializer combatant1, ICombatInitializer combatant2) {
        Combat combat = new Combat(combatant1, combatant2, this);
        combatant1.SetIsInCombat(true);
        combatant2.SetIsInCombat(true);
        if (combatant1 is Party) {
            combat.AddCharacters(SIDES.A, (combatant1 as Party).partyMembers);
        } else {
            combat.AddCharacter(SIDES.A, combatant1 as Character);
        }
        if (combatant2 is Party) {
            combat.AddCharacters(SIDES.B, (combatant2 as Party).partyMembers);
        } else {
            combat.AddCharacter(SIDES.B, combatant2 as Character);
        }
        //this.specificLocation.SetCurrentCombat(combat);
        MultiThreadPool.Instance.AddToThreadPool(combat);
    }
    public void ContinueDailyActions() {
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            ICombatInitializer currItem = _charactersAtLocation[i];
            currItem.ContinueDailyAction();
        }
    }
#endregion

#region Materials
    public void SetMaterialOnTile(MATERIAL material) {
        //_materialOnTile = material;
        //GameObject resource = GameObject.Instantiate(Biomes.Instance.ebonyPrefab, resourceParent) as GameObject;
        //resource.transform.localPosition = Vector3.zero;
        //resource.transform.localScale = Vector3.one;
        region.AddTileWithMaterial(this);
    }
#endregion

#region Corruption
    public void SetCorruption(bool state, BaseLandmark landmark = null) {
        if(_isCorrupted != state) {
            _isCorrupted = state;
            if (_isCorrupted) {
                corruptedLandmark = landmark;
            }
            this._corruptionHighlightGO.SetActive(_isCorrupted);
            if (landmarkOnTile != null) {
                landmarkOnTile.ToggleCorruption(_isCorrupted);
            }
        }
    }
    public bool CanThisTileBeCorrupted() {
        //if(landmarkNeighbor != null && (!landmarkNeighbor.tileLocation.isCorrupted || landmarkNeighbor.tileLocation.uncorruptibleLandmarkNeighbors > 0)) {
        //    return false;
        //}
        //if(_landmarkDirection.Count > 0) {
        //    foreach (BaseLandmark landmark in _landmarkDirection.Keys) {
        //        if (landmark.IsDirectionBlocked(_landmarkDirection[landmark])) {
        //            return false;
        //        }
        //    }
        //}
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
#endregion
}
