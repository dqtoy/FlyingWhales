using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Kingdom{
	public int id;
	public string name;
	public RACE race;
	public int[] horoscope; 

	[SerializeField]
	private KingdomTypeData _kingdomTypeData;
	private Kingdom _sourceKingdom;

	internal List<City> cities;
	internal Citizen king;
	internal List<Citizen> successionLine;
	internal List<Citizen> pretenders;
//	public List<Citizen> royaltyList;
	internal List<CityWar> holderIntlWarCities;

	internal BASE_RESOURCE_TYPE basicResource;
	internal BASE_RESOURCE_TYPE rareResource;

	internal List<RelationshipKingdom> relationshipsWithOtherKingdoms;

	internal Color kingdomColor;
	internal List<History> kingdomHistory;

	internal List<City> adjacentCitiesFromOtherKingdoms;
	internal List<Kingdom> adjacentKingdoms;

	public int expansionChance = 1;

	public KINGDOM_TYPE kingdomType {
		get { 
			if (this._kingdomTypeData == null) {
				return KINGDOM_TYPE.NONE;
			}
			return this._kingdomTypeData.kingdomType; 
		}
	}

	public KingdomTypeData kingdomTypeData {
		get { 
			return this._kingdomTypeData; 
		}
	}

	public Kingdom sourceKingdom {
		get { 
			return this._sourceKingdom;
		}
	}

	// Kingdom constructor paramters
	//	race - the race of this kingdom
	//	cities - the cities that this kingdom will initially own
	//	sourceKingdom (optional) - the kingdom from which this new kingdom came from
	public Kingdom(RACE race, List<HexTile> cities, Kingdom sourceKingdom = null) {
		this.id = Utilities.SetID(this);
		this.race = race;
		this.name = RandomNameGenerator.Instance.GenerateKingdomName(this.race);
		this.king = null;
		this.successionLine = new List<Citizen>();
		this.pretenders = new List<Citizen> ();
		this.cities = new List<City>();
		this.holderIntlWarCities = new List<CityWar> ();
		this.kingdomHistory = new List<History>();
		this.kingdomColor = Utilities.GetColorForKingdom();
		this.adjacentCitiesFromOtherKingdoms = new List<City>();
		this.adjacentKingdoms = new List<Kingdom>();

		this._sourceKingdom = sourceKingdom;
		// Determine what type of Kingdom this will be upon initialization.
		this._kingdomTypeData = null;
		this.UpdateKingdomTypeData();

		if (race == RACE.HUMANS) {
			this.basicResource = BASE_RESOURCE_TYPE.STONE;
			this.rareResource = BASE_RESOURCE_TYPE.MITHRIL;
		} else if (race == RACE.ELVES) {
			this.basicResource = BASE_RESOURCE_TYPE.WOOD;
			this.rareResource = BASE_RESOURCE_TYPE.MANA_STONE;
		} else if (race == RACE.MINGONS) {
			this.basicResource = BASE_RESOURCE_TYPE.WOOD;
			this.rareResource = BASE_RESOURCE_TYPE.NONE;
		} else {
			this.basicResource = BASE_RESOURCE_TYPE.STONE;
			this.rareResource = BASE_RESOURCE_TYPE.COBALT;
		}

		for (int i = 0; i < cities.Count; i++) {
			this.AddTileToKingdom(cities[i]);
		}

		// For the kingdom's first city, setup its distance towards other habitable tiles.
		HexTile habitableTile;
		if (this.basicResource == BASE_RESOURCE_TYPE.STONE) {			
			for (int i = 0; i < CityGenerator.Instance.stoneHabitableTiles.Count; i++) {	
				habitableTile = CityGenerator.Instance.stoneHabitableTiles [i];
				this.cities[0].AddHabitableTileDistance(habitableTile, PathGenerator.Instance.GetDistanceBetweenTwoTiles (this.cities[0].hexTile , habitableTile));
			}

		} else if (this.basicResource == BASE_RESOURCE_TYPE.WOOD) {
			for (int i = 0; i < CityGenerator.Instance.woodHabitableTiles.Count; i++) {	
				habitableTile = CityGenerator.Instance.woodHabitableTiles [i];
				this.cities[0].AddHabitableTileDistance(habitableTile, PathGenerator.Instance.GetDistanceBetweenTwoTiles (this.cities[0].hexTile , habitableTile));
			}

		}
//		Debug.Log ("Kingdom: " + this.name + " : " + this.cities [0].habitableTileDistance.Count);
		//this.cities [0].OrderHabitableTileDistanceList ();

		this.relationshipsWithOtherKingdoms = new List<RelationshipKingdom>();
		this.CreateInitialRelationships();
		EventManager.Instance.onCreateNewKingdomEvent.AddListener(NewKingdomCreated);
		EventManager.Instance.onWeekEnd.AddListener(AttemptToExpand);
		this.kingdomHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "This kingdom was born.", HISTORY_IDENTIFIER.NONE));
	}

	// Updates this kingdom's type and horoscope
	public void UpdateKingdomTypeData() {
		// Update Kingdom Type whenever the kingdom expands to a new city
		KingdomTypeData prevKingdomTypeData = this._kingdomTypeData;
		this._kingdomTypeData = StoryTellingManager.Instance.InitializeKingdomType (this);

		// If the Kingdom Type Data changed
		if (_kingdomTypeData != prevKingdomTypeData) {			
			// Update horoscope
			if (prevKingdomTypeData == null) {
				this.horoscope = GetHoroscope ();
			} else {				
				this.horoscope = GetHoroscope (prevKingdomTypeData.kingdomType);
			}
			// Update expansion chance
			this.expansionChance = this.kingdomTypeData.expansionRate;
		}


	}

	internal int[] GetHoroscope(KINGDOM_TYPE prevKingdomType = KINGDOM_TYPE.NONE){
		int[] newHoroscope = new int[2];

		if (this._kingdomTypeData.kingdomType == KINGDOM_TYPE.BARBARIC_TRIBE) {
			newHoroscope [0] = UnityEngine.Random.Range (0, 2);
			newHoroscope [1] = 0;
		} else if (this._kingdomTypeData.kingdomType == KINGDOM_TYPE.HERMIT_TRIBE) {
			newHoroscope [0] = UnityEngine.Random.Range (0, 2);
			newHoroscope [1] = 1;
		} else if (this._kingdomTypeData.kingdomType == KINGDOM_TYPE.RELIGIOUS_TRIBE) {
			newHoroscope [0] = 0;
			newHoroscope [1] = UnityEngine.Random.Range (0, 2);
		} else if (this._kingdomTypeData.kingdomType == KINGDOM_TYPE.OPPORTUNISTIC_TRIBE) {
			newHoroscope [0] = 1;
			newHoroscope [1] = UnityEngine.Random.Range (0, 2);
		} else if (this._kingdomTypeData.kingdomType == KINGDOM_TYPE.NOBLE_KINGDOM) {
			newHoroscope [0] = 0;
			newHoroscope [1] = 0;
		} else if (this._kingdomTypeData.kingdomType == KINGDOM_TYPE.EVIL_EMPIRE) {
			newHoroscope [0] = 1;
			newHoroscope [1] = 0;
		} else if (this._kingdomTypeData.kingdomType == KINGDOM_TYPE.MERCHANT_NATION) {
			newHoroscope [0] = 0;
			newHoroscope [1] = 1;
		} else if (this._kingdomTypeData.kingdomType == KINGDOM_TYPE.CHAOTIC_STATE) {
			newHoroscope [0] = 1;
			newHoroscope [1] = 1;
		} else if (this._kingdomTypeData.kingdomType == KINGDOM_TYPE.RIGHTEOUS_SUPERPOWER) {
			if (prevKingdomType == KINGDOM_TYPE.NOBLE_KINGDOM) {
				newHoroscope [0] = 0;
				newHoroscope [1] = UnityEngine.Random.Range (0, 2);
			} else if (prevKingdomType == KINGDOM_TYPE.MERCHANT_NATION) {
				newHoroscope [0] = UnityEngine.Random.Range (0, 2);
				newHoroscope [1] = 1;				
			}
		} else if (this._kingdomTypeData.kingdomType == KINGDOM_TYPE.WICKED_SUPERPOWER) {
			if (prevKingdomType == KINGDOM_TYPE.EVIL_EMPIRE) {
				newHoroscope [0] = 1;
				newHoroscope [1] = UnityEngine.Random.Range (0, 2);
			} else if (prevKingdomType == KINGDOM_TYPE.CHAOTIC_STATE) {
				newHoroscope [0] = UnityEngine.Random.Range (0, 2);
				newHoroscope [1] = 0;				
			}
		}

		return newHoroscope;
	}

	// Function to call if you want to determine whether the Kingdom is still alive or dead
	// At the moment, a Kingdom is considered dead if it doesnt have any cities.
	public bool isAlive() {
		if (this.cities.Count > 0) {
			return true;
		}
		return false;
	}

	protected void CreateInitialRelationships() {
		for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
			if (KingdomManager.Instance.allKingdoms[i].id != this.id) {
				this.relationshipsWithOtherKingdoms.Add (new RelationshipKingdom(this, KingdomManager.Instance.allKingdoms [i]));
			}
		}
	}

	protected void NewKingdomCreated(Kingdom createdKingdom){
		//Add relationship to newly created kingdom
		if (createdKingdom.id == this.id) {
			return;
		}
		for (int i = 0; i < this.relationshipsWithOtherKingdoms.Count; i++) {
			if (this.relationshipsWithOtherKingdoms [i].targetKingdom.id == createdKingdom.id) {
				//this kingdom already has a relationship with created kingdom!
				return;
			}
		}
		this.relationshipsWithOtherKingdoms.Add(new RelationshipKingdom(this, createdKingdom));
	}

	protected void AttemptToExpand(){
		if (EventManager.Instance.GetEventsStartedByKingdom(this, new EVENT_TYPES[]{EVENT_TYPES.EXPANSION}).Where(x => x.isActive).Count() > 0) {
			return;
		}

		int chance = Random.Range (0, 300 + (50 * this.cities.Count));
		if (chance < this.expansionChance) {
		
			List<City> citiesThatCanExpand = new List<City> ();
			List<Citizen> allUnassignedAdultCitizens = new List<Citizen> ();
			List<Resource> expansionCost = new List<Resource> () {
				new Resource (BASE_RESOURCE_TYPE.GOLD, 0)
			};

			Citizen citizenToLeadExpansion = this.cities [0].CreateCitizenForExpansion();
			this.cities [0].AdjustResources (expansionCost);
			HexTile hexTileToExpandTo = CityGenerator.Instance.GetNearestHabitableTile (this.cities[0]);
			if(hexTileToExpandTo != null && citizenToLeadExpansion != null){
				Expansion newExpansionEvent = new Expansion (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, citizenToLeadExpansion, hexTileToExpandTo);
			}

		}
	}

	internal List<Citizen> GetAllCitizensForMarriage(Citizen citizen){
		List<Citizen> elligibleCitizens = new List<Citizen>();
		for (int i = 0; i < this.cities.Count; i++) {
			if (citizen.gender == GENDER.MALE) {
				elligibleCitizens.AddRange (this.cities [i].elligibleBachelorettes);
			} else {
				elligibleCitizens.AddRange (this.cities [i].elligibleBachelors);
			}
		}
		return elligibleCitizens;
	}

	internal void AddTileToKingdom(HexTile tile){
		CityGenerator.Instance.CreateNewCity (tile, this);
	}

	internal List<Citizen> GetAllCitizensInKingdom(){
		List<Citizen> allCitizens = new List<Citizen>();
		for (int i = 0; i < this.cities.Count; i++) {
			allCitizens.AddRange (this.cities [i].citizens);
		}
		return allCitizens;
	}

	internal void UpdateKingSuccession(){
		List<Citizen> orderedMaleRoyalties = this.successionLine.Where (x => x.gender == GENDER.MALE && x.generation > this.king.generation && x.isDirectDescendant == true).OrderBy(x => x.generation).ThenByDescending(x => x.age).ToList();
		List<Citizen> orderedFemaleRoyalties = this.successionLine.Where (x => x.gender == GENDER.FEMALE && x.generation > this.king.generation && x.isDirectDescendant == true).OrderBy(x => x.generation).ThenByDescending(x => x.age).ToList();
		List<Citizen> orderedBrotherRoyalties = this.successionLine.Where (x => x.gender == GENDER.MALE && x.father.id == this.king.father.id && x.id != this.king.id).OrderByDescending(x => x.age).ToList();
		List<Citizen> orderedSisterRoyalties = this.successionLine.Where (x => x.gender == GENDER.FEMALE && x.father.id == this.king.father.id && x.id != this.king.id).OrderByDescending(x => x.age).ToList();

		List<Citizen> orderedRoyalties = orderedMaleRoyalties.Concat (orderedFemaleRoyalties).Concat(orderedBrotherRoyalties).Concat(orderedSisterRoyalties).ToList();

		this.successionLine.Clear ();
		this.successionLine = orderedRoyalties;
	}

	internal void AssignNewKing(Citizen newKing, City city = null){
		if(this.king != null){
			if(this.king.city != null){
				this.king.city.hasKing = false;
			}
		}

		if(newKing == null){
//			KingdomManager.Instance.RemoveRelationshipToOtherKings (this.king);
//			this.king.city.CreateInitialRoyalFamily ();
//			this.king.CreateInitialRelationshipsToKings ();
//			KingdomManager.Instance.AddRelationshipToOtherKings (this.king);

			if(city == null){
//				Debug.Log("NO MORE SUCCESSOR! CREATING NEW KING IN KINGDOM!" + this.name);
				newKing = this.king.city.CreateNewKing ();
			}else{
//				Debug.Log("NO MORE SUCCESSOR! CREATING NEW KING ON CITY " + city.name + " IN KINGDOM!" + this.name);
				newKing = city.CreateNewKing ();
			}
			if(newKing == null){
				if(this.king != null){
					if(this.king.city != null){
						this.king.city.hasKing = true;
					}
				}
				return;
			}
		}
		newKing.city.hasKing = true;

		if(newKing.city.governor.id == newKing.id){
			newKing.city.AssignNewGovernor ();
		}
		if (newKing.isMarried) {
			if (newKing.spouse.city.kingdom.king.id == newKing.spouse.id) {
				AssimilateKingdom (newKing.spouse.city.kingdom);
				return;
			}
		}
		if(!newKing.isDirectDescendant){
			//				RoyaltyEventDelegate.TriggerChangeIsDirectDescendant (false);
			Utilities.ChangeDescendantsRecursively (newKing, true);
			Utilities.ChangeDescendantsRecursively (this.king, false);
		}
		if(newKing.assignedRole != null && newKing.role == ROLE.GENERAL){
			newKing.DetachGeneralFromCitizen ();
		}
//		newKing.role = ROLE.KING;
		newKing.AssignRole(ROLE.KING);
//		newKing.isKing = true;
		newKing.isGovernor = false;
//			KingdomManager.Instance.RemoveRelationshipToOtherKings (this.king);
		newKing.history.Add(new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, newKing.name + " became the new King/Queen of " + this.name + ".", HISTORY_IDENTIFIER.NONE));
