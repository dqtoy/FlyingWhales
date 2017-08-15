using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathFind;
using System.Linq;
using Panda;

public class HexTile : MonoBehaviour,  IHasNeighbours<HexTile>{
    [Header("General Tile Details")]
    public int xCoordinate;
	public int yCoordinate;
    public int tag;
	public string tileName;

    [Space(10)]
    [Header("Biome Settings")]
    public float elevationNoise;
	public float moistureNoise;
	public float temperature;
	public BIOMES biomeType;
	public ELEVATION elevationType;
	public int movementDays;

    [Space(10)]
    [Header("Resources")]
    public RESOURCE specialResource;
    public int nearbyResourcesCount = 0;

    [System.NonSerialized] public City city = null;
	internal City ownedByCity = null; // this is populated whenever the hex tile is occupied or becomes a border of a particular city

	public Lair lair;

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

    [SerializeField] private List<City> _isBorderOfCities = new List<City>();
    [SerializeField] private List<City> _isOuterTileOfCities = new List<City>();
    [SerializeField] private List<Kingdom> _seenByKingdoms = new List<Kingdom>(); //This is only occupied when a tile becomes occupies, becaoms a border or becomes an outer tile of a city!
	//public int isBorderOfCityID = 0;
	//internal int isOccupiedByCityID = 0;
    //[SerializeField] internal List<City> isVisibleByCities = new List<City>();

    [Space(10)]
    [Header("Tile Visuals")]
    [SerializeField] private GameObject _centerPiece;
    //[SerializeField] private GameObject resourceVisualGO;
    [SerializeField] private ResourceIcon resourceIcon;
	[SerializeField] private SpriteRenderer _kingdomColorSprite;
	[SerializeField] private GameObject _highlightGO;
    [SerializeField] private Transform UIParent;
    [SerializeField] private Transform resourceParent;
    [SerializeField] private GameObject biomeDetailParentGO;
    [SerializeField] private TextMesh tagVisual;

    [Space(10)]
    [Header("Tile Edges")]
    [SerializeField] private GameObject topLeftEdge;
	[SerializeField] private GameObject leftEdge;
	[SerializeField] private GameObject botLeftEdge;
	[SerializeField] private GameObject botRightEdge;
	[SerializeField] private GameObject rightEdge;
	[SerializeField] private GameObject topRightEdge;

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

    private List<Citizen> _citizensOnTile = new List<Citizen>();

    [System.NonSerialized] public List<HexTile> connectedTiles = new List<HexTile>();

	public IEnumerable<HexTile> AllNeighbours { get; set; }
	public IEnumerable<HexTile> ValidTiles { get { return AllNeighbours.Where(o => o.elevationType != ELEVATION.WATER && o.elevationType != ELEVATION.MOUNTAIN);}}
	public IEnumerable<HexTile> RoadTiles { get { return AllNeighbours.Where(o => o.isRoad); } }
	public IEnumerable<HexTile> PurchasableTiles { get { return AllNeighbours.Where (o => o.elevationType != ELEVATION.WATER);}}
	public IEnumerable<HexTile> CombatTiles { get { return AllNeighbours.Where (o => o.elevationType != ELEVATION.WATER);}}
    public IEnumerable<HexTile> AvatarTiles { get { return AllNeighbours.Where(o => o.elevationType != ELEVATION.WATER);}}

    public List<HexTile> elligibleNeighbourTilesForPurchase { get { return PurchasableTiles.Where(o => !o.isOccupied && !o.isHabitable).ToList(); } } 

	//private List<WorldEventItem> eventsOnTile = new List<WorldEventItem>();

	#region getters/setters
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
    public List<Kingdom> seenByKingdoms {
        get { return _seenByKingdoms; }
    }
    #endregion

    internal void SetBiome(BIOMES biome) {
        biomeType = biome;
        if(elevationType == ELEVATION.WATER) {
            SetMinimapTileColor(new Color(64f/255f, 164f/255f, 223f/255f));
        } else {
            SetMinimapTileColor(Utilities.biomeColor[biome]);
        }
        biomeColor = minimapHexSprite.color;
        
    }

