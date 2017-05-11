using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Panda;

[System.Serializable]
public class City{

	public int id;
	public string name;
	public HexTile hexTile;
	public Kingdom kingdom;
	public Citizen governor;
	public List<City> adjacentCities;
	public List<HexTile> ownedTiles;
	public List<General> incomingGenerals;
	public List<Citizen> citizens;
	public List<History> cityHistory;
	public bool hasKing;

	[Space(10)] //Resources
	public int sustainability;
	public int lumberCount;
	public int stoneCount;
	public int manaStoneCount;
	public int mithrilCount;
	public int cobaltCount;
	public int goldCount;
	public int currentGrowth;
	public int dailyGrowth;
	public int maxGrowth;
	public int maxGeneralHP;
	public int goldProduction;
	public int totalCitizenConsumption;
	public List<RESOURCE> bonusResources;
//	public TradeManager tradeManager;

	[Space(5)]
	public IsActive isActive;
	public bool isStarving;
	public bool isDead;


	internal Dictionary<ROLE, int> citizenCreationTable;
	internal List<HexTile> borderTiles;
	protected List<ROLE> creatableRoles;



	protected List<HexTile> unoccupiedOwnedTiles{
		get{ return this.ownedTiles.Where (x => !x.isOccupied).ToList();}
	}

	protected List<HexTile> structures{
		get{ return this.ownedTiles.Where (x => x.isOccupied && !x.isHabitable).ToList();}
	}

	public List<Citizen> elligibleBachelorettes{
		get{ return this.citizens.Where(x => x.age >= 16 && x.gender == GENDER.FEMALE && !x.isMarried && !x.isDead).ToList();}
	}

	public List<Citizen> elligibleBachelors{
		get{ return this.citizens.Where(x => x.age >= 16 && x.gender == GENDER.MALE && !x.isMarried && !x.isDead).ToList();}
	}

	public List<HexTile> adjacentHabitableTiles{
		get{ return this.hexTile.connectedTiles.Where(x => !x.isOccupied).ToList();}
	}

	public City(HexTile hexTile, Kingdom kingdom){
		this.id = Utilities.SetID(this);
		this.hexTile = hexTile;
		this.kingdom = kingdom;
		this.name = RandomNameGenerator.Instance.GenerateCityName(this.kingdom.race);
		this.governor = null;
		this.adjacentCities = new List<City>();
		this.ownedTiles = new List<HexTile>();
		this.incomingGenerals = new List<General> ();
		this.citizens = new List<Citizen>();
		this.cityHistory = new List<History>();
		this.isActive = new IsActive (false);
		this.hasKing = false;
		this.isStarving = false;
		this.isDead = false;
		this.bonusResources = new List<RESOURCE>();
//		this.tradeManager = new TradeManager(this, this.kingdom);
		this.citizenCreationTable = Utilities.defaultCitizenCreationTable;
		this.creatableRoles = new List<ROLE>();
		this.borderTiles = new List<HexTile>();


		this.hexTile.isOccupied = true;
		this.hexTile.isOccupiedByCityID = this.id;
		this.ownedTiles.Add(this.hexTile);
//		this.hexTile.ShowCitySprite();
		this.UpdateBorderTiles();
//		this.CreateInitialFamilies();

		EventManager.Instance.onCityEverydayTurnActions.AddListener(CityEverydayTurnActions);
		EventManager.Instance.onCitizenDiedEvent.AddListener(CheckCityDeath);

		this.cityHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "City " + this.name + " was founded.", HISTORY_IDENTIFIER.NONE));
	}


	/*
	 * Initialize City With Initial Citizens aka. Families
	 * */
	internal void CreateInitialFamilies(bool hasRoyalFamily = true){
//		BuyInitialTiles ();
		if(hasRoyalFamily){
			this.hasKing = true;
			CreateInitialRoyalFamily ();
		}
		CreateInitialGovernorFamily ();
//		CreateInitialFoodProducerFamily ();
//		CreateInitialGathererFamily ();
//		CreateInitialGeneralFamily ();
//		CreateInitialUntrainedFamily ();
		GenerateInitialTraitsForInitialCitizens ();
		UpdateResourceProduction ();


		for (int i = 0; i < this.citizens.Count; i++) {
			this.citizens[i].UpdatePrestige();
		}
	}

//	private void BuyInitialTiles(){
//		Debug.Log ("========Buying tiles for kingdom: " + this.kingdom.name + " " + this.kingdom.race.ToString() + "========");
//		List<HexTile> allAdjacentTiles = this.hexTile.elligibleNeighbourTilesForPurchase.OrderBy(x => x.specialResource == RESOURCE.NONE).ToList();
//
//		//Buy Food Tiles
//		int foodCounter = 0;
//		for (int i = 0; i < allAdjacentTiles.Count; i++) {
//			if (foodCounter >= 2) {
//				break;
//			}
//			HexTile currentHexTile = allAdjacentTiles[i];
//			if (this.ownedTiles.Contains (currentHexTile)) {
//				continue;
//			}
//
//			if (currentHexTile.specialResource == RESOURCE.NONE) {
//				if (Utilities.GetBaseResourceType (currentHexTile.defaultResource) == BASE_RESOURCE_TYPE.FOOD) {
//					foodCounter++;
//					Debug.Log ("Bought tile " + currentHexTile.name + " for food"); 
//					this.PurchaseTile(currentHexTile);
////					currentHexTile.roleIntendedForTile = ROLE.FOODIE;
//				}
//			} else {
//				if (Utilities.GetBaseResourceType (currentHexTile.specialResource) == BASE_RESOURCE_TYPE.FOOD) {
//					foodCounter++;
//					Debug.Log ("Bought tile " + currentHexTile.name + " for food"); 
//					this.PurchaseTile(currentHexTile);
////					currentHexTile.roleIntendedForTile = ROLE.FOODIE;
//				}
//			}
//		}
//			
//		//buy base resource tile
//		allAdjacentTiles = this.hexTile.elligibleNeighbourTilesForPurchase.OrderBy(x => x.specialResource == RESOURCE.NONE).ToList();
//		bool hasTileForBasicResource = false;
//
//		for (int i = 0; i < allAdjacentTiles.Count; i++) {
//			HexTile currentHexTile = allAdjacentTiles[i];
//			if (currentHexTile.specialResource == RESOURCE.NONE) {
//				if (Utilities.GetBaseResourceType (currentHexTile.defaultResource) == this.kingdom.basicResource) {
//					hasTileForBasicResource = true;
//				}
//			} else {
//				if (Utilities.GetBaseResourceType (currentHexTile.specialResource) == this.kingdom.basicResource) {
//					hasTileForBasicResource = true;
//				}
//			}
//		}
//
//		for (int i = 0; i < allAdjacentTiles.Count; i++) {
//			HexTile currentHexTile = allAdjacentTiles[i];
//			if (this.ownedTiles.Contains (currentHexTile)) {
//				continue;
//			}
//			if (hasTileForBasicResource) {
//				if (currentHexTile.specialResource == RESOURCE.NONE) {
//					BASE_RESOURCE_TYPE baseResType = Utilities.GetBaseResourceType (currentHexTile.defaultResource);
//					if (baseResType == this.kingdom.basicResource) {
//						Debug.Log ("Bought tile " + currentHexTile.name + " for base resource"); 
//						this.PurchaseTile (currentHexTile);
////						currentHexTile.roleIntendedForTile = ROLE.GATHERER;
//						break;
//					}
//				} else {
//					BASE_RESOURCE_TYPE baseResType = Utilities.GetBaseResourceType (currentHexTile.specialResource);
//					if (baseResType == this.kingdom.basicResource) {
//						Debug.Log ("Bought tile " + currentHexTile.name + " for base resource"); 
//						this.PurchaseTile (currentHexTile);
////						currentHexTile.roleIntendedForTile = ROLE.GATHERER;
//						break;
//					}
//				}
//			} else {
//				if (currentHexTile.specialResource == RESOURCE.NONE) {
//					BASE_RESOURCE_TYPE baseResType = Utilities.GetBaseResourceType (currentHexTile.defaultResource);
//					if (baseResType == BASE_RESOURCE_TYPE.STONE || baseResType == BASE_RESOURCE_TYPE.WOOD) {
//						Debug.Log ("Bought tile " + currentHexTile.name + " for base resource"); 
//						this.PurchaseTile (currentHexTile);
////						currentHexTile.roleIntendedForTile = ROLE.GATHERER;
//						break;
//					}
//				} else {
//					BASE_RESOURCE_TYPE baseResType = Utilities.GetBaseResourceType (currentHexTile.specialResource);
//					if (baseResType == BASE_RESOURCE_TYPE.STONE || baseResType == BASE_RESOURCE_TYPE.WOOD) {
//						Debug.Log ("Bought tile " + currentHexTile.name + " for base resource"); 
//						this.PurchaseTile (currentHexTile);
////						currentHexTile.roleIntendedForTile = ROLE.GATHERER;
//						break;
//					}
//				}
//			}
//		}
//
//
//		allAdjacentTiles = this.hexTile.elligibleNeighbourTilesForPurchase.ToList();
//		//buy normal tile without special resource
//		for (int i = 0; i < allAdjacentTiles.Count; i++) {
//			HexTile currentHexTile = allAdjacentTiles[i];
//			if (this.ownedTiles.Contains (currentHexTile)) {
//				continue;
//			}
//
//			if (currentHexTile.specialResource == RESOURCE.NONE) {
//				Debug.Log ("Bought tile " + currentHexTile.name + " for nothing"); 
//				this.PurchaseTile(currentHexTile);
////				currentHexTile.roleIntendedForTile = ROLE.GENERAL;
//				break;
//			}
//		}
//
//	}

	internal Citizen CreateNewKing(){
		GENDER gender = GENDER.MALE;
		int randomGender = UnityEngine.Random.Range (0, 100);
		if(randomGender < 20){
			gender = GENDER.FEMALE;
		}
		Citizen king = new Citizen (this, UnityEngine.Random.Range (20, 36), gender, 2);

		MONTH monthKing = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
		king.AssignBirthday (monthKing, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthKing] + 1), (GameManager.Instance.year - king.age));

		return king;
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

		father.name = RandomNameGenerator.Instance.GenerateRandomName (this.kingdom.race, father.gender);
		mother.name = RandomNameGenerator.Instance.GenerateRandomName (this.kingdom.race, mother.gender);

		king.AssignRole(ROLE.KING);

