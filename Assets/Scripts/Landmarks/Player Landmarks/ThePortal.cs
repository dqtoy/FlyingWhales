using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThePortal : BaseLandmark {
    public int currentMinionToSummonIndex { get; private set; }
    public int currentSummonTick { get; private set; }
    public int currentSummonDuration { get; private set; }

    public ThePortal(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) {
        currentMinionToSummonIndex = -1;
    }

    public ThePortal(HexTile location, SaveDataLandmark data) : base(location, data) {
        currentMinionToSummonIndex = -1;
    }

    public void LoadSummonMinion(SaveDataThePortal data) {
        if (data.currentMinionToSummonIndex != -1) {
            StartSummon(data.currentMinionToSummonIndex, data.currentSummonTick, data.currentSummonDuration);
        } else {
            currentMinionToSummonIndex = data.currentMinionToSummonIndex;
            currentSummonTick = data.currentSummonTick;
            currentSummonDuration = data.currentSummonDuration;
        }
    }
    public void StartSummon(int minionToSummonIndex, int currentSummonTick, int summonDuration = 0) {
        currentMinionToSummonIndex = minionToSummonIndex;
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
        TimerHubUI.Instance.AddItem("Summmoning " + PlayerManager.Instance.player.minionsToSummon[currentMinionToSummonIndex].className + " Minion", currentSummonDuration - currentSummonTick, null);
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
        UnsummonedMinionData minionData = PlayerManager.Instance.player.minionsToSummon[currentMinionToSummonIndex];
        Minion minion = PlayerManager.Instance.player.CreateNewMinion(minionData.className, RACE.DEMON, false);
        minion.character.SetName(minionData.minionName);
        minion.SetCombatAbility(minionData.combatAbility);
        minion.SetRandomResearchInterventionAbilities(minionData.interventionAbilitiesToResearch);

        PlayerManager.Instance.player.AddMinion(minion, true);
        PlayerManager.Instance.player.GenerateMinionsToSummon();
    }
    private void StopSummon() {
        currentSummonTick = 0;
        currentSummonDuration = 0;
        currentMinionToSummonIndex = -1;
        if (tileLocation.region.assignedMinion != null) {
            tileLocation.region.assignedMinion.SetAssignedRegion(null);
            tileLocation.region.SetAssignedMinion(null);
        }
        Messenger.Broadcast(Signals.AREA_INFO_UI_UPDATE_APPROPRIATE_CONTENT, tileLocation.region);
    }
}

public class SaveDataThePortal : SaveDataLandmark {
    public int currentMinionToSummonIndex;
    public int currentSummonTick;
    public int currentSummonDuration;

    public override void Save(BaseLandmark landmark) {
        base.Save(landmark);
        ThePortal portal = landmark as ThePortal;
        currentMinionToSummonIndex = portal.currentMinionToSummonIndex;
        currentSummonTick = portal.currentSummonTick;
        currentSummonDuration = portal.currentSummonDuration;
    }
    public override void LoadSpecificLandmarkData(BaseLandmark landmark) {
        base.LoadSpecificLandmarkData(landmark);
        ThePortal portal = landmark as ThePortal;
        portal.LoadSummonMinion(this);
    }
}
