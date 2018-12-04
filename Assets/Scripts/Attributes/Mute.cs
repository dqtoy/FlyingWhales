using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Mute : CharacterAttribute {

    CharacterAction sing;
    public Mute() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.MUTE) {

    }

    #region Overrides
    public override void OnAddAttribute(Character character) {
        base.OnAddAttribute(character);
        sing = character.GetMiscAction(ACTION_TYPE.SING);
        if (sing != null) {
            sing.AdjustDisableCounter(1);
        }
    }
    public override void OnRemoveAttribute() {
        base.OnRemoveAttribute();
        if (sing != null) {
            sing.AdjustDisableCounter(-1);
        }
    }
    public override void CharacterHasAction(CharacterAction action) {
        base.CharacterHasAction(action);
        if (action.actionType == ACTION_TYPE.SING) {
            sing = action;
            action.AdjustDisableCounter(1);
        }
    }
    #endregion
}
