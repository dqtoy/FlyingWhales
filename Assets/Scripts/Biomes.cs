using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Biomes : MonoBehaviour {
	public static Biomes Instance;

    [Header("Biome Generation Settings")]
	public float initialTemperature;
	public float initialTemperature2;
	public float intervalTemperature;
	public float temperature;
	public int[] hexInterval;
	public float[] temperatureInterval;

	[Space(10)]
    [Header("Biome Sprites")]
    [SerializeField] private Sprite[] grasslandTiles;
	[SerializeField] private Sprite[] forestTiles;
	[SerializeField] private Sprite[] woodlandTiles;
	[SerializeField] private Sprite[] desertTiles;
	[SerializeField] private Sprite[] tundraTiles;
	[SerializeField] private Sprite[] waterTiles;
	[SerializeField] private Sprite[] snowTiles;
	[SerializeField] private Sprite[] _bareTiles;

    [Space(10)]
    [Header("Biome Detail Prefabs")]
    [SerializeField] private GameObject[] grasslandDetails;
    [SerializeField] private GameObject[] tundraDetails;
    [SerializeField] private GameObject[] snowDetails;
    [SerializeField] private GameObject[] desertDetails;


    [Space(10)]
    [Header("Mountain Sprites")]
    [SerializeField] private Sprite[] greenMountainTiles;
	[SerializeField] private Sprite[] desertMountainTiles;
	[SerializeField] private Sprite[] snowAndTundraMountainTiles;

    [Space(10)]
    [Header("Tree Sprites")]
    [SerializeField] private Sprite[] woodlandTrees;
	[SerializeField] private Sprite[] forestTrees;

    [Space(10)]
    [Header("Biome Textures")]
    [SerializeField] private Sprite grasslandTexture;
	[SerializeField] private Sprite snowTexture;
	[SerializeField] private Sprite desertTexture;
	[SerializeField] private Sprite forestTexture;
	[SerializeField] private Sprite woodlandTexture;
	[SerializeField] private Sprite tundraTexture;

    [Space(10)]
    [Header("Hextile Masks")]
    public Texture[] topRightMasks;
    public Texture[] rightMasks;
    public Texture[] botRightMasks;
    public Texture[] topLeftMasks;
    public Texture[] leftMasks;
    public Texture[] botLeftMasks;

    [Space(10)]
    [Header("Resource Prefabs")]
    public GameObject behemothPrefab;
    public GameObject cobaltPrefab;
    public GameObject cornPrefab;
    public GameObject deerPrefab;
    public GameObject ebonyPrefab;
    public GameObject granitePrefab;
    public GameObject manaStonesPrefab;
    public GameObject mithrilPrefab;
    public GameObject oakPrefab;
    public GameObject pigPrefab;
    public GameObject ricePrefab;
    public GameObject slatePrefab;
    public GameObject wheatPrefab;

    #region getters/setters
    public Sprite[] bareTiles{
		get{ return this._bareTiles; }
	}
	#endregion

	void Awake(){
		Instance = this;
	}

	internal void GenerateBiome(List<HexTile> tiles){
		//CalculateNewTemperature();
		for(int i = 0; i < tiles.Count; i++){
            HexTile currentHexTile = tiles[i];
            BIOMES biomeForTile = GetBiomeSimple(currentHexTile.gameObject);
            SetBiomeForTile(biomeForTile, currentHexTile);
            //SetElevationSpriteForTile(currentHexTile);
            //currentHexTile.SetPassableState(false);
        }
		//GenerateBareBiome();
	}
    internal void LoadPassableObjects(List<HexTile> tiles, List<HexTile> outerGrid = null) {
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currentHexTile = tiles[i];
            LoadPassableObjects(currentHexTile);
        }
        if (outerGrid != null) {
            for (int i = 0; i < outerGrid.Count; i++) {
                HexTile currentHexTile = outerGrid[i];
                LoadPassableObjects(currentHexTile, true);
            }
        }
    }

    internal void LoadPassableObjects(HexTile currentHexTile, bool isOuterGrid = false) {
        object centerObj = GetCenterObject(currentHexTile);
        currentHexTile.SetPassableObject(centerObj);
        if (currentHexTile.elevationType == ELEVATION.PLAIN && !isOuterGrid) {
            currentHexTile.SetPassableState(true);
        } else {
            currentHexTile.SetPassableState(false);
        }
    }
    //internal void LoadElevationSprites() {
    //    for (int i = 0; i < GridMap.Instance.hexTiles.Count; i++) {
    //        HexTile currTile = GridMap.Instance.hexTiles[i];
    //        SetElevationSpriteForTile(currTile);
    //        currTile.UpdateLedgesAndOutlines();
    //    }
    //}
    internal void SetBiomeForTile(BIOMES biomeForTile, HexTile currentHexTile) {
        currentHexTile.SetBiome(biomeForTile);

        //if (currentHexTile.elevationType == ELEVATION.WATER) {
        //    return;
        //}
        //int sortingOrder = currentHexTile.xCoordinate - currentHexTile.yCoordinate;
        //switch (currentHexTile.biomeType) {
        //    case BIOMES.SNOW:
        //        Sprite snowSpriteToUse = snowTiles[Random.Range(0, snowTiles.Length)];
        //        currentHexTile.SetBaseSprite(snowSpriteToUse);
        //        sortingOrder += 6;
        //        break;
        //    case BIOMES.TUNDRA:
        //        Sprite tundraSpriteToUse = tundraTiles[Random.Range(0, tundraTiles.Length)];
        //        currentHexTile.SetBaseSprite(tundraSpriteToUse);
        //        sortingOrder += 3;
        //        break;
        //    case BIOMES.DESERT:
        //        Sprite desertSpriteToUse = desertTiles[Random.Range(0, desertTiles.Length)];
        //        currentHexTile.SetBaseSprite(desertSpriteToUse);
        //        sortingOrder += 5;
        //        break;
        //    case BIOMES.GRASSLAND:
        //        Sprite grasslandSpriteToUse = grasslandTiles[Random.Range(0, grasslandTiles.Length)];
        //        currentHexTile.SetBaseSprite(grasslandSpriteToUse);
        //        sortingOrder += 1;
        //        break;
        //    //case BIOMES.WOODLAND:
        //    //    Sprite woodlandSpriteToUse = woodlandTiles[Random.Range(0, woodlandTiles.Length)];
        //    //    currentHexTile.SetBaseSprite(woodlandSpriteToUse);
        //    //    sortingOrder += 2;
        //    //    break;
        //    case BIOMES.FOREST:
        //        Sprite forestSpriteToUse = forestTiles[Random.Range(0, forestTiles.Length)];
        //        currentHexTile.SetBaseSprite(forestSpriteToUse);
        //        sortingOrder += 4;
        //        break;
        //}

        //currentHexTile.SetSortingOrder(sortingOrder);
    }

    internal void UpdateTileVisuals(HexTile currentHexTile, bool updateNeighbours = false) {
        int sortingOrder = currentHexTile.xCoordinate - currentHexTile.yCoordinate;
        if (currentHexTile.elevationType == ELEVATION.WATER) {
            SetElevationSpriteForTile(currentHexTile);
            currentHexTile.UpdateLedgesAndOutlines();
            currentHexTile.SetSortingOrder(sortingOrder);
            return;
        } else {
            currentHexTile.UpdateLedgesAndOutlines();
            if (updateNeighbours) {
                currentHexTile.AllNeighbours.ForEach(x => x.UpdateLedgesAndOutlines());
            }
        }
        switch (currentHexTile.biomeType) {
            case BIOMES.SNOW:
                Sprite snowSpriteToUse = snowTiles[Random.Range(0, snowTiles.Length)];
                currentHexTile.SetBaseSprite(snowSpriteToUse);
                sortingOrder += 6;
                break;
            case BIOMES.TUNDRA:
                Sprite tundraSpriteToUse = tundraTiles[Random.Range(0, tundraTiles.Length)];
                currentHexTile.SetBaseSprite(tundraSpriteToUse);
                sortingOrder += 3;
                break;
            case BIOMES.DESERT:
                Sprite desertSpriteToUse = desertTiles[Random.Range(0, desertTiles.Length)];
                currentHexTile.SetBaseSprite(desertSpriteToUse);
                sortingOrder += 5;
                break;
            case BIOMES.GRASSLAND:
                Sprite grasslandSpriteToUse = grasslandTiles[Random.Range(0, grasslandTiles.Length)];
                currentHexTile.SetBaseSprite(grasslandSpriteToUse);
                sortingOrder += 1;
                break;
            //case BIOMES.WOODLAND:
            //    Sprite woodlandSpriteToUse = woodlandTiles[Random.Range(0, woodlandTiles.Length)];
            //    currentHexTile.SetBaseSprite(woodlandSpriteToUse);
            //    sortingOrder += 2;
            //    break;
            case BIOMES.FOREST:
                Sprite forestSpriteToUse = forestTiles[Random.Range(0, forestTiles.Length)];
                currentHexTile.SetBaseSprite(forestSpriteToUse);
                sortingOrder += 4;
                break;
        }

        currentHexTile.SetSortingOrder(sortingOrder);
    }

    internal void UpdateTileVisuals(List<HexTile> allTiles) {
        //List<HexTile> allTiles = new List<HexTile>(GridMap.Instance.hexTiles);
        //allTiles.AddRange(GridMap.Instance.outerGridList);
        for (int i = 0; i < allTiles.Count; i++) {
            HexTile currentHexTile = allTiles[i];
            UpdateTileVisuals(currentHexTile);
        }
        
    }
    internal void GenerateTileBiomeDetails(List<HexTile> tiles) {
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currentHexTile = tiles[i];
            //if(currentHexTile.elevationType != ELEVATION.PLAIN) {
            //    continue;
            //}
            //if (currentHexTile.biomeType == BIOMES.FOREST) {
            //    continue;
            //}
            GenerateTileBiomeDetails(currentHexTile);
        }
    }
    internal void GenerateTileBiomeDetails(HexTile tile) {
        AddBiomeDetailToTile(tile);
    }

    internal void AddBiomeDetailToTile(HexTile tile) {
        //if(tile.elevationType != ELEVATION.PLAIN) {
        //    return;
        //}
        GameObject biomeDetailToUse = null;
        //Sprite centerSpriteToUse = null;
        if (tile.elevationType == ELEVATION.PLAIN) {
            switch (tile.biomeType) {
                case BIOMES.SNOW:
                    if (snowDetails.Length > 0) {
                        biomeDetailToUse = snowDetails[Random.Range(0, snowDetails.Length)];
                    }
                    break;
                case BIOMES.TUNDRA:
                    if (tundraDetails.Length > 0) {
                        biomeDetailToUse = tundraDetails[Random.Range(0, tundraDetails.Length)];
                    }
                    break;
                case BIOMES.DESERT:
                    if (desertDetails.Length > 0) {
                        biomeDetailToUse = desertDetails[Random.Range(0, desertDetails.Length)];
                    }
                    break;
                case BIOMES.GRASSLAND:
                    if (grasslandDetails.Length > 0) {
                        biomeDetailToUse = grasslandDetails[Random.Range(0, grasslandDetails.Length)];
                    }
                    break;
                    //case BIOMES.WOODLAND:
                    //    centerSpriteToUse = woodlandTrees[Random.Range(0, woodlandTrees.Length)];
                    //    tile.SetCenterSprite(centerSpriteToUse);
                    //    //Utilities.SetSpriteSortingLayer(tile.centerPiece.spriteRenderer, "Structures Layer");
                    //    break;
                    //case BIOMES.FOREST:
                    //    centerSpriteToUse = forestTrees[Random.Range(0, forestTrees.Length)];
                    //    tile.SetCenterSprite(centerSpriteToUse);
                    //    //Utilities.SetSpriteSortingLayer(tile.centerPiece.spriteRenderer, "Structures Layer");
                    //    break;
            }
        }
        //if (biomeDetailToUse != null) {
            tile.AddBiomeDetailToTile(biomeDetailToUse);
        //}

        //tile.UpdateSortingOrder();
    }

    internal void SetElevationSpriteForTile(HexTile currentHexTile) {
        //int sortingOrder = currentHexTile.xCoordinate - currentHexTile.yCoordinate;
        if(currentHexTile.elevationType == ELEVATION.WATER) {
            Sprite waterSpriteToUse = waterTiles[Random.Range(0, waterTiles.Length)];
            currentHexTile.spriteRenderer.sortingLayerName = "Water";
            currentHexTile.spriteRenderer.sprite = waterSpriteToUse;
            currentHexTile.centerPiece.SetActive(false);
            return;
        }
        //switch (currentHexTile.biomeType) {
        //    case BIOMES.SNOW:
        //        if (currentHexTile.elevationType == ELEVATION.MOUNTAIN) {
        //            Sprite mountainSpriteToUse = snowAndTundraMountainTiles[Random.Range(0, snowAndTundraMountainTiles.Length)];
        //            currentHexTile.SetCenterSprite(mountainSpriteToUse);
        //            Utilities.SetSpriteSortingLayer(currentHexTile.centerPiece.spriteRenderer, "TileDetails");
        //        }
        //        break;
        //    case BIOMES.TUNDRA:
        //        if (currentHexTile.elevationType == ELEVATION.MOUNTAIN) {
        //            Sprite mountainSpriteToUse = snowAndTundraMountainTiles[Random.Range(0, snowAndTundraMountainTiles.Length)];
        //            currentHexTile.SetCenterSprite(mountainSpriteToUse);
        //            Utilities.SetSpriteSortingLayer(currentHexTile.centerPiece.spriteRenderer, "TileDetails");
        //        }
        //        break;
        //    case BIOMES.DESERT:
        //        if (currentHexTile.elevationType == ELEVATION.MOUNTAIN) {
        //            Sprite mountainSpriteToUse = desertMountainTiles[Random.Range(0, desertMountainTiles.Length)];
        //            currentHexTile.SetCenterSprite(mountainSpriteToUse);
        //            Utilities.SetSpriteSortingLayer(currentHexTile.centerPiece.spriteRenderer, "TileDetails");
        //        }
        //        break;
        //    case BIOMES.GRASSLAND:
        //        if (currentHexTile.elevationType == ELEVATION.MOUNTAIN) {
        //            Sprite mountainSpriteToUse = greenMountainTiles[Random.Range(0, greenMountainTiles.Length)];
        //            currentHexTile.SetCenterSprite(mountainSpriteToUse);
        //            Utilities.SetSpriteSortingLayer(currentHexTile.centerPiece.spriteRenderer, "TileDetails");
        //        }
        //        break;
        //    //case BIOMES.WOODLAND:
        //    //    if (currentHexTile.elevationType == ELEVATION.MOUNTAIN) {
        //    //        Sprite mountainSpriteToUse = greenMountainTiles[Random.Range(0, greenMountainTiles.Length)];
        //    //        currentHexTile.SetCenterSprite(mountainSpriteToUse);
        //    //        Utilities.SetSpriteSortingLayer(currentHexTile.centerPiece.spriteRenderer, "TileDetails");
        //    //    }
        //    //    break;
        //    case BIOMES.FOREST:
        //        if (currentHexTile.elevationType == ELEVATION.MOUNTAIN) {
        //            Sprite mountainSpriteToUse = greenMountainTiles[Random.Range(0, greenMountainTiles.Length)];
        //            currentHexTile.SetCenterSprite(mountainSpriteToUse);
        //            Utilities.SetSpriteSortingLayer(currentHexTile.centerPiece.spriteRenderer, "TileDetails");
        //        }
        //        break;
        //}
        //currentHexTile.UpdateSortingOrder();
    }
	internal void DeactivateCenterPieces(){
		for (int i = 0; i < GridMap.Instance.listHexes.Count; i++) {
			HexTile currentHexTile = GridMap.Instance.listHexes [i].GetComponent<HexTile> ();
			currentHexTile.DeactivateCenterPiece();
		}
	}
	internal void GenerateElevation(List<HexTile> tiles, int mapWidth, int mapHeight) {
		CalculateElevationAndMoisture(tiles, mapWidth, mapHeight);
	}
	private void CalculateElevationAndMoisture(List<HexTile> tiles, int mapWidth, int mapHeight){
        float elevationFrequency = 19.1f; //14.93f;//2.66f;
        float moistureFrequency = 12.34f; //3.34f;//2.94f;
		float tempFrequency = 2.64f;//2.4f;

		float elevationRand = UnityEngine.Random.Range(500f,2000f);
		float moistureRand = UnityEngine.Random.Range(500f,2000f);
		float temperatureRand = UnityEngine.Random.Range(500f,2000f);

		string[] splittedNameEq = EquatorGenerator.Instance.listEquator[0].name.Split(new char[]{','});
		int equatorY = int.Parse (splittedNameEq [1]);

		for(int i = 0; i < tiles.Count; i++){
            HexTile currTile = tiles[i];

            string[] splittedName = currTile.name.Split(new char[]{','});
			int[] xy = {int.Parse(splittedName[0]), int.Parse(splittedName[1])};

			float nx = ((float)xy[0]/mapWidth);
			float ny = ((float)xy[1]/mapHeight);

			float elevationNoise = Mathf.PerlinNoise((nx + elevationRand) * elevationFrequency, (ny + elevationRand) * elevationFrequency);
			ELEVATION elevationType = GetElevationType(elevationNoise);

            currTile.data.elevationNoise = elevationNoise;
            currTile.SetElevation (elevationType);
            currTile.data.moistureNoise = Mathf.PerlinNoise((nx + moistureRand) * moistureFrequency, (ny + moistureRand) * moistureFrequency);

			int distanceToEquator = Mathf.Abs (xy [1] - equatorY);
			float tempGradient = 1.23f / mapHeight;
            currTile.data.temperature = distanceToEquator * tempGradient;
            currTile.data.temperature += (Mathf.PerlinNoise((nx + temperatureRand) * tempFrequency, (ny + temperatureRand) * tempFrequency)) * 0.6f;
		}
	}

	private ELEVATION GetElevationType(float elevationNoise){
        //return ELEVATION.PLAIN;
        if (elevationNoise <= 0.20f) {
			return ELEVATION.WATER;
		} else if (elevationNoise > 0.20f && elevationNoise <= 0.39f) {
			return ELEVATION.TREES;
        } else if (elevationNoise > 0.39f && elevationNoise <= 0.6f) {
            return ELEVATION.PLAIN;
        } else { 
            return ELEVATION.MOUNTAIN;
        }
    }

	private BIOMES GetBiomeSimple(GameObject goHex){
		float moistureNoise = goHex.GetComponent<HexTile>().moistureNoise;
		float temperature = goHex.GetComponent<HexTile>().temperature;

		if(temperature <= 0.4f) {
            if (moistureNoise <= 0.45f) {
                return BIOMES.DESERT;
            } else {
                return BIOMES.GRASSLAND;
            }

            //if(moistureNoise <= 0.45f){
            //	return BIOMES.DESERT;
            //}else if(moistureNoise > 0.45f && moistureNoise <= 0.65f){
            //	return BIOMES.GRASSLAND;
            //}else if(moistureNoise > 0.65f){
            //	return BIOMES.WOODLAND;
            //}	

            /*
			if(moistureNoise <= 0.20f){
				return BIOMES.DESERT;
			}else if(moistureNoise > 0.20f && moistureNoise <= 0.40f){
				return BIOMES.GRASSLAND;
			}else if(moistureNoise > 0.40f && moistureNoise <= 0.55f){
				return BIOMES.WOODLAND;
			}else if(moistureNoise > 0.55f){
				return BIOMES.FOREST;
			}
			*/
        } else if(temperature > 0.4f && temperature <= 0.72f){
            if (moistureNoise <= 0.45f) {
                return BIOMES.GRASSLAND;
            } else {
                return BIOMES.FOREST;
            }
            //if(moistureNoise <= 0.45f){
            //	return BIOMES.GRASSLAND;
            //}else if(moistureNoise > 0.45f && moistureNoise <= 0.55f){
            //	return BIOMES.WOODLAND;
            //}else if(moistureNoise > 0.55f){
            //	return BIOMES.FOREST;
            //}			
            /*
			if(moistureNoise <= 0.20f){
				return BIOMES.DESERT;
			}else if(moistureNoise > 0.20f && moistureNoise <= 0.55f){
				return BIOMES.GRASSLAND;
			}else if(moistureNoise > 0.55f && moistureNoise <= 0.75f){
				return BIOMES.WOODLAND;
			}else if(moistureNoise > 0.75f){
				return BIOMES.FOREST;
			}
			*/
        } else if(temperature > 0.72f && temperature <= 0.82f){
			if (moistureNoise <= 0.62f){
				return BIOMES.TUNDRA;			
			} else if (moistureNoise > 0.62f){
				return BIOMES.SNOW;
			}

			/*
			if(moistureNoise <= 0.2f){
				return BIOMES.TUNDRA;
			}else if(moistureNoise > 0.2f && moistureNoise <= 0.55f){
				return BIOMES.GRASSLAND;
			}else if(moistureNoise > 0.55f && moistureNoise <= 0.75f){
				return BIOMES.WOODLAND;
			}else if(moistureNoise > 0.75f){
				return BIOMES.SNOW;
			}
			*/
		} else if(temperature > 0.82f){
			if(moistureNoise <= 0.4f){
				return BIOMES.TUNDRA;
			}else if(moistureNoise > 0.4f){
				return BIOMES.SNOW;
			}
		}
		return BIOMES.DESERT;
	}

	internal void GenerateBareBiome(){
		for(int i = 0; i < GridMap.Instance.listHexes.Count; i++){
			HexTile currentHexTile = GridMap.Instance.listHexes[i].GetComponent<HexTile>();
			ELEVATION elevationType = currentHexTile.elevationType;
			float moisture = currentHexTile.moistureNoise;

			if(elevationType == ELEVATION.WATER){
				if(moisture <= 0.3f) {
					currentHexTile.data.biomeType = BIOMES.BARE;
					Sprite bareSpriteToUse = _bareTiles [Random.Range (0, _bareTiles.Length)];
					currentHexTile.SetBaseSprite (bareSpriteToUse);
				}
			}
		}
	}

	internal void GenerateTileEdges(){
		for (int i = 0; i < GridMap.Instance.listHexes.Count; i++) {
			HexTile currHexTile = GridMap.Instance.listHexes[i].GetComponent<HexTile>();
            currHexTile.LoadEdges();
		}
        for (int i = 0; i < GridMap.Instance.outerGridList.Count; i++) {
            HexTile currHexTile = GridMap.Instance.outerGridList[i];
            currHexTile.LoadEdges();
        }
    }

    public void DetermineIslands() {
        List<HexTile> passableTiles = GetPassableTiles();
        Dictionary<HexTile, MapIsland> islands = new Dictionary<HexTile, MapIsland>();
        for (int i = 0; i < passableTiles.Count; i++) {
            HexTile currTile = passableTiles[i];
            MapIsland island = new MapIsland(currTile);
            islands.Add(currTile, island);
        }

        Queue<HexTile> tileQueue = new Queue<HexTile>();

        while (passableTiles.Count != 0) {
            HexTile currTile;
            if (tileQueue.Count <= 0) {
                currTile = passableTiles[Random.Range(0, passableTiles.Count)];
            } else {
                currTile = tileQueue.Dequeue();
            }
            MapIsland islandOfCurrTile = islands[currTile];
            List<HexTile> neighbours = currTile.AllNeighbours;
            for (int i = 0; i < neighbours.Count; i++) {
                HexTile currNeighbour = neighbours[i];
                if (currNeighbour.isPassable && passableTiles.Contains(currNeighbour)) {
                    MapIsland islandOfNeighbour = islands[currNeighbour];
                    MergeIslands(islandOfCurrTile, islandOfNeighbour, islands);
                    tileQueue.Enqueue(currNeighbour);
                }
            }
            passableTiles.Remove(currTile);
        }

        List<MapIsland> allIslands = new List<MapIsland>();
        foreach (KeyValuePair<HexTile, MapIsland> kvp in islands) {
            MapIsland currIsland = kvp.Value;
            if (!allIslands.Contains(currIsland)) {
                allIslands.Add(currIsland);
            }
        }
        ////Color islands
        //for (int i = 0; i < allIslands.Count; i++) {
        //    MapIsland currIsland = allIslands[i];
        //    Color islandColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        //    for (int j = 0; j < currIsland.tilesInIsland.Count; j++) {
        //        HexTile currTile = currIsland.tilesInIsland[j];
        //        currTile.highlightGO.SetActive(true);
        //        currTile.highlightGO.GetComponent<SpriteRenderer>().color = islandColor;
        //    }
        //}

        ConnectIslands(allIslands, islands);
    }

    private void ConnectIslands(List<MapIsland> islands, Dictionary<HexTile, MapIsland> islandsDict) {
        //List<MapIsland> allIslands = islands.OrderByDescending(x => x.tilesInIsland.Count).ToList();
        //for (int i = 0; i < allIslands.Count; i++) {
        //    MapIsland currIsland = allIslands[i];
        //    if (currIsland.tilesInIsland.Count > 0) {
        //        ConnectToNearestIsland(currIsland, islandsDict, islands);
        //    }
        //}
        //islands = islands.OrderByDescending(x => x.tilesInIsland.Count).ToList();
        while (islands.Count > 1) {
            MapIsland currIsland = islands[Random.Range(0, islands.Count)];
            ConnectToNearestIsland(currIsland, islandsDict, islands);
            //for (int i = 0; i < islands.Count; i++) {
            //    MapIsland currIsland = islands[i];
            //    CameraMove.Instance.CenterCameraOn(currIsland.mainTile.gameObject);
            //    if (currIsland.tilesInIsland.Count > 0) {
            //        ConnectToNearestIsland(currIsland, islandsDict, islands);
            //    }
        }
    }

    private void ConnectToNearestIsland(MapIsland originIsland, Dictionary<HexTile, MapIsland> islandsDict, List<MapIsland> islands) {
        int nearestDistance = 9999;
        MapIsland nearestIsland = null;
        List<HexTile> nearestPath = null;


        for (int i = 0; i < islands.Count; i++) {
            MapIsland otherIsland = islands[i];
            if (otherIsland != originIsland && !AreIslandsConnected(originIsland, otherIsland)) {
                List<HexTile> path = PathGenerator.Instance.GetPath(originIsland.mainTile, otherIsland.mainTile, PATHFINDING_MODE.UNRESTRICTED);
                if (path != null && path.Count < nearestDistance) {
                    nearestDistance = path.Count;
                    nearestPath = path;
                    nearestIsland = otherIsland;
                }
            }
        }

        //List<MapIsland> nearIslands = GetNearIslands(originIsland, islandsDict);

        //if (nearIslands.Count <= 0) {
        //    throw new System.Exception("There are no near islands!");
        //}

        //for (int i = 0; i < originIsland.outerTiles.Count; i++) {
        //    HexTile originIslandTile = originIsland.outerTiles[i];
        //    List<HexTile> allOtherOuterTiles = new List<HexTile>();
        //    for (int j = 0; j < nearIslands.Count; j++) {
        //        MapIsland otherIsland = nearIslands[j];
        //        if (originIsland != otherIsland && otherIsland.tilesInIsland.Count > 0 && !AreIslandsConnected(originIsland, otherIsland)) {
        //            allOtherOuterTiles.AddRange(otherIsland.outerTiles);
        //        }
        //    }
        //    for (int j = 0; j < allOtherOuterTiles.Count; j++) {
        //        HexTile tileInOtherIsland = allOtherOuterTiles[j];
        //        List<HexTile> path = PathGenerator.Instance.GetPath(tileInOtherIsland, originIslandTile, PATHFINDING_MODE.UNRESTRICTED);
        //        if (path != null) {
        //            if (path.Count < nearestDistance) {
        //                nearestPath = path;
        //                nearestDistance = path.Count;
        //                nearestIsland = islandsDict[tileInOtherIsland];
        //            }
        //        }
        //    }
        //}

        //for (int i = 0; i < islands.Count; i++) {
        //    MapIsland otherIsland = islands[i];
        //    if (originIsland != otherIsland && !AreIslandsConnected(originIsland, otherIsland)) {
        //        for (int j = 0; j < otherIsland.outerTiles.Count; j++) {
        //            HexTile tileInOtherIsland = otherIsland.outerTiles[j];
        //            for (int k = 0; k < originIsland.outerTiles.Count; k++) {
        //                HexTile tileInOriginIsland = originIsland.outerTiles[k];
        //                List<HexTile> path = PathGenerator.Instance.GetPath(tileInOtherIsland, tileInOriginIsland, PATHFINDING_MODE.NORMAL);
        //                if (path != null) {
        //                    if (path.Count < nearestDistance) {
        //                        nearestPath = path;
        //                        nearestDistance = path.Count;
        //                        nearestIsland = otherIsland;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        if (nearestPath != null) {
            //originIsland.AddTileToIsland(nearestPath); 
            MergeIslands(originIsland, nearestIsland, islandsDict);
            islands.Remove(nearestIsland);
            FlattenTiles(nearestPath);
        }
        

        //else {
        //    throw new System.Exception("Could not not connect island!");
        //}
    }

    private List<MapIsland> GetNearIslands(MapIsland originIsland, Dictionary<HexTile, MapIsland> islands) {
        List<MapIsland> nearIslands = new List<MapIsland>();
        List<Collider2D> colliders = new List<Collider2D>();
        List<HexTile> checkedTiles = new List<HexTile>();
        for (int i = 0; i < originIsland.outerTiles.Count; i++) {
            HexTile currOuterTile = originIsland.outerTiles[i];
            Collider2D[] collidesWith = Physics2D.OverlapCircleAll(currOuterTile.transform.position, 10f, LayerMask.GetMask("Hextiles"));
            for (int j = 0; j < collidesWith.Length; j++) {
                HexTile currTile = collidesWith[j].GetComponent<HexTile>();
                if (currTile != null && !checkedTiles.Contains(currTile) && islands.ContainsKey(currTile)) {
                    MapIsland islandOfTile = islands[currTile];
                    if (originIsland != islandOfTile && !nearIslands.Contains(islandOfTile)) {
                        nearIslands.Add(islandOfTile);
                    }
                }
                checkedTiles.Add(currTile);
            }

        }
        //Collider2D[] colliders = Physics2D.OverlapCircleAll(originIsland.mainTile.transform.position, 20f, LayerMask.GetMask("Hextiles"));
        //for (int i = 0; i < colliders.Length; i++) {
        //    HexTile currTile = colliders[i].GetComponent<HexTile>();
        //    if (islands.ContainsKey(currTile)) {
        //        MapIsland islandOfTile = islands[currTile];
        //        if (originIsland != islandOfTile && !nearIslands.Contains(islandOfTile)) {
        //            nearIslands.Add(islandOfTile);
        //        }
        //    }
        //}
        return nearIslands;
    }

    private bool AreIslandsConnected(MapIsland island1, MapIsland island2) {
        HexTile randomTile1 = island1.tilesInIsland[Random.Range(0, island1.tilesInIsland.Count)];
        HexTile randomTile2 = island2.tilesInIsland[Random.Range(0, island2.tilesInIsland.Count)];

        return PathGenerator.Instance.GetPath(randomTile1, randomTile2, PATHFINDING_MODE.USE_ROADS) != null;
    }

    public List<HexTile> GetPassableTiles() {
        List<HexTile> passableTiles = new List<HexTile>();
        for (int i = 0; i < GridMap.Instance.hexTiles.Count; i++) {
            HexTile currTile = GridMap.Instance.hexTiles[i];
            if (currTile.isPassable) {
                passableTiles.Add(currTile);
            }
        }
        return passableTiles;
    }
    private MapIsland MergeIslands(MapIsland island1, MapIsland island2, Dictionary<HexTile, MapIsland> islands) {
        if (island1 == island2) {
            return island1;
        }
        island1.AddTileToIsland(island2.tilesInIsland);
        for (int i = 0; i < island2.tilesInIsland.Count; i++) {
            HexTile currTile = island2.tilesInIsland[i];
            islands[currTile] = island1;
        }
        island2.ClearIsland();
        return island1;
    }
    private void FlattenTiles(List<HexTile> tiles) {
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currTile = tiles[i];
            if (currTile.isPassable) {
                continue;
            }
            //currTile.highlightGO.SetActive(true);
            //currTile.highlightGO.GetComponent<SpriteRenderer>().color = Color.black;
            currTile.SetElevation(ELEVATION.PLAIN);
            currTile.SetPassableState(true);
        }
    }

   // [ContextMenu("Generate Tags")]
   // public void GenerateTileTags() {
   //     List<HexTile> tilesToTag = new List<HexTile>(GridMap.Instance.listHexes.Select(x => x.GetComponent<HexTile>()));
   //     int currTag = 0;
   //     Queue<HexTile> tagQueue = new Queue<HexTile>();
   //     HexTile firstTile = null;
   //     //tagQueue.Enqueue(firstTile);

   //     ELEVATION currElevation = ELEVATION.PLAIN;

   //     while (tilesToTag.Count != 0) {
   //         if(tagQueue.Count <= 0) {
   //             //move on to other tag
   //             currTag++;
   //             firstTile = tilesToTag.FirstOrDefault();
   //             firstTile.SetTag(currTag);
   //             tilesToTag.Remove(firstTile);
   //             tagQueue.Enqueue(firstTile);
   //             currElevation = firstTile.elevationType;
   //         }

   //         HexTile parentTile = tagQueue.Dequeue();
            
			//List<HexTile> parentTileNeighbours = new List<HexTile>(parentTile.AllNeighbours);
   //         for (int i = 0; i < parentTileNeighbours.Count; i++) {
   //             HexTile currNeighbour = parentTileNeighbours[i];
   //             if(tilesToTag.Contains(currNeighbour) && 
   //                 (currNeighbour.elevationType == currElevation 
   //                 || (currNeighbour.elevationType == ELEVATION.MOUNTAIN && currElevation == ELEVATION.PLAIN) 
   //                 || currNeighbour.elevationType == ELEVATION.PLAIN && currElevation == ELEVATION.MOUNTAIN)) {
   //                 currNeighbour.SetTag(currTag);
   //                 tilesToTag.Remove(currNeighbour);
   //                 tagQueue.Enqueue(currNeighbour);
   //             }
   //         }
   //     }
   // }

    [ContextMenu("DisableAllFogOfWarSprites")]
    public void DisableAllFogOfWarSprites() {
        for (int i = 0; i < GridMap.Instance.listHexes.Count; i++) {
            HexTile currTile = GridMap.Instance.listHexes[i].GetComponent<HexTile>();
            currTile.HideFogOfWarObjects();
        }
    }

    [ContextMenu("EnableAllFogOfWarSprites")]
    public void EnableAllFogOfWarSprites() {
        for (int i = 0; i < GridMap.Instance.listHexes.Count; i++) {
            HexTile currTile = GridMap.Instance.listHexes[i].GetComponent<HexTile>();
            currTile.ShowFogOfWarObjects();
        }
    }

    #region Utilities
    public Sprite GetCenterPieceSprite(HexTile tile) {
        if (!tile.isPassable && tile.elevationType != ELEVATION.WATER) {
            switch (tile.biomeType) {
                case BIOMES.SNOW:
                case BIOMES.TUNDRA:
                    return snowAndTundraMountainTiles[Random.Range(0, snowAndTundraMountainTiles.Length)];
                case BIOMES.DESERT:
                    return desertMountainTiles[Random.Range(0, desertMountainTiles.Length)];
                case BIOMES.GRASSLAND:
                    return greenMountainTiles[Random.Range(0, greenMountainTiles.Length)];
                case BIOMES.FOREST:
                    return forestTrees[Random.Range(0, forestTrees.Length)];
            }
        }
        return null;
    }
    internal Sprite GetTextureForBiome(BIOMES biomeType) {
        if (biomeType == BIOMES.GRASSLAND) {
            return grasslandTexture;
        } 
        //else if (biomeType == BIOMES.WOODLAND) {
        //    return grasslandTexture;
        //} 
        else if (biomeType == BIOMES.TUNDRA) {
            return tundraTexture;
        } else if (biomeType == BIOMES.FOREST) {
            return forestTexture;
        } else if (biomeType == BIOMES.DESERT) {
            return desertTexture;
        } else if (biomeType == BIOMES.SNOW) {
            return snowTexture;
        }
        return null;
    }
    public object GetCenterObject(HexTile tile) {
        switch (tile.elevationType) {
            case ELEVATION.MOUNTAIN:
                switch (tile.biomeType) {
                    case BIOMES.SNOW:
                    case BIOMES.TUNDRA:
                        return snowAndTundraMountainTiles[Random.Range(0, snowAndTundraMountainTiles.Length)];
                    case BIOMES.DESERT:
                        return desertMountainTiles[Random.Range(0, desertMountainTiles.Length)];
                    case BIOMES.GRASSLAND:
                    case BIOMES.FOREST:
                        return greenMountainTiles[Random.Range(0, greenMountainTiles.Length)];
                }
                break;
            case ELEVATION.PLAIN:
            case ELEVATION.TREES:
                switch (tile.biomeType) {
                    case BIOMES.SNOW:
                    case BIOMES.TUNDRA:
                    case BIOMES.DESERT:
                        return ebonyPrefab;
                    case BIOMES.GRASSLAND:
                        return oakPrefab;
                    case BIOMES.FOREST:
                        return forestTrees[Random.Range(0, forestTrees.Length)];
                }
                break;
        }
        return null;
    }
    #endregion

    //internal GameObject GetPrefabForResource(RESOURCE resource) {
    //    switch (resource) {
    //        case RESOURCE.CORN:
    //            return cornPrefab;
    //        case RESOURCE.WHEAT:
    //            return wheatPrefab;
    //        case RESOURCE.RICE:
    //            return ricePrefab;
    //        case RESOURCE.DEER:
    //            return deerPrefab;
    //        case RESOURCE.PIG:
    //            return pigPrefab;
    //        case RESOURCE.BEHEMOTH:
    //            return behemothPrefab;
    //        case RESOURCE.OAK:
    //            return oakPrefab;
    //        case RESOURCE.EBONY:
    //            return ebonyPrefab;
    //        case RESOURCE.GRANITE:
    //            return granitePrefab;
    //        case RESOURCE.SLATE:
    //            return slatePrefab;
    //        case RESOURCE.MANA_STONE:
    //            return manaStonesPrefab;
    //        case RESOURCE.MITHRIL:
    //            return mithrilPrefab;
    //        case RESOURCE.COBALT:
    //            return cobaltPrefab;
    //        default:
    //            return null;
    //    }
    //}
}

