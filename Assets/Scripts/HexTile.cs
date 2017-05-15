using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathFind;
using System.Linq;
using Panda;

public class HexTile : MonoBehaviour,  IHasNeighbours<HexTile>{
	public int xCoordinate;
	public int yCoordinate;

	public string tileName;

	public float elevationNoise;
	public float moistureNoise;
	public float temperature;

	public RESOURCE specialResource;

	public BIOMES biomeType;
	public ELEVATION elevationType;

	public int movementDays;

	public City city = null;

	public bool isHabitable = false;
	public bool isRoad = false;
	public bool isOccupied = false;
	public bool isBorder = false;
	public int isBorderOfCityID = 0;
	internal int isOccupiedByCityID = 0;

	public GameObject centerPiece;

	public GameObject leftBorder;
	public GameObject rightBorder;
	public GameObject topLeftBorder;
	public GameObject topRightBorder;
	public GameObject bottomLeftBorder;
	public GameObject bottomRightBorder;

	public GameObject resourceVisualGO;
	public GameObject structureGO;
	public Transform eventsParent;
	public GameObject cityNameGO;
	public TextMesh cityNameLbl;
	public SpriteRenderer kingdomColorSprite;
	public GameObject highlightGO;

	public List<HexTile> connectedTiles = new List<HexTile>();

	public IEnumerable<HexTile> AllNeighbours { get; set; }
	public IEnumerable<HexTile> ValidTiles { get { return AllNeighbours.Where(o => o.elevationType != ELEVATION.WATER && o.elevationType != ELEVATION.MOUNTAIN); } }
	public IEnumerable<HexTile> RoadTiles { get { return AllNeighbours.Where(o => o.isRoad); } }
	public IEnumerable<HexTile> PurchasableTiles { get { return AllNeighbours.Where (o => o.elevationType != ELEVATION.WATER);}}
	public IEnumerable<HexTile> CombatTiles { get { return AllNeighbours.Where (o => o.elevationType != ELEVATION.WATER);}}

	public List<HexTile> elligibleNeighbourTilesForPurchase { get { return PurchasableTiles.Where(o => !o.isOccupied && !o.isHabitable).ToList(); } } 

	private List<WorldEventItem> eventsOnTile = new List<WorldEventItem>();

	public int range = 0;
	List<HexTile> tiles = new List<HexTile> ();

	[ContextMenu("Show Tiles In Range")]
	public void ShowTilesInRange(){
		for (int i = 0; i < tiles.Count; i++) {
			tiles [i].GetComponent<SpriteRenderer> ().color = Color.white;
		}
		tiles.Clear ();
		tiles.AddRange(this.GetTilesInRange (range));
		for (int i = 0; i < tiles.Count; i++) {
			tiles [i].GetComponent<SpriteRenderer> ().color = Color.magenta;
		}
	}

	[ContextMenu("Show Border Tiles")]
	public void ShowBorderTiles(){
		for (int i = 0; i < this.city.borderTiles.Count; i++) {
			this.city.borderTiles[i].GetComponent<SpriteRenderer> ().color = Color.magenta;
		}
	}
		
	[ContextMenu("Increase General HP")]
	public void IncreaseGeneralHP(){
		List<Citizen> generals = this.city.GetCitizensWithRole (ROLE.GENERAL);
		for (int i = 0; i < generals.Count; i++) {
			((General)generals[i].assignedRole).army.hp += 100;
			Debug.Log (((General)generals [i].assignedRole).citizen.name + " hp is " + ((General)generals [i].assignedRole).army.hp.ToString ());
		}
	}

	[ContextMenu("Show Citizen Creation Table")]
	public void ShowCitizenCreationTable(){
		Dictionary<ROLE, int> citizenCreationTable = this.city.citizenCreationTable;
		for (int i = 0; i < citizenCreationTable.Keys.Count; i++) {
			ROLE key = citizenCreationTable.Keys.ElementAt(i);
			Debug.Log (key.ToString () + " - " + citizenCreationTable [key].ToString ());
		}
	}

	[ContextMenu("Show Adjacent Cities")]
	public void ShowAdjacentCities(){
		for (int i = 0; i < this.city.adjacentCities.Count; i++) {
			Debug.Log ("Adjacent City: " + this.city.adjacentCities [i].name);
		}
	}

	[ContextMenu("Show Adjacent Kingdoms")]
	public void ShowAdjacentKingdoms(){
		for (int i = 0; i < this.city.kingdom.adjacentKingdoms.Count; i++) {
			Debug.Log ("Adjacent Kingdom: " + this.city.kingdom.adjacentKingdoms[i].name);
		}
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
            specialChanceForBiome = 11;
        } else if (this.biomeType == BIOMES.DESERT) {
            specialChanceForBiome = 14;
        } else if (this.biomeType == BIOMES.TUNDRA || this.biomeType == BIOMES.SNOW) {
            specialChanceForBiome = 7;
        }

