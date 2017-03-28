using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathFind;
using System.Linq;

public class HexTile : MonoBehaviour,  IHasNeighbours<HexTile>{
	public int xCoordinate;
	public int yCoordinate;

	public string tileName;

	public float elevationNoise;
	public float moistureNoise;
	public float temperature;

	public RESOURCE defaultResource;
	public RESOURCE specialResource;

	public BIOMES biomeType;
	public ELEVATION elevationType;

	public City city;
	public Citizen occupant = null;
	public ROLE roleIntendedForTile = ROLE.UNTRAINED;

	public bool isHabitable = false;
	public bool isRoad = false;
	public bool isOccupied = false;
	public bool isOwned = false;
	public GameObject topLeft, topRight, right, bottomRight, bottomLeft, left;

	public GameObject leftGround;
	public GameObject bottomLeftGround;
	public GameObject rightGround;
	public GameObject bottomRightGround;
	public GameObject centerPiece;

	public GameObject leftBorder;
	public GameObject rightBorder;
	public GameObject topLeftBorder;
	public GameObject topRightBorder;
	public GameObject bottomLeftBorder;
	public GameObject bottomRightBorder;

	public GameObject resourceVisualGO;

	public List<HexTile> connectedTiles = new List<HexTile>();

	public IEnumerable<HexTile> AllNeighbours { get; set; }
	public IEnumerable<HexTile> ValidTiles { get { return AllNeighbours.Where(o => o.elevationType != ELEVATION.WATER && o.elevationType != ELEVATION.MOUNTAIN); } }
	public IEnumerable<HexTile> RoadTiles { get { return AllNeighbours.Where(o => o.isRoad); } }

	public List<HexTile> elligibleNeighbourTilesForPurchase { get { return AllNeighbours.Where(o => o.elevationType != ELEVATION.WATER && !o.isOwned && !o.isHabitable).ToList(); } } 

//	public List<HexTile> allFoodNeighbours { get 
//		{ return 
//			AllNeighbours.Where(o => (o.specialResource == RESOURCE.NONE && Utilities.GetBaseResourceType(o.defaultResource) == BASE_RESOURCE_TYPE.FOOD) || 
//				(Utilities.GetBaseResourceType(o.specialResource) == BASE_RESOURCE_TYPE.FOOD)).ToList(); 
//		}
//	}
//
//	public List<HexTile> allBaseResourceNeighbours { get 
//		{ return 
//			AllNeighbours.Where(o => (o.specialResource == RESOURCE.NONE && Utilities.GetBaseResourceType(o.defaultResource) == city.kingdom.basicResource) || 
//				(Utilities.GetBaseResourceType(o.specialResource) == city.kingdom.basicResource)).ToList(); 
//		}
//	}
//
//	public List<HexTile> allNormalNeighbours { get 
//		{ return 
//			AllNeighbours.Where(o => o.specialResource == RESOURCE.NONE).ToList(); 
//		}
//	}

	[ContextMenu("LALALA")]
	public void Show(){
		HexTile[] tiles = this.GetTilesInRange (13.5f);
		for (int i = 0; i < tiles.Length; i++) {
			if (tiles [i] != null) {
				tiles [i].GetComponent<SpriteRenderer> ().color = Color.magenta;
			}
		}
	}

	[ContextMenu("Show Occupant")]
	public void ShowOccupant(){
		if (this.isOccupied) {
			Debug.Log ("Occupant: " + this.occupant.role.ToString ());
		} else {
			Debug.Log ("Not Occupied");
		}
	}

	[ContextMenu("Show Pending Task")]
	public void ShowCityPendingTask(){
		for (int i = 0; i < this.city.pendingTask.Count; i++) {
			Debug.Log (this.city.pendingTask.Keys.ElementAt (i).ToString () + " " + this.city.pendingTask [this.city.pendingTask.Keys.ElementAt (i)].tileName);
		}
	}

