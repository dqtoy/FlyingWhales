using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathFind;
using System.Linq;
using Panda;
using ECS;

public class HexTile : MonoBehaviour,  IHasNeighbours<HexTile>, ILocation{
    [Header("General Tile Details")]
    public int id;
    public int xCoordinate;
	public int yCoordinate;
    public int tileTag;
	public string tileName;
    private Region _region;
    private MATERIAL _materialOnTile = MATERIAL.NONE;

    [Space(10)]
    [Header("Biome Settings")]
    public float elevationNoise;
	public float moistureNoise;
	public float temperature;
	public BIOMES biomeType;
	public ELEVATION elevationType;

    [Space(10)]
    [Header("Booleans")]
    public bool isHabitable = false;
	public bool isRoad = false;
    public bool isRoadHidden = true;
	public bool isOccupied = false;

    private List<GameObject> roadGOs = new List<GameObject>();

    [Space(10)]
    [Header("Tile Visuals")]
    [SerializeField] private GameObject _centerPiece;
	[SerializeField] private SpriteRenderer _kingdomColorSprite;
	[SerializeField] private GameObject _highlightGO;
    [SerializeField] internal Transform UIParent;
    [SerializeField] private Transform resourceParent;
    [SerializeField] private GameObject biomeDetailParentGO;
    [SerializeField] private TextMesh tileTextMesh;
	[SerializeField] private GameObject _emptyCityGO;
    [SerializeField] private GameObject _hoverHighlightGO;
	[SerializeField] private GameObject _clickHighlightGO;

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
    private StructureObject structureObjOnTile;

    [Space(10)]
    [Header("Minimap Objects")]
    [SerializeField] private SpriteRenderer minimapHexSprite;
    private Color biomeColor;

    [Space(10)]
    [Header("Fog Of War Objects")]
    [SerializeField] private SpriteRenderer FOWSprite;
    [SerializeField] private SpriteRenderer minimapFOWSprite;

    [Space(10)]
    [Header("Fog Of War Objects")]
    [SerializeField] private FOG_OF_WAR_STATE _currFogOfWarState;

    [Space(10)]
    [Header("Road Objects")]
    [SerializeField] private List<HexRoads> roads;
    [SerializeField] private ROAD_TYPE _roadType = ROAD_TYPE.NONE;

    //Landmark
    private BaseLandmark _landmarkOnTile = null;

	protected List<ICombatInitializer> _charactersAtLocation = new List<ICombatInitializer>(); //List of characters/party on landmark

    private Dictionary<HEXTILE_DIRECTION, HexTile> _neighbourDirections;

	[System.NonSerialized] public Dictionary<HexTile, RoadConnection> connectedTiles = new Dictionary<HexTile, RoadConnection>();

    public List<HexTile> allNeighbourRoads = new List<HexTile>();

	public List<HexTile> AllNeighbours { get; set; }
	public List<HexTile> ValidTiles { get { return AllNeighbours.Where(o => o.elevationType != ELEVATION.WATER && o.elevationType != ELEVATION.MOUNTAIN).ToList();}}
    public List<HexTile> NoWaterTiles { get { return AllNeighbours.Where(o => o.elevationType != ELEVATION.WATER).ToList(); } }
    public List<HexTile> RoadCreationTiles { get { return AllNeighbours.Where(o => !o.hasLandmark).ToList(); } }
    public List<HexTile> LandmarkCreationTiles { get { return AllNeighbours.Where(o => !o.hasLandmark).ToList(); } }
    public List<HexTile> MajorRoadTiles { get { return allNeighbourRoads.Where(o => o._roadType == ROAD_TYPE.MAJOR).ToList(); } }
	public List<HexTile> MinorRoadTiles { get { return allNeighbourRoads.Where(o => o._roadType == ROAD_TYPE.MINOR).ToList(); } }
    public List<HexTile> RegionConnectionTiles { get { return AllNeighbours.Where(o => !o.isRoad).ToList(); } }
    public List<HexTile> LandmarkConnectionTiles { get { return AllNeighbours.Where(o => !o.isRoad).ToList(); } }
    //public List<HexTile> LandmarkExternalConnectionTiles { get { return AllNeighbours.Where(o => !o.isRoad).ToList(); } }
    public List<HexTile> AllNeighbourRoadTiles { get { return AllNeighbours.Where(o => o.isRoad).ToList(); } }
    //public List<HexTile> CombatTiles { get { return NoWaterTiles; } }
    public List<HexTile> AvatarTiles { get { return NoWaterTiles; }}

    public List<HexTile> sameTagNeighbours;

    private bool _hasScheduledCombatCheck = false;