		if (specialChance < specialChanceForBiome) {
			if (this.elevationType != ELEVATION.WATER && this.elevationType != ELEVATION.MOUNTAIN) {
				this.specialResource = ComputeSpecialResource (Utilities.specialResourcesLookup [this.biomeType]);
				if (this.specialResource != RESOURCE.NONE) {
					this.resourceVisualGO.GetComponent<SpriteRenderer> ().sprite = Resources.LoadAll<Sprite> ("Resources Icons")
                    .Where (x => x.name == this.specialResource.ToString ()).ToList () [0];
					this.resourceVisualGO.SetActive (true);
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
	 * 3 - 1 tile radius
	 * 6 - 2 tile radius
	 * 10 - 3 tile radius
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
	public void SetTileSprites(Sprite baseSprite){
		this.GetComponent<SpriteRenderer>().sprite = baseSprite;
		if (this.elevationType == ELEVATION.MOUNTAIN) {
			this.centerPiece.SetActive(true);
		}
	}


	#endregion

	public void ShowCitySprite(){
		this.structureGO.GetComponent<SpriteRenderer>().sprite = CityGenerator.Instance.elfCitySprite;
		this.structureGO.SetActive(true);
		Color color = this.city.kingdom.kingdomColor;
		color.a = 76.5f/255f;
		this.kingdomColorSprite.color = color;
		this.GetComponent<SpriteRenderer>().color = Color.white;
		this.GetComponent<SpriteRenderer>().sprite = Biomes.Instance.bareTiles [Random.Range (0, Biomes.Instance.bareTiles.Length)];
	}

	public void ShowNamePlate(){
		this.cityNameGO.SetActive(true);
		this.cityNameLbl.GetComponent<Renderer>().sortingLayerName = "CityNames";
		this.cityNameLbl.text = this.city.name + "\n" + this.city.kingdom.name;
	}

	public void ShowOccupiedSprite(){
		this.GetComponent<SpriteRenderer> ().sprite = Biomes.Instance.bareTiles [Random.Range (0, Biomes.Instance.bareTiles.Length)];
		this.structureGO.GetComponent<SpriteRenderer>().sprite = CityGenerator.Instance.elfTraderSprite;
		this.structureGO.SetActive(true);
		this.centerPiece.SetActive(false);
	}

	public void ResetTile(){
//		this.isOwned = false;
		this.isOccupied = false;
		this.isBorder = false;
		this.isBorderOfCityID = 0;
		this.isOccupiedByCityID = 0;
		this.structureGO.SetActive(false);
		this.kingdomColorSprite.color = Color.white;
		this.kingdomColorSprite.gameObject.SetActive(false);
	}

	public void AddEventOnTile(GameEvent gameEvent){
		GameObject eventGO = GameObject.Instantiate (Resources.Load ("GameObjects/WorldEventItem") as GameObject, this.eventsParent) as GameObject;
		eventGO.transform.localPosition = Vector3.zero;
		eventGO.GetComponent<WorldEventItem> ().SetGameEvent(gameEvent);
	}


	void OnMouseDown(){
		if (UIManager.Instance.IsMouseOnUI ()) {
			return;
		}
		if (this.isHabitable && this.isOccupied && this.city != null) {
			CameraMove.Instance.CenterCameraOn(this.gameObject);
			UIManager.Instance.ShowCityInfo (this.city, true);
		}
	}

	void OnMouseOver(){
		if (UIManager.Instance.IsMouseOnUI ()) {
			return;
		}
		if (this.isHabitable && this.isOccupied) {
			this.city.kingdom.HighlightAllOwnedTilesInKingdom();
			this.city.HighlightAllOwnedTiles(204f/255f);
		}
	}

	void OnMouseExit(){
		if (this.isHabitable && this.isOccupied) {
			if (UIManager.Instance.currentlyShowingKingdom != null) {
				//if there is currently showing kingdom, if this city is part of that kingdom remain higlighted, but less
				if (UIManager.Instance.currentlyShowingKingdom.id == this.city.kingdom.id) {
					this.city.kingdom.HighlightAllOwnedTilesInKingdom();
					if (UIManager.Instance.currentlyShowingCity != null) {
						if (UIManager.Instance.currentlyShowingCity.id == this.city.id) {
							this.city.HighlightAllOwnedTiles (204f / 255f);
						}
					}
				} else {
					this.city.kingdom.UnHighlightAllOwnedTilesInKingdom ();
					if (UIManager.Instance.currentlyShowingCity != null) {
						if (UIManager.Instance.currentlyShowingCity.id == this.city.id) {
							this.city.HighlightAllOwnedTiles (204f / 255f);
						}
					}
				}
			}
		}
	}
}
