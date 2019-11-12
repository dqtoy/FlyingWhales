using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Traits;

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
    public Dictionary<CHARACTER_ROLE, INTERACTION_TYPE[]> characterRoleInteractions { get; private set; }
    public Dictionary<string, CharacterClass> classesDictionary { get; private set; }
    public Dictionary<string, CharacterClass> uniqueClasses { get; private set; }
    public Dictionary<string, CharacterClass> normalClasses { get; private set; }
    public Dictionary<string, CharacterClass> beastClasses { get; private set; }
    public Dictionary<string, CharacterClass> demonClasses { get; private set; }
    public Dictionary<string, Dictionary<string, CharacterClass>> identifierClasses { get; private set; }
    public Dictionary<string, DeadlySin> deadlySins { get; private set; }

    public static readonly string[] sevenDeadlySinsClassNames = { "Lust", "Gluttony", "Greed", "Sloth", "Wrath", "Envy", "Pride" };
    private List<string> deadlySinsRotation = new List<string>();

    public int defaultSleepTicks { get; protected set; } //how many ticks does a character must sleep per day?

    #region getters/setters
    public List<Character> allCharacters {
        get { return _allCharacters; }
    }
    //public Dictionary<ELEMENT, float> elementsChanceDictionary {
    //    get { return _elementsChanceDictionary; }
    //}
    #endregion

    private void Awake() {
        Instance = this;
        _allCharacters = new List<Character>();
        _allCharacterAvatars = new List<CharacterAvatar>();
        allCharacterLogs = new Dictionary<Character, List<string>>();
    }

    public void Initialize() {
        ConstructAllClasses();
        //ConstructElementChanceDictionary();
        CreateDeadlySinsData();
        defaultSleepTicks = GameManager.Instance.GetTicksBasedOnHour(8);
        //ConstructAwayFromHomeInteractionWeights();
        //ConstructAtHomeInteractionWeights();
        //ConstructRoleInteractions();
        //ConstructPortraitDictionaries();
        Messenger.AddListener<GoapAction, GoapActionState>(Signals.ACTION_STATE_SET, OnActionStateSet);
        Messenger.AddListener<Character, GoapAction, string>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
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
                    characterFaction.JoinFaction(currCharacter);
                    //FactionSaveData factionData = data.GetFactionData(characterFaction.id);
                    //if (factionData.leaderID != -1 && factionData.leaderID == currCharacter.id) {
                    //    characterFaction.SetLeader(currCharacter);
                    //}
                }
#if !WORLD_CREATION_TOOL
                else {
                    characterFaction = FactionManager.Instance.neutralFaction;
                    //currCharacter.SetFaction(characterFaction);
                    characterFaction.JoinFaction(currCharacter);
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
        Region homeLocation = null, Dwelling homeStructure = null) {
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
            faction.JoinFaction(newCharacter);
        }
