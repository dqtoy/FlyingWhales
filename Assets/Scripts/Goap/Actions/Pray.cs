using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Pray : GoapAction {

    public Pray() : base(INTERACTION_TYPE.PRAY) {
        this.goapName = "Pray";
        actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.EARLY_NIGHT,
            TIME_IN_WORDS.LATE_NIGHT,
        };
        actionIconString = GoapActionStateDB.Pray_Icon;
        shouldIntelNotificationOnlyIfActorIsActive = true;
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
        SetState("Pray Success", goapNode);
    }
<<<<<<< Updated upstream
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        //**Cost**: randomize between 15 - 55
        return Utilities.rng.Next(15, 56);
=======
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = "\n" + name + " " + target.nameWithID + ":";
        int cost = Ruinarch.Utilities.rng.Next(90, 131);
        costLog += " +" + cost + "(Initial)";
        int numOfTimesActionDone = actor.jobComponent.GetNumOfTimesActionDone(this);
        if (numOfTimesActionDone > 5) {
            cost += 2000;
            costLog += " +2000(Times Prayed > 5)";
        } else {
            int timesCost = 10 * numOfTimesActionDone;
            cost += timesCost;
            costLog += " +" + timesCost + "(10 x Times Prayed)";
        }
        if (actor.traitContainer.GetNormalTrait<Trait>("Evil", "Serial Killer") != null) {
            cost += 2000;
            costLog += " +2000(Evil/Psychopath)";
        }
        if (actor.traitContainer.GetNormalTrait<Trait>("Chaste") != null) {
            cost += -15;
            costLog += " -15(Chaste)";
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

    #region State Effects
    public void PrePraySuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustDoNotGetLonely(1);
    }
    public void PerTickPraySuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustHappiness(400);
    }
    public void AfterPraySuccess(ActualGoapNode goapNode) {
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
            if (actor.traitContainer.GetNormalTrait<Trait>("Evil") != null) {
                return false;
            }
            return actor == poiTarget;
        }
        return false;
    }
    #endregion
}

public class PrayData : GoapActionData {
    public PrayData() : base(INTERACTION_TYPE.PRAY) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        return actor == poiTarget;
    }
}
