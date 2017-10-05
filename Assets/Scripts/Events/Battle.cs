using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Battle {
	private Warfare _warfare;
	private Kingdom _kingdom1;
	private Kingdom _kingdom2;
	private KingdomRelationship _kr;
	private City _kingdom1City;
	private City _kingdom2City;
	private bool _isOver;
	private bool _isKingdomsAtWar;
	private Kingdom _deadAttackerKingdom;
	private Kingdom _deadDefenderKingdom;

	private City attacker;
	private City defender;
	private GameDate _supposedAttackDate;

    private List<string> _battleLogs; 

	public GameDate supposedAttackDate{
		get{ return this._supposedAttackDate; }
	}
	public City attackCity{
		get { return this.attacker; }
	}
	public City defenderCity{
		get { return this.defender; }
	}
	public Kingdom deadAttackerKingdom{
		get { return this._deadAttackerKingdom; }
	}
	public Kingdom deadDefenderKingdom{
		get { return this._deadDefenderKingdom; }
	}
    public List<string> battleLogs {
        get { return _battleLogs; }
    }

    public Battle(Warfare warfare, City kingdom1City, City kingdom2City){
		this._warfare = warfare;
		this._kingdom1 = kingdom1City.kingdom;
		this._kingdom2 = kingdom2City.kingdom;
		this._kingdom1City = kingdom1City;
		this._kingdom2City = kingdom2City;
		this._kingdom1City.isPaired = true;
		this._kingdom2City.isPaired = true;
		this._deadAttackerKingdom = null;
		this._deadDefenderKingdom = null;
		this._kr = this._kingdom1.GetRelationshipWithKingdom (this._kingdom2);
		this._isKingdomsAtWar = this._kr.isAtWar;
		this._kr.ChangeHasPairedCities (true);
		this._supposedAttackDate = new GameDate (1, 1, 1);
        this._battleLogs = new List<string>();
		SetAttackerAndDefenderCity(this._kingdom1City, this._kingdom2City);
		Step2();

		Messenger.AddListener<City> ("CityDied", CityDied);
//		if(!this._kr.isAtWar){
//			this._kr.SetPreparingWar (true);
//			this._kr.SetWarfare (this._warfare);
//		}

//		Log newLog = this._warfare.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Warfare", "first_mobilization");
//		newLog.AddToFillers (this.attacker.kingdom, this.attacker.kingdom.name, LOG_IDENTIFIER.KINGDOM_1);
//      this._warfare.ShowUINotification(newLog, new HashSet<Kingdom> { attackCity.kingdom });
	}
    private void AddBattleLog(string log) {
        _battleLogs.Add(log);
        UIManager.Instance.onAddNewBattleLog();
    }

	private void SetAttackerAndDefenderCity(City attacker, City defender){
		if (!this._warfare.isOver) {
			this.attacker = attacker;
			this.defender = defender;
			this.attacker.ChangeAttackingState (true);
			this.defender.ChangeDefendingState (true);
            AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + attacker.name + " is now attacking");
            AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + defender.name + " is now defending");
        }
	}
	private void Step1(){
		if(!this._warfare.isOver){
			GameDate gameDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
			gameDate.AddDays(5);
			SchedulingManager.Instance.AddEntry(gameDate.month, gameDate.day, gameDate.year, () => TransferPowerFromNonAdjacentCities());
			if(this._kr.isAtWar){
				SchedulingManager.Instance.AddEntry(gameDate.month, gameDate.day, gameDate.year, () => TransferDefenseFromNonAdjacentCities());
			}
			gameDate.AddDays(5);
			SchedulingManager.Instance.AddEntry(gameDate.month, gameDate.day, gameDate.year, () => TransferPowerFromNonAdjacentCities());
			if(this._kr.isAtWar){
				SchedulingManager.Instance.AddEntry(gameDate.month, gameDate.day, gameDate.year, () => TransferDefenseFromNonAdjacentCities());
			}
			SchedulingManager.Instance.AddEntry(gameDate.month, gameDate.day, gameDate.year, () => Step2());
		}
	}

	private void Step2(){
		if (!this._warfare.isOver) {
			DeclareWar ();
			Attack ();
		}
	}
	private void Step3(){
		if (!this._warfare.isOver) {
			Combat ();
		}
	}
	#region Step 1
	private void TransferPowerFromNonAdjacentCities(){
//		List<City> nonAdjacentCities = new List<City>(this.attacker.kingdom.cities);
		if (!this._warfare.isOver) {
			for (int i = 0; i < this.attacker.kingdom.cities.Count; i++) {
				City otherCity = this.attacker.kingdom.cities [i];
				if (this.attacker.id != otherCity.id) {
					if (otherCity.weapons > 0) {
						int powerTransfer = (int)(otherCity.weapons * 0.10f);
						otherCity.AdjustWeapons (-powerTransfer);
						this.attacker.AdjustWeapons (powerTransfer);
					}
				}
			}
		}
//		for (int i = 0; i < this.attacker.region.adjacentRegions.Count; i++) {
//			if(this.attacker.region.adjacentRegions[i].occupant != null){
//				if(this.attacker.region.adjacentRegions[i].occupant.kingdom.id == this.attacker.kingdom.id){
//					nonAdjacentCities.Remove(this.attacker.region.adjacentRegions[i].occupant);
//				}
//			}
//		}
//		for (int i = 0; i < nonAdjacentCities.Count; i++) {
//			City nonAdjacentCity = nonAdjacentCities[i];
//			if(nonAdjacentCity.power > 0){
//				int powerTransfer = (int)(nonAdjacentCity.power * 0.04f);
//				nonAdjacentCity.AdjustPower(-powerTransfer);
//				this.attacker.AdjustPower(powerTransfer);
//			}
//		}
	}
	private void TransferDefenseFromNonAdjacentCities(){
		if (!this._warfare.isOver) {
			for (int i = 0; i < this.defender.kingdom.cities.Count; i++) {
				City otherCity = this.defender.kingdom.cities [i];
				if (this.defender.id != otherCity.id) {
					if (otherCity.armor > 0) {
						int defenseTransfer = (int)(otherCity.armor * 0.10f);
						otherCity.AdjustArmor (-defenseTransfer);
						this.defender.AdjustArmor (defenseTransfer);
					}
				}
			}
		}
//		List<City> nonAdjacentCities = new List<City>(this.defender.kingdom.cities);
//		for (int i = 0; i < this.defender.region.adjacentRegions.Count; i++) {
//			if(this.defender.region.adjacentRegions[i].occupant != null){
//				if(this.defender.region.adjacentRegions[i].occupant.kingdom.id == this.defender.kingdom.id){
//					nonAdjacentCities.Remove(this.defender.region.adjacentRegions[i].occupant);
//				}
//			}
//		}
//		for (int i = 0; i < nonAdjacentCities.Count; i++) {
//			City nonAdjacentCity = nonAdjacentCities[i];
//			if(nonAdjacentCity.defense > 0){
//				int defenseTransfer = (int)(nonAdjacentCity.defense * 0.04f);
//				nonAdjacentCity.AdjustDefense(-defenseTransfer);
//				this.defender.AdjustDefense(defenseTransfer);
//			}
//		}
	}
	#endregion

	#region Step 2
	private void DeclareWar(){
		if(!this._kr.isAtWar){
			this._isKingdomsAtWar = true;
			this._kr.ChangeWarStatus(true, this._warfare);
			Log newLog = this._warfare.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Warfare", "declare_war");
			newLog.AddToFillers (this._kingdom1, this._kingdom1.name, LOG_IDENTIFIER.KINGDOM_1);
			newLog.AddToFillers (this._kingdom2, this._kingdom2.name, LOG_IDENTIFIER.KINGDOM_2);
			this._warfare.ShowUINotification (newLog);
		}
	}
	private void Attack(){
		GameDate gameDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
//		gameDate.AddDays(5);
//		SchedulingManager.Instance.AddEntry(gameDate.month, gameDate.day, gameDate.year, () => TransferPowerFromNonAdjacentCities());
//		SchedulingManager.Instance.AddEntry(gameDate.month, gameDate.day, gameDate.year, () => TransferDefenseFromNonAdjacentCities());
//		gameDate.AddDays(5);
//		SchedulingManager.Instance.AddEntry(gameDate.month, gameDate.day, gameDate.year, () => TransferPowerFromNonAdjacentCities());
//		SchedulingManager.Instance.AddEntry(gameDate.month, gameDate.day, gameDate.year, () => TransferDefenseFromNonAdjacentCities());
		gameDate.AddDays(UnityEngine.Random.Range(15, 31));
		SchedulingManager.Instance.AddEntry(gameDate.month, gameDate.day, gameDate.year, () => Step3());

		this._supposedAttackDate.SetDate (gameDate);

        Log offenseLog = this._warfare.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Warfare", "offense_mobilization");
        offenseLog.AddToFillers(this.attacker.kingdom, this.attacker.kingdom.name, LOG_IDENTIFIER.KINGDOM_1);
        offenseLog.AddToFillers(this.attacker, this.attacker.name, LOG_IDENTIFIER.CITY_1);
        this._warfare.ShowUINotification(offenseLog, new HashSet<Kingdom> { attacker.kingdom });
	}
	#endregion

	#region Step 3
	private void Combat(){
		if(!this.attacker.isDead && !this.defender.isDead){
			Debug.Log ("=============== ENTERING COMBAT BETWEEN " + this.attacker.name + " of " + this.attacker.kingdom.name + " AND " + this.defender.name + " of " + this.defender.kingdom.name + "===============");
			int attackerPower = this.attacker.kingdom.effectiveAttack;
			int defenderDefense = this.defender.kingdom.effectiveDefense;

            AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + attacker.name + " has an attack power of " + attackerPower.ToString());
            AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + defender.name + " has defense of " + defenderDefense.ToString());

            Debug.Log ("EFFECTIVE ATTACK: " + attackerPower);	
			Debug.Log ("EFFECTIVE DEFENSE: " + defenderDefense);
			Debug.Log ("---------------------------");

			int attackMaxRoll = (int)(Mathf.Sqrt ((2000f * (float)attackerPower)) * (1f + (0.05f * (float)this.attacker.cityLevel)));
			int defenseMaxRoll = (int)(Mathf.Sqrt ((2000f * (float)defenderDefense)) * (1f + (0.05f * (float)this.defender.cityLevel)));

            AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + attacker.name + " can roll a max of " + attackMaxRoll.ToString());
            AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + defender.name + " can roll a max of " + defenseMaxRoll.ToString());

            Debug.Log ("ATTACK MAX ROLL: " + attackMaxRoll);	
			Debug.Log ("DEFENSE MAX ROLL: " + defenseMaxRoll);
			Debug.Log ("---------------------------");

			int attackRoll = UnityEngine.Random.Range (0, attackMaxRoll);
			int defenseRoll = UnityEngine.Random.Range (0, defenseMaxRoll);

            AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + attacker.name + " rolls " + attackRoll.ToString());
            AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + defender.name + " rolls " + defenseRoll.ToString());

            Debug.Log ("ATTACK ROLL: " + attackRoll);	
			Debug.Log ("DEFENSE ROLL: " + defenseRoll);
			Debug.Log ("---------------------------");

			int attackDamage = (int)((float)attackerPower / 15f);
			int defenseDamage = (int)((float)defenderDefense / 12f);

            AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + attacker.name + " deals " + attackDamage.ToString() + " damage");
            AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + defender.name + " defends against " + defenseDamage.ToString() + " damage");

            Debug.Log ("ATTACK DAMAGE: " + attackDamage);	
			Debug.Log ("DEFENSE DAMAGE: " + defenseDamage);
			Debug.Log ("---------------------------");

			int attackAfterDamage = attackerPower - defenseDamage;
			int defenseAfterDamage = defenderDefense - attackDamage;
			if(attackAfterDamage <= 0){
				attackAfterDamage = 0;
				this._deadAttackerKingdom = this.attacker.kingdom;
			}
			if(defenseAfterDamage <= 0){
				defenseAfterDamage = 0;
				this._deadDefenderKingdom = this.defender.kingdom;
			}

			//If attackAfterDamage/defenseAfterDamage is 0, wipe out kingdom

			Debug.Log ("ATTACK AFTER DAMAGE: " + attackAfterDamage);	
			Debug.Log ("DEFENSE AFTER DAMAGE: " + defenseAfterDamage);
			Debug.Log ("---------------------------");

			if(attackAfterDamage > 0){
				int maxDamageToWeapons = GetMaxDamageToWeapons(attackAfterDamage);
				int maxRollForDamageInWeapons = this.attacker.kingdom.baseWeapons - maxDamageToWeapons;
				int minRollForDamageInWeapons = maxRollForDamageInWeapons / 2;
				int rollForDamageInWeapons = UnityEngine.Random.Range (minRollForDamageInWeapons, maxRollForDamageInWeapons + 1);
				this.attacker.kingdom.AdjustBaseWeapons (-rollForDamageInWeapons);
				int damageToSoldiersAttacker = GetDamageToSoldiers (attackAfterDamage, this.attacker.kingdom.baseWeapons);
				int damageToPopulationAttacker = GetDamageToPopulationAttacker (damageToSoldiersAttacker);
				this.attacker.kingdom.AdjustPopulation (-damageToPopulationAttacker);

				Debug.Log ("MAX DAMAGE TO WEAPONS: " + maxDamageToWeapons);
				Debug.Log ("MAX ROLL DAMAGE TO WEAPONS: " + maxRollForDamageInWeapons);
				Debug.Log ("MIN ROLL DAMAGE TO WEAPONS: " + minRollForDamageInWeapons);
				Debug.Log ("ROLL FOR DAMAGE TO WEAPONS: " + rollForDamageInWeapons);	
				Debug.Log ("DAMAGE TO ATTACKER'S POPULATION: " + damageToPopulationAttacker);
				Debug.Log ("---------------------------");

                AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + attacker.name + " loses " + damageToPopulationAttacker.ToString() + " population " +
                    "(" + attacker.kingdom.population.ToString() + ")");
            }
			if(defenseAfterDamage > 0){
				int maxDamageToArmors = GetMaxDamageToArmors(defenseAfterDamage);
				int maxRollForDamageInArmors = this.defender.kingdom.baseArmor - maxDamageToArmors;
				int minRollForDamageInArmors = maxRollForDamageInArmors / 2;
				int rollForDamageInArmors = UnityEngine.Random.Range (minRollForDamageInArmors, maxRollForDamageInArmors + 1);
				this.defender.kingdom.AdjustBaseArmors (-rollForDamageInArmors);
				int damageToSoldiersDefender = GetDamageToSoldiers (defenseAfterDamage, this.defender.kingdom.baseArmor);
				int damageToPopulationDefender = GetDamageToPopulationDefender (damageToSoldiersDefender);
				this.defender.kingdom.AdjustPopulation (-damageToPopulationDefender);

				Debug.Log ("MAX DAMAGE TO ARMORS: " + maxDamageToArmors);
				Debug.Log ("MAX ROLL DAMAGE TO ARMORS: " + maxRollForDamageInArmors);
				Debug.Log ("MIN ROLL DAMAGE TO ARMORS: " + minRollForDamageInArmors);
				Debug.Log ("ROLL FOR DAMAGE TO ARMORS: " + rollForDamageInArmors);
				Debug.Log ("DAMAGE TO DEFENDER'S POPULATION: " + damageToPopulationDefender);
				Debug.Log ("---------------------------");

                AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + defender.name + " loses " + damageToPopulationDefender.ToString() + " population " +
                    "(" + defender.kingdom.population.ToString() + ")");
            }

			if(attackRoll > defenseRoll){
                //Attacker Wins
                AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + attacker.name + " wins the battle against " + defender.name);
                EndBattle(this.attacker, this.defender);
			}else{
                //Defender Wins
                AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + defender.name + " wins the battle against " + attacker.name);
                Log newLog = this._warfare.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Warfare", "successful_defense");
				newLog.AddToFillers(this.defender, this.defender.name, LOG_IDENTIFIER.CITY_1);
				newLog.AddToFillers(this.attacker, this.attacker.name, LOG_IDENTIFIER.CITY_2);
				this._warfare.ShowUINotification(newLog);

                //When failing in attacking an enemy city, Stability is reduced by 10.
                attacker.kingdom.AdjustStability(-10);

                if (this._deadAttackerKingdom == null && this._deadDefenderKingdom == null){
					ChangePositionAndGoToStep1();
				}else{
					ForceEndBattle ();
					if(this._deadAttackerKingdom != null){
						if(!this._deadAttackerKingdom.isDead){
                            AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - Attacker kingdom " + attacker.kingdom.name + " is wiped out by " + defender.kingdom.name);
                            this._deadAttackerKingdom.AdjustPopulation (-this._deadAttackerKingdom.population);
						}
					}
					if(this._deadDefenderKingdom != null){
						if(!this._deadDefenderKingdom.isDead){
                            AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - Defending kingdom " + defender.kingdom.name + " is wiped out by " + attacker.kingdom.name);
                            this._deadDefenderKingdom.AdjustPopulation (-this._deadDefenderKingdom.population);
						}
					}
				}
			}

