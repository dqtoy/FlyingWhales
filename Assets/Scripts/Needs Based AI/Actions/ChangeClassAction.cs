using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class ChangeClassAction : CharacterAction {
    public NewParty partyAssigned;
    private string _advertisedClassName;

    public ChangeClassAction() : base(ACTION_TYPE.CHANGE_CLASS) {
    }

    #region Overrides
    public override void OnChooseAction(NewParty iparty, IObject targetObject) {
        base.OnChooseAction(iparty, targetObject);
        partyAssigned = iparty;
    }
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        GiveAllReward(party);
        if(partyAssigned != null && partyAssigned.icharacters[0] is Character) {
            Character character = partyAssigned.icharacters[0] as Character;
            character.ChangeClass(_advertisedClassName);
        }
    }
    public override bool CanBeDone(IObject targetObject) {
        if(partyAssigned != null) {
            return false;
        }
        return base.CanBeDone(targetObject);
    }
    public override bool CanBeDoneBy(CharacterParty party, IObject targetObject) {
        if(party.icharacters[0].characterClass != null) {
            if(party.home.excessClasses.Contains(party.icharacters[0].characterClass.className)
                && party.home.missingClasses.Contains(_advertisedClassName)) { //TODO: Subject for change
                return true;
            }
        }
        return false;
    }
    public override void EndAction(CharacterParty party, IObject targetObject) {
        base.EndAction(party, targetObject);
        partyAssigned = null;
    }
    #endregion

    public void SetAdvertisedClass(string className) {
        _advertisedClassName = className;
    }
}
