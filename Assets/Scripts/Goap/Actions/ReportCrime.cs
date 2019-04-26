using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReportCrime : GoapAction {

    private CRIME crime;
    private Character criminal;

    public ReportCrime(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.REPORT_CRIME, INTERACTION_ALIGNMENT.GOOD, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Work_Icon;
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
            TIME_IN_WORDS.LATE_NIGHT,
        };
    }

    public void SetCrimeToReport(CRIME crime, Character criminal) {
        this.crime = crime;
        this.criminal = criminal;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetCharacterMissing && (actor.gridTileLocation == poiTarget.gridTileLocation || actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation))) {
            SetState("Report Crime Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 3;
    }
    public override void InitializeOtherData(object[] otherData) {
        base.InitializeOtherData(otherData);
        //GoapAction crime = otherData[0] as GoapAction;
        SetCrimeToReport((CRIME)otherData[0], otherData[1] as Character);
        if (thoughtBubbleMovingLog != null) {
            thoughtBubbleMovingLog.AddToFillers(criminal, criminal.name, LOG_IDENTIFIER.CHARACTER_3);
        }
    }
    #endregion

    #region State Effects
    public void PreReportCrimeSuccess() {
        //**Effect 1**: The reported criminal will gain the associated Crime trait
        criminal.AddCriminalTrait(crime);
        currentState.AddLogFiller(criminal, criminal.name, LOG_IDENTIFIER.CHARACTER_3);
    }
    #endregion

    #region Requirements
    private bool Requirement() {
        //**Advertiser**: All Faction Leaders, Nobles and Soldiers
        if (poiTarget is Character && poiTarget != actor && poiTarget != criminal) {
            Character character = poiTarget as Character;
            if (character.role.roleType == CHARACTER_ROLE.LEADER || character.role.roleType == CHARACTER_ROLE.NOBLE || character.role.roleType == CHARACTER_ROLE.SOLDIER) {
                return true;
            }
        }
        return false;
    }
    #endregion
}
