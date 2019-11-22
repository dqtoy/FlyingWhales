using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LocationStructureObject : MonoBehaviour {

    [Header("Tilemaps")]
    [SerializeField] private Tilemap groundTileMap;
    [SerializeField] private Tilemap wallTileMap;
    [SerializeField] private Tilemap objectTileMap;
    [SerializeField] private Tilemap deatilTileMap;

    [SerializeField] private Vector2Int size;
    [SerializeField] private Transform contentParent;
    [SerializeField] private Vector3Int center;

    public LocationGridTile[] tiles;

    #region Testers
    [ContextMenu("Center Content")]
    public void CenterContent() {
        Vector2 center = new Vector2(size.x / 2f, size.y / 2f);
        center.x += 0.5f;
        center.y += 0.5f;
        contentParent.localPosition = center * -1f;
    }
    [ContextMenu("Log Occupied coordinates")]
    public void LogOccupiedCoordinates() {
        groundTileMap.CompressBounds();
        BoundsInt bounds = groundTileMap.cellBounds;
        string summary = "Occupied coordinates of " + this.name;
        for (int x = 0; x < bounds.xMax; x++) {
            for (int y = 0; y < bounds.yMax; y++) {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tb = groundTileMap.GetTile(pos);
                if (tb != null) {
                    summary += "\n" + pos.ToString();
                }
            }
        }
        Debug.Log(summary);
    }
    [ContextMenu("Log Bounds")]
    public void LogBounds() {
        groundTileMap.CompressBounds();
        BoundsInt bounds = groundTileMap.cellBounds;
        string boundsSummary = bounds.ToString();
        boundsSummary += "\nxMin - " + bounds.xMin.ToString();
        boundsSummary += "\nyMin - " + bounds.yMin.ToString();
        boundsSummary += "\nxMax - " + bounds.xMax.ToString();
        boundsSummary += "\nyMax - " + bounds.yMax.ToString();
        Debug.Log(boundsSummary);
    }
    #endregion

    #region Tile Maps
    public void RefreshAllTilemaps() {
        groundTileMap.RefreshAllTiles();
        wallTileMap.RefreshAllTiles();
        objectTileMap.RefreshAllTiles();
        deatilTileMap.RefreshAllTiles();
    }
    #endregion

    #region Tiles
    public void SetTilesInStructure(LocationGridTile[] tiles) {
        this.tiles = tiles;
    }
    #endregion

    #region Tile Objects
    public void RegisterPreplacedObjects() {
        for (int i = 0; i < tiles.Length; i++) {
            LocationGridTile currTile = tiles[i];
            UnityEngine.Tilemaps.TileBase objTile = currTile.parentAreaMap.objectsTilemap.GetTile(currTile.localPlace);
            //TODO: Make this better! because this does not scale well.
            if (objTile != null) {
                switch (objTile.name) {
                    case "Bed":
                        AddPOI(new Bed(this), currTile, false);
                        currTile.SetReservedType(TILE_OBJECT_TYPE.BED);
                        break;
                    case "Desk":
                        AddPOI(new Desk(this), currTile, false);
                        currTile.SetReservedType(TILE_OBJECT_TYPE.DESK);
                        break;
                    case "Table0":
                    case "Table1":
                    case "Table2":
                    case "tableDecor00":
                    case "Bartop_Left":
                    case "Bartop_Right":
                        Table table = new Table(this);
                        table.SetUsedAsset(objTile);
                        AddPOI(table, currTile, false);
                        currTile.SetReservedType(TILE_OBJECT_TYPE.TABLE);
                        break;
                    case "SupplyPile":
                        AddPOI(new SupplyPile(this), currTile, false);
                        currTile.SetReservedType(TILE_OBJECT_TYPE.SUPPLY_PILE);
                        break;
                    case "FoodPile":
                        AddPOI(new FoodPile(this), currTile, false);
                        currTile.SetReservedType(TILE_OBJECT_TYPE.FOOD_PILE);
                        break;
                    case "Guitar":
                        AddPOI(new Guitar(this), currTile, false);
                        currTile.SetReservedType(TILE_OBJECT_TYPE.GUITAR);
                        break;
                    case "WaterWell":
                        AddPOI(new WaterWell(this), currTile, false);
                        currTile.SetReservedType(TILE_OBJECT_TYPE.WATER_WELL);
                        break;
                    default:
                        break;
                }
            }
        }
    }
    #endregion

    public List<LocationGridTile> GetTilesOccupiedByStructure(AreaInnerTileMap map) {
        List<LocationGridTile> occupiedTiles = new List<LocationGridTile>();

        groundTileMap.CompressBounds();
        BoundsInt bounds = groundTileMap.cellBounds;

        List<Vector3Int> occupiedCoordinates = new List<Vector3Int>();
        for (int x = bounds.xMin; x < bounds.xMax; x++) {
            for (int y = bounds.yMin; y < bounds.yMax; y++) {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tb = groundTileMap.GetTile(pos);
                if (tb != null) {
                    occupiedCoordinates.Add(pos);
                }
            }
        }

        Vector3Int actualLocation = new Vector3Int(Mathf.FloorToInt(this.transform.localPosition.x), Mathf.FloorToInt(this.transform.localPosition.y), 0) ;
        for (int i = 0; i < occupiedCoordinates.Count; i++) {
            Vector3Int currCoordinate = occupiedCoordinates[i];

            Vector3Int gridTileLocation = actualLocation;

            //get difference from center
            int xDiffFromCenter = currCoordinate.x - center.x;
            int yDiffFromCenter = currCoordinate.y - center.y;
            gridTileLocation.x += xDiffFromCenter;
            gridTileLocation.y += yDiffFromCenter;


            LocationGridTile tile = map.map[gridTileLocation.x, gridTileLocation.y];
            occupiedTiles.Add(tile);
        }
        return occupiedTiles;
    }
}
