using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Panda;

#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.

[System.Serializable]
public class City{

	public int id;
	public string name;
	public HexTile hexTile;
	private Kingdom _kingdom;
	public Citizen governor;
	public List<City> adjacentCities;
	public List<HexTile> _ownedTiles;
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
	//public int goldCount;
	private int _currentGrowth;
    //private int _dailyGrowth;
    private int _dailyGrowthFromStructures;
    private int _dailyGrowthFromKingdom;
    private int _maxGrowth;
//	public int maxGeneralHP;
	//public int _goldProduction;
	private int raidLoyaltyExpiration;

	[Space(5)]
	private int _hp;
//	public IsActive isActive;
	public bool isUnderAttack;
	public bool hasReinforced;
	public bool isRaided;
	public bool isStarving;
	public bool isDead;

//	internal Dictionary<ROLE, int> citizenCreationTable;
	internal List<HabitableTileDistance> habitableTileDistance; // Lists distance of habitable tiles in ascending order
	internal List<HexTile> borderTiles;
	internal Rebellion rebellion;
	internal Plague plague;
//	protected List<ROLE> creatableRoles;

	protected const int HP_INCREASE = 5;
	private int increaseHpInterval = 0;

	#region getters/setters
	public Kingdom kingdom{
		get{ return this._kingdom; }
	}
	public int currentGrowth{
		get{ return this._currentGrowth; }
	}
	public int totalDailyGrowth{
		get{ return this._dailyGrowthFromKingdom + this._dailyGrowthFromStructures; }
	}
	public int maxGrowth{
		get{ return this._maxGrowth; }
	}
	public List<HexTile> structures{
		get{ return this._ownedTiles.Where (x => x.isOccupied && !x.isHabitable).ToList();} //Contains all structures, except capital city
	}
	public List<HexTile> plaguedSettlements{
		get{ return this.structures.Where (x => x.isPlagued).ToList();} //Get plagued settlements
	}
	public int hp{
		get{ return this._hp; }
		set{ this._hp = value; }
	}
	public int maxHP{
		get{ return Utilities.defaultCityHP +  (50 * this.structures.Count); } //+1 since the structures list does not contain the main hex tile
	}
    public List<HexTile> ownedTiles {
        get { return this._ownedTiles; }
    }
	#endregion

