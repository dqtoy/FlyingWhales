using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestFilter {

    #region virtuals
    public virtual bool MeetsRequirements(Character character) {
        return true;
    }
    #endregion
}

public class MustNotHaveTraits : QuestFilter {

    private List<TRAIT> _traits;

    public MustNotHaveTraits(List<TRAIT> traits) {
        _traits = traits;
    }

    #region overrides
    public override bool MeetsRequirements(Character character) {
        for (int i = 0; i < _traits.Count; i++) {
            if (character.HasTrait(_traits[i])) {
                return false;
            }
        }
        return true;
    }
    #endregion
}

public class MustHaveTraits : QuestFilter {

    private List<TRAIT> _requiredTraits;

    public MustHaveTraits(List<TRAIT> requiredTraits) {
        _requiredTraits = requiredTraits;
    }

    #region overrides
    public override bool MeetsRequirements(Character character) {
        for (int i = 0; i < _requiredTraits.Count; i++) {
            if (!character.HasTrait(_requiredTraits[i])) {
                return false;
            }
        }
        return true;
    }
    #endregion
}

public class MustBeRole : QuestFilter {

    private CHARACTER_ROLE _requiredRole;

    public MustBeRole(CHARACTER_ROLE requiredRole) {
        _requiredRole = requiredRole;
    }

    #region overrides
    public override bool MeetsRequirements(Character character) {
        if(character._role != null) {
            return character._role.roleType == _requiredRole;
        }
        return false;
    }
    #endregion
}

public class MustBeClass : QuestFilter {

    private CHARACTER_CLASS _requiredClass;

    public MustBeClass(CHARACTER_CLASS requiredClass) {
        _requiredClass = requiredClass;
    }

    #region overrides
    public override bool MeetsRequirements(Character character) {
         return character._characterClass == _requiredClass;
    }
    #endregion
}