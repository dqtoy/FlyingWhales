using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Civilian : CharacterRole {

    public Civilian(Character character) : base(character) {
        _roleType = CHARACTER_ROLE.CIVILIAN;

        SetFullness(1000);
        SetEnergy(1000);
        SetFun(600);
        SetPrestige(400);
        SetSanity(1000);
        UpdateHappiness();
    }
}
