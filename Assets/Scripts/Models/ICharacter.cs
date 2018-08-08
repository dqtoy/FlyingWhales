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
    int vitality { get; }
    int baseAgility { get; } //Subject for removal
    int level { get; }
    int currentHP { get; }
    int maxHP { get; }
    int currentRow { get; }
    int id { get; }
    int currentSP { get; }
    int pFinalAttack { get; }
    int mFinalAttack { get; }
    int speed { get; }
    string coloredUrlName { get; }
    string urlName { get; }
    string name { get; }
    float computedPower { get; }
    float critChance { get; }
    float critDamage { get; }
    bool isDead { get; }
    GENDER gender { get; }
    ICHARACTER_TYPE icharacterType { get; }
    MODE currentMode { get; }
    CharacterBattleOnlyTracker battleOnlyTracker { get; }
    Faction faction { get; }
    BaseLandmark homeLandmark { get; }
    StructureObj homeStructure { get; }
    Area home { get; } //Character only
    CharacterRole role { get; } //Character only
    CharacterClass characterClass { get; } //Character only
    CharacterPortrait characterPortrait { get; }
    //Combat currentCombat { get; set; }
    Dictionary<ELEMENT, float> elementalWeaknesses { get; }
    Dictionary<ELEMENT, float> elementalResistances { get; }
    List<Skill> skills { get; }
    List<BodyPart> bodyParts { get; }
    List<CharacterAction> desperateActions { get; }
    List<CharacterAction> idleActions { get; }
    PortraitSettings portraitSettings { get; }
    NewParty ownParty { get; }
    NewParty currentParty { get; }
    Squad squad { get; }
    CharacterActionQueue<ActionQueueItem> actionQueue { get; }

    //functions
    void FaintOrDeath();
    void ResetToFullHP();
    void ResetToFullSP();
    void Initialize();
    void EverydayAction();
    void SetSide(ECS.SIDES side);
    void SetRowNumber(int row);
    void AdjustSP(int amount);
    void AdjustHP(int amount);
    void AdjustExperience(int amount);
    void EnableDisableSkills(Combat combat);
    void SetOwnedParty(NewParty party);
    void SetCurrentParty(NewParty party);
    void OnRemovedFromParty();
    void OnAddedToParty();
    void SetHomeLandmark(BaseLandmark newHomeLandmark);
    void SetHomeStructure(StructureObj newHomeStructure);
    void AddHistory(Log log); //Character only
    void SetSquad(Squad squad);
    void SetMode(MODE mode);
    bool InviteToParty(ICharacter inviter);
    bool IsInOwnParty();
    int GetDef();
    NewParty CreateOwnParty();
    CharacterAction GetRandomDesperateAction(ref IObject targetObject);
    CharacterAction GetRandomIdleAction(ref IObject targetObject);
    CharacterAction GetIdleOrDesperateAction(ACTION_CATEGORY category, ACTION_TYPE type);
    CharacterAttribute AssignTag(ATTRIBUTE tag); //Character only
    void AddActionToQueue(CharacterAction action, IObject targetObject, CharacterQuestData associatedQuestData = null, int position = -1);
    void RemoveActionFromQueue(ActionQueueItem item);
}
