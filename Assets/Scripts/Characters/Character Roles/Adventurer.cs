﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Adventurer : CharacterRole {

    public Adventurer() : base(CHARACTER_ROLE.ADVENTURER, "Normal", new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.INVENTORY, INTERACTION_CATEGORY.RECRUITMENT, INTERACTION_CATEGORY.EXPANSION, INTERACTION_CATEGORY.DEFENSE }) {
    }
}