using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CombatAttribute {
    public string name;
    public string description;
    public STAT stat;
    public float amount;
    public bool hasRequirement;
    public bool isPercentage;
    public DAMAGE_IDENTIFIER damageIdentifier; //dealt or received
    public COMBAT_ATTRIBUTE_REQUIREMENT requirementType;
    public string requirement;
}
