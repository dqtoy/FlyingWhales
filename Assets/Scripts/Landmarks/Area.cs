using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class Area {

    public int id { get; private set; }
    public int totalCivilians { get { return landmarks.Sum(x => x.civilianCount); } }
    public int suppliesInBank { get; private set; }
    public string name { get; private set; }
    public AREA_TYPE areaType { get; private set; }
    public HexTile coreTile { get; private set; }
    public Color areaColor { get; private set; }
    public Faction owner { get; private set; }
    public Area attackTarget { get; private set; }
    public LocationIntel locationIntel { get; private set; }
    public DefenderIntel defenderIntel { get; private set; }
    public List<HexTile> tiles { get; private set; }
    public List<BaseLandmark> landmarks { get { return tiles.Where(x => x.landmarkOnTile != null).Select(x => x.landmarkOnTile).ToList(); } }
    public List<BaseLandmark> exposedTiles { get; private set; }
    public List<BaseLandmark> unexposedTiles { get; private set; }
    public List<Character> areaResidents { get; private set; }
    public List<Character> charactersAtLocation { get; private set; }
    public List<Character> attackCharacters { get; private set; }
    public List<Log> history { get; private set; }
    public List<Interaction> currentInteractions { get; private set; }
    public bool isHighlighted { get; private set; }
    public bool hasBeenInspected { get; private set; }
    public bool areAllLandmarksDead { get; private set; }
    public bool stopDefaultAllExistingInteractions { get; private set; }
    public AreaInvestigation areaInvestigation { get; private set; }
    public int supplyCapacity { get; private set; }

    //defenders
    public int maxDefenderGroups { get; private set; }
    public int initialDefenderGroups { get; private set; }
    public List<DefenderGroup> defenderGroups { get; private set; }
    public List<RACE> possibleOccupants { get; private set; }
    public List<InitialRaceSetup> initialRaceSetup { get; private set; }
    public Dictionary<JOB, List<INTERACTION_TYPE>> jobInteractionTypes { get; private set; }
    public int initialSupply { get; private set; } //this should not change when scavenging
    public int residentCapacity { get; private set; }
    public int workSupplyProduction { get; private set; }

    private Race defaultRace;
    private List<HexTile> outerTiles;
    private List<SpriteRenderer> outline;

    #region getters
    public RACE raceType {
        get { return owner == null ? defaultRace.race : owner.raceType; }
    }
    public Race race {
        get { return owner == null ? defaultRace : owner.race; }
    }
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
        locationIntel = new LocationIntel(this);
        defenderIntel = new DefenderIntel(this);
        areaInvestigation = new AreaInvestigation(this);
        jobInteractionTypes = new Dictionary<JOB, List<INTERACTION_TYPE>>();
        initialRaceSetup = new List<InitialRaceSetup>();
        defaultRace = new Race(RACE.HUMANS, RACE_SUB_TYPE.NORMAL);
        SetAreaType(areaType);
        SetCoreTile(coreTile);
        SetSupplyCapacity(1000);
        AddTile(coreTile);
        if(areaType != AREA_TYPE.DEMONIC_INTRUSION) {
            Messenger.AddListener(Signals.DAY_ENDED_2, DefaultAllExistingInteractions);
        }
#if !WORLD_CREATION_TOOL
        StartSupplyLine();
#endif
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
        locationIntel = new LocationIntel(this);
        defenderIntel = new DefenderIntel(this);
        areaInvestigation = new AreaInvestigation(this);
        jobInteractionTypes = new Dictionary<JOB, List<INTERACTION_TYPE>>();
        if (data.raceSetup != null) {
            initialRaceSetup = new List<InitialRaceSetup>(data.raceSetup);
        } else {
            initialRaceSetup = new List<InitialRaceSetup>();
        }
#if WORLD_CREATION_TOOL
        SetCoreTile(worldcreator.WorldCreatorManager.Instance.GetHexTile(data.coreTileID));
#else
        SetCoreTile(GridMap.Instance.GetHexTile(data.coreTileID));
        StartSupplyLine();
#endif
        SetMaxDefenderGroups(data.maxDefenderGroups);
        SetInitialDefenderGroups(data.initialDefenderGroups);
        SetSupplyCapacity(data.supplyCapacity);
        SetInitialSupplies(data.initialSupply);
        SetResidentCapacity(data.residentCapacity);
        SetWorkSupplyProduction(data.workSupplyProduction);
        possibleOccupants = new List<RACE>();
        if (data.possibleOccupants != null) {
            possibleOccupants.AddRange(data.possibleOccupants);
        }
        //SetDefaultRace(data.defaultRace);
        AddTile(Utilities.GetTilesFromIDs(data.tileData)); //exposed tiles will be determined after loading landmarks at MapGeneration
        UpdateBorderColors();
        if (areaType != AREA_TYPE.DEMONIC_INTRUSION) {
            Messenger.AddListener(Signals.DAY_ENDED_2, DefaultAllExistingInteractions);
        }
        GenerateDefaultRace();
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
    public void SetInitialSupplies(int amount) {
        initialSupply = amount;
    }
    public void SetResidentCapacity(int amount) {
        residentCapacity = amount;
    }
    public void SetWorkSupplyProduction(int amount) {
        workSupplyProduction = amount;
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
        if (currTile.landmarkOnTile == null || currTile.landmarkOnTile.landmarkObj.isRuined) {
            return; //if there is no landmark on the tile, or it's landmark is already ruined, do not count as exposed
        }
        //check if the tile has a flat empty tile as a neighbour
        //if it does, it is an exposed tile
        bool isExposed = false;
        for (int j = 0; j < currTile.AllNeighbours.Count; j++) {
            HexTile currNeighbour = currTile.AllNeighbours[j];
            if (!isExposed && (currNeighbour.landmarkOnTile == null || currNeighbour.landmarkOnTile.landmarkObj.isRuined)
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
        if (currTile.landmarkOnTile == null || currTile.landmarkOnTile.landmarkObj.isRuined || currTile.areaOfTile == null || (currTile.areaOfTile != null && currTile.areaOfTile != this)) {
            return; //if there is no landmark on the tile, or it's landmark is already ruined, do not count as exposed
        }
        //check if the tile has a flat empty tile as a neighbour
        //if it does, it is an exposed tile
        bool isExposed = false;
        for (int j = 0; j < currTile.AllNeighbours.Count; j++) {
            HexTile currNeighbour = currTile.AllNeighbours[j];
            if ((currNeighbour.landmarkOnTile == null || currNeighbour.landmarkOnTile.landmarkObj.isRuined)
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
    #endregion

    #region Owner
    public void SetOwner(Faction owner) {
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
    }
    #endregion

    #region Utilities
    public void LoadAdditionalData() {
        //DetermineExposedTiles();
        Messenger.AddListener<StructureObj, ObjectState>(Signals.STRUCTURE_STATE_CHANGED, OnStructureStateChanged);
        GenerateInitialDefenders();
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
            if (tiles[i].landmarkOnTile != null && !tiles[i].landmarkOnTile.landmarkObj.isRuined) {
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
        if(owner != FactionManager.Instance.neutralFaction) {
            LandmarkManager.Instance.OwnArea(FactionManager.Instance.neutralFaction, this);
        }
    }
    public bool IsHostileTowards(Character character) {
        if (character.faction.isNeutral || this.owner == null || this.owner.id == character.faction.id) {
            return false;
        }
        FACTION_RELATIONSHIP_STATUS relStat = FactionManager.Instance.GetRelationshipStatusBetween(character.faction, this.owner);
        if (relStat == FACTION_RELATIONSHIP_STATUS.AT_WAR || relStat == FACTION_RELATIONSHIP_STATUS.HOSTILE) {
            return true;
        }
        return false;
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
        AdjustSuppliesInBank(100);
        Messenger.AddListener(Signals.MONTH_START, ExecuteSupplyLine);
    }
    private void ExecuteSupplyLine() {
        CollectDailySupplies();
        //PayMaintenance();
        LandmarkStartDayActions();
    }
    private void CollectDailySupplies() {
        int totalCollectedSupplies = 0;
        string supplySummary = string.Empty;
        for (int i = 0; i < landmarks.Count; i++) {
            BaseLandmark currLandmark = landmarks[i];
            if (currLandmark.canProduceSupplies && !currLandmark.landmarkObj.isRuined && currLandmark.MeetsSupplyProductionRequirements()) {
                int providedSupplies = UnityEngine.Random.Range(currLandmark.minDailySupplyProduction, currLandmark.maxDailySupplyProduction);
                totalCollectedSupplies += providedSupplies;
                AdjustSuppliesInBank(providedSupplies);
                supplySummary += currLandmark.name + "(" + currLandmark.specificLandmarkType.ToString() + ") - " + providedSupplies.ToString() + "\n";
            } else {
                supplySummary += currLandmark.name + "(" + currLandmark.specificLandmarkType.ToString() + ") - Cannot Produce\n";
            }
        }
        //Debug.Log(this.name + " collected supplies " + totalCollectedSupplies + " Summary: \n" + supplySummary);
    }
    private void PayMaintenance() {
        //consumes Supply per existing unit
        //for (int i = 0; i < landmarks.Count; i++) {
        //    BaseLandmark currLandmark = landmarks[i];
        //    if (currLandmark.defenders == null) {
        //        continue;
        //    }
        //    for (int j = 0; j < currLandmark.defenders.icharacters.Count; j++) {
        //        ICharacter currDefender = currLandmark.defenders.icharacters[j];
        //        if (currDefender is CharacterArmyUnit) {
        //            CharacterArmyUnit armyUnit = currDefender as CharacterArmyUnit;
        //            AdjustSuppliesInBank(-armyUnit.armyCount);
        //        } else if (currDefender is MonsterArmyUnit) {
        //            MonsterArmyUnit armyUnit = currDefender as MonsterArmyUnit;
        //            AdjustSuppliesInBank(-armyUnit.armyCount);
        //        } else {
        //            AdjustSuppliesInBank(-1); //if just a single character or monster
        //        }
        //    }
        //}
    }
    private void LandmarkStartDayActions() {
        for (int i = 0; i < landmarks.Count; i++) {
            BaseLandmark currLandmark = landmarks[i];
            currLandmark.landmarkObj.StartDayAction();
        }
    }
    public void SetSuppliesInBank(int amount) {
        suppliesInBank = amount;
        suppliesInBank = Mathf.Max(0, suppliesInBank);
        //suppliesInBank = Mathf.Clamp(suppliesInBank, 0, supplyCapacity);
    }
    public void AdjustSuppliesInBank(int amount) {
        suppliesInBank += amount;
        suppliesInBank = Mathf.Max(0, suppliesInBank);
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
    public void SetSupplyCapacity(int supplyCapacity) {
        this.supplyCapacity = supplyCapacity;
        //suppliesInBank = Mathf.Clamp(suppliesInBank, 0, supplyCapacity);
    }
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
    public BaseLandmark GetFirstAliveExposedTile() {
        for (int i = 0; i < exposedTiles.Count; i++) {
            if (!exposedTiles[i].landmarkObj.isRuined) {
                return exposedTiles[i];
            }
        }
        return null;
    }
    public void CenterOnCoreLandmark() {
        CameraMove.Instance.CenterCameraOn(coreTile.gameObject);
    }
    #endregion

    #region Interactions
    public void AddInteraction(Interaction interaction) {
        if(currentInteractions.Count > 0) {
            int interactionToBeAddedIndex = Utilities.GetInteractionPriorityIndex(interaction.type);
            bool hasBeenInserted = false;
            if (interactionToBeAddedIndex != -1) {
                for (int i = 0; i < currentInteractions.Count; i++) {
                    int currentInteractionIndex = Utilities.GetInteractionPriorityIndex(currentInteractions[i].type);
                    if (interactionToBeAddedIndex < currentInteractionIndex) {
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
    public List<Character> GetAreaResidents() {
        List<Character> choices = new List<Character>();
        List<BaseLandmark> landmarkCandidates = this.landmarks;
        for (int i = 0; i < landmarkCandidates.Count; i++) {
            for (int j = 0; j < landmarkCandidates[i].charactersWithHomeOnLandmark.Count; j++) {
                Character character = landmarkCandidates[i].charactersWithHomeOnLandmark[j];
                if (character.specificLocation.tileLocation.areaOfTile == this) {
                    choices.Add(character);
                }
            }
        }
        return choices;
    }
    #endregion

    #region Defenders
    public void SetMaxDefenderGroups(int maxDefenderGroups) {
        this.maxDefenderGroups = maxDefenderGroups;
    }
    public void SetInitialDefenderGroups(int initialDefenderGroups) {
        this.initialDefenderGroups = initialDefenderGroups;
    }
    private void GenerateInitialDefenders() {
        if (initialDefenderGroups == 0) {
            return;
        }
        WeightedDictionary<AreaCharacterClass> defenderWeights = GetClassWeights();
        //if (this.owner != null && this.owner.defenderWeights.GetTotalOfWeights() > 0) {
        //    defenderWeights = this.owner.defenderWeights;
        //} else {
        //    defenderWeights = LandmarkManager.Instance.GetDefaultDefenderWeights(race);
        //}
        if (defenderWeights == null || defenderWeights.GetTotalOfWeights() <= 0) {
            return;
        }
        for (int i = 0; i < initialDefenderGroups; i++) {
            DefenderGroup newGroup = new DefenderGroup();
            AddDefenderGroup(newGroup);
            int defendersToGenerate = 4;
            for (int j = 0; j < defendersToGenerate; j++) {
                string chosenClass = defenderWeights.PickRandomElementGivenWeights().className;
                Character createdCharacter = CharacterManager.Instance.CreateNewCharacter(chosenClass, raceType, Utilities.GetRandomGender(), owner, coreTile.landmarkOnTile);
                newGroup.AddCharacterToGroup(createdCharacter);
                //TODO: Add Level
            }
        }
    }
    public void AddDefenderGroup(DefenderGroup defenderGroup) {
        if (defenderGroups.Count < maxDefenderGroups) {
            defenderGroups.Add(defenderGroup);
            defenderGroup.SetDefendingArea(this);
        }
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
        //if (this.owner != null && this.owner.defenderWeights.GetTotalOfWeights() > 0) {
        //    return this.owner.defenderWeights;
        //} else {
            return LandmarkManager.Instance.GetDefaultDefenderWeights(race);
        //}
    }
    #endregion

    #region Characters
    public void AddResident(Character character, bool ignoreCapacity = true) {
        if (!areaResidents.Contains(character)) {
            if (!ignoreCapacity) {
                if (areaResidents.Count >= residentCapacity) {
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
        }
    }
    public void RemoveCharacterAtLocation(Character character) {
        charactersAtLocation.Remove(character);
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
    #endregion

    #region Logs
    public void AddHistory(Log log) {
        if (!history.Contains(log)) {
            history.Add(log);
            if (this.history.Count > 60) {
                this.history.RemoveAt(0);
            }
        }
    }
    #endregion

    #region Attack
    public void AttackTarget() {
        for (int i = 1; i < attackCharacters.Count; i++) {
            attackCharacters[0].ownParty.AddCharacter(attackCharacters[i]);
        }
        attackCharacters[0].AttackAnArea(attackTarget);
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