using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Traits;
using System.IO;
using System;
using Inner_Maps;

public class CharacterManager : MonoBehaviour {

    public static CharacterManager Instance = null;

    [Header("Sub Managers")]
    [SerializeField] private CharacterClassManager classManager;

    public static readonly string[] sevenDeadlySinsClassNames = { "Lust", "Gluttony", "Greed", "Sloth", "Wrath", "Envy", "Pride" };
    public const int MAX_HISTORY_LOGS = 300;
    public const int CHARACTER_MAX_MEMORY = 20;
    public const int FULLNESS_DECREASE_RATE = 380;
    public const int TIREDNESS_DECREASE_RATE = 340;
    public const int HAPPINESS_DECREASE_RATE = 640;
    public const string Original_Alter_Ego = "Original";

    public GameObject characterIconPrefab;
    public Transform characterIconsParent;

    public int maxLevel;
    private List<Character> _allCharacters;
    private List<CharacterAvatar> _allCharacterAvatars;

    public Sprite heroSprite;

    [Header("Character Portrait Assets")]
    [SerializeField] private GameObject _characterPortraitPrefab;
    [SerializeField] private List<RacePortraitAssets> portraitAssets;
    [SerializeField] private RolePortraitFramesDictionary portraitFrames;
    [SerializeField] private StringSpriteDictionary classPortraits;
    public Material hsvMaterial;
    public Material spriteLightingMaterial;

    //TODO: Will move this once other hair assets arrive
    [SerializeField] private Sprite[] maleHairSprite;
    [SerializeField] private Sprite[] femaleHairSprite;

    [Header("Character Marker Assets")]
    [SerializeField] private List<RaceMarkerAsset> markerAssets;
    [SerializeField] private RuntimeAnimatorController baseAnimator;
    public Sprite corpseSprite;

    [Header("Summon Settings")]
    [SerializeField] private SummonSettingDictionary summonSettings;

    [Header("Artifact Settings")]
    [SerializeField] private ArtifactSettingDictionary artifactSettings;

    public Dictionary<Character, List<string>> allCharacterLogs { get; private set; }
    public Dictionary<CHARACTER_ROLE, INTERACTION_TYPE[]> characterRoleInteractions { get; private set; }
    public Dictionary<string, DeadlySin> deadlySins { get; private set; }

    
    private List<string> deadlySinsRotation = new List<string>();

    public int defaultSleepTicks { get; private set; } //how many ticks does a character must sleep per day?
    public SUMMON_TYPE[] summonsPool { get; private set; }

    #region getters/setters
    public List<Character> allCharacters {
        get { return _allCharacters; }
    }
    public GameObject characterPortraitPrefab {
        get { return _characterPortraitPrefab; }
    }
    #endregion

    private void Awake() {
        Instance = this;
        _allCharacters = new List<Character>();
        _allCharacterAvatars = new List<CharacterAvatar>();
        allCharacterLogs = new Dictionary<Character, List<string>>();
    }

    public void Initialize() {
        classManager.Initialize();
        CreateDeadlySinsData();
        defaultSleepTicks = GameManager.Instance.GetTicksBasedOnHour(8);
        summonsPool = new SUMMON_TYPE[] { SUMMON_TYPE.Wolf, SUMMON_TYPE.Golem, SUMMON_TYPE.Incubus, SUMMON_TYPE.Succubus };
        Messenger.AddListener<Character, GoapAction, string>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
    }

