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
    [SerializeField] private Tilemap detailsTilemap;

    [SerializeField] private List<TileBase> allTileAssets;

    private string templatePath;

    private void Awake() {
        templatePath = Application.dataPath + "/StreamingAssets/Structure Templates/";
    }

    private TileTemplateData[] GetTileData(Tilemap tilemap, BoundsInt bounds) {
        TileTemplateData[] data = new TileTemplateData[bounds.size.x * bounds.size.y];
        int count = 0;

        for (int x = bounds.xMin; x < bounds.xMax; x++) {
            for (int y = bounds.yMin; y < bounds.yMax; y++) {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tb = tilemap.GetTile(pos);
                Matrix4x4 matrix = tilemap.GetTransformMatrix(pos);

                int normalizedX = x;
                int normalizedY = y;

                if (bounds.xMin != 0) {
                    if (bounds.xMin < 0) {
                        //x is negative
                        normalizedX += Mathf.Abs(bounds.xMin);
                    } else {
                        //x is positive
                        normalizedX -= Mathf.Abs(bounds.xMin);
                    }
                }

                if (bounds.yMin != 0) {
                    if (bounds.yMin < 0) {
                        //y is negative
                        normalizedY += Mathf.Abs(bounds.yMin);
                    } else {
                        //y is positive
                        normalizedY -= Mathf.Abs(bounds.yMin);
                    }
                }

                data[count] = new TileTemplateData(tb, matrix, new Vector3(normalizedX, normalizedY, 0));
                count++;
            }
        }
        return data;
    }

    [ContextMenu("Save Template")]
    public void SaveTemplate() {
        groundTilemap.CompressBounds();
        //get all tiles based on the bounds of the ground tile
        TileTemplateData[] groundTiles = GetTileData(groundTilemap, groundTilemap.cellBounds);
        TileTemplateData[] wallTiles = GetTileData(structureWallTilemap, groundTilemap.cellBounds);
        TileTemplateData[] objectTiles = GetTileData(objectsTilemap, groundTilemap.cellBounds);
        TileTemplateData[] detailTiles = GetTileData(detailsTilemap, groundTilemap.cellBounds);

        Debug.Log("Got " + groundTiles.Length + " tiles");

        StructureTemplate newTemplate = new StructureTemplate(groundTiles, wallTiles, objectTiles, detailTiles,
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
        //int currXCoordinate = 0;
        //int currYCoordinate = 0;
        //Vector3Int currPos = new Vector3Int(currXCoordinate, currYCoordinate, 0);
        DrawTiles(groundTilemap, st.groundTiles);
        DrawTiles(structureWallTilemap, st.structureWallTiles);
        DrawTiles(objectsTilemap, st.objectTiles);
        DrawTiles(detailsTilemap, st.detailTiles);
        //for (int i = 0; i < st.groundTiles.Length; i++) {
        //    //ground tile map
        //    string groundTileName = st.groundTiles[i];
        //    if (string.IsNullOrEmpty(groundTileName)) {
        //        groundTilemap.SetTile(currPos, null);
        //    } else {
        //        groundTilemap.SetTile(currPos, GetTileAsset(groundTileName));
        //    }

        //    //wall tile map
        //    string wallTileName = st.structureWallTiles[i];
        //    if (string.IsNullOrEmpty(wallTileName)) {
        //        structureWallTilemap.SetTile(currPos, null);
        //    } else {
        //        structureWallTilemap.SetTile(currPos, GetTileAsset(wallTileName));
        //    }

        //    //object tile map
        //    string objectTileName = st.objectTiles[i];
        //    if (string.IsNullOrEmpty(objectTileName)) {
        //        objectsTilemap.SetTile(currPos, null);
        //    } else {
        //        objectsTilemap.SetTile(currPos, GetTileAsset(objectTileName));
        //    }

        //    //detail tile map
        //    string detailTileName = st.detailTiles[i];
        //    if (string.IsNullOrEmpty(detailTileName)) {
        //        detailsTilemap.SetTile(currPos, null);
        //    } else {
        //        detailsTilemap.SetTile(currPos, GetTileAsset(detailTileName));
        //    }

        //    //increment positions (goes from left to right, then from bottom to top)
        //    currPos.x++;
        //    if (currPos.x >= st.size.X) {
        //        currPos.x = 0;
        //        currPos.y++;
        //    }
        //}
    }

    private void DrawTiles(Tilemap tilemap, TileTemplateData[] data) {
        if (data == null) {
            return;
        }
        for (int i = 0; i < data.Length; i++) {
            TileTemplateData currData = data[i];
            Vector3Int pos = new Vector3Int((int)currData.tilePosition.x, (int)currData.tilePosition.y, 0);
            tilemap.SetTile(pos, GetTileAsset(currData.tileAssetName));
            tilemap.SetTransformMatrix(pos, currData.matrix);
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
        detailsTilemap.ClearAllTiles();
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
    public TileTemplateData[] groundTiles;
    public TileTemplateData[] structureWallTiles;
    public TileTemplateData[] objectTiles;
    public TileTemplateData[] detailTiles;

    public StructureTemplate(TileTemplateData[] ground, TileTemplateData[] walls, TileTemplateData[] objects, TileTemplateData[] details, Point size) {
        this.size = size;
        groundTiles = ground;
        structureWallTiles = walls;
        objectTiles = objects;
        detailTiles = details;

        //groundTiles = Convert(ground);
        //structureWallTiles = Convert(walls);
        //objectTiles = Convert(objects);
        //detailTiles = Convert(details);
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

[System.Serializable]
public class TileTemplateData {

    public Vector3 tilePosition;
    public string tileAssetName;
    //public Vector3 rotation;
    public Matrix4x4 matrix;

    public TileTemplateData(TileBase tb, Matrix4x4 m, Vector3 pos) {
        if (tb == null) {
            tileAssetName = string.Empty;
        } else {
            tileAssetName = tb.name;
        }
        tilePosition = pos;
        //rotation = m.rotation.eulerAngles;
        matrix = m;
    }
}

