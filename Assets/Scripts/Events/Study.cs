using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Study : WorldEvent {

    public Study() : base(WORLD_EVENT.STUDY) {
        eventEffects = new WORLD_EVENT_EFFECT[] { WORLD_EVENT_EFFECT.GAIN_POSITIVE_TRAIT, WORLD_EVENT_EFFECT.EXPLORE };
    }

    #region Overrides
    protected override void ExecuteAfterEffect(Region region, Character spawner) {
        //(gain a positive trait) 
        List<string> buffs = AttributeManager.Instance.GetAllBuffTraitsThatCharacterCanHave(region.eventSpawnedBy);
        Log log;
        if (buffs.Count > 0) {
            string chosenBuff = buffs[Random.Range(0, buffs.Count)];
            region.eventSpawnedBy.AddTrait(chosenBuff);
            log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect_gain_trait");
            log.AddToFillers(null, chosenBuff, LOG_IDENTIFIER.STRING_1);
        } else {
            log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect_no_trait");
        }
        AddDefaultFillersToLog(log, region);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        base.ExecuteAfterEffect(region, spawner);
    }
    #endregion

}
