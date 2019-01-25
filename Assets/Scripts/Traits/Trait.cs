using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ONLY HAS ONE INSTANCE IN THE WORLD, DO NOT PUT ANYTHING THAT WILL HAVE DIFFERENT VALUES IN DIFFERENT INSTANCES
[System.Serializable]
public class Trait {
    public virtual string nameInUI {
        get { return name; }
    }
    public string name;
    public string description;
    public TRAIT_TYPE type;
    public TRAIT_EFFECT effect;
    public TRAIT_TRIGGER trigger;
    public int daysDuration; //Zero (0) means Permanent
    public List<TraitEffect> effects;

    #region Virtuals
    public virtual void OnAddTrait(Character sourceCharacter) { }
    public virtual void OnRemoveTrait(Character sourceCharacter) { }
    public virtual bool IsUnique() { return true; }
    #endregion
}

[System.Serializable]
public class TraitEffect {
    public STAT stat;
    public float amount;
    public bool isPercentage;
    public TRAIT_REQUIREMENT_CHECKER checker;
    public TRAIT_REQUIREMENT_TARGET target;
    public DAMAGE_IDENTIFIER damageIdentifier; //Only used during combat
    public string description;

    public bool hasRequirement;
    public bool isNot;
    public TRAIT_REQUIREMENT requirementType;
    public TRAIT_REQUIREMENT_SEPARATOR requirementSeparator;
    public List<string> requirements;
}