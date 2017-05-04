using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Biomes : MonoBehaviour {
	public static Biomes Instance;

	public float initialTemperature;
	public float initialTemperature2;
	public float intervalTemperature;
	[Space(10)]
	public float temperature;
	[Space(10)]
	public int[] hexInterval;
	public float[] temperatureInterval;
	[Space(10)]
	public Sprite forestSprite;
	public Sprite grasslandSprite;
	public Sprite woodlandSprite;
	public Sprite desertSprite;
	public Sprite tundraSprite;
	public Sprite snowSprite;
	public Sprite waterSprite;
	[Space(10)]
	public Sprite forestLeft;
	public Sprite forestRight;
	public Sprite forestLeftCorner;
	public Sprite forestRightCorner;
	public Sprite forestTopLeftCorner;
	public Sprite forestTopRightCorner;
	public Sprite forestCenter;
	public Sprite forestCenter2;
	public Sprite forestCenter3;
	[Space(10)]
	public Sprite grasslandLeft;
	public Sprite grasslandRight;
	public Sprite grasslandTopLeftCorner;
	public Sprite grasslandTopRightCorner;
	public Sprite grasslandLeftCorner;
	public Sprite grasslandRightCorner;
	public Sprite grasslandCenter;
	public Sprite grasslandCenter2;
	[Space(10)]
	public Sprite woodlandLeft;
	public Sprite woodlandRight;
	public Sprite woodlandTopLeftCorner;
	public Sprite woodlandTopRightCorner;
	public Sprite woodlandLeftCorner;
	public Sprite woodlandRightCorner;
	public Sprite woodlandCenter;
	public Sprite woodlandCenter2;
	public Sprite woodlandCenter3;
	[Space(10)]
	public Sprite desertLeft;
	public Sprite desertRight;
	public Sprite desertTopLeftCorner;
	public Sprite desertTopRightCorner;
	public Sprite desertLeftCorner;
	public Sprite desertRightCorner;
	public Sprite desertCenter;
	public Sprite desertCenter2;
	public Sprite desertCenter3;
	public Sprite desertCenter4;
	[Space(10)]
	public Sprite tundraLeft;
	public Sprite tundraRight;
	public Sprite tundraTopLeftCorner;
	public Sprite tundraTopRightCorner;
	public Sprite tundraLeftCorner;
	public Sprite tundraRightCorner;
	public Sprite tundraCenter;
	public Sprite tundraCenter2;
	public Sprite tundraCenter3;
	[Space(10)]
	public Sprite snowLeft;
	public Sprite snowRight;
	public Sprite snowTopLeftCorner;
	public Sprite snowTopRightCorner;
	public Sprite snowLeftCorner;
	public Sprite snowRightCorner;
	public Sprite snowCenter;
	public Sprite snowCenter2;
	[Space(10)]
	public Sprite mountainCenter;

	[Space(10)]
	public Sprite[] grasslandTiles;
	public Sprite[] forestTiles;
	public Sprite[] woodlandTiles;
	public Sprite[] desertTiles;
	public Sprite[] tundraTiles;
	public Sprite[] waterTiles;
	public Sprite[] snowTiles;
	public Sprite[] mountainTiles;

	[Space(10)]
	public List<HexTile> snowHexTiles;
	public List<HexTile> tundraHexTiles;
	public List<HexTile> grasslandHexTiles;
	public List<HexTile> woodsHexTiles;
	public List<HexTile> forestHexTiles;
	public List<HexTile> desertHexTiles;

	void Awake(){
		Instance = this;
	}

	internal void GenerateBiome(){
		//CalculateNewTemperature();
		for(int i = 0; i < GridMap.Instance.listHexes.Count; i++){
			GameObject currentHexTileGO = GridMap.Instance.listHexes[i];
			HexTile currentHexTile = GridMap.Instance.listHexes[i].GetComponent<HexTile>();
			currentHexTile.biomeType = GetBiomeSimple(currentHexTileGO);
			currentHexTile.AssignDefaultResource ();
			currentHexTile.AssignSpecialResource ();
			if(currentHexTile.elevationType == ELEVATION.WATER){
				continue;
			}
//			AssignHexTileToList (currentHexTile);
			switch(currentHexTile.biomeType){
			case BIOMES.SNOW:
				currentHexTile.movementDays = 1;
				Sprite snowSpriteToUse = snowTiles [Random.Range (0, snowTiles.Length)];
				currentHexTile.SetTileSprites (snowSpriteToUse, snowLeft, snowRight, snowTopLeftCorner, snowTopRightCorner, snowLeftCorner, snowRightCorner, new Sprite[]{snowCenter, snowCenter2});
//				currentHexTile.SetTileSprites (snowSprite, snowLeft, snowRight, snowTopLeftCorner, snowTopRightCorner, snowLeftCorner, snowRightCorner, new Sprite[]{snowCenter, snowCenter2});
				break;
			case BIOMES.TUNDRA:
				currentHexTile.movementDays = 1;
				Sprite tundraSpriteToUse = tundraTiles [Random.Range (0, tundraTiles.Length)];
				currentHexTile.SetTileSprites (tundraSpriteToUse, tundraLeft, tundraRight, tundraTopLeftCorner, tundraTopRightCorner, tundraLeftCorner, tundraRightCorner,  new Sprite[]{tundraCenter, tundraCenter2, tundraCenter3});
//				currentHexTile.SetTileSprites (tundraSprite, tundraLeft, tundraRight, tundraTopLeftCorner, tundraTopRightCorner, tundraLeftCorner, tundraRightCorner,  new Sprite[]{tundraCenter, tundraCenter2, tundraCenter3});
				break;
			case BIOMES.DESERT:
				currentHexTile.movementDays = 1;
				Sprite desertSpriteToUse = desertTiles [Random.Range (0, desertTiles.Length)];
				currentHexTile.SetTileSprites (desertSpriteToUse, desertLeft, desertRight, desertTopLeftCorner, desertTopRightCorner, desertLeftCorner, desertRightCorner,  new Sprite[]{desertCenter, desertCenter2, desertCenter3, desertCenter4});
//				currentHexTile.SetTileSprites (desertSprite, desertLeft, desertRight, desertTopLeftCorner, desertTopRightCorner, desertLeftCorner, desertRightCorner,  new Sprite[]{desertCenter, desertCenter2, desertCenter3, desertCenter4});
				break;
			case BIOMES.GRASSLAND:
				currentHexTile.movementDays = 1;
				Sprite grasslandSpriteToUse = grasslandTiles [Random.Range (0, grasslandTiles.Length)];
				currentHexTile.SetTileSprites (grasslandSpriteToUse, grasslandLeft, grasslandRight, grasslandTopLeftCorner, grasslandTopRightCorner, grasslandLeftCorner, grasslandRightCorner, new Sprite[]{grasslandCenter, grasslandCenter2});
//				currentHexTile.SetTileSprites (grasslandSprite, grasslandLeft, grasslandRight, grasslandTopLeftCorner, grasslandTopRightCorner, grasslandLeftCorner, grasslandRightCorner, new Sprite[]{grasslandCenter, grasslandCenter2});
				break;
			case BIOMES.WOODLAND:
				currentHexTile.movementDays = 1;
				Sprite woodlandSpriteToUse = woodlandTiles [Random.Range (0, woodlandTiles.Length)];
				currentHexTile.SetTileSprites (woodlandSpriteToUse, woodlandLeft, woodlandRight, woodlandTopLeftCorner, woodlandTopRightCorner, woodlandLeftCorner, woodlandRightCorner, new Sprite[]{woodlandCenter, woodlandCenter2, woodlandCenter3});
//				currentHexTile.SetTileSprites (woodlandSprite, woodlandLeft, woodlandRight, woodlandTopLeftCorner, woodlandTopRightCorner, woodlandLeftCorner, woodlandRightCorner, new Sprite[]{woodlandCenter, woodlandCenter2, woodlandCenter3});
				break;
			case BIOMES.FOREST:
				currentHexTile.movementDays = 1;
				Sprite forestSpriteToUse = forestTiles [Random.Range (0, forestTiles.Length)];
				currentHexTile.SetTileSprites (forestSpriteToUse, forestLeft, forestRight, forestTopLeftCorner, forestTopRightCorner, forestLeftCorner, forestRightCorner, new Sprite[]{forestCenter, forestCenter2, forestCenter3});
//				currentHexTile.SetTileSprites (forestSprite, forestLeft, forestRight, forestTopLeftCorner, forestTopRightCorner, forestLeftCorner, forestRightCorner, new Sprite[]{forestCenter, forestCenter2, forestCenter3});
				break;
			}
		}
		GenerateBareBiome();

	}

	internal void GenerateElevation(){
		CalculateElevationAndMoisture();

		for(int i = 0; i < GridMap.Instance.listHexes.Count; i++){
			switch(GridMap.Instance.listHexes[i].GetComponent<HexTile>().elevationType){
			case ELEVATION.MOUNTAIN:
				Sprite mountainSpriteToUse = mountainTiles [Random.Range (0, mountainTiles.Length)];
//				GridMap.Instance.listHexes[i].GetComponent<SpriteRenderer>().color = new Color(165f/255f,42f/255f,42f/255f);
				GridMap.Instance.listHexes[i].GetComponent<HexTile>().centerPiece.GetComponent<SpriteRenderer>().sprite = mountainSpriteToUse;
//				GridMap.Instance.listHexes[i].GetComponent<HexTile>().GetComponent<SpriteRenderer>().sprite = mountainSpriteToUse;
				break;
			case ELEVATION.PLAIN:
//				GridMap.Instance.listHexes[i].GetComponent<SpriteRenderer>().color = Color.green;
				break;
			case ELEVATION.WATER:
//				GridMap.Instance.listHexes[i].GetComponent<SpriteRenderer>().color = Color.blue;
				Sprite waterSpriteToUse = waterTiles [Random.Range (0, waterTiles.Length)];
				GridMap.Instance.listHexes[i].GetComponent<SpriteRenderer>().sprite = waterSpriteToUse;
//				GridMap.Instance.listHexes[i].GetComponent<SpriteRenderer>().sprite = waterSprite;
				break;
			}
		}
	}

	private void CalculateElevationAndMoisture(){
		float elevationFrequency = 2.66f;
		float moistureFrequency = 2.94f;
		float tempFrequency = 2.4f;

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
			float tempGradient = 1.2f / GridMap.Instance.height;
			GridMap.Instance.listHexes [i].GetComponent<HexTile>().temperature = distanceToEquator * tempGradient;
			GridMap.Instance.listHexes[i].GetComponent<HexTile>().temperature += (Mathf.PerlinNoise((nx + temperatureRand) * tempFrequency, (ny + temperatureRand) * tempFrequency)) * 0.6f;
		}
	}

	private ELEVATION GetElevationType(float elevationNoise){
		if(elevationNoise <= 0.30f){
			return ELEVATION.WATER;
		}else if(elevationNoise > 0.30f && elevationNoise <= 0.60f){
			return ELEVATION.PLAIN;
		}else{
			return ELEVATION.MOUNTAIN;
		}
	}

	private BIOMES GetBiomeSimple(GameObject goHex){
		float moistureNoise = goHex.GetComponent<HexTile>().moistureNoise;
		float temperature = goHex.GetComponent<HexTile>().temperature;

		if(temperature <= 0.35f) {
			if(moistureNoise <= 0.20f){
				return BIOMES.DESERT;
			}else if(moistureNoise > 0.20f && moistureNoise <= 0.40f){
				return BIOMES.GRASSLAND;
			}else if(moistureNoise > 0.40f && moistureNoise <= 0.55f){
				return BIOMES.WOODLAND;
			}else if(moistureNoise > 0.55f){
				return BIOMES.FOREST;
			}
		} else if(temperature > 0.35f && temperature <= 0.65f){
			if(moistureNoise <= 0.20f){
				return BIOMES.DESERT;
			}else if(moistureNoise > 0.20f && moistureNoise <= 0.55f){
				return BIOMES.GRASSLAND;
			}else if(moistureNoise > 0.55f && moistureNoise <= 0.75f){
				return BIOMES.WOODLAND;
			}else if(moistureNoise > 0.75f){
				return BIOMES.FOREST;
			}
		} else if(temperature > 0.65f && temperature <= 0.85f){
			if(moistureNoise <= 0.2f){
				return BIOMES.TUNDRA;
			}else if(moistureNoise > 0.2f && moistureNoise <= 0.55f){
				return BIOMES.GRASSLAND;
			}else if(moistureNoise > 0.55f && moistureNoise <= 0.75f){
				return BIOMES.WOODLAND;
			}else if(moistureNoise > 0.75f){
				return BIOMES.SNOW;
			}
		} else if(temperature > 0.85f){
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
			ELEVATION elevationType = GridMap.Instance.listHexes[i].GetComponent<HexTile>().elevationType;
			float moisture = GridMap.Instance.listHexes[i].GetComponent<HexTile>().moistureNoise;

			if(elevationType == ELEVATION.WATER){
				if(moisture <= 0.2f){
					GridMap.Instance.listHexes[i].GetComponent<HexTile>().movementDays = 1;
					GridMap.Instance.listHexes[i].GetComponent<HexTile>().biomeType = BIOMES.BARE;
					GridMap.Instance.listHexes[i].GetComponent<SpriteRenderer>().color = new Color(186f/255f, 154f/255f, 154f/255f);
				}
			}
		}
	}
	/*
	 * Place each hexagon tile in their respective
	 * biome lists for easy access to each specific biomes
	 * tiles.
	 * */
