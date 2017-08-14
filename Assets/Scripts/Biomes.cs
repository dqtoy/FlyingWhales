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
            GameObject biomeDetailToUse = null;
            currentHexTile.SetBiome(GetBiomeSimple(currentHexTileGO));
//			currentHexTile.AssignDefaultResource ();
			//currentHexTile.AssignSpecialResource ();
			if(currentHexTile.elevationType == ELEVATION.WATER){
				continue;
			}
//			AssignHexTileToList (currentHexTile);
			int sortingOrder = currentHexTile.xCoordinate - currentHexTile.yCoordinate;
			switch(currentHexTile.biomeType){
			case BIOMES.SNOW:
				currentHexTile.movementDays = 1;//3;
				Sprite snowSpriteToUse = snowTiles [Random.Range (0, snowTiles.Length)];
				currentHexTile.SetBaseSprite(snowSpriteToUse);
				if (currentHexTile.elevationType == ELEVATION.MOUNTAIN) {
					Sprite mountainSpriteToUse = snowAndTundraMountainTiles [Random.Range (0, snowAndTundraMountainTiles.Length)];
					currentHexTile.SetCenterSprite (mountainSpriteToUse);
					Utilities.SetSpriteSortingLayer (currentHexTile.centerPiece.GetComponent<SpriteRenderer> (), "TileDetails");
				} else {
                    if(snowDetails.Length > 0) {
                        biomeDetailToUse = snowDetails[Random.Range(0, snowDetails.Length)];
                    }
                    sortingOrder += 6;
				}
				break;
			case BIOMES.TUNDRA:
				currentHexTile.movementDays = 1;//2;
				Sprite tundraSpriteToUse = tundraTiles [Random.Range (0, tundraTiles.Length)];
				currentHexTile.SetBaseSprite(tundraSpriteToUse);
				if (currentHexTile.elevationType == ELEVATION.MOUNTAIN) {
					Sprite mountainSpriteToUse = snowAndTundraMountainTiles [Random.Range (0, snowAndTundraMountainTiles.Length)];
					currentHexTile.SetCenterSprite (mountainSpriteToUse);
					Utilities.SetSpriteSortingLayer (currentHexTile.centerPiece.GetComponent<SpriteRenderer> (), "TileDetails");
				} else {
                    if (tundraDetails.Length > 0) {
                        biomeDetailToUse = tundraDetails[Random.Range(0, tundraDetails.Length)];
                    }
                    sortingOrder += 3;
				}
				break;
			case BIOMES.DESERT:
				currentHexTile.movementDays = 2;//4;
				Sprite desertSpriteToUse = desertTiles [Random.Range (0, desertTiles.Length)];
				currentHexTile.SetBaseSprite(desertSpriteToUse);
				if (currentHexTile.elevationType == ELEVATION.MOUNTAIN) {
					Sprite mountainSpriteToUse = desertMountainTiles [Random.Range (0, desertMountainTiles.Length)];
					currentHexTile.SetCenterSprite (mountainSpriteToUse);
					Utilities.SetSpriteSortingLayer (currentHexTile.centerPiece.GetComponent<SpriteRenderer> (), "TileDetails");
				} else {
                    if (desertDetails.Length > 0) {
                        biomeDetailToUse = desertDetails[Random.Range(0, desertDetails.Length)];
                    }
                    sortingOrder += 5;
				}
				break;
			case BIOMES.GRASSLAND:
				currentHexTile.movementDays = 1;//2;
				Sprite grasslandSpriteToUse = grasslandTiles [Random.Range (0, grasslandTiles.Length)];
				currentHexTile.SetBaseSprite(grasslandSpriteToUse);
				if (currentHexTile.elevationType == ELEVATION.MOUNTAIN) {
					Sprite mountainSpriteToUse = greenMountainTiles [Random.Range (0, greenMountainTiles.Length)];
					currentHexTile.SetCenterSprite(mountainSpriteToUse);
					Utilities.SetSpriteSortingLayer(currentHexTile.centerPiece.GetComponent<SpriteRenderer> (), "TileDetails");
				} else {
                    if (grasslandDetails.Length > 0) {
                        biomeDetailToUse = grasslandDetails[Random.Range(0, grasslandDetails.Length)];
                    }
                    sortingOrder += 1;
				}
				break;
			case BIOMES.WOODLAND:
				currentHexTile.movementDays = 1;//3;
				Sprite woodlandSpriteToUse = woodlandTiles [Random.Range (0, woodlandTiles.Length)];
				currentHexTile.SetBaseSprite(woodlandSpriteToUse);
				if (currentHexTile.elevationType == ELEVATION.MOUNTAIN) {
					Sprite mountainSpriteToUse = greenMountainTiles [Random.Range (0, greenMountainTiles.Length)];
					currentHexTile.SetCenterSprite (mountainSpriteToUse);
					Utilities.SetSpriteSortingLayer (currentHexTile.centerPiece.GetComponent<SpriteRenderer> (), "TileDetails");
				} else {                    
					Sprite centerSpriteToUse = woodlandTrees [Random.Range (0, woodlandTrees.Length)];
					currentHexTile.SetCenterSprite(centerSpriteToUse);
					Utilities.SetSpriteSortingLayer (currentHexTile.centerPiece.GetComponent<SpriteRenderer> (), "Structures Layer");
					sortingOrder += 2;
				}
				break;
			case BIOMES.FOREST:
				currentHexTile.movementDays = 2;
				Sprite forestSpriteToUse = forestTiles [Random.Range (0, forestTiles.Length)];
				currentHexTile.SetBaseSprite(forestSpriteToUse);
				if (currentHexTile.elevationType == ELEVATION.MOUNTAIN) {
					Sprite mountainSpriteToUse = greenMountainTiles [Random.Range (0, greenMountainTiles.Length)];
					currentHexTile.SetCenterSprite (mountainSpriteToUse);
					Utilities.SetSpriteSortingLayer (currentHexTile.centerPiece.GetComponent<SpriteRenderer> (), "TileDetails");
				} else {
					Sprite centerSpriteToUse = forestTrees [Random.Range (0, forestTrees.Length)];
					currentHexTile.SetCenterSprite(centerSpriteToUse);
					Utilities.SetSpriteSortingLayer (currentHexTile.centerPiece.GetComponent<SpriteRenderer> (), "Structures Layer");
					sortingOrder += 4;
				}
				break;
			}
            currentHexTile.AssignSpecialResource();
            if(biomeDetailToUse != null) {
                if (currentHexTile.specialResource == RESOURCE.NONE) {
                    currentHexTile.AddBiomeDetailToTile(biomeDetailToUse);
                }
            }
            
            currentHexTile.SetSortingOrder (sortingOrder);
            if (currentHexTile.elevationType == ELEVATION.MOUNTAIN) {
				currentHexTile.movementDays = 3;
			}

		}
		//GenerateBareBiome();

	}

	internal void GenerateElevation(){
		CalculateElevationAndMoisture();

		for(int i = 0; i < GridMap.Instance.listHexes.Count; i++){
			HexTile currHexTile = GridMap.Instance.listHexes[i].GetComponent<HexTile>();
			switch(currHexTile.elevationType){
			case ELEVATION.MOUNTAIN:
//				Sprite mountainSpriteToUse = mountainTiles [Random.Range (0, mountainTiles.Length)];
//				GridMap.Instance.listHexes[i].GetComponent<SpriteRenderer>().color = new Color(165f/255f,42f/255f,42f/255f);
//				GridMap.Instance.listHexes[i].GetComponent<HexTile>().centerPiece.GetComponent<SpriteRenderer>().sprite = mountainSpriteToUse;
//				GridMap.Instance.listHexes[i].GetComponent<HexTile>().GetComponent<SpriteRenderer>().sprite = mountainSpriteToUse;
				break;
			case ELEVATION.PLAIN:
//				GridMap.Instance.listHexes[i].GetComponent<SpriteRenderer>().color = Color.green;
				break;
			case ELEVATION.WATER:
//				GridMap.Instance.listHexes[i].GetComponent<SpriteRenderer>().color = Color.blue;
				Sprite waterSpriteToUse = waterTiles [Random.Range (0, waterTiles.Length)];
				currHexTile.GetComponent<SpriteRenderer>().sortingLayerName = "Water";
				currHexTile.GetComponent<SpriteRenderer>().sprite = waterSpriteToUse;
//				GridMap.Instance.listHexes[i].GetComponent<SpriteRenderer>().sprite = waterSprite;
				break;
			}
		}
	}

	private void CalculateElevationAndMoisture(){
		float elevationFrequency = 6.93f;//2.66f;
		float moistureFrequency = 3.34f;//2.94f;
		float tempFrequency = 2.64f;//2.4f;

		float elevationRand = UnityEngine.Random.Range(500f,2000f);
		float moistureRand = UnityEngine.Random.Range(500f,2000f);
		float temperatureRand = UnityEngine.Random.Range(500f,2000f);

		string[] splittedNameEq = EquatorGenerator.Instance.listEquator[0].name.Split(new char[]{','});
		int equatorY = int.Parse (splittedNameEq [1]);

		for(int i = 0; i < GridMap.Instance.listHexes.Count; i++){
			string[] splittedName = GridMap.Instance.listHexes[i].name.Split(new char[]{','});
			int[] xy = {int.Parse(splittedName[0]), int.Parse(splittedName[1])};

			float nx = ((float)xy[0]/GridMap.Instance.width);
			float ny = ((float)xy[1]/GridMap.Instance.height);

			float elevationNoise = Mathf.PerlinNoise((nx + elevationRand) * elevationFrequency, (ny + elevationRand) * elevationFrequency);
			ELEVATION elevationType = GetElevationType(elevationNoise);

			GridMap.Instance.listHexes[i].GetComponent<HexTile>().elevationNoise = elevationNoise;
			GridMap.Instance.listHexes[i].GetComponent<HexTile>().elevationType = elevationType;
			GridMap.Instance.listHexes[i].GetComponent<HexTile>().moistureNoise = Mathf.PerlinNoise((nx + moistureRand) * moistureFrequency, (ny + moistureRand) * moistureFrequency);

			int distanceToEquator = Mathf.Abs (xy [1] - equatorY);
			float tempGradient = 1.23f / GridMap.Instance.height;
			GridMap.Instance.listHexes [i].GetComponent<HexTile>().temperature = distanceToEquator * tempGradient;
			GridMap.Instance.listHexes[i].GetComponent<HexTile>().temperature += (Mathf.PerlinNoise((nx + temperatureRand) * tempFrequency, (ny + temperatureRand) * tempFrequency)) * 0.6f;
		}
	}

	private ELEVATION GetElevationType(float elevationNoise){
		if(elevationNoise <= 0.25f){
			return ELEVATION.WATER;
		}else if(elevationNoise > 0.25f && elevationNoise <= 0.7f){
			return ELEVATION.PLAIN;
		}else{
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

	internal void GenerateTileDetails(){
		for (int i = 0; i < GridMap.Instance.listHexes.Count; i++) {
			HexTile currHexTile = GridMap.Instance.listHexes[i].GetComponent<HexTile>();
			//if (currHexTile.elevationType != ELEVATION.WATER) {
                currHexTile.LoadEdges();
//                if (currHexTile.biomeType == BIOMES.GRASSLAND) {
//					currHexTile.LoadEdges();
//				} else if (currHexTile.biomeType == BIOMES.WOODLAND) {
//					currHexTile.LoadEdges();
//				} 
////				else if (currHexTile.biomeType == BIOMES.FOREST) {
////					currHexTile.LoadEdges(forestTexture, edgeMaterial);
////				} 
//				else if (currHexTile.biomeType == BIOMES.DESERT) {
//					currHexTile.LoadEdges();
//				} else if (currHexTile.biomeType == BIOMES.SNOW) {
//					currHexTile.LoadEdges(snowTexture, edgeMaterial);
//				}
//				else if (currHexTile.biomeType == BIOMES.TUNDRA) {
//					currHexTile.LoadEdges(tundraTexture, edgeMaterial);
//				}
			//}

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
            
            List<HexTile> parentTileNeighbours = parentTile.AllNeighbours.ToList();
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