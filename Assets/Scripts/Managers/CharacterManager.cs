using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ECS;

public class CharacterManager : MonoBehaviour {

    public static CharacterManager Instance = null;

    public GameObject characterIconPrefab;
    public Transform characterIconsParent;

    public GameObject armyIconPrefab;
    public Transform armyIconsParent;

    public int maxLevel;
    public List<CharacterType> characterTypes;
    private Dictionary<string, CharacterClass> _classesDictionary;
    private Dictionary<ELEMENT, float> _elementsChanceDictionary;
    private List<Character> _allCharacters;

    public List<Squad> allSquads { get; private set; }

	public Sprite heroSprite;
	public Sprite villainSprite;
	public Sprite hermitSprite;
	public Sprite beastSprite;
	public Sprite banditSprite;
	public Sprite chieftainSprite;

    [Header("Character Tag Icons")]
    [SerializeField] private List<CharacterAttributeIconSetting> characterTagIcons;

    [Header("Character Portrait Assets")]
    public GameObject characterPortraitPrefab;
    [SerializeField] private List<RacePortraitAssets> portraitAssetsx64;
    [SerializeField] private List<RacePortraitAssets> portraitAssetsx256;
    public List<Color> hairColors;

    [Header("Character Role Animators")]
    [SerializeField] private RuntimeAnimatorController[] characterAnimators;

    public readonly int HAPPINESS_THRESHOLD = 20;
    public readonly int MENTAL_THRESHOLD = -3;
    public readonly int PHYSICAL_THRESHOLD = -3;

    #region getters/setters
    //public Dictionary<int, HashSet<Citizen>> elligibleCitizenAgeTable {
    //    get { return citizenAgeTable.Where(x => x.Value.Any()).ToDictionary(x => x.Key, v => v.Value); }
    //}
    public Dictionary<string, CharacterClass> classesDictionary {
        get { return _classesDictionary; }
    }
    public List<Character> allCharacters {
        get { return _allCharacters; }
    }
    public Dictionary<ELEMENT, float> elementsChanceDictionary {
        get { return _elementsChanceDictionary; }
    }
    //public int bodySpriteCount {
    //    get { return bodySprites[IMAGE_SIZE.X64].Count; }
    //}
    //public int hairSpriteCount {
    //    get { return hairSprites[IMAGE_SIZE.X64].Count; }
    //}
    //public int headSpriteCount {
    //    get { return headSprites[IMAGE_SIZE.X64].Count; }
    //}
    //public int noseSpriteCount {
    //    get { return noseSprites[IMAGE_SIZE.X64].Count; }
    //}
    //public int mouthSpriteCount {
    //    get { return mouthSprites[IMAGE_SIZE.X64].Count; }
    //}
    //public int eyeSpriteCount {
    //    get { return eyeSprites[IMAGE_SIZE.X64].Count; }
    //}
    //public int eyebrowSpriteCount {
    //    get { return eyebrowSprites[IMAGE_SIZE.X64].Count; }
    //}
    #endregion

    private void Awake() {
        Instance = this;
        _allCharacters = new List<Character>();
        allSquads = new List<Squad>();
    }

    public void Initialize() {
        ConstructAllClasses();
        ConstructElementChanceDictionary();
        //ConstructPortraitDictionaries();
    }

    #region ECS.Character Types
    internal CharacterType GetRandomCharacterType() {
        return characterTypes[Random.Range(0, characterTypes.Count)];
    }
    #endregion

