using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leader : CharacterRole {

    public Leader(ECS.Character character) : base(character) {
        _roleType = CHARACTER_ROLE.LEADER;
    }
}
