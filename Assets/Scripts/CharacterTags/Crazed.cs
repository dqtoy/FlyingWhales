using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class Crazed : CharacterTag {
    public Crazed(Character character) : base(character, CHARACTER_TAG.CRAZED) {
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
