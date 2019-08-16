using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

//[ExecuteInEditMode]
public class STCManager : MonoBehaviour {

    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap structureWallTilemap;
    [SerializeField] private Tilemap objectsTilemap;
    [SerializeField] private Tilemap detailsTilemap;
    [SerializeField] private Transform connectorsParent;
    [SerializeField] private Transform furnitureParent;
    [SerializeField] private GameObject connectorPrefab;
    [SerializeField] private GameObject furnitureSpotPrefab;

    [SerializeField] private List<TileBase> allTileAssets;

    private string templatePath;

    private void Awake() {
#if UNITY_EDITOR
        templatePath = Application.dataPath + "/StreamingAssets/Structure Templates/";
#endif
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

    /// <summary>
    /// Convinience function, for making the tiles have all positive coordinates
    /// </summary>
    public void PlaceAtOrigin() {
        groundTilemap.CompressBounds();
        BoundsInt bounds = groundTilemap.cellBounds;
        int shiftXBy = 0; //shift x position of all objects by n
        int shiftYBy = 0;//shift y position of all objects by n
        if (bounds.xMin != 0) {
            shiftXBy = bounds.xMin * -1;
        }

        if (bounds.yMin != 0) {
            shiftYBy = bounds.yMin * -1;
        }

        Vector3Int shiftBy = new Vector3Int(shiftXBy, shiftYBy, 0);
        Debug.Log("Shifting map by " + shiftBy.ToString());
        Tilemap[] tilemaps = GetComponentsInChildren<Tilemap>();
        for (int i = 0; i < tilemaps.Length; i++) {
            ShiftTilemapPosition(tilemaps[i], bounds);
        }

        //shift connectors
        ConnectorMono[] connectors = connectorsParent.GetComponentsInChildren<ConnectorMono>();
        for (int i = 0; i < connectors.Length; i++) {
            Vector3 currPos = connectors[i].transform.localPosition;
            //Vector3 actualPos = new Vector3(currPos.x - 0.5f, currPos.y - 0.5f, 0f);
            Vector3 newPos = new Vector3(currPos.x + shiftBy.x, currPos.y + shiftBy.y, 0f);
            connectors[i].transform.localPosition = newPos;
        }

        //shift Furniture spots
        FurnitureSpotMono[] furnitureSpots = connectorsParent.GetComponentsInChildren<FurnitureSpotMono>();
        for (int i = 0; i < furnitureSpots.Length; i++) {
            Vector3 currPos = furnitureSpots[i].transform.localPosition;
            //Vector3 actualPos = new Vector3(currPos.x - 0.5f, currPos.y - 0.5f, 0f);
            Vector3 newPos = new Vector3(currPos.x + shiftBy.x, currPos.y + shiftBy.y, 0f);
            furnitureSpots[i].transform.localPosition = newPos;
        }
    }

    private void ShiftTilemapPosition(Tilemap map, BoundsInt bounds) {
        TileTemplateData[] allData = GetTileData(map, bounds);

        map.ClearAllTiles();

        for (int i = 0; i < allData.Length; i++) {
            TileTemplateData currData = allData[i];
            Vector3Int newCoords = new Vector3Int((int)(currData.tilePosition.x), (int)(currData.tilePosition.y), 0);
            map.SetTile(newCoords, GetTileAsset(currData.tileAssetName));
            map.SetTransformMatrix(newCoords, currData.matrix);
        }
    }

    [ContextMenu("Save Template")]
    public void SaveTemplate() {
        groundTilemap.CompressBounds();
        PlaceAtOrigin();
        //get all tiles based on the bounds of the ground tile
        TileTemplateData[] groundTiles = GetTileData(groundTilemap, groundTilemap.cellBounds);
        TileTemplateData[] wallTiles = GetTileData(structureWallTilemap, groundTilemap.cellBounds);
        TileTemplateData[] objectTiles = GetTileData(objectsTilemap, groundTilemap.cellBounds);
        TileTemplateData[] detailTiles = GetTileData(detailsTilemap, groundTilemap.cellBounds);

        ConnectorMono[] connectorMonos = Utilities.GetComponentsInDirectChildren<ConnectorMono>(connectorsParent.gameObject);
        FurnitureSpotMono[] furnitureSpotMonos = Utilities.GetComponentsInDirectChildren<FurnitureSpotMono>(furnitureParent.gameObject);


        StructureConnector[] connectors = new StructureConnector[connectorMonos.Length];
        for (int i = 0; i < connectorMonos.Length; i++) {
            ConnectorMono currMono = connectorMonos[i];
            connectors[i] = currMono.GetConnector();
        }

        FurnitureSpot[] furnitureSpots = new FurnitureSpot[furnitureSpotMonos.Length];
        for (int i = 0; i < furnitureSpotMonos.Length; i++) {
            FurnitureSpotMono currMono = furnitureSpotMonos[i];
            furnitureSpots[i] = currMono.GetFurnitureSpot();
        }


        Debug.Log("Got " + groundTiles.Length + " tiles");

        StructureTemplate newTemplate = new StructureTemplate("Structure_Template", groundTiles, wallTiles, objectTiles, detailTiles,
            new Point(groundTilemap.cellBounds.size.x, groundTilemap.cellBounds.size.y), connectors, furnitureSpots);

        string dataAsJson = JsonUtility.ToJson(newTemplate);

#if UNITY_EDITOR

        string path = EditorUtility.SaveFilePanel("Save Structure Template", templatePath, "Structure_Template", "json");
        if (path == "")
            return;
        File.WriteAllText(path, dataAsJson);
        AssetDatabase.Refresh();
#endif
    }

    [ContextMenu("Load Template")]
    public void LoadTemplate() {
#if UNITY_EDITOR
        string path = EditorUtility.OpenFilePanel("Load Structure Template", templatePath, "json");
        if (path.Length != 0) {
            string dataAsJson = File.ReadAllText(path);
            StructureTemplate loaded = JsonUtility.FromJson<StructureTemplate>(dataAsJson);
            LoadTemplate(loaded);
            Debug.Log("Loaded " + path);
        }
#endif
    }

    private void LoadTemplate(StructureTemplate st) {
        ClearTiles();
        DrawTiles(groundTilemap, st.groundTiles);
        DrawTiles(structureWallTilemap, st.structureWallTiles);
        DrawTiles(objectsTilemap, st.objectTiles);
        DrawTiles(detailsTilemap, st.detailTiles);

        //connectors
        Utilities.DestroyChildren(connectorsParent);
        if (st.connectors != null) {
            for (int i = 0; i < st.connectors.Length; i++) {
                StructureConnector connector = st.connectors[i];
                GameObject newConnector = GameObject.Instantiate(connectorPrefab, connectorsParent);
                newConnector.transform.localPosition = new Vector3(connector.location.x, connector.location.y, 0);
                ConnectorMono cm = newConnector.GetComponent<ConnectorMono>();
                cm.connectionDirection = connector.neededDirection;
                cm.allowedStructureType = connector.allowedStructureType;
            }
        }

        //furniture spots
        Utilities.DestroyChildren(furnitureParent);
        if (st.furnitureSpots != null) {
            for (int i = 0; i < st.furnitureSpots.Length; i++) {
                FurnitureSpot spot = st.furnitureSpots[i];
                GameObject newSpot = GameObject.Instantiate(furnitureSpotPrefab, furnitureParent);
                newSpot.transform.localPosition = new Vector3(spot.location.x + 0.5f, spot.location.y + 0.5f, 0);
                FurnitureSpotMono cm = newSpot.GetComponent<FurnitureSpotMono>();
                cm.allowedFurnitureTypes = spot.allowedFurnitureTypes;
                cm.furnitureSettings = spot.furnitureSettings;
            }
        }
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

        Utilities.DestroyChildren(connectorsParent);
    }

    [ContextMenu("Load Tile Assets")]
    public void LoadAllTilesAssets() {
        allTileAssets = Resources.LoadAll("Tile Map Assets", typeof(TileBase)).Cast<TileBase>().ToList();
        //foreach (var t in textures)
        //    Debug.Log(t.name);
    }

    #region Connectors
    public void CreateNewConnector() {
        GameObject newConnector = GameObject.Instantiate(connectorPrefab, connectorsParent);
        newConnector.transform.localPosition = Vector3.zero;
        //ConnectorMono cm = newConnector.GetComponent<ConnectorMono>();
        //cm.connectionDirection = connector.neededDirection;
        //cm.connectionType = connector.allowedStructureType;
    }
    #endregion

    #region Furniture Spots
    public FurnitureSpotMono CreateNewFurnitureSpot() {
        GameObject newConnector = GameObject.Instantiate(furnitureSpotPrefab, furnitureParent);
        newConnector.transform.localPosition = Vector3.zero;
        return newConnector.GetComponent<FurnitureSpotMono>();
        //ConnectorMono cm = newConnector.GetComponent<ConnectorMono>();
        //cm.connectionDirection = connector.neededDirection;
        //cm.connectionType = connector.allowedStructureType;
    }
    public FurnitureSpotMono CreateNewFurnitureSpot(Vector3 position) {
        GameObject newConnector = GameObject.Instantiate(furnitureSpotPrefab, furnitureParent);
        newConnector.transform.localPosition = position;
        return newConnector.GetComponent<FurnitureSpotMono>();
        //ConnectorMono cm = newConnector.GetComponent<ConnectorMono>();
        //cm.connectionDirection = connector.neededDirection;
        //cm.connectionType = connector.allowedStructureType;
    }
    public void CreateFurnitureSpotsFromPlacedObjects() {
        groundTilemap.CompressBounds();
        PlaceAtOrigin();
        TileTemplateData[] data = GetTileData(objectsTilemap, groundTilemap.cellBounds);
        for (int i = 0; i < data.Length; i++) {
            TileTemplateData currData = data[i];
            if (!string.IsNullOrEmpty(currData.tileAssetName)) {
                List<FURNITURE_TYPE> allowedFurnitureTypes = new List<FURNITURE_TYPE>();
                if (currData.tileAssetName.Contains("Bed")) {
                    allowedFurnitureTypes.Add(FURNITURE_TYPE.BED);
                } else if (currData.tileAssetName.Contains("Table")) {
                    allowedFurnitureTypes.Add(FURNITURE_TYPE.TABLE);
                } else if (currData.tileAssetName.Contains("Desk")) {
                    allowedFurnitureTypes.Add(FURNITURE_TYPE.DESK);
                } else if (currData.tileAssetName.Contains("Guitar")) {
                    allowedFurnitureTypes.Add(FURNITURE_TYPE.GUITAR);
                }
                FurnitureSpotMono mono = CreateNewFurnitureSpot(currData.centeredTilePosition);
                mono.allowedFurnitureTypes = allowedFurnitureTypes.ToArray();
            }
        }

    }
    #endregion


    #region For Testing
    private Dictionary<STRUCTURE_TYPE, int> testingStructures = new Dictionary<STRUCTURE_TYPE, int>() {
        { STRUCTURE_TYPE.DWELLING, 3 },
    };
    private List<StructureTemplate> GetValidTownCenterTemplates() {
        List<StructureTemplate> valid = new List<StructureTemplate>();
        List<StructureTemplate> choices = GetStructureTemplates("TOWN CENTER");
        for (int i = 0; i < choices.Count; i++) {
            StructureTemplate currTemplate = choices[i];
            if (currTemplate.HasConnectorsForStructure(testingStructures)) {
                valid.Add(currTemplate);
            }
        }

        return valid;
    }
    private List<StructureTemplate> GetTemplatesThatCanConnectTo(StructureTemplate otherTemplate, List<StructureTemplate> choices) {
        List<StructureTemplate> valid = new List<StructureTemplate>();
        for (int i = 0; i < choices.Count; i++) {
            StructureTemplate currTemp = choices[i];
            if (currTemp.CanConnectTo(otherTemplate)) {
                valid.Add(currTemp);
            }
        }

        return valid;
    }
    public List<StructureTemplate> GetStructureTemplates(string folderName) {
        List<StructureTemplate> templates = new List<StructureTemplate>();
        string path = templatePath + folderName + "/";
        if (Directory.Exists(path)) {
            DirectoryInfo info = new DirectoryInfo(path);
            FileInfo[] files = info.GetFiles();
            for (int i = 0; i < files.Length; i++) {
                FileInfo currInfo = files[i];
                if (currInfo.Extension.Equals(".json")) {
                    string dataAsJson = File.ReadAllText(currInfo.FullName);
                    StructureTemplate loaded = JsonUtility.FromJson<StructureTemplate>(dataAsJson);
                    templates.Add(loaded);
                }
            }
        }
        return templates;
    }
    private void PlaceConnectors(StructureTemplate template) {
        for (int i = 0; i < template.connectors.Length; i++) {
            StructureConnector connector = template.connectors[i];
            GameObject newConnector = GameObject.Instantiate(connectorPrefab, connectorsParent);
            newConnector.transform.localPosition = new Vector3(connector.location.x + 0.5f, connector.location.y + 0.5f, 0);
            ConnectorMono cm = newConnector.GetComponent<ConnectorMono>();
            cm.connectionDirection = connector.neededDirection;
            cm.allowedStructureType = connector.allowedStructureType;
        }
    }
    #endregion
}


[System.Serializable]
public struct StructureTemplate {
    public string name;
    public Point size;
    public TileTemplateData[] groundTiles;
    public TileTemplateData[] structureWallTiles;
    public TileTemplateData[] objectTiles;
    public TileTemplateData[] detailTiles;
    public StructureConnector[] connectors;
    public FurnitureSpot[] furnitureSpots;

