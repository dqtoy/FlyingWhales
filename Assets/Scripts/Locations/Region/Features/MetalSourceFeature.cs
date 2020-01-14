using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;

public class MetalSourceFeature : TileFeature {
    private const int MaxOres = 4;
    
    private HexTile owner;
    private int currentOreCount;
    private bool isGeneratingPerHour;
    
    public MetalSourceFeature() {
        name = "Metal Source";
        description = "Provides Metal";
        type = REGION_FEATURE_TYPE.PASSIVE;
        isRemovedOnInvade = true;
    }

    #region Overrides
    public override void GameStartActions(HexTile tile) {
        owner = tile;
        List<TileObject> ores = tile.GetTileObjectsInHexTile(TILE_OBJECT_TYPE.ORE);
        currentOreCount = ores.Count;
        if (ores.Count < MaxOres) {
            int missingOres = MaxOres - ores.Count;
            for (int i = 0; i <= missingOres; i++) {
                if (CreateNewOre() == false) {
                    break;
                }
            }
        }
        Messenger.AddListener<TileObject, LocationGridTile>(Signals.TILE_OBJECT_PLACED, OnTileObjectPlaced);
        Messenger.AddListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
    }
    #endregion
    
    
    private void OnTileObjectPlaced(TileObject tileObject, LocationGridTile tile) {
        if (tile.buildSpotOwner.hexTileOwner == owner && tileObject.tileObjectType == TILE_OBJECT_TYPE.ORE) {
            AdjustOreCount(1);
        }
    }
    private void OnTileObjectRemoved(TileObject tileObject, Character character, LocationGridTile tile) {
        if (tile.buildSpotOwner.hexTileOwner == owner && tileObject.tileObjectType == TILE_OBJECT_TYPE.ORE) {
            AdjustOreCount(-1);
        }
    }

    private void AdjustOreCount(int amount) {
        currentOreCount += amount;
        OnOreCountChanged();
    }


    private void OnOreCountChanged() {
        if (currentOreCount < MaxOres) {
            if (isGeneratingPerHour == false) {
                isGeneratingPerHour = true;
                Messenger.AddListener(Signals.HOUR_STARTED, TryGeneratePerHour);    
            }
        } else {
            isGeneratingPerHour = false;
            Messenger.RemoveListener(Signals.HOUR_STARTED, TryGeneratePerHour);
        }
    }

    private void TryGeneratePerHour() {
        if (Random.Range(0, 100) < 15) {
            CreateNewOre();
        }
    }

    private bool CreateNewOre() {
        List<LocationGridTile> choices = owner.locationGridTiles.Where(x => x.isOccupied == false 
                                                                            && x.structure.structureType.IsOpenSpace() 
                                                                            && x.GetNeighbourOfTypeCount(LocationGridTile.Ground_Type.Cave, true) == 1).ToList();
        if (choices.Count > 0) {
            LocationGridTile chosenTile = Utilities.GetRandomElement(choices);
            chosenTile.structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.ORE),
                chosenTile);
            return true;
        }
        return false;
    }
}
