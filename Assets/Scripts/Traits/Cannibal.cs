using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannibal : Trait {
    public int level { get; private set; }

    public Cannibal(int level) {
        name = "Cannibal";
        description = "This character eats his own kind.";
        type = TRAIT_TYPE.SPECIAL;
        effect = TRAIT_EFFECT.NEGATIVE;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
        this.level = level;
        if (level == 1) {
            daysDuration = GameManager.Instance.GetTicksBasedOnHour(3);
        } else if (level == 2) {
            daysDuration = GameManager.Instance.GetTicksBasedOnHour(6);
        } else if (level == 3) {
            daysDuration = GameManager.Instance.GetTicksBasedOnHour(9);
        }
    }
}
