using UnityEngine;
using System.Collections;

public class Mediator : CharacterRole {

	public Mediator(ECS.Character character): base (character) {
        _roleType = CHARACTER_ROLE.MEDIATOR;
    }
}