	#region Resource
	internal void AssignDefaultResource(){
		if(elevationType == ELEVATION.MOUNTAIN){
			this.defaultResource = RESOURCE.GRANITE;
		}else{
			if (this.elevationType != ELEVATION.WATER) {
				switch (biomeType) {
				case BIOMES.BARE:
					this.defaultResource = RESOURCE.NONE;
					break;
				case BIOMES.DESERT:
					this.defaultResource = RESOURCE.GRANITE;
					break;
				case BIOMES.FOREST:
					this.defaultResource = RESOURCE.OAK;
					break;
				case BIOMES.GRASSLAND:
					this.defaultResource = RESOURCE.CORN;
					break;
				case BIOMES.SNOW:
					this.defaultResource = RESOURCE.NONE;
					break;
				case BIOMES.TUNDRA:
					this.defaultResource = RESOURCE.CORN;
					break;
				case BIOMES.WOODLAND:
					this.defaultResource = RESOURCE.CEDAR;
					break;

				}
			}
		}
	}
	internal void AssignSpecialResource(){
		int specialChance = UnityEngine.Random.Range (0, 100);

		if(specialChance < 15){
			//			Utilities.specialResourceCount += 1;
			if(this.elevationType == ELEVATION.MOUNTAIN){
				SpecialResourceChance specialResources = new SpecialResourceChance (
					new RESOURCE[] {
						RESOURCE.BEHEMOTH,
						RESOURCE.SLATE,
						RESOURCE.MARBLE,
						RESOURCE.MANA_STONE,
						RESOURCE.MITHRIL,
						RESOURCE.COBALT,
						RESOURCE.GOLD
					}, 
					new int[] { 5, 60, 40, 15, 15, 15, 5 });
				this.specialResource = ComputeSpecialResource (specialResources);
				if (this.specialResource != RESOURCE.NONE) {
					this.resourceVisualGO.GetComponent<SpriteRenderer> ().sprite = Resources.LoadAll<Sprite> ("Resources Icons")
					.Where (x => x.name == this.specialResource.ToString ()).ToList () [0];
					this.resourceVisualGO.SetActive (true);
				}
			}else{
				if (this.elevationType != ELEVATION.WATER) {
					this.specialResource = ComputeSpecialResource (Utilities.specialResourcesLookup [this.biomeType]);
					if (this.specialResource != RESOURCE.NONE) {
						this.resourceVisualGO.GetComponent<SpriteRenderer> ().sprite = Resources.LoadAll<Sprite> ("Resources Icons")
						.Where (x => x.name == this.specialResource.ToString ()).ToList () [0];
						this.resourceVisualGO.SetActive (true);
					}
				}
			}
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
		
	/*
	 * Returns all Hex tiles gameobjects within a radius
	 * 3 - 1 tile radius
	 * 6 - 2 tile radius
	 * 10 - 3 tile radius
	 * */
	public HexTile[] GetTilesInRange(float radius){
		Collider2D[] nearHexes = Physics2D.OverlapCircleAll (new Vector2(transform.position.x, transform.position.y), radius);
		HexTile[] nearTiles = new HexTile[nearHexes.Length];
		for (int i = 0; i < nearTiles.Length; i++) {
			if (nearHexes[i].name != this.name) {
				nearTiles[i] = nearHexes[i].gameObject.GetComponent<HexTile>();
//				nearHexes[i].gameObject.GetComponent<SpriteRenderer>().color = Color.black;
//				Debug.Log (nearHexes [i].name);
			}
		}
		return nearTiles;
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
	public void GenerateTileDetails(){
		List<HexTile> neighbours = this.AllNeighbours.ToList ();
		for (int i = 0; i < neighbours.Count; i++) {
			
			int neighbourX = neighbours [i].xCoordinate;
			int neighbourY = neighbours [i].yCoordinate;

			Point difference = new Point((neighbourX - this.xCoordinate), (neighbourY - this.yCoordinate));
			if (this.yCoordinate % 2 == 0) {
				if (difference.X == -1 && difference.Y == 1) {
					//top left
					if (neighbours[i].biomeType != this.biomeType && neighbours[i].elevationType != ELEVATION.WATER) {
						this.topLeftBorder.SetActive (true);
					}
				} else if (difference.X == 0 && difference.Y == 1) {
					//top right
					if (neighbours [i].biomeType != this.biomeType && neighbours [i].elevationType != ELEVATION.WATER) {
						this.topRightBorder.SetActive (true);
					}
				} else if (difference.X == 1 && difference.Y == 0) {
					//right
					if (neighbours [i].elevationType == ELEVATION.WATER) {
						this.rightGround.SetActive (true);
					} else if (neighbours [i].biomeType != this.biomeType) {
						this.rightBorder.SetActive (true);
					}
				} else if (difference.X == 0 && difference.Y == -1){
					//bottom right
					if (neighbours [i].elevationType == ELEVATION.WATER) {
						this.bottomRightGround.SetActive (true);
					} else if (neighbours [i].biomeType != this.biomeType) {
						this.bottomRightBorder.SetActive (true);
					}
				} else if (difference.X == -1 && difference.Y == -1){
					//bottom left
					if (neighbours [i].elevationType == ELEVATION.WATER) {
						this.bottomLeftGround.SetActive (true);
					} else if (neighbours [i].biomeType != this.biomeType) {
						this.bottomLeftBorder.SetActive (true);
					}
				} else if (difference.X == -1 && difference.Y == 0){
					//left
					if (neighbours [i].elevationType == ELEVATION.WATER) {
						this.leftGround.SetActive (true);
					} else if (neighbours [i].biomeType != this.biomeType) {
						this.leftBorder.SetActive (true);
					}
				}
			} else {
				if (difference.X == 0 && difference.Y == 1) {
					//top left
					if (neighbours [i].biomeType != this.biomeType && neighbours [i].elevationType != ELEVATION.WATER) {
						this.topLeftBorder.SetActive (true);
					}
				} else if (difference.X == 1 && difference.Y == 1) {
					//top right
					if (neighbours [i].biomeType != this.biomeType && neighbours [i].elevationType != ELEVATION.WATER) {
						this.topRightBorder.SetActive (true);
					}
				} else if (difference.X == 1 && difference.Y == 0) {
					//right
					if (neighbours [i].elevationType == ELEVATION.WATER) {
						this.rightGround.SetActive (true);
					} else if (neighbours [i].biomeType != this.biomeType) {
						this.rightBorder.SetActive (true);
					}
				} else if (difference.X == 1 && difference.Y == -1){
					//bottom right
					if (neighbours [i].elevationType == ELEVATION.WATER) {
						this.bottomRightGround.SetActive (true);
					} else if (neighbours [i].biomeType != this.biomeType) {
						this.bottomRightBorder.SetActive (true);
					}
				} else if (difference.X == 0 && difference.Y == -1){
					//bottom left
					if (neighbours [i].elevationType == ELEVATION.WATER) {
						this.bottomLeftGround.SetActive (true);
					} else if (neighbours [i].biomeType != this.biomeType) {
						this.bottomLeftBorder.SetActive (true);
					}
				} else if (difference.X == -1 && difference.Y == 0){
					//left
					if (neighbours [i].elevationType == ELEVATION.WATER) {
						this.leftGround.SetActive (true);
					} else if (neighbours [i].biomeType != this.biomeType) {
						this.leftBorder.SetActive (true);
					}
				}
			}
		}
	}
	public void SetTileSprites(Sprite baseSprite, Sprite leftSprite, Sprite rightSprite, Sprite leftCornerSprite, Sprite rightCornerSprite, Sprite[] centerSprite){
		this.GetComponent<SpriteRenderer>().sprite = baseSprite;
		this.leftGround.GetComponent<SpriteRenderer>().sprite = leftSprite;
		this.rightGround.GetComponent<SpriteRenderer>().sprite = rightSprite;
		this.bottomLeftGround.GetComponent<SpriteRenderer>().sprite = leftCornerSprite;
		this.bottomRightGround.GetComponent<SpriteRenderer>().sprite = rightCornerSprite;
		if (this.elevationType == ELEVATION.MOUNTAIN) {
			this.centerPiece.SetActive(true);
		} else {
			if (this.biomeType == BIOMES.GRASSLAND) {
				return;
			} else if (this.biomeType == BIOMES.WOODLAND || this.biomeType == BIOMES.FOREST || this.biomeType == BIOMES.TUNDRA) {
				this.centerPiece.GetComponent<SpriteRenderer>().sprite = centerSprite[Random.Range(0, centerSprite.Length)];
				this.centerPiece.SetActive(true);
				if (this.biomeType != BIOMES.TUNDRA) {
					this.centerPiece.transform.localPosition = new Vector3 (0f, 0.37f, 0f);
				}
			} else {
				int chanceForDetail = Random.Range (0, 100);
				if (chanceForDetail < 25) {
					this.centerPiece.GetComponent<SpriteRenderer>().sprite = centerSprite[Random.Range(0, centerSprite.Length)];
					this.centerPiece.SetActive(true);
					float randomXPosition = Random.Range(-1.30f, 1.30f);
					float randomYPosition = Random.Range(-0.40f, 0.70f);
					if (randomXPosition <= 0.45f && randomXPosition >= -0.45f) {
						int chanceToModify = Random.Range(0, 100);
						if (chanceToModify < 25) {
							if (Mathf.Sign (randomYPosition) == 0) {
								//negative
								randomYPosition -= Random.Range(0.20f,0.40f);
							} else {
								//positive
								randomYPosition += Random.Range(0.20f,0.40f);
							}
						}
					}
					this.centerPiece.transform.localPosition = new Vector3(randomXPosition, randomYPosition, 0f);
				}
			}

		}
	}


	#endregion

	public void OccupyTile(Citizen citizen){
		this.isOccupied = true;
		this.occupant = citizen;
		switch (citizen.role) {
		case ROLE.FOODIE:
			this.GetComponent<SpriteRenderer> ().color = Color.green;
			break;
		case ROLE.GATHERER:
			this.GetComponent<SpriteRenderer> ().color = Color.gray;
			break;
		case ROLE.GENERAL:
			this.GetComponent<SpriteRenderer> ().color = Color.red;
			break;
		case ROLE.MINER:
			this.GetComponent<SpriteRenderer> ().color = Color.grey;
			break;
		default:
			this.GetComponent<SpriteRenderer> ().color = Color.blue;
			break;
		}
	}

	void OnMouseDown(){
		if (this.isHabitable && this.city != null) {
//			CameraMove.Instance.CenterCameraOn (this.gameObject);
			UIManager.Instance.ShowCityInfo (this.city);
		}
	}

}
