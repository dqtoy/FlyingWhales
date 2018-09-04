using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Deafened : Attribute {

    public Deafened() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.DEAFENED) {

    }

    #region Overrides
    public override void OnAddAttribute(Character character) {
        base.OnAddAttribute(character);
        CharacterAction sing = character.GetMiscAction(ACTION_TYPE.SING);
        CharacterAction playInstrument = character.GetMiscAction(ACTION_TYPE.PLAYING_INSTRUMENT);
        if(sing != null) {
            character.RemoveMiscAction(sing);
            _tempRemovedActions.Add(sing);
        }
        if (playInstrument != null) {
            character.RemoveMiscAction(playInstrument);
            _tempRemovedActions.Add(playInstrument);
        }
    }
    public override void OnRemoveAttribute() {
        base.OnRemoveAttribute();
        for (int i = 0; i < _tempRemovedActions.Count; i++) {
            character.AddMiscAction(_tempRemovedActions[i]);
        }
        _tempRemovedActions.Clear();
    }
    #endregion
}
