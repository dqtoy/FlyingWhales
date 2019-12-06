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
    //public string initialLeaderClass { get; protected set; }
    public int level { get; protected set; }
    public bool isPlayerFaction { get; protected set; }
    public GENDER initialLeaderGender { get; protected set; }
    public RACE initialLeaderRace { get; protected set; }
    public RACE race { get; protected set; }
    public ILeader leader { get; protected set; }
    public Sprite emblem { get; protected set; }
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
    public bool isActive { get; private set; }
    public List<Log> history { get; private set; }
    public Region mainRegion { get { return ownedRegions[0]; } }
    public Quest activeQuest { get; protected set; }
    public FactionIdeologyComponent ideologyComponent { get; protected set; }

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
        factionType = Utilities.GetRandomEnumValue<FACTION_TYPE>();
        characters = new List<Character>();
        ownedLandmarks = new List<BaseLandmark>();
        relationships = new Dictionary<Faction, FactionRelationship>();
        ownedRegions = new List<Region>();
        recruitableRaces = new List<RACE>();
        startingFollowers = new List<RACE>();
        history = new List<Log>();
        ideologyComponent = new FactionIdeologyComponent(this);
        AddListeners();
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
        //initialLeaderClass = data.initialLeaderClass;
        initialLeaderRace = data.initialLeaderRace;
        initialLeaderGender = data.initialLeaderGender;
        level = data.level;
        factionType = data.factionType;

        characters = new List<Character>();
        ownedLandmarks = new List<BaseLandmark>();
        relationships = new Dictionary<Faction, FactionRelationship>();
        ownedRegions = new List<Region>();
        recruitableRaces = new List<RACE>();
        startingFollowers = new List<RACE>();
        history = new List<Log>();
        ideologyComponent = new FactionIdeologyComponent(this);
        AddListeners();
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
            previousRuler.UnassignBuildStructureComponent();
        }
        if (leader != null && leader is Character) {
            Character newRuler = leader as Character;
            if(newRuler.role.roleType != CHARACTER_ROLE.LEADER) {
                newRuler.AssignRole(CharacterRole.LEADER);
                newRuler.AssignClassByRole(newRuler.role);
            }
            newRuler.AssignBuildStructureComponent();


            //if (newRuler.characterClass.className != initialLeaderClass) {
            //    newRuler.AssignClass(CharacterManager.Instance.CreateNewCharacterClass(initialLeaderClass));
            //}
        }
        OnlySetLeader(leader);

        //Every time the leader changes, faction ideology changes
        FACTION_IDEOLOGY newIdeology = Utilities.GetRandomEnumValue<FACTION_IDEOLOGY>();
        ideologyComponent.SwitchToIdeology(newIdeology); //Inclusive only right now
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
    public bool JoinFaction(Character character) {
        if(ideologyComponent.DoesCharacterFitCurrentIdeology(character)) {
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
            return true;
        } else {
            if (GameManager.Instance.gameHasStarted) {
                Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "cannot_join_faction");
                log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(this, name, LOG_IDENTIFIER.FACTION_1);
                character.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
            }
            return false;
        }
    }
    public bool LeaveFaction(Character character) {
        if (characters.Remove(character)) {
            if (leader == character) {
                SetNewLeader(); //so a new leader can be set if the leader is ever removed from the list of characters of this faction
            }
            character.SetFaction(null);
            Messenger.Broadcast(Signals.CHARACTER_REMOVED_FROM_FACTION, character, this);
            return true;
        }
        return false;
        //if (_leader != null && character.id == _leader.id) {
        //    SetLeader(null);
        //}
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
            if (currCharacter.gender == gender && !currCharacter.isDead && !currCharacter.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE) && !currCharacter.traitContainer.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
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
    public List<Character> GetViableCharacters(GENDER gender, params string[] classNames) {
        List<Character> chars = new List<Character>();
        for (int i = 0; i < characters.Count; i++) {
            Character currCharacter = characters[i];
            if (currCharacter.gender == gender && !currCharacter.isDead && !currCharacter.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE) && !currCharacter.traitContainer.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
                for (int j = 0; j < classNames.Length; j++) {
                    if (currCharacter.characterClass.className == classNames[j]) {
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
            List<Character> maleRelatives = FactionManager.Instance.GetViableRulers(previousRuler, GENDER.MALE, RELATIONSHIP_TRAIT.RELATIVE);
            if(maleRelatives.Count > 0) {
                newRuler = maleRelatives[UnityEngine.Random.Range(0, maleRelatives.Count)];
                log += "\nNew Ruler: " + newRuler.name;
            } else {
                log += "\nChecking male nobles...";
                List<Character> maleNobles = GetViableCharacters(GENDER.MALE, "Noble");
                if(maleNobles.Count > 0) {
                    newRuler = maleNobles[UnityEngine.Random.Range(0, maleNobles.Count)];
                    log += "\nNew Ruler: " + newRuler.name;
                } else {
                    log += "\nChecking female relatives of the king...";
                    List<Character> femaleRelatives = FactionManager.Instance.GetViableRulers(previousRuler, GENDER.FEMALE, RELATIONSHIP_TRAIT.RELATIVE);
                    if (femaleRelatives.Count > 0) {
                        newRuler = femaleRelatives[UnityEngine.Random.Range(0, femaleRelatives.Count)];
                        log += "\nNew Ruler: " + newRuler.name;
                    } else {
                        log += "\nChecking female nobles...";
                        List<Character> femaleNobles = GetViableCharacters(GENDER.FEMALE, "Noble");
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
    private void OnCharacterClassChange(Character character, CharacterClass previousClass, CharacterClass currentClass) {
        CheckIfCharacterStillFitsIdeology(character);
    }
    private void OnCharacterRaceChange(Character character) {
        CheckIfCharacterStillFitsIdeology(character);
    }
    private void OnCharacterRemoved(Character character) {
        LeaveFaction(character);
    }
    public void CheckIfCharacterStillFitsIdeology(Character character) {
        if (character.faction == this && !ideologyComponent.DoesCharacterFitCurrentIdeology(character)) {
            character.ChangeFactionTo(FactionManager.Instance.friendlyNeutralFaction);

            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "left_faction_not_fit");
            log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(this, name, LOG_IDENTIFIER.FACTION_1);
            character.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
        }
    }
    #endregion

    #region Utilities
    private void AddListeners() {
        Messenger.AddListener<Character>(Signals.CHARACTER_REMOVED, OnCharacterRemoved);
        Messenger.AddListener<Character>(Signals.CHARACTER_CHANGED_RACE, OnCharacterRaceChange);
        Messenger.AddListener<Character, CharacterClass, CharacterClass>(Signals.CHARACTER_CLASS_CHANGE, OnCharacterClassChange);
        //Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        //if (!_isPlayerFaction) {
        //    Messenger.AddListener(Signals.TICK_STARTED, DailyInteractionGeneration);
        //}
    }
    private void RemoveListeners() {
        Messenger.RemoveListener<Character>(Signals.CHARACTER_REMOVED, OnCharacterRemoved);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_CHANGED_RACE, OnCharacterRaceChange);
        Messenger.RemoveListener<Character, CharacterClass, CharacterClass>(Signals.CHARACTER_CLASS_CHANGE, OnCharacterClassChange);
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
    //public void SetInitialFactionLeaderClass(string className) {
    //    initialLeaderClass = className;
    //}
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
    public void GenerateStartingCitizens(int leaderLevel, int citizensLevel, int citizenCount, LocationClassManager classManager) {
        for (int i = 0; i < citizenCount; i++) {
            string className = classManager.GetCurrentClassToCreate();
            Character citizen = CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, className, race, Utilities.GetRandomGender(), this, mainRegion);
            if(className == "Leader") {
                citizen.LevelUp(leaderLevel - 1);
                SetLeader(citizen);
            } else {
                citizen.LevelUp(citizensLevel - 1);
            }
        }
        mainRegion.area.SetInitialResidentCount(citizenCount);
        RelationshipManager.Instance.GenerateRelationships(this.characters);
    }
    public int GetNumberOfDwellingsToHouseCharacters(List<Character> characters) {
        //To get number of dwellings needed,
        //loop through all the characters, then check if each one is single
        //if a character is single, assign it 1 dwelling, then remove that character from the list
        //if the character is not single then remove it and its lover from the list of characters, and assign them 1 dwelling.
        //continue the loop until the list of characters becomes empty

        List<Character> listOfCharacters = new List<Character>(characters);
        int neededDwellingCount = 0;

        while (listOfCharacters.Count != 0) {
            Character currCharacter = listOfCharacters[0];
            Character lover = (currCharacter.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.LOVER) as AlterEgoData)?.owner ?? null;
            if (lover != null) {
                listOfCharacters.Remove(lover);
            }
            listOfCharacters.Remove(currCharacter);
            neededDwellingCount++;
        }
        return neededDwellingCount;
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
    public bool HasOwnedRegionWithSettlement() {
        for (int i = 0; i < ownedRegions.Count; i++) {
            if (ownedRegions[i].area != null) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Emblems
    public void SetEmblem(Sprite sprite) {
        emblem = sprite;
    }
    #endregion

    #region Morality
    public void SetMorality(MORALITY morality) {
        this.morality = morality;
    }
    #endregion

    #region Logs
    public void AddHistory(Log log) {
        if (!history.Contains(log)) {
            history.Add(log);
            if (this.history.Count > 60) {
                //if (this.history[0].node != null) {
                //    this.history[0].node.AdjustReferenceCount(-1);
                //}
                this.history.RemoveAt(0);
            }
            //if (log.node != null) {
            //    log.node.AdjustReferenceCount(1);
            //}
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
