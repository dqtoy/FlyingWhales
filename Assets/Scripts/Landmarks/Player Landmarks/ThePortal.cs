﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThePortal : BaseLandmark {
    public string currentMinionClassToSummon { get; private set; }
    public int currentSummonTick { get; private set; }
    public int currentSummonDuration { get; private set; }

    public ThePortal(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) {
        currentMinionClassToSummon = string.Empty;
    }

    public ThePortal(HexTile location, SaveDataLandmark data) : base(location, data) {
        currentMinionClassToSummon = string.Empty;
    }

    public void LoadSummonMinion(SaveDataThePortal data) {
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
        if (tileLocation.region.assignedMinion != null) {
            tileLocation.region.assignedMinion.SetAssignedRegion(null);
            tileLocation.region.SetAssignedMinion(null);
        }
        Messenger.Broadcast(Signals.AREA_INFO_UI_UPDATE_APPROPRIATE_CONTENT, tileLocation.region);
    }
}

public class SaveDataThePortal : SaveDataLandmark {
    public string currentMinionClassToSummon;
    public int currentSummonTick;
    public int currentSummonDuration;

    public override void Save(BaseLandmark landmark) {
        base.Save(landmark);
        ThePortal portal = landmark as ThePortal;
        currentMinionClassToSummon = portal.currentMinionClassToSummon;
        currentSummonTick = portal.currentSummonTick;
        currentSummonDuration = portal.currentSummonDuration;
    }
    public override void LoadSpecificLandmarkData(BaseLandmark landmark) {
        base.LoadSpecificLandmarkData(landmark);
        ThePortal portal = landmark as ThePortal;
        portal.LoadSummonMinion(this);
    }
}