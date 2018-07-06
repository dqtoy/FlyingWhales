﻿using UnityEngine;
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
    public List<Trait> traitSetup;
    private Dictionary<TRAIT, string> traitDictionary;
    private Dictionary<string, CharacterClass> _classesDictionary;
    private Dictionary<ELEMENT, float> _elementsChanceDictionary;
    private List<Character> _allCharacters;

	public Sprite heroSprite;
	public Sprite villainSprite;
	public Sprite hermitSprite;
	public Sprite beastSprite;
	public Sprite banditSprite;
	public Sprite chieftainSprite;

    [Header("Character Portrait Assets")]
    public GameObject characterPortraitPrefab;
    //public List<HairSetting> hairSettings;
    //public List<Sprite> headSprites;
    //public List<Sprite> noseSprites;
    //public List<Sprite> mouthSprites;
    //public List<Sprite> eyeSprites;
    //public List<Sprite> eyeBrowSprites;
    [SerializeField] private List<PortraitAsset> bodyAssets;
    [SerializeField] private List<PortraitAsset> hairAssets;
    [SerializeField] private List<PortraitAsset> headAssets;
    [SerializeField] private List<PortraitAsset> noseAssets;
    [SerializeField] private List<PortraitAsset> mouthAssets;
    [SerializeField] private List<PortraitAsset> eyeAssets;
    [SerializeField] private List<PortraitAsset> eyebrowAssets;
    [SerializeField] private List<Color> hairColors;

    private Dictionary<IMAGE_SIZE, List<Sprite>> bodySprites;
    private Dictionary<IMAGE_SIZE, List<Sprite>> hairSprites;
    private Dictionary<IMAGE_SIZE, List<Sprite>> headSprites;
    private Dictionary<IMAGE_SIZE, List<Sprite>> noseSprites;
    private Dictionary<IMAGE_SIZE, List<Sprite>> mouthSprites;
    private Dictionary<IMAGE_SIZE, List<Sprite>> eyeSprites;
    private Dictionary<IMAGE_SIZE, List<Sprite>> eyebrowSprites;

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
    public int bodySpriteCount {
        get { return bodySprites[IMAGE_SIZE.X64].Count; }
    }
    public int hairSpriteCount {
        get { return hairSprites[IMAGE_SIZE.X64].Count; }
    }
    public int headSpriteCount {
        get { return headSprites[IMAGE_SIZE.X64].Count; }
    }
    public int noseSpriteCount {
        get { return noseSprites[IMAGE_SIZE.X64].Count; }
    }
    public int mouthSpriteCount {
        get { return mouthSprites[IMAGE_SIZE.X64].Count; }
    }
    public int eyeSpriteCount {
        get { return eyeSprites[IMAGE_SIZE.X64].Count; }
    }
    public int eyebrowSpriteCount {
        get { return eyebrowSprites[IMAGE_SIZE.X64].Count; }
    }
    #endregion

    private void Awake() {
        Instance = this;
        _allCharacters = new List<Character>();
    }

    public void Initialize() {
        ConstructTraitDictionary();
        ConstructAllClasses();
        ConstructElementChanceDictionary();
        ConstructPortraitDictionaries();
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
                    characterFaction.AddNewCharacter(currCharacter);
                    currCharacter.SetFaction(characterFaction);
                    FactionSaveData factionData = data.GetFactionData(characterFaction.id);
                    if (factionData.leaderID != -1 && factionData.leaderID == currCharacter.id) {
                        characterFaction.SetLeader(currCharacter);
                    }
                }
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
    /*
     Create a new character, given a role, class and race.
         */
    public ECS.Character CreateNewCharacter(CHARACTER_ROLE charRole, string className, RACE race, GENDER gender, CHARACTER_JOB job = CHARACTER_JOB.NONE, Faction faction = null) {
		if(className == "None"){
            className = "Classless";
		}
        ECS.CharacterSetup setup = ECS.CombatManager.Instance.GetBaseCharacterSetup(className, race);
        if (setup == null) {
            Debug.LogError("THERE IS NO CLASS WITH THE NAME: " + className + "!");
            return null;
        }
		ECS.Character newCharacter = new ECS.Character(setup, gender);
        if (faction != null) {
            newCharacter.SetFaction(faction);
        }
        if (setup.optionalRole == CHARACTER_ROLE.NONE) {
            if (charRole != CHARACTER_ROLE.NONE) {
                newCharacter.AssignRole(charRole);
            }
        } else {
            newCharacter.AssignRole(setup.optionalRole);
        }

        if (job != CHARACTER_JOB.NONE && newCharacter.role != null) {
            newCharacter.role.AssignJob(job);
        }

        _allCharacters.Add(newCharacter);
        Messenger.Broadcast(Signals.CHARACTER_CREATED, newCharacter);
        return newCharacter;
    }

	public ECS.Character CreateNewCharacter(CHARACTER_ROLE charRole, GENDER gender, CHARACTER_JOB job, ECS.CharacterSetup setup) {
		if (setup == null) {
			return null;
		}
		ECS.Character newCharacter = new ECS.Character(setup, gender);
        if(setup.optionalRole == CHARACTER_ROLE.NONE) {
            if(charRole != CHARACTER_ROLE.NONE) {
                newCharacter.AssignRole(charRole);
            }
        } else {
            newCharacter.AssignRole(setup.optionalRole);
        }

        if (job != CHARACTER_JOB.NONE && newCharacter.role != null) {
            newCharacter.role.AssignJob(job);
        }

#if WORLD_CREATION_TOOL
        CharacterParty party = newCharacter.CreateNewParty();
#endif

        _allCharacters.Add(newCharacter);
        Messenger.Broadcast(Signals.CHARACTER_CREATED, newCharacter);
        return newCharacter;
	}

    /*
     Create a new character, given filename.
         */
    public ECS.Character CreateNewCharacter(string fileName, GENDER gender, CHARACTER_JOB job = CHARACTER_JOB.NONE, Faction faction = null) {
        ECS.CharacterSetup setup = ECS.CombatManager.Instance.GetBaseCharacterSetup(fileName);
        if (setup == null) {
            Debug.LogError("THERE IS NO CLASS WITH THE NAME: " + fileName + "!");
            return null;
        }
        ECS.Character newCharacter = new ECS.Character(setup, gender);

        if (faction != null) {
            newCharacter.SetFaction(faction);
        }
        if (setup.optionalRole != CHARACTER_ROLE.NONE) {
            newCharacter.AssignRole(setup.optionalRole);
        }

        if (job != CHARACTER_JOB.NONE && newCharacter.role != null) {
            newCharacter.role.AssignJob(job);
        }
        _allCharacters.Add(newCharacter);
        Messenger.Broadcast(Signals.CHARACTER_CREATED, newCharacter);
        return newCharacter;
    }
    public ECS.Character CreateNewCharacter(CharacterSaveData data) {
        ECS.Character newCharacter = new ECS.Character(data);        
        newCharacter.AssignRole(data.role);
        if (newCharacter.role != null && data.job != CHARACTER_JOB.NONE) {
            newCharacter.role.AssignJob(data.job);
        }
        if (data.homeID != -1) {
            Area homeLocation = LandmarkManager.Instance.GetAreaByID(data.homeID);
            newCharacter.SetHome(homeLocation);
            //homeLocation.AddCharacterHomeOnLandmark(newCharacter);
        }
        CharacterParty party = newCharacter.CreateNewParty();
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
            newCharacter.SetHomeStructure(homeLandmark.landmarkObj);
#endif
        }

        if (data.equipmentData != null) {
            for (int i = 0; i < data.equipmentData.Count; i++) {
                string equipmentName = data.equipmentData[i];
                Item currItem = ItemManager.Instance.CreateNewItemInstance(equipmentName);
                newCharacter.EquipItem(currItem);
            }
        }

        if (data.inventoryData != null) {
            for (int i = 0; i < data.inventoryData.Count; i++) {
                string itemName = data.inventoryData[i];
                Item currItem = ItemManager.Instance.CreateNewItemInstance(itemName);
                newCharacter.PickupItem(currItem);
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

    #region Traits
    internal Trait CreateNewTraitForCharacter(TRAIT traitType, ECS.Character character) {
        if(traitDictionary == null) {
            ConstructTraitDictionary();
        }
        Trait createdTrait = null;
        switch (traitType) {
            case TRAIT.IMPERIALIST:
                createdTrait = JsonUtility.FromJson<Imperialist>(traitDictionary[traitType]);
                break;
            case TRAIT.HOSTILE:
                createdTrait = JsonUtility.FromJson<Hostile>(traitDictionary[traitType]);
                break;
            case TRAIT.PACIFIST:
                createdTrait = JsonUtility.FromJson<Pacifist>(traitDictionary[traitType]);
                break;
            case TRAIT.SCHEMING:
                createdTrait = JsonUtility.FromJson<Scheming>(traitDictionary[traitType]);
                break;
            case TRAIT.OPPORTUNIST:
                createdTrait = JsonUtility.FromJson<Opportunist>(traitDictionary[traitType]);
                break;
            case TRAIT.EFFICIENT:
                createdTrait = JsonUtility.FromJson<Efficient>(traitDictionary[traitType]);
                break;
            case TRAIT.INEPT:
                createdTrait = JsonUtility.FromJson<Inept>(traitDictionary[traitType]);
                break;
            case TRAIT.MEDDLER:
                createdTrait = JsonUtility.FromJson<Meddler>(traitDictionary[traitType]);
                break;
            case TRAIT.SMART:
                createdTrait = JsonUtility.FromJson<Smart>(traitDictionary[traitType]);
                break;
            case TRAIT.DUMB:
                createdTrait = JsonUtility.FromJson<Dumb>(traitDictionary[traitType]);
                break;
            case TRAIT.CHARISMATIC:
                createdTrait = JsonUtility.FromJson<Charismatic>(traitDictionary[traitType]);
                break;
            case TRAIT.REPULSIVE:
                createdTrait = JsonUtility.FromJson<Repulsive>(traitDictionary[traitType]);
                break;
            case TRAIT.RUTHLESS:
                createdTrait = JsonUtility.FromJson<Ruthless>(traitDictionary[traitType]);
                break;
            case TRAIT.DECEITFUL:
                createdTrait = JsonUtility.FromJson<Deceitful>(traitDictionary[traitType]);
                break;
            case TRAIT.BENEVOLENT:
                createdTrait = JsonUtility.FromJson<Benevolent>(traitDictionary[traitType]);
                break;
            case TRAIT.DIPLOMATIC:
                createdTrait = JsonUtility.FromJson<Diplomatic>(traitDictionary[traitType]);
                break;
            case TRAIT.DEFENSIVE:
                createdTrait = JsonUtility.FromJson<Defensive>(traitDictionary[traitType]);
                break;
            case TRAIT.HONEST:
                createdTrait = JsonUtility.FromJson<Honest>(traitDictionary[traitType]);
                break;
            case TRAIT.RACIST:
                createdTrait = JsonUtility.FromJson<Racist>(traitDictionary[traitType]);
                break;
            case TRAIT.ROBUST:
                createdTrait = JsonUtility.FromJson<Robust>(traitDictionary[traitType]);
                break;
            case TRAIT.FRAGILE:
                createdTrait = JsonUtility.FromJson<Fragile>(traitDictionary[traitType]);
                break;
            case TRAIT.STRONG:
                createdTrait = JsonUtility.FromJson<Strong>(traitDictionary[traitType]);
                break;
            case TRAIT.WEAK:
                createdTrait = JsonUtility.FromJson<Weak>(traitDictionary[traitType]);
                break;
            case TRAIT.CLUMSY:
                createdTrait = JsonUtility.FromJson<Clumsy>(traitDictionary[traitType]);
                break;
            case TRAIT.AGILE:
                createdTrait = JsonUtility.FromJson<Agile>(traitDictionary[traitType]);
                break;
            default:
                break;
        }
        //if (character != null && createdTrait != null) {
        //    createdTrait.AssignCitizen(citizen);
        //}
        return createdTrait;
    }
    internal Trait GetTrait(TRAIT trait) {
        for (int i = 0; i < traitSetup.Count; i++) {
            Trait currTrait = traitSetup[i];
            if (currTrait.trait == trait) {
                return currTrait;
            }
        }
        return null;
    }
    public void ResetTraitSetup() {
        traitSetup.Clear();
        TRAIT[] allTraits = Utilities.GetEnumValues<TRAIT>();
        for (int i = 0; i < allTraits.Length; i++) {
            TRAIT currTrait = allTraits[i];
            string jsonStringOfTrait = GetJsonStringOfTrait(currTrait);
            if (!string.IsNullOrEmpty(jsonStringOfTrait)) {
                Trait traitFromFile = JsonUtility.FromJson<Trait>(jsonStringOfTrait);
                traitSetup.Add(traitFromFile);
            }
        }
    }
#if UNITY_EDITOR
    public void ApplyTraitSetup() {
        for (int i = 0; i < traitSetup.Count; i++) {
            Trait currTrait = traitSetup[i];
            SaveTraitJson(currTrait.traitName, currTrait);
        }
    }
    private void SaveTraitJson(string fileName, Trait traitSetup) {
        string path = Utilities.dataPath + "Traits/" + fileName + ".json";

        string jsonString = JsonUtility.ToJson(traitSetup);

        System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
        writer.WriteLine(jsonString);
        writer.Close();

        //Re-import the file to update the reference in the editor
        UnityEditor.AssetDatabase.ImportAsset(path);
        //TextAsset asset = Resources.Load("Data/Traits/" + fileName + ".json") as TextAsset;

        //Print the text from the file
        Debug.Log(GetJsonStringOfTrait(traitSetup.trait));
    }
#endif
    private string GetJsonStringOfTrait(TRAIT trait) {
        string path = Utilities.dataPath + "Traits/" + Utilities.NormalizeString(trait.ToString()) + ".json";
        string jsonString = string.Empty;
        try {
            //Read the text from directly from the test.txt file
            System.IO.StreamReader reader = new System.IO.StreamReader(path);
            jsonString = reader.ReadToEnd();
            reader.Close();
        } catch {
            //Do nothing
        }
        
        return jsonString;
    }
    internal void ConstructTraitDictionary() {
        traitDictionary = new Dictionary<TRAIT, string>();
        TRAIT[] allTraits = Utilities.GetEnumValues<TRAIT>();
        for (int i = 0; i < allTraits.Length; i++) {
            TRAIT currTrait = allTraits[i];
            string jsonStringOfTrait = GetJsonStringOfTrait(currTrait);
            if (!string.IsNullOrEmpty(jsonStringOfTrait)) {
                traitDictionary.Add(currTrait, jsonStringOfTrait);
            }
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
    public List<Character> GetCharactersWithJob(CHARACTER_JOB job) {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < allCharacters.Count; i++) {
            Character currChar = allCharacters[i];
            if (currChar.role.job != null && currChar.role.job.jobType == job) {
                characters.Add(currChar);
            }
        }
        return characters;
    }
    public bool HasCharacterWithJob(CHARACTER_JOB job) {
        for (int i = 0; i < allCharacters.Count; i++) {
            Character currChar = allCharacters[i];
            if (currChar.role.job != null && currChar.role.job.jobType == job) {
                return true;
            }
        }
        return false;
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
    private void ConstructPortraitDictionaries() {
        bodySprites = new Dictionary<IMAGE_SIZE, List<Sprite>>();
        for (int i = 0; i < bodyAssets.Count; i++) {
            PortraitAsset assets = bodyAssets[i];
            bodySprites.Add(assets.imageSize, assets.assets);
        }
        hairSprites = new Dictionary<IMAGE_SIZE, List<Sprite>>();
        for (int i = 0; i < hairAssets.Count; i++) {
            PortraitAsset assets = hairAssets[i];
            hairSprites.Add(assets.imageSize, assets.assets);
        }
        headSprites = new Dictionary<IMAGE_SIZE, List<Sprite>>();
        for (int i = 0; i < headAssets.Count; i++) {
            PortraitAsset assets = headAssets[i];
            headSprites.Add(assets.imageSize, assets.assets);
        }
        noseSprites = new Dictionary<IMAGE_SIZE, List<Sprite>>();
        for (int i = 0; i < noseAssets.Count; i++) {
            PortraitAsset assets = noseAssets[i];
            noseSprites.Add(assets.imageSize, assets.assets);
        }
        mouthSprites = new Dictionary<IMAGE_SIZE, List<Sprite>>();
        for (int i = 0; i < mouthAssets.Count; i++) {
            PortraitAsset assets = mouthAssets[i];
            mouthSprites.Add(assets.imageSize, assets.assets);
        }
        eyeSprites = new Dictionary<IMAGE_SIZE, List<Sprite>>();
        for (int i = 0; i < eyeAssets.Count; i++) {
            PortraitAsset assets = eyeAssets[i];
            eyeSprites.Add(assets.imageSize, assets.assets);
        }
        eyebrowSprites = new Dictionary<IMAGE_SIZE, List<Sprite>>();
        for (int i = 0; i < eyebrowAssets.Count; i++) {
            PortraitAsset assets = eyebrowAssets[i];
            eyebrowSprites.Add(assets.imageSize, assets.assets);
        }
    }
    public PortraitSettings GenerateRandomPortrait() {
        PortraitSettings ps = new PortraitSettings();
        ps.headIndex = Random.Range(0, headAssets[0].assets.Count);
        ps.eyesIndex = Random.Range(0, eyeAssets[0].assets.Count);
        ps.eyeBrowIndex = Random.Range(0, eyebrowAssets[0].assets.Count);
        ps.hairIndex = Random.Range(0, hairAssets[0].assets.Count);
        ps.noseIndex = Random.Range(0, noseAssets[0].assets.Count);
        ps.mouthIndex = Random.Range(0, mouthAssets[0].assets.Count);
        ps.bodyIndex = Random.Range(0, bodyAssets[0].assets.Count);
        ps.hairColor = hairColors[Random.Range(0, hairColors.Count)];
        return ps;
    }
    public Sprite GetHairSprite(int index,  IMAGE_SIZE imgSize) {
        return hairSprites[imgSize][index];
    }
    public Sprite GetBodySprite(int index, IMAGE_SIZE imgSize) {
        return bodySprites[imgSize][index];
    }
    public Sprite GetHeadSprite(int index, IMAGE_SIZE imgSize) {
        return headSprites[imgSize][index];
    }
    public Sprite GetNoseSprite(int index, IMAGE_SIZE imgSize) {
        return noseSprites[imgSize][index];
    }
    public Sprite GetMouthSprite(int index, IMAGE_SIZE imgSize) {
        return mouthSprites[imgSize][index];
    }
    public Sprite GetEyeSprite(int index, IMAGE_SIZE imgSize) {
        return eyeSprites[imgSize][index];
    }
    public Sprite GetEyebrowSprite(int index, IMAGE_SIZE imgSize) {
        return eyebrowSprites[imgSize][index];
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
}