	#region getters/setters
    public string locationName {
        get { return tileName; }
    }
	public string urlName{
		get { return "[url=" + this.id.ToString() + "_hextile]" + tileName + "[/url]"; }
	}
    public Region region {
        get { return _region; }
    }
	public GameObject centerPiece{
		get { return this._centerPiece; }
	}
	public SpriteRenderer kingdomColorSprite{
		get { return this._kingdomColorSprite; }
	}
	public GameObject highlightGO{
		get { return this._highlightGO; }
	}
    public FOG_OF_WAR_STATE currFogOfWarState {
        //get { return _currFogOfWarState; }
        get { return FOG_OF_WAR_STATE.VISIBLE; }
    }
    internal Dictionary<HEXTILE_DIRECTION, HexTile> neighbourDirections {
        get { return _neighbourDirections; }
    }
	public GameObject emptyCityGO{
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
	public List<ICombatInitializer> charactersAtLocation{
		get { return _charactersAtLocation; }
	}
	public HexTile tileLocation{
		get { return this; }
	}
	public LOCATION_IDENTIFIER locIdentifier{
		get { return LOCATION_IDENTIFIER.HEXTILE; }
	}
    public MATERIAL materialOnTile {
        get { return _materialOnTile; }
    }
    public bool hasLandmark {
        get { return _landmarkOnTile != null; }
    }
    #endregion

    #region Region Functions
    internal void SetRegion(Region region) {
        _region = region;
    }
    #endregion

    #region Elevation Functions
    internal void SetElevation(ELEVATION elevationType) {
        this.elevationType = elevationType;
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
        //if (landmarkType != LANDMARK_TYPE.TOWN && baseLandmarkType != BASE_LANDMARK_TYPE.RESOURCE) {
            //NOTE: Only create landmark object if landmark type is not a city and a resource!
            landmarkGO = GameObject.Instantiate(CityGenerator.Instance.GetLandmarkGO(), structureParentGO.transform) as GameObject;
            landmarkGO.transform.localPosition = Vector3.zero;
            landmarkGO.transform.localScale = Vector3.one;
        //}

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
  //      if (landmarkType != LANDMARK_TYPE.TOWN) {
		//	if (landmarkGO != null) {
		//		_landmarkOnTile.SetLandmarkObject (landmarkGO.GetComponent<LandmarkObject> ());
		//	}
		//} else {
		//	//Created landmark was a city
		//	_landmarkOnTile.SetLandmarkObject (_emptyCityGO.GetComponent<LandmarkObject> ());
		//}
        _region.AddLandmarkToRegion(_landmarkOnTile);

        return _landmarkOnTile;
    }
    public void RemoveLandmarkOnTile() {
        _landmarkOnTile = null;
    }

    //internal void HideLandmarkObject() {
    //    if(_landmarkOnTile != null && _landmarkOnTile.landmarkObject != null && GameManager.Instance.hideLandmarks) {
    //        _landmarkOnTile.landmarkObject.gameObject.SetActive(false);
    //    }
    //}
    //internal void ShowLandmarkObject() {
    //    if (_landmarkOnTile != null && GameManager.Instance.hideLandmarks) {
    //        _landmarkOnTile.landmarkObject.gameObject.SetActive(true);
    //    }
    //}
    #endregion

    #region Resource
  //  internal void AssignSpecialResource(){
		//if (this.elevationType == ELEVATION.WATER || this.elevationType == ELEVATION.MOUNTAIN) {
		//	return;
		//}
		//int specialChance = UnityEngine.Random.Range (0, 100);
  //      int specialChanceForBiome = 0;

  //      if (this.biomeType == BIOMES.GRASSLAND || this.biomeType == BIOMES.WOODLAND || this.biomeType == BIOMES.FOREST) {
  //          specialChanceForBiome = 5;
  //      } else if (this.biomeType == BIOMES.DESERT) {
  //          specialChanceForBiome = 3;
  //      } else if (this.biomeType == BIOMES.TUNDRA || this.biomeType == BIOMES.SNOW) {
  //          specialChanceForBiome = 3;
  //      }

		//if (specialChance < specialChanceForBiome) {
		//	this.specialResource = ComputeSpecialResource (Utilities.specialResourcesLookup [this.biomeType]);
		//	if (this.specialResource != RESOURCE.NONE) {
  //              resourceIcon.SetResource(specialResource);
  //              GameObject resource = GameObject.Instantiate(Biomes.Instance.GetPrefabForResource(this.specialResource), resourceParent) as GameObject;
  //              resource.transform.localPosition = Vector3.zero;
  //              resource.transform.localScale = Vector3.one;
  //          }
		//} else {
		//	this.specialResource = RESOURCE.NONE;
		//}
  //  }
	//private RESOURCE ComputeSpecialResource(SpecialResourceChance specialResources){
	//	int totalChance = 0;
	//	int lowerLimit = 0;
	//	int upperLimit = specialResources.chance [0];
	//	for(int i = 0; i < specialResources.resource.Length; i++){
	//		totalChance += specialResources.chance[i];
	//	}

	//	int chance = UnityEngine.Random.Range (0, totalChance);
	//	for(int i = 0; i < specialResources.resource.Length; i++){
	//		if(chance >= lowerLimit && chance < upperLimit){
	//			return specialResources.resource[i];
	//		}else{
	//			lowerLimit = upperLimit;
	//			if (i + 1 < specialResources.resource.Length) {
	//				upperLimit += specialResources.chance [i + 1];
	//			}
	//		}
	//	}
	//	return RESOURCE.NONE;
	//}
	//private void SetSpecialResourceType(){
	//	if (this.specialResource == RESOURCE.DEER || this.specialResource == RESOURCE.PIG || this.specialResource == RESOURCE.BEHEMOTH
	//	   || this.specialResource == RESOURCE.WHEAT || this.specialResource == RESOURCE.RICE || this.specialResource == RESOURCE.CORN) {

	//		this.specialResourceType = RESOURCE_TYPE.FOOD;
	//	} else if(this.specialResource == RESOURCE.SLATE || this.specialResource == RESOURCE.GRANITE || this.specialResource == RESOURCE.OAK
	//		|| this.specialResource == RESOURCE.EBONY){

	//		this.specialResourceType = RESOURCE_TYPE.MATERIAL;
	//	} else if(this.specialResource == RESOURCE.COBALT || this.specialResource == RESOURCE.MANA_STONE || this.specialResource == RESOURCE.MITHRIL){

	//		this.specialResourceType = RESOURCE_TYPE.ORE;
	//	}
	//}
	//private void SetCityCapacity(){
	//	switch(this.specialResource){
	//	case RESOURCE.DEER:
	//		this.cityCapacity = 3;
	//		break;
	//	case RESOURCE.PIG:
	//		this.cityCapacity = 4;
	//		break;
	//	case RESOURCE.BEHEMOTH:
	//		this.cityCapacity = 5;
	//		break;
	//	case RESOURCE.WHEAT:
	//		this.cityCapacity = 3;
	//		break;
	//	case RESOURCE.RICE:
	//		this.cityCapacity = 4;
	//		break;
	//	case RESOURCE.CORN:
	//		this.cityCapacity = 5;
	//		break;
	//	case RESOURCE.SLATE:
	//		this.cityCapacity = 4;
	//		break;
	//	case RESOURCE.GRANITE:
	//		this.cityCapacity = 5;
	//		break;
	//	case RESOURCE.OAK:
	//		this.cityCapacity = 4;
	//		break;
	//	case RESOURCE.EBONY:
	//		this.cityCapacity = 5;
	//		break;
	//	case RESOURCE.COBALT:
	//		this.cityCapacity = 3;
	//		break;
	//	case RESOURCE.MANA_STONE:
	//		this.cityCapacity = 4;
	//		break;
	//	case RESOURCE.MITHRIL:
	//		this.cityCapacity = 5;
	//		break;
	//	}
	//}
	//internal void ProduceResource(){
 //       if (this.region.occupant == null) {
 //           Debug.Log(name + " is trying to produce resource, but occupant is null");
	//		return;
 //       }

 //       switch (this.specialResource){
	//	case RESOURCE.DEER:
	//		this.resourceCount += UnityEngine.Random.Range (15, 26);
	//		break;
	//	case RESOURCE.PIG:
	//		this.resourceCount += UnityEngine.Random.Range (25, 36);
	//		break;
	//	case RESOURCE.BEHEMOTH:
	//		this.resourceCount += UnityEngine.Random.Range (35, 46);
	//		break;
	//	case RESOURCE.WHEAT:
	//		this.resourceCount += 18;
	//		break;
	//	case RESOURCE.RICE:
	//		this.resourceCount += 32;
	//		break;
	//	case RESOURCE.CORN:
	//		this.resourceCount += 42;
	//		break;
	//	case RESOURCE.SLATE:
	//		this.resourceCount += UnityEngine.Random.Range (25, 36);
	//		break;
	//	case RESOURCE.GRANITE:
	//		this.resourceCount += UnityEngine.Random.Range (35, 46);
	//		break;
	//	case RESOURCE.OAK:
	//		this.resourceCount += UnityEngine.Random.Range (25, 36);
	//		break;
	//	case RESOURCE.EBONY:
	//		this.resourceCount += UnityEngine.Random.Range (35, 46);
	//		break;
	//	case RESOURCE.COBALT:
	//		this.resourceCount += UnityEngine.Random.Range (10, 21);
	//		break;
	//	case RESOURCE.MANA_STONE:
	//		this.resourceCount += UnityEngine.Random.Range (20, 31);
	//		break;
	//	case RESOURCE.MITHRIL:
	//		this.resourceCount += UnityEngine.Random.Range (30, 41);
	//		break;
	//	}
	//	CheckResourceCount ();
	//}
	//private void CheckResourceCount(){
	//	if(this.specialResourceType == RESOURCE_TYPE.FOOD){
	//		if(this.resourceCount >= this.region.occupant.foodReserved){
	//			//Create Caravan
	//			EventCreator.Instance.CreateSendResourceEvent(this.resourceCount, 0, 0, this.specialResourceType, this.specialResource, this, this.region.occupant.hexTile, this.region.occupant);
	//			this.resourceCount = 0;
	//		}
	//	}else if(this.specialResourceType == RESOURCE_TYPE.MATERIAL){
	//		if(this.resourceCount >= this.region.occupant.materialReserved){
	//			//Create Caravan
	//			EventCreator.Instance.CreateSendResourceEvent(0, this.resourceCount, 0, this.specialResourceType, this.specialResource, this, this.region.occupant.hexTile, this.region.occupant);
	//			this.resourceCount = 0;
	//		}
	//	}else if(this.specialResourceType == RESOURCE_TYPE.ORE){
	//		if(this.resourceCount >= this.region.occupant.oreReserved){
	//			//Create Caravan
	//			EventCreator.Instance.CreateSendResourceEvent(0, 0, this.resourceCount, this.specialResourceType, this.specialResource, this, this.region.occupant.hexTile, this.region.occupant);
	//			this.resourceCount = 0;
	//		}
	//	}

	//}
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
			int neighbourCoordinateX = xCoordinate + possibleExits [i].X;
			int neighbourCoordinateY = yCoordinate + possibleExits [i].Y;
			if (neighbourCoordinateX >= 0 && neighbourCoordinateX < gameBoard.GetLength(0) && neighbourCoordinateY >= 0 && neighbourCoordinateY < gameBoard.GetLength(1)){
                HexTile currNeighbour = gameBoard[neighbourCoordinateX, neighbourCoordinateY];
                if(currNeighbour != null) {
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
            try {
                _neighbourDirections.Add(GetNeighbourDirection(currNeighbour, isForOuterGrid), currNeighbour);
            } catch {
                Debug.Log("LALALALALALALA");
            }
            
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
        if (thisYCoordinate % 2 == 0) {
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
        } else {
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
        //roadToChange.GetComponent<SpriteRenderer>().color = color;
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
        if(this.isHabitable || this.hasLandmark) {
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
        isRoadHidden = !state;
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
        GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
        UpdateSortingOrder();
    }
    internal void UpdateSortingOrder() {
        int sortingOrder = GetComponent<SpriteRenderer>().sortingOrder;
        if (elevationType == ELEVATION.MOUNTAIN) {
            centerPiece.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 56;
        } else {
            centerPiece.GetComponent<SpriteRenderer>().sortingOrder = 60; //sortingOrder + 52;
        }

        SpriteRenderer[] resourcesSprites = resourceParent.GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < resourcesSprites.Length; i++) {
            resourcesSprites[i].sortingOrder = sortingOrder + 57;
        }

        kingdomColorSprite.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 3;
        highlightGO.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 4;

        topLeftEdge.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 1;
        leftEdge.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 1;
        botLeftEdge.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 1;
        botRightEdge.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 1;
        rightEdge.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 1;
        topRightEdge.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 1;
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
                int neighbourX = currentNeighbour.xCoordinate;
                int neighbourY = currentNeighbour.yCoordinate;

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
                        }
                        else if (difference.X == 0 && difference.Y == 1) {
                            //top right
                            gameObjectToEdit = this.topRightEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.topRightMasks;
                        }
                        else if (difference.X == 1 && difference.Y == 0) {
                            //right
                            gameObjectToEdit = this.rightEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.rightMasks;
                        }
                        else if (difference.X == 0 && difference.Y == -1) {
                            //bottom right
                            gameObjectToEdit = this.botRightEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.botRightMasks;
                        }
                        else if (difference.X == -1 && difference.Y == -1) {
                            //bottom left
                            gameObjectToEdit = this.botLeftEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.botLeftMasks;
                        }
                        else if (difference.X == -1 && difference.Y == 0) {
                            //left
                            gameObjectToEdit = this.leftEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.leftMasks;
                        }
                    }
                    else {
                        if (difference.X == 0 && difference.Y == 1) {
                            //top left
                            gameObjectToEdit = this.topLeftEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.topLeftMasks;
                        }
                        else if (difference.X == 1 && difference.Y == 1) {
                            //top right
                            gameObjectToEdit = this.topRightEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.topRightMasks;
                        }
                        else if (difference.X == 1 && difference.Y == 0) {
                            //right
                            gameObjectToEdit = this.rightEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.rightMasks;
                        }
                        else if (difference.X == 1 && difference.Y == -1) {
                            //bottom right
                            gameObjectToEdit = this.botRightEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.botRightMasks;
                        }
                        else if (difference.X == 0 && difference.Y == -1) {
                            //bottom left
                            gameObjectToEdit = this.botLeftEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.botLeftMasks;
                        }
                        else if (difference.X == -1 && difference.Y == 0) {
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

                        //                        gameObjectToEdit.GetComponent<SpriteRenderer>().material = mat;
                        //gameObjectToEdit.GetComponent<SpriteRenderer>().material.SetTexture("Alpha (A)", (Texture)spriteMasksToChooseFrom[Random.Range(0, spriteMasksToChooseFrom.Length)]);
                        //					gameObjectToEdit.GetComponent<SpriteRenderer> ().material = materialForTile;
                    }

                }
            }


        }
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
    //            int neighbourX = currentNeighbour.xCoordinate;
    //            int neighbourY = currentNeighbour.yCoordinate;

