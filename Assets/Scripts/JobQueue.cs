using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobQueue {
    public Character character { get; private set; } //If there is character it means that this job queue is a personal job queue
    public Quest quest { get; private set; } //If there is a quest it means that this is a quest job

    public List<JobQueueItem> jobsInQueue { get; private set; }

    public bool isAreaOrQuestJobQueue {
        get { return character == null; }
    }
    public JobQueue(Character character) {
        this.character = character;
        jobsInQueue = new List<JobQueueItem>();
    }

    //NOTE: IMPROVE THIS! PROBABLY PUT THIS IN CONSTRUCTOR JUST LIKE WITH THE CHARACTER
    public void SetQuest(Quest quest) {
        this.quest = quest;
    }

    public void AddJobInQueue(JobQueueItem job, bool processLogicForPersonalJob = true) {
        job.SetJobQueueParent(this);
        bool hasBeenInserted = false;
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if(job.priority < jobsInQueue[i].priority) {
                jobsInQueue.Insert(i, job);
                hasBeenInserted = true;
                break;
            }
        }
        if (!hasBeenInserted) {
            jobsInQueue.Add(job);
        }
        job.OnAddJobToQueue();
        if(quest != null) {
            quest.OnAddJob(job);
        }

        if(!isAreaOrQuestJobQueue) {
            //bool hasProcessed = false;

            //If the current action's job of the character is overridable and the added job has higher priority than it,
            //then process that job immediately and stop what the character is currently doing
            //Sometimes, in rare cases, the character still cannot be assigned to a job even if it is a personal job,
            //It might be because CanTakeJob/CanCharacterTakeThisJob function is not satisfied
            if (processLogicForPersonalJob) {
                if (character.CanCurrentJobBeOverriddenByJob(job)) {
                    AssignCharacterToJobAndCancelCurrentAction(job, character);
                } else {
                    //If a character is a higher priority than one of the currently existing jobs
                    //Check all currently existing plans of character
                    //All plans' job of character that has lower priority than the added job must be dropped so that the job will be prioritized than those 
                    if (hasBeenInserted) {
                        for (int i = 0; i < character.allGoapPlans.Count; i++) {
                            GoapPlan plan = character.allGoapPlans[i];
                            if(plan.job == null || (job.priority < plan.job.priority)) {
                                if(plan.job == null) {
                                    character.JustDropPlan(plan);
                                } else {
                                    if (plan.job.jobQueueParent.isAreaOrQuestJobQueue) {
                                        plan.job.UnassignJob(false);
                                    } else {
                                        character.JustDropPlan(plan);
                                    }
                                }
                                i--;
                            }
                        }
                    }
                }
            }
            //if (processLogicForPersonalJob && !hasProcessed) {
            //    if ((character.stateComponent.currentState != null && (character.stateComponent.currentState.characterState == CHARACTER_STATE.STROLL || character.stateComponent.currentState.characterState == CHARACTER_STATE.STROLL_OUTSIDE))
            //        || (character.currentAction != null && character.currentAction.goapType == INTERACTION_TYPE.RETURN_HOME &&
            //        (character.currentAction.parentPlan == null || character.currentAction.parentPlan.category == GOAP_CATEGORY.IDLE))) {
            //        character.jobQueue.ProcessFirstJobInQueue(character);
            //    }
            //}
        }
    }
    public bool RemoveJobInQueue(JobQueueItem job) {
        if (jobsInQueue.Remove(job)) {
            string ownerName = "area";
            if (!isAreaOrQuestJobQueue) {
                ownerName = character.name;
            }
            string removeLog = job.name + " has been removed from " + ownerName + " job queue.";
            //removeLog += "\nIs Personal: " + (character != null ? character.name : "False");
            //removeLog += "\nAssigned Character: " + (job.assignedCharacter != null ? job.assignedCharacter.name : "None");
            //if(job is GoapPlanJob) {
            //    GoapPlanJob planJob = job as GoapPlanJob;
            //removeLog += "\nAssigned Plan: " + (planJob.assignedPlan != null);
            //}
            Debug.Log(GameManager.Instance.TodayLogString() + removeLog);
            if (quest != null) {
                quest.OnRemoveJob(job);
            }
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
    public JobQueueItem GetFirstUnassignedJobInQueue(Character characterToDoJob) {
        if (jobsInQueue.Count > 0) {
            for (int i = 0; i < jobsInQueue.Count; i++) {
                JobQueueItem job = jobsInQueue[i];
                if (CanAssignCharacterToJob(job, characterToDoJob)) {
                    return job;
                }
            }
        }
        return null;
    }
    public bool ProcessFirstJobInQueue(Character characterToDoJob) {
        if(jobsInQueue.Count > 0) {
            for (int i = 0; i < jobsInQueue.Count; i++) {
                JobQueueItem job = jobsInQueue[i];
                if(!AssignCharacterToJob(job, characterToDoJob)) {
                    continue;
                } else {
                    return true;
                }
            }
        }
        return false;
    }
    public void AssignCharacterToJobAndCancelCurrentAction(JobQueueItem job, Character character) {
        if (AssignCharacterToJob(job, character)) {
            if (job is CharacterStateJob) {
                //Will no longer stop what is currently doing if job is a state job because it will already be done by that state
                return;
            }
            if (character.stateComponent.currentState != null) {
                character.stateComponent.currentState.OnExitThisState();
                //This call is doubled so that it will also exit the previous major state if there's any
                if (character.stateComponent.currentState != null) {
                    character.stateComponent.currentState.OnExitThisState();
                }
            } else if (character.stateComponent.stateToDo != null) {
                character.stateComponent.SetStateToDo(null);
            } else {
                if (character.currentParty.icon.isTravelling) {
                    if (character.currentParty.icon.travelLine == null) {
                        character.marker.StopMovement();
                    } else {
                        character.currentParty.icon.SetOnArriveAction(() => character.OnArriveAtAreaStopMovement());
                    }
                }
                character.AdjustIsWaitingForInteraction(1);
                character.StopCurrentAction(false);
                character.AdjustIsWaitingForInteraction(-1);
            }
        }
    }
    public bool AssignCharacterToJob(JobQueueItem job, Character characterToDoJob) {
        if (CanAssignCharacterToJob(job, characterToDoJob)) {
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
                        characterToDoJob.StartGOAP(goapPlanJob.targetEffect, goapPlanJob.targetPOI, GOAP_CATEGORY.WORK, false, null, true, goapPlanJob, goapPlanJob.otherData, goapPlanJob.allowDeadTargets);
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
    public bool CanAssignCharacterToJob(JobQueueItem job, Character characterToDoJob) {
        return job.assignedCharacter == null && job.CanCharacterTakeThisJob(characterToDoJob) && !job.blacklistedCharacters.Contains(characterToDoJob);
    }
    public void ForceAssignCharacterToJob(JobQueueItem job, Character characterToDoJob) {
        if (job.assignedCharacter == null) {
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
                        characterToDoJob.StartGOAP(goapPlanJob.targetEffect, goapPlanJob.targetPOI, GOAP_CATEGORY.WORK, false, null, true, goapPlanJob, goapPlanJob.otherData, goapPlanJob.allowDeadTargets);
                    }
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
    public bool HasJob(params JOB_TYPE[] jobTypes) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            for (int j = 0; j < jobTypes.Length; j++) {
                if (jobsInQueue[i].jobType == jobTypes[j]) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasJob(JOB_TYPE jobType, IPointOfInterest targetPOI) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if(jobsInQueue[i].jobType == jobType && jobsInQueue[i] is GoapPlanJob) {
                GoapPlanJob job = jobsInQueue[i] as GoapPlanJob;
                if (job.targetPOI == targetPOI) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasJob(JOB_TYPE jobType, INTERACTION_TYPE targetGoapActionType) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i].jobType == jobType && jobsInQueue[i] is GoapPlanJob) {
                GoapPlanJob job = jobsInQueue[i] as GoapPlanJob;
                if (job.targetInteractionType == targetGoapActionType) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasJobWithOtherData(JOB_TYPE jobType, object otherData) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i].jobType == jobType && jobsInQueue[i] is GoapPlanJob) {
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
    public JobQueueItem GetJob(JOB_TYPE jobType, IPointOfInterest targetPOI) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i].jobType == jobType && jobsInQueue[i] is GoapPlanJob) {
                GoapPlanJob job = jobsInQueue[i] as GoapPlanJob;
                if (job.targetPOI == targetPOI) {
                    return job;
                }
            }
        }
        return null;
    }
    public JobQueueItem GetJob(params JOB_TYPE[] jobTypes) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            for (int j = 0; j < jobTypes.Length; j++) {
                if (jobsInQueue[i].jobType == jobTypes[j]) {
                    return jobsInQueue[i];
                }
            }
        }
        return null;
    }
    public JobQueueItem GetJobByID(int id) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i].id == id) {
                return jobsInQueue[i];
            }
        }
        return null;
    }
    public List<JobQueueItem> GetJobs(JOB_TYPE jobType) {
        List<JobQueueItem> jobs = new List<JobQueueItem>();
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i].jobType == jobType) {
                jobs.Add(jobsInQueue[i]);
            }
        }
        return jobs;
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
    public int GetNumberOfJobsWith(JOB_TYPE type) {
        int count = 0;
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (jobsInQueue[i].jobType == type) {
                count++;
            }
        }
        return count;
    }
    /// <summary>
    /// Get the number of jobs that return true from the provided checker.
    /// </summary>
    /// <param name="checker">The function that checks if the item is valid</param>
    /// <returns></returns>
    public int GetNumberOfJobsWith(System.Func<JobQueueItem, bool> checker) {
        int count = 0;
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (checker.Invoke(jobsInQueue[i])) {
                count++;
            }
        }
        return count;
    }
    public void CancelAllJobs(JOB_TYPE jobType) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if(jobsInQueue[i].jobType == jobType) {
                if (CancelJob(jobsInQueue[i])) {
                    i--;
                }
            }
        }
    }
    public void CancelAllJobs(params JOB_TYPE[] jobTypes) {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            for (int j = 0; j < jobTypes.Length; j++) {
                if (jobsInQueue[i].jobType == jobTypes[j]) {
                    if (CancelJob(jobsInQueue[i])) {
                        i--;
                    }
                    break;
                }
            }
        }
    }
    public void CancelAllJobs() {
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if (CancelJob(jobsInQueue[i])) {
                i--;
            }
        }
    }

    //Returnsc true or false if job was really removed in queue
    public bool CancelJob(JobQueueItem job, string cause = "", bool shouldDoAfterEffect = true) {
        //When cancelling a job, we must check if it's personal or not because if it is a faction/settlement job it cannot be removed from queue
        //The only way for a faction/settlement job to be removed is if it is forced or it is actually finished
        //We don't remove them, but they will be considered as removed so that other processes will be triggered
        bool hasBeenRemovedInJobQueue = false;
        bool process = false;
        if (job.jobQueueParent.isAreaOrQuestJobQueue) {
            process = true;
        } else {
            hasBeenRemovedInJobQueue = RemoveJobInQueue(job);
            process = hasBeenRemovedInJobQueue;
        }
        if (process) {
            if(job is GoapPlanJob && cause != "") {
                GoapPlanJob planJob = job as GoapPlanJob;
                Character actor = null;
                if(!isAreaOrQuestJobQueue) {
                    actor = this.character;
                } else if(job.assignedCharacter != null) {
                    actor = job.assignedCharacter;
                }
                if(actor != null && actor != planJob.targetPOI) { //only log if the actor is not the same as the target poi.
                    actor.RegisterLogAndShowNotifToThisCharacterOnly("Generic", "job_cancelled_cause", null, cause);
                }
            }
            job.UnassignJob(shouldDoAfterEffect);
        }
        return hasBeenRemovedInJobQueue;
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
                //if (character.currentAction != null && character.currentAction.parentPlan.job != null && character.currentAction.parentPlan.job == currJob) {
                //    //skip
                //    character.currentAction.parentPlan.job.SetAssignedCharacter(null);
                //    continue;
                //}
                summary += "\nUnassigned " + character.name + " from job " + currJob.name; 
                currJob.UnassignJob(false);
            }
        }
        character.PrintLogIfActive(summary);
    }
}