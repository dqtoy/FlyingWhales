using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beast : CharacterRole {

    public Beast(ECS.Character character) : base(character) {
        _roleType = CHARACTER_ROLE.BEAST;
    }
}
