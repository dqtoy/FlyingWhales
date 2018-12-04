using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Marked : CharacterAttribute {

    public Marked() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.MARKED) {

    }

    #region Overrides
    public override void OnAddAttribute(Character character) {
        base.OnAddAttribute(character);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, CharacterDeath);
        PlayerManager.Instance.player.SetMarkedCharacter(_character);
        Messenger.Broadcast(Signals.CHARACTER_MARKED);
    }
    public override void OnRemoveAttribute() {
        base.OnRemoveAttribute();
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, CharacterDeath);
        PlayerManager.Instance.player.SetMarkedCharacter(null);
    }
    #endregion
    private void CharacterDeath(Character character) {
        if(_character.id == character.id) {
            _character.RemoveAttribute(this);
        }
    }
}
