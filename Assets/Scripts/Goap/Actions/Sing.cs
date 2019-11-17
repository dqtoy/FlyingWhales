using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Sing : GoapAction {
    private int happinessValue = 1000;

    public Sing(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.SING, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.LUNCH_TIME,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
        };
        actionIconString = GoapActionStateDB.Entertain_Icon;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Sing Success");
    }
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        //**Cost**: randomize between 20 - 36 (if music lover 10 - 26)
        if(actor.traitContainer.GetNormalTrait("Music Lover") != null) {
            return Utilities.rng.Next(10, 27);
        }
        return Utilities.rng.Next(20, 37);
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    public override void OnStopWhilePerforming() {
        if (currentState.name == "Sing Success") {
            actor.AdjustDoNotGetLonely(-1);
        }
    }
    #endregion

    #region Effects
    private void PreSingSuccess() {
        actor.AdjustDoNotGetLonely(1);
        if(actor.traitContainer.GetNormalTrait("Music Lover") != null) {
            happinessValue = 1200;
        }
        currentState.SetIntelReaction(SingSuccessIntelReaction);
    }
    private void PerTickSingSuccess() {
        actor.AdjustHappiness(happinessValue);
    }
    private void AfterSingSuccess() {
        actor.AdjustDoNotGetLonely(-1);
    }
    #endregion

    #region Intel Reactions
    private List<string> SingSuccessIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();

        if (status == SHARE_INTEL_STATUS.WITNESSED && recipient.traitContainer.GetNormalTrait("Music Hater") != null) {
            recipient.traitContainer.AddTrait(recipient, "Annoyed");
            if (recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER) || recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.PARAMOUR)) {
                if (recipient.CreateBreakupJob(actor) != null) {
                    Log log = new Log(GameManager.Instance.Today(), "Trait", "MusicHater", "break_up");
                    log.AddToFillers(recipient, recipient.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    log.AddLogToInvolvedObjects();
                    PlayerManager.Instance.player.ShowNotificationFrom(recipient, log);
                }
            } else if (!recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
                //Otherwise, if the Actor does not yet consider the Target an Enemy, relationship degradation will occur, log:
                Log log = new Log(GameManager.Instance.Today(), "Trait", "MusicHater", "degradation");
                log.AddToFillers(recipient, recipient.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                log.AddLogToInvolvedObjects();
                PlayerManager.Instance.player.ShowNotificationFrom(recipient, log);
                RelationshipManager.Instance.RelationshipDegradation(actor, recipient);
            }
        }
        return reactions;
    }
    #endregion
}

public class SingData : GoapActionData {
    public SingData() : base(INTERACTION_TYPE.SING) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor == poiTarget && actor.traitContainer.GetNormalTrait("Music Hater") == null && (actor.currentMoodType == CHARACTER_MOOD.GOOD || actor.currentMoodType == CHARACTER_MOOD.GREAT);
    }
}