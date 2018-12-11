using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

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
    [SerializeField] private Sprite[] grasslandCorruptedTiles;
    [SerializeField] private Sprite[] forestTiles;
    [SerializeField] private Sprite[] forestCorruptedTiles;
    [SerializeField] private Sprite[] desertTiles;
    [SerializeField] private Sprite[] desertCorruptedTiles;
    [SerializeField] private Sprite[] tundraTiles;
    [SerializeField] private Sprite[] tundraCorruptedTiles;
    [SerializeField] private Sprite[] waterTiles;
	[SerializeField] private Sprite[] snowTiles;
    [SerializeField] private Sprite[] snowCorruptedTiles;
    [SerializeField] private Sprite[] _bareTiles;
    [SerializeField] private Sprite[] _ancientRuinTiles;
    [SerializeField] private Sprite[] ancientRuinCorruptedTiles;

    [Space(10)]
    [Header("Mountain Sprites")]
    [SerializeField] private Sprite[] grasslandMountains;
    [SerializeField] private Sprite[] grasslandMountainsCorrupted;
    [SerializeField] private Sprite[] forestMountains;
    [SerializeField] private Sprite[] forestMountainsCorrupted;
    [SerializeField] private Sprite[] desertMountains;
    [SerializeField] private Sprite[] desertMountainsCorrupted;
    [SerializeField] private Sprite[] snowMountains;
    [SerializeField] private Sprite[] snowMountainsCorrupted;
    [SerializeField] private Sprite[] tundraMounains;
    [SerializeField] private Sprite[] tundraMountainsCorrupted;

    [Space(10)]
    [Header("Tree Sprites")]
    [SerializeField] private Sprite[] grasslandTrees;
    [SerializeField] private Sprite[] grasslandTreesCorrupted;
    [SerializeField] private Sprite[] forestTrees;
    [SerializeField] private Sprite[] forestTreesCorrupted;
    [SerializeField] private Sprite[] desertTrees;
    [SerializeField] private Sprite[] desertTreesCorrupted;
    [SerializeField] private Sprite[] snowTrees;
    [SerializeField] private Sprite[] snowTreesCorrupted;
    [SerializeField] private Sprite[] tundraTrees;
    [SerializeField] private Sprite[] tundraTreesCorrupted;

    [Space(10)]
    [Header("Animations")]
    [SerializeField] private BiomeSpriteAnimationDictionary biomeSpriteAnimations;

    #region getters/setters
    public Sprite[] bareTiles{
		get{ return this._bareTiles; }
	}
    public Sprite[] ancienctRuinTiles {
        get { return this._ancientRuinTiles; }
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

    public void LoadPassableStates(List<HexTile> tiles, List<HexTile> outerGrid = null) {
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currentHexTile = tiles[i];
            LoadPassableStates(currentHexTile);
        }
        if (outerGrid != null) {
            for (int i = 0; i < outerGrid.Count; i++) {
                HexTile currentHexTile = outerGrid[i];
                LoadPassableStates(currentHexTile, true);
            }
        }
    }
    public void LoadPassableStates(HexTile currentHexTile, bool isOuterGrid = false) {
        if (currentHexTile.elevationType == ELEVATION.PLAIN && !isOuterGrid) {
            currentHexTile.SetPassableState(true);
        } else {
            currentHexTile.SetPassableState(false);
        }
    }
    internal void SetBiomeForTile(BIOMES biomeForTile, HexTile currentHexTile) {
        currentHexTile.SetBiome(biomeForTile);
    }
    internal void UpdateTileVisuals(List<HexTile> allTiles) {
        for (int i = 0; i < allTiles.Count; i++) {
            HexTile currentHexTile = allTiles[i];
            UpdateTileVisuals(currentHexTile);
        }
    }
    public void UpdateTileVisuals(HexTile currentHexTile) {
#if WORLD_CREATION_TOOL
        int sortingOrder = (((int)worldcreator.WorldCreatorManager.Instance.height - 1) -  (currentHexTile.yCoordinate - 2)) * 10;
#else
        int sortingOrder = (((int)GridMap.Instance.height - 1) -  (currentHexTile.yCoordinate - 2)) * 10; //10 is the number of sorting order between rows
        if (PlayerManager.Instance.player != null &&  PlayerManager.Instance.player.playerArea != null 
            && PlayerManager.Instance.player.playerArea.tiles.Contains(currentHexTile)) {
            return;
        }
#endif

        LoadBeachVisuals(currentHexTile);
        if (currentHexTile.elevationType == ELEVATION.PLAIN) {
            LoadPlainTileVisuals(currentHexTile, sortingOrder);
        } else if (currentHexTile.elevationType == ELEVATION.MOUNTAIN) {
            LoadMountainTileVisuals(currentHexTile, sortingOrder);
        } else if (currentHexTile.elevationType == ELEVATION.TREES) {
            LoadTreeTileVisuals(currentHexTile, sortingOrder);
        } else {
            //For Water
            LoadWaterTileVisuals(currentHexTile, sortingOrder);
            //currentHexTile.SetSortingOrder(sortingOrder);
        }
    }
    public void CorruptTileVisuals(HexTile tile) {
        if(tile.elevationType == ELEVATION.PLAIN) {
            LoadCorruptedPlainTileVisuals(tile);
        }else if (tile.elevationType == ELEVATION.MOUNTAIN) {
            LoadCorruptedMountainTileVisuals(tile);
        }else if (tile.elevationType == ELEVATION.TREES) {
            LoadCorruptedTreeTileVisuals(tile);
        }
    }
    private void LoadPlainTileVisuals(HexTile tile, int sortingOrder) {
        switch (tile.biomeType) {
            case BIOMES.SNOW:
                Sprite snowSpriteToUse = snowTiles[UnityEngine.Random.Range(0, snowTiles.Length)];
                tile.SetBaseSprite(snowSpriteToUse);
                break;
            case BIOMES.TUNDRA:
                Sprite tundraSpriteToUse = tundraTiles[UnityEngine.Random.Range(0, tundraTiles.Length)];
                tile.SetBaseSprite(tundraSpriteToUse);
                break;
            case BIOMES.DESERT:
                Sprite desertSpriteToUse = desertTiles[UnityEngine.Random.Range(0, desertTiles.Length)];
                tile.SetBaseSprite(desertSpriteToUse);
                break;
            case BIOMES.GRASSLAND:
                Sprite grasslandSpriteToUse = grasslandTiles[UnityEngine.Random.Range(0, grasslandTiles.Length)];
                tile.SetBaseSprite(grasslandSpriteToUse);
                break;
            case BIOMES.FOREST:
                Sprite forestSpriteToUse = forestTiles[UnityEngine.Random.Range(0, forestTiles.Length)];
                tile.SetBaseSprite(forestSpriteToUse);
                break;
            case BIOMES.ANCIENT_RUIN:
                Sprite ruinSpriteToUse = ancienctRuinTiles[UnityEngine.Random.Range(0, ancienctRuinTiles.Length)];
                tile.SetBaseSprite(ruinSpriteToUse);
                break;
        }
        tile.SetSortingOrder(sortingOrder);
    }
    private void LoadCorruptedPlainTileVisuals(HexTile tile) {
        int index = 0;
        Sprite[] choices = null;
        switch (tile.biomeType) {
            case BIOMES.SNOW:
                index = Array.IndexOf(snowTiles, tile.spriteRenderer.sprite);
                choices = snowCorruptedTiles;
                break;
            case BIOMES.TUNDRA:
                index = Array.IndexOf(tundraTiles, tile.spriteRenderer.sprite);
                choices = tundraCorruptedTiles;
                break;
            case BIOMES.DESERT:
                index = Array.IndexOf(desertTiles, tile.spriteRenderer.sprite);
                choices = desertCorruptedTiles;
                break;
            case BIOMES.GRASSLAND:
                index = Array.IndexOf(grasslandTiles, tile.spriteRenderer.sprite);
                choices = grasslandCorruptedTiles;
                break;
            case BIOMES.FOREST:
                index = Array.IndexOf(forestTiles, tile.spriteRenderer.sprite);
                choices = forestCorruptedTiles;
                break;
            case BIOMES.ANCIENT_RUIN:
                index = Array.IndexOf(ancienctRuinTiles, tile.spriteRenderer.sprite);
                choices = ancientRuinCorruptedTiles;
                break;
        }
        if (index != -1) {
            tile.SetBaseSprite(choices[index]);
        }
    }
    private void LoadMountainTileVisuals(HexTile tile, int sortingOrder) {
        switch (tile.biomeType) {
            case BIOMES.SNOW:
                Sprite snowSpriteToUse = snowMountains[UnityEngine.Random.Range(0, snowMountains.Length)];
                tile.SetBaseSprite(snowSpriteToUse);
                break;
            case BIOMES.TUNDRA:
                Sprite tundraSpriteToUse = tundraMounains[UnityEngine.Random.Range(0, tundraMounains.Length)];
                tile.SetBaseSprite(tundraSpriteToUse);
                break;
            case BIOMES.DESERT:
                Sprite desertSpriteToUse = desertMountains[UnityEngine.Random.Range(0, desertMountains.Length)];
                tile.SetBaseSprite(desertSpriteToUse);
                break;
            case BIOMES.GRASSLAND:
                Sprite grasslandSpriteToUse = grasslandMountains[UnityEngine.Random.Range(0, grasslandMountains.Length)];
                tile.SetBaseSprite(grasslandSpriteToUse);
                break;
            case BIOMES.FOREST:
                Sprite forestSpriteToUse = forestMountains[UnityEngine.Random.Range(0, forestMountains.Length)];
                tile.SetBaseSprite(forestSpriteToUse);
                break;
            case BIOMES.ANCIENT_RUIN:
                Sprite ruinSpriteToUse = ancienctRuinTiles[UnityEngine.Random.Range(0, ancienctRuinTiles.Length)];
                tile.SetBaseSprite(ruinSpriteToUse);
                break;
        }
        tile.SetSortingOrder(sortingOrder);
    }
    private void LoadCorruptedMountainTileVisuals(HexTile tile) {
        int index = 0;
        switch (tile.biomeType) {
            case BIOMES.SNOW:
            index = Array.IndexOf(snowMountains, tile.spriteRenderer.sprite);
            tile.SetBaseSprite(snowMountainsCorrupted[index]);
            break;
            case BIOMES.TUNDRA:
            index = Array.IndexOf(tundraMounains, tile.spriteRenderer.sprite);
            tile.SetBaseSprite(tundraMountainsCorrupted[index]);
            break;
            case BIOMES.DESERT:
            index = Array.IndexOf(desertMountains, tile.spriteRenderer.sprite);
            tile.SetBaseSprite(desertMountainsCorrupted[index]);
            break;
            case BIOMES.GRASSLAND:
            index = Array.IndexOf(grasslandMountains, tile.spriteRenderer.sprite);
            tile.SetBaseSprite(grasslandMountainsCorrupted[index]);
            break;
            case BIOMES.FOREST:
            index = Array.IndexOf(forestMountains, tile.spriteRenderer.sprite);
            tile.SetBaseSprite(forestMountainsCorrupted[index]);
            break;
            case BIOMES.ANCIENT_RUIN:
            index = Array.IndexOf(ancienctRuinTiles, tile.spriteRenderer.sprite);
            tile.SetBaseSprite(ancientRuinCorruptedTiles[index]);
            break;
        }
    }
    private void LoadTreeTileVisuals(HexTile tile, int sortingOrder) {
        switch (tile.biomeType) {
            case BIOMES.SNOW:
                Sprite snowSpriteToUse = snowTrees[UnityEngine.Random.Range(0, snowTrees.Length)];
                tile.SetBaseSprite(snowSpriteToUse);
                break;
            case BIOMES.TUNDRA:
                Sprite tundraSpriteToUse = tundraTrees[UnityEngine.Random.Range(0, tundraTrees.Length)];
                tile.SetBaseSprite(tundraSpriteToUse);
                break;
            case BIOMES.DESERT:
                Sprite desertSpriteToUse = desertTrees[UnityEngine.Random.Range(0, desertTrees.Length)];
                tile.SetBaseSprite(desertSpriteToUse);
                break;
            case BIOMES.GRASSLAND:
                Sprite grasslandSpriteToUse = grasslandTrees[UnityEngine.Random.Range(0, grasslandTrees.Length)];
                tile.SetBaseSprite(grasslandSpriteToUse);
                break;
            case BIOMES.FOREST:
                Sprite forestSpriteToUse = forestTrees[UnityEngine.Random.Range(0, forestTrees.Length)];
                tile.SetBaseSprite(forestSpriteToUse);
                break;
            case BIOMES.ANCIENT_RUIN:
                Sprite ruinSpriteToUse = ancienctRuinTiles[UnityEngine.Random.Range(0, ancienctRuinTiles.Length)];
                tile.SetBaseSprite(ruinSpriteToUse);
                break;
        }
        tile.SetSortingOrder(sortingOrder);
    }
    private void LoadCorruptedTreeTileVisuals(HexTile tile) {
        int index = 0;
        switch (tile.biomeType) {
            case BIOMES.SNOW:
            index = Array.IndexOf(snowTrees, tile.spriteRenderer.sprite);
            tile.SetBaseSprite(snowTreesCorrupted[index]);
            break;
            case BIOMES.TUNDRA:
            index = Array.IndexOf(tundraTrees, tile.spriteRenderer.sprite);
            tile.SetBaseSprite(tundraTreesCorrupted[index]);
            break;
            case BIOMES.DESERT:
            index = Array.IndexOf(desertTrees, tile.spriteRenderer.sprite);
            tile.SetBaseSprite(desertTreesCorrupted[index]);
            break;
            case BIOMES.GRASSLAND:
            index = Array.IndexOf(grasslandTrees, tile.spriteRenderer.sprite);
            tile.SetBaseSprite(grasslandTreesCorrupted[index]);
            break;
            case BIOMES.FOREST:
            index = Array.IndexOf(forestTrees, tile.spriteRenderer.sprite);
            tile.SetBaseSprite(forestTreesCorrupted[index]);
            break;
            case BIOMES.ANCIENT_RUIN:
            index = Array.IndexOf(ancienctRuinTiles, tile.spriteRenderer.sprite);
            tile.SetBaseSprite(ancientRuinCorruptedTiles[index]);
            break;
        }
    }
    private void LoadWaterTileVisuals(HexTile tile, int sortingOrder) {
        Sprite waterSpriteToUse = waterTiles[UnityEngine.Random.Range(0, waterTiles.Length)];
        tile.spriteRenderer.sortingLayerName = "Water";
        tile.spriteRenderer.sprite = waterSpriteToUse;
        tile.DeactivateCenterPiece();
        return;
    }
    private void LoadBeachVisuals(HexTile tile) {
        tile.LoadBeaches();
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
					Sprite bareSpriteToUse = _bareTiles [UnityEngine.Random.Range (0, _bareTiles.Length)];
					currentHexTile.SetBaseSprite (bareSpriteToUse);
				}
			}
		}
	}

	//internal void GenerateTileEdges(){
	//	for (int i = 0; i < GridMap.Instance.listHexes.Count; i++) {
	//		HexTile currHexTile = GridMap.Instance.listHexes[i].GetComponent<HexTile>();
 //           currHexTile.LoadEdges();
	//	}
 //       for (int i = 0; i < GridMap.Instance.outerGridList.Count; i++) {
 //           HexTile currHexTile = GridMap.Instance.outerGridList[i];
 //           currHexTile.LoadEdges();
 //       }
 //   }

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
                currTile = passableTiles[UnityEngine.Random.Range(0, passableTiles.Count)];
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
            MapIsland currIsland = islands[UnityEngine.Random.Range(0, islands.Count)];
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
        HexTile randomTile1 = island1.tilesInIsland[UnityEngine.Random.Range(0, island1.tilesInIsland.Count)];
        HexTile randomTile2 = island2.tilesInIsland[UnityEngine.Random.Range(0, island2.tilesInIsland.Count)];

        return PathGenerator.Instance.GetPath(randomTile1, randomTile2, PATHFINDING_MODE.PASSABLE) != null;
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

    public RuntimeAnimatorController GetTileSpriteAnimation(Sprite sprite) {
        if (biomeSpriteAnimations.ContainsKey(sprite)) {
            return biomeSpriteAnimations[sprite];
        }
        return null;
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

    //[ContextMenu("DisableAllFogOfWarSprites")]
    //public void DisableAllFogOfWarSprites() {
    //    for (int i = 0; i < GridMap.Instance.listHexes.Count; i++) {
    //        HexTile currTile = GridMap.Instance.listHexes[i].GetComponent<HexTile>();
    //        currTile.HideFogOfWarObjects();
    //    }
    //}

    //[ContextMenu("EnableAllFogOfWarSprites")]
    //public void EnableAllFogOfWarSprites() {
    //    for (int i = 0; i < GridMap.Instance.listHexes.Count; i++) {
    //        HexTile currTile = GridMap.Instance.listHexes[i].GetComponent<HexTile>();
    //        currTile.ShowFogOfWarObjects();
    //    }
    //}

    #region Utilities
    //public Sprite GetCenterPieceSprite(HexTile tile) {
    //    if (!tile.isPassable && tile.elevationType != ELEVATION.WATER) {
    //        switch (tile.biomeType) {
    //            case BIOMES.SNOW:
    //            case BIOMES.TUNDRA:
    //                return snowAndTundraMountainTiles[UnityEngine.Random.Range(0, snowAndTundraMountainTiles.Length)];
    //            case BIOMES.DESERT:
    //                return desertMountainTiles[UnityEngine.Random.Range(0, desertMountainTiles.Length)];
    //            case BIOMES.GRASSLAND:
    //                return greenMountainTiles[UnityEngine.Random.Range(0, greenMountainTiles.Length)];
    //            case BIOMES.FOREST:
    //                return forestTrees[UnityEngine.Random.Range(0, forestTrees.Length)];
    //        }
    //    }
    //    return null;
    //}
    //internal Sprite GetTextureForBiome(BIOMES biomeType) {
    //    if (biomeType == BIOMES.GRASSLAND) {
    //        return grasslandTexture;
    //    } 
    //    //else if (biomeType == BIOMES.WOODLAND) {
    //    //    return grasslandTexture;
    //    //} 
    //    else if (biomeType == BIOMES.TUNDRA) {
    //        return tundraTexture;
    //    } else if (biomeType == BIOMES.FOREST) {
    //        return forestTexture;
    //    } else if (biomeType == BIOMES.DESERT) {
    //        return desertTexture;
    //    } else if (biomeType == BIOMES.SNOW) {
    //        return snowTexture;
    //    }
    //    return null;
    //}
    //public object GetCenterObject(HexTile tile) {
    //    switch (tile.elevationType) {
    //        case ELEVATION.MOUNTAIN:
    //            switch (tile.biomeType) {
    //                case BIOMES.SNOW:
    //                case BIOMES.TUNDRA:
    //                    return snowAndTundraMountainTiles[UnityEngine.Random.Range(0, snowAndTundraMountainTiles.Length)];
    //                case BIOMES.DESERT:
    //                    return desertMountainTiles[UnityEngine.Random.Range(0, desertMountainTiles.Length)];
    //                case BIOMES.GRASSLAND:
    //                case BIOMES.FOREST:
    //                    return greenMountainTiles[UnityEngine.Random.Range(0, greenMountainTiles.Length)];
    //            }
    //            break;
    //        case ELEVATION.PLAIN:
    //        case ELEVATION.TREES:
    //            switch (tile.biomeType) {
    //                case BIOMES.SNOW:
    //                case BIOMES.TUNDRA:
    //                case BIOMES.DESERT:
    //                    return ebonyPrefab;
    //                case BIOMES.GRASSLAND:
    //                    return oakPrefab;
    //                case BIOMES.FOREST:
    //                    return forestTrees[UnityEngine.Random.Range(0, forestTrees.Length)];
    //            }
    //            break;
    //    }
    //    return null;
    //}
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