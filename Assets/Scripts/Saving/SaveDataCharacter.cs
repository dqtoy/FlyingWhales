using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGameFree.Types;

[System.Serializable]
public class SaveDataCharacter {
    public int id;
    public string name;
    public string characterColorCode;
    public int doNotDisturb;
    public int doNotGetHungry;
    public int doNotGetTired;
    public int doNotGetLonely;
    public int factionID;
    public int homeID;
    public int currentLocationID;
    public bool isDead;
    public GENDER gender;
    public SEXUALITY sexuality;
    public string className;
    public RACE race;
    public CHARACTER_ROLE roleType;
    public PortraitSettings portraitSettings;
    public ColorSave characterColor;
    public List<SaveDataTrait> normalTraits;

    //Stats
    public int currentHP;
    public int maxHP;
    public int level;
    public int experience;
    public int maxExperience;
    public int attackPowerMod;
    public int speedMod;
    public int maxHPMod;
    public int attackPowerPercentMod;
    public int speedPercentMod;
    public int maxHPPercentMod;

    public MORALITY morality;

    //public Dictionary<AlterEgoData, CharacterRelationshipData> relationships {
    //    get {
    //        return currentAlterEgo?.relationships ?? null;
    //    }
    //}
    //public Dictionary<POINT_OF_INTEREST_TYPE, List<IAwareness>> awareness {
    //    get {
    //        return currentAlterEgo.awareness;
    //    }
    //}

    public List<INTERACTION_TYPE> currentInteractionTypes;
    public int supply;
    public List<SaveDataItem> items;
    public int moodValue;
    public bool isCombatant;
    public bool isDisabledByPlayer;
    public float speedModifier;
    public string deathStr;
    public int isStoppedByOtherCharacter;

    public POI_STATE state;

    //Needs
    public int tiredness;
    public int fullness;
    public int happiness;
    public int fullnessDecreaseRate;
    public int tirednessDecreaseRate;
    public int happinessDecreaseRate;

    //portrait
    public float hSkinColor;
    public float hHairColor;
    public float demonColor;

    //hostility
    public int ignoreHostility;

    //alter egos
    public string currentAlterEgoName;
    public List<SaveDataAlterEgo> alterEgos;

    public string originalClassName;
    public bool isMinion;
    public bool isSummon;
    public bool isFactionLeader;

    public int fullnessForcedTick;
    public int tirednessForcedTick;
    public TIME_IN_WORDS forcedFullnessRecoveryTimeInWords;
    public TIME_IN_WORDS forcedTirednessRecoveryTimeInWords;

    public bool returnedToLife;

    //For Summons Only
    public SUMMON_TYPE summonType;

