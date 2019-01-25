using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reanimated : Trait {

    public Reanimated() {
        name = "Reanimated";
        description = "Brought back to life by some unholy magic.";
        type = TRAIT_TYPE.ENCHANTMENT;
        effect = TRAIT_EFFECT.POSITIVE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void OnRemoveTrait(Character sourceCharacter) {
        base.OnRemoveTrait(sourceCharacter);
        sourceCharacter.Death(); //when a character with a reanimated trait has the reanimated trait removed from him/her, he/she dies again
    }
    #endregion
}
