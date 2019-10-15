/*
 This is the base class for each faction (major/minor)
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;


public class Faction {
    public int id { get; protected set; }
    public string name { get; protected set; }
    public string description { get; protected set; }
    public string initialLeaderClass { get; protected set; }
    public int level { get; protected set; }
    public int inventoryTaskWeight { get; protected set; }
    public bool isPlayerFaction { get; protected set; }
    public GENDER initialLeaderGender { get; protected set; }
    public RACE initialLeaderRace { get; protected set; }
    public RACE race { get; protected set; }
    public ILeader leader { get; protected set; }
    public FactionEmblemSetting emblem { get; protected set; }
    public List<BaseLandmark> ownedLandmarks { get; protected set; }
    public Color factionColor { get; protected set; }
    public List<Character> characters { get; protected set; }//List of characters that are part of the faction
    public List<Region> ownedRegions { get; protected set; }
    public List<RACE> recruitableRaces { get; protected set; }
    public List<RACE> startingFollowers { get; protected set; }
    public Dictionary<Faction, FactionRelationship> relationships { get; protected set; }

    public MORALITY morality { get; private set; }
    public FACTION_SIZE size { get; private set; }
    public FACTION_TYPE factionType { get; private set; }
    public WeightedDictionary<AreaCharacterClass> additionalClassWeights { get; private set; }
    public bool isActive { get; private set; }
    public List<Log> history { get; private set; }
    public Region mainRegion { get { return ownedRegions[0]; } }
    public Quest activeQuest { get; protected set; }

    public string requirementForJoining { get; private set; }

    #region getters/setters
    public string urlName {
        get { return "<link=" + '"' + this.id.ToString() + "_faction" + '"' + ">" + this.name + "</link>"; }
    }
    public bool isDestroyed {
        get { return leader == null; }
    }
    #endregion

    public Faction(bool isPlayerFaction = false) {
        this.isPlayerFaction = isPlayerFaction;
        this.id = Utilities.SetID<Faction>(this);
        SetName(RandomNameGenerator.Instance.GenerateKingdomName());
        SetEmblem(FactionManager.Instance.GenerateFactionEmblem(this));
        SetFactionColor(Utilities.GetColorForFaction());
        SetRace(RACE.HUMANS);
        SetMorality(MORALITY.GOOD);
        SetSize(FACTION_SIZE.MAJOR);
        SetFactionActiveState(true);
        level = 1;
        inventoryTaskWeight = FactionManager.Instance.GetRandomInventoryTaskWeight();
        factionType = Utilities.GetRandomEnumValue<FACTION_TYPE>();
        characters = new List<Character>();
        ownedLandmarks = new List<BaseLandmark>();
        relationships = new Dictionary<Faction, FactionRelationship>();
        ownedRegions = new List<Region>();
        recruitableRaces = new List<RACE>();
        startingFollowers = new List<RACE>();
        history = new List<Log>();
        //favor = new Dictionary<Faction, int>();
        //defenderWeights = new WeightedDictionary<AreaCharacterClass>();
        additionalClassWeights = new WeightedDictionary<AreaCharacterClass>();
        GenerateFactionRequirementForJoining();
        //InitializeInteractions();
#if !WORLD_CREATION_TOOL
        //SetDailyInteractionGenerationTick();
        AddListeners();
#endif
    }

    public Faction(FactionSaveData data) {
        id = Utilities.SetID(this, data.factionID);
        SetName(data.factionName);
        SetDescription(data.factionDescription);
        SetFactionColor(data.factionColor);
        SetEmblem(FactionManager.Instance.GetFactionEmblem(data.emblemIndex));
        SetMorality(data.morality);
        SetSize(data.size);
        SetRace(data.race.race);
        SetLevel(data.level);
        SetFactionActiveState(data.isActive);
        initialLeaderClass = data.initialLeaderClass;
        initialLeaderRace = data.initialLeaderRace;
        initialLeaderGender = data.initialLeaderGender;
        recruitableRaces = data.recruitableRaces;
        startingFollowers = data.startingFollowers;

        inventoryTaskWeight = FactionManager.Instance.GetRandomInventoryTaskWeight();
        factionType = Utilities.GetRandomEnumValue<FACTION_TYPE>();
        characters = new List<Character>();
        ownedLandmarks = new List<BaseLandmark>();
        relationships = new Dictionary<Faction, FactionRelationship>();
        ownedRegions = new List<Region>();
        if (recruitableRaces == null) {
            recruitableRaces = new List<RACE>();
        }
        if (startingFollowers == null) {
            startingFollowers = new List<RACE>();
        }
        history = new List<Log>();
        //favor = new Dictionary<Faction, int>();
        //if (data.defenderWeights != null) {
        //    defenderWeights = new WeightedDictionary<AreaCharacterClass>(data.defenderWeights);
        //} else {
        //    defenderWeights = new WeightedDictionary<AreaCharacterClass>();
        //}
        additionalClassWeights = new WeightedDictionary<AreaCharacterClass>();
        GenerateFactionRequirementForJoining();
        //InitializeInteractions();
#if !WORLD_CREATION_TOOL
        //SetDailyInteractionGenerationTick();
        AddListeners();
#endif
    }
    public Faction(SaveDataFaction data) {
        this.isPlayerFaction = data.isPlayerFaction;
        this.id = Utilities.SetID(this, data.id);
        SetName(data.name);
        SetDescription(data.description);
        SetEmblem(FactionManager.Instance.GetFactionEmblem(data.emblemIndex));
        SetFactionColor(data.factionColor);
        SetRace(data.race);
        SetMorality(data.morality);
        SetSize(data.size);
        SetFactionActiveState(data.isActive);
        initialLeaderClass = data.initialLeaderClass;
        initialLeaderRace = data.initialLeaderRace;
        initialLeaderGender = data.initialLeaderGender;
        level = data.level;
        inventoryTaskWeight = data.inventoryTaskWeight;
        factionType = data.factionType;
        requirementForJoining = data.requirementForJoining;

        characters = new List<Character>();
        ownedLandmarks = new List<BaseLandmark>();
        relationships = new Dictionary<Faction, FactionRelationship>();
        ownedRegions = new List<Region>();
        recruitableRaces = new List<RACE>();
        startingFollowers = new List<RACE>();
        history = new List<Log>();
        //favor = new Dictionary<Faction, int>();
        //defenderWeights = new WeightedDictionary<AreaCharacterClass>();
        additionalClassWeights = new WeightedDictionary<AreaCharacterClass>();
        //InitializeInteractions();
#if !WORLD_CREATION_TOOL
        //SetDailyInteractionGenerationTick();
        AddListeners();
#endif
    }

    #region Virtuals
    /*
     Set the leader of this faction, change this per faction type if needed.
     This creates relationships between the leader and it's village heads by default.
         */
    public virtual void SetLeader(ILeader leader) {
        if (this.leader != null && this.leader is Character) {
            Character previousRuler = this.leader as Character;
            if(previousRuler.role.roleType != CHARACTER_ROLE.NOBLE) {
                previousRuler.AssignRole(CharacterRole.NOBLE);
                previousRuler.AssignClassByRole(previousRuler.role);
            }
            if (previousRuler.characterClass.className != previousRuler.GetClassForRole(previousRuler.role)) {
                previousRuler.AssignClassByRole(previousRuler.role);
            }
        }
        if (leader != null && leader is Character) {
            Character newRuler = leader as Character;
            if(newRuler.role.roleType != CHARACTER_ROLE.LEADER) {
                newRuler.AssignRole(CharacterRole.LEADER);
            }
            if (newRuler.characterClass.className != initialLeaderClass) {
                newRuler.AssignClass(CharacterManager.Instance.CreateNewCharacterClass(initialLeaderClass));
            }
        }
        OnlySetLeader(leader);
    }
    #endregion

    #region Landmarks
    public void OwnLandmark(BaseLandmark landmark) {
        if (!ownedLandmarks.Contains(landmark)) {
            ownedLandmarks.Add(landmark);
        }
    }
    public void UnownLandmark(BaseLandmark landmark) {
        ownedLandmarks.Remove(landmark);
    }
    #endregion

    #region Characters
    public void JoinFaction(Character character, bool processRequirement = true) {
        if(!processRequirement || DoesCharacterFitFactionRequirement(character)) {
            if (!characters.Contains(character)) {
                characters.Add(character);
                character.SetFaction(this);
                if (this != FactionManager.Instance.neutralFaction && character.role == CharacterRole.BANDIT) {
                    if (UnityEngine.Random.Range(0, 2) == 0) {
                        character.AssignRole(CharacterRole.SOLDIER);
                    } else {
                        character.AssignRole(CharacterRole.ADVENTURER);
                    }
                }
                Messenger.Broadcast(Signals.CHARACTER_ADDED_TO_FACTION, character, this);
            }
        }
    }
    public void LeaveFaction(Character character) {
        if (characters.Remove(character)) {
            if (leader == character) {
                SetNewLeader(); //so a new leader can be set if the leader is ever removed from the list of characters of this faction
            }
            character.SetFaction(null);
            Messenger.Broadcast(Signals.CHARACTER_REMOVED_FROM_FACTION, character, this);
        }
        //if (_leader != null && character.id == _leader.id) {
        //    SetLeader(null);
        //}
    }
    public bool DoesCharacterFitFactionRequirement(Character character) {
        if(requirementForJoining == string.Empty) {
            return true;
        }
        //TODO: must meet requirement
        return true;
    }
    private void GenerateFactionRequirementForJoining() {
        //TODO
        requirementForJoining = string.Empty;
    }

    public List<Character> GetCharactersOfType(CHARACTER_ROLE role) {
        List<Character> chars = new List<Character>();
        for (int i = 0; i < characters.Count; i++) {
            Character currCharacter = characters[i];
            if (currCharacter.role.roleType == role) {
                chars.Add(currCharacter);
            }
        }
        return chars;
    }
    public List<Character> GetViableCharacters(GENDER gender, params CHARACTER_ROLE[] role) {
        List<Character> chars = new List<Character>();
        for (int i = 0; i < characters.Count; i++) {
            Character currCharacter = characters[i];
            if (currCharacter.gender == gender && !currCharacter.isDead && !currCharacter.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER) && !currCharacter.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
                for (int j = 0; j < role.Length; j++) {
                    if(currCharacter.role.roleType == role[j]) {
                        chars.Add(currCharacter);
                        break;
                    }
                }
            }
        }
        return chars;
    }
    public void SetNewLeader() {
        if(leader != null) {
            Character previousRuler = leader as Character;
            Character newRuler = null;
            string log = GameManager.Instance.TodayLogString() + name + " faction is deciding a new leader...";

            log += "\nChecking male relatives of the king...";
            List<Character> maleRelatives = previousRuler.GetViableCharactersWithRelationship(GENDER.MALE, RELATIONSHIP_TRAIT.RELATIVE);
            if(maleRelatives.Count > 0) {
                newRuler = maleRelatives[UnityEngine.Random.Range(0, maleRelatives.Count)];
                log += "\nNew Ruler: " + newRuler.name;
            } else {
                log += "\nChecking male nobles...";
                List<Character> maleNobles = GetViableCharacters(GENDER.MALE, CHARACTER_ROLE.NOBLE);
                if(maleNobles.Count > 0) {
                    newRuler = maleNobles[UnityEngine.Random.Range(0, maleNobles.Count)];
                    log += "\nNew Ruler: " + newRuler.name;
                } else {
                    log += "\nChecking female relatives of the king...";
                    List<Character> femaleRelatives = previousRuler.GetViableCharactersWithRelationship(GENDER.FEMALE, RELATIONSHIP_TRAIT.RELATIVE);
                    if (femaleRelatives.Count > 0) {
                        newRuler = femaleRelatives[UnityEngine.Random.Range(0, femaleRelatives.Count)];
                        log += "\nNew Ruler: " + newRuler.name;
                    } else {
                        log += "\nChecking female nobles...";
                        List<Character> femaleNobles = GetViableCharacters(GENDER.FEMALE, CHARACTER_ROLE.NOBLE);
                        if (femaleNobles.Count > 0) {
                            newRuler = femaleNobles[UnityEngine.Random.Range(0, femaleNobles.Count)];
                            log += "\nNew Ruler: " + newRuler.name;
                        } else {
                            log += "\nChecking male soldiers and adventurers...";
                            List<Character> maleSoldiersAndAdventurers = GetViableCharacters(GENDER.MALE, CHARACTER_ROLE.SOLDIER, CHARACTER_ROLE.ADVENTURER);
                            if (maleSoldiersAndAdventurers.Count > 0) {
                                newRuler = maleSoldiersAndAdventurers[UnityEngine.Random.Range(0, maleSoldiersAndAdventurers.Count)];
                                log += "\nNew Ruler: " + newRuler.name;
                            } else {
                                log += "\nChecking female soldiers and adventurers...";
                                List<Character> femaleSoldiersAndAdventurers = GetViableCharacters(GENDER.FEMALE, CHARACTER_ROLE.SOLDIER, CHARACTER_ROLE.ADVENTURER);
                                if (femaleSoldiersAndAdventurers.Count > 0) {
                                    newRuler = femaleSoldiersAndAdventurers[UnityEngine.Random.Range(0, femaleSoldiersAndAdventurers.Count)];
                                    log += "\nNew Ruler: " + newRuler.name;
                                } else {
                                    log += "\nChecking male civilians...";
                                    List<Character> maleCivilians = GetViableCharacters(GENDER.MALE, CHARACTER_ROLE.CIVILIAN);
                                    if (maleCivilians.Count > 0) {
                                        newRuler = maleCivilians[UnityEngine.Random.Range(0, maleCivilians.Count)];
                                        log += "\nNew Ruler: " + newRuler.name;
                                    } else {
                                        log += "\nChecking female civilians...";
                                        List<Character> femaleCivilians = GetViableCharacters(GENDER.FEMALE, CHARACTER_ROLE.CIVILIAN);
                                        if (femaleCivilians.Count > 0) {
                                            newRuler = femaleCivilians[UnityEngine.Random.Range(0, femaleCivilians.Count)];
                                            log += "\nNew Ruler: " + newRuler.name;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            previousRuler.PrintLogIfActive(log);
            if(newRuler != null) {
                SetLeader(newRuler);

                Log logNotif = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "new_faction_leader");
                logNotif.AddToFillers(newRuler, newRuler.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                logNotif.AddToFillers(this, name, LOG_IDENTIFIER.FACTION_1);
                newRuler.AddHistory(logNotif);
                PlayerManager.Instance.player.ShowNotification(logNotif);
            } 
            //else {
            //    Debug.LogError(GameManager.Instance.TodayLogString() + name + " couldn't set a new leader replacing " + previousRuler.name);
            //}
        } else {
            Debug.LogError(GameManager.Instance.TodayLogString() + name + " cannot set new leader because there is no previous leader!");
        }
    }
    public void OnlySetLeader(ILeader leader) {
        this.leader = leader;
    }
    #endregion

    #region Utilities
    private void AddListeners() {
        Messenger.AddListener<Character>(Signals.CHARACTER_REMOVED, LeaveFaction);
        //Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        //if (!_isPlayerFaction) {
        //    Messenger.AddListener(Signals.TICK_STARTED, DailyInteractionGeneration);
        //}
    }
    private void RemoveListeners() {
        Messenger.RemoveListener<Character>(Signals.CHARACTER_REMOVED, LeaveFaction);
        //Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        //if (!_isPlayerFaction) {
        //    Messenger.RemoveListener(Signals.TICK_STARTED, DailyInteractionGeneration);
        //}
    }
    public void SetRace(RACE race) {
        this.race = race;
    }
    public void SetSize(FACTION_SIZE size) {
        this.size = size;
    }
    public void SetFactionColor(Color color) {
        factionColor = color;
    }
    public void SetName(string name) {
        this.name = name;
    }
    public void SetDescription(string description) {
        this.description = description;
    }
    public void SetInitialFactionLeaderGender(GENDER gender) {
        initialLeaderGender = gender;
    }
    public void SetInitialFactionLeaderClass(string className) {
        initialLeaderClass = className;
    }
    public void SetInitialFactionLeaderRace(RACE race) {
        initialLeaderRace = race;
    }
    public Character GetCharacterByID(int id) {
        for (int i = 0; i < characters.Count; i++) {
            if (characters[i].id == id) {
                return characters[i];
            }
        }
        return null;
    }
    public bool IsHostileWith(Faction faction) {
        if (faction.id == this.id) {
            return false;
        }
        FactionRelationship rel = GetRelationshipWith(faction);
        return rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.HOSTILE;
    }
    public bool HasLandmarkOfType(LANDMARK_TYPE landmarkType) {
        for (int i = 0; i < ownedLandmarks.Count; i++) {
            BaseLandmark currLandmark = ownedLandmarks[i];
            if (currLandmark.specificLandmarkType == landmarkType) {
                return true;
            }
        }
        return false;
    }
    public override string ToString() {
        return name;
    }
    //private void OnCharacterDied(Character characterThatDied) {
    //    if (leader != null && leader is Character && leader.id == characterThatDied.id) {
    //        Debug.Log(this.name + "'s Leader that died was " + characterThatDied.name);
    //        OnLeaderDied();
    //    }
    //}
    //private void OnLeaderDied() {
    //    Debug.Log(this.name + "'s leader died");
    //    SetNewLeader();
    //    //Messenger.Broadcast(Signals.FACTION_LEADER_DIED, this);
    //}
    public void SetLevel(int amount) {
        level = amount;
    }
    public void LevelUp() {
        level++;
    }
    public void LevelUp(int amount) {
        level += amount;
    }
    public void SetFactionActiveState(bool state) {
        if (isActive == state) {
            return; //ignore change
        }
        isActive = state;
        Messenger.Broadcast(Signals.FACTION_ACTIVE_CHANGED, this);
    }
    public void GenerateStartingCitizens(int leaderLevel, int citizensLevel) {
        Character leader = CharacterManager.Instance.CreateNewCharacter(CharacterRole.LEADER, initialLeaderClass, initialLeaderRace, initialLeaderGender,
                    this, mainRegion);
        leader.LevelUp(leaderLevel - 1);
        SetLeader(leader);
        //Debug.Log(GameManager.Instance.TodayLogString() + "LEADER Generated Lvl. " + leader.level.ToString() +
        //        " character " + leader.characterClass.className + " " + leader.name + " at " + this.name + " for faction " + leader.name);

        if (name == "Fyn") {
            int soldierCount = 4;
            int adventurerCount = 3;
            int civilianCount = 3;
#if TRAILER_BUILD
            soldierCount = 1;  
            adventurerCount = 1;
            civilianCount = 1;
#endif
            //**4 Human Soldiers**
            for (int i = 0; i < soldierCount; i++) {
                Character createdCharacter = CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, RACE.ELVES, Utilities.GetRandomGender(),
                    this, mainRegion);
                createdCharacter.LevelUp(citizensLevel - 1);
                //CharacterManager.Instance.CreateNewRelationshipBetween(leader, createdCharacter, RELATIONSHIP_TRAIT.SERVANT);
            }
            //**3 Human Adventurers**
            for (int i = 0; i < adventurerCount; i++) {
                Character adventurer = CharacterManager.Instance.CreateNewCharacter(CharacterRole.ADVENTURER, RACE.HUMANS, Utilities.GetRandomGender(),
                    this, mainRegion);
                adventurer.LevelUp(citizensLevel - 1);
            }
            //**3 Human Civilians**
            for (int i = 0; i < civilianCount; i++) {
                Character civilian = CharacterManager.Instance.CreateNewCharacter(CharacterRole.CIVILIAN, RACE.HUMANS, Utilities.GetRandomGender(),
                    this, mainRegion);
                civilian.LevelUp(citizensLevel - 1);
            }
        } else if (name == "Orelia") {
            //Female Elf Queen  with **3 Elven Soldiers** each as her servants
            for (int i = 0; i < 3; i++) {
                Character createdCharacter = CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, RACE.ELVES, Utilities.GetRandomGender(),
                    this, mainRegion);
                createdCharacter.LevelUp(citizensLevel - 1);
                //CharacterManager.Instance.CreateNewRelationshipBetween(leader, createdCharacter, RELATIONSHIP_TRAIT.SERVANT);
            }

            //**2 Elven Nobles**
            for (int i = 0; i < 2; i++) {
                Character noble = CharacterManager.Instance.CreateNewCharacter(CharacterRole.NOBLE, RACE.ELVES, Utilities.GetRandomGender(),
                    this, mainRegion);
                noble.LevelUp(citizensLevel - 1);
            }

            //**2 Elven Adventurer**
            for (int i = 0; i < 2; i++) {
                Character elvenAdventurer = CharacterManager.Instance.CreateNewCharacter(CharacterRole.ADVENTURER, RACE.ELVES, Utilities.GetRandomGender(),
                   this, mainRegion);
                elvenAdventurer.LevelUp(citizensLevel - 1);
            }

            //**1 Elven Civilian**
            for (int i = 0; i < 1; i++) {
                Character elvenCivilian = CharacterManager.Instance.CreateNewCharacter(CharacterRole.CIVILIAN, RACE.ELVES, Utilities.GetRandomGender(),
                   this, mainRegion);
                elvenCivilian.LevelUp(citizensLevel - 1);
            }

        } else if (name == "Ziranna") {
            //Male Human Necromancer with **2 Skeleton Soldiers** and **1 Goblin Soldier** as his Servants
            for (int i = 0; i < 2; i++) {
                Character skeletonSoldier = CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, RACE.SKELETON, Utilities.GetRandomGender(),
                    this, mainRegion);
                skeletonSoldier.LevelUp(citizensLevel - 1);
                //CharacterManager.Instance.CreateNewRelationshipBetween(leader, skeletonSoldier, RELATIONSHIP_TRAIT.SERVANT);

                if (i == 0) {
                    Character goblinSoldier = CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, RACE.GOBLIN, Utilities.GetRandomGender(),
                   this, mainRegion);
                    goblinSoldier.LevelUp(citizensLevel - 1);
                    //CharacterManager.Instance.CreateNewRelationshipBetween(leader, goblinSoldier, RELATIONSHIP_TRAIT.SERVANT);
                }
            }
        } else if (name == "Rikitik") {
            //Male Goblin Bandit Boss with **4 Goblin Soldiers** as Servants
            for (int i = 0; i < 4; i++) {
                Character goblinSoldier = CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, RACE.GOBLIN, Utilities.GetRandomGender(),
                    this, mainRegion);
                goblinSoldier.LevelUp(citizensLevel - 1);
                //CharacterManager.Instance.CreateNewRelationshipBetween(leader, goblinSoldier, RELATIONSHIP_TRAIT.SERVANT);
            }

            //**1 Goblin Noble** with **3 Goblin Soldiers** as servants
            for (int i = 0; i < 1; i++) {
                Character noble = CharacterManager.Instance.CreateNewCharacter(CharacterRole.NOBLE, RACE.GOBLIN, Utilities.GetRandomGender(),
                    this, mainRegion);
                noble.LevelUp(citizensLevel - 1);

                for (int j = 0; j < 3; j++) {
                    Character createdCharacter = CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, RACE.GOBLIN, Utilities.GetRandomGender(),
                   this, mainRegion);
                    createdCharacter.LevelUp(citizensLevel - 1);
                    //CharacterManager.Instance.CreateNewRelationshipBetween(noble, createdCharacter, RELATIONSHIP_TRAIT.SERVANT);
                }
            }

            //**3 Goblin Civilians**
            for (int i = 0; i < 3; i++) {
                Character goblinCivilian = CharacterManager.Instance.CreateNewCharacter(CharacterRole.CIVILIAN, RACE.GOBLIN, Utilities.GetRandomGender(),
                           this, mainRegion);
                goblinCivilian.LevelUp(citizensLevel - 1);
            }
        } else if (name == "Caeven") {
            //Male Faery Tempest with **3 Faery Soldiers** as servants
            for (int i = 0; i < 3; i++) {
                Character faerySoldier = CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, RACE.FAERY, Utilities.GetRandomGender(),
                    this, mainRegion);
                faerySoldier.LevelUp(citizensLevel - 1);
                //CharacterManager.Instance.CreateNewRelationshipBetween(leader, faerySoldier, RELATIONSHIP_TRAIT.SERVANT);
            }

            //**1 Faery Adventurers**
            //**2 Faery Civilians**
            for (int i = 0; i < 2; i++) {
                if (i < 1) {
                    Character faeryAdventurer = CharacterManager.Instance.CreateNewCharacter(CharacterRole.ADVENTURER, RACE.FAERY, Utilities.GetRandomGender(),
                           this, mainRegion);
                    faeryAdventurer.LevelUp(citizensLevel - 1);
                }

                Character faeryCivilian = CharacterManager.Instance.CreateNewCharacter(CharacterRole.CIVILIAN, RACE.FAERY, Utilities.GetRandomGender(),
                           this, mainRegion);
                faeryCivilian.LevelUp(citizensLevel - 1);
            }
        } else if (name == "Lucareth") {
            //Female Faery Witch with **3 Spider Soldiers**
            for (int i = 0; i < 3; i++) {
                Character spiderSoldier = CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, RACE.SPIDER, Utilities.GetRandomGender(),
                    this, mainRegion);
                spiderSoldier.LevelUp(citizensLevel - 1);
                //CharacterManager.Instance.CreateNewRelationshipBetween(leader, spiderSoldier, RELATIONSHIP_TRAIT.SERVANT);
            }

            //**3 Faery Nobles** with **3 Spider Soldiers** each as their servants
            for (int i = 0; i < 3; i++) {
                Character noble = CharacterManager.Instance.CreateNewCharacter(CharacterRole.NOBLE, RACE.FAERY, Utilities.GetRandomGender(),
                    this, mainRegion);
                noble.LevelUp(citizensLevel - 1);

                for (int j = 0; j < 3; j++) {
                    Character createdCharacter = CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, RACE.SPIDER, Utilities.GetRandomGender(),
                   this, mainRegion);
                    createdCharacter.LevelUp(citizensLevel - 1);
                    //CharacterManager.Instance.CreateNewRelationshipBetween(noble, createdCharacter, RELATIONSHIP_TRAIT.SERVANT);
                }
            }

            //**4 Faery Adventurers**
            //**4 Faery Civilians**
            for (int i = 0; i < 4; i++) {
                Character faeryAdventurer = CharacterManager.Instance.CreateNewCharacter(CharacterRole.ADVENTURER, RACE.FAERY, Utilities.GetRandomGender(),
                           this, mainRegion);
                faeryAdventurer.LevelUp(citizensLevel - 1);

                Character faeryCivilian = CharacterManager.Instance.CreateNewCharacter(CharacterRole.CIVILIAN, RACE.FAERY, Utilities.GetRandomGender(),
                           this, mainRegion);
                faeryCivilian.LevelUp(citizensLevel - 1);
            }
        } else if (name == "Magus") {
            //Male Elf Archmage with **1 Elf Soldier** and **1 Human Soldier** as Servants
            for (int i = 0; i < 1; i++) {
                Character elfSoldier = CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, RACE.ELVES, Utilities.GetRandomGender(),
                    this, mainRegion);
                elfSoldier.LevelUp(citizensLevel - 1);
                //CharacterManager.Instance.CreateNewRelationshipBetween(leader, elfSoldier, RELATIONSHIP_TRAIT.SERVANT);

                Character humanSoldier = CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, RACE.HUMANS, Utilities.GetRandomGender(),
                   this, mainRegion);
                humanSoldier.LevelUp(citizensLevel - 1);
                //CharacterManager.Instance.CreateNewRelationshipBetween(leader, humanSoldier, RELATIONSHIP_TRAIT.SERVANT);
            }

            //**2 Human Adventurers** and **2 Elven Adventurers**
            //**2 Human Civilians** and **2 Elven Civilians**
            for (int i = 0; i < 2; i++) {
                Character humanAdventurer = CharacterManager.Instance.CreateNewCharacter(CharacterRole.ADVENTURER, RACE.HUMANS, Utilities.GetRandomGender(),
                           this, mainRegion);
                humanAdventurer.LevelUp(citizensLevel - 1);

                Character elfAdventurer = CharacterManager.Instance.CreateNewCharacter(CharacterRole.ADVENTURER, RACE.ELVES, Utilities.GetRandomGender(),
                           this, mainRegion);
                elfAdventurer.LevelUp(citizensLevel - 1);

                Character humanCivilian = CharacterManager.Instance.CreateNewCharacter(CharacterRole.CIVILIAN, RACE.HUMANS, Utilities.GetRandomGender(),
                           this, mainRegion);
                humanCivilian.LevelUp(citizensLevel - 1);

                Character elfCivilian = CharacterManager.Instance.CreateNewCharacter(CharacterRole.CIVILIAN, RACE.ELVES, Utilities.GetRandomGender(),
                           this, mainRegion);
                elfCivilian.LevelUp(citizensLevel - 1);
            }
        }
    }
    public void GenerateStartingCitizens(int leaderLevel, int citizensLevel, int citizenCount) {
        for (int i = 0; i < citizenCount; i++) {
            if (i == 0) {
                //leader
                Character leader = CharacterManager.Instance.CreateNewCharacter(CharacterRole.LEADER, initialLeaderClass, race, initialLeaderGender, this, mainRegion);
                leader.LevelUp(leaderLevel - 1);
                SetLeader(leader);
            } else {
                WeightedDictionary<CharacterRole> roleChoices = new WeightedDictionary<CharacterRole>();
                roleChoices.AddElement(CharacterRole.CIVILIAN, 30);
                roleChoices.AddElement(CharacterRole.ADVENTURER, 35);
                roleChoices.AddElement(CharacterRole.SOLDIER, 35);

                //citizens
                Character citizen = CharacterManager.Instance.CreateNewCharacter(roleChoices.PickRandomElementGivenWeights(), race, Utilities.GetRandomGender(), this, mainRegion);
                citizen.LevelUp(citizensLevel - 1);
            }
        }
        mainRegion.area.SetInitialResidentCount(citizenCount);
    }
    #endregion

    #region Relationships
    public void AddNewRelationship(Faction relWith, FactionRelationship relationship) {
        if (!relationships.ContainsKey(relWith)) {
            relationships.Add(relWith, relationship);
        } else {
            throw new System.Exception(this.name + " already has a relationship with " + relWith.name + ", but something is trying to create a new one!");
        }
    }
    public void RemoveRelationshipWith(Faction relWith) {
        if (relationships.ContainsKey(relWith)) {
            relationships.Remove(relWith);
        }
    }
    public FactionRelationship GetRelationshipWith(Faction faction) {
        if (relationships.ContainsKey(faction)) {
            return relationships[faction];
        }
        return null;
    }
    public bool HasRelationshipStatus(FACTION_RELATIONSHIP_STATUS stat, bool excludePlayer = true) {
        foreach (KeyValuePair<Faction, FactionRelationship> kvp in relationships) {
            if (excludePlayer && kvp.Key.id == PlayerManager.Instance.player.playerFaction.id) {
                continue; //exclude player faction
            }
            if (kvp.Value.relationshipStatus == stat) {
                return true;
            }
        }
        return false;
    }
    public bool HasRelationshipStatusWith(FACTION_RELATIONSHIP_STATUS stat, Faction faction) {
        if (relationships.ContainsKey(faction)) {
            return relationships[faction].relationshipStatus == stat;
        }
        return false;
    }
    public Faction GetFactionWithRelationship(FACTION_RELATIONSHIP_STATUS stat, bool excludePlayer = true) {
        foreach (KeyValuePair<Faction, FactionRelationship> kvp in relationships) {
            if (excludePlayer && kvp.Key.id == PlayerManager.Instance.player.playerFaction.id) {
                continue; //exclude player faction
            }
            if (kvp.Value.relationshipStatus == stat) {
                return kvp.Key;
            }
        }
        return null;
    }
    public List<Faction> GetFactionsWithRelationship(FACTION_RELATIONSHIP_STATUS stat, bool excludePlayer = true) {
        List<Faction> factions = new List<Faction>();
        foreach (KeyValuePair<Faction, FactionRelationship> kvp in relationships) {
            if (excludePlayer && kvp.Key.id == PlayerManager.Instance.player.playerFaction.id) {
                continue; //exclude player faction
            }
            if (kvp.Value.relationshipStatus == stat) {
                factions.Add(kvp.Key);
            }
        }
        return factions;
    }
    public void AdjustRelationshipFor(Faction otherFaction, int adjustment) {
        if (relationships.ContainsKey(otherFaction)) {
            relationships[otherFaction].AdjustRelationshipStatus(adjustment);
        } else {
            Debug.LogWarning("There is no key for " + otherFaction.name + " in " + this.name + "'s relationship dictionary");
        }
    }
    public void SetRelationshipFor(Faction otherFaction, FACTION_RELATIONSHIP_STATUS status) {
        if (relationships.ContainsKey(otherFaction)) {
            relationships[otherFaction].SetRelationshipStatus(status);
        } else {
            Debug.LogWarning("There is no key for " + otherFaction.name + " in " + this.name + "'s relationship dictionary");
        }
    }
    public bool IsAtWar() {
        foreach (KeyValuePair<Faction, FactionRelationship> kvp in relationships) {
            if (kvp.Key.isActive && kvp.Value.relationshipStatus == FACTION_RELATIONSHIP_STATUS.HOSTILE) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Death
    public void Death() {
        RemoveListeners();
        FactionManager.Instance.RemoveRelationshipsWith(this);
    }
    #endregion

    #region Landmarks
    public BaseLandmark GetOwnedLandmarkOfType(LANDMARK_TYPE landmarkType) {
        for (int i = 0; i < ownedLandmarks.Count; i++) {
            BaseLandmark currLandmark = ownedLandmarks[i];
            if (currLandmark.specificLandmarkType == landmarkType) {
                return currLandmark;
            }
        }
        return null;
    }
    #endregion

    #region Areas
    public void AddToOwnedRegions(Region region) {
        if (!ownedRegions.Contains(region)) {
            ownedRegions.Add(region);
            Messenger.Broadcast(Signals.FACTION_OWNED_REGION_ADDED, this, region);
        }
    }
    public void RemoveFromOwnedRegions(Region region) {
        if (ownedRegions.Remove(region)) {
            Messenger.Broadcast(Signals.FACTION_OWNED_REGION_REMOVED, this, region);
        }
    }
    #endregion

    #region Emblems
    public void SetEmblem(FactionEmblemSetting sprite) {
        emblem = sprite;
    }
    #endregion

    #region Morality
    public void SetMorality(MORALITY morality) {
        this.morality = morality;
    }
    #endregion

    #region Class Weights
    public void AddClassWeight(string className, int weight) {
        additionalClassWeights.AddElement(new AreaCharacterClass(className), weight);
    }
    #endregion

    #region Logs
    public void AddHistory(Log log) {
        if (!history.Contains(log)) {
            history.Add(log);
            if (this.history.Count > 60) {
                if (this.history[0].goapAction != null) {
                    this.history[0].goapAction.AdjustReferenceCount(-1);
                }
                this.history.RemoveAt(0);
            }
            if (log.goapAction != null) {
                log.goapAction.AdjustReferenceCount(1);
            }
            Messenger.Broadcast(Signals.HISTORY_ADDED, this as object);
        }
    }
    #endregion

    #region Quests
    public void CreateAndSetActiveQuest(string name, Region region) {
        var typeName = Utilities.RemoveAllWhiteSpace(name) + "Quest";
        System.Type type = System.Type.GetType(typeName);
        Quest quest = null;
        if(type != null) {
            quest = System.Activator.CreateInstance(type, this, region) as Quest;
        } else {
            quest = new Quest(this, region);
        }
        SetActiveQuest(quest);
    }
    public void SetActiveQuest(Quest quest) {
        if(activeQuest != null) {
            activeQuest.FinishQuest();
        }
        activeQuest = quest;
        if(activeQuest != null) {
            activeQuest.ActivateQuest();
        }
    }
    #endregion
}
public struct FactionTaskWeight {
    public int baseWeight; //Must not be changed by area
    public int areaWeight;
    public int supplyCost;
    public bool areaCannotDoTask;
    public bool factionCannotDoTask;
}
