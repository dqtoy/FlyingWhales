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
                Biomes.Instance.SetElevationSpriteForTile(currHex);
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
    /*
     * Divide the created grid into 
     * regions. Refer to https://gamedev.stackexchange.com/questions/57213/generate-equal-regions-in-a-hex-map 
     * for the logic used.
     * */
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

            }
        }

        for (int i = 0; i < allRegions.Count; i++) {
            Region currRegion = allRegions[i];
            currRegion.RevalidateCenterOfMass();
            currRegion.CheckForAdjacency();
            currRegion.ComputeNaturalResourceLevel();
        }
        return true;
    }
    public void UpdateAllRegionsDiscoveredKingdoms() {
        for (int i = 0; i < allRegions.Count; i++) {
            Region currRegion = allRegions[i];
            if (currRegion.occupant != null) {
                currRegion.CheckForDiscoveredKingdoms();
            }
        }
    }
    #endregion

    #region Landmark Generation
    /*
     Generate new landmarks (Resources, Lairs, Dungeons)
         */
    public void GenerateOtherLandmarks() {
        List<HexTile> elligibleTiles = new List<HexTile>(hexTiles.Where(x => x.elevationType != ELEVATION.WATER && !x.isHabitable && !x.isRoad)); //Get tiles that aren't water and are not habitable
        //Tiles that are within 2 tiles of a habitable tile, cannot be landmarks
        for (int i = 0; i < allRegions.Count; i++) {
            Region currRegion = allRegions[i];
            List<HexTile> tilesToRemove = currRegion.centerOfMass.GetTilesInRange(2);
            Utilities.ListRemoveRange(elligibleTiles, tilesToRemove);
        }
        Dictionary<LANDMARK_TYPE, int> createdLandmarksDict = new Dictionary<LANDMARK_TYPE, int>();
        int numOfLandmarksToCreate = Mathf.FloorToInt(3f * (float)allRegions.Count); //there will be 2.5 times (rounded down) as many landmarks as there are number of cities
        Debug.Log("Creating " + numOfLandmarksToCreate.ToString() + " landmarks..... ");
        int createdLandmarks = 0;

        for (int i = 0; i < numOfLandmarksToCreate; i++) {
            if(elligibleTiles.Count <= 0) {
                Debug.Log("Only created " + createdLandmarks.ToString() + " landmarks");
                return;
            }
            HexTile chosenTile = elligibleTiles[Random.Range(0, elligibleTiles.Count)];
            elligibleTiles.Remove(chosenTile);
            List<HexTile> tilesToRemove = chosenTile.GetTilesInRange(2);
            Utilities.ListRemoveRange(elligibleTiles, tilesToRemove);
            List<HexTile> createdRoad = chosenTile.CreateRandomLandmark();

            //Keep track of number of landmarks per type
            if(chosenTile.landmarkOnTile != null) {
                LANDMARK_TYPE createdLandmarkType = chosenTile.landmarkOnTile.specificLandmarkType;
                if (createdLandmarksDict.ContainsKey(createdLandmarkType)) {
                    createdLandmarksDict[createdLandmarkType]++;
                } else {
                    createdLandmarksDict.Add(createdLandmarkType, 1);
                }
            }
            if(createdRoad != null) {
                Utilities.ListRemoveRange(elligibleTiles, createdRoad);
            }
            createdLandmarks++;
        }
        Debug.Log("Created " + createdLandmarks.ToString() + " landmarks");

        foreach (KeyValuePair<LANDMARK_TYPE, int> kvp in createdLandmarksDict) {
            Debug.Log(kvp.Key.ToString() + " - " + kvp.Value.ToString());
        }
    }
    #endregion
}


