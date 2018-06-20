using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Area {

    private List<HexTile> _tiles;
    private HexTile _coreTile;

    public Area(HexTile coreTile) {
        _coreTile = coreTile;
        _tiles = new List<HexTile>();
        AddTile(coreTile);
    }

    #region Tile Management
    public void AddTile(HexTile tile) {
        if (!_tiles.Contains(tile)) {
            _tiles.Add(tile);
        }
    }
    public void RemoveTile(HexTile tile) {
        _tiles.Remove(tile);
    }
    private void RevalidateTiles() {
        //TODO: An Area has a primary core tile and any tile that isnt connected to it should also automatically be removed from it.
    }
    #endregion

}
