using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReportCrime : GoapAction {

    private CRIME crime;
    private AlterEgoData criminal;
    private GoapAction crimeAction;

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

    public void SetCrimeToReport(CRIME crime, AlterEgoData criminal, GoapAction crimeAction) {
        this.crime = crime;
        this.criminal = criminal;
        this.crimeAction = crimeAction;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing && (poiTarget as Character).IsInOwnParty()) {
            SetState("Report Crime Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 3;
    }
    public override bool InitializeOtherData(object[] otherData) {
        if (otherData.Length == 3 && otherData[0] is CRIME && otherData[1] is AlterEgoData && otherData[2] is GoapAction) {
            //GoapAction crime = otherData[0] as GoapAction;
            SetCrimeToReport((CRIME)otherData[0], otherData[1] as AlterEgoData, otherData[2] as GoapAction);
            if (thoughtBubbleMovingLog != null) {
                thoughtBubbleMovingLog.AddToFillers(criminal.owner, criminal.owner.name, LOG_IDENTIFIER.CHARACTER_3);
            }
            return true;
        }
        return base.InitializeOtherData(otherData);
    }
    #endregion

    #region State Effects
    public void PreReportCrimeSuccess() {
        //**Effect 1**: The reported criminal will gain the associated Crime trait
        criminal.owner.AddCriminalTrait(crime);
        currentState.AddLogFiller(criminal, criminal.owner.name, LOG_IDENTIFIER.CHARACTER_3);
        Character target = poiTarget as Character;

        //**Effect 2**: Share event related to the Crime to the Target's memories
        if (crimeAction != null) {
            target.CreateInformedEventLog(crimeAction);
        }
        target.ReactToCrime(crime, criminal, null, crimeAction);

        
    }
    #endregion

    #region Requirements
    private bool Requirement() {
        //**Advertiser**: All Faction Leaders, Nobles and Soldiers
        if (poiTarget is Character && poiTarget != actor && (criminal == null || poiTarget != criminal.owner)) {
            Character character = poiTarget as Character;
            if (character.GetTrait("Restrained") != null) {
                return false; //do not allow restrained
            }
            if (character.role.roleType == CHARACTER_ROLE.LEADER || character.role.roleType == CHARACTER_ROLE.NOBLE || character.role.roleType == CHARACTER_ROLE.SOLDIER) {
                return true;
            }
        }
        return false;
    }
    #endregion
}
