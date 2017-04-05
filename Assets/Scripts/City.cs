using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

[System.Serializable]
public class City{

	public int id;
	public string name;
	public HexTile hexTile;
	public Kingdom kingdom;
	public Citizen governor;
	public List<HexTile> ownedTiles;
	public List<Citizen> incomingGenerals;
	public List<Citizen> citizens;
	public List<City> connectedCities;
	public string cityHistory;
	public bool hasKing;

	[Space(10)] //Resources
	public int sustainability;
	public int lumberCount;
	public int stoneCount;
	public int manaStoneCount;
	public int mithrilCount;
	public int cobaltCount;
	public int goldCount;
	public int[] allResourceProduction; //food, lumber, stone, mana stone, mithril, cobalt, gold
	public TradeManager tradeManager;

	[Space(5)]
	public IsActive isActive;
	public bool isStarving;
	public bool isDead;
	//generals
	//incoming generals

	protected Dictionary<ROLE, int> citizenCreationTable;
	public Dictionary<CITY_TASK, HexTile> pendingTask;
	protected List<HexTile> allUnownedNeighbours;
	protected List<HexTile> purchasableFoodTiles;
	protected List<HexTile> purchasableBasicTiles;
	protected List<HexTile> purchasableRareTiles;
	protected List<HexTile> purchasabletilesWithUnneededResource;


	protected List<HexTile> unoccupiedOwnedTiles{
		get{ return this.ownedTiles.Where (x => !x.isOccupied).ToList();}
	}

	public List<Citizen> elligibleBachelorettes{
		get{ return this.citizens.Where(x => x.age >= 16 && x.gender == GENDER.FEMALE && !x.isMarried).ToList();}
	}

	public List<Citizen> elligibleBachelors{
		get{ return this.citizens.Where(x => x.age >= 16 && x.gender == GENDER.MALE && !x.isMarried).ToList();}
	}

	public List<HexTile> adjacentHabitableTiles{
		get{ return this.hexTile.connectedTiles.Where(x => !x.isOccupied).ToList();}
	}

	public City(HexTile hexTile, Kingdom kingdom){
		this.id = Utilities.SetID(this);
		this.name = "City" + this.id.ToString();
		this.hexTile = hexTile;
		this.kingdom = kingdom;
		this.governor = null;
		this.ownedTiles = new List<HexTile>();
		this.incomingGenerals = new List<Citizen> ();
		this.citizens = new List<Citizen>();
		this.cityHistory = string.Empty;
		this.isActive = new IsActive (false);
		this.hasKing = false;
		this.isStarving = false;
		this.isDead = false;
		this.allResourceProduction = new int[]{ 0, 0, 0, 0, 0, 0, 0 };
		this.tradeManager = new TradeManager(this, this.kingdom);
		this.citizenCreationTable = Utilities.defaultCitizenCreationTable;
		this.pendingTask = new Dictionary<CITY_TASK, HexTile>();
		this.allUnownedNeighbours = new List<HexTile>();
		this.purchasableFoodTiles = new List<HexTile>();
		this.purchasableBasicTiles = new List<HexTile>();
		this.purchasableRareTiles = new List<HexTile>();
		this.purchasabletilesWithUnneededResource = new List<HexTile>();

		this.hexTile.isOccupied = true;
		this.ownedTiles.Add(this.hexTile);
		this.hexTile.ShowCitySprite();

//		this.CreateInitialFamilies();

		EventManager.Instance.onCityEverydayTurnActions.AddListener(CityEverydayTurnActions);
		EventManager.Instance.onCitizenDiedEvent.AddListener(CheckCityDeath);
		EventManager.Instance.onRecruitCitizensForExpansion.AddListener(DonateCitizensToExpansion);
//		EventManager.Instance.onCitizenDiedEvent.AddListener (UpdateHextileRoles);
	}


	/*
	 * Initialize City With Initial Citizens aka. Families
	 * */
	internal void CreateInitialFamilies(){
		BuyInitialTiles ();
		CreateInitialRoyalFamily ();
		CreateInitialGovernorFamily ();
		CreateInitialFoodProducerFamily ();
		CreateInitialGathererFamily ();
		CreateInitialGeneralFamily ();
		CreateInitialUntrainedFamily ();
		GenerateInitialTraitsForInitialCitizens ();
		UpdateResourceProduction ();
		UpdateUnownedNeighbourTiles();

		for (int i = 0; i < this.citizens.Count; i++) {
			this.citizens[i].UpdatePrestige();
		}
	}

