using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GridMap : MonoBehaviour {
	public static GridMap Instance;

	public GameObject goHex;
    [Space(10)]
    [Header("Map Settings")]
    public float width;
	public float height;
    [SerializeField] private Transform _borderParent;
    [SerializeField] internal int _borderThickness;

    public float xOffset;
    public float yOffset;

    public int tileSize;

    public float elevationFrequency;
    public float moistureFrequency;

    [Space(10)]
    [Header("Region Settings")]
    public int numOfRegions;
    public int refinementLevel;

    [Space(10)]
	public List<GameObject> listHexes;
    public List<HexTile> outerGridList;
    public List<HexTile> hexTiles;
    public List<Region> allRegions;
	public HexTile[,] map;
    public HexTile[,] outerGrid;

	internal float mapWidth;
	internal float mapHeight;

    #region getters/setters
    public List<HexTile> allTiles {
        get {
            List<HexTile> tiles = new List<HexTile>(hexTiles);
            tiles.AddRange(outerGridList);
            return tiles;
        }
    }
    #endregion

    void Awake(){
		Instance = this;
//		ConvertInitialResourceSetupToDictionary ();
	}

    #region Grid Generation
    internal void GenerateGrid() {
        float newX = xOffset * ((int)width / 2);
        float newY = yOffset * ((int)height / 2);
        this.transform.localPosition = new Vector2(-newX, -newY);
        //CameraMove.Instance.minimapCamera.transform.position
        map = new HexTile[(int)width, (int)height];
        listHexes = new List<GameObject>();
        hexTiles = new List<HexTile>();
        int id = 0;
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                float xPosition = x * xOffset;

                float yPosition = y * yOffset;
                if (y % 2 == 1) {
                    xPosition += xOffset / 2;
                }

                GameObject hex = GameObject.Instantiate(goHex) as GameObject;
                hex.transform.parent = this.transform;
                hex.transform.localPosition = new Vector3(xPosition, yPosition, 0f);
                hex.transform.localScale = new Vector3(tileSize, tileSize, 0f);
                hex.name = x + "," + y;
                HexTile currHex = hex.GetComponent<HexTile>();
                hexTiles.Add(currHex);
                currHex.Initialize();
                currHex.data.id = id;
				currHex.data.tileName = RandomNameGenerator.Instance.GetTileName();
                currHex.data.xCoordinate = x;
                currHex.data.yCoordinate = y;
                listHexes.Add(hex);
                map[x, y] = hex.GetComponent<HexTile>();
                id++;
            }
        }
        listHexes.ForEach(o => o.GetComponent<HexTile>().FindNeighbours(map));
        mapWidth = listHexes[listHexes.Count - 1].transform.position.x;
        mapHeight = listHexes[listHexes.Count - 1].transform.position.y;
        //listHexes.ForEach(o => Debug.Log(o.name + " id: " + o.GetComponent<HexTile>().id));
    }
    internal void GenerateGrid(WorldSaveData data) {
        this.width = data.width;
        this.height = data.height;
        float newX = xOffset * ((int)width / 2);
        float newY = yOffset * ((int)height / 2);
        this.transform.localPosition = new Vector2(-newX, -newY);
        map = new HexTile[(int)width, (int)height];
        hexTiles = new List<HexTile>();
        int totalTiles = (int)width * (int)height;
        int id = 0;
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                float xPosition = x * xOffset;

                float yPosition = y * yOffset;
                if (y % 2 == 1) {
                    xPosition += xOffset / 2;
                }

                GameObject hex = GameObject.Instantiate(goHex) as GameObject;
                hex.transform.parent = this.transform;
                hex.transform.localPosition = new Vector3(xPosition, yPosition, 0f);
                hex.transform.localScale = new Vector3(tileSize, tileSize, 0f);
                hex.name = x + "," + y;
                HexTile currHex = hex.GetComponent<HexTile>();
                hexTiles.Add(currHex);
                currHex.Initialize();
                currHex.SetData(data.GetTileData(id));
                map[x, y] = currHex;
                id++;
            }
        }
        hexTiles.ForEach(o => o.FindNeighbours(map));
        //Biomes.Instance.UpdateTileVisuals(hexTiles);
        //Biomes.Instance.GenerateTileBiomeDetails(hexTiles);
        //Biomes.Instance.LoadPassableObjects(hexTiles);

        //LoadRegions(data);
        //LoadFactions(data);
        //LoadLandmarks(data);
        //OccupyRegions(data);

    }
    internal void GenerateOuterGrid() {
        int newWidth = (int)width + (_borderThickness * 2);
        int newHeight = (int)height + (_borderThickness * 2);

        float newX = xOffset * (int)(newWidth / 2);
        float newY = yOffset * (int)(newHeight / 2);

        int id = 0;

        outerGridList = new List<HexTile>();
        _borderParent.transform.localPosition = new Vector2(-newX, -newY);
        for (int x = 0; x < newWidth; x++) {
            for (int y = 0; y < newHeight; y++) {
                if ((x >= _borderThickness && x < newWidth - _borderThickness) && (y >= _borderThickness && y < newHeight - _borderThickness)) {
                    continue;
                }
                float xPosition = x * xOffset;

                float yPosition = y * yOffset;
                if (y % 2 == 1) {
                    xPosition += xOffset / 2;
                }

                GameObject hex = GameObject.Instantiate(goHex) as GameObject;
                hex.transform.parent = _borderParent.transform;
                hex.transform.localPosition = new Vector3(xPosition, yPosition, 0f);
                hex.transform.localScale = new Vector3(tileSize, tileSize, 0f);
                HexTile currHex = hex.GetComponent<HexTile>();
                currHex.Initialize();
                currHex.data.id = id;
                currHex.data.tileName = hex.name;
                currHex.data.xCoordinate = x - _borderThickness;
                currHex.data.yCoordinate = y - _borderThickness;
                
                outerGridList.Add(currHex);

                int xToCopy = x - _borderThickness;
                int yToCopy = y - _borderThickness;
                if (x < _borderThickness && y - _borderThickness >= 0 && y < height) { //if border thickness is 2 (0 and 1)
                    //left border
                    xToCopy = 0;
                    yToCopy = y - _borderThickness;
                } else if (x >= _borderThickness && x <= width && y < _borderThickness) {
                    //bottom border
                    xToCopy = x - _borderThickness;
                    yToCopy = 0;
                } else if (x > width && (y - _borderThickness >= 0 && y - _borderThickness <= height - 1)) {
                    //right border
                    xToCopy = (int)width - 1;
                    yToCopy = y - _borderThickness;
                } else if (x >= _borderThickness && x <= width && y - _borderThickness >= height) {
                    //top border
                    xToCopy = x - _borderThickness;
                    yToCopy = (int)height - 1;
                } else {
                    //corners
                    xToCopy = x;
                    yToCopy = y;
                    xToCopy = Mathf.Clamp(xToCopy, 0, (int)width - 1);
                    yToCopy = Mathf.Clamp(yToCopy, 0, (int)height - 1);
                }

                HexTile hexToCopy = map[xToCopy, yToCopy];

                currHex.name = currHex.xCoordinate + "," + currHex.yCoordinate + "(Border) Copied from " + hexToCopy.name;

                currHex.SetElevation(hexToCopy.elevationType);
                Biomes.Instance.SetBiomeForTile(hexToCopy.biomeType, currHex);
                //Biomes.Instance.AddBiomeDetailToTile(currHex);
                Biomes.Instance.UpdateTileVisuals(currHex);
                hexToCopy.region.AddOuterGridTile(currHex);


                currHex.DisableColliders();
                currHex.unpassableGO.GetComponent<PolygonCollider2D>().enabled = true;
                currHex.unpassableGO.SetActive(true);
                id++;
            }
        }
        outerGridList.ForEach(o => o.GetComponent<HexTile>().FindNeighboursForBorders());
    }
    internal void GenerateOuterGrid(WorldSaveData data) {
        if (data.outerGridTilesData == null) {
            GenerateOuterGrid(); //generate default outer grid
            return;
        }
        _borderThickness = data.borderThickness;
        int newWidth = (int)width + (_borderThickness * 2);
        int newHeight = (int)height + (_borderThickness * 2);

        float newX = xOffset * (int)(newWidth / 2);
        float newY = yOffset * (int)(newHeight / 2);

        outerGridList = new List<HexTile>();
        int id = 0;
        _borderParent.transform.localPosition = new Vector2(-newX, -newY);
        for (int x = 0; x < newWidth; x++) {
            for (int y = 0; y < newHeight; y++) {
                if ((x >= _borderThickness && x < newWidth - _borderThickness) && (y >= _borderThickness && y < newHeight - _borderThickness)) {
                    continue;
                }
                float xPosition = x * xOffset;

                float yPosition = y * yOffset;
                if (y % 2 == 1) {
                    xPosition += xOffset / 2;
                }

                GameObject hex = GameObject.Instantiate(goHex) as GameObject;
                hex.transform.SetParent(_borderParent.transform);
                hex.transform.localPosition = new Vector3(xPosition, yPosition, 0f);
                hex.transform.localScale = new Vector3(tileSize, tileSize, 0f);
                HexTile currHex = hex.GetComponent<HexTile>();
                currHex.Initialize();
                currHex.data = data.GetOuterTileData(id);
                currHex.data.xCoordinate = x - _borderThickness;
                currHex.data.yCoordinate = y - _borderThickness;

                outerGridList.Add(currHex);
                id++;
            }
        }
        Biomes.Instance.UpdateTileVisuals(outerGridList);
        outerGridList.ForEach(o => o.GetComponent<HexTile>().FindNeighboursForBorders());
    }
    //public void GenerateNeighboursWithSameTag() {
    //    for (int i = 0; i < listHexes.Count; i++) {
    //        HexTile currHex = listHexes[i].GetComponent<HexTile>();
    //        currHex.sameTagNeighbours = currHex.AllNeighbours.Where(x => x.tileTag == currHex.tileTag).ToList();
    //    }
    //}
    public HexTile GetTileFromCoordinates(int x, int y) {
        if ((x < 0 || x > width - 1) || (y < 0 || y > height - 1)) {
            //outer tile
            return GetBorderTile(x, y);
        } else {
            return map[x, y];
        }
    }
    private HexTile GetBorderTile(int x, int y) {
        for (int i = 0; i < outerGridList.Count; i++) {
            HexTile currTile = outerGridList[i];
            if (currTile.xCoordinate == x && currTile.yCoordinate == y) {
                return currTile;
            }
        }
        return null;
    }
    #endregion

    #region Grid Utilities
    internal HexTile GetHexTile(int id) {
        for (int i = 0; i < hexTiles.Count; i++) {
            if (hexTiles[i].id == id) {
                return hexTiles[i];
            }
        }
        return null;
    }
    internal HexTile GetHexTile(int xCoordinate, int yCoordinate) {
        for (int i = 0; i < hexTiles.Count; i++) {
            if (xCoordinate == hexTiles[i].xCoordinate && yCoordinate == hexTiles[i].yCoordinate) {
                return hexTiles[i];
            }
        }
        return null;
    }
    internal GameObject GetHex(string hexName, List<HexTile> tiles) {
        for (int i = 0; i < tiles.Count; i++) {
            if (hexName == tiles[i].name) {
                return tiles[i].gameObject;
            }
        }
        return null;
    }
    public List<HexTile> GetTilesInRange(HexTile center, int range) {
        List<HexTile> tilesInRange = new List<HexTile>();
        CubeCoordinate cube = OddRToCube(new HexCoordinate(center.xCoordinate, center.yCoordinate));
        Debug.Log("Center in cube coordinates: " + cube.x.ToString() + "," + cube.y.ToString() + "," + cube.z.ToString());
        for (int dx = -range; dx <= range; dx++) {
            for (int dy = Mathf.Max(-range, -dx - range); dy <= Mathf.Min(range, -dx + range); dy++) {
                int dz = -dx - dy;
                HexCoordinate hex = CubeToOddR(new CubeCoordinate(cube.x + dx, cube.y + dy, cube.z + dz));
                //Debug.Log("Hex neighbour: " + hex.col.ToString() + "," + hex.row.ToString());
                if (hex.col >= 0 && hex.row >= 0 && !(hex.col == center.xCoordinate && hex.row == center.yCoordinate)) {
                    tilesInRange.Add(map[hex.col, hex.row]);
                }
            }
        }
        return tilesInRange;
    }
    public HexCoordinate CubeToOddR(CubeCoordinate cube) {
        int modifier = 0;
        if (cube.z % 2 == 1) {
            modifier = 1;
        }
        int col = cube.x + (cube.z - (modifier)) / 2;
        int row = cube.z;
        return new HexCoordinate(col, row);
    }
    public CubeCoordinate OddRToCube(HexCoordinate hex) {
        int modifier = 0;
        if (hex.row % 2 == 1) {
            modifier = 1;
        }

        int x = hex.col - (hex.row - (modifier)) / 2;
        int z = hex.row;
        int y = -x - z;
        return new CubeCoordinate(x, y, z);
    }
    #endregion

    #region Region Generation
    //internal void DivideOuterGridRegions() {
    //    for (int i = 0; i < outerGridList.Count; i++) {
    //        HexTile currTile = outerGridList[i];
    //        for (int j = 0; j < currTile.AllNeighbours.Count; j++) {
    //            HexTile currNeighbour = currTile.AllNeighbours[j];
    //            if (currNeighbour.region != currTile.region) {
    //                //Load Border For currTile
    //                HEXTILE_DIRECTION borderTileToActivate = currTile.GetNeighbourDirection(currNeighbour);
    //                SpriteRenderer border = currTile.ActivateBorder(borderTileToActivate, Color.white);
    //                currTile.region.AddRegionBorderLineSprite(border);

    //                if (GridMap.Instance.hexTiles.Contains(currNeighbour)) {
    //                    HEXTILE_DIRECTION neighbourBorderTileToActivate = HEXTILE_DIRECTION.NONE;
    //                    if (borderTileToActivate == HEXTILE_DIRECTION.NORTH_WEST) {
    //                        neighbourBorderTileToActivate = HEXTILE_DIRECTION.SOUTH_EAST;
    //                    } else if (borderTileToActivate == HEXTILE_DIRECTION.NORTH_EAST) {
    //                        neighbourBorderTileToActivate = HEXTILE_DIRECTION.SOUTH_WEST;
    //                    } else if (borderTileToActivate == HEXTILE_DIRECTION.EAST) {
    //                        neighbourBorderTileToActivate = HEXTILE_DIRECTION.WEST;
    //                    } else if (borderTileToActivate == HEXTILE_DIRECTION.SOUTH_EAST) {
    //                        neighbourBorderTileToActivate = HEXTILE_DIRECTION.NORTH_WEST;
    //                    } else if (borderTileToActivate == HEXTILE_DIRECTION.SOUTH_WEST) {
    //                        neighbourBorderTileToActivate = HEXTILE_DIRECTION.NORTH_EAST;
    //                    } else if (borderTileToActivate == HEXTILE_DIRECTION.WEST) {
    //                        neighbourBorderTileToActivate = HEXTILE_DIRECTION.EAST;
    //                    }
    //                    border = currNeighbour.ActivateBorder(neighbourBorderTileToActivate, Color.white);
    //                    currNeighbour.region.AddRegionBorderLineSprite(border);
    //                }

    //                //if(currTile.xCoordinate == _borderThickness - 1 && currTile.yCoordinate > _borderThickness && currTile.yCoordinate < height) {
    //                //    //tile is part of left border
    //                //    if(borderTileToActivate == HEXTILE_DIRECTION.NORTH_WEST) {
    //                //        currTile.region.AddRegionBorderLineSprite(currTile.ActivateBorder(HEXTILE_DIRECTION.NORTH_EAST));
    //                //    }
    //                //}
    //            }
    //        }
    //    }
    //}
    /*
     * Divide the created grid into 
     * regions. Refer to https://gamedev.stackexchange.com/questions/57213/generate-equal-regions-in-a-hex-map 
     * for the logic used.
     * */
    public void GenerateRegions(int numOfRegions, int refinementLevel) {
        HexTile[] initialCenters = new HexTile[numOfRegions];
        List<HexTile> allHexTiles = new List<HexTile>(hexTiles);
        List<HexTile> chosenTiles = new List<HexTile>();
        
        while(chosenTiles.Count <= 0) {
            Utilities.Shuffle(allHexTiles);
            List<HexTile> possibleCenterTiles = new List<HexTile>(allHexTiles.Where(x => (x.xCoordinate > 1 && x.xCoordinate < width - 1) && (x.yCoordinate < height - 2 && x.yCoordinate > 2)));
            for (int i = 0; i < numOfRegions; i++) {
                if (possibleCenterTiles.Count <= 0) {
                    //throw new System.Exception("All tiles have been used up!");
                    chosenTiles.Clear();
                    break;
                } else {
                    HexTile chosenHexTile = possibleCenterTiles[Random.Range(0, possibleCenterTiles.Count)];
                    possibleCenterTiles.Remove(chosenHexTile);
                    chosenTiles.Add(chosenHexTile);
                    foreach (HexTile hex in chosenHexTile.GetTilesInRange(5)) {
                        possibleCenterTiles.Remove(hex);
                    }
                }
            }
        }
       

        allRegions = new List<Region>();
        for (int i = 0; i < chosenTiles.Count; i++) {
            HexTile chosenHexTile = chosenTiles[i];
            allHexTiles.Remove(chosenHexTile);
            initialCenters[i] = chosenHexTile;
            Region newRegion = new Region(chosenHexTile);
            allRegions.Add(newRegion);
            //Color centerOfMassColor = newRegion.regionColor;
            //centerOfMassColor.a = 75.6f / 255f;
            //chosenHexTile.SetTileHighlightColor(centerOfMassColor);
            //chosenHexTile.ShowTileHighlight();
        }
        Debug.Log("Successfully got " + initialCenters.Length.ToString() + " center of masses!");

        for (int i = 0; i < refinementLevel; i++) {
            if (i != 0) {
                allHexTiles = new List<HexTile>(hexTiles);
                Utilities.Shuffle(allHexTiles);
                for (int j = 0; j < allRegions.Count; j++) {
                    Region currRegion = allRegions[j];
                    currRegion.ReComputeCenterOfMass();
                    currRegion.ResetTilesInRegion();
                    currRegion.AddTile(currRegion.centerOfMass);
                    allHexTiles.Remove(currRegion.centerOfMass);
                }
            }
            for (int j = 0; j < allHexTiles.Count; j++) {
                HexTile currHexTile = allHexTiles[j];
                Region regionClosestTo = null;
                float closestDistance = 999999f;
                for (int k = 0; k < allRegions.Count; k++) {
                    Region currRegion = allRegions[k];
                    HexTile currCenter = currRegion.centerOfMass;
                    float distance = Vector2.Distance(currHexTile.transform.position, currCenter.transform.position);
                    if (distance < closestDistance) {
                        closestDistance = distance;
                        regionClosestTo = currRegion;
                    }
                }
                if (regionClosestTo != null) {
                    regionClosestTo.AddTile(currHexTile);
                } else {
                    throw new System.Exception("Could not find closest distance for tile " + currHexTile.name);
                }
                //allRegions = allRegions.OrderByDescending(x => x.tilesInRegion.Count).ToList();
            }
        }

        for (int i = 0; i < allRegions.Count; i++) {
            Region currRegion = allRegions[i];
            currRegion.RevalidateCenterOfMass();
            currRegion.UpdateAdjacency();
            currRegion.WallOffRegion();
            currRegion.DetermineRegionIslands();
        }
        for (int i = 0; i < allRegions.Count; i++) {
            Region currRegion = allRegions[i];
            currRegion.ConnectToOtherRegions();
            //currRegion.DetermineRegionIslands();
        }
        //return true;
    }
    public void LoadRegions(WorldSaveData data) {
        allRegions = new List<Region>();
        for (int i = 0; i < data.regionsData.Count; i++) {
            RegionSaveData currData = data.regionsData[i];
            HexTile centerTile = null;
            List<HexTile> regionTiles = GetRegionTiles(currData, ref centerTile);
            LoadRegion(regionTiles, centerTile, currData);
        }
        for (int i = 0; i < allRegions.Count; i++) {
            Region currRegion = allRegions[i];
            currRegion.UpdateAdjacency();
        }
    }
    private List<HexTile> GetRegionTiles(RegionSaveData regionData, ref HexTile centerTile) {
        List<int> tileIDs = new List<int>(regionData.tileData);
        List<HexTile> regionTiles = new List<HexTile>();
        for (int i = 0; i < hexTiles.Count; i++) {
            HexTile currTile = hexTiles[i];
            if (tileIDs.Contains(currTile.id)) {
                regionTiles.Add(currTile);
                tileIDs.Remove(currTile.id);
                if (currTile.id == regionData.centerTileID) {
                    centerTile = currTile;
                }
                if (tileIDs.Count == 0) {
                    break;
                }
            }
        }
        return regionTiles;
    }
    private Region LoadRegion(List<HexTile> tiles, HexTile centerTile, RegionSaveData data) {
        Region newRegion = new Region(centerTile, tiles, data);
        newRegion.AddTile(tiles);
        allRegions.Add(newRegion);

        //WorldCreatorUI.Instance.editRegionsMenu.OnRegionCreated(newRegion);
        return newRegion;
    }
    public void OccupyRegions(WorldSaveData data) {
        for (int i = 0; i < allRegions.Count; i++) {
            Region currRegion = allRegions[i];
            RegionSaveData regionData = data.GetRegionData(currRegion.id);
            if (regionData.owner != -1) {
                Faction owner = FactionManager.Instance.GetFactionBasedOnID(regionData.owner);
                owner.OwnRegion(currRegion);
                currRegion.SetOwner(owner);
                currRegion.ReColorBorderTiles(owner.factionColor);
                currRegion.SetMinimapColor(owner.factionColor, 69f / 255f);
            }
        }
    }
    //public void BottleneckBorders() {
    //    for (int i = 0; i < allRegions.Count; i++) {
    //        Region currRegion = allRegions[i];
    //        for (int j = 0; j < currRegion.outerTiles.Count; j++) {
    //            HexTile currBorderTile = currRegion.outerTiles[j];
    //            if (currBorderTile.isPassable && !currBorderTile.IsBottleneck()) {
    //                List<HexTile> borderNeighbours = currBorderTile.AllNeighbours.Where(x => x.IsBorderTileOfRegion()).ToList();
    //                if (!currBorderTile.AllNeighbours.Where(x => x.IsBottleneck()).Any()) {
    //                    //this tile has no border neighbours that are bottlenecks or dead ends, make this tile unpassable
    //                    currBorderTile.SetPassableState(false);
    //                    //make passable neighbours recompute their passable type
    //                    List<HexTile> passableNeighbours = currBorderTile.AllNeighbours.Where(x => x.isPassable).ToList();
    //                    passableNeighbours.ForEach(x => x.DeterminePassableType());
    //                }
    //            }
    //        }
    //        currRegion.LogPassableTiles();
    //        currRegion.DetermineRegionIslands();
    //    }
    //}
    //private void NormalizeRegions() {
    //    List<Region> orderedRegions = allRegions.OrderBy(x => x.tilesInRegion.Count).ToList();
    //    while (true) {
    //        //check if the regions have too much gaps between their number of tiles
    //        bool hasBigGaps = false;
    //        for (int i = 0; i < allRegions.Count; i++) {
    //            Region currRegion = allRegions[i];
    //            for (int j = 0; j < allRegions.Count; j++) {
    //                Region otherRegion = allRegions[j];
    //                if (currRegion.id != otherRegion.id) {
    //                    if (otherRegion.tilesInRegion.Count > currRegion.tilesInRegion.Count) {
    //                        if (otherRegion.tilesInRegion.Count - currRegion.tilesInRegion.Count >= currRegion.tilesInRegion.Count * 1.5f) {
    //                            hasBigGaps = true;
    //                            break;
    //                        }
    //                    }
    //                }
    //            }
    //        }

    //        if (hasBigGaps) {

    //        } else {
    //            break;
    //        }
    //    }
    //}
    //private void AddTilesToRegion(Region region, int tilesToAdd) {

    //}
    #endregion
}


