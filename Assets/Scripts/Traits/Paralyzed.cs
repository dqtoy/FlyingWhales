using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paralyzed : Trait {

    public Character character { get; private set; }
    public Paralyzed() {
        name = "Paralyzed";
        description = "This character is paralyzed.";
        thoughtText = "[Character] is paralyzed.";
        type = TRAIT_TYPE.DISABLER;
        effect = TRAIT_EFFECT.NEGATIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.FEED, };
        daysDuration = 0;
    }

    #region Overrides
    public override void OnAddTrait(ITraitable addedTo) {
        base.OnAddTrait(addedTo);
        if (addedTo is Character) {
            character = addedTo as Character;
            character.CancelAllJobsAndPlans();
            Messenger.AddListener(Signals.TICK_STARTED, CheckParalyzedTrait);
        }
    }
    public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
        if (character != null) {
            Messenger.RemoveListener(Signals.TICK_STARTED, CheckParalyzedTrait);
        }
        base.OnRemoveTrait(sourceCharacter, removedBy);
    }
    #endregion

    private void CheckParalyzedTrait() {
        if (!PlanFullnessRecovery()) {
            //Plan tiredeness
            //Plan happiness
        }
    }

    #region Fullness Recovery
    private bool PlanFullnessRecovery() {
        if(character.isStarving || character.isHungry) {
            return CreateFeedJob();
        }
        return false;
    }
    private bool CreateFeedJob() {
        if (!character.HasJobTargettingThisCharacter(JOB_TYPE.FEED) && character.specificLocation.IsResident(character)) {
            GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, targetPOI = character };
            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.FEED, goapEffect);
            job.SetCanTakeThisJobChecker(CanCharacterTakeFeedJob);
            character.specificLocation.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;
    }
    private bool CanCharacterTakeFeedJob(Character character, JobQueueItem job) {
        return this.character.faction == character.faction && character.GetRelationshipEffectWith(this.character) != RELATIONSHIP_EFFECT.NEGATIVE;
    }
    #endregion

}
