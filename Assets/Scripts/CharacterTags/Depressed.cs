using UnityEngine;
using System.Collections;
using ECS;

public class Depressed : CharacterAttribute {
    public Depressed(Character character) : base(character, ATTRIBUTE.DEPRESSED) {

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
