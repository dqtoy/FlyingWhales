using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheEye : BaseLandmark {

    public TheEye(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) { }

    public TheEye(HexTile location, SaveDataLandmark data) : base(location, data) { }

    #region Override
    public override void OnMinionAssigned(Minion minion) {
        base.OnMinionAssigned(minion);
        //start listening for events
        Messenger.AddListener<Region, WorldEvent>(Signals.WORLD_EVENT_SPAWNED, OnEventSpawned);
    }
    public override void OnMinionUnassigned(Minion minion) {
        base.OnMinionUnassigned(minion);
        //stop listening to events
        Messenger.RemoveListener<Region, WorldEvent>(Signals.WORLD_EVENT_SPAWNED, OnEventSpawned);
    }
    #endregion

    private void OnEventSpawned(Region region, WorldEvent we){
        //check if the assigned minion can interfere with the type of world event.
    }
}
