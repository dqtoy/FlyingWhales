using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Gregarious : CharacterAttribute {

    public Gregarious() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.GREGARIOUS) {

    }

    #region Overrides
    public override void OnAddAttribute(Character character) {
        base.OnAddAttribute(character);
        CharacterAction party = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.PARTY);
        if (party != null) {
            party.AdjustWeight(30);
            character.AddMiscAction(party);
        }
    }
    public override void OnRemoveAttribute() {
        base.OnRemoveAttribute();
        character.RemoveMiscAction(ACTION_TYPE.PARTY);
    }
    #endregion
}
