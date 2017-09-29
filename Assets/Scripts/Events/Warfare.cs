using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Warfare {
	private int _id;
	private List<Kingdom> _sideA;
	private List<Kingdom> _sideB;
	private List<Battle> _battles;
	private List<Log> _logs;

	private Dictionary<Kingdom, WAR_SIDE> _kingdomSides;

	private bool _isOver;

	#region getters/setters
	public int id{
		get { return this._id; }
	}
	public Dictionary<Kingdom, WAR_SIDE> kingdomSides{
		get { return this._kingdomSides; }
	}
	public List<Battle> battles{
		get { return this._battles; }
	}
	public List<Kingdom> sideA{
		get { return this._sideA; }
	}
	public List<Kingdom> sideB{
		get { return this._sideB; }
	}
	public bool isOver{
		get { return this._isOver; }
	}
	#endregion
	public Warfare(Kingdom firstKingdom, Kingdom secondKingdom){
		SetID();
		this._isOver = false;
		this._sideA = new List<Kingdom>();
		this._sideB = new List<Kingdom>();
		this._battles = new List<Battle>();
		this._logs = new List<Log> ();
		this._kingdomSides = new Dictionary<Kingdom, WAR_SIDE>();
		JoinWar(WAR_SIDE.A, firstKingdom, false);
		JoinWar(WAR_SIDE.B, secondKingdom, false);
		InstantDeclareWarIfNotAdjacent (firstKingdom, secondKingdom);
		CreateNewBattle (firstKingdom, true);
		KingdomManager.Instance.AddWarfare (this);
	}
	private void SetID(){
		this._id = Utilities.lastWarfareID + 1;
		Utilities.lastWarfareID = this._id;
	}

	internal void JoinWar(WAR_SIDE side, Kingdom kingdom, bool isCreateBattle = true){
		if (!this._kingdomSides.ContainsKey(kingdom)) {
			this._kingdomSides.Add(kingdom, side);

			if(side == WAR_SIDE.A){
				this._sideA.Add(kingdom);
				for (int i = 0; i < this._sideB.Count; i++) {
					InstantDeclareWarIfNotAdjacent (kingdom, this._sideB [i]);
				}
			}else if(side == WAR_SIDE.B){
				this._sideB.Add(kingdom);
				for (int i = 0; i < this._sideA.Count; i++) {
					InstantDeclareWarIfNotAdjacent (kingdom, this._sideA [i]);
				}
			}
			kingdom.AddWarfareInfo(new WarfareInfo(side, this));
			if(isCreateBattle){
				CreateNewBattle (kingdom, true);
			}
		}
	}
	internal void UnjoinWar(Kingdom kingdom){
		if (this._kingdomSides.ContainsKey (kingdom)) {
			if(this._kingdomSides[kingdom] == WAR_SIDE.A){
				this._sideA.Remove(kingdom);
			}else if(this._kingdomSides[kingdom] == WAR_SIDE.B){
				this._sideB.Remove(kingdom);
			}
			kingdom.RemoveWarfareInfo(this);
			this._kingdomSides.Remove(kingdom);
		}

//		CheckWarfareDone ();
	}
	internal void BattleEnds(City winnerCity, City loserCity, Battle battle){
		//Conquer City if not null, if null means both dead
		RemoveBattle (battle);
		if(winnerCity != null && loserCity != null){
			Log newLog = CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Warfare", "invade");
			newLog.AddToFillers (winnerCity.kingdom, winnerCity.kingdom.name, LOG_IDENTIFIER.KINGDOM_1);
			newLog.AddToFillers (loserCity, loserCity.name, LOG_IDENTIFIER.CITY_2);
			ShowUINotification (newLog);

			winnerCity.kingdom.ConquerCity(loserCity);
			if(winnerCity.kingdom.cities.Count >= winnerCity.kingdom.cityCap){
				if(!loserCity.kingdom.isDead){
					PeaceDeclaration (winnerCity.kingdom, loserCity.kingdom);
				}else{
					CreateNewBattle (winnerCity.kingdom);
				}
			}else{
				CreateNewBattle (winnerCity.kingdom);
			}
		}
	}
	internal void CreateNewBattle(Kingdom kingdom, bool isFirst = false){
		if(isFirst){
			City friendlyCity = null;
			City enemyCity = GetEnemyCity (kingdom);
			if(enemyCity != null){
				for (int i = 0; i < enemyCity.region.adjacentRegions.Count; i++) {
					City city = enemyCity.region.adjacentRegions [i].occupant;
					if(city != null && city.kingdom.id == kingdom.id){
						friendlyCity = city;
						break;
					}
				}
			}
			if(friendlyCity != null){
				Battle newBattle = new Battle (this, friendlyCity, enemyCity);
				AddBattle (newBattle);
			}
		}else{
			City friendlyCity = null;
			City enemyCity = null;
//			List<City> nonRebellingCities = kingdom.nonRebellingCities;
			if(kingdom.cities.Count > 0){
				friendlyCity = kingdom.cities[kingdom.cities.Count - 1];
			}
			if(friendlyCity != null){
				enemyCity = GetEnemyCity (friendlyCity);
			}
			if(enemyCity != null){
				Battle newBattle = new Battle (this, friendlyCity, enemyCity);
				AddBattle (newBattle);
			}else{
				enemyCity = GetEnemyCity (kingdom);
				if (enemyCity != null) {
					Battle newBattle = new Battle (this, friendlyCity, enemyCity);
					AddBattle (newBattle);
				}
			}
		}
	}

	internal void CreateNewBattle(City city){
		City friendlyCity = city;
		City enemyCity = GetEnemyCity (city);
		if(friendlyCity != null && enemyCity != null){
			Battle newBattle = new Battle (this, friendlyCity, enemyCity);
			AddBattle (newBattle);
		}
	}

	private City GetEnemyCity(City sourceCity){
		WarfareInfo sourceWarfareInfo = sourceCity.kingdom.GetWarfareInfo(this._id);
		if (sourceWarfareInfo.warfare == null) {
			return null;
		}
		List<City> enemyCities = new List<City> ();
		for (int j = 0; j < sourceCity.region.adjacentRegions.Count; j++) {
			City adjacentCity = sourceCity.region.adjacentRegions [j].occupant;
			if(adjacentCity != null && adjacentCity.kingdom.id != sourceCity.kingdom.id){
				KingdomRelationship kr = sourceCity.kingdom.GetRelationshipWithKingdom (adjacentCity.kingdom);
				if(!adjacentCity.isPaired && adjacentCity.kingdom.warfareInfo.Count > 0 && !kr.hasPairedCities){
					WarfareInfo adjacentWarfareInfo = adjacentCity.kingdom.GetWarfareInfo(this._id);
					if(adjacentWarfareInfo.warfare != null){
						if(adjacentWarfareInfo.side != sourceWarfareInfo.side){
							if(!enemyCities.Contains(adjacentCity)){
								enemyCities.Add (adjacentCity);
							}
						}
					}
				}
			}
		}
		if (enemyCities.Count > 0) {
			int lowestDef = enemyCities.Min (x => x.defense);
			for (int i = 0; i < enemyCities.Count; i++) {
				if (enemyCities [i].defense == lowestDef) {
					return enemyCities [i];
				}
			}
		}
		return null;
	}
	private City GetEnemyCity(Kingdom sourceKingdom){
		WarfareInfo sourceWarfareInfo = sourceKingdom.GetWarfareInfo(this._id);
		if (sourceWarfareInfo.warfare == null) {
			return null;
		}
		List<City> enemyCities = new List<City> ();
		for (int i = 0; i < sourceKingdom.cities.Count; i++) {
			if(!sourceKingdom.cities[i].isPaired){
				for (int j = 0; j < sourceKingdom.cities[i].region.adjacentRegions.Count; j++) {
					City adjacentCity = sourceKingdom.cities [i].region.adjacentRegions [j].occupant;
					if(adjacentCity != null && adjacentCity.kingdom.id != sourceKingdom.id){
						KingdomRelationship kr = sourceKingdom.GetRelationshipWithKingdom (adjacentCity.kingdom);
						if (!adjacentCity.isPaired && adjacentCity.kingdom.warfareInfo.Count > 0 && !kr.hasPairedCities) {
							WarfareInfo adjacentWarfareInfo = adjacentCity.kingdom.GetWarfareInfo (this._id);
							if (adjacentWarfareInfo.warfare != null) {
								if (adjacentWarfareInfo.side != sourceWarfareInfo.side) {
									if (!enemyCities.Contains (adjacentCity)) {
										enemyCities.Add (adjacentCity);
									}
								}
							}
						}
					}
				}
			}
		}
		if(enemyCities.Count > 0){
			int lowestDef = enemyCities.Min (x => x.defense);
			for (int i = 0; i < enemyCities.Count; i++) {
				if(enemyCities[i].defense == lowestDef){
					return enemyCities [i];
				}
			}
		}
		return null;
	}

	private void AddBattle(Battle battle){
		this._battles.Add (battle);
	}
	private void RemoveBattle(Battle battle){
		this._battles.Remove (battle);
	}
	private void PeaceDeclaration(Kingdom kingdom1, Kingdom kingdom2){
		this._isOver = true;
		DeclarePeace (kingdom1, kingdom2);
		for (int i = 0; i < this._sideA.Count; i++) {
			for (int j = 0; j < this._sideB.Count; j++) {
				DeclarePeace (this._sideA[i], this._sideB[j]);
			}
		}
		WarfareDone ();
	}
	private void DeclarePeace(Kingdom kingdom1, Kingdom kingdom2){
		KingdomRelationship kr = kingdom1.GetRelationshipWithKingdom (kingdom2);
		if(kr.isAtWar){
			kr.ChangeWarStatus (false, null);
			kr.ChangeRecentWar (true);
			SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year + 1, () => kr.ChangeRecentWar (false));

			//		WarfareInfo kingdom1Info = kingdom1.GetWarfareInfo(this._id);
			//		WarfareInfo kingdom2Info = kingdom2.GetWarfareInfo(this._id);
			//
			//		if(CanUnjoinWar(kingdom1Info.side, kingdom1)){
			//			UnjoinWar (kingdom1Info.side, kingdom1);
			//		}
			//		if(CanUnjoinWar(kingdom2Info.side, kingdom2)){
			//			UnjoinWar (kingdom2Info.side, kingdom2);
			//		}


			Log newLog = CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Warfare", "peace");
			newLog.AddToFillers (kingdom1, kingdom1.name, LOG_IDENTIFIER.KINGDOM_1);
			newLog.AddToFillers (kingdom2, kingdom2.name, LOG_IDENTIFIER.KINGDOM_2);
			ShowUINotification (newLog);
		}
	}
	private void DeclarePeaceToUnadjacentKingdoms(Kingdom kingdom){
		WarfareInfo kingdomInfo = kingdom.GetWarfareInfo(this._id);
		if(kingdomInfo.side == WAR_SIDE.A){
			for (int i = 0; i < this._sideB.Count; i++) {
				KingdomRelationship kr = this._sideB[i].GetRelationshipWithKingdom(kingdom);
				if(!kr.isAdjacent && kr.isAtWar && kr.warfare.id == this._id){
					DeclarePeace (this._sideB [i], kingdom);
				}
			}
		}else{
			for (int i = 0; i < this._sideA.Count; i++) {
				KingdomRelationship kr = this._sideA[i].GetRelationshipWithKingdom(kingdom);
				if(!kr.isAdjacent && kr.isAtWar && kr.warfare.id == this._id){
					DeclarePeace (this._sideA [i], kingdom);
				}
			}
		}
	}
	private bool CanUnjoinWar(WAR_SIDE side, Kingdom kingdom){
		if(side == WAR_SIDE.A){
			for (int i = 0; i < this._sideB.Count; i++) {
				KingdomRelationship kr = kingdom.GetRelationshipWithKingdom(this._sideB[i]);
				if(kr.isAtWar && kr.warfare.id == this._id){
					return false;
				}
			}
		}else{
			for (int i = 0; i < this._sideA.Count; i++) {
				KingdomRelationship kr = kingdom.GetRelationshipWithKingdom(this._sideA[i]);
				if(kr.isAtWar && kr.warfare.id == this._id){
					return false;
				}
			}
		}
		return true;
	}
	internal Log CreateNewLogForEvent(int month, int day, int year, string category, string file, string key){
		Log newLog = new Log (month, day, year, category, file, key);
		this._logs.Add (newLog);
		return newLog;
	}
	internal void ShowUINotification(Log log, HashSet<Kingdom> kingdomsThatShouldShowNotif = null, bool addLogToHistory = true) {
		UIManager.Instance.ShowNotification(log, kingdomsThatShouldShowNotif, addLogToHistory);
	}

	private bool IsAdjacent(Kingdom kingdom1, Kingdom kingdom2){
		KingdomRelationship kr = kingdom1.GetRelationshipWithKingdom(kingdom2);
		return kr.isAdjacent;
	}

	private void InstantDeclareWarIfNotAdjacent(Kingdom kingdom1, Kingdom kingdom2){
		KingdomRelationship kr = kingdom1.GetRelationshipWithKingdom(kingdom2);
		if(!kr.isAtWar && !kr.isAdjacent){
			kr.ChangeWarStatus(true, this);
			Log newLog = CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Warfare", "declare_war");
			newLog.AddToFillers (kingdom1, kingdom1.name, LOG_IDENTIFIER.KINGDOM_1);
			newLog.AddToFillers (kingdom2, kingdom2.name, LOG_IDENTIFIER.KINGDOM_2);
			ShowUINotification (newLog);
		}
	}
	private void WarfareDone(){
//		if(this._sideA.Count <= 0 || this._sideB.Count <= 0){
		this._isOver = true;
		KingdomManager.Instance.RemoveWarfare (this);
		while(this._sideA.Count > 0){
			this._sideA [0].RemoveWarfareInfo (this);
			this._kingdomSides.Remove(this._sideA [0]);
			this._sideA.RemoveAt (0);
		}
		while(this._sideB.Count > 0){
			this._sideB [0].RemoveWarfareInfo (this);
			this._kingdomSides.Remove(this._sideB [0]);
			this._sideB.RemoveAt (0);
		}
//		}
	}
}
