using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SabotageFarm : WorldEvent {

    public SabotageFarm() : base(WORLD_EVENT.SABOTAGE_FARM) {
        duration = 3 * GameManager.ticksPerHour;
    }

    #region Overrides
    public override void ExecuteAfterEffect(BaseLandmark landmark) {
        //- after effect: farm landmark will be destroyed
        landmark.ChangeLandmarkType(LANDMARK_TYPE.NONE);
        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
        AddDefaultFillersToLog(log, landmark);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);

        base.ExecuteAfterEffect(landmark);
    }
    public override void ExecuteAfterInvasionEffect(BaseLandmark landmark) {
        //- after invasion: resident becomes a minion
        landmark.eventSpawnedBy.RecruitAsMinion();
        base.ExecuteAfterInvasionEffect(landmark);
    }
    public override bool CanSpawnEventAt(BaseLandmark landmark) {
        //- requirement: Cultist resident + Farm landmark + demon cult is active
        bool hasCultistResident = false;
        for (int i = 0; i < landmark.charactersHere.Count; i++) {
            Character currResident = landmark.charactersHere[i];
            if (currResident.GetNormalTrait("Cultist") != null) {
                hasCultistResident = true;
                break;
            }
        }
        return hasCultistResident && landmark.specificLandmarkType == LANDMARK_TYPE.FARM && StoryEventsManager.Instance.isCultActive;
    }
    public override Character GetCharacterThatCanSpawnEvent(BaseLandmark landmark) {
        for (int i = 0; i < landmark.charactersHere.Count; i++) {
            Character currResident = landmark.charactersHere[i];
            if (currResident.GetNormalTrait("Cultist") != null) {
                return currResident;
            }
        }
        return null;
    }
    #endregion
}
