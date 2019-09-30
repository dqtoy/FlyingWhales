﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheNeedles : BaseLandmark {

    public int currentCooldownTick { get; private set; }
    public int cooldownDuration { get; private set; }
    public bool isInCooldown {
        get { return currentCooldownTick < cooldownDuration; }
    }

    public TheNeedles(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) {
        cooldownDuration = GameManager.Instance.GetTicksBasedOnHour(4);
        currentCooldownTick = cooldownDuration;
    }

    public TheNeedles(HexTile location, SaveDataLandmark data) : base(location, data) {
        cooldownDuration = GameManager.Instance.GetTicksBasedOnHour(4);
    }

    public void Activate() {
        Minion minion = tileLocation.region.assignedMinion;
        int gainedMana = GetManaValue(minion);
        PlayerManager.Instance.player.AdjustMana(gainedMana);
        tileLocation.region.SetAssignedMinion(null);
        minion.SetAssignedRegion(null);
        minion.character.Death("ConvertedMana");
        StartCooldown();
        Messenger.Broadcast(Signals.AREA_INFO_UI_UPDATE_APPROPRIATE_CONTENT, tileLocation.region);
    }

    private void StartCooldown() {
        currentCooldownTick = 0;
        Messenger.AddListener(Signals.TICK_ENDED, PerTickCooldown);
    }
    private void PerTickCooldown() {
        currentCooldownTick++;
        if (currentCooldownTick == cooldownDuration) {
            //coodlown done
            StopCooldown();
        }
    }
    private void StopCooldown() {
        Messenger.RemoveListener(Signals.TICK_ENDED, PerTickCooldown);
        Messenger.Broadcast(Signals.AREA_INFO_UI_UPDATE_APPROPRIATE_CONTENT, tileLocation.region);
    }

    public int GetManaValue(Minion minion) {
       return minion.character.level * 100;
    }
}
