using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Inner_Maps;
using UnityEngine;

public class Area : IJobOwner, ILocation {

    public int id { get; private set; }
    public LOCATION_TYPE locationType { get; private set; }
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
    //public List<SpecialToken> itemsInArea { get; private set; }
    public const int MAX_ITEM_CAPACITY = 15;

    //structures
    public Dictionary<STRUCTURE_TYPE, List<LocationStructure>> structures { get; private set; }
    public AreaInnerTileMap areaMap { get; private set; }
    public InnerTileMap innerMap => areaMap;

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
    //public LocationStructure mainStorageStructure { get; private set; }
    #endregion

    public Area(Region region, LOCATION_TYPE locationType, int citizenCount) {
        this.region = region;
        id = Utilities.SetID(this);
        this.citizenCount = citizenCount;
        //charactersAtLocation = new List<Character>();
        //defaultRace = new Race(RACE.HUMANS, RACE_SUB_TYPE.NORMAL);
        //itemsInArea = new List<SpecialToken>();
        structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
        //jobQueue = new JobQueue(this);
        SetAreaType(locationType);
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
        //itemsInArea = new List<SpecialToken>();
        structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
        //jobQueue = new JobQueue(null);

        SetAreaType(saveDataArea.locationType);

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
        Messenger.AddListener<IPointOfInterest, string>(Signals.FORCE_CANCEL_ALL_JOBS_TARGETTING_POI, ForceCancelAllJobsTargettingCharacter);

    }
    private void UnsubscribeToSignals() {
        Messenger.RemoveListener(Signals.HOUR_STARTED, HourlyJobActions);
        Messenger.RemoveListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        Messenger.RemoveListener<FoodPile>(Signals.FOOD_IN_PILE_REDUCED, OnFoodInPileReduced);
        Messenger.RemoveListener<WoodPile>(Signals.WOOD_IN_PILE_REDUCED, OnWoodInPileReduced);
        Messenger.RemoveListener<Character, CharacterClass, CharacterClass>(Signals.CHARACTER_CLASS_CHANGE, OnCharacterClassChange);
        Messenger.RemoveListener<IPointOfInterest, string>(Signals.FORCE_CANCEL_ALL_JOBS_TARGETTING_POI, ForceCancelAllJobsTargettingCharacter);
    }
    private void OnTileObjectRemoved(TileObject removedObj, Character character, LocationGridTile removedFrom) {
        //craft replacement tile object job
        if (removedFrom.parentMap.location == this) {
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
    public void SetAreaType(LOCATION_TYPE locationType) {
        this.locationType = locationType;
        OnAreaTypeSet();
    }
    public BASE_AREA_TYPE GetBaseAreaType() {
        AreaData data = LandmarkManager.Instance.GetAreaData(locationType);
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
        if (locationType == LOCATION_TYPE.DEMONIC_INTRUSION) {
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
        CheckAreaInventoryJobs(mainStorage);
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
        if (!character.isFactionless && !structures.ContainsKey(STRUCTURE_TYPE.DWELLING)) {
            Debug.LogWarning(this.name + " doesn't have any dwellings for " + character.name);
            return;
        }
        if (character.isFactionless) {
            character.SetHomeStructure(null);
            return;
        }
        Dwelling chosenDwelling = dwellingOverride;
        if (chosenDwelling == null) {
            if (PlayerManager.Instance != null && PlayerManager.Instance.player != null && this.id == PlayerManager.Instance.player.playerArea.id) {
                chosenDwelling = structures[STRUCTURE_TYPE.DWELLING][0] as Dwelling; //to avoid errors, residents in player area will all share the same dwelling
            } else {
                Character lover = (character.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TYPE.LOVER) as AlterEgoData)?.owner ?? null;
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
    }
    public void RemoveCharacterFromLocation(Character character) {
        region.RemoveCharacterFromLocation(character);
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
    public void SetInitialResidentCount(int count) {
        citizenCount = count;
    }
    private void OnCharacterClassChange(Character character, CharacterClass previousClass, CharacterClass currentClass) {
        if(character.homeRegion.area == this) {
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
        LocationGridTile mainEntrance = areaMap.GetRandomUnoccupiedEdgeTile();
        newResident.CreateMarker();
        newResident.InitialCharacterPlacement(mainEntrance);
    }
    #endregion

    #region Special Tokens
    //public bool AddSpecialTokenToLocation(SpecialToken token, LocationStructure structure = null, LocationGridTile gridLocation = null) {
    //    if (!itemsInArea.Contains(token)) {
    //        itemsInArea.Add(token);
    //        token.SetOwner(this.owner);
    //        if (areaMap != null) { //if the area map of this area has already been created.
    //            //Debug.Log(GameManager.Instance.TodayLogString() + "Added " + token.name + " at " + name);
    //            if (structure != null) {
    //                structure.AddItem(token, gridLocation);
    //            } else {
    //                //get structure for token
    //                LocationStructure chosen = InnerMapManager.Instance.GetRandomStructureToPlaceItem(this, token);
    //                chosen.AddItem(token);
    //            }
    //            OnItemAddedToLocation(token, token.structureLocation);
    //        }
    //        Messenger.Broadcast(Signals.ITEM_ADDED_TO_AREA, this, token);
    //        return true;
    //    }
    //    return false;
    //}
    //public void RemoveSpecialTokenFromLocation(SpecialToken token) {
    //    if (itemsInArea.Remove(token)) {
    //        LocationStructure takenFrom = token.structureLocation;
    //        if (takenFrom != null) {
    //            takenFrom.RemoveItem(token);
    //            OnItemRemovedFromLocation(token, takenFrom);
    //        }
    //        //Debug.Log(GameManager.Instance.TodayLogString() + "Removed " + token.name + " from " + name);
    //        Messenger.Broadcast(Signals.ITEM_REMOVED_FROM_AREA, this, token);
    //    }
    //}
    //public bool IsItemInventoryFull() {
    //    return itemsInArea.Count >= MAX_ITEM_CAPACITY;
    //}
    //private int GetItemsInAreaCount(SPECIAL_TOKEN itemType) {
    //    int count = 0;
    //    for (int i = 0; i < itemsInArea.Count; i++) {
    //        SpecialToken currItem = itemsInArea[i];
    //        if (currItem.specialTokenType == itemType) {
    //            count++;
    //        }
    //    }
    //    return count;
    //}
    public void OnItemAddedToLocation(SpecialToken item, LocationStructure structure) {
        CheckAreaInventoryJobs(structure);
    }
    public void OnItemRemovedFromLocation(SpecialToken item, LocationStructure structure) {
        CheckAreaInventoryJobs(structure);
    }
    public bool IsRequiredByLocation(SpecialToken token) {
        if (token.gridTileLocation != null && token.gridTileLocation.structure == mainStorage) {
            if (token.specialTokenType == SPECIAL_TOKEN.HEALING_POTION) {
                return mainStorage.GetItemsOfTypeCount(SPECIAL_TOKEN.HEALING_POTION) <= 2; //item is required by warehouse.
            } else if (token.specialTokenType == SPECIAL_TOKEN.TOOL) {
                return mainStorage.GetItemsOfTypeCount(SPECIAL_TOKEN.TOOL) <= 2; //item is required by warehouse.
            }
        }
        return false;
    }
    public bool IsSameCoreLocationAs(ILocation location) {
        return location.coreTile == this.coreTile;
    }
    #endregion

    #region Structures
    public void GenerateStructures(int citizenCount) {
        structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
        LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.CITY_CENTER, true);
        LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.WORK_AREA, true);
        LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.WILDERNESS, false);
        LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.POND, true);
        
        // LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.APOTHECARY, true);
        // LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.ASSASSIN_GUILD, true);
        // LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.BARRACKS, true);
        // LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.HUNTER_LODGE, true);
        // LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.MAGE_QUARTERS, true);
        // LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.MINER_CAMP, true);
        // LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.RAIDER_CAMP, true);
        // LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.SMITHY, true);

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
    public LocationStructure GetRandomStructureOfType(STRUCTURE_TYPE type) {
        if (structures.ContainsKey(type)) {
            return structures[type][Utilities.rng.Next(0, structures[type].Count)];
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
    public bool HasStructure(STRUCTURE_TYPE type) {
        return structures.ContainsKey(type);
    }
    #endregion

    #region Inner Map
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
            structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.MAGIC_CIRCLE));
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
                List<LocationGridTile> validTiles = structure.unoccupiedTiles.ToList();
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
        WoodPile woodPile = InnerMapManager.Instance.CreateNewTileObject<WoodPile>(TILE_OBJECT_TYPE.WOOD_PILE);
        woodPile.SetResourceInPile(51);
        mainStorage.AddPOI(woodPile);
        woodPile.gridTileLocation.SetReservedType(TILE_OBJECT_TYPE.WOOD_PILE);

        StonePile stonePile = InnerMapManager.Instance.CreateNewTileObject<StonePile>(TILE_OBJECT_TYPE.STONE_PILE);
        stonePile.SetResourceInPile(51);
        mainStorage.AddPOI(stonePile);
        stonePile.gridTileLocation.SetReservedType(TILE_OBJECT_TYPE.STONE_PILE);

        MetalPile metalPile = InnerMapManager.Instance.CreateNewTileObject<MetalPile>(TILE_OBJECT_TYPE.METAL_PILE);
        metalPile.SetResourceInPile(51);
        mainStorage.AddPOI(metalPile);
        metalPile.gridTileLocation.SetReservedType(TILE_OBJECT_TYPE.METAL_PILE);

        FoodPile foodPile = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(TILE_OBJECT_TYPE.FOOD_PILE);
        foodPile.SetResourceInPile(51);
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
                List<LocationGridTile> validTiles = structure.unoccupiedTiles.ToList();
                if (validTiles.Count > 0) {
                    LocationGridTile chosenTile = validTiles[UnityEngine.Random.Range(0, validTiles.Count)];
                    structure.AddPOI(new SmallAnimal(), chosenTile);
                } else {
                    break;
                }
            }

            for (int i = 0; i < ediblePlantsCount; i++) {
                List<LocationGridTile> validTiles = structure.unoccupiedTiles.ToList();
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
    private void AssignPrison() {
        if (locationType == LOCATION_TYPE.DEMONIC_INTRUSION) {
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
            if (mainStorage == null || mainStorage.structureType == STRUCTURE_TYPE.CITY_CENTER) {
                SetMainStorage(structure);
            }
        } else if (structure.structureType == STRUCTURE_TYPE.CITY_CENTER) {
            if (mainStorage == null) {
                SetMainStorage(structure);
            }
        }
    }
    private void SetMainStorage(LocationStructure structure) {
        bool shouldCheckResourcePiles = false;
        if(mainStorage != null && structure != null && mainStorage != structure) {
            shouldCheckResourcePiles = true;
        }
        mainStorage = structure;
        if (shouldCheckResourcePiles) {
            Messenger.Broadcast(Signals.REGION_CHANGE_STORAGE, region);
        }
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
    public bool AssignCharacterToJobBasedOnVision(Character character) {
        List<JobQueueItem> choices = new List<JobQueueItem>();
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job.assignedCharacter == null && job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                if (goapJob.targetPOI != null && character.marker.inVisionPOIs.Contains(goapJob.targetPOI) &&
                    character.jobQueue.CanJobBeAddedToQueue(job)) {
                    choices.Add(job);
                }
            }
        }
        if (choices.Count > 0) {
            JobQueueItem job = Utilities.GetRandomElement(choices);
            return character.jobQueue.AddJobInQueue(job);
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
    private void CheckAreaInventoryJobs(LocationStructure affectedStructure) {
        if (affectedStructure == mainStorage) {

            //brew potion
            if (affectedStructure.GetItemsOfTypeCount(SPECIAL_TOKEN.HEALING_POTION) < 2) {
                if (!HasJob(JOB_TYPE.BREW_POTION)) {
                    //create an un crafted potion and place it at the main storage structure, then use that as the target for the job.
                    SpecialToken item = TokenManager.Instance.CreateSpecialToken(SPECIAL_TOKEN.HEALING_POTION);
                    affectedStructure.AddItem(item);
                    item.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);

                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BREW_POTION, INTERACTION_TYPE.CRAFT_ITEM, item, this);
                    job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { TokenManager.Instance.itemData[SPECIAL_TOKEN.HEALING_POTION].craftCost });
                    job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanBrewPotion);
                    AddToAvailableJobs(job);
                }
            }


            //craft tool
            if (affectedStructure.GetItemsOfTypeCount(SPECIAL_TOKEN.TOOL) < 2) {
                if (!HasJob(JOB_TYPE.CRAFT_TOOL)) {
                    //create an un crafted potion and place it at the main storage structure, then use that as the target for the job.
                    SpecialToken item = TokenManager.Instance.CreateSpecialToken(SPECIAL_TOKEN.TOOL);
                    affectedStructure.AddItem(item);
                    item.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);

                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_TOOL, INTERACTION_TYPE.CRAFT_ITEM, item, this);
                    job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { TokenManager.Instance.itemData[SPECIAL_TOKEN.TOOL].craftCost });
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
            CheckAreaInventoryJobs(mainStorage);
        }
    }
    //private void CreateReplaceTileObjectJob(TileObject removedObj, LocationGridTile removedFrom) {
    //    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REPLACE_TILE_OBJECT, INTERACTION_TYPE.REPLACE_TILE_OBJECT, new Dictionary<INTERACTION_TYPE, object[]>() {
    //                    { INTERACTION_TYPE.REPLACE_TILE_OBJECT, new object[]{ removedObj, removedFrom } },
    //    });
    //    job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeReplaceTileObjectJob);
    //    AddToAvailableJobs(job);
    //}
    private void ForceCancelAllJobsTargettingCharacter(IPointOfInterest target, string reason) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if(job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                if(goapJob.targetPOI == target) {
                    if(goapJob.ForceCancelJob(false, reason)) {
                        i--;
                    }
                }
            }
        }
    }
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
    private int maxHeroEventJobs => region.residents.Count / 5; //There should be at most 1 Move Out Job per 5 residents
    private int currentHeroEventJobs => GetNumberOfJobsWith(IsJobTypeAHeroEventJob);
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
    private void PerDayHeroEventCreation() {
        //improve job at 8 am
        GameDate improveJobDate = GameManager.Instance.Today();
        improveJobDate.SetTicks(GameManager.Instance.GetTicksBasedOnHour(8));
        SchedulingManager.Instance.AddEntry(improveJobDate, TryCreateImproveJob, this);

        // //explore job at 8 am
        // GameDate exploreJobDate = GameManager.Instance.Today();
        // exploreJobDate.SetTicks(GameManager.Instance.GetTicksBasedOnHour(8));
        // SchedulingManager.Instance.AddEntry(exploreJobDate, TryCreateExploreJob, this);

        // //combat job at 8 am
        // GameDate combatJobDate = GameManager.Instance.Today();
        // combatJobDate.SetTicks(GameManager.Instance.GetTicksBasedOnHour(8));
        // SchedulingManager.Instance.AddEntry(combatJobDate, TryCreateCombatJob, this);
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
        Region validRegion;
        if (UnityEngine.Random.Range(0, 100) < 15 && TryGetRegionToStudyAt(out validRegion)) {//15 
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IMPROVE, 
                new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Buff", false, GOAP_EFFECT_TARGET.ACTOR),
                validRegion.regionTileObject, this) ;
            AddToAvailableJobs(job);
            //expires at midnight
            GameDate expiry = GameManager.Instance.Today();
            expiry.SetTicks(GameManager.Instance.GetTicksBasedOnHour(24));
            SchedulingManager.Instance.AddEntry(expiry, () => CheckIfJobWillExpire(job), this);
        }
    }
    private bool TryGetRegionToStudyAt(out Region validRegion) {
        var validRegions = new List<Region>();
        for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            if (currRegion.mainLandmark.specificLandmarkType.IsPlayerLandmark() == false && currRegion.mainLandmark.specificLandmarkType != LANDMARK_TYPE.NONE 
                && currRegion.locationType.IsSettlementType() == false) {
                validRegions.Add(currRegion);
            }
        }
        if (validRegions.Count > 0) {
            validRegion = Utilities.GetRandomElement(validRegions);
            return true;
        } else {
            validRegion = null;
            return false;    
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
    //public void OnMapGenerationFinished() {
    //    //place tokens in area to actual structures.
    //    //get structure for token
    //    for (int i = 0; i < itemsInArea.Count; i++) {
    //        SpecialToken token = itemsInArea[i];
    //        LocationStructure chosen = InnerMapManager.Instance.GetRandomStructureToPlaceItem(this, token);
    //        chosen.AddItem(token);
    //        if (chosen.isInside) {
    //            token.SetOwner(this.owner);
    //        }
    //    }
    //}
    #endregion

    #region Awareness
    public bool AddAwareness(IPointOfInterest pointOfInterest) {
        return region.AddAwareness(pointOfInterest);
    }
    public void RemoveAwareness(IPointOfInterest pointOfInterest) {
        region.RemoveAwareness(pointOfInterest);
    }
    public void RemoveAwareness(POINT_OF_INTEREST_TYPE poiType) {
        region.RemoveAwareness(poiType);
    }
    public bool HasAwareness(IPointOfInterest poi) {
        return region.HasAwareness(poi);
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