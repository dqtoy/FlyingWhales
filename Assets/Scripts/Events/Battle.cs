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

	private City attacker;
	private City defender;
	private GameDate _supposedAttackDate;

	public GameDate supposedAttackDate{
		get{ return this._supposedAttackDate; }
	}
	public City attackCity{
		get { return this.attacker; }
	}
	public City defenderCity{
		get { return this.defender; }
	}
	public Battle(Warfare warfare, City kingdom1City, City kingdom2City){
		this._warfare = warfare;
		this._kingdom1 = kingdom1City.kingdom;
		this._kingdom2 = kingdom2City.kingdom;
		this._kingdom1City = kingdom1City;
		this._kingdom2City = kingdom2City;
		this._kingdom1City.isPaired = true;
		this._kingdom2City.isPaired = true;
		this._kr = this._kingdom1.GetRelationshipWithKingdom (this._kingdom2);
		this._isKingdomsAtWar = this._kr.isAtWar;

		this._kr.ChangeHasPairedCities (true);
//		if(!this._kr.isAtWar){
//			this._kr.SetPreparingWar (true);
//			this._kr.SetWarfare (this._warfare);
//		}

		SetAttackerAndDefenderCity(this._kingdom1City, this._kingdom2City);
		Step1();
		Log newLog = this._warfare.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Warfare", "first_mobilization");
		newLog.AddToFillers (this.attacker.kingdom, this.attacker.kingdom.name, LOG_IDENTIFIER.KINGDOM_1);
        //if(UIManager.Instance.currentlyShowingKingdom == attackCity.kingdom) {
            this._warfare.ShowUINotification(newLog, new HashSet<Kingdom> { attackCity.kingdom });
        //}
	}
	private void SetAttackerAndDefenderCity(City attacker, City defender){
		if (!this._warfare.isOver) {
			this.attacker = attacker;
			this.defender = defender;
			this.attacker.ChangeAttackingState (true);
			this.defender.ChangeDefendingState (true);

			this._supposedAttackDate = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
			this._supposedAttackDate.AddDays (25);
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
		gameDate.AddDays(5);
		SchedulingManager.Instance.AddEntry(gameDate.month, gameDate.day, gameDate.year, () => TransferPowerFromNonAdjacentCities());
		SchedulingManager.Instance.AddEntry(gameDate.month, gameDate.day, gameDate.year, () => TransferDefenseFromNonAdjacentCities());
		gameDate.AddDays(5);
		SchedulingManager.Instance.AddEntry(gameDate.month, gameDate.day, gameDate.year, () => TransferPowerFromNonAdjacentCities());
		SchedulingManager.Instance.AddEntry(gameDate.month, gameDate.day, gameDate.year, () => TransferDefenseFromNonAdjacentCities());
		gameDate.AddDays(5);
		SchedulingManager.Instance.AddEntry(gameDate.month, gameDate.day, gameDate.year, () => Step3());
	}
	#endregion

	#region Step 3
	private void Combat(){
		if(!this.attacker.isDead && !this.defender.isDead){
			int attackerPower = this.attacker.weapons + GetPowerBuffs(this.attacker);
			int defenderDefense = this.defender.armor + GetDefenseBuffs(this.defender);

			this.attacker.AdjustWeapons (-this.defender.armor);
			this.defender.AdjustArmor (-this.attacker.weapons);

			if(attackerPower >= defenderDefense){
				//Attacker Wins
				EndBattle(this.attacker, this.defender);
			}else{
                //Defender Wins
                Log newLog = this._warfare.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Warfare", "successful_defense");
                newLog.AddToFillers(this.defender, this.defender.name, LOG_IDENTIFIER.CITY_1);
                newLog.AddToFillers(this.attacker, this.attacker.name, LOG_IDENTIFIER.CITY_2);
                this._warfare.ShowUINotification(newLog);
                ChangePositionAndGoToStep1();
			}
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
	private void EndBattle(City winnerCity, City loserCity){
		this._isOver = true;
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
		this._kingdom1City.isPaired = false;
		this._kingdom2City.isPaired = false;
		this._kingdom1City.ChangeAttackingState (false);
		this._kingdom1City.ChangeDefendingState (false);
		this._kingdom2City.ChangeAttackingState (false);
		this._kingdom2City.ChangeDefendingState (false);
		if(!this._kingdom1.isDead && !this._kingdom2.isDead){
			this._kr.ChangeHasPairedCities (false);
		}

		this._warfare.RemoveBattle (this);
		if(!this.attacker.isDead){
			this._warfare.CreateNewBattle (this.attacker);
		}else{
			this._warfare.CreateNewBattle (this.attacker.kingdom);
		}

	}
	private void ChangePositionAndGoToStep1(){
		SetAttackerAndDefenderCity (this.defender, this.attacker);
		Step1 ();
        //if (UIManager.Instance.currentlyShowingKingdom == attackCity.kingdom) {
            Log offenseLog = this._warfare.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Warfare", "offense_mobilization");
            offenseLog.AddToFillers(this.attacker.kingdom, this.attacker.kingdom.name, LOG_IDENTIFIER.KINGDOM_1);
            offenseLog.AddToFillers(this.attacker, this.attacker.name, LOG_IDENTIFIER.CITY_1);
            this._warfare.ShowUINotification(offenseLog, new HashSet<Kingdom> { attacker.kingdom });
        //}

		if(this._isKingdomsAtWar){
            //if (UIManager.Instance.currentlyShowingKingdom == defender.kingdom) {
                Log defenseLog = this._warfare.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Warfare", "defense_mobilization");
                defenseLog.AddToFillers(this.defender.kingdom, this.defender.kingdom.name, LOG_IDENTIFIER.KINGDOM_1);
                defenseLog.AddToFillers(this.defender, this.defender.name, LOG_IDENTIFIER.CITY_1);
                this._warfare.ShowUINotification(defenseLog, new HashSet<Kingdom> { defender.kingdom });
            //}
		}
	}
}
