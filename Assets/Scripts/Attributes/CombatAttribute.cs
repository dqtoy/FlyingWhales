using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Trait {
    public string name;
    public string description;
    public TRAIT_TYPE type;
    public int daysDuration;
    public List<TraitEffect> effects;
}

[System.Serializable]
public class TraitEffect {
    public STAT stat;
    public float amount;
    public bool isPercentage;
    public TRAIT_REQUIREMENT_TARGET target;

    public bool hasRequirement;
    public bool isNot;
    public TRAIT_REQUIREMENT requirementType;
    public TRAIT_REQUIREMENT_SEPARATOR requirementSeparator;
    public List<string> requirements;
}