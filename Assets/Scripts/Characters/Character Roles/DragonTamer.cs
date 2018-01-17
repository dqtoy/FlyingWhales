using UnityEngine;
using System.Collections;

public class DragonTamer : CharacterRole {
    
	public DragonTamer(ECS.Character character): base (character) {
        _roleType = CHARACTER_ROLE.DRAGON_TAMER;
    }
}