    //            Point difference = new Point((currentNeighbour.xCoordinate - this.xCoordinate),
    //                (currentNeighbour.yCoordinate - this.yCoordinate));
    //            if ((currentNeighbour.biomeType != this.biomeType && currentNeighbour.elevationType != ELEVATION.WATER) ||
    //                this.elevationType == ELEVATION.WATER) {
    //                GameObject gameObjectToEdit = null;
    //                Sprite[] spriteMasksToChooseFrom = null;
    //                if (this.yCoordinate % 2 == 0) {
    //                    if (difference.X == -1 && difference.Y == 1) {
    //                        //top left
    //                        gameObjectToEdit = this.topLeftEdge;
    //                        spriteMasksToChooseFrom = Biomes.Instance.topLeftMasks;
    //                    }
    //                    else if (difference.X == 0 && difference.Y == 1) {
    //                        //top right
    //                        gameObjectToEdit = this.topRightEdge;
    //                        spriteMasksToChooseFrom = Biomes.Instance.topRightMasks;
    //                    }
    //                    else if (difference.X == 1 && difference.Y == 0) {
    //                        //right
    //                        gameObjectToEdit = this.rightEdge;
    //                        spriteMasksToChooseFrom = Biomes.Instance.rightMasks;
    //                    }
    //                    else if (difference.X == 0 && difference.Y == -1) {
    //                        //bottom right
    //                        gameObjectToEdit = this.botRightEdge;
    //                        spriteMasksToChooseFrom = Biomes.Instance.botRightMasks;
    //                    }
    //                    else if (difference.X == -1 && difference.Y == -1) {
    //                        //bottom left
    //                        gameObjectToEdit = this.botLeftEdge;
    //                        spriteMasksToChooseFrom = Biomes.Instance.botLeftMasks;
    //                    }
    //                    else if (difference.X == -1 && difference.Y == 0) {
    //                        //left
    //                        gameObjectToEdit = this.leftEdge;
    //                        spriteMasksToChooseFrom = Biomes.Instance.leftMasks;
    //                    }
    //                }
    //                else {
    //                    if (difference.X == 0 && difference.Y == 1) {
    //                        //top left
    //                        gameObjectToEdit = this.topLeftEdge;
    //                        spriteMasksToChooseFrom = Biomes.Instance.topLeftMasks;
    //                    }
    //                    else if (difference.X == 1 && difference.Y == 1) {
    //                        //top right
    //                        gameObjectToEdit = this.topRightEdge;
    //                        spriteMasksToChooseFrom = Biomes.Instance.topRightMasks;
    //                    }
    //                    else if (difference.X == 1 && difference.Y == 0) {
    //                        //right
    //                        gameObjectToEdit = this.rightEdge;
    //                        spriteMasksToChooseFrom = Biomes.Instance.rightMasks;
    //                    }
    //                    else if (difference.X == 1 && difference.Y == -1) {
    //                        //bottom right
    //                        gameObjectToEdit = this.botRightEdge;
    //                        spriteMasksToChooseFrom = Biomes.Instance.botRightMasks;
    //                    }
    //                    else if (difference.X == 0 && difference.Y == -1) {
    //                        //bottom left
    //                        gameObjectToEdit = this.botLeftEdge;
    //                        spriteMasksToChooseFrom = Biomes.Instance.botLeftMasks;
    //                    }
    //                    else if (difference.X == -1 && difference.Y == 0) {
    //                        //left
    //                        gameObjectToEdit = this.leftEdge;
    //                        spriteMasksToChooseFrom = Biomes.Instance.leftMasks;
    //                    }
    //                }
    //                if (gameObjectToEdit != null && spriteMasksToChooseFrom != null) {
    //                    SpriteRenderer sr = gameObjectToEdit.GetComponent<SpriteRenderer>();
    //                    sr.sprite = Biomes.Instance.GetTextureForBiome(currentNeighbour.biomeType);
    //                    sr.sortingOrder += biomeLayerOfNeighbour;
    //                    gameObjectToEdit.GetComponentInChildren<SpriteMask>().sprite = spriteMasksToChooseFrom[Random.Range(0, spriteMasksToChooseFrom.Length)];
    //                    //                        Material mat = new Material(Shader.Find("AlphaMask"));
    //                    //gameObjectToEdit.GetComponent<SpriteRenderer>().material.SetTexture("_Alpha", spriteMasksToChooseFrom[Random.Range(0, spriteMasksToChooseFrom.Length)]);
    //                    gameObjectToEdit.SetActive(true);

    //                    //                        gameObjectToEdit.GetComponent<SpriteRenderer>().material = mat;
    //                    //gameObjectToEdit.GetComponent<SpriteRenderer>().material.SetTexture("Alpha (A)", (Texture)spriteMasksToChooseFrom[Random.Range(0, spriteMasksToChooseFrom.Length)]);
    //                    //					gameObjectToEdit.GetComponent<SpriteRenderer> ().material = materialForTile;
    //                }

