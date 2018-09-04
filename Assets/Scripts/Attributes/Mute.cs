using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Mute : Attribute {

    public Mute() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.MUTE) {

    }

    #region Overrides
    public override void OnAddAttribute(Character character) {
        base.OnAddAttribute(character);
        CharacterAction sing = character.GetMiscAction(ACTION_TYPE.SING);
        if (sing != null) {
            character.RemoveMiscAction(sing);
            _tempRemovedActions.Add(sing);
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
