using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionFilter : ICloneable {

    public virtual bool MeetsRequirements(ECS.Character character, BaseLandmark landmark) {
        return false;
    }
    public virtual bool MeetsRequirements(BaseLandmark landmark) {
        return false;
    }

    public virtual object Clone() {
        return this.MemberwiseClone();
    }
}

#region Character Filters
public class MustBeRole : ActionFilter {

    private List<CHARACTER_ROLE> _allowedRoles;

    public MustBeRole(List<ACTION_FILTER> allowedRoles) {
        _allowedRoles = new List<CHARACTER_ROLE>();
        for (int i = 0; i < allowedRoles.Count; i++) {
            ACTION_FILTER currFilter = allowedRoles[i];
            CHARACTER_ROLE role = (CHARACTER_ROLE)Enum.Parse(typeof(CHARACTER_ROLE), currFilter.ToString());
            _allowedRoles.Add(role);
        }
    }
    public override bool MeetsRequirements(ECS.Character character, BaseLandmark landmark) {
        if (character.role != null) {
            if (_allowedRoles.Contains(character.role.roleType)) {
                return true;
            }
        }
        return false;
    }
}
public class MustNotBeRole : ActionFilter {

    private List<CHARACTER_ROLE> _unallowedRoles;

    public MustNotBeRole(List<ACTION_FILTER> unallowedRoles) {
        _unallowedRoles = new List<CHARACTER_ROLE>();
        for (int i = 0; i < unallowedRoles.Count; i++) {
            ACTION_FILTER currFilter = unallowedRoles[i];
            CHARACTER_ROLE role = (CHARACTER_ROLE)Enum.Parse(typeof(CHARACTER_ROLE), currFilter.ToString());
            _unallowedRoles.Add(role);
        }
    }
    public override bool MeetsRequirements(ECS.Character character, BaseLandmark landmark) {
        if (character.role != null && !_unallowedRoles.Contains(character.role.roleType)) {
            return true;
        }
        return false;
    }
}
public class MustBeJob : ActionFilter {

    private List<CHARACTER_JOB> _allowedJobs;

    public MustBeJob(List<ACTION_FILTER> allowedJobs) {
        _allowedJobs = new List<CHARACTER_JOB>();
        for (int i = 0; i < allowedJobs.Count; i++) {
            ACTION_FILTER currFilter = allowedJobs[i];
            CHARACTER_JOB job = (CHARACTER_JOB)Enum.Parse(typeof(CHARACTER_JOB), currFilter.ToString());
            _allowedJobs.Add(job);
        }
    }
    public override bool MeetsRequirements(ECS.Character character, BaseLandmark landmark) {
        if (character.role != null && character.role.job != null) {
            if (_allowedJobs.Contains(character.role.job.jobType)) {
                return true;
            }
        }
        return false;
    }
}
public class MustNotBeJob : ActionFilter {

    private List<CHARACTER_JOB> _unallowedJobs;

    public MustNotBeJob(List<ACTION_FILTER> unallowedJobs) {
        _unallowedJobs = new List<CHARACTER_JOB>();
        for (int i = 0; i < unallowedJobs.Count; i++) {
            ACTION_FILTER currFilter = unallowedJobs[i];
            CHARACTER_JOB job = (CHARACTER_JOB)Enum.Parse(typeof(CHARACTER_JOB), currFilter.ToString());
            _unallowedJobs.Add(job);
        }
    }
    public override bool MeetsRequirements(ECS.Character character, BaseLandmark landmark) {
        if (character.role != null && character.role.job != null) {
            if (_unallowedJobs.Contains(character.role.job.jobType)) {
                return false;
            }
        }
        return true;
    }
}
#endregion

#region Landmark Filters
public class LandmarkMustBeState : ActionFilter {

    private STRUCTURE_STATE requiredState;

    public LandmarkMustBeState(ACTION_FILTER requiredState) {
        this.requiredState = (STRUCTURE_STATE)Enum.Parse(typeof(STRUCTURE_STATE), requiredState.ToString());
    }
    public override bool MeetsRequirements(ECS.Character character, BaseLandmark landmark) {
        if (landmark.tileLocation.structureObjOnTile != null && landmark.tileLocation.structureObjOnTile.structureState == requiredState) {
            return true;
        }
        return false;
    }
}
#endregion



