using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionFilter : ICloneable {

    public virtual bool MeetsRequirements(Character character, BaseLandmark landmark) {
        return false;
    }
    public virtual bool MeetsRequirements(BaseLandmark landmark) {
        return false;
    }
    public virtual bool MeetsRequirements(string className, BaseLandmark landmark) {
        return true;
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
    public override bool MeetsRequirements(Character character, BaseLandmark landmark) {
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
    public override bool MeetsRequirements(Character character, BaseLandmark landmark) {
        if (character.role != null && !_unallowedRoles.Contains(character.role.roleType)) {
            return true;
        }
        return false;
    }
}
public class MustBeClass : ActionFilter {

    private List<string> _allowedClasses;

    public MustBeClass(List<ACTION_FILTER> allowedClasses) {
        _allowedClasses = new List<string>();
        for (int i = 0; i < allowedClasses.Count; i++) {
            ACTION_FILTER currFilter = allowedClasses[i];
            CHARACTER_CLASS charClass = (CHARACTER_CLASS) Enum.Parse(typeof(CHARACTER_CLASS), currFilter.ToString());
            _allowedClasses.Add(Utilities.NormalizeStringUpperCaseFirstLetters(charClass.ToString()));
        }
    }
    public override bool MeetsRequirements(Character character, BaseLandmark landmark) {
        if (character.characterClass != null) {
            if (_allowedClasses.Contains(character.characterClass.className)) {
                return true;
            }
        }
        return false;
    }
    public override bool MeetsRequirements(string className, BaseLandmark landmark) {
        if (_allowedClasses.Contains(className)) {
            return true;
        }
        return false;
    }
}
public class MustNotBeClass : ActionFilter {

    private List<string> _unallowedClasses;

    public MustNotBeClass(List<ACTION_FILTER> unallowedJobs) {
        _unallowedClasses = new List<string>();
        for (int i = 0; i < unallowedJobs.Count; i++) {
            ACTION_FILTER currFilter = unallowedJobs[i];
            CHARACTER_CLASS charClass = (CHARACTER_CLASS) Enum.Parse(typeof(CHARACTER_CLASS), currFilter.ToString());
            _unallowedClasses.Add(charClass.ToString());
        }
    }
    public override bool MeetsRequirements(Character character, BaseLandmark landmark) {
        if (character.characterClass != null) {
            if (_unallowedClasses.Contains(character.characterClass.className)) {
                return false;
            }
        }
        return true;
    }
    public override bool MeetsRequirements(string className, BaseLandmark landmark) {
        if (_unallowedClasses.Contains(className)) {
            return false;
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
    public override bool MeetsRequirements(Character character, BaseLandmark landmark) {
        //if (landmark.tileLocation.structureObjOnTile != null && landmark.tileLocation.structureObjOnTile.structureState == requiredState) {
        //    return true;
        //}
        return false;
    }
}
#endregion



