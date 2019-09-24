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
        canBeTriggered = true;
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
    public override void TriggerFlaw(Character character) {
        base.TriggerFlaw(character);
        //Character paramour = character.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.PARAMOUR);
        //if (paramour == null) {
        //    //If no paramour yet, the character will create a Have Affair Job which will attempt to have an affair with a viable target.
        //} else {
        //    //If already has a paramour, the character will attempt to make love with one.
        //    GoapPlanJob cheatJob = new GoapPlanJob(JOB_TYPE.CHEAT, INTERACTION_TYPE.INVITE_TO_MAKE_LOVE, paramour);
        //    character.jobQueue.AddJobInQueue(cheatJob);
        //}

    }
    #endregion

}