//		this.kingdom.king.isKing = true;
		this.kingdom.king.isDirectDescendant = true;

		father.isDirectDescendant = true;
		mother.isDirectDescendant = true;
		father.isDead = true;
		mother.isDead = true;

		this.citizens.Remove(father);
		this.citizens.Remove(mother);

		father.UnsubscribeListeners();
		mother.UnsubscribeListeners();

		father.AddChild (this.kingdom.king);
		mother.AddChild (this.kingdom.king);
		king.AddParents(father, mother);

		MONTH monthFather = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
		MONTH monthMother = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
		MONTH monthKing = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));

		father.AssignBirthday (monthFather, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthFather] + 1), GameManager.Instance.year - father.age);
		mother.AssignBirthday (monthMother, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthMother] + 1), GameManager.Instance.year - mother.age);
		this.kingdom.king.AssignBirthday (monthKing, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthKing] + 1), (GameManager.Instance.year - this.kingdom.king.age));

		king.isBusy = true;


		MarriageManager.Instance.Marry(father, mother);

		MONTH monthSibling = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
		MONTH monthSibling2 = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));

		int siblingsChance = UnityEngine.Random.Range (0, 100);
		if(siblingsChance < 25){
			Citizen sibling = MarriageManager.Instance.MakeBaby (father, mother, UnityEngine.Random.Range(0,this.kingdom.king.age));
			Citizen sibling2 = MarriageManager.Instance.MakeBaby (father, mother, UnityEngine.Random.Range(0,this.kingdom.king.age));

			sibling.AssignBirthday (monthSibling, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthSibling] + 1), (GameManager.Instance.year - sibling.age));
			sibling2.AssignBirthday (monthSibling2, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthSibling2] + 1), (GameManager.Instance.year - sibling2.age));

		}else if(siblingsChance >= 25 && siblingsChance < 75){
			Citizen sibling = MarriageManager.Instance.MakeBaby (father, mother, UnityEngine.Random.Range(0,this.kingdom.king.age));
			sibling.AssignBirthday (monthSibling, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthSibling] + 1), (GameManager.Instance.year - sibling.age));
		}


		MONTH monthChild1 = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
		MONTH monthChild2 = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
		MONTH monthChild3 = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));

		int spouseChance = UnityEngine.Random.Range (0, 100);
		if (spouseChance < 80) {
			Citizen spouse = MarriageManager.Instance.CreateSpouse (this.kingdom.king);

			List<int> childAges = Enumerable.Range(0, (spouse.age - 16)).ToList();
			if(spouse.gender == GENDER.MALE){
				childAges = Enumerable.Range(0, (this.kingdom.king.age - 16)).ToList();
			}


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

				child1.AssignBirthday (monthChild1, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthChild1] + 1), (GameManager.Instance.year - child1.age));
				child2.AssignBirthday (monthChild2, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthChild2] + 1), (GameManager.Instance.year - child2.age));
				child3.AssignBirthday (monthChild3, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthChild3] + 1), (GameManager.Instance.year - child3.age));


			} else if (childChance >= 25 && childChance < 50) {
				
				int age1 = UnityEngine.Random.Range (0, childAges.Count);
				Citizen child1 = MarriageManager.Instance.MakeBaby (this.kingdom.king, this.kingdom.king.spouse, childAges[age1]);
				childAges.RemoveAt (age1);

				int age2 = UnityEngine.Random.Range (0, childAges.Count);
				Citizen child2 = MarriageManager.Instance.MakeBaby (this.kingdom.king, this.kingdom.king.spouse, childAges[age2]);

				child1.AssignBirthday (monthChild1, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthChild1] + 1), (GameManager.Instance.year - child1.age));
				child2.AssignBirthday (monthChild2, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthChild2] + 1), (GameManager.Instance.year - child2.age));

			} else if (childChance >= 50 && childChance < 75) {

				int age1 = UnityEngine.Random.Range (0, childAges.Count);

				Citizen child1 = MarriageManager.Instance.MakeBaby (this.kingdom.king, this.kingdom.king.spouse, childAges[age1]);

				child1.AssignBirthday (monthChild1, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthChild1] + 1), (GameManager.Instance.year - child1.age));

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

		father.name = RandomNameGenerator.Instance.GenerateRandomName (this.kingdom.race, father.gender);
		mother.name = RandomNameGenerator.Instance.GenerateRandomName (this.kingdom.race, mother.gender);

		governor.AssignRole(ROLE.GOVERNOR);

		father.AddChild (governor);
		mother.AddChild (governor);
		governor.AddParents(father, mother);

		MONTH monthFather = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
		MONTH monthMother = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
		MONTH monthGovernor = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));

		father.AssignBirthday (monthFather, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthFather] + 1), GameManager.Instance.year - father.age);
		mother.AssignBirthday (monthMother, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthMother] + 1), GameManager.Instance.year - mother.age);
		governor.AssignBirthday (monthGovernor, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthGovernor] + 1), (GameManager.Instance.year - governor.age));

		father.isDead = true;
		mother.isDead = true;
		this.citizens.Remove(father);
		this.citizens.Remove(mother);
		father.UnsubscribeListeners();
		mother.UnsubscribeListeners();

		MarriageManager.Instance.Marry(father, mother);

		governor.isBusy = true;

		this.governor = governor;
		this.UpdateCitizenCreationTable();

		MONTH monthSibling = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
		MONTH monthSibling2 = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));

		int siblingsChance = UnityEngine.Random.Range (0, 100);
		if(siblingsChance < 25){
			Citizen sibling = MarriageManager.Instance.MakeBaby (father, mother, UnityEngine.Random.Range(0,governor.age));
			Citizen sibling2 = MarriageManager.Instance.MakeBaby (father, mother, UnityEngine.Random.Range(0,governor.age));

			sibling.AssignBirthday (monthSibling, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthSibling] + 1), (GameManager.Instance.year - sibling.age));
			sibling2.AssignBirthday (monthSibling2, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthSibling2] + 1), (GameManager.Instance.year - sibling2.age));

		}else if(siblingsChance >= 25 && siblingsChance < 75){
			Citizen sibling = MarriageManager.Instance.MakeBaby (father, mother, UnityEngine.Random.Range(0,governor.age));
			sibling.AssignBirthday (monthSibling, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthSibling] + 1), (GameManager.Instance.year - sibling.age));
		}

		MONTH monthChild1 = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
		MONTH monthChild2 = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
		MONTH monthChild3 = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));

		int spouseChance = UnityEngine.Random.Range (0, 100);
		if (spouseChance < 80) {
			Citizen spouse = MarriageManager.Instance.CreateSpouse (governor);
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

				child1.AssignBirthday (monthChild1, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthChild1] + 1), (GameManager.Instance.year - child1.age));
				child2.AssignBirthday (monthChild2, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthChild2] + 1), (GameManager.Instance.year - child2.age));
				child3.AssignBirthday (monthChild3, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthChild3] + 1), (GameManager.Instance.year - child3.age));


			} else if (childChance >= 25 && childChance < 50) {

				int age1 = UnityEngine.Random.Range (0, childAges.Count);
				Citizen child1 = MarriageManager.Instance.MakeBaby (governor, spouse, childAges[age1]);
				childAges.RemoveAt (age1);

				int age2 = UnityEngine.Random.Range (0, childAges.Count);
				Citizen child2 = MarriageManager.Instance.MakeBaby (governor, spouse, childAges[age2]);

				child1.AssignBirthday (monthChild1, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthChild1] + 1), (GameManager.Instance.year - child1.age));
				child2.AssignBirthday (monthChild2, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthChild2] + 1), (GameManager.Instance.year - child2.age));

			} else if (childChance >= 50 && childChance < 75) {

				int age1 = UnityEngine.Random.Range (0, childAges.Count);

				Citizen child1 = MarriageManager.Instance.MakeBaby (governor, spouse, childAges[age1]);

				child1.AssignBirthday (monthChild1, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthChild1] + 1), (GameManager.Instance.year - child1.age));

			}
		}

		this.cityHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, governor.name + " became the new Governor of " + this.name + ".", HISTORY_IDENTIFIER.NONE));

	}

