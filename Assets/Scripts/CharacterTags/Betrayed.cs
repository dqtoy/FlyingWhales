using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class Betrayed : CharacterAttribute {
    public Betrayed(Character character) : base(character, ATTRIBUTE.BETRAYED) {
    }

    public override void Initialize() {
        base.Initialize();
        _character.role.AdjustConstantSanityBuff(-50);
        _character.role.AdjustConstantFunBuff(-25);
    }

    public override void OnRemoveTag() {
        base.OnRemoveTag();
        _character.role.AdjustConstantSanityBuff(50);
        _character.role.AdjustConstantFunBuff(25);
    }
}
