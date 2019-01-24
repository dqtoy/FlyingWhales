using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Meditator : CharacterAttribute {

    public Meditator() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.MEDITATOR) {

    }

    #region Overrides
    public override void OnAddAttribute(Character character) {
        base.OnAddAttribute(character);
        CharacterAction meditate = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.MEDITATE);
        character.AddMiscAction(meditate);
    }
    public override void OnRemoveAttribute() {
        base.OnRemoveAttribute();
        character.RemoveMiscAction(ACTION_TYPE.MEDITATE);
    }
    #endregion
}
