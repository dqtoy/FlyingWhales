using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HousekeepingAction : CharacterAction {

    public HousekeepingAction() : base(ACTION_TYPE.HOUSEKEEPING) {
        _actionData.providedEnergy = -1f;
        _actionData.providedFun = 1f;

        _actionData.duration = 8;
        _weight = 200;
    }

    #region Overrides
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);

        //give the character the Provided Hunger, Provided Energy, Provided Joy, Provided Prestige
        if(party is CharacterParty) {
            GiveAllReward(party as CharacterParty);
        }
    }
    public override void DoneDuration(Party party, IObject targetObject) {
        base.DoneDuration(party, targetObject);
        StructureObj structure = targetObject as StructureObj;
        structure.SetIsDirty(false);
        GameDate dirtyDate = GameManager.Instance.Today();
        dirtyDate.AddMonths(1);
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
    public override string GetArriveActionString(Party party = null) {
        Log arriveLog = new Log(GameManager.Instance.Today(), "CharacterActions", this.GetType().ToString(), "arrive_action");
        arriveLog.AddToFillers(party.owner as ECS.Character, (party.owner as ECS.Character).name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        return Utilities.LogReplacer(arriveLog);
    }
    public override string GetLeaveActionString(Party party = null) {
        Log arriveLog = new Log(GameManager.Instance.Today(), "CharacterActions", this.GetType().ToString(), "leave_action");
        arriveLog.AddToFillers(party.owner as ECS.Character, (party.owner as ECS.Character).name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        return Utilities.LogReplacer(arriveLog);
    }
    #endregion
}