	private void BuyInitialTiles(){
		Debug.Log ("Buying tiles for kingdom: " + this.kingdom.name + " " + this.kingdom.race.ToString());
		List<HexTile> allAdjacentTiles = this.hexTile.elligibleNeighbourTilesForPurchase.ToList();
		//Buy Food Tiles
		int foodCounter = 0;
		for (int i = 0; i < allAdjacentTiles.Count; i++) {
			if (foodCounter >= 2) {
				break;
			}
			HexTile currentHexTile = allAdjacentTiles[i];
			if (this.ownedTiles.Contains (currentHexTile)) {
				continue;
			}

			if (currentHexTile.specialResource == RESOURCE.NONE) {
				if (Utilities.GetBaseResourceType (currentHexTile.defaultResource) == BASE_RESOURCE_TYPE.FOOD) {
					foodCounter++;
					Debug.Log ("Bought tile " + currentHexTile.name + " for food"); 
					this.PurchaseTile(currentHexTile);
				}
			} else {
				if (Utilities.GetBaseResourceType (currentHexTile.specialResource) == BASE_RESOURCE_TYPE.FOOD) {
					foodCounter++;
					Debug.Log ("Bought tile " + currentHexTile.name + " for food"); 
					this.PurchaseTile(currentHexTile);
				}
			}
		}

		//buy base resource tile
		for (int i = 0; i < allAdjacentTiles.Count; i++) {
			HexTile currentHexTile = allAdjacentTiles[i];
			if (this.ownedTiles.Contains (currentHexTile)) {
				continue;
			}
			if (currentHexTile.specialResource == RESOURCE.NONE) {
				if (Utilities.GetBaseResourceType (currentHexTile.defaultResource) == this.kingdom.basicResource) {
					Debug.Log ("Bought tile " + currentHexTile.name + " for base resource"); 
					this.PurchaseTile(currentHexTile);
					break;
				}
			} else {
				if (Utilities.GetBaseResourceType (currentHexTile.specialResource) == this.kingdom.basicResource) {
					Debug.Log ("Bought tile " + currentHexTile.name + " for base resource"); 
					this.PurchaseTile(currentHexTile);
					break;
				}
			}
		}

		//buy normal tile without special resource
		for (int i = 0; i < allAdjacentTiles.Count; i++) {
			HexTile currentHexTile = allAdjacentTiles[i];
			if (this.ownedTiles.Contains (currentHexTile)) {
				continue;
			}

			if (currentHexTile.specialResource == RESOURCE.NONE) {
				Debug.Log ("Bought tile " + currentHexTile.name + " for nothing"); 
				this.PurchaseTile(currentHexTile);
				break;
			}
		}
	}
	internal void CreateInitialRoyalFamily(){
		this.kingdom.successionLine.Clear ();
		GENDER gender = GENDER.MALE;
		int randomGender = UnityEngine.Random.Range (0, 100);
		if(randomGender < 20){
			gender = GENDER.FEMALE;
		}
		Citizen king = new Citizen (this, UnityEngine.Random.Range (20, 36), gender, 2);
		Citizen father = new Citizen (this, UnityEngine.Random.Range (60, 81), GENDER.MALE, 1);
		Citizen mother = new Citizen (this, UnityEngine.Random.Range (60, 81), GENDER.FEMALE, 1);

		king.role = ROLE.KING;
		King job = new King (king);
		king.assignedRole = job;
		job.SetOwnedKingdom(this.kingdom);
		this.kingdom.king = king;

		this.kingdom.king.isKing = true;
		this.kingdom.king.isDirectDescendant = true;

		father.isDirectDescendant = true;
		mother.isDirectDescendant = true;
		father.isDead = true;
		mother.isDead = true;

		this.citizens.Remove(father);
		this.citizens.Remove(mother);
//		father.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), PoliticsPrototypeManager.Instance.year - father.age);
//		mother.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), PoliticsPrototypeManager.Instance.year - mother.age);
//		this.kingdom.king.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - this.assignedLord.age));

		father.AddChild (this.kingdom.king);
		mother.AddChild (this.kingdom.king);
		king.AddParents(father, mother);
		MarriageManager.Instance.Marry(father, mother);

		int siblingsChance = UnityEngine.Random.Range (0, 100);
		if(siblingsChance < 25){
			Citizen sibling = MarriageManager.Instance.MakeBaby (father, mother, UnityEngine.Random.Range(0,this.kingdom.king.age));
			Citizen sibling2 = MarriageManager.Instance.MakeBaby (father, mother, UnityEngine.Random.Range(0,this.kingdom.king.age));

//			sibling.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - sibling.age));
//			sibling2.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - sibling2.age));

		}else if(siblingsChance >= 25 && siblingsChance < 75){
			Citizen sibling = MarriageManager.Instance.MakeBaby (father, mother, UnityEngine.Random.Range(0,this.kingdom.king.age));
//			sibling.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - sibling.age));
		}

		int spouseChance = UnityEngine.Random.Range (0, 100);
		if (spouseChance < 80) {
			Citizen spouse = MarriageManager.Instance.CreateSpouse (this.kingdom.king);
			List<int> childAges = Enumerable.Range(0, (spouse.age - 16)).ToList();
			if(spouse.gender == GENDER.MALE){
				childAges = Enumerable.Range(0, (this.kingdom.king.age - 16)).ToList();
			}
//			spouse.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - spouse.age));


			int childChance = UnityEngine.Random.Range (0, 100);
			if (childChance < 25) {
				
				int age1 = UnityEngine.Random.Range (0, childAges.Count);
				Citizen child1 = MarriageManager.Instance.MakeBaby (this.kingdom.king, this.kingdom.king.spouse, childAges[age1]);
				childAges.RemoveAt (age1);

				int age2 = UnityEngine.Random.Range (0, childAges.Count);
				Citizen child2 = MarriageManager.Instance.MakeBaby (this.kingdom.king, this.kingdom.king.spouse, childAges[age2]);
				childAges.RemoveAt (age2);

				int age3 = UnityEngine.Random.Range (0, childAges.Count);
				Citizen child3 = MarriageManager.Instance.MakeBaby (this.kingdom.king, this.kingdom.king.spouse, childAges[age3]);




//				child1.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - child1.age));
//				child2.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - child2.age));
//				child3.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - child3.age));


			} else if (childChance >= 25 && childChance < 50) {
				
				int age1 = UnityEngine.Random.Range (0, childAges.Count);
				Citizen child1 = MarriageManager.Instance.MakeBaby (this.kingdom.king, this.kingdom.king.spouse, childAges[age1]);
				childAges.RemoveAt (age1);

				int age2 = UnityEngine.Random.Range (0, childAges.Count);
				Citizen child2 = MarriageManager.Instance.MakeBaby (this.kingdom.king, this.kingdom.king.spouse, childAges[age2]);



//				child1.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - child1.age));
//				child2.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - child2.age));


			} else if (childChance >= 50 && childChance < 75) {

				int age1 = UnityEngine.Random.Range (0, childAges.Count);

				Citizen child1 = MarriageManager.Instance.MakeBaby (this.kingdom.king, this.kingdom.king.spouse, childAges[age1]);

//				child1.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - child1.age));

			}
		}
	}
	private void CreateInitialGovernorFamily(){
		GENDER gender = GENDER.MALE;
		int randomGender = UnityEngine.Random.Range (0, 100);
		if(randomGender < 20){
			gender = GENDER.FEMALE;
		}
		Citizen governor = new Citizen (this, UnityEngine.Random.Range (20, 36), gender, 2);
		Citizen father = new Citizen (this, UnityEngine.Random.Range (60, 81), GENDER.MALE, 1);
		Citizen mother = new Citizen (this, UnityEngine.Random.Range (60, 81), GENDER.FEMALE, 1);

		governor.isGovernor = true;
		governor.role = ROLE.GOVERNOR;
		Governor job = new Governor (governor);
		governor.assignedRole = job;
		job.SetOwnedCity(this);

		//		father.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), PoliticsPrototypeManager.Instance.year - father.age);
		//		mother.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), PoliticsPrototypeManager.Instance.year - mother.age);
		//		this.kingdom.king.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - this.assignedLord.age));

		father.isDead = true;
		mother.isDead = true;
		this.citizens.Remove(father);
		this.citizens.Remove(mother);

		father.AddChild (governor);
		mother.AddChild (governor);
		governor.AddParents(father, mother);
		MarriageManager.Instance.Marry(father, mother);

		this.governor = governor;
		this.UpdateCitizenCreationTable();

		int siblingsChance = UnityEngine.Random.Range (0, 100);
		if(siblingsChance < 25){
			Citizen sibling = MarriageManager.Instance.MakeBaby (father, mother, UnityEngine.Random.Range(0,governor.age));
			Citizen sibling2 = MarriageManager.Instance.MakeBaby (father, mother, UnityEngine.Random.Range(0,governor.age));

			//			sibling.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - sibling.age));
			//			sibling2.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - sibling2.age));

		}else if(siblingsChance >= 25 && siblingsChance < 75){
			Citizen sibling = MarriageManager.Instance.MakeBaby (father, mother, UnityEngine.Random.Range(0,governor.age));
			//			sibling.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - sibling.age));
		}

		int spouseChance = UnityEngine.Random.Range (0, 100);
		if (spouseChance < 80) {
			Citizen spouse = MarriageManager.Instance.CreateSpouse (governor);
			//			spouse.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - spouse.age));
			List<int> childAges = Enumerable.Range(0, (spouse.age - 16)).ToList();
			if(spouse.gender == GENDER.MALE){
				childAges = Enumerable.Range(0, (governor.age - 16)).ToList();
			}

			int childChance = UnityEngine.Random.Range (0, 100);
			if (childChance < 25) {

				int age1 = UnityEngine.Random.Range (0, childAges.Count);
				Citizen child1 = MarriageManager.Instance.MakeBaby (governor, spouse, childAges[age1]);
				childAges.RemoveAt (age1);

				int age2 = UnityEngine.Random.Range (0, childAges.Count);
				Citizen child2 = MarriageManager.Instance.MakeBaby (governor, spouse, childAges[age2]);
				childAges.RemoveAt (age2);

				int age3 = UnityEngine.Random.Range (0, childAges.Count);
				Citizen child3 = MarriageManager.Instance.MakeBaby (governor, spouse, childAges[age3]);



				//				child1.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - child1.age));
				//				child2.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - child2.age));
				//				child3.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - child3.age));


			} else if (childChance >= 25 && childChance < 50) {

				int age1 = UnityEngine.Random.Range (0, childAges.Count);
				Citizen child1 = MarriageManager.Instance.MakeBaby (governor, spouse, childAges[age1]);
				childAges.RemoveAt (age1);

				int age2 = UnityEngine.Random.Range (0, childAges.Count);
				Citizen child2 = MarriageManager.Instance.MakeBaby (governor, spouse, childAges[age2]);

				//				child1.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - child1.age));
				//				child2.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - child2.age));


			} else if (childChance >= 50 && childChance < 75) {

				int age1 = UnityEngine.Random.Range (0, childAges.Count);

				Citizen child1 = MarriageManager.Instance.MakeBaby (governor, spouse, childAges[age1]);

//				child1.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - child1.age));

			}
		}
	}
	private void CreateInitialGeneralFamily(){
		GENDER gender = GENDER.MALE;
		int randomGender = UnityEngine.Random.Range (0, 2);
		if(randomGender == 0){
			gender = GENDER.FEMALE;
		}
		Citizen general = new Citizen (this, UnityEngine.Random.Range (20, 36), gender, 2);
		Citizen father = new Citizen (this, UnityEngine.Random.Range (60, 81), GENDER.MALE, 1);
		Citizen mother = new Citizen (this, UnityEngine.Random.Range (60, 81), GENDER.FEMALE, 1);

		general.role = ROLE.GENERAL;
		general.assignedRole = new General (general);
		//		father.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), PoliticsPrototypeManager.Instance.year - father.age);
		//		mother.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), PoliticsPrototypeManager.Instance.year - mother.age);
		//		this.kingdom.king.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - this.assignedLord.age));

		father.isDead = true;
		mother.isDead = true;
		this.citizens.Remove(father);
		this.citizens.Remove(mother);

		father.AddChild (general);
		mother.AddChild (general);
		general.AddParents(father, mother);
		MarriageManager.Instance.Marry(father, mother);

		int spouseChance = UnityEngine.Random.Range (0, 2);
		if (spouseChance == 0) {
			Citizen spouse = MarriageManager.Instance.CreateSpouse (general);
			//			spouse.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - spouse.age));

			int childChance = UnityEngine.Random.Range (0, 100);
			if (childChance < 25) {

				int age = UnityEngine.Random.Range (0, ((spouse.age - 16) + 1));
				if(spouse.gender == GENDER.MALE){
					age = UnityEngine.Random.Range (0, ((general.age - 16) + 1));
				}
				Citizen child1 = MarriageManager.Instance.MakeBaby (general, spouse, age);

				//				child1.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - child1.age));
				//				child2.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - child2.age));
				//				child3.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - child3.age));


			}
		}
		HexTile tileForCitizen = this.FindTileForCitizen (general);
		if (tileForCitizen != null) {
			this.OccupyTile (tileForCitizen, general);
		}
	
	}
	private void CreateInitialFoodProducerFamily(){
		for(int i = 0; i < 3; i++){
			GENDER gender = GENDER.MALE;
			int randomGender = UnityEngine.Random.Range (0, 2);
			if(randomGender == 0){
				gender = GENDER.FEMALE;
			}
			Citizen producer = new Citizen (this, UnityEngine.Random.Range (20, 36), gender, 2);
			Citizen father = new Citizen (this, UnityEngine.Random.Range (60, 81), GENDER.MALE, 1);
			Citizen mother = new Citizen (this, UnityEngine.Random.Range (60, 81), GENDER.FEMALE, 1);

			producer.AssignRole (ROLE.FOODIE);
			//		father.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), PoliticsPrototypeManager.Instance.year - father.age);
			//		mother.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), PoliticsPrototypeManager.Instance.year - mother.age);
			//		this.kingdom.king.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - this.assignedLord.age));

			father.isDead = true;
			mother.isDead = true;
			this.citizens.Remove(father);
			this.citizens.Remove(mother);

			father.AddChild (producer);
			mother.AddChild (producer);
			producer.AddParents(father, mother);
			MarriageManager.Instance.Marry(father, mother);

			int spouseChance = UnityEngine.Random.Range (0, 2);
			if (spouseChance == 0) {
				Citizen spouse = MarriageManager.Instance.CreateSpouse (producer);
				//			spouse.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - spouse.age));

				int childChance = UnityEngine.Random.Range (0, 100);
				if (childChance < 25) {

					int age = UnityEngine.Random.Range (0, ((spouse.age - 16) + 1));
					if(spouse.gender == GENDER.MALE){
						age = UnityEngine.Random.Range (0, ((producer.age - 16) + 1));
					}
					Citizen child1 = MarriageManager.Instance.MakeBaby (producer, spouse, age);

					//				child1.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - child1.age));
					//				child2.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - child2.age));
					//				child3.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - child3.age));


				}
			}

			if (i == 0 || i == 1) {
				HexTile tileForCitizen = this.FindTileForCitizen (producer);
				if (tileForCitizen != null) {
					this.OccupyTile (tileForCitizen, producer);
				}
			}
		}
	}
	private void CreateInitialGathererFamily(){
		for(int i = 0; i < 2; i++){
			GENDER gender = GENDER.MALE;
			int randomGender = UnityEngine.Random.Range (0, 2);
			if(randomGender == 0){
				gender = GENDER.FEMALE;
			}
			Citizen gatherer = new Citizen (this, UnityEngine.Random.Range (20, 36), gender, 2);
			Citizen father = new Citizen (this, UnityEngine.Random.Range (60, 81), GENDER.MALE, 1);
			Citizen mother = new Citizen (this, UnityEngine.Random.Range (60, 81), GENDER.FEMALE, 1);

			gatherer.AssignRole (ROLE.GATHERER);
			//		father.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), PoliticsPrototypeManager.Instance.year - father.age);
			//		mother.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), PoliticsPrototypeManager.Instance.year - mother.age);
			//		this.kingdom.king.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - this.assignedLord.age));

			father.isDead = true;
			mother.isDead = true;
			this.citizens.Remove(father);
			this.citizens.Remove(mother);

			father.AddChild (gatherer);
			mother.AddChild (gatherer);
			gatherer.AddParents(father, mother);
			MarriageManager.Instance.Marry(father, mother);

			int spouseChance = UnityEngine.Random.Range (0, 2);
			if (spouseChance == 0) {
				Citizen spouse = MarriageManager.Instance.CreateSpouse (gatherer);
				//			spouse.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - spouse.age));

				int childChance = UnityEngine.Random.Range (0, 100);
				if (childChance < 25) {

					int age = UnityEngine.Random.Range (0, ((spouse.age - 16) + 1));
					if(spouse.gender == GENDER.MALE){
						age = UnityEngine.Random.Range (0, ((gatherer.age - 16) + 1));
					}
					Citizen child1 = MarriageManager.Instance.MakeBaby (gatherer, spouse, age);

					//				child1.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - child1.age));
					//				child2.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - child2.age));
					//				child3.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - child3.age));


				}
			}

			if (i == 0) {
				HexTile tileForCitizen = this.FindTileForCitizen (gatherer);
				if (tileForCitizen != null) {
					this.OccupyTile (tileForCitizen, gatherer);
				}
			}
		}
	}
	private void CreateInitialUntrainedFamily(){
		for(int i = 0; i < 3; i++){
			GENDER gender = GENDER.MALE;
			int randomGender = UnityEngine.Random.Range (0, 2);
			if(randomGender == 0){
				gender = GENDER.FEMALE;
			}
			Citizen normal = new Citizen (this, UnityEngine.Random.Range (20, 36), gender, 2);
			Citizen father = new Citizen (this, UnityEngine.Random.Range (60, 81), GENDER.MALE, 1);
			Citizen mother = new Citizen (this, UnityEngine.Random.Range (60, 81), GENDER.FEMALE, 1);

			//		father.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), PoliticsPrototypeManager.Instance.year - father.age);
			//		mother.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), PoliticsPrototypeManager.Instance.year - mother.age);
			//		this.kingdom.king.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - this.assignedLord.age));

			father.isDead = true;
			mother.isDead = true;
			this.citizens.Remove(father);
			this.citizens.Remove(mother);

			father.AddChild (normal);
			mother.AddChild (normal);
			normal.AddParents(father, mother);
			MarriageManager.Instance.Marry(father, mother);

			int spouseChance = UnityEngine.Random.Range (0, 2);
			if (spouseChance == 0) {
				Citizen spouse = MarriageManager.Instance.CreateSpouse (normal);
				//			spouse.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - spouse.age));

				int childChance = UnityEngine.Random.Range (0, 100);
				if (childChance < 25) {

					int age = UnityEngine.Random.Range (0, ((spouse.age - 16) + 1));
					if(spouse.gender == GENDER.MALE){
						age = UnityEngine.Random.Range (0, ((normal.age - 16) + 1));
					}
					Citizen child1 = MarriageManager.Instance.MakeBaby (normal, spouse, age);

					//				child1.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - child1.age));
					//				child2.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - child2.age));
					//				child3.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - child3.age));


				}
			}
		}
	}
	private void GenerateInitialTraitsForInitialCitizens(){
		for (int i = 0; i < this.citizens.Count; i++) {
			Citizen currentCitizen = this.citizens[i];
			currentCitizen.behaviorTraits.Clear();
			currentCitizen.skillTraits.Clear();
			currentCitizen.miscTraits.Clear();

			//Generate Behaviour trait
			int firstItem = 1;
			int secondItem = 2;
			for (int j = 0; j < 4; j++) {
				BEHAVIOR_TRAIT[] behaviourPair = new BEHAVIOR_TRAIT[2]{(BEHAVIOR_TRAIT)firstItem, (BEHAVIOR_TRAIT)secondItem};
				int chanceForTrait = UnityEngine.Random.Range (0, 100);
				if (chanceForTrait <= 20) {
					currentCitizen.behaviorTraits.Add (behaviourPair [UnityEngine.Random.Range (0, behaviourPair.Length)]);
				}
				firstItem += 2;
				secondItem += 2;
			}

			int chanceForSkillTraitLength = UnityEngine.Random.Range (0, 100);
			int numOfSkillTraits = 0;
			if (chanceForSkillTraitLength <= 20) {
				numOfSkillTraits = 2;
			} else if (chanceForSkillTraitLength >= 21 && chanceForSkillTraitLength <= 40) {
				numOfSkillTraits = 1;
			}

			//Generate Skill Traits
			List<SKILL_TRAIT> skillTraits = Utilities.GetEnumValues<SKILL_TRAIT>().ToList();
			skillTraits.Remove (SKILL_TRAIT.NONE);
			for (int j = 0; j < numOfSkillTraits; j++) {
				SKILL_TRAIT chosenSkillTrait = skillTraits[UnityEngine.Random.Range(0, skillTraits.Count)];
				currentCitizen.skillTraits.Add (chosenSkillTrait);
				if (numOfSkillTraits > 1) {
					skillTraits.Remove (chosenSkillTrait);
					if (chosenSkillTrait == SKILL_TRAIT.EFFICIENT) {
						skillTraits.Remove (SKILL_TRAIT.INEFFICIENT);
					} else if (chosenSkillTrait == SKILL_TRAIT.INEFFICIENT) {
						skillTraits.Remove (SKILL_TRAIT.EFFICIENT);
					} else if (chosenSkillTrait == SKILL_TRAIT.LAVISH) {
						skillTraits.Remove (SKILL_TRAIT.THRIFTY);
					} else if (chosenSkillTrait == SKILL_TRAIT.THRIFTY) {
						skillTraits.Remove (SKILL_TRAIT.LAVISH);
					}
				}
			}

			int chanceForMiscTraitLength = UnityEngine.Random.Range (0, 100);
			int numOfMiscTraits = 0;
			if (chanceForMiscTraitLength <= 10) {
				numOfMiscTraits = 2;
			} else if (chanceForMiscTraitLength >= 11 && chanceForMiscTraitLength <= 21) {
				numOfMiscTraits = 1;
			}

			//Generate Misc Traits
			List<MISC_TRAIT> miscTraits = Utilities.GetEnumValues<MISC_TRAIT>().ToList();
			miscTraits.Remove (MISC_TRAIT.NONE);
			for (int j = 0; j < numOfMiscTraits; j++) {
				MISC_TRAIT chosenMiscTrait = miscTraits[UnityEngine.Random.Range(0, miscTraits.Count)];
				currentCitizen.miscTraits.Add (chosenMiscTrait);
			}
		}
	}

