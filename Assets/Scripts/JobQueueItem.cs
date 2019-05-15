using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobQueueItem {
    public JobQueue jobQueueParent { get; protected set; }
    public Character assignedCharacter { get; protected set; }
    public string name { get; private set; }
    public bool cannotCancelJob { get; private set; }
    public bool cancelJobOnFail { get; private set; }
    public bool cannotOverrideJob { get; private set; }
    public bool isPriority { get; private set; }
    public List<Character> blacklistedCharacters { get; private set; }

    protected System.Func<Character, bool> _canTakeThisJob;
    protected System.Func<Character, Character, bool> _canTakeThisJobWithTarget;

    public JobQueueItem(string name) {
        this.name = name;
        this.blacklistedCharacters = new List<Character>();
    }

    #region Virtuals
    public virtual void UnassignJob(bool shouldDoAfterEffect = true) { }
    protected virtual bool CanTakeJob(Character character) {
        return true;
    }
    public virtual void OnAddJobToQueue() { }
    public virtual bool OnRemoveJobFromQueue() { return true; }
    public virtual bool CanCharacterTakeThisJob(Character character) {
        if (_canTakeThisJob != null) {
            if (_canTakeThisJob(character)) {
                return CanTakeJob(character);
            }
            return false;
        }
        return CanTakeJob(character);
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
    }
    public void SetCanTakeThisJobChecker(System.Func<Character, bool> function) {
        _canTakeThisJob = function;
    }
    public void SetCanTakeThisJobChecker(System.Func<Character, Character, bool> function) {
        _canTakeThisJobWithTarget = function;
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
    public void SetIsPriority(bool state) {
        isPriority = state;
    }
    public void AddBlacklistedCharacter(Character character) {
        if (!blacklistedCharacters.Contains(character)) {
            blacklistedCharacters.Add(character);
        }
    }
    public void RemoveBlacklistedCharacter(Character character) {
        blacklistedCharacters.Remove(character);
    }
}
