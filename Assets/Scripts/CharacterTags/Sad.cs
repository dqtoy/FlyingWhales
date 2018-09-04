using UnityEngine;
using System.Collections;
using ECS;

public class Sad : CharacterTag {
    public Sad(Character character) : base(character, CHARACTER_TAG.SAD) {

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
