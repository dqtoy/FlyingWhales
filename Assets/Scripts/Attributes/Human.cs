using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Human : CharacterAttribute {
    public Human() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.HUMAN) {

    }

    #region Overrides
    public override void OnAddAttribute(Character character) {
        base.OnAddAttribute(character);
        CharacterAction sing = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.SING);
        CharacterAction playInstrument = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.PLAYING_INSTRUMENT);

        character.AddMiscAction(sing);
        character.AddMiscAction(playInstrument);
    }
    public override void OnRemoveAttribute() {
        base.OnRemoveAttribute();
        character.RemoveMiscAction(ACTION_TYPE.SING);
        character.RemoveMiscAction(ACTION_TYPE.PLAYING_INSTRUMENT);
    }
    #endregion
}
