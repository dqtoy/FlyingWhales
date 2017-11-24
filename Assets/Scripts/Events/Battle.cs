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

	internal AttackCity attackCityEvent;
	internal DefendCity defendCityEvent;

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
			this.attackCityEvent = null;
			this.defendCityEvent = null;
            AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + attacker.name + " is now attacking(" + attacker.kingdom.name + ")");
            AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + defender.name + " is now defending(" + defender.kingdom.name + ")");
        }
	}
	private void Step2(){
		if (!this._warfare.isOver && !this._isOver) {
			DeclareWar ();
			AttackMobilization ();
		}
	}
//	private void Step3(){
//		if (!this._warfare.isOver && !this._isOver) {
//			Combat ();
//		}
//	}

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
	private void AttackMobilization(){
		Log offenseLog = this._warfare.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Warfare", "offense_mobilization");
		offenseLog.AddToFillers(this.attacker.kingdom, this.attacker.kingdom.name, LOG_IDENTIFIER.KINGDOM_1);
		offenseLog.AddToFillers(this.attacker, this.attacker.name, LOG_IDENTIFIER.CITY_1);
		this._warfare.ShowUINotification(offenseLog, new HashSet<Kingdom> { attacker.kingdom });

		this.attackCityEvent = EventCreator.Instance.CreateAttackCityEvent (this.attacker, this.defender, this, this.attacker.soldiers);

		int totalSoldiers = 0;
		int totalConnectedOwnedSoldiers = 0;
		int baseSoldiers = this.attacker.soldiers;
		int connectedOwnedCitiesSoldiers = 0;
		int lessenSoldiers = 0;
		List<City> ownedConnectedCities = new List<City> ();
		for (int i = 0; i < this.attackerKingdom.cities.Count; i++) {
			City city = this.attackerKingdom.cities [i];
			if(city.id != this.attacker.id && city.soldiers > 0){
				if(Utilities.AreTwoCitiesConnected(city, this.attacker, PATHFINDING_MODE.MAJOR_ROADS_ONLY_KINGDOM, this.attackerKingdom)){
					ownedConnectedCities.Add (city);
					connectedOwnedCitiesSoldiers += city.soldiers;
					for (int j = 0; j < city.region.connections.Count; j++) {
						if(city.region.connections[j] is Region){
							Region connectedRegion = (Region)city.region.connections [j];
							if(connectedRegion.occupant != null && connectedRegion.occupant.kingdom.id != city.kingdom.id){
								KingdomRelationship kr = city.kingdom.GetRelationshipWithKingdom (connectedRegion.occupant.kingdom);
								float threat = (float)kr.targetKingdomThreatLevel;
								if(threat > 100f){
									threat = 100f;
								}
								threat /= 100f;
								lessenSoldiers += Mathf.RoundToInt(threat * connectedRegion.occupant.soldiers);
							}
						}
					}
				}
			}
		}
			
		if(ownedConnectedCities.Count > 0){
			totalConnectedOwnedSoldiers = Mathf.RoundToInt((connectedOwnedCitiesSoldiers - lessenSoldiers) / (this.attacker.kingdom.warfareInfo.Count + 1));
			if(totalConnectedOwnedSoldiers < 0){
				totalConnectedOwnedSoldiers = 0;
			}
			int distributableSoldiers = Mathf.RoundToInt(totalConnectedOwnedSoldiers / ownedConnectedCities.Count);
			int excessSoldiers = 0;
			for (int i = 0; i < ownedConnectedCities.Count; i++) {
				City city = ownedConnectedCities [i];
				if (city.id != this.attacker.id) {
					int totalSoldiersCount = distributableSoldiers + excessSoldiers;
					if(city.soldiers < totalSoldiersCount){
						excessSoldiers += totalSoldiersCount - city.soldiers;
						if(city.soldiers > 0){
							ReinforceCity reinforceCity = EventCreator.Instance.CreateReinforceCityEvent (city, this.attacker, city.soldiers);
							if(reinforceCity != null){
								this.attackCityEvent.AddReinforcements (reinforceCity);
							}
						}
					}else{
						excessSoldiers = 0;
						if(totalSoldiersCount > 0){
							ReinforceCity reinforceCity = EventCreator.Instance.CreateReinforceCityEvent (city, this.attacker, totalSoldiersCount);
							if(reinforceCity != null){
								this.attackCityEvent.AddReinforcements (reinforceCity);
							}
						}
					}
				}
			}
		}

		if(this.attackCityEvent.reinforcements.Count <= 0){
			this.attackCityEvent.Attack ();
		}

	}
	internal void Attack(){
		DefenseMobilization ();
	}

	private void DefenseMobilization(){
		this.defendCityEvent = EventCreator.Instance.CreateDefendCityEvent (this.defender, this.attacker, this, this.defender.soldiers);

		List<City> ownedConnectedCities = new List<City> ();
		for (int i = 0; i < this.defender.region.connections.Count; i++) {
			if (this.defender.region.connections [i] is Region) {
				Region connectedRegion = (Region)this.defender.region.connections [i];
				if(connectedRegion.occupant != null && connectedRegion.occupant.kingdom.id == this.defender.kingdom.id && connectedRegion.occupant.soldiers > 0){
					int totalSoldiersCount = connectedRegion.occupant.soldiers / 2;
					if(totalSoldiersCount > 0){
						ReinforceCity reinforceCity = EventCreator.Instance.CreateReinforceCityEvent (connectedRegion.occupant, this.defender, totalSoldiersCount);
						if(reinforceCity != null){
							this.defendCityEvent.AddReinforcements (reinforceCity);
						}
					}
				}
			}
		}
	}

	#endregion

	#region Step 3
	internal void Combat(){
		if(!this.attacker.isDead && !this.defender.isDead){
			Debug.Log ("=============== ENTERING COMBAT BETWEEN " + this.attacker.name + " of " + this.attacker.kingdom.name + " AND " + this.defender.name + " of " + this.defender.kingdom.name + " " + GameManager.Instance.month.ToString() + "/" + GameManager.Instance.days.ToString() + "/" + GameManager.Instance.year.ToString() + " ===============");
			this._warfare.AdjustWeariness (this.attacker.kingdom, 2);

			this._deadAttackerKingdom = null;
			this._deadDefenderKingdom = null;

			int attackerPower = this.attackCityEvent.general.soldiers;
			int defenderDefense = this.defendCityEvent.general.soldiers;

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

//			int attackDamage = (int)((float)attackerPower / 15f);
//			int defenseDamage = (int)((float)defenderDefense / 12f);

            //AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + attacker.name + " deals " + attackDamage.ToString() + " damage");
            //AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + defender.name + " defends against " + defenseDamage.ToString() + " damage");

			//If attackAfterDamage/defenseAfterDamage is 0, wipe out kingdom


//			Debug.Log ("ATTACKER'S SOLDIERS BEFORE DAMAGE: " + this.attacker.kingdom.soldiersCount);
//			Debug.Log ("DEFENDER'S POPULATION BEFORE DAMAGE: " + this.defender.kingdom.soldiersCount);


//			if(attackAfterDamage > 0){
//				this.attackCityEvent.general.AdjustSoldiers (-defenseDamage);
//				Debug.Log ("DAMAGE TO ATTACKER'S SOLDIERS: " + defenseDamage);
//				Debug.Log ("---------------------------");
//
//				AddBattleLog ((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + attacker.kingdom.name + " loses " + defenseDamage.ToString () + " soldiers " +
//					"(" + attacker.kingdom.soldiersCount.ToString () + ")");
//			}else{
//				this.attackCityEvent.general.AdjustSoldiers (-attackerPower);
//				this.attackCityEvent.DoneEvent ();
//				Debug.Log ("DAMAGE TO ATTACKER'S SOLDIERS: " + attackerPower);
//				Debug.Log ("---------------------------");
//
//				AddBattleLog ((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + attacker.kingdom.name + " loses " + attackerPower.ToString () + " soldiers " +
//					"(" + attacker.kingdom.soldiersCount.ToString () + ")");
//			}
//
//
//			if (defenseAfterDamage > 0) {
//				this.defendCityEvent.general.AdjustSoldiers (-attackDamage);
//				Debug.Log ("DAMAGE TO DEFENDER'S SOLDIERS: " + attackDamage);
//				Debug.Log ("---------------------------");
//
//				AddBattleLog ((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + defender.kingdom.name + " loses " + attackDamage.ToString () + " soldiers " +
//					"(" + defender.kingdom.soldiersCount.ToString () + ")");
//
//			} else {
//				this.defendCityEvent.general.AdjustSoldiers (-defenderDefense);
//				this.defendCityEvent.DoneEvent ();
//				Debug.Log ("DAMAGE TO DEFENDER'S SOLDIERS: " + defenderDefense);
//				Debug.Log ("---------------------------");
//
//				AddBattleLog ((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + defender.kingdom.name + " loses " + defenderDefense.ToString () + " soldiers " +
//					"(" + defender.kingdom.soldiersCount.ToString () + ")");
//				
//			}

			int corpseCount = 0;

			if(corpseCount > 0 && GameManager.Instance.enableGameAgents){
				CreateCorpses (corpseCount, this.attacker.region, this.defender.region);
			}
			if(attackRoll >= defenseRoll){
                //Attacker Wins
                AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + attacker.name + "(" + attacker.kingdom.name + ") wins the battle against " + defender.name + "(" + defender.kingdom.name + ")");
				EndBattle(this.attacker, this.defender);
				AttackerWins ();
			}else{
                //Defender Wins
				DefenderWins();
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
	private void DamageComputation(General winnerGeneral, General loserGeneral){
		float winnerPowerSquared = Mathf.Pow ((float)winnerGeneral.soldiers, 2f);
		float loserPowerSquared = Mathf.Pow ((float)loserGeneral.soldiers, 2f);

		float deathPercentage = (loserPowerSquared / winnerPowerSquared) * 100f;
		int deathCount = 0;
		if(deathPercentage >= 100f){
			deathCount = winnerGeneral.soldiers;
		}else{
			for (int i = 0; i < winnerGeneral.soldiers; i++) {
				if(UnityEngine.Random.Range(0, 100) < deathPercentage){
					deathCount += 1;
				}
			}
		}
		winnerGeneral.AdjustSoldiers (-deathCount);
		loserGeneral.AdjustSoldiers (-loserGeneral.soldiers);
	}
	private void AttackerWins(){
		DamageComputation (this.attackCityEvent.general, this.defendCityEvent.general);
		this.defendCityEvent.DoneEvent ();
		if(this.attackCityEvent.general.soldiers <= 0){
			this.attackCityEvent.DoneEvent ();
		}else{
//			this.attackCityEvent.DropSoldiersAndDisappear ();
		}
	}
	private void DefenderWins(){
		DamageComputation (this.defendCityEvent.general, this.attackCityEvent.general);
		this.attackCityEvent.DoneEvent ();
		if(this.defendCityEvent.general.soldiers <= 0){
			this.defendCityEvent.DoneEvent ();
		}else{
			this.defendCityEvent.DropSoldiersAndDisappear ();
		}
	}
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
//		if(this.attackCityEvent != null && this.attackCityEvent.isActive){
//			this.attackCityEvent.CancelEvent ();
//		}
//		if(this.defendCityEvent != null && this.defendCityEvent.isActive){
//			this.defendCityEvent.CancelEvent ();
//		}
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
	internal void BattleEnd(General winnerGeneral, General loserGeneral){
		if(winnerGeneral.citizen.city.kingdom.id != loserGeneral.citizen.city.kingdom.id){
			Debug.Log (winnerGeneral.citizen.city.kingdom.name + " wins against " + loserGeneral.citizen.city.kingdom.name + ", battle ends");
			AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - " + winnerGeneral.citizen.name + " of " + winnerGeneral.citizen.city.name + "(" + winnerGeneral.citizen.city.kingdom.name + ") wins the battle against " + loserGeneral.citizen.name + " of " + loserGeneral.citizen.city.name + "(" + loserGeneral.citizen.city.kingdom.name + ")");

			if(winnerGeneral.citizen.city.id == this.attacker.id && loserGeneral.citizen.city.id == this.defender.id){
				this._isOver = true;
				Messenger.RemoveListener<City> ("CityDied", CityDied);
				Messenger.RemoveListener<City> ("CityTransfered", CityTransfered);
				this._warfare.BattleEnds (winnerGeneral, loserGeneral, this);
			}else if(winnerGeneral.citizen.city.id == this.defender.id && loserGeneral.citizen.city.id == this.attacker.id){
				winnerGeneral.DropSoldiersAndDisappear ();
				float peaceMultiplier = this._warfare.PeaceMultiplier (this.defender.kingdom);
				int value = (int)((float)this._warfare.kingdomSideWeariness[this.defender.kingdom.id].weariness * peaceMultiplier);
				int chance = UnityEngine.Random.Range (0, 100);
				if(chance < value){
					DeclarePeaceByDefender ();
				}else{
					ChangePositionAndGoToStep1();
				}
			}
		}
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
				if(this.attackCityEvent != null && this.attackCityEvent.isActive){
					this.attackCityEvent.CancelEvent ();
				}
				if(this.defendCityEvent != null && this.defendCityEvent.isActive){
					this.defendCityEvent.CancelEvent ();
				}

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
				if(this.attackCityEvent != null && this.attackCityEvent.isActive){
					this.attackCityEvent.CancelEvent ();
				}
				if(this.defendCityEvent != null && this.defendCityEvent.isActive){
					this.defendCityEvent.CancelEvent ();
				}
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
		if(this.attackCityEvent != null && this.attackCityEvent.isActive){
			this.attackCityEvent.CancelEvent ();
		}
		if(this.defendCityEvent != null && this.defendCityEvent.isActive){
			this.defendCityEvent.CancelEvent ();
		}
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
				this._deadAttackerKingdom.DamagePopulation (this._deadAttackerKingdom.population);
			}
		}
		if(this._deadDefenderKingdom != null){
			if(!this._deadDefenderKingdom.isDead){
				AddBattleLog((MONTH)GameManager.Instance.month + " " + GameManager.Instance.days + ", " + GameManager.Instance.year + " - Defending kingdom " + defenderKingdom.name + " is wiped out by " + attackerKingdom.name);
				this._deadDefenderKingdom.SetBaseWeapons (0);
				this._deadDefenderKingdom.DamagePopulation (this._deadDefenderKingdom.population);
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
		this.attacker.kingdom.DamagePopulation (soldiers);
	}
	private void SetDefenderToMaximumDamageReceived(int soldiers){
		this.defender.kingdom.SetBaseWeapons (1);
		this.defender.kingdom.DamagePopulation (soldiers);
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
			if(defenderRegion.outerTiles[i].elevationType != ELEVATION.WATER && defenderRegion.outerTiles[i].corpseMound == null && defenderRegion.outerTiles[i].IsAdjacentWithRegion(attackerRegion)){
				defenderRegion.outerTiles [i].CreateCorpseMoundObjectOnTile (amount);
				return;
			}
		}
	}
}