//		this.king = newKing;
		this.king.CreateInitialRelationshipsToKings ();
		KingdomManager.Instance.AddRelationshipToOtherKings (this.king);
		this.successionLine.Clear();
		ChangeSuccessionLineRescursively (newKing);
		this.successionLine.AddRange (newKing.GetSiblings());
		UpdateKingSuccession ();
		this.RetrieveInternationWar();
//		UIManager.Instance.UpdateKingsGrid();
//		UIManager.Instance.UpdateKingdomSuccession ();

		for (int i = 0; i < this.cities.Count; i++) {
			this.cities[i].UpdateResourceProduction();
		}
	}
	internal void SuccessionWar(Citizen newKing, List<Citizen> claimants){
//		Debug.Log ("SUCCESSION WAR");

		if(newKing.city.governor.id == newKing.id){
			newKing.city.AssignNewGovernor ();
		}
		if(!newKing.isDirectDescendant){
			Utilities.ChangeDescendantsRecursively (newKing, true);
			Utilities.ChangeDescendantsRecursively (this.king, false);
		}
		if(newKing.assignedRole != null && newKing.role == ROLE.GENERAL){
			newKing.DetachGeneralFromCitizen ();
		}
//		newKing.role = ROLE.KING;
		newKing.AssignRole(ROLE.KING);
//		newKing.isKing = true;
		newKing.isGovernor = false;
//		KingdomManager.Instance.RemoveRelationshipToOtherKings (this.king);
		newKing.history.Add(new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, newKing.name + " became the new King/Queen of " + this.name + ".", HISTORY_IDENTIFIER.NONE));

