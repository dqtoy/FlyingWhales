using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Panda;

[System.Serializable]
public class City{

    [Header("City Info")]
	public int id;
	public string name;
    private int _hp;
    [NonSerialized] public HexTile hexTile;
	[NonSerialized] private Kingdom _kingdom;
    [NonSerialized] public Citizen governor;
    [NonSerialized] public List<City> adjacentCities;
    [NonSerialized] public List<HexTile> _ownedTiles;
    [NonSerialized] public List<General> incomingGenerals;
    [NonSerialized] public List<Citizen> citizens;
    [NonSerialized] public List<History> cityHistory;
	
	//Resources
	private int _currentGrowth;
    private int _dailyGrowthFromStructures;
    private int _dailyGrowthFromKingdom;
    private int _dailyGrowthBuffs;
    private int _maxGrowth;

    //Balance of Power
    private int _powerPoints;
    private int _defensePoints;
    private int _happinessPoints;
    private int _power;
    private int _defense;

	private int _slavesCount;
	private int raidLoyaltyExpiration;

	[Space(5)]
    [Header("Booleans")]
    public bool hasKing;
    public bool isUnderAttack;
	public bool hasReinforced;
	public bool isStarving;
	public bool isDead;

    [NonSerialized] internal List<HabitableTileDistance> habitableTileDistance; // Lists distance of habitable tiles in ascending order
    [NonSerialized] internal List<HexTile> borderTiles;
    [NonSerialized] internal List<HexTile> outerTiles;
    [NonSerialized] internal Rebellion rebellion;
    [NonSerialized] internal Plague plague;

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
		get{ return (int)((_dailyGrowthFromKingdom + _dailyGrowthFromStructures + _dailyGrowthBuffs + this._slavesCount) * this.kingdom.productionGrowthPercentage); }
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
    public int powerPoints {
        get { return _powerPoints; }
    }
    public int defensePoints {
        get { return _defensePoints; }
    }
    public int happinessPoints {
        get { return _happinessPoints; }
    }
    public int power {
        get { return _power; }
    }
    public int defense {
        get { return _defense; }
    }
	public int hp{
		get{ return this._hp; }
		set{ this._hp = value; }
	}
	public int maxHP{
		get{
			if(this.rebellion == null){
				return Utilities.defaultCityHP +  (40 * this.structures.Count) + (20 * this.kingdom.techLevel);
			}else{
				return 600;
			}
		} //+1 since the structures list does not contain the main hex tile
	}
//	public int maxHPRebel {
//		get{ return 600;}
//	}
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
        this._power = 0;
        this._defense = 0;
		this.adjacentCities = new List<City>();
		this._ownedTiles = new List<HexTile>();
		this.incomingGenerals = new List<General> ();
		this.citizens = new List<Citizen>();
		this.cityHistory = new List<History>();
		this.hasKing = false;
		this.isUnderAttack = false;
		this.hasReinforced = false;
		this.isStarving = false;
		this.isDead = false;
		this.borderTiles = new List<HexTile>();
        this.outerTiles = new List<HexTile>();
		this.habitableTileDistance = new List<HabitableTileDistance> ();
		this.raidLoyaltyExpiration = 0;

		this.hexTile.Occupy (this);
		this.ownedTiles.Add(this.hexTile);
		this.plague = null;
		this._hp = this.maxHP;
        kingdom.SetFogOfWarStateForTile(this.hexTile, FOG_OF_WAR_STATE.VISIBLE);
		hexTile.CheckLairsInRange ();
        LevelUpBalanceOfPower();
        AdjustDefense(100);
        //		this.CreateInitialFamilies();
        Messenger.AddListener("CityEverydayActions", CityEverydayTurnActions);
        Messenger.AddListener("CitizenDied", CheckCityDeath);
        //EventManager.Instance.onCitizenDiedEvent.AddListener(CheckCityDeath);
        GameDate levelUpDueDate = new GameDate(GameManager.Instance.month, 1, GameManager.Instance.year);
        levelUpDueDate.AddMonths(1);
        SchedulingManager.Instance.AddEntry(levelUpDueDate.month, levelUpDueDate.day, levelUpDueDate.year, () => IncreaseBOPAttributesEveryMonth());
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

