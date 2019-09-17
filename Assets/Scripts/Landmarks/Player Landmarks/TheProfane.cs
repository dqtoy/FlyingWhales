public class TheProfane : BaseLandmark {
    public int currentDelayTick { get; private set; }
    public bool hasActivatedDelayDivineIntervention { get; private set; }

    public TheProfane(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) { }

    public TheProfane(HexTile location, SaveDataLandmark data) : base(location, data) { }

    public void LoadDelay (SaveDataTheProfane data) {
        if (data.hasActivated) {
            StartDelay(data.currentDelayTick);
        } else {
            currentDelayTick = data.currentDelayTick;
            hasActivatedDelayDivineIntervention = data.hasActivated;
        }
    }
    public void StartDelay(int currentDelayTick) {
        hasActivatedDelayDivineIntervention = true;
        this.currentDelayTick = currentDelayTick;
        TimerHubUI.Instance.AddItem("Delay Divine Intervention", LandmarkManager.DELAY_DIVINE_INTERVENTION_DURATION - currentDelayTick, null);
        Messenger.AddListener(Signals.TICK_STARTED, PerTickDelay);
    }
    private void PerTickDelay() {
        currentDelayTick++;
        if (currentDelayTick >= LandmarkManager.DELAY_DIVINE_INTERVENTION_DURATION) {
            Messenger.RemoveListener(Signals.TICK_STARTED, PerTickDelay);
            PlayerManager.Instance.player.AdjustDivineInterventionDuration(72);
            StopDelay();
        }
    }
    private void StopDelay() {
        currentDelayTick = 0;
        hasActivatedDelayDivineIntervention = false;
        tileLocation.region.assignedMinion.SetAssignedRegion(null);
        tileLocation.region.SetAssignedMinion(null);
        Messenger.Broadcast(Signals.AREA_INFO_UI_UPDATE_APPROPRIATE_CONTENT, tileLocation.region);
    }

    public override void DestroyLandmark() {
        base.DestroyLandmark();
        Messenger.RemoveListener(Signals.TICK_STARTED, PerTickDelay);
    }
}


public class SaveDataTheProfane : SaveDataLandmark {
    public int currentDelayTick;
    public bool hasActivated;

    public override void Save(BaseLandmark landmark) {
        base.Save(landmark);
        TheProfane profane = landmark as TheProfane;
        currentDelayTick = profane.currentDelayTick;
        hasActivated = profane.hasActivatedDelayDivineIntervention;
    }
    public override void LoadSpecificLandmarkData(BaseLandmark landmark) {
        base.LoadSpecificLandmarkData(landmark);
        TheProfane profane = landmark as TheProfane;
        profane.LoadDelay(this);
    }
}
