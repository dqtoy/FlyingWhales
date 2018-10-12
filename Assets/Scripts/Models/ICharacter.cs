using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public interface ICharacter {
    //getters
    SIDES currentSide { get; }
    float actRate { get; set; }
    int level { get; }
    int currentHP { get; }
    int maxHP { get; }
    int currentSP { get; }
    int maxSP { get; }
    int currentRow { get; }
    int id { get; }
    float attackPower { get; }
    float speed { get; }
    string coloredUrlName { get; }
    string urlName { get; }
    string name { get; }
    float computedPower { get; }
    bool isDead { get; }
    bool isBeingInspected { get; }
    GENDER gender { get; }
    ICHARACTER_TYPE icharacterType { get; }
    MODE currentMode { get; }
    RACE race { get; }
    CharacterBattleOnlyTracker battleOnlyTracker { get; }
    Faction faction { get; }
    BaseLandmark homeLandmark { get; }
    //StructureObj homeStructure { get; }
    //Area home { get; } //Character only
    CharacterRole role { get; } //Character only
    CharacterClass characterClass { get; } //Character only
    CharacterPortrait characterPortrait { get; }
    Weapon equippedWeapon { get; }
    Armor equippedArmor { get; }
    Item equippedAccessory { get; }
    Item equippedConsumable { get; }
    //Combat currentCombat { get; set; }
    Dictionary<ELEMENT, float> elementalWeaknesses { get; }
    Dictionary<ELEMENT, float> elementalResistances { get; }
    Dictionary<Character, Relationship> relationships { get; }
    List<Skill> skills { get; }
    //List<BodyPart> bodyParts { get; }
    List<CharacterAction> miscActions { get; }
    List<Attribute> attributes { get; }
    List<Item> inventory { get; }
    List<Log> history { get; }
    List<CombatAttribute> combatAttributes { get; }
    PortraitSettings portraitSettings { get; }
    Party ownParty { get; }
    Party currentParty { get; }
    Squad squad { get; }
    CharacterActionQueue<ActionQueueItem> actionQueue { get; }

    //functions
    void ResetToFullHP();
    void ResetToFullSP();
    void Initialize();
    void Death();
    //void EverydayAction();
    void FaintOrDeath(ICharacter killer);
    void SetSide(ECS.SIDES side);
    void SetRowNumber(int row);
    void AdjustSP(int amount);
    void AdjustHP(int amount, ICharacter killer = null);
    void AdjustExperience(int amount);
    void EnableDisableSkills(Combat combat);
    void SetOwnedParty(Party party);
    void SetCurrentParty(Party party);
    void OnRemovedFromParty();
    void OnAddedToParty();
    void SetHomeLandmark(BaseLandmark newHomeLandmark);
    //void SetHomeStructure(StructureObj newHomeStructure);
    void AddHistory(Log log); //Character only
    void SetSquad(Squad squad);
    void SetMode(MODE mode);
    void AddMiscAction(CharacterAction characterAction);
    void RemoveMiscAction(ACTION_TYPE actionType);
    bool InviteToParty(ICharacter inviter);
    bool IsInOwnParty();
    Party CreateOwnParty();
    Attribute GetAttribute(string attribute);
    CharacterAction GetRandomMiscAction(ref IObject targetObject);
    CharacterAction GetMiscAction(ACTION_TYPE type);
    Attribute AddAttribute(ATTRIBUTE tag); //Character only
    void AddActionToQueue(CharacterAction action, IObject targetObject, Quest associatedQuest = null, int position = -1);
    void RemoveActionFromQueue(ActionQueueItem item);
}