//		this.king = newKing;
		this.king.CreateInitialRelationshipsToKings ();
		KingdomManager.Instance.AddRelationshipToOtherKings (this.king);
		this.successionLine.Clear();
		ChangeSuccessionLineRescursively (newKing);
		this.successionLine.AddRange (newKing.GetSiblings());
		UpdateKingSuccession ();
		this.RetrieveInternationWar();
//		UIManager.Instance.UpdateKingsGrid();
//		UIManager.Instance.UpdateKingdomSuccession ();

		for(int i = 0; i < claimants.Count; i++){
			newKing.AddSuccessionWar (claimants [i]);
			newKing.campaignManager.CreateCampaign ();

			if(claimants[i].isGovernor){
				claimants [i].supportedCitizen = claimants [i];
			}
			claimants[i].AddSuccessionWar (newKing);
			claimants[i].campaignManager.CreateCampaign ();
		}

	}
//	internal void DethroneKing(Citizen newKing){
////		RoyaltyEventDelegate.TriggerMassChangeLoyalty(newLord, this.assignedLord);
//
//		if(!newKing.isDirectDescendant){
////			RoyaltyEventDelegate.TriggerChangeIsDirectDescendant (false);
//			Utilities.ChangeDescendantsRecursively (newKing, true);
//			Utilities.ChangeDescendantsRecursively (this.king, false);
//		}
//		this.king = newKing;
//		this.king.CreateInitialRelationshipsToKings ();
//		KingdomManager.Instance.AddRelationshipToOtherKings (this.king);
//		this.successionLine.Clear();
//		ChangeSuccessionLineRescursively (newKing);
//		this.successionLine.AddRange (GetSiblings (newKing));
//		UpdateKingSuccession ();
//	}
	internal void ChangeSuccessionLineRescursively(Citizen royalty){
		if(this.king.id != royalty.id){
			if(!royalty.isDead){
				this.successionLine.Add (royalty);
			}
		}

		for(int i = 0; i < royalty.children.Count; i++){
			if(royalty.children[i] != null){
				this.ChangeSuccessionLineRescursively (royalty.children [i]);
			}
		}
	}

	internal RelationshipKingdom GetRelationshipWithOtherKingdom(Kingdom kingdomTarget){
		for (int i = 0; i < this.relationshipsWithOtherKingdoms.Count; i++) {
			if (this.relationshipsWithOtherKingdoms[i].targetKingdom.id == kingdomTarget.id) {
				return this.relationshipsWithOtherKingdoms[i];
			}
		}
		return null;
	}
	internal void AddPretender(Citizen citizen){
		this.pretenders.Add (citizen);
		this.pretenders = this.pretenders.Distinct ().ToList ();
	}
	internal List<Citizen> GetPretenderClaimants(Citizen successor){
		List<Citizen> pretenderClaimants = new List<Citizen> ();
		for(int i = 0; i < this.pretenders.Count; i++){
			if(this.pretenders[i].prestige > successor.prestige){
				pretenderClaimants.Add (this.pretenders [i]);
			}
		}
		return pretenderClaimants;
	}
	internal bool CheckForSpecificWar(Kingdom kingdom){
		for(int i = 0; i < this.relationshipsWithOtherKingdoms.Count; i++){
			if(this.relationshipsWithOtherKingdoms[i].targetKingdom.id == kingdom.id){
				if(this.relationshipsWithOtherKingdoms[i].isAtWar){
					return true;
				}
			}
		}
		return false;
	}
	internal void AssimilateKingdom(Kingdom newKingdom){
		for(int i = 0; i < this.cities.Count; i++){
			newKingdom.AddCityToKingdom (this.cities [i]);
		}
		KingdomManager.Instance.MakeKingdomDead(this);
	}

	internal void AddCityToKingdom(City city){
		this.cities.Add (city);
		city.kingdom = this;
		this.UpdateKingdomTypeData();
	}

	internal void ResetAdjacencyWithOtherKingdoms(){
		for (int i = 0; i < this.relationshipsWithOtherKingdoms.Count; i++) {
			this.relationshipsWithOtherKingdoms[i].ResetAdjacency();
		}
	}

	internal List<Citizen> GetAllCitizensOfType(ROLE role){
		List<Citizen> citizensOfType = new List<Citizen>();
		for (int i = 0; i < this.cities.Count; i++) {
			citizensOfType.AddRange (this.cities [i].GetCitizensWithRole(role));
		}
		return citizensOfType;
	}

	internal List<Kingdom> GetAdjacentKingdoms(){
		List<Kingdom> adjacentKingdoms = new List<Kingdom>();
		for (int i = 0; i < this.relationshipsWithOtherKingdoms.Count; i++) {
			if (relationshipsWithOtherKingdoms[i].isAdjacent) {
				adjacentKingdoms.Add(relationshipsWithOtherKingdoms[i].targetKingdom);
			}
		}
		return adjacentKingdoms;
	}

	internal bool IsKingdomAdjacentTo(Kingdom kingdomToCheck){
		if(this.id != kingdomToCheck.id){
			return this.GetRelationshipWithOtherKingdom(kingdomToCheck).isAdjacent;
		}else{
			return false;
		}
	}

	internal List<HexTile> GetAllHexTilesInKingdom(){
		List<HexTile> tilesOwnedByKingdom = new List<HexTile>();
		for (int i = 0; i < this.cities.Count; i++) {
			tilesOwnedByKingdom.AddRange (this.cities[i].ownedTiles);
		}
		return tilesOwnedByKingdom;
	}

	internal List<Kingdom> GetKingdomsByRelationship(RELATIONSHIP_STATUS relationshipStatus){
		List<Kingdom> kingdomsByRelationship = new List<Kingdom>();
		for (int i = 0; i < this.king.relationshipKings.Count; i++) {
			if (this.king.relationshipKings[i].lordRelationship == relationshipStatus) {
				kingdomsByRelationship.Add(this.king.relationshipKings [i].king.city.kingdom);
			}
		}
		return kingdomsByRelationship;
	}

	internal void AddInternationalWar(Kingdom kingdom){
//		Debug.Log ("INTERNATIONAL WAR");
		for(int i = 0; i < kingdom.cities.Count; i++){
			if(!this.king.campaignManager.SearchForInternationalWarCities(kingdom.cities[i])){
				this.king.campaignManager.intlWarCities.Add(new CityWar(kingdom.cities[i], false, WAR_TYPE.INTERNATIONAL));
			}
		}
		for(int i = 0; i < this.cities.Count; i++){
			if(!this.king.campaignManager.SearchForDefenseWarCities(this.cities[i], WAR_TYPE.INTERNATIONAL)){
				this.king.campaignManager.defenseWarCities.Add(new CityWar(this.cities[i], false, WAR_TYPE.INTERNATIONAL));
			}
//			if(this.cities[i].governor.supportedCitizen == null){
//				if(!this.king.campaignManager.SearchForDefenseWarCities(kingdom.cities[i])){
//					this.king.campaignManager.defenseWarCities.Add(new CityWar(kingdom.cities[i], false, WAR_TYPE.INTERNATIONAL));
//				}
//			}else{
//				if(!this.king.SearchForSuccessionWar(this.cities[i].governor.supportedCitizen)){
//					if(!this.king.campaignManager.SearchForDefenseWarCities(kingdom.cities[i])){
//						this.king.campaignManager.defenseWarCities.Add(new CityWar(kingdom.cities[i], false, WAR_TYPE.INTERNATIONAL));
//					}
//				}
//			}
		}
		this.king.campaignManager.CreateCampaign ();
	}

	internal void RemoveInternationalWar(Kingdom kingdom){
		this.king.campaignManager.intlWarCities.RemoveAll(x => x.city.kingdom.id == kingdom.id);
		for(int i = 0; i < this.king.campaignManager.activeCampaigns.Count; i++){
			if(this.king.campaignManager.activeCampaigns[i].warType == WAR_TYPE.INTERNATIONAL){
				if(this.king.campaignManager.activeCampaigns[i].targetCity.kingdom.id == kingdom.id){
					this.king.campaignManager.CampaignDone(this.king.campaignManager.activeCampaigns[i]);
				}
			}
		}
	}

	internal void PassOnInternationalWar(){
		this.holderIntlWarCities.Clear();
		this.holderIntlWarCities.AddRange(this.king.campaignManager.intlWarCities);
	}
	internal void RetrieveInternationWar(){
		this.king.campaignManager.intlWarCities.AddRange(this.holderIntlWarCities);
		this.holderIntlWarCities.Clear();
	}

	internal City SearchForCityById(int id){
		for(int i = 0; i < this.cities.Count; i++){
			if(this.cities[i].id == id){
				return this.cities[i];
			}
		}
		return null;
	}

	internal IEnumerator ConquerCity(City city){
		HexTile hex = city.hexTile;
//		city.kingdom.cities.Remove(city);
		city.KillCity();
		yield return null;
		City newCity = CityGenerator.Instance.CreateNewCity (hex, this);
		newCity.CreateInitialFamilies(false);
		KingdomManager.Instance.UpdateKingdomAdjacency();
		this.AddInternationalWarCity (newCity);
		if (UIManager.Instance.currentlyShowingKingdom.id == newCity.kingdom.id) {
			newCity.kingdom.HighlightAllOwnedTilesInKingdom();
		}
		KingdomManager.Instance.CheckWarTriggerMisc (newCity.kingdom, WAR_TRIGGER.TARGET_GAINED_A_CITY);
	}
	internal void AddInternationalWarCity(City newCity){
		for(int i = 0; i < this.relationshipsWithOtherKingdoms.Count; i++){
			if(this.relationshipsWithOtherKingdoms[i].isAtWar){
				if(!this.relationshipsWithOtherKingdoms[i].targetKingdom.king.campaignManager.SearchForInternationalWarCities(newCity)){
					this.relationshipsWithOtherKingdoms [i].targetKingdom.king.campaignManager.intlWarCities.Add (new CityWar (newCity, false, WAR_TYPE.INTERNATIONAL));
				}
			}
		}
		if(this.IsKingdomHasWar()){
			if(!this.king.campaignManager.SearchForDefenseWarCities(newCity, WAR_TYPE.INTERNATIONAL)){
				this.king.campaignManager.defenseWarCities.Add (new CityWar (newCity, false, WAR_TYPE.INTERNATIONAL));
			}
		}

	}
	internal bool IsKingdomHasWar(){
		for(int i = 0; i < this.relationshipsWithOtherKingdoms.Count; i++){
			if(this.relationshipsWithOtherKingdoms[i].isAtWar){
				return true;
			}
		}
		return false;
	}
	internal void RemoveFromSuccession(Citizen citizen){
		if(citizen != null){
			for(int i = 0; i < this.successionLine.Count; i++){
				if(this.successionLine[i].id == citizen.id){
					this.successionLine.RemoveAt (i);
//					UIManager.Instance.UpdateKingdomSuccession ();
					break;
				}
			}
		}
	}

	internal void AdjustExhaustionToAllRelationship(int amount){
		for (int i = 0; i < this.relationshipsWithOtherKingdoms.Count; i++) {
			this.relationshipsWithOtherKingdoms [i].AdjustExhaustion (amount);
		}
	}

	internal Citizen GetCitizenWithHighestPrestigeInKingdom(){
		Citizen citizenHighest = null;
		for(int i = 0; i < this.cities.Count; i++){
			Citizen citizen = this.cities [i].GetCitizenWithHighestPrestige ();
			if(citizen != null){
				if(citizenHighest == null){
					citizenHighest = citizen;
				}else{
					if(citizen.prestige > citizenHighest.prestige){
						citizenHighest = citizen;
					}
				}

			}
		}
		return citizenHighest;
	}

	internal void HighlightAllOwnedTilesInKingdom(){
		for (int i = 0; i < this.cities.Count; i++) {
			if (UIManager.Instance.currentlyShowingCity != null && UIManager.Instance.currentlyShowingCity.id == this.cities [i].id) {
				continue;
			}
			this.cities [i].HighlightAllOwnedTiles (127.5f / 255f);
		}
	}

	internal void UnHighlightAllOwnedTilesInKingdom(){
		for (int i = 0; i < this.cities.Count; i++) {
			if (UIManager.Instance.currentlyShowingCity != null && UIManager.Instance.currentlyShowingCity.id == this.cities [i].id) {
				continue;
			}
			this.cities [i].UnHighlightAllOwnedTiles ();
		}
	}

	internal void UpdateKingdomAdjacency(){
		this.adjacentKingdoms.Clear();
		this.adjacentCitiesFromOtherKingdoms.Clear ();
		for (int i = 0; i < this.cities.Count; i++) {
			List<City> adjacentCitiesOfCurrentCity = this.cities[i].adjacentCities;
			for (int j = 0; j < adjacentCitiesOfCurrentCity.Count; j++) {
				City currAdjacentCity = adjacentCitiesOfCurrentCity [j];
				if (adjacentCitiesOfCurrentCity[j].kingdom.id != this.id) {
					if (!this.adjacentKingdoms.Contains (currAdjacentCity.kingdom)) {
						this.adjacentKingdoms.Add (currAdjacentCity.kingdom);
					}
					if (!currAdjacentCity.kingdom.adjacentKingdoms.Contains(this)) {
						currAdjacentCity.kingdom.adjacentKingdoms.Add(this);
					}
					if (!this.adjacentCitiesFromOtherKingdoms.Contains(currAdjacentCity)) {
						this.adjacentCitiesFromOtherKingdoms.Add(currAdjacentCity);
					}
					if (!currAdjacentCity.kingdom.adjacentCitiesFromOtherKingdoms.Contains(this.cities[i])) {
						currAdjacentCity.kingdom.adjacentCitiesFromOtherKingdoms.Add(this.cities[i]);
					}

				}
			}
		}
	}

	protected List<HexTile> GetAllKingdomBorderTiles(){
		List<HexTile> allBorderTiles = new List<HexTile>();
		for (int i = 0; i < this.cities.Count; i++) {
			allBorderTiles.AddRange (this.cities [i].borderTiles);
		}
		return allBorderTiles;
	}

	internal MILITARY_STRENGTH GetMilitaryStrengthAgainst(Kingdom kingdom){
		int sourceMilStrength = this.GetAllArmyHp ();
		int targetMilStrength = kingdom.GetAllArmyHp ();

		int fiftyPercent = (int)(targetMilStrength * 0.50f);
		int twentyPercent = (int)(targetMilStrength * 0.20f);
//		Debug.Log ("TARGET MILITARY STRENGTH: " + targetMilStrength);
//		Debug.Log ("SOURCE MILITARY STRENGTH: " + sourceMilStrength);
		if(sourceMilStrength == 0 && targetMilStrength == 0){
			return MILITARY_STRENGTH.COMPARABLE;
		}else{
			if(sourceMilStrength > (targetMilStrength + fiftyPercent)){
				return MILITARY_STRENGTH.MUCH_STRONGER;
			}else if(sourceMilStrength > (targetMilStrength + twentyPercent)){
				return MILITARY_STRENGTH.SLIGHTLY_STRONGER;
			}else if(sourceMilStrength > (targetMilStrength - twentyPercent)){
				return MILITARY_STRENGTH.COMPARABLE;
			}else if(sourceMilStrength > (targetMilStrength - fiftyPercent)){
				return MILITARY_STRENGTH.SLIGHTLY_WEAKER;
			}else{
				return MILITARY_STRENGTH.MUCH_WEAKER;
			}
		}
	}

	internal int GetAllArmyHp(){
		int total = 0;
		List<Citizen> allGenerals = this.GetAllCitizensOfType (ROLE.GENERAL);
		for(int i = 0; i < allGenerals.Count; i++){
			if(allGenerals[i] is General){
				total += ((General)allGenerals [i].assignedRole).GetArmyHP ();
			}
		}
		return total;
	}
	internal int GetWarCount(){
		int total = 0;
		for (int i = 0; i < relationshipsWithOtherKingdoms.Count; i++) {
			if(relationshipsWithOtherKingdoms[i].isAtWar){
				total += 1;
			}
		}
		return total;
	}
	//Destructor for unsubscribing listeners
	~Kingdom(){
		EventManager.Instance.onCreateNewKingdomEvent.RemoveListener(NewKingdomCreated);
	}
}
