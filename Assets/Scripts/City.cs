using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class City{

	public int id;
	public string name;
	public HexTile hexTile;
	public Kingdom kingdom;
	public List<HexTile> ownedTiles;
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
	public int[] allResourceProduction;

	[Space(5)]
	public bool isStarving;

	//generals
	//incoming generals

	public City(HexTile hexTile, Kingdom kingdom){
		this.id = Utilities.SetID(this);
		this.name = "City" + this.id.ToString();
		this.hexTile = hexTile;
		this.kingdom = kingdom;
		this.ownedTiles = new List<HexTile>();
		this.citizens = new List<Citizen>();
		this.cityHistory = string.Empty;
		this.hasKing = false;

		this.CreateInitialFamilies ();

		EventManager.StartListening ("Starvation", Starvation);
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
		CreateInitialUntrainedFamily ();
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
	private void CreateInitialRoyalFamily(){
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
				childAges.RemoveAt (age1);
				int age2 = UnityEngine.Random.Range (0, childAges.Count);
				childAges.RemoveAt (age2);
				int age3 = UnityEngine.Random.Range (0, childAges.Count);
				childAges.RemoveAt (age3);


				Citizen child1 = MarriageManager.Instance.MakeBaby (this.kingdom.king, this.kingdom.king.spouse, childAges[age1]);
				Citizen child2 = MarriageManager.Instance.MakeBaby (this.kingdom.king, this.kingdom.king.spouse, childAges[age2]);
				Citizen child3 = MarriageManager.Instance.MakeBaby (this.kingdom.king, this.kingdom.king.spouse, childAges[age3]);

//				child1.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - child1.age));
//				child2.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - child2.age));
//				child3.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - child3.age));


			} else if (childChance >= 25 && childChance < 50) {
				
				int age1 = UnityEngine.Random.Range (0, childAges.Count);
				childAges.RemoveAt (age1);
				int age2 = UnityEngine.Random.Range (0, childAges.Count);
				childAges.RemoveAt (age2);

				Citizen child1 = MarriageManager.Instance.MakeBaby (this.kingdom.king, this.kingdom.king.spouse, childAges[age1]);
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
				childAges.RemoveAt (age1);
				int age2 = UnityEngine.Random.Range (0, childAges.Count);
				childAges.RemoveAt (age2);
				int age3 = UnityEngine.Random.Range (0, childAges.Count);
				childAges.RemoveAt (age3);


				Citizen child1 = MarriageManager.Instance.MakeBaby (governor, spouse, childAges[age1]);
				Citizen child2 = MarriageManager.Instance.MakeBaby (governor, spouse, childAges[age2]);
				Citizen child3 = MarriageManager.Instance.MakeBaby (governor, spouse, childAges[age3]);

				//				child1.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - child1.age));
				//				child2.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - child2.age));
				//				child3.AssignBirthday ((MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length)), UnityEngine.Random.Range (1, 5), (PoliticsPrototypeManager.Instance.year - child3.age));


			} else if (childChance >= 25 && childChance < 50) {

				int age1 = UnityEngine.Random.Range (0, childAges.Count);
				childAges.RemoveAt (age1);
				int age2 = UnityEngine.Random.Range (0, childAges.Count);
				childAges.RemoveAt (age2);

				Citizen child1 = MarriageManager.Instance.MakeBaby (governor, spouse, childAges[age1]);
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
			producer.assignedRole = new Foodie ();
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
			gatherer.assignedRole = new Gatherer ();
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


	internal void Starvation(){
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
}
