using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class Starving : CharacterAttribute {
    public Starving(Character character) : base(character, ATTRIBUTE.STARVING) {
    }

    public override void Initialize() {
        base.Initialize();
        _character.AdjustPhysicalPoints(-3);
    }

    public override void OnRemoveTag() {
        base.OnRemoveTag();
        _character.AdjustPhysicalPoints(3);
    }
}
