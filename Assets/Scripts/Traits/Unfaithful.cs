using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unfaithful : Trait {

    public float affairChanceMultiplier { get; private set; }
    //public float makeLoveChanceMultiplier { get; private set; }

    public Unfaithful() {
        name = "Unfaithful";
        description = "Unfaithful characters are prone to having illicit love affairs.";
        type = TRAIT_TYPE.FLAW;
        effect = TRAIT_EFFECT.NEGATIVE;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
        //effects = new List<TraitEffect>();
    }

    #region Overrides
    protected override void OnChangeLevel() {
        base.OnChangeLevel();
        if (level == 1) {
            affairChanceMultiplier = 5f;
        } else if (level == 2) {
            affairChanceMultiplier = 10f;
        } else if (level == 3) {
            affairChanceMultiplier = 5f;
        }
    }
    #endregion

}
