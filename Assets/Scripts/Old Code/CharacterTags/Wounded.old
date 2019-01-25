using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Wounded : CharacterAttribute {
    public Wounded() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.WOUNDED) {
    }

    public override void OnAddAttribute(Character character) {
        base.OnAddAttribute(character);
        //_character.AdjustPhysicalPoints(-1);
    }
    public override void OnRemoveAttribute() {
        base.OnRemoveAttribute();
        //_character.AdjustPhysicalPoints(1);
    }
}
