using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Kingdom{
	public int id;
	public string name;
	public RACE race;
	public List<City> cities;
	public Citizen king;
	public List<Citizen> successionLine;
	public List<Citizen> pretenders;
//	public List<Citizen> royaltyList;
	public List<CityWar> holderIntlWarCities;

	public BASE_RESOURCE_TYPE basicResource;
	public BASE_RESOURCE_TYPE rareResource;

	public List<Relationship<Kingdom>> relationshipsWithOtherKingdoms;

	public Color kingdomColor;
	public List<History> kingdomHistory;

	public Kingdom(RACE race, List<HexTile> cities){
		this.id = Utilities.SetID(this);
		this.race = race;
		this.name = RandomNameGenerator.Instance.GenerateKingdomName(this.race);
		this.king = null;
		this.successionLine = new List<Citizen>();
		this.pretenders = new List<Citizen> ();
		this.cities = new List<City>();
		this.holderIntlWarCities = new List<CityWar> ();
		this.kingdomHistory = new List<History>();
		this.kingdomColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);

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
		this.relationshipsWithOtherKingdoms = new List<Relationship<Kingdom>>();
		this.CreateInitialRelationships();
		EventManager.Instance.onCreateNewKingdomEvent.AddListener(NewKingdomCreated);
		EventManager.Instance.onWeekEnd.AddListener(AttemptToExpand);
		this.kingdomHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.week, GameManager.Instance.year, "This kingdom was born.", HISTORY_IDENTIFIER.NONE));
	}

	protected void CreateInitialRelationships(){
		for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
			if (KingdomManager.Instance.allKingdoms[i].id != this.id) {
				this.relationshipsWithOtherKingdoms.Add (new Relationship<Kingdom>(KingdomManager.Instance.allKingdoms [i]));
			}
		}
	}

	protected void NewKingdomCreated(Kingdom createdKingdom){
		//Add relationship to newly created kingdom
		if (createdKingdom.id == this.id) {
			return;
		}
		for (int i = 0; i < this.relationshipsWithOtherKingdoms.Count; i++) {
			if (this.relationshipsWithOtherKingdoms [i].objectInRelationship.id == createdKingdom.id) {
				//this kingdom already has a relationship with created kingdom!
				return;
			}
		}
		this.relationshipsWithOtherKingdoms.Add(new Relationship<Kingdom>(createdKingdom));
	}

	protected void AttemptToExpand(){
		if (EventManager.Instance.GetEventsOfTypePerKingdom (this, EVENT_TYPES.EXPANSION).Count > 0) {
			return;
		}

		List<City> citiesThatCanExpand = new List<City>();
		List<Citizen> allUnassignedAdultCitizens = new List<Citizen>();
		List<Resource> expansionCost = new List<Resource> () {
			new Resource (BASE_RESOURCE_TYPE.GOLD, 1000),
			new Resource (this.basicResource, 200)
		};
//		List<Resource> expansionCost = new List<Resource> () {
//			new Resource (BASE_RESOURCE_TYPE.GOLD, 50),
//			new Resource (this.basicResource, 20)
//		};

		for (int i = 0; i < this.cities.Count; i++) {
			if (this.cities[i].HasEnoughResourcesForAction(expansionCost) && this.cities[i].adjacentHabitableTiles.Count > 0) {
				citiesThatCanExpand.Add(this.cities[i]);
			}
		}

//		float expansionChance = 0f;
//		for (int i = 0; i < citiesThatCanExpand.Count; i++) {
//			List<Citizen> untrainedCitizens = citiesThatCanExpand[i].GetCitizensWithRole(ROLE.UNTRAINED).Where(x => (x.spouse != null && x.spouse.role != ROLE.GOVERNOR) && x.age >= 16).ToList();
//			allUnassignedAdultCitizens.AddRange(untrainedCitizens);
//			expansionChance += 0.5f * untrainedCitizens.Count;
//		}

//		float chance = Random.Range(1f, expansionChance);
//		if (chance < expansionChance) {
//			Citizen highestPrestigeCitizen = allUnassignedAdultCitizens.OrderByDescending(x => x.prestige).First();
//			Expansion newExpansionEvent = new Expansion (GameManager.Instance.week, GameManager.Instance.month, GameManager.Instance.year, highestPrestigeCitizen);
//		}

		if (citiesThatCanExpand.Count > 0) {
			float expansionChance = 3f;
			float chance = Random.Range (1f, 100f);
			if (chance < expansionChance) {
				Citizen governorToLeadExpansion = citiesThatCanExpand[0].governor;
				citiesThatCanExpand[0].AssignNewGovernor();
				citiesThatCanExpand[0].AdjustResources(expansionCost);
				Expansion newExpansionEvent = new Expansion (GameManager.Instance.week, GameManager.Instance.month, GameManager.Instance.year, governorToLeadExpansion);
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
		tile.city = new City (tile, this);
		tile.GetComponent<SpriteRenderer> ().color = this.kingdomColor;
		this.cities.Add (tile.city);
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

	internal void AssignNewKing(Citizen newKing){
		if(newKing == null){
//			KingdomManager.Instance.RemoveRelationshipToOtherKings (this.king);
			this.king.city.CreateInitialRoyalFamily ();
			this.king.CreateInitialRelationshipsToKings ();
			KingdomManager.Instance.AddRelationshipToOtherKings (this.king);

		}else{
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
			newKing.role = ROLE.UNTRAINED;
			newKing.assignedRole = null;
			newKing.isKing = true;
			newKing.isGovernor = false;
//			KingdomManager.Instance.RemoveRelationshipToOtherKings (this.king);
			newKing.history.Add(new History (GameManager.Instance.month, GameManager.Instance.week, GameManager.Instance.year, newKing.name + " became the new King/Queen of " + this.name + ".", HISTORY_IDENTIFIER.NONE));
			this.king = newKing;
			this.king.CreateInitialRelationshipsToKings ();
			KingdomManager.Instance.AddRelationshipToOtherKings (this.king);
			this.successionLine.Clear();
			ChangeSuccessionLineRescursively (newKing);
			this.successionLine.AddRange (GetSiblings (newKing));
			UpdateKingSuccession ();
			this.RetrieveInternationWar();
		}
	}
	internal void SuccessionWar(Citizen newKing, List<Citizen> claimants){
		Debug.Log ("SUCCESSION WAR");

		if(newKing.city.governor.id == newKing.id){
			newKing.city.AssignNewGovernor ();
		}
		if(!newKing.isDirectDescendant){
			Utilities.ChangeDescendantsRecursively (newKing, true);
			Utilities.ChangeDescendantsRecursively (this.king, false);
		}
		newKing.role = ROLE.UNTRAINED;
		newKing.assignedRole = null;
		newKing.isKing = true;
		newKing.isGovernor = false;
//		KingdomManager.Instance.RemoveRelationshipToOtherKings (this.king);
		newKing.history.Add(new History (GameManager.Instance.month, GameManager.Instance.week, GameManager.Instance.year, newKing.name + " became the new King/Queen of " + this.name + ".", HISTORY_IDENTIFIER.NONE));

		this.king = newKing;
		this.king.CreateInitialRelationshipsToKings ();
		KingdomManager.Instance.AddRelationshipToOtherKings (this.king);
		this.successionLine.Clear();
		ChangeSuccessionLineRescursively (newKing);
		this.successionLine.AddRange (GetSiblings (newKing));
		UpdateKingSuccession ();
		this.RetrieveInternationWar();

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
	internal void DethroneKing(Citizen newKing){
//		RoyaltyEventDelegate.TriggerMassChangeLoyalty(newLord, this.assignedLord);

		if(!newKing.isDirectDescendant){
//			RoyaltyEventDelegate.TriggerChangeIsDirectDescendant (false);
			Utilities.ChangeDescendantsRecursively (newKing, true);
			Utilities.ChangeDescendantsRecursively (this.king, false);
		}
		this.king = newKing;
		this.king.CreateInitialRelationshipsToKings ();
		KingdomManager.Instance.AddRelationshipToOtherKings (this.king);
		this.successionLine.Clear();
		ChangeSuccessionLineRescursively (newKing);
		this.successionLine.AddRange (GetSiblings (newKing));
		UpdateKingSuccession ();
	}
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

	internal List<Citizen> GetSiblings(Citizen royalty){
		List<Citizen> siblings = new List<Citizen> ();
		for(int i = 0; i < royalty.mother.children.Count; i++){
			if(royalty.mother.children[i].id != royalty.id){
				if(!royalty.mother.children[i].isDead){
					siblings.Add (royalty.mother.children [i]);
				}
			}
		}

		return siblings;
	}

	internal Relationship<Kingdom> GetRelationshipWithOtherKingdom(Kingdom kingdomTarget){
		for (int i = 0; i < this.relationshipsWithOtherKingdoms.Count; i++) {
			if (this.relationshipsWithOtherKingdoms[i].objectInRelationship.id == kingdomTarget.id) {
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
			if(this.relationshipsWithOtherKingdoms[i].objectInRelationship.id == kingdom.id){
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
		city.hexTile.GetComponent<HexTile>().SetTileColor (this.kingdomColor);
	}

	internal void ResetAdjacencyWithOtherKingdoms(){
		for (int i = 0; i < this.relationshipsWithOtherKingdoms.Count; i++) {
			this.relationshipsWithOtherKingdoms[i].isAdjacent = false;
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
				adjacentKingdoms.Add(relationshipsWithOtherKingdoms[i].objectInRelationship);
			}
		}
		return adjacentKingdoms;
	}

	internal bool IsKingdomAdjacentTo(Kingdom kingdomToCheck){
		return this.GetRelationshipWithOtherKingdom(kingdomToCheck).isAdjacent;
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
		Debug.Log ("INTERNATIONAL WAR");
		for(int i = 0; i < kingdom.cities.Count; i++){
			if(!this.king.campaignManager.SearchForInternationalWarCities(kingdom.cities[i])){
				this.king.campaignManager.intlWarCities.Add(new CityWar(kingdom.cities[i], false, WAR_TYPE.INTERNATIONAL));
			}
		}
		for(int i = 0; i < this.cities.Count; i++){
			if(!this.king.campaignManager.SearchForDefenseWarCities(kingdom.cities[i])){
				this.king.campaignManager.defenseWarCities.Add(new CityWar(kingdom.cities[i], false, WAR_TYPE.INTERNATIONAL));
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

	internal void ConquerCity(City city){
		HexTile hex = city.hexTile;
		city.kingdom.cities.Remove(city);
		City newCity = new City(hex, this);
		newCity.CreateInitialFamilies(false);
		this.AddCityToKingdom(newCity);
	}
	//Destructor for unsubscribing listeners
	~Kingdom(){
		EventManager.Instance.onCreateNewKingdomEvent.RemoveListener(NewKingdomCreated);
	}
}
