using UnityEngine;
using System.Collections;
using ECS;

public class Depressed : CharacterTag {
    public Depressed(Character character) : base(character, CHARACTER_TAG.DEPRESSED) {

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