//	private void CreateInitialGeneralFamily(){
//		GENDER gender = GENDER.MALE;
//		int randomGender = UnityEngine.Random.Range (0, 2);
//		if(randomGender == 0){
//			gender = GENDER.FEMALE;
//		}
//		Citizen general = new Citizen (this, UnityEngine.Random.Range (20, 36), gender, 2);
//		Citizen father = new Citizen (this, UnityEngine.Random.Range (60, 81), GENDER.MALE, 1);
//		Citizen mother = new Citizen (this, UnityEngine.Random.Range (60, 81), GENDER.FEMALE, 1);
//
//		father.name = RandomNameGenerator.Instance.GenerateRandomName (this.kingdom.race, father.gender);
//		mother.name = RandomNameGenerator.Instance.GenerateRandomName (this.kingdom.race, mother.gender);
//
//		general.role = ROLE.GENERAL;
//		general.assignedRole = new General (general);
//
//		father.AddChild (general);
//		mother.AddChild (general);
//		general.AddParents(father, mother);
//
//		MONTH monthFather = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
//		MONTH monthMother = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
//		MONTH monthGeneral = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
//
//		father.AssignBirthday (monthFather, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthFather] + 1), GameManager.Instance.year - father.age);
//		mother.AssignBirthday (monthMother, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthMother] + 1), GameManager.Instance.year - mother.age);
//		general.AssignBirthday (monthGeneral, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthGeneral] + 1), (GameManager.Instance.year - general.age));
//
//		father.isDead = true;
//		mother.isDead = true;
//		this.citizens.Remove(father);
//		this.citizens.Remove(mother);
//		father.UnsubscribeListeners();
//		mother.UnsubscribeListeners();
//
//		MarriageManager.Instance.Marry(father, mother);
//
//		int spouseChance = UnityEngine.Random.Range (0, 2);
//		if (spouseChance == 0) {
//			Citizen spouse = MarriageManager.Instance.CreateSpouse (general);
//
//			int childChance = UnityEngine.Random.Range (0, 100);
//			if (childChance < 25) {
//
//				int age = UnityEngine.Random.Range (0, ((spouse.age - 16) + 1));
//				if(spouse.gender == GENDER.MALE){
//					age = UnityEngine.Random.Range (0, ((general.age - 16) + 1));
//				}
//				Citizen child1 = MarriageManager.Instance.MakeBaby (general, spouse, age);
//				MONTH monthChild1 = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
//				child1.AssignBirthday (monthChild1, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthChild1] + 1), (GameManager.Instance.year - child1.age));
//			}
//		}
//		HexTile tileForCitizen = this.FindTileForCitizen (general);
//		if (tileForCitizen != null) {
//			this.OccupyTile (tileForCitizen, general);
//		} else {
//			Debug.LogError (this.name + ": NO TILE FOR GENERAL!");
//		}
//	
//	}

//	private void CreateInitialFoodProducerFamily(){
//		for(int i = 0; i < 3; i++){
//			GENDER gender = GENDER.MALE;
//			int randomGender = UnityEngine.Random.Range (0, 2);
//			if(randomGender == 0){
//				gender = GENDER.FEMALE;
//			}
//			Citizen producer = new Citizen (this, UnityEngine.Random.Range (20, 36), gender, 2);
//			Citizen father = new Citizen (this, UnityEngine.Random.Range (60, 81), GENDER.MALE, 1);
//			Citizen mother = new Citizen (this, UnityEngine.Random.Range (60, 81), GENDER.FEMALE, 1);
//
//			father.name = RandomNameGenerator.Instance.GenerateRandomName (this.kingdom.race, father.gender);
//			mother.name = RandomNameGenerator.Instance.GenerateRandomName (this.kingdom.race, mother.gender);
//
//			producer.AssignRole (ROLE.FOODIE);
//
//			father.AddChild (producer);
//			mother.AddChild (producer);
//			producer.AddParents(father, mother);
//
//			MONTH monthFather = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
//			MONTH monthMother = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
//			MONTH monthProducer = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
//
//			father.AssignBirthday (monthFather, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthFather] + 1), GameManager.Instance.year - father.age);
//			mother.AssignBirthday (monthMother, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthMother] + 1), GameManager.Instance.year - mother.age);
//			producer.AssignBirthday (monthProducer, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthProducer] + 1), (GameManager.Instance.year - producer.age));
//
//			father.isDead = true;
//			mother.isDead = true;
//			this.citizens.Remove(father);
//			this.citizens.Remove(mother);
//			father.UnsubscribeListeners();
//			mother.UnsubscribeListeners();
//
//			MarriageManager.Instance.Marry(father, mother);
//
//			int spouseChance = UnityEngine.Random.Range (0, 2);
//			if (spouseChance == 0) {
//				Citizen spouse = MarriageManager.Instance.CreateSpouse (producer);
//
//				int childChance = UnityEngine.Random.Range (0, 100);
//				if (childChance < 25) {
//
//					int age = UnityEngine.Random.Range (0, ((spouse.age - 16) + 1));
//					if(spouse.gender == GENDER.MALE){
//						age = UnityEngine.Random.Range (0, ((producer.age - 16) + 1));
//					}
//					Citizen child1 = MarriageManager.Instance.MakeBaby (producer, spouse, age);
//					MONTH monthChild1 = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
//					child1.AssignBirthday (monthChild1, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthChild1] + 1), (GameManager.Instance.year - child1.age));
//				}
//			}
//
//			if (i == 0 || i == 1) {
//				HexTile tileForCitizen = this.FindTileForCitizen (producer);
//				if (tileForCitizen != null) {
//					this.OccupyTile (tileForCitizen, producer);
//				} else {
//					Debug.LogError (this.name + ": NO TILE FOR FOODIE!");
//				}
//			}
//		}
//	}

