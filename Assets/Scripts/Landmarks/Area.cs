using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class Area {

    public int id { get; private set; }
    public int supplyCapacity { get; private set; }
    public string name { get; private set; }
    public bool isHighlighted { get; private set; }
    public bool hasBeenInspected { get; private set; }
    public bool areAllLandmarksDead { get; private set; }
    public bool stopDefaultAllExistingInteractions { get; private set; }
    public bool isDead { get; private set; }
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
    public List<Corpse> corpsesInArea { get; private set; }
    public int monthlyActions { get; private set; }

    //defenders
    public int maxDefenderGroups { get; private set; }
    public int initialDefenderGroups { get; private set; }
    public List<DefenderGroup> defenderGroups { get; private set; }
    public List<RACE> possibleOccupants { get; private set; }
    public List<InitialRaceSetup> initialSpawnSetup { get; private set; } //only to be used when unoccupied
    public Dictionary<JOB, List<INTERACTION_TYPE>> jobInteractionTypes { get; private set; }
    public int residentCapacity { get; private set; }
    public int monthlySupply { get; private set; }
    public List<Interaction> eventsTargettingThis { get; private set; }

    //special tokens
    public List<SpecialToken> possibleSpecialTokenSpawns { get; private set; }
    public const int MAX_ITEM_CAPACITY = 15;

    //structures
    public Dictionary<STRUCTURE_TYPE, List<LocationStructure>> structures { get; private set; }
    public int dungeonSupplyRangeMin { get; private set; }
    public int dungeonSupplyRangeMax { get; private set; }

    //misc
    public Sprite locationPortrait { get; private set; }
    public Vector2 nameplatePos { get; private set; }
    public bool isBeingTracked { get; private set; }

    //for testing
    public List<string> supplyLog { get; private set; } //limited to 100 entries
    public List<string> charactersAtLocationHistory { get; private set; }

    public Race defaultRace { get; private set; }
    private RACE _raceType;
    private List<HexTile> outerTiles;
    private List<SpriteRenderer> outline;
    private int _offenseTaskWeightMultiplier;

    #region getters
    public RACE raceType {
        get { return _raceType; }
    }
    public List<Character> visitors {
        get { return charactersAtLocation.Where(x => !areaResidents.Contains(x)).ToList(); }
    }
    public int offenseTaskWeightMultiplier {
        get { return _offenseTaskWeightMultiplier; }
        set { _offenseTaskWeightMultiplier = value; }
    }
    public int suppliesInBank {
        get {
            LocationStructure warehouse = GetRandomStructureOfType(STRUCTURE_TYPE.WAREHOUSE, 1);
            if (warehouse == null) {
                return 0;
            }
            return warehouse.GetSupplyPile().suppliesInPile;
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
        initialSpawnSetup = new List<InitialRaceSetup>();
        eventsTargettingThis = new List<Interaction>();
        supplyLog = new List<string>();
        defaultRace = new Race(RACE.HUMANS, RACE_SUB_TYPE.NORMAL);
        possibleSpecialTokenSpawns = new List<SpecialToken>();
        charactersAtLocationHistory = new List<string>();
        corpsesInArea = new List<Corpse>();
        structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
        SetDungeonSupplyRange(0, 0);
        SetMonthlyActions(2);
        SetAreaType(areaType);
        SetCoreTile(coreTile);
        //SetSupplyCapacity(1000);
        AddTile(coreTile);
        SetSuppliesInBank(1000);
        //if (areaType != AREA_TYPE.DEMONIC_INTRUSION) {
        //    Messenger.AddListener(Signals.DAY_ENDED_2, DefaultAllExistingInteractions);
        //}
#if !WORLD_CREATION_TOOL
        ConstructAreaTasksInteractionWeights();
        //StartSupplyLine();
#endif
        nameplatePos = LandmarkManager.Instance.GetAreaNameplatePosition(this);
        possibleOccupants = new List<RACE>();
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
        corpsesInArea = new List<Corpse>();
        if (data.raceSetup != null) {
            initialSpawnSetup = new List<InitialRaceSetup>(data.raceSetup);
        } else {
            initialSpawnSetup = new List<InitialRaceSetup>();
        }
        SetDungeonSupplyRange(data.dungeonSupplyRangeMin, data.dungeonSupplyRangeMax);
        SetMaxDefenderGroups(data.maxDefenderGroups);
        SetInitialDefenderGroups(data.initialDefenderGroups);
        SetResidentCapacity(data.residentCapacity);
        SetMonthlySupply(data.monthlySupply);
        SetInitialResidents(data.initialResidents);
        SetMonthlyActions(data.monthlyActions);
        LoadStructures(data);
#if WORLD_CREATION_TOOL
        SetCoreTile(worldcreator.WorldCreatorManager.Instance.GetHexTile(data.coreTileID));
#else
        SetSuppliesInBank(data.monthlySupply);
        SetCoreTile(GridMap.Instance.GetHexTile(data.coreTileID));
        ConstructAreaTasksInteractionWeights();
        //StartSupplyLine();
#endif

        possibleOccupants = new List<RACE>();
        if (data.possibleOccupants != null) {
            possibleOccupants.AddRange(data.possibleOccupants);
        }


        //LoadSpecialTokens(data);
        AddTile(Utilities.GetTilesFromIDs(data.tileData)); //exposed tiles will be determined after loading landmarks at MapGeneration
        UpdateBorderColors();
        //if (areaType != AREA_TYPE.DEMONIC_INTRUSION) {
        //    Messenger.AddListener(Signals.DAY_ENDED_2, DefaultAllExistingInteractions);
        //}
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
    private InitialRaceSetup GetRaceSetup(Race race) {
        for (int i = 0; i < initialSpawnSetup.Count; i++) {
            InitialRaceSetup raceSetup = initialSpawnSetup[i];
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
            if (this.coreTile == null) {
                SetCoreTile(tile);
            }
#if !WORLD_CREATION_TOOL
            DetermineIfTileIsExposed(tile);
            if (determineOuterTiles) {
                UpdateOuterTiles();
            }
#endif
            OnTileAddedToArea(tile);
            Messenger.Broadcast(Signals.AREA_TILE_ADDED, this, tile);
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
            if (coreTile == tile) {
                SetCoreTile(null);
            }
            OnTileRemovedFromArea(tile);

#if !WORLD_CREATION_TOOL
            if (tile.landmarkOnTile != null) {
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
        //DetermineExposedTiles();
        //Messenger.AddListener<Interaction>(Signals.INTERACTION_ENDED, RemoveEventTargettingThis); 
        //GenerateInitialDefenders();
        //GenerateInitialResidents();
        CreateNameplate();
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
    public void SetAttackTargetAndCharacters(Area target, List<Character> characters) {
        attackTarget = target;
        attackCharacters = characters;
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
        }
    }
    private void ReleaseAllAbductedCharacters() {
        for (int i = 0; i < charactersAtLocation.Count; i++) {
            charactersAtLocation[i].ReleaseFromAbduction();
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
                && currArea.GetEventsOfTypeTargettingThis(INTERACTION_TYPE.MOVE_TO_EXPANSION_EVENT).Count == 0) {
                targets.Add(currArea);
            }
        }
        return targets;
    }
    public string GetAreaTypeString() {
        if (_raceType != RACE.NONE) {
            if (tiles.Count > 1) {
                return Utilities.GetNormalizedSingularRace(_raceType) + " " + Utilities.NormalizeStringUpperCaseFirstLetters(GetBaseAreaType().ToString());
            } else {
                return Utilities.GetNormalizedSingularRace(_raceType) + " " + Utilities.NormalizeStringUpperCaseFirstLetters(coreTile.landmarkOnTile.specificLandmarkType.ToString());
            }
        } else {
            return Utilities.NormalizeStringUpperCaseFirstLetters(coreTile.landmarkOnTile.specificLandmarkType.ToString());
        }
    }
    #endregion

    #region Supplies
    //private void StartSupplyLine() {
    //    //AdjustSuppliesInBank(100);
    //    Messenger.AddListener(Signals.MONTH_START, StartMonthActions);
    //}
    //public void StopSupplyLine() {
    //    Messenger.RemoveListener(Signals.MONTH_START, StartMonthActions);
    //}
    //private void StartMonthActions() {
    //    if (areaInvestigation.assignedMinion != null) {
    //        areaInvestigation.assignedMinion.character.job.DoPassiveEffect(this);
    //    }
    //    //CollectMonthlySupplies();
    //    //AreaTasksAssignments();
    //}
    public void AreaTasksAssignments() {
        //List<Character> defenderCandidates = new List<Character>();
        List<Character> interactionCandidates = new List<Character>();

        for (int i = 0; i < areaResidents.Count; i++) {
            Character resident = areaResidents[i];
            if (resident.doNotDisturb <= 0 && !resident.isDefender && !resident.currentParty.icon.isTravelling && resident.specificLocation.id == id) {
                if (attackCharacters != null && attackCharacters.Contains(resident)) {
                    continue;
                }
                //if (resident.forcedInteraction == null || (resident.forcedInteraction != null && resident.forcedInteraction.type != INTERACTION_TYPE.MOVE_TO_ATTACK)) {
                //    defenderCandidates.Add(resident);
                //}
                //if (!resident.isDefender) {
                if ((owner == null && resident.faction == FactionManager.Instance.neutralFaction) || resident.faction == owner) {
                    interactionCandidates.Add(resident);
                }
                //}
            }
        }
        //AssignMonthlyDefenders(defenderCandidates, interactionCandidates);
        AssignInteractionsToResidents(interactionCandidates);
    }
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
    public bool HasEnoughSupplies(int neededSupplies) {
        return suppliesInBank >= neededSupplies;
    }
    public void PayForReward(Reward reward) {
        if (reward.rewardType == REWARD.SUPPLY) {
            AdjustSuppliesInBank(-reward.amount);
        }
    }
    //public void GetSuppliesFrom(Area targetArea, int amount) {

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
    public void CenterOnCoreLandmark() {
        CameraMove.Instance.CenterCameraOn(coreTile.gameObject);
    }
    #endregion

    #region Interactions
    private void ConstructAreaTasksInteractionWeights() {
        areaTasksInteractionWeights = new Dictionary<INTERACTION_TYPE, int>() {
            {INTERACTION_TYPE.MOVE_TO_EXPLORE_EVENT, 100},
            {INTERACTION_TYPE.MOVE_TO_EXPANSION_EVENT, 15},
            //{INTERACTION_TYPE.MOVE_TO_SCAVENGE, 60},
            {INTERACTION_TYPE.MOVE_TO_RAID_EVENT, 40},
            //{INTERACTION_TYPE.MOVE_TO_CHARM, 35},
            //{INTERACTION_TYPE.MOVE_TO_RECRUIT, 35},
            {INTERACTION_TYPE.MOVE_TO_ABDUCT_ACTION, 25},
            //{INTERACTION_TYPE.MOVE_TO_STEAL, 20},
            {INTERACTION_TYPE.MOVE_TO_HUNT_ACTION, 20},
            {INTERACTION_TYPE.MOVE_TO_IMPROVE_RELATIONS_EVENT, 40},
            {INTERACTION_TYPE.PATROL_ACTION, 50},
            {INTERACTION_TYPE.EAT_DEFENSELESS, 20},
            {INTERACTION_TYPE.TORTURE_ACTION, 25},
            {INTERACTION_TYPE.MOVE_TO_REANIMATE_ACTION, 50},
            {INTERACTION_TYPE.MOVE_TO_SAVE_ACTION, 30},
            {INTERACTION_TYPE.MOVE_TO_VISIT, 50},
            {INTERACTION_TYPE.MOVE_TO_LOOT_ACTION, 50},
            {INTERACTION_TYPE.MOVE_TO_TAME_BEAST_ACTION, 50},
            {INTERACTION_TYPE.MOVE_TO_HANG_OUT_ACTION, 50},
            {INTERACTION_TYPE.MOVE_TO_ARGUE_ACTION, 50},
            {INTERACTION_TYPE.MOVE_TO_CURSE_ACTION, 50},
            {INTERACTION_TYPE.MOVE_TO_RECRUIT_FRIEND_ACTION_FACTION, 50},
            {INTERACTION_TYPE.MOVE_TO_CHARM_ACTION_FACTION, 50},
            {INTERACTION_TYPE.MOVE_TO_ASSASSINATE_ACTION_FACTION, 50},
            {INTERACTION_TYPE.MOVE_TO_STEAL_ACTION_FACTION, 50},
            {INTERACTION_TYPE.MOVE_TO_SCAVENGE_EVENT_FACTION, 50},
        };
    }
    public void AddInteraction(Interaction interaction) {
        if (currentInteractions.Count > 0) {
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
        //if (interaction.characterInvolved != null) {
        //    interaction.characterInvolved.currentInteractions.Add(interaction);
        //}
        //interaction.interactable.currentInteractions.Add(interaction);
        //interaction.Initialize();
        //Messenger.Broadcast(Signals.ADDED_INTERACTION, this as IInteractable, interaction);
    }
    public void RemoveInteraction(Interaction interaction) {
        if (currentInteractions.Remove(interaction)) {
            //if (interaction.characterInvolved != null) {
            //    interaction.characterInvolved.currentInteractions.Remove(interaction);
            //}
            //interaction.interactable.currentInteractions.Remove(interaction);
            //Messenger.Broadcast(Signals.REMOVED_INTERACTION, this as IInteractable, interaction);
        }
    }
    //public void DefaultAllExistingInteractions() { //NOTE: Only 
    //    if(stopDefaultAllExistingInteractions) { return; }
    //    for (int i = 0; i < currentInteractions.Count; i++) {
    //        if (!currentInteractions[i].hasActivatedTimeOut) {
    //            currentInteractions[i].TimedOutRunDefault();
    //            i--;
    //        }
    //    }
    //}
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
        int supplySpent = 0;
        if (candidates.Count <= 0) {
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
                if (interactionType != INTERACTION_TYPE.MOVE_TO_RAID_EVENT && interactionType != INTERACTION_TYPE.MOVE_TO_SCAVENGE_EVENT) {
                    supplySpent += 100;
                    if (supplySpent <= suppliesInBank) {
                        testLog += "\nChosen Interaction: " + interactionType.ToString();
                        interaction = InteractionManager.Instance.CreateNewInteraction(interactionType, chosenCandidate.specificLocation);
                        interaction.SetCanInteractionBeDoneAction(() => CanDoAreaTaskInteraction(interaction.type, chosenCandidate, 100));
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
                    interaction = InteractionManager.Instance.CreateNewInteraction(interactionType, chosenCandidate.specificLocation);
                    interaction.SetCanInteractionBeDoneAction(() => InteractionManager.Instance.CanCreateInteraction(interaction.type, chosenCandidate));
                    chosenCandidate.SetForcedInteraction(interaction);
                }
            }
        }
        if (UIManager.Instance.areaInfoUI.activeArea != null && UIManager.Instance.areaInfoUI.activeArea.id == id) {
            Debug.Log(testLog);
        }
    }
    private INTERACTION_TYPE GetInteractionTypeForResidentCharacter(Character resident, out string testLog) {
        string log = "\nValid Interactions for " + resident.name + ":";
        WeightedDictionary<INTERACTION_TYPE> interactionWeights = new WeightedDictionary<INTERACTION_TYPE>();
        foreach (KeyValuePair<INTERACTION_TYPE, int> areaTasks in areaTasksInteractionWeights) {
            if (InteractionManager.Instance.CanCreateInteraction(areaTasks.Key, resident)) {
                interactionWeights.AddElement(areaTasks.Key, areaTasks.Value);
                log += "\n - " + areaTasks.Key.ToString();
            }
        }
        testLog = log;
        return interactionWeights.PickRandomElementGivenWeights();
    }
    public bool CanDoAreaTaskInteraction(INTERACTION_TYPE interactionType, Character character, int supplyCost) {
        return suppliesInBank >= supplyCost; //&& InteractionManager.Instance.CanCreateInteraction(interactionType, character);
    }
    public Dictionary<Character, List<INTERACTION_TYPE>> GetResidentAndInteractionsTheyCanDoByCategoryAndAlignment(INTERACTION_CATEGORY category, MORALITY factionMorality) {
        Dictionary<Character, List<INTERACTION_TYPE>> residentInteractions = new Dictionary<Character, List<INTERACTION_TYPE>>();
        for (int i = 0; i < areaResidents.Count; i++) {
            Character resident = areaResidents[i];
            if (resident.doNotDisturb <= 0 && !resident.isDefender && !resident.currentParty.icon.isTravelling && resident.specificLocation.id == id && resident.GetTraitOr("Starving", "Exhausted") == null) {
                if (attackCharacters != null && attackCharacters.Contains(resident)) {
                    continue;
                }
                if ((owner == null && resident.faction == FactionManager.Instance.neutralFaction) || resident.faction == owner) {
                    List<INTERACTION_TYPE> interactionTypes = RaceManager.Instance.GetFactionInteractionsOfRace(resident, category, factionMorality);
                    if (interactionTypes != null) {
                        for (int j = 0; j < interactionTypes.Count; j++) {
                            if (!InteractionManager.Instance.CanCreateInteraction(interactionTypes[j], resident)) {
                                interactionTypes.RemoveAt(j);
                                j--;
                            }
                        }
                        if (interactionTypes.Count > 0) {
                            residentInteractions.Add(resident, interactionTypes);
                        }
                    }
                }
            }
        }
        return residentInteractions;
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
    public DefenderGroup GetDefenseGroup() {
        List<Character> defenders = FormCombatCharacters();
        if (defenders.Count > 0) {
            DefenderGroup group = new DefenderGroup();
            group.SetDefendingArea(this);
            for (int i = 0; i < defenders.Count; i++) {
                group.AddCharacterToGroup(defenders[i]);
            }
            return group;
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
        return GetClassWeights(raceType);
    }
    public WeightedDictionary<AreaCharacterClass> GetClassWeights(RACE raceType) {
        WeightedDictionary<AreaCharacterClass> classWeights = LandmarkManager.Instance.GetDefaultClassWeights(raceType);
        if (!Utilities.IsRaceBeast(raceType) //Check if the area is a beast type, if it is, only use the default class weights
            && this.owner != null
            && this.owner.additionalClassWeights.GetTotalOfWeights() > 0) {
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
        if (owner != null) {
            string testLog = "[Day " + GameManager.Instance.continuousDays + "] DEFENDERS FOR " + this.name;
            DefenderGroup defenderGroup = GetFirstDefenderGroup();
            if (defenderGroup != null) {
                testLog += "\nDisbanding current defenders...";
                //if(defenderGroup.party != null) {
                //    for (int i = 0; i < defenderGroup.party.characters.Count; i++) {
                //        interactionCandidates.Add(defenderGroup.party.characters[i]);
                //    }
                //}
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
    /*
     Check if any of the characters provided is a resident in this area.
         */
    private bool HasResidentFromChoices(List<Character> choices) {
        for (int i = 0; i < areaResidents.Count; i++) {
            Character currResident = areaResidents[i];
            if (choices.Contains(currResident)) {
                return true;
            }
        }
        return false;
    }
    private List<Character> GetResidentsFromChoices(List<Character> choices) {
        List<Character> residents = new List<Character>();
        for (int i = 0; i < areaResidents.Count; i++) {
            Character currResident = areaResidents[i];
            if (choices.Contains(currResident)) {
                residents.Add(currResident);
            }
        }
        return residents;
    }
    public void AddResident(Character character, bool ignoreCapacity = true) {
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
            AssignCharacterToDwellingInArea(character);
            //}
            //Messenger.Broadcast(Signals.AREA_RESIDENT_ADDED, this, character);
        }
    }
    public void AssignCharacterToDwellingInArea(Character character) {
        if (!structures.ContainsKey(STRUCTURE_TYPE.DWELLING)) {
            Debug.LogWarning(this.name + " doesn't have any dwellings for " + character.name);
            return;
        }

        Dwelling chosenDwelling = null;
        if (PlayerManager.Instance.player != null && this.id == PlayerManager.Instance.player.playerArea.id) {
            chosenDwelling = structures[STRUCTURE_TYPE.DWELLING][0] as Dwelling; //to avoid errors, residents in player area will all share the same dwelling
        } else {
            Character lover = character.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER);
            if (lover != null && areaResidents.Contains(lover)) { //check if the character has a lover that lives in the area
                chosenDwelling = lover.homeStructure;
            } else {
                //if none, check if they have a master/servant in the area
                Character master = character.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.MASTER);
                if (master != null && areaResidents.Contains(master)) { //if this character is the servant
                    chosenDwelling = master.homeStructure; //Move to his master's home, since this character doesn't have a lover (from the first checking) 
                } else { //if this character is a master
                    List<Character> servants = GetResidentsFromChoices(character.GetCharactersWithRelationship(RELATIONSHIP_TRAIT.SERVANT));
                    if (servants.Count > 0) { //check if he has any servant in this location that does not have a lover living with him
                        for (int i = 0; i < servants.Count; i++) {
                            Character currServant = servants[i];
                            if (!currServant.IsLivingWith(RELATIONSHIP_TRAIT.LOVER)) {
                                chosenDwelling = currServant.homeStructure; //if there is a valid servant, live with the valid servant
                                break;
                            }
                        }
                    }
                }
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

        if (chosenDwelling == null) {
            //if the code reaches here, it means that the area could not find a dwelling for the character
            //Debug.LogWarning(GameManager.Instance.TodayLogString() + "Could not find a dwelling for " + character.name + " at " + this.name);
        } else {
            character.MigrateHomeStructureTo(chosenDwelling);
            if (character.specificLocation != null
                && character.specificLocation.id == this.id) { //if the character is currently at his home area, and his home was changed, relocate him
                character.currentStructure.RemoveCharacterAtLocation(character);
                AddCharacterToAppropriateStructure(character);
            }
        }
    }
    public bool CanCharacterMigrateHere(Character character) {
        Character lover = character.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER);
        if (lover != null && areaResidents.Contains(lover)) {
            return true;
        } else {
            //if none, check if they have a master/servant in the area
            Character master = character.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.MASTER);
            if (master != null && areaResidents.Contains(master)) { //if this character is the servant
                return true;
            } else { //if this character is a master
                List<Character> servants = GetResidentsFromChoices(character.GetCharactersWithRelationship(RELATIONSHIP_TRAIT.SERVANT));
                if (servants.Count > 0) { //check if he has any servant in this location that does not have a lover living with him
                    for (int i = 0; i < servants.Count; i++) {
                        Character currServant = servants[i];
                        if (!currServant.IsLivingWith(RELATIONSHIP_TRAIT.LOVER)) {
                            return true;
                        }
                    }
                }
            }
        }

        //If character has no relationship whatsoever with residents here, check if there is an empty dwelling to reside in
        if(GetNumberOfUnoccupiedStructure(STRUCTURE_TYPE.DWELLING) > 0) {
            return true;
        }
        return false;
    }
    public void RemoveResident(Character character) {
        if (areaResidents.Remove(character)) {
            character.SetHome(null);
            character.homeStructure.RemoveResident(character);
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
    public void AddCharacterToLocation(Character character, LocationStructure structureOverride = null) {
        if (!charactersAtLocation.Contains(character)) {
            charactersAtLocation.Add(character);
            character.ownParty.SetSpecificLocation(this);
            AddCharacterAtLocationHistory("Added " + character.name + "ST: " + StackTraceUtility.ExtractStackTrace());
            //if (PlayerManager.Instance.player == null || PlayerManager.Instance.player.playerArea.id != this.id) {
            if (structureOverride != null) {
                structureOverride.AddCharacterAtLocation(character);
            } else {
                AddCharacterToAppropriateStructure(character);
            }
            //}
                
            Messenger.Broadcast(Signals.CHARACTER_ENTERED_AREA, this, character);
        }
    }
    public void AddCharacterToLocation(Party party, LocationStructure structureOverride = null) {
        for (int i = 0; i < party.characters.Count; i++) {
            AddCharacterToLocation(party.characters[i], structureOverride);
        }
    }
    public void RemoveCharacterFromLocation(Character character) {
        if (charactersAtLocation.Remove(character)) {
            //character.ownParty.SetSpecificLocation(null);
            if (character.currentStructure == null) {
                throw new Exception(character.name + " doesn't have a current structure at " + this.name);
            }
            character.currentStructure.RemoveCharacterAtLocation(character);
            AddCharacterAtLocationHistory("Removed " + character.name + "ST: " + StackTraceUtility.ExtractStackTrace());
            Messenger.Broadcast(Signals.CHARACTER_EXITED_AREA, this, character);
        }

    }
    public void RemoveCharacterFromLocation(Party party) {
        for (int i = 0; i < party.characters.Count; i++) {
            RemoveCharacterFromLocation(party.characters[i]);
        }
    }
    public void AddCharacterToAppropriateStructure(Character character) {
        if (character.homeArea.id == this.id) {
            if (character.homeStructure == null) {
                throw new Exception(character.name + "'s homeStructure is null!");
            }
            //If this is his home, the character will be placed in his Dwelling.
            character.homeStructure.AddCharacterAtLocation(character);
        } else {
            // Otherwise:
            if (Utilities.IsRaceBeast(character.race)) {
                //- Beasts will be placed at a random Wilderness.
                GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS).AddCharacterAtLocation(character);
            } else if (this.owner != null) {
                FACTION_RELATIONSHIP_STATUS relStat;
                if (character.faction.id == this.owner.id) { //character is part of the same faction as the owner of this area
                    relStat = FACTION_RELATIONSHIP_STATUS.ALLY;
                } else {
                    relStat = character.faction.GetRelationshipWith(this.owner).relationshipStatus; 
                }
                switch (relStat) {
                    case FACTION_RELATIONSHIP_STATUS.AT_WAR:
                    case FACTION_RELATIONSHIP_STATUS.ENEMY:
                        //- If location is occupied, non-beasts whose faction relationship is Enemy or worse will be placed in a random structure Outside Settlement.
                        List<LocationStructure> choices = GetStructuresAtLocation(false);
                        choices[UnityEngine.Random.Range(0, choices.Count)].AddCharacterAtLocation(character);
                        break;
                    case FACTION_RELATIONSHIP_STATUS.DISLIKED:
                    case FACTION_RELATIONSHIP_STATUS.NEUTRAL:
                    case FACTION_RELATIONSHIP_STATUS.FRIEND:
                    case FACTION_RELATIONSHIP_STATUS.ALLY:
                        LocationStructure inn = GetRandomStructureOfType(STRUCTURE_TYPE.INN);
                        if (inn != null) {
                            //- If location is occupied, non-beasts whose faction relationship is Disliked or better will be placed at the Inn. 
                            inn.AddCharacterAtLocation(character);
                        } else {
                            //If no Inn in the Location, he will be placed in a random Wilderness.
                            LocationStructure wilderness = GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
                            wilderness.AddCharacterAtLocation(character);
                        }
                        break;
                }
            } else {
                //- If location is unoccupied, non-beasts will be placed at a random Wilderness.
                GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS).AddCharacterAtLocation(character);
            }
        }

        if (character.currentStructure == null) {
            Debug.LogWarning(GameManager.Instance.TodayLogString() + "Could not find structure for " + character.name + " at " + this.name);
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
        if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.playerArea.id == this.id) {
            return false; //resident capacity is never full for player area
        }
        return structures[STRUCTURE_TYPE.DWELLING].Where(x => !x.IsOccupied()).Count() == 0; //check if there are still unoccupied dwellings
    }
    public bool IsResidentsFull(Character character) {
        if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.playerArea.id == this.id) {
            return false; //resident capacity is never full for player area
        }
        for (int i = 0; i < structures[STRUCTURE_TYPE.DWELLING].Count; i++) {
            Dwelling dwelling = structures[STRUCTURE_TYPE.DWELLING][i] as Dwelling;
            if (dwelling.CanBeResidentHere(character)) {
                return false;
            }
        }
        return true;
    }
    public int GetNumberOfUnoccupiedStructure(STRUCTURE_TYPE structureType) {
        if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.playerArea.id == this.id) {
            return 0;
        }
        return structures[structureType].Where(x => !x.IsOccupied()).Count();
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
                Character createdCharacter = CharacterManager.Instance.CreateNewCharacter(chosenClass.className, defaultRace.race, Utilities.GetRandomGender(), 
                    FactionManager.Instance.neutralFaction, this);
                createdCharacter.SetLevel(UnityEngine.Random.Range(setup.levelRange.lowerBound, setup.levelRange.upperBound + 1));
                //Debug.Log(GameManager.Instance.TodayLogString() + "Generated Lvl. " + createdCharacter.level.ToString() + 
                //    " neutral character " + createdCharacter.characterClass.className + " " + createdCharacter.name + " at " + this.name);
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
            Character createdCharacter = CharacterManager.Instance.CreateNewCharacter(chosenClass.className, this.raceType, Utilities.GetRandomGender(),
                owner, this);
            createdCharacter.SetLevel(owner.level);
            //Debug.Log(GameManager.Instance.TodayLogString() + "Generated Lvl. " + createdCharacter.level.ToString() +
            //        " character " + createdCharacter.characterClass.className + " " + createdCharacter.name + " at " + this.name + " for faction " + this.owner.name);
        }
    }
    public void GenerateStartingFollowers(int followersLevel) {
        if(owner != null) {
            for (int i = 0; i < owner.startingFollowers.Count; i++) {
                WeightedDictionary<AreaCharacterClass> classWeights = GetClassWeights(owner.startingFollowers[i]);
                AreaCharacterClass chosenClass = classWeights.PickRandomElementGivenWeights();
                Character createdCharacter = CharacterManager.Instance.CreateNewCharacter(chosenClass.className, owner.startingFollowers[i], Utilities.GetRandomGender(),
                    owner, this);
                createdCharacter.LevelUp(followersLevel - 1);
                //Debug.Log(GameManager.Instance.TodayLogString() + "Generated Lvl. " + createdCharacter.level.ToString() +
                //        " character " + createdCharacter.characterClass.className + " " + createdCharacter.name + " at " + this.name + " for faction " + this.owner.name);
            }
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
    public void SetMonthlyActions(int amount) {
        monthlyActions = amount;
    }
    public void SpawnRandomCharacters(int howMany) {
        if (IsResidentsFull()) {
            return;
        }
        WeightedDictionary<AreaCharacterClass> classWeights = GetClassWeights();
        for (int i = 0; i < howMany; i++) {
            if (IsResidentsFull()) {
                break;
            }
            string classNameToBeSpawned = classWeights.PickRandomElementGivenWeights().className;
            Character createdCharacter = CharacterManager.Instance.CreateNewCharacter(classNameToBeSpawned, raceType, Utilities.GetRandomGender(), owner, this);
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
    public List<Character> FormCombatCharacters() {
        List<Character> residentsAtArea = new List<Character>();
        CombatGrid combatGrid = new CombatGrid();
        combatGrid.Initialize();
        for (int i = 0; i < areaResidents.Count; i++) {
            Character resident = areaResidents[i];
            if (resident.isIdle && !resident.isLeader
                && resident.role.roleType != CHARACTER_ROLE.CIVILIAN
                && !resident.isDefender && resident.specificLocation.id == id && resident.currentStructure.isInside) {
                if((owner != null && resident.faction == owner) || (owner == null && resident.faction == FactionManager.Instance.neutralFaction)) {
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
                if (!attackCharacters.Contains(combatGrid.slots[i].character)){
                    attackCharacters.Add(combatGrid.slots[i].character);
                }
            }
        }
        return attackCharacters;
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
    public bool AddSpecialTokenToLocation(SpecialToken token, LocationStructure structure = null) {
        if (!IsItemInventoryFull() && !possibleSpecialTokenSpawns.Contains(token)) {
            possibleSpecialTokenSpawns.Add(token);
            Debug.Log(GameManager.Instance.TodayLogString() + "Added " + token.name + " at " + name);
            if (structure != null) {
                structure.AddItem(token);
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
            Messenger.Broadcast(Signals.ITEM_ADDED_TO_AREA, this, token);
            return true;
        }
        return false;
    }
    public void RemoveSpecialTokenFromLocation(SpecialToken token) {
        if (possibleSpecialTokenSpawns.Remove(token)) {
            token.structureLocation.RemoveItem(token);
            Debug.Log(GameManager.Instance.TodayLogString() + "Removed " + token.name + " from " + name);
            Messenger.Broadcast(Signals.ITEM_REMOVED_FROM_AREA, this, token);
        }
        
    }
    public bool IsItemInventoryFull() {
        return possibleSpecialTokenSpawns.Count >= MAX_ITEM_CAPACITY;
    }
    public List<SpecialToken> GetElligibleTokensForCharacter(Character character) {
        List<SpecialToken> choices = new List<SpecialToken>(possibleSpecialTokenSpawns);
        choices.Remove(character.tokenInInventory);
        //Utilities.ListRemoveRange(choices, character.tokenInInventory);
        return choices;
    }
    private LocationStructure GetRandomStructureToPlaceItem(SpecialToken token) {
        /*
         Items are now placed specifically in a structure when spawning at world creation. 
         Randomly place it at any non-Dwelling structure in the location.
         */
        List<LocationStructure> choices = new List<LocationStructure>();
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> kvp in structures) {
            if (kvp.Key != STRUCTURE_TYPE.DWELLING) {
                choices.AddRange(kvp.Value);
            }
        }
        if (choices.Count > 0) {
            return choices[UnityEngine.Random.Range(0, choices.Count)];
        }
        return null;
    }
    #endregion

    #region Tracking
    public void SetTrackedState(bool state) {
        isBeingTracked = state;
        for (int i = 0; i < charactersAtLocation.Count; i++) {
            Character currCharacter = charactersAtLocation[i];
            currCharacter.ownParty.icon.UpdateTravelLineVisualState();
        }
    }
    #endregion

    #region Corpses
    public void AddCorpse(Character character) {
        if (!HasCorpseOf(character)) {
            corpsesInArea.Add(new Corpse(character));
        }
    }
    public void RemoveCorpse(Character character) {
        corpsesInArea.Remove(GetCorpseOf(character));
    }
    private bool HasCorpseOf(Character character) {
        for (int i = 0; i < corpsesInArea.Count; i++) {
            Corpse currCorpse = corpsesInArea[i];
            if (currCorpse.character.id == character.id) {
                return true;
            }
        }
        return false;
    }
    private Corpse GetCorpseOf(Character character) {
        for (int i = 0; i < corpsesInArea.Count; i++) {
            Corpse currCorpse = corpsesInArea[i];
            if (currCorpse.character.id == character.id) {
                return currCorpse;
            }
        }
        return null;
    }
    #endregion

    #region Structures
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
                return structures[type][UnityEngine.Random.Range(0, structures[type].Count)];
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
                return choices[UnityEngine.Random.Range(0, choices.Count)];
            }
            
        }
        return null;
    }
    public LocationStructure GetRandomStructure() {
        int dictIndex = UnityEngine.Random.Range(0, structures.Count);
        int count = 0;
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> kvp in structures) {
            if(count == dictIndex) {
                return kvp.Value[UnityEngine.Random.Range(0, kvp.Value.Count)];
            }
            count++;
        }
        return null;
    }
    public List<LocationStructure> GetStructuresAtLocation(bool inside) {
        List<LocationStructure> structures = new List<LocationStructure>();
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> kvp in this.structures) {
            for (int i = 0; i < kvp.Value.Count; i++) {
                LocationStructure currStructure = kvp.Value[i];
                if (currStructure.isInside == inside) {
                    structures.Add(currStructure);
                }
            }
        }
        return structures;
    }
    public bool HasStructure(STRUCTURE_TYPE type) {
        return structures.ContainsKey(type);
    }
    public void SetDungeonSupplyRange(int min, int max) {
        SetDungeonSupplyMinRange(min);
        SetDungeonSupplyMaxRange(max);
    }
    public void SetDungeonSupplyMinRange(int min) {
        dungeonSupplyRangeMin = min;
    }
    public void SetDungeonSupplyMaxRange(int max) {
        dungeonSupplyRangeMax = max;
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