using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Starving : CharacterAttribute {
    public Starving() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.STARVING) {
    }

    public override void OnAddAttribute(Character character) {
        base.OnAddAttribute(character);
        //_character.AdjustPhysicalPoints(-3);
    }
    public override void OnRemoveAttribute() {
        base.OnRemoveAttribute();
        //_character.AdjustPhysicalPoints(3);
    }
}
