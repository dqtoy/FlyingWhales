﻿using UnityEngine;
using System.Collections;
using ECS;

public class Depressed : Attribute {
    public Depressed() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.DEPRESSED) {

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
