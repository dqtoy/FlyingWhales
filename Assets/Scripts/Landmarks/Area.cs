using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class Area : IJobOwner {

    public int id { get; private set; }
    public AREA_TYPE areaType { get; private set; }
    public Region region { get; private set; }
    public LocationStructure prison { get; private set; }
    public LocationStructure mainStorage { get; private set; }
    public int citizenCount { get; private set; }

    //Data that are only referenced from this area's region
    //These are only getter data, meaning it cannot be stored
    public string name { get { return region.name; } }
    public HexTile coreTile { get { return region.coreTile; } }
    public Faction owner { get { return region.owner; } }
    public Faction previousOwner { get { return region.previousOwner; } }
    public List<HexTile> tiles { get { return region.tiles; } }
    public List<Character> charactersAtLocation { get { return region.charactersAtLocation; } }

    //special tokens
    public List<SpecialToken> itemsInArea { get; private set; }
    public const int MAX_ITEM_CAPACITY = 15;

    //structures
    public Dictionary<STRUCTURE_TYPE, List<LocationStructure>> structures { get; private set; }
    public AreaInnerTileMap areaMap { get; private set; }

    //misc
    public Sprite locationPortrait { get; private set; }
    public Vector2 nameplatePos { get; private set; }

    public List<JobQueueItem> availableJobs { get; protected set; }
    public JOB_OWNER ownerType { get { return JOB_OWNER.QUEST; } }

    public LocationClassManager classManager { get; private set; }
    public LocationEventManager eventManager { get; private set; }
    public LocationJobManager jobManager { get; private set; }

    #region getters
    public List<Character> visitors {
        get { return charactersAtLocation.Where(x => !region.residents.Contains(x)).ToList(); }
    }
    //public int suppliesInBank {
    //    get {
    //        if (this.supplyPile == null) {
    //            return 0;
    //        }
    //        return this.supplyPile.resourceInPile;
    //    }
    //}
    //public WoodPile supplyPile { get; private set; }
    //public FoodPile foodPile { get; private set; }
    public int residentCapacity {
        get {
            if (structures.ContainsKey(STRUCTURE_TYPE.DWELLING)) {
                return structures[STRUCTURE_TYPE.DWELLING].Count;
            }
            return 0;
        }
    }
    public LocationStructure mainStorageStructure { get; private set; }
    #endregion

    public Area(Region region, AREA_TYPE areaType, int citizenCount) {
        this.region = region;
        id = Utilities.SetID(this);
        this.citizenCount = citizenCount;
        //charactersAtLocation = new List<Character>();
        //defaultRace = new Race(RACE.HUMANS, RACE_SUB_TYPE.NORMAL);
        itemsInArea = new List<SpecialToken>();
        structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
        //jobQueue = new JobQueue(this);
        SetAreaType(areaType);
        //AddTile(coreTile);
        nameplatePos = LandmarkManager.Instance.GetNameplatePosition(this.coreTile);
        availableJobs = new List<JobQueueItem>();
        classManager = new LocationClassManager();
        eventManager = new LocationEventManager(this);
        jobManager = new LocationJobManager(this);

    }
    public Area(SaveDataArea saveDataArea) {
        region = GridMap.Instance.GetRegionByID(saveDataArea.regionID);
        id = Utilities.SetID(this, saveDataArea.id);
        citizenCount = saveDataArea.citizenCount;
        //charactersAtLocation = new List<Character>();
        itemsInArea = new List<SpecialToken>();
        structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
        //jobQueue = new JobQueue(null);

        SetAreaType(saveDataArea.areaType);

        nameplatePos = LandmarkManager.Instance.GetNameplatePosition(this.coreTile);

        LoadStructures(saveDataArea);
    }

    #region Listeners
    private void SubscribeToSignals() {
        Messenger.AddListener(Signals.HOUR_STARTED, HourlyJobActions);
        Messenger.AddListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        Messenger.AddListener<FoodPile>(Signals.FOOD_IN_PILE_REDUCED, OnFoodInPileReduced);
        Messenger.AddListener<WoodPile>(Signals.WOOD_IN_PILE_REDUCED, OnWoodInPileReduced);
        Messenger.AddListener(Signals.DAY_STARTED, PerDayHeroEventCreation);
        Messenger.AddListener<Character, CharacterClass, CharacterClass>(Signals.CHARACTER_CLASS_CHANGE, OnCharacterClassChange);
    }
    private void UnsubscribeToSignals() {
        Messenger.RemoveListener(Signals.HOUR_STARTED, HourlyJobActions);
        Messenger.RemoveListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        Messenger.RemoveListener<FoodPile>(Signals.FOOD_IN_PILE_REDUCED, OnFoodInPileReduced);
        Messenger.RemoveListener<WoodPile>(Signals.WOOD_IN_PILE_REDUCED, OnWoodInPileReduced);
        Messenger.RemoveListener<Character, CharacterClass, CharacterClass>(Signals.CHARACTER_CLASS_CHANGE, OnCharacterClassChange);
    }
    private void OnTileObjectRemoved(TileObject removedObj, Character character, LocationGridTile removedFrom) {
        //craft replacement tile object job
        if (removedFrom.parentAreaMap.area.id == this.id) {
            //&& removedFrom.structure.structureType != STRUCTURE_TYPE.DWELLING
            if (removedObj.CanBeReplaced()) { //if the removed object can be replaced and it is not part of a dwelling, create a replace job
                if (removedFrom.structure.structureType == STRUCTURE_TYPE.DWELLING) {
                    Dwelling dwelling = removedFrom.structure as Dwelling;
                    if (dwelling.residents.Count > 0) {
                        //check if any of the residents can craft the tile object
                        bool canBeCrafted = false;
                        for (int i = 0; i < dwelling.residents.Count; i++) {
                            Character currResident = dwelling.residents[i];
                            if (removedObj.tileObjectType.CanBeCraftedBy(currResident)) {
                                //add job to resident
                                //currResident.CreateReplaceTileObjectJob(removedObj, removedFrom);
                                //canBeCrafted = true;
                                break;
                            }
                        }
                        if (!canBeCrafted) {
                            //no resident can craft object, post in settlement
                            //CreateReplaceTileObjectJob(removedObj, removedFrom);
                        }
                    } else {
                        //CreateReplaceTileObjectJob(removedObj, removedFrom);
                    }
                } else {
                    //CreateReplaceTileObjectJob(removedObj, removedFrom);
                }
            }
        }
    }
    private void OnFoodInPileReduced(FoodPile pile) {
        //if (foodPile == pile) {
        //    TryCreateObtainFoodOutsideJob();
        //}
    }
    private void OnWoodInPileReduced(WoodPile pile) {
        //if (supplyPile == pile) {
        //    TryCreateObtainSupplyOutsideJob();
        //}
    }
    #endregion

    #region Area Type
    public void SetAreaType(AREA_TYPE areaType) {
        this.areaType = areaType;
        OnAreaTypeSet();
    }
    public BASE_AREA_TYPE GetBaseAreaType() {
        AreaData data = LandmarkManager.Instance.GetAreaData(areaType);
        return data.baseAreaType;
    }
    private void OnAreaTypeSet() {
        //update tile visuals
        //for (int i = 0; i < tiles.Count; i++) {
        //    HexTile currTile = tiles[i];
        //    OnTileAddedToArea(currTile);
        //}
    }
    #endregion

    #region Visuals
    public void HighlightArea() {
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currTile = tiles[i];
#if WORLD_CREATION_TOOL
            if (!worldcreator.WorldCreatorManager.Instance.selectionComponent.selection.Contains(currTile)) {
                if (currTile.id == coreTile.id) {
                    currTile.HighlightTile(areaColor, 255f/255f);
                } else {
                    currTile.HighlightTile(areaColor, 128f/255f);
                }
            }
#endif
        }
    }
    public void UnhighlightArea() {
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currTile = tiles[i];
            currTile.UnHighlightTile();
        }
    }
    public void TintStructuresInArea(Color color) {
        for (int i = 0; i < tiles.Count; i++) {
            tiles[i].SetStructureTint(color);
        }
    }
    public void SetLocationPortrait(Sprite portrait) {
        locationPortrait = portrait;
    }
    private void CreateNameplate() {
        //GameObject nameplateGO = UIManager.Instance.InstantiateUIObject("AreaNameplate", coreTile.tileLocation.landmarkOnTile.landmarkVisual.landmarkCanvas.transform);
        ////nameplateGO.transform.position = coreTile.transform.position;
        //nameplateGO.GetComponent<AreaNameplate>().SetArea(this);
        UIManager.Instance.CreateAreaNameplate(this);
    }
    #endregion

    #region Utilities
    public void LoadAdditionalData() {
        CreateNameplate();
    }
    public string GetAreaTypeString() {
        if (areaType == AREA_TYPE.DEMONIC_INTRUSION) {
            return "Demonic Intrusion";
        }
        //if (_raceType != RACE.NONE) {
        //    if (tiles.Count > 1) {
        //        return Utilities.GetNormalizedRaceAdjective(_raceType) + " " + Utilities.NormalizeStringUpperCaseFirstLetters(GetBaseAreaType().ToString());
        //    } else {
        //        return Utilities.GetNormalizedRaceAdjective(_raceType) + " " + Utilities.NormalizeStringUpperCaseFirstLetters(coreTile.landmarkOnTile.specificLandmarkType.ToString());
        //    }
        //} else {
        //    return Utilities.NormalizeStringUpperCaseFirstLetters(coreTile.landmarkOnTile.specificLandmarkType.ToString());
        //}
        return Utilities.NormalizeStringUpperCaseFirstLetters(coreTile.landmarkOnTile.specificLandmarkType.ToString());
    }
    /// <summary>
    /// Called when this area is set as the current active area.
    /// </summary>
    public void OnAreaSetAsActive() {
        SubscribeToSignals();
        //LocationStructure warehouse = GetRandomStructureOfType(STRUCTURE_TYPE.WAREHOUSE);
        CheckAreaInventoryJobs(mainStorageStructure);
    }
    public bool CanInvadeSettlement() {
        return coreTile.region.HasCorruptedConnection() && PlayerManager.Instance.player.currentAreaBeingInvaded == null && PlayerManager.Instance.player.minions.Where(x => x.assignedRegion == null).ToList().Count > 0;
    }
    #endregion

    #region Supplies
    //public void AdjustSuppliesInBank(int amount) {
    //    if (supplyPile == null) {
    //        return;
    //    }
    //    supplyPile.AdjustResourceInPile(amount);
    //    Messenger.Broadcast(Signals.AREA_SUPPLIES_CHANGED, this);
    //    //suppliesInBank = Mathf.Clamp(suppliesInBank, 0, supplyCapacity);
    //}
    #endregion

    #region Food
    //public void SetFoodInBank(int amount) {
    //    FoodPile currFoodPile = foodPile;
    //    if (currFoodPile == null) {
    //        return;
    //    }
    //    currFoodPile.SetResourceInPile(amount);
    //    Messenger.Broadcast(Signals.AREA_FOOD_CHANGED, this);
    //}
    //public void AdjustFoodInBank(int amount) {
    //    FoodPile currFoodPile = foodPile;
    //    if (currFoodPile == null) {
    //        return;
    //    }
    //    currFoodPile.AdjustResourceInPile(amount);
    //    Messenger.Broadcast(Signals.AREA_FOOD_CHANGED, this);
    //}
    #endregion

    #region Characters
    public void AssignCharacterToDwellingInArea(Character character, Dwelling dwellingOverride = null) {
        if (structures == null) {
            Debug.LogWarning(this.name + " doesn't have any dwellings for " + character.name + " because structrues have not been generated yet");
            return;
        }
        if (character.faction != FactionManager.Instance.neutralFaction && !structures.ContainsKey(STRUCTURE_TYPE.DWELLING)) {
            Debug.LogWarning(this.name + " doesn't have any dwellings for " + character.name);
            return;
        }
        if (character.faction == FactionManager.Instance.neutralFaction) {
            character.SetHomeStructure(null);
            return;
        }
        Dwelling chosenDwelling = dwellingOverride;
        if (chosenDwelling == null) {
            if (PlayerManager.Instance != null && PlayerManager.Instance.player != null && this.id == PlayerManager.Instance.player.playerArea.id) {
                chosenDwelling = structures[STRUCTURE_TYPE.DWELLING][0] as Dwelling; //to avoid errors, residents in player area will all share the same dwelling
            } else {
                Character lover = (character.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.LOVER) as AlterEgoData)?.owner ?? null;
                if (lover != null && lover.faction.id == character.faction.id && region.residents.Contains(lover)) { //check if the character has a lover that lives in the area
                    chosenDwelling = lover.homeStructure;
                }
            }
            if (chosenDwelling == null && (character.homeStructure == null || character.homeStructure.location.id != this.id)) { //else, find an unoccupied dwelling (also check if the character doesn't already live in this area)
                List<LocationStructure> structureList = structures[STRUCTURE_TYPE.DWELLING];
                for (int i = 0; i < structureList.Count; i++) {
                    Dwelling currDwelling = structureList[i] as Dwelling;
                    if (currDwelling.CanBeResidentHere(character)) {
                        chosenDwelling = currDwelling;
                        break;
                    }
                }
            }
        }

        if (chosenDwelling == null) {
            //if the code reaches here, it means that the area could not find a dwelling for the character
            Debug.LogWarning(GameManager.Instance.TodayLogString() + "Could not find a dwelling for " + character.name + " at " + this.name);
        }
        character.MigrateHomeStructureTo(chosenDwelling);
    }
    public void AddCharacterToLocation(Character character, LocationGridTile tileOverride = null, bool isInitial = false) {
        region.AddCharacterToLocation(character);
        //if (!charactersAtLocation.Contains(character)) {
        //    charactersAtLocation.Add(character);
        //    character.ownParty.SetSpecificLocation(this);
        //    //AddCharacterAtLocationHistory("Added " + character.name + "ST: " + StackTraceUtility.ExtractStackTrace());
        //    //if (tileOverride != null) {
        //    //    tileOverride.structure.AddCharacterAtLocation(character, tileOverride);
        //    //} else {
        //    //    if (isInitial) {
        //    //        AddCharacterToAppropriateStructure(character);
        //    //    } else {
        //    //        LocationGridTile exit = GetRandomUnoccupiedEdgeTile();
        //    //        exit.structure.AddCharacterAtLocation(character, exit);
        //    //    }
        //    //}
        //    //Debug.Log(GameManager.Instance.TodayLogString() + "Added " + character.name + " to location " + name);
        //    Messenger.Broadcast(Signals.CHARACTER_ENTERED_AREA, this, character);
        //}
    }
    public void RemoveCharacterFromLocation(Character character) {
        region.RemoveCharacterFromLocation(character);
        //if (charactersAtLocation.Remove(character)) {
        //    //character.ownParty.SetSpecificLocation(null);
        //    if (character.currentStructure == null && this != PlayerManager.Instance.player.playerArea) {
        //        throw new Exception(character.name + " doesn't have a current structure at " + this.name);
        //    }
        //    if (character.currentStructure != null) {
        //        character.currentStructure.RemoveCharacterAtLocation(character);
        //    }
        //    //AddCharacterAtLocationHistory("Removed " + character.name + "ST: " + StackTraceUtility.ExtractStackTrace());
        //    Messenger.Broadcast(Signals.CHARACTER_EXITED_AREA, this, character);
        //}
    }
    public void RemoveCharacterFromLocation(Party party) {
        RemoveCharacterFromLocation(party.owner);
    }
    public bool IsResidentsFull() {
        if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.playerArea.id == this.id) {
            return false; //resident capacity is never full for player area
        }
        if (structures.ContainsKey(STRUCTURE_TYPE.DWELLING)) {
            List<LocationStructure> dwellings = structures[STRUCTURE_TYPE.DWELLING];
            for (int i = 0; i < dwellings.Count; i++) {
                if (!dwellings[i].IsOccupied()) {
                    return false;
                }
            }
        }
        return true;
        //return structures[STRUCTURE_TYPE.DWELLING].Where(x => !x.IsOccupied()).Count() == 0; //check if there are still unoccupied dwellings
    }
    public int GetNumberOfUnoccupiedStructure(STRUCTURE_TYPE structureType) {
        if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.playerArea.id == this.id) {
            return 0;
        }
        int num = 0;
        if (structures.ContainsKey(structureType)) {
            List<LocationStructure> structureList = structures[structureType];
            for (int i = 0; i < structureList.Count; i++) {
                if (!structureList[i].IsOccupied()) {
                    num++;
                }
            }
        }
        return num;
    }
    public Character GetRandomCharacterAtLocationExcept(Character character) {
        List<Character> choices = new List<Character>();
        for (int i = 0; i < charactersAtLocation.Count; i++) {
            if (charactersAtLocation[i] != character) {
                choices.Add(charactersAtLocation[i]);
            }
        }
        if (choices.Count > 0) {
            return choices[UnityEngine.Random.Range(0, choices.Count)];
        }
        return null;
    }
    public List<Character> GetAllDeadCharactersInArea() {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character character = CharacterManager.Instance.allCharacters[i];
            if(character.isDead && character.specificLocation == this && !(character is Summon)) {
                if(character.marker != null || character.grave != null) { //Only resurrect characters who are in the tombstone or still has a marker in the area
                    characters.Add(character);
                }
            }
        }
        //CharacterMarker[] markers = Utilities.GetComponentsInDirectChildren<CharacterMarker>(areaMap.objectsParent.gameObject);
        //for (int i = 0; i < markers.Length; i++) {
        //    CharacterMarker currMarker = markers[i];
        //    if (currMarker.character != null && currMarker.character.isDead) {
        //        characters.Add(currMarker.character);
        //    }
        //}

        //List<TileObject> tombstones = GetTileObjectsOfType(TILE_OBJECT_TYPE.TOMBSTONE);
        //for (int i = 0; i < tombstones.Count; i++) {
        //    characters.Add(tombstones[i].users[0]);
        //}
        return characters;
    }
    public void SetInitialResidentCount(int count) {
        citizenCount = count;
    }
    private void OnCharacterClassChange(Character character, CharacterClass previousClass, CharacterClass currentClass) {
        if(character.homeArea == this) {
            classManager.OnResidentChangeClass(character, previousClass, currentClass);
        }
    }
    public Character AddNewResident(RACE race, Faction faction) {
        string className = classManager.GetCurrentClassToCreate();
        Character citizen = CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, className, race, Utilities.GetRandomGender(), faction, region);
        PlaceNewResidentInInnerMap(citizen);
        //citizen.CenterOnCharacter();
        return citizen;

        //if (className == "Leader") {
        //    citizen.LevelUp(leaderLevel - 1);
        //    SetLeader(leader);
        //} else {
        //    citizen.LevelUp(citizensLevel - 1);
        //}
    }
    public Character AddNewResident(RACE race, GENDER gender, Faction faction) {
        string className = classManager.GetCurrentClassToCreate();
        Character citizen = CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, className, race, gender, faction, region);
        PlaceNewResidentInInnerMap(citizen);
        //citizen.CenterOnCharacter();
        return citizen;
    }
    public Character AddNewResident(RACE race, GENDER gender, SEXUALITY sexuality, Faction faction) {
        string className = classManager.GetCurrentClassToCreate();
        Character citizen = CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, className, race, gender, sexuality, faction, region);
        PlaceNewResidentInInnerMap(citizen);
        //citizen.CenterOnCharacter();
        return citizen;
    }
    public Character CreateNewResidentNoLocation(RACE race, string className, Faction faction) {
        Character citizen = CharacterManager.Instance.CreateNewCharacter(CharacterRole.SOLDIER, className, race, Utilities.GetRandomGender(), faction);
        return citizen;
    }
    public void PlaceNewResidentInInnerMap(Character newResident) {
        LocationGridTile mainEntrance = GetRandomUnoccupiedEdgeTile();
        newResident.CreateMarker();
        newResident.InitialCharacterPlacement(mainEntrance);
    }
    #endregion

    #region Special Tokens
    public bool AddSpecialTokenToLocation(SpecialToken token, LocationStructure structure = null, LocationGridTile gridLocation = null) {
        if (!itemsInArea.Contains(token)) {
            itemsInArea.Add(token);
            token.SetOwner(this.owner);
            if (areaMap != null) { //if the area map of this area has already been created.
                //Debug.Log(GameManager.Instance.TodayLogString() + "Added " + token.name + " at " + name);
                if (structure != null) {
                    structure.AddItem(token, gridLocation);
                    //if (structure.isInside) {
                    //    token.SetOwner(this.owner);
                    //}
                } else {
                    //get structure for token
                    LocationStructure chosen = GetRandomStructureToPlaceItem(token);
                    chosen.AddItem(token);
                    //if (chosen.isInside) {
                    //    token.SetOwner(this.owner);
                    //}
                }
                OnItemAddedToLocation(token, token.structureLocation);
            }
            Messenger.Broadcast(Signals.ITEM_ADDED_TO_AREA, this, token);
            return true;
        }
        return false;
    }
    public void RemoveSpecialTokenFromLocation(SpecialToken token) {
        if (itemsInArea.Remove(token)) {
            LocationStructure takenFrom = token.structureLocation;
            if (takenFrom != null) {
                takenFrom.RemoveItem(token);
                OnItemRemovedFromLocation(token, takenFrom);
            }
            //Debug.Log(GameManager.Instance.TodayLogString() + "Removed " + token.name + " from " + name);
            Messenger.Broadcast(Signals.ITEM_REMOVED_FROM_AREA, this, token);
        }

    }
    public bool IsItemInventoryFull() {
        return itemsInArea.Count >= MAX_ITEM_CAPACITY;
    }
    private LocationStructure GetRandomStructureToPlaceItem(SpecialToken token) {
        //Items are now placed specifically in a structure when spawning at world creation. 
        //Randomly place it at any non-Dwelling structure in the location.
        List<LocationStructure> choices = new List<LocationStructure>();
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> kvp in structures) {
            if (kvp.Key != STRUCTURE_TYPE.DWELLING && kvp.Key != STRUCTURE_TYPE.EXIT && kvp.Key != STRUCTURE_TYPE.CEMETERY
                && kvp.Key != STRUCTURE_TYPE.INN && kvp.Key != STRUCTURE_TYPE.WORK_AREA && kvp.Key != STRUCTURE_TYPE.PRISON && kvp.Key != STRUCTURE_TYPE.POND) {
                choices.AddRange(kvp.Value);
            }
        }
        if (choices.Count > 0) {
            return choices[UnityEngine.Random.Range(0, choices.Count)];
        }
        return null;
    }
    private int GetItemsInAreaCount(SPECIAL_TOKEN itemType) {
        int count = 0;
        for (int i = 0; i < itemsInArea.Count; i++) {
            SpecialToken currItem = itemsInArea[i];
            if (currItem.specialTokenType == itemType) {
                count++;
            }
        }
        return count;
    }
    private void OnItemAddedToLocation(SpecialToken item, LocationStructure structure) {
        CheckAreaInventoryJobs(structure);
    }
    private void OnItemRemovedFromLocation(SpecialToken item, LocationStructure structure) {
        CheckAreaInventoryJobs(structure);
    }
    public bool IsRequiredByArea(SpecialToken token) {
        if (token.gridTileLocation != null && token.gridTileLocation.structure == mainStorageStructure) {
            if (token.specialTokenType == SPECIAL_TOKEN.HEALING_POTION) {
                return mainStorageStructure.GetItemsOfTypeCount(SPECIAL_TOKEN.HEALING_POTION) <= 2; //item is required by warehouse.
            } else if (token.specialTokenType == SPECIAL_TOKEN.TOOL) {
                return mainStorageStructure.GetItemsOfTypeCount(SPECIAL_TOKEN.TOOL) <= 2; //item is required by warehouse.
            }
        }
        return false;
    }
    #endregion

    #region Structures
    public void GenerateStructures(int citizenCount) {
        structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
        //all areas should have
        // - a warehouse
        // - an inn
        // - a prison
        // - a work area
        // - a cemetery
        // - wilderness
        // - enough dwellings for it's citizens
        LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.CITY_CENTER, true);
        //LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.INN, true);
        //LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.WAREHOUSE, true);
        //LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.PRISON, true);
        //LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.CEMETERY, true);
        LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.WORK_AREA, true);
        LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.WILDERNESS, false);
        LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.POND, true);
        for (int i = 0; i < citizenCount; i++) {
            LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.DWELLING, true);
        }
        AssignPrison();
    }
    public void LoadStructures(SaveDataArea data) {
        structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();

        for (int i = 0; i < data.structures.Count; i++) {
            LandmarkManager.Instance.LoadStructureAt(this, data.structures[i]);
        }
        AssignPrison();
    }
    public void AddStructure(LocationStructure structure) {
        if (!structures.ContainsKey(structure.structureType)) {
            structures.Add(structure.structureType, new List<LocationStructure>());
        }

        if (!structures[structure.structureType].Contains(structure)) {
            structures[structure.structureType].Add(structure);
        }
    }
    public void RemoveStructure(LocationStructure structure) {
        if (structures.ContainsKey(structure.structureType)) {
            if (structures[structure.structureType].Remove(structure)) {

                if (structures[structure.structureType].Count == 0) { //this is only for optimization
                    structures.Remove(structure.structureType);
                }
            }
        }
    }
    /*
     NOTE: Location Status Legend:
     0 = outside,
     1 = inside,
     2 = any
         */
    public LocationStructure GetRandomStructureOfType(STRUCTURE_TYPE type, int locationStatus = 2) {
        if (structures.ContainsKey(type)) { //any
            if (locationStatus == 2) {
                return structures[type][Utilities.rng.Next(0, structures[type].Count)];
            } else if (locationStatus == 0) { //outside only
                List<LocationStructure> choices = new List<LocationStructure>();
                for (int i = 0; i < structures[type].Count; i++) {
                    LocationStructure currStructure = structures[type][i];
                    if (!currStructure.isInside) {
                        choices.Add(currStructure);
                    }
                }
                return choices[UnityEngine.Random.Range(0, choices.Count)];
            } else if (locationStatus == 1) { //inside only
                List<LocationStructure> choices = new List<LocationStructure>();
                for (int i = 0; i < structures[type].Count; i++) {
                    LocationStructure currStructure = structures[type][i];
                    if (currStructure.isInside) {
                        choices.Add(currStructure);
                    }
                }
                return choices[Utilities.rng.Next(0, choices.Count)];
            }

        }
        return null;
    }
    public LocationStructure GetRandomStructure() {
        Dictionary<STRUCTURE_TYPE, List<LocationStructure>> structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>(this.structures);
        structures.Remove(STRUCTURE_TYPE.EXIT);
        int dictIndex = UnityEngine.Random.Range(0, structures.Count);
        int count = 0;
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> kvp in structures) {
            if (count == dictIndex) {
                return kvp.Value[UnityEngine.Random.Range(0, kvp.Value.Count)];
            }
            count++;
        }
        return null;
    }
    public LocationStructure GetStructureByID(STRUCTURE_TYPE type, int id) {
        if (structures.ContainsKey(type)) {
            List<LocationStructure> locStructures = structures[type];
            for (int i = 0; i < locStructures.Count; i++) {
                if(locStructures[i].id == id) {
                    return locStructures[i];
                }
            }
        }
        return null;
    }
    public List<LocationStructure> GetStructuresOfType(STRUCTURE_TYPE structureType) {
        if (structures.ContainsKey(structureType)) {
            return structures[structureType];
        }
        return null;
    }
    public List<LocationStructure> GetStructuresAtLocation(bool inside) {
        List<LocationStructure> structures = new List<LocationStructure>();
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> kvp in this.structures) {
            for (int i = 0; i < kvp.Value.Count; i++) {
                LocationStructure currStructure = kvp.Value[i];
                if (currStructure.isInside == inside && currStructure.structureType != STRUCTURE_TYPE.EXIT) {
                    structures.Add(currStructure);
                }
            }
        }
        return structures;
    }
    public Dictionary<STRUCTURE_TYPE, List<LocationStructure>> GetStructures(bool inside, bool includeExit = false) {
        Dictionary<STRUCTURE_TYPE, List<LocationStructure>> structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> kvp in this.structures) {
            structures.Add(kvp.Key, new List<LocationStructure>());
            for (int i = 0; i < kvp.Value.Count; i++) {
                LocationStructure currStructure = kvp.Value[i];
                if (currStructure.isInside == inside) {
                    if (kvp.Key == STRUCTURE_TYPE.EXIT && !includeExit) {
                        continue; //do not include exit
                    }
                    structures[kvp.Key].Add(currStructure);
                }
            }
        }
        return structures;
    }
    public bool HasStructure(STRUCTURE_TYPE type) {
        return structures.ContainsKey(type);
    }
    public void SetAreaMap(AreaInnerTileMap map) {
        areaMap = map;
    }
    public void PlaceObjects() {
        //pre placed objects
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in structures) {
            for (int i = 0; i < keyValuePair.Value.Count; i++) {
                LocationStructure structure = keyValuePair.Value[i];
                structure.structureObj?.RegisterPreplacedObjects(structure, this.areaMap);
            }
        }

        //place build spots
        PlaceBuildSpots();

        PlaceOres();
        PlaceResourcePiles();
        SpawnFoodObjects();

        //magic circle
        if (structures.ContainsKey(STRUCTURE_TYPE.WILDERNESS)) {
            LocationStructure structure = structures[STRUCTURE_TYPE.WILDERNESS][0];
            structure.AddPOI(new MagicCircle());
        }
        //Goddess Statue
        if (structures.ContainsKey(STRUCTURE_TYPE.WORK_AREA)) {
            for (int i = 0; i < 4; i++) {
                LocationStructure structure = structures[STRUCTURE_TYPE.WORK_AREA][0];
                structure.AddPOI(new GoddessStatue());
            }
        }
    }
    private void PlaceBuildSpots() {
        for (int x = 0; x < areaMap.buildingSpots.GetUpperBound(0); x++) {
            for (int y = 0; y < areaMap.buildingSpots.GetUpperBound(1); y++) {
                BuildingSpot spot = areaMap.buildingSpots[x, y];
                BuildSpotTileObject tileObj = new BuildSpotTileObject();
                tileObj.SetBuildingSpot(spot);
                LocationGridTile tileLocation = areaMap.map[spot.location.x, spot.location.y];
                //if (tileLocation.objHere != null) {
                //    tileLocation.structure.RemovePOI(tileLocation.objHere);
                //}
                tileLocation.structure.AddPOI(tileObj, tileLocation, false);
                tileObj.SetGridTileLocation(tileLocation); //manually placed so that only the data of the build spot will be set, and the tile will not consider the build spot as objHere
                if (tileLocation.structure.structureType != STRUCTURE_TYPE.WORK_AREA && tileLocation.structure.structureType != STRUCTURE_TYPE.WILDERNESS) {
                    tileLocation.structure.SetOccupiedBuildSpot(tileObj);
                }
            }
        }
    }
    private void PlaceOres() {
        if (structures.ContainsKey(STRUCTURE_TYPE.WILDERNESS)) {
            LocationStructure structure = structures[STRUCTURE_TYPE.WILDERNESS][0];
            int oreCount = 4;
            for (int i = 0; i < oreCount; i++) {
                List<LocationGridTile> validTiles = structure.unoccupiedTiles.Where(x => x.IsAdjacentToPasssableTiles(3)).ToList();
                if (validTiles.Count > 0) {
                    LocationGridTile chosenTile = validTiles[UnityEngine.Random.Range(0, validTiles.Count)];
                    structure.AddPOI(new Ore(), chosenTile);
                } else {
                    break;
                }
            }
        }
    }
    private void PlaceResourcePiles() {
        if (structures.ContainsKey(STRUCTURE_TYPE.WAREHOUSE)) {
            mainStorage = GetRandomStructureOfType(STRUCTURE_TYPE.WAREHOUSE);
        } else {
            mainStorage = GetRandomStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
        }
        TileObject woodPile = InteriorMapManager.Instance.CreateNewTileObject(TILE_OBJECT_TYPE.WOOD_PILE);
        mainStorage.AddPOI(woodPile);
        woodPile.gridTileLocation.SetReservedType(TILE_OBJECT_TYPE.WOOD_PILE);

        TileObject stonePile = InteriorMapManager.Instance.CreateNewTileObject(TILE_OBJECT_TYPE.STONE_PILE);
        mainStorage.AddPOI(stonePile);
        stonePile.gridTileLocation.SetReservedType(TILE_OBJECT_TYPE.STONE_PILE);

        TileObject metalPile = InteriorMapManager.Instance.CreateNewTileObject(TILE_OBJECT_TYPE.METAL_PILE);
        mainStorage.AddPOI(metalPile);
        metalPile.gridTileLocation.SetReservedType(TILE_OBJECT_TYPE.METAL_PILE);

        TileObject foodPile = InteriorMapManager.Instance.CreateNewTileObject(TILE_OBJECT_TYPE.FOOD_PILE);
        mainStorage.AddPOI(foodPile);
        foodPile.gridTileLocation.SetReservedType(TILE_OBJECT_TYPE.FOOD_PILE);
    }
    private void SpawnFoodObjects() {
        if (structures.ContainsKey(STRUCTURE_TYPE.WILDERNESS)) {
            LocationStructure structure = structures[STRUCTURE_TYPE.WILDERNESS][0];
            //Reduce number of Small Animals and Edible Plants in the wilderness to 4 and 6 respectively. 
            //Also, they should all be placed in spots adjacent to at least three passsable tiles.
            int smallAnimalCount = 4;
            int ediblePlantsCount = 6;

            for (int i = 0; i < smallAnimalCount; i++) {
                List<LocationGridTile> validTiles = structure.unoccupiedTiles.Where(x => x.IsAdjacentToPasssableTiles(3)).ToList();
                if (validTiles.Count > 0) {
                    LocationGridTile chosenTile = validTiles[UnityEngine.Random.Range(0, validTiles.Count)];
                    structure.AddPOI(new SmallAnimal(), chosenTile);
                } else {
                    break;
                }
            }

            for (int i = 0; i < ediblePlantsCount; i++) {
                List<LocationGridTile> validTiles = structure.unoccupiedTiles.Where(x => x.IsAdjacentToPasssableTiles(3)).ToList();
                if (validTiles.Count > 0) {
                    LocationGridTile chosenTile = validTiles[UnityEngine.Random.Range(0, validTiles.Count)];
                    structure.AddPOI(new EdiblePlant(), chosenTile);
                } else {
                    break;
                }
            }
        }
    }
    public IPointOfInterest GetRandomTileObject() {
        List<IPointOfInterest> tileObjects = new List<IPointOfInterest>();
        foreach (List<LocationStructure> locationStructures in structures.Values) {
            for (int i = 0; i < locationStructures.Count; i++) {
                for (int j = 0; j < locationStructures[i].pointsOfInterest.Count; j++) {
                    if (locationStructures[i].pointsOfInterest[j].poiType != POINT_OF_INTEREST_TYPE.CHARACTER) {
                        tileObjects.Add(locationStructures[i].pointsOfInterest[j]);
                    }
                }
            }
        }
        if (tileObjects.Count > 0) {
            return tileObjects[UnityEngine.Random.Range(0, tileObjects.Count)];
        }
        return null;
    }
    public LocationGridTile GetRandomUnoccupiedEdgeTile() {
        List<LocationGridTile> unoccupiedEdgeTiles = new List<LocationGridTile>();
        for (int i = 0; i < areaMap.allEdgeTiles.Count; i++) {
            if (!areaMap.allEdgeTiles[i].isOccupied && areaMap.allEdgeTiles[i].structure != null) { // - There should not be a checker for structure, fix the generation of allEdgeTiles in AreaInnerTileMap's GenerateGrid
                unoccupiedEdgeTiles.Add(areaMap.allEdgeTiles[i]);
            }
        }
        if (unoccupiedEdgeTiles.Count > 0) {
            return unoccupiedEdgeTiles[UnityEngine.Random.Range(0, unoccupiedEdgeTiles.Count)];
        }
        return null;
    }
    private void AssignPrison() {
        if (areaType == AREA_TYPE.DEMONIC_INTRUSION) {
            return;
        }
        LocationStructure chosenPrison = GetRandomStructureOfType(STRUCTURE_TYPE.PRISON);
        if (chosenPrison != null) {
            prison = chosenPrison;
        } else {
            chosenPrison = GetRandomStructureOfType(STRUCTURE_TYPE.EXPLORE_AREA);
            if (chosenPrison != null) {
                prison = chosenPrison;
            } else {
                chosenPrison = GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA);
                if (chosenPrison != null) {
                    prison = chosenPrison;
                } else {
                    Debug.LogError("Cannot assign a prison! There is no warehouse, explore area, or work area structure in the location of " + name);
                }
            }
        }
    }
    //public void SetSupplyPile(WoodPile supplyPile) {
    //    this.supplyPile = supplyPile;
    //}
    //public void SetFoodPile(FoodPile foodPile) {
    //    this.foodPile = foodPile;
    //}
    public void OnLocationStructureObjectPlaced(LocationStructure structure) {
        if (structure.structureType == STRUCTURE_TYPE.WAREHOUSE) {
            //if a warehouse was placed, and this area does not yet have a main storage structure, or is using the city center as their main storage structure, then use the new warehouse instead.
            if (mainStorageStructure == null || mainStorageStructure.structureType == STRUCTURE_TYPE.CITY_CENTER) {
                SetMainStorageStructure(structure);
            }
        } else if (structure.structureType == STRUCTURE_TYPE.CITY_CENTER) {
            if (mainStorageStructure == null) {
                SetMainStorageStructure(structure);
            }
        }
    }
    private void SetMainStorageStructure(LocationStructure structure) {
        mainStorageStructure = structure;
    }
    #endregion

    #region POI
    public List<IPointOfInterest> GetPointOfInterestsOfType(POINT_OF_INTEREST_TYPE type) {
        List<IPointOfInterest> pois = new List<IPointOfInterest>();
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in structures) {
            for (int i = 0; i < keyValuePair.Value.Count; i++) {
                pois.AddRange(keyValuePair.Value[i].GetPOIsOfType(type));
            }
        }
        return pois;
    }
    public List<TileObject> GetTileObjectsOfType(TILE_OBJECT_TYPE type) {
        List<TileObject> objs = new List<TileObject>();
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in structures) {
            for (int i = 0; i < keyValuePair.Value.Count; i++) {
                objs.AddRange(keyValuePair.Value[i].GetTileObjectsOfType(type));
            }
        }
        return objs;
    }
    public List<TileObject> GetTileObjectsThatAdvertise(params INTERACTION_TYPE[] types) {
        List<TileObject> objs = new List<TileObject>();
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in structures) {
            for (int i = 0; i < keyValuePair.Value.Count; i++) {
                objs.AddRange(keyValuePair.Value[i].GetTileObjectsThatAdvertise(types));
            }
        }
        return objs;
    }
    #endregion

    #region Jobs
    public void AddToAvailableJobs(JobQueueItem job) {
        availableJobs.Add(job);
        jobManager.OnAddToAvailableJobs(job);
        Debug.Log($"{GameManager.Instance.TodayLogString()}{job.ToString()} was added to {this.name}'s available jobs");
    }
    public bool RemoveFromAvailableJobs(JobQueueItem job) {
        if (availableJobs.Remove(job)) {
            Debug.Log($"{GameManager.Instance.TodayLogString()}{job.ToString()} was removed from {this.name}'s available jobs");
            OnJobRemovedFromAvailableJobs(job);
            return true;
        }
        return false;
    }
    //public bool RemoveFromAvailableJobs(JOB_TYPE jobType) {
    //    for (int i = 0; i < availableJobs.Count; i++) {
    //        if(availableJobs[i].jobType == jobType) {
    //            availableJobs.RemoveAt(i);
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    public int GetNumberOfJobsWith(CHARACTER_STATE state) {
        int count = 0;
        for (int i = 0; i < availableJobs.Count; i++) {
            if (availableJobs[i] is CharacterStateJob) {
                CharacterStateJob job = availableJobs[i] as CharacterStateJob;
                if (job.targetState == state) {
                    count++;
                }
            }
        }
        return count;
    }
    public int GetNumberOfJobsWith(JOB_TYPE type) {
        int count = 0;
        for (int i = 0; i < availableJobs.Count; i++) {
            if (availableJobs[i].jobType == type) {
                count++;
            }
        }
        return count;
    }
    public int GetNumberOfJobsWith(System.Func<JobQueueItem, bool> checker) {
        int count = 0;
        for (int i = 0; i < availableJobs.Count; i++) {
            if (checker.Invoke(availableJobs[i])) {
                count++;
            }
        }
        return count;
    }
    public bool HasJob(JobQueueItem job) {
        for (int i = 0; i < availableJobs.Count; i++) {
            if (job == availableJobs[i]) {
                return true;
            }
        }
        return false;
    }
    public bool HasJob(JOB_TYPE job, IPointOfInterest target) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem jqi = availableJobs[i];
            if (jqi is GoapPlanJob) {
                GoapPlanJob gpj = jqi as GoapPlanJob;
                if (job == gpj.jobType && target == gpj.targetPOI) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasJob(params JOB_TYPE[] jobTypes) {
        for (int i = 0; i < availableJobs.Count; i++) {
            for (int j = 0; j < jobTypes.Length; j++) {
                if (availableJobs[i].jobType == jobTypes[j]) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasJobWithOtherData(JOB_TYPE jobType, object otherData) {
        for (int i = 0; i < availableJobs.Count; i++) {
            if (availableJobs[i].jobType == jobType && availableJobs[i] is GoapPlanJob) {
                GoapPlanJob job = availableJobs[i] as GoapPlanJob;
                if (job.allOtherData != null) {
                    for (int j = 0; j < job.allOtherData.Count; j++) {
                        object data = job.allOtherData[j];
                        if (data == otherData) {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
    public JobQueueItem GetJob(params JOB_TYPE[] jobTypes) {
        for (int i = 0; i < availableJobs.Count; i++) {
            for (int j = 0; j < jobTypes.Length; j++) {
                JobQueueItem job = availableJobs[i];
                if (job.jobType == jobTypes[j]) {
                    return job;
                }
            }
        }
        return null;
    }
    public JobQueueItem GetJob(JOB_TYPE job, IPointOfInterest target) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem jqi = availableJobs[i];
            if (jqi is GoapPlanJob) {
                GoapPlanJob gpj = jqi as GoapPlanJob;
                if (job == gpj.jobType && target == gpj.targetPOI) {
                    return gpj;
                }
            }
        }
        return null;
    }
    public bool AddFirstUnassignedJobToCharacterJob(Character character) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if(job.assignedCharacter == null && character.jobQueue.AddJobInQueue(job)) {
                return true;
            }
        }
        return false;
    }
    private void HourlyJobActions() {
        CreatePatrolJobs();
    }
    private void CreatePatrolJobs() {
        int patrolChance = UnityEngine.Random.Range(0, 100);
        if (patrolChance < 25 && GetNumberOfJobsWith(CHARACTER_STATE.PATROL) < 2) {
            CharacterStateJob stateJob = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.PATROL, CHARACTER_STATE.PATROL, this);
            stateJob.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoPatrolAndExplore);
            AddToAvailableJobs(stateJob);
        }
    }
    public void CheckAreaInventoryJobs(LocationStructure affectedStructure) {
        if (affectedStructure == mainStorageStructure) {
            //brew potion
            //- If there are less than 2 Healing Potions in the Warehouse, it will create a Brew Potion job
            //- the warehouse stores an inventory count of items it needs to keep in stock. anytime an item is added or removed (claimed by someone, stolen or destroyed), inventory will be checked and missing items will be procured
            //- any character that can produce this item may take this Job
            //- cancel Brew Potion job whenever inventory check occurs and it specified that there are enough Healing Potions already

            //if (affectedStructure.GetItemsOfTypeCount(SPECIAL_TOKEN.HEALING_POTION) < 2) {
            //    if (!HasJob(JOB_TYPE.BREW_POTION)) {
            //        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BREW_POTION, INTERACTION_TYPE.DROP_ITEM, , new Dictionary<INTERACTION_TYPE, object[]>() {
            //            { INTERACTION_TYPE.DROP_ITEM, new object[]{ SPECIAL_TOKEN.HEALING_POTION } },
            //            { INTERACTION_TYPE.CRAFT_ITEM, new object[]{ SPECIAL_TOKEN.HEALING_POTION } },
            //        }, this);
            //        job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanBrewPotion);
            //        job.SetOnTakeJobAction(InteractionManager.Instance.OnTakeBrewPotion);
            //        //job.SetCannotOverrideJob(false);
            //        AddToAvailableJobs(job);
            //    }
            //} else {
            //    //warehouse has 2 or more healing potions
            //    JobQueueItem brewJob = GetJob(JOB_TYPE.BREW_POTION);
            //    if (brewJob != null) {
            //        ForceCancelJob(brewJob);
            //    }
            //}

            if (affectedStructure.GetItemsOfTypeCount(SPECIAL_TOKEN.HEALING_POTION) < 2) {
                if (!HasJob(JOB_TYPE.BREW_POTION)) {
                    //create an un crafted potion and place it at the main storage structure, then use that as the target for the job.
                    SpecialToken item = TokenManager.Instance.CreateSpecialToken(SPECIAL_TOKEN.HEALING_POTION);
                    AddSpecialTokenToLocation(item, affectedStructure);
                    item.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);

                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BREW_POTION, INTERACTION_TYPE.CRAFT_ITEM, item, this);
                    job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanBrewPotion);
                    AddToAvailableJobs(job);
                }
            }


            //craft tool

            //if (affectedStructure.GetItemsOfTypeCount(SPECIAL_TOKEN.TOOL) < 2) {
            //    if (!HasJob(JOB_TYPE.CRAFT_TOOL)) {
            //        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_TOOL, INTERACTION_TYPE.DROP_ITEM, new Dictionary<INTERACTION_TYPE, object[]>() {
            //            { INTERACTION_TYPE.DROP_ITEM, new object[]{ GetRandomStructureOfType(STRUCTURE_TYPE.WAREHOUSE), SPECIAL_TOKEN.TOOL } },
            //            { INTERACTION_TYPE.CRAFT_ITEM, new object[]{ SPECIAL_TOKEN.TOOL } },
            //        }, this);
            //        job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCraftTool);
            //        job.SetOnTakeJobAction(InteractionManager.Instance.OnTakeCraftTool);
            //        //job.SetCannotOverrideJob(false);
            //        AddToAvailableJobs(job);
            //    }
            //} else {
            //    JobQueueItem craftToolJob = GetJob(JOB_TYPE.CRAFT_TOOL);
            //    if (craftToolJob != null) {
            //        ForceCancelJob(craftToolJob);
            //    }
            //}

            if (affectedStructure.GetItemsOfTypeCount(SPECIAL_TOKEN.TOOL) < 2) {
                if (!HasJob(JOB_TYPE.CRAFT_TOOL)) {
                    //create an un crafted potion and place it at the main storage structure, then use that as the target for the job.
                    SpecialToken item = TokenManager.Instance.CreateSpecialToken(SPECIAL_TOKEN.TOOL);
                    AddSpecialTokenToLocation(item, affectedStructure);
                    item.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);

                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_TOOL, INTERACTION_TYPE.CRAFT_ITEM, item, this);
                    job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCraftTool);
                    AddToAvailableJobs(job);
                }
            }
        }
    }
    private void OnJobRemovedFromAvailableJobs(JobQueueItem job) {
        jobManager.OnRemoveFromAvailableJobs(job);
        JobManager.Instance.OnFinishJob(job);
        if (job.jobType == JOB_TYPE.CRAFT_TOOL || job.jobType == JOB_TYPE.BREW_POTION) {
            CheckAreaInventoryJobs(mainStorageStructure);
        }
    }
    //private void CreateReplaceTileObjectJob(TileObject removedObj, LocationGridTile removedFrom) {
    //    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REPLACE_TILE_OBJECT, INTERACTION_TYPE.REPLACE_TILE_OBJECT, new Dictionary<INTERACTION_TYPE, object[]>() {
    //                    { INTERACTION_TYPE.REPLACE_TILE_OBJECT, new object[]{ removedObj, removedFrom } },
    //    });
    //    job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeReplaceTileObjectJob);
    //    AddToAvailableJobs(job);
    //}
    #endregion

    #region IJobOwner
    public void OnJobAddedToCharacterJobQueue(JobQueueItem job, Character character) {
        //RemoveFromAvailableJobs(job);
    }
    public void OnJobRemovedFromCharacterJobQueue(JobQueueItem job, Character character) {
        if (!job.IsJobStillApplicable()) {
            RemoveFromAvailableJobs(job);
        }
    }
    public bool ForceCancelJob(JobQueueItem job) {
        return RemoveFromAvailableJobs(job);
    }
    #endregion

    #region Hero Event Jobs
    private int maxHeroEventJobs {
        get { return region.residents.Count / 5; } //There should be at most 1 Move Out Job per 5 residents
    }
    private int currentHeroEventJobs {
        get { return GetNumberOfJobsWith(IsJobTypeAHeroEventJob); }
    }
    private bool IsJobTypeAHeroEventJob(JobQueueItem item) {
        switch (item.jobType) {
            case JOB_TYPE.OBTAIN_FOOD_OUTSIDE:
            case JOB_TYPE.OBTAIN_SUPPLY_OUTSIDE:
            case JOB_TYPE.IMPROVE:
            case JOB_TYPE.EXPLORE:
            case JOB_TYPE.COMBAT_WORLD_EVENT:
                return true;
            default:
                return false;
        }
    }
    private bool CanStillCreateHeroEventJob() {
        return currentHeroEventJobs < maxHeroEventJobs;
    }
    /// <summary>
    /// Check if this area should create an obtain food outside job.
    /// Criteria can be found at: https://trello.com/c/cICMVSch/2706-hero-events
    /// </summary>
    private void TryCreateObtainFoodOutsideJob() {
        //TODO:
        //if (!CanStillCreateHeroEventJob()) {
        //    return; //hero events are maxed.
        //}
        //int obtainFoodOutsideJobs = GetNumberOfJobsWith(JOB_TYPE.OBTAIN_FOOD_OUTSIDE);
        //if (obtainFoodOutsideJobs == 0 && foodPile.resourceInPile < 1000) {
        //    CreateObtainFoodOutsideJob();
        //} else  if (obtainFoodOutsideJobs == 1 && foodPile.resourceInPile < 500) { //there is at least 1 existing obtain food outside job.
        //    //allow the creation of a second obtain food outside job
        //    CreateObtainFoodOutsideJob();
        //}
    }
    private void CreateObtainFoodOutsideJob() {
        CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.OBTAIN_FOOD_OUTSIDE, CHARACTER_STATE.MOVE_OUT, this);
        job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoObtainFoodOutsideJob);
        AddToAvailableJobs(job);
    }
    /// <summary>
    /// Check if this area should create an obtain supply outside job.
    /// Criteria can be found at: https://trello.com/c/cICMVSch/2706-hero-events
    /// </summary>
    private void TryCreateObtainSupplyOutsideJob() {
        //TODO:
        //if (!CanStillCreateHeroEventJob()) {
        //    return; //hero events are maxed.
        //}
        //int obtainSupplyOutsideJobs = GetNumberOfJobsWith(JOB_TYPE.OBTAIN_SUPPLY_OUTSIDE);
        //if (obtainSupplyOutsideJobs == 0 && supplyPile.resourceInPile < 1000) {
        //    CreateObtainSupplyOutsideJob();
        //} else if (obtainSupplyOutsideJobs == 1 && supplyPile.resourceInPile < 500) { //there is at least 1 existing obtain supply outside job.
        //    //allow the creation of a second obtain supply outside job
        //    CreateObtainSupplyOutsideJob();
        //}
    }
    private void CreateObtainSupplyOutsideJob() {
        CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.OBTAIN_SUPPLY_OUTSIDE, CHARACTER_STATE.MOVE_OUT, this);
        job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoObtainSupplyOutsideJob);
        AddToAvailableJobs(job);
    }
    private void PerDayHeroEventCreation() {
        //improve job at 8 am
        GameDate improveJobDate = GameManager.Instance.Today();
        improveJobDate.SetTicks(GameManager.Instance.GetTicksBasedOnHour(8));
        SchedulingManager.Instance.AddEntry(improveJobDate, TryCreateImproveJob, this);

        //explore job at 8 am
        GameDate exploreJobDate = GameManager.Instance.Today();
        exploreJobDate.SetTicks(GameManager.Instance.GetTicksBasedOnHour(8));
        SchedulingManager.Instance.AddEntry(exploreJobDate, TryCreateExploreJob, this);

        //combat job at 8 am
        GameDate combatJobDate = GameManager.Instance.Today();
        combatJobDate.SetTicks(GameManager.Instance.GetTicksBasedOnHour(8));
        SchedulingManager.Instance.AddEntry(combatJobDate, TryCreateCombatJob, this);
    }
    /// <summary>
    /// Try and create an improve job. This checks chances and max hero event jobs.
    /// Criteria can be found at: https://trello.com/c/cICMVSch/2706-hero-events
    /// NOTE: Since this will be checked each day at a specific time, I just added a scheduled event that calls this at the start of each day, rather than checking it every tick.
    /// </summary>
    private void TryCreateImproveJob() {
        if (!CanStillCreateHeroEventJob()) {
            return; //hero events are maxed.
        }
        if (UnityEngine.Random.Range(0, 100) < 15) {//15
            CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.IMPROVE, CHARACTER_STATE.MOVE_OUT, this);
            AddToAvailableJobs(job);
            //expires at midnight
            GameDate expiry = GameManager.Instance.Today();
            expiry.SetTicks(GameManager.Instance.GetTicksBasedOnHour(24));
            SchedulingManager.Instance.AddEntry(expiry, () => CheckIfJobWillExpire(job), this);
        }
    }
    /// <summary>
    /// Try and create an explore job. This checks chances and max hero event jobs.
    /// Criteria can be found at: https://trello.com/c/cICMVSch/2706-hero-events
    /// NOTE: Since this will be checked each day at a specific time, I just added a scheduled event that calls this at the start of each day, rather than checking it every tick.
    /// </summary>
    private void TryCreateExploreJob() {
        if (!CanStillCreateHeroEventJob()) {
            return; //hero events are maxed.
        }
        if (UnityEngine.Random.Range(0, 100) < 15) {//15
            CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.EXPLORE, CHARACTER_STATE.MOVE_OUT, this);
            //Used lambda expression instead of new function. Reference: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/lambda-expressions
            job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoExploreJob);
            AddToAvailableJobs(job);
            //expires at midnight
            GameDate expiry = GameManager.Instance.Today();
            expiry.SetTicks(GameManager.Instance.GetTicksBasedOnHour(24));
            SchedulingManager.Instance.AddEntry(expiry, () => CheckIfJobWillExpire(job), this);
        }
    }
    /// <summary>
    /// Try and create a combat job. This checks chances and max hero event jobs.
    /// Criteria can be found at: https://trello.com/c/cICMVSch/2706-hero-events
    /// NOTE: Since this will be checked each day at a specific time, I just added a scheduled event that calls this at the start of each day, rather than checking it every tick.
    /// </summary>
    private void TryCreateCombatJob() {
        if (!CanStillCreateHeroEventJob()) {
            return; //hero events are maxed.
        }
        if (UnityEngine.Random.Range(0, 100) < 15) {//15
            CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.COMBAT_WORLD_EVENT, CHARACTER_STATE.MOVE_OUT, this);
            job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoCombatJob);
            AddToAvailableJobs(job);
            //expires at midnight
            GameDate expiry = GameManager.Instance.Today();
            expiry.SetTicks(GameManager.Instance.GetTicksBasedOnHour(24));
            SchedulingManager.Instance.AddEntry(expiry, () => CheckIfJobWillExpire(job), this);
        }
    }
    private void CheckIfJobWillExpire(JobQueueItem item) {
        if (item.assignedCharacter == null) {
            Debug.Log(GameManager.Instance.TodayLogString() + item.jobType.ToString() + " expired.");
            item.CancelJob();
        }
    }
    #endregion

    #region Area Map
    public void OnMapGenerationFinished() {
        //place tokens in area to actual structures.
        //get structure for token
        for (int i = 0; i < itemsInArea.Count; i++) {
            SpecialToken token = itemsInArea[i];
            LocationStructure chosen = GetRandomStructureToPlaceItem(token);
            chosen.AddItem(token);
            if (chosen.isInside) {
                token.SetOwner(this.owner);
            }
        }
    }
    #endregion

    public override string ToString() {
        return name;
    }
}

