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
            //currentHexTile.SetPassableState(false);
        }
		//GenerateBareBiome();
	}
    internal void LoadPassableObjects() {
        for (int i = 0; i < GridMap.Instance.hexTiles.Count; i++) {
            HexTile currentHexTile = GridMap.Instance.hexTiles[i];
            object centerObj = GetCenterObject(currentHexTile);
            currentHexTile.SetPassableObject(centerObj);
            if (currentHexTile.elevationType == ELEVATION.PLAIN) {
                currentHexTile.SetPassableState(true);
            } else {
                currentHexTile.SetPassableState(false);
            }
            currentHexTile.UpdateSortingOrder();
        }
    }
    internal void LoadElevationSprites() {
        for (int i = 0; i < GridMap.Instance.hexTiles.Count; i++) {
            HexTile currTile = GridMap.Instance.hexTiles[i];
            SetElevationSpriteForTile(currTile);
            currTile.UpdateLedgesAndOutlines();
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

    internal void GenerateTileBiomeDetails() {
        for (int i = 0; i < GridMap.Instance.hexTiles.Count; i++) {
            HexTile currentHexTile = GridMap.Instance.hexTiles[i].GetComponent<HexTile>();
            //if(currentHexTile.elevationType != ELEVATION.PLAIN) {
            //    continue;
            //}
            //if (currentHexTile.biomeType == BIOMES.FOREST) {
            //    continue;
            //}
            AddBiomeDetailToTile(currentHexTile);
        }
    }

    internal void AddBiomeDetailToTile(HexTile tile) {
        if(tile.elevationType != ELEVATION.PLAIN) {
            return;
        }
        GameObject biomeDetailToUse = null;
        //Sprite centerSpriteToUse = null;
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
        if (biomeDetailToUse != null) {
            tile.AddBiomeDetailToTile(biomeDetailToUse);
        }

        tile.UpdateSortingOrder();
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
        currentHexTile.UpdateSortingOrder();
    }
	internal void DeactivateCenterPieces(){
		for (int i = 0; i < GridMap.Instance.listHexes.Count; i++) {
			HexTile currentHexTile = GridMap.Instance.listHexes [i].GetComponent<HexTile> ();
			currentHexTile.DeactivateCenterPiece();
		}
	}
	internal void GenerateElevation(){
		CalculateElevationAndMoisture();
	}
	private void CalculateElevationAndMoisture(){
        float elevationFrequency = 19.1f; //14.93f;//2.66f;
        float moistureFrequency = 12.34f; //3.34f;//2.94f;
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
            //case ELEVATION.PLAIN:
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