	internal void ExpandToThisCity(List<Citizen> citizensToOccupyCity){
		this.citizens.AddRange(citizensToOccupyCity);
		this.UpdateUnownedNeighbourTiles();

		citizensToOccupyCity = citizensToOccupyCity.OrderBy(x => x.prestige).ToList();
		//Assign Governor
		citizensToOccupyCity.Last().AssignRole(ROLE.GOVERNOR);

		//Assign Farmer
		citizensToOccupyCity[0].AssignRole(ROLE.FOODIE);
		HexTile tileForFarmer = this.purchasableFoodTiles[0];
		this.PurchaseTile(tileForFarmer);
		this.OccupyTile(tileForFarmer, citizensToOccupyCity[0]);

		//Assign Gatherer
		citizensToOccupyCity[1].AssignRole(ROLE.GATHERER);
		if (this.purchasableBasicTiles.Count > 0) {
			HexTile tileForGatherer = this.purchasableBasicTiles [0];
			this.PurchaseTile(tileForGatherer);
			this.OccupyTile(tileForGatherer, citizensToOccupyCity [1]);
		} else {
			HexTile tileForGatherer = this.purchasabletilesWithUnneededResource[0];
			this.PurchaseTile(tileForGatherer);
			this.OccupyTile(tileForGatherer, citizensToOccupyCity [1]);
		}
	}

