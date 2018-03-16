using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class CharacterManager : MonoBehaviour {

    public static CharacterManager Instance = null;

    public List<CharacterType> characterTypes;
    public List<Trait> traitSetup;
    private Dictionary<TRAIT, string> traitDictionary;

    //private Dictionary<MONTH, Dictionary<int, HashSet<Citizen>>> citizenBirthdays;
    //private Dictionary<int, HashSet<Citizen>> citizenAgeTable = new Dictionary<int, HashSet<Citizen>>() {
    //    {9, new HashSet<Citizen>()},
    //    {19, new HashSet<Citizen>()},
    //    {29, new HashSet<Citizen>()},
    //    {39, new HashSet<Citizen>()},
    //    {49, new HashSet<Citizen>()},
    //    {59, new HashSet<Citizen>()},
    //    {69, new HashSet<Citizen>()},
    //    {79, new HashSet<Citizen>()},
    //    {89, new HashSet<Citizen>()},
    //    {99, new HashSet<Citizen>()},
    //    {100, new HashSet<Citizen>()}
    //};

    //private Dictionary<int, int> ageRangeDeathChances = new Dictionary<int, int>() {
    //    {9, 5},
    //    {19, 1},
    //    {29, 15},
    //    {39, 20},
    //    {49, 51},
    //    {59, 100},
    //    {69, 320},
    //    {79, 734},
    //    {89, 1728},
    //    {99, 4000},
    //    {100, 8000},
    //};

    //private HashSet<Citizen> allCitizens;

    #region getters/setters
    //public Dictionary<int, HashSet<Citizen>> elligibleCitizenAgeTable {
    //    get { return citizenAgeTable.Where(x => x.Value.Any()).ToDictionary(x => x.Key, v => v.Value); }
    //}
    #endregion

    private void Awake() {
        Instance = this;
        //allCitizens = new HashSet<Citizen>();
        //citizenBirthdays = new Dictionary<MONTH, Dictionary<int, HashSet<Citizen>>>();
        //Messenger.AddListener("OnDayEnd", AgeCitizens);
        //Messenger.AddListener("OnDayEnd", CheckForDeath);
    }

    //internal void RegisterCitizen(Citizen citizen) {
    //    allCitizens.Add(citizen);
    //    if (!citizenBirthdays.ContainsKey(citizen.birthMonth)) {
    //        citizenBirthdays.Add(citizen.birthMonth, new Dictionary<int, HashSet<Citizen>>());
    //    }
    //    Dictionary<int, HashSet<Citizen>> currMonthDict = citizenBirthdays[citizen.birthMonth];

    //    if (!currMonthDict.ContainsKey(citizen.birthDay)) {
    //        currMonthDict.Add(citizen.birthDay, new HashSet<Citizen>());
    //    }
    //    HashSet<Citizen> citizensWithBday = currMonthDict[citizen.birthDay];
    //    citizensWithBday.Add(citizen);

    //    //Add Citizen to age table
    //    AddCitizenToAgeTable(citizen);
    //}

    //internal void UnregisterCitizen(Citizen citizen) {
    //    allCitizens.Remove(citizen);
        
    //    if (citizenBirthdays.ContainsKey(citizen.birthMonth)) {
    //        if (citizenBirthdays[citizen.birthMonth].ContainsKey(citizen.birthDay)) {
    //            HashSet<Citizen> citizensWithBday = citizenBirthdays[citizen.birthMonth][citizen.birthDay];
    //            citizensWithBday.Remove(citizen);
    //            if (citizensWithBday.Count <= 0) {
    //                citizenBirthdays[citizen.birthMonth].Remove(citizen.birthDay);
    //            }
    //        }
    //    }
    //    //Remove Citizen from age table
    //    RemoveCitizenFromAgeTable(citizen);
    //}

    //protected void AgeCitizens() {
    //    MONTH currMonth = (MONTH)GameManager.Instance.month;
    //    int currDay = GameManager.Instance.days;
    //    if (citizenBirthdays.ContainsKey(currMonth)) {
    //        if (citizenBirthdays[currMonth].ContainsKey(currDay)) {
    //            HashSet<Citizen> citizensToAge = citizenBirthdays[currMonth][currDay];
    //            for (int i = 0; i < citizensToAge.Count; i++) {
    //                Citizen currCitizen = citizensToAge.ElementAt(i);
    //                currCitizen.AdjustAge(1);
    //                RemoveCitizenFromAgeTable(currCitizen);
    //                AddCitizenToAgeTable(currCitizen);
    //            }
    //        }
    //    }
    //}

    //public void AddCitizenToAgeTable(Citizen citizen) {
    //    if(citizen.age >= 100) {
    //        citizenAgeTable[100].Add(citizen);
    //        citizen.SetAgeTableKey(100);
    //    } else {
    //        for (int i = 0; i < citizenAgeTable.Keys.Count; i++) {
    //            int currKey = citizenAgeTable.Keys.ElementAt(i);
    //            if(citizen.age <= currKey) {
    //                citizenAgeTable[currKey].Add(citizen);
    //                citizen.SetAgeTableKey(currKey);
    //                break;
    //            }
    //        }
    //    }
    //}

    //public void RemoveCitizenFromAgeTable(Citizen citizen) {
    //    if(citizen.ageTableKey != -1) {
    //        citizenAgeTable[citizen.ageTableKey].Remove(citizen);
    //        citizen.SetAgeTableKey(-1);
    //    }
    //}

    #region Grim Reaper
    //private void CheckForDeath() {
    //    int numOfRolls = allCitizens.Count / 10;
    //    for (int i = 0; i < numOfRolls; i++) {
    //        //a roll has a 15 out of 20000 chance to trigger a death
    //        if (Random.Range(0, 20000) < 15) {
    //            //Death Triggered
    //            HashSet<Citizen> citizensToChooseFrom = GetCitizensToDieToChooseFrom();
    //            if(citizensToChooseFrom.Count > 0) {
    //                Citizen citizenToDie = citizensToChooseFrom.ElementAt(Random.Range(0, citizensToChooseFrom.Count));
    //                citizenToDie.Death(DEATH_REASONS.ACCIDENT);
    //            }
    //        }
    //    }
    //}
    //private HashSet<Citizen> GetCitizensToDieToChooseFrom() {
    //    int[] elligibleKeys = elligibleCitizenAgeTable.Keys.ToArray();
    //    int totalChance = ageRangeDeathChances.Where(x => elligibleKeys.Contains(x.Key)).Sum(x => x.Value);
    //    int chance = Random.Range(0, totalChance);
    //    int upperBound = 0;
    //    int lowerBound = 0;
    //    for (int j = 0; j < elligibleKeys.Length; j++) {
    //        int currKey = elligibleKeys[j];
    //        upperBound += ageRangeDeathChances[currKey];
    //        if (chance >= lowerBound && chance < upperBound) {
    //            return elligibleCitizenAgeTable[currKey];
    //        }
    //    }
    //    return null;
    //}
    #endregion

    #region ECS.Character Types
    internal CharacterType GetRandomCharacterType() {
        return characterTypes[Random.Range(0, characterTypes.Count)];
    }
    #endregion

    #region Characters
    /*
     Create a new character, given a role, class and race.
         */
	public ECS.Character CreateNewCharacter(CHARACTER_ROLE charRole, string className, RACE race, int statAllocationBonus = 0, Faction faction = null) {
		if(className == "None"){
			className = "Classless";
		}
        ECS.CharacterSetup setup = ECS.CombatPrototypeManager.Instance.GetBaseCharacterSetup(className, race);
        if (setup == null) {
            Debug.LogError("THERE IS NO CLASS WITH THE NAME: " + className + "!");
            return null;
        }
		ECS.Character newCharacter = new ECS.Character(setup, statAllocationBonus);
        if (faction != null) {
            newCharacter.SetFaction(faction);
        }
        if(charRole != CHARACTER_ROLE.NONE) {
            newCharacter.AssignRole(charRole);
        }
        return newCharacter;
    }
	public ECS.Character CreateNewCharacter(CHARACTER_ROLE charRole, ECS.CharacterSetup setup, int statAllocationBonus = 0) {
		if (setup == null) {
			return null;
		}
		ECS.Character newCharacter = new ECS.Character(setup, statAllocationBonus);
		newCharacter.AssignRole(charRole);
		return newCharacter;
	}
    #endregion

    #region Traits
    // internal Trait CreateNewTraitForCitizen(TRAIT traitType, Citizen citizen) {
    //     Trait createdTrait = null;
    //     switch (traitType) {
    //         case TRAIT.OPPORTUNIST:
    //             createdTrait = JsonUtility.FromJson<Opportunist>(traitDictionary[traitType]);
    //             break; 
    //         case TRAIT.DECEITFUL:
    //             createdTrait = JsonUtility.FromJson<Deceitful>(traitDictionary[traitType]);
    //             break; 
    //         case TRAIT.IMPERIALIST:
    //             createdTrait = JsonUtility.FromJson<Imperialist>(traitDictionary[traitType]);
    //             break; 
    //         case TRAIT.HOSTILE:
    //             createdTrait = JsonUtility.FromJson<Hostile>(traitDictionary[traitType]);
    //             break;
    //         case TRAIT.PACIFIST:
    //             createdTrait = JsonUtility.FromJson<Pacifist>(traitDictionary[traitType]);
    //             break;
    //         case TRAIT.SCHEMING:
    //             createdTrait = JsonUtility.FromJson<Scheming>(traitDictionary[traitType]);
    //             break;
    //         case TRAIT.DIPLOMATIC:
    //             createdTrait = JsonUtility.FromJson<Diplomatic>(traitDictionary[traitType]);
    //             break;
    //         case TRAIT.BENEVOLENT:
    //             createdTrait = JsonUtility.FromJson<Benevolent>(traitDictionary[traitType]);
    //             break;
    //case TRAIT.RUTHLESS:
    //	createdTrait = JsonUtility.FromJson<Ruthless>(traitDictionary[traitType]);
    //	break;
    //     }
    //     if(citizen != null && createdTrait != null) {
    //         createdTrait.AssignCitizen(citizen);
    //     }
    //     return createdTrait;
    // }
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
    public void ApplyTraitSetup() {
        for (int i = 0; i < traitSetup.Count; i++) {
            Trait currTrait = traitSetup[i];
            SaveTraitJson(currTrait.traitName, currTrait);
        }
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
    private void SaveTraitJson(string fileName, Trait traitSetup) {
        string path = "Assets/Resources/Data/Traits/" + fileName + ".json";

        string jsonString = JsonUtility.ToJson(traitSetup);

        System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
        writer.WriteLine(jsonString);
        writer.Close();

        //Re-import the file to update the reference in the editor
        UnityEditor.AssetDatabase.ImportAsset(path);
        TextAsset asset = Resources.Load("Data/Traits/" + fileName + ".json") as TextAsset;

        //Print the text from the file
        Debug.Log(GetJsonStringOfTrait(traitSetup.trait));
    }
    private string GetJsonStringOfTrait(TRAIT trait) {
        string path = "Assets/Resources/Data/Traits/" + Utilities.NormalizeString(trait.ToString()) + ".json";
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
    /*
     Create a new relationship between 2 characters,
     then add add a reference to that relationship, to both of the characters.
         */
    public void CreateNewRelationshipBetween(ECS.Character character1, ECS.Character character2) {
        Relationship newRel = new Relationship(character1, character2);
        character1.AddNewRelationship(character2, newRel);
        character2.AddNewRelationship(character1, newRel);
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
	public void SchedulePrisonerConversion(){
		GameDate newSched = GameManager.Instance.Today ();
		newSched.AddDays (7);
		SchedulingManager.Instance.AddEntry (newSched, () => PrisonerConversion ());
	}
	private void PrisonerConversion(){
		int allPrisonersWorldwide = FactionManager.Instance.allFactions.Sum (x => x.settlements.Sum (y => y.prisoners.Count));
		if(allPrisonersWorldwide > 0){
			int chance = UnityEngine.Random.Range (0, 100);
			float value = (float)allPrisonersWorldwide * 0.2f;

			if(chance < value){
				Faction faction = FactionManager.Instance.allFactions [UnityEngine.Random.Range (0, FactionManager.Instance.allFactions.Count)];
				Settlement settlement = faction.settlements [UnityEngine.Random.Range (0, faction.settlements.Count)];
				ECS.Character characterToBeConverted = settlement.prisoners [UnityEngine.Random.Range (0, settlement.prisoners.Count)];
				characterToBeConverted.ConvertToFaction ();
			}
		}

		SchedulePrisonerConversion ();
	}
    #endregion

    #region Utilities
    public List<ECS.Character> GetCharacters(Region region, CharacterTask task) {
        List<ECS.Character> characters = new List<ECS.Character>();
        for (int i = 0; i < region.charactersInRegion.Count; i++) {
            ECS.Character character = region.charactersInRegion[i].mainCharacter;
            if (task.CanMeetRequirements(character)) {
                characters.Add(character);
            }
        }
        return characters;
    }
    public List<ECS.Character> GetCharacters(ILocation location, CharacterTask task) {
        List<ECS.Character> characters = new List<ECS.Character>();
        for (int i = 0; i < location.charactersAtLocation.Count; i++) {
            ECS.Character character = location.charactersAtLocation[i].mainCharacter;
            if (task.CanMeetRequirements(character)) {
                characters.Add(character);
            }
        }
        return characters;
        //List<ECS.Character> filteredCharacters = new List<ECS.Character>();
        //for (int i = 0; i < region.charactersInRegion.Count; i++) {
        //    ECS.Character currCharacter = region.charactersInRegion[i];
        //    if (filter.MeetsRequirements(currCharacter)) {
        //        if (!filteredCharacters.Contains(currCharacter)) {
        //            filteredCharacters.Add(currCharacter);
        //        }
        //    }
        //}
        //return filteredCharacters;
    }
    public void EquipCharacterWithBestGear(Settlement village, ECS.Character character) {
        MATERIAL matForArmor = village.GetMaterialFor(PRODUCTION_TYPE.ARMOR);
        MATERIAL matForWeapon = village.GetMaterialFor(PRODUCTION_TYPE.WEAPON);
        EquipCharacterWithBestAvailableArmor(character, matForArmor, village);
        EquipCharacterWithBestAvailableWeapon(character, matForWeapon, village);
    }
    private void EquipCharacterWithBestAvailableArmor(ECS.Character character, MATERIAL material, Settlement village) {
        foreach (ARMOR_TYPE armorType in ItemManager.Instance.armorTypeData.Keys) {
            TECHNOLOGY neededTech = Utilities.GetTechnologyForEquipment((EQUIPMENT_TYPE)armorType);
            if (village.HasTechnology(neededTech)) {
                string armorName = Utilities.NormalizeString(material.ToString()) + " " + Utilities.NormalizeString(armorType.ToString());
                int armorQuantityToCreate = 1;
                if (armorType == ARMOR_TYPE.BOOT || armorType == ARMOR_TYPE.BRACER) {
                    armorQuantityToCreate = 2; //Create a pair of boots or bracer
                }
                for (int i = 0; i < armorQuantityToCreate; i++) {
                    ECS.Item item = ItemManager.Instance.CreateNewItemInstance(armorName);
                    character.EquipItem(item);
                }
            }
        }
    }
    private void EquipCharacterWithBestAvailableWeapon(ECS.Character character, MATERIAL material, Settlement village) {
        for (int i = 0; i < character.characterClass.allowedWeaponTypes.Count; i++) {
            WEAPON_TYPE weaponType = character.characterClass.allowedWeaponTypes[i];
            TECHNOLOGY neededTech = Utilities.GetTechnologyForEquipment((EQUIPMENT_TYPE)weaponType);
            if (village.HasTechnology(neededTech)) {
                string weaponName = Utilities.NormalizeString(material.ToString()) + " " + Utilities.NormalizeString(weaponType.ToString());
                ECS.Item item = ItemManager.Instance.CreateNewItemInstance(weaponName);
                character.EquipItem(item);
                break;
            }
        }
    }
    #endregion
}