    #region Characters
    public void LoadCharacters(WorldSaveData data) {
        if (data.charactersData != null) {
            for (int i = 0; i < data.charactersData.Count; i++) {
                CharacterSaveData currData = data.charactersData[i];
                ECS.Character currCharacter = CreateNewCharacter(currData);
                Faction characterFaction = FactionManager.Instance.GetFactionBasedOnID(currData.factionID);
                if (characterFaction != null) {
                    currCharacter.SetFaction(characterFaction);
                    characterFaction.AddNewCharacter(currCharacter);
                    FactionSaveData factionData = data.GetFactionData(characterFaction.id);
                    if (factionData.leaderID != -1 && factionData.leaderID == currCharacter.id) {
                        characterFaction.SetLeader(currCharacter);
                    }
                }
#if !WORLD_CREATION_TOOL
                else {
                    characterFaction = FactionManager.Instance.neutralFaction;
                    currCharacter.SetFaction(characterFaction);
                    characterFaction.AddNewCharacter(currCharacter);
                }
#endif
            }
#if WORLD_CREATION_TOOL
            worldcreator.WorldCreatorUI.Instance.editFactionsMenu.UpdateItems();
#endif
        }
    }
    public void LoadRelationships(WorldSaveData data) {
        if (data.charactersData != null) {
            for (int i = 0; i < data.charactersData.Count; i++) {
                CharacterSaveData currData = data.charactersData[i];
                ECS.Character currCharacter = CharacterManager.Instance.GetCharacterByID(currData.id);
                currCharacter.LoadRelationships(currData.relationshipsData);
            }
        }
    }
    public void LoadSquads(WorldSaveData data) {
        if (data.squadData != null) {
            for (int i = 0; i < data.squadData.Count; i++) {
                SquadSaveData currData = data.squadData[i];
                CreateNewSquad(currData);
            }
        }
    
    }
    /*
     Create a new character, given a role, class and race.
         */
    public ECS.Character CreateNewCharacter(CHARACTER_ROLE charRole, string className, RACE race, GENDER gender, Faction faction = null) {
		if(className == "None"){
            className = "Classless";
		}
		ECS.Character newCharacter = new ECS.Character(className, race, gender);
        NewParty party = newCharacter.CreateOwnParty();
        if (faction != null) {
            newCharacter.SetFaction(faction);
        }
        if (newCharacter.role == null && charRole != CHARACTER_ROLE.NONE) {
            newCharacter.AssignRole(charRole);
        }

        _allCharacters.Add(newCharacter);
        Messenger.Broadcast(Signals.CHARACTER_CREATED, newCharacter);
        return newCharacter;
    }
    public ECS.Character CreateNewCharacter(CharacterSaveData data) {
        ECS.Character newCharacter = new ECS.Character(data);        
        newCharacter.AssignRole(data.role);
        if (data.homeID != -1) {
            Area homeLocation = LandmarkManager.Instance.GetAreaByID(data.homeID);
            newCharacter.SetHome(homeLocation);
            //homeLocation.AddCharacterHomeOnLandmark(newCharacter);
        }
        NewParty party = newCharacter.CreateOwnParty();
        if (data.locationID != -1) {
            ILocation currentLocation = LandmarkManager.Instance.GetLocationBasedOnID(data.locationType, data.locationID);
#if !WORLD_CREATION_TOOL
            party.CreateIcon();
            party.icon.SetPosition(currentLocation.tileLocation.transform.position);            
#endif
            if (currentLocation is BaseLandmark) {
                currentLocation.AddCharacterToLocation(party);
            }
#if WORLD_CREATION_TOOL
            else{
                party.SetSpecificLocation(currentLocation);
            }
#endif
        }

        if (data.homeLandmarkID != -1) {
            BaseLandmark homeLandmark = LandmarkManager.Instance.GetLandmarkByID(data.homeLandmarkID);
            newCharacter.SetHomeLandmark(homeLandmark);
#if !WORLD_CREATION_TOOL
            homeLandmark.AddCharacterHomeOnLandmark(newCharacter);
            newCharacter.SetHomeStructure(homeLandmark.landmarkObj);
#endif
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

        _allCharacters.Add(newCharacter);
        Messenger.Broadcast(Signals.CHARACTER_CREATED, newCharacter);
        return newCharacter;
    }
    public void RemoveCharacter(ECS.Character character) {
        _allCharacters.Remove(character);
        Messenger.Broadcast<ECS.Character>(Signals.CHARACTER_REMOVED, character);
    }
    private void ConstructAllClasses() {
        _classesDictionary = new Dictionary<string, CharacterClass>();
        string path = Utilities.dataPath + "CharacterClasses/";
        string[] classes = System.IO.Directory.GetFiles(path, "*.json");
        for (int i = 0; i < classes.Length; i++) {
            CharacterClass currentClass = JsonUtility.FromJson<CharacterClass>(System.IO.File.ReadAllText(classes[i]));
            //CharacterClass currentClass = new CharacterClass();
            currentClass.ConstructSkills();
            _classesDictionary.Add(currentClass.className, currentClass);
        }
    }
    #endregion

    #region Relationships
    public Relationship CreateNewRelationshipTowards(ECS.Character sourceCharacter, ECS.Character targetCharacter) {
        Relationship newRel = new Relationship(sourceCharacter, targetCharacter);
        sourceCharacter.AddNewRelationship(targetCharacter, newRel);
        return newRel;
    }
    /*
     Utility Function for getting the relationship between 2 characters,
     this just adds a checking for data consistency if, the 2 characters have the
     same reference to their relationship.
     NOTE: This is probably more performance intensive because of the additional checking.
     User can opt to use each characters GetRelationshipWith() instead.
         */
    public Relationship GetRelationshipBetween(ECS.Character character1, ECS.Character character2) {
        if(character1 == null || character2 == null) {
            return null;
        }
        Relationship char1Rel = character1.GetRelationshipWith(character2);
        Relationship char2Rel = character2.GetRelationshipWith(character1);
        if(char1Rel == char2Rel) {
            return char1Rel;
        }
        throw new System.Exception(character1.name + " does not have the same relationship object as " + character2.name + "!");
    }
    #endregion

	#region Prisoner Conversion
	//public void SchedulePrisonerConversion(){
	//	GameDate newSched = GameManager.Instance.Today ();
	//	newSched.AddDays (7);
	//	SchedulingManager.Instance.AddEntry (newSched, () => PrisonerConversion ());
	//}
	//private void PrisonerConversion(){
	//	int allPrisonersWorldwide = FactionManager.Instance.allFactions.Sum (x => x.settlements.Sum (y => y.prisoners.Count));
	//	if(allPrisonersWorldwide > 0){
	//		int chance = UnityEngine.Random.Range (0, 100);
	//		float value = (float)allPrisonersWorldwide * 0.2f;

	//		if(chance < value){
	//			Faction faction = FactionManager.Instance.allFactions [UnityEngine.Random.Range (0, FactionManager.Instance.allFactions.Count)];
	//			Settlement settlement = faction.settlements [UnityEngine.Random.Range (0, faction.settlements.Count)];
	//			ECS.Character characterToBeConverted = settlement.prisoners [UnityEngine.Random.Range (0, settlement.prisoners.Count)];
	//			characterToBeConverted.ConvertToFaction ();
	//		}
	//	}

	//	SchedulePrisonerConversion ();
	//}
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
            if (currChar.name.Equals(name)) {
                return currChar;
            }
        }
        return null;
    }
    public void GenerateCharactersForTesting(int number) {
        List<BaseLandmark> allLandmarks = LandmarkManager.Instance.GetAllLandmarks().Where(x => x.owner != null).ToList();
        //List<Settlement> allOwnedSettlements = new List<Settlement>();
        //for (int i = 0; i < FactionManager.Instance.allTribes.Count; i++) {
        //    allOwnedSettlements.AddRange(FactionManager.Instance.allTribes[i].settlements);
        //}
        WeightedDictionary<CHARACTER_ROLE> characterRoleProductionDictionary = LandmarkManager.Instance.GetCharacterRoleProductionDictionary();

        for (int i = 0; i < number; i++) {
            BaseLandmark chosenLandmark = allLandmarks[Random.Range(0, allLandmarks.Count)];
            //WeightedDictionary<CHARACTER_CLASS> characterClassProductionDictionary = LandmarkManager.Instance.GetCharacterClassProductionDictionary(chosenSettlement);

            //CHARACTER_CLASS chosenClass = characterClassProductionDictionary.PickRandomElementGivenWeights();
            CHARACTER_CLASS chosenClass = CHARACTER_CLASS.WARRIOR;
            CHARACTER_ROLE chosenRole = characterRoleProductionDictionary.PickRandomElementGivenWeights();
            ECS.Character newChar = chosenLandmark.CreateNewCharacter(RACE.HUMANS, chosenRole, Utilities.NormalizeString(chosenClass.ToString()), false);
            //Initial Character tags
            newChar.AssignInitialTags();
            //CharacterManager.Instance.EquipCharacterWithBestGear(chosenSettlement, newChar);
        }
    }
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
    public NewParty GetPartyByID(int id) {
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
    public Sprite GetCharacterAttributeSprite(ATTRIBUTE tag) {
        for (int i = 0; i < characterTagIcons.Count; i++) {
            CharacterAttributeIconSetting currSettings = characterTagIcons[i];
            if (currSettings.tag == tag) {
                return currSettings.icon;
            }
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
    #endregion

    #region Character Portraits
    //private void ConstructPortraitDictionaries() {
        //bodySprites = new Dictionary<IMAGE_SIZE, List<Sprite>>();
        //for (int i = 0; i < bodyAssets.Count; i++) {
        //    PortraitAsset assets = bodyAssets[i];
        //    bodySprites.Add(assets.imageSize, assets.assets);
        //}
        //hairSprites = new Dictionary<IMAGE_SIZE, List<Sprite>>();
        //for (int i = 0; i < hairAssets.Count; i++) {
        //    PortraitAsset assets = hairAssets[i];
        //    hairSprites.Add(assets.imageSize, assets.assets);
        //}
        //headSprites = new Dictionary<IMAGE_SIZE, List<Sprite>>();
        //for (int i = 0; i < headAssets.Count; i++) {
        //    PortraitAsset assets = headAssets[i];
        //    headSprites.Add(assets.imageSize, assets.assets);
        //}
        //noseSprites = new Dictionary<IMAGE_SIZE, List<Sprite>>();
        //for (int i = 0; i < noseAssets.Count; i++) {
        //    PortraitAsset assets = noseAssets[i];
        //    noseSprites.Add(assets.imageSize, assets.assets);
        //}
        //mouthSprites = new Dictionary<IMAGE_SIZE, List<Sprite>>();
        //for (int i = 0; i < mouthAssets.Count; i++) {
        //    PortraitAsset assets = mouthAssets[i];
        //    mouthSprites.Add(assets.imageSize, assets.assets);
        //}
        //eyeSprites = new Dictionary<IMAGE_SIZE, List<Sprite>>();
        //for (int i = 0; i < eyeAssets.Count; i++) {
        //    PortraitAsset assets = eyeAssets[i];
        //    eyeSprites.Add(assets.imageSize, assets.assets);
        //}
        //eyebrowSprites = new Dictionary<IMAGE_SIZE, List<Sprite>>();
        //for (int i = 0; i < eyebrowAssets.Count; i++) {
        //    PortraitAsset assets = eyebrowAssets[i];
        //    eyebrowSprites.Add(assets.imageSize, assets.assets);
        //}
    //}

    public PortraitAssetCollection GetPortraitAssets(RACE race, GENDER gender, IMAGE_SIZE imgSize = IMAGE_SIZE.X256) {
        if (imgSize == IMAGE_SIZE.X64) {
            for (int i = 0; i < portraitAssetsx64.Count; i++) {
                RacePortraitAssets racePortraitAssets = portraitAssetsx64[i];
                if (racePortraitAssets.race == race) {
                    if (gender == GENDER.MALE) {
                        return racePortraitAssets.maleAssets;
                    } else {
                        return racePortraitAssets.femaleAssets;
                    }
                }
            }
        } else {
            for (int i = 0; i < portraitAssetsx256.Count; i++) {
                RacePortraitAssets racePortraitAssets = portraitAssetsx256[i];
                if (racePortraitAssets.race == race) {
                    if (gender == GENDER.MALE) {
                        return racePortraitAssets.maleAssets;
                    } else {
                        return racePortraitAssets.femaleAssets;
                    }
                }
            }
        }
        throw new System.Exception("No portraits for " + race.ToString() + " " + gender.ToString());
    }
    public PortraitSettings GenerateRandomPortrait(RACE race, GENDER gender) {
        PortraitAssetCollection pac = GetPortraitAssets(race, gender);
        PortraitSettings ps = new PortraitSettings();
        ps.race = race;
        ps.gender = gender;
        ps.headIndex = Random.Range(0, pac.headAssets.Count);
        ps.eyesIndex = Random.Range(0, pac.eyeAssets.Count);
        ps.eyeBrowIndex = Random.Range(0, pac.eyebrowAssets.Count);
        ps.hairIndex = Random.Range(0, pac.hairAssets.Count);
        ps.noseIndex = Random.Range(0, pac.noseAssets.Count);
        ps.mouthIndex = Random.Range(0, pac.mouthAssets.Count);
        ps.bodyIndex = Random.Range(0, pac.bodyAssets.Count);
        ps.facialHairIndex = Random.Range(0, pac.facialHairAssets.Count);
        ps.hairColor = hairColors[Random.Range(0, hairColors.Count)];
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
    public HairSetting GetHairSprite(int index,  IMAGE_SIZE imgSize, RACE race, GENDER gender) {
        PortraitAssetCollection pac = GetPortraitAssets(race, gender, imgSize);
        return pac.hairAssets[index];
    }
    public Sprite GetBodySprite(int index, IMAGE_SIZE imgSize, RACE race, GENDER gender) {
        PortraitAssetCollection pac = GetPortraitAssets(race, gender, imgSize);
        return pac.bodyAssets[index];
    }
    public Sprite GetFacialHairSprite(int index, IMAGE_SIZE imgSize, RACE race, GENDER gender) {
        PortraitAssetCollection pac = GetPortraitAssets(race, gender, imgSize);
        if (pac.facialHairAssets.Count <= 0) {
            return null;
        }
        return pac.facialHairAssets[index];
    }
    public Sprite GetHeadSprite(int index, IMAGE_SIZE imgSize, RACE race, GENDER gender) {
        PortraitAssetCollection pac = GetPortraitAssets(race, gender, imgSize);
        return pac.headAssets[index];
    }
    public Sprite GetNoseSprite(int index, IMAGE_SIZE imgSize, RACE race, GENDER gender) {
        PortraitAssetCollection pac = GetPortraitAssets(race, gender, imgSize);
        return pac.noseAssets[index];
    }
    public Sprite GetMouthSprite(int index, IMAGE_SIZE imgSize, RACE race, GENDER gender) {
        PortraitAssetCollection pac = GetPortraitAssets(race, gender, imgSize);
        return pac.mouthAssets[index];
    }
    public Sprite GetEyeSprite(int index, IMAGE_SIZE imgSize, RACE race, GENDER gender) {
        PortraitAssetCollection pac = GetPortraitAssets(race, gender, imgSize);
        return pac.eyeAssets[index];
    }
    public Sprite GetEyebrowSprite(int index, IMAGE_SIZE imgSize, RACE race, GENDER gender) {
        PortraitAssetCollection pac = GetPortraitAssets(race, gender, imgSize);
        return pac.eyebrowAssets[index];
    }
    public int GetHairSpriteCount(RACE race, GENDER gender) {
        PortraitAssetCollection pac = GetPortraitAssets(race, gender);
        return pac.hairAssets.Count;
    }
    public int GetBodySpriteCount(RACE race, GENDER gender) {
        PortraitAssetCollection pac = GetPortraitAssets(race, gender);
        return pac.bodyAssets.Count;
    }
    public int GetFacialHairSpriteCount(RACE race, GENDER gender) {
        PortraitAssetCollection pac = GetPortraitAssets(race, gender);
        return pac.facialHairAssets.Count;
    }
    public int GetHeadSpriteCount(RACE race, GENDER gender) {
        PortraitAssetCollection pac = GetPortraitAssets(race, gender);
        return pac.headAssets.Count;
    }
    public int GetNoseSpriteCount(RACE race, GENDER gender) {
        PortraitAssetCollection pac = GetPortraitAssets(race, gender);
        return pac.noseAssets.Count;
    }
    public int GetMouthSpriteCount(RACE race, GENDER gender) {
        PortraitAssetCollection pac = GetPortraitAssets(race, gender);
        return pac.mouthAssets.Count;
    }
    public int GetEyeSpriteCount(RACE race, GENDER gender) {
        PortraitAssetCollection pac = GetPortraitAssets(race, gender);
        return pac.eyeAssets.Count;
    }
    public int GetEyebrowSpriteCount(RACE race, GENDER gender) {
        PortraitAssetCollection pac = GetPortraitAssets(race, gender);
        return pac.eyebrowAssets.Count;
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

    #region Squads
    public Squad CreateNewSquad() {
        Squad newSquad = new Squad();
        AddSquad(newSquad);
        Messenger.Broadcast(Signals.SQUAD_CREATED, newSquad);
        return newSquad;
    }
    public void CreateNewSquad(SquadSaveData data) {
        Squad newSquad = new Squad(data);
        AddSquad(newSquad);
        Messenger.Broadcast(Signals.SQUAD_CREATED, newSquad);
        foreach (KeyValuePair<int, ICHARACTER_TYPE> kvp in data.memberIDs) {
            if (kvp.Value == ICHARACTER_TYPE.CHARACTER) {
                Character character = GetCharacterByID(kvp.Key);
                if (kvp.Key == data.leaderID) {
                    newSquad.SetLeader(character);
                } else {
                    newSquad.AddMember(character);
                }
            }
        }
    }
    public void DeleteSquad(Squad squad) {
        squad.Disband();
        RemoveSquad(squad);
        Messenger.Broadcast(Signals.SQUAD_DELETED, squad);
    }
    public void AddSquad(Squad squad) {
        if (!allSquads.Contains(squad)) {
            allSquads.Add(squad);
        }
    }
    public void RemoveSquad(Squad squad) {
        allSquads.Remove(squad);
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

#if UNITY_EDITOR
    #region Editor
    public void LoadPortraitAssets(IMAGE_SIZE imgSize, string assetsPath) {
        if (imgSize == IMAGE_SIZE.X64) {
            portraitAssetsx64.Clear();
        } else if (imgSize == IMAGE_SIZE.X256) {
            portraitAssetsx256.Clear();
        }

        string[] subdirectories = System.IO.Directory.GetDirectories(assetsPath); //races
        for (int i = 0; i < subdirectories.Length; i++) {
            string fullSubDirPath = subdirectories[i];
            string dirFileName = System.IO.Path.GetFileName(fullSubDirPath);
            RACE currRace = (RACE)System.Enum.Parse(typeof(RACE), dirFileName);
            RacePortraitAssets currRaceAssets = new RacePortraitAssets(currRace);
            string[] genderDirs = System.IO.Directory.GetDirectories(fullSubDirPath);
            for (int j = 0; j < genderDirs.Length; j++) {
                string fullGenderPath = genderDirs[j];
                GENDER currGender = (GENDER)System.Enum.Parse(typeof(GENDER), System.IO.Path.GetFileName(fullGenderPath));
                string[] files = System.IO.Directory.GetFiles(fullGenderPath, "*.png");
                PortraitAssetCollection collectionToUse = currRaceAssets.maleAssets;
                if (currGender == GENDER.FEMALE) {
                    collectionToUse = currRaceAssets.femaleAssets;
                }
                LoadSpritesToList(files, collectionToUse);
            }
            if (imgSize == IMAGE_SIZE.X64) {
                portraitAssetsx64.Add(currRaceAssets);
            } else if (imgSize == IMAGE_SIZE.X256) {
                portraitAssetsx256.Add(currRaceAssets);
            }
        }
    }
    private void LoadSpritesToList(string[] files, PortraitAssetCollection collectionToUse) {
        for (int k = 0; k < files.Length; k++) {
            string fullFilePath = files[k];
            string fileName = System.IO.Path.GetFileName(fullFilePath);
            Sprite currSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(fullFilePath);
            if (fileName.Contains("body")) {
                collectionToUse.bodyAssets.Add(currSprite);
            } else if (fileName.Contains("brow")) {
                collectionToUse.eyebrowAssets.Add(currSprite);
            } else if (fileName.Contains("eye")) {
                collectionToUse.eyeAssets.Add(currSprite);
            } else if (fileName.Contains("face")) {
                collectionToUse.headAssets.Add(currSprite);
            } else if (fileName.Contains("hair")) {
                if (fileName.Contains("b")) {
                    for (int l = 0; l < collectionToUse.hairAssets.Count; l++) {
                        HairSetting hairSetting = collectionToUse.hairAssets[l];
                        string currSpriteID = System.Text.RegularExpressions.Regex.Match(currSprite.name, @"\d+").Value;
                        string currSettingID = System.Text.RegularExpressions.Regex.Match(hairSetting.hairSprite.name, @"\d+").Value;
                        if (currSpriteID.Equals(currSettingID)) {
                            hairSetting.hairBackSprite = currSprite;
                            break;
                        }
                    }
                } else {
                    HairSetting newHair = new HairSetting();
                    newHair.hairSprite = currSprite;
                    collectionToUse.hairAssets.Add(newHair);
                }
            } else if (fileName.Contains("mouth")) {
                collectionToUse.mouthAssets.Add(currSprite);
            } else if (fileName.Contains("nose")) {
                collectionToUse.noseAssets.Add(currSprite);
            } else if (fileName.Contains("beard")) {
                collectionToUse.facialHairAssets.Add(currSprite);
            }
        }
        OrganizeLists(collectionToUse);
    }
    private void OrganizeLists(PortraitAssetCollection collection) {
        collection.bodyAssets = collection.bodyAssets.OrderBy(x => System.Int32.Parse(System.Text.RegularExpressions.Regex.Match(x.name, @"\d+").Value)).ToList();
        collection.eyebrowAssets = collection.eyebrowAssets.OrderBy(x => System.Int32.Parse(System.Text.RegularExpressions.Regex.Match(x.name, @"\d+").Value)).ToList();
        collection.eyeAssets = collection.eyeAssets.OrderBy(x => System.Int32.Parse(System.Text.RegularExpressions.Regex.Match(x.name, @"\d+").Value)).ToList();
        collection.headAssets = collection.headAssets.OrderBy(x => System.Int32.Parse(System.Text.RegularExpressions.Regex.Match(x.name, @"\d+").Value)).ToList();
        collection.hairAssets = collection.hairAssets.OrderBy(x => System.Int32.Parse(System.Text.RegularExpressions.Regex.Match(x.hairSprite.name, @"\d+").Value)).ToList();
        collection.mouthAssets = collection.mouthAssets.OrderBy(x => System.Int32.Parse(System.Text.RegularExpressions.Regex.Match(x.name, @"\d+").Value)).ToList();
        collection.noseAssets = collection.noseAssets.OrderBy(x => System.Int32.Parse(System.Text.RegularExpressions.Regex.Match(x.name, @"\d+").Value)).ToList();
        if (collection.facialHairAssets != null) {
            collection.facialHairAssets = collection.facialHairAssets.OrderBy(x => System.Int32.Parse(System.Text.RegularExpressions.Regex.Match(x.name, @"\d+").Value)).ToList();
        }
    }
    #endregion
#endif

}