	protected void DonateCitizensToExpansion(Expansion expansionEvent, Kingdom kingdomToExpand){
		if (this.kingdom.id != kingdomToExpand.id) {
			return;
		}
		int donationLimit = 3;
		List<Citizen> untrainedCitizens = this.GetCitizensWithRole(ROLE.UNTRAINED).Where(x => x.age >= 16 && (x.spouse != null && x.spouse.role != ROLE.GOVERNOR)).ToList();
		if (untrainedCitizens.Count > 0) {
			if (untrainedCitizens.Count < donationLimit) {
				for (int i = 0; i < untrainedCitizens.Count; i++) {
					if (untrainedCitizens[i].dependentChildren.Count > 0) {
						untrainedCitizens.AddRange(untrainedCitizens[i].dependentChildren);
					}
					if (untrainedCitizens[i].spouse != null) {
						untrainedCitizens.Add(untrainedCitizens[i].spouse);
						untrainedCitizens[i].spouse.Unemploy();
					}
				}
				expansionEvent.AddCitizensToExpansion(untrainedCitizens);
			} else {
				List<Citizen> citizensToJoinExpansion = new List<Citizen>(){
					untrainedCitizens[0],
					untrainedCitizens[1],
					untrainedCitizens[2]
				};

				for (int i = 0; i < citizensToJoinExpansion.Count; i++) {
					if (citizensToJoinExpansion[i].dependentChildren.Count > 0) {
						citizensToJoinExpansion.AddRange(citizensToJoinExpansion[i].dependentChildren);
					}
					if (citizensToJoinExpansion[i].spouse != null) {
						citizensToJoinExpansion.Add(citizensToJoinExpansion[i].spouse);
						citizensToJoinExpansion[i].spouse.Unemploy();
					}
				}
				expansionEvent.AddCitizensToExpansion(citizensToJoinExpansion);
			}
		}

	}