//	private void CreateInitialGathererFamily(){
//		for(int i = 0; i < 2; i++){
//			GENDER gender = GENDER.MALE;
//			int randomGender = UnityEngine.Random.Range (0, 2);
//			if(randomGender == 0){
//				gender = GENDER.FEMALE;
//			}
//			Citizen gatherer = new Citizen (this, UnityEngine.Random.Range (20, 36), gender, 2);
//			Citizen father = new Citizen (this, UnityEngine.Random.Range (60, 81), GENDER.MALE, 1);
//			Citizen mother = new Citizen (this, UnityEngine.Random.Range (60, 81), GENDER.FEMALE, 1);
//
//			father.name = RandomNameGenerator.Instance.GenerateRandomName (this.kingdom.race, father.gender);
//			mother.name = RandomNameGenerator.Instance.GenerateRandomName (this.kingdom.race, mother.gender);
//
//			gatherer.AssignRole (ROLE.GATHERER);
//
//			father.AddChild (gatherer);
//			mother.AddChild (gatherer);
//			gatherer.AddParents(father, mother);
//
//			MONTH monthFather = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
//			MONTH monthMother = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
//			MONTH monthGatherer = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
//
//			father.AssignBirthday (monthFather, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthFather] + 1), GameManager.Instance.year - father.age);
//			mother.AssignBirthday (monthMother, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthMother] + 1), GameManager.Instance.year - mother.age);
//			gatherer.AssignBirthday (monthGatherer, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthGatherer] + 1), (GameManager.Instance.year - gatherer.age));
//
//			father.isDead = true;
//			mother.isDead = true;
//			this.citizens.Remove(father);
//			this.citizens.Remove(mother);
//			father.UnsubscribeListeners();
//			mother.UnsubscribeListeners();
//		
//			MarriageManager.Instance.Marry(father, mother);
//
//			int spouseChance = UnityEngine.Random.Range (0, 2);
//			if (spouseChance == 0) {
//				Citizen spouse = MarriageManager.Instance.CreateSpouse (gatherer);
//
//				int childChance = UnityEngine.Random.Range (0, 100);
//				if (childChance < 25) {
//
//					int age = UnityEngine.Random.Range (0, ((spouse.age - 16) + 1));
//					if(spouse.gender == GENDER.MALE){
//						age = UnityEngine.Random.Range (0, ((gatherer.age - 16) + 1));
//					}
//					Citizen child1 = MarriageManager.Instance.MakeBaby (gatherer, spouse, age);
//					MONTH monthChild1 = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
//					child1.AssignBirthday (monthChild1, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthChild1] + 1), (GameManager.Instance.year - child1.age));
//
//				}
//			}
//
//			if (i == 0) {
//				HexTile tileForCitizen = this.FindTileForCitizen (gatherer);
//				if (tileForCitizen != null) {
//					this.OccupyTile (tileForCitizen, gatherer);
//				} else {
//					Debug.LogError (this.name + ": NO TILE FOR GATHERER!");
//				}
//			}
//		}
//	}