    //            }
    //        }
    //    }
    //}
    internal void SetBaseSprite(Sprite baseSprite){
		this.GetComponent<SpriteRenderer>().sprite = baseSprite;
	}
    internal void SetCenterSprite(Sprite centerSprite){
		this.centerPiece.GetComponent<SpriteRenderer>().sprite = centerSprite;
		this.centerPiece.SetActive(true);
	}
 //   internal void SetTileHighlightColor(Color color){
 //       //color.a = 30f / 255f;
 //       this._kingdomColorSprite.color = color;
	//}
    internal void SetMinimapTileColor(Color color) {
        color.a = 255f / 255f;
        minimapHexSprite.color = color;
    }
 //   internal void ShowTileHighlight(){
	//	this._kingdomColorSprite.gameObject.SetActive(true);
	//}
 //   internal void HideTileHighlight(){
	//	this.kingdomColorSprite.gameObject.SetActive(false);
	//}
    //internal void ShowNamePlate() {
    //    if (_namePlateParent != null) {
    //        _namePlateParent.gameObject.SetActive(true);
    //    }
    //    if (_cityInfo != null) {
    //        UpdateCityNamePlate();
    //    }
    //    if (_lairItem != null) {
    //        UpdateLairNamePlate();
    //    }
    //}
    //internal void HideNamePlate() {
    //    _namePlateParent.gameObject.SetActive(false);
    //}
    /*
     * This will instantiate a new CityItem Prefab and set it's city 
     * according to the passed parameter.
     * */
 //   internal void CreateCityNamePlate(City city) {
 //       //Debug.Log("Create nameplate for: " + city.name + " on " + this.name);

 //       GameObject namePlateGO = UIManager.Instance.InstantiateUIObject("CityNamePlatePanel", UIParent);
 //       namePlateGO.layer = LayerMask.NameToLayer("HextileNamePlates");
 //       _namePlateParent = namePlateGO.transform;
 //       _cityInfo = namePlateGO.GetComponentInChildren<CityItem>();
 //       namePlateGO.transform.localPosition = new Vector3(0f, -1.45f, 0f);
 //       Messenger.AddListener("UpdateUI", UpdateCityNamePlate);

 //       UpdateCityNamePlate();
	//	UpdateCityFoodMaterialOreUI ();
 //   }
 //   internal void UpdateCityNamePlate() {
 //       if (_currFogOfWarState == FOG_OF_WAR_STATE.VISIBLE) {
 //           _cityInfo.SetCity(city, false, false, false, true);
 //       } else {
 //           _cityInfo.SetCity(city, false, true, false, true);
 //       }
 //   }
	//internal void UpdateCityFoodMaterialOreUI(){
	//	if(_cityInfo != null){
	//		_cityInfo.UpdateFoodMaterialOreUI ();
	//	}
	//}
 //   internal void RemoveCityNamePlate() {
 //       if (_namePlateParent != null) {
 //           ObjectPoolManager.Instance.DestroyObject(_namePlateParent.gameObject);
 //           _namePlateParent = null;
 //           _cityInfo = null;
 //           Messenger.RemoveListener("UpdateUI", UpdateCityNamePlate);
 //       }
 //   }
    #endregion

	//#region Lair
 //   internal void CreateLairNamePlate() {
 //       //Debug.Log("Create lair nameplate on " + this.name);

 //       GameObject namePlateGO = UIManager.Instance.InstantiateUIObject("LairNamePlatePanel", UIParent);
 //       namePlateGO.layer = LayerMask.NameToLayer("HextileNamePlates");
 //       _namePlateParent = namePlateGO.transform;
 //       _lairItem = namePlateGO.GetComponentInChildren<LairItem>();
 //       namePlateGO.transform.localPosition = new Vector3(-2.22f, -1.02f, 0f);
 //       Messenger.AddListener("UpdateUI", UpdateLairNamePlate);

 //       UpdateLairNamePlate();
	//}
 //   internal void UpdateLairNamePlate() {
	//	this._lairItem.SetLair(this.lair);
	//}
 //   internal void RemoveLairNamePlate() {
 //       if (_namePlateParent != null) {
 //           ObjectPoolManager.Instance.DestroyObject(_namePlateParent.gameObject);
 //           _namePlateParent = null;
 //           _lairItem = null;
 //           Messenger.RemoveListener("UpdateUI", UpdateLairNamePlate);
 //       }
 //   }
	//internal void AddLairsInRange(Lair lair){
	//	this._lairsInRange.Add (lair);
	//}
	//internal void CheckLairsInRange(){
	//	if (this._lairsInRange.Count > 0) {
	//		for (int i = 0; i < this._lairsInRange.Count; i++) {
	//			this._lairsInRange [i].ActivateLair ();
	//		}
	//		this._lairsInRange.Clear ();
	//	}
	//}
 //   #endregion

    #region Structures Functions
  //  internal void CreateStructureOnTile(STRUCTURE_TYPE structureType, STRUCTURE_STATE structureState = STRUCTURE_STATE.NORMAL) {
  //      //Debug.Log("Create " + structureType.ToString() + " on " + this.name);
  //      GameObject[] gameObjectsToChooseFrom = CityGenerator.Instance.GetStructurePrefabsForRace(this.ownedByCity.faction.race, structureType);

  //      string structureKey = gameObjectsToChooseFrom[Random.Range(0, gameObjectsToChooseFrom.Length)].name;
		//GameObject structureGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(structureKey, Vector3.zero, Quaternion.identity, structureParentGO.transform);
  //      AssignStructureObjectToTile(structureGO.GetComponent<StructureObject>());
		//structureGO.transform.localPosition = new Vector3 (0f, -0.85f, 0f);
  //      structureObjOnTile.Initialize(structureType, this.ownedByCity.faction.factionColor, structureState, this);

  //      this._centerPiece.SetActive(false);

