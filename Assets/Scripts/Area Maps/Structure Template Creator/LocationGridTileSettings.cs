using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct LocationGridTileSettings {
    public TileTemplateData groundTile;
    public TileTemplateData groundWallTile;
    public TileTemplateData detailTile;
    public TileTemplateData structureWallTile;
    public TileTemplateData objectTile;
    public bool hasBuildingSpot;
    public BuildingSpotData buildingSpot; //if this has value then it means that there is a building spot here

    public LocationGridTileSettings MergeWith(LocationGridTileSettings otherSetting) {
        LocationGridTileSettings setting = this;
        setting.groundTile = otherSetting.groundTile;
        setting.groundWallTile = otherSetting.groundWallTile;
        setting.detailTile = otherSetting.detailTile;
        setting.structureWallTile = otherSetting.structureWallTile;
        setting.objectTile = otherSetting.objectTile;
        if (setting.hasBuildingSpot == false) {
            setting.buildingSpot = otherSetting.buildingSpot;
        }
        return setting;
    }

    public void UpdatePositions(Vector3 newPos) {
        groundTile.tilePosition = newPos;
        groundWallTile.tilePosition = newPos;
        detailTile.tilePosition = newPos;
        structureWallTile.tilePosition = newPos;
        objectTile.tilePosition = newPos;
        if (hasBuildingSpot) {
            buildingSpot.location = new Vector3Int((int)newPos.x, (int)newPos.y, 0);
        }

    }
}
