using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatAbility {
    public string name { get; protected set; }
    public string description { get; protected set; }
    public int lvl { get; protected set; }
    public int abilityRadius { get; protected set; } //0 means single target
    public List<ABILITY_TAG> abilityTags { get; protected set; }

	public CombatAbility() {
        name = "Combat Ability 1";
        lvl = 1;
        abilityRadius = 0;
        abilityTags = new List<ABILITY_TAG>();
    }


    public void LevelUp() {
        lvl++;
        OnLevelUp();
    }

    #region Virtuals
    protected virtual void OnLevelUp() { }

    //For single target abilities
    public virtual void ActivateAbility(Character targetCharacter) { }

    //For AOE effect abilities
    public virtual void ActivateAbility(List<Character> targetCharacters) { }
    #endregion
}
