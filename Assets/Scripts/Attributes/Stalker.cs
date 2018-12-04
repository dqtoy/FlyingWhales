using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Stalker : CharacterAttribute {

    private Character _stalkee;

    #region getters/setters
    public Character stalkee {
        get { return _stalkee; }
    }
    #endregion

    public Stalker() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.STALKER) {

    }

    #region Overrides
    public override void OnAddAttribute(Character character) {
        base.OnAddAttribute(character);
        CharacterAction stalk = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.STALK);
        character.AddMiscAction(stalk);
    }
    public override void OnRemoveAttribute() {
        base.OnRemoveAttribute();
        character.RemoveMiscAction(ACTION_TYPE.STALK);
    }
    #endregion

    public void SetStalkee(Character character) {
        _stalkee = character;
    }
}