//			int attackerPower = this.attacker.weapons + GetPowerBuffs(this.attacker);
//			int defenderDefense = this.defender.armor + GetDefenseBuffs(this.defender);
//
//			this.attacker.AdjustWeapons (-this.defender.armor);
//			this.defender.AdjustArmor (-this.attacker.weapons);
//
//			if(attackerPower >= defenderDefense){
//				//Attacker Wins
//				EndBattle(this.attacker, this.defender);
//			}else{
//                //Defender Wins
//                Log newLog = this._warfare.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Warfare", "successful_defense");
//                newLog.AddToFillers(this.defender, this.defender.name, LOG_IDENTIFIER.CITY_1);
//                newLog.AddToFillers(this.attacker, this.attacker.name, LOG_IDENTIFIER.CITY_2);
//                this._warfare.ShowUINotification(newLog);
//                ChangePositionAndGoToStep1();
//			}
		}else{
			CityDied ();
		}
	}
	private int GetPowerBuffs(City city){
		WarfareInfo sourceWarfareInfo = city.kingdom.GetWarfareInfo(this._warfare.id);
		if (sourceWarfareInfo.warfare == null) {
			return 0;
		}
		float powerBuff = 0f;
		for (int i = 0; i < city.region.adjacentRegions.Count; i++) {
			City adjacentCity = city.region.adjacentRegions [i].occupant;
			if(adjacentCity != null){
				if(adjacentCity.kingdom.id != city.kingdom.id){
					KingdomRelationship kr = adjacentCity.kingdom.GetRelationshipWithKingdom (city.kingdom);
					if(kr.AreAllies()){
						if(kr.totalLike > 0){
							powerBuff += (adjacentCity.weapons * 0.15f);
						}else{
							//Did not honor commitment
							adjacentCity.kingdom.LeaveAlliance();
							adjacentCity.kingdom.AdjustPrestige (-GridMap.Instance.numOfRegions);
						}
					}
					if(kr.isAtWar){
						powerBuff -= (adjacentCity.weapons * 0.15f);
					}
				}else{
					powerBuff += (adjacentCity.weapons * 0.15f);
				}
			}
		}
		if(city.kingdom.alliancePool != null){
			for (int i = 0; i < city.kingdom.alliancePool.kingdomsInvolved.Count; i++) {
				Kingdom kingdom = city.kingdom.alliancePool.kingdomsInvolved [i];
				if(city.kingdom.id != kingdom.id){
					KingdomRelationship kr = kingdom.GetRelationshipWithKingdom (city.kingdom);
					if(kr.totalLike > 0){
						powerBuff += (kingdom.baseWeapons * 0.05f);
					}else{
						kingdom.LeaveAlliance();
						kingdom.AdjustPrestige (-GridMap.Instance.numOfRegions);
						i--;
						if(city.kingdom.alliancePool == null || city.kingdom.alliancePool.isDissolved){
							break;
						}
					}
				}
			}
		}
		return (int)powerBuff;
	}
	private int GetDefenseBuffs(City city){
		WarfareInfo sourceWarfareInfo = city.kingdom.GetWarfareInfo(this._warfare.id);
		if (sourceWarfareInfo.warfare == null) {
			return 0;
		}
		float defenseBuff = 0f;
		for (int i = 0; i < city.region.adjacentRegions.Count; i++) {
			City adjacentCity = city.region.adjacentRegions [i].occupant;
			if(adjacentCity != null){
				if(adjacentCity.kingdom.id != city.kingdom.id){
					KingdomRelationship kr = adjacentCity.kingdom.GetRelationshipWithKingdom (city.kingdom);
					if(kr.AreAllies()){
						if(kr.totalLike > 0){
							defenseBuff += (adjacentCity.armor * 0.15f);
						}else{
							//Did not honor commitment
							adjacentCity.kingdom.LeaveAlliance();
							adjacentCity.kingdom.AdjustPrestige (-GridMap.Instance.numOfRegions);
						}
					}
					if(kr.isAtWar){
						defenseBuff -= (adjacentCity.weapons * 0.15f);
					}
				}else{
					defenseBuff += (adjacentCity.armor * 0.15f);
				}
			}
		}
		if(city.kingdom.alliancePool != null){
			for (int i = 0; i < city.kingdom.alliancePool.kingdomsInvolved.Count; i++) {
				Kingdom kingdom = city.kingdom.alliancePool.kingdomsInvolved [i];
				if(city.kingdom.id != kingdom.id){
					KingdomRelationship kr = kingdom.GetRelationshipWithKingdom (city.kingdom);
					if(kr.totalLike > 0){
						defenseBuff += (kingdom.baseArmor * 0.05f);
					}else{
						kingdom.LeaveAlliance();
						kingdom.AdjustPrestige (-GridMap.Instance.numOfRegions);
						i--;
						if(city.kingdom.alliancePool == null || city.kingdom.alliancePool.isDissolved){
							break;
						}
					}
				}
			}
		}
		return (int)defenseBuff;
	}
	#endregion
	private void ForceEndBattle(){
		this._isOver = true;
		Messenger.RemoveListener<City> ("CityDied", CityDied);
		this._kingdom1City.isPaired = false;
		this._kingdom2City.isPaired = false;
		this._kingdom1City.ChangeAttackingState (false);
		this._kingdom1City.ChangeDefendingState (false);
		this._kingdom2City.ChangeAttackingState (false);
		this._kingdom2City.ChangeDefendingState (false);
		if (!this._kingdom1.isDead && !this._kingdom2.isDead) {
			this._kr.ChangeHasPairedCities (false);
		}
		this._warfare.RemoveBattle (this);
	}
	private void EndBattle(City winnerCity, City loserCity){
		this._isOver = true;
		Messenger.RemoveListener<City> ("CityDied", CityDied);
		this._kingdom1City.isPaired = false;
		this._kingdom2City.isPaired = false;
		this._kingdom1City.ChangeAttackingState (false);
		this._kingdom1City.ChangeDefendingState (false);
		this._kingdom2City.ChangeAttackingState (false);
		this._kingdom2City.ChangeDefendingState (false);
		if (!this._kingdom1.isDead && !this._kingdom2.isDead) {
			this._kr.ChangeHasPairedCities (false);
		}
		this._warfare.BattleEnds (winnerCity, loserCity, this);
	}
	private void CityDied(){
		this._isOver = true;
		Messenger.RemoveListener<City> ("CityDied", CityDied);
		this._kingdom1City.isPaired = false;
		this._kingdom2City.isPaired = false;
		this._kingdom1City.ChangeAttackingState (false);
		this._kingdom1City.ChangeDefendingState (false);
		this._kingdom2City.ChangeAttackingState (false);
		this._kingdom2City.ChangeDefendingState (false);
		if(!this._kingdom1.isDead && !this._kingdom2.isDead){
			this._kr.ChangeHasPairedCities (false);
			if(!this._kr.isAdjacent){
				this._warfare.PeaceDeclaration (this._kingdom1, this._kingdom2);
			}
		}

		this._warfare.RemoveBattle (this);
		if(!this.attacker.isDead){
			this._warfare.CreateNewBattle (this.attacker);
		}else{
			if(!this.attacker.kingdom.isDead){
				this._warfare.CreateNewBattle (this.attacker.kingdom);
			}
		}
	}
	private void CityDied(City city){
		if(!this._isOver){
			if(city.id == this._kingdom1City.id || city.id == this._kingdom2City.id){
				this._isOver = true;
				Messenger.RemoveListener<City> ("CityDied", CityDied);
				this._kingdom1City.isPaired = false;
				this._kingdom2City.isPaired = false;
				this._kingdom1City.ChangeAttackingState (false);
				this._kingdom1City.ChangeDefendingState (false);
				this._kingdom2City.ChangeAttackingState (false);
				this._kingdom2City.ChangeDefendingState (false);
				if(!this._kingdom1.isDead && !this._kingdom2.isDead){
					this._kr.ChangeHasPairedCities (false);
					if(!this._kr.isAdjacent){
						this._warfare.PeaceDeclaration (this._kingdom1, this._kingdom2);
						this._warfare.RemoveBattle (this);
						return;
					}
				}

				this._warfare.RemoveBattle (this);
				if(!this.attacker.isDead){
					this._warfare.CreateNewBattle (this.attacker);
				}else{
					if(!this.attacker.kingdom.isDead){
						this._warfare.CreateNewBattle (this.attacker.kingdom);
					}
				}
			}
		}
	}
	private void ChangePositionAndGoToStep1(){
		SetAttackerAndDefenderCity (this.defender, this.attacker);
		Step2 ();

//        Log offenseLog = this._warfare.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Warfare", "offense_mobilization");
//        offenseLog.AddToFillers(this.attacker.kingdom, this.attacker.kingdom.name, LOG_IDENTIFIER.KINGDOM_1);
//        offenseLog.AddToFillers(this.attacker, this.attacker.name, LOG_IDENTIFIER.CITY_1);
//        this._warfare.ShowUINotification(offenseLog, new HashSet<Kingdom> { attacker.kingdom });

//		if(this._isKingdomsAtWar){
            //if (UIManager.Instance.currentlyShowingKingdom == defender.kingdom) {
//                Log defenseLog = this._warfare.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Warfare", "defense_mobilization");
//                defenseLog.AddToFillers(this.defender.kingdom, this.defender.kingdom.name, LOG_IDENTIFIER.KINGDOM_1);
//                defenseLog.AddToFillers(this.defender, this.defender.name, LOG_IDENTIFIER.CITY_1);
//                this._warfare.ShowUINotification(defenseLog, new HashSet<Kingdom> { defender.kingdom });
            //}
//		}
	}

	private int GetMaxDamageToWeapons(int attackAfterDamage){
		//Solve for max damage to weapons which is x
		//x = attackAfterDamage * soldiers / (2 * soldiers) - attackAfterDamage;
		float soldiers = (float)this.attacker.kingdom.soldiers;
		return (int)(((float)attackAfterDamage * soldiers) / ((2f * soldiers) - (float)attackAfterDamage));
	}
	private int GetMaxDamageToArmors(int defenseAfterDamage){
		//Solve for max damage to weapons which is x
		//x = defenseAfterDamage * soldiers / (2 * soldiers) - defenseAfterDamage;
		float soldiers = (float)this.defender.kingdom.soldiers;
		return (int)(((float)defenseAfterDamage * soldiers) / ((2f * soldiers) - (float)defenseAfterDamage));
	}

	private int GetDamageToSoldiers(int remainingEffectiveAttDef, int remainingWeapArmor){
		//Solve for max damage to weapons which is x
		//x = remainingEffectiveAttDef * remainingWeapArmor / (2 * remainingWeapArmor) - remainingEffectiveAttDef;
		return (int)(((float)remainingEffectiveAttDef * remainingWeapArmor) / ((2f * remainingWeapArmor) - (float)remainingEffectiveAttDef));
	}

	private int GetDamageToPopulationAttacker(int damageToSoldiers){
		float soldiers = (float)this.attacker.kingdom.soldiers;
		float remainingSoldiers = soldiers - (float)damageToSoldiers;
		return (int)(remainingSoldiers / this.attacker.kingdom.draftRate);
	}
	private int GetDamageToPopulationDefender(int damageToSoldiers){
		float soldiers = (float)this.defender.kingdom.soldiers;
		float remainingSoldiers = soldiers - (float)damageToSoldiers;
		return (int)(remainingSoldiers / this.defender.kingdom.draftRate);
	}
}
