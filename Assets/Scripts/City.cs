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
	private Kingdom _kingdom;
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
	public int _currentGrowth;
	public int _dailyGrowth;
	public int _maxGrowth;
//	public int maxGeneralHP;
	public int _goldProduction;

	[Space(5)]
	private int _hp;
	public IsActive isActive;
	public bool isStarving;
	public bool isDead;

//	internal Dictionary<ROLE, int> citizenCreationTable;
	internal List<HabitableTileDistance> habitableTileDistance; // Lists distance of habitable tiles in ascending order
	internal List<HexTile> borderTiles;
//	protected List<ROLE> creatableRoles;

	protected const int HP_INCREASE = 5;

	#region getters/setters
	public Kingdom kingdom{
		get{ return this._kingdom; }
	}
	public int currentGrowth{
		get{ return this._currentGrowth; }
	}
	public int dailyGrowth{
		get{ return this._dailyGrowth; }
	}
	public int maxGrowth{
		get{ return this._maxGrowth; }
	}
	protected List<HexTile> structures{
		get{ return this.ownedTiles.Where (x => x.isOccupied && !x.isHabitable).ToList();}
	}
	public int hp{
		get{ return this._hp; }
	}
	public int maxHP{
		get{ return 300 * (this.structures.Count + 1); } //+1 since the structures list does not contain the main hex tile
	}
	#endregion

	public City(HexTile hexTile, Kingdom kingdom){
		this.id = Utilities.SetID(this);
		this.hexTile = hexTile;
		this._kingdom = kingdom;
		this.name = RandomNameGenerator.Instance.GenerateCityName(this._kingdom.race);
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
//		this.citizenCreationTable = Utilities.defaultCitizenCreationTable;
//		this.creatableRoles = new List<ROLE>();
		this.borderTiles = new List<HexTile>();
		this.habitableTileDistance = new List<HabitableTileDistance> ();
		this._hp = 100;

		this.hexTile.Occupy (this);
		this.ownedTiles.Add(this.hexTile);
		this.UpdateBorderTiles();
//		this.CreateInitialFamilies();

		EventManager.Instance.onCityEverydayTurnActions.AddListener(CityEverydayTurnActions);
		EventManager.Instance.onCitizenDiedEvent.AddListener(CheckCityDeath);

		this.cityHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "City " + this.name + " was founded.", HISTORY_IDENTIFIER.NONE));
	}


	/*
	 * This will add a new habitable hex tile to the habitableTileDistance variable.
	 * */
	public void AddHabitableTileDistance(HexTile hexTile, int distance) {
		if (distance == 0) {
			return;
		}
		if (this.habitableTileDistance.Count == 0) {			
			this.habitableTileDistance.Add (new HabitableTileDistance (hexTile, distance));
		} else {
			for (int i = 0; i < this.habitableTileDistance.Count; i++) {
				if (this.habitableTileDistance [i].distance >= distance) {
					this.habitableTileDistance.Insert (i, new HabitableTileDistance (hexTile, distance));
					break;
				}
			}
		}
		//this.habitableTileDistance.Add (new HabitableTileDistance (hexTile, distance));
	}

	/*
	 * This will rearrange habitableTileDistance by ascending distance.
	 * */
	public void OrderHabitableTileDistanceList() {
		//this.habitableTileDistance = this.habitableTileDistance.OrderBy (x => x.distance);
	}

	/*
	 * Initialize City With Initial Citizens aka. Families
	 * */
	internal void CreateInitialFamilies(bool hasRoyalFamily = true){
		if(hasRoyalFamily){
			this.hasKing = true;
			this.CreateInitialRoyalFamily ();
		}
		this.CreateInitialGovernorFamily ();
		this.UpdateDailyProduction();


		for (int i = 0; i < this.citizens.Count; i++) {
			this.citizens[i].UpdatePrestige();
		}
	}
		
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
			currBorderTile.Borderize (this);
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
	internal void ExpandToThisCity(Citizen citizenToOccupyCity){
//		this.CreateInitialFamilies(false);
		this.AddCitizenToCity(citizenToOccupyCity);
		citizenToOccupyCity.role = ROLE.GOVERNOR;
		citizenToOccupyCity.assignedRole = null;
		citizenToOccupyCity.AssignRole(ROLE.GOVERNOR);
		this.UpdateDailyProduction();
		KingdomManager.Instance.UpdateKingdomAdjacency();
		this.kingdom.AddInternationalWarCity (this);
		if (UIManager.Instance.kingdomInfoGO.activeSelf) {
			if (UIManager.Instance.currentlyShowingKingdom != null && UIManager.Instance.currentlyShowingKingdom.id == this.kingdom.id) {
				this.kingdom.HighlightAllOwnedTilesInKingdom();
			}
		}

		KingdomManager.Instance.CheckWarTriggerMisc (this.kingdom, WAR_TRIGGER.TARGET_GAINED_A_CITY);
	}

	/*internal List<General> GetIncomingAttackers(){
		List<General> incomingAttackers = new List<General> ();
		for(int i = 0; i < this.incomingGenerals.Count; i++){
			if (this.incomingGenerals [i].assignedCampaign.campaignType == CAMPAIGN.OFFENSE && this.incomingGenerals [i].assignedCampaign.targetCity.id == this.id) {
				incomingAttackers.Add (this.incomingGenerals [i]);
			}
		}
		return incomingAttackers;
	}*/

	/*
	 * Purchase new tile for city. Called in CityTaskManager.
	 * */
	internal void PurchaseTile(HexTile tileToBuy){
		tileToBuy.movementDays = 2;
		tileToBuy.Occupy (this);

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
			City otherCity = CityGenerator.Instance.GetCityByID (tileToBuy.isBorderOfCityID);
			otherCity.borderTiles.Remove (tileToBuy);
			this.UpdateBorderTiles ();
			otherCity.UpdateBorderTiles ();
		} else {
			this.UpdateBorderTiles();
		}

        //Add special resources to kingdoms available resources, if the purchased tile has any
        if (tileToBuy.specialResource != RESOURCE.NONE) {
            this._kingdom.AddResourceToKingdom(tileToBuy.specialResource);
        }
		
		//Update necessary data
		this.UpdateDailyProduction();
