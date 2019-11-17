using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobQueue {
    public IJobQueueOwner owner { get; private set; }
    //public Quest quest { get; private set; } //If there is a quest it means that this is a quest job

    public List<JobQueueItem> jobsInQueue { get; private set; }

    //public bool isAreaOrQuestJobQueue {
    //    get { return owner == null; }
    //}
    public JobQueue() { //IJobQueueOwner owner
        //this.owner = owner;
        jobsInQueue = new List<JobQueueItem>();
    }

    //NOTE: IMPROVE THIS! PROBABLY PUT THIS IN CONSTRUCTOR JUST LIKE WITH THE CHARACTER
    //public void SetQuest(Quest quest) {
    //    this.quest = quest;
    //}

    public bool AddJobInQueue(JobQueueItem job, bool processLogicForPersonalJob = true) {
        if (!CanJobBeAddedToQueue(job)) {
            return false;
        }
        job.SetCurrentOwner(owner);
        bool hasBeenInserted = false;
        int insertIndex = -1;
        for (int i = 0; i < jobsInQueue.Count; i++) {
            if(job.priority < jobsInQueue[i].priority) {
                jobsInQueue.Insert(i, job);
                insertIndex = i;
                hasBeenInserted = true;
                break;
            }
        }
        if (!hasBeenInserted) {
            jobsInQueue.Add(job);
        }
        job.OnAddJobToQueue();
        owner.OnAddJob(job);
        //if(quest != null) {
        //    quest.OnAddJob(job);
        //}

        if(owner.ownerType == JOB_QUEUE_OWNER.CHARACTER && processLogicForPersonalJob) {
            //bool hasProcessed = false;
            //Character characterOwner = owner as Character;
            //If the current action's job of the character is overridable and the added job has higher priority than it,
            //then process that job immediately and stop what the character is currently doing
            //Sometimes, in rare cases, the character still cannot be assigned to a job even if it is a personal job,
            //It might be because CanTakeJob/CanCharacterTakeThisJob function is not satisfied
            //if (processLogicForPersonalJob) {
            if (hasBeenInserted && insertIndex == 0){ //This means that the job is inserted as the top most priority //characterOwner.CanCurrentJobBeOverriddenByJob(job))
                CurrentTopPriorityIsPushedBackBy(job);
            } 
            //else {
            //    //If a character is a higher priority than one of the currently existing jobs
            //    //Check all currently existing plans of character
            //    //All plans' job of character that has lower priority than the added job must be dropped so that the job will be prioritized than those 
            //    if (hasBeenInserted) {
            //        for (int i = 0; i < owner.allGoapPlans.Count; i++) {
            //            GoapPlan plan = owner.allGoapPlans[i];
            //            if(plan.job == null || (job.priority < plan.job.priority)) {
            //                if(plan.job == null) {
            //                    owner.JustDropPlan(plan);
            //                } else {
            //                    if (plan.job.jobQueueParent.isAreaOrQuestJobQueue) {
            //                        plan.job.UnassignJob(false);
            //                    } else {
            //                        owner.JustDropPlan(plan);
            //                    }
            //                }
            //                i--;
            //            }
            //        }
            //    }
            //}
            //}
            //if (processLogicForPersonalJob && !hasProcessed) {
            //    if ((character.stateComponent.currentState != null && (character.stateComponent.currentState.characterState == CHARACTER_STATE.STROLL || character.stateComponent.currentState.characterState == CHARACTER_STATE.STROLL_OUTSIDE))
            //        || (character.currentAction != null && character.currentAction.goapType == INTERACTION_TYPE.RETURN_HOME &&
            //        (character.currentAction.parentPlan == null || character.currentAction.parentPlan.category == GOAP_CATEGORY.IDLE))) {
            //        character.jobQueue.ProcessFirstJobInQueue(character);
            //    }
            //}
        }
        return true;
    }
    public bool RemoveJobInQueue(JobQueueItem job) {
        if (jobsInQueue.Remove(job)) {
            string ownerName = owner.name;
            string removeLog = job.name + " has been removed from " + ownerName + " job queue.";
            //removeLog += "\nIs Personal: " + (character != null ? character.name : "False");
            //removeLog += "\nAssigned Character: " + (job.assignedCharacter != null ? job.assignedCharacter.name : "None");
            //if(job is GoapPlanJob) {
            //    GoapPlanJob planJob = job as GoapPlanJob;
            //removeLog += "\nAssigned Plan: " + (planJob.assignedPlan != null);
            //}
            Debug.Log(GameManager.Instance.TodayLogString() + removeLog);
            //if (quest != null) {
            //    quest.OnRemoveJob(job);
            //}
            owner.OnRemoveJob(job);
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
                if (job.CanCharacterDoJob(characterToDoJob)) {
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
                    if(!isAreaOrQuestJobQueue && !job.CanCharacterTakeThisJob(characterToDoJob)) {
                        //If it is a personal job, and it cannot be done by this character, remove it from queue
                        RemoveJobInQueue(job);
                        i--;
                    }
                    continue;
                } else {
                    return true;
                }
            }
        }
        return false;
    }
    public void CurrentTopPriorityIsPushedBackBy(JobQueueItem job) {
        if(owner.ownerType != JOB_QUEUE_OWNER.CHARACTER) {
            return;
        }
        Character characterOwner = owner as Character;
        if (characterOwner.stateComponent.currentState != null) {
            characterOwner.stateComponent.currentState.OnExitThisState();
            //This call is doubled so that it will also exit the previous major state if there's any
            if (characterOwner.stateComponent.currentState != null) {
                characterOwner.stateComponent.currentState.OnExitThisState();
            }
        }
        //else if (character.stateComponent.stateToDo != null) {
        //    character.stateComponent.SetStateToDo(null);
        //} 
        else {
            if (characterOwner.currentParty.icon.isTravelling) {
                if (characterOwner.currentParty.icon.travelLine == null) {
                    characterOwner.marker.StopMovement();
                } else {
                    characterOwner.currentParty.icon.SetOnArriveAction(() => characterOwner.OnArriveAtAreaStopMovement());
                }
            }
            characterOwner.AdjustIsWaitingForInteraction(1);
            characterOwner.StopCurrentAction(false, "Have something important to do");
            characterOwner.AdjustIsWaitingForInteraction(-1);
        }
        job.ProcessJob();
        //if (AssignCharacterToJob(job)) {
        //    if (job is CharacterStateJob) {
        //        //Will no longer stop what is currently doing if job is a state job because it will already be done by that state
        //        return;
        //    }
        //    if (characterOwner.stateComponent.currentState != null) {
        //        characterOwner.stateComponent.currentState.OnExitThisState();
        //        //This call is doubled so that it will also exit the previous major state if there's any
        //        if (characterOwner.stateComponent.currentState != null) {
        //            characterOwner.stateComponent.currentState.OnExitThisState();
        //        }
        //    }
        //    //else if (character.stateComponent.stateToDo != null) {
        //    //    character.stateComponent.SetStateToDo(null);
        //    //} 
        //    else {
        //        if (characterOwner.currentParty.icon.isTravelling) {
        //            if (characterOwner.currentParty.icon.travelLine == null) {
        //                characterOwner.marker.StopMovement();
        //            } else {
        //                characterOwner.currentParty.icon.SetOnArriveAction(() => characterOwner.OnArriveAtAreaStopMovement());
        //            }
        //        }
        //        characterOwner.AdjustIsWaitingForInteraction(1);
        //        characterOwner.StopCurrentAction(false, "Have something important to do");
        //        characterOwner.AdjustIsWaitingForInteraction(-1);
        //    }
        //}
    }
    //public bool AssignCharacterToJob(JobQueueItem job) {
    //    if (CanJobBeAddedToQueue(job)) {
    //        //job.SetAssignedCharacter(characterToDoJob);
    //        if (job is GoapPlanJob) {
    //            GoapPlanJob goapPlanJob = job as GoapPlanJob;
    //            if (goapPlanJob.targetInteractionType != INTERACTION_TYPE.NONE) {
    //                characterToDoJob.StartGOAP(goapPlanJob.targetInteractionType, goapPlanJob.targetPOI, GOAP_CATEGORY.WORK, false, null, true, goapPlanJob, goapPlanJob.otherData, goapPlanJob.allowDeadTargets);
    //            } else {
    //                characterToDoJob.StartGOAP(goapPlanJob.goals, goapPlanJob.targetPOI, GOAP_CATEGORY.WORK, false, null, true, goapPlanJob, goapPlanJob.otherData, goapPlanJob.allowDeadTargets);
    //            }
    //            //if (goapPlanJob.targetPlan != null) {
    //            //    characterToDoJob.AddPlan(goapPlanJob.targetPlan);
    //            //    goapPlanJob.SetAssignedPlan(goapPlanJob.targetPlan);
    //            //} else {
    //            //    if (goapPlanJob.targetInteractionType != INTERACTION_TYPE.NONE) {
    //            //        characterToDoJob.StartGOAP(goapPlanJob.targetInteractionType, goapPlanJob.targetPOI, GOAP_CATEGORY.WORK, false, null, true, goapPlanJob, goapPlanJob.otherData, goapPlanJob.allowDeadTargets);
    //            //    } else {
    //            //        characterToDoJob.StartGOAP(goapPlanJob.targetEffect, goapPlanJob.targetPOI, GOAP_CATEGORY.WORK, false, null, true, goapPlanJob, goapPlanJob.otherData, goapPlanJob.allowDeadTargets);
    //            //    }
    //            //}
    //        } else if (job is CharacterStateJob) {
    //            CharacterStateJob stateJob = job as CharacterStateJob;
    //            CharacterState newState = characterToDoJob.stateComponent.SwitchToState(stateJob.targetState);
    //            if (newState != null) {
    //                stateJob.SetAssignedState(newState);
    //            } else {
    //                throw new System.Exception(characterToDoJob.name + " tried doing state " + stateJob.targetState.ToString() + " but was unable to do so! This must not happen!");
    //            }
    //        }
    //        return true;
    //    }
    //    return false;
    //}
    public bool CanJobBeAddedToQueue(JobQueueItem job) {
        if(owner.ownerType == JOB_QUEUE_OWNER.CHARACTER) {
            //Personal jobs can only be added to job queue of original owner
            if(job.originalOwner.ownerType == JOB_QUEUE_OWNER.CHARACTER) {
                return job.originalOwner == owner;
            } else {
                Character characterToDoJob = owner as Character;
                if(jobsInQueue.Count > 0) {
                    if(job.priority < jobsInQueue[0].priority) {
                        return job.CanCharacterDoJob(characterToDoJob);
                    } else {
                        return false;
                    }
                } else {
                    return job.CanCharacterDoJob(characterToDoJob);
                }
            }
        }
        return true;
    }
    public void ForceAssignCharacterToJob(JobQueueItem job, Character characterToDoJob) {
        if (job.assignedCharacter == null) {
            job.SetAssignedCharacter(characterToDoJob);
            if (job is GoapPlanJob) {
                GoapPlanJob goapPlanJob = job as GoapPlanJob;
                //if (goapPlanJob.targetPlan != null) {
                //    characterToDoJob.AddPlan(goapPlanJob.targetPlan);
                //    goapPlanJob.SetAssignedPlan(goapPlanJob.targetPlan);
                //} else {
                //    if (goapPlanJob.targetInteractionType != INTERACTION_TYPE.NONE) {
                //        characterToDoJob.StartGOAP(goapPlanJob.targetInteractionType, goapPlanJob.targetPOI, GOAP_CATEGORY.WORK, false, null, true, goapPlanJob, goapPlanJob.otherData, goapPlanJob.allowDeadTargets);
                //    } else {
                //        characterToDoJob.StartGOAP(goapPlanJob.targetEffect, goapPlanJob.targetPOI, GOAP_CATEGORY.WORK, false, null, true, goapPlanJob, goapPlanJob.otherData, goapPlanJob.allowDeadTargets);
                //    }
                //}
                if (goapPlanJob.targetInteractionType != INTERACTION_TYPE.NONE) {
                    characterToDoJob.StartGOAP(goapPlanJob.targetInteractionType, goapPlanJob.targetPOI, GOAP_CATEGORY.WORK, false, null, true, goapPlanJob, goapPlanJob.otherData, goapPlanJob.allowDeadTargets);
                } else {
                    characterToDoJob.StartGOAP(goapPlanJob.goals, goapPlanJob.targetPOI, GOAP_CATEGORY.WORK, false, null, true, goapPlanJob, goapPlanJob.otherData, goapPlanJob.allowDeadTargets);
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
                if (job.goals.conditionType == conditionType && job.goals.targetPOI == poi) {
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
                if (job.goals.conditionType == conditionType && job.goals.targetPOI == poi) {
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
                if (job.goals.conditionType == conditionType && job.goals.conditionKey == conditionKey && job.goals.targetPOI == poi) {
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
    public bool CancelJob(JobQueueItem job, string cause = "", bool shouldDoAfterEffect = true, bool forceRemove = false, string reason = "") {
        //When cancelling a job, we must check if it's personal or not because if it is a faction/settlement job it cannot be removed from queue
        //The only way for a faction/settlement job to be removed is if it is forced or it is actually finished
        bool hasBeenRemovedInJobQueue = false;
        bool process = false;
        if (job.currentOwner.isAreaOrQuestJobQueue) {
            process = true;
            if (forceRemove) {
                hasBeenRemovedInJobQueue = RemoveJobInQueue(job);
            }
        } else {
            hasBeenRemovedInJobQueue = RemoveJobInQueue(job);
            process = hasBeenRemovedInJobQueue;
        }
        if (process) {
            if(job is GoapPlanJob && cause != "") {
                GoapPlanJob planJob = job as GoapPlanJob;
                Character actor = null;
                if(!isAreaOrQuestJobQueue) {
                    actor = this.owner;
                } else if(job.assignedCharacter != null) {
                    actor = job.assignedCharacter;
                }
                if(actor != null && actor != planJob.targetPOI) { //only log if the actor is not the same as the target poi.
                    actor.RegisterLogAndShowNotifToThisCharacterOnly("Generic", "job_cancelled_cause", null, cause);
                }
            }
            job.UnassignJob(shouldDoAfterEffect, reason);
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

    public void AddPremadeJob(Character actor, JOB_TYPE jobType, GOAP_CATEGORY goapCategory, bool allowDeadTargets = false, bool isStealth = false, bool cancelOnFail = false,
        params IGoapJobPremadeNodeCreator[] premadeCreator) {

        List<GoapNode> nodes = new List<GoapNode>();
        for (int i = 0; i < premadeCreator.Length; i++) {
            if(premadeCreator[i] is ActionJobPremadeNodeCreator) {
                GoapAction action = InteractionManager.Instance.CreateNewGoapInteraction(((ActionJobPremadeNodeCreator) premadeCreator[i]).actionType, actor, premadeCreator[i].targetPOI);
                GoapNode parentNode = null;
                if(nodes.Count > 0) {
                    parentNode = nodes[nodes.Count - 1];
                }
                GoapNode node = new GoapNode(parentNode, action.cost, action);
                nodes.Add(node);
            }
        }
        GoapNode goalNode = nodes[0];
        GoapNode startingNode = nodes[nodes.Count - 1];
        GoapPlanJob job = new GoapPlanJob(jobType, goalNode.action.goapType, goalNode.action.poiTarget);

        GOAP_EFFECT_CONDITION[] goalEffects = new GOAP_EFFECT_CONDITION[goalNode.action.expectedEffects.Count];
        for (int i = 0; i < goalNode.action.expectedEffects.Count; i++) {
            goalEffects[i] = goalNode.action.expectedEffects[i].conditionType;
        }
        GoapPlan plan = new GoapPlan(startingNode, goalEffects, goapCategory);
        plan.ConstructAllNodes();
        plan.SetDoNotRecalculate(true);
        job.SetIsStealth(isStealth);
        job.SetAssignedPlan(plan);
        job.SetAssignedCharacter(actor);
        job.SetCancelOnFail(cancelOnFail);

        actor.jobQueue.AddJobInQueue(job, false);

        //TODO: Add plan immediately? Stop current action or state?
    }
}