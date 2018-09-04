using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Bookworm : Attribute {

    CharacterAction readAction;
    public Bookworm() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.BOOKWORM) {

    }

    #region Overrides
    public override void OnAddAttribute(Character character) {
        base.OnAddAttribute(character);
        readAction = character.GetMiscAction(ACTION_TYPE.READ);
        readAction.AdjustWeight(30);
    }
    public override void OnRemoveAttribute() {
        base.OnRemoveAttribute();
        readAction.AdjustWeight(-30);
    }
    #endregion
}
