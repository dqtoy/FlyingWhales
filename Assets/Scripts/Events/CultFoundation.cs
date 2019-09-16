using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CultFoundation : WorldEvent {

    public CultFoundation() : base(WORLD_EVENT.CULT_FOUNDATION) {
        isUnique = true;
    }

    #region Overrides
    protected override void ExecuteAfterEffect(Region region) {
        //- after effect: the resident will gain Cultist trait and Cult Founder and will start spreading the demon cult
        region.eventSpawnedBy.AddTrait("Cultist");
        StoryEventsManager.Instance.SetIsCultActive(true);

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
        //- requirement: dark mood resident + demon cult is inactive
        bool hasDarkMoodResident = false;
        for (int i = 0; i < region.charactersAtLocation.Count; i++) {
            Character currResident = region.charactersAtLocation[i];
            if (currResident.currentMoodType == CHARACTER_MOOD.DARK) {
                hasDarkMoodResident = true;
                break;
            }
        }
        return hasDarkMoodResident && !StoryEventsManager.Instance.isCultActive;
    }
    public override Character GetCharacterThatCanSpawnEvent(Region region) {
        for (int i = 0; i < region.charactersAtLocation.Count; i++) {
            Character currResident = region.charactersAtLocation[i];
            if (currResident.currentMoodType == CHARACTER_MOOD.DARK) {
                return currResident;
            }
        }
        return null;
    }
    #endregion
}
