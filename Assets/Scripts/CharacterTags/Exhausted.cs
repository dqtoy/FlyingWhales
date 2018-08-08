using UnityEngine;
using System.Collections;
using ECS;

public class Exhausted : CharacterAttribute {
    public Exhausted(Character character) : base(character, ATTRIBUTE.EXHAUSTED) {

    }

    public override void Initialize() {
        base.Initialize();
        _character.AdjustPhysicalPoints(-2);
    }

    public override void OnRemoveTag() {
        base.OnRemoveTag();
        _character.AdjustPhysicalPoints(2);
    }
}