    #region Characters
    /*
     Create a new character, given a role, class and race.
         */
    public Character CreateNewCharacter(CharacterRole role, RACE race, GENDER gender, Faction faction = null, 
        Region homeLocation = null, Dwelling homeStructure = null) {
        Character newCharacter = null;
        //if (role == CharacterRole.LEADER) {
        //    //If the role is leader, it must have a faction, so get the data for the class from the faction
        //    newCharacter = new Character(role, faction.initialLeaderClass, race, gender);
        //} else {
        //    newCharacter = new Character(role, race, gender);
        //}
        newCharacter = new Character(role, race, gender);

        //Party party = newCharacter.CreateOwnParty();
        newCharacter.Initialize();
        if (faction != null) {
            if (!faction.JoinFaction(newCharacter)) {
                FactionManager.Instance.friendlyNeutralFaction.JoinFaction(newCharacter);
            }
        }
        else {
            FactionManager.Instance.neutralFaction.JoinFaction(newCharacter);
        }
        newCharacter.ownParty.CreateIcon();
        if(homeLocation != null) {
            newCharacter.ownParty.icon.SetPosition(homeLocation.coreTile.transform.position);
            newCharacter.MigrateHomeTo(homeLocation, homeStructure, false);
            homeLocation.AddCharacterToLocation(newCharacter);
        }
        //newCharacter.AddAwareness(newCharacter);
        newCharacter.CreateInitialTraitsByClass();
        //newCharacter.CreateInitialTraitsByRace();
        AddNewCharacter(newCharacter);
        return newCharacter;
    }
    public Character CreateNewCharacter(CharacterRole role, string className, RACE race, GENDER gender, Faction faction = null, 
        Region homeLocation = null, Dwelling homeStructure = null) {
        Character newCharacter = new Character(role, className, race, gender);
        newCharacter.Initialize();
        if (faction != null) {
            if (!faction.JoinFaction(newCharacter)) {
                FactionManager.Instance.friendlyNeutralFaction.JoinFaction(newCharacter);
            }
        } else {
            FactionManager.Instance.neutralFaction.JoinFaction(newCharacter);
        }
        newCharacter.ownParty.CreateIcon();
        if (homeLocation != null) {
            newCharacter.ownParty.icon.SetPosition(homeLocation.coreTile.transform.position);
            newCharacter.MigrateHomeTo(homeLocation, homeStructure, false);
            homeLocation.AddCharacterToLocation(newCharacter);
        }
        newCharacter.CreateInitialTraitsByClass();
        AddNewCharacter(newCharacter);
        return newCharacter;
    }
    public Character CreateNewCharacter(CharacterRole role, string className, RACE race, GENDER gender, SEXUALITY sexuality, Faction faction = null,
        Region homeLocation = null, Dwelling homeStructure = null) {
        Character newCharacter = new Character(role, className, race, gender, sexuality);
        newCharacter.Initialize();
        if (faction != null) {
            if (!faction.JoinFaction(newCharacter)) {
                FactionManager.Instance.friendlyNeutralFaction.JoinFaction(newCharacter);
            }
        } else {
            FactionManager.Instance.neutralFaction.JoinFaction(newCharacter);
        }
        newCharacter.ownParty.CreateIcon();
        if (homeLocation != null) {
            newCharacter.ownParty.icon.SetPosition(homeLocation.coreTile.transform.position);
            newCharacter.MigrateHomeTo(homeLocation, homeStructure, false);
            homeLocation.AddCharacterToLocation(newCharacter);
        }
        newCharacter.CreateInitialTraitsByClass();
        AddNewCharacter(newCharacter);
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
        newCharacter.ownParty.CreateIcon();

        Region home = null;
        if (data.homeID != -1) {
            home = GridMap.Instance.GetRegionByID(data.homeID);
        }
        Region currRegion = null;
        if (data.currentLocationID != -1) {
            currRegion = GridMap.Instance.GetRegionByID(data.currentLocationID);
        }
        if (currRegion != null) {
            newCharacter.ownParty.icon.SetPosition(currRegion.coreTile.transform.position);
        }
        if (data.isDead) {
            if (home != null) {
                newCharacter.SetHome(home); //keep this data with character to prevent errors
                //home.AssignCharacterToDwellingInArea(newCharacter); //We do not save LocationStructure, so this is only done so that the dead character will not have null issues with homeStructure
            }
            if(currRegion != null) {
                newCharacter.SetRegionLocation(currRegion);
            }
        } else {
            if (home != null) {
                newCharacter.MigrateHomeTo(home, null, false);
            }
            if (currRegion != null) {
                currRegion.AddCharacterToLocation(newCharacter.ownParty.owner, null, false);
            }
        }
        for (int i = 0; i < data.items.Count; i++) {
            data.items[i].Load(newCharacter);
        }

        AddNewCharacter(newCharacter);
        return newCharacter;
    }
    public void AddNewCharacter(Character character) {
        _allCharacters.Add(character);
        Messenger.Broadcast(Signals.CHARACTER_CREATED, character);
    }
    public void RemoveCharacter(Character character) {
        _allCharacters.Remove(character);
        Messenger.Broadcast<Character>(Signals.CHARACTER_REMOVED, character);
    }
    public string GetDeadlySinsClassNameFromRotation() {
        if (deadlySinsRotation.Count == 0) {
            deadlySinsRotation.AddRange(sevenDeadlySinsClassNames);
        }
        string nextClass = deadlySinsRotation[0];
        deadlySinsRotation.RemoveAt(0);
        return nextClass;
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
    //public void GenerateInitialAwareness() {
    //    for (int i = 0; i < allCharacters.Count; i++) {
    //        Character character = allCharacters[i];
    //        character.AddInitialAwareness();
    //    }
    //}
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
            if (character.minion == null) {
                if (UnityEngine.Random.Range(0, 2) == 0) {
                    SPECIAL_TOKEN randomItem = choices[UnityEngine.Random.Range(0, choices.Count)];
                    SpecialToken token = TokenManager.Instance.CreateSpecialToken(randomItem);
                    character.ObtainToken(token);
                }
            }
        }
    }
    #endregion

    #region Character Class Manager
    public string RunCharacterIdlePlan(Character character) {
        return classManager.RunIdlePlanForCharacter(character);
    }
    public CharacterClass CreateNewCharacterClass(string className) {
        return classManager.CreateNewCharacterClass(className);
    }
    public string GetRandomClassByIdentifier(string identifier) {
        if (identifier == "Demon") {
            return GetDeadlySinsClassNameFromRotation();
        }
        return classManager.GetRandomClassByIdentifier(identifier);
    }
    public bool HasCharacterClass(string className) {
        return classManager.classesDictionary.ContainsKey(className);
    }
    public CharacterClass GetCharacterClass(string className) {
        if (HasCharacterClass(className)) {
            return classManager.classesDictionary[className];
        }
        return null;
    }
    public List<CharacterClass> GetNormalCombatantClasses() {
        return classManager.normalCombatantClasses;
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
        AddNewCharacter(newCharacter);
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

        Region currRegion = null;
        if (data.currentLocationID != -1) {
            currRegion = GridMap.Instance.GetRegionByID(data.currentLocationID);
        }
        if (currRegion != null) {
            newCharacter.ownParty.icon.SetPosition(currRegion.coreTile.transform.position);
        }
        if (data.isDead) {
            if(home != null) {
                newCharacter.SetHome(home); //keep this data with character to prevent errors
                //home.AssignCharacterToDwellingInArea(newCharacter); //We do not save LocationStructure, so this is only done so that the dead character will not have null issues with homeStructure
            }
            if(currRegion != null) {
                newCharacter.SetRegionLocation(currRegion);
            }
        } else {
            if (home != null) {
                newCharacter.MigrateHomeTo(home, null, false);
            }
            if (currRegion != null) {
                currRegion.AddCharacterToLocation(newCharacter.ownParty.owner, null, false);
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

        AddNewCharacter(newCharacter);
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
    }
    public PortraitSettings GenerateRandomPortrait(RACE race, GENDER gender, string characterClass) {
        PortraitAssetCollection pac = GetPortraitAssets(race, gender);
        PortraitSettings ps = new PortraitSettings();
        ps.race = race;
        ps.gender = gender;
        if (race == RACE.DEMON) {
            ps.head = -1;
            ps.brows = -1;
            ps.eyes = -1;
            ps.mouth = -1;
            ps.nose = -1;
            ps.hair = -1;
            ps.mustache = -1;
            ps.beard = -1;
            ps.wholeImage = characterClass;
            ps.hairColor = 0f;
            ps.wholeImageColor = UnityEngine.Random.Range(-144f, 144f);
        } else {
            ps.head = Utilities.GetRandomIndexInList(pac.head);
            ps.brows = Utilities.GetRandomIndexInList(pac.brows);
            ps.eyes = Utilities.GetRandomIndexInList(pac.eyes);
            ps.mouth = Utilities.GetRandomIndexInList(pac.mouth);
            ps.nose = Utilities.GetRandomIndexInList(pac.nose);

            if (UnityEngine.Random.Range(0, 100) < 10 && gender != GENDER.FEMALE) { //females have no chance to be bald
                ps.hair = -1; //chance to have no hair
            } else {
                ps.hair = Utilities.GetRandomIndexInList(pac.hair);
            }
            if (UnityEngine.Random.Range(0, 100) < 20) {
                ps.mustache = -1; //chance to have no mustache
            } else {
                ps.mustache = Utilities.GetRandomIndexInList(pac.mustache);
            }
            if (UnityEngine.Random.Range(0, 100) < 10) {
                ps.beard = -1; //chance to have no beard
            } else {
                ps.beard = Utilities.GetRandomIndexInList(pac.beard);
            }
            ps.wholeImage = string.Empty;
            ps.hairColor = UnityEngine.Random.Range(-720f, 720f);
            ps.wholeImageColor = 0f;
        }
        return ps;
    }
    public PortraitFrame GetPortraitFrame(CHARACTER_ROLE role) {
        if (portraitFrames.ContainsKey(role)) {
            return portraitFrames[role];
        }
        throw new System.Exception("There is no frame for role " + role.ToString());
    }
    public Sprite GetWholeImagePortraitSprite(string className) {
        if (classPortraits.ContainsKey(className)) {
            return classPortraits[className];
        }
        return null;
    }
    public bool TryGetPortraitSprite(string identifier, int index, RACE race, GENDER gender, out Sprite sprite) {
        if (index < 0) {
            sprite = null;
            return false;
        }
        PortraitAssetCollection pac = GetPortraitAssets(race, gender);
        List<Sprite> listToUse;
        switch (identifier) {
            case "head":
                listToUse = pac.head;
                break;
            case "brows":
                listToUse = pac.brows;
                break;
            case "eyes":
                listToUse = pac.eyes;
                break;
            case "mouth":
                listToUse = pac.mouth;
                break;
            case "nose":
                listToUse = pac.nose;
                break;
            case "hair":
                listToUse = pac.hair;
                break;
            case "mustache":
                listToUse = pac.mustache;
                break;
            case "beard":
                listToUse = pac.beard;
                break;
            default:
                listToUse = null;
                break;
        }
        if (listToUse != null && listToUse.Count > index) {
            sprite = listToUse[index];
            return true;
        }
        sprite = null;
        return false;
    }
#if UNITY_EDITOR
    public void LoadCharacterPortraitAssets() {
        portraitAssets = new List<RacePortraitAssets>();
        string characterPortraitAssetPath = "Assets/Textures/Portraits/";
        string[] races = Directory.GetDirectories(characterPortraitAssetPath);

        //loop through races found in directory
        for (int i = 0; i < races.Length; i++) {
            string currRacePath = races[i];
            string raceName = new DirectoryInfo(currRacePath).Name.ToUpper();
            RACE race;
            if (System.Enum.TryParse(raceName, out race)) {
                RacePortraitAssets raceAsset = new RacePortraitAssets(race);
                //loop through genders found in races directory
                string[] genders = System.IO.Directory.GetDirectories(currRacePath);
                for (int j = 0; j < genders.Length; j++) {
                    string currGenderPath = genders[j];
                    string genderName = new DirectoryInfo(currGenderPath).Name.ToUpper();
                    GENDER gender;
                    if (System.Enum.TryParse(genderName, out gender)) {
                        PortraitAssetCollection assetCollection = raceAsset.GetPortraitAssetCollection(gender);
                        string[] faceParts = System.IO.Directory.GetDirectories(currGenderPath);
                        for (int k = 0; k < faceParts.Length; k++) {
                            string currFacePath = faceParts[k];
                            string facePartName = new DirectoryInfo(currFacePath).Name;
                            string[] facePartFiles = Directory.GetFiles(currFacePath);
                            for (int l = 0; l < facePartFiles.Length; l++) {
                                string facePartAssetPath = facePartFiles[l];
                                Sprite loadedSprite = (Sprite)UnityEditor.AssetDatabase.LoadAssetAtPath(facePartAssetPath, typeof(Sprite));
                                if (loadedSprite != null) {
                                    assetCollection.AddSpriteToCollection(facePartName, loadedSprite);
                                }
                            }
                        }
                    }
                }
                portraitAssets.Add(raceAsset);
            }
        }
    }
#endif
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

    #region Marker Assets
    public CharacterClassAsset GetMarkerAsset(RACE race, GENDER gender, string characterClassName) {
        for (int i = 0; i < markerAssets.Count; i++) {
            RaceMarkerAsset currRaceAsset = markerAssets[i];
            if (currRaceAsset.race == race) {
                GenderMarkerAsset asset = currRaceAsset.GetMarkerAsset(gender);
                if (asset.characterClassAssets.ContainsKey(characterClassName) == false) {
                    Debug.LogWarning($"There are no class assets for {characterClassName} {gender.ToString()} {race.ToString()}");
                    return null;
                }
                return asset.characterClassAssets[characterClassName];
            }
        }
        Debug.LogWarning($"There are no race assets for {characterClassName} {gender.ToString()} {race.ToString()}");
        return null;
    }
    public Sprite GetMarkerHairSprite(GENDER gender) {
        switch (gender) {
            case GENDER.MALE:
                return maleHairSprite[UnityEngine.Random.Range(0, maleHairSprite.Length)];
            case GENDER.FEMALE:
                return femaleHairSprite[UnityEngine.Random.Range(0, femaleHairSprite.Length)];
            default:
                return null;
        }
    }
#if UNITY_EDITOR
    public void LoadCharacterMarkerAssets() {
        markerAssets = new List<RaceMarkerAsset>();
        string characterMarkerAssetPath = "Assets/Textures/Character Markers/";
        string[] races = Directory.GetDirectories(characterMarkerAssetPath);

        //loop through races found in directory
        for (int i = 0; i < races.Length; i++) {
            string currRacePath = races[i];
            string raceName = new DirectoryInfo(currRacePath).Name.ToUpper();
            RACE race;
            if (System.Enum.TryParse(raceName, out race)) {
                RaceMarkerAsset raceAsset = new RaceMarkerAsset(race);
                //loop through genders found in races directory
                string[] genders = System.IO.Directory.GetDirectories(currRacePath);
                for (int j = 0; j < genders.Length; j++) {
                    string currGenderPath = genders[j];
                    string genderName = new DirectoryInfo(currGenderPath).Name.ToUpper();
                    GENDER gender;
                    if (System.Enum.TryParse(genderName, out gender)) {
                        GenderMarkerAsset markerAsset = raceAsset.GetMarkerAsset(gender);
                        //loop through all folders found in gender directory. consider all these as character classes
                        string[] characterClasses = System.IO.Directory.GetDirectories(currGenderPath);
                        for (int k = 0; k < characterClasses.Length; k++) {
                            string currCharacterClassPath = characterClasses[k];
                            string className = new DirectoryInfo(currCharacterClassPath).Name;
                            string[] classFiles = Directory.GetFiles(currCharacterClassPath);
                            CharacterClassAsset characterClassAsset = new CharacterClassAsset();
                            markerAsset.characterClassAssets.Add(className, characterClassAsset);
                            for (int l = 0; l < classFiles.Length; l++) {
                                string classAssetPath = classFiles[l];
                                Sprite loadedSprite = (Sprite)UnityEditor.AssetDatabase.LoadAssetAtPath(classAssetPath, typeof(Sprite));
                                if (loadedSprite != null) {
                                    if (loadedSprite.name.Contains("_body")) {
                                        characterClassAsset.defaultSprite = loadedSprite;
                                    } else {
                                        //assume that sprite is for animation
                                        characterClassAsset.animationSprites.Add(loadedSprite);
                                    }
                                }

                            }
                        }
                    }
                }
                markerAssets.Add(raceAsset);
            }
        }
    }
#endif
    #endregion

    #region Listeners
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

    #region POI
    public bool POIValueTypeMatching(POIValueType poi1, POIValueType poi2) {
        return poi1.id == poi2.id && poi1.poiType == poi2.poiType && poi1.tileObjectType == poi2.tileObjectType;
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
