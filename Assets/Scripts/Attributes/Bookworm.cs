using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Bookworm : CharacterAttribute {

    CharacterAction readAction;
    public Bookworm() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.BOOKWORM) {

    }

    #region Overrides
    public override void OnAddAttribute(Character character) {
        base.OnAddAttribute(character);
        readAction = character.GetMiscAction(ACTION_TYPE.READ);
        if(readAction != null) {
            readAction.AdjustWeight(30);
        }
    }
    public override void OnRemoveAttribute() {
        base.OnRemoveAttribute();
        if (readAction != null) {
            readAction.AdjustWeight(30);
        }
    }
    public override void CharacterHasAction(CharacterAction action) {
        base.CharacterHasAction(action);
        if(action.actionType == ACTION_TYPE.READ) {
            readAction = action;
            readAction.AdjustWeight(30);
        }
    }
    #endregion
}
