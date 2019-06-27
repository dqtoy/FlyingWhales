using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobQueueItem {
    public JobQueue jobQueueParent { get; protected set; }
    public Character assignedCharacter { get; protected set; }
    public string name { get; private set; }
    public JOB_TYPE jobType { get; protected set; }
    public bool cannotCancelJob { get; private set; }
    public bool cancelJobOnFail { get; private set; }
    public bool cannotOverrideJob { get; private set; }
    public List<Character> blacklistedCharacters { get; private set; }
    public int priority { get { return GetPriority(); } }

    protected System.Func<Character, JobQueueItem, bool> _canTakeThisJob;
    protected System.Func<Character, Character, JobQueueItem, bool> _canTakeThisJobWithTarget;
    protected System.Action<Character, JobQueueItem> _onTakeJobAction;
    protected int _priority; //The lower the amount the higher the priority

    public JobQueueItem(JOB_TYPE jobType) {
        this.jobType = jobType;
        this.name = Utilities.NormalizeStringUpperCaseFirstLetters(this.jobType.ToString());
        this.blacklistedCharacters = new List<Character>();
        SetInitialPriority();
    }

    #region Virtuals
    public virtual void UnassignJob(bool shouldDoAfterEffect = true) { }
    protected virtual bool CanTakeJob(Character character) {
        if (jobQueueParent.isAreaJobQueue) {
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
        if (_canTakeThisJob != null) {
            if (_canTakeThisJob(character, this)) {
                return CanTakeJob(character);
            }
            return false;
        }
        return CanTakeJob(character);
    }
    public virtual void OnCharacterAssignedToJob(Character character) {
        _onTakeJobAction?.Invoke(character, this);
    }
    #endregion

    public void SetJobQueueParent(JobQueue parent) {
        jobQueueParent = parent;
    }
    public void SetAssignedCharacter(Character character) {
        if (assignedCharacter != null) {
            assignedCharacter.SetCurrentJob(null);
            Debug.Log(assignedCharacter.name + " quit job " + name);
        }
        if (character != null) {
            character.SetCurrentJob(this);
            Debug.Log(character.name + " took job " + name);
        }
        
        assignedCharacter = character;
        if (assignedCharacter != null) {
            OnCharacterAssignedToJob(assignedCharacter);
        }
    }
    public void SetCanTakeThisJobChecker(System.Func<Character, JobQueueItem, bool> function) {
        _canTakeThisJob = function;
    }
    public void SetCanTakeThisJobChecker(System.Func<Character, Character, JobQueueItem, bool> function) {
        _canTakeThisJobWithTarget = function;
    }
    public void SetOnTakeJobAction(System.Action<Character, JobQueueItem> action) {
        _onTakeJobAction = action;
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
    public void AddBlacklistedCharacter(Character character) {
        if (!blacklistedCharacters.Contains(character)) {
            blacklistedCharacters.Add(character);
        }
    }
    public void RemoveBlacklistedCharacter(Character character) {
        blacklistedCharacters.Remove(character);
    }

    #region Priority
    public int GetPriority() {
        return _priority;
    }
    public void SetPriority(int amount) {
        _priority = amount;
    }
    private void SetInitialPriority() {
        int priority = 0;
        switch (jobType) {
            case JOB_TYPE.BERSERK:
            case JOB_TYPE.TANTRUM:
                priority = 10;
                break;
            case JOB_TYPE.KNOCKOUT:
            case JOB_TYPE.ABDUCT:
            case JOB_TYPE.UNDERMINE_ENEMY:
                priority = 20;
                break;
            case JOB_TYPE.FULLNESS_RECOVERY_STARVING:
            case JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED:
                priority = 30;
                break;
            case JOB_TYPE.REPORT_HOSTILE:
            case JOB_TYPE.APPREHEND:
            case JOB_TYPE.REPORT_CRIME:
                priority = 40;
                break;
            case JOB_TYPE.REMOVE_TRAIT:
                priority = 50;
                break;
            case JOB_TYPE.RESTRAIN:
                priority = 60;
                break;
            case JOB_TYPE.REMOVE_POISON:
                priority = 70;
                break;
            case JOB_TYPE.ASK_FOR_HELP_REMOVE_POISON_TABLE:
                priority = 80;
                break;
            case JOB_TYPE.SAVE_CHARACTER:
                priority = 90;
                break;
            case JOB_TYPE.ASK_FOR_HELP_SAVE_CHARACTER:
                priority = 90;
                break;
            case JOB_TYPE.HAPPINESS_RECOVERY_FORLORN:
                priority = 100;
                break;
            case JOB_TYPE.FEED:
                priority = 110;
                break;
            case JOB_TYPE.BURY:
            case JOB_TYPE.CRAFT_TOOL:
            case JOB_TYPE.BREW_POTION:
            case JOB_TYPE.OBTAIN_SUPPLY:
                priority = 120;
                break;
            case JOB_TYPE.BREAK_UP:
                priority = 130;
                break;
            case JOB_TYPE.REPLACE_TILE_OBJECT:
                priority = 140;
                break;
            case JOB_TYPE.EXPLORE:
                priority = 150;
                break;
            case JOB_TYPE.DELIVER_TREASURE:
                priority = 160;
                break;
            case JOB_TYPE.PATROL:
                priority = 170;
                break;
            case JOB_TYPE.FULLNESS_RECOVERY:
                //priority = 180;
                //break;
            case JOB_TYPE.TIREDNESS_RECOVERY:
                //priority = 190;
                //break;
            case JOB_TYPE.HAPPINESS_RECOVERY:
                //priority = 200;
                //break;
            case JOB_TYPE.SHARE_INFORMATION:
                //priority = 210;
                //break;
            case JOB_TYPE.JUDGEMENT:
                priority = 220;
                break;
            case JOB_TYPE.BUILD_FURNITURE:
            case JOB_TYPE.OBTAIN_ITEM:
                priority = 230;
                break;
        }
        if(priority > 0) {
            SetPriority(priority);
        } else {
            Debug.LogError("Cannot set initial priority for " + name + " job because priority is " + priority);
        }
    }
    #endregion
}
