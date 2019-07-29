using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldConfigManager : MonoBehaviour {

    public static WorldConfigManager Instance;

    [System.NonSerialized]public WorldSaveData loadedData = null; //Used for saved worlds

    [Header("Map Settings")]
    public int gridSizeX;
    public int gridSizeY;
    public int regionCount;

    public int minColumnsBetweenSettlements;
    public int maxColumnsBetweenSettlements;

    public int tileColumnRows; //how many rows per tile column?

    public int tileCollectionWidth; //Number of tiles horizontally per collection
    public int tileCollectionHeight; //Number of tiles vertically per collection


    [Header("Settlements")]
    [Tooltip("Minimum number of settlements to generate")]
    public int minSettltementCount;
    [Tooltip("Maximum number of settlements to generate")]
    public int maxSettltementCount;
    [Tooltip("Minimum number of citizens to generate per settlement")]
    public int minCitizenCount;
    [Tooltip("Maximum number of citizens to generate per settlement")]
    public int maxCitizenCount;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(Instance.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }

    public void SetDataToUse(WorldSaveData data) {
        loadedData = data;
    }

    public RandomWorld GenerateRandomWorldData() {
        RandomWorld world = new RandomWorld();
        int settlementCount = Random.Range(minSettltementCount, maxSettltementCount);

        int width = 0;
        int height = tileCollectionHeight * tileColumnRows;

        List<TileColumn> columns = new List<TileColumn>();
        for (int i = 0; i < settlementCount; i++) {
            columns.Add(new TileColumn(tileColumnRows, true, width));
            width += tileCollectionWidth; //1 column for settlement
            if (i + 1 != settlementCount) { //do not add tiles after last settlement
                int columnsInBetween = Random.Range(minColumnsBetweenSettlements, maxColumnsBetweenSettlements);
                for (int j = 0; j < columnsInBetween; j++) {
                    columns.Add(new TileColumn(tileColumnRows, false, width));
                    width += tileCollectionWidth; //random number of columns after settlement
                }
            }
        }

        world.mapWidth = width;
        world.mapHeight = height;
        world.columns = columns;
        world.settlementCount = settlementCount;
        return world;
    }
}

public class RandomWorld {
    public int mapWidth;
    public int mapHeight;
    public int settlementCount;
    public List<TileColumn> columns;

    public TileColumn this[int index] {
        get {
            return GetColumnWithIndex(index);
        }
    }

    /// <summary>
    /// Get the column that has a tile with the xCoordinate of [index]
    /// </summary>
    /// <param name="index">The xCoordinate.</param>
    /// <returns>The column that has the tile.</returns>
    private TileColumn GetColumnWithIndex(int index) {
        for (int i = 0; i < columns.Count; i++) {
            TileColumn currColumn = columns[i];
            for (int j = 0; j < currColumn.rows.Length; j++) {
                TileRow collection = currColumn.rows[j];
                if (index >= collection.minX && index <= collection.maxX) {
                    return columns[i];
                }
            }
        }
        throw new System.Exception("There is no column that has tile with x coordinate " + index.ToString());
    }

    public void LogWorldData() {
        string summary = "Generated world summary: ";
        summary += "\nWidth: " + mapWidth.ToString();
        summary += "\nHeight: " + mapHeight.ToString();
        summary += "\nSettlements: " + settlementCount.ToString();
        summary += "\nColumns: " + columns.Count.ToString();
        Debug.Log(summary);
    }
}

public class TileColumn {
    public bool hasMajorLandmark; //Portal and Settlements (This is set during Random World Data Generation, and should NOT be used as a getter if a settlement has been placed)
    public TileRow[] rows;

    public TileColumn(int _rows, bool _hasMajorLandmark, int startingPoint) {
        hasMajorLandmark = _hasMajorLandmark;
        rows = new TileRow[_rows];

        int lastY = 0;

        for (int i = 0; i < rows.Length; i++) {
            TileRow collection = new TileRow();
            collection.minX = startingPoint;
            collection.maxX = startingPoint + WorldConfigManager.Instance.tileCollectionWidth - 1;
            collection.minY = lastY;
            collection.maxY = collection.minY + WorldConfigManager.Instance.tileCollectionHeight - 1;
            lastY = collection.maxY + 1;
        }

    }

    public List<int> GetValidRowsForLandmarks(TileColumn previousColumn) {
        List<int> validRowIndices = new List<int>();
        for (int i = 0; i < rows.Length; i++) {
            //check rows of previous column,
            //if the upper, lower or equal row has a landmark, then the current row of this column is valid
            TileRow upperRow = previousColumn.rows.ElementAtOrDefault(i + 1);
            TileRow lowerRow = previousColumn.rows.ElementAtOrDefault(i - 1);
            TileRow equalRow = previousColumn.rows[i];
            if (equalRow.hasLandmark) {
                validRowIndices.Add(i);
            } else if (upperRow != null && upperRow.hasLandmark) {
                validRowIndices.Add(i);
            } else if (lowerRow != null && lowerRow.hasLandmark) {
                validRowIndices.Add(i);
            }
        }
        return validRowIndices;
    }
    public List<TileRow> GetValidRowsInNextColumnForLandmarks(TileRow currentRow, TileColumn nextColumn) {
        List<TileRow> validRows = new List<TileRow>();
        int i = System.Array.IndexOf(rows, currentRow);
        TileRow upperRow = nextColumn.rows.ElementAtOrDefault(i + 1);
        TileRow lowerRow = nextColumn.rows.ElementAtOrDefault(i - 1);
        TileRow equalRow = nextColumn.rows[i];
        validRows.Add(equalRow);
        if (upperRow != null) {
            validRows.Add(upperRow);
        }
        if (lowerRow != null) {
            validRows.Add(lowerRow);
        }
        return validRows;
    }

    public TileRow GetRowWithMajorLandmark() {
        if (!hasMajorLandmark) {
            return null;
        }
        for (int i = 0; i < rows.Length; i++) {
            TileRow currRow = rows[i];
            if (currRow.hasLandmark) {
                return currRow;
            }
        }
        return null;
    }
}

public class TileRow {
    public int minX, maxX, minY, maxY;
    
    public BaseLandmark landmark;
    public bool hasLandmark {
        get { return landmark != null; }
    }

    public void SetLandmark(BaseLandmark landmark) {
        this.landmark = landmark;
    }

    public HexTile[] GetAllTiles(HexTile[,] map) {
        HexTile[] tiles = new HexTile[maxX * maxY];
        int count = 0;
        for (int x = minX; x <= maxX; x++) {
            for (int y = minY; y <= maxY; y++) {
                tiles[count] = map[x, y];
            }
        }
        return tiles;
    }
    /// <summary>
    /// Funtion to only get tiles that are at the middle of the collection.
    /// </summary>
    /// <param name="map">The reference map.</param>
    /// <returns>List of elligible tiles for landmarks.</returns>
    public List<HexTile> GetElligibleTilesForLandmark(HexTile[,] map) {
        List<HexTile> tiles = new List<HexTile>();

        int total = minX + maxX;
        List<int> midPoints = new List<int>();
        midPoints.Add(total / 2);
        if (total % 2 != 0) {
            midPoints.Add((total / 2) + 1);
        }

        for (int y = minY; y < maxY; y++) {
            for (int i = 0; i < midPoints.Count; i++) {
                int x = midPoints[i];
                tiles.Add(map[x, y]);
            }
        }
        return tiles;
    }
}