    #region Citizen Creation Functions
    /*
	 * Initialize City With Initial Citizens aka. Families
	 * */
    internal void CreateInitialFamilies(bool hasRoyalFamily = true) {
        if (hasRoyalFamily) {
            this.hasKing = true;
            this.CreateInitialRoyalFamily();
        }
        this.CreateInitialGovernorFamily();
        this.UpdateDailyProduction();
    }
    internal Citizen CreateNewKing() {
        GENDER gender = GENDER.MALE;
        int randomGender = UnityEngine.Random.Range(0, 100);
        if (randomGender < 20) {
            gender = GENDER.FEMALE;
        }
        Citizen king = new Citizen(this, UnityEngine.Random.Range(20, 36), gender, 2);

        MONTH monthKing = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        king.AssignBirthday(monthKing, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthKing] + 1), (GameManager.Instance.year - king.age));

        return king;
    }
    internal void CreateInitialRoyalFamily() {
        this.kingdom.successionLine.Clear();
        GENDER gender = GENDER.MALE;
        int randomGender = UnityEngine.Random.Range(0, 100);
        if (randomGender < 20) {
            gender = GENDER.FEMALE;
        }
        Citizen king = new Citizen(this, UnityEngine.Random.Range(20, 36), gender, 2);
        Citizen father = new Citizen(this, UnityEngine.Random.Range(60, 81), GENDER.MALE, 1);
        Citizen mother = new Citizen(this, UnityEngine.Random.Range(60, 81), GENDER.FEMALE, 1);

        //father.name = RandomNameGenerator.Instance.GenerateRandomName (this.kingdom.race, father.gender);
        //mother.name = RandomNameGenerator.Instance.GenerateRandomName (this.kingdom.race, mother.gender);

        MONTH monthFather = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthMother = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthKing = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        father.AssignBirthday(monthFather, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthFather] + 1), GameManager.Instance.year - father.age, false);
        mother.AssignBirthday(monthMother, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthMother] + 1), GameManager.Instance.year - mother.age, false);
        king.AssignBirthday(monthKing, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthKing] + 1), (GameManager.Instance.year - king.age));

        king.AssignRole(ROLE.KING);

        //		this.kingdom.king.isKing = true;
        this.kingdom.king.isDirectDescendant = true;

        father.isDirectDescendant = true;
        mother.isDirectDescendant = true;
        father.isDead = true;
        mother.isDead = true;

        this.citizens.Remove(father);
        this.citizens.Remove(mother);

        father.AddChild(this.kingdom.king);
        mother.AddChild(this.kingdom.king);
        king.AddParents(father, mother);

        MarriageManager.Instance.Marry(father, mother);

        MONTH monthSibling = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthSibling2 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        int siblingsChance = UnityEngine.Random.Range(0, 100);
        if (siblingsChance < 25) {
            Citizen sibling = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, this.kingdom.king.age));
            Citizen sibling2 = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, this.kingdom.king.age));

            sibling.AssignBirthday(monthSibling, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling] + 1), (GameManager.Instance.year - sibling.age));
            sibling2.AssignBirthday(monthSibling2, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling2] + 1), (GameManager.Instance.year - sibling2.age));

        } else if (siblingsChance >= 25 && siblingsChance < 75) {
            Citizen sibling = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, this.kingdom.king.age));
            sibling.AssignBirthday(monthSibling, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling] + 1), (GameManager.Instance.year - sibling.age));
        }

        MONTH monthChild1 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthChild2 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthChild3 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        int spouseChance = UnityEngine.Random.Range(0, 100);
        if (spouseChance < 80) {
            Citizen spouse = MarriageManager.Instance.CreateSpouse(this.kingdom.king);

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
    private void CreateInitialGovernorFamily() {
        GENDER gender = GENDER.MALE;
        int randomGender = UnityEngine.Random.Range(0, 100);
        if (randomGender < 20) {
            gender = GENDER.FEMALE;
        }
        Citizen governor = new Citizen(this, UnityEngine.Random.Range(20, 36), gender, 2);
        Citizen father = new Citizen(this, UnityEngine.Random.Range(60, 81), GENDER.MALE, 1);
        Citizen mother = new Citizen(this, UnityEngine.Random.Range(60, 81), GENDER.FEMALE, 1);

        //father.name = RandomNameGenerator.Instance.GenerateRandomName (this.kingdom.race, father.gender);
        //mother.name = RandomNameGenerator.Instance.GenerateRandomName (this.kingdom.race, mother.gender);

        governor.AssignRole(ROLE.GOVERNOR);

        father.AddChild(governor);
        mother.AddChild(governor);
        governor.AddParents(father, mother);

        MONTH monthFather = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthMother = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthGovernor = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        father.AssignBirthday(monthFather, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthFather] + 1), GameManager.Instance.year - father.age, false);
        mother.AssignBirthday(monthMother, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthMother] + 1), GameManager.Instance.year - mother.age, false);
        governor.AssignBirthday(monthGovernor, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthGovernor] + 1), (GameManager.Instance.year - governor.age));

        father.isDead = true;
        mother.isDead = true;
        this.citizens.Remove(father);
        this.citizens.Remove(mother);

        MarriageManager.Instance.Marry(father, mother);

        this.governor = governor;

        MONTH monthSibling = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthSibling2 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        int siblingsChance = UnityEngine.Random.Range(0, 100);
        if (siblingsChance < 25) {
            Citizen sibling = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, governor.age));
            Citizen sibling2 = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, governor.age));

            sibling.AssignBirthday(monthSibling, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling] + 1), (GameManager.Instance.year - sibling.age));
            sibling2.AssignBirthday(monthSibling2, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling2] + 1), (GameManager.Instance.year - sibling2.age));

        } else if (siblingsChance >= 25 && siblingsChance < 75) {
            Citizen sibling = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, governor.age));
            sibling.AssignBirthday(monthSibling, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling] + 1), (GameManager.Instance.year - sibling.age));
        }

        MONTH monthChild1 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthChild2 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthChild3 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        int spouseChance = UnityEngine.Random.Range(0, 100);
        if (spouseChance < 80) {
            Citizen spouse = MarriageManager.Instance.CreateSpouse(governor);

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

        this.cityHistory.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, governor.name + " became the new Governor of " + this.name + ".", HISTORY_IDENTIFIER.NONE));

    }
    #endregion

    /*
     * Compute to get the borders of a city.
     * */
    internal void UpdateBorderTiles(){
        List<HexTile> newBorderTiles = new List<HexTile>();
        List<HexTile> oldBorderTiles = new List<HexTile>(borderTiles);

        //Get outmost owned tiles
        //To get outmost owned tiles, get tiles that still have unoccupied neighbours (unoccupied, meaning there are no structures on it yet)
        List<HexTile> outmostOwnedTiles = new List<HexTile>();
        for (int i = 0; i < this.ownedTiles.Count; i++) {
            HexTile currHexTile = this.ownedTiles[i];
            List<HexTile> currHexTileUnoccupiedNeighbours = currHexTile.AllNeighbours
                .Where(x => x.elevationType != ELEVATION.WATER && !x.isOccupied && !ownedTiles.Contains(x)).ToList();
            if (currHexTileUnoccupiedNeighbours.Count > 0) {
                outmostOwnedTiles.Add(currHexTile);
            }
        }

        //Get border tiles from outmost owned tiles, get x tiles in range
        for (int i = 0; i < outmostOwnedTiles.Count; i++) {
            List<HexTile> possibleBorderTiles = outmostOwnedTiles[i].GetTilesInRange(3).Where(x => !ownedTiles.Contains(x)).ToList();
            newBorderTiles = newBorderTiles.Union(possibleBorderTiles).ToList();
        }

        //Get invalid border tiles (old border tiles that are no longer present in new border tiles)
        HexTile[] invalidBorderTiles = oldBorderTiles.Except(newBorderTiles).ToArray();
        //Get brand new border tiles (new border tiles that are not present in old border tiles)
        HexTile[] brandNewBorderTiles = newBorderTiles.Except(oldBorderTiles).ToArray();

        //Unborderize invalid border tiles
        for (int i = 0; i < invalidBorderTiles.Length; i++) {
            HexTile currInvalidTile = invalidBorderTiles[i];
            currInvalidTile.UnBorderize(this);
            kingdom.SetFogOfWarStateForTile(currInvalidTile, FOG_OF_WAR_STATE.SEEN);
        }

        borderTiles.Clear();
        borderTiles.AddRange(newBorderTiles);

        //Get outer tiles (tiles to set as visible even though they are not owned or borders of this city)
        //if there are no invalid or brand new border tiles, assume that there was no change in borders and skip recomputing outer tiles
        if (invalidBorderTiles.Length > 0 || brandNewBorderTiles.Length > 0) {
            //Reset outer tiles, set them as seen, and remove them as outer tiles of this city
            for (int i = 0; i < this.outerTiles.Count; i++) {
                HexTile currOuterTile = this.outerTiles[i];
                currOuterTile.RemoveAsOuterTileOf(this);
                kingdom.SetFogOfWarStateForTile(currOuterTile, FOG_OF_WAR_STATE.SEEN);
            }
            this.outerTiles.Clear();

            //Get outer tiles based on ownedtiles and border tiles 
            //(outer tiles are those that have neighbours that are not border tiles or owned tiles of this city)
            List<HexTile> outmostTiles = new List<HexTile>();
            for (int i = 0; i < this.ownedTiles.Count; i++) {
                HexTile currOwnedTile = this.ownedTiles[i];
                if (currOwnedTile.AllNeighbours.Where(x => !borderTiles.Contains(x) && !ownedTiles.Contains(x)).Any()) {
                    outmostTiles.Add(currOwnedTile);
                }
            }
            for (int i = 0; i < this.borderTiles.Count; i++) {
                HexTile currBorderTile = this.borderTiles[i];
                if (currBorderTile.AllNeighbours.Where(x => !borderTiles.Contains(x) && !ownedTiles.Contains(x)).Any()) {
                    outmostTiles.Add(currBorderTile);
                }
            }

            //Set outer tiles as visible
            for (int i = 0; i < outmostTiles.Count; i++) {
                HexTile currOutmostTile = outmostTiles[i];
                List<HexTile> currHexTileUnoccupiedNeighbours = currOutmostTile.AllNeighbours
                    .Where(x => !borderTiles.Contains(x) && !ownedTiles.Contains(x)).ToList();

                for (int j = 0; j < currHexTileUnoccupiedNeighbours.Count; j++) {
                    HexTile currNeighbour = currHexTileUnoccupiedNeighbours[j];
                    outerTiles.Add(currNeighbour);
                    currNeighbour.SetAsOuterTileOf(this);
                    kingdom.SetFogOfWarStateForTile(currNeighbour, FOG_OF_WAR_STATE.VISIBLE);
                }
            }
        }

        //Borderize brand new border tiles
        for (int i = 0; i < brandNewBorderTiles.Length; i++) {
            HexTile currBrandNewTile = brandNewBorderTiles[i];
            currBrandNewTile.Borderize(this);
            kingdom.SetFogOfWarStateForTile(currBrandNewTile, FOG_OF_WAR_STATE.VISIBLE);
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
		citizenToOccupyCity.role = ROLE.UNTRAINED;
		citizenToOccupyCity.assignedRole = null;
		citizenToOccupyCity.AssignRole(ROLE.GOVERNOR);
		this.UpdateDailyProduction();
//		this.kingdom.AddInternationalWarCity (this);
		if (UIManager.Instance.kingdomInfoGO.activeSelf) {
			if (UIManager.Instance.currentlyShowingKingdom != null && UIManager.Instance.currentlyShowingKingdom.id == this.kingdom.id) {
				this.kingdom.HighlightAllOwnedTilesInKingdom();
			}
		}

        this.hexTile.CreateCityNamePlate(this);
        KingdomManager.Instance.CheckWarTriggerMisc (this.kingdom, WAR_TRIGGER.TARGET_GAINED_A_CITY);
	}

	/*
	 * Purchase new tile for city. Called in CityTaskManager.
	 * */
	internal void PurchaseTile(HexTile tileToBuy){
        float percentageHP = (float)this._hp / (float)this.maxHP;
		tileToBuy.movementDays = 2;

        //Add tileToBuy to ownedTiles
        this.ownedTiles.Add(tileToBuy);
        //Update Border Tiles based on new owned tile
        this.UpdateBorderTiles();

        //Set tile as occupied
        tileToBuy.Occupy (this);

        //Collect any events on the purchased tile
        tileToBuy.CollectEventOnTile(kingdom);

        //Set tile as visible for the kingdom that bought it
        kingdom.SetFogOfWarStateForTile(tileToBuy, FOG_OF_WAR_STATE.VISIBLE);

		if(Messenger.eventTable.ContainsKey("OnUpdatePath")){
			Messenger.Broadcast<HexTile>("OnUpdatePath", tileToBuy);
		}
        tileToBuy.CreateStructureOnTile(Utilities.GetStructureTypeForResource(kingdom.race, tileToBuy.specialResource));

        //Update necessary data
        this.UpdateDailyProduction();
        this.kingdom.CheckForDiscoveredKingdoms(this);
        //if(otherCity != null) {
        //    otherCity.UpdateBorderTiles();
        //}

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
		tileToBuy.CheckLairsInRange ();
        LevelUpBalanceOfPower();
		this.UpdateHP (percentageHP);


	}

    internal void AddTilesToCity(List<HexTile> hexTilesToAdd) {
        for (int i = 0; i < hexTilesToAdd.Count; i++) {
            HexTile currTile = hexTilesToAdd[i];
            PurchaseTile(currTile);
        }
    }

    internal void ForcePurchaseTile() {
        CityTaskManager ctm = hexTile.GetComponent<CityTaskManager>();
        HexTile nextTileToPurchase = ctm.GetNextTileToPurchase();
        if(nextTileToPurchase == null) {
            Debug.LogWarning("A city is trying to force purchase a tile, but the city task manager has not determined a next tile to purchase");
        } else {
            PurchaseTile(nextTileToPurchase);
            if (nextTileToPurchase.name.Equals(ctm.targetHexTileToPurchase.name)) {
                ctm.targetHexTileToPurchase = null;
                ctm.pathToTargetHexTile.Clear();
            } else {
                ctm.pathToTargetHexTile.Remove(nextTileToPurchase);
            }
        }
        
    }

	/*
	 * Function that listens to onWeekEnd. Performed every tick.
	 * */
	protected void CityEverydayTurnActions(){
		this.hasReinforced = false;
		//this.ProduceGold();
		this.AttemptToIncreaseHP();
		if(this._slavesCount > 0){
			this.AdjustSlavesCount(-1);
		}
	}
	/*
	 * Function that listens to onWeekEnd. Performed every tick.
	 * */
	protected void RebelFortEverydayTurnActions(){
		this.hasReinforced = false;
		this.AttemptToIncreaseHP();
		if(this._slavesCount > 0){
			this.AdjustSlavesCount(-1);
		}
	}
	/*
	 * Increase a city's HP every month.
	 * */
	protected void AttemptToIncreaseHP(){
		if(GameManager.Instance.days == 1){
			int hpIncrease = 0;
			if(this.rebellion == null){
				hpIncrease = 60 + (5 * this.kingdom.techLevel);
				if(this.kingdom.HasWar()){
					hpIncrease = (int)(hpIncrease / 2);
				}
			}else{
				hpIncrease = 100;
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
//			if(this.rebellion != null){
//				this._hp = this.maxHPRebel;
//			}
		}
	}

	public void AdjustHP(int amount){
		this._hp += amount;
		if(this._hp < 0){
			this._hp = 0;
		}
        hexTile.UpdateCityNamePlate();
	}

	private void UpdateHP(float percentageHP){
		this._hp = (int)((float)this.maxHP * percentageHP);
	}

	#region Resource Production
	internal void AddToDailyGrowth(){
        AdjustDailyGrowth(this.totalDailyGrowth);
    }
	internal void ResetDailyGrowth(){
		this._currentGrowth = 0;
	}
    internal void AdjustDailyGrowth(int amount) {
        if (kingdom.isGrowthEnabled) {
            this._currentGrowth += amount;
            this._currentGrowth = Mathf.Clamp(this._currentGrowth, 0, this._maxGrowth);
        }
    }
    internal void AdjustDailyGrowthBuffs(int adjustment) {
        _dailyGrowthBuffs += adjustment;
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

	public void KillCity(){
        RemoveListeners();
        /*
         * Remove irrelevant scripts on hextile
         * */
        UnityEngine.Object.Destroy(this.hexTile.GetComponent<PandaBehaviour>());
        UnityEngine.Object.Destroy(this.hexTile.GetComponent<CityTaskManager>());
        //EventManager.Instance.onCitizenDiedEvent.RemoveListener(CheckCityDeath);
        this.incomingGenerals.Clear ();
		this.isUnderAttack = false;

		if (this.rebellion != null) {
			if (this.rebellion.rebelLeader.citizen.city.id == this.id) {
				this.rebellion.rebelLeader.citizen.city = this.rebellion.conqueredCities [0];
			}
		}

        List<HexTile> tilesToSetAsSeen = new List<HexTile>();

        /*
         * Reset all owned, border and outer tiles!
         * */
        for (int i = 0; i < this.ownedTiles.Count; i++) {
			HexTile currentTile = this.ownedTiles[i];
            currentTile.city = null;
            currentTile.Unoccupy();
            if (!currentTile.isOuterTileOfCities.Intersect(this.kingdom.cities).Any()
                && !currentTile.isBorderOfCities.Intersect(this.kingdom.cities).Any()) {
                kingdom.SetFogOfWarStateForTile(currentTile, FOG_OF_WAR_STATE.SEEN);
            }

        }

		for (int i = 0; i < this.borderTiles.Count; i++) {
			HexTile currentTile = this.borderTiles[i];
            currentTile.UnBorderize(this);
            if (currentTile.isOccupied) {
                if (currentTile.ownedByCity == null || currentTile.ownedByCity.kingdom.id != kingdom.id) {
                    kingdom.SetFogOfWarStateForTile(currentTile, FOG_OF_WAR_STATE.SEEN);
                }
            } else {
                if (!currentTile.isOuterTileOfCities.Intersect(this.kingdom.cities).Any()
                && !currentTile.isBorderOfCities.Intersect(this.kingdom.cities).Any()) {
                    kingdom.SetFogOfWarStateForTile(currentTile, FOG_OF_WAR_STATE.SEEN);
                }
            }
        }

        for (int i = 0; i < this.outerTiles.Count; i++) {
            HexTile currentTile = this.outerTiles[i];
            currentTile.RemoveAsOuterTileOf(this);
            if (currentTile.isOccupied) {
                if(currentTile.ownedByCity == null || currentTile.ownedByCity.kingdom.id != kingdom.id) {
                    kingdom.SetFogOfWarStateForTile(currentTile, FOG_OF_WAR_STATE.SEEN);
                }
            } else {
                if (!currentTile.isOuterTileOfCities.Intersect(this.kingdom.cities).Any()
                && !currentTile.isBorderOfCities.Intersect(this.kingdom.cities).Any()) {
                    kingdom.SetFogOfWarStateForTile(currentTile, FOG_OF_WAR_STATE.SEEN);
                }
            }
        }
		this.ownedTiles.Clear();
		this.borderTiles.Clear();
        this.outerTiles.Clear();

		this.isDead = true;
        //EventManager.Instance.onDeathToGhost.Invoke (this);
        KillAllCitizens(DEATH_REASONS.INTERNATIONAL_WAR);
        this.hexTile.city = null;

        Debug.Log(this.id + " - City " + this.name + " of " + this._kingdom.name + " has been killed!");
        Debug.Log("Stack Trace: " + System.Environment.StackTrace);

        this._kingdom.RemoveCityFromKingdom(this);

        if (!this._kingdom.isDead) {
            if (this.hasKing) {
                this.hasKing = false;
                if (this._kingdom.cities.Count > 0) {
                    this._kingdom.AssignNewKing(null, this._kingdom.cities[0]);
                }
            }
        }
    }

    /*
     * Conquer this city and transfer ownership to the conqueror
     * */
    internal void ConquerCity(Kingdom conqueror) {
        //Transfer items to conqueror
        TransferItemsToConqueror(conqueror);

		KingdomRelationship relationship = this.kingdom.GetRelationshipWithKingdom(conqueror);

		//Trigger Request Peace before changing kingdoms, The losing side has a 20% chance for every city he has lost since the start of the war to send a Request for Peace
		relationship.TriggerRequestPeace();

        //and a random number of settlements (excluding capital) will be destroyed
        int structuresDestroyed = UnityEngine.Random.Range(0, this.structures.Count);
        for (int i = 0; i < structuresDestroyed; i++) {
            this.RemoveTileFromCity(this.structures[UnityEngine.Random.Range(0, this.structures.Count)]);
        }

        List<City> remainingCitiesOfConqueredKingdom = new List<City>(_kingdom.cities);
        for (int i = 0; i < remainingCitiesOfConqueredKingdom.Count; i++) {
            if(remainingCitiesOfConqueredKingdom[i].id == this.id) {
                remainingCitiesOfConqueredKingdom.RemoveAt(i);
                break;
            }
        }

        //Transfer Tiles
        List<HexTile> structureTilesToTransfer = new List<HexTile>(structures);

        /*
         * Reset all owned, border and outer tiles!
         * */
        for (int i = 0; i < this.ownedTiles.Count; i++) {
            HexTile currentTile = this.ownedTiles[i];
            currentTile.city = null;
            currentTile.Unoccupy(true);
            if (!currentTile.isOuterTileOfCities.Intersect(remainingCitiesOfConqueredKingdom).Any()
                && !currentTile.isBorderOfCities.Intersect(remainingCitiesOfConqueredKingdom).Any()) {
                _kingdom.SetFogOfWarStateForTile(currentTile, FOG_OF_WAR_STATE.SEEN);
            }

        }

        for (int i = 0; i < this.borderTiles.Count; i++) {
            HexTile currentTile = this.borderTiles[i];
            currentTile.UnBorderize(this);
            if (currentTile.isOccupied) {
                if (currentTile.ownedByCity == null || currentTile.ownedByCity.kingdom.id != kingdom.id) {
                    _kingdom.SetFogOfWarStateForTile(currentTile, FOG_OF_WAR_STATE.SEEN);
                }
            } else {
                if (!currentTile.isOuterTileOfCities.Intersect(remainingCitiesOfConqueredKingdom).Any()
                && !currentTile.isBorderOfCities.Intersect(remainingCitiesOfConqueredKingdom).Any()) {
                    _kingdom.SetFogOfWarStateForTile(currentTile, FOG_OF_WAR_STATE.SEEN);
                }
            }
        }

        for (int i = 0; i < this.outerTiles.Count; i++) {
            HexTile currentTile = this.outerTiles[i];
            currentTile.RemoveAsOuterTileOf(this);
            if (currentTile.isOccupied) {
                if (currentTile.ownedByCity == null || currentTile.ownedByCity.kingdom.id != kingdom.id) {
                    _kingdom.SetFogOfWarStateForTile(currentTile, FOG_OF_WAR_STATE.SEEN);
                }
            } else {
                if (!currentTile.isOuterTileOfCities.Intersect(remainingCitiesOfConqueredKingdom).Any()
                && !currentTile.isBorderOfCities.Intersect(remainingCitiesOfConqueredKingdom).Any()) {
                    _kingdom.SetFogOfWarStateForTile(currentTile, FOG_OF_WAR_STATE.SEEN);
                }
            }
        }

        //this._kingdom.RemoveCityFromKingdom(this);
        KillAllCitizens(DEATH_REASONS.INTERNATIONAL_WAR);
        RemoveListeners();
        this.isDead = true;
        //Assign new king to conquered kingdom if, conquered city was the home of the current king
        if (this.hasKing) {
            this.hasKing = false;
            if (this._kingdom.cities.Count > 0) {
                this._kingdom.AssignNewKing(null, this._kingdom.cities[0]);
            }
        }


        City newCity = conqueror.CreateNewCityOnTileForKingdom(this.hexTile);
        newCity.name = this.name;
        newCity.AddTilesToCity(structureTilesToTransfer);
        newCity.CreateInitialFamilies(false);
        newCity.hexTile.CreateCityNamePlate(newCity);
        //when a city's defense reaches zero, it will be conquered by the attacking kingdom, 
        //its initial defense will only be 300HP + (20HP x tech level)
        newCity.WarDefeatedHP();
        KingdomManager.Instance.CheckWarTriggerMisc(newCity.kingdom, WAR_TRIGGER.TARGET_GAINED_A_CITY);

        if (UIManager.Instance.currentlyShowingKingdom.id == conqueror.id) {
            conqueror.HighlightAllOwnedTilesInKingdom();
        }

        Debug.Log("Created new city on: " + this.hexTile.name + " because " + conqueror.name + " has conquered it!");
    }
    private void TransferItemsToConqueror(Kingdom conqueror){
		for(int i = 0; i < this.ownedTiles.Count; i++){
			if(this.ownedTiles[i].hasFirst){
				conqueror.CollectFirst();
			}
			if(this.ownedTiles[i].hasKeystone){
				conqueror.CollectKeystone();
			}
		}
	}

    private void RemoveListeners() {
        Messenger.RemoveListener("CityEverydayActions", CityEverydayTurnActions);
        Messenger.RemoveListener("CitizenDied", CheckCityDeath);
        Messenger.RemoveListener("OnDayEnd", this.hexTile.gameObject.GetComponent<PandaBehaviour>().Tick);
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

	internal void MoveCitizenToThisCity(Citizen citizenToMove, bool isFleeing = false){
		citizenToMove.city.RemoveCitizenFromCity(citizenToMove, isFleeing);
		this.AddCitizenToCity(citizenToMove);
	}

	internal Citizen CreateAgent(ROLE role, EVENT_TYPES eventType, HexTile targetLocation, int duration, List<HexTile> newPath = null){
		if(role == ROLE.GENERAL){
			return null;
		}
		if(role == ROLE.REBEL){
			GENDER gender = GENDER.MALE;
			int randomGender = UnityEngine.Random.Range (0, 100);
			if(randomGender < 20){
				gender = GENDER.FEMALE;
			}
			Citizen citizen = new Citizen (this, UnityEngine.Random.Range (20, 36), gender, 1);
			MONTH monthCitizen = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
			citizen.AssignBirthday (monthCitizen, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthCitizen] + 1), (GameManager.Instance.year - citizen.age), false);
			citizen.AssignRole (role);
			this.citizens.Remove (citizen);
			return citizen;
		}else{
            List<HexTile> path = null;
			PATHFINDING_MODE pathMode = PATHFINDING_MODE.AVATAR;
            if (newPath == null) {
                if (role == ROLE.TRADER) {
                    pathMode = PATHFINDING_MODE.NORMAL;
				}else if (role == ROLE.RANGER) {
					pathMode = PATHFINDING_MODE.NO_HIDDEN_TILES;
				}else {
					pathMode = PATHFINDING_MODE.AVATAR;
                }
				path = PathGenerator.Instance.GetPath(this.hexTile, targetLocation, pathMode, BASE_RESOURCE_TYPE.STONE, this.kingdom);

				if(role != ROLE.RANGER){
					if (path == null) {
						return null;
					}
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
			Citizen citizen = new Citizen (this, UnityEngine.Random.Range (20, 36), gender, 1);
			MONTH monthCitizen = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
			citizen.AssignBirthday (monthCitizen, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthCitizen] + 1), (GameManager.Instance.year - citizen.age), false);
			citizen.AssignRole (role);
			citizen.assignedRole.targetLocation = targetLocation;
			citizen.assignedRole.path = path;
			if(targetLocation != null){
				citizen.assignedRole.targetCity = targetLocation.city;
			}
			if(path != null){
				citizen.assignedRole.daysBeforeMoving = path [0].movementDays;
			}
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
//		int maxGeneration = this.citizens.Max (x => x.generation);
		Citizen citizen = new Citizen (this, UnityEngine.Random.Range (20, 36), gender, 1);
		MONTH monthCitizen = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
		citizen.AssignBirthday (monthCitizen, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthCitizen] + 1), (GameManager.Instance.year - citizen.age), false);
		citizen.AssignRole (ROLE.GENERAL);
		citizen.assignedRole.targetLocation = targetLocation;
		citizen.assignedRole.targetCity = targetLocation.city;
		citizen.assignedRole.path = newPath;
		citizen.assignedRole.daysBeforeMoving = newPath [0].movementDays;

        General general = (General)citizen.assignedRole;
        if(kingdom.weaponsCount > 0) {
            general.AdjustWeaponCount(1);
            kingdom.AdjustWeaponsCount(-1);
        }
		if(citizen.city.kingdom.serumsOfAlacrity > 0){
			int chance = UnityEngine.Random.Range(0,100);
			if(chance < 35){
				citizen.city.kingdom.AdjustSerumOfAlacrity(-1);
				general.InjectSerumOfAlacrity();
			}
		}
        general.spawnRate = path.Sum (x => x.movementDays) + 2;
        general.damage = ((General)citizen.assignedRole).GetDamage();
//		this._kingdom.AdjustGold (-cost);
		this.citizens.Remove (citizen);
		return citizen;
	}
	internal Citizen CreateGeneralForLair(List<HexTile> path, HexTile targetLocation){
		GENDER gender = GENDER.MALE;
		int randomGender = UnityEngine.Random.Range (0, 100);
		if(randomGender < 20){
			gender = GENDER.FEMALE;
		}
//		int maxGeneration = this.citizens.Max (x => x.generation);
		Citizen citizen = new Citizen (this, UnityEngine.Random.Range (20, 36), gender, 1);
		MONTH monthCitizen = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
		citizen.AssignBirthday (monthCitizen, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthCitizen] + 1), (GameManager.Instance.year - citizen.age), false);
		citizen.AssignRole (ROLE.GENERAL);
		citizen.assignedRole.targetLocation = targetLocation;
		citizen.assignedRole.path = path;
		citizen.assignedRole.daysBeforeMoving = path [0].movementDays;

		General general = (General)citizen.assignedRole;
		if(kingdom.weaponsCount > 0) {
			general.AdjustWeaponCount(1);
			kingdom.AdjustWeaponsCount(-1);
		}
		if(citizen.city.kingdom.serumsOfAlacrity > 0){
			int chance = UnityEngine.Random.Range(0,100);
			if(chance < 35){
				citizen.city.kingdom.AdjustSerumOfAlacrity(-1);
				general.InjectSerumOfAlacrity();
			}
		}
		general.spawnRate = path.Sum (x => x.movementDays) + 2;
		general.damage = ((General)citizen.assignedRole).GetDamage();
		//		this._kingdom.AdjustGold (-cost);
		this.citizens.Remove (citizen);
		return citizen;
	}
    internal void ChangeKingdom(Kingdom otherKingdom) {
        List<HexTile> allTilesOfCity = new List<HexTile>();
        allTilesOfCity.AddRange(ownedTiles);
        allTilesOfCity.AddRange(borderTiles);
        allTilesOfCity.AddRange(outerTiles);
        for (int i = 0; i < allTilesOfCity.Count; i++) {
            HexTile currTile = allTilesOfCity[i];
            if(!currTile.isBorderOfCities.Intersect(_kingdom.cities).Any() && !currTile.isOuterTileOfCities.Intersect(_kingdom.cities).Any() && 
                (currTile.ownedByCity == null || !_kingdom.cities.Contains(currTile.ownedByCity))) {
                _kingdom.SetFogOfWarStateForTile(currTile, FOG_OF_WAR_STATE.SEEN);
            }
            otherKingdom.SetFogOfWarStateForTile(currTile, FOG_OF_WAR_STATE.VISIBLE);
        }
        otherKingdom.AddCityToKingdom(this);
        this._kingdom = otherKingdom;
        for (int i = 0; i < this._ownedTiles.Count; i++) {
            this._ownedTiles[i].ReColorStructure();
            this._ownedTiles[i].SetMinimapTileColor(_kingdom.kingdomColor);
        }
        this.hexTile.UpdateCityNamePlate();
    }

    internal void RemoveTileFromCity(HexTile tileToRemove) {
        this._ownedTiles.Remove(tileToRemove);
        tileToRemove.Unoccupy();
        kingdom.SetFogOfWarStateForTile(tileToRemove, FOG_OF_WAR_STATE.SEEN);
        //tileToRemove.isVisibleByCities.Remove(this);
        //tileToRemove.ResetTile();
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

        if(this.plague != null) {
            this.plague.CheckIfCityIsCured(this);
        }
    }
	internal void AttackCity(City targetCity, List<HexTile> path, GameEvent gameEvent, bool isRebel = false){
		EventCreator.Instance.CreateAttackCityEvent (this, targetCity, path, gameEvent, isRebel);
	}
	internal void ReinforceCity(City targetCity, bool isRebel = false){
		EventCreator.Instance.CreateReinforcementEvent (this, targetCity, isRebel);
	}
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
		if(rebellion == null){
			return;
		}
		if(!isStart){
			if (this.hexTile.cityInfo.city != null){
				this.hexTile.cityInfo.rebelIcon.SetActive (true);
			}
		}
		rebellion.warPair.isDone = true;
		this.rebellion = rebellion;
		ResetToDefaultHP();
		Messenger.RemoveListener("CityEverydayActions", CityEverydayTurnActions);
        Messenger.RemoveListener("CitizenDied", CheckCityDeath);
		Messenger.AddListener("CityEverydayActions", RebelFortEverydayTurnActions);
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
		Messenger.RemoveListener("CityEverydayActions", RebelFortEverydayTurnActions);
		Messenger.AddListener("CityEverydayActions", CityEverydayTurnActions);
        Messenger.AddListener("CitizenDied", CheckCityDeath);
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
	internal void WarDefeatedHP(){
		this._hp = Utilities.defaultCityHP + (20 * this.kingdom.techLevel);
	}

	internal void RetaliateToMonster(HexTile targetHextile){
		FOG_OF_WAR_STATE fogOfWarState = this.kingdom.GetFogOfWarStateOfTile (targetHextile);
		if(fogOfWarState != FOG_OF_WAR_STATE.HIDDEN && targetHextile.lair != null){
			List<HexTile> path = PathGenerator.Instance.GetPath(this.hexTile, targetHextile, PATHFINDING_MODE.AVATAR);
			if(path != null){
				EventCreator.Instance.CreateAttackLairEvent(this, targetHextile, path, null);
			}
		}
	}
	internal void AdjustSlavesCount(int amount){
		this._slavesCount += amount;
		if(this._slavesCount < 0){
			this._slavesCount = 0;
		}
	}

    #region Balance Of Power
    internal void AdjustPower(int adjustment) {
        _power += adjustment;
        _kingdom.AdjustBasePower(adjustment);
    }
    internal void AdjustDefense(int adjustment) {
        _defense += adjustment;
        _kingdom.AdjustBaseDefense(adjustment);
    }
    internal void IncreaseBOPAttributesEveryMonth() {
        if (!isDead) {
            int powerIncrease = _powerPoints * 3;
            int defenseIncrease = _defensePoints * 4;
            //Each City contributes a base +4 Happiness
            int happinessIncrease = 4 + (_happinessPoints * 2);
            int happinessDecrease = (structures.Count * 3);
            AdjustPower(powerIncrease);
            AdjustDefense(defenseIncrease);
            //TODO: Add checking for militarize, put happiness increase to power but keep decrease in happiness
            _kingdom.AdjustHappiness(happinessIncrease - happinessDecrease);

            GameDate levelUpDueDate = new GameDate(GameManager.Instance.month, 1, GameManager.Instance.year);
            levelUpDueDate.AddMonths(1);
            SchedulingManager.Instance.AddEntry(levelUpDueDate.month, levelUpDueDate.day, levelUpDueDate.year, () => IncreaseBOPAttributesEveryMonth());
        }
    }

    private void LevelUpBalanceOfPower() {
        _powerPoints += _kingdom.kingdomTypeData.productionPointsSpend.power;
        _defensePoints += _kingdom.kingdomTypeData.productionPointsSpend.defense;
        _happinessPoints += _kingdom.kingdomTypeData.productionPointsSpend.happiness;
    }
    #endregion
}
