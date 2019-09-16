using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlayMonster : WorldEvent {

    public SlayMonster() : base(WORLD_EVENT.SLAY_MONSTER) {
        duration = 3 * GameManager.ticksPerHour;
    }

    #region Overrides
    protected override void ExecuteAfterEffect(Region region) {
        //- after effect: removes a beast from the region
        Summon summon = region.worldObj as Summon;
        summon.Death();
        region.SetWorldObject(null);
        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
        AddDefaultFillersToLog(log, region);
        log.AddToFillers(null, summon.summonType.SummonName(), LOG_IDENTIFIER.STRING_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        base.ExecuteAfterEffect(region);
    }
    public override bool CanSpawnEventAt(Region region) {
        return region.worldObj is Summon && base.CanSpawnEventAt(region);
    }
    public override bool IsBasicEvent() {
        return true;
    }
    #endregion
}
