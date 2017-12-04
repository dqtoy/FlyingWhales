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
    public int numOfUniqueLandmarks;
    public List<InitialMapResource> resourceSetup;
	internal Dictionary<RESOURCE, int> resources = new Dictionary<RESOURCE, int>();

    [Space(10)]
	public List<GameObject> listHexes;
    public List<HexTile> outerGridList;
    public List<HexTile> hexTiles;
    public List<Region> allRegions;
	public HexTile[,] map;
    public HexTile[,] outerGrid;

	internal float mapWidth;
	internal float mapHeight;

	void Awake(){
		Instance = this;
//		ConvertInitialResourceSetupToDictionary ();
	}

    #region Grid Generation
    internal void GenerateGrid() {
        float newX = xOffset * (width / 2);
        float newY = yOffset * (height / 2);
        this.transform.localPosition = new Vector2(-newX, -newY);
        //CameraMove.Instance.minimapCamera.transform.position
        map = new HexTile[(int)width, (int)height];
        listHexes = new List<GameObject>();
        hexTiles = new List<HexTile>();
        int id = 1;
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
                currHex.id = id;
                currHex.tileName = hex.name;
                currHex.xCoordinate = x;
                currHex.yCoordinate = y;
                currHex.SetPathfindingTag(0);
                listHexes.Add(hex);
                map[x, y] = hex.GetComponent<HexTile>();
                id++;
            }
        }
        listHexes.ForEach(o => o.GetComponent<HexTile>().FindNeighbours(map));
        mapWidth = listHexes[listHexes.Count - 1].transform.position.x;
        mapHeight = listHexes[listHexes.Count - 1].transform.position.y;
    }
    internal void GenerateOuterGrid() {
        int newWidth = (int)width + (_borderThickness * 2);
        int newHeight = (int)height + (_borderThickness * 2);

        float newX = xOffset * (newWidth / 2);
        float newY = yOffset * (newHeight / 2);

        outerGridList = new List<HexTile>();
        outerGrid = new HexTile[newWidth, newHeight];

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
                currHex.tileName = hex.name;
                currHex.xCoordinate = x;
                currHex.yCoordinate = y;
                outerGrid[x, y] = currHex;
                outerGridList.Add(currHex);

                //int xToCopy = Mathf.Clamp(x, 0, (int)width - 1);
                //int yToCopy = Mathf.Clamp(y, 0, (int)height - 1);

                //int xToCopy = Mathf.Max(x - (_borderThickness * 2), 0);
                //int yToCopy = Mathf.Max(y - (_borderThickness * 2), 0);
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

                hex.name = x + "," + y + "(Border) Copied from " + hexToCopy.name;

                currHex.SetElevation(hexToCopy.elevationType);
                Biomes.Instance.SetBiomeForTile(hexToCopy.biomeType, currHex);
                Biomes.Instance.AddBiomeDetailToTile(currHex);
                hexToCopy.region.AddOuterGridTile(currHex);

                currHex.DisableColliders();
                //currHex.HideFogOfWarObjects();
            }
        }

        outerGridList.ForEach(o => o.GetComponent<HexTile>().FindNeighbours(outerGrid, true));
    }
    public void GenerateNeighboursWithSameTag() {
        for (int i = 0; i < listHexes.Count; i++) {
            HexTile currHex = listHexes[i].GetComponent<HexTile>();
            currHex.sameTagNeighbours = currHex.AllNeighbours.Where(x => x.tileTag == currHex.tileTag).ToList();
        }
    }
    #endregion

    #region Grid Utilities
    internal GameObject GetHex(string hexName) {
        for (int i = 0; i < listHexes.Count; i++) {
            if (hexName == listHexes[i].name) {
                return listHexes[i];
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
    internal void DivideOuterGridRegions() {
        for (int i = 0; i < outerGridList.Count; i++) {
            HexTile currTile = outerGridList[i];
            for (int j = 0; j < currTile.AllNeighbours.Count; j++) {
                HexTile currNeighbour = currTile.AllNeighbours[j];
                if (currNeighbour.region != currTile.region) {
                    //Load Border For currTile
                    HEXTILE_DIRECTION borderTileToActivate = currTile.GetNeighbourDirection(currNeighbour, true);
                    SpriteRenderer border = currTile.ActivateBorder(borderTileToActivate);
                    currTile.region.AddRegionBorderLineSprite(border);

                    if (GridMap.Instance.hexTiles.Contains(currNeighbour)) {
                        HEXTILE_DIRECTION neighbourBorderTileToActivate = HEXTILE_DIRECTION.NONE;
                        if (borderTileToActivate == HEXTILE_DIRECTION.NORTH_WEST) {
                            neighbourBorderTileToActivate = HEXTILE_DIRECTION.SOUTH_EAST;
                        } else if (borderTileToActivate == HEXTILE_DIRECTION.NORTH_EAST) {
                            neighbourBorderTileToActivate = HEXTILE_DIRECTION.SOUTH_WEST;
                        } else if (borderTileToActivate == HEXTILE_DIRECTION.EAST) {
                            neighbourBorderTileToActivate = HEXTILE_DIRECTION.WEST;
                        } else if (borderTileToActivate == HEXTILE_DIRECTION.SOUTH_EAST) {
                            neighbourBorderTileToActivate = HEXTILE_DIRECTION.NORTH_WEST;
                        } else if (borderTileToActivate == HEXTILE_DIRECTION.SOUTH_WEST) {
                            neighbourBorderTileToActivate = HEXTILE_DIRECTION.NORTH_EAST;
                        } else if (borderTileToActivate == HEXTILE_DIRECTION.WEST) {
                            neighbourBorderTileToActivate = HEXTILE_DIRECTION.EAST;
                        }
                        border = currNeighbour.ActivateBorder(neighbourBorderTileToActivate);
                        currNeighbour.region.AddRegionBorderLineSprite(border);
                    }

                    //if(currTile.xCoordinate == _borderThickness - 1 && currTile.yCoordinate > _borderThickness && currTile.yCoordinate < height) {
                    //    //tile is part of left border
                    //    if(borderTileToActivate == HEXTILE_DIRECTION.NORTH_WEST) {
                    //        currTile.region.AddRegionBorderLineSprite(currTile.ActivateBorder(HEXTILE_DIRECTION.NORTH_EAST));
                    //    }
                    //}
                }
            }
        }
    }
    public bool GenerateRegions(int numOfRegions, int refinementLevel) {
        List<HexTile> allHexTiles = new List<HexTile>(listHexes.Select(x => x.GetComponent<HexTile>()));
        List<HexTile> possibleCenterTiles = new List<HexTile>(allHexTiles.Where(x => (x.xCoordinate > 1 && x.xCoordinate < width - 1) && (x.yCoordinate < height - 2 && x.yCoordinate > 2)));
        HexTile[] initialCenters = new HexTile[numOfRegions];
        allRegions = new List<Region>();
        for (int i = 0; i < numOfRegions; i++) {
            if (possibleCenterTiles.Count <= 0) {
                //throw new System.Exception("All tiles have been used up!");
                return false;
            }
            HexTile chosenHexTile = possibleCenterTiles[Random.Range(0, possibleCenterTiles.Count)];
            possibleCenterTiles.Remove(chosenHexTile);
            allHexTiles.Remove(chosenHexTile);
            initialCenters[i] = chosenHexTile;
            Region newRegion = new Region(chosenHexTile);
            allRegions.Add(newRegion);
            //Color centerOfMassColor = newRegion.regionColor;
            //centerOfMassColor.a = 75.6f / 255f;
            //chosenHexTile.SetTileHighlightColor(centerOfMassColor);
            //chosenHexTile.ShowTileHighlight();
            foreach (HexTile hex in chosenHexTile.GetTilesInRange(5)) {
                possibleCenterTiles.Remove(hex);
            }
        }
        Debug.Log("Successfully got " + initialCenters.Length.ToString() + " center of masses!");

        for (int i = 0; i < refinementLevel; i++) {
            if (i != 0) {
                allHexTiles = new List<HexTile>(listHexes.Select(x => x.GetComponent<HexTile>()));
                for (int j = 0; j < allRegions.Count; j++) {
                    allRegions[j].ReComputeCenterOfMass();
                    allRegions[j].ResetTilesInRegion();
                    allHexTiles.Remove(allRegions[j].centerOfMass);
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
                    //currHexTile.SetTileHighlightColor(regionClosestTo.regionColor);
                    //currHexTile.ShowTileHighlight();
                } else {
                    throw new System.Exception("Could not find closest distance for tile " + currHexTile.name);
                }

            }
        }

        for (int i = 0; i < allRegions.Count; i++) {
            allRegions[i].RevalidateCenterOfMass();
            allRegions[i].CheckForAdjacency();
        }
        return true;
    }
    public void GenerateResourcesPerRegion() {
        for (int i = 0; i < allRegions.Count; i++) {
            Region currRegion = allRegions[i];
            if (currRegion.tileWithSpecialResource != null && currRegion.tileWithSpecialResource.specialResource != RESOURCE.NONE) {
                continue;
            }
			RESOURCE resourceForRegion = ResourcesManager.Instance.resourceCapDict.Keys.ElementAt(Random.Range(0, ResourcesManager.Instance.resourceCapDict.Count));
			ResourcesManager.Instance.ReduceResourceCount(resourceForRegion);
            currRegion.SetSpecialResource(resourceForRegion);
            currRegion.ComputeNaturalResourceLevel(); //Compute For Natural Resource Level of current region
        }
    }
    public void GenerateResourceTiles() {
        for (int i = 0; i < allRegions.Count; i++) {
            Region currRegion = allRegions[i];
            currRegion.AssignATileAsResourceTile();
        }
    }
//    private void ConvertInitialResourceSetupToDictionary() {
//        for (int i = 0; i < resourceSetup.Count; i++) {
//            InitialMapResource r = resourceSetup[i];
//            resources.Add(r.resourceType, r.resourceAmount);
//        }
//    }
//    internal bool ReduceResourceCount(RESOURCE resourceForRegion) {
//        this.resources[resourceForRegion] -= 1;
//        if (this.resources[resourceForRegion] <= 0) {
//            this.resources.Remove(resourceForRegion);
//            return true;
//        } else {
//            return false;
//        }
//    }
    /*
     * <summary>
     * Generate landmarks for all regions
     * other landmarks include: shrine, habitat
     * </summary>
     * */
    public void GenerateOtherLandmarksPerRegion() {
        List<RESOURCE> allSpecialResources = Utilities.GetEnumValues<RESOURCE>().ToList();
        allSpecialResources.Remove(RESOURCE.NONE);
        for (int i = 0; i < allRegions.Count; i++) {
            Region currRegion = allRegions[i];
            if (currRegion.landmarkCount < 2) {
                if (Random.Range(0, 2) == 0) {
                    currRegion.SetSummoningShrine();
                } else {
                    currRegion.SetHabitat();
                }
            }
        }
    }
    public void GenerateLandmarkExternalConnections() {
        for (int i = 0; i < allRegions.Count; i++) {
            Region currRegion = allRegions[i];
            //Debug.Log("Creating External connection for region at " + currRegion.centerOfMass.name);
            List<Landmark> landmarksInRegion = new List<Landmark>(currRegion.landmarks);
            List<Region> alreadyConnectedRegions = new List<Region>();

            for (int j = 0; j < landmarksInRegion.Count; j++) {
                Landmark currLandmark = landmarksInRegion[j];
                for (int k = 0; k < currLandmark.connections.Count; k++) {
                    object connectedObj = currLandmark.connections[k];
                    if (connectedObj is Region) {
                        Region connectedRegion = (Region)connectedObj;
                        if (connectedRegion.id != currRegion.id) {
                            if (!alreadyConnectedRegions.Contains(connectedRegion)) {
                                alreadyConnectedRegions.Add(connectedRegion);

                            }
                        }
                    } else if (connectedObj is HexTile) {
                        HexTile connectedTile = (HexTile)connectedObj;
                        if (connectedTile.region.id != currRegion.id) {
                            if (!alreadyConnectedRegions.Contains(connectedTile.region)) {
                                alreadyConnectedRegions.Add(connectedTile.region);
                            }
                        }
                    }
                }
            }

            if (alreadyConnectedRegions.Count == currRegion.adjacentRegions.Count) {
                continue;
            }

            for (int j = 0; j < landmarksInRegion.Count; j++) {
                Landmark currLandmark = landmarksInRegion[j];
                List<HexTile> otherLandmarksInRegion = new List<HexTile>(landmarksInRegion.Where(x => x != currLandmark).Select(x => x.location));
                //When connecting landmarks to nearby landmarks, exclude the other landmark on the same region if they are already connected
                //Check if currLandmark is already connected to other landmark in same region
                bool isAlreadyConnectedToLandmarkInThisRegion = false;
                for (int k = 0; k < otherLandmarksInRegion.Count; k++) {
                    HexTile otherLandmarkLocation = otherLandmarksInRegion[k];
                    if (PathGenerator.Instance.GetPath(currLandmark.location, otherLandmarkLocation, PATHFINDING_MODE.POINT_TO_POINT) != null) {
                        isAlreadyConnectedToLandmarkInThisRegion = true;
                        break;
                    }
                }

                List<HexTile> tilesToChooseFrom = null;
                if (isAlreadyConnectedToLandmarkInThisRegion) {
                    //connect to an adjacent region city/landmark instead
                    List<Region> adjacentRegions = new List<Region>(currRegion.adjacentRegions);
                    tilesToChooseFrom = new List<HexTile>(adjacentRegions.Where(x => !alreadyConnectedRegions.Contains(x)
                        && !x.connections.Contains(currLandmark.location)).Select(x => x.centerOfMass));
                    for (int k = 0; k < adjacentRegions.Count; k++) {
                        tilesToChooseFrom.AddRange(adjacentRegions[k].landmarks.Where(x => !x.connections.Contains(currLandmark.location)).Select(x => x.location));
                    }
                } else {
                    tilesToChooseFrom = new List<HexTile>(landmarksInRegion.Where(x => x != currLandmark).Select(x => x.location));
                }

                tilesToChooseFrom = tilesToChooseFrom.OrderBy(x => Vector2.Distance(currLandmark.location.transform.position, x.transform.position)).ToList();
                for (int k = 0; k < tilesToChooseFrom.Count; k++) {
                    HexTile currTile = tilesToChooseFrom[k];
                    if (alreadyConnectedRegions.Contains(currTile.region)) {
                        continue;
                    }
                    List<HexTile> path = PathGenerator.Instance.GetPath(currLandmark.location, currTile, PATHFINDING_MODE.LANDMARK_EXTERNAL_CONNECTION);
                    if (path != null) {
                        if (currTile.isHabitable) {
                            RoadManager.Instance.ConnectLandmarkToRegion(currLandmark.location, currTile.region);
                            alreadyConnectedRegions.Add(currTile.region);
                        } else if (currTile.hasLandmark) {
                            RoadManager.Instance.ConnectLandmarkToLandmark(currLandmark.location, currTile);
                            alreadyConnectedRegions.Add(currTile.region);
                        }
                        RoadManager.Instance.CreateRoad(path, ROAD_TYPE.MINOR);
                        break;
                    }
                }

            }
        }
    }
    public void UpdateAllRegionsDiscoveredKingdoms() {
        for (int i = 0; i < allRegions.Count; i++) {
            Region currRegion = allRegions[i];
            if (currRegion.occupant != null) {
                currRegion.CheckForDiscoveredKingdoms();
            }
        }
    }
    /*
     * Add a third landmark to 20 regions in the world map. These would be the Unique Landmarks. 
     * These should have 2 roads connecting to other landmarks or minor roads only.
     * */
     [ContextMenu("Generate Unique Landmarks")]
    public void GenerateUniqueLandmarks() {
        List<Region> allRegions = Utilities.Shuffle(this.allRegions);
        //List<Landmark> allLandmarksInWorld = new List<Landmark>();
        //allRegions.ForEach(x => allLandmarksInWorld.AddRange(x.landmarks));
        List<HexTile> allMinorRoadsInWorld = new List<HexTile>(RoadManager.Instance.minorRoadTiles);
        int uniqueLandmarksCount = 0;
        for (int i = 0; i < allRegions.Count; i++) {
            Region currRegion = allRegions[i];
            List<Landmark> elligibleLandmarks = new List<Landmark>();
            elligibleLandmarks.AddRange(currRegion.landmarks);
            currRegion.adjacentRegions.ForEach(x => elligibleLandmarks.AddRange(x.landmarks));

            //Get tiles that is not habitable, is not a landmark and is not a road.
            List<HexTile> elligibleTilesInRegion = new List<HexTile>(currRegion.tilesInRegion.Where(x => !x.isHabitable && !x.hasLandmark && !x.isRoad));
            for (int j = 0; j < elligibleTilesInRegion.Count; j++) {
                //Check if each elligible tile can have 2 roads
                HexTile currElligibleTile = elligibleTilesInRegion[j];
                if(currElligibleTile.GetTilesInRange(2).Where(x => x.hasLandmark).Count() > 0) {
                    //Check if currElligibleTile has any landmark tiles within 2 tiles
                    continue;
                }
                List<HexTile> possibleTilesToConnectTo = new List<HexTile>(elligibleLandmarks.Select(x => x.location));
                possibleTilesToConnectTo = new List<HexTile>(possibleTilesToConnectTo.Where(x => PathGenerator.Instance.GetPath(currElligibleTile, x, PATHFINDING_MODE.POINT_TO_POINT) == null 
                    && PathGenerator.Instance.GetPath(currElligibleTile, x, PATHFINDING_MODE.UNIQUE_LANDMARK_CREATION) != null)
                    .OrderBy(x => PathGenerator.Instance.GetPath(currElligibleTile, x, PATHFINDING_MODE.UNIQUE_LANDMARK_CREATION).Count));

                if(possibleTilesToConnectTo.Count <= 1) {
                    continue; //skip this tile
                }

                Dictionary<HexTile, List<HexTile>> roadsForElligibleTile = new Dictionary<HexTile, List<HexTile>>();
                allMinorRoadsInWorld = allMinorRoadsInWorld.OrderBy(x => Vector2.Distance(currElligibleTile.transform.position, x.transform.position)).ToList();

                for (int k = 0; k < possibleTilesToConnectTo.Count; k++) {
                    HexTile otherHexTile = possibleTilesToConnectTo[k];
                    List<HexTile> pathToOtherHexTile = PathGenerator.Instance.GetPath(currElligibleTile, otherHexTile, PATHFINDING_MODE.UNIQUE_LANDMARK_CREATION);
                    for (int l = 0; l < allMinorRoadsInWorld.Count; l++) {
                        HexTile currRoadTile = allMinorRoadsInWorld[l];
                        if(PathGenerator.Instance.GetPath(currRoadTile, otherHexTile, PATHFINDING_MODE.POINT_TO_POINT) != null) {
                            List<HexTile> pathToCurrRoadTile = PathGenerator.Instance.GetPath(currElligibleTile, currRoadTile, PATHFINDING_MODE.UNIQUE_LANDMARK_CREATION);
                            if (pathToCurrRoadTile != null && pathToCurrRoadTile.Count < pathToOtherHexTile.Count) {
                                pathToOtherHexTile = pathToCurrRoadTile;
                                break;
                            }
                        }
                    }
                    roadsForElligibleTile.Add(otherHexTile, pathToOtherHexTile);
                    if (roadsForElligibleTile.Count == 2) {
                        break;
                    }
                }

                if (roadsForElligibleTile.Count == 2) {
                    //currElligibleTile can have 2 roads
                    uniqueLandmarksCount++;
                    currElligibleTile.CreateUniqueLandmark();
                    foreach (KeyValuePair<HexTile, List<HexTile>> kvp in roadsForElligibleTile) {
                        RoadManager.Instance.CreateRoad(kvp.Value, ROAD_TYPE.MINOR);
                        if (kvp.Key.hasLandmark) {
                            RoadManager.Instance.ConnectLandmarkToLandmark(currElligibleTile, kvp.Key);
                        }
                    }
                    break;
                }
            }
            if(uniqueLandmarksCount >= numOfUniqueLandmarks) {
                break;
            }
        }
        Debug.Log("Created unique landmarks: " + uniqueLandmarksCount);
    }
    #endregion
}