public class MapIsland {
    private HexTile _mainTile;
    private List<HexTile> _tilesInIsland;
    private List<HexTile> _outerTiles;

    public HexTile mainTile {
        get { return _mainTile; }
    }
    public List<HexTile> tilesInIsland {
        get { return _tilesInIsland; }
    }
    public List<HexTile> outerTiles {
        get { return _outerTiles; }
    }

    public MapIsland(HexTile tile) {
        _mainTile = tile;
        _tilesInIsland = new List<HexTile>();
        _outerTiles = new List<HexTile>();
        AddTileToIsland(tile);
    }

    public void AddTileToIsland(HexTile tile, bool recomputeOuterTiles = true) {
        if (!_tilesInIsland.Contains(tile)) {
            _tilesInIsland.Add(tile);
            if (recomputeOuterTiles) {
                RecomputeOuterTiles();
            }
        }
    }
    public void AddTileToIsland(List<HexTile> tiles) {
        for (int i = 0; i < tiles.Count; i++) {
            AddTileToIsland(tiles[i], false);
        }
        RecomputeOuterTiles();
    }
    public void RemoveTileFromIsland(HexTile tile) {
        _tilesInIsland.Remove(tile);
    }
    public void ClearIsland() {
        _tilesInIsland.Clear();
    }
    public void RecomputeOuterTiles() {
        _outerTiles.Clear();
        for (int i = 0; i < tilesInIsland.Count; i++) {
            HexTile currTile = tilesInIsland[i];
            if (currTile.AllNeighbours.Where(x => !_tilesInIsland.Contains(x)).Any()) {
                _outerTiles.Add(currTile);
            }
        }
    }
}