using UnityEngine;
using System.Collections;

public class DungeonLandmark : BaseLandmark {

    public DungeonLandmark(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) {
        _canBeOccupied = false;
    }

    #region Encounterables
    protected override void InititalizeEncounterables() {
        base.InititalizeEncounterables();
        if(specificLandmarkType == LANDMARK_TYPE.ANCIENT_RUIN) {
            _encounterables.AddElement(new ItemChest(1, ITEM_TYPE.ARMOR, 35), 50);
            //TODO: Add Goblin party to encounterables
        }
    }
    #endregion
}
