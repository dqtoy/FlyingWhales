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

	public City(HexTile hexTile, Kingdom kingdom){
		this.id = Utilities.SetID(this);
		this.name = "City" + this.id.ToString();
		this.hexTile = hexTile;
		this.kingdom = kingdom;
		this.governor = null;
		this.ownedTiles = new List<HexTile>();
		this.citizens = new List<Citizen>();
		this.cityHistory = string.Empty;
		this.hasKing = false;
		this.isStarving = false;
		this.isDead = false;
		this.allResourceProduction = new int[]{ 0, 0, 0, 0, 0, 0, 0 };

		this.CreateInitialFamilies();

		EventManager.Instance.onCitizenTurnActions.AddListener (CityEverydayTurnActions);
		EventManager.StartListening ("CityTurnActions", CityTurnActions);
		EventManager.StartListening ("CitizenDied", CheckCityDeath);
	}


	/*
	 * Initialize City With Initial Citizens aka. Families
	 * */
	internal void CreateInitialFamilies(){
		BuyInitialTiles ();
		CreateInitialRoyalFamily ();
		CreateInitialGovernorFamily ();
		CreateInitialGeneralFamily ();
		CreateInitialFoodProducerFamily ();
		CreateInitialGathererFamily ();
		CreateInitialUntrainedFamily ();
		GenerateInitialTraitsForInitialCitizens ();
	}

	private void BuyInitialTiles(){
		Debug.Log ("Buying tiles for kingdom: " + this.kingdom.name + " " + this.kingdom.race.ToString());
		List<HexTile> allAdjacentTiles = this.hexTile.elligibleTilesForPurchase.ToList();
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

		//buy normal tile without speacial resource
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

//		int foodTiles = 0;
//		int baseTile = 0;
//		int normalTile = 0;
//		for (int i = 0; i < allAdjacentTiles.Count; i++) {
//			if (foodTiles == 2 && baseTile == 1 && normalTile == 1) {
//				break;
//			}
//			HexTile currentHexTile = allAdjacentTiles[i];
//			//Check if tile is good for food, base resource or normal
//			if (currentHexTile.specialResource == RESOURCE.NONE) {
//				//only check default resource
//				if (baseTile <= 0 && Utilities.GetBaseResourceType (currentHexTile.defaultResource) == this.kingdom.basicResource) {
//					//buy tile for basic resource
//					baseTile++;
//					this.PurchaseTile(currentHexTile);
//				} else if(foodTiles != 2 && Utilities.GetBaseResourceType (currentHexTile.defaultResource) == BASE_RESOURCE_TYPE.FOOD){
//					foodTiles++;
//					this.PurchaseTile(currentHexTile);
//				} else if(normalTile <= 0){
//					normalTile++;
//					this.PurchaseTile(currentHexTile);
//				}
//			} else {
//				if (baseTile <= 0 && Utilities.GetBaseResourceType (currentHexTile.specialResource) == this.kingdom.basicResource) {
//					//buy tile for basic resource
//					baseTile++;
//					this.PurchaseTile(currentHexTile);
//				} else if(foodTiles != 2 && Utilities.GetBaseResourceType (currentHexTile.specialResource) == BASE_RESOURCE_TYPE.FOOD){
//					foodTiles++;
//					this.PurchaseTile(currentHexTile);
//				}
//			}
//		}
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

		general.role = ROLE.FOODIE;
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
			int firstItem = 0;
			int secondItem = 1;
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
			for (int j = 0; j < numOfMiscTraits; j++) {
				MISC_TRAIT chosenMiscTrait = miscTraits[UnityEngine.Random.Range(0, miscTraits.Count)];
				currentCitizen.miscTraits.Add (chosenMiscTrait);
			}
		}
	}


	internal void CityTurnActions(){
		ProduceResources();
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
		this.ownedTiles.Add(tileToBuy);
		tileToBuy.GetComponent<SpriteRenderer>().color = Color.magenta;
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
				}
			}
		}
		return null;
	}
	internal void CheckBattleMidwayCity(){
		
	}
	protected void CityEverydayTurnActions(){
		ProduceResources ();
		Starvation ();
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
			EventManager.StopListening ("CityTurnActions", CityTurnActions);
			EventManager.StopListening ("CitizenDied", CheckCityDeath);
		}
	}
}