	public City(HexTile hexTile, Kingdom kingdom){
		this.id = Utilities.SetID(this);
		this.hexTile = hexTile;
		this._kingdom = kingdom;
		this.name = RandomNameGenerator.Instance.GenerateCityName(this._kingdom.race);
		this.governor = null;
		this.adjacentCities = new List<City>();
		this._ownedTiles = new List<HexTile>();
		this.incomingGenerals = new List<General> ();
		this.citizens = new List<Citizen>();
		this.cityHistory = new List<History>();
//		this.isActive = new IsActive (false);
		this.hasKing = false;
		this.isUnderAttack = false;
		this.hasReinforced = false;
		this.isRaided = false;
		this.isStarving = false;
		this.isDead = false;
//		this.citizenCreationTable = Utilities.defaultCitizenCreationTable;
//		this.creatableRoles = new List<ROLE>();
		this.borderTiles = new List<HexTile>();
		this.habitableTileDistance = new List<HabitableTileDistance> ();
		this.raidLoyaltyExpiration = 0;

		this.hexTile.Occupy (this);
		this.ownedTiles.Add(this.hexTile);
		this.plague = null;
		ResetToDefaultHP ();
//		this.CreateInitialFamilies();
		EventManager.Instance.onCityEverydayTurnActions.AddListener(CityEverydayTurnActions);
		EventManager.Instance.onCitizenDiedEvent.AddListener(CheckCityDeath);
//		this.cityHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "City " + this.name + " was founded.", HISTORY_IDENTIFIER.NONE));
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
					return;
				}
			}
			this.habitableTileDistance.Add (new HabitableTileDistance (hexTile, distance));
		}
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

        this.hexTile.ShowNamePlate();
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

		MONTH monthFather = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
		MONTH monthMother = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
		MONTH monthKing = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));

		father.AssignBirthday (monthFather, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthFather] + 1), GameManager.Instance.year - father.age);
		mother.AssignBirthday (monthMother, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthMother] + 1), GameManager.Instance.year - mother.age);
		king.AssignBirthday (monthKing, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthKing] + 1), (GameManager.Instance.year - king.age));

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

			//List<int> childAges = Enumerable.Range(0, (spouse.age - 16)).ToList();
			//if(spouse.gender == GENDER.MALE){
			//	childAges = Enumerable.Range(0, (this.kingdom.king.age - 16)).ToList();
			//}


			//int childChance = UnityEngine.Random.Range (0, 100);
			//if (childChance < 25) {
				
			//	int age1 = UnityEngine.Random.Range (0, childAges.Count);
			//	Citizen child1 = MarriageManager.Instance.MakeBaby (this.kingdom.king, this.kingdom.king.spouse, childAges[age1]);
			//	childAges.RemoveAt (age1);

			//	int age2 = UnityEngine.Random.Range (0, childAges.Count);
			//	Citizen child2 = MarriageManager.Instance.MakeBaby (this.kingdom.king, this.kingdom.king.spouse, childAges[age2]);
			//	childAges.RemoveAt (age2);

			//	int age3 = UnityEngine.Random.Range (0, childAges.Count);
			//	Citizen child3 = MarriageManager.Instance.MakeBaby (this.kingdom.king, this.kingdom.king.spouse, childAges[age3]);

			//	child1.AssignBirthday (monthChild1, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthChild1] + 1), (GameManager.Instance.year - child1.age));
			//	child2.AssignBirthday (monthChild2, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthChild2] + 1), (GameManager.Instance.year - child2.age));
			//	child3.AssignBirthday (monthChild3, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthChild3] + 1), (GameManager.Instance.year - child3.age));


			//} else if (childChance >= 25 && childChance < 50) {
				
			//	int age1 = UnityEngine.Random.Range (0, childAges.Count);
			//	Citizen child1 = MarriageManager.Instance.MakeBaby (this.kingdom.king, this.kingdom.king.spouse, childAges[age1]);
			//	childAges.RemoveAt (age1);

			//	int age2 = UnityEngine.Random.Range (0, childAges.Count);
			//	Citizen child2 = MarriageManager.Instance.MakeBaby (this.kingdom.king, this.kingdom.king.spouse, childAges[age2]);

			//	child1.AssignBirthday (monthChild1, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthChild1] + 1), (GameManager.Instance.year - child1.age));
			//	child2.AssignBirthday (monthChild2, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthChild2] + 1), (GameManager.Instance.year - child2.age));

			//} else if (childChance >= 50 && childChance < 75) {

			//	int age1 = UnityEngine.Random.Range (0, childAges.Count);

			//	Citizen child1 = MarriageManager.Instance.MakeBaby (this.kingdom.king, this.kingdom.king.spouse, childAges[age1]);

			//	child1.AssignBirthday (monthChild1, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthChild1] + 1), (GameManager.Instance.year - child1.age));

			//}
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

   //         List<int> childAges = Enumerable.Range(0, (spouse.age - 16)).ToList();
			//if(spouse.gender == GENDER.MALE){
			//	childAges = Enumerable.Range(0, (governor.age - 16)).ToList();
			//}

			//int childChance = UnityEngine.Random.Range (0, 100);
			//if (childChance < 25) {

			//	int age1 = UnityEngine.Random.Range (0, childAges.Count);
			//	Citizen child1 = MarriageManager.Instance.MakeBaby (governor, spouse, childAges[age1]);
			//	childAges.RemoveAt (age1);

			//	int age2 = UnityEngine.Random.Range (0, childAges.Count);
			//	Citizen child2 = MarriageManager.Instance.MakeBaby (governor, spouse, childAges[age2]);
			//	childAges.RemoveAt (age2);

			//	int age3 = UnityEngine.Random.Range (0, childAges.Count);
			//	Citizen child3 = MarriageManager.Instance.MakeBaby (governor, spouse, childAges[age3]);

			//	child1.AssignBirthday (monthChild1, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthChild1] + 1), (GameManager.Instance.year - child1.age));
			//	child2.AssignBirthday (monthChild2, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthChild2] + 1), (GameManager.Instance.year - child2.age));
			//	child3.AssignBirthday (monthChild3, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthChild3] + 1), (GameManager.Instance.year - child3.age));


			//} else if (childChance >= 25 && childChance < 50) {

			//	int age1 = UnityEngine.Random.Range (0, childAges.Count);
			//	Citizen child1 = MarriageManager.Instance.MakeBaby (governor, spouse, childAges[age1]);
			//	childAges.RemoveAt (age1);

			//	int age2 = UnityEngine.Random.Range (0, childAges.Count);
			//	Citizen child2 = MarriageManager.Instance.MakeBaby (governor, spouse, childAges[age2]);

			//	child1.AssignBirthday (monthChild1, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthChild1] + 1), (GameManager.Instance.year - child1.age));
			//	child2.AssignBirthday (monthChild2, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthChild2] + 1), (GameManager.Instance.year - child2.age));

			//} else if (childChance >= 50 && childChance < 75) {

			//	int age1 = UnityEngine.Random.Range (0, childAges.Count);

			//	Citizen child1 = MarriageManager.Instance.MakeBaby (governor, spouse, childAges[age1]);

			//	child1.AssignBirthday (monthChild1, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthChild1] + 1), (GameManager.Instance.year - child1.age));

			//}
		}

		this.cityHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, governor.name + " became the new Governor of " + this.name + ".", HISTORY_IDENTIFIER.NONE));

	}

	internal void UpdateBorderTiles(){
		for (int i = 0; i < this.borderTiles.Count; i++) {
            if (!this.borderTiles[i].isOccupied) {
                this.borderTiles[i].ResetTile();
            }
            //this.borderTiles[i].isBorder = false;
            //this.borderTiles[i].isBorderOfCityID = 0;
            //this.borderTiles[i].ResetTile();
        }
		this.borderTiles.Clear();

		List<HexTile> outmostTiles = new List<HexTile>();
		for (int i = 0; i < this.ownedTiles.Count; i++) {
			HexTile currHexTile = this.ownedTiles[i];
			List<HexTile> currHexTileUnoccupiedNeighbours = currHexTile.AllNeighbours.Where(x => x.elevationType != ELEVATION.WATER && !x.isOccupied).ToList();
			if (currHexTileUnoccupiedNeighbours.Count > 0) {
				outmostTiles.Add (currHexTile);
			}
		}

		for (int i = 0; i < outmostTiles.Count; i++) {
			List<HexTile> possibleBorderTiles = outmostTiles [i].GetTilesInRange(3).Where (x => x.elevationType != ELEVATION.WATER && !x.isOccupied && !x.isBorder).ToList();
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
		//KingdomManager.Instance.UpdateKingdomAdjacency();
//		this.kingdom.AddInternationalWarCity (this);
		if (UIManager.Instance.kingdomInfoGO.activeSelf) {
			if (UIManager.Instance.currentlyShowingKingdom != null && UIManager.Instance.currentlyShowingKingdom.id == this.kingdom.id) {
				this.kingdom.HighlightAllOwnedTilesInKingdom();
			}
		}
        this.hexTile.ShowNamePlate();
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
		float percentageHP = (float)this._hp / (float)this.maxHP;
		tileToBuy.movementDays = 2;
		tileToBuy.Occupy (this);
		CollectEventInTile (tileToBuy);
		EventManager.Instance.onUpdatePath.Invoke (tileToBuy);

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

        //Update necessary data
        this.UpdateDailyProduction();
        this.kingdom.CheckForDiscoveredKingdoms(this);
        //this.UpdateAdjacentCities();
        //this.kingdom.UpdateKingdomAdjacency();

        //Add special resources to kingdoms available resources, if the purchased tile has any
        if (tileToBuy.specialResource != RESOURCE.NONE) {
            this._kingdom.AddResourceToKingdom(tileToBuy.specialResource);
        }
        

        //Show Highlight if kingdom or city is currently highlighted
        if (UIManager.Instance.currentlyShowingKingdom != null && UIManager.Instance.currentlyShowingKingdom.id == this.kingdom.id) {
			this._kingdom.HighlightAllOwnedTilesInKingdom ();
		} else {
			if (this.hexTile.kingdomColorSprite.gameObject.activeSelf) {
				this._kingdom.HighlightAllOwnedTilesInKingdom ();
			}
		}
		this.UpdateHP (percentageHP);
	}

	/*
	 * Function that listens to onWeekEnd. Performed every tick.
	 * */
	protected void CityEverydayTurnActions(){
		this.hasReinforced = false;
		//this.ProduceGold();
		this.AttemptToIncreaseHP();
	}
	/*
	 * Function that listens to onWeekEnd. Performed every tick.
	 * */
	protected void RebelFortEverydayTurnActions(){
		this.hasReinforced = false;
		this.AttemptToIncreaseHP();
	}
	/*
	 * Increase a city's HP every month.
	 * */
	protected void AttemptToIncreaseHP(){
		if(GameManager.Instance.days == 1){
			int hpIncrease = 60 + (5 * this.kingdom.techLevel);
			if(this.kingdom.HasWar()){
				hpIncrease = (int)(hpIncrease / 2);
			}
			this.IncreaseHP (hpIncrease);
		}
//		if(this.increaseHpInterval == 1){
//			this.increaseHpInterval = 0;
//			this.IncreaseHP (1);
//		}else{
//			this.increaseHpInterval += 1;
//		}

//		if (GameManager.daysInMonth[GameManager.Instance.month] == GameManager.Instance.days) {
//			this.IncreaseHP(HP_INCREASE);
//		}
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

	public void AdjustHP(int amount){
		this._hp += amount;
		if(this._hp < 0){
			this._hp = 0;
		}
	}

	private void UpdateHP(float percentageHP){
		this._hp = (int)((float)this.maxHP * percentageHP);
	}
	private void CheckRaidExpiration(){
		if(this.isRaided){
			if(this.raidLoyaltyExpiration > 0){
				this.raidLoyaltyExpiration -= 1;
			}else{
				this.HasNotBeenRaided ();
			}

		}
	}

	#region Resource Production
	//protected void ProduceGold(){
	//	this.kingdom.AdjustGold(this._goldProduction);
	//}

	internal void AddToDailyGrowth(){
        AdjustDailyGrowth(this.totalDailyGrowth);
    }

	internal void ResetDailyGrowth(){
		this._currentGrowth = 0;
	}

    internal void AdjustDailyGrowth(int amount) {
        this._currentGrowth += amount;
        this._currentGrowth = Mathf.Clamp(this._currentGrowth, 0, this._maxGrowth);
    }

	internal void UpdateDailyProduction(){
		this._maxGrowth = 200 + ((300 + (350 * this.structures.Count)) * this.structures.Count);
		this._dailyGrowthFromStructures = 10;
		for (int i = 0; i < this.structures.Count; i++) {
			HexTile currentStructure = this.structures [i];
            if (!currentStructure.isPlagued) {
                if (currentStructure.biomeType == BIOMES.GRASSLAND) {
                    this._dailyGrowthFromStructures += 3;
                } else if (currentStructure.biomeType == BIOMES.WOODLAND) {
                    this._dailyGrowthFromStructures += 3;
                } else if (currentStructure.biomeType == BIOMES.FOREST) {
                    this._dailyGrowthFromStructures += 2;
                } else if (currentStructure.biomeType == BIOMES.DESERT) {
                    this._dailyGrowthFromStructures += 1;
                } else if (currentStructure.biomeType == BIOMES.TUNDRA) {
                    this._dailyGrowthFromStructures += 2;
                } else if (currentStructure.biomeType == BIOMES.SNOW) {
                    this._dailyGrowthFromStructures += 1;
                } else if (currentStructure.biomeType == BIOMES.BARE) {
                    this._dailyGrowthFromStructures += 1;
                }
            }
		}
	}

    /*
     * Add to this city's daily growth based on the resources it's kingdom has.
     * Including resources from trade. Increase is computed by kingdom.
     * */
    internal void UpdateDailyGrowthBasedOnSpecialResources(int dailyGrowthGained) {
        this._dailyGrowthFromKingdom = dailyGrowthGained;
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
	#endregion

	internal void RemoveCitizenFromCity(Citizen citizenToRemove, bool isFleeing = false){
		if(!isFleeing){
			if (citizenToRemove.role == ROLE.GOVERNOR) {
				this.AssignNewGovernor();
			}
			/*else if (citizenToRemove.role == ROLE.GENERAL) {
				((General)citizenToRemove.assignedRole).UntrainGeneral();
			}*/

			citizenToRemove.role = ROLE.UNTRAINED;
			citizenToRemove.assignedRole = null;
		}


		this.citizens.Remove (citizenToRemove);
		citizenToRemove.city = null;
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
		Citizen newGovernor = GetGovernorSuccession ();
		if (newGovernor != null) {
			/*if(newGovernor.assignedRole != null && newGovernor.role == ROLE.GENERAL){
				newGovernor.DetachGeneralFromCitizen();
			}*/
			if(this.governor != null){
				this.governor.isGovernor = false;
			}
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

	internal Citizen GetGovernorSuccession(){
		List<Citizen> succession = new List<Citizen> ();
		if (this.governor != null) {
			for (int i = 0; i < this.governor.children.Count; i++) {
				if (!this.governor.children [i].isGovernor) {
					return this.governor.children [i];
				}
			}
			List<Citizen> siblings = this.governor.GetSiblings ();
			if(siblings != null && siblings.Count > 0){
				return siblings [0];
			}
		}

		GENDER gender = GENDER.MALE;
		int randomGender = UnityEngine.Random.Range (0, 100);
		if(randomGender < 20){
			gender = GENDER.FEMALE;
		}
		Citizen governor = new Citizen (this, UnityEngine.Random.Range (20, 36), gender, 2);

		MONTH monthGovernor = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
		governor.AssignBirthday (monthGovernor, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthGovernor] + 1), (GameManager.Instance.year - governor.age));

		return governor;
	}
	internal void KillCity(){
		this.incomingGenerals.Clear ();
		this.isUnderAttack = false;
		if (this.rebellion != null) {
			if (this.rebellion.rebelLeader.citizen.city.id == this.id) {
				this.rebellion.rebelLeader.citizen.city = this.rebellion.conqueredCities [0];
			}
		}
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
		this._kingdom.RemoveCityFromKingdom(this);

		if(this.hasKing){
			this.hasKing = false;
			if(this._kingdom.cities.Count > 0){
				this._kingdom.AssignNewKing(null, this._kingdom.cities[0]);
			}
		}

		EventManager.Instance.onCityEverydayTurnActions.RemoveListener (CityEverydayTurnActions);
		EventManager.Instance.onCitizenDiedEvent.RemoveListener (CheckCityDeath);
		this.hexTile.city = null;
		//KingdomManager.Instance.UpdateKingdomAdjacency();
//		for (int i = 0; i < this.kingdom.relationshipsWithOtherKingdoms.Count; i++) {
//			if(this.kingdom.relationshipsWithOtherKingdoms[i].war != null && this.kingdom.relationshipsWithOtherKingdoms[i].isAtWar){
//				if(this.kingdom.relationshipsWithOtherKingdoms[i].war.warPair.kingdom1City.id == this.id || this.kingdom.relationshipsWithOtherKingdoms[i].war.warPair.kingdom2City.id == this.id){
//					this.kingdom.relationshipsWithOtherKingdoms [i].war.UpdateWarPair ();
//				}
//			}
//		}
//		for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
//			KingdomManager.Instance.allKingdoms [i].intlWarCities.Remove (this);
//			KingdomManager.Instance.allKingdoms [i].activeCitiesToAttack.Remove (this);
//			KingdomManager.Instance.allKingdoms [i].activeCitiesPairInWar.RemoveAll (x => x.targetCity.id == this.id || x.sourceCity.id == this.id);
//			KingdomManager.Instance.allKingdoms [i].TargetACityToAttack();
//		}
	}

    /*
     * Conquer this city and transfer ownership to the conqueror
     * */
    internal void ConquerCity(Kingdom conqueror) {
        //when a city's defense reaches zero, it will be conquered by the attacking kingdom, 
        //its initial defense will only be 300HP 
		ResetToDefaultHP();

        //and a random number of settlements (excluding capital) will be destroyed
        int structuresDestroyed = UnityEngine.Random.Range(0, this.structures.Count);
        for (int i = 0; i < structuresDestroyed; i++) {
            this.RemoveTileFromCity(this.structures[UnityEngine.Random.Range(0, this.structures.Count)]);
        }
        

        //Kill all current citizens
        this.KillAllCitizens(DEATH_REASONS.INTERNATIONAL_WAR);

        this.kingdom.RemoveCityFromKingdom(this);

        //Assign new king to conquered kingdom if, conquered city was the home of the current king
        if (this.hasKing) {
            this.hasKing = false;
            if (this._kingdom.cities.Count > 0) {
                this._kingdom.AssignNewKing(null, this._kingdom.cities[0]);
            }
        }
        this.ChangeKingdom(conqueror);
        this.CreateInitialFamilies(false);
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

	internal void MoveCitizenToThisCity(Citizen citizenToMove, bool isFleeing = false){
		citizenToMove.city.RemoveCitizenFromCity(citizenToMove, isFleeing);
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

	internal Citizen CreateAgent(ROLE role, EVENT_TYPES eventType, HexTile targetLocation, int duration, List<HexTile> newPath = null){
		if(role == ROLE.GENERAL){
			return null;
		}
//		int cost = 0;
//		if(eventType != EVENT_TYPES.SECESSION){
//			if(!this.kingdom.CanCreateAgent(role, ref cost)){
//				return null;
//			}
//		}
		if(role == ROLE.REBEL){
			GENDER gender = GENDER.MALE;
			int randomGender = UnityEngine.Random.Range (0, 100);
			if(randomGender < 20){
				gender = GENDER.FEMALE;
			}
			int maxGeneration = this.citizens.Max (x => x.generation);
			Citizen citizen = new Citizen (this, UnityEngine.Random.Range (20, 36), gender, maxGeneration + 1);
			MONTH monthCitizen = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
			citizen.AssignBirthday (monthCitizen, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthCitizen] + 1), (GameManager.Instance.year - governor.age));
			citizen.AssignRole (role);
			this.citizens.Remove (citizen);
			return citizen;
		}else{
            List<HexTile> path = null;
            if (newPath == null) {
                if (role == ROLE.TRADER) {
                    path = PathGenerator.Instance.GetPath(this.hexTile, targetLocation, PATHFINDING_MODE.NORMAL);
                } else {
                    path = PathGenerator.Instance.GetPath(this.hexTile, targetLocation, PATHFINDING_MODE.AVATAR);
                }
                if (path == null) {
                    return null;
                }
            } else {
                path = newPath;
            }

            if (!Utilities.CanReachInTime(eventType, path, duration)){
				return null;
			}
			GENDER gender = GENDER.MALE;
			int randomGender = UnityEngine.Random.Range (0, 100);
			if(randomGender < 20){
				gender = GENDER.FEMALE;
			}
			int maxGeneration = this.citizens.Max (x => x.generation);
			Citizen citizen = new Citizen (this, UnityEngine.Random.Range (20, 36), gender, maxGeneration + 1);
			MONTH monthCitizen = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
			citizen.AssignBirthday (monthCitizen, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthCitizen] + 1), (GameManager.Instance.year - citizen.age));
			citizen.AssignRole (role);
			citizen.assignedRole.targetLocation = targetLocation;
			citizen.assignedRole.targetCity = targetLocation.city;
			citizen.assignedRole.path = path;
			citizen.assignedRole.daysBeforeMoving = path [0].movementDays;
//			this._kingdom.AdjustGold (-cost);
			this.citizens.Remove (citizen);
			return citizen;
		}
	}
	internal Citizen CreateGeneralForCombat(List<HexTile> path, HexTile targetLocation){
//		int cost = 0;
//		if(!this.kingdom.CanCreateAgent(ROLE.GENERAL, ref cost)){
//			return null;
//		}
		List<HexTile> newPath = new List<HexTile> (path);
		if(targetLocation == path[0]){
			newPath.Reverse ();
			newPath.RemoveAt (0);
		}else{
			newPath.RemoveAt (0);
		}

		GENDER gender = GENDER.MALE;
		int randomGender = UnityEngine.Random.Range (0, 100);
		if(randomGender < 20){
			gender = GENDER.FEMALE;
		}
		int maxGeneration = this.citizens.Max (x => x.generation);
		Citizen citizen = new Citizen (this, UnityEngine.Random.Range (20, 36), gender, maxGeneration + 1);
		MONTH monthCitizen = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
		citizen.AssignBirthday (monthCitizen, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthCitizen] + 1), (GameManager.Instance.year - citizen.age));
		citizen.AssignRole (ROLE.GENERAL);
		citizen.assignedRole.targetLocation = targetLocation;
		citizen.assignedRole.targetCity = targetLocation.city;
		citizen.assignedRole.path = newPath;
		citizen.assignedRole.daysBeforeMoving = newPath [0].movementDays;
		((General)citizen.assignedRole).spawnRate = path.Sum (x => x.movementDays) + 2;
		((General)citizen.assignedRole).damage = ((General)citizen.assignedRole).GetDamage();
