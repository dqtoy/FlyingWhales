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
		if(kingdom.isLockedDown){
			return null;
		}
		if(kingdom.capitalCity == null){
			return null;
		}
        HexTile hexTileToExpandTo = CityGenerator.Instance.GetExpandableTileForKingdom(kingdom);
        
        if (hexTileToExpandTo == null){
            Debug.Log(kingdom.name + " cannot find a region to expand to!");
			return null;
		}

        Region regionToExpandTo = hexTileToExpandTo.region;
        float nearestDistance = 9999f;
        HexTile origin = null;
        //Get nearest region from hexTileToExpandTo
        for (int i = 0; i < regionToExpandTo.adjacentRegions.Count; i++) {
            Region currAdjacentRegion = regionToExpandTo.adjacentRegions[i];
            float distance = hexTileToExpandTo.GetDistanceTo(currAdjacentRegion.centerOfMass);
            if(currAdjacentRegion.occupant != null && currAdjacentRegion.occupant.kingdom == kingdom && distance < nearestDistance) {
                origin = currAdjacentRegion.centerOfMass;
                nearestDistance = distance;
            }
        }
        if(origin == null) {
            throw new System.Exception("Could not find origin tile for expansion of " + kingdom.name + " to " + hexTileToExpandTo.name);
        }
        Citizen expander = origin.city.CreateNewAgent (ROLE.EXPANDER, EVENT_TYPES.EXPANSION, hexTileToExpandTo);
		if (expander != null) {
			Expansion expansion = new Expansion (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, expander, hexTileToExpandTo);
			expander.assignedRole.Initialize (expansion);
			return expansion;
		}
		return null;
	}

	internal Raid CreateRaidEvent(Kingdom firstKingdom, Kingdom secondKingdom){
		if(firstKingdom.isLockedDown || secondKingdom.isLockedDown){
			return null;
		}
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
		if(firstKingdom.isLockedDown || secondKingdom.isLockedDown){
			return null;
		}
		Citizen startedBy = firstKingdom.king;
		BorderConflict borderConflict = new BorderConflict(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, startedBy, firstKingdom, secondKingdom);
		return borderConflict;
	}
	internal DiplomaticCrisis CreateDiplomaticCrisisEvent(Kingdom firstKingdom, Kingdom secondKingdom){
		if(firstKingdom.isLockedDown || secondKingdom.isLockedDown){
			return null;
		}
		Citizen startedBy = secondKingdom.king;
		DiplomaticCrisis diplomaticCrisis = new DiplomaticCrisis(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, startedBy, secondKingdom, firstKingdom);
		return diplomaticCrisis;
	}
	internal JoinWar CreateJoinWarEvent(Kingdom kingdom, Kingdom friend, InvasionPlan invasionPlan){
		if(kingdom.isLockedDown || friend.isLockedDown){
			return null;
		}
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
		if(firstKingdom.isLockedDown || secondKingdom.isLockedDown){
			return null;
		}
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
		if(sourceCity.kingdom.isLockedDown || targetCity.kingdom.isLockedDown){
			return null;
		}
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
		if(sourceKingdom.isLockedDown || targetKingdom.isLockedDown){
			return null;
		}
		Citizen citizen = sourceKingdom.capitalCity.CreateAgent (ROLE.ENVOY, EVENT_TYPES.SABOTAGE, targetKingdom.capitalCity.hexTile, remainingDays);
		if(citizen != null){
			Envoy envoy = (Envoy)citizen.assignedRole;
			Sabotage sabotage = new Sabotage(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year,
				sourceKingdom.king, envoy, eventToSabotage);
			envoy.Initialize (sabotage);
		}
		return null;
	}

	internal Assassination CreateAssassinationEvent(Kingdom sourceKingdom, Citizen targetCitizen, GameEvent gameEventTrigger, int remainingDays, ASSASSINATION_TRIGGER_REASONS assassinationReasons){
		if(sourceKingdom.isLockedDown || targetCitizen.city.kingdom.isLockedDown){
			return null;
		}
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
				sourceKingdom.king, targetCitizen, spy, gameEventTrigger, assassinationReasons);
			spy.Initialize (assassination);
		}
		return null;
	}

	internal AttackCity CreateAttackCityEvent(City sourceCity, City targetCity, List<HexTile> path, GameEvent gameEvent, bool isRebel = false){
		if(sourceCity.kingdom.isLockedDown || targetCity.kingdom.isLockedDown){
			return null;
		}
		Citizen citizen = sourceCity.CreateGeneralForCombat(path, targetCity.hexTile, isRebel);
		if(citizen != null){

			General general = (General)citizen.assignedRole;
			AttackCity attackCity = new AttackCity(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year,
				citizen, general, sourceCity, targetCity, gameEvent);
			general.Initialize (attackCity);
			general.isRebel = isRebel;
		}
		return null;
	}
	internal AttackLair CreateAttackLairEvent(City sourceCity, HexTile targetHextile, List<HexTile> path, GameEvent gameEvent, bool isRebel = false){
		if(sourceCity.kingdom.isLockedDown){
			return null;
		}
		Citizen citizen = sourceCity.CreateGeneralForLair(path, targetHextile);
		if(citizen != null){
			General general = (General)citizen.assignedRole;
			AttackLair attackLair = new AttackLair(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year,
				citizen, general, targetHextile);
			general.Initialize (attackLair);
			general.isRebel = isRebel;
		}
		return null;
	}

	internal RequestPeace CreateRequestPeace(Kingdom kingdomToRequest, Kingdom targetKingdom, bool isSureAccept = false) {
		if(kingdomToRequest.isLockedDown || targetKingdom.isLockedDown){
			return null;
		}
    	Citizen citizen = kingdomToRequest.capitalCity.CreateAgent(ROLE.ENVOY, EVENT_TYPES.REQUEST_PEACE, targetKingdom.capitalCity.hexTile, EventManager.Instance.eventDuration[EVENT_TYPES.REQUEST_PEACE]);
		if (citizen != null) {
        	RequestPeace requestPeace = new RequestPeace(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year,
				kingdomToRequest.king, (Envoy)citizen.assignedRole, targetKingdom, isSureAccept);
			citizen.assignedRole.Initialize(requestPeace);
        	return requestPeace;
    	}
    	return null;
	}

	internal Reinforcement CreateReinforcementEvent(City sourceCity, City targetCity, int amount = -1, Wars war = null, bool isRebel = false){
//		City targetCity = sourceKingdom.GetReceiverCityForReinforcement ();
//		if(targetCity == null){
//			return null;
//		}
//		City sourceCity = sourceKingdom.GetSenderCityForReinforcement ();
//		if(sourceCity == null){
//			return null;
//		}
		if(sourceCity.kingdom.isLockedDown || targetCity.kingdom.isLockedDown){
			return null;
		}
		Citizen citizen = sourceCity.CreateAgent(ROLE.REINFORCER, EVENT_TYPES.REINFORCEMENT, targetCity.hexTile, EventManager.Instance.eventDuration[EVENT_TYPES.REINFORCEMENT]);
		if(citizen != null){
			Reinforcer reinforcer = (Reinforcer)citizen.assignedRole;
			Reinforcement reinforcement = new Reinforcement(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year,
				citizen, reinforcer, targetCity, sourceCity, amount, war);
			reinforcer.Initialize(reinforcement);
			reinforcer.isRebel = isRebel;
			return reinforcement;
		}
		return null;
	}

	internal Secession CreateSecessionEvent(Citizen governor){
		if(governor.city.kingdom.isLockedDown){
			return null;
		}
		Secession secession = new Secession(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, governor);
		return secession;
	}

	internal Riot CreateRiotEvent(Kingdom sourceKingdom){
		if(sourceKingdom.isLockedDown){
			return null;
		}
		Citizen rebel = sourceKingdom.capitalCity.CreateAgent(ROLE.REBEL, EVENT_TYPES.RIOT, sourceKingdom.capitalCity.hexTile, EventManager.Instance.eventDuration[EVENT_TYPES.RIOT]);
		if(rebel != null){
			Riot riot = new Riot(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, rebel);
			rebel.assignedRole.Initialize (riot);
			return riot;
		}
		return null;
	}
	internal Rebellions CreateRebellionEvent(Kingdom sourceKingdom, Citizen provokerKing){
		if(sourceKingdom.isLockedDown){
			return null;
		}
		Citizen rebel = sourceKingdom.capitalCity.CreateAgent(ROLE.REBEL, EVENT_TYPES.REBELLION, sourceKingdom.capitalCity.hexTile, EventManager.Instance.eventDuration[EVENT_TYPES.REBELLION]);
		if(rebel != null){
			Rebellions rebellion = new Rebellions(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, rebel, provokerKing);
			rebel.assignedRole.Initialize (rebellion);
			return rebellion;
		}
		return null;
	}

	internal Plague CreatePlagueEvent(){
		Debug.Log ("CREATING PLAGUE EVENT");
		List<Kingdom> targetKingdoms = KingdomManager.Instance.allKingdoms.Where (x => x.stability >= 0 && x.cities.FirstOrDefault (y => y.ownedTiles.Count >= 2) != null).ToList();
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
		if(sourceKingdom.isLockedDown || targetKingdom.isLockedDown){
			return null;
		}
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
//		if(targetHextile.isOccupied || targetHextile.isBorder || targetHextile.gameEventInTile != null){
//			return null;
//		}
		BoonOfPower boonOfPower = new BoonOfPower (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, null, targetHextile);
		WorldEventManager.Instance.ResetCurrentInterveneEvent();
		return boonOfPower;
	}

	internal Provocation CreateProvocationEvent(Kingdom sourceKingdom, Kingdom targetKingdom){
		if(sourceKingdom.isLockedDown || targetKingdom.isLockedDown){
			return null;
		}
		Citizen citizen = sourceKingdom.capitalCity.CreateNewAgent(ROLE.PROVOKER, EVENT_TYPES.PROVOCATION, targetKingdom.capitalCity.hexTile);
		if(citizen != null){
			Provoker provoker = (Provoker)citizen.assignedRole;
			Provocation provocation = new Provocation(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, sourceKingdom.king, sourceKingdom, targetKingdom, provoker);
			provoker.Initialize (provocation);
			return provocation;
		}
		return null;
	}

	internal Evangelism CreateEvangelismEvent(Kingdom sourceKingdom, Kingdom targetKingdom){
		if(sourceKingdom.isLockedDown || targetKingdom.isLockedDown){
			return null;
		}
		Citizen citizen = sourceKingdom.capitalCity.CreateAgent(ROLE.MISSIONARY, EVENT_TYPES.EVANGELISM, targetKingdom.capitalCity.hexTile, EventManager.Instance.eventDuration[EVENT_TYPES.EVANGELISM]);
		if(citizen != null){
			Missionary missionary = (Missionary)citizen.assignedRole;
			Evangelism evangelism = new Evangelism(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, sourceKingdom.king, sourceKingdom, targetKingdom, missionary);
			missionary.Initialize (evangelism);
			return evangelism;
		}
		return null;
	}

	internal SpouseAbduction CreateSpouseAbductionEvent(Citizen abductorKing, Citizen targetKing){
		if(abductorKing.city.kingdom.isLockedDown || targetKing.city.kingdom.isLockedDown){
			return null;
		}
		Debug.Log ("Creating SPOUSE ABDUCTION: " + abductorKing.name + " wants to abduct the spouse of " + targetKing.name);
		SpouseAbduction spouseAbduction = new SpouseAbduction(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, abductorKing, targetKing);
		return spouseAbduction;
	}

	internal FirstAndKeystone CreateFirstAndKeystoneEvent(HexTile targetHextile){
//		if(targetHextile.isOccupied || targetHextile.isBorder || targetHextile.gameEventInTile != null){
//			return null;
//		}
		FirstAndKeystone firstAndKeystone = new FirstAndKeystone (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, null, targetHextile);
		return firstAndKeystone;
	}

	internal Rumor CreateRumorEvent(Citizen startedBy, Kingdom rumorKingdom, Kingdom targetKingdom){
		if(startedBy.city.kingdom.isLockedDown || rumorKingdom.isLockedDown || targetKingdom.isLockedDown){
			return null;
		}
		Rumor rumor = new Rumor (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, startedBy, rumorKingdom, targetKingdom);
		return rumor;
	}
	internal SlavesMerchant CreateSlavesMerchantEvent(Citizen startedBy){
		if(startedBy.city.kingdom.isLockedDown){
			return null;
		}
		SlavesMerchant slavesMerchant = new SlavesMerchant (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, startedBy);
		return slavesMerchant;
	}
	internal HiddenHistoryBook CreateHiddenHistoryBookEvent(Citizen startedBy){
		if(startedBy.city.kingdom.isLockedDown){
			return null;
		}
		HiddenHistoryBook hiddenHistoryBook = new HiddenHistoryBook (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, startedBy);
		return hiddenHistoryBook;
	}

    internal Hypnotism CreateHypnotismEvent(Kingdom sourceKingdom, Kingdom targetKingdom) {
		if(sourceKingdom.isLockedDown || targetKingdom.isLockedDown){
			return null;
		}
        Citizen citizen = sourceKingdom.capitalCity.CreateAgent(ROLE.WITCH, EVENT_TYPES.HYPNOTISM, targetKingdom.capitalCity.hexTile, EventManager.Instance.eventDuration[EVENT_TYPES.HYPNOTISM]);
        if (citizen != null) {
            Witch witch = (Witch)citizen.assignedRole;
            Hypnotism hypnotism = new Hypnotism(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, sourceKingdom.king, witch, sourceKingdom, targetKingdom);
            witch.Initialize(hypnotism);
            return hypnotism;
        }
        return null;
    }

    internal KingdomHoliday CreateKingdomHolidayEvent(Kingdom sourceKingdom) {
		if(sourceKingdom.isLockedDown){
			return null;
		}
        KingdomHoliday kingdomHoliday = new KingdomHoliday(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, sourceKingdom.king, sourceKingdom);
        return kingdomHoliday;
    }

    internal DevelopWeapons CreateDevelopWeaponsEvent(Kingdom sourceKingdom) {
        DevelopWeapons developWeapons = new DevelopWeapons(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, sourceKingdom.king, sourceKingdom);
        //WorldEventManager.Instance.ResetCurrentInterveneEvent();
        return developWeapons;
    }

    internal KingsCouncil CreateKingsCouncilEvent(Kingdom sourceKingdom) {
		if(sourceKingdom.isLockedDown){
			return null;
		}
        KingsCouncil kingsCouncil = new KingsCouncil(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, sourceKingdom.king, sourceKingdom);
        return kingsCouncil;
    }
	internal SerumOfAlacrity CreateSerumOfAlacrityEvent(Citizen startedBy) {
		if(startedBy.city.kingdom.isLockedDown){
			return null;
		}
		SerumOfAlacrity serumOfAlacrity = new SerumOfAlacrity(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, startedBy);
		return serumOfAlacrity;
	}
	internal AltarOfBlessing CreateAltarOfBlessingEvent(HexTile targetHextile){
		AltarOfBlessing altarOfBlessing = new AltarOfBlessing (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, null, targetHextile);
		WorldEventManager.Instance.ResetCurrentInterveneEvent();
		return altarOfBlessing;
	}

    internal Adventure CreateAdventureEvent(Kingdom sourceKingdom) {
		if(sourceKingdom.isLockedDown){
			return null;
		}
		if (sourceKingdom.HasActiveEvent(EVENT_TYPES.ADVENTURE)) {
            return null;
        }
        List<HexTile> tilesToChooseFrom = sourceKingdom.capitalCity.hexTile.AvatarTiles.ToList();
        HexTile targetTile = tilesToChooseFrom[Random.Range(0, tilesToChooseFrom.Count)];
        if(targetTile != null) {
            Citizen adventurer = sourceKingdom.capitalCity.CreateAgent(ROLE.ADVENTURER, EVENT_TYPES.ADVENTURE, targetTile, EventManager.Instance.eventDuration[EVENT_TYPES.ADVENTURE]);
            if(adventurer != null) {
                Adventure adventureEvent = new Adventure(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, sourceKingdom.king, (Adventurer)adventurer.assignedRole);
                adventurer.assignedRole.Initialize(adventureEvent);
                return adventureEvent;
            }
        }
        return null;
    }

	internal SendReliefGoods CreateSendReliefGoodsEvent(Kingdom senderKingdom, Kingdom receiverKingdom, GameEvent gameEvent, int reliefGoods){
		if(senderKingdom.isLockedDown || senderKingdom.isLockedDown){
			return null;
		}
		Citizen citizen = senderKingdom.capitalCity.CreateAgent(ROLE.RELIEVER, EVENT_TYPES.SEND_RELIEF_GOODS, receiverKingdom.capitalCity.hexTile, -1);
		if(citizen != null){
			Reliever reliever = (Reliever)citizen.assignedRole;
			SendReliefGoods sendReliefGoods = new SendReliefGoods(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, senderKingdom.king, receiverKingdom, reliever, gameEvent, reliefGoods);
			reliever.Initialize(sendReliefGoods);
			return sendReliefGoods;
		}
		return null;
	}

	internal GreatStorm CreateGreatStormEvent(Kingdom sourceKingdom){
		if(sourceKingdom.isLockedDown){
			return null;
		}
		GreatStorm greatStorm = new GreatStorm (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, null, sourceKingdom);
		return greatStorm;
	}

    internal AncientRuin CreateAncientRuinEvent(HexTile targetHextile) {
        AncientRuin ancientRuin = new AncientRuin(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, null, targetHextile);
        WorldEventManager.Instance.ResetCurrentInterveneEvent();
        return ancientRuin;
    }
	internal HuntLair CreateHuntLairEvent(Kingdom sourceKingdom){
		if(sourceKingdom.isLockedDown){
			return null;
		}
		if (sourceKingdom.GetActiveEventsOfTypeCount(EVENT_TYPES.HUNT_LAIR) >= sourceKingdom.cities.Count) {
			return null;
		}
		HexTile chosenLairTile = null;
		Lair lair = null;
		List<HexTile> lairTiles = new List<HexTile>();

		lairTiles = sourceKingdom.fogOfWarDict [FOG_OF_WAR_STATE.VISIBLE].Where(x => x.lair != null).ToList();
		if(lairTiles == null || lairTiles.Count <= 0){
			lairTiles = sourceKingdom.fogOfWarDict [FOG_OF_WAR_STATE.SEEN].Where(x => x.lair != null).ToList();
		}


		if(lairTiles != null && lairTiles.Count > 0){
			lairTiles = lairTiles.OrderBy (x => PathGenerator.Instance.GetDistanceBetweenTwoTiles (sourceKingdom.capitalCity.hexTile, x)).ToList();
			chosenLairTile = lairTiles [0];
			lair = chosenLairTile.lair;
		}
		Citizen citizen = sourceKingdom.capitalCity.CreateAgent(ROLE.RANGER, EVENT_TYPES.HUNT_LAIR, chosenLairTile, EventManager.Instance.eventDuration[EVENT_TYPES.HUNT_LAIR]);
		if(citizen != null){
			Ranger ranger = (Ranger)citizen.assignedRole;
			HuntLair huntLair = new HuntLair(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year,
				sourceKingdom.king, ranger, lair);
			ranger.Initialize (huntLair);
            return huntLair;
		}
		return null;
	}

    internal MilitaryAllianceOffer CreateMilitaryAllianceOffer(Kingdom sourceKingdom, Kingdom targetKingdom) {
        Citizen agent = sourceKingdom.capitalCity.CreateNewAgent(ROLE.MILITARY_ALLIANCE_OFFICER, 
            EVENT_TYPES.MILITARY_ALLIANCE_OFFER, targetKingdom.capitalCity.hexTile);

        KingdomRelationship sourceRel = sourceKingdom.GetRelationshipWithKingdom(targetKingdom);
        if(agent != null && sourceRel.currentActiveMilitaryAllianceOffer == null) {
            MilitaryAllianceOffer mao = new MilitaryAllianceOffer(GameManager.Instance.days, GameManager.Instance.month, 
                GameManager.Instance.year, sourceKingdom.king, sourceKingdom, targetKingdom);
            agent.assignedRole.Initialize(mao);
            return mao;
        }
        return null;
    }
	internal MutualDefenseTreaty CreateMutualDefenseTreatyEvent(Kingdom sourceKingdom, Kingdom targetKingdom){
        KingdomRelationship sourceRel = sourceKingdom.GetRelationshipWithKingdom(targetKingdom);
        if (sourceRel.currentActiveDefenseTreatyOffer == null) {
            MutualDefenseTreaty mutualDefenseTreaty = new MutualDefenseTreaty(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, sourceKingdom.king, targetKingdom);
            return mutualDefenseTreaty;
        }
        
        return null;
	}
	internal Tribute CreateTributeEvent(Kingdom sourceKingdom, Kingdom targetKingdom){
		Tribute tribute = new Tribute(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, sourceKingdom.king, targetKingdom);
		return tribute;
	}
	internal Instigation CreateInstigationEvent(Kingdom sourceKingdom, Kingdom instigatedKingdom, Kingdom targetKingdom){
		Instigation instigation = new Instigation(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, sourceKingdom.king, instigatedKingdom, targetKingdom);
		return instigation;
	}
	internal Wars CreateWarEvent(Kingdom sourceKingdom, Kingdom targetKingdom){
		Wars war = new Wars(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, sourceKingdom.king, sourceKingdom, targetKingdom);
		return war;
	}
	//-------------------------------------------- PLAYER EVENTS ----------------------------------------------------//

	internal KingdomDiscovery CreateKingdomDiscoveryEvent(Kingdom sourceKingdom, Kingdom targetKingdom){
		KingdomDiscovery kingdomDiscovery = new KingdomDiscovery (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, sourceKingdom.king, targetKingdom);
		return kingdomDiscovery;
	}

	internal Crime CreateCrimeEvent(Kingdom sourceKingdom, CrimeData crimeData){
		Crime crime = new Crime (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, sourceKingdom.king, crimeData);
		return crime;
	}

}
