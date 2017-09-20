using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Battle {
	private Warfare _warfare;
	private Kingdom _kingdom1;
	private Kingdom _kingdom2;
	private City _kingdom1City;
	private City _kingdom2City;

	private City attacker;
	private City defender;

	public Battle(Warfare warfare, City kingdom1City, City kingdom2City){
		this._warfare = warfare;
		this._kingdom1 = kingdom1City.kingdom;
		this._kingdom2 = kingdom2City.kingdom;
		this._kingdom1City = kingdom1City;
		this._kingdom2City = kingdom2City;
		SetAttackerAndDefenderCity(this._kingdom1City, this._kingdom2City);
		Step1();
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
		SchedulingManager.Instance.AddEntry(gameDate.month,gameDate.day,gameDate.year, () => TransferPowerFromNonAdjacentCities(this.attacker));
	}

	private void Step2(){
		DeclareWar();
		Attack();
	}

	#region Step 1
	private void TransferPowerFromNonAdjacentCities(City city){
		List<City> nonAdjacentCities = new List<City>(city.kingdom.cities);
		for (int i = 0; i < city.region.adjacentRegions.Count; i++) {
			if(city.region.adjacentRegions[i].occupant != null){
				if(city.region.adjacentRegions[i].occupant.kingdom.id == city.kingdom.id){
					nonAdjacentCities.Remove(city.region.adjacentRegions[i].occupant);
				}
			}
		}
		for (int i = 0; i < nonAdjacentCities.Count; i++) {
			City nonAdjacentCity = nonAdjacentCities[i];
			if(nonAdjacentCity.power > 0){
				int powerTransfer = (int)(nonAdjacentCity.power * 0.15f);
				nonAdjacentCity.AdjustPower(-powerTransfer);
				city.AdjustPower(powerTransfer);
			}
		}
		Step2();
	}
	private void TransferDefenseFromNonAdjacentCities(){

	}
	#endregion

	#region Step 2
	private void DeclareWar(){
		KingdomRelationship kr = this._kingdom1.GetRelationshipWithKingdom(this._kingdom2);
		if(!kr.isAtWar){
			kr.ChangeWarStatus(true);
		}
	}
	private void Attack(){
		GameDate gameDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
		gameDate.AddMonths(1);
		SchedulingManager.Instance.AddEntry(gameDate.month,gameDate.day,gameDate.year, () => Combat());
	}
	#endregion

	#region Step 3
	private void Combat(){
		if(!this.attacker.isDead && !this.defender.isDead){
			
		}else{
			
		}
	}
	#endregion

	private void DeclareWinner(){
		if(!this._kingdom1City.isDead && this._kingdom2City.isDead){
			//Kingdom 1 wins
		}else if(this._kingdom1City.isDead && !this._kingdom2City.isDead){
			//Kingdom 1 wins
		}else{
			//Both dead
		}
	}
}