	internal void TriggerStarvation(){
		if(this.isStarving){
			int deathChance = UnityEngine.Random.Range (0, 100);
			if(deathChance < 5){
				int youngestAge = this.citizens.Min (x => x.age);
				List<Citizen> youngestCitizens = this.citizens.Where(x => x.age == youngestAge).ToList();
				youngestCitizens [UnityEngine.Random.Range(0, youngestCitizens.Count)].DeathByStarvation ();
			}
		}
	}

	protected void PurchaseTile(HexTile tileToBuy){
		tileToBuy.isOwned = true;
		this.ownedTiles.Add(tileToBuy);
		Debug.Log ("Bought Tile: " + tileToBuy.name);
		tileToBuy.GetComponent<SpriteRenderer>().color = Color.clear;
		this.UpdateUnownedNeighbourTiles();
	}

	protected void OccupyTile(HexTile tileToOccupy, Citizen citizenToOccupy){
		tileToOccupy.isOccupied = true;
		tileToOccupy.OccupyTile(citizenToOccupy);
		citizenToOccupy.workLocation = tileToOccupy;
		citizenToOccupy.currentLocation = tileToOccupy;
		this.UpdateResourceProduction();
		if (citizenToOccupy.role == ROLE.TRADER) {
			((Trader)citizenToOccupy.assignedRole).AssignTask();
		}
	}

	protected HexTile FindTileForCitizen(Citizen citizen){
		for (int i = 0; i < this.ownedTiles.Count; i++) {
			if (!this.ownedTiles [i].isOccupied) {
				RESOURCE resourceToCheck = RESOURCE.NONE;
				if (this.ownedTiles [i].specialResource != RESOURCE.NONE) {
					resourceToCheck = this.ownedTiles[i].specialResource;
				} else {
					resourceToCheck = this.ownedTiles[i].defaultResource;
				}

				if (citizen.role == ROLE.FOODIE) {
					if (Utilities.GetBaseResourceType (resourceToCheck) == BASE_RESOURCE_TYPE.FOOD) {
						return this.ownedTiles [i];
					}
				} else if (citizen.role == ROLE.GATHERER) {
					if (Utilities.GetBaseResourceType (resourceToCheck) == BASE_RESOURCE_TYPE.STONE ||
					    Utilities.GetBaseResourceType (resourceToCheck) == BASE_RESOURCE_TYPE.WOOD) {
						return this.ownedTiles [i];
					}
				} else if (citizen.role == ROLE.GENERAL) {
					if (this.ownedTiles [i].specialResource == RESOURCE.NONE) {
						return this.ownedTiles [i];
					}
				}
			}
		}
		return null;
	}

	internal void CheckBattleMidwayCity(){
		
	}

	protected void CityEverydayTurnActions(){
		this.ProduceResources();
		this.AttemptToPerformAction();
		this.UpdateTradeManager();
	}

	protected void UpdateTradeManager(){
		if (this.tradeManager.lastMonthUpdated != GameManager.Instance.month) {
			this.tradeManager.UpdateNeededResources();
		}
	}

	#region Resource Production
	protected void UpdateResourceProduction(){
		this.allResourceProduction = new int[7];
		for (int i = 0; i < this.citizens.Count; i++) {
			if(this.citizens[i].assignedRole != null && this.citizens[i].workLocation != null){
				int[] citizenProduction = this.citizens[i].assignedRole.GetResourceProduction();
				for (int j = 0; j < citizenProduction.Length; j++) {
					this.allResourceProduction[j] += citizenProduction[j];
				}
			}
		}
	}

	protected void ProduceResources(){
		this.sustainability = this.allResourceProduction[0] + tradeManager.sustainabilityBuff;
		this.lumberCount += this.allResourceProduction[1];
		this.stoneCount += this.allResourceProduction[2];
		this.manaStoneCount += this.allResourceProduction[3];
		this.mithrilCount += this.allResourceProduction[4];
		this.cobaltCount += this.allResourceProduction[5];
		this.goldCount += this.allResourceProduction[6];

		if (this.citizens.Count > this.sustainability) {
			this.isStarving = true;
			this.TriggerStarvation();
		}
	}
	#endregion

	internal void CheckCityDeath(){
		if (this.citizens.Count <= 0) {
			this.isDead = true;
			this.hexTile.city = null;
			EventManager.Instance.onCityEverydayTurnActions.RemoveListener (CityEverydayTurnActions);
			EventManager.Instance.onCitizenDiedEvent.RemoveListener (CheckCityDeath);
			EventManager.Instance.onRecruitCitizensForExpansion.RemoveListener(DonateCitizensToExpansion);
//			EventManager.Instance.onCitizenDiedEvent.RemoveListener (UpdateHextileRoles);
		}
	}

