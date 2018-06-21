using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Area {

    public int id { get; private set; }
    public string name { get; private set; }
    public AREA_TYPE areaType { get; private set; }
    public List<HexTile> tiles { get; private set; }
    public HexTile coreTile { get; private set; }

    private Color _areaColor;

    public Area(HexTile coreTile, AREA_TYPE areaType) {
        id = Utilities.SetID(this);
        name = RandomNameGenerator.Instance.GetRegionName();
        tiles = new List<HexTile>();
        _areaColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        SetAreaType(areaType);
        SetCoreTile(coreTile);
        //AddTile(coreTile);
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
            if (revalidateTiles) {
                RevalidateTiles();
            }
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
    #endregion

    #region Area Type
    public void SetAreaType(AREA_TYPE areaType) {
        this.areaType = areaType;
    }
    #endregion

    #region Visuals
    public void HighlightArea() {
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currTile = tiles[i];
#if WORLD_CREATION_TOOL
            if (!worldcreator.WorldCreatorManager.Instance.selectionComponent.selection.Contains(currTile)) {
                if (currTile.id == coreTile.id) {
                    currTile.HighlightTile(_areaColor, 255f/255f);
                } else {
                    currTile.HighlightTile(_areaColor, 128f/255f);
                }
            }
#else
            if (currTile.id == coreTile.id) {
                currTile.HighlightTile(_areaColor, 255f/255f);
            } else {
                currTile.HighlightTile(_areaColor, 128f/255f);
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
    #endregion

}
