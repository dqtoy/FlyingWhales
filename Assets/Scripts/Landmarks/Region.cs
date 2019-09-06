using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Region {

    private const float Hovered_Border_Alpha = 0f / 255f;
    private const float Unhovered_Border_Alpha = 0f / 255f;

    public int id { get; private set; }
    public string name { get; private set; }
    public List<HexTile> tiles { get; private set; }
    public HexTile coreTile { get; private set; }
    public int ticksInInvasion { get; private set; }
    public Color regionColor { get; private set; }
    private List<HexTile> outerTiles;
    private List<SpriteRenderer> borderSprites;

    public Minion assignedMinion { get; private set; }

    //Player Building Demonic Landmark
    public DemonicLandmarkBuildingData demonicBuildingData { get; private set; }

    #region getter/setter
    public BaseLandmark mainLandmark {
        get { return coreTile.landmarkOnTile; }
    }
    #endregion

    public Region(HexTile coreTile) {
        id = Utilities.SetID(this);
        name = RandomNameGenerator.Instance.GetRegionName();
        this.coreTile = coreTile;
        tiles = new List<HexTile>();
        AddTile(coreTile);
        regionColor = Random.ColorHSV();
    }
    public Region(SaveDataRegion data) {
        id = Utilities.SetID(this, data.id);
        name = data.name;
        coreTile = GridMap.Instance.hexTiles[data.coreTileID];
        tiles = new List<HexTile>();
        regionColor = data.regionColor;
        ticksInInvasion = data.ticksInInvasion;
    }
    public void AddTile(HexTile tile) {
        if (!tiles.Contains(tile)) {
            tiles.Add(tile);
            tile.SetRegion(this);
            //if (tile != coreTile) {
            //    tile.spriteRenderer.color = regionColor;
            //}
        }
    }

    #region Utilities
    public void FinalizeData() {
        outerTiles = GetOuterTiles();
        borderSprites = GetOuterBorders();
    }
    public void RedetermineCore() {
        int maxX = tiles.Max(t => t.data.xCoordinate);
        int minX = tiles.Min(t => t.data.xCoordinate);
        int maxY = tiles.Max(t => t.data.yCoordinate);
        int minY = tiles.Min(t => t.data.yCoordinate);

        int x = (minX + maxX) / 2;
        int y = (minY + maxY) / 2;

        //coreTile.spriteRenderer.color = regionColor;

        coreTile = GridMap.Instance.map[x, y];
        //coreTile.spriteRenderer.color = Color.white;

        if (!tiles.Contains(coreTile)) {
            throw new System.Exception("Region does not contain new core tile! " + coreTile.ToString());
        }
    }
    /// <summary>
    /// Get the outer tiles of this region. NOTE: Made this into a getter instead of saving it in a variable, to save memory.
    /// </summary>
    /// <returns>List of outer tiles.</returns>
    private List<HexTile> GetOuterTiles() {
        List<HexTile> outerTiles = new List<HexTile>();
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currTile = tiles[i];
            if (currTile.AllNeighbours.Count != 6 || currTile.HasNeighbourFromOtherRegion()) {
                outerTiles.Add(currTile);
            }
        }
        return outerTiles;
    }
    private List<SpriteRenderer> GetOuterBorders() {
        List<HexTile> outerTiles = GetOuterTiles();
        List<SpriteRenderer> borders = new List<SpriteRenderer>();
        HEXTILE_DIRECTION[] dirs = Utilities.GetEnumValues<HEXTILE_DIRECTION>();
        for (int i = 0; i < outerTiles.Count; i++) {
            HexTile currTile = outerTiles[i];
            for (int j = 0; j < dirs.Length; j++) {
                HEXTILE_DIRECTION dir = dirs[j];
                if (dir == HEXTILE_DIRECTION.NONE) { continue; }
                HexTile neighbour = currTile.GetNeighbour(dir);
                if (neighbour == null || neighbour.region != currTile.region) {
                    SpriteRenderer border = currTile.GetBorder(dir);
                    //currTile.SetBorderColor(regionColor);
                    borders.Add(border);
                }
            }
        }
        return borders;
    }
    public List<Region> AdjacentRegions() {
        List<Region> adjacent = new List<Region>();
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currTile = tiles[i];
            List<Region> regions;
            if (currTile.TryGetDifferentRegionNeighbours(out regions)) {
                for (int j = 0; j < regions.Count; j++) {
                    Region currRegion = regions[j];
                    if (!adjacent.Contains(currRegion)) {
                        adjacent.Add(currRegion);
                    }
                }
            }
        }
        return adjacent;
    }
    public void OnHoverOverAction() {
        ShowSolidBorder();
    }
    public void OnHoverOutAction() {
        if (UIManager.Instance.regionInfoUI.isShowing) {
            if (UIManager.Instance.regionInfoUI.activeRegion != this) {
                ShowTransparentBorder();
            }
        } else {
            ShowTransparentBorder();
        }
        
    }
    public void ShowSolidBorder() {
        for (int i = 0; i < borderSprites.Count; i++) {
            SpriteRenderer s = borderSprites[i];
            Color color = s.color;
            color.a = Hovered_Border_Alpha;
            s.color = color;
            s.gameObject.SetActive(true);
        }
    }
    public void ShowTransparentBorder() {
        for (int i = 0; i < borderSprites.Count; i++) {
            SpriteRenderer s = borderSprites[i];
            Color color = s.color;
            color.a = Unhovered_Border_Alpha;
            s.color = color;
            s.gameObject.SetActive(true);
        }
    }
    public void CenterCameraOnRegion() {
        coreTile.CenterCameraHere();
    }
    #endregion

    #region Invasion
    public bool CanBeInvaded() {
        return mainLandmark.HasCorruptedConnection() && !coreTile.isCorrupted && !PlayerManager.Instance.player.isInvadingRegion;
    }
    public void StartInvasion(Minion assignedMinion) {
        PlayerManager.Instance.player.SetInvadingRegion(this);
        assignedMinion.SetAssignedRegion(this);
        SetAssignedMinion(assignedMinion);

        ticksInInvasion = 0;
        Messenger.AddListener(Signals.TICK_STARTED, PerInvasionTick);
        TimerHubUI.Instance.AddItem("Invasion of " + (mainLandmark.tileLocation.areaOfTile != null ? mainLandmark.tileLocation.areaOfTile.name : name), mainLandmark.invasionTicks, () => UIManager.Instance.ShowHextileInfo(this.mainLandmark.tileLocation));
    }
    public void LoadInvasion(int ticksInInvasion) {
        PlayerManager.Instance.player.SetInvadingRegion(this);
        //assignedMinion.SetAssignedRegion(this);
        //SetAssignedMinion(assignedMinion);

        this.ticksInInvasion = ticksInInvasion;
        Messenger.AddListener(Signals.TICK_STARTED, PerInvasionTick);
        TimerHubUI.Instance.AddItem("Invasion of " + (mainLandmark.tileLocation.areaOfTile != null ? mainLandmark.tileLocation.areaOfTile.name : name), mainLandmark.invasionTicks - ticksInInvasion, () => UIManager.Instance.ShowHextileInfo(this.mainLandmark.tileLocation));
    }
    private void Invade() {
        //corrupt region
        mainLandmark?.InvadeThisLandmark();
        PlayerManager.Instance.AddTileToPlayerArea(coreTile);
        PlayerManager.Instance.player.SetInvadingRegion(null);
        assignedMinion.SetAssignedRegion(null);
        SetAssignedMinion(null);

        //This is done so that when a region is invaded by the player, the showing Info UI will update appropriately
        if(UIManager.Instance.regionInfoUI.isShowing && UIManager.Instance.regionInfoUI.activeRegion == this) {
            UIManager.Instance.ShowHextileInfo(coreTile);
        }
    }
    public void SetAssignedMinion(Minion minion) {
        assignedMinion = minion;
    }
    private void PerInvasionTick() {
        if (ticksInInvasion >= mainLandmark.invasionTicks) {
            //invaded.
            Invade();
            Messenger.RemoveListener(Signals.TICK_STARTED, PerInvasionTick);
        } else {
            ticksInInvasion += 1;
        }
    }
    #endregion

    #region Player Build Structure
    public void StartBuildingStructure(LANDMARK_TYPE landmarkType, Minion minion) {
        SetAssignedMinion(minion);
        minion.SetAssignedRegion(this);
        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(landmarkType);
        demonicBuildingData = new DemonicLandmarkBuildingData() {
            landmarkType = landmarkType,
            landmarkName = landmarkData.landmarkTypeString,
            buildDuration = landmarkData.buildDuration,
            currentDuration = 0,
        };
        TimerHubUI.Instance.AddItem("Building " + demonicBuildingData.landmarkName + " at " + name, demonicBuildingData.buildDuration, () => UIManager.Instance.ShowHextileInfo(coreTile));
        Messenger.AddListener(Signals.TICK_STARTED, PerTickBuilding);
    }
    public void LoadBuildingStructure(SaveDataRegion data) {
        demonicBuildingData = data.demonicBuildingData;
        TimerHubUI.Instance.AddItem("Building " + demonicBuildingData.landmarkName + " at " + name, demonicBuildingData.buildDuration - demonicBuildingData.currentDuration, () => UIManager.Instance.ShowHextileInfo(coreTile));
        Messenger.AddListener(Signals.TICK_STARTED, PerTickBuilding);
    }
    private void PerTickBuilding() {
        if(demonicBuildingData.currentDuration >= demonicBuildingData.buildDuration) {
            FinishBuildingStructure();
        } else {
            DemonicLandmarkBuildingData tempData = demonicBuildingData;
            tempData.currentDuration++;
            demonicBuildingData = tempData;
        }
    }
    private void FinishBuildingStructure() {
        Messenger.RemoveListener(Signals.TICK_STARTED, PerTickBuilding);
        mainLandmark.ChangeLandmarkType(demonicBuildingData.landmarkType);

        demonicBuildingData = new DemonicLandmarkBuildingData();
        assignedMinion.SetAssignedRegion(null);
        SetAssignedMinion(null);
        Messenger.Broadcast(Signals.AREA_INFO_UI_UPDATE_APPROPRIATE_CONTENT, this);
    }
    #endregion
}
