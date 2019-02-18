using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Adventurer : CharacterRole {

    public Adventurer(Character character) : base(character) {
        _roleType = CHARACTER_ROLE.ADVENTURER;
    }
}
