using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Palace : StructureObj {

    public Palace() : base() {
        _specificObjectType = LANDMARK_TYPE.PALACE;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
        ScheduleStartOfMonthActions();
    }

    #region Overrides
    public override IObject Clone() {
        Palace clone = new Palace();
        SetCommonData(clone);
        return clone;
    }
    #endregion

    private void StartOfMonth() {
        UpdateAdvertisedChangeClassAction();
        ScheduleStartOfMonthActions();
    }
    private void ScheduleStartOfMonthActions() {
        GameDate gameDate = GameManager.Instance.FirstDayOfTheMonth();
        gameDate.AddMonths(1);
        gameDate.AddHours(1);
        SchedulingManager.Instance.AddEntry(gameDate, () => StartOfMonth());
    }
    private void UpdateAdvertisedChangeClassAction() {
        ChangeClassAction changeClassAction = currentState.GetAction(ACTION_TYPE.CHANGE_CLASS) as ChangeClassAction;
        if(changeClassAction != null) {
            string highestPriorityMissingRole = string.Empty;
            for (int i = 0; i < objectLocation.tileLocation.areaOfTile.orderClasses.Count; i++) {
                if (objectLocation.tileLocation.areaOfTile.missingClasses.Contains(objectLocation.tileLocation.areaOfTile.orderClasses[i])){
                    highestPriorityMissingRole = objectLocation.tileLocation.areaOfTile.orderClasses[i];
                    break;
                }
            }
            if(highestPriorityMissingRole != string.Empty) {
                changeClassAction.SetAdvertisedClass(highestPriorityMissingRole);
            }
        }
    }
}
