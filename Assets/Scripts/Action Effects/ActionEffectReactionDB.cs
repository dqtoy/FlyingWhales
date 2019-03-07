using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ActionEffectReactionDB {

    public static Dictionary<GoapEffect, ActionEffectReaction> eventIntelReactions = new Dictionary<GoapEffect, ActionEffectReaction>() {
        { new GoapEffect(GOAP_EFFECT_CONDITION.ADD_TRAIT, "Abducted"), new AddTraitReaction() },
        { new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY), new RemoveFromPartyReaction() },
    };
}
