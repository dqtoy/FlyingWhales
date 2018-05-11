using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PassableTileData {

    public List<TileCollection> adjacentTiles;
    public List<TileCollection> unadjacentTiles;

    public PassableTileData(HexTile tile) {
        adjacentTiles = new List<TileCollection>();
        unadjacentTiles = new List<TileCollection>();
        ComputeData(tile);
    }

    private void ComputeData(HexTile mainTile) {
        List<HexTile> mainPassableNeighbours = mainTile.AllNeighbours.Where(x => x.isPassable).ToList();
        for (int i = 0; i < mainPassableNeighbours.Count; i++) {
            HexTile currTile = mainPassableNeighbours[i];
            //if (IsInAdjacentList(currTile) || IsInUnadjacentList(currTile)) {
            //    continue; //skip
            //}
            //bool hasAdjacency = false;
            for (int j = 0; j < currTile.AllNeighbours.Count; j++) {
                HexTile neighbourOfCurrTile = currTile.AllNeighbours[j];
                if (mainPassableNeighbours.Contains(neighbourOfCurrTile)) {
                    //hasAdjacency = true;
                    //this tile is adjacent to another passable tile
                    if (IsInAdjacentList(neighbourOfCurrTile)) {
                        //the adjacent neighbour is already in a collection, add the current tile to that collection instead
                        GetAdjacentListOf(neighbourOfCurrTile).AddTile(currTile);
                    } else if (IsInAdjacentList(currTile)) {
                        //the adjacent neighbour is already in a collection, add the current tile to that collection instead
                        GetAdjacentListOf(currTile).AddTile(neighbourOfCurrTile);
                    } else {
                        //the adjacent neighbour is not yet in a collection, create a new one.
                        TileCollection newCollection = new TileCollection(currTile);
                        newCollection.AddTile(neighbourOfCurrTile);
                        adjacentTiles.Add(newCollection);
                    }
                }
            }
        }
    }

    private bool IsInAdjacentList(HexTile tile) {
        for (int i = 0; i < adjacentTiles.Count; i++) {
            TileCollection currCollection = adjacentTiles[i];
            if (currCollection.Contains(tile)) {
                return true;
            }
        }
        return false;
    }
    private TileCollection GetAdjacentListOf(HexTile tile) {
        for (int i = 0; i < adjacentTiles.Count; i++) {
            TileCollection currCollection = adjacentTiles[i];
            if (currCollection.Contains(tile)) {
                return currCollection;
            }
        }
        throw new System.Exception("No adjacent list for " + tile.name);
    }
    private bool IsInUnadjacentList(HexTile tile) {
        for (int i = 0; i < unadjacentTiles.Count; i++) {
            TileCollection currCollection = unadjacentTiles[i];
            if (currCollection.Contains(tile)) {
                return true;
            }
        }
        return false;
    }
    private TileCollection GetUnadjacentListOf(HexTile tile) {
        for (int i = 0; i < unadjacentTiles.Count; i++) {
            TileCollection currCollection = unadjacentTiles[i];
            if (currCollection.Contains(tile)) {
                return currCollection;
            }
        }
        throw new System.Exception("No unadjacent list for " + tile.name);
    }
}

public struct TileCollection {
    public List<HexTile> tiles;

    public TileCollection(HexTile tile) {
        tiles = new List<HexTile>();
        AddTile(tile);
    }

    public bool Contains(HexTile tile) {
        return tiles.Contains(tile);
    }

    public void AddTile(HexTile tile) {
        if (!tiles.Contains(tile)) {
            tiles.Add(tile);
        }
    }
}
