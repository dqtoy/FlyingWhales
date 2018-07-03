using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class Woodcutter : Civilian {
    public Woodcutter(Character character) : base(character) {
        _roleType = CHARACTER_ROLE.WOODCUTTER;
    }
}
