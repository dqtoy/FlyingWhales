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
    public HEALTH healthTrait;
    public STRENGTH strengthTrait;
    public AGILITY agilityTrait;

    public List<TRAIT> otherTraits;

    #region getters/setters
    public List<TRAIT> allTraits {
        get {
            List<TRAIT> _allTraits = new List<TRAIT>();
            _allTraits.Add((TRAIT)charismaTrait);
            _allTraits.Add((TRAIT)intelligenceTrait);
            _allTraits.Add((TRAIT)efficiencyTrait);
            _allTraits.Add((TRAIT)militaryTrait);
            _allTraits.Add((TRAIT)healthTrait);
            _allTraits.Add((TRAIT)strengthTrait);
            _allTraits.Add((TRAIT)agilityTrait);
            _allTraits.AddRange(otherTraits);
            return _allTraits;
        }
    }
    #endregion
}
