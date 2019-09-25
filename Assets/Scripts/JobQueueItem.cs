using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public class JobQueueItem {
    public int id { get; protected set; }
    public JobQueue jobQueueParent { get; protected set; }
    public Character assignedCharacter { get; protected set; }
    public string name { get; private set; }
    public JOB_TYPE jobType { get; protected set; }
    public bool cannotCancelJob { get; private set; }
    public bool cancelJobOnFail { get; private set; }
    public bool cannotOverrideJob { get; private set; }
    public bool canBeDoneInLocation { get; private set; } //If a character is unable to create a plan for this job and the value of this is true, push the job to the location job queue
    public bool cancelJobOnDropPlan { get; private set; }
    public bool isStealth { get; private set; }
    public bool isNotSavable { get; protected set; }
    public List<Character> blacklistedCharacters { get; private set; }
    public int priority { get { return GetPriority(); } }

    public System.Func<Character, JobQueueItem, bool> canTakeThisJob { get; protected set; }
    public System.Func<Character, Character, JobQueueItem, bool> canTakeThisJobWithTarget { get; protected set; }
    public System.Action<Character, JobQueueItem> onTakeJobAction { get; protected set; }
    protected int _priority; //The lower the amount the higher the priority

    public JobQueueItem(JOB_TYPE jobType) {
        id = Utilities.SetID(this);
        this.jobType = jobType;
        this.name = Utilities.NormalizeStringUpperCaseFirstLetters(this.jobType.ToString());
        this.blacklistedCharacters = new List<Character>();
        SetInitialPriority();
    }
    public JobQueueItem(SaveDataJobQueueItem data) {
        id = Utilities.SetID(this, data.id);
        name = data.name;
        jobType = data.jobType;
        isNotSavable = data.isNotSavable;
        blacklistedCharacters = new List<Character>();
        for (int i = 0; i < data.blacklistedCharacterIDs.Count; i++) {
            blacklistedCharacters.Add(CharacterManager.Instance.GetCharacterByID(data.blacklistedCharacterIDs[i]));
        }
        SetCannotCancelJob(data.cannotCancelJob);
        SetCancelOnFail(data.cancelJobOnFail);
        SetCannotOverrideJob(data.cannotOverrideJob);
        SetCanBeDoneInLocation(data.canBeDoneInLocation);
        SetCancelJobOnDropPlan(data.cancelJobOnDropPlan);
        SetIsStealth(data.isStealth);
        SetInitialPriority();
    }

    #region Virtuals
    public virtual void UnassignJob(bool shouldDoAfterEffect = true) { }
    protected virtual bool CanTakeJob(Character character) {
        if (jobQueueParent.isAreaOrQuestJobQueue) {
            //Criminals and Characters with Negative Disabler Traits should no longer create and take Location Jobs
            return !character.HasTraitOf(TRAIT_TYPE.CRIMINAL) && !character.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER);
        }
        return true;
    }
    public virtual void OnAddJobToQueue() { }
    public virtual bool OnRemoveJobFromQueue() { return true; }
    public virtual bool CanCharacterTakeThisJob(Character character) {
        //All jobs that are personal will bypass _canTakeThisJob/_canTakeThisJobWithTarget function checkers if the character parameter is the owner of the job queue
        if (character == jobQueueParent.character) {
            return CanTakeJob(character);
        }
        if (canTakeThisJob != null) {
            if (canTakeThisJob(character, this)) {
                return CanTakeJob(character);
            }
            return false;
        }
        return CanTakeJob(character);
    }
    public virtual void OnCharacterAssignedToJob(Character character) {
        onTakeJobAction?.Invoke(character, this);
    }
    public virtual void OnCharacterUnassignedToJob(Character character) { }
    #endregion

    public void SetJobQueueParent(JobQueue parent) {
        jobQueueParent = parent;
    }
    public void SetAssignedCharacter(Character character) {
        Character previousAssignedCharacter = null;
        if (assignedCharacter != null) {
            previousAssignedCharacter = assignedCharacter;
            assignedCharacter.SetCurrentJob(null);
            assignedCharacter.PrintLogIfActive(GameManager.Instance.TodayLogString() + assignedCharacter.name + " quit job " + name);
        }
        if (character != null) {
            character.SetCurrentJob(this);
            character.PrintLogIfActive(GameManager.Instance.TodayLogString() + character.name + " took job " + name);
        }
        
        assignedCharacter = character;
        if (assignedCharacter != null) {
            OnCharacterAssignedToJob(assignedCharacter);
        } else if (assignedCharacter == null && previousAssignedCharacter != null) {
            OnCharacterUnassignedToJob(previousAssignedCharacter);
        }
    }
    public void SetCanTakeThisJobChecker(System.Func<Character, JobQueueItem, bool> function) {
        canTakeThisJob = function;
    }
    public void SetCanTakeThisJobChecker(System.Func<Character, Character, JobQueueItem, bool> function) {
        canTakeThisJobWithTarget = function;
    }
    public void SetOnTakeJobAction(System.Action<Character, JobQueueItem> action) {
        onTakeJobAction = action;
    }
    public void SetCannotCancelJob(bool state) {
        cannotCancelJob = state;
    }
    public void SetCancelOnFail(bool state) {
        cancelJobOnFail = state;
    }
    public void SetCannotOverrideJob(bool state) {
        cannotOverrideJob = state;
    }
    public void SetCanBeDoneInLocation(bool state) {
        canBeDoneInLocation = state;
    }
    public void SetCancelJobOnDropPlan(bool state) {
        cancelJobOnDropPlan = state;
    }
    public void AddBlacklistedCharacter(Character character) {
        if (!blacklistedCharacters.Contains(character)) {
            blacklistedCharacters.Add(character);
        }
    }
    public void RemoveBlacklistedCharacter(Character character) {
        blacklistedCharacters.Remove(character);
    }
    public void SetIsStealth(bool state) {
        isStealth = state;
    }

    #region Priority
    public int GetPriority() {
        return _priority;
    }
    public void SetPriority(int amount) {
        _priority = amount;
    }
    private void SetInitialPriority() {
        int priority = InteractionManager.Instance.GetInitialPriority(jobType);
        if(priority > 0) {
            SetPriority(priority);
        } else {
            Debug.LogError("Cannot set initial priority for " + name + " job because priority is " + priority);
        }
    }
    #endregion

    #region Utilities
    public override string ToString() {
        return jobType.ToString() + " assigned to " + assignedCharacter?.name ?? "None";
    }
    #endregion
}

