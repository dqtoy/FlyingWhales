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

	internal void GenerateBiome(){
		//CalculateNewTemperature();
		for(int i = 0; i < GridMap.Instance.listHexes.Count; i++){
			GameObject currentHexTileGO = GridMap.Instance.listHexes[i];
			HexTile currentHexTile = GridMap.Instance.listHexes[i].GetComponent<HexTile>();
            BIOMES biomeForTile = GetBiomeSimple(currentHexTileGO);
            SetBiomeForTile(biomeForTile, currentHexTile);
            //SetElevationSpriteForTile(currentHexTile);
        }
		//GenerateBareBiome();
	}
    internal void LoadElevationSprites() {
        for (int i = 0; i < GridMap.Instance.hexTiles.Count; i++) {
            HexTile currTile = GridMap.Instance.hexTiles[i];
            SetElevationSpriteForTile(currTile);
        }
    }
    internal void SetBiomeForTile(BIOMES biomeForTile, HexTile currentHexTile) {
        currentHexTile.SetBiome(biomeForTile);

        //if (currentHexTile.elevationType == ELEVATION.WATER) {
        //    return;
        //}
        int sortingOrder = currentHexTile.xCoordinate - currentHexTile.yCoordinate;
        switch (currentHexTile.biomeType) {
            case BIOMES.SNOW:
                currentHexTile.movementDays = 1;//3;
                Sprite snowSpriteToUse = snowTiles[Random.Range(0, snowTiles.Length)];
                currentHexTile.SetBaseSprite(snowSpriteToUse);
                sortingOrder += 6;
                break;
            case BIOMES.TUNDRA:
                currentHexTile.movementDays = 1;//2;
                Sprite tundraSpriteToUse = tundraTiles[Random.Range(0, tundraTiles.Length)];
                currentHexTile.SetBaseSprite(tundraSpriteToUse);
                sortingOrder += 3;
                break;
            case BIOMES.DESERT:
                currentHexTile.movementDays = 2;//4;
                Sprite desertSpriteToUse = desertTiles[Random.Range(0, desertTiles.Length)];
                currentHexTile.SetBaseSprite(desertSpriteToUse);
                sortingOrder += 5;
                break;
            case BIOMES.GRASSLAND:
                currentHexTile.movementDays = 1;//2;
                Sprite grasslandSpriteToUse = grasslandTiles[Random.Range(0, grasslandTiles.Length)];
                currentHexTile.SetBaseSprite(grasslandSpriteToUse);
                sortingOrder += 1;
                break;
            case BIOMES.WOODLAND:
                currentHexTile.movementDays = 1;//3;
                Sprite woodlandSpriteToUse = woodlandTiles[Random.Range(0, woodlandTiles.Length)];
                currentHexTile.SetBaseSprite(woodlandSpriteToUse);
                sortingOrder += 2;
                break;
            case BIOMES.FOREST:
                currentHexTile.movementDays = 2;
                Sprite forestSpriteToUse = forestTiles[Random.Range(0, forestTiles.Length)];
                currentHexTile.SetBaseSprite(forestSpriteToUse);
                sortingOrder += 4;
                break;
        }

        currentHexTile.SetSortingOrder(sortingOrder);
    }

    internal void GenerateTileBiomeDetails() {
        for (int i = 0; i < GridMap.Instance.hexTiles.Count; i++) {
            HexTile currentHexTile = GridMap.Instance.hexTiles[i].GetComponent<HexTile>();
            if(currentHexTile.elevationType != ELEVATION.PLAIN) {
                continue;
            }
            AddBiomeDetailToTile(currentHexTile);
        }
    }

    internal void AddBiomeDetailToTile(HexTile tile) {
        if(tile.elevationType != ELEVATION.PLAIN) {
            return;
        }
        GameObject biomeDetailToUse = null;
        Sprite centerSpriteToUse = null;
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
            case BIOMES.WOODLAND:
                centerSpriteToUse = woodlandTrees[Random.Range(0, woodlandTrees.Length)];
                tile.SetCenterSprite(centerSpriteToUse);
                Utilities.SetSpriteSortingLayer(tile.centerPiece.GetComponent<SpriteRenderer>(), "Structures Layer");
                break;
            case BIOMES.FOREST:
                centerSpriteToUse = forestTrees[Random.Range(0, forestTrees.Length)];
                tile.SetCenterSprite(centerSpriteToUse);
                Utilities.SetSpriteSortingLayer(tile.centerPiece.GetComponent<SpriteRenderer>(), "Structures Layer");
                break;
        }
        if (biomeDetailToUse != null) {
            if (tile.specialResource == RESOURCE.NONE) {
                tile.AddBiomeDetailToTile(biomeDetailToUse);
            }
        }

        tile.UpdateSortingOrder();
    }

    internal void SetElevationSpriteForTile(HexTile currentHexTile) {
        int sortingOrder = currentHexTile.xCoordinate - currentHexTile.yCoordinate;
        if(currentHexTile.elevationType == ELEVATION.WATER) {
            Sprite waterSpriteToUse = waterTiles[Random.Range(0, waterTiles.Length)];
            currentHexTile.GetComponent<SpriteRenderer>().sortingLayerName = "Water";
            currentHexTile.GetComponent<SpriteRenderer>().sprite = waterSpriteToUse;
            currentHexTile.centerPiece.SetActive(false);
            return;
        }
        switch (currentHexTile.biomeType) {
            case BIOMES.SNOW:
                if (currentHexTile.elevationType == ELEVATION.MOUNTAIN) {
                    Sprite mountainSpriteToUse = snowAndTundraMountainTiles[Random.Range(0, snowAndTundraMountainTiles.Length)];
                    currentHexTile.SetCenterSprite(mountainSpriteToUse);
                    Utilities.SetSpriteSortingLayer(currentHexTile.centerPiece.GetComponent<SpriteRenderer>(), "TileDetails");
                }
                break;
            case BIOMES.TUNDRA:
                if (currentHexTile.elevationType == ELEVATION.MOUNTAIN) {
                    Sprite mountainSpriteToUse = snowAndTundraMountainTiles[Random.Range(0, snowAndTundraMountainTiles.Length)];
                    currentHexTile.SetCenterSprite(mountainSpriteToUse);
                    Utilities.SetSpriteSortingLayer(currentHexTile.centerPiece.GetComponent<SpriteRenderer>(), "TileDetails");
                }
                break;
            case BIOMES.DESERT:
                if (currentHexTile.elevationType == ELEVATION.MOUNTAIN) {
                    Sprite mountainSpriteToUse = desertMountainTiles[Random.Range(0, desertMountainTiles.Length)];
                    currentHexTile.SetCenterSprite(mountainSpriteToUse);
                    Utilities.SetSpriteSortingLayer(currentHexTile.centerPiece.GetComponent<SpriteRenderer>(), "TileDetails");
                }
                break;
            case BIOMES.GRASSLAND:
                if (currentHexTile.elevationType == ELEVATION.MOUNTAIN) {
                    Sprite mountainSpriteToUse = greenMountainTiles[Random.Range(0, greenMountainTiles.Length)];
                    currentHexTile.SetCenterSprite(mountainSpriteToUse);
                    Utilities.SetSpriteSortingLayer(currentHexTile.centerPiece.GetComponent<SpriteRenderer>(), "TileDetails");
                }
                break;
            case BIOMES.WOODLAND:
                if (currentHexTile.elevationType == ELEVATION.MOUNTAIN) {
                    Sprite mountainSpriteToUse = greenMountainTiles[Random.Range(0, greenMountainTiles.Length)];
                    currentHexTile.SetCenterSprite(mountainSpriteToUse);
                    Utilities.SetSpriteSortingLayer(currentHexTile.centerPiece.GetComponent<SpriteRenderer>(), "TileDetails");
                }
                break;
            case BIOMES.FOREST:
                if (currentHexTile.elevationType == ELEVATION.MOUNTAIN) {
                    Sprite mountainSpriteToUse = greenMountainTiles[Random.Range(0, greenMountainTiles.Length)];
                    currentHexTile.SetCenterSprite(mountainSpriteToUse);
                    Utilities.SetSpriteSortingLayer(currentHexTile.centerPiece.GetComponent<SpriteRenderer>(), "TileDetails");
                }
                break;
        }
        currentHexTile.UpdateSortingOrder();
        if (currentHexTile.elevationType == ELEVATION.MOUNTAIN) {
            currentHexTile.movementDays = 3;
        }
    }
	internal void GenerateSpecialResources(){
		for (int i = 0; i < GridMap.Instance.listHexes.Count; i++) {
//			GameObject currentHexTileGO = GridMap.Instance.listHexes [i];
			HexTile currentHexTile = GridMap.Instance.listHexes [i].GetComponent<HexTile> ();
			currentHexTile.AssignSpecialResource();
		}
	}
	internal void DeactivateCenterPieces(){
		for (int i = 0; i < GridMap.Instance.listHexes.Count; i++) {
//			GameObject currentHexTileGO = GridMap.Instance.listHexes [i];
			HexTile currentHexTile = GridMap.Instance.listHexes [i].GetComponent<HexTile> ();
			currentHexTile.DeactivateCenterPiece();
		}
	}
	internal void GenerateElevation(){
		CalculateElevationAndMoisture();
	}
    /*
     * Generate elavation for each of
     * the regions border tiles.
     * */
    internal void GenerateRegionBorderElevation() {
        List<HexTile> checkedTiles = new List<HexTile>();
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];

            for (int j = 0; j < currRegion.outerTiles.Count; j++) {
                HexTile currBorderTile = currRegion.outerTiles[j];
                if (currBorderTile.elevationType != ELEVATION.PLAIN || checkedTiles.Contains(currBorderTile)) {
                    //The current border tile already has an elevation type
                    //that is not plain, skip it.
                    continue;
                }
                //Get Tiles in batch
                List<HexTile> tilesInBatch = new List<HexTile>();
                tilesInBatch.Add(currBorderTile);
                for (int k = 0; k < tilesInBatch.Count; k++) {
                    HexTile currTile = tilesInBatch[k];
                    for (int l = 0; l < currTile.AllNeighbours.Count; l++) {
                        HexTile currNeighbour = currTile.AllNeighbours[l];
                        if(currRegion.outerTiles.Contains(currNeighbour) && currNeighbour.roadType != ROAD_TYPE.MAJOR) {
                            if (!tilesInBatch.Contains(currNeighbour)) {
                                tilesInBatch.Add(currNeighbour);
                            }
                        }
                    }
                }

                ELEVATION elevationToUse = ELEVATION.PLAIN;


                //Dictionary<ELEVATION, int> elevations = new Dictionary<ELEVATION, int>();
                //List<HexTile> tilesToCheck = new List<HexTile>(tilesInBatch);
                //tilesInBatch.ForEach(x => tilesToCheck.AddRange(x.AllNeighbours.Where(y => y.roadType != ROAD_TYPE.MAJOR && !tilesInBatch.Contains(y) && y.region.outerTiles.Contains(y))));
                ////Check if the tiles in the batch have any elevation types
                //for (int k = 0; k < tilesToCheck.Count; k++) {
                //    HexTile tileToCheck = tilesToCheck[k];
                //    if(tileToCheck.elevationType != ELEVATION.PLAIN) {
                //        if (elevations.ContainsKey(tileToCheck.elevationType)) {
                //            elevations[tileToCheck.elevationType]++;
                //        } else {
                //            elevations.Add(tileToCheck.elevationType, 1);
                //        }
                //    }
                //}

                //if(elevations.Count > 0) {
                //    elevationToUse = elevations.Keys.First();
                //    if (elevations.Count > 1) {
                //        Debug.LogWarning("There is more than one elevation type in this batch!");
                //    }
                //}

                //if (Random.Range(0, 2) == 0) {
                    elevationToUse = ELEVATION.MOUNTAIN;
                //} else {
                //    elevationToUse = ELEVATION.WATER;
                //}

                for (int k = 0; k < tilesInBatch.Count; k++) {
                    HexTile tile = tilesInBatch[k];
                    checkedTiles.Add(tile);
                    for (int l = 0; l < tile.AllNeighbours.Count; l++) {
                        HexTile currNeighbour = tile.AllNeighbours[l];
                        if(currNeighbour.roadType != ROAD_TYPE.MAJOR && currNeighbour.region.outerTiles.Contains(currNeighbour) && currNeighbour.region.id != tile.region.id) {
                            //if(currNeighbour.AllNeighbours.Where(x => x.region.id != tile.region.id).Count() < 3) {
                            currNeighbour.SetElevation(ELEVATION.PLAIN);
                            if (!checkedTiles.Contains(currNeighbour)) {
                                    checkedTiles.Add(currNeighbour);
                            }
                            //}
                        }
                    }
                    tile.SetElevation(elevationToUse);
                }
                
            }
        }
    }
    internal void GenerateElevationAfterRoads() {
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            for (int j = 0; j < currRegion.adjacentRegions.Count; j++) {
                Region adjacentRegion = currRegion.adjacentRegions[j];
                if (!currRegion.connections.Contains(adjacentRegion)) {
                    //currRegion and adjacentRegion are not connected
                    //Get tiles that are adjacent to to adjacentRegion
                    List<HexTile> adjacentTiles = currRegion.GetTilesAdjacentOnlyTo(adjacentRegion);
                    for (int k = 0; k < adjacentTiles.Count; k++) {
                        HexTile adjacentTile = adjacentTiles[k];
                        int tilesToGet = 0;
                        if(Random.Range(0, 100) < 3) {
                            tilesToGet = 1;
                        }
                        List<HexTile> tilesToSetElevation = new List<HexTile>();
                        if (tilesToGet > 0) {
                            tilesToSetElevation.AddRange((adjacentTile.GetTilesInRange(tilesToGet)
                            .Where(x => currRegion.tilesInRegion.Contains(x))));
                        }
                        tilesToSetElevation.Add(adjacentTile);
                        for (int l = 0; l < tilesToSetElevation.Count; l++) {
                            HexTile currTile = tilesToSetElevation[l];
                            if(adjacentTiles.Contains(currTile) && currTile.id != adjacentTile.id) {
                                continue;
                            }
                            
                            if (currTile.hasLandmark || currTile.isRoad) {
                                currTile.SetElevation(ELEVATION.MOUNTAIN);
                            } else {
                                int neighbourWaterTiles = currTile.AllNeighbours.Where(x => x.elevationType == ELEVATION.WATER).Count();
                                int neighbourMountainTiles = currTile.AllNeighbours.Where(x => x.elevationType == ELEVATION.MOUNTAIN).Count();
                                if (neighbourMountainTiles > neighbourWaterTiles) {
                                    currTile.SetElevation(ELEVATION.MOUNTAIN);
                                } else if (neighbourWaterTiles > neighbourMountainTiles) {
                                    currTile.SetElevation(ELEVATION.WATER);
                                } else {
                                    if (Random.Range(0, 2) == 0) {
                                        currTile.SetElevation(ELEVATION.WATER);
                                    } else {
                                        currTile.SetElevation(ELEVATION.MOUNTAIN);
                                    }
                                }
                            }
                            SetElevationSpriteForTile(currTile);
                        }
                    }
                }
            }
        }
    }

	private void CalculateElevationAndMoisture(){
        float elevationFrequency = 10f; //14.93f;//2.66f;
        float moistureFrequency = 3.34f; //3.34f;//2.94f;
		float tempFrequency = 2.64f;//2.4f;

		float elevationRand = UnityEngine.Random.Range(500f,2000f);
		float moistureRand = UnityEngine.Random.Range(500f,2000f);
		float temperatureRand = UnityEngine.Random.Range(500f,2000f);

		string[] splittedNameEq = EquatorGenerator.Instance.listEquator[0].name.Split(new char[]{','});
		int equatorY = int.Parse (splittedNameEq [1]);

		for(int i = 0; i < GridMap.Instance.listHexes.Count; i++){
            HexTile currTile = GridMap.Instance.listHexes[i].GetComponent<HexTile>();

            string[] splittedName = GridMap.Instance.listHexes[i].name.Split(new char[]{','});
			int[] xy = {int.Parse(splittedName[0]), int.Parse(splittedName[1])};

			float nx = ((float)xy[0]/GridMap.Instance.width);
			float ny = ((float)xy[1]/GridMap.Instance.height);

			float elevationNoise = Mathf.PerlinNoise((nx + elevationRand) * elevationFrequency, (ny + elevationRand) * elevationFrequency);
			ELEVATION elevationType = GetElevationType(elevationNoise);

            currTile.elevationNoise = elevationNoise;
            currTile.SetElevation (elevationType);
            currTile.moistureNoise = Mathf.PerlinNoise((nx + moistureRand) * moistureFrequency, (ny + moistureRand) * moistureFrequency);

			int distanceToEquator = Mathf.Abs (xy [1] - equatorY);
			float tempGradient = 1.23f / GridMap.Instance.height;
			GridMap.Instance.listHexes [i].GetComponent<HexTile>().temperature = distanceToEquator * tempGradient;
			GridMap.Instance.listHexes[i].GetComponent<HexTile>().temperature += (Mathf.PerlinNoise((nx + temperatureRand) * tempFrequency, (ny + temperatureRand) * tempFrequency)) * 0.6f;
		}
	}

	private ELEVATION GetElevationType(float elevationNoise){
        //return ELEVATION.PLAIN;
        if (elevationNoise <= 0.25f) {
            return ELEVATION.WATER;
        } else if (elevationNoise > 0.25f && elevationNoise <= 0.7f) {
            return ELEVATION.PLAIN;
        } else {
            return ELEVATION.MOUNTAIN;
        }
    }

	private BIOMES GetBiomeSimple(GameObject goHex){
		float moistureNoise = goHex.GetComponent<HexTile>().moistureNoise;
		float temperature = goHex.GetComponent<HexTile>().temperature;

		if(temperature <= 0.4f) {
			if(moistureNoise <= 0.45f){
				return BIOMES.DESERT;
			}else if(moistureNoise > 0.45f && moistureNoise <= 0.65f){
				return BIOMES.GRASSLAND;
			}else if(moistureNoise > 0.65f){
				return BIOMES.WOODLAND;
			}	

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
			if(moistureNoise <= 0.45f){
				return BIOMES.GRASSLAND;
			}else if(moistureNoise > 0.45f && moistureNoise <= 0.55f){
				return BIOMES.WOODLAND;
			}else if(moistureNoise > 0.55f){
				return BIOMES.FOREST;
			}			
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
					
					currentHexTile.movementDays = 2;
					currentHexTile.biomeType = BIOMES.BARE;
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
    [ContextMenu("Generate Tags")]
    public void GenerateTileTags() {
        List<HexTile> tilesToTag = new List<HexTile>(GridMap.Instance.listHexes.Select(x => x.GetComponent<HexTile>()));
        int currTag = 0;
        Queue<HexTile> tagQueue = new Queue<HexTile>();
        HexTile firstTile = null;
        //tagQueue.Enqueue(firstTile);

        ELEVATION currElevation = ELEVATION.PLAIN;

        while (tilesToTag.Count != 0) {
            if(tagQueue.Count <= 0) {
                //move on to other tag
                currTag++;
                firstTile = tilesToTag.FirstOrDefault();
                firstTile.SetTag(currTag);
                tilesToTag.Remove(firstTile);
                tagQueue.Enqueue(firstTile);
                currElevation = firstTile.elevationType;
            }

            HexTile parentTile = tagQueue.Dequeue();
            
			List<HexTile> parentTileNeighbours = new List<HexTile>(parentTile.AllNeighbours);
            for (int i = 0; i < parentTileNeighbours.Count; i++) {
                HexTile currNeighbour = parentTileNeighbours[i];
                if(tilesToTag.Contains(currNeighbour) && 
                    (currNeighbour.elevationType == currElevation 
                    || (currNeighbour.elevationType == ELEVATION.MOUNTAIN && currElevation == ELEVATION.PLAIN) 
                    || currNeighbour.elevationType == ELEVATION.PLAIN && currElevation == ELEVATION.MOUNTAIN)) {
                    currNeighbour.SetTag(currTag);
                    tilesToTag.Remove(currNeighbour);
                    tagQueue.Enqueue(currNeighbour);
                }
            }
        }
    }

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

    internal Sprite GetTextureForBiome(BIOMES biomeType) {
        if (biomeType == BIOMES.GRASSLAND) {
            return grasslandTexture;
        } else if (biomeType == BIOMES.WOODLAND) {
            return grasslandTexture;
        } else if (biomeType == BIOMES.TUNDRA) {
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
       
    internal GameObject GetPrefabForResource(RESOURCE resource) {
        switch (resource) {
            case RESOURCE.CORN:
                return cornPrefab;
            case RESOURCE.WHEAT:
                return wheatPrefab;
            case RESOURCE.RICE:
                return ricePrefab;
            case RESOURCE.DEER:
                return deerPrefab;
            case RESOURCE.PIG:
                return pigPrefab;
            case RESOURCE.BEHEMOTH:
                return behemothPrefab;
            case RESOURCE.OAK:
                return oakPrefab;
            case RESOURCE.EBONY:
                return ebonyPrefab;
            case RESOURCE.GRANITE:
                return granitePrefab;
            case RESOURCE.SLATE:
                return slatePrefab;
            case RESOURCE.MANA_STONE:
                return manaStonesPrefab;
            case RESOURCE.MITHRIL:
                return mithrilPrefab;
            case RESOURCE.COBALT:
                return cobaltPrefab;
            default:
                return null;
        }
    }
}