  //      //Color color = this.ownedByCity.kingdom.kingdomColor;
  //      //SetMinimapTileColor(color);
  //      //SetTileHighlightColor(color);
  //  }
    internal void CreateStructureOnTile(Faction faction, STRUCTURE_TYPE structureType, STRUCTURE_STATE structureState = STRUCTURE_STATE.NORMAL) {
        //Debug.Log("Create " + structureType.ToString() + " on " + this.name);
        GameObject[] gameObjectsToChooseFrom = CityGenerator.Instance.GetStructurePrefabsForRace(faction.race, structureType);

        string structureKey = gameObjectsToChooseFrom[Random.Range(0, gameObjectsToChooseFrom.Length)].name;
        GameObject structureGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(structureKey, Vector3.zero, Quaternion.identity, structureParentGO.transform);
        AssignStructureObjectToTile(structureGO.GetComponent<StructureObject>());
        if(structureType == STRUCTURE_TYPE.CITY) {
            structureGO.transform.localPosition = new Vector3(0f, -0.85f, 0f);
            _landmarkOnTile.landmarkObject.SetBGState(false);
        }

        structureObjOnTile.Initialize(structureType, faction.factionColor, structureState, this);
        this._centerPiece.SetActive(false);

        //Color color = this.ownedByCity.kingdom.kingdomColor;
        //SetMinimapTileColor(color);
        //SetTileHighlightColor(color);
    }
    /*
     * Assign a structure object to this tile.
     * NOTE: This will destroy any current structures on this tile
     * and replace it with the new assigned one.
     * */
    internal void AssignStructureObjectToTile(StructureObject structureObj) {
        if (structureObjOnTile != null) {
            //Destroy Current Structure
            structureObjOnTile.DestroyStructure();
        }
        structureObjOnTile = structureObj;
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
	//internal CorpseMound CreateCorpseMoundObjectOnTile(int initialCorpseCount) {
	//	this._corpseMoundGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(CityGenerator.Instance.GetCorpseMoundGO().name,
	//		Vector3.zero, Quaternion.identity, structureParentGO.transform);
	//	this._corpseMoundGO.transform.localPosition = Vector3.zero;
	//	SetCorpseMound (this._corpseMoundGO.GetComponent<CorpseMound>());
	//	this._corpseMound.Initialize (this, initialCorpseCount);
	//	return this._corpseMound;
	//}
    internal void HideStructures() {
        structureParentGO.SetActive(false);
    }
    internal void ShowStructures() {
        structureParentGO.SetActive(true);
    }
    //public void ReColorStructure() {
    //    Transform[] children = Utilities.GetComponentsInDirectChildren<Transform>(structureParentGO);
    //    for (int i = 0; i < children.Length; i++) {
    //        GameObject structureToRecolor = children[i].gameObject;

    //        SpriteRenderer[] allColorizers = structureToRecolor.GetComponentsInChildren<SpriteRenderer>().
    //        Where(x => x.gameObject.tag == "StructureColorizers").ToArray();

    //        for (int j = 0; j < allColorizers.Length; j++) {
    //            allColorizers[j].color = this.ownedByCity.kingdom.kingdomColor;
    //        }
    //    }
    //}
    public void RuinStructureOnTile(bool immediatelyDestroyStructures) {
        if (structureObjOnTile != null) {
            Debug.Log(GameManager.Instance.month + "/" + GameManager.Instance.days + "/" + GameManager.Instance.year + " - RUIN STRUCTURE ON: " + this.name);
            if (immediatelyDestroyStructures) {
                structureObjOnTile.DestroyStructure();
            } else {
                structureObjOnTile.SetStructureState(STRUCTURE_STATE.RUINED);
            }
			if(landmarkOnTile != null){
				//landmarkOnTile.AddHistory ("Landmark structure destroyed!");
			}
        }
    }
    /*
     Does this tile have a structure on it?
         */
    public bool HasStructure() {
        return structureObjOnTile != null || (landmarkOnTile != null && landmarkOnTile.isOccupied);
    }
    #endregion

    #region Fog of War Functions
    internal void SetFogOfWarState(FOG_OF_WAR_STATE fowState) {
        //if (!KingdomManager.Instance.useFogOfWar) {
        //    fowState = FOG_OF_WAR_STATE.VISIBLE;
        //}
        _currFogOfWarState = fowState;
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
        //this.city = null;
        this.isOccupied = false;
        //this.ownedByCity = null;
        //SetMinimapTileColor(biomeColor);
        this._kingdomColorSprite.color = Color.white;
        this.kingdomColorSprite.gameObject.SetActive(false);
        //this._lairItem = null;
        //Messenger.RemoveListener("UpdateUI", UpdateLairNamePlate);

        RuinStructureOnTile(false);
        //RemoveCityNamePlate();
        Transform[] children = Utilities.GetComponentsInDirectChildren<Transform>(UIParent.gameObject);
        for (int i = 0; i < children.Length; i++) {
            ObjectPoolManager.Instance.DestroyObject(children[i].gameObject);
        }
    }
    //public void Occupy(City city) {
    //    this.isOccupied = true;
    //    //if (!isVisibleByCities.Contains(city)) {
    //    //    this.isVisibleByCities.Add(city);
    //    //}
    //    //this.isOccupiedByCityID = city.id;
    //    this.city = city;
    //    this.ownedByCity = city;
    //    if (!_visibleByKingdoms.Contains(city.kingdom)) {
    //        _visibleByKingdoms.Add(city.kingdom);
    //    }
    //    //this.isBorder = false;
    //    //this.isBorderOfCityID = 0;
    //}
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
    //public void Borderize(City city) {
    //    this.isBorder = true;
    //    //if (!_isBorderOfCities.Contains(city)) {
    //    //    _isBorderOfCities.Add(city);
    //    //}
    //    //if (!_visibleByKingdoms.Contains(city.kingdom)) {
    //    //    _visibleByKingdoms.Add(city.kingdom);
    //    //}
    //    //if (!isVisibleByCities.Contains(city)) {
    //    //    this.isVisibleByCities.Add(city);
    //    //}
    //    //this.isBorderOfCityID = city.id;
    //    //this.ownedByCity = city;
    //}
    //public void UnBorderize(City city) {
    //    this.isBorder = false;
    //    //this.isBorderOfCityID = 0;
    //    //this.ownedByCity = null;
    //    //_isBorderOfCities.Remove(city);
    //    //if (_isBorderOfCities.Count <= 0) {
    //    //    this.isBorder = false;
    //    //    this._kingdomColorSprite.color = Color.white;
    //    //    this.kingdomColorSprite.gameObject.SetActive(false);
    //    //}

    //    //if (!_isBorderOfCities.Select(x => x.kingdom).Contains(city.kingdom)
    //    //    && !_isOuterTileOfCities.Select(x => x.kingdom).Contains(city.kingdom)
    //    //    && (ownedByCity == null || ownedByCity.kingdom.id != city.kingdom.id)) {
    //    //    _visibleByKingdoms.Remove(city.kingdom);
    //    //}
    //    //this.isVisibleByCities.Remove(city);
    //}
    #endregion

    #region Monobehaviour Functions
//	private void OnMouseDown(){
//		
//	}
	private void OnMouseOver() {
		if (UIManager.Instance.IsMouseOnUI() || currFogOfWarState != FOG_OF_WAR_STATE.VISIBLE) {
			return;
		}
        if (this.landmarkOnTile != null) {
            _hoverHighlightGO.SetActive(true);
        }
		//if (_landmarkOnTile != null) {
		//	if(_landmarkOnTile.owner != null) { //landmark is occupied
		//		if (isHabitable) {
		//			this.region.HighlightRegionTiles(_landmarkOnTile.owner.factionColor, 127f / 255f);
		//		}
		//	}
		//} 
		if(Input.GetMouseButtonDown(0)){
			LeftClick ();
		}else if(Input.GetMouseButtonDown(1)){
			RightClick ();
		}
	}
	private void OnMouseExit() {
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

    #endregion

    internal bool HasCombatPathTo(HexTile target){
        List<HexTile> path = PathGenerator.Instance.GetPath (this, target, PATHFINDING_MODE.AVATAR);
		if(path != null){
			return true;
		}
		return false;
	}

    #region For Testing
    //private void HighlightTilesInRegion() {
    //    for (int i = 0; i < _region.tilesInRegion.Count; i++) {
    //        HexTile currTileInRegion = _region.tilesInRegion[i];
    //        currTileInRegion.SetTileHighlightColor(Color.gray);
    //        currTileInRegion.ShowTileHighlight();
    //    }
    //}

    //private void UnHighlightTilesInRegion() {
    //    for (int i = 0; i < _region.tilesInRegion.Count; i++) {
    //        HexTile currTileInRegion = _region.tilesInRegion[i];
    //        currTileInRegion.HideTileHighlight();
    //    }
    //}

    [Space(10)]
    [Header("For Testing")]
    //[SerializeField] private int kingdomToConquerIndex = 0;
    [SerializeField] private int range = 0;
    List<HexTile> tiles = new List<HexTile>();
    [ContextMenu("Show Tiles In Range")]
    public void ShowTilesInRange() {
        for (int i = 0; i < tiles.Count; i++) {
            tiles[i].GetComponent<SpriteRenderer>().color = Color.white;
        }
        tiles.Clear();
        tiles.AddRange(this.GetTilesInRange(range));
        for (int i = 0; i < tiles.Count; i++) {
            tiles[i].GetComponent<SpriteRenderer>().color = Color.magenta;
        }
    }
    //[ContextMenu("Show Border Tiles")]
    //public void ShowBorderTiles() {
    //    for (int i = 0; i < this.city.borderTiles.Count; i++) {
    //        this.city.borderTiles[i].GetComponent<SpriteRenderer>().color = Color.magenta;
    //    }
    //}
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
    //[ContextMenu("Select Tiles in Same Region")]
    //public void SelectAllTilesInRegion() {
    //    UnityEditor.Selection.objects = region.tilesInRegion.Select(x => x.gameObject).ToArray();
    //}
    //[ContextMenu("Select All Relevant Tiles")]
    //public void SelectAllRelevantTiles() {
    //    List<GameObject> allTiles = new List<GameObject>();
    //    allTiles.AddRange(city.borderTiles.Select(x => x.gameObject));
    //    allTiles.AddRange(city.ownedTiles.Select(x => x.gameObject));
    //    allTiles.AddRange(city.outerTiles.Select(x => x.gameObject));
    //    UnityEditor.Selection.objects = allTiles.ToArray();
    //}

    //[ContextMenu("Select All Border Tiles")]
    //public void SelectAllBorderTiles() {
    //    List<GameObject> allTiles = new List<GameObject>();
    //    allTiles.AddRange(city.borderTiles.Select(x => x.gameObject));
    //    UnityEditor.Selection.objects = allTiles.ToArray();
    //}

    //[ContextMenu("Select All Outer Tiles")]
    //public void SelectAllOuterTiles() {
    //    List<GameObject> allTiles = new List<GameObject>();
    //    allTiles.AddRange(city.outerTiles.Select(x => x.gameObject));
    //    UnityEditor.Selection.objects = allTiles.ToArray();
    //}
    //[ContextMenu("Toggle Militarize")]
    //public void ToggleMilitarize() {
    //    ownedByCity.kingdom.Militarize(!ownedByCity.kingdom.isMilitarize);
    //}
    //[ContextMenu("Toggle Fortify")]
    //public void ToggleFortify() {
    //    ownedByCity.kingdom.Fortify(!ownedByCity.kingdom.isFortifying);
    //}
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
    //[ContextMenu("Make King Determine Weighted Action")]
    //public void MakeKingDetermineWeightedAction() {
    //    this.city.kingdom.PerformWeightedAction();
    //}
    //private void ShowRegionInfo() {
    //    string text = string.Empty;
    //    text += "[b]Tile:[/b] " + this.name + "\n";
    //    text += "[b]Connections:[/b] " + this.region.connections.Count.ToString();
    //    for (int i = 0; i < this.region.connections.Count; i++) {
    //        object currConnection = this.region.connections[i];
    //        if (currConnection is Region) {
    //            text += "\n Region - " + ((Region)currConnection).centerOfMass.name;
    //        } else {
				//text += "\n " + currConnection.ToString() + " - " + ((BaseLandmark)currConnection).tileLocation.name;
    //        }
    //    }
    //    UIManager.Instance.ShowSmallInfo(text);
    //}
    //private void HideRegionInfo() {
    //    HideSmallInfoWindow();
    //    for (int i = 0; i < this.region.connections.Count; i++) {
    //        object currConnection = this.region.connections[i];
    //        //if (currConnection is Region) {
    //        //    ((Region)currConnection).centerOfMass.UnHighlightTilesInRegion();
    //        //} else if (currConnection is HexTile) {
    //        //    ((HexTile)currConnection).HideTileHighlight();
    //        //}
    //    }
    //}
    private void ShowHexTileInfo() {
        string text = string.Empty;
        text += "Characters in tile: ";
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            ICombatInitializer currObj = _charactersAtLocation[i];
            if (currObj is Party) {
                text += "\n" + ((Party)currObj).name;
            } else if (currObj is Character) {
                text += "\n" + ((Character)currObj).name;
            }
        }
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

        //        text += "[b]Tile:[/b] " + this.name + "\n";
        //        text += "[b]Connections:[/b] " + this.landmarkOnTile.connections.Count.ToString();
        //        for (int i = 0; i < this.landmarkOnTile.connections.Count; i++) {
        //            object currConnection = this.landmarkOnTile.connections[i];
        //            if(currConnection is Region) {
        //                text += "\n Region - " + ((Region)currConnection).centerOfMass.name;
        //            } else {
        //                text += "\n " + currConnection.ToString() + " - " + ((BaseLandmark)currConnection).location.name;
        //            }
        //        }
        //        if (this.landmarkOnTile.owner != null) {
        //            text += "\n[b]Owner:[/b] " + this.landmarkOnTile.owner.name + "/" + this.landmarkOnTile.owner.race.ToString();
        //            text += "\n[b]Total Population: [/b] " + this.landmarkOnTile.totalPopulation.ToString();
        //            text += "\n[b]Civilian Population: [/b] " + this.landmarkOnTile.civiliansWithReserved.ToString();
        //            text += "\n[b]Population Growth: [/b] " + (this.landmarkOnTile.totalPopulation * this.landmarkOnTile.location.region.populationGrowth).ToString();
        ////            text += "\n[b]Characters: [/b] ";
        ////            if (landmarkOnTile.charactersOnLandmark.Count > 0) {
        ////                for (int i = 0; i < landmarkOnTile.charactersOnLandmark.Count; i++) {
        ////                    Character currChar = landmarkOnTile.charactersOnLandmark[i];
        ////                    text += "\n" + currChar.name + " - " + currChar.characterClass.className + "/" + currChar.role.roleType.ToString();
        ////                    if (currChar.currentQuest != null) {
        ////                        text += " " + currChar.currentQuest.questType.ToString();
        ////                    }
        ////                }
        ////            } else {
        ////                text += "NONE";
        ////            }

        //            text += "\n[b]Character Caps: [/b] ";
        //            for (int i = 0; i < LandmarkManager.Instance.characterProductionWeights.Count; i++) {
        //                CharacterProductionWeight currWweight = LandmarkManager.Instance.characterProductionWeights[i];
        //                bool isCapReached = false;
        //                for (int j = 0; j < currWweight.productionCaps.Count; j++) {
        //                    CharacterProductionCap cap = currWweight.productionCaps[j];
        //                    if(cap.IsCapReached(currWweight.role, this.landmarkOnTile.owner)) {
        //                        isCapReached = true;
        //                        break;
        //                    }
        //                }
        //                text += "\n" + currWweight.role.ToString() + " - " + isCapReached.ToString();
        //            }

        //            text += "\n[b]Active Quests: [/b] ";
        //            if (landmarkOnTile.owner.internalQuestManager.activeQuests.Count > 0) {
        //                for (int i = 0; i < landmarkOnTile.owner.internalQuestManager.activeQuests.Count; i++) {
        //                    OldQuest.Quest currQuest = landmarkOnTile.owner.internalQuestManager.activeQuests[i];
        //                    text += "\n" + currQuest.GetType().ToString();
        //                }
        //            } else {
        //                text += "NONE";
        //            }
        //        }
        //        text += "\n[b]Technologies: [/b] ";
        //        List<TECHNOLOGY> availableTech = this.landmarkOnTile.technologies.Where(x => x.Value == true).Select(x => x.Key).ToList();
        //        if (availableTech.Count > 0) {
        //            for (int i = 0; i < availableTech.Count; i++) {
        //                TECHNOLOGY currTech = availableTech[i];
        //                text += currTech.ToString();
        //                if(i + 1 != availableTech.Count) {
        //                    text += ", ";
        //                }
        //            }
        //        } else {
        //            text += "NONE";
        //        }
        UIManager.Instance.ShowSmallInfo(text);
    }
    //private void HideLandmarkInfo() {
    //    HideSmallInfoWindow();
    //    //for (int i = 0; i < this.landmark.connections.Count; i++) {
    //    //    object currConnection = this.landmark.connections[i];
    //    //    if (currConnection is Region) {
    //    //        ((Region)currConnection).centerOfMass.UnHighlightTilesInRegion();
    //    //    } else if (currConnection is HexTile) {
    //    //        ((HexTile)currConnection).HideTileHighlight();
    //    //    }
    //    //}
    //}
//    private void ShowKingdomInfo() {
//        string text = this.city.name + " HP: " + this.city.hp.ToString() + "/" + this.city.maxHP.ToString() + "\n";
//        text += "[b]Tile:[/b] " + this.name + "\n";
//        text += "[b]" + this.city.kingdom.name + "[/b]" +
//        "\n [b]King Character Type:[/b] " + this.city.kingdom.king.characterType.characterTypeName +
//        "\n [b]Connections:[/b] " + this.region.connections.Count.ToString();
//        for (int i = 0; i < this.region.connections.Count; i++) {
//            object currConnection = this.region.connections[i];
//            text += "\n " + currConnection.ToString();
//            if (currConnection is Region) {
//                text += " - " + ((Region)currConnection).centerOfMass.name;
//            }
//        }
//        //text += "\n [b]Special Resource:[/b] " + this.city.region.specialResource.ToString();
//        //if(this.city.region.specialResource != RESOURCE.NONE) {
//        //    text += "\n [b]Special Resource Loc:[/b] " + this.city.region.tileWithSpecialResource.name;
//        //}
//		text += "\n [b]Food Count:[/b] " + this.city.foodCount.ToString () + "/" + this.city.foodCapacity + "(" + this.city.foodRequirement.ToString () + ")" +
//		"\n [b]Material Count:[/b] " + this.city.materialCount.ToString () + "/" + this.city.materialCapacity + "(" + this.city.materialRequirement.ToString () + ")" +
//		"\n [b]Material Count For Humans:[/b] " + this.city.materialCountForHumans.ToString () + "/" + this.city.materialCapacity +
//		"\n [b]Material Count For Elves:[/b] " + this.city.materialCountForElves.ToString () + "/" + this.city.materialCapacity +
//		"\n [b]Ore Count:[/b] " + this.city.oreCount.ToString () + "/" + this.city.oreCapacity + "(" + this.city.oreRequirement.ToString () + ")" +
//		"\n [b]Kingdom Food S/D:[/b] " + this.city.kingdom.cities.Count.ToString () + "/" + this.city.kingdom.foodCityCapacity +
//		"\n [b]Kingdom Material For Humans S/D:[/b] " + ((this.city.kingdom.race == RACE.HUMANS) ? this.city.kingdom.cities.Count.ToString () : "0") + "/" + this.city.kingdom.materialCityCapacityForHumans +
//		"\n [b]Kingdom Material For Elves S/D:[/b] " + ((this.city.kingdom.race == RACE.ELVES) ? this.city.kingdom.cities.Count.ToString () : "0") + "/" + this.city.kingdom.materialCityCapacityForElves +
//		"\n [b]Kingdom Ore S/D:[/b] " + this.city.kingdom.cities.Count.ToString () + "/" + this.city.kingdom.oreCityCapacity +
//		"\n [b]Power Points:[/b] " + this.city.powerPoints.ToString() +
//        "\n [b]Defense Points:[/b] " + this.city.defensePoints.ToString() +
//        "\n [b]Tech Points:[/b] " + this.city.techPoints.ToString() +
//        "\n [b]Kingdom Base Weapons:[/b] " + this.city.kingdom.baseWeapons.ToString() +
////        "\n [b]Kingdom Base Armor:[/b] " + this.city.kingdom.baseArmor.ToString() +
//        "\n [b]Weapons Over Production:[/b] " + this.city.kingdom.GetWeaponOverProductionPercentage().ToString() + "%" + 
////        "\n [b]Armor Over Production:[/b] " + this.city.kingdom.GetArmorOverProductionPercentage().ToString() + "%" +
//        "\n [b]City Level Cap:[/b] " + this.region.cityLevelCap.ToString() +
//		"\n [b]Population Count:[/b] " + this.city.population.ToString() + "/" + this.city.populationCapacity.ToString() +
//		"\n [b]PopulationF Count:[/b] " + this.city._population.ToString() +
//		"\n [b]City Population Growth:[/b] " + this.region.populationGrowth.ToString() +
//        "\n [b]Kingdom Type:[/b] " + this.city.kingdom.kingdomType.ToString() +
//        "\n [b]Kingdom Size:[/b] " + this.city.kingdom.kingdomSize.ToString() +
//        "\n [b]Growth Rate: [/b]" + this.city.totalDailyGrowth.ToString() +
//        "\n [b]Draft Rate: [/b]" + (this.city.kingdom.draftRate * 100f).ToString() + "%" +
//        "\n [b]Research Rate: [/b]" + (this.city.kingdom.researchRate * 100f).ToString() + "%" +
//        "\n [b]Production Rate: [/b]" + (this.city.kingdom.productionRate * 100f).ToString() + "%" +
//        "\n [b]Current Growth: [/b]" + this.city.currentGrowth.ToString() + "/" + this.city.maxGrowth.ToString() + "\n";

//        text += "[b]Trade Deals: [/b]\n";
//        if (this.city.kingdom.kingdomsInTradeDealWith.Count > 0) {
//            for (int i = 0; i < this.city.kingdom.kingdomsInTradeDealWith.Count; i++) {
//                text += this.city.kingdom.kingdomsInTradeDealWith[i].name + "\n";
//            }
//        } else {
//            text += "NONE\n";
//        }

//        text += "[b]Adjacent Kingdoms: [/b]\n";
//        if (this.city.kingdom.adjacentKingdoms.Count > 0) {
//            for (int i = 0; i < this.city.kingdom.adjacentKingdoms.Count; i++) {
//                text += this.city.kingdom.adjacentKingdoms[i].name + "\n";
//            }
//        } else {
//            text += "NONE\n";
//        }

//        //text += "[b]Discovered Kingdoms: [/b]\n";
//        //if (this.city.kingdom.discoveredKingdoms.Count > 0) {
//        //    for (int i = 0; i < this.city.kingdom.discoveredKingdoms.Count; i++) {
//        //        Kingdom currKingdom = this.city.kingdom.discoveredKingdoms[i];
//        //        text += currKingdom.name + "\n";
//        //    }
//        //} else {
//        //    text += "NONE\n";
//        //}

//        //text += "[b]Alliance Kingdoms: [/b]\n";
//        //if(this.city.kingdom.alliancePool != null) {
//        //    for (int i = 0; i < this.city.kingdom.alliancePool.kingdomsInvolved.Count; i++) {
//        //        Kingdom currKingdom = this.city.kingdom.alliancePool.kingdomsInvolved[i];
//        //        text += currKingdom.name + "\n";
//        //    }
//        //} else {
//        //    text += "NONE\n";
//        //}

//        //text += "[b]Important Citizens in City: [/b]\n";
//        //if (this.city.importantCitizensInCity.Count > 0) {
//        //    for (int i = 0; i < this.city.importantCitizensInCity.Count; i++) {
//        //        text += this.city.importantCitizensInCity.Values.ElementAt(i).name + " - " + this.city.importantCitizensInCity.Keys.ElementAt(i).ToString() + "\n";
//        //    }
//        //} else {
//        //    text += "NONE\n";
//        //}

//        //text += "[b]Citizens in City: [/b]\n";
//        //if (this.city.citizens.Count > 0) {
//        //    for (int i = 0; i < this.city.citizens.Count; i++) {
//        //        Citizen currCitizen = this.city.citizens[i];
//        //        if (currCitizen.role != ROLE.UNTRAINED) {
//        //            text += currCitizen.role + " - " + currCitizen.name + "\n";
//        //        }
//        //    }
//        //} else {
//        //    text += "NONE\n";
//        //}

//        //text += "[b]Kingdom values: [/b]\n";
//        //Dictionary<CHARACTER_VALUE, int> kingdomVals = this.city.kingdom.importantCharacterValues;
//        //if (kingdomVals.Count > 0) {
//        //    for (int i = 0; i < kingdomVals.Count(); i++) {
//        //        KeyValuePair<CHARACTER_VALUE, int> kvp = kingdomVals.ElementAt(i);
//        //        text += kvp.Key.ToString() + " - " + kvp.Value.ToString() + "\n";
//        //    }
//        //} else {
//        //    text += "NONE\n";
//        //}
//        UIManager.Instance.ShowSmallInfo(text);
//    }
    private void HideSmallInfoWindow() {
        UIManager.Instance.HideSmallInfo();
    }
    public override string ToString() {
        return this.tileName;
    }
    #endregion

    internal float GetDistanceTo(HexTile targetHextile){
		return Vector3.Distance (this.transform.position, targetHextile.transform.position);
	}
	
    public void SetTag(int tag) {
        this.tileTag = tag;
        //tileTextMesh.text = xCoordinate.ToString() + "," + yCoordinate.ToString();
        //tileTextMesh.gameObject.SetActive(true);
    }

    public void SetTileText(string text, int fontSize, Color fontColor, string layer = "Default") {
        tileTextMesh.text = text;
        tileTextMesh.characterSize = fontSize;
        tileTextMesh.color = fontColor;
        tileTextMesh.gameObject.layer = LayerMask.NameToLayer(layer);
        tileTextMesh.transform.localPosition = Vector3.zero;
        tileTextMesh.gameObject.SetActive(true);
    }

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

	//internal void SetCorpseMound(CorpseMound corpseMound){
	//	this._corpseMound = corpseMound;
	//	if(corpseMound != null){
	//		this.region.AddCorpseMoundTile (this);
	//	}else{
	//		if(this.region.corpseMoundTiles.Count > 0){
	//			this.region.RemoveCorpseMoundTile (this);
	//		}
	//	}
	//}
	internal bool IsAdjacentWithRegion(Region region){
		List<HexTile> neighbors = this.AllNeighbours;
		for (int i = 0; i < neighbors.Count; i++) {
			if(neighbors[i].region.id == region.id){
				return true;
			}
		}
		return false;
	}

	//internal void DestroyConnections(){
	//	while(this.connectedTiles.Count > 0){
	//		RoadManager.Instance.DestroyConnection (this, this.connectedTiles.Keys.ElementAt(0));
	//	}
	//}

	internal int GetNumOfConnectedCenterOfMass(){
		int count = 0;
		if(this.connectedTiles.Count > 0){
			foreach (HexTile tile in this.connectedTiles.Keys) {
				if(tile.region.centerOfMass.id == tile.id){
					count += 1;
				}
			}
		}
		return count;
	}

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
        if (_charactersAtLocation.Count <= 0) {
            return null;
        }
        return _charactersAtLocation.Where(x => x.currentTask.combatPriority > 0).OrderByDescending(x => x.currentTask.combatPriority).ToList();
    }
    //public void CheckAttackingGroupsCombat() {
    //    List<ICombatInitializer> attackingGroups = GetAttackingGroups();
    //    for (int i = 0; i < attackingGroups.Count; i++) {
    //        ICombatInitializer currAttackingGroup = attackingGroups[i];
    //        if (currAttackingGroup.isInCombat) {
    //            continue; //this current group is already in combat, skip it
    //        }
    //        //- If there are hostile parties in combat stance who are not engaged in combat, the attacking character will initiate combat with one of them at random
    //        List<ICombatInitializer> combatGroups = new List<ICombatInitializer>(GetGroupsBasedOnStance(STANCE.COMBAT, true, currAttackingGroup).Where(x => x.IsHostileWith(currAttackingGroup)));
    //        if (combatGroups.Count > 0) {
    //            ICombatInitializer chosenEnemy = combatGroups[Random.Range(0, combatGroups.Count)];
    //            StartCombatBetween(currAttackingGroup, chosenEnemy);
    //            continue; //the attacking group has found an enemy! skip to the next group
    //        }

