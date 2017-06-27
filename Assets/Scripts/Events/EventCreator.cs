using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EventCreator: MonoBehaviour {
	public static EventCreator Instance;

	void Awake(){
		Instance = this;
	}
	internal Expansion CreateExpansionEvent(Kingdom kingdom){
		if(kingdom.capitalCity == null){
			return null;
		}
		HexTile hexTileToExpandTo = CityGenerator.Instance.GetNearestHabitableTile (kingdom.cities [0]);
		if(hexTileToExpandTo == null){
			return null;
		}
		Citizen expander = kingdom.capitalCity.CreateAgent (ROLE.EXPANDER, EVENT_TYPES.EXPANSION, hexTileToExpandTo, EventManager.Instance.eventDuration[EVENT_TYPES.EXPANSION]);
		if (expander != null) {
			Expansion expansion = new Expansion (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, expander, hexTileToExpandTo);
			expander.assignedRole.Initialize (expansion);
			return expansion;
		}
		return null;
	}

	internal Raid CreateRaidEvent(Kingdom firstKingdom, Kingdom secondKingdom){
		City city = null;
		if(secondKingdom.cities.Count > 0){
			city = secondKingdom.cities [UnityEngine.Random.Range (0, secondKingdom.cities.Count)];
		}
		if(city == null){
			return null;
		}
		Citizen raider = firstKingdom.capitalCity.CreateAgent (ROLE.RAIDER, EVENT_TYPES.RAID, city.hexTile, EventManager.Instance.eventDuration[EVENT_TYPES.RAID]);
		if(raider != null){
			Raid raid = new Raid(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, firstKingdom.king, city);
			raider.assignedRole.Initialize (raid);
			return raid;
		}
		return null;
	}
	internal BorderConflict CreateBorderConflictEvent(Kingdom firstKingdom, Kingdom secondKingdom){
		Citizen startedBy = firstKingdom.king;
		BorderConflict borderConflict = new BorderConflict(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, startedBy, firstKingdom, secondKingdom);
		return borderConflict;
	}
	internal DiplomaticCrisis CreateDiplomaticCrisisEvent(Kingdom firstKingdom, Kingdom secondKingdom){
		Citizen startedBy = secondKingdom.king;
		DiplomaticCrisis diplomaticCrisis = new DiplomaticCrisis(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, startedBy, secondKingdom, firstKingdom);
		return diplomaticCrisis;
	}
	internal JoinWar CreateJoinWarEvent(Kingdom kingdom, Kingdom friend, InvasionPlan invasionPlan){
		Citizen citizen = kingdom.capitalCity.CreateAgent (ROLE.ENVOY, EVENT_TYPES.JOIN_WAR_REQUEST, friend.capitalCity.hexTile, EventManager.Instance.eventDuration[EVENT_TYPES.JOIN_WAR_REQUEST]);
		Citizen citizenToPersuade = friend.king;
		if(citizen != null && citizenToPersuade != null){
			Envoy envoy = (Envoy)citizen.assignedRole;
			JoinWar joinWar = new JoinWar (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, invasionPlan.startedBy, 
				citizenToPersuade, envoy, invasionPlan.targetKingdom, invasionPlan);
			envoy.Initialize (joinWar);
			return joinWar;
		}
		return null;
	}
	internal StateVisit CreateStateVisitEvent(Kingdom firstKingdom, Kingdom secondKingdom){
		Citizen visitor = firstKingdom.capitalCity.CreateAgent (ROLE.ENVOY, EVENT_TYPES.STATE_VISIT, secondKingdom.capitalCity.hexTile, EventManager.Instance.eventDuration[EVENT_TYPES.STATE_VISIT]);
		if(visitor != null){
			Envoy envoy = (Envoy)visitor.assignedRole;
			StateVisit stateVisit = new StateVisit(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, firstKingdom.king, secondKingdom, envoy);
			envoy.Initialize (stateVisit);
			return stateVisit;
		}
		return null;
	}
    
    internal Trade CreateTradeEvent(City sourceCity, City targetCity) {
        Citizen trader = sourceCity.CreateAgent(ROLE.TRADER, EVENT_TYPES.TRADE, targetCity.hexTile, EventManager.Instance.eventDuration[EVENT_TYPES.TRADE]);
        if (trader != null) {
            Debug.Log("CREATED NEW TRADE EVENT: " + sourceCity.name + " towards " + targetCity.name);
            Trade tradeEvent = new Trade(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year,
                sourceCity.kingdom.king, sourceCity, targetCity, trader);
            trader.assignedRole.Initialize(tradeEvent);
            return tradeEvent;
        }

        return null;
    }

	internal Sabotage CreateSabotageEvent(Kingdom sourceKingdom, Kingdom targetKingdom, GameEvent eventToSabotage, int remainingDays){
		Citizen citizen = sourceKingdom.capitalCity.CreateAgent (ROLE.ENVOY, EVENT_TYPES.SABOTAGE, targetKingdom.capitalCity.hexTile, remainingDays);
		if(citizen != null){
			Envoy envoy = (Envoy)citizen.assignedRole;
			Sabotage sabotage = new Sabotage(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year,
				sourceKingdom.king, envoy, eventToSabotage);
			envoy.Initialize (sabotage);
		}
		return null;
	}

	internal Assassination CreateAssassinationEvent(Kingdom sourceKingdom, Citizen targetCitizen, GameEvent gameEventTrigger, int remainingDays){
		HexTile targetLocation = targetCitizen.currentLocation;
		if(targetCitizen.assignedRole != null){
			if(targetCitizen.assignedRole.targetLocation != null){
				targetLocation = targetCitizen.assignedRole.targetLocation;
			}
		}
		Citizen citizen = sourceKingdom.capitalCity.CreateAgent (ROLE.SPY, EVENT_TYPES.SABOTAGE, targetLocation, remainingDays);
		if(citizen != null){
			Spy spy = (Spy)citizen.assignedRole;
			Assassination assassination = new Assassination(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year,
				sourceKingdom.king, targetCitizen, spy, gameEventTrigger);
			spy.Initialize (assassination);
		}
		return null;
	}

	internal AttackCity CreateAttackCityEvent(City sourceCity, City targetCity, List<HexTile> path, GameEvent gameEvent, bool isRebel = false){
		Citizen citizen = sourceCity.CreateGeneralForCombat(path, targetCity.hexTile);
		if(citizen != null){
			General general = (General)citizen.assignedRole;
			AttackCity attackCity = new AttackCity(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year,
				citizen, general, targetCity, gameEvent);
			general.Initialize (attackCity);
			general.isRebel = isRebel;
		}
		return null;
	}
	internal RequestPeace CreateRequestPeace(Kingdom kingdomToRequest, Kingdom targetKingdom) {
    	Citizen citizen = kingdomToRequest.capitalCity.CreateAgent(ROLE.ENVOY, EVENT_TYPES.REQUEST_PEACE, targetKingdom.capitalCity.hexTile, EventManager.Instance.eventDuration[EVENT_TYPES.REQUEST_PEACE]);
		if (citizen != null) {
        	RequestPeace requestPeace = new RequestPeace(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year,
				kingdomToRequest.king, (Envoy)citizen.assignedRole, targetKingdom);
			citizen.assignedRole.Initialize(requestPeace);
        	return requestPeace;
    	}
    	return null;
	}

	internal Reinforcement CreateReinforcementEvent(City sourceCity, City targetCity, bool isRebel = false){
//		City targetCity = sourceKingdom.GetReceiverCityForReinforcement ();
//		if(targetCity == null){
//			return null;
//		}
//		City sourceCity = sourceKingdom.GetSenderCityForReinforcement ();
//		if(sourceCity == null){
//			return null;
//		}
		Citizen citizen = sourceCity.CreateAgent(ROLE.REINFORCER, EVENT_TYPES.REINFORCEMENT, targetCity.hexTile, EventManager.Instance.eventDuration[EVENT_TYPES.REINFORCEMENT]);
		if(citizen != null){
			Reinforcer reinforcer = (Reinforcer)citizen.assignedRole;
			Reinforcement reinforcement = new Reinforcement(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year,
				citizen, reinforcer, targetCity, sourceCity);
			reinforcer.Initialize(reinforcement);
			reinforcer.isRebel = isRebel;
			return reinforcement;
		}
		return null;
	}

	internal Secession CreateSecessionEvent(Citizen governor){
		Secession secession = new Secession(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, governor);
		return secession;
	}

	internal Riot CreateRiotEvent(Kingdom sourceKingdom){
		Citizen rebel = sourceKingdom.capitalCity.CreateAgent(ROLE.REBEL, EVENT_TYPES.RIOT, sourceKingdom.capitalCity.hexTile, EventManager.Instance.eventDuration[EVENT_TYPES.RIOT]);
		if(rebel != null){
			Riot riot = new Riot(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, rebel);
			rebel.assignedRole.Initialize (riot);
			return riot;
		}
		return null;
	}
	internal Rebellion CreateRebellionEvent(Kingdom sourceKingdom){
		Citizen rebel = sourceKingdom.capitalCity.CreateAgent(ROLE.REBEL, EVENT_TYPES.REBELLION, sourceKingdom.capitalCity.hexTile, EventManager.Instance.eventDuration[EVENT_TYPES.REBELLION]);
		if(rebel != null){
			Rebellion rebellion = new Rebellion(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, rebel);
			rebel.assignedRole.Initialize (rebellion);
			return rebellion;
		}
		return null;
	}

	internal Plague CreatePlagueEvent(){
		Debug.Log ("CREATING PLAGUE EVENT");
		List<Kingdom> targetKingdoms = KingdomManager.Instance.allKingdoms.Where (x => x.unrest >= 0 && x.cities.FirstOrDefault (y => y.ownedTiles.Count >= 2) != null).ToList();
		if(targetKingdoms != null && targetKingdoms.Count > 0){
			Kingdom plaguedKingdom = targetKingdoms [UnityEngine.Random.Range (0, targetKingdoms.Count)];
			List<City> targetCities = plaguedKingdom.cities.Where (x => x.ownedTiles.Count >= 2).ToList();
			City plaguedCity = targetCities [UnityEngine.Random.Range (0, targetCities.Count)];
			if(plaguedCity != null){
				Plague plague = new Plague(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, null, plaguedCity);
				return plague;
			}
		}
		return null;
	}
	internal Plague CreatePlagueEvent(Kingdom kingdom){
		Debug.Log ("CREATING FORCE PLAGUE EVENT");
		List<City> targetCities = kingdom.cities.Where (x => x.structures.Count > 0).ToList();
		City plaguedCity = targetCities [UnityEngine.Random.Range (0, targetCities.Count)];
		if(plaguedCity != null){
			Plague plague = new Plague(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, null, plaguedCity);
			return plague;
		}
		return null;
	}

	internal ScourgeCity CreateScourgeCityEvent(Kingdom sourceKingdom, Kingdom targetKingdom){
		City targetCity = targetKingdom.cities [UnityEngine.Random.Range (0, targetKingdom.cities.Count)];
		Citizen citizen = sourceKingdom.capitalCity.CreateAgent(ROLE.SCOURGE, EVENT_TYPES.SCOURGE_CITY, targetCity.hexTile, EventManager.Instance.eventDuration[EVENT_TYPES.SCOURGE_CITY]);

		if(citizen != null && targetCity != null){
			Scourge scourge = (Scourge)citizen.assignedRole;
			ScourgeCity scourgeCity = new ScourgeCity(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, sourceKingdom.king, scourge, targetCity);
			scourge.Initialize (scourgeCity);
			return scourgeCity;
		}
		return null;
	}

	internal BoonOfPower CreateBoonOfPowerEvent(HexTile targetHextile){
		if(targetHextile.isOccupied || targetHextile.isBorder || targetHextile.gameEventInTile != null){
			return null;
		}
		BoonOfPower boonOfPower = new BoonOfPower (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, null, targetHextile);
		WorldEventManager.Instance.ResetCurrentInterveneEvent();
		return boonOfPower;
	}

	internal Provocation CreateProvocationEvent(Kingdom sourceKingdom, Kingdom targetKingdom){
		Citizen citizen = sourceKingdom.capitalCity.CreateAgent(ROLE.PROVOKER, EVENT_TYPES.PROVOCATION, targetKingdom.capitalCity.hexTile, EventManager.Instance.eventDuration[EVENT_TYPES.PROVOCATION]);
		if(citizen != null){
			Provoker provoker = (Provoker)citizen.assignedRole;
			Provocation provocation = new Provocation(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, sourceKingdom.king, sourceKingdom, targetKingdom, provoker);
			provoker.Initialize (provocation);
			return provocation;
		}
		return null;
	}

	internal Evangelism CreateEvangelismEvent(Kingdom sourceKingdom, Kingdom targetKingdom){
		Citizen citizen = sourceKingdom.capitalCity.CreateAgent(ROLE.MISSIONARY, EVENT_TYPES.EVANGELISM, targetKingdom.capitalCity.hexTile, EventManager.Instance.eventDuration[EVENT_TYPES.EVANGELISM]);
		if(citizen != null){
			Missionary missionary = (Missionary)citizen.assignedRole;
			Evangelism evangelism = new Evangelism(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, sourceKingdom.king, sourceKingdom, targetKingdom, missionary);
			missionary.Initialize (evangelism);
			return evangelism;
		}
		return null;
	}
}
