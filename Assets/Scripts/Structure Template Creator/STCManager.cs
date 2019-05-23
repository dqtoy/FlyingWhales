using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

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

        ConnectorMono[] connectors = Utilities.GetComponentsInDirectChildren<ConnectorMono>(connectorsParent.gameObject);
        FurnitureSpotMono[] furnitureSpots = Utilities.GetComponentsInDirectChildren<FurnitureSpotMono>(furnitureParent.gameObject);

        Debug.Log("Got " + groundTiles.Length + " tiles");

        StructureTemplate newTemplate = new StructureTemplate(groundTiles, wallTiles, objectTiles, detailTiles,
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
                newConnector.transform.localPosition = new Vector3(connector.location.x + 0.5f, connector.location.y + 0.5f, 0);
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
    public void GenerateTestTown() {
        ClearTiles();
        List<StructureTemplate> validTownCenters = GetValidTownCenterTemplates();
        if (validTownCenters.Count == 0) {
            throw new System.Exception("There are no valid town center structures");
        }
        StructureTemplate chosenTownCenter = validTownCenters[Random.Range(0, validTownCenters.Count)];
        DrawTiles(groundTilemap, chosenTownCenter.groundTiles);

        //connectors
        Utilities.DestroyChildren(connectorsParent);
        PlaceConnectors(chosenTownCenter);

        foreach (KeyValuePair<STRUCTURE_TYPE, int> keyValuePair in testingStructures) {
            if (keyValuePair.Key.IsOpenSpace()) {
                continue; //skip
            }

            for (int i = 0; i < keyValuePair.Value; i++) {
                List<StructureTemplate> templates = GetStructureTemplates(keyValuePair.Key.ToString()); //placed this inside loop so that instance of template is unique per iteration
                List<StructureTemplate> choices = GetTemplatesThatCanConnectTo(chosenTownCenter, templates);
                if (choices.Count == 0) {
                    throw new System.Exception("There are no valid " + keyValuePair.Key.ToString() + " templates to connect to town center");
                }
                StructureTemplate chosenTemplate = choices[Random.Range(0, choices.Count)];
                StructureConnector townCenterConnector;
                StructureConnector chosenTemplateConnector = chosenTemplate.GetValidConnectorTo(chosenTownCenter, out townCenterConnector);

            }
        }
    }
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
public class StructureTemplate {
    public string name;
    public Point size;
    public TileTemplateData[] groundTiles;
    public TileTemplateData[] structureWallTiles;
    public TileTemplateData[] objectTiles;
    public TileTemplateData[] detailTiles;
    public StructureConnector[] connectors;
    public FurnitureSpot[] furnitureSpots;

    public StructureTemplate(TileTemplateData[] ground, TileTemplateData[] walls,
        TileTemplateData[] objects, TileTemplateData[] details, Point size, ConnectorMono[] connectorMonos, FurnitureSpotMono[] furnitureMonos) {
        this.size = size;
        groundTiles = ground;
        structureWallTiles = walls;
        objectTiles = objects;
        detailTiles = details;
        connectors = ConvertToStructureConnectors(connectorMonos);
        furnitureSpots = ConvertToFurnitureSpots(furnitureMonos);

        //groundTiles = Convert(ground);
        //structureWallTiles = Convert(walls);
        //objectTiles = Convert(objects);
        //detailTiles = Convert(details);
    }

    private StructureConnector[] ConvertToStructureConnectors(ConnectorMono[] connectorMonos) {
        StructureConnector[] sc = new StructureConnector[connectorMonos.Length];
        for (int i = 0; i < sc.Length; i++) {
            ConnectorMono cm = connectorMonos[i];
            sc[i] = new StructureConnector() {
                allowedStructureType = cm.allowedStructureType,
                neededDirection = cm.connectionDirection,
                location = new Vector3Int((int)(cm.transform.localPosition.x - 0.5f), (int)(cm.transform.localPosition.y - 0.5f), 0),
                isOpen = true
            };
        }
        return sc;
    }
    private FurnitureSpot[] ConvertToFurnitureSpots(FurnitureSpotMono[] monos) {
        FurnitureSpot[] sc = new FurnitureSpot[monos.Length];
        for (int i = 0; i < sc.Length; i++) {
            FurnitureSpotMono fs = monos[i];
            sc[i] = new FurnitureSpot() {
                allowedFurnitureTypes = fs.allowedFurnitureTypes,
                location = new Vector3Int((int)(fs.transform.localPosition.x - 0.5f), (int)(fs.transform.localPosition.y - 0.5f), 0)
            };
        }
        return sc;
    }

    #region Utilities
    public bool HasConnectorsForStructure(Dictionary<STRUCTURE_TYPE, List<LocationStructure>> structures) {
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in structures) {
            if (keyValuePair.Key.IsOpenSpace()) {
                continue; //skip
            }
            if (GetCountOfConnectorsForType(keyValuePair.Key) < keyValuePair.Value.Count) {
                //this template has less than the number of needed connections for the current type
                return false;
            }
        }
        return true;
    }
    public bool HasConnectorsForStructure(Dictionary<STRUCTURE_TYPE, int> structures) {
        foreach (KeyValuePair<STRUCTURE_TYPE, int> keyValuePair in structures) {
            if (keyValuePair.Key.IsOpenSpace()) {
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
        availableConnector = null;
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
        }
    }
    private void UpdatePositionsGivenOrigin(StructureConnector[] data, Vector3Int origin) {
        for (int i = 0; i < data.Length; i++) {
            StructureConnector currData = data[i];
            currData.location = new Vector3Int(currData.location.x + origin.x, currData.location.y + origin.y, 0);
        }
    }
    #endregion
}

[System.Serializable]
public class TileTemplateData {

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

}

