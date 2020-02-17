using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class BlockWall : TileObject {
    
    public BlockWall() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.BLOCK_WALL);
        RemoveCommonAdvertisements();
        traitContainer.RemoveTrait(this, "Flammable");
    }
    public BlockWall(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
        RemoveCommonAdvertisements();
        traitContainer.RemoveTrait(this, "Flammable");
    }

    #region Overrides
    public override void OnPlacePOI() {
        base.OnPlacePOI();
       
        // InitializeGUS(Vector2.zero, Vector2.one);
    }
    protected override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true,
        bool destroyTileSlots = true) {
        removedFrom.parentMap.structureTilemap.SetTile(removedFrom.localPlace, null);
        removedFrom.SetTileType(LocationGridTile.Tile_Type.Empty);
        DestroyExistingGUS();
        base.OnRemoveTileObject(removedBy, removedFrom, removeTraits, destroyTileSlots);
    }
    protected override void OnPlaceTileObjectAtTile(LocationGridTile tile) {
        tile.parentMap.structureTilemap.SetTile(tile.localPlace, InnerMapManager.Instance.assetManager.caveWallTile);
        tile.SetTileType(LocationGridTile.Tile_Type.Wall);
        InitializeGUS(Vector2.zero, Vector2.one);
        base.OnPlaceTileObjectAtTile(tile);
    }
    #endregion
}
