using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CombatAttribute {
    public string name;
    public string description;
    public STAT stat;
    public float amount;
    public bool hasRequirement;
    public bool isPercentage;
    public string[] requirementNames;
}
