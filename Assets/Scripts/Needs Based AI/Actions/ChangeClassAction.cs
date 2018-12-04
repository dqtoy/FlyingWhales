using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ChangeClassAction : CharacterAction {
    public Party partyAssigned;
    private string _advertisedClassName;

    public ChangeClassAction() : base(ACTION_TYPE.CHANGE_CLASS) {
    }

    #region Overrides
    public override void OnChooseAction(Party iparty, IObject targetObject) {
        base.OnChooseAction(iparty, targetObject);
        partyAssigned = iparty;
    }
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        if (party is CharacterParty) {
            GiveAllReward(party as CharacterParty);
        }
        if (partyAssigned != null) {
            partyAssigned.mainCharacter.ChangeClass(_advertisedClassName);
        }
    }
    public override bool CanBeDone(IObject targetObject) {
        if(partyAssigned != null) {
            return false;
        }
        return base.CanBeDone(targetObject);
    }
    public override bool CanBeDoneBy(Party party, IObject targetObject) {
        if(party.mainCharacter.characterClass != null) {
            //if(party.homeLandmark.tileLocation.areaOfTile.excessClasses.Contains(party.mainCharacter.characterClass.className)
            //    && party.homeLandmark.tileLocation.areaOfTile.missingClasses.Contains(_advertisedClassName)) { //TODO: Subject for change
            //    return true;
            //}
        }
        return false;
    }
    public override void EndAction(Party party, IObject targetObject) {
        base.EndAction(party, targetObject);
        partyAssigned = null;
    }
    public override CharacterAction Clone() {
        ChangeClassAction action = new ChangeClassAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion

    public void SetAdvertisedClass(string className) {
        _advertisedClassName = className;
    }
}
