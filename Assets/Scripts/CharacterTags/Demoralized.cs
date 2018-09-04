using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class Demoralized : CharacterAttribute {
    public Demoralized(Character character) : base(character, ATTRIBUTE.DEMORALIZED) {
    }

    public override void Initialize() {
        base.Initialize();
        _character.AdjustMentalPoints(-2);
    }

    public override void OnRemoveTag() {
        base.OnRemoveTag();
        _character.AdjustMentalPoints(2);
    }
}
