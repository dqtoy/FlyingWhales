using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClaimRegion : WorldEvent {

    public ClaimRegion() : base(WORLD_EVENT.CLAIM_REGION) {
        eventEffects = new WORLD_EVENT_EFFECT[] { WORLD_EVENT_EFFECT.CONQUER_REGION };
        description = "This mission will conquer a region.";
        duration = GameManager.Instance.GetTicksBasedOnHour(6);
    }

    #region Overrides
    protected override void ExecuteAfterEffect(Region region, Character spawner) {
        Log log = CreateNewEventLog("after_effect", region);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        LandmarkManager.Instance.OwnRegion(spawner.faction, spawner.faction.race, region);
        base.ExecuteAfterEffect(region, spawner);
    }
    public override bool CanSpawnEventAt(Region region, Character spawner) {
        //requirement: Actor is Royalty + Region Criteria from Job
        if (spawner.traitContainer.GetNormalTrait("Royalty") == null) {
            return false;
        }
        //- not owned by any faction
        if (region.owner != null) {
            return false;
        }
        //-adjacent to a region owned by the actor's settlement's ruling faction
        if (region.IsConnectedToRegionOwnedBy(spawner.faction) == false) {
            return false;
        }
        return true;
    }
    #endregion
}
