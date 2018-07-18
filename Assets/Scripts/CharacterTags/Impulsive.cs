using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class Impulsive : CharacterTag {
    public Impulsive(Character character) : base(character, CHARACTER_TAG.IMPULSIVE) {
    }

    public override void Initialize() {
        base.Initialize();
    }

    public override void OnRemoveTag() {
        base.OnRemoveTag();
    }
}
