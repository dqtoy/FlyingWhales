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
    private Region _region;
    [NonSerialized] public HexTile hexTile;
	[NonSerialized] private Kingdom _kingdom;
    [NonSerialized] public Citizen governor;
    [NonSerialized] public List<HexTile> _ownedTiles;
    [NonSerialized] public List<General> incomingGenerals;
    //[NonSerialized] public List<Citizen> citizens;
    [NonSerialized] public List<History> cityHistory;
	
	//Resources
	private int _currentGrowth;
    private int _dailyGrowthFromStructures;
    private int _dailyGrowthBuffs;
    private int _maxGrowth;
	private int _dailyGrowthResourceBenefits;
	private float _productionGrowthPercentage;
	private int _foodCount;
	private int _materialCount;
	private int _oreCount;
	private int _virtualFoodCount;
	private int _virtualMaterialCount;
	private int _virtualOreCount;

    //Balance of Power
    //private int _powerPoints;
    //private int _defensePoints;
    //private int _techPoints;
    private int _weapons;
    private int _armor;

	private int _slavesCount;
	private int raidLoyaltyExpiration;

	private int _population;
	private int _populationGrowth;

	private int _soldiers;

	internal PandaBehaviour _cityBT;

	[Space(5)]
    [Header("Booleans")]
    //internal bool hasKing;
	internal bool isPaired;
	internal bool isAttacking;
	internal bool isDefending;
	internal bool hasReinforced;
	internal bool isDead;
	private bool _isStarving;
	private bool _isNoCityGrowth;

    [NonSerialized] internal List<HabitableTileDistance> habitableTileDistance; // Lists distance of habitable tiles in ascending order
    [NonSerialized] internal List<HexTile> borderTiles;
    [NonSerialized] internal List<HexTile> outerBorderTiles;
