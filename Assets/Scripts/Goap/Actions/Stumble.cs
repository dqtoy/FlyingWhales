using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stumble : GoapAction {

    public Stumble(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.STUMBLE, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.No_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        isNotificationAnIntel = false;
        animationName = "Sleep Ground";
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        SetState("Stumble Success");
    }
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    protected override int GetCost() {
        return 10;
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    public override void OnResultReturnedToActor() {
        base.OnResultReturnedToActor();
        if (currentState.name == "Stumble Success") {
            if (actor.currentHP <= 0) {
                actor.Death(deathFromAction: this);
            }
        }
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        return actor == poiTarget;
    }
    #endregion

    #region State Effects
    private void PreStumbleSuccess() {
        currentState.SetIntelReaction(SuccessReactions);
    }
    private void PerTickStumbleSuccess() {
        int randomHpToLose = UnityEngine.Random.Range(1, 6);
        float percentMaxHPToLose = randomHpToLose / 100f;
        int actualHPToLose = Mathf.CeilToInt(actor.maxHP * percentMaxHPToLose);

        actor.AdjustHP(-actualHPToLose);
    }
    #endregion

    #region Intel Reactions
    private List<string> SuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        if (status == SHARE_INTEL_STATUS.WITNESSED) {
            RELATIONSHIP_EFFECT relWithActor = recipient.GetRelationshipEffectWith(actor);
            if (relWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
                if (recipient.HasRelationshipOfTypeWith(actorAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
                    CreateLaughAtJob(recipient, actor);
                }
            } else if (relWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
                if (recipient.HasRelationshipOfTypeWith(actorAlterEgo, false, RELATIONSHIP_TRAIT.PARAMOUR, RELATIONSHIP_TRAIT.LOVER)) {
                    //If they are lovers, paramours or relatives and they saw the other: -stumbled
                    //They will trigger a personal https://trello.com/c/iDsfwQ7d/2845-character-feeling-concerned job
                    CreateFeelingConcernedJob(recipient, actor);
                } else if (recipient.HasRelationshipOfTypeWith(actorAlterEgo, RELATIONSHIP_TRAIT.FRIEND)) {
                    //50% they will trigger a personal https://trello.com/c/iDsfwQ7d/2845-character-feeling-concerned job
                    if (Random.Range(0, 100) < 50) {
                        CreateFeelingConcernedJob(recipient, actor);
                    }
                    //50 % they will trigger a personal https://trello.com/c/Gz12n7Af/2847-character-tease job
                    else {
                        CreateTeaseJob(recipient, actor);
                    }
                }
            }
        }
        return reactions;
    }
    #endregion

    #region Check Up
    private bool CreateLaughAtJob(Character characterThatWillDoJob, Character target) {
        if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.MISC, INTERACTION_TYPE.LAUGH_AT)) {
            GoapPlanJob laughJob = new GoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.LAUGH_AT, target);
            characterThatWillDoJob.jobQueue.AddJobInQueue(laughJob);
            return true;
        }
        return false;
    }
    private bool CreateFeelingConcernedJob(Character characterThatWillDoJob, Character target) {
        if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.MISC, INTERACTION_TYPE.FEELING_CONCERNED)) {
            GoapPlanJob laughJob = new GoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.FEELING_CONCERNED, target);
            characterThatWillDoJob.jobQueue.AddJobInQueue(laughJob);
            return true;
        }
        return false;
    }
    private bool CreateTeaseJob(Character characterThatWillDoJob, Character target) {
        if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.MISC, INTERACTION_TYPE.TEASE)) {
            GoapPlanJob laughJob = new GoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.TEASE, target);
            characterThatWillDoJob.jobQueue.AddJobInQueue(laughJob);
            return true;
        }
        return false;
    }
    #endregion
}

public class StumbleData : GoapActionData {
    public StumbleData() : base(INTERACTION_TYPE.STUMBLE) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor == poiTarget;
    }
}