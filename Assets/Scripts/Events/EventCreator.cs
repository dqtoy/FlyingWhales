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
		Citizen expander = kingdom.capitalCity.CreateAgent (ROLE.EXPANDER);
		List<HexTile> path = PathGenerator.Instance.GetPath (kingdom.capitalCity.hexTile, hexTileToExpandTo, PATHFINDING_MODE.COMBAT).ToList();
		if (expander != null && path != null) {
			Expansion expansion = new Expansion (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, expander, hexTileToExpandTo);
			expander.assignedRole.Initialize (expansion, path);
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
		List<HexTile> path = PathGenerator.Instance.GetPath (firstKingdom.capitalCity.hexTile, city.hexTile, PATHFINDING_MODE.COMBAT).ToList();
		if(path != null){
			Citizen raider = firstKingdom.capitalCity.CreateAgent (ROLE.RAIDER);
			if(raider != null){
				Raid raid = new Raid(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, firstKingdom.king, city);
				raider.assignedRole.Initialize (raid, path);
				return raid;
			}
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
		List<HexTile> path = PathGenerator.Instance.GetPath (kingdom.capitalCity.hexTile, friend.capitalCity.hexTile, PATHFINDING_MODE.COMBAT).ToList();
		if (path == null) {
			return null;
		}
		Citizen envoy = kingdom.capitalCity.CreateAgent (ROLE.ENVOY);
		Citizen citizenToPersuade = friend.king;
		if(envoy != null && citizenToPersuade != null){
			Envoy chosenEnvoy = (Envoy)envoy.assignedRole;
			JoinWar joinWar = new JoinWar (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, invasionPlan.startedBy, 
				citizenToPersuade, chosenEnvoy, invasionPlan.targetKingdom, invasionPlan);
			envoy.assignedRole.Initialize (joinWar, path);
			return joinWar;
		}
		return null;
	}
	internal StateVisit CreateStateVisitEvent(Kingdom firstKingdom, Kingdom secondKingdom){
		List<HexTile> path = PathGenerator.Instance.GetPath (secondKingdom.capitalCity.hexTile, firstKingdom.capitalCity.hexTile, PATHFINDING_MODE.COMBAT).ToList();
		if (path == null) {
			return null;
		}
		Citizen visitor = secondKingdom.capitalCity.CreateAgent (ROLE.ENVOY);
		if(visitor != null){
			Envoy chosenEnvoy = (Envoy)visitor.assignedRole;
			StateVisit stateVisit = new StateVisit(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, firstKingdom.king, secondKingdom, chosenEnvoy);
			visitor.assignedRole.Initialize (stateVisit, path);
			return stateVisit;
		}
		return null;
	}
}
