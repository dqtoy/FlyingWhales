using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HousekeepingAction : CharacterAction {

    public HousekeepingAction() : base(ACTION_TYPE.HOUSEKEEPING) {
        _actionData.providedEnergy = -1f;
        _actionData.providedFun = 1f;

        _actionData.duration = 8;
    }

    #region Overrides
    public override void PerformAction(NewParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);

        //give the character the Provided Hunger, Provided Energy, Provided Joy, Provided Prestige
        if(party is CharacterParty) {
            GiveAllReward(party as CharacterParty);
        }
    }
    public override void DoneDuration(NewParty party, IObject targetObject) {
        base.DoneDuration(party, targetObject);
        StructureObj structure = targetObject as StructureObj;
        structure.SetIsDirty(false);
        GameDate dirtyDate = GameManager.Instance.Today();
        dirtyDate.AddDays(1);
        SchedulingManager.Instance.AddEntry(dirtyDate, () => structure.SetIsDirty(true));
    }
    public override IObject GetTargetObject(CharacterParty sourceParty) {
        if(sourceParty.mainCharacter.homeLandmark.landmarkObj.currentState.stateName != "Ruined" && sourceParty.mainCharacter.homeLandmark.landmarkObj.isDirty) {
            return sourceParty.mainCharacter.homeLandmark.landmarkObj;
        }
        return null;
    }
    public override CharacterAction Clone() {
        HousekeepingAction action = new HousekeepingAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion
}
