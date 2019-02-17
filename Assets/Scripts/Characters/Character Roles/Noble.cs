using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noble : CharacterRole {

    public Noble(Character character) : base(character) {
        _roleType = CHARACTER_ROLE.NOBLE;
    }
}
