using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Disturbed : CharacterAttribute {
    public Disturbed() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.DISTURBED) {
    }

    public override void OnAddAttribute(Character character) {
        base.OnAddAttribute(character);
        //_character.AdjustMentalPoints(-2);
    }
    public override void OnRemoveAttribute() {
        base.OnRemoveAttribute();
        //_character.AdjustMentalPoints(2);
    }
}
