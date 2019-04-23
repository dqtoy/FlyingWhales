using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobQueueItem {
    public JobQueue jobQueueParent { get; protected set; }
    public Character assignedCharacter { get; protected set; }
    public string name { get; private set; }
    public bool cannotCancelJob { get; private set; }

    private System.Func<Character, bool> _canTakeThisJob;

    public JobQueueItem(string name) {
        this.name = name;
    }

    #region Virtuals
    public virtual void UnassignJob(bool shouldDoAfterEffect = true) { }
    protected virtual bool CanTakeJob(Character character) {
        return true;
    }
    public virtual void OnAddJobToQueue() { }
    public virtual bool OnRemoveJobFromQueue() { return true; }
    #endregion

    public void SetJobQueueParent(JobQueue parent) {
        jobQueueParent = parent;
    }
    public void SetAssignedCharacter(Character character) {
        if (assignedCharacter != null) {
            assignedCharacter.SetCurrentJob(null);
        }
        if (character != null) {
            character.SetCurrentJob(this);
        }
        assignedCharacter = character;
    }
    public void SetCanTakeThisJobChecker(System.Func<Character, bool> function) {
        _canTakeThisJob = function;
    }
    public void SetCannotCancelJob(bool state) {
        cannotCancelJob = state;
    }

    public bool CanCharacterTakeThisJob(Character character) {
        if(_canTakeThisJob != null) {
            if (_canTakeThisJob(character)) {
                return CanTakeJob(character);
            }
            return false;
        }
        return CanTakeJob(character);
    }
}
