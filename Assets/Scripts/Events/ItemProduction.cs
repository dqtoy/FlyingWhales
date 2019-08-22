using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemProduction : WorldEvent {

    public ItemProduction() : base(WORLD_EVENT.ITEM_PRODUCTION) {
        duration = 2 * GameManager.ticksPerHour;
    }

    #region Overrides
    public override void ExecuteAfterEffect(BaseLandmark landmark) {
        base.ExecuteAfterEffect(landmark);
        //- after effect: adds a new item in settlement warehouse
        SpecialToken token = TokenManager.Instance.CreateRandomDroppableSpecialToken();
        landmark.eventSpawnedBy.homeArea.AddSpecialTokenToLocation(token, landmark.eventSpawnedBy.homeArea.GetRandomStructureOfType(STRUCTURE_TYPE.WAREHOUSE));

        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
        AddDefaultFillersToLog(log, landmark);
        log.AddToFillers(null, token.name, LOG_IDENTIFIER.ITEM_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
    }
    public override bool CanSpawnEventAt(BaseLandmark landmark) {
        return landmark.HasAnyCharacterOfType(CHARACTER_ROLE.CIVILIAN) && landmark.specificLandmarkType == LANDMARK_TYPE.WORKSHOP;
    }
    public override Character GetCharacterThatCanSpawnEvent(BaseLandmark landmark) {
        return landmark.GetAnyCharacterOfType(CHARACTER_ROLE.CIVILIAN);
    }
    #endregion
}