//	private void CreateInitialUntrainedFamily(){
//		for(int i = 0; i < 3; i++){
//			GENDER gender = GENDER.MALE;
//			int randomGender = UnityEngine.Random.Range (0, 2);
//			if(randomGender == 0){
//				gender = GENDER.FEMALE;
//			}
//			Citizen normal = new Citizen (this, UnityEngine.Random.Range (20, 36), gender, 2);
//			Citizen father = new Citizen (this, UnityEngine.Random.Range (60, 81), GENDER.MALE, 1);
//			Citizen mother = new Citizen (this, UnityEngine.Random.Range (60, 81), GENDER.FEMALE, 1);
//
//			father.name = RandomNameGenerator.Instance.GenerateRandomName (this.kingdom.race, father.gender);
//			mother.name = RandomNameGenerator.Instance.GenerateRandomName (this.kingdom.race, mother.gender);
//
//			father.AddChild (normal);
//			mother.AddChild (normal);
//			normal.AddParents(father, mother);
//
//			MONTH monthFather = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
//			MONTH monthMother = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
//			MONTH monthNormal = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
//
//			father.AssignBirthday (monthFather, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthFather] + 1), GameManager.Instance.year - father.age);
//			mother.AssignBirthday (monthMother, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthMother] + 1), GameManager.Instance.year - mother.age);
//			normal.AssignBirthday (monthNormal, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthNormal] + 1), (GameManager.Instance.year - normal.age));
//
//			father.isDead = true;
//			mother.isDead = true;
//			this.citizens.Remove(father);
//			this.citizens.Remove(mother);
//			father.UnsubscribeListeners();
//			mother.UnsubscribeListeners();
//
//			MarriageManager.Instance.Marry(father, mother);
//
//			int spouseChance = UnityEngine.Random.Range (0, 2);
//			if (spouseChance == 0) {
//				Citizen spouse = MarriageManager.Instance.CreateSpouse (normal);
//
//				int childChance = UnityEngine.Random.Range (0, 100);
//				if (childChance < 25) {
//
//					int age = UnityEngine.Random.Range (0, ((spouse.age - 16) + 1));
//					if(spouse.gender == GENDER.MALE){
//						age = UnityEngine.Random.Range (0, ((normal.age - 16) + 1));
//					}
//					Citizen child1 = MarriageManager.Instance.MakeBaby (normal, spouse, age);
//					MONTH monthChild1 = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
//					child1.AssignBirthday (monthChild1, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthChild1] + 1), (GameManager.Instance.year - child1.age));
//
//				}
//			}
//		}
//	}

	private void GenerateInitialTraitsForInitialCitizens(){
		for (int i = 0; i < this.citizens.Count; i++) {
			Citizen currentCitizen = this.citizens[i];
			currentCitizen.behaviorTraits.Clear();
//			currentCitizen.skillTraits.Clear();
//			currentCitizen.miscTraits.Clear();

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

//			//Generate Skill Traits
//			List<SKILL_TRAIT> skillTraits = Utilities.GetEnumValues<SKILL_TRAIT>().ToList();
//			skillTraits.Remove (SKILL_TRAIT.NONE);
//			for (int j = 0; j < numOfSkillTraits; j++) {
//				SKILL_TRAIT chosenSkillTrait = skillTraits[UnityEngine.Random.Range(0, skillTraits.Count)];
//				currentCitizen.skillTraits.Add (chosenSkillTrait);
//				if (numOfSkillTraits > 1) {
//					skillTraits.Remove (chosenSkillTrait);
//					if (chosenSkillTrait == SKILL_TRAIT.EFFICIENT) {
//						skillTraits.Remove (SKILL_TRAIT.INEFFICIENT);
//					} else if (chosenSkillTrait == SKILL_TRAIT.INEFFICIENT) {
//						skillTraits.Remove (SKILL_TRAIT.EFFICIENT);
//					} else if (chosenSkillTrait == SKILL_TRAIT.LAVISH) {
//						skillTraits.Remove (SKILL_TRAIT.THRIFTY);
//					} else if (chosenSkillTrait == SKILL_TRAIT.THRIFTY) {
//						skillTraits.Remove (SKILL_TRAIT.LAVISH);
//					}
//				}
//			}
//
//			int chanceForMiscTraitLength = UnityEngine.Random.Range (0, 100);
//			int numOfMiscTraits = 0;
//			if (chanceForMiscTraitLength <= 10) {
//				numOfMiscTraits = 2;
//			} else if (chanceForMiscTraitLength >= 11 && chanceForMiscTraitLength <= 21) {
//				numOfMiscTraits = 1;
//			}
//
//			//Generate Misc Traits
//			List<MISC_TRAIT> miscTraits = Utilities.GetEnumValues<MISC_TRAIT>().ToList();
//			miscTraits.Remove (MISC_TRAIT.NONE);
//			for (int j = 0; j < numOfMiscTraits; j++) {
//				MISC_TRAIT chosenMiscTrait = miscTraits[UnityEngine.Random.Range(0, miscTraits.Count)];
//				currentCitizen.miscTraits.Add (chosenMiscTrait);
//				miscTraits.Remove (chosenMiscTrait);
//			}
			currentCitizen.behaviorTraits.Distinct().ToList();
//			currentCitizen.skillTraits.Distinct().ToList();
//			currentCitizen.miscTraits.Distinct().ToList();
		}
		this.UpdateCitizenCreationTable();
	}

	protected void UpdateBorderTiles(){
		for (int i = 0; i < this.borderTiles.Count; i++) {
			this.borderTiles[i].isBorder = false;
			this.borderTiles[i].isBorderOfCityID = 0;
		}
		this.borderTiles.Clear();

		List<HexTile> outmostTiles = new List<HexTile>();
		for (int i = 0; i < this.ownedTiles.Count; i++) {
			HexTile currHexTile = this.ownedTiles[i];
			List<HexTile> currHexTileUnoccupiedNeighbours = currHexTile.AllNeighbours.Where(x => x.elevationType != ELEVATION.WATER && !x.isOccupied && !x.isHabitable).ToList();
			if (currHexTileUnoccupiedNeighbours.Count > 0) {
				outmostTiles.Add (currHexTile);
			}
		}

		for (int i = 0; i < outmostTiles.Count; i++) {
			List<HexTile> possibleBorderTiles = outmostTiles [i].GetTilesInRange(2).Where (x => x.elevationType != ELEVATION.WATER && !x.isOccupied && !x.isHabitable && !x.isBorder).ToList();
			this.borderTiles.AddRange (possibleBorderTiles);
		}
		this.borderTiles.Distinct();
		for (int i = 0; i < this.borderTiles.Count; i++) {
			HexTile currBorderTile = this.borderTiles[i];
			currBorderTile.isBorder = true;
			currBorderTile.isBorderOfCityID = this.id;
		}
	}

	internal void HighlightAllOwnedTiles(float alpha){
		Color color = this.kingdom.kingdomColor;
		color.a = alpha;
		for (int i = 0; i < this.ownedTiles.Count; i++) {
			HexTile currentTile = this.ownedTiles[i];
			currentTile.kingdomColorSprite.color = color;
			currentTile.kingdomColorSprite.gameObject.SetActive(true);
		}

		for (int i = 0; i < this.borderTiles.Count; i++) {
			HexTile currentTile = this.borderTiles[i];
			currentTile.kingdomColorSprite.color = color;
			currentTile.kingdomColorSprite.gameObject.SetActive(true);
		}
	}

	internal void UnHighlightAllOwnedTiles(){
		for (int i = 0; i < this.ownedTiles.Count; i++) {
			HexTile currentTile = this.ownedTiles[i];
			currentTile.kingdomColorSprite.gameObject.SetActive(false);
		}
		for (int i = 0; i < this.borderTiles.Count; i++) {
			HexTile currentTile = this.borderTiles[i];
			currentTile.kingdomColorSprite.gameObject.SetActive(false);
		}
	}

	internal void ExpandToThisCity(List<Citizen> citizensToOccupyCity){
		this.CreateInitialFamilies(false);
		KingdomManager.Instance.UpdateKingdomAdjacency();
		this.kingdom.AddInternationalWarCity (this);
		if (UIManager.Instance.kingdomInfoGO.activeSelf) {
			if (UIManager.Instance.currentlyShowingKingdom != null && UIManager.Instance.currentlyShowingKingdom.id == this.kingdom.id) {
				this.kingdom.HighlightAllOwnedTilesInKingdom();
			}
		}
	}
	internal List<General> GetIncomingAttackers(){
		List<General> incomingAttackers = new List<General> ();
		for(int i = 0; i < this.incomingGenerals.Count; i++){
			if (this.incomingGenerals [i].assignedCampaign.campaignType == CAMPAIGN.OFFENSE && this.incomingGenerals [i].assignedCampaign.targetCity.id == this.id) {
				incomingAttackers.Add (this.incomingGenerals [i]);
			}
		}
		return incomingAttackers;
	}

//	internal void TriggerStarvation(){
//		if(this.isStarving){
//			this.tradeManager.numberOfTimesStarved += 1;
//			int deathChance = UnityEngine.Random.Range (0, 100);
//			if(deathChance < 5){
//				int youngestAge = this.citizens.Min (x => x.age);
//				List<Citizen> youngestCitizens = this.citizens.Where(x => x.age == youngestAge).ToList();
//				youngestCitizens [UnityEngine.Random.Range(0, youngestCitizens.Count)].DeathByStarvation ();
//			}
//		}
//	}

	internal void PurchaseTile(HexTile tileToBuy){
		tileToBuy.isOccupied = true;
		tileToBuy.isOccupiedByCityID = this.id;
		this.ownedTiles.Add(tileToBuy);

		//Set color of tile
		Color color = this.kingdom.kingdomColor;
		color.a = 76.5f/255f;
		tileToBuy.kingdomColorSprite.color = color;
		tileToBuy.kingdomColorSprite.gameObject.SetActive (this.hexTile.kingdomColorSprite.gameObject.activeSelf);
		tileToBuy.ShowOccupiedSprite();

		//Remove tile from any border tile list
		if (tileToBuy.isBorder && tileToBuy.isBorderOfCityID != this.id) {
			tileToBuy.isBorder = false;
			CityGenerator.Instance.GetCityByID(tileToBuy.isBorderOfCityID).borderTiles.Remove(tileToBuy);
		}

		//Update necessary data
		this.UpdateResourceProduction();
		this.UpdateBorderTiles();
		this.UpdateAdjacentCities();
		this.kingdom.UpdateKingdomAdjacency();

		//Show Highlight if kingdom or city is currently highlighted
		if (UIManager.Instance.kingdomInfoGO.activeSelf) {
			if (UIManager.Instance.currentlyShowingKingdom != null && UIManager.Instance.currentlyShowingKingdom.id == this.kingdom.id) {
				this.kingdom.HighlightAllOwnedTilesInKingdom ();
			}
		} else {
			if (UICamera.hoveredObject.GetComponent<CharacterPortrait>() != null && UICamera.hoveredObject.GetComponent<CharacterPortrait>().citizen.city.kingdom.id == this.kingdom.id) {
				this.kingdom.HighlightAllOwnedTilesInKingdom ();
			}
		}

		Debug.Log (GameManager.Instance.month + "/" + GameManager.Instance.days + ": Bought Tile: " + tileToBuy.name);
	}

	protected void CityEverydayTurnActions(){
//		this.UpdateResourceProduction();
		this.ProduceResources();
//		this.hexTile.GetBehaviourTree().Tick();
//		this.AttemptToPerformAction();
//		this.AttemptToIncreaseArmyHP();
//		this.UpdateTradeManager();
	}