//    [NonSerialized] internal Rebellions rebellion;
    [NonSerialized] internal Plague plague;

	protected const int HP_INCREASE = 5;
	private int increaseHpInterval = 0;

	private int _bonusStability;

    private List<Guard> _activeGuards;
    private float _cityBounds;

	private int[] populationIncreasePool;

    #region getters/setters
    internal Region region {
        get { return _region; }
    }
	public Kingdom kingdom{
		get{ return this._kingdom; }
	}
	public int currentGrowth{
		get{ return this._currentGrowth; }
	}
	public int totalDailyGrowth{
        //get{ return (int)((_dailyGrowthFromStructures + _dailyGrowthBuffs + this._slavesCount + this._dailyGrowthResourceBenefits) * this._productionGrowthPercentage); }
//        get { return Mathf.FloorToInt((baseDailyGrowth + _dailyGrowthResourceBenefits) * _productionGrowthPercentage); }
		get { return GetNaturalResourceLevel(); }
    }
    internal int baseDailyGrowth {
        get { return GetBaseDailyGrowth(); }
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
        get { return kingdom.kingdomTypeData.productionPointsSpend.power + cityLevel + kingdom.techLevel; }
    }
    public int defensePoints {
        get { return kingdom.kingdomTypeData.productionPointsSpend.defense + cityLevel + kingdom.techLevel; }
    }
    public int techPoints {
        get { return kingdom.kingdomTypeData.productionPointsSpend.tech + cityLevel; }
    }
    public int weapons {
        get { return _weapons; }
    }
    public int armor {
        get { return _armor; }
    }
	public float productionGrowthPercentage {
		get { return this._productionGrowthPercentage; }
	}
	public int hp{
		get{ return this._hp; }
		set{ this._hp = value; }
	}
	public int maxHP{
		get{
			return Utilities.defaultCityHP +  (40 * this.structures.Count) + (20 * this.kingdom.techLevel);
		} //+1 since the structures list does not contain the main hex tile
	}
    public List<HexTile> ownedTiles {
        get { return this._ownedTiles; }
    }
	public int bonusStability{
		get { return this._bonusStability;}
	}
    internal int cityLevel {
        get { return ownedTiles.Count; }
    }
    internal List<Citizen> citizens {
        get { return kingdom.citizens[this]; }
    }
    internal float cityBounds {
        get { return _cityBounds; }
    }
	internal int foodCount{
		get { return this._foodCount; }
	}
	internal int virtualFoodCount{
		get { return this._foodCount + this._virtualFoodCount; }
	}
	internal int virtualMaterialCount{
		get { return this._materialCount + this._virtualMaterialCount; }
	}
	internal int virtualOreCount{
		get { return this._oreCount + this._virtualOreCount; }
	}
	internal int materialCount{
		get { return this._materialCount; }
	}
	internal int oreCount{
		get { return this._oreCount; }
	}
	internal int foodRequirement{
		get { return 80 + (20 * this.cityLevel); }
	}
	internal int materialRequirement{
		get { return 80 + (20 * this.cityLevel); }
	}
	internal int oreRequirement{
		get { return 80 + (20 * this.cityLevel); }
	}
	internal int population {
		get { return _population; }
	}
	internal int populationGrowth {
		get { return this._populationGrowth + this.cityLevel; }
	}
	internal int populationCapacity {
		get { return 500 + (100 * this.cityLevel); }
	}
	internal int foodExcessCapacity{
//		get { return this._region.foodMultiplierCapacity * this.foodRequirement; }
		get { return 2 * this.foodRequirement; }
	}
	internal int materialExcessCapacity{
//		get { return this._region.materialMultiplierCapacity * this.materialRequirement; }
		get { return 2 * this.materialRequirement; }
	}
	internal int oreExcessCapacity{
//		get { return this._region.oreMultiplierCapacity * this.oreRequirement; }
		get { return 2 * this.oreRequirement; }
	}
	internal int foodCapacity{
		get { return 10 * this.foodRequirement; }
	}
	internal int materialCapacity{
		get { return 10 * this.materialRequirement; }
	}
	internal int oreCapacity{
		get { return 10 * this.oreRequirement; }
	}
	internal int soldiers{
		get { return this._soldiers; }
	}
	internal int soldiersMax {
		get { return Mathf.FloorToInt(this._population * this._kingdom.draftRate); }
	}
	private int[] soldiersCap{
		get { return new int[]{ GetSoldiersCap (), this._oreCount, this._population }; }
	}
    #endregion

    public City(HexTile hexTile, Kingdom kingdom){
		this.id = Utilities.SetID(this);
		this.hexTile = hexTile;
        this._region = hexTile.region;
        this._kingdom = kingdom;
		this.name = RandomNameGenerator.Instance.GenerateCityName(this._kingdom.race);
		this.governor = null;
        this._weapons = 0;
        this._armor = 0;
		this._bonusStability = 0;
		this._ownedTiles = new List<HexTile>();
		this.incomingGenerals = new List<General> ();
		//this.citizens = new List<Citizen>();
		this.cityHistory = new List<History>();
		//this.hasKing = false;
		this.isPaired = false;
		this.isAttacking = false;
		this.isDefending = false;
		this.hasReinforced = false;
		this._isStarving = false;
		this._isNoCityGrowth = false;
		this.isDead = false;
		this.borderTiles = new List<HexTile>();
        this.outerBorderTiles = new List<HexTile>();
		this.habitableTileDistance = new List<HabitableTileDistance> ();
		this.raidLoyaltyExpiration = 0;
		this._foodCount = 0;
		this._materialCount = 0;
		this._oreCount = 0;

        this.hexTile.Occupy (this);
		this.ownedTiles.Add(this.hexTile);
		this.plague = null;
		this._hp = this.maxHP;
		this.populationIncreasePool = new int[]{ 30, 32, 34, 36, 38, 40 };
		this._populationGrowth = populationIncreasePool [UnityEngine.Random.Range (0, populationIncreasePool.Length)];
		this._cityBT = null;
        _activeGuards = new List<Guard>();
        _cityBounds = 50f;
        kingdom.SetFogOfWarStateForTile(this.hexTile, FOG_OF_WAR_STATE.VISIBLE);

//		AdjustPopulation (50);

		GameDate increaseDueDate = new GameDate(GameManager.Instance.month, 1, GameManager.Instance.year);
		increaseDueDate.AddMonths(1);
		SchedulingManager.Instance.AddEntry(increaseDueDate.month, increaseDueDate.day, increaseDueDate.year, () => MonthlyAction());
    }
	private void MonthlyAction(){
		if (!this.isDead) {
			ConsumeResources ();
			IncreasePopulationPerMonth ();
			IncreaseSoldiersPerMonth ();
			this._cityBT.Tick ();

			GameDate increaseDueDate = new GameDate(GameManager.Instance.month, 1, GameManager.Instance.year);
			increaseDueDate.AddMonths(1);
			SchedulingManager.Instance.AddEntry(increaseDueDate.month, increaseDueDate.day, increaseDueDate.year, () => MonthlyAction());
		}
	}
    internal void SetupInitialValues() {
        hexTile.CheckLairsInRange();
		AdjustFoodCount(this.foodExcessCapacity);
        SetProductionGrowthPercentage(1f);
        DailyGrowthResourceBenefits();
        AddOneTimeResourceBenefits();
        Messenger.AddListener("CityEverydayActions", CityEverydayTurnActions);
        if (GameManager.Instance.enableGameAgents) {
            SchedulingManager.Instance.AddEntry(GameManager.Instance.month, GameManager.daysInMonth[GameManager.Instance.month], GameManager.Instance.year, () => SpawnGuardsAtEndOfMonth());
        }
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
    #region Border Tile Functions
    internal void PopulateBorderTiles() {
        borderTiles = new List<HexTile>(_region.tilesInRegion);
        outerBorderTiles = new List<HexTile>(_region.outerGridTilesInRegion);
        for (int i = 0; i < borderTiles.Count; i++) {
            HexTile currTile = borderTiles[i];
            currTile.Borderize(this);
        }

    }
    internal void UnPopulateBorderTiles() {
        for (int i = 0; i < borderTiles.Count; i++) {
            HexTile currTile = borderTiles[i];
            currTile.UnBorderize(this);
            currTile.SetMinimapTileColor(Color.black);
        }
        outerBorderTiles.Clear();
    }
    #endregion

    #region Tile Highlight
    internal void HighlightAllOwnedTiles(float alpha) {
        Color color = this.kingdom.kingdomColor;
        color.a = alpha;
        for (int i = 0; i < this.ownedTiles.Count; i++) {
            HexTile currentTile = this.ownedTiles[i];
            currentTile.kingdomColorSprite.color = color;
            currentTile.kingdomColorSprite.gameObject.SetActive(true);
            currentTile.SetMinimapTileColor(this.kingdom.kingdomColor);
        }

        for (int i = 0; i < this.borderTiles.Count; i++) {
            HexTile currentTile = this.borderTiles[i];
            currentTile.kingdomColorSprite.color = color;
            currentTile.kingdomColorSprite.gameObject.SetActive(true);
            currentTile.SetMinimapTileColor(this.kingdom.kingdomColor);
        }

        for (int i = 0; i < this.outerBorderTiles.Count; i++) {
            HexTile currentTile = this.outerBorderTiles[i];
            currentTile.kingdomColorSprite.color = color;
            currentTile.kingdomColorSprite.gameObject.SetActive(true);
            currentTile.SetMinimapTileColor(this.kingdom.kingdomColor);
        }
    }
    internal void UnHighlightAllOwnedTiles() {
        for (int i = 0; i < this.ownedTiles.Count; i++) {
            HexTile currentTile = this.ownedTiles[i];
            currentTile.kingdomColorSprite.gameObject.SetActive(false);
        }
        for (int i = 0; i < this.borderTiles.Count; i++) {
            HexTile currentTile = this.borderTiles[i];
            currentTile.kingdomColorSprite.gameObject.SetActive(false);
        }
        for (int i = 0; i < this.outerBorderTiles.Count; i++) {
            HexTile currentTile = this.outerBorderTiles[i];
            currentTile.kingdomColorSprite.gameObject.SetActive(false);
        }
    }
    #endregion

    internal void ExpandToThisCity(Citizen citizenToOccupyCity){
        //this.AddCitizenToCity(citizenToOccupyCity);
        //citizenToOccupyCity.role = ROLE.UNTRAINED;
        //citizenToOccupyCity.assignedRole = null;
        //citizenToOccupyCity.AssignRole(ROLE.GOVERNOR);
        //citizenToOccupyCity.GenerateCharacterValues();
        //citizenToOccupyCity.UpdateKingOpinion();
        //CreateInitialFamilies(false);
        this.kingdom.CreateNewGovernorFamily(this);
        this.hexTile.CreateCityNamePlate(this);
        HighlightAllOwnedTiles(69f / 255f);
        UIManager.Instance.UpdateKingdomCitiesMenu();
        UIManager.Instance.UpdateMinimapInfo();
	}

	/*
	 * Purchase new tile for city. Called in CityTaskManager.
	 * */
	internal void PurchaseTile(HexTile tileToBuy){
        float percentageHP = (float)this._hp / (float)this.maxHP;
		tileToBuy.movementDays = 2;

        //Add tileToBuy to ownedTiles
        this.ownedTiles.Add(tileToBuy);

        //Set tile as occupied
        tileToBuy.Occupy (this);

        ////Set tile as visible for the kingdom that bought it
        //kingdom.SetFogOfWarStateForTile(tileToBuy, FOG_OF_WAR_STATE.VISIBLE);

		if(Messenger.eventTable.ContainsKey("OnUpdatePath")){
			Messenger.Broadcast<HexTile>("OnUpdatePath", tileToBuy);
		}

        //Update necessary data
        this.UpdateDailyProduction();

        tileToBuy.CheckLairsInRange ();
        UIManager.Instance.UpdateMinimapInfo();
    }

    internal void AddTilesToCity(List<HexTile> hexTilesToAdd) {
        for (int i = 0; i < hexTilesToAdd.Count; i++) {
            HexTile currTile = hexTilesToAdd[i];
            PurchaseTile(currTile);
        }
    }

    internal void ForcePurchaseTile() {
        CityTaskManager ctm = hexTile.GetComponent<CityTaskManager>();
        PurchaseTile(ctm.targetHexTileToPurchase);
        ctm.targetHexTileToPurchase = null;
    }

	/*
	 * Function that listens to onWeekEnd. Performed every tick.
	 * */
	protected void CityEverydayTurnActions(){
		this.hasReinforced = false;
	}
	/*
	 * Function that listens to onWeekEnd. Performed every tick.
	 * */
	protected void RebelFortEverydayTurnActions(){
		this.hasReinforced = false;
	}
	/*
	 * Increase a city's HP every month.
	 * */
	protected void AttemptToIncreaseHP(){
		if(GameManager.Instance.days == 1){
			int hpIncrease = 0;
			hpIncrease = 60 + (5 * this.kingdom.techLevel);
			if(this.kingdom.HasWar()){
				hpIncrease = (int)(hpIncrease / 2);
			}
			this.IncreaseHP (hpIncrease);
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

	#region Resources
    private int GetBaseDailyGrowth() {
		int naturalResourceLevel = GetNaturalResourceLevel();
		double workerValue = Math.Sqrt(5 * (_kingdom.workers / _kingdom.cities.Count));
        int cities = _kingdom.cities.Count;
		return (int)((2 * naturalResourceLevel * workerValue) / (workerValue + naturalResourceLevel));
    }
	internal void AddToDailyGrowth(){
        AdjustDailyGrowth(this.totalDailyGrowth);
    }
	internal void ResetDailyGrowth(){
		this._currentGrowth = 0;
	}
    internal void AdjustDailyGrowth(int amount) {
		if(!this._isNoCityGrowth){
			this._currentGrowth += amount;
			this._currentGrowth = Mathf.Clamp(this._currentGrowth, 0, this._maxGrowth);
		}
    }
	internal void UpdateDailyProduction(){
		this._maxGrowth = CityGenerator.Instance.cityMonthlyMaxGrowthMultiplier[this.cityLevel - 1] * 150;
	}
	internal void AdjustFoodCount(int amount){
		this._foodCount += amount;
		this._foodCount = Mathf.Clamp (this._foodCount, 0, this.foodCapacity);
		this.hexTile.UpdateCityFoodMaterialOreUI ();
		CheckFoodSupply ();
	}
	internal void AdjustVirtualFoodCount(int amount){
		this._virtualFoodCount += amount;
		if(this._virtualFoodCount < 0){
			this._virtualFoodCount = 0;
		}
	}
	internal void SetFoodCount(int amount){
		this._foodCount = amount;
		this.hexTile.UpdateCityFoodMaterialOreUI ();
	}
	internal void AdjustMaterialCount(int amount){
		this._materialCount += amount;
		this._materialCount = Mathf.Clamp (this._materialCount, 0, this.materialCapacity);
		this.hexTile.UpdateCityFoodMaterialOreUI ();
		CheckMaterialSupply ();
	}
	internal void AdjustVirtualMaterialCount(int amount){
		this._virtualMaterialCount += amount;
		if(this._virtualMaterialCount < 0){
			this._virtualMaterialCount = 0;
		}
	}
	internal void SetMaterialCount(int amount){
		this._materialCount = amount;
		this.hexTile.UpdateCityFoodMaterialOreUI ();
	}
	internal void AdjustOreCount(int amount){
		this._oreCount += amount;
		this._oreCount = Mathf.Clamp (this._oreCount, 0, this.oreCapacity);
		this.hexTile.UpdateCityFoodMaterialOreUI ();
		CheckOreSupply ();
	}
	internal void AdjustVirtualOreCount(int amount){
		this._virtualOreCount += amount;
		if(this._virtualOreCount < 0){
			this._virtualOreCount = 0;
		}
	}
	internal void SetOreCount(int amount){
		this._oreCount = amount;
		this.hexTile.UpdateCityFoodMaterialOreUI ();
	}
	private void ConsumeResources(){
		ConsumeFood ();
		ConsumeMaterial ();
		ConsumeOre ();
	}
	private void ConsumeFood(){
		int foodToBeConsumed = this.foodRequirement;
		if(this._foodCount >= foodToBeConsumed){
			AdjustFoodCount (-foodToBeConsumed);
			this._isStarving = false;
		}else{
			AdjustFoodCount (-this._foodCount);
			//Suffer Population Decline
			PopulationDecline();
		}
	}
	private void ConsumeMaterial(){
		int materialToBeConsumed = this.materialRequirement;
		if(this._materialCount >= materialToBeConsumed){
			AdjustMaterialCount (-materialToBeConsumed);
			this._isNoCityGrowth = false;
		}else{
			AdjustMaterialCount (-this._materialCount);
			//Suffer No City Growth
			this._isNoCityGrowth = true;
		}
	}
	private void ConsumeOre(){
		int oreToBeConsumed = this.oreRequirement;
		if(this._oreCount >= oreToBeConsumed){
			AdjustOreCount (-oreToBeConsumed);
		}else{
			AdjustOreCount (-this._oreCount);
			//Suffer No City Growth
		}
	}
	private void PopulationDecline(){
		this._isStarving = true;
		if(this.structures.Count > 0){
			int chance = UnityEngine.Random.Range (0, 100);
			if(chance < 5){
				this.RemoveTileFromCity (this.structures [this.structures.Count - 1]);
			}
		}
	}
	private void CheckFoodSupply(){
		if(this._kingdom.cities.Count > 1){
			int foodCap = this.foodExcessCapacity;
			if(this.foodCount > foodCap){
				int excessFood = this.foodCount - foodCap;
				if (excessFood >= foodCap) {
					//Send caravan to other cities to give excess food
					SendFoodToOtherCities (excessFood);
				}
			}
		}
	}
	private void CheckMaterialSupply(){
		if (this._kingdom.cities.Count > 1) {
			int materialCap = this.materialExcessCapacity;
			if (this.materialCount > materialCap) {
				int excessMaterial = this.materialCount - materialCap;
				if (excessMaterial >= materialCap) {
					//Send caravan to other cities to give excess material
					SendMaterialToOtherCities (excessMaterial);
				}
			}
		}
	}
	private void CheckOreSupply(){
		if (this._kingdom.cities.Count > 1) {
			int oreCap = this.oreExcessCapacity;
			if (this.oreCount > oreCap) {
				int excessOre = this.oreCount - oreCap;
				if(excessOre >= oreCap){
					//Send caravan to other cities to give excess ore
					SendOreToOtherCities(excessOre);
				}
			}
		}
	}
	private void SendFoodToOtherCities(int foodAmount){
		SendResourceThreadPool.Instance.AddToThreadPool (new SendResourceThread (foodAmount, 0, 0, RESOURCE_TYPE.FOOD, this.hexTile, this));
	}
	private void SendMaterialToOtherCities(int materialAmount){
		SendResourceThreadPool.Instance.AddToThreadPool (new SendResourceThread (0, materialAmount, 0, RESOURCE_TYPE.MATERIAL, this.hexTile, this));
	}
	private void SendOreToOtherCities(int oreAmount){
		SendResourceThreadPool.Instance.AddToThreadPool (new SendResourceThread (0, 0, oreAmount, RESOURCE_TYPE.ORE, this.hexTile, this));
	}
	internal void ReceiveSendResourceThread(int foodAmount, int materialAmount, int oreAmount, RESOURCE_TYPE resourceType, HexTile sourceHextile, HexTile targetHextile, City targetCity, List<HexTile> path){
		if (targetHextile != null && targetHextile.city != null && targetCity != null && (path != null || path.Count > 0)) {
			if(targetCity.id == targetHextile.city.id){
				if (resourceType == RESOURCE_TYPE.FOOD) {
					this.AdjustFoodCount (-foodAmount);
				}else if (resourceType == RESOURCE_TYPE.MATERIAL) {
					this.AdjustMaterialCount (-materialAmount);
				}else if (resourceType == RESOURCE_TYPE.ORE) {
					this.AdjustOreCount (-oreAmount);
				}
				EventCreator.Instance.CreateSendResourceEvent (foodAmount, materialAmount, oreAmount, resourceType, sourceHextile, targetHextile, this, path);
			}
		}
	}
	#endregion

	public void KillCity(){
        RemoveListeners();
		RemoveOneTimeResourceBenefits();
        KillActiveGuards();
        /*
         * Remove irrelevant scripts on hextile
         * */
        UnityEngine.Object.Destroy(this.hexTile.GetComponent<PandaBehaviour>());
        UnityEngine.Object.Destroy(this.hexTile.GetComponent<CityTaskManager>());

		this.isPaired = false;

        region.RemoveOccupant();
		this.hexTile.DestroyConnections ();

        //Destroy owned settlements
        for (int i = 0; i < ownedTiles.Count; i++) {
            HexTile currentTile = this.ownedTiles[i];
            currentTile.city = null;
            currentTile.Unoccupy();
        }
        for (int i = 0; i < borderTiles.Count; i++) {
            HexTile currentTile = this.borderTiles[i];
            currentTile.kingdomColorSprite.color = Color.white;
            currentTile.kingdomColorSprite.gameObject.SetActive(false);
        }
        UnPopulateBorderTiles();        
       
        this.ownedTiles.Clear();
		this.borderTiles.Clear();
        this.outerBorderTiles.Clear();

		this.isDead = true;
		ChangeAttackingState (false);
		ChangeDefendingState (false);
        this.hexTile.city = null;

        Debug.Log(this.id + " - City " + this.name + " of " + this._kingdom.name + " has been killed!");
        Debug.Log("Stack Trace: " + System.Environment.StackTrace);

        List<Citizen> remainingCitizens = this._kingdom.RemoveCityFromKingdom(this);
        for (int i = 0; i < remainingCitizens.Count; i++) {
            Citizen currCitizen = remainingCitizens[i];
            currCitizen.Death(DEATH_REASONS.INTERNATIONAL_WAR, true);
        }

        CameraMove.Instance.UpdateMinimapTexture();
		if(Messenger.eventTable.ContainsKey("CityDied")){
			Messenger.Broadcast<City>("CityDied", this);
		}

    }

    /*
     * Conquer this city and transfer ownership to the conqueror
     * */
	internal void ConquerCity(Kingdom conqueror, Warfare warfare) {
        RemoveOneTimeResourceBenefits();
        KillActiveGuards();
        //Combat invasion should reduce City level by half.
        int halfOfCityLevel = Mathf.FloorToInt(this.ownedTiles.Count / 2);
        for (int i = 0; i < halfOfCityLevel; i++) {
            this.RemoveTileFromCity(this.structures[UnityEngine.Random.Range(0, this.structures.Count)]);
        }

        //Transfer Tiles
        List<HexTile> structureTilesToTransfer = new List<HexTile>(structures);

        region.RemoveOccupant();
        //Destroy owned settlements
        for (int i = 0; i < ownedTiles.Count; i++) {
            HexTile currentTile = this.ownedTiles[i];
            currentTile.city = null;
            currentTile.Unoccupy();
        }
        UnPopulateBorderTiles();
        
        RemoveListeners();
        this.isDead = true;
		ChangeAttackingState (false);
		ChangeDefendingState (false);


        City newCity = conqueror.CreateNewCityOnTileForKingdom(this.hexTile);
        newCity.name = this.name;
        conqueror.CreateNewGovernorFamily(newCity);
        newCity.region.SetOccupant(newCity);
        newCity.region.CheckForDiscoveredKingdoms();
        newCity.AddTilesToCity(structureTilesToTransfer);        
        newCity.hexTile.CreateCityNamePlate(newCity);
        newCity.SetupInitialValues();
        newCity.HighlightAllOwnedTiles(69f / 255f);

        for (int i = 0; i < conqueror.discoveredKingdoms.Count; i++) {
            Kingdom otherKingdom = conqueror.discoveredKingdoms[i];
            if (otherKingdom.regionFogOfWarDict[newCity.region] != FOG_OF_WAR_STATE.VISIBLE) {
                otherKingdom.SetFogOfWarStateForRegion(newCity.region, FOG_OF_WAR_STATE.SEEN);
            }
        }
        //When occupying an invaded city, Stability is reduced by 2.
        newCity.kingdom.AdjustStability(-2);
        //newCity.kingdom.AddStabilityDecreaseBecauseOfInvasion();


		List<Citizen> remainingCitizens = this._kingdom.RemoveCityFromKingdom(this);
        for (int i = 0; i < remainingCitizens.Count; i++) {
            Citizen currCitizen = remainingCitizens[i];
            currCitizen.Death(DEATH_REASONS.INTERNATIONAL_WAR, true);
        }

        Debug.Log("Created new city on: " + this.hexTile.name + " because " + conqueror.name + " has conquered it!");
        CameraMove.Instance.UpdateMinimapTexture();
		if (Messenger.eventTable.ContainsKey ("CityDied")) {
			Messenger.Broadcast<City> ("CityDied", this);
		}

		Log newLog = warfare.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Warfare", "invade");
		newLog.AddToFillers (conqueror, conqueror.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (newCity, newCity.name, LOG_IDENTIFIER.CITY_2);
		warfare.ShowUINotification (newLog, new HashSet<Kingdom>() { conqueror, this.kingdom });
    }

    internal void RemoveListeners() {
		Messenger.RemoveListener("CityEverydayActions", CityEverydayTurnActions);
		Messenger.RemoveListener("OnDayEnd", this.hexTile.gameObject.GetComponent<PandaBehaviour>().Tick);
    }
		
	internal Citizen CreateNewAgent(ROLE role, HexTile targetLocation, HexTile sourceLocation = null){
//		if(role == ROLE.GENERAL){
//			return null;
//		}
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
			//this.citizens.Remove (citizen);
			return citizen;
		}else{
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
			if(targetLocation != null){
				citizen.assignedRole.targetCity = targetLocation.city;
			}
			if(sourceLocation != null){
				citizen.assignedRole.location = sourceLocation;
			}
			return citizen;
		}
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
				path = PathGenerator.Instance.GetPath(this.hexTile, targetLocation, pathMode, this.kingdom);

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
			return citizen;
		}
	}
	internal Citizen CreateGeneralForCombat(List<HexTile> path, HexTile targetLocation, bool isRebel = false){
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
		Citizen citizen = new Citizen (this, UnityEngine.Random.Range (20, 36), gender, 1);
		MONTH monthCitizen = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
		citizen.AssignBirthday (monthCitizen, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthCitizen] + 1), (GameManager.Instance.year - citizen.age), false);
		citizen.AssignRole (ROLE.GENERAL);
		citizen.assignedRole.targetLocation = targetLocation;
		citizen.assignedRole.targetCity = targetLocation.city;
		citizen.assignedRole.path = newPath;
		citizen.assignedRole.daysBeforeMoving = newPath [0].movementDays;

//        General general = (General)citizen.assignedRole;
//        general.spawnRate = path.Sum (x => x.movementDays) + 2;
//		if(!isRebel){
//			general.damage = ((General)citizen.assignedRole).GetDamage();
//		}
		return citizen;
	}
	internal Citizen CreateGeneralForLair(List<HexTile> path, HexTile targetLocation){
		GENDER gender = GENDER.MALE;
		int randomGender = UnityEngine.Random.Range (0, 100);
		if(randomGender < 20){
			gender = GENDER.FEMALE;
		}
		Citizen citizen = new Citizen (this, UnityEngine.Random.Range (20, 36), gender, 1);
		MONTH monthCitizen = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
		citizen.AssignBirthday (monthCitizen, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthCitizen] + 1), (GameManager.Instance.year - citizen.age), false);
		citizen.AssignRole (ROLE.GENERAL);
		citizen.assignedRole.targetLocation = targetLocation;
		citizen.assignedRole.path = path;
		citizen.assignedRole.daysBeforeMoving = path [0].movementDays;

		General general = (General)citizen.assignedRole;
