using UnityEngine;
using System.Collections;
using ECS;

public class Exhausted : CharacterTag {
    public Exhausted(Character character) : base(character, CHARACTER_TAG.EXHAUSTED) {

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
