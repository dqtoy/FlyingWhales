using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public interface ICharacter {
    //getters
    SIDES currentSide { get; }
    int actRate { get; set; }
    int strength { get; }
    int intelligence { get; }
    int agility { get; }
    int baseAgility { get; } //Subject for removal
    int level { get; }
    int currentHP { get; }
    int maxHP { get; }
    int currentRow { get; }
    int id { get; }
    int currentSP { get; }
    int numOfAttackers { get; set; }
    string coloredUrlName { get; }
    string name { get; }
    float critChance { get; }
    float critDamage { get; }
    float computedPower { get; }
    bool isDead { get; }
    GENDER gender { get; }
    CharacterBattleOnlyTracker battleOnlyTracker { get; }
    Faction faction { get; }
    Faction attackedByFaction { get; set; }
    Combat currentCombat { get; set; }
    Dictionary<ELEMENT, float> elementalWeaknesses { get; }
    Dictionary<ELEMENT, float> elementalResistances { get; }
    List<Skill> skills { get; }
    List<BodyPart> bodyParts { get; }
    ICharacterObject icharacterObject { get; }
    ILocation specificLocation { get; }

    //functions
    void SetSide(ECS.SIDES side);
    void SetRowNumber(int row);
    void AdjustSP(int amount);
    void AdjustHP(int amount);
    void FaintOrDeath();
    void ResetToFullHP();
    void ResetToFullSP();
    void Initialize();
    int GetPDef(ICharacter enemy);
    int GetMDef(ICharacter enemy);
    void SetSpecificLocation(ILocation location);
}