    public StructureTemplate(string _name, TileTemplateData[] _ground, TileTemplateData[] _walls,
        TileTemplateData[] _objects, TileTemplateData[] _details, Point _size, StructureConnector[] _connectors, FurnitureSpot[] _furnitureSpots) {
        name = _name;
        size = _size;
        groundTiles = _ground;
        structureWallTiles = _walls;
        objectTiles = _objects;
        detailTiles = _details;
        connectors = _connectors;
        furnitureSpots = _furnitureSpots;
    }


    #region Utilities
    public bool HasConnectorsForStructure(Dictionary<STRUCTURE_TYPE, List<LocationStructure>> structures) {
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in structures) {
            if (keyValuePair.Key.IsOpenSpace() && keyValuePair.Key != STRUCTURE_TYPE.CEMETERY) {
                continue; //skip
            }
            if (GetCountOfConnectorsForType(keyValuePair.Key) < keyValuePair.Value.Count) {
                //this template has less than the number of needed connections for the current type
                return false;
            }
        }
        return true;
    }
    /// <summary>
    /// Does this template have all the needed connectors for all the given structures?
    /// </summary>
    /// <param name="structures">The structures to connect.</param>
    /// <returns>True or False</returns>
    public bool HasConnectorsForStructure(Dictionary<STRUCTURE_TYPE, int> structures) {
        foreach (KeyValuePair<STRUCTURE_TYPE, int> keyValuePair in structures) {
            if (keyValuePair.Key.IsOpenSpace() && keyValuePair.Key != STRUCTURE_TYPE.CEMETERY) {
                continue; //skip
            }
            if (GetCountOfConnectorsForType(keyValuePair.Key) < keyValuePair.Value) {
                //this template has less than the number of needed connections for the current type
                return false;
            }
        }
        return true;
    }
    private int GetCountOfConnectorsForType(STRUCTURE_TYPE type) {
        int count = 0;
        for (int i = 0; i < connectors.Length; i++) {
            if (connectors[i].allowedStructureType == type) {
                count++;
            }
        }
        return count;
    }
    public bool HasAvailableConnectionFor(STRUCTURE_TYPE type, Cardinal_Direction direction) {
        for (int i = 0; i < connectors.Length; i++) {
            StructureConnector currConnector = connectors[i];
            if (currConnector.neededDirection == direction && currConnector.allowedStructureType == type && currConnector.isOpen) {
                return true;
            }
        }
        return false;
    }
    public bool HasAvailableConnectionFor(STRUCTURE_TYPE type, Cardinal_Direction direction, out StructureConnector availableConnector) {
        for (int i = 0; i < connectors.Length; i++) {
            StructureConnector currConnector = connectors[i];
            if (currConnector.neededDirection == direction && currConnector.allowedStructureType == type && currConnector.isOpen) {
                availableConnector = currConnector;
                return true;
            }
        }
        availableConnector = default(StructureConnector);
        return false;
    }

