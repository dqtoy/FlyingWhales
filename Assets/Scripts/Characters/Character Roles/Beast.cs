using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beast : CharacterRole {

    public Beast(Character character) : base(character) {
        _roleType = CHARACTER_ROLE.BEAST;
    }
}