//		general.spawnRate = path.Sum (x => x.movementDays) + 2;
//		general.damage = ((General)citizen.assignedRole).GetDamage();
		return citizen;
	}
    internal void ChangeKingdom(Kingdom otherKingdom, List<Citizen> citizensToAdd) {
        _region.RemoveOccupant();
        KillActiveGuards();

		this._kingdom.AdjustPopulation (-this._population, false);
		this._kingdom.AdjustSoldiers (-this._soldiers, false);

        otherKingdom.AddCityToKingdom(this);
        this._kingdom = otherKingdom;
        _region.SetOccupant(this);

		this._kingdom.AdjustPopulation (this._population, false);
		this._kingdom.AdjustSoldiers (this._soldiers, false);

        for (int i = 0; i < citizensToAdd.Count; i++) {
            Citizen citizenToAdd = citizensToAdd[i];
            otherKingdom.AddCitizenToKingdom(citizenToAdd, this);
        }
        for (int i = 0; i < this._ownedTiles.Count; i++) {
            this._ownedTiles[i].ReColorStructure();
            this._ownedTiles[i].SetMinimapTileColor(_kingdom.kingdomColor);
        }
        this.hexTile.UpdateCityNamePlate();
        CameraMove.Instance.UpdateMinimapTexture();
    }

    internal void RemoveTileFromCity(HexTile tileToRemove) {
        this._ownedTiles.Remove(tileToRemove);
        tileToRemove.Unoccupy();
        //kingdom.SetFogOfWarStateForTile(tileToRemove, FOG_OF_WAR_STATE.SEEN);
        //tileToRemove.isVisibleByCities.Remove(this);
        //tileToRemove.ResetTile();
        //this.UpdateBorderTiles();
        this.UpdateDailyProduction();
//        if (tileToRemove.specialResource != RESOURCE.NONE) {
            //this._kingdom.RemoveInvalidTradeRoutes();
//            this._kingdom.UpdateAvailableResources();
//            this._kingdom.UpdateAllCitiesDailyGrowth();
//            this._kingdom.UpdateExpansionRate();
//        }
        //if (UIManager.Instance.currentlyShowingKingdom.id == this.kingdom.id) {
        //    this.kingdom.HighlightAllOwnedTilesInKingdom();
        //} else {
        //    this.kingdom.UnHighlightAllOwnedTilesInKingdom();
        //}

        //if(this.plague != null) {
        //    this.plague.CheckIfCityIsCured(this);
        //}
    }

	internal void ResetToDefaultHP(){
		this._hp = Utilities.defaultCityHP;
	}
	internal void AdjustSlavesCount(int amount){
		this._slavesCount += amount;
		if(this._slavesCount < 0){
			this._slavesCount = 0;
		}
	}

    #region Balance Of Power
    internal void SetWeapons(int newPower) {
        //_kingdom.AdjustBasePower(-_power);
        _weapons = 0;
        AdjustWeapons(newPower);
        KingdomManager.Instance.UpdateKingdomList();
    }
    internal void SetArmor(int newDefense) {
        //_kingdom.AdjustBaseDefense(-_defense);
        _armor = 0;
        AdjustArmor(newDefense);
        KingdomManager.Instance.UpdateKingdomList();
    }
    internal void AdjustWeapons(int adjustment) {
        _weapons += adjustment;
        //_kingdom.AdjustBasePower(adjustment);
        _weapons = Mathf.Max(_weapons, 0);
        KingdomManager.Instance.UpdateKingdomList();
    }
    internal void AdjustArmor(int adjustment) {
        _armor += adjustment;
        //_kingdom.AdjustBaseDefense(adjustment);
        _armor = Mathf.Max(_armor, 0);
        KingdomManager.Instance.UpdateKingdomList();
    }
	//internal void MonthlyResourceBenefits(ref int weaponsIncrease, ref int armorIncrease, int stabilityIncrease){
	//	switch (this._region.specialResource){
	//	case RESOURCE.CORN:
	//		stabilityIncrease += 1;
	//		break;
	//	case RESOURCE.WHEAT:
	//		stabilityIncrease += 2;
	//		break;
	//	case RESOURCE.RICE:
	//		stabilityIncrease += 3;
	//		break;
	//	case RESOURCE.OAK:
	//		armorIncrease += 5;
	//		break;
	//	case RESOURCE.EBONY:
	//		armorIncrease += 10;
	//		break;
	//	case RESOURCE.GRANITE:
	//		weaponsIncrease += 5;
	//		break;
	//	case RESOURCE.SLATE:
	//		weaponsIncrease += 10;
	//		break;
	//	case RESOURCE.COBALT:
	//		break;
	//	}
	//}
	private void DailyGrowthResourceBenefits(){
		switch (this._region.specialResource){
		case RESOURCE.DEER:
			this._dailyGrowthResourceBenefits = 10;
			break;
		case RESOURCE.PIG:
			this._dailyGrowthResourceBenefits = 15;
			break;
		case RESOURCE.BEHEMOTH:
			this._dailyGrowthResourceBenefits = 20;
			break;
		default:
			this._dailyGrowthResourceBenefits = 0;
			break;
		}
	}
	private void AddOneTimeResourceBenefits(){
		switch (this._region.specialResource){
		case RESOURCE.MANA_STONE:
			SetProductionGrowthPercentage(2f);
			for (int i = 0; i < this.kingdom.cities.Count; i++) {
				if (this.kingdom.cities[i].id != this.id) {
					this.kingdom.cities[i].SetProductionGrowthPercentage(1.25f);
				}
			}
			break;
		case RESOURCE.MITHRIL:
			this.kingdom.SetTechProductionPercentage(2f);
			break;
		}
	}
	private void RemoveOneTimeResourceBenefits(){
		switch (this._region.specialResource){
		case RESOURCE.MANA_STONE:
			for (int i = 0; i < this.kingdom.cities.Count; i++) {
				this.kingdom.cities[i].SetProductionGrowthPercentage(1f);
			}
			break;
		case RESOURCE.MITHRIL:
			this.kingdom.SetTechProductionPercentage(1f);
			break;
		}
	}
	internal void SetProductionGrowthPercentage(float amount){
		this._productionGrowthPercentage = amount;
	}
    #endregion

	internal void ChangeAttackingState(bool state){
		this.isAttacking = state;
		if(state){
			this.isDefending = false;
		}
	}

	internal void ChangeDefendingState(bool state){
		this.isDefending = state;
		if(state){
			this.isAttacking = false;
		}
	}

	internal int GetNaturalResourceLevel(){
		return (int)((float)this._region.naturalResourceLevel [kingdom.race] * (1f + (0.1f * this._kingdom.techLevel)));
	}

    #region Agent Functions
    internal void SpawnGuardsAtEndOfMonth() {
        if (this.isDead) {
            return;
        }
        int maxGuards = 1 + (cityLevel / 3);
        if(_activeGuards.Count < maxGuards) {
            //Spawn a new guard to patrol the city
            _activeGuards.Add(SpawnPatrollingGuard());
        }
        GameDate nextSpawnDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
        nextSpawnDate.AddMonths(1);
        nextSpawnDate.SetDate(nextSpawnDate.month, GameManager.daysInMonth[nextSpawnDate.month], nextSpawnDate.year);
        SchedulingManager.Instance.AddEntry(nextSpawnDate, () => SpawnGuardsAtEndOfMonth());
    }
    private Guard SpawnPatrollingGuard() {
        Guard newGuard = new Guard(this);
        AIBehaviour attackBehaviour = new AttackHostiles(newGuard);
        AIBehaviour fleeBehaviour = new RunAwayFromHostile(newGuard);
        AIBehaviour randomBehaviour = new PatrolCity(newGuard, this);
        newGuard.SetAttackBehaviour(attackBehaviour);
        newGuard.SetFleeBehaviour(fleeBehaviour);
        newGuard.SetRandomBehaviour(randomBehaviour);
        GameObject guardObj = ObjectPoolManager.Instance.InstantiateObjectFromPool("AgentGO", this.hexTile.transform.position, Quaternion.identity, this.hexTile.transform);
        guardObj.transform.localPosition = Vector3.zero;
        AgentObject agentObj = guardObj.GetComponent<AgentObject>();
        newGuard.SetAgentObj(agentObj);
        agentObj.Initialize(newGuard, new int[] { _kingdom.kingdomTagIndex });
        return newGuard;
    }
    private void KillActiveGuards() {
        for (int i = 0; i < _activeGuards.Count; i++) {
            _activeGuards[i].KillAgent();
        }
        _activeGuards.Clear();
    }
    #endregion

	#region Population
	private void IncreasePopulationPerMonth(){
		if(!this._isStarving){
			AdjustPopulation (this.populationGrowth);
		}
	}
	internal void AdjustPopulation(int adjustment, bool isUpdateKingdomList = true) {
		int supposedPopulation = this._population + adjustment;
		int populationCap = this.populationCapacity;
		if(supposedPopulation > populationCap){
			int addedPopulation = populationCap - this._population;
			this._population += addedPopulation;
			this._kingdom.AdjustPopulation (addedPopulation, isUpdateKingdomList);
		}else if (supposedPopulation < 0){
			this._kingdom.AdjustPopulation (-this._population, isUpdateKingdomList);
			this._population = 0;
		}else{
			this._population += adjustment;
			this._kingdom.AdjustPopulation (adjustment, isUpdateKingdomList);
		}
			
		if(this._population == 0) {
			if(supposedPopulation < 0){
				AdjustSoldiers (supposedPopulation);
				if(this._soldiers == 0){
					KillCity ();
				}
			}
		}
	}
	internal void SetPopulation(int newPopulation) {
		this._population = newPopulation;
		this._kingdom.UpdatePopulation ();
	}
	#endregion

	#region Soldiers
	internal void AdjustSoldiers(int amount, bool isUpdateKingdomList = true){
		int supposedSoldiers = this._soldiers + amount;
		int maxSoldiers = this.soldiersMax;
		if(supposedSoldiers > maxSoldiers){
			int addedSoldiers = maxSoldiers - this._soldiers;
			this._soldiers += addedSoldiers;
			this._kingdom.AdjustSoldiers (addedSoldiers, isUpdateKingdomList);
		}else if (supposedSoldiers < 0){
			this._kingdom.AdjustSoldiers (-this._soldiers, isUpdateKingdomList);
			this._soldiers = 0;
		}else{
			this._soldiers += amount;
			this._kingdom.AdjustSoldiers (amount, isUpdateKingdomList);
		}
	}
	internal void SetSoldiers(int amount) {
		this._soldiers = amount;
		this._kingdom.UpdatePopulation ();
		//		KingdomManager.Instance.UpdateKingdomList();
	}
	private void IncreaseSoldiersPerMonth(){
		int soldiersCap = this.soldiersCap.Min();
		int kingdomMissingSoldiers = this._kingdom.soldiersCap - this._kingdom.soldiersCount;

		if(soldiersCap > kingdomMissingSoldiers){
			soldiersCap = kingdomMissingSoldiers;
		}
		if(soldiersCap > 0){
			AdjustPopulation (-soldiersCap, false);
			AdjustOreCount (-soldiersCap);
			AdjustSoldiers (soldiersCap);
		}else{
			if(!this._isStarving && this._population > 0){
				int normalSoldiersCap = GetSoldiersCap ();
				int halfNormalSoldiersCap = normalSoldiersCap / 2;
				if(halfNormalSoldiersCap > this._population){
					halfNormalSoldiersCap = this._population;
				}
				AdjustPopulation (-halfNormalSoldiersCap, false);
				AdjustOreCount (-halfNormalSoldiersCap);
				AdjustSoldiers (halfNormalSoldiersCap);
			}
		}
	}
	private int GetSoldiersCap(){
		int baseAmount = 6;
		if(this._kingdom.king.military == TRAIT.HOSTILE){
			baseAmount += 2;
		}else if(this._kingdom.king.military == TRAIT.MILITANT){
			baseAmount += 1;
		}else if(this._kingdom.king.military == TRAIT.PACIFIST){
			baseAmount += -1;
		}

		if(this.governor.military == TRAIT.HOSTILE){
			baseAmount += 1;
		}else if(this.governor.military == TRAIT.MILITANT){
			baseAmount += 1;
		}else if(this.governor.military == TRAIT.PACIFIST){
			baseAmount += -1;
		}

		baseAmount += (this.cityLevel / 2);
		return baseAmount;
	}

	internal General RaiseSoldiers(int soldiers, HexTile targetLocation, bool isIdle){
		City targetCity = targetLocation.city;
		Citizen citizen = this.CreateNewAgent (ROLE.GENERAL, targetLocation);
		if(citizen != null){
			General general	= (General)citizen.assignedRole;
			general.SetSoldiers (soldiers);
			this.AdjustSoldiers (-soldiers);
			general.isIdle = isIdle;
			return general;
		}
		return null;
	}
	#endregion
}
