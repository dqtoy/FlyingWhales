using System.Collections.Generic;

public class TrainingDummy : TileObject{
    public TrainingDummy() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.TRAINING_DUMMY);
        RemoveCommonAdvertisements();
    }
    public TrainingDummy(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
}
