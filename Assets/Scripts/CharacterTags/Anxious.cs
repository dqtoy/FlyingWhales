using UnityEngine;
using System.Collections;
using ECS;

public class Anxious : CharacterAttribute {
    public Anxious(Character character) : base(character, ATTRIBUTE.ANXIOUS) {

    }

    public override void Initialize() {
        base.Initialize();
        _character.AdjustMentalPoints(-1);
    }

    public override void OnRemoveTag() {
        base.OnRemoveTag();
        _character.AdjustMentalPoints(1);
    }
}
