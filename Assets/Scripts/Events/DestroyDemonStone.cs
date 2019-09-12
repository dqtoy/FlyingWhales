using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyDemonStone : WorldEvent {

    public DestroyDemonStone() : base(WORLD_EVENT.DESTROY_DEMON_STONE) {
    }

    #region Overrides
    public override void ExecuteAfterEffect(Region region) {
        //- after effect: Demon Stone object will be removed
        region.SetWorldObject(null);
        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
        AddDefaultFillersToLog(log, region);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        base.ExecuteAfterEffect(region);
    }
    public override bool CanSpawnEventAt(Region region) {
        return region.HasAnyCharacterOfType(CHARACTER_ROLE.ADVENTURER) && region.worldObj is DemonStone;
    }
    public override Character GetCharacterThatCanSpawnEvent(Region region) {
        return region.GetAnyCharacterOfType(CHARACTER_ROLE.ADVENTURER);
    }
    public override bool IsBasicEvent() {
        return true;
    }
    #endregion
}

