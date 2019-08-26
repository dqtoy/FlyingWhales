using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CultFoundation : WorldEvent {

    public CultFoundation() : base(WORLD_EVENT.CULT_FOUNDATION) {
        isUnique = true;
    }

    #region Overrides
    public override void ExecuteAfterEffect(BaseLandmark landmark) {
        //- after effect: the resident will gain Cultist trait and Cult Founder and will start spreading the demon cult
        landmark.eventSpawnedBy.AddTrait("Cultist");
        StoryEventsManager.Instance.SetIsCultActive(true);

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
        //- requirement: dark mood resident + demon cult is inactive
        bool hasDarkMoodResident = false;
        for (int i = 0; i < landmark.charactersHere.Count; i++) {
            Character currResident = landmark.charactersHere[i];
            if (currResident.currentMoodType == CHARACTER_MOOD.DARK) {
                hasDarkMoodResident = true;
                break;
            }
        }
        return hasDarkMoodResident && !StoryEventsManager.Instance.isCultActive;
    }
    public override Character GetCharacterThatCanSpawnEvent(BaseLandmark landmark) {
        for (int i = 0; i < landmark.charactersHere.Count; i++) {
            Character currResident = landmark.charactersHere[i];
            if (currResident.currentMoodType == CHARACTER_MOOD.DARK) {
                return currResident;
            }
        }
        return null;
    }
    #endregion
}
