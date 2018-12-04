using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Deafened : CharacterAttribute {

    CharacterAction sing;
    CharacterAction playInstrument;
    public Deafened() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.DEAFENED) {

    }

    #region Overrides
    public override void OnAddAttribute(Character character) {
        base.OnAddAttribute(character);
        sing = character.GetMiscAction(ACTION_TYPE.SING);
        playInstrument = character.GetMiscAction(ACTION_TYPE.PLAYING_INSTRUMENT);

        if (sing != null) {
            sing.AdjustDisableCounter(1);
        }
        if (playInstrument != null) {
            playInstrument.AdjustDisableCounter(1);
        }
    }
    public override void OnRemoveAttribute() {
        base.OnRemoveAttribute();
        if (sing != null) {
            sing.AdjustDisableCounter(-1);
        }
        if (playInstrument != null) {
            playInstrument.AdjustDisableCounter(-1);
        }
    }
    public override void CharacterHasAction(CharacterAction action) {
        base.CharacterHasAction(action);
        if(action.actionType == ACTION_TYPE.SING) {
            sing = action;
            action.AdjustDisableCounter(1);
        }else if (action.actionType == ACTION_TYPE.PLAYING_INSTRUMENT) {
            playInstrument = action;
            action.AdjustDisableCounter(1);
        }
    }
    #endregion
}