//		this.UpdateAdjacentCities();
//		this.kingdom.UpdateKingdomAdjacency();

		//Show Highlight if kingdom or city is currently highlighted
		if (UIManager.Instance.currentlyShowingKingdom != null && UIManager.Instance.currentlyShowingKingdom.id == this.kingdom.id) {
			this._kingdom.HighlightAllOwnedTilesInKingdom ();
		} else {
			if (this.hexTile.kingdomColorSprite.gameObject.activeSelf) {
				this._kingdom.HighlightAllOwnedTilesInKingdom ();
			}
		}
	}

	/*
	 * Function that listens to onWeekEnd. Performed every tick.
	 * */
	protected void CityEverydayTurnActions(){
		this.ProduceGold();
		this.AttemptToIncreaseHP();
	}

	/*
	 * Increase a city's HP every month.
	 * */
	protected void AttemptToIncreaseHP(){
		if (GameManager.daysInMonth[GameManager.Instance.month] == GameManager.Instance.days) {
			this.IncreaseHP(HP_INCREASE);
		}
	}

	/*
	 * Function to increase HP.
	 * */
	public void IncreaseHP(int amountToIncrease){
		this._hp += amountToIncrease;
		if (this._hp > this.maxHP) {
			this._hp = this.maxHP;
		}
	}

	#region Resource Production
	protected void ProduceGold(){
		this.kingdom.AdjustGold(this._goldProduction);
	}

	internal void AddToDailyGrowth(){
		this._currentGrowth += this._dailyGrowth;
		if (this._currentGrowth >= this._maxGrowth) {
			this._currentGrowth = this._maxGrowth;
		}
	}

	internal void ResetDailyGrowth(){
		this._currentGrowth = 0;
	}

	internal void UpdateDailyProduction(){
		this._maxGrowth = 100 + ((100 + (100 * this.structures.Count)) * this.structures.Count);
		this._dailyGrowth = 10;
		this._goldProduction = 20;
		List<RESOURCE> specialResources = new List<RESOURCE>();
		for (int i = 0; i < this.structures.Count; i++) {
			HexTile currentStructure = this.structures [i];
			if (currentStructure.biomeType == BIOMES.GRASSLAND) {
				this._dailyGrowth += 5;
				this._goldProduction += 2;
			} else if (currentStructure.biomeType == BIOMES.WOODLAND) {
				this._dailyGrowth += 4;
				this._goldProduction += 3;
			} else if (currentStructure.biomeType == BIOMES.FOREST) {
				this._dailyGrowth += 3;
				this._goldProduction += 3;
			} else if (currentStructure.biomeType == BIOMES.DESERT) {
				this._dailyGrowth += 1;
				this._goldProduction += 4;
			} else if (currentStructure.biomeType == BIOMES.TUNDRA) {
				this._dailyGrowth += 2;
				this._goldProduction += 2;
			} else if (currentStructure.biomeType == BIOMES.SNOW) {
				this._dailyGrowth += 1;
				this._goldProduction += 1;
			} else if (currentStructure.biomeType == BIOMES.BARE) {
				this._dailyGrowth += 1;
			}
			RESOURCE currentSpecialResource = RESOURCE.NONE;
			if (currentStructure.specialResource != RESOURCE.NONE) {
				currentSpecialResource = currentStructure.specialResource;
			}
			if (!specialResources.Contains(currentSpecialResource)) {
				specialResources.Add(currentSpecialResource);
			}
		}

		for (int i = 0; i < specialResources.Count; i++) {
			RESOURCE currentResource = specialResources[i];
//			if (currentResource == RESOURCE.GRANITE || currentResource == RESOURCE.SLATE || currentResource == RESOURCE.MARBLE) {
//				this.stoneCount += 3;
//			} else if (currentResource == RESOURCE.CEDAR || currentResource == RESOURCE.OAK || currentResource == RESOURCE.EBONY) {
//				this.lumberCount += 3;
//			} else 
			if (currentResource == RESOURCE.CORN || currentResource == RESOURCE.DEER) {
				this._dailyGrowth += 5;
			} else if (currentResource == RESOURCE.WHEAT || currentResource == RESOURCE.RICE ||
				currentResource == RESOURCE.PIG || currentResource == RESOURCE.BEHEMOTH ||
				currentResource == RESOURCE.COBALT) {
				this._dailyGrowth += 10;
			} else if (currentResource == RESOURCE.MANA_STONE) {
				this._dailyGrowth += 15;
			} else if (currentResource == RESOURCE.MITHRIL) {
				this._dailyGrowth += 25;
			}
		}

	}
	#endregion

	internal void CheckCityDeath(){
		if (this.citizens.Count <= 0) {
			this.KillCity ();
		}
	}

