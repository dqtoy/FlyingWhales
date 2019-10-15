using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class Area {

    public int id { get; private set; }
    //public bool isDead { get; private set; }
    public AREA_TYPE areaType { get; private set; }
    public Region region { get; private set; }
    public JobQueue jobQueue { get; private set; }
    public LocationStructure prison { get; private set; }
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

    //public Race defaultRace { get; private set; }
    //private RACE _raceType;

    #region getters
    public List<Character> visitors {
        get { return charactersAtLocation.Where(x => !region.residents.Contains(x)).ToList(); }
    }
    public int suppliesInBank {
        get {
            LocationStructure warehouse = GetRandomStructureOfType(STRUCTURE_TYPE.WAREHOUSE, 1);
            if (warehouse == null) {
                return 0;
            }
            SupplyPile supplyPile = warehouse.GetSupplyPile();
            if (supplyPile == null) {
                return 0;
            }
            return supplyPile.suppliesInPile;
        }
    }
    public SupplyPile supplyPile {
        get {
            LocationStructure warehouse = GetRandomStructureOfType(STRUCTURE_TYPE.WAREHOUSE, 1);
            if (warehouse == null) {
                return null;
            }
            return warehouse.GetSupplyPile();
        }
    }
    public int foodInBank {
        get {
            FoodPile currFoodPile = foodPile;
            if (currFoodPile == null) {
                return 0;
            }
            return currFoodPile.foodInPile;
        }
    }
    public FoodPile foodPile {
        get {
            LocationStructure warehouse = GetRandomStructureOfType(STRUCTURE_TYPE.WAREHOUSE, 1);
            if (warehouse == null) {
                return null;
            }
            return warehouse.GetFoodPile();
        }
    }
    public int residentCapacity {
        get {
            if (structures.ContainsKey(STRUCTURE_TYPE.DWELLING)) {
                return structures[STRUCTURE_TYPE.DWELLING].Count;
            }
            return 0;
        }
    }
    #endregion

    public Area(Region region, AREA_TYPE areaType, int citizenCount) {
        this.region = region;
        id = Utilities.SetID(this);
        this.citizenCount = citizenCount;
        //charactersAtLocation = new List<Character>();
        //defaultRace = new Race(RACE.HUMANS, RACE_SUB_TYPE.NORMAL);
        itemsInArea = new List<SpecialToken>();
        structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
        jobQueue = new JobQueue(null);
        SetAreaType(areaType);
        //AddTile(coreTile);
        nameplatePos = LandmarkManager.Instance.GetNameplatePosition(this.coreTile);
    }
    public Area(AreaSaveData data) {
        id = Utilities.SetID(this, data.areaID);
        //charactersAtLocation = new List<Character>();
        SetAreaType(data.areaType);
        itemsInArea = new List<SpecialToken>();
        jobQueue = new JobQueue(null);
        LoadStructures(data);
        AssignPrison();

//#if WORLD_CREATION_TOOL
//        SetCoreTile(worldcreator.WorldCreatorManager.Instance.GetHexTile(data.coreTileID));
//#else
//        SetCoreTile(GridMap.Instance.GetHexTile(data.coreTileID));
//#endif

        //AddTile(Utilities.GetTilesFromIDs(data.tileData));
        //UpdateBorderColors();
        //GenerateDefaultRace();
        nameplatePos = LandmarkManager.Instance.GetNameplatePosition(this.coreTile);
    }

    public Area(SaveDataArea saveDataArea) {
        region = GridMap.Instance.GetRegionByID(saveDataArea.regionID);
        id = Utilities.SetID(this, saveDataArea.id);
        citizenCount = saveDataArea.citizenCount;
        //charactersAtLocation = new List<Character>();
        itemsInArea = new List<SpecialToken>();
        structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
        jobQueue = new JobQueue(null);

        SetAreaType(saveDataArea.areaType);

        nameplatePos = LandmarkManager.Instance.GetNameplatePosition(this.coreTile);

        LoadStructures(saveDataArea);
    }

    #region Listeners
    private void SubscribeToSignals() {
        Messenger.AddListener(Signals.HOUR_STARTED, HourlyJobActions);
        Messenger.AddListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        Messenger.AddListener<FoodPile>(Signals.FOOD_IN_PILE_REDUCED, OnFoodInPileReduced);
        Messenger.AddListener<SupplyPile>(Signals.SUPPLY_IN_PILE_REDUCED, OnSupplyInPileReduced);
        Messenger.AddListener(Signals.DAY_STARTED, PerDayHeroEventCreation);
    }
    private void UnsubscribeToSignals() {
        Messenger.RemoveListener(Signals.HOUR_STARTED, HourlyJobActions);
        Messenger.RemoveListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        Messenger.RemoveListener<FoodPile>(Signals.FOOD_IN_PILE_REDUCED, OnFoodInPileReduced);
        Messenger.RemoveListener<SupplyPile>(Signals.SUPPLY_IN_PILE_REDUCED, OnSupplyInPileReduced);
        Messenger.RemoveListener(Signals.DAY_STARTED, PerDayHeroEventCreation);
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
                                currResident.CreateReplaceTileObjectJob(removedObj, removedFrom);
                                canBeCrafted = true;
                                break;
                            }
                        }
                        if (!canBeCrafted) {
                            //no resident can craft object, post in settlement
                            CreateReplaceTileObjectJob(removedObj, removedFrom);
                        }
                    } else {
                        CreateReplaceTileObjectJob(removedObj, removedFrom);
                    }
                } else {
                    CreateReplaceTileObjectJob(removedObj, removedFrom);
                }
            }
        }
    }
    private void OnFoodInPileReduced(FoodPile pile) {
        if (foodPile == pile) {
            TryCreateObtainFoodOutsideJob();
        }
    }
    private void OnSupplyInPileReduced(SupplyPile pile) {
        if (supplyPile == pile) {
            TryCreateObtainSupplyOutsideJob();
        }
    }
    #endregion

    #region Tile Management
    //public void AddTile(List<HexTile> tiles) {
    //    for (int i = 0; i < tiles.Count; i++) {
    //        AddTile(tiles[i]);
    //    }
    //}
    //public void AddTile(HexTile tile) {
    //    region.AddTile(tile);
    //    //if (!tiles.Contains(tile)) {
    //    //    tiles.Add(tile);
    //    //    tile.SetArea(this);
    //    //    OnTileAddedToArea(tile);
    //    //    Messenger.Broadcast(Signals.AREA_TILE_ADDED, this, tile);
    //    //}
    //}
    //public void RemoveTile(List<HexTile> tiles) {
    //    for (int i = 0; i < tiles.Count; i++) {
    //        RemoveTile(tiles[i]);
    //    }
    //}
    //public void RemoveTile(HexTile tile) {
    //    region.RemoveTile(tile);
    //    //if (tiles.Remove(tile)) {
    //    //    tile.SetArea(null);
    //    //    OnTileRemovedFromArea(tile);
    //    //    Messenger.Broadcast(Signals.AREA_TILE_REMOVED, this, tile);
    //    //}
    //}
    //public void OnTileAddedToArea(HexTile addedTile) {
    //    if (this.areaType == AREA_TYPE.ANCIENT_RUINS) {
    //        addedTile.SetBiome(BIOMES.ANCIENT_RUIN);
    //        Biomes.Instance.UpdateTileVisuals(addedTile);
    //    }
    //    //update tile visuals if necessary
    //    if (this.areaType == AREA_TYPE.DEMONIC_INTRUSION) {
    //        Biomes.Instance.CorruptTileVisuals(addedTile);
    //    }
    //}
    //public void OnTileRemovedFromArea(HexTile removedTile) {
    //    if (this.areaType == AREA_TYPE.ANCIENT_RUINS) {
    //        removedTile.SetBaseSprite(Biomes.Instance.ancienctRuinTiles[UnityEngine.Random.Range(0, Biomes.Instance.ancienctRuinTiles.Length)]);
    //        removedTile.HideLandmarkTileSprites();
    //    }
    //    ////update tile visuals if necessary
    //    //if (this.areaType == AREA_TYPE.DEMONIC_INTRUSION) {
    //    //    removedTile.SetBaseSprite(PlayerManager.Instance.playerAreaFloorSprites[Random.Range(0, PlayerManager.Instance.playerAreaFloorSprites.Length)]);
    //    //    if (coreTile.id != removedTile.id) {
    //    //        removedTile.SetLandmarkTileSprite(new LandmarkStructureSprite(PlayerManager.Instance.playerAreaDefaultStructureSprites[Random.Range(0, PlayerManager.Instance.playerAreaDefaultStructureSprites.Length)], null));
    //    //    }
    //    //} else if (this.areaType == AREA_TYPE.ANCIENT_RUINS) {
    //    //    removedTile.SetBaseSprite(Biomes.Instance.ancienctRuinTiles[Random.Range(0, Biomes.Instance.ancienctRuinTiles.Length)]);
    //    //    if (coreTile.id == removedTile.id) {
    //    //        removedTile.SetLandmarkTileSprite(new LandmarkStructureSprite(LandmarkManager.Instance.ancientRuinTowerSprite, null));
    //    //    } else {
    //    //        if (Utilities.IsEven(tiles.Count)) {
    //    //            removedTile.SetLandmarkTileSprite(new LandmarkStructureSprite(LandmarkManager.Instance.ancientRuinBlockerSprite, null));
    //    //        }
    //    //    }
    //    //}
    //}
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
    private void UpdateBorderColors() {
        for (int i = 0; i < tiles.Count; i++) {
            if (owner == null) {
                Color defaultColor = Color.gray;
                defaultColor.a = 128f / 255f;
                tiles[i].SetBorderColor(defaultColor);
            } else {
                tiles[i].SetBorderColor(owner.factionColor);
            }
        }
    }
    //public void Death() {
    //    if (!isDead) {
    //        isDead = true;
    //        if (owner != null) {
    //            for (int i = 0; i < areaResidents.Count; i++) {
    //                Character resident = areaResidents[i];
    //                if (!resident.isFactionless && !resident.currentParty.icon.isTravelling && resident.faction.id == owner.id && resident.id != resident.faction.leader.id && resident.specificLocation.id == id) {
    //                    resident.Death();
    //                }
    //            }
    //        }
    //        LandmarkManager.Instance.UnownArea(this);
    //        FactionManager.Instance.neutralFaction.AddToOwnedAreas(this);

    //        if (previousOwner != null && previousOwner.leader != null && previousOwner.leader is Character) {
    //            Character leader = previousOwner.leader as Character;
    //            if (!leader.currentParty.icon.isTravelling && leader.specificLocation.id == id && leader.homeArea.id == id) {
    //                leader.Death();
    //            }
    //        }

    //        ReleaseAllAbductedCharacters();
    //        UnsubscribeToSignals();
    //    }
    //}
    private void ReleaseAllAbductedCharacters() {
        for (int i = 0; i < charactersAtLocation.Count; i++) {
            charactersAtLocation[i].ReleaseFromAbduction();
        }
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
        LocationStructure warehouse = GetRandomStructureOfType(STRUCTURE_TYPE.WAREHOUSE);
        CheckAreaInventoryJobs(warehouse);
    }
    //public void SetOutlineState(bool state) {
    //    SpriteRenderer[] borders = coreTile.GetAllBorders();
    //    for (int i = 0; i < borders.Length; i++) {
    //        SpriteRenderer renderer = borders[i];
    //        renderer.gameObject.SetActive(state);
    //    }
    //}
    public bool CanInvadeSettlement() {
        return coreTile.region.HasCorruptedConnection() && PlayerManager.Instance.player.minions.Where(x => x.assignedRegion == null).ToList().Count > 0 && PlayerManager.Instance.player.currentAreaBeingInvaded == null;
    }
    #endregion

    #region Supplies
    public void SetSuppliesInBank(int amount) {
        if (supplyPile == null) {
            return;
        }
        supplyPile.SetSuppliesInPile(amount);
        Messenger.Broadcast(Signals.AREA_SUPPLIES_CHANGED, this);
        //suppliesInBank = Mathf.Clamp(suppliesInBank, 0, supplyCapacity);
    }
    public void AdjustSuppliesInBank(int amount) {
        if (supplyPile == null) {
            return;
        }
        supplyPile.AdjustSuppliesInPile(amount);
        Messenger.Broadcast(Signals.AREA_SUPPLIES_CHANGED, this);
        //suppliesInBank = Mathf.Clamp(suppliesInBank, 0, supplyCapacity);
    }
    #endregion

    #region Food
    public void SetFoodInBank(int amount) {
        FoodPile currFoodPile = foodPile;
        if (currFoodPile == null) {
            return;
        }
        currFoodPile.SetFoodInPile(amount);
        Messenger.Broadcast(Signals.AREA_FOOD_CHANGED, this);
    }
    public void AdjustFoodInBank(int amount) {
        FoodPile currFoodPile = foodPile;
        if (currFoodPile == null) {
            return;
        }
        currFoodPile.AdjustFoodInPile(amount);
        Messenger.Broadcast(Signals.AREA_FOOD_CHANGED, this);
    }
    #endregion

    #region Characters
    public void AssignCharacterToDwellingInArea(Character character, Dwelling dwellingOverride = null) {
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
                Character lover = character.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER);
                if (lover != null && lover.faction.id == character.faction.id && region.residents.Contains(lover)) { //check if the character has a lover that lives in the area
                    chosenDwelling = lover.homeStructure;
                }
            }
            if (chosenDwelling == null && (character.homeStructure == null || character.homeStructure.location.id != this.id)) { //else, find an unoccupied dwelling (also check if the character doesn't already live in this area)
                for (int i = 0; i < structures[STRUCTURE_TYPE.DWELLING].Count; i++) {
                    Dwelling currDwelling = structures[STRUCTURE_TYPE.DWELLING][i] as Dwelling;
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
    //private void CheckForUnoccupancy() {
    //    //whenever an owned area loses a resident, check if the area still has any residents that are part of the owner faction
    //    //if there aren't any, unoccupy this area
    //    if (this.owner != null) {
    //        bool unoccupy = true;
    //        for (int i = 0; i < areaResidents.Count; i++) {
    //            Character currResident = areaResidents[i];
    //            if (currResident.faction.id == this.owner.id) {
    //                unoccupy = false;
    //                break;
    //            }
    //        }
    //        if (unoccupy) {
    //            LandmarkManager.Instance.UnownRegion(region);
    //            FactionManager.Instance.neutralFaction.AddToOwnedRegions(region);
    //        }
    //    }
    //}
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
        return structures[STRUCTURE_TYPE.DWELLING].Where(x => !x.IsOccupied()).Count() == 0; //check if there are still unoccupied dwellings
    }
    public int GetNumberOfUnoccupiedStructure(STRUCTURE_TYPE structureType) {
        if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.playerArea.id == this.id) {
            return 0;
        }
        return structures[structureType].Where(x => !x.IsOccupied()).Count();
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
        CharacterMarker[] markers = Utilities.GetComponentsInDirectChildren<CharacterMarker>(areaMap.objectsParent.gameObject);
        for (int i = 0; i < markers.Length; i++) {
            CharacterMarker currMarker = markers[i];
            if (currMarker.character != null && currMarker.character.isDead) {
                characters.Add(currMarker.character);
            }
        }

        List<TileObject> tombstones = GetTileObjectsOfType(TILE_OBJECT_TYPE.TOMBSTONE);
        for (int i = 0; i < tombstones.Count; i++) {
            characters.Add(tombstones[i].users[0]);
        }
        return characters;
    }
    public void SetInitialResidentCount(int count) {
        citizenCount = count;
    }
    #endregion

    //#region Attack
    //public List<Character> FormCombatCharacters() {
    //    List<Character> residentsAtArea = new List<Character>();
    //    CombatGrid combatGrid = new CombatGrid();
    //    combatGrid.Initialize();
    //    for (int i = 0; i < residents.Count; i++) {
    //        Character resident = residents[i];
    //        if (resident.isIdle && !resident.isLeader
    //            && !resident.characterClass.isNonCombatant
    //            && !resident.isDefender && resident.specificLocation.id == id && resident.currentStructure.isInside) {
    //            if ((owner != null && resident.faction == owner) || (owner == null && resident.faction == FactionManager.Instance.neutralFaction)) {
    //                residentsAtArea.Add(resident);
    //            }
    //        }
    //    }
    //    List<int> frontlineIndexes = new List<int>();
    //    List<int> backlineIndexes = new List<int>();
    //    for (int i = 0; i < residentsAtArea.Count; i++) {
    //        if (residentsAtArea[i].characterClass.combatPosition == COMBAT_POSITION.FRONTLINE) {
    //            frontlineIndexes.Add(i);
    //        } else {
    //            backlineIndexes.Add(i);
    //        }
    //    }
    //    if (frontlineIndexes.Count > 0) {
    //        for (int i = 0; i < frontlineIndexes.Count; i++) {
    //            if (combatGrid.IsPositionFull(COMBAT_POSITION.FRONTLINE)) {
    //                break;
    //            } else {
    //                combatGrid.AssignCharacterToGrid(residentsAtArea[frontlineIndexes[i]]);
    //                frontlineIndexes.RemoveAt(i);
    //                i--;
    //            }
    //        }
    //    }
    //    if (backlineIndexes.Count > 0) {
    //        for (int i = 0; i < backlineIndexes.Count; i++) {
    //            if (combatGrid.IsPositionFull(COMBAT_POSITION.BACKLINE)) {
    //                break;
    //            } else {
    //                combatGrid.AssignCharacterToGrid(residentsAtArea[backlineIndexes[i]]);
    //                backlineIndexes.RemoveAt(i);
    //                i--;
    //            }
    //        }
    //    }
    //    List<Character> attackCharacters = new List<Character>();
    //    for (int i = 0; i < combatGrid.slots.Length; i++) {
    //        if (combatGrid.slots[i].isOccupied) {
    //            if (!attackCharacters.Contains(combatGrid.slots[i].character)) {
    //                attackCharacters.Add(combatGrid.slots[i].character);
    //            }
    //        }
    //    }
    //    return attackCharacters;
    //}
    //#endregion

    #region Special Tokens
    public bool AddSpecialTokenToLocation(SpecialToken token, LocationStructure structure = null, LocationGridTile gridLocation = null) {
        if (!IsItemInventoryFull() && !itemsInArea.Contains(token)) {
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
        /*
         Items are now placed specifically in a structure when spawning at world creation. 
         Randomly place it at any non-Dwelling structure in the location.
         */
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
    public bool IsRequiredByWarehouse(SpecialToken token) {
        if (token.gridTileLocation != null && token.gridTileLocation.structure.structureType == STRUCTURE_TYPE.WAREHOUSE) {
            LocationStructure warehouse = GetRandomStructureOfType(STRUCTURE_TYPE.WAREHOUSE);
            if (token.specialTokenType == SPECIAL_TOKEN.HEALING_POTION) {
                return warehouse.GetItemsOfTypeCount(SPECIAL_TOKEN.HEALING_POTION) <= 2; //item is required by warehouse.
            } else if (token.specialTokenType == SPECIAL_TOKEN.TOOL) {
                return warehouse.GetItemsOfTypeCount(SPECIAL_TOKEN.TOOL) <= 2; //item is required by warehouse.
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
        LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.WAREHOUSE, true);
        LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.INN, true);
        LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.PRISON, true);
        LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.CEMETERY, true);
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
    private void LoadStructures(AreaSaveData data) {
        structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
        if (data.structures == null) {
            return;
        }
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> kvp in data.structures) {
            for (int i = 0; i < kvp.Value.Count; i++) {
                LocationStructure currStructure = kvp.Value[i];
                LandmarkManager.Instance.CreateNewStructureAt(this, currStructure.structureType, currStructure.isInside);
            }
        }
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
    public List<LocationStructure> GetStructuresOfType(STRUCTURE_TYPE structureType, LocationStructure except) {
        if (structures.ContainsKey(structureType)) {
            List<LocationStructure> currentStructures = structures[structureType];
            List<LocationStructure> newStructures = new List<LocationStructure>();
            for (int i = 0; i < currentStructures.Count; i++) {
                if (currentStructures[i] != except) {
                    newStructures.Add(currentStructures[i]);
                }
            }
            return newStructures;
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
    public void PlaceTileObjects() {
        //pre placed objects
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in structures) {
            for (int i = 0; i < keyValuePair.Value.Count; i++) {
                LocationStructure structure = keyValuePair.Value[i];
                if (structure.isFromTemplate) {
                    structure.RegisterPreplacedObjects();
                }
            }
        }

        PlaceOres();
        PlaceSupplyPiles();
        PlaceFoodPiles();
        SpawnFoodNow();

        //magic circle
        if (structures.ContainsKey(STRUCTURE_TYPE.WILDERNESS)) {
            LocationStructure structure = structures[STRUCTURE_TYPE.WILDERNESS][0];
            structure.AddPOI(new MagicCircle(structure));
        }
        //Guitar
        //Each Dwelling has a 40% chance of having one Guitar. Guitar should be placed at an edge tile.
        if (structures.ContainsKey(STRUCTURE_TYPE.DWELLING)) {
            for (int i = 0; i < structures[STRUCTURE_TYPE.DWELLING].Count; i++) {
                LocationStructure currDwelling = structures[STRUCTURE_TYPE.DWELLING][i];
                if (!currDwelling.isFromTemplate) {
                    if (UnityEngine.Random.Range(0, 100) < 40) {
                        currDwelling.AddPOI(new Guitar(currDwelling));
                    }
                }
            }
        }
        //Well
        if (structures.ContainsKey(STRUCTURE_TYPE.WORK_AREA)) {
            for (int i = 0; i < 3; i++) {
                LocationStructure structure = structures[STRUCTURE_TYPE.WORK_AREA][0];
                structure.AddPOI(new WaterWell(structure));
            }
        }
        //Goddess Statue
        if (structures.ContainsKey(STRUCTURE_TYPE.WORK_AREA)) {
            for (int i = 0; i < 4; i++) {
                LocationStructure structure = structures[STRUCTURE_TYPE.WORK_AREA][0];
                structure.AddPOI(new GoddessStatue(structure));
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
                    structure.AddPOI(new Ore(structure), chosenTile);
                } else {
                    break;
                }
            }
        }
    }
    private void PlaceSupplyPiles() {
        if (structures.ContainsKey(STRUCTURE_TYPE.DUNGEON)) {
            for (int i = 0; i < structures[STRUCTURE_TYPE.DUNGEON].Count; i++) {
                LocationStructure structure = structures[STRUCTURE_TYPE.DUNGEON][i];
                structure.AddPOI(new SupplyPile(structure));
            }
        }
        if (structures.ContainsKey(STRUCTURE_TYPE.WAREHOUSE)) {
            for (int i = 0; i < structures[STRUCTURE_TYPE.WAREHOUSE].Count; i++) {
                LocationStructure structure = structures[STRUCTURE_TYPE.WAREHOUSE][i];
                if (!structure.isFromTemplate) {
                    structure.AddPOI(new SupplyPile(structure));
                }
            }
        }
    }
    private void PlaceFoodPiles() {
        if (structures.ContainsKey(STRUCTURE_TYPE.DUNGEON)) {
            for (int i = 0; i < structures[STRUCTURE_TYPE.DUNGEON].Count; i++) {
                LocationStructure structure = structures[STRUCTURE_TYPE.DUNGEON][i];
                structure.AddPOI(new FoodPile(structure));
            }
        }
        if (structures.ContainsKey(STRUCTURE_TYPE.WAREHOUSE)) {
            for (int i = 0; i < structures[STRUCTURE_TYPE.WAREHOUSE].Count; i++) {
                LocationStructure structure = structures[STRUCTURE_TYPE.WAREHOUSE][i];
                FoodPile foodPile = new FoodPile(structure);
                structure.AddPOI(foodPile);
                foodPile.gridTileLocation.SetReservedType(TILE_OBJECT_TYPE.FOOD_PILE);
            }
        }
    }
    private void SpawnFoodNow() {
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
                    structure.AddPOI(new SmallAnimal(structure), chosenTile);
                } else {
                    break;
                }
            }

            for (int i = 0; i < ediblePlantsCount; i++) {
                List<LocationGridTile> validTiles = structure.unoccupiedTiles.Where(x => x.IsAdjacentToPasssableTiles(3)).ToList();
                if (validTiles.Count > 0) {
                    LocationGridTile chosenTile = validTiles[UnityEngine.Random.Range(0, validTiles.Count)];
                    structure.AddPOI(new EdiblePlant(structure), chosenTile);
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
    private void HourlyJobActions() {
        CreatePatrolJobs();
        //if (UnityEngine.Random.Range(0, 100) < 5 && currentMoveOutJobs < maxMoveOutJobs) {
        //    CreateMoveOutJobs();
        //}
    }
    private void CreatePatrolJobs() {
        int patrolChance = UnityEngine.Random.Range(0, 100);
        if (patrolChance < 25 && jobQueue.GetNumberOfJobsWith(CHARACTER_STATE.PATROL) < 2) {
            CharacterStateJob stateJob = new CharacterStateJob(JOB_TYPE.PATROL, CHARACTER_STATE.PATROL, null);
            stateJob.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoPatrolAndExplore);
            jobQueue.AddJobInQueue(stateJob);
        }
    }
    public void CheckAreaInventoryJobs(LocationStructure affectedStructure) {
        if (affectedStructure.structureType == STRUCTURE_TYPE.WAREHOUSE) {
            //brew potion
            //- If there are less than 2 Healing Potions in the Warehouse, it will create a Brew Potion job
            //- the warehouse stores an inventory count of items it needs to keep in stock. anytime an item is added or removed (claimed by someone, stolen or destroyed), inventory will be checked and missing items will be procured
            //- any character that can produce this item may take this Job
            //- cancel Brew Potion job whenever inventory check occurs and it specified that there are enough Healing Potions already
            if (affectedStructure.GetItemsOfTypeCount(SPECIAL_TOKEN.HEALING_POTION) < 2) {
                if (!jobQueue.HasJob(JOB_TYPE.BREW_POTION)) {
                    GoapPlanJob job = new GoapPlanJob(JOB_TYPE.BREW_POTION, INTERACTION_TYPE.DROP_ITEM_WAREHOUSE, new Dictionary<INTERACTION_TYPE, object[]>() {
                        { INTERACTION_TYPE.DROP_ITEM_WAREHOUSE, new object[]{ SPECIAL_TOKEN.HEALING_POTION } },
                        { INTERACTION_TYPE.CRAFT_ITEM, new object[]{ SPECIAL_TOKEN.HEALING_POTION } },
                    });
                    job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanBrewPotion);
                    job.SetOnTakeJobAction(InteractionManager.Instance.OnTakeBrewPotion);
                    //job.SetCannotOverrideJob(false);
                    jobQueue.AddJobInQueue(job);
                }
            } else {
                CancelBrewPotion();
            }

            //craft tool
            if (affectedStructure.GetItemsOfTypeCount(SPECIAL_TOKEN.TOOL) < 2) {
                if (!jobQueue.HasJob(JOB_TYPE.CRAFT_TOOL)) {
                    GoapPlanJob job = new GoapPlanJob(JOB_TYPE.CRAFT_TOOL, INTERACTION_TYPE.DROP_ITEM_WAREHOUSE, new Dictionary<INTERACTION_TYPE, object[]>() {
                        { INTERACTION_TYPE.DROP_ITEM_WAREHOUSE, new object[]{ SPECIAL_TOKEN.TOOL } },
                        { INTERACTION_TYPE.CRAFT_ITEM, new object[]{ SPECIAL_TOKEN.TOOL } },
                    });
                    job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCraftTool);
                    job.SetOnTakeJobAction(InteractionManager.Instance.OnTakeCraftTool);
                    //job.SetCannotOverrideJob(false);
                    jobQueue.AddJobInQueue(job);
                }
            } else {
                CancelCraftTool();
            }
        }
    }
    private void CancelBrewPotion() {
        //warehouse has 2 or more healing potions
        if (jobQueue.HasJob(JOB_TYPE.BREW_POTION)) {
            JobQueueItem brewJob = jobQueue.GetJob(JOB_TYPE.BREW_POTION);
            jobQueue.CancelJob(brewJob, forceRemove: true);
        }
    }
    private void CancelCraftTool() {
        //warehouse has 2 or more healing potions
        if (jobQueue.HasJob(JOB_TYPE.CRAFT_TOOL)) {
            JobQueueItem craftTool = jobQueue.GetJob(JOB_TYPE.CRAFT_TOOL);
            jobQueue.CancelJob(craftTool, forceRemove: true);
        }
    }
    private void CreateReplaceTileObjectJob(TileObject removedObj, LocationGridTile removedFrom) {
        GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REPLACE_TILE_OBJECT, INTERACTION_TYPE.REPLACE_TILE_OBJECT, new Dictionary<INTERACTION_TYPE, object[]>() {
                        { INTERACTION_TYPE.REPLACE_TILE_OBJECT, new object[]{ removedObj, removedFrom } },
        });
        job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeReplaceTileObjectJob);
        job.SetCancelOnFail(false);
        job.SetCancelJobOnDropPlan(false);
        jobQueue.AddJobInQueue(job);
    }
    #endregion

    #region Hero Event Jobs
    private int maxHeroEventJobs {
        get { return region.residents.Count / 5; } //There should be at most 1 Move Out Job per 5 residents
    }
    private int currentHeroEventJobs {
        get { return jobQueue.GetNumberOfJobsWith(IsJobTypeAHeroEventJob); }
    }
    private bool IsJobTypeAHeroEventJob(JobQueueItem item) {
        switch (item.jobType) {
            case JOB_TYPE.OBTAIN_FOOD_OUTSIDE:
            case JOB_TYPE.OBTAIN_SUPPLY_OUTSIDE:
            case JOB_TYPE.IMPROVE:
            case JOB_TYPE.EXPLORE:
            case JOB_TYPE.COMBAT:
                return true;
            default:
                return false;
        }
    }
    private bool CanStillCreateHeroEventJob() {
        return currentHeroEventJobs < maxHeroEventJobs;
    }
    private void CreateMoveOutJobs() {
        CharacterStateJob job = new CharacterStateJob(JOB_TYPE.MOVE_OUT, CHARACTER_STATE.MOVE_OUT);
        job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanMoveOut);
        jobQueue.AddJobInQueue(job);
    }
    /// <summary>
    /// Check if this area should create an obtain food outside job.
    /// Criteria can be found at: https://trello.com/c/cICMVSch/2706-hero-events
    /// </summary>
    private void TryCreateObtainFoodOutsideJob() {
        if (!CanStillCreateHeroEventJob()) {
            return; //hero events are maxed.
        }
        int obtainFoodOutsideJobs = jobQueue.GetNumberOfJobsWith(JOB_TYPE.OBTAIN_FOOD_OUTSIDE);
        if (obtainFoodOutsideJobs == 0 && foodPile.foodInPile < 1000) {
            CreateObtainFoodOutsideJob();
        } else  if (obtainFoodOutsideJobs == 1 && foodPile.foodInPile < 500) { //there is at least 1 existing obtain food outside job.
            //allow the creation of a second obtain food outside job
            CreateObtainFoodOutsideJob();
        }
    }
    private void CreateObtainFoodOutsideJob() {
        CharacterStateJob job = new CharacterStateJob(JOB_TYPE.OBTAIN_FOOD_OUTSIDE, CHARACTER_STATE.MOVE_OUT);
        job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoObtainFoodOutsideJob);
        jobQueue.AddJobInQueue(job);
    }
    /// <summary>
    /// Check if this area should create an obtain supply outside job.
    /// Criteria can be found at: https://trello.com/c/cICMVSch/2706-hero-events
    /// </summary>
    private void TryCreateObtainSupplyOutsideJob() {
        if (!CanStillCreateHeroEventJob()) {
            return; //hero events are maxed.
        }
        int obtainSupplyOutsideJobs = jobQueue.GetNumberOfJobsWith(JOB_TYPE.OBTAIN_SUPPLY_OUTSIDE);
        if (obtainSupplyOutsideJobs == 0 && supplyPile.suppliesInPile < 1000) {
            CreateObtainSupplyOutsideJob();
        } else if (obtainSupplyOutsideJobs == 1 && supplyPile.suppliesInPile < 500) { //there is at least 1 existing obtain supply outside job.
            //allow the creation of a second obtain supply outside job
            CreateObtainSupplyOutsideJob();
        }
    }
    private void CreateObtainSupplyOutsideJob() {
        CharacterStateJob job = new CharacterStateJob(JOB_TYPE.OBTAIN_SUPPLY_OUTSIDE, CHARACTER_STATE.MOVE_OUT);
        job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoObtainSupplyOutsideJob);
        jobQueue.AddJobInQueue(job);
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
            CharacterStateJob job = new CharacterStateJob(JOB_TYPE.IMPROVE, CHARACTER_STATE.MOVE_OUT);
            jobQueue.AddJobInQueue(job);
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
            CharacterStateJob job = new CharacterStateJob(JOB_TYPE.EXPLORE, CHARACTER_STATE.MOVE_OUT);
            //Used lambda expression instead of new function. Reference: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/lambda-expressions
            job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoExploreJob); 
            jobQueue.AddJobInQueue(job);
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
            CharacterStateJob job = new CharacterStateJob(JOB_TYPE.COMBAT, CHARACTER_STATE.MOVE_OUT);
            job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoCombatJob);
            jobQueue.AddJobInQueue(job);
            //expires at midnight
            GameDate expiry = GameManager.Instance.Today();
            expiry.SetTicks(GameManager.Instance.GetTicksBasedOnHour(24));
            SchedulingManager.Instance.AddEntry(expiry, () => CheckIfJobWillExpire(job), this);
        }
    }
    private void CheckIfJobWillExpire(JobQueueItem item) {
        if (item.assignedCharacter == null) {
            Debug.Log(GameManager.Instance.TodayLogString() + item.jobType.ToString() + " expired.");
            item.jobQueueParent.RemoveJobInQueue(item);
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