//	protected void AttemptToIncreaseArmyHP(){
//		int chance = UnityEngine.Random.Range (0, 100);
//		if (chance < 30) {
//			BASE_RESOURCE_TYPE upgradeResource = BASE_RESOURCE_TYPE.NONE;
//			if (this.kingdom.race == RACE.HUMANS || this.kingdom.race == RACE.CROMADS) {
//				upgradeResource = BASE_RESOURCE_TYPE.STONE;
//			} else if (this.kingdom.race == RACE.ELVES || this.kingdom.race == RACE.MINGONS) {
//				upgradeResource = BASE_RESOURCE_TYPE.WOOD;
//			}
//			List<Resource> increaseArmyHPCost = new List<Resource> () {
//				new Resource (BASE_RESOURCE_TYPE.GOLD, 200),
//				new Resource (upgradeResource, 40)
//			};
//			if (HasEnoughResourcesForAction (increaseArmyHPCost)) {
//				List<General> inactiveGenerals = this.GetCitizensWithRole (ROLE.GENERAL).Where (x => !((General)x.assignedRole).inAction)
//					.Select(x => (General)x.assignedRole).ToList();
//				inactiveGenerals.OrderBy(x => x.GetArmyHP());
//				if (inactiveGenerals.Count > 0) {
//					int hpIncrease = 0;
//					if (this.kingdom.race == RACE.HUMANS) {
//						hpIncrease = 30;
//					} else if (this.kingdom.race == RACE.ELVES) {
//						hpIncrease = 25;
//					} else if (this.kingdom.race == RACE.MINGONS) {
//						hpIncrease = 30;
//					} else if (this.kingdom.race == RACE.CROMADS) {
//						hpIncrease = 40;
//					}
//					inactiveGenerals[0].army.hp += hpIncrease;
//					inactiveGenerals[0].UpdateUI();
//					this.AdjustResources(increaseArmyHPCost);
//					Debug.Log (GameManager.Instance.month + "/" + GameManager.Instance.days + ": Increased army hp of " + inactiveGenerals [0].citizen.name + " by " + hpIncrease.ToString ());
//				}
//			}
//		}
//
//		if (EventManager.Instance.GetEventsOfTypePerKingdom (this.kingdom, EVENT_TYPES.MILITARIZATION).Count > 0) {
//			if (chance < 70) {
//				BASE_RESOURCE_TYPE upgradeResource = BASE_RESOURCE_TYPE.NONE;
//				if (this.kingdom.race == RACE.HUMANS || this.kingdom.race == RACE.CROMADS) {
//					upgradeResource = BASE_RESOURCE_TYPE.STONE;
//				} else if (this.kingdom.race == RACE.ELVES || this.kingdom.race == RACE.MINGONS) {
//					upgradeResource = BASE_RESOURCE_TYPE.WOOD;
//				}
//				List<Resource> increaseArmyHPCost = new List<Resource> () {
//					new Resource (BASE_RESOURCE_TYPE.GOLD, 200),
//					new Resource (upgradeResource, 40)
//				};
//				if (HasEnoughResourcesForAction (increaseArmyHPCost)) {
//					List<General> inactiveGenerals = this.GetCitizensWithRole (ROLE.GENERAL).Where (x => !((General)x.assignedRole).inAction)
//						.Select(x => (General)x.assignedRole).ToList();
//					inactiveGenerals.OrderBy(x => x.GetArmyHP());
//					if (inactiveGenerals.Count > 0) {
//						int hpIncrease = 0;
//						if (this.kingdom.race == RACE.HUMANS) {
//							hpIncrease = 30;
//						} else if (this.kingdom.race == RACE.ELVES) {
//							hpIncrease = 25;
//						} else if (this.kingdom.race == RACE.MINGONS) {
//							hpIncrease = 30;
//						} else if (this.kingdom.race == RACE.CROMADS) {
//							hpIncrease = 40;
//						}
//						inactiveGenerals[0].army.hp += hpIncrease;
//						inactiveGenerals[0].UpdateUI();
//						this.AdjustResources(increaseArmyHPCost);
//						Debug.LogError (GameManager.Instance.month + "/" + GameManager.Instance.week + ": Increased army hp of " + inactiveGenerals [0].citizen.name + " by " + hpIncrease.ToString ());
//					}
//				}
//			}
//		}
//	}

