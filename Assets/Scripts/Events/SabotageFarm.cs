using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SabotageFarm : WorldEvent {

    public SabotageFarm() : base(WORLD_EVENT.SABOTAGE_FARM) {
    }

    #region Overrides
    public override void ExecuteAfterEffect(Region region) {
        //- after effect: farm landmark will be destroyed
        region.mainLandmark.ChangeLandmarkType(LANDMARK_TYPE.NONE);
        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
        AddDefaultFillersToLog(log, region);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);

        base.ExecuteAfterEffect(region);
    }
    public override void ExecuteAfterInvasionEffect(Region region) {
        //- after invasion: resident becomes a minion
        region.eventSpawnedBy.RecruitAsMinion();
        base.ExecuteAfterInvasionEffect(region);
    }
    public override bool CanSpawnEventAt(Region region) {
        //- requirement: Cultist resident + Farm landmark + demon cult is active
        bool hasCultistResident = false;
        for (int i = 0; i < region.charactersHere.Count; i++) {
            Character currResident = region.charactersHere[i];
            if (currResident.GetNormalTrait("Cultist") != null) {
                hasCultistResident = true;
                break;
            }
        }
        return hasCultistResident && region.mainLandmark.specificLandmarkType == LANDMARK_TYPE.FARM && StoryEventsManager.Instance.isCultActive;
    }
    public override Character GetCharacterThatCanSpawnEvent(Region region) {
        for (int i = 0; i < region.charactersHere.Count; i++) {
            Character currResident = region.charactersHere[i];
            if (currResident.GetNormalTrait("Cultist") != null) {
                return currResident;
            }
        }
        return null;
    }
    #endregion
}
