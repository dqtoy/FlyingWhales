using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Army : CharacterRole {

    public Army(Character character) : base(character) {
        _roleType = CHARACTER_ROLE.ARMY;
    }
}
