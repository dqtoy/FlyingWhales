using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobQueue {
    public Character character { get; private set; } //If there is character it means that this job queue is a personal job queue
	public List<JobQueueItem> jobsInQueue { get; private set; }

    public bool isAreaJobQueue {
        get { return character == null; }
    }
    public JobQueue(Character character) {
        this.character = character;
        jobsInQueue = new List<JobQueueItem>();
    }

    public void AddJobInQueue(JobQueueItem job, bool isPriority = false, bool processLogicForPersonalJob = true) {
        job.SetJobQueueParent(this);
        job.SetIsPriority(isPriority);
        if (!isPriority) {
            jobsInQueue.Add(job);
        } else {
            jobsInQueue.Insert(0, job);
        }
        job.OnAddJobToQueue();

        if(processLogicForPersonalJob && character != null) {
            if((character.stateComponent.currentState != null && (character.stateComponent.currentState.characterState == CHARACTER_STATE.STROLL || character.stateComponent.currentState.characterState == CHARACTER_STATE.STROLL_OUTSIDE))
                || (character.currentAction != null && character.currentAction.goapType == INTERACTION_TYPE.RETURN_HOME && 
                (character.currentAction.parentPlan == null || character.currentAction.parentPlan.category == GOAP_CATEGORY.IDLE))) {
                character.jobQueue.ProcessFirstJobInQueue(character);
            }
        }
    }
    public bool RemoveJobInQueue(JobQueueItem job) {
        if (jobsInQueue.Remove(job)) {
            string removeLog = job.name + " has been removed from its job queue.";
            removeLog += "\nIs Personal: " + (character != null ? character.name : "False");
            removeLog += "\nAssigned Character: " + (job.assignedCharacter != null ? job.assignedCharacter.name : "None");
            if(job is GoapPlanJob) {
                GoapPlanJob planJob = job as GoapPlanJob;
                removeLog += "\nAssigned Plan: " + (planJob.assignedPlan != null);
            }
            Debug.Log(GameManager.Instance.TodayLogString() + removeLog);
            return job.OnRemoveJobFromQueue();
        }
        return false;
    }
    public void MoveJobToTopPriority(JobQueueItem job) {
        if (jobsInQueue.Remove(job)) {
            jobsInQueue.Insert(0, job);
        }
    }
    public bool IsJobInTopPriority(JobQueueItem job) {
        return jobsInQueue.Count > 0 && jobsInQueue[0] == job;
    }
    public bool ProcessFirstJobInQueue(Character characterToDoJob) {
        if(jobsInQueue.Count > 0) {
            for (int i = 0; i < jobsInQueue.Count; i++) {
                JobQueueItem job = jobsInQueue[i];
                if (job.assignedCharacter == null && job.CanCharacterTakeThisJob(characterToDoJob)) {
                    if (job.blacklistedCharacters.Contains(characterToDoJob)) {
                        continue;
                    }
                    job.SetAssignedCharacter(characterToDoJob);
                    if(job is GoapPlanJob) {
                        GoapPlanJob goapPlanJob = job as GoapPlanJob;
                        if(goapPlanJob.targetPlan != null) {
                            characterToDoJob.AddPlan(goapPlanJob.targetPlan);
                            goapPlanJob.SetAssignedPlan(goapPlanJob.targetPlan);
                        } else {
                            if (goapPlanJob.targetInteractionType != INTERACTION_TYPE.NONE) {
                                characterToDoJob.StartGOAP(goapPlanJob.targetInteractionType, goapPlanJob.targetPOI, GOAP_CATEGORY.WORK, false, null, true, goapPlanJob, goapPlanJob.otherData, goapPlanJob.allowDeadTargets);
                            } else {
                                characterToDoJob.StartGOAP(goapPlanJob.targetEffect, goapPlanJob.targetPOI, GOAP_CATEGORY.WORK, false, null, true, goapPlanJob, goapPlanJob.allowDeadTargets);
                            }
                        }
                    }else if (job is CharacterStateJob) {
                        CharacterStateJob stateJob = job as CharacterStateJob;
                        CharacterState newState = characterToDoJob.stateComponent.SwitchToState(stateJob.targetState, null, stateJob.targetArea);
                        if(newState != null) {
                            stateJob.SetAssignedState(newState);
                        } else {
                            throw new System.Exception(characterToDoJob.name + " tried doing state " + stateJob.targetState.ToString() + " but was unable to do so! This must not happen!");
                        }
                    }
                    return true;
                }
            }
        }
        return false;
    }
    public bool AssignCharacterToJob(JobQueueItem job, Character characterToDoJob) {
        if (job.assignedCharacter == null && job.CanCharacterTakeThisJob(characterToDoJob)) {
            if (job.blacklistedCharacters.Contains(characterToDoJob)) {
                return false;
            }
            job.SetAssignedCharacter(characterToDoJob);
            if (job is GoapPlanJob) {
                GoapPlanJob goapPlanJob = job as GoapPlanJob;
                if (goapPlanJob.targetPlan != null) {
                    characterToDoJob.AddPlan(goapPlanJob.targetPlan);
                    goapPlanJob.SetAssignedPlan(goapPlanJob.targetPlan);
                } else {
                    if (goapPlanJob.targetInteractionType != INTERACTION_TYPE.NONE) {
                        characterToDoJob.StartGOAP(goapPlanJob.targetInteractionType, goapPlanJob.targetPOI, GOAP_CATEGORY.WORK, false, null, true, goapPlanJob, goapPlanJob.otherData, goapPlanJob.allowDeadTargets);
                    } else {
                        characterToDoJob.StartGOAP(goapPlanJob.targetEffect, goapPlanJob.targetPOI, GOAP_CATEGORY.WORK, false, null, true, goapPlanJob, goapPlanJob.allowDeadTargets);
                    }
                }
            } else if (job is CharacterStateJob) {
                CharacterStateJob stateJob = job as CharacterStateJob;
                CharacterState newState = characterToDoJob.stateComponent.SwitchToState(stateJob.targetState, null, stateJob.targetArea);
                if (newState != null) {
                    stateJob.SetAssignedState(newState);
                } else {
                    throw new System.Exception(characterToDoJob.name + " tried doing state " + stateJob.targetState.ToString() + " but was unable to do so! This must not happen!");
                }
            }
            return true;
        }
        return false;
    }
    public void ForceAssignCharacterToJob(JobQueueItem job, Character characterToDoJob) {
        if (job.assignedCharacter == null) {
            job.SetAssignedCharacter(characterToDoJob);
            if (job is GoapPlanJob) {
                GoapPlanJob goapPlanJob = job as GoapPlanJob;
                if (goapPlanJob.targetInteractionType != INTERACTION_TYPE.NONE) {
                    characterToDoJob.StartGOAP(goapPlanJob.targetInteractionType, goapPlanJob.targetPOI, GOAP_CATEGORY.WORK, false, null, true, goapPlanJob, goapPlanJob.otherData);
                } else {
                    characterToDoJob.StartGOAP(goapPlanJob.targetEffect, goapPlanJob.targetPOI, GOAP_CATEGORY.WORK, false, null, true, goapPlanJob);
                }
            } else if (job is CharacterStateJob) {
                CharacterStateJob stateJob = job as CharacterStateJob;
                CharacterState newState = characterToDoJob.stateComponent.SwitchToState(stateJob.targetState);
                if (newState != null) {
                    stateJob.SetAssignedState(newState);
                } else {
                    throw new System.Exception(characterToDoJob.name + " tried doing state " + stateJob.targetState.ToString() + " but was unable to do so! This must not happen!");
                }
            }
        }
    }
    public void CancelAllJobsRelatedTo(GOAP_EFFECT_CONDITION conditionType, IPointOfInterest poi) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if(jobsInQueue[i] is GoapPlanJob) {
                GoapPlanJob job = jobsInQueue[i] as GoapPlanJob;
                if (job.targetEffect.conditionType == conditionType && job.targetEffect.targetPOI == poi) {
                    if (CancelJob(job)) {
                        i--;
                    }
                }
            }
        }
    }
    public void CancelAllJobsRelatedTo(CHARACTER_STATE state, Character actor) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i] is CharacterStateJob) {
                CharacterStateJob job = jobsInQueue[i] as CharacterStateJob;
                if (job.targetState == state && job.assignedCharacter == actor) {
                    if (CancelJob(job)) {
                        i--;
                    }
                }
            }
        }
    }
    public bool HasJob(JobQueueItem job) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (job == jobsInQueue[i]) {
                return true;
            }
        }
        return false;
    }
    public bool HasJob(string jobName) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i].name == jobName) {
                return true;
            }
        }
        return false;
    }
    public bool HasJob(string jobName, IPointOfInterest targetPOI) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if(jobsInQueue[i].name == jobName && jobsInQueue[i] is GoapPlanJob) {
                GoapPlanJob job = jobsInQueue[i] as GoapPlanJob;
                if (job.targetPOI == targetPOI) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasJobWithOtherData(string jobName, object otherData) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i].name == jobName && jobsInQueue[i] is GoapPlanJob) {
                GoapPlanJob job = jobsInQueue[i] as GoapPlanJob;
                if(job.allOtherData != null) {
                    for (int j = 0; j < job.allOtherData.Count; j++) {
                        object data = job.allOtherData[j];
                        if(data == otherData) {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
    public bool HasJobRelatedTo(GOAP_EFFECT_CONDITION conditionType, IPointOfInterest poi) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i] is GoapPlanJob) {
                GoapPlanJob job = jobsInQueue[i] as GoapPlanJob;
                if (job.targetEffect.conditionType == conditionType && job.targetEffect.targetPOI == poi) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasJobRelatedTo(GOAP_EFFECT_CONDITION conditionType, object conditionKey, IPointOfInterest poi) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i] is GoapPlanJob) {
                GoapPlanJob job = jobsInQueue[i] as GoapPlanJob;
                if (job.targetEffect.conditionType == conditionType && job.targetEffect.conditionKey == conditionKey && job.targetEffect.targetPOI == poi) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasJobRelatedTo(CHARACTER_STATE state) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i] is CharacterStateJob) {
                CharacterStateJob job = jobsInQueue[i] as CharacterStateJob;
                if (job.targetState == state) {
                    return true;
                }
            }
        }
        return false;
    }
    public JobQueueItem GetJob(string jobName, IPointOfInterest targetPOI) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i].name == jobName && jobsInQueue[i] is GoapPlanJob) {
                GoapPlanJob job = jobsInQueue[i] as GoapPlanJob;
                if (job.targetPOI == targetPOI) {
                    return job;
                }
            }
        }
        return null;
    }
    public JobQueueItem GetJob(string jobName) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i].name == jobName) {
                return jobsInQueue[i];
            }
        }
        return null;
    }
    public int GetNumberOfJobsWith(CHARACTER_STATE state) {
        int count = 0;
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i] is CharacterStateJob) {
                CharacterStateJob job = jobsInQueue[i] as CharacterStateJob;
                if (job.targetState == state) {
                    count++;
                }
            }
        }
        return count;
    }
    public void CancelAllJobs(string jobName) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if(jobsInQueue[i].name == jobName) {
                if (CancelJob(jobsInQueue[i])) {
                    i--;
                }
            }
        }
    }
    public bool CancelJob(JobQueueItem job, string cause = "", bool shouldDoAfterEffect = true) {
        if (!job.cannotCancelJob) {
            if(job is GoapPlanJob && cause != "") {
                Character actor = null;
                if(this.character != null) {
                    actor = this.character;
                } else if(job.assignedCharacter != null) {
                    actor = job.assignedCharacter;
                }
                if(actor != null) {
                    actor.RegisterLogAndShowNotifToThisCharacterOnly("Generic", "job_cancelled_cause", null, cause);
                }
            }
            job.UnassignJob(shouldDoAfterEffect);
            return RemoveJobInQueue(job);
        }
        return false;
    }
    /// <summary>
    /// Unassign all jobs that a certain character has taken.
    /// </summary>
    /// <param name="character">The character in question.</param>
    public void UnassignAllJobsTakenBy(Character character) {
        string summary = "Unassigning all jobs taken by " + character.name;
        List<JobQueueItem> allJobs = new List<JobQueueItem>(jobsInQueue);
        for (int i = 0; i < allJobs.Count; i++) {
            JobQueueItem currJob = allJobs[i];
            if (currJob.assignedCharacter == character) {
                summary += "\nUnassigned " + character.name + " from job " + currJob.name; 
                currJob.UnassignJob(false);
            }
        }
        character.PrintLogIfActive(summary);
    }
}
