﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Cleaner : Attribute {

    CharacterAction housekeep;
    public Cleaner() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.CLEANER) {

    }

    #region Overrides
    public override void OnAddAttribute(Character character) {
        base.OnAddAttribute(character);
        housekeep = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.HOUSEKEEPING);
        character.AddMiscAction(housekeep);
    }
    public override void OnRemoveAttribute() {
        base.OnRemoveAttribute();
        character.RemoveMiscAction(housekeep);
    }
    #endregion
}