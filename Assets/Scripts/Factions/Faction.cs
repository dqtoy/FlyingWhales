/*
 This is the base class for each faction (major/minor)
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;


public class Faction {
    protected int _id;
    protected string _name;
    protected string _description;
    protected string _initialLeaderClass;
    protected int _level;
    protected int _currentInteractionTick;
    protected int _usedMonthForInteraction;
    protected int _inventoryTaskWeight;
    protected int _offenseTaskNumOfReserved;
    protected int _supplyTaskNumOfReserved;
    protected bool _isPlayerFaction;
    protected GENDER _initialLeaderGender;
    protected RACE _initialLeaderRace;
    protected Race _race;
    protected ILeader _leader;
    protected FactionEmblemSetting _emblem;
    protected List<Region> _ownedRegions;
    protected List<BaseLandmark> _ownedLandmarks;
    protected Color _factionColor;
    protected List<Character> _characters; //List of characters that are part of the faction
    protected List<BaseLandmark> _landmarkInfo;
    protected List<Area> _ownedAreas;
    protected List<RACE> _recruitableRaces;
    protected List<RACE> _startingFollowers;
    protected Dictionary<Faction, FactionRelationship> _relationships;
    protected Dictionary<INTERACTION_TYPE, int> _nonNeutralInteractionTypes;
    protected Dictionary<INTERACTION_TYPE, int> _neutralInteractionTypes;
    protected Dictionary<INTERACTION_CATEGORY, FactionTaskWeight> _factionTasksWeights;

    public MORALITY morality { get; private set; }
    public FACTION_SIZE size { get; private set; }
    public FACTION_TYPE factionType { get; private set; }
    public FactionToken factionToken { get; private set; }
    public WeightedDictionary<AreaCharacterClass> additionalClassWeights { get; private set; }
    public bool isActive { get; private set; }
    public List<Log> history { get; private set; }

    #region getters/setters
    public int id {
        get { return _id; }
    }
    public int level {
        get { return _level; }
    }
    public string name {
        get { return _name; }
    }
    public string urlName {
        get { return "<link=" + '"' + this._id.ToString() + "_faction" + '"' + ">" + this._name + "</link>"; }
    }
    public string description {
        get { return _description; }
    }
    public string initialLeaderClass {
        get { return _initialLeaderClass; }
    }
    public bool isDestroyed {
        get { return _leader == null; }
    }
    public RACE raceType {
        get { return _race.race; }
    }
    public RACE_SUB_TYPE subRaceType {
        get { return _race.subType; }
    }
    public GENDER initialLeaderGender {
        get { return _initialLeaderGender; }
    }
    public RACE initialLeaderRace {
        get { return _initialLeaderRace; }
    }
    public ILeader leader {
        get { return _leader; }
    }
    public Race race {
        get { return _race; }
    }
    public FactionEmblemSetting emblem {
        get { return _emblem; }
    }
    public Color factionColor {
        get { return _factionColor; }
    }
    public List<Character> characters {
        get { return _characters; }
    }
    public List<Region> ownedRegions {
        get { return _ownedRegions; }
    }
    public Dictionary<Faction, FactionRelationship> relationships {
        get { return _relationships; }
    }
    public List<BaseLandmark> landmarkInfo {
        get { return _landmarkInfo; }
    }
    public List<BaseLandmark> ownedLandmarks {
        get { return _ownedLandmarks; }
    }
    public List<Area> ownedAreas {
        get { return _ownedAreas; }
    }
    public List<RACE> recruitableRaces {
        get { return _recruitableRaces; }
    }
    public List<RACE> startingFollowers {
        get { return _startingFollowers; }
    }
    public bool isNeutral {
        get { return this.id == FactionManager.Instance.neutralFaction.id; }
    }
    #endregion

    public Faction(bool isPlayerFaction = false) {
        _isPlayerFaction = isPlayerFaction;
        this._id = Utilities.SetID<Faction>(this);
        SetName(RandomNameGenerator.Instance.GenerateKingdomName());
        SetEmblem(FactionManager.Instance.GenerateFactionEmblem(this));
        SetFactionColor(Utilities.GetColorForFaction());
        SetRace(new Race(RACE.HUMANS, RACE_SUB_TYPE.NORMAL));
        SetMorality(MORALITY.GOOD);
        SetSize(FACTION_SIZE.MAJOR);
        SetFactionActiveState(true);
        _level = 1;
        _inventoryTaskWeight = FactionManager.Instance.GetRandomInventoryTaskWeight();
        factionType = Utilities.GetRandomEnumValue<FACTION_TYPE>();
        _characters = new List<Character>();
        _ownedLandmarks = new List<BaseLandmark>();
        _ownedRegions = new List<Region>();
        _relationships = new Dictionary<Faction, FactionRelationship>();
        _landmarkInfo = new List<BaseLandmark>();
        _ownedAreas = new List<Area>();
        _recruitableRaces = new List<RACE>();
        _startingFollowers = new List<RACE>();
        factionToken = new FactionToken(this);
        history = new List<Log>();
        //favor = new Dictionary<Faction, int>();
        //defenderWeights = new WeightedDictionary<AreaCharacterClass>();
        additionalClassWeights = new WeightedDictionary<AreaCharacterClass>();
        //InitializeInteractions();
        ConstructFactionTasksWeights();
#if !WORLD_CREATION_TOOL
        //SetDailyInteractionGenerationTick();
        AddListeners();
#endif
    }

    public Faction(FactionSaveData data) {
        _id = Utilities.SetID(this, data.factionID);
        SetName(data.factionName);
        SetDescription(data.factionDescription);
        SetFactionColor(data.factionColor);
        SetEmblem(FactionManager.Instance.GetFactionEmblem(data.emblemIndex));
        SetMorality(data.morality);
        SetSize(data.size);
        SetRace(data.race);
        SetLevel(data.level);
        SetFactionActiveState(data.isActive);
        _initialLeaderClass = data.initialLeaderClass;
        _initialLeaderRace = data.initialLeaderRace;
        _initialLeaderGender = data.initialLeaderGender;
        _recruitableRaces = data.recruitableRaces;
        _startingFollowers = data.startingFollowers;

        _inventoryTaskWeight = FactionManager.Instance.GetRandomInventoryTaskWeight();
        factionType = Utilities.GetRandomEnumValue<FACTION_TYPE>();
        _characters = new List<Character>();
        _ownedLandmarks = new List<BaseLandmark>();
        _ownedRegions = new List<Region>();
        _relationships = new Dictionary<Faction, FactionRelationship>();
        _landmarkInfo = new List<BaseLandmark>();
        _ownedAreas = new List<Area>();
        if (_recruitableRaces == null) {
            _recruitableRaces = new List<RACE>();
        }
        if (_startingFollowers == null) {
            _startingFollowers = new List<RACE>();
        }
        factionToken = new FactionToken(this);
        history = new List<Log>();
        //favor = new Dictionary<Faction, int>();
        //if (data.defenderWeights != null) {
        //    defenderWeights = new WeightedDictionary<AreaCharacterClass>(data.defenderWeights);
        //} else {
        //    defenderWeights = new WeightedDictionary<AreaCharacterClass>();
        //}
        additionalClassWeights = new WeightedDictionary<AreaCharacterClass>();
        //InitializeInteractions();
        ConstructFactionTasksWeights();
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
        if (_leader != null && _leader is Character) {
            Character character = _leader as Character;
        }

        _leader = leader;
        if (_leader != null && _leader is Character) {
            Character character = _leader as Character;
            if (character.job.jobType != JOB.LEADER) {
                character.AssignJob(JOB.LEADER);
            }
        }
    }
    #endregion

    #region Regions
    public void OwnRegion(Region region) {
        if (!_ownedRegions.Contains(region)) {
            _ownedRegions.Add(region);
        }
    }
    public void UnownRegion(Region region) {
        _ownedRegions.Remove(region);
    }
    #endregion

    #region Landmarks
    public void OwnLandmark(BaseLandmark landmark) {
        if (!_ownedLandmarks.Contains(landmark)) {
            _ownedLandmarks.Add(landmark);
        }
    }
    public void UnownLandmark(BaseLandmark landmark) {
        _ownedLandmarks.Remove(landmark);
    }
    #endregion

    #region Characters
    public void AddNewCharacter(Character character) {
        if (!_characters.Contains(character)) {
            _characters.Add(character);
            character.SetFaction(this);
            if(this.id != FactionManager.Instance.neutralFaction.id && character.role == CharacterRole.BANDIT) {
                if(UnityEngine.Random.Range(0, 2) == 0) {
                    character.AssignRole(CharacterRole.SOLDIER);
                } else {
                    character.AssignRole(CharacterRole.ADVENTURER);
                }
            }
            Messenger.Broadcast(Signals.CHARACTER_ADDED_TO_FACTION, character, this);
        }
    }
    public void RemoveCharacter(Character character) {
        if (_characters.Remove(character)) {
            character.SetFaction(null);
            Messenger.Broadcast(Signals.CHARACTER_REMOVED_FROM_FACTION, character, this);
        }
        //if (_leader != null && character.id == _leader.id) {
        //    SetLeader(null);
        //}
    }
    public List<Character> GetCharactersOfType(CHARACTER_ROLE role) {
        List<Character> chars = new List<Character>();
        for (int i = 0; i < _characters.Count; i++) {
            Character currCharacter = _characters[i];
            if (currCharacter.role.roleType == role) {
                chars.Add(currCharacter);
            }
        }
        return chars;
    }
    #endregion

    #region Utilities
    private void AddListeners() {
        Messenger.AddListener<Character>(Signals.CHARACTER_REMOVED, RemoveCharacter);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        //if (!_isPlayerFaction) {
        //    Messenger.AddListener(Signals.TICK_STARTED, DailyInteractionGeneration);
        //}
    }
    private void RemoveListeners() {
        Messenger.RemoveListener<Character>(Signals.CHARACTER_REMOVED, RemoveCharacter);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        //if (!_isPlayerFaction) {
        //    Messenger.RemoveListener(Signals.TICK_STARTED, DailyInteractionGeneration);
        //}
    }
    public void SetRace(Race race) {
        _race = race;
    }
    public void SetRaceType(RACE race) {
        _race.race = race;
    }
    public void SetSubRaceType(RACE_SUB_TYPE race) {
        _race.subType = race;
    }
    public void SetSize(FACTION_SIZE size) {
        this.size = size;
    }
    public void SetFactionColor(Color color) {
        _factionColor = color;
    }
    public void SetName(string name) {
        _name = name;
    }
    public void SetDescription(string description) {
        _description = description;
    }
    public void SetInitialFactionLeaderGender(GENDER gender) {
        _initialLeaderGender = gender;
    }
    public void SetInitialFactionLeaderClass(string className) {
        _initialLeaderClass = className;
    }
    public void SetInitialFactionLeaderRace(RACE race) {
        _initialLeaderRace = race;
    }
    public Character GetCharacterByID(int id) {
        for (int i = 0; i < _characters.Count; i++) {
            if (_characters[i].id == id) {
                return _characters[i];
            }
        }
        return null;
    }
    public bool IsHostileWith(Faction faction) {
        if (faction.id == this.id) {
            return false;
        }
        FactionRelationship rel = GetRelationshipWith(faction);
        return rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ENEMY;
    }
    public bool HasLandmarkOfType(LANDMARK_TYPE landmarkType) {
        for (int i = 0; i < _ownedLandmarks.Count; i++) {
            BaseLandmark currLandmark = _ownedLandmarks[i];
            if (currLandmark.specificLandmarkType == landmarkType) {
                return true;
            }
        }
        return false;
    }
    public bool HasAccessToLandmarkOfType(LANDMARK_TYPE landmarkType) {
        for (int i = 0; i < _ownedRegions.Count; i++) {
            Region currRegion = _ownedRegions[i];
            if (currRegion.HasLandmarkOfType(landmarkType)) {
                return true;
            }
        }
        return false;
    }
    public BaseLandmark GetAccessibleLandmarkOfType(LANDMARK_TYPE landmarkType) {
        for (int i = 0; i < _ownedRegions.Count; i++) {
            Region currRegion = _ownedRegions[i];
            if (currRegion.HasLandmarkOfType(landmarkType)) {
                return currRegion.GetLandmarksOfType(landmarkType).First();
            }
        }
        return null;
    }
    public override string ToString() {
        return name;
    }
    private void OnCharacterDied(Character characterThatDied) {
        if (leader != null && leader is Character && leader.id == characterThatDied.id) {
            Debug.Log(this.name + "'s Leader that died was " + characterThatDied.name);
            OnLeaderDied();
        }
    }
    private void OnLeaderDied() {
        Debug.Log(this.name + "'s leader died");
        SetLeader(null);
        //kill all characters in faction and un own all areas
        List<Character> remainingCharacters = new List<Character>(characters);
        for (int i = 0; i < remainingCharacters.Count; i++) {
            remainingCharacters[i].ChangeFactionTo(FactionManager.Instance.neutralFaction);
        }
        List<Area> areasToUnown = new List<Area>(ownedAreas);
        for (int i = 0; i < areasToUnown.Count; i++) {
            LandmarkManager.Instance.UnownArea(areasToUnown[i]);
            FactionManager.Instance.neutralFaction.AddToOwnedAreas(areasToUnown[i]);
        }
        Messenger.Broadcast(Signals.FACTION_LEADER_DIED, this);
        Messenger.Broadcast(Signals.FACTION_DIED, this);
    }
    public void SetLevel(int amount) {
        _level = amount;
    }
    public void LevelUp() {
        _level++;
    }
    public void LevelUp(int amount) {
        _level += amount;
    }
    public void SetFactionActiveState(bool state) {
        if (isActive == state) {
            return; //ignore change
        }
        isActive = state;
        Messenger.Broadcast(Signals.FACTION_ACTIVE_CHANGED, this);
    }
    public void GenerateStartingCitizens(int leaderLevel, int citizensLevel) {
        Character leader = CharacterManager.Instance.CreateNewCharacter(CharacterRole.LEADER, _initialLeaderClass, _initialLeaderRace, _initialLeaderGender,
                    this, _ownedAreas[0]);
        leader.LevelUp(leaderLevel - 1);
        SetLeader(leader);
        //Debug.Log(GameManager.Instance.TodayLogString() + "LEADER Generated Lvl. " + leader.level.ToString() +
        //        " character " + leader.characterClass.className + " " + leader.name + " at " + this.name + " for faction " + leader.name);

        if (_name == "Fyn") {
            //Male Human King with **3 Human Soldiers** as his servants
            for (int i = 0; i < 2; i++) {
                Character createdCharacter = CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, RACE.HUMANS, Utilities.GetRandomGender(),
                    this, _ownedAreas[0]);
                createdCharacter.LevelUp(citizensLevel - 1);
                CharacterManager.Instance.CreateNewRelationshipBetween(leader, createdCharacter, RELATIONSHIP_TRAIT.SERVANT);
            }

            /*
            //**2 Human Nobles** with **2 Human Soldiers** each as their servants
            for (int i = 0; i < 2; i++) {
                Character noble = CharacterManager.Instance.CreateNewCharacter(CharacterRole.NOBLE, RACE.HUMANS, Utilities.GetRandomGender(),
                    this, _ownedAreas[0]);
                noble.LevelUp(citizensLevel - 1);

                for (int j = 0; j < 2; j++) {
                    Character createdCharacter = CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, RACE.HUMANS, Utilities.GetRandomGender(),
                   this, _ownedAreas[0]);
                    createdCharacter.LevelUp(citizensLevel - 1);
                    CharacterManager.Instance.CreateNewRelationshipBetween(noble, createdCharacter, RELATIONSHIP_TRAIT.SERVANT);
                }
            }
            */

            //**2 Human Adventurers**
            //**2 Human Civilians**
            for (int i = 0; i < 2; i++) {
                Character adventurer = CharacterManager.Instance.CreateNewCharacter(CharacterRole.ADVENTURER, RACE.HUMANS, Utilities.GetRandomGender(),
                    this, _ownedAreas[0]);
                adventurer.LevelUp(citizensLevel - 1);

                Character civilian = CharacterManager.Instance.CreateNewCharacter(CharacterRole.CIVILIAN, RACE.HUMANS, Utilities.GetRandomGender(),
                    this, _ownedAreas[0]);
                civilian.LevelUp(citizensLevel - 1);
            }
        } else if (_name == "Orelia") {
            //Female Elf Queen  with **4 Elven Soldiers** each as her servants
            for (int i = 0; i < 4; i++) {
                Character createdCharacter = CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, RACE.ELVES, Utilities.GetRandomGender(),
                    this, _ownedAreas[0]);
                createdCharacter.LevelUp(citizensLevel - 1);
                CharacterManager.Instance.CreateNewRelationshipBetween(leader, createdCharacter, RELATIONSHIP_TRAIT.SERVANT);
            }

            //**3 Elven Nobles** with **2 Elven Soldiers** each as their servants
            for (int i = 0; i < 3; i++) {
                Character noble = CharacterManager.Instance.CreateNewCharacter(CharacterRole.NOBLE, RACE.ELVES, Utilities.GetRandomGender(),
                    this, _ownedAreas[0]);
                noble.LevelUp(citizensLevel - 1);

                for (int j = 0; j < 2; j++) {
                    Character createdCharacter = CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, RACE.ELVES, Utilities.GetRandomGender(),
                   this, _ownedAreas[0]);
                    createdCharacter.LevelUp(citizensLevel - 1);
                    CharacterManager.Instance.CreateNewRelationshipBetween(noble, createdCharacter, RELATIONSHIP_TRAIT.SERVANT);
                }
            }

            //**2 Elven Adventurers**
            //**8 Elven Civilians**
            for (int i = 0; i < 8; i++) {
                if (i < 2) {
                    Character elvenAdventurer = CharacterManager.Instance.CreateNewCharacter(CharacterRole.ADVENTURER, RACE.ELVES, Utilities.GetRandomGender(),
                    this, _ownedAreas[0]);
                    elvenAdventurer.LevelUp(citizensLevel - 1);
                }

                Character elvenCivilian = CharacterManager.Instance.CreateNewCharacter(CharacterRole.CIVILIAN, RACE.ELVES, Utilities.GetRandomGender(),
                    this, _ownedAreas[0]);
                elvenCivilian.LevelUp(citizensLevel - 1);

            }
        } else if (_name == "Ziranna") {
            //Male Human Necromancer with **2 Skeleton Soldiers** and **2 Goblin Soldiers** as his Servants
            for (int i = 0; i < 2; i++) {
                Character skeletonSoldier = CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, RACE.SKELETON, Utilities.GetRandomGender(),
                    this, _ownedAreas[0]);
                skeletonSoldier.LevelUp(citizensLevel - 1);
                CharacterManager.Instance.CreateNewRelationshipBetween(leader, skeletonSoldier, RELATIONSHIP_TRAIT.SERVANT);

                Character goblinSoldier = CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, RACE.GOBLIN, Utilities.GetRandomGender(),
                    this, _ownedAreas[0]);
                goblinSoldier.LevelUp(citizensLevel - 1);
                CharacterManager.Instance.CreateNewRelationshipBetween(leader, goblinSoldier, RELATIONSHIP_TRAIT.SERVANT);
            }

            //**1 Skeleton Adventurer** and **1 Goblin Adventurer**
            //**2 Goblin Civilians**
            for (int i = 0; i < 2; i++) {
                if (i < 1) {
                    Character skeletonSoldier = CharacterManager.Instance.CreateNewCharacter(CharacterRole.ADVENTURER, RACE.SKELETON, Utilities.GetRandomGender(),
                            this, _ownedAreas[0]);
                    skeletonSoldier.LevelUp(citizensLevel - 1);

                    Character goblinSoldier = CharacterManager.Instance.CreateNewCharacter(CharacterRole.ADVENTURER, RACE.GOBLIN, Utilities.GetRandomGender(),
                            this, _ownedAreas[0]);
                    goblinSoldier.LevelUp(citizensLevel - 1);
                }
                Character goblinCivilian = CharacterManager.Instance.CreateNewCharacter(CharacterRole.CIVILIAN, RACE.GOBLIN, Utilities.GetRandomGender(),
                           this, _ownedAreas[0]);
                goblinCivilian.LevelUp(citizensLevel - 1);
            }
        } else if (_name == "Rikitik") {
            //Male Goblin Bandit Boss with **4 Goblin Soldiers** as Servants
            for (int i = 0; i < 4; i++) {
                Character goblinSoldier = CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, RACE.GOBLIN, Utilities.GetRandomGender(),
                    this, _ownedAreas[0]);
                goblinSoldier.LevelUp(citizensLevel - 1);
                CharacterManager.Instance.CreateNewRelationshipBetween(leader, goblinSoldier, RELATIONSHIP_TRAIT.SERVANT);
            }

            //**1 Goblin Noble** with **3 Goblin Soldiers** as servants
            for (int i = 0; i < 1; i++) {
                Character noble = CharacterManager.Instance.CreateNewCharacter(CharacterRole.NOBLE, RACE.GOBLIN, Utilities.GetRandomGender(),
                    this, _ownedAreas[0]);
                noble.LevelUp(citizensLevel - 1);

                for (int j = 0; j < 3; j++) {
                    Character createdCharacter = CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, RACE.GOBLIN, Utilities.GetRandomGender(),
                   this, _ownedAreas[0]);
                    createdCharacter.LevelUp(citizensLevel - 1);
                    CharacterManager.Instance.CreateNewRelationshipBetween(noble, createdCharacter, RELATIONSHIP_TRAIT.SERVANT);
                }
            }

            //**3 Goblin Civilians**
            for (int i = 0; i < 3; i++) {
                Character goblinCivilian = CharacterManager.Instance.CreateNewCharacter(CharacterRole.CIVILIAN, RACE.GOBLIN, Utilities.GetRandomGender(),
                           this, _ownedAreas[0]);
                goblinCivilian.LevelUp(citizensLevel - 1);
            }
        } else if (_name == "Caeven") {
            //Male Faery Tempest with **3 Faery Soldiers** as servants
            for (int i = 0; i < 3; i++) {
                Character faerySoldier = CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, RACE.FAERY, Utilities.GetRandomGender(),
                    this, _ownedAreas[0]);
                faerySoldier.LevelUp(citizensLevel - 1);
                CharacterManager.Instance.CreateNewRelationshipBetween(leader, faerySoldier, RELATIONSHIP_TRAIT.SERVANT);
            }

            //**2 Faery Adventurers**
            //**3 Faery Civilians**
            for (int i = 0; i < 3; i++) {
                if (i < 2) {
                    Character faeryAdventurer = CharacterManager.Instance.CreateNewCharacter(CharacterRole.ADVENTURER, RACE.FAERY, Utilities.GetRandomGender(),
                           this, _ownedAreas[0]);
                    faeryAdventurer.LevelUp(citizensLevel - 1);
                }

                Character faeryCivilian = CharacterManager.Instance.CreateNewCharacter(CharacterRole.CIVILIAN, RACE.FAERY, Utilities.GetRandomGender(),
                           this, _ownedAreas[0]);
                faeryCivilian.LevelUp(citizensLevel - 1);
            }
        } else if (_name == "Lucareth") {
            //Female Faery Witch with **3 Spider Soldiers**
            for (int i = 0; i < 3; i++) {
                Character spiderSoldier = CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, RACE.SPIDER, Utilities.GetRandomGender(),
                    this, _ownedAreas[0]);
                spiderSoldier.LevelUp(citizensLevel - 1);
                CharacterManager.Instance.CreateNewRelationshipBetween(leader, spiderSoldier, RELATIONSHIP_TRAIT.SERVANT);
            }

            //**3 Faery Nobles** with **3 Spider Soldiers** each as their servants
            for (int i = 0; i < 3; i++) {
                Character noble = CharacterManager.Instance.CreateNewCharacter(CharacterRole.NOBLE, RACE.FAERY, Utilities.GetRandomGender(),
                    this, _ownedAreas[0]);
                noble.LevelUp(citizensLevel - 1);

                for (int j = 0; j < 3; j++) {
                    Character createdCharacter = CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, RACE.SPIDER, Utilities.GetRandomGender(),
                   this, _ownedAreas[0]);
                    createdCharacter.LevelUp(citizensLevel - 1);
                    CharacterManager.Instance.CreateNewRelationshipBetween(noble, createdCharacter, RELATIONSHIP_TRAIT.SERVANT);
                }
            }

            //**4 Faery Adventurers**
            //**4 Faery Civilians**
            for (int i = 0; i < 4; i++) {
                Character faeryAdventurer = CharacterManager.Instance.CreateNewCharacter(CharacterRole.ADVENTURER, RACE.FAERY, Utilities.GetRandomGender(),
                           this, _ownedAreas[0]);
                faeryAdventurer.LevelUp(citizensLevel - 1);

                Character faeryCivilian = CharacterManager.Instance.CreateNewCharacter(CharacterRole.CIVILIAN, RACE.FAERY, Utilities.GetRandomGender(),
                           this, _ownedAreas[0]);
                faeryCivilian.LevelUp(citizensLevel - 1);
            }
        } else if (_name == "Magus") {
            //Male Elf Archmage with **1 Elf Soldier** and **1 Human Soldier** as Servants
            for (int i = 0; i < 1; i++) {
                Character elfSoldier = CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, RACE.ELVES, Utilities.GetRandomGender(),
                    this, _ownedAreas[0]);
                elfSoldier.LevelUp(citizensLevel - 1);
                CharacterManager.Instance.CreateNewRelationshipBetween(leader, elfSoldier, RELATIONSHIP_TRAIT.SERVANT);

                Character humanSoldier = CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, RACE.HUMANS, Utilities.GetRandomGender(),
                   this, _ownedAreas[0]);
                humanSoldier.LevelUp(citizensLevel - 1);
                CharacterManager.Instance.CreateNewRelationshipBetween(leader, humanSoldier, RELATIONSHIP_TRAIT.SERVANT);
            }

            //**2 Human Adventurers** and **2 Elven Adventurers**
            //**2 Human Civilians** and **2 Elven Civilians**
            for (int i = 0; i < 2; i++) {
                Character humanAdventurer = CharacterManager.Instance.CreateNewCharacter(CharacterRole.ADVENTURER, RACE.HUMANS, Utilities.GetRandomGender(),
                           this, _ownedAreas[0]);
                humanAdventurer.LevelUp(citizensLevel - 1);

                Character elfAdventurer = CharacterManager.Instance.CreateNewCharacter(CharacterRole.ADVENTURER, RACE.ELVES, Utilities.GetRandomGender(),
                           this, _ownedAreas[0]);
                elfAdventurer.LevelUp(citizensLevel - 1);

                Character humanCivilian = CharacterManager.Instance.CreateNewCharacter(CharacterRole.CIVILIAN, RACE.HUMANS, Utilities.GetRandomGender(),
                           this, _ownedAreas[0]);
                humanCivilian.LevelUp(citizensLevel - 1);

                Character elfCivilian = CharacterManager.Instance.CreateNewCharacter(CharacterRole.CIVILIAN, RACE.ELVES, Utilities.GetRandomGender(),
                           this, _ownedAreas[0]);
                elfCivilian.LevelUp(citizensLevel - 1);
            }
        }
    }
    #endregion

    #region Relationships
    public void AddNewRelationship(Faction relWith, FactionRelationship relationship) {
        if (!_relationships.ContainsKey(relWith)) {
            _relationships.Add(relWith, relationship);
        } else {
            throw new System.Exception(this.name + " already has a relationship with " + relWith.name + ", but something is trying to create a new one!");
        }
    }
    public void RemoveRelationshipWith(Faction relWith) {
        if (_relationships.ContainsKey(relWith)) {
            _relationships.Remove(relWith);
        }
    }
    public FactionRelationship GetRelationshipWith(Faction faction) {
        if (_relationships.ContainsKey(faction)) {
            return _relationships[faction];
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
    public bool IsAtWar() {
        foreach (KeyValuePair<Faction, FactionRelationship> kvp in relationships) {
            if (kvp.Key.isActive && kvp.Value.relationshipStatus == FACTION_RELATIONSHIP_STATUS.AT_WAR) {
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
        for (int i = 0; i < _ownedLandmarks.Count; i++) {
            BaseLandmark currLandmark = _ownedLandmarks[i];
            if (currLandmark.specificLandmarkType == landmarkType) {
                return currLandmark;
            }
        }
        return null;
    }
    public void AddLandmarkInfo(BaseLandmark landmark) {
        if (!_landmarkInfo.Contains(landmark)) {
            _landmarkInfo.Add(landmark);
        }
    }
    public void RemoveLandmarkInfo(BaseLandmark landmark) {
        _landmarkInfo.Remove(landmark);
    }
    #endregion

    #region Areas
    public void AddToOwnedAreas(Area area) {
        if (!_ownedAreas.Contains(area)) {
            _ownedAreas.Add(area);
            Messenger.Broadcast(Signals.FACTION_OWNED_AREA_ADDED, this, area);
        }
    }
    public void RemoveFromOwnedAreas(Area area) {
        if (_ownedAreas.Remove(area)) {
            Messenger.Broadcast(Signals.FACTION_OWNED_AREA_REMOVED, this, area);
        }
    }
    #endregion

    #region Emblems
    public void SetEmblem(FactionEmblemSetting sprite) {
        _emblem = sprite;
    }
    #endregion

    #region Morality
    public void SetMorality(MORALITY morality) {
        this.morality = morality;
    }
    #endregion

    #region Interaction
    private void ConstructFactionTasksWeights() {
        _factionTasksWeights = new Dictionary<INTERACTION_CATEGORY, FactionTaskWeight>() {
            { INTERACTION_CATEGORY.RECRUITMENT, new FactionTaskWeight() },
            { INTERACTION_CATEGORY.SUPPLY, new FactionTaskWeight() },
            { INTERACTION_CATEGORY.DIPLOMACY, new FactionTaskWeight() },
            { INTERACTION_CATEGORY.INVENTORY, new FactionTaskWeight() },
            { INTERACTION_CATEGORY.SUBTERFUGE, new FactionTaskWeight() { baseWeight = 0, areaWeight = 0, supplyCost = 100 } },
            { INTERACTION_CATEGORY.OFFENSE, new FactionTaskWeight() { baseWeight = 0, areaWeight = 0, supplyCost = 100 } },
            { INTERACTION_CATEGORY.DEFENSE, new FactionTaskWeight() },
            { INTERACTION_CATEGORY.EXPANSION, new FactionTaskWeight() { baseWeight = 0, areaWeight = 0, supplyCost = 100 } },
        };
    }

    //This updates faction weights before doing the update per area, meaning this is constant to all areas. This is done to prevent from recomputing everytime in the area loop
    private void UpdateBaseFactionTasksWeights() {
        List<INTERACTION_CATEGORY> categories = _factionTasksWeights.Keys.ToList();
        for (int i = 0; i < categories.Count; i++) {
            INTERACTION_CATEGORY category = categories[i];
            FactionTaskWeight taskWeight = _factionTasksWeights[category];
            taskWeight.factionCannotDoTask = false;
            taskWeight.baseWeight = GetInitialFactionTaskWeight(category);
            _factionTasksWeights[category] = taskWeight;
        }
    }

    //This is the update of faction weights per area
    private void UpdateAreaFactionTasksWeights(Area area) {
        //int supplySpentForInteraction = 0;
        List<INTERACTION_CATEGORY> categories = _factionTasksWeights.Keys.ToList();
        for (int i = 0; i < categories.Count; i++) {
            INTERACTION_CATEGORY category = categories[i];
            FactionTaskWeight taskWeight = _factionTasksWeights[category];
            taskWeight.areaWeight = GetFactionTaskWeightPerArea(category, area);
            //supplySpentForInteraction += taskWeight.supplyCost;
            //if (supplySpentForInteraction <= area.suppliesInBank) {
            //    taskWeight.areaCannotDoTask = false;
            //    taskWeight.areaWeight = GetFactionTaskWeightPerArea(category, area);
            //} else {
            //    taskWeight.areaCannotDoTask = true;
            //}
            _factionTasksWeights[category] = taskWeight;
        }
    }

    private int GetInitialFactionTaskWeight(INTERACTION_CATEGORY category) {
        int weight = 0;
        if (category == INTERACTION_CATEGORY.RECRUITMENT) {
            if (morality == MORALITY.GOOD && size == FACTION_SIZE.MAJOR) {
                weight += 100;
                foreach (FactionRelationship relationship in relationships.Values) {
                    if (relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ENEMY) {
                        weight += 10;
                    } else if (relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.AT_WAR) {
                        weight += 20;
                    }
                }
            } else if (morality == MORALITY.EVIL && size == FACTION_SIZE.MAJOR) {
                weight += 120;
                foreach (FactionRelationship relationship in relationships.Values) {
                    if (relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ENEMY) {
                        weight += 15;
                    } else if (relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.AT_WAR) {
                        weight += 30;
                    }
                }
            } else if (size == FACTION_SIZE.MINOR) {
                weight += 80;
                foreach (FactionRelationship relationship in relationships.Values) {
                    if (relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ENEMY) {
                        weight += 5;
                    } else if (relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.AT_WAR) {
                        weight += 10;
                    }
                }
            }
        } else if (category == INTERACTION_CATEGORY.SUPPLY) {
            if (morality == MORALITY.GOOD && size == FACTION_SIZE.MAJOR) {
                _supplyTaskNumOfReserved = 500;
            } else if (morality == MORALITY.EVIL && size == FACTION_SIZE.MAJOR) {
                _supplyTaskNumOfReserved = 400;
            } else if (size == FACTION_SIZE.MINOR) {
                _supplyTaskNumOfReserved = 300;
            }
        } else if (category == INTERACTION_CATEGORY.DIPLOMACY) {
            int allyStrength = _characters.Sum(x => x.level);
            int enemyStrength = 0;
            foreach (KeyValuePair<Faction, FactionRelationship> kvp in relationships) {
                if (kvp.Value.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ALLY) {
                    allyStrength += kvp.Key.characters.Sum(x => x.level);
                } else if (kvp.Value.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ENEMY || kvp.Value.relationshipStatus == FACTION_RELATIONSHIP_STATUS.AT_WAR) {
                    enemyStrength += kvp.Key.characters.Sum(x => x.level);
                }
            }
            if (allyStrength < enemyStrength) {
                int diff = enemyStrength - allyStrength;
                if (morality == MORALITY.GOOD && size == FACTION_SIZE.MAJOR) {
                    weight += (25 * diff);
                } else if (morality == MORALITY.EVIL && size == FACTION_SIZE.MAJOR) {
                    weight += (50 * diff);
                } else if (size == FACTION_SIZE.MINOR) {
                    weight += (75 * diff);
                }
            }
        } else if (category == INTERACTION_CATEGORY.INVENTORY) {
            //will not set base weight for inventory type interactions
        } else if (category == INTERACTION_CATEGORY.SUBTERFUGE) {
            foreach (FactionRelationship relationship in relationships.Values) {
                if (relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.DISLIKED || relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ENEMY
                    || relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.AT_WAR) {
                    if (morality == MORALITY.NEUTRAL) {
                        weight += 100;
                    } else if (morality == MORALITY.EVIL) {
                        weight += 250;
                    }
                    break;
                }
            }
        } else if (category == INTERACTION_CATEGORY.DEFENSE) {
            if (factionType == FACTION_TYPE.HOSTILE) {
                weight += 100;
            } else if (factionType == FACTION_TYPE.BALANCED) {
                weight += 200;
            } else if (factionType == FACTION_TYPE.DEFENSIVE) {
                weight += 400;
            }
        } else if (category == INTERACTION_CATEGORY.OFFENSE) {
            if (factionType == FACTION_TYPE.HOSTILE) {
                _offenseTaskNumOfReserved = 4;
            } else if (factionType == FACTION_TYPE.BALANCED) {
                _offenseTaskNumOfReserved = 6;
            } else if (factionType == FACTION_TYPE.DEFENSIVE) {
                _offenseTaskNumOfReserved = 8;
            }
        } else if (category == INTERACTION_CATEGORY.EXPANSION) {
            if(size != FACTION_SIZE.MINOR) {
                weight = 100;
            }
        }
        return weight;
    }
    private int GetFactionTaskWeightPerArea(INTERACTION_CATEGORY category, Area area) {
        int weight = 0;
        if (category == INTERACTION_CATEGORY.RECRUITMENT) {
            //Minus 1 because the computation is baseWeight + areaWeight, if there is no minus 1, the total weight will be 1 base weight higher than the correct weight
            weight = _factionTasksWeights[category].baseWeight * (area.GetNumberOfUnoccupiedStructure(STRUCTURE_TYPE.DWELLING) - 1);
        } else if (category == INTERACTION_CATEGORY.SUPPLY) {
            if(area.suppliesInBank < _supplyTaskNumOfReserved) {
                int missingSupplies = _supplyTaskNumOfReserved - area.suppliesInBank;
                weight = missingSupplies * 4;
            }
        } else if (category == INTERACTION_CATEGORY.DIPLOMACY) {
            //No diplomacy modifier per area
        } else if (category == INTERACTION_CATEGORY.INVENTORY) {
            if (!area.IsItemInventoryFull()) {
                weight = _inventoryTaskWeight;
            }
        } else if (category == INTERACTION_CATEGORY.SUBTERFUGE) {
            //See supply checker above
        } else if (category == INTERACTION_CATEGORY.DEFENSE) {
            //No defense modifier per area
        } else if (category == INTERACTION_CATEGORY.OFFENSE) {
            if (IsAtWar()) {
                int half = _offenseTaskNumOfReserved / 2;
                int frontlineCharacters = 0;
                int backlineCharacters = 0;
                for (int i = 0; i < area.areaResidents.Count; i++) {
                    Character resident = area.areaResidents[i];
                    if (resident.isIdle && !resident.isLeader
                        && !resident.characterClass.isNonCombatant
                        && !resident.isDefender && resident.specificLocation.id == id && resident.currentStructure.isInside) {
                        if ((area.owner != null && resident.faction == area.owner) || (area.owner == null && resident.faction == FactionManager.Instance.neutralFaction)) {
                            if (resident.characterClass.combatPosition == COMBAT_POSITION.FRONTLINE) {
                                frontlineCharacters++;
                            } else {
                                backlineCharacters++;
                            }
                        }
                    }
                }
                if (frontlineCharacters >= half && backlineCharacters >= half) {
                    weight = 200 + (100 * area.offenseTaskWeightMultiplier);
                }
            }
        } else if (category == INTERACTION_CATEGORY.EXPANSION) {
            if (morality == MORALITY.GOOD && size == FACTION_SIZE.MAJOR) {
                weight = (-25 * area.GetNumberOfUnoccupiedStructure(STRUCTURE_TYPE.DWELLING));
            } else if (morality == MORALITY.EVIL && size == FACTION_SIZE.MAJOR) {
                weight = (-20 * area.GetNumberOfUnoccupiedStructure(STRUCTURE_TYPE.DWELLING));
            }
        }
        return weight;
    }
    private void InitializeInteractions() {
        _nonNeutralInteractionTypes = new Dictionary<INTERACTION_TYPE, int> {
            { INTERACTION_TYPE.SPAWN_CHARACTER, 50 },
            { INTERACTION_TYPE.MOVE_TO_ATTACK, 50 },
            //INTERACTION_TYPE.DEFENSE_MOBILIZATION,
            //INTERACTION_TYPE.DEFENSE_UPGRADE,
        };
        _neutralInteractionTypes = new Dictionary<INTERACTION_TYPE, int> {
            { INTERACTION_TYPE.SPAWN_NEUTRAL_CHARACTER, 50 }
        };
    }
    private void SetDailyInteractionGenerationTick() {
        //_currentInteractionTick = UnityEngine.Random.Range(1, GameManager.hoursPerDay + 1);
        int remainingDaysInMonth = GameManager.Instance.continuousDays % GameManager.daysPerMonth;
        int startDay = GameManager.Instance.continuousDays + remainingDaysInMonth + 1;
        _currentInteractionTick = UnityEngine.Random.Range(startDay, startDay + GameManager.daysPerMonth);
    }
    public void SetDailyInteractionGenerationTick(int tick) {
        _currentInteractionTick = tick;
    }
    private void DailyInteractionGeneration() {
        if (_currentInteractionTick == GameManager.Instance.tick) {
            GenerateDailyInteraction();
            //SetDailyInteractionGenerationTick();
        }
    }
    private void GenerateDailyInteraction() {
        if(this == FactionManager.Instance.neutralFaction) {
            return;
        }
        GenerateFactionTasks();
    }
    //private void GenerateFactionInteraction() {
    //    for (int i = 0; i < _ownedAreas.Count; i++) {
    //        GenerateAreaInteraction(_ownedAreas[i]);
    //        _ownedAreas[i].AreaTasksAssignments();
    //    }
    //}
    private void GenerateFactionTasks() {
        UpdateBaseFactionTasksWeights();
        for (int i = 0; i < _ownedAreas.Count; i++) {
            GenerateAreaInteraction(_ownedAreas[i]);
        }
    }
    private void GenerateAreaInteraction(Area area) {
        string interactionLog = GameManager.Instance.TodayLogString() + "GENERATING FACTION TASKS FOR " + this.name + " in " + area.name;
        interactionLog += "\nAREA MONTHLY ACTIONS: " + area.monthlyActions.ToString() + ", FACTION MORALITY: " + morality.ToString() + " " + size.ToString() + ", FACTION TYPE: " + factionType.ToString();

        UpdateAreaFactionTasksWeights(area);
        WeightedDictionary<INTERACTION_CATEGORY> tasksWeights = new WeightedDictionary<INTERACTION_CATEGORY>();
        foreach (KeyValuePair<INTERACTION_CATEGORY, FactionTaskWeight> kvp in _factionTasksWeights) {
            if (!kvp.Value.factionCannotDoTask) {
                int totalWeight = kvp.Value.baseWeight + kvp.Value.areaWeight;
                tasksWeights.AddElement(kvp.Key, totalWeight);
                interactionLog += "\n" + kvp.Key.ToString() + ": " + totalWeight.ToString();
            } else {
                interactionLog += "\nCAN'T PUT " + kvp.Key.ToString() + " IN WEIGHTS BECAUSE IT MAY ONLY BE TRIGGERED ONCE PER FACTION!";
            }
        }
        int actionsCount = 0;
        int supplySpentOnInteraction = 0;
        while (actionsCount < area.monthlyActions && tasksWeights.GetTotalOfWeights() > 0) {
            INTERACTION_CATEGORY chosenCategory = tasksWeights.PickRandomElementGivenWeights();
            interactionLog += "\n--------------------------------------------------------";
            if(supplySpentOnInteraction + _factionTasksWeights[chosenCategory].supplyCost <= area.suppliesInBank) {
                interactionLog += "\nCHOSEN INTERACTION TYPE: " + chosenCategory.ToString();
            } else {
                interactionLog += "\nCAN'T DO " + chosenCategory.ToString() + " TYPE BECAUSE AREA CANNOT ACCOMODATE SUPPLY COST: " + _factionTasksWeights[chosenCategory].supplyCost
                    + ". Setting weight to 0 and randomizing again...";
                tasksWeights.RemoveElement(chosenCategory);
                continue;
            }
            //Get all residents of area that can do the interaction category;
            Dictionary<Character, List<INTERACTION_TYPE>> residentInteractions = area.GetResidentAndInteractionsForFactionTasks(chosenCategory);
            if (residentInteractions.Count > 0) {
                //For testing only
                interactionLog += "\nALL CHARACTERS AND INTERACTIONS THEY CAN DO";
                foreach (KeyValuePair<Character, List<INTERACTION_TYPE>> kvp in residentInteractions) {
                    interactionLog += "\n" + kvp.Key.name.ToString() + ": ";
                    for (int i = 0; i < kvp.Value.Count; i++) {
                        if (i > 0) {
                            interactionLog += ", ";
                        }
                        interactionLog += kvp.Value[i].ToString();
                    }
                }
                actionsCount++;
                Character chosenCharacter = residentInteractions.ElementAt(UnityEngine.Random.Range(0, residentInteractions.Count)).Key;
                INTERACTION_TYPE chosenInteraction = residentInteractions[chosenCharacter][UnityEngine.Random.Range(0, residentInteractions[chosenCharacter].Count)];
                interactionLog += "\nCHOSEN CHARACTER AND INTERACTION: " + chosenCharacter.name + " - " + chosenInteraction.ToString();
                interactionLog += "\n--------------------------------------------------------";
                Interaction interaction = InteractionManager.Instance.CreateNewInteraction(chosenInteraction, chosenCharacter.specificLocation);
                if (_factionTasksWeights[chosenCategory].supplyCost > 0) {
                    supplySpentOnInteraction += _factionTasksWeights[chosenCategory].supplyCost;
                    interaction.SetCanInteractionBeDoneAction(() => area.CanDoAreaTaskInteraction(interaction.type, chosenCharacter, _factionTasksWeights[chosenCategory].supplyCost));
                    interaction.SetInitializeAction(() => area.AdjustSuppliesInBank(-_factionTasksWeights[chosenCategory].supplyCost));
                    interaction.SetMinionSuccessAction(() => area.AdjustSuppliesInBank(_factionTasksWeights[chosenCategory].supplyCost));
                }
                chosenCharacter.SetForcedInteraction(interaction);

                if (chosenCategory == INTERACTION_CATEGORY.DIPLOMACY) {
                    //may only trigger once per Month tick per faction
                    FactionTaskWeight taskWeight = _factionTasksWeights[chosenCategory];
                    taskWeight.factionCannotDoTask = true;
                    _factionTasksWeights[chosenCategory] = taskWeight;
                } else if (chosenCategory == INTERACTION_CATEGORY.OFFENSE) {
                    //may only trigger once per Month tick per location
                    tasksWeights.RemoveElement(chosenCategory);
                }
            } else {
                interactionLog += "\nNO RESIDENT CAN DO " + chosenCategory.ToString() + " TYPE ACTION! Setting weight to zero and randomizing again... ";
                tasksWeights.RemoveElement(chosenCategory);
            }
        }
        if (actionsCount >= area.monthlyActions) {
            interactionLog += "\nMONTHLY ACTIONS REACHED! Faction tasks assignment ends.";
        }
        if (tasksWeights.Count < 0) {
            interactionLog += "\nNO MORE TASKS THAT CAN BE DONE! Faction tasks assignment ends.";
        }
        //Debug.Log(interactionLog);
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
                this.history.RemoveAt(0);
            }
            Messenger.Broadcast(Signals.HISTORY_ADDED, this as object);
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
