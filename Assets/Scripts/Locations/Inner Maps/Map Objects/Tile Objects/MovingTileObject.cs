using Inner_Maps;

public abstract class MovingTileObject : TileObject {
    public sealed override LocationGridTile gridTileLocation => TryGetGridTileLocation(out var tile) ? tile : base.gridTileLocation;
    protected abstract bool TryGetGridTileLocation(out LocationGridTile tile);
}
