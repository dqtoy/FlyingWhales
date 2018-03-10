using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Slyx : CharacterRole {

	public Slyx(ECS.Character character): base (character) {
		_roleType = CHARACTER_ROLE.SLYX;
		_cancelsAllOtherTasks = true;
	}
}
