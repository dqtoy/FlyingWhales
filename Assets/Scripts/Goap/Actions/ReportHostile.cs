using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReportHostile : GoapAction {

    private Character hostile;

    public ReportHostile(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.REPORT_HOSTILE, INTERACTION_ALIGNMENT.GOOD, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Work_Icon;
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
            TIME_IN_WORDS.LATE_NIGHT,
        };
    }

    public void SetHostileToReport(Character hostile) {
        this.hostile = hostile;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        Character targetCharacter = poiTarget as Character;
        if (!isTargetMissing && targetCharacter.IsInOwnParty()) {
            if(hostile.GetNumOfJobsTargettingThisCharacter("Assault") < 3 && hostile.GetTraitOf(TRAIT_TYPE.DISABLER) == null) {
                SetState("Report Hostile Success");
            } else {
                SetState("Report Hostile Fail");
            }
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 3;
    }
    public override bool InitializeOtherData(object[] otherData) {
        base.InitializeOtherData(otherData);
        //GoapAction crime = otherData[0] as GoapAction;
        SetHostileToReport(otherData[0] as Character);
        if (thoughtBubbleMovingLog != null) {
            thoughtBubbleMovingLog.AddToFillers(hostile, hostile.name, LOG_IDENTIFIER.CHARACTER_3);
        }
        return true;
    }
    #endregion

    #region State Effects
    private void PreReportHostileSuccess() {
        currentState.AddLogFiller(hostile, hostile.name, LOG_IDENTIFIER.CHARACTER_3);
        currentState.AddLogFiller(actor.specificLocation, actor.specificLocation.name, LOG_IDENTIFIER.LANDMARK_1);
        Character targetCharacter = poiTarget as Character;
        targetCharacter.CreateAssaultJobs(hostile, false, 3);
    }
    private void PreReportHostileFail() {
        currentState.AddLogFiller(hostile, hostile.name, LOG_IDENTIFIER.CHARACTER_3);
        currentState.AddLogFiller(actor.specificLocation, actor.specificLocation.name, LOG_IDENTIFIER.LANDMARK_1);
        currentState.AddLogFiller(null, Utilities.GetPronounString(hostile.gender, PRONOUN_TYPE.OBJECTIVE, false), LOG_IDENTIFIER.STRING_1);
    }
    private void PreTargetMissing() {
        currentState.AddLogFiller(hostile, hostile.name, LOG_IDENTIFIER.CHARACTER_3);
    }
    #endregion

    #region Requirements
    private bool Requirement() {
        //**Advertiser**: All Faction Leaders, Nobles and Soldiers
        if (poiTarget is Character && poiTarget != actor && poiTarget != hostile) {
            Character character = poiTarget as Character;
            if (character.role.roleType == CHARACTER_ROLE.LEADER || character.role.roleType == CHARACTER_ROLE.NOBLE || character.role.roleType == CHARACTER_ROLE.SOLDIER) {
                return true;
            }
        }
        return false;
    }
    #endregion
}
