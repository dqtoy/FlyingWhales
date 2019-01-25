using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class CharacterManager : MonoBehaviour {

    public static CharacterManager Instance = null;

    public GameObject characterIconPrefab;
    public Transform characterIconsParent;

    public int maxLevel;
    private Dictionary<string, CharacterClass> _classesDictionary;
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
    [SerializeField] private JobPortraitFramesDictionary portraitFrames;
    [SerializeField] private StringSpriteDictionary demonPortraits;

    [Header("Character Role Animators")]
    [SerializeField] private RuntimeAnimatorController[] characterAnimators;

    [Header("Job Icons")]
    [SerializeField] private JobIconsDictionary jobIcons;

    public Material hsvMaterial;

    public Dictionary<Character, List<string>> allCharacterLogs { get; private set; }
    public Dictionary<INTERACTION_TYPE, int> awayFromHomeInteractionWeights { get; private set; }
    public Dictionary<INTERACTION_TYPE, int> atHomeInteractionWeights { get; private set; }
    private static readonly string[] _sevenDeadlySinsClassNames = { "Lust", "Gluttony", "Greed", "Sloth", "Wrath", "Envy", "Pride" };
    private List<string> deadlySinsRotation = new List<string>();

    #region getters/setters
    public Dictionary<string, CharacterClass> classesDictionary {
        get { return _classesDictionary; }
    }
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
    public void LoadCharactersInfo() {
        for (int i = 0; i < allCharacters.Count; i++) {
            Character currCharacter = allCharacters[i];
            //CheckForHiddenDesire(currCharacter);
            //CheckForIntelActions(currCharacter);
            //CheckForIntelReactions(currCharacter);
            //CheckForSecrets(currCharacter);
        }
    }
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
    public Character CreateNewCharacter(string className, RACE race, GENDER gender, Faction faction = null, Area homeLocation = null, bool generateTraits = true) {
		if(className == "None"){
            className = "Classless";
		}
		Character newCharacter = new Character(className, race, gender);
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
            newCharacter.MigrateHomeTo(homeLocation, false);
            homeLocation.AddCharacterToLocation(party);
        }
#endif
        if (generateTraits) {
            newCharacter.GenerateRandomTraits();
        }
        _allCharacters.Add(newCharacter);
        //CheckForDuplicateIDs(newCharacter);
        Messenger.Broadcast(Signals.CHARACTER_CREATED, newCharacter);
        return newCharacter;
    }
    public Character CreateNewCharacter(CharacterSaveData data) {
        Character newCharacter = new Character(data);
        allCharacterLogs.Add(newCharacter, new List<string>());

        if (data.homeAreaID != -1) {
            Area homeArea = LandmarkManager.Instance.GetAreaByID(data.homeAreaID);
            if (homeArea != null) {
                homeArea.AddResident(newCharacter, true);
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
                currentLocation.AddCharacterToLocation(party);
            }
        }

        if (data.equipmentData != null) {
            for (int i = 0; i < data.equipmentData.Count; i++) {
                string equipmentName = data.equipmentData[i];
                Item currItem = ItemManager.Instance.CreateNewItemInstance(equipmentName);
                if (currItem != null) {
                    newCharacter.EquipItem(currItem);
                }
            }
        }

        if (data.inventoryData != null) {
            for (int i = 0; i < data.inventoryData.Count; i++) {
                string itemName = data.inventoryData[i];
                Item currItem = ItemManager.Instance.CreateNewItemInstance(itemName);
                if (currItem != null) {
                    newCharacter.PickupItem(currItem);
                }
            }
        }

        if (data.level != 0) {
            newCharacter.SetLevel(data.level);
        }

        newCharacter.GenerateRandomTraits();
        _allCharacters.Add(newCharacter);
        //CheckForDuplicateIDs(newCharacter);
        Messenger.Broadcast(Signals.CHARACTER_CREATED, newCharacter);
        return newCharacter;
    }
    public void RemoveCharacter(Character character) {
        _allCharacters.Remove(character);
        Messenger.Broadcast<Character>(Signals.CHARACTER_REMOVED, character);
    }
    private void ConstructAllClasses() {
        _classesDictionary = new Dictionary<string, CharacterClass>();
        string path = Utilities.dataPath + "CharacterClasses/";
        string[] classes = System.IO.Directory.GetFiles(path, "*.json");
        for (int i = 0; i < classes.Length; i++) {
            CharacterClass currentClass = JsonUtility.FromJson<CharacterClass>(System.IO.File.ReadAllText(classes[i]));
            //CharacterClass currentClass = new CharacterClass();
            currentClass.ConstructData();
            _classesDictionary.Add(currentClass.className, currentClass);
        }
    }
    public string GetRandomDeadlySinsClassName() {
        //return "Envy";
        //return "Sloth";
        return _sevenDeadlySinsClassNames[UnityEngine.Random.Range(0, _sevenDeadlySinsClassNames.Length)];
    }
    public string GetDeadlySinsClassNameFromRotation() {
        //return "Envy";
        //return "Sloth";
        //return _sevenDeadlySinsClassNames[UnityEngine.Random.Range(0, _sevenDeadlySinsClassNames.Length)];
        if (deadlySinsRotation.Count == 0) {
            deadlySinsRotation.AddRange(_sevenDeadlySinsClassNames);
        }
        string nextClass = deadlySinsRotation[0];
        deadlySinsRotation.RemoveAt(0);
        return nextClass;
    }
    public bool IsClassADeadlySin(string className) {
        for (int i = 0; i < _sevenDeadlySinsClassNames.Length; i++) {
            if(className == _sevenDeadlySinsClassNames[i]) {
                return true;
            }
        }
        return false;
    }
    public string GetRandomClassName() {
        int random = UnityEngine.Random.Range(0, CharacterManager.Instance.classesDictionary.Count);
        int count = 0;
        foreach (string className in CharacterManager.Instance.classesDictionary.Keys) {
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
    private void CheckForDuplicateIDs(Character createdCharacter) {
        for (int i = 0; i < allCharacters.Count; i++) {
            Character currCharacter = allCharacters[i];
            if (currCharacter != createdCharacter) {
                if (currCharacter.id == createdCharacter.id) {
                    throw new System.Exception(currCharacter.name + " has same id as " + createdCharacter.name);
                }
            }
        }
    }
    #endregion

    #region Relationships
    public void ChangePersonalRelationshipBetweenTwoCharacters(Character character1, Character character2, int amount) {
        if(amount < 0) {
            //negative
            int range = amount * -1;
            for (int i = 0; i < range; i++) {
                Friend friendTrait1 = character1.GetFriendTraitWith(character2);
                Friend friendTrait2 = character2.GetFriendTraitWith(character1);

                Enemy enemyTrait1 = character1.GetEnemyTraitWith(character2);
                Enemy enemyTrait2 = character2.GetEnemyTraitWith(character1);

                if (friendTrait1 != null && friendTrait2 != null) {
                    character1.RemoveTrait(friendTrait1);
                    character2.RemoveTrait(friendTrait2);
                } else if (enemyTrait1 == null && enemyTrait2 == null) {
                    enemyTrait1 = new Enemy(character2);
                    enemyTrait2 = new Enemy(character1);

                    character1.AddTrait(enemyTrait1);
                    character2.AddTrait(enemyTrait2);
                }
            }
        }else if (amount > 0) {
            //positive
            for (int i = 0; i < amount; i++) {
                Friend friendTrait1 = character1.GetFriendTraitWith(character2);
                Friend friendTrait2 = character2.GetFriendTraitWith(character1);

                Enemy enemyTrait1 = character1.GetEnemyTraitWith(character2);
                Enemy enemyTrait2 = character2.GetEnemyTraitWith(character1);

                if (enemyTrait1 != null && enemyTrait2 != null) {
                    character1.RemoveTrait(enemyTrait1);
                    character2.RemoveTrait(enemyTrait2);
                } else if (friendTrait1 == null && friendTrait2 == null) {
                    friendTrait1 = new Friend(character2);
                    friendTrait2 = new Friend(character1);

                    character1.AddTrait(friendTrait1);
                    character2.AddTrait(friendTrait2);
                }
            }
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
		switch(role){
		case CHARACTER_ROLE.HERO:
			return heroSprite;
		case CHARACTER_ROLE.VILLAIN:
			return villainSprite;
        case CHARACTER_ROLE.BEAST:
            return beastSprite;
        case CHARACTER_ROLE.BANDIT:
            return banditSprite;
        case CHARACTER_ROLE.LEADER:
            return chieftainSprite;
            //case CHARACTER_ROLE.HERMIT:
            //	return hermitSprite;
            //case CHARACTER_ROLE.BEAST:
            //	return beastSprite;
            //case CHARACTER_ROLE.BANDIT:
            //	return banditSprite;
            //case CHARACTER_ROLE.CHIEFTAIN:
            //	return chieftainSprite;
        }
        return null;
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
    public PortraitFrame GetPortraitFrame(JOB job) {
        if (portraitFrames.ContainsKey(job)) {
            return portraitFrames[job];
        }
        throw new System.Exception("There is no frame for job " + job.ToString());
    }
    public Sprite GetDemonPortraitSprite(string className) {
        if (demonPortraits.ContainsKey(className)) {
            return demonPortraits[className];
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
    private void ConstructAwayFromHomeInteractionWeights() {
        awayFromHomeInteractionWeights = new Dictionary<INTERACTION_TYPE, int> {
            { INTERACTION_TYPE.MOVE_TO_RETURN_HOME, 100 },
            { INTERACTION_TYPE.FOUND_LUCARETH, 50 },
            { INTERACTION_TYPE.FOUND_BESTALIA, 50 },
            { INTERACTION_TYPE.FOUND_MAGUS, 50 },
            { INTERACTION_TYPE.FOUND_ZIRANNA, 50 },
            { INTERACTION_TYPE.CHANCE_ENCOUNTER, 2 },
            { INTERACTION_TYPE.TRANSFER_HOME, 50 },
        };
    }
    private void ConstructAtHomeInteractionWeights() {
        atHomeInteractionWeights = new Dictionary<INTERACTION_TYPE, int> {
            { INTERACTION_TYPE.CHANCE_ENCOUNTER, 2 },
            { INTERACTION_TYPE.STEAL_ACTION, 10 },
        };
    }
    #endregion

    //#region Squads
    //public Squad CreateNewSquad() {
    //    Squad newSquad = new Squad();
    //    AddSquad(newSquad);
    //    Messenger.Broadcast(Signals.SQUAD_CREATED, newSquad);
    //    return newSquad;
    //}
    //public void CreateNewSquad(SquadSaveData data) {
    //    Squad newSquad = new Squad(data);
    //    AddSquad(newSquad);
    //    Messenger.Broadcast(Signals.SQUAD_CREATED, newSquad);
    //    foreach (KeyValuePair<int, ICHARACTER_TYPE> kvp in data.memberIDs) {
    //        if (kvp.Value == ICHARACTER_TYPE.CHARACTER) {
    //            Character character = GetCharacterByID(kvp.Key);
    //            if (kvp.Key == data.leaderID) {
    //                newSquad.SetLeader(character);
    //            } else {
    //                newSquad.AddMember(character);
    //            }
    //        }
    //    }
    //}
    //public void DeleteSquad(Squad squad) {
    //    squad.Disband();
    //    RemoveSquad(squad);
    //    Messenger.Broadcast(Signals.SQUAD_DELETED, squad);
    //}
    //public void AddSquad(Squad squad) {
    //    if (!allSquads.Contains(squad)) {
    //        allSquads.Add(squad);
    //    }
    //}
    //public void RemoveSquad(Squad squad) {
    //    allSquads.Remove(squad);
    //}
    //#endregion

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
}

[System.Serializable]
public class PortraitFrame {
    public Sprite baseBG;
    public Sprite frameOutline;
}