[System.Serializable]
public class SaveDataJobQueueItem {
    public int id;
    public string jobTypeIdentifier;
    //public Character assignedCharacter { get; protected set; }
    public string name;
    public JOB_TYPE jobType;
    public bool cannotCancelJob;
    public bool cancelJobOnFail;
    public bool cannotOverrideJob;
    public bool canBeDoneInLocation; //If a character is unable to create a plan for this job and the value of this is true, push the job to the location job queue
    public bool cancelJobOnDropPlan;
    public bool isStealth;
    public bool isNotSavable;
    public List<int> blacklistedCharacterIDs;

    public string canTakeThisJobMethodName;
    public string canTakeThisJobWithTargetMethodName;
    public string onTakeJobActionMethodName;

    public virtual void Save(JobQueueItem job) {
        id = job.id;
        jobTypeIdentifier = job.GetType().ToString();
        name = job.name;
        jobType = job.jobType;
        cannotCancelJob = job.cannotCancelJob;
        cancelJobOnFail = job.cancelJobOnFail;
        cannotOverrideJob = job.cannotOverrideJob;
        canBeDoneInLocation = job.canBeDoneInLocation;
        cancelJobOnDropPlan = job.cancelJobOnDropPlan;
        isStealth = job.isStealth;
        isNotSavable = job.isNotSavable;

        blacklistedCharacterIDs = new List<int>();
        for (int i = 0; i < job.blacklistedCharacters.Count; i++) {
            blacklistedCharacterIDs.Add(job.blacklistedCharacters[i].id);
        }

        if(job.canTakeThisJob != null) {
            canTakeThisJobMethodName = job.canTakeThisJob.Method.Name;
        } else {
            canTakeThisJobMethodName = string.Empty;
        }
        if (job.canTakeThisJobWithTarget != null) {
            canTakeThisJobWithTargetMethodName = job.canTakeThisJobWithTarget.Method.Name;
        } else {
            canTakeThisJobWithTargetMethodName = string.Empty;
        }
        if (job.onTakeJobAction != null) {
            onTakeJobActionMethodName = job.onTakeJobAction.Method.Name;
        } else {
            onTakeJobActionMethodName = string.Empty;
        }
    }

    public virtual JobQueueItem Load() {
        JobQueueItem job = System.Activator.CreateInstance(System.Type.GetType(jobTypeIdentifier), this) as JobQueueItem;

        Type thisType = typeof(InteractionManager);
        if(canTakeThisJobMethodName != string.Empty) {
            MethodInfo canTakeThisJobMethod = thisType.GetMethod(canTakeThisJobMethodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if(canTakeThisJobMethod != null) {
                Func<Character, JobQueueItem, bool> function = (Func<Character, JobQueueItem, bool>) Delegate.CreateDelegate(typeof(Func<Character, JobQueueItem, bool>), canTakeThisJobMethod, false);
                job.SetCanTakeThisJobChecker(function);
            }
        }
        if (canTakeThisJobWithTargetMethodName != string.Empty) {
            MethodInfo canTakeThisJobWithTargetMethod = thisType.GetMethod(canTakeThisJobWithTargetMethodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (canTakeThisJobWithTargetMethod != null) {
                Func<Character, Character, JobQueueItem, bool> function = (Func<Character, Character, JobQueueItem, bool>) Delegate.CreateDelegate(typeof(Func<Character, Character, JobQueueItem, bool>), canTakeThisJobWithTargetMethod, false);
                job.SetCanTakeThisJobChecker(function);
            }
        }
        if (onTakeJobActionMethodName != string.Empty) {
            MethodInfo onTakeJobActionMethod = thisType.GetMethod(onTakeJobActionMethodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (onTakeJobActionMethod != null) {
                Action<Character, JobQueueItem> function = (Action<Character, JobQueueItem>) Delegate.CreateDelegate(typeof(Action<Character, JobQueueItem>), onTakeJobActionMethod, false);
                job.SetOnTakeJobAction(function);
            }
        }
        return job;
    }
}