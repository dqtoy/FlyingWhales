using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionFilter : ICloneable {

    public virtual bool MeetsRequirements(ECS.Character character) {
        return false;
    }

    public virtual object Clone() {
        return this.MemberwiseClone();
    }
}

public class MustBeRole : ActionFilter {

    private List<CHARACTER_ROLE> _allowedRoles;

    public MustBeRole(List<CHARACTER_ROLE> allowedRoles) {
        _allowedRoles = allowedRoles;
    }
    public override bool MeetsRequirements(ECS.Character character) {
        if (character.role != null && _allowedRoles.Contains(character.role.roleType)) {
            return true;
        }
        return false;
    }
}