[System.Serializable]
public struct IntRange {
    public int lowerBound;
    public int upperBound;
    
    public IntRange(int low, int high) {
        lowerBound = low;
        upperBound = high;
    }

    public void SetLower(int lower) {
        lowerBound = lower;
    }
    public void SetUpper(int upper) {
        upperBound = upper;
    }

    public int Random() {
        return UnityEngine.Random.Range(lowerBound, upperBound + 1);
    }

    public bool IsInRange(int value) {
        if (value >= lowerBound && value <= upperBound) {
            return true;
        }
        return false;
    }

    public bool IsNearUpperBound(int value) {
        int lowerBoundDifference = Mathf.Abs(value - lowerBound);
        int upperBoundDifference = Mathf.Abs(value - upperBound);
        if (upperBoundDifference < lowerBoundDifference) {
            return true;
        }
        return false;
    }
}
[System.Serializable]
public struct Race {
    public RACE race;
    public RACE_SUB_TYPE subType;

    public Race(RACE race, RACE_SUB_TYPE subType) {
        this.race = race;
        this.subType = subType;
    }
}
[System.Serializable]
public class InitialRaceSetup {
    public Race race;
    public IntRange spawnRange;
    public IntRange levelRange;

    public InitialRaceSetup(Race race) {
        this.race = race;
        spawnRange = new IntRange(0, 0);
        levelRange = new IntRange(0, 0);
    }
}