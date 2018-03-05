using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestFilter {

    #region virtuals
    public virtual bool MeetsRequirements(ECS.Character character) {
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
    public override bool MeetsRequirements(ECS.Character character) {
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
    public override bool MeetsRequirements(ECS.Character character) {
        for (int i = 0; i < _requiredTraits.Count; i++) {
            if (!character.HasTrait(_requiredTraits[i])) {
                return false;
            }
        }
        return true;
    }
    #endregion
}

public class MustNotHaveTags : QuestFilter {

	private List<CHARACTER_TAG> _tags;

	public MustNotHaveTags(List<CHARACTER_TAG> tags) {
		_tags = tags;
	}
	public MustNotHaveTags(CHARACTER_TAG tag) {
		_tags = new List<CHARACTER_TAG>();
		_tags.Add (tag);
	}

	#region overrides
	public override bool MeetsRequirements(ECS.Character character) {
		for (int i = 0; i < _tags.Count; i++) {
			if (character.HasTag(_tags[i])) {
				return false;
			}
		}
		return true;
	}
	#endregion
}

public class MustHaveTags : QuestFilter {

	private List<CHARACTER_TAG> _tags;

	public MustHaveTags(List<CHARACTER_TAG> tags) {
		_tags = tags;
	}
	public MustHaveTags(CHARACTER_TAG tag) {
		_tags = new List<CHARACTER_TAG>();
		_tags.Add (tag);
	}

	#region overrides
	public override bool MeetsRequirements(ECS.Character character) {
		for (int i = 0; i < _tags.Count; i++) {
			if (!character.HasTag(_tags[i])) {
				return false;
			}
		}
		return true;
	}
	#endregion
}

//public class MustBeRole : QuestFilter {
//
//    private CHARACTER_ROLE _requiredRole;
//
//    public MustBeRole(CHARACTER_ROLE requiredRole) {
//        _requiredRole = requiredRole;
//    }
//
//    #region overrides
//    public override bool MeetsRequirements(ECS.Character character) {
//        if(character.role != null) {
//			return character.role.roleType == _requiredRole;
//        }
//        return false;
//    }
//    #endregion
//}

public class MustBeClass : QuestFilter {

	private List<CHARACTER_CLASS> _allowedClasses;

	public MustBeClass(CHARACTER_CLASS requiredClass) {
        _allowedClasses = new List<CHARACTER_CLASS>();
        _allowedClasses.Add(requiredClass);
    }
    public MustBeClass(List<CHARACTER_CLASS> requiredClasses) {
        _allowedClasses = requiredClasses;
    }
    #region overrides
    public override bool MeetsRequirements(ECS.Character character) {
        //TODO change enum parsing if possible
        return _allowedClasses.Contains((CHARACTER_CLASS)System.Enum.Parse(typeof(CHARACTER_CLASS), character.characterClass.className, true));
    }
    #endregion
}

public class MustHaveSkills : QuestFilter {
    //TODO: Create MustHaveSkill OldQuest.Quest Filter on Merge of Combat Prototype 
}

public class MustBeFaction : QuestFilter {

    private List<Faction> _allowedFactions;

    public MustBeFaction(List<Faction> allowedFactions) {
        _allowedFactions = allowedFactions;
    }
    public MustBeFaction(Faction allowedFaction) {
        _allowedFactions = new List<Faction>();
        _allowedFactions.Add(allowedFaction);
    }

    #region overrides
    public override bool MeetsRequirements(ECS.Character character) {
        if(character.faction != null) {
            if (_allowedFactions.Contains(character.faction)) {
                return true;
            }
        }
        return false;
    }
    #endregion
}

public class MustNotBeFaction : QuestFilter {

    private List<Faction> _bannedFactions;

    public MustNotBeFaction(List<Faction> bannedFactions) {
        _bannedFactions = bannedFactions;
    }

    #region overrides
    public override bool MeetsRequirements(ECS.Character character) {
        if (character.faction != null) {
            if (_bannedFactions.Contains(character.faction)) {
                return false;
            }
        }
        return true;
    }
    #endregion
}