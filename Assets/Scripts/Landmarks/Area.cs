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
        if (revalidateTiles) {
            RevalidateTiles();
        }
    }
    public void AddTile(HexTile tile, bool revalidateTiles = true) {
        if (!tiles.Contains(tile)) {
            tiles.Add(tile);
            tile.SetArea(this);
            tile.SetMinimapTileColor(areaColor);
            if (revalidateTiles) {
                RevalidateTiles();
            }
            Messenger.Broadcast(Signals.AREA_TILE_ADDED, this, tile);
        }
    }
    public void RemoveTile(List<HexTile> tiles, bool revalidateTiles = true) {
        for (int i = 0; i < tiles.Count; i++) {
            RemoveTile(tiles[i], false);
        }
        if (revalidateTiles) {
            RevalidateTiles();
        }
    }
    public void RemoveTile(HexTile tile, bool revalidateTiles = true) {
        tiles.Remove(tile);
        tile.SetArea(null);
        if (revalidateTiles) {
            RevalidateTiles();
        }
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
    #endregion

    #region Area Type
    public void SetAreaType(AREA_TYPE areaType) {
        this.areaType = areaType;
    }
    public BASE_AREA_TYPE GetBaseAreaType() {
        AreaData data = LandmarkManager.Instance.GetAreaData(areaType);
        return data.baseAreaType;
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
        UpdateExcessAndMissingClasses();
        ScheduleStartOfMonthActions();
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
        int count = newResidents.Count;
        for (int i = 0; i < count; i++) {
            ICharacter resident = ResidentsHasClass(orderClasses[i], newResidents);
            if (resident != null) {
                newResidents.Remove(resident);
            } else {
                missingClasses.Add(orderClasses[i]);
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
}
