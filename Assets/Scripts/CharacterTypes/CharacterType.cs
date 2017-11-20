using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct CharacterType {

    public string characterTypeName;

    public CHARISMA charismaTrait;
    public INTELLIGENCE intelligenceTrait;
    public EFFICIENCY efficiencyTrait;
    public MILITARY militaryTrait;

    public List<TRAIT> otherTraits;
}