    //        //Otherwise, if there are hostile parties in neutral stance who are not engaged in combat, the attacking character will initiate combat with one of them at random
    //        List<ICombatInitializer> neutralGroups = new List<ICombatInitializer>(GetGroupsBasedOnStance(STANCE.NEUTRAL, true, currAttackingGroup).Where(x => x.IsHostileWith(currAttackingGroup)));
    //        if (neutralGroups.Count > 0) {
    //            ICombatInitializer chosenEnemy = neutralGroups[Random.Range(0, neutralGroups.Count)];
    //            StartCombatBetween(currAttackingGroup, chosenEnemy);
    //            continue; //the attacking group has found an enemy! skip to the next group
    //        }

    //        //- Otherwise, if there are hostile parties in stealthy stance who are not engaged in combat, the attacking character will attempt to initiate combat with one of them at random.
    //        List<ICombatInitializer> stealthGroups = new List<ICombatInitializer>(GetGroupsBasedOnStance(STANCE.STEALTHY, true, currAttackingGroup).Where(x => x.IsHostileWith(currAttackingGroup)));
    //        if (stealthGroups.Count > 0) {
    //            //The chance of initiating combat is 35%
    //            if (Random.Range(0, 100) < 35) {
    //                ICombatInitializer chosenEnemy = stealthGroups[Random.Range(0, stealthGroups.Count)];
    //                StartCombatBetween(currAttackingGroup, chosenEnemy);
    //                continue; //the attacking group has found an enemy! skip to the next group
    //            }
    //        }
    //    }
    //}
    //public void CheckPatrollingGroupsCombat() {
    //    List<ICombatInitializer> patrollingGroups = GetPatrollingGroups();
    //    for (int i = 0; i < patrollingGroups.Count; i++) {
    //        ICombatInitializer currPatrollingGroup = patrollingGroups[i];
    //        if (currPatrollingGroup.isInCombat) {
    //            continue; //this current group is already in combat, skip it
    //        }
    //        //- If there are hostile parties in combat stance who are not engaged in combat, the attacking character will initiate combat with one of them at random
    //        List<ICombatInitializer> combatGroups = new List<ICombatInitializer>(GetGroupsBasedOnStance(STANCE.COMBAT, true, currPatrollingGroup).Where(x => x.IsHostileWith(currPatrollingGroup)));
    //        if (combatGroups.Count > 0) {
    //            ICombatInitializer chosenEnemy = combatGroups[Random.Range(0, combatGroups.Count)];
    //            StartCombatBetween(currPatrollingGroup, chosenEnemy);
    //            continue; //the attacking group has found an enemy! skip to the next group
    //        }

