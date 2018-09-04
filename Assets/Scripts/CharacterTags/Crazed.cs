using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class Crazed : CharacterAttribute {
    public Crazed(Character character) : base(character, ATTRIBUTE.CRAZED) {
    }

    public override void Initialize() {
        base.Initialize();
        _character.AdjustMentalPoints(-3);
    }

    public override void OnRemoveTag() {
        base.OnRemoveTag();
        _character.AdjustMentalPoints(3);
    }
}
