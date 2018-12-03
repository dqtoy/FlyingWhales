using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public interface ICharacter {
    //getters
    SIDES currentSide { get; }
    float actRate { get; set; }
    int level { get; }
    //int maxHP { get; }
    int currentHP { get; }
    int currentSP { get; }
    int maxSP { get; }
    int currentRow { get; }
    int id { get; }
    int hp { get; }
    int attackPower { get; }
    int speed { get; }
    int combatBaseAttack { get; set; }
    int combatBaseSpeed { get; set; }
    int combatBaseHP { get; set; }
    int combatAttackFlat { get; set; }
    int combatAttackMultiplier { get; set; }
    int combatSpeedFlat { get; set; }
    int combatSpeedMultiplier { get; set; }
    int combatHPFlat { get; set; }
    int combatHPMultiplier { get; set; }
    int combatPowerFlat { get; set; }
    int combatPowerMultiplier { get; set; }
    string coloredUrlName { get; }
    string urlName { get; }
    string name { get; }
    float computedPower { get; }
    bool isDead { get; }
    bool isBeingInspected { get; }
    int experience { get; }
    int maxExperience { get; }
    GENDER gender { get; }
    ICHARACTER_TYPE icharacterType { get; }
    MODE currentMode { get; }
    RACE race { get; }
    ILocation specificLocation { get; }
    CharacterBattleOnlyTracker battleOnlyTracker { get; }
    Faction faction { get; }
    BaseLandmark homeLandmark { get; }
    CharacterRole role { get; } //Character only
    CharacterClass characterClass { get; } //Character only
    Job job { get; } //Character only
    CharacterPortrait characterPortrait { get; }
    Weapon equippedWeapon { get; }
    Armor equippedArmor { get; }
    Item equippedAccessory { get; }
    Item equippedConsumable { get; }
    Minion minion { get; }
    PairCombatStats[] pairCombatStats { get; set; }
    Dictionary<ELEMENT, float> elementalWeaknesses { get; }
    Dictionary<ELEMENT, float> elementalResistances { get; }
    //Dictionary<Character, Relationship> relationships { get; }
    List<Skill> skills { get; }
    List<CharacterAction> miscActions { get; }
    List<CharacterAttribute> attributes { get; }
    List<Item> inventory { get; }
    List<Log> history { get; }
    List<Trait> traits { get; }
    PortraitSettings portraitSettings { get; }
    Party ownParty { get; }
    Party currentParty { get; }
    CharacterActionQueue<ActionQueueItem> actionQueue { get; }
    Dictionary<STAT, float> buffs { get; }
    PlayerCharacterItem playerCharacterItem { get; }
    WeightedDictionary<INTERACTION_TYPE> interactionWeights { get; }
    CharacterIntel characterIntel { get; }

    //functions
    void SetName(string name);
    //void ResetToFullHP();
    //void ResetToFullSP();
    void Initialize();
    void Death();
    void UpgradeWeapon();
    void UpgradeArmor();
    void UpgradeAccessory();
    void LevelUp();
    void OnRemovedFromParty();
    void OnAddedToParty();
    void OnAddedToPlayer();
    //void FaintOrDeath(ICharacter killer);
    void SetSide(ECS.SIDES side);
    void SetRowNumber(int row);
    void AdjustSP(int amount);
    //void AdjustHP(int amount, ICharacter killer = null);
    void AdjustExperience(int amount);
    void LevelUp(int amount);
    void SetLevel(int amount);
    void EnableDisableSkills(Combat combat);
    void SetOwnedParty(Party party);
    void SetCurrentParty(Party party);
    void SetHomeLandmark(BaseLandmark newHomeLandmark);
    void AddHistory(Log log); //Character only
    void SetMode(MODE mode);
    void AddMiscAction(CharacterAction characterAction);
    void RemoveMiscAction(ACTION_TYPE actionType);
    void SetMinion(Minion minion);
    void Assassinate(ICharacter assassin);
    void AddTrait(Trait combatAttribute);
    bool IsInParty();
    bool IsInOwnParty();
    bool InviteToParty(ICharacter inviter);
    bool RemoveTrait(Trait combatAttribute);
    Party CreateOwnParty();
    CharacterAttribute GetAttribute(string attribute);
    CharacterAction GetRandomMiscAction(ref IObject targetObject);
    CharacterAction GetMiscAction(ACTION_TYPE type);
    CharacterAttribute AddAttribute(ATTRIBUTE tag); //Character only
    Trait GetTrait(string name);
    void AddActionToQueue(CharacterAction action, IObject targetObject, Quest associatedQuest = null, int position = -1);
    void RemoveActionFromQueue(ActionQueueItem item);
    void ConstructBuffs();
    void AddBuff(Buff buff);
    void RemoveBuff(Buff buff);
    void SetPlayerCharacterItem(PlayerCharacterItem item);
    void DisableInteractionGeneration();
    void AddInteractionWeight(INTERACTION_TYPE type, int weight);
    void RemoveInteractionFromWeights(INTERACTION_TYPE type, int weight);
    void SetDailyInteractionGenerationTick();
    void DailyInteractionGeneration();
    void GenerateDailyInteraction();
}
