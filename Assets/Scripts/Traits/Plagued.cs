using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plagued : Trait {

    public Character owner { get; private set; } //poi that has the poison

    private float pukeChance;
    private float septicChance;

    private GoapAction stoppedAction;
    private CharacterState pausedState;

    public Plagued() {
        name = "Plagued";
        description = "This character is Plagued.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEGATIVE;
        daysDuration = GameManager.ticksPerDay * 3;
    }

    #region Overrides
    public override void OnAddTrait(ITraitable sourceCharacter) {
        base.OnAddTrait(sourceCharacter);
        if (sourceCharacter is Character) {
            owner = sourceCharacter as Character;
        }
    }
    protected override void OnChangeLevel() {
        if (level == 1) {
            pukeChance = 5f;
            septicChance = 0.5f;
        } else if (level == 2) {
            pukeChance = 7f;
            septicChance = 1f;
        } else {
            pukeChance = 9f;
            septicChance = 1.5f;
        }
    }
    public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
        if (traitOwner is Character) {
            Character targetCharacter = traitOwner as Character;
            if (!targetCharacter.isDead && !targetCharacter.HasJobTargettingThisCharacter(JOB_TYPE.REMOVE_TRAIT, name) && !targetCharacter.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
                GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = name, targetPOI = targetCharacter };
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REMOVE_TRAIT, goapEffect);
                if (CanCharacterTakeRemoveIllnessesJob(characterThatWillDoJob, targetCharacter, null)) {
                    characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                    return true;
                } else {
                    if (!IsResponsibleForTrait(characterThatWillDoJob)) {
                        job.SetCanTakeThisJobChecker(CanCharacterTakeRemoveIllnessesJob);
                        characterThatWillDoJob.specificLocation.jobQueue.AddJobInQueue(job);
                    }
                    return false;
                }
            }
        }
        return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
    }
    public override bool PerTickOwnerMovement() {
        string summary = owner.name + " is rolling for plagued chances....";
        float pukeRoll = Random.Range(0f, 100f);
        float septicRoll = Random.Range(0f, 100f);
        summary += "\nPuke roll is: " + pukeRoll.ToString();
        summary += "\nSeptic Shock roll is: " + septicRoll.ToString();
        bool hasCreatedJob = false;
        if (pukeRoll < pukeChance) {
            summary += "\nPuke chance met. Doing puke action.";
            //do puke action
            if (owner.currentAction != null && owner.currentAction.goapType != INTERACTION_TYPE.PUKE) {
                stoppedAction = owner.currentAction;
                owner.StopCurrentAction(false);
                owner.marker.StopMovement();

                GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.PUKE, owner, owner);

                GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
                GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.REACTION);
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.PUKE);
                job.SetAssignedPlan(goapPlan);
                goapPlan.ConstructAllNodes();

                goapAction.CreateStates();
                owner.SetCurrentAction(goapAction);
                owner.currentAction.SetEndAction(ResumeLastAction);
                owner.currentAction.DoAction();
                hasCreatedJob = true;
            } else if (owner.stateComponent.currentState != null) {
                pausedState = owner.stateComponent.currentState;
                owner.stateComponent.currentState.PauseState();
                owner.marker.StopMovement();
                GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.PUKE, owner, owner);

                GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
                GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.REACTION);
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.PUKE);
                job.SetAssignedPlan(goapPlan);
                goapPlan.ConstructAllNodes();

                goapAction.CreateStates();
                owner.SetCurrentAction(goapAction);
                owner.currentAction.SetEndAction(ResumePausedState);
                owner.currentAction.DoAction();
                hasCreatedJob = true;
            } else {
                GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.PUKE, owner, owner);

                GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
                GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.REACTION);
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.PUKE);
                job.SetAssignedPlan(goapPlan);
                goapPlan.ConstructAllNodes();

                goapAction.CreateStates();
                owner.SetCurrentAction(goapAction);
                owner.currentAction.DoAction();
                hasCreatedJob = true;
            }
            Debug.Log(summary);
        } else if (septicRoll < septicChance) {
            summary += "\nSeptic Shock chance met. Doing septic shock action.";
            if (owner.currentAction != null && owner.currentAction.goapType != INTERACTION_TYPE.SEPTIC_SHOCK) {
                stoppedAction = owner.currentAction;
                owner.StopCurrentAction(false);
                owner.marker.StopMovement();
                GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.SEPTIC_SHOCK, owner, owner);

                GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
                GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.REACTION);
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.SEPTIC_SHOCK);
                job.SetAssignedPlan(goapPlan);
                goapPlan.ConstructAllNodes();

                goapAction.CreateStates();
                owner.SetCurrentAction(goapAction);
                owner.currentAction.DoAction();
                hasCreatedJob = true;
            } else if (owner.stateComponent.currentState != null) {
                owner.stateComponent.currentState.OnExitThisState();
                owner.marker.StopMovement();
                GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.SEPTIC_SHOCK, owner, owner);

                GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
                GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.REACTION);
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.SEPTIC_SHOCK);
                job.SetAssignedPlan(goapPlan);
                goapPlan.ConstructAllNodes();

                goapAction.CreateStates();
                owner.SetCurrentAction(goapAction);
                owner.currentAction.DoAction();
                hasCreatedJob = true;
            } else {
                GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.SEPTIC_SHOCK, owner, owner);

                GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
                GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.REACTION);
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.SEPTIC_SHOCK);
                job.SetAssignedPlan(goapPlan);
                goapPlan.ConstructAllNodes();

                goapAction.CreateStates();
                owner.SetCurrentAction(goapAction);
                owner.currentAction.DoAction();
                hasCreatedJob = true;
            }
            Debug.Log(summary);
        }
        return hasCreatedJob;
    }
    #endregion

    private void ResumeLastAction(string result, GoapAction action) {
        if (stoppedAction.CanSatisfyRequirements()) {
            stoppedAction.DoAction();
        } else {
            owner.GoapActionResult(result, action);
        }
        
    }
    private void ResumePausedState(string result, GoapAction action) {
        owner.GoapActionResult(result, action);
        pausedState.ResumeState();
    }

    public int GetChatInfectChance() {
        if (level == 1) {
            return 25;
        } else if (level == 2) {
            return 35;
        } else {
            return 45;
        }
    }
    public int GetMakeLoveInfectChance() {
        if (level == 1) {
            return 50;
        } else if (level == 2) {
            return 75;
        } else {
            return 100;
        }
    }
}
