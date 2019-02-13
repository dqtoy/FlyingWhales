using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InteriorMapManager : MonoBehaviour {
    public int width;
    public int height;

    public Tilemap interiorMap;
    public Tile[] outsideGroundTiles;
    public Tile[] insideGroundTiles;

    private TileData[,] map; //0 = outside, 1 = inside

    private void Awake() {
        
    }
    // Use this for initialization
    void Start () {
        CreateInteriorMap();
	}
	
	
    private void CreateInteriorMap() {
        map = new TileData[width, height];
        for (int x = 0; x < map.GetUpperBound(0); x++) {
            for (int y = 0; y < map.GetUpperBound(1); y++) {
                TileData data = map[x, y];
                data.isInside = false;
                map[x, y] = data;
            }
        }
    }

    private Tile GetRandomOutsideGroundTiles() {
        return outsideGroundTiles[UnityEngine.Random.Range(0, outsideGroundTiles.Length)];
    }

    private void UpdateMap() {
        for (int x = 0; x < map.GetUpperBound(0); x++) {
            for (int y = 0; y < map.GetUpperBound(1); y++) {
                //map[x, y] = 0;
            }
        }
    }
}

public struct TileData {
    public bool isInside;
}
