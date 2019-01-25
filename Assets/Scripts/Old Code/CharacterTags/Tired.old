using UnityEngine;
using System.Collections;


public class Tired : CharacterAttribute {
    public Tired() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.TIRED) {

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