#if !WORLD_CREATION_TOOL
        else {
            FactionManager.Instance.neutralFaction.JoinFaction(newCharacter);
        }
        newCharacter.ownParty.CreateIcon();
        if(homeLocation != null) {
            newCharacter.ownParty.icon.SetPosition(homeLocation.coreTile.transform.position);
            newCharacter.MigrateHomeTo(homeLocation, homeStructure, false);
            homeLocation.AddCharacterToLocation(newCharacter.ownParty.owner);
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
        Region homeLocation = null, Dwelling homeStructure = null) {
        Character newCharacter = new Character(role, className, race, gender);
        newCharacter.Initialize();
        if (faction != null) {
            faction.JoinFaction(newCharacter);
        } else {
            FactionManager.Instance.neutralFaction.JoinFaction(newCharacter);
        }
#if !WORLD_CREATION_TOOL
        newCharacter.ownParty.CreateIcon();
        if (homeLocation != null) {
            newCharacter.ownParty.icon.SetPosition(homeLocation.coreTile.transform.position);
            newCharacter.MigrateHomeTo(homeLocation, homeStructure, false);
            homeLocation.AddCharacterToLocation(newCharacter.ownParty.owner);
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
        if (data.homeRegionID != -1) {
            Region homeRegion = GridMap.Instance.GetRegionByID(data.homeRegionID);
            if (homeRegion != null) {
#if !WORLD_CREATION_TOOL
                newCharacter.ownParty.CreateIcon();
                newCharacter.ownParty.icon.SetPosition(homeRegion.coreTile.transform.position);
                newCharacter.MigrateHomeTo(homeRegion, null, false);
                homeRegion.AddCharacterToLocation(newCharacter.ownParty.owner);
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
            faction.JoinFaction(newCharacter);
            if (data.isFactionLeader) {
                faction.OnlySetLeader(newCharacter);
            }
        }
#if !WORLD_CREATION_TOOL
        newCharacter.ownParty.CreateIcon();

        Region home = null;
        if (data.homeID != -1) {
            home = GridMap.Instance.GetRegionByID(data.homeID);
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
                //home.AssignCharacterToDwellingInArea(newCharacter); //We do not save LocationStructure, so this is only done so that the dead character will not have null issues with homeStructure
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
    public Summon CreateNewSummon(SUMMON_TYPE summonType, Faction faction = null, Region homeLocation = null, Dwelling homeStructure = null) {
        Summon newCharacter = CreateNewSummonClassFromType(summonType) as Summon;
        newCharacter.Initialize();
        if (faction != null) {
            faction.JoinFaction(newCharacter);
        } else {
            FactionManager.Instance.neutralFaction.JoinFaction(newCharacter);
        }
        newCharacter.ownParty.CreateIcon();
        if (homeLocation != null) {
            newCharacter.ownParty.icon.SetPosition(homeLocation.coreTile.transform.position);
            newCharacter.MigrateHomeTo(homeLocation, homeStructure, false);
            homeLocation.AddCharacterToLocation(newCharacter.ownParty.owner);
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
            faction.JoinFaction(newCharacter);
            if (data.isFactionLeader) {
                faction.OnlySetLeader(newCharacter);
            }
        }

        newCharacter.ownParty.CreateIcon();
        Region home = null;
        if (data.homeID != -1) {
            home = GridMap.Instance.GetRegionByID(data.homeID);
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
                //home.AssignCharacterToDwellingInArea(newCharacter); //We do not save LocationStructure, so this is only done so that the dead character will not have null issues with homeStructure
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
        //for (int i = 0; i < data.normalTraits.Count; i++) {
        //    Character responsibleCharacter = null;
        //    Trait trait = data.normalTraits[i].Load(ref responsibleCharacter);
        //    newCharacter.AddTrait(trait, responsibleCharacter);
        //}
        //newCharacter.LoadAllStatsOfCharacter(data);

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

    //#region Elements
    //private void ConstructElementChanceDictionary() {
    //    _elementsChanceDictionary = new Dictionary<ELEMENT, float>();
    //    ELEMENT[] elements = (ELEMENT[]) System.Enum.GetValues(typeof(ELEMENT));
    //    for (int i = 0; i < elements.Length; i++) {
    //        _elementsChanceDictionary.Add(elements[i], 0f);
    //    }
    //}
    //#endregion

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

    #region Listeners
    private void OnActionStateSet(GoapAction action, GoapActionState state) {
        action.actor.marker.UpdateAnimation();

        IPointOfInterest target = null;
        if (action.goapType == INTERACTION_TYPE.MAKE_LOVE) {
            target = (action as MakeLove).targetCharacter;
        } else {
            target = action.poiTarget;
        }
        List<Character> allInVisionCharacters = action.actor.marker.inVisionCharacters;
        //allInVisionCharacters.Add(action.actor);
        if (target is Character) {
            Character targetCharacter = target as Character;
            if (!targetCharacter.isDead) {
                //TODO: FOR PERFORMANCE TESTING!
                allInVisionCharacters = action.actor.marker.inVisionCharacters.Union(targetCharacter.marker.inVisionCharacters).ToList();
            }
        }
        //if(allInVisionCharacters.Count <= 0) {
        //    allInVisionCharacters.AddRange(action.actor.marker.inVisionCharacters);
        //    allInVisionCharacters.Add(action.actor);
        //}
        //if(action.goapType == INTERACTION_TYPE.ASSAULT_CHARACTER) {
        //    Debug.LogError("Check this!");
        //}
        for (int i = 0; i < allInVisionCharacters.Count; i++) {
            Character inVisionChar = allInVisionCharacters[i];
            if (target != inVisionChar && action.actor != inVisionChar) {
                inVisionChar.OnActionStateSet(action, state);
            } else if (inVisionChar is Summon) {
                inVisionChar.OnActionStateSet(action, state);
            }
        }
    }
    private void OnCharacterFinishedAction(Character actor, GoapAction action, string result) {
        actor.marker.UpdateActionIcon();
        actor.marker.UpdateAnimation();

        //for (int i = 0; i < actor.marker.inVisionCharacters.Count; i++) {
        //    Character otherCharacter = actor.marker.inVisionCharacters[i];
        //    //crime system:
        //    //if the other character committed a crime,
        //    //check if that character is in this characters vision 
        //    //and that this character can react to a crime (not in flee or engage mode)
        //    if (action.IsConsideredACrimeBy(otherCharacter)
        //        && action.CanReactToThisCrime(otherCharacter)
        //        && otherCharacter.CanReactToCrime()) {
        //        bool hasRelationshipDegraded = false;
        //        otherCharacter.ReactToCrime(action, ref hasRelationshipDegraded);
        //    }
        //}
    }
    #endregion

    #region Deadly Sins
    private void CreateDeadlySinsData() {
        deadlySins = new Dictionary<string, DeadlySin>();
        for (int i = 0; i < sevenDeadlySinsClassNames.Length; i++) {
            deadlySins.Add(sevenDeadlySinsClassNames[i], CreateNewDeadlySin(sevenDeadlySinsClassNames[i]));
        }
    }
    private DeadlySin CreateNewDeadlySin(string deadlySin) {
        System.Type type = System.Type.GetType(deadlySin);
        if(type != null) {
            DeadlySin sin = System.Activator.CreateInstance(type) as DeadlySin;
            return sin;
        }
        return null;
    }
    public DeadlySin GetDeadlySin(string sinName) {
        if (deadlySins.ContainsKey(sinName)) {
            return deadlySins[sinName];
        }
        return null;
    }
    public bool CanDoDeadlySinAction(string deadlySinName, DEADLY_SIN_ACTION action) {
        return deadlySins[deadlySinName].CanDoDeadlySinAction(action);
    }
    public List<INTERVENTION_ABILITY> Get3RandomResearchInterventionAbilities(DeadlySin deadlySin) {
        List<INTERVENTION_ABILITY> interventionAbilitiesToResearch = new List<INTERVENTION_ABILITY>();
        INTERVENTION_ABILITY_CATEGORY category = deadlySin.GetInterventionAbilityCategory();
        if (category != INTERVENTION_ABILITY_CATEGORY.NONE) {
            List<INTERVENTION_ABILITY> possibleAbilities = PlayerManager.Instance.GetAllInterventionAbilityByCategory(category);
            if (possibleAbilities.Count > 0) {

                int index1 = UnityEngine.Random.Range(0, possibleAbilities.Count);
                interventionAbilitiesToResearch.Add(possibleAbilities[index1]);
                possibleAbilities.RemoveAt(index1);

                int index2 = UnityEngine.Random.Range(0, possibleAbilities.Count);
                interventionAbilitiesToResearch.Add(possibleAbilities[index2]);
                possibleAbilities.RemoveAt(index2);

                interventionAbilitiesToResearch.Add(possibleAbilities[UnityEngine.Random.Range(0, possibleAbilities.Count)]);
            }
        }
        return interventionAbilitiesToResearch;
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
