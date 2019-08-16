using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Region {

    private const float Hovered_Border_Alpha = 255f / 255f;
    private const float Unhovered_Border_Alpha = 128f / 255f;

    public int id { get; private set; }
    public string name { get; private set; }
    public List<HexTile> tiles { get; private set; }
    public HexTile coreTile { get; private set; }
    public int ticksInInvasion { get; private set; }
    private Color regionColor;
    private List<HexTile> outerTiles;
    private List<SpriteRenderer> borderSprites;

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
        CameraMove.Instance.CenterCameraOn(this.coreTile.gameObject);
    }
    #endregion

    #region Invasion
    public bool CanBeInvaded() {
        return mainLandmark.HasCorruptedConnection() && !coreTile.isCorrupted && !PlayerManager.Instance.player.isInvadingRegion;
    }
    public void StartInvasion() {
        PlayerManager.Instance.player.SetInvadingRegion(this);
        ticksInInvasion = 0;
        Messenger.AddListener(Signals.TICK_STARTED, PerInvasionTick);
    }
    private void Invade() {
        //corrupt region
        PlayerManager.Instance.AddTileToPlayerArea(coreTile);
        mainLandmark.ObtainWorldWobject();
        PlayerManager.Instance.player.SetInvadingRegion(null);
        //TODO: Place invasion actions here
    }
    private void PerInvasionTick() {
        if (ticksInInvasion >= mainLandmark.invasionTicks) {
            //invaded.
            Invade();
            Messenger.RemoveListener(Signals.TICK_STARTED, PerInvasionTick);
        } else {
            ticksInInvasion += GameManager.Instance.ticksToAddPerTick;
        }
    }
    #endregion

}
