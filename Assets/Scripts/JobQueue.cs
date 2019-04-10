using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobQueue {
	public List<JobQueueItem> jobsInQueue { get; private set; }

    public JobQueue() {
        jobsInQueue = new List<JobQueueItem>();
    }

    public void AddJobInQueue(JobQueueItem job) {
        job.SetJobQueueParent(this);
        jobsInQueue.Add(job);
    }
    public bool RemoveJobInQueue(JobQueueItem job) {
        return jobsInQueue.Remove(job);
    }
    public bool ProcessFirstJobInQueue(Character characterToDoJob) {
        if(jobsInQueue.Count > 0) {
            for (int i = 0; i < jobsInQueue.Count; i++) {
                JobQueueItem job = jobsInQueue[i];
                if (job.assignedCharacter == null && job.CanCharacterTakeThisJob(characterToDoJob)) {
                    job.SetAssignedCharacter(characterToDoJob);
                    characterToDoJob.StartGOAP(job.targetEffect, job.targetPOI, GOAP_CATEGORY.WORK, false, null, true, job);
                    return true;
                }
            }
        }
        return false;
    }
    public void CancelAllJobsRelatedTo(GOAP_EFFECT_CONDITION conditionType, IPointOfInterest poi) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            JobQueueItem job = jobsInQueue[i];
            if(job.targetEffect.conditionType == conditionType && job.targetEffect.targetPOI == poi) {
                if (CancelJob(job)) {
                    i--;
                }
            }
        }
    }
    public bool HasJobRelatedTo(GOAP_EFFECT_CONDITION conditionType, IPointOfInterest poi) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            JobQueueItem job = jobsInQueue[i];
            if (job.targetEffect.conditionType == conditionType && job.targetEffect.targetPOI == poi) {
                return true;
            }
        }
        return false;
    }
    public bool CancelJob(JobQueueItem job) {
        UnassignJob(job);
        return RemoveJobInQueue(job);
    }
    public void UnassignJob(JobQueueItem job) {
        if (job.assignedPlan != null && job.assignedCharacter != null) {
            job.assignedCharacter.AdjustIsWaitingForInteraction(1);
            if (job.assignedCharacter.currentAction != null && job.assignedCharacter.currentAction.parentPlan == job.assignedPlan) {
                job.assignedCharacter.marker.StopMovementOnly();
                if (job.assignedCharacter.currentAction.isPerformingActualAction && !job.assignedCharacter.currentAction.isDone) {
                    job.assignedCharacter.currentAction.currentState.EndPerTickEffect();
                }
                job.assignedCharacter.SetCurrentAction(null);
                job.assignedCharacter.DropPlan(job.assignedPlan);
            } else {
                job.assignedCharacter.DropPlan(job.assignedPlan);
            }
            job.assignedCharacter.AdjustIsWaitingForInteraction(-1);
            job.SetAssignedCharacter(null);
            job.SetAssignedPlan(null);
        }
    }
}
