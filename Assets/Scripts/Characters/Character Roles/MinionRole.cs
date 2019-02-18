using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionRole : CharacterRole {

    public MinionRole(Character character) : base(character) {
        _roleType = CHARACTER_ROLE.MINION;
    }
}
