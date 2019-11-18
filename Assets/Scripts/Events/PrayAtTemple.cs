using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
public class PrayAtTemple : WorldEvent {

    public PrayAtTemple() : base(WORLD_EVENT.PRAY_AT_TEMPLE) {
        eventEffects = new WORLD_EVENT_EFFECT[] { WORLD_EVENT_EFFECT.REMOVE_NEGATIVE_TRAIT };
        description = "This mission will cleanse the character of one of his Flaws.";
    }

    #region Overrides
    protected override void ExecuteAfterEffect(Region region, Character spawner) {
        //(remove a negative trait) 
        List<Trait> choices = spawner.traitContainer.GetAllTraitsOf(TRAIT_TYPE.FLAW);
        if (choices.Count > 0) {
            Trait chosen = choices[Random.Range(0, choices.Count)];
            spawner.traitContainer.RemoveTrait(spawner, chosen);
            Log log = new Log(GameManager.Instance.Today(), "WorldEvent", this.GetType().ToString(), "after_effect");
            AddDefaultFillersToLog(log, region);
            log.AddToFillers(null, chosen.name, LOG_IDENTIFIER.STRING_1);
            log.AddLogToInvolvedObjects();
            PlayerManager.Instance.player.ShowNotification(log);
        } else {
            Debug.LogWarning(GameManager.Instance.Today() + spawner.name + " no longer has any flaws!");
        }
        base.ExecuteAfterEffect(region,spawner);
    }
    public override bool CanSpawnEventAt(Region region, Character spawner) {
        return spawner.traitContainer.HasTraitOf(TRAIT_TYPE.FLAW); //the character must have at least 1 flaw
    }
    #endregion

}
