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
        orderStructures = new List<StructurePriority>();
        areaColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        SetAreaType(areaType);
        SetCoreTile(coreTile);
        AddTile(coreTile);
#if !WORLD_CREATION_TOOL
        ScheduleFirstAction();
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
#if WORLD_CREATION_TOOL
        SetCoreTile(worldcreator.WorldCreatorManager.Instance.GetHexTile(data.coreTileID));
#else
        SetCoreTile(GridMap.Instance.GetHexTile(data.coreTileID));
        ScheduleFirstAction();
        StartSupplyLine();
#endif
        AddTile(Utilities.GetTilesFromIDs(data.tileData));
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
    public void AddTile(List<HexTile> tiles, bool revalidateTiles = true) {
        for (int i = 0; i < tiles.Count; i++) {
            AddTile(tiles[i], false);
        }
        //if (revalidateTiles) {
        //    RevalidateTiles();
        //}
    }
    public void AddTile(HexTile tile, bool revalidateTiles = true) {
        if (!tiles.Contains(tile)) {
            tiles.Add(tile);
            tile.SetArea(this);
            tile.SetMinimapTileColor(areaColor);
            //if (revalidateTiles) {
            //    RevalidateTiles();
            //}
            OnTileAddedToArea(tile);
            Messenger.Broadcast(Signals.AREA_TILE_ADDED, this, tile);
        }
    }
    public void RemoveTile(List<HexTile> tiles, bool revalidateTiles = true) {
        for (int i = 0; i < tiles.Count; i++) {
            RemoveTile(tiles[i], false);
        }
        //if (revalidateTiles) {
        //    RevalidateTiles();
        //}
    }
    public void RemoveTile(HexTile tile, bool revalidateTiles = true) {
        tiles.Remove(tile);
        tile.SetArea(null);
        //if (revalidateTiles) {
        //    RevalidateTiles();
        //}
        OnTileRemovedFromArea(tile);
        Messenger.Broadcast(Signals.AREA_TILE_REMOVED, this, tile);
    }
    private void RevalidateTiles() {
        List<HexTile> tilesToCheck = new List<HexTile>(tiles);
        tilesToCheck.Remove(coreTile);
        while (tilesToCheck.Count != 0) {
            HexTile currTile = tilesToCheck[0];
            if (PathGenerator.Instance.GetPath(currTile, coreTile, PATHFINDING_MODE.AREA_ONLY, this) == null) {
                RemoveTile(currTile); //Remove tile from area
                currTile.UnHighlightTile();
            }
            tilesToCheck.Remove(currTile);
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
    #endregion

    #region Owner
    public void SetOwner(Faction owner) {
        this.owner = owner;
    }
    #endregion

    #region Utilities
    public bool HasLandmarkOfType(LANDMARK_TYPE type) {
        return landmarks.Where(x => x.specificLandmarkType == type).Any();
    }
    private void StartOfMonth() {
        if(orderClasses.Count > 0) {
            UpdateExcessAndMissingClasses();
            ScheduleStartOfMonthActions();
        }
    }
    private void ScheduleStartOfMonthActions() {
        GameDate gameDate = GameManager.Instance.Today();
        gameDate.SetHours(1);
        gameDate.AddDays(1);
        SchedulingManager.Instance.AddEntry(gameDate, () => StartOfMonth());
    }
    private void ScheduleFirstAction() {
        GameDate gameDate = new GameDate(1, 1, GameManager.Instance.year, 1);
        SchedulingManager.Instance.AddEntry(gameDate, () => StartOfMonth());
    }
    #endregion

    #region Classes
    private void UpdateExcessAndMissingClasses() {
        missingClasses.Clear();
        excessClasses.Clear();
        List<ICharacter> newResidents = new List<ICharacter>(residents);
        //int count = newResidents.Count;
        for (int i = 0; i < orderClasses.Count; i++) {
            string prioClass = orderClasses[i];
            ICharacter resident = ResidentsHasClass(prioClass, newResidents);
            if (resident != null) {
                newResidents.Remove(resident);
            } else {
                missingClasses.Add(prioClass);
            }
        }
        for (int i = 0; i < newResidents.Count; i++) {
            if(newResidents[i].characterClass != null) {
                excessClasses.Add(newResidents[i].characterClass.className);
            }
        }
    }
    private ICharacter ResidentsHasClass(string className, List<ICharacter> residents) {
        for (int i = 0; i < residents.Count; i++) {
            if(residents[i].characterClass != null && residents[i].characterClass.className == className) {
                return residents[i];
            }
        }
        return null;
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
        BaseLandmark camp = LandmarkManager.Instance.CreateNewLandmarkOnTile(tile, LANDMARK_TYPE.CAMP);
        camp.tileLocation.SetArea(this);
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
        Messenger.AddListener(Signals.DAY_START, ExecuteSupplyLine);
    }
    private void ExecuteSupplyLine() {
        CollectDailySupplies();
        PayMaintenance();
        LandmarkStartDayActions();
    }
    private void CollectDailySupplies() {
        int totalCollectedSupplies = 0;
        for (int i = 0; i < landmarks.Count; i++) {
            BaseLandmark currLandmark = landmarks[i];
            LandmarkData data = LandmarkManager.Instance.GetLandmarkData(currLandmark.specificLandmarkType);
            totalCollectedSupplies += data.dailySupplyProduction;
            AdjustSuppliesInBank(data.dailySupplyProduction);
        }
        Debug.Log(this.name + " collected supplies " + totalCollectedSupplies);
    }
    private void PayMaintenance() {
        //consumes Supply per existing unit
        for (int i = 0; i < landmarks.Count; i++) {
            BaseLandmark currLandmark = landmarks[i];
            for (int j = 0; j < currLandmark.defenders.Length; j++) {
                Party currDefenderParty = currLandmark.defenders[j];
                if (currDefenderParty != null) {
                    for (int k = 0; k < currDefenderParty.icharacters.Count; k++) {
                        ICharacter currCharacter = currDefenderParty.icharacters[k];
                        if (currCharacter is CharacterArmyUnit) {
                            CharacterArmyUnit armyUnit = currCharacter as CharacterArmyUnit;
                            AdjustSuppliesInBank(-armyUnit.armyCount);
                        } else if (currCharacter is MonsterArmyUnit) {
                            MonsterArmyUnit armyUnit = currCharacter as MonsterArmyUnit;
                            AdjustSuppliesInBank(-armyUnit.armyCount);
                        } else {
                            AdjustSuppliesInBank(-1); //if just a single character or monster
                        }
                    }
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
    #endregion
}
