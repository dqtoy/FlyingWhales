using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nocturnal : Trait {

    public Nocturnal() {
        name = "Nocturnal";
        description = "Nocturnals are awake at night and asleep during the day.";
        type = TRAIT_TYPE.PERSONALITY;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
        //effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void OnAddTrait(ITraitable sourcePOI) {
        base.OnAddTrait(sourcePOI);
        if (sourcePOI is Character) {
            Character character = sourcePOI as Character;
            character.SetForcedFullnessRecoveryTimeInWords(TIME_IN_WORDS.EARLY_NIGHT);
            character.SetForcedTirednessRecoveryTimeInWords(TIME_IN_WORDS.MORNING);
            character.SetFullnessForcedTick();
            character.SetTirednessForcedTick();
        }
    }
    public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy) {
        base.OnRemoveTrait(sourcePOI, removedBy);
        if (sourcePOI is Character) {
            Character character = sourcePOI as Character;
            character.SetForcedFullnessRecoveryTimeInWords(TIME_IN_WORDS.LUNCH_TIME);
            character.SetForcedTirednessRecoveryTimeInWords(TIME_IN_WORDS.LATE_NIGHT);
            character.SetFullnessForcedTick();
            character.SetTirednessForcedTick();
        }
    }
    #endregion
}
