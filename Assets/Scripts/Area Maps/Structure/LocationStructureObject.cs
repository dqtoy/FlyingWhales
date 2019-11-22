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
