using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Warfare {
	private int _id;
	private string _name;
	private List<Kingdom> _sideA;
	private List<Kingdom> _sideB;
	private List<Battle> _battles;
	private List<Log> _logs;

	private Dictionary<int, WAR_SIDE> _kingdomSides;

	private bool _isOver;

	#region getters/setters
	public int id{
		get { return this._id; }
	}
	public string name{
		get { return this._name; }
	}
	public Dictionary<int, WAR_SIDE> kingdomSides{
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
		this._name = Utilities.GetWarfareName ();
		this._isOver = false;
		this._sideA = new List<Kingdom>();
		this._sideB = new List<Kingdom>();
		this._battles = new List<Battle>();
		this._logs = new List<Log> ();
		this._kingdomSides = new Dictionary<int, WAR_SIDE>();
		JoinWar(WAR_SIDE.A, firstKingdom, false);
		JoinWar(WAR_SIDE.B, secondKingdom, false);
//		InstantDeclareWarIfNotAdjacent (firstKingdom, secondKingdom);
		CreateNewBattle (firstKingdom, true);
		if(this._sideA.Count <= 0 || this._sideB.Count <= 0){
			KingdomRelationship kr = firstKingdom.GetRelationshipWithKingdom (secondKingdom);
			Debug.LogError (firstKingdom.name + " can't pair with " + secondKingdom.name + " because their adjacency is " + kr.isAdjacent.ToString () + " and invasion value is " + kr.targetKingdomInvasionValue.ToString ());
		}
		KingdomManager.Instance.AddWarfare (this);
	}
	private void SetID(){
		this._id = Utilities.lastWarfareID + 1;
		Utilities.lastWarfareID = this._id;
	}

	internal void JoinWar(WAR_SIDE side, Kingdom kingdom, bool isCreateBattle = true){
		if (!this._kingdomSides.ContainsKey(kingdom.id)) {
			this._kingdomSides.Add(kingdom.id, side);

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
		if (this._kingdomSides.ContainsKey (kingdom.id)) {
			if(this._kingdomSides[kingdom.id] == WAR_SIDE.A){
				this._sideA.Remove(kingdom);
			}else if(this._kingdomSides[kingdom.id] == WAR_SIDE.B){
				this._sideB.Remove(kingdom);
			}
			kingdom.RemoveWarfareInfo(this);
			this._kingdomSides.Remove(kingdom.id);
			CheckWarfare ();
		}


	}
	internal void BattleEnds(City winnerCity, City loserCity, Battle battle){
		//Conquer City if not null, if null means both dead
		RemoveBattle (battle);
		if(winnerCity != null && loserCity != null){
			KingdomRelationship kr = winnerCity.kingdom.GetRelationshipWithKingdom (loserCity.kingdom);

			Log newLog = CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Warfare", "invade");
			newLog.AddToFillers (winnerCity.kingdom, winnerCity.kingdom.name, LOG_IDENTIFIER.KINGDOM_1);
			newLog.AddToFillers (loserCity, loserCity.name, LOG_IDENTIFIER.CITY_2);
			ShowUINotification (newLog);

			winnerCity.kingdom.AdjustStability (-20);
			winnerCity.kingdom.ConquerCity(loserCity);

//			bool isWinnerKingdomWipedOut = false;
//			bool isLoserKingdomWipedOut = false;

			if (battle.deadAttackerKingdom != null) {
				if (!battle.deadAttackerKingdom.isDead) {
					battle.deadAttackerKingdom.AdjustPopulation (-battle.deadAttackerKingdom.population);
				}
			}
			if (battle.deadDefenderKingdom != null) {
				if(!battle.deadDefenderKingdom.isDead){
					battle.deadDefenderKingdom.AdjustPopulation (-battle.deadDefenderKingdom.population);
				}
			}


			if (!loserCity.kingdom.isDead) {
				if(!winnerCity.kingdom.isDead){
					if (winnerCity.kingdom.stability <= 0 || !kr.isAdjacent) {
						PeaceDeclaration (winnerCity.kingdom, loserCity.kingdom);
					}else{
						CreateNewBattle (winnerCity.kingdom);
					}
				}
			}else{
				if (!winnerCity.kingdom.isDead) {
					CreateNewBattle (winnerCity.kingdom);
				}
			}
		}
	}
	internal void CreateNewBattle(Kingdom kingdom, bool isFirst = false){
		if (!this._isOver) {
			if (isFirst) {
				CreateFirstBattle (kingdom);
			} else {
				City friendlyCity = null;
				City enemyCity = null;
//			List<City> nonRebellingCities = kingdom.nonRebellingCities;
				if (kingdom.cities.Count > 0) {
					friendlyCity = kingdom.cities [kingdom.cities.Count - 1];
				}
				if (friendlyCity != null) {
					enemyCity = GetEnemyCity (friendlyCity);
				}
				if (enemyCity != null) {
					Battle newBattle = new Battle (this, friendlyCity, enemyCity);
					AddBattle (newBattle);
				} else {
					CreateFirstBattle (kingdom);
				}
			}
		}
	}
	internal void CreateNewBattle(City city){
		if(!this._isOver){
			City friendlyCity = city;
			City enemyCity = GetEnemyCity (city);
			if(enemyCity != null){
				Battle newBattle = new Battle (this, friendlyCity, enemyCity);
				AddBattle (newBattle);
			}else{
				CreateFirstBattle(city.kingdom);
			}
		}

	}
	private void CreateFirstBattle(Kingdom kingdom){
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
		if(friendlyCity == null || enemyCity == null){
			
			Debug.Log ("---------------------------------Can't pair cities: force peace!");
			CantPairForcePeace(kingdom);
		}
	}
	private City GetEnemyCity(City sourceCity){
		WarfareInfo sourceKingdomInfo = sourceCity.kingdom.GetWarfareInfo (this._id);
		List<City> enemyCities = new List<City> ();

		for (int j = 0; j < sourceCity.region.adjacentRegions.Count; j++) {
			City adjacentCity = sourceCity.region.adjacentRegions [j].occupant;
			if(adjacentCity != null && adjacentCity.kingdom.id != sourceCity.kingdom.id){
				KingdomRelationship kr = sourceCity.kingdom.GetRelationshipWithKingdom (adjacentCity.kingdom);
				if(kr.battle == null){
					if(kr.warfare != null){
						if(kr.warfare.id == this._id){
							enemyCities.Add (adjacentCity);
						}
					}else{
						WarfareInfo targetKingdomInfo = adjacentCity.kingdom.GetWarfareInfo (this._id);
						if(sourceKingdomInfo.warfare != null && targetKingdomInfo.warfare != null){
							if(sourceKingdomInfo.side != targetKingdomInfo.side){
								enemyCities.Add (adjacentCity);
							}
						}

					}
				}
			}
		}
		if (enemyCities.Count > 0) {
			enemyCities = enemyCities.Distinct().ToList();
			int lowestLevel = enemyCities.Min (x => x.cityLevel);
			for (int i = 0; i < enemyCities.Count; i++) {
				if (enemyCities [i].cityLevel == lowestLevel) {
					return enemyCities [i];
				}
			}
		}
		return null;
	}
	private City GetEnemyCity(Kingdom sourceKingdom){
		WarfareInfo sourceKingdomInfo = sourceKingdom.GetWarfareInfo (this._id);
		List<City> enemyCities = new List<City> ();

		for (int i = 0; i < sourceKingdom.cities.Count; i++) {
			for (int j = 0; j < sourceKingdom.cities[i].region.adjacentRegions.Count; j++) {
				City adjacentCity = sourceKingdom.cities [i].region.adjacentRegions [j].occupant;
				if(adjacentCity != null && adjacentCity.kingdom.id != sourceKingdom.id){
					KingdomRelationship kr = sourceKingdom.GetRelationshipWithKingdom (adjacentCity.kingdom);
					if(kr.battle == null){
						if(kr.warfare != null){
							if(kr.warfare.id == this._id){
								enemyCities.Add (adjacentCity);
							}
						}else{
							WarfareInfo targetKingdomInfo = adjacentCity.kingdom.GetWarfareInfo (this._id);
							if(sourceKingdomInfo.warfare != null && targetKingdomInfo.warfare != null){
								if(sourceKingdomInfo.side != targetKingdomInfo.side){
									enemyCities.Add (adjacentCity);
								}
							}

						}
					}
				}
			}
		}
		if (enemyCities.Count > 0) {
			enemyCities = enemyCities.Distinct().ToList();
			int lowestLevel = enemyCities.Min (x => x.cityLevel);
			for (int i = 0; i < enemyCities.Count; i++) {
				if (enemyCities [i].cityLevel == lowestLevel) {
					return enemyCities [i];
				}
			}
		}
		return null;
	}


	private void AddBattle(Battle battle){
		this._battles.Add (battle);
	}
	internal void RemoveBattle(Battle battle){
		this._battles.Remove (battle);
	}
	internal void PeaceDeclaration(Kingdom kingdom1, Kingdom kingdom2){
		if(!this._isOver){
			this._isOver = true;
			DeclarePeace (kingdom1, kingdom2);
			WAR_SIDE peaceDeclarerSide = this._kingdomSides [kingdom1.id];
			if(peaceDeclarerSide == WAR_SIDE.A){
				for (int i = 0; i < this._sideA.Count; i++) {
					for (int j = 0; j < this._sideB.Count; j++) {
						DeclarePeace (this._sideA[i], this._sideB[j]);
					}
				}
			}else{
				for (int i = 0; i < this._sideB.Count; i++) {
					for (int j = 0; j < this._sideA.Count; j++) {
						DeclarePeace (this._sideB[i], this._sideA[j]);
					}
				}
			}

			WarfareDone ();
		}
	}
	private void DeclarePeace(Kingdom kingdom1, Kingdom kingdom2){
		KingdomRelationship kr = kingdom1.GetRelationshipWithKingdom (kingdom2);
		if(kr.isAtWar){
			kr.ChangeWarStatus (false, null);
			kr.ChangeRecentWar (true);
			SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year + 1, () => kr.ChangeRecentWar (false));
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
	private void CheckWarfare(){
		if(this._sideA.Count <= 0 || this._sideB.Count <= 0){
			WarfareDone ();
		}
	}
	private void WarfareDone(){
		this._isOver = true;
		KingdomManager.Instance.RemoveWarfare (this);
		while(this._sideA.Count > 0){
			this._sideA [0].RemoveWarfareInfo (this);
			this._kingdomSides.Remove(this._sideA [0].id);
			this._sideA.RemoveAt (0);
		}
		while(this._sideB.Count > 0){
			this._sideB [0].RemoveWarfareInfo (this);
			this._kingdomSides.Remove(this._sideB [0].id);
			this._sideB.RemoveAt (0);
		}
		while(this._battles.Count > 0){
			this._battles [0].ResolveBattle ();
		}
	}
	internal int GetTotalLikeOfKingdomToSide(Kingdom kingdom, WAR_SIDE side){
		int totalLike = 0;
		if(side == WAR_SIDE.A){
			for (int i = 0; i < this._sideA.Count; i++) {
				KingdomRelationship kr = kingdom.GetRelationshipWithKingdom (this._sideA [i]);
				if(kr.totalLike >= 0){
					totalLike += kr.totalLike;
				}
			}
		}else if(side == WAR_SIDE.B){
			for (int i = 0; i < this._sideB.Count; i++) {
				KingdomRelationship kr = kingdom.GetRelationshipWithKingdom (this._sideB [i]);
				if(kr.totalLike >= 0){
					totalLike += kr.totalLike;
				}
			}
		}
		return totalLike;
	}
	internal List<Kingdom> GetListFromSide(WAR_SIDE side){
		if(side == WAR_SIDE.A){
			return this._sideA;
		}else{
			return this._sideB;
		}
	}
	internal bool IsAdjacentToEnemyKingdoms(Kingdom kingdom, WAR_SIDE side){
		if(side == WAR_SIDE.A){
			for (int i = 0; i < this._sideB.Count; i++) {
				KingdomRelationship kr = kingdom.GetRelationshipWithKingdom (this._sideB [i]);
				if(kr.isAdjacent){
					return true;
				}
			}
			return false;
		}else{
			for (int i = 0; i < this._sideA.Count; i++) {
				KingdomRelationship kr = kingdom.GetRelationshipWithKingdom (this._sideA [i]);
				if(kr.isAdjacent){
					return true;
				}
			}
			return false;
		}
	}
	private void CantPairForcePeace(Kingdom kingdom){
		if(this._kingdomSides.ContainsKey(kingdom.id)){
			if (this._kingdomSides[kingdom.id] == WAR_SIDE.A) {
				if (this._sideB.Count > 0) {
					PeaceDeclaration(kingdom, this._sideB[0]);
				} else {
					WarfareDone();
					return;
				}
			} else {
				if (this._sideA.Count > 0) {
					PeaceDeclaration(kingdom, this._sideA[0]);
				} else {
					WarfareDone();
					return;
				}
			}
		}
		CheckWarfare ();
	}
}
