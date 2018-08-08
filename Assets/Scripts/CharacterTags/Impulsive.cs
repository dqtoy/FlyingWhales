using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class Impulsive : CharacterAttribute {
    public Impulsive(Character character) : base(character, ATTRIBUTE.IMPULSIVE) {
    }

    public override void Initialize() {
        base.Initialize();
    }

    public override void OnRemoveTag() {
        base.OnRemoveTag();
    }
}
