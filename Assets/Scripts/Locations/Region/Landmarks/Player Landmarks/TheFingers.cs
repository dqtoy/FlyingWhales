using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheFingers : BaseLandmark {
    public int currentTick { get; private set; }
    public int duration { get; private set; }
    public bool hasBeenActivated { get; private set; }

    public TheFingers(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) { }

    public TheFingers(HexTile location, SaveDataLandmark data) : base(location, data) { }

    public void Activate() {
        hasBeenActivated = true;
        currentTick = 0;
        duration = GameManager.Instance.GetTicksBasedOnHour(4);
        Messenger.AddListener(Signals.TICK_STARTED, PerTick);
    }
    private void Deactivate() {
        Messenger.RemoveListener(Signals.TICK_STARTED, PerTick);
        hasBeenActivated = false;
        currentTick = 0;
        Messenger.Broadcast(Signals.REGION_INFO_UI_UPDATE_APPROPRIATE_CONTENT, tileLocation.region);
    }

    private void PerTick() {
        currentTick++;
        if(currentTick >= duration) {
            Deactivate();
        }
    }

}
