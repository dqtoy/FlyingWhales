using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bandit : CharacterRole {

    public Bandit(ECS.Character character) : base(character) {
        _roleType = CHARACTER_ROLE.BANDIT;
    }
}
