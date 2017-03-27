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
	[HideInInspector]public List<Citizen> citizens;
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

	[Space(5)]
	public bool isStarving;
	public bool isDead;
	//generals
	//incoming generals

	protected Dictionary<ROLE, int> citizenCreationTable;
	protected Dictionary<CITY_TASK, HexTile> pendingTask;
	protected List<HexTile> unoccupiedOwnedTiles{
		get{ return this.ownedTiles.Where (x => !x.isOccupied).ToList();}
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
		this.connectedCities = new List<City> ();
		this.cityHistory = string.Empty;
		this.hasKing = false;
		this.isStarving = false;
		this.isDead = false;
		this.allResourceProduction = new int[]{ 0, 0, 0, 0, 0, 0, 0 };
		this.citizenCreationTable = Utilities.defaultCitizenCreationTable;
		this.pendingTask = new Dictionary<CITY_TASK, HexTile>();
		this.CreateInitialFamilies();

		EventManager.Instance.onCityEverydayTurnActions.AddListener (CityEverydayTurnActions);
		EventManager.Instance.onCitizenDiedEvent.AddListener (CheckCityDeath);
		EventManager.Instance.onCitizenDiedEvent.AddListener (UpdateHextileRoles);
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
		this.kingdom.king = new Citizen (this, UnityEngine.Random.Range (20, 36), gender, 2);
		Citizen father = new Citizen (this, UnityEngine.Random.Range (60, 81), GENDER.MALE, 1);
		Citizen mother = new Citizen (this, UnityEngine.Random.Range (60, 81), GENDER.FEMALE, 1);

		this.kingdom.king.isKing = true;
		this.kingdom.king.isDirectDescendant = true;
		father.isDirectDescendant = true;
		mother.isDirectDescendant = true;
//		father.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), PoliticsPrototypeManager.Instance.year - father.age);
//		mother.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), PoliticsPrototypeManager.Instance.year - mother.age);
//		this.kingdom.king.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - this.assignedLord.age));

		father.AddChild (this.kingdom.king);
		mother.AddChild (this.kingdom.king);
		this.kingdom.king.AddParents(father, mother);
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
		//		father.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), PoliticsPrototypeManager.Instance.year - father.age);
		//		mother.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), PoliticsPrototypeManager.Instance.year - mother.age);
		//		this.kingdom.king.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - this.assignedLord.age));

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

			producer.role = ROLE.FOODIE;
			producer.assignedRole = new Foodie (producer);
			//		father.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), PoliticsPrototypeManager.Instance.year - father.age);
			//		mother.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), PoliticsPrototypeManager.Instance.year - mother.age);
			//		this.kingdom.king.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - this.assignedLord.age));

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

			gatherer.role = ROLE.GATHERER;
			gatherer.assignedRole = new Gatherer (gatherer);
			//		father.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), PoliticsPrototypeManager.Instance.year - father.age);
			//		mother.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), PoliticsPrototypeManager.Instance.year - mother.age);
			//		this.kingdom.king.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - this.assignedLord.age));

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
	}

	protected void OccupyTile(HexTile tileToOccupy, Citizen citizenToOccupy){
		tileToOccupy.isOccupied = true;
		tileToOccupy.OccupyTile(citizenToOccupy);
		citizenToOccupy.workLocation = tileToOccupy;
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
//		this.AttemptToPerformAction();
	}

	#region Resource Production
	protected void UpdateResourceProduction(){
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
		this.sustainability = this.allResourceProduction[0];
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
			EventManager.Instance.onCitizenDiedEvent.RemoveListener (UpdateHextileRoles);
		}
	}

	protected void UpdateHextileRoles(){
		for (int i = 0; i < this.unoccupiedOwnedTiles.Count; i++) {
			
		}
	}

	protected void AttemptToPerformAction(){
		if (pendingTask.Count > 0) {
			//Perform pending task first
			if (AttemptToPerformPendingTask ()) {
				return;
			}
		}

		if (this.AllTilesAreOccupied()) {
			List<HexTile> allTilesAvailableForPurchase = new List<HexTile>();
			for (int i = 0; i < this.ownedTiles.Count; i++) {
				allTilesAvailableForPurchase.AddRange(this.ownedTiles[i].elligibleNeighbourTilesForPurchase);
			}
			List<HexTile> foodTilesForPurchase = allTilesAvailableForPurchase.Where (x => (x.specialResource == RESOURCE.NONE &&
				Utilities.GetBaseResourceType(x.defaultResource) == BASE_RESOURCE_TYPE.FOOD) || x.specialResource != RESOURCE.NONE &&
				Utilities.GetBaseResourceType(x.specialResource) == BASE_RESOURCE_TYPE.FOOD).ToList();

			List<HexTile> basicResourceTilesForPurchase = allTilesAvailableForPurchase.Where (x => (x.specialResource == RESOURCE.NONE &&
				Utilities.GetBaseResourceType(x.defaultResource) == this.kingdom.basicResource) || x.specialResource != RESOURCE.NONE &&
				Utilities.GetBaseResourceType(x.specialResource) == this.kingdom.basicResource).ToList();

			List<HexTile> rareResourceTilesForPurchase = allTilesAvailableForPurchase.Where (x => (x.specialResource == RESOURCE.NONE &&
				Utilities.GetBaseResourceType(x.defaultResource) == this.kingdom.rareResource) || x.specialResource != RESOURCE.NONE &&
				Utilities.GetBaseResourceType(x.specialResource) == this.kingdom.rareResource).ToList();

			Dictionary<BASE_RESOURCE_TYPE, List<HexTile>> resourceTilesToBuy = new Dictionary<BASE_RESOURCE_TYPE, List<HexTile>> () {
				{BASE_RESOURCE_TYPE.FOOD, foodTilesForPurchase},
				{this.kingdom.basicResource, basicResourceTilesForPurchase},
				{this.kingdom.rareResource, rareResourceTilesForPurchase}
			};

			//Pick which of the three resources to produce, based on what tiles are available
			if (this.isStarving) {
				if (foodTilesForPurchase.Count > 0) {
					this.BuyTileFromList (BASE_RESOURCE_TYPE.FOOD, foodTilesForPurchase);
					return;
				}
			}

			if (!this.IsProducingResource (this.kingdom.basicResource) && basicResourceTilesForPurchase.Count > 0) {
				this.BuyTileFromList (this.kingdom.basicResource, basicResourceTilesForPurchase);
			} else {
				if (!this.IsProducingResource (this.kingdom.rareResource) && rareResourceTilesForPurchase.Count > 0) {
					this.BuyTileFromList (this.kingdom.rareResource, rareResourceTilesForPurchase);
				} else {
					if (this.IsProducingResource (BASE_RESOURCE_TYPE.FOOD) && this.IsProducingResource (this.kingdom.basicResource) &&
					    this.IsProducingResource (this.kingdom.rareResource)) {
						List<HexTile> tilesWithNoSpecialResource = allTilesAvailableForPurchase.Where (x => x.specialResource == RESOURCE.NONE).ToList();
						this.BuyTileFromList(BASE_RESOURCE_TYPE.NONE, tilesWithNoSpecialResource);
					} else {
						for (int i = 0; i < resourceTilesToBuy.Count; i++) {
							if (resourceTilesToBuy[resourceTilesToBuy.Keys.ElementAt(i)].Count <= 0) {
								this.BuyTileFromList (resourceTilesToBuy.Keys.ElementAt(i), resourceTilesToBuy [resourceTilesToBuy.Keys.ElementAt(i)]);
							}
						}
					}
				}
			}
		} else {
			//Train citizen
			List<HexTile> pendingTiles = new List<HexTile> ();
			if (this.isStarving) {
				pendingTiles.AddRange (this.unoccupiedOwnedTiles.Where (x => x.roleIntendedForTile == ROLE.FOODIE).ToList ());
			}
			if (!this.IsProducingResource (this.kingdom.basicResource)) {
				pendingTiles.AddRange (this.unoccupiedOwnedTiles.Where (x => x.roleIntendedForTile == ROLE.GATHERER).ToList ());
			}
			if (!this.IsProducingResource (this.kingdom.rareResource)) {
				pendingTiles.AddRange (this.unoccupiedOwnedTiles.Where (x => x.roleIntendedForTile == ROLE.MINER).ToList ());
			}

			pendingTiles.AddRange(this.unoccupiedOwnedTiles);
			pendingTiles = pendingTiles.Distinct().ToList();


			if (this.HasEnoughResourcesForAction (GetCitizenCreationCostPerType (pendingTiles [0].roleIntendedForTile).ToList())) {
				List<Citizen> unemployedCitizens = this.GetCitizensWithRole (ROLE.UNTRAINED).Where (x => x.age >= 16).ToList ();
				if (unemployedCitizens.Count > 0) {
					this.AdjustResources (GetCitizenCreationCostPerType (hexTile.roleIntendedForTile));
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
			Resource[] purchaseTileCost = new Resource[]{ 
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
			if (this.HasEnoughResourcesForAction (GetCitizenCreationCostPerType (hexTile.roleIntendedForTile).ToList())) {
				List<Citizen> unemployedCitizens = this.GetCitizensWithRole (ROLE.UNTRAINED).Where (x => x.age >= 16).ToList ();
				if (unemployedCitizens.Count > 0) {
					this.AdjustResources (GetCitizenCreationCostPerType (hexTile.roleIntendedForTile));
					unemployedCitizens [0].AssignRole (hexTile.roleIntendedForTile);
					this.OccupyTile (hexTile, unemployedCitizens [0]);
					this.pendingTask.Clear ();
					return true;
				}
				return false;
			}
		}
		return false;
	}

	protected void BuyTileFromList(BASE_RESOURCE_TYPE resourceToProduce, List<HexTile> choices){
		HexTile tileToPurchase = choices [UnityEngine.Random.Range (0, choices.Count)];
		if (resourceToProduce != BASE_RESOURCE_TYPE.NONE) {
			tileToPurchase.roleIntendedForTile = Utilities.GetRoleThatProducesResource (resourceToProduce);
		} else {
			//Choose non-producing role
			tileToPurchase.roleIntendedForTile = this.GetNonProducingRoleToCreate();
		}

		Resource[] purchaseTileCost = new Resource[]{ 
			new Resource(BASE_RESOURCE_TYPE.GOLD, 500)
		};
		if (this.HasEnoughResourcesForAction (purchaseTileCost.ToList())) {
			this.AdjustResources (purchaseTileCost);
			this.PurchaseTile (tileToPurchase);
		} else {
			if (this.pendingTask.Count <= 0) {
				this.pendingTask.Add (CITY_TASK.PURCHASE_TILE, tileToPurchase);
			}
		}

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



	#region boolean functions
	private bool AllTilesAreOccupied(){
		for (int i = 0; i < this.ownedTiles.Count; i++) {
			if (!this.ownedTiles [i].isOccupied) {
				return false;
			}
		}
		return true;
	}

	internal bool CheckCityConnectivity (List<City> connections){
		for(int i = 0; i < this.connectedCities.Count; i++){
			for(int j = 0; j < connections.Count; j++){
				if(this.connectedCities[i].id == connections[j].id){
					return true;
				}
			}
		}
		return false;
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
		
	private bool IsProducingResource(BASE_RESOURCE_TYPE baseResourceType){
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

	protected void AdjustResources(Resource[] resource, bool reduce = true){
		int currentResourceQuantity = 0;
		for(int i = 0; i < resource.Length; i++){
			currentResourceQuantity = resource [i].resourceQuantity;
			if (reduce) {
				currentResourceQuantity *= -1;
			}
			AdjustResourceCount (resource [i].resourceType, currentResourceQuantity);
		}
	}

	protected void AdjustResourceCount(BASE_RESOURCE_TYPE resourceType, int amount){
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

	protected bool HasEnoughResourcesForAction(List<Resource> resourceCost){
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

	protected Resource[] GetCitizenCreationCostPerType(ROLE role){
		if(role == ROLE.UNTRAINED){
			return null;
		}

		int numOfCitizensOfSameType = this.GetCitizensWithRole(role).Count;
	
		Resource[] citizenCreationCosts;
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
			citizenCreationCosts = new Resource[]{
				new Resource (BASE_RESOURCE_TYPE.GOLD, 500),
				new Resource (this.kingdom.basicResource, 40)
			};
			return citizenCreationCosts;
		}


		int creationResourceCount = 80 + (20 * numOfCitizensOfSameType);
		int goldCost = 200;

		if(numOfCitizensOfSameType == 0) {
			citizenCreationCosts = new Resource[] {
				new Resource (BASE_RESOURCE_TYPE.GOLD, goldCost),
			};
		} else {
			citizenCreationCosts = new Resource[]{
				new Resource (BASE_RESOURCE_TYPE.GOLD, goldCost),
				new Resource (creationResource, creationResourceCount)
			};
		}

		return citizenCreationCosts;
	}
	#endregion
}
