using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Crazed : CharacterAttribute {
    public Crazed() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.CRAZED) {
    }

    public override void OnAddAttribute(Character character) {
        base.OnAddAttribute(character);
        //_character.AdjustMentalPoints(-3);
    }
    public override void OnRemoveAttribute() {
        base.OnRemoveAttribute();
        //_character.AdjustMentalPoints(3);
    }
}
