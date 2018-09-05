using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Stalker : Attribute {

    CharacterAction stalk;
    public Stalker() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.STALKER) {

    }

    #region Overrides
    public override void OnAddAttribute(Character character) {
        base.OnAddAttribute(character);
        stalk = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.STALK);
        character.AddMiscAction(stalk);
    }
    public override void OnRemoveAttribute() {
        base.OnRemoveAttribute();
        character.RemoveMiscAction(stalk);
    }
    #endregion
}
