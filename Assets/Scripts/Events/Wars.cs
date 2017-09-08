using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Wars : GameEvent {
	private enum MOBILIZATION_TYPE{
		EQUAL, //all cities will contribute power equally
		FOCUSED, //will only get power from 1 city, will only get from another city if the power is not yet enough
	}
	private enum CURRENT_TURN{
		KINGDOM1,
		KINGDOM2,
	}

	private Kingdom _kingdom1;
	private Kingdom _kingdom2;

	private KingdomRelationship _kingdom1Rel;
	private KingdomRelationship _kingdom2Rel;

	private List<City> _safeCitiesKingdom1;
	private List<City> _safeCitiesKingdom2;
	private List<City> _safeCitiesKingdom;

	private CityWarPair _warPair;

	private bool _isAtWar;
	private bool kingdom1Attacked;
	private bool isInitialAttack;
	private CURRENT_TURN currentTurn;

	private int _mobilizedReinforcements;
	private City _attacker = null;
	private City _defender = null;

	#region getters/setters
	public Kingdom kingdom1 {
		get { return _kingdom1; }
	}

	public Kingdom kingdom2{
		get { return _kingdom2; }
	}

	public KingdomRelationship kingdom1Rel {
		get { return _kingdom1Rel; }
	}

	public KingdomRelationship kingdom2Rel {
		get { return _kingdom2Rel; }
	}
	public CityWarPair warPair {
		get { return _warPair; }
	}
	public bool isAtWar {
		get { return _isAtWar; }
	}
	#endregion
	public Wars(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom _kingdom1, Kingdom _kingdom2) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.KINGDOM_WAR;
		this.name = "War";
		this.description = "War between " + _kingdom1.name + " and " + _kingdom2.name + ".";
		this._kingdom1 = _kingdom1;
		this._kingdom2 = _kingdom2;
		this._kingdom1Rel = _kingdom1.GetRelationshipWithKingdom(_kingdom2);
		this._kingdom2Rel = _kingdom2.GetRelationshipWithKingdom(_kingdom1);
		this._kingdom1Rel.AssignWarEvent(this);
		this._kingdom2Rel.AssignWarEvent(this);
		this._safeCitiesKingdom1 = new List<City>();
		this._safeCitiesKingdom2 = new List<City>();
		this._safeCitiesKingdom = new List<City>();
		this._warPair.DefaultValues();
		this.kingdom1Attacked = false;
		this.isInitialAttack = false;
		this.currentTurn = CURRENT_TURN.KINGDOM1;
		this._attacker = null;
		this._defender = null;
		Messenger.AddListener<HexTile>("OnUpdatePath", UpdatePath);
		InitializeMobilization ();
	}
	private void MilitaryAlliancesMustJoin(Kingdom kingdom, Kingdom targetKingdom){
		for (int i = 0; i < kingdom.militaryAlliances.Count; i++) {
			Kingdom militaryAllianceKingdom = kingdom.militaryAlliances [i];
			KingdomRelationship allianceTargetRelationship = militaryAllianceKingdom.GetRelationshipWithKingdom (targetKingdom);
			if(!allianceTargetRelationship.isAtWar){
				bool hasIdleCity = false;
				for (int j = 0; j < militaryAllianceKingdom.cities.Count; j++) {
					City militaryAllianceCity = militaryAllianceKingdom.cities [i];
					if (militaryAllianceCity.rebellion == null && !militaryAllianceCity.isUnderAttack) {
						hasIdleCity = true;
						break;
					}
				}
				if(hasIdleCity){
					if(allianceTargetRelationship.isMilitaryAlliance){
						DissolveMilitaryAlliance (kingdom, militaryAllianceKingdom);
					}
				}else{
					DissolveMilitaryAlliance (kingdom, militaryAllianceKingdom);
				}
			}
		}
	}
	private void MutualDefenseTreatyMustJoin(Kingdom kingdom, Kingdom targetKingdom){
		for (int i = 0; i < kingdom.mutualDefenseTreaties.Count; i++) {
			Kingdom mutualDefenseTreatyKingdom = kingdom.mutualDefenseTreaties [i];
			KingdomRelationship allianceTargetRelationship = mutualDefenseTreatyKingdom.GetRelationshipWithKingdom (targetKingdom);
			if(!allianceTargetRelationship.isAtWar){
				bool hasIdleCity = false;
				for (int j = 0; j < mutualDefenseTreatyKingdom.cities.Count; j++) {
					City militaryAllianceCity = mutualDefenseTreatyKingdom.cities [i];
					if (militaryAllianceCity.rebellion == null && !militaryAllianceCity.isUnderAttack) {
						hasIdleCity = true;
						break;
					}
				}
				if(hasIdleCity){
					if(allianceTargetRelationship.isMutualDefenseTreaty){
						DissolveMutualDefenseTreaty (kingdom, mutualDefenseTreatyKingdom);
					}
				}else{
					DissolveMutualDefenseTreaty (kingdom, mutualDefenseTreatyKingdom);
				}
			}
		}
	}
	private void DissolveMilitaryAlliance(Kingdom sourceKingdom, Kingdom targetKingdom){
		//Dissolve
		KingdomRelationship relationship = sourceKingdom.GetRelationshipWithKingdom (targetKingdom);
		relationship.ChangeMilitaryAlliance (false);
	}
	private void DissolveMutualDefenseTreaty(Kingdom sourceKingdom, Kingdom targetKingdom){
		//Dissolve
		KingdomRelationship relationship = sourceKingdom.GetRelationshipWithKingdom (targetKingdom);
		relationship.ChangeMutualDefenseTreaty (false);
	}
	private void UpdatePath(HexTile hexTile){
		if(this._warPair.path != null && this._warPair.path.Count > 0){
			for (int i = 0; i < this._warPair.path.Count; i++) {
				if(this._warPair.path[i] == hexTile){
					this._warPair.UpdateSpawnRate();
					break;
				}
			}
		}
	}
	private void UpdateWarPair(){
//		this._warPair.DefaultValues ();
		CreateCityWarPair ();
	}
	private void CreateCityWarPair(){
		if(!this._warPair.kingdom1City.isDead && !this._warPair.kingdom2City.isDead){
			return;
		}
		List<HexTile> path = null;
		City kingdom1CityToBeAttacked = null;
		City kingdom2CityToBeAttacked = null;

		if((this._warPair.kingdom1City == null || this._warPair.kingdom2City == null) || (this._warPair.kingdom1City.isDead && this._warPair.kingdom2City.isDead)){
			kingdom1CityToBeAttacked = GetNearestCityFrom(this.kingdom2.capitalCity.hexTile, this.kingdom1.nonRebellingCities);
			if (kingdom1CityToBeAttacked != null) {
				kingdom2CityToBeAttacked = GetNearestCityFrom (kingdom1CityToBeAttacked.hexTile, this.kingdom2.nonRebellingCities);
				path = PathGenerator.Instance.GetPath (kingdom1CityToBeAttacked.hexTile, kingdom2CityToBeAttacked.hexTile, PATHFINDING_MODE.COMBAT);
			}
		}else{
			if(this._warPair.isDone){
				if(this._warPair.kingdom1City.isDead){
					kingdom1CityToBeAttacked = GetNearestCityFrom(this.kingdom2.cities[this.kingdom2.cities.Count - 1].hexTile, this.kingdom1.nonRebellingCities);
					if (kingdom1CityToBeAttacked != null) {
						kingdom2CityToBeAttacked = this.kingdom2.cities[this.kingdom2.cities.Count - 1];
						path = PathGenerator.Instance.GetPath (kingdom1CityToBeAttacked.hexTile, kingdom2CityToBeAttacked.hexTile, PATHFINDING_MODE.COMBAT);
					}
				}else{
					for (int i = 0; i < this.kingdom1.capitalCity.habitableTileDistance.Count; i++) {
						HexTile hexTile = this.kingdom1.capitalCity.habitableTileDistance [i].hexTile;
						if(hexTile.city != null && hexTile.city.id != 0 
							&& !hexTile.city.isDead && !hexTile.city.isUnderAttack && hexTile.city.rebellion == null){

							if(hexTile.city.kingdom.id == this.kingdom2.id){
								kingdom2CityToBeAttacked = hexTile.city;
								break;
							}
						}
					}
					kingdom2CityToBeAttacked = GetNearestCityFrom(this.kingdom1.cities[this.kingdom1.cities.Count - 1].hexTile, this.kingdom2.nonRebellingCities);
					if (kingdom2CityToBeAttacked != null) {
						kingdom1CityToBeAttacked = this.kingdom1.cities[this.kingdom1.cities.Count - 1];
						path = PathGenerator.Instance.GetPath (kingdom1CityToBeAttacked.hexTile, kingdom2CityToBeAttacked.hexTile, PATHFINDING_MODE.COMBAT);
					}
				}
			}else{
				if(this._warPair.kingdom1City.isDead){
					kingdom1CityToBeAttacked = GetNearestCityFrom(this._warPair.kingdom2City.hexTile, this.kingdom1.nonRebellingCities);
					if (kingdom1CityToBeAttacked != null) {
						kingdom2CityToBeAttacked = this._warPair.kingdom2City;
						path = PathGenerator.Instance.GetPath (kingdom1CityToBeAttacked.hexTile, kingdom2CityToBeAttacked.hexTile, PATHFINDING_MODE.COMBAT);
					}
				}else{
					kingdom2CityToBeAttacked = GetNearestCityFrom(this._warPair.kingdom1City.hexTile, this.kingdom2.nonRebellingCities);
					if (kingdom2CityToBeAttacked != null) {
						kingdom1CityToBeAttacked = this._warPair.kingdom1City;
						path = PathGenerator.Instance.GetPath (kingdom1CityToBeAttacked.hexTile, kingdom2CityToBeAttacked.hexTile, PATHFINDING_MODE.COMBAT);
					}
				}
			}
		}



		if(kingdom1CityToBeAttacked != null && kingdom2CityToBeAttacked != null && path != null){
			kingdom1CityToBeAttacked.isUnderAttack = true;
			kingdom2CityToBeAttacked.isUnderAttack = true;
			this._warPair = new CityWarPair (kingdom1CityToBeAttacked, kingdom2CityToBeAttacked, path);
		}
	}
	private void ResetToStep1(){
		this._mobilizedReinforcements = 0;
		this._attacker = null;
		this._defender = null;
	}
	internal void Mobilize(){
		if(this._warPair.kingdom1City == null || this._warPair.kingdom2City == null){
			if(this._attacker != null){
				this._attacker.kingdom.CheckMobilizationQueue ();
			}
			return;
		}
		if(this._warPair.kingdom1City.isDead || this._warPair.kingdom2City.isDead){
			//Peace?
			if(this._attacker != null){
				this._attacker.kingdom.CheckMobilizationQueue ();
			}
			return;
		}
		ResetToStep1 ();
		if (this.currentTurn == CURRENT_TURN.KINGDOM1) {
			this._attacker = this._warPair.kingdom1City;
			this._defender = this._warPair.kingdom2City;
		}else{
			this._attacker = this._warPair.kingdom2City;
			this._defender = this._warPair.kingdom1City;
		}
		if(this._attacker.kingdom.isMobilizing){
			this._attacker.kingdom.AddToMobilizationQueue (this);
			return;
		}
		this._attacker.kingdom.MobilizingState (true);
		if(this._attacker.kingdom.cities.Count > 1){
			float buffEnemyPower = UnityEngine.Random.Range (1.1f, 1.5f);
			int enemyPower = (int)(GetEnemyPowerAndDefense (this._defender) * buffEnemyPower);
			if(this._attacker.power < enemyPower){
				int diffPower = enemyPower - this._attacker.power;
				MOBILIZATION_TYPE mobType = GetMobilizationType ();
				bool hasReinforced = false;
				if (mobType == MOBILIZATION_TYPE.EQUAL) {
					int powerContribution = Mathf.CeilToInt(diffPower / (this._attacker.kingdom.cities.Count - 1));
					int powerShortage = 0;
					int totalNeededContribution = 0;
					bool isComplete = false;
					while(!isComplete){
						for (int i = 0; i < this._attacker.kingdom.cities.Count; i++) {
							City reinforcerCity = this._attacker.kingdom.cities [i];
							if (this._attacker.id != reinforcerCity.id && reinforcerCity.power > 0 && !reinforcerCity.isUnderAttack) {
								hasReinforced = true;
								totalNeededContribution = powerContribution + powerShortage;
								if(reinforcerCity.power >= totalNeededContribution){
									powerShortage = 0;
									reinforcerCity.ReinforceCity (this._attacker, totalNeededContribution, this);
								}else{
									powerShortage = totalNeededContribution - reinforcerCity.power;
									reinforcerCity.ReinforceCity (this._attacker, reinforcerCity.power, this);
								}
							}
						}
						if(powerShortage <= 0){
							isComplete = true;
						}
					}
				}else{
					int neededPower = diffPower;
					for (int i = 0; i < this._attacker.kingdom.cities.Count; i++) {
						City reinforcerCity = this._attacker.kingdom.cities [i];
						if (this._attacker.id != reinforcerCity.id && reinforcerCity.power > 0 && !reinforcerCity.isUnderAttack) {
							hasReinforced = true;
							if(reinforcerCity.power >= neededPower){
								reinforcerCity.ReinforceCity (this._attacker, neededPower, this);
								break;
							}else{
								neededPower -= reinforcerCity.power;
								reinforcerCity.ReinforceCity (this._attacker, reinforcerCity.power, this);
							}
						}
					}
				}
				if(!hasReinforced){
					DeclareWar ();
				}
			}else{
				DeclareWar ();
			}
		}else if (this._attacker.kingdom.cities.Count == 1){
			DeclareWar ();
		}
	}
	private void DeclareWar(){
		if(!this._isAtWar){
			this._isAtWar = true;
			KingdomManager.Instance.DeclareWarBetweenKingdoms (this);
			this.isInitialAttack = true;
			MilitaryAlliancesMustJoin (this._kingdom1, this._kingdom2);
			MilitaryAlliancesMustJoin (this._kingdom2, this._kingdom1);
			MutualDefenseTreatyMustJoin (this._kingdom2, this._kingdom1);
		}
		AttackCity ();
	}
	private void AttackCity(){
		if(!this._attacker.isDead && !this._defender.isDead){
			if(this._attacker.power > 0){
				this._attacker.AttackCity (this._defender, this.warPair.path, this);
				ReinforcementKingdom (this._defender);
			}else{
				ChangeTurn ();	
			}
		}else{
			InitializeMobilization ();
		}
	}
	internal void CheckWarPair(){
		UpdateWarPair ();

	}
	internal void ChangeTurn(){
		if(this.currentTurn == CURRENT_TURN.KINGDOM1){
			this.currentTurn = CURRENT_TURN.KINGDOM2;
		}else{
			this.currentTurn = CURRENT_TURN.KINGDOM1;
		}
		if (this.warPair.kingdom1City.isDead || this.warPair.kingdom2City.isDead) {
			CheckWarPair ();
		}
		Mobilize ();
	}
	internal void InitializeMobilization(){
		CheckWarPair ();
		Mobilize ();
	}
	private int GetEnemyPowerAndDefense(City city){
		return city.power + city.defense;
	}

	private MOBILIZATION_TYPE GetMobilizationType(){
		return MOBILIZATION_TYPE.FOCUSED;

		int chance = UnityEngine.Random.Range (0, 2);
		if (chance == 0) {
			return MOBILIZATION_TYPE.EQUAL;
		}else{
			return MOBILIZATION_TYPE.FOCUSED;
		}
	}

	internal void AdjustMobilizedReinforcementsCount(int amount){
		this._mobilizedReinforcements += amount;
		if(this._mobilizedReinforcements <= 0){
			this._mobilizedReinforcements = 0;
			//Step 2: Declare War
			DeclareWar ();
		}
	}

	internal void ChangeDoneStateWarPair(bool state){
		this._warPair.isDone = state;
	}
	private void ReinforcementKingdom(City city){
		//		List<City> safeCitiesKingdom1 = this.kingdom1.cities.Where (x => !x.isUnderAttack && !x.hasReinforced && x.hp >= 100).ToList ();
		_safeCitiesKingdom.Clear();
		for (int i = 0; i < city.kingdom.cities.Count; i++) {
			if (!city.kingdom.cities[i].isUnderAttack && !city.kingdom.cities[i].hasReinforced && city.kingdom.cities[i].power > 0) {
				_safeCitiesKingdom.Add(city.kingdom.cities[i]);
			}
		}
		int chance = 0;
		int value = 0;
		int maxChanceKingdom1 = 100 + ((_safeCitiesKingdom.Count - 1) * 10);

		if(_safeCitiesKingdom != null){
			for(int i = 0; i < _safeCitiesKingdom.Count; i++){
				chance = UnityEngine.Random.Range (0, maxChanceKingdom1);
				value = 1 * _safeCitiesKingdom [i].ownedTiles.Count;
				if(chance < value){
					_safeCitiesKingdom [i].hasReinforced = true;
					_safeCitiesKingdom [i].ReinforceCity (city);
				}
			}
		}
	}
	private void ReinforcementKingdom1(){
		//		List<City> safeCitiesKingdom1 = this.kingdom1.cities.Where (x => !x.isUnderAttack && !x.hasReinforced && x.hp >= 100).ToList ();
		_safeCitiesKingdom1.Clear();
		for (int i = 0; i < this.kingdom1.cities.Count; i++) {
			if (!this.kingdom1.cities[i].isUnderAttack && !this.kingdom1.cities[i].hasReinforced && this.kingdom1.cities[i].power > 0) {
				_safeCitiesKingdom1.Add(this.kingdom1.cities[i]);
			}
		}
		int chance = 0;
		int value = 0;
		int maxChanceKingdom1 = 100 + ((_safeCitiesKingdom1.Count - 1) * 10);

		if(_safeCitiesKingdom1 != null){
			for(int i = 0; i < _safeCitiesKingdom1.Count; i++){
				chance = UnityEngine.Random.Range (0, maxChanceKingdom1);
				value = 1 * _safeCitiesKingdom1 [i].ownedTiles.Count;
				if(chance < value){
					_safeCitiesKingdom1 [i].hasReinforced = true;
					_safeCitiesKingdom1 [i].ReinforceCity (this.warPair.kingdom1City);
				}
			}
		}
	}
	private void ReinforcementKingdom2(){
		//		List<City> safeCitiesKingdom2 = this.kingdom2.cities.Where (x => !x.isUnderAttack && !x.hasReinforced && x.hp >= 100).ToList ();
		_safeCitiesKingdom2.Clear();
		for (int i = 0; i < this.kingdom2.cities.Count; i++) {
			if (!this.kingdom2.cities[i].isUnderAttack && !this.kingdom2.cities[i].hasReinforced && this.kingdom2.cities[i].power > 0) {
				_safeCitiesKingdom2.Add(this.kingdom2.cities[i]);
			}
		}
		int chance = 0;
		int value = 0;
		int maxChanceKingdom2 = 100 + ((_safeCitiesKingdom2.Count - 1) * 10);

		if(_safeCitiesKingdom2 != null){
			for(int i = 0; i < _safeCitiesKingdom2.Count; i++){
				chance = UnityEngine.Random.Range (0, maxChanceKingdom2);
				value = 1 * _safeCitiesKingdom2 [i].ownedTiles.Count;
				if(chance < value){
					_safeCitiesKingdom2 [i].hasReinforced = true;
					_safeCitiesKingdom2 [i].ReinforceCity (this.warPair.kingdom2City);
				}
			}
		}

	}
	private City GetNearestCityFrom(HexTile hexTile, List<City> citiesToChooseFrom) {
		if(hexTile == null){
			return null;
		}
		City nearestCity = null;
		float nearestDistance = 0f;
		City nearestCityUnderAttack = null;
		float nearestDistanceUnderAttack = 0f;
		for (int i = 0; i < citiesToChooseFrom.Count; i++) {
			City currCity = citiesToChooseFrom[i];
			if(!currCity.isDead && currCity.rebellion == null){
				float distance = Vector3.Distance (hexTile.transform.position, currCity.hexTile.transform.position);
				if(!currCity.isUnderAttack){
					if(nearestCity == null) {
						nearestCity = currCity;
						nearestDistance = distance;
					} else {
						if(distance < nearestDistance) {
							nearestCity = currCity;
							nearestDistance = distance;
						}
					}
				}else{
					if(nearestCityUnderAttack == null) {
						nearestCityUnderAttack = currCity;
						nearestDistanceUnderAttack = distance;
					} else {
						if(distance < nearestDistanceUnderAttack) {
							nearestCityUnderAttack = currCity;
							nearestDistanceUnderAttack = distance;
						}
					}
				}
			}
		}
		if(nearestCity == null){
			nearestCityUnderAttack = nearestCity;
		}
		return nearestCity;
	}
}
