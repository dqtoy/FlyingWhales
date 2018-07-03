using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class Farmer : Civilian {
    public Farmer(Character character) : base(character) {
        _roleType = CHARACTER_ROLE.FARMER;
    }

}
