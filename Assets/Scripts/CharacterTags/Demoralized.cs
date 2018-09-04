using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class Demoralized : CharacterTag {
    public Demoralized(Character character) : base(character, CHARACTER_TAG.DEMORALIZED) {
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
