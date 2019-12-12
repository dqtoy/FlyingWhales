using System.Collections.Generic;

public class RegionTileObject : TileObject {

    public RegionTileObject() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.REGION_TILE_OBJECT);
    }
    public RegionTileObject(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
    public void SetName(Region region) {
        this.name = region.name + " Region tile object";
    }

    #region Overrides
    public override string ToString() {
        return this.name + " " + this.id.ToString();
    }
    #endregion

    public void UpdateAdvertisements(Region region) {
        advertisedActions.Clear();
        if (region.coreTile.isCorrupted) {
            advertisedActions.Add(INTERACTION_TYPE.CLEANSE_REGION);
        } else {
            advertisedActions.Add(INTERACTION_TYPE.STUDY);
            if (region.owner == null) {
                advertisedActions.Add(INTERACTION_TYPE.CLAIM_REGION);
            } else {
                if (region.locationType.IsSettlementType()) {
                    advertisedActions.Add(INTERACTION_TYPE.INVADE_REGION);      
                }
            }
            
            //landmark types
            if (region.mainLandmark.specificLandmarkType != LANDMARK_TYPE.NONE) {
                advertisedActions.Add(INTERACTION_TYPE.ATTACK_REGION);
            }
            if (region.mainLandmark.specificLandmarkType == LANDMARK_TYPE.FARM) {
                advertisedActions.Add(INTERACTION_TYPE.HARVEST_FOOD_REGION);
            } else if (region.mainLandmark.specificLandmarkType == LANDMARK_TYPE.LUMBERYARD) {
                advertisedActions.Add(INTERACTION_TYPE.CHOP_WOOD_REGION);
            }else if (region.mainLandmark.specificLandmarkType == LANDMARK_TYPE.THE_PROFANE) {
                advertisedActions.Add(INTERACTION_TYPE.CORRUPT_CULTIST);
            }
            
            //features
            if (region.HasFeature(RegionFeatureDB.Game_Feature)) {
                advertisedActions.Add(INTERACTION_TYPE.FORAGE_FOOD_REGION);
            } 
            if (region.HasFeature(RegionFeatureDB.Hallowed_Ground_Feature)) {
                advertisedActions.Add(INTERACTION_TYPE.HOLY_INCANTATION);
                advertisedActions.Add(INTERACTION_TYPE.DEMONIC_INCANTATION);
            }
        }
    }
}
