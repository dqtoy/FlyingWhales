using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnrollAction : CharacterAction {

    private ECS.Character mentor;

    public EnrollAction() : base(ACTION_TYPE.ENROLL) {
        
    }

    #region Overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        //ActionSuccess();
        GiveAllReward(party);
    }
    public override CharacterAction Clone() {
        EnrollAction idleAction = new EnrollAction();
        SetCommonData(idleAction);
        idleAction.Initialize();
        return idleAction;
    }
    public override bool CanBeDoneBy(CharacterParty party, IObject targetObject) {
        if (targetObject is ICharacterObject) {
            ICharacterObject owner = targetObject as ICharacterObject;
            if (owner.iparty.icharacters[0] is ECS.Character) {
                mentor = (owner.iparty.icharacters[0] as ECS.Character);
            }
        }
        ICharacter currCharacter = party.mainCharacter;
        if (currCharacter is ECS.Character) {
            ECS.Character character = currCharacter as ECS.Character;
            Relationship rel = mentor.GetRelationshipWith(character);
            if (rel == null || !rel.HasStatus(CHARACTER_RELATIONSHIP.STUDENT)) {
                return true;
            }
        }
        return base.CanBeDoneBy(party, targetObject);
    }
    public override bool CanBeDone(IObject targetObject) {
        return false; //Change this to something more elegant, this is to prevent other characters that don't have the release character quest from releasing this character.
    }
    #endregion
}
