using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class War : GameEvent {

	private Kingdom _kingdom1;
	private Kingdom _kingdom2;

	private RelationshipKingdom _kingdom1Rel;
	private RelationshipKingdom _kingdom2Rel;

	internal CityWarPair warPair;

	private bool _isAtWar;
	private int attackRate;
	private bool kingdom1Attacked;
	private bool isInitialAttack;

	internal GameEvent gameEventTrigger;
	internal WAR_TRIGGER warTrigger;

	private int kingdom1Waves;
	private int kingdom2Waves;

	#region getters/setters
	public Kingdom kingdom1 {
		get { return _kingdom1; }
	}

	public Kingdom kingdom2{
		get { return _kingdom2; }
	}

	public RelationshipKingdom kingdom1Rel {
		get { return _kingdom1Rel; }
	}

	public RelationshipKingdom kingdom2Rel {
		get { return _kingdom2Rel; }
	}

	public bool isAtWar {
		get { return _isAtWar; }
	}
	#endregion

	public War(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom _kingdom1, Kingdom _kingdom2, WAR_TRIGGER warTrigger) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.KINGDOM_WAR;
		this.name = "War";
		this.description = "War between " + _kingdom1.name + " and " + _kingdom2.name + ".";
		this._kingdom1 = _kingdom1;
		this._kingdom2 = _kingdom2;
		this._kingdom1Rel = _kingdom1.GetRelationshipWithOtherKingdom(_kingdom2);
		this._kingdom2Rel = _kingdom2.GetRelationshipWithOtherKingdom(_kingdom1);
		this._kingdom1Rel.AssignWarEvent(this);
		this._kingdom2Rel.AssignWarEvent(this);
		this.warPair.DefaultValues();
		this.kingdom1Attacked = false;
		this.isInitialAttack = false;
		this.attackRate = 0;
		this.gameEventTrigger = null;
		this.warTrigger = warTrigger;

		Log titleLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "War", "event_title");
		titleLog.AddToFillers (_kingdom1, _kingdom1.name, LOG_IDENTIFIER.KINGDOM_1);
		titleLog.AddToFillers (_kingdom2, _kingdom2.name, LOG_IDENTIFIER.KINGDOM_2);

		EventManager.Instance.onUpdatePath.AddListener (UpdatePath);
		EventManager.Instance.AddEventToDictionary(this);
	
	}
	internal override void PerformAction (){
		Attack ();
	}
	internal void CreateInvasionPlan(Kingdom kingdomToDeclare, GameEvent gameEventTrigger){
        this.gameEventTrigger = gameEventTrigger;
		if (kingdomToDeclare.id == this._kingdom1.id) {
			this._kingdom1Rel.CreateInvasionPlan(gameEventTrigger);
		} else {
			this._kingdom2Rel.CreateInvasionPlan(gameEventTrigger);
		}
	}

	internal void CreateRequestPeaceEvent(Kingdom kingdomToRequest){
        RequestPeace requestPeaceEvent = null;
		if (kingdomToRequest.id == this._kingdom1.id) {
            //this._kingdom1Rel.CreateRequestPeaceEvent(citizenToSend, saboteurs);
            requestPeaceEvent = EventCreator.Instance.CreateRequestPeace(kingdomToRequest, this._kingdom2);
            if (requestPeaceEvent != null) {
                this._kingdom1Rel.AssignRequestPeaceEvent(requestPeaceEvent);
            }
        } else {
            //this._kingdom2Rel.CreateRequestPeaceEvent(citizenToSend, saboteurs);
            requestPeaceEvent = EventCreator.Instance.CreateRequestPeace(kingdomToRequest, this._kingdom1);
            if (requestPeaceEvent != null) {
                this._kingdom2Rel.AssignRequestPeaceEvent(requestPeaceEvent);
            }
        }
	}

	internal void DeclareWar(Kingdom sourceKingdom){
		if(!this._isAtWar){
			this._isAtWar = true;
			if(sourceKingdom.id == this._kingdom1.id){
				KingdomManager.Instance.DeclareWarBetweenKingdoms(this._kingdom1, this._kingdom2, this);
			}else{
				KingdomManager.Instance.DeclareWarBetweenKingdoms(this._kingdom2, this._kingdom1, this);
			}
			this.isInitialAttack = true;
//            EventManager.Instance.onWeekEnd.AddListener(AttemptToRequestPeace);
			this.ReplenishWavesKingdom1();
			this.ReplenishWavesKingdom2();
			EventManager.Instance.onWeekEnd.AddListener (this.PerformAction);

		}
	}

	internal void DeclarePeace(){
		this._isAtWar = false;

		this._kingdom1Rel.DeclarePeace();
		this._kingdom2Rel.DeclarePeace();

        RelationshipKings rel1 = kingdom1.king.GetRelationshipWithCitizen(kingdom2.king);
        RelationshipKings rel2 = kingdom2.king.GetRelationshipWithCitizen(kingdom1.king);

        rel1.AddEventModifier(-30, "recent war with " + kingdom2.name, this);
        rel2.AddEventModifier(-30, "recent war with " + kingdom1.name, this);

        KingdomManager.Instance.DeclarePeaceBetweenKingdoms(this._kingdom1, this._kingdom2);
		this.DoneEvent();
	}

	internal Kingdom GetKingdomInvolvedInWar(Kingdom kingdom){
		if (kingdom1.id == kingdom.id) {
			return kingdom1;
		} else {
			return kingdom2;
		}
	}

	internal void InvasionPlanCancelled(){
		if (this._kingdom1Rel.invasionPlan == null && this._kingdom2Rel.invasionPlan == null) {
			this.DeclarePeace();
			return;
		}

		if (this._kingdom1Rel.invasionPlan != null) {
			if (this._kingdom2Rel.invasionPlan != null) {
				if (!this._kingdom1Rel.invasionPlan.isActive && !this._kingdom2Rel.invasionPlan.isActive) {
					this.DeclarePeace ();
					return;
				}
			} else {
				if (!this._kingdom1Rel.invasionPlan.isActive) {
					this.DeclarePeace ();
					return;
				}
			}
		} else {
			if (this._kingdom2Rel.invasionPlan != null) {
				if (!this._kingdom2Rel.invasionPlan.isActive) {
					this.DeclarePeace ();
					return;
				}
			}
		}
	}

    protected void AttemptToRequestPeace() {
        Kingdom[] kingdomsInWar = new Kingdom[] { this._kingdom1, this._kingdom2 };
        for (int i = 0; i < kingdomsInWar.Length; i++) {
            Kingdom currKingdom = kingdomsInWar[i];
            Kingdom otherKingdom = this._kingdom2;
            RelationshipKingdom rel = this._kingdom1Rel;
            if (currKingdom.id == this._kingdom2.id) {
                otherKingdom = this._kingdom1;
                rel = this._kingdom2Rel;
            }

            if (rel.monthToMoveOnAfterRejection == MONTH.NONE
                && KingdomManager.Instance.GetRequestPeaceBetweenKingdoms(currKingdom, otherKingdom) == null) {

                int chanceToTriggerRequestPeace = 0;
                if (rel.kingdomWar.exhaustion >= 100) {
                    if (currKingdom.king.hostilityTrait == TRAIT.PACIFIST) {
                        chanceToTriggerRequestPeace = 4;
                    } else if (currKingdom.king.hostilityTrait == TRAIT.WARMONGER) {
                        chanceToTriggerRequestPeace = 2;
                    }
                } else if (rel.kingdomWar.exhaustion >= 75) {
                    if (currKingdom.king.hostilityTrait == TRAIT.PACIFIST) {
                        chanceToTriggerRequestPeace = 3;
                    } else if (currKingdom.king.hostilityTrait == TRAIT.WARMONGER) {
                        chanceToTriggerRequestPeace = 1;
                    }
                } else if (rel.kingdomWar.exhaustion >= 50) {
                    if (currKingdom.king.hostilityTrait == TRAIT.PACIFIST) {
                        chanceToTriggerRequestPeace = 2;
                    } else if (currKingdom.king.hostilityTrait == TRAIT.WARMONGER) {
                        chanceToTriggerRequestPeace = 0;
                    }
                }

                int chance = Random.Range(0, 100);
                if (chance < chanceToTriggerRequestPeace) {
                    this.CreateRequestPeaceEvent(currKingdom);
                }
            }
        }
    }

	internal void RequestPeace(Kingdom kingdomToRequest){
		this.CreateRequestPeaceEvent(kingdomToRequest);
	}
	internal void CreateCityWarPair(){
		if(this.warPair.kingdom1City == null || this.warPair.kingdom2City == null){
			List<HexTile> path = null;
			City kingdom1CityToBeAttacked = null;
			for (int i = 0; i < this.kingdom2.capitalCity.habitableTileDistance.Count; i++) {
				if(this.kingdom2.capitalCity.habitableTileDistance[i].hexTile.city != null && this.kingdom2.capitalCity.habitableTileDistance[i].hexTile.city.id != 0 && !this.kingdom2.capitalCity.habitableTileDistance[i].hexTile.city.isDead){
					if(this.kingdom2.capitalCity.habitableTileDistance[i].hexTile.city.kingdom.id == this.kingdom1.id){
						kingdom1CityToBeAttacked = this.kingdom2.capitalCity.habitableTileDistance [i].hexTile.city;
						break;
					}
				}
			}
			City kingdom2CityToBeAttacked = null;
			if (kingdom1CityToBeAttacked != null) {
				for (int i = 0; i < this.kingdom1.capitalCity.habitableTileDistance.Count; i++) {
					if (this.kingdom1.capitalCity.habitableTileDistance [i].hexTile.city != null && this.kingdom1.capitalCity.habitableTileDistance [i].hexTile.city.id != 0 && !this.kingdom1.capitalCity.habitableTileDistance [i].hexTile.city.isDead) {
						if (this.kingdom1.capitalCity.habitableTileDistance [i].hexTile.city.kingdom.id == this.kingdom2.id) {
							path = PathGenerator.Instance.GetPath (kingdom1CityToBeAttacked.hexTile, this.kingdom1.capitalCity.habitableTileDistance [i].hexTile, PATHFINDING_MODE.COMBAT);
							if (path != null) {
								kingdom2CityToBeAttacked = this.kingdom1.capitalCity.habitableTileDistance [i].hexTile.city;
								break;
							}
						}
					}
				}
			}


			if(kingdom1CityToBeAttacked != null && kingdom2CityToBeAttacked != null && path != null){
				kingdom1CityToBeAttacked.isUnderAttack = true;
				kingdom2CityToBeAttacked.isUnderAttack = true;
				this.warPair = new CityWarPair (kingdom1CityToBeAttacked, kingdom2CityToBeAttacked, path);
			}
		}
	}
	internal void UpdateWarPair(){
		this.warPair.DefaultValues ();
		CreateCityWarPair ();
	}
	private void Attack(){
		this.attackRate += 1;
		if((this.warPair.kingdom1City == null || this.warPair.kingdom1City.isDead) || (this.warPair.kingdom2City == null || this.warPair.kingdom2City.isDead) || this.warPair.isDone){
			if(this.warPair.kingdom1City == null || this.warPair.kingdom1City.isDead){
				this.ReplenishWavesKingdom2();
			}else if(this.warPair.kingdom2City == null || this.warPair.kingdom2City.isDead){
				this.ReplenishWavesKingdom1();
			}
			UpdateWarPair ();
			if(this.warPair.path == null){
				return;
			}
		}
		if(this.isInitialAttack){
			if(this.attackRate < KingdomManager.Instance.initialSpawnRate){
				return;
			}else{
				this.isInitialAttack = false;
			}
		}else{
			if(this.attackRate < this.warPair.spawnRate){
				return;
			}
		}
		this.attackRate = 0;
		if ((this.warPair.kingdom1City != null && !this.warPair.kingdom1City.isDead) && (this.warPair.kingdom2City != null && !this.warPair.kingdom2City.isDead)) {
			if(this.kingdom1Waves > 0){
				this.kingdom1Waves -= 1;
				this.warPair.kingdom1City.AttackCity (this.warPair.kingdom2City, this.warPair.path, this);
				ReinforcementKingdom2();
			}else if(this.kingdom2Waves > 0){
				this.kingdom2Waves -= 1;
				this.warPair.kingdom2City.AttackCity (this.warPair.kingdom1City, this.warPair.path, this);
				ReinforcementKingdom1();
			}else{
				this.ReplenishWavesKingdom1();
				this.ReplenishWavesKingdom2();
				if(this.kingdom1Waves <= 0 && this.kingdom2Waves <= 0){
                    //Peace 100% - kingdom1 will request peace while kingdom2 will accept peace 100%
                    EventCreator.Instance.CreateRequestPeace(kingdom1, kingdom2, true);
				}
			}
//				if(!this.kingdom1Attacked){
//					this.kingdom1Attacked = true;
//					this.warPair.kingdom1City.AttackCity (this.warPair.kingdom2City, this.warPair.path);
//				}else{
//					this.kingdom1Attacked = false;
//					this.warPair.kingdom2City.AttackCity (this.warPair.kingdom1City, this.warPair.path);
//				}
//			Reinforcement ();
		}
	}
	private void ReinforcementKingdom1(){
		List<City> safeCitiesKingdom1 = this.kingdom1.cities.Where (x => !x.isUnderAttack && !x.hasReinforced && x.hp >= 100).ToList (); 
		int chance = 0;
		int value = 0;
		int maxChanceKingdom1 = 100 + ((safeCitiesKingdom1.Count - 1) * 10);

		if(this.warPair.kingdom1City.hp != this.warPair.kingdom1City.maxHP){
			if(safeCitiesKingdom1 != null){
				for(int i = 0; i < safeCitiesKingdom1.Count; i++){
					chance = UnityEngine.Random.Range (0, maxChanceKingdom1);
					value = 1 * safeCitiesKingdom1 [i].ownedTiles.Count;
					if(chance < value){
						safeCitiesKingdom1 [i].hasReinforced = true;
						safeCitiesKingdom1 [i].ReinforceCity (this.warPair.kingdom1City);
					}
				}
			}
		}
	}
	private void ReinforcementKingdom2(){
		List<City> safeCitiesKingdom2 = this.kingdom2.cities.Where (x => !x.isUnderAttack && !x.hasReinforced && x.hp >= 100).ToList ();
		int chance = 0;
		int value = 0;
		int maxChanceKingdom2 = 100 + ((safeCitiesKingdom2.Count - 1) * 10);

		if(this.warPair.kingdom2City.hp != this.warPair.kingdom2City.maxHP){
			if(safeCitiesKingdom2 != null){
				for(int i = 0; i < safeCitiesKingdom2.Count; i++){
					chance = UnityEngine.Random.Range (0, maxChanceKingdom2);
					value = 1 * safeCitiesKingdom2 [i].ownedTiles.Count;
					if(chance < value){
						safeCitiesKingdom2 [i].hasReinforced = true;
						safeCitiesKingdom2 [i].ReinforceCity (this.warPair.kingdom2City);
					}
				}
			}
		}

	}
	private void UpdatePath(HexTile hexTile){
		if(this.warPair.path != null && this.warPair.path.Count > 0){
			if(this.warPair.path.Contains(hexTile)){
				this.warPair.UpdateSpawnRate();
			}
		}
	}
	#region Overrides
    internal override void DoneEvent() {
        base.DoneEvent();
        if (this._kingdom1.isDead) {
			GameEventWarWinner (this._kingdom2);
            Log titleLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "War", "kingdom_defeat");
			titleLog.AddToFillers(_kingdom1, _kingdom1.name, LOG_IDENTIFIER.KINGDOM_1);
			titleLog.AddToFillers(_kingdom2, _kingdom2.name, LOG_IDENTIFIER.KINGDOM_2);
        } else if (this._kingdom2.isDead) {
			GameEventWarWinner (this._kingdom1);
            Log titleLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "War", "kingdom_defeat");
			titleLog.AddToFillers(_kingdom2, _kingdom2.name, LOG_IDENTIFIER.KINGDOM_1);
			titleLog.AddToFillers(_kingdom1, _kingdom1.name, LOG_IDENTIFIER.KINGDOM_2);
        }
