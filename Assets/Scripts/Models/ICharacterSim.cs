using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface ICharacterSim {
    string name { get; }
    string idName { get; }
    float actRate { get; set; }
    int speed { get; }
    int currentRow { get; }
    int level { get; }
    int currentSP { get; }
    int currentHP { get; }
    int maxHP { get; }
    int attackPower { get; }
    //int pFinalAttack { get; }
    //int mFinalAttack { get; }
    //int strength { get; }
    //int intelligence { get; }
    //float critChance { get; }
    //float critDamage { get; }
    SIDES currentSide { get; }
    ICHARACTER_TYPE icharacterType { get; }
    GENDER gender { get; }
    RACE race { get; }
    CharacterBattleOnlyTracker battleOnlyTracker { get; }
    CharacterClass characterClass { get; }
    List<Skill> skills { get; }
    List<Trait> traits { get; }
    //List<BodyPart> bodyParts { get; }
    Dictionary<ELEMENT, float> elementalWeaknesses { get; }
    Dictionary<ELEMENT, float> elementalResistances { get; }

    void InitializeSim();
    void ResetToFullHP();
    void ResetToFullSP();
    void DeathSim();
    void SetSide(SIDES side);
    void SetRowNumber(int row);
    void AdjustSP(int amount);
    void AdjustHP(int amount, Character killer = null);
    void EnableDisableSkills(CombatSim combatSim);
    CharacterAttribute GetAttribute(string attribute);
    //int GetDef();
}
