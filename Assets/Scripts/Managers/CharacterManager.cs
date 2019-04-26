using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class CharacterManager : MonoBehaviour {

    public static CharacterManager Instance = null;

    public GameObject characterIconPrefab;
    public Transform characterIconsParent;

    public int maxLevel;
    private Dictionary<ELEMENT, float> _elementsChanceDictionary;
    private List<Character> _allCharacters;
    private List<CharacterAvatar> _allCharacterAvatars;

	public Sprite heroSprite;
	public Sprite villainSprite;
	public Sprite hermitSprite;
	public Sprite beastSprite;
	public Sprite banditSprite;
	public Sprite chieftainSprite;

    [Header("Squad Emblems")]
    [SerializeField] private List<EmblemBG> _emblemBGs;
    [SerializeField] private List<Sprite> _emblemSymbols;

    [Header("Character Portrait Assets")]
    public GameObject characterPortraitPrefab;
    [SerializeField] private List<RacePortraitAssets> portraitAssets;
    public List<Color> hairColors;
    [SerializeField] private RolePortraitFramesDictionary portraitFrames;
    [SerializeField] private StringSpriteDictionary classPortraits;
    public Material hsvMaterial;

    [Header("Character Marker Assets")]
    [SerializeField] private List<RaceMarkerAssets> markerAssets;

    [Header("Character Role Animators")]
    [SerializeField] private RuntimeAnimatorController[] characterAnimators;

    [Header("Job Icons")]
    [SerializeField] private JobIconsDictionary jobIcons;
    

    public Dictionary<Character, List<string>> allCharacterLogs { get; private set; }
    public Dictionary<INTERACTION_TYPE, int> awayFromHomeInteractionWeights { get; private set; }
    public Dictionary<INTERACTION_TYPE, int> atHomeInteractionWeights { get; private set; }
    public Dictionary<CHARACTER_ROLE, INTERACTION_TYPE[]> characterRoleInteractions { get; private set; }
    public Dictionary<string, CharacterClass> classesDictionary { get; private set; }
    public Dictionary<string, CharacterClass> uniqueClasses { get; private set; }
    public Dictionary<string, CharacterClass> normalClasses { get; private set; }
    public Dictionary<string, CharacterClass> beastClasses { get; private set; }
    public Dictionary<string, CharacterClass> demonClasses { get; private set; }
    public Dictionary<string, Dictionary<string, CharacterClass>> identifierClasses { get; private set; }

    private static readonly string[] _sevenDeadlySinsClassNames = { "Lust", "Gluttony", "Greed", "Sloth", "Wrath", "Envy", "Pride" };
    private List<string> deadlySinsRotation = new List<string>();

    #region getters/setters
    public List<Character> allCharacters {
        get { return _allCharacters; }
    }
    public Dictionary<ELEMENT, float> elementsChanceDictionary {
        get { return _elementsChanceDictionary; }
    }
    public List<EmblemBG> emblemBGs {
        get { return _emblemBGs; }
    }
    public List<Sprite> emblemSymbols {
        get { return _emblemSymbols; }
    }
    #endregion

    private void Awake() {
        Instance = this;
        _allCharacters = new List<Character>();
        _allCharacterAvatars = new List<CharacterAvatar>();
        allCharacterLogs = new Dictionary<Character, List<string>>();
    }

    public void Initialize() {
        ConstructAllClasses();
        ConstructElementChanceDictionary();
        ConstructAwayFromHomeInteractionWeights();
        ConstructAtHomeInteractionWeights();
        //ConstructRoleInteractions();
        //ConstructPortraitDictionaries();
    }

    #region Characters
    public void LoadCharacters(WorldSaveData data) {
        if (data.charactersData != null) {
            for (int i = 0; i < data.charactersData.Count; i++) {
                CharacterSaveData currData = data.charactersData[i];
                Character currCharacter = CreateNewCharacter(currData);
                Faction characterFaction = FactionManager.Instance.GetFactionBasedOnID(currData.factionID);
                if (characterFaction != null) {
                    //currCharacter.SetFaction(characterFaction);
                    characterFaction.AddNewCharacter(currCharacter);
                    FactionSaveData factionData = data.GetFactionData(characterFaction.id);
                    if (factionData.leaderID != -1 && factionData.leaderID == currCharacter.id) {
                        characterFaction.SetLeader(currCharacter);
                    }
                }
#if !WORLD_CREATION_TOOL
                else {
                    characterFaction = FactionManager.Instance.neutralFaction;
                    //currCharacter.SetFaction(characterFaction);
                    characterFaction.AddNewCharacter(currCharacter);
                }
#endif
            }
#if WORLD_CREATION_TOOL
            worldcreator.WorldCreatorUI.Instance.editFactionsMenu.UpdateItems();
#endif
        }
    }
    //public void LoadCharactersInfo() {
    //    for (int i = 0; i < allCharacters.Count; i++) {
    //        Character currCharacter = allCharacters[i];
    //        //CheckForHiddenDesire(currCharacter);
    //        //CheckForIntelActions(currCharacter);
    //        //CheckForIntelReactions(currCharacter);
    //        //CheckForSecrets(currCharacter);
    //    }
    //}
    //public void LoadCharactersInfo(WorldSaveData data) {
    //    for (int i = 0; i < allCharacters.Count; i++) {
    //        Character currCharacter = allCharacters[i];
    //        CharacterSaveData saveData = data.GetCharacterSaveData(currCharacter.id);
    //        //if (saveData != null) {
    //        //    SetHiddenDesireForCharacter(saveData.hiddenDesire, currCharacter); //hidden desire
    //        //    if (saveData.secrets != null) { //secrets
    //        //        for (int j = 0; j < saveData.secrets.Count; j++) {
    //        //            int secretID = saveData.secrets[j];
    //        //            currCharacter.AddSecret(secretID);
    //        //        }
    //        //    }
    //        //}
    //    }
    //}
    //public void LoadRelationships(WorldSaveData data) {
    //    if (data.charactersData != null) {
    //        for (int i = 0; i < data.charactersData.Count; i++) {
    //            CharacterSaveData currData = data.charactersData[i];
    //            Character currCharacter = CharacterManager.Instance.GetCharacterByID(currData.id);
    //            currCharacter.LoadRelationships(currData.relationshipsData);
    //        }
    //    }
    //}
    //public void LoadSquads(WorldSaveData data) {
    //    if (data.squadData != null) {
    //        for (int i = 0; i < data.squadData.Count; i++) {
    //            SquadSaveData currData = data.squadData[i];
    //            CreateNewSquad(currData);
    //        }
    //    }
    //}
    /*
     Create a new character, given a role, class and race.
         */
    public Character CreateNewCharacter(CharacterRole role, RACE race, GENDER gender, Faction faction = null, 
        Area homeLocation = null, Dwelling homeStructure = null) {
        Character newCharacter = null;
        if (role == CharacterRole.LEADER) {
            //If the role is leader, it must have a faction, so get the data for the class from the faction
            newCharacter = new Character(role, faction.initialLeaderClass, race, gender);
        } else {
            newCharacter = new Character(role, race, gender);
        }
        Party party = newCharacter.CreateOwnParty();
        if (faction != null) {
            faction.AddNewCharacter(newCharacter);
        } else {
            FactionManager.Instance.neutralFaction.AddNewCharacter(newCharacter);
        }
#if !WORLD_CREATION_TOOL
        party.CreateIcon();
        if(homeLocation != null) {
            party.icon.SetPosition(homeLocation.coreTile.transform.position);
            newCharacter.MigrateHomeTo(homeLocation, homeStructure, false);
            //if (homeStructure != null) {
            //    newCharacter.MigrateHomeStructureTo(homeStructure);
            //}
            homeLocation.AddCharacterToLocation(party.owner, null, true);
        }
        //newCharacter.AddAwareness(newCharacter);
#endif
        newCharacter.CreateInitialTraitsByClass();
        //newCharacter.CreateInitialTraitsByRace();
        _allCharacters.Add(newCharacter);
        Messenger.Broadcast(Signals.CHARACTER_CREATED, newCharacter);
        return newCharacter;
    }
    public Character CreateNewCharacter(CharacterRole role, string className, RACE race, GENDER gender, Faction faction = null, 
        Area homeLocation = null, Dwelling homeStructure = null) {
        Character newCharacter = new Character(role, className, race, gender);
        Party party = newCharacter.CreateOwnParty();
        if (faction != null) {
            faction.AddNewCharacter(newCharacter);
        } else {
            FactionManager.Instance.neutralFaction.AddNewCharacter(newCharacter);
        }
#if !WORLD_CREATION_TOOL
        party.CreateIcon();
        if (homeLocation != null) {
            party.icon.SetPosition(homeLocation.coreTile.transform.position);
            newCharacter.MigrateHomeTo(homeLocation, homeStructure, false);
            homeLocation.AddCharacterToLocation(party.owner, null, true);
        }
        //newCharacter.AddAwareness(newCharacter);
#endif
        newCharacter.CreateInitialTraitsByClass();
        //newCharacter.CreateInitialTraitsByRace();
        _allCharacters.Add(newCharacter);
        Messenger.Broadcast(Signals.CHARACTER_CREATED, newCharacter);
        return newCharacter;
    }
    public Character CreateNewCharacter(CharacterSaveData data) {
        Character newCharacter = new Character(data);
        allCharacterLogs.Add(newCharacter, new List<string>());

        if (data.homeAreaID != -1) {
            Area homeArea = LandmarkManager.Instance.GetAreaByID(data.homeAreaID);
            if (homeArea != null) {
                homeArea.AddResident(newCharacter, null, true);
            }
        }
        Party party = newCharacter.CreateOwnParty();
        if (data.locationID != -1) {
            Area currentLocation = LandmarkManager.Instance.GetAreaByID(data.locationID);
            if (currentLocation != null) {
#if !WORLD_CREATION_TOOL
                party.CreateIcon();
                party.icon.SetPosition(currentLocation.coreTile.transform.position);
#endif
                currentLocation.AddCharacterToLocation(party.owner, null, true);
            }
        }
        //newCharacter.AddAwareness(newCharacter);

        if (data.level != 0) {
            newCharacter.SetLevel(data.level);
        }

        _allCharacters.Add(newCharacter);
        Messenger.Broadcast(Signals.CHARACTER_CREATED, newCharacter);
        return newCharacter;
    }
    public void RemoveCharacter(Character character) {
        _allCharacters.Remove(character);
        Messenger.Broadcast<Character>(Signals.CHARACTER_REMOVED, character);
    }
    private void ConstructAllClasses() {
        classesDictionary = new Dictionary<string, CharacterClass>();
        normalClasses = new Dictionary<string, CharacterClass>();
        uniqueClasses = new Dictionary<string, CharacterClass>();
        beastClasses = new Dictionary<string, CharacterClass>();
        demonClasses = new Dictionary<string, CharacterClass>();
        identifierClasses = new Dictionary<string, Dictionary<string, CharacterClass>>();
        string path = Utilities.dataPath + "CharacterClasses/";
        string[] classes = System.IO.Directory.GetFiles(path, "*.json");
        for (int i = 0; i < classes.Length; i++) {
            CharacterClass currentClass = JsonUtility.FromJson<CharacterClass>(System.IO.File.ReadAllText(classes[i]));
            currentClass.ConstructData();
            classesDictionary.Add(currentClass.className, currentClass);
            if(currentClass.identifier == "Normal") {
                normalClasses.Add(currentClass.className, currentClass);
                if (!identifierClasses.ContainsKey(currentClass.identifier)) {
                    identifierClasses.Add(currentClass.identifier, normalClasses);
                }
            }else if (currentClass.identifier == "Unique") {
                uniqueClasses.Add(currentClass.className, currentClass);
                if (!identifierClasses.ContainsKey(currentClass.identifier)) {
                    identifierClasses.Add(currentClass.identifier, uniqueClasses);
                }
            } else if (currentClass.identifier == "Beast") {
                beastClasses.Add(currentClass.className, currentClass);
                if (!identifierClasses.ContainsKey(currentClass.identifier)) {
                    identifierClasses.Add(currentClass.identifier, beastClasses);
                }
            } else if (currentClass.identifier == "Demon") {
                demonClasses.Add(currentClass.className, currentClass);
                if (!identifierClasses.ContainsKey(currentClass.identifier)) {
                    identifierClasses.Add(currentClass.identifier, demonClasses);
                }
            }
        }
        identifierClasses.Add("All", classesDictionary);
    }
    public string GetDeadlySinsClassNameFromRotation() {
        if (deadlySinsRotation.Count == 0) {
            deadlySinsRotation.AddRange(_sevenDeadlySinsClassNames);
        }
        string nextClass = deadlySinsRotation[0];
        deadlySinsRotation.RemoveAt(0);
        return nextClass;
    }
    public string GetRandomClassByIdentifier(string identifier) {
        if(identifier == "Demon") {
            return GetDeadlySinsClassNameFromRotation();
        } else if (identifierClasses.ContainsKey(identifier)) {
            return GetRandomClassName(identifierClasses[identifier]);
        }
        return string.Empty;
        //throw new System.Exception("There are no classes with the identifier " + identifier);
    }
    public bool IsClassADeadlySin(string className) {
        for (int i = 0; i < _sevenDeadlySinsClassNames.Length; i++) {
            if(className == _sevenDeadlySinsClassNames[i]) {
                return true;
            }
        }
        return false;
    }
    public string GetRandomClassName(Dictionary<string, CharacterClass> dict) {
        int random = UnityEngine.Random.Range(0, dict.Count);
        int count = 0;
        foreach (string className in dict.Keys) {
            if (count == random) {
                return className;
            }
            count++;
        }
        return string.Empty;
    }
    public void AddCharacterAvatar(CharacterAvatar characterAvatar) {
        int centerOrderLayer = (_allCharacterAvatars.Count * 2) + 1;
        int frameOrderLayer = centerOrderLayer + 1;
        characterAvatar.SetFrameOrderLayer(frameOrderLayer);
        characterAvatar.SetCenterOrderLayer(centerOrderLayer);
        _allCharacterAvatars.Add(characterAvatar);
    }
    public void RemoveCharacterAvatar(CharacterAvatar characterAvatar) {
        _allCharacterAvatars.Remove(characterAvatar);
    }
    public Character GetCharacterByClass(string className) {
        for (int i = 0; i < _allCharacters.Count; i++) {
            if(_allCharacters[i].characterClass.className == className) {
                return _allCharacters[i];
            }
        }
        return null;
    }
    public void CreateNeutralCharacters() {
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area currArea = LandmarkManager.Instance.allAreas[i];
            if (currArea.owner == null && currArea.areaType != AREA_TYPE.DEMONIC_INTRUSION) { //if unowned (neutral)
                currArea.GenerateNeutralCharacters();
            }
        }
    }
    public void GenerateInitialAwareness() {
        for (int i = 0; i < allCharacters.Count; i++) {
            Character character = allCharacters[i];
            character.AddInitialAwareness();
        }
    }
    public void PlaceInitialCharacters() {
        for (int i = 0; i < allCharacters.Count; i++) {
            Character character = allCharacters[i];
            if (!character.isFactionless) {
                character.CreateMarker();
                if (character.homeStructure != null) {
                    //place the character at a random unoccupied tile in his/her home
                    List<LocationGridTile> choices = character.homeStructure.unoccupiedTiles;
                    LocationGridTile chosenTile = choices[UnityEngine.Random.Range(0, choices.Count)];
                    character.marker.PlaceMarkerAt(chosenTile);
                }
            }
        }
    }
    #endregion

    #region Relationships
    public Trait CreateRelationshipTrait(RELATIONSHIP_TRAIT type, Character targetCharacter) {
        switch (type) {
            case RELATIONSHIP_TRAIT.ENEMY:
                return new Enemy(targetCharacter);
            case RELATIONSHIP_TRAIT.FRIEND:
                return new Friend(targetCharacter);
            case RELATIONSHIP_TRAIT.RELATIVE:
                return new Relative(targetCharacter);
            case RELATIONSHIP_TRAIT.LOVER:
                return new Lover(targetCharacter);
            case RELATIONSHIP_TRAIT.PARAMOUR:
                return new Paramour(targetCharacter);
            case RELATIONSHIP_TRAIT.MASTER:
                return new Master(targetCharacter);
            case RELATIONSHIP_TRAIT.SERVANT:
                return new Servant(targetCharacter);
            case RELATIONSHIP_TRAIT.SAVER:
                return new Saver(targetCharacter);
            case RELATIONSHIP_TRAIT.SAVE_TARGET:
                return new SaveTarget(targetCharacter);
        }
        return null;
    }
    public CharacterRelationshipData CreateNewRelationshipBetween(Character currCharacter, Character targetCharacter, RELATIONSHIP_TRAIT rel, bool triggerOnAdd = true) {
        RELATIONSHIP_TRAIT pair = GetPairedRelationship(rel);
        //if (currCharacter.CanHaveRelationshipWith(rel, targetCharacter)
        //    && targetCharacter.CanHaveRelationshipWith(pair, currCharacter)) {

        currCharacter.AddTrait(CreateRelationshipTrait(rel, targetCharacter), null, null, null, triggerOnAdd);
        targetCharacter.AddTrait(CreateRelationshipTrait(pair, currCharacter), null, null, null, triggerOnAdd);

        if (currCharacter.GetRelationshipTraitWith(targetCharacter, rel) == null
            || targetCharacter.GetRelationshipTraitWith(currCharacter, pair) == null) {
            Debug.LogWarning(currCharacter.name + " and " + targetCharacter.name + " have inconsistent relationships: " + rel.ToString() + " - " + pair.ToString());
        }

        return currCharacter.GetCharacterRelationshipData(targetCharacter);
            //else {
            //    Debug.Log(currCharacter.name + " and " + targetCharacter.name + " became " + rel.ToString() + " - " + pair.ToString());
            //}

        //} else {
        //    Debug.LogWarning(currCharacter.name + " and " + targetCharacter.name + " cannot have relationship " + rel.ToString() + " - " + pair.ToString());
        //}
    }
    public void RemoveRelationshipBetween(Character character, Character targetCharacter, RELATIONSHIP_TRAIT rel, bool triggerOnRemove = true) {
        if (!character.relationships.ContainsKey(targetCharacter)
            || !targetCharacter.relationships.ContainsKey(character)) {
            return;
        }
        if (!character.HasRelationshipOfTypeWith(targetCharacter, rel)) {
            //the source character does not have that type of relationship with the character
            return;
        }
        RELATIONSHIP_TRAIT pair = GetPairedRelationship(rel);
        if (character.relationships[targetCharacter].HasRelationshipTrait(rel)
            && targetCharacter.relationships[character].HasRelationshipTrait(pair)) {

            character.RemoveTrait(character.GetRelationshipTraitWith(targetCharacter, rel), triggerOnRemove);
            targetCharacter.RemoveTrait(targetCharacter.GetRelationshipTraitWith(character, rel), triggerOnRemove);
        } else {
            Debug.LogWarning(character.name + " and " + targetCharacter.name + " have inconsistent relationships " + rel.ToString() + " - " + pair.ToString() + ". Cannot remove!");
        }
    }
    public void RemoveRelationshipBetween(Character character, Character targetCharacter, List<RelationshipTrait> rels, bool triggerOnRemove = true) {
        if (!character.relationships.ContainsKey(targetCharacter)
            || !targetCharacter.relationships.ContainsKey(character)) {
            return;
        }
        for (int i = 0; i < rels.Count; i++) {
            RelationshipTrait currRel = rels[i];
            RemoveRelationshipBetween(character, targetCharacter, currRel.relType, triggerOnRemove);
        }
    }
    public void RemoveRelationshipBetween(Character character, Character targetCharacter, bool triggerOnRemove = true) {
        if (!character.relationships.ContainsKey(targetCharacter)
            || !targetCharacter.relationships.ContainsKey(character)) {
            return;
        }
        List<RELATIONSHIP_TRAIT> rels = character.GetAllRelationshipTraitTypesWith(targetCharacter);
        for (int i = 0; i < rels.Count; i++) {
            RemoveRelationshipBetween(character, targetCharacter, rels[i], triggerOnRemove);
        }

        //character.RemoveRelationship(targetCharacter);
        //targetCharacter.RemoveRelationship(character);
    }
    private RELATIONSHIP_TRAIT GetPairedRelationship(RELATIONSHIP_TRAIT rel) {
        switch (rel) {
            case RELATIONSHIP_TRAIT.ENEMY:
                return RELATIONSHIP_TRAIT.ENEMY;
            case RELATIONSHIP_TRAIT.FRIEND:
                return RELATIONSHIP_TRAIT.FRIEND;
            case RELATIONSHIP_TRAIT.RELATIVE:
                return RELATIONSHIP_TRAIT.RELATIVE;
            case RELATIONSHIP_TRAIT.LOVER:
                return RELATIONSHIP_TRAIT.LOVER;
            case RELATIONSHIP_TRAIT.PARAMOUR:
                return RELATIONSHIP_TRAIT.PARAMOUR;
            case RELATIONSHIP_TRAIT.MASTER:
                return RELATIONSHIP_TRAIT.SERVANT;
            case RELATIONSHIP_TRAIT.SERVANT:
                return RELATIONSHIP_TRAIT.MASTER;
            case RELATIONSHIP_TRAIT.SAVER:
                return RELATIONSHIP_TRAIT.SAVE_TARGET;
            case RELATIONSHIP_TRAIT.SAVE_TARGET:
                return RELATIONSHIP_TRAIT.SAVER;
            default:
                return RELATIONSHIP_TRAIT.NONE;
        }
    }
    public void SetIsDisabledRelationshipBetween(Character character1, Character character2, bool state) {
        CharacterRelationshipData data1 = null;
        CharacterRelationshipData data2 = null;
        if (character1.relationships.ContainsKey(character2)) {
            data1 = character1.relationships[character2];
        }
        if (character2.relationships.ContainsKey(character1)) {
            data2 = character2.relationships[character1];
        }
        if (data1 != null && data2 != null) {
            data1.SetIsDisabled(state);
            data2.SetIsDisabled(state);
        } else {
            Debug.LogError("Inconsistency! Either " + character1.name + " or " + character2.name + " has no relationship data with the other");
        }
    }
    public void GenerateRelationships() {
        int maxInitialRels = 3;
        RELATIONSHIP_TRAIT[] relsInOrder = new RELATIONSHIP_TRAIT[] { RELATIONSHIP_TRAIT.RELATIVE, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.ENEMY, RELATIONSHIP_TRAIT.FRIEND, RELATIONSHIP_TRAIT.PARAMOUR };
        for (int i = 0; i < allCharacters.Count; i++) {
            Character currCharacter = allCharacters[i];
            int currentRelCount = currCharacter.GetAllRelationshipCount(new List<RELATIONSHIP_TRAIT>() { RELATIONSHIP_TRAIT.MASTER, RELATIONSHIP_TRAIT.SERVANT });
            if (currentRelCount >= maxInitialRels) {
                continue; //skip
            }
            int totalCreatedRels = currentRelCount;
            string summary = currCharacter.name + " relationship generation summary:";
            for (int k = 0; k < relsInOrder.Length; k++) {
                RELATIONSHIP_TRAIT currRel = relsInOrder[k];
                if (totalCreatedRels >= maxInitialRels) {
                    summary += "\nMax Initial Relationships reached, stopping relationship generation for " + currCharacter.name;
                    break; //stop generating more relationships for this character
                }
                int relsToCreate = 0;
                int chance = Random.Range(0, 100);
                switch (currRel) {
                    case RELATIONSHIP_TRAIT.RELATIVE:
                        if (currCharacter.role.roleType == CHARACTER_ROLE.BEAST) { continue; } //a beast character has no relatives
                        else {
                            //- a non-beast character may have either zero (35%), one (50%) or two (15%) relatives from characters of the same race
                            if (chance < 35)  relsToCreate = 0;
                            else if (chance >= 35 && chance < 85)  relsToCreate = 1;
                            else relsToCreate = 2;
                        }
                        break;
                    case RELATIONSHIP_TRAIT.LOVER:
                        //- a character has a 25% chance to have a lover
                        if (chance < 25) relsToCreate = 1;
                        break;
                    case RELATIONSHIP_TRAIT.ENEMY:
                        //- a character may have either zero (25%), one (60%) or two (15%) enemies
                        if (chance < 25) relsToCreate = 0;
                        else if (chance >= 25 && chance < 85) relsToCreate = 1;
                        else relsToCreate = 1;
                        break;
                    case RELATIONSHIP_TRAIT.FRIEND:
                        //- a character may have either zero (15%), one (35%) or two (50%) friends
                        if (chance < 15) relsToCreate = 0;
                        else if (chance >= 15 && chance < 50) relsToCreate = 1;
                        else relsToCreate = 2;
                        break;
                    case RELATIONSHIP_TRAIT.PARAMOUR:
                        //- only valid for non-beast characters with lovers
                        if (currCharacter.role.roleType == CHARACTER_ROLE.BEAST || currCharacter.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER) == null) { continue; }
                        //- a character may have either zero (75%) or one (25%) paramour
                        if (chance < 75) relsToCreate = 0;
                        else relsToCreate = 1;
                        break;
                }
                summary += "\n===========Creating " + relsToCreate + " " + currRel.ToString() + " Relationships...==========";
                if (relsToCreate > 0) {
                    WeightedFloatDictionary<Character> relWeights = new WeightedFloatDictionary<Character>();
                    for (int l = 0; l < allCharacters.Count; l++) {
                        Character otherCharacter = allCharacters[l];
                        if (currCharacter.id != otherCharacter.id && currCharacter.faction == otherCharacter.faction) {
                            List<RELATIONSHIP_TRAIT> existingRels = currCharacter.GetAllRelationshipTraitTypesWith(otherCharacter);
                            float weight = 0;
                            switch (currRel) {
                                case RELATIONSHIP_TRAIT.RELATIVE:
                                    if (otherCharacter.role.roleType == CHARACTER_ROLE.BEAST) { continue; } //a beast character has no relatives
                                    else {
                                        if (otherCharacter.specificLocation.id == currCharacter.specificLocation.id) weight += 50; // character is in same location: +50 Weight
                                        else weight += 10; //character is in different location: +10 Weight

                                        if (currCharacter.race != otherCharacter.race) weight *= 0; //character is a different race: Weight x0
                                        if (existingRels != null && existingRels.Contains(RELATIONSHIP_TRAIT.FRIEND)) {
                                            weight *= 0;
                                        }
                                    }
                                    break;
                                case RELATIONSHIP_TRAIT.LOVER:
                                    if (currCharacter.CanHaveRelationshipWith(currRel, otherCharacter) && otherCharacter.CanHaveRelationshipWith(currRel, currCharacter)) {
                                        if (currCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
                                            /*
                                         - if non beast, from valid characters, choose based on these weights
                                          - character is in same location: +50 Weight
                                          - character is in different location: +10 Weight
                                          - character is the same race: Weight x5
                                          - character is the opposite gender: Weight x6
                                          - character is a beast: Weight x0
                                          - character is a relative: Weight x0.1    
                                         */
                                            if (otherCharacter.specificLocation.id == currCharacter.specificLocation.id) {
                                                weight += 50;
                                            } else {
                                                weight += 10;
                                            }
                                            if (currCharacter.race == otherCharacter.race) {
                                                weight *= 5;
                                            }
                                            if (currCharacter.gender != otherCharacter.gender) {
                                                weight *= 6;
                                            }
                                            if (otherCharacter.role.roleType == CHARACTER_ROLE.BEAST) {
                                                weight *= 0;
                                            }
                                            if (existingRels != null && existingRels.Contains(RELATIONSHIP_TRAIT.RELATIVE)) {
                                                weight *= 0.1f;
                                            }
                                        } else {
                                            /*
                                         - if beast, from valid characters, choose based on these weights
                                          - character is in same location: +50 Weight
                                          - character is in different location: +10 Weight
                                          - character is a different race: Weight x0
                                          - character is the opposite gender: Weight x6
                                         */
                                            if (otherCharacter.specificLocation.id == currCharacter.specificLocation.id) {
                                                weight += 50;
                                            } else {
                                                weight += 10;
                                            }
                                            if (currCharacter.race != otherCharacter.race) {
                                                weight *= 0;
                                            }
                                            if (currCharacter.gender != otherCharacter.gender) {
                                                weight *= 6;
                                            }
                                        }
                                    }
                                    break;
                                case RELATIONSHIP_TRAIT.ENEMY:
                                    if (currCharacter.CanHaveRelationshipWith(currRel, otherCharacter) && otherCharacter.CanHaveRelationshipWith(currRel, currCharacter)) {
                                        if (currCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
                                            /*
                                         - if non beast, from valid characters, choose based on these weights
                                           - character is non-beast: +50 Weight
                                           - character is a beast: +10 Weight
                                           - character is from same faction: Weight x4
                                           - character is a relative: Weight x0.5
                                           - character is a different race: Weight x2
                                           - character is a lover: Weight x0
                                           - character is a master/servant: Weight x0
                                         */
                                            if (otherCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
                                                weight += 50;
                                            } else {
                                                weight += 10;
                                            }
                                            if (currCharacter.faction.id == otherCharacter.faction.id) {
                                                weight *= 4;
                                            }
                                            if (existingRels != null) {
                                                if (existingRels.Contains(RELATIONSHIP_TRAIT.RELATIVE)) {
                                                    weight *= 0.5f;
                                                }
                                                if (existingRels.Contains(RELATIONSHIP_TRAIT.LOVER) 
                                                    || existingRels.Contains(RELATIONSHIP_TRAIT.MASTER) 
                                                    || existingRels.Contains(RELATIONSHIP_TRAIT.SERVANT)) {
                                                    weight *= 0;
                                                }
                                            }
                                        } else {
                                            /*
                                        - if beast, from valid characters, choose based on these weights
                                           - character is non-beast: +10 Weight
                                           - character is a beast: +50 Weight
                                           - character is a relative: Weight x0
                                           - character is a different race: Weight x2
                                         */
                                            if (otherCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
                                                weight += 10;
                                            } else {
                                                weight += 50;
                                            }
                                            if (existingRels != null) {
                                                if (existingRels.Contains(RELATIONSHIP_TRAIT.RELATIVE)) {
                                                    weight *= 0;
                                                }
                                            }
                                            if (currCharacter.race != otherCharacter.race) {
                                                weight *= 2;
                                            }
                                        }
                                    }
                                    break;
                                case RELATIONSHIP_TRAIT.FRIEND:
                                    if (currCharacter.CanHaveRelationshipWith(currRel, otherCharacter) && otherCharacter.CanHaveRelationshipWith(currRel, currCharacter)) {
                                        if (currCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
                                            /*
                                         - if non beast, from valid characters, choose based on these weights
                                           - character is non-beast: +50 Weight
                                           - character is a beast: +5 Weight
                                           - character is from same faction: Weight x4
                                           - character is from same race: Weight x2
                                           - character is a relative: Weight x0
                                           - character is a lover: Weight x0
                                           - character is a master/servant: Weight x0
                                           - character is an enemy: Weight x0
                                         */
                                            if (otherCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
                                                weight += 50;
                                            } else {
                                                weight += 5;
                                            }
                                            if (currCharacter.faction.id == otherCharacter.faction.id) {
                                                weight *= 4;
                                            }
                                            if (currCharacter.race == otherCharacter.race) {
                                                weight *= 2;
                                            }
                                            if (existingRels != null) {
                                                if (existingRels.Contains(RELATIONSHIP_TRAIT.RELATIVE) 
                                                    || existingRels.Contains(RELATIONSHIP_TRAIT.LOVER)
                                                    || existingRels.Contains(RELATIONSHIP_TRAIT.MASTER)
                                                    || existingRels.Contains(RELATIONSHIP_TRAIT.SERVANT)
                                                    || existingRels.Contains(RELATIONSHIP_TRAIT.ENEMY)) {
                                                    weight *= 0;
                                                }
                                            }
                                            
                                        } else {
                                            /*
                                        - if beast, from valid characters, choose based on these weights
                                           - character is beast: +50 Weight
                                           - character is non-beast: +5 Weight
                                           - character is from same faction: Weight x4
                                           - character is from same race: Weight x2
                                           - character is a relative: Weight x0
                                           - character is a lover: Weight x0
                                           - character is a master/servant: Weight x0
                                           - character is an enemy: Weight x0
                                         */
                                            if (otherCharacter.role.roleType == CHARACTER_ROLE.BEAST) {
                                                weight += 50;
                                            } else {
                                                weight += 5;
                                            }

                                            if (currCharacter.faction.id == otherCharacter.faction.id) {
                                                weight *= 4;
                                            }
                                            if (currCharacter.race == otherCharacter.race) {
                                                weight *= 2;
                                            }

                                            if (existingRels != null) {
                                                if (existingRels.Contains(RELATIONSHIP_TRAIT.RELATIVE)
                                                    || existingRels.Contains(RELATIONSHIP_TRAIT.LOVER)
                                                    || existingRels.Contains(RELATIONSHIP_TRAIT.MASTER)
                                                    || existingRels.Contains(RELATIONSHIP_TRAIT.SERVANT)
                                                    || existingRels.Contains(RELATIONSHIP_TRAIT.ENEMY)) {
                                                    weight *= 0;
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case RELATIONSHIP_TRAIT.PARAMOUR:
                                    if (otherCharacter.role.roleType == CHARACTER_ROLE.BEAST) { continue; } //a beast character has no paramours
                                    if (currCharacter.CanHaveRelationshipWith(currRel, otherCharacter) && otherCharacter.CanHaveRelationshipWith(currRel, currCharacter)) {
                                        /*
                                        - if non beast, from valid characters, choose based on these weights
                                           - character is non-beast: +50 Weight
                                           - character is same gender of lover: Weight x6
                                           - character is from same race: Weight x5
                                           - character is from same faction: Weight x3
                                           - character is a relative: Weight x0.1
                                           - character is a lover: Weight x0
                                           - character is a master/servant: Weight x0
                                           - character is an enemy: Weight x0
                                        */
                                        if (otherCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
                                            weight += 50;
                                        }
                                        Character lover = currCharacter.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER);
                                        if (otherCharacter.gender == lover.gender) {
                                            weight *= 6;
                                        }
                                        if (currCharacter.race == otherCharacter.race) {
                                            weight *= 5;
                                        }
                                        if (currCharacter.faction.id == otherCharacter.faction.id) {
                                            weight *= 3;
                                        }
                                        
                                        if (existingRels != null) {
                                            if (existingRels.Contains(RELATIONSHIP_TRAIT.RELATIVE)) {
                                                weight *= 0.1f;
                                            }
                                            if (existingRels.Contains(RELATIONSHIP_TRAIT.LOVER)
                                                || existingRels.Contains(RELATIONSHIP_TRAIT.MASTER)
                                                || existingRels.Contains(RELATIONSHIP_TRAIT.SERVANT)
                                                || existingRels.Contains(RELATIONSHIP_TRAIT.ENEMY)) {
                                                weight *= 0;
                                            }
                                        }

                                    }
                                    break;
                            }
                            if (weight > 0f) {
                                relWeights.AddElement(otherCharacter, weight);
                            }
                        }
                    }
                    if (relWeights.GetTotalOfWeights() > 0) {
                        summary += "\n" + relWeights.GetWeightsSummary("Weights are: ");
                    } else {
                        summary += "\nThere are no valid characters to have a relationship with.";
                    }


                    for (int j = 0; j < relsToCreate; j++) {
                        if (relWeights.GetTotalOfWeights() > 0) {
                            Character chosenCharacter = relWeights.PickRandomElementGivenWeights();
                            CreateNewRelationshipBetween(currCharacter, chosenCharacter, currRel);
                            totalCreatedRels++;
                            summary += "\nCreated new relationship " + currRel.ToString() + " between " + currCharacter.name + " and " + chosenCharacter.name + ". Total relationships created for " + currCharacter.name + " are " + totalCreatedRels.ToString();
                            relWeights.RemoveElement(chosenCharacter);
                        } else {
                            break;
                        }
                        if (totalCreatedRels >= maxInitialRels) {
                            //summary += "\nMax Initial Relationships reached, stopping relationship generation for " + currCharacter.name;
                            break; //stop generating more relationships for this character
                        }
                    }
                }
            }
            //Debug.Log(summary);
        }
    }
    #endregion

    #region Utilities
    public Character GetCharacterByID(int id) {
        for (int i = 0; i < _allCharacters.Count; i++) {
            Character currChar = _allCharacters[i];
            if (currChar.id == id) {
                return currChar;
            }
        }
        return null;
    }
    public Character GetCharacterByName(string name) {
        for (int i = 0; i < _allCharacters.Count; i++) {
            Character currChar = _allCharacters[i];
            if (currChar.name.Equals(name, System.StringComparison.CurrentCultureIgnoreCase)) {
                return currChar;
            }
        }
        return null;
    }
    //public void GenerateCharactersForTesting(int number) {
    //    List<BaseLandmark> allLandmarks = LandmarkManager.Instance.GetAllLandmarks().Where(x => x.owner != null).ToList();
    //    //List<Settlement> allOwnedSettlements = new List<Settlement>();
    //    //for (int i = 0; i < FactionManager.Instance.allTribes.Count; i++) {
    //    //    allOwnedSettlements.AddRange(FactionManager.Instance.allTribes[i].settlements);
    //    //}
    //    WeightedDictionary<CHARACTER_ROLE> characterRoleProductionDictionary = LandmarkManager.Instance.GetCharacterRoleProductionDictionary();

    //    for (int i = 0; i < number; i++) {
    //        BaseLandmark chosenLandmark = allLandmarks[Random.Range(0, allLandmarks.Count)];
    //        //WeightedDictionary<CHARACTER_CLASS> characterClassProductionDictionary = LandmarkManager.Instance.GetCharacterClassProductionDictionary(chosenSettlement);

    //        //CHARACTER_CLASS chosenClass = characterClassProductionDictionary.PickRandomElementGivenWeights();
    //        CHARACTER_CLASS chosenClass = CHARACTER_CLASS.WARRIOR;
    //        CHARACTER_ROLE chosenRole = characterRoleProductionDictionary.PickRandomElementGivenWeights();
    //        Character newChar = chosenLandmark.CreateNewCharacter(RACE.HUMANS, chosenRole, Utilities.NormalizeString(chosenClass.ToString()), false);
    //        //Initial Character tags
    //        newChar.AssignInitialTags();
    //        //CharacterManager.Instance.EquipCharacterWithBestGear(chosenSettlement, newChar);
    //    }
    //}
    public List<string> GetNonCivilianClasses() {
        return classesDictionary.Keys.Where(x => x != "Civilian").ToList();
    }
    public List<Character> GetCharactersWithClass(string className) {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < allCharacters.Count; i++) {
            Character currChar = allCharacters[i];
            if (currChar.characterClass != null && currChar.characterClass.className == className) {
                characters.Add(currChar);
            }
        }
        return characters;
    }
    public bool HasCharacterWithClass(string className) {
        for (int i = 0; i < allCharacters.Count; i++) {
            Character currChar = allCharacters[i];
            if (currChar.characterClass != null && currChar.characterClass.className == className) {
                return true;
            }
        }
        return false;
    }
    public Party GetPartyByID(int id) {
        for (int i = 0; i < allCharacters.Count; i++) {
            Character currCharacter = allCharacters[i];
            if (currCharacter.ownParty.id == id) {
                return currCharacter.ownParty;
            } else if (currCharacter.currentParty.id == id) {
                return currCharacter.currentParty;
            }
        }
        return null;
    }
    public void CategorizeLog(string log, string stackTrace, LogType type) {
        Dictionary<Character, List<string>> modifiedLogs = new Dictionary<Character, List<string>>();
        foreach (KeyValuePair<Character, List<string>> kvp in allCharacterLogs) {
            Character currCharacter = kvp.Key;
            List<string> currLogs = kvp.Value;
            if (log.Contains(currCharacter.name) || log.Contains(currCharacter.name + "'s")) {
                currLogs.Add(log + " Stack Trace: \n" + stackTrace);
                if (currLogs.Count > 50) {
                    currLogs.RemoveAt(0);
                }
            }
            //allCharacterLogs[currCharacter] = currLogs;
            modifiedLogs.Add(currCharacter, currLogs);
        }
        allCharacterLogs = modifiedLogs;
    }
    public List<string> GetCharacterLogs(Character character) {
        if (allCharacterLogs.ContainsKey(character)) {
            return allCharacterLogs[character];
        }
        return null;
    }
    public Sprite GetJobSprite(JOB job) {
        if (jobIcons.ContainsKey(job)) {
            return jobIcons[job];
        }
        return null;
    }
    #endregion

    #region Avatars
    public Sprite GetSpriteByRole(CHARACTER_ROLE role){
        return heroSprite;
        //switch(role){
        //case CHARACTER_ROLE.HERO:
        //	return heroSprite;
        //case CHARACTER_ROLE.VILLAIN:
        //	return villainSprite;
        //      case CHARACTER_ROLE.BEAST:
        //          return beastSprite;
        //      case CHARACTER_ROLE.BANDIT:
        //          return banditSprite;
        //      case CHARACTER_ROLE.LEADER:
        //          return chieftainSprite;
        //      }
        //return null;
    }
    public Sprite GetSpriteByMonsterType(MONSTER_TYPE monsterType) {
        //TODO: Add different sprite for diff monster types
        return beastSprite;
    }
    #endregion

    #region Character Portraits
    public PortraitAssetCollection GetPortraitAssets(RACE race, GENDER gender) {
        //if (race != RACE.HUMANS) {
        //    race = RACE.ELVES; //TODO: Change this when needed assets arrive
        //}
        for (int i = 0; i < portraitAssets.Count; i++) {
            RacePortraitAssets racePortraitAssets = portraitAssets[i];
            if (racePortraitAssets.race == race) {
                if (gender == GENDER.MALE) {
                    return racePortraitAssets.maleAssets;
                } else {
                    return racePortraitAssets.femaleAssets;
                }
            }
        }

        if (gender == GENDER.MALE) {
            return portraitAssets[0].maleAssets;
        } else {
            return portraitAssets[0].femaleAssets;
        }
        //throw new System.Exception("No portraits for " + race.ToString() + " " + gender.ToString());
    }
    public PortraitSettings GenerateRandomPortrait(RACE race, GENDER gender) {
        PortraitAssetCollection pac = GetPortraitAssets(race, gender);
        PortraitSettings ps = new PortraitSettings();
        ps.race = race;
        ps.gender = gender;
        ps.skinIndex = Random.Range(0, pac.skinAssets.Count);
        ps.hairIndex = Random.Range(0, pac.hairAssets.Count);
        ps.underIndex = Random.Range(0, pac.underAssets.Count);
        ps.topIndex = Random.Range(0, pac.topAssets.Count);
        ps.bodyIndex = Random.Range(0, pac.bodyAssets.Count);
        return ps;
    }
    public PortraitSettings GenerateRandomPortrait() {
        RACE randomRace = RACE.HUMANS;
        if (Random.Range(0, 2) == 1) {
            randomRace = RACE.ELVES;
        }
        GENDER[] genderChoices = Utilities.GetEnumValues<GENDER>();
        GENDER randomGender = genderChoices[Random.Range(0, genderChoices.Length)];
        return GenerateRandomPortrait(randomRace, randomGender);
    }
    public Sprite GetHairSprite(int index, RACE race, GENDER gender) {
        PortraitAssetCollection pac = GetPortraitAssets(race, gender);
        return pac.hairAssets.ElementAtOrDefault(index);
    }
    public Sprite GetBodySprite(int index, RACE race, GENDER gender) {
        PortraitAssetCollection pac = GetPortraitAssets(race, gender);
        return pac.bodyAssets.ElementAtOrDefault(index);
    }
    public Sprite GetSkinSprite(int index, RACE race, GENDER gender) {
        PortraitAssetCollection pac = GetPortraitAssets(race, gender);
        return pac.skinAssets.ElementAtOrDefault(index);
    }
    public Sprite GetTopSprite(int index, RACE race, GENDER gender) {
        PortraitAssetCollection pac = GetPortraitAssets(race, gender);
        return pac.topAssets.ElementAtOrDefault(index);
    }
    public Sprite GetUnderSprite(int index, RACE race, GENDER gender) {
        PortraitAssetCollection pac = GetPortraitAssets(race, gender);
        return pac.underAssets.ElementAtOrDefault(index);
    }
    public PortraitFrame GetPortraitFrame(CHARACTER_ROLE role) {
        if (portraitFrames.ContainsKey(role)) {
            return portraitFrames[role];
        }
        throw new System.Exception("There is no frame for role " + role.ToString());
    }
    public Sprite GetClassPortraitSprite(string className) {
        if (classPortraits.ContainsKey(className)) {
            return classPortraits[className];
        }
        return null;
    }
    #endregion

    #region Elements
    private void ConstructElementChanceDictionary() {
        _elementsChanceDictionary = new Dictionary<ELEMENT, float>();
        ELEMENT[] elements = (ELEMENT[]) System.Enum.GetValues(typeof(ELEMENT));
        for (int i = 0; i < elements.Length; i++) {
            _elementsChanceDictionary.Add(elements[i], 0f);
        }
    }
    #endregion

    #region Interaction
    private void ConstructRoleInteractions() {
        characterRoleInteractions = new Dictionary<CHARACTER_ROLE, INTERACTION_TYPE[]>() {
            {CHARACTER_ROLE.CIVILIAN, new INTERACTION_TYPE[] {
                INTERACTION_TYPE.MINE_ACTION,
                INTERACTION_TYPE.CHOP_WOOD,
                INTERACTION_TYPE.SCRAP,
            } },
            {CHARACTER_ROLE.SOLDIER, new INTERACTION_TYPE[] {
                INTERACTION_TYPE.PATROL,
                INTERACTION_TYPE.PATROL_ROAM,
            } },
        };
    }
    private void ConstructAwayFromHomeInteractionWeights() {
        awayFromHomeInteractionWeights = new Dictionary<INTERACTION_TYPE, int> {
            { INTERACTION_TYPE.MOVE_TO_RETURN_HOME, 100 },
            { INTERACTION_TYPE.FOUND_LUCARETH, 50 },
            //{ INTERACTION_TYPE.FOUND_BESTALIA, 50 },
            { INTERACTION_TYPE.FOUND_MAGUS, 50 },
            { INTERACTION_TYPE.FOUND_ZIRANNA, 50 },
            //{ INTERACTION_TYPE.CHANCE_ENCOUNTER, 2 },
            { INTERACTION_TYPE.TRANSFER_HOME, 50 },
        };
    }
    private void ConstructAtHomeInteractionWeights() {
        atHomeInteractionWeights = new Dictionary<INTERACTION_TYPE, int> {
            //{ INTERACTION_TYPE.CHANCE_ENCOUNTER, 2 },
            { INTERACTION_TYPE.STEAL_ACTION, 10 },
        };
    }
    #endregion

    #region Animator
    public RuntimeAnimatorController GetAnimatorByRole(CHARACTER_ROLE role) {
        for (int i = 0; i < characterAnimators.Length; i++) {
            if (characterAnimators[i].name == role.ToString()) {
                return characterAnimators[i];
            }
        }
        return null;
    }
    #endregion

    #region Squad Emblems
    public EmblemBG GetRandomEmblemBG() {
        return _emblemBGs[Random.Range(0, _emblemBGs.Count)];
    }
    public Sprite GetRandomEmblem() {
        return _emblemSymbols[Random.Range(0, _emblemSymbols.Count)];
    }
    public int GetEmblemBGIndex(EmblemBG emblemBG) {
        for (int i = 0; i < _emblemBGs.Count; i++) {
            EmblemBG currBG = _emblemBGs[i];
            if (currBG.Equals(emblemBG)) {
                return i;
            }
        }
        return -1;
    }
    public int GetEmblemIndex(Sprite emblem) {
        for (int i = 0; i < _emblemSymbols.Count; i++) {
            Sprite currSprite = _emblemSymbols[i];
            if (currSprite.name.Equals(emblem.name)) {
                return i;
            }
        }
        return -1;
    }
    #endregion

    #region Marker Assets
    public MarkerAsset GetMarkerAsset(RACE race, GENDER gender) {
        for (int i = 0; i < markerAssets.Count; i++) {
            RaceMarkerAssets currRaceAsset = markerAssets[i];
            if (currRaceAsset.race == race) {
                MarkerAsset asset = currRaceAsset.maleAssets;
                if (gender == GENDER.FEMALE) {
                    asset  = currRaceAsset.femaleAssets;
                }
                return asset;
            }
        }
        if (gender == GENDER.FEMALE) {
            return markerAssets[0].femaleAssets;
        }
        return markerAssets[0].maleAssets;
    }
    #endregion

    #region For Testing
    public void GenerateRelationshipsForTesting() {
        for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
            Faction currFaction = FactionManager.Instance.allFactions[i];
            if (currFaction.isActive) {
                for (int j = 0; j < currFaction.characters.Count; j++) {
                    Character currCharacter = currFaction.characters[j];
                    Character targetCharacter = GetRandomCharacter(currFaction.characters.Where(x => x.id != currCharacter.id).ToList());
                    RELATIONSHIP_TRAIT rel = GetRandomRelationship();
                    CreateNewRelationshipBetween(currCharacter, targetCharacter, rel);
                }
            }
        }
    }
    private Character GetRandomCharacter(List<Character> characters) {
        return characters[Random.Range(0, characters.Count)];
    }
    private RELATIONSHIP_TRAIT GetRandomRelationship() {
        WeightedDictionary<RELATIONSHIP_TRAIT> relWeights = new WeightedDictionary<RELATIONSHIP_TRAIT>();
        relWeights.AddElement(RELATIONSHIP_TRAIT.ENEMY, 35);
        relWeights.AddElement(RELATIONSHIP_TRAIT.FRIEND, 35);
        relWeights.AddElement(RELATIONSHIP_TRAIT.LOVER, 10);
        relWeights.AddElement(RELATIONSHIP_TRAIT.RELATIVE, 10);
        relWeights.AddElement(RELATIONSHIP_TRAIT.PARAMOUR, 10);
        //RELATIONSHIP_TRAIT[] choices = Utilities.GetEnumValues<RELATIONSHIP_TRAIT>();
        return relWeights.PickRandomElementGivenWeights();
    }
    #endregion
}

[System.Serializable]
public class PortraitFrame {
    public Sprite baseBG;
    public Sprite frameOutline;
}
