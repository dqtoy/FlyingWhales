using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyArtifact : WorldEvent {

    public DestroyArtifact() : base(WORLD_EVENT.DESTROY_ARTIFACT) {
    }

    #region Overrides
    public override void ExecuteAfterEffect(Region region) {
        //- after effect: removes an artifact from the region
        Artifact artifact = region.worldObj as Artifact;
        region.SetWorldObject(null);

        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
        AddDefaultFillersToLog(log, region);
        log.AddToFillers(null, artifact.name, LOG_IDENTIFIER.STRING_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        base.ExecuteAfterEffect(region);
    }
    public override bool CanSpawnEventAt(Region region) {
        return region.HasAnyCharacterOfType(CHARACTER_ROLE.ADVENTURER, CHARACTER_ROLE.SOLDIER) && region.worldObj is Artifact;
    }
    public override Character GetCharacterThatCanSpawnEvent(Region region) {
        return region.GetAnyCharacterOfType(CHARACTER_ROLE.ADVENTURER, CHARACTER_ROLE.SOLDIER);
    }
    public override bool IsBasicEvent() {
        return true;
    }
    #endregion
}
