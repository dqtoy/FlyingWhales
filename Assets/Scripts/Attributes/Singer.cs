using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Singer : CharacterAttribute {

    CharacterAction sing;
    public Singer() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.SINGER) {

    }

    #region Overrides
    public override void OnAddAttribute(Character character) {
        base.OnAddAttribute(character);
        sing = character.GetMiscAction(ACTION_TYPE.SING);
        if (sing != null) {
            sing.AdjustWeight(30);
        }
    }
    public override void OnRemoveAttribute() {
        base.OnRemoveAttribute();
        if (sing != null) {
            sing.AdjustWeight(-30);
        }
    }
    public override void CharacterHasAction(CharacterAction action) {
        base.CharacterHasAction(action);
        if (action.actionType == ACTION_TYPE.SING) {
            sing = action;
            sing.AdjustWeight(30);
        }
    }
    #endregion
}