    /// <summary>
    /// Checks if this template has a connector that is compatible with another template.
    /// NOTE: Used opposite direction for checking connection because 
    /// connections should be only allowed if (East connects to West, North connects to South and vise versa)
    /// </summary>
    /// <param name="otherTemplate">The template that this template wants to check</param>
    /// <returns></returns>
    public bool CanConnectTo(StructureTemplate otherTemplate) {
        if (connectors != null) {
            for (int i = 0; i < connectors.Length; i++) {
                StructureConnector connector = connectors[i];
                if (otherTemplate.HasAvailableConnectionFor(connector.allowedStructureType, connector.neededDirection.OppositeDirection())) {
                    return true;
                }
            }
        }
        return false;
    }
    /// <summary>
    /// Get a connector in this template that can connect to the other template.
    /// </summary>
    /// <param name="otherTemplate">The template to connect to</param>
    /// <param name="connectTo">The valid connector that was found in the other template</param>
    /// <returns></returns>
    public StructureConnector GetValidConnectorTo(StructureTemplate otherTemplate, out StructureConnector connectTo) {
        for (int i = 0; i < connectors.Length; i++) {
            StructureConnector connector = connectors[i];
            if (otherTemplate.HasAvailableConnectionFor(connector.allowedStructureType, connector.neededDirection.OppositeDirection(), out connectTo)) {
                return connector;
            }
        }
        throw new System.Exception("Could not find valid connector!");
    }
    public void UpdatePositionsGivenOrigin(Vector3Int origin) {
        UpdatePositionsGivenOrigin(groundTiles, origin);
        UpdatePositionsGivenOrigin(structureWallTiles, origin);
        UpdatePositionsGivenOrigin(objectTiles, origin);
        UpdatePositionsGivenOrigin(detailTiles, origin);
        UpdatePositionsGivenOrigin(connectors, origin);
    }
    private void UpdatePositionsGivenOrigin(TileTemplateData[] data, Vector3Int origin) {
        for (int i = 0; i < data.Length; i++) {
            TileTemplateData currData = data[i];
            currData.tilePosition = new Vector3(currData.tilePosition.x + origin.x, currData.tilePosition.y + origin.y);
            data[i] = currData;
        }
    }
    private void UpdatePositionsGivenOrigin(StructureConnector[] data, Vector3Int origin) {
        for (int i = 0; i < data.Length; i++) {
            StructureConnector currData = data[i];
            currData.location = new Vector3Int(currData.location.x + origin.x, currData.location.y + origin.y, 0);
            data[i] = currData;
        }
    }
    #endregion
}

[System.Serializable]
public struct TileTemplateData {

    public Vector3 tilePosition;
    public string tileAssetName;
    public Matrix4x4 matrix;

    public Vector3 centeredTilePosition {
        get { return new Vector3(tilePosition.x + 0.5f, tilePosition.y + 0.5f, tilePosition.z); }
    }
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

    public static TileTemplateData Empty {
        get {
            return new TileTemplateData() {
                tilePosition = Vector3.zero,
                tileAssetName = string.Empty,
                matrix = Matrix4x4.zero
            };
        }
    }

}

