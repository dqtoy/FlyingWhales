using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonicPortal : BaseLandmark {
    public string currentMinionClassToSummon { get; private set; }
    public int currentSummonTick { get; private set; }
    public int currentSummonDuration { get; private set; }

    public DemonicPortal(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) {
        currentMinionClassToSummon = string.Empty;
    }

    public DemonicPortal(HexTile location, SaveDataLandmark data) : base(location, data) {
        currentMinionClassToSummon = string.Empty;
    }

    public void LoadSummonMinion(SaveDataDemonicPortal data) {
        if (data.currentMinionClassToSummon != string.Empty) {
            StartSummon(data.currentMinionClassToSummon, data.currentSummonTick, data.currentSummonDuration);
        } else {
            currentMinionClassToSummon = data.currentMinionClassToSummon;
            currentSummonTick = data.currentSummonTick;
            currentSummonDuration = data.currentSummonDuration;
        }
    }
    public void StartSummon(string minionClassToSummon, int currentSummonTick, int summonDuration = 0) {
        currentMinionClassToSummon = minionClassToSummon;
        this.currentSummonTick = currentSummonTick;
        if(summonDuration != 0) {
            currentSummonDuration = summonDuration;
        } else {
            currentSummonDuration = LandmarkManager.SUMMON_MINION_DURATION;
            if(tileLocation.region.assignedMinion != null) {
                int speedUpDuration = Mathf.CeilToInt(LandmarkManager.SUMMON_MINION_DURATION * 0.25f);
                currentSummonDuration -= speedUpDuration;
            }
        }
        TimerHubUI.Instance.AddItem("Summmoning " + currentMinionClassToSummon + " Minion", currentSummonDuration - currentSummonTick, null);
        Messenger.AddListener(Signals.TICK_STARTED, PerTickSummon);
    }
    private void PerTickSummon() {
        currentSummonTick++;
        if (currentSummonTick >= currentSummonDuration) {
            Messenger.RemoveListener(Signals.TICK_STARTED, PerTickSummon);
            SummonMinion();
            StopSummon();
        }
    }
    private void SummonMinion() {
        Minion minion = PlayerManager.Instance.player.CreateNewMinion(currentMinionClassToSummon, RACE.DEMON);
        PlayerManager.Instance.player.AddMinion(minion, true);
        PlayerManager.Instance.player.GenerateMinionClassesToSummon();
    }
    private void StopSummon() {
        currentSummonTick = 0;
        currentSummonDuration = 0;
        currentMinionClassToSummon = string.Empty;
        tileLocation.region.assignedMinion.SetAssignedRegion(null);
        tileLocation.region.SetAssignedMinion(null);
    }
}

public class SaveDataDemonicPortal : SaveDataLandmark {
    public string currentMinionClassToSummon;
    public int currentSummonTick;
    public int currentSummonDuration;

    public override void Save(BaseLandmark landmark) {
        base.Save(landmark);
        DemonicPortal portal = landmark as DemonicPortal;
        currentMinionClassToSummon = portal.currentMinionClassToSummon;
        currentSummonTick = portal.currentSummonTick;
        currentSummonDuration = portal.currentSummonDuration;
    }
    public override void LoadSpecificLandmarkData(BaseLandmark landmark) {
        base.LoadSpecificLandmarkData(landmark);
        DemonicPortal portal = landmark as DemonicPortal;
        portal.LoadSummonMinion(this);
    }
}
