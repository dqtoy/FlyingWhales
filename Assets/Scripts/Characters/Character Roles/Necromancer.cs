using UnityEngine;
using System.Collections;

public class Necromancer : CharacterRole {

	public Necromancer(ECS.Character character): base (character) {
        _roleType = CHARACTER_ROLE.NECROMANCER;
    }
}