	internal void SetSortingOrder(int sortingOrder){
		GetComponent<SpriteRenderer> ().sortingOrder = sortingOrder;
		if (elevationType == ELEVATION.MOUNTAIN) {
			centerPiece.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 56;
		} else {
			centerPiece.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 52;
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

	#region Resource
//	internal void AssignDefaultResource(){
//		if(elevationType == ELEVATION.MOUNTAIN){
//			this.defaultResource = RESOURCE.GRANITE;
//		}else{
//			if (this.elevationType != ELEVATION.WATER) {
//				switch (biomeType) {
//				case BIOMES.BARE:
//					this.defaultResource = RESOURCE.NONE;
//					break;
//				case BIOMES.DESERT:
//					this.defaultResource = RESOURCE.GRANITE;
//					break;
//				case BIOMES.FOREST:
//					this.defaultResource = RESOURCE.OAK;
//					break;
//				case BIOMES.GRASSLAND:
//					this.defaultResource = RESOURCE.CORN;
//					break;
//				case BIOMES.SNOW:
//					this.defaultResource = RESOURCE.NONE;
//					break;
//				case BIOMES.TUNDRA:
//					this.defaultResource = RESOURCE.CORN;
//					break;
//				case BIOMES.WOODLAND:
//					this.defaultResource = RESOURCE.CEDAR;
//					break;
//
//				}
//			}
//		}
//	}

	internal void AssignSpecialResource(){
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
			if (this.elevationType != ELEVATION.WATER && this.elevationType != ELEVATION.MOUNTAIN) {
				this.specialResource = ComputeSpecialResource (Utilities.specialResourcesLookup [this.biomeType]);
				if (this.specialResource != RESOURCE.NONE) {
                    resourceIcon.SetResource(specialResource);
                    GameObject resource = GameObject.Instantiate(Biomes.Instance.GetPrefabForResource(this.specialResource), resourceParent) as GameObject;
                    resource.transform.localPosition = Vector3.zero;
                    resource.transform.localScale = Vector3.one;
                    if (this.biomeType == BIOMES.FOREST && Utilities.GetBaseResourceType(this.specialResource) == BASE_RESOURCE_TYPE.WOOD && this.elevationType == ELEVATION.PLAIN) {
                        centerPiece.SetActive(false);
                    }
                }
            }
		} else {
			this.specialResource = RESOURCE.NONE;
		}
    }

	public PandaBehaviour GetBehaviourTree(){
		return this.GetComponent<PandaBehaviour>();
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
		
	/*
	 * Returns all Hex tiles gameobjects within a radius
	 * */
	public List<HexTile> GetTilesInRange(int range){
		List<HexTile> tilesInRange = new List<HexTile>();
		List<HexTile> checkedTiles = new List<HexTile> ();

		for (int i = 0; i < range; i++) {
			if (tilesInRange.Count <= 0) {
				tilesInRange.AddRange (this.AllNeighbours);
				checkedTiles.Add (this);
			}else{
				List<HexTile> tilesToAdd = new List<HexTile> ();
				for (int j = 0; j < tilesInRange.Count; j++) {
					if (!checkedTiles.Contains (tilesInRange [j])) {
						checkedTiles.Add (tilesInRange [j]);
						tilesToAdd.AddRange (tilesInRange[j].AllNeighbours);
					}
				}
				tilesInRange.AddRange (tilesToAdd);
			}
		}
		return tilesInRange.Distinct().ToList();
	}

	#region Pathfinding
	public void FindNeighbours(HexTile[,] gameBoard) {
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
				neighbours.Add (gameBoard [neighbourCoordinateX, neighbourCoordinateY]);
			}

		}
		this.AllNeighbours = neighbours;
	}
    #endregion