//		this._kingdom.AdjustGold (-cost);
		this.citizens.Remove (citizen);
		return citizen;
	}
    internal void ChangeKingdom(Kingdom kingdom) {
        kingdom.AddCityToKingdom(this);
        this._kingdom = kingdom;
        for (int i = 0; i < this._ownedTiles.Count; i++) {
            this._ownedTiles[i].ReColorStructure();
        }
        this.hexTile.UpdateNamePlate();
    }

    internal void RemoveTileFromCity(HexTile tileToRemove) {
        this._ownedTiles.Remove(tileToRemove);
        tileToRemove.ResetTile();
        this.UpdateBorderTiles();
        this.UpdateDailyProduction();
        if (tileToRemove.specialResource != RESOURCE.NONE) {
            //this._kingdom.RemoveInvalidTradeRoutes();
            this._kingdom.UpdateAvailableResources();
            this._kingdom.UpdateAllCitiesDailyGrowth();
            this._kingdom.UpdateExpansionRate();
        }
        if (UIManager.Instance.currentlyShowingKingdom.id == this.kingdom.id) {
            this.kingdom.HighlightAllOwnedTilesInKingdom();
        } else {
            this.kingdom.UnHighlightAllOwnedTilesInKingdom();
        }
    }

	internal void HasBeenRaided(){
		if(!this.isRaided){
			this.isRaided = true;
			((Governor)this.governor.assignedRole).UpdateLoyalty ();
		}
		this.raidLoyaltyExpiration = 90;
	}
	internal void HasNotBeenRaided(){
		this.isRaided = false;
		this.raidLoyaltyExpiration = 0;
		((Governor)this.governor.assignedRole).UpdateLoyalty ();
	}
	internal void AttackCity(City targetCity, List<HexTile> path, GameEvent gameEvent, bool isRebel = false){
		EventCreator.Instance.CreateAttackCityEvent (this, targetCity, path, gameEvent, isRebel);
//		int chance = UnityEngine.Random.Range (0, 100);
//		if(chance < this.kingdom.kingdomTypeData.warGeneralCreationRate){
//			
//		}
	}
	internal void ReinforceCity(City targetCity, bool isRebel = false){
		EventCreator.Instance.CreateReinforcementEvent (this, targetCity, isRebel);
	}
