using UnityEngine;
using System.Collections;
using ECS;

public class Hungry : CharacterAttribute {
    public Hungry(Character character) : base(character, ATTRIBUTE.HUNGRY) {

    }

    public override void Initialize() {
        base.Initialize();
        _character.AdjustPhysicalPoints(-1);
    }

    public override void OnRemoveTag() {
        base.OnRemoveTag();
        _character.AdjustPhysicalPoints(1);
    }
}