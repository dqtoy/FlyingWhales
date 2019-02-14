using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class LocationGridTile {

    public enum Tile_Type { Empty, Wall, Structure, Gate }

    public Tilemap parentMap { get; private set; }
    public Vector3Int localPlace { get; private set; }
    public Vector3 worldLocation { get; private set; }
    public bool isInside { get; private set; }
    public Tile_Type tileType { get; private set; }

    [SerializeField] private Image image;
    [SerializeField] private Image topImage;

    public Dictionary<TileNeighbourDirection, LocationGridTile> neighbours;

    public LocationGridTile(int x, int y, Tilemap tilemap) {
        parentMap = tilemap;
        localPlace = new Vector3Int(x, y, 0);
        worldLocation = tilemap.CellToWorld(localPlace);
        //xCoordinate = x;
        //yCoordinate = y;
        //name = x.ToString() + ", " + y.ToString();
    }

    public void FindNeighbours(LocationGridTile[,] map) {
        neighbours = new Dictionary<TileNeighbourDirection, LocationGridTile>();
        Point thisPoint = new Point(localPlace.x, localPlace.y);
        foreach (KeyValuePair<TileNeighbourDirection, Point> kvp in possibleExits) {
            TileNeighbourDirection currDir = kvp.Key;
            Point exit = kvp.Value;
            Point result = exit.Sum(thisPoint);
            if (Utilities.IsInRange(result.X, 0, GridTownGenerator.Instance.width) &&
                Utilities.IsInRange(result.Y, 0, GridTownGenerator.Instance.height)) {
                neighbours.Add(currDir, map[result.X, result.Y]);
            }
        }
    }

    public Dictionary<TileNeighbourDirection, Point> possibleExits {
        get {
            return new Dictionary<TileNeighbourDirection, Point>() {
                {TileNeighbourDirection.Top, new Point(0,1) },
                {TileNeighbourDirection.Bottom, new Point(0,-1) },
                {TileNeighbourDirection.Left, new Point(-1,0) },
                {TileNeighbourDirection.Right, new Point(1,0) },
            };
        }
    }

    public void SetIsInside(bool isInside) {
        this.isInside = isInside;
        if (isInside) {
            image.sprite = GridTownGenerator.Instance.insideSprite;
        } else {
            image.sprite = GridTownGenerator.Instance.outsideSprite;
        }
    }

    public void SetTileType(Tile_Type tileType) {
        this.tileType = tileType;
        switch (tileType) {
            case Tile_Type.Wall:
                //UpdateWallVisual(true);
                break;
            default:
                break;
        }
    }
    public void UpdateWallVisual(bool updateNeighbours = false) {
        List<TileNeighbourDirection> wallDirections = new List<TileNeighbourDirection>();
        foreach (KeyValuePair<TileNeighbourDirection, LocationGridTile> kvp in neighbours) {
            if (kvp.Value.tileType == Tile_Type.Wall) {
                wallDirections.Add(kvp.Key);
                if (updateNeighbours) {
                    kvp.Value.UpdateWallVisual();
                }
            }
        }
        if (wallDirections.Count > 2) {
            Debug.LogWarning("There is a tile with more than 2 wall neighbours!");
        } else if (wallDirections.Count == 2) {
            TileNeighbourDirection dir1 = wallDirections[0];
            TileNeighbourDirection dir2 = wallDirections[1];
            Sprite wallSprite = GridTownGenerator.Instance.GetWallSprite(new TwoTileDirections(dir1, dir2));
            if (wallSprite == null) {
                wallSprite = GridTownGenerator.Instance.GetWallSprite(new TwoTileDirections(dir2, dir1));
            }
            SetTopImage(wallSprite);
        } else if (wallDirections.Count == 1) {
            //assume wall is next to an empty space, or edge of map
            TileNeighbourDirection dir1 = wallDirections[0];
            TileNeighbourDirection dir2 = TileNeighbourDirection.None;
            Sprite wallSprite = GridTownGenerator.Instance.GetWallSprite(new TwoTileDirections(dir1, dir2));
            SetTopImage(wallSprite);
        } else if (wallDirections.Count == 0) {
            SetTopImage(GridTownGenerator.Instance.GetWallSprite(new TwoTileDirections(TileNeighbourDirection.Left, TileNeighbourDirection.Right)));
        }
    }
    public void SetTopImage(Sprite sprite) {
        topImage.sprite = sprite;
        topImage.gameObject.SetActive(topImage.sprite != null);
    }


    #region Utilities
    public bool HasOutsideNeighbour() {
        foreach (KeyValuePair<TileNeighbourDirection, LocationGridTile> kvp in neighbours) {
            if (!kvp.Value.isInside) {
                return true;
            }
        }
        return false;
    }
    #endregion
}

[System.Serializable]
public struct TwoTileDirections {
    public TileNeighbourDirection from;
    public TileNeighbourDirection to;

    public TwoTileDirections(TileNeighbourDirection from, TileNeighbourDirection to) {
        this.from = from;
        this.to = to;
    }
}
