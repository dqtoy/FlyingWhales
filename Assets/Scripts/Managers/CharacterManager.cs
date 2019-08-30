using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CharacterManager : MonoBehaviour {

    public static CharacterManager Instance = null;

    public const int MAX_HISTORY_LOGS = 300;
    public const int CHARACTER_MAX_MEMORY = 20;
    public const int FULLNESS_DECREASE_RATE = 380;
    public const int TIREDNESS_DECREASE_RATE = 340;
    public const int HAPPINESS_DECREASE_RATE = 640;

    public GameObject characterIconPrefab;
    public Transform characterIconsParent;

    public int maxLevel;
    private Dictionary<ELEMENT, float> _elementsChanceDictionary;
    private List<Character> _allCharacters;
    private List<CharacterAvatar> _allCharacterAvatars;

	public Sprite heroSprite;

    [Header("Character Portrait Assets")]
    public GameObject characterPortraitPrefab;
    [SerializeField] private List<RacePortraitAssets> portraitAssets;
    public List<Color> hairColors;
    [SerializeField] private RolePortraitFramesDictionary portraitFrames;
    [SerializeField] private StringSpriteDictionary classPortraits;
    public Material hsvMaterial;

    [Header("Character Marker Assets")]
    [SerializeField] private List<RaceMarkerAssets> markerAssets;

    [Header("Summon Settings")]
    [SerializeField] private SummonSettingDictionary summonSettings;

    [Header("Artifact Settings")]
    [SerializeField] private ArtifactSettingDictionary artifactSettings;

    //alter egos
    public const string Original_Alter_Ego = "Original";

    public Dictionary<Character, List<string>> allCharacterLogs { get; private set; }
    //public Dictionary<INTERACTION_TYPE, int> awayFromHomeInteractionWeights { get; private set; }
    //public Dictionary<INTERACTION_TYPE, int> atHomeInteractionWeights { get; private set; }
    public Dictionary<CHARACTER_ROLE, INTERACTION_TYPE[]> characterRoleInteractions { get; private set; }
    public Dictionary<string, CharacterClass> classesDictionary { get; private set; }
    public Dictionary<string, CharacterClass> uniqueClasses { get; private set; }
    public Dictionary<string, CharacterClass> normalClasses { get; private set; }
    public Dictionary<string, CharacterClass> beastClasses { get; private set; }
    public Dictionary<string, CharacterClass> demonClasses { get; private set; }
    public Dictionary<string, Dictionary<string, CharacterClass>> identifierClasses { get; private set; }

    public static readonly string[] sevenDeadlySinsClassNames = { "Lust", "Gluttony", "Greed", "Sloth", "Wrath", "Envy", "Pride" };
    private List<string> deadlySinsRotation = new List<string>();

    #region getters/setters
    public List<Character> allCharacters {
        get { return _allCharacters; }
    }
    public Dictionary<ELEMENT, float> elementsChanceDictionary {
        get { return _elementsChanceDictionary; }
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
        //ConstructAwayFromHomeInteractionWeights();
        //ConstructAtHomeInteractionWeights();
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
                    //FactionSaveData factionData = data.GetFactionData(characterFaction.id);
                    //if (factionData.leaderID != -1 && factionData.leaderID == currCharacter.id) {
                    //    characterFaction.SetLeader(currCharacter);
                    //}
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
        //Party party = newCharacter.CreateOwnParty();
        newCharacter.Initialize();
        if (faction != null) {
            faction.AddNewCharacter(newCharacter);
        }
#if !WORLD_CREATION_TOOL
        else {
            FactionManager.Instance.neutralFaction.AddNewCharacter(newCharacter);
        }
        newCharacter.ownParty.CreateIcon();
        if(homeLocation != null) {
            newCharacter.ownParty.icon.SetPosition(homeLocation.coreTile.transform.position);
            newCharacter.MigrateHomeTo(homeLocation, homeStructure, false);
            homeLocation.AddCharacterToLocation(newCharacter.ownParty.owner, null, true);
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
        newCharacter.Initialize();
        if (faction != null) {
            faction.AddNewCharacter(newCharacter);
        } else {
            FactionManager.Instance.neutralFaction.AddNewCharacter(newCharacter);
        }
#if !WORLD_CREATION_TOOL
        newCharacter.ownParty.CreateIcon();
        if (homeLocation != null) {
            newCharacter.ownParty.icon.SetPosition(homeLocation.coreTile.transform.position);
            newCharacter.MigrateHomeTo(homeLocation, homeStructure, false);
            homeLocation.AddCharacterToLocation(newCharacter.ownParty.owner, null, true);
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
        newCharacter.Initialize();
        allCharacterLogs.Add(newCharacter, new List<string>());
        if (data.homeAreaID != -1) {
            Area homeArea = LandmarkManager.Instance.GetAreaByID(data.homeAreaID);
            if (homeArea != null) {
#if !WORLD_CREATION_TOOL
                newCharacter.ownParty.CreateIcon();
                newCharacter.ownParty.icon.SetPosition(homeArea.coreTile.transform.position);
                newCharacter.MigrateHomeTo(homeArea, null, false);
                homeArea.AddCharacterToLocation(newCharacter.ownParty.owner, null, true);
#endif
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
    public Character CreateNewCharacter(SaveDataCharacter data) {
        Character newCharacter = new Character(data);
        newCharacter.CreateOwnParty();
        for (int i = 0; i < data.alterEgos.Count; i++) {
            data.alterEgos[i].Load(newCharacter);
        }

        Faction faction = FactionManager.Instance.GetFactionBasedOnID(data.factionID);
        if(faction != null) {
            faction.AddNewCharacter(newCharacter);
            if (data.isFactionLeader) {
                faction.OnlySetLeader(newCharacter);
            }
        }
#if !WORLD_CREATION_TOOL
        newCharacter.ownParty.CreateIcon();

        Area home = null;
        if (data.homeID != -1) {
            home = LandmarkManager.Instance.GetAreaByID(data.homeID);
        }
        Area specificLocation = null;
        if (data.currentLocationID != -1) {
            specificLocation = LandmarkManager.Instance.GetAreaByID(data.currentLocationID);
        }
        if (specificLocation != null) {
            newCharacter.ownParty.icon.SetPosition(specificLocation.coreTile.transform.position);
        }
        if (data.isDead) {
            if (home != null) {
                newCharacter.SetHome(home); //keep this data with character to prevent errors
                home.AssignCharacterToDwellingInArea(newCharacter); //We do not save LocationStructure, so this is only done so that the dead character will not have null issues with homeStructure
            }
            if(specificLocation != null) {
                newCharacter.ownParty.SetSpecificLocation(specificLocation);
            }
        } else {
            if (home != null) {
                newCharacter.MigrateHomeTo(home, null, false);
            }
            if (specificLocation != null) {
                specificLocation.AddCharacterToLocation(newCharacter.ownParty.owner, null, false);
            }
        }
#endif
        for (int i = 0; i < data.items.Count; i++) {
            data.items[i].Load(newCharacter);
        }
        for (int i = 0; i < data.normalTraits.Count; i++) {
            data.normalTraits[i].Load(newCharacter);
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
            //currentClass.ConstructData();
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
    public CharacterClass CreateNewCharacterClass(string className) {
        if (classesDictionary.ContainsKey(className)) {
            return classesDictionary[className].CreateNewCopy();
        }
        return null;
    }
    public string GetDeadlySinsClassNameFromRotation() {
        if (deadlySinsRotation.Count == 0) {
            deadlySinsRotation.AddRange(sevenDeadlySinsClassNames);
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
    public void GenerateInitialAwareness() {
        for (int i = 0; i < allCharacters.Count; i++) {
            Character character = allCharacters[i];
            character.AddInitialAwareness();
        }
    }
    public void PlaceInitialCharacters(Area area) {
        for (int i = 0; i < area.charactersAtLocation.Count; i++) {
            Character character = area.charactersAtLocation[i];
            if (character.marker == null) {
                character.CreateMarker();
            }
            if (character.homeStructure != null && character.homeStructure.location == area) {
                //place the character at a random unoccupied tile in his/her home
                List<LocationGridTile> choices = character.homeStructure.unoccupiedTiles.Where(x => x.charactersHere.Count == 0).ToList();
                LocationGridTile chosenTile = choices[UnityEngine.Random.Range(0, choices.Count)];
                character.InitialCharacterPlacement(chosenTile);
            } else {
                //place the character at a random unoccupied tile in the area's wilderness
                LocationStructure wilderness = area.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
                List<LocationGridTile> choices = wilderness.unoccupiedTiles.Where(x => x.charactersHere.Count == 0).ToList();
                LocationGridTile chosenTile = choices[UnityEngine.Random.Range(0, choices.Count)];
                character.InitialCharacterPlacement(chosenTile);
            }
        }
    }
    public void GiveInitialItems() {
        List<SPECIAL_TOKEN> choices = Utilities.GetEnumValues<SPECIAL_TOKEN>().ToList();
        choices.Remove(SPECIAL_TOKEN.HEALING_POTION);
        choices.Remove(SPECIAL_TOKEN.TOOL);
        for (int i = 0; i < allCharacters.Count; i++) {
            Character character = allCharacters[i];
#if TRAILER_BUILD
            if (character.name == "Fiona" || character.name == "Audrey" || character.name == "Jamie") {
                continue;
            }
#endif
            if (character.minion == null) {
                if (Random.Range(0, 2) == 0) {
                    SPECIAL_TOKEN randomItem = choices[UnityEngine.Random.Range(0, choices.Count)];
                    SpecialToken token = TokenManager.Instance.CreateSpecialToken(randomItem);
                    character.ObtainToken(token);
                }
            }
        }
    }
    #endregion

    #region Summons
    public Summon CreateNewSummon(SUMMON_TYPE summonType, Faction faction = null, Area homeLocation = null, Dwelling homeStructure = null) {
        Summon newCharacter = CreateNewSummonClassFromType(summonType) as Summon;
        newCharacter.Initialize();
        if (faction != null) {
            faction.AddNewCharacter(newCharacter);
        } else {
            FactionManager.Instance.neutralFaction.AddNewCharacter(newCharacter);
        }
        newCharacter.ownParty.CreateIcon();
        if (homeLocation != null) {
            newCharacter.ownParty.icon.SetPosition(homeLocation.coreTile.transform.position);
            newCharacter.MigrateHomeTo(homeLocation, homeStructure, false);
            homeLocation.AddCharacterToLocation(newCharacter.ownParty.owner, null, true);
        }
        newCharacter.CreateInitialTraitsByClass();
        _allCharacters.Add(newCharacter);
        Messenger.Broadcast(Signals.CHARACTER_CREATED, newCharacter);
        return newCharacter;
    }
    public Summon CreateNewSummon(SaveDataCharacter data) {
        Summon newCharacter = CreateNewSummonClassFromType(data);
        newCharacter.CreateOwnParty();
        newCharacter.ConstructInitialGoapAdvertisementActions();

        for (int i = 0; i < data.alterEgos.Count; i++) {
            data.alterEgos[i].Load(newCharacter);
        }

        Faction faction = FactionManager.Instance.GetFactionBasedOnID(data.factionID);
        if (faction != null) {
            faction.AddNewCharacter(newCharacter);
            if (data.isFactionLeader) {
                faction.OnlySetLeader(newCharacter);
            }
        }

        newCharacter.ownParty.CreateIcon();
        Area home = null;
        if (data.homeID != -1) {
            home = LandmarkManager.Instance.GetAreaByID(data.homeID);
        }
        Area specificLocation = null;
        if (data.currentLocationID != -1) {
            specificLocation = LandmarkManager.Instance.GetAreaByID(data.currentLocationID);
        }
        if (specificLocation != null) {
            newCharacter.ownParty.icon.SetPosition(specificLocation.coreTile.transform.position);
        }
        if (data.isDead) {
            if(home != null) {
                newCharacter.SetHome(home); //keep this data with character to prevent errors
                home.AssignCharacterToDwellingInArea(newCharacter); //We do not save LocationStructure, so this is only done so that the dead character will not have null issues with homeStructure
            }
            if(specificLocation != null) {
                newCharacter.ownParty.SetSpecificLocation(specificLocation);
            }
        } else {
            if (home != null) {
                newCharacter.MigrateHomeTo(home, null, false);
            }
            if (specificLocation != null) {
                specificLocation.AddCharacterToLocation(newCharacter.ownParty.owner, null, false);
            }
        }

        for (int i = 0; i < data.items.Count; i++) {
            data.items[i].Load(newCharacter);
        }
        for (int i = 0; i < data.normalTraits.Count; i++) {
            data.normalTraits[i].Load(newCharacter);
        }

        _allCharacters.Add(newCharacter);
        Messenger.Broadcast(Signals.CHARACTER_CREATED, newCharacter);
        return newCharacter;
    }
    public Summon CreateNewSummonClassFromType(SaveDataCharacter data) {
        switch (data.summonType) {
            case SUMMON_TYPE.Wolf:
                return new Wolf(data);
            case SUMMON_TYPE.ThiefSummon:
                return new ThiefSummon(data);
            case SUMMON_TYPE.Skeleton:
                return new Skeleton(data);
            case SUMMON_TYPE.Succubus:
                return new Succubus(data);
            case SUMMON_TYPE.Incubus:
                return new Incubus(data);
            case SUMMON_TYPE.Golem:
                return new Golem(data);
        }
        return null;
    }
    public object CreateNewSummonClassFromType(SUMMON_TYPE summonType) {
        var typeName = summonType.ToString();
        return System.Activator.CreateInstance(System.Type.GetType(typeName));
    }
    public SummonSettings GetSummonSettings(SUMMON_TYPE type) {
        return summonSettings[type];
    }
    public ArtifactSettings GetArtifactSettings(ARTIFACT_TYPE type) {
        return artifactSettings[type];
    }
    #endregion

    #region Relationships
    public void LoadRelationships(WorldSaveData data) {
        if (data.charactersData != null) {
            for (int i = 0; i < data.charactersData.Count; i++) {
                CharacterSaveData currData = data.charactersData[i];
                Character currCharacter = GetCharacterByID(currData.id);
                for (int j = 0; j < currData.relationshipsData.Count; j++) {
                    RelationshipSaveData relData = currData.relationshipsData[j];
                    for (int k = 0; k < relData.rels.Count; k++) {
                        RELATIONSHIP_TRAIT currRel = relData.rels[k];
                        Character target = GetCharacterByID(relData.targetCharacterID);
                        if (!currCharacter.HasRelationshipOfTypeWith(target, currRel)) {
                            CreateNewRelationshipBetween(currCharacter, target, currRel);
                        }
                    }
                }
            }
        }
    }
    public RelationshipTrait CreateRelationshipTrait(RELATIONSHIP_TRAIT type, Character targetCharacter) {
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
    /// <summary>
    /// Add a one way relationship to a character.
    /// </summary>
    /// <param name="currCharacter">The character that will gain the relationship.</param>
    /// <param name="targetCharacter">The character that the new relationship is targetting.</param>
    /// <param name="rel">The type of relationship to create.</param>
    /// <param name="triggerOnAdd">Should this trigger the trait's OnAdd Function.</param>
    /// <returns>The created relationship data.</returns>
    public CharacterRelationshipData CreateNewOneWayRelationship(Character currCharacter, Character targetCharacter, RELATIONSHIP_TRAIT rel) {
        if (!currCharacter.HasRelationshipOfTypeWith(targetCharacter, rel)) {
            if (rel == RELATIONSHIP_TRAIT.ENEMY && currCharacter.GetNormalTrait("Diplomatic") != null) {
                return currCharacter.GetCharacterRelationshipData(targetCharacter);
            }
            currCharacter.AddRelationship(targetCharacter, CreateRelationshipTrait(rel, targetCharacter));
        }
        return currCharacter.GetCharacterRelationshipData(targetCharacter);
    }
    public CharacterRelationshipData CreateNewOneWayRelationship(Character currCharacter, AlterEgoData alterEgo, RELATIONSHIP_TRAIT rel) {
        if (!currCharacter.HasRelationshipOfTypeWith(alterEgo, rel)) {
            if (rel == RELATIONSHIP_TRAIT.ENEMY && currCharacter.GetNormalTrait("Diplomatic") != null) {
                return currCharacter.GetCharacterRelationshipData(alterEgo.owner);
            }
            currCharacter.AddRelationship(alterEgo, CreateRelationshipTrait(rel, alterEgo.owner));
        }
        return currCharacter.GetCharacterRelationshipData(alterEgo);
    }
    public CharacterRelationshipData CreateNewRelationshipBetween(Character currCharacter, Character targetCharacter, RELATIONSHIP_TRAIT rel) {
        RELATIONSHIP_TRAIT pair = GetPairedRelationship(rel);

        if (!(rel == RELATIONSHIP_TRAIT.ENEMY && currCharacter.GetNormalTrait("Diplomatic") != null)) {
            currCharacter.AddRelationship(targetCharacter, CreateRelationshipTrait(rel, targetCharacter));
        }
        if (!(rel == RELATIONSHIP_TRAIT.ENEMY && targetCharacter.GetNormalTrait("Diplomatic") != null)) {
            targetCharacter.AddRelationship(currCharacter, CreateRelationshipTrait(pair, currCharacter));
        }

        if (currCharacter.GetRelationshipTraitWith(targetCharacter, rel) == null
            || targetCharacter.GetRelationshipTraitWith(currCharacter, pair) == null) {
            Debug.LogWarning(currCharacter.name + " and " + targetCharacter.name + " have inconsistent relationships: " + rel.ToString() + " - " + pair.ToString());
        }

        return currCharacter.GetCharacterRelationshipData(targetCharacter);
    }
    public CharacterRelationshipData CreateNewRelationshipBetween(Character currCharacter, AlterEgoData alterEgo, RELATIONSHIP_TRAIT rel) {
        RELATIONSHIP_TRAIT pair = GetPairedRelationship(rel);

        if (!(rel == RELATIONSHIP_TRAIT.ENEMY && currCharacter.GetNormalTrait("Diplomatic") != null)) {
            currCharacter.AddRelationship(alterEgo, CreateRelationshipTrait(rel, alterEgo.owner));
        }

        if (!(rel == RELATIONSHIP_TRAIT.ENEMY && alterEgo.owner.GetNormalTrait("Diplomatic") != null)) {
            alterEgo.AddRelationship(currCharacter.currentAlterEgo, CreateRelationshipTrait(pair, currCharacter));
        }

        if (currCharacter.GetRelationshipTraitWith(alterEgo, rel) == null
            || alterEgo.GetRelationshipTraitWith(currCharacter.currentAlterEgo, pair) == null) {
            Debug.LogWarning(currCharacter.name + " and " + alterEgo.name + " have inconsistent relationships: " + rel.ToString() + " - " + pair.ToString());
        }

        return currCharacter.GetCharacterRelationshipData(alterEgo);
    }
    public void RemoveOneWayRelationship(Character currCharacter, Character targetCharacter, RELATIONSHIP_TRAIT rel) {
        RemoveOneWayRelationship(currCharacter, targetCharacter.currentAlterEgo, rel);
    }
    public void RemoveOneWayRelationship(Character currCharacter, AlterEgoData targetCharacter, RELATIONSHIP_TRAIT rel) {
        currCharacter.RemoveRelationshipWith(targetCharacter, rel);
    }
    public void RemoveRelationshipBetween(Character character, AlterEgoData alterEgo, RELATIONSHIP_TRAIT rel) {
        if (!character.relationships.ContainsKey(alterEgo)
            || !alterEgo.relationships.ContainsKey(character.currentAlterEgo)) {
            return;
        }
        //if (!character.HasRelationshipOfTypeWith(targetCharacter, rel)) {
        //    //the source character does not have that type of relationship with the character
        //    return;
        //}
        RELATIONSHIP_TRAIT pair = GetPairedRelationship(rel);
        if (character.relationships[alterEgo].HasRelationshipTrait(rel)
            && alterEgo.relationships[character.currentAlterEgo].HasRelationshipTrait(pair)) {

            character.RemoveRelationshipWith(alterEgo, rel);
            alterEgo.RemoveRelationship(character.currentAlterEgo, pair);
        } else {
            Debug.LogWarning(character.name + " and " + alterEgo.name + " have inconsistent relationships " + rel.ToString() + " - " + pair.ToString() + ". Cannot remove!");
        }
    }
    public void RemoveRelationshipBetween(Character character, Character targetCharacter, RELATIONSHIP_TRAIT rel) {
        RemoveRelationshipBetween(character, targetCharacter.currentAlterEgo, rel);
    }
    public void RemoveRelationshipBetween(Character character, Character targetCharacter, List<RelationshipTrait> rels) {
        if (!character.relationships.ContainsKey(targetCharacter.currentAlterEgo)
            || !targetCharacter.relationships.ContainsKey(character.currentAlterEgo)) {
            return;
        }
        for (int i = 0; i < rels.Count; i++) {
            RelationshipTrait currRel = rels[i];
            RemoveRelationshipBetween(character, targetCharacter, currRel.relType);
        }
    }
    public void RemoveRelationshipBetween(Character character, Character targetCharacter) {
        if (!character.relationships.ContainsKey(targetCharacter.currentAlterEgo)
            || !targetCharacter.relationships.ContainsKey(character.currentAlterEgo)) {
            return;
        }
        List<RELATIONSHIP_TRAIT> rels = character.GetAllRelationshipTraitTypesWith(targetCharacter);
        for (int i = 0; i < rels.Count; i++) {
            RemoveRelationshipBetween(character, targetCharacter, rels[i]);
        }
        Messenger.Broadcast(Signals.ALL_RELATIONSHIP_REMOVED, character, targetCharacter);
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
    public void GenerateRelationships() {
        int maxInitialRels = 4;
        RELATIONSHIP_TRAIT[] relsInOrder = new RELATIONSHIP_TRAIT[] { RELATIONSHIP_TRAIT.RELATIVE, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.ENEMY, RELATIONSHIP_TRAIT.FRIEND };

        // Loop through all characters in the world
        for (int i = 0; i < allCharacters.Count; i++) {
            Character currCharacter = allCharacters[i];
#if TRAILER_BUILD
            if (currCharacter.name == "Jamie" || currCharacter.name == "Audrey" || currCharacter.name == "Fiona") {
                continue; //skip main cast (For Trailer Only)
            }
#endif
            if (currCharacter.isFactionless) {
                continue; //skip factionless characters
            }
            int currentRelCount = currCharacter.GetAllRelationshipCountExcept(new List<RELATIONSHIP_TRAIT>() { RELATIONSHIP_TRAIT.MASTER, RELATIONSHIP_TRAIT.SERVANT });
            if (currentRelCount >= maxInitialRels) {
                continue; //skip
            }
            int totalCreatedRels = currentRelCount;
            string summary = currCharacter.name + "(" + currCharacter.sexuality.ToString() + ") relationship generation summary:";

            //  Loop through all relationship types
            for (int k = 0; k < relsInOrder.Length; k++) {
                RELATIONSHIP_TRAIT currRel = relsInOrder[k];
                if (totalCreatedRels >= maxInitialRels) {
                    summary += "\nMax Initial Relationships reached, stopping relationship generation for " + currCharacter.name;
                    break; //stop generating more relationships for this character
                }
                int relsToCreate = 0;
                int chance = Random.Range(0, 100);

                // Compute the number of relations to create per relationship type
                switch (currRel) {
                    case RELATIONSHIP_TRAIT.RELATIVE:
                        if (currCharacter.role.roleType == CHARACTER_ROLE.BEAST) { continue; } //a beast character has no relatives
                        else {
                            //- a non-beast character may have either zero (75%), one (20%) or two (5%) relatives from characters of the same race
                            if (chance < 75)  relsToCreate = 0;
                            else if (chance >= 75 && chance < 95)  relsToCreate = 1;
                            else relsToCreate = 2;
                        }
                        break;
                    case RELATIONSHIP_TRAIT.LOVER:
                        //- a character has a 20% chance to have a lover
                        if (chance < 20) relsToCreate = 1;
                        //relsToCreate = 1;
                        break;
                    case RELATIONSHIP_TRAIT.ENEMY:
                        //- a character may have either zero (75%), one (20%) or two (5%) enemies
                        if (chance < 75) relsToCreate = 0;
                        else if (chance >= 75 && chance < 95) relsToCreate = 1;
                        else relsToCreate = 2;
                        //relsToCreate = 2;
                        break;
                    case RELATIONSHIP_TRAIT.FRIEND:
                        //- a character may have either zero (65%), one (25%) or two (10%) friends
                        if (chance < 65) relsToCreate = 0;
                        else if (chance >= 65 && chance < 90) relsToCreate = 1;
                        else relsToCreate = 2;
                        break;
                    //case RELATIONSHIP_TRAIT.PARAMOUR:
                    //    //- only valid for non-beast characters with lovers
                    //    if (currCharacter.role.roleType == CHARACTER_ROLE.BEAST || currCharacter.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER) == null) { continue; }
                    //    //- a character may have either zero (85%) or one (15%) paramour
                    //    //if (chance < 85) relsToCreate = 0;
                    //    //else relsToCreate = 1;
                    //    relsToCreate = 1;
                    //    break;
                }
                summary += "\n===========Creating " + relsToCreate + " " + currRel.ToString() + " Relationships...==========";

                
                if (relsToCreate > 0) {
                    WeightedFloatDictionary<Character> relWeights = new WeightedFloatDictionary<Character>();
                    // Loop through all characters that are in the same faction as the current character
                    for (int l = 0; l < currCharacter.faction.characters.Count; l++) {
                        Character otherCharacter = currCharacter.faction.characters[l];
#if TRAILER_BUILD
                        if (otherCharacter.name == "Jamie" || otherCharacter.name == "Audrey" || otherCharacter.name == "Fiona") {
                            continue; //skip main cast (For Trailer Only)
                        }
#endif
                        if (currCharacter.id != otherCharacter.id) { //&& currCharacter.faction == otherCharacter.faction
                            List<RELATIONSHIP_TRAIT> existingRelsOfCurrentCharacter = currCharacter.GetAllRelationshipTraitTypesWith(otherCharacter);
                            List<RELATIONSHIP_TRAIT> existingRelsOfOtherCharacter = otherCharacter.GetAllRelationshipTraitTypesWith(currCharacter);
                            //if the current character already has a relationship of the same type with the other character, skip
                            if (existingRelsOfCurrentCharacter != null && existingRelsOfCurrentCharacter.Contains(currRel)) {
                                continue; //skip
                            }
                            float weight = 0;

                            // Compute the weight that determines how likely this character will have the current relationship type with current character
                            switch (currRel) {
                                case RELATIONSHIP_TRAIT.RELATIVE:
                                    if (otherCharacter.role.roleType == CHARACTER_ROLE.BEAST) { continue; } //a beast character has no relatives
                                    else {
                                        if (otherCharacter.specificLocation.id == currCharacter.specificLocation.id) {
                                            // character is in same location: +50 Weight
                                            weight += 50;
                                        } else {
                                            //character is in different location: +10 Weight
                                            weight += 10;
                                        }

                                        if (currCharacter.race != otherCharacter.race) weight *= 0; //character is a different race: Weight x0
                                        if (existingRelsOfCurrentCharacter != null && existingRelsOfCurrentCharacter.Contains(RELATIONSHIP_TRAIT.FRIEND)) {
                                            //Disabled possiblity that relatives can be friends
                                            weight *= 0;
                                        }
                                        if (currCharacter.faction != otherCharacter.faction) {
                                            weight *= 0; //disabled different faction positive relationships
                                        }
                                    }
                                    break;
                                case RELATIONSHIP_TRAIT.LOVER:
                                    if (currCharacter.CanHaveRelationshipWith(currRel, otherCharacter) && otherCharacter.CanHaveRelationshipWith(currRel, currCharacter)) {
                                        if (currCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
                                            //- if non beast, from valid characters, choose based on these weights
                                            if (otherCharacter.specificLocation.id == currCharacter.specificLocation.id) {
                                                //- character is in same location: +500 Weight
                                                weight += 500;
                                            } else {
                                                //- character is in different location: +5 Weight
                                                weight += 5;
                                            }
                                            if (currCharacter.race == otherCharacter.race) {
                                                //- character is the same race: Weight x5
                                                weight *= 5;
                                            }
                                            if (!IsSexuallyCompatible(currCharacter, otherCharacter)) {
                                                //- character is sexually incompatible: Weight x0.1
                                                weight *= 0.05f;
                                            }
                                            if (otherCharacter.role.roleType == CHARACTER_ROLE.BEAST) {
                                                //- character is a beast: Weight x0
                                                weight *= 0;
                                            }
                                            if (existingRelsOfCurrentCharacter != null && existingRelsOfCurrentCharacter.Contains(RELATIONSHIP_TRAIT.RELATIVE)) {
                                                //- character is a relative: Weight x0.1    
                                                weight *= 0.1f;
                                            }
                                            if (currCharacter.faction != otherCharacter.faction) {
                                                weight *= 0; //disabled different faction positive relationships
                                            }
                                        } else {
                                            //- if beast, from valid characters, choose based on these weights
                                            if (otherCharacter.specificLocation.id == currCharacter.specificLocation.id) {
                                                //- character is in same location: +50 Weight
                                                weight += 50;
                                            } else {
                                                // - character is in different location: +5 Weight
                                                weight += 5;
                                            }
                                            if (currCharacter.race != otherCharacter.race) {
                                                //- character is a different race: Weight x0
                                                weight *= 0;
                                            }
                                            if (currCharacter.gender != otherCharacter.gender) {
                                                //- character is the opposite gender: Weight x6
                                                weight *= 6;
                                            }
                                        }
                                    }
                                    break;
                                case RELATIONSHIP_TRAIT.ENEMY:
                                    if (currCharacter.CanHaveRelationshipWith(currRel, otherCharacter) && otherCharacter.CanHaveRelationshipWith(currRel, currCharacter)) {
                                        if (currCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
                                            //- if non beast, from valid characters, choose based on these weights
                                            if (otherCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
                                                // - character is non-beast: +50 Weight
                                                weight += 50;
                                            } else {
                                                //- character is a beast: +5 Weight
                                                weight += 5;
                                            }
                                            if (currCharacter.faction.id == otherCharacter.faction.id) {
                                                //- character is from same faction: Weight x6
                                                weight *= 6;
                                            }
                                            if (currCharacter.race != otherCharacter.race) {
                                                //- character is a different race: Weight x2
                                                weight *= 2;
                                            }

                                            if (existingRelsOfCurrentCharacter != null) {
                                                //DISABLED, because of Ask for Help
                                                //if (existingRels.Contains(RELATIONSHIP_TRAIT.RELATIVE)) {
                                                //    //- character is a relative: Weight x0.5
                                                //    weight *= 0.5f;
                                                //}
                                                if (existingRelsOfCurrentCharacter.Contains(RELATIONSHIP_TRAIT.LOVER)) {
                                                    //- character is a lover: Weight x0
                                                    weight *= 0;
                                                }
                                            }
                                            if (existingRelsOfOtherCharacter != null) {
                                                //- character considers this one as an Enemy: Weight x6
                                                if (existingRelsOfOtherCharacter.Contains(RELATIONSHIP_TRAIT.ENEMY)) {
                                                    weight *= 6;
                                                }
                                                //- character considers this one as a Friend: Weight x0.3
                                                if (existingRelsOfOtherCharacter.Contains(RELATIONSHIP_TRAIT.FRIEND)) {
                                                    weight *= 0.3f;
                                                }
                                            }
                                        } 
                                        //REMOVED: Beast
                                        //else {
                                        //    /*
                                        //- if beast, from valid characters, choose based on these weights
                                        //   - character is non-beast: +10 Weight
                                        //   - character is a beast: +50 Weight
                                        //   - character is a relative: Weight x0
                                        //   - character is a different race: Weight x2
                                        // */
                                        //    if (otherCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
                                        //        weight += 10;
                                        //    } else {
                                        //        weight += 50;
                                        //    }
                                        //    if (existingRels != null) {
                                        //        if (existingRels.Contains(RELATIONSHIP_TRAIT.RELATIVE)) {
                                        //            weight *= 0;
                                        //        }
                                        //    }
                                        //    if (currCharacter.race != otherCharacter.race) {
                                        //        weight *= 2;
                                        //    }
                                        //}
                                    }
                                    break;
                                case RELATIONSHIP_TRAIT.FRIEND:
                                    if (currCharacter.CanHaveRelationshipWith(currRel, otherCharacter) && otherCharacter.CanHaveRelationshipWith(currRel, currCharacter)) {
                                        if (currCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
                                            //- if non beast, from valid characters, choose based on these weights:
                                            if (otherCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
                                                //- character is non-beast: +50 Weight
                                                weight += 50;
                                            } else {
                                                //- character is a beast: +5 Weight
                                                weight += 5;
                                            }
                                            if (currCharacter.faction.id == otherCharacter.faction.id) {
                                                // - character is from same faction: Weight x6
                                                weight *= 6;
                                            }
                                            if (currCharacter.race == otherCharacter.race) {
                                                //- character is from same race: Weight x2
                                                weight *= 2;
                                            }
                                            if (existingRelsOfCurrentCharacter != null) {
                                                if (existingRelsOfCurrentCharacter.Contains(RELATIONSHIP_TRAIT.RELATIVE)
                                                    || existingRelsOfCurrentCharacter.Contains(RELATIONSHIP_TRAIT.LOVER)
                                                    || existingRelsOfCurrentCharacter.Contains(RELATIONSHIP_TRAIT.ENEMY)) {
                                                    //- character is a relative: Weight x0
                                                    //- character is a lover: Weight x0
                                                    //- this one considers the character an enemy: Weight x0
                                                    weight *= 0;
                                                }
                                            }
                                            if (existingRelsOfOtherCharacter != null) {
                                                //- character considers this one as an Enemy: Weight x0.3
                                                if (existingRelsOfOtherCharacter.Contains(RELATIONSHIP_TRAIT.ENEMY)) {
                                                    weight *= 0.3f;
                                                }
                                                //- character considers this one as a Friend: Weight x6
                                                if (existingRelsOfOtherCharacter.Contains(RELATIONSHIP_TRAIT.FRIEND)) {
                                                    weight *= 6;
                                                }
                                            }
                                            if (currCharacter.faction != otherCharacter.faction) {
                                                weight *= 0; //disabled different faction positive relationships
                                            }
                                        }
                                    }
                                        //REMOVED : Beast
                                    //    else {
                                    //        /*
                                    //    - if beast, from valid characters, choose based on these weights
                                    //       - character is beast: +50 Weight
                                    //       - character is non-beast: +5 Weight
                                    //       - character is from same faction: Weight x4
                                    //       - character is from same race: Weight x2
                                    //       - character is a relative: Weight x0
                                    //       - character is a lover: Weight x0
                                    //       - character is a master/servant: Weight x0
                                    //       - character is an enemy: Weight x0
                                    //     */
                                    //        if (otherCharacter.role.roleType == CHARACTER_ROLE.BEAST) {
                                    //            weight += 50;
                                    //        } else {
                                    //            weight += 5;
                                    //        }

                                    //        if (currCharacter.faction.id == otherCharacter.faction.id) {
                                    //            weight *= 4;
                                    //        }
                                    //        if (currCharacter.race == otherCharacter.race) {
                                    //            weight *= 2;
                                    //        }

                                    //        if (existingRels != null) {
                                    //            if (existingRels.Contains(RELATIONSHIP_TRAIT.RELATIVE)
                                    //                || existingRels.Contains(RELATIONSHIP_TRAIT.LOVER)
                                    //                || existingRels.Contains(RELATIONSHIP_TRAIT.MASTER)
                                    //                || existingRels.Contains(RELATIONSHIP_TRAIT.SERVANT)
                                    //                || existingRels.Contains(RELATIONSHIP_TRAIT.ENEMY)) {
                                    //                weight *= 0;
                                    //            }
                                    //        }
                                    //    }
                                    //}
                                    break;
                                //case RELATIONSHIP_TRAIT.PARAMOUR:
                                //    if (otherCharacter.role.roleType == CHARACTER_ROLE.BEAST) { continue; } //a beast character has no paramours
                                //    if (currCharacter.CanHaveRelationshipWith(currRel, otherCharacter) && otherCharacter.CanHaveRelationshipWith(currRel, currCharacter)) {
                                //        //- if non beast, from valid characters, choose based on these weights:
                                //        if (otherCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
                                //            //- character is non-beast: +500 Weight
                                //            weight += 500;
                                //        }
                                //        if (!IsSexuallyCompatible(currCharacter, otherCharacter)) {
                                //            //- character is sexually incompatible: Weight x0.1
                                //            weight *= 0.1f;
                                //        }
                                //        if (currCharacter.race == otherCharacter.race) {
                                //            // - character is from same race: Weight x5
                                //            weight *= 5;
                                //        }
                                //        if (currCharacter.faction.id == otherCharacter.faction.id) {
                                //            //- character is from same faction: Weight x3
                                //            weight *= 3;
                                //        }

                                //        if (existingRelsOfCurrentCharacter != null) {
                                //            if (existingRelsOfCurrentCharacter.Contains(RELATIONSHIP_TRAIT.RELATIVE)) {
                                //                //- character is a relative: Weight x0.1
                                //                weight *= 0.1f;
                                //            }
                                //            if (existingRelsOfCurrentCharacter.Contains(RELATIONSHIP_TRAIT.LOVER)
                                //                || existingRelsOfCurrentCharacter.Contains(RELATIONSHIP_TRAIT.ENEMY)) {
                                //                //- character is a lover: Weight x0
                                //                //- character is an enemy: Weight x0
                                //                weight *= 0;
                                //            }
                                //            //REMOVED - character is a master/servant: Weight x0
                                //        }
                                //        if (currCharacter.faction != otherCharacter.faction) {
                                //            weight *= 0; //disabled different faction positive relationships
                                //        }
                                //    }
                                //    break;
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
            Debug.Log(summary);
        }
    }
    public bool IsSexuallyCompatible(Character character1, Character character2) {
        bool sexuallyCompatible = IsSexuallyCompatibleOneSided(character1, character2);
        if (!sexuallyCompatible) {
            return false; //if they are already sexually incompatible in one side, return false
        }
        sexuallyCompatible = IsSexuallyCompatibleOneSided(character2, character1);
        return sexuallyCompatible;
    }
    /// <summary>
    /// Is a character sexually compatible with another.
    /// </summary>
    /// <param name="character1">The character whose sexuality will be taken into account.</param>
    /// <param name="character2">The character that character 1 is checking.</param>
    /// <returns></returns>
    public bool IsSexuallyCompatibleOneSided(Character character1, Character character2) {
        switch (character1.sexuality) {
            case SEXUALITY.STRAIGHT:
                return character1.gender != character2.gender;
            case SEXUALITY.BISEXUAL:
                return true; //because bisexuals are attracted to both genders.
            case SEXUALITY.GAY:
                return character1.gender == character2.gender;
            default:
                return false;
        }
    }
    public bool RelationshipImprovement(Character actor, Character target, GoapAction cause = null) {
        if (actor.returnedToLife || target.returnedToLife) {
            return false; //do not let zombies or skeletons develop other relationships
        }
        string summary = "Relationship improvement between " + actor.name + " and " + target.name;
        bool hasImproved = false;
        Log log = null;
        if (target.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.ENEMY)) {
            //If Actor and Target are Enemies, 25% chance to remove Enemy relationship. If so, Target now considers Actor a Friend.
            summary += "\n" + target.name + " considers " + actor.name + " an enemy. Rolling for chance to consider as a friend...";
            int roll = UnityEngine.Random.Range(0, 100);
            summary += "\nRoll is " + roll.ToString();
            if (roll < 25) {
                log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "enemy_now_friend");
                summary += target.name + " now considers " + actor.name + " an enemy.";
                RemoveOneWayRelationship(target, actor, RELATIONSHIP_TRAIT.ENEMY);
                CreateNewOneWayRelationship(target, actor, RELATIONSHIP_TRAIT.FRIEND);
                hasImproved = true;
            }
        }
        //If character is already a Friend, will not change actual relationship but will consider it improved
        else if (target.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.FRIEND)) {
            hasImproved = true;
        } 
        else if (!target.HasRelationshipWith(actor)) {
            log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "now_friend");
            summary += "\n" + target.name + " has no relationship with " + actor.name + ". " + target.name + " now considers " + actor.name + " a friend.";
            //If Target has no relationship with Actor, Target now considers Actor a Friend.
            CreateNewOneWayRelationship(target, actor, RELATIONSHIP_TRAIT.FRIEND);
            hasImproved = true;
        }
        Debug.Log(summary);
        if (log != null) {
            log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            PlayerManager.Instance.player.ShowNotificationFrom(log, target, actor);
        }
        return hasImproved;
    }
    /// <summary>
    /// Unified way of degrading a relationship of a character with a target character.
    /// </summary>
    /// <param name="actor">The character that did something to degrade the relationship.</param>
    /// <param name="target">The character that will change their relationship with the actor.</param>
    /// <param name="cause">The action that caused of the degradation. Can be null.</param>
    public bool RelationshipDegradation(Character actor, Character target, GoapAction cause = null) {
        return RelationshipDegradation(actor.currentAlterEgo, target, cause);
    }
    public bool RelationshipDegradation(AlterEgoData actorAlterEgo, Character target, GoapAction cause = null) {
        if (actorAlterEgo.owner.returnedToLife || target.returnedToLife) {
            return false; //do not let zombies or skeletons develop other relationships
        }

        bool hasDegraded = false;
        if(actorAlterEgo.owner.isFactionless || target.isFactionless) {
            Debug.LogWarning("Relationship degredation was called and one or both of those characters is factionless");
            return hasDegraded;
        }
        if (actorAlterEgo.owner == target) {
            Debug.LogWarning("Relationship degredation was called and provided same characters " + target.name);
            return hasDegraded;
        }
        if (target.GetNormalTrait("Diplomatic") != null) {
            Debug.LogWarning("Relationship degredation was called but " + target.name + " is Diplomatic");
            return hasDegraded;
        }
        string summary = "Relationship degradation between " + actorAlterEgo.owner.name + " and " + target.name;
        if (cause != null && cause.IsFromApprehendJob()) {
            //If this has been triggered by an Action's End Result that is part of an Apprehend Job, skip processing.
            summary += "Relationship degradation was caused by an action in an apprehend job. Skipping degredation...";
            Debug.Log(summary);
            return hasDegraded;
        }
        //If Actor and Target are Lovers, 25% chance to create a Break Up Job with the Lover.
        if (target.HasRelationshipOfTypeWith(actorAlterEgo, RELATIONSHIP_TRAIT.LOVER)) {
            summary += "\n" + actorAlterEgo.owner.name + " and " + target.name + " are  lovers. Rolling for chance to create break up job...";
            int roll = UnityEngine.Random.Range(0, 100);
            summary += "\nRoll is " + roll.ToString();
            if (roll < 25) {
                summary += "\n" + target.name + " created break up job targetting " + actorAlterEgo.owner.name;
                target.CreateBreakupJob(actorAlterEgo.owner);

                Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "break_up");
                log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(actorAlterEgo.owner, actorAlterEgo.owner.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                PlayerManager.Instance.player.ShowNotificationFrom(log, target, actorAlterEgo.owner);
                hasDegraded = true;
            }
        }
        //If Actor and Target are Paramours, 25% chance to create a Break Up Job with the Paramour.
        else if (target.HasRelationshipOfTypeWith(actorAlterEgo, RELATIONSHIP_TRAIT.PARAMOUR)) {
            summary += "\n" + actorAlterEgo.owner.name + " and " + target.name + " are  paramours. Rolling for chance to create break up job...";
            int roll = UnityEngine.Random.Range(0, 100);
            summary += "\nRoll is " + roll.ToString();
            if (roll < 25) {
                summary += "\n" + target.name + " created break up job targetting " + actorAlterEgo.owner.name;
                target.CreateBreakupJob(actorAlterEgo.owner);

                Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "break_up");
                log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(actorAlterEgo.owner, actorAlterEgo.owner.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                PlayerManager.Instance.player.ShowNotificationFrom(log, target, actorAlterEgo.owner);
                hasDegraded = true;
            }
        }

        //If Target considers Actor a Friend, remove that. If Target is in Bad or Dark Mood, Target now considers Actor an Enemy. Otherwise, they are just no longer friends.
        if (target.HasRelationshipOfTypeWith(actorAlterEgo, RELATIONSHIP_TRAIT.FRIEND)) {
            summary += "\n" + target.name + " considers " + actorAlterEgo.name + " as a friend. Removing friend and replacing with enemy";
            RemoveOneWayRelationship(target, actorAlterEgo, RELATIONSHIP_TRAIT.FRIEND);
            if (target.currentMoodType == CHARACTER_MOOD.BAD || target.currentMoodType == CHARACTER_MOOD.DARK) {
                CreateNewOneWayRelationship(target, actorAlterEgo, RELATIONSHIP_TRAIT.ENEMY);
                Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "friend_now_enemy");
                log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(actorAlterEgo.owner, actorAlterEgo.owner.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                PlayerManager.Instance.player.ShowNotificationFrom(log, target, actorAlterEgo.owner);
                hasDegraded = true;
            } else {
                Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "no_longer_friend");
                log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(actorAlterEgo.owner, actorAlterEgo.owner.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                PlayerManager.Instance.player.ShowNotificationFrom(log, target, actorAlterEgo.owner);
                hasDegraded = true;
            }
        } 
        //If character is already an Enemy, will not change actual relationship but will consider it degraded
        else if (target.HasRelationshipOfTypeWith(actorAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
            hasDegraded = true;
        }
        //If Target is only Relative of Actor(no other relationship) or has no relationship with Actor, Target now considers Actor an Enemy.
        else if (!target.HasRelationshipWith(actorAlterEgo) || (target.HasRelationshipOfTypeWith(actorAlterEgo, RELATIONSHIP_TRAIT.RELATIVE) && target.GetCharacterRelationshipData(actorAlterEgo).rels.Count == 1)) {
            summary += "\n" + target.name + " and " + actorAlterEgo.name + " has no relationship or only has relative relationship. " + target.name + " now considers " + actorAlterEgo.name + " an enemy.";
            CreateNewOneWayRelationship(target, actorAlterEgo, RELATIONSHIP_TRAIT.ENEMY);

            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "now_enemy");
            log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(actorAlterEgo.owner, actorAlterEgo.owner.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            PlayerManager.Instance.player.ShowNotificationFrom(log, target, actorAlterEgo.owner);
            hasDegraded = true;
        }


        Debug.Log(summary);
        return hasDegraded;
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
    public List<string> GetCharacterLogs(Character character) {
        if (allCharacterLogs.ContainsKey(character)) {
            return allCharacterLogs[character];
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

    #region Role
    public CharacterRole GetRoleByRoleType(CHARACTER_ROLE roleType) {
        CharacterRole[] characterRoles = CharacterRole.ALL;
        for (int i = 0; i < characterRoles.Length; i++) {
            if(characterRoles[i].roleType == roleType) {
                return characterRoles[i];
            }
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
}

[System.Serializable]
public class PortraitFrame {
    public Sprite baseBG;
    public Sprite frameOutline;
}

[System.Serializable]
public struct SummonSettings {
    public Sprite summonPortrait;
}

[System.Serializable]
public struct ArtifactSettings {
    public Sprite artifactPortrait;
}
