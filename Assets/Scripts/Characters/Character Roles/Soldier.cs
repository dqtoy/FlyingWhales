using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : CharacterRole {

    public Soldier(Character character) : base(character) {
        _roleType = CHARACTER_ROLE.SOLDIER;
    }
}
