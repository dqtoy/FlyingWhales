﻿using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class Disturbed : CharacterTag {
    public Disturbed(Character character) : base(character, CHARACTER_TAG.DISTURBED) {
    }

    public override void Initialize() {
        base.Initialize();
        _character.AdjustMentalPoints(-2);
    }

    public override void OnRemoveTag() {
        base.OnRemoveTag();
        _character.AdjustMentalPoints(2);
    }

    public override void PerformDailyAction() {
        base.PerformDailyAction();
    }

    public override string ToString() {
        return base.ToString();
    }
}
