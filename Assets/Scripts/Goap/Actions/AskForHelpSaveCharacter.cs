using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AskForHelpSaveCharacter : GoapAction {

    private Character troubledCharacter;

    public AskForHelpSaveCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.ASK_FOR_HELP_SAVE_CHARACTER, INTERACTION_ALIGNMENT.GOOD, actor, poiTarget) {
        troubledCharacter = actor.troubledCharacter;
        actionIconString = GoapActionStateDB.Hostile_Icon;
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
            TIME_IN_WORDS.LATE_NIGHT,
        };
        isNotificationAnIntel = false;
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
    protected override void AddDefaultObjectsToLog(Log log) {
        base.AddDefaultObjectsToLog(log);
        if (troubledCharacter != null) {
            log.AddToFillers(troubledCharacter, troubledCharacter.name, LOG_IDENTIFIER.CHARACTER_3);
        }
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
    }
    public void AfterAskSuccess() {
        Character target = poiTarget as Character;
        //Add Save Character Job to target
        target.CreateSaveCharacterJob(troubledCharacter);
    }
    public void PreTargetMissing() {
        currentState.AddLogFiller(troubledCharacter, troubledCharacter.name, LOG_IDENTIFIER.CHARACTER_3);
    }
    #endregion
}

public class AskForHelpSaveCharacterData : GoapActionData {
    public AskForHelpSaveCharacterData() : base(INTERACTION_TYPE.ASK_FOR_HELP_SAVE_CHARACTER) {
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor != poiTarget;
    }
}