    public void Save(Character character) {
        id = character.id;
        name = character.name;
        characterColorCode = character.characterColorCode;
        doNotDisturb = character.doNotDisturb;
        doNotGetHungry = character.doNotGetHungry;
        doNotGetLonely = character.doNotGetLonely;
        doNotGetTired = character.doNotGetTired;
        if(character.faction != null) {
            factionID = character.faction.id;
        } else {
            factionID = -1;
        }
        if(character.homeArea != null) {
            homeID = character.homeArea.id;
        } else {
            homeID = -1;
        }
        if (character.specificLocation != null) {
            currentLocationID = character.specificLocation.id;
        } else {
            currentLocationID = -1;
        }
        isDead = character.isDead;
        gender = character.gender;
        sexuality = character.sexuality;
        className = character.characterClass.className;
        race = character.race;
        roleType = character.role.roleType;
        portraitSettings = character.portraitSettings;
        characterColor = character.characterColor;
        isStoppedByOtherCharacter = character.isStoppedByOtherCharacter;

        normalTraits = new List<SaveDataTrait>();
        for (int i = 0; i < character.normalTraits.Count; i++) {
            Trait trait = character.normalTraits[i];
            SaveDataTrait saveDataTrait = null;
            System.Type type = System.Type.GetType("SaveData" + trait.name);
            if (type != null) {
                saveDataTrait = System.Activator.CreateInstance(type) as SaveDataTrait;
            } else {
                saveDataTrait = new SaveDataTrait();
            }
            saveDataTrait.Save(trait);
            normalTraits.Add(saveDataTrait);
        }

        currentHP = character.currentHP;
        maxHP = character.maxHP;
        level = character.level;
        experience = character.experience;
        maxExperience = character.maxExperience;
        attackPowerMod = character.attackPowerMod;
        speedMod = character.speedMod;
        maxHPMod = character.maxHPMod;
        attackPowerPercentMod = character.attackPowerPercentMod;
        speedPercentMod = character.speedPercentMod;
        maxHPPercentMod = character.maxHPPercentMod;

        morality = character.morality;

        currentInteractionTypes = character.currentInteractionTypes;
        supply = character.supply;
        moodValue = character.moodValue;
        isCombatant = character.isCombatant;
        isDisabledByPlayer = character.isDisabledByPlayer;
        speedModifier = character.speedModifier;
        deathStr = character.deathStr;

        state = character.state;

        items = new List<SaveDataItem>();
        for (int i = 0; i < character.items.Count; i++) {
            SaveDataItem newSaveDataItem = new SaveDataItem();
            newSaveDataItem.Save(character.items[i]);
            items.Add(newSaveDataItem);
        }

        tiredness = character.tiredness;
        fullness = character.fullness;
        happiness = character.happiness;
        fullnessDecreaseRate = character.fullnessDecreaseRate;
        tirednessDecreaseRate = character.tirednessDecreaseRate;
        happinessDecreaseRate = character.happinessDecreaseRate;

        hSkinColor = character.hSkinColor;
        hHairColor = character.hHairColor;
        demonColor = character.demonColor;

        ignoreHostility = character.ignoreHostility;
        originalClassName = character.originalClassName;
        isMinion = character.minion != null;
        isSummon = character is Summon;
        if (isSummon) {
            Summon summon = character as Summon;
            summonType = summon.summonType;
        }
        isFactionLeader = character.faction.leader == character;

        currentAlterEgoName = character.currentAlterEgoName;
        alterEgos = new List<SaveDataAlterEgo>();
        foreach (AlterEgoData alterEgo in character.alterEgos.Values) {
            SaveDataAlterEgo saveDataAlterEgo = new SaveDataAlterEgo();
            saveDataAlterEgo.Save(alterEgo);
            alterEgos.Add(saveDataAlterEgo);
        }

        fullnessForcedTick = character.fullnessForcedTick;
        tirednessForcedTick = character.tirednessForcedTick;
        forcedFullnessRecoveryTimeInWords = character.forcedFullnessRecoveryTimeInWords;
        forcedTirednessRecoveryTimeInWords = character.forcedTirednessRecoveryTimeInWords;

        returnedToLife = character.returnedToLife;
    }

    public void Load() {
        Character character = null;
        if (isSummon) {
            character = CharacterManager.Instance.CreateNewSummon(this);
        } else {
            character = CharacterManager.Instance.CreateNewCharacter(this);
        }
        if (!isMinion && !isSummon && isDead) {
            //Do not process dead save data if character is a minion or summon, there is a separate process for that in Player
            character.ownParty.PartyDeath();
            if (character.role != null) {
                character.role.OnDeath(character);
            }
        }
    }

    public void LoadTraits(Character character) {
        for (int i = 0; i < normalTraits.Count; i++) {
            Character responsibleCharacter = null;
            Trait trait = normalTraits[i].Load(ref responsibleCharacter);
            character.AddTrait(trait, responsibleCharacter);
        }
        character.LoadAllStatsOfCharacter(this);
    }

    public void LoadRelationships(Character character) {
        for (int i = 0; i < alterEgos.Count; i++) {
            alterEgos[i].LoadRelationships(character);
        }
    }
}