//	internal ROLE GetNonProducingRoleToCreate(){
//		int previousRoleCount = 10;
//		ROLE roleToCreate = ROLE.UNTRAINED;
//
//		for (int i = 0; i < this.creatableRoles.Count; i++) {
//			ROLE currentRole = this.creatableRoles [i];
//			int currentRoleCount = this.GetCitizensWithRole (currentRole).Count;
//			int currentRoleLimit = this.citizenCreationTable [currentRole];
//			if (currentRoleCount < previousRoleCount && currentRoleCount < currentRoleLimit) {
//				roleToCreate = currentRole;
//				previousRoleCount = currentRoleCount;
//			}
//		}
//		return roleToCreate;
//	}

//	internal void UpdateCitizenCreationTable(){
//		this.citizenCreationTable = new Dictionary<ROLE, int>(Utilities.defaultCitizenCreationTable);
//		Dictionary<ROLE, int> currentTraitTable = Utilities.citizenCreationTable[this.governor.honestyTrait];
//		for (int j = 0; j < currentTraitTable.Count; j++) {
//			ROLE currentRole = currentTraitTable.Keys.ElementAt(j);
//			this.citizenCreationTable [currentRole] += currentTraitTable [currentRole];
//		}
//
//		currentTraitTable = Utilities.citizenCreationTable[this.governor.hostilityTrait];
//		for (int j = 0; j < currentTraitTable.Count; j++) {
//			ROLE currentRole = currentTraitTable.Keys.ElementAt(j);
//			this.citizenCreationTable [currentRole] += currentTraitTable [currentRole];
//		}
//		this.creatableRoles.Clear();
//		for (int i = 0; i < this.citizenCreationTable.Keys.Count; i++) {
//			ROLE currentKey = this.citizenCreationTable.Keys.ElementAt(i);
//			if (this.citizenCreationTable [currentKey] > 0) {
//				this.creatableRoles.Add (currentKey);
//			}
//		}
//	}

	/*internal int GetCityArmyStrength(){
		int total = 0;
		for(int i = 0; i < this.citizens.Count; i++){
			if(this.citizens[i].assignedRole != null && this.citizens[i].role == ROLE.GENERAL){
				total += ((General)this.citizens [i].assignedRole).GetArmyHP ();
			}
		}
		return total;
	}
	internal int GetTotalAttackerStrength(int nearest){
		int total = 0;
		if(nearest != -2){
			List<General> hostiles = GetIncomingAttackers().Where(x => x.daysBeforeArrival == nearest).ToList();
			if(hostiles.Count > 0){
				for(int i = 0; i < hostiles.Count; i++){
					total += hostiles[i].GetArmyHP ();
				}
			}
		}
		return total;	
	}*/

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
			break;
		case BASE_RESOURCE_TYPE.GOLD:
			break;
		case BASE_RESOURCE_TYPE.WOOD:
			break;
		case BASE_RESOURCE_TYPE.STONE:
			break;
		case BASE_RESOURCE_TYPE.MANA_STONE:
			break;
		case BASE_RESOURCE_TYPE.MITHRIL:
			break;
		case BASE_RESOURCE_TYPE.COBALT:
			break;
		}
	}

	internal bool HasEnoughResourcesForAction(List<Resource> resourceCost){
		if(resourceCost != null){
			for (int i = 0; i < resourceCost.Count; i++) {
				Resource currentResource = resourceCost [i];
				if (this.GetResourceAmountPerType (currentResource.resourceType) < currentResource.resourceQuantity) {
					return false;
				}
			}
		}else{
			return false;
		}
		return true;
	}

	protected int GetResourceAmountPerType(BASE_RESOURCE_TYPE resourceType){
		if (resourceType == BASE_RESOURCE_TYPE.FOOD) {
			return 0;
		} else if (resourceType == BASE_RESOURCE_TYPE.WOOD) {
			return 0;
		} else if (resourceType == BASE_RESOURCE_TYPE.STONE) {
			return 0;
		} else if (resourceType == BASE_RESOURCE_TYPE.MITHRIL) {
			return 0;
		} else if (resourceType == BASE_RESOURCE_TYPE.MANA_STONE) {
			return 0;
		} else if (resourceType == BASE_RESOURCE_TYPE.COBALT) {
			return 0;
		} else if (resourceType == BASE_RESOURCE_TYPE.GOLD) {
			return 0;
		}
		return -1;
	}

	internal List<Resource> GetCitizenCreationCostPerType(ROLE role){
		if(role == ROLE.UNTRAINED){
			return null;
		}
		List<Resource> citizenCreationCosts = new List<Resource>();

		int goldCost = 500;

		switch (role) {
		case ROLE.TRADER:
		case ROLE.GENERAL:
			citizenCreationCosts = new List<Resource>(){
				new Resource (BASE_RESOURCE_TYPE.GOLD, goldCost),
				new Resource (this.kingdom.basicResource, 2)
			};
			return citizenCreationCosts;
		case ROLE.SPY:
		case ROLE.GUARDIAN:
		case ROLE.ENVOY:
			citizenCreationCosts = new List<Resource>(){
				new Resource (BASE_RESOURCE_TYPE.GOLD, goldCost),
				new Resource (this._kingdom.basicResource, 3)
			};
			return citizenCreationCosts;
		
		}
		return citizenCreationCosts;
	}
	#endregion

	internal void RemoveCitizenFromCity(Citizen citizenToRemove){
		if (citizenToRemove.role == ROLE.GOVERNOR) {
			this.AssignNewGovernor();
		}
		/*else if (citizenToRemove.role == ROLE.GENERAL) {
			((General)citizenToRemove.assignedRole).UntrainGeneral();
		}*/
		this.citizens.Remove (citizenToRemove);
		citizenToRemove.city = null;
		citizenToRemove.role = ROLE.UNTRAINED;
		citizenToRemove.assignedRole = null;
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
			/*if(newGovernor.assignedRole != null && newGovernor.role == ROLE.GENERAL){
				newGovernor.DetachGeneralFromCitizen();
			}*/
			this.governor.isGovernor = false;
			newGovernor.role = ROLE.GOVERNOR;
			newGovernor.assignedRole = null;
			newGovernor.AssignRole(ROLE.GOVERNOR);
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
		this.ownedTiles.Clear ();
		this.borderTiles.Clear ();
		if(!this.isDead){
			bool removed = BehaviourTreeManager.Instance.allTrees.Remove (this.hexTile.GetComponent<PandaBehaviour> ());
//			Debug.Log ("REMOVED BT?: " + this.name + " = " + removed);

			GameObject.Destroy (this.hexTile.GetComponent<PandaBehaviour> ());
			GameObject.Destroy (this.hexTile.GetComponent<CityTaskManager> ());
		}
		this.isDead = true;
		EventManager.Instance.onDeathToGhost.Invoke (this);
		int countCitizens = this.citizens.Count;
		for (int i = 0; i < countCitizens; i++) {
			this.citizens [0].Death (DEATH_REASONS.INTERNATIONAL_WAR, false, null, true);
		}
		this._kingdom.cities.Remove (this);
		if(this.hasKing){
			this.hasKing = false;
			if(this._kingdom.cities.Count > 0){
				this._kingdom.AssignNewKing(null, this._kingdom.cities[0]);
			}
		}
		// This will update kingdom type whenever the kingdom loses a city.
		this._kingdom.UpdateKingdomTypeData();
		this._kingdom.CheckIfKingdomIsDead();

		EventManager.Instance.onCityEverydayTurnActions.RemoveListener (CityEverydayTurnActions);
		EventManager.Instance.onCitizenDiedEvent.RemoveListener (CheckCityDeath);
		this.hexTile.city = null;
		KingdomManager.Instance.UpdateKingdomAdjacency();
	}

	/*internal void LookForNewGeneral(General general){
//		Debug.Log (general.citizen.name + " IS LOOKING FOR A NEW GENERAL FOR HIS/HER ARMY...");
		general.inAction = false;
		for(int i = 0; i < this.citizens.Count; i++){
			if(this.citizens[i].assignedRole != null && this.citizens[i].role == ROLE.GENERAL){
				General chosenGeneral = (General)this.citizens [i].assignedRole;
				if(chosenGeneral.location == general.location){
					if(!chosenGeneral.citizen.isDead){
//						Debug.Log (chosenGeneral.citizen.name + " IS THE NEW GENERAL FOR " + general.citizen.name + "'s ARMY");
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
//		Debug.Log (general.citizen.name + " IS LOOKING FOR LOST ARMIES...");
		EventManager.Instance.onLookForLostArmies.Invoke (general);
	}*/

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

	internal Citizen CreateAgent(ROLE role, EVENT_TYPES eventType, HexTile targetLocation, int duration){
		int cost = 0;
		if(!this.kingdom.CanCreateAgent(role, ref cost)){
			return null;
		}
		List<HexTile> path = PathGenerator.Instance.GetPath (this.hexTile, targetLocation, PATHFINDING_MODE.COMBAT).ToList();
		if (path == null) {
			return null;
		}
		if(!Utilities.CanReachInTime(eventType, path, duration)){
			return null;
		}
		GENDER gender = GENDER.MALE;
		int randomGender = UnityEngine.Random.Range (0, 100);
		if(randomGender < 20){
			gender = GENDER.FEMALE;
		}
		int maxGeneration = this.citizens.Max (x => x.generation);
		Citizen expandCitizen = new Citizen (this, UnityEngine.Random.Range (20, 36), gender, maxGeneration + 1);
		MONTH monthCitizen = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
		expandCitizen.AssignBirthday (monthCitizen, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthCitizen] + 1), (GameManager.Instance.year - governor.age));
		expandCitizen.AssignRole (role);
		expandCitizen.assignedRole.targetLocation = targetLocation;
		expandCitizen.assignedRole.path = path;
		expandCitizen.assignedRole.daysBeforeMoving = path [0].movementDays;
		this._kingdom.AdjustGold (-cost);
		this.citizens.Remove (expandCitizen);

		return expandCitizen;
	}
}
