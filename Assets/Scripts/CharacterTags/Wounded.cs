using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class Wounded : CharacterAttribute {
    public Wounded(Character character) : base(character, ATTRIBUTE.WOUNDED) {
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