	protected void AttemptToPerformAction(){
		if (pendingTask.Count > 0) {
			//Perform pending task first
			if (AttemptToPerformPendingTask ()) {
				return;
			}
		}

		bool isInMilitarization = EventManager.Instance.GetEventsOfTypePerKingdom(this.kingdom, EVENT_TYPES.MILITARIZATION).Count > 0;

		if (this.AllTilesAreOccupied()) {
			//Pick which of the three resources to produce, based on what tiles are available
			if (this.isStarving) {
				if (this.purchasableFoodTiles.Count > 0) {
					if (this.BuyTileFromList (BASE_RESOURCE_TYPE.FOOD, this.purchasableFoodTiles)) {
						return;
					}
				}
			}

			if (!this.IsProducingResource (this.kingdom.basicResource) && this.purchasableBasicTiles.Count > 0) {
				if (this.BuyTileFromList (this.kingdom.basicResource, this.purchasableBasicTiles)) {
					return;
				}
			} else {
				if (!this.IsProducingResource (this.kingdom.rareResource) && this.purchasableRareTiles.Count > 0) {
					if (this.BuyTileFromList (this.kingdom.rareResource, this.purchasableRareTiles)) {
						return;
					}
				} 
			}

			if (this.IsProducingResource (BASE_RESOURCE_TYPE.FOOD) && this.IsProducingResource (this.kingdom.basicResource) &&
				this.IsProducingResource (this.kingdom.rareResource)) {

				List<HexTile> tilesWithNoSpecialResource = this.allUnownedNeighbours.Where (x => x.specialResource == RESOURCE.NONE).ToList();
				if (this.BuyTileFromList (BASE_RESOURCE_TYPE.NONE, tilesWithNoSpecialResource)) {
					return;
				}
			}

			//buy additional basic or rare
			Debug.Log(GameManager.Instance.month + "/" + GameManager.Instance.week + " - " + this.kingdom.name + ": Attempt to buy additional basic resource tile");
			if (this.BuyTileFromList (this.kingdom.basicResource, this.purchasableBasicTiles)) {
				return;
			}

			Debug.Log(GameManager.Instance.month + "/" + GameManager.Instance.week + " - " + this.kingdom.name + ": Attempt to buy additional rare resource tile");
			if (this.BuyTileFromList (this.kingdom.rareResource, this.purchasableRareTiles)) {
				return;
			}

			//buy tile with unneeded resource
			Debug.Log(GameManager.Instance.month + "/" + GameManager.Instance.week + " - " + this.kingdom.name + ": Attempt to buy additional unneeded resource tile");
			this.BuyTileFromList (BASE_RESOURCE_TYPE.NONE, this.purchasabletilesWithUnneededResource, true);

		} else {
			//Train citizen
			List<HexTile> pendingTiles = new List<HexTile> ();
			if (this.isStarving) {
				pendingTiles.AddRange (this.unoccupiedOwnedTiles.Where (x => !x.isOccupied && x.roleIntendedForTile == ROLE.FOODIE).ToList ());
			}
			if (!this.IsProducingResource (this.kingdom.basicResource)) {
				pendingTiles.AddRange (this.unoccupiedOwnedTiles.Where (x => !x.isOccupied && x.roleIntendedForTile == ROLE.GATHERER).ToList ());
			}
			if (!this.IsProducingResource (this.kingdom.rareResource)) {
				pendingTiles.AddRange (this.unoccupiedOwnedTiles.Where (x => !x.isOccupied && x.roleIntendedForTile == ROLE.MINER).ToList ());
			}

			pendingTiles.AddRange(this.unoccupiedOwnedTiles);
			pendingTiles = pendingTiles.Distinct().ToList();


			if (this.HasEnoughResourcesForAction (GetCitizenCreationCostPerType (pendingTiles [0].roleIntendedForTile))) {
				List<Citizen> unemployedCitizens = this.GetCitizensWithRole (ROLE.UNTRAINED).Where (x => x.age >= 16).ToList ();
				if (unemployedCitizens.Count > 0) {
					this.AdjustResources (GetCitizenCreationCostPerType (pendingTiles [0].roleIntendedForTile));
					unemployedCitizens [0].AssignRole (pendingTiles [0].roleIntendedForTile);
					this.OccupyTile (pendingTiles [0], unemployedCitizens [0]);
				} else {
					if (this.pendingTask.Count <= 0) {
						this.pendingTask.Add (CITY_TASK.ASSIGN_CITIZEN, pendingTiles [0]);
					}
				}
			} else {
				if (this.pendingTask.Count <= 0) {
					this.pendingTask.Add (CITY_TASK.ASSIGN_CITIZEN, pendingTiles [0]);
				}
			}
		}
	}

	protected bool AttemptToPerformPendingTask(){
		HexTile hexTileConcerned = this.pendingTask [this.pendingTask.Keys.ElementAt (0)];
		if (this.pendingTask.Keys.ElementAt (0) == CITY_TASK.PURCHASE_TILE) {
			//Purchase Tile
			List<Resource> purchaseTileCost = new List<Resource>(){ 
				new Resource(BASE_RESOURCE_TYPE.GOLD, 500)
			};
			if (this.HasEnoughResourcesForAction (purchaseTileCost.ToList())) {
				this.AdjustResources (purchaseTileCost);
				this.PurchaseTile(hexTileConcerned);
				this.pendingTask.Clear();
				return true;
			}
			return false;
		} else {
			//Assign Citizen
			if (this.HasEnoughResourcesForAction (GetCitizenCreationCostPerType (hexTileConcerned.roleIntendedForTile))) {
				List<Citizen> unemployedCitizens = this.GetCitizensWithRole (ROLE.UNTRAINED).Where (x => x.age >= 16).ToList ();
				if (unemployedCitizens.Count > 0) {
					this.AdjustResources (GetCitizenCreationCostPerType (hexTileConcerned.roleIntendedForTile));
					unemployedCitizens [0].AssignRole (hexTileConcerned.roleIntendedForTile);
					this.OccupyTile (hexTileConcerned, unemployedCitizens [0]);
					this.pendingTask.Clear ();
					return true;
				}
				return false;
			}
		}
		return false;
	}

