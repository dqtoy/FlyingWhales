using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Area {

    public int id { get; private set; }
    public string name { get; private set; }
    public float recommendedPower { get; private set; }
    public AREA_TYPE areaType { get; private set; }
    public List<HexTile> tiles { get; private set; }
    public HexTile coreTile { get; private set; }
    public Color areaColor { get; private set; }
    public Faction owner { get; private set; }
    public List<string> orderClasses { get; private set; }
    public List<StructurePriority> orderStructures { get; private set; }
    public int suppliesInBank { get; private set; }
    public List<BaseLandmark> landmarks { get { return tiles.Where(x => x.landmarkOnTile != null).Select(x => x.landmarkOnTile).ToList(); } }
    public int totalCivilians { get { return landmarks.Sum(x => x.civilianCount); } }
    public LocationIntel locationIntel { get; private set; }
    public List<BaseLandmark> exposedTiles { get; private set; }
    public List<BaseLandmark> unexposedTiles { get; private set; }
    public bool isHighlighted { get; private set; }
    public bool hasBeenInspected { get; private set; }
    public bool areAllLandmarksDead { get; private set; }
    public AreaInvestigation areaInvestigation { get; private set; }
    private List<HexTile> outerTiles;
    private List<SpriteRenderer> outline;

    public List<string> excessClasses;
    public List<string> missingClasses;
    public List<ICharacter> residents;

    public Area(HexTile coreTile, AREA_TYPE areaType) {
        id = Utilities.SetID(this);
        SetName(RandomNameGenerator.Instance.GetRegionName());
        tiles = new List<HexTile>();
        residents = new List<ICharacter>();
        orderClasses = new List<string>();
        excessClasses = new List<string>();
        missingClasses = new List<string>();
        exposedTiles = new List<BaseLandmark>();
        unexposedTiles = new List<BaseLandmark>();
        orderStructures = new List<StructurePriority>();
        areaColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        locationIntel = new LocationIntel(this);
        areaInvestigation = new AreaInvestigation(this); 
        SetAreaType(areaType);
        SetCoreTile(coreTile);
        AddTile(coreTile);
#if !WORLD_CREATION_TOOL
        StartSupplyLine();
#endif
    }
    public Area(AreaSaveData data) {
        id = Utilities.SetID(this, data.areaID);
        SetName(data.areaName);
        SetRecommendedPower(data.recommendedPower);
        tiles = new List<HexTile>();
        residents = new List<ICharacter>();
        excessClasses = new List<string>();
        missingClasses = new List<string>();
        exposedTiles = new List<BaseLandmark>();
        unexposedTiles = new List<BaseLandmark>();
        if (data.orderClasses != null) {
            orderClasses = data.orderClasses;
        } else {
            orderClasses = new List<string>();
        }
        if (data.orderStructures != null) {
            orderStructures = data.orderStructures;
        } else {
            orderStructures = new List<StructurePriority>();
        }
        areaColor = data.areaColor;
        SetAreaType(data.areaType);
        locationIntel = new LocationIntel(this);
        areaInvestigation = new AreaInvestigation(this);
#if WORLD_CREATION_TOOL
        SetCoreTile(worldcreator.WorldCreatorManager.Instance.GetHexTile(data.coreTileID));
#else
        SetCoreTile(GridMap.Instance.GetHexTile(data.coreTileID));
        StartSupplyLine();
#endif
        AddTile(Utilities.GetTilesFromIDs(data.tileData)); //exposed tiles will be determined after loading landmarks at MapGeneration
        UpdateBorderColors();
    }

    public void SetRecommendedPower(float power) {
        this.recommendedPower = power;
    }

    public void SetName(string name) {
        this.name = name;
    }

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
            removedTile.SetBaseSprite(Biomes.Instance.ancienctRuinTiles[Random.Range(0, Biomes.Instance.ancienctRuinTiles.Length)]);
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
            return exposedTiles[Random.Range(0, exposedTiles.Count)];
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
        if (state) {
            if (owner != null && owner.id != PlayerManager.Instance.player.playerFaction.id) {
                PlayerManager.Instance.player.AddIntel(owner.factionIntel);
            }
            if (id != PlayerManager.Instance.player.playerArea.id) {
                PlayerManager.Instance.player.AddIntel(locationIntel);
            }
        }
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
    #endregion

    #region Classes
    public bool HasResidentWithClass(string className) {
        for (int i = 0; i < residents.Count; i++) {
            if(residents[i].characterClass != null && residents[i].characterClass.className == className) {
                return true;
            }
        }
        return false;
    }
    public List<ICharacter> GetResidentsWithClass(string className) {
        List<ICharacter> characters = new List<ICharacter>();
        for (int i = 0; i < residents.Count; i++) {
            if (residents[i].characterClass != null && residents[i].characterClass.className == className) {
                characters.Add(residents[i]);
            }
        }
        return characters;
    }
    public void AddClassPriority(string newPrio) {
        orderClasses.Add(newPrio);
    }
    public void RemoveClassPriority(int index) {
        orderClasses.RemoveAt(index);
    }
    #endregion

    #region Structure Priorities
    public void AddStructurePriority(StructurePriority newPrio) {
        orderStructures.Add(newPrio);
    }
    public void RemoveStructurePriority(StructurePriority newPrio) {
        orderStructures.Remove(newPrio);
    }
    public StructurePriority GetNextStructurePriority() {
        List<LANDMARK_TYPE> currentLandmarks = new List<LANDMARK_TYPE>(landmarks.Select(x => x.specificLandmarkType));
        for (int i = 0; i < orderStructures.Count; i++) {
            StructurePriority currPrio = orderStructures[i];
            if (currentLandmarks.Contains(currPrio.setting.landmarkType)) {
                currentLandmarks.Remove(currPrio.setting.landmarkType);
            } else {
                return currPrio;
            }
        }
        return null;
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
        Messenger.AddListener(Signals.DAY_START, ExecuteSupplyLine);
    }
    private void ExecuteSupplyLine() {
        CollectDailySupplies();
        PayMaintenance();
        LandmarkStartDayActions();
    }
    private void CollectDailySupplies() {
        int totalCollectedSupplies = 0;
        string supplySummary = string.Empty;
        for (int i = 0; i < landmarks.Count; i++) {
            BaseLandmark currLandmark = landmarks[i];
            if (currLandmark.canProduceSupplies && !currLandmark.landmarkObj.isRuined && currLandmark.MeetsSupplyProductionRequirements()) {
                int providedSupplies = Random.Range(currLandmark.minDailySupplyProduction, currLandmark.maxDailySupplyProduction);
                totalCollectedSupplies += providedSupplies;
                AdjustSuppliesInBank(providedSupplies);
                supplySummary += currLandmark.name + "(" + currLandmark.specificLandmarkType.ToString() + ") - " + providedSupplies.ToString() + "\n";
            } else {
                supplySummary += currLandmark.name + "(" + currLandmark.specificLandmarkType.ToString() + ") - Cannot Produce\n";
            }
        }
        Debug.Log(this.name + " collected supplies " + totalCollectedSupplies + " Summary: \n" + supplySummary);
    }
    private void PayMaintenance() {
        //consumes Supply per existing unit
        for (int i = 0; i < landmarks.Count; i++) {
            BaseLandmark currLandmark = landmarks[i];
            if (currLandmark.defenders == null) {
                continue;
            }
            for (int j = 0; j < currLandmark.defenders.icharacters.Count; j++) {
                ICharacter currDefender = currLandmark.defenders.icharacters[j];
                if (currDefender is CharacterArmyUnit) {
                    CharacterArmyUnit armyUnit = currDefender as CharacterArmyUnit;
                    AdjustSuppliesInBank(-armyUnit.armyCount);
                } else if (currDefender is MonsterArmyUnit) {
                    MonsterArmyUnit armyUnit = currDefender as MonsterArmyUnit;
                    AdjustSuppliesInBank(-armyUnit.armyCount);
                } else {
                    AdjustSuppliesInBank(-1); //if just a single character or monster
                }
            }
        }
    }
    private void LandmarkStartDayActions() {
        for (int i = 0; i < landmarks.Count; i++) {
            BaseLandmark currLandmark = landmarks[i];
            currLandmark.landmarkObj.StartDayAction();
        }
    }
    public void AdjustSuppliesInBank(int amount) {
        suppliesInBank += amount;
        suppliesInBank = Mathf.Max(0, suppliesInBank);
    }
    public bool HasEnoughSupplies(int neededSupplies) {
        return suppliesInBank >= neededSupplies;
    }
    public void PayForReward(Reward reward) {
        if (reward.rewardType == REWARD.SUPPLY) {
            AdjustSuppliesInBank(-reward.amount);
        }
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
}
