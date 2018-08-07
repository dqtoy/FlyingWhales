﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRole : CharacterRole {

    public PlayerRole(ECS.Character character) : base(character) {
        _roleType = CHARACTER_ROLE.PLAYER;
    }
}
