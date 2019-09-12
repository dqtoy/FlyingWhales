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
        ///Did not add listener, instead added checking if notification should be showed to player at WorldEvent script <see cref="WorldEvent.Spawn(Region, IWorldEventData, out string)"/>. Reason for this is if there are multiple Eye structures, they will each create a notification, which is not ideal.
        //Messenger.AddListener<Region, WorldEvent>(Signals.WORLD_EVENT_SPAWNED, OnEventSpawned);
    }
    public override void OnMinionUnassigned(Minion minion) {
        base.OnMinionUnassigned(minion);
        //stop listening to events
        //Messenger.RemoveListener<Region, WorldEvent>(Signals.WORLD_EVENT_SPAWNED, OnEventSpawned);
    }
    #endregion

    //private void OnEventSpawned(Region region, WorldEvent we){
        
    //}
}
