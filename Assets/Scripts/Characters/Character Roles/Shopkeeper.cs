using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class Shopkeeper : Civilian {

    public Shopkeeper(Character character) : base(character) {
        _roleType = CHARACTER_ROLE.SHOPKEEPER;
    }
}