//	void AssignHexTileToList(HexTile tile){
//		BIOMES biomeType = tile.biomeType;
//		switch (biomeType) {
//		case BIOMES.SNOW:
//			snowHexTiles.Add(tile);
//			CityGenerator.Instance.tilesElligibleForCities.Add(tile);
//			break;
//		case BIOMES.TUNDRA:
//			tundraHexTiles.Add(tile);
//			CityGenerator.Instance.tilesElligibleForCities.Add(tile);
//			break;
//		case BIOMES.GRASSLAND:
//			grasslandHexTiles.Add(tile);
//			CityGenerator.Instance.tilesElligibleForCities.Add(tile);
//			break;
//		case BIOMES.WOODLAND:
//			woodsHexTiles.Add(tile);
//			CityGenerator.Instance.tilesElligibleForCities.Add(tile);
//			break;
//		case BIOMES.FOREST:
//			forestHexTiles.Add(tile);
//			CityGenerator.Instance.tilesElligibleForCities.Add(tile);
//			break;
//		case BIOMES.DESERT:
//			desertHexTiles.Add(tile);
//			CityGenerator.Instance.tilesElligibleForCities.Add(tile);
//			break;
//		}
//	}

	public void GenerateTileDetails(){
		HexTile currentHexTile = null;
		for (int i = 0; i < GridMap.Instance.listHexes.Count; i++) {
			currentHexTile = GridMap.Instance.listHexes [i].GetComponent<HexTile>();
			if (currentHexTile.elevationType != ELEVATION.WATER) {
//				currentHexTile.GenerateTileDetails ();
			}
		}
	}
}