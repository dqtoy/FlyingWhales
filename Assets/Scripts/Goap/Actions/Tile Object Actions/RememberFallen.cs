using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class RememberFallen : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.INDIRECT; } }

    public RememberFallen() : base(INTERACTION_TYPE.REMEMBER_FALLEN) {
        actionIconString = GoapActionStateDB.Entertain_Icon;
        isNotificationAnIntel = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Remember Success", goapNode);
    }
<<<<<<< Updated upstream
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        //**Cost**: randomize between 5-35
        return Utilities.rng.Next(5, 36);
=======
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = "\n" + name + " " + target.nameWithID + ":";
        int cost = Ruinarch.Utilities.rng.Next(80, 121);
        costLog += " +" + cost + "(Initial)";
        int numOfTimesActionDone = actor.jobComponent.GetNumOfTimesActionDone(this);
        if (numOfTimesActionDone > 5) {
            cost += 2000;
            costLog += " +2000(Times Reminisced > 5)";
        } else {
            int timesCost = 10 * numOfTimesActionDone;
            cost += timesCost;
            costLog += " +" + timesCost + "(10 x Times Reminisced)";
        }
        if (actor.traitContainer.GetNormalTrait<Trait>("Serial Killer") != null) {
            cost += 2000;
            costLog += " +2000(Psychopath)";
        }
        if (actor.moodComponent.moodState == MOOD_STATE.LOW || actor.moodComponent.moodState == MOOD_STATE.CRITICAL) {
            cost += -15;
            costLog += " -15(Low or Crit Mood)";
        }
        actor.logComponent.AppendCostLog(costLog);
        return cost;
>>>>>>> Stashed changes
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        LocationStructure targetStructure = node.targetStructure;
        log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        Tombstone tombstone = poiTarget as Tombstone;
        log.AddToFillers(tombstone.character, tombstone.character.name, LOG_IDENTIFIER.TARGET_CHARACTER); //Target character is only the identifier but it doesn't mean that this is a character, it can be item, etc.
        //if (targetStructure != null) {
        //    log.AddToFillers(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        //} else {
        //    log.AddToFillers(actor.specificLocation, actor.specificLocation.name, LOG_IDENTIFIER.LANDMARK_1);
        //}
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        actor.needsComponent.AdjustDoNotGetLonely(-1);
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (!poiTarget.IsAvailable() || poiTarget.gridTileLocation == null) {
                return false;
            }
            if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
                return false;
            }
            if (poiTarget is Tombstone) {
                Tombstone tombstone = poiTarget as Tombstone;
                Character target = tombstone.character;
                return actor.opinionComponent.GetRelationshipEffectWith(target) == RELATIONSHIP_EFFECT.POSITIVE;
            }
            return false;
        }
        return false;   
    }
    #endregion

    #region Effects
    public void PreRememberSuccess(ActualGoapNode goapNode) {
        Tombstone tombstone = goapNode.poiTarget as Tombstone;
        goapNode.descriptionLog.AddToFillers(null, tombstone.character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        goapNode.actor.needsComponent.AdjustDoNotGetLonely(1);
    }
    public void PerTickRememberSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustHappiness(500);
    }
    public void AfterRememberSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustDoNotGetLonely(-1);
    }
    //public void PreTargetMissing() {
    //    Tombstone tombstone = goapNode.poiTarget as Tombstone;
    //    goapNode.descriptionLog.AddToFillers(null, tombstone.character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
    #endregion
}

public class RememberFallenData : GoapActionData {
    public RememberFallenData() : base(INTERACTION_TYPE.REMEMBER_FALLEN) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (!poiTarget.IsAvailable() || poiTarget.gridTileLocation == null) {
            return false;
        }
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        if (poiTarget is Tombstone) {
            Tombstone tombstone = poiTarget as Tombstone;
            Character target = tombstone.character;
            return actor.opinionComponent.GetRelationshipEffectWith(target) == RELATIONSHIP_EFFECT.POSITIVE;
        }
        return false;
    }
}