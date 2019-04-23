using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobQueue {
    public Character character { get; private set; }
	public List<JobQueueItem> jobsInQueue { get; private set; }

    public JobQueue(Character character) {
        this.character = character;
        jobsInQueue = new List<JobQueueItem>();
    }

    public void AddJobInQueue(JobQueueItem job, bool isPriority = false) {
        job.SetJobQueueParent(this);
        if (!isPriority) {
            jobsInQueue.Add(job);
        } else {
            jobsInQueue.Insert(0, job);
        }
        job.OnAddJobToQueue();
    }
    public bool RemoveJobInQueue(JobQueueItem job) {
        if (jobsInQueue.Remove(job)) {
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
                    job.SetAssignedCharacter(characterToDoJob);
                    if(job is GoapPlanJob) {
                        GoapPlanJob goapPlanJob = job as GoapPlanJob;
                        if (goapPlanJob.targetInteractionType != INTERACTION_TYPE.NONE) {
                            characterToDoJob.StartGOAP(goapPlanJob.targetInteractionType, goapPlanJob.targetPOI, GOAP_CATEGORY.WORK, false, null, true, goapPlanJob, goapPlanJob.otherData);
                        } else {
                            characterToDoJob.StartGOAP(goapPlanJob.targetEffect, goapPlanJob.targetPOI, GOAP_CATEGORY.WORK, false, null, true, goapPlanJob);
                        }
                    }else if (job is CharacterStateJob) {
                        CharacterStateJob stateJob = job as CharacterStateJob;
                        CharacterState newState = characterToDoJob.stateComponent.SwitchToState(stateJob.targetState);
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
    public void AssignCharacterToJob(JobQueueItem job, Character characterToDoJob) {
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
                    Log addLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "plan_cancelled_cause");
                    addLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    addLog.AddToFillers(null, cause, LOG_IDENTIFIER.STRING_1);
                    addLog.AddLogToInvolvedObjects();
                    if (PlayerManager.Instance.player.ShouldShowNotificationFrom(actor)) {
                        PlayerManager.Instance.player.ShowNotification(addLog);
                    }
                }
            }
            job.UnassignJob(shouldDoAfterEffect);
            return RemoveJobInQueue(job);
        }
        return false;
    }
}