    #region Tile Visuals
    internal void LoadEdges() {
        int biomeLayerOfHexTile = Utilities.biomeLayering.IndexOf(this.biomeType);
        List<HexTile> neighbours = this.AllNeighbours.ToList();
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
                        gameObjectToEdit.SetActive(true);
                        gameObjectToEdit.GetComponent<SpriteRenderer>().sprite = Biomes.Instance.GetTextureForBiome(currentNeighbour.biomeType);
                        gameObjectToEdit.GetComponent<SpriteRenderer>().sortingOrder += biomeLayerOfNeighbour;
                        Material mat = new Material(Shader.Find("AlphaMask"));
                        mat.SetTexture("_Alpha", spriteMasksToChooseFrom[Random.Range(0, spriteMasksToChooseFrom.Length)]);
                        gameObjectToEdit.GetComponent<SpriteRenderer>().material = mat;
                        //gameObjectToEdit.GetComponent<SpriteRenderer>().material.SetTexture("Alpha (A)", (Texture)spriteMasksToChooseFrom[Random.Range(0, spriteMasksToChooseFrom.Length)]);
                        //					gameObjectToEdit.GetComponent<SpriteRenderer> ().material = materialForTile;
                    }

                }
            }

            
        }
    }

    public void SetBaseSprite(Sprite baseSprite){
		this.GetComponent<SpriteRenderer>().sprite = baseSprite;
	}

	public void SetCenterSprite(Sprite centerSprite){
		this.centerPiece.GetComponent<SpriteRenderer>().sprite = centerSprite;
		this.centerPiece.SetActive(true);
	}

	public void SetTileHighlightColor(Color color){
        color.a = 76.5f / 255f;
        this._kingdomColorSprite.color = color;
	}

    internal void SetMinimapTileColor(Color color) {
        minimapHexSprite.color = color;
    }

	public void ShowTileHighlight(){
		this._kingdomColorSprite.gameObject.SetActive(true);
	}

	public void HideTileHighlight(){
		this.kingdomColorSprite.gameObject.SetActive(false);
	}

    public void CreateStructureOnTile(STRUCTURE_TYPE structureType, STRUCTURE_STATE structureState = STRUCTURE_STATE.NORMAL) {
        //Debug.Log("Create " + structureType.ToString() + " on " + this.name);
        GameObject[] gameObjectsToChooseFrom = CityGenerator.Instance.GetStructurePrefabsForRace(this.ownedByCity.kingdom.race, structureType);
        GameObject structureGO = GameObject.Instantiate(
        gameObjectsToChooseFrom[Random.Range(0, gameObjectsToChooseFrom.Length)],
        structureParentGO.transform) as GameObject;
        AssignStructureObjectToTile(structureGO.GetComponent<StructureObject>());
        structureObjOnTile.Initialize(structureType, this.ownedByCity.kingdom.kingdomColor, structureState);

        this._centerPiece.SetActive(false);

        Color color = this.ownedByCity.kingdom.kingdomColor;
        SetMinimapTileColor(color);
        SetTileHighlightColor(color);
    }

    /*
     * Assign a structure object to this tile.
     * NOTE: This will destroy any current structures on this tile
     * and replace it with the new assigned one.
     * */
    public void AssignStructureObjectToTile(StructureObject structureObj) {
        if(structureObjOnTile != null) {
            //Destroy Current Structure
            structureObjOnTile.DestroyStructure();
        }
        structureObjOnTile = structureObj;
        structureObj.transform.SetParent(this.structureParentGO.transform);
        structureObj.transform.localPosition = Vector3.zero;
    }

    //public void ShowCitySprite() {
    //    GameObject[] gameObjectsToChooseFrom = CityGenerator.Instance.GetStructurePrefabsForRace(this.city.kingdom.race, STRUCTURE_TYPE.CITY);
    //    GameObject structureGO = GameObject.Instantiate(
    //       gameObjectsToChooseFrom[Random.Range(0, gameObjectsToChooseFrom.Length)],
    //       structureParentGO.transform) as GameObject;
    //    structureGO.transform.localPosition = Vector3.zero;
    //    SpriteRenderer[] allColorizers = structureGO.GetComponentsInChildren<SpriteRenderer>().
    //        Where(x => x.gameObject.tag == "StructureColorizers").ToArray();

    //    for (int i = 0; i < allColorizers.Length; i++) {
    //        allColorizers[i].color = this.ownedByCity.kingdom.kingdomColor;
    //    }
    //    this._centerPiece.SetActive(false);

    //    //this.structureGO.GetComponent<SpriteRenderer>().sprite = CityGenerator.Instance.elfCitySprite;
    //    //this.structureGO.SetActive(true);
    //    //this.centerPiece.SetActive(false);
    //    Color color = this.city.kingdom.kingdomColor;
    //    color.a = 255f / 255f;
    //    SetMinimapTileColor(color);
    //    color.a = 76.5f / 255f;
    //    this._kingdomColorSprite.color = color;
    //    this.GetComponent<SpriteRenderer>().color = Color.white;
    //    //this.GetComponent<SpriteRenderer>().sprite = Biomes.Instance.bareTiles[Random.Range(0, Biomes.Instance.bareTiles.Length)];
    //}

    public GameObject CreateSpecialStructureOnTile(LAIR lairType) {
        GameObject structureGO = GameObject.Instantiate(
            CityGenerator.Instance.GetStructurePrefabForSpecialStructures(lairType), structureParentGO.transform) as GameObject;
        structureGO.transform.localPosition = Vector3.zero;
        return structureGO;
    }

    public void ShowNamePlate() {
        _namePlateParent.gameObject.SetActive(true);
        if(_cityInfo != null) {
            UpdateCityNamePlate();
        }
        if(_lairItem != null) {
            UpdateLairNamePlate();
        }
    }
    public void HideNamePlate() {
        _namePlateParent.gameObject.SetActive(false);
    }

    /*
     * This will instantiate a new CityItem Prefab and set it's city 
     * according to the passed parameter.
     * */
    public void CreateCityNamePlate(City city) {
        Debug.Log("Create nameplate for: " + city.name + " on " + this.name);

        GameObject namePlateGO = UIManager.Instance.InstantiateUIObject("CityNamePlatePanel", UIParent);
        namePlateGO.layer = LayerMask.NameToLayer("HextileNamePlates");
        _namePlateParent = namePlateGO.transform;
        _cityInfo = namePlateGO.GetComponentInChildren<CityItem>();
        namePlateGO.transform.localPosition = new Vector3(-2.22f, -1.02f, 0f);
        Messenger.AddListener("UpdateUI", UpdateCityNamePlate);

        UpdateCityNamePlate();
    }
    public void UpdateCityNamePlate() {
        //_cityInfo.SetCity(city);
        if (KingdomManager.Instance.useFogOfWar) {
            if (_currFogOfWarState == FOG_OF_WAR_STATE.VISIBLE) {
                _cityInfo.SetCity(city);
            } else {
                _cityInfo.SetCity(city, false, true);
            }
        } else {
            _cityInfo.SetCity(city);
        }
    }
    public void RemoveCityNamePlate() {
        ObjectPoolManager.Instance.DestroyObject(_namePlateParent.gameObject);
        _namePlateParent = null;
        _cityInfo = null;
        Messenger.RemoveListener("UpdateUI", UpdateCityNamePlate);
    }

	public void CreateLairNamePlate() {
        Debug.Log("Create lair nameplate on " + this.name);

        GameObject namePlateGO = UIManager.Instance.InstantiateUIObject("LairNamePlatePanel", UIParent);
        namePlateGO.layer = LayerMask.NameToLayer("HextileNamePlates");
        _namePlateParent = namePlateGO.transform;
        _lairItem = namePlateGO.GetComponentInChildren<LairItem>();
        namePlateGO.transform.localPosition = new Vector3(-2.22f, -1.02f, 0f);
        Messenger.AddListener("UpdateUI", UpdateLairNamePlate);

        UpdateLairNamePlate();
	}
	public void UpdateLairNamePlate() {
		this._lairItem.SetLair(this.lair);
	}
    public void RemoveLairNamePlate() {
        ObjectPoolManager.Instance.DestroyObject(_namePlateParent.gameObject);
        _namePlateParent = null;
        _lairItem = null;
        Messenger.RemoveListener("UpdateUI", UpdateLairNamePlate);
    }

	public void CreateEventNamePlate() {
        Debug.Log("Create " + gameEventInTile.eventType.ToString() + " nameplate on " + this.name);

        GameObject namePlateGO = UIManager.Instance.InstantiateUIObject("EventNamePlatePanel", UIParent);
        namePlateGO.layer = LayerMask.NameToLayer("HextileNamePlates");
        _namePlateParent = namePlateGO.transform;
        _hextileEventItem = namePlateGO.GetComponentInChildren<HextileEventItem>();
        namePlateGO.transform.localPosition = new Vector3(-2.22f, -1.02f, 0f);
        Messenger.AddListener("UpdateUI", UpdateHextileEventNamePlate);

        UpdateHextileEventNamePlate();
	}
	public void UpdateHextileEventNamePlate() {
		_hextileEventItem.SetHextileEvent(_gameEventInTile);
	}
    public void RemoveHextileEventNamePlate() {
        ObjectPoolManager.Instance.DestroyObject(_namePlateParent.gameObject);
        _namePlateParent = null;
        _hextileEventItem = null;
        Messenger.RemoveListener("UpdateUI", UpdateHextileEventNamePlate);
    }

   // public void ShowOccupiedSprite() {
   //     //this.GetComponent<SpriteRenderer>().sprite = Biomes.Instance.bareTiles[Random.Range(0, Biomes.Instance.bareTiles.Length)];
   //     GameObject[] structuresToChooseFrom = CityGenerator.Instance.GetStructurePrefabsForRace(this.ownedByCity.kingdom.race, STRUCTURE_TYPE.GENERIC);
   //     if (this.specialResource != RESOURCE.NONE) {
   //         if(Utilities.GetBaseResourceType(this.specialResource) == BASE_RESOURCE_TYPE.FOOD) {
   //             if(this.specialResource == RESOURCE.BEHEMOTH || this.specialResource == RESOURCE.DEER || 
   //                 this.specialResource == RESOURCE.PIG) {
   //                 structuresToChooseFrom = CityGenerator.Instance.GetStructurePrefabsForRace(this.ownedByCity.kingdom.race, STRUCTURE_TYPE.HUNTING_LODGE);
   //             } else {
   //                 structuresToChooseFrom = CityGenerator.Instance.GetStructurePrefabsForRace(this.ownedByCity.kingdom.race, STRUCTURE_TYPE.MINES);
   //             }
   //} else if(Utilities.GetBaseResourceType(this.specialResource) == BASE_RESOURCE_TYPE.WOOD && this.ownedByCity.kingdom.race == RACE.ELVES) {
   //             structuresToChooseFrom = CityGenerator.Instance.GetStructurePrefabsForRace(this.ownedByCity.kingdom.race, STRUCTURE_TYPE.LUMBERYARD);
   //} else if (Utilities.GetBaseResourceType(this.specialResource) == BASE_RESOURCE_TYPE.STONE && this.ownedByCity.kingdom.race == RACE.HUMANS) {
   //             structuresToChooseFrom = CityGenerator.Instance.GetStructurePrefabsForRace(this.ownedByCity.kingdom.race, STRUCTURE_TYPE.QUARRY);
   //         } else {
   //             structuresToChooseFrom = CityGenerator.Instance.GetStructurePrefabsForRace(this.ownedByCity.kingdom.race, STRUCTURE_TYPE.MINES);
   //         }
   //     }

    //     GameObject structureGO = GameObject.Instantiate(
    //         structuresToChooseFrom[Random.Range(0, structuresToChooseFrom.Length)],
    //         structureParentGO.transform) as GameObject;
    //     structureGO.transform.localPosition = Vector3.zero;
    //     SpriteRenderer[] allColorizers = structureGO.GetComponentsInChildren<SpriteRenderer>().
    //         Where(x => x.gameObject.tag == "StructureColorizers").ToArray();

    //     Color color = ownedByCity.kingdom.kingdomColor;
    //     color.a = 255f / 255f;
    //     SetMinimapTileColor(color);

    //     for (int i = 0; i < allColorizers.Length; i++) {
    //         allColorizers[i].color = this.ownedByCity.kingdom.kingdomColor;
    //     }
    //     this._centerPiece.SetActive(false);
    // }

    public void HideStructures() {
        structureParentGO.SetActive(false);
    }

    public void ShowStructures() {
        structureParentGO.SetActive(true);
    }

    public void SetFogOfWarState(FOG_OF_WAR_STATE fowState) {
        //if(fowState == _currFogOfWarState) {
        //    return;
        //}
        _currFogOfWarState = fowState;
        if (KingdomManager.Instance.useFogOfWar) {
            Color newColor = FOWSprite.color;
            switch (fowState) {
                case FOG_OF_WAR_STATE.VISIBLE:
                    newColor.a = 0f / 255f;
                    if ((isHabitable && isOccupied) || isLair || gameEventInTile != null) {
                        ShowNamePlate();
					}
                    if (isOccupied) {
                        ShowStructures();
                    }
                    gameEventObjectsParentGO.SetActive(true);
                    UIParent.gameObject.SetActive(true);
                    break;
                case FOG_OF_WAR_STATE.SEEN:
                    newColor.a = 128f / 255f;
                    if ((isHabitable && isOccupied) || isLair || gameEventInTile != null) {
                        ShowNamePlate();
					}
                    if (isOccupied) {
                        ShowStructures();
                    }
                    gameEventObjectsParentGO.SetActive(false);
                    UIParent.gameObject.SetActive(true);
                    break;
                case FOG_OF_WAR_STATE.HIDDEN:
                    newColor.a = 255f / 255f;
                    if ((isHabitable && isOccupied) || isLair || gameEventInTile != null) {
                        HideNamePlate();
					}
                    if (isOccupied) {
                        HideStructures();
                    }
                    gameEventObjectsParentGO.SetActive(false);
                    UIParent.gameObject.SetActive(false);
                    break;
                default:
                    break;
            }
            FOWSprite.color = newColor;
            minimapFOWSprite.color = newColor;
        }
    }

    public void HideFogOfWarObjects() {
        FOWSprite.gameObject.SetActive(false);
        minimapFOWSprite.gameObject.SetActive(false);
    }
    public void ShowFogOfWarObjects() {
        FOWSprite.gameObject.SetActive(true);
        minimapFOWSprite.gameObject.SetActive(true);
    }

    public void AddBiomeDetailToTile(GameObject detailPrefab) {
        GameObject detailGO = GameObject.Instantiate(detailPrefab, biomeDetailParentGO.transform) as GameObject;
        detailGO.transform.localScale = Vector3.one;
        detailGO.transform.localPosition = Vector3.zero;
    }

    public void SetBiomeDetailState(bool state) {
        biomeDetailParentGO.SetActive(state);
    }
    #endregion

    /*
     * Reset all values for this tile.
     * NOTE: This will set the structure to ruined.
     * To force destroy structure, call DestroyStructure
     * in StructureObject instead.
     * */
    public void ResetTile(){
        //this.city = null;
        this.isOccupied = false;
		this.isBorder = false;
        this.isPlagued = false;
		this.ownedByCity = null;
        SetMinimapTileColor(biomeColor);
        this._kingdomColorSprite.color = Color.white;
		this.kingdomColorSprite.gameObject.SetActive(false);
		this._lairItem = null;
		this._hextileEventItem = null;
		Messenger.RemoveListener("UpdateUI", UpdateLairNamePlate);

        RuinStructureOnTile();
        RemoveCityNamePlate();
        Transform[] children = Utilities.GetComponentsInDirectChildren<Transform>(UIParent.gameObject);
        for (int i = 0; i < children.Length; i++) {
            Destroy(children[i].gameObject);
        }
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

	public void Occupy(City city) {
		this.isOccupied = true;
        //if (!isVisibleByCities.Contains(city)) {
        //    this.isVisibleByCities.Add(city);
        //}
		//this.isOccupiedByCityID = city.id;		
		this.ownedByCity = city;
        if (!_seenByKingdoms.Contains(city.kingdom)) {
            _seenByKingdoms.Add(city.kingdom);
        }
        //this.isBorder = false;
        //this.isBorderOfCityID = 0;
    }

    public void Unoccupy(bool immediatelyDestroyStructures = false) {
        if (!_isBorderOfCities.Select(x => x.kingdom).Contains(ownedByCity.kingdom)
            && !_isOuterTileOfCities.Select(x => x.kingdom).Contains(ownedByCity.kingdom)) {
            _seenByKingdoms.Remove(ownedByCity.kingdom);
        }
        isOccupied = false;
        ownedByCity = null;
        SetMinimapTileColor(biomeColor);
        this._kingdomColorSprite.color = Color.white;
        this.kingdomColorSprite.gameObject.SetActive(false);
        RuinStructureOnTile(immediatelyDestroyStructures);
        city = null;

        //Destroy Nameplates
        RemoveCityNamePlate();
        Transform[] children = Utilities.GetComponentsInDirectChildren<Transform>(UIParent.gameObject);
        for (int i = 0; i < children.Length; i++) {
            Destroy(children[i].gameObject);
        }
    }

    public void RuinStructureOnTile(bool immediatelyDestroyStructures = false) {
        if (structureObjOnTile != null) {
            Debug.Log(GameManager.Instance.month + "/" + GameManager.Instance.days + "/" + GameManager.Instance.year + " - RUIN STRUCTURE ON: " + this.name);
            if (immediatelyDestroyStructures) {
                structureObjOnTile.DestroyStructure();
            } else {
                structureObjOnTile.SetStructureState(STRUCTURE_STATE.RUINED);
            }
            
        }
    }

	public void Borderize(City city) {
		this.isBorder = true;
        if (!_isBorderOfCities.Contains(city)) {
            _isBorderOfCities.Add(city);
        }
        if (!_seenByKingdoms.Contains(city.kingdom)) {
            _seenByKingdoms.Add(city.kingdom);
        }
        //if (!isVisibleByCities.Contains(city)) {
        //    this.isVisibleByCities.Add(city);
        //}
        //this.isBorderOfCityID = city.id;
        //this.ownedByCity = city;
    }

    public void UnBorderize(City city) {
        //this.isBorderOfCityID = 0;
        //this.ownedByCity = null;
        _isBorderOfCities.Remove(city);
        if (_isBorderOfCities.Count <= 0) {
            this.isBorder = false;
            this._kingdomColorSprite.color = Color.white;
            this.kingdomColorSprite.gameObject.SetActive(false);
        }

        if (!_isBorderOfCities.Select(x => x.kingdom).Contains(city.kingdom)
            && !_isOuterTileOfCities.Select(x => x.kingdom).Contains(city.kingdom)) {
            _seenByKingdoms.Remove(city.kingdom);
        }
        //this.isVisibleByCities.Remove(city);
    }

    public void SetAsOuterTileOf(City city) {
        if (!_isOuterTileOfCities.Contains(city)) {
            _isOuterTileOfCities.Add(city);
        }
        if (!_seenByKingdoms.Contains(city.kingdom)) {
            _seenByKingdoms.Add(city.kingdom);
        }
    }

    public void RemoveAsOuterTileOf(City city) {
        _isOuterTileOfCities.Remove(city);
        if (!_isBorderOfCities.Select(x => x.kingdom).Contains(city.kingdom) 
            && !_isOuterTileOfCities.Select(x => x.kingdom).Contains(city.kingdom)) {
            _seenByKingdoms.Remove(city.kingdom);
        }
    }

    #region Monobehaviour Functions
    void OnMouseDown() {
        if (UIManager.Instance.IsMouseOnUI() || (KingdomManager.Instance.useFogOfWar && currFogOfWarState != FOG_OF_WAR_STATE.VISIBLE)) {
            return;
        }
        if (this.isHabitable && this.isOccupied && this.city != null) {
            CameraMove.Instance.CenterCameraOn(this.gameObject);
            //UIManager.Instance.SetKingdomAsSelected(this.city.kingdom);
		}
		InterveneEventOnTile (WorldEventManager.Instance.currentInterveneEvent);
    }

    void OnMouseOver() {
        if (UIManager.Instance.IsMouseOnUI() || (KingdomManager.Instance.useFogOfWar && currFogOfWarState != FOG_OF_WAR_STATE.VISIBLE)) {
            return;
        }
        if (this.isOccupied) {
			if(!this.isHabitable){
				if(this.city == null){
					return;
				}else{
					if(this.city.rebellion == null){
						return;
					}
				}
			}
            this.city.kingdom.HighlightAllOwnedTilesInKingdom();
            this.city.HighlightAllOwnedTiles(204f / 255f);
            this.ShowKingdomInfo();
        }
    }

    void OnMouseExit() {
        if (this.isOccupied) {
			if(!this.isHabitable){
				if(this.city == null){
					return;
				}else{
					if(this.city.rebellion == null){
						return;
					}
				}
			}
            this.HideKingdomInfo();
            if(this.ownedByCity != null) {
                this.ownedByCity.UnHighlightAllOwnedTiles();
            }
            if (UIManager.Instance.currentlyShowingKingdom != null) {
                UIManager.Instance.currentlyShowingKingdom.HighlightAllOwnedTilesInKingdom();
                ////if there is currently showing kingdom, if this city is part of that kingdom remain higlighted, but less
                //if (UIManager.Instance.currentlyShowingKingdom.id == this.city.kingdom.id) {
                //    this.city.kingdom.HighlightAllOwnedTilesInKingdom();
                //    if (UIManager.Instance.currentlyShowingCity != null) {
                //        if (UIManager.Instance.currentlyShowingCity.id == this.city.id) {
                //            this.city.HighlightAllOwnedTiles(204f / 255f);
                //        }
                //    }
                //} else {
                //    this.city.kingdom.UnHighlightAllOwnedTilesInKingdom();
                //    if (UIManager.Instance.currentlyShowingCity != null) {
                //        if (UIManager.Instance.currentlyShowingCity.id == this.city.id) {
                //            this.city.HighlightAllOwnedTiles(204f / 255f);
                //        }
                //    }
                //}
            }
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

    /*[ContextMenu("Increase General HP")]
	public void IncreaseGeneralHP(){
		List<Citizen> generals = this.city.GetCitizensWithRole (ROLE.GENERAL);
		for (int i = 0; i < generals.Count; i++) {
			((General)generals[i].assignedRole).army.hp += 100;
			Debug.Log (((General)generals [i].assignedRole).citizen.name + " hp is " + ((General)generals [i].assignedRole).army.hp.ToString ());
		}
	}*/

    [ContextMenu("Show Adjacent Cities")]
    public void ShowAdjacentCities() {
        for (int i = 0; i < this.city.adjacentCities.Count; i++) {
            Debug.Log("Adjacent City: " + this.city.adjacentCities[i].name);
        }
    }

    [ContextMenu("Show Adjacent Kingdoms")]
    public void ShowAdjacentKingdoms() {
        for (int i = 0; i < this.city.kingdom.adjacentKingdoms.Count; i++) {
            Debug.Log("Adjacent Kingdom: " + this.city.kingdom.adjacentKingdoms[i].name);
        }
    }

    [ContextMenu("Show Hextile Positions")]
    public void ShowHextileBounds() {
        Debug.Log("Local Pos: " + this.transform.localPosition.ToString());
        Debug.Log("Pos: " + this.transform.position.ToString());
    }

    [ContextMenu("Force Kill City")]
    public void ForceKillCity() {
        city.KillCity();
    }

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

    private void ShowKingdomInfo() {
        string text = this.city.name + " HP: " + this.city.hp.ToString() + "/" + this.city.maxHP.ToString() + "\n";
        text += "[b]" + this.city.kingdom.name + "[/b]" +
            "\n [b]Unrest:[/b] " + this.city.kingdom.unrest.ToString() +
            "\n [b]GOLD:[/b] " + this.city.kingdom.goldCount.ToString() + "/" + this.city.kingdom.maxGold.ToString() +
            "\n [b]Tech Level:[/b] " + this.city.kingdom.techLevel.ToString() +
            "\n [b]Kingdom Type:[/b] " + this.city.kingdom.kingdomType.ToString() +
            "\n [b]Expansion Rate:[/b] " + this.city.kingdom.expansionRate.ToString() +
            "\n [b]Growth Rate: [/b]" + this.city.totalDailyGrowth.ToString() +
            "\n [b]Current Growth: [/b]" + this.city.currentGrowth.ToString() + "/" + this.city.maxGrowth.ToString() + "\n";
        //if(this.city.kingdom.availableResources.Count > 0) {
        //    for (int i = 0; i < this.city.kingdom.availableResources.Keys.Count; i++) {
        //        text += this.city.kingdom.availableResources.Keys.ElementAt(i).ToString() + "\n";
        //    }
        //} else {
        //    text += "NONE\n";
        //}
       
        text += "[b]Embargo List: [/b]\n";
        if (this.city.kingdom.embargoList.Count > 0) {
            for (int i = 0; i < this.city.kingdom.embargoList.Keys.Count; i++) {
                text += this.city.kingdom.embargoList.Keys.ElementAt(i).name + "\n";
            }
        } else {
            text += "NONE\n";
        }

        //text += "[b]Trade Routes: [/b]\n";
        //if (this.city.kingdom.tradeRoutes.Count > 0) {
        //    for (int i = 0; i < this.city.kingdom.tradeRoutes.Count; i++) {
        //        TradeRoute currTradeRoute = this.city.kingdom.tradeRoutes[i];
        //        text += currTradeRoute.sourceKingdom.name + " -> " + currTradeRoute.targetKingdom.name + ": " + currTradeRoute.resourceBeingTraded.ToString() + "\n";
        //    }
        //} else {
        //    text += "NONE\n";
        //}

        text += "[b]Discovered Kingdoms: [/b]\n";
        if (this.city.kingdom.discoveredKingdoms.Count > 0) {
            for (int i = 0; i < this.city.kingdom.discoveredKingdoms.Count; i++) {
                Kingdom currKingdom = this.city.kingdom.discoveredKingdoms[i];
                text += currKingdom.name + "\n";
            }
        } else {
            text += "NONE\n";
        }

        //text += "[b]King Values: [/b]\n";
        //Dictionary<CHARACTER_VALUE, int> charVals = this.city.kingdom.king.importantCharacterValues;
        //if (charVals.Count > 0) {
        //    for (int i = 0; i < charVals.Count(); i++) {
        //        KeyValuePair<CHARACTER_VALUE, int> kvp = charVals.ElementAt(i);
        //        text += kvp.Key.ToString() + " - " + kvp.Value.ToString() + "\n";
        //    }
        //} else {
        //    text += "NONE\n";
        //}

        text += "[b]Kingdom values: [/b]\n";
        Dictionary<CHARACTER_VALUE, int> kingdomVals = this.city.kingdom.importantCharacterValues;
        if (kingdomVals.Count > 0) {
            for (int i = 0; i < kingdomVals.Count(); i++) {
                KeyValuePair<CHARACTER_VALUE, int> kvp = kingdomVals.ElementAt(i);
                text += kvp.Key.ToString() + " - " + kvp.Value.ToString() + "\n";
            }
        } else {
            text += "NONE\n";
        }
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
		//TODO: add/remove poison icon on tile
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
				Destroy(this.plagueIcon);
			}
		}
	}

	private void InterveneEventOnTile(EVENT_TYPES eventType){
		switch(eventType){
		case EVENT_TYPES.BOON_OF_POWER:
			EventCreator.Instance.CreateBoonOfPowerEvent (this);
			break;
		}
	}
	internal void PutEventOnTile(GameEvent gameEvent){
		if(this._gameEventInTile == null){
			this._gameEventInTile = gameEvent;
            if(gameEvent is FirstAndKeystone) {
                ((FirstAndKeystone)gameEvent).avatar = GameObject.Instantiate(Resources.Load("GameObjects/Keystone"), gameEventObjectsParentGO.transform) as GameObject;
                ((FirstAndKeystone)gameEvent).avatar.transform.localPosition = Vector3.zero;
            } else if(gameEvent is BoonOfPower) {
                ((BoonOfPower)gameEvent).avatar = GameObject.Instantiate(Resources.Load("GameObjects/BoonOfPower"), gameEventObjectsParentGO.transform) as GameObject;
                ((BoonOfPower)gameEvent).avatar.transform.localPosition = Vector3.zero;
                ((BoonOfPower)gameEvent).avatar.GetComponent<BoonOfPowerAvatar>().Init((BoonOfPower)gameEvent);
            } else if (gameEvent is AltarOfBlessing) {
                ((AltarOfBlessing)gameEvent).avatar = GameObject.Instantiate(Resources.Load("GameObjects/AltarOfBlessing"), gameEventObjectsParentGO.transform) as GameObject;
                ((AltarOfBlessing)gameEvent).avatar.transform.localPosition = Vector3.zero;
                ((AltarOfBlessing)gameEvent).avatar.GetComponent<AltarOfBlessingAvatar>().Init((AltarOfBlessing)gameEvent);
            } else {
                GameObject eventAvatar = GameObject.Instantiate(Resources.Load("GameObjects/GameEventAvatar"), gameEventObjectsParentGO.transform) as GameObject;
                gameEvent.gameEventAvatar = eventAvatar.GetComponent<GameEventAvatar>();
                gameEvent.gameEventAvatar.Init(gameEvent, this);
                gameEvent.gameEventAvatar.transform.localPosition = Vector3.zero;
            } 
			CreateEventNamePlate();
        }
	}
	internal void RemoveEventOnTile(){
		this._gameEventInTile = null;
        RemoveHextileEventNamePlate();
  //      Transform[] children = Utilities.GetComponentsInDirectChildren<Transform>(UIParent.gameObject);
		//for (int i = 0; i < children.Length; i++) {
		//	if(children[i].gameObject.tag == "EventTileNameplate"){
		//		Destroy(children[i].gameObject);
		//	}
		//}
	}
	internal GameEvent GetEventFromTile(){
		return this._gameEventInTile;
	}
	internal void SetKeystone(bool state){
		this.hasKeystone = state;
		//TODO: add/remove keystone icon on tile
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
				Destroy(this.plagueIcon);
			}
		}
	}

	internal void SetFirst(bool state){
		this.hasFirst = state;
		//TODO: add/remove first icon on tile
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
				Destroy(this.plagueIcon);
			}
		}
	}

    internal void CollectEventOnTile(Kingdom claimant, Citizen citizen = null) {
        if (gameEventInTile != null) {
            //if(citizen != null) {
            //    if(citizen.assignedRole is Adventurer) {
            //        ((Adventurer)citizen.assignedRole).SetLatestDiscovery(gameEventInTile);
            //    }
            //}

            if (gameEventInTile is BoonOfPower) {
                BoonOfPower boonOfPower = (BoonOfPower)gameEventInTile;
                boonOfPower.TransferBoonOfPower(claimant, citizen);
            } else if (gameEventInTile is FirstAndKeystone) {
                FirstAndKeystone firstAndKeystone = (FirstAndKeystone)gameEventInTile;
                firstAndKeystone.TransferKeystone(claimant, citizen);
			} else if (gameEventInTile is AltarOfBlessing) {
				AltarOfBlessing altarOfBlessing = (AltarOfBlessing)gameEventInTile;
				altarOfBlessing.TransferAltarOfBlessing(claimant, citizen);
			}else if(gameEventInTile is DevelopWeapons) {
                if(claimant.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.STRENGTH) 
                    || claimant.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.TRADITION)) {
                    DevelopWeapons developWeapons = (DevelopWeapons)gameEventInTile;
                    developWeapons.ClaimWeapon(claimant);
                }
            } else {
                gameEventInTile.OnCollectAvatarAction(claimant);
            }
        }
    }

    public void SetTag(int tag) {
        this.tag = tag;
        tagVisual.text = tag.ToString();
        //tagVisual.gameObject.SetActive(true);
    }

    internal void EnterCitizen(Citizen citizen) {
        this._citizensOnTile.Add(citizen);
    }
    internal void ExitCitizen(Citizen citizen) {
        this._citizensOnTile.Remove(citizen);
    }
    #region For Testing
    [ContextMenu("Force Reset Tile")]
    public void ForceResetTile() {
        ResetTile();
    }

    [Space(10)]
    [Header("For Testing")]
    [SerializeField] private int kingdomToConquerIndex = 0;
    [ContextMenu("Force Conquer Tile")]
    public void ForceTileToBeConqueredByKingdom() {
        Kingdom conqueror = KingdomManager.Instance.allKingdoms[kingdomToConquerIndex];
        if(conqueror.id == this.city.kingdom.id) {
            Debug.LogWarning("City is already part of " + conqueror.name);
        } else {
            conqueror.ConquerCity(city, null);
        }
    }
    #endregion
}
