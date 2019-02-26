﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class AreaInnerTileMap : MonoBehaviour {

    public int width;
    public int height;
    private const float cellSize = 64f;

    private LocationGridTile gate;
    private Cardinal_Direction outsideDirection;

    [SerializeField] private Grid grid;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Canvas worldUICanvas;
    [SerializeField] private Transform tilemapsParent;

    [Header("Tile Maps")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap strcutureTilemap;
    [SerializeField] private Tilemap objectsTilemap;

    [Header("Tiles")]
    [SerializeField] private TileBase outsideTile;
    [SerializeField] private TileBase insideTile;
    [SerializeField] private TileBase wallTile;
    [SerializeField] private TileBase structureTile;
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase characterTile;
    [SerializeField] private TileBase supplyIconTile;
    [SerializeField] private TileBase corpseIconTile;
    [SerializeField] private ItemTileBaseDictionary itemTiles;
    [SerializeField] private FoodTileBaseDictionary foodTiles;

    [Header("Characters")]
    [SerializeField] private RectTransform charactersParent;

    [Header("Events")]
    [SerializeField] private GameObject eventPopupPrefab;
    [SerializeField] private RectTransform eventPopupParent;

    [Header("Other")]
    [SerializeField] private RectTransform scrollviewContent;
    [SerializeField] private GameObject travelLinePrefab;
    [SerializeField] private Transform travelLineParent;

    public int x;
    public int y;
    public int radius;

    public Area area { get; private set; }
    public LocationGridTile[,] map { get; private set; }
    public List<LocationGridTile> allTiles { get; private set; }
    public List<LocationGridTile> outsideTiles { get; private set; }
    public List<LocationGridTile> insideTiles { get; private set; }

    public Tilemap charactersTM {
        get { return objectsTilemap; }
    }

    //private LocationGridTile exitTile;
    private bool isHovering;

    private enum Cardinal_Direction { North, South, East, West };
    private Dictionary<STRUCTURE_TYPE, List<Point>> structureSettings = new Dictionary<STRUCTURE_TYPE, List<Point>>() {
         { STRUCTURE_TYPE.DWELLING,
            new List<Point>(){
                new Point(4, 3),
                new Point(3, 4),
                new Point(3, 3),
                //new Point(4, 4),
            }
        },
          { STRUCTURE_TYPE.EXPLORE_AREA,
            new List<Point>(){
                new Point(4, 3),
                new Point(3, 4),
                //new Point(2, 2),
                new Point(3, 3),
                new Point(4, 4),
            }
        },
        { STRUCTURE_TYPE.WAREHOUSE,
            new List<Point>(){
                new Point(4, 8),
                new Point(8, 4),
            }
        },
        { STRUCTURE_TYPE.INN,
            new List<Point>(){
                new Point(4, 6),
                new Point(6, 4),
            }
        },
        { STRUCTURE_TYPE.DUNGEON,
            new List<Point>(){
                new Point(5, 8),
                new Point(4, 9),
                new Point(8, 5),
                new Point(9, 4),
            }
        },
        { STRUCTURE_TYPE.EXIT,
            new List<Point>(){
                new Point(3, 3),
            }
        },
    };

    #region Map Generation
    public void Initialize(Area area) {
        this.area = area;
        this.name = area.name + "'s Inner Map";
        canvas.worldCamera = AreaMapCameraMove.Instance.areaMapsCamera;
        worldUICanvas.worldCamera = AreaMapCameraMove.Instance.areaMapsCamera;
        GenerateInnerStructures();
    }
    public void GenerateInnerStructures() {
        groundTilemap.ClearAllTiles();
        objectsTilemap.ClearAllTiles();
        strcutureTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        scrollviewContent.sizeDelta = new Vector2(cellSize * width, cellSize * height);
        eventPopupParent.sizeDelta = new Vector2(cellSize * width, cellSize * height);
        scrollviewContent.anchoredPosition = Vector2.zero;
        eventPopupParent.anchoredPosition = Vector2.zero;
        GenerateGrid();
        SplitMap();
        ConstructWalls();
        PlaceStructures(area.GetStructures(true, true), insideTiles);
        PlaceStructures(area.GetStructures(false, true), outsideTiles);
        AssignOuterAreas();
        //DetermineExitTile();
    }
    public void GenerateInnerStructures(Dictionary<STRUCTURE_TYPE, List<LocationStructure>> inside, Dictionary<STRUCTURE_TYPE, List<LocationStructure>> outside) {
        GenerateGrid();
        SplitMap();
        ConstructWalls();
        PlaceStructures(inside, insideTiles);
        PlaceStructures(outside, outsideTiles);
        AssignOuterAreas();
    }
    private void GenerateGrid() {
        map = new LocationGridTile[width, height];
        allTiles = new List<LocationGridTile>();
        //groundTileData = new Sprite[width, height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                groundTilemap.SetTile(new Vector3Int(x, y, 0), insideTile);
                //GameObject tileGO = GameObject.Instantiate(tilePrefab, tilePrefabParent);
                //tileGO.transform.position = new Vector3(x, y, 0);
                //TileBase tbase = groundTilemap.GetTile(new Vector3Int(x, y, 0));
                LocationGridTile tile = new LocationGridTile(x, y, groundTilemap);
                allTiles.Add(tile);
                map[x, y] = tile;
            }
        }
        allTiles.ForEach(x => x.FindNeighbours(map));
    }
    private void SplitMap() {
        //float elevationFrequency = 19.1f; //14.93f;//2.66f;

        //for (int i = 0; i < allTiles.Count; i++) {
        //    LocationGridTile tile = allTiles[i];
        //    float nx = ((float)tile.localPlace.x/(float)width);
        //    float ny = ((float)tile.localPlace.y/(float)height);
        //    float elevationRand = UnityEngine.Random.Range(500f, 2000f);
        //    float elevationNoise = Mathf.PerlinNoise((nx + elevationRand) * elevationFrequency, (ny + elevationRand) * elevationFrequency);
        //    Debug.Log(elevationNoise);
        //    if (elevationNoise < 0.50f) {
        //        groundTilemap.SetTile(tile.localPlace, outsideTile);
        //    } else {
        //        groundTilemap.SetTile(tile.localPlace, insideTile);
        //    }

        //}
        //return;
        outsideTiles = new List<LocationGridTile>();
        insideTiles = new List<LocationGridTile>();
        //Cardinal_Direction[] choices = Utilities.GetEnumValues<Cardinal_Direction>();
        Cardinal_Direction outsideDirection = Cardinal_Direction.East;
        IntRange xOutRange = new IntRange();
        IntRange yOutRange = new IntRange();

        this.outsideDirection = outsideDirection;

        int edgeRange = 12;
        float outerMapPercentage = 0.30f;
        switch (outsideDirection) {
            case Cardinal_Direction.North:
                edgeRange = (int)(height * outerMapPercentage);
                xOutRange = new IntRange(0, width);
                yOutRange = new IntRange(height - edgeRange, height);
                break;
            case Cardinal_Direction.South:
                edgeRange = (int)(height * outerMapPercentage);
                xOutRange = new IntRange(0, width);
                yOutRange = new IntRange(0, edgeRange);
                break;
            case Cardinal_Direction.East:
                edgeRange = (int)(width * outerMapPercentage);
                xOutRange = new IntRange(width - edgeRange, width);
                yOutRange = new IntRange(0, height);
                break;
            case Cardinal_Direction.West:
                edgeRange = (int)(width * outerMapPercentage);
                xOutRange = new IntRange(0, edgeRange);
                yOutRange = new IntRange(0, height);
                break;
            default:
                break;
        }

        for (int i = 0; i < allTiles.Count; i++) {
            LocationGridTile currTile = allTiles[i];
            if (xOutRange.IsInRange(currTile.localPlace.x) && yOutRange.IsInRange(currTile.localPlace.y)) {
                //outside
                currTile.SetIsInside(false);
                groundTilemap.SetTile(new Vector3Int(currTile.localPlace.x, currTile.localPlace.y, 0), outsideTile);
                outsideTiles.Add(currTile);
            } else {
                //inside
                currTile.SetIsInside(true);
                groundTilemap.SetTile(new Vector3Int(currTile.localPlace.x, currTile.localPlace.y, 0), insideTile);
                insideTiles.Add(currTile);
            }
        }
    }
    private void ConstructWalls() {
        List<LocationGridTile> outerTiles = new List<LocationGridTile>();
        for (int i = 0; i < insideTiles.Count; i++) {
            LocationGridTile currTile = insideTiles[i];
            if (currTile.HasOutsideNeighbour()) {
                outerTiles.Add(currTile);
            }
        }

        //randomly choose a gate from the outer tiles
        List<LocationGridTile> gateChoices = outerTiles.Where(
            x => Utilities.IsInRange(x.localPlace.x - 3, 0, width)
            && Utilities.IsInRange(x.localPlace.x + 3, 0, width)
            && Utilities.IsInRange(x.localPlace.y - 3, 0, height)
            && Utilities.IsInRange(x.localPlace.y + 3, 0, height)
            ).ToList();
        LocationGridTile chosenGate = gateChoices[Random.Range(0, gateChoices.Count)];
        outerTiles.Remove(chosenGate);
        chosenGate.SetTileType(LocationGridTile.Tile_Type.Gate);
        insideTiles.Remove(chosenGate); //NOTE: I remove the tiles that become gates from inside tiles, so as not to include them when determining tiles with structures
        gate = chosenGate;

        for (int i = 0; i < outerTiles.Count; i++) {
            LocationGridTile currTile = outerTiles[i];
            currTile.SetTileType(LocationGridTile.Tile_Type.Wall);
            wallTilemap.SetTile(new Vector3Int(currTile.localPlace.x, currTile.localPlace.y, 0), wallTile);
            insideTiles.Remove(currTile); //NOTE: I remove the tiles that become walls from inside tiles, so as not to include them when determining tiles with structures
        }
    }
    private void PlaceStructures(Dictionary<STRUCTURE_TYPE, List<LocationStructure>> structures, List<LocationGridTile> sourceTiles) {
        Dictionary<LocationStructure, Point> structuresToCreate = new Dictionary<LocationStructure, Point>();
        int neededTiles = 0;
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> kvp in structures) {
            if (!structureSettings.ContainsKey(kvp.Key)) {
                continue; //skip
            }
            //structuresToCreate.Add(kvp.Key, new List<Point>());
            for (int i = 0; i < kvp.Value.Count; i++) {
                Point point = structureSettings[kvp.Key][Random.Range(0, structureSettings[kvp.Key].Count)];
                structuresToCreate.Add(kvp.Value[i], point);
                neededTiles += point.Product();
            }
        }
        structuresToCreate = structuresToCreate.OrderBy(x => x.Key.structureType).ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

        Debug.Log("We need at least " + neededTiles.ToString() + " tiles to meet the required structures. Current tiles are: " + sourceTiles.Count);
        List<LocationGridTile> elligibleTiles = new List<LocationGridTile>(sourceTiles);
        int leftMostCoordinate = sourceTiles.Min(t => t.localPlace.x);
        int rightMostCoordinate = sourceTiles.Max(t => t.localPlace.x);
        int topMostCoordinate = sourceTiles.Max(t => t.localPlace.y);
        int botMostCoordinate = sourceTiles.Min(t => t.localPlace.y);

        foreach (KeyValuePair<LocationStructure, Point> kvp in structuresToCreate) {
            Point currPoint = kvp.Value;
            List<LocationGridTile> choices;
            if (kvp.Key.structureType == STRUCTURE_TYPE.EXIT) {
                string summary = "Generating exit structure for " + area.name;
                summary += "\nLeft most coordinate is " + leftMostCoordinate;
                summary += "\nRight most coordinate is " + rightMostCoordinate;
                summary += "\nTop most coordinate is " + topMostCoordinate;
                summary += "\nBot most coordinate is " + botMostCoordinate;

                Cardinal_Direction chosenEdge = outsideDirection;
                summary += "\nChosen edge is " + chosenEdge.ToString();
                switch (chosenEdge) {
                    case Cardinal_Direction.North:
                        choices = elligibleTiles.Where(
                        t => (t.localPlace.x == gate.localPlace.x && t.localPlace.y + currPoint.Y == topMostCoordinate + 1)
                        && Utilities.ContainsRange(elligibleTiles, GetTiles(currPoint, t))).ToList();
                        break;
                    case Cardinal_Direction.South:
                        choices = elligibleTiles.Where(
                        t => (t.localPlace.x == gate.localPlace.x && t.localPlace.y == botMostCoordinate)
                        && Utilities.ContainsRange(elligibleTiles, GetTiles(currPoint, t))).ToList();
                        break;
                    case Cardinal_Direction.East:
                        choices = elligibleTiles.Where(
                        t => (t.localPlace.x + currPoint.X == rightMostCoordinate + 1 && t.localPlace.y == gate.localPlace.y)
                        && Utilities.ContainsRange(elligibleTiles, GetTiles(currPoint, t))).ToList();
                        break;
                    case Cardinal_Direction.West:
                        choices = elligibleTiles.Where(
                        t => (t.localPlace.x == leftMostCoordinate && t.localPlace.y == gate.localPlace.y)
                        && Utilities.ContainsRange(elligibleTiles, GetTiles(currPoint, t))).ToList();
                        break;
                    default:
                        choices = elligibleTiles.Where(
                        t => t.localPlace.x + currPoint.X <= rightMostCoordinate
                        && t.localPlace.y + currPoint.Y <= topMostCoordinate
                        && Utilities.ContainsRange(elligibleTiles, GetTiles(currPoint, t))).ToList();
                        break;
                }
                Debug.Log(summary);
            } else {
                choices = elligibleTiles.Where(
                t => t.localPlace.x + currPoint.X < rightMostCoordinate
                && t.localPlace.y + currPoint.Y < topMostCoordinate
                && Utilities.ContainsRange(elligibleTiles, GetTiles(currPoint, t))).ToList();
            }

            if (choices.Count <= 0) {
                throw new System.Exception("No More Tiles for" + kvp.Key + " at " + area.name);
            }
            LocationGridTile chosenStartingTile = choices[Random.Range(0, choices.Count)];
            List<LocationGridTile> tiles = GetTiles(currPoint, chosenStartingTile);
            for (int j = 0; j < tiles.Count; j++) {
                LocationGridTile currTile = tiles[j];
                strcutureTilemap.SetTile(currTile.localPlace, structureTile);
                groundTilemap.SetTile(currTile.localPlace, floorTile);
                currTile.SetTileType(LocationGridTile.Tile_Type.Structure);
                currTile.SetStructure(kvp.Key);
                elligibleTiles.Remove(currTile);
                switch (kvp.Key.structureType) {
                    default:
                        for (int k = 0; k < currTile.neighbours.Values.Count; k++) {
                            elligibleTiles.Remove(currTile.neighbours.Values.ToList()[k]);
                        }
                        break;
                }
                //yield return new WaitForSeconds(0.1f);
            }
            //DrawStructureTileAssets(tiles);
            //kvp.Key.DetermineInsideTiles();
        }
    }
    private List<LocationGridTile> GetTiles(Point size, LocationGridTile startingTile) {
        List<LocationGridTile> tiles = new List<LocationGridTile>();
        for (int x = startingTile.localPlace.x; x < startingTile.localPlace.x + size.X; x++) {
            for (int y = startingTile.localPlace.y; y < startingTile.localPlace.y + size.Y; y++) {
                tiles.Add(map[x, y]);
            }
        }
        return tiles;
    }
    private void DrawStructureTileAssets(List<LocationGridTile> tiles) {
        //List<LocationGridTile> tilesWithWalls = new List<LocationGridTile>();
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile currTile = tiles[i];
            if (currTile.HasDifferentDwellingOrOutsideNeighbour()) {
                strcutureTilemap.SetTile(currTile.localPlace, structureTile);
            }
        }
    }
    private void AssignOuterAreas() {
        if (area.HasStructure(STRUCTURE_TYPE.WORK_AREA)) {
            for (int i = 0; i < insideTiles.Count; i++) {
                LocationGridTile currTile = insideTiles[i];
                if (currTile.structure == null) {
                    currTile.SetStructure(area.GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA));
                }
            }
        } else {
            Debug.LogWarning(area.name + " doesn't have a structure for work area");
        }
        if (area.HasStructure(STRUCTURE_TYPE.WILDERNESS)) {
            for (int i = 0; i < outsideTiles.Count; i++) {
                LocationGridTile currTile = outsideTiles[i];
                if (currTile.structure == null) {
                    currTile.SetStructure(area.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS));
                }
            }
        } else {
            Debug.LogWarning(area.name + " doesn't have a structure for wilderness");
        }
    }
    private Cardinal_Direction GetOppositeDirection(Cardinal_Direction dir) {
        switch (dir) {
            case Cardinal_Direction.North:
                return Cardinal_Direction.South;
            case Cardinal_Direction.South:
                return Cardinal_Direction.North;
            case Cardinal_Direction.East:
                return Cardinal_Direction.West;
            case Cardinal_Direction.West:
                return Cardinal_Direction.East;
            default:
                return Cardinal_Direction.North;
        }
    }
    #endregion

    #region Movement & Mouse Interaction
    private float xDiff = 30.5f;
    private float yDiff = 22f;
    private float originX = -8f;
    private float originY = -4.5f;
    public void OnScroll(Vector2 value) {
        //Debug.Log(value);
        Vector2 newMapPos;
        float newX = originX + ((xDiff * value.x) * -1f);
        float newY = originY + ((yDiff * value.y) * -1f);
        newMapPos = new Vector2(newX, newY);
        tilemapsParent.transform.localPosition = newMapPos;
        eventPopupParent.transform.position = scrollviewContent.transform.position;
        //if (value.x ) {

        //}
    }
    public void LateUpdate() {
        if (UIManager.Instance.IsMouseOnUI()) {
            return;
        }
        Vector3 mouseWorldPos = (worldUICanvas.worldCamera.ScreenToWorldPoint(Input.mousePosition));
        Vector3 localPos = grid.WorldToLocal(mouseWorldPos);
        Vector3Int coordinate = grid.LocalToCell(localPos);
        if (coordinate.x >= 0 && coordinate.x < width
            && coordinate.y >= 0 && coordinate.y < height) {
            LocationGridTile hoveredTile = map[coordinate.x, coordinate.y];
            if (hoveredTile.objHere != null) {
                ShowTileData(hoveredTile);
                if (Input.GetMouseButtonDown(0)) {
                    if (hoveredTile.objHere is Character) {
                        UIManager.Instance.ShowCharacterInfo(hoveredTile.objHere as Character);
                    }
                }
            } else {
                if (Input.GetMouseButtonDown(0)) {
                    Messenger.Broadcast(Signals.HIDE_MENUS);
                }
                UIManager.Instance.HideSmallInfo();
            }
        } else {
            UIManager.Instance.HideSmallInfo();
        }
    }
    //public void OnPointerEnter(BaseEventData baseData) {
    //    isHovering = true;
    //}
    //public void OnPointerExit(BaseEventData baseData) {
    //    isHovering = false;
    //    if (UIManager.Instance != null) {
    //        UIManager.Instance.HideSmallInfo();
    //    }
    //}
    #endregion

    #region Points of Interest
    public void PlaceObject(IPointOfInterest obj, LocationGridTile tile) {
        TileBase tileToUse = null;
        switch (obj.poiType) {
            case POINT_OF_INTEREST_TYPE.ITEM:
                tileToUse = itemTiles[(obj as SpecialToken).specialTokenType];
                break;
            case POINT_OF_INTEREST_TYPE.SUPLY_PILE:
                tileToUse = supplyIconTile;
                break;
            case POINT_OF_INTEREST_TYPE.CORPSE:
                OnPlaceCorpseOnTile(obj as Corpse, tile);
                //tileToUse = corpseIconTile;
                break;
            case POINT_OF_INTEREST_TYPE.FOOD:
                tileToUse = foodTiles[(obj as Food).foodType];
                break;
            case POINT_OF_INTEREST_TYPE.CHARACTER:
                OnPlaceCharacterOnTile(obj as Character, tile);
                //tileToUse = characterTile;
                break;
            default:
                tileToUse = characterTile;
                break;
        }
        objectsTilemap.SetTile(tile.localPlace, tileToUse);
        tile.SetObjectHere(obj);
    }
    public void RemoveObject(LocationGridTile tile) {
        tile.RemoveObjectHere();
        if (tile.prefabHere != null) {
            CharacterPortrait portrait = tile.prefabHere.GetComponent<CharacterPortrait>();
            if (portrait != null) {
                portrait.SetImageRaycastTargetState(true);
            }
            ObjectPoolManager.Instance.DestroyObject(tile.prefabHere);
        }
        objectsTilemap.SetTile(tile.localPlace, null);
    }
    private void OnPlaceCharacterOnTile(Character character, LocationGridTile tile) {
        Vector3 pos = new Vector3(tile.localPlace.x + 0.5f, tile.localPlace.y + 0.5f);
        GameObject portraitGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("CharacterPortrait", pos, Quaternion.identity, charactersParent);
        CharacterPortrait portrait = portraitGO.GetComponent<CharacterPortrait>();
        portrait.SetClickButton(PointerEventData.InputButton.Left);
        RectTransform rect = portraitGO.transform as RectTransform;
        rect.anchorMax = Vector2.zero;
        rect.anchorMin = Vector2.zero;
        portrait.gameObject.layer = LayerMask.NameToLayer("Area Maps");
        portrait.GeneratePortrait(character);
        portraitGO.transform.localScale = new Vector3(0.008f, 0.008f, 1f);
        portrait.SetImageRaycastTargetState(false);
        rect.anchoredPosition = pos;
        tile.SetPrefabHere(portraitGO);
    }
    private void OnPlaceCorpseOnTile(Corpse corpse, LocationGridTile tile) {
        Vector3 pos = new Vector3(tile.localPlace.x, tile.localPlace.y);
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool("CorpseObject", pos, Quaternion.identity, charactersParent);
        CorpseObject obj = go.GetComponent<CorpseObject>();
        obj.SetCorpse(corpse);
        RectTransform rect = go.transform as RectTransform;
        rect.anchorMax = Vector2.zero;
        rect.anchorMin = Vector2.zero;
        go.layer = LayerMask.NameToLayer("Area Maps");
        rect.anchoredPosition = pos;
        tile.SetPrefabHere(go);
    }
    #endregion

    #region Other
    public void Open() {
        this.gameObject.SetActive(true);
    }
    public void Close() {
        this.gameObject.SetActive(false);
        //if (UIManager.Instance.areaInfoUI.isShowing) {
        //    UIManager.Instance.areaInfoUI.ToggleMapMenu(false);
        //}
        isHovering = false;
    }
    private void ShowTileData(LocationGridTile tile) {
        string summary = tile.localPlace.ToString();
        summary += "\nTile Type: " + tile.tileType.ToString();
        summary += "\nTile State: " + tile.tileState.ToString();
        summary += "\nContent: " + tile.objHere?.ToString() ?? "None";
        if (tile.structure != null) {
            summary += "\nStructure: " + tile.structure.ToString();
        }
        UIManager.Instance.ShowSmallInfo(summary);
    }
    public List<LocationGridTile> GetTilesInRadius(LocationGridTile centerTile, int radius, bool includeCenterTile = false) {
        List<LocationGridTile> tiles = new List<LocationGridTile>();
        int mapSizeX = map.GetUpperBound(0);
        int mapSizeY = map.GetUpperBound(1);
        int x = centerTile.localPlace.x;
        int y = centerTile.localPlace.y;
        if (includeCenterTile) {
            tiles.Add(centerTile);
        }
        for (int dx = x - radius; dx <= x + radius; dx++) {
            for (int dy = y - radius; dy <= y + radius; dy++) {
                if(dx >= 0 && dx <= mapSizeX && dy >= 0 && dy <= mapSizeY) {
                    if(dx == x && dy == y) {
                        continue;
                    }
                    LocationGridTile result = map[dx, dy];
                    tiles.Add(result);
                }
            }
        }
        return tiles;
    }
    #endregion

    #region Travel Lines
    public void DrawLine(LocationGridTile startTile, LocationGridTile destination, Character character) {
        GameObject travelLine = ObjectPoolManager.Instance.InstantiateObjectFromPool
            (travelLinePrefab.name, Vector3.zero, Quaternion.identity, travelLineParent);
        travelLine.GetComponent<AreaMapTravelLine>().DrawLine(startTile, destination, character, this);
        
        Debug.Log(GameManager.Instance.TodayLogString() + "Drawing line at " + area.name + "'s map. From " + startTile.localPlace.ToString() + " to " + destination.localPlace.ToString());
    }
    public void DrawLineToExit(LocationGridTile startTile, Character character) {
        GameObject travelLine = ObjectPoolManager.Instance.InstantiateObjectFromPool
            (travelLinePrefab.name, Vector3.zero, Quaternion.identity, travelLineParent);
        LocationGridTile exitTile = area.structures[STRUCTURE_TYPE.EXIT][0].tiles[Random.Range(0, area.structures[STRUCTURE_TYPE.EXIT][0].tiles.Count)];
        travelLine.GetComponent<AreaMapTravelLine>().DrawLine(startTile, exitTile, character, this);
        Debug.Log(GameManager.Instance.TodayLogString() + "Drawing line at " + area.name + "'s map. From " + startTile.localPlace.ToString() + " to exit" + exitTile.localPlace.ToString());
    }
    [ContextMenu("Draw Line For Testing")]
    public void DrawLineForTesting() {
        LocationGridTile startTile = new LocationGridTile(0, 4, groundTilemap);
        LocationGridTile destinationTile = new LocationGridTile(20, 15, groundTilemap);

        GameObject travelLine = GameObject.Instantiate(travelLinePrefab, travelLineParent);
        travelLine.GetComponent<AreaMapTravelLine>().DrawLine(startTile, destinationTile, null, this);

        //(newLine.transform as RectTransform).anchoredPosition = new Vector2(32f * startTile.localPlace.x, 32f * startTile.localPlace.y);
        //float angle = Mathf.Atan2(destinationTile.worldLocation.y - startTile.worldLocation.y, destinationTile.worldLocation.x - startTile.worldLocation.x) * Mathf.Rad2Deg;
        //newLine.transform.eulerAngles = new Vector3(newLine.transform.rotation.x, newLine.transform.rotation.y, angle);

        //float distance = Vector3.Distance(startTile.worldLocation, destinationTile.worldLocation);
        //(newLine.transform as RectTransform).sizeDelta = new Vector2((distance + 1) * 32f, 15f);
    }
    [ContextMenu("Move Travel Line Content")]
    public void MoveTravelLineContent() {
        Debug.Log(grid.CellToWorld(new Vector3Int(0, 0, 0)).ToString());
        //(travelLineParent.transform as RectTransform).anchoredPosition = canvas.worldCamera.WorldToViewportPoint();
    }
    #endregion

    #region Events
    public void ShowEventPopupAt(LocationGridTile location, Log log) {
        if (location == null) {
            Debug.LogWarning(GameManager.Instance.TodayLogString() + "Passed location is null! Not showing event popup for log: " + Utilities.LogReplacer(log));
            return;
        }
        Vector3 pos = new Vector3(location.localPlace.x + 0.5f, location.localPlace.y + 0.5f);
        //Vector3 worldPos = groundTilemap.CellToWorld(location.localPlace);
        //Vector3 screenPos = worldUICanvas.worldCamera.WorldToScreenPoint(worldPos);
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(eventPopupPrefab.name, Vector3.zero, Quaternion.identity, eventPopupParent);
        go.transform.localScale = Vector3.one;
        (go.transform as RectTransform).anchoredPosition = pos;
        //(go.transform as RectTransform).OverlayPosition(worldPos, worldUICanvas.worldCamera);
        //go.transform.SetParent(eventPopupParent);


        EventPopup popup = go.GetComponent<EventPopup>();
        popup.Initialize(log, location, worldUICanvas);
    }
    [Header("Event Popup testing")]
    [SerializeField] private int xLocation;
    [SerializeField] private int yLocation;
    [ContextMenu("Create Event Popup For testing")]
    public void CreateEventPopupForTesting() {
        LocationGridTile startTile = map[xLocation, yLocation];
        ShowEventPopupAt(startTile, null);
    }
    #endregion

    [ContextMenu("Get Radius")]
    public void GetRadius() {
        List<LocationGridTile> tiles = GetTilesInRadius(map[x, y], radius);
        for (int i = 0; i < tiles.Count; i++) {
            Debug.Log(tiles[i].localPlace.x + "," + tiles[i].localPlace.y);
        }
    }
}
