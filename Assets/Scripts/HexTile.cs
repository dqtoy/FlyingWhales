using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathFind;
using System.Linq;
using Panda;
using Pathfinding;

public class HexTile : MonoBehaviour,  IHasNeighbours<HexTile>{
    [Header("General Tile Details")]
    public int id;
    public int xCoordinate;
	public int yCoordinate;
    public int tileTag;
	public string tileName;
    private Region _region;

    [Space(10)]
    [Header("Biome Settings")]
    public float elevationNoise;
	public float moistureNoise;
	public float temperature;
	public BIOMES biomeType;
	public ELEVATION elevationType;
	public int movementDays;

    [Space(10)]
    [Header("Pathfinding Objects")]
    [SerializeField] private GameObject pathfindingGO; //Place this in tag Water, Mountain or Plain when biome & elevation is determined
    [SerializeField] private GraphUpdateScene gus; //This is used to tag the tiles
    [SerializeField] private Collider2D _pathfindingCollider;

    [Space(10)]
    [Header("Resources")]
    public RESOURCE specialResource;
    public int nearbyResourcesCount = 0;

    [System.NonSerialized] public City city = null;
	internal City ownedByCity = null; // this is populated whenever the hex tile is occupied or becomes a border of a particular city

	public Lair lair;

	private List<Lair> _lairsInRange = new List<Lair>();

    [Space(10)]
    [Header("Booleans")]
    public bool isHabitable = false;
	public bool isRoad = false;
	public bool isOccupied = false;
	public bool isBorder = false;
	public bool isPlagued = false;
	public bool isTargeted = false;
	public bool hasKeystone = false;
	public bool hasFirst = false;
	public bool isLair = false;
	public bool hasLandmark = false;

    [SerializeField] private List<City> _isBorderOfCities = new List<City>();
    [SerializeField] private List<City> _isOuterTileOfCities = new List<City>();
    [SerializeField] private List<Kingdom> _visibleByKingdoms = new List<Kingdom>(); //This is only occupied when a tile becomes occupies, becaoms a border or becomes an outer tile of a city!

    [Space(10)]
    [Header("Tile Visuals")]
    [SerializeField] private GameObject _centerPiece;
    [SerializeField] private ResourceIcon resourceIcon;
	[SerializeField] private SpriteRenderer _kingdomColorSprite;
	[SerializeField] private GameObject _highlightGO;
    [SerializeField] private Transform UIParent;
    [SerializeField] private Transform resourceParent;
    [SerializeField] private GameObject biomeDetailParentGO;
    [SerializeField] private TextMesh tileTextMesh;
	[SerializeField] private GameObject _emptyCityGO;

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
    [Header("Game Event Objects")]
    [SerializeField] private GameObject plagueIconGO;
    [SerializeField] private GameObject gameEventObjectsParentGO;


    private GameEvent _gameEventInTile;
    private Transform _namePlateParent;
    private CityItem _cityInfo;
	private LairItem _lairItem;
	private HextileEventItem _hextileEventItem;
	private GameObject plagueIcon;
	private GameObject _corpseMoundGO = null;
	private CorpseMound _corpseMound = null;
	private GameDate _corpseMoundDestroyDate;

    private List<Citizen> _citizensOnTile = new List<Citizen>();
    private Dictionary<HEXTILE_DIRECTION, HexTile> _neighbourDirections;

	[System.NonSerialized] public Dictionary<HexTile, RoadConnection> connectedTiles = new Dictionary<HexTile, RoadConnection>();
//	[System.NonSerialized] public List<GameObject> connectionsGO = new List<GameObject>();

	public List<HexTile> AllNeighbours { get; set; }
	public List<HexTile> ValidTiles { get { return AllNeighbours.Where(o => o.elevationType != ELEVATION.WATER && o.elevationType != ELEVATION.MOUNTAIN).ToList();}}
	public List<HexTile> RoadTiles { get { return AllNeighbours.Where(o => o.isRoad).ToList(); } }
	public List<HexTile> PurchasableTiles { get { return AllNeighbours.Where (o => o.elevationType != ELEVATION.WATER).ToList();}}
	public List<HexTile> CombatTiles { get { return AllNeighbours.Where (o => o.elevationType != ELEVATION.WATER).ToList();}}
    public List<HexTile> AvatarTiles { get { return AllNeighbours.Where(o => o.elevationType != ELEVATION.WATER).ToList();}}

    public List<HexTile> elligibleNeighbourTilesForPurchase { get { return PurchasableTiles.Where(o => !o.isOccupied && !o.isHabitable).ToList(); } }

    public List<HexTile> sameTagNeighbours;

	//private List<WorldEventItem> eventsOnTile = new List<WorldEventItem>();

