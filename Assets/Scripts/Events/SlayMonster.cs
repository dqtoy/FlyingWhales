using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlayMonster : WorldEvent {

    public SlayMonster() : base(WORLD_EVENT.SLAY_MONSTER) {
        duration = 3 * GameManager.ticksPerHour;
    }

    #region Overrides
    public override void ExecuteAfterEffect(BaseLandmark landmark) {
        base.ExecuteAfterEffect(landmark);
        //- after effect: removes a beast from the region
        Summon summon = landmark.worldObj as Summon;
        summon.Death();
        landmark.SetWorldObject(null);
        Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
        AddDefaultFillersToLog(log, landmark);
        log.AddToFillers(null, summon.summonType.SummonName(), LOG_IDENTIFIER.STRING_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
    }
    public override bool CanSpawnEventAt(BaseLandmark landmark) {
        return landmark.HasAnyCharacterOfType(CHARACTER_ROLE.ADVENTURER) && landmark.worldObj is Summon && base.CanSpawnEventAt(landmark);
    }
    public override Character GetCharacterThatCanSpawnEvent(BaseLandmark landmark) {
        return landmark.GetAnyCharacterOfType(CHARACTER_ROLE.ADVENTURER);
    }
    #endregion
}