//	internal void AttackCampEvent(Camp targetCamp){
//		int chance = UnityEngine.Random.Range (0, 100);
//		if(chance < this.kingdom.kingdomTypeData.warGeneralCreationRate){
//			EventCreator.Instance.CreateAttackCampEvent (this, targetCamp, false);
//		}
//	}
	internal void KillAllCitizens(DEATH_REASONS deathReason){
		int countCitizens = this.citizens.Count;
		for (int i = 0; i < countCitizens; i++) {
			this.citizens [0].Death (deathReason, false, null, true);
		}

	}
	internal void TransferCityToRebellion(){
//		this._kingdom.RemoveCityFromKingdom(this);
		if(this.hasKing){
			this.hasKing = false;
			if(this._kingdom.cities.Count > 0){
				this._kingdom.AssignNewKing(null, this._kingdom.cities[0]);
			}
		}

		this.rebellion.conqueredCities.Add (this);
	}
	internal void TransferRebellionToCity(){
		this.rebellion.conqueredCities.Remove (this);
//		this._kingdom.AddCityToKingdom (this);
	}
	internal void ChangeToRebelFort(Rebellion rebellion, bool isStart = false){
		if (this.hexTile.cityInfo.city != null){
			this.hexTile.cityInfo.rebelIcon.SetActive (true);
		}
		rebellion.warPair.isDone = true;
		this.rebellion = rebellion;
		ResetToDefaultHP();
		EventManager.Instance.onCityEverydayTurnActions.RemoveListener(CityEverydayTurnActions);
		EventManager.Instance.onCitizenDiedEvent.RemoveListener (CheckCityDeath);
		EventManager.Instance.onCityEverydayTurnActions.AddListener(RebelFortEverydayTurnActions);
		KillAllCitizens (DEATH_REASONS.REBELLION);
		TransferCityToRebellion ();
		this.AssignNewGovernor ();
		if(!isStart){
			Log newLog = rebellion.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Rebellion", "rebel_conquer_city");
			newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.CITY_2);
		}

	}
	internal void ChangeToCity(){
		if (this.hexTile.cityInfo.city != null){
			this.hexTile.cityInfo.rebelIcon.SetActive (false);
		}
		this.rebellion.warPair.isDone = true;
		ResetToDefaultHP();
		EventManager.Instance.onCityEverydayTurnActions.RemoveListener(RebelFortEverydayTurnActions);
		EventManager.Instance.onCityEverydayTurnActions.AddListener(CityEverydayTurnActions);
		EventManager.Instance.onCitizenDiedEvent.AddListener(CheckCityDeath);
		KillAllCitizens (DEATH_REASONS.REBELLION);
		TransferRebellionToCity ();
		this.AssignNewGovernor ();
		if(!this.rebellion.rebelLeader.citizen.isKing){
			if(this.rebellion.rebelLeader.citizen.city.id == this.id){
				this.rebellion.rebelLeader.citizen.city = this.rebellion.conqueredCities [0];
			}
		}
		Log newLog = this.rebellion.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Rebellion", "kingdom_conquer_city");
		newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.CITY_2);
		this.rebellion = null;
	}

	internal void ResetToDefaultHP(){
		this._hp = Utilities.defaultCityHP;
	}

	private void CollectEventInTile(HexTile hexTile){
		if(hexTile.gameEventInTile != null){
			if(hexTile.gameEventInTile is BoonOfPower){
				BoonOfPower boonOfPower = (BoonOfPower)hexTile.gameEventInTile;
				boonOfPower.TransferBoonOfPower (this.kingdom, null);
			}
		}
	}
}
