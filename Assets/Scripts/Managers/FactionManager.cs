using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class FactionManager : MonoBehaviour {

    public static FactionManager Instance = null;

    public List<Faction> allFactions = new List<Faction>();
    public Faction neutralFaction { get; private set; }

    private Faction _zombieFaction;
    

    [Space(10)]
    [Header("Visuals")]
    [SerializeField] private List<FactionEmblemSetting> _factionEmblems;

    [SerializeField] private List<EmblemBG> _emblemBGs;
    [SerializeField] private List<Sprite> _emblemSymbols;
    //[SerializeField] private List<Sprite> usedEmblems = new List<Sprite>();

    private int[] _inventoryTaskWeights = new int[] { 100, 250, 500 };
    private List<int> factionTaskTriggerTicks = new List<int>();

    #region getters
    public List<EmblemBG> emblemBGs {
        get { return _emblemBGs; }
    }
    public List<Sprite> emblemSymbols {
        get { return _emblemSymbols; }
    }
    public List<FactionEmblemSetting> factionEmblems {
        get { return _factionEmblems; }
    }
    public Faction zombieFaction {
        get {
            if (_zombieFaction == null) {
                _zombieFaction = CreateNewFaction(factionName: "Zombies");
            }
            return _zombieFaction;
        }
    }
    #endregion

    private void Awake() {
        Instance = this;
    }

    #region Faction Generation
    public void LoadFactions(WorldSaveData data) {
        if (data.factionsData != null) {
            for (int i = 0; i < data.factionsData.Count; i++) {
                FactionSaveData currData = data.factionsData[i];
                Faction currFaction = CreateNewFaction(currData);
#if WORLD_CREATION_TOOL
                worldcreator.WorldCreatorUI.Instance.editFactionsMenu.OnFactionCreated(currFaction);
            }
            worldcreator.WorldCreatorUI.Instance.editCharactersMenu.characterInfoEditor.LoadFactionDropdownOptions();
#else
            }
#endif
            //LoadAdditionalFactionInfo(data);
        }
#if !WORLD_CREATION_TOOL
        //if (data.HasFactionlessCharacter()) {
            CreateNeutralFaction();
        //}
#endif
    }
    public void CreateNeutralFaction() {
        Faction newFaction = new Faction();
        newFaction.SetName("Neutral");
        newFaction.SetFactionActiveState(false);
        newFaction.SetEmblem(GetFactionEmblem(4));
        allFactions.Add(newFaction);
        SetNeutralFaction(newFaction);
        //CreateRelationshipsForFaction(newFaction);
        //CreateFavorsForFaction(newFaction);
        Messenger.Broadcast(Signals.FACTION_CREATED, newFaction);
    }
    public void SetNeutralFaction(Faction faction) {
        neutralFaction = faction;
    }
    public void RandomizeStartingFactions(WorldSaveData data) {
        string log = "Starting Factions are: ";
        /*
         Upon startup:
        1 Good Major Faction
        1 Evil Major Faction
        2 Minor Factions
         */
        List<Faction> goodMajorChoices = GetFactionsOfMoralityAndSize(MORALITY.GOOD, FACTION_SIZE.MAJOR);
        //List<Faction> evilMajorChoices = GetFactionsOfMoralityAndSize(MORALITY.EVIL, FACTION_SIZE.MAJOR);
        //List<Faction> minorChoices = GetFactionsOfSize(FACTION_SIZE.MINOR);

        Faction goodMajor = goodMajorChoices[Random.Range(0, goodMajorChoices.Count)];
        //Faction evilMajor = evilMajorChoices[Random.Range(0, evilMajorChoices.Count)];
        List<Faction> startFactions = new List<Faction>() { goodMajor }; //

        log += "\nGood Major: " + goodMajor.name;
        //log += "\nEvil Major: " + evilMajor.name;

        //for (int i = 0; i < 1; i++) {
        //    Faction chosenMinorFaction = minorChoices[Random.Range(0, minorChoices.Count)];
        //    minorChoices.Remove(chosenMinorFaction);
        //    startFactions.Add(chosenMinorFaction);
        //    log += "\nMinor Faction: " + chosenMinorFaction.name;
        //}

        for (int i = 0; i < startFactions.Count; i++) {
            Faction faction = startFactions[i];
            if (!faction.isActive) {
                OwnInitialAreasOfFaction(data.areaData, faction);
                //faction.GenerateStartingCitizens(9, 7);
                faction.SetFactionActiveState(true);
            }
        }

        //Debug.Log(log);

       

        ////First random faction
        //int index1 = UnityEngine.Random.Range(0, factions.Count);
        //Faction firstRandomFaction = factions[index1];
        //if (!firstRandomFaction.isActive) {
        //    OwnInitialAreasOfFaction(data.areaData, firstRandomFaction);
        //    firstRandomFaction.GenerateStartingLeader(6);
        //    firstRandomFaction.ownedAreas[0].GenerateStartingFollowers(4);
        //    firstRandomFaction.SetFactionActiveState(true);
        //    factions.RemoveAt(index1);
        //}

        ////Second random faction
        //int index2 = UnityEngine.Random.Range(0, factions.Count);
        //Faction secondRandomFaction = factions[index2];
        //if (!secondRandomFaction.isActive) {
        //    OwnInitialAreasOfFaction(data.areaData, secondRandomFaction);
        //    secondRandomFaction.GenerateStartingLeader(5);
        //    secondRandomFaction.ownedAreas[0].GenerateStartingFollowers(3);
        //    secondRandomFaction.SetFactionActiveState(true);
        //}
    }

    public void GenerateStartingFactionData() {
        Faction[] startingFactions = allFactions.Where(x => x.isActive).ToArray();
        for (int i = 0; i < startingFactions.Length; i++) {
            Faction currFaction = startingFactions[i];
            currFaction.GenerateStartingCitizens(2, 1); //9,7
        }
        /*
        If both Factions are opposing alignment, -1 to relationship.
        If both Factions have shared alignment, +1 to relationship.
        If both Faction's faction leaders have same race, +1 to relationship.
        */
        //Generate Faction Relationships
        for (int i = 0; i < startingFactions.Length; i++) {
            Faction currFaction = startingFactions[i];
            for (int j = i; j < startingFactions.Length; j++) {
                Faction otherFaction = startingFactions[j];
                if (currFaction.id != otherFaction.id) {
                    FactionRelationship rel = currFaction.GetRelationshipWith(otherFaction);
                    if (currFaction.morality == otherFaction.morality) {
                        rel.AdjustRelationshipStatus(1);
                    } else if (currFaction.morality != MORALITY.NEUTRAL
                        && otherFaction.morality != MORALITY.NEUTRAL) {
                        rel.AdjustRelationshipStatus(-1);
                    }
                    if (currFaction.leader.race == otherFaction.leader.race) {
                        rel.AdjustRelationshipStatus(1);
                    }
                }
            }
        }
    }
    private List<Faction> GetFactionsOfMoralityAndSize(MORALITY morality, FACTION_SIZE size) {
        List<Faction> factions = new List<Faction>();
        for (int i = 0; i < allFactions.Count; i++) {
            Faction currFaction = allFactions[i];
            if (currFaction.name != "Neutral") {
                if (currFaction.morality == morality 
                    && currFaction.size == size) {
                    factions.Add(currFaction);
                }
            }
        }
        return factions;
    }
    private List<Faction> GetFactionsOfSize(FACTION_SIZE size) {
        List<Faction> factions = new List<Faction>();
        for (int i = 0; i < allFactions.Count; i++) {
            Faction currFaction = allFactions[i];
            if (currFaction.name != "Neutral") {
                if (currFaction.size == size) {
                    factions.Add(currFaction);
                }
            }
        }
        return factions;
    }
    private void OwnInitialAreasOfFaction(List<AreaSaveData> areaSaveData, Faction faction) {
        if (areaSaveData != null) {
            for (int i = 0; i < areaSaveData.Count; i++) {
                AreaSaveData areaData = areaSaveData[i];
                if (areaData.ownerID != -1) {
                    if (areaData.ownerID == faction.id) {
                        Area newArea = LandmarkManager.Instance.GetAreaByID(areaData.areaID);
                        if (neutralFaction != null) {
                            neutralFaction.RemoveFromOwnedAreas(newArea); //this will add area to the neutral factions owned area list, but the area's owner will still be null
                        }
                        LandmarkManager.Instance.OwnArea(faction, faction.race, newArea);
                    }
                }
            }
        }
    }
    //public void GenerateFactionCharacters() {
    //    for (int i = 0; i < allFactions.Count; i++) {
    //        Faction currFaction = allFactions[i];
    //        for (int j = 0; j < currFaction.ownedLandmarks.Count; j++) {
    //            BaseLandmark currLandmark = currFaction.ownedLandmarks[j];
    //            CreateInitialFactionCharacters(currFaction, currLandmark);
    //        }
    //        //CreateChieftainForFaction(currTribe);
    //    }
    //}
    // /*
    //  Initital tribes should have a chieftain and a village head.
    //      */
    // private void CreateInitialFactionCharacters(Faction faction, BaseLandmark landmark) {
    //     int numOfCharacters = Random.Range(1, 3); //Generate 1 to 3 characters in each Village with civilians, limit class based on technologies known by its Faction.
    //     WeightedDictionary<CHARACTER_ROLE> characterRoleProductionDictionary = LandmarkManager.Instance.GetCharacterRoleProductionDictionary();
    //     for (int i = 0; i < numOfCharacters; i++) {
    //         CHARACTER_CLASS chosenClass = CHARACTER_CLASS.WARRIOR;
    //         CHARACTER_ROLE chosenRole = characterRoleProductionDictionary.PickRandomElementGivenWeights();
    //         RACE randomRace = RACE.HUMANS;
    //         if (Random.Range(0, 2) == 1) {
    //             randomRace = RACE.ELVES;
    //         }
    //Character newChar = landmark.CreateNewCharacter(randomRace, chosenRole, Utilities.NormalizeString(chosenClass.ToString()));
    ////Initial Character tags
    //newChar.AssignInitialTags();
    //     }
    // }
    private void EquipFullArmorSet(MATERIAL materialToUse, Character character){
		if(materialToUse == MATERIAL.NONE){
			return;
		}
		foreach (ARMOR_TYPE armorType in ItemManager.Instance.armorTypeData.Keys) {
			string armorName = Utilities.NormalizeString(materialToUse.ToString()) + " " + Utilities.NormalizeString(armorType.ToString());
			Item item = ItemManager.Instance.CreateNewItemInstance(armorName);
            if(item != null) {
                character.EquipItem(item);
            }
        }
	}
    public Faction CreateNewFaction(bool isPlayerFaction = false, string factionName = "") {
        Faction newFaction = new Faction(isPlayerFaction);
        allFactions.Add(newFaction);
        //CreateRelationshipsForFaction(newFaction);
        //CreateFavorsForFaction(newFaction);
        if (!string.IsNullOrEmpty(factionName)) {
            newFaction.SetName(factionName);
        }
        if (!isPlayerFaction) {
            Messenger.Broadcast(Signals.FACTION_CREATED, newFaction);
        }
        return newFaction;
    }
    public Faction CreateNewFaction(FactionSaveData data) {
        Faction newFaction = new Faction(data);
        allFactions.Add(newFaction);
        LoadRelationshipsForFaction(newFaction, data);
        //LoadFavorsForFaction(newFaction, data);
        Messenger.Broadcast(Signals.FACTION_CREATED, newFaction);
        return newFaction;
    }
    public Faction CreateNewFaction(SaveDataFaction data) {
        Faction newFaction = new Faction(data);
        if(data.name == "Neutral") {
            SetNeutralFaction(newFaction);
        }
        allFactions.Add(newFaction);
        if (!newFaction.isPlayerFaction) {
            Messenger.Broadcast(Signals.FACTION_CREATED, newFaction);
        }
        return newFaction;
    }
    public void DeleteFaction(Faction faction) {
        for (int i = 0; i < faction.ownedAreas.Count; i++) {
            Area ownedArea = faction.ownedAreas[i];
            LandmarkManager.Instance.UnownArea(ownedArea);
        }
        RemoveRelationshipsWith(faction);
        Messenger.Broadcast(Signals.FACTION_DELETED, faction);
        allFactions.Remove(faction);
    }
    private int GetFactionTaskTickTrigger() {
        if(factionTaskTriggerTicks.Count <= 0) {
            for (int i = 1; i <= GameManager.ticksPerTimeInWords; i++) {
                factionTaskTriggerTicks.Add(i);
            }
        }
        int index = UnityEngine.Random.Range(0, factionTaskTriggerTicks.Count);
        int tick = factionTaskTriggerTicks[index];
        factionTaskTriggerTicks.RemoveAt(index);
        return tick;
    }
    //public void OccupyLandmarksInFactionRegions() {
    //    for (int i = 0; i < allFactions.Count; i++) {
    //        Faction currFaction = allFactions[i];
    //        for (int j = 0; j < currFaction.ownedRegions.Count; j++) {
    //            Region currRegion = currFaction.ownedRegions[j];
    //            for (int k = 0; k < currRegion.landmarks.Count; k++) {
    //                BaseLandmark currLandmark = currRegion.landmarks[k];
    //                if (!currLandmark.isOccupied) { //currLandmark is Settlement &&
    //                    currLandmark.OccupyLandmark(currFaction);
    //                }
    //            }
    //        }
    //    }
    //}
    //public void LoadAdditionalFactionInfo(WorldSaveData data) {
    //    for (int i = 0; i < data.factionsData.Count; i++) {
    //        FactionSaveData currData = data.factionsData[i];
    //        Faction faction = GetFactionBasedOnID(currData.factionID);
    //        //LoadRelationshipsForFaction(faction, currData);
    //        //LoadFavorsForFaction(faction, currData);
    //    }
    //}
    #endregion

    #region Emblem
    /*
     * Generate an emblem for a kingdom.
     * This will return a sprite and set that sprite as used.
     * Will return an error if there are no more available emblems.
     * */
    internal FactionEmblemSetting GenerateFactionEmblem(Faction faction) {
        //List<Sprite> emblemsToUse = new List<Sprite>(_emblemSymbols);
        //for (int i = 0; i < emblemsToUse.Count; i++) {
        //    Sprite currSprite = emblemsToUse[i];
        //    if (!usedEmblems.Contains(currSprite)) {
        //        AddEmblemAsUsed(currSprite);
        //        return currSprite;
        //    }
        //}
        return _factionEmblems[Random.Range(0, _factionEmblems.Count)];
        //throw new System.Exception("There are no more emblems for kingdom: " + faction.name);
    }
    //internal Sprite GenerateFactionEmblemBG() {
    //    return _emblemBGs[Random.Range(0, _emblemBGs.Count)];
    //}
    public FactionEmblemSetting GetFactionEmblem(int emblemIndex) {
        return _factionEmblems[emblemIndex];
        //for (int i = 0; i < _emblemBGs.Count; i++) {
        //    EmblemBG currBG = _emblemBGs[i];
        //    if (currBG.id.Equals(emblemID)) {
        //        return currBG;
        //    }
        //}
        //throw new System.Exception("There is no emblem bg with id " + emblemID);
    }
    public int GetFactionEmblemIndex(FactionEmblemSetting emblem) {
        for (int i = 0; i < _factionEmblems.Count; i++) {
            FactionEmblemSetting currSetting = _factionEmblems[i];
            if (currSetting == emblem) {
                return i;
            }
            //foreach (KeyValuePair<int, Sprite> kvp in currSetting.emblems) {
            //    if (kvp.Value.name == emblem.name) {
            //        return i;
            //    }
            //}
        }
        return -1;
    }
    //internal void AddEmblemAsUsed(Sprite emblem) {
    //    if (!usedEmblems.Contains(emblem)) {
    //        usedEmblems.Add(emblem);
    //    } else {
    //        throw new System.Exception("Emblem " + emblem.name + " is already being used!");
    //    }
    //}
    //internal void RemoveEmblemAsUsed(Sprite emblem) {
    //    usedEmblems.Remove(emblem);
    //}
    //public int GetEmblemSymbolIndex(Sprite symbol) {
    //    if (symbol != null) {
    //        for (int i = 0; i < _emblemSymbols.Count; i++) {
    //            Sprite currSymbol = _emblemSymbols[i];
    //            if (currSymbol == symbol) {
    //                return i;
    //            }
    //        }
    //    }
    //    return -1;
    //}
    //public Sprite GetFactionEmblemSymbol(int index) {
    //    return _emblemSymbols[index];
    //}
    #endregion

    #region Characters
    public List<Character> GetAllCharactersOfType(CHARACTER_ROLE role) {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < allFactions.Count; i++) {
            Faction currFaction = allFactions[i];
            characters.AddRange(currFaction.GetCharactersOfType(role));
        }
        return characters;
    }
    public Character GetCharacterByID(int id) {
        for (int i = 0; i < allFactions.Count; i++) {
            Faction currFaction = allFactions[i];
            Character charInFaction = currFaction.GetCharacterByID(id);
            if(charInFaction != null) {
                return charInFaction;
            }
        }
        return null;
    }
    #endregion

    #region Utilities
    public Faction GetFactionBasedOnID(int id) {
        for (int i = 0; i < allFactions.Count; i++) {
            if (allFactions[i].id == id) {
                return allFactions[i];
            }
        }
        return null;
    }
    public Faction GetFactionBasedOnName(string name) {
        for (int i = 0; i < allFactions.Count; i++) {
            if (allFactions[i].name.ToLower() == name.ToLower()) {
                return allFactions[i];
            }
        }
        return null;
    }
    public void TransferCharacter(Character character, Faction faction, Area newHome) {
        if (character.faction != null) {
            character.faction.RemoveCharacter(character);
        }
        faction.AddNewCharacter(character);
        character.MigrateHomeTo(newHome);
        //character.homeLandmark.RemoveCharacterHomeOnLandmark(character);
        //newHome.AddCharacterHomeOnLandmark(character);
        //Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MOVE_TO_RETURN_HOME, _characterInvolved.specificLocation.coreTile.landmarkOnTile);
        //character.SetForcedInteraction(interaction);
    }
    public int GetRandomInventoryTaskWeight() {
        return _inventoryTaskWeights[UnityEngine.Random.Range(0, _inventoryTaskWeights.Length)];
    }
    #endregion

    #region Relationships
    public void CreateRelationshipsForFaction(Faction faction) {
        for (int i = 0; i < allFactions.Count; i++) {
            Faction otherFaction = allFactions[i];
            if(otherFaction.id != faction.id) {
                CreateNewRelationshipBetween(otherFaction, faction);
            }
        }
    }
    public void CreateFavorsForFaction(Faction faction) {
        for (int i = 0; i < allFactions.Count; i++) {
            Faction otherFaction = allFactions[i];
            if (otherFaction.id != faction.id) {
                //faction.AddNewFactionFavor(otherFaction);
                //otherFaction.AddNewFactionFavor(faction);
            }
        }
    }
    public void LoadRelationshipsForFaction(Faction faction, FactionSaveData data) {
        for (int i = 0; i < allFactions.Count; i++) {
            Faction otherFaction = allFactions[i];
            if (otherFaction.id != faction.id) {
                FactionRelationship rel = CreateNewRelationshipBetween(otherFaction, faction);
                if (data.relationships.ContainsKey(otherFaction.id)) {
                    rel.SetRelationshipStatus(data.relationships[otherFaction.id]);
                }
            }
        }
    }
    
    //public void LoadFavorsForFaction(Faction faction, FactionSaveData data) {
    //    if (data.favor == null) {
    //        CreateFavorsForFaction(faction);
    //    } else {
    //        for (int i = 0; i < allFactions.Count; i++) {
    //            Faction otherFaction = allFactions[i];
    //            if (otherFaction.id != faction.id) {
    //                if (data.favor.ContainsKey(otherFaction.id)) {
    //                    //faction.AddNewFactionFavor(otherFaction, data.favor[otherFaction.id]);
    //                }
    //            }
    //        }
    //    }
    //}
    public void RemoveRelationshipsWith(Faction faction) {
        for (int i = 0; i < allFactions.Count; i++) {
            Faction otherFaction = allFactions[i];
            if (otherFaction.id != faction.id) {
                otherFaction.RemoveRelationshipWith(faction);
            }
        }
    }
    /*
     Create a new relationship between 2 factions,
     then add add a reference to that relationship, to both of the factions.
         */
    public FactionRelationship CreateNewRelationshipBetween(Faction faction1, Faction faction2) {
        FactionRelationship newRel = new FactionRelationship(faction1, faction2);
        faction1.AddNewRelationship(faction2, newRel);
        faction2.AddNewRelationship(faction1, newRel);
        return newRel;
    }
    /*
     Utility Function for getting the relationship between 2 factions,
     this just adds a checking for data consistency if, the 2 factions have the
     same reference to their relationship.
     NOTE: This is probably more performance intensive because of the additional checking.
     User can opt to use each factions GetRelationshipWith() instead.
         */
    public FactionRelationship GetRelationshipBetween(Faction faction1, Faction faction2) {
        FactionRelationship faction1Rel = faction1.GetRelationshipWith(faction2);
        FactionRelationship faction2Rel = faction2.GetRelationshipWith(faction1);
        if (faction1Rel == faction2Rel) {
            return faction1Rel;
        }
        throw new System.Exception(faction1.name + " does not have the same relationship object as " + faction2.name + "!");
    }
    public FACTION_RELATIONSHIP_STATUS GetRelationshipStatusBetween(Faction faction1, Faction faction2) {
        FactionRelationship rel = GetRelationshipBetween(faction1, faction2);
        return rel.relationshipStatus;
    }
    public List<Faction> GetFactionsWithByStatus(Faction faction, FACTION_RELATIONSHIP_STATUS status) {
        List<Faction> factions = new List<Faction>();
        foreach (KeyValuePair<Faction, FactionRelationship> kvp in faction.relationships) {
            if (kvp.Value.relationshipStatus == status) {
                factions.Add(kvp.Key);
            }
        }
        return factions;
    }
    public void DeclareWarBetween(Faction faction1, Faction faction2) {
        FactionRelationship rel = GetRelationshipBetween(faction1, faction2);
        rel.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.ENEMY);
        Messenger.Broadcast<string, int, UnityEngine.Events.UnityAction>(Signals.SHOW_DEVELOPER_NOTIFICATION, "<color=\"green\"><b> " + faction1.name + "</b></color> declares war on <color=\"green\"><b>" + faction2.name + "</b></color>.", 5, null);
    }
    public void DeclarePeaceBetween(Faction faction1, Faction faction2) {
        //faction1.SetFavorFor(faction2, -4);
        //faction2.SetFavorFor(faction1, -4);

        FactionRelationship rel = GetRelationshipBetween(faction1, faction2);
        rel.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.NEUTRAL);
        Messenger.Broadcast<string, int, UnityEngine.Events.UnityAction>(Signals.SHOW_DEVELOPER_NOTIFICATION, "<color=\"green\"><b> " + faction1.name + "</b></color> declares peace on <color=\"green\"><b>" + faction2.name + "</b></color>.", 5, null);
    }
    public int GetAverageFactionLevel() {
        int activeFactionsCount = allFactions.Where(x => x.isActive).Count();
        int totalFactionLvl = allFactions.Where(x => x.isActive).Sum(x => x.level);
        return totalFactionLvl / activeFactionsCount;
    }
    #endregion
}

[System.Serializable]
public class FactionEmblemSetting {
    public FactionEmblemDictionary emblems;

    public Sprite GetSpriteForSize(Image image) {
        if (image.rectTransform.sizeDelta.x <= 24) {
            return emblems[24];
        } else {
            return emblems[96];
        }
    }
    public Sprite GetSpriteForSize(int size) {
        if (size <= 24) {
            return emblems[24];
        } else {
            return emblems[96];
        }
    }
}
