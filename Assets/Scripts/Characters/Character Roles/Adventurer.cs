using UnityEngine;
using System.Collections;

public class Adventurer : CharacterRole {

	public Adventurer(ECS.Character character): base (character) {
        _roleType = CHARACTER_ROLE.ADVENTURER;
    }
}
