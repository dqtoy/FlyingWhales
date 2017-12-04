using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EventCreator: MonoBehaviour {
	public static EventCreator Instance;

	void Awake(){
		Instance = this;
	}
	internal Expansion CreateExpansionEvent(Kingdom kingdom, HexTile hexTileToExpandTo = null) {
		if(kingdom.isLockedDown){
			return null;
		}
		if(kingdom.capitalCity == null){
			return null;
		}

        if(hexTileToExpandTo == null) {
            hexTileToExpandTo = CityGenerator.Instance.GetExpandableTileForKingdom(kingdom);
        }
        
        if (hexTileToExpandTo == null){
			Debug.Log(kingdom.name + " cannot find a region to expand to!");
			return null;
		}

        Region regionToExpandTo = hexTileToExpandTo.region;
        float nearestDistance = 9999f;
        HexTile origin = null;
        //Get nearest region from hexTileToExpandTo
        for (int i = 0; i < regionToExpandTo.connections.Count; i++) {
			if(regionToExpandTo.connections[i] is Region){
				Region currAdjacentRegion = (Region)regionToExpandTo.connections[i];
				float distance = hexTileToExpandTo.GetDistanceTo(currAdjacentRegion.centerOfMass);
				if(currAdjacentRegion.occupant != null && currAdjacentRegion.occupant.kingdom == kingdom && distance < nearestDistance && currAdjacentRegion.occupant.population > 50) {
					origin = currAdjacentRegion.occupant.hexTile;
					nearestDistance = distance;
				}
			}
        }
        if(origin == null) {
			Debug.Log("Could not find origin tile for expansion of " + kingdom.name + " to " + hexTileToExpandTo.name);
			return null;
        }
        Citizen expander = origin.city.CreateNewAgent (ROLE.EXPANDER, hexTileToExpandTo);
		if (expander != null) {
			Expansion expansion = new Expansion (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, expander, hexTileToExpandTo);
			expander.assignedRole.Initialize (expansion);
			return expansion;
		}
		return null;
	}
	internal SendResource CreateSendResourceEvent(int foodAmount, int materialAmount, int oreAmount, RESOURCE_TYPE resourceType, RESOURCE resource, HexTile sourceLocation, HexTile targetLocation, City sourceCity, List<HexTile> path = null){
		Citizen caravan = sourceCity.CreateNewAgent (ROLE.CARAVAN, targetLocation, sourceLocation);
		if(caravan != null){
			SendResource sendResource = new SendResource (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, caravan, foodAmount, materialAmount, oreAmount, resourceType, resource);
			caravan.assignedRole.Initialize (sendResource);
			if (path == null) {
				caravan.assignedRole.citizenAvatar.CreatePath (PATHFINDING_MODE.POINT_TO_POINT);
			} else {
				if(path.Count > 0){
					caravan.assignedRole.path = path;
					caravan.assignedRole.citizenAvatar.StartMoving ();
				}else{
					caravan.assignedRole.citizenAvatar.CancelEventInvolvedIn ();
				}
			}

			return sendResource;
		}
		return null;
	}
	internal Riot CreateRiotEvent(Kingdom sourceKingdom){
		Riot riot = new Riot(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, null, sourceKingdom);
		return riot;
	}
    internal RiotSettlement CreateRiotSettlementEvent(Kingdom sourceKingdom) {
        RiotSettlement riotSettlement = new RiotSettlement(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, null, sourceKingdom);
        return riotSettlement;
    }
    internal Plague CreatePlagueEvent(Kingdom infectedKingdom, bool isResetStability = true) {
        Plague newPlague = new Plague(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, null, infectedKingdom, isResetStability);
        return newPlague;
    }
    internal Regression CreateRegressionEvent(Kingdom sourceKingdom) {
        Regression regression = new Regression(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, null, sourceKingdom);
        return regression;
    }
	internal ReinforceCity CreateReinforceCityEvent(City sourceCity, City targetCity, int soldiers) {
		Citizen citizen = sourceCity.CreateNewAgent (ROLE.GENERAL, targetCity.hexTile);
		if(citizen != null){
			General general	= (General)citizen.assignedRole;
			sourceCity.AdjustSoldiers (-soldiers);
			ReinforceCity reinforceCity = new ReinforceCity(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, general.citizen);
			general.Initialize (reinforceCity);
			general.SetSoldiers (soldiers);
			general.citizenAvatar.CreatePath (PATHFINDING_MODE.MAJOR_ROADS_ONLY_KINGDOM);
			return reinforceCity;
		}
		return null;
	}
	internal AttackCity CreateAttackCityEvent(City sourceCity, City targetCity, Battle battle, int soldiers) {
		Citizen citizen = sourceCity.CreateNewAgent (ROLE.GENERAL, targetCity.hexTile);
		if(citizen != null){
			General general	= (General)citizen.assignedRole;
			general.isIdle = true;
			sourceCity.AdjustSoldiers (-soldiers);
			AttackCity attackCity = new AttackCity(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, general.citizen, battle, targetCity);
			general.Initialize (attackCity);
			general.SetSoldiers (soldiers);
			general.citizenAvatar.CreatePath (PATHFINDING_MODE.MAJOR_ROADS);
			return attackCity;
		}
		return null;
	}
	internal DefendCity CreateDefendCityEvent(City sourceCity, City targetCity, Battle battle, int soldiers) {
		Citizen citizen = sourceCity.CreateNewAgent (ROLE.GENERAL, targetCity.hexTile);
		if(citizen != null){
			General general	= (General)citizen.assignedRole;
			general.isIdle = true;
			sourceCity.AdjustSoldiers (-soldiers);
			DefendCity defendCity = new DefendCity(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, general.citizen, battle);
			general.Initialize (defendCity);
			general.SetSoldiers (soldiers);
//			general.avatar.GetComponent<GeneralAvatar> ().CreatePath (PATHFINDING_MODE.MAJOR_ROADS_ONLY_KINGDOM);
			return defendCity;
		}
		return null;
	}
	internal Caravaneer CreateCaravaneerEvent(City sourceCity) {
		Citizen citizen = sourceCity.CreateNewAgent (ROLE.CARAVAN, null);
		Caravan caravan = (Caravan)citizen.assignedRole;
		Caravaneer caravaneer = new Caravaneer(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, citizen);
		caravan.Initialize (caravaneer);
		caravaneer.Initialize ();
		return caravaneer;
	}
	internal Refuge CreateRefugeEvent(City city, int population) {
		Citizen citizen = city.CreateNewAgent (ROLE.REFUGEE, null);
		if(citizen != null){
			Refugee refugee	= (Refugee)citizen.assignedRole;
			Refuge refuge = new Refuge(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, refugee.citizen, city);
			refugee.Initialize (refuge);
			refugee.SetPopulation (population);
			refuge.Initialize ();
			return refuge;
		}
		return null;
	}
    internal AllianceOfConquestOffer CreateAllianceOfConquestOfferEvent(Kingdom offeringKingdom, Kingdom offeredToKingdom, Kingdom conquestTarget) {
        AllianceOfConquestOffer aoc = new AllianceOfConquestOffer(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, 
            offeringKingdom.king, offeringKingdom, offeredToKingdom, conquestTarget);
        return aoc;
    }
    internal InternationalIncident CreateInternationalIncidentEvent(Kingdom startedByKingdom, Kingdom sourceKingdom, Kingdom targetKingdom, bool isSourceKingdomAggrieved, bool isTargetKingdomAggrieved) {
		InternationalIncident internationalIncident = new InternationalIncident(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, startedByKingdom.king, sourceKingdom, targetKingdom, isSourceKingdomAggrieved, isTargetKingdomAggrieved);
		return internationalIncident;
	}

    internal AllianceOfProtectionOffer CreateAllianceOfProtectionOfferEvent(Kingdom offeringKingdom, Kingdom offeredToKingdom) {
        AllianceOfProtectionOffer aop = new AllianceOfProtectionOffer(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year,
            offeringKingdom.king, offeringKingdom, offeredToKingdom);
        return aop;
    }
    internal TradeDealOffer CreateTradeDealOfferEvent(Kingdom offeringKingdom, Kingdom offeredToKingdom) {
        TradeDealOffer tdo = new TradeDealOffer(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, offeringKingdom.king,
            offeringKingdom, offeredToKingdom);
        return tdo;
    }
    //internal HuntLair CreateHuntLairEvent(Kingdom sourceKingdom){
    //	if(sourceKingdom.isLockedDown){
    //		return null;
    //	}
    //	if (sourceKingdom.GetActiveEventsOfTypeCount(EVENT_TYPES.HUNT_LAIR) >= sourceKingdom.cities.Count) {
    //		return null;
    //	}
    //	HexTile chosenLairTile = null;
    //	Lair lair = null;
    //	List<HexTile> lairTiles = new List<HexTile>();

    //	lairTiles = sourceKingdom.fogOfWarDict [FOG_OF_WAR_STATE.VISIBLE].Where(x => x.lair != null).ToList();
    //	if(lairTiles == null || lairTiles.Count <= 0){
    //		lairTiles = sourceKingdom.fogOfWarDict [FOG_OF_WAR_STATE.SEEN].Where(x => x.lair != null).ToList();
    //	}


    //	if(lairTiles != null && lairTiles.Count > 0){
    //		lairTiles = lairTiles.OrderBy (x => PathGenerator.Instance.GetDistanceBetweenTwoTiles (sourceKingdom.capitalCity.hexTile, x)).ToList();
    //		chosenLairTile = lairTiles [0];
    //		lair = chosenLairTile.lair;
    //	}
    //	Citizen citizen = sourceKingdom.capitalCity.CreateAgent(ROLE.RANGER, EVENT_TYPES.HUNT_LAIR, chosenLairTile, EventManager.Instance.eventDuration[EVENT_TYPES.HUNT_LAIR]);
    //	if(citizen != null){
    //		Ranger ranger = (Ranger)citizen.assignedRole;
    //		HuntLair huntLair = new HuntLair(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year,
    //			sourceKingdom.king, ranger, lair);
    //		ranger.Initialize (huntLair);
    //           return huntLair;
    //	}
    //	return null;
    //}

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
