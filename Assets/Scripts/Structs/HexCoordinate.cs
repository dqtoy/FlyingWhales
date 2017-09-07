using UnityEngine;
using System.Collections;

public struct HexCoordinate {
    public int col;
    public int row;

    public HexCoordinate(int col, int row) {
        this.col = col;
        this.row = row;
    }
}
