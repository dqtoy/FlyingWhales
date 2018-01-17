using UnityEngine;
using System.Collections;

public class Spy : CharacterRole {

	public Spy(ECS.Character character): base (character) {
        _roleType = CHARACTER_ROLE.SPY;
    }
}
