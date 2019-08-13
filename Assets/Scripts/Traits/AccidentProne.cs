using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AccidentProne : Trait {

    public Character owner { get; private set; }
    public CharacterState storedState { get; private set; }

    public AccidentProne() {
        name = "Accident Prone";
        description = "This character is prone to accidents.";
        type = TRAIT_TYPE.FLAW;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ACCIDENT, INTERACTION_TYPE.STUMBLE };
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;

    }

    #region Overrides
    public override void OnAddTrait(ITraitable sourceCharacter) {
        base.OnAddTrait(sourceCharacter);
        if (sourceCharacter is Character) {
            owner = sourceCharacter as Character;
        }
    }
    public override bool PerTickOwnerMovement() {
        int stumbleChance = UnityEngine.Random.Range(0, 100);
        bool hasCreatedJob = false;
        if(stumbleChance < 2) {
            if(owner.currentAction == null || (owner.currentAction.goapType != INTERACTION_TYPE.STUMBLE && owner.currentAction.goapType != INTERACTION_TYPE.ACCIDENT)) {
                GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.STUMBLE, owner, owner);

                GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
                GoapPlan plan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.REACTION);
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.STUMBLE, owner);
                plan.ConstructAllNodes();
                plan.SetDoNotRecalculate(true);
                job.SetAssignedPlan(plan);
                job.SetAssignedCharacter(owner);
                job.SetCancelOnFail(true);

                owner.jobQueue.AddJobInQueue(job, false);

                owner.AdjustIsWaitingForInteraction(1);
                if (owner.currentParty.icon.isTravelling) {
                    owner.marker.StopMovement();
                }
                if (owner.currentAction != null) {
                    owner.StopCurrentAction(false);
                }
                if (owner.stateComponent.currentState != null) {
                    storedState = owner.stateComponent.currentState;
                    owner.stateComponent.currentState.PauseState();
                    goapAction.SetEndAction(ResumePausedState);
                } else if (owner.stateComponent.stateToDo != null) {
                    storedState = owner.stateComponent.stateToDo;
                    owner.stateComponent.SetStateToDo(null, false, false);
                    goapAction.SetEndAction(ResumeStateToDoState);
                }
                owner.AdjustIsWaitingForInteraction(-1);

                owner.AddPlan(plan, true, false);
                owner.PerformGoapPlans();

                hasCreatedJob = true;
            }
        }
        return hasCreatedJob;
    }
    public override bool OnStartPerformGoapAction(GoapAction action, ref bool willStillContinueAction) {
        int accidentChance = UnityEngine.Random.Range(0, 100);
        bool hasCreatedJob = false;
        if (accidentChance < 10) {
            if (action != null && !AttributeManager.Instance.excludedActionsFromAccidentProneTrait.Contains(action.goapType)) {
                GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.ACCIDENT, owner, owner);
                goapAction.InitializeOtherData(new object[] { action });

                GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
                GoapPlan plan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.REACTION);
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.ACCIDENT, owner);
                plan.ConstructAllNodes();
                plan.SetDoNotRecalculate(true);
                job.SetAssignedPlan(plan);
                job.SetAssignedCharacter(owner);
                job.SetCancelOnFail(true);

                owner.jobQueue.AddJobInQueue(job, false);

                owner.AdjustIsWaitingForInteraction(1);
                if (owner.currentParty.icon.isTravelling) {
                    owner.marker.StopMovement();
                }
                action.StopAction(true);
                owner.AdjustIsWaitingForInteraction(-1);

                owner.AddPlan(plan, true, false);
                owner.PerformGoapPlans();

                hasCreatedJob = true;
                willStillContinueAction = false;
            }
        }
        return hasCreatedJob;
    }
    #endregion

    private void ResumePausedState(string result, GoapAction action) {
        owner.GoapActionResult(result, action);
        storedState.ResumeState();
    }
    private void ResumeStateToDoState(string result, GoapAction action) {
        owner.GoapActionResult(result, action);
        owner.stateComponent.SetStateToDo(storedState);
    }
}