    //        //Otherwise, if there are hostile parties in neutral stance who are not engaged in combat, the attacking character will initiate combat with one of them at random
    //        List<ICombatInitializer> neutralGroups = new List<ICombatInitializer>(GetGroupsBasedOnStance(STANCE.NEUTRAL, true, currPatrollingGroup).Where(x => x.IsHostileWith(currPatrollingGroup)));
    //        if (neutralGroups.Count > 0) {
    //            ICombatInitializer chosenEnemy = neutralGroups[Random.Range(0, neutralGroups.Count)];
    //            StartCombatBetween(currPatrollingGroup, chosenEnemy);
    //            continue; //the attacking group has found an enemy! skip to the next group
    //        }

    //        //- Otherwise, if there are hostile parties in stealthy stance who are not engaged in combat, the attacking character will attempt to initiate combat with one of them at random
    //        List<ICombatInitializer> stealthGroups = new List<ICombatInitializer>(GetGroupsBasedOnStance(STANCE.STEALTHY, true, currPatrollingGroup).Where(x => x.IsHostileWith(currPatrollingGroup)));
    //        if (stealthGroups.Count > 0) {
    //            //The chance of initiating combat is 35%
    //            if (Random.Range(0, 100) < 35) {
    //                ICombatInitializer chosenEnemy = stealthGroups[Random.Range(0, stealthGroups.Count)];
    //                StartCombatBetween(currPatrollingGroup, chosenEnemy);
    //                continue; //the attacking group has found an enemy! skip to the next group
    //            }
    //        }
    //    }
    //}
    public bool HasCombatInitializers() {
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            ICombatInitializer currChar = _charactersAtLocation[i];
            if (currChar.currentTask.combatPriority > 0) {
                return true;
            }
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
    //public List<ICombatInitializer> GetAttackingGroups() {
    //    List<ICombatInitializer> groups = new List<ICombatInitializer>();
    //    for (int i = 0; i < _charactersAtLocation.Count; i++) {
    //        ICombatInitializer currGroup = _charactersAtLocation[i];
    //        if (currGroup.currentTask is Invade) {
    //            groups.Add(currGroup);
    //        }
    //    }
    //    return groups;
    //}
    //public List<ICombatInitializer> GetPatrollingGroups() {
    //    List<ICombatInitializer> groups = new List<ICombatInitializer>();
    //    for (int i = 0; i < _charactersAtLocation.Count; i++) {
    //        ICombatInitializer currGroup = _charactersAtLocation[i];
    //        if (currGroup.currentTask is Patrol) {
    //            groups.Add(currGroup);
    //        }
    //    }
    //    return groups;
    //}
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
        CombatThreadPool.Instance.AddToThreadPool(combat);
    }
    public void ContinueDailyActions() {
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            ICombatInitializer currItem = _charactersAtLocation[i];
            currItem.ContinueDailyAction();
        }
    }
    //   public void StartCombatAtLocation(){
    //	if(!CombatAtLocation()){
    //		this._currentCombat = null;
    //		for (int i = 0; i < _charactersAtLocation.Count; i++) {
    //               ICombatInitializer currItem = _charactersAtLocation[i];
    //               currItem.SetIsDefeated (false);
    //			currItem.SetIsInCombat (false);
    //			if(currItem.currentFunction != null){
    //				currItem.currentFunction ();
    //			}
    //			currItem.SetCurrentFunction(null);
    //           }
    //       } else {
    //           for (int i = 0; i < _charactersAtLocation.Count; i++) {
    //               ICombatInitializer currItem = _charactersAtLocation[i];
    //			currItem.SetIsInCombat (false);
    //           }
    //       }
    //}
    //   public bool CombatAtLocation(){
    //	for (int i = 0; i < _charactersAtLocation.Count; i++) {
    //		if(_charactersAtLocation[i].InitializeCombat()){
    //			return true;
    //		}
    //	}
    //	return false;
    //}
    //   public ICombatInitializer GetCombatEnemy (ICombatInitializer combatInitializer) {
    //	for (int i = 0; i < _charactersAtLocation.Count; i++) {
    //		if(_charactersAtLocation[i] != combatInitializer){
    //			if(_charactersAtLocation[i] is Party){
    //				if(((Party)_charactersAtLocation[i]).isDefeated){
    //					continue;
    //				}
    //			}
    //			if(combatInitializer.IsHostileWith(_charactersAtLocation[i])){
    //				return _charactersAtLocation [i];
    //			}
    //		}
    //	}
    //	return null;
    //}
    //   public void SetCurrentCombat(CombatPrototype combat) {
    //	_currentCombat = combat;
    //}
    #endregion

    #region Materials
    public void SetMaterialOnTile(MATERIAL material) {
        _materialOnTile = material;
        GameObject resource = GameObject.Instantiate(Biomes.Instance.ebonyPrefab, resourceParent) as GameObject;
        resource.transform.localPosition = Vector3.zero;
        resource.transform.localScale = Vector3.one;
        region.AddTileWithMaterial(this);
    }
    #endregion
}
