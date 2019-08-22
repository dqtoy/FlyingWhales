using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyArtifact : WorldEvent {

    public DestroyArtifact() : base(WORLD_EVENT.DESTROY_ARTIFACT) {
        duration = 3 * GameManager.ticksPerHour;
    }

    #region Overrides
    public override void ExecuteAfterEffect(BaseLandmark landmark) {
        base.ExecuteAfterEffect(landmark);
        //- after effect: removes an artifact from the region
        Artifact artifact = landmark.worldObj as Artifact;
        landmark.SetWorldObject(null);

        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
        AddDefaultFillersToLog(log, landmark);
        log.AddToFillers(null, artifact.name, LOG_IDENTIFIER.STRING_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
    }
    public override bool CanSpawnEventAt(BaseLandmark landmark) {
        return landmark.HasAnyCharacterOfType(CHARACTER_ROLE.ADVENTURER, CHARACTER_ROLE.SOLDIER) && landmark.worldObj is Artifact;
    }
    public override Character GetCharacterThatCanSpawnEvent(BaseLandmark landmark) {
        return landmark.GetAnyCharacterOfType(CHARACTER_ROLE.ADVENTURER, CHARACTER_ROLE.SOLDIER);
    }
    #endregion
}
