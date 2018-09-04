using UnityEngine;
using System.Collections;
using ECS;

public class Hungry : CharacterTag {
    public Hungry(Character character) : base(character, CHARACTER_TAG.HUNGRY) {

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