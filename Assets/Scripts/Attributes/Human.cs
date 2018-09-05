using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Human : Attribute {

    CharacterAction sing;
    CharacterAction playInstrument;
    public Human() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.HUMAN) {

    }

    #region Overrides
    public override void OnAddAttribute(Character character) {
        base.OnAddAttribute(character);
        sing = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.SING);
        playInstrument = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.PLAYING_INSTRUMENT);

        character.AddMiscAction(sing);
        character.AddMiscAction(playInstrument);
    }
    public override void OnRemoveAttribute() {
        base.OnRemoveAttribute();
        character.RemoveMiscAction(sing);
        character.RemoveMiscAction(playInstrument);
    }
    #endregion
}
