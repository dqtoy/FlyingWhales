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
    public int width;
	public int height;
    [SerializeField] private Transform _borderParent;
    [SerializeField] internal int _borderThickness;

    public float xOffset;
    public float yOffset;

    public int tileSize;

    public float elevationFrequency;
    public float moistureFrequency;

    [Space(10)]
	public List<GameObject> listHexes;
    public List<HexTile> outerGridList;
    public List<HexTile> hexTiles;
	public HexTile[,] map;

	internal float mapWidth;
	internal float mapHeight;

    public Region[] allRegions { get; private set; }


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
    public void SetupInitialData(int width, int height) {
        this.width = width;
        this.height = height;
    }
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
                hex.transform.SetParent(this.transform);
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
    internal void GenerateGrid(Save data) {
        this.width = data.width;
        this.height = data.height;
        float newX = xOffset * ((int) width / 2);
        float newY = yOffset * ((int) height / 2);
        this.transform.localPosition = new Vector2(-newX, -newY);
        map = new HexTile[(int) width, (int) height];
        hexTiles = new List<HexTile>();
        //int totalTiles = (int)width * (int)height;
        for (int i = 0; i < data.hextileSaves.Count; i++) {
            SaveDataHextile saveDataHextile = data.hextileSaves[i];
            int x = saveDataHextile.xCoordinate;
            int y = saveDataHextile.yCoordinate;
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
            saveDataHextile.Load(currHex);
            map[x, y] = currHex;
        }
        hexTiles.ForEach(o => o.FindNeighbours(map));
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
                hex.transform.SetParent(_borderParent.transform);
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
                //hexToCopy.region.AddOuterGridTile(currHex);


                currHex.DisableColliders();
                id++;
            }
        }
        outerGridList.ForEach(o => o.GetComponent<HexTile>().FindNeighboursForBorders());
    }
    internal void GenerateOuterGrid(Save data) {
        _borderThickness = data.borderThickness;
        int newWidth = (int) width + (_borderThickness * 2);
        int newHeight = (int) height + (_borderThickness * 2);

        float newX = xOffset * (int) (newWidth / 2);
        float newY = yOffset * (int) (newHeight / 2);

        outerGridList = new List<HexTile>();
        _borderParent.transform.localPosition = new Vector2(-newX, -newY);
        for (int i = 0; i < data.outerHextileSaves.Count; i++) {
            SaveDataHextile saveDataHextile = data.outerHextileSaves[i];
            int x = saveDataHextile.xCoordinate + _borderThickness;
            int y = saveDataHextile.yCoordinate + _borderThickness;

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
            saveDataHextile.Load(currHex);
            currHex.name = currHex.xCoordinate + "," + currHex.yCoordinate;

            outerGridList.Add(currHex);
            currHex.DisableColliders();
        }
        //Biomes.Instance.UpdateTileVisuals(outerGridList);
        outerGridList.ForEach(o => o.GetComponent<HexTile>().FindNeighboursForBorders());
    }
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

    #region Regions
    public void LoadRegions(Region[] regions) {
        allRegions = regions;
        for (int i = 0; i < allRegions.Length; i++) {
            allRegions[i].FinalizeData();
            allRegions[i].ShowTransparentBorder();
        }
    }
    public void DivideToRegions(List<HexTile> tiles, int regionCount, int mapSize) {
        List<HexTile> regionCoreTileChoices = new List<HexTile>(tiles);
        List<HexTile> remainingTiles = new List<HexTile>(tiles);
        allRegions = new Region[regionCount];
        for (int i = 0; i < regionCount; i++) {
            if (regionCoreTileChoices.Count == 0) {
                throw new System.Exception("No more core tiles for regions!");
            }
            HexTile chosenTile = regionCoreTileChoices[Random.Range(0, regionCoreTileChoices.Count)];
            Region newRegion = new Region(chosenTile);
            int range = 1;
            List<HexTile> tilesInRange = chosenTile.GetTilesInRange(range);
            regionCoreTileChoices.Remove(chosenTile);
            remainingTiles.Remove(chosenTile);
            for (int j = 0; j < tilesInRange.Count; j++) {
                regionCoreTileChoices.Remove(tilesInRange[j]);
            }
            allRegions[i] = newRegion;
        }
        //assign each remaining tile to a region, based on each tiles distance from a core tile.
        for (int i = 0; i < remainingTiles.Count; i++) {
            HexTile currTile = remainingTiles[i];
            Region nearestRegion = null;
            float nearestDistance = 99999f;
            for (int j = 0; j < allRegions.Length; j++) {
                Region currRegion = allRegions[j];
                float dist = Vector2.Distance(currTile.transform.position, currRegion.coreTile.transform.position);
                if (dist < nearestDistance) {
                    nearestRegion = currRegion;
                    nearestDistance = dist;
                }
            }
            nearestRegion.AddTile(currTile);
        }
        //string summary = "Generated regions: ";
        for (int i = 0; i < allRegions.Length; i++) {
            allRegions[i].FinalizeData();
            allRegions[i].ShowTransparentBorder();
            //summary += "\n" + i.ToString() + " - " + allRegions[i].tiles.Count.ToString();
        }
        //Debug.Log(summary);
    }
    #region Region
    public Region GetRegionByID(int id) {
        for (int i = 0; i < allRegions.Length; i++) {
            if(allRegions[i].id == id) {
                return allRegions[i];
            }
        }
        return null;
    }
    #endregion
    #endregion
}