//        EventManager.Instance.onWeekEnd.RemoveListener(AttemptToRequestPeace);
		EventManager.Instance.onWeekEnd.RemoveListener (this.PerformAction);
		EventManager.Instance.onUpdatePath.RemoveListener (UpdatePath);
    }
	internal override void CancelEvent (){
		base.CancelEvent ();
        this.DeclarePeace();
	}
	#endregion
	internal void GameEventWarWinner(Kingdom winnerKingdom){
		if (this.gameEventTrigger != null && this.gameEventTrigger.isActive) {
			if (this.gameEventTrigger is SpouseAbduction) {
				((SpouseAbduction)this.gameEventTrigger).WarWinner (winnerKingdom);
			}
			GameEventTriggerWarResults ();
		}else{
			GameEventTriggerWarResults ();
		}
	}

	internal void GameEventTriggerWarResults(){
		this.gameEventTrigger = null;
	}

	private void ReplenishWavesKingdom1(){
		this.kingdom1Waves = this.kingdom1.kingdomTypeData.combatStats.waves - (this.kingdom1.GetNumberOfWars() + this.kingdom1.rebellions.Count);
	}
	private void ReplenishWavesKingdom2(){
		this.kingdom2Waves = this.kingdom2.kingdomTypeData.combatStats.waves - (this.kingdom2.GetNumberOfWars() + this.kingdom2.rebellions.Count);
	}
}
