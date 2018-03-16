using UnityEngine;
using System.Collections;
using ECS;

public class Successor : CharacterTag {

    private ECS.Character _characterToSucceed;

    public Successor(Character character) : base(character, CHARACTER_TAG.SUCCESSOR) {
    }

    #region overrides
    public override void OnRemoveTag() {
        base.OnRemoveTag();
        (_characterToSucceed.faction as Tribe).SetSuccessor(null);
        _characterToSucceed.RemoveActionOnDeath(TriggerSuccession);
    }
    #endregion

    public void SetCharacterToSucceed(ECS.Character characterToSucceed) {
        _characterToSucceed = characterToSucceed;
        (_character.faction as Tribe).SetSuccessor(_character);
        _characterToSucceed.AddActionOnDeath(TriggerSuccession);
    }
    /*
     Make this character succeed the current leader if the current leader dies.
         */
    private void TriggerSuccession() {
        _characterToSucceed.faction.SetLeader(_character);
        _character.AssignRole(CHARACTER_ROLE.CHIEFTAIN);
        _character.RemoveCharacterTag(CHARACTER_TAG.SUCCESSOR);
    }
}
