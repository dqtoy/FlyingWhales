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
	private Kingdom attackerKingdom;
	private Kingdom defenderKingdom;
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
	public Kingdom kingdom1 {
		get { return this._kingdom1; }
	}
	public Kingdom kingdom2 {
		get { return _kingdom2; }
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
		this._kr.ChangeBattle (this);
		this._supposedAttackDate = new GameDate (1, 1, 1);
        this._battleLogs = new List<string>();
		SetAttackerAndDefenderCity(this._kingdom1City, this._kingdom2City, this._kingdom1, this._kingdom2);
		Step2();

		Messenger.AddListener<City> ("CityDied", CityDied);
		Messenger.AddListener<City> ("CityTransfered", CityTransfered);

	}
    private void AddBattleLog(string log) {
        _battleLogs.Add(log);
        UIManager.Instance.onAddNewBattleLog();
    }

	private void SetAttackerAndDefenderCity(City attacker, City defender, Kingdom attackerKingdom, Kingdom defenderKingdom){
		if (!this._warfare.isOver && !this._isOver) {
			this.attacker = attacker;
			this.defender = defender;
			this.attackerKingdom = attackerKingdom;
			this.defenderKingdom = defenderKingdom;
			this.attacker.ChangeAttackingState (true);
			this.defender.ChangeDefendingState (true);
            AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + attacker.name + " is now attacking(" + attacker.kingdom.name + ")");
            AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + defender.name + " is now defending(" + defender.kingdom.name + ")");
        }
	}
	private void Step2(){
		if (!this._warfare.isOver && !this._isOver) {
			DeclareWar ();
			Attack ();
		}
	}
	private void Step3(){
		if (!this._warfare.isOver && !this._isOver) {
			Combat ();
		}
	}

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
			Debug.Log ("=============== ENTERING COMBAT BETWEEN " + this.attacker.name + " of " + this.attacker.kingdom.name + " AND " + this.defender.name + " of " + this.defender.kingdom.name + " " + GameManager.Instance.month.ToString() + "/" + GameManager.Instance.days.ToString() + "/" + GameManager.Instance.year.ToString() + " ===============");
			this._warfare.AdjustWeariness (this.attacker.kingdom, 2);

			this._deadAttackerKingdom = null;
			this._deadDefenderKingdom = null;

			int attackerPower = this.attacker.kingdom.effectiveAttack;
			int defenderDefense = this.defender.kingdom.effectiveDefense;

            //AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + attacker.name + " has an attack power of " + attackerPower.ToString());
            //AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + defender.name + " has defense of " + defenderDefense.ToString());

            Debug.Log ("EFFECTIVE ATTACK: " + attackerPower);	
			Debug.Log ("EFFECTIVE DEFENSE: " + defenderDefense);
			Debug.Log ("---------------------------");

			int attackMaxRoll = (int)(Mathf.Sqrt ((2000f * (float)attackerPower)) * (1f + (0.05f * (float)this.attacker.cityLevel)));
			int defenseMaxRoll = (int)(Mathf.Sqrt ((2000f * (float)defenderDefense)) * (1f + (0.05f * (float)this.defender.cityLevel)));

            //AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + attacker.name + " can roll a max of " + attackMaxRoll.ToString());
            //AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + defender.name + " can roll a max of " + defenseMaxRoll.ToString());

            Debug.Log ("ATTACK MAX ROLL: " + attackMaxRoll);	
			Debug.Log ("DEFENSE MAX ROLL: " + defenseMaxRoll);
			Debug.Log ("---------------------------");

			int attackRoll = UnityEngine.Random.Range (0, attackMaxRoll);
			int defenseRoll = UnityEngine.Random.Range (0, defenseMaxRoll);

            //AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + attacker.name + " rolls " + attackRoll.ToString());
            //AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + defender.name + " rolls " + defenseRoll.ToString());

            Debug.Log ("ATTACK ROLL: " + attackRoll);	
			Debug.Log ("DEFENSE ROLL: " + defenseRoll);
			Debug.Log ("---------------------------");

			int attackDamage = (int)((float)attackerPower / 15f);
			int defenseDamage = (int)((float)defenderDefense / 12f);

            //AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + attacker.name + " deals " + attackDamage.ToString() + " damage");
            //AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + defender.name + " defends against " + defenseDamage.ToString() + " damage");

            Debug.Log ("ATTACK DAMAGE: " + attackDamage);	
			Debug.Log ("DEFENSE DAMAGE: " + defenseDamage);
			Debug.Log ("---------------------------");

			int attackAfterDamage = attackerPower - defenseDamage;
			int defenseAfterDamage = defenderDefense - attackDamage;
			if(attackAfterDamage <= 0){
				attackAfterDamage = 0;
			}
			if(defenseAfterDamage <= 0){
				defenseAfterDamage = 0;
			}

			//If attackAfterDamage/defenseAfterDamage is 0, wipe out kingdom

			Debug.Log ("ATTACK AFTER DAMAGE: " + attackAfterDamage);	
			Debug.Log ("DEFENSE AFTER DAMAGE: " + defenseAfterDamage);
			Debug.Log ("---------------------------");

			Debug.Log ("ATTACKER'S POPULATION BEFORE DAMAGE: " + this.attacker.kingdom.population);

			int attackerSoldiers = this.attacker.kingdom.soldiers;
			int corpseCount = 0;
			if(attackAfterDamage > 0){
				int maxDamageToWeapons = GetMaxDamageToWeaponsArmors(attackAfterDamage, attackerSoldiers);
				int maxRollForDamageInWeapons = this.attacker.kingdom.baseWeapons - maxDamageToWeapons;
				int minRollForDamageInWeapons = maxRollForDamageInWeapons / 2;
				int damageToWeapons = UnityEngine.Random.Range (minRollForDamageInWeapons, maxRollForDamageInWeapons + 1);
				int remainingWeapons = this.attacker.kingdom.baseWeapons - damageToWeapons;
				int damageToSoldiersAttacker = GetDamageToSoldiers (attackAfterDamage, remainingWeapons);
				int damageToPopulationAttacker = GetDamageToPopulation (damageToSoldiersAttacker, attackerSoldiers, this.attacker.kingdom.draftRate);
				if(damageToPopulationAttacker > attackerSoldiers){
					damageToPopulationAttacker = attackerSoldiers;
				}

				float damageToWeapPercentage = (float)this.attacker.cityLevel / (float)this.attacker.kingdom.GetSumOfCityLevels ();
				int capDamageToWeapons = (int)((float)this.attacker.kingdom.baseWeapons * damageToWeapPercentage);
				if(damageToWeapons > capDamageToWeapons){
					damageToWeapons = capDamageToWeapons;
				}
				corpseCount += damageToPopulationAttacker;
				this.attacker.kingdom.AdjustBaseWeapons (-damageToWeapons);
				this.attacker.kingdom.AdjustPopulation (-damageToPopulationAttacker);

				Debug.Log ("MAX DAMAGE TO WEAPONS: " + maxDamageToWeapons);
				Debug.Log ("MAX ROLL DAMAGE TO WEAPONS: " + maxRollForDamageInWeapons);
				Debug.Log ("MIN ROLL DAMAGE TO WEAPONS: " + minRollForDamageInWeapons);
				Debug.Log ("ROLL FOR DAMAGE TO WEAPONS: " + damageToWeapons);
				Debug.Log ("DAMAGE TO ATTACKER'S POPULATION: " + damageToPopulationAttacker);
				Debug.Log ("---------------------------");
				AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - Damage to " + attacker.kingdom.name + " weapons " + damageToWeapons.ToString() +
					"(" + attacker.kingdom.baseWeapons.ToString() + ")");

				AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + attacker.kingdom.name + " loses " + damageToPopulationAttacker.ToString() + " population " +
					"(" + attacker.kingdom.population.ToString() + ")");
				
			}else{
				if(attackerSoldiers < this.attacker.kingdom.population){
					int damageToWeapons = this.attacker.kingdom.baseWeapons - 1;
					corpseCount += attackerSoldiers;
					SetAttackerToMaximumDamageReceived (attackerSoldiers);
					Debug.Log ("DAMAGE TO ATTACKER'S WEAPONS: " + damageToWeapons);
					Debug.Log ("DAMAGE TO ATTACKER'S POPULATION: " + attackerSoldiers);
					AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - Damage to " + attacker.kingdom.name + " weapons " + damageToWeapons.ToString() +
						"(" + this.attacker.kingdom.baseWeapons + ")");

					AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + attacker.kingdom.name + " loses " +  attackerSoldiers.ToString() + " population " +
						"(" + this.attacker.kingdom.population.ToString() + ")");
				}else{
					this._deadAttackerKingdom = this.attacker.kingdom;
					corpseCount += this._deadAttackerKingdom.population;
					Debug.Log ("ATTACKER KINGDOM IS WIPED OUT BY DEFENDER KINGDOM!");
					Debug.Log ("DAMAGE TO ATTACKER'S WEAPONS: " + this.attacker.kingdom.baseWeapons);
					Debug.Log ("DAMAGE TO ATTACKER'S POPULATION: " + attackerSoldiers);
					AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - Damage to " + attacker.kingdom.name + " weapons " + this.attacker.kingdom.baseWeapons.ToString() +
						"(0)");

					AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + attacker.kingdom.name + " loses " +  attackerSoldiers.ToString() + " population " +
						"(0)");
				}

			}


			Debug.Log ("DEFENDER'S POPULATION BEFORE DAMAGE: " + this.defender.kingdom.population);
			int defenderSoldiers = this.defender.kingdom.soldiers;
			if(defenseAfterDamage > 0){
				int maxDamageToArmors = GetMaxDamageToWeaponsArmors(defenseAfterDamage, defenderSoldiers);
				int maxRollForDamageInArmors = this.defender.kingdom.baseArmor - maxDamageToArmors;
				int minRollForDamageInArmors = maxRollForDamageInArmors / 2;
				int damageToArmors = UnityEngine.Random.Range (minRollForDamageInArmors, maxRollForDamageInArmors + 1);
				int remainingArmors = this.defender.kingdom.baseArmor - damageToArmors;
				int damageToSoldiersDefender = GetDamageToSoldiers (defenseAfterDamage, remainingArmors);
				int damageToPopulationDefender = GetDamageToPopulation (damageToSoldiersDefender, defenderSoldiers, this.defender.kingdom.draftRate);
				if(damageToPopulationDefender > defenderSoldiers){
					damageToPopulationDefender = defenderSoldiers;
				}

				float damageToArmorsPercentage = (float)this.defender.cityLevel / (float)this.defender.kingdom.GetSumOfCityLevels ();
				int capDamageToArmors = (int)((float)this.defender.kingdom.baseArmor * damageToArmorsPercentage);
				if(damageToArmors > capDamageToArmors){
					damageToArmors = capDamageToArmors;
				}
				corpseCount += damageToPopulationDefender;
				this.defender.kingdom.AdjustBaseArmors (-damageToArmors);
				this.defender.kingdom.AdjustPopulation (-damageToPopulationDefender);

				Debug.Log ("MAX DAMAGE TO ARMORS: " + maxDamageToArmors);
				Debug.Log ("MAX ROLL DAMAGE TO ARMORS: " + maxRollForDamageInArmors);
				Debug.Log ("MIN ROLL DAMAGE TO ARMORS: " + minRollForDamageInArmors);
				Debug.Log ("ROLL FOR DAMAGE TO ARMORS: " + damageToArmors);
				Debug.Log ("DAMAGE TO DEFENDER'S POPULATION: " + damageToPopulationDefender);
				Debug.Log ("---------------------------");
				AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - Damage to " + defender.kingdom.name + " armor " + damageToArmors.ToString() +
					"(" + defender.kingdom.baseArmor.ToString() + ")");

				AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + defender.kingdom.name + " loses " + damageToPopulationDefender.ToString() + " population " +
					"(" + defender.kingdom.population.ToString() + ")");
				
			}else{
				if(defenderSoldiers < this.defender.kingdom.population){
					int damageToArmors = this.defender.kingdom.baseArmor - 1;
					corpseCount += defenderSoldiers;
					SetDefenderToMaximumDamageReceived (defenderSoldiers);
					Debug.Log ("DAMAGE TO DEFENDER'S ARMORS: " + damageToArmors);
					Debug.Log ("DAMAGE TO DEFENDER'S POPULATION: " + defenderSoldiers);
					AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - Damage to " + defender.kingdom.name + " armor " + damageToArmors.ToString() +
						"(" + this.defender.kingdom.baseArmor + ")");

					AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + defender.kingdom.name + " loses " + defenderSoldiers.ToString() + " population " +
						"(" + this.defender.kingdom.population.ToString() + ")");
				}else{
					this._deadDefenderKingdom = this.defender.kingdom;
					corpseCount += this._deadDefenderKingdom.population;
					Debug.Log ("DEFENDER KINGDOM IS WIPED OUT BY ATTACKER KINGDOM!");
					Debug.Log ("DAMAGE TO DEFENDER'S ARMORS: " + this.defender.kingdom.baseArmor);
					Debug.Log ("DAMAGE TO DEFENDER'S POPULATION: " + defenderSoldiers);
					AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - Damage to " + defender.kingdom.name + " armor " + defender.kingdom.baseArmor.ToString() +
						"(0)");

					AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + defender.kingdom.name + " loses " + defenderSoldiers.ToString() + " population " +
						"(0)");
				}
			}


			if(corpseCount > 0){
				CreateCorpses (corpseCount, this.attacker.region, this.defender.region);
			}
			if(attackRoll > defenseRoll){
                //Attacker Wins
                AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + attacker.name + "(" + attacker.kingdom.name + ") wins the battle against " + defender.name + "(" + defender.kingdom.name + ")");
                EndBattle(this.attacker, this.defender);
			}else{
                //Defender Wins
				this._warfare.AdjustWeariness (this.defender.kingdom, 1);

                AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + defender.name + "(" + defender.kingdom.name + ") wins the battle against " + attacker.name + "(" + attacker.kingdom.name + ")");
                List<Kingdom> kingdomsToShowNotif = new List<Kingdom>();
                kingdomsToShowNotif.AddRange(_warfare.GetListFromSide(WAR_SIDE.A));
                kingdomsToShowNotif.AddRange(_warfare.GetListFromSide(WAR_SIDE.B));
                kingdomsToShowNotif.Add(attacker.kingdom);
                kingdomsToShowNotif.Add(defender.kingdom);
                Log newLog = this._warfare.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Warfare", "successful_defense");
				newLog.AddToFillers(this.defender, this.defender.name, LOG_IDENTIFIER.CITY_1);
				newLog.AddToFillers(this.attacker, this.attacker.name, LOG_IDENTIFIER.CITY_2);
				this._warfare.ShowUINotification(newLog, new HashSet<Kingdom>(kingdomsToShowNotif));

                //When failing in attacking an enemy city, Stability is reduced by 5.
                attacker.kingdom.AdjustStability(-5);

                if (this._deadAttackerKingdom == null && this._deadDefenderKingdom == null){
					float peaceMultiplier = this._warfare.PeaceMultiplier (this.defender.kingdom);
					int value = (int)((float)this._warfare.kingdomSideWeariness[this.defender.kingdom.id].weariness * peaceMultiplier);
					int chance = UnityEngine.Random.Range (0, 100);
					if(chance < value){
						DeclarePeaceByDefender ();
					}else{
						ChangePositionAndGoToStep1();
					}
				}else{
					ForceEndBattle ();
				}
			}
		}else{
			CityDied ();
		}
	}
	#endregion
	private void DeclarePeaceByDefender(){
		Debug.Log (this.defender.kingdom + " is defender and has declared peace with " + this.attacker.kingdom);
		this._isOver = true;
		Messenger.RemoveListener<City> ("CityDied", CityDied);
		Messenger.RemoveListener<City> ("CityTransfered", CityTransfered);
		this._kingdom1City.isPaired = false;
		this._kingdom2City.isPaired = false;
		this._kingdom1City.ChangeAttackingState (false);
		this._kingdom1City.ChangeDefendingState (false);
		this._kingdom2City.ChangeAttackingState (false);
		this._kingdom2City.ChangeDefendingState (false);
		if (!this._kingdom1.isDead && !this._kingdom2.isDead) {
			this._kr.ChangeBattle(null);
		}
		this._warfare.RemoveBattle (this);
		this._warfare.PeaceDeclaration (this.defender.kingdom);
	}
	private void ForceEndBattle(){
		string forceBattleLog = this.attacker.kingdom + " and " + this.defender.kingdom + " has forced end battle because";
		if (this._deadAttackerKingdom != null){
			forceBattleLog += " dead attacker kingdom is " + this._deadAttackerKingdom.name; 
		}
		if (this._deadDefenderKingdom != null) {
			forceBattleLog += " dead defender kingdom is " + this._deadDefenderKingdom.name; 
		}
		Debug.Log (forceBattleLog);
		this._isOver = true;
		Messenger.RemoveListener<City> ("CityDied", CityDied);
		Messenger.RemoveListener<City> ("CityTransfered", CityTransfered);
		this._kingdom1City.isPaired = false;
		this._kingdom2City.isPaired = false;
		this._kingdom1City.ChangeAttackingState (false);
		this._kingdom1City.ChangeDefendingState (false);
		this._kingdom2City.ChangeAttackingState (false);
		this._kingdom2City.ChangeDefendingState (false);
		if (!this._kingdom1.isDead && !this._kingdom2.isDead) {
			this._kr.ChangeBattle(null);
		}
		this._warfare.RemoveBattle (this);
		CheckIfKingdomsAreWipedOut ();
	}
	private void EndBattle(City winnerCity, City loserCity){
		Debug.Log (winnerCity.kingdom.name + " wins against " + loserCity.kingdom.name + ", battle ends");
		this._isOver = true;
		Messenger.RemoveListener<City> ("CityDied", CityDied);
		Messenger.RemoveListener<City> ("CityTransfered", CityTransfered);
		this._kingdom1City.isPaired = false;
		this._kingdom2City.isPaired = false;
		this._kingdom1City.ChangeAttackingState (false);
		this._kingdom1City.ChangeDefendingState (false);
		this._kingdom2City.ChangeAttackingState (false);
		this._kingdom2City.ChangeDefendingState (false);
//		if (!this._kingdom1.isDead && !this._kingdom2.isDead) {
//			this._kr.ChangeBattle(null);
//		}
		this._warfare.BattleEnds (winnerCity, loserCity, this);
		CheckIfKingdomsAreWipedOut ();
	}
	private void CityDied(){
		string cityDiedLog = "Battle will end, city has died";
		if(this.attacker.isDead){
			cityDiedLog += " " + this.attacker.name + " of " + this.attacker.kingdom.name + " is dead";
		}
		if(this.defender.isDead){
			cityDiedLog += " " + this.defender.name + " of " + this.defender.kingdom.name + " is dead";
		}
		Debug.Log (cityDiedLog);
		this._isOver = true;
		Messenger.RemoveListener<City> ("CityDied", CityDied);
		Messenger.RemoveListener<City> ("CityTransfered", CityTransfered);
		this._kingdom1City.isPaired = false;
		this._kingdom2City.isPaired = false;
		this._kingdom1City.ChangeAttackingState (false);
		this._kingdom1City.ChangeDefendingState (false);
		this._kingdom2City.ChangeAttackingState (false);
		this._kingdom2City.ChangeDefendingState (false);
		if(!this._kingdom1.isDead && !this._kingdom2.isDead){
			this._kr.ChangeBattle (null);
//			if(!this._kr.isAdjacent){
//				this._warfare.PeaceDeclaration (this._kingdom1, this._kingdom2);
//			}
		}

		this._warfare.RemoveBattle (this);
		CheckIfShouldPeaceOrPair ();
//		if(!this.attacker.isDead){
//			this._warfare.CreateNewBattle (this.attacker);
//		}else{
//			if(!this.attacker.kingdom.isDead){
//				this._warfare.CreateNewBattle (this.attacker.kingdom);
//			}
//		}

	}
	private void CityDied(City city){
		if(!this._isOver){
			if(city.id == this._kingdom1City.id || city.id == this._kingdom2City.id){
				Debug.Log (city.name + " has died, battle will end");
				this._isOver = true;
				Messenger.RemoveListener<City> ("CityDied", CityDied);
				Messenger.RemoveListener<City> ("CityTransfered", CityTransfered);
				this._kingdom1City.isPaired = false;
				this._kingdom2City.isPaired = false;
				this._kingdom1City.ChangeAttackingState (false);
				this._kingdom1City.ChangeDefendingState (false);
				this._kingdom2City.ChangeAttackingState (false);
				this._kingdom2City.ChangeDefendingState (false);
				if(!this._kingdom1.isDead && !this._kingdom2.isDead){
					this._kr.ChangeBattle (null);
				}

				this._warfare.RemoveBattle (this);
				CheckIfShouldPeaceOrPair ();
//				if(!this.attacker.isDead){
//					this._warfare.CreateNewBattle (this.attacker);
//				}else{
//					if(!this.attacker.kingdom.isDead){
//						this._warfare.CreateNewBattle (this.attacker.kingdom);
//					}
//				}
			}
		}
	}
	private void CityTransfered(City city){
		if(!this._isOver){
			if(city.id == this._kingdom1City.id || city.id == this._kingdom2City.id){
				Debug.Log (city.name + " has transfered, battle will end");
				this._isOver = true;
				Messenger.RemoveListener<City> ("CityDied", CityDied);
				Messenger.RemoveListener<City> ("CityTransfered", CityTransfered);
				this._kingdom1City.isPaired = false;
				this._kingdom2City.isPaired = false;
				this._kingdom1City.ChangeAttackingState (false);
				this._kingdom1City.ChangeDefendingState (false);
				this._kingdom2City.ChangeAttackingState (false);
				this._kingdom2City.ChangeDefendingState (false);
				if(!this._kingdom1.isDead && !this._kingdom2.isDead){
					this._kr.ChangeBattle (null);
				}

				this._warfare.RemoveBattle (this);
				CheckIfShouldPeaceOrPair ();
//				if(!this.attacker.isDead){
//					this._warfare.CreateNewBattle (this.attacker);
//				}else{
//					if(!this.attacker.kingdom.isDead){
//						this._warfare.CreateNewBattle (this.attacker.kingdom);
//					}
//				}
			}
		}
	}
	internal void ResolveBattle(){
		Debug.Log ("this battle is resolved, battle will end");
		this._isOver = true;
		Messenger.RemoveListener<City> ("CityDied", CityDied);
		Messenger.RemoveListener<City> ("CityTransfered", CityTransfered);
		this._kingdom1City.isPaired = false;
		this._kingdom2City.isPaired = false;
		this._kingdom1City.ChangeAttackingState (false);
		this._kingdom1City.ChangeDefendingState (false);
		this._kingdom2City.ChangeAttackingState (false);
		this._kingdom2City.ChangeDefendingState (false);
		if(!this._kingdom1.isDead && !this._kingdom2.isDead){
			this._kr.ChangeBattle (null);
		}
		this._warfare.RemoveBattle (this);
	}
	private void CheckIfKingdomsAreWipedOut(){
		if(this._deadAttackerKingdom != null){
			if(!this._deadAttackerKingdom.isDead){
				AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - Attacker kingdom " + attackerKingdom.name + " is wiped out by " + defenderKingdom.name);
				this._deadAttackerKingdom.SetBaseWeapons (0);
				this._deadAttackerKingdom.AdjustPopulation (-this._deadAttackerKingdom.population);
			}
		}
		if(this._deadDefenderKingdom != null){
			if(!this._deadDefenderKingdom.isDead){
				AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - Defending kingdom " + defenderKingdom.name + " is wiped out by " + attackerKingdom.name);
				this._deadDefenderKingdom.SetBaseArmor (0);
				this._deadDefenderKingdom.AdjustPopulation (-this._deadDefenderKingdom.population);
			}
		}
	}
	private void CheckIfShouldPeaceOrPair(){
		if(!this._warfare.isOver){
			if(!this._kingdom1.isDead && !this._warfare.HasAdjacentEnemy(this._kingdom1)){
				this._warfare.PeaceDeclaration (this._kingdom1);
			}
		}
		if(!this._warfare.isOver){
			if(!this._kingdom2.isDead && !this._warfare.HasAdjacentEnemy(this._kingdom2)){
				this._warfare.PeaceDeclaration (this._kingdom2);
			}
		}
		if(!this._warfare.isOver){
			this._warfare.CheckWarfare();
		}
		if(!this._warfare.isOver){
			if(!this.attackerKingdom.isDead && this._warfare.HasAdjacentEnemyWithNoBattle(this.attackerKingdom)){
				if(!this.attacker.isDead){
					this._warfare.CreateNewBattle (this.attacker);
				}else{
					this._warfare.CreateNewBattle (this.attackerKingdom);
				}
			}
		}
	}
	private void SetAttackerToMaximumDamageReceived(int soldiers){
		this.attacker.kingdom.SetBaseWeapons (1);
		this.attacker.kingdom.AdjustPopulation (-soldiers);
	}
	private void SetDefenderToMaximumDamageReceived(int soldiers){
		this.defender.kingdom.SetBaseArmor (1);
		this.defender.kingdom.AdjustPopulation (-soldiers);
	}
	private void ChangePositionAndGoToStep1(){
		SetAttackerAndDefenderCity (this.defender, this.attacker, this.defenderKingdom, this.attackerKingdom);
		Step2 ();
	}

	private int GetMaxDamageToWeaponsArmors(int afterDamage, int soldiers){
		//Solve for max damage to weapons which is x
		//x = attackAfterDamage * soldiers / (2 * soldiers) - attackAfterDamage;
//		float soldiers = (float)this.attacker.kingdom.soldiers;
		return (int)(((float)afterDamage * (float)soldiers) / ((2f * (float)soldiers) - (float)afterDamage));
	}

	private int GetDamageToSoldiers(int remainingEffectiveAttDef, int remainingWeapArmor){
		//Solve for max damage to weapons which is x
		//x = remainingEffectiveAttDef * remainingWeapArmor / (2 * remainingWeapArmor) - remainingEffectiveAttDef;
		return (int)(((float)remainingEffectiveAttDef * remainingWeapArmor) / ((2f * remainingWeapArmor) - (float)remainingEffectiveAttDef));
	}

	private int GetDamageToPopulation(int damageToSoldiers, int soldiers, float draftRate){
		float remainingSoldiers = (float)soldiers - (float)damageToSoldiers;
		return (int)(remainingSoldiers / draftRate);
	}

	private void CreateCorpses(int amount, Region attackerRegion, Region defenderRegion){
		for (int i = 0; i < defenderRegion.corpseMoundTiles.Count; i++) {
			if(defenderRegion.corpseMoundTiles[i].IsAdjacentWithRegion(attackerRegion)){
				defenderRegion.corpseMoundTiles [i].corpseMound.AdjustCorpseCount (amount);
				return;
			}
		}

		for (int i = 0; i < defenderRegion.outerTiles.Count; i++) {
			if(defenderRegion.outerTiles[i].corpseMound == null && defenderRegion.outerTiles[i].IsAdjacentWithRegion(attackerRegion)){
				defenderRegion.outerTiles [i].CreateCorpseMoundObjectOnTile (amount);
				return;
			}
		}
	}
}