	#region getters/setters
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
	public CityItem cityInfo{
		get { return this._cityInfo; }
	}
	public LairItem lairItem{
		get { return this._lairItem; }
	}
	public HextileEventItem hextileEventItem{
		get { return this._hextileEventItem; }
	}
	public GameEvent gameEventInTile{
		get { return this._gameEventInTile; }
	}
    public FOG_OF_WAR_STATE currFogOfWarState {
        get { return _currFogOfWarState; }
    }
    public List<Citizen> citizensOnTile {
        get { return this._citizensOnTile; }
    }
    public List<City> isBorderOfCities {
        get { return _isBorderOfCities; }
    }
    public List<City> isOuterTileOfCities {
        get { return _isOuterTileOfCities; }
    }
    public List<Kingdom> visibleByKingdoms {
        get { return _visibleByKingdoms; }
    }
    internal Dictionary<HEXTILE_DIRECTION, HexTile> neighbourDirections {
        get { return _neighbourDirections; }
    }
    internal Collider2D pathfindingCollider {
        get { return _pathfindingCollider; }
    }
    public CorpseMound corpseMound{
		get { return this._corpseMound; }
	}
	public GameObject emptyCityGO{
		get { return this._emptyCityGO; }
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
        switch (elevationType) {
            case ELEVATION.MOUNTAIN:
                //AddPathfindingTag(PathfindingManager.mountainTag);
                pathfindingGO.layer = LayerMask.NameToLayer("Pathfinding_Mountains");
                break;
            case ELEVATION.WATER:
                //AddPathfindingTag(PathfindingManager.waterTag);
                pathfindingGO.layer = LayerMask.NameToLayer("Pathfinding_Water");
                break;
            case ELEVATION.PLAIN:
                //AddPathfindingTag(PathfindingManager.plainTag);
                pathfindingGO.layer = LayerMask.NameToLayer("Pathfinding_Plains");
                break;
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

	#region Resource
    internal void AssignSpecialResource(RESOURCE resource) {
		this.hasLandmark = true;
        specialResource = resource;
        resourceIcon.SetResource(specialResource);
        GameObject resourceGO = GameObject.Instantiate(Biomes.Instance.GetPrefabForResource(this.specialResource), resourceParent) as GameObject;
        resourceGO.transform.localPosition = Vector3.zero;
        resourceGO.transform.localScale = Vector3.one;
    }
	internal void CreateSummoningShrine(){
		this.hasLandmark = true;
		GameObject shrineGO = GameObject.Instantiate(CityGenerator.Instance.GetSummoningShrineGO(), structureParentGO.transform) as GameObject;
		shrineGO.transform.localPosition = Vector3.zero;
		shrineGO.transform.localScale = Vector3.one;
	}
	internal void CreateHabitat(){
		this.hasLandmark = true;
		GameObject habitatGO = GameObject.Instantiate(CityGenerator.Instance.GetHabitatGO(), structureParentGO.transform) as GameObject;
		habitatGO.transform.localPosition = Vector3.zero;
		habitatGO.transform.localScale = Vector3.one;
	}
    internal void AssignSpecialResource(){
		if (this.elevationType == ELEVATION.WATER || this.elevationType == ELEVATION.MOUNTAIN) {
			return;
		}
		int specialChance = UnityEngine.Random.Range (0, 100);
        int specialChanceForBiome = 0;

        if (this.biomeType == BIOMES.GRASSLAND || this.biomeType == BIOMES.WOODLAND || this.biomeType == BIOMES.FOREST) {
            specialChanceForBiome = 5;
        } else if (this.biomeType == BIOMES.DESERT) {
            specialChanceForBiome = 3;
        } else if (this.biomeType == BIOMES.TUNDRA || this.biomeType == BIOMES.SNOW) {
            specialChanceForBiome = 3;
        }

		if (specialChance < specialChanceForBiome) {
			this.specialResource = ComputeSpecialResource (Utilities.specialResourcesLookup [this.biomeType]);
			if (this.specialResource != RESOURCE.NONE) {
                resourceIcon.SetResource(specialResource);
                GameObject resource = GameObject.Instantiate(Biomes.Instance.GetPrefabForResource(this.specialResource), resourceParent) as GameObject;
                resource.transform.localPosition = Vector3.zero;
                resource.transform.localScale = Vector3.one;
            }
		} else {
			this.specialResource = RESOURCE.NONE;
		}
    }
	private RESOURCE ComputeSpecialResource(SpecialResourceChance specialResources){
		int totalChance = 0;
		int lowerLimit = 0;
		int upperLimit = specialResources.chance [0];
		for(int i = 0; i < specialResources.resource.Length; i++){
			totalChance += specialResources.chance[i];
		}

		int chance = UnityEngine.Random.Range (0, totalChance);
		for(int i = 0; i < specialResources.resource.Length; i++){
			if(chance >= lowerLimit && chance < upperLimit){
				return specialResources.resource[i];
			}else{
				lowerLimit = upperLimit;
				if (i + 1 < specialResources.resource.Length) {
					upperLimit += specialResources.chance [i + 1];
				}
			}
		}
		return RESOURCE.NONE;
	}
    #endregion

    #region Tile Utilities
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
    internal void SetPathfindingTag(int pathfindingTag) {
        gus.setTag = pathfindingTag;
        gus.Apply();
    }
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
			int neighbourCoordinateX = xCoordinate + possibleExits [i].X;
			int neighbourCoordinateY = yCoordinate + possibleExits [i].Y;
			if (neighbourCoordinateX >= 0 && neighbourCoordinateX < gameBoard.GetLength(0) && neighbourCoordinateY >= 0 && neighbourCoordinateY < gameBoard.GetLength(1)){
                HexTile currNeighbour = gameBoard[neighbourCoordinateX, neighbourCoordinateY];
                neighbours.Add (currNeighbour);
            }

		}
		this.AllNeighbours = neighbours;

        for (int i = 0; i < neighbours.Count; i++) {
            HexTile currNeighbour = neighbours[i];
            _neighbourDirections.Add(GetNeighbourDirection(currNeighbour), currNeighbour);
        }
	}
    internal HEXTILE_DIRECTION GetNeighbourDirection(HexTile neighbour) {
        if (!AllNeighbours.Contains(neighbour)) {
            throw new System.Exception(neighbour.name + " is not a neighbour of " + this.name);
        }
        Point difference = new Point((neighbour.xCoordinate - this.xCoordinate),
                    (neighbour.yCoordinate - this.yCoordinate));
        if (this.yCoordinate % 2 == 0) {
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
        return HEXTILE_DIRECTION.NORTH_WEST;
    }
    #endregion

    #region Tile Visuals
    internal void SetSortingOrder(int sortingOrder) {
        GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
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
        if (this.biomeType == BIOMES.FOREST && Utilities.GetBaseResourceType(this.specialResource) == BASE_RESOURCE_TYPE.WOOD && this.elevationType == ELEVATION.PLAIN) {
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

            if(biomeLayerOfHexTile < biomeLayerOfNeighbour || this.elevationType == ELEVATION.WATER) {
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

//                        gameObjectToEdit.GetComponent<SpriteRenderer>().material = mat;
                        //gameObjectToEdit.GetComponent<SpriteRenderer>().material.SetTexture("Alpha (A)", (Texture)spriteMasksToChooseFrom[Random.Range(0, spriteMasksToChooseFrom.Length)]);
                        //					gameObjectToEdit.GetComponent<SpriteRenderer> ().material = materialForTile;
                    }

                }
            }

            
        }
    }
    internal void SetBaseSprite(Sprite baseSprite){
		this.GetComponent<SpriteRenderer>().sprite = baseSprite;
	}
    internal void SetCenterSprite(Sprite centerSprite){
		this.centerPiece.GetComponent<SpriteRenderer>().sprite = centerSprite;
		this.centerPiece.SetActive(true);
	}
    internal void SetTileHighlightColor(Color color){
        //color.a = 30f / 255f;
        this._kingdomColorSprite.color = color;
	}
    internal void SetMinimapTileColor(Color color) {
        color.a = 255f / 255f;
        minimapHexSprite.color = color;
    }
    internal void ShowTileHighlight(){
		this._kingdomColorSprite.gameObject.SetActive(true);
	}
    internal void HideTileHighlight(){
		this.kingdomColorSprite.gameObject.SetActive(false);
	}
    internal void ShowNamePlate() {
        if (_namePlateParent != null) {
            _namePlateParent.gameObject.SetActive(true);
        }
        if (_cityInfo != null) {
            UpdateCityNamePlate();
        }
        if (_lairItem != null) {
            UpdateLairNamePlate();
        }
    }
    internal void HideNamePlate() {
        _namePlateParent.gameObject.SetActive(false);
    }
    /*
     * This will instantiate a new CityItem Prefab and set it's city 
     * according to the passed parameter.
     * */
    internal void CreateCityNamePlate(City city) {
        //Debug.Log("Create nameplate for: " + city.name + " on " + this.name);

        GameObject namePlateGO = UIManager.Instance.InstantiateUIObject("CityNamePlatePanel", UIParent);
        namePlateGO.layer = LayerMask.NameToLayer("HextileNamePlates");
        _namePlateParent = namePlateGO.transform;
        _cityInfo = namePlateGO.GetComponentInChildren<CityItem>();
        namePlateGO.transform.localPosition = new Vector3(0f, -1.45f, 0f);
        Messenger.AddListener("UpdateUI", UpdateCityNamePlate);

        UpdateCityNamePlate();
    }
    internal void UpdateCityNamePlate() {
        if (_currFogOfWarState == FOG_OF_WAR_STATE.VISIBLE) {
            _cityInfo.SetCity(city);
        } else {
            _cityInfo.SetCity(city, false, true);
        }
    }
    internal void RemoveCityNamePlate() {
        if (_namePlateParent != null) {
            ObjectPoolManager.Instance.DestroyObject(_namePlateParent.gameObject);
            _namePlateParent = null;
            _cityInfo = null;
            Messenger.RemoveListener("UpdateUI", UpdateCityNamePlate);
        }
    }
    #endregion

	#region Lair
    internal void CreateLairNamePlate() {
        //Debug.Log("Create lair nameplate on " + this.name);

        GameObject namePlateGO = UIManager.Instance.InstantiateUIObject("LairNamePlatePanel", UIParent);
        namePlateGO.layer = LayerMask.NameToLayer("HextileNamePlates");
        _namePlateParent = namePlateGO.transform;
        _lairItem = namePlateGO.GetComponentInChildren<LairItem>();
        namePlateGO.transform.localPosition = new Vector3(-2.22f, -1.02f, 0f);
        Messenger.AddListener("UpdateUI", UpdateLairNamePlate);

        UpdateLairNamePlate();
	}
    internal void UpdateLairNamePlate() {
		this._lairItem.SetLair(this.lair);
	}
    internal void RemoveLairNamePlate() {
        if (_namePlateParent != null) {
            ObjectPoolManager.Instance.DestroyObject(_namePlateParent.gameObject);
            _namePlateParent = null;
            _lairItem = null;
            Messenger.RemoveListener("UpdateUI", UpdateLairNamePlate);
        }
    }
	internal void AddLairsInRange(Lair lair){
		this._lairsInRange.Add (lair);
	}
	internal void CheckLairsInRange(){
		if (this._lairsInRange.Count > 0) {
			for (int i = 0; i < this._lairsInRange.Count; i++) {
				this._lairsInRange [i].ActivateLair ();
			}
			this._lairsInRange.Clear ();
		}
	}
    #endregion

    #region Events Functions
    internal void CreateEventNamePlate() {
        //Debug.Log("Create " + gameEventInTile.eventType.ToString() + " nameplate on " + this.name);

        GameObject namePlateGO = UIManager.Instance.InstantiateUIObject("EventNamePlatePanel", UIParent);
        namePlateGO.layer = LayerMask.NameToLayer("HextileNamePlates");
        _namePlateParent = namePlateGO.transform;
        _hextileEventItem = namePlateGO.GetComponentInChildren<HextileEventItem>();
        namePlateGO.transform.localPosition = new Vector3(-2.22f, -1.02f, 0f);
        Messenger.AddListener("UpdateUI", UpdateHextileEventNamePlate);

        UpdateHextileEventNamePlate();
    }
    internal void UpdateHextileEventNamePlate() {
        _hextileEventItem.SetHextileEvent(_gameEventInTile);
    }
    internal void RemoveHextileEventNamePlate() {
        if (_namePlateParent != null) {
            ObjectPoolManager.Instance.DestroyObject(_namePlateParent.gameObject);
            _namePlateParent = null;
            _hextileEventItem = null;
            Messenger.RemoveListener("UpdateUI", UpdateHextileEventNamePlate);
        }
    }
    #endregion

    #region Structures Functions
    internal void CreateStructureOnTile(STRUCTURE_TYPE structureType, STRUCTURE_STATE structureState = STRUCTURE_STATE.NORMAL) {
        //Debug.Log("Create " + structureType.ToString() + " on " + this.name);
        GameObject[] gameObjectsToChooseFrom = CityGenerator.Instance.GetStructurePrefabsForRace(this.ownedByCity.kingdom.race, structureType);
        string structureKey = gameObjectsToChooseFrom[Random.Range(0, gameObjectsToChooseFrom.Length)].name;
		GameObject structureGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(structureKey, Vector3.zero, Quaternion.identity, structureParentGO.transform);
        AssignStructureObjectToTile(structureGO.GetComponent<StructureObject>());
		structureGO.transform.localPosition = new Vector3 (0f, -0.85f, 0f);
        structureObjOnTile.Initialize(structureType, this.ownedByCity.kingdom.kingdomColor, structureState, this);

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
	internal CorpseMound CreateCorpseMoundObjectOnTile(int initialCorpseCount) {
		this._corpseMoundGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(CityGenerator.Instance.GetCorpseMoundGO().name,
			Vector3.zero, Quaternion.identity, structureParentGO.transform);
		this._corpseMoundGO.transform.localPosition = Vector3.zero;
		SetCorpseMound (this._corpseMoundGO.GetComponent<CorpseMound>());
		this._corpseMound.Initialize (this, initialCorpseCount);
		return this._corpseMound;
	}
    internal void HideStructures() {
        structureParentGO.SetActive(false);
    }
    internal void ShowStructures() {
        structureParentGO.SetActive(true);
    }
    public void ReColorStructure() {
        Transform[] children = Utilities.GetComponentsInDirectChildren<Transform>(structureParentGO);
        for (int i = 0; i < children.Length; i++) {
            GameObject structureToRecolor = children[i].gameObject;

            SpriteRenderer[] allColorizers = structureToRecolor.GetComponentsInChildren<SpriteRenderer>().
            Where(x => x.gameObject.tag == "StructureColorizers").ToArray();

            for (int j = 0; j < allColorizers.Length; j++) {
                allColorizers[j].color = this.ownedByCity.kingdom.kingdomColor;
            }
        }
    }
    public void RuinStructureOnTile(bool immediatelyDestroyStructures) {
        if (structureObjOnTile != null) {
            Debug.Log(GameManager.Instance.month + "/" + GameManager.Instance.days + "/" + GameManager.Instance.year + " - RUIN STRUCTURE ON: " + this.name);
            if (immediatelyDestroyStructures) {
                structureObjOnTile.DestroyStructure();
            } else {
                structureObjOnTile.SetStructureState(STRUCTURE_STATE.RUINED);
            }

        }
    }
    #endregion

    #region Fog of War Functions
    internal void SetFogOfWarState(FOG_OF_WAR_STATE fowState) {
        if (!KingdomManager.Instance.useFogOfWar) {
            fowState = FOG_OF_WAR_STATE.VISIBLE;
        }
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
                if ((isHabitable && isOccupied) || isLair || gameEventInTile != null) {
                    ShowNamePlate();
                }
                if (isOccupied) {
                    ShowStructures();
                }
                gameEventObjectsParentGO.SetActive(true);
                UIParent.gameObject.SetActive(true);
                ShowAllCitizensOnTile();
                break;
            case FOG_OF_WAR_STATE.SEEN:
                newColor.a = 128f / 255f;
                //if (isHabitable && isOccupied) {
                //    minimapColor = ownedByCity.kingdom.kingdomColor;
                //} else {
                //    minimapColor = biomeColor;
                //}
                if ((isHabitable && isOccupied) || isLair || gameEventInTile != null) {
                    ShowNamePlate();
                }
                if (isOccupied) {
                    ShowStructures();
                }
                gameEventObjectsParentGO.SetActive(false);
                UIParent.gameObject.SetActive(true);
                HideAllCitizensOnTile();
                break;
            case FOG_OF_WAR_STATE.HIDDEN:
                newColor.a = 255f / 255f;
                //minimapColor = Color.black;
                if ((isHabitable && isOccupied) || isLair || gameEventInTile != null) {
                    HideNamePlate();
                }
                if (isOccupied) {
                    HideStructures();
                }
                gameEventObjectsParentGO.SetActive(false);
                UIParent.gameObject.SetActive(false);
                HideAllCitizensOnTile();
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
    /*
     * Reset all values for this tile.
     * NOTE: This will set the structure to ruined.
     * To force destroy structure, call DestroyStructure
     * in StructureObject instead.
     * */
    public void ResetTile() {
        //this.city = null;
        this.isOccupied = false;
        this.isBorder = false;
        this.isPlagued = false;
        this.ownedByCity = null;
        //SetMinimapTileColor(biomeColor);
        this._kingdomColorSprite.color = Color.white;
        this.kingdomColorSprite.gameObject.SetActive(false);
        this._lairItem = null;
        this._hextileEventItem = null;
        Messenger.RemoveListener("UpdateUI", UpdateLairNamePlate);

        RuinStructureOnTile(false);
        RemoveCityNamePlate();
        Transform[] children = Utilities.GetComponentsInDirectChildren<Transform>(UIParent.gameObject);
        for (int i = 0; i < children.Length; i++) {
            ObjectPoolManager.Instance.DestroyObject(children[i].gameObject);
        }
    }
    public void Occupy(City city) {
        this.isOccupied = true;
        //if (!isVisibleByCities.Contains(city)) {
        //    this.isVisibleByCities.Add(city);
        //}
        //this.isOccupiedByCityID = city.id;		
        this.ownedByCity = city;
        if (!_visibleByKingdoms.Contains(city.kingdom)) {
            _visibleByKingdoms.Add(city.kingdom);
        }
        //this.isBorder = false;
        //this.isBorderOfCityID = 0;
    }
    public void Unoccupy(bool immediatelyDestroyStructures = false) {
        //if (!_isBorderOfCities.Select(x => x.kingdom).Contains(ownedByCity.kingdom)
        //    && !_isOuterTileOfCities.Select(x => x.kingdom).Contains(ownedByCity.kingdom)) {
        //    _visibleByKingdoms.Remove(ownedByCity.kingdom);
        //}
        isOccupied = false;
        ownedByCity = null;
        //SetMinimapTileColor(biomeColor);
        //this._kingdomColorSprite.color = Color.white;
        //this.kingdomColorSprite.gameObject.SetActive(false);
        RuinStructureOnTile(immediatelyDestroyStructures);
        city = null;

        //Destroy Nameplates
        RemoveCityNamePlate();
        Transform[] children = Utilities.GetComponentsInDirectChildren<Transform>(UIParent.gameObject);
        for (int i = 0; i < children.Length; i++) {
            ObjectPoolManager.Instance.DestroyObject(children[i].gameObject);
        }
    }
    public void Borderize(City city) {
        this.isBorder = true;
        //if (!_isBorderOfCities.Contains(city)) {
        //    _isBorderOfCities.Add(city);
        //}
        //if (!_visibleByKingdoms.Contains(city.kingdom)) {
        //    _visibleByKingdoms.Add(city.kingdom);
        //}
        //if (!isVisibleByCities.Contains(city)) {
        //    this.isVisibleByCities.Add(city);
        //}
        //this.isBorderOfCityID = city.id;
        //this.ownedByCity = city;
    }
    public void UnBorderize(City city) {
        this.isBorder = false;
        //this.isBorderOfCityID = 0;
        //this.ownedByCity = null;
        //_isBorderOfCities.Remove(city);
        //if (_isBorderOfCities.Count <= 0) {
        //    this.isBorder = false;
        //    this._kingdomColorSprite.color = Color.white;
        //    this.kingdomColorSprite.gameObject.SetActive(false);
        //}

        //if (!_isBorderOfCities.Select(x => x.kingdom).Contains(city.kingdom)
        //    && !_isOuterTileOfCities.Select(x => x.kingdom).Contains(city.kingdom)
        //    && (ownedByCity == null || ownedByCity.kingdom.id != city.kingdom.id)) {
        //    _visibleByKingdoms.Remove(city.kingdom);
        //}
        //this.isVisibleByCities.Remove(city);
    }
    public void ShowAllCitizensOnTile() {
        for (int i = 0; i < _citizensOnTile.Count; i++) {
            if (_citizensOnTile[i].assignedRole.avatar != null) {
                CitizenAvatar currCitizenAvatar = _citizensOnTile[i].assignedRole.avatar.GetComponent<CitizenAvatar>();
                currCitizenAvatar.SetAvatarState(true);
            }
        }
    }
    public void HideAllCitizensOnTile() {
        for (int i = 0; i < _citizensOnTile.Count; i++) {
            if (_citizensOnTile[i].assignedRole.avatar != null) {
                CitizenAvatar currCitizenAvatar = _citizensOnTile[i].assignedRole.avatar.GetComponent<CitizenAvatar>();
                currCitizenAvatar.SetAvatarState(false);
            }
        }
    }
    #endregion

    #region Monobehaviour Functions
    private void OnMouseDown() {
        if (UIManager.Instance.IsMouseOnUI() || currFogOfWarState != FOG_OF_WAR_STATE.VISIBLE) {
            return;
        }
        if (this.isHabitable && this.isOccupied && this.city != null && UIManager.Instance.spawnType == AGENT_TYPE.NONE) {
            CameraMove.Instance.CenterCameraOn(this.gameObject);
            if(UIManager.Instance.currentlyShowingKingdom != null && UIManager.Instance.currentlyShowingKingdom.id != this.city.kingdom.id) {
                UIManager.Instance.SetKingdomAsActive(this.city.kingdom);
				if(UIManager.Instance.notificationCityHistoryGO.activeSelf){
					UIManager.Instance.ShowCityHistory (this.city);
				}
            }
        }

        if(UIManager.Instance.spawnType != AGENT_TYPE.NONE && GameManager.Instance.enableGameAgents) {
            if(UIManager.Instance.spawnType == AGENT_TYPE.NECROMANCER) {
                //Spawn New Necromancer
                Necromancer newNecromancer = new Necromancer();
                AIBehaviour attackBehaviour = new AttackHostiles(newNecromancer);
                AIBehaviour fleeBehaviour = new RunAwayFromHostile(newNecromancer);
                AIBehaviour randomBehaviour = new SearchForCorpseMound(newNecromancer);
                newNecromancer.SetAttackBehaviour(attackBehaviour);
                newNecromancer.SetFleeBehaviour(fleeBehaviour);
                newNecromancer.SetRandomBehaviour(randomBehaviour);
                
                GameObject necromancerObj = ObjectPoolManager.Instance.InstantiateObjectFromPool("AgentGO", Vector3.zero, Quaternion.identity, this.transform);
                AgentObject agentObj = necromancerObj.GetComponent<AgentObject>();
                Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                pos.z = 0f;
                agentObj.aiPath.transform.position = pos;
                newNecromancer.SetAgentObj(agentObj);
                agentObj.Initialize(newNecromancer, new int[] {-1});
                UIManager.Instance.spawnType = AGENT_TYPE.NONE;
                UIManager.Instance._spawnNecromancerBtn.SetAsUnClicked();
            }
        }
    }
    private void OnMouseOver() {
        if (UIManager.Instance.IsMouseOnUI() || currFogOfWarState != FOG_OF_WAR_STATE.VISIBLE) {
            return;
        }
        if (this.isOccupied) {
			if(!this.isHabitable){
				if(this.city == null){
					return;
				}
			}
            //this.city.kingdom.HighlightAllOwnedTilesInKingdom();
            this.city.HighlightAllOwnedTiles(127f / 255f);
            this.ShowKingdomInfo();
        }
    }
    private void OnMouseExit() {
        if (this.isOccupied) {
			if(!this.isHabitable){
				if(this.city == null){
					return;
				}
			}
            this.HideKingdomInfo();
            this.city.HighlightAllOwnedTiles(69f / 255f);
            //if(this.ownedByCity != null) {
            //    this.ownedByCity.kingdom.UnHighlightAllOwnedTilesInKingdom();
            //}
            //if (UIManager.Instance.currentlyShowingKingdom != null) {
            //    UIManager.Instance.currentlyShowingKingdom.HighlightAllOwnedTilesInKingdom();
            //}
        }
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

    [ContextMenu("Show Border Tiles")]
    public void ShowBorderTiles() {
        for (int i = 0; i < this.city.borderTiles.Count; i++) {
            this.city.borderTiles[i].GetComponent<SpriteRenderer>().color = Color.magenta;
        }
    }

    //[ContextMenu("Show Adjacent Kingdoms")]
    //public void ShowAdjacentKingdoms() {
    //    for (int i = 0; i < this.city.kingdom.adjacentKingdoms.Count; i++) {
    //        Debug.Log("Adjacent Kingdom: " + this.city.kingdom.adjacentKingdoms[i].name);
    //    }
    //}

    [ContextMenu("Show Hextile Positions")]
    public void ShowHextileBounds() {
        Debug.Log("Local Pos: " + this.transform.localPosition.ToString());
        Debug.Log("Pos: " + this.transform.position.ToString());
    }

    [ContextMenu("Force Kill City")]
    public void ForceKillCity() {
        city.KillCity();
    }

    [ContextMenu("Force Reset Tile")]
    public void ForceResetTile() {
        ResetTile();
    }

    //[ContextMenu("Force Conquer Tile")]
    //public void ForceTileToBeConqueredByKingdom() {
    //    Kingdom conqueror = KingdomManager.Instance.allKingdoms[kingdomToConquerIndex];
    //    if (conqueror.id == this.city.kingdom.id) {
    //        Debug.LogWarning("City is already part of " + conqueror.name);
    //    } else {
    //        conqueror.ConquerCity(city, null);
    //    }
    //}

    [ContextMenu("Kill King")]
    public void KillKing() {
        Debug.Log("Force kill " + this.ownedByCity.kingdom.king.name + " king of " + this.ownedByCity.kingdom.name);
        this.ownedByCity.kingdom.king.Death(DEATH_REASONS.ACCIDENT);
    }

    //[ContextMenu("Kill Chancellor")]
    //public void KillChancellor() {
    //    this.ownedByCity.importantCitizensInCity[ROLE.GRAND_CHANCELLOR].Death(DEATH_REASONS.ACCIDENT);
    //}

    //[ContextMenu("Kill Marshal")]
    //public void KillMarshal() {
    //    this.ownedByCity.importantCitizensInCity[ROLE.GRAND_MARSHAL].Death(DEATH_REASONS.ACCIDENT);
    //}

    //[ContextMenu("Force Transfer City")]
    //public void ForceChangeKingdom() {
    //    KingdomManager.Instance.TransferCitiesToOtherKingdom(ownedByCity.kingdom, KingdomManager.Instance.allKingdoms[kingdomToConquerIndex], ownedByCity);
    //}

    [ContextMenu("Kill Kingdom Using Population")]
    public void KillKingdomUsingPopulation() {
        ownedByCity.kingdom.AdjustPopulation(-ownedByCity.kingdom.population);
    }
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

    [ContextMenu("Toggle Militarize")]
    public void ToggleMilitarize() {
        ownedByCity.kingdom.Militarize(!ownedByCity.kingdom.isMilitarize);
    }

    [ContextMenu("Toggle Fortify")]
    public void ToggleFortify() {
        ownedByCity.kingdom.Fortify(!ownedByCity.kingdom.isFortifying);
    }

    private void ShowKingdomInfo() {
        string text = this.city.name + " HP: " + this.city.hp.ToString() + "/" + this.city.maxHP.ToString() + "\n";
        text += "[b]" + this.city.kingdom.name + "[/b]" +
        "\n [b]Special Resource:[/b] " + this.city.region.specialResource.ToString();
        if(this.city.region.specialResource != RESOURCE.NONE) {
            text += "\n [b]Special Resource Loc:[/b] " + this.city.region.tileWithSpecialResource.name;
        }
        text += "\n [b]Power Points:[/b] " + this.city.powerPoints.ToString() +
        "\n [b]Defense Points:[/b] " + this.city.defensePoints.ToString() +
        "\n [b]Tech Points:[/b] " + this.city.techPoints.ToString() +
        "\n [b]City Weapons:[/b] " + this.city.weapons.ToString() +
        "\n [b]City Armor:[/b] " + this.city.armor.ToString() +
        "\n [b]Kingdom Base Weapons:[/b] " + this.city.kingdom.baseWeapons.ToString() +
        "\n [b]Kingdom Base Armor:[/b] " + this.city.kingdom.baseArmor.ToString() +
        "\n [b]City Level Cap:[/b] " + this.region.cityLevelCap.ToString() +
        "\n [b]Region Population Growth:[/b] " + this.region.populationGrowth.ToString() +
        "\n [b]Kingdom Type:[/b] " + this.city.kingdom.kingdomType.ToString() +
        "\n [b]Kingdom Size:[/b] " + this.city.kingdom.kingdomSize.ToString() +
        "\n [b]Expansion Rate:[/b] " + this.city.kingdom.GetMonthlyExpansionRateIncrease().ToString() +
        "\n [b]Growth Rate: [/b]" + this.city.totalDailyGrowth.ToString() +
        "\n [b]Draft Rate: [/b]" + (this.city.kingdom.draftRate * 100f).ToString() + "%" +
        "\n [b]Research Rate: [/b]" + (this.city.kingdom.researchRate * 100f).ToString() + "%" +
        "\n [b]Production Rate: [/b]" + (this.city.kingdom.productionRate * 100f).ToString() + "%" +
        "\n [b]Current Growth: [/b]" + this.city.currentGrowth.ToString() + "/" + this.city.maxGrowth.ToString() + "\n";

        text += "[b]Adjacent Kingdoms: [/b]\n";
        if (this.city.kingdom.adjacentKingdoms.Count > 0) {
            for (int i = 0; i < this.city.kingdom.adjacentKingdoms.Count; i++) {
                text += this.city.kingdom.adjacentKingdoms[i].name + "\n";
            }
        } else {
            text += "NONE\n";
        }

        text += "[b]Discovered Kingdoms: [/b]\n";
        if (this.city.kingdom.discoveredKingdoms.Count > 0) {
            for (int i = 0; i < this.city.kingdom.discoveredKingdoms.Count; i++) {
                Kingdom currKingdom = this.city.kingdom.discoveredKingdoms[i];
                text += currKingdom.name + "\n";
            }
        } else {
            text += "NONE\n";
        }

        //text += "[b]Alliance Kingdoms: [/b]\n";
        //if(this.city.kingdom.alliancePool != null) {
        //    for (int i = 0; i < this.city.kingdom.alliancePool.kingdomsInvolved.Count; i++) {
        //        Kingdom currKingdom = this.city.kingdom.alliancePool.kingdomsInvolved[i];
        //        text += currKingdom.name + "\n";
        //    }
        //} else {
        //    text += "NONE\n";
        //}

        //text += "[b]Important Citizens in City: [/b]\n";
        //if (this.city.importantCitizensInCity.Count > 0) {
        //    for (int i = 0; i < this.city.importantCitizensInCity.Count; i++) {
        //        text += this.city.importantCitizensInCity.Values.ElementAt(i).name + " - " + this.city.importantCitizensInCity.Keys.ElementAt(i).ToString() + "\n";
        //    }
        //} else {
        //    text += "NONE\n";
        //}

        text += "[b]Citizens in City: [/b]\n";
        if (this.city.citizens.Count > 0) {
            for (int i = 0; i < this.city.citizens.Count; i++) {
                Citizen currCitizen = this.city.citizens[i];
                if (currCitizen.role != ROLE.UNTRAINED) {
                    text += currCitizen.role + " - " + currCitizen.name + "\n";
                }
            }
        } else {
            text += "NONE\n";
        }

        //text += "[b]Kingdom values: [/b]\n";
        //Dictionary<CHARACTER_VALUE, int> kingdomVals = this.city.kingdom.importantCharacterValues;
        //if (kingdomVals.Count > 0) {
        //    for (int i = 0; i < kingdomVals.Count(); i++) {
        //        KeyValuePair<CHARACTER_VALUE, int> kvp = kingdomVals.ElementAt(i);
        //        text += kvp.Key.ToString() + " - " + kvp.Value.ToString() + "\n";
        //    }
        //} else {
        //    text += "NONE\n";
        //}
        UIManager.Instance.ShowSmallInfo(text);
    }

    private void HideKingdomInfo() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

	internal float GetDistanceTo(HexTile targetHextile){
		return Vector3.Distance (this.transform.position, targetHextile.transform.position);
	}

	internal void SetPlague(bool state){
		this.isPlagued = state;
		this.SetActivePlagueIcon(state);
        this.ownedByCity.UpdateDailyProduction();
	}

	private void SetActivePlagueIcon(bool state){
		if(state){
			if(this.plagueIcon == null){
				this.plagueIcon = UIManager.Instance.InstantiateUIObject (this.plagueIconGO.name, this.UIParent);
				this.plagueIcon.transform.localPosition = Vector3.zero;
			}
		}else{
			if(this.plagueIcon != null){
				ObjectPoolManager.Instance.DestroyObject(this.plagueIcon);
			}
		}
	}
		
	internal void RemoveEventOnTile(){
		this._gameEventInTile = null;
        RemoveHextileEventNamePlate();
        //      Transform[] children = Utilities.GetComponentsInDirectChildren<Transform>(UIParent.gameObject);
        //for (int i = 0; i < children.Length; i++) {
        //	if(children[i].gameObject.tag == "EventTileNameplate"){
        //Destroy(children[i].gameObject);
        //	}
        //}
    }
	internal GameEvent GetEventFromTile(){
		return this._gameEventInTile;
	}
	internal void SetKeystone(bool state){
		this.hasKeystone = state;
		this.SetActiveKeystoneIcon(state);
        this.RemoveEventOnTile();
    }

	private void SetActiveKeystoneIcon(bool state){
		if(state){
			if(this.plagueIcon == null){
				this.plagueIcon = UIManager.Instance.InstantiateUIObject (this.plagueIconGO.name, this.UIParent);
				this.plagueIcon.transform.localPosition = Vector3.zero;
			}
		}else{
			if(this.plagueIcon != null){
				ObjectPoolManager.Instance.DestroyObject(this.plagueIcon);
			}
		}
	}

	internal void SetFirst(bool state){
		this.hasFirst = state;
		this.SetActiveFirstIcon(state);
	}

	private void SetActiveFirstIcon(bool state){
		if(state){
			if(this.plagueIcon == null){
				this.plagueIcon = UIManager.Instance.InstantiateUIObject (this.plagueIconGO.name, this.UIParent);
				this.plagueIcon.transform.localPosition = Vector3.zero;
			}
		}else{
			if(this.plagueIcon != null){
				ObjectPoolManager.Instance.DestroyObject(this.plagueIcon);
			}
		}
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

    internal void EnterCitizen(Citizen citizen) {
        this._citizensOnTile.Add(citizen);
    }
    internal void ExitCitizen(Citizen citizen) {
        this._citizensOnTile.Remove(citizen);
    }

	internal void SetCorpseMound(CorpseMound corpseMound){
		this._corpseMound = corpseMound;
		if(corpseMound != null){
			this.region.AddCorpseMoundTile (this);
		}else{
			if(this.region.corpseMoundTiles.Count > 0){
				this.region.RemoveCorpseMoundTile (this);
			}
		}
	}
	internal bool IsAdjacentWithRegion(Region region){
		List<HexTile> neighbors = this.AllNeighbours;
		for (int i = 0; i < neighbors.Count; i++) {
			if(neighbors[i].region.id == region.id){
				return true;
			}
		}
		return false;
	}

	internal void DestroyConnections(){
		while(this.connectedTiles.Count > 0){
			RoadManager.Instance.DestroyConnection (this, this.connectedTiles.Keys.ElementAt(0));
		}
	}

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
}
