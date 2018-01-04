using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Warfare {
	private int _id;
	private string _name;
//	private List<Kingdom> _sideA;
//	private List<Kingdom> _sideB;
	private List<Battle> _battles;
    private List<Battle> _allBattles;
	private List<Log> _logs;

	private Dictionary<int, SideWeariness> _kingdomSideWeariness;
	private Dictionary<WAR_SIDE, List<Kingdom>> _kingdomSideList;

	private bool _isOver;

	#region getters/setters
	public int id{
		get { return this._id; }
	}
	public string name{
		get { return this._name; }
	}
	public Dictionary<int, SideWeariness> kingdomSideWeariness{
		get { return this._kingdomSideWeariness; }
	}
	public Dictionary<WAR_SIDE, List<Kingdom>> kingdomSideList{
		get { return this._kingdomSideList; }
	}
	public List<Battle> battles{
		get { return this._battles; }
	}
    public List<Battle> allBattles {
        get { return this._allBattles; }
    }
	public bool isOver{
		get { return this._isOver; }
	}
	#endregion
	public Warfare(Kingdom firstKingdom, Kingdom secondKingdom, bool affectWarmonerValue = true){
		SetID();
		this._name = RandomNameGenerator.Instance.GetWarfareName ();
		this._isOver = false;
		this._battles = new List<Battle>();
        this._allBattles = new List<Battle>();
        this._logs = new List<Log> ();
		this._kingdomSideWeariness = new Dictionary<int, SideWeariness>();
		this._kingdomSideList = new Dictionary<WAR_SIDE, List<Kingdom>> ();
		this._kingdomSideList.Add (WAR_SIDE.A, new List<Kingdom>());
		this._kingdomSideList.Add (WAR_SIDE.B, new List<Kingdom>());

		if(affectWarmonerValue){
			firstKingdom.AdjustWarmongerValue (75);
		}

		JoinWar(WAR_SIDE.A, firstKingdom, true);
		JoinWar(WAR_SIDE.B, secondKingdom, true);

		DeclareWar (firstKingdom, secondKingdom);

		if(firstKingdom.alliancePool != null){
			firstKingdom.alliancePool.AlliesReactionToWar (firstKingdom, secondKingdom, this);
		}
		if(secondKingdom.alliancePool != null){
			secondKingdom.alliancePool.AlliesReactionToWar (secondKingdom, firstKingdom, this);
		}
		KingdomManager.Instance.AddWarfare (this);
	}
	private void SetID(){
		this._id = Utilities.lastWarfareID + 1;
		Utilities.lastWarfareID = this._id;
	}

	internal void JoinWar(WAR_SIDE side, Kingdom kingdom, bool isFirst = false){
		if (!this._kingdomSideWeariness.ContainsKey(kingdom.id)) {
			WAR_SIDE oppositeSide = WAR_SIDE.A;
			if(side == WAR_SIDE.A){
				oppositeSide = WAR_SIDE.B;
			}
			this._kingdomSideWeariness.Add(kingdom.id, new SideWeariness(side, 0));
			this._kingdomSideList [side].Add (kingdom);

			if(!isFirst){
				for (int i = 0; i < this._kingdomSideList [oppositeSide].Count; i++) {
					DeclareWar (kingdom, this._kingdomSideList [oppositeSide] [i]);
				}
			}

			kingdom.AddWarfareInfo(new WarfareInfo(side, this));
		}
	}
	internal void UnjoinWar(Kingdom kingdom){
		if (this._kingdomSideWeariness.ContainsKey (kingdom.id)) {
			Debug.Log ("******************* " + kingdom.name + " UNJOIN " + this.name);
			this._kingdomSideList [this._kingdomSideWeariness[kingdom.id].side].Remove (kingdom);
			kingdom.RemoveWarfareInfo(this);
			this._kingdomSideWeariness.Remove(kingdom.id);
//			for (int i = 0; i < this._battles.Count; i++) {
//				if(this._battles[i].kingdom1.id == kingdom.id || this._battles[i].kingdom2.id == kingdom.id){
//					this._battles [i].ResolveBattle ();
//					i--;
//				}
//			}
			CheckWarfare ();
//			if(!this._isOver){
//				CheckSides ();
//			}
		}
	}
	private void CheckSides(){
		if(this._kingdomSideList[WAR_SIDE.A].Count > 0){
			for (int i = 0; i < this._kingdomSideList[WAR_SIDE.A].Count; i++) {
				if (!IsAdjacentToEnemyKingdoms (this._kingdomSideList [WAR_SIDE.A] [i], WAR_SIDE.A)) {
					PeaceDeclaration (this._kingdomSideList [WAR_SIDE.A] [i]);
					return;
				}
			}
		}
		if(this._kingdomSideList[WAR_SIDE.B].Count > 0){
			for (int i = 0; i < this._kingdomSideList[WAR_SIDE.B].Count; i++) {
				if (!IsAdjacentToEnemyKingdoms (this._kingdomSideList [WAR_SIDE.B] [i], WAR_SIDE.B)) {
					PeaceDeclaration (this._kingdomSideList [WAR_SIDE.B] [i]);
					return;
				}
			}
		}
	}
	internal void BattleEnds(General winnerGeneral, General loserGeneral, Battle battle){
		RemoveBattle (battle);

		City winnerCity = winnerGeneral.citizen.city;
		City loserCity = loserGeneral.citizen.city;

		Kingdom winnerKingdom = winnerCity.kingdom;
		Kingdom loserKingdom = loserCity.kingdom;

//		winnerKingdom.ConquerCity(loserCity, this);
		if(winnerGeneral.soldiers > 0){
//			loserCity.hexTile.city.AdjustSoldiers (winnerGeneral.soldiers, true, true);
//			winnerGeneral.WillDropSoldiersAndDisappear ();
			winnerGeneral.gameEventInvolvedIn.DoneEvent();
		}

		if(!winnerKingdom.isDead && !loserKingdom.isDead){
			KingdomRelationship kr = winnerKingdom.GetRelationshipWithKingdom (loserKingdom);
			kr.ChangeBattle (null);
		}

		if(!this._isOver){
			if(!loserKingdom.isDead && !HasAdjacentEnemy(loserKingdom) && this._kingdomSideWeariness.ContainsKey(loserKingdom.id)){
				Debug.Log ("LOSER: " + loserKingdom.name + " is dead or has no adjacent kingdom and will declare peace!");
				PeaceDeclaration (loserKingdom);
			}
		}
		if(!this._isOver && this._kingdomSideWeariness.ContainsKey(winnerKingdom.id)){
			if(!winnerKingdom.isDead && !HasAdjacentEnemy(winnerKingdom)){
				Debug.Log ("WINNER: " + winnerKingdom.name + " is dead or has no adjacent kingdom and will declare peace!");
				PeaceDeclaration (winnerKingdom);
				return;
			}
			if (!winnerKingdom.isDead && battle.deadAttackerKingdom == null) {
				if(!loserKingdom.isDead && battle.deadDefenderKingdom == null){
					AdjustWeariness (loserKingdom, 5);
					Debug.Log (winnerKingdom.name + " won and will try to declare peace or create new battle");
					float peaceMultiplier = PeaceMultiplier (winnerKingdom);
					int value = (int)((float)this._kingdomSideWeariness[winnerKingdom.id].weariness * peaceMultiplier);
					int chance = UnityEngine.Random.Range (0, 100);
					if(chance < value){
						PeaceDeclaration (winnerKingdom);
					}else{
						Debug.Log (winnerKingdom.name + " won and did not declare peace and will create new battle");
						CreateNewBattle (winnerKingdom);
					}
				}else{
					if(battle.deadDefenderKingdom == null){
						Debug.Log (loserKingdom.name + " lost and is dead " + winnerKingdom.name + " create new battle");
					}else{
						Debug.Log (loserKingdom.name + " lost and is dead or dead defender kingdom is " + battle.deadDefenderKingdom.name + ", " + winnerKingdom.name + " create new battle");
					}
					CreateNewBattle (winnerKingdom);
				}
			}else{
				if(battle.deadAttackerKingdom == null){
					Debug.Log (winnerKingdom.name + " won and is dead, will check this war");
				}else{
					Debug.Log (winnerKingdom.name + " won and is dead or dead attacker kingdom is " + battle.deadAttackerKingdom.name + ", will check this war");
				}
				CheckWarfare ();
			}
		}

	}
	internal void BattleEnds(City winnerCity, City loserCity, Battle battle){
		//Conquer City if not null, if null means both dead
		RemoveBattle (battle);
		if(winnerCity != null && loserCity != null){
//			KingdomRelationship kr = winnerCity.kingdom.GetRelationshipWithKingdom (loserCity.kingdom);
			Kingdom winnerKingdom = winnerCity.kingdom;
			Kingdom loserKingdom = loserCity.kingdom;

//			winnerKingdom.ConquerCity(loserCity, this);

			if(!winnerKingdom.isDead && !loserKingdom.isDead){
				KingdomRelationship kr = winnerKingdom.GetRelationshipWithKingdom (loserKingdom);
				kr.ChangeBattle (null);
			}
//			if (battle.deadAttackerKingdom != null) {
//				if (!battle.deadAttackerKingdom.isDead) {
//					battle.deadAttackerKingdom.AdjustPopulation (-battle.deadAttackerKingdom.population);
//				}
//			}
//			if (battle.deadDefenderKingdom != null) {
//				if(!battle.deadDefenderKingdom.isDead){
//					battle.deadDefenderKingdom.AdjustPopulation (-battle.deadDefenderKingdom.population);
//				}
//			}

			if(!this._isOver){
				if(!loserKingdom.isDead && !HasAdjacentEnemy(loserKingdom) && this._kingdomSideWeariness.ContainsKey(loserKingdom.id)){
					Debug.Log ("LOSER: " + loserKingdom.name + " is dead or has no adjacent kingdom and will declare peace!");
					PeaceDeclaration (loserKingdom);
				}
			}
			if(!this._isOver && this._kingdomSideWeariness.ContainsKey(winnerKingdom.id)){
				if(!winnerKingdom.isDead && !HasAdjacentEnemy(winnerKingdom)){
					Debug.Log ("WINNER: " + winnerKingdom.name + " is dead or has no adjacent kingdom and will declare peace!");
					PeaceDeclaration (winnerKingdom);
					return;
				}
				if (!winnerKingdom.isDead && battle.deadAttackerKingdom == null) {
					if(!loserKingdom.isDead && battle.deadDefenderKingdom == null){
						AdjustWeariness (loserKingdom, 5);
						Debug.Log (winnerKingdom.name + " won and will try to declare peace or create new battle");
						float peaceMultiplier = PeaceMultiplier (winnerKingdom);
						int value = (int)((float)this._kingdomSideWeariness[winnerKingdom.id].weariness * peaceMultiplier);
						int chance = UnityEngine.Random.Range (0, 100);
						if(chance < value){
							PeaceDeclaration (winnerKingdom);
						}else{
							Debug.Log (winnerKingdom.name + " won and did not declare peace and will create new battle");
							CreateNewBattle (winnerKingdom);
						}
					}else{
						if(battle.deadDefenderKingdom == null){
							Debug.Log (loserKingdom.name + " lost and is dead " + winnerKingdom.name + " create new battle");
						}else{
							Debug.Log (loserKingdom.name + " lost and is dead or dead defender kingdom is " + battle.deadDefenderKingdom.name + ", " + winnerKingdom.name + " create new battle");
						}
						CreateNewBattle (winnerKingdom);
					}
				}else{
					if(battle.deadAttackerKingdom == null){
						Debug.Log (winnerKingdom.name + " won and is dead, will check this war");
					}else{
						Debug.Log (winnerKingdom.name + " won and is dead or dead attacker kingdom is " + battle.deadAttackerKingdom.name + ", will check this war");
					}
					CheckWarfare ();
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
			for (int i = 0; i < enemyCity.region.connections.Count; i++) {
				City city = ((Region)enemyCity.region.connections [i]).occupant;
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

		for (int j = 0; j < sourceCity.region.connections.Count; j++) {
			if(sourceCity.region.connections [j] is Region){
				City adjacentCity = ((Region)sourceCity.region.connections [j]).occupant;
				if(adjacentCity != null && adjacentCity.kingdom.id != sourceCity.kingdom.id){
					KingdomRelationship kr = sourceCity.kingdom.GetRelationshipWithKingdom (adjacentCity.kingdom);
					if(kr.sharedRelationship.battle == null){
						if(kr.sharedRelationship.warfare != null){
							if(kr.sharedRelationship.warfare.id == this._id){
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
	private City GetEnemyCity(Kingdom sourceKingdom){
		WarfareInfo sourceKingdomInfo = sourceKingdom.GetWarfareInfo (this._id);
		List<City> enemyCities = new List<City> ();

		for (int i = 0; i < sourceKingdom.cities.Count; i++) {
			for (int j = 0; j < sourceKingdom.cities[i].region.connections.Count; j++) {
				if (sourceKingdom.cities [i].region.connections [j] is Region) {
					City adjacentCity = ((Region)sourceKingdom.cities [i].region.connections [j]).occupant;
					if(adjacentCity != null && adjacentCity.kingdom.id != sourceKingdom.id){
						KingdomRelationship kr = sourceKingdom.GetRelationshipWithKingdom (adjacentCity.kingdom);
						if(kr.sharedRelationship.battle == null){
							if(kr.sharedRelationship.warfare != null){
								if(kr.sharedRelationship.warfare.id == this._id){
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
        this._allBattles.Add(battle);
	}
	internal void RemoveBattle(Battle battle){
		this._battles.Remove (battle);
	}
	internal void AttemptToPeace(Kingdom winnerKingdom){
		float peaceMultiplier = PeaceMultiplier (winnerKingdom);
		int value = (int)((float)this._kingdomSideWeariness[winnerKingdom.id].weariness * peaceMultiplier);
		int chance = UnityEngine.Random.Range (0, 100);
		if(chance < value){
			PeaceDeclaration (winnerKingdom);
		}
	}
	internal void PeaceDeclaration(Kingdom kingdom1){
		if(!this._isOver){
			WAR_SIDE oppositeSide = WAR_SIDE.A;
			if(this._kingdomSideWeariness [kingdom1.id].side == WAR_SIDE.A){
				oppositeSide = WAR_SIDE.B;
			}
			if(this._kingdomSideList[oppositeSide].Count > 0){
				for (int i = 0; i < this._kingdomSideList[oppositeSide].Count; i++) {
					DeclarePeace (kingdom1, this._kingdomSideList[oppositeSide][i]);
				}
			}else{
				WarfareDone ();
				return;
			}
			UnjoinWar (kingdom1);
		}
	}
	private void DeclarePeace(Kingdom kingdom1, Kingdom kingdom2){
		KingdomRelationship kr = kingdom1.GetRelationshipWithKingdom (kingdom2);
		if(kr.sharedRelationship.isAtWar){
			kr.ChangeWarStatus (false, null);
//			kr.ChangeBattle (null);
			kr.ChangeRecentWar (true);
			SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year + 1, () => kr.ChangeRecentWar (false));
			Log newLog = CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Warfare", "peace");
			newLog.AddToFillers (kingdom1, kingdom1.name, LOG_IDENTIFIER.KINGDOM_1);
			newLog.AddToFillers (kingdom2, kingdom2.name, LOG_IDENTIFIER.KINGDOM_2);
			ShowUINotification (newLog);
			if(Messenger.eventTable.ContainsKey("PeaceDeclared")){
				Messenger.Broadcast<Kingdom, Kingdom> ("PeaceDeclared", kingdom1, kingdom2);
			}
		}
	}
	private void DeclarePeaceToUnadjacentKingdoms(Kingdom kingdom){
		WarfareInfo kingdomInfo = kingdom.GetWarfareInfo(this._id);
		WAR_SIDE side = WAR_SIDE.A;
		if(kingdomInfo.side == WAR_SIDE.A){
			side = WAR_SIDE.B;
		}
		for (int i = 0; i < this._kingdomSideList[side].Count; i++) {
			KingdomRelationship kr = this._kingdomSideList[side][i].GetRelationshipWithKingdom(kingdom);
			if(!kr.sharedRelationship.isAdjacent && kr.sharedRelationship.isAtWar && kr.sharedRelationship.warfare.id == this._id){
				DeclarePeace (this._kingdomSideList[side] [i], kingdom);
			}
		}
	}
	private bool CanUnjoinWar(WAR_SIDE side, Kingdom kingdom){
		WAR_SIDE oppositeSide = WAR_SIDE.A;
		if(side == WAR_SIDE.A){
			oppositeSide = WAR_SIDE.B;
		}
		for (int i = 0; i < this._kingdomSideList[oppositeSide].Count; i++) {
			KingdomRelationship kr = kingdom.GetRelationshipWithKingdom(this._kingdomSideList[oppositeSide][i]);
			if(kr.sharedRelationship.isAtWar && kr.sharedRelationship.warfare.id == this._id){
				return false;
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
		return kr.sharedRelationship.isAdjacent;
	}
	internal void CheckWarfare(){
		if(this._kingdomSideList[WAR_SIDE.A].Count <= 0 || this._kingdomSideList[WAR_SIDE.B].Count <= 0){
			WarfareDone ();
		}
	}
	private void WarfareDone(){
		this._isOver = true;
		KingdomManager.Instance.RemoveWarfare (this);
		while(this._kingdomSideList[WAR_SIDE.A].Count > 0){
			this._kingdomSideList[WAR_SIDE.A] [0].RemoveWarfareInfo (this);
			this._kingdomSideWeariness.Remove(this._kingdomSideList[WAR_SIDE.A] [0].id);
			this._kingdomSideList[WAR_SIDE.A].RemoveAt (0);
		}
		while(this._kingdomSideList[WAR_SIDE.B].Count > 0){
			this._kingdomSideList[WAR_SIDE.B] [0].RemoveWarfareInfo (this);
			this._kingdomSideWeariness.Remove(this._kingdomSideList[WAR_SIDE.B] [0].id);
			this._kingdomSideList[WAR_SIDE.B].RemoveAt (0);
		}
		while(this._battles.Count > 0){
			this._battles [0].ResolveBattle ();
		}
	}
	internal int GetTotalLikeOfKingdomToSide(Kingdom kingdom, WAR_SIDE side){
		int totalLike = 0;
		for (int i = 0; i < this._kingdomSideList[side].Count; i++) {
			KingdomRelationship kr = kingdom.GetRelationshipWithKingdom (this._kingdomSideList[side] [i]);
			if(kr.totalLike >= 0){
				totalLike += kr.totalLike;
			}
		}
		return totalLike;
	}
	internal List<Kingdom> GetListFromSide(WAR_SIDE side){
		return this._kingdomSideList [side];
	}
	internal bool IsAdjacentToEnemyKingdoms(Kingdom kingdom, WAR_SIDE side){
		WAR_SIDE oppositeSide = WAR_SIDE.A;
		if(side == WAR_SIDE.A){
			oppositeSide = WAR_SIDE.B;
		}
		for (int i = 0; i < this._kingdomSideList[oppositeSide].Count; i++) {
			KingdomRelationship kr = kingdom.GetRelationshipWithKingdom(this._kingdomSideList[oppositeSide][i]);
			if(kr.sharedRelationship.isAdjacent){
				return true;
			}
		}
		return false;
	}
	private void CantPairForcePeace(Kingdom kingdom){
		if(this._kingdomSideWeariness.ContainsKey(kingdom.id)){
			WAR_SIDE oppositeSide = WAR_SIDE.A;
			if(this._kingdomSideWeariness[kingdom.id].side == WAR_SIDE.A){
				oppositeSide = WAR_SIDE.B;
			}
			if (this._kingdomSideList[oppositeSide].Count > 0) {
				PeaceDeclaration(kingdom);
			} else {
				WarfareDone();
				return;
			}
		}else{
			CheckWarfare ();	
		}
	}
	internal void AdjustWeariness(Kingdom kingdom, int amount){
		if(this._kingdomSideWeariness.ContainsKey(kingdom.id)){
			SideWeariness sideWeariness = this._kingdomSideWeariness [kingdom.id];
            if(kingdom.stability < 0) {
                //Double the increase in War Weariness if Kingdom's stability is negative.
                amount *= 2;
            }
            amount = Mathf.Clamp(amount, 0, 100);
            
            sideWeariness.AdjustWeariness (amount);
			this._kingdomSideWeariness [kingdom.id] = sideWeariness;
			Debug.Log (kingdom.name + " WEARINESS: " + this._kingdomSideWeariness [kingdom.id].weariness.ToString ());
            if (amount > 0) {
                CheckForPeace(kingdom); //Each time War Weariness changes, check if a Kingdom should ask for Peace.
            }
        }
    }
	internal int GetAllAttackDefenseFromSide(WAR_SIDE side){
		int totalAttDef = 0;
		for (int i = 0; i < this._kingdomSideList[side].Count; i++) {
			totalAttDef += this._kingdomSideList [side] [i].effectiveAttack;
//			totalAttDef += this._kingdomSideList [side] [i].effectiveDefense;
		}
		return totalAttDef;
	}
	internal float PeaceMultiplier(Kingdom kingdom){
		int totalAttDefSideA = GetAllAttackDefenseFromSide (WAR_SIDE.A);
		int totalAttDefSideB = GetAllAttackDefenseFromSide (WAR_SIDE.B);
		if(this._kingdomSideWeariness[kingdom.id].side == WAR_SIDE.A){
			if(totalAttDefSideA > totalAttDefSideB){
				return 1f;
			}
		}else{
			if(totalAttDefSideB > totalAttDefSideA){
				return 1f;
			}
		}
		return 0.5f;
	}

	internal bool HasAdjacentEnemy(Kingdom kingdom){
		if(!kingdom.warfareInfo.ContainsKey(this._id)){
			return false;
		}else{
			WAR_SIDE side = kingdom.warfareInfo[this._id].side;
			WAR_SIDE oppositeSide = WAR_SIDE.A;
			if(side == WAR_SIDE.A){
				oppositeSide = WAR_SIDE.B;
			}
			for (int i = 0; i < this._kingdomSideList[oppositeSide].Count; i++) {
				KingdomRelationship kr = kingdom.GetRelationshipWithKingdom(this._kingdomSideList[oppositeSide][i]);
				if(kr.sharedRelationship.isAdjacent && kr.sharedRelationship.warfare != null && kr.sharedRelationship.warfare.id == this._id){
					return true;
				}
			}
			return false;
		}
	}
	internal bool HasAdjacentEnemyWithNoBattle(Kingdom kingdom){
		if(!kingdom.warfareInfo.ContainsKey(this._id)){
			return false;
		}else{
			WAR_SIDE side = kingdom.warfareInfo[this._id].side;
			WAR_SIDE oppositeSide = WAR_SIDE.A;
			if(side == WAR_SIDE.A){
				oppositeSide = WAR_SIDE.B;
			}
			for (int i = 0; i < this._kingdomSideList[oppositeSide].Count; i++) {
				KingdomRelationship kr = kingdom.GetRelationshipWithKingdom(this._kingdomSideList[oppositeSide][i]);
				if(kr.sharedRelationship.isAdjacent && kr.sharedRelationship.battle == null && kr.sharedRelationship.warfare != null && kr.sharedRelationship.warfare.id == this._id){
					return true;
				}
			}
			return false;
		}
	}

    internal WAR_SIDE GetSideOfKingdom(Kingdom kingdom) {
        foreach (KeyValuePair<WAR_SIDE, List<Kingdom>> kvp in _kingdomSideList) {
            WAR_SIDE currSide = kvp.Key;
            if (kvp.Value.Contains(kingdom)) {
                return currSide;
            }
        }
        return WAR_SIDE.NONE;
    }

	private void DeclareWar(Kingdom firstKingdom, Kingdom secondKingdom){
		KingdomRelationship kr = firstKingdom.GetRelationshipWithKingdom (secondKingdom);
		if(!kr.sharedRelationship.isAtWar){
			if(kr.AreAllies()){
				firstKingdom.LeaveAlliance ();
			}

			kr.ChangeWarStatus(true, this);
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Warfare", "declare_war");
			newLog.AddToFillers (firstKingdom, firstKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
			newLog.AddToFillers (secondKingdom, secondKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
			this.ShowUINotification (newLog);


		}
	}

    private void CheckForPeace(Kingdom sourceKingdom) {
        WeightedDictionary<ACTION_CHOICES> peaceWeights = GetPeaceDeclarationWeights(sourceKingdom);
        peaceWeights.LogDictionaryValues(sourceKingdom.name + " Peace Declaration (" + this.name + ") weights: ");
        ACTION_CHOICES choice = peaceWeights.PickRandomElementGivenWeights();
        Debug.Log(sourceKingdom.name + " chose to " + choice.ToString());
        if(choice == ACTION_CHOICES.DO_ACTION) {
            PeaceDeclaration(sourceKingdom);
        }
    }
    private WeightedDictionary<ACTION_CHOICES> GetPeaceDeclarationWeights(Kingdom sourceKingdom) {
        int doActionDefaultWeight = 50; //Default Weight to Declare Peace is 50
        int dontDoActionDefaultWeight = 50; //Default Weight to Dont Declare Peace is 50

        if(sourceKingdom.stability < 0) {
            //Add 1 to Default Weight to Declare Peace for every point of negative stability I have
            doActionDefaultWeight += Mathf.Abs(sourceKingdom.stability);
        } else if(sourceKingdom.stability > 0) {
            //Add 1 to Default Weight to Dont Declare Peace for every point of positive stability I have
            dontDoActionDefaultWeight += sourceKingdom.stability;
        }
        
        WAR_SIDE mySide = GetSideOfKingdom(sourceKingdom);
        WAR_SIDE enemySide = WAR_SIDE.B;
        if(mySide == WAR_SIDE.B) {
            enemySide = WAR_SIDE.A;
        }
        //loop through kingdoms in the enemy side
        List<Kingdom> enemyKingdoms = _kingdomSideList[enemySide];
        for (int i = 0; i < enemyKingdoms.Count; i++) {
            Kingdom enemyKingdom = enemyKingdoms[i];
            //compare my Relative Strength vs his Relative Strength
            KingdomRelationship relSourceWithEnemy = sourceKingdom.GetRelationshipWithKingdom(enemyKingdom);
            KingdomRelationship relEnemyWithSource = enemyKingdom.GetRelationshipWithKingdom(sourceKingdom);

            if(relSourceWithEnemy.relativeStrength > 0) {
                //add 2 to Default Weight to Declare Peace for every Relative Strength the enemy kingdoms have over me
                doActionDefaultWeight += (2 * relSourceWithEnemy.relativeStrength);
            }
            
            if(relEnemyWithSource.relativeStrength > 0) {
                //add 2 to Default Weight to Dont Declare Peace for every Relative Strength I have over each enemy kingdom
                dontDoActionDefaultWeight += 2 * relEnemyWithSource.relativeStrength;
            }
        }
        //Default Weights have a minimum value of 0
        doActionDefaultWeight = Mathf.Max(0, doActionDefaultWeight);
        dontDoActionDefaultWeight = Mathf.Max(0, dontDoActionDefaultWeight);

        //Add Default Weight to Declare Peace for every War Weariness I have
        int doAction = doActionDefaultWeight * _kingdomSideWeariness[sourceKingdom.id].weariness;

        //Add Default Weight to Dont Declare Peace for (100 - War Weariness)
        int dontDoAction = dontDoActionDefaultWeight * (100 - _kingdomSideWeariness[sourceKingdom.id].weariness);

        WeightedDictionary<ACTION_CHOICES> weights = new WeightedDictionary<ACTION_CHOICES>();
        weights.AddElement(ACTION_CHOICES.DO_ACTION, doAction);
        weights.AddElement(ACTION_CHOICES.DONT_DO_ACTION, dontDoAction);

        return weights;
    }
}
