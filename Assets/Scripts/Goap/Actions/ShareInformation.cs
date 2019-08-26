using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShareInformation : GoapAction {
    public GoapAction eventToBeShared { get; private set; }

    public ShareInformation(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.SHARE_INFORMATION, INTERACTION_ALIGNMENT.GOOD, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Work_Icon;
        //validTimeOfDays = new TIME_IN_WORDS[] {
        //    TIME_IN_WORDS.MORNING,
        //    TIME_IN_WORDS.AFTERNOON,
        //    TIME_IN_WORDS.EARLY_NIGHT,
        //    TIME_IN_WORDS.LATE_NIGHT,
        //};
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        Character targetCharacter = poiTarget as Character;
        if (!isTargetMissing && targetCharacter.IsInOwnParty()) {
            SetState("Share Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 3;
    }
    public override bool InitializeOtherData(object[] otherData) {
        this.otherData = otherData;
        if (otherData.Length == 1 && otherData[0] is GoapAction) {
            eventToBeShared = otherData[0] as GoapAction;
            if (thoughtBubbleMovingLog != null) {
                thoughtBubbleMovingLog.AddToFillers(null, Utilities.LogDontReplace(eventToBeShared.currentState.descriptionLog), LOG_IDENTIFIER.APPEND);
                thoughtBubbleMovingLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.OTHER);
                thoughtBubbleMovingLog.AddToFillers(poiTarget, poiTarget.name, LOG_IDENTIFIER.OTHER_2);
                thoughtBubbleMovingLog.AddToFillers(eventToBeShared.currentState.descriptionLog.fillers);
            }
            return true;
        }
        return base.InitializeOtherData(otherData);
    }
    #endregion

    #region State Effects
    private void PreShareSuccess() {
        currentState.AddLogFiller(null, Utilities.LogDontReplace(eventToBeShared.currentState.descriptionLog), LOG_IDENTIFIER.APPEND);
        currentState.AddLogFiller(actor, actor.name, LOG_IDENTIFIER.OTHER);
        currentState.AddLogFiller(poiTarget, poiTarget.name, LOG_IDENTIFIER.OTHER_2);
        currentState.AddLogFillers(eventToBeShared.currentState.descriptionLog.fillers);
    }
    private void AfterShareSuccess() {
        if(eventToBeShared.currentState.shareIntelReaction != null) {
            eventToBeShared.currentState.shareIntelReaction.Invoke(poiTarget as Character, null, SHARE_INTEL_STATUS.INFORMED);
        }
    }
    #endregion

    #region Requirements
    private bool Requirement() {
        Character target = poiTarget as Character;
        return actor != target && target.role.roleType != CHARACTER_ROLE.BEAST;
    }
    #endregion
}

public class ShareInformationData : GoapActionData {
    public ShareInformationData() : base(INTERACTION_TYPE.SHARE_INFORMATION) {
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        Character target = poiTarget as Character;
        return actor != target && target.role.roleType != CHARACTER_ROLE.BEAST;
    }
}
