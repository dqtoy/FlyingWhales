using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class Miner : Civilian {

    public Miner(Character character) : base(character) {
        _roleType = CHARACTER_ROLE.MINER;
    }
}
