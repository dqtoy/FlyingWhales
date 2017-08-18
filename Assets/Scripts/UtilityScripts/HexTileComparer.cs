using System.Collections.Generic;

class HexTileComparer : IEqualityComparer<HexTile> {

    public bool Equals(HexTile h1, HexTile h2) {
        return h1.id == h2.id;
    }

    public int GetHashCode(HexTile h) {
        return h.id;
    }
}