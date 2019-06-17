using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AskForHelpRemovePoisonTable : GoapAction {

    private Character troubledCharacter;
    private IPointOfInterest targetTable;

    public AskForHelpRemovePoisonTable(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.ASK_FOR_HELP_REMOVE_POISON_TABLE, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        troubledCharacter = actor.troubledCharacter;
        actionIconString = GoapActionStateDB.Work_Icon;
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
            TIME_IN_WORDS.LATE_NIGHT,
        };
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    //protected override void ConstructPreconditionsAndEffects() {
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = actor.homeArea, targetPOI = poiTarget });
    //}
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing && (poiTarget as Character).IsInOwnParty()) {
            SetState("Ask Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 3;
    }
    //protected override void AddDefaultObjectsToLog(Log log) {
    //    log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //    log.AddToFillers(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER); //Target character is only the identifier but it doesn't mean that this is a character, it can be item, etc.
    //    //if (targetTable != null) {
    //    //    log.AddToFillers(targetTable.gridTileLocation.structure, targetTable.gridTileLocation.structure.name, LOG_IDENTIFIER.LANDMARK_1);
    //    //}
    //}
    public override bool InitializeOtherData(object[] otherData) {
        if(otherData.Length == 1 && otherData[0] is IPointOfInterest) {
            targetTable = otherData[0] as IPointOfInterest;
            if (thoughtBubbleMovingLog != null) {
                thoughtBubbleMovingLog.AddToFillers(targetTable.gridTileLocation.structure, targetTable.gridTileLocation.structure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
            }
            return true;
        }
        return base.InitializeOtherData(otherData);
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        return actor != poiTarget;
    }
    #endregion

    #region State Effects
    public void PreAskSuccess() {
        currentState.AddLogFiller(troubledCharacter, troubledCharacter.name, LOG_IDENTIFIER.CHARACTER_3);
        currentState.AddLogFiller(targetTable.gridTileLocation.structure, targetTable.gridTileLocation.structure.name, LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterAskSuccess() {
        Character target = poiTarget as Character;
        //**Effect 1**: Target will add a Remove Poison Table job on his personal queue.
        GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REMOVE_POISON, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Poisoned", targetPOI = targetTable });
        target.jobQueue.AddJobInQueue(job);
    }
    //public void PreTargetMissing() {
    //    currentState.AddLogFiller(troubledCharacter, troubledCharacter.name, LOG_IDENTIFIER.CHARACTER_3);
    //}
    #endregion
}