//	protected void UpdateTradeManager(){
//		if (this.tradeManager.lastMonthUpdated != GameManager.Instance.month) {
//			this.tradeManager.UpdateNeededResources();
//		}
//	}

	#region Resource Production
	internal void UpdateResourceProduction(){
//		this.allResourceProduction = new int[8];
//		for (int i = 0; i < this.citizens.Count; i++) {
//			if(this.citizens[i].isBusy && this.citizens[i].role != ROLE.UNTRAINED && this.citizens[i].role != ROLE.GOVERNOR && this.citizens[i].role != ROLE.KING){
//				int[] citizenProduction = this.citizens[i].assignedRole.GetResourceProduction();
//				for (int j = 0; j < citizenProduction.Length; j++) {
//					this.allResourceProduction[j] += citizenProduction[j];
//				}
//			}
//		}
		this.maxGrowth = 100 + (150 * this.structures.Count);
		this.dailyGrowth = 10;
		this.maxGeneralHP = 0;
		this.goldProduction = 20;
		this.bonusResources.Clear ();
		for (int i = 0; i < this.structures.Count; i++) {
			HexTile currentStructure = this.structures [i];
			if (currentStructure.biomeType == BIOMES.GRASSLAND) {
				this.dailyGrowth += 5;
				this.goldProduction += 2;
				this.maxGeneralHP += 100;
			} else if (currentStructure.biomeType == BIOMES.WOODLAND) {
				this.dailyGrowth += 4;
				this.goldProduction += 3;
				this.maxGeneralHP += 300;
			} else if (currentStructure.biomeType == BIOMES.FOREST) {
				this.dailyGrowth += 3;
				this.goldProduction += 3;
				this.maxGeneralHP += 200;
			} else if (currentStructure.biomeType == BIOMES.DESERT) {
				this.dailyGrowth += 1;
				this.goldProduction += 4;
				this.maxGeneralHP += 100;
			} else if (currentStructure.biomeType == BIOMES.TUNDRA) {
				this.dailyGrowth += 2;
				this.goldProduction += 2;
				this.maxGeneralHP += 100;
			} else if (currentStructure.biomeType == BIOMES.SNOW) {
				this.dailyGrowth += 1;
				this.goldProduction += 1;
				this.maxGeneralHP += 200;
			} else if (currentStructure.biomeType == BIOMES.BARE) {
				this.dailyGrowth += 1;
				this.maxGeneralHP += 100;
			}

			RESOURCE currentResource = RESOURCE.NONE;
			if (currentStructure.specialResource != RESOURCE.NONE) {
				currentResource = currentStructure.specialResource;
			}
			if (!this.bonusResources.Contains (currentResource) || currentResource == RESOURCE.GRANITE || currentResource == RESOURCE.CEDAR) {
				this.bonusResources.Add (currentResource);
			}
		}
		this.stoneCount = 0;
		this.lumberCount = 0;
		for (int i = 0; i < this.bonusResources.Count; i++) {
			RESOURCE currentResource = this.bonusResources[i];
			if (currentResource == RESOURCE.GRANITE || currentResource == RESOURCE.SLATE || currentResource == RESOURCE.MARBLE) {
				this.stoneCount += 3;
			} else if (currentResource == RESOURCE.CEDAR || currentResource == RESOURCE.OAK || currentResource == RESOURCE.EBONY) {
				this.lumberCount += 3;
			} else if (currentResource == RESOURCE.CORN || currentResource == RESOURCE.DEER) {
				this.dailyGrowth += 5;
			} else if (currentResource == RESOURCE.WHEAT || currentResource == RESOURCE.RICE ||
				currentResource == RESOURCE.PIG || currentResource == RESOURCE.BEHEMOTH ||
				currentResource == RESOURCE.COBALT) {
				this.dailyGrowth += 10;
			} else if (currentResource == RESOURCE.MANA_STONE) {
				this.dailyGrowth += 15;
			} else if (currentResource == RESOURCE.MITHRIL) {
				this.dailyGrowth += 25;
			}
		}
	}

	protected int GetNumberOfResourcesPerType(BASE_RESOURCE_TYPE resourceType){
		if (resourceType == BASE_RESOURCE_TYPE.FOOD) {
			return this.sustainability;
		} else if (resourceType == BASE_RESOURCE_TYPE.WOOD) {
			return this.lumberCount;
		} else if (resourceType == BASE_RESOURCE_TYPE.STONE) {
			return this.stoneCount;
		} else if (resourceType == BASE_RESOURCE_TYPE.MANA_STONE) {
			return this.manaStoneCount;
		} else if (resourceType == BASE_RESOURCE_TYPE.MITHRIL) {
			return this.mithrilCount;
		} else if (resourceType == BASE_RESOURCE_TYPE.COBALT) {
			return this.cobaltCount;
		} else if (resourceType == BASE_RESOURCE_TYPE.GOLD) {
			return this.goldCount;
		}
		return -1;
	}

	protected void ProduceResources(){
		this.goldCount += this.goldProduction;
		if (this.goldCount > 1000) {
			this.goldCount = 1000;
		}
	}

	internal void UpdateCityConsumption(){
		List<Citizen> citizensWithJobs = this.citizens.Where(x => x.role != ROLE.GOVERNOR && x.role != ROLE.KING && x.role != ROLE.UNTRAINED).ToList();
		this.totalCitizenConsumption = 0;
		for (int i = 0; i < citizensWithJobs.Count; i++) {
			ROLE currentCitizenRole = citizensWithJobs[i].role;
			if (currentCitizenRole == ROLE.TRADER || currentCitizenRole == ROLE.GENERAL) {
				this.totalCitizenConsumption += 2;
			} else {
				this.totalCitizenConsumption += 3;
			}
		}
	}
	#endregion

	internal void CheckCityDeath(){
		if (this.citizens.Count <= 0) {
			this.KillCity ();
		}
	}

	internal ROLE GetNonProducingRoleToCreate(){
		int previousRoleCount = 10;
		ROLE roleToCreate = ROLE.UNTRAINED;

		for (int i = 0; i < this.creatableRoles.Count; i++) {
			ROLE currentRole = this.creatableRoles [i];
			int currentRoleCount = this.GetCitizensWithRole (currentRole).Count;
			int currentRoleLimit = this.citizenCreationTable [currentRole];
//			bool isProducingAllResources = true;
//			List<Resource> citizenCreationCost = GetCitizenCreationCostPerType (currentRole);
//			if (citizenCreationCost != null) {
//				for (int j = 0; j < citizenCreationCost.Count; j++) {
//					if (!this.IsProducingResource (citizenCreationCost [j].resourceType)) {
//						isProducingAllResources = false;
//					}
//				}
//			} else {
//				isProducingAllResources = false;
//			}
			if (currentRoleCount < previousRoleCount && currentRoleCount < currentRoleLimit) {
				roleToCreate = currentRole;
				previousRoleCount = currentRoleCount;
			}
		}
		return roleToCreate;
	}

	internal void UpdateCitizenCreationTable(){
		this.citizenCreationTable = new Dictionary<ROLE, int>(Utilities.defaultCitizenCreationTable);
		for (int i = 0; i < this.governor.behaviorTraits.Count; i++) {
			Dictionary<ROLE, int> currentTraitTable = Utilities.citizenCreationTable[this.governor.behaviorTraits[i]];
			for (int j = 0; j < currentTraitTable.Count; j++) {
				ROLE currentRole = currentTraitTable.Keys.ElementAt(j);
				this.citizenCreationTable [currentRole] += currentTraitTable [currentRole];
			}
		}
		this.creatableRoles.Clear();
		for (int i = 0; i < this.citizenCreationTable.Keys.Count; i++) {
			ROLE currentKey = this.citizenCreationTable.Keys.ElementAt(i);
			if (this.citizenCreationTable [currentKey] > 0) {
				this.creatableRoles.Add (currentKey);
			}
		}
	}

	#region boolean functions
	protected bool AllSpecialRolesMaxed(){
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

		if (traderCount >= traderLimit && generalCount >= generalLimit && spyCount >= spyLimit && envoyCount >= envoyLimit && guardianCount >= guardianLimit) {
			return true;
		}
		return false;
	}

	protected bool IsRoleMaxed(ROLE roleToCheck){
		int roleCount = this.GetCitizensWithRole(roleToCheck).Count;
		int roleLimit = this.citizenCreationTable[roleToCheck];

		if (roleCount >= roleLimit) {
			return true;
		}
		return false;
	}

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
		List<General> hostiles = GetIncomingAttackers();
		if(hostiles.Count > 0){
			int nearest = hostiles.Min (x => x.daysBeforeArrival);
			List<General> nearestHostiles = hostiles.Where(x => x.daysBeforeArrival == nearest).ToList();
			for(int i = 0; i < nearestHostiles.Count; i++){
				total += nearestHostiles[i].GetArmyHP ();
			}
		}

		return total;	
	}
	internal List<General> GetAllGenerals(General attacker){
		List<General> allGenerals = new List<General> ();
		if(attacker.citizen.city.governor.id != this.governor.id){
			for(int i = 0; i < this.citizens.Count; i++){
				if(this.citizens[i].assignedRole != null && this.citizens[i].role == ROLE.GENERAL){
					if(((General)this.citizens[i].assignedRole).location == this.hexTile){
						allGenerals.Add (((General)this.citizens [i].assignedRole));
					}
				}
			}
		}
		return allGenerals;
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

	internal void AdjustResources(List<Resource> resource, bool reduce = true){
		int currentResourceQuantity = 0;
//		Debug.Log ("Cost is: ");
//		for (int i = 0; i < resource.Count; i++) {
//			Debug.Log (resource[i].resourceType.ToString() + " " + resource[i].resourceQuantity.ToString());
//		}

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
		if(resourceCost != null){
			for (int i = 0; i < resourceCost.Count; i++) {
				Resource currentResource = resourceCost [i];
				if (currentResource.resourceType == this.kingdom.basicResource) {
					if (this.GetResourceAmountPerType (currentResource.resourceType) - this.totalCitizenConsumption < currentResource.resourceQuantity) {
						return false;
					}
				} else {
					if (this.GetResourceAmountPerType (currentResource.resourceType) < currentResource.resourceQuantity) {
						return false;
					}
				}
			}
		}else{
			return false;
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

	internal List<Resource> GetCitizenCreationCostPerType(ROLE role){
		if(role == ROLE.UNTRAINED){
			return null;
		}
		List<Resource> citizenCreationCosts = new List<Resource>();

		int goldCost = 500;
//		int goldCost = 1;


		switch (role) {
		case ROLE.TRADER:
		case ROLE.GENERAL:
			citizenCreationCosts = new List<Resource>(){
				new Resource (BASE_RESOURCE_TYPE.GOLD, goldCost),
//				new Resource (this.kingdom.basicResource, 2)
				new Resource (this.kingdom.basicResource, 1)
			};
			return citizenCreationCosts;
		case ROLE.SPY:
		case ROLE.GUARDIAN:
		case ROLE.ENVOY:
			citizenCreationCosts = new List<Resource>(){
				new Resource (BASE_RESOURCE_TYPE.GOLD, goldCost),
//				new Resource (this.kingdom.basicResource, 3)
				new Resource (this.kingdom.basicResource, 1)
			};
			return citizenCreationCosts;
		
		}
		return citizenCreationCosts;


	}
	#endregion

	internal void RemoveCitizenFromCity(Citizen citizenToRemove){
		if (citizenToRemove.role == ROLE.GOVERNOR) {
			this.AssignNewGovernor();
		}else if (citizenToRemove.role == ROLE.GENERAL) {
			((General)citizenToRemove.assignedRole).UntrainGeneral();
		}
		this.citizens.Remove (citizenToRemove);
//		if(citizenToRemove.workLocation != null){
//			citizenToRemove.workLocation.UnoccupyTile();
//		}
		citizenToRemove.city = null;
		citizenToRemove.role = ROLE.UNTRAINED;
		citizenToRemove.assignedRole = null;
		this.UpdateResourceProduction();
	}

	internal void AddCitizenToCity(Citizen citizenToAdd){
		this.citizens.Add(citizenToAdd);
		citizenToAdd.city = this;
		citizenToAdd.currentLocation = this.hexTile;
	}

	internal void AssignNewGovernor(){
		if(this.isDead){
			return;
		}
		Citizen newGovernor = GetCitizenWithHighestPrestige ();
		if (newGovernor != null) {
			if(newGovernor.assignedRole != null && newGovernor.role == ROLE.GENERAL){
				newGovernor.DetachGeneralFromCitizen();
			}
			this.governor.isGovernor = false;
			newGovernor.role = ROLE.GOVERNOR;
			newGovernor.assignedRole = null;
			newGovernor.AssignRole(ROLE.GOVERNOR);
			this.UpdateCitizenCreationTable();
			this.UpdateResourceProduction();
			newGovernor.history.Add(new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, newGovernor.name + " became the new Governor of " + this.name + ".", HISTORY_IDENTIFIER.NONE));
			this.cityHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, newGovernor.name + " became the new Governor of " + this.name + ".", HISTORY_IDENTIFIER.NONE));

		}
	}
	internal Citizen GetCitizenWithHighestPrestige(){
		List<Citizen> prestigeCitizens = new List<Citizen> ();
		if (this.citizens.Count > 1) {
			int maxPrestige = this.citizens.Where (x => !x.isGovernor && !x.isDead && !x.isKing).Max (x => x.prestige);
			for(int i = 0; i < this.citizens.Count; i++){
				if(this.citizens[i].prestige == maxPrestige){
					if(!this.citizens[i].isDead && !this.citizens[i].isGovernor && !this.citizens[i].isKing){
						prestigeCitizens.Add (this.citizens [i]);
					}
				}
			}
			return prestigeCitizens [UnityEngine.Random.Range (0, prestigeCitizens.Count)];
		}
		return null;


	}
	internal void KillCity(){
		this.incomingGenerals.Clear ();
		for (int i = 0; i < this.ownedTiles.Count; i++) {
			HexTile currentTile = this.ownedTiles[i];
			currentTile.ResetTile();
		}
		for (int i = 0; i < this.borderTiles.Count; i++) {
			HexTile currentTile = this.borderTiles[i];
			currentTile.ResetTile();
		}

		if(!this.isDead){
			bool removed = BehaviourTreeManager.Instance.allTrees.Remove (this.hexTile.GetComponent<PandaBehaviour> ());
			Debug.Log ("REMOVED BT?: " + this.name + " = " + removed);

			GameObject.Destroy (this.hexTile.GetComponent<PandaBehaviour> ());
			GameObject.Destroy (this.hexTile.GetComponent<CityTaskManager> ());
		}
		this.isDead = true;
		int countCitizens = this.citizens.Count;
		for (int i = 0; i < countCitizens; i++) {
			this.citizens [0].Death (DEATH_REASONS.INTERNATIONAL_WAR, false, null, true);
		}
		this.kingdom.cities.Remove (this);
		if(this.hasKing){
			this.hasKing = false;
			this.kingdom.AssignNewKing(null, this.kingdom.cities[0]);
		}
		EventManager.Instance.onCityEverydayTurnActions.RemoveListener (CityEverydayTurnActions);
		EventManager.Instance.onCitizenDiedEvent.RemoveListener (CheckCityDeath);
		this.hexTile.city = null;
		KingdomManager.Instance.UpdateKingdomAdjacency();

	}

	internal void LookForNewGeneral(General general){
		Debug.Log (general.citizen.name + " IS LOOKING FOR A NEW GENERAL FOR HIS/HER ARMY...");
		general.inAction = false;
		for(int i = 0; i < this.citizens.Count; i++){
			if(this.citizens[i].assignedRole != null && this.citizens[i].role == ROLE.GENERAL){
				General chosenGeneral = (General)this.citizens [i].assignedRole;
				if(chosenGeneral.location == general.location){
					if(!chosenGeneral.citizen.isDead){
						Debug.Log (chosenGeneral.citizen.name + " IS THE NEW GENERAL FOR " + general.citizen.name + "'s ARMY");
						chosenGeneral.army.hp += general.army.hp;
						general.army.hp = 0;
						chosenGeneral.UpdateUI ();
						general.GeneralDeath ();
					}
				}
			}
		}
	}

	internal void LookForLostArmy(General general){
		Debug.Log (general.citizen.name + " IS LOOKING FOR LOST ARMIES...");
		EventManager.Instance.onLookForLostArmies.Invoke (general);
//		List<General> deadGenerals = new List<General> ();
//		for(int i = 0; i < this.citizens.Count; i++){
//			if(this.citizens[i].assignedRole != null && this.citizens[i].role == ROLE.GENERAL){
//				General chosenGeneral = (General)this.citizens [i].assignedRole;
//				if(chosenGeneral.location == this.hexTile && chosenGeneral.citizen.city.id == this.id){
//					if(chosenGeneral.citizen.isDead){
//						Debug.Log (general.citizen.name + " IS THE NEW GENERAL FOR " + chosenGeneral.citizen.name + "'s ARMY");
//						general.army.hp += chosenGeneral.army.hp;
//						chosenGeneral.army.hp = 0;
//						deadGenerals.Add (chosenGeneral);
//					}
//				}
//			}
//		}
//		for(int i = 0; i < deadGenerals.Count; i++){
//			deadGenerals [i].GeneralDeath ();
//		}
	}

	internal bool HasAdjacency(int kingdomID){
		for(int i = 0; i < this.hexTile.connectedTiles.Count; i++){
			if(this.hexTile.connectedTiles[i].isOccupied){
				if(this.hexTile.connectedTiles[i].city.kingdom.id == kingdomID){
					return true;
				}
			}
		}
		return false;
	}

	internal void MoveCitizenToThisCity(Citizen citizenToMove){
		citizenToMove.city.RemoveCitizenFromCity(citizenToMove);
		this.AddCitizenToCity(citizenToMove);
	}

	protected void UpdateAdjacentCities(){
		this.adjacentCities.Clear();
		for (int i = 0; i < this.borderTiles.Count; i++) {
			HexTile currentHexTile = this.borderTiles[i];
			List<HexTile> currHexTileNeighbours = currentHexTile.AllNeighbours.Where (x => x.elevationType != ELEVATION.WATER).ToList();
			for (int j = 0; j < currHexTileNeighbours.Count; j++) {
				HexTile currNeighbour = currHexTileNeighbours[j];
				if (currNeighbour.isOccupied) {
					//Check if current neighbour is occupied by a different city
					if (currNeighbour.isOccupiedByCityID != this.id) {
						City cityToAdd = CityGenerator.Instance.GetCityByID(currNeighbour.isOccupiedByCityID);
						if (!this.adjacentCities.Contains(cityToAdd)) {
							this.adjacentCities.Add(cityToAdd);
						}
						if (!cityToAdd.adjacentCities.Contains (this)) {
							cityToAdd.adjacentCities.Add (this);
						}
					}
				} else if(currNeighbour.isBorder) {
					//Check if current neighbour is border of a different city
					if (currNeighbour.isBorderOfCityID != this.id) {
						City cityToAdd = CityGenerator.Instance.GetCityByID(currNeighbour.isBorderOfCityID);
						if (!this.adjacentCities.Contains(cityToAdd)) {
							this.adjacentCities.Add(cityToAdd);
						}
						if (!cityToAdd.adjacentCities.Contains (this)) {
							cityToAdd.adjacentCities.Add (this);
						}
					}
				}
			}
		}
	}
}
