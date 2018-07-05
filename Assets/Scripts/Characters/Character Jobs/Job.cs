using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Job {
    protected CHARACTER_JOB _jobType;
    protected CharacterRole _role;

    #region getters/setters
    public CharacterRole role {
        get { return _role; }
    }
    public CHARACTER_JOB jobType {
        get { return _jobType; }
    }
    #endregion

    public Job(CharacterRole role) {
        _role = role;
    }

    #region Virtuals
    public virtual void OnAssignJob() { }
    #endregion
}
