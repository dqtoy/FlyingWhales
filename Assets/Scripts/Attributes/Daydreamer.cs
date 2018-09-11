﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Daydreamer : Attribute {

    CharacterAction daydream;
    public Daydreamer() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.DAYDREAMER) {

    }

    #region Overrides
    public override void OnAddAttribute(Character character) {
        base.OnAddAttribute(character);
        daydream = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.DAYDREAM);
        character.AddMiscAction(daydream);
    }
    public override void OnRemoveAttribute() {
        base.OnRemoveAttribute();
        character.RemoveMiscAction(daydream);
    }
    #endregion
}