	protected bool BuyTileFromList(BASE_RESOURCE_TYPE resourceToProduce, List<HexTile> choices, bool forUnneededResource = false){
		if (choices.Count <= 0) {
			Debug.Log (GameManager.Instance.month + "/" + GameManager.Instance.week + " - " + this.kingdom.name + ": Could not buy tile, because there are no available " + resourceToProduce.ToString () + " tiles.");
			return false;
		}
		HexTile tileToPurchase = choices [UnityEngine.Random.Range (0, choices.Count)];
		if (resourceToProduce != BASE_RESOURCE_TYPE.NONE) {
			tileToPurchase.roleIntendedForTile = Utilities.GetRoleThatProducesResource (resourceToProduce);
		} else {
			if (forUnneededResource) {
				if (tileToPurchase.specialResource == RESOURCE.NONE) {
					tileToPurchase.roleIntendedForTile = Utilities.GetRoleThatProducesResource (Utilities.GetBaseResourceType(tileToPurchase.defaultResource));
				} else {
					tileToPurchase.roleIntendedForTile = Utilities.GetRoleThatProducesResource (Utilities.GetBaseResourceType(tileToPurchase.specialResource));
				}
			} else {
				//Choose non-producing role
				tileToPurchase.roleIntendedForTile = this.GetNonProducingRoleToCreate ();
			}
		}

		List<Resource> purchaseTileCost = new List<Resource>(){ 
			new Resource(BASE_RESOURCE_TYPE.GOLD, 500)
		};
		if (this.HasEnoughResourcesForAction (purchaseTileCost.ToList())) {
			this.AdjustResources (purchaseTileCost);
			this.PurchaseTile (tileToPurchase);
			return true;
		} else {
			if (this.pendingTask.Count <= 0) {
				Debug.Log (GameManager.Instance.month + "/" + GameManager.Instance.week + " - " + this.kingdom.name + ": setting task as pending " + tileToPurchase.tileName.ToString ());
				this.pendingTask.Add (CITY_TASK.PURCHASE_TILE, tileToPurchase);
			}
		}
		return false;

	}

	protected ROLE GetNonProducingRoleToCreate(){
		int traderCount = this.GetCitizensWithRole(ROLE.TRADER).Count;
		int generalCount = this.GetCitizensWithRole(ROLE.GENERAL).Count;
		int spyCount = this.GetCitizensWithRole(ROLE.SPY).Count;
		int envoyCount = this.GetCitizensWithRole(ROLE.ENVOY).Count;
		int guardianCount = this.GetCitizensWithRole(ROLE.GUARDIAN).Count;

		int traderLimit = this.citizenCreationTable[ROLE.TRADER];
		int generalLimit = this.citizenCreationTable[ROLE.GENERAL];
		int spyLimit = this.citizenCreationTable[ROLE.SPY];
		int envoyLimit = this.citizenCreationTable[ROLE.ENVOY];
		int guardianLimit = this.citizenCreationTable[ROLE.GUARDIAN];

		if (traderCount < traderLimit) {
			return ROLE.TRADER;
		} else if (generalCount < generalLimit) {
			return ROLE.GENERAL;
		} else if (spyCount < spyLimit) {
			return ROLE.SPY;
		} else if (envoyCount < envoyLimit) {
			return ROLE.ENVOY;
		} else if (guardianCount < guardianLimit) {
			return ROLE.GUARDIAN;
		}
		return ROLE.UNTRAINED;
	}

	protected void UpdateCitizenCreationTable(){
		this.citizenCreationTable = Utilities.defaultCitizenCreationTable;
		for (int i = 0; i < this.governor.behaviorTraits.Count; i++) {
			Dictionary<ROLE, int> currentTraitTable = Utilities.citizenCreationTable[this.governor.behaviorTraits[i]];
			for (int j = 0; j < currentTraitTable.Count; j++) {
				ROLE currentRole = currentTraitTable.Keys.ElementAt(j);
				this.citizenCreationTable [currentRole] += currentTraitTable [currentRole];
			}
		}
	}

	protected void UpdateUnownedNeighbourTiles(){
		this.allUnownedNeighbours.Clear ();
		for (int i = 0; i < this.ownedTiles.Count; i++) {
			this.allUnownedNeighbours.AddRange(this.ownedTiles[i].elligibleNeighbourTilesForPurchase);
		}

		this.purchasableFoodTiles = this.allUnownedNeighbours.Where (x => (x.specialResource == RESOURCE.NONE &&
			Utilities.GetBaseResourceType(x.defaultResource) == BASE_RESOURCE_TYPE.FOOD) || (x.specialResource != RESOURCE.NONE &&
				Utilities.GetBaseResourceType(x.specialResource) == BASE_RESOURCE_TYPE.FOOD)).ToList();

		this.purchasableBasicTiles = this.allUnownedNeighbours.Where (x => (x.specialResource == RESOURCE.NONE &&
			Utilities.GetBaseResourceType(x.defaultResource) == this.kingdom.basicResource) || (x.specialResource != RESOURCE.NONE &&
				Utilities.GetBaseResourceType(x.specialResource) == this.kingdom.basicResource)).ToList();

		this.purchasableRareTiles = this.allUnownedNeighbours.Where (x => (x.specialResource == RESOURCE.NONE &&
			Utilities.GetBaseResourceType(x.defaultResource) == this.kingdom.rareResource) || (x.specialResource != RESOURCE.NONE &&
				Utilities.GetBaseResourceType(x.specialResource) == this.kingdom.rareResource)).ToList();

		this.purchasabletilesWithUnneededResource = this.allUnownedNeighbours.Where (x => (x.specialResource == RESOURCE.NONE && Utilities.GetBaseResourceType (x.defaultResource) != this.kingdom.basicResource) ||
			(x.specialResource == RESOURCE.NONE && Utilities.GetBaseResourceType (x.defaultResource) != this.kingdom.rareResource) || (x.specialResource != RESOURCE.NONE && Utilities.GetBaseResourceType (x.specialResource) != this.kingdom.basicResource) || 
			(x.specialResource != RESOURCE.NONE && Utilities.GetBaseResourceType (x.specialResource) != this.kingdom.rareResource)).ToList();

		this.purchasabletilesWithUnneededResource = this.purchasabletilesWithUnneededResource.Except(this.purchasableFoodTiles).ToList();


	}

	#region boolean functions
	private bool AllTilesAreOccupied(){
		for (int i = 0; i < this.ownedTiles.Count; i++) {
			if (!this.ownedTiles [i].isOccupied) {
				return false;
			}
		}
		return true;
	}

	internal int GetCityArmyStrength(){
		int total = 0;
		for(int i = 0; i < this.citizens.Count; i++){
			if(this.citizens[i].assignedRole != null && this.citizens[i].role == ROLE.GENERAL){
				total += ((General)this.citizens [i].assignedRole).GetArmyHP ();
			}
		}
		return total;
	}
	internal int GetTotalAttackerStrength(){
		int total = 0;
		for(int i = 0; i < this.incomingGenerals.Count; i++){
			if(((General)this.incomingGenerals[i].assignedRole).assignedCampaign == CAMPAIGN.OFFENSE){
				total += ((General)this.incomingGenerals[i].assignedRole).GetArmyHP ();
			}
		}
		return total;	
	}
	internal List<Citizen> GetAllGenerals(){
		List<Citizen> allGenerals = new List<Citizen> ();
		for(int i = 0; i < this.citizens.Count; i++){
			if(this.citizens[i].assignedRole != null && this.citizens[i].role == ROLE.GENERAL){
				allGenerals.Add (this.citizens [i]);
			}
		}
		return allGenerals;
	}
		
