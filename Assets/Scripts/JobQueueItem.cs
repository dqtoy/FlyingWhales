using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobQueueItem {
    public JobQueue jobQueueParent { get; protected set; }
    public Character assignedCharacter { get; protected set; }

    private System.Func<Character, bool> _canTakeThisJob;

    #region Virtuals
    public virtual void UnassignJob() { }
    protected virtual bool CanTakeJob(Character character) { return true; }
    #endregion

    public void SetJobQueueParent(JobQueue parent) {
        jobQueueParent = parent;
    }
    public void SetAssignedCharacter(Character character) {
        assignedCharacter = character;
    }
    public void SetCanTakeThisJobChecker(System.Func<Character, bool> function) {
        _canTakeThisJob = function;
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
