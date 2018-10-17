using UnityEngine;
using System.Collections;
using ECS;

public class Hungry : Attribute {


    public Hungry() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.HUNGRY) {

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