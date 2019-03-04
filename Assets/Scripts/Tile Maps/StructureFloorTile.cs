using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class StructureFloorTile : Tile {

    public Sprite[] evenSprite;
    public Sprite[] oddSprite;
    // This refreshes itself and other RoadTiles that are orthogonally and diagonally adjacent
    public override void RefreshTile(Vector3Int location, ITilemap tilemap) {
        //for (int yd = -1; yd <= 1; yd++)
        //    for (int xd = -1; xd <= 1; xd++) {
        //        Vector3Int position = new Vector3Int(location.x + xd, location.y + yd, location.z);
        //        tilemap.RefreshTile(position);
        //    }
    }
    // This determines which sprite is used based on the RoadTiles that are adjacent to it and rotates it to fit the other tiles.
    // As the rotation is determined by the RoadTile, the TileFlags.OverrideTransform is set for the tile.
    public override void GetTileData(Vector3Int location, ITilemap tilemap, ref TileData tileData) {
        if (location.y % 2 == 0) {
            if (location.x % 2 == 0) {
                tileData.sprite = evenSprite[Random.Range(0, evenSprite.Length)];
            } else {
                tileData.sprite = oddSprite[Random.Range(0, oddSprite.Length)];
            }
        } else {
            if (location.x % 2 == 0) {
                tileData.sprite = oddSprite[Random.Range(0, oddSprite.Length)];
            } else {
                tileData.sprite = evenSprite[Random.Range(0, evenSprite.Length)];
            }
        }
        
        tileData.color = Color.white;
        var m = tileData.transform;
        m.SetTRS(Vector3.zero, Quaternion.identity, Vector3.one);
        tileData.transform = m;
        tileData.flags = TileFlags.LockTransform;
        tileData.colliderType = ColliderType.None;
    }
#if UNITY_EDITOR
    // The following is a helper that adds a menu item to create a RoadTile Asset
    [MenuItem("Assets/Create/StructureFloor")]
    public static void CreateStructureFloorTile() {
        string path = EditorUtility.SaveFilePanelInProject("Save Floor Tile", "New Floor Tile", "Asset", "Save Floor Tile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<StructureFloorTile>(), path);
    }
#endif
}
