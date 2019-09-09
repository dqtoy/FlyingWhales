using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemProduction : WorldEvent {

    public ItemProduction() : base(WORLD_EVENT.ITEM_PRODUCTION) {
    }

    #region Overrides
    public override void ExecuteAfterEffect(Region region) {
        //- after effect: adds a new item in settlement warehouse
        SpecialToken token = TokenManager.Instance.CreateRandomDroppableSpecialToken();
        region.eventSpawnedBy.homeArea.AddSpecialTokenToLocation(token, region.eventSpawnedBy.homeArea.GetRandomStructureOfType(STRUCTURE_TYPE.WAREHOUSE));

        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
        AddDefaultFillersToLog(log, region);
        log.AddToFillers(null, token.name, LOG_IDENTIFIER.ITEM_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        base.ExecuteAfterEffect(region);
    }
    public override bool CanSpawnEventAt(Region region) {
        return region.HasAnyCharacterOfType(CHARACTER_ROLE.CIVILIAN) && region.mainLandmark.specificLandmarkType == LANDMARK_TYPE.WORKSHOP;
    }
    public override Character GetCharacterThatCanSpawnEvent(Region region) {
        return region.GetAnyCharacterOfType(CHARACTER_ROLE.CIVILIAN);
    }
    public override bool IsBasicEvent() {
        return true;
    }
    #endregion
}
