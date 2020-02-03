using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Sing : GoapAction {

    public Sing() : base(INTERACTION_TYPE.SING) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.LUNCH_TIME,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
        };
        actionIconString = GoapActionStateDB.Entertain_Icon;
        isNotificationAnIntel = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Sing Success", goapNode);
    }
<<<<<<< Updated upstream
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return Utilities.rng.Next(20, 37);
=======
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = "\n" + name + " " + target.nameWithID + ":";
        int cost = Ruinarch.Utilities.rng.Next(90, 131);
        costLog += " +" + cost + "(Initial)";
        int numOfTimesActionDone = actor.jobComponent.GetNumOfTimesActionDone(this);
        if (numOfTimesActionDone > 5) {
            cost += 2000;
            costLog += " +2000(Times Played > 5)";
        } else {
            int timesCost = 10 * numOfTimesActionDone;
            cost += timesCost;
            costLog += " +" + timesCost + "(10 x Times Played)";
        }
        Trait trait = actor.traitContainer.GetNormalTrait<Trait>("Music Hater", "Music Lover");
        if (trait != null) {
            if (trait.name == "Music Hater") {
                cost += 2000;
                costLog += " +2000(Music Hater)";
            } else {
                cost += -15;
                costLog += " -15(Music Lover)";
            }
        }
        actor.logComponent.AppendCostLog(costLog);
        return cost;
>>>>>>> Stashed changes
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        actor.needsComponent.AdjustDoNotGetLonely(-1);
    }
    #endregion

    #region Effects
    public void PreSingSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustDoNotGetLonely(1);
        //currentState.SetIntelReaction(SingSuccessIntelReaction);
    }
    public void PerTickSingSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustHappiness(1000);
    }
    public void AfterSingSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustDoNotGetLonely(-1);
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
                return false;
            }
            return actor == poiTarget;
        }
        return false;
    }
    #endregion
    //#region Intel Reactions
    //private List<string> SingSuccessIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();

    //    if (status == SHARE_INTEL_STATUS.WITNESSED && recipient.traitContainer.GetNormalTrait<Trait>("Music Hater") != null) {
    //        recipient.traitContainer.AddTrait(recipient, "Annoyed");
    //        if (recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER) || recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.PARAMOUR)) {
    //            if (recipient.CreateBreakupJob(actor) != null) {
    //                Log log = new Log(GameManager.Instance.Today(), "Trait", "MusicHater", "break_up");
    //                log.AddToFillers(recipient, recipient.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //                log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //                log.AddLogToInvolvedObjects();
    //                PlayerManager.Instance.player.ShowNotificationFrom(recipient, log);
    //            }
    //        } else if (!recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
    //            //Otherwise, if the Actor does not yet consider the Target an Enemy, relationship degradation will occur, log:
    //            Log log = new Log(GameManager.Instance.Today(), "Trait", "MusicHater", "degradation");
    //            log.AddToFillers(recipient, recipient.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //            log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //            log.AddLogToInvolvedObjects();
    //            PlayerManager.Instance.player.ShowNotificationFrom(recipient, log);
    //            RelationshipManager.Instance.RelationshipDegradation(actor, recipient);
    //        }
    //    }
    //    return reactions;
    //}
    //#endregion
}

public class SingData : GoapActionData {
    public SingData() : base(INTERACTION_TYPE.SING) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor == poiTarget && actor.traitContainer.GetNormalTrait<Trait>("Music Hater") == null && (actor.currentMoodType == CHARACTER_MOOD.GOOD || actor.currentMoodType == CHARACTER_MOOD.GREAT);
    }
}