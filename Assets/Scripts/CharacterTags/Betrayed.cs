﻿using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class Betrayed : Attribute {
    public Betrayed() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.BETRAYED) {
    }

    public override void OnAddAttribute(Character character) {
        base.OnAddAttribute(character);
        _character.role.AdjustConstantFunBuff(-25);
    }
    public override void OnRemoveAttribute() {
        base.OnRemoveAttribute();
        _character.role.AdjustConstantFunBuff(25);
    }
}