	internal bool IsProducingResource(BASE_RESOURCE_TYPE baseResourceType){
		bool result = false;
		switch (baseResourceType) {
		case BASE_RESOURCE_TYPE.FOOD:
			result = this.allResourceProduction[0] > 0 ? true : false;
			break;
		case BASE_RESOURCE_TYPE.WOOD:
			result = this.allResourceProduction[1] > 0 ? true : false;
			break;
		case BASE_RESOURCE_TYPE.STONE:
			result = this.allResourceProduction[2] > 0 ? true : false;
			break;
		case BASE_RESOURCE_TYPE.MANA_STONE:
			result = this.allResourceProduction[3] > 0 ? true : false;
			break;
		case BASE_RESOURCE_TYPE.MITHRIL:
			result = this.allResourceProduction[4] > 0 ? true : false;
			break;
		case BASE_RESOURCE_TYPE.COBALT:
			result = this.allResourceProduction[5] > 0 ? true : false;
			break;
		default:
			break;
		}
		return result;
	}
	#endregion

	#region utlity functions
	internal List<Citizen> GetCitizensWithRole(ROLE role){
		List<Citizen> citizensWithRole = new List<Citizen>();
		for (int i = 0; i < this.citizens.Count; i++) {
			if (this.citizens [i].role == role) {
				citizensWithRole.Add (this.citizens [i]);
			}
		}
		return citizensWithRole;
	}

	protected void AdjustResources(List<Resource> resource, bool reduce = true){
		int currentResourceQuantity = 0;
		Debug.Log ("Cost is: ");
		for (int i = 0; i < resource.Count; i++) {
			Debug.Log (resource[i].resourceType.ToString() + " " + resource[i].resourceQuantity.ToString());
		}

		for(int i = 0; i < resource.Count; i++){
			currentResourceQuantity = resource [i].resourceQuantity;
			if (reduce) {
				currentResourceQuantity *= -1;
			}
			AdjustResourceCount (resource [i].resourceType, currentResourceQuantity);
		}
	}

	internal void AdjustResourceCount(BASE_RESOURCE_TYPE resourceType, int amount){
		switch (resourceType) {
		case BASE_RESOURCE_TYPE.FOOD:
			this.sustainability += amount;
			break;
		case BASE_RESOURCE_TYPE.GOLD:
			this.goldCount += amount;
			break;
		case BASE_RESOURCE_TYPE.WOOD:
			this.lumberCount += amount;
			break;
		case BASE_RESOURCE_TYPE.STONE:
			this.stoneCount += amount;
			break;
		case BASE_RESOURCE_TYPE.MANA_STONE:
			this.manaStoneCount += amount;
			break;
		case BASE_RESOURCE_TYPE.MITHRIL:
			this.mithrilCount += amount;
			break;
		case BASE_RESOURCE_TYPE.COBALT:
			this.cobaltCount += amount;
			break;
		}
	}

	internal bool HasEnoughResourcesForAction(List<Resource> resourceCost){
		for (int i = 0; i < resourceCost.Count; i++) {
			if (this.GetResourceAmountPerType (resourceCost [i].resourceType) < resourceCost [i].resourceQuantity) {
				return false;
			}
		}
		return true;
	}

	protected int GetResourceAmountPerType(BASE_RESOURCE_TYPE resourceType){
		if (resourceType == BASE_RESOURCE_TYPE.FOOD) {
			return this.sustainability;
		} else if (resourceType == BASE_RESOURCE_TYPE.WOOD) {
			return this.lumberCount;
		} else if (resourceType == BASE_RESOURCE_TYPE.STONE) {
			return this.stoneCount;
		} else if (resourceType == BASE_RESOURCE_TYPE.MITHRIL) {
			return this.mithrilCount;
		} else if (resourceType == BASE_RESOURCE_TYPE.MANA_STONE) {
			return this.manaStoneCount;
		} else if (resourceType == BASE_RESOURCE_TYPE.COBALT) {
			return this.cobaltCount;
		} else if (resourceType == BASE_RESOURCE_TYPE.GOLD) {
			return this.goldCount;
		}
		return -1;
	}

	protected List<Resource> GetCitizenCreationCostPerType(ROLE role){
		if(role == ROLE.UNTRAINED){
			return null;
		}

		int numOfCitizensOfSameType = this.GetCitizensWithRole(role).Count;
	
		List<Resource> citizenCreationCosts;
		BASE_RESOURCE_TYPE creationResource = BASE_RESOURCE_TYPE.NONE;

		switch (role) {
		case ROLE.FOODIE:
		case ROLE.GATHERER:
		case ROLE.MINER:
		case ROLE.TRADER:
			creationResource = this.kingdom.basicResource;
			break;
		case ROLE.SPY:
		case ROLE.GUARDIAN:
		case ROLE.ENVOY:
			creationResource = this.kingdom.rareResource;
			break;
		case ROLE.GENERAL:
			citizenCreationCosts = new List<Resource>(){
				new Resource (BASE_RESOURCE_TYPE.GOLD, 500),
				new Resource (this.kingdom.basicResource, 40)
			};
			return citizenCreationCosts;
		}


		int creationResourceCount = 80 + (20 * numOfCitizensOfSameType);
		int goldCost = 200;

		if(numOfCitizensOfSameType == 0) {
			citizenCreationCosts = new List<Resource>() {
				new Resource (BASE_RESOURCE_TYPE.GOLD, goldCost),
			};
		} else {
			citizenCreationCosts = new List<Resource>(){
				new Resource (BASE_RESOURCE_TYPE.GOLD, goldCost),
				new Resource (creationResource, creationResourceCount)
			};
		}

		return citizenCreationCosts;
	}
	#endregion

	internal void RemoveCitizenFromCity(Citizen citizenToRemove){
		this.citizens.Remove (citizenToRemove);
		citizenToRemove.workLocation = null;
		citizenToRemove.city = null;
		citizenToRemove.currentLocation = null;
	}

	internal void AddCitizenToCity(Citizen citizenToAdd){
		this.citizens.Add(citizenToAdd);
		citizenToAdd.city = this;
		citizenToAdd.currentLocation = this.hexTile;
	}

	internal void AssignNewGovernor(){
		Citizen newGovernor = GetCitizenWithHighestPrestige ();
		this.governor.isGovernor = false;

		newGovernor.assignedRole = null;
		newGovernor.role = ROLE.UNTRAINED;
		newGovernor.isGovernor = true;
		this.governor = newGovernor;

	}
	internal Citizen GetCitizenWithHighestPrestige(){
		List<Citizen> prestigeCitizens = new List<Citizen> ();
		int maxPrestige = this.citizens.Where (x => !x.isGovernor && !x.isDead).Max (x => x.prestige);
		for(int i = 0; i < this.citizens.Count; i++){
			if(this.citizens[i].prestige == maxPrestige){
				if(!this.citizens[i].isDead && !this.citizens[i].isGovernor){
					prestigeCitizens.Add (this.citizens [i]);
				}
			}
		}

		return prestigeCitizens [UnityEngine.Random.Range (0, prestigeCitizens.Count)];
	}
}
