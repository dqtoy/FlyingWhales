using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

[System.Serializable]
public class Trait{
    public string traitName;
    public TRAIT trait;
    public ActionWeight[] actionWeights;
	public StatsModifierPercentage statsModifierPercentage;

    [System.NonSerialized] protected ECS.Character _ownerOfTrait;

    public void AssignCharacter(ECS.Character ownerOfTrait) {
        _ownerOfTrait = ownerOfTrait;
    }
}
