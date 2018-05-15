/*
 HEX TILE FEATURES:
- major deadend (connected to only 1 or 2 adjacent passable tiles)
- minor deadend (connected to only 3 adjacent passable tiles)
- major bottleneck (connected to 2 unadjacent passable tiles)
- minor bottleneck (connected to 2 unadjacent pairs of either 1 or 2 adjacent passable tiles)
- crossroad (connected to 3 unadjacent passable tiles)
- wide open (connected to 4o to 6 passable tiles)
- open (the rest)
*/

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PassableTileData {

    public List<TileCollection> adjacentTiles; //neighbour tiles that have adjacent passable tiles
    public List<TileCollection> unadjacentTiles; //neighbour tiles that have no adjacent passable tiles

    public PassableTileData(HexTile tile) {
        adjacentTiles = new List<TileCollection>();
        unadjacentTiles = new List<TileCollection>();
        ComputeData(tile);
    }

    private void ComputeData(HexTile mainTile) {
        List<HexTile> mainPassableNeighbours = mainTile.AllNeighbours.Where(x => x.isPassable).ToList();
        for (int i = 0; i < mainPassableNeighbours.Count; i++) {
            HexTile currTile = mainPassableNeighbours[i];
            if (IsInUnadjacentList(currTile)) {
                continue; //skip
            }
            bool hasAdjacency = false;
            for (int j = 0; j < currTile.AllNeighbours.Count; j++) {
                HexTile neighbourOfCurrTile = currTile.AllNeighbours[j];
                if (mainPassableNeighbours.Contains(neighbourOfCurrTile)) {
                    hasAdjacency = true;
                    //this tile is adjacent to another passable tile
                    if (IsInAdjacentList(currTile) && IsInAdjacentList(neighbourOfCurrTile)) {
                        //both tiles are already in a list, merge them
                        TileCollection collectionOfCurrTile = GetAdjacentListOf(currTile);
                        TileCollection collectionOfNeighbour = GetAdjacentListOf(neighbourOfCurrTile);
                        collectionOfCurrTile.AddTile(collectionOfNeighbour.tiles);
                        adjacentTiles.Remove(collectionOfNeighbour);
                    } else if (IsInAdjacentList(neighbourOfCurrTile)) {
                        //the adjacent neighbour is already in a collection, add the current tile to that collection instead
                        GetAdjacentListOf(neighbourOfCurrTile).AddTile(currTile);
                    } else if (IsInAdjacentList(currTile)) {
                        //the current neighbour is already in a collection, add the neigbour to that collection instead
                        GetAdjacentListOf(currTile).AddTile(neighbourOfCurrTile);
                    } else {
                        //the adjacent neighbour is not yet in a collection, create a new one.
                        TileCollection newCollection = new TileCollection(currTile);
                        newCollection.AddTile(neighbourOfCurrTile);
                        adjacentTiles.Add(newCollection);
                    }
                }
            }
            if (!hasAdjacency) { //this tile is not adjacent to any passable tile
                unadjacentTiles.Add(new TileCollection(currTile));
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

    public bool IsDeadEnd() {
        if ((adjacentTiles.Count == 1 && adjacentTiles[0].tiles.Count <= 2 &&  unadjacentTiles.Count == 0)
            || (unadjacentTiles.Count == 1 && adjacentTiles.Count == 0)) {
            return true;
        }
        return false;
    }

    public bool HasNumberOfAdjacentTiles(int tileCount) {
        for (int i = 0; i < adjacentTiles.Count; i++) {
            TileCollection currCollection = adjacentTiles[i];
            if (currCollection.tiles.Count == tileCount) {
                return true;
            }
        }
        return false;
    }
    public bool HasNumberOfUnadjacentTiles(int tileCount) {
        if (unadjacentTiles.Count == 1) {
            TileCollection currCollection = unadjacentTiles[0];
            if (currCollection.tiles.Count == tileCount) {
                return true;
            }
        }
        return false;
    }
    public int TotalPassableNeighbours(HexTile mainTile) {
        return mainTile.AllNeighbours.Where(x => x.isPassable).Count();
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
    public void AddTile(List<HexTile> tiles) {
        for (int i = 0; i < tiles.Count; i++) {
            AddTile(tiles[i]);
        }
    }
}
