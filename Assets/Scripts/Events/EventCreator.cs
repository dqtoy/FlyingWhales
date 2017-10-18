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
