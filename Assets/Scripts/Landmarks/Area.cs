using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class Area {

    public int id { get; private set; }
    public string name { get; private set; }
    public bool isDead { get; private set; }
    public AREA_TYPE areaType { get; private set; }
    public HexTile coreTile { get; private set; }
    public Color areaColor { get; private set; }
    public Faction owner { get; private set; }
    public Faction previousOwner { get; private set; }
    public List<HexTile> tiles { get; private set; }
    public List<BaseLandmark> landmarks { get { return tiles.Where(x => x.landmarkOnTile != null).Select(x => x.landmarkOnTile).ToList(); } }
    public List<Character> areaResidents { get; private set; }
    public List<Character> charactersAtLocation { get; private set; }
    public List<Log> history { get; private set; }
    public JobQueue jobQueue { get; private set; }
    public LocationStructure prison { get; private set; }
    public List<RACE> possibleOccupants { get; private set; }
    public List<InitialRaceSetup> initialSpawnSetup { get; private set; } //only to be used when unoccupied

    //special tokens
    public List<SpecialToken> possibleSpecialTokenSpawns { get; private set; }
    public const int MAX_ITEM_CAPACITY = 15;

    //structures
    public Dictionary<STRUCTURE_TYPE, List<LocationStructure>> structures { get; private set; }
    public AreaInnerTileMap areaMap { get; private set; }

    //misc
    public Sprite locationPortrait { get; private set; }
    public Vector2 nameplatePos { get; private set; }
    public bool isBeingTracked { get; private set; }

    //for testing
    public List<string> charactersAtLocationHistory { get; private set; }

    public Race defaultRace { get; private set; }
    private RACE _raceType;
    public bool hasBeenInvaded { get; private set; }

    #region getters
    public RACE raceType {
        get { return _raceType; }
    }
    public List<Character> visitors {
        get { return charactersAtLocation.Where(x => !areaResidents.Contains(x)).ToList(); }
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
    public int residentCapacity {
        get {
            if (structures.ContainsKey(STRUCTURE_TYPE.DWELLING)) {
                return structures[STRUCTURE_TYPE.DWELLING].Count;
            }
            return 0;
        }
    }
    #endregion

    public Area(HexTile coreTile, AREA_TYPE areaType) {
        id = Utilities.SetID(this);
        SetName(RandomNameGenerator.Instance.GetRegionName());
        hasBeenInvaded = false;
        tiles = new List<HexTile>();
        areaResidents = new List<Character>();
        charactersAtLocation = new List<Character>();
        history = new List<Log>();
        areaColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        initialSpawnSetup = new List<InitialRaceSetup>();
        defaultRace = new Race(RACE.HUMANS, RACE_SUB_TYPE.NORMAL);
        possibleSpecialTokenSpawns = new List<SpecialToken>();
        charactersAtLocationHistory = new List<string>();
        structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
        jobQueue = new JobQueue(null);
        SetAreaType(areaType);
        SetCoreTile(coreTile);
        AddTile(coreTile);
        nameplatePos = LandmarkManager.Instance.GetAreaNameplatePosition(this);
        possibleOccupants = new List<RACE>();
    }
    public Area(AreaSaveData data) {
        hasBeenInvaded = false;
        id = Utilities.SetID(this, data.areaID);
        SetName(data.areaName);
        tiles = new List<HexTile>();
        areaResidents = new List<Character>();
        charactersAtLocation = new List<Character>();
        history = new List<Log>();
        areaColor = data.areaColor;
        SetAreaType(data.areaType);
        charactersAtLocationHistory = new List<string>();
        possibleSpecialTokenSpawns = new List<SpecialToken>();
        jobQueue = new JobQueue(null);
        if (data.raceSetup != null) {
            initialSpawnSetup = new List<InitialRaceSetup>(data.raceSetup);
        } else {
            initialSpawnSetup = new List<InitialRaceSetup>();
        }
        LoadStructures(data);
        AssignPrison();
#if WORLD_CREATION_TOOL
        SetCoreTile(worldcreator.WorldCreatorManager.Instance.GetHexTile(data.coreTileID));
#else
        SetCoreTile(GridMap.Instance.GetHexTile(data.coreTileID));
#endif
        possibleOccupants = new List<RACE>();
        if (data.possibleOccupants != null) {
            possibleOccupants.AddRange(data.possibleOccupants);
        }

        AddTile(Utilities.GetTilesFromIDs(data.tileData));
        UpdateBorderColors();
        GenerateDefaultRace();
        nameplatePos = LandmarkManager.Instance.GetAreaNameplatePosition(this);
    }

    #region Listeners
    private void SubscribeToSignals() {
        Messenger.AddListener(Signals.HOUR_STARTED, CreatePatrolAndExploreJobs);
        Messenger.AddListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
    }
    private void UnsubscribeToSignals() {
        if (Messenger.eventTable.ContainsKey(Signals.HOUR_STARTED)) {
            Messenger.RemoveListener(Signals.HOUR_STARTED, CreatePatrolAndExploreJobs);
        }
        Messenger.RemoveListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
    }
    private void OnTileObjectRemoved(TileObject removedObj, Character character, LocationGridTile removedFrom) {
        if (removedFrom.parentAreaMap.area.id == this.id) {
            if (removedObj.CanBeReplaced()) {
                CreateReplaceTileObjectJob(removedObj, removedFrom);
            }
        }
    }
    #endregion

    #region Area Details
    public void SetName(string name) {
        this.name = name;
    }
    //public void SetDefaultRace(RACE race) {
    //    defaultRace = race;
    //}
    public void AddPossibleOccupant(RACE race) {
        possibleOccupants.Add(race);
    }
    public void RemovePossibleOccupant(RACE race) {
        possibleOccupants.Remove(race);
    }
    public bool HasRaceSetup(RACE race, RACE_SUB_TYPE subType) {
        for (int i = 0; i < initialSpawnSetup.Count; i++) {
            InitialRaceSetup raceSetup = initialSpawnSetup[i];
            if (raceSetup.race.race == race && raceSetup.race.subType == subType) {
                return true;
            }
        }
        return false;
    }
    public void AddRaceSetup(RACE race, RACE_SUB_TYPE subType) {
        Race newRace = new Race(race, subType);
        initialSpawnSetup.Add(new InitialRaceSetup(newRace));
    }
    public void RemoveRaceSetup(RACE race, RACE_SUB_TYPE subType) {
        for (int i = 0; i < initialSpawnSetup.Count; i++) {
            InitialRaceSetup raceSetup = initialSpawnSetup[i];
            if (raceSetup.race.race == race && raceSetup.race.subType == subType) {
                initialSpawnSetup.RemoveAt(i);
                break;
            }
        }
    }
    private void GenerateDefaultRace() {
        if (initialSpawnSetup.Count > 0) {
            InitialRaceSetup chosenSetup = initialSpawnSetup[UnityEngine.Random.Range(0, initialSpawnSetup.Count)];
            defaultRace = chosenSetup.race;
            SetRaceType(defaultRace.race);
        } else {
            defaultRace = new Race(RACE.NONE, RACE_SUB_TYPE.NORMAL);
        }
    }
    #endregion

    #region Tile Management
    public void SetCoreTile(HexTile tile) {
        coreTile = tile;
    }
    public void AddTile(List<HexTile> tiles) {
        for (int i = 0; i < tiles.Count; i++) {
            AddTile(tiles[i]);
        }
    }
    public void AddTile(HexTile tile) {
        if (!tiles.Contains(tile)) {
            tiles.Add(tile);
            tile.SetArea(this);
            if (this.coreTile == null) {
                SetCoreTile(tile);
            }
            OnTileAddedToArea(tile);
            Messenger.Broadcast(Signals.AREA_TILE_ADDED, this, tile);
        }
    }
    public void RemoveTile(List<HexTile> tiles) {
        for (int i = 0; i < tiles.Count; i++) {
            RemoveTile(tiles[i]);
        }
    }
    public void RemoveTile(HexTile tile) {
        if (tiles.Remove(tile)) {
            tile.SetArea(null);
            if (coreTile == tile) {
                SetCoreTile(null);
            }
            OnTileRemovedFromArea(tile);
            Messenger.Broadcast(Signals.AREA_TILE_REMOVED, this, tile);
        }
    }
    private void OnTileAddedToArea(HexTile addedTile) {
        if (this.areaType == AREA_TYPE.ANCIENT_RUINS) {
            addedTile.SetBiome(BIOMES.ANCIENT_RUIN);
            Biomes.Instance.UpdateTileVisuals(addedTile);
        }
        //update tile visuals if necessary
        if (this.areaType == AREA_TYPE.DEMONIC_INTRUSION) {
            Biomes.Instance.CorruptTileVisuals(addedTile);
        }
    }
    private void OnTileRemovedFromArea(HexTile removedTile) {
        if (this.areaType == AREA_TYPE.ANCIENT_RUINS) {
            removedTile.SetBaseSprite(Biomes.Instance.ancienctRuinTiles[UnityEngine.Random.Range(0, Biomes.Instance.ancienctRuinTiles.Length)]);
            removedTile.HideLandmarkTileSprites();
        }
        ////update tile visuals if necessary
        //if (this.areaType == AREA_TYPE.DEMONIC_INTRUSION) {
        //    removedTile.SetBaseSprite(PlayerManager.Instance.playerAreaFloorSprites[Random.Range(0, PlayerManager.Instance.playerAreaFloorSprites.Length)]);
        //    if (coreTile.id != removedTile.id) {
        //        removedTile.SetLandmarkTileSprite(new LandmarkStructureSprite(PlayerManager.Instance.playerAreaDefaultStructureSprites[Random.Range(0, PlayerManager.Instance.playerAreaDefaultStructureSprites.Length)], null));
        //    }
        //} else if (this.areaType == AREA_TYPE.ANCIENT_RUINS) {
        //    removedTile.SetBaseSprite(Biomes.Instance.ancienctRuinTiles[Random.Range(0, Biomes.Instance.ancienctRuinTiles.Length)]);
        //    if (coreTile.id == removedTile.id) {
        //        removedTile.SetLandmarkTileSprite(new LandmarkStructureSprite(LandmarkManager.Instance.ancientRuinTowerSprite, null));
        //    } else {
        //        if (Utilities.IsEven(tiles.Count)) {
        //            removedTile.SetLandmarkTileSprite(new LandmarkStructureSprite(LandmarkManager.Instance.ancientRuinBlockerSprite, null));
        //        }
        //    }
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
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currTile = tiles[i];
            OnTileAddedToArea(currTile);
        }
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
#else
            if (currTile.id == coreTile.id) {
                currTile.HighlightTile(areaColor, 255f / 255f);
            } else {
                currTile.HighlightTile(areaColor, 128f / 255f);
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

    #region Owner
    public void SetOwner(Faction owner) {
        previousOwner = this.owner;
        this.owner = owner;
        //if(areaType != AREA_TYPE.DEMONIC_INTRUSION) {
        //    if (this.owner != null &&  this.owner != FactionManager.Instance.neutralFaction) {
        //        SubscribeToSignals();
        //    } else {
        //        UnsubscribeToSignals();
        //    }
        //}
        UpdateBorderColors();
        /*Whenever a location is occupied, 
         all items in structures Inside Settlement will be owned by the occupying faction.*/
        List<LocationStructure> insideStructures = GetStructuresAtLocation(true);
        for (int i = 0; i < insideStructures.Count; i++) {
            insideStructures[i].OwnItemsInLocation(owner);
        }
        Messenger.Broadcast(Signals.AREA_OWNER_CHANGED, this);
    }
    public void SetRaceType(RACE raceType) {
        _raceType = raceType;
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
    public void Death() {
        if (!isDead) {
            isDead = true;
            if (owner != null) {
                for (int i = 0; i < areaResidents.Count; i++) {
                    Character resident = areaResidents[i];
                    if (!resident.isFactionless && !resident.currentParty.icon.isTravelling && resident.faction.id == owner.id && resident.id != resident.faction.leader.id && resident.specificLocation.id == id) {
                        resident.Death();
                    }
                }
            }
            LandmarkManager.Instance.UnownArea(this);
            FactionManager.Instance.neutralFaction.AddToOwnedAreas(this);

            if (previousOwner != null && previousOwner.leader != null && previousOwner.leader is Character) {
                Character leader = previousOwner.leader as Character;
                if (!leader.currentParty.icon.isTravelling && leader.specificLocation.id == id && leader.homeArea.id == id) {
                    leader.Death();
                }
            }

            ReleaseAllAbductedCharacters();
            UnsubscribeToSignals();
        }
    }
    private void ReleaseAllAbductedCharacters() {
        for (int i = 0; i < charactersAtLocation.Count; i++) {
            charactersAtLocation[i].ReleaseFromAbduction();
        }
    }
    public string GetAreaTypeString() {
        if (areaType == AREA_TYPE.DEMONIC_INTRUSION) {
            return "Demonic Portal";
        }
        if (_raceType != RACE.NONE) {
            if (tiles.Count > 1) {
                return Utilities.GetNormalizedRaceAdjective(_raceType) + " " + Utilities.NormalizeStringUpperCaseFirstLetters(GetBaseAreaType().ToString());
            } else {
                return Utilities.GetNormalizedRaceAdjective(_raceType) + " " + Utilities.NormalizeStringUpperCaseFirstLetters(coreTile.landmarkOnTile.specificLandmarkType.ToString());
            }
        } else {
            return Utilities.NormalizeStringUpperCaseFirstLetters(coreTile.landmarkOnTile.specificLandmarkType.ToString());
        }
    }
    /// <summary>
    /// Called when this area is set as the current active area.
    /// </summary>
    public void OnAreaSetAsActive() {
        SubscribeToSignals();
        LocationStructure warehouse = GetRandomStructureOfType(STRUCTURE_TYPE.WAREHOUSE);
        CheckAreaInventoryJobs(warehouse);
    }
    public void SetHasBeenInvaded(bool state) {
        hasBeenInvaded = state;
    }
    public void SetOutlineState(bool state) {
        SpriteRenderer[] borders = coreTile.GetAllBorders();
        for (int i = 0; i < borders.Length; i++) {
            SpriteRenderer renderer = borders[i];
            renderer.gameObject.SetActive(state);
        }
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

    #region Landmarks
    public void CenterOnCoreLandmark() {
        CameraMove.Instance.CenterCameraOn(coreTile.gameObject);
    }
    #endregion

    #region Characters
    public bool IsResident(Character character) {
        return areaResidents.Contains(character);
    }
    public void AddResident(Character character, Dwelling chosenHome = null, bool ignoreCapacity = true) {
        if (!areaResidents.Contains(character)) {
            if (!ignoreCapacity) {
                if (IsResidentsFull()) {
                    Debug.LogWarning(GameManager.Instance.TodayLogString() + "Cannot add " + character.name + " as resident of " + this.name + " because residency is already full!");
                    return; //area is at capacity
                }
            }
            character.SetHome(this);
            areaResidents.Add(character);
            //if (PlayerManager.Instance.player == null || PlayerManager.Instance.player.playerArea.id != this.id) {
#if !WORLD_CREATION_TOOL
            AssignCharacterToDwellingInArea(character, chosenHome);
#endif
            //}
            //Messenger.Broadcast(Signals.AREA_RESIDENT_ADDED, this, character);
        }
    }
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
                if (lover != null && lover.faction.id == character.faction.id && areaResidents.Contains(lover)) { //check if the character has a lover that lives in the area
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
    public void RemoveResident(Character character) {
        if (areaResidents.Remove(character)) {
            character.SetHome(null);
            if (character.homeArea != null) {
                character.homeStructure.RemoveResident(character);
            }
            CheckForUnoccupancy();
            //Messenger.Broadcast(Signals.AREA_RESIDENT_REMOVED, this, character);
        }
    }
    private void CheckForUnoccupancy() {
        //whenever an owned area loses a resident, check if the area still has any residents that are part of the owner faction
        //if there aren't any, unoccupy this area
        if (this.owner != null) {
            bool unoccupy = true;
            for (int i = 0; i < areaResidents.Count; i++) {
                Character currResident = areaResidents[i];
                if (currResident.faction.id == this.owner.id) {
                    unoccupy = false;
                    break;
                }
            }
            if (unoccupy) {
                LandmarkManager.Instance.UnownArea(this);
                FactionManager.Instance.neutralFaction.AddToOwnedAreas(this);
            }
        }
    }
    public void AddCharacterToLocation(Character character, LocationGridTile tileOverride = null, bool isInitial = false) {
        if (!charactersAtLocation.Contains(character)) {
            charactersAtLocation.Add(character);
            character.ownParty.SetSpecificLocation(this);
            AddCharacterAtLocationHistory("Added " + character.name + "ST: " + StackTraceUtility.ExtractStackTrace());
            //if (tileOverride != null) {
            //    tileOverride.structure.AddCharacterAtLocation(character, tileOverride);
            //} else {
            //    if (isInitial) {
            //        AddCharacterToAppropriateStructure(character);
            //    } else {
            //        LocationGridTile exit = GetRandomUnoccupiedEdgeTile();
            //        exit.structure.AddCharacterAtLocation(character, exit);
            //    }
            //}
            Debug.Log(GameManager.Instance.TodayLogString() + "Added " + character.name + " to location " + name);
            Messenger.Broadcast(Signals.CHARACTER_ENTERED_AREA, this, character);
        }
    }
    public void RemoveCharacterFromLocation(Character character) {
        if (charactersAtLocation.Remove(character)) {
            //character.ownParty.SetSpecificLocation(null);
            if (character.currentStructure == null && this != PlayerManager.Instance.player.playerArea) {
                throw new Exception(character.name + " doesn't have a current structure at " + this.name);
            }
            if (character.currentStructure != null) {
                character.currentStructure.RemoveCharacterAtLocation(character);
            }
            AddCharacterAtLocationHistory("Removed " + character.name + "ST: " + StackTraceUtility.ExtractStackTrace());
            Messenger.Broadcast(Signals.CHARACTER_EXITED_AREA, this, character);
        }

    }
    public void RemoveCharacterFromLocation(Party party) {
        RemoveCharacterFromLocation(party.owner);
        //for (int i = 0; i < party.characters.Count; i++) {
        //    RemoveCharacterFromLocation(party.characters[i]);
        //}
    }
    //public void AddCharacterToAppropriateStructure(Character character) {
    //    if (character.GetTraitOr("Abducted", "Restrained") != null) {
    //        GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA).AddCharacterAtLocation(character);
    //    } else {
    //        if (character.homeArea.id == this.id) {
    //            if (character.homeStructure == null) {
    //                //throw new Exception(character.name + "'s homeStructure is null!");
    //                if (UnityEngine.Random.Range(0, 2) == 0) {
    //                    LocationStructure wilderness = GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
    //                    if (wilderness != null) {
    //                        wilderness.AddCharacterAtLocation(character);
    //                    } else {
    //                        GetRandomStructureOfType(STRUCTURE_TYPE.DUNGEON).AddCharacterAtLocation(character);
    //                    }
    //                } else {
    //                    LocationStructure dungeon = GetRandomStructureOfType(STRUCTURE_TYPE.DUNGEON);
    //                    if (dungeon != null) {
    //                        dungeon.AddCharacterAtLocation(character);
    //                    } else {
    //                        GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS).AddCharacterAtLocation(character);
    //                    }
    //                }
    //            } else {
    //                //If this is his home, the character will be placed in his Dwelling.
    //                character.homeStructure.AddCharacterAtLocation(character);
    //            }
    //        } else {
    //            // Otherwise:
    //            if (Utilities.IsRaceBeast(character.race)) {
    //                //- Beasts will be placed at a random Wilderness.
    //                GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS).AddCharacterAtLocation(character);
    //            } else if (this.owner != null) {
    //                FACTION_RELATIONSHIP_STATUS relStat;
    //                if (character.faction.id == this.owner.id) { //character is part of the same faction as the owner of this area
    //                    relStat = FACTION_RELATIONSHIP_STATUS.ALLY;
    //                } else {
    //                    relStat = character.faction.GetRelationshipWith(this.owner).relationshipStatus;
    //                }
    //                switch (relStat) {
    //                    case FACTION_RELATIONSHIP_STATUS.AT_WAR:
    //                    case FACTION_RELATIONSHIP_STATUS.ENEMY:
    //                        //- If location is occupied, non-beasts whose faction relationship is Enemy or worse will be placed in a random structure Outside Settlement.
    //                        List<LocationStructure> choices = GetStructuresAtLocation(false);
    //                        choices[UnityEngine.Random.Range(0, choices.Count)].AddCharacterAtLocation(character);
    //                        break;
    //                    case FACTION_RELATIONSHIP_STATUS.DISLIKED:
    //                    case FACTION_RELATIONSHIP_STATUS.NEUTRAL:
    //                    case FACTION_RELATIONSHIP_STATUS.FRIEND:
    //                    case FACTION_RELATIONSHIP_STATUS.ALLY:
    //                        LocationStructure inn = GetRandomStructureOfType(STRUCTURE_TYPE.INN);
    //                        if (inn != null) {
    //                            //- If location is occupied, non-beasts whose faction relationship is Disliked or better will be placed at the Inn. 
    //                            inn.AddCharacterAtLocation(character);
    //                        } else {
    //                            //If no Inn in the Location, he will be placed in a random Wilderness.
    //                            LocationStructure wilderness = GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
    //                            wilderness.AddCharacterAtLocation(character);
    //                        }
    //                        break;
    //                }
    //            } else {
    //                //- If location is unoccupied, non-beasts will be placed at a random Wilderness.
    //                GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS).AddCharacterAtLocation(character);
    //            }
    //        }
    //    }
    //    if (character.currentStructure == null) {
    //        Debug.LogWarning(GameManager.Instance.TodayLogString() + "Could not find structure for " + character.name + " at " + this.name);
    //    } else {
    //        if (character.currentStructure != character.gridTileLocation.structure && character.marker != null) {
    //            LocationGridTile tile = character.currentStructure.GetRandomUnoccupiedTile();
    //            character.marker.PlaceMarkerAt(tile);
    //        }
    //    }
    //}
    private void AddCharacterAtLocationHistory(string str) {
#if !WORLD_CREATION_TOOL
        charactersAtLocationHistory.Add(GameManager.Instance.TodayLogString() + str);
        if (charactersAtLocationHistory.Count > 100) {
            charactersAtLocationHistory.RemoveAt(0);
        }
#endif
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
    #endregion

    #region Logs
    public void AddHistory(Log log) {
        if (!history.Contains(log)) {
            history.Add(log);
            if (this.history.Count > 60) {
                if (this.history[0].goapAction != null) {
                    this.history[0].goapAction.AdjustReferenceCount(-1);
                }
                this.history.RemoveAt(0);
            }
            if (log.goapAction != null) {
                log.goapAction.AdjustReferenceCount(1);
            }
            Messenger.Broadcast(Signals.HISTORY_ADDED, this as object);
        }
    }
    #endregion

    #region Attack
    public List<Character> FormCombatCharacters() {
        List<Character> residentsAtArea = new List<Character>();
        CombatGrid combatGrid = new CombatGrid();
        combatGrid.Initialize();
        for (int i = 0; i < areaResidents.Count; i++) {
            Character resident = areaResidents[i];
            if (resident.isIdle && !resident.isLeader
                && !resident.characterClass.isNonCombatant
                && !resident.isDefender && resident.specificLocation.id == id && resident.currentStructure.isInside) {
                if ((owner != null && resident.faction == owner) || (owner == null && resident.faction == FactionManager.Instance.neutralFaction)) {
                    residentsAtArea.Add(resident);
                }
            }
        }
        List<int> frontlineIndexes = new List<int>();
        List<int> backlineIndexes = new List<int>();
        for (int i = 0; i < residentsAtArea.Count; i++) {
            if (residentsAtArea[i].characterClass.combatPosition == COMBAT_POSITION.FRONTLINE) {
                frontlineIndexes.Add(i);
            } else {
                backlineIndexes.Add(i);
            }
        }
        if (frontlineIndexes.Count > 0) {
            for (int i = 0; i < frontlineIndexes.Count; i++) {
                if (combatGrid.IsPositionFull(COMBAT_POSITION.FRONTLINE)) {
                    break;
                } else {
                    combatGrid.AssignCharacterToGrid(residentsAtArea[frontlineIndexes[i]]);
                    frontlineIndexes.RemoveAt(i);
                    i--;
                }
            }
        }
        if (backlineIndexes.Count > 0) {
            for (int i = 0; i < backlineIndexes.Count; i++) {
                if (combatGrid.IsPositionFull(COMBAT_POSITION.BACKLINE)) {
                    break;
                } else {
                    combatGrid.AssignCharacterToGrid(residentsAtArea[backlineIndexes[i]]);
                    backlineIndexes.RemoveAt(i);
                    i--;
                }
            }
        }
        List<Character> attackCharacters = new List<Character>();
        for (int i = 0; i < combatGrid.slots.Length; i++) {
            if (combatGrid.slots[i].isOccupied) {
                if (!attackCharacters.Contains(combatGrid.slots[i].character)) {
                    attackCharacters.Add(combatGrid.slots[i].character);
                }
            }
        }
        return attackCharacters;
    }
    #endregion

    #region Special Tokens
    public bool AddSpecialTokenToLocation(SpecialToken token, LocationStructure structure = null, LocationGridTile gridLocation = null) {
        if (!IsItemInventoryFull() && !possibleSpecialTokenSpawns.Contains(token)) {
            possibleSpecialTokenSpawns.Add(token);
            if (areaMap != null) { //if the area map of this area has already been created.
                //Debug.Log(GameManager.Instance.TodayLogString() + "Added " + token.name + " at " + name);
                if (structure != null) {
                    structure.AddItem(token, gridLocation);
                    if (structure.isInside) {
                        token.SetOwner(this.owner);
                    }
                } else {
                    //get structure for token
                    LocationStructure chosen = GetRandomStructureToPlaceItem(token);
                    chosen.AddItem(token);
                    if (chosen.isInside) {
                        token.SetOwner(this.owner);
                    }
                }
                OnItemAddedToLocation(token, token.structureLocation);
            }            
            Messenger.Broadcast(Signals.ITEM_ADDED_TO_AREA, this, token);
            return true;
        }
        return false;
    }
    public void RemoveSpecialTokenFromLocation(SpecialToken token) {
        if (possibleSpecialTokenSpawns.Remove(token)) {
            LocationStructure takenFrom = token.structureLocation;
            if (takenFrom != null) {
                takenFrom.RemoveItem(token);
                OnItemRemovedFromLocation(token, takenFrom);
            }
            Debug.Log(GameManager.Instance.TodayLogString() + "Removed " + token.name + " from " + name);
            Messenger.Broadcast(Signals.ITEM_REMOVED_FROM_AREA, this, token);
        }

    }
    public bool IsItemInventoryFull() {
        return possibleSpecialTokenSpawns.Count >= MAX_ITEM_CAPACITY;
    }
    private LocationStructure GetRandomStructureToPlaceItem(SpecialToken token) {
        /*
         Items are now placed specifically in a structure when spawning at world creation. 
         Randomly place it at any non-Dwelling structure in the location.
         */
        List<LocationStructure> choices = new List<LocationStructure>();
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> kvp in structures) {
            if (kvp.Key != STRUCTURE_TYPE.DWELLING && kvp.Key != STRUCTURE_TYPE.EXIT && kvp.Key != STRUCTURE_TYPE.CEMETERY
                && kvp.Key != STRUCTURE_TYPE.INN && kvp.Key != STRUCTURE_TYPE.WORK_AREA && kvp.Key != STRUCTURE_TYPE.PRISON) {
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
        for (int i = 0; i < possibleSpecialTokenSpawns.Count; i++) {
            SpecialToken currItem = possibleSpecialTokenSpawns[i];
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
        for (int i = 0; i < citizenCount; i++) {
            LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.DWELLING, true);
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
    public void PlaceTileObjects() { //TODO: Unify placement of static POI's
        //pre placed objects
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in structures) {
            for (int i = 0; i < keyValuePair.Value.Count; i++) {
                LocationStructure structure = keyValuePair.Value[i];
                if (structure.isFromTemplate) {
                    structure.RegisterPreplacedObjects();
                }
            }
        }

        //PlaceBedsAndTables();
        PlaceOres();
        PlaceSupplyPiles();
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

    }
    //private void PlaceBedsAndTables() {
    //    if (structures.ContainsKey(STRUCTURE_TYPE.DWELLING)) {
    //        for (int i = 0; i < structures[STRUCTURE_TYPE.DWELLING].Count; i++) {
    //            LocationStructure structure = structures[STRUCTURE_TYPE.DWELLING][i];
    //            if (!structure.isFromTemplate) {
    //                structure.AddPOI(new Bed(structure));
    //                structure.AddPOI(new Table(structure));
    //            }
    //        }
    //    }
    //    if (structures.ContainsKey(STRUCTURE_TYPE.INN)) {
    //        int randomInnTables = UnityEngine.Random.Range(2, 5);
    //        for (int i = 0; i < structures[STRUCTURE_TYPE.INN].Count; i++) {
    //            LocationStructure structure = structures[STRUCTURE_TYPE.INN][i];
    //            if (!structure.isFromTemplate) {
    //                for (int j = 0; j < randomInnTables; j++) {
    //                    structure.AddPOI(new Table(structure));
    //                }
    //            }
    //        }
    //    }
    //}
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
        if(unoccupiedEdgeTiles.Count > 0) {
            return unoccupiedEdgeTiles[UnityEngine.Random.Range(0, unoccupiedEdgeTiles.Count)];
        }
        return null;
    }
    private void AssignPrison() {
        if (areaType == AREA_TYPE.DEMONIC_INTRUSION) {
            return;
        }
        LocationStructure chosenPrison = GetRandomStructureOfType(STRUCTURE_TYPE.PRISON);
        if(chosenPrison != null) {
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
    #endregion

    #region Jobs
    private void CreatePatrolAndExploreJobs() {
        int patrolChance = UnityEngine.Random.Range(0, 100);
        if(patrolChance < 25 && jobQueue.GetNumberOfJobsWith(CHARACTER_STATE.PATROL) < 2) {
            CharacterStateJob stateJob = new CharacterStateJob(JOB_TYPE.PATROL, CHARACTER_STATE.PATROL, null);
            stateJob.SetCanTakeThisJobChecker(CanDoPatrolAndExplore);
            jobQueue.AddJobInQueue(stateJob);
        }

        //int exploreChance = UnityEngine.Random.Range(0, 100);
        //if (exploreChance < 15 && !jobQueue.HasJobRelatedTo(CHARACTER_STATE.EXPLORE)) {
        //    Area dungeon = LandmarkManager.Instance.GetRandomAreaOfType(AREA_TYPE.DUNGEON);
        //    CharacterStateJob stateJob = new CharacterStateJob(JOB_TYPE.EXPLORE, CHARACTER_STATE.EXPLORE, dungeon);
        //    //stateJob.SetOnTakeJobAction(OnTakeExploreJob);
        //    stateJob.SetCanTakeThisJobChecker(CanDoPatrolAndExplore);
        //    jobQueue.AddJobInQueue(stateJob);
        //}
    }
    private bool CanDoPatrolAndExplore(Character character, JobQueueItem job) {
        return character.GetNormalTrait("Injured") == null;
    }
    //private void OnTakeExploreJob(Character character) {
    //    //Explorers should pick up a Tool and a Healing Potion before leaving
    //    GoapPlanJob toolJob = new GoapPlanJob("Get Tool", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = SPECIAL_TOKEN.TOOL.ToString(), targetPOI = character });
    //    character.jobQueue.AddJobInQueue(toolJob, true);
    //    GoapPlanJob potionJob = new GoapPlanJob("Get Potion", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = SPECIAL_TOKEN.HEALING_POTION.ToString(), targetPOI = character });
    //    character.jobQueue.AddJobInQueue(potionJob, true);
    //}
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
                    });
                    job.SetCanTakeThisJobChecker(CanBrewPotion);
                    job.SetOnTakeJobAction(OnTakeBrewPotion);
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
                    });
                    job.SetCanTakeThisJobChecker(CanCraftTool);
                    job.SetOnTakeJobAction(OnTakeCraftTool);
                    //job.SetCannotOverrideJob(false);
                    jobQueue.AddJobInQueue(job);
                }
            } else {
                CancelCraftTool();
            }
        }        
    }
    private bool CanCraftTool(Character character, JobQueueItem job) {
        //return character.HasExtraTokenInInventory(SPECIAL_TOKEN.TOOL);
        return SPECIAL_TOKEN.TOOL.CanBeCraftedBy(character);
    }
    private bool CanBrewPotion(Character character, JobQueueItem job) {
        //return character.HasExtraTokenInInventory(SPECIAL_TOKEN.HEALING_POTION);
        return SPECIAL_TOKEN.HEALING_POTION.CanBeCraftedBy(character);
    }
    private void OnTakeBrewPotion(Character character, JobQueueItem job) {
        GoapPlanJob j = job as GoapPlanJob;
        j.ClearForcedActions();
        j.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = SPECIAL_TOKEN.HEALING_POTION.ToString(), targetPOI = character }, INTERACTION_TYPE.CRAFT_ITEM);
    }
    private void OnTakeCraftTool(Character character, JobQueueItem job) {
        GoapPlanJob j = job as GoapPlanJob;
        j.ClearForcedActions();
        j.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = SPECIAL_TOKEN.TOOL.ToString(), targetPOI = character }, INTERACTION_TYPE.CRAFT_ITEM);
    }
    private void CancelBrewPotion() {
        //warehouse has 2 or more healing potions
        if (jobQueue.HasJob(JOB_TYPE.BREW_POTION)) {
            JobQueueItem brewJob = jobQueue.GetJob(JOB_TYPE.BREW_POTION);
            jobQueue.CancelJob(brewJob);
        }
    }
    private void CancelCraftTool() {
        //warehouse has 2 or more healing potions
        if (jobQueue.HasJob(JOB_TYPE.CRAFT_TOOL)) {
            JobQueueItem craftTool = jobQueue.GetJob(JOB_TYPE.CRAFT_TOOL);
            jobQueue.CancelJob(craftTool);
        }
    }
    private void CreateReplaceTileObjectJob(TileObject removedObj, LocationGridTile removedFrom) {
        GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REPLACE_TILE_OBJECT, INTERACTION_TYPE.REPLACE_TILE_OBJECT, new Dictionary<INTERACTION_TYPE, object[]>() {
                        { INTERACTION_TYPE.REPLACE_TILE_OBJECT, new object[]{ removedObj, removedFrom } },
                    });
        //job.SetCanTakeThisJobChecker(job.CanCraftItemChecker);
        //job.SetCannotOverrideJob(false);
        jobQueue.AddJobInQueue(job);
    }
    #endregion

    #region Area Map
    public void OnMapGenerationFinished() {
        //place tokens in area to actual structures.
        //get structure for token
        for (int i = 0; i < possibleSpecialTokenSpawns.Count; i++) {
            SpecialToken token = possibleSpecialTokenSpawns[i];
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