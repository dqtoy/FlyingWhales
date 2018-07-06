using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnrollAction : CharacterAction {

    private ECS.Character mentor;

    public EnrollAction(ObjectState state) : base(state, ACTION_TYPE.ENROLL) {
        if (this.state.obj is ICharacterObject) {
            ICharacterObject owner = this.state.obj as ICharacterObject;
            if (owner.iparty is CharacterParty) {
                mentor = ((owner.iparty as CharacterParty).mainCharacter as ECS.Character);
            }
        }
    }

    #region Overrides
    public override void PerformAction(CharacterParty party) {
        base.PerformAction(party);
        ActionSuccess();
        GiveAllReward(party);
    }
    public override CharacterAction Clone(ObjectState state) {
        EnrollAction idleAction = new EnrollAction(state);
        SetCommonData(idleAction);
        idleAction.Initialize();
        return idleAction;
    }
    public override bool CanBeDoneBy(CharacterParty party) {
        ICharacter currCharacter = party.mainCharacter;
        if (currCharacter is ECS.Character) {
            ECS.Character character = currCharacter as ECS.Character;
            Relationship rel = mentor.GetRelationshipWith(character);
            if (rel == null || !rel.HasStatus(CHARACTER_RELATIONSHIP.STUDENT)) {
                return true;
            }
        }
        return base.CanBeDoneBy(party);
    }
    public override bool CanBeDone() {
        return false; //Change this to something more elegant, this is to prevent other characters that don't have the release character quest from releasing this character.
    }
    #endregion
}
