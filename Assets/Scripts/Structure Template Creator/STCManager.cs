#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class STCManager : MonoBehaviour {

    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap structureWallTilemap;
    [SerializeField] private Tilemap objectsTilemap;

    [SerializeField] private List<TileBase> allTileAssets;

    private string templatePath;

    private void Awake() {
        templatePath = Application.dataPath + "/StreamingAssets/Structure Templates/";
    }

    [ContextMenu("Save Template")]
    public void SaveTemplate() {
        groundTilemap.CompressBounds();
        //get all tiles based on the bounds of the ground tile
        TileBase[] groundTiles = groundTilemap.GetTilesBlock(groundTilemap.cellBounds);
        TileBase[] wallTiles = structureWallTilemap.GetTilesBlock(groundTilemap.cellBounds);
        TileBase[] objectTiles = objectsTilemap.GetTilesBlock(groundTilemap.cellBounds);

        Debug.Log("Got " + groundTiles.Length + " tiles");

        StructureTemplate newTemplate = new StructureTemplate(groundTiles, wallTiles, objectTiles,
            new Point(groundTilemap.cellBounds.size.x, groundTilemap.cellBounds.size.y));

        string dataAsJson = JsonUtility.ToJson(newTemplate);
        
        string path = EditorUtility.SaveFilePanel("Save Structure Template", templatePath, "Structure_Template", "json");
        if (path == "")
            return;
        File.WriteAllText(path, dataAsJson);
        AssetDatabase.Refresh();
    }

    [ContextMenu("Load Template")]
    private void LoadTemplate() {
        string path = EditorUtility.OpenFilePanel("Load Structure Template", templatePath, "json");
        if (path.Length != 0) {
            string dataAsJson = File.ReadAllText(path);
            StructureTemplate loaded = JsonUtility.FromJson<StructureTemplate>(dataAsJson);
            LoadTemplate(loaded);
        }
    }

    private void LoadTemplate(StructureTemplate st) {
        ClearTiles();
        int currXCoordinate = 0;
        int currYCoordinate = 0;
        Vector3Int currPos = new Vector3Int(currXCoordinate, currYCoordinate, 0);
        for (int i = 0; i < st.groundTiles.Length; i++) {
            //ground tile map
            string groundTileName = st.groundTiles[i];
            if (string.IsNullOrEmpty(groundTileName)) {
                groundTilemap.SetTile(currPos, null);
            } else {
                groundTilemap.SetTile(currPos, GetTileAsset(groundTileName));
            }

            //wall tile map
            string wallTileName = st.structureWallTiles[i];
            if (string.IsNullOrEmpty(wallTileName)) {
                structureWallTilemap.SetTile(currPos, null);
            } else {
                structureWallTilemap.SetTile(currPos, GetTileAsset(wallTileName));
            }

            //object tile map
            string objectTileName = st.objectTiles[i];
            if (string.IsNullOrEmpty(objectTileName)) {
                objectsTilemap.SetTile(currPos, null);
            } else {
                objectsTilemap.SetTile(currPos, GetTileAsset(objectTileName));
            }

            //increment positions (goes from left to right, then from bottom to top)
            currPos.x++;
            if (currPos.x >= st.size.X) {
                currPos.x = 0;
                currPos.y++;
            }
        }
    }

    private TileBase GetTileAsset(string name) {
        for (int i = 0; i < allTileAssets.Count; i++) {
            TileBase currTile = allTileAssets[i];
            if (currTile.name == name) {
                return currTile;
            }
        }
        return null;
    }

    [ContextMenu("Clear Tiles")]
    public void ClearTiles() {
        groundTilemap.ClearAllTiles();
        structureWallTilemap.ClearAllTiles();
        objectsTilemap.ClearAllTiles();
    }

    [ContextMenu("Load Tile Assets")]
    private void LoadAllTilesAssets() {
        allTileAssets = Resources.LoadAll("Tile Map Assets", typeof(TileBase)).Cast<TileBase>().ToList();
        //foreach (var t in textures)
        //    Debug.Log(t.name);
    }
}
#endif

[System.Serializable]
public class StructureTemplate {
    public Point size;
    public string[] groundTiles;
    public string[] structureWallTiles;
    public string[] objectTiles;

    public StructureTemplate(TileBase[] ground, TileBase[] walls, TileBase[] objects, Point size) {
        this.size = size;
        groundTiles = null;
        structureWallTiles = null;
        objectTiles = null;

        groundTiles = Convert(ground);
        structureWallTiles = Convert(walls);
        objectTiles = Convert(objects);
    }

    private string[] Convert(TileBase[] source) {
        string[] converted = new string[source.Length];
        for (int i = 0; i < source.Length; i++) {
            TileBase currTile = source[i];
            if (currTile == null) {
                converted[i] = string.Empty;
            } else {
                converted[i] = currTile.name;
            }
        }
        return converted;
    }
}

