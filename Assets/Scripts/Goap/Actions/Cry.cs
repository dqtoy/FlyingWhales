using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Cry : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.INDIRECT; } }

    public Cry() : base(INTERACTION_TYPE.CRY) {
        actionIconString = GoapActionStateDB.Entertain_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        isNotificationAnIntel = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Cry Success", goapNode);
    }
<<<<<<< Updated upstream
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return Utilities.rng.Next(25, 51);
=======
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = "\n" + name + " " + target.nameWithID + ":";
        int cost = Ruinarch.Utilities.rng.Next(90, 131);
        costLog += " +" + cost + "(Initial)";
        int timesCost = 10 * actor.jobComponent.GetNumOfTimesActionDone(this);
        cost += timesCost;
        costLog += " +" + timesCost + "(10 x Times Cried)";
        if (actor.moodComponent.moodState != MOOD_STATE.LOW && actor.moodComponent.moodState != MOOD_STATE.CRITICAL) {
            cost += 2000;
            costLog += " +2000(not Low and Crit mood)";
        }
        actor.logComponent.AppendCostLog(costLog);
        return cost;
    }
    public override string ReactionToActor(Character witness, ActualGoapNode node) {
        string response = base.ReactionToActor(witness, node);
        Character actor = node.actor;
        string opinionLabel = witness.opinionComponent.GetOpinionLabel(actor);
        if (opinionLabel == OpinionComponent.Enemy || opinionLabel == OpinionComponent.Rival) {
            if (UnityEngine.Random.Range(0, 2) == 0) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Scorn, witness, actor);
            }
        } else if (opinionLabel == OpinionComponent.Friend || opinionLabel == OpinionComponent.Close_Friend) {
            if (!witness.isSerialKiller) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor);
            }        
        } else if (opinionLabel == OpinionComponent.Acquaintance) {
            if (!witness.isSerialKiller && UnityEngine.Random.Range(0, 2) == 0) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor);
            }        
        } 
        return response;
>>>>>>> Stashed changes
    }
    #endregion    

    #region State Effects
    public void PerTickCrySuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustHappiness(500);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return actor == poiTarget;
        }
        return false;
    }
    #endregion
}

public class CryData : GoapActionData {
    public CryData() : base(INTERACTION_TYPE.CRY) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor == poiTarget && actor.currentMoodType == CHARACTER_MOOD.DARK;
    }
}