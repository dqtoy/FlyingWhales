using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ONLY HAS ONE INSTANCE IN THE WORLD, DO NOT PUT ANYTHING THAT WILL HAVE DIFFERENT VALUES IN DIFFERENT INSTANCES
[System.Serializable]
public class Trait {
    public string name;
    public string description;
    public TRAIT_TYPE type;
    public int daysDuration; //Zero (0) means Permanent
    public List<TraitEffect> effects;
}

[System.Serializable]
public class TraitEffect {
    public STAT stat;
    public float amount;
    public bool isPercentage;
    public TRAIT_REQUIREMENT_TARGET target;
    public string description;

    public bool hasRequirement;
    public bool isNot;
    public TRAIT_REQUIREMENT requirementType;
    public TRAIT_REQUIREMENT_SEPARATOR requirementSeparator;
    public List<string> requirements;
}