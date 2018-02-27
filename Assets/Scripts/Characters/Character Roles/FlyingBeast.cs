using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlyingBeast : CharacterRole {

	public FlyingBeast(ECS.Character character): base (character) {
		_roleType = CHARACTER_ROLE.FLYING_BEAST;
		_allowedRoadTypes = new List<ROAD_TYPE>() {
			ROAD_TYPE.MAJOR, ROAD_TYPE.MINOR
		};
		_canPassHiddenRoads = true;

		_roleTasks.Add (new DoNothing (this._character));
		_roleTasks.Add (new Rest (this._character));
		_roleTasks.Add (new Hibernate (this._character));
		_roleTasks.Add (new MoveTo (this._character));
		_roleTasks.Add (new HuntPrey (this._character));
		_roleTasks.Add (new Pillage (this._character));
		_roleTasks.Add (new Raze (this._character, 5));

		_defaultRoleTask = _roleTasks [2];
	}
}

