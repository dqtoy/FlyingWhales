using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Battle {
	private Warfare _warfare;
	private Kingdom _kingdom1;
	private Kingdom _kingdom2;
	private City _kingdom1City;
	private City _kingdom2City;
	private bool _isOver;
	private bool _isKingdomsAtWar;

	private City attacker;
	private City defender;

	public Battle(Warfare warfare, City kingdom1City, City kingdom2City){
		this._warfare = warfare;
		this._kingdom1 = kingdom1City.kingdom;
		this._kingdom2 = kingdom2City.kingdom;
		this._kingdom1City = kingdom1City;
		this._kingdom2City = kingdom2City;
		this._kingdom1City.isUnderAttack = true;
		this._kingdom2City.isUnderAttack = true;
		this._isKingdomsAtWar = false;

		SetAttackerAndDefenderCity(this._kingdom1City, this._kingdom2City);
		Step1();
		Log newLog = this._warfare.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Warfare", "first_mobilization");
		newLog.AddToFillers (this.attacker.kingdom, this.attacker.kingdom.name, LOG_IDENTIFIER.KINGDOM_1);
		this._warfare.ShowUINotificaiton (newLog);
	}
	internal void SetWarfare(Warfare warfare){
		this._warfare = warfare;
	}
	private void SetAttackerAndDefenderCity(City attacker, City defender){
		this.attacker = attacker;
		this.defender = defender;
	}
	private void Step1(){
		GameDate gameDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
		gameDate.AddMonths(2);
		SchedulingManager.Instance.AddEntry(gameDate.month, gameDate.day, gameDate.year, () => Step2());
	}

	private void Step2(){
		TransferPowerFromNonAdjacentCities ();
		DeclareWar();
		Attack();
	}
	private void Step3(){
		Combat ();
	}
	#region Step 1
	private void TransferPowerFromNonAdjacentCities(){
		List<City> nonAdjacentCities = new List<City>(this.attacker.kingdom.cities);
		for (int i = 0; i < this.attacker.region.adjacentRegions.Count; i++) {
			if(this.attacker.region.adjacentRegions[i].occupant != null){
				if(this.attacker.region.adjacentRegions[i].occupant.kingdom.id == this.attacker.kingdom.id){
					nonAdjacentCities.Remove(this.attacker.region.adjacentRegions[i].occupant);
				}
			}
		}
		for (int i = 0; i < nonAdjacentCities.Count; i++) {
			City nonAdjacentCity = nonAdjacentCities[i];
			if(nonAdjacentCity.power > 0){
				int powerTransfer = (int)(nonAdjacentCity.power * 0.15f);
				nonAdjacentCity.AdjustPower(-powerTransfer);
				this.attacker.AdjustPower(powerTransfer);
			}
		}
	}
	private void TransferDefenseFromNonAdjacentCities(){
		List<City> nonAdjacentCities = new List<City>(this.defender.kingdom.cities);
		for (int i = 0; i < this.defender.region.adjacentRegions.Count; i++) {
			if(this.defender.region.adjacentRegions[i].occupant != null){
				if(this.defender.region.adjacentRegions[i].occupant.kingdom.id == this.defender.kingdom.id){
					nonAdjacentCities.Remove(this.defender.region.adjacentRegions[i].occupant);
				}
			}
		}
		for (int i = 0; i < nonAdjacentCities.Count; i++) {
			City nonAdjacentCity = nonAdjacentCities[i];
			if(nonAdjacentCity.defense > 0){
				int defenseTransfer = (int)(nonAdjacentCity.defense * 0.15f);
				nonAdjacentCity.AdjustDefense(-defenseTransfer);
				this.defender.AdjustDefense(defenseTransfer);
			}
		}
	}
	#endregion

	#region Step 2
	private void DeclareWar(){
		KingdomRelationship kr = this._kingdom1.GetRelationshipWithKingdom(this._kingdom2);
		if(!kr.isAtWar){
			this._isKingdomsAtWar = true;
			kr.ChangeWarStatus(true);
			Log newLog = this._warfare.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Warfare", "declare_war");
			newLog.AddToFillers (this._kingdom1, this._kingdom1.name, LOG_IDENTIFIER.KINGDOM_1);
			newLog.AddToFillers (this._kingdom2, this._kingdom2.name, LOG_IDENTIFIER.KINGDOM_2);
			this._warfare.ShowUINotificaiton (newLog);
		}else{
			TransferDefenseFromNonAdjacentCities ();
		}
	}
	private void Attack(){
		GameDate gameDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
		gameDate.AddMonths(1);
		SchedulingManager.Instance.AddEntry(gameDate.month,gameDate.day,gameDate.year, () => Step3());
	}
	#endregion

	#region Step 3
	private void Combat(){
		if(!this.attacker.isDead && !this.defender.isDead){
			int attackerPower = this.attacker.power + GetPowerBuffs(this.attacker);
			int defenderDefense = this.defender.defense + GetDefenseBuffs(this.defender);

			this.attacker.AdjustPower (-this.defender.defense);
			this.defender.AdjustDefense (-this.attacker.power);

			if(attackerPower >= defenderDefense){
				//Attacker Wins
				EndBattle(this.attacker, this.defender);
			}else{
				//Defender Wins
				ChangePositionAndGoToStep1();
			}
		}
	}
	private int GetPowerBuffs(City city){
		float powerBuff = 0f;
		for (int i = 0; i < city.region.adjacentRegions.Count; i++) {
			City adjacentCity = city.region.adjacentRegions [i].occupant;
			if(adjacentCity != null){
				if(adjacentCity.kingdom.id != city.kingdom.id){
					if(adjacentCity.kingdom.warfareInfo.warfare != null && adjacentCity.kingdom.warfareInfo.side != WAR_SIDE.NONE){
						if(adjacentCity.kingdom.warfareInfo.warfare.id == city.kingdom.warfareInfo.warfare.id){
							KingdomRelationship kr = city.kingdom.GetRelationshipWithKingdom (adjacentCity.kingdom);

							if(adjacentCity.kingdom.warfareInfo.side != city.kingdom.warfareInfo.side){
								powerBuff -= (adjacentCity.power * 0.15f);
							}else{
								if(kr.AreAllies()){
									powerBuff += (adjacentCity.power * 0.15f);
								}
							}
						}
					}
				}else{
					powerBuff += (adjacentCity.power * 0.15f);
				}
			}
		}
		if(city.kingdom.alliancePool != null){
			for (int i = 0; i < city.kingdom.alliancePool.kingdomsInvolved.Count; i++) {
				Kingdom kingdom = city.kingdom.alliancePool.kingdomsInvolved [i];
				if(city.kingdom.id != kingdom.id){
					if(kingdom.warfareInfo.side != WAR_SIDE.NONE && kingdom.warfareInfo.warfare != null){
						if(kingdom.warfareInfo.side == city.kingdom.warfareInfo.side && kingdom.warfareInfo.warfare.id == city.kingdom.warfareInfo.warfare.id){
							powerBuff += (kingdom.basePower * 0.05f);
						}
					}
				}
			}
		}
		return (int)powerBuff;
	}
	private int GetDefenseBuffs(City city){
		float defenseBuff = 0f;
		for (int i = 0; i < city.region.adjacentRegions.Count; i++) {
			City adjacentCity = city.region.adjacentRegions [i].occupant;
			if(adjacentCity != null){
				if(adjacentCity.kingdom.id != city.kingdom.id){
					if(adjacentCity.kingdom.warfareInfo.warfare != null && adjacentCity.kingdom.warfareInfo.side != WAR_SIDE.NONE){
						if(adjacentCity.kingdom.warfareInfo.warfare.id == city.kingdom.warfareInfo.warfare.id){
							KingdomRelationship kr = city.kingdom.GetRelationshipWithKingdom (adjacentCity.kingdom);

							if(adjacentCity.kingdom.warfareInfo.side != city.kingdom.warfareInfo.side){
								defenseBuff -= (adjacentCity.power * 0.15f);
							}else{
								if(kr.AreAllies()){
									defenseBuff += (adjacentCity.defense * 0.15f);
								}
							}
						}
					}
				}else{
					defenseBuff += (adjacentCity.defense * 0.15f);
				}
			}
		}
		if(city.kingdom.alliancePool != null){
			for (int i = 0; i < city.kingdom.alliancePool.kingdomsInvolved.Count; i++) {
				Kingdom kingdom = city.kingdom.alliancePool.kingdomsInvolved [i];
				if(city.kingdom.id != kingdom.id){
					if(kingdom.warfareInfo.side != WAR_SIDE.NONE && kingdom.warfareInfo.warfare != null){
						if(kingdom.warfareInfo.side == city.kingdom.warfareInfo.side && kingdom.warfareInfo.warfare.id == city.kingdom.warfareInfo.warfare.id){
							defenseBuff += (kingdom.baseDefense * 0.05f);
						}
					}
				}
			}
		}
		return (int)defenseBuff;
	}
	#endregion

	internal void CityDied(City city){
		
	}
	private void DeclareWinner(){
		if(!this._kingdom1City.isDead && this._kingdom2City.isDead){
			//Kingdom 1 wins
			EndBattle(this._kingdom1City, this._kingdom2City);
		}else if(this._kingdom1City.isDead && !this._kingdom2City.isDead){
			//Kingdom 1 wins
			EndBattle(this._kingdom2City, this._kingdom1City);
		}else{
			//Both dead
			EndBattle(null, null);
		}
	}

	private void EndBattle(City winnerCity, City loserCity){
		this._isOver = true;
		this._kingdom1City.isUnderAttack = false;
		this._kingdom2City.isUnderAttack = false;
		this._warfare.BattleEnds (winnerCity, loserCity, this);
	}

	private void ChangePositionAndGoToStep1(){
		SetAttackerAndDefenderCity (this.defender, this.attacker);
		Step1 ();
		Log offenseLog = this._warfare.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Warfare", "offense_mobilization");
		offenseLog.AddToFillers (this.attacker.kingdom, this.attacker.kingdom.name, LOG_IDENTIFIER.KINGDOM_1);
		offenseLog.AddToFillers (this.attacker, this.attacker.name, LOG_IDENTIFIER.CITY_1);
		this._warfare.ShowUINotificaiton (offenseLog);

		if(this._isKingdomsAtWar){
			Log defenseLog = this._warfare.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Warfare", "defense_mobilization");
			defenseLog.AddToFillers (this.defender.kingdom, this.defender.kingdom.name, LOG_IDENTIFIER.KINGDOM_1);
			defenseLog.AddToFillers (this.defender, this.defender.name, LOG_IDENTIFIER.CITY_1);
			this._warfare.ShowUINotificaiton (defenseLog);
		}
	}
}
