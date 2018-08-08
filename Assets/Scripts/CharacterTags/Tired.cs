using UnityEngine;
using System.Collections;
using ECS;

public class Tired : CharacterAttribute {
    public Tired(Character character) : base(character, ATTRIBUTE.TIRED) {

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
