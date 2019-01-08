using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class Area {

    public int id { get; private set; }
    public int totalCivilians { get { return landmarks.Sum(x => x.civilianCount); } }
    public int suppliesInBank { get; private set; }
    public int supplyCapacity { get; private set; }
    public string name { get; private set; }
    public bool isHighlighted { get; private set; }
    public bool hasBeenInspected { get; private set; }
    public bool areAllLandmarksDead { get; private set; }
    public bool stopDefaultAllExistingInteractions { get; private set; }
    public AREA_TYPE areaType { get; private set; }
    public HexTile coreTile { get; private set; }
    public Color areaColor { get; private set; }
    public Faction owner { get; private set; }
    public Faction previousOwner { get; private set; }
    public Area attackTarget { get; private set; }
    public AreaInvestigation areaInvestigation { get; private set; }
    public LocationToken locationToken { get; private set; }
    public DefenderToken defenderToken { get; private set; }
    public List<HexTile> tiles { get; private set; }
    public List<BaseLandmark> landmarks { get { return tiles.Where(x => x.landmarkOnTile != null).Select(x => x.landmarkOnTile).ToList(); } }
    public List<BaseLandmark> exposedTiles { get; private set; }
    public List<BaseLandmark> unexposedTiles { get; private set; }
    public List<Character> areaResidents { get; private set; }
    public List<Character> charactersAtLocation { get; private set; }
    public List<Character> attackCharacters { get; private set; }
    public List<Log> history { get; private set; }
    public List<Interaction> currentInteractions { get; private set; }
    public Dictionary<INTERACTION_TYPE, int> areaTasksInteractionWeights { get; private set; }
    public int initialResidents { get; private set; }

    //defenders
    public int maxDefenderGroups { get; private set; }
    public int initialDefenderGroups { get; private set; }
    public List<DefenderGroup> defenderGroups { get; private set; }
    public List<RACE> possibleOccupants { get; private set; }
    public List<InitialRaceSetup> initialRaceSetup { get; private set; }
    public Dictionary<JOB, List<INTERACTION_TYPE>> jobInteractionTypes { get; private set; }
    public int initialSupply { get; private set; } //this should not change when scavenging
    public int residentCapacity { get; private set; }
    public int monthlySupply { get; private set; }
    public List<Interaction> eventsTargettingThis { get; private set; }

    //special tokens
    public List<SpecialToken> possibleSpecialTokenSpawns { get; private set; }

    //misc
    public Sprite locationPortrait { get; private set; }
    public Vector2 nameplatePos { get; private set; }

    //for testing
    public List<string> supplyLog { get; private set; } //limited to 100 entries
    public List<string> charactersAtLocationHistory { get; private set; }

    private Race defaultRace;
    private RACE _raceType;
    private List<HexTile> outerTiles;
    private List<SpriteRenderer> outline;

    #region getters
    public RACE raceType {
        get { return owner == null ? defaultRace.race : _raceType; }
    }
    public Race race {
        get { return owner == null ? defaultRace : owner.race; }
    }
    //public int elligibleResidents {
    //    get { return areaResidents.Where(x => !x.isDefender).Count(); }
    //}
    #endregion

    public Area(HexTile coreTile, AREA_TYPE areaType) {
        id = Utilities.SetID(this);
        SetName(RandomNameGenerator.Instance.GetRegionName());
        tiles = new List<HexTile>();
        areaResidents = new List<Character>();
        charactersAtLocation = new List<Character>();
        exposedTiles = new List<BaseLandmark>();
        unexposedTiles = new List<BaseLandmark>();
        defenderGroups = new List<DefenderGroup>();
        history = new List<Log>();
        currentInteractions = new List<Interaction>();
        areaColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        locationToken = new LocationToken(this);
        defenderToken = new DefenderToken(this);
        areaInvestigation = new AreaInvestigation(this);
        jobInteractionTypes = new Dictionary<JOB, List<INTERACTION_TYPE>>();
        initialRaceSetup = new List<InitialRaceSetup>();
        eventsTargettingThis = new List<Interaction>();
        supplyLog = new List<string>();
        defaultRace = new Race(RACE.HUMANS, RACE_SUB_TYPE.NORMAL);
        possibleSpecialTokenSpawns = new List<SpecialToken>();
        charactersAtLocationHistory = new List<string>();
        SetAreaType(areaType);
        SetCoreTile(coreTile);
        //SetSupplyCapacity(1000);
        AddTile(coreTile);
        SetSuppliesInBank(1000);
        if (areaType != AREA_TYPE.DEMONIC_INTRUSION) {
            Messenger.AddListener(Signals.DAY_ENDED_2, DefaultAllExistingInteractions);
        }
#if !WORLD_CREATION_TOOL
        ConstructAreaTasksInteractionWeights();
        StartSupplyLine();
#endif
        nameplatePos = LandmarkManager.Instance.GetAreaNameplatePosition(this);
    }
    public Area(AreaSaveData data) {
        id = Utilities.SetID(this, data.areaID);
        SetName(data.areaName);
        tiles = new List<HexTile>();
        areaResidents = new List<Character>();
        charactersAtLocation = new List<Character>();
        exposedTiles = new List<BaseLandmark>();
        unexposedTiles = new List<BaseLandmark>();
        defenderGroups = new List<DefenderGroup>();
        history = new List<Log>();
        currentInteractions = new List<Interaction>();
        areaColor = data.areaColor;
        SetAreaType(data.areaType);
        locationToken = new LocationToken(this);
        defenderToken = new DefenderToken(this);
        areaInvestigation = new AreaInvestigation(this);
        jobInteractionTypes = new Dictionary<JOB, List<INTERACTION_TYPE>>();
        eventsTargettingThis = new List<Interaction>();
        charactersAtLocationHistory = new List<string>();
        possibleSpecialTokenSpawns = new List<SpecialToken>();
        supplyLog = new List<string>();
        if (data.raceSetup != null) {
            initialRaceSetup = new List<InitialRaceSetup>(data.raceSetup);
        } else {
            initialRaceSetup = new List<InitialRaceSetup>();
        }
        SetMaxDefenderGroups(data.maxDefenderGroups);
        SetInitialDefenderGroups(data.initialDefenderGroups);
        SetResidentCapacity(data.residentCapacity);
        SetMonthlySupply(data.monthlySupply);
        SetInitialResidents(data.initialResidents);
#if WORLD_CREATION_TOOL
        SetCoreTile(worldcreator.WorldCreatorManager.Instance.GetHexTile(data.coreTileID));
#else
        SetSuppliesInBank(data.monthlySupply);
        SetCoreTile(GridMap.Instance.GetHexTile(data.coreTileID));
        ConstructAreaTasksInteractionWeights();
        StartSupplyLine();
#endif

        possibleOccupants = new List<RACE>();
        if (data.possibleOccupants != null) {
            possibleOccupants.AddRange(data.possibleOccupants);
        }
        //LoadSpecialTokens(data);
        AddTile(Utilities.GetTilesFromIDs(data.tileData)); //exposed tiles will be determined after loading landmarks at MapGeneration
        UpdateBorderColors();
        if (areaType != AREA_TYPE.DEMONIC_INTRUSION) {
            Messenger.AddListener(Signals.DAY_ENDED_2, DefaultAllExistingInteractions);
        }
        GenerateDefaultRace();
        nameplatePos = LandmarkManager.Instance.GetAreaNameplatePosition(this);
    }

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
        for (int i = 0; i < initialRaceSetup.Count; i++) {
            InitialRaceSetup raceSetup = initialRaceSetup[i];
            if (raceSetup.race.race == race && raceSetup.race.subType == subType) {
                return true;
            }
        }
        return false;
    }
    public void AddRaceSetup(RACE race, RACE_SUB_TYPE subType) {
        Race newRace = new Race(race, subType);
        initialRaceSetup.Add(new InitialRaceSetup(newRace));
    }
    public void RemoveRaceSetup(RACE race, RACE_SUB_TYPE subType) {
        for (int i = 0; i < initialRaceSetup.Count; i++) {
            InitialRaceSetup raceSetup = initialRaceSetup[i];
            if (raceSetup.race.race == race && raceSetup.race.subType == subType) {
                initialRaceSetup.RemoveAt(i);
                break;
            }
        }
    }
    private InitialRaceSetup GetRaceSetup(Race race) {
        for (int i = 0; i < initialRaceSetup.Count; i++) {
            InitialRaceSetup raceSetup = initialRaceSetup[i];
            if (raceSetup.race.race == race.race && raceSetup.race.subType == race.subType) {
                return raceSetup;
            }
        }
        return null;
    }
    //public void SetInitialSupplies(int amount) {
    //    initialSupply = amount;
    //}
    public void SetResidentCapacity(int amount) {
        residentCapacity = amount;
    }
    public void SetMonthlySupply(int amount) {
        monthlySupply = amount;
    }
    private void GenerateDefaultRace() {
        if (initialRaceSetup.Count > 0) {
            InitialRaceSetup chosenSetup = initialRaceSetup[UnityEngine.Random.Range(0, initialRaceSetup.Count)];
            defaultRace = chosenSetup.race;
        } else {
            defaultRace = new Race(RACE.NONE, RACE_SUB_TYPE.NORMAL);
        }
    }
    #endregion

    #region Tile Management
    public void SetCoreTile(HexTile tile) {
        coreTile = tile;
    }
    public void AddTile(List<HexTile> tiles, bool determineOuterTiles = true) {
        for (int i = 0; i < tiles.Count; i++) {
            AddTile(tiles[i], false);
        }
#if !WORLD_CREATION_TOOL
        //if (determineExposedTiles) {
        //    DetermineExposedTiles();
        //}
        if (determineOuterTiles) {
            UpdateOuterTiles();
        }
#endif
    }
    public void AddTile(HexTile tile, bool determineOuterTiles = true) {
        if (!tiles.Contains(tile)) {
            tiles.Add(tile);
            tile.SetArea(this);
            tile.SetMinimapTileColor(areaColor);
#if !WORLD_CREATION_TOOL
            DetermineIfTileIsExposed(tile);
            if (determineOuterTiles) {
                UpdateOuterTiles();
            }
#endif
            OnTileAddedToArea(tile);
            Messenger.Broadcast(Signals.AREA_TILE_ADDED, this, tile);
            CheckDeath();
        }
    }
    public void RemoveTile(List<HexTile> tiles, bool determineOuterTiles = true) {
        for (int i = 0; i < tiles.Count; i++) {
            RemoveTile(tiles[i], false, false);
        }
#if !WORLD_CREATION_TOOL
        //if (determineExposedTiles) {
        //    DetermineExposedTiles();
        //}
        if (determineOuterTiles) {
            UpdateOuterTiles();
        }
#endif
    }
    public void RemoveTile(HexTile tile, bool determineExposedTiles = true, bool determineOuterTiles = true) {
        if (tiles.Remove(tile)) {
            tile.SetArea(null);
            OnTileRemovedFromArea(tile);
#if !WORLD_CREATION_TOOL
            if(tile.landmarkOnTile != null) {
                if (!exposedTiles.Remove(tile.landmarkOnTile)) {
                    unexposedTiles.Remove(tile.landmarkOnTile);
                }
            }
            if (determineOuterTiles) {
                UpdateOuterTiles();
            }
#endif
            Messenger.Broadcast(Signals.AREA_TILE_REMOVED, this, tile);
        }
    }
    //private void RevalidateTiles() {
    //    List<HexTile> tilesToCheck = new List<HexTile>(tiles);
    //    tilesToCheck.Remove(coreTile);
    //    while (tilesToCheck.Count != 0) {
    //        HexTile currTile = tilesToCheck[0];
    //        if (PathGenerator.Instance.GetPath(currTile, coreTile, PATHFINDING_MODE.AREA_ONLY, this) == null) {
    //            RemoveTile(currTile); //Remove tile from area
    //            currTile.UnHighlightTile();
    //        }
    //        tilesToCheck.Remove(currTile);
    //    }
    //}
    public List<HexTile> GetAdjacentBuildableTiles() {
        List<HexTile> elligibleTiles = new List<HexTile>();
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currTile = tiles[i];
            for (int j = 0; j < currTile.AllNeighbours.Count; j++) {
                HexTile currNeighbour = currTile.AllNeighbours[j];
                if (currNeighbour.isPassable && currNeighbour.landmarkOnTile == null && currNeighbour.areaOfTile == null && !elligibleTiles.Contains(currNeighbour)) {
                    elligibleTiles.Add(currNeighbour);
                }
            }
        }
        return elligibleTiles;
    }
    private void OnTileAddedToArea(HexTile addedTile) {
        if (this.areaType == AREA_TYPE.ANCIENT_RUINS) {
            addedTile.SetBiome(BIOMES.ANCIENT_RUIN);
            Biomes.Instance.UpdateTileVisuals(addedTile);
        }
        //update tile visuals if necessary
        if (this.areaType == AREA_TYPE.DEMONIC_INTRUSION) {
            //addedTile.SetBaseSprite(PlayerManager.Instance.playerAreaFloorSprites[Random.Range(0, PlayerManager.Instance.playerAreaFloorSprites.Length)]);
            //if (coreTile.id != addedTile.id) {
            //    addedTile.SetLandmarkTileSprite(new LandmarkStructureSprite(PlayerManager.Instance.playerAreaDefaultStructureSprites[Random.Range(0, PlayerManager.Instance.playerAreaDefaultStructureSprites.Length)], null));
            //}
            Biomes.Instance.CorruptTileVisuals(addedTile);
        } else if (this.areaType == AREA_TYPE.ANCIENT_RUINS) {
            //addedTile.SetBaseSprite(Biomes.Instance.ancienctRuinTiles[Random.Range(0, Biomes.Instance.ancienctRuinTiles.Length)]);
            if (coreTile.id == addedTile.id) {
                addedTile.SetLandmarkTileSprite(new LandmarkStructureSprite(LandmarkManager.Instance.ancientRuinTowerSprite, null));
            } else {
                if (Utilities.IsEven(tiles.Count)) {
                    addedTile.SetLandmarkTileSprite(new LandmarkStructureSprite(LandmarkManager.Instance.ancientRuinBlockerSprite, null));
                }
            }
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
    public void DetermineIfTileIsExposed(HexTile currTile) {
        if (currTile.landmarkOnTile == null) {
            return; //if there is no landmark on the tile, or it's landmark is already ruined, do not count as exposed
        }
        //check if the tile has a flat empty tile as a neighbour
        //if it does, it is an exposed tile
        bool isExposed = false;
        for (int j = 0; j < currTile.AllNeighbours.Count; j++) {
            HexTile currNeighbour = currTile.AllNeighbours[j];
            if (!isExposed && (currNeighbour.landmarkOnTile == null)
                && currNeighbour.elevationType == ELEVATION.PLAIN) {
                unexposedTiles.Remove(currTile.landmarkOnTile);
                if (!exposedTiles.Contains(currTile.landmarkOnTile)) {
                    exposedTiles.Add(currTile.landmarkOnTile);
                    currTile.SetExternalState(true);
                    currTile.SetInternalState(false);
                }
                isExposed = true;
            }
            DetermineIfNeighborTileIsExposed(currNeighbour);
        }
        if (!isExposed) {
            exposedTiles.Remove(currTile.landmarkOnTile);
            if (!unexposedTiles.Contains(currTile.landmarkOnTile)) {
                unexposedTiles.Add(currTile.landmarkOnTile);
                currTile.SetExternalState(false);
                currTile.SetInternalState(true);
            }
        }
    }
    public void DetermineIfNeighborTileIsExposed(HexTile currTile) {
        if (currTile.landmarkOnTile == null || currTile.areaOfTile == null || (currTile.areaOfTile != null && currTile.areaOfTile != this)) {
            return; //if there is no landmark on the tile, or it's landmark is already ruined, do not count as exposed
        }
        //check if the tile has a flat empty tile as a neighbour
        //if it does, it is an exposed tile
        bool isExposed = false;
        for (int j = 0; j < currTile.AllNeighbours.Count; j++) {
            HexTile currNeighbour = currTile.AllNeighbours[j];
            if ((currNeighbour.landmarkOnTile == null)
                && currNeighbour.elevationType == ELEVATION.PLAIN) {
                unexposedTiles.Remove(currTile.landmarkOnTile);
                if (!exposedTiles.Contains(currTile.landmarkOnTile)) {
                    exposedTiles.Add(currTile.landmarkOnTile);
                    currTile.SetExternalState(true);
                    currTile.SetInternalState(false);
                }
                isExposed = true;
                break;
            }

        }
        if (!isExposed) {
            exposedTiles.Remove(currTile.landmarkOnTile);
            if (!unexposedTiles.Contains(currTile.landmarkOnTile)) {
                unexposedTiles.Add(currTile.landmarkOnTile);
                currTile.SetExternalState(false);
                currTile.SetInternalState(true);
            }
        }
    }
    public BaseLandmark GetRandomExposedLandmark() {
        if (exposedTiles.Count > 0) {
            return exposedTiles[UnityEngine.Random.Range(0, exposedTiles.Count)];
        }
        return null;
    }
    private void UpdateOuterTiles() {
        outerTiles = new List<HexTile>();
        outline = new List<SpriteRenderer>();
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currTile = tiles[i];
            HEXTILE_DIRECTION[] neighbourDirections = Utilities.GetEnumValues<HEXTILE_DIRECTION>();
            for (int j = 0; j < neighbourDirections.Length; j++) {
                HEXTILE_DIRECTION currDirection = neighbourDirections[j];
                HexTile currNeighbour = currTile.GetNeighbour(currDirection);
                if (currNeighbour == null || currNeighbour.areaOfTile == null || currNeighbour.areaOfTile.id != this.id) {
                    if (!outerTiles.Contains(currTile)) {
                        outerTiles.Add(currTile);
                    }
                    SpriteRenderer border = currTile.GetBorder(currDirection);
                    if (border != null) {
                        outline.Add(border);
                    }
                }
            }
            //for (int j = 0; j < currTile.AllNeighbours.Count; j++) {
            //    HexTile currNeighbour = currTile.AllNeighbours[j];
            //    if (currNeighbour.areaOfTile == null || currNeighbour.areaOfTile.id != this.id) {
            //        if (!outerTiles.Contains(currTile)) {
            //            outerTiles.Add(currTile);
            //        }
            //        outline.Add(currTile.GetBorder(currTile.GetNeighbourDirection(currNeighbour)));
            //    }
            //}
        }
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
                currTile.HighlightTile(areaColor, 255f/255f);
            } else {
                currTile.HighlightTile(areaColor, 128f/255f);
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
    public void SetOutlineState(bool state) {
        isHighlighted = state;
        for (int i = 0; i < outline.Count; i++) {
            SpriteRenderer renderer = outline[i];
            renderer.gameObject.SetActive(state);
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
        UpdateBorderColors();
        if (owner != null) {
            for (int i = 0; i < landmarks.Count; i++) {
                landmarks[i].OccupyLandmark(owner);
            }
        } else {
            for (int i = 0; i < landmarks.Count; i++) {
                landmarks[i].UnoccupyLandmark();
            }
        }
        Messenger.Broadcast(Signals.AREA_OWNER_CHANGED, this);
    }
    public void SetRaceType(RACE raceType) {
        _raceType = raceType;
    }
    #endregion

     #region Utilities
    public void LoadAdditionalData() {
        //DetermineExposedTiles();
        Messenger.AddListener<StructureObj, ObjectState>(Signals.STRUCTURE_STATE_CHANGED, OnStructureStateChanged);
        //Messenger.AddListener<Interaction>(Signals.INTERACTION_ENDED, RemoveEventTargettingThis); 
        //GenerateInitialDefenders();
        GenerateInitialResidents();
        CreateNameplate();
    }
    public bool HasLandmarkOfType(LANDMARK_TYPE type) {
        return landmarks.Where(x => x.specificLandmarkType == type).Any();
    }
    //private void StartOfMonth() {
    //    if(orderClasses.Count > 0) {
    //        UpdateExcessAndMissingClasses();
    //        ScheduleStartOfMonthActions();
    //    }
    //}
    //private void ScheduleStartOfMonthActions() {
    //    GameDate gameDate = GameManager.Instance.Today();
    //    gameDate.SetHours(1);
    //    gameDate.AddDays(1);
    //    SchedulingManager.Instance.AddEntry(gameDate, () => StartOfMonth());
    //}
    //private void ScheduleFirstAction() {
    //    GameDate gameDate = new GameDate(1, 1, GameManager.Instance.year, 1);
    //    SchedulingManager.Instance.AddEntry(gameDate, () => StartOfMonth());
    //}
    private void OnStructureStateChanged(StructureObj structureObj, ObjectState state) {
        if (structureObj.objectLocation == null) {
            return;
        }
        if (tiles.Contains(structureObj.objectLocation.tileLocation)) {
            if (state.stateName.Equals("Ruined")) {
                DetermineIfTileIsExposed(structureObj.objectLocation.tileLocation);
                if (this.areaType == AREA_TYPE.DEMONIC_INTRUSION) { //if player area
                    PlayerManager.Instance.player.OnPlayerLandmarkRuined(structureObj.objectLocation);
                }
            }
        }
    }
    private void UpdateBorderColors() {
        for (int i = 0; i < tiles.Count; i++) {
            if (owner == null) {
                Color defaultColor = Color.gray;
                defaultColor.a = 128f/255f;
                tiles[i].SetBorderColor(defaultColor);
            } else {
                tiles[i].SetBorderColor(owner.factionColor);
            }
        }
    }
    public void SetHasBeenInspected(bool state) {
        hasBeenInspected = state;
        //if (state) {
        //    if (owner != null && owner.id != PlayerManager.Instance.player.playerFaction.id) {
        //        PlayerManager.Instance.player.AddIntel(owner.factionIntel);
        //    }
        //    if (id != PlayerManager.Instance.player.playerArea.id) {
        //        PlayerManager.Instance.player.AddIntel(locationIntel);
        //    }
        //}
    }
    public void CheckDeath() {
        for (int i = 0; i < tiles.Count; i++) {
            if (tiles[i].landmarkOnTile != null) {
                areAllLandmarksDead = false;
                return;
            }
        }
        areAllLandmarksDead = true;
    }
    public void SetAttackTargetAndCharacters(Area target, List<Character> characters) {
        attackTarget = target;
        attackCharacters = characters;
    }
    public void Death() {
        if (owner != null) {
            for (int i = 0; i < areaResidents.Count; i++) {
                Character resident = areaResidents[i];
                if (resident.faction.name != "Neutral" && !resident.currentParty.icon.isTravelling && resident.faction.id == owner.id && resident.id != resident.faction.leader.id && resident.specificLocation.tileLocation.areaOfTile.id == id) {
                    resident.Death();
                }
            }
            LandmarkManager.Instance.UnownArea(this);
        }

        FactionManager.Instance.neutralFaction.OwnArea(this);

        if (previousOwner != null && previousOwner.leader != null && previousOwner.leader is Character) {
            Character leader = previousOwner.leader as Character;
            if (!leader.currentParty.icon.isTravelling && leader.specificLocation.tileLocation.areaOfTile.id == id && leader.homeLandmark.tileLocation.areaOfTile.id == id) {
                leader.Death();
            }
        }
    }
    public bool IsHostileTowards(Character character) {
        if (character.faction.isNeutral || this.owner == null || this.owner.id == character.faction.id) {
            return false;
        }
        FACTION_RELATIONSHIP_STATUS relStat = FactionManager.Instance.GetRelationshipStatusBetween(character.faction, this.owner);
        if (relStat == FACTION_RELATIONSHIP_STATUS.ENEMY || relStat == FACTION_RELATIONSHIP_STATUS.ENEMY) {
            return true;
        }
        return false;
    }
    public List<Area> GetElligibleExpansionTargets(Character expander) {
        List<Area> targets = new List<Area>();
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area currArea = LandmarkManager.Instance.allAreas[i];
            if (currArea.id != PlayerManager.Instance.player.playerArea.id 
                && currArea.owner == null
                && currArea.possibleOccupants.Contains(expander.race)
                && currArea.id != this.id
                && currArea.GetEventsOfTypeTargettingThis(INTERACTION_TYPE.MOVE_TO_EXPAND).Count == 0) {
                targets.Add(currArea);
            }
        }
        return targets;
    }
    public string GetAreaTypeString() {
        if (race.race != RACE.NONE) {
            if (tiles.Count > 1) {
                return Utilities.GetNormalizedSingularRace(race.race) + " " + Utilities.NormalizeStringUpperCaseFirstLetters(GetBaseAreaType().ToString());
            } else {
                return Utilities.GetNormalizedSingularRace(race.race) + " " + Utilities.NormalizeStringUpperCaseFirstLetters(coreTile.landmarkOnTile.specificLandmarkType.ToString());
            }
        } else {
            return Utilities.NormalizeStringUpperCaseFirstLetters(coreTile.landmarkOnTile.specificLandmarkType.ToString());
        }
    }
    #endregion

    #region Camp
    public BaseLandmark CreateCampOnTile(HexTile tile) {
        tile.SetArea(this);
        BaseLandmark camp = LandmarkManager.Instance.CreateNewLandmarkOnTile(tile, LANDMARK_TYPE.CAMP);
        return camp;
    }
    public BaseLandmark CreateCampForHouse(HexTile houseTile) {
        HexTile campsite = GetCampsiteForHouse(houseTile);
        return CreateCampOnTile(campsite);
    }
    private HexTile GetCampsiteForHouse(HexTile houseTile) {
        HexTile chosenTile = null;
        for (int i = 0; i < houseTile.AllNeighbours.Count; i++) {
            HexTile neighbor = houseTile.AllNeighbours[i];
            if (neighbor.isPassable && neighbor.landmarkOnTile == null) {
                chosenTile = neighbor;
                break;
            }
        }
        if(chosenTile != null) {
            return chosenTile;
        }

        List<HexTile> potentialCampsites = new List<HexTile>();
        for (int i = 0; i < landmarks.Count; i++) {
            for (int j = 0; j < landmarks[i].tileLocation.AllNeighbours.Count; j++) {
                HexTile neighbor = landmarks[i].tileLocation.AllNeighbours[j];
                if(neighbor.isPassable && neighbor.landmarkOnTile == null) {
                    potentialCampsites.Add(neighbor);
                }
            }
        }
        chosenTile = potentialCampsites[UnityEngine.Random.Range(0, potentialCampsites.Count)];
        return chosenTile;
    }
    #endregion

    #region Supplies
    private void StartSupplyLine() {
        //AdjustSuppliesInBank(100);
        Messenger.AddListener(Signals.MONTH_START, StartMonthActions);
    }
    public void StopSupplyLine() {
        Messenger.RemoveListener(Signals.MONTH_START, StartMonthActions);
    }
    private void StartMonthActions() {
        CollectMonthlySupplies();

        List<Character> defenderCandidates = new List<Character>();
        List<Character> interactionCandidates = new List<Character>();

        for (int i = 0; i < areaResidents.Count; i++) {
            Character resident = areaResidents[i];
            if (resident.doNotDisturb <= 0 && resident.IsInOwnParty() && !resident.currentParty.icon.isTravelling && resident.faction == owner && resident.specificLocation.tileLocation.areaOfTile.id == id) {
                if(attackCharacters != null && attackCharacters.Contains(resident)) {
                    continue;
                }
                if (resident.forcedInteraction == null || (resident.forcedInteraction != null && resident.forcedInteraction.type != INTERACTION_TYPE.MOVE_TO_ATTACK)) {
                    defenderCandidates.Add(resident);
                }
                if (!resident.isDefender) {
                    interactionCandidates.Add(resident);
                }
            }
        }
        AssignMonthlyDefenders(defenderCandidates, interactionCandidates);
        AssignInteractionsToResidents(interactionCandidates);
    }
    private void CollectMonthlySupplies() {
        SetSuppliesInBank(monthlySupply);
        if(areaInvestigation.assignedMinion != null) {
            areaInvestigation.assignedMinion.character.job.DoPassiveEffect(this);
        }
    }
    //private void LandmarkStartMonthActions() {
    //    for (int i = 0; i < landmarks.Count; i++) {
    //        BaseLandmark currLandmark = landmarks[i];
    //        currLandmark.landmarkObj.StartMonthAction();
    //    }
    //}
    public void SetSuppliesInBank(int amount) {
        suppliesInBank = amount;
        suppliesInBank = Mathf.Clamp(suppliesInBank, 0, monthlySupply);
        Debug.Log(GameManager.Instance.TodayLogString() + "Set " + this.name + " supplies to " + suppliesInBank.ToString());
        Messenger.Broadcast(Signals.AREA_SUPPLIES_CHANGED, this);
        //suppliesInBank = Mathf.Clamp(suppliesInBank, 0, supplyCapacity);
    }
    public void AdjustSuppliesInBank(int amount) {
        suppliesInBank += amount;
        suppliesInBank = Mathf.Clamp(suppliesInBank, 0, monthlySupply);
        supplyLog.Add(GameManager.Instance.TodayLogString() + "Adjusted supplies in bank by " + amount.ToString() +
            " ST: " + StackTraceUtility.ExtractStackTrace());
        if (supplyLog.Count > 100) {
            supplyLog.RemoveAt(0);
        }
        Messenger.Broadcast(Signals.AREA_SUPPLIES_CHANGED, this);
        //suppliesInBank = Mathf.Clamp(suppliesInBank, 0, supplyCapacity);
    }
    public bool HasEnoughSupplies(int neededSupplies) {
        return suppliesInBank >= neededSupplies;
    }
    public void PayForReward(Reward reward) {
        if (reward.rewardType == REWARD.SUPPLY) {
            AdjustSuppliesInBank(-reward.amount);
        }
    }
    //public void SetSupplyCapacity(int supplyCapacity) {
    //    this.supplyCapacity = supplyCapacity;
    //    //suppliesInBank = Mathf.Clamp(suppliesInBank, 0, supplyCapacity);
    //}
    #endregion

    #region Rewards
    public void ClaimReward(Reward reward) {
        switch (reward.rewardType) {
            case REWARD.SUPPLY:
            AdjustSuppliesInBank(reward.amount);
            break;
            default:
            break;
        }
    }
    #endregion

    #region Landmarks
    //public BaseLandmark GetFirstAliveExposedTile() {
    //    for (int i = 0; i < exposedTiles.Count; i++) {
    //        if (!exposedTiles[i].landmarkObj.isRuined) {
    //            return exposedTiles[i];
    //        }
    //    }
    //    return null;
    //}
    public void CenterOnCoreLandmark() {
        CameraMove.Instance.CenterCameraOn(coreTile.gameObject);
    }
    #endregion

    #region Interactions
    private void ConstructAreaTasksInteractionWeights() {
        areaTasksInteractionWeights = new Dictionary<INTERACTION_TYPE, int>() {
            {INTERACTION_TYPE.MOVE_TO_EXPLORE, 100},
            {INTERACTION_TYPE.MOVE_TO_EXPAND, 15},
            {INTERACTION_TYPE.MOVE_TO_SCAVENGE, 60},
            {INTERACTION_TYPE.MOVE_TO_RAID, 40},
            {INTERACTION_TYPE.MOVE_TO_CHARM, 35},
            {INTERACTION_TYPE.MOVE_TO_RECRUIT, 35},
            {INTERACTION_TYPE.MOVE_TO_ABDUCT, 25},
            {INTERACTION_TYPE.MOVE_TO_STEAL, 20},
            {INTERACTION_TYPE.MOVE_TO_HUNT, 20},
            {INTERACTION_TYPE.MOVE_TO_IMPROVE_RELATIONS, 40},
            {INTERACTION_TYPE.PATROL_ACTION, 50},
            {INTERACTION_TYPE.EAT_ABDUCTED, 20},
            {INTERACTION_TYPE.TORTURE_ACTION, 25},
            {INTERACTION_TYPE.MOVE_TO_SPREAD_UNDEATH, 20},
        };
    }
    public void AddInteraction(Interaction interaction) {
        if(currentInteractions.Count > 0) {
            int interactionToBeAddedIndex = Utilities.GetInteractionPriorityIndex(interaction.type);
            bool hasBeenInserted = false;
            if (interactionToBeAddedIndex != -1) {
                for (int i = 0; i < currentInteractions.Count; i++) {
                    int currentInteractionIndex = Utilities.GetInteractionPriorityIndex(currentInteractions[i].type);
                    if (currentInteractionIndex == -1 || interactionToBeAddedIndex < currentInteractionIndex) {
                        hasBeenInserted = true;
                        currentInteractions.Insert(i, interaction);
                        break;
                    }
                }
            }
            if (!hasBeenInserted) {
                currentInteractions.Add(interaction);
            }
        } else {
            currentInteractions.Add(interaction);
        }
        if (interaction.characterInvolved != null) {
            interaction.characterInvolved.currentInteractions.Add(interaction);
        }
        interaction.interactable.currentInteractions.Add(interaction);
        //interaction.Initialize();
        //Messenger.Broadcast(Signals.ADDED_INTERACTION, this as IInteractable, interaction);
    }
    public void RemoveInteraction(Interaction interaction) {
        if (currentInteractions.Remove(interaction)) {
            if (interaction.characterInvolved != null) {
                interaction.characterInvolved.currentInteractions.Remove(interaction);
            }
            interaction.interactable.currentInteractions.Remove(interaction);
            //Messenger.Broadcast(Signals.REMOVED_INTERACTION, this as IInteractable, interaction);
        }
    }
    public void DefaultAllExistingInteractions() {
        if(stopDefaultAllExistingInteractions) { return; }
        for (int i = 0; i < currentInteractions.Count; i++) {
            if (!currentInteractions[i].hasActivatedTimeOut) {
                currentInteractions[i].TimedOutRunDefault();
                i--;
            }
        }
    }
    public void SetStopDefaultInteractionsState(bool state) {
        stopDefaultAllExistingInteractions = stopDefaultAllExistingInteractions;
    }
    public List<Interaction> GetInteractionsOfJob(JOB jobType) {
        List<Interaction> choices = new List<Interaction>();
        for (int i = 0; i < currentInteractions.Count; i++) {
            Interaction interaction = currentInteractions[i];
            if (interaction.DoesJobTypeFitsJobFilter(jobType)) {
                choices.Add(interaction);
            }
        }
        return choices;
    }
    public void AddEventTargettingThis(Interaction interaction) {
        if (!eventsTargettingThis.Contains(interaction)) {
            eventsTargettingThis.Add(interaction);
        }
    }
    public void RemoveEventTargettingThis(Interaction interaction) {
        eventsTargettingThis.Remove(interaction);
    }
    public List<Interaction> GetEventsOfTypeTargettingThis(INTERACTION_TYPE type) {
        List<Interaction> events = new List<Interaction>();
        for (int i = 0; i < eventsTargettingThis.Count; i++) {
            if (eventsTargettingThis[i].type == type) {
                events.Add(eventsTargettingThis[i]);
            }
        }
        return events;
    }
    private void AssignInteractionsToResidents(List<Character> candidates) {
        string testLog = "[Day " + GameManager.Instance.continuousDays + "] AREA TASKS FOR " + this.name;
        if (owner != null) {
            int supplySpent = 0;
            if(candidates.Count <= 0) {
                testLog += "\nNo available residents to be chosen!";
            } else {
                testLog += "\nAvailable residents to choose from: " + candidates[0].name;
                for (int i = 1; i < candidates.Count; i++) {
                    testLog += ", " + candidates[i].name;
                }
                while (candidates.Count > 0) {
                    int index = UnityEngine.Random.Range(0, candidates.Count);
                    Character chosenCandidate = candidates[index];
                    candidates.RemoveAt(index);
                    testLog += "\nChosen Resident: " + chosenCandidate.name;
                    string validInteractionsLog = string.Empty;
                    INTERACTION_TYPE interactionType = GetInteractionTypeForResidentCharacter(chosenCandidate, out validInteractionsLog);
                    testLog += validInteractionsLog;
                    Interaction interaction = null;
                    if (interactionType != INTERACTION_TYPE.MOVE_TO_RAID && interactionType != INTERACTION_TYPE.MOVE_TO_SCAVENGE) {
                        supplySpent += 100;
                        if (supplySpent <= suppliesInBank) {
                            testLog += "\nChosen Interaction: " + interactionType.ToString();
                            interaction = InteractionManager.Instance.CreateNewInteraction(interactionType, chosenCandidate.specificLocation as BaseLandmark);
                            interaction.SetCanInteractionBeDoneAction(() => CanDoAreaTaskInteraction(interaction.type, chosenCandidate));
                            interaction.SetInitializeAction(() => AdjustSuppliesInBank(-100));
                            interaction.SetMinionSuccessAction(() => AdjustSuppliesInBank(100));
                            chosenCandidate.SetForcedInteraction(interaction);
                            if (supplySpent == suppliesInBank) {
                                testLog += "\nAssigning area tasks will stop, all area supplies have been allotted.";
                                break;
                            }
                        } else {
                            testLog += "\nCan't do " + interactionType.ToString() + "! Area cannot accomodate supply cost anymore!";
                            break;
                        }
                    } else {
                        interaction = InteractionManager.Instance.CreateNewInteraction(interactionType, chosenCandidate.specificLocation as BaseLandmark);
                        interaction.SetCanInteractionBeDoneAction(() => InteractionManager.Instance.CanCreateInteraction(interaction.type, chosenCandidate));
                        chosenCandidate.SetForcedInteraction(interaction);
                    }
                }
            }
        } else {
            testLog += "\nNo Area Tasks because this area has NO FACTION!";
        }
        if(UIManager.Instance.areaInfoUI.activeArea != null && UIManager.Instance.areaInfoUI.activeArea.id == id) {
            Debug.Log(testLog);
        }
    }
    private INTERACTION_TYPE GetInteractionTypeForResidentCharacter(Character resident, out string testLog) {
        string log = "\nValid Interactions for " + resident.name + ":";
        WeightedDictionary<INTERACTION_TYPE> interactionWeights = new WeightedDictionary<INTERACTION_TYPE>();
        foreach (KeyValuePair<INTERACTION_TYPE, int> areaTasks in areaTasksInteractionWeights) {
            if(InteractionManager.Instance.CanCreateInteraction(areaTasks.Key, resident)) {
                interactionWeights.AddElement(areaTasks.Key, areaTasks.Value);
                log += "\n - " + areaTasks.Key.ToString();
            }
        }
        testLog = log;
        return interactionWeights.PickRandomElementGivenWeights();
    }
    private bool CanDoAreaTaskInteraction(INTERACTION_TYPE interactionType, Character character) {
        return suppliesInBank >= 100 && InteractionManager.Instance.CanCreateInteraction(interactionType, character);
    }
    #endregion

    #region Defenders
    public void SetMaxDefenderGroups(int maxDefenderGroups) {
        this.maxDefenderGroups = maxDefenderGroups;
    }
    public void SetInitialDefenderGroups(int initialDefenderGroups) {
        this.initialDefenderGroups = initialDefenderGroups;
    }
    //private void GenerateInitialDefenders() {
    //    //This will generate an empty defender group
    //    AddDefenderGroup(new DefenderGroup());
    //    //if (initialDefenderGroups == 0) {
    //    //    return;
    //    //}
    //    //WeightedDictionary<AreaCharacterClass> defenderWeights = GetClassWeights();
    //    ////if (this.owner != null && this.owner.defenderWeights.GetTotalOfWeights() > 0) {
    //    ////    defenderWeights = this.owner.defenderWeights;
    //    ////} else {
    //    ////    defenderWeights = LandmarkManager.Instance.GetDefaultDefenderWeights(race);
    //    ////}
    //    //if (defenderWeights == null || defenderWeights.GetTotalOfWeights() <= 0) {
    //    //    return;
    //    //}
    //    //for (int i = 0; i < initialDefenderGroups; i++) {
    //    //    DefenderGroup newGroup = new DefenderGroup();
    //    //    AddDefenderGroup(newGroup);
    //    //    int defendersToGenerate = 4;
    //    //    for (int j = 0; j < defendersToGenerate; j++) {
    //    //        string chosenClass = defenderWeights.PickRandomElementGivenWeights().className;
    //    //        Character createdCharacter = CharacterManager.Instance.CreateNewCharacter(chosenClass, raceType, Utilities.GetRandomGender(), owner, coreTile.landmarkOnTile);
    //    //        newGroup.AddCharacterToGroup(createdCharacter);
    //    //        //TODO: Add Level
    //    //    }
    //    //}
    //}
    public void AddDefenderGroup(DefenderGroup defenderGroup) {
        //if (defenderGroups.Count < maxDefenderGroups) {
            defenderGroups.Add(defenderGroup);
            defenderGroup.SetDefendingArea(this);
        //}
    }
    public void RemoveDefenderGroup(DefenderGroup defenderGroup) {
        if (defenderGroups.Remove(defenderGroup)) {
            defenderGroup.SetDefendingArea(null);
        }
    }
    public DefenderGroup GetFirstDefenderGroup() {
        if (defenderGroups.Count > 0) {
            return defenderGroups[0];
        }
        return null;
    }
    public DefenderGroup GetRandomDefenderGroup() {
        if (defenderGroups.Count > 0) {
            return defenderGroups[UnityEngine.Random.Range(0, defenderGroups.Count)];
        }
        return null;
    }
    public WeightedDictionary<AreaCharacterClass> GetClassWeights() {
        WeightedDictionary<AreaCharacterClass> classWeights = LandmarkManager.Instance.GetDefaultDefenderWeights(race);
        if (this.owner != null && this.owner.additionalClassWeights.GetTotalOfWeights() > 0) {
            classWeights.AddElements(this.owner.additionalClassWeights);
        }
        return classWeights;
    }
    public bool HasClassInWeights(string className) {
        WeightedDictionary<AreaCharacterClass> classWeights = GetClassWeights();
        foreach (KeyValuePair<AreaCharacterClass, int> keyValuePair in classWeights.dictionary) {
            if (keyValuePair.Key.className.Equals(className)) {
                return true;
            }
        }
        return false;
    }
    public void UpgradeDefendersToMatchFactionLvl() {
        for (int i = 0; i < defenderGroups.Count; i++) {
            for (int j = 0; j < defenderGroups[i].party.characters.Count; j++) {
                Character defender = defenderGroups[i].party.characters[j];
                if (defender.level < owner.level) {
                    defender.SetLevel(owner.level);
                }
            }
        }
    }
    private void AssignMonthlyDefenders(List<Character> candidates, List<Character> interactionCandidates) {
        if(owner != null) {
            string testLog = "[Day " + GameManager.Instance.continuousDays + "] DEFENDERS FOR " + this.name;
            DefenderGroup defenderGroup = GetFirstDefenderGroup();
            if (defenderGroup != null) {
                testLog += "\nDisbanding current defenders...";
                defenderGroup.DisbandGroup();
            } else {
                testLog += "\nNo more defender group. Creating new one...";
                defenderGroup = new DefenderGroup();
                AddDefenderGroup(defenderGroup);
            }
            if (candidates.Count > 0) {
                List<int> frontlineIndexes = new List<int>();
                List<int> backlineIndexes = new List<int>();
                for (int i = 0; i < candidates.Count; i++) {
                    if (candidates[i].characterClass.combatPosition == COMBAT_POSITION.FRONTLINE) {
                        frontlineIndexes.Add(i);
                    } else {
                        backlineIndexes.Add(i);
                    }
                }
                for (int i = 0; i < 2; i++) {
                    if (frontlineIndexes.Count > 0) {
                        int index = UnityEngine.Random.Range(0, frontlineIndexes.Count);
                        defenderGroup.AddCharacterToGroup(candidates[frontlineIndexes[index]]);
                        interactionCandidates.Remove(candidates[frontlineIndexes[index]]);
                        testLog += "\nFRONTLINE DEFENDER " + (i + 1) + ": " + candidates[frontlineIndexes[index]].name;
                        frontlineIndexes.RemoveAt(index);
                    }
                }
                for (int i = 0; i < 2; i++) {
                    if (backlineIndexes.Count > 0) {
                        int index = UnityEngine.Random.Range(0, backlineIndexes.Count);
                        defenderGroup.AddCharacterToGroup(candidates[backlineIndexes[index]]);
                        interactionCandidates.Remove(candidates[backlineIndexes[index]]);
                        testLog += "\nBACKLINE DEFENDER " + (i + 1) + ": " + candidates[backlineIndexes[index]].name;
                        backlineIndexes.RemoveAt(index);
                    }
                }
            }
            Messenger.Broadcast(Signals.AREA_DEFENDERS_CHANGED, this);
            if (UIManager.Instance.areaInfoUI.activeArea != null && UIManager.Instance.areaInfoUI.activeArea.id == id) {
                Debug.Log(testLog);
            }
        }
    }
    #endregion

    #region Characters
    public void AddResident(Character character, bool ignoreCapacity = true) {
        if (!areaResidents.Contains(character)) {
            if (!ignoreCapacity) {
                if (IsResidentsFull()) {
                    return; //area is at capacity
                }
            }
            areaResidents.Add(character);
        }
    }
    public void RemoveResident(Character character) {
        areaResidents.Remove(character);
    }
    public void AddCharacterAtLocation(Character character) {
        if (!charactersAtLocation.Contains(character)) {
            charactersAtLocation.Add(character);
            AddCharacterAtLocationHistory("Added " + character.name + "ST: " + StackTraceUtility.ExtractStackTrace());
        }
    }
    public void RemoveCharacterAtLocation(Character character) {
        if (charactersAtLocation.Remove(character)) {
            AddCharacterAtLocationHistory("Removed " + character.name + "ST: " + StackTraceUtility.ExtractStackTrace());
        }

    }
    private void AddCharacterAtLocationHistory(string str) {
#if !WORLD_CREATION_TOOL
        charactersAtLocationHistory.Add(GameManager.Instance.TodayLogString() + str);
        if (charactersAtLocationHistory.Count > 100) {
            charactersAtLocationHistory.RemoveAt(0);
        }
#endif
    }
    public bool HasResidentWithClass(string className) {
        for (int i = 0; i < areaResidents.Count; i++) {
            if (areaResidents[i].characterClass != null && areaResidents[i].characterClass.className == className) {
                return true;
            }
        }
        return false;
    }
    public List<Character> GetResidentsWithClass(string className) {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < areaResidents.Count; i++) {
            if (areaResidents[i].characterClass != null && areaResidents[i].characterClass.className == className) {
                characters.Add(areaResidents[i]);
            }
        }
        return characters;
    }
    public bool IsResidentsFull() {
        return areaResidents.Count >= residentCapacity;
    }
    public void GenerateNeutralCharacters() {
        if (defaultRace.race == RACE.NONE) {
            return; //no default race was generated
        }
        InitialRaceSetup setup = GetRaceSetup(defaultRace);
        int charactersToCreate = UnityEngine.Random.Range(setup.spawnRange.lowerBound, setup.spawnRange.upperBound + 1);
        WeightedDictionary<AreaCharacterClass> classWeights = GetClassWeights();
        if (classWeights.GetTotalOfWeights() > 0) {
            for (int i = 0; i < charactersToCreate; i++) {
                AreaCharacterClass chosenClass = classWeights.PickRandomElementGivenWeights();
                BaseLandmark randomHome = this.landmarks[UnityEngine.Random.Range(0, landmarks.Count)];
                Character createdCharacter = CharacterManager.Instance.CreateNewCharacter(chosenClass.className, defaultRace.race, Utilities.GetRandomGender(), 
                    FactionManager.Instance.neutralFaction, randomHome);
                createdCharacter.SetLevel(createdCharacter.raceSetting.neutralSpawnLevel);
                Debug.Log(GameManager.Instance.TodayLogString() + "Generated Lvl. " + createdCharacter.level.ToString() + 
                    " neutral character " + createdCharacter.characterClass.className + " " + createdCharacter.name + " at " + this.name);
            }
        }
        
    }
    public void SetInitialResidents(int initialResidents) {
        this.initialResidents = initialResidents;
    }
    private void GenerateInitialResidents() {
        if (initialResidents <= 0) {
            return;
        }
        WeightedDictionary<AreaCharacterClass> classWeights = GetClassWeights();
        int remainingCharactersToGenerate = initialResidents - areaResidents.Count;
        for (int i = 0; i < remainingCharactersToGenerate; i++) {
            AreaCharacterClass chosenClass = classWeights.PickRandomElementGivenWeights();
            BaseLandmark randomHome = this.landmarks[UnityEngine.Random.Range(0, landmarks.Count)];
            Character createdCharacter = CharacterManager.Instance.CreateNewCharacter(chosenClass.className, this.raceType, Utilities.GetRandomGender(),
                owner, randomHome);
            createdCharacter.SetLevel(owner.level);
            Debug.Log(GameManager.Instance.TodayLogString() + "Generated Lvl. " + createdCharacter.level.ToString() +
                    " character " + createdCharacter.characterClass.className + " " + createdCharacter.name + " at " + this.name + " for faction " + this.owner.name);
        }
    }
    public bool HasCharacterThatIsNotFromFaction(Faction faction) {
        for (int i = 0; i < charactersAtLocation.Count; i++) {
            Character currChar = charactersAtLocation[i];
            if (currChar.faction.id != faction.id) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Logs
    public void AddHistory(Log log) {
        if (!history.Contains(log)) {
            history.Add(log);
            if (this.history.Count > 60) {
                this.history.RemoveAt(0);
            }
            Messenger.Broadcast(Signals.HISTORY_ADDED, this as object);
        }
    }
    #endregion

    #region Attack
    public void AttackTarget() {
        for (int i = 1; i < attackCharacters.Count; i++) {
            attackCharacters[0].ownParty.AddCharacter(attackCharacters[i]);
        }
        attackCharacters[0].AttackAnArea(attackTarget);
        SetAttackTargetAndCharacters(null, null);
    }
    #endregion

    #region Special Tokens
    //private void LoadSpecialTokens(AreaSaveData data) {
    //    possibleSpecialTokenSpawns = new List<SpecialToken>();
    //    if (data.possibleSpecialTokenSpawns != null) {
    //        for (int i = 0; i < data.possibleSpecialTokenSpawns.Count; i++) {
    //            string tokenName = data.possibleSpecialTokenSpawns[i];
    //            possibleSpecialTokenSpawns.Add(TokenManager.Instance.GetSpecialToken(tokenName));
    //        }
    //        Messenger.AddListener<SpecialToken>(Signals.SPECIAL_TOKEN_RAN_OUT, OnSpecialTokenRanOut);
    //    }
    //}
    //private void OnSpecialTokenRanOut(SpecialToken token) { //Called when special token quantity reaches 0
    //    if (possibleSpecialTokenSpawns.Contains(token)) {
    //        possibleSpecialTokenSpawns.Remove(token);
    //        if (possibleSpecialTokenSpawns.Count == 0) {
    //            Messenger.RemoveListener<SpecialToken>(Signals.SPECIAL_TOKEN_RAN_OUT, OnSpecialTokenRanOut);
    //        }
    //    }
    //}
    public void AddSpecialTokenToLocation(SpecialToken token) {
        if (!possibleSpecialTokenSpawns.Contains(token)) {
            possibleSpecialTokenSpawns.Add(token);
            Debug.Log(GameManager.Instance.TodayLogString() + "Added " + token.name + " at " + name);
        }
    }
    public void RemoveSpecialTokenFromLocation(SpecialToken token) {
        possibleSpecialTokenSpawns.Remove(token);
        Debug.Log(GameManager.Instance.TodayLogString() + "Removed " + token.name + " from " + name);
    }
    public List<SpecialToken> GetElligibleTokensForCharacter(Character character) {
        List<SpecialToken> choices = new List<SpecialToken>(possibleSpecialTokenSpawns);
        choices.Remove(character.tokenInInventory);
        //Utilities.ListRemoveRange(choices, character.tokenInInventory);
        return choices;
    }
    #endregion
}

[System.Serializable]
public struct IntRange {
    public int lowerBound;
    public int upperBound;
    
    public IntRange(int low, int high) {
        lowerBound = low;
        upperBound = high;
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

    public InitialRaceSetup(Race race) {
        this.race = race;
        spawnRange = new IntRange(0,0);
    }
}