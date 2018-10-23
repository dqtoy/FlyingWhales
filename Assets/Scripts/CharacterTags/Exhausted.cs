using UnityEngine;
using System.Collections;
using ECS;

public class Exhausted : CharacterAttribute {
    public Exhausted() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.EXHAUSTED) {

    }
    public override void OnAddAttribute(Character character) {
        base.OnAddAttribute(character);
        //_character.AdjustPhysicalPoints(-2);
    }
    public override void OnRemoveAttribute() {
        base.OnRemoveAttribute();
        //_character.AdjustPhysicalPoints(2);
    }
}
