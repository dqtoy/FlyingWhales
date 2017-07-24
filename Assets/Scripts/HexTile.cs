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

	public int isBorderOfCityID = 0;
	internal int isOccupiedByCityID = 0;
    internal List<City> isVisibleByCities = new List<City>();

    [Space(10)]
    [Header("Tile Visuals")]
    [SerializeField] private GameObject _centerPiece;
    //[SerializeField] private GameObject resourceVisualGO;
    [SerializeField] private ResourceIcon resourceIcon;
	[SerializeField] private SpriteRenderer _kingdomColorSprite;
	[SerializeField] private GameObject _highlightGO;
    [SerializeField] private Transform UIParent;
    [SerializeField] private Transform resourceParent;

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
    [SerializeField] private GameObject plagueIconGO;
    [SerializeField] private FOG_OF_WAR_STATE _currFogOfWarState;

    private GameEvent _gameEventInTile;
    private Transform _cityInfoParent;
    private CityItem _cityInfo;
	private GameObject plagueIcon;

    [System.NonSerialized] public List<HexTile> connectedTiles = new List<HexTile>();

	public IEnumerable<HexTile> AllNeighbours { get; set; }
	public IEnumerable<HexTile> ValidTiles { get { return AllNeighbours.Where(o => o.elevationType != ELEVATION.WATER && o.elevationType != ELEVATION.MOUNTAIN); } }
	public IEnumerable<HexTile> RoadTiles { get { return AllNeighbours.Where(o => o.isRoad); } }
	public IEnumerable<HexTile> PurchasableTiles { get { return AllNeighbours.Where (o => o.elevationType != ELEVATION.WATER);}}
	public IEnumerable<HexTile> CombatTiles { get { return AllNeighbours.Where (o => o.elevationType != ELEVATION.WATER);}}
    public IEnumerable<HexTile> AvatarTiles { get { return AllNeighbours.Where(o => o.elevationType != ELEVATION.WATER); } }

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
	public GameEvent gameEventInTile{
		get { return this._gameEventInTile; }
	}
    public FOG_OF_WAR_STATE currFogOfWarState {
        get { return _currFogOfWarState; }
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
		this._kingdomColorSprite.color = color;
	}

    private void SetMinimapTileColor(Color color) {
        minimapHexSprite.color = color;
    }

	public void ShowTileHighlight(){
		this._kingdomColorSprite.gameObject.SetActive(true);
	}

	public void HideTileHighlight(){
		this.kingdomColorSprite.gameObject.SetActive(false);
	}

    public void ShowCitySprite() {
        GameObject[] gameObjectsToChooseFrom = CityGenerator.Instance.GetStructurePrefabsForRace(this.city.kingdom.race, STRUCTURE_TYPE.CITY);
        GameObject structureGO = GameObject.Instantiate(
           gameObjectsToChooseFrom[Random.Range(0, gameObjectsToChooseFrom.Length)],
           structureParentGO.transform) as GameObject;
        structureGO.transform.localPosition = Vector3.zero;
        SpriteRenderer[] allColorizers = structureGO.GetComponentsInChildren<SpriteRenderer>().
            Where(x => x.gameObject.tag == "StructureColorizers").ToArray();

        for (int i = 0; i < allColorizers.Length; i++) {
            allColorizers[i].color = this.ownedByCity.kingdom.kingdomColor;
        }
        this._centerPiece.SetActive(false);

        //this.structureGO.GetComponent<SpriteRenderer>().sprite = CityGenerator.Instance.elfCitySprite;
        //this.structureGO.SetActive(true);
        //this.centerPiece.SetActive(false);
        Color color = this.city.kingdom.kingdomColor;
        color.a = 255f / 255f;
        SetMinimapTileColor(color);
        color.a = 76.5f / 255f;
        this._kingdomColorSprite.color = color;
        this.GetComponent<SpriteRenderer>().color = Color.white;
        //this.GetComponent<SpriteRenderer>().sprite = Biomes.Instance.bareTiles[Random.Range(0, Biomes.Instance.bareTiles.Length)];
    }

    public GameObject CreateSpecialStructureOnTile(LAIR lairType) {
        GameObject structureGO = GameObject.Instantiate(
            CityGenerator.Instance.GetStructurePrefabForSpecialStructures(lairType), structureParentGO.transform) as GameObject;
        structureGO.transform.localPosition = Vector3.zero;
        return structureGO;
    }

    public void ShowNamePlate() {
        //this.cityNameGO.SetActive(true);
        //this.cityNameLbl.GetComponent<Renderer>().sortingLayerName = "CityNames";
        //this.cityNameLbl.text = this.city.name + "\n" + this.city.kingdom.name;
        if(_cityInfo == null) {
            GameObject parentPanel = new GameObject("NamePlatePanel", typeof(UIPanel));
            parentPanel.layer = LayerMask.NameToLayer("UI");
            parentPanel.transform.SetParent(UIParent);
            parentPanel.transform.localPosition = Vector3.zero;
            parentPanel.transform.localScale = Vector3.one;
            this._cityInfoParent = parentPanel.transform;

            GameObject namePlateGO = UIManager.Instance.InstantiateUIObject(UIManager.Instance.cityItemPrefab, parentPanel.transform);
            this._cityInfo = namePlateGO.GetComponent<CityItem>();
            namePlateGO.transform.localPosition = new Vector3(-2.3f, -1.2f, 0f);
            namePlateGO.transform.localScale = new Vector3(0.02f, 0.02f, 0f);
            EventManager.Instance.onUpdateUI.AddListener(UpdateNamePlate);
        }
        UpdateNamePlate();
        this.cityInfo.gameObject.SetActive(true);
    }

    public void UpdateNamePlate() {
        this._cityInfo.SetCity(this.city);
    }

    public void HideNamePlate() {
        this._cityInfo.gameObject.SetActive(false);
    }

    public void ShowOccupiedSprite() {
        //this.GetComponent<SpriteRenderer>().sprite = Biomes.Instance.bareTiles[Random.Range(0, Biomes.Instance.bareTiles.Length)];
        GameObject[] structuresToChooseFrom = CityGenerator.Instance.GetStructurePrefabsForRace(this.ownedByCity.kingdom.race, STRUCTURE_TYPE.GENERIC);
        if (this.specialResource != RESOURCE.NONE) {
            if(Utilities.GetBaseResourceType(this.specialResource) == BASE_RESOURCE_TYPE.FOOD) {
                if(this.specialResource == RESOURCE.BEHEMOTH || this.specialResource == RESOURCE.DEER || 
                    this.specialResource == RESOURCE.PIG) {
                    structuresToChooseFrom = CityGenerator.Instance.GetStructurePrefabsForRace(this.ownedByCity.kingdom.race, STRUCTURE_TYPE.HUNTING_LODGE);
                } else {
                    structuresToChooseFrom = CityGenerator.Instance.GetStructurePrefabsForRace(this.ownedByCity.kingdom.race, STRUCTURE_TYPE.MINES);
                }
			} else if(Utilities.GetBaseResourceType(this.specialResource) == BASE_RESOURCE_TYPE.WOOD && this.ownedByCity.kingdom.race == RACE.ELVES) {
                structuresToChooseFrom = CityGenerator.Instance.GetStructurePrefabsForRace(this.ownedByCity.kingdom.race, STRUCTURE_TYPE.LUMBERYARD);
			} else if (Utilities.GetBaseResourceType(this.specialResource) == BASE_RESOURCE_TYPE.STONE && this.ownedByCity.kingdom.race == RACE.HUMANS) {
                structuresToChooseFrom = CityGenerator.Instance.GetStructurePrefabsForRace(this.ownedByCity.kingdom.race, STRUCTURE_TYPE.QUARRY);
            } else {
                structuresToChooseFrom = CityGenerator.Instance.GetStructurePrefabsForRace(this.ownedByCity.kingdom.race, STRUCTURE_TYPE.MINES);
            }
        }

        GameObject structureGO = GameObject.Instantiate(
            structuresToChooseFrom[Random.Range(0, structuresToChooseFrom.Length)],
            structureParentGO.transform) as GameObject;
        structureGO.transform.localPosition = Vector3.zero;
        SpriteRenderer[] allColorizers = structureGO.GetComponentsInChildren<SpriteRenderer>().
            Where(x => x.gameObject.tag == "StructureColorizers").ToArray();

        Color color = ownedByCity.kingdom.kingdomColor;
        color.a = 255f / 255f;
        SetMinimapTileColor(color);

        for (int i = 0; i < allColorizers.Length; i++) {
            allColorizers[i].color = this.ownedByCity.kingdom.kingdomColor;
        }
        this._centerPiece.SetActive(false);
    }

    public void HideStructures() {
        structureParentGO.SetActive(false);
    }

    public void ShowStructures() {
        structureParentGO.SetActive(true);
    }

    public void SetFogOfWarState(FOG_OF_WAR_STATE fowState) {
        _currFogOfWarState = fowState;
        if (KingdomManager.Instance.useFogOfWar) {
            Color newColor = FOWSprite.color;
            switch (fowState) {
                case FOG_OF_WAR_STATE.VISIBLE:
                    newColor.a = 0f / 255f;
                    if (isHabitable && isOccupied) {
                        ShowNamePlate();
                    }
                    if (isOccupied) {
                        ShowStructures();
                    }
                    break;
                case FOG_OF_WAR_STATE.SEEN:
                    newColor.a = 128f / 255f;
                    if (isHabitable && isOccupied) {
                        HideNamePlate();
                    }
                    if (isOccupied) {
                        HideStructures();
                    }
                    break;
                case FOG_OF_WAR_STATE.HIDDEN:
                    newColor.a = 230f / 255f;
                    if (isHabitable && isOccupied) {
                        HideNamePlate();
                    }
                    if (isOccupied) {
                        HideStructures();
                    }
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
    #endregion

    public void ResetTile(){
        //this.city = null;
        this.isOccupied = false;
		this.isBorder = false;
        this.isPlagued = false;
		this.ownedByCity = null;
		this.isBorderOfCityID = 0;
		this.isOccupiedByCityID = 0;
        SetMinimapTileColor(biomeColor);
        this._kingdomColorSprite.color = Color.white;
		this.kingdomColorSprite.gameObject.SetActive(false);
        EventManager.Instance.onUpdateUI.RemoveListener(UpdateNamePlate);
        Transform[] children = structureParentGO.GetComponentsInChildren<Transform>();
        for (int i = 0; i < children.Length; i++) {
            if (children[i].gameObject != null && children[i].gameObject != structureParentGO) {
                Destroy(children[i].gameObject);
            }
        }
        children = Utilities.GetComponentsInDirectChildren<Transform>(UIParent.gameObject);
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
        this.isVisibleByCities.Add(city);
		this.isOccupiedByCityID = city.id;		
		this.ownedByCity = city;
	}

	public void Borderize(City city) {
		this.isBorder = true;
        this.isVisibleByCities.Add(city);
        this.isBorderOfCityID = city.id;
		this.ownedByCity = city;
	}

    #region Monobehaviour Functions
    void OnMouseDown() {
        if (UIManager.Instance.IsMouseOnUI() || (KingdomManager.Instance.useFogOfWar && currFogOfWarState != FOG_OF_WAR_STATE.VISIBLE)) {
            return;
        }
        if (this.isHabitable && this.isOccupied && this.city != null) {
            CameraMove.Instance.CenterCameraOn(this.gameObject);
            UIManager.Instance.SetKingdomAsSelected(this.city.kingdom);
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
            if (UIManager.Instance.currentlyShowingKingdom != null) {
                //if there is currently showing kingdom, if this city is part of that kingdom remain higlighted, but less
                if (UIManager.Instance.currentlyShowingKingdom.id == this.city.kingdom.id) {
                    this.city.kingdom.HighlightAllOwnedTilesInKingdom();
                    if (UIManager.Instance.currentlyShowingCity != null) {
                        if (UIManager.Instance.currentlyShowingCity.id == this.city.id) {
                            this.city.HighlightAllOwnedTiles(204f / 255f);
                        }
                    }
                } else {
                    this.city.kingdom.UnHighlightAllOwnedTilesInKingdom();
                    if (UIManager.Instance.currentlyShowingCity != null) {
                        if (UIManager.Instance.currentlyShowingCity.id == this.city.id) {
                            this.city.HighlightAllOwnedTiles(204f / 255f);
                        }
                    }
                }
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

    private void ShowKingdomInfo() {
        string text = this.city.name + " HP: " + this.city.hp.ToString() + "/" + this.city.maxHP.ToString() + "\n";
        text += "[b]" + this.city.kingdom.name + "[/b]" +
            "\n [b]Unrest:[/b] " + this.city.kingdom.unrest.ToString() +
            "\n [b]GOLD:[/b] " + this.city.kingdom.goldCount.ToString() + "/" + this.city.kingdom.maxGold.ToString() +
            "\n [b]Tech Level:[/b] " + this.city.kingdom.techLevel.ToString() +
            "\n [b]Kingdom Type:[/b] " + this.city.kingdom.kingdomType.ToString() +
            "\n [b]Expansion Rate:[/b] " + this.city.kingdom.expansionRate.ToString() +
            "\n [b]Growth Rate: [/b]" + this.city.totalDailyGrowth.ToString() + 
            "\n [b]Current Growth: [/b]" + this.city.currentGrowth.ToString() + "/" + this.city.maxGrowth.ToString() +
            "\n [b]Available Resources: [/b]\n";
        if(this.city.kingdom.availableResources.Count > 0) {
            for (int i = 0; i < this.city.kingdom.availableResources.Keys.Count; i++) {
                text += this.city.kingdom.availableResources.Keys.ElementAt(i).ToString() + "\n";
            }
        } else {
            text += "NONE\n";
        }
       
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

        text += "[b]King Values: [/b]\n";
        Dictionary<CHARACTER_VALUE, int> charVals = this.city.kingdom.king.importantCharacterValues;
        if (charVals.Count > 0) {
            for (int i = 0; i < charVals.Count(); i++) {
                KeyValuePair<CHARACTER_VALUE, int> kvp = charVals.ElementAt(i);
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
				this.plagueIcon = UIManager.Instance.InstantiateUIObject (this.plagueIconGO, this.UIParent);
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
		}
	}
	internal void RemoveEventOnTile(){
		this._gameEventInTile = null;
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
				this.plagueIcon = UIManager.Instance.InstantiateUIObject (this.plagueIconGO, this.UIParent);
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
				this.plagueIcon = UIManager.Instance.InstantiateUIObject (this.plagueIconGO, this.UIParent);
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
            if(citizen != null) {
                if(citizen.assignedRole is Adventurer) {
                    ((Adventurer)citizen.assignedRole).SetLatestDiscovery(gameEventInTile);
                }
            }

            if (gameEventInTile is BoonOfPower) {
                BoonOfPower boonOfPower = (BoonOfPower)gameEventInTile;
                boonOfPower.TransferBoonOfPower(claimant, citizen);
            } else if (gameEventInTile is FirstAndKeystone) {
                FirstAndKeystone firstAndKeystone = (FirstAndKeystone)gameEventInTile;
                firstAndKeystone.TransferKeystone(claimant, citizen);
                RemoveEventOnTile();
			} else if (gameEventInTile is AltarOfBlessing) {
				AltarOfBlessing altarOfBlessing = (AltarOfBlessing)gameEventInTile;
				altarOfBlessing.TransferAltarOfBlessing(claimant, citizen);
			}
        }
    }
}
