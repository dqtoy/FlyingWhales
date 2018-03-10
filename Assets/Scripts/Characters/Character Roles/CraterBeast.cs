using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CraterBeast : CharacterRole {

	public CraterBeast(ECS.Character character): base (character) {
		_roleType = CHARACTER_ROLE.CRATER_BEAST;
		_allowedRoadTypes = new List<ROAD_TYPE>() {
			ROAD_TYPE.MAJOR, ROAD_TYPE.MINOR
		};
		_canPassHiddenRoads = true;

		_roleTasks.Add (new DoNothing (this._character));
		_roleTasks.Add (new Patrol (this._character, 10));

		_defaultRoleTask = _roleTasks [1];